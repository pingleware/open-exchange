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

using OPEX.AS.Service;
using OPEX.SupplyService.Common;
using OPEX.OM.Common;
using OPEX.Common;

namespace OPEX.DWEAS.Client
{
    public class AssignmentController
    {
        private enum ChangeType
        {
            Allocate,
            Release,
            Fill
        };
        private readonly SortedDictionary<double, AssignmentBucket> _table;
        private readonly AssignmentComparer _comparer;
        private readonly Logger _logger;

        public AssignmentController(string logTitle)
        {
            _comparer = new AssignmentComparer();
            _table = new SortedDictionary<double, AssignmentBucket>(_comparer);
            _logger = new Logger(logTitle);
        }

        public void AddAssignment(Assignment a)
        {
            if (a != null)
            {
                _comparer.Side = a.Side;

                if (!_table.ContainsKey(a.Price))
                {
                    _table.Add(a.Price, new AssignmentBucket(a.Price, a.Ric, a.Side, a.Currency));
                }
                _table[a.Price].AddAssignment(a);
            }
            _logger.Trace(LogLevel.Method, "AddAssignment. NEW TABLE: {0}", this.ToString());
        }

        public void Clear()
        {
            _logger.Trace(LogLevel.Method, "Clear. ALL ASSIGNMENT WERE CLEARED");
            _table.Clear();
        }

        public void Fll(double price, int quantity)
        {
            Change(price, quantity, ChangeType.Fill);
            _logger.Trace(LogLevel.Method, "Fill. price {0}. qty {1}. NEW TABLE: {2}",
                price, quantity, this.ToString());
        }

        public void Rel(double price, int quantity)
        {
            Change(price, quantity, ChangeType.Release);
            _logger.Trace(LogLevel.Method, "Release. price {0}. qty {1}. NEW TABLE: {2}",
                price, quantity, this.ToString());
        }

        public void Allocate(double price, int quantity)
        {
            Change(price, quantity, ChangeType.Allocate);
            _logger.Trace(LogLevel.Method, "Allocate. price {0}. qty {1}. NEW TABLE: {2}",
                price, quantity, this.ToString());
        }

        private void Change(double price, int quantity, ChangeType chgType)
        {
            if (!_table.ContainsKey(price))
            {
                return;
            }

            AssignmentBucket pb = _table[price];

            switch (chgType)
            {
                case ChangeType.Allocate:
                    pb.Allocate(quantity);
                    break;
                case ChangeType.Release:
                    pb.Release(quantity);
                    break;
                case ChangeType.Fill:
                    pb.Fill(quantity);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public SortedDictionary<double, AssignmentBucket> AssignmentBuckets { get { return _table; } }

        public int QtyRem
        {
            get
            {
                int res = 0;

                foreach (AssignmentBucket ab in _table.Values)
                {
                    res += ab.QtyRem;
                }

                return res;
            }
        }

        public int QtyOnMkt
        {
            get
            {
                int res = 0;

                foreach (AssignmentBucket ab in _table.Values)
                {
                    res += ab.QtyOnMkt;
                }

                return res;
            }
        }

        public int QtyFilled
        {
            get            
            {
                int res = 0;

                foreach (AssignmentBucket ab in _table.Values)
                {
                    res += ab.QtyFilled;
                }

                return res;
            }
        }

        public int Qty
        {
            get
            {
                int res = 0;

                foreach (AssignmentBucket ab in _table.Values)
                {
                    res += ab.Qty;
                }

                return res;
            }
        }

        public int OpnBckts
        {
            get
            {
                int res = 0;

                foreach (AssignmentBucket ab in _table.Values)
                {
                    if (ab.QtyRem > 0)
                    {
                        ++res;
                    }
                }

                return res;
            }
        }

        public AssignmentBucket BestAvailableAssignmentBucket
        { 
            get 
            {
                AssignmentBucket ab = null;

                foreach (double price in _table.Keys)
                {
                    if (_table[price].QtyRem > 0)
                    {
                        ab = _table[price];
                        break;
                    }
                }

                return ab; 
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (AssignmentBucket ab in _table.Values)
            {
                sb.AppendFormat("[{0} {1}] ", ab.Price, ab.ToString()); 
            }

            return sb.ToString();
        }

        #region Comparer

        private class AssignmentComparer : IComparer<double>
        {
            private int sortingOrder;

            public AssignmentComparer()
            {
                sortingOrder = 1;
            }

            public OrderSide Side { set { SetComparer(value); } }
        
            #region IComparer<double> Members

            public int Compare(double x, double y)
            {
                return x.CompareTo(y) * sortingOrder;
            }

            #endregion

            private void SetComparer(OrderSide side)
            {
                sortingOrder = (side == OrderSide.Buy) ? -1 : 1;
            }
        }

        #endregion Comparer
    }
}
