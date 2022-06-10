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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using MySql.Data;

using OPEX.Common;
using OPEX.AS.Service;
using OPEX.Storage;
using OPEX.MDS.Client;
using OPEX.MDS.Common;

using Timer = System.Threading.Timer;

namespace OPEX.AssignmentGUI
{
    public partial class MainForm : Form
    {
        enum GUIStatus
        {
            Connecting,
            InError,
            Idle, 
            Playing,
            Paused
        }
        private readonly Dictionary<GUIStatus, string> _statusText;        
        private readonly AssignmentService _assignmentService;
        private readonly Timer _sessionTimer;
        private readonly Logger _logger;
        private JobLoader _jobLoader;
        private GUIStatus _status;
        private SessionChangedEventArgs _currentSessionState;

        public MainForm()
        {
            InitializeComponent();

            if (DesignMode)
            {
                return;
            }

            _status = GUIStatus.Connecting;
            _statusText = new Dictionary<GUIStatus, string>();
            InitStatusText();

            _logger = new Logger("AssignmentGUI");

            _sessionTimer = new Timer(new System.Threading.TimerCallback(TimerTick), null, Timeout.Infinite, Timeout.Infinite);
            
            if (!StartServices())
            {            
                _statusText[GUIStatus.InError] = "Error: services unavailable";
                ChangeGUIStatus(GUIStatus.InError);
            }
            _assignmentService = new AssignmentService();
        }

        void _assignmentService_Resume(object sender, EventArgs e)
        {
            string name = null;
            _currentJob = _assignmentService.CurrentJob;
            if (_currentJob != null)
            {
                name = _currentJob.JobName;
            }
            _statusText[GUIStatus.Playing] = string.Format("Playing [{0}]", name);
            ChangeGUIStatus(GUIStatus.Playing);
        }

        void _assignmentService_Reset(object sender, EventArgs e)
        {
            ChangeGUIStatus(GUIStatus.Idle);
        }

        void _assignmentService_Pause(object sender, EventArgs e)
        {
            string name = null;
            _currentJob = _assignmentService.CurrentJob;
            if (_currentJob != null)
            {
                name = _currentJob.JobName;
            }
            _statusText[GUIStatus.Paused] = string.Format("Paused [{0}]", name);
            ChangeGUIStatus(GUIStatus.Paused);
        }

        void _assignmentService_FinishedProcessingJob(object sender, EventArgs e)
        {
            ChangeGUIStatus(GUIStatus.Idle);
        }

        private SimulationJob _currentJob;
        void _assignmentService_StartedProcessingNewJob(object sender, EventArgs e)
        {
            string name = null;
            _currentJob = _assignmentService.CurrentJob;
            if (_currentJob != null)
            {
                name = _currentJob.JobName;
            }
            _statusText[GUIStatus.Playing] = string.Format("Playing [{0}]", name);
            ChangeGUIStatus(GUIStatus.Playing);
        }

        private bool _closing = false;
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseGUI();
        }

        private void CloseGUI()
        {
            _closing = true;
            DialogResult result = MessageBox.Show("Are you sure you want to exit?", "Close GUI", MessageBoxButtons.YesNo);
            if (result != DialogResult.Yes)
            {
                return;
            }

            StopServices();
        }

        private void StopServices()
        {
            _sessionTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _assignmentService.Stop();

            if (DBConnectionManager.Instance.IsConnected)
            {
                DBConnectionManager.Instance.Disconnect();
            }

            MarketDataClient.Instance.SessionChanged -= new SessionChangedEventHandler(SessionChanged);
            MarketDataClient.Instance.Stop();            
        }

        private System.Threading.Timer _initialRequestTimer;
        private bool StartServices()
        {
            DBConnectionManager.Instance.Connect();
            if (!DBConnectionManager.Instance.IsConnected)
            {
                _statusText[GUIStatus.InError] = "Couldn't connect to DB";
                return false;
            }           

            _jobLoader = new JobLoader(DBConnectionManager.Instance.Connection);
            if (!_jobLoader.Load())
            {
                _statusText[GUIStatus.InError] = "Couldn't load assignments from DB";
                return false;
            }

            MarketDataClient.Instance.Start();
            MarketDataClient.Instance.SessionChanged += new SessionChangedEventHandler(SessionChanged);
            _initialRequestTimer =
                new System.Threading.Timer(new System.Threading.TimerCallback(InitialStatusRequest), 
                    null, 1000, System.Threading.Timeout.Infinite);

            return true;
        }

