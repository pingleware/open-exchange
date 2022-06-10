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
using System.Text;

using OPEX.Configuration.Common;

namespace OPEX.Common
{
    /// <summary>
    /// Represents an application-wide log that writes to
    /// a log file. The file is named after ApplicationName.
    /// Log lines are filtered according to the LogLevel bitmask.
    /// A custom welcome and goodbye message are displayed when
    /// the log is Started and Stopped.
    /// LogLevel and ApplicationName are settings retrieved
    /// from the application configuration.
    /// This class is abstract.
    /// </summary>
    public abstract class MainLogger : Logger, IDisposable
    {
        private static string DefaultLogFolder = ".";

        protected Log _fileLog;
        protected string _applicationName;
        protected string[] _welcomeMessage;
        protected string[] _goodbyeMessage;

        /// <summary>
        /// Initialises a new instance of the OPEX.Common.MainLogger class.
        /// </summary>
        /// <param name="defaultApplicationName">The default name to use for this MainLogger,
        /// in case none is specified in the application configuration. The log file will be named
        /// after the application name.</param>
        /// <param name="welcomeMessage">The custom welcome message that will be traced when
        /// the MainLogger starts.</param>
        /// <param name="goodbyeMessage">The custom welcome message that will be traced when
        /// the MainLogger stops.</param>
        public MainLogger(string defaultApplicationName, string[] welcomeMessage, string[] goodbyeMessage)
            : base(defaultApplicationName)
        {
            string logFolder = ConfigurationHelper.GetConfigSetting("LogFolder", DefaultLogFolder);
            _applicationName = ConfigurationHelper.GetConfigSetting("ApplicationName", defaultApplicationName);
            bool appendTime = bool.Parse(ConfigurationHelper.GetConfigSetting("AppendTimeStamp", "false"));

            _fileLog = new LogFile(_applicationName, logFolder, appendTime);

            if (welcomeMessage != null)
            {
                _welcomeMessage = welcomeMessage;
            }
            else
            {
                _welcomeMessage = DefaultWelcomeMessage;
            }

            if (goodbyeMessage != null)
            {
                _goodbyeMessage = goodbyeMessage;
            }
            else
            {
                _goodbyeMessage = DefaultGoodbyeMessage;
            }

            StartLog();
        }

        /// <summary>
        /// Initialises a new instance of the OPEX.Common.MainLogger class.
        /// </summary>
        /// <param name="defaultApplicationName">The default name to use for this MainLogger,
        /// in case none is specified in the application configuration. The log file will be named
        /// after the application name.</param>
        public MainLogger(string defaultApplicationName)
            : this(defaultApplicationName, null, null)
        {
            string sLogLevel = ConfigurationHelper.GetConfigSetting("LogLevel", "Debug");
            LogLevel userLevel = (LogLevel) Enum.Parse(typeof(LogLevel), sLogLevel);
            LogLevel fileLogLevelMask = LogLevel.Critical;

            LogLevel[] levels = new LogLevel[] {
                LogLevel.Critical,
                LogLevel.Error,
                LogLevel.Warning,
                LogLevel.Method,                
                LogLevel.Info,
                LogLevel.Debug
            };
            foreach (LogLevel lvl in levels)
            {
                fileLogLevelMask |= lvl;
                if (userLevel == lvl)
                {
                    break;
                }
            }

            _fileLog.LevelMask = fileLogLevelMask;
        }

        /// <summary>
        /// Called after the FileLog is attached, the MainLog is
        /// started and the welcome message is printed.
        /// </summary>
        protected virtual void OnLogStarted() { }

        /// <summary>
        /// Called before the FileLog is attached, the MainLog is
        /// started and the welcome message is printed.
        /// </summary>
        protected virtual void OnStartLog() { }

        /// <summary>
        /// Called before the goodbye message is printed,
        /// the log is stopped and the filelog is detached.
        /// </summary>
        protected virtual void OnStopLog() { }

        /// <summary>
        /// Called after the goodbye message is printed,
        /// the log is stopped and the filelog is detached.
        /// </summary>
        protected virtual void OnLogStopped() { }

        private void StartLog()
        {
            OnStartLog();

            MainLog.Attach(_fileLog);
            MainLog.Start();

            foreach (string line in _welcomeMessage)
            {
                Trace(LogLevel.Info, line);
            }

            OnLogStarted();
        }

        private void Shutdown()
        {
            OnStopLog();

            foreach (string line in _goodbyeMessage)
            {
                Trace(LogLevel.Info, line);
            }
            MainLog.Stop();
            MainLog.Detach(_fileLog);

            OnLogStopped();
        }

        private string[] DefaultGoodbyeMessage
        {
            get
            {
                return new string[] {
                "#",
                "#",
                string.Format("# {0} - Goodbye", _applicationName),
                "#",
                "################################################################",
                "#",
                "#" };
            }
        }

        private string[] DefaultWelcomeMessage
        {
            get
            {
                return new string[] {
                "#",
                "#",
                "################################################################",
                "#",
                "#",
                string.Format("# {0} - Welcome", _applicationName),
                "#" };
            }
        }

        #region IDisposable Members

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Shutdown();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
