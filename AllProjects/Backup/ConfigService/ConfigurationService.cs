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
using System.Threading;

using OPEX.Configuration.Common;

namespace OPEX.Configuration.Service
{
    /// <summary>
    /// Marshals a ConfigurationService and offers it
    /// remotely. Periodically reloads the configuration.
    /// </summary>
    public class ConfigurationService
    {
        private readonly string DefaultChannelName = "ConfigurationServiceChannel";
        private readonly int DefaultPort = 12000;
        private readonly int DefaultPollIntervalSec = 10;
        private readonly string DefaultUri = "ConfigurationService.rem";
        private readonly int PollIntervalSec = 10;
        private readonly ConfigurationProvider _provider;
        private readonly Thread _mainThread;
        private readonly ManualResetEvent _reset;
        private readonly AutoResetEvent _reload;
        private readonly string _channelName;
        private readonly string _uri;
        private readonly int _port;
        private bool _isMarshalled = false;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.Configuration.Service.ConfigurationService.
        /// </summary>
        /// <param name="provider">The ConfigurationProvider to associate
        /// to this ConfigurationService.</param>
        public ConfigurationService(ConfigurationProvider provider)
        {
            _provider = provider;
            _channelName = ConfigurationHelper.GetConfigSetting("CSChannelName", DefaultChannelName);
            _port = Int32.Parse(ConfigurationHelper.GetConfigSetting("CSPort", DefaultPort.ToString()));
            _uri = ConfigurationHelper.GetConfigSetting("CSUri", DefaultUri);
            PollIntervalSec = Int32.Parse(ConfigurationHelper.GetConfigSetting("PollInterval", DefaultPollIntervalSec.ToString()));
            _mainThread = new Thread(new ThreadStart(MainThread));
            _reset = new ManualResetEvent(false);
            _reload = new AutoResetEvent(false);
        }        

        private void Init()
        {
            BinaryServerFormatterSinkProvider formatter = new BinaryServerFormatterSinkProvider();
            TcpServerChannel channel = new TcpServerChannel(_channelName, _port, formatter);
            RemotingServices.Marshal(_provider, _uri);
        }

        /// <summary>
        /// Starts the ConfigurationService.
        /// </summary>
        public void Start()
        {
            _provider.Start();
            _mainThread.Start();
        }

        /// <summary>
        /// Stops the ConfigurationService.
        /// </summary>
        public void Stop()
        {
            _reset.Set();
            _mainThread.Join(7000);
            _provider.Stop();
        }

        private void MainThread()
        {
            WaitHandle[] EventArray = new WaitHandle[] {
                _reset,
                _reload
            };

            Timer t = new Timer(new TimerCallback(TimerExpired), null, TimeSpan.FromSeconds(PollIntervalSec), TimeSpan.FromMilliseconds(-1));

            while (WaitHandle.WaitAny(EventArray) != 0)
            {
                t.Change(Timeout.Infinite, Timeout.Infinite);
                bool loadOk =_provider.LoadConfigurationData();
                if (!_isMarshalled && loadOk)
                {
                    _isMarshalled = true;
                    Init();
                }
                t.Change(TimeSpan.FromSeconds(PollIntervalSec), TimeSpan.FromMilliseconds(-1));
            }

            t.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void TimerExpired(object state)
        {
            _reload.Set();
        }
    }
}
