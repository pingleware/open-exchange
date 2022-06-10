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
using OPEX.DES.OrderManager;

namespace OPEX.DES.AA
{
    class AggressivenessModel
    {
        private readonly double Epsilon = 1.0;
        private OrderSide _side;
        private ComputeTauDelegate InnerComputeTau;
        private readonly NewtonSolver NewtonSolver;
        private double PMAX;
        private readonly Logger _logger;
        private readonly DESAAAgent _agent;

        private double _limitPrice;
        private double _tau;
        private double _currentA;

        private delegate double ComputeTauDelegate(double theta, double r, double estimatedPrice);

        public AggressivenessModel(DESAAAgent agent)
        {
            _logger = new Logger(string.Format("AggressivenessModel({0})", agent.Name));
            _agent = agent;
            _tau = double.NaN;
            NewtonSolver = new NewtonSolver();
        }      

        public double PMax { set { PMAX = value; } }         
        public OrderSide Side
        {
            set
            {
                _side = value;
                if (value == OrderSide.Buy)
                {
                    InnerComputeTau = InnerComputeTauBuyer;
                }
                else
                {
                    InnerComputeTau = InnerComputeTauSeller;
                }
            }
        }
        public double LimitPrice { set { _limitPrice = value; } }
        public double Tau { get { return _tau; } }

        public double ComputeTau(double theta, double r, double estimatedPrice)
        {
            return _tau = InnerComputeTau(theta, r, estimatedPrice);
        }

        public double ComputeRShout(double theta, double estimatedPrice, double pi)
        {
            bool intraMarginal = IsIntraMarginal(estimatedPrice);
            double rShout;

            if (_side == OrderSide.Buy)
            {
                if (pi > _limitPrice)
                {
                    rShout = 0.0;
                }
                else if (intraMarginal)
                {
                    rShout = InnerComputeRShoutIntraMarginalBuyer(pi, theta, estimatedPrice);
                }
                else
                {
                    rShout = InnerComputeRShoutExtraMarginalBuyer(pi, theta);
                }
            }
            else
            {
                if (pi < _limitPrice)
                {
                    rShout = 0.0;
                }
                else if (intraMarginal)
                {
                    rShout = InnerComputeRShoutIntraMarginalSeller(pi, theta, estimatedPrice);
                }
                else
                {
                    rShout = InnerComputeRShoutExtraMarginalSeller(pi, theta);
                }
            }

            _logger.Trace(LogLevel.Debug, "ComputeRShout. RShout(theta={0},p*={1},pi={2})={3}", theta, estimatedPrice, pi, rShout);

            return rShout;
        }

        private bool IsIntraMarginal(double estimatedPrice)
        {
            return (_side == OrderSide.Buy && (_limitPrice > estimatedPrice)) || (_side == OrderSide.Sell && (_limitPrice < estimatedPrice));
        }

        #region RShout = RShout(pi, theta, p*)

        private double InnerComputeRShoutIntraMarginalBuyer(double pi, double theta, double estimatedPrice)
        {
            double rShout;

            if (0.0 <= pi && pi < estimatedPrice)
            {
                double thetaBar = ComputeThetaBarBuyer(estimatedPrice, theta);
                if (thetaBar != 0.0)
                {
                    rShout = -(1.0 / thetaBar) * Math.Log(((estimatedPrice - pi) / estimatedPrice) * (Math.Exp(thetaBar) - 1.0) + 1.0);
                }
                else
                {
                    rShout = pi / estimatedPrice - 1.0;
                }
            }
            else if (estimatedPrice <= pi && pi <= _limitPrice)
            {
                if (theta != 0.0)
                {
                    rShout = (1.0 / theta) * Math.Log(((pi - estimatedPrice) / (_limitPrice - estimatedPrice)) * (Math.Exp(theta) - 1.0) + 1.0);
                }
                else
                {
                    rShout = (pi - estimatedPrice) / (_limitPrice - estimatedPrice);
                }
            }
            else
            {
                throw new ArgumentException("InnerComputeRShoutIntraMarginalBuyer. Invalid value for pi: " + pi);
            }

            return rShout;
        }

