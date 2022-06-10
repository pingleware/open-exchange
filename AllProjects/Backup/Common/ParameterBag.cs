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

namespace OPEX.Common
{
    /// <summary>
    /// Collects parameters in a key-value fashion.
    /// Supports default values.
    /// </summary>
    [Serializable]
    public class ParameterBag
    {
        /// <summary>
        /// Represents a "value, default-value" pair.
        /// </summary>
        [Serializable]
        class ValueEntry
        {
            private string _value;
            private string _defaultValue;

            /// <summary>
            /// Initialises a new instance of the class OPEX.Common.ParameterBag.ValueEntry.
            /// </summary>
            public ValueEntry()
                : this(null, null)
            { }

            /// <summary>
            /// Initialises a new instance of the class OPEX.Common.ParameterBag.ValueEntry.
            /// </summary>
            /// <param name="value">The value of the ValueEntry.</param>
            public ValueEntry(string value)
                : this(value, null)
            { }

            /// <summary>
            /// Initialises a new instance of the class OPEX.Common.ParameterBag.ValueEntry.
            /// </summary>
            /// <param name="value">The value of the ValueEntry.</param>
            /// <param name="defaultValue">The default value of the ValueEntry.</param>
            public ValueEntry(string value, string defaultValue)
            {
                _value = value;
                _defaultValue = defaultValue;
            }

            /// <summary>
            /// Gets or sets the Value of the ValueEntry.
            /// </summary>
            public string Value { get { return (_value != null) ? _value : _defaultValue; } set { _value = value; } }

            /// <summary>
            /// Gets or sets the DefaultValue of the ValueEntry.
            /// </summary>
            public string DefaultValue { get { return _defaultValue; } set { _defaultValue = value; } }
        }

        private readonly Dictionary<string, ValueEntry> _bag;      

        /// <summary>
        /// Initialises a new instance of the OPEX.Common.ParameterBag class
        /// that contains the specified parameters.
        /// </summary>
        /// <param name="parameters">The parameters in a semi-colon separated lhs=rhs list.
        /// E.g. "param1=val1;param2=val2;param3=val3". Integers, doubles and bools are 
        /// supported value types.
        /// </param>
        public ParameterBag(string parameters)
        {
            _bag = new Dictionary<string, ValueEntry>();

            if (parameters == null)
            {
                return;
            }

            string[] bits = parameters.Split(new char[] { ';' });
            if (bits == null || bits.Length == 0)
            {
                return;
            }

            foreach (string bit in bits)
            {
                string[] sides = bit.Split(new char[] { '=' });
                if (sides == null || sides.Length != 2)
                {
                    continue;
                }
                string lhs = sides[0].Trim();
                string rhs = sides[1].Trim();
                _bag[lhs] = new ValueEntry(rhs);
            }
        }

        /// <summary>
        /// Adds a default value for a parameter.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="defaultValue">The default value to assign to the parameter.</param>
        public void AddDefaultValue(string parameterName, string defaultValue)
        {            
            if (_bag.ContainsKey(parameterName))
            {
                _bag[parameterName].DefaultValue = defaultValue;
            }
            else
            {
                _bag[parameterName] = new ValueEntry(null, defaultValue);
            }
        }

        /// <summary>
        /// Retrieves the value of the a parameter, 
        /// converting it to the specified type.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="type">The type of the parameter.</param>
        /// <returns>The value of the parameter, or its default 
        /// value if it doesn't have a value.
        /// </returns>
        public object GetValue(string parameterName, Type type)
        {
            ValueEntry ve = _bag[parameterName];
            if (type == typeof(int))
            {
                return int.Parse(ve.Value);   
            }
            else if (type == typeof(double))
            {
                return double.Parse(ve.Value);   
            }
            else if (type == typeof(bool))
            {
                return bool.Parse(ve.Value);   
            }
            else
            {
                return ve.Value;   
            }         
        }        
    }
}
