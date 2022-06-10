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
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

using Timer = System.Threading.Timer;

namespace OPEX.DWEGUI
{
    class Blinking
    {        
        private readonly int EffectPeriod = 250;
        private readonly int EffectLengthSec = 5;
        private readonly Control _messageTextBox;
        private readonly Control _blinkControl;        
        private readonly Color _blinkColor1;
        private readonly Color _blinkColor2;

        private DateTime _startTime;
        private bool _isServingRequest = false;        
        private Timer _timer;

        public Blinking(TextBox messageTextBox, Control blinkControl, Color blinkColor1, Color blinkColor2)
        {
            _messageTextBox = messageTextBox;
            _blinkControl = blinkControl;
            _blinkColor1 = blinkColor1;
            _blinkColor2 = blinkColor2;            
        }

        public event EventHandler Finished;

        void timerCallback(object state)
        {
            TimerHasExpired();
        }       

        public void Start(string message)
        {            
            if (_isServingRequest)
            {
                return;
            }
            
            _isServingRequest = true;
            SafeSetText(message);            
            StartEffect();
        }

        private void StartEffect()
        {
            _startTime = DateTime.Now;
            OnStartEffect();
            _timer = new System.Threading.Timer(new TimerCallback(timerCallback), null, 
                TimeSpan.FromMilliseconds(EffectPeriod), TimeSpan.FromMilliseconds(EffectPeriod));
        }                

        private void OnStopEffect()
        {
            SafeChangeBackColor(SystemColors.Control);            
            SafeChangeVisible(false);

            if (Finished != null)
            {
                Finished(this, null);
            }
        }

        private void OnStartEffect()
        {            
            SafeChangeVisible(true);            
        }

        private void DoEffect()
        {
            if (_blinkControl.BackColor == _blinkColor1)
            {
                SafeChangeBackColor(_blinkColor2);
            }
            else
            {
                SafeChangeBackColor(_blinkColor1);
            }      
        }

        private void StopEffect()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _isServingRequest = false;
            OnStopEffect();
        }

        private void TimerHasExpired()
        {
            DoEffect();

            if (DateTime.Now.Subtract(_startTime).CompareTo(TimeSpan.FromSeconds(EffectLengthSec)) >= 0)
            {
                StopEffect();
            }
        }

        void Tick(object sender, EventArgs e)
        {
            TimerHasExpired();
        }

        delegate void SafeChangeBackColorDelegate(Color color);
        void SafeChangeBackColor(Color color)
        {
            if (_blinkControl.InvokeRequired)
            {
                _blinkControl.Invoke(new SafeChangeBackColorDelegate(SafeChangeBackColor), color);
            }
            else
            {
                _blinkControl.BackColor = color;
            }
        }

        delegate void SafeChangeVisibledelegate(bool visible);
        void SafeChangeVisible(bool visible)
        {
            if (_messageTextBox.InvokeRequired)
            {
                _messageTextBox.Invoke(new SafeChangeVisibledelegate(SafeChangeVisible), visible);
            }
            else
            {
                _messageTextBox.Visible = visible;
            }
        }

        delegate void SafeSetTextDelegate(string text);
        void SafeSetText(string text)
        {
            if (_messageTextBox.InvokeRequired)
            {
                _messageTextBox.Invoke(new SafeSetTextDelegate(SafeSetText), text);
            }
            else
            {
                _messageTextBox.Text = text;
            }
        }
    }
}
