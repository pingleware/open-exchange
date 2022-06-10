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

using OPEX.Configuration.Common;

namespace OPEX.Configuration.Service
{
    /// <summary>
    /// Provides configuration data.
    /// This class is abstract.
    /// </summary>
    public abstract class ConfigurationProvider : MarshalByRefObject, IConfigurationProvider
    {
        protected readonly object _root;
        protected Dictionary<string, ApplicationData> _dataStore;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.Configuration.Service.ConfigurationProvider.
        /// </summary>
        public ConfigurationProvider()
        {
            _root = new object();
            _dataStore = new Dictionary<string, ApplicationData>();
        }

        /// <summary>
        /// Loads configuration data.
        /// </summary>
        /// <returns>True if configuration data was loaded successfully.</returns>
        public abstract bool LoadConfigurationData();

        public override object InitializeLifetimeService()
        {
            // grant the object an infinite lifetime
            return null;
        }

        /// <summary>
        /// Starts the ConfigurationProvider.
        /// </summary>
        public virtual void Start() { }

        /// <summary>
        /// Stops the ConfigurationProvider.
        /// </summary>
        public virtual void Stop() { }

        #region IConfigurationProvider Members

        /// <summary>
        /// Retrieves the application configuration.
        /// </summary>
        /// <param name="applicationName">The name of the application.</param>
        /// <returns>The data attached to the application.</returns>
        public ApplicationData GetApplicationConfiguration(string applicationName)
        {
            ApplicationData data = null;

            lock (_root)
            {
                if (_dataStore.ContainsKey(applicationName))
                {
                    data = _dataStore[applicationName];
                }
            }

            return data;
        }

        #endregion
    }
}
