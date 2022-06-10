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

namespace OPEX.Common
{
    /// <summary>
    /// Represents a data structure that supports insertion
    /// of new items, and extraction of the item with highest
    /// priority (lower numbers indicate higher priority).
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    public class PriorityQueue<T>
    {
        private readonly object _root = new object();
        private readonly SortedDictionary<int, Queue<T>> _header;
        private int _count = 0;

        /// <summary>
        /// Initialises a new instance of the OPEX.Common.PriorityQueue class that is empty.
        /// </summary>
        public PriorityQueue()
        {
            _header = new SortedDictionary<int, Queue<T>>();
        }

        /// <summary>
        /// Gets the number of items in the queue.
        /// </summary>
        public int Count { get { return _count; } }

        /// <summary>
        /// Enqueues a new item.
        /// </summary>
        /// <param name="priority">The priority of the item.</param>
        /// <param name="item">The item to enqueue.</param>
        /// <returns>The updated queue.</returns>
        public PriorityQueue<T> Enqueue(int priority, T item)
        {
            lock (_root)
            {
                if (!_header.ContainsKey(priority))
                {
                    _header[priority] = new Queue<T>();
                }
                Queue<T> q = _header[priority];
                q.Enqueue(item);
                _count++;
            }
            return this;
        }

        /// <summary>
        /// Extracts the highest priority item from the queue.
        /// </summary>
        /// <returns>The extracted item.</returns>
        public T Dequeue()
        {
            T result = default(T);

            lock (_root)
            {
                foreach (int priority in _header.Keys)
                {
                    Queue<T> q = _header[priority];
                    if (q.Count == 0)
                    {
                        continue;
                    }
                    result = q.Dequeue();
                    _count--;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Clears the queue.
        /// </summary>
        public void Clear()
        {
            lock (_root)
            {
                _header.Clear();
            }
        }
    }
}
