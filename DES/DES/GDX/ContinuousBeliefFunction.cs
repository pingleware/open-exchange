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
    public class ContinuousBeliefFunction : BeliefFunction
    {
        protected readonly List<CubicEquation> _cubicSegments;

        public ContinuousBeliefFunction(ShoutHistory history, OrderSide side, double pmin, double pmax, string name)
            : base(history, side, pmin, pmax, name)
        {
            _cubicSegments = new List<CubicEquation>();
        }

        public double Belief(double price)
        {
            double res = 0;

            if ((_side == OrderSide.Buy && price >= _history.BestBid)
                || (_side == OrderSide.Sell && price <= _history.BestAsk))
            {
                for (int i = 0; i < _function.Values.Count - 1; ++i)
                {
                    if (price < _function.XValues[i] || price > _function.XValues[i + 1])
                    {
                        continue;
                    }

                    CubicEquation eq = _cubicSegments[i];
                    res = eq.Evaluate(price);
                    break;
                }
            }

            return res;
        }

        public override void Build()
        {
            base.Build();
            Interpolate();
        }

        protected void Interpolate()
        {
            TabulatedFunction p = this._function;
            _cubicSegments.Clear();

            if (_side == OrderSide.Buy)
            {
                bool foundBestBid = false;
                for (int i = 0; i < p.Values.Count - 1; ++i)
                {
                    CubicEquation pspline = null;
                    if (!foundBestBid)
                    {
                        foundBestBid = ((p.XValues[i] <= _history.BestBid) && (_history.BestBid <= p.XValues[i + 1]));
                    }
                    if (foundBestBid)
                    {
                        pspline = this.ComputeSpline(p.XValues[i], p.XValues[i + 1], p.YValues[i], p.YValues[i + 1]);
                    }
                    else
                    {
                        pspline = new CubicEquation(0, 0, 0, 0);
                    }
                    _cubicSegments.Add(pspline);
                }
            }
            else
            {
                bool foundBestAsk = false;
                for (int i = 0; i < p.Values.Count - 1; ++i)
                {
                    CubicEquation pspline = null;
                    if (!foundBestAsk)
                    {
                        pspline = this.ComputeSpline(p.XValues[i], p.XValues[i + 1], p.YValues[i], p.YValues[i + 1]);
                        foundBestAsk = ((p.XValues[i] <= _history.BestAsk) && (_history.BestAsk <= p.XValues[i + 1]));
                    }
                    else
                    {
                        pspline = new CubicEquation(0, 0, 0, 0);
                    }
                    _cubicSegments.Add(pspline);
                }
            }
        }

        private CubicEquation ComputeSpline(double a0, double a1, double p0, double p1)
        {
            double alpha3, alpha2, alpha1, alpha0;
            double pdiff, adiffcube;

            pdiff = p1 - p0;
            adiffcube = (a0 - a1);
            adiffcube = adiffcube * adiffcube * adiffcube;

            alpha3 = 2.0 * pdiff / adiffcube;
            alpha2 = -3.0 * (a0 + a1) * pdiff / adiffcube;
            alpha1 = 6.0 * a0 * a1 * pdiff / adiffcube;
            alpha0 = (a1 * a1 * p0 * (3.0 * a0 - a1) + a0 * a0 * p1 * (a0 - 3.0 * a1)) / adiffcube;

            return new CubicEquation(alpha3, alpha2, alpha1, alpha0);
        }
    }
}
