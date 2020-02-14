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
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using Neos.IdentityServer.MultiFactor.Data;

namespace Neos.IdentityServer.MultiFactor.NotificationHub
{
    internal class CleanUpManager
    {
        private Task _cleanup = null;
        private CancellationTokenSource _canceller = null;
        private bool _mustexit = false;
        private readonly bool _onlymfacerts = true;
        private readonly int _minutes = 5;
        private readonly bool _cleanupenable = false;
        private ServiceBase _service;

        /// <summary>
        /// Static constructor implementation
        /// </summary>
        internal CleanUpManager()
        {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey("Software\\MFA");
            _cleanupenable = Convert.ToBoolean(rk.GetValue("EnablePrivateKeysCleanUp"));
            _minutes = Convert.ToInt32(rk.GetValue("PrivateKeysCleanUpDelay"));
            _onlymfacerts = Convert.ToBoolean(rk.GetValue("LimitPrivateKeysCleanUp"));
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
                        int res = Certs.CleanOrphanedPrivateKeys(_onlymfacerts);
                        if (res > 0)
                            _service.EventLog.WriteEntry(string.Format("{0} orphaned certificates private keys deleted.", res), EventLogEntryType.Error, 1010);
                        Thread.Sleep(new TimeSpan(0, 0, _minutes, 0)); 
                    }
                    catch (ThreadAbortException)
                    {
                        _mustexit = true;
                        return;
                    }
                    catch (Exception ex)
                    {
                        _service.EventLog.WriteEntry(string.Format("error on CleanUp Keys : {0}.", ex.Message), EventLogEntryType.Error, 1011);
                    }
                }
            }
        }

        /// <summary>
        /// Start method implementation
        /// </summary>
        internal void Start(ServiceBase svc)
        {
            if (!_cleanupenable)
            {
                _mustexit = true;
                return;
            }
            _service = svc;
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
            if (_canceller!=null)
                _canceller.Cancel();
        }
    }
}
