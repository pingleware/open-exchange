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
using System.ComponentModel;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using OPEX.Common;
using OPEX.MDS;
using OPEX.MDS.Common;
using OPEX.OM.Common;
using OPEX.TDS.Common;
using OPEX.TDS.Server;
using OPEX.Configuration.Client;
using OPEX.ShoutService;

namespace OPEX.ME
{
    /// <summary>
    /// Represents the OrderBook of a security.
    /// </summary>
    public class OrderBook
    {
        private static readonly string DataSource;
        private static readonly bool NYSESpreadImprovement;
        private static readonly IOrderProcessor Validator;

        private readonly Logger _logger;
        private readonly MarketDataService MDService;
        private readonly TradeDataServer TDServer;
        private readonly ShoutServer ShoutService;
        private readonly string _instrumentName;
        private readonly OrderBookSide _buySide;
        private readonly OrderBookSide _sellSide;
        private readonly AggregatedDepth _depth;
        private readonly List<ScheduledFill> _ordersToFill = new List<ScheduledFill>();

        static OrderBook()
        {
            DataSource = ConfigurationClient.Instance.GetConfigSetting("MDSDataSource", "OPEX");
            NYSESpreadImprovement = bool.Parse(ConfigurationClient.Instance.GetConfigSetting("NYSESpreadImprovement", "false"));
            Validator = new MEIncomingOrderProcessor();
        }

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.ME.OrderBook.
        /// </summary>
        /// <param name="instrumentName">The name of the instrument
        /// to create a new OrderBook for.</param>
        public OrderBook(string instrumentName)
        {
            _logger = new Logger(string.Format("OrderBook({0}) ", instrumentName));

            _logger.Trace(LogLevel.Debug, "Creating new OrderBook.");

            if (instrumentName == null || instrumentName.Length == 0)
            {
                _logger.TraceAndThrow("Null or empty instrumentName!");
            }

            _instrumentName = instrumentName;

            _buySide = new OrderBookSide(OrderSide.Buy);
            _sellSide = new OrderBookSide(OrderSide.Sell);

            MDService = MarketDataService.Get(DataSource);
            TDServer = TradeDataServer.Instance;
            ShoutService = ShoutServer.Instance;

            _depth = MDService.Depths[instrumentName];

            _logger.Trace(LogLevel.Debug, "New OrderBook created.");
        }

        /// <summary>
        /// The instrument of this OrderBook
        /// </summary>
        public string InstrumentName { get { return _instrumentName; } }

        /// <summary>
        /// Process an OrderRequest.
        /// Note: this is called SYNCHRONOUSLY from within the OrderProcessor's main loop.
        /// </summary>
        /// <param name="orderRequest">The OrderRequest to process.</param>
        public void Process(OrderRequest orderRequest)
        {
            IncomingOrder order = orderRequest.IncomingOrder;

            _logger.Trace(LogLevel.Info, "Started processing request for OrderID {0}.", order.OrderID);
            _logger.Trace(LogLevel.Debug, "Order to be processed: {0}", order.ToString());

            string instrument = order.Instrument;

            if (!_instrumentName.Equals(instrument))
            {
                _logger.TraceAndThrow("Wrong instrument for this book");
            }

            OrderBookSide orderSide = null;
            OrderBookSide otherSide = null;

            if (order.Side == OrderSide.Buy)
            {
                orderSide = _buySide;
                otherSide = _sellSide;
            }
            else if (order.Side == OrderSide.Sell)
            {
                orderSide = _sellSide;
                otherSide = _buySide;
            }

            switch (orderRequest.Type)
            {
                case OrderRequestType.NewOrder:
                    ProcessNewOrderRequest(order, orderSide, otherSide, true);
                    break;
                case OrderRequestType.Amendment:
                    ProcessAmendmentRequest(order, orderRequest.Order, orderSide, otherSide);
                    break;
                case OrderRequestType.Cancellation:
                    ProcessCancellationRequest(order, orderSide, otherSide);
                    break;
                case OrderRequestType.CancelByExchange:
                    ProcessCancelByExchangeRequest(order, orderSide, orderRequest.Message);
                    break;
                default:
                    _logger.TraceAndThrow("Invalid request type {0}", orderRequest.Type.ToString());
                    break;
            }            

            _logger.Trace(LogLevel.Debug, "Finished processing order {0}.", order.OrderID);
        }

        private void ProcessCancelByExchangeRequest(IncomingOrder order, OrderBookSide orderSide, string reason)
        {
            if (orderSide.Contains(order.OrderID))
            {
                orderSide.Remove(order.OrderID);
                _depth.Subtract(new AggregatedQuote(order.Side, order.QuantityRemaining, order.Price));
                order.Cancel(reason);
            }
        }
        
