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
using OPEX.Configuration.Common;
using OPEX.MDS.Common;
using OPEX.MDS.Client;
using OPEX.ShoutService;

namespace OPEX.SalesGUI
{
    public partial class DepthPanelControl : UserControl
    {
        private static readonly int DefaultLevels = 5;
        private static readonly int NumColumns = 5;
        private readonly string MyUser;
        private readonly GradientFlash _gradientFlash;
        private Form _lastParentForm;
        
        private InstrumentWatcher _watcher;
        private Logger _logger;
        private string[][] _matrix;
        private int _lastTradeQuantity = 0;
        private double _lastTradePrice = 0;
        private readonly Label[] _labels;

        public DepthPanelControl()
        {
            InitializeComponent();

            if (DesignMode)
            {
                lblRIC.Text = "TTECH.L";
                lblLastTradeQty.Text = "1";
                lblLastTradePrice.Text = "160";
            }
            else
            {
                MyUser = ConfigurationHelper.GetConfigSetting("ApplicationName", "OPEXApplication");
                _logger = new Logger("DepthPanelControl");

                _labels = new Label[] {
                    lblAt,                    
                    lblLastTradePrice,
                    lblLastTradeQty,
                    lblLastTrd,
                    lblRIC };

                EmptyMatrix();
                UpdateGrid(true);
                UpdateLastTrade();
                ToggleLabels(false);
                lblRIC.Text = null;
                _gradientFlash = new GradientFlash(1.5, 100, 0.1, Color.LimeGreen, SystemColors.Window, 0.2, this.lstDepth);
            }
        }
        private readonly int GetSnapshotTimeoutMsec = 2000;
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

                _logger.Trace(LogLevel.Debug, "DepthPanelControl.set_Instrument. ####################");

                ToggleLabels(false);
                _firstTradeUpdateReceived = true;
                lblRIC.Text = value;

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
                    }
                }
                else if (value != null)
                {
                    _logger.Trace(LogLevel.Debug, "DepthPanelControl.set_Instrument. NewInstrument: '{0}'", value);
                    _watcher = MarketDataClient.Instance.CreateInstrumentWatcher(value);
                    _watcher.MarketDataChanged += new EventHandler<MarketDataEventArgs>(Watcher_MarketDataChanged);
                    bool b = _watcher.GetLastSnapshot(GetSnapshotTimeoutMsec);
                    _logger.Trace(LogLevel.Debug, "DepthPanelControl.set_Instrument. GetLastSnapshot({1}) returned {0}", b, GetSnapshotTimeoutMsec);
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

        private delegate void ToggleLabelsDelegate(bool enable);
        private void ToggleLabels(bool enable)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new ToggleLabelsDelegate(ToggleLabels), enable);
            }
            else
            {
                foreach (Label lbl in _labels)
                {
                    lbl.Visible = enable;
                    lbl.Enabled = enable;
                }

                lblRIC.Text = this.Instrument;
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
            if (trade)
            {                
                tradeLog = string.Format(" LastTrade @ {0}", lastTrade.Price);
            }
            _logger.Trace(LogLevel.Info, "Watcher_MarketDataChanged. Type {2} Best Bid: {0}. Best Ask {1}{3} ###################################",
                lastSnapshot.Buy.Best, lastSnapshot.Sell.Best, type, tradeLog);

            UpdateDepthSnapshot(_watcher.LastSnapshot);
            if (args.Type == MarketDataEventType.DepthChangedWithNewTrade)
            {
                UpdateLastTrade(_watcher.LastTrade);
            }
        }

        #region Flash

        public void BeginFlash()
        {
            Flash();
        }

        public void StopFlash()
        {
            Stop();
        }        

        private void Stop()
        {
            lock (_flashRoot)
            {
                if (_flashing)
                {
                    _gradientFlash.StopFlash();
                }
                _flashing = false;
            }
        }

        private bool _flashing = false;
        private readonly object _flashRoot = new object();
        private void Flash()
        {
            lock (_flashRoot)
            {
                if (!_flashing)
                {
                    _gradientFlash.FlashFinished += new EventHandler(FlashFinished);
                    _gradientFlash.StartFlash();
                }

                _flashing = true;
            }
        }
        
        private void FlashFinished(object sender, EventArgs e)
        {
            _gradientFlash.FlashFinished -= new EventHandler(FlashFinished);
            _flashing = false;
        }

        #endregion Flash

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

        private readonly object _updateGridRoot = new object();
        private void UpdateDepthSnapshot(AggregatedDepthSnapshot snapshot)
        {
            lock (_updateGridRoot)
            {
                EmptyMatrix();

                if (snapshot != null)
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

                        _matrix[i][1] = (buyQuote != null) ? buyQuote.Quantity.ToString("N0") : string.Empty;
                        _matrix[i][2] = (buyQuote != null) ? buyQuote.Price.ToString("N0") : string.Empty;
                        _matrix[i][3] = (sellQuote != null) ? sellQuote.Price.ToString("N0") : string.Empty;
                        _matrix[i][4] = (sellQuote != null) ? sellQuote.Quantity.ToString("N0") : string.Empty;
                    }
                }

                if (this.InvokeRequired)
                {
                    this.Invoke(new UpdateEventHandler(UpdateGrid));
                }
                else
                {
                    UpdateGrid();
                }
            }
        }

        private delegate void UpdateEventHandler();

        private bool _firstTradeUpdateReceived = true;
        private void UpdateLastTrade()
        {
            lblLastTradeQty.Text = string.Format("{0:N0}", _lastTradeQuantity);
            lblLastTradePrice.Text = string.Format("{0:N0}", _lastTradePrice);

            if (_firstTradeUpdateReceived)
            {
                ToggleLabels(true);
                _firstTradeUpdateReceived = false;
            }
        }

        private void UpdateGrid()
        {
            UpdateGrid(false);
        }

        private void UpdateGrid(bool init)
        {
            for (int i = 0; i < DefaultLevels; ++i)
            {
                ListViewItem item = new ListViewItem(_matrix[i]);

                if (!init)
                {
                    lstDepth.Items[i] = item;
                }
                else
                {
                    lstDepth.Items.Add(item);
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

        void ParentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Stop();
        }
        
        private void DepthPanelControl_ParentChanged(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            if (this.ParentForm != null)
            {
                this.ParentForm.FormClosing += new FormClosingEventHandler(ParentForm_FormClosing);
            }
            else if (_lastParentForm != null)
            {
                _lastParentForm.FormClosing -= new FormClosingEventHandler(ParentForm_FormClosing);
            }
            _lastParentForm = this.ParentForm;
        }
    }
}
