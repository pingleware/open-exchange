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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OPEX.Common;
using OPEX.Messaging;
using OPEX.MDS.Common;
using OPEX.Configuration.Client;

namespace OPEX.MDS.Client
{    
    /// <summary>
    /// Creates InstrumentWatchers that receive market data updates.
    /// Sends requests for exchange status and dispatches the responses.
    /// </summary>
    public class MarketDataClient : OutgoingDuplexChannel<MarketDataMessage>
    {
        /// <summary>
        /// Default data source name.
        /// </summary>
        public static readonly string DefaultDataSource = "OPEX";
        private static readonly int SessionMessageToleranceSec = 3;

        private static readonly int BroadcastThreadSleepMsec = 1000;
        private static readonly object _staticRoot = new object();
        private static MarketDataClient _theInstance;

        private readonly object _watchersRoot = new object();
        private readonly object _subscriptionRoot = new object();
        private readonly object _sendRoot = new object();
        private readonly Channel _ackChannel;
        private readonly Dictionary<string, List<InstrumentWatcher>> _watchers;
        private readonly KeepAliveMessageGenerator _keepAliveMessageGenerator;

        protected readonly Dictionary<string, Channel> _broadCastChannels;
        protected readonly HashSet<string> _subscriptionSet;
        protected readonly MarketDataCache _cache;

        /// <summary>
        /// Gets the instance of the class
        /// OPEX.MDS.Client.MarketDataClient.
        /// </summary>
        public static MarketDataClient Instance
        {
            get
            {
                lock (_staticRoot)
                {
                    if (_theInstance == null)
                    {
                        _theInstance = new MarketDataClient();
                        bool purge = bool.Parse(ConfigurationClient.Instance.GetConfigSetting("PurgeQueuesOnStartup", "true"));
                        if (purge)
                        {
                            _theInstance.Purge();
                        }
                    }
                }
                return _theInstance;
            }
        }

        protected Dictionary<string, Channel>.KeyCollection AllowedDataSources { get { return _broadCastChannels.Keys; } }

        private MarketDataClient()
            : base(string.Concat("MDS", "_", ConfigurationClient.Instance.GetConfigSetting("ApplicationName", null)))
        {            
            _broadCastChannels = new Dictionary<string, Channel>();            
            _subscriptionSet = new HashSet<string>();
            _cache = new MarketDataCache();
            _watchers = new Dictionary<string, List<InstrumentWatcher>>();
            _ackChannel = new Channel(string.Format("{0}ACKQueueChannel", _name),
                    QueueHelper.GetQueueName("MDSACKQueue", DuplexChannelType.Incoming, ChannelType.Response),
                    "MDSACKQueue",
                    true,
                    1000);
            _keepAliveMessageGenerator = new KeepAliveMessageGenerator(1000, new MarketDataPingMessage(MarketDataClient.DefaultDataSource));
            _keepAliveMessageGenerator.Ping += new KeepAliveEventHandler(KeepAliveMessageGenerator_Ping);
            Init();
        }

        /// <summary>
        /// Occurs when the exchange session changes status, i.e. market open, market close.
        /// </summary>
        public event SessionChangedEventHandler SessionChanged;

        #region Properties 

        internal MarketDataCache Cache { get { return _cache; } }

        #endregion Properties       

        #region Methods

        /// <summary>
        /// Creates an InstrumentWatcher that receives market data
        /// updates for a specific instrument.
        /// </summary>
        /// <param name="instrument">The instrument to subscribe to.</param>
        /// <returns>The InstrumentWatcher for the instrument specified.</returns>
        public InstrumentWatcher CreateInstrumentWatcher(string instrument)
        {
            lock (_watchersRoot)
            {
                InstrumentWatcher watcher = new InstrumentWatcher(this, instrument);

                List<InstrumentWatcher> watchersList = null;
                if (!_watchers.ContainsKey(instrument))
                {
                    watchersList = new List<InstrumentWatcher>();
                    _watchers.Add(instrument, watchersList);
                }
                else
                {
                    watchersList = _watchers[instrument];
                }
                watchersList.Add(watcher);

                return watcher;
            }
        }

        /// <summary>
        /// Requests the exchange status of a specific data source.
        /// </summary>
        /// <param name="dataSource">The data source for which to receive exchange status.</param>
        public void RequestStatus(string dataSource)
        {
            MarketDataMessage marketDataMessage = new MarketDataStatusRequestMessage(null, dataSource);
            SendMessage(marketDataMessage);
        }

