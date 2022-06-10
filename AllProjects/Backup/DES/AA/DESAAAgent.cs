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

using OPEX.Common;
using OPEX.DES.Exchange;
using OPEX.DES.OrderManager;
using OPEX.StaticData;

namespace OPEX.DES.AA
{
    class DESAAAgent : DESAgent
    {
        private EquilibriumEstimator _equilibriumEstimator;
        
        private AdaptiveComponent _adaptiveComponent;
        private AggressivenessModel _aggressivenessModel;
        private BiddingComponent _biddingComponent;

        public DESAAAgent(string agentName, DESAgentWakeupMode wakeupMode, int sleepTimeMsec, int inactivitySleepTimeMsec, ParameterBag parameters, GlobalOrderBook gob)
            : base(agentName, wakeupMode, sleepTimeMsec, inactivitySleepTimeMsec, parameters, gob)
        {
        }

        public override string Type
        {
            get { return "DESAA"; }
        }

        protected override void LoadSettings()
        {
            // estimator parameters
            _parameters.AddDefaultValue("WindowSize", "30");
            _parameters.AddDefaultValue("Rho", "0.9"); // (0.0, 1.0)

            // bidding parameters            
            _parameters.AddDefaultValue("Eta", "3.0"); // (1.0, +inf)
            _parameters.AddDefaultValue("MaxSpread", "1.0");

            // learning parameters           
            _parameters.AddDefaultValue("Beta1", "0.5"); // (0.0, 1.0)
            _parameters.AddDefaultValue("Beta2", "0.5"); // (0.0, 1.0)
            _parameters.AddDefaultValue("LambdaRelative", "0.05"); // (0.0, 1.0)
            _parameters.AddDefaultValue("LambdaAbsolute", "0.01"); // (0.0, 1.0) 
        }

        protected override void OnNewSimulationStarted()
        {
            _equilibriumEstimator = new EquilibriumEstimator(this);
            _aggressivenessModel = new AggressivenessModel(this);
            _adaptiveComponent = new AdaptiveComponent(this, _aggressivenessModel);
            _biddingComponent = new BiddingComponent(this);
        }

        protected override void OnNewOrder()
        {
            OrderSide side = CurrentAssignment.Side;
            double limitPrice = CurrentAssignment.Price;
            Instrument currentInstrument = CurrentInstrument;

            _logger.Trace(LogLevel.Info, "OnNewOrder.");

            // aggressiveness model
            _aggressivenessModel.LimitPrice = limitPrice;
            _aggressivenessModel.PMax = currentInstrument.MaxPrice;
            _aggressivenessModel.Side = side;

            // adaptive component
            _adaptiveComponent.LimitPrice = limitPrice;
            _adaptiveComponent.Side = side;

            // bidding component
            _biddingComponent.Instrument = currentInstrument;
            _biddingComponent.LimitPrice = limitPrice;
            _biddingComponent.Side = side;
        }

        protected override void OnPlay(out bool newShout, out bool newDepth)
        {            
            newShout = newDepth = false;
            if (IsActive)
            {
                if (_biddingComponent.IsFirstTradingRound)
                {
                    _logger.Trace(LogLevel.Debug, "OnPlay. Haven't received the first transaction yet, cannot price order. Skipping.");
                    return;
                }

                double tau = _aggressivenessModel.ComputeTau(_adaptiveComponent.Theta, _adaptiveComponent.Aggressiveness, _equilibriumEstimator.EstimatedPrice);
                bool success = false;
                double price = _biddingComponent.Price(tau, out success);
                _biddingComponent.IsFirstTradingRound = false;

                if (success)
                {
                    newShout = newDepth = SendOrAmendCurrentOrder(price);
                }
            }
        }
        
        protected override void OnNewShout()
        {
            Shout lastShout = LastMarketData.LastShout;
            bool accepted = lastShout.Accepted;

            // 1. Update Equilibrium Estimator
            double estimatedPrice = _equilibriumEstimator.EstimatedPrice;
            double transactionPrice = double.NaN;
            if (!accepted)
            {
                _logger.Trace(LogLevel.Debug, "OnNewShout. Shout was REJECTED : {0}", lastShout.ToString());
            }
            else
            {
                transactionPrice = lastShout.TradePrice;
                _logger.Trace(LogLevel.Debug, "OnNewShout. Shout was ACCEPTED @ {1}: {0}", lastShout.ToString(), transactionPrice);
                _equilibriumEstimator.AddNewTransactionPrice(transactionPrice);
                estimatedPrice = _equilibriumEstimator.EstimatedPrice;
                _adaptiveComponent.UpdateLongTerm(transactionPrice, estimatedPrice);
            }

            // 2. Update Bidding Component
            _biddingComponent.UpdateBestPrices(LastMarketData.LastSnapshot.Buy.Best, LastMarketData.LastSnapshot.Sell.Best);

            if (!accepted && _biddingComponent.IsFirstTradingRound)
            {
                _logger.Trace(LogLevel.Debug, "OnNewShout. AA needs a transaction to start. Skipping update.");
                return;
            }

            double theta = _adaptiveComponent.Theta;
            double aggressiveness = _adaptiveComponent.Aggressiveness;

            // 2.5. Setup Aggressiveness Model (FIRST TIME ONLY)
            if (_biddingComponent.IsFirstTradingRound && accepted)
            {                
                bool b = false;
                double pi = _biddingComponent.Price(_aggressivenessModel.Tau, out b);
                _logger.Trace(LogLevel.Debug, "OnShout. (FIRST ROUND) pi := {0}", pi);
                _logger.Trace(LogLevel.Debug, "OnShout. (FIRST ROUND) theta = {0}", theta);
                aggressiveness = _aggressivenessModel.ComputeRShout(theta, estimatedPrice, pi);
                _logger.Trace(LogLevel.Debug, "OnShout. (FIRST ROUND) r = {0}", aggressiveness);
                _adaptiveComponent.InitialiseAggressiveness(aggressiveness);
                _biddingComponent.IsFirstTradingRound = false;
            }

            // 3. Update Adaptive Component (may require _aggressivenessModel.Tau)
            _adaptiveComponent.UpdateShortTerm(lastShout, estimatedPrice);
            _logger.Trace(LogLevel.Info, "OnShout. OLD r = {0}; NEW r = {1}", aggressiveness, _adaptiveComponent.Aggressiveness);            
        }        
    }
}