        void InitialStatusRequest(object state)
        {
            _logger.Trace(LogLevel.Info, "InitialStatusRequest. Requesting MARKET STATUS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            MarketDataClient.Instance.RequestStatus(MarketDataClient.DefaultDataSource);
        }

        private void Wakeup()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateDelegate(Wakeup));
            }
            else
            {
                if (!_assignmentService.Start())
                {
                    _statusText[GUIStatus.InError] = "Wakeup. Couldn't connect to Assignment Service";
                    ChangeGUIStatus(GUIStatus.InError);
                    return;
                }

                _logger.Trace(LogLevel.Method, "Wakeup. ##############################################");

                grpControl.Enabled = true;
                grpExplorer.Enabled = true;
                ChangeGUIStatus(GUIStatus.Idle);
                controlPanel1.Initialise(_jobLoader, _assignmentService);
                jobExplorerPanel1.Initialise(_jobLoader, _assignmentService);
                _assignmentService.PausePerformed += new EventHandler(_assignmentService_Pause);
                _assignmentService.ResetPerformed += new EventHandler(_assignmentService_Reset);
                _assignmentService.ResumePerformed += new EventHandler(_assignmentService_Resume);
                _assignmentService.FinishedProcessingJob += new EventHandler(_assignmentService_FinishedProcessingJob);

                ChangeGUIStatus(GUIStatus.Idle);
            }
        }

        void SessionChanged(object sender, SessionChangedEventArgs sessionState)
        {
            _currentSessionState = sessionState;

            _logger.Trace(LogLevel.Method, "SessionChanged. sessionState {0} ##############################################", sessionState.ToString());

            if (!sessionState.ServerAlive)
            {
                _logger.Trace(LogLevel.Warning, "SessionChanged. EXCHANGE UNAVAILABLE. Trying to RECONNECT... ##############################################");
                MarketDataClient.Instance.RequestStatus(MarketDataClient.DefaultDataSource);
                return;
            }

            if (_status == GUIStatus.Connecting)
            {                    
                Wakeup();                    
            }

            if (sessionState.SessionState == SessionState.Open)
            {
                // start timer
                _logger.Trace(LogLevel.Warning, "SessionChanged. TIMER STARTED ##############################################");
                _sessionTimer.Change(500, Timeout.Infinite);                    
            }

            UpdateTimeLabels();
            UpdateExchangeStatusLabel();
        }

        private void UpdateTimeLabels()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateDelegate(UpdateTimeLabels));
            }
            else
            {
                lblTimeStart.Text = _currentSessionState.StartTime.ToString("HH:mm:ss");
                lblTimeEnd.Text = _currentSessionState.EndTime.ToString("HH:mm:ss");
            }
        }

        private void UpdateRemainingTimeLabel()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateDelegate(UpdateRemainingTimeLabel));
            }
            else
            {
                lblTimeLeft.Text = string.Format("{0:D2}:{1:D2}:{2:D2}",
               (int)_remainingTime.TotalHours % 24,
               (int)_remainingTime.TotalMinutes % 60,
               (int)_remainingTime.TotalSeconds % 60);
            }
        }

        TimeSpan _remainingTime;
        void TimerTick(object state)
        {
            _sessionTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _remainingTime = _currentSessionState.EndTime.Subtract(DateTime.Now);

            if (!_closing)
            {
                UpdateRemainingTimeLabel();
                UpdateExchangeStatusLabel();
                if (_remainingTime.CompareTo(TimeSpan.FromSeconds(1)) > 0)
                {
                    _sessionTimer.Change(500, Timeout.Infinite);
                }
            }
        }

        private void UpdateExchangeStatusLabel()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateDelegate(UpdateExchangeStatusLabel));
            }
            else
            {
                Color bg = SystemColors.Control;
                string text = null;
                if (!_currentSessionState.ServerAlive)
                {
                    text = "Exchange unavailable";
                    bg = Color.Yellow;
                }
                else if (_currentSessionState.SessionState == SessionState.Close)
                {
                    text = "Exchange closed";
                    bg = Color.Red;
                }
                else
                {
                    if (_currentSessionState.EndTime.Subtract(DateTime.Now).CompareTo(TimeSpan.FromSeconds(10)) > 0)
                    {
                        text = "Exchange open";
                        bg = Color.Green;
                    }
                    else
                    {
                        text = "Exchange closing";
                        bg = (exchangeStatusLabel.BackColor == Color.Orange) ? Color.Green : Color.Orange;
                    }
                }
                exchangeStatusLabel.Text = text;
                exchangeStatusLabel.BackColor = bg;
            }
        }

        private void ChangeGUIStatus(GUIStatus newStatus)
        {
            _status = newStatus;
            UpdateStatusLabel();
        }

        delegate void UpdateDelegate();
        private void UpdateStatusLabel()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateDelegate(UpdateStatusLabel));
            }
            else
            {
                statusLabel.Text = _statusText[_status];
            }
        }

        private void InitStatusText()
        {
            _statusText.Clear();
            _statusText[GUIStatus.Connecting] = "Connecting to services...";
            _statusText[GUIStatus.Idle] = "Idle";
            _statusText[GUIStatus.InError] = "Error";
            _statusText[GUIStatus.Paused] = "Paused";
            _statusText[GUIStatus.Playing] = "Playing";
        } 
    }
}
