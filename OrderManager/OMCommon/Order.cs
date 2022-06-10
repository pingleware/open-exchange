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
using System.Text;
using System.Runtime.Serialization;

using OPEX.Messaging;
using OPEX.Common;
using OPEX.Storage;

namespace OPEX.OM.Common
{
    /// <summary>
    /// Represents a generic order that can be
    /// written to a DB, and sent and received through
    /// a communication channel.
    /// This class is abstract.
    /// </summary>
    [Serializable]
    public abstract class Order : IOrder, IWriteable, IChannelMessage
    {
        protected static readonly string SQLFieldList =
            @"ClientOrderID,OrderID,ParentOrderID,Origin,Destination,Status,Instrument,Side,Currency,Type,Quantity,QuantityFilled,LastQuantityFilled,Price,LastPriceFilled,AveragePriceFilled,TimeSig,Version,Parameters,Message,User,LimitPrice,DateSig";
        protected long _clientOrderID;
        protected long _orderID;
        protected long _parentOrderID;

        protected string _instrument;
        protected string _origin;
        protected string _destination;
        protected string _currency;

        protected OrderSide _side;        
        protected OrderStateMachine _stateMachine;
        protected OrderType _type;

        protected int _quantity;
        protected int _quantityFilled;
        protected int _lastQuantityFilled;

        protected double _limitPrice;
        protected double _price;
        protected double _lastPriceFilled;
        protected double _averagePriceFilled;

        protected string _message;
        protected string _parameters;

        protected DateTime _timeStamp;
        protected string _user;

        [NonSerialized]
        protected int _version;

        /// <summary>
        /// Creates a new Order, setting
        /// TimeStamp and State only.
        /// </summary>
        protected Order()
        {
            _version = 1;
            _timeStamp = DateTime.Now;

            _stateMachine = new OrderStateMachine(OrderStatus.NewOrder);
        }

        /// <summary>
        /// Creates a new Order, copying the
        /// entire content of the order passed
        /// as a parameter.
        /// </summary>
        /// <param name="o">The order from which to copy the entire content.</param>
        protected Order(Order o)
        {
            _version = o._version;
            _timeStamp = DateTime.Now;

            _clientOrderID = o._clientOrderID;
            _orderID = o._orderID;
            _parentOrderID = o._parentOrderID;

            _instrument = o._instrument;
            _origin = o._origin;
            _destination = o._destination;
            _currency = o._currency;
            _side = o._side;
            _type = o._type;            

            CopyContent(o);
        }

        /// <summary>
        /// Copies the following from the order passed
        /// as a parameter: State, QuantityFilled, LastQuantityFilled,
        /// LastPriceFilled, AveragePriceFilled.
        /// </summary>
        /// <param name="o">The order from which to copy the fields.</param>
        internal void CopyContent(Order o)
        {
            _stateMachine = new OrderStateMachine(o.Status);

            _quantity = o._quantity;
            _price = o._price;
            _limitPrice = o._limitPrice;
            _parameters = o._parameters;
            _message = o._message;
            _quantityFilled = o._quantityFilled;
            _lastQuantityFilled = o._lastQuantityFilled;
            _lastPriceFilled = o._lastPriceFilled;
            _averagePriceFilled = o._averagePriceFilled;
            _user = o._user;
        }

        /// <summary>
        /// Returns the string representation of this Order.
        /// </summary>
        public override string ToString()
        {
            return string.Format(
                "CID {15} OID {0} St {9} Usr {16} Dir {2} Qty {3:N0} LmtPrc {17:F4} Prc {5:F4} QtyFld {8:N0} LstQtyFld {10:N0} LstPrcFld {11:F4}" +
                "RIC {4} TIF {6} CCY {7} POID {1}  AvgPrcFld {12:F4} Org {13} Dst {14} Parms {19} Msg {18}",
                _orderID, _parentOrderID, _side.ToString(), _quantity, _instrument, _price, _type.ToString(), _currency, _quantityFilled,
                _stateMachine.Status.ToString(), _lastQuantityFilled, _lastPriceFilled, _averagePriceFilled, _origin, _destination, _clientOrderID, _user, _limitPrice,
                _message, _parameters);
        }

