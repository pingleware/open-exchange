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

namespace OPEX.MDS.Common
{
    [Flags]
    public enum MDSQueueUserType
    { 
        BroadcastListener = 1,
        RequestListener = 2,
        ResponseListener = 4
    }

    internal enum MDSMode
    {
        Client,
        Server
    }

    public abstract class MDSQueueUser
    {
        protected static readonly int BroadcastThreadSleepMsec = 1000;
        protected static readonly int RequestThreadSleepMsec = 1000;
        protected static readonly int ResponseThreadSleepMsec = 1000;

        private MDSMode _mdsMode;
        protected Logger _logger;
        protected string _dataSource;
        protected MDSQueueUserType _type;
        protected IMessageFormatter _formatter;
        protected Dictionary<string, Channel> _channels;
        protected MessageQueue _requestQueue;
        protected MessageQueue _responseQueue;
        protected MessageQueue _broadcastQueue;

        protected virtual void OnSnapshotUpdateMessageReceived(MarketDataSnapshotUpdateMessage snapshotUpdateMessage) { }
        protected virtual void OnLastTradeMessageReceived(LastTradeUpdateMessage lastTradeMessage) { }
        protected virtual void OnStatusRequestMessageReceived(MarketDataStatusRequestMessage statusRequestMessage) { }
        protected virtual void OnStatusResponseMessageReceived(MarketDataStatusResponseMessage statusResponseMessage) { }
        protected virtual void OnSnapshotRequestMessageReceived(MarketDataSnapshotRequestMessage snapshotRequestMessage) { }
        protected virtual void OnSnapshotResponseMessageReceived(MarketDataSnapshotResponseMessage snapshotResponseMessage) { }

        private bool IsBroadcastListener { get { return (_type & MDSQueueUserType.BroadcastListener) != 0; } }
        private bool IsRequestListener { get { return (_type & MDSQueueUserType.RequestListener) != 0; } }
        private bool IsResponseListener { get { return (_type & MDSQueueUserType.ResponseListener) != 0; } }

        public string DataSource { get { return _dataSource; } }

        public MDSQueueUser(string dataSource, MDSQueueUserType type)
        {
            _mdsMode = (MDSMode)Enum.Parse(typeof(MDSMode), Configuration.GetConfigSetting("MDSMode", "Client"));
            _dataSource = dataSource;
            _type = type;
            _logger = new Logger(string.Format("MDSQueueUser({0})", _dataSource));
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

        private void ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue q = null;            

            try
            {
                q = sender as MessageQueue;

                Message m = q.EndReceive(e.AsyncResult);

                if (!_formatter.CanRead(m))
                {
                    _logger.Trace(LogLevel.Critical, "ReceiveCompleted. Formatter cannot read message. Message skipped.");
                }
                else
                {
                    object o = _formatter.Read(m);
                    MarketDataMessage message = o as MarketDataMessage;

                    if (_dataSource.Equals(message.DataSource))
                    {
                        switch (message.Type)
                        {
                            case MarketDataMessageType.SnapshotUpdate:
                                {
                                    MarketDataSnapshotUpdateMessage snapshotUpdateMessage = message as MarketDataSnapshotUpdateMessage;
                                    OnSnapshotUpdateMessageReceived(snapshotUpdateMessage);
                                    break;
                                }
                            case MarketDataMessageType.LastTrade:
                                {
                                    LastTradeUpdateMessage lastTradeMessage = message as LastTradeUpdateMessage;
                                    OnLastTradeMessageReceived(lastTradeMessage);
                                    break;
                                }
                            case MarketDataMessageType.Snapshot:
                                {
                                    if (q.Path.Equals(_requestQueue.Path))
                                    {
                                        MarketDataSnapshotRequestMessage snapshotRequestMessage = message as MarketDataSnapshotRequestMessage;
                                        OnSnapshotRequestMessageReceived(snapshotRequestMessage);
                                    }
                                    else
                                    {
                                        MarketDataSnapshotResponseMessage snapshotResponseMessage = message as MarketDataSnapshotResponseMessage;
                                        OnSnapshotResponseMessageReceived(snapshotResponseMessage);
                                    }
                                    break;
                                }
                            case MarketDataMessageType.Status:
                                {
                                    if (q.Path.Equals(_requestQueue.Path))
                                    {
                                        MarketDataStatusRequestMessage statusRequestMessage = message as MarketDataStatusRequestMessage;
                                        OnStatusRequestMessageReceived(statusRequestMessage);
                                    }
                                    else
                                    {
                                        MarketDataStatusResponseMessage statusResponseMessage = message as MarketDataStatusResponseMessage;
                                        OnStatusResponseMessageReceived(statusResponseMessage);
                                    }
                                    break;
                                }
                            default:
                                _logger.TraceAndThrow("ReceiveCompleted. Invalid MarketDataMessageType while dispatching message");
                                break;
                        }
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
            bool servermode = (_mdsMode == MDSMode.Server);
            _channels = new Dictionary<string, Channel>(3);

            _channels["MDSBroadcastQueueChannel"] =
                new Channel("MDSBroadcastQueueChannel",
                    MQName.GetQueueName(_dataSource, MQType.Broadcast),
                    "MDSBroadcastQueue",
                    IsBroadcastListener,
                    true,
                    servermode,
                    BroadcastThreadSleepMsec);
            _channels["MDSRequestQueueChannel"] =
                new Channel("MDSRequestQueueChannel",
                    MQName.GetQueueName(_dataSource, MQType.Request),
                    "MDSRequestQueue",
                    IsRequestListener,
                    false,
                    servermode,
                    RequestThreadSleepMsec);
            _channels["MDSResponseQueueChannel"] =
                new Channel("MDSResponseQueueChannel",
                    MQName.GetQueueName(_dataSource, MQType.Response),
                    "MDSResponseQueue",
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

            _requestQueue = _channels["MDSRequestQueueChannel"].Queue;
            _responseQueue = _channels["MDSResponseQueueChannel"].Queue;
            _broadcastQueue = _channels["MDSBroadcastQueueChannel"].Queue;
        }
    }
}
