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

using MySql.Data;
using MySql.Data.MySqlClient;

using OPEX.Common;
using OPEX.OM.Common;

namespace OPEX.DWEAS
{
    class ScheduleLoader
    {
        private readonly Logger _logger;
        private readonly MySqlConnection _connection;
        private readonly Dictionary<int, SortedDictionary<int, List<ScheduleEntry>>> _timeTables;

        public ScheduleLoader(MySqlConnection connection)
        {
            _connection = connection;
            _logger = new Logger("ScheduleLoader");            
            _timeTables = new Dictionary<int, SortedDictionary<int, List<ScheduleEntry>>>();
        }

        public List<int> ScheduleIDs { get { return _timeTables.Keys.ToList<int>(); } }

        public SortedDictionary<int, List<ScheduleEntry>> GetTimeTable(int sid)
        { 
            return _timeTables[sid]; 
        }

        public bool Load()
        {
            bool allWell = false;

            _logger.Trace(LogLevel.Method, "Load. Loading configuration data.");

            try
            {
                allWell = LoadSchedule();
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "Load. Couldn't load configuration data: {0}", ex.Message);
            }

            if (allWell)
            {
                _logger.Trace(LogLevel.Method, "Load. Configuration data loaded.");
            }

            return allWell;
        }

        private bool LoadSchedule()
        {
            MySqlCommand cmd = new MySqlCommand(
                @"select s.SID, s.Step, s.UserName, p.Price, p.Side, p.RIC, p.CCY, p.Qty, t.WaitTime
                from DWESchedule s, DWEPermits p, DWETimeTable t
                where s.SID = p.SID
                and s.SID = t.SID
                and s.Step = t.Step
                and p.UserName = s.UserName
                and p.PermitType = s.PermitType
                order by s.SID, s.Step asc;",
                _connection);
            MySqlDataReader reader = cmd.ExecuteReader();

            _timeTables.Clear();

            while (reader.Read())
            {
                try
                {
                    int sid = Int32.Parse(reader["SID"].ToString());
                    int step = Int32.Parse(reader["Step"].ToString());
                    double time = Double.Parse(reader["WaitTime"].ToString());
                    string userName = reader["UserName"].ToString();
                    double price = Double.Parse(reader["Price"].ToString());
                    OrderSide side = (OrderSide)Enum.Parse(typeof(OrderSide), reader["Side"].ToString());
                    string ric = reader["RIC"].ToString();
                    string currency = reader["CCY"].ToString();
                    int quantity = Int32.Parse(reader["Qty"].ToString());

                    if (!_timeTables.ContainsKey(sid))
                    {
                        _timeTables.Add(sid, new SortedDictionary<int, List<ScheduleEntry>>());
                    }
                    SortedDictionary<int, List<ScheduleEntry>> timeTable = _timeTables[sid];

                    ScheduleEntry entry = 
                        new ScheduleEntry(step, time, userName, price, side, ric, currency, quantity);

                    _logger.Trace(LogLevel.Info, "LoadSchedule. Read entry: {0}", entry.ToString());

                    if (!timeTable.ContainsKey(step))
                    {
                        timeTable.Add(step, new List<ScheduleEntry>());
                    }
                    List<ScheduleEntry> l = timeTable[step];
                    l.Add(entry);
                }
                catch (Exception ex)
                {
                    _logger.Trace(LogLevel.Critical, "LoadSchedule. Exception: {0}", ex.Message);
                }
            }

            reader.Close();

            return true;
        }
    }
}
