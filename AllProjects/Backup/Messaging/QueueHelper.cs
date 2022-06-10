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
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPEX.Messaging
{
    /// <summary>
    /// Helper class for queue handling.
    /// </summary>
    public class QueueHelper
    {
        private static readonly string Root = @"\private$\OPEX";
        private static readonly string LocalHostName;        

        static QueueHelper()
        {
            LocalHostName = Dns.GetHostName();            
        }

        /// <summary>
        /// Gets the local host name.
        /// </summary>
        public static string LocalHost { get { return LocalHostName; } }

        /// <summary>
        /// Gets the name of a local (i.e. running on localhost) queue,
        /// formatted according to the OPEX convention.        
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="dchType">The type of the DuplexChannel.</param>
        /// <param name="type">The type of the Channel.</param>
        /// <returns>The formatted queue name.</returns>
        public static string GetQueueName(string queueName, DuplexChannelType dchType, ChannelType type)
        {
            return GetQueueName(queueName, dchType, type, "localhost");
        }

        /// <summary>
        /// Gets the name of a remote (i.e. running on a remote machine) queue,
        /// formatted according to the OPEX convention.        
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="dchType">The type of the DuplexChannel.</param>
        /// <param name="type">The type of the Channel.</param>
        /// <param name="remoteMachineName">The name of the remote machine where the queue is located.</param>
        /// <returns>The formatted queue name.</returns>
        public static string GetQueueName(string queueName, DuplexChannelType dchType, ChannelType type, string remoteMachineName)
        {
            string prefix = string.Empty;

            if (!IsLocalHost(remoteMachineName))
            {
                prefix = "FormatName:DIRECT=OS:";
            }
            else
            {
                remoteMachineName = LocalHostName;
            }

            return string.Format("{5}{0}{1}{2}_{3}_{4}", remoteMachineName, Root, queueName, dchType.ToString(), type.ToString(), prefix);
        }

        /// <summary>
        /// Checks whether a machine name is localhost.
        /// </summary>
        /// <param name="machineName">The name of the machine to check.</param>
        /// <returns>True, if the name indicated is localhost.</returns>
        private static bool IsLocalHost(string machineName)
        {
            if (machineName.Equals(".") ||
                machineName.ToLower().Equals("localhost") ||
                machineName.ToLower().Equals(LocalHostName.ToLower()))
            {
                return true;
            }

            return false;
        }
    }
}
