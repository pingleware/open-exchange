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

using OPEX.SupplyService.Common;
using OPEX.Storage;
using OPEX.Common;
using OPEX.OM.Common;

namespace OPEX.SupplyServer
{
    /// <summary>
    /// Loads Assignments from the DB.
    /// </summary>
    public class AssignmentLoader
    {
        private readonly Logger _logger;
        private readonly List<Assignment> _assignments;        

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.SupplyServer.AssignmentLoader.
        /// </summary>
        public AssignmentLoader()
        {
            _logger = new Logger("AssignmentLoader");
            _assignments = new List<Assignment>();
        }

        /// <summary>
        /// Loads Assignments from the DB for a specific session.
        /// </summary>
        /// <param name="sessionName">The session name for which to load assignments.</param>
        /// <returns>True, if the assignments for the given session name were successfully loaded. False otherwise.</returns>
        public bool Load(string sessionName)
        {
            bool allWell = false;
            DBConnectionManager dbConnectionManager = DBConnectionManager.Instance;

            _logger.Trace(LogLevel.Method, "Loading assignments");

            try
            {
                _assignments.Clear();

                _logger.Trace(LogLevel.Debug, "Connecting to DB...");
                dbConnectionManager.Connect();
                _logger.Trace(LogLevel.Info, "Connected to DB");

                MySqlCommand cmd = new MySqlCommand(
                    string.Format("SELECT ApplicationName, Phase, ID, Ric, Currency, Side, Quantity, Price FROM Assignments WHERE SessionName='{0}';", sessionName),
                    dbConnectionManager.Connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string applicationName = reader["ApplicationName"].ToString();
                    int phase = Int32.Parse(reader["Phase"].ToString());
                    int id = Int32.Parse(reader["ID"].ToString());
                    string ric = reader["Ric"].ToString();
                    string currency = reader["Currency"].ToString();
                    OrderSide side = (OrderSide)Enum.Parse(typeof(OrderSide), reader["Side"].ToString());
                    int quantity = Int32.Parse(reader["Quantity"].ToString());
                    double price = Double.Parse(reader["Price"].ToString());

                    Assignment assignment = new Assignment(
                        applicationName,
                        phase,
                        id,
                        ric,
                        currency,
                        side,
                        quantity,
                        price);

                    _logger.Trace(LogLevel.Info, "Read entry: {0}", assignment.ToString());

                    _assignments.Add(assignment);
                }

                reader.Close();

                allWell = true;
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "Exception while loading assignments: {0}", ex.Message);
            }
            finally
            {
                if (dbConnectionManager.IsConnected)
                {
                    _logger.Trace(LogLevel.Debug, "Disconnecting from DB...");
                    dbConnectionManager.Disconnect();
                    _logger.Trace(LogLevel.Info, "Disconnected from DB");
                }
            }

            _logger.Trace(LogLevel.Method, "Done loading assignments");

            return allWell;
        }

        /// <summary>
        /// Gets the Assignments that were loaded.
        /// </summary>
        public Assignment[] Assignments
        {
            get
            { 
                Assignment[] assignments = null;

                if (_assignments != null && _assignments.Count > 0)
                {
                    assignments = new Assignment[_assignments.Count];
                    _assignments.CopyTo(assignments, 0);
                }
                else
                {
                    return new Assignment[0];
                }

                return assignments;
            }
        }
    }
}
