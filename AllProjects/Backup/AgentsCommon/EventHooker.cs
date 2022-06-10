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

using OPEX.Common;
using OPEX.OM.Common;

namespace OPEX.Agents.Common
{
    public delegate void EventHookerEventHandler(OutgoingOrder order, bool hook);

    public class EventHooker
    {
        private readonly object _root;
        private Dictionary<long, bool> _eventHookedToOrder;
        private EventHookerEventHandler _hookHandler;
        private Logger _logger;
        private string _name;

        public EventHooker(string name, EventHookerEventHandler hookHandler)
        {
            _root = new object();
            _eventHookedToOrder = new Dictionary<long, bool>();
            _hookHandler = hookHandler;
            _name = name;
            _logger = new Logger(string.Format("EventHooker({0})", _name));
        }

        public void Hook(OutgoingOrder order)
        {
            lock (_root)
            {
                long id = order.ClientOrderID;
                if (_eventHookedToOrder.ContainsKey(id) && _eventHookedToOrder[id])
                {
                    _logger.Trace(LogLevel.Warning, "Event is already hooked. Skipping.");
                    return;
                }
                InvokeHandler(order, true);
                _eventHookedToOrder[id] = true;
            }
        }

        public void Unhook(OutgoingOrder order)
        {
            lock (_root)
            {
                long id = order.ClientOrderID;
                if (!_eventHookedToOrder.ContainsKey(id) || !_eventHookedToOrder[id])
                {
                    _logger.Trace(LogLevel.Warning, "Event is not hooked. Skipping.");
                    return;
                }
                InvokeHandler(order, false);
                _eventHookedToOrder[id] = false;
            }
        }

        private void InvokeHandler(OutgoingOrder order, bool hook)
        {
            try
            {
                long id = order.ClientOrderID;
                _hookHandler(order, hook);
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "Exception while invoking handler: {0} {1}", ex.Message, ex.StackTrace);
            }
        }
    }
}
