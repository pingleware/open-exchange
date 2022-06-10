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
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OPEX.Common;

namespace OPEX.Messaging
{
    /// <summary>
    /// Represents an Outgoing DuplexChannel.
    /// Owns the Response queue.
    /// This class is abstract.
    /// </summary>
    /// <typeparam name="T">The IChannelMessage sub-class to use as message type.</typeparam>
    public abstract class OutgoingDuplexChannel<T> : DuplexChannel<T> where T : IChannelMessage
    {
        private readonly object _sendRoot = new object();
        protected Dictionary<string, Channel> _requestChannels;

        /// <summary>
        /// Initialises a new instance of the class OPEX.Messaging.OutgoingDuplexChannel.
        /// </summary>
        /// <param name="channelName">The name of the OutgoingDuplexChannel.</param>
        public OutgoingDuplexChannel(string channelName)
            : base (channelName, DuplexChannelType.Outgoing)
        {
            _requestChannels = new Dictionary<string, Channel>();
        }       

        protected virtual void SendMessage(T messageContent)
        {
            lock (_sendRoot)
            {
                Channel requestChannel = GetRequestChannel(messageContent.Destination);
                if (requestChannel == null)
                {
                    _logger.Trace(LogLevel.Critical, "SendMessage. Couldn't find request channel associated to Destination {0}. Message can't be sent.", messageContent.Destination);
                    return;
                }

                MessageQueue requestQueue = requestChannel.Queue;
                if (requestQueue == null)
                {
                    _logger.Trace(LogLevel.Critical, "SendMessage. Channel {0} has a NULL requestQueue", requestChannel.Name);
                    return;
                }
                Message m = new Message(messageContent, requestQueue.Formatter);
                m.ResponseQueue = _ownChannel.Queue;
                //_logger.Trace(LogLevel.Debug, "SendMessage. Sending message on queue {0} response {1}.", requestQueue.Path, _ownChannel.Queue.Path); 
                requestQueue.Send(m);
            }
        }

        protected Channel GetRequestChannel(string destination)
        {
            string key = string.Empty;

            if (_requestChannels.ContainsKey("*"))
            {
                key = "*";
            }
            else
            {
                key = destination;
            }

            if (_requestChannels.ContainsKey(key))
            {
                return _requestChannels[key] as Channel;
            }

            return null;            
        }      

        protected void RegisterDefaultChannel(string destination, string machineName)
        {
            RegisterChannel(destination, destination, machineName, true);
        }

        protected void RegisterChannel(string destination, string machineName)
        {
            RegisterChannel(destination, destination, machineName, false);
        }

        protected void RegisterChannel(string destination, string machineName, string destinationQueueName)
        {
            RegisterChannel(destination, destinationQueueName, machineName, false);
        }

        private void RegisterChannel(string destination, string destinationQueueName, string machineName, bool isDefault)
        {
            if (_requestChannels.ContainsKey(destination))
            {
                _logger.Trace(LogLevel.Warning, "RegisterChannel. Destination {0} already registered", destination);
                return;
            }

            ChannelType chType = ChannelType.Request;
            DuplexChannelType dchType = DuplexChannelType.Incoming;
            Channel channel = new Channel(
                    string.Format("{0}QueueChannel", chType.ToString()),
                    QueueHelper.GetQueueName(destinationQueueName, dchType, chType, machineName),
                    string.Format("{0}Queue", chType.ToString()),
                    false,
                    0);
            channel.Init();

            string key = destination;
            if (isDefault)
            {
                key = "*";                
            }
            _requestChannels[key] = channel;
        }
    }
}
