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
using System.Collections;

using OPEX.Common;
using OPEX.Configuration.Client;

namespace OPEX.OM.Common
{   
    /// <summary>
    /// Exposes methods to create IncomingOrder-s and
    /// OutgoingOrder-s, and holds all the created
    /// Order-s.
    /// </summary>
    public class OrderFactory
    {
        private static readonly string _OMClientName;
        private static readonly string GeneratorSettingName = "IDGeneratorType";
        private static readonly IUIDGenerator OutgoingOrderIDGenerator;
        private static readonly IncrementalIDGenerator IncomingOrderIDGenerator;

        private static IOrderSender FactoryOrderSender;
        private static IDictionary FactoryOutgoingOrdersByClientOrderID;
        private static IDictionary FactoryOutgoingOrdersByOrderID;
        private static IDictionary FactoryIncomingOrders;
        public static long _pingID = 0;
        private static object Root;

        static OrderFactory() 
        {
            Root = new object();

            string idGeneratorType = ConfigurationClient.Instance.GetConfigSetting(GeneratorSettingName, "Sequential");

            if (idGeneratorType.Equals("Random"))
            {
                OutgoingOrderIDGenerator = RandomIDGenerator.Instance;
            }
            else if (idGeneratorType.Equals("Sequential"))
            {
                OutgoingOrderIDGenerator = new IncrementalIDGenerator();
            }
            else
            {
                throw new ApplicationException("Orderfactory - Unknown IDGenerator type. It can only be Random or Sequential. Check configuration file.");
            }

            _OMClientName = ConfigurationClient.Instance.GetConfigSetting("ApplicationName", null);

            IncomingOrderIDGenerator = new IncrementalIDGenerator();

            FactoryIncomingOrders = Hashtable.Synchronized(new Hashtable());
            FactoryOutgoingOrdersByClientOrderID = Hashtable.Synchronized(new Hashtable());
            FactoryOutgoingOrdersByOrderID = Hashtable.Synchronized(new Hashtable());
        }

        /// <summary>
        /// Gets the name of the Order Manager client.
        /// </summary>
        public static string OMClientName { get { return _OMClientName; } }

        /// <summary>
        /// Sets the OrderSender.
        /// </summary>
        public static IOrderSender OrderSender
        {
            set
            {
                lock (Root)
                {
                    if (FactoryOrderSender != null)
                    {
                        throw new ApplicationException("Cannot setup OrderSender more than once!");
                    }

                    FactoryOrderSender = value;
                }
            }
        }

        /// <summary>
        /// Creates an IncomingOrder.
        /// </summary>
        /// <param name="order">The Order to clone the IncomingOrder from.</param>
        /// <returns>The new IncomingOrder.</returns>
        public static IncomingOrder CreateIncomingOrder(Order order)
        {
            IncomingOrder o;

            if (order.OrderID != 0)
            {
                // the order received was sent by the OrderManager,
                // therefore there's no need to generate an ID
                o = new IncomingOrder(order);
            }
            else
            {
                // the order received was sent by something other than 
                // the OrderManager, therefore it doesn't have an ID ->
                // generate one
                o = new IncomingOrder(order, IncomingOrderIDGenerator.NextID());
            }

            FactoryIncomingOrders[o.OrderID] = o;

            return o;
        }

        /// <summary>
        /// Creates a new OutgoingOrder.
        /// </summary>
        /// <returns>A new OutgoingOrder.</returns>
        public static OutgoingOrder CreateOutgoingOrder()
        {
            return CreateOutgoingOrder(_OMClientName);
        }

        /// <summary>
        /// Creates a new OutgoingOrder, presetting its Origin
        /// and setting its User to the specified userName.
        /// </summary>
        /// <param name="userName">The User to assign to the OutgoingOrder.</param>
        /// <returns>The new OutgoingOrder.</returns>
        public static OutgoingOrder CreateOutgoingOrder(string userName)
        {
            long id;
            for (id = OutgoingOrderIDGenerator.NextID(); FactoryOutgoingOrdersByClientOrderID.Contains(id); id = OutgoingOrderIDGenerator.NextID()) ;
            OutgoingOrder o = new OutgoingOrder(FactoryOrderSender, id);
            o.Origin = _OMClientName;
            o.User = userName;

            FactoryOutgoingOrdersByClientOrderID[o.ClientOrderID] = o;

            return o;
        }

