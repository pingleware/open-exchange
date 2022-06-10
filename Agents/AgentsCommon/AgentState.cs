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

using OPEX.OM.Common;
using OPEX.MDS.Common;
using OPEX.SupplyService.Common;
using OPEX.MDS.Client;
using OPEX.StaticData;
using OPEX.DWEAS.Client;

namespace OPEX.Agents.Common
{
    public abstract partial class Agent
    {
        public class AgentState
        {
            private AggregatedDepthSnapshot _lastSnapshot;
            private LastTradeUpdateMessage _lastTrade;
            private Stimulus _currentStimulus;
            private OutgoingOrder _currentOrder;
            private AssignmentBucket _currentAssignment;
            private SessionChangedEventArgs _lastSessionInfo;
            private Instrument _currentInstrument;

            public Instrument CurrentInstrument
            {
                get { return _currentInstrument; }
                set { _currentInstrument = value; }
            }

            public SessionChangedEventArgs LastSessionInfo
            {
                get { return _lastSessionInfo; }
                set { _lastSessionInfo = value; }
            }

            public AggregatedDepthSnapshot LastSnapshot
            {
                get { return _lastSnapshot; }
                set { _lastSnapshot = value; }
            }
            public LastTradeUpdateMessage LastTrade
            {
                get { return _lastTrade; }
                set { _lastTrade = value; }
            }
            public Stimulus CurrentStimulus
            {
                get { return _currentStimulus; }
                set { _currentStimulus = value; }
            }
            public OutgoingOrder CurrentOrder
            {
                get { return _currentOrder; }
                set { _currentOrder = value; }
            }
            public AssignmentBucket CurrentAssignmentBucket
            {
                get { return _currentAssignment; }
                set { _currentAssignment = value; }
            }
        }
    }
}