        internal void RemoveWatcher(InstrumentWatcher watcher)
        {
            lock (_watchersRoot)
            {
                string instrument = watcher.Instrument;
                if (!_watchers.ContainsKey(instrument))
                {
                    return;
                }
                List<InstrumentWatcher> watchersList = _watchers[instrument];
                if (!watchersList.Contains(watcher))
                {
                    return;
                }
                watchersList.Remove(watcher);
                if (watchersList.Count == 0)
                {
                    Unsubscribe(instrument);
                    _watchers.Remove(instrument);
                }                
            }
        }

        internal void Subscribe(string instrument)
        {
            _logger.Trace(LogLevel.Info, "Subscribe. Subscribing to instrument {0} ================= ", instrument);
            if (instrument == null)
            {
                return;
            }
            lock (_subscriptionRoot)
            {
                _subscriptionSet.Add(instrument);
            }
        }

        private void Unsubscribe(string instrument)
        {
            _logger.Trace(LogLevel.Info, "Unsubscribe. Unsubscribing from instrument {0} ===============", instrument);
            if (instrument == null)
            {
                return;
            }
            lock (_subscriptionRoot)
            {
                if (_subscriptionSet.Contains(instrument))
                {
                    _subscriptionSet.Remove(instrument);
                }
            }
        }      

        private void UnsubscribeAll()
        {
            lock (_subscriptionRoot)
            {
                _subscriptionSet.Clear();
            }
        }

        /// <summary>
        /// Downloads a snapshot.
        /// </summary>
        /// <param name="instrument">The instrument to download</param>
        internal void DownloadSnapshot(string instrument)
        {
            string dataSource = GetDataSource(instrument);
            MarketDataSnapshotRequestMessage marketDataMessage = new MarketDataSnapshotRequestMessage(dataSource, instrument);
            SendMessage(marketDataMessage);
        }

        protected override void SendMessage(MarketDataMessage messageContent)
        {
            lock (_sendRoot)
            {
                if (messageContent is MarketDataPingMessage)
                {
                    foreach (Channel requestChannel in _requestChannels.Values)
                    {
                        Message m = new Message(messageContent, requestChannel.Queue.Formatter);
                        m.ResponseQueue = _ownChannel.Queue;
                        requestChannel.Queue.Send(m);
                    }
                }
                else if (!(messageContent is MarketDataStatusRequestMessage))
                {
                    base.SendMessage(messageContent);
                }
                else
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

                    // request acknowledgement for Status Messages

                    Message m = new Message(messageContent, requestQueue.Formatter);
                    m.ResponseQueue = _ownChannel.Queue;
                    requestQueue.Send(m);
                }
            }
        }

        private string GetDataSource(string instrument)
        {
            /// TODO
            return DefaultDataSource;
        }

        /// <summary>
        /// Downloads ALL the instruments subscribed.
        /// </summary>
        internal void DownloadAll()
        {
            lock (_subscriptionRoot)
            {
                foreach (string instrument in _subscriptionSet)
                {
                    DownloadSnapshot(instrument);
                }
            }
        }

        /// <summary>
        /// Starts MarketDataClient.
        /// </summary>
        public override void Start()
        {            
            base.Start();
            foreach (Channel channel in _broadCastChannels.Values)
            {
                channel.Start();
            }
            _ackChannel.Start();
            _keepAliveMessageGenerator.Start();
            _logger.Trace(LogLevel.Method, "Start. MarketDataClient has started =============================================");
        }

        /// <summary>
        /// Stops MarketDataClient.
        /// </summary>
        public override void Stop()
        {
            _keepAliveMessageGenerator.Ping -= new KeepAliveEventHandler(KeepAliveMessageGenerator_Ping);            
            _keepAliveMessageGenerator.Stop();
            base.Stop();
            foreach (Channel channel in _broadCastChannels.Values)
            {
                channel.ReceiveCompleted -= new ReceiveCompletedEventHandler(BroadcastChannel_ReceiveCompleted);
                channel.Stop();
            }
            _ackChannel.Stop();
            
            _logger.Trace(LogLevel.Method, "Stop. MarketDataClient has stopped =============================================");
        }

        #endregion Methods        

        #region Message Processing

        private void OnSessionMessageReceived(MarketDataSessionMessage sessionMessage)
        {
            OnSessionMessageReceived(sessionMessage, true, true);
        }