        /// <summary>
        /// Creates a new 'ping' OutgoingOrder (debug only).
        /// </summary>
        /// <returns>The new 'ping' OutgoingOrder (debug only).</returns>
        public static OutgoingOrder CreateOutgoingPingOrder()
        {
            _pingID--;
            OutgoingOrder o = new OutgoingOrder(FactoryOrderSender, _pingID);
            o.Origin = _OMClientName;
            o.Destination = "OPEX";
            return o;
        }

        /// <summary>
        /// Creates a new OutgoingOrder, cloning the content of a specific Order.
        /// </summary>
        /// <param name="order">The Order to clone.</param>
        /// <returns>The new OutgoingOrder.</returns>
        public static OutgoingOrder CreateOutgoingOrder(Order order)
        {
            long id;
            for (id = OutgoingOrderIDGenerator.NextID(); FactoryOutgoingOrdersByClientOrderID.Contains(id); id = OutgoingOrderIDGenerator.NextID()) ;
            OutgoingOrder o = new OutgoingOrder(FactoryOrderSender, id, order);
            o.Origin = _OMClientName;

            FactoryOutgoingOrdersByClientOrderID[o.ClientOrderID] = o;

            return o;
        }

        /// <summary>
        /// Updates an existing OutgoingOrder with the data of a specific Order.
        /// </summary>
        /// <param name="order">The OutgoingOrder to update.</param>
        /// <param name="newOrder">The Order to clone.</param>
        public static void UpdateOutgoingOrder(OutgoingOrder order, Order newOrder)
        {
            order.PerformUpdate(newOrder);
        }

        /// <summary>
        /// Create an OutgoingOrder linked to a specific IncomingOrder.
        /// Only used for advanced order routing.
        /// </summary>
        /// <param name="incomingOrder">The parent order.</param>
        /// <returns>The new OutgoingOrder.</returns>
        internal static OutgoingOrder CreateRelatedOutgoingOrder(IncomingOrder incomingOrder)
        {
            OutgoingOrder outgoingOrder = CreateOutgoingOrder();

            outgoingOrder.ParentOrderID = incomingOrder.OrderID;
            outgoingOrder.Currency = incomingOrder.Currency;
            outgoingOrder.Origin = incomingOrder.Destination;
            outgoingOrder.Instrument = incomingOrder.Instrument;
            outgoingOrder.LimitPrice = incomingOrder.LimitPrice;
            outgoingOrder.Price = incomingOrder.Price;
            outgoingOrder.Quantity = incomingOrder.Quantity;
            outgoingOrder.Side = incomingOrder.Side;
            outgoingOrder.Type = incomingOrder.Type;            
            outgoingOrder.ParentOrder = incomingOrder;

            return outgoingOrder;
        }

        /// <summary>
        /// Create an OutgoingOrder linked to a specific IncomingOrder.
        /// Typically used by the OrderManager.
        /// </summary>
        /// <param name="incomingOrder">The parent order.</param>
        /// <returns>The new OutgoingOrder.</returns>
        internal static OutgoingOrder CreateThroughOutgoingOrder(IncomingOrder incomingOrder)
        {
            OutgoingOrder o = new OutgoingOrder(FactoryOrderSender, incomingOrder);
            o.ParentOrder = incomingOrder;
            o.ParentOrderID = incomingOrder.ParentOrderID;
            o.Origin = incomingOrder.Origin;
            FactoryOutgoingOrdersByClientOrderID[o.ClientOrderID] = o;
            FactoryOutgoingOrdersByOrderID[o.OrderID] = o;
            return o;
        }

        /// <summary>
        /// Gets the OutgoingOrders created so far, in n IDictionary indexed by OrderID.
        /// </summary>
        public static IDictionary OutgoingOrderByOrderID { get { return FactoryOutgoingOrdersByOrderID; } }

        /// <summary>
        /// Gets the OutgoingOrders created so far, in n IDictionary indexed by ClientOrderID.
        /// </summary>
        public static IDictionary OutgoingOrders { get { return FactoryOutgoingOrdersByClientOrderID; } }

        /// <summary>
        /// Gets the OutgoingOrders created so far, in n IDictionary indexed by OrderID.
        /// </summary>
        public static IDictionary IncomingOrders { get { return FactoryIncomingOrders; } }
    }
}

