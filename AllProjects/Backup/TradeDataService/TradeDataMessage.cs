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

using OPEX.Messaging;
using OPEX.Common;
using OPEX.Storage;
using OPEX.OM.Common;

namespace OPEX.TDS.Common
{
    /// <summary>
    /// Collects the information that identify an order fill. 
    /// </summary>
    [Serializable]
    public class TradeDataMessage : IChannelMessage, IWriteable
    {
        private static readonly string SQLFieldList =
            @"FillID,TimeSig,OrderID,Quantity,Price,Counterparty,Instrument,DateSig";

        private DateTime _timeStamp;
        private long _fillID;
        private long _orderID;
        private int _quantity;
        private double _price;
        private string _counterparty;
        private string _instrument;
        private string _user;
        private double _limitPrice;
        private OrderSide _side;

        /// <summary>
        /// Gets the counterparty of the fill.
        /// </summary>
        public string Counterparty
        {
            get { return _counterparty; }
        }

        /// <summary>
        /// Gets the price of the fill.
        /// </summary>
        public double Price
        {
            get { return _price; }
        }

        /// <summary>
        /// Gets the limit price of the underlying order.
        /// </summary>
        public double LimitPrice
        {
            get { return _limitPrice; }
        }

        /// <summary>
        /// Gets the quantity traded.
        /// </summary>
        public int Quantity
        {
            get { return _quantity; }
        }

        /// <summary>
        /// Gets the order ID of the underlying order.
        /// </summary>
        public long OrderID
        {
            get { return _orderID; }
        }

        /// <summary>
        /// Gets the unique numeric ID of this TradeDataMessage.
        /// </summary>
        public long FillID
        {
            get { return _fillID; }
        }

        /// <summary>
        /// Gets the timestamp of the creation of this TradeDataMessage.
        /// </summary>
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
        }

        /// <summary>
        /// Gets the RIC of the security traded.
        /// </summary>
        public string Instrument
        {
            get { return _instrument; }
        }

        /// <summary>
        /// Gets the user name of the owner of the underlying order.
        /// </summary>
        public string User
        {
            get { return _user; }
        }

        /// <summary>
        /// Gets the side of the underlying order.
        /// </summary>
        public OrderSide Side
        {
            get { return _side; }
        }

        /// <summary>
        /// Initialises a new instance of the class OPEX.TDS.Common.TradeDataMessage.
        /// </summary>
        /// <param name="fillID">The unique numeric ID of this TradeDataMessage.</param>
        /// <param name="orderID">The order ID of the underlying order.</param>
        /// <param name="quantity">The quantity traded.</param>
        /// <param name="price">The price of the fill.</param>
        /// <param name="limitPrice">The limit price of the underlying order.</param>
        /// <param name="user">The user name of the owner of the underlying order.</param>
        /// <param name="counterparty">The counterparty of the fill.</param>
        /// <param name="instrument">The RIC of the security traded.</param>
        /// <param name="side">The side of the underlying order.</param>
        internal TradeDataMessage(long fillID, long orderID, int quantity, double price, double limitPrice, string user, string counterparty, string instrument, OrderSide side)
        {
            _timeStamp = DateTime.Now;
            _fillID = fillID;
            _orderID = orderID;
            _quantity = quantity;
            _price = price;
            _counterparty = counterparty;
            _instrument = instrument;
            _limitPrice = limitPrice;
            _user = user;
            _side = side;
        }

        /// <summary>
        /// Returns the string representation of this TradeDataMessage.
        /// </summary>
        public override string ToString()
        {
            return string.Format("FillID {0} OrderID {1} Quantity {2} Price {3} LimitPrice {4} User {5} Counterparty {6} Instrument {7} Side {8}",
                _fillID, _orderID, _quantity, _price, _limitPrice, _user, _counterparty, _instrument, _side);
        }

        #region IChannelMessage Members

        public string Origin
        {
            get { return "TDSOrigin"; }
        }

        public string Destination
        {
            get { return "TDSDestination"; }
        }

        #endregion

        #region IWriteable Members

        public string TableName { get { return "Fills"; } }
        public string FieldList { get { return SQLFieldList; } }
        public string Values
        {
            get 
            {
                return string.Format("{0},'{1}',{2},{3},{4},'{5}','{6}','{7}'", 
                    _fillID, _timeStamp.ToString("HHmmss.ffffff"), _orderID,
                    _quantity, _price, _counterparty, _instrument, DateTime.Today.ToString("yyyyMMdd"));
            }
        }

        #endregion
    }
}
