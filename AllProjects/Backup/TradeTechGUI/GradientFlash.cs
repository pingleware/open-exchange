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
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using Timer = System.Threading.Timer;

namespace OPEX.SalesGUI
{
    public class GradientFlash
    {
        private readonly double _T;
        private readonly double _alpha;
        private readonly Color _startingColor;
        private readonly Color _endingColor;
        private readonly double _beta;
        private readonly Timer _timer;
        private readonly Control _controlToFlash;
        private readonly int _refreshIntervalMsec;

        private double _Tplus;
        private double _Tminus;
        private double _A;
        private DateTime _tmrClicked;

        public GradientFlash(double totalTimeSec, int refreshIntervalMsec, double minimumGradient, Color startingColor, Color endingColor, double beta, Control controlToFlash)
        {
            _T = totalTimeSec;
            _alpha = minimumGradient;
            _startingColor = startingColor;
            _endingColor = endingColor;
            _controlToFlash = controlToFlash;
            _beta = beta;
            _refreshIntervalMsec = refreshIntervalMsec;
            _timer = new Timer(new TimerCallback(Tick), null, Timeout.Infinite, Timeout.Infinite);
        }

        public event EventHandler FlashFinished;

        public void StartFlash()
        {
            _tmrClicked = DateTime.Now;
            _Tplus = (1.0 - _beta) * _T;
            _Tminus = _beta * _T;
            _A = -(1.0 / _Tplus) * Math.Log(_alpha);
            _timer.Change(_refreshIntervalMsec, _refreshIntervalMsec);
        }

        public void StopFlash()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void Tick(object state)
        {
            ChangeColor();
        }

        private delegate void SafeChangeColor();
        private void ChangeColor()
        {
            if (_controlToFlash.InvokeRequired)
            {
                _controlToFlash.Invoke(new SafeChangeColor(ChangeColor));
            }
            else
            {
                TimeSpan timeElapsed = DateTime.Now.Subtract(_tmrClicked);
                if (timeElapsed.CompareTo(TimeSpan.FromSeconds(_T)) >= 0)
                {
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    _controlToFlash.BackColor = _endingColor;
                    if (FlashFinished != null)
                    {
                        FlashFinished(this, null);
                    }
                    return;
                }

                double t = timeElapsed.TotalSeconds;
                double F = ComputeF(t);

                _controlToFlash.BackColor = BlendColor(F);
            }
        }

        double ComputeF(double t)
        {
            double F;

            if (t < _Tminus)
            {
                F = 1.0;
            }
            else
            {
                F = Math.Exp(-_A * (t - _Tminus));
            }

            return F;
        }

        Color BlendColor(double x)
        {
            double r = (double)_startingColor.R * x + (double)_endingColor.R * (1.0 - x);
            double g = (double)_startingColor.G * x + (double)_endingColor.G * (1.0 - x);
            double b = (double)_startingColor.B * x + (double)_endingColor.B * (1.0 - x);
            return Color.FromArgb((int)r, (int)g, (int)b);
        }
    }
}
