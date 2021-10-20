//******************************************************************************************************************************************************************************************//
// Copyright (c) 2021 @redhook62 (adfsmfa@gmail.com)                                                                                                                                    //                        
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
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.Win32;
using System.IO;
using Microsoft.IdentityModel.Tokens;

namespace Neos.IdentityServer.MultiFactor
{
    internal class ReplayManager
    {
        private List<ReplayRecord> _lst = new List<ReplayRecord>();
        private readonly object _lock = new object();
        private readonly object _bloblock = new object();
        private Task _cleanup = null;
        private Task _blobrefresh = null;
        private CancellationTokenSource _canceller = null;
        private CancellationTokenSource _blobcanceller = null;
        private bool _mustexit = false;
        private bool _blobmustexit = false;
        private EventLog _log;
        private bool _started = false;
        private readonly HttpClient _httpClient;
        protected readonly string _blobUrl;

        /// <summary>
        /// Static constructor implementation
        /// </summary>
        internal ReplayManager(IDependency dep)
        {
            _log = dep.GetEventLog();
            _blobUrl = SystemUtilities.PayloadUrlDownloadFileBlob;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// AddToReplay method implementation
        /// </summary>
        internal bool AddToReplay(ReplayRecord record)
        {
            bool exist = false;
            try
            {
                if (record.ReplayLevel == ReplayLevel.Disabled)
                    return true;
                else if (record.ReplayLevel == ReplayLevel.Intermediate)
                {
                    lock (_lock)
                    {
                        exist = _lst.Exists(s => (s.UserName.ToLower().Equals(record.UserName.ToLower())) && (s.Code == record.Code) && (!s.UserIPAdress.Equals(record.UserIPAdress)));
                        if (!exist)
                            _lst.Add(record);
                        return !exist;
                    }
                }
                else if (record.ReplayLevel == ReplayLevel.Full)
                {
                    lock (_lock)
                    {
                        exist = _lst.Exists(s => (s.UserName.ToLower().Equals(record.UserName.ToLower())) && (s.Code == record.Code));
                        if (!exist)
                            _lst.Add(record);
                        return !exist;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.WriteEntry(string.Format("error on replay registration method : {0}.", ex.Message), EventLogEntryType.Error, 1012);
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
                            int res = _lst.RemoveAll(s => s.UserLogon.AddSeconds(s.DeliveryWindow) < DateTime.Now);
                            if (res > 0)
                                Trace.WriteLine(string.Format("{0} Replay entries removed from checklist.", res));
                        }
                        Thread.Sleep(new TimeSpan(0, 0, 1, 0)); // every minute
                    }
                    catch (ThreadAbortException ex)
                    {
                        _mustexit = true;
                        _log.WriteEntry(string.Format("error on replay cleanup method : {0}.", ex.Message), EventLogEntryType.Error, 1013);
                        return;
                    }
                    catch (Exception ex)
                    {
                        _log.WriteEntry(string.Format("error on replay cleanup method : {0}.", ex.Message), EventLogEntryType.Error, 1013);
                    }
                }
            }
        }

        /// <summary>
        /// Backgroud RefreshBLOBMethod  method
        /// </summary>
        private void RefreshBLOBMethod()
        {
            using (_blobcanceller.Token.Register(Thread.CurrentThread.Abort))
            {
                while (!_blobmustexit)
                {
                    try
                    {
                        lock (_bloblock)
                        {
                            BLOBPayloadInformations infos = GetBLOBPayloadCache();
                            if (infos.CanDownload && ((DateTime.Now > Convert.ToDateTime(infos.NextUpdate)) || (!File.Exists(SystemUtilities.PayloadCacheFile))))
                            {
                                try
                                {
                                    infos.BLOB = GetRawBlob().Result;
                                }
                                catch
                                {
                                    infos.BLOB = null;
                                }
                                if (!string.IsNullOrEmpty(infos.BLOB))
                                {
                                    int oldnumber = infos.Number;
                                    DateTime oldnextupdate = infos.NextUpdate;

                                    RetreiveBlobProperties(infos);
                                    if ((infos.Number > oldnumber) || (Convert.ToDateTime(infos.NextUpdate) > Convert.ToDateTime(oldnextupdate)) || (!File.Exists(SystemUtilities.PayloadCacheFile)))
                                    {
                                        SetBLOBPayloadCache(infos);
                                    }
                                }
                            }
                        }
                        Thread.Sleep(new TimeSpan(0, 1, 0, 0)); // every hour
                    }
                    catch (ThreadAbortException ex)
                    {
                        _blobmustexit = true;
                        _log.WriteEntry(string.Format("error on refresh BLOB method : {0}.", ex.Message), EventLogEntryType.Error, 1014);
                    }
                    catch (Exception ex)
                    {
                        _log.WriteEntry(string.Format("error on refresh BLOB method : {0}.", ex.Message), EventLogEntryType.Error, 1014);
                    }
                }
            }
        }

