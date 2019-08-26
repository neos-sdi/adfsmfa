//******************************************************************************************************************************************************************************************//
// Copyright (c) 2019 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
//                                                                                                                                                                                          //
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),                                       //
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,   //
// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:                                                                                   //
//                                                                                                                                                                                          //
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.                                                           //
//                                                                                                                                                                                          //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,                                      //
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,                            //
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                               //
//                                                                                                                                                                                          //
// https://adfsmfa.codeplex.com                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Neos.IdentityServer.MultiFactor;

namespace Neos.IdentityServer.MultiFactor.NotificationHub
{
    internal class ReplayManager
    {
        private List<NamedPipeNotificationReplayRecord> _lst = new List<NamedPipeNotificationReplayRecord>();
        private object _lock = new object();
        private Task _cleanup = null;
        private CancellationTokenSource _canceller = null;
        private bool _mustexit = false;

        /// <summary>
        /// Static constructor implementation
        /// </summary>
        internal ReplayManager()
        {
            
        }

        /// <summary>
        /// AddToReplay method implementation
        /// </summary>
        internal bool AddToReplay(NamedPipeNotificationReplayRecord record)
        {
            bool exist = false;
            if (record.ReplayLevel == ReplayLevel.Disabled)
                return true;
            else if (record.ReplayLevel == ReplayLevel.Intermediate)
            {
                lock (_lock)
                {
                    exist = _lst.Exists(s => (s.UserName.ToLower().Equals(record.UserName.ToLower())) && (s.Totp == record.Totp) && (!s.UserIPAdress.Equals(record.UserIPAdress)));
                    if (!exist)
                        _lst.Add(record);
                }
                return !exist;
            }
            else if (record.ReplayLevel == ReplayLevel.Full)
            {
                lock (_lock)
                {
                    exist = _lst.Exists(s => (s.UserName.ToLower().Equals(record.UserName.ToLower())) && (s.Totp == record.Totp));
                    if (!exist)
                        _lst.Add(record);
                }
                return !exist;
            }
            return false;
        }

        /// <summary>
        /// Background CleanUp method
        /// </summary>
        private void CleanUpMethod()
        {
            using (_canceller.Token.Register(Thread.CurrentThread.Abort))
            {
                while (!_mustexit)
                {
                    try
                    {
                        lock (_lock)
                        {
                            _lst.RemoveAll(s => s.UserLogon.AddSeconds(s.DeliveryWindow) < DateTime.Now);  // adjust correctly delay -> datetime must bes passed in record, not only logon time
                        }
                        Thread.Sleep(new TimeSpan(0, 0, 1, 0)); // every minute
                    }
                    catch (ThreadAbortException)
                    {
                        _mustexit = true;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Start method implementation
        /// </summary>
        internal void Start()
        {
            _mustexit = false;
            _canceller = new CancellationTokenSource();
            _cleanup = Task.Factory.StartNew(new Action(CleanUpMethod), _canceller.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <summary>
        /// Close method implementation
        /// </summary>
        internal void Close()
        {
            _mustexit = true;
            _canceller.Cancel();
        }
    }
}