        private void OnSessionMessageReceived(MarketDataSessionMessage sessionMessage, bool serverAlive, bool isBroadcast)
        {
            if (SessionChanged == null)
            {
                return;
            }

            DateTime nowPlus = DateTime.Now.AddSeconds(SessionMessageToleranceSec);
            DateTime nowMinus = DateTime.Now.Subtract(TimeSpan.FromSeconds(SessionMessageToleranceSec));
            bool isNowPlusIncluded = (sessionMessage.StartTime.CompareTo(nowPlus) < 0 && sessionMessage.EndTime.CompareTo(nowPlus) > 0);
            bool isNowMinusIncluded = (sessionMessage.StartTime.CompareTo(nowMinus) < 0 && sessionMessage.EndTime.CompareTo(nowMinus) > 0);
            if (isNowMinusIncluded || isNowPlusIncluded)
            {
                foreach (SessionChangedEventHandler handler in SessionChanged.GetInvocationList())
                {
                    handler(this,
                        new SessionChangedEventArgs(
                            serverAlive,
                            isBroadcast,
                            sessionMessage.SessionState,
                            sessionMessage.StartTime,
                            sessionMessage.EndTime));
                }
            }
            else
            {
                _logger.Trace(LogLevel.Warning, "OnSessionMessageReceived. An OLD session message was received, the message will be DISCARDED! {0}", sessionMessage);
            }
        }

        private void OnSnapshotUpdateMessageReceived(MarketDataSnapshotUpdateMessage snapshotUpdateMessage)
        {
            if (!Match(snapshotUpdateMessage.Instrument))
            {
                return;
            }
            _cache.ReceiveSnapshot(snapshotUpdateMessage);
            MarketDataEventType type = MarketDataEventType.DepthChanged;
            if (snapshotUpdateMessage.Trade != null)
            {
                type = MarketDataEventType.DepthChangedWithNewTrade;
            }
            else if (snapshotUpdateMessage.Shout != null)
            {
                type = MarketDataEventType.DepthChangedWithNewShout;
            }

            OnMarketDataChanged(new MarketDataEventArgs(type, snapshotUpdateMessage.Instrument));
        }

        private void OnSnapshotResponseMessageReceived(MarketDataSnapshotResponseMessage snapshotResponseMessage)
        {
            if (!Match(snapshotResponseMessage.Instrument))
            {
                return;
            }
            _cache.ReceiveDownloadFinishedSnapshot(snapshotResponseMessage);
            OnMarketDataChanged(new MarketDataEventArgs(MarketDataEventType.DownloadFinished, snapshotResponseMessage.Instrument));
        }

        private void OnStatusResponseMessageReceived(MarketDataSessionMessage sessionMessage) 
        {            
            OnSessionMessageReceived(sessionMessage, true, false);
        }

        private void OnMarketDataChanged(MarketDataEventArgs args)
        {
            bool anyWatchers = false;
            List<InstrumentWatcher> watchersList = null;

            lock (_watchersRoot)
            {
                if (_watchers.ContainsKey(args.Instrument))
                {
                    watchersList = _watchers[args.Instrument];
                    if (watchersList != null && watchersList.Count > 0)
                    {
                        anyWatchers = true;
                    }
                }
                if (!anyWatchers)
                {
                    _logger.Trace(LogLevel.Warning, "No InstrumentWatcher is interested in this update.");
                    return;
                }
                
                foreach (InstrumentWatcher watcher in watchersList)
                {
                    watcher.ReceiveMarketData(args);
                }
            }
        }

        #endregion Message Processing

        private void BroadcastChannel_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue q = sender as MessageQueue;
            Message m = q.EndReceive(e.AsyncResult);

