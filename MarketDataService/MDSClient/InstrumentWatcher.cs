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
using System.Threading;

using OPEX.Common;
using OPEX.MDS.Common;
using OPEX.ShoutService;

namespace OPEX.MDS.Client
{
    /// <summary>
    /// Observes market data of a specific instrument.
    /// </summary>
    public class InstrumentWatcher : IDisposable
    {
        private readonly object _root = new object();
        private readonly object _requestRoot = new object();
        private readonly Logger _logger;
        private readonly MarketDataClient _client;
        private readonly string _instrument;
        private readonly ManualResetEvent _downloadRequest;

        private Shout _lastShout;
        private LastTradeUpdateMessage _lastTradeUpdateMessage;
        private AggregatedDepthSnapshot _lastSnapshot;
        private bool _waitingForReply = false;
        private bool _currentRequestHasBeenServed = true;
        private EventHandler<MarketDataEventArgs> _marketDataChanged;

        /// <summary>
        /// Gets the instrument.
        /// </summary>
        public string Instrument { get { return _instrument; } }

        /// <summary>
        /// Gets the last AggregatedDepthSnapshot received for this instrument.
        /// </summary>
        public AggregatedDepthSnapshot LastSnapshot { get { return _lastSnapshot; } }

        /// <summary>
        /// Gets the LastTradeUpdateMessage received for this instrument.
        /// </summary>
        public LastTradeUpdateMessage LastTrade { get { return _lastTradeUpdateMessage; } }

        /// <summary>
        /// Gets the last Shout received for this instrument.
        /// </summary>
        public Shout LastShout { get { return _lastShout; } }

        internal InstrumentWatcher(MarketDataClient client, string instrument)
        {
            _client = client;
            _instrument = instrument;
            _downloadRequest = new ManualResetEvent(false);
            _logger = new Logger(string.Format("Watcher({0})", instrument));
            _client.Subscribe(_instrument);
        }

        /// <summary>
        /// Gets the last AggregatedDepthSnapshot of this instrument,
        /// in a synchronous way.
        /// </summary>
        /// <param name="waitMsec">The timeout.</param>
        /// <returns>True, if the snapshot was successfully retrieved. False otherwise.</returns>
        public bool GetLastSnapshot(int waitMsec)
        {
            bool success = false;

            lock (_requestRoot)
            {
                _waitingForReply = true;
                _currentRequestHasBeenServed = false;
                _downloadRequest.Reset();
                _client.DownloadSnapshot(_instrument);
                success = _downloadRequest.WaitOne(waitMsec);
            }

            return success;
        }

        #region Events

        /// <summary>
        /// Event raised when market data has changed.
        /// </summary>
        public event EventHandler<MarketDataEventArgs> MarketDataChanged
        {
            add { lock (_root) { _marketDataChanged += value; } }
            remove { lock (_root) { _marketDataChanged -= value; } }
        }
      
        #endregion Events

        internal void ReceiveMarketData(MarketDataEventArgs args)
        {
            if (_waitingForReply)
            {
                _waitingForReply = false;
                _downloadRequest.Set();
            }

            if (args.Type == MarketDataEventType.DownloadFinished && _currentRequestHasBeenServed)
            {
                return;
            }

            lock (_root)
            {
                if (_marketDataChanged != null)
                {
                    _lastSnapshot = _client.Cache.GetSnapshot(_instrument);
                    bool trade = (args.Type == MarketDataEventType.DepthChangedWithNewTrade);
                    bool shout = trade || (args.Type == MarketDataEventType.DepthChangedWithNewShout);
                    if (shout)
                    {
                        _lastShout = _client.Cache.GetLastShout(_instrument);
                    }
                    if (trade)
                    {
                        _lastTradeUpdateMessage = _client.Cache.GetLastTrade(_instrument);
                    }

                    foreach (EventHandler<MarketDataEventArgs> marketDataChangedHandler in _marketDataChanged.GetInvocationList())
                    {
                        try
                        {
                            marketDataChangedHandler(this, args);
                        }
                        catch (Exception ex)
                        {
                            _logger.Trace(LogLevel.Critical, "Exception while calling handler: {0} {1}", ex.Message, ex.StackTrace.Replace(Environment.NewLine, " "));
                        }
                    }
                }
            }

            _currentRequestHasBeenServed = true;
        }

        #region IDisposable Members

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _client.RemoveWatcher(this);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
