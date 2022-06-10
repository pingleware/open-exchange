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
using System.Text;
using System.Threading;
using System.Messaging;

using OPEX.Common;
using OPEX.Configuration.Client;
using OPEX.Messaging;
using OPEX.AS.Service;
using OPEX.SupplyService.Common;
using OPEX.MDS.Client;
using OPEX.MDS.Common;
using OPEX.Storage;

namespace OPEX.AS.Service
{
    /// <summary>
    /// Plays SimulationJobs. Handles multiple
    /// SimulationJob in a queue, and exposes methods
    /// to play, pause, stop, restart the execution.
    /// Raises events on control signals and when a
    /// new SimulationPhase starts.
    /// </summary>
    public class AssignmentService : IOPEXService
    {
        enum Status
        {
            Paused,
            Playing
        };

        private readonly SummaryWriter _summaryWriter;
        private readonly Logger _logger;        
        private readonly Thread _mainThread;
        private readonly ManualResetEvent _reset;
        private readonly AutoResetEvent _newMessage;
        private readonly Queue<SimulationJob> _jobs;
        private readonly IMessageFormatter _formatter;
        private readonly AutoResetEvent _currentJobReset;
        private readonly AutoResetEvent _sessionOpen;
        private readonly AutoResetEvent _jobControl;

        private Channel _broadcastChannel;
        private SimulationJob _currentJob;
        private SessionChangedEventArgs _currentSessionArgs;
        private Status _status;
        private SimulationPhase _currentPhase;        
        private SimulationStep _currentStep;
        private int _currentSubStepID = -1;
        private bool _terminating = false;

        private IEnumerator _stepEnumerator;
        private IEnumerator _phaseEnumerator;
        private bool _newSimulationStarted;


        /// <summary>
        /// Initialises a  new instance of the class
        /// OPEX.AS.Service.AssignmentService;
        /// </summary>
        public AssignmentService()
        {
            _logger = new Logger("AssignmentService");

            _jobs = new Queue<SimulationJob>();
            _newMessage = new AutoResetEvent(false);
            _reset = new ManualResetEvent(false);
            _currentJobReset = new AutoResetEvent(false);
            _sessionOpen = new AutoResetEvent(false);
            _jobControl = new AutoResetEvent(false);

            _mainThread = new Thread(MainThread);
            _currentJob = null;
            _formatter = new BinaryMessageFormatter();
            _summaryWriter = new SummaryWriter(DBConnectionManager.Instance.Connection);
            _status = Status.Paused;
        }

        /// <summary>
        /// Gets the SimulationJob that's currently being processed.
        /// </summary>
        public SimulationJob CurrentJob { get { return _currentJob; } }

        /// <summary>
        /// Gets the sub-step of the SimulationStep that's currently being processed.
        /// </summary>
        public int CurrentSubStepID { get { return _currentSubStepID; } }

        /// <summary>
        /// Gets the SimulationStep that's currently being processed.
        /// </summary>
        public SimulationStep CurrentStep { get { return _currentStep; } }

        /// <summary>
        /// Gets the SimulationPhase that's currently being processed.
        /// </summary>
        public SimulationPhase CurrentPhase { get { return _currentPhase; } }

        /// <summary>
        /// Occurs when a new SimulationJob is started.
        /// </summary>
        public event EventHandler StartedProcessingNewJob;

        /// <summary>
        /// Occurs when a SimulationJob finishes.
        /// </summary>
        public event EventHandler FinishedProcessingJob;

        /// <summary>
        /// Occurs when AssignmentService is reset.
        /// </summary>
        public event EventHandler ResetPerformed;

        /// <summary>
        /// Occurs when execution is paused.
        /// </summary>
        public event EventHandler PausePerformed;

        /// <summary>
        /// Occurs when execution is resumed.
        /// </summary>
        public event EventHandler ResumePerformed;

        /// <summary>
        /// Occurs when a new phase is broadcast.
        /// </summary>
        public event EventHandler NewPhaseBroadcasted;        

