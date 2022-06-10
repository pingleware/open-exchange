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

using OPEX.DES.Exchange;
using OPEX.Common;

namespace OPEX.DES.HUMAN
{
    class DESHumanAgent : DESAgent
    {

        public DESHumanAgent(string agentName, DESAgentWakeupMode wakeupMode, int sleepTimeMsec, int inactivitySleepTimeMsec, ParameterBag parameters, GlobalOrderBook gob)
            : base(agentName, wakeupMode, sleepTimeMsec, inactivitySleepTimeMsec, parameters, gob)
        { 
        }

        public override string Type
        {
            get { return "DESHUMAN"; }
        }

        protected override void OnPlay(out bool newShout, out bool newDepth)
        {
            int p;            

            do
            {
                Console.Write("[{0}]> Enter a price: ", _agentName);                
            }
            while (!Int32.TryParse(Console.ReadLine(), out p));

            newShout = newDepth = SendOrAmendCurrentOrder(p);
        }

        protected override void OnNewOrder()
        {
            Console.WriteLine("[{0}]> A new order is ready for you to process: Side {1} Price {2}", _agentName, CurrentAssignment.Side.ToString(), CurrentAssignment.Price);
        }

        protected override void OnNewPhaseStarted()
        {
            Console.WriteLine("[{0}]> New PHASE STARTED", _agentName);
        }

        protected override void OnNewShout()
        {
            if (IsActive)
            {
                Console.WriteLine();
                Console.WriteLine("[{0}]> You received new market data.", _agentName);
                Console.WriteLine((LastMarketData.LastShout == null) ? "[NO SHOUT]" : LastMarketData.LastShout.ToString());
                Console.WriteLine(LastMarketData.LastSnapshot);
            }
        }
    }
}
