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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPEX.DES.AA
{
    public class Cache<T> : IEnumerable, IEnumerator
    {
        private readonly object _root = new object();
        private readonly int _size;
        private readonly Array _data;
        private int _position;
        private int _count;
        private int _enumPos;

        public Cache(int size)
        {
            if (size < 1)
            {
                throw new ArgumentException("Cache.ctor: size must be > 0");
            }
            _size = size;
            _data = new T[_size];
            _enumPos = -1;

            Initialise();
        }

        public int Size { get { return _size; } }
        public int Count { get { return _count; } }

        public void Add(T item)
        {
            lock (_root)
            {
                _data.SetValue(item, _position);
                _position = (_position + 1) % _size;
                if (_count < _size)
                {
                    ++_count;
                }
            }
        }

        public void Clear()
        {
            lock (_root)
            {
                Initialise();
            }
        }

        private void Initialise()
        {
            _position = 0;
            _count = 0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (double d in this)
            {
                sb.AppendFormat("{0} ", d);
            }

            return sb.ToString();
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            Reset();
            return this;
        }

        #endregion

        #region IEnumerator Members

        public object Current
        {
            get
            {
                int actualPosition = (_size + _position - 1 - _enumPos) % _size;
                return _data.GetValue(actualPosition);
            }
        }

        public bool MoveNext()
        {
            _enumPos++;

            if (_enumPos < _count)
            {
                return true;
            }

            Reset();
            return false;
        }

        public void Reset()
        {
            _enumPos = -1;
        }

        #endregion
    }
}
