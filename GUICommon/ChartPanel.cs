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

using OPEX.TDS.Client;
using OPEX.TDS.Common;
using OPEX.MDS.Common;
using OPEX.MDS.Client;

using ZedGraph;

namespace OPEX.GUICommon
{
    public partial class ChartPanel : UserControl
    {
        protected virtual int DefaultXMax { get { return 180; } }
        protected virtual double DefaultYMax { get { return 400.0; } }
        protected virtual double DefaultXAxisMinorStep { get { return 10.0; } }
        protected virtual double DefaultXAxisMajorStep { get { return 60.0; } }
        protected virtual double DefaultYAxisMinorStep { get { return 10.0; } }
        protected virtual double DefaultYAxisMajorStep { get { return 50.0; } }
        protected TimeSpan TimeSinceLastSessionOpen { get { return DateTime.Now.Subtract(_startTime); } }
        protected bool SessionOpen { get { return _sessionOpen; } }

        private int _xMax;
        private double _yMax;
        private double _xAxisMinorStep;
        private double _xAxisMajorStep;
        private double _yAxisMinorStep;
        private double _yAxisMajorStep;
        private bool _sessionOpen;
        private DateTime _startTime;    

        [Browsable(true)]
        protected double XAxisMinorStep { get { return _xAxisMinorStep; } set { _xAxisMinorStep = value; } }
        [Browsable(true)]
        protected double XAxisMajorStep { get { return _xAxisMajorStep; } set { _xAxisMajorStep = value; } }
        [Browsable(true)]
        protected double YAxisMinorStep { get { return _yAxisMinorStep; } set { _yAxisMinorStep = value; } }
        [Browsable(true)]
        protected double YAxisMajorStep { get { return _yAxisMajorStep; } set { _yAxisMajorStep = value; } }
        [Browsable(true)]
        public int XMax { get { return _xMax; } set { _xMax = value; } }
        [Browsable(true)]
        public double YMax { get { return _yMax; } set { _yMax = value; } }    
        [Browsable(true)]
        public string ChartTitle { get { return zgc1.GraphPane.Title.Text; } set { zgc1.GraphPane.Title.Text = value; } }
        [Browsable(true)]
        public string XAxisTitle { get { return zgc1.GraphPane.XAxis.Title.Text; } set { zgc1.GraphPane.XAxis.Title.Text = value; } }
        [Browsable(true)]
        public string YAxisTitle { get { return zgc1.GraphPane.YAxis.Title.Text; } set { zgc1.GraphPane.YAxis.Title.Text = value; } }        

        protected GraphPane GraphPane { get { return zgc1.GraphPane; } }

        /// <summary>
        /// Gets if the control is in design mode, or if any of its
        /// parents are in design mode.
        /// </summary>
        public bool IsDesignerHosted
        {
            get
            {
                Control ctrl = this;
                while (ctrl != null)
                {
                    if (ctrl.Site == null)
                        return false;
                    if (ctrl.Site.DesignMode == true)
                        return true;
                    ctrl = ctrl.Parent;
                }
                return false;
            }
        }

        public ChartPanel()
        {
            InitializeComponent();

            _xMax = DefaultXMax;
            _yMax = DefaultYMax;
            _xAxisMinorStep = DefaultXAxisMinorStep;
            _xAxisMajorStep = DefaultXAxisMajorStep;
            _yAxisMinorStep = DefaultYAxisMinorStep;
            _yAxisMajorStep = DefaultYAxisMajorStep;            
        }       

        protected virtual void CreateChart() { }
        protected virtual bool OnSessionOpen() { return false; }
        protected virtual bool OnSessionClose() { return false; }
        protected virtual bool OnTradeMessageReceived(TradeDataMessage tradeDataMessage) { return false; }  
        protected virtual bool SetupAxis() 
        {
            GraphPane myPane = GraphPane;

            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = _xMax;
            myPane.XAxis.Scale.MinorStep = _xAxisMinorStep;
            myPane.XAxis.Scale.MajorStep = _xAxisMajorStep;            

            myPane.YAxis.Scale.Min = 0;
            myPane.YAxis.Scale.Max = _yMax;
            myPane.YAxis.Scale.MinorStep = _yAxisMinorStep;
            myPane.YAxis.Scale.MajorStep = _yAxisMajorStep;

            return false;
        }

        private Form _parentForm;
        private void ChartPanel_Load(object sender, EventArgs e)
        {
            if (!IsDesignerHosted)
            {
                _parentForm = this.FindForm();
                if (_parentForm != null)
                {
                    _parentForm.FormClosing += new FormClosingEventHandler(ParentForm_FormClosing);
                }
                bool axisChange = SetupAxis();
                CreateChart();
                if (axisChange)
                {
                    zgc1.AxisChange();
                }
                HookEvents(true);                
            }            
        }

        void ParentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _parentForm.FormClosing -= new FormClosingEventHandler(ParentForm_FormClosing);
            HookEvents(false);
        }

        private void HookEvents(bool hook)
        {
            if (hook)
            {
                MarketDataClient.Instance.SessionChanged += new SessionChangedEventHandler(SessionChanged);
                TradeDataClient.Instance.TradeMessageReceived += new TradeMessageReceivedEventHandler(TradeMessageReceived);
            }
            else
            {
                MarketDataClient.Instance.SessionChanged -= new SessionChangedEventHandler(SessionChanged);
                TradeDataClient.Instance.TradeMessageReceived -= new TradeMessageReceivedEventHandler(TradeMessageReceived);
            }
        }

        private void SessionChanged(object sender, SessionChangedEventArgs sessionState)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SessionChangedEventHandler(SessionChanged), sender, sessionState);
            }
            else
            {
                bool refresh = false;

                if (sessionState.SessionState == SessionState.Open)
                {
                    _sessionOpen = true;
                    _startTime = DateTime.Now;
                    this.BackColor = Color.Green;
                    refresh = OnSessionOpen();
                }
                else
                {
                    _sessionOpen = false;
                    this.BackColor = Color.Red;
                    refresh = OnSessionClose();
                }

                if (refresh)
                {
                    RefreshChart();
                }
            }
        }

        private void TradeMessageReceived(object sender, TradeDataMessage tradeDataMessage)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new TradeMessageReceivedEventHandler(TradeMessageReceived), sender, tradeDataMessage);
            }
            else
            {
                if (OnTradeMessageReceived(tradeDataMessage))
                {
                    RefreshChart();
                }
            }
        }

        protected void RefreshChart()
        {
            this.zgc1.Invalidate();
        }
    }
}
