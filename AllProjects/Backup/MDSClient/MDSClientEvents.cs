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

using OPEX.MDS.Common;

namespace OPEX.MDS.Client
{
    /// <summary>
    /// Specifies the type of the MarketDataEvent.
    /// </summary>
    public enum MarketDataEventType
    {
        /// <summary>
        /// A piece of information that was requested
        /// was delivererd.
        /// </summary>
        DownloadFinished,

        /// <summary>
        /// An updated version of a depth is available.
        /// </summary>
        DepthChanged,

        /// <summary>
        /// An updated version of a depth is available,
        /// together with the last Shout that made it
        /// change.
        /// </summary>
        DepthChangedWithNewShout,

        /// <summary>
        /// An updated version of a depth is available,
        /// together with the last Shout that made it
        /// change and the last trade that was made.
        /// </summary>
        DepthChangedWithNewTrade        
    }

    /// <summary>
    /// Event arguments for MarketDataEvents.
    /// </summary>
    public class MarketDataEventArgs : EventArgs
    {
        private readonly MarketDataEventType _type;
        private readonly string _instrument;

        /// <summary>
        /// Gets the MarketDataEventType of the MarketDataEvent.
        /// </summary>
        public MarketDataEventType Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Gets the instrument underlying the MarketDataEvent.
        /// </summary>
        public string Instrument
        {
            get { return _instrument; }
        }

        /// <summary>
        /// Initialises a new instance of the class
        /// MarketDataEventArgs.
        /// </summary>
        /// <param name="type">The MarketDataEventType of this MarketDataEventArgs.</param>
        /// <param name="instrument">The instrument underlying this MarketDataEventArgs.</param>
        public MarketDataEventArgs(MarketDataEventType type, string instrument)
            : base()
        {
            _instrument = instrument;
            _type = type;
        }

        /// <summary>
        /// Returns a string representation of this MarketDataEventArgs.
        /// </summary>
        public override string ToString()
        {
            return string.Format("Instrument {0} Type {1}", _instrument, _type.ToString());
        }
    }

    /// <summary>
    /// Event arguments of SessionChangedEventHandler.
    /// </summary>
    [Serializable]
    public class SessionChangedEventArgs : EventArgs
    {
        private readonly SessionState _sessionState;
        private readonly DateTime _startTime;
        private readonly DateTime _endTime;
        private readonly bool _serverAlive;
        private readonly bool _isBroadcast;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.MDS.Client.SessionChangedEventArgs.
        /// </summary>
        /// <param name="serverAlive">Indicates whether MarketDataServer is alive.</param>
        /// <param name="isBroadcast">Indicates whether this is a broadcast message, or a response message to a previous request.</param>
        /// <param name="sessionState">The status of the current exchange session.</param>
        /// <param name="startTime">The start time of the current exchange session.</param>
        /// <param name="endTime">The end time of the current exchange session.</param>
        public SessionChangedEventArgs(bool serverAlive, bool isBroadcast, SessionState sessionState, DateTime startTime, DateTime endTime)            
            : base()
        {
            _isBroadcast = isBroadcast;
            _serverAlive = serverAlive;
            _sessionState = sessionState;
            _startTime = startTime;
            _endTime = endTime;
        }

        /// <summary>
        /// Indicates whether this is a broadcast message, or a response message to a previous request.
        /// </summary>
        public bool IsBroadcast { get { return _isBroadcast; } } 

        /// <summary>
        /// Indicates whether MarketDataServer is alive.
        /// </summary>
        public bool ServerAlive { get { return _serverAlive; } }

        /// <summary>
        /// Gets the status of the current exchange session.
        /// </summary>
        public SessionState SessionState { get { return _sessionState; } }

        /// <summary>
        /// Gets the start time of the current exchange session.
        /// </summary>
        public DateTime StartTime { get { return _startTime; } }

        /// <summary>
        /// Gets the end time of the current exchange session.
        /// </summary>
        public DateTime EndTime { get { return _endTime; } }

        /// <summary>
        /// Returns the string representation of this SessionChangedEventArgs.
        /// </summary>
        public override string ToString()
        {
            return string.Format("ServerAlive {3} IsBroadcast {4} State {0} Start {1} End {2}", SessionState, StartTime, EndTime, ServerAlive, IsBroadcast);
        }
    }

    /// <summary>
    /// Represents the method that will handle an event
    /// that has SessionChangedEventArgs arguments.
    /// </summary>
    /// <param name="sender">The sender of this event.</param>
    /// <param name="sessionState">The parameters of this event.</param>
    public delegate void SessionChangedEventHandler(object sender, SessionChangedEventArgs sessionState);
}
