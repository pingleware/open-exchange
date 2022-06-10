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
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPEX.Stop
{
    class StopParameters
    {
        private double _stopPrice;

        StopParameters(double stopPrice)
        {
            _stopPrice = stopPrice;
        }

        public double StopPrice { get { return _stopPrice; } }

        public static StopParameters Parse(string commaSeparatedParameters)
        {
            if (commaSeparatedParameters == null)
            {
                throw new ApplicationException("CommaSeparatedParameters is null!");
            }
            Dictionary<string, string> parameterBag = new Dictionary<string, string>();
            string[] bits = commaSeparatedParameters.Split(new char[] { ',' });

            if (bits.Length == 0)
            {
                throw new ApplicationException("Invalid format for CommaSeparatedParameters.");
            }
            else if (bits.Length % 2 != 0)
            {
                throw new ApplicationException("Invalid format for CommaSeparatedParameters.");
            }

            for (int i = 0; i < bits.Length; i+=2)
            {
                parameterBag.Add(bits[i], bits[i + 1]);
            }

            if (!parameterBag.ContainsKey("StopPrice"))
            {
                throw new ApplicationException("CommaSeparatedParameters don't contain parameter 'StopPrice'.");
            }

            double stopPrice = 0;
            if (!Double.TryParse(parameterBag["StopPrice"].ToString(), out stopPrice))
            { 
                throw new ApplicationException("Invalid format for StopPrice.");
            }
            return new StopParameters(stopPrice);
        }

        public static bool TryParse(string commaSeparatedParameters, ref StopParameters stopParameters)
        {
            StopParameters parameters = null;
            bool success = false;

            try
            {
                parameters = Parse(commaSeparatedParameters);
                success = true;
            }
            finally
            {
                if (success)
                {
                    stopParameters = parameters;
                }
            }

            return success;
        }
    }
}