        /// <summary>
        /// Pushes a SimulationJob in the processing queue.
        /// </summary>
        /// <param name="job">The SimulationJob to be pushed in the processing queue.</param>
        /// <param name="jobRepeat">The number of times to repeat the SimulationJob.</param>
        public void EnqueueJob(SimulationJob job, int jobRepeat)
        {
            _logger.Trace(LogLevel.Method, "EnqueueJob. Enqueueing {1} times new job: {0}", job.ToString(), jobRepeat);
            for (int i = 0; i < jobRepeat; ++i)
            {
                _jobs.Enqueue(job);
            }
            _newMessage.Set();
        }

        /// <summary>
        /// Pushes a SimulationJob in the processing queue,
        /// to be executed only once.
        /// </summary>
        /// <param name="job">The SimulationJob to be pushed in the processing queue.</param>
        public void EnqueueJob(SimulationJob job)
        {
            EnqueueJob(job, 1);
        }

        /// <summary>
        /// Resets AssignmentService.
        /// </summary>
        public void Reset()
        {
            _logger.Trace(LogLevel.Method, "Reset. RESETTING job queue... ################################");
            _currentJobReset.Set();
            InvokeHandler(ResetPerformed);            
        }      

        /// <summary>
        /// Plays or resumes the current SimulationJob.
        /// </summary>
        public void Play()
        {
            _logger.Trace(LogLevel.Method, "Play. Playing job...");
            _status = Status.Playing;
            _jobControl.Set();
        }

        /// <summary>
        /// Pauses the execution of the current SimulationJob.
        /// </summary>
        public void Pause()
        {
            _logger.Trace(LogLevel.Method, "Pause. Playing job...");
            _status = Status.Paused;
            _jobControl.Set();
        }       

        #region IOPEXService Members

        private bool _hasStarted;
        public bool Start()
        {
            if (_hasStarted)
            {
                _logger.Trace(LogLevel.Critical, "Can't start service because it's already started");
                return false;
            }

            _broadcastChannel = new BroadcastChannel<AssignmentMessage>(
                "ASBroadcastQueueChannel",
                null,
                "ASBroadcastQueue",
                0,
                AssignmentMessage.DefaultMulticastAddress,
                BroadcastChannelType.Sender);

            if (!_broadcastChannel.Init())
            {
                return false;
            }

            MarketDataClient.Instance.SessionChanged += new SessionChangedEventHandler(SessionChanged);

            _mainThread.Start();

            return _hasStarted = true;
        }     

        public bool Stop()
        {
            if (!_hasStarted)
            {
                _logger.Trace(LogLevel.Critical, "Can't stop service because hasn't started yet");
                return false;
            }

            _hasStarted = false;
            MarketDataClient.Instance.SessionChanged -= new SessionChangedEventHandler(SessionChanged);
            _reset.Set();

            return _mainThread.Join(5000);
        }

        #endregion

