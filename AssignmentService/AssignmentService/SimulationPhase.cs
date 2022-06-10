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

using OPEX.SupplyService.Common;

namespace OPEX.AS.Service
{
    /// <summary>
    /// Represents the smaller element of a simulation.
    /// </summary>
    public class SimulationPhase
    {
        private readonly int _phaseID;
        private readonly List<Assignment> _assignments;

        /// <summary>
        /// Initialises a new instance of the class 
        /// OPEX.AS.Service.SimulationPhase.
        /// </summary>
        /// <param name="phaseID">The ID of the SimulationPhase to add.</param>
        public SimulationPhase(int phaseID)
        {
            _phaseID = phaseID;
            _assignments = new List<Assignment>();
        }

        /// <summary>
        /// Gets the ID of the SimulationPhase to add.
        /// </summary>
        public int PhaseID { get { return _phaseID; } }

        /// <summary>
        /// Gets the Assignments in this SimulationPhase.
        /// </summary>
        public ICollection<Assignment> Assignments { get { return _assignments; } }

        /// <summary>
        /// Adds an Assignment to this SimulationPhase.
        /// </summary>
        /// <param name="assignment"></param>
        public void AddAssignment(Assignment assignment)
        {
            if (assignment.Phase != _phaseID)
            {
                throw new ApplicationException("SimulationPhase.AddAssignment. Tried to add an assignment with a different phase.");
            }

            foreach (Assignment a in _assignments)
            {
                if (a.ApplicationName.Equals(assignment.ApplicationName) && a.Id == assignment.Id)
                {
                    throw new ApplicationException("SimulationPhase.AddAssignment. Duplicate key.");                    
                }
            }

            _assignments.Add(assignment);
            _assignments.Sort(new AssignmentComparer());            
        }

        /// <summary>
        /// Removes all the Assignments from this SimulationPhase.
        /// </summary>
        public void Clear()
        {
            _assignments.Clear();
        }

        private class AssignmentComparer : IComparer<Assignment>     
        {
            #region IComparer<Assignment> Members

            public int Compare(Assignment x, Assignment y)
            {
                if (x == null && y == null)
                {
                    return 0;
                }
                else if (x == null)
                {
                    return -1;
                }
                else if (y == null)
                {
                    return 1;
                }

                int p = (x.Phase.CompareTo(y.Phase));
                if (p != 0)
                {
                    return p;
                }
                int n = (x.ApplicationName.CompareTo(y.ApplicationName));
                if (n != 0)
                {
                    return n;
                }
                return x.Id.CompareTo(y.Id);
            }

            #endregion
        }
    }
}
