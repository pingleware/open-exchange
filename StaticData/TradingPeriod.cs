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

namespace OPEX.StaticData
{
    /// <summary>
    /// Specifies the market state.
    /// </summary>
    public enum TradingState
    {
        /// <summary>
        /// Market is Open.
        /// </summary>
        Open,

        /// <summary>
        /// Market is Closed.
        /// </summary>
        Close
    }

    /// <summary>
    /// Represents a time interval and the
    /// specific market condition associated to it.
    /// </summary>
    public class TradingPeriod
    {
        private DateTime _startTime;
        private DateTime _endTime;
        private TradingState _state;

        /// <summary>
        /// Initialises a new instance of the class OPEX.StaticData.TradingPeriod.
        /// </summary>
        /// <param name="startTime">The time at which the TradingPeriod starts.</param>
        /// <param name="endTime">The time at which the TradingPeriod ends.</param>
        /// <param name="phase">The market status during this TradingPeriod.</param>
        public TradingPeriod(string startTime, string endTime, string phase)
        {
            _startTime = DateTime.Parse(startTime);
            _endTime = DateTime.Parse(endTime);
            _state = (TradingState) Enum.Parse(typeof(TradingState), phase);
        }

        /// <summary>
        /// Gets the market status during this TradingPeriod.
        /// </summary>
        public TradingState State
        {
            get { return _state; }
        }

        /// <summary>
        /// Gets the time at which the TradingPeriod ends.
        /// </summary>
        public DateTime EndTime
        {
            get { return _endTime; }
        }

        /// <summary>
        /// Gets the time at which the TradingPeriod starts.
        /// </summary>
        public DateTime StartTime
        {
            get { return _startTime; }
        }
    }
}