        private void InvokeHandler(EventHandler Handler)
        {
            EventHandler handler = Handler;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        private void MainThread()
        {
            WaitHandle[] EventArray = new WaitHandle[] {
                _reset,
                _newMessage
            };

            _logger.Trace(LogLevel.Method, "MainThread started ###############################################");

            MarketDataClient.Instance.RequestStatus(MarketDataClient.DefaultDataSource);

            while (WaitHandle.WaitAny(EventArray) != 0)
            {
                try 
                {
                    _summaryWriter.ResetSimID();
                    while (_jobs.Count > 0)
                    {
                        _currentJob = _jobs.Dequeue();
                        InvokeHandler(StartedProcessingNewJob);
                        ProcessJob();
                        if (!_terminating)
                        {
                            InvokeHandler(FinishedProcessingJob);
                        }
                        _currentJob = null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Trace(LogLevel.Error, "MainThread. Exception: {0} {1}",
                        ex.Message, ex.StackTrace.Replace(Environment.NewLine, " ").Replace("\n", " "));
                }
            }

            _logger.Trace(LogLevel.Method, "MainThread terminated ###############################################");
        }

        private bool InitEnumerators()
        {
            SimulationJob job = _currentJob;

            _logger.Trace(LogLevel.Info, "InitEnumerators. Current job: {0}", job.ToString());

            _stepEnumerator = job.Steps.Values.GetEnumerator();
            if (!_stepEnumerator.MoveNext())
            {
                _logger.Trace(LogLevel.Warning, "InitEnumerators. No steps in this job. End of job processing.");
                return false;
            }

            _currentStep = _stepEnumerator.Current as SimulationStep;
            _logger.Trace(LogLevel.Info, "InitEnumerators. Current step: {0}", _currentStep.ToString());

            _phaseEnumerator = _currentStep.Phases.GetEnumerator();
            if (!_phaseEnumerator.MoveNext())
            {
                _logger.Trace(LogLevel.Warning, "InitEnumerators. No phases in this step. End of job processing.");
                return false;
            }
            _currentPhase = _phaseEnumerator.Current as SimulationPhase;
            _currentSubStepID = 0;
            _newSimulationStarted = true;

            return true;
        }

        private void ProcessJob()
        {            
            WaitHandle[] EventArray = new WaitHandle[] {
                _currentJobReset,
                _reset,
                _sessionOpen,
                _jobControl
            };

            _logger.Trace(LogLevel.Method, "ProcessJob. BEGIN");

            if (!InitEnumerators())
            {
                return;
            }
            // now we have a valid currentPhase and currentStep

            int currentStepRepeatCount = 0;
            int reset = -1;            
            Status _lastStatus = _status;
            /*
             * 0 _currentJobReset,
             * 1 _reset,
             * 2 _sessionOpen,
             * 3 _jobControl
             */
            while ((reset = WaitHandle.WaitAny(EventArray)) > 1)
            {
                if (reset == 3)
                {
                    _logger.Trace(LogLevel.Info, "ProcessJob. PLAY/PAUSE request received.");
                    if (_lastStatus != _status)
                    {
                        if (_status == Status.Paused)
                        {
                            _logger.Trace(LogLevel.Info, "ProcessJob. Service Paused. Waiting for next session.");
                            InvokeHandler(PausePerformed);
                        }
                        else
                        {
                            _logger.Trace(LogLevel.Info, "ProcessJob. Service Resumed. Waiting for next session.");
                            InvokeHandler(ResumePerformed);
                        }
                    }
                    _lastStatus = _status;
                    continue;
                }
                if (_status == Status.Paused)
                {
                    _logger.Trace(LogLevel.Info, "ProcessJob. Service Paused. Waiting for next session.");
                    continue;
                }

                BroadcastAssignments(_currentPhase);
                InvokeHandler(NewPhaseBroadcasted);
                _newSimulationStarted = false;

                bool noMorePhasesInThisStep = !_phaseEnumerator.MoveNext();
                bool moveToNextStep = false;
                if (noMorePhasesInThisStep)
                {
                    // either repeat this step or move to the next one

                    _logger.Trace(LogLevel.Info, "ProcessJob. No more phases in this step.");

                    if (_currentStep.Repeat <= 0)
                    {
                         // repeat this step
                         _logger.Trace(LogLevel.Info, "ProcessJob. Repeating this step UNTIL RESET");
                         _currentSubStepID = -1;
                    }
                    else
                    {
                        // perhaps repeat the current step
                        ++currentStepRepeatCount;
                        _logger.Trace(LogLevel.Debug, "ProcessJob. currentStepRepeatCount {0} currentStep.Repeat {1}", 
                            currentStepRepeatCount, _currentStep.Repeat);
                        if (currentStepRepeatCount >= _currentStep.Repeat)
                        {
                            // move to next step
                            moveToNextStep = true;                            
                        }
                        else
                        {
                            // repeat this step
                            _logger.Trace(LogLevel.Info, "ProcessJob. Repeating this step ({0} cycle(s) left including this one)", 
                                _currentStep.Repeat - currentStepRepeatCount);
                            _currentSubStepID = -1;
                        }
                    }

                    if (moveToNextStep)
                    {
                        _currentSubStepID = 0;
                        _logger.Trace(LogLevel.Info, "ProcessJob. Already cycled through this step's phases {0} time(s). Moving to next step", 
                            _currentStep.Repeat);

                        currentStepRepeatCount = 0;
                        if (!_stepEnumerator.MoveNext())
                        {
                            _logger.Trace(LogLevel.Method, "ProcessJob. No more steps in this job. End of job processing. ==================================");
                            if (_jobs.Count > 0)
                            {
                                _currentPhase = null;
                                if (!_terminating)
                                {
                                    InvokeHandler(FinishedProcessingJob);
                                }
                                _currentJob = null;
                                _summaryWriter.ResetSimID();
                                _currentJob = _jobs.Dequeue();
                                InvokeHandler(StartedProcessingNewJob);
                                currentStepRepeatCount = 0;
                                InitEnumerators();                                
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                        _currentStep = _stepEnumerator.Current as SimulationStep;
                    }
                }
                else
                {
                    _logger.Trace(LogLevel.Info, "ProcessJob. There are more phases in this step. Moving to next phase.");                    
                }

                _phaseEnumerator = _currentStep.Phases.GetEnumerator();
                _phaseEnumerator.MoveNext();
                _currentPhase = _phaseEnumerator.Current as SimulationPhase;
                ++_currentSubStepID;
            }

            if (reset == 0)
            {
                _logger.Trace(LogLevel.Info, "ProcessJob. JOB RESET sensed. Flushing queue #########################################");
                _jobs.Clear();
                _currentPhase = null;
            }
            else if (reset == 1)
            {
                _logger.Trace(LogLevel.Info, "ProcessJob. SERVICE RESET sensed. Flushing queue #########################################");
                _jobs.Clear();
                _currentPhase = null;
                _terminating = true;
            }

            _status = Status.Paused;            

            _logger.Trace(LogLevel.Method, "ProcessJob. END");
        }      
      
        private void SessionChanged(object sender, SessionChangedEventArgs sessionState)
        {
            _logger.Trace(LogLevel.Method, "SessionChanged. sessionState: {0}", sessionState.ToString());
            _currentSessionArgs = sessionState;

            if (!sessionState.ServerAlive)
            {
                _logger.Trace(LogLevel.Warning, "SessionChanged. Server id DEAD. Trying again...");
                MarketDataClient.Instance.RequestStatus(MarketDataClient.DefaultDataSource);
                return;
            }

            if (sessionState.IsBroadcast)
            {                                
                if (sessionState.SessionState == SessionState.Open)
                {
                    _sessionOpen.Set();
                }
            }
        }        

        private void BroadcastAssignments(SimulationPhase currentPhase)
        {
            Dictionary<string, AssignmentBatch> batches = new Dictionary<string, AssignmentBatch>();

            foreach (Assignment assignment in currentPhase.Assignments)
            {
                if (!batches.ContainsKey(assignment.ApplicationName))
                {
                    batches[assignment.ApplicationName] = new AssignmentBatch(assignment.ApplicationName);
                }
                AssignmentBatch b = batches[assignment.ApplicationName];
                b.Add(assignment);
            }

            foreach (string applicationName in batches.Keys)
            {
                _logger.Trace(LogLevel.Info, "BroadcastAssignments. Broadcasting assignments for phase {0}, applicationName {1}", currentPhase.PhaseID, applicationName);
                AssignmentBatch batch = batches[applicationName];
                try
                {
                    AssignmentMessage msg = AssignmentMessageFactory.CreateAssignmentMessage(batch, _currentSessionArgs.EndTime, _newSimulationStarted);
                    SendBroadcastMessage(msg);
                }
                catch (Exception ex)
                {
                    _logger.Trace(LogLevel.Error, "BroadcastAssignments. Exception: {0} {1}",
                        ex.Message, ex.StackTrace.Replace(Environment.NewLine, " ").Replace("\n", " "));
                }
            }

            _summaryWriter.Write(currentPhase, _currentSessionArgs.StartTime, _currentSessionArgs.EndTime);
        }            

        private void SendBroadcastMessage(AssignmentMessage msg)
        {
            try
            {
                _logger.Trace(LogLevel.Info,
                    "SendBroadcastMessage. About to send a message on queue {0}...", _broadcastChannel.Queue.Path);
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
    }
}
