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

using OPEX.DES.Exchange;
using OPEX.Storage;
using OPEX.DES.OrderManager;

namespace OPEX.DES.DB
{
    public class WriteableFill : IWriteable
    {
        private readonly string SQLFields = "SimID, Round, Move, Quantity, Price, LimitPrice, User, Counterparty, Instrument, Side, DateSig";
        private readonly int _quantity;
        private readonly double _price;
        private readonly double _limitPrice;
        private readonly string _user;
        private readonly string _counterparty;
        private readonly string _instrument;
        private readonly OrderSide _side;
        private readonly TimeStamp _timeStamp;

        public WriteableFill(int quantity, double price, double limitPrice, string user, string counterparty, string instrument, OrderSide side)
        {
            _quantity = quantity;
            _price = price;
            _side = side;
            _limitPrice = limitPrice;
            _user = user;
            _counterparty = counterparty;
            _instrument = instrument;
            _timeStamp = TimeManager.CurrentTimeStamp;
        }

        #region IWriteable Members

        public string TableName
        {
            get { return "DESFills"; }
        }

        public string FieldList
        {
            get { return SQLFields; }
        }

        public string Values
        {
            get
            {
                return string.Format("{0}, {1}, {2}, {3}, {4}, {5}, '{6}', '{7}', '{8}', '{9}', '{10}'",
                    _timeStamp.SimID, _timeStamp.Round, _timeStamp.Move, _quantity, _price, _limitPrice,
                    _user, _counterparty, _instrument, _side.ToString(), DateTime.Today.ToString("yyyyMMdd"));
            }
        }      

        #endregion
    }
}
