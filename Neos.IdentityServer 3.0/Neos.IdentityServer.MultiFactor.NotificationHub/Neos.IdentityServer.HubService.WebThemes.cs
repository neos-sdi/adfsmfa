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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;

namespace Neos.IdentityServer.MultiFactor
{
    #region WebThemeManager
    /// <summary>
    /// WebThemeManager class implementation
    /// </summary>
    public class WebThemeManager
    {
        private static RegistryVersion _registry = new RegistryVersion();
        private static WebThemesList _themesList = new WebThemesList();
        private EventLog _log;

        /// <summary>
        /// Constructor implmentation
        /// </summary>
        public WebThemeManager(IDependency dep)
        {
            _log = dep.GetEventLog();
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public bool Initialize(Dictionary<string, bool> servers, WebThemesParametersRecord context, Uri request, out string identifier)
        {
            identifier = string.Empty;
            if (_registry.IsWindows2012R2)
                return false;
            string rpidentifier = RelyingPartyIdentifier.GetIdentifier(request.OriginalString);
            if (string.IsNullOrEmpty(rpidentifier))
                return false;
            try
            {
                context.Identifier = rpidentifier;
                // Theme loaded/available ?
                WebTheme th = GetWebTheme(context);
                if (th != null) 
                {
                    identifier = th.Identifier;
                    return true;
                }
                else
                {
                    // ask other servers for Theme availability
                    WebTheme th2 = (WebTheme)DoRequestTheme(servers, context); 
                    if (th2 != null)  
                    {
                        _themesList.Add(th2);
                        identifier = th2.Identifier;
                        return true;
                    }
                    else if (IsPrimaryComputer(servers))
                    {
                        // Load requested Theme if PrimaryComputer
                        WebTheme th3 = new WebTheme(request);
                        if (th3.LoadTheme(_log))
                        {
                            _themesList.Add(th3);
                            identifier = th3.Identifier;
                            DoDispachTheme(servers, (WebThemeRecord)th3);
                            return true;
                        }
                        else // No valid Theme found
                            identifier = string.Empty;
                    }
                    else // No Theme found
                        identifier = string.Empty;
                }
            }
            catch (Exception ex)
            {
                _log.WriteEntry(string.Format("Error calling  Initialize method : {0}.", ex.Message), EventLogEntryType.Error, 2012);
                identifier = string.Empty;
            }            
            return false;
        }

        /// <summary>
        /// HasRelyingPartyTheme
        /// </summary>
        public bool HasRelyingPartyTheme(WebThemesParametersRecord context)
        {
            if (string.IsNullOrEmpty(context.Identifier))
                return false;
            try
            {
                return (GetWebTheme(context) != null);
            }
            catch (Exception ex)
            {
                _log.WriteEntry(string.Format("Error calling  HasRelyingPartyTheme method : {0}.", ex.Message), EventLogEntryType.Error, 2012);
                return false;
            }
        }

        /// <summary>
        /// GetWebTheme method implementation
        /// </summary>
        private WebTheme GetWebTheme(WebThemesParametersRecord context)
        {            
            try
            {
                return _themesList.Find(context.Identifier);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// GetIllustrationAddress method implementation
        /// </summary>
        public string GetIllustrationAddress(WebThemesParametersRecord context)
        {
            if (string.IsNullOrEmpty(context.Identifier))
                return string.Empty;
            try
            {
                WebTheme th = GetWebTheme(context);
                if (th != null)
                    return th.GetIllustrationAddress(context.LCID);
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                _log.WriteEntry(string.Format("Error calling  GetIllustrationAddress method : {0}.", ex.Message), EventLogEntryType.Error, 2012);
                return string.Empty;
            }
        }

        /// <summary>
        /// GetLogoAddress method implementation
        /// </summary>
        public string GetLogoAddress(WebThemesParametersRecord context)
        {
            if (string.IsNullOrEmpty(context.Identifier))
                return string.Empty;
            try
            {
                WebTheme th = GetWebTheme(context);
                if (th != null)
                    return th.GetLogoAddress(context.LCID);
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                _log.WriteEntry(string.Format("Error calling  GetLogoAddress method : {0}.", ex.Message), EventLogEntryType.Error, 2012);
                return string.Empty;
            }
        }

        /// <summary>
        /// GetStyleSheetAddress method implementation
        /// </summary>
        public string GetStyleSheetAddress(WebThemesParametersRecord context)
        {
            if (string.IsNullOrEmpty(context.Identifier))
                return string.Empty;
            try
            {
                WebTheme th = GetWebTheme(context);
                if (th != null)
                    return th.GetStyleSheetAddress(context.LCID);
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                _log.WriteEntry(string.Format("Error calling  GetStyleSheetAddress method : {0}.", ex.Message), EventLogEntryType.Error, 2012);
                return string.Empty;
            }
        }

        /// <summary>
        /// GetAddresses method implmentation
        /// </summary>
        public Dictionary<WebThemeAddressKind, string> GetAddresses(WebThemesParametersRecord context)
        {
            if (string.IsNullOrEmpty(context.Identifier))
                return null;
            try
            {
                WebTheme th = GetWebTheme(context);
                if (th != null)
                {
                    Dictionary<WebThemeAddressKind, string> dic = new Dictionary<WebThemeAddressKind, string>();
                    dic.Add(WebThemeAddressKind.Illustration, th.GetIllustrationAddress(context.LCID));
                    dic.Add(WebThemeAddressKind.CompanyLogo, th.GetLogoAddress(context.LCID));
                    dic.Add(WebThemeAddressKind.StyleSheet, th.GetStyleSheetAddress(context.LCID));
                    return dic;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                _log.WriteEntry(string.Format("Error calling  GetAddresses method : {0}.", ex.Message), EventLogEntryType.Error, 2012);
                return null;
            }
        }

        /// <summary>
        /// DispachTheme method implementation
        /// </summary>
        public void DispachTheme(WebThemeRecord theme)
        {
            WebTheme th = (WebTheme)theme;
            try
            {
                WebTheme res = _themesList.Find(theme.Identifier);
                if (res == null)
                    _themesList.Add(th);
            }
            catch (Exception)
            {
                
            }
        }

        /// <summary>
        /// RequestTheme method implementation
        /// </summary>
        public WebThemeRecord RequestTheme(WebThemesParametersRecord theme)
        {
            try
            {
                return (WebThemeRecord)GetWebTheme(theme);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// ResetThemes method implmentation
        /// </summary>
        public void ResetThemes()
        {
            try
            {
                _themesList.Clear();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region private methods
        /// <summary>
        /// IsPrimaryComputer method implementation
        /// </summary>
        /// <param name="servers"></param>
        /// <returns></returns>
        private bool IsPrimaryComputer(Dictionary<string, bool> servers)
        {
            string fqdn = Dns.GetHostEntry("localhost").HostName;
            List<string> servernames = (from server in servers
                                        where (server.Key.ToLower() == fqdn.ToLower() && (server.Value == true))
                                        select server.Key.ToLower()).ToList<string>();
            return (servernames.Count == 1);
        }

        /// <summary>
        /// DoDispachTheme method implementation
        /// </summary>
        private void DoDispachTheme(Dictionary<string, bool> servers, WebThemeRecord theme)
        {
            string fqdn = Dns.GetHostEntry("localhost").HostName;
            List<string> servernames = (from server in servers
                                        where (server.Key.ToLower() != fqdn.ToLower())
                                        select server.Key.ToLower()).ToList<string>();
            if (servernames != null)
            {
                foreach (string srvfqdn in servernames)
                {
                    WebThemesClient webthemeclient = new WebThemesClient();
                    try
                    {
                        webthemeclient.Initialize(srvfqdn);
                        IWebThemeManager client = webthemeclient.Open();
                        try
                        {
                            client.DispachTheme(theme);
                        }
                        catch (Exception e)
                        {
                            _log.WriteEntry(string.Format("Error calling  DispachTheme method : {0} => {1}.", srvfqdn, e.Message), EventLogEntryType.Error, 2011);
                        }
                        finally
                        {
                            webthemeclient.Close(client);
                        }
                    }
                    catch (Exception e)
                    {
                        _log.WriteEntry(string.Format("Error calling DispachTheme method : {0} => {1}.", srvfqdn, e.Message), EventLogEntryType.Error, 2011);
                    }
                }
            }
            return;
        }

        /// <summary>
        /// DoRequestTheme method implementation
        /// </summary>
        private WebThemeRecord DoRequestTheme(Dictionary<string, bool> servers, WebThemesParametersRecord theme)
        {
            string fqdn = Dns.GetHostEntry("localhost").HostName;
            List<string> servernames = (from server in servers
                      where (server.Key.ToLower() != fqdn.ToLower() && (server.Value==true))
                      select server.Key.ToLower()).ToList<string>(); 
            if (servernames != null)
            {
                foreach (string srvfqdn in servernames)
                {
                    WebThemesClient webthemeclient = new WebThemesClient();
                    try
                    {
                        webthemeclient.Initialize(srvfqdn);
                        IWebThemeManager client = webthemeclient.Open();
                        try
                        {
                            WebThemeRecord res = client.RequestTheme(theme);
                            if (res != null)
                                return res;
                        }
                        catch (Exception e)
                        {
                            webthemeclient.UnInitialize();
                            _log.WriteEntry(string.Format("Error calling  DispachTheme method : {0} => {1}.", srvfqdn, e.Message), EventLogEntryType.Error, 2011);
                        }
                        finally
                        {
                            webthemeclient.Close(client);
                        }
                    }
                    catch (Exception e)
                    {
                        _log.WriteEntry(string.Format("Error calling  DispachTheme method : {0} => {1}.", srvfqdn, e.Message), EventLogEntryType.Error, 2011);
                        return null;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// ResetThemes method implmentation
        /// </summary>
        public void ResetThemesList(Dictionary<string, bool> servers)
        {
            string fqdn = Dns.GetHostEntry("localhost").HostName;
            List<string> servernames = (from server in servers
                                        where (server.Key.ToLower() != fqdn.ToLower())
                                        select server.Key.ToLower()).ToList<string>();
            ResetThemes();
            if (servernames != null)
            {
                foreach (string srvfqdn in servernames)
                {
                    WebThemesClient webthemeclient = new WebThemesClient();
                    try
                    {
                        webthemeclient.Initialize(srvfqdn);
                        IWebThemeManager client = webthemeclient.Open();
                        try
                        {
                            client.ResetThemes();
                        }
                        catch (Exception e)
                        {
                            _log.WriteEntry(string.Format("Error calling  DispachTheme method : {0} => {1}.", srvfqdn, e.Message), EventLogEntryType.Error, 2011);
                        }
                        finally
                        {
                            webthemeclient.Close(client);
                        }
                    }
                    catch (Exception e)
                    {
                        _log.WriteEntry(string.Format("Error calling DispachTheme method : {0} => {1}.", srvfqdn, e.Message), EventLogEntryType.Error, 2011);
                    }
                }
            }
            return;
        }

        #endregion
    }
    #endregion

    #region BaseWebTheme
    /// <summary>
    /// BaseWebTheme class implementation
    /// </summary>
    internal class BaseWebTheme
    {
        private Dictionary<string, string> _styleSheetUri;
        private Dictionary<string, string> _logoImageUri;
        private Dictionary<string, string> _illustrationImageUri;
        private Dictionary<string, string> _resourceHash;

        /// <summary>
        /// WebTheme constructor
        /// </summary>
        public BaseWebTheme()
        {
            this._styleSheetUri = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this._logoImageUri = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this._illustrationImageUri = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this._resourceHash = new Dictionary<string, string>();
        }

        /// <summary>
        /// BaseWebTheme constructor
        /// </summary>
        public BaseWebTheme(Uri request):this()
        {
            Identifier = RelyingPartyIdentifier.GetIdentifier(request.OriginalString);
        }

        /// <summary>
        /// Name property implementation
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Identifier property implementation
        /// </summary>
        public string Identifier { get; internal set; }

        /// <summary>
        /// IsBuiltinTheme property implementation
        /// </summary>
        public bool IsBuiltinTheme { get; set; } = false;

        /// <summary>
        /// StyleSheetUri property implementation
        /// </summary>
        internal IDictionary<string, string> StyleSheetUri
        {
            get { return this._styleSheetUri; }
            set
            {
                this._styleSheetUri.Clear();
                foreach (var it in value)
                {
                    this._styleSheetUri.Add(it.Key, it.Value);
                }
            }
        }

        /// <summary>
        /// LogoImageUri property implementation
        /// </summary>
        internal IDictionary<string, string> LogoImageUri
        {
            get { return this._logoImageUri; }
            set
            {
                this._logoImageUri.Clear();
                foreach (var it in value)
                {
                    this._logoImageUri.Add(it.Key, it.Value);
                }
            }
        }

        /// <summary>
        /// FileResources method implementation
        /// </summary>
        internal IDictionary<string, string> IllustrationImageUri
        {
            get { return this._illustrationImageUri; }
            set
            {
                this._illustrationImageUri.Clear();
                foreach (var it in value)
                {
                    this._illustrationImageUri.Add(it.Key, it.Value);
                }
            }
        }

        /// <summary>
        /// HasLoaded property
        /// </summary>
        public bool HasLoaded { get; set; }

        /// <summary>
        /// implicit operator WebTheme
        /// </summary>
        public static explicit operator BaseWebTheme(WebThemeRecord webrec)
        {
            if (webrec == null)
                return null;
            WebTheme w = new WebTheme()
            {
                Name = webrec.Name,
                Identifier = webrec.Identifier,
                StyleSheetUri = webrec.StyleSheetUri,
                LogoImageUri = webrec.LogoImageUri,
                IllustrationImageUri = webrec.IllustrationImageUri
            };
            return w;
        }

        /// <summary>
        /// implicit operator WebTheme
        /// </summary>
        public static explicit operator WebThemeRecord(BaseWebTheme webthem)
        {
            if (webthem == null)
                return null;
            WebThemeRecord w = new WebThemeRecord()
            {
                Name = webthem.Name,
                Identifier = webthem.Identifier,
                StyleSheetUri = webthem.StyleSheetUri,
                LogoImageUri = webthem.LogoImageUri,
                IllustrationImageUri = webthem.IllustrationImageUri
            };
            return w;
        }

        /// <summary>
        /// GetMainRessourceHash method implementation
        /// </summary>
        protected string GetResourceHash(string resourceAddress)
        {
            string text;
            if (!this._resourceHash.TryGetValue(resourceAddress, out text))
            {
                byte[] address = UTF8Encoding.UTF8.GetBytes(resourceAddress);
                text = BitConverter.ToString(SHA256.Create().ComputeHash(address)).Replace("-", "");
                Dictionary<string, string> resourceHash = this._resourceHash;
                lock (resourceHash)
                {
                    if (!this._resourceHash.ContainsKey(resourceAddress))
                    {
                        this._resourceHash.Add(resourceAddress, text);
                    }
                }
            }
            return text;
        }

        /// <summary>
        /// GetResourceAddress method implementation
        /// </summary>
        protected virtual string GetResourceAddress(string resourceAddress)
        {
            string resourceHash = this.GetResourceHash(resourceAddress);
            if (!string.IsNullOrEmpty(resourceHash))
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}?id={1}", resourceAddress, resourceHash);
            }
            return resourceAddress;
        }

        /// <summary>
        /// GetIllustrationAddress method implementation
        /// </summary>
        public virtual string GetIllustrationAddress(int LCID)
        {
            foreach (KeyValuePair<string, string> id in this.IllustrationImageUri)
            {
                CultureInfo c = new CultureInfo(id.Key);
                if (c.LCID.Equals(LCID))
                    return GetResourceAddress(id.Value);
            }
            foreach (KeyValuePair<string, string> id in this.IllustrationImageUri)
            {
                if (string.IsNullOrEmpty(id.Key))
                    return GetResourceAddress(id.Value);
            }
            return string.Empty;
        }

        /// <summary>
        /// GetLogoAddress method implementation
        /// </summary>
        public virtual string GetLogoAddress(int LCID)
        {
            foreach (KeyValuePair<string, string> id in this.LogoImageUri)
            {
                CultureInfo c = new CultureInfo(id.Key);
                if (c.LCID.Equals(LCID))
                    return GetResourceAddress(id.Value);
            }
            foreach (KeyValuePair<string, string> id in this.LogoImageUri)
            {
                if (string.IsNullOrEmpty(id.Key))
                    return GetResourceAddress(id.Value);
            }
            return string.Empty;
        }

        /// <summary>
        /// GetStyleSheetAddress
        /// </summary>
        public virtual string GetStyleSheetAddress(int LCID)
        {
            foreach (KeyValuePair<string, string> id in this.StyleSheetUri)
            {
                CultureInfo c = new CultureInfo(id.Key);
                if (c.LCID.Equals(LCID))
                    return GetResourceAddress(id.Value);
            }
            foreach (KeyValuePair<string, string> id in this.StyleSheetUri)
            {
                if (string.IsNullOrEmpty(id.Key))
                    return GetResourceAddress(id.Value);
            }
            return string.Empty;
        }

        /// <summary>
        /// LoadTheme method implementation
        /// </summary>
        internal bool LoadTheme(EventLog log)
        {
            HasLoaded = false;
            try
            {
                GetThemeName();
                if (!string.IsNullOrEmpty(this.Name))
                {
                    if (HasLoaded)
                        LoadThemeProps();
                    if (HasLoaded)
                        LoadThemeResources();
                    return HasLoaded;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                log.WriteEntry(string.Format("Error Loading Theme method : {0}.", ex.Message), EventLogEntryType.Error, 2013);
                return false;
            }
        }

        /// <summary>
        /// GetThemeName method
        /// </summary>
        protected virtual void GetThemeName()
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            this.Name = string.Empty;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("(Get-ADFSWebConfig).ActiveThemeName", true);
                pipeline.Commands.Add(exportcmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();
                foreach (var result in PSOutput)
                {
                    if (result != null)
                    {
                        this.Name = result.BaseObject.ToString();
                        HasLoaded = true;
                        break;
                    }
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
            return;
        }

        /// <summary>
        /// LoadThemeProps method
        /// </summary>
        protected virtual void LoadThemeProps()
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            string activetheme = string.Empty;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("(Get-AdfsWebTheme -Name '" + this.Name + "').IsbuiltinTheme", true);
                pipeline.Commands.Add(exportcmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();

                foreach (var result in PSOutput)
                {
                    if (result != null)
                    {
                        this.IsBuiltinTheme = Convert.ToBoolean(result.BaseObject);
                        HasLoaded = true;
                        break;
                    }
                    else
                        HasLoaded = false;
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
        }

        /// <summary>
        /// LoadThemeIllustrations method
        /// </summary>
        protected virtual void LoadThemeResources()
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            string activetheme = string.Empty;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Command illstrationscmd = new Command("(Get-AdfsWebTheme -RelyingPartyName '" + this.Name + "').Illustration", true);
                Pipeline pipeline1 = SPRunSpace.CreatePipeline();
                pipeline1.Commands.Add(illstrationscmd);
                Command logoscmd = new Command("(Get-AdfsWebTheme -RelyingPartyName '" + this.Name + "').Logo", true);
                Pipeline pipeline2 = SPRunSpace.CreatePipeline();
                pipeline2.Commands.Add(logoscmd);
                Command stylesheetscmd = new Command("(Get-AdfsWebTheme -RelyingPartyName '" + this.Name + "').StyleSheet", true);
                Pipeline pipeline3 = SPRunSpace.CreatePipeline();
                pipeline3.Commands.Add(stylesheetscmd);

                this.IllustrationImageUri.Clear();
                this.LogoImageUri.Clear();
                StyleSheetUri.Clear();

                Collection<PSObject> PSOutput1 = pipeline1.Invoke();
                Collection<PSObject> PSOutput2 = pipeline2.Invoke();
                Collection<PSObject> PSOutput3 = pipeline3.Invoke();
                foreach (var result in PSOutput1)
                {
                    if (result != null)
                    {
                        byte[] data = (byte[])result.Properties["Value"].Value;
                        string ext = ImagesFilesHelper.GetKnownFileType(data);
                        string lng = result.Properties["Key"].Value.ToString();
                        if (string.IsNullOrEmpty(lng))
                            this.IllustrationImageUri.Add(result.Properties["Key"].Value.ToString(), "/adfs/portal/illustration/illustration." + ext);
                        else
                            this.IllustrationImageUri.Add(result.Properties["Key"].Value.ToString(), "/adfs/portal/illustration/illustration." + lng + "." + ext);
                        HasLoaded = true;
                    }
                    else
                    {
                        HasLoaded = false;
                        return;
                    }
                }
                foreach (var result in PSOutput2)
                {
                    if (result != null)
                    {
                        byte[] data = (byte[])result.Properties["Value"].Value;
                        string ext = ImagesFilesHelper.GetKnownFileType(data);
                        string lng = result.Properties["Key"].Value.ToString();
                        if (string.IsNullOrEmpty(lng))
                            this.LogoImageUri.Add(result.Properties["Key"].Value.ToString(), "/adfs/portal/logo/logo." + ext);
                        else
                            this.LogoImageUri.Add(result.Properties["Key"].Value.ToString(), "/adfs/portal/logo/logo." + lng + "." + ext);
                        HasLoaded = true;
                    }
                    else
                    {
                        HasLoaded = false;
                        return;
                    }
                }
                foreach (var result in PSOutput3)
                {
                    if (result != null)
                    {
                        string ext = "css";
                        string lng = result.Properties["Key"].Value.ToString();
                        if (string.IsNullOrEmpty(lng))
                            this.StyleSheetUri.Add(result.Properties["Key"].Value.ToString(), "/adfs/portal/css/style." + ext);
                        else
                            this.StyleSheetUri.Add(result.Properties["Key"].Value.ToString(), "/adfs/portal/css/style." + lng + "." + ext);
                        HasLoaded = true;
                    }
                    else
                    {
                        HasLoaded = false;
                        return;
                    }
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
        }
    }
    #endregion

    #region WebTheme
    /// <summary>
    /// RPWebTheme class implementation
    /// </summary>
    internal class WebTheme : BaseWebTheme
    {
        /// <summary>
        /// RPWebTheme constructor
        /// </summary>
        public WebTheme(): base()
        {

        }

        /// <summary>
        /// RPWebTheme constructor
        /// </summary>
        public WebTheme(Uri request) : base(request)
        {

        }

        /// <summary>
        /// ObjectId property implementation
        /// </summary>
        public string ObjectId { get; set; }

        /// <summary>
        /// implicit operator WebTheme
        /// </summary>
        public static explicit operator WebTheme(WebThemeRecord webrec)
        {
            if (webrec == null)
                return null;
            WebTheme w = new WebTheme()
            {
                Name = webrec.Name,
                ObjectId = webrec.ObjectId,
                Identifier = webrec.Identifier,
                StyleSheetUri = webrec.StyleSheetUri,
                LogoImageUri = webrec.LogoImageUri,
                IllustrationImageUri = webrec.IllustrationImageUri
            };
            return w;
        }

        /// <summary>
        /// implicit operator WebTheme
        /// </summary>
        public static explicit operator WebThemeRecord(WebTheme webthem)
        {
            if (webthem == null)
                return null;
            WebThemeRecord w = new WebThemeRecord()
            {
                Name = webthem.Name,
                ObjectId = webthem.ObjectId,
                Identifier = webthem.Identifier,
                StyleSheetUri = webthem.StyleSheetUri,
                LogoImageUri = webthem.LogoImageUri,
                IllustrationImageUri = webthem.IllustrationImageUri
            };
            return w;
        }
        /// <summary>
        /// GetMainResourceAddress method implementation
        /// </summary>
        protected override string GetResourceAddress(string resourceAddress)
        {
            string resourceHash = this.GetResourceHash(resourceAddress);
            if (!string.IsNullOrEmpty(resourceHash))
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}?id={1}&rp={2}", resourceAddress, resourceHash, this.ObjectId);
            }
            return resourceAddress;
        }

        /// <summary>
        /// GetThemeName method
        /// </summary>
        protected override void GetThemeName()
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            this.Name = string.Empty;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("Get-AdfsRelyingPartyTrust -Identifier '" + this.Identifier + "' | select-object -Property Name, ObjectIdentifier", true);
                pipeline.Commands.Add(exportcmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();
                foreach (var result in PSOutput)
                {
                    if (result != null)
                    {
                        this.Name = result.Properties["Name"].Value.ToString();
                        this.ObjectId = result.Properties["ObjectIdentifier"].Value.ToString();
                        HasLoaded = true;
                        break;
                    }
                    else
                        HasLoaded = false;
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
            return;
        }

        /// <summary>
        /// LoadThemeProps method
        /// </summary>
        protected override void LoadThemeProps()
        {
            return;
        }

        /// <summary>
        /// LoadThemeIllustrations method
        /// </summary>
        protected override void LoadThemeResources()
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            string activetheme = string.Empty;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Command illstrationscmd = new Command("(Get-AdfsRelyingPartyWebTheme -RelyingPartyName '" + this.Name + "').Illustration", true);
                Pipeline pipeline1 = SPRunSpace.CreatePipeline();
                pipeline1.Commands.Add(illstrationscmd);
                Command logoscmd = new Command("(Get-AdfsRelyingPartyWebTheme -RelyingPartyName '" + this.Name + "').Logo", true);
                Pipeline pipeline2 = SPRunSpace.CreatePipeline();
                pipeline2.Commands.Add(logoscmd);
                Command stylesheetscmd = new Command("(Get-AdfsRelyingPartyWebTheme -RelyingPartyName '" + this.Name + "').StyleSheet", true);
                Pipeline pipeline3 = SPRunSpace.CreatePipeline();
                pipeline3.Commands.Add(stylesheetscmd);

                this.IllustrationImageUri.Clear();
                this.LogoImageUri.Clear();
                StyleSheetUri.Clear();

                Collection<PSObject> PSOutput1 = pipeline1.Invoke();
                Collection<PSObject> PSOutput2 = pipeline2.Invoke();
                Collection<PSObject> PSOutput3 = pipeline3.Invoke();
                foreach (var result in PSOutput1)
                {
                    if (result != null)
                    {
                        byte[] data = (byte[])result.Properties["Value"].Value;
                        string ext = ImagesFilesHelper.GetKnownFileType(data);
                        string lng = result.Properties["Key"].Value.ToString();
                        if (string.IsNullOrEmpty(lng))
                            this.IllustrationImageUri.Add(result.Properties["Key"].Value.ToString(), "/adfs/portal/illustration/illustration." + ext);
                        else
                            this.IllustrationImageUri.Add(result.Properties["Key"].Value.ToString(), "/adfs/portal/illustration/illustration." + lng + "." + ext);
                        HasLoaded = true;
                    }
                    else
                    {
                        HasLoaded = false;
                        return;
                    }
                }
                foreach (var result in PSOutput2)
                {
                    if (result != null)
                    {
                        byte[] data = (byte[])result.Properties["Value"].Value;
                        string ext = ImagesFilesHelper.GetKnownFileType(data);
                        string lng = result.Properties["Key"].Value.ToString();
                        if (string.IsNullOrEmpty(lng))
                            this.LogoImageUri.Add(result.Properties["Key"].Value.ToString(), "/adfs/portal/logo/logo." + ext);
                        else
                            this.LogoImageUri.Add(result.Properties["Key"].Value.ToString(), "/adfs/portal/logo/logo." + lng + "." + ext);
                        HasLoaded = true;
                    }
                    else
                    {
                        HasLoaded = false;
                        return;
                    }
                }
                foreach (var result in PSOutput3)
                {
                    if (result != null)
                    {
                        string ext = "css";
                        string lng = result.Properties["Key"].Value.ToString();
                        if (string.IsNullOrEmpty(lng))
                            this.StyleSheetUri.Add(result.Properties["Key"].Value.ToString(), "/adfs/portal/css/style." + ext);
                        else
                            this.StyleSheetUri.Add(result.Properties["Key"].Value.ToString(), "/adfs/portal/css/style." + lng + "." + ext);
                        HasLoaded = true;
                    }
                    else
                    {
                        HasLoaded = false;
                        return;
                    }
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
        }
    }
    #endregion

    #region ImagesFilesHelper
    /// <summary>
    /// ImagesFilesHelper class implementation
    /// </summary>
    internal static class ImagesFilesHelper
    {
        private static readonly Dictionary<string, byte[]> KnownFileHeaders = new Dictionary<string, byte[]>()
        {
            { "jpg", new byte[]{ 0xFF, 0xD8 }}, // JPEG
		    { "png", new byte[]{ 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }}, // PNG
            // { "bmp", new byte[]{ 0x42, 0x4D }}, // BMP
		    // { "gif", new byte[]{ 0x47, 0x49, 0x46 }}, // GIF
	    };

        internal static string GetKnownFileType(byte[] data)
        {
            foreach (var check in KnownFileHeaders)
            {
                byte[] slice = new byte[check.Value.Length];
                Buffer.BlockCopy(data, 0, slice, 0, check.Value.Length);
                if (slice.SequenceEqual(check.Value))
                {
                    return check.Key;
                }
            }
            return "png";
        }
    }
    #endregion

    #region RelyingPartyIdentifier
    /// <summary>
    /// RelyingPartyIdentifier class implementation
    /// </summary>
    internal static class RelyingPartyIdentifier
    {
        public static string GetIdentifier(string request)
        {
            string identifier = string.Empty;
            try { identifier = GetWSFedIdentifier(request); }
            catch (Exception) { }
            if (!string.IsNullOrEmpty(identifier))
                return identifier;
            try { identifier = GetSAMLPIdentifier(request); }
            catch (Exception) { }
            return identifier;
        }

        /// <summary>
        /// GetSAMLPIdentifier method implementation
        /// </summary>
        internal static string GetSAMLPIdentifier(string request)
        {
            var utf8 = Encoding.UTF8;
            Uri SAMLUri = new Uri(request);
            string[] parts = SAMLUri.Query.Split(new char[] { '?', '&' });
            string encodedRequest = string.Empty;
            string decodedrequest = string.Empty;
            string returnvalue = string.Empty;
            foreach (string itm in parts)
            {
                string[] subparts = itm.Split(new char[] { '=' });
                if (subparts[0].ToLower().Equals("samlrequest"))
                {
                    encodedRequest = subparts[1];
                    break;
                }
            }

            var bytes = Convert.FromBase64String(HttpUtility.UrlDecode(encodedRequest));
            using (var output = new MemoryStream())
            {
                using (var input = new MemoryStream(bytes))
                {
                    using (var unzip = new DeflateStream(input, CompressionMode.Decompress))
                    {
                        unzip.CopyTo(output, bytes.Length);
                        unzip.Close();
                    }
                    decodedrequest = utf8.GetString(output.ToArray());
                }
            }

            if (!string.IsNullOrEmpty(decodedrequest))
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(decodedrequest);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("samlp", xml.DocumentElement.NamespaceURI);
                nsmgr.AddNamespace("saml", xml.DocumentElement.NamespaceURI);

                XmlNode root = xml.SelectSingleNode("/samlp:AuthnRequest", nsmgr);
                foreach (XmlNode x in root.ChildNodes)
                {
                    if (x.Name.ToLower().Equals("saml:issuer"))
                    {
                        returnvalue = x.InnerText;
                        break;
                    }
                }
            }
            return returnvalue;
        }

        /// <summary>
        /// GetWSFedIdentifier method implementation
        /// </summary>
        internal static string GetWSFedIdentifier(string request)
        {
            Uri WSFEdUri = new Uri(request);
            string[] parts = WSFEdUri.Query.Split(new char[] { '?', '&' });

            foreach (string itm in parts)
            {
                string[] subparts = itm.Split(new char[] { '=' });
                if (subparts[0].ToLower().Equals("wtrealm"))
                {
                    return HttpUtility.UrlDecode(subparts[1]);
                }
            }
            return string.Empty;
        }
    }
    #endregion

    #region WebThemesList
    /// <summary>
    /// WebThemesList class implementation
    /// </summary>
    internal class WebThemesList
    {
        private List<WebTheme> _themes = new List<WebTheme>();
        private static readonly object _lock = new object();

        /// <summary>
        /// Clear method implementation
        /// </summary>
        internal void Clear()
        {
            lock (_lock)
            {
                _themes.Clear();
            }
        }

        /// <summary>
        /// Find method implmentation
        /// </summary>
        internal WebTheme Find(string identifier)
        {
            lock (_lock)
            {
                try
                {
                    return _themes.First(s => s.Identifier.ToLower().Equals(identifier.ToLower()));
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Add method implmentation
        /// </summary>
        internal void Add(WebTheme theme)
        {
            lock (_lock)
            {
                try
                {
                    WebTheme exists = Find(theme.Identifier);
                    if (exists != null)
                        _themes.Remove(exists);
                    _themes.Add(theme);
                }
                catch
                {
                    _themes.Add(theme);
                }
            }
        }
    }
    #endregion
}
