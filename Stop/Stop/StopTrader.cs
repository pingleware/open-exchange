//////////////////////////////////////////////////////////////////////////////
//
// Attribution-NonCommercial-ShareAlike 3.0 Unported (CC BY-NC-SA 3.0)
// Disclaimer
// 
// This file is part of OpEx.
// 
// You are free:
// 	* to Share : to copy, distribute and transmit the work;
//	* to Remix : to adapt the work.
//		
// Under the following conditions:
// 	* Attribution : You must attribute OpEx to Marco De Luca
//     (marco.de.luca@algotradingconsulting.com);
// 	* Noncommercial : You may not use this work for commercial purposes;
// 	* Share Alike : If you alter, transform, or build upon this work,
//     you may distribute the resulting work only under the same or similar
//     license to this one. 
// 	
// With the understanding that: 
// 	* Waiver : Any of the above conditions can be waived if you get permission
//     from the copyright holder. 
// 	* Public Domain : Where the work or any of its elements is in the public
//     domain under applicable law, that status is in no way affected by the
//     license. 
// 	* Other Rights : In no way are any of the following rights affected by
//     the license: 
//		 . Your fair dealing or fair use rights, or other applicable copyright
//          exceptions and limitations; 
//		 . The author's moral rights; 
//		 . Rights other persons may have either in the work itself or in how
//          the work is used, such as publicity or privacy rights. 
//	* Notice : For any reuse or distribution, you must make clear to others
//     the license terms of this work. The best way to do this is with a link
//     to this web page. 
// 
// You should have received a copy of the Legal Code of the CC BY-NC-SA 3.0
// Licence along with OpEx.
// If not, see <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>.
//
//////////////////////////////////////////////////////////////////////////////


﻿using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OPEX.Common;
using OPEX.OM.Common;
using OPEX.MDS.Client;
using OPEX.MDS.Common;

namespace OPEX.Stop
{
    class StopTrader
    {
        private Logger _logger;
        private IncomingOrder _incomingOrder;
        private OutgoingOrder _outgoingOrder;
        private InstrumentWatcher _instrumentWatcher;
        private object _releaseLock;
        
        private StopParameters _parameters;        

        public StopTrader(IncomingOrder incomingOrder)
        {
            _incomingOrder = incomingOrder;
            _logger = new Logger(string.Format("StopOrder({0})", incomingOrder.OrderID));
            
            string incomingOrderParameters = incomingOrder.Parameters;
            _parameters = StopParameters.Parse(incomingOrderParameters);

            _releaseLock = new object();

            _logger.Trace(LogLevel.Debug, "StopPrice: {0}.", _parameters.StopPrice);
        }

        internal void Start()
        {
            _logger.Trace(LogLevel.Method, "Starting order.");

            CreateChildOrder();            

            _logger.Trace(LogLevel.Debug, "Subscribing to instrument {0}.", _incomingOrder.Instrument);
            _instrumentWatcher = MarketDataClient.Instance.CreateInstrumentWatcher(_incomingOrder.Instrument);
            _instrumentWatcher.MarketDataChanged += new EventHandler<MarketDataEventArgs>(MarketDataChanged);
            _instrumentWatcher.GetLastSnapshot(1000);

            _logger.Trace(LogLevel.Method, "Order started.");
        }

        private void CreateChildOrder()
        {
            _logger.Trace(LogLevel.Debug, "Preparing outgoing order..");
            _outgoingOrder = _incomingOrder.CreateRelatedOutgoingOrder();
            _outgoingOrder.StatusChanged += new OutgoingOrderEventHandler(OutgoingOrder_StatusChanged);
            _outgoingOrder.Destination = "OPEX";
            // Source is automatically set by IncomingOrder.CreateRelatedOutgoingOrder
            // Destination is temporarily set to OPEX manually, however in the future
            // we will implement a static table in the DB that says what the preferred destination is,
            // and orders will be automatically routed to the preferred destination unless elsehow specified
            // by the user by manually overwriting the field Destination.
            _logger.Trace(LogLevel.Debug, "Created OutgoingOrder {0}.", _outgoingOrder.ToString());
        }

        void MarketDataChanged(object sender, MarketDataEventArgs args)
        {            
            _logger.Trace(LogLevel.Debug, "Evaluating whether the trigger condition is met...");
            if (Hold(_instrumentWatcher.LastSnapshot))
            {
                _logger.Trace(LogLevel.Debug, "Trigger condition not met. Holding order.");
                return;
            }

            _logger.Trace(LogLevel.Info, "Trigger condition met. Releasing order...");            
            Release();
        }

        private void Release()
        {
            lock (_releaseLock)
            {
                try
                {
                    if (!_outgoingOrder.IsSent)
                    {
                        _outgoingOrder.Send();
                        Finish();
                    }
                }
                finally { }
            }
        }

        public bool Hold(AggregatedDepthSnapshot s)
        {
            bool res = true;
            OrderSide sideToCheck = (_incomingOrder.Side == OrderSide.Buy) ? OrderSide.Sell : OrderSide.Buy;
            _logger.Trace(LogLevel.Debug, "The order is a {0}. Checking the {1} side of the depth.", 
                _incomingOrder.Side.ToString(), sideToCheck.ToString());
            
            AggregatedDepthSide side = s[sideToCheck];
            if (side.Depth < 1)
            {
                _logger.Trace(LogLevel.Debug, "No levels in the depth side.");
                return true;
            }
            double bestPrice = side[0].Price;
            _logger.Trace(LogLevel.Debug, "Best price on side {0} : {1:F4}", sideToCheck.ToString(), bestPrice.ToString());

            if (_incomingOrder.Side == OrderSide.Buy)
            {
                res = (_parameters.StopPrice > bestPrice);
            }
            else
            {
                res = (_parameters.StopPrice < bestPrice);
            }

            return res;
        }

        void OutgoingOrder_StatusChanged(object sender, Order newOrder)
        {
            OutgoingOrder oldOrder = sender as OutgoingOrder;
            IncomingOrder incomingOrder = oldOrder.ParentOrder;

            switch (newOrder.Status)
            {
                case OrderStatus.Accepted:
                    //incomingOrder.Accept();
                    break;

                case OrderStatus.Rejected:
                    incomingOrder.Reject(newOrder.Message);
                    break;

                case OrderStatus.Filled:
                case OrderStatus.CompletelyFilled:
                case OrderStatus.Overfilled:
                    incomingOrder.Fill(newOrder.LastPriceFilled, newOrder.LastQuantityFilled);
                    break;

                case OrderStatus.CancelledByExchange:
                    incomingOrder.Cancel(newOrder.Message);
                    break;

                default:
                    break;
            }
        }

        private void Finish()
        {
            _instrumentWatcher.MarketDataChanged -= new EventHandler<MarketDataEventArgs>(MarketDataChanged);
            _instrumentWatcher.Dispose();
        }
    }
}