        #region IOrder Members

        public long ClientOrderID
        {
            get { return _clientOrderID; }
        }

        public long OrderID
        {
            get { return _orderID; }
            internal set { _orderID = value; }
        }

        public long ParentOrderID
        {
            get { return _parentOrderID; }
            internal set { _parentOrderID = value; }
        }

        public string Origin
        {
            get
            {
                return _origin;
            }
            internal set
            {
                _origin = value;
            }
        }

        public string Destination
        {
            get
            {
                return _destination;
            }
            set
            {
                _destination = value;
            }
        }

        public OrderStatus Status
        {
            get { return _stateMachine.Status; }
        }

        public string Instrument
        {
            get
            {
                return _instrument;
            }
            set
            {
                _instrument = value;
            }
        }

        public OrderSide Side
        {
            get
            {
                return _side;
            }
            set
            {
                _side = value;
            }
        }

        public string Currency
        {
            get
            {
                return _currency;
            }
            set
            {
                _currency = value;
            }
        }

        public OrderType Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        public int Quantity
        {
            get
            {
                return _quantity;
            }
            set
            {
                _quantity = value;
            }
        }

        public int QuantityFilled
        {
            get
            {
                return _quantityFilled;
            }
            set
            {
                _quantityFilled = value;
            }
        }

        public int LastQuantityFilled
        {
            get
            {
                return _lastQuantityFilled;
            }
            set
            {
                _lastQuantityFilled = value;
            }
        }

        public double LimitPrice
        {
            get
            {
                return _limitPrice;
            }
            set
            {
                _limitPrice = value;
            }
        }

        public double Price
        {
            get
            {
                return _price;
            }
            set
            {
                _price = value;
            }
        }

        public double LastPriceFilled
        {
            get
            {
                return _lastPriceFilled;
            }
            set
            {
                _lastPriceFilled = value;
            }
        }

        public double AveragePriceFilled
        {
            get
            {
                return _averagePriceFilled;
            }
            set
            {
                _averagePriceFilled = value;
            }
        }

        public int QuantityRemaining
        {
            get { return _quantity - _quantityFilled; }
        }

        public double Turnover
        {
            get { return _averagePriceFilled * _quantityFilled; }
        }

        public string Message
        {
            get { return _message; }
            internal set { _message = value; }
        }

        public string Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            internal set { _timeStamp = value; }
        }

        public string User
        {
            get { return _user; }
            set { _user = value; }
        }

        #endregion

        #region IWriteable Members

        public virtual string TableName
        {
            get { return null;  }
        }

        public string FieldList { get { return SQLFieldList; } }
        public string Values
        {
            get
            {
                StringBuilder sb = new StringBuilder();
               
                sb.AppendFormat("{0},{1},{2}", _clientOrderID, _orderID, _parentOrderID);
                sb.AppendFormat(",\"{0}\",\"{1}\",\"{2}\"", _origin, _destination, _stateMachine.Status.ToString());
                sb.AppendFormat(",\"{0}\",\"{1}\",\"{2}\"", _instrument, _side.ToString(), _currency);
                sb.AppendFormat(",\"{0}\",{1},{2}", _type.ToString(), _quantity, _quantityFilled);
                sb.AppendFormat(",{0},{1},{2}", _lastQuantityFilled, _price, _lastPriceFilled);
                sb.AppendFormat(",{0},\"{1}\",{2}", _averagePriceFilled, _timeStamp.ToString("HHmmss.ffffff"), _version);
                sb.AppendFormat(",\"{0}\",\"{1}\",'{2}',{3},'{4}'", _parameters, _message, _user, _limitPrice, DateTime.Today.ToString("yyyyMMdd"));

                return sb.ToString();
            }
        }

        #endregion
    }
}
