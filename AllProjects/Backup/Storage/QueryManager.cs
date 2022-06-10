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
using System.Collections;
using System.Threading;

using MySql.Data;
using MySql.Data.MySqlClient;

using OPEX.Common;
using OPEX.Configuration.Client;

namespace OPEX.Storage
{
    /// <summary>
    /// Handles SQL queries in a queue fashion, and
    /// executes them on the specified MySqlCommection.
    /// </summary>
    public class QueryManager
    {
        private static readonly int MainThreadSleepMsec = 1000;
        private static readonly bool CommitAfterEveryCommand;
        private static long CommandsExecuted;

        private readonly object _root;
        private readonly Logger _logger;
        private readonly Thread _mainThread;
        private readonly ManualResetEvent _reset;
        private readonly Queue _commandQueue;
        private readonly MySqlConnection _connection;

        private MySqlCommand _command;
        private bool _running;

        /// <summary>
        /// Indicates whether QueryManager is running.
        /// </summary>
        public bool Running { get { return _running; } }

        static QueryManager()
        {
            CommandsExecuted = 0;
            CommitAfterEveryCommand = bool.Parse(ConfigurationClient.Instance.GetConfigSetting("CommitAfterEveryCommand", "false"));
        }

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.Storage.QueryManager.
        /// </summary>
        /// <param name="connection">The MySqlConnection to use to
        /// communicate with the DB.</param>
        public QueryManager(MySqlConnection connection)
        {
            _logger = new Logger("QueryManager");
            _connection = connection;
            _commandQueue = Queue.Synchronized(new Queue());
            _mainThread = new Thread(new ThreadStart(MainThread));
            _reset = new ManualResetEvent(false);
            _running = false;
            _root = new object();
            _command = new MySqlCommand();
            _command.Connection = _connection;
        }

        /// <summary>
        /// Enqueues a SQL query for execution.
        /// </summary>
        /// <param name="sql">The SQL query to enqueue for execution.</param>
        public void RunSQLCommand(string sql)
        {
            _commandQueue.Enqueue(sql);
        }

        /// <summary>
        /// Starts QueryManager.
        /// </summary>
        internal void Start()
        {
            lock (_root)
            {
                if (_running)
                {
                    _logger.Trace(LogLevel.Critical, "Cannot start: already running");
                    return;
                }

                _reset.Reset();
                _mainThread.Start();
            }
        }

        /// <summary>
        /// Stops QueryManager.
        /// </summary>
        internal void Stop()
        {
            lock (_root)
            {
                if (!_running)
                {
                    _logger.Trace(LogLevel.Critical, "Cannot stop: not running");
                    return;
                }

                _reset.Set();
                _mainThread.Join();
                try
                {
                    if (_command != null)
                    {
                        _command.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Trace(LogLevel.Critical, "Exception while disposing of MySqlCommand: {0}", ex.Message);
                }
            }
        }

        private void MainThread()
        {
            try
            {
                _running = true;
                _logger.Trace(LogLevel.Method, "MainThread started");

                while (!_reset.WaitOne(0) || _commandQueue.Count > 0)
                {
                    while (_commandQueue.Count > 0)
                    {
                        string sql = _commandQueue.Dequeue() as string;
                        InternalRunSQLCommand(sql);
                    }
                    Thread.Sleep(MainThreadSleepMsec);
                }
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "MainThread. Exception: {0}", ex.Message);
            }
            finally
            {
                _logger.Trace(LogLevel.Method, "MainThread ended");
                _running = false;
            }
        }

        private void InternalRunSQLCommand(string sql)
        {
            _command.CommandText = sql;
            try
            {
                CommandsExecuted++;
                _logger.Trace(LogLevel.Info, "[Command #{1}] About to run SQL {0}", sql, CommandsExecuted);
                _command.ExecuteNonQuery();
                _logger.Trace(LogLevel.Info, "[Command #{1}] SQL Command executed successfully {0}", sql, CommandsExecuted);
                if (CommitAfterEveryCommand)
                {
                    _command.CommandText = "COMMIT;";
                    _command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "[Command #{2}] Exception while executing SQL Command: {0} {1}", ex.Message, sql, CommandsExecuted);
            }
        }
    }
}
