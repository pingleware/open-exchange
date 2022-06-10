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

using OPEX.DES.OrderManager;
using OPEX.DES.Exchange;

namespace OPEX.DES.GDX
{
    public class TabulatedFunction
    {
        private readonly double XResolution = 0.00001;
        private readonly SortedDictionary<double, double> _values;        

        public TabulatedFunction()
        {
            _values = new SortedDictionary<double, double>();
        }

        public SortedDictionary<double, double> Values { get { return _values; } }
        public List<double> XValues  { get { return _values.Keys.ToList(); } }
        public List<double> YValues { get { return _values.Values.ToList(); } }
        public double XMin { get { return _values.Keys.Min(); } }
        public double XMax { get { return _values.Keys.Max(); } }
        public int Count { get { return _values.Keys.Count; } }
        public TabulatedFunction this[double xmin, double xmax]
        {
            get
            {
                TabulatedFunction tf = new TabulatedFunction();
                if (xmin <= xmax)
                {
                    foreach (double x in _values.Keys)
                    {
                        if (x <= xmax && x >= xmin)
                        {
                            tf._values[x] = _values[x];
                        }
                    }
                }
                return tf;
            }
        }

        public double LeftmostMaxPoint
        {
            get
            {
                double xmax = double.PositiveInfinity;
                double max = _values.Values.Max();
                foreach (double x in _values.Keys)
                {
                    if (_values[x] != max)
                    {
                        continue;
                    }
                    if (x < xmax)
                    {
                        xmax = x;
                    }
                }
                return xmax;
            }
        }

        public double RightmostMaxPoint
        {
            get
            {
                double xmax = double.NegativeInfinity;
                double max = _values.Values.Max();
                foreach (double x in _values.Keys)
                {
                    if (_values[x] != max)
                    {
                        continue;
                    }
                    if (x > xmax)
                    {
                        xmax = x;
                    }
                }
                return xmax;
            }
        }

        public void Add(double x, double y)
        {
            foreach (double xx in _values.Keys)
            {
                if (Math.Abs(x - xx) < XResolution)
                {
                    return;
                }
            }
            _values[x] = y;            
        }

        public void Remove(double x)
        {
            if (_values.ContainsKey(x))
            {
                _values.Remove(x);
            }
        }

        public void RemoveTrailingZeroes()
        {
            bool foundFirstZero = false;
            HashSet<double> pointsToRemove = new HashSet<double>();

            foreach (double price in _values.Keys)
            {
                if (_values[price] != 0)
                {
                    continue;
                }

                if (!foundFirstZero)
                {
                    foundFirstZero = true;
                    continue;
                }

                pointsToRemove.Add(price);
            }

            foreach (double price in pointsToRemove)
            {
                _values.Remove(price);
            }
        }

        public void RemoveLeadingZeroes()
        {            
            double lastZero = double.NegativeInfinity;
            HashSet<double> pointsToRemove = new HashSet<double>();

            foreach (double price in _values.Keys)
            {
                if (_values[price] != 0.0)
                {
                    break;
                }
                if (lastZero < price)
                {
                    if (lastZero != double.NegativeInfinity)
                    {
                        pointsToRemove.Add(lastZero);
                    }
                    lastZero = price;
                }
            }

            foreach (double price in pointsToRemove)
            {
                _values.Remove(price);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (double price in _values.Keys)
            {
                sb.AppendFormat(" ({0}, {1})", price, _values[price]);
            }

            return sb.ToString();
        }
    }
}
