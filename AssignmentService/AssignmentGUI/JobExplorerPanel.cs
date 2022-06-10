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
using OPEX.SupplyService.Common;
using OPEX.OM.Common;

namespace OPEX.AssignmentGUI
{
    public partial class JobExplorerPanel : UserControl
    {
        private JobLoader _jobLoader;
        private AssignmentService _service;
        private bool _loaded;

        public JobExplorerPanel()
        {
            InitializeComponent();
        }

        internal void Initialise(JobLoader jobLoader, AssignmentService service)
        {
            _jobLoader = jobLoader;
            _service = service;

            _service.NewPhaseBroadcasted += new EventHandler(_service_NewPhaseBroadcasted);
            _service.ResetPerformed += new EventHandler(_service_ResetPerformed);

            treeView1.Nodes.Clear();

            foreach (SimulationJob job in jobLoader.Jobs.Values)
            {                
                TreeNode thisJobNode = new TreeNode();
                thisJobNode.Text = GetJobNodeText(job);
                thisJobNode.Tag = job.JobName;
                
                foreach (SimulationStep step in job.Steps.Values)
                {
                    TreeNode thisStepNode = new TreeNode();
                    thisStepNode.Text = GetStepNodeText(step);
                    thisStepNode.Tag = step.StepID;
                    int subID = 0;
                    foreach (SimulationPhase phase in step.Phases)
                    {
                        TreeNode thisPhaseNode = new TreeNode();
                        thisPhaseNode.Text = GetPhaseNodeText(phase);
                        thisPhaseNode.Tag = subID;
                        thisStepNode.Nodes.Add(thisPhaseNode);
                        subID++;
                    }
                    thisJobNode.Nodes.Add(thisStepNode);
                }

                treeView1.Nodes.Add(thisJobNode);
                thisJobNode.Collapse();
            }

            treeView1.ExpandAll();
            _loaded = true;
        }       

        void _service_ResetPerformed(object sender, EventArgs e)
        {
            UpdateControls();
        }

        void _service_NewPhaseBroadcasted(object sender, EventArgs e)
        {
            UpdateControls();
        }

        delegate void UpdateDelegate();
        private void UpdateControls()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateDelegate(UpdateControls));
            }
            else
            {
                SimulationPhase ph = _service.CurrentPhase;
                string phaseText = "Current Phase:";
                int a = 0;
                int subID = 0;

                if (ph != null)
                {
                    a = ph.Assignments.Count;
                    subID = _service.CurrentSubStepID;
                    phaseText += string.Format(" [{0}].{1}.{2} (Phase ID {3})",
                        _service.CurrentJob.JobName,
                        _service.CurrentStep.StepID,
                        subID,
                        ph.PhaseID);

                    opexDataSet.Clear();
                    foreach (Assignment assignment in ph.Assignments)
                    {
                        opexDataSet.SimulationPhases.AddSimulationPhasesRow(
                            (uint)assignment.Phase, (uint)assignment.Id, assignment.ApplicationName, assignment.Ric,
                            assignment.Currency, assignment.Side.ToString(), (uint)assignment.Quantity, assignment.Price);
                    }

                    string path = string.Format(@"{0}\\{1}\\{2}",
                        GetJobNodeText(_service.CurrentJob),
                        GetStepNodeText(_service.CurrentStep),
                        GetPhaseNodeText(ph));
                    TreeNode node = null;
                    foreach (TreeNode jobNode in treeView1.Nodes)
                    {
                        foreach (TreeNode stepNode in jobNode.Nodes)
                        {
                            foreach (TreeNode phaseNode in stepNode.Nodes)
                            {
                                if (phaseNode.FullPath.Equals(path))
                                {
                                    node = phaseNode;
                                    break;
                                }
                            }
                            if (node != null)
                            {
                                break;
                            }
                        }
                        if (node != null)
                        {
                            break;
                        }
                    }
                    if (node != null)
                    {
                        node.EnsureVisible();
                        treeView1.SelectedNode = node;
                        treeView1.Focus();
                    }
                }
                else
                {
                    opexDataSet.SimulationPhases.Clear();
                    treeView1.SelectedNode = null;
                }

                lblCurrentPhase.Text = phaseText;
                lblAssignments.Text = string.Format("{0} Assignment(s)", a);
            }
        }

        private void treeView1_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            if (_loaded)
            {
                e.Cancel = true;
            }
        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (_loaded)
            {
                e.Cancel = true;
            }
        }

        private void treeView1_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node == null)
            {
                return;
            }

            if (e.Node.Level != 2)
            {
                e.Cancel = true;
            }          
        }

        private void OnNewPhaseSelected()
        {
            TreeNode node = treeView1.SelectedNode;
            TreeNode stepNode = node.Parent;
            TreeNode jobNode = stepNode.Parent;
            int subID = (int)node.Tag;
            int stepID = (int)stepNode.Tag;

            SimulationJob job = null;
            foreach (SimulationJob j in _jobLoader.Jobs.Values)
            {
                if (!j.JobName.Equals(jobNode.Tag.ToString()))
                {
                    continue;
                }
                job = j;
                break;
            }

            if (job == null)
            {
                return;
            }

            SimulationStep step = job.Steps[stepID];
            SimulationPhase ph = step.Phases[subID];
            if (ph == null)
            {
                return;
            }
            int a = ph.Assignments.Count;
            string phaseText = string.Format("Current Phase: [{0}].{1}.{2} (Phase ID {3})",
                job.JobName,
                stepID,
                subID,
                ph.PhaseID);

            opexDataSet.Clear();
            foreach (Assignment assignment in ph.Assignments)
            {
                opexDataSet.SimulationPhases.AddSimulationPhasesRow(
                    (uint)assignment.Phase, (uint)assignment.Id, assignment.ApplicationName, assignment.Ric,
                    assignment.Currency, assignment.Side.ToString(), (uint)assignment.Quantity, assignment.Price);
            }
        }

        string GetJobNodeText(SimulationJob job)
        {
            return string.Format("[{0}]", job.JobName);
        }

        string GetStepNodeText(SimulationStep step)
        {
            string repeatString = (step.Repeat>0) ? 
                string.Format("[{0}x]", step.Repeat) :
                "[LOOP]";
            return string.Format("Step #{0} {1}", step.StepID, repeatString);
        }

        string GetPhaseNodeText(SimulationPhase phase)
        {
            return string.Format("Phase {0}", phase.PhaseID);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            OnNewPhaseSelected();
        }
    }
}
