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

using OPEX.MDS.Common;
using OPEX.Common;
using OPEX.Messaging;
using OPEX.Configuration.Client;
using OPEX.ShoutService;
using OPEX.Storage;

namespace OPEX.MDS
{
    /// <summary>
    /// Specifies the status of
    /// MarketDataService.
    /// </summary>
    public enum MDSStatus
    {
        /// <summary>
        /// MarketDataService is not running.
        /// </summary>
        Idle,

        /// <summary>
        /// MarketDataService is running.
        /// </summary>
        Running,

        /// <summary>
        /// MarketDataService is in error.
        /// </summary>
        Error
    }

    /// <summary>
    /// Sends MarketDataMessages, both point to point
    /// and broadcast.
    /// </summary>
    public class MarketDataService : IncomingDuplexChannel<MarketDataMessage>
    {
        #region Statics

        private static Dictionary<string, MarketDataService> Services = null;

        /// <summary>
        /// Checks whether a specific data source exists.
        /// </summary>
        /// <param name="dataSource">The data source to check.</param>
        /// <returns>Trus if the specified data source exists. False otherwise.</returns>
        public static bool Exist(string dataSource)
        {
            return Services.ContainsKey(dataSource);
        }

        /// <summary>
        /// Gets the MarketDataService associated to a specific data source.
        /// </summary>
        /// <param name="dataSource">The data source for which to retrieve the associated MarketDataService.</param>
        /// <returns>The MarketDataService associated to a specific data source.</returns>
        public static MarketDataService Get(string dataSource)
        {
            return Services[dataSource] as MarketDataService;
        }

        /// <summary>
        /// Creates the MarketDataService associated to a specific data source.
        /// </summary>
        /// <param name="dataSource">The data source for which to create the MarketDataService.</param>
        /// <returns>The MarketDataService created for the specified data source.</returns>
        public static MarketDataService Create(string dataSource)
        {
            MarketDataService service = null;

            if (!Services.ContainsKey(dataSource))
            {
                MarketDataService mds = new MarketDataService(dataSource);
                bool purge = bool.Parse(ConfigurationClient.Instance.GetConfigSetting("PurgeQueuesOnStartup", "true"));
                if (purge)
                {
                    mds.Purge();
                }
                Services[dataSource] = mds;
            }
            service = Services[dataSource] as MarketDataService;

            return service;
        }

        static MarketDataService()
        {
            Services = new Dictionary<string, MarketDataService>();
        }

        #endregion        

        private readonly Logger _mdsLogger;
        private readonly string _dataSource;
        private readonly DepthCollection _depthCollection;
        private readonly SnapshotCollection _cachedSnapshotCollection;
        private readonly Channel _broadcastChannel;

        private SessionState _currentState = SessionState.Close;
        private DateTime _currentStartTime = DateTime.Now;
        private DateTime _currentEndTime = DateTime.MaxValue;
        private MDSStatus _status;

        /// <summary>
        /// Gets the status of MarketDataService.
        /// </summary>
        public MDSStatus Status { get { return _status; } }

        /// <summary>
        /// Gets the collection of depths underlying MarketDataService.
        /// </summary>
        public DepthCollection Depths { get { return _depthCollection; } }
     
        private MarketDataService(string dataSource)
            : base(string.Concat("MDS", "_", dataSource))
        {
            _dataSource = dataSource;
            _depthCollection = new DepthCollection();
            _cachedSnapshotCollection = new SnapshotCollection();
            _status = MDSStatus.Idle;
            _mdsLogger = new Logger(string.Format("MarketDataService({0})", dataSource));

            _broadcastChannel = new BroadcastChannel<MarketDataMessage>(
                string.Format("MDSBroadcastQueueChannel({0})", _dataSource),
                null,
                string.Format("MDSBroadcastQueue({0})", _dataSource),
                0,
                BroadcastChannel<MarketDataMessage>.DefaultMulticastAddress,
                BroadcastChannelType.Sender);
            
            _broadcastChannel.Init();
        }       

        /// <summary>
        /// Sends a MarketDataSessionMessage onto the default broadcast channel.
        /// </summary>
        /// <param name="sessionState">The state of the trading session.</param>
        /// <param name="startTime">The start time of the trading session.</param>
        /// <param name="endTime">The end time of the trading session.</param>
        public void BroadcastStatusMessage(SessionState sessionState, DateTime startTime, DateTime endTime)
        {
            _currentState = sessionState;
            _currentStartTime = startTime;
            _currentEndTime = endTime;

            MarketDataSessionMessage msg = new MarketDataSessionMessage(sessionState, startTime, endTime, _dataSource);

            SendBroadcastMessage(msg);
        }

        /// <summary>
        /// Sends a new MarketDataSnapshotUpdateMessage onto the default broadcast channel.
        /// An updated AggregatedDepthSnapshot will be attached to the message.
        /// </summary>
        /// <param name="underlying">The instrument for which to send a MarketDataSnapshotUpdateMessage.</param>
        public void BroadcastNewSnapshotUpdate(string underlying)
        {
            BroadcastNewSnapshotUpdate(underlying, null, null);
        }

        /// <summary>
        /// Sends a new MarketDataSnapshotUpdateMessage onto the default broadcast channel.
        /// An updated AggregatedDepthSnapshot will be attached to the message, together with the
        /// last Shout.
        /// </summary>
        /// <param name="underlying">The instrument for which to send a MarketDataSnapshotUpdateMessage.</param>
        /// <param name="shout">The Shout associated to the MarketDataSnapshotUpdateMessage to send.</param>
        public void BroadcastNewSnapshotUpdate(string underlying, Shout shout)
        {
            BroadcastNewSnapshotUpdate(underlying, shout, null);
        }

