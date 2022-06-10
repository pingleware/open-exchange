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
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using OPEX.Common;
using OPEX.OM.Common;

namespace OPEX.ME
{
    /// <summary>
    /// Processes OrderRequest-s.
    /// </summary>
    public class OrderProcessor
    {
        private readonly Logger _logger;        
        private readonly OrderBook _orderBook;
        private readonly Mutex _mutex;

        private bool _running;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.ME.OrderProcessor.
        /// </summary>
        /// <param name="orderBook">The OrderBook whose orders to process.</param>
        public OrderProcessor(OrderBook orderBook)
        {
            _logger = new Logger(string.Format("OrderProcessor({0})", orderBook.InstrumentName));

            _logger.Trace(LogLevel.Debug, "Creating new OrderProcessor.");

            if (orderBook == null)
            {
                _logger.TraceAndThrow("Null orderbook!");
            }

            _orderBook = orderBook;

            if (orderBook.InstrumentName == null || orderBook.InstrumentName.Length == 0)
            {
                _logger.TraceAndThrow("Null InstrumentName!");
            }

            _mutex = new Mutex(false);
            _running = false;

            _logger.Trace(LogLevel.Debug, "New OrderProcessor created.");
        }

        /// <summary>
        /// Starts the OrderProcessor.
        /// </summary>
        public void Start()
        {
            _logger.Trace(LogLevel.Debug, "Starting OrderProcessor.");

            if (_running)
            {
                _logger.TraceAndThrow("Can't start OrderProcessor, as it's already running!");
            }

            _running = true;

            _logger.Trace(LogLevel.Debug, "OrderProcessor Started.");
        }

        /// <summary>
        /// Stops the OrderProcessor, also stopping
        /// the associated OrderBook.
        /// </summary>
        public void Stop()
        {
            _logger.Trace(LogLevel.Debug, "Stopping OrderProcessor.");

            if (!_running)
            {
                _logger.TraceAndThrow("Can't stop OrderProcessor, as it's not running!");
            }

            _mutex.WaitOne();
            if (_orderBook != null)
            {
                _orderBook.Stop();
            }
            _mutex.ReleaseMutex();

            _running = false;

            _logger.Trace(LogLevel.Debug, "OrderProcessor stopped.");
        }

        /// <summary>
        /// Processes OrderRequest-s synchronously.
        /// </summary>
        /// <param name="order">The OrderRequest to process.</param>
        public void Process(OrderRequest request)
        {
            _mutex.WaitOne();
            try
            {
                _orderBook.Process(request);
            }
            catch (Exception ex)
            {
                _logger.Trace(LogLevel.Critical, "OrderProcessor.Process. An exception occurred while processing an order: {0} {1}", ex.Message, ex.StackTrace.Replace("\n", " ").Replace(Environment.NewLine, " "));
            }
            _mutex.ReleaseMutex();
        }
    }
}
