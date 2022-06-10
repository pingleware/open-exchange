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
using OPEX.Configuration.Client;

namespace OPEX.Messaging
{
    /// <summary>
    /// Specifies the type of BroadcastChannel
    /// </summary>
    public enum BroadcastChannelType
    { 
        /// <summary>
        /// Broadcast data.
        /// </summary>
        Sender,

        /// <summary>
        /// Receive broadcast data.
        /// </summary>
        Receiver
    }

    /// <summary>
    /// Represents a typed unidirectional logical broadcast channel.
    /// </summary>
    /// <typeparam name="T">The IChannelMessage sub-class to use as message type.</typeparam>
    public class BroadcastChannel<T> : Channel where T : IChannelMessage
    {
        protected static readonly string ChannelDefaultMulticastAddress = "234.222.23.21:2020";

        /// <summary>
        /// Gets the default multicast address.
        /// </summary>
        public static string DefaultMulticastAddress { get { return ChannelDefaultMulticastAddress; } }

        protected BroadcastChannelType _type;
        protected string _multicastAddress;

        /// <summary>
        /// Initialises a new instance of the class OPEX.Messaging.BroadcastChannel.
        /// </summary>
        /// <param name="channelName">The name of the BroadcastChannel.</param>
        /// <param name="queuePath">The path of the MessageQueue.</param>
        /// <param name="queueName">The name of the MessageQueue.</param>
        /// <param name="threadSleepMsec">The period, in milliseconds, with which
        /// the main loop checks whether the BroadcastChannel has been stopped.</param>
        /// <param name="multicastAddress">The multicast address of the BroadcastChannel, expressed as "x.y.z.w:p".</param>
        /// <param name="type">The type of the BroadcastChannel.</param>
        public BroadcastChannel(string channelName, string queuePath, string queueName, int threadSleepMsec, string multicastAddress, BroadcastChannelType type)
            : base(channelName, queuePath, queueName, (type == BroadcastChannelType.Receiver), threadSleepMsec)
        {
            _type = type;
            _multicastAddress = multicastAddress;
            _logger.LogTitle = string.Format("BroadcastChannel({0})", channelName);
            if (_type == BroadcastChannelType.Sender)
            {
                _queuePath = string.Format("FormatName:MULTICAST={0}", _multicastAddress);
            }
            else
            {
                _queuePath = string.Format("{0}_{1}", _queuePath, ConfigurationClient.Instance.GetConfigSetting("ApplicationName", null));
            }
            _logger.Trace(LogLevel.Info, " BroadcastChannelType={0}. QueuePath overwritten to {1}.", _type.ToString(), _queuePath);
        }

        protected override bool InitQueue()
        {
            bool success = false;

            try
            {
                _logger.Trace(LogLevel.Info, "BroadcastChannelType: {0}.", _type.ToString());

                if (_type == BroadcastChannelType.Receiver)
                {
                    if (!MessageQueue.Exists(_queuePath))
                    {
                        _logger.Trace(LogLevel.Info, "{0} doesn't exist. Creating new queue: {1}", _queueName, _queuePath);
                        _queue = MessageQueue.Create(_queuePath, false);
                        _queue.SetPermissions(new MessageQueueAccessControlEntry(
                            new WellKnownTrustee(WELL_KNOWN_SID_TYPE.WinWorldSid).Trustee,
                            MessageQueueAccessRights.FullControl));
                        _queue.SetPermissions(new MessageQueueAccessControlEntry(
                            new WellKnownTrustee(WELL_KNOWN_SID_TYPE.WinAnonymousSid).Trustee,
                            MessageQueueAccessRights.FullControl));
                        _logger.Trace(LogLevel.Debug, "Setting MulticastAddress: {0}", _multicastAddress);
                    }
                    else
                    {
                        _queue = new MessageQueue(_queuePath);
                    }
                    _queue.MulticastAddress = _multicastAddress;
                    _queue.MaximumQueueSize = MessageQueue.InfiniteQueueSize;

                    _logger.Trace(LogLevel.Debug, "Hooking ReceivedCompleted event to queue {0}", _queue.Path);
                    _queue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnReceiveCompleted);
                }
                else if (_type == BroadcastChannelType.Sender)
                {
                    _queue = new MessageQueue(_queuePath);
                }
                else
                {
                    _logger.TraceAndThrow("Unknown BroadcastChannelType: {0}", _type.ToString());
                }

                _queue.Formatter = new BinaryMessageFormatter();

                success = true;
            }
            catch (MessageQueueException mex)
            {
                _logger.Trace(LogLevel.Critical, "Message Queue Exception while trying to start {1}: {0}", mex.Message, _queueName);
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "Exception while trying to start {1}: {0}", ex.Message, _queueName);
            }

            return success;
        }
    }
}
