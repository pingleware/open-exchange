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
using System.Linq;
using System.Text;
using System.Threading;

using OPEX.Common;

namespace OPEX.Messaging
{
    /// <summary>
    /// Ping function call.
    /// </summary>
    /// <param name="sender">The sender of the "ping" message.</param>
    /// <param name="pingMessage">The content of the "ping" message.</param>
    public delegate void KeepAliveEventHandler(object sender, object pingMessage);

    /// <summary>
    /// Generates a "ping" message periodically.
    /// </summary>
    public class KeepAliveMessageGenerator
    {
        private readonly Thread _mainThread;
        private readonly ManualResetEvent _reset;
        private readonly int _pingTimeMsec;
        private readonly object _pingMessage;
        private readonly Logger _logger;
        private bool _started;

        /// <summary>
        /// Initialises a new instance of the class OPEX.Messaging.KeepAliveMessageGenerator.
        /// </summary>
        /// <param name="pingTimeMsec">The period of the "ping" message, in milliseconds.</param>
        /// <param name="pingMessage">The content of the "ping" message.</param>
        public KeepAliveMessageGenerator(int pingTimeMsec, object pingMessage)
        {
            _pingMessage = pingMessage;
            _pingTimeMsec = pingTimeMsec;
            _mainThread = new Thread(new ThreadStart(MainThread));
            _reset = new ManualResetEvent(false);
            _logger = new Logger("KeepAliveMessageGenerator");
        }

        /// <summary>
        /// Occurs periodically, with the period specified in the constructor.
        /// </summary>
        public event KeepAliveEventHandler Ping;

        /// <summary>
        /// Starts the KeepAliveMessageGenerator.
        /// </summary>
        public void Start()
        {
            if (_started)
            {
                throw new ApplicationException("Can't start KeepAliveMessageGenerator because it has already started");
            }

            _reset.Reset();
            _mainThread.Start();

            _started = true;

            _logger.Trace(LogLevel.Method, "KeepAliveMessageGenerator STARTED");
        }

        /// <summary>
        /// Stops the KeepAliveMessageGenerator.
        /// </summary>
        public void Stop()
        {
            if (!_started)
            {
                throw new ApplicationException("Can't start KeepAliveMessageGenerator because it has not started yet");
            }

            _reset.Set();
            _mainThread.Join(3000);

            _started = false;

            _logger.Trace(LogLevel.Method, "KeepAliveMessageGenerator STOPPED");
        }

        private void MainThread()
        {
            while (!_reset.WaitOne(_pingTimeMsec))
            {
                if (Ping != null)
                {
                    Ping(this, _pingMessage);
                }
            }
        }
    }
}
