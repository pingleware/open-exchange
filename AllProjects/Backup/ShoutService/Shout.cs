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

using OPEX.Storage;
using OPEX.Messaging;
using OPEX.OM.Common;

namespace OPEX.ShoutService
{
    /// <summary>
    /// Represents the market's perspective of an order.
    /// </summary>
    [Serializable]
    public class Shout : IChannelMessage, IWriteable
    {
        private static readonly string SQLFieldList =
            @"ShoutID,TimeSig,Side,Accepted,Price,User,Instrument,DateSig";

        private readonly DateTime _timeStamp;
        private readonly double _price;
        private readonly bool _accepted;
        private readonly string _instrument;
        private readonly OrderSide _side;
        private readonly string _user;
        private readonly long _id;

        private DateTime _localTimeStamp;

        /// <summary>
        /// Initialises a new instance of the class OPEX.SoutService.Shout.
        /// </summary>
        /// <param name="ID">The numerical unique ID of this Shout.</param>
        /// <param name="accepted">Identifies whether this Shouts matches some previously sent order in the market.</param>
        /// <param name="price">The price of this Shout.</param>
        /// <param name="instrument">The instrument to buy or sell.</param>
        /// <param name="side">The side of the trade, i.e. buy or sell.</param>
        /// <param name="user">The user id of the market participant who wants to buy or sell.</param>
        internal Shout(long ID, bool accepted, double price, string instrument, OrderSide side, string user)
        {
            _id = ID;
            _localTimeStamp = _timeStamp = DateTime.Now;
            _accepted = accepted;
            _price = price;
            _instrument = instrument;
            _side = side;
            _user = user;
        }

        /// <summary>
        /// Gets the side of the trade, i.e. buy or sell.
        /// </summary>
        public OrderSide Side { get { return _side; } }

        /// <summary>
        /// Identifies whether this Shouts matches some previously sent order in the market.
        /// </summary>
        public bool Accepted { get { return _accepted; } }

        /// <summary>
        /// Gets the price of this Shout.
        /// </summary>
        public double Price { get { return _price; } }

        /// <summary>
        /// Gets the timestamp of the creation of this Shout.
        /// </summary>
        public DateTime TimeStamp { get { return _timeStamp; } }

        /// <summary>
        /// Gets the timestamp of the creation of this Shout.
        /// </summary>
        public DateTime LocalTimeStamp { get { return _localTimeStamp; } internal set { _localTimeStamp = value; } }

        /// <summary>
        /// Gets the instrument to buy or sell.
        /// </summary>
        public string Instrument { get { return _instrument; } }

        /// <summary>
        /// Gets the user id of the market participant who wants to buy or sell.
        /// </summary>
        public string User { get { return _user; } }

        /// <summary>
        /// Gets the numerical unique ID of this Shout.
        /// </summary>
        public long ID { get { return _id; } }

        /// <summary>
        /// Returns the string representation of this Shout.
        /// </summary>
        public override string ToString()
        {
            return string.Format("ID {5} Accepted {0} Side {1} Price {2:F4} Instrument {3} TimeStamp {4}", _accepted, _side, _price, _instrument, _timeStamp, _id);
        }

        #region IChannelMessage Members

        /// <summary>
        /// Gets the origin of the message.
        /// </summary>
        public string Origin
        {
            get { return "*"; }
        }

        /// <summary>
        /// Gets the destination of the message.
        /// </summary>
        public string Destination
        {
            get { return "*"; }
        }

        #endregion

        #region IWriteable Members

        public string TableName { get { return "Shouts"; } }
        public string FieldList { get { return SQLFieldList; } }
        public string Values
        {
            get
            {
                return string.Format("{0},'{1}','{2}',{3},{4},'{5}','{6}','{7}'", 
                    _id, _timeStamp.ToString("HHmmss.ffffff"), _side.ToString(),
                     _accepted ? 1 : 0, _price, _user, _instrument, DateTime.Today.ToString("yyyyMMdd"));

            }
        }

        public string InsertSQL
        {
            get 
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat("INSERT INTO {0} ({1}) VALUES (", TableName, SQLFieldList); //@"ShoutID,TimeSig,Side,Accepted,Price,User,Instrument,DateSig";
                sb.AppendFormat("{0},'{1}','{2}'", _id, _timeStamp.ToString("HHmmss.ffffff"), _side.ToString());
                sb.AppendFormat(",{0},{1},'{2}','{3}','{4}');", _accepted ? 1 : 0, _price, _user, _instrument, DateTime.Today.ToString("yyyyMMdd"));

                return sb.ToString();
            }
        }

        #endregion
    }
}
