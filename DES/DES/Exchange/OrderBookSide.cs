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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OPEX.DES.OrderManager;

namespace OPEX.DES.Exchange
{
    public partial class GlobalOrderBook
    {
        protected class OrderBookSide : IEnumerable, IEnumerator
        {
            private static readonly IComparer DefaultPriority = new PriceTimePriority();

            private int _rowPos;
            private ArrayList _orderDataStore = ArrayList.Synchronized(new ArrayList());
            private Hashtable _ordersByOrderID = Hashtable.Synchronized(new Hashtable());
            private OrderSide _side;

            public OrderBookSide(OrderSide side)
            {
                _side = side;
                _rowPos = -1;
            }

            public void Add(IOrder order)
            {
                if (order == null)
                {
                    throw new NullReferenceException("Null order");
                }

                if (order.Quantity <= 0)
                {
                    throw new NullReferenceException("Non positive quantity");
                }

                _orderDataStore.Add(order);
                _ordersByOrderID.Add(order.ID, order);
                _orderDataStore.Sort(DefaultPriority);
            }

            public bool Contains(long orderId)
            {
                return _ordersByOrderID.ContainsKey(orderId);
            }

            public void Remove(long orderId)
            {
                if (!_ordersByOrderID.ContainsKey(orderId))
                {
                    return;
                }

                int index = -1;
                for (int i = 0; i < _orderDataStore.Count; ++i)
                {
                    IOrder o = _orderDataStore[i] as IOrder;
                    if (o.ID == orderId)
                    {
                        index = i;
                        break;
                    }
                }

                if (index >= 0)
                {
                    _orderDataStore.RemoveAt(index);
                    _ordersByOrderID.Remove(orderId);
                    _orderDataStore.Sort(DefaultPriority);
                }
            }

            public OrderSide Side { get { return _side; } }
            public int Depth { get { return _orderDataStore.Count; } }

            #region IEnumerator Members

            public object Current
            {
                get { return _orderDataStore[_rowPos]; }
            }

            public bool MoveNext()
            {
                _rowPos++;

                while (_rowPos < _orderDataStore.Count)
                {
                    IOrder currentOrder = _orderDataStore[_rowPos] as IOrder;

                    if (currentOrder.QuantityRemaining == 0)
                    {
                        _orderDataStore.RemoveAt(_rowPos);
                    }
                    else
                    {
                        return true;
                    }
                }

                Reset();

                return false;
            }

            public void Reset()
            {
                _rowPos = -1;
            }

            #endregion

            #region IEnumerable Members

            public IEnumerator GetEnumerator()
            {
                Reset();
                return this;
            }

            #endregion

            public void Clear()
            {
                _orderDataStore.Clear();
                _ordersByOrderID.Clear();
            }
        }
    }
}
