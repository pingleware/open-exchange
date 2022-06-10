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
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using OPEX.Common;
using OPEX.MDS;
using OPEX.OM.Common;
using OPEX.Configuration.Client;

namespace OPEX.ME
{
    /// <summary>
    /// Represents a set of orderbooks that can take orders and react to them.
    /// </summary>
    public class BusinessDomain
    {
        private readonly Logger _logger;
        private readonly string _domainName;
        private readonly Hashtable _orderBooks;
        private readonly Hashtable _orderProcessors;
        private readonly MarketDataService MDService;
        private readonly string[] _instrumentNames;

        private bool _running;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.ME.BusinessDomain.
        /// </summary>
        /// <param name="domainName">The symbolic name of the domain</param>
        /// <param name="instrumentNames">The name of the instruments in the domain</param>
        public BusinessDomain(string domainName, string[] instrumentNames)
        {
            _logger = new Logger(string.Format("BusinessDomain({0})", domainName));
            _domainName = domainName;
            _orderProcessors = Hashtable.Synchronized(new Hashtable());
            _orderBooks = Hashtable.Synchronized(new Hashtable());
            _instrumentNames = instrumentNames;

            _logger.Trace(LogLevel.Debug, "Creating new domain.");

            if (instrumentNames != null)
            {
                _logger.Trace(LogLevel.Debug, "Adding {0} instruments to the domain.", instrumentNames.Length);

                foreach (string instrumentName in instrumentNames)
                {
                    OrderBook ob = new OrderBook(instrumentName);
                    _orderBooks[instrumentName] = ob;
                }

                _logger.Trace(LogLevel.Debug, "Added {0} instruments to the domain.", instrumentNames.Length);
            }

            _logger.Trace(LogLevel.Debug, "Created new domain.");

            MDService = MarketDataService.Get(ConfigurationClient.Instance.GetConfigSetting("MDSDataSource", "OPEX"));

            _running = false;
        }

        /// <summary>
        /// Starts a BusinessDomain.
        /// </summary>
        public void Start()
        {           
            if (_running)
            {
                _logger.TraceAndThrow("Can't start a BusinessDomain if it's already running!");
            }

            _logger.Trace(LogLevel.Debug, "Starting domain.");           

            foreach (OrderBook ob in _orderBooks.Values)
            {
                OrderProcessor op = new OrderProcessor(ob);
                _orderProcessors[ob.InstrumentName] = op;

                op.Start();
            }

            _running = true;

            _logger.Trace(LogLevel.Debug, "Domain started.");
        }

        /// <summary>
        /// Stops a BusinessDomain.
        /// </summary>
        public void Stop()
        {
            if (!_running)
            {
                _logger.TraceAndThrow("Can't stop a BusinessDomain if it's not running!");
            }

            _logger.Trace(LogLevel.Debug, "Stopping domain.");

            foreach (OrderProcessor op in _orderProcessors.Values)
            {
                op.Stop();
            }

            PullOrders("Exchange down.");

            _logger.Trace(LogLevel.Debug, "Domain stopped.");
        }        

        /// <summary>
        /// Processes a new order request.
        /// </summary>
        /// <param name="order">The new order.</param>
        public void ReceiveNewOrder(IncomingOrder order)
        {
            ProcessNewRequest(OrderRequestType.NewOrder, order, null);
        }

        /// <summary>
        /// Processes an amendment request.
        /// </summary>
        /// <param name="oldOrder">The order to amend.</param>
        /// <param name="newOrder">The amended version of the order.</param>
        public void ReceiveAmendmentRequest(IncomingOrder oldOrder, Order newOrder)
        {
            ProcessNewRequest(OrderRequestType.Amendment, oldOrder, newOrder);
        }

        /// <summary>
        /// Processes a cancellation request.
        /// </summary>
        /// <param name="orderToCancel">The order to cancel.</param>
        public void ReceiveCancellationRequest(IncomingOrder orderToCancel)
        {
            ProcessNewRequest(OrderRequestType.Cancellation, orderToCancel, null);
        }

        /// <summary>
        /// Pulls all the existing orders from the BusinessDomain.
        /// </summary>
        /// <param name="reason">A description of the reason why orders were pulled.</param>
        public void PullOrders(string reason)
        {
            _logger.Trace(LogLevel.Warning, "PullOrders. reason: {0}", reason);
            lock(OrderFactory.IncomingOrders.SyncRoot)
            {
                try
                {
                    foreach (IncomingOrder incomingOrder in OrderFactory.IncomingOrders.Values)
                    {
                        if (OrderStateMachine.IsActiveStatus(incomingOrder.Status))
                        {
                            ProcessNewRequest(OrderRequestType.CancelByExchange, incomingOrder, null, reason);
                        }
                    }
                    Thread.Sleep(1000);
                    BroadcastAllDepths();
                    
                }
                finally { }                
            }
        }

        private void BroadcastAllDepths()
        {
            if (_instrumentNames == null)
            {
                return;
            }

            foreach (string instrument in _instrumentNames)
            {
                _logger.Trace(LogLevel.Warning, "BroadcastAllDepths. Broadcasting snapshot for instrument {0}", instrument);
                MDService.BroadcastNewSnapshotUpdate(instrument);
            }
        }

        private void ProcessNewRequest(OrderRequestType type, IncomingOrder incomingOrder, Order order)
        {
            ProcessNewRequest(type, incomingOrder, order, null);
        }

        private void ProcessNewRequest(OrderRequestType type, IncomingOrder incomingOrder, Order order, string message)
        {
            if (!_running)
            {
                _logger.TraceAndThrow("Can't process a request while not running!");
            }

            if (incomingOrder == null)
            {
                _logger.TraceAndThrow("Can't process a null order");
            }

            string instrument = incomingOrder.Instrument;
            if (instrument == null)
            {
                _logger.TraceAndThrow("Null instrument in order");
            }

            _logger.Trace(LogLevel.Debug, "New request received. Order: {0}", incomingOrder.ToString());

            OrderProcessor op = null;
            if (_orderProcessors.ContainsKey(instrument))
            {
                _logger.Trace(LogLevel.Debug, "Found an order processor for the instrument '{0}'.", instrument);

                op = _orderProcessors[instrument] as OrderProcessor;
            }
            else
            {
                _logger.Trace(LogLevel.Warning, "Could NOT find an order processor for the instrument '{0}'. This shouldn't happen. Anyways, creating a new one.", instrument);

                OrderBook ob = new OrderBook(instrument);
                op = new OrderProcessor(ob);
                _orderProcessors[instrument] = op;

                op.Start();
            }

            op.Process(new OrderRequest(type, incomingOrder, order, message));
        }       
    }
}
