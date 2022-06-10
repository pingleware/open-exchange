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
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OPEX.Common;

namespace OPEX.Messaging
{   
    /// <summary>
    /// Specifies the type of the DuplexChannel
    /// </summary>
    public enum DuplexChannelType
    {
        /// <summary>
        /// Incoming DuplexChannel: owns the Request queue.
        /// Picks up the Response queue from the System.
        /// </summary>
        Incoming,

        /// <summary>
        /// Outgoing DuplexChannel: owns the Response queue.
        /// Picks up the Request queue from the System.
        /// </summary>
        Outgoing
    }

    /// <summary>
    /// Represents a bidirectional logical communication channel,
    /// that is a Request/Response pair of Channels.
    /// This class is abstract.
    /// </summary>
    /// <typeparam name="T">The IChannelMessage sub-class to use as message type.</typeparam>
    public abstract class DuplexChannel<T> where T : IChannelMessage
    {
        protected static readonly int RequestThreadSleepMsec = 1000;
        protected static readonly int ResponseThreadSleepMsec = 1000;
        
        protected readonly Logger _logger;
        protected string _name;
        protected DuplexChannelType _type;
        protected IMessageFormatter _formatter;
        protected Channel _ownChannel;
        protected Dictionary<string, MessageQueue> _responseQueues;

        protected abstract void OnMessageReceived(T channelMessage);

        /// <summary>
        /// Initialises a new instance of the class OPEX.Messaging.DuplexChannel.
        /// </summary>
        /// <param name="name">The name of the DuplexChannel.</param>
        /// <param name="type">The type of the DuplexChannel.</param>
        public DuplexChannel(string name, DuplexChannelType type)
        {
            _name = name;
            _type = type;
            _logger = new Logger(string.Format("DuplexChannel({0})", _name));
            _formatter = new BinaryMessageFormatter();
            _responseQueues = new Dictionary<string, MessageQueue>();

            InitChannel();
        }

        /// <summary>
        /// Starts the DuplexChannel.
        /// </summary>
        public virtual void Start()
        {
            _ownChannel.Start();
        }

        /// <summary>
        /// Starts the DuplexChannel.
        /// </summary>
        public virtual void Stop()
        {
            _ownChannel.Stop();
        }

        /// <summary>
        /// Purges the content of the owned MessageQueue.
        /// </summary>
        public void Purge()
        {
            try
            {
                _logger.Trace(LogLevel.Info, "Purging queue...");

                _ownChannel.Queue.Purge();

                _logger.Trace(LogLevel.Info, "Queue purged.");
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "Exception while purging queue: {0}", ex.Message);
            }
        }

        private void ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue q = sender as MessageQueue;
            Message m = q.EndReceive(e.AsyncResult);

            if (!_formatter.CanRead(m))
            {
                _logger.Trace(LogLevel.Error, "ReceiveCompleted. Cannot read message! The message couldn't be deserialized by the formatter. Skipping.");
            }
            else
            {
                T messageContent = (T)_formatter.Read(m);

                if (messageContent != null)
                {
                    if (_type == DuplexChannelType.Incoming)
                    {
                        CacheResponseQueue(messageContent, m.ResponseQueue);
                    }
                    OnMessageReceived(messageContent);
                }
                else
                {
                    _logger.Trace(LogLevel.Critical, "ReceiveCompleted. A null object was received. Skipping.");
                }
            }            
        }       

        private void InitChannel()
        {
            ChannelType chType = (_type == DuplexChannelType.Incoming) ? ChannelType.Request : ChannelType.Response;
            int threadSleepMsec = (_type == DuplexChannelType.Incoming) ? RequestThreadSleepMsec : ResponseThreadSleepMsec;

            _ownChannel = new Channel(string.Format("{0}QueueChannel", chType.ToString()),
                    QueueHelper.GetQueueName(_name, _type, chType),
                    string.Format("{0}Queue", chType.ToString()),
                    true,
                    threadSleepMsec);

            if (!_ownChannel.Init())
            {
                _logger.Trace(LogLevel.Critical, "Couldn't initialise channel {0}", _ownChannel.Name);
            }
            else
            {               
                _ownChannel.ReceiveCompleted += new ReceiveCompletedEventHandler(ReceiveCompleted);
            }
        }

        protected string GetMessageKey(IChannelMessage messageContent)
        {
            string key = null;
            if (this._type == DuplexChannelType.Incoming)
            {
                key = messageContent.Origin;
            }
            else
            {
                key = messageContent.Destination;
            }
            return key;
        }

        private void CacheResponseQueue(IChannelMessage messageContent, MessageQueue responseQueue)
        {
            if (responseQueue == null)
            {
                _logger.Trace(LogLevel.Critical, "CacheResponseQueue. Cannot cache NULL MessageQueue");
                return;
            }
            
            string key = GetMessageKey(messageContent);
            if (key == null || key.Length == 0)
            {
                _logger.Trace(LogLevel.Critical, "CacheResponseQueue. Message has a NULL key");
                return;
            }

            if (!_responseQueues.ContainsKey(key))
            {
                _responseQueues.Add(key, responseQueue);
                _logger.Trace(LogLevel.Debug, "CacheResponseQueue. messageContent: Origin {0} Destination {1} Key {2} Path {3}.",
                messageContent.Origin, messageContent.Destination, key, responseQueue.Path);
            }
        }
    }
}
