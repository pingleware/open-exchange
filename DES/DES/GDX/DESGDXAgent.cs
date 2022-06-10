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

using OPEX.Common;
using OPEX.DES.Exchange;

namespace OPEX.DES.GDX
{
    class DESGDXAgent : DESAgent
    {
        private DESGDXPricer _pricer;

        public DESGDXAgent(string agentName, DESAgentWakeupMode wakeupMode, int sleepTimeMsec, int inactivitySleepTimeMsec, ParameterBag parameters, GlobalOrderBook gob)
            : base(agentName, wakeupMode, sleepTimeMsec, inactivitySleepTimeMsec, parameters, gob)
        { 
        }

        public override string Type
        {
            get { return "DESGDX"; }
        }

        protected override void LoadSettings()
        {
            _parameters.AddDefaultValue("Alpha", "0.1");
            _parameters.AddDefaultValue("Gamma", "0.9");
            _parameters.AddDefaultValue("Tau", "15.0");
            _parameters.AddDefaultValue("WindowSize", "4");
            _parameters.AddDefaultValue("GracePeriodSecs", "5");
            _parameters.AddDefaultValue("ForceImprovePrice", "true");           
        }

        protected override void OnNewSimulationStarted()
        {
            _logger.Trace(LogLevel.Method, "OnNewSimulationStarted. Creating NEW PRICER ==========================");
            _pricer = new DESGDXPricer(this);
            _pricer.Init();
        }

        protected override void OnNewPhaseStarted()
        {
            _pricer.NewPhaseStarted();
        }

        protected override void OnNewOrder()
        {
            _logger.Trace(LogLevel.Info, "OnNewOrder. Initialising pricer.");
            _pricer.Init();
        }

        protected override void OnPlay(out bool newShout, out bool newDepth)
        {
            newShout = newDepth = false;
            if (IsActive)
            {
                if (_pricer.Price())
                {
                    double price = _pricer.LastPriceComputed;
                    newShout = newDepth = SendOrAmendCurrentOrder(price);
                }
            }    
        }

        protected override void OnNewShout()
        {
            _pricer.ReceiveShout(LastMarketData.LastShout);
        }
    }
}
