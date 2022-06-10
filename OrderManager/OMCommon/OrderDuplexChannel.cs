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
using System.Collections.Specialized;
using System.Threading;
using System.Messaging;

using OPEX.Common;
using OPEX.Messaging;

namespace OPEX.OM.Common
{
    [Flags]
    public enum OMQueueUserType
    {
        RequestListener = 1,
        ResponseListener = 2
    }

    internal enum OMMode
    {
        Client,
        Server
    }

    internal enum OrderDuplexChannelType
    {
        Incoming,
        Outgoing
    }

    public abstract class OrderDuplexChannel
    {
        protected static readonly int RequestThreadSleepMsec = 1000;
        protected static readonly int ResponseThreadSleepMsec = 1000;        

        private OMMode _omMode;
        protected readonly Logger _logger;
        protected string _name;
        protected OMQueueUserType _type;
        protected IMessageFormatter _formatter;        
        protected Dictionary<string, Channel> _channels;
        protected MessageQueue _requestQueue;
        protected MessageQueue _responseQueue;

        private bool IsRequestListener { get { return (_type & OMQueueUserType.RequestListener) != 0; } }
        private bool IsResponseListener { get { return (_type & OMQueueUserType.ResponseListener) != 0; } }

        protected virtual void OnOrderRequestReceived(Order order) { }
        protected virtual void OnOrderResponseReceived(Order order) { }        

        public OrderDuplexChannel(string name, OMQueueUserType type)
        {
            _omMode = (OMMode)Enum.Parse(typeof(OMMode), Configuration.GetConfigSetting("OMMode", "Client"));
            _name = name;
            _type = type;
            _logger = new Logger(string.Format("OrderDuplexChannel({0})", _name));
            _formatter = new BinaryMessageFormatter();

            InitChannels();
        }      

        public virtual void Start()
        {
            foreach (Channel channel in _channels.Values)
            {
                channel.Start();
            }
        }

        public virtual void Stop()
        {
            foreach (Channel channel in _channels.Values)
            {
                channel.Stop();
            }
        }

        public void Purge()
        {
            try
            {
                _logger.Trace(LogLevel.Info, "Purging queues...");

                foreach (Channel channel in _channels.Values)
                {
                    channel.Queue.Purge();
                }

                _logger.Trace(LogLevel.Info, "Queues purged.");
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "Exception while purging queues: {0}", ex.Message);
            }
        }

        private void ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue q = null;

            try
            {
                q = sender as MessageQueue;

                Message m = q.EndReceive(e.AsyncResult);

                if (!_formatter.CanRead(m))
                {
                    _logger.Trace(LogLevel.Error, "ReceiveCompleted. Cannot read message! The message couldn't be deserialized by the formatter. Skipping.");                    
                }
                else
                {
                    Order order = _formatter.Read(m) as Order;

                    if (order != null)
                    {
                        if (q.Path.Equals(_requestQueue.Path))
                        {
                            OnOrderRequestReceived(order);
                        }
                        else if (q.Path.Equals(_responseQueue.Path))
                        {
                            OnOrderResponseReceived(order);
                        }
                        else
                        {
                            _logger.Trace(LogLevel.Critical, "ReceiveCompleted. Invalid queue path: {0}", q.Path);
                        }
                    }
                    else
                    {
                        _logger.Trace(LogLevel.Critical, "ReceiveCompleted. A null order was received. Skipping.");
                    }
                }
            }
            catch (MessageQueueException mex)
            {
                _logger.Trace(LogLevel.Critical, "ReceiveCompleted. MessageQueueException: {0}", mex.Message);
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "ReceiveCompleted. Exception: {0}", ex.Message);
            }
            finally
            {
                q.BeginReceive();
            }
        }      

        private void InitChannels()
        {
            bool servermode = (_omMode == OMMode.Server);
            _channels = new Dictionary<string, Channel>(2);

            _channels["OMRequestQueueChannel"] =
                new Channel("OMRequestQueueChannel",
                    OMQNames.GetQueueName(_name, OMQType.Request),
                    "OMRequestQueue",
                    IsRequestListener,
                    false,
                    servermode,
                    RequestThreadSleepMsec);
            _channels["OMResponseQueueChannel"] =
                new Channel("OMResponseQueueChannel",
                    OMQNames.GetQueueName(_name, OMQType.Response),
                    "OMResponseQueue",
                    IsResponseListener,
                    false,
                    servermode,
                    ResponseThreadSleepMsec);

            foreach (Channel channel in _channels.Values)
            {
                if (!channel.Init())
                {
                    _logger.Trace(LogLevel.Critical, "Couldn't initialise channel {0}", channel.Name);
                    break;
                }
                else
                {
                    channel.ReceiveCompleted += new ReceiveCompletedEventHandler(ReceiveCompleted);
                }
            }

            _requestQueue = _channels["OMRequestQueueChannel"].Queue;
            _responseQueue = _channels["OMResponseQueueChannel"].Queue;
        }
    }
}
