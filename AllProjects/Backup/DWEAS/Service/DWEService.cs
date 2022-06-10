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
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OPEX.Configuration.Client;
using OPEX.Common;
using OPEX.Messaging;
using OPEX.AS.Service;
using OPEX.Storage;
using OPEX.SupplyService.Common;
using OPEX.MDS.Client;

namespace OPEX.DWEAS.Service
{
    public class DWEService : IOPEXService
    {
        private readonly DBConnectionManager _dbConnectionManager;        
        private readonly Logger _logger;
        private readonly AssignmentDispatcher _dispatcher;

        private ScheduleLoader _loader;
        private bool _hasStarted;

        public DWEService() 
        {
            _logger = new Logger("DWEService");
            _hasStarted = false;
            _dbConnectionManager = DBConnectionManager.Instance;
            _dispatcher = new AssignmentDispatcher();
        }

        #region IOPEXService Members
        
        public bool Start()
        {
            if (_hasStarted)
            {
                _logger.Trace(LogLevel.Critical, "Start. Can't start service because it's already started");
                return false;
            }

            try
            {
                _dbConnectionManager.Connect();
                if (!_dbConnectionManager.IsConnected)
                {
                    _logger.Trace(LogLevel.Error, "Start. Cannot connect to db server.");
                    return false;
                }

                MarketDataClient.Instance.SessionChanged += new SessionChangedEventHandler(Instance_SessionChanged);
                MarketDataClient.Instance.Start();
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Error, "Start. Exception while starting DWEService: {0}", ex.Message);
                return false;
            }

            try
            {
                _loader = new ScheduleLoader(_dbConnectionManager.Connection);
                if (!_loader.Load())
                {
                    _logger.Trace(LogLevel.Error, "Start. Couldn't load configuration.");
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.Trace(LogLevel.Critical, "Start. Exception: {0}", e.Message);
            }                                   

            return _hasStarted = true;
        }

        void Instance_SessionChanged(object sender, SessionChangedEventArgs sessionState)
        {
            if (_dispatcher != null)
            {
                _dispatcher.SessionChanged(sessionState);
            }
        }

        public bool Stop()
        {
            if (!_hasStarted)
            {
                _logger.Trace(LogLevel.Critical, "Stop. Can't stop service because hasn't started yet");
                return false;
            }

            _hasStarted = false;            

            try
            {

                MarketDataClient.Instance.Stop();
                MarketDataClient.Instance.SessionChanged -= new SessionChangedEventHandler(Instance_SessionChanged);

                if (_dbConnectionManager.IsConnected)
                {
                    _dbConnectionManager.Disconnect();
                }
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "Exception while stopping DWEService: {0} {1}", ex.Message, ex.StackTrace);
                return false;
            }

            return true;
        }

        #endregion        

        public List<int> ScheduleIDs { get { return _loader.ScheduleIDs; } }

        public void Play(int sid)
        {
            _dispatcher.Start(_loader.GetTimeTable(sid));
        }

        public void Pause()
        {
            _dispatcher.Stop();
        }

        public void ForceReload()
        {
            try
            {                
                if (!_loader.Load())
                {
                    _logger.Trace(LogLevel.Error, "ForceReload. Couldn't load configuration.");
                }
            }
            catch (Exception e)
            {
                _logger.Trace(LogLevel.Critical, "ForceReload. Exception: {0}", e.Message);
            }               
        }
    }
}
