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

namespace OPEX.Agents.GDX.Calculus
{    
    public delegate double RealFunction(double x);
    public delegate double RealFunctionOfTwoVariables(double x, int m);

    public class PriceEstimator
    {
        private Dictionary<double, double> _tabulatedEstimationFunction;
        private int _M;
        private int _N;
        private double[,] _V;
        private double[,] _profitTable;        
        private double _gamma;
        private readonly StringBuilder _sb;

        public PriceEstimator()
        {
            _tabulatedEstimationFunction = new Dictionary<double, double>();
            _sb = new StringBuilder();
        }

        public double[,] V { get { return _V; } }
        public string Report { get { return _sb.ToString(); } }

        public void Init(int M, int N)
        {
            _M = M;
            _N = N;
            _V = new double[M+1, N+1];            

            for (int m = 0; m <= M; ++m)
            {
                for (int n = 0; n <= N; ++n)
                {
                    _V[m, n] = 0.0;
                }
            }

            _tabulatedEstimationFunction.Clear();            
        }

        public double OptimalPrice(RealFunction f, RealFunctionOfTwoVariables s,
            double priceMin, double priceMax, double step, double gamma)
        {
            double optimalPrice = 0.0;

            SetupProfitTable(s, priceMin, priceMax, step);
            
            _gamma = gamma;
            _sb.Remove(0, _sb.Length);

            _sb.AppendFormat("OPTIMAL PRICE CALCULATION. priceMin {0} priceMax {1} step {2} gamma {3}\n",
                priceMin, priceMax, step, gamma);

            _sb.Append("TabulatedEstimationFunction:");

            for (double p = priceMin; p <= priceMax; p += step)
            {
                _tabulatedEstimationFunction[p] = f(p);
                _sb.AppendFormat("\n{0}\t{1}", p, _tabulatedEstimationFunction[p]);
            }

            _sb.AppendLine();

            for (int n = 1; n <= _N; ++n)
            {
                for (int m = 1; m <= _M; ++m)
                {
                    _V[m, n] = MaxStepComputation(priceMin, priceMax, step, m, n, out optimalPrice);
                }
            }

            _sb.AppendFormat("\n========================> OPTIMISATION RESULT::::::::::::: {0}\n", optimalPrice);

            return optimalPrice;
        }

        private void SetupProfitTable(RealFunctionOfTwoVariables s, double priceMin, double priceMax, double step)
        {
            _profitTable = new double[_M + 1, (int)((priceMax - priceMin) / step) + 1];
            for (int m = 1; m <= _M; ++m)
            {
                int profitIdx = 0;
                for (double p = priceMin; p <= priceMax; p += step)
                {
                    _profitTable[m, profitIdx] = s(p, m);
                    ++profitIdx;
                }
            }
        }

        private double MaxStepComputation(double priceMin, double priceMax, double step, int m, int n, out double pStar)
        {
            double max = double.NegativeInfinity;
            double f = 0.0;
            double y = 0.0;
            pStar = 0.0;

            _sb.AppendFormat("MaxStepComputation: m {0} n {1}", m, n);

            int profitIdx = 0;
            for (double p = priceMin; p <= priceMax; p += step)
            {
                double pfpm = _profitTable[m, profitIdx];
                double vm_1n_1 = _V[m - 1, n - 1];
                double vm_n_1 = _V[m, n - 1];

                f = _tabulatedEstimationFunction[p];
                y = f * (pfpm + _gamma * vm_1n_1) + (1.0 - f) * _gamma * vm_n_1;
                if (y > max)
                {
                    max = y;
                    pStar = p;
                }

                _sb.AppendFormat("\np {0} profit(p,m) {1} V[m-1,n-1] {2} V[m,n-1] {3} y {4} max {5} pStar {6}",
                    p, pfpm, vm_1n_1, vm_n_1, y, max, pStar);

                ++profitIdx;
            }

            _sb.AppendFormat("\n========== V[m={0},n={1}]={2} (p*={3})\n", m, n, max, pStar);

            return max;
        }
    }
}
