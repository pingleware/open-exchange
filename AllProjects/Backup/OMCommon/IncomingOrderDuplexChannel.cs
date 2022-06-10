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
using System.Messaging;

using OPEX.Common;
using OPEX.Messaging;

namespace OPEX.OM.Common
{
    /// <summary>
    /// Represents the method that will handle events raised 
    /// on new OrderInstruction-s.
    /// </summary>
    /// <param name="sender">The sender of this event.</param>
    /// <param name="instruction">The OrderInstruction associated to this event.</param>
    /// <param name="order">The IncomingOrder associated to this event.</param>
    /// <param name="otherOrder">The Order associated to this event.</param>
    public delegate void OrderInstructionEventHandler(object sender, OrderInstruction instruction, IncomingOrder order, Order otherOrder);

    /// <summary>
    /// A strongly typed, bidirectional communication channel
    /// that receives OrderMessage-s and responds to them.
    /// </summary>
    public class IncomingOrderDuplexChannel : IncomingDuplexChannel<OrderMessage>
    {
        protected new readonly Logger _logger;
        private IOrderWriter _orderWriter;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.OM.Common.IncomingOrderDuplexChannel.
        /// </summary>
        /// <param name="channelName"></param>
        /// <param name="orderWriter"></param>
        public IncomingOrderDuplexChannel(string channelName, IOrderWriter orderWriter)
            : base(channelName)
        {
            _logger = new Logger(string.Format("IncomingOrderDuplexChannel({0})", channelName));
            _orderWriter = orderWriter;
        }

        /// <summary>
        /// Occurs when an order request (of any kind) is received.
        /// </summary>
        public event OrderInstructionEventHandler OrderInstructionReceived;

        protected override void OnMessageReceived(OrderMessage orderMessage)
        {
            Order oldOrder = orderMessage.Order;
            IncomingOrder incomingOrder = null;

            if (orderMessage.Instruction == OrderInstruction.Ping)
            {
                //_logger.Trace(LogLevel.Info, "OnMessageReceived. PING message received from {0}, ignoring.", orderMessage.Origin);
                return;
            }

            _logger.Trace(LogLevel.Method, "New order request received: {0} {1}", orderMessage.Instruction.ToString(), oldOrder.ToString());

            if (orderMessage.Instruction == OrderInstruction.New)
            {
                incomingOrder = OrderFactory.CreateIncomingOrder(orderMessage.Order);
                incomingOrder.Changed += new EventHandler<IncomingOrderChangedEventArgs>(Order_Changed);
                _logger.Trace(LogLevel.Info, "Saving Order as soon as it's received: {0}", incomingOrder.ToString());
                Save(incomingOrder);
            }
            else
            {
                if (!OrderFactory.IncomingOrders.Contains(orderMessage.Order.OrderID))
                {
                    _logger.TraceAndThrow("An amendment/cancellation has been sent for an order that hasn't been received yet: {0}", orderMessage.Order.OrderID);
                }
                incomingOrder = OrderFactory.IncomingOrders[orderMessage.Order.OrderID] as IncomingOrder;
            }

            if (OrderInstructionReceived != null)
            {
                OrderInstructionReceived(this, orderMessage.Instruction, incomingOrder, orderMessage.Order);
            }

        }

        private void Order_Changed(object sender, IncomingOrderChangedEventArgs args)
        {
            IncomingOrder incomingOrder = sender as IncomingOrder;

            Save(incomingOrder);

            _logger.Trace(LogLevel.Method,
                "Propagating changes on IncomingOrder {0} back to Source {1}", incomingOrder.OrderID, incomingOrder.Origin);

            Respond(new OrderMessage(args.Instruction, incomingOrder));
        }

        private void Save(IncomingOrder order)
        {
            try
            {
                _orderWriter.WriteOrder(order);
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "Error while saving order: {0}", ex.Message);
            }
        }
    }
}
