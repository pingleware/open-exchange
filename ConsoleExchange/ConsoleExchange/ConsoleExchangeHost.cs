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
using OPEX.ME;
using OPEX.OM.Common;
using OPEX.MDS;
using OPEX.MDS.Common;
using OPEX.TDS.Server;
using OPEX.StaticData;
using OPEX.Storage;
using OPEX.Configuration.Client;
using OPEX.ShoutService;

namespace OPEX.ConsoleExchange
{
    /// <summary>
    /// Hosts a BusinessDomain.
    /// </summary>
    class ConsoleExchangeHost : MainLogger
    {
        private enum ExchangeRunningStatus
        {
            Init,
            Open,
            ClosedByUser,
            ClosedByTimeout,
            Terminated
        }

        private readonly System.Speech.Synthesis.SpeechSynthesizer Synth = new System.Speech.Synthesis.SpeechSynthesizer();
        private readonly ManualResetEvent _quitSignal = new ManualResetEvent(false);
        private readonly AutoResetEvent _userSignal = new AutoResetEvent(false);
        private readonly string _exchangeName;
        private readonly IncomingOrderDuplexChannel _incomingOrderChannel;
        private readonly bool _allowCancellations;
        private readonly bool _allowAmendments;
        private readonly bool _speak;
        private readonly Thread _tradingPhasesThread;

        private MarketDataService MDService;
        private BusinessDomain _domain;
        private bool _running;        
        private ExchangeRunningStatus _exchangeRunningStatus = ExchangeRunningStatus.Init;

        /// <summary>
        /// Initialises a new instance of the class
        /// OPEX.ConsoleExchange.ConsoleExchangeHost.
        /// </summary>
        public ConsoleExchangeHost()
            : base("ConsoleExchange")
        {
            _exchangeName = ConfigurationClient.Instance.GetConfigSetting("ApplicationName", null);
            if (_exchangeName == null)
            {
                throw new ApplicationException("Configuration setting OMClientName is null. Cannot create IncomingOrderDuplexChannel.");
            }

            _allowCancellations = bool.Parse(ConfigurationClient.Instance.GetConfigSetting("AllowCancellations", "true"));
            _allowAmendments = bool.Parse(ConfigurationClient.Instance.GetConfigSetting("AllowAmendments", "true"));
            _speak = bool.Parse(ConfigurationClient.Instance.GetConfigSetting("SpeakSessionState", "false"));

            _incomingOrderChannel =
                new IncomingOrderDuplexChannel(_exchangeName, new DummyOrderWriter());
            bool purge = bool.Parse(ConfigurationClient.Instance.GetConfigSetting("PurgeQueuesOnStartup", "true"));
            if (purge)
            {
                _incomingOrderChannel.Purge();
            }
            
            _tradingPhasesThread = new Thread(EnhanchedTradingPhasesThread);
            _running = false;
        }

        /// <summary>
        /// Runs the ConsoleExchangeHost.
        /// </summary>
        public void Run()
        {
            try
            {
                StaticDataManager.Instance.Load();
                StartServices();
            }
            catch (Exception ex)
            {
                Trace(LogLevel.Critical, "Exception while starting MarketDataService: {0}", ex.Message);
                return;
            }

            try
            {
                bool open = true;
                StartExchange();

                int c = 0;
                do
                {
                    while (!Int32.TryParse(Console.ReadLine(), out c) || !(c >= 0 && c <= 1)) ;
                    switch (c)
                    {                        
                        case 0:                            
                            break;

                        case 1:
                            _userSignal.Set();                            
                            break;

                        default:
                            break;
                    }
                }
                while (c != 0);
            }
            catch (Exception e)
            {
                Trace(LogLevel.Critical, "Exception in ConsoleExchangeHost.Run(): {0}", e.Message);
            }
            finally 
            {
                try
                {                    
                    if (_running)
                    {
                        DBConnectionManager.Instance.Disconnect();
                        StopExchange();
                    }
                    StopServices();                    
                }
                catch (Exception ex)
                {
                    Trace(LogLevel.Critical, "Exception while stopping Exchange: {0}", ex.Message);
                }
            }
        }

