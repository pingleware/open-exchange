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
using System.Threading;

namespace OPEX.Common
{    
    /// <summary>
    /// Signals periodically, either at fixed or at pseudo-random time intervals.
    /// </summary>
    public class ComplexTimer
    {
        private bool _fixed;
        private int _fixedPeriodMsec;
        private int _signallingPeriodMinMsec;
        private int _signallingPeriodMaxMsec;
        private int _lastWaitTimeMsec;
        private ManualResetEvent _reset = new ManualResetEvent(false);
        private Thread _timerMainLoop;

        /// <summary>
        /// Event raised when the current signalling period has elapsed.
        /// </summary>
        public event EventHandler TimerElapsed;

        /// <summary>
        /// Gets a value indicating whether the signalling mode is fixed.
        /// </summary>
        public bool Fixed { get { return _fixed; } }

        /// <summary>
        /// Gets the length of the fixed signalling time period in milliseconds.
        /// </summary>
        public int FixedPeriodMsec { get { return _fixedPeriodMsec; } }

        /// <summary>
        /// Gets the lower boundary of the uniform distribution used for random signalling, in milliseconds.
        /// </summary>
        public int SignallingPeriodMinMsec { get { return _signallingPeriodMinMsec; } }

        /// <summary>
        /// Gets the upper boundary of the uniform distribution used for random signalling, in milliseconds.
        /// </summary>
        public int SignallingPeriodMaxMsec { get { return _signallingPeriodMaxMsec; } }

        /// <summary>
        /// Gets the length of the last signalling time period, in milliseconds.
        /// </summary>
        public int LastWaitTimeMsec { get { return _lastWaitTimeMsec; } }

        /// <summary>
        /// Initialises a new instance of the Opex.Common.ComplexTimer class.
        /// </summary>
        private ComplexTimer()
        {
            _timerMainLoop = new Thread(new ThreadStart(MainLoop));
            _lastWaitTimeMsec = 0;               
        }

        /// <summary>
        /// Initialises a new instance of the OPEX.Common.ComplexTimer class
        /// that will signal periodically, using the indicated signalling period.
        /// </summary>
        /// <param name="signallingPeriodMsec">The signalation period in milliseconds.</param>
        public ComplexTimer(int signallingPeriodMsec)
            : this()
        {
            _fixed = true;
            _fixedPeriodMsec = signallingPeriodMsec;
        }

        /// <summary>
        /// Initialises a new instance of the OPEX.Common.ComplexTimer class
        /// that will signal randomly, using the pseudo-uniform distribution
        /// defined by the specified boundaries.
        /// </summary>
        /// <param name="signallingPeriodMinMsec">The lower boundary of the pseudo-uniform distribution.</param>
        /// <param name="signallingPeriodMaxMsec">The upper boundary of the pseudo-uniform distribution.</param>
        public ComplexTimer(int signallingPeriodMinMsec, int signallingPeriodMaxMsec)
            : this()
        {
            _fixed = false;
            _signallingPeriodMaxMsec = signallingPeriodMaxMsec;
            _signallingPeriodMinMsec = signallingPeriodMinMsec;
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public void Start()
        {
            _reset.Reset();
            _timerMainLoop.Start();
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void Stop()
        {
            _reset.Set();
        }

        private int ComputeNextWait()
        {
            if (_fixed)
            {
                return _fixedPeriodMsec;
            }
            else
            {
                return new Random().Next(_signallingPeriodMinMsec, _signallingPeriodMaxMsec);
            }
        }
      
        private void MainLoop()
        {
            while (!_reset.WaitOne(0))
            {
                Thread.Sleep(_lastWaitTimeMsec = ComputeNextWait());
                if (TimerElapsed != null)
                {
                    TimerElapsed.BeginInvoke(this, null, null, null);
                }
            }
        }
    }
}
