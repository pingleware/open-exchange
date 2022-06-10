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

namespace OPEX.OM.Common
{
    /// <summary>
    /// Specifies the status of an order.
    /// </summary>
    public enum OrderStatus : int
    {
        /// <summary>
        /// New order, just created.
        /// </summary>
        NewOrder = 0,

        /// <summary>
        /// Created and accepted.
        /// </summary>
        Accepted,

        /// <summary>
        /// Partially filled.
        /// </summary>
        Filled,

        /// <summary>
        /// Completelly filled.
        /// </summary>
        CompletelyFilled,

        /// <summary>
        /// Overfilled.
        /// </summary>
        Overfilled,

        /// <summary>
        /// Rejected by the destination.
        /// </summary>
        Rejected,

        /// <summary>
        /// Cancelled by the Market.
        /// </summary>
        CancelledByExchange,

        /// <summary>
        /// Amend request rejected.
        /// </summary>
        AmendRejected,

        /// <summary>
        /// Amend request accepted.
        /// </summary>
        AmendAccepted,

        /// <summary>
        /// Cancel request rejected.
        /// </summary>
        CancelRejected,

        /// <summary>
        /// Cancel request accepted.
        /// </summary>
        Cancelled
    }

    /// <summary>
    /// Represents the state machine of an Order.
    /// </summary>
    [Serializable]
    public class OrderStateMachine
    {
        private OrderStatus _state;

        /// <summary>
        /// Checks whether a specific OrderStatus represents an
        /// 'active' state of the OrderStateMachine, that is
        /// a state in which an Order can be filled, amended
        /// or cancelled.
        /// </summary>
        /// <param name="status">The OrderStatus to check.</param>
        /// <returns>True if the specified OrderStatus represents an 'active'
        /// state of the OrderStateMachine.</returns>
        public static bool IsActiveStatus(OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.Accepted:
                case OrderStatus.Filled:
                case OrderStatus.AmendAccepted:
                case OrderStatus.AmendRejected:
                case OrderStatus.CancelRejected:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.OM.Common.OrderStateMachine.
        /// </summary>
        /// <param name="initialStatus">The initial OrderStatus
        /// of the OrderStateMachine.</param>
        public OrderStateMachine(OrderStatus initialStatus)
        {
            _state = initialStatus;
        }

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.OM.Common.OrderStateMachine, using the
        /// default initial OrderStatus.
        /// </summary>
        public OrderStateMachine()
            : this(OrderStatus.NewOrder)
        { }

        /// <summary>
        /// Gets the current status of the OrderStateMachine.
        /// </summary>
        public OrderStatus Status { get { return _state; } }

        /// <summary>
        /// Tries to switch to the specified OrderStatus.
        /// </summary>
        /// <param name="newState">The OrderStateMachine to switch to.</param>
        /// <returns>True if it was possible to switch to the specified
        /// status. False otherwise.</returns>
        public bool Change(OrderStatus newState)
        {
            return CheckAndChange(newState);
        }

        private bool CheckAndChange(OrderStatus newState)
        {
            bool res = false;

            switch (newState)
            {
                case OrderStatus.NewOrder:
                    break;
                case OrderStatus.Accepted:
                case OrderStatus.Rejected:
                    {
                        switch (_state)
                        {                            
                            case OrderStatus.NewOrder:
                                res = true;
                                break;
                            default:
                                break;
                        }
                        break;
                    }
                case OrderStatus.Overfilled:
                    {
                        res = IsActiveStatus(_state) || (_state == OrderStatus.CompletelyFilled);
                        break;
                    }
                case OrderStatus.CancelledByExchange:
                    {
                        res = IsActiveStatus(_state) || (_state == OrderStatus.NewOrder);
                        break;
                    }
                case OrderStatus.Filled:
                case OrderStatus.CompletelyFilled:
                case OrderStatus.AmendAccepted:
                case OrderStatus.AmendRejected:
                case OrderStatus.CancelRejected:
                case OrderStatus.Cancelled:
                    {
                        res = IsActiveStatus(_state);
                        break;
                    }

                default:
                    throw new ApplicationException("Unknown state " + _state.ToString());
            }

            if (res)
            {
                _state = newState;
            }

            return res;
        }
    }
}
