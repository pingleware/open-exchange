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
using System.Threading;

using OPEX.Common;
using OPEX.OM.Common;

namespace OPEX.Agents.Common
{
    public class SynchronousOrderSender
    {
        private readonly Logger _logger;
        private readonly OutgoingOrder _order;
        private readonly Semaphore _send;        
        private readonly Semaphore _amend;
        private readonly Semaphore _cancel;
        private readonly AutoResetEvent _sent;
        private readonly AutoResetEvent _amended;
        private readonly AutoResetEvent _cancelled;

        public SynchronousOrderSender(OutgoingOrder order)
        {
            _order = order;
            _logger = new Logger(string.Format("SynchronousOrderSender({0})", order.ClientOrderID));
            _send = new Semaphore(1, 1);
            _sent = new AutoResetEvent(false);
            _amend = new Semaphore(1, 1);
            _amended = new AutoResetEvent(false);
            _cancel = new Semaphore(1, 1);
            _cancelled = new AutoResetEvent(false);
        }

        public OutgoingOrder Order { get { return _order; } }

        public bool SendSynch(int maxWaitMsec, ref string errorMessage)
        {
            bool res = false;
            string error = null;

            _send.WaitOne();           
            _order.StatusChanged += new OutgoingOrderEventHandler(StatusChanged);
            res = _order.SafeSend(ref error);
            if (res)
            {
                _sent.Reset();
                if (!(res = _sent.WaitOne(maxWaitMsec)))
                {
                    error = "Timeout";
                }
            }
            else 
            {
                _order.StatusChanged -= new OutgoingOrderEventHandler(StatusChanged);
            }
        
            if (!res)
            {
                errorMessage = error;
            }
            _send.Release();
            return res;
        }

        public bool AmendSynch(double price, int maxWaitMsec, ref string errorMessage)
        {
            return AmendSynch(price, _order.Quantity, maxWaitMsec, ref errorMessage);
        }

        public bool AmendSynch(double price, int newQty, int maxWaitMsec, ref string errorMessage)
        {
            bool res = false;
            string error = null;

            _amend.WaitOne();
            _order.StatusChanged += new OutgoingOrderEventHandler(StatusChanged);
            res = _order.SafeAmend(newQty, price, ref error);
            if (res)
            {
                _amended.Reset();
                if (!(res = _amended.WaitOne(maxWaitMsec)))
                {
                    error = "Timeout";
                }
            }
            else
            {
                _order.StatusChanged -= new OutgoingOrderEventHandler(StatusChanged);
            }

            if (!res)
            {
                errorMessage = error;
            }
            _amend.Release();
            return res;
        }

        public bool CancelSynch(int maxWaitMsec, ref string errorMessage)
        {
            bool res = false;
            string error = null;

            _cancel.WaitOne();
            _order.StatusChanged += new OutgoingOrderEventHandler(StatusChanged);
            res = _order.SafeCancel(ref error);
            if (res)
            {
                _cancelled.Reset();
                if (!(res = _cancelled.WaitOne(maxWaitMsec)))
                {
                    error = "Timeout";
                }
            }
            else
            {
                _order.StatusChanged -= new OutgoingOrderEventHandler(StatusChanged);
            }

            if (!res)
            {
                errorMessage = error;
            }
            _cancel.Release();
            return res;
        }

        void StatusChanged(object sender, Order order)
        {
            _order.StatusChanged -= new OutgoingOrderEventHandler(StatusChanged);
            _sent.Set();
            _amended.Set();
            _cancelled.Set();
        }
    }
}
