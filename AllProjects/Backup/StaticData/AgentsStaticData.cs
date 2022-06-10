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

using MySql.Data;
using MySql.Data.MySqlClient;

using OPEX.Storage;

namespace OPEX.StaticData
{
    /// <summary>
    /// Exposes static data of all the active agents.
    /// Retrieves the data from the DB.
    /// </summary>
    public class AgentsStaticData : IStaticData
    {
        private Dictionary<string, AgentInfo> _agents;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.StaticData.AgentsStaticData.
        /// </summary>
        public AgentsStaticData()
        {
            _agents = new Dictionary<string, AgentInfo>();
        }

        /// <summary>
        /// Gets a data dictionary of AgentInfo for
        /// all the active agents.
        /// </summary>
        public Dictionary<string, AgentInfo> Agents
        {
            get
            {
                return _agents;
            }
        }

        #region IStaticData Members

        /// <summary>
        /// Loads static data for all the active agents from the DB.
        /// </summary>
        public void Load()
        {
            if (!DBConnectionManager.Instance.IsConnected)
            {
                DBConnectionManager.Instance.Connect();
            }

            MySqlCommand cmd = new MySqlCommand("SELECT AgentName, AgentType, Owner, WakeOnTimer, WakeOnTrades, WakeOnOrders, SleepTimeMsec, InactivityTimerSleepTimeMsec, Parameters FROM Agents where Active = 'true';", DBConnectionManager.Instance.Connection);
            MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                string agentName = reader["AgentName"].ToString();
                string agentType = reader["AgentType"].ToString();
                string owner = reader["Owner"].ToString();
                bool wakeOnTimer = (bool)reader["WakeOnTimer"];
                bool wakeOnTrades = (bool)reader["WakeOnTrades"];
                bool wakeOnOrders = (bool)reader["WakeOnOrders"];
                int sleepTimeMsec = Int32.Parse(reader["SleepTimeMsec"].ToString());
                string parameters = reader["Parameters"].ToString();
                int inactivityTimerSleepTimeMsec = Int32.Parse(reader["InactivityTimerSleepTimeMsec"].ToString());

                _agents[agentName] = new AgentInfo(agentName, agentType, owner, wakeOnTimer, wakeOnTrades, wakeOnOrders, sleepTimeMsec, inactivityTimerSleepTimeMsec, parameters);
            }           

            reader.Close();
        }

        #endregion
    }
}
