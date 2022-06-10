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
using System.Collections.Generic;
using System.Text;

using OPEX.Common;
using OPEX.Agents.Common;
using OPEX.MDS.Client;
using OPEX.MDS.Common;

namespace OPEX.Agents.Sniper
{
    public class SniperAgent : Agent
    {
        private readonly int SecondsBeforeCloseToStartSniping;
        private SniperPricer _pricer;
        private Timer _snipeTimer;
        private bool _timerExpired;

        public SniperAgent(string agentName, AgentWakeupMode wakeupMode, int sleepTimeMsec, int inactivitySleepTimeMsec, ParameterBag parameters)
            : base(agentName, wakeupMode, sleepTimeMsec, inactivitySleepTimeMsec, parameters)
        {
            _timerExpired = false;
            SecondsBeforeCloseToStartSniping = (int)parameters.GetValue("SecondsBeforeCloseToStartSniping", typeof(int));
        }

        public override string Type
        {
            get { return "Sniper"; }
        }

        protected override void LoadSettings()
        {
            _parameters.AddDefaultValue("MaxAskSpreadFactor", "0.2");
            _parameters.AddDefaultValue("MaxBidSpreadFactor", "0.2");
            _parameters.AddDefaultValue("MinimumProfitFactor", "0.1");
            _parameters.AddDefaultValue("SecondsBeforeCloseToStartSniping", "10");
        }

        protected override void OnNewOrder()
        {
            _logger.Trace(LogLevel.Info, "OnNewOrder. BEGIN");

            if (_pricer == null)
            {
                _logger.Trace(LogLevel.Debug, "OnNewOrder. Creating new pricer");
                _pricer = new SniperPricer(this, _state.CurrentOrder);
            }
            _logger.Trace(LogLevel.Debug, "OnNewOrder. Calling SniperPricer.Init()");
            _pricer.Init();
            
            if (_timerExpired)
            {
                _logger.Trace(LogLevel.Debug, "OnNewOrder. TimerExpired: calling WorkOrder()");
                WorkOrder();
            }

            _logger.Trace(LogLevel.Info, "OnNewOrder. END");
        }      
        
        protected override void OnSession(SessionStimulus sessionStimulus)
        {
            SessionChangedEventArgs sessionInfo = sessionStimulus.SessionInfo;

            if (sessionInfo.SessionState == SessionState.Open)
            {
                DateTime currentSessionStartTime = sessionInfo.StartTime;
                DateTime currentSessionStopTime = sessionInfo.EndTime;
                TimeSpan sessionLength = currentSessionStopTime.Subtract(currentSessionStartTime);
                TimeSpan dueTime = sessionLength.Subtract(TimeSpan.FromSeconds(SecondsBeforeCloseToStartSniping));
                int dueTimeSec = (int)dueTime.TotalSeconds;
                _snipeTimer = new Timer(new TimerCallback(TimerExpired), null, dueTimeSec*1000, Timeout.Infinite);
                _logger.Trace(LogLevel.Method, "OnSession. Setting sniping timer to expire at {0}, that is in {1} seconds", 
                    DateTime.Now.Add(dueTime), dueTimeSec);
            }
            else
            {
                _timerExpired = false;
            }

            if (_pricer != null)
            {
                _pricer.SessionChanged(sessionStimulus);
            }
        }        

        private void TimerExpired(object state)
        {
            _logger.Trace(LogLevel.Info, "################ TimerExpired !!!! ###################");
            _timerExpired = true;
            if (_pricer != null && this.AgentStatus.CurrentAssignmentBucket != null)
            {
                WorkOrder();
            }
        }

        protected override void OnOfflineShout(ShoutStimulus stimulus)
        {
            _logger.Trace(LogLevel.Warning, "OnOfflineShout. {0}", stimulus.ToString());
            if (_pricer != null) 
            {
                _pricer.ReceiveShout(stimulus);
            }
        }

        protected override void OnWakeUp(Stimulus stimulus)
        {
            switch (stimulus.Type)
            {
                case StimulusType.Shout:
                    ShoutStimulus shoutStimulus = stimulus as ShoutStimulus;
                    _pricer.ReceiveShout(shoutStimulus);
                    break;
                case StimulusType.Timer:
                    break;
                default:
                    return;
            }
            WorkOrder();
        }

        private void WorkOrder()
        {
            bool success = false;
            double price = _pricer.Price(_timerExpired, out success);

            if (success)
            {
                SendOrAmendCurrentOrder(price);
            }         
        }
    }
}

