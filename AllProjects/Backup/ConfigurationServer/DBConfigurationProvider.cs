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
using OPEX.Configuration.Common;
using OPEX.Configuration.Service;

namespace OPEX.ConfigurationServer
{
    /// <summary>
    /// Provides configuraion data, loading it from
    /// the specific DB set in the configuration file.
    /// </summary>
    class DBConfigurationProvider : ConfigurationProvider
    {
        private readonly static string DefaultDBSchemaName = "opex";
        private readonly static string DefaultDBUserName = "opex";
        private readonly static int DefaultDBPort = 3306;
        private readonly static string DefaultDBPassword = "password";
        private readonly static string DefaultDBHostName = "localhost";

        private Logger _logger;
        private string _database;
        private string _server;
        private string _user;
        private int _port;
        private string _password;
        private MySqlConnection _connection;
        private bool _isConnected;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.ConfigurationServer.DBConfigurationProvider.
        /// </summary>
        public DBConfigurationProvider()
            : base()
        {
            _logger = new Logger("DBConfigurationProvider");

            _database = ConfigurationHelper.GetConfigSetting("DBSchemaName", DefaultDBSchemaName);
            _user = ConfigurationHelper.GetConfigSetting("DBUserName", DefaultDBUserName);
            Int32.TryParse(ConfigurationHelper.GetConfigSetting("DBPort", DefaultDBPort.ToString()), out _port);
            _password = ConfigurationHelper.GetConfigSetting("DBPassword", DefaultDBPassword);
            _server = ConfigurationHelper.GetConfigSetting("DBHostName", DefaultDBHostName);
            _isConnected = false;
        }

        /// <summary>
        /// Starts the DBConfigurationProvider.
        /// </summary>
        public override void Start()
        {
            Connect();
        }

        /// <summary>
        /// Stops the DBConfigurationProvider.
        /// </summary>
        public override void Stop()
        {
            Disconnect();            
        }

        /// <summary>
        /// Loads the configuration data from a specific DB.
        /// </summary>
        /// <returns></returns>
        public override bool LoadConfigurationData()
        {
            bool allWell = false;

            _logger.Trace(LogLevel.Method, "Loading configuration data...");

            lock (_root)
            {

                try
                {
                    MySqlCommand cmd = new MySqlCommand(
                        "SELECT ApplicationName, ConfigKey, ConfigValue FROM Configuration;",
                        _connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string applicationName = reader["ApplicationName"].ToString();
                        string configKey = reader["ConfigKey"].ToString();
                        string configValue = reader["ConfigValue"].ToString();

                        _logger.Trace(LogLevel.Info, "Read entry: ApplicationName {0} ConfigKey {1} COnfigValue {2}", applicationName, configKey, configValue);

                        if (!_dataStore.ContainsKey(applicationName))
                        {
                            _dataStore[applicationName] = new ApplicationData(applicationName);
                        }
                        ApplicationData appData = _dataStore[applicationName];
                        appData.Data[configKey] = configValue;
                    }

                    reader.Close();

                    allWell = true;
                }
                catch (Exception ex)
                {
                    _logger.Trace(LogLevel.Critical, "Couldn't load configuration data: {0}", ex.Message);
                }
            }

            if (allWell)
            {
                _logger.Trace(LogLevel.Method, "Configuration data loaded.");
            }

            return allWell;
        }

        private string ConnectionString
        {
            get
            {
                return string.Format("server={0};database={2};port={3};user id={1};password={4};",
                    _server, _user, _database, _port, _password);
            }
        }

        private void Disconnect()
        {
            if (_isConnected)
            {
                _logger.Trace(LogLevel.Method,
                       "Disconnecting from DB {0} Server {1} User {2} Port {3}",
                       _database, _server, _user, _port);
                _connection.Close();
                _isConnected = false;
            }
        }

        private void Connect()
        {
            if (!_isConnected)
            {
                _logger.Trace(LogLevel.Method, "Connecting to database {0}", _database);

                _connection = new MySqlConnection(ConnectionString);
                _logger.Trace(LogLevel.Method,
                    "Connecting to DB {0} Server {1} User {2} Port {3}",
                    _database, _server, _user, _port);
                _connection.Open();
                _isConnected = true;
            }
        }
    }
}
