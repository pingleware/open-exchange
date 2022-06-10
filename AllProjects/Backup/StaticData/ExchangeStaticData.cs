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

using MySql.Data;
using MySql.Data.MySqlClient;

using OPEX.Storage;

namespace OPEX.StaticData
{
    /// <summary>
    /// Exposes static data for all the configured exchanges.
    /// Retrieves the data from teh DB.
    /// </summary>
    public class ExchangeStaticData : IStaticData
    {
        private Dictionary<string, TradingDay> _tradingDays;
        private Dictionary<string, string> _extensions;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.StaticData.ExchangeStaticData.
        /// </summary>
        public ExchangeStaticData()
        {
            _tradingDays = new Dictionary<string, TradingDay>();
            _extensions = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the symbol extension associated to the given exchange.
        /// </summary>
        /// <param name="exchangeName">The exchange name to retrieve the extension for.</param>
        /// <returns>The extension of the given exchange, if it exists; null otherwise.</returns>
        public string Extension(string exchangeName)
        {
            string s = null;

            if (_extensions.ContainsKey(exchangeName))
            {
                s = _extensions[exchangeName];
            }

            return s;
        }

        /// <summary>
        /// Gets hte TradingDay of the given exchange.
        /// </summary>
        /// <param name="exchangeName">The exchange name to retrieve the TradeingDay for.</param>
        /// <returns>The TradingDay of the given exchange, if it existsl null otherwise.</returns>
        public TradingDay TradingDay(string exchangeName)
        {
            TradingDay d = null;

            if (_tradingDays.ContainsKey(exchangeName))
            {
                d = _tradingDays[exchangeName];
            }

            return d;
        }        
        
        #region IStaticData Members

        /// <summary>
        /// Loads from the DB static data for all the configured exchanges.
        /// </summary>
        public void Load()
        {
            if (!DBConnectionManager.Instance.IsConnected)
            {
                DBConnectionManager.Instance.Connect();
            }

            MySqlCommand cmd = new MySqlCommand("SELECT ExchangeName, StartTime, EndTime, Phase FROM ExchangePhases;", DBConnectionManager.Instance.Connection);
            MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                string exchangeName = reader["ExchangeName"].ToString();
                TradingDay d = null;

                if (_tradingDays.ContainsKey(exchangeName))
                {
                    d = _tradingDays[exchangeName];
                }
                else
                {
                    d = new TradingDay();
                    _tradingDays.Add(exchangeName, d);
                }

                d.AddTradingPeriod(new TradingPeriod(reader["StartTime"].ToString(), reader["EndTime"].ToString(), reader["Phase"].ToString()));
            }

            reader.Close();

            cmd.CommandText = "SELECT ExchangeName, Extension from Exchange;";
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                _extensions[reader["ExchangeName"].ToString()] = reader["Extension"].ToString();
            }

            reader.Close();
        }

        #endregion
    }
}
