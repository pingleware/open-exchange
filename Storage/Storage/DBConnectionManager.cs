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

using MySql.Data;
using MySql.Data.MySqlClient;

using OPEX.Common;
using OPEX.Configuration.Client;

namespace OPEX.Storage
{
    /// <summary>
    /// Handles the connection to the MySql DB specified
    /// in the application coniguration file.
    /// </summary>
    public class DBConnectionManager
    {
        private readonly static string DefaultDBSchemaName = "opex";
        private readonly static string DefaultDBUserName = "opex";
        private readonly static int DefaultDBPort = 3306;
        private readonly static int DefaultCommandTimeout = 3600;
        private readonly static string DefaultDBPassword = "password";
        private readonly static string DefaultDBHostName = "localhost";
        private static DBConnectionManager TheInstance;
        private static object Root;
        
        private Logger _logger; 
        private bool _isConnected;
        private string _database;
        private string _server;
        private string _user;
        private int _port;
        private string _password;
        private MySqlConnection _connection;
        private QueryManager _queryManager;
        private int _defaultCommandTimeout;

        static DBConnectionManager()
        {
            Root = new object();
        }

        /// <summary>
        /// Gets the instance of the class OPEX.Storage.DBConnectionManager.
        /// </summary>
        public static DBConnectionManager Instance
        {
            get
            {
                lock (Root)
                {
                    if (TheInstance == null)
                    {
                        TheInstance = new DBConnectionManager();
                    }
                }
                return TheInstance;
            }
        }

        private DBConnectionManager() 
        { 
            _logger = new Logger("DBConnectionManager");

            _database = ConfigurationClient.Instance.GetConfigSetting("DBSchemaName", DefaultDBSchemaName);
            _user = ConfigurationClient.Instance.GetConfigSetting("DBUserName", DefaultDBUserName);
            Int32.TryParse(ConfigurationClient.Instance.GetConfigSetting("DBPort", DefaultDBPort.ToString()), out _port);
            _password = ConfigurationClient.Instance.GetConfigSetting("DBPassword", DefaultDBPassword);
            _server = ConfigurationClient.Instance.GetConfigSetting("DBHostName", DefaultDBHostName);
            Int32.TryParse(ConfigurationClient.Instance.GetConfigSetting("DBCmdTimeout", DefaultCommandTimeout.ToString()), out _defaultCommandTimeout);
        }

        /// <summary>
        /// Indicates whether the DBConnectionManager is connected.
        /// </summary>
        public bool IsConnected { get { return _isConnected; } }

        /// <summary>
        /// Gets the connection string to the DB.
        /// </summary>
        private string ConnectionString
        {
            get
            {
                return string.Format("server={0};database={2};port={3};user id={1};password={4};default command timeout={5};",
                    _server, _user, _database, _port, _password, _defaultCommandTimeout);
            }
        }

        /// <summary>
        /// Gets the MySqlConnection associated to the DBConnectionManager.
        /// </summary>
        public MySqlConnection Connection { get { return _connection; } }
        
        /// <summary>
        /// Gets the QueryManager associated to the DBConnectionManager.
        /// </summary>
        public QueryManager QueryManager { get { return _queryManager; } }

        /// <summary>
        /// Connects to the DB.
        /// </summary>
        public void Connect()
        {
            _logger.Trace(LogLevel.Method, "Connecting to database {0}", _database);

            if (_isConnected)
            {
                _logger.TraceAndThrow("Can't connect to database - already connected!");
            }

            _connection = new MySqlConnection(ConnectionString);
            try
            {
                _logger.Trace(LogLevel.Method,
                    "Connecting to DB {0} Server {1} User {2} Port {3}", 
                    _database, _server, _user, _port);
                _connection.Open();
                _isConnected = true;

                _queryManager = new QueryManager(_connection);
                _queryManager.Start();
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "Exception while connecting to DB: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Disconnects from the DB.
        /// </summary>
        public void Disconnect()
        {
            if (!_isConnected)
            {
                _logger.TraceAndThrow("Can't disconnect from database - not connected yet!");
            }

            if (_connection == null)
            {
                _logger.TraceAndThrow("Can't disconnect from database - connection null");
            }

            try
            {
                _queryManager.Stop();

                _logger.Trace(LogLevel.Method,
                    "Disconnecting from DB {0} Server {1} User {2} Port {3}",
                    _database, _server, _user, _port);
                _connection.Close();
                _isConnected = false;                
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "Exception while disconnecting from DB: {0}", ex.Message);
            }
        }
    }
}
