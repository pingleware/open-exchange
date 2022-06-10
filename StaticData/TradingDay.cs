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
using System.Text;

namespace OPEX.StaticData
{
    /// <summary>
    /// Compares TradingPeriod-s.
    /// </summary>
    public class TimePriority : IComparer
    {       
        #region IComparer Members

        /// <summary>
        /// Compares two TradingPeriod-s.
        /// </summary>
        /// <param name="x">First TradingPeriod to compare.</param>
        /// <param name="y">Seconds TradingPeriod to compare.</param>
        /// <returns>1 if x starts after y. -1 if x starts before y. 0 otherwise.</returns>
        public int Compare(object x, object y)
        {
            TradingPeriod periodX = x as TradingPeriod;
            TradingPeriod periodY = y as TradingPeriod;

            return periodX.StartTime.CompareTo(periodY.StartTime);
        }

        #endregion
    }

    /// <summary>
    /// Represents a daily market schedule.
    /// Allows iterating through its TradingPeriod-s.
    /// </summary>
    public class TradingDay : IEnumerable, IEnumerator
    {
        private static readonly IComparer Comparer = new TimePriority();

        private ArrayList _periods;
        private int _rowPos = -1;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.StaticData.TradingDay.
        /// </summary>
        public TradingDay()
        {
            _periods = new ArrayList();
        }

        /// <summary>
        /// Gets the current TradingPeriod, based on the
        /// current System clock.
        /// </summary>
        public TradingPeriod CurrentPeriod
        {
            get
            {
                TradingPeriod p = null;
                int oldRowPos = _rowPos;
                DateTime now = DateTime.Now;

                foreach (TradingPeriod t in this)
                {
                    if (now.CompareTo(t.EndTime) < 0)
                    {
                        p = t;
                        break;
                    }
                }

                _rowPos = oldRowPos;
                return p;
            }
        }

        /// <summary>
        /// Adds a new TradingPeriod to this TradingDay.
        /// </summary>
        /// <param name="tradingPeriod">The TradingPeriod to add to this TradingDay.</param>
        public void AddTradingPeriod(TradingPeriod tradingPeriod)
        {
            _periods.Add(tradingPeriod);
            _periods.Sort(Comparer);
        }

        /// <summary>
        /// Removes all the TradingPeriods configured as part of this TradingDay.
        /// </summary>
        public void Clear()
        {
            _periods.Clear();
        }

        /// <summary>
        /// Indicates whether the market is currently open,
        /// according to the System clock and the configured
        /// market schedule.
        /// </summary>
        /// <returns>True, if the market is Open. False, otherwise.</returns>
        public bool IsNowOpen()
        {
            bool res = false;
            DateTime now = DateTime.Now;
            int oldRowPos = _rowPos;

            foreach(TradingPeriod tradingPeriod in this)
            {
                if (tradingPeriod.State == TradingState.Close)
                {
                    continue;
                }

                if (tradingPeriod.StartTime.CompareTo(now) < 0 && now.CompareTo(tradingPeriod.EndTime) < 0)
                {
                    res = true;
                    break;
                }
            }

            _rowPos = oldRowPos;
            return res;
        }

        #region IEnumerator Members

        public object Current
        {
            get { return _periods[_rowPos]; }
        }

        public bool MoveNext()
        {
            _rowPos++;

            if (_rowPos < _periods.Count)
            {
                return true;
            }

            Reset();
            return false;
        }

        public void Reset()
        {
            _rowPos = -1;
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            Reset();
            return this;
        }

        #endregion
    }
}
