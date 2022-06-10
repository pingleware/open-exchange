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
using System.Threading;

using OPEX.Common;

namespace OPEX.Agents.Common
{
    public abstract class StimulusQueue : IStimulusQueue
    {
        protected readonly Logger _logger;
        protected bool _send;
        private readonly object _root = new object();       
        protected readonly string _queueName;
        protected readonly StimulusType _type;
        private readonly Thread _mainThread;
        private readonly ManualResetEvent _reset;
        private readonly AutoResetEvent _newStimulusToSend;
        private readonly PriorityQueue<Stimulus> _queue;

        public StimulusQueue(string queueName, StimulusType queueType)
        {
            _logger = new Logger(queueName);
            _queueName = queueName;
            _type = queueType;
            _queue = new PriorityQueue<Stimulus>();
            _send = false;            
            _mainThread = new Thread(new ThreadStart(MainThread));
            _reset = new ManualResetEvent(false);
            _newStimulusToSend = new AutoResetEvent(false);
        }

        public StimulusType Type { get { return _type; } }
        public string Name { get { return _queueName; } }

        protected void SendNewStimulus(Stimulus stimulus)
        {
            lock (_root)
            {
                if (stimulus == null)
                {
                    return;
                }

                foreach (StimulusEventHandler handler in _newStatus.GetInvocationList())
                {
                    try { handler(this, new StimulusEventArgs(stimulus)); }
                    catch (Exception ex)
                    {
                        _logger.Trace(LogLevel.Critical, "SendNewStimulus. Exception: {0} {1}", ex.Message, 
                            ex.StackTrace.Replace(Environment.NewLine, " "));
                    }
                }
            }
        }
        
        private void MainThread()
        {
            _logger.Trace(LogLevel.Debug, "MainThread started.");

            WaitHandle[] EventArray = new WaitHandle[] {
                    _reset,
                    _newStimulusToSend
                };

            while (WaitHandle.WaitAny(EventArray) != 0)
            {
                while (_running && _send && _queue.Count > 0)                
                {
                    Stimulus stimulus = _queue.Dequeue();
                    SendNewStimulus(stimulus);
                }            
            }

            _logger.Trace(LogLevel.Debug, "MainThread finished.");
        }       

        protected void Enqueue(Stimulus stimulus)
        {
            int priority = stimulus.Priority;
            _queue.Enqueue(priority, stimulus);
        
            _newStimulusToSend.Set();
        }

        public void Start(bool startSending, bool startReceiving)
        {
            Start();
            if (startSending)
            {
                StartSending();
            }
            if (startReceiving)
            {
                StartReceiving();
            }
        }

        public void Stop(bool stopSending, bool stopReceiving)
        {
            if (stopReceiving)
            {
                StopReceiving();
            }
            if (stopSending)
            {
                StopSending();
            }            
            Stop();
        }

        #region IStimulusQueue Members

        private bool _running = false;

        public virtual void Start()
        {
            _newStimulusToSend.Reset();
            _reset.Reset();
            _mainThread.Start();
            _running = true;
        }

        public virtual void Stop()
        {
            _running = false;
            _reset.Set();
            _newStimulusToSend.Set();
            _mainThread.Join(1000);
        }

        public abstract void StartReceiving();
        public abstract void StopReceiving();

        public virtual void StartSending()
        {
            _send = true;
        }

        public virtual void StopSending()
        {
            _send = false;
        }

        public void Flush()
        {
            _queue.Clear();
        }

        public event StimulusEventHandler NewStimulus
        {
            add { lock (_root) { _newStatus += value; } }
            remove { lock (_root) { _newStatus -= value; } }
        }
        private StimulusEventHandler _newStatus;

        #endregion
    }
}
