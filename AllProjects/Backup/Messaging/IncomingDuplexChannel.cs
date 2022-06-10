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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OPEX.Common;

namespace OPEX.Messaging
{
    /// <summary>
    /// Represents an Incoming DuplexChannel.
    /// Owns the Request queue.
    /// This class is abstract.
    /// </summary>
    /// <typeparam name="T">The IChannelMessage sub-class to use as message type.</typeparam>
    public abstract class IncomingDuplexChannel<T> : DuplexChannel<T> where T : IChannelMessage
    {
        /// <summary>
        /// Initialises a new instance of the class OPEX.Messaging.IncomingDuplexChannel.
        /// </summary>
        /// <param name="channelName">The name of the IncomingDuplexChannel.</param>
        public IncomingDuplexChannel(string channelName)
            : base(channelName, DuplexChannelType.Incoming)
        { }

        /// <summary>
        /// Sends the message to the appropriate Response MessageQueue.
        /// </summary>
        /// <param name="messageContent">The message to send.</param>
        public void Respond(T messageContent)
        {
            string key = GetMessageKey(messageContent);
            if (key == null || key.Length == 0)
            {
                _logger.Trace(LogLevel.Critical, "Respond. Message has a NULL key");
                return;
            }

            MessageQueue responseQueue = _responseQueues[key] as MessageQueue;
            _logger.Trace(LogLevel.Debug, "Respond. Queue selected for key {0}: {1}", key, responseQueue.Path);
            if (responseQueue == null)
            {
                _logger.Trace(LogLevel.Critical, "Respond. Message has a NULL responseQueue");
                return;
            }

            Message m = new Message(messageContent, _formatter);
            responseQueue.Send(m);
        }
    }
}

