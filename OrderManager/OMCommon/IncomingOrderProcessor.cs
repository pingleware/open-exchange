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
    /// Validates order requests, and saves new versions
    /// of Order-s.
    /// </summary>
    public class IncomingOrderProcessor : IOrderProcessor
    {
        #region IOrderProcessor Members

        public virtual bool ValidateNewOrderRequest(Order order, ref string errorMessage)
        {
            return ValidateOrder(order, ref errorMessage);
        }

        public virtual bool ValidateCancelOrderRequest(Order order, ref string errorMessage)
        {
            return ValidateOrder(order, ref errorMessage);
        }

        public virtual bool ValidateAmendOrderRequest(Order oldOrder, Order newOrder, ref string errorMessage)
        {
            bool res = false;
            string m = null;
            res = ValidateOrderCommonFields(oldOrder, ref m) && ValidateOrderCommonFields(newOrder, ref m);

            if (res)
            {
                if (!OrderStateMachine.IsActiveStatus(oldOrder.Status))
                {
                    m = string.Format("Cannot amend order while it's in closed status ({0})", oldOrder.Status);
                }
                else if (!newOrder.Instrument.Equals(oldOrder.Instrument))
                {
                    m = "Cannot amend instrument";
                }
                else if (!newOrder.Currency.Equals(oldOrder.Currency))
                {
                    m = "Cannot amend currency";
                }
                else if (!newOrder.Side.Equals(oldOrder.Side))
                {
                    m = "Cannot amend side";
                }
                else if (newOrder.Status != (oldOrder.Status))
                {
                    m = "Cannot amend status";
                }
                else if (newOrder.Type != (oldOrder.Type))
                {
                    m = "Cannot amend type";
                }
                else if (newOrder.Quantity < oldOrder.QuantityFilled)
                {
                    m = "Amended quantity must be >= QuantityFilled";
                }
                else
                {
                    res = true;
                }
            }

            if (!res)
            {
                errorMessage = m;
            }
            return res;
        }

        protected bool ValidateOrder(Order order, ref string errorMessage)
        {          
            bool res = false;
            string m = null;
            res = ValidateOrderCommonFields(order, ref errorMessage);

            if (res)
            {
                if (order.QuantityFilled != 0)
                {
                    m = "Invalid QuantityFilled.";
                }
                else if (order.LastQuantityFilled != 0)
                {
                    m = "Invalid LastQuantityFilled.";
                }
                else if (order.LastPriceFilled != 0)
                {
                    m = "Invalid LastPriceFilled.";
                }
                else if (order.AveragePriceFilled != 0)
                {
                    m = "Invalid AveragePriceFilled.";
                }
                else
                {
                    res = true;
                }
            }

            if (!res)
            {
                errorMessage = m;
            }
            return res;
        }

        protected bool ValidateOrderCommonFields(Order order, ref string errorMessage)
        {
            string m = null;
            bool res = false;

            if (order == null)
            {
                m = "Null order.";
            }
            else if (order.ClientOrderID <= 0)
            {
                m = "Invalid ClientOrderID.";
            }
            else if (order.Instrument == null || order.Instrument.Length == 0)
            {
                m = "Null Instrument.";
            }
            else if (order.Quantity < 1)
            {
                m = "Invalid quantity.";
            }
            else if ((order.Type == OrderType.Limit) && (order.Price <= 0))
            {
                m = "Invalid price.";
            }
            else if (order.Currency == null || order.Currency.Length == 0)
            {
                m = "Null currency.";
            }
            else if (order.Origin == null || order.Origin.Length == 0)
            {
                m = "Null Origin.";
            }
            else if (order.Destination == null || order.Destination.Length == 0)
            {
                m = "Null Destination.";
            }            
            else
            {
                res = true;
            }

            if (!res)
            {
                errorMessage = m;
            }
            return res;
        }

        public virtual bool Save(Order order, ref string errorMessage)
        {
            return true;
        }

        #endregion
    }
}
