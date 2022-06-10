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
using System.Threading;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using OPEX.Common;
using OPEX.Storage;

using OPEX.DWEAS.Service;

namespace OPEX.DWEAS
{
    class DWEASHost : MainLogger
    {
        private readonly DWEService _dweService;

        public DWEASHost()
            : base("DWEAS")
        {
            _dweService = new DWEService();
        }

        public void Run()
        {
            try
            {
                Console.WriteLine("Days Without End (DWE) Assignment Host.");
                Console.WriteLine();
                Console.WriteLine("Loading schedule from DB...");
                bool b = _dweService.Start();
                if (!b)
                {
                    Console.WriteLine("An error occurred while loading the schedule from DB. Please check the log.");
                    return;
                }

                int c = 0;

                do
                {
                    c = MainMenu();
                    switch (c)
                    {
                        case 0:
                            ReloadSchedule();
                            break;
                        case 1:
                            PrintSchedules();
                            break;
                        case 2:
                            PlaySimulation();
                            break;
                        case 3:
                        default:
                            break;
                    }                    
                }
                while (c != 3);                
                
                _dweService.Stop();
            }
            catch (Exception ex)
            {
                Trace(LogLevel.Error, "DWEASHost.Run. Exception while starting DWEService: {0} {1}", 
                    ex.Message, ex.StackTrace);
                return;
            }           
        }

        private void ReloadSchedule()
        {
            _dweService.ForceReload();
            Console.Write("Schedule reloaded.");
        }

        private void PlaySimulation()
        {
            int sid = 0;
            do
            {
                Console.Write("Enter the schedule ID you want to play: ");
            } 
            while (!Int32.TryParse(Console.ReadLine(), out sid));

            if (!_dweService.ScheduleIDs.Contains(sid))
            {
                Console.WriteLine("Schedule ID {0} doesn't exist in the DB.", sid);
                return;
            }

            Console.WriteLine();
            _dweService.Play(sid);
            Pause("Simulation started. Press ENTER to stop");
            _dweService.Pause();
        }

        private void PrintSchedules()
        {
            Console.WriteLine("The following schedule IDs are available:");
            Console.WriteLine();

            foreach (int sid in _dweService.ScheduleIDs)
            {
                Console.WriteLine("\t" + sid);
            }

            Console.WriteLine();
        }

        private int MainMenu()
        {
            int c = 0;

            do
            {
                Console.WriteLine("Main menu");
                Console.WriteLine("========================================");
                Console.WriteLine("0. RELOAD schedules");
                Console.WriteLine("1. Print available schedules");
                Console.WriteLine("2. Play a schedule");
                Console.WriteLine("3. Quit");
                Console.WriteLine();
                Console.Write("Enter your choice [1-3]: ");
            } 
            while (!Int32.TryParse(Console.ReadLine(), out c) || !(c >= 0 && c <= 3));

            return c;
        }

        private void Pause(string message)
        {
            Console.WriteLine(message);
            Console.ReadLine();
        }
    }
}


