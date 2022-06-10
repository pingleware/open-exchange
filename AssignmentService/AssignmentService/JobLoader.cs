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

using OPEX.Common;
using OPEX.SupplyService.Common;
using OPEX.Configuration.Client;
using OPEX.OM.Common;
using OPEX.Storage;

namespace OPEX.AS.Service
{
    /// <summary>
    /// Loads the configured SimulationJobs from the DB.
    /// </summary>
    public class JobLoader
    {        
        private readonly Logger _logger;    
        private readonly Dictionary<int, SimulationJob> _jobs;
        private readonly Dictionary<int, SimulationPhase> _phases;        
        private MySqlConnection _connection;                

        /// <summary>
        /// Initialises a new instance of OPEX.AS.Service.JobLoader.
        /// </summary>
        /// <param name="connection">The MySqlConnection to use to connect to the DB.</param>
        public JobLoader(MySqlConnection connection)
        {
            _logger = new Logger("JobLoader");

            _connection = connection;
            _jobs = new Dictionary<int, SimulationJob>();
            _phases = new Dictionary<int, SimulationPhase>();
        }

        /// <summary>
        /// Gets the SimulationJobs.
        /// </summary>
        public Dictionary<int, SimulationJob> Jobs { get { return _jobs; } }

        /// <summary>
        /// Loads the SimulationJobs.
        /// </summary>
        /// <returns>True if the SimulationJobs were loaded successfully. False otherwise.</returns>
        public bool Load()
        {
            bool allWell = false;

            _logger.Trace(LogLevel.Method, "Loading configuration data...");

            try
            {
                bool loadSuccess = false;              

                loadSuccess = LoadPhases() && LoadDescription() && LoadSteps() && LoadDetails();

                allWell = loadSuccess;
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "Couldn't load configuration data: {0}", ex.Message);
            }

            if (allWell)
            {
                _logger.Trace(LogLevel.Method, "Configuration data loaded.");
            }

            return allWell;
        }

        private bool LoadPhases()
        {
            MySqlCommand cmd = new MySqlCommand(
                              "SELECT PID, AID, ApplicationName, Ric, Currency, Side, Quantity, Price FROM SimulationPhases",
                              _connection);
            MySqlDataReader reader = cmd.ExecuteReader();

            _phases.Clear();

            while (reader.Read())
            {
                int phase = Int32.Parse(reader["PID"].ToString());
                int id = Int32.Parse(reader["AID"].ToString());
                string applicationName = reader["ApplicationName"].ToString();
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

                if (!_phases.ContainsKey(phase))
                {
                    _phases[phase] = new SimulationPhase(phase);
                }
                SimulationPhase currentPhase = _phases[phase];
                currentPhase.AddAssignment(assignment);
            }

            reader.Close();

            return true;
        }

        private bool LoadSteps()
        {
            MySqlCommand cmd = new MySqlCommand(
                             "SELECT JID, SID, Repetitions FROM SimulationJobSteps",
                             _connection);
            MySqlDataReader reader = cmd.ExecuteReader();            

            while (reader.Read())
            {
                int jobID = Int32.Parse(reader["JID"].ToString());
                int stepID = Int32.Parse(reader["SID"].ToString());
                int repeat = Int32.Parse(reader["Repetitions"].ToString());
                
                SimulationStep step = new SimulationStep(jobID, stepID, repeat);
                SimulationJob job = _jobs[jobID];
                job.AddStep(step);

                _logger.Trace(LogLevel.Info, "Read entry: {0}", step.ToString());            
            }

            reader.Close();

            return true;
        }

        private bool LoadDescription()
        {
            MySqlCommand cmd = new MySqlCommand(
                             "SELECT JID, JobName, JobDescription FROM SimulationJobDescription",
                             _connection);
            MySqlDataReader reader = cmd.ExecuteReader();

            _jobs.Clear();

            while (reader.Read())
            {
                int jobID = Int32.Parse(reader["JID"].ToString());
                string jobName = reader["JobName"].ToString();
                string jobDescription = reader["JobDescription"].ToString();

                SimulationJob job = new SimulationJob(jobID, jobName, jobDescription);
                _jobs[jobID] = job;

                _logger.Trace(LogLevel.Info, "Read entry: {0}", job.ToString());
            }

            reader.Close();

            return true;
        }

        private bool LoadDetails()
        {
            MySqlCommand cmd = new MySqlCommand(
                             "SELECT JID, SID, SubSID, PID FROM SimulationJobDetails",
                             _connection);
            MySqlDataReader reader = cmd.ExecuteReader();            

            while (reader.Read())
            {
                int jobID = Int32.Parse(reader["JID"].ToString());
                int stepID = Int32.Parse(reader["SID"].ToString());
                int subStepID = Int32.Parse(reader["SubSID"].ToString());
                int phaseID = Int32.Parse(reader["PID"].ToString());
                
                SimulationJob job = _jobs[jobID];
                SimulationStep step = job.Steps[stepID];
                SimulationPhase phase = _phases[phaseID];
                step.AddPhase(subStepID, phase);

                _logger.Trace(LogLevel.Info, "Read entry: JID {0} SID {1} SubSID {2} PID {3}", jobID, stepID, subStepID, phaseID);            
            }

            reader.Close();

            return true;
        }        
    }
}
