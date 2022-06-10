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
using System.Threading;
using OPEX.Common;
using Timer = System.Threading.Timer;

namespace OPEX.SalesGUI.Controls
{
    public partial class InfoPanel : UserControl
    {
        private readonly int TimerPeriod = 500;
        private DateTime _endTime;
        private readonly Logger _logger;
        private readonly Timer _timer;


        public InfoPanel()
        {
            InitializeComponent();

            if (!DesignMode)
            {
                _logger = new Logger("InfoPanel");
            }

            _timer = new System.Threading.Timer(new TimerCallback(timerCallback), null,
                Timeout.Infinite, Timeout.Infinite);               
        }      

        public TextBox MessageTextBox { get { return txtMessage; } }

        public int OrdersRemaining
        {
            set
            {                
                UpdateLabel(lblOrders, value.ToString());
            }
        }

        public int OrderExecuted
        {
            set
            {
                UpdateLabel(lblOrdersExecuted, value.ToString());
            }
        }

        public double PNL { set { SetPNL(value, false); } }
        public double PNLCurrent { set { SetPNL(value, true); } }
      
        public TimeSpan RemainingTime { get { return _endTime.Subtract(DateTime.Now); } }

        public DateTime StartTime
        {
            set
            {
                UpdateLabel(lblStartTime, value.ToString("HH:mm:ss"));                
            }
        }

        public DateTime EndTime
        {
            set
            {
                _endTime = value;

                UpdateLabel(lblEndTime, value.ToString("HH:mm:ss"));
                TimeSpan remainingTime = _endTime.Subtract(DateTime.Now);
                if (remainingTime.CompareTo(TimeSpan.Zero) > 0)
                {
                    StartSession();
                }
            }
        }

        public void Stop()
        {
            ToggleTimer(false);
        }

        delegate void UpdateLabelDelegate(Label lbl, string text);
        void UpdateLabel(Label lbl, string text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateLabelDelegate(UpdateLabel), lbl, text);
            }
            else
            {
                lbl.Text = text;
            }
        }

        private void StartSession()
        {
            lblMarket.Text = "Market OPEN";
            pnlMarket.BackColor = OpenColor;
            ToggleTimer(true);
        }

        private void EndSession()
        {
            lblTimeToEnd.Font = new Font(lblTimeToEnd.Font, FontStyle.Regular);
            lblTimeToEnd.ForeColor = Color.Black;
            ToggleTimer(false);
            pnlMarket.BackColor = CloseColor;
            lblMarket.Text = "Market CLOSED";
        }

        private void timerCallback(object state)
        {
            SafeTick();
        }

        private readonly int CountDownSeconds = 11;
        private readonly Color OpenColor = Color.PaleGreen;
        private readonly Color CloseColor = Color.IndianRed;              
        delegate void UpdateDelegate();
        private void SafeTick()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateDelegate(SafeTick));
            }
            else
            {
                TimeSpan remainingTime = RemainingTime;
                lblTimeToEnd.Text = string.Format("{0:D2}:{1:D2}:{2:D2}",
                    (int)remainingTime.TotalHours % 24,
                    (int)remainingTime.TotalMinutes % 60,
                    (int)remainingTime.TotalSeconds % 60);
                if (remainingTime.CompareTo(TimeSpan.FromSeconds(CountDownSeconds)) > 0)
                {
                    return;
                }

                if (remainingTime.CompareTo(TimeSpan.FromSeconds(1)) > 0)
                {
                    lblMarket.Text = "Market CLOSING...";
                    Color c = (pnlMarket.BackColor == OpenColor) ? CloseColor : OpenColor;
                    _logger.Trace(LogLevel.Debug, "timer_Tick. About to set pnlMarket.BackColor to {0}", c.ToString());
                    pnlMarket.BackColor = c;
                    _logger.Trace(LogLevel.Debug, "timer_Tick. pnlMarket.BackColor set to {0}", c.ToString());
                    pnlMarket.Invalidate();
                    lblTimeToEnd.Font = new Font(lblTimeToEnd.Font, FontStyle.Bold);
                }
                else
                {
                    EndSession();
                }
            }
        }

        delegate void SafeSetPNL(double value, bool current);
        private void SetPNL(double value, bool current)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SafeSetPNL(SetPNL), value, current);
            }
            else
            {
                Label label = current ? lblPNLCurrent : lblPNL;
                label.Text = string.Format("£{0}", value);

                FontStyle fs = FontStyle.Bold;
                Color c = Color.Black;

                if (value > 0)
                {
                    c = Color.Green;
                }
                else if (value < 0)
                {
                    c = Color.Red;
                }
                else
                {
                    fs = FontStyle.Regular;
                }

                label.Font = new Font(label.Font, fs);
                label.ForeColor = c;
            }
        }

        private void ToggleTimer(bool start)
        {
            if (start)
            {
                _timer.Change(TimeSpan.FromMilliseconds(TimerPeriod), TimeSpan.FromMilliseconds(TimerPeriod));
            }
            else
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
    }
}
