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
using System.Text;
using System.Threading;

namespace OPEX.Agents.Common
{
    public class TimerStimulusQueue : StimulusQueue
    {
        private readonly double JitterPlusMinorRangePercentage = 0.25;
        private static readonly Random _jitter = new Random();
        private readonly Timer _primaryTimer;
        private readonly Timer _secondaryTimer;
        private readonly int _sleepTimeMsec;
        private readonly int _inactivityTimerCycleMsec;
        private readonly int NumTimesInactivityTimerRings = 0;
        private int _n;

        public TimerStimulusQueue(string queueName, int sleepTimeMsec, int inactivityTimerCycleMsec)
            : base(queueName, StimulusType.Timer)
        {
            _inactivityTimerCycleMsec = inactivityTimerCycleMsec;

            if (_inactivityTimerCycleMsec > 0)
            {
                NumTimesInactivityTimerRings = sleepTimeMsec / inactivityTimerCycleMsec;
                if (NumTimesInactivityTimerRings * _inactivityTimerCycleMsec == sleepTimeMsec)
                {
                    NumTimesInactivityTimerRings = Math.Max(NumTimesInactivityTimerRings-1, 0);
                }
            }

            if (NumTimesInactivityTimerRings == 0)
            {
                _inactivityTimerCycleMsec = 0;
            }
            
            _sleepTimeMsec = sleepTimeMsec;
            _primaryTimer = new Timer(new TimerCallback(PrimaryTimerExpired));
            _secondaryTimer = new Timer(new TimerCallback(SecondaryTimerExpired));
        }

        public void ToggleTimer(bool start)
        {
            InnerToggleTimer(start, true);
            InnerToggleTimer(start, false);
        }

        public override void StartReceiving() 
        {
        }

        public override void StopReceiving() 
        {
            ToggleTimer(false); 
        }

        private int GetDueTimeMsec(bool primary)
        {
            int dueTimeMsec;

            if (primary)
            {
                double k = _jitter.NextDouble() * (2.0 * JitterPlusMinorRangePercentage) + 0.75;
                dueTimeMsec = (int)(k * _sleepTimeMsec);
            }
            else
            {
                dueTimeMsec = (NumTimesInactivityTimerRings > 0) ? _inactivityTimerCycleMsec : Timeout.Infinite;
            }

            return dueTimeMsec;
        }

        private void InnerToggleTimer(bool start, bool primary)
        {
            Timer timer = primary ? _primaryTimer : _secondaryTimer;
            
            int dueTime = (start) ? GetDueTimeMsec(primary) : Timeout.Infinite;
            timer.Change(dueTime, Timeout.Infinite);
        }

        private void PrimaryTimerExpired(object state)
        {
            Enqueue(new TimerStimulus(DateTime.Now, true));
        }

        private void SecondaryTimerExpired(object state)
        {
            Enqueue(new TimerStimulus(DateTime.Now, false));
            _n++;
            if (_n >= NumTimesInactivityTimerRings)
            {
                _n = 0;
                InnerToggleTimer(false, false);
            }
            else
            {
                InnerToggleTimer(true, false);
            }
        }
    }
}
