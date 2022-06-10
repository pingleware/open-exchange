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
using System.Threading;

using OPEX.Common;
using OPEX.OM.Common;
using OPEX.Agents.Common;
using OPEX.Agents.AA.Components;
using OPEX.StaticData;
using OPEX.SupplyService.Common;
using OPEX.DWEAS.Client;

namespace OPEX.Agents.AA
{
    public class AAAgent : Agent
    {       
        private readonly EquilibriumEstimator _equilibriumEstimator;
     
        private AssignmentBucket _lastAssignmentBucket;        
        private AdaptiveComponent _adaptiveComponent;
        private AggressivenessModel _aggressivenessModel;
        private BiddingComponent _biddingComponent;
        private bool _firstUpdate;

        public AAAgent(string agentName, AgentWakeupMode wakeupMode, int sleepTimeMsec, int inactivitySleepTimeMsec, ParameterBag parameters)
            : base(agentName, wakeupMode, sleepTimeMsec, inactivitySleepTimeMsec, parameters)       
        {
            _equilibriumEstimator = new EquilibriumEstimator(this);           
        }

        public override string Type
        {
            get { return "AA"; }
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
       
        protected override void OnWakeUp(Stimulus stimulus)
        {
            bool workOrder = false;
            switch (stimulus.Type)
            {
                case StimulusType.Shout:
                    ShoutStimulus shout = stimulus as ShoutStimulus;
                    OnShout(shout);
                    workOrder = true;                    
                    break;
                case StimulusType.Timer:
                    TimerStimulus ts = stimulus as TimerStimulus;
                    if (ts.Primary)
                    {
                        workOrder = AdjustFromInactivity();
                    }
                    else
                    {
                        AdjustFromInactivity();
                    }
                    break;
                default:
                    return;
            }

            if (WakeUp && workOrder)
            {
                WorkOrder();
            }
        }    

        private bool AdjustFromInactivity()
        {
            // 1. Update Equilibrium Estimator
            double estimatedPrice = _equilibriumEstimator.EstimatedPrice;                       

            // 2. Update Bidding Component
            _biddingComponent.UpdateBestPrices(_state.LastSnapshot.Buy.Best, _state.LastSnapshot.Sell.Best);

            double theta = _adaptiveComponent.Theta;
            double aggressiveness = _adaptiveComponent.Aggressiveness;

            // 3. Update Adaptive Component (may require _aggressivenessModel.Tau)
            bool update = _adaptiveComponent.UpdateShortTermFromInactivity(estimatedPrice);
            if (update)
            {
                _logger.Trace(LogLevel.Info, "OnShout. r[{1}] = {0}; r[{2}] = {3}", aggressiveness, _roundNumber, _roundNumber + 1, _adaptiveComponent.Aggressiveness);
                _roundNumber++;
                return true;
            }

            return false;
        }       

        protected override void OnOfflineShout(ShoutStimulus shoutStimulus)
        {
            _logger.Trace(LogLevel.Info, "OnOfflineShout. {0}", shoutStimulus.ToString());
            
            OnShout(shoutStimulus);
        }

        private int _roundNumber = 0;
        private void OnShout(ShoutStimulus shoutStimulus)
        {
            bool accepted = shoutStimulus.Shout.Accepted;            

            // 1. Update Equilibrium Estimator
            double estimatedPrice = _equilibriumEstimator.EstimatedPrice;
            double transactionPrice = double.NaN;
            if (!accepted)
            {
                _logger.Trace(LogLevel.Debug, "OnShout. Shout was REJECTED : {0}", shoutStimulus.Shout);
            }
            else
            {
                transactionPrice = shoutStimulus.LastTrade.Price;
                _logger.Trace(LogLevel.Debug, "OnShout. Shout was ACCEPTED @ {1}: {0}", shoutStimulus.Shout, transactionPrice);
                _equilibriumEstimator.AddNewTransactionPrice(transactionPrice);
                estimatedPrice = _equilibriumEstimator.EstimatedPrice;
                _adaptiveComponent.UpdateLongTerm(transactionPrice, estimatedPrice);
            }

            // 2. Update Bidding Component
            _biddingComponent.UpdateBestPrices(_state.LastSnapshot.Buy.Best, _state.LastSnapshot.Sell.Best);

            double theta = _adaptiveComponent.Theta;           
            double aggressiveness = _adaptiveComponent.Aggressiveness;                    

            // 2.5. Setup Aggressiveness Model (FIRST TIME ONLY)
            if (_firstUpdate && accepted)
            {
                _roundNumber = 0;
                bool b = false;
                double pi = _biddingComponent.Price(_aggressivenessModel.Tau, out b);
                _logger.Trace(LogLevel.Debug, "OnShout. (FIRST ROUND) pi := {0}", pi);
                _logger.Trace(LogLevel.Debug, "OnShout. (FIRST ROUND) theta[{1}] = {0}", theta, _roundNumber);
                aggressiveness = _aggressivenessModel.ComputeRShout(theta, estimatedPrice, pi);
                _logger.Trace(LogLevel.Debug, "OnShout. (FIRST ROUND) r[{1}] = {0}", aggressiveness, _roundNumber);
                _adaptiveComponent.InitialiseAggressiveness(aggressiveness);
                _firstUpdate = false;
            }           
            
            // 3. Update Adaptive Component (may require _aggressivenessModel.Tau)
            _adaptiveComponent.UpdateShortTerm(shoutStimulus, estimatedPrice);
            _logger.Trace(LogLevel.Info, "OnShout. r[{1}] = {0}; r[{2}] = {3}", aggressiveness, _roundNumber, _roundNumber + 1, _adaptiveComponent.Aggressiveness);
            _roundNumber++;
        }
        
        private void WorkOrder()
        {         
            double tau = _aggressivenessModel.ComputeTau(_adaptiveComponent.Theta, _adaptiveComponent.Aggressiveness, _equilibriumEstimator.EstimatedPrice);
            bool success = false;            

            _logger.Trace(LogLevel.Info, "WorkOrder. tau[{1}] = {0}", tau, _roundNumber);
            
            double price = _biddingComponent.Price(tau, out success);
            if (price != 0)
            {
                _biddingComponent.IsFirstTradingRound = false;

                if (success)
                {
                    SendOrAmendCurrentOrder(price);
                }
                else
                {
                    _logger.Trace(LogLevel.Warning, "WorkOrder. BiddingComponent.Price was not successful.");
                }
            }
            else
            {
                _logger.Trace(LogLevel.Error, "WorkOrder. BiddingComponent.Price returned a ZERO PRICE!!!");
            }
        }

        protected override void OnNewSimulationStarted()
        {
            _lastAssignmentBucket = null;
        }

        protected override void OnNewOrder()
        {
            _logger.Trace(LogLevel.Info, "OnNewOrder. BEGIN");                        

            bool firstOrder = false;
            bool priceChanged = false;
            bool ricChanged = false;
            bool sideChanged = false;

            if (_lastAssignmentBucket == null)
            {                
                firstOrder = true;                
                _logger.Trace(LogLevel.Debug, "OnNewOrder. lastAssignment == null ---> firstOrder = true;");
            }
            else
            {
                priceChanged = _lastAssignmentBucket.Price != _state.CurrentAssignmentBucket.Price;
                _logger.Trace(LogLevel.Debug, "OnNewOrder. lastLimitPrice {0} currentLimitPrice {1} priceChanged {2}",
                    _lastAssignmentBucket.Price, _state.CurrentAssignmentBucket.Price, priceChanged);
                
                if (!_lastAssignmentBucket.RIC.Equals(_state.CurrentAssignmentBucket.RIC))
                {
                    sideChanged = true;
                    _logger.Trace(LogLevel.Debug, "OnNewOrder. lastRic {0} currentRic {1} ricChanged true",
                        _lastAssignmentBucket.RIC, _state.CurrentAssignmentBucket.RIC, priceChanged);
                }
                
                if (_lastAssignmentBucket.Side != _state.CurrentAssignmentBucket.Side)
                {
                    sideChanged = true;  
                    _logger.Trace(LogLevel.Debug, "OnNewOrder. lastSide {0} currentSide {1} sideChanged true",
                        _lastAssignmentBucket.Side, _state.CurrentAssignmentBucket.Side, sideChanged);                                      
                }
            }

            OrderSide side = _state.CurrentAssignmentBucket.Side;
            double limitPrice = _state.CurrentAssignmentBucket.Price;
            Instrument currentInstrument = _state.CurrentInstrument;

            if (firstOrder || ricChanged)
            {
                _logger.Trace(LogLevel.Method, "OnNewOrder. Resetting EquilibriumEstimator =======================================");
                _equilibriumEstimator.Reset();                
            }
            
            if (firstOrder || ricChanged || sideChanged)
            {
                _logger.Trace(LogLevel.Method, "OnNewOrder. Creating new components =======================================");
                _aggressivenessModel = new AggressivenessModel(this, side, limitPrice, currentInstrument.MaxPrice);
                _adaptiveComponent = new AdaptiveComponent(this, side, limitPrice, _aggressivenessModel);
                _biddingComponent = new BiddingComponent(this, side, limitPrice, currentInstrument);                
            }
            else if (priceChanged)
            {
                _logger.Trace(LogLevel.Info, "OnNewOrder. Updating limit price =========================================");
                _aggressivenessModel.UpdateLimitPrice(limitPrice);
                _adaptiveComponent.UpdateLimitPrice(limitPrice);
                _biddingComponent.UpdateLimitPrice(limitPrice);
            }

            _firstUpdate = firstOrder || ricChanged || sideChanged;
            _lastAssignmentBucket = _state.CurrentAssignmentBucket;

            _logger.Trace(LogLevel.Debug, "OnNewOrder. firstUpdate = {0}", _firstUpdate);

            AdjustFromInactivity();
            WorkOrder();

            _logger.Trace(LogLevel.Info, "OnNewOrder. END");
        }     
    }
}
