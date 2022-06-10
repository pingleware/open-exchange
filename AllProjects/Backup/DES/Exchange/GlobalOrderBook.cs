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
using OPEX.Storage;
using OPEX.DES.OrderManager;

namespace OPEX.DES.Exchange
{
    public partial class GlobalOrderBook : IOrderProcessor
    {
        private DBWriter _dbWriter;
        private readonly Dictionary<string, OrderBook> _orderBooks;
        private readonly Dictionary<string, AggregatedDepth> _depths;

        public GlobalOrderBook()
        {
            _orderBooks = new Dictionary<string, OrderBook>();
            _depths = new Dictionary<string, AggregatedDepth>();

            foreach (string instrument in StaticDataManager.Instance.InstrumentStaticData.Instruments)
            {
                _depths.Add(instrument, new AggregatedDepth(instrument));
                _orderBooks.Add(instrument, new OrderBook(this, instrument));                
            }

            _dbWriter = new DBWriter();
        }

        public MarketData this[string instrument] { get { return _orderBooks[instrument].MarketData; } }

        public void Clear()
        {
            foreach (string instrument in _orderBooks.Keys)
            {
                OrderBook ob = _orderBooks[instrument];
                ob.Clear();
                AggregatedDepth depth = _depths[instrument];
                depth.Clear();
            }
        }

        #region IOrderProcessor Members

        public bool AcceptNewOrder(IIncomingOrder order)
        {
            OrderBook ob = SelectOrderBook(order);
            return ob.AcceptNewOrder(order);
        }

        public bool AcceptOrderAmendment(IIncomingOrder order, double newPrice, int newQuantity)
        {
            OrderBook ob = SelectOrderBook(order);
            return ob.AcceptOrderAmendment(order, newPrice, newQuantity);
        }

        #endregion

        private OrderBook SelectOrderBook(IOrder order)
        {
            if (order == null)
            {
                throw new NullReferenceException("AcceptNewOrder. Null order");
            }

            if (!_orderBooks.ContainsKey(order.RIC))
            {
                throw new ApplicationException(string.Format("AcceptNewOrder. Orderbook for {0} doesn't exist.", order.RIC));
            }

            return _orderBooks[order.RIC];
        }
    }
}
