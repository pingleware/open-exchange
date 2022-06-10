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

using OPEX.Common;
using OPEX.OM.Common;

namespace OPEX.DWEGUI
{
    public class OrderMonitor
    {
        private static OrderMonitor _instance;
        private static readonly object _root = new object();

        private readonly Dictionary<long, bool> _sentOrders;
        private readonly Dictionary<long, Order> _myOrdersByOrderID;
        private readonly Logger _log;        

        public static OrderMonitor Instance
        {
            get
            {
                lock (_root)
                {
                    if (_instance == null)
                    {
                        _instance = new OrderMonitor();
                    }

                    return _instance;
                }
            }
        }

        public OutgoingOrderEventHandler OrderAccepted;
        public OutgoingOrderEventHandler OrderSwitchedToActiveState;
        public OutgoingOrderEventHandler OrderSwitchedToClosedState;
        public OutgoingOrderEventHandler OrderFilled;
        public OutgoingOrderEventHandler OrderRejected;

        private OrderMonitor()
        {
            _sentOrders = new Dictionary<long, bool>();
            _myOrdersByOrderID = new Dictionary<long, Order>();
            _log = new Logger("OrderMonitor");
        }

        public void Hook(OutgoingOrder o)
        {
            _log.Trace(LogLevel.Debug, "Hook. Hooking order {0}", o.ToString());
            o.StatusChanged += new OutgoingOrderEventHandler(OnStatusChanged);
            o.OrderSent += new OutgoingOrderEventHandler(OnOrderSent);
        }

        public void Send(OutgoingOrder o)
        {
            _log.Trace(LogLevel.Info, "Send. Sending order {0}", o.ToString());
            _sentOrders[o.ClientOrderID] = false;
            o.Send();
        }

        public void Unhook(OutgoingOrder o)
        {
            _log.Trace(LogLevel.Debug, "Unhook. Unhooking order {0}", o.ToString());
            o.StatusChanged -= new OutgoingOrderEventHandler(OnStatusChanged);
            o.OrderSent -= new OutgoingOrderEventHandler(OnOrderSent);
        }

        private void OnStatusChanged(object sender, Order newOrder)
        {
            OutgoingOrder order = sender as OutgoingOrder;
            bool active = OrderStateMachine.IsActiveStatus(order.Status);

            _log.Trace(LogLevel.Debug, "OnStatusChanged. Status changed {0}", order.ToString());

            if (!active)
            {
                order.StatusChanged -= new OutgoingOrderEventHandler(OnStatusChanged);
            }

            if (!_sentOrders.ContainsKey(order.ClientOrderID))
            {
                _log.Trace(LogLevel.Warning,
                    "OnStatusChanged. A status change was received for an order that wasn't sent: ClientOrderID {0}",
                    order.ClientOrderID);
                return;
            }

            if (!_sentOrders[order.ClientOrderID])
            {
                _sentOrders[order.ClientOrderID] = true;                
            }
            else if (!active)
            {
                // the order had been already sent, and now
                // it's not active
            }

            _myOrdersByOrderID[order.OrderID] = order;

            switch (order.Status)
            {
                case OrderStatus.Accepted:
                    OrderAccepted(order, newOrder);
                    break;
                case OrderStatus.AmendAccepted:
                case OrderStatus.AmendRejected:
                case OrderStatus.CancelRejected:                
                    OrderSwitchedToActiveState(order, newOrder);
                    break;

                case OrderStatus.CancelledByExchange:
                case OrderStatus.Cancelled:
                case OrderStatus.Overfilled:
                    OrderSwitchedToClosedState(order, newOrder);
                    break;

                case OrderStatus.Filled:
                case OrderStatus.CompletelyFilled:
                    OrderFilled(order, newOrder);
                    break;

                case OrderStatus.Rejected:
                    OrderRejected(order, newOrder);
                    break;

                case OrderStatus.NewOrder:
                default:
                    _log.TraceAndThrow("OnStatusChanged. Should never get here! Status = {0}", order.Status.ToString());
                    break;
            }
        }

        private void OnOrderSent(object sender, Order order)
        {
            Order o = sender as Order;
            _log.Trace(LogLevel.Debug, "OnOrderSent. Order sent: {0}", o.ToString());
        }
    }
}
