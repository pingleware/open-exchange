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

using OPEX.OM.Common;
using OPEX.Common;
using OPEX.Storage;
using OPEX.Configuration.Client;
using OPEX.TDS.Client;
using OPEX.TDS.Common;
using OPEX.ShoutService;

namespace OPEX.OM
{
    /// <summary>
    /// Receives IncomingOrder-s, and routes them creating OutgoingOrder-s
    /// and sending them to the appropriate destination.
    /// </summary>
    public class OrderManager
    {
        private readonly static string DefaultOMChannelname = "OM";
        private readonly Logger _logger;
        private readonly IncomingOrderDuplexChannel _incomingChannel;
        private readonly OutgoingOrderDuplexChannel _outgoingChannel;
        private readonly IOrderProcessor _OMIncomingOrderProcessor;
        private readonly DBWriter _tradeWriter;
        private readonly DBWriter _shoutWriter;
        
        private ShoutWatcher _shoutWatcher;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.OM.OrderManager.
        /// </summary>
        public OrderManager()
        {
            _logger = new Logger("OrderManager");

            if (!DBConnectionManager.Instance.IsConnected)
            {
                DBConnectionManager.Instance.Connect();
            }
            
            _incomingChannel = new IncomingOrderDuplexChannel(DefaultOMChannelname, new OrderWriter());
            _outgoingChannel = new OutgoingOrderDuplexChannel(DefaultOMChannelname);
            _OMIncomingOrderProcessor = new OMIncomingOrderProcessor();

            bool purge = bool.Parse(ConfigurationClient.Instance.GetConfigSetting("PurgeQueuesOnStartup", "true"));
            if (purge)
            {
                _incomingChannel.Purge();
                _outgoingChannel.Purge();
            }

            OrderFactory.OrderSender = _outgoingChannel;

            _tradeWriter = new DBWriter();
            _shoutWriter = new DBWriter();
        }

        /// <summary>
        /// Starts the OrderManager.
        /// </summary>
        public void Start()
        {
            _incomingChannel.OrderInstructionReceived += new OrderInstructionEventHandler(IncomingChannel_OrderInstructionReceived);

            TradeDataClient.Instance.TradeMessageReceived += new TradeMessageReceivedEventHandler(TradeMessageReceived);
            TradeDataClient.Instance.Start();

            _shoutWatcher = ShoutClient.Instance.CreateShoutWatcher("*");
            _shoutWatcher.NewShout += new NewShoutEventHandler(ShoutWatcher_NewShout);
            ShoutClient.Instance.Start();

            _outgoingChannel.Start();
            _incomingChannel.Start();
        }       
      
        /// <summary>
        /// Stops the OrderManager.
        /// </summary>
        public void Stop()
        {
            _incomingChannel.OrderInstructionReceived -= new OrderInstructionEventHandler(IncomingChannel_OrderInstructionReceived);

            TradeDataClient.Instance.TradeMessageReceived -= new TradeMessageReceivedEventHandler(TradeMessageReceived);
            TradeDataClient.Instance.Stop();

            _shoutWatcher.NewShout -= new NewShoutEventHandler(ShoutWatcher_NewShout);
            _shoutWatcher.Dispose();
            ShoutClient.Instance.Stop();

            _incomingChannel.Stop();
            _outgoingChannel.Stop();

            if (DBConnectionManager.Instance.IsConnected)
            {
                DBConnectionManager.Instance.Disconnect();
            }
        }

        private void ShoutWatcher_NewShout(object sender, NewShoutEventArgs args)
        {
            _shoutWriter.Write(args.Shout);
        }

        private void TradeMessageReceived(object sender, TradeDataMessage tradeDataMessage)
        {
            _tradeWriter.Write(tradeDataMessage);
        }

