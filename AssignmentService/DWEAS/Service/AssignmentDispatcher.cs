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
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Messaging;

using OPEX.Configuration.Client;
using OPEX.Common;
using OPEX.Messaging;
using OPEX.AS.Service;
using OPEX.Storage;
using OPEX.SupplyService.Common;
using OPEX.MDS.Client;

using MySql.Data;
using MySql.Data.MySqlClient;

namespace OPEX.DWEAS.Service
{
    class AssignmentDispatcher
    {
        private readonly Logger _logger;        
        private readonly ManualResetEvent _reset;
        private readonly IMessageFormatter _formatter;        
        private readonly Dictionary<string, int> _idByName;
        private readonly Channel _broadcastChannel;
        private readonly RandomIDGenerator _simIDGenerator;
        
        private SortedDictionary<int, List<ScheduleEntry>> _currentTimeTable;
        private Thread _mainThread;
        private bool _playing;
        private bool _welcomeSent;
        private long _simID;

        public AssignmentDispatcher()
        {
            _logger = new Logger("AssignmentDispatcher");
            
            _broadcastChannel = new BroadcastChannel<AssignmentMessage>(
               "DWEASBroadcastQueueChannel",
               null,
               "DWEASBroadcastQueue",
               0,
               AssignmentMessage.DWEMulticastAddress,
               BroadcastChannelType.Sender);

            _simIDGenerator = RandomIDGenerator.Instance;
            _playing = false;            
            _reset = new ManualResetEvent(false);            
            _formatter = new BinaryMessageFormatter();
            _idByName = new Dictionary<string, int>();
            _broadcastChannel.Init();
        }

        public void Start(SortedDictionary<int, List<ScheduleEntry>> timeTable)
        {
            if (_playing)
            {
                return;
            }

            _welcomeSent = false;
            _currentTimeTable = timeTable;
            _reset.Reset();
            _mainThread = new Thread(new ThreadStart(this.MainThread));
            _mainThread.Start();
            _simID = _simIDGenerator.NextID();
            _playing = true;
        }

        public void Stop()
        {
            if (!_playing)
            {
                return;
            }

            _reset.Set();
            _mainThread.Join(1000);
            _playing = false;            
        }

        private void MainThread()
        {
            bool stop = false;

            _logger.Trace(LogLevel.Method, "MainThread. Started");

            try
            {
                while (!stop)
                {
                    foreach (int step in this._currentTimeTable.Keys)
                    {
                        List<ScheduleEntry> permitsAtThisTime = _currentTimeTable[step];

                        if (permitsAtThisTime.Count == 0)
                        {
                            continue;
                        }

                        int msecToWait = (int)(permitsAtThisTime[0].Time * 1000);
                        Consume(permitsAtThisTime, msecToWait);

                        _logger.Trace(LogLevel.Debug, "MainThread. Waiting {0} msec...", msecToWait);
                        if (_reset.WaitOne(msecToWait))
                        {
                            _logger.Trace(LogLevel.Info, "MainThread. Interrupted by user");
                            stop = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "MainThread. Exception: {0} {1}", ex.Message, ex.StackTrace);
            }

            _logger.Trace(LogLevel.Method, "MainThread. Terminated");
        }

        private void Consume(List<ScheduleEntry> permits, int msecToWait)
        {
            const int PhaseID = 0;

            try
            {
                if (!_welcomeSent)
                {
                    AssignmentBatch welcomeBatch = new AssignmentBatch("*");
                    AssignmentMessage welcomeMsg =
                        AssignmentMessageFactory.CreateAssignmentMessage(
                        welcomeBatch, _lastEndTime, true);
                    SendBroadcastMessage(welcomeMsg);
                    _welcomeSent = true;
                }

                foreach (ScheduleEntry entry in permits)
                {
                    _logger.Trace(LogLevel.Debug, "Consume. Processing entry: {0}", entry.ToString());

                    Assignment a = new Assignment(
                        entry.UserName,
                        PhaseID,
                        NextID(entry.UserName),
                        entry.RIC,
                        entry.CCY,
                        entry.Side,
                        entry.Qty,
                        entry.Price);

                    _logger.Trace(LogLevel.Info, "Consume. Created assignment: {0}", a.ToString());

                    AssignmentBatch batch = new AssignmentBatch(entry.UserName);
                    batch.Add(a);

                    AssignmentMessage msg = 
                        AssignmentMessageFactory.CreateAssignmentMessage(
                        batch, _lastEndTime, false);
                    SendBroadcastMessage(msg);

                    WriteToDB(a, msecToWait);
                }
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "Consume. Exception: {0}", ex.Message);
            }
        }

        private void WriteToDB(Assignment a, int waitTime)
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = DBConnectionManager.Instance.Connection;
            StringBuilder sb = new StringBuilder();

            sb.Append(@"INSERT INTO DWESimulation
                (SIMID, DateSig, TimeSig, UserName, Side, Price, Quantity, RIC, CCY, WaitTime) VALUES (");
            sb.AppendFormat("{0}, '{1}', '{2}', ", _simID, DateTime.Today.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmss.ffffff"));
            sb.AppendFormat("'{0}', '{1}', {2}, {3}, '{4}', '{5}', {6});", 
                a.ApplicationName, a.Side.ToString(), a.Price, a.Quantity, a.Ric, a.Currency, waitTime);

            cmd.CommandText = sb.ToString();
            cmd.ExecuteNonQuery();
        }

        private int NextID(string p)
        {
            if (!_idByName.ContainsKey(p))
            {
                _idByName[p] = 0;
            }
            else
            {
                _idByName[p] += 1;
            }

            return _idByName[p];
        }       

        private void SendBroadcastMessage(AssignmentMessage msg)
        {
            try
            {
                _logger.Trace(LogLevel.Info,
                    "SendBroadcastMessage. About to send a message on queue {0}...", 
                    _broadcastChannel.Queue.Path);
                _broadcastChannel.Queue.Send(new Message(msg, _formatter));
            }
            catch (MessageQueueException mex)
            {
                _logger.Trace(LogLevel.Error, "SendBroadcastMessage. MessageQueueException: {0} {1}",
                    mex.Message, mex.StackTrace.Replace(Environment.NewLine, " ").Replace("\n", " "));
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Error, "SendBroadcastMessage. Exception: {0} {1}",
                    ex.Message, ex.StackTrace.Replace(Environment.NewLine, " ").Replace("\n", " "));
            }
        }

        private DateTime _lastEndTime = DateTime.MaxValue;
        internal void SessionChanged(SessionChangedEventArgs sessionState)
        {
            _lastEndTime = sessionState.EndTime;
        }
    }
}
