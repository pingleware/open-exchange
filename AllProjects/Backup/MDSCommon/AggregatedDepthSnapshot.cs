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
using System.Linq;
using System.Text;

using OPEX.OM.Common;

namespace OPEX.MDS.Common
{
    /// <summary>
    /// Represents a snapshot of the orderbook
    /// of an instrument.
    /// </summary>
    [Serializable]
    public class AggregatedDepthSnapshot
    {
        private readonly string _instrument;
        private readonly AggregatedDepthSide _buySide;
        private readonly AggregatedDepthSide _sellSide;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.MDS.Common.AggregatedDepthSnapshot.
        /// </summary>
        /// <param name="instrument">The instrument of which
        /// this AggregatedDepthSnapshot represents a snapshot
        /// of the orderbook.</param>
        public AggregatedDepthSnapshot(string instrument)
        {
            _buySide = new AggregatedDepthSide(OrderSide.Buy);
            _sellSide = new AggregatedDepthSide(OrderSide.Sell);

            _instrument = instrument;
        }

        /// <summary>
        /// Gets the depth of this AggregatedDepthSnapshot.
        /// </summary>
        public int Size { get { return Math.Max(_buySide.Depth, _sellSide.Depth); } }

        /// <summary>
        /// Gets the instrument 
        /// </summary>
        public string Instrument { get { return _instrument; } }

        /// <summary>
        /// Gets the Buy side of this AggregatedDepthSnapshot.
        /// </summary>
        public AggregatedDepthSide Buy { get { return _buySide; } }

        /// <summary>
        /// Gets the Sell side of this AggregatedDepthSnapshot.
        /// </summary>
        public AggregatedDepthSide Sell { get { return _sellSide; } }

        /// <summary>
        /// Gets a specific side of this AggregatedDepthSnapshot.
        /// </summary>
        /// <param name="side">The side to get.</param>
        /// <returns>The side specified.</returns>
        public AggregatedDepthSide this[OrderSide side]
        {
            get
            {
                if (side == OrderSide.Buy)
                {
                    return this._buySide;
                }
                else
                {
                    return this._sellSide;
                }
            }
        }    

        /// <summary>
        /// Returns the string representation of this AggregatedDepthSnapshot.
        /// </summary>
        public override string ToString()
        {
            const string separator = "+--------+--------+--------+--------+";

            StringBuilder sb = new StringBuilder();
            IEnumerator buyQuoteEnumerator = _buySide.GetEnumerator();
            IEnumerator sellQuoteEnumerator = _sellSide.GetEnumerator();
            bool moreBuy = true;
            bool moreSell = true;
            for (int i = 0; i < this.Size; ++i)
            {
                AggregatedQuote buyQuote = null;
                AggregatedQuote sellQuote = null;

                if (moreBuy && (moreBuy = buyQuoteEnumerator.MoveNext()))
                {
                    buyQuote = buyQuoteEnumerator.Current as AggregatedQuote;
                }
                if (moreSell && (moreSell = sellQuoteEnumerator.MoveNext()))
                {
                    sellQuote = sellQuoteEnumerator.Current as AggregatedQuote;
                }

                sb.AppendLine(separator);
                sb.AppendFormat("|{0,8:N0}|{1,8:F4}|{3,8:F4}|{2,8:N0}|\n",
                    (buyQuote != null) ? buyQuote.Quantity : 0,
                    (buyQuote != null) ? buyQuote.Price : 0,
                    (sellQuote != null) ? sellQuote.Quantity : 0,
                    (sellQuote != null) ? sellQuote.Price : 0);
            }
            if (this.Size > 0)
            {
                sb.AppendLine(separator);
            }

            return sb.ToString();
        }
    }    
}
