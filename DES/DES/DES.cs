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

using OPEX.Common;
using OPEX.Storage;
using OPEX.StaticData;

using OPEX.DES.AA;
using OPEX.DES.GDX;
using OPEX.DES.ZIC;
using OPEX.DES.ZIP;
using OPEX.DES.HUMAN;
using OPEX.DES.OrderManager;
using OPEX.DES.Exchange;
using OPEX.DES.DB;

namespace OPEX.DES
{
    internal class DES : MainLogger
    {
        private readonly Dictionary<string, DESAgent> _agents;

        private Simulator _simulator;
        private JobLoader _jobLoader;
        private GlobalOrderBook _gob;

        public DES()
            : base("DES")
        {
            _agents = new Dictionary<string, DESAgent>();
        }

        public void Run()
        {
            bool atLeastOneHuman = false;
            try
            {
                DBConnectionManager.Instance.Connect();
                StaticDataManager.Instance.Load();
                _jobLoader = new JobLoader();
                _jobLoader.Load();

                _gob = new GlobalOrderBook();
                OrderFactory.OrderProcessor = _gob;
                atLeastOneHuman = InitAgents();
                _simulator.Start();
            }
            catch (Exception ex)
            {
                Trace(LogLevel.Critical, "Exception while starting DES: {0}", ex.Message);
                return;
            }

            if (!atLeastOneHuman)
            {
                Console.WriteLine("Press ENTER to stop the simulator");
                Console.ReadLine();
            }
            else
            {
                _simulator.WaitToFinish();
            }

            try
            {
                _simulator.Stop();
                DBConnectionManager.Instance.Disconnect();
            }
            catch (Exception ex)
            {
                Trace(LogLevel.Critical, "Exception while stopping AgentHost: {0}", ex.Message);
                return;
            }
        }
      
        private bool InitAgents()
        {
            bool atLeastOneHuman = false;
            foreach (AgentInfo agentInfo in StaticDataManager.Instance.AgentsStaticData.Agents.Values)
            {
                DESAgentWakeupMode wakeUpMode = GetWakeupMode(agentInfo);
                Type type = null;

                Trace(LogLevel.Info, "Creating agent {0} of type {1}", agentInfo.AgentName, agentInfo.AgentType);

                switch (agentInfo.AgentType)
                {
                    case "DESZIC":
                        type = typeof(DESZICAgent);
                        break;
                    case "DESZIP":
                        type = typeof(DESZIPAgent);
                        break;
                    case "DESAA":
                        type = typeof(DESAAAgent);
                        break;
                    case "DESGDX":
                        type = typeof(DESGDXAgent);
                        break;
                    case "DESHUMAN":
                        atLeastOneHuman = true;
                        type = typeof(DESHumanAgent);
                        break;
                    default:
                        Trace(LogLevel.Warning, "Couldn't create agent of type '{0}'", agentInfo.AgentType);
                        break;
                }

                if (type != null)
                {
                    _agents[agentInfo.AgentName] = 
                        Activator.CreateInstance(
                        type, 
                        agentInfo.AgentName, 
                        wakeUpMode, 
                        agentInfo.SleepTimeMsec, 
                        agentInfo.InactivityTimerSleepTimeMsec, 
                        agentInfo.Parameters,
                        _gob) as DESAgent;
                    Trace(LogLevel.Info, "Agent {0} of type {1} successfully created", agentInfo.AgentName, agentInfo.AgentType);                    
                }                
            }

            _simulator = new Simulator(_agents.Values, _jobLoader, _gob);

            foreach (DESAgent agent in _agents.Values)
            {
                Trace(LogLevel.Info, "Starting agent {0} of type {1}", agent.Name, agent.Type);
                agent.Init();
                Trace(LogLevel.Info, "Agent {0} of type {1} started", agent.Name, agent.Type);
            }

            return atLeastOneHuman;
        }

        private DESAgentWakeupMode GetWakeupMode(AgentInfo agentInfo)
        {
            DESAgentWakeupMode wakeUpMode = DESAgentWakeupMode.AlwaysSleep;
            if (agentInfo.WakeOnOrders)
            {
                wakeUpMode |= DESAgentWakeupMode.WakeUpOnOrders;
            }
            if (agentInfo.WakeOnTimer)
            {
                wakeUpMode |= DESAgentWakeupMode.WakeUpOnTimer;
            }
            if (agentInfo.WakeOnTrades)
            {
                wakeUpMode |= DESAgentWakeupMode.WakeUpOnTrades;
            }
            return wakeUpMode;
        }
    }
}