        private void IncomingChannel_OrderInstructionReceived(object sender, OrderInstruction instruction, IncomingOrder order, Order otherOrder)
        {
            switch (instruction)
            {
                case OrderInstruction.New:
                    {
                        IncomingOrder newOrder = order;
                        _logger.Trace(LogLevel.Info, "NewOrderRequestReceived. IncomingOrder: {0}", newOrder.ToString());

                        string error = null;
                        if (!_OMIncomingOrderProcessor.ValidateNewOrderRequest(newOrder, ref error))
                        {
                            _logger.Trace(LogLevel.Warning, "Validation FAILED: {0}", error);
                            newOrder.Reject(string.Format("Invalid order : {0}", error));
                            return;
                        }

                        OutgoingOrder outgoingOrder = newOrder.CreateThroughOutgoingOrder();

                        _logger.Trace(LogLevel.Info, "NewOrderRequestReceived. ThroughOutgoingOrder: {0}", outgoingOrder.ToString());

                        outgoingOrder.StatusChanged += new OutgoingOrderEventHandler(OutgoingOrder_StatusChanged);
                        outgoingOrder.Send();
                        break;
                    }
                case OrderInstruction.Amend:
                    {
                        IncomingOrder oldOrder = order;
                        Order newOrder = otherOrder;
                        _logger.Trace(LogLevel.Info, "OrderAmendmentRequestReceived. IncomingOrder: {0}", newOrder.ToString());

                        string message = null;
                        if (!_OMIncomingOrderProcessor.ValidateAmendOrderRequest(oldOrder, newOrder, ref message))
                        {
                            _logger.Trace(LogLevel.Warning, "Validation FAILED: {0}", message);
                            oldOrder.RejectAmendment(message);
                            return;
                        }

                        OutgoingOrder outgoingOrder = null;
                        if (!OrderFactory.OutgoingOrderByOrderID.Contains(oldOrder.OrderID))
                        {
                            _logger.TraceAndThrow("An amendment request was received for an order that hasn't been received yet: {0}", oldOrder.OrderID);
                        }
                        outgoingOrder = OrderFactory.OutgoingOrderByOrderID[oldOrder.OrderID] as OutgoingOrder;

                        _logger.Trace(LogLevel.Info, "OrderAmendmentRequestReceived. OutgoingOrder: {0}", outgoingOrder.ToString());

                        if (OrderStateMachine.IsActiveStatus(outgoingOrder.Status))
                        {
                            try
                            {
                                outgoingOrder.Amend(newOrder.Quantity, newOrder.Price, newOrder.Parameters);
                            }
                            finally { }
                        }
                        else
                        {
                            _logger.Trace(LogLevel.Warning,
                                "Cannot amend order {1} because its status is {0}. The request won't be propagate downstream",
                                outgoingOrder.Status, outgoingOrder.OrderID);
                        }
                        break;
                    }
                case OrderInstruction.Cancel:
                    {
                        IncomingOrder orderToCancel = order;
                        _logger.Trace(LogLevel.Info, "OrderCancellationRequestReceived. IncomingOrder: {0}", orderToCancel.ToString());

                        string message = null;
                        if (!_OMIncomingOrderProcessor.ValidateCancelOrderRequest(orderToCancel, ref message))
                        {
                            _logger.Trace(LogLevel.Warning, "Validation FAILED: {0}", message);
                            orderToCancel.RejectCancel(message);
                            return;
                        }

                        OutgoingOrder outgoingOrder = null;
                        if (!OrderFactory.OutgoingOrderByOrderID.Contains(orderToCancel.OrderID))
                        {
                            _logger.TraceAndThrow("A Cancellation request was received for an order that hasn't been received yet: {0}", orderToCancel.OrderID);
                        }
                        outgoingOrder = OrderFactory.OutgoingOrderByOrderID[orderToCancel.OrderID] as OutgoingOrder;

                        _logger.Trace(LogLevel.Info, "OrderCancellationRequestReceived. OutgoingOrder: {0}", outgoingOrder.ToString());

                        outgoingOrder.Cancel();
                        break;
                    }
                case OrderInstruction.Ping:
                    _logger.Trace(LogLevel.Debug, "PING message received from {0}, ignoring", order.Origin);
                    break;
                default:
                    throw new ArgumentException("IncomingChannel_OrderInstructionReceived. Invalid instruction: " + instruction.ToString());                    
            }
        }

        private void OutgoingOrder_StatusChanged(object sender, Order newOrder)
        {
            OutgoingOrder oldOrder = sender as OutgoingOrder;
            IncomingOrder incomingOrder = oldOrder.ParentOrder;

            switch (newOrder.Status)
            {
                case OrderStatus.Accepted:
                    incomingOrder.Accept();
                    break;
                case OrderStatus.Rejected:
                    incomingOrder.Reject(newOrder.Message);
                    break;
                case OrderStatus.Filled:
                case OrderStatus.CompletelyFilled:
                case OrderStatus.Overfilled:
                    incomingOrder.Fill(newOrder);
                    break;
                case OrderStatus.AmendAccepted:
                    incomingOrder.AcceptAmendment(newOrder);
                    break;
                case OrderStatus.AmendRejected:
                    incomingOrder.RejectAmendment(newOrder.Message);
                    break;
                case OrderStatus.Cancelled:
                    incomingOrder.AcceptCancel();
                    break;
                case OrderStatus.CancelRejected:
                    incomingOrder.RejectCancel(newOrder.Message);
                    break;
                case OrderStatus.CancelledByExchange:
                    incomingOrder.Cancel(newOrder.Message);
                    break;
                default:
                    break;
            }
        }
    }
}
