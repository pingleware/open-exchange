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
using System.Linq;
using System.Text;

using OPEX.MDS.Common;
using OPEX.Common;
using OPEX.ShoutService;
using OPEX.Storage;

namespace OPEX.MDS.Client
{
    /// <summary>
    /// Holds LastTradeUpdateMessages, Shouts and
    /// AggregatedDepthSnapshot of multiple instruments.
    /// </summary>
    public class MarketDataCache
    {
        private readonly Logger _logger;
        private readonly Hashtable _snapshots;
        private readonly Hashtable _lastTrades;
        private readonly Hashtable _shouts;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.MDS.Client.MarketDataCache.
        /// </summary>
        public MarketDataCache()
        {            
            _logger = new Logger("MarketDataCache");
            _snapshots = Hashtable.Synchronized(new Hashtable());
            _lastTrades = Hashtable.Synchronized(new Hashtable());
            _shouts = Hashtable.Synchronized(new Hashtable());
        }

        internal MarketDataSnapshotMessage GetSnapshotMessage(string instrument)
        {
            MarketDataSnapshotMessage msg = null;

            if (_snapshots.ContainsKey(instrument))
            {
                msg = _snapshots[instrument] as MarketDataSnapshotMessage;
            }

            return msg;
        }

        /// <summary>
        /// Retrieves the last Shout for the specified instrument.
        /// </summary>
        /// <param name="instrument">The instrument for which to retrieve the last Shout.</param>
        /// <returns>The last Shout received for the specified instrument, if any.</returns>
        public Shout GetLastShout(string instrument)
        {
            Shout s = null;

            if (_shouts.ContainsKey(instrument))
            {
                s = _shouts[instrument] as Shout;
            }

            return s;
        }

        /// <summary>
        /// Retrieves the last AggregatedDepthSnapshot for the specified instrument.
        /// </summary>
        /// <param name="instrument">The instrument for which to retrieve the last AggregatedDepthSnapshot.</param>
        /// <returns>The last AggregatedDepthSnapshot received for the specified instrument, if any.</returns>
        public AggregatedDepthSnapshot GetSnapshot(string instrument)
        {
            AggregatedDepthSnapshot s = null;

            if (_snapshots.ContainsKey(instrument))
            {
                MarketDataSnapshotMessage msg = _snapshots[instrument] as MarketDataSnapshotMessage;
                s = msg.Snapshot;
            }

            return s;
        }

        /// <summary>
        /// Retrieves the last LastTradeUpdateMessage for the specified instrument.
        /// </summary>
        /// <param name="instrument">The instrument for which to retrieve the last LastTradeUpdateMessage.</param>
        /// <returns>The last LastTradeUpdateMessage received for the specified instrument, if any.</returns>
        public LastTradeUpdateMessage GetLastTrade(string instrument)
        {
            LastTradeUpdateMessage msg = null;

            if (_lastTrades.ContainsKey(instrument))
            {
                msg = _lastTrades[instrument] as LastTradeUpdateMessage;
            }

            return msg;
        }
      
        internal void ReceiveSnapshot(MarketDataSnapshotMessage snapshotMessage)
        {
            string instrument = snapshotMessage.Instrument;           
            MarketDataSnapshotUpdateMessage updateMessage = snapshotMessage as MarketDataSnapshotUpdateMessage;

           _snapshots[instrument] = snapshotMessage;
            
            if (updateMessage != null)
            {
                if (updateMessage.Shout != null)
                {
                    _shouts[instrument] = updateMessage.Shout;
                }
                if (updateMessage.Trade != null)
                {
                    _lastTrades[instrument] = updateMessage.Trade;
                }
            }
        }

        internal void ReceiveDownloadFinishedSnapshot(MarketDataSnapshotMessage snapshotMessage)
        {
            string instrument = snapshotMessage.Instrument;            
            DateTime lastUpdateTime = DateTime.MinValue;
            MarketDataSnapshotUpdateMessage updateMessage = snapshotMessage as MarketDataSnapshotUpdateMessage;

            if (_snapshots.ContainsKey(instrument))
            {
                MarketDataSnapshotMessage oldMsg = _snapshots[instrument] as MarketDataSnapshotMessage;
                lastUpdateTime = oldMsg.TimeStamp;
            }

            if (snapshotMessage.TimeStamp.CompareTo(lastUpdateTime) < 0)
            {
                _logger.Trace(LogLevel.Info, "The last message received is out of date. lastUpdateTime {0} snapshotTime {1}. Keeping existing copy and DISCARDING new message.",
                    lastUpdateTime.ToString("HH:mm:ss.fff"), snapshotMessage.TimeStamp.ToString("HH:mm:ss.fff"));
            }
            else
            {
                _snapshots[instrument] = snapshotMessage;

                if (updateMessage != null)
                {
                    if (updateMessage.Shout != null)
                    {
                        _shouts[instrument] = updateMessage.Shout;
                    }
                    if (updateMessage.Trade != null)
                    {
                        _lastTrades[instrument] = updateMessage.Trade;
                    }
                }
            }
        }
    }
}
