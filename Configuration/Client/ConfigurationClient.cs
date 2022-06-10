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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections.Generic;
using System.Net.Sockets;

using OPEX.Common;
using OPEX.Configuration.Common;

namespace OPEX.Configuration.Client
{
    /// <summary>
    /// Holds local and global application configuration
    /// data, handling a remote ConfigurationProvider via
    /// Remoting.
    /// </summary>
    public class ConfigurationClient
    {
        private static ConfigurationClient _theInstance = null;
        private static readonly object _root = new object();

        /// <summary>
        /// Gets the instance of the class OPEX.Configuration.Client.ConfigurationClient.
        /// </summary>
        public static ConfigurationClient Instance
        {
            get
            {
                lock (_root)
                {
                    if (_theInstance == null)
                    {
                        _theInstance = new ConfigurationClient();
                    }
                }
                return _theInstance;
            }
        }

        private readonly int RetryDelaySec = 3;
        private Logger _logger;
        private IConfigurationProvider _configurationProvider;
        private ApplicationData _myData;
        private ApplicationData _defaultData;

        private string _applicationName;
        private string _channelName;
        private string _serverHost;
        private int _port;
        private string _uri;

        protected ConfigurationClient()
        {
            _applicationName = ConfigurationHelper.GetConfigSetting("ApplicationName", "OPEXApplication");
            _channelName = ConfigurationHelper.GetConfigSetting("CSChannelName", "ConfigurationServiceChannel");
            _serverHost = ConfigurationHelper.GetConfigSetting("CSHost", "localhost");
            _port = Int32.Parse(ConfigurationHelper.GetConfigSetting("CSPort", "12000"));
            _uri = ConfigurationHelper.GetConfigSetting("CSUri", "ConfigurationService.rem");
            _myData = new ApplicationData(_applicationName);
            _defaultData = new ApplicationData("*");
            Init();
            Start();
        }

        /// <summary>
        /// Gets the local data dictionary.
        /// </summary>
        public Dictionary<string, string> Data { get { return _myData.Data; } }

        private void Start()
        {
            bool dataLoaded = false;

            _logger.Trace(LogLevel.Info, "Starting client...");

            while(!dataLoaded)
            {
                try
                {
                    ApplicationData myData = _configurationProvider.GetApplicationConfiguration(_applicationName);
                    ApplicationData defaultData = _configurationProvider.GetApplicationConfiguration("*");

                    if (myData != null)
                    {
                        _myData = myData;
                    }
                        
                    if (defaultData != null)
                    {
                        _defaultData = defaultData;
                    }

                    dataLoaded = true;
                }
                catch (SocketException sex)
                {
                    _logger.Trace(LogLevel.Warning, "SocketException while connecting to CS: {0}", sex.Message);
                    _logger.Trace(LogLevel.Info, "Retrying in {0} seconds...", RetryDelaySec);
                    System.Threading.Thread.Sleep(RetryDelaySec * 1000);
                }
                catch (Exception ex)
                {
                    _logger.Trace(LogLevel.Critical, "Exception while using remote object: {0}", ex.Message);
                }
            }

            _logger.Trace(LogLevel.Method, "CONFIGURATION CLIENT STARTED.");
        }

        /// <summary>
        /// Gets the configuration data of a specific application.
        /// </summary>
        /// <param name="applicationName">The name of the application to retrieve configuration data for.</param>
        /// <returns></returns>
        public Dictionary<string, string> GetApplicationData(string applicationName)
        {            
            ApplicationData data = _configurationProvider.GetApplicationConfiguration(applicationName);

            if (data == null)
            {
                return new Dictionary<string, string>();
            }
            else
            {
                return data.Data;
            }
        }

        /// <summary>
        /// Gets a local configuration setting.
        /// </summary>
        /// <param name="settingName">The name of the local configuration setting.</param>
        /// <param name="defaultValue">The default value to return in case the setting was not set.</param>
        /// <returns>The value of the setting, or the default value if none was set.</returns>
        public string GetConfigSetting(string settingName, string defaultValue)
        {
            string settingValue = defaultValue;

            if (_myData.Data.ContainsKey(settingName))
            {
                settingValue = _myData.Data[settingName];
            }
            else if (_defaultData.Data.ContainsKey(settingName))
            {
                settingValue = _defaultData.Data[settingName];
            }
            else
            {
                settingValue = ConfigurationHelper.GetConfigSetting(settingName, settingValue);
            }

            return settingValue;
        }

        private void Init()
        {
            _logger = new Logger(string.Format("ConfigurationClient({0})", _applicationName));

            _logger.Trace(LogLevel.Method, "Creating new client. ChannelName: {0} ServerHost {1} Port {2} Uri {3}",
                _channelName, _serverHost, _port, _uri);

            BinaryClientFormatterSinkProvider formatter = new BinaryClientFormatterSinkProvider();
            TcpClientChannel channel = new TcpClientChannel(_channelName, formatter);
            ChannelServices.RegisterChannel(channel, false);
            string serverUrl = string.Format("tcp://{0}:{1}/{2}", _serverHost, _port, _uri);
            _logger.Trace(LogLevel.Info, "ServerUrl: {0}", serverUrl);

            _logger.Trace(LogLevel.Info, "Creating remote object...");

            _configurationProvider = Activator.GetObject(
                typeof(IConfigurationProvider),
                serverUrl) as IConfigurationProvider;

            _logger.Trace(LogLevel.Info, "Remote object created.");
        }
    }
}
