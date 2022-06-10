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
using OPEX.SupplyService.Common;

namespace OPEX.DWEAS.Client
{
    public class AssignmentBucket
    {
        private readonly double _price;
        private readonly List<Assignment> _assignments;
        private readonly string _ric;
        private readonly OrderSide _side;
        private readonly string _ccy;

        private int _quantityOnMarket;
        private int _quantityFilled;

        public AssignmentBucket(double price, string ric, OrderSide side, string ccy)
        {
            _price = price;
            _assignments = new List<Assignment>();
            _ric = ric;
            _ccy = ccy;
            _side = side;
            _quantityFilled = 0;
            _quantityOnMarket = 0;
        }

        public void AddAssignment(Assignment p)
        {
            if (p.Price != _price)
            {
                throw new ApplicationException(
                    string.Format("Cannot add permit @ price {0} to a bucket @ price {1}",
                    p.Price, _price));
            }

            _assignments.Add(p);
        }

        public void Allocate(int quantity)
        {
            if (quantity > 0)
            {
                _quantityOnMarket += quantity;
            }
        }        

        public void Release(int quantity)
        {
            if (quantity > 0 )
            {
                _quantityOnMarket = Math.Max(_quantityOnMarket - quantity, 0);
            }
        }

        public void Fill(int quantity)
        {
            if (quantity > 0)
            {
                _quantityOnMarket = Math.Max(_quantityOnMarket - quantity, 0);
                _quantityFilled += quantity;
            }
        }

        public string CCY { get { return _ccy; } }
        public string RIC { get { return _ric; } }
        public OrderSide Side { get { return _side; } }
        public double Price { get { return _price; } }
        public int QtyFilled { get { return _quantityFilled; } }
        public int QtyOnMkt { get { return _quantityOnMarket; } }
        public int QtyRem { get { return Qty - _quantityOnMarket - _quantityFilled; } }
        public int Qty
        {
            get
            {
                int q = 0;
                foreach (Assignment p in _assignments)
                {
                    q += p.Quantity;
                }
                return q;
            }
        }

        public override string ToString()
        {
            return string.Format(
                "{0} {1}@{3} QtyFll {7} QtyMkt {5} QtyRem {6}",
                _side.ToString(), Qty, _ric, _price, _ccy, _quantityOnMarket, QtyRem, _quantityFilled);
        }
    }
}
