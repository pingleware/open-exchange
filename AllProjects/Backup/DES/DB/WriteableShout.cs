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

using OPEX.Storage;
using OPEX.DES.Exchange;

namespace OPEX.DES.DB
{
    public class WriteableShout : IWriteable
    {
        private readonly string SQLFields = "SimID, Round, Move, Side, Accepted, Price, User, Instrument, DateSig";
        private readonly Shout _shout;

        public WriteableShout(Shout s)
        {
            _shout = s;
        }

        #region IWriteable Members

        public string TableName
        {
            get { return "DESShouts"; }
        }

        public string FieldList 
        {
            get { return SQLFields; }
        }

        public string Values 
        {
            get
            {
                return string.Format("{0}, {1}, {2}, '{3}', {4}, {5}, '{6}', '{7}', '{8}'",
                    _shout.TimeStamp.SimID, _shout.TimeStamp.Round, _shout.TimeStamp.Move,
                    _shout.Side.ToString(), _shout.Accepted ? 1 : 0, _shout.Price,
                    _shout.User, _shout.Instrument, DateTime.Today.ToString("yyyyMMdd"));
            }
        }              

        #endregion
    }
}
