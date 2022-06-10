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
using OPEX.OM.Common;
using OPEX.MDS.Common;
using OPEX.MDS.Client;
using OPEX.TDS.Client;
using OPEX.TDS.Common;
using OPEX.Configuration.Client;
using OPEX.SupplyService.Common;
using OPEX.AS.Service;

namespace OPEX.SalesGUI
{
    internal class Services
    {
        #region Statics

        private static Services _theInstance;
        private static readonly object _root = new object();

        public static Services Instance
        {
            get
            {
                lock (_root)
                {
                    if (_theInstance == null)
                    {
                        _theInstance = new Services();
                    }
                }
                return _theInstance;
            }
        }

        #endregion Statics

        private OutgoingOrderDuplexChannel _outChannel;

        private Services()
        {         
        }
        
        public event TradeMessageReceivedEventHandler TradeMessageReceived;        

        public OutgoingOrderDuplexChannel OutChannel { get { return _outChannel; } }

        public void Start()
        {
            _outChannel = new OutgoingOrderDuplexChannel(OrderFactory.OMClientName);
            bool purge = bool.Parse(ConfigurationClient.Instance.GetConfigSetting("PurgeQueuesOnStartup", "true"));
            if (purge)
            {
                _outChannel.Purge();
            }
            OrderFactory.OrderSender = _outChannel;
            _outChannel.Start();

            MarketDataClient.Instance.Start();

            TradeDataClient.Instance.TradeMessageReceived += new TradeMessageReceivedEventHandler(OnTradeMessageReceived);
            TradeDataClient.Instance.Start();
            AssignmentClient.Instance.Start();            
        }       

        public void Stop()
        {
            _outChannel.Stop();
            MarketDataClient.Instance.Stop();

            TradeDataClient.Instance.Stop();
            TradeDataClient.Instance.TradeMessageReceived -= new TradeMessageReceivedEventHandler(OnTradeMessageReceived);
            AssignmentClient.Instance.Stop();
        }      

        void OnTradeMessageReceived(object sender, TradeDataMessage tradeDataMessage)
        {
            if (TradeMessageReceived == null)
            {
                return;
            }

            foreach (TradeMessageReceivedEventHandler handler in TradeMessageReceived.GetInvocationList())
            {
                handler(sender, tradeDataMessage);
            }
        }
    }
}