        private double InnerComputeRShoutExtraMarginalBuyer(double pi, double theta)
        {
            double rShout;

            if (0.0 <= pi && pi <= _limitPrice)
            {
                if (theta != 0.0)
                {
                    rShout = -(1.0 / theta) * Math.Log(((pi - _limitPrice) / _limitPrice) * (1.0 - Math.Exp(theta)) + 1.0);
                }
                else
                {
                    rShout = (pi - _limitPrice) / _limitPrice;
                }
            }
            else
            {
                throw new ArgumentException("InnerComputeRShoutExtraMarginalBuyer. Invalid value for pi: " + pi);
            }

            return rShout;
        }

        private double InnerComputeRShoutIntraMarginalSeller(double pi, double theta, double estimatedPrice)
        {
            double rShout;

            if (estimatedPrice <= pi && pi <= PMAX)
            {
                double thetaBar = ComputeThetaBarSeller(estimatedPrice, theta);
                if (thetaBar != 0.0)
                {
                    rShout = -(1.0 / thetaBar) * Math.Log(((pi - estimatedPrice) / (PMAX - estimatedPrice)) * (Math.Exp(thetaBar) - 1.0) + 1.0);
                }
                else
                {
                    rShout = (pi - estimatedPrice) / (estimatedPrice - PMAX);
                }
            }
            else if (_limitPrice <= pi && pi <= estimatedPrice)
            {
                if (theta != 0.0)
                {
                    rShout = (1.0 / theta) * Math.Log(((pi - estimatedPrice) / (estimatedPrice - _limitPrice)) * (1.0 - Math.Exp(theta)) + 1.0);
                }
                else
                {
                    rShout = (estimatedPrice - pi) / (estimatedPrice - _limitPrice);
                }
            }
            else
            {
                throw new ArgumentException("InnerComputeRShoutIntraMarginalSeller. Invalid value for pi: " + pi);
            }

            return rShout;
        }

        private double InnerComputeRShoutExtraMarginalSeller(double pi, double theta)
        {
            double rShout;

            if (_limitPrice <= pi && pi <= PMAX)
            {
                if (theta != 0.0)
                {
                    rShout = -(1.0 / theta) * Math.Log(((pi - _limitPrice) / (PMAX - _limitPrice)) * (Math.Exp(theta) - 1.0) + 1.0);
                }
                else
                {
                    rShout = (_limitPrice - pi) / (PMAX - _limitPrice);
                }
            }
            else
            {
                throw new ArgumentException("InnerComputeRShoutExtraMarginalSeller. Invalid value for pi: " + pi);
            }

            return rShout;
        }

        #endregion RShout

        #region Tau = Tau(r, theta, p*)

        private double InnerComputeTauBuyer(double theta, double r, double estimatedPrice)
        {
            double tau = 0.0;
            bool intraMarginal = _limitPrice > estimatedPrice;

            if (Math.Abs(r) > 1.0)
            {
                double r1 = Math.Sign(r);
                _logger.Trace(LogLevel.Warning, "InnerComputeTauBuyer. Bad value for r: {0}. Clipping r to {1}", r.ToString(), r1.ToString());
                r = r1;
            }

            if (r >= -1.0 && r <= 0.0)
            {
                if (intraMarginal)
                {
                    double theta_buyer = ComputeThetaBarBuyer(estimatedPrice, theta);
                    tau = estimatedPrice * (1.0 - ((Math.Exp(-r * theta_buyer) - 1.0) / (Math.Exp(theta_buyer) - 1.0)));
                }
                else
                {
                    tau = _limitPrice * (1.0 - ((Math.Exp(-r * theta) - 1.0) / (Math.Exp(theta) - 1.0)));
                }
            }
            else if (r > 0.0 && r <= 1.0)
            {
                if (intraMarginal)
                {
                    tau = estimatedPrice + (_limitPrice - estimatedPrice) * ((Math.Exp(r * theta) - 1.0) / (Math.Exp(theta) - 1.0));
                }
                else
                {
                    tau = _limitPrice;
                }
            }
            else
            {
                throw new ArgumentException("InnerComputeTauBuyer. Bad value for r: " + r.ToString());
            }

            return tau;
        }

