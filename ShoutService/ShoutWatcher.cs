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

namespace OPEX.ShoutService
{
    /// <summary>
    /// Watches for a specific user's Shout-s.
    /// </summary>
    public class ShoutWatcher : IDisposable
    {
        private readonly ShoutClient _client;
        private readonly string _myUserName;
        private NewShoutEventHandler _newShout;

        internal ShoutWatcher(ShoutClient client, string myUserName)
        {
            _client = client;
            _myUserName = myUserName;
        }

        /// <summary>
        /// Gets the user whose Shouts to listen to.
        /// </summary>
        public string User { get { return _myUserName; } }

        /// <summary>
        /// Occurs when a new Shout is received.
        /// </summary>
        public event NewShoutEventHandler NewShout
        {
            add { _newShout += value; }
            remove { _newShout -= value; }
        }

        internal void ReceiveShout(Shout shout)
        {
            if (_newShout != null)
            {
                foreach (NewShoutEventHandler handler in _newShout.GetInvocationList())
                {
                    handler(this, new NewShoutEventArgs(shout));
                }
            }
        }

        #region IDisposable Members

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _client.RemoveWatcher(this);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
