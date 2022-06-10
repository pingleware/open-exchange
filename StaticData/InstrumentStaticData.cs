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
    /// Exposes static data for all the configured instruments.
    /// Retrieves the data from teh DB.
    /// </summary>
    public class InstrumentStaticData : IStaticData
    {
        private Dictionary<string, Instrument> _instruments;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.StaticData.InstrumentStaticData.
        /// </summary>
        public InstrumentStaticData()
        {
            _instruments = new Dictionary<string, Instrument>();
        }
        
        /// <summary>
        /// Gets the configured Instruments.
        /// </summary>
        public string[] Instruments
        {
            get
            {
                if (_instruments.Count == 0)
                {
                    return null;
                }

                string[] instruments = new string[_instruments.Count];
                _instruments.Keys.CopyTo(instruments, 0);
                return instruments;
            }
        }

        /// <summary>
        /// Gets a specific instrument.
        /// </summary>
        /// <param name="ric">The RIC of the Instrument to retrieve.</param>
        /// <returns>The Instrument identified by ric.</returns>
        public Instrument this[string ric]
        {
            get
            {
                Instrument instrument = null;

                if (_instruments.ContainsKey(ric))
                {
                    instrument = _instruments[ric];
                }

                return instrument;
            }
        }

        #region IStaticData Members

        /// <summary>
        /// Loads from the DB the static data for all the configured instruments. 
        /// </summary>
        public void Load()
        {
            if (!DBConnectionManager.Instance.IsConnected)
            {
                DBConnectionManager.Instance.Connect();
            }

            MySqlCommand cmd = new MySqlCommand("SELECT RIC, ExchangeName, MinQty, MaxQty, MinPrice, MaxPrice, PriceTick FROM Instrument;", DBConnectionManager.Instance.Connection);
            MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                string instrument = reader["RIC"].ToString();                
                string exchangeName = reader["ExchangeName"].ToString();
                int minQty = Int32.Parse(reader["MinQty"].ToString());
                int maxQty = Int32.Parse(reader["MaxQty"].ToString());
                double minPrice = Double.Parse(reader["MinPrice"].ToString());
                double maxPrice = Double.Parse(reader["MaxPrice"].ToString());
                double priceTick = Double.Parse(reader["PriceTick"].ToString());                

                _instruments[instrument] = new Instrument(instrument, exchangeName, minQty, maxQty, minPrice, maxPrice, priceTick);
            }

            reader.Close();
        }

        #endregion
    }
}
