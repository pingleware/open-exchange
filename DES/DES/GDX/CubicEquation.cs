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
using System.Text;

using OPEX.Common;
using OPEX.DES.OrderManager;
using OPEX.DES.Exchange;

namespace OPEX.DES.GDX
{
    public class CubicEquation
    {
        private static Logger _logger;
        private readonly double[] _aa;
        private double[] _roots;

        static CubicEquation()
        {
            _logger = new Logger("CubicEquation");
        }

        /// <summary>
        /// Creates a cubic equation of the form:
        ///  a3*x^3 + a2*x^2 + a1*x + a0
        /// </summary>
        public CubicEquation(double a3, double a2, double a1, double a0)
        { 
            _aa = new double[] { a0, a1, a2, a3 };
        }

        /// <summary>
        /// Returns the coefficient of the specified degree
        /// </summary>
        public double this[int degree]
        {
            get
            {
                return _aa[degree];
            }
        }

        /// <summary>
        /// Evaluates the equation in a given point.
        /// </summary>
        public double Evaluate(double x0)
        {
            return _aa[0] + _aa[1] * x0 + _aa[2] * x0 * x0 + _aa[3] * x0 * x0 * x0;
        }

        /// <summary>
        /// The real roots of the equation
        /// </summary>
        public double[] RealRoots
        {
            get
            {
                if (_roots == null)
                {
                    if (_aa[3] != 0.0)
                    {
                        FindRoots3();
                    }
                    else if (_aa[2] != 0.0)
                    {
                        FindRoots2();
                    }
                    else if (_aa[1] != 0.0)
                    {
                        _roots = new double[1] { -_aa[0] / _aa[1] };
                    }
                    else if (_aa[0] != 0.0)
                    {
                        _roots = new double[0];
                    }
                    else
                    {
                        _logger.Trace(LogLevel.Critical, "Equation 0 = 0 has infinite solutions!");
                    }
                }

                return _roots;
            }
        }      

        private double CubicRoot(double x)
        {
            return Math.Sign(x) * Math.Pow(Math.Abs(x), 1.0 / 3.0);
        }

        private void FindRoots2()
        {
            double a = _aa[2];
            double b = _aa[1];
            double c = _aa[0];
            double delta = b * b - 4.0 * a * c;

            if (delta < 0.0)
            {
                _roots = new double[0];
            }
            else if (delta == 0.0)
            {
                _roots = new double[] { -b / (2.0 * a) };
            }
            else
            {
                _roots = new double[2];
                double q = -0.5 * (b + Math.Sign(b) * Math.Sqrt(delta));
                _roots[0] = q / a;
                _roots[1] = c / q;
            }
        }

        private void FindRoots3()
        {            
            double a = _aa[3];
            double b = _aa[2];
            double c = _aa[1];
            double d = _aa[0];
            double delta2 = (b * b - 3.0 * a * c) / (9.0 * a * a);
            double h2 = 4.0 * a * a * delta2 * delta2 * delta2;
            double yN = d + 2.0 * b * b * b / (27.0 * a * a) - (b * c) / (3.0 * a);
            double yN2 = yN * yN;
            double xN = -b / (3.0 * a);

            if (yN2 > h2)
            {
                _roots = new double[1];
                _roots[0] = xN
                    + CubicRoot((-yN + Math.Sqrt(yN2 - h2)) / (2.0 * a))
                    + CubicRoot((-yN - Math.Sqrt(yN2 - h2)) / (2.0 * a));
            }
            else if (yN2 < h2)
            {
                double aa = _aa[2] / _aa[3];
                double bb = _aa[1] / _aa[3];
                double cc = _aa[0] / _aa[3];
                double Q = (aa * aa - 3.0 * bb) / 9.0;
                double R = (2.0 * aa * aa * aa - 9.0 * aa * bb + 27.0 * cc) / 54.0;
                double Qcube = Q * Q * Q;

                if (R * R < Qcube)
                {
                    double thetaThird = Math.Acos(R / Math.Sqrt(Qcube)) / 3.0;
                    double athird = aa / 3.0;
                    double twoSqrtQ = 2.0 * Math.Sqrt(Q);

                    _roots = new double[3];
                    _roots[0] = -twoSqrtQ * Math.Cos(thetaThird) - athird;
                    _roots[1] = -twoSqrtQ * Math.Cos(thetaThird + 2.0 * Math.PI / 3.0) - athird;
                    _roots[2] = -twoSqrtQ * Math.Cos(thetaThird - 2.0 * Math.PI / 3.0) - athird;
                }           
            }
            else
            {                
                _roots = new double[3];

                if (h2 != 0)
                {
                    double delta = CubicRoot(yN / (2.0 * a));
                    _roots[0] = xN - 2.0 * delta;
                    _roots[2] = _roots[1] = xN + delta;
                }
                else
                {
                    _roots[0] = _roots[1] = _roots[2] = xN;
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0}*x^3", _aa[3]);
            for (int i = 2; i >= 0; --i)
            {
                if (_aa[i] == 0.0)
                {
                    continue;
                }
                sb.Append(" ");
                if (_aa[i] > 0)
                {
                    sb.Append("+");
                }
                sb.AppendFormat("{0}", _aa[i]);
                if (i > 0)
                {
                    sb.Append("*x");
                    if (i > 1)
                    {
                        sb.AppendFormat("^{0}", i);
                    }
                }
            }

            return sb.ToString();
        }
    }
}
