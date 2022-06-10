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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OPEX.Common;
using OPEX.DES.OrderManager;
using OPEX.DES.Exchange;

namespace OPEX.DES.GDX
{
    public class ShoutHistory
    {
        private readonly double Alpha;
        private readonly int WindowsSize;
        private readonly double GracePeriodSecs;
        private readonly double PMIN;
        private readonly double PMAX;
        private readonly double TAU;

        private Logger _logger;
        private SortedDictionary<double, PriceBucket> _priceBuckets;
        private Queue<long> _tradesIDs;
        private double _bestBid;
        private double _bestAsk;

        public ShoutHistory(double pmin, double pmax, string name, double alpha, int windowSize, double gracePeriodSecs, double tau)
        {
            _logger = new Logger(string.Format("ShoutHistory({0})", name));

            PMAX = pmax;
            PMIN = pmin;
            TAU = tau;
            Alpha = alpha;
            WindowsSize = windowSize;
            GracePeriodSecs = gracePeriodSecs;
            ResetBestPrices();

            _tradesIDs = new Queue<long>();            
            _priceBuckets = new SortedDictionary<double, PriceBucket>();
        }      

        public double BestBid { get { return _bestBid; } }
        public double BestAsk { get { return _bestAsk; } }
        
        public List<double> Domain
        {
            get 
            {
                List<double> domain = new List<double>();
                domain.AddRange(_priceBuckets.Keys);
                return domain; 
            }
        }
        
        public void Add(Shout shout, AggregatedDepthSnapshot lastSnapshot)
        {
            bool useMarketData = true;
            _logger.Trace(LogLevel.Debug, "Add. Received shout: {0}", shout);
            _logger.Trace(LogLevel.Debug, "Add. Current Best prices: OB = {0}, OA = {1}", _bestBid, _bestAsk);

            if (useMarketData)
            {
                double bBid = lastSnapshot.Buy.Best;
                double bAsk = lastSnapshot.Sell.Best;

                if (bBid == 0.0)
                {
                    bBid = PMIN;
                }
                if (bAsk == 0.0)
                {
                    bAsk = PMAX;
                }

                _bestBid = bBid;
                _bestAsk = bAsk;
            }
            else
            {
                if (shout.Accepted)
                {
                    ResetBestPrices();
                }
                else
                {
                    if (shout.Side == OrderSide.Buy)
                    {
                        if (shout.Price > _bestBid)
                        {
                            _bestBid = shout.Price;
                        }
                    }
                    else
                    {
                        if (shout.Price < _bestAsk)
                        {
                            _bestAsk = shout.Price;
                        }
                    }
                }
            }

            _logger.Trace(LogLevel.Debug, "Add. Updated Best prices: OB = {0}, OA = {1}", _bestBid, _bestAsk); 
            
            if (shout.Accepted)
            {
                _tradesIDs.Enqueue(shout.ID);

                _logger.Trace(LogLevel.Debug, "Add. ShoutID {0} enqueued. We now have {1} trades in memory",
                shout.ID, _tradesIDs.Count);

                if (_tradesIDs.Count > WindowsSize)
                {
                    _logger.Trace(LogLevel.Info, "Add. TRUNCATING QUEUE: More than {0} trades (i.e. ACCEPTED shouts) detected in the queue.", WindowsSize);

                    long id = _tradesIDs.Dequeue();
                    int before = 0;
                    int after = 0;
                    List<double> emptyBuckets = new List<double>();                         
                    foreach (PriceBucket pb in _priceBuckets.Values)
                    {
                        double p = pb.Price;
                        int b = pb.Count;                        
                        pb.Truncate(id);
                        int a = pb.Count;
                        _logger.Trace(LogLevel.Debug, "Add. TRUNCATING QUEUE: (price = {2}) #Shouts BEFORE truncation: {0}. #Shouts AFTER truncation: {1}.", b, a, p);
                        before += b;
                        after += a;
                        if (a == 0)
                        {
                            _logger.Trace(LogLevel.Debug, "Add. TRUNCATING QUEUE: pricebucket @ {0} is EMPTY and will be removed.", p);
                            emptyBuckets.Add(p);
                        }
                    }
                    foreach (double p in emptyBuckets)
                    {
                        _priceBuckets.Remove(p);
                    }
                    _logger.Trace(LogLevel.Info, "Add. TRUNCATING QUEUE: #Shouts BEFORE truncation: {0}. #Shouts AFTER truncation: {1}.", before, after);
                }
            }
            
            AddShoutToPriceBuckets(shout);
            _logger.Trace(LogLevel.Debug, "Add. Updated history: {0}", this.ToString());    
        }        

