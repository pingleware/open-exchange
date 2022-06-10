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

namespace OPEX.MDS.Common
{    
    /// <summary>
    /// Represents a message that carries session information.
    /// </summary>
    [Serializable]
    public class MarketDataSessionMessage : MarketDataMessage
    {
        private readonly SessionState _sessionState;
        private readonly DateTime _startTime;
        private readonly DateTime _endTime;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.MDS.Common.MarketDataSessionMessage.
        /// </summary>
        /// <param name="sessionState">The state of the exchange session.</param>
        /// <param name="startTime">The start time of the exchange session.</param>
        /// <param name="endTime">The end time of the exchange session.</param>
        /// <param name="dataSource">The data source associated to this message.</param>
        public MarketDataSessionMessage(SessionState sessionState, DateTime startTime, DateTime endTime, string dataSource)            
            : base(MarketDataMessageType.Status, "*", dataSource)
        {
            _sessionState = sessionState;
            _startTime = startTime;
            _endTime = endTime;
        }

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.MDS.Common.MarketDataSessionMessage.
        /// </summary>
        /// <param name="sessionState">The state of the exchange session.</param>
        /// <param name="startTime">The start time of the exchange session.</param>
        /// <param name="endTime">The end time of the exchange session.</param>
        /// <param name="dataSource">The data source associated to this message.</param>
        /// <param name="origin">The data origin associated to this message.</param>
        public MarketDataSessionMessage(SessionState sessionState, DateTime startTime, DateTime endTime, string dataSource, string origin)
            : this(sessionState, startTime, endTime, dataSource)
        {
            _origin = origin;
        }

        /// <summary>
        /// Gets the state of the exchange session.
        /// </summary>
        public SessionState SessionState { get { return _sessionState; } }

        /// <summary>
        /// Gets the start time of the exchange session.
        /// </summary>
        public DateTime StartTime { get { return _startTime; } }


        /// <summary>
        /// Gets the end time of the exchange session.
        /// </summary>
        public DateTime EndTime { get { return _endTime; } }

        /// <summary>
        /// Retrns the string representation of this MarketDataSessionMessage.
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} {1} {2}", _sessionState.ToString(), _startTime.ToString("HH:mm:ss"), _endTime.ToString("HH:mm:ss"));
        }
    }
}
