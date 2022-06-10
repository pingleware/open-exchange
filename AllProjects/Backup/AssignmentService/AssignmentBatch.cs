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
    /// Represents a list of Assignments.
    /// </summary>
    [Serializable]
    public class AssignmentBatch
    {
        private readonly List<Assignment> _assignmentList;
        private readonly string _applicationName;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.AS.Service.AssignmentBatch.
        /// </summary>
        /// <param name="applicationName">The name of the
        /// application that owns the Assignments.</param>
        public AssignmentBatch(string applicationName) 
        {
            _applicationName = applicationName;
            _assignmentList = new List<Assignment>();
        }

        /// <summary>
        /// Gets the name of the application that owns
        /// the Assignments.
        /// </summary>
        public string ApplicationName { get { return _applicationName; } }

        /// <summary>
        /// Gets the list of Assignments.
        /// </summary>
        public List<Assignment> Assignments { get { return _assignmentList; } }

        /// <summary>
        /// Adds an Assignment.
        /// </summary>
        /// <param name="assignment">The assignment to add.</param>
        public void Add(Assignment assignment)
        {
            if (!assignment.ApplicationName.Equals(_applicationName))
            {
                throw new ArgumentException(string.Format(
                    "The assignment's ApplicationName {0} doesn't match theBatch's one {1}.",
                    assignment.ApplicationName, _applicationName));
            }
            _assignmentList.Add(assignment);
        }

        /// <summary>
        /// Removes all the Assignments.
        /// </summary>
        public void Clear()
        {
            _assignmentList.Clear();
        }

        /// <summary>
        /// Returns the string representation of this
        /// AssignmentBatch.
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("ApplicationName: {0}. #Assignments: {1}", _applicationName, _assignmentList.Count);

            foreach (Assignment a in _assignmentList)
            {
                sb.AppendFormat(" {0};", a.ToString());
            }

            return sb.ToString();
        }
    }
}
