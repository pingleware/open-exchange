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
using System.Messaging;

using OPEX.Common;
using OPEX.Messaging;
using OPEX.TDS.Common;
using OPEX.OM.Common;

namespace OPEX.TDS.Server
{
    /// <summary>
    /// Sends TradeDataMessages to the default
    /// broadcast distribution channel.
    /// </summary>
    public class TradeDataServer
    {
        #region Static

        private static readonly string ChannelDefaultMulticastAddress = "234.222.23.21:2021";
        private static readonly object _root = new object();
        private static TradeDataServer _theInstance;

        /// <summary>
        /// Gets the default multicast address for the TradeDataServer.
        /// </summary>
        public static string DefaultMulticastAddress { get { return ChannelDefaultMulticastAddress; } }

        /// <summary>
        /// Gets the instance of the class OPEX.TDS.Server.TradeDataServer.
        /// </summary>
        public static TradeDataServer Instance
        {
            get
            {
                lock (_root)
                {
                    if (_theInstance == null)
                    {
                        _theInstance = new TradeDataServer();
                    }
                }
                return _theInstance;
            }
        }

        #endregion        

        private readonly Logger _tdsLogger;
        private readonly Channel _broadcastChannel;
        private readonly IMessageFormatter _formatter;

        private TradeDataServer()
        {
            _tdsLogger = new Logger("TradeDataServer");
            
            _formatter = new BinaryMessageFormatter();
            _broadcastChannel = new BroadcastChannel<TradeDataMessage>(
                "TDSBroadcastQueueChannel",
                null,
                "TDSBroadcastQueue",
                0,
                ChannelDefaultMulticastAddress,
                BroadcastChannelType.Sender);
            
            _broadcastChannel.Init();
        }

        /// <summary>
        /// Starts TradeDataServer.
        /// </summary>
        public void Start() { _broadcastChannel.Start(); }
        
        /// <summary>
        /// Stops TradeDataServer.
        /// </summary>
        public void Stop() { _broadcastChannel.Stop();  }

        /// <summary>
        /// Sends a TradeDataMessage onto the broadcast channel.
        /// </summary>
        /// <param name="tradeDataMessage">The TradeDataMessage to send.</param>
        public void BroadcastNewTradeData(TradeDataMessage tradeDataMessage)
        {
            try
            {
                _tdsLogger.Trace(LogLevel.Info, "About to send a message on queue {0}...", _broadcastChannel.Queue.Path);
                _broadcastChannel.Queue.Send(new Message(tradeDataMessage, _formatter));
            }
            catch (MessageQueueException mex)
            {
                _tdsLogger.Trace(LogLevel.Critical, "BroadcastNewTradeData. MessageQueueException: {0}", mex.Message);
            }
            catch (Exception ex)
            {
                _tdsLogger.Trace(LogLevel.Critical, "BroadcastNewTradeData. Exception: {0}", ex.Message);
            }       
        }
    }
}
