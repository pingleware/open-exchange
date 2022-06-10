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

using OPEX.MDS.Common;

namespace OPEX.TestMDSClient
{
    public delegate void MarketDataConsumerMessageReceivedEventHandler(object sender, MarketDataMessage message);

    public class MarketDataConsumer
    {
        private readonly string QueuePath;

        private MessageQueue _queue;
        private Thread _mainThread;
        private ManualResetEvent _reset;
        private BinaryMessageFormatter _formatter;
        private bool _running;

        public MarketDataConsumer(string path)
        {
            QueuePath = path;

            try
            {
                _running = false;
                _reset = new ManualResetEvent(false);
                _formatter = new BinaryMessageFormatter();

                if (!MessageQueue.Exists(QueuePath))
                {
                    Console.WriteLine("Queue doesn't exist - creating new queue");
                    _queue = MessageQueue.Create(QueuePath, false);
                }
                else
                {
                    Console.WriteLine("Queue exist already - retrieving queue");
                    _queue = new MessageQueue(QueuePath);
                }

                _queue.ReceiveCompleted += new ReceiveCompletedEventHandler(Queue_ReceiveCompleted);
                _queue.Formatter = new BinaryMessageFormatter();
            }
            catch (MessageQueueException mex)
            {
                Console.WriteLine("MessageQueueException: " + mex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }

            _mainThread = new Thread(new ThreadStart(MainThread));
        }

        public void Start()
        {
            if (_running)
            {
                throw new ApplicationException("Already running!");
            }

            _reset.Reset();
            _mainThread.Start();
            _queue.BeginReceive();
        }

        public void Stop()
        {
            _reset.Set();
            _mainThread.Join();
        }

        public event MarketDataConsumerMessageReceivedEventHandler MessageReceived;

        private void Queue_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue q = null;
            bool error = true;

            try
            {
                q = sender as MessageQueue;

                Message m = q.EndReceive(e.AsyncResult);

                if (!_formatter.CanRead(m))
                {
                    Console.WriteLine("Formatter cannot read message!!!! Skipping this message... better luck next time...");
                }
                else
                {
                    object o = _formatter.Read(m);
                    MarketDataMessage message = o as MarketDataMessage;

                    if (MessageReceived != null)
                    {
                        MessageReceived.BeginInvoke(this, message, null, null);
                    }

                    error = false;
                }
            }
            catch (MessageQueueException mex)
            {
                Console.WriteLine("MessageQueueException: " + mex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
            finally
            {
                if (!error)
                {
                    _queue.BeginReceive();
                }
                else
                {
                    this.Stop();
                }
            }
        }

        private void MainThread()
        {
            try
            {
                _running = true;

                while (!_reset.WaitOne(0))
                {
                    Thread.Sleep(1000);
                }

                _running = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in AsyncReceiver MainLoop: " + ex.Message);
            }
        }
    }
}