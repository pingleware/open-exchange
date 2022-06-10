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

using OPEX.Common;
using OPEX.Configuration.Client;
using OPEX.Configuration.Common;
using OPEX.MDS.Client;
using OPEX.MDS.Common;
using OPEX.SupplyService.Common;

namespace OPEX.SupplyServer
{
    /// <summary>
    /// Loads Assignments from the DB, and listens for
    /// SessionChanged events from MarketDataClient.
    /// Raises MessageArrived events when a session changes,
    /// distributing the Assignments of that phase.
    /// </summary>
    [Serializable]
    public class SupplyService : MarshalByRefObject, ISupplyMessageBroadcaster
    {        
        [NonSerialized]
        private readonly Logger _logger;
        [NonSerialized]
        private readonly string _sessionName;
        [NonSerialized]
        private readonly Dictionary<int, List<Assignment>> _assignmentsByPhase;

        [NonSerialized]
        private MarketDataClient MDClient;
        [NonSerialized]
        private int _currentPhase;
        [NonSerialized]
        private int _totalPhases;
        [NonSerialized]
        private SupplyMessageArrivedHandler _messageArrived;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.SupplyServer.SupplyService.
        /// </summary>
        public SupplyService()
        {
            _sessionName = ConfigurationClient.Instance.GetConfigSetting("SessionName", "Test");
            _logger = new Logger(string.Format("SupplyService({0})", _sessionName));
            _assignmentsByPhase = new Dictionary<int, List<Assignment>>();
            _currentPhase = 0;
            _totalPhases = 0;
        }

        /// <summary>
        /// Starts SupplyService.
        /// </summary>
        /// <returns>True, if SupplyService started successfully. False otherwise.</returns>
        public bool Start()
        {
            return InitMDClient() && LoadAssignments();
        }

        /// <summary>
        /// Stops SupplyService.
        /// </summary>
        /// <returns>True, if SupplyService stopped successfully. False otherwise.</returns>
        public bool Stop()
        {             
            return ShutDownMDClient();
        }

        #region ISupplyMessageBroadcaster Members        

        /// <summary>
        /// Occurs when a new SupplyMessage arrives.
        /// </summary>
        public event SupplyMessageArrivedHandler MessageArrived
        {
            add { _messageArrived += value; }
            remove { _messageArrived -= value; }
        }

        #endregion

        public override object InitializeLifetimeService()
        {
            return null;
        }

        private bool LoadAssignments()
        {
            AssignmentLoader loader = new AssignmentLoader();

            if (loader.Load(_sessionName))
            {
                foreach (Assignment a in loader.Assignments)
                {
                    int phase = a.Phase;
                    List<Assignment> assignmentList = null;

                    if (!_assignmentsByPhase.ContainsKey(phase))
                    {
                        _assignmentsByPhase[phase] = new List<Assignment>();
                    }
                    assignmentList = _assignmentsByPhase[phase];

                    assignmentList.Add(a);
                    if (phase > _totalPhases)
                    {
                        _totalPhases = phase;
                    }
                }

                return true;
            }

            return false;
        }

        private void MDClient_SessionChanged(object sender, SessionChangedEventArgs args)
        {
            _logger.Trace(LogLevel.Info, "TradingPeriod changed: {0}", args.SessionState.ToString());

            if (args.SessionState == SessionState.Close)
            {
                return;
            }
            
            List<Assignment> assignmentList = _assignmentsByPhase[_currentPhase];

            _logger.Trace(LogLevel.Info, "Current phase: {0}. Found {1} assignments for this phase.", _currentPhase, assignmentList.Count);

            foreach (Assignment a in assignmentList)
            {
                _logger.Trace(LogLevel.Info, "Sending assignment: {0}", a.ToString());
                SupplyMessage message = new SupplyMessage(a);
                SafeInvokeEvent(message);
            }

            if (_currentPhase == _totalPhases)
            {
                _currentPhase = 0;
            }
            else
            {
                _currentPhase++;
            }                        
        }

        private bool InitMDClient()
        {
            bool res = false;

            try
            {
                MDClient = MarketDataClient.Instance;
                MDClient.SessionChanged += new SessionChangedEventHandler(MDClient_SessionChanged);
                MDClient.Start();
                res = true;
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "Exception while starting MarketDataClient: {0}", ex.Message);
            }

            return res;
        }

        private bool ShutDownMDClient()
        {
            bool res = false;

            try
            {
                if (MDClient != null)
                {
                    MDClient.SessionChanged -= new SessionChangedEventHandler(MDClient_SessionChanged);
                    MDClient.Stop();
                }
                res = true;
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "Exception while starting MarketDataClient: {0}", ex.Message);
            }

            return res;
        }

        private void SafeInvokeEvent(SupplyMessage msg)
        {
            if (_messageArrived == null)
            {
                return;
            }

            SupplyMessageArrivedHandler messageArrivedHandler = null;
            foreach (Delegate del in _messageArrived.GetInvocationList())
            {
                try
                {
                    messageArrivedHandler = (SupplyMessageArrivedHandler)del;
                    messageArrivedHandler(msg);
                }
                catch (Exception ex)
                {
                    _logger.Trace(LogLevel.Critical, "Exception while broadcasting message: {0}", ex.Message);
                    _messageArrived -= messageArrivedHandler;
                }
            }
        }
    }
}
