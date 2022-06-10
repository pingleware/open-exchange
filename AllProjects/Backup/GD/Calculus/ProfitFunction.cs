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

using OPEX.Common;
using OPEX.OM.Common;
using OPEX.StaticData;

namespace OPEX.Agents.GD.Calculus
{
    public class ProfitFunction
    {
        private Logger _logger;
        private BeliefFunction _belief;
        private double _c;
        private OrderSide _side;
        private Instrument _instrument;

        public ProfitFunction(BeliefFunction belief, double c, OrderSide side, Instrument instrument, string name)
        {
            _logger = new Logger(string.Format("ProfitFunction({0})", name));
            _belief = belief;
            _c = c;
            _side = side;
            _instrument = instrument;
        }              

        public double MaxPoint(double searchIntervalMin, double searchIntervalMax)
        {
            TabulatedFunction p = _belief.Function;
            TabulatedFunction pcut = new TabulatedFunction();

            _logger.Trace(LogLevel.Debug, "MaxPoint. Search range: [{0}, {1}]", searchIntervalMin, searchIntervalMax);

            for (int i = 0; i < p.Values.Count - 1; ++i)
            {
                double amin = p.XValues[i];
                double amax = p.XValues[i + 1];

                _logger.Trace(LogLevel.Debug, "MaxPoint. Current interval: [{0}, {1}]", amin, amax);

                if (searchIntervalMin >= amax || searchIntervalMax <= amin)
                {
                    _logger.Trace(LogLevel.Debug, "MaxPoint. Interval out of search range. Skipping.");
                    continue;
                }

                if (amin <= searchIntervalMin && searchIntervalMin <= amax)
                {
                    amin = searchIntervalMin;
                }
                if (amin <= searchIntervalMax && searchIntervalMax <= amax)
                {
                    amax = searchIntervalMax;
                }
                _logger.Trace(LogLevel.Debug, "MaxPoint. Actual interval for maximisation: [{0}, {1}]", amin, amax);

                _logger.Trace(LogLevel.Debug, "MaxPoint. Computing cubic spline through [{0}, {2}] and [{1}, {3}]", p.XValues[i], p.XValues[i + 1], p.YValues[i], p.YValues[i + 1]);
                CubicEquation pspline = this.ComputeSpline(p.XValues[i], p.XValues[i + 1], p.YValues[i], p.YValues[i + 1]);
                _logger.Trace(LogLevel.Debug, "MaxPoint. Pspline: {0}", pspline.ToString());

                double pspmin = pspline.Evaluate(amin);
                double pspmax = pspline.Evaluate(amax);
                double profitmin = Profit(amin);
                double profitmax = Profit(amax);
                double pcutmin = pspmin * profitmin;
                double pcutmax = pspmax * profitmax;
                _logger.Trace(LogLevel.Debug, "MaxPoint. Pspline({0}) = {1}. Profit({0}) = {4}. Pspline({2}) = {3}. Profit({2}) = {5}.", amin, pspmin, amax, pspmax, profitmin, profitmax);
                _logger.Trace(LogLevel.Debug, "MaxPoint. Adding points to pcut: [{0}, {1}], [{2}, {3}]", amin, pcutmin, amax, pcutmax);
                pcut.Add(amin, pcutmin);
                pcut.Add(amax, pcutmax);
                CubicEquation F1 = ComputeDerivative(pspline);
                _logger.Trace(LogLevel.Debug, "MaxPoint. F1 = diff(Pspline(a)*Profit(a)) = {0}", F1.ToString());
                double[] roots = F1.RealRoots;

                if (roots != null)
                {
                    if (roots.Length > 0)
                    {
                        string s = string.Format("{0}", roots[0]);
                        for (int j = 1; j < roots.Length; ++j)
                        {
                            s += string.Format(" {0}", roots[j]);
                        }
                        _logger.Trace(LogLevel.Debug, "MaxPoint. Turning points of F1: {0}", s);
                    }

                    foreach (double turningPoint in roots)
                    {
                        if (amin <= turningPoint && turningPoint <= amax)
                        {
                            double val = pspline.Evaluate(turningPoint) * Profit(turningPoint);
                            pcut.Add(turningPoint, val);
                            _logger.Trace(LogLevel.Debug, "MaxPoint. Point [{0}, {1}] added to pcut", turningPoint, val);
                        }
                    }
                }
            }
            _logger.Trace(LogLevel.Debug, "MaxPoint. pcut: {0}", pcut.ToString());        

            double max = 0.0;
            if (pcut.Count > 0)
            {
                if (_side == OrderSide.Sell)
                {                    
                    max = pcut.RightmostMaxPoint;                 
                }
                else
                {
                    max = pcut.LeftmostMaxPoint;
                }
            }
            else
            {
                if (_side == OrderSide.Sell)
                {
                    max = searchIntervalMax;
                }
                else
                {
                    max = searchIntervalMin;
                }
            }

            _logger.Trace(LogLevel.Debug, "MaxPoint. Result: {0}", max);

            return max;
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

        private CubicEquation ComputeDerivative(CubicEquation p)
        {
            double beta3 = -4.0 * p[3];
            double beta2 = 3.0 * (_c * p[3] - p[2]);
            double beta1 = 2.0 * (p[2] * _c - p[1]);
            double beta0 = p[1] * _c - p[0];

            if (_side == OrderSide.Sell)
            {
                beta3 = -beta3;
                beta2 = -beta2;
                beta1 = -beta1;
                beta0 = -beta0;
            }

            return new CubicEquation(beta3, beta2, beta1, beta0);
        }

        private double Profit(double x)
        {
            if (_side == OrderSide.Sell)
            {
                return Math.Max(x - _c, 0.0);
            }
            else
            {
                return Math.Max(_c - x, 0.0);
            }
        }
    }
}
