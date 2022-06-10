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

using OPEX.Common;

namespace OPEX.OM.Common
{
    /// <summary>
    /// Represents the method that will handle an event
    /// that has Order arguments.
    /// </summary>
    /// <param name="sender">The sender of this event.</param>
    /// <param name="args">The Order associated to this event.</param>
    public delegate void OutgoingOrderEventHandler(object sender, Order order);

    /// <summary>
    /// Represents an Order that is sent from this application
    /// to a specific destination.
    /// </summary>
    [Serializable]
    public class OutgoingOrder : Order
    {
        [NonSerialized]
        private IOrderSender _orderSender;
        [NonSerialized]
        private IncomingOrder _parentOrder;
        [NonSerialized]
        private bool _isSent;
        [NonSerialized]
        private Logger _logger;
        [NonSerialized]
        private OutgoingOrderEventHandler _orderSent;
        [NonSerialized]
        private OutgoingOrderEventHandler _orderCancelled;
        [NonSerialized]
        private OutgoingOrderEventHandler _statusChanged;
        [NonSerialized]
        private OutgoingOrderEventHandler _orderAmended;


        #region C'tor

        /// <summary>
        /// Creates a new empty OutgoingOrder, only setting
        /// State, TimeStamp and ClientOrderID.
        /// this.OrderID = 0; this.ClientOrderID = clientOrderID
        /// this.State = NEWORDER
        /// </summary>
        /// <param name="orderSender">The IOrderSender that will be used to send the order.</param>
        /// <param name="clientOrderID">The ClientOrderID to assign the order.</param>
        internal OutgoingOrder(IOrderSender orderSender, long clientOrderID)
            : base()
        {
            _clientOrderID = clientOrderID;
            _orderSender = orderSender;
            _logger = new Logger(string.Format("OutgoingOrder({0},{1})", _orderID, _clientOrderID));
        }

        /// <summary>
        /// Creates a new empty OutgoingOrder, only setting
        /// State, TimeStamp, ClientOrderID and OrderID.
        /// this.OrderID = orderID; this.ClientOrderID = clientOrderID;
        /// this.State = NEWORDER
        /// </summary>
        /// <param name="orderSender">The IOrderSender that will be used to send the order.</param>
        /// <param name="clientOrderID">The ClientOrderID to assign the order.</param>
        /// <param name="orderID">The OrderID to assign the order.</param>
        internal OutgoingOrder(IOrderSender orderSender, long clientOrderID, long orderID)
            : this(orderSender, clientOrderID)
        {
            _orderID = orderID;
            _logger = new Logger(string.Format("OutgoingOrder({0})", _clientOrderID));
        }

        /// <summary>
        /// Creates a new OutgoingOrder copying the entire content
        /// of the Order passed as a parameter, and overwriting the
        /// ClientOrderID with the one passed as a parameter.
        /// this.ClientOrderID = clientOrderID; this.OrderID = 0;
        /// this.State = NEWORDER
        /// </summary>
        /// <param name="orderSender">The IOrderSender that will be used to send the order.</param>
        /// <param name="clientOrderID">The ClientOrderID to assign the order.</param>
        /// <param name="order">The Order from which to copy the content from.</param>
        internal OutgoingOrder(IOrderSender orderSender, long clientOrderID, Order order)
            : base(order)
        {
            _clientOrderID = clientOrderID;
            _orderSender = orderSender;
            _stateMachine = new OrderStateMachine(OrderStatus.NewOrder);
            _instrument = order.Instrument;
            _logger = new Logger(string.Format("OutgoingOrder({0})", _clientOrderID));
        }

        /// <summary>
        /// Creates an OutgoingOrder IDENTICAL to the IncomingOrder passed as a parameter.
        /// this.OrderID = order.OrderID; this.ClientOrderID = order.ClientOrderID;
        /// this.State = order.State
        /// </summary>
        /// <param name="orderSender">The IOrderSender that will be used to send the order.</param>
        /// <param name="order">The Order from which to copy the content from.</param>
        internal OutgoingOrder(IOrderSender orderSender, IncomingOrder order)
            : base(order)
        {
            _orderSender = orderSender;
            _orderID = order.OrderID;
            _logger = new Logger(string.Format("OutgoingOrder({0})", _clientOrderID));
        }

