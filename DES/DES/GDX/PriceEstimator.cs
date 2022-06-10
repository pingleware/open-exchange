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

namespace OPEX.DES.GDX
{   
    public delegate double RealFunction(double x);
    public delegate double RealFunctionOfTwoVariables(double x, int m);

    public class PriceEstimator
    {
        private Dictionary<double, double> _tabulatedEstimationFunction;
        private int _M;
        private int _N;
        private double[,] _V;
        private RealFunctionOfTwoVariables _profitFunction;
        private double _gamma;
        private double PMIN;
        private double PMAX;

        public PriceEstimator()
        {
            _tabulatedEstimationFunction = new Dictionary<double, double>();
        }

        public void Init(int M, int N)
        {
            _M = M;
            _N = N;
            _V = new double[M + 1, N + 1];

            for (int m = 0; m <= M; ++m)
            {
                for (int n = 0; n <= N; ++n)
                {
                    _V[m, n] = 0.0;
                }
            }

            _tabulatedEstimationFunction.Clear();
        }

        public double OptimalPrice(RealFunction f, RealFunctionOfTwoVariables s, double priceMin, double priceMax, double step, double gamma, double minPrice, double maxPrice)
        {
            double optimalPrice = 0.0;
            PMIN = minPrice;
            PMAX = maxPrice;
            _profitFunction = s;
            _gamma = gamma;

            // tabulate estimation function before calculation starts;
            // this saves a whole lot of function invocations            
            for (double p = priceMin; p <= priceMax; p += step)
            {
                _tabulatedEstimationFunction[p] = f(p);
            }

            for (int n = 1; n <= _N; ++n)
            {
                for (int m = 1; m <= _M; ++m)
                {
                    _V[m, n] = MaxStepComputation(priceMin, priceMax, step, m, n, out optimalPrice);
                }
            }

            return optimalPrice;
        }

        private double MaxStepComputation(double priceMin, double priceMax, double step, int m, int n, out double pStar)
        {
            double max = double.NegativeInfinity;
            double f = 0.0;
            double y = 0.0;
            pStar = 0.0;

            for (double p = priceMin; p <= priceMax; p += step)
            {
                f = _tabulatedEstimationFunction[p];
                y = f * (_profitFunction(p, m) + _gamma * _V[m - 1, n - 1]) + (1.0 - f) * _gamma * _V[m, n - 1];
                if (y > max)
                {
                    max = y;
                    pStar = p;
                }
            }

            return max;
        }
    }    
}
