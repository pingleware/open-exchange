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

namespace OPEX.Agents.Common
{
    public class OrderStatusStimulusQueue : StimulusQueue
    {
        private readonly object _root = new object();
        private readonly EventHooker _statusHooker;
        private OutgoingOrder _order;

        public OrderStatusStimulusQueue(string queueName)
            : base(queueName, StimulusType.OrderStatus)        
        { 
            _statusHooker = new EventHooker("OrderStatusChanged", HookStatusChanged);
        }

        public void ListenToOrder(OutgoingOrder outgoingOrder)
        {
            lock (_root)
            {
                _logger.Trace(LogLevel.Debug, "Listening to changes on order {0}", outgoingOrder);
                if (_order != null)
                {
                    _statusHooker.Unhook(_order);
                }
                Flush();
                _order = outgoingOrder;
                if (_order != null)
                {
                    _statusHooker.Hook(_order);
                }
            }
        }       

        public override void StartReceiving()
        {
            lock (_root)
            {
                if (_order != null)
                {
                    _statusHooker.Hook(_order);
                }
            }
        }

        public override void StopReceiving()
        {
            lock (_root)
            {
                if (_order != null)
                {
                    _statusHooker.Unhook(_order);
                    _order = null;
                }
            }            
        }

        private void StatusChanged(object sender, Order order)
        {
            lock (_root)
            {
                OutgoingOrder currentOrder = sender as OutgoingOrder;
                if (_order != null && _order.ClientOrderID == currentOrder.ClientOrderID)
                {
                    Enqueue(new OrderStatusStimulus(currentOrder.ClientOrderID, currentOrder.Status, currentOrder.Message));
                }
            }
        }

        private void HookStatusChanged(OutgoingOrder currentOrder, bool hook)
        {
            if (hook)
            {
                _logger.Trace(LogLevel.Debug, "HookStatusChanged. Hooking StatusChangedEvent on order {0}", currentOrder);
                currentOrder.StatusChanged += new OutgoingOrderEventHandler(StatusChanged);
            }
            else
            {
                _logger.Trace(LogLevel.Debug, "HookStatusChanged. Unhooking StatusChangedEvent on order {0}", currentOrder);
                currentOrder.StatusChanged -= new OutgoingOrderEventHandler(StatusChanged);
            }
        }        
    }
}
