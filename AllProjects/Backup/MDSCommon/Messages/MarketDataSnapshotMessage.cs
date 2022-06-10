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
using System.Collections.Generic;

using OPEX.OM.Common;

namespace OPEX.MDS.Common
{
    /// <summary>
    /// Base class for MarketDataMessage
    /// that carry an AggregatedDepthSnapshot.
    /// This class is abstract.
    /// </summary>
    [Serializable]
    public abstract class MarketDataSnapshotMessage : MarketDataMessage
    {
        private readonly AggregatedDepthSnapshot _snapshot;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.MDS.Common.MarketDataSnapshotMessage.
        /// </summary>
        /// <param name="instrument">The instrument underlying this MarketDataSnapshotMessage.</param>
        /// <param name="dataSource">The data source underlying this MarketDataSnapshotMessage.</param>
        /// <param name="snapshot">The AggregatedDepthSnapshot associated to this MarketDataSnapshotMessage.</param>
        public MarketDataSnapshotMessage(string instrument, string dataSource, AggregatedDepthSnapshot snapshot)
            : this(MarketDataMessageType.Snapshot, instrument, dataSource, snapshot)
        {
        }

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.MDS.Common.MarketDataSnapshotMessage.
        /// </summary>
        /// <param name="type">The MarketDataMessageType of this MarketDataSnapshotMessage.</param>
        /// <param name="instrument">The instrument underlying this MarketDataSnapshotMessage.</param>
        /// <param name="dataSource">The data source underlying this MarketDataSnapshotMessage.</param>
        /// <param name="snapshot">The AggregatedDepthSnapshot associated to this MarketDataSnapshotMessage.</param>
        public MarketDataSnapshotMessage(MarketDataMessageType type, string instrument, string dataSource, AggregatedDepthSnapshot snapshot)
            : base(type, instrument, dataSource)
        {
            _snapshot = snapshot;
        }
        
        /// <summary>
        /// Gets the AggregatedDepthSnapshot.
        /// </summary>
        public AggregatedDepthSnapshot Snapshot
        {
            get
            {
                return _snapshot;
            }
        }

        /// <summary>
        /// Returns the string representation of this MarketDataSnapshotMessage.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Timestamp {3} Instrument {0} Type {1} Datasource {2} Snapshot {3}", 
                this._instrument, this._type.ToString(), this._dataSource, this.TimeStamp.ToString("HH:mm:ss.fff"), _snapshot.ToString());
        }
    }
}
