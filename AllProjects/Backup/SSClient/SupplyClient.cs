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
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;

using OPEX.Common;
using OPEX.Configuration.Client;
using OPEX.SupplyService.Common;

namespace OPEX.SupplyService.Client
{
    /// <summary>
    /// Receives SupplyMessage-s and raises an event
    /// when a SupplyMessage is received.
    /// </summary>
    public class SupplyClient
    {
        private static readonly int RetryDelaySec = 3;
        private static readonly object _root = new object();
        private static SupplyClient _theInstance = null;

        private readonly Logger _logger;
        private readonly HashSet<string> _subscriptionSet;
        private readonly ISupplyMessageBroadcaster _broadCaster;

        private SupplyMessageReceiver _receiver = null;        
        private NewAssignmentReceivedEventHandler _newAssignmentReceived;

        /// <summary>
        /// Gets the instance of the class
        /// OPEX.SupplyService.Client.SupplyClient.
        /// </summary>
        public static SupplyClient Instance
        {
            get
            {
                lock (_root)
                {
                    if (_theInstance == null)
                    {
                        _theInstance = new SupplyClient();
                    }
                }
                return _theInstance;
            }
        }        

        protected SupplyClient()
        {
            string applicationName = ConfigurationClient.Instance.GetConfigSetting("ApplicationName", "OPEXApplication");
            _logger = new Logger(string.Format("SupplyClient({0})", applicationName));
            _subscriptionSet = new HashSet<string>();
            _subscriptionSet.Add(applicationName);

            string channelName = ConfigurationClient.Instance.GetConfigSetting("SSChannelName", "SupplyServiceChannel");
            string serverHost = ConfigurationClient.Instance.GetConfigSetting("SSHost", "localhost");
            int port = Int32.Parse(ConfigurationClient.Instance.GetConfigSetting("SSPort", "12001"));
            string uri = ConfigurationClient.Instance.GetConfigSetting("SSUri", "SupplyService.rem");
            _logger.Trace(LogLevel.Method, "Creating new client. ChannelName: {0} ServerHost {1} Port {2} Uri {3}",
                channelName, serverHost, port, uri);

            BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
            BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
            serverProvider.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = 0;
            props["typeFilterLevel"] = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
            TcpChannel chan = new TcpChannel(props, clientProvider, serverProvider);
            ChannelServices.RegisterChannel(chan, false);

            string serverUrl = string.Format("tcp://{0}:{1}/{2}", serverHost, port, uri);
            _logger.Trace(LogLevel.Info, "ServerUrl: {0}", serverUrl);

            _logger.Trace(LogLevel.Info, "Creating remote object...");

            _broadCaster = Activator.GetObject(
                typeof(ISupplyMessageBroadcaster),
                serverUrl) as ISupplyMessageBroadcaster;

            _logger.Trace(LogLevel.Info, "Remote object created.");
        }

        /// <summary>
        /// Subscribes to the SupplyMessages of a specific application.
        /// </summary>
        /// <param name="applicationName">The name of the application for
        /// which to subscribe to SupplyMessage-s.</param>
        public void Subscribe(string applicationName)
        {            
            _subscriptionSet.Add(applicationName);
            _logger.Trace(LogLevel.Info, "Application {0} added to subscription list", applicationName);
        }

        /// <summary>
        /// Starts SupplyClient.
        /// </summary>
        public void Start()
        {
            bool allWell = false;

            _logger.Trace(LogLevel.Info, "Starting client...");

            _receiver = new SupplyMessageReceiver();
            _receiver.SupplyMessageReceived += new SupplyMessageArrivedHandler(SupplyMessageReceived);

            while (!allWell)
            {
                try
                {
                    _broadCaster.MessageArrived += new SupplyMessageArrivedHandler(_receiver.SupplyMessageArrived);
                    allWell = true;
                }
                catch (SocketException sex)
                {
                    _logger.Trace(LogLevel.Warning, "SocketException while connecting to SS: {0}", sex.Message);
                    _logger.Trace(LogLevel.Info, "Retrying in {0} seconds...", RetryDelaySec);
                    System.Threading.Thread.Sleep(RetryDelaySec * 1000);
                }
                catch (Exception ex)
                {
                    _logger.TraceAndThrow("Exception while Starting SupplyClient: {0}", ex.Message);
                }
            }

            _logger.Trace(LogLevel.Info, "Client started.");
        }

        /// <summary>
        /// Stops SupplyClient.
        /// </summary>
        public void Stop()
        {
            try
            {
                _receiver.SupplyMessageReceived -= new SupplyMessageArrivedHandler(SupplyMessageReceived);
                _broadCaster.MessageArrived -= new SupplyMessageArrivedHandler(_receiver.SupplyMessageArrived);                             
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "Exception while stopping SupplyClient: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Occurs when a new Assignment is received.
        /// </summary>
        public event NewAssignmentReceivedEventHandler NewAssignmentReceived
        {
            add { _newAssignmentReceived += value; }
            remove { _newAssignmentReceived -= value; }
        }

        private void SupplyMessageReceived(SupplyMessage msg)
        {
            if (msg == null || msg.Assignment == null)
            {
                return;
            }

            Assignment assignment = msg.Assignment;            

            if (!_subscriptionSet.Contains(assignment.ApplicationName))
            {
                _logger.Trace(LogLevel.Debug, "Assignment belongs to application {0}, skipping. {1}", assignment.ApplicationName, assignment.ToString());
                return;
            }

            _logger.Trace(LogLevel.Info, "New assignment received: {0}", assignment.ToString());

            if (_newAssignmentReceived != null)
            {
                foreach (NewAssignmentReceivedEventHandler handler in _newAssignmentReceived.GetInvocationList())
                {
                    handler(this, msg.Assignment);
                }
            }
        }
    }
}
