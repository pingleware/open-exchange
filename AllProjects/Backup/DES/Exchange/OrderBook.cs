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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OPEX.StaticData;
using OPEX.DES.OrderManager;
using OPEX.Common;
using OPEX.Storage;
using OPEX.DES.DB;

namespace OPEX.DES.Exchange
{
    public partial class GlobalOrderBook
    {
        protected class OrderBook : IOrderProcessor
        {
            private readonly GlobalOrderBook _gob;
            private readonly Instrument _instrument;
            private readonly OrderBookSide _buySide;
            private readonly OrderBookSide _sellSide;
            private readonly List<ScheduledFill> _ordersToFill = new List<ScheduledFill>();
            private readonly AggregatedDepth _depth;
            private readonly Logger _logger;

            private MarketData _marketData;

            public OrderBook(GlobalOrderBook gob, string instrument)
            {
                _logger = new Logger(string.Format("OrderBook({0}) ", instrument));
                _gob = gob;
                _instrument = StaticDataManager.Instance.InstrumentStaticData[instrument];
                if (_instrument == null)
                {
                    throw new NullReferenceException(string.Format("OrderBook.ctor. No static data for instrument {0}", instrument));
                }

                _buySide = new OrderBookSide(OrderSide.Buy);
                _sellSide = new OrderBookSide(OrderSide.Sell);
              
                _depth = _gob._depths[instrument];
            }

            public string Instrument { get { return _instrument.Ric; } }
            public MarketData MarketData { get { return _marketData; } }
            private DBWriter Writer { get { return _gob._dbWriter; } }

            #region IOrderProcessor Members

            public bool AcceptNewOrder(IIncomingOrder order)
            {
                if (!Validate(order))
                {
                    return false;
                }

                if (!order.Open)
                {
                    order.AcceptNewOrder();
                }

                OrderBookSide orderSide = MySide(order);
                OrderBookSide otherSide = OtherSide(order);

                int originalQuantity = order.Quantity;
                int remainingQuantity = order.QuantityRemaining;
                int quantityFilled = 0;
                double avgFillPrice = 0;

                _ordersToFill.Clear();

                foreach (IIncomingOrder otherOrder in otherSide)
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
                    avgFillPrice /= (double)quantityFilled;
                    FillOrder(order, quantityFilled, avgFillPrice, false);
                }

                bool addWhatsRemainingToBook = false;

                if (remainingQuantity > 0)
                {
                    // if there's anything left, sit in the book
                    addWhatsRemainingToBook = true;
                    _depth.Add(new AggregatedQuote(order.Side, remainingQuantity, order.Price));
                }

                bool shoutAccepted = (avgFillPrice != 0) && (quantityFilled != 0);
                Shout shout = new Shout(shoutAccepted, order.Price, avgFillPrice, _instrument.Ric, order.Side, order.Owner);
                AggregatedDepthSnapshot snapshot = _depth.Snapshot;
                _marketData = new MarketData(snapshot, shout);
                
                Writer.Write(new WriteableShout(shout));                

                foreach (ScheduledFill fill in _ordersToFill)
                {
                    DoFillOrder(fill.Order, fill.AvgFillPrice, fill.QuantityFilled);
                }

                if (addWhatsRemainingToBook)
                {
                    orderSide.Add(order);
                }  

                return true;
            }

            private void Trade(IIncomingOrder oldOrder, IIncomingOrder newOrder, int quantityFilled)
            {
                double price = oldOrder.Price;

                FillOrder(oldOrder, quantityFilled, price, true);

                Writer.Write(new WriteableFill(quantityFilled, price, oldOrder.LimitPrice, oldOrder.Owner, newOrder.Owner, oldOrder.RIC, oldOrder.Side));
                Writer.Write(new WriteableFill(quantityFilled, price, newOrder.LimitPrice, newOrder.Owner, oldOrder.Owner, oldOrder.RIC, newOrder.Side));
            }