        private double InnerComputeTauSeller(double theta, double r, double estimatedPrice)
        {
            double tau = 0.0;
            bool intraMarginal = _limitPrice < estimatedPrice;

            if (Math.Abs(r) > 1.0)
            {
                double r1 = Math.Sign(r);
                _logger.Trace(LogLevel.Warning, "InnerComputeTauBuyer. Bad value for r: {0}. Clipping r to {1}", r.ToString(), r1.ToString());
                r = r1;
            }

            if (r >= -1.0 && r <= 0.0)
            {
                if (intraMarginal)
                {
                    double theta_seller = ComputeThetaBarSeller(estimatedPrice, theta);
                    tau = estimatedPrice + (PMAX - estimatedPrice) * ((Math.Exp(-r * theta_seller) - 1.0) / (Math.Exp(theta_seller) - 1.0));
                }
                else
                {
                    tau = _limitPrice + (PMAX - _limitPrice) * ((Math.Exp(-r * theta) - 1.0) / (Math.Exp(theta) - 1.0));
                }
            }
            else if (r > 0.0 && r <= 1.0)
            {
                if (intraMarginal)
                {
                    tau = _limitPrice + (estimatedPrice - _limitPrice) * (1.0 - ((Math.Exp(r * theta) - 1.0) / (Math.Exp(theta) - 1.0)));
                }
                else
                {
                    tau = _limitPrice;
                }
            }
            else
            {
                throw new ArgumentException("InnerComputeTauBuyer. Bad value for r: " + r.ToString());
            }

            return tau;
        }

        #endregion Tau

        #region ThetaBar = ThetaBar(theta, p*)

        private double ComputeThetaBarSeller(double estimatedPrice, double theta)
        {
            if (theta != 0.0)
            {
                _currentA = ((estimatedPrice - PMAX) / (estimatedPrice - _limitPrice)) * (1.0 - Math.Exp(theta)) / theta;
            }
            else
            {
                _currentA = (estimatedPrice - PMAX) / estimatedPrice;
            }

            return ComputeThetaBar();
        }

        private double ComputeThetaBarBuyer(double estimatedPrice, double theta)
        {
            if (theta != 0.0)
            {
                _currentA = (estimatedPrice / (_limitPrice - estimatedPrice)) * (Math.Exp(theta) - 1.0) / theta;
            }
            else
            {
                _currentA = estimatedPrice / (_limitPrice - estimatedPrice);
            }

            return ComputeThetaBar();
        }

        private double _lastA = double.NaN;
        private double _lastSolution = double.NaN;
        private double ComputeThetaBar()
        {
            double solution;

            if (!double.IsNaN(_lastA) && _lastA == _currentA && !double.IsNaN(_lastSolution))
            {
                solution = _lastSolution;
                _logger.Trace(LogLevel.Debug, "ComputeThetaBar. ThetaBar = {0} (solution found in cache)", solution);
            }
            else
            {
                if (_currentA <= 0.0 || _currentA == 1.0)
                {
                    solution = 0.0;
                }
                else
                {
                    double startingPoint = Math.Log(_currentA);
                    if (0.0 < _currentA && _currentA <= 1)
                    {
                        startingPoint -= Epsilon;
                    }
                    else
                    {
                        startingPoint += Epsilon;
                    }

                    solution = NewtonSolver.Solve(F, F1, startingPoint);
                }
                _logger.Trace(LogLevel.Debug, "ComputeThetaBar. exp(ThetaBar) - ({0})*ThetaBar - 1 = 0 <=> ThetaBar={1}", _currentA, solution);
            }

            _lastA = _currentA;
            _lastSolution = solution;
            return solution;
        }

        private double F(double theta_)
        {
            return Math.Exp(theta_) - _currentA * theta_ - 1.0;
        }

        private double F1(double theta_)
        {
            return Math.Exp(theta_) - _currentA;
        }

        #endregion ThetaBar = ThetaBar(theta, p*)
    }

}