        public void Clear()
        {            
            _tradesIDs.Clear();
            _priceBuckets.Clear();
            ResetBestPrices();
        }

        public string Dump()
        {
            StringBuilder sb = new StringBuilder();

            foreach (PriceBucket pb in _priceBuckets.Values)
            {
                sb.AppendFormat("{0} ", pb.Dump());
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (PriceBucket pb in _priceBuckets.Values)
            {                
                sb.AppendFormat("{0} ", pb.ToString());
            }

            return sb.ToString();
        }

        public double FindExp(double price, ShoutFilter shoutFilter, OrderSide orderSide, bool greaterThan)
        {
            ///
            /// Note: tau represents roughly the time after which sum terms
            /// are attenuated by more that 37%. 
            ///            
            double weightedSum = 0.0;

            foreach (PriceBucket pb in _priceBuckets.Values)
            {
                if (greaterThan && pb.Price < price)
                {
                    continue;
                }
                else if (!greaterThan && pb.Price > price)
                {
                    continue;
                }

                TimeStamp lastTimeStamp = TimeManager.CurrentTimeStamp;
                PriceBucketSide pbs = pb[orderSide];
                List<Shout> l = new List<Shout>();

                foreach (Shout shout in pbs)
                {
                    bool isAccepted = (shout.Accepted &&
                        TimeManager.CurrentTimeStamp.Subtract(shout.TimeStamp) <= GracePeriodSecs);

                    if ((isAccepted && (shoutFilter & ShoutFilter.Accepted) != 0)
                        || (!isAccepted && (shoutFilter & ShoutFilter.Rejected) != 0))
                    {
                        l.Add(shout);
                    }
                }

                foreach (Shout s in l)
                {
                    weightedSum += Math.Exp(-(double)lastTimeStamp.Subtract(s.TimeStamp) / TAU);
                }
            }       

            return weightedSum;
        }
        
        public double Find(double price, ShoutFilter shoutFilter, OrderSide orderSide, bool greaterThan)
        {
            int n = 0;

            foreach (PriceBucket pb in _priceBuckets.Values)
            {
                if (greaterThan && pb.Price < price)
                {
                    continue;
                }
                else if (!greaterThan && pb.Price > price)
                {
                    continue;
                }

                PriceBucketSide pbs = pb[orderSide];
                if (shoutFilter == ShoutFilter.All)
                {
                    n += pbs.Count;
                }
                else if ((shoutFilter & ShoutFilter.Accepted) != 0)
                {
                    n += pbs.CountAccepted;
                }
                else if ((shoutFilter & ShoutFilter.Rejected) != 0)
                {
                    n += pbs.CountRejected;
                }
            }

            return (1.0 - Math.Pow(1.0 - Alpha, n)) / Alpha;
        }
      
        private void AddShoutToPriceBuckets(Shout s)
        {
            if (s == null)
            {
                return;
            }

            double price = s.Price;
            if (!_priceBuckets.ContainsKey(price))
            {
                _priceBuckets.Add(price, new PriceBucket(price, GracePeriodSecs));
            }
            PriceBucket pb = _priceBuckets[price];

            pb.Add(s);
        }

        public void ResetBestPrices()
        {
            _bestBid = PMIN;
            _bestAsk = PMAX;
            _logger.Trace(LogLevel.Debug, "ResetBestPrices. Current Best prices: OB = {0}, OA = {1}", _bestBid, _bestAsk); 
        }
    }

    [Flags]
    public enum ShoutFilter : int
    {
        None = 0,
        Accepted = 1,
        Rejected = 2,
        All = 3
    }  
}
