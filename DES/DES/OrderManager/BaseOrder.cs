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

namespace OPEX.DES.OrderManager
{
    public abstract class BaseOrder : IIncomingOrder, IOutgoingOrder
    {
        private static long _lastID;

        protected readonly IOrderProcessor _orderProcessor;
        protected readonly long _id;
        protected readonly string _ric;
        protected readonly string _owner;
        protected readonly OrderSide _side;

        protected double _price;
        protected double _limitPrice;
        protected double _avgPriceFilled;
        protected int _quantity;
        protected int _quantityFilled;
        protected bool _open;
        protected TimeStamp _timeStamp;
        protected bool _isSent;

        static BaseOrder()
        {
            Random r = new Random((int)DateTime.Now.TimeOfDay.TotalSeconds);
            _lastID = r.Next();
        }

        static long NextID()
        {
            if (_lastID == long.MaxValue)
            {
                _lastID = 0;
            }
            return _lastID++;
        }

        public BaseOrder(string ric, string owner, OrderSide side, double price, double limitPrice, int quantity, IOrderProcessor orderProcessor)
        {
            _id = NextID();
            _ric = ric;
            _owner = owner;
            _side = side;
            _price = price;
            _limitPrice = limitPrice;
            _quantity = quantity;
            _orderProcessor = orderProcessor;
        }

        #region IOrder Members
        
        public long ID
        {
            get { return _id; }
        }

        public string RIC
        {
            get { return _ric; }
        }

        public string Owner
        {
            get { return _owner; }
        }

        public OrderSide Side
        {
            get { return _side; }
        }

        public double AvgPriceFilled
        {
            get { return _avgPriceFilled; }
        }

        public double LimitPrice
        {
            get { return _limitPrice; }
        }

        public int QuantityFilled
        {
            get { return _quantityFilled; }
        }

        public bool Open
        {
            get { return _open; }
        }

        public TimeStamp TimeStamp
        {
            get { return _timeStamp; }
        }

        public int QuantityRemaining
        {
            get { return _quantity - _quantityFilled; }
        }

        #endregion

        #region IIncomingOrder Members

        public void AcceptNewOrder()
        {
            _open = true;
        }

        public bool Fill(int quantity, double price)
        {
            if (!_open)
            {
                return false;
            }

            int remainingQuantity = _quantity - _quantityFilled;
            if (quantity > remainingQuantity)
            {
                return false;
            }

            if ((_side == OrderSide.Buy && price > _price)
                || (_side == OrderSide.Sell && price < _price))
            {
                return false;
            }

            _avgPriceFilled = (_avgPriceFilled * _quantityFilled + price * quantity) / (_quantityFilled + quantity);
            _quantityFilled += quantity;

            if (_quantityFilled == _quantity)
            {
                _open = false;
            }

            return true;
        }

        public bool Cancel()
        {
            _open = false;
            return true;
        }

        public bool AcceptAmend(int newQuantity, double newPrice)
        {
            _quantity = newQuantity;
            _price = newPrice;
            return true;
        }

        #endregion

        #region IOutgoingOrder Members

        public bool IsSent { get { return _isSent; } }
        public double Price { get { return _price; } set { _price = value; } }
        public int Quantity { get { return _quantity; } set { _quantity = value; } }

        public bool Send()
        {
            if (_isSent)
            {
                return false;
            }

            _timeStamp = TimeManager.CurrentTimeStamp;
            _isSent = _orderProcessor.AcceptNewOrder(this);
            return _isSent;
        }

        public bool Amend(double newPrice)
        {
            if (!_isSent)
            {
                return false;
            }

            return _orderProcessor.AcceptOrderAmendment(this, newPrice, _quantity);
        }

        public bool Amend(double newPrice, int newQuantity)
        {
            if (!_isSent)
            {
                return false;
            }

            return _orderProcessor.AcceptOrderAmendment(this, newPrice, newQuantity);
        }        

        #endregion
    }
}
