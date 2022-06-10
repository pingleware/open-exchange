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
using OPEX.OM.Common;
using OPEX.StaticData;
using OPEX.MDS.Client;
using OPEX.Configuration.Client;
using OPEX.Configuration.Common;
using OPEX.SupplyService.Common;
using OPEX.Storage;
using OPEX.ShoutService;

using OPEX.Agents.Common;
using OPEX.Agents.ZIC;
using OPEX.Agents.ZIP;
using OPEX.Agents.GD;
using OPEX.Agents.GDX;
using OPEX.Agents.Sniper;
using OPEX.Agents.AA;
using OPEX.AS.Service;

using OPEX.DWEAS.Client;
using NewAssignmentBatchReceivedEventHandler = OPEX.DWEAS.Client.NewAssignmentBatchReceivedEventHandler;

namespace AgentHost
{
    internal class AgentHost : MainLogger
    {
        private OutgoingOrderDuplexChannel _outChannel;
        private Dictionary<string, Agent> _agents;

        public AgentHost()
            : base("AgentHost")
        {
            _outChannel = new OutgoingOrderDuplexChannel(OrderFactory.OMClientName);
            OrderFactory.OrderSender = _outChannel;
            _agents = new Dictionary<string, Agent>();
        }

        public void Run()
        {
            try
            {
                DBConnectionManager.Instance.Connect();
                StaticDataManager.Instance.Load();

                _outChannel.Start();

                StartAgents();

                MarketDataClient.Instance.Start();
                MarketDataClient.Instance.RequestStatus(MarketDataClient.DefaultDataSource);

                System.Threading.Thread.Sleep(1000);

                DWEAssignmentClient.Instance.Start();                
            }
            catch (Exception ex)
            {
                Trace(LogLevel.Critical, "Exception while starting AgentHost: {0}", ex.Message);
                return;
            }

            Console.WriteLine("Press ENTER to stop AgentHost");
            Console.ReadLine();

            try
            {
                StopAgents();

                _outChannel.Stop();
                DWEAssignmentClient.Instance.Stop();
                MarketDataClient.Instance.Stop();
                DBConnectionManager.Instance.Disconnect();
            }
            catch (Exception ex)
            {
                Trace(LogLevel.Critical, "Exception while starting AgentHost: {0}", ex.Message);
                return;
            }
        }

     
        private void StopAgents()
        {
            foreach (Agent agent in _agents.Values)
            {
                Trace(LogLevel.Info, "Stopping agent {0} of type {1}", agent.Name, agent.Type);
                agent.Stop();
                Trace(LogLevel.Info, "Agent {0} of type {1} Stopped", agent.Name, agent.Type);
            }
        }

        private void StartAgents()
        {
            string appName = ConfigurationHelper.GetConfigSetting("ApplicationName", "OPEXApp");
            foreach (AgentInfo agentInfo in StaticDataManager.Instance.AgentsStaticData.Agents.Values)
            {
                if (!agentInfo.Owner.Equals(appName))
                {
                    continue;
                }

                AgentWakeupMode wakeUpMode = GetWakeupMode(agentInfo);                
                Type type = null;

                Trace(LogLevel.Info, "Creating agent {0} of type {1}", agentInfo.AgentName, agentInfo.AgentType);
                
                switch (agentInfo.AgentType)
                {
                    case "ZIC":
                        type = typeof(ZICAgent);                        
                        break;
                    case "ZIP":
                        type = typeof(ZIPAgent);                        
                        break;
                    case "GD":
                        type = typeof(GDAgent);                        
                        break;
                    case "Sniper":
                        type = typeof(SniperAgent);                        
                        break;                    
                    case "AA":
                        type = typeof(AAAgent);
                        break;
                    case "GDX":
                        type = typeof(GDXAgent);
                        break;
                    default:
                        Trace(LogLevel.Critical, "Couldn't create agent of type '{0}'", agentInfo.AgentType);
                        break;
                }

                if (type != null)
                {                    
                    _agents[agentInfo.AgentName] = Activator.CreateInstance(type, agentInfo.AgentName, wakeUpMode, agentInfo.SleepTimeMsec, agentInfo.InactivityTimerSleepTimeMsec, agentInfo.Parameters) as Agent;
                    Trace(LogLevel.Info, "Agent {0} of type {1} successfully created", agentInfo.AgentName, agentInfo.AgentType);
                    DWEAssignmentClient.Instance.Subscribe(agentInfo.AgentName);    
                }
            }

            foreach (Agent agent in _agents.Values)
            {
                Trace(LogLevel.Info, "Starting agent {0} of type {1}", agent.Name, agent.Type);
                agent.Start();
                Trace(LogLevel.Info, "Agent {0} of type {1} started", agent.Name, agent.Type);
            }
        }

        private AgentWakeupMode GetWakeupMode(AgentInfo agentInfo)
        {
            AgentWakeupMode wakeUpMode = AgentWakeupMode.AlwaysSleep;
            if (agentInfo.WakeOnOrders)
            {
                wakeUpMode |= AgentWakeupMode.WakeUpOnOrders;
            }
            if (agentInfo.WakeOnTimer)
            {
                wakeUpMode |= AgentWakeupMode.WakeUpOnTimer;
            }
            if (agentInfo.WakeOnTrades)
            {
                wakeUpMode |= AgentWakeupMode.WakeUpOnTrades;
            }
            return wakeUpMode;
        }
    }
}
