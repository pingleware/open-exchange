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

using OPEX.SupplyService.Common;
using OPEX.Configuration.Client;
using OPEX.AS.Service;

namespace OPEX.SalesGUI
{
    public delegate void NewAssignmentsAvailableEventHandler(object sender, bool newSimulationStarted);
    internal class AssignmentManager
    {
        private readonly string _applicationName;
        private readonly List<Assignment> _assignments;

        public AssignmentManager()
        {
            _applicationName = ConfigurationClient.Instance.GetConfigSetting("ApplicationName", null);
            _assignments = new List<Assignment>();
            AssignmentClient.Instance.NewAssignmentBatchReceived += new NewAssignmentBatchReceivedEventHandler(OnNewAssignmentReceived);                
        }

        public event NewAssignmentsAvailableEventHandler NewAssignmentsAvailable;        

        public Assignment CurrentAssignment
        {
            get
            {
                Assignment a = null;

                if (_assignments.Count > 0)
                {
                    a = _assignments[0];
                }

                return a;
            }
        }

        public Assignment[] Assignments
        {
            get
            {
                Assignment[] aa = new Assignment[_assignments.Count];
                _assignments.CopyTo(aa);
                return aa;
            }
        }

        private void Add(Assignment assignment)
        {            
            _assignments.Add(assignment);            
        }

        internal void ForceReceiveNewAssignment(Assignment assignment)
        {
            AssignmentBatch b = new AssignmentBatch(assignment.ApplicationName);
            b.Add(assignment);
            OnNewAssignmentReceived(this, b, false);
        }
                
        private void OnNewAssignmentReceived(object sender, AssignmentBatch assignmentBatch, bool newSimulationStarted)
        {
            foreach (Assignment assignment in assignmentBatch.Assignments)
            {
                Add(assignment);
            }
            if (NewAssignmentsAvailable != null)
            {
                foreach (NewAssignmentsAvailableEventHandler handler in NewAssignmentsAvailable.GetInvocationList())
                {
                    handler(this, newSimulationStarted);
                }
            }            
        }

        internal void CompleteCurrentAssignment()
        {
            if (_assignments.Count > 0)
            {
                _assignments.RemoveAt(0);
            }
        }

        internal void Flush()
        {
            _assignments.Clear();
        }
    }
}
