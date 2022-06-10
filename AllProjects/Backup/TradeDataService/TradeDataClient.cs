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
using System.Messaging;

using OPEX.Common;
using OPEX.Messaging;
using OPEX.TDS.Common;
using OPEX.TDS.Server;
using OPEX.Configuration.Client;

namespace OPEX.TDS.Client
{
    /// <summary>
    /// Listens to the default TradeDataServer channel, and
    /// raises events when new TradeDataMessages are available.
    /// </summary>
    public class TradeDataClient
    {
        private static readonly int BroadcastThreadSleepMsec = 1000;
        private static object _staticRoot = new object();
        private static TradeDataClient _theInstance;

        private object _root;
        private Channel _broadCastChannel;
        private IMessageFormatter _formatter;
        private Logger _logger;
        private TradeMessageReceivedEventHandler _tradeMessageReceived;

        /// <summary>
        /// Gets the instance of the class OPEX.TDS.Client.TradeDataClient.
        /// </summary>
        public static TradeDataClient Instance
        {
            get
            {
                lock (_staticRoot)
                {
                    if (_theInstance == null)
                    {
                        _theInstance = new TradeDataClient();
                    }
                }
                return _theInstance;
            }
        }

        private TradeDataClient()
        {
            _root = new object();
            _formatter = new BinaryMessageFormatter();
            _logger = new Logger("TradeDataClient");
            
            Init();
        }

        #region Methods

        /// <summary>
        /// Starts TradeDataClient.
        /// </summary>
        public void Start()
        {
            _broadCastChannel.Start();
        }

        /// <summary>
        /// Stops TradeDataClient.
        /// </summary>
        public void Stop()
        {
            _broadCastChannel.Stop();
        }

        #endregion Methods

        /// <summary>
        /// Occurs when a new TradeDataMessage is received.
        /// </summary>
        public event TradeMessageReceivedEventHandler TradeMessageReceived
        {
            add { _tradeMessageReceived += value; }
            remove { _tradeMessageReceived -= value; }
        }

        private void BroadCastChannel_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue q = sender as MessageQueue;
            Message m = q.EndReceive(e.AsyncResult);

            if (!_formatter.CanRead(m))
            {
                _logger.Trace(LogLevel.Error, "BroadcastChannel_ReceiveCompleted. Cannot read message! The message couldn't be deserialized by the formatter. Skipping.");
            }
            else
            {
                TradeDataMessage message = _formatter.Read(m) as TradeDataMessage;

                if (message != null)
                {
                    OnTradeMessageReceived(message);
                }
                else
                {
                    _logger.Trace(LogLevel.Critical, "BroadcastChannel_ReceiveCompleted. A null object was received. Skipping.");
                }
            }
        }

        private void OnTradeMessageReceived(TradeDataMessage tradeDataMessage)
        {
            if (_tradeMessageReceived != null)
            {
                foreach (TradeMessageReceivedEventHandler tradeMessageReceivedEventHandler in _tradeMessageReceived.GetInvocationList())
                {
                    tradeMessageReceivedEventHandler(this, tradeDataMessage);
                }
            }
        }

        private void Init()
        {
            _broadCastChannel = new BroadcastChannel<TradeDataMessage>(
                "TDSBroadcastQueueChannel",
                QueueHelper.GetQueueName(string.Format("TDS_{0}", ConfigurationClient.Instance.GetConfigSetting("ApplicationName", null)), DuplexChannelType.Incoming, ChannelType.Broadcast),
                "TDSBroadcastQueue",
                BroadcastThreadSleepMsec,
                TradeDataServer.DefaultMulticastAddress,    
                BroadcastChannelType.Receiver);
            
            _broadCastChannel.Init();
            _broadCastChannel.ReceiveCompleted += new ReceiveCompletedEventHandler(BroadCastChannel_ReceiveCompleted);
        }
    }

    /// <summary>
    /// Represents the method that will handle an event
    /// that has TradeDataMessage arguments.
    /// </summary>
    /// <param name="sender">The sender of this event.</param>
    /// <param name="tradeDataMessage">The TradeDataMessage associated to this event.</param>
    public delegate void TradeMessageReceivedEventHandler(object sender, TradeDataMessage tradeDataMessage);
}
