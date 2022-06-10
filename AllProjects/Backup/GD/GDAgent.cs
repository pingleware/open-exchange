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

using OPEX.Common;
using OPEX.Agents.Common;

namespace OPEX.Agents.GD
{
    public class GDAgent : Agent
    {      
        private GDPricer _pricer = null;

        public GDAgent(string agentName, AgentWakeupMode wakeupMode, int sleepTimeMsec, int inactivitySleepTimeMsec, ParameterBag parameters)
            : base(agentName, wakeupMode, sleepTimeMsec, inactivitySleepTimeMsec, parameters)
        {}

        public override string Type
        {
            get { return "GD"; }
        }

        protected override void LoadSettings()
        {
            _parameters.AddDefaultValue("Alpha", "0.1");
            _parameters.AddDefaultValue("WindowSize", "8");
            _parameters.AddDefaultValue("GracePeriodSecs", "5");            
        }     

        protected override void OnNewOrder()
        {
            _logger.Trace(LogLevel.Info, "OnNewOrder");
            ResetAgent(false);
        }

        protected override void OnNewSimulationStarted()
        {
            ResetAgent(true);
        }      

        protected override void OnWakeUp(Stimulus stimulus)
        {
            bool workOrder = false;

            switch (stimulus.Type)
            {
                case StimulusType.Shout:
                    ShoutStimulus shout = stimulus as ShoutStimulus;
                    _pricer.ReceiveShout(shout.Shout);
                    workOrder = _pricer.Price();
                    break;
                case StimulusType.Timer:
                    TimerStimulus ts = stimulus as TimerStimulus;
                    if (ts.Primary)
                    {
                        workOrder = AdjustFromInactivity();
                    }
                    else
                    {
                        AdjustFromInactivity();
                    }
                    break;
                default:
                    return;
            }

            if (WakeUp && workOrder)
            {
                WorkOrder();
            }        
        }

        protected override void OnSession(SessionStimulus sessionStimulus)
        {
            if (_pricer != null)
            {
                _pricer.ResetBestPrices();
            }
        }       

        private void WorkOrder()
        {
            double price = _pricer.LastPriceComputed;

            SendOrAmendCurrentOrder(price);
        }
      
        private bool AdjustFromInactivity()
        {
            return _pricer.Price();            
        }

        private void ResetAgent(bool force)
        {
            if (force || (_pricer == null))
            {
                _logger.Trace(LogLevel.Method, "ResetAgent. NEW PRICER CREATED! ==========================");
                _pricer = new GDPricer(this);
            }
            _pricer.Init();
        }
    }
}
