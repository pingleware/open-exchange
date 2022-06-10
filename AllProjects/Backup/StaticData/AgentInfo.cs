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

using OPEX.Common;

namespace OPEX.StaticData
{
    /// <summary>
    /// Contains all the static information that
    /// describe an Agent.
    /// </summary>
    public class AgentInfo
    {
        private readonly string _agentName;
        private readonly string _owner;
        private readonly string _agentType;
        private readonly bool _wakeOnTimer;
        private readonly bool _wakeOnTrades;
        private readonly bool _wakeOnOrders;
        private readonly int _sleepTimeMsec;
        private readonly int _inactivityTimerSleepTimeMsec;
        private readonly ParameterBag _parameterBag;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.StaticData.AgentInfo.
        /// </summary>
        /// <param name="agentName">The name of the agent.</param>
        /// <param name="agentType">The type of the agent.</param>
        /// <param name="owner">The name of the application that "owns" this agent.</param>
        /// <param name="wakeOnTimer">Indicates whether this agent wakes up when the primary timer expires.</param>
        /// <param name="wakeOnTrades">Indicates whether this agent wakes up when a new order is sent by any market participant to the market.</param>
        /// <param name="wakeOnOrders">Indicates whether this agent wakes up when a new order is sent by any market participant to the market.</param>
        /// <param name="sleepTimeMsec">Gets the sleep time of this agent.</param>
        /// <param name="inactivityTimerSleepTimeMsec">Gets the longest period of market inactivity after which the agent wakes up.</param>
        /// <param name="parameters">Gets the parameters of this agent.</param>
        public AgentInfo(string agentName, string agentType, string owner, bool wakeOnTimer, bool wakeOnTrades, bool wakeOnOrders, int sleepTimeMsec, int inactivityTimerSleepTimeMsec, string parameters)
        {
            _agentName = agentName;
            _agentType = agentType;
            _owner = owner;
            _wakeOnOrders = wakeOnOrders;
            _wakeOnTimer = wakeOnTimer;
            _wakeOnTrades = wakeOnTrades;
            _sleepTimeMsec = sleepTimeMsec;
            _inactivityTimerSleepTimeMsec = inactivityTimerSleepTimeMsec;

            _parameterBag = new ParameterBag(parameters);            
        }

        /// <summary>
        /// Gets the name of the agent.
        /// </summary>
        public string AgentName { get { return _agentName; } }

        /// <summary>
        /// Gets the type of the agent.
        /// </summary>
        public string AgentType { get { return _agentType; } }

        /// <summary>
        /// Gets the name of the application that "owns" this agent.
        /// </summary>
        public string Owner { get { return _owner; } }

        /// <summary>
        /// Indicates whether this agent wakes up when the primary timer expires.
        /// </summary>
        public bool WakeOnTimer { get { return _wakeOnTimer; } }

        /// <summary>
        /// Indicates whether this agent wakes up when a new order is sent
        /// by any market participant to the market.
        /// </summary>
        public bool WakeOnOrders { get { return _wakeOnOrders; } }

        /// <summary>
        /// Indicates whether this agent wakes up when a new trade is made
        /// between any two market participants.
        /// </summary>
        public bool WakeOnTrades { get { return _wakeOnTrades; } }

        /// <summary>
        /// Gets the sleep time of this agent.
        /// </summary>
        public int SleepTimeMsec { get { return _sleepTimeMsec; } }        

        /// <summary>
        /// Gets the longest period of market inactivity after which
        /// the agent wakes up.
        /// </summary>
        public int InactivityTimerSleepTimeMsec { get { return _inactivityTimerSleepTimeMsec; } }

        /// <summary>
        /// Gets the parameters of this agent.
        /// </summary>
        public ParameterBag Parameters { get { return _parameterBag; } }
    }
}