        /// <summary>
        /// Sends a new MarketDataSnapshotUpdateMessage onto the default broadcast channel.
        /// An updated AggregatedDepthSnapshot will be attached to the message, together with the
        /// last Shout, and the LastTradeUpdateMessage containing information regarding the last
        /// trade that was generated.
        /// </summary>
        /// <param name="underlying">The instrument for which to send a MarketDataSnapshotUpdateMessage.</param>
        /// <param name="shout">The Shout associated to the MarketDataSnapshotUpdateMessage to send.</param>
        /// <param name="trade">The LastTradeUpdateMessage associated to the MarketDataSnapshotUpdateMessage to send.</param>
        public void BroadcastNewSnapshotUpdate(string underlying, Shout shout, LastTradeUpdateMessage trade)
        {
            AggregatedDepthSnapshot snapshot = _depthCollection[underlying].Snapshot;
            _logger.Trace(LogLevel.Debug, "BroadcastNewSnapshotUpdate. Broadcasting snapshot: {0}", snapshot.ToString());
            MarketDataSnapshotUpdateMessage msg = new MarketDataSnapshotUpdateMessage(underlying, _dataSource, snapshot, shout, trade);

            SendBroadcastMessage(msg);
        }

        /// <summary>
        /// Starts MarketDataService.
        /// </summary>
        public override void Start()
        {
            if (_status == MDSStatus.Running)
            {
                _logger.TraceAndThrow("SendBroadcastMessage. Can't start service. Service already running.");
            }

            base.Start();

            ChangeStatus(MDSStatus.Running);
        }

        /// <summary>
        /// Stops MarketDataService.
        /// </summary>
        public override void Stop()
        {
            if (_status != MDSStatus.Running)
            {
                _logger.TraceAndThrow("SendBroadcastMessage. Can't stop service. Service isn't running.");
            }

            base.Stop();

            ChangeStatus(MDSStatus.Idle);
        }

        protected override void OnMessageReceived(MarketDataMessage channelMessage)
        {
            MarketDataMessage responseMessage = null;

            if (channelMessage is MarketDataPingMessage)
            {
                MarketDataPingMessage pingMessage = channelMessage as MarketDataPingMessage;
                _logger.Trace(LogLevel.Debug,
                    "MarketDataService.OnMessageReceived. PING message received from {0}: responding with PONG.",
                    channelMessage.Origin);
                responseMessage = pingMessage.CreatePingResponseMessage(_dataSource);
            }
            else
            {
                switch (channelMessage.Type)
                {
                    case MarketDataMessageType.Snapshot:
                        MarketDataSnapshotRequestMessage snapshotRequestMessage = channelMessage as MarketDataSnapshotRequestMessage;
                        AggregatedDepthSnapshot snapshot = null;
                        try
                        {
                            snapshot = _depthCollection[snapshotRequestMessage.Instrument].Snapshot;
                        }
                        catch (Exception ex)
                        {
                            _logger.Trace(LogLevel.Error, "MarketDataService.OnMessageReceived. Exception while retrieving depthcollection elements: {0}", ex.Message);
                            return;
                        }
                        responseMessage = snapshotRequestMessage.CreateResponseMessage(snapshot);
                        break;
                    case MarketDataMessageType.Status:
                        try
                        {
                            MarketDataStatusRequestMessage requestMessage = channelMessage as MarketDataStatusRequestMessage;
                            responseMessage = requestMessage.CreateStatusResponseMessage(_currentState, _currentStartTime, _currentEndTime, _dataSource);
                            _logger.Trace(LogLevel.Info, "MarketDataService.OnMessageReceived. MarketDataStatusRequestMessage received {0} REPLY will be sent: {1}", requestMessage.ToString(), responseMessage.ToString());
                        }
                        catch (Exception ex)
                        {
                            _logger.Trace(LogLevel.Error, "MarketDataService.OnMessageReceived. Exception {0}", ex.Message);
                            return;
                        }
                        break;
                    default:
                        _logger.Trace(LogLevel.Error, "MarketDataService.OnMessageReceived. Cannot handle message type {0}. Skipping message.", channelMessage.Type.ToString());
                        break;
                }
            }
            this.Respond(responseMessage);
        }
        
        private void SendBroadcastMessage(MarketDataMessage msg)
        {
            if (_status != MDSStatus.Running)
            {
                _logger.TraceAndThrow("SendBroadcastMessage. Can't broadcast message - Server not running!");
            }

            if (!msg.DataSource.Equals(this._dataSource))
            {
                _logger.Trace(LogLevel.Critical, "SendBroadcastMessage. Can't broadcast message - datasource mismatch!");
                return;
            }

            _logger.Trace(LogLevel.Info, "SendBroadcastMessage. Broadcasting message with NO timeout.");

            try
            {
                Message m = new Message(msg, _formatter);
                _broadcastChannel.Queue.Send(m);
            }
            catch (MessageQueueException mex)
            {
                _logger.Trace(LogLevel.Critical, "SendBroadcastMessage. MessageQueueException: {0}", mex.Message);
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "SendBroadcastMessage. Exception: {0}", ex.Message);
            }          
        }

        private void ChangeStatus(MDSStatus newStatus)
        {
            _logger.Trace(LogLevel.Info, "Changing status from {0} to {1}", _status.ToString(), newStatus.ToString());
            _status = newStatus;
        }
    }
}
