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
using OPEX.AS.Service;

namespace OPEX.DWEAS.Client
{
    public delegate void NewAssignmentBatchReceivedEventHandler(object sender, AssignmentBatch assignmentBatch, bool newSimulationStarted);

    public class DWEAssignmentClient
    {
        private static readonly object _root = new object();
        private static DWEAssignmentClient _theInstance = null;

        private static readonly int BroadcastThreadSleepMsec = 1000;
        private readonly Logger _logger;
        private readonly Channel _broadCastChannel;
        private readonly IMessageFormatter _formatter;
        private readonly HashSet<string> _subscriptionSet;

        public static DWEAssignmentClient Instance
        {
            get
            {
                lock (_root)
                {
                    if (_theInstance == null)
                    {
                        _theInstance = new DWEAssignmentClient();
                    }
                }
                return _theInstance;
            }
        }

        private DWEAssignmentClient()
        {            
            string applicationName = ConfigurationClient.Instance.GetConfigSetting("ApplicationName", "OPEXApplication");
            _logger = new Logger(string.Format("DWEAssignmentClient({0})", applicationName));
            _subscriptionSet = new HashSet<string>();
            _subscriptionSet.Add(applicationName);

            _formatter = new BinaryMessageFormatter();
            _broadCastChannel = new BroadcastChannel<AssignmentMessage>(
               "DWEASBroadcastQueueChannel",
               QueueHelper.GetQueueName(string.Format("DWEAS_{0}", ConfigurationClient.Instance.GetConfigSetting("ApplicationName", null)), DuplexChannelType.Incoming, ChannelType.Broadcast),
               "DWEASBroadcastQueue",
               BroadcastThreadSleepMsec,
               AssignmentMessage.DWEMulticastAddress,
               BroadcastChannelType.Receiver);

            _broadCastChannel.Init();
            _broadCastChannel.ReceiveCompleted += new ReceiveCompletedEventHandler(BroadCastChannel_ReceiveCompleted);
        }

        private bool _hasStarted = false;
        public void Start()
        {
            lock (_root)
            {
                if (_hasStarted)
                {
                    _logger.Trace(LogLevel.Critical, "Start. Cannot start DWEAssignmentClient: client is already running.");
                    return;
                }                

                _broadCastChannel.Start();

                _hasStarted = true;
                _logger.Trace(LogLevel.Method, "Start. DWEAssignmentClient STARTED.");
            }
        }

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
                _logger.Trace(LogLevel.Method, "Stop. DWEAssignmentClient STOPPED.");
            }
        }

        public void Subscribe(string applicationName)
        {
            _subscriptionSet.Add(applicationName);
            _logger.Trace(LogLevel.Info, "Subscribe. Application {0} added to subscription list", applicationName);
        }

        public event NewAssignmentBatchReceivedEventHandler NewAssignmentBatchReceived
        {
            add { _newAssignmentBatchReceived += value; }
            remove { _newAssignmentBatchReceived -= value; }
        }
        private NewAssignmentBatchReceivedEventHandler _newAssignmentBatchReceived;

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
            
            if (DateTime.Now.CompareTo(assignmentMessage.Expiry) >= 0)
            {
                _logger.Trace(LogLevel.Warning, "OnNewAssignmentBatchReceived. Discarding message because it has expired.");
                return;
            }

            AssignmentBatch assignmentBatch = assignmentMessage.AssignmentBatch;

            if (!assignmentMessage.NewSimulationStarted)
            {
                if (assignmentBatch == null)
                {
                    _logger.Trace(LogLevel.Error, "OnNewAssignmentBatchReceived. NULL assignmentBatch!");
                    return;
                }

                if (!_subscriptionSet.Contains(assignmentBatch.ApplicationName))
                {
                    _logger.Trace(LogLevel.Debug, "OnNewAssignmentBatchReceived. AssignmentBatch belongs to application {0}, skipping. {1}",
                        assignmentBatch.ApplicationName, assignmentBatch.ToString());
                    return;
                }

                _logger.Trace(LogLevel.Info, "OnNewAssignmentBatchReceived. New assignment batch received, INVOKING HANDLER.");
            }

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
