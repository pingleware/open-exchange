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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using OPEX.Common;
using OPEX.MDS.Common;
using OPEX.MDS.Client;
using OPEX.ShoutService;

namespace OPEX.TradingGUI
{
    public partial class DepthPanelControl : UserControl
    {
        private static readonly int DefaultLevels = 5;
        private static readonly int NumColumns = 4;
        
        private InstrumentWatcher _watcher;
        private Logger _logger;
        private string[][] _matrix;
        private int _lastTradeQuantity = 0;
        private double _lastTradePrice = 0;        

        public DepthPanelControl()
        {
            InitializeComponent();

            if (!this.DesignMode)
            {
                _logger = new Logger("DepthPanelControl");

                EmptyMatrix();
                UpdateGrid();
                UpdateLastTrade();
            }
        }

        public string Instrument
        {
            get
            {
                if (_watcher != null)
                {
                    return _watcher.Instrument;
                }
                return null;
            }
            set
            {
                if (this.DesignMode)
                {
                    return;
                }                
                if (_watcher != null)
                {                       
                    string oldInstrument = _watcher.Instrument;

                    _logger.Trace(LogLevel.Debug, "DepthPanelControl.set_Instrument. _watcher != null. OldInstrument: '{0}'", oldInstrument);

                    if (oldInstrument.Equals(value))
                    {
                        return;
                    }

                    _watcher.MarketDataChanged -= new EventHandler<MarketDataEventArgs>(Watcher_MarketDataChanged);
                    _watcher.Dispose();

                    if (value == null)
                    {
                        _watcher = null;
                        return;
                    }
                }

                if (value != null)
                {
                    _logger.Trace(LogLevel.Debug, "DepthPanelControl.set_Instrument. NewInstrument: '{0}'", value);
                    _watcher = MarketDataClient.Instance.CreateInstrumentWatcher(value);
                    _watcher.MarketDataChanged += new EventHandler<MarketDataEventArgs>(Watcher_MarketDataChanged);
                    _logger.Trace(LogLevel.Debug, "DepthPanelControl.set_Instrument. Invoking DownloadSnapshot()...");
                    _watcher.GetLastSnapshot(500);
                }
            }
        }

        public AggregatedDepthSnapshot LastSnapshot 
        { 
            get 
            {
                if (_watcher == null)
                {
                    return null;
                }
                return _watcher.LastSnapshot;
            } 
        }

        void Watcher_MarketDataChanged(object sender, MarketDataEventArgs args)
        {
            MarketDataEventType type = args.Type;
            bool trade = (type == MarketDataEventType.DepthChangedWithNewTrade);
            bool shout = trade || (type == MarketDataEventType.DepthChangedWithNewShout);

            AggregatedDepthSnapshot lastSnapshot = _watcher.LastSnapshot;
            Shout lastShout = _watcher.LastShout;
            LastTradeUpdateMessage lastTrade = _watcher.LastTrade;
            _logger.Trace(LogLevel.Info, "Watcher_MarketDataChanged. Type {0} trade {1} shout {2} LastTradeIsNull {3} LastSnapshotIsNull {4} LastShoutIsNull {5}",
                type, trade, shout, (lastTrade == null), (lastSnapshot == null), (lastShout == null));

            string tradeLog = null;
            _logger.Trace(LogLevel.Info, "Watcher_MarketDataChanged. Type {2} Best Bid: {0}. Best Ask {1}{3} ###################################",
                lastSnapshot.Buy.Best, lastSnapshot.Sell.Best, type, tradeLog);

            UpdateDepthSnapshot(_watcher.LastSnapshot);
            if (args.Type == MarketDataEventType.DepthChangedWithNewTrade)
            {
                UpdateLastTrade(_watcher.LastTrade);
            }
            UpdateSpreadLabels();
        }

