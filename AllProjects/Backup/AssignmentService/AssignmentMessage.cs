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

using OPEX.Messaging;
using OPEX.SupplyService.Common;

namespace OPEX.AS.Service
{
    /// <summary>
    /// Contains an AssignmentBatch, its expiration date
    /// and indicates whether it is the first one of a 
    /// simulation.
    /// </summary>
    [Serializable]
    public class AssignmentMessage : IChannelMessage
    {
        /// <summary>
        /// Gets the default multicast address of the AssignmentService
        /// for IBM-mode simulations, i.e. finite trading periods.
        /// </summary>
        public static string DefaultMulticastAddress { get { return "234.222.23.21:3030"; } }

        /// <summary>
        /// Gets the default multicast address of the AssignmentService
        /// for DWE-mode simulations, i.e. continuous market.
        /// </summary>
        public static string DWEMulticastAddress { get { return "234.222.23.25:6060"; } }

        private readonly AssignmentBatch _assignmentBatch;
        private readonly DateTime _expiry;
        private readonly bool _newSimulationStarted;

        internal AssignmentMessage(AssignmentBatch assignmentBatch, DateTime expiry, bool newSimulationStarted)
        {
            _assignmentBatch = assignmentBatch;
            _expiry = expiry;
            _newSimulationStarted = newSimulationStarted;
        }

        /// <summary>
        /// Gets the AssignmentBatch.
        /// </summary>
        public AssignmentBatch AssignmentBatch { get { return _assignmentBatch; } }

        /// <summary>
        /// Gets the expiration time of this message.
        /// </summary>
        public DateTime Expiry { get { return _expiry; } }

        /// <summary>
        /// Indicates whether this is the first message sent after a new
        /// simulation started.
        /// </summary>
        public bool NewSimulationStarted { get { return _newSimulationStarted; } }

        /// <summary>
        /// Returns the string representation of this AssignmentMessage.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Expiry {0} NewSimulationStarted {2} {1}", _expiry.ToString("HH:mm:ss"), _assignmentBatch.ToString(), _newSimulationStarted);
        }

        #region IChannelMessage Members

        public string Origin
        {
            get { throw new NotImplementedException(); }
        }

        public string Destination
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