        private void PrintMainMenu(bool open)
        {
            string openClose = open ? "CLOSE" : "OPEN";

            Console.WriteLine("\nMain menu");
            Console.WriteLine("========================================");
            Console.WriteLine("0. QUIT exchange");
            Console.WriteLine("1. {0} market", openClose);
            Console.WriteLine();
            Console.Write("Enter your choice [0-1]: ");
        }

        private int MainMenu(bool open)
        {
            int c = 0;            

            do
            {
                PrintMainMenu(open);
            }
            while (!Int32.TryParse(Console.ReadLine(), out c) || !(c >= 0 && c <= 1));

            return c;
        }

        private void StartServices()
        {
            MDService = MarketDataService.Create(ConfigurationClient.Instance.GetConfigSetting("MDSDataSource", "OPEX"));
            MDService.Start();


            string[] loadedInstruments = StaticDataManager.Instance.InstrumentStaticData.Instruments;
            _domain = new BusinessDomain("OPEX Domain", loadedInstruments);

            TradeDataServer.Instance.Start();
            ShoutServer.Instance.Start();            
        }

        private void StopServices()
        {
            MDService.Stop();
            TradeDataServer.Instance.Stop();
            ShoutServer.Instance.Stop();
        }        

        private void StopExchange()
        {
            _incomingOrderChannel.OrderInstructionReceived -= new OrderInstructionEventHandler(IncomingOrderChannel_OrderInstructionReceived);
           
            _incomingOrderChannel.Stop();
            _domain.Stop();
            _quitSignal.Set();
            _tradingPhasesThread.Join();            
            Thread.Sleep(1000);
        }       

        private void StartExchange()
        {
            _domain.Start();
            Thread.Sleep(1000);

            _incomingOrderChannel.OrderInstructionReceived += new OrderInstructionEventHandler(IncomingOrderChannel_OrderInstructionReceived);
            _incomingOrderChannel.Start();

            _quitSignal.Reset();
            _tradingPhasesThread.Start();
            _running = true;
        }       