        private delegate void UpdateSpreadLabelsDelegate();
        private void UpdateSpreadLabels()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateSpreadLabelsDelegate(UpdateSpreadLabels));
            }
            else
            {
                _logger.Trace(LogLevel.Debug, "UpdateSpreadLabels. Updating spread labels BEGIN");
                string bidSpreadString = string.Empty;
                string askSpreadString = string.Empty;
                if (_watcher != null)
                {
                    _logger.Trace(LogLevel.Debug, "UpdateSpreadLabels. _watcher != null. Retrieving Snapshot.");
                    AggregatedDepthSnapshot snp = _watcher.LastSnapshot;
                    if (snp != null)
                    {
                        double bBid = snp.Buy.Best;
                        double bAsk = snp.Sell.Best;
                        double spread = bAsk - bBid;
                        _logger.Trace(LogLevel.Debug, "UpdateSpreadLabels. bBid {0} bAsk {1} spread {2}.", bBid, bAsk, spread);
                        if (bBid != 0)
                        {
                            double bidSpread = spread / bBid;
                            bidSpreadString = string.Format("{0:F4}", bidSpread);
                            _logger.Trace(LogLevel.Debug, "UpdateSpreadLabels. bidSpread != 0. bidSpreadString = {0}.", bidSpreadString);
                        }
                        if (bAsk != 0)
                        {
                            double askSpread = spread / bAsk;
                            askSpreadString = string.Format("{0:F4}", askSpread);
                            _logger.Trace(LogLevel.Debug, "UpdateSpreadLabels. askSpread != 0. askSpreadString = {0}.", askSpreadString);
                        }
                    }
                    else
                    {
                        _logger.Trace(LogLevel.Debug, "UpdateSpreadLabels. snapshot == null. No update will be performed.");
                    }
                }
                else
                {
                    _logger.Trace(LogLevel.Debug, "UpdateSpreadLabels. _watcher == null. No update will be performed.");
                }
                lblAskSpreadValue.Text = askSpreadString;
                lblBidSpreadValue.Text = bidSpreadString;
                _logger.Trace(LogLevel.Debug, "UpdateSpreadLabels. Updating spread labels END");
            }
        }                    

        private void UpdateLastTrade(LastTradeUpdateMessage msg)
        {
            if (msg == null)
            {
                return;
            }

            _lastTradePrice = msg.Price;
            _lastTradeQuantity = msg.Size;

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new UpdateEventHandler(UpdateLastTrade));
            }
            else
            {
                UpdateLastTrade();
            }
        }

        private void UpdateDepthSnapshot(AggregatedDepthSnapshot snapshot)
        {
            if (snapshot == null)
            {
                EmptyMatrix();
            }
            else
            {
                IEnumerator buyQuoteEnumerator = snapshot.Buy.GetEnumerator();
                IEnumerator sellQuoteEnumerator = snapshot.Sell.GetEnumerator();
                bool moreBuy = true;
                bool moreSell = true;
                for (int i = 0; i < DefaultLevels; ++i)
                {
                    AggregatedQuote buyQuote = null;
                    AggregatedQuote sellQuote = null;

                    if (moreBuy && (moreBuy = buyQuoteEnumerator.MoveNext()))
                    {
                        buyQuote = buyQuoteEnumerator.Current as AggregatedQuote;
                    }
                    if (moreSell && (moreSell = sellQuoteEnumerator.MoveNext()))
                    {
                        sellQuote = sellQuoteEnumerator.Current as AggregatedQuote;
                    }

                    _matrix[i][0] = (buyQuote != null) ? buyQuote.Quantity.ToString("N0") : string.Empty;
                    _matrix[i][1] = (buyQuote != null) ? buyQuote.Price.ToString("F4") : string.Empty;
                    _matrix[i][2] = (sellQuote != null) ? sellQuote.Price.ToString("F4") : string.Empty;
                    _matrix[i][3] = (sellQuote != null) ? sellQuote.Quantity.ToString("N0") : string.Empty;
                }
            }

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new UpdateEventHandler(UpdateGrid));
            }
            else
            {
                UpdateGrid();
            }
        }

        private delegate void UpdateEventHandler();

        private void UpdateLastTrade()
        {
            string tradeQty = string.Empty;
            string tradePrice = string.Empty;
            string tradeTime = string.Empty;

            if (_lastTradePrice != 0 && _lastTradeQuantity != 0)
            {
                tradeQty = string.Format("{0:N0}", _lastTradeQuantity);
                tradePrice = string.Format("{0:F4}", _lastTradePrice);
                tradeTime = DateTime.Now.ToString("HH:mm:ss");     
            }
            lblLastTradeQty.Text = tradeQty;
            lblLastTradePrice.Text = tradePrice;
            lblTime.Text = tradeTime;
        }

        private void UpdateGrid()
        {
            bool firstTime = false;

            if (lstDepth.Items.Count == 0)
            {
                firstTime = true;
            }

            for (int i = 0; i < DefaultLevels; ++i)
            {
                ListViewItem item = new ListViewItem(_matrix[i]);

                if (firstTime)
                {
                    lstDepth.Items.Add(item);
                }
                else
                {
                    lstDepth.Items[i] = item;
                }
            }
        }

        private void EmptyMatrix()
        {
            _matrix = new string[DefaultLevels][];
            for (int i = 0; i < DefaultLevels; ++i)
            {
                _matrix[i] = new string[NumColumns];
                for (int j = 0; j < NumColumns; ++j)
                {
                    _matrix[i][j] = string.Empty;
                }
            }
        }
    }
}
