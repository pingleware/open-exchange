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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Messaging;
using System.Threading;

using OPEX.Configuration.Client;
using OPEX.Common;
using OPEX.Messaging;

namespace OPEX.ShoutService
{
    /// <summary>
    /// Dispatches the shouts received on the standard channel
    /// to the ShoutWatcher-s created at run-time.
    /// </summary>
    public class ShoutClient
    {
        #region Static
        
        private static readonly int BroadcastThreadSleepMsec = 1000;
        private static readonly object _root = new object();
        private static ShoutClient _theInstance;

        /// <summary>
        /// Gets the instance of the class OPEX.ShoutService.ShoutClient.
        /// </summary>
        public static ShoutClient Instance
        {
            get
            {
                lock (_root)
                {
                    if (_theInstance == null)
                    {
                        _theInstance = new ShoutClient();
                    }
                }
                return _theInstance;
            }
        }

        #endregion Static

        private IMessageFormatter _formatter;
        private bool _running;
        private Channel _broadcastChannel;
        private Logger _logger;
        private Dictionary<string, List<ShoutWatcher>> _watchers;

        private ShoutClient()
        {
            _logger = new Logger("ShoutClient");
            _formatter = new BinaryMessageFormatter();                        
            _running = false;
            _watchers = new Dictionary<string, List<ShoutWatcher>>();
            InitChannel();
        }               

        /// <summary>
        /// Starts ShoutClient.
        /// </summary>
        public void Start()
        {
            if (!_running)
            {
                _broadcastChannel.ReceiveCompleted += new ReceiveCompletedEventHandler(BroadcastChannel_ReceiveCompleted);
                _broadcastChannel.Start();
                _running = true;
            }
        }

        /// <summary>
        /// Stops ShoutClient.
        /// </summary>
        public void Stop()
        {
            if (_running)
            {
                _broadcastChannel.ReceiveCompleted -= new ReceiveCompletedEventHandler(BroadcastChannel_ReceiveCompleted);
                _broadcastChannel.Stop();
                _running = false;
            }
        }

        /// <summary>
        /// Creates a ShoutWatcher for the specified username.
        /// </summary>
        /// <param name="userName">The userName whose shouts to listen to.</param>
        /// <returns>A new ShoutWatcher for the specified username.</returns>
        public ShoutWatcher CreateShoutWatcher(string userName)
        {
            lock (_root)
            {
                ShoutWatcher watcher = new ShoutWatcher(this, userName);

                List<ShoutWatcher> watchersList = null;
                if (!_watchers.ContainsKey(userName))
                {
                    watchersList = new List<ShoutWatcher>();
                    _watchers.Add(userName, watchersList);
                }
                else
                {
                    watchersList = _watchers[userName];
                }
                watchersList.Add(watcher);

                return watcher;
            }
        }

        internal void RemoveWatcher(ShoutWatcher watcher)
        {
            lock (_root)
            {
                string userName = watcher.User;
                if (!_watchers.ContainsKey(userName))
                {
                    return;
                }
                List<ShoutWatcher> watchersList = _watchers[userName];
                if (!watchersList.Contains(watcher))
                {
                    return;
                }
                watchersList.Remove(watcher);
                if (watchersList.Count == 0)
                {                    
                    _watchers.Remove(userName);
                }
            }
        }

        private void BroadcastChannel_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue q = sender as MessageQueue;
            Message m = q.EndReceive(e.AsyncResult);

            if (!_formatter.CanRead(m))
            {
                _logger.Trace(LogLevel.Error, "BroadcastChannel_ReceiveCompleted. Cannot read message! The message couldn't be deserialized by the formatter. Skipping.");
                return;
            }

            Shout shout = _formatter.Read(m) as Shout;
            if (shout == null)
            {
                _logger.Trace(LogLevel.Critical, "BroadcastChannel_ReceiveCompleted. A null object was received. Skipping.");
                return;
            }
            shout.LocalTimeStamp = DateTime.Now;

            OnNewShout(shout);
        }

        private void OnNewShout(Shout shout)
        {
            lock (_root)
            {
                try
                {
                    foreach (string username in _watchers.Keys)
                    {
                        if (username.Equals(shout.User))
                        {
                            continue;
                        }

                        List<ShoutWatcher> watcherList = _watchers[username];
                        foreach (ShoutWatcher w in watcherList)
                        {
                            w.ReceiveShout(shout);
                        }
                    }
                }
                finally { }
            }
        }

        private void InitChannel()
        {
            string sourceName = string.Format("SHS_{0}", ConfigurationClient.Instance.GetConfigSetting("ApplicationName", "OPEXAppliation"));
            _broadcastChannel = new BroadcastChannel<Shout>(
                    "ShoutBroadcastQueueChannel",
                    QueueHelper.GetQueueName(sourceName, DuplexChannelType.Incoming, ChannelType.Broadcast),
                    "ShoutBroadcastQueue",
                    BroadcastThreadSleepMsec,
                    ShoutServer.ChannelDefaultMulticastAddress,
                    BroadcastChannelType.Receiver);

            _broadcastChannel.Init();
        }
    }
}