        private void ProcessCancellationRequest(IncomingOrder order, OrderBookSide orderSide, OrderBookSide otherSide)
        {
            string message = null;
            bool success = false;

            if (!Validator.ValidateCancelOrderRequest(order, ref message))
            {
                _logger.Trace(LogLevel.Warning, "Validation FAILED: {0}", message);
            }
            else if (!orderSide.Contains(order.OrderID))
            {
                message = string.Format("OrderID {0} doesn't exist.", order.OrderID);
            }
            else
            {                
                orderSide.Remove(order.OrderID);
                _depth.Subtract(new AggregatedQuote(order.Side, order.QuantityRemaining, order.Price));
                success = true;
            }

            if (success)
            {
                order.AcceptCancel();
                MDService.BroadcastNewSnapshotUpdate(_instrumentName);            
            }
            else
            {
                order.RejectCancel(message);
            }            
        }

        private void ProcessAmendmentRequest(IncomingOrder order, Order newOrder, OrderBookSide orderSide, OrderBookSide otherSide)
        {
            string message = null;
            bool success = false;

            if (!Validator.ValidateAmendOrderRequest(order, newOrder, ref message))
            {
                _logger.Trace(LogLevel.Warning, "Validation FAILED: {0}", message);
            }
            else if (!OrderStateMachine.IsActiveStatus(order.Status))
            {
                message = "Order not active";
            }
            else if (order.QuantityRemaining == 0)
            {
                message = "QuantityRemaining = 0";
            }
            else if (newOrder.Quantity < order.QuantityFilled)
            {
                message = "Quantity is less than quantity remaining";
            }
            else if (!IsNYSECompliant(newOrder, orderSide))
            {
                message = "NYSE spread improvement rule not satisfied.";                
            }
            else
            {
                if (!orderSide.Contains(order.OrderID))
                {
                    message = string.Format("OrderID {0} doesn't exist.", order.OrderID);
                }
                else
                {
                    orderSide.Remove(order.OrderID);
                    _depth.Subtract(new AggregatedQuote(order.Side, order.QuantityRemaining, order.Price));
                    success = true;
                }
            }

            if (success)
            {
                order.AcceptAmendment(newOrder);
                ProcessNewOrderRequest(order, orderSide, otherSide, false);
            }
            else
            {
                order.RejectAmendment(message);
            }
        }

        private void AnnounceShout(bool accepted, double price, OrderSide side, string user)
        {
            Shout shout = ShoutFactory.CreateShout(accepted, price, _instrumentName, side, user);
            ShoutService.BroadcastShout(shout);
        }

        private void ProcessNewOrderRequest(IncomingOrder order, OrderBookSide orderSide, OrderBookSide otherSide, bool isNewOrder)
        {
            if (isNewOrder)
            {
                string error = null;
                if (!Validator.ValidateNewOrderRequest(order, ref error))
                {
                    _logger.Trace(LogLevel.Warning, "Validation FAILED: {0}", error);
                    order.Reject(string.Format("Invalid order : {0}", error));
                    return;
                }
                else if (!IsNYSECompliant(order, orderSide))
                {
                    order.Reject("NYSE spread improvement rule not satisfied.");                    
                    return;
                }

                order.Accept();
            }

            // look for a match
            bool shoutNewMarketData = false;
            int originalQuantity = order.Quantity;
            int remainingQuantity = order.QuantityRemaining;
            int quantityFilled = 0;
            double avgFillPrice = 0;

            _ordersToFill.Clear();

            foreach (IncomingOrder otherOrder in otherSide)
            {
                if (order.Type == OrderType.Limit)
                {
                    if (order.Side == OrderSide.Buy)
                    {
                        if (order.Price < otherOrder.Price)
                        {
                            break;
                        }
                    }
                    else if (order.Side == OrderSide.Sell)
                    {
                        if (order.Price > otherOrder.Price)
                        {
                            break;
                        }
                    }
                }

                if (otherOrder.QuantityRemaining < remainingQuantity)
                {
                    remainingQuantity -= otherOrder.QuantityRemaining;
                    quantityFilled += otherOrder.QuantityRemaining;
                    avgFillPrice += otherOrder.QuantityRemaining * otherOrder.Price;
                    Trade(otherOrder, order, otherOrder.QuantityRemaining);
                }
                else
                {
                    avgFillPrice += remainingQuantity * otherOrder.Price;
                    Trade(otherOrder, order, remainingQuantity);
                    quantityFilled += remainingQuantity;
                    remainingQuantity = 0;
                    break;
                }
            }

            if (quantityFilled > 0)
            {
                shoutNewMarketData = true;
                avgFillPrice /= (double)quantityFilled;
                FillOrder(order, quantityFilled, avgFillPrice, false);
            }

            bool addWhatsRemainingToBook = false;

            if (remainingQuantity > 0 && order.Type == OrderType.Limit)
            {
                // if there's anything left, sit in the book
                shoutNewMarketData = true;
                addWhatsRemainingToBook = true;
                _depth.Add(new AggregatedQuote(order.Side, remainingQuantity, order.Price));
            }


            bool shoutAccepted = (avgFillPrice != 0) && (quantityFilled != 0);
            if (shoutNewMarketData)
            {
                Shout shout = ShoutFactory.CreateShout(shoutAccepted, order.Price, _instrumentName, order.Side, order.User);
                LastTradeUpdateMessage trade = null;
                if (shoutAccepted)
                {
                    trade = new LastTradeUpdateMessage(DataSource, _instrumentName, quantityFilled, avgFillPrice);
                }
                MDService.BroadcastNewSnapshotUpdate(_instrumentName, shout, trade);
            }
            AnnounceShout(shoutAccepted, order.Price, order.Side, order.User);

            foreach (ScheduledFill fill in _ordersToFill)
            {
                DoFillOrder(fill.Order, fill.AvgFillPrice, fill.QuantityFilled);
            }

            if (order.Type == OrderType.Market && remainingQuantity > 0)
            {
                order.Cancel("No more quantity available on the market.");
                _logger.Trace(LogLevel.Debug, "This is a market order, and it wasn't (completely) filled. The order will be cancelled.");
            }

            if (addWhatsRemainingToBook)
            {
                _logger.Trace(LogLevel.Debug, "Adding order to the book. Original quantity: {0}. Quantity filled: {1}. Quantity remaining: {2}.", originalQuantity, quantityFilled, remainingQuantity);
                orderSide.Add(order);
            }                            
        }

