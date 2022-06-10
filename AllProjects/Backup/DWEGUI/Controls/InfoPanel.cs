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

namespace OPEX.DWEGUI.Controls
{
    public partial class InfoPanel : UserControl
    {
        private readonly Logger _logger;

        public InfoPanel()
        {
            InitializeComponent();

            if (!DesignMode)
            {
                _logger = new Logger("InfoPanel");
            }
        }   

        public TextBox MessageTextBox { get { return txtMessage; } }

        public double PNLCurrent { set { SetPNL(value, true); } }
        public TimeSpan TimeLeft { set { UpdateTimer(value); } }
      
        delegate void UpdateDelegate();

        delegate void SafeUpdateTimer(TimeSpan remainingTime);
        private void UpdateTimer(TimeSpan remainingTime)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new SafeUpdateTimer(UpdateTimer), remainingTime);
                }
                else
                {
                    txtTime.Text = string.Format("{0:D2}:{1:D2}:{2:D2}",
                    (int)remainingTime.TotalHours % 24,
                    (int)remainingTime.TotalMinutes % 60,
                    (int)remainingTime.TotalSeconds % 60);
                }
            }
            catch { }
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
                txtPNL.Text = string.Format("£ {0}", value);

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

                txtPNL.Font = new Font(txtPNL.Font, fs);
                txtPNL.ForeColor = c;
            }
        }
    }
}
