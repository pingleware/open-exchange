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

namespace OPEX.SalesGUI
{
    class BlinkingScheduler
    {
        class BlinkingItem
        {
            private Blinking _b;
            private string _m;

            public BlinkingItem(Blinking b, string message)
            {
                _b = b;
                _m = message;
            }

            public Blinking Blinking { get { return _b; } }
            public string Message { get { return _m; } }
        }

        private readonly Queue<BlinkingItem> _queue;
        private readonly ManualResetEvent _mainThreadReset;
        private readonly AutoResetEvent _newItemEvent;
        private readonly Thread _mainThread;
        private readonly AutoResetEvent _finished;
        private readonly object _root = new object();
        private bool _enabled = false;
        private DateTime _timeLastBlinkingEnqueued = DateTime.MinValue;

        public BlinkingScheduler()
        {
            _queue = new Queue<BlinkingItem>();

            _mainThread = new Thread(new ThreadStart(MainThread));
            _mainThreadReset = new ManualResetEvent(false);
            _newItemEvent = new AutoResetEvent(false);
            _finished = new AutoResetEvent(false);
        }

        public bool Enabled { get { return _enabled; } set { _enabled = value; } }

        public void Start()
        {
            _mainThreadReset.Reset();
            _mainThread.Start();
        }

        public void Stop()
        {
            _mainThreadReset.Set();            
            _mainThread.Join(7000);
        }

        private int _lastID = -1;
        public void Schedule(Blinking blinking, int ID, string message)
        {
            if (_enabled)
            {
                bool enqueue = true;
                if (enqueue)
                {
                    _lastID = ID;
                    _timeLastBlinkingEnqueued = DateTime.Now;
                    _queue.Enqueue(new BlinkingItem(blinking, message));
                    _newItemEvent.Set();
                }
            }
        }

        public void Flush()
        {
            lock (_root)
            {
                _queue.Clear();
            }
        }
        
        private void MainThread()
        {
             WaitHandle[] EventArray = new WaitHandle[] {
                    _mainThreadReset,
                    _newItemEvent
                };

             while (WaitHandle.WaitAny(EventArray) != 0)
             {
                 BlinkingItem item = null;

                 lock (_root)
                 {
                     if (_queue.Count > 0)
                     {
                         item = _queue.Dequeue();
                     }
                 }

                 if (item != null && _enabled)
                 {
                     item.Blinking.Finished += new EventHandler(Blinking_Finished);
                     item.Blinking.Start(item.Message);
                     _finished.WaitOne();
                 }
             }
        }

        void Blinking_Finished(object sender, EventArgs e)
        {
            Blinking b = sender as Blinking;
            b.Finished -= new EventHandler(Blinking_Finished);
            _finished.Set();
            _newItemEvent.Set();
        }
    }    
}
