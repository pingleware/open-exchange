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

namespace OPEX.Common
{
    /// <summary>
    /// Writes to LogService, adding its own customisable
    /// title to each trace. 
    /// Traces are written to LogService, whether it's started or not,
    /// and whether there are logs attached or not.
    /// </summary>
    public class Logger : ILogWriter
    {
        protected static readonly LogService MainLog;

        private string _logTitle;

        static Logger()
        {
            MainLog = LogService.MainInstance;
        }

        /// <summary>
        /// Initialises a new instance of the class OPEX.Common.Logger.
        /// </summary>
        /// <param name="logTitle">The title of the Logger.</param>
        public Logger(string logTitle)
        {
            _logTitle = logTitle;
        }

        /// <summary>
        /// Gets and sets the title of the Logger.
        /// </summary>
        public string LogTitle
        {
            get { return _logTitle; }
            set { _logTitle = value; }
        }

        /// <summary>
        /// Writes a line to the MainLog.
        /// </summary>
        /// <param name="level">The LogLevel of the line to trace.</param>
        /// <param name="format">The content of the line (string.Format()-compliant).</param>
        /// <param name="args">The variable arguments to use to format the line.</param>
        public virtual void Trace(LogLevel level, string format, params object[] args)
        {
            MainLog.Trace(level, string.Format("{0} #|# {1}", _logTitle, format), args);
        }

        /// <summary>
        /// Writes a LogLeve.Critical line to the MainLog and throws
        /// an ApplicationException.
        /// </summary>
        /// <param name="format">The content of the line (string.Format()-compliant).</param>
        /// <param name="args">The variable arguments to use to format the line.</param>
        public virtual void TraceAndThrow(string format, params object[] args)
        {
            Trace(LogLevel.Critical, format, args);
            throw new ApplicationException(string.Format(format, args));
        }  
    }
}
