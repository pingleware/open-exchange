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
    public class SimulationStep : IEnumerable<SimulationPhase>
    {
        private readonly SortedDictionary<int, SimulationPhase> _phases;
        private readonly int _jobID;
        private readonly int _stepID;
        private readonly int _repeat;

        public SimulationStep(int jobID, int stepID, int repeat)
        {
            _phases = new SortedDictionary<int, SimulationPhase>();
            _jobID = jobID;
            _stepID = stepID;
            _repeat = repeat;
        }

        public int JobID { get { return _jobID; } }
        public int StepID { get { return _stepID; } }
        public int Repeat { get { return _repeat; } }
        public SimulationPhase[] Phases 
        { 
            get 
            {
                SimulationPhase[] phases = new SimulationPhase[_phases.Count];

                _phases.Values.CopyTo(phases, 0);

                return phases;
            } 
        }

        public void AddPhase(int subStepID, SimulationPhase phase)
        {
            _phases[subStepID] = phase;
        }

        public void Clear()
        {
            _phases.Clear();
        }

        public override string ToString()
        {
            return string.Format("JOBID {0} StepID {1} Phases# {2} Repeat {3}", _jobID, _stepID, _phases.Count, _repeat);
        }

        #region IEnumerable<SimulationPhase> Members

        public IEnumerator<SimulationPhase> GetEnumerator()
        {
            int i = 0;
            bool more = false;

            do
            {
                foreach (SimulationPhase phase in _phases.Values)
                {
                    yield return phase;
                }
                more = (_repeat < 1) || (++i < _repeat);
            }
            while (more);
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
