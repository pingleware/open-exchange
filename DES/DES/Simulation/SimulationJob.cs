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

namespace OPEX.DES.Simulation
{
    public class SimulationJob : IEnumerable<SimulationPhase>
    {
        private readonly SortedDictionary<int, SimulationStep> _steps;
        private readonly int _jobID;
        private readonly string _jobName;
        private readonly string _jobDescription;
        private readonly int _repeat;

        public SimulationJob(int jobID, string jobName, string jobDescription, int repeat)
        {
            _steps = new SortedDictionary<int, SimulationStep>();

            _jobID = jobID;
            _jobName = jobName;
            _jobDescription = jobDescription;
            _repeat = repeat;
        }

        public int JobID { get { return _jobID; } }
        public int Repeat { get { return _repeat; } }
        public string JobName { get { return _jobName; } }
        public string JobDescription { get { return _jobDescription; } }
        public SortedDictionary<int, SimulationStep> Steps { get { return _steps; } }

        public void AddStep(SimulationStep step)
        {
            if (step.JobID != _jobID)
            {
                throw new ApplicationException("SimulationJob.AddStep. Tried to add a step that doesn't belong to this job.");
            }

            _steps[step.StepID] = step;
        }

        public void Clear()
        {
            _steps.Clear();
        }

        public override string ToString()
        {
            return string.Format("ID {0} Name {1} Steps# {3} Description {2}", _jobID, _jobName, _jobDescription, _steps.Count);
        }

        #region IEnumerable<SimulationPhase> Members

        public IEnumerator<SimulationPhase> GetEnumerator()
        {
            foreach (SimulationStep step in _steps.Values)
            {
                foreach (SimulationPhase phase in step)
                {
                    yield return phase;
                }
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
