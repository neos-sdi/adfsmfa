//******************************************************************************************************************************************************************************************//
// Copyright (c) 2020 @redhook62 (adfsmfa@gmail.com)                                                                                                                                    //                        
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
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Neos.IdentityServer.MultiFactor
{
    /// <summary>
    /// ReplayService class
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ReplayService : IReplay
    {
        private static ReplayManager _manager = new ReplayManager();
        private static readonly object _lock = new object();
        private EventLog _log;

        /// <summary>
        /// Constructor implmentation
        /// </summary>
        public ReplayService(IDependency dep)
        {
            _log = dep.GetEventLog();
        }

        /// <summary>
        /// Check method implementation
        /// </summary>
        public bool Check(List<string> servernames, ReplayRecord record)
        {
            bool tempresult = false;
            try
            {
                lock (_lock)
                {
                    tempresult = _manager.AddToReplay(record);
                    if (tempresult)
                    {
                        if ((record.MustDispatch) && (servernames != null))
                        {
                            foreach (string fqdn in servernames)
                            {
                                ReplayClient replaymanager = new ReplayClient();
                                try
                                {
                                    replaymanager.Initialize(fqdn);
                                    IReplay client = replaymanager.Open();

                                    try
                                    {
                                        record.MustDispatch = false;
                                        tempresult = client.Check(servernames, record);
                                    }
                                    catch (Exception e)
                                    {
                                        _log.WriteEntry(string.Format("Error on Check Remote Service method : {0} => {1}.", fqdn, e.Message), EventLogEntryType.Error, 1011);
                                    }
                                    finally
                                    {
                                        replaymanager.Close(client);
                                    }
                                }
                                catch (Exception e)
                                {
                                    _log.WriteEntry(string.Format("Error on Check Remote Service method : {0} => {1}.", fqdn, e.Message), EventLogEntryType.Error, 1011);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on Check Service method : {0}.", e.Message), EventLogEntryType.Error, 1010);
            }
            return tempresult;
        }

        /// <summary>
        /// Reset method implementation
        /// </summary>
        public void Reset(List<string> servernames)
        {
            try
            {
                lock (_lock)
                {
                    _manager.Reset();
                }
                if (servernames != null)
                {
                    foreach (string fqdn in servernames)
                    {
                        ReplayClient replaymanager = new ReplayClient();
                        try
                        {
                            replaymanager.Initialize(fqdn);
                            IReplay client = replaymanager.Open();

                            try
                            {
                                client.Reset(null);
                            }
                            catch (Exception e)
                            {
                                _log.WriteEntry(string.Format("Error on Check Remote Service method : {0} => {1}.", fqdn, e.Message), EventLogEntryType.Error, 1011);
                            }
                            finally
                            {
                                replaymanager.Close(client);
                            }
                        }
                        catch (Exception e)
                        {
                            _log.WriteEntry(string.Format("Error on Check Remote Service method : {0} => {1}.", fqdn, e.Message), EventLogEntryType.Error, 1011);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on Reset Service method : {0}.", e.Message), EventLogEntryType.Error, 1011);
            }
        }
    }

    /// <summary>
    /// WebThemeService class
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class WebThemeService : IWebThemeManager
    {
        private static WebThemeManager _manager = null;
        private EventLog _log;

        /// <summary>
        /// Constructor implmentation
        /// </summary>
        public WebThemeService(IDependency dep)
        {
            _log = dep.GetEventLog();
            _manager = new WebThemeManager(dep);
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public bool Initialize(Dictionary<string, bool> servers, WebThemesParametersRecord context, string request, out string identifier)
        {
            try
            {
                return _manager.Initialize(servers, context, new Uri(request), out identifier);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebTheme Service Initialize method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// HasRelyingPartyTheme method implmentation
        /// </summary>
        public bool HasRelyingPartyTheme(WebThemesParametersRecord context)
        {
            try
            {
                return _manager.HasRelyingPartyTheme(context);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebTheme Service HasRelyingPartyTheme method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// GetAddresses method implementation
        /// </summary>
        public Dictionary<WebThemeAddressKind, string> GetAddresses(WebThemesParametersRecord context)
        {
            try
            {
                return _manager.GetAddresses(context);   
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebTheme Service GetAddresses method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// GetIllustrationAddress method implementation
        /// </summary>
        public string GetIllustrationAddress(WebThemesParametersRecord context)
        {
            try
            {
                return _manager.GetIllustrationAddress(context);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebTheme Service GetIllustrationAddress method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// GetLogoAddress method implementation
        /// </summary>
        public string GetLogoAddress(WebThemesParametersRecord context)
        {
            try
            {
                return _manager.GetLogoAddress(context);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebTheme Service GetLogoAddress method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// GetStyleSheetAddress method implementation
        /// </summary>
        public string GetStyleSheetAddress(WebThemesParametersRecord context)
        {
            try
            {
                return _manager.GetStyleSheetAddress(context);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebTheme Service GetStyleSheetAddress method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// DispachTheme method implmentation
        /// </summary>
        public void DispachTheme(WebThemeRecord theme)
        {
            try
            {
                _manager.DispachTheme(theme);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebTheme Service DispachTheme method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// RequestTheme method implmentation
        /// </summary>
        public WebThemeRecord RequestTheme(WebThemesParametersRecord theme)
        {
            try
            {
                return _manager.RequestTheme(theme);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebTheme Service RequestTheme method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// ResetThemesList method implmentation
        /// </summary>
        public void ResetThemes()
        {
            try
            {
                _manager.ResetThemes();
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebTheme Service RestThemesList method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// ResetThemesList method implmentation
        /// </summary>
        public void ResetThemesList(Dictionary<string, bool> servers)
        {
            try
            {
                _manager.ResetThemesList(servers);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebTheme Service RestThemesList method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }
    }
}
