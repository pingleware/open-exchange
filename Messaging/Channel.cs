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
using System.Messaging;
using System.Threading;
using System.Collections.Generic;
using System.Text;

using OPEX.Common;
using OPEX.Configuration.Client;

namespace OPEX.Messaging
{
    /// <summary>
    /// Specifies the channel type
    /// </summary>
    public enum ChannelType
    {
        /// <summary>
        /// Request channel
        /// </summary>
        Request,

        /// <summary>
        /// Response channel
        /// </summary>
        Response,

        /// <summary>
        /// Broadcast channel
        /// </summary>
        Broadcast
    }    

    /// <summary>
    /// Represents a unidirectional logical communication channel,
    /// built on top of a System.Messaging.MessageQueue.
    /// </summary>
    public class Channel
    {
        protected static readonly int DefaultChannelRetryTimes = 10;
        protected static readonly int DefaultSleepAfterRetryMsec = 5000;

        protected Logger _logger;
        protected MessageQueue _queue;
        protected Thread _mainThread;
        protected ManualResetEvent _reset;
        protected int _retryTimes;
        protected int _sleepAfterRetryMsec;
        protected IMessageFormatter _formatter;
        
        protected string _queuePath;
        private string _channelName;
        protected string _queueName;
        protected int _threadSleepMsec;
        protected bool _queueOwner;

        /// <summary>
        /// Gets the name of the Channel.
        /// </summary>
        public string Name { get { return _channelName; } }

        /// <summary>
        /// Gets the MessageQueue underlying the Channel.
        /// </summary>
        public MessageQueue Queue { get { return _queue; } }

        /// <summary>
        /// Occurs when a message has been received.
        /// </summary>
        public event ReceiveCompletedEventHandler ReceiveCompleted;

        /// <summary>
        /// Initialises a new instance of the class OPEX.Messaging.Channel.
        /// </summary>
        /// <param name="channelName">The name of the Channel.</param>
        /// <param name="queuePath">The path of the MessageQueue.</param>
        /// <param name="queueName">The name of the MessageQueue.</param>
        /// <param name="queueOwner">True if this channel owns a MessageQueue.</param>
        /// <param name="threadSleepMsec">The period, in milliseconds, with which
        /// the main loop checks whether the Channel has been stopped.</param>
        public Channel(string channelName, string queuePath, string queueName, bool queueOwner, int threadSleepMsec)
        {
            _channelName = channelName;
            _logger = new Logger(string.Format("Channel({0})", _channelName));

            _formatter = new BinaryMessageFormatter();
            _queuePath = queuePath;
            _queueName = queueName;
            _threadSleepMsec = threadSleepMsec;            
            _queueOwner = queueOwner;
            _retryTimes = Int32.Parse(ConfigurationClient.Instance.GetConfigSetting("ChannelRetryTimes", DefaultChannelRetryTimes.ToString()));
            _sleepAfterRetryMsec = Int32.Parse(ConfigurationClient.Instance.GetConfigSetting("SleepAfterRetry", DefaultSleepAfterRetryMsec.ToString()));
            _reset = new ManualResetEvent(false);
            _mainThread = new Thread(new ThreadStart(MainThread));
        }

        /// <summary>
        /// Starts the Channel.
        /// </summary>
        public void Start()
        {
            Switch(true);
        }

        /// <summary>
        /// Stops the Channel.
        /// </summary>
        public void Stop()
        {
            Switch(false);
        }

        /// <summary>
        /// Initialises the Channel. Must be called before
        /// any other method.
        /// </summary>
        /// <returns></returns>
        public bool Init()
        {
            bool success = false;
            for (int i = 0; i < _retryTimes; ++i)
            {
                string message = string.Format("Initialising queue {0}.", _queuePath);
                if (i > 0)
                {
                    message += string.Format(" Retry #{0}...", i); 
                }
                _logger.Trace(LogLevel.Method, message);
                if (InitQueue())
                {
                    success = true;
                    break;
                }
                Thread.Sleep(_sleepAfterRetryMsec);
            }
            if (!success)
            {
                _logger.Trace(LogLevel.Critical, "Couldn't initialise queue {0}", _queuePath);
                return false;
            }

            _logger.Trace(LogLevel.Method, "Queue {0} initialised.", _queueName);
            return true;
        }

        protected virtual bool ListenToQueue { get { return _queueOwner; } }

