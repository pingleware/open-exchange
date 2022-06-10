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
using System.Text;
using System.Collections.Generic;

using OPEX.Common;

namespace OPEX.Storage
{
    /// <summary>
    /// Performes buffered write to the DB, handling one queue per
    /// each DB table.
    /// </summary>
    public partial class DBWriter    
    {        
        private readonly int MaxNumValues = 10;
        private readonly Logger _logger;
        private readonly QueryManager _queryManager;
        private readonly Dictionary<string, CommandBuilder> _entries;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.Storage.DBWriter.
        /// </summary>
        public DBWriter()
        {
            _logger = new Logger("DBWriter");
            _queryManager = DBConnectionManager.Instance.QueryManager;
            _entries = new Dictionary<string, CommandBuilder>();
        }        

        /// <summary>
        /// Pushes an IWriteable object to a DB write queue.
        /// </summary>
        /// <param name="w">The IWriteable to write to the DB.</param>
        public void Write(IWriteable w)
        {
            if (!_queryManager.Running)
            {
                _logger.Trace(LogLevel.Critical, "Write. Cannot write to DB: QueryManager not running!");
                return;
            }

            InnerWrite(w);            
        }

        /// <summary>
        /// Flushes all the DB write queues.
        /// </summary>
        public void Flush()
        {
            foreach (string table in _entries.Keys)
            {
                CommandBuilder cb = _entries[table];
                if (cb.NumValues > 0)
                {
                    cb.Finalise();
                    _queryManager.RunSQLCommand(cb.SQLCommand);                    
                }
            }
        }

        private void InnerWrite(IWriteable w)
        {
            CommandBuilder cb = null;

            if (!_entries.ContainsKey(w.TableName))
            {
                cb = new CommandBuilder(w.TableName);
                cb.Init(w);
                _entries[w.TableName] = cb;
            }
            else
            {
                cb = _entries[w.TableName];        
            }

            if (cb.NumValues >= MaxNumValues)
            {
                cb.Finalise();
                _queryManager.RunSQLCommand(cb.SQLCommand);
                cb.Init(w);
            }

            cb.AddValues(w.Values);
        }      
    }
}
