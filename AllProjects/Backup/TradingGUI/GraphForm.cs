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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ZedGraph;

using OPEX.Common;
using OPEX.MDS.Common;
using OPEX.MDS.Client;

namespace OPEX.TradingGUI
{
    public partial class GraphForm : Form
    {        
        private Logger _logger;        
        private InstrumentWatcher _watcher;

        public GraphForm()
        {
            InitializeComponent();
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
                else if (value != null)
                {
                    _watcher = MarketDataClient.Instance.CreateInstrumentWatcher(value);
                    _watcher.MarketDataChanged += new EventHandler<MarketDataEventArgs>(Watcher_MarketDataChanged);
                    _watcher.GetLastSnapshot(1000);
                    _logger = new Logger(string.Format("GraphForm({0})", _watcher.Instrument));
                }
            }
        }

        void Watcher_MarketDataChanged(object sender, MarketDataEventArgs args)
        {
            if (args.Type == MarketDataEventType.DepthChangedWithNewTrade)
            {
                _logger.Trace(LogLevel.Method, "Watcher_MarketDataChanged. Instrument: {0}", args.Instrument);
                LastTradeUpdateMessage msg = _watcher.LastTrade;
                _logger.Trace(LogLevel.Method, "Watcher_MarketDataChanged. LastTradeUpdateMessage: {0}", msg.ToString());
                AddPointToChart(msg.Price);
            }
        }
      
        private void GraphForm_Load(object sender, EventArgs e)
        {
            InitGraph();
        }

        LineItem myCurve;
        private void InitGraph()
        {
            GraphPane myPane = this.zedGraphControl1.GraphPane;

            // Set the titles and axis labels
            myPane.Title.Text = "Last Traded Price";
            myPane.XAxis.Title.Text = "Time (sec)";
            myPane.YAxis.Title.Text = "Price";

            PointPairList list = new PointPairList();

            myCurve = myPane.AddCurve("Transaction Price", list, Color.Blue,
                                    SymbolType.None);
            // Fill the area under the curve with a white-red gradient at 45 degrees
            myCurve.Line.Fill = new Fill(Color.White, Color.Red, 45F);
            // Make the symbols opaque by filling them with white
            myCurve.Symbol.Fill = new Fill(Color.White);

            // Fill the axis background with a color gradient
            myPane.Chart.Fill = new Fill(Color.Black, Color.FromArgb(0, 0, 128), 45F);

            // Fill the pane background with a color gradient
            myPane.Fill = new Fill(Color.Black, Color.FromArgb(220, 220, 255), 45F);

            // Calculate the Axis Scale Ranges
            this.zedGraphControl1.AxisChange();
        }

        private readonly int MaxPointsInChart = 100;
        private delegate void AddPointToChartDelegate(double price);
        private DateTime _firstTime = DateTime.MinValue;
        private bool _firstPoint = true;
        private void AddPointToChart(double price)
        {            
            if (this.InvokeRequired)
            {
                this.Invoke(new AddPointToChartDelegate(AddPointToChart), price);
            }
            else
            {
                double y = price;
                double time = 0;
                if (_firstPoint)
                {
                    _firstPoint = false;
                    _firstTime = DateTime.Now;
                }
                else
                {
                    time = DateTime.Now.Subtract(_firstTime).TotalSeconds;
                }

                myCurve.AddPoint(time, y);
                if (myCurve.NPts > MaxPointsInChart)
                {
                    myCurve.RemovePoint(0);
                }

                this.zedGraphControl1.AxisChange();
                this.zedGraphControl1.Refresh();
            }
        }
    }
}
