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

namespace OPEX.TradingGUI.AlgoPanels
{
    public partial class StopAlgoPanel : VanillaAlgoPanel
    {
        public StopAlgoPanel()
        {
            InitializeComponent();
        }

        public double StopPrice
        {
            get
            {
                return (double)spinPrice.Value;
            }
            set
            {
                spinPrice.Value = (decimal)value;
            }
        }

        public override string Parameters
        {
            get
            {                
                return string.Format("StopPrice,{0:F4}", spinPrice.Value.ToString());
            }
            set
            {
                if (value == null || value.Length == 0)
                {
                    return;
                }

                string[] bits = value.Split(new char[] { ',' });
                if (bits == null || bits.Length != 2)
                {
                    return;
                }

                if (!bits[0].Equals("StopPrice"))
                {
                    return;
                }

                Decimal d = 0;
                if (!decimal.TryParse(bits[1], out d))
                {
                    return;
                }

                spinPrice.Value = d;
            }
        }

        public override string Destination
        {
            get
            {
                return "STOP";
            }
        }

        public override string AlgoName
        {
            get
            {
                return "StopLoss";
            }
        }
    }
}
