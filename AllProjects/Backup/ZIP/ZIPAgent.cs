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
using System.Threading;

using OPEX.Common;
using OPEX.OM.Common;
using OPEX.Agents.Common;
using OPEX.MDS.Common;
using OPEX.ShoutService;
using OPEX.Configuration.Client;

namespace OPEX.Agents.ZIP
{    
    public class ZIPAgent : Agent
    {
        private ZIPPricer _pricer;        

        public ZIPAgent(string agentName, AgentWakeupMode wakeupMode, int sleepTimeMsec, int inactivitySleepTimeMsec, ParameterBag parameters)
            : base(agentName, wakeupMode, sleepTimeMsec, inactivitySleepTimeMsec, parameters)       
        { }        

        public override string Type
        {
            get { return "ZIP"; }
        }

        protected override void OnNewOrder()
        {
            _logger.Trace(LogLevel.Info, "OnNewOrder");
            ResetAgent(false);
            WorkOrder();
        }

        protected override void OnNewSimulationStarted()
        {
            _logger.Trace(LogLevel.Method, "OnNewSimulationStarted."); 
            ResetAgent(true);
            WorkOrder();
        }

        private void ResetAgent(bool force)
        {
            if (force || (_pricer == null))
            {
                _logger.Trace(LogLevel.Method, "ResetAgent. NEW PRICER CREATED! ==========================");
                _pricer = new ZIPPricer(this);
            }
            _pricer.ResetLimitPrice(); 
        }
       
        protected override void OnWakeUp(Stimulus stimulus)
        {
            bool workOrder = false;
            switch (stimulus.Type)
            {
                case StimulusType.Shout:
                    ShoutStimulus shoutStimulus = stimulus as ShoutStimulus;
                    Shout shout = shoutStimulus.Shout;
                    if (!shout.User.Equals(_agentName))
                    {
                        _logger.Trace(LogLevel.Debug, "OnWakeUp. SHOUT BY {0}", shout.User);
                    }
                    else
                    {
                        _logger.Trace(LogLevel.Debug, "OnWakeUp. SKIPPING MY OWN SHOUT");
                    }
                    _pricer.ReceiveShout(shout);
                    break;

                case StimulusType.Timer:
                    TimerStimulus ts = stimulus as TimerStimulus;
                    _logger.Trace(LogLevel.Debug, "OnWakeUp. TIMER: primary {0} WakeUp {1}", ts.Primary, WakeUp);
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

        protected override void OnOfflineShout(ShoutStimulus shoutStimulus)
        {
            if (this.AgentStatus != null && this.AgentStatus.CurrentOrder != null)
            {
                _pricer.ReceiveShout(shoutStimulus.Shout);
                _logger.Trace(LogLevel.Info, "OnOfflineShout. {0}", shoutStimulus.ToString());
            }
        }  

        private bool AdjustFromInactivity()
        {
            return _pricer.AdjustFromInactivity();
        }                      

        private void WorkOrder()
        {
            if (!IsActive)
            {
                return;
            }

            double price = _pricer.Price();
            SendOrAmendCurrentOrder(price); 
        }
    }
}
