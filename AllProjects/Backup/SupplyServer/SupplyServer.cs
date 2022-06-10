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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections.Generic;
using System.Text;

using OPEX.Common;
using OPEX.Configuration.Client;
using OPEX.Configuration.Common;

namespace OPEX.SupplyServer
{    
    /// <summary>
    /// Embeds a SupplyService.
    /// </summary>
    public class SupplyServer : MainLogger
    {
        private readonly SupplyService _supplyService;
        private readonly ConfigurationClient _configClient;               

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.SupplyServer.SupplyServer.
        /// </summary>
        public SupplyServer()
            : base("SupplyServer")
        {
            _configClient = ConfigurationClient.Instance;
            _supplyService = new SupplyService();
        }
       
        /// <summary>
        /// Runs SupplyServer.
        /// </summary>
        public void Run()
        {
            Trace(LogLevel.Method, "Supply server started");

            if (InitRemoting())
            {
                if (_supplyService.Start())
                {
                    Pause("Press ENTER to stop the Supply Server...");

                    _supplyService.Stop();
                }
            }

            Trace(LogLevel.Method, "Supply server terminated");
        }               

        private bool InitRemoting()
        {
            bool res = false;

            try
            {
                string channelName = ConfigurationClient.Instance.GetConfigSetting("SSChannelName", "SupplyServiceChannel");
                int port = Int32.Parse(ConfigurationClient.Instance.GetConfigSetting("SSPort", "12001"));
                string uri = ConfigurationClient.Instance.GetConfigSetting("SSUri", "SupplyService.rem");

                BinaryClientFormatterSinkProvider clientProvider = null;
                BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
                serverProvider.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
                IDictionary props = new Hashtable();
                props["port"] = port;
                props["typeFilterLevel"] = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
                TcpChannel chan = new TcpChannel(props, clientProvider, serverProvider);
                ChannelServices.RegisterChannel(chan, false);
                
                RemotingServices.Marshal(_supplyService, uri);
                res = true;
            }
            catch (Exception ex)
            {
                Trace(LogLevel.Critical, "Exception while initialising remoting: {0}", ex.Message);
            }

            return res;
        }       

        private void Pause(string message)
        {
            Console.WriteLine(message);
            Console.ReadLine();
        }
    }
}
