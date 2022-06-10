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

namespace OPEX.AS.Service
{
    /// <summary>
    /// Represents a sequence of SimulationSteps.
    /// </summary>
    public class SimulationJob
    {
        private readonly SortedDictionary<int, SimulationStep> _steps;
        private readonly int _jobID;
        private readonly string _jobName;
        private readonly string _jobDescription;

        /// <summary>
        /// Initialises a new instance of
        /// OPEX.AS.Service.SimulationJob.
        /// </summary>
        /// <param name="jobID">The ID to assign to this SimulationJob.</param>
        /// <param name="jobName">The name of this SimulationJob.</param>
        /// <param name="jobDescription">The description of this SimulationJob.</param>
        public SimulationJob(int jobID, string jobName, string jobDescription)
        {
            _steps = new SortedDictionary<int, SimulationStep>();

            _jobID = jobID;
            _jobName = jobName;
            _jobDescription = jobDescription;
        }

        /// <summary>
        /// Gets the ID of this SimulationJob.
        /// </summary>
        public int JobID { get { return _jobID; } }


        /// <summary>
        /// Gets the name of this SimulationJob.
        /// </summary>
        public string JobName { get { return _jobName; } }


        /// <summary>
        /// Gets the description of this SimulationJob.
        /// </summary>
        public string JobDescription { get { return _jobDescription; } }


        /// <summary>
        /// Gets the steps of this SimulationJob.
        /// </summary>
        public SortedDictionary<int, SimulationStep> Steps { get { return _steps; } }

        /// <summary>
        /// Adds a SimulationStep to this SimulationJob.
        /// </summary>
        /// <param name="step">The SimulationStep to add.</param>
        public void AddStep(SimulationStep step)
        {
            if (step.JobID != _jobID)
            {
                throw new ApplicationException("SimulationJob.AddStep. Tried to add a step that doesn't belong to this job.");
            }

            _steps[step.StepID] = step;
        }

        /// <summary>
        /// Removes all the SimulationSteps from this SimulationJob.
        /// </summary>
        public void Clear()
        {
            _steps.Clear();
        }

        /// <summary>
        /// Returns the string representation of this SimulationJob.
        /// </summary>
        public override string ToString()
        {
            return string.Format("ID {0} Name {1} Steps# {3} Description {2}", _jobID, _jobName, _jobDescription, _steps.Count);
        }        
    }
}
