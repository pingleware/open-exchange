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

using OPEX.Common;
using OPEX.MDS.Client;
using OPEX.MDS.Common;
using OPEX.OM.Common;

namespace OPEX.Agents.Common
{   
    public class ShoutStimulusQueue : StimulusQueue
    {
        private readonly object _root = new object();        
        private string _instrument = null;
        private InstrumentWatcher _watcher = null;

        public ShoutStimulusQueue(string queueName)
            : base(queueName, StimulusType.Shout)        
        {             
        }

        public void ListenToInstrument(string instrument)
        {
            lock (_root)
            {
                if (instrument == null || instrument.Length == 0)
                {
                    _logger.Trace(LogLevel.Warning, "ShoutStimulusQueue.ListenToInstrument. Can't listen to null or empty instrument. Ignoring request.");
                    return;
                }

                if (instrument.Equals(_instrument))
                {
                    _logger.Trace(LogLevel.Info, "ShoutStimulusQueue.ListenToInstrument. Already listening to instrument {0}. Ignoring request.", _instrument);
                    return;
                }
                if (_instrument != null)
                {
                    HookMarketDataChanged(_instrument, false);
                }
                                                
                Flush();
                _instrument = instrument;
                HookMarketDataChanged(_instrument, true);
            }
        }       

        public override void StartReceiving()
        {
            _logger.Trace(LogLevel.Debug, "ShoutStimulusQueue.StartReceiving");
            lock (_root)
            {
                if (_instrument != null)
                {
                    HookMarketDataChanged(_instrument, true);   
                }
            }
        }

        public override void StopReceiving()
        {
            _logger.Trace(LogLevel.Debug, "ShoutStimulusQueue.StopReceiving");
            lock (_root)
            {
                if (_instrument != null)
                {
                    HookMarketDataChanged(_instrument, false);                                        
                }
            }            
        }

        private void MarketDataChanged(object sender, MarketDataEventArgs e)
        {
            switch (e.Type)
            {
                case MarketDataEventType.DepthChangedWithNewShout:
                case MarketDataEventType.DepthChangedWithNewTrade:
                    Enqueue(new ShoutStimulus(_watcher.LastShout, _watcher.LastSnapshot, _watcher.LastTrade));
                    break;
                default:
                    break;
            }
        } 

        private void HookMarketDataChanged(string instrument, bool hook)
        {            
            if (hook)
            {
                _logger.Trace(LogLevel.Debug, "ShoutStimulusQueue.HookMarketDataChanged. Hooking MarketDataChanged for instrument {0}", instrument);
                _watcher = MarketDataClient.Instance.CreateInstrumentWatcher(instrument);
                _watcher.MarketDataChanged += new EventHandler<MarketDataEventArgs>(MarketDataChanged);
            }
            else
            {
                _logger.Trace(LogLevel.Debug, "ShoutStimulusQueue.HookMarketDataChanged. Unhooking MarketDataChanged for instrument {0}", instrument);
                _watcher.MarketDataChanged -= new EventHandler<MarketDataEventArgs>(MarketDataChanged);
                _watcher.Dispose();
            }     
        }               
    }
}
