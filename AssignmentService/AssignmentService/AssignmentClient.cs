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
using System.Text;
using System.Messaging;

using OPEX.Common;
using OPEX.SupplyService.Common;
using OPEX.Messaging;
using OPEX.Configuration.Client;

namespace OPEX.AS.Service
{
    /// <summary>
    /// Represents the method that will handle an event
    /// that occurs when a new AssignmentMessage is received.
    /// </summary>
    /// <param name="sender">The sender of this event.</param>
    /// <param name="assignmentBatch">The AssignmentBatch just received.</param>
    /// <param name="newSimulationStarted">Indicates whether this is the first AssignmentMessage received after a new simulation started.</param>
    public delegate void NewAssignmentBatchReceivedEventHandler(object sender, AssignmentBatch assignmentBatch, bool newSimulationStarted);    

    /// <summary>
    /// Dispatches AssignmentMessages received on the
    /// default multicast channel.
    /// </summary>
    public class AssignmentClient
    {
        private static readonly int BroadcastThreadSleepMsec = 1000;
        private static readonly object _root = new object();
        private static AssignmentClient _theInstance = null;

        private readonly Logger _logger;
        private readonly Channel _broadCastChannel;
        private readonly IMessageFormatter _formatter;
        private readonly HashSet<string> _subscriptionSet;

        private NewAssignmentBatchReceivedEventHandler _newAssignmentBatchReceived;
        private bool _hasStarted = false;

        /// <summary>
        /// Gets the instance of the class OPEX.AS.Service.AssignmentClient.
        /// </summary>
        public static AssignmentClient Instance
        {
            get
            {
                lock (_root)
                {
                    if (_theInstance == null)
                    {
                        _theInstance = new AssignmentClient();
                    }
                }
                return _theInstance;
            }
        }

        private AssignmentClient()
        {            
            string applicationName = ConfigurationClient.Instance.GetConfigSetting("ApplicationName", "OPEXApplication");
            _logger = new Logger(string.Format("AssignmentClient({0})", applicationName));
            _subscriptionSet = new HashSet<string>();
            _subscriptionSet.Add(applicationName);

            _formatter = new BinaryMessageFormatter();
            _broadCastChannel = new BroadcastChannel<AssignmentMessage>(
               "ASBroadcastQueueChannel",
               QueueHelper.GetQueueName(string.Format("AS_{0}", ConfigurationClient.Instance.GetConfigSetting("ApplicationName", null)), DuplexChannelType.Incoming, ChannelType.Broadcast),
               "ASBroadcastQueue",
               BroadcastThreadSleepMsec,
               AssignmentMessage.DefaultMulticastAddress,
               BroadcastChannelType.Receiver);

            _broadCastChannel.Init();
            _broadCastChannel.ReceiveCompleted += new ReceiveCompletedEventHandler(BroadCastChannel_ReceiveCompleted);
        }

        /// <summary>
        /// Starts the channel.
        /// </summary>
        public void Start()
        {
            lock (_root)
            {
                if (_hasStarted)
                {
                    _logger.Trace(LogLevel.Critical, "Start. Cannot start client: client is already running.");
                    return;
                }

                _broadCastChannel.Start();

                _hasStarted = true;
                _logger.Trace(LogLevel.Method, "Start. AssignmentClient STARTED.");
            }
        }

        /// <summary>
        /// Stops the channel.
        /// </summary>
        public void Stop()
        {
            lock (_root)
            {
                if (!_hasStarted)
                {
                    _logger.Trace(LogLevel.Critical, "Stop. Cannot stop client: client hasn't been started yet.");
                    return;
                }

                _broadCastChannel.Stop();

                _hasStarted = false;
                _logger.Trace(LogLevel.Method, "Stop. AssignmentClient STOPPED.");
            }
        }

        /// <summary>
        /// Subscribes to AssignmentMessages of a specific
        /// application name.
        /// </summary>
        /// <param name="applicationName">The name of the application
        /// that owns the AssignmentMessages to subscribe to.</param>
        public void Subscribe(string applicationName)
        {
            _subscriptionSet.Add(applicationName);
            _logger.Trace(LogLevel.Info, "Subscribe. Application {0} added to subscription list", applicationName);
        }

        /// <summary>
        /// Occurs when a new AssignmentMessage is received.
        /// </summary>
        public event NewAssignmentBatchReceivedEventHandler NewAssignmentBatchReceived
        {
            add { _newAssignmentBatchReceived += value; }
            remove { _newAssignmentBatchReceived -= value; }
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
                AssignmentMessage message = _formatter.Read(m) as AssignmentMessage;

                if (message != null)
                {
                    OnNewAssignmentBatchReceived(message);
                }
                else
                {
                    _logger.Trace(LogLevel.Critical, "BroadcastChannel_ReceiveCompleted. A null object was received. Skipping.");
                }
            }
        }

        private void OnNewAssignmentBatchReceived(AssignmentMessage assignmentMessage)
        {
            _logger.Trace(LogLevel.Debug, "OnNewAssignmentBatchReceived. assignmentMessage: {0}", assignmentMessage.ToString());

            AssignmentBatch assignmentBatch = assignmentMessage.AssignmentBatch;

            if (!_subscriptionSet.Contains(assignmentBatch.ApplicationName))
            {
                _logger.Trace(LogLevel.Debug, "OnNewAssignmentBatchReceived. AssignmentBatch belongs to application {0}, skipping. {1}",
                    assignmentBatch.ApplicationName, assignmentBatch.ToString());
                return;
            }

            if (DateTime.Now.CompareTo(assignmentMessage.Expiry) >= 0)
            {
                _logger.Trace(LogLevel.Warning, "OnNewAssignmentBatchReceived. Discarding message because it has expired.");
                return;
            }

            _logger.Trace(LogLevel.Info, "OnNewAssignmentBatchReceived. New assignment batch received, INVOKING HANDLER.");

            if (_newAssignmentBatchReceived != null)
            {
                foreach (NewAssignmentBatchReceivedEventHandler handler in _newAssignmentBatchReceived.GetInvocationList())
                {
                    handler(this, assignmentBatch, assignmentMessage.NewSimulationStarted);
                }
            }
        }              
    }
}
