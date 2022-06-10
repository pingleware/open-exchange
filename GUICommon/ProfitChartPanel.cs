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
using System.Windows.Forms.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Design;

using OPEX.Common;
using OPEX.OM.Common;
using OPEX.TDS.Common;

using ZedGraph;

namespace OPEX.GUICommon
{        
    public partial class ProfitChartPanel : ChartPanel
    {
        public class ProfitEntry
        {
            private readonly double _maxThSurplus;
            private string _user;
            private double _profit;

            public ProfitEntry(string traderName, double maxThSurplus)
            {
                _user = traderName;
                _maxThSurplus = maxThSurplus;
            }

            public double MaxThSurplus { get { return _maxThSurplus; } }
            public string User { get { return _user; } }
            public double Profit { get { return _profit; } set { _profit = value; } }

            public double ExcessProfit { get { return Math.Max(0, _profit - _maxThSurplus); } }
            public double BasicProfit { get { return Math.Min(_maxThSurplus, _profit); } }
            public double RemainingProfit { get { return _maxThSurplus - BasicProfit; } }
        }

        private List<string> _traderNames = new List<string>();
        private List<double> _maxThSurplus = new List<double>();
        private PointPairList _remainingProfitList;
        private PointPairList _profitList;
        private PointPairList _excessProfitList;
        private List<ProfitEntry> _tradersProfit;
        private readonly Logger _logger;

        [Browsable(true)]
        public List<ProfitEntry> TradersProfit { get { return _tradersProfit; } }

        [Browsable(true)]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        public List<string> TraderNames { get { return _traderNames; } set { _traderNames = value; } }//{ "GDN_1_B", "GDN_2_B", "GDN_3_B", "GDN_1_S", "GDN_2_S", "GDN_3_S" };
        
        [Browsable(true)]
        public List<double> MaxThSurplus { get { return _maxThSurplus; } set { _maxThSurplus = value; } }//{ 175.0, 125.0, 75.0, 175.0, 125.0, 75.0 };

        public ProfitChartPanel()
        {
            InitializeComponent();

            _tradersProfit = new List<ProfitEntry>();

            _remainingProfitList = new PointPairList();
            _profitList = new PointPairList();
            _excessProfitList = new PointPairList();

            if (!this.IsDesignerHosted)
            {
                ResetBars();
                _logger = new Logger("ProfitChartPanel");
            }
        }

        private void ResetBars()
        {
            _tradersProfit.Clear();

            _profitList.Clear();
            _excessProfitList.Clear();
            _remainingProfitList.Clear();

            double[] zeroes = new double[_traderNames.Count];
            for (int i = 0; i < _traderNames.Count; ++i)
            {
                _tradersProfit.Add(new ProfitEntry(TraderNames[i], MaxThSurplus[i]));                
            }

            _remainingProfitList.Add(null, _maxThSurplus.ToArray());
            _excessProfitList.Add(null, zeroes);
            _profitList.Add(null, zeroes);
        }

        private bool IsMyTrader(string user)
        {
            foreach (ProfitEntry pe in _tradersProfit)
            {
                if (pe.User.Equals(user))
                {
                    return true;
                }
            }

            return false;
        }

        private void ReceiveProfitUpdate(string user, double profitJustMade)
        {                        
            int position = 0;
            ProfitEntry pe = null;
            foreach (ProfitEntry profitEntry in _tradersProfit)
            {
                _logger.Trace(LogLevel.Info, "ReceiveProfitUpdate. profitentry.user {2} user {0} position {1}", user, position, profitEntry.User);
                if (profitEntry.User.Equals(user))
                {
                    pe = profitEntry;
                    _logger.Trace(LogLevel.Info, "ReceiveProfitUpdate. FOUND!!! profitentry.user {2} user {0} position {1}", user, position, profitEntry.User);
                    break;
                }
                position++;
            }

            _logger.Trace(LogLevel.Info, "ReceiveProfitUpdate. User {0} profitJustMade {1} BasicProfit {2} RemainingProfit {3} ExcessProfit {4}",
                pe.User, profitJustMade, pe.BasicProfit, pe.RemainingProfit, pe.ExcessProfit);

            pe.Profit += profitJustMade;

            _logger.Trace(LogLevel.Info, "ReceiveProfitUpdate. NEW VALUES: User {0} profitJustMade {1} BasicProfit {2} RemainingProfit {3} ExcessProfit {4}",
              pe.User, profitJustMade, pe.BasicProfit, pe.RemainingProfit, pe.ExcessProfit);

            _profitList[position].Y = pe.BasicProfit;
            _remainingProfitList[position].Y = pe.RemainingProfit;
            _excessProfitList[position].Y = pe.ExcessProfit;

            if (pe.Profit > GraphPane.YAxis.Scale.Max)
            {
                /// TODO : RESCALE Y axis
            }
        }

        protected override bool SetupAxis()
        {            
            GraphPane myPane = GraphPane;

            myPane.XAxis.Title.Text = "Trader";
            myPane.XAxis.Type = AxisType.Text;
            if (_traderNames != null && _traderNames.Count > 0)
            {
                myPane.XAxis.Scale.TextLabels = _traderNames.ToArray();
            }

            myPane.YAxis.Title.Text = "Profit (GBp)";
            myPane.YAxis.Scale.Max = 300;
            myPane.YAxis.Scale.Min = 0;
            myPane.YAxis.Scale.MinorStep = 5;
            myPane.YAxis.Scale.MajorStep = 25;                       

            return true;
        }

        protected override void CreateChart()
        {
            GraphPane myPane = this.GraphPane;          

            BarItem bar = myPane.AddBar("Profit", _profitList, Color.LimeGreen);
            bar.Bar.Fill = new Fill(Color.LimeGreen, Color.Black, Color.LimeGreen);
            bar = myPane.AddBar("RemainingProfit", _remainingProfitList, Color.DarkRed);
            bar.Bar.Fill = new Fill(Color.DarkRed, Color.Black, Color.DarkRed);
            bar = myPane.AddBar("ExcessProfit", _excessProfitList, Color.LightGreen);
            bar.Bar.Fill = new Fill(Color.LightGreen, Color.Black, Color.LightGreen);

            myPane.Chart.Fill = new Fill(Color.Black, Color.DarkBlue, 45F);
            myPane.Fill = new Fill(Color.White, Color.FromArgb(220, 220, 255), 45F);

            myPane.BarSettings.Type = BarType.Stack;
        }

        protected override bool OnSessionOpen()
        {            
            ResetBars();

            _remainingProfitList.Add(null, _maxThSurplus.ToArray());

            return true;
        }

        protected override bool OnSessionClose()
        {
            return base.OnSessionClose();
        }

        protected override bool OnTradeMessageReceived(TradeDataMessage tradeDataMessage)
        {
            bool refresh = false;            
            string user = tradeDataMessage.User;
            double limitPrice = tradeDataMessage.LimitPrice;
            double price = tradeDataMessage.Price;
            OrderSide side = tradeDataMessage.Side;

            _logger.Trace(LogLevel.Info, "OnTradeMessageReceived. tradeDataMessage: {0}", tradeDataMessage.ToString());
             

            double profit = (tradeDataMessage.LimitPrice - tradeDataMessage.Price) * ((tradeDataMessage.Side == OrderSide.Buy) ? 1.0 : -1.0);

            _logger.Trace(LogLevel.Info, "OnTradeMessageReceived. PROFIT: {0}", profit);

            if (IsMyTrader(user))
            {
                refresh = true;
                ReceiveProfitUpdate(user, profit);
            }

            return refresh;
        }       
    }
}