            if (!_formatter.CanRead(m))
            {
                _logger.Trace(LogLevel.Error, "BroadcastChannel_ReceiveCompleted. Cannot read message! The message couldn't be deserialized by the formatter. Skipping.");
            }
            else
            {
                MarketDataMessage message = _formatter.Read(m) as MarketDataMessage;

                if (message != null)
                {
                    if (AllowedDataSources.Contains(message.DataSource))
                    {
                        switch (message.Type)
                        {
                            case MarketDataMessageType.SnapshotUpdate:
                                {
                                    MarketDataSnapshotUpdateMessage snapshotUpdateMessage = message as MarketDataSnapshotUpdateMessage;
                                    OnSnapshotUpdateMessageReceived(snapshotUpdateMessage);
                                    break;
                                }                           
                            case MarketDataMessageType.Status:
                                {
                                    MarketDataSessionMessage sessionMessage = message as MarketDataSessionMessage;
                                    OnSessionMessageReceived(sessionMessage);
                                    break;
                                }
                            default:
                                break;
                        }
                    }
                }
                else
                {
                    _logger.Trace(LogLevel.Critical, "BroadcastChannel_ReceiveCompleted. A null object was received. Skipping.");
                }
            }
        }

        private void KeepAliveMessageGenerator_Ping(object sender, object pingMessage)
        {
            MarketDataPingMessage messageContent = pingMessage as MarketDataPingMessage;
            SendMessage(messageContent);            
        }

        private void AckChannel_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue q = sender as MessageQueue;
            Message m = q.EndReceive(e.AsyncResult);

            if (!_formatter.CanRead(m))
            {
                _logger.Trace(LogLevel.Error, "AckChannel_ReceiveCompleted. Cannot read message! The message couldn't be deserialized by the formatter. Skipping.");
            }
            else
            {
                MarketDataMessage message = _formatter.Read(m) as MarketDataStatusRequestMessage;

                if (message != null)
                {
                    OnServerDead();
                }
                else 
                {
                    _logger.Trace(LogLevel.Critical, "AckChannel_ReceiveCompleted. A null object was received. Skipping.");
                }
            }
        }

        private void OnServerDead()
        {
            _logger.Trace(LogLevel.Critical, "OnServerDead. ");
 
            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.MaxValue;
            SessionState state = SessionState.Close;
            MarketDataSessionMessage msg = new MarketDataSessionMessage(state, startTime, endTime, GetDataSource(null));
            OnSessionMessageReceived(msg, false, false);
        }

        protected override void OnMessageReceived(MarketDataMessage message)
        {
            if (message is MarketDataPingMessage)
            {
                //_logger.Trace(LogLevel.Debug,
                    //"MarketDataClient.OnMessageReceived. PING message received from {0}: ignoring.", message.Origin);
                return;
            }

            if (AllowedDataSources.Contains(message.DataSource))
            {
                switch (message.Type)
                {
                    case MarketDataMessageType.Snapshot:
                        {
                            MarketDataSnapshotResponseMessage snapshotResponseMessage = message as MarketDataSnapshotResponseMessage;
                            OnSnapshotResponseMessageReceived(snapshotResponseMessage);
                            break;
                        }
                    case MarketDataMessageType.Status:
                        {
                            MarketDataSessionMessage sessionMessage = message as MarketDataSessionMessage;
                            OnStatusResponseMessageReceived(sessionMessage);
                            break;
                        }
                    default:
                        _logger.TraceAndThrow("arketDataClient.OnMessageReceived. Invalid MarketDataMessageType while dispatching message");
                        break;
                }
            }
        }

        private void Init()
        {
            string hostsCSL = ConfigurationClient.Instance.GetConfigSetting("MDSAllowedSourcesHosts", "");
            string sourcesCSL = ConfigurationClient.Instance.GetConfigSetting("MDSAllowedSources", "");

            string[] hostsBits = hostsCSL.Split(new char[] { ',' });
            string[] sourcesBits = sourcesCSL.Split(new char[] { ',' });
            for (int i = 0; i < hostsBits.Length; ++i)
            {
                string host = hostsBits[i];
                string source = sourcesBits[i];
                string sourceName = string.Concat("MDS_", source);

                RegisterChannel(source, host, sourceName);
                
                Channel channel = new BroadcastChannel<MarketDataMessage>(
                    string.Format("MDSBroadcastQueueChannel({0})", source),
                    QueueHelper.GetQueueName(sourceName, DuplexChannelType.Incoming, ChannelType.Broadcast),
                    string.Format("MDSBroadcastQueue({0})", source),
                    BroadcastThreadSleepMsec,
                    BroadcastChannel<MarketDataMessage>.DefaultMulticastAddress,
                    BroadcastChannelType.Receiver);

                channel.Init();
                channel.ReceiveCompleted += new ReceiveCompletedEventHandler(BroadcastChannel_ReceiveCompleted);
                _broadCastChannels.Add(source, channel);                                
            }

            _ackChannel.Init();
            _ackChannel.Queue.MessageReadPropertyFilter.Body = true;
            _ackChannel.Queue.MessageReadPropertyFilter.CorrelationId = true;
            _ackChannel.ReceiveCompleted += new ReceiveCompletedEventHandler(AckChannel_ReceiveCompleted);            
        }

        private bool Match(string instrument)
        {
            lock (_subscriptionRoot)
            {
                return _subscriptionSet.Contains(instrument);
            }
        }  
    }
}
