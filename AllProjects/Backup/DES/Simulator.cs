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
using System.Threading;

using OPEX.Common;
using OPEX.DES.Simulation;
using OPEX.DES.Exchange;
using OPEX.DES.DB;

namespace OPEX.DES
{
    public class Simulator
    {
        private readonly Random _random;

        private readonly GlobalOrderBook _gob;
        private readonly JobLoader _jobLoader;
        private readonly Logger _logger;
        private readonly ManualResetEvent _mainThreadReset;
        private Thread _mainThread;
        private bool _isRunning;
        private readonly SummaryWriter _summaryWriter;
        private readonly List<DESAgent> _agents;

        public Simulator(ICollection<DESAgent> agents, JobLoader jl, GlobalOrderBook gob)
        {
            _logger = new Logger("Simulator");
            _gob = gob;
            _jobLoader = jl;
            _mainThreadReset = new ManualResetEvent(false);
            _isRunning = false;
            _agents = new List<DESAgent>();
            _agents.AddRange(agents);   
            _random = new Random(this.GetHashCode() ^ _agents.GetHashCode());
            _summaryWriter = new SummaryWriter();
        }
        
        public void Start() 
        {
            if (_isRunning)
            {
                _logger.Trace(LogLevel.Critical, "Start. Cannot start Simulator: already running!");
                return;
            }

            _isRunning = true;

            _mainThread = new Thread(new ThreadStart(SimulatorMainThread));
            _mainThreadReset.Reset();
            _mainThread.Start();

            _logger.Trace(LogLevel.Method, "Start. Simulator STARTED");
        }

        public void Stop() 
        {
            if (!_isRunning)
            {
                _logger.Trace(LogLevel.Critical, "Stop. Cannot stop Simulator: not running!");
                return;
            }            

            _mainThreadReset.Set();            
            _mainThread.Join(1000);

            _isRunning = false;

            _logger.Trace(LogLevel.Method, "Stop. Simulator STOPPED");
        }

        
        private IEnumerator<SimulationJob> MoveToNextJob()
        {
            foreach (SimulationJob job in _jobLoader.Jobs.Values)
            {
                for (int i = 0; i < job.Repeat; ++i)
                {
                    yield return job;
                }
            }            
        }

        private void SimulatorMainThread()
        {
            IEnumerator<SimulationJob> jobEnum = MoveToNextJob();
            if (!jobEnum.MoveNext()) return;
            bool newSimulation = true;
            SimulationJob currentJob = jobEnum.Current;
            IEnumerator<SimulationPhase> phaseEnum = currentJob.GetEnumerator();
            TimeManager.Reset();
            DateTime start = DateTime.Now;

            _logger.Trace(LogLevel.Method, "SimulatorMainThread. Simulation started");

            while (!_mainThreadReset.WaitOne(0))
            {
                try
                {
                    bool endOfSimulation = false;

                    if (TimeManager.CurrentTimeStamp.Move == 0)
                    {
                        while (!phaseEnum.MoveNext())
                        {
                            // phase finished --> switch to next job
                            if (!jobEnum.MoveNext())
                            {
                                endOfSimulation = true;
                                break;
                            }
                            else
                            {
                                // new SIMID
                                TimeManager.NextSimulation();
                                newSimulation = true;
                            }
                            currentJob = jobEnum.Current;
                            phaseEnum = currentJob.GetEnumerator();
                        }
                        if (endOfSimulation)
                        {
                            break;
                        }

                        SimulationPhase currentPhase = phaseEnum.Current;
                        foreach (DESAgent agent in _agents)
                        {
                            agent.LoadAssignments(currentPhase, newSimulation);
                        }
                        newSimulation = false;

                        _gob.Clear();
                        NotifyMarketData(false, true);

                        _logger.Trace(LogLevel.Method, "SimulatorMainThread. NEW ROUND STARTED. [Round {0}] Writing summary to DB.", TimeManager.CurrentTimeStamp.Round);
                        _summaryWriter.Write(currentPhase);
                    }

                    MakeAMove();

                    TimeManager.NextMove();
                }
                catch (Exception ex)
                {
                    _logger.Trace(LogLevel.Critical, "SimulatorMainThread. EXCEPTION: {0} {1}", 
                        ex.Message, ex.StackTrace.Replace(Environment.NewLine, null).Replace('\n', '\0'));
                }
            }
            TimeSpan simLength = DateTime.Now.Subtract(start);
            _logger.Trace(LogLevel.Method, "SimulatorMainThread. Simulation finished. Time elapsed: {0}", simLength.ToString());
        }

        private void NotifyMarketData(bool newShout, bool newDepth)
        {
            foreach (DESAgent agent in _agents)
            {
                agent.NotifyMarketData(newShout, newDepth);
            }
        }

        private void MakeAMove()
        {
            bool newShout = false;
            bool newDepth = false;
            int n = _random.Next(0, _agents.Count);
            DESAgent currentAgent = _agents[n];
            string s = string.Format(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>> BEGIN MOVE. Timestamp {0}. Agent #{1} ({2}).", TimeManager.CurrentTimeStamp.ToString(), n, currentAgent.Name);
            _logger.Trace(LogLevel.Method, s);
            currentAgent.Play(out newShout, out newDepth);
            NotifyMarketData(newShout, newDepth);
            s = string.Format("<<<<<<<<<<<<<<<<<<<<<<<<<< END MOVE. Timestamp {0}. Agent #{1} ({2}).", TimeManager.CurrentTimeStamp.ToString(), n, currentAgent.Name);
            _logger.Trace(LogLevel.Method, s);
        }

        internal void WaitToFinish()
        {
            _mainThread.Join();
        }
    }
}
