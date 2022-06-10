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

namespace OPEX.DES
{
    public class TimeManager
    {
        public static readonly int MovesPerRound = 300;

        private static int _currentMove;
        private static int _currentRound;
        private static long _currentSimID;        

        static TimeManager()
        {
            _currentMove = 0;
            _currentRound = 0;
            byte[] gb = Guid.NewGuid().ToByteArray();
            Random r = new Random(BitConverter.ToInt32(gb, 0));

            _currentSimID = r.Next(Int32.MaxValue / 2);
            _currentSimID += r.Next(1, 9) * 10000000000;
        }

        static long NextSimID()
        {
            return _currentSimID + 1;
        }

        public static void Reset() { _currentMove = 0; }
        public static void NextMove() { _currentMove = (_currentMove + 1) % MovesPerRound; if (_currentMove == 0) ++_currentRound; }
        public static void NextRound() { ++_currentRound; _currentMove = 0; }
        public static void NextSimulation() { _currentMove = 0; _currentRound = 0; _currentSimID = NextSimID(); }
        public static TimeStamp CurrentTimeStamp { get { return new TimeStamp(_currentSimID, _currentRound, _currentMove); } }
    }

    public class TimeStamp : IComparable
    {
        private readonly int _round;
        private readonly int _move;
        private readonly long _simID;

        public TimeStamp(long simID, int round, int move)
        {
            _simID = simID;
            _move = move;
            _round = round;
        }

        public static TimeStamp MinValue { get { return new TimeStamp(0, 0, 0); } }

        public int Round { get { return _round; } }
        public int Move { get { return _move; } }
        public long SimID { get { return _simID; } }
        public int TotalMoves { get { return _round * TimeManager.MovesPerRound + _move; } }

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}", _simID, _round, _move);
        }

        public long ToLong() { return _simID + (TimeManager.MovesPerRound * _round) + _move; }

        public static bool operator <(TimeStamp t1, TimeStamp t2) { return t1.CompareTo(t2) < 0; }
        public static bool operator <=(TimeStamp t1, TimeStamp t2) { return t1.CompareTo(t2) <= 0; }
        public static bool operator >(TimeStamp t1, TimeStamp t2) { return t1.CompareTo(t2) > 0; }
        public static bool operator >=(TimeStamp t1, TimeStamp t2) { return t1.CompareTo(t2) >= 0; }        

        /// <summary>
        /// Subtracts two TimeStamps
        /// </summary>        
        /// <returns>The number of moves between t1 and t2 (positive, if t1 > t2)</returns>
        public int Subtract(TimeStamp t2)
        {
            if (_simID != t2._simID)
            {
                throw new ArgumentException("TimeStamp.Subtract. Operands must have the same simID");
            }

            return this.TotalMoves - t2.TotalMoves;
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (!(obj is TimeStamp))
            {
                throw new ArgumentException(
                   "A TimeStamp object is required for comparison.");
            }

            TimeStamp t2 = obj as TimeStamp;
            if (t2._simID != this._simID)
            {
                return this._simID.CompareTo(t2._simID);
            }
            if (t2._round != this._round)
            {
                return this._round.CompareTo(t2._round);
            }
            return _move.CompareTo(t2._move);
        }

        #endregion
    }
}
