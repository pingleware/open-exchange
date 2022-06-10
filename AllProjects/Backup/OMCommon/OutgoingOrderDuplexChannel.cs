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
using OPEX.Configuration.Client;

namespace OPEX.OM.Common
{
    /// <summary>
    /// A strongly typed, bidirectional communication channel
    /// that sends OrderMessage-s and receives status notifications.
    /// </summary>
    public class OutgoingOrderDuplexChannel : OutgoingDuplexChannel<OrderMessage>, IOrderSender
    {
        private readonly static bool IsOMClient;
        private readonly static string OMLocation;
        private readonly KeepAliveMessageGenerator _keepAliveMessageGenerator;

        protected new readonly Logger _logger;

        /// <summary>
        /// The default name of the OM channel.
        /// </summary>
        public readonly static string DefaultOMChannelName = "OM";

        static OutgoingOrderDuplexChannel()
        {
            IsOMClient = !ConfigurationClient.Instance.GetConfigSetting("OMMode", "Client").Equals("Server");
            if (IsOMClient)
            {
                OMLocation = ConfigurationClient.Instance.GetConfigSetting("OMHostLocation", "localhost");
            }
        }

        /// <summary>
        /// Starts the OutgoingOrderDuplexChannel.
        /// </summary>
        public override void Start()
        {
            base.Start();
            _keepAliveMessageGenerator.Start();
        }

        /// <summary>
        /// Stops the OutgoingOrderDuplexChannel.
        /// </summary>
        public override void Stop()
        {
            _keepAliveMessageGenerator.Ping -= new KeepAliveEventHandler(KeepAliveMessageGenerator_Ping);
            _keepAliveMessageGenerator.Stop();
            base.Stop();            
        }

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.OM.Common.OutgoingOrderDuplexChannel.
        /// </summary>
        /// <param name="channelName">The name of the channel.</param>
        public OutgoingOrderDuplexChannel(string channelName)
            : base(channelName)
        {
            _logger = new Logger(string.Format("OutgoingOrderDuplexChannel({0})", channelName));

            if (IsOMClient)
            {
                RegisterDefaultChannel(DefaultOMChannelName, OMLocation);
            }
            else
            {
                string hostsCSL = ConfigurationClient.Instance.GetConfigSetting("OMAllowedDestinationsHosts", "");
                string destsCSL = ConfigurationClient.Instance.GetConfigSetting("OMAllowedDestinations", "");

                string[] hostsBits = hostsCSL.Split(new char[] { ',' });
                string[] destsBits = destsCSL.Split(new char[] { ',' });
                for (int i = 0; i < hostsBits.Length; ++i)
                {
                    string host = hostsBits[i];
                    string dest = destsBits[i];
                    RegisterChannel(dest, host);
                }
            }
            _keepAliveMessageGenerator = new KeepAliveMessageGenerator(1000, null);
            _keepAliveMessageGenerator.Ping += new KeepAliveEventHandler(KeepAliveMessageGenerator_Ping);
        }       

        protected override void OnMessageReceived(OrderMessage orderMessage)
        {
            Order newOrderResponse = orderMessage.Order;

            _logger.Trace(LogLevel.Info, "OnOrderResponseReceived: {0}", newOrderResponse.ToString());

            if (orderMessage.Instruction == OrderInstruction.Ping)
            {
                // debug only
                // _logger.Trace(LogLevel.Info, "OnMessageReceived. Ping message received from {0}, ignoring.",
                // orderMessage.Origin);
            }
            else
            {
                OutgoingOrder oldOrder = FindOldOrder(newOrderResponse);

                if (oldOrder == null)
                {
                    _logger.Trace(LogLevel.Warning, "OnMessageReceived. Response received for an order that isn't in the database");
                }
                else
                {
                    OrderStateMachine stateMachine = new OrderStateMachine(oldOrder.Status);
                    if (!stateMachine.Change(newOrderResponse.Status))
                    {
                        _logger.Trace(LogLevel.Warning, "OnMessageReceived. Unexpected status change from {0} to {1} for OrderID {2} ClientOrderID {3}.",
                            oldOrder.Status.ToString(), newOrderResponse.Status.ToString(),
                            newOrderResponse.OrderID, newOrderResponse.ClientOrderID);
                    }

                    OrderFactory.UpdateOutgoingOrder(oldOrder, newOrderResponse);
                }
            }
        }

        private OutgoingOrder FindOldOrder(Order newOrder)
        {
            OutgoingOrder o = OrderFactory.OutgoingOrders[newOrder.ClientOrderID] as OutgoingOrder;

            return o;
        }

        private void KeepAliveMessageGenerator_Ping(object sender, object pingMessage)
        {
            // debug only
            //_logger.Trace(LogLevel.Debug, "About to send PING ORDER MESSAGE");

            Order o = OrderFactory.CreateOutgoingPingOrder();
            base.SendMessage(new OrderMessage(OrderInstruction.Ping, o));

            // debug only
            //_logger.Trace(LogLevel.Debug, "PING ORDER MESSAGE sent");
        }

        #region IOrderSender Members

        public void Send(OutgoingOrder order)
        {
            _logger.Trace(LogLevel.Debug, "About to send order {0}", order.ToString());
            base.SendMessage(new OrderMessage(OrderInstruction.New, order));
            _logger.Trace(LogLevel.Info, "Order sent: {0}", order.ToString());
        }

        public void Amend(OutgoingOrder order, Order newOrder)
        {
            _logger.Trace(LogLevel.Debug, "About to amend order {0} with order {1}", order.OrderID, newOrder.ToString());
            base.SendMessage(new OrderMessage(OrderInstruction.Amend, newOrder));
            _logger.Trace(LogLevel.Info, "Amendment sent: {0}", order.ToString());
        }

        public void Cancel(OutgoingOrder order)
        {
            _logger.Trace(LogLevel.Debug, "About to cancel order {0}", order.ToString());
            base.SendMessage(new OrderMessage(OrderInstruction.Cancel, order));
            _logger.Trace(LogLevel.Info, "Order cancelled: {0}", order.ToString());
        }

        #endregion
    }
}
