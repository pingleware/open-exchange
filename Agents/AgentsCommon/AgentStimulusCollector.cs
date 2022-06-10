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

namespace OPEX.Agents.Common
{
    public class AgentStimulusCollector : StimulusCollector
    {
        private readonly string _agentName;
        private readonly Dictionary<StimulusType, string> _queueNames;

        public AgentStimulusCollector(string agentName, int sleepTimeMsec, int inactivityTimerCycleMsec)     
            : base(string.Format("Collector({0})", agentName))
        {
            _agentName = agentName;
            _queueNames = new Dictionary<StimulusType, string>();

            foreach (StimulusType type in Enum.GetValues(typeof(StimulusType)))
            {
                _queueNames[type] = string.Format("{0}({1})", type.ToString(), _agentName);
            }

            AddQueue(new TimerStimulusQueue(_queueNames[StimulusType.Timer], sleepTimeMsec, inactivityTimerCycleMsec));
            AddQueue(new NewOrderStimulusQueue(_queueNames[StimulusType.NewOrder], _agentName));
            AddQueue(new OrderStatusStimulusQueue(_queueNames[StimulusType.OrderStatus]));
            AddQueue(new SessionStimulusQueue(_queueNames[StimulusType.Session]));
            AddQueue(new ShoutStimulusQueue(_queueNames[StimulusType.Shout]));
        }

        public TimerStimulusQueue TimerQueue { get { return _inputQueues[_queueNames[StimulusType.Timer]] as TimerStimulusQueue; } }
        public NewOrderStimulusQueue NewOrderQueue { get { return _inputQueues[_queueNames[StimulusType.NewOrder]] as NewOrderStimulusQueue; } }
        public OrderStatusStimulusQueue OrderStatusQueue { get { return _inputQueues[_queueNames[StimulusType.OrderStatus]] as OrderStatusStimulusQueue; } }
        public SessionStimulusQueue SessionQueue { get { return _inputQueues[_queueNames[StimulusType.Session]] as SessionStimulusQueue; } }
        public ShoutStimulusQueue ShoutQueue { get { return _inputQueues[_queueNames[StimulusType.Shout]] as ShoutStimulusQueue; } }        
    }
}
