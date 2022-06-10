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

using OPEX.ShoutService;
using OPEX.Common;
using OPEX.StaticData;
using OPEX.OM.Common;
using OPEX.Agents.GD.Calculus;
using OPEX.Agents.Common;
using OPEX.DWEAS.Client;

namespace OPEX.Agents.GD
{
    public class GDPricer
    {
        private readonly Logger _logger;
        private readonly GDAgent _agent;

        private BeliefFunction _beliefFunction;        
        private ShoutHistory _history;
        private Instrument _lastInstrument;
        private double _lastPrice = double.NaN;

        public GDPricer(GDAgent agent)
        {
            _agent = agent;
            _logger = new Logger(string.Format("GDPricer({0})", _agent.Name));
        }

        public double LastPriceComputed { get { return _lastPrice; } }

        public void ReceiveShout(Shout shout)
        {
            _history.Add(shout, _agent.AgentStatus.LastSnapshot);
        }

        public void Init()
        {
            if (_lastInstrument == null || !_lastInstrument.Ric.Equals(_agent.AgentStatus.CurrentInstrument.Ric))
            {
                double alpha = (double)_agent.Parameters.GetValue("Alpha", typeof(double));
                double tau = (double)_agent.Parameters.GetValue("Tau", typeof(double));
                int windowSize = (int)_agent.Parameters.GetValue("WindowSize", typeof(int));
                double gracePeriodSecs = (double)_agent.Parameters.GetValue("GracePeriodSecs", typeof(double));

                _lastInstrument = _agent.AgentStatus.CurrentInstrument;
                _history = new ShoutHistory(_lastInstrument.MinPrice, _lastInstrument.MaxPrice, _agent.Name, alpha, windowSize, gracePeriodSecs, tau);                
                _beliefFunction = new BeliefFunction(_history, _agent.AgentStatus.CurrentAssignmentBucket.Side, _lastInstrument.MinPrice, _lastInstrument.MaxPrice, _agent.Name);
            }
        }

        public bool Price() 
        {
            bool success = true;
            OrderSide side = _agent.AgentStatus.CurrentAssignmentBucket.Side;
            double searchIntervalMin = _beliefFunction.OB;
            double searchIntervalMax = _beliefFunction.OA;
            double limitPrice = _agent.AgentStatus.CurrentAssignmentBucket.Price;            

            if (side == OrderSide.Sell)
            {
                if (searchIntervalMin < limitPrice)
                {
                    if (searchIntervalMax < limitPrice)
                    {
                        _logger.Trace(LogLevel.Warning, "Price. Cost {0} is greater than [{1}, {2}]. Can't do better than {0}.", limitPrice, searchIntervalMin, searchIntervalMax);
                        success = false;
                    }
                    else
                    {
                        searchIntervalMin = limitPrice;
                    }
                }
            }
            else
            {
                if (searchIntervalMax > limitPrice)
                {
                    if (searchIntervalMin > limitPrice)
                    {
                        _logger.Trace(LogLevel.Warning, "Price. Value {0} is less than [{1}, {2}]. Can't do better than {0}.", limitPrice, searchIntervalMin, searchIntervalMax);
                        success = false;
                    }
                    else
                    {
                        searchIntervalMax = limitPrice;
                    }
                }
            }

            if (success)
            {
                _beliefFunction.Build();
                ProfitFunction profitFunction = new ProfitFunction(_beliefFunction, _agent.AgentStatus.CurrentAssignmentBucket.Price, _agent.AgentStatus.CurrentAssignmentBucket.Side, _lastInstrument, _agent.Name);
                _lastPrice = Math.Round(profitFunction.MaxPoint(searchIntervalMin, searchIntervalMax));
            }

            return success;
        }

        public void ResetBestPrices()
        {
            _history.ResetBestPrices();
        }
    }
}
