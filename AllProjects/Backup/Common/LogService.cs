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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace OPEX.Common
{
    /// <summary>
    /// Asynchronously and periodically dispatches traces to
    /// one or more Logs, which can be added and removed dynamically.
    /// </summary>
    public class LogService : Log
    {
        private static readonly int DefaultMainLoopTimeoutMsec = 1000;
        protected static LogService _theInstance = null;

        private bool _running = false;
        private Hashtable _logs;
        private Thread _logThread;
        private Queue _messageQueue;
        private AutoResetEvent _newMessage;

        private LogService()
            : base()
        {
            _logs = Hashtable.Synchronized(new Hashtable());
            _messageQueue = Queue.Synchronized(new Queue());
            _newMessage = new AutoResetEvent(false);
            _running = true;

            _logThread = new Thread(new ThreadStart(LogMainLoop));
        }

        /// <summary>
        /// Returns the (only) instance of the OPEX.Common.LogService class.
        /// </summary>
        public static LogService MainInstance
        {
            get
            {
                if (_theInstance == null)
                {
                    _theInstance = new LogService();
                }

                return _theInstance;
            }
        }

        /// <summary>
        /// Retrieves an attached log by its ID.
        /// </summary>
        /// <param name="logID">The ID of the attached Log to retrieve.</param>
        /// <returns>The attached log identified by logID, if found. Else null.</returns>
        public Log this[string logID]
        {
            get
            {
                Log log = null;

                if (_logs.ContainsKey(logID))
                {
                    log = _logs[logID] as Log;
                }

                return log;
            }
        }

        /// <summary>
        /// Attaches a Log.
        /// </summary>
        /// <param name="log">The Log to attach.</param>
        public void Attach(Log log)
        {
            Validatelog(log);

            if (_logs.ContainsKey(log.ID))
            {
                throw new ApplicationException("Can't attach Log - Log already attached!");
            }

            if (log.ID.Equals(this.ID))
            {
                throw new ApplicationException("Can't attach a Log to itself! That's nasty!!");
            }

            _logs.Add(log.ID, log);
        }
        /// <summary>
        /// Detaches a Log.
        /// </summary>
        /// <param name="log">The Log to detach.</param>
        public void Detach(Log log)
        { 
             Validatelog(log);

            if (!_logs.ContainsKey(log.ID))
            {
                throw new ApplicationException("Can't detach Log - Log not attached!");
            }

            _logs.Remove(log.ID);
        }

        private void Validatelog(Log log)
        {
            if (log == null)
            {
                throw new NullReferenceException("Null log");
            }

            if (log.ID == null)
            {
                throw new NullReferenceException("Null Log ID");
            }
        }

        /// <summary>
        /// Starts the MainLoop. Any line traced before this call,
        /// and all the succeeding ones, will be dispatched to the
        /// attached Logs.
        /// </summary>
        public override void Start()
        {
            foreach (Log log in _logs.Values)
            {
                log.Start();
            }

            _logThread.Start();            
        }

        /// <summary>
        /// Stops the MainLoop. No more lines will be dispatched to
        /// the attached Logs after this call, although they will be
        /// kept in a queue.
        /// </summary>
        public override void Stop()
        {
            _running = false;

            try
            {
                if (_logThread.IsAlive)
                {
                    _logThread.Join();
                }
            }
            finally { }

            foreach (Log log in _logs.Values)
            {
                log.Stop();
            }
        }
        
        private void LogMainLoop()
        {
            while (_running) 
            {
                _newMessage.WaitOne(DefaultMainLoopTimeoutMsec, false);

                while (_messageQueue.Count > 0)
                {
                    LogTrace logTrace = _messageQueue.Dequeue() as LogTrace;

                    foreach (Log log in _logs.Values)
                    {
                        log.Trace(logTrace.Level, logTrace.Message, logTrace.Args);
                    }
                }
            }
        }

        protected override void InnerTrace(LogLevel level, string format, params object[] args)
        {
            _messageQueue.Enqueue(new LogTrace(level, format, args));
            _newMessage.Set();            
        }
    }
}
