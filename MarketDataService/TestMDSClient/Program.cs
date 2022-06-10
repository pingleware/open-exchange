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

using OPEX.MDS.Common;
using OPEX.MDS.Client;
using OPEX.Common;

namespace OPEX.TestMDSClient
{
    class Program
    {
        private readonly string DataSource;
        
        private MDSClient MdsClient;

        public Program()
        {
            DataSource = Configuration.GetConfigSetting("MDSDataSource", "OPEX");
        }

        static void Main(string[] args)
        {      
            Program p = new Program();
            p.Run();
        }

        void Run()
        {
            try
            {
                MdsClient = new MDSClient(DataSource);
                MdsClient.Subscribe("VOD.L");
                MdsClient.DepthChanged += new DepthChangedEventHandler(MdsClient_DepthChanged);
                MdsClient.NewTrade += new NewTradeEventHandler(MdsClient_NewTrade);
                MdsClient.DownloadFinished += new DownloadFinishedEventHandler(MdsClient_DownloadFinished);
                MdsClient.Start();

                string s = null;
                do
                {
                    Console.WriteLine("Press q to STOP the server.");
                    Console.WriteLine("Press h to request a SNAPSHOT");
                    Console.WriteLine("Press s to request a STATUS update");
                    s = Console.ReadLine();

                    switch (s)
                    {
                        case "h":
                            MdsClient.DownloadSnapshot("VOD.L");
                            break;

                        case "s":
                            break;

                        default:
                            break;
                    }
                }
                while (!s.Equals("q"));
                
                MdsClient.Stop();
            }
            catch (Exception ex)
            {
                Pause("Simulation terminated ABnormally: " + ex.Message + " " + ex.StackTrace);
            }
        }

        void MdsClient_DownloadFinished(MDSClient sender, DownloadFinishedEventArgs args)
        {
            Console.WriteLine("DownloadFinished for instrument {0}", args.Instrument);

            AggregatedDepthSnapshot s = MdsClient.Cache.GetSnapshot(args.Instrument);
            if (s != null)
            {
                Console.WriteLine(s.ToString());
            }
        }

        void MdsClient_NewTrade(MDSClient sender, NewTradeEventArgs args)
        {
            Console.WriteLine("NewTrade received for instrument {0}", args.Instrument);

            LastTradeUpdateMessage msg = MdsClient.Cache.GetLastTrade(args.Instrument);
            if (msg != null)
            {
                Console.WriteLine("Last trade: {2} {0} @ {1} {3}", msg.Size, msg.Price, msg.InstrumentName, msg.TimeStamp.ToString());
            }
        }

        void MdsClient_DepthChanged(MDSClient sender, DepthChangedEventArgs args)
        {
            Console.WriteLine("Market data update received for instrument {0}", args.Instrument);

            AggregatedDepthSnapshot s = MdsClient.Cache.GetSnapshot(args.Instrument);
            if (s != null)
            {
                Console.WriteLine(s.ToString());
            }
        }

        void Pause(string s)
        {
            Console.WriteLine(s);
            Console.WriteLine("Press ENTER to continue...");
            Console.ReadLine();
        }
    }
}