        /// <summary>
        /// Reset method implementation
        /// </summary>
        internal void Reset()
        {
            lock (_lock)
            {
                _lst.Clear();
                Trace.WriteLine("All Replay entries removed from checklist.");
            }
         }

        /// <summary>
        /// Start method implementation
        /// </summary>
        internal void Start()
        {
            if (!_started)
            {
                _mustexit = false;
                _blobmustexit = false;
                _canceller = new CancellationTokenSource();
                _blobcanceller = new CancellationTokenSource();
                _cleanup = Task.Factory.StartNew(new Action(CleanUpMethod), _canceller.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                _blobrefresh = Task.Factory.StartNew(new Action(RefreshBLOBMethod), _blobcanceller.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                _started = true;
            }
        }

        /// <summary>
        /// Close method implementation
        /// </summary>
        internal void Close()
        {
            _mustexit = true;
            _blobmustexit = true;
            _canceller.Cancel();
            _blobcanceller.Cancel();
            _started = false;
        }

        #region blob management
        /// <summary>
        /// GetRawBlob method implementation
        /// </summary>
        protected async Task<string> GetRawBlob()
        {
            var url = _blobUrl;
            return await DownloadStringAsync(url);
        }

        /// <summary>
        /// DownloadStringAsync method implementation
        /// </summary>
        protected async Task<string> DownloadStringAsync(string url)
        {
            return await _httpClient.GetStringAsync(url);
        }

        /// <summary>
        /// RetreiveBlobProperties method implementation 
        /// </summary>
        protected virtual BLOBPayloadInformations RetreiveBlobProperties(BLOBPayloadInformations infos)
        {
            Dictionary<string, string> dic = DecodeNewPayload(infos.BLOB);
            var xnumber = dic["Number"];
            var xnextupdate = dic["NextUpdate"];
            infos.Number = Convert.ToInt32(xnumber);
            infos.NextUpdate = Convert.ToDateTime(xnextupdate);
            return infos;
        }

        /// <summary>
        /// GetBLOBPayloadCache method implementation
        /// </summary>
        protected virtual BLOBPayloadInformations GetBLOBPayloadCache()
        {
            BLOBPayloadInformations infos = new BLOBPayloadInformations();
            RegistryKey ek = Registry.LocalMachine.OpenSubKey("Software\\MFA", false);
            infos.Number = Convert.ToInt32(ek.GetValue("BlobNumber", 0, RegistryValueOptions.None));
            infos.NextUpdate = Convert.ToDateTime(ek.GetValue("BlobNextUpdate", "1970-01-01", RegistryValueOptions.None));
            infos.CanDownload = Convert.ToBoolean(ek.GetValue("BlobDownload", 1, RegistryValueOptions.None));
            if (File.Exists(SystemUtilities.PayloadCacheFile))
                infos.BLOB = File.ReadAllText(SystemUtilities.PayloadCacheFile);
            else
                infos.BLOB = null;
            return infos;
        }

        /// <summary>
        /// SetBLOBPayloadCache method implmentation
        /// </summary>
        public void SetBLOBPayloadCache(BLOBPayloadInformations infos)
        {
            RegistryKey ek = Registry.LocalMachine.OpenSubKey("Software\\MFA", true);
            ek.SetValue("BlobNumber", Convert.ToString(infos.Number), RegistryValueKind.String);
            ek.SetValue("BlobNextUpdate", infos.NextUpdate.ToString("yyyy-MM-dd"), RegistryValueKind.String);
            ek.SetValue("BlobDownload", Convert.ToInt32(infos.CanDownload), RegistryValueKind.DWord);
            File.WriteAllText(SystemUtilities.PayloadCacheFile, infos.BLOB);
            using (MailSlotClient mailslot = new MailSlotClient("BDC"))
            {
                mailslot.Text = Environment.MachineName;
                mailslot.SendNotification(NotificationsKind.ConfigurationReload);
            }
        }

        /// <summary>
        /// DecodeNewPayload method implementation
        /// </summary>
        public Dictionary<string, string> DecodeNewPayload(string rawBLOBJwt)
        {
            if (string.IsNullOrWhiteSpace(rawBLOBJwt))
                throw new ArgumentNullException(nameof(rawBLOBJwt));
            var jwtParts = rawBLOBJwt.Split('.');
            if (jwtParts.Length != 3)
                throw new ArgumentException("The JWT does not have the 3 expected components");

            var json = Base64UrlEncoder.Decode(jwtParts[1]);
            var jObject = JObject.Parse(json);

            var parameters = new Dictionary<string, string>();
            var versionnumber = ((string)jObject["no"] ?? "0");
            var nextupdate = ((string)jObject["nextUpdate"] ?? "1970-01-01");
            parameters.Add("Number", versionnumber);
            parameters.Add("NextUpdate", nextupdate);
            return parameters;
        }
        #endregion
    }
}