        private delegate void ToggleDelegate();
        private void Switch(bool on)
        {
            bool allWell = false;
            string hi = (on) ? "Starting" : "Stopping";
            string bye = (on) ? "started" : "stopped";
            ToggleDelegate toggle;
            if (on)
            {
                toggle = InnerStart;
            }
            else
            {
                toggle = InnerStop;
            }

            try
            {
                _logger.Trace(LogLevel.Info, "{0} channel...", hi);

                toggle();

                allWell = true;
            }
            catch (MessageQueueException mex)
            {
                _logger.Trace(LogLevel.Critical, "MessageQueue Exception while {1} channel: {0}", mex.Message, hi);
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "Exception while {1} channel: {0}", ex.Message, hi);
            }
            finally
            {
                if (allWell)
                {
                    _logger.Trace(LogLevel.Debug, "Channel {0}.", bye);
                }
            }
        }

        protected virtual bool InitQueue()
        {
            bool success = false;

            try
            {
                _logger.Trace(LogLevel.Info, "QueueOwner: {0}.", _queueOwner.ToString());
                if (_queueOwner && !MessageQueue.Exists(_queuePath))
                {
                    _logger.Trace(LogLevel.Info, "{0} doesn't exist. Creating new queue: {1}", _queueName, _queuePath);
                    _queue = MessageQueue.Create(_queuePath, false);
                    _queue.SetPermissions(new MessageQueueAccessControlEntry(
                        new WellKnownTrustee(WELL_KNOWN_SID_TYPE.WinWorldSid).Trustee,
                        MessageQueueAccessRights.FullControl));
                    _queue.SetPermissions(new MessageQueueAccessControlEntry(
                        new WellKnownTrustee(WELL_KNOWN_SID_TYPE.WinAnonymousSid).Trustee,
                        MessageQueueAccessRights.FullControl));                    
                }
                else
                {
                    _logger.Trace(LogLevel.Info, "Retrieving {0} Path: {1}", _queueName, _queuePath);
                    _queue = new MessageQueue(_queuePath);
                }
                
                _queue.Formatter = new BinaryMessageFormatter();

                if (ListenToQueue)
                {
                    _logger.Trace(LogLevel.Debug, "Hooking ReceivedCompleted event to queue {0}", _queue.Path);
                    _queue.ReceiveCompleted += new ReceiveCompletedEventHandler(OnReceiveCompleted);
                }

                success = true;
            }
            catch (MessageQueueException mex)
            {
                _logger.Trace(LogLevel.Critical, "Message Queue Exception while trying to start {1}: {0}", mex.Message, _queueName);
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "Exception while trying to start {1}: {0}", ex.Message, _queueName);
            }

            return success;
        }

        private void InnerStart()
        {
            if (ListenToQueue)
            {
                _logger.Trace(LogLevel.Info, "Starting MainThread...");
                _reset.Reset();
                _mainThread.Start();
                
                _queue.BeginReceive();
                _logger.Trace(LogLevel.Info, "Called BeginReceive on queue {0}", _queue.Path);
            }
        }

        private void InnerStop()
        {
            if (ListenToQueue)
            {
                _logger.Trace(LogLevel.Info, "Stopping MainThread...");
                _queue.ReceiveCompleted -= new ReceiveCompletedEventHandler(OnReceiveCompleted);
                _reset.Set();

                _logger.Trace(LogLevel.Info, "Joining MainThread...");
                _mainThread.Join(_threadSleepMsec * 2);
                _logger.Trace(LogLevel.Info, "MainThread has joined.");
            }
        }

        protected void OnReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            if (ReceiveCompleted != null)
            {
                MessageQueue q = null;

                try
                {
                    q = sender as MessageQueue;

                    ReceiveCompleted(sender, e);
                }
                catch (MessageQueueException mex)
                {
                    _logger.Trace(LogLevel.Critical, "OnReceiveCompleted. MessageQueueException: {0} {1}", mex.Message, mex.StackTrace.Replace(Environment.NewLine, null));
                }
                catch (Exception ex)
                {
                    _logger.Trace(LogLevel.Critical, "OnReceiveCompleted. Exception: {0} {1}", ex.Message, ex.StackTrace.Replace(Environment.NewLine, null));
                }
                finally
                {
                    if (q != null)
                    {
                        q.BeginReceive();
                    }
                }
            }
        }

        private void MainThread()
        {
            try
            {
                _logger.Trace(LogLevel.Method, "MainThread started.");

                while (!_reset.WaitOne(0))
                {
                    Thread.Sleep(_threadSleepMsec);
                }
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "MainThread. Exception: {0}", ex.Message);
            }
            finally
            {
                _logger.Trace(LogLevel.Method, "MainThread stopped.");
            }
        }
    }
}
