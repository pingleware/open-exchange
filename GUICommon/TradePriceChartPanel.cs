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
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

using OPEX.TDS.Common;

using ZedGraph;

namespace OPEX.GUICommon
{
    public partial class TradePriceChartPanel : ChartPanel
    {
        private readonly int TimerInterval = 250;
        private readonly string PriceCurveLabel = "Trade Price";
        private readonly string AvgPriceCurveLabel = "Average Price";
        private double _runningSum = 0.0;
        private double _runningAvg = 0.0;
        private double _equilibriumPrice;
        private PointPairList _pricePointList;
        private PointPairList _avgPricePointList;
        private LineItem _priceCurve;
        private LineItem _avgPriceCurve;
        private LineObj _timeLine;
        private LineObj _equilibriumLine;
        private string _instrument = string.Empty;        

        [Browsable(true)]
        public string Instrument { get { return _instrument; } set { _instrument = value; ChartTitle = string.Format("Trade Price ({0})", _instrument); } }

        [Browsable(true)]
        public double EquilibriumPrice { get { return _equilibriumPrice; } set { _equilibriumPrice = value; } }      

        public TradePriceChartPanel()
        {
            InitializeComponent();            
        }

        protected override bool SetupAxis()
        {
            base.SetupAxis();

            GraphPane myPane = GraphPane;
            myPane.XAxis.Scale.MinorUnit = DateUnit.Second;

            return false;
        }

        protected override void CreateChart()
        {
            GraphPane myPane = this.GraphPane;

            _pricePointList = new PointPairList();

            _avgPricePointList = new PointPairList();
            _avgPriceCurve = myPane.AddCurve(AvgPriceCurveLabel, _avgPricePointList, Color.Red, SymbolType.None);
            _avgPriceCurve.Line.Width = 1.5F;
            _avgPriceCurve.Line.Style = DashStyle.Dash;
            _avgPriceCurve.Line.StepType = StepType.NonStep;            

            _priceCurve = myPane.AddCurve(PriceCurveLabel, _pricePointList, Color.Orange, SymbolType.Circle);
            _priceCurve.Symbol.Size = 2.5F;
            _priceCurve.Line.Width = 2.0F;
            _priceCurve.Line.Fill = new Fill(Color.DarkGray, Color.Blue, 45F);
            _priceCurve.Symbol.Fill = new Fill(Color.Orange);

            myPane.Chart.Fill = new Fill(Color.Black, Color.DarkBlue, 45F);
            myPane.Fill = new Fill(Color.White, Color.FromArgb(220, 220, 255), 45F);

            _timeLine = new LineObj(Color.Goldenrod, 0, YMax, 0, 0);
            _timeLine.ZOrder = ZOrder.A_InFront;
            _timeLine.Location.CoordinateFrame = CoordType.AxisXYScale;
            myPane.GraphObjList.Add(_timeLine);
            _timeLine.IsVisible = false;         
     
            _equilibriumLine = new LineObj(Color.Orange, 0, _equilibriumPrice, XMax, _equilibriumPrice);
            _equilibriumLine.ZOrder = ZOrder.E_BehindCurves;
            _equilibriumLine.Line.Width = 2.5F;
            _equilibriumLine.Line.Style = DashStyle.Dash;
            myPane.GraphObjList.Add(_equilibriumLine);
        }

        protected override bool OnTradeMessageReceived(TradeDataMessage tradeDataMessage)
        {
            bool refresh = false;

            if (SessionOpen && _instrument.Equals(tradeDataMessage.Instrument))
            {
                AddPoint(TimeSinceLastSessionOpen.TotalSeconds, tradeDataMessage.Price);
                refresh = true;
            }

            return refresh;
        }
        
        protected override bool OnSessionOpen()
        {            
            _pricePointList.Clear();
            _avgPricePointList.Clear();
            _timeLine.IsVisible = true;
            timer1.Interval = TimerInterval;
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Start();
            return true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int n = _avgPricePointList.Count;
            double x = TimeSinceLastSessionOpen.TotalSeconds;

            _timeLine.Location.X = x;            

            if (n > 0)
            {
                PointPair p = _avgPricePointList[n - 1];
                double y = p.Y;
                _avgPricePointList.Add(x, y);
                if (_avgPricePointList.Count >= 3)
                {
                    PointPair p1 = _avgPricePointList[n - 2];
                    if (p1.Y == p.Y)
                    {
                        _avgPricePointList.RemoveAt(n - 1);
                    }
                }
            }
            
            RefreshChart();
        }

        protected override bool OnSessionClose()
        {            
            timer1.Tick -= new EventHandler(timer1_Tick);
            timer1.Stop();           
            return true;
        }
      
        private void AddPoint(double x, double y)
        {
            if (x < XMax)
            {
                _pricePointList.Add(x, y);
                if (_pricePointList.Count == 1)
                {
                    _runningSum = 0;
                }
                _runningSum += y;
                _runningAvg = _runningSum / _pricePointList.Count;
                _avgPricePointList.Add(x, _runningAvg);
            }
        }
    }
}
