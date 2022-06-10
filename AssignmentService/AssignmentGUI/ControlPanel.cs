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
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using OPEX.AS.Service;
using OPEX.Common;

namespace OPEX.AssignmentGUI
{
    public partial class ControlPanel : UserControl
    {
        private readonly Logger _logger;
        private JobLoader _jobLoader;
        private string _lastSelectedJob;
        private AssignmentService _service;

        public ControlPanel()
        {
            InitializeComponent();

            if (DesignMode)
            {
                return;
            }

            lstQueue.Items.Clear();
            lstJobs.Items.Clear();
            _logger = new Logger("ControlPanel");
            UpdateQueueLabel();
        }

        internal void Initialise(JobLoader jobLoader, AssignmentService service)
        {
            _jobLoader = jobLoader;
            _service = service;

            lstJobs.Items.Clear();
            foreach (SimulationJob job in _jobLoader.Jobs.Values)
            {
                lstJobs.Items.Add(job.JobName);
            }

            _service.FinishedProcessingJob += new EventHandler(_service_FinishedProcessingJob);
        }

        private readonly object _clearListbox = new object();
        void _service_FinishedProcessingJob(object sender, EventArgs e)
        {
            lock (_clearListbox)
            {
                try
                {
                    if (lstQueue.Items.Count > 0)
                    {
                        SimulationJob j = _service.CurrentJob;
                        string firstItemInQueue = lstQueue.Items[0].ToString();
                        if (firstItemInQueue.Equals(j.JobName))
                        {
                            DequeueJob();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Trace(LogLevel.Critical, "_service_FinishedProcessingJob. Exception: {0} {1}",
                        ex.Message, ex.StackTrace.Replace(Environment.NewLine, " ").Replace("\n", " "));
                }
            }
        }

        private void UpdateQueueLabel()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateDelegate(UpdateQueueLabel));
            }
            else
            {
                int n = lstQueue.Items.Count;
                lblJobQueue.Text = string.Format("Current Job Queue ({0} job(s))", n);
            }
        }

        delegate void UpdateDelegate();
        private void DequeueJob()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateDelegate(DequeueJob));
            }
            else
            {
                lstQueue.Items.RemoveAt(0);
                if (lstQueue.Items.Count == 0)
                {
                    btnPlay.Text = "Play";
                    btnPlay.Enabled = false;
                    btnStop.Enabled = false;
                }
                UpdateQueueLabel();
            }
        }        

        private SimulationJob _availableJobSelected;
        private void NewAvailableJobSelected()
        {
            btnEnqueue.Enabled = true;
            numericUpDown1.Enabled = true;
            _logger.Trace(LogLevel.Info, "NewAvailableJobSelected. lastSelectedJob {0}", _lastSelectedJob);
            if (_lastSelectedJob != null)
            {
                _availableJobSelected = FindJobByName(_lastSelectedJob);
                txtDescription.Text = _availableJobSelected.JobDescription;
            }
            else
            {
                txtDescription.Text = null;
            }
        }

        private void lstJobs_SelectedValueChanged(object sender, EventArgs e)
        {
            string sel = null;
            if (lstJobs.SelectedItem != null)
            {
                sel = lstJobs.SelectedItem.ToString();
            }

            if ((sel != null && sel.Equals(_lastSelectedJob)) || (sel == null && _lastSelectedJob == null))
            {
                _logger.Trace(LogLevel.Info, "lstJobs_SelectedValueChanged. selected: {0} == lastSelected {1}: NO ACTUAL CHANGE. SKIP.",
                    sel, _lastSelectedJob);
                return;
            }

            _lastSelectedJob = sel;
            NewAvailableJobSelected();
        }

        private void btnEnqueue_Click(object sender, EventArgs e)
        {
            int repeat = (int)numericUpDown1.Value;

            for (int i = 0; i < repeat; ++i)
            {
                lstQueue.Items.Add(_availableJobSelected.JobName);
            }

            if (!btnPlay.Enabled)
            {
                btnPlay.Enabled = true;
                btnPlay.Text = "Play";
            }
            btnStop.Enabled = true;            
            _service.EnqueueJob(_availableJobSelected, repeat);
            UpdateQueueLabel();
        }

        private SimulationJob FindJobByName(string jobName)
        {
            foreach (SimulationJob job in _jobLoader.Jobs.Values)
            {
                if (job.JobName.Equals(jobName))
                {
                    return job;
                }
            }
            return null;
        }

        private SimulationJob _currentJobPlaying;
        public SimulationJob CurrentJobPlaying { get { return _currentJobPlaying; } }
        private void QueueListBoxContentHasChanged()
        {
            // get the first element
            string firstJobName = lstQueue.Items[0].ToString();
            _currentJobPlaying = FindJobByName(firstJobName);
            if (_currentJobPlaying != null)
            {
                _service.EnqueueJob(_currentJobPlaying);                                
            }            
        }               

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (btnPlay.Text.Equals("Play"))
            {
                _service.Play();
                btnPlay.Text = "Pause";
            }
            else 
            {
                _service.Pause();
                btnPlay.Text = "Play";
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show("This will stop the assignment server AND ALSO clear the queue. Are you sure?", 
                "Reset", MessageBoxButtons.YesNo);
            if (res != DialogResult.Yes)
            {
                return;
            }

            lock (_clearListbox)
            {
                try
                {
                    lstQueue.Items.Clear();
                    btnPlay.Text = "Play";
                    btnPlay.Enabled = false;
                    btnStop.Enabled = false;
                    UpdateQueueLabel();
                }
                catch (Exception ex)
                {
                    _logger.Trace(LogLevel.Critical, "_service_FinishedProcessingJob. Exception: {0} {1}",
                        ex.Message, ex.StackTrace.Replace(Environment.NewLine, " ").Replace("\n", " "));
                }
            }

            _service.Reset();            
        }
    }
}