        #endregion C'tor

        #region Properties        

        /// <summary>
        /// Indicates whether the order has been sent.
        /// </summary>
        public bool IsSent { get { return _isSent; } }

        /// <summary>
        /// Gets the IncomingOrder from which this OutgoingOrder
        /// was generated, if any.
        /// </summary>
        public IncomingOrder ParentOrder
        {
            get
            {
                return _parentOrder;
            }
            internal set 
            {
                _parentOrder = value;
            }
        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Occurs when new order request is sent.
        /// </summary>
        public event OutgoingOrderEventHandler OrderSent
        {
            add { _orderSent += value; }
            remove { _orderSent -= value; }
        }

        /// <summary>
        /// Occurs when an amend order request is sent.
        /// </summary>
        public event OutgoingOrderEventHandler OrderAmended
        {
            add { _orderAmended += value; }
            remove { _orderAmended -= value; }
        }

        /// <summary>
        /// Occurs when a cancel order request is sent.
        /// </summary>
        public event OutgoingOrderEventHandler OrderCancelled
        {
            add { _orderCancelled += value; }
            remove { _orderCancelled -= value; }
        }

        /// <summary>
        /// Event raised when the status of the order has changed.
        /// </summary>
        public event OutgoingOrderEventHandler StatusChanged
        {
            add { _statusChanged += value; }
            remove { _statusChanged -= value; }
        }

        #endregion Events

        /// <summary>
        /// Sends the order.
        /// </summary>
        public void Send()
        {
            if (_isSent)
            {
                throw new ApplicationException("Can't send the order: order already sent!");
            }

            if (_orderSender == null)
            {
                throw new NullReferenceException("No Order Sender defined!");
            }

            _orderSender.Send(this);

            if (_orderSent != null)
            {
                foreach (OutgoingOrderEventHandler handler in _orderSent.GetInvocationList())
                {
                    handler(this, this);
                }
            }

            _isSent = true;
        }

        /// <summary>
        /// Sends the order.
        /// </summary>
        public bool SafeSend(ref string errorMessage)
        {
            bool allWell = false;

            if (_isSent)
            {
                errorMessage = "Can't send the order: order already sent!";
            }
            else if (_orderSender == null)
            {
                errorMessage = "No Order Sender defined!";
            }
            else
            {
                try
                {
                    _orderSender.Send(this);

                    if (_orderSent != null)
                    {
                        foreach (OutgoingOrderEventHandler handler in _orderSent.GetInvocationList())
                        {
                            handler(this, this);
                        }
                    }

                    allWell = true;
                }
                catch (Exception ex)
                {
                    errorMessage = string.Format("SafeSend. Exception: {0} {1}", ex.Message, ex.StackTrace.Replace(Environment.NewLine, " ").Replace("\n", " "));
                }
            }

            if (allWell)
            {
                _isSent = true;
            }

            return allWell;
        }

        /// <summary>
        /// Cancels the order.
        /// </summary>
        public void Cancel()
        {
            if (_orderSender == null)
            {
                _logger.TraceAndThrow("No Order Sender defined!");
            }

            if (!_isSent)
            {
                _logger.TraceAndThrow("Can't cancel the order: order not sent!");
            }

            _orderSender.Cancel(this);

            if (_orderCancelled != null)
            {
                foreach (OutgoingOrderEventHandler handler in _orderCancelled.GetInvocationList())
                {
                    handler(this, this);
                }
            }
        }

        /// <summary>
        /// Cancels the order.
        /// </summary>
        public bool SafeCancel(ref string errorMessage)
        {
            bool allWell = false;

            if (!_isSent)
            {
                errorMessage = "Can't cancel the order: order not sent!";
            }
            else if (_orderSender == null)
            {
                errorMessage = "No Order Sender defined!";
            }
            else
            {
                try
                {
                    _orderSender.Cancel(this);

                    if (_orderCancelled != null)
                    {
                        foreach (OutgoingOrderEventHandler handler in _orderCancelled.GetInvocationList())
                        {
                            handler(this, this);
                        }
                    }

                    allWell = true;
                }
                catch (Exception ex)
                {
                    errorMessage = string.Format("SafeCancel. Exception: {0} {1}", ex.Message, ex.StackTrace.Replace(Environment.NewLine, " ").Replace("\n", " "));
                }
            }           

            return allWell;
        }

        /// <summary>
        /// Amends the order.
        /// </summary>
        /// <param name="quantity">The new quantity.</param>
        /// <param name="price">The new price.</param>
        /// <param name="parameters">The new parameters.</param>
        public void Amend(int quantity, double price, string parameters)
        {
            InnerAmend(quantity, price, parameters);
        }        

        /// <summary>
        /// Amends the order.
        /// </summary>
        /// <param name="parameters">The new parameters.</param>
        public void Amend(string parameters)
        {
            InnerAmend(this.Quantity, this.Price, parameters);
        }

        /// <summary>
        /// Amends the order.
        /// </summary>
        /// <param name="parameters">The new parameters.</param>
        public bool SafeAmend(string parameters, ref string errorMessage)
        {
            string error = null;
            bool res = SafeAmend(this.Quantity, this.Price, parameters, ref error);
            errorMessage = error;
            return res;
        }

        /// <summary>
        /// Amends the order.
        /// </summary>
        /// <param name="quantity">The new quantity.</param>
        /// <param name="price">The new price.</param>
        public void Amend(int quantity, double price)
        {
            InnerAmend(quantity, price, this.Parameters);
        }

        /// <summary>
        /// Amends the order.
        /// </summary>
        /// <param name="quantity">The new quantity.</param>
        /// <param name="price">The new price.</param>
        public bool SafeAmend(int quantity, double price, ref string errorMessage)
        {
            string error = null;
            bool res = SafeAmend(quantity, price, this.Parameters, ref error);
            errorMessage = error;
            return res;
        }

        /// <summary>
        /// Amends the order.
        /// </summary>
        /// <param name="quantity">The new quantity.</param>
        public void Amend(int quantity)
        {
            InnerAmend(quantity, this.Price, this.Parameters);
        }

        /// <summary>
        /// Amends the order.
        /// </summary>
        /// <param name="quantity">The new quantity.</param>
        public bool SafeAmend(int quantity, ref string errorMessage)
        {
            string error = null;
            bool res = SafeAmend(quantity, this.Price, this.Parameters, ref error);
            errorMessage = error;
            return res;
        }

        /// <summary>
        /// Amends the order.
        /// </summary>
        /// <param name="price">The new price.</param>
        public void Amend(double price)
        {
            InnerAmend(this.Quantity, price, this.Parameters);
        }

        /// <summary>
        /// Amends the order.
        /// </summary>
        /// <param name="price">The new price.</param>
        public bool SafeAmend(double price, ref string errorMessage)
        {
            string error = null;
            bool res = SafeAmend(this.Quantity, price, this.Parameters, ref error);
            errorMessage = error;
            return res;
        }
        
        private void InnerAmend(int quantity, double price, string parameters)
        {
            if (_orderSender == null)
            {
                _logger.TraceAndThrow("No Order Sender defined!");
            }

            if (!_isSent)
            {
                _logger.TraceAndThrow("Can't amend the order: order is not sent!");
            }

            if (!OrderStateMachine.IsActiveStatus(_stateMachine.Status))
            {
                _logger.TraceAndThrow("Can't amend the order: status is {0}", _stateMachine.Status);
            }

            ShadowOrder shadowOrder = new ShadowOrder(this);
            shadowOrder.Quantity = quantity;
            shadowOrder.Price = price;
            shadowOrder.Parameters = parameters;
            _orderSender.Amend(this, shadowOrder);

            if (_orderAmended != null)
            {
                foreach (OutgoingOrderEventHandler handler in _orderAmended.GetInvocationList())
                {
                    handler(this, shadowOrder);
                }                
            }
        }

        /// <summary>
        /// Amends the order.
        /// </summary>
        public bool SafeAmend(int quantity, double price, string parameters, ref string errorMessage)
        {
            bool allWell = false;

            if (!_isSent)
            {
                errorMessage = "Can't amend the order: order is not sent!";
            }
            else if (_orderSender == null)
            {
                errorMessage = "No Order Sender defined!";
            }
            else if (!OrderStateMachine.IsActiveStatus(_stateMachine.Status))
            {
                errorMessage = string.Format("Can't amend the order: status is {0}", _stateMachine.Status);
            }
            else
            {
                try
                {
                    ShadowOrder shadowOrder = new ShadowOrder(this);
                    shadowOrder.Quantity = quantity;
                    shadowOrder.Price = price;
                    shadowOrder.Parameters = parameters;
                    _orderSender.Amend(this, shadowOrder);

                    if (_orderAmended != null)
                    {
                        foreach (OutgoingOrderEventHandler handler in _orderAmended.GetInvocationList())
                        {
                            handler(this, shadowOrder);
                        }
                    }

                    allWell = true;
                }
                catch (Exception ex)
                {
                    errorMessage = string.Format("SafeAmend. Exception: {0} {1}", ex.Message, ex.StackTrace.Replace(Environment.NewLine, " ").Replace("\n", " "));
                }
            }
          
            return allWell;
        }

        internal void PerformUpdate(Order newOrder)
        {
            bool statusHasChanged = (newOrder.Status != this.Status);
            bool quantityHasChanged  = (newOrder.QuantityFilled != this.QuantityFilled);
            bool somethingElseHasChanged = (newOrder.Quantity != this.Quantity ||
                newOrder.Price != this.Price ||
                newOrder.Parameters != this.Parameters);
            bool somethingHasChanged = statusHasChanged || quantityHasChanged || somethingElseHasChanged;

            this.OrderID = newOrder.OrderID;
            _logger.LogTitle = string.Format("OutgoingOrder({0},{1})", _orderID, _clientOrderID);

            _logger.Trace(LogLevel.Debug, "PerformUpdate. statusHasChanged {0} quantityFilledHasChanged {1} somethingElseHasChanged {2}",
                statusHasChanged.ToString(), quantityHasChanged.ToString(), somethingElseHasChanged.ToString());

            if (somethingHasChanged)
            {
                if (statusHasChanged)
                {
                    _logger.Trace(LogLevel.Info, "PerformUpdate. OldStatus {0} NewStatus {1}",
                        this.Status.ToString(), newOrder.Status.ToString());
                }
                if (quantityHasChanged)
                {
                    _logger.Trace(LogLevel.Info, "PerformUpdate. OldQuantityFilled {0} NewQuantityFilled {1}",
                        this.QuantityFilled.ToString("N0"), newOrder.QuantityFilled.ToString("N0"));
                }
                if (somethingElseHasChanged)
                {
                    _logger.Trace(LogLevel.Info, "PerformUpdate. OldQuantity {0} NewQuantity {1} OldPrice {2} NewPrice {3} OldParameters '{4}' NewParameters '{5}'",
                                           this.Quantity.ToString("N0"), newOrder.Quantity.ToString("N0"),
                                           this.Price.ToString("F4"), newOrder.Price.ToString("F4"),
                                           this.Parameters, newOrder.Parameters);
                }
            }            

            this.CopyContent(newOrder);

            if (statusHasChanged)
            {
                this._stateMachine.Change(newOrder.Status);                
            }
            
            if (_statusChanged != null)
            {
                foreach (OutgoingOrderEventHandler handler in _statusChanged.GetInvocationList())
                {
                    handler(this, newOrder);
                }
            }

            IncomingOrder parentOrder = this.ParentOrder;
            if (parentOrder != null)
            {
                bool update = (this.OrderID != parentOrder.OrderID);
                if (update)
                {
                    _logger.Trace(LogLevel.Warning, 
                        "OutgoingOrder.PerformUpdate. If you're reading this, then you used CreateRelatedOutgoingOrder.");
                    parentOrder.CopyContent(newOrder);
                    parentOrder.PerformChanged();
                }
            }
        }

        public override string TableName
        {
            get
            {
                return "Orders";
            }
        }
    }
}
