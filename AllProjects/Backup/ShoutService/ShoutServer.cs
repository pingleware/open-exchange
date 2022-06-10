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
using System.Text;
using System.Messaging;

using OPEX.Common;
using OPEX.Messaging;

namespace OPEX.ShoutService
{
    /// <summary>
    /// Sends shouts to the default multicast distribution channel. 
    /// </summary>
    public class ShoutServer
    {
        #region Static 

        private static readonly object _root = new object();
        private static ShoutServer _theInstance;

        /// <summary>
        /// The default multicast address for ShoutServer.
        /// </summary>
        public static readonly string ChannelDefaultMulticastAddress = "234.222.23.21:2022";

        /// <summary>
        /// Gets the instance of the class OPEX.ShoutService.ShoutServer.
        /// </summary>
        public static ShoutServer Instance
        {
            get
            {
                lock (_root)
                {
                    if (_theInstance == null)
                    {
                        _theInstance = new ShoutServer();
                    }
                }
                return _theInstance;
            }
        }

        #endregion Statics

        private Logger _logger;        
        private bool _running;
        private Channel _broadcastChannel;
        private IMessageFormatter _formatter;

        /// <summary>
        /// Indicates whether ShoutServer is running.
        /// </summary>
        public bool Running { get { return _running; } }

        private ShoutServer()
        {
            _logger = new Logger("ShoutServer");
            _running = false;
            _formatter = new BinaryMessageFormatter();

            _broadcastChannel = new BroadcastChannel<Shout>(
                "ShoutBroadcastQueueChannel",
                null,
                "ShoutBroadcastQueue",
                0,
                ChannelDefaultMulticastAddress,
                BroadcastChannelType.Sender);

            _broadcastChannel.Init();
        }

        /// <summary>
        /// Starts ShoutServer.
        /// </summary>
        public void Start()
        {
            if (_running)
            {
                _logger.TraceAndThrow("Can't start service. Service already running.");
            }            

            _running = true;
        }

        /// <summary>
        /// Stops ShoutServer.
        /// </summary>
        public void Stop()
        {
            if (!_running)
            {
                _logger.TraceAndThrow("Can't stop service. Service isn't running.");
            }

            _running = false;
        }

        /// <summary>
        /// Sends a shout onto the broadcast channel.
        /// </summary>
        /// <param name="s">The shout to send.</param>
        public void BroadcastShout(Shout s)
        {
            if (!_running)
            {
                _logger.TraceAndThrow("BroadcastShout. Can't broadcast message - Server not running!");
            }
           
            try
            {
                _logger.Trace(LogLevel.Debug, "BroadcastShout. About to send a message on queue {0}...", _broadcastChannel.Queue.Path);
                _broadcastChannel.Queue.Send(new Message(s, _formatter));
            }
            catch (MessageQueueException mex)
            {
                _logger.Trace(LogLevel.Critical, "BroadcastShout. MessageQueueException: {0}", mex.Message);
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "BroadcastShout. Exception: {0}", ex.Message);
            }
        }
    }
}