            private void FillOrder(IIncomingOrder order, int quantityFilled, double avgFillPrice, bool updateDepth)
            {
                if (quantityFilled <= 0)
                {
                    _logger.Trace(LogLevel.Critical, "FillOrder. Cannot fill for a NEGATIVE or NULL quantity!");
                }

                if (updateDepth)
                {
                    _depth.Subtract(new AggregatedQuote(order.Side, quantityFilled, order.Price));
                }

                _ordersToFill.Add(new ScheduledFill(order, avgFillPrice, quantityFilled));
            }

            private void DoFillOrder(IIncomingOrder order, double avgFillPrice, int quantityFilled)
            {
                int originalQuantity = order.QuantityRemaining;

                order.Fill(quantityFilled, avgFillPrice);
                if (order.QuantityRemaining == 0)
                {
                    OrderBookSide mySide = MySide(order);
                    if (mySide.Contains(order.ID))
                    {
                        mySide.Remove(order.ID);
                    }
                }
            }

            public bool AcceptOrderAmendment(IIncomingOrder order, double newPrice, int newQuantity)
            {                
                if (!Validate(order, false, newPrice, newQuantity))
                {
                    return false;
                }

                OrderBookSide orderSide = MySide(order);
                OrderBookSide otherSide = OtherSide(order);

                if (!orderSide.Contains(order.ID))
                {
                    _logger.Trace(LogLevel.Critical, "AcceptOrderAmendment. OrderID {0} doesn't exist.", order.ID);
                    return false;
                }

                
                orderSide.Remove(order.ID);
                _depth.Subtract(new AggregatedQuote(order.Side, order.QuantityRemaining, order.Price));

                order.AcceptAmend(newQuantity, newPrice);

                return AcceptNewOrder(order);
            }

            #endregion

            private OrderBookSide MySide(IOrder o)
            {
                return (o.Side == OrderSide.Buy) ? _buySide : _sellSide;
            }

            private OrderBookSide OtherSide(IOrder o)
            {
                return (o.Side == OrderSide.Sell) ? _buySide : _sellSide;
            }

            private bool Validate(IIncomingOrder order)
            {
                return Validate(order, true, 0, 0);
            }

            private bool Validate(IIncomingOrder order, bool newOrder, double newPrice, double newQuantity)
            {
                if (order == null)
                {
                    return false;
                }

                if (order.RIC == null || !order.RIC.Equals(_instrument.Ric))
                {
                    return false;
                }

                if (order.Quantity < _instrument.MinQty || order.Quantity > _instrument.MaxQty)
                {
                    return false;
                }                

                if (order.Price < _instrument.MinPrice || order.Price > _instrument.MaxPrice)
                {
                    return false;
                }

                if (order.Owner == null || order.Owner.Length == 0)
                {
                    return false;
                }

                if (!newOrder)
                {
                    if (newQuantity < order.QuantityFilled)
                    {
                        return false;
                    }                    
                }

                return true;
            }

            private class ScheduledFill
            {
                private IIncomingOrder _order;
                private double _avgFillPrice;
                private int _quantityFilled;

                public ScheduledFill(IIncomingOrder order, double avgFillPrice, int quantityFilled)
                {
                    _order = order;
                    _avgFillPrice = avgFillPrice;
                    _quantityFilled = quantityFilled;
                }

                public IIncomingOrder Order { get { return _order; } }
                public double AvgFillPrice { get { return _avgFillPrice; } }
                public int QuantityFilled { get { return _quantityFilled; } }
            }

            public void Clear()
            {
                foreach (OrderBookSide side in new OrderBookSide[] { _buySide, _sellSide })
                {
                    foreach (IIncomingOrder order in side)
                    {
                        order.Cancel();
                    }
                    side.Clear();
                }
                _marketData = new MarketData(new AggregatedDepthSnapshot(_instrument.Ric), null);
            }
        }
    }
}