        private bool IsNYSECompliant(Order order, OrderBookSide orderSide)
        {
            bool res = true;
            if (NYSESpreadImprovement && (orderSide.Depth > 0) && (order.Type == OrderType.Limit))
            {
                foreach (IncomingOrder o in orderSide)
                {
                    if (((o.Side == OrderSide.Buy) && (o.Price >= order.Price))
                        || ((o.Side == OrderSide.Sell) && (o.Price <= order.Price)))
                    {
                        res = false;
                    }

                    break;
                }
            }
            return res;
        }

        private void Trade(IncomingOrder oldOrder, IncomingOrder newOrder, int quantityFilled)
        {
            double price = oldOrder.Price;

            FillOrder(oldOrder, quantityFilled, price, true);

            TradeDataMessage tdm = TradeDataMessageFactory.CreateMessage(
                oldOrder.OrderID,
                quantityFilled,
                price,
                oldOrder.LimitPrice,
                oldOrder.User,
                newOrder.User,
                oldOrder.Instrument,
                oldOrder.Side);
            TDServer.BroadcastNewTradeData(tdm);


            TradeDataMessage tdm2 = TradeDataMessageFactory.CreateMessage(
                newOrder.OrderID,
                quantityFilled,
                price,
                newOrder.LimitPrice,
                newOrder.User,
                oldOrder.User,
                newOrder.Instrument,
                newOrder.Side);
            TDServer.BroadcastNewTradeData(tdm2);
        }        

        private void FillOrder(IncomingOrder order, int quantityFilled, double avgFillPrice, bool updateDepth)
        {
            if (quantityFilled <= 0)
            {
                _logger.Trace(LogLevel.Critical, "Cannot fill for a NEGATIVE or NULL quantity!");
            }            
            
            if (updateDepth)
            {
                _depth.Subtract(new AggregatedQuote(order.Side, quantityFilled, order.Price));
            }

            _ordersToFill.Add(new ScheduledFill(order, avgFillPrice, quantityFilled));            
        }        

        private void DoFillOrder(IncomingOrder order, double avgFillPrice, int quantityFilled)
        {
            int originalQuantity = order.QuantityRemaining;

            order.Fill(avgFillPrice, quantityFilled);

            _logger.Trace(LogLevel.Debug, "Order {0} filled. QuantityRemainingBeforeFill: {1}. QuantityRemainingAfterFill: {2}.",
                order.OrderID, originalQuantity, order.QuantityRemaining);
        }

        internal void Stop()
        {
            _depth.Clear();
            MDService.BroadcastNewSnapshotUpdate(_instrumentName);
        }

        private class ScheduledFill
        {
            private IncomingOrder _order;
            private double _avgFillPrice;
            private int _quantityFilled;

            public ScheduledFill(IncomingOrder order, double avgFillPrice, int quantityFilled)
            {
                _order = order;
                _avgFillPrice = avgFillPrice;
                _quantityFilled = quantityFilled;
            }

            public IncomingOrder Order { get { return _order; } }
            public double AvgFillPrice { get { return _avgFillPrice; } }
            public int QuantityFilled { get { return _quantityFilled; } }
        }
    }
}
