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
    /// Contains event data for events raised when
    /// an IncomingOrder changes.
    /// </summary>
    internal class IncomingOrderChangedEventArgs : EventArgs
    {
        private OrderInstruction _instruction;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.OM.Common.IncomingOrderChangedEventArgs.
        /// </summary>
        /// <param name="instruction">The OrderInstruction associated to 
        /// this IncomingOrderChangedEventArgs.</param>
        public IncomingOrderChangedEventArgs(OrderInstruction instruction)
            : base()
        {
            _instruction = instruction;
        }

        /// <summary>
        /// Gets the OrderInstruction associated to this IncomingOrderChangedEventArgs.
        /// </summary>
        public OrderInstruction Instruction { get { return _instruction; } }
    }

    /// <summary>
    /// Represents an Order that travels towards this application.
    /// </summary>
    [Serializable]
    public class IncomingOrder : Order
    {
        [NonSerialized]
        private Logger _logger;

        [NonSerialized]
        private EventHandler<IncomingOrderChangedEventArgs> _changed;

        /// <summary>
        /// Creates a new IncomingOrder, copying the entire content
        /// of the Order passed as a parameter, and overwriting the
        /// OrderID.
        /// </summary>
        /// <param name="o">The Order from which to copy the content.</param>
        /// <param name="orderID">The OrderID to assign to the new order.</param>
        internal IncomingOrder(Order o, long orderID)
            : base(o)
        {
            if (orderID != 0)
            {
                _orderID = orderID;
            }
            _logger = new Logger(string.Format("IncomingOrder({0})", _orderID));
        }

        /// <summary>
        /// Creates a new IncomingOrder, copying the entire content
        /// of the Order passed as a parameter.
        /// </summary>
        /// <param name="o">The Order from which to copy the content.</param>
        internal IncomingOrder(Order o)
            : this(o, 0)
        {
        }

        /// <summary>
        /// Accepts a new order.
        /// </summary>
        public void Accept()
        {
            ChangeStatus(OrderStatus.Accepted);
            OnChanged();
        }

        /// <summary>
        /// Rejects a new order request.
        /// </summary>
        /// <param name="reson">The reason why the request was rejected.</param>
        public void Reject(string reason)
        {
            ChangeStatus(OrderStatus.Rejected);            
            OnChanged(OrderInstruction.New, reason);
        }

        /// <summary>
        /// Accepts a new cancellation request.
        /// </summary>
        public void AcceptCancel()
        {
            ChangeStatus(OrderStatus.Cancelled);
            OnChanged(OrderInstruction.Cancel);
        }

        /// <summary>
        /// Rejects a cancellation request.
        /// </summary>
        /// <param name="reson">The reason why the request was rejected.</param>
        public void RejectCancel(string reason)
        {
            ChangeStatus(OrderStatus.CancelRejected);            
            OnChanged(OrderInstruction.Cancel, reason);
        }

        /// <summary>
        /// Accepts a new amendment request.
        /// </summary>
        /// <param name="newOrder">The new version of the order.</param>
        public void AcceptAmendment(Order newOrder)
        {
            _price = newOrder.Price;
            _quantity = newOrder.Quantity;
            _parameters = newOrder.Parameters;
            ChangeStatus(OrderStatus.AmendAccepted);
            OnChanged(OrderInstruction.Amend);
        }

        /// <summary>
        /// Rejects an amendment request.
        /// </summary>
        /// <param name="reson">The reason why the request was rejected.</param>
        public void RejectAmendment(string reason)
        {
            if (OrderStateMachine.IsActiveStatus(_stateMachine.Status))
            {
                ChangeStatus(OrderStatus.AmendRejected);
            }
            else
            {
                _lastPriceFilled = 0;
                _lastQuantityFilled = 0;
            }
            OnChanged(OrderInstruction.Amend, reason);
        }

        /// <summary>
        /// Cancels the order.
        /// </summary>
        /// <param name="reason">The reason why the order was cancelled.</param>
        public void Cancel(string reason)
        {
            ChangeStatus(OrderStatus.CancelledByExchange);            
            OnChanged(OrderInstruction.Cancel, reason);
        }

        /// <summary>
        /// Fills the order, copying the fill status
        /// of the specified order.
        /// </summary>
        /// <param name="order">The order to copy the fill status from.</param>
        public void Fill(Order order)
        {
            int newQuantityFilled = this._quantityFilled + order.LastQuantityFilled;

            this._lastQuantityFilled = order.LastQuantityFilled;
            this._lastPriceFilled = order.LastPriceFilled;
            this._averagePriceFilled = order.AveragePriceFilled;
            this._quantityFilled = order.QuantityFilled;

            if (_quantity > newQuantityFilled && (_stateMachine.Status != OrderStatus.Filled))
            {
                ChangeStatus(OrderStatus.Filled);
            }
            else if (_quantity == newQuantityFilled && (_stateMachine.Status != OrderStatus.CompletelyFilled))
            {
                ChangeStatus(OrderStatus.CompletelyFilled);
            }
            else if (_quantity < newQuantityFilled && (_stateMachine.Status != OrderStatus.Overfilled))
            {
                ChangeStatus(OrderStatus.Overfilled);
            }

            this._quantityFilled = newQuantityFilled;

            OnChanged();
        }

        /// <summary>
        /// Fills the order.
        /// </summary>
        /// <param name="price">The price to fill the order at.</param>
        /// <param name="quantity">The quantity to fill.</param>
        public void Fill(double price, int quantity)
        {
            int newQuantityFilled = this._quantityFilled + quantity;

            this._lastQuantityFilled = quantity;
            this._lastPriceFilled = price;            
            this._averagePriceFilled = (this._averagePriceFilled * this._quantityFilled + price * quantity) / newQuantityFilled;

            if (_quantity > newQuantityFilled)
            {
                ChangeStatus(OrderStatus.Filled);
            }
            else if (_quantity == newQuantityFilled)
            {
                ChangeStatus(OrderStatus.CompletelyFilled);
            }
            else
            {
                ChangeStatus(OrderStatus.Overfilled);
            }
            
            this._quantityFilled = newQuantityFilled;

            OnChanged();
        }

        /// <summary>
        /// Creates an OutgoingOrder, child of this IncomingOrder.
        /// Typically used by algos.
        /// </summary>
        /// <returns></returns>
        public OutgoingOrder CreateRelatedOutgoingOrder()
        {
            OutgoingOrder o = OrderFactory.CreateRelatedOutgoingOrder(this);
            OnChanged();
            return o;
        }

        /// <summary>
        /// Creates an OutgoingOrder that serves as the 
        /// continuation of this IncomingOrder in a 
        /// routing process. Typically used by the OrderManager.
        /// </summary>
        /// <returns>The OutgoingOrder created.</returns>
        public OutgoingOrder CreateThroughOutgoingOrder()
        {
            return OrderFactory.CreateThroughOutgoingOrder(this);
        }

        /// <summary>
        /// Occurs when the IncomingOrder changes.
        /// </summary>
        internal event EventHandler<IncomingOrderChangedEventArgs> Changed
        {
            add { _changed += value; }
            remove { _changed -= value; }
        }

        private void OnChanged()
        {
            OnChanged(OrderInstruction.New);
        }

        private void OnChanged(OrderInstruction instruction)
        {
            OnChanged(instruction, string.Empty);
        }

        private void OnChanged(OrderInstruction instruction, string message)
        {
            this._timeStamp = DateTime.Now;
            this._version++;
            this._message = message;
            _logger.Trace(LogLevel.Method, "OnChanged. NewVersion {0}", _version.ToString());

            if (_changed != null)
            {
                foreach (EventHandler<IncomingOrderChangedEventArgs> handler in _changed.GetInvocationList())
                {
                    handler(this, new IncomingOrderChangedEventArgs(instruction));
                }
            } 
        }

        private void ChangeStatus(OrderStatus newStatus)
        {
            if (!_stateMachine.Change(newStatus))
            {
                _logger.Trace(LogLevel.Error, "Couldn't change order status from {0} to {1}.", 
                    _stateMachine.Status.ToString(), newStatus.ToString());
            }
        }

        /// <summary>
        /// Gets the DB table to which this IncomingOrder is written.
        /// </summary>
        public override string TableName
        {
            get
            {
                return "Orders";
            }
        }

        /// <summary>
        /// Forcefully raises a changed event. 
        /// </summary>
        internal void PerformChanged()
        {
            OnChanged();
        }        
    }
}
