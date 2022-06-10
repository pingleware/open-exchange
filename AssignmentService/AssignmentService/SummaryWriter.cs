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

using MySql.Data;
using MySql.Data.MySqlClient;

using OPEX.Common;
using OPEX.OM.Common;
using OPEX.Storage;
using OPEX.SupplyService.Common;

namespace OPEX.AS.Service
{
    /// <summary>
    /// Calculates maximum theoretical equilibrium
    /// price and maximum theoretical surplus of the market
    /// participants to a SimulationPhase, and it writes them,
    /// together with the supply and demand curve of the
    /// SimulationPhase, to the DB.
    /// </summary>
    public class SummaryWriter
    {
        private readonly RandomIDGenerator _simIDGenerator;

        private IncrementalIDGenerator _phaseIDGenerator;
        private SimulationPhase _phase;
        private DateTime _start;
        private DateTime _end;
        private Dictionary<string, double> _maxThSurplusByUser;
        private double _eqPrice;
        private readonly Logger _logger;
        private readonly MySqlConnection _connection; 
        private long _simID;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.AS.Service.SummaryWriter.
        /// </summary>
        /// <param name="connection">The MySqlConnection to use.</param>
        public SummaryWriter(MySqlConnection connection)
        {
            _logger = new Logger("SummaryWriter");
            _simIDGenerator = RandomIDGenerator.Instance;
            _connection = connection;
        }

        /// <summary>
        /// Gets the SimulationID.
        /// </summary>
        public long SimID { get { return _simID; } }

        /// <summary>
        /// Resets the SimulationID.
        /// </summary>
        public void ResetSimID()
        {
            _simID = _simIDGenerator.NextID();
            _phaseIDGenerator = new IncrementalIDGenerator();
        }

        /// <summary>
        /// Writes a SimulationPhase to the DB.
        /// </summary>
        /// <param name="phase">The SimulationPhase to write to the DB.</param>
        /// <param name="start">The start date and time of the SimulationPhase to write to the DB.</param>
        /// <param name="end">The end date and time of the SimulationPhase to write to the DB.</param>
        public void Write(SimulationPhase phase, DateTime start, DateTime end)
        {
            _phase = phase;
            _start = start;
            _end = end;

            try
            {                
                _maxThSurplusByUser = new Dictionary<string, double>();
                ComputeEquilibriumPrice();
                ComputeMaxThSurplus();
                WriteToDB();
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "Write. Exception: {0} {1}", ex.Message, ex.StackTrace.Replace(Environment.NewLine, " ").Replace("\n", " "));
            }
        }

        private void WriteToDB()
        {                        
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = _connection;
            StringBuilder sb = new StringBuilder();
            long phID = _phaseIDGenerator.NextID();

            foreach (string user in _maxThSurplusByUser.Keys)
            {
                double maxThSurplus = _maxThSurplusByUser[user];                
                sb.Remove(0, sb.Length);
                sb.Append("INSERT INTO SimulationSummary (SimID, PhaseID, UserName, PhaseStart, PhaseEnd, MaxThSplus, EqPrice, DateSig) VALUES (");
                sb.AppendFormat("{0}, {1}, '{2}', ", _simID, phID, user);
                sb.AppendFormat("'{0}', '{1}', {2}, {3}, '{4}');", _start.ToString("HHmmss.ffffff"), _end.ToString("HHmmss.ffffff"), maxThSurplus, _eqPrice, DateTime.Today.ToString("yyyyMMdd"));
                cmd.CommandText = sb.ToString();
                cmd.ExecuteNonQuery();
            }           
        }

        private void ComputeMaxThSurplus()
        {
            foreach (Assignment assignment in _phase.Assignments)
            {
                double currentSurplus = 0.0;
                if (_maxThSurplusByUser.ContainsKey(assignment.ApplicationName))
                {
                    currentSurplus = _maxThSurplusByUser[assignment.ApplicationName];
                }
                double delta = 0.0;
                if (assignment.Side == OrderSide.Buy)
                {
                    delta = Math.Max(assignment.Price - _eqPrice, 0);
                }
                else
                {
                    delta = Math.Max(-assignment.Price + _eqPrice, 0);
                }
                _maxThSurplusByUser[assignment.ApplicationName] = currentSurplus + delta;
            }
        }

        private void ComputeEquilibriumPrice()
        {
            HashSet<double> demand = new HashSet<double>();
            HashSet<double> supply = new HashSet<double>();
            List<double> lDemand = new List<double>();
            List<double> lSupply = new List<double>();

            foreach (Assignment a in _phase.Assignments)
            {
                if (a.Side == OrderSide.Buy)
                {
                    demand.Add(a.Price);
                    lDemand.Add(a.Price);
                }
                else
                {
                    supply.Add(a.Price);
                    lSupply.Add(a.Price);
                }
            }

            lSupply.Sort(new PriceComparer(true));
            lDemand.Sort(new PriceComparer(false));

            double max = Math.Max(demand.Max(), supply.Max());
            double min = Math.Min(demand.Min(), supply.Min());
            double maxThSurplus = -1;
            int maxTrades = 0;
            double eqPrice = 0;
            for (double p = min; p < max; p += 1.0)
            {
                int nTrades;
                double currentTotalSurplus = EvaluateSurplus(lSupply, lDemand, p, out nTrades);
                if (currentTotalSurplus > maxThSurplus || (currentTotalSurplus == maxThSurplus && nTrades > maxTrades))
                {
                    maxTrades = nTrades;
                    maxThSurplus = currentTotalSurplus;
                    eqPrice = p;
                }
            }
            _eqPrice = eqPrice;
        }

        private double EvaluateSurplus(List<double> supply, List<double> demand, double eqPrice, out int nTrades)
        {
            double totalSurplus = 0.0;
            nTrades = 0;

            for (int i = 0; i < supply.Count && i < demand.Count; ++i)
            {
                double pSell = supply[i];
                double pBuy = demand[i];
                if (pSell > eqPrice)
                {
                    // best seller can't trade. end.
                    break;
                }
                if (pBuy < eqPrice)
                {
                    // best buyer can't trade. end.
                    break;
                }
                totalSurplus += (pBuy - eqPrice) + (eqPrice - pSell);
                nTrades++;
            }

            return totalSurplus;
        }

        class PriceComparer : Comparer<double>
        {
            bool _ascending;
            public PriceComparer(bool ascending)
            {
                _ascending = ascending;
            }

            public override int Compare(double x, double y)
            {
                return (int)((x - y) * (_ascending ? 1.0 : -1.0));
            }
        }
    }    
}
