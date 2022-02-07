//******************************************************************************************************************************************************************************************//
// Copyright (c) 2022 @redhook62 (adfsmfa@gmail.com)                                                                                                                                        //                        
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
//                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor
{
    public class NTServiceManager
    {
        private EventLog _log;

        /// <summary>
        /// NTServiceManager constructor
        /// </summary>
        internal NTServiceManager(IDependency dep)
        {
            _log = dep.GetEventLog();
        }

        /// <summary>
        /// Continue method implementation
        /// </summary>
        public bool Continue(string name)
        {
            ServiceController svc = new ServiceController(name);
            try
            {
                svc.Continue();
                svc.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 1, 0));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                svc.Close();
            }
        }

        /// <summary>
        /// Pause method implementation
        /// </summary>
        public bool Pause(string name)
        {
            ServiceController svc = new ServiceController(name);
            try
            {
                svc.Pause();
                svc.WaitForStatus(ServiceControllerStatus.Paused, new TimeSpan(0, 1, 0));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                svc.Close();
            }
        }

        /// <summary>
        /// Start method implementation
        /// </summary>
        public bool Start(string name)
        {
            ServiceController svc = new ServiceController(name);
            try
            {
                svc.Start();
                svc.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 1, 0));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                svc.Close();
            }
        }

        /// <summary>
        /// Stop method implementation
        /// </summary>
        public bool Stop(string name)
        {
            ServiceController svc = new ServiceController(name);
            try
            {
                svc.Stop();
                svc.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 1, 0));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                svc.Close();
            }
        }

        /// <summary>
        /// IsRunning method implementation
        /// </summary>
        public bool IsRunning(string name)
        {
            ServiceController svc = new ServiceController(name);
            try
            {
                return svc.Status == ServiceControllerStatus.Running;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                svc.Close();
            }
        }

        /// <summary>
        /// Exists method implementation
        /// </summary>
        public bool Exists(string name)
        {
            ServiceController svc = new ServiceController(name);
            try
            {               
                return svc.Status != 0;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                svc.Close();
            }
        }       
    }
}
