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

namespace OPEX.StaticData
{
    /// <summary>
    /// Represents static data of a financial product, or instrument.
    /// </summary>
    public class Instrument
    {
        private string _ric;
        private string _exchangeName;
        private int _minQty;
        private int _maxQty;
        private double _minPrice;
        private double _maxPrice;
        private double _priceTick;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.StaticData.Instrument.
        /// </summary>
        /// <param name="ric"> The unique identifier of the security.</param>
        /// <param name="exchangeName">The name of the exchange on which the security is listed.</param>
        /// <param name="minQty">The minimum quantity of the security that must be traded in one order.</param>
        /// <param name="maxQty">The maximum quantity of the security that can be traded in one order.</param>
        /// <param name="minPrice">The minimum price at which the security can be traded.</param>
        /// <param name="maxPrice">The maximum price at which the security can be traded.</param>
        /// <param name="priceTick">The minimum price increment allowed.</param>
        public Instrument(string ric, string exchangeName, int minQty, int maxQty, double minPrice, double maxPrice, double priceTick)
        {
            _ric = ric;
            _exchangeName = exchangeName;
            _minQty = minQty;
            _maxQty = maxQty;
            _minPrice = minPrice;
            _maxPrice = maxPrice;
            _priceTick = priceTick;
        }

        /// <summary>
        /// The minimum price at which the security can be traded.
        /// </summary>
        public double MinPrice
        {
            get { return _minPrice; }
        }

        /// <summary>
        /// The maximum price at which the security can be traded.
        /// </summary>
        public double MaxPrice
        {
            get { return _maxPrice; }
        }

        /// <summary>
        /// The minimum price increment allowed.
        /// </summary>
        public double PriceTick
        {
            get { return _priceTick; }
        }

        /// <summary>
        /// The maximum quantity of the security that
        /// can be traded in one order.
        /// </summary>
        public int MaxQty
        {
            get { return _maxQty; }
        }

        /// <summary>
        /// The minimum quantity of the security that must
        /// be traded in one order.
        /// </summary>
        public int MinQty
        {
            get { return _minQty; }
        }

        /// <summary>
        /// The name of the exchange on which the security
        /// is listed.
        /// </summary>
        public string ExchangeName
        {
            get { return _exchangeName; }
        }

        /// <summary>
        /// The unique identifier of the security.
        /// </summary>
        public string Ric
        {
            get { return _ric; }
        }

        /// <summary>
        /// Formats the information contained in this object into a string.
        /// </summary>
        public override string ToString()
        {
            return string.Format("Ric {0} MinPrice {1} MaxPrice {2} PriceTick {3} MinQty {4} MaxQty {5} ExchangeName {6}", 
                _ric, _minPrice, _maxPrice, _priceTick, _minQty, _maxQty, _exchangeName);
        }
    }
}
