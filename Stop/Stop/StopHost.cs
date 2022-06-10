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
using System.Text;
using System.Collections;

using OPEX.Common;
using OPEX.OM.Common;
using OPEX.MDS.Client;
using OPEX.Messaging;
using OPEX.Configuration.Client;

namespace OPEX.Stop
{
    class StopHost : MainLogger
    {
        private MarketDataClient MDClient;
        private IncomingOrderDuplexChannel _incomingOrderChannel;
        private OutgoingOrderDuplexChannel _outgoingOrderChannel;
        private IOrderProcessor _STOPOrderProcessor;
        private bool _running;

        public StopHost()
            : base("StopLossTrader")
        {
            string channelName = ConfigurationClient.Instance.GetConfigSetting("ApplicationName", null);
            if (channelName == null)
            {
                throw new ApplicationException("Configuration setting OMClientName is null. Cannot create IncomingOrderDuplexChannel.");
            }

            _incomingOrderChannel =
                new IncomingOrderDuplexChannel(channelName, new DummyOrderWriter());
            _outgoingOrderChannel =
                new OutgoingOrderDuplexChannel(channelName);
            OrderFactory.OrderSender = _outgoingOrderChannel;
            _STOPOrderProcessor = new StopOrderProcessor();
            _running = false;
        }

        public void Run()
        {
            try
            {
                MDClient = MarketDataClient.Instance;
                MDClient.Start();
            }
            catch (Exception ex)
            {
                Trace(LogLevel.Critical, "Exception while starting MarketDataClient: {0}", ex.Message);
                return;
            }

            try
            {
                _incomingOrderChannel.NewIncomingOrderReceived += new NewIncomingOrderEventHandler(IncomingChannel_NewOrderRequestReceived);
                _incomingOrderChannel.NewOrderAmendmentReceived += new AmendOrderEventHandler(IncomingChannel_OrderAmendmentRequestReceived);
                _incomingOrderChannel.NewOrderCancellationReceived += new CancelOrderEventHandler(IncomingChannel_OrderCancellationRequestReceived);
                _incomingOrderChannel.Start();
                _outgoingOrderChannel.Start();
                _running = true;

                Pause("Press ENTER to stop the algorithm...");
            }
            catch (Exception e)
            {
                Trace(LogLevel.Critical, "Exception in StopHost.Run(): {0}", e.Message);
            }
            finally
            {
                try
                {
                    MDClient.Stop();
                    if (_running)
                    {
                        _incomingOrderChannel.NewIncomingOrderReceived -= new NewIncomingOrderEventHandler(IncomingChannel_NewOrderRequestReceived);
                        _incomingOrderChannel.NewOrderAmendmentReceived -= new AmendOrderEventHandler(IncomingChannel_OrderAmendmentRequestReceived);
                        _incomingOrderChannel.NewOrderCancellationReceived -= new CancelOrderEventHandler(IncomingChannel_OrderCancellationRequestReceived);
                        _incomingOrderChannel.Stop();
                        _outgoingOrderChannel.Stop();
                        Thread.Sleep(2000);
                    }
                }
                catch (Exception ex)
                {
                    Trace(LogLevel.Critical, "Exception while stopping MarketDataClient: {0}", ex.Message);
                }
            }
        }

        void IncomingChannel_NewOrderRequestReceived(object sender, IncomingOrder newOrder)
        {
            string message = null;
            if (!_STOPOrderProcessor.ValidateNewOrderRequest(newOrder, ref message))
            {
                Trace(LogLevel.Warning, "Validation failed: {0}", message);
                newOrder.Reject(message);
                return;
            }

            newOrder.Accept();
            StopTrader trader = new StopTrader(newOrder);
            trader.Start();
        }

        void IncomingChannel_OrderCancellationRequestReceived(object sender, IncomingOrder orderToCancel)
        {
            string message = null;
            if (!_STOPOrderProcessor.ValidateCancelOrderRequest(orderToCancel, ref message))
            {
                Trace(LogLevel.Warning, "Validation failed: {0}", message);
                orderToCancel.RejectCancel(message);
                return;
            }

            orderToCancel.RejectCancel("Cancellation not implemented.");
        }

        void IncomingChannel_OrderAmendmentRequestReceived(object sender, IncomingOrder oldOrder, Order newOrder)
        {
            string message = null;
            if (!_STOPOrderProcessor.ValidateAmendOrderRequest(oldOrder, newOrder, ref message))
            {
                Trace(LogLevel.Warning, "Validation failed: {0}", message);
                oldOrder.RejectAmendment(message);
                return;
            }

            oldOrder.RejectAmendment("Amendment not implemented.");
        }        

        private void Pause(string message)
        {
            Console.WriteLine(message);
            Console.ReadLine();
        }
    }
}