        private void EnhanchedTradingPhasesThread()
        {
            WaitHandle[] EventArray = new WaitHandle[] {
                _quitSignal,
                _userSignal
            };

            _exchangeRunningStatus = ExchangeRunningStatus.Init;

            Trace(LogLevel.Method, "EnhanchedTradingPhasesThread started.");

            Dictionary<SessionState, TimeSpan> periodDuration = new Dictionary<SessionState, TimeSpan>();
            periodDuration[SessionState.Open] = TimeSpan.Parse(ConfigurationClient.Instance.GetConfigSetting("OpenPeriodDuration", "00:02:50"));
            periodDuration[SessionState.Close] = TimeSpan.Parse(ConfigurationClient.Instance.GetConfigSetting("ClosePeriodDuration", "00:00:10"));

            SessionState currentState;
            DateTime end;
            int which = -1;

            do
            {
                switch (_exchangeRunningStatus)
                {
                    case ExchangeRunningStatus.Init:
                        _exchangeRunningStatus = ExchangeRunningStatus.Open;
                        break;

                    case ExchangeRunningStatus.Open:
                        currentState = SessionState.Open;
                        end = DateTime.Now.Add(periodDuration[currentState]);
                        Trace(LogLevel.Warning, "New trading phase started: '{2}' from {0} to {1} ",
                            DateTime.Now, end,
                            currentState);
                        MDService.BroadcastStatusMessage(currentState, DateTime.Now, end);
                        Console.WriteLine("\n\n{1} Exchange now OPEN. Will CLOSE at {0}\n", end, DateTime.Now);

                        PrintMainMenu(true);

                        which = WaitHandle.WaitAny(EventArray, periodDuration[currentState]);
                        if (which == 0)
                        {
                            _exchangeRunningStatus = ExchangeRunningStatus.Terminated;
                        }
                        else if (which == 1)
                        {
                            _exchangeRunningStatus = ExchangeRunningStatus.ClosedByUser;
                        }
                        else
                        {
                            _exchangeRunningStatus = ExchangeRunningStatus.ClosedByTimeout;
                        }
                        break;

                    case ExchangeRunningStatus.ClosedByTimeout:
                        currentState = SessionState.Close;
                        _domain.PullOrders("Trading period expired.");
                        end = DateTime.Now.Add(periodDuration[currentState]);
                        Trace(LogLevel.Warning, "New trading phase started: '{2}' from {0} to {1} ",
                            DateTime.Now, end,
                            currentState);
                        MDService.BroadcastStatusMessage(currentState, DateTime.Now, end);
                        Console.WriteLine("\n\n{1} Exchange now CLOSE. Will OPEN at {0}\n", end, DateTime.Now);

                        PrintMainMenu(false);

                        which = WaitHandle.WaitAny(EventArray, periodDuration[currentState]);
                        if (which == 0)
                        {
                            _exchangeRunningStatus = ExchangeRunningStatus.Terminated;
                        }
                        else
                        {
                            _exchangeRunningStatus = ExchangeRunningStatus.Open;
                        }  
                        break;

                    case ExchangeRunningStatus.ClosedByUser:
                        currentState = SessionState.Close;
                        _domain.PullOrders("Exchange closed by user.");
                        end = DateTime.Now.AddDays(1);
                        Trace(LogLevel.Warning, "Exchange manually CLOSED by user");
                        Trace(LogLevel.Warning, "New trading phase started: '{2}' from {0} to {1} ",
                            DateTime.Now, end,
                            currentState);
                        MDService.BroadcastStatusMessage(currentState, DateTime.Now, end);
                        Console.WriteLine("\n\n{0} Exchange CLOSED by user\n", DateTime.Now);

                        PrintMainMenu(false);

                        which = WaitHandle.WaitAny(EventArray);
                        if (which == 0)
                        {
                            _exchangeRunningStatus = ExchangeRunningStatus.Terminated;
                        }
                        else
                        {
                            _exchangeRunningStatus = ExchangeRunningStatus.Open;
                        }  
                        break;

                    case ExchangeRunningStatus.Terminated:
                    default:
                        break;
                }
            }
            while (_exchangeRunningStatus != ExchangeRunningStatus.Terminated);

            MDService.BroadcastStatusMessage(SessionState.Close, DateTime.Now, DateTime.Now);

            Trace(LogLevel.Method, "EnhanchedTradingPhasesThread finished.");
        }

        private bool IsNowOpen { get { return _exchangeRunningStatus == ExchangeRunningStatus.Open; } }

        private void IncomingOrderChannel_OrderInstructionReceived(object sender, OrderInstruction instruction, IncomingOrder incomingOrder, Order otherOrder)
        {            
            switch (instruction)
            {
                case OrderInstruction.New:
                    {
                        if (!IsNowOpen)
                        {
                            incomingOrder.Reject("Exchange closed");
                        }
                        else
                        {
                            _domain.ReceiveNewOrder(incomingOrder);
                        }
                        break;
                    }
                case OrderInstruction.Amend:
                    {
                        if (!_allowAmendments)
                        {
                            incomingOrder.RejectAmendment("Amendments not allowed");
                        }
                        else if (!IsNowOpen)
                        {
                            incomingOrder.RejectAmendment("Exchange closed");
                        }
                        else
                        {
                            _domain.ReceiveAmendmentRequest(incomingOrder, otherOrder);
                        }
                        break;
                    }
                case OrderInstruction.Cancel:
                    {
                        if (!_allowCancellations)
                        {
                            incomingOrder.RejectCancel("Cancellations not allowed");
                        }
                        else if (!IsNowOpen)
                        {
                            incomingOrder.RejectCancel("Exchange closed");
                        }
                        else
                        {
                            _domain.ReceiveCancellationRequest(incomingOrder);
                        }
                        break;
                    }
                case OrderInstruction.Ping:
                    {
                        Trace(LogLevel.Debug, "PING message received from {0}, ignoring", incomingOrder.Origin);
                        break;
                    }
                default:
                    throw new ArgumentException("IncomingOrderChannel_OrderInstructionReceived. Invalid instruction: " + instruction.ToString());
            }
        }            

        private void Pause(string message)
        {
            Console.WriteLine(message);
            Console.ReadLine();
        }
    }
}
