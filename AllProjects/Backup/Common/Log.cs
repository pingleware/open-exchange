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
    /// Represents a Log that traces well-structured log-lines.
    /// It supports level-based filtering and formatting.
    /// This class is abstract.
    /// </summary>
    public abstract class Log : ILogWriter
    {
        /// <summary>
        /// Default LogLevel bitmask, i.e. show all traces.
        /// </summary>
        public static readonly LogLevel DefaultLevelMask = LogLevel.Debug | LogLevel.Info | LogLevel.Method | LogLevel.Warning | LogLevel.Error | LogLevel.Critical;

        protected LogLevel _levelMask;
        protected string _id;

        /// <summary>
        /// Gets the Log ID.
        /// </summary>
        public virtual string ID { get { return _id; } }

        /// <summary>
        /// Initialises a new instance of the OPEX.Common.Log class
        /// with the indicated value of levelMask.
        /// </summary>
        /// <param name="levelMask">The levelMask to use for this Log.</param>
        public Log(LogLevel levelMask)
        {
            _id = Guid.NewGuid().ToString();
            _levelMask = levelMask;
        }        

        /// <summary>
        /// Initialises a new instance of the OPEX.Common.Log class
        /// with the default value of levelMask.
        /// </summary>
        public Log() : this(DefaultLevelMask) { }

        /// <summary>
        /// Gets or sets the LevelMask of the Log.
        /// </summary>
        public LogLevel LevelMask
        {
            get { return _levelMask; }
            set { _levelMask = value; }
        }

        /// <summary>
        /// Writes a line to the Log.
        /// </summary>
        /// <param name="level">The LogLevel of the line to trace.</param>
        /// <param name="format">The content of the line (string.Format()-compliant).</param>
        /// <param name="args">The variable arguments to use to format the line.</param>
        public void Trace(LogLevel level, string format, params object[] args)
        {
            if ((level & _levelMask) != 0)
            {
                InnerTrace(level, format, args);
            }
        }

        /// <summary>
        /// Starts the Log.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Stops the Log.
        /// </summary>
        public abstract void Stop();
        
        protected abstract void InnerTrace(LogLevel level, string format, params object[] args);

        protected virtual string FormatLine(LogLevel level, string format, params object[] args)
        {            
            string message = string.Format(format, args);
            return string.Format("{0} #|# {1} #|# {2}", 
                DateTime.Now.ToString("dd/MM/yyyy@HH:mm:ss.fff"), level.ToString(), message);
        }
    }

    

    /// <summary>
    /// Specifies the level of importance of a LogTrace.
    /// </summary>
    [Flags]
    public enum LogLevel : int
    {
        /// <summary>
        /// Lowest importance informative level.
        /// </summary>
        Debug = 1,

        /// <summary>
        /// Medium importance informative level.
        /// </summary>
        Info = 2,

        /// <summary>
        /// High importance informative level.
        /// </summary>
        Method = 4,

        /// <summary>
        /// Lowest importance alarm level.
        /// </summary>
        Warning = 8,

        /// <summary>
        /// Medium importance alarm level.
        /// </summary>
        Error = 16,

        /// <summary>
        /// High importance alarm level.
        /// </summary>
        Critical = 32
    }

    /// <summary>
    /// Represents a log line.
    /// </summary>
    public class LogTrace
    {
        private LogLevel _level;
        private string _message;
        private object[] _args;

        /// <summary>
        /// Initialises a new instance of the OPEX.Common.LogTrace
        /// immutable class with the specified parameters.
        /// </summary>
        /// <param name="level">The LogLevel.</param>
        /// <param name="message">The message to trace.</param>
        /// <param name="args">Variable arguments, used to format the message.</param>
        public LogTrace(LogLevel level, string message, object[] args)
        {
            _level = level;
            _message = message;
            _args = args;
        }

        /// <summary>
        /// Gets the LogLevel.
        /// </summary>
        public LogLevel Level { get { return _level; } }

        /// <summary>
        /// Gets the Message.
        /// </summary>
        public string Message { get { return _message; } }

        /// <summary>
        /// Gets the variable arguments.
        /// </summary>
        public object[] Args { get { return _args; } }
    }
}
