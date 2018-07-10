//******************************************************************************************************************************************************************************************//
// Copyright (c) 2017 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
using Microsoft.Win32;
using Neos.IdentityServer.MultiFactor.Data;
using Neos.IdentityServer.MultiFactor.QrEncoding;
using Neos.IdentityServer.MultiFactor.QrEncoding.Windows.Render;
using Neos.IdentityServer.MultiFactor.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Xml.Serialization;


namespace Neos.IdentityServer.MultiFactor
{
    /// <summary>
    /// RuntimeAuthProvider
    /// </summary>
    internal static class RuntimeAuthProvider
    {
        private static Dictionary<PreferredMethod, IExternalProvider> _lst = new Dictionary<PreferredMethod, IExternalProvider>();
        /// <summary>
        /// GetAuthenticationProvider method implementation
        /// </summary>
        internal static IExternalProvider GetAuthenticationProvider(MFAConfig cfg, AuthenticationContext ctx)
        {
            IExternalProvider provider = null;
            switch (ctx.PreferredMethod)
            {
                case PreferredMethod.Code:
                    provider = GetProviderInstance(ctx.PreferredMethod);
                    if (provider == null)
                    {
                        provider = new NeosOTPProvider();
                        AddOrUpdateProvider(ctx.PreferredMethod, provider);
                        provider.Initialize(new OTPProviderParams(cfg.OTPProvider));
                    }
                    break;
                case PreferredMethod.Email:
                    provider = GetProviderInstance(ctx.PreferredMethod);
                    if (provider == null)
                    {
                        provider = new NeosMailProvider();
                        AddOrUpdateProvider(ctx.PreferredMethod, provider);
                        provider.Initialize(new MailProviderParams(cfg.MailProvider));
                    }
                    break;
                case PreferredMethod.External:
                    provider = GetProviderInstance(ctx.PreferredMethod);
                    if (provider == null)
                    {
                        try
                        {
                            if (IsLegacyExternalWrapper(cfg.ExternalProvider.FullQualifiedImplementation))
                                provider = new NeosLegacySMSProvider();
                            else
                                provider = LoadExternalProvider(cfg.ExternalProvider.FullQualifiedImplementation);
                            AddOrUpdateProvider(ctx.PreferredMethod, provider);
                            provider.Initialize(new ExternalProviderParams(cfg.ExternalProvider));
                        }
                        catch (Exception)
                        {
                            provider = null;
                        }
                    }
                    break;
                case PreferredMethod.Azure:
                    provider = GetProviderInstance(ctx.PreferredMethod);
                    if (provider == null)
                    {
                        try
                        {
                            provider = LoadAzureProvider();
                            AddOrUpdateProvider(ctx.PreferredMethod, provider);
                            provider.Initialize(new AzureProviderParams(cfg.AzureProvider, cfg.Hosts.ADFSFarm.FarmIdentifier, cfg.Issuer));
                        }
                        catch (Exception)
                        {
                            provider = null;
                        }
                    }
                    break;
                case PreferredMethod.Biometrics:
                    return null;
                default:
                    return null;
            }
            if (!IsProviderAvailableForUser(ctx, ctx.PreferredMethod))
                return null;
            return provider;
        }

        /// <summary>
        /// GetAdministrativeProvider method provider
        /// </summary>
        internal static IExternalAdminProvider GetAdministrativeProvider(MFAConfig cfg)
        {
            IExternalAdminProvider provider = new NeosAdministrationProvider();
            if (provider != null)
            {
                provider.Initialize(cfg);
                return provider;
            }
            else
                return null;
        }

        /// <summary>
        /// LoadAzureProvider method implmentation
        /// </summary>
        private static IExternalProvider LoadAzureProvider()
        {
            Assembly assembly = Assembly.Load(@"Neos.IdentityServer.MultiFactor.SAS.Azure, Version=2.2.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2");
            Type _typetoload = assembly.GetType(@"Neos.IdentityServer.MultiFactor.SAS.NeosAzureProvider");
            if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IExternalProvider") != null)
                return (IExternalProvider)Activator.CreateInstance(_typetoload, true); // Allow Calling internal Constructors
            else
                return null;
        }

        /// <summary>
        /// LoadExternalProvider method implmentation
        /// </summary>
        private static IExternalProvider LoadExternalProvider(string AssemblyFulldescription)
        {
            Assembly assembly = Assembly.Load(Utilities.ParseAssembly(AssemblyFulldescription));
            Type _typetoload = assembly.GetType(Utilities.ParseType(AssemblyFulldescription));

            if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IExternalProvider") != null)
                return (IExternalProvider)Activator.CreateInstance(_typetoload, true); // Allow Calling internal Constructors
            else
                return null;
        }

        /// <summary>
        /// IsLegacyExternalWrapper method implmentation
        /// </summary>
        private static bool IsLegacyExternalWrapper(string AssemblyFulldescription)
        {
            Assembly assembly = Assembly.Load(Utilities.ParseAssembly(AssemblyFulldescription));
            Type _typetoload = assembly.GetType(Utilities.ParseType(AssemblyFulldescription));

            if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IExternalProvider") != null)
                return false;
            else if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IExternalOTPProvider") != null)
                return true;
            else
                return false;
        }

        /// <summary>
        /// EnableProvider method implementation
        /// </summary>
        private static void AddOrUpdateProvider(PreferredMethod method, IExternalProvider prov)
        {
            if (prov == null)
                return;
            if (GetProviderInstance(method) == null)
                _lst.Add(method, prov);
            else
                _lst[method] = prov;
        }

        /// <summary>
        /// DisableProvider method provider
        /// </summary>
        private static void RemoveProvider(PreferredMethod method)
        {
            if (GetProviderInstance(method) != null)
                _lst.Remove(method);
        }

        /// <summary>
        /// GetProvider method provider
        /// </summary>
        internal static IExternalProvider GetProvider(PreferredMethod method)
        {
            IExternalProvider pp =  GetProviderInstance(method);
            if ((pp != null) && (pp.Enabled))
               return pp;
            else
                return null;
        }

        /// <summary>
        /// GetProviderInstance method provider
        /// </summary>
        internal static IExternalProvider GetProviderInstance(PreferredMethod method)
        {
            if (_lst.ContainsKey(method))
                return _lst.First(x => x.Key == method).Value;
            return null;
        }

        /// <summary>
        /// IsProviderAvailable method provider
        /// </summary>
        internal static bool IsProviderAvailable(AuthenticationContext ctx, PreferredMethod method)
        {
            IExternalProvider prov = GetProvider(method);
            if (prov == null)
                return false;
            else
                return prov.IsAvailable(ctx);
        }

        /// <summary>
        /// GetProvider method provider
        /// </summary>
        internal static bool IsProviderAvailableForUser(AuthenticationContext ctx, PreferredMethod method)
        {
            IExternalProvider prov = GetProvider(method);
            if (prov == null)
                return false;
            else
            {
                if (prov.PinRequired)
                {
                    if (ctx.PinCode < 0)
                        return false;
                }
                return prov.IsAvailableForUser(ctx);
            }
        }

        /// <summary>
        /// Providers property implementation
        /// </summary>
        private static Dictionary<PreferredMethod, IExternalProvider> Providers
        {
            get { return _lst; }
        }

        /// <summary>
        /// LoadProviders() method implementation
        /// </summary>
        internal static void LoadProviders(MFAConfig cfg)
        {
            _lst.Clear();
            foreach (PreferredMethod meth in Enum.GetValues(typeof(PreferredMethod)))
            {
                IExternalProvider provider = null;
                switch (meth)
                {
                    case PreferredMethod.Code:
                        provider = GetProviderInstance(meth);
                        if (provider == null)
                        {
                            provider = new NeosOTPProvider();
                            AddOrUpdateProvider(meth, provider);
                            provider.Initialize(new OTPProviderParams(cfg.OTPProvider));
                        }
                        break;
                    case PreferredMethod.Email:
                        provider = GetProviderInstance(meth);
                        if (provider == null)
                        {
                            provider = new NeosMailProvider();
                            AddOrUpdateProvider(meth, provider);
                            provider.Initialize(new MailProviderParams(cfg.MailProvider));
                        }
                        break;
                    case PreferredMethod.External:
                        provider = GetProviderInstance(meth);
                        if (provider == null)
                        {
                            try
                            {
                                if (IsLegacyExternalWrapper(cfg.ExternalProvider.FullQualifiedImplementation))
                                    provider = new NeosLegacySMSProvider();
                                else
                                    provider = LoadExternalProvider(cfg.ExternalProvider.FullQualifiedImplementation);
                                AddOrUpdateProvider(meth, provider);
                                provider.Initialize(new ExternalProviderParams(cfg.ExternalProvider));
                            }
                            catch (Exception)
                            {
                                provider = null;
                            }
                        }
                        break;
                    case PreferredMethod.Azure:
                        provider = GetProviderInstance(meth);
                        if (provider == null)
                        {
                            try
                            {
                                provider = LoadAzureProvider();
                                AddOrUpdateProvider(meth, provider);
                                provider.Initialize(new AzureProviderParams(cfg.AzureProvider, cfg.Hosts.ADFSFarm.FarmIdentifier, cfg.Issuer));
                            }
                            catch (Exception)
                            {
                                provider = null;
                            }
                        }
                        break;
                    case PreferredMethod.Biometrics:
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// ResetProviders() method implementation
        /// </summary>
        internal static void ResetProviders()
        {
            _lst.Clear();
        }

        /// <summary>
        /// IsUIElementRequired method implementation
        /// </summary>
        internal static bool IsUIElementRequired(AuthenticationContext ctx, RequiredMethodElements element)
        {
            foreach (KeyValuePair<PreferredMethod, IExternalProvider> prov in _lst)
            {
                if ((prov.Value.Enabled) && (prov.Value.IsUIElementRequired(ctx, element)))
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// RepositoryService class implementation
    /// </summary>
    internal static class RuntimeRepository
    {
        private static MailSlotServer _mailslotserver;

        /// <summary>
        /// MailslotServer property implementation
        /// </summary>
        internal static MailSlotServer MailslotServer
        {
            get
            {
                if (_mailslotserver == null)
                {
                    _mailslotserver = new MailSlotServer("MFA");
                }
                return _mailslotserver;
            }
        }

        #region Registrations 
        /// <summary>
        /// KeyDataEvent method implementation
        /// </summary>
        internal static void KeyDataEvent(string user, KeysDataManagerEventKind kind)
        {
            switch (kind)
            {
                case KeysDataManagerEventKind.add:
                    KeysManager.NewKey(user);
                    break;
                case KeysDataManagerEventKind.Get:
                    KeysManager.ReadKey(user);
                    break;
                case KeysDataManagerEventKind.Remove:
                    KeysManager.RemoveKey(user);
                    break;
            }
        }

        /// <summary>
        /// GetUserRegistration method implementation
        /// </summary>
        internal static Registration GetUserRegistration(MFAConfig cfg, string upn)
        {
            DataRepositoryService client = null;
            if (cfg.UseActiveDirectory)
                client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
            else
                client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
            client.OnKeyDataEvent += KeyDataEvent;
            return client.GetUserRegistration(upn);
        }

        /// <summary>
        /// SetUserRegistration method implementation
        /// </summary>
        internal static Registration SetUserRegistration(MFAConfig cfg, Registration reg, bool resetkey = false)
        {
            DataRepositoryService client = null;
            if (cfg.UseActiveDirectory)
                client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
            else
                client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
            client.OnKeyDataEvent += KeyDataEvent;
            if (reg.PIN <= 0)
                reg.PIN = cfg.DefaultPin;
            return client.SetUserRegistration(reg, resetkey);
        }

        /// <summary>
        /// AddUserRegistration method implementation
        /// </summary>
        internal static Registration AddUserRegistration(MFAConfig cfg, Registration reg, bool addkey = true)
        {
            DataRepositoryService client = null;
            if (cfg.UseActiveDirectory)
                client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
            else
                client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
            if (reg.PIN <= 0)
                reg.PIN = cfg.DefaultPin;
            client.OnKeyDataEvent += KeyDataEvent;
            return client.AddUserRegistration(reg, addkey);
        }

        /// <summary>
        /// DeleteUserRegistration method implementation
        /// </summary>
        internal static bool DeleteUserRegistration(MFAConfig cfg, Registration reg, bool dropkey = true)
        {
            DataRepositoryService client = null;
            if (cfg.UseActiveDirectory)
                client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
            else
                client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
            client.OnKeyDataEvent += KeyDataEvent;
            return client.DeleteUserRegistration(reg, dropkey);
        }

        /// <summary>
        /// EnableUserRegistration method implementation
        /// </summary>
        internal static Registration EnableUserRegistration(MFAConfig cfg, Registration reg)
        {
            DataRepositoryService client = null;
            if (cfg.UseActiveDirectory)
                client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
            else
                client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
            client.OnKeyDataEvent += KeyDataEvent;
            return client.EnableUserRegistration(reg);
        }

        /// <summary>
        /// DisableUserRegistration method implementation
        /// </summary>
        internal static Registration DisableUserRegistration(MFAConfig cfg, Registration reg)
        {
            DataRepositoryService client = null;
            if (cfg.UseActiveDirectory)
                client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
            else
                client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
            client.OnKeyDataEvent += KeyDataEvent;
            return client.DisableUserRegistration(reg);
        }

        /// <summary>
        /// GetUserRegistrations method implementation
        /// </summary>
        internal static RegistrationList GetUserRegistrations(MFAConfig cfg, DataFilterObject filter, DataOrderObject order, DataPagingObject paging, int maxrows = 20000)
        {
            DataRepositoryService client = null;
            if (cfg.UseActiveDirectory)
                client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
            else
                client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
            client.OnKeyDataEvent += KeyDataEvent;
            return client.GetUserRegistrations(filter, order, paging, maxrows);
        }

        /// <summary>
        /// GetAllUserRegistrations method implementation
        /// </summary>
        internal static RegistrationList GetAllUserRegistrations(MFAConfig cfg, DataOrderObject order, int maxrows = 20000, bool enabledonly = false)
        {
            DataRepositoryService client = null;
            if (cfg.UseActiveDirectory)
                client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
            else
                client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
            client.OnKeyDataEvent += KeyDataEvent;
            return client.GetAllUserRegistrations(order, maxrows, enabledonly);
        }

        /// <summary>
        /// GetUserRegistrationsCount method implementation
        /// </summary>
        internal static int GetUserRegistrationsCount(MFAConfig cfg, DataFilterObject filter)
        {
            DataRepositoryService client = null;
            if (cfg.UseActiveDirectory)
                client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
            else
                client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
            client.OnKeyDataEvent += KeyDataEvent;
            return client.GetUserRegistrationsCount(filter);
        }

        /// <summary>
        /// ImportUsers method implementation
        /// </summary>
        internal static void ImportADDSUsers(MFAConfig cfg, string ldappath, bool enable)
        {
            string filename = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)+"\\MFA\\adimport_"+DateTime.UtcNow.ToFileTimeUtc().ToString()+".log";
            TraceListener listen = InitializeTrace(filename);
            try
            {
                Trace.WriteLine("");
                Trace.WriteLine(string.Format("Importing for AD : {0}", ldappath));
                Trace.Indent();
                Trace.WriteLine("Querying users from AD");
                DataRepositoryService client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                RegistrationList lst = client.GetImportUserRegistrations(ldappath, enable);
                Trace.WriteLine(string.Format("Querying return {0} users from AD", lst.Count.ToString()));
                DataRepositoryService client2 = null;
                if (cfg.UseActiveDirectory)
                {
                    Trace.WriteLine("");
                    Trace.WriteLine("Importing ADDS Mode");
                    Trace.Indent();
                    client2 = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                }
                else
                {
                    Trace.WriteLine("");
                    Trace.WriteLine("Importing SQL Mode");
                    Trace.Indent();
                    client2 = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                }
                client2.OnKeyDataEvent += KeyDataEvent;
                foreach (Registration reg in lst)
                {
                    
                    if (enable)
                        reg.Enabled = true;
                    if (!client2.HasRegistration(reg.UPN))
                    {
                        Trace.TraceInformation(string.Format("Importing user {0} from AD", reg.UPN));
                        client2.AddUserRegistration(reg);
                        Trace.TraceInformation(string.Format("User {0} Imported in MFA", reg.UPN));
                    }
                    else
                        Trace.TraceWarning(string.Format("User {0} always exists in MFA, import canceled", reg.UPN));
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(string.Format("Error importing from AD \r\r {0}",ex.Message));
            }
            finally
            {
                Trace.Unindent();
                FinalizeTrace(listen);
            }
        }

        /// <summary>
        /// InitializeTrace method implementation
        /// </summary>
        private static TraceListener InitializeTrace(string filename)
        {
            DefaultTraceListener listen = new DefaultTraceListener();
            listen.Name = "MFATrace";
            Trace.Listeners.Add(listen);
            listen.TraceOutputOptions = TraceOptions.DateTime;
            listen.LogFileName = filename;
            return listen;
        }

        /// <summary>
        /// FinalizeTrace method implmentation
        /// </summary>
        private static void FinalizeTrace(TraceListener listen)
        {
            Trace.Flush();
            Trace.Close();
            listen.Close();
            Trace.Listeners.Remove("MFATrace");
        }


        /// <summary>
        /// CheckRepositoryAttribute method implementation
        /// </summary>
        internal static bool CheckRepositoryAttribute(MFAConfig cfg, string attributename, int choice = 0)
        {
            DataRepositoryService client = null;
            switch (choice)
            {
                case 1:
                    client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                    return client.CheckRepositoryAttribute(attributename);
                case 2:
                    client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                    return client.CheckRepositoryAttribute(attributename);
                default:
                    if (cfg.UseActiveDirectory)
                    {
                        client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                        return client.CheckRepositoryAttribute(attributename);
                    }
                    else
                    {
                        client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                        return client.CheckRepositoryAttribute(attributename);
                    }
            }
        }
        #endregion

        #region Password Management
        /// <summary>
        /// ChangePassword method implmentation
        /// </summary>
        internal static void ChangePassword(string username, string oldpassword, string newpassword)
        {
            using (var ctx = new PrincipalContext(ContextType.Domain))
            {
                using (var user = UserPrincipal.FindByIdentity(ctx, IdentityType.UserPrincipalName, username))
                {
                    user.ChangePassword(oldpassword, newpassword);
                }
            }
        }
        #endregion

        #region Keys management
        /// <summary>
        /// GetUserKey method implementation
        /// </summary>
        internal static string GetUserKey(MFAConfig config, string upn)
        {
            return KeysManager.ReadKey(upn);
        }

        /// <summary>
        /// GetEncodedUserKey method implementation
        /// </summary>
        internal static string GetEncodedUserKey(MFAConfig config, string upn)
        {
            return KeysManager.EncodedKey(upn);
        }

        /// <summary>
        /// NewUserKey method implementation
        /// </summary>
        internal static string NewUserKey(MFAConfig config, string upn)
        {
            return KeysManager.NewKey(upn);
        }

        /// <summary>
        /// RemoveUserKey method implmentation
        /// </summary>
        internal static bool RemoveUserKey(MFAConfig config, string upn)
        {
            return KeysManager.RemoveKey(upn);
        }
        #endregion    
    }    

    /// <summary>
    /// OTPGenerator static class
    /// </summary>
    internal partial class OTPGenerator
    {
        private int _secondsToGo;
        private string _identity;
        private byte[] _secret;
        private Int64 _timestamp;
        private byte[] _hmac;
        private int _offset;
        private int _oneTimePassword;
        private DateTime _datetime;
        private HashMode _mode = HashMode.SHA1;

        public static int TOTPDuration = 30;

        /// <summary>
        /// Constructor
        /// </summary>
        public OTPGenerator(HashMode mode = HashMode.SHA1)
        {
            RequestedDatetime = DateTime.UtcNow;
            _mode = mode;
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += (s, e) => SecondsToGo = TOTPDuration - Convert.ToInt32(GetUnixTimestamp(RequestedDatetime) % TOTPDuration);
            timer.IsEnabled = true;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OTPGenerator(byte[] asecret, string aid, HashMode mode = HashMode.SHA1)
        {
            RequestedDatetime = DateTime.UtcNow;
            _mode = mode;
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += (s, e) => SecondsToGo = TOTPDuration - Convert.ToInt32(GetUnixTimestamp(RequestedDatetime) % TOTPDuration);
            timer.IsEnabled = true;

            Secret = asecret;
            Identity = aid;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OTPGenerator(string ssecret, string aid, HashMode mode = HashMode.SHA1)
        {
            RequestedDatetime = DateTime.UtcNow;
            byte[] asecret = Base32.GetBytesFromString(ssecret);
            _mode = mode;
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += (s, e) => SecondsToGo = TOTPDuration - Convert.ToInt32(GetUnixTimestamp(RequestedDatetime) % TOTPDuration);
            timer.IsEnabled = true;

            Secret = asecret;
            Identity = aid;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OTPGenerator(string ssecret, string aid, DateTime datetime, HashMode mode = HashMode.SHA1)
        {
            RequestedDatetime = datetime;
            byte[] asecret = Base32.GetBytesFromString(ssecret);
            _mode = mode;
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += (s, e) => SecondsToGo = TOTPDuration - Convert.ToInt32(GetUnixTimestamp(RequestedDatetime) % TOTPDuration);
            timer.IsEnabled = true;

            Secret = asecret;
            Identity = aid;
        }

        /// <summary>
        /// SecondsToGo property implmentation
        /// </summary>
        public int SecondsToGo
        {
            get { return _secondsToGo; }
            private set 
            { 
                _secondsToGo = value;
                if (SecondsToGo == TOTPDuration)
                    ComputeOTP(RequestedDatetime); 
            }
        }
        
        /// <summary>
        /// Identity property implementation
        /// </summary>
        public string Identity
        {
            get { return _identity; }
            set { _identity = value; }
        }
       
        /// <summary>
        /// Secret propertry implmentation
        /// </summary>
        private byte[] Secret
        {
            get { return _secret; }
            set { _secret = value; }
        }

        /// <summary>
        /// Timestamp property implmentation
        /// </summary>
        public Int64 Timestamp
        {
            get { return _timestamp; }
            private set { _timestamp = value; }
        }

        /// <summary>
        /// RequestedDatetime property implmentation
        /// </summary>
        public DateTime RequestedDatetime
        {
            get { return _datetime; }
            private set { _datetime = value; }
        }

        /// <summary>
        /// Hmac property implementation
        /// </summary>
        public byte[] Hmac
        {
            get { return _hmac; }
            private set { _hmac = value; }
        }

        /// <summary>
        /// HmacPart1 property implementation
        /// </summary>
        public byte[] HmacPart1
        {
            get { return _hmac.Take(Offset).ToArray(); }
        }

        /// <summary>
        /// HmacPart2 property implmentation 
        /// </summary>
        public byte[] HmacPart2
        {
            get { return _hmac.Skip(Offset).Take(4).ToArray(); }
        }

        /// <summary>
        /// HmacPart3 property implmentation
        /// </summary>
        public byte[] HmacPart3
        {
            get { return _hmac.Skip(Offset + 4).ToArray(); }
        }

        /// <summary>
        /// Offset property implmentation
        /// </summary>
        public int Offset
        {
            get { return _offset; }
            private set { _offset = value; }
        }

        /// <summary>
        /// OTP property implementation
        /// </summary>
        public int OTP
        {
            get { return _oneTimePassword; }
            set { _oneTimePassword = value; }
        }

        /// <summary>
        /// ComputeOneTimePassword method implmentation
        /// </summary>
        public void ComputeOTP(DateTime date)
        {
            // https://tools.ietf.org/html/rfc4226
            Timestamp = Convert.ToInt64(GetUnixTimestamp(date) / TOTPDuration);
            var data = BitConverter.GetBytes(Timestamp).Reverse().ToArray();
            switch (_mode)
            {
                case HashMode.SHA1:
                    Hmac = new HMACSHA1(Secret).ComputeHash(data);
                    break;
                case HashMode.SHA256:
                    Hmac = new HMACSHA256(Secret).ComputeHash(data);
                    break;
                case HashMode.SHA384:
                    Hmac = new HMACSHA384(Secret).ComputeHash(data);
                    break;
                case HashMode.SHA512:
                    Hmac = new HMACSHA512(Secret).ComputeHash(data);
                    break;
            }
            Offset = Hmac.Last() & 0x0F;
            OTP = (((Hmac[Offset + 0] & 0x7f) << 24) | ((Hmac[Offset + 1] & 0xff) << 16) | ((Hmac[Offset + 2] & 0xff) << 8) | (Hmac[Offset + 3] & 0xff)) % 1000000;
        }

        /// <summary>
        /// GetUnixTimestamp method implementation
        /// </summary>
        private static Int64 GetUnixTimestamp(DateTime date)
        {
            return Convert.ToInt64(Math.Round((date - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds));
        }
    }

    /// <summary>
    /// Base32 static class
    /// </summary>
    public static class Base32
    {
        private const int IN_BYTE_SIZE = 8;
        private const int OUT_BYTE_SIZE = 5;
        private static char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

        /// <summary>
        /// Encode method implmentation
        /// </summary>
        public static string Encode(byte[] data)
        {
            int i = 0, index = 0, digit = 0;
            int current_byte, next_byte;
            StringBuilder result = new StringBuilder((data.Length + 7) * IN_BYTE_SIZE / OUT_BYTE_SIZE);

            while (i < data.Length)
            {
                current_byte = (data[i] >= 0) ? data[i] : (data[i] + 256); // Unsign

                /* Is the current digit going to span a byte boundary? */
                if (index > (IN_BYTE_SIZE - OUT_BYTE_SIZE))
                {
                    if ((i + 1) < data.Length)
                        next_byte = (data[i + 1] >= 0) ? data[i + 1] : (data[i + 1] + 256);
                    else
                        next_byte = 0;

                    digit = current_byte & (0xFF >> index);
                    index = (index + OUT_BYTE_SIZE) % IN_BYTE_SIZE;
                    digit <<= index;
                    digit |= next_byte >> (IN_BYTE_SIZE - index);
                    i++;
                }
                else
                {
                    digit = (current_byte >> (IN_BYTE_SIZE - (index + OUT_BYTE_SIZE))) & 0x1F;
                    index = (index + OUT_BYTE_SIZE) % IN_BYTE_SIZE;
                    if (index == 0)
                        i++;
                }
                result.Append(alphabet[digit]);
            }
            return result.ToString();
        }

        /// <summary>
        /// Encode method overload
        /// </summary>
        public static string Encode(string data)
        {
            byte[] result = GetBytesFromString(data); 
            return Encode(result);
        }

        /// <summary>
        /// EncodeString method
        /// </summary>
        public static string EncodeString(string data)
        {
            byte[] bytes = new byte[data.Length * sizeof(char)];
            System.Buffer.BlockCopy(data.ToCharArray(), 0, bytes, 0, bytes.Length);
            return Encode(bytes);
        }

        /// <summary>
        /// GetBytesFromString method
        /// </summary>
        internal static byte[] GetBytesFromString(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>
        /// GetStringFromByteArray method
        /// </summary>
        internal static string GetStringFromByteArray(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            string str = new string(chars);
            return str; 
        }
    }

    /// <summary>
    /// KeysManager static class
    /// </summary>
    internal static class KeysManager
    {
        private static ISecretKeyManager _manager;
        private static bool _isloaded = true;

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        internal static void Initialize(MFAConfig cfg)
        {
            Trace.TraceInformation("KeysManager.Initialize()");   
            switch (cfg.KeysConfig.KeyFormat)
            {
                case SecretKeyFormat.RNG:
                    _manager = new RNGKeyManager();
                    _manager.Initialize(cfg);
                    _isloaded = true;
                    break;
                case SecretKeyFormat.RSA:
                    _manager = new RSAKeyManager();
                    _manager.Initialize(cfg);
                    _isloaded = true;
                    break;
                case SecretKeyFormat.CUSTOM:
                    _manager = Utilities.LoadExternalKeyManagerWrapper(cfg.KeysConfig.ExternalKeyManager.FullQualifiedImplementation);
                    if (_manager == null)
                        IsLoaded = false;
                    else
                    {
                        _manager.Initialize(cfg);
                        _isloaded = true;
                    }
                    break;
                default:
                    throw new NotImplementedException("CUSTOM SecretKeyManager not found !");
            }
        }

        /// <summary>
        /// IsLoaded property
        /// </summary>
        internal static bool IsLoaded
        {
            get { return _isloaded; }
            set { _isloaded = value; }
        }
        /// <summary>
        /// EnsureKey method iplementation
        /// </summary>
        internal static void EnsureKey(string upn) 
        {
            string key = ReadKey(upn);
            if (string.IsNullOrEmpty(key))
            {
                NewKey(upn);
            }
        }

        /// <summary>
        /// NewKey method implmentation
        /// </summary>
        internal static string NewKey(string upn)
        {
            return _manager.NewKey(upn);
        }

        /// <summary>
        /// ReadKey method implementation
        /// </summary>
        internal static string ReadKey(string upn)
        {
            return _manager.ReadKey(upn);
        }

        /// <summary>
        /// EncodedKey method implementation
        /// </summary>
        internal static string EncodedKey(string upn)
        {
            return _manager.EncodedKey(upn);
        }

        /// <summary>
        /// ProbeKey method implementation
        /// </summary>
        internal static string ProbeKey(string upn)
        {
            return _manager.ProbeKey(upn);
        }

        /// <summary>
        /// RemoveKey method implementation
        /// </summary>
        internal static bool RemoveKey(string upn)
        {
            return _manager.RemoveKey(upn);
        }

        /// <summary>
        /// CheckKey method implmentation
        /// </summary>
        internal static bool ValidateKey(string upn)
        {
            return _manager.ValidateKey(upn);
        }

        /// <summary>
        /// StripKeyPrefix method implementation
        /// </summary>
        internal static string StripKeyPrefix(string key)
        {
            return _manager.StripKeyPrefix(key);
        }

        /// <summary>
        /// AddKeyPrefix method implementation
        /// </summary>
        internal static string AddKeyPrefix(string key)
        {
            return _manager.AddKeyPrefix(key);
        }

        /// <summary>
        /// HasKeyPrefix method implementation
        /// </summary>
        internal static bool HasKeyPrefix(string key)
        {
            return _manager.HasKeyPrefix(key);
        }

        /// <summary>
        /// GetDataManager 
        /// </summary>
       /* internal static IKeysDataManager GetDataManager()
        {
            return _datamanager;
        } */
    }

    /// <summary>
    /// RNGKeyManager class
    /// </summary>
    public class RNGKeyManager : ISecretKeyManager
    {
        private KeyGeneratorMode _mode = KeyGeneratorMode.ClientSecret512;
        private KeySizeMode _ksize = KeySizeMode.KeySize1024;
        private MFAConfig _config = null;
        private KeysRepositoryService _repos = null;
        private int MAX_PROBE_LEN = 128;

        /// <summary>
        /// RNGKeyManager constructor
        /// limit creation by KeyManager
        /// </summary>
        internal RNGKeyManager()
        {
            Trace.TraceInformation("RNGKeyManager()");
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public void Initialize(MFAConfig config)
        {
            _config = config;
            _mode = config.KeysConfig.KeyGenerator;
            _ksize = config.KeysConfig.KeySize;
            if (_config.UseActiveDirectory)
                _repos = new ADDSKeysRepositoryService(_config);
            else
                _repos = new SQLKeysRepositoryService(_config);
        }

        /// <summary>
        /// KeysStorage method implementation
        /// </summary>
        public KeysRepositoryService KeysStorage
        {
            get { return _repos; }
        }

        /// <summary>
        /// Prefix property
        /// </summary>
        public string Prefix
        {
            get { return "rng://"; }
        }

        /// <summary>
        /// NewKey method implementation
        /// </summary>
        public string NewKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return null;
            string lupn = upn.ToLower();
            RandomNumberGenerator cryptoRandomDataGenerator = new RNGCryptoServiceProvider();
            byte[] buffer = null;
            string crypted = string.Empty;
            switch (_mode)
            {
                case KeyGeneratorMode.ClientSecret128:
                    buffer = new byte[16];
                    cryptoRandomDataGenerator.GetBytes(buffer);
                    crypted = AddKeyPrefix(Convert.ToBase64String(buffer));
                    break;
                case KeyGeneratorMode.ClientSecret256:
                    buffer = new byte[32];
                    cryptoRandomDataGenerator.GetBytes(buffer);
                    crypted = AddKeyPrefix(Convert.ToBase64String(buffer));
                    break;
                case KeyGeneratorMode.ClientSecret384:
                    buffer = new byte[48];
                    cryptoRandomDataGenerator.GetBytes(buffer);
                    crypted = AddKeyPrefix(Convert.ToBase64String(buffer));
                    break;
                case KeyGeneratorMode.ClientSecret512:
                    buffer = new byte[64];
                    cryptoRandomDataGenerator.GetBytes(buffer);
                    crypted = AddKeyPrefix(Convert.ToBase64String(buffer));
                    break;
                default:
                    buffer = Guid.NewGuid().ToByteArray();
                    cryptoRandomDataGenerator.GetBytes(buffer);
                    crypted = AddKeyPrefix(Convert.ToBase64String(buffer));
                    break;
            }
            KeysStorage.NewUserKey(lupn, crypted);
            return crypted;
        }

        /// <summary>
        /// ValidateKey method implementation
        /// </summary>
        public bool ValidateKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return false;
            string lupn = upn.ToLower();
            string key = ReadKey(lupn);
            if (HasKeyPrefix(key))
                return true;
            else if (key.Equals(StripKeyPrefix(key)))
                return true; // Old RNG without prefix for compatibility
            else
                return false;
        }

        /// <summary>
        /// ReadKey method implementation
        /// </summary>
        public string ReadKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return null;
            string lupn = upn.ToLower();
            return KeysStorage.GetUserKey(lupn);
        }

        /// <summary>
        /// EncodedKey method implementation
        /// </summary>
        public string EncodedKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return null;
            string lupn = upn.ToLower();
            string full = StripKeyPrefix(ReadKey(lupn));
            if (string.IsNullOrEmpty(full))
                return null;
            if (full.Length > MAX_PROBE_LEN)
                return Base32.Encode(full.Substring(0, MAX_PROBE_LEN));
            else
                return Base32.Encode(full);
        }

        /// <summary>
        /// ProbeKey method implmentation
        /// </summary>
        public string ProbeKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return null;
            string lupn = upn.ToLower();
            string full = StripKeyPrefix(ReadKey(lupn));
            if (string.IsNullOrEmpty(full))
                return null;
            if (full.Length > MAX_PROBE_LEN)
                return full.Substring(0, MAX_PROBE_LEN);
            else
                return full;
        }

        /// <summary>
        /// RemoveKey method implementation
        /// </summary>
        public bool RemoveKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return false;
            string lupn = upn.ToLower();
            return KeysStorage.RemoveUserKey(lupn);
        }
        
        /// <summary>
        /// StripKeyPrefix method implementation
        /// </summary>
        public virtual string StripKeyPrefix(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            if (key.StartsWith(this.Prefix))
                key = key.Replace(this.Prefix, "");
            return key;
        }

        /// <summary>
        /// AddKeyPrefix method implementation
        /// </summary>
        public virtual string AddKeyPrefix(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            if (!key.StartsWith(this.Prefix))
                key = this.Prefix + key;
            return key;
        }

        /// <summary>
        /// HasKeyPrefix method implementation
        /// </summary>
        public virtual bool HasKeyPrefix(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
            return key.StartsWith(this.Prefix);
        }
    }

    /// <summary>
    /// RSAKeyManager class
    /// </summary>
    public class RSAKeyManager : ISecretKeyManager
    {
        private string _certificatethumbprint;
        private MFAConfig _config = null;
        private static Encryption _cryptoRSADataProvider = null;
        private KeySizeMode _ksize = KeySizeMode.KeySize1024;
        private KeysRepositoryService _repos = null;
        private int MAX_PROBE_LEN = 0;

        /// <summary>
        /// RSAKeyManager constructor
        /// limit creation by KeyManager
        /// </summary>
        internal RSAKeyManager()
        {
            Trace.TraceInformation("RSAKeyManager()");   
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public void Initialize(MFAConfig config)
        {
            _config = config;
            _certificatethumbprint = config.KeysConfig.CertificateThumbprint;
            _ksize = config.KeysConfig.KeySize;
            if (_config.UseActiveDirectory)
                _repos = new ADDSKeysRepositoryService(_config);
            else
                _repos = new SQLKeysRepositoryService(_config);
            switch (_ksize)
            {
                case KeySizeMode.KeySize512:
                    MAX_PROBE_LEN = 64;
                    break;
                case KeySizeMode.KeySize1024:
                    MAX_PROBE_LEN = 128;
                    break;
                case KeySizeMode.KeySize2048:
                    MAX_PROBE_LEN = 256;
                    break;
                default:
                    MAX_PROBE_LEN = 128;
                    break;
            }
        }

        /// <summary>
        /// KeysStorage method implementation
        /// </summary>
        public KeysRepositoryService KeysStorage
        {
            get { return _repos; }
        }

        /// <summary>
        /// Prefix property
        /// </summary>
        public string Prefix
        {
            get { return "rsa://";  }
        }

        /// <summary>
        /// NewKey method implementation
        /// </summary>
        public string NewKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return null;
            string lupn = upn.ToLower();
            string strcert = string.Empty;
            if (_cryptoRSADataProvider == null)
                _cryptoRSADataProvider = new Encryption(_certificatethumbprint);
            string crypted = AddKeyPrefix(_cryptoRSADataProvider.Encrypt(lupn));
            KeysStorage.NewUserKey(lupn, crypted);
            return crypted;
        }

        /// <summary>
        /// ValidateKey method implmentation
        /// </summary>
        public bool ValidateKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return false;
            string lupn = upn.ToLower();
            string key = ReadKey(lupn);
            if (HasKeyPrefix(key))
            {
                if (_cryptoRSADataProvider == null)
                    _cryptoRSADataProvider = new Encryption(_certificatethumbprint);
                key = StripKeyPrefix(key);
                string user = _cryptoRSADataProvider.Decrypt(key);
                if (string.IsNullOrEmpty(user))
                    return false; // Key corrupted
                if (user.ToLower().Equals(lupn))
                    return true;  // OK RSA
                else
                    return false; // Key corrupted
            }
            else
                return false; 
        }

        /// <summary>
        /// ReadKey method implementation
        /// </summary>
        public string ReadKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return null;
            string lupn = upn.ToLower();
            return KeysStorage.GetUserKey(lupn);
        }

        /// <summary>
        /// EncodedKey method implementation
        /// </summary>
        public string EncodedKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return null;
            string lupn = upn.ToLower();
            string full = StripKeyPrefix(ReadKey(lupn));
            if (full.Length > MAX_PROBE_LEN)
                return Base32.Encode(full.Substring(0, MAX_PROBE_LEN));
            else
                return Base32.Encode(full);
        }

        /// <summary>
        /// ProbeKey method implmentation
        /// </summary>
        public string ProbeKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return null;
            string lupn = upn.ToLower();
            string full = StripKeyPrefix(ReadKey(lupn));
            if (full.Length > MAX_PROBE_LEN)
                return full.Substring(0, MAX_PROBE_LEN);
            else
                return full;
        }

        /// <summary>
        /// RemoveKey method implementation
        /// </summary>
        public bool RemoveKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return false;
            string lupn = upn.ToLower();
            return KeysStorage.RemoveUserKey(lupn);
        }
        
        /// <summary>
        /// StripKeyPrefix method implementation
        /// </summary>
        public virtual string StripKeyPrefix(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            if (key.StartsWith(this.Prefix))
                key = key.Replace(this.Prefix, "");
            return key;
        }

        /// <summary>
        /// AddKeyPrefix method implementation
        /// </summary>
        public virtual string AddKeyPrefix(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            if (!key.StartsWith(this.Prefix))
                key = this.Prefix + key;
            return key;
        }

        /// <summary>
        /// HasKeyPrefix method implementation
        /// </summary>
        public virtual bool HasKeyPrefix(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
            return key.StartsWith(this.Prefix);
        }
    }

    /// <summary>
    /// MailUtilities class
    /// </summary>
    public static class MailUtilities
    {
        private static object lck = 0;
        /// <summary>
        /// SetCultureInfo method implementation
        /// </summary>
        internal static void SetCultureInfo(int lcid)
        {
            System.Globalization.CultureInfo inf = new System.Globalization.CultureInfo(lcid);
            cmail_strings.Culture = inf;
        }

        /// <summary>
        /// SendMail method implementation
        /// </summary>
        private static void SendMail(MailMessage Message, MailProvider mail)
        {
            SmtpClient client = new SmtpClient();
            client.Host = mail.Host;
            client.Port = mail.Port;
            client.UseDefaultCredentials = false;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = mail.UseSSL;
            client.Credentials = new NetworkCredential(mail.UserName, mail.Password);
            client.Send(Message);
        }

        /// <summary>
        /// SendOTPByEmail method implementation
        /// </summary>
        public static void SendOTPByEmail(string to, string upn, string code, MailProvider mail, CultureInfo culture)
        {
            string htmlres = string.Empty;
            if (mail.MailOTPContent != null)
            {
                int ctry = culture.LCID;
                string tmp = mail.MailOTPContent.Where(c => c.LCID.Equals(ctry) && c.Enabled).Select(s => s.FileName).FirstOrDefault();
                if (!string.IsNullOrEmpty(tmp))
                {
                    if (File.Exists(tmp))
                    {
                        FileStream fileStream = new FileStream(tmp, FileMode.Open, FileAccess.Read);

                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            htmlres = reader.ReadToEnd();
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(htmlres))
            {
                lock(lck)
                {
                    cmail_strings.Culture = culture;
                    htmlres = cmail_strings.MailOTPContent;
                }
            }
            string html = StripEmailContent(htmlres);
            string name = upn.Remove(2, upn.IndexOf('@') - 2).Insert(2, "*********");
            MailMessage Message = new MailMessage(mail.From, to);
            Message.BodyEncoding = UTF8Encoding.UTF8;
            Message.IsBodyHtml = true;
            Message.Body = string.Format(html, mail.Company, name, code);
            Message.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;
            lock(lck)
            {
                cmail_strings.Culture = culture;
                Message.Subject = cmail_strings.MailOTPTitle;
            }
            SendMail(Message, mail);
        }

        /// <summary>
        /// SendInscriptionMail method implementation
        /// </summary>
        public static void SendInscriptionMail(string to, Registration user, MailProvider mail, CultureInfo culture)
        {
            string htmlres = string.Empty;
            if (mail.MailAdminContent != null)
            {
                int ctry = culture.LCID;
                string tmp = mail.MailAdminContent.Where(c => c.LCID.Equals(ctry) && c.Enabled).Select(s => s.FileName).FirstOrDefault();
                if (!string.IsNullOrEmpty(tmp))
                {
                    if (File.Exists(tmp))
                    {
                        FileStream fileStream = new FileStream(tmp, FileMode.Open, FileAccess.Read);

                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            htmlres = reader.ReadToEnd();
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(htmlres))
            {
                lock (lck)
                {
                    cmail_strings.Culture = culture;
                    htmlres = cmail_strings.MailAdminContent;
                }
            }
            string sendermail = GetUserBusinessEmail(user.UPN);
            string html = StripEmailContent(htmlres);
            MailMessage Message = new MailMessage(mail.From, to);
            if (!string.IsNullOrEmpty(sendermail))
                Message.CC.Add(sendermail);
            Message.BodyEncoding = UTF8Encoding.UTF8;
            Message.IsBodyHtml = true;
            Message.Body = string.Format(htmlres, mail.Company, user.UPN, user.MailAddress, user.PhoneNumber, user.PreferredMethod);
            Message.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;
            lock (lck)
            {
                cmail_strings.Culture = culture;
                Message.Subject = string.Format(cmail_strings.MailAdminTitle, user.UPN);
            }
            SendMail(Message, mail);
        }

        /// <summary>
        /// SendKeyByEmail method implementation
        /// </summary>
        public static void SendKeyByEmail(string email, string upn, string key, MailProvider mail, MFAConfig config, CultureInfo culture)
        {
            string htmlres = string.Empty;
            if (mail.MailKeyContent != null)
            {
                int ctry = culture.LCID;
                string tmp = mail.MailKeyContent.Where(c => c.LCID.Equals(ctry) && c.Enabled).Select(s => s.FileName).FirstOrDefault();
                if (!string.IsNullOrEmpty(tmp))
                {
                    if (File.Exists(tmp))
                    {
                        FileStream fileStream = new FileStream(tmp, FileMode.Open, FileAccess.Read);

                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            htmlres = reader.ReadToEnd();
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(htmlres))
            {
                lock(lck)
                {
                    cmail_strings.Culture = culture;
                    htmlres = cmail_strings.MailKeyContent;
                }
            }

            string sendermail = GetUserBusinessEmail(upn);
            string html = StripEmailContent(htmlres);
            using (Stream qrcode = QRUtilities.GetQRCodeStream(upn, key, config))
            {
                qrcode.Position = 0;
                var inlineLogo = new LinkedResource(qrcode, "image/png");
                inlineLogo.ContentId = Guid.NewGuid().ToString();

                MailMessage Message = new MailMessage(mail.From, email);
                if (!string.IsNullOrEmpty(sendermail))
                    Message.CC.Add(sendermail);
                Message.BodyEncoding = UTF8Encoding.UTF8;
                Message.IsBodyHtml = true;

                Message.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;
                lock (lck)
                {
                    cmail_strings.Culture = culture;
                    Message.Subject = cmail_strings.MailKeyTitle;
                }
                Message.Priority = MailPriority.High;

                string body = string.Format(html, mail.Company, upn, key, inlineLogo.ContentId);
                var view = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                view.LinkedResources.Add(inlineLogo);
                Message.AlternateViews.Add(view);
                SendMail(Message, mail);
            }
        }

        /// <summary>
        /// GetUserBusinessEmail method implmentation
        /// </summary>
        internal static string GetUserBusinessEmail(string username)
        {
            using (var ctx = new PrincipalContext(ContextType.Domain))
            {
                using (var user = UserPrincipal.FindByIdentity(ctx, IdentityType.UserPrincipalName, username))
                {
                    return user.EmailAddress;
                }
            }
        }

        #region private methods
        /// <summary>
        /// StripEmailDomain method
        /// </summary>
        public static string StripEmailDomain(string email)
        {
            try
            {

                if (string.IsNullOrEmpty(email))
                    return string.Empty;
                else
                    return email.Substring(email.IndexOf("@", 0));
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// StripEmailContent method 
        /// </summary>
        internal static string StripEmailContent(string content)
        {
            return content.Replace("<![CDATA[", "").Replace("]]>", "");
        }
        #endregion
    }

    /// <summary>
    /// QRUtilities static class
    /// </summary>
    public static class QRUtilities
    {
        #region QRUtilities
        /// <summary>
        /// ConvertToBase64 method implmentation
        /// </summary>
        private static string ConvertToBase64(Stream stream)
        {
            Byte[] inArray = new Byte[(int)stream.Length];
            stream.Read(inArray, 0, (int)stream.Length);
            return Convert.ToBase64String(inArray);
        }

        /// <summary>
        /// ConvertToBase64Stream 
        /// </summary>
        private static Stream ConvertToBase64Stream(Stream stream)
        {
            byte[] inArray = new Byte[(int)stream.Length];
            stream.Read(inArray, 0, (int)stream.Length);
            return new MemoryStream(inArray);
        }

        /// <summary>
        /// GetQRCodeString method implmentation
        /// </summary>
        public static string GetQRCodeString(string UPN, string QRString, MFAConfig config)
        {
            string result = string.Empty;
            string Content = string.Format("otpauth://totp/{0}:{1}?secret={2}&issuer={3}&algorithm={4}", config.Issuer, UPN, QRString, config.QRIssuer, config.OTPProvider.Algorithm);

            var encoder = new QrEncoding.QrEncoder(ErrorCorrectionLevel.L);
            QrCode qr;
            if (!encoder.TryEncode(Content, out qr))
                return string.Empty;
            BitMatrix matrix = qr.Matrix;
            using (MemoryStream ms = new MemoryStream())
            {
                var render = new GraphicsRenderer(new FixedModuleSize(3, QuietZoneModules.Zero));
                render.WriteToStream(matrix, ImageFormat.Png, ms);
                ms.Position = 0;
                result = ConvertToBase64(ms);
            }
            return result;
        }

        /// <summary>
        /// GetQRCodeValue method implmentation
        /// </summary>
        public static string GetQRCodeValue(string UPN, string QRString, MFAConfig config)
        {
            string result = string.Empty;
            string Content = string.Format("otpauth://totp/{0}:{1}?secret={2}&issuer={3}&algorithm={4}", config.Issuer, UPN, QRString, config.QRIssuer, config.OTPProvider.Algorithm);
            return Content;
        }

        /// <summary>
        /// GetQRCodeStream method implmentation
        /// </summary>
        public static Stream GetQRCodeStream(string UPN, string QRString, MFAConfig config)
        {
            string result = string.Empty;
            string Content = string.Format("otpauth://totp/{0}:{1}?secret={2}&issuer={3}&algorithm={4}", config.Issuer, UPN, QRString, config.QRIssuer, config.OTPProvider.Algorithm);

            var encoder = new QrEncoding.QrEncoder(ErrorCorrectionLevel.L);
            QrCode qr;
            if (!encoder.TryEncode(Content, out qr))
                return null;
            BitMatrix matrix = qr.Matrix;
            var render = new GraphicsRenderer(new FixedModuleSize(3, QuietZoneModules.Zero));
            using (MemoryStream ms = new MemoryStream())
            {
                render.WriteToStream(matrix, ImageFormat.Png, ms);
                ms.Position = 0;
                return ConvertToBase64Stream(ms);
            }
        }
        #endregion
    }

    /// <summary>
    /// XmlConfigSerializer class
    /// </summary>
    internal class XmlConfigSerializer : XmlSerializer
    {
        public XmlConfigSerializer(Type type): base(type)
        {
            this.UnknownAttribute += OnUnknownAttribute;
            this.UnknownElement += OnUnknownElement;
            this.UnknownNode += OnUnknownNode;
            this.UnreferencedObject += OnUnreferencedObject;
        }

        /// <summary>
        /// OnUnknownNode method implementation
        /// </summary>
        private void OnUnknownNode(object sender, XmlNodeEventArgs e)
        {
            Log.WriteEntry("Xml Serialization error : Unknow Node : "+e.Name+ " Position ("+ e.LineNumber.ToString()+", "+e.LinePosition.ToString()+")", EventLogEntryType.Error, 700);
        }

        /// <summary>
        /// OnUnknownElement method implementation
        /// </summary>
        private void OnUnknownElement(object sender, XmlElementEventArgs e)
        {
            Log.WriteEntry("Xml Serialization error : Unknow Element : "+e.Element.Name+" at Position (" + e.LineNumber.ToString() + ", " + e.LinePosition.ToString() + ")", EventLogEntryType.Error, 701);
        }

        /// <summary>
        /// OnUnknownAttribute method implementation
        /// </summary>
        private void OnUnknownAttribute(object sender, XmlAttributeEventArgs e)
        { 
            Log.WriteEntry("Xml Serialization error : Unknow Attibute : "+e.Attr.Name+" at Position (" + e.LineNumber.ToString() + ", " + e.LinePosition.ToString() + ")", EventLogEntryType.Error, 702);
        }

        /// <summary>
        /// OnUnreferencedObject method implementation
        /// </summary>
        private void OnUnreferencedObject(object sender, UnreferencedObjectEventArgs e)
        {
            Log.WriteEntry("Xml Serialization error : Unknow Object : " + e.UnreferencedId + " of Type (" + e.UnreferencedObject.GetType().ToString() + ")", EventLogEntryType.Error, 703);
        }
    }

    /// <summary>
    /// CFGUtilities class
    /// </summary>
    internal static class CFGUtilities
    {
        /// <summary>
        /// ReadConfiguration method implementation
        /// </summary>
        internal static MFAConfig ReadConfiguration(PSHost Host = null)
        {
            MFAConfig config = null;
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            string pth = Path.GetTempPath() + Path.GetRandomFileName();
            try
            {
                try
                {
                    RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                    SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                    SPPowerShell = PowerShell.Create();
                    SPPowerShell.Runspace = SPRunSpace;
                    SPRunSpace.Open();

                    Pipeline pipeline = SPRunSpace.CreatePipeline();
                    Command exportcmd = new Command("Export-AdfsAuthenticationProviderConfigurationData", false);
                    CommandParameter NParam = new CommandParameter("Name", "MultifactorAuthenticationProvider");
                    exportcmd.Parameters.Add(NParam);
                    CommandParameter PParam = new CommandParameter("FilePath", pth);
                    exportcmd.Parameters.Add(PParam);
                    pipeline.Commands.Add(exportcmd);
                    Collection<PSObject> PSOutput = pipeline.Invoke();
                }
                finally
                {
                    if (SPRunSpace != null)
                        SPRunSpace.Close();
                }

                FileStream stm = new FileStream(pth, FileMode.Open, FileAccess.Read);
                XmlConfigSerializer xmlserializer = new XmlConfigSerializer(typeof(MFAConfig));
                using (StreamReader reader = new StreamReader(stm))
                {
                    config = (MFAConfig)xmlserializer.Deserialize(stm);
                    if ((!config.OTPProvider.Enabled) && (!config.MailProvider.Enabled) && (!config.ExternalProvider.Enabled) && (!config.AzureProvider.Enabled))
                        config.OTPProvider.Enabled = true;   // always let an active option eg : aplication in this case
                    KeysManager.Initialize(config);  // Important
                    RuntimeAuthProvider.LoadProviders(config);
                }
            }
            finally
            {
                if (File.Exists(pth))
                    File.Delete(pth);
            }
            return config;
        }

        /// <summary>
        /// WriteConfiguration method implementation
        /// </summary>
        internal static MFAConfig WriteConfiguration(PSHost Host, MFAConfig config)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            string pth = Path.GetTempPath() + Path.GetRandomFileName();
            try
            {
                FileStream stm = new FileStream(pth, FileMode.CreateNew, FileAccess.ReadWrite);
                XmlConfigSerializer xmlserializer = new XmlConfigSerializer(typeof(MFAConfig));
                stm.Position = 0;
                using (StreamReader reader = new StreamReader(stm))
                {
                    xmlserializer.Serialize(stm, config);
                }
                try
                {
                    RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                    SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                    SPPowerShell = PowerShell.Create();
                    SPPowerShell.Runspace = SPRunSpace;
                    SPRunSpace.Open();

                    Pipeline pipeline = SPRunSpace.CreatePipeline();
                    Command exportcmd = new Command("Import-AdfsAuthenticationProviderConfigurationData", false);
                    CommandParameter NParam = new CommandParameter("Name", "MultifactorAuthenticationProvider");
                    exportcmd.Parameters.Add(NParam);
                    CommandParameter PParam = new CommandParameter("FilePath", pth);
                    exportcmd.Parameters.Add(PParam);
                    pipeline.Commands.Add(exportcmd);
                    Collection<PSObject> PSOutput = pipeline.Invoke();
                }
                finally
                {
                    if (SPRunSpace != null)
                        SPRunSpace.Close();
                }
            }
            finally
            {
                if (File.Exists(pth))
                    File.Delete(pth);
            }
            return config;
        }
    }

    /// <summary>
    /// Utilities class
    /// </summary>
    internal static class Utilities
    {
        /// <summary>
        /// GetRandomOTP  method implementation
        /// </summary>
        internal static int GetRandomOTP()
        {
            RandomNumberGenerator rnd = new RNGCryptoServiceProvider();
            byte[] buffer = new byte[4];
            rnd.GetBytes(buffer);
            uint val = BitConverter.ToUInt32(buffer, 0) % 1000000;
            return Convert.ToInt32(val);
           // Random random = new Random();
           // return random.Next(1, 999999);
        }

        /// <summary>
        /// GetEmailOTP method implmentation
        /// </summary>
        internal static int GetEmailOTP(AuthenticationContext ctx, MailProvider mail, CultureInfo culture)
        {
            Registration reg = (Registration)ctx;
            int otpres = GetRandomOTP();
            MailUtilities.SendOTPByEmail(reg.MailAddress, reg.UPN, otpres.ToString("D"), mail, culture);
            ctx.Notification = (int)AuthenticationResponseKind.EmailOTP;
            return otpres;
        }

        /// <summary>
        /// LoadExternalKeyManagerWrapper method implmentation
        /// </summary>
        internal static ISecretKeyManager LoadExternalKeyManagerWrapper(string AssemblyFulldescription)
        {
            Assembly assembly = Assembly.Load(Utilities.ParseAssembly(AssemblyFulldescription));
            Type _typetoload = assembly.GetType(Utilities.ParseType(AssemblyFulldescription));
            ISecretKeyManager wrapper = null;
            if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("ISecretKeyManager") != null)
            {
                object o = Activator.CreateInstance(_typetoload, true); // Allow Calling internal Constructors
                if (o is ISecretKeyManager)
                    wrapper = o as ISecretKeyManager;
            }
            return wrapper;
        }

        /// <summary>
        /// ParseType method implmentation
        /// </summary>
        internal static string ParseAssembly(string AssemblyFulldescription)
        {
            int cnt = AssemblyFulldescription.IndexOf(',');
            return AssemblyFulldescription.Remove(0, cnt).TrimStart(new char[] { ',', ' ' });
        }

        /// <summary>
        /// ParseType method implmentation
        /// </summary>
        internal static string ParseType(string AssemblyFulldescription)
        {
            string[] type = AssemblyFulldescription.Split(new char[] { ',' });
            return type[0];
        }

        /// <summary>
        /// ValidateEmail method implementation
        /// </summary>
        internal static bool ValidateEmail(string email, bool checkempty = false)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                    if (checkempty)
                        return false;
                    else
                        return true;
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email)
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(ex.Message, EventLogEntryType.Error, 800);
                throw ex;
            }
        }

        /// <summary>
        /// ValidateEmail method implementation
        /// </summary>
        internal static bool ValidatePhoneNumber(string phone, bool checkempty = false)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                {
                    if (checkempty)
                        return false;
                    else
                        return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(ex.Message, EventLogEntryType.Error, 800);
                throw ex;
            }
        }

        /// <summary>
        /// StripEmailAddress method
        /// </summary>
        internal static string StripEmailAddress(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                    return string.Empty;
                else
                    return email.Remove(2, email.IndexOf('@') - 2).Insert(2, "*********");
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// StripPhoneNumer method
        /// </summary>
        internal static string StripPhoneNumber(string phone)
        {
            try
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[]
		        {
			        Regex.Replace(phone.Substring(0, phone.Length - 4), "[0-9]", "x"),	phone.Substring(phone.Length - 4)
		        });

               /* if (string.IsNullOrEmpty(phone))
                    return "* ** ** ** **";
                else
                    return "* ** ** ** " + phone.Substring(phone.Length - 2, 2); */
            }
            catch
            {
                return "* ** ** ** **";
            }
        }
    }

    /// <summary>
    /// Log class
    /// </summary>
    public static class Log
    {
        private const string EventLogSource = "ADFS MFA Service";
        private const string EventLogGroup = "Application";

        /// <summary>
        /// Log constructor
        /// </summary>
        static Log()
        {
            if (!EventLog.SourceExists(Log.EventLogSource))
                EventLog.CreateEventSource(Log.EventLogSource, Log.EventLogGroup);
        }

        /// <summary>
        /// WriteEntry method implementation
        /// </summary>
        public static void WriteEntry(string message, EventLogEntryType type, int eventID)
        {
            EventLog.WriteEntry(EventLogSource, message, type, eventID);
        }
    }

    /// <summary>
    /// ImageExtensions class
    /// </summary>
    public static class ImageExtensions
    {
        public static byte[] ToByteArray(this Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }
    }
    #region ADFS Version
    internal class RegistryVersion
    {
        private string _currentVersion;
        private string _productName;
        private string _installationType;
        private int _currentBuild;
        private int _currentMajorVersionNumber;
        private int _currentMinorVersionNumber;

        internal RegistryVersion()
        {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion");

            _currentVersion = Convert.ToString(rk.GetValue("CurrentVersion"));
            _productName = Convert.ToString(rk.GetValue("ProductName"));
            _installationType = Convert.ToString(rk.GetValue("InstallationType"));
            _currentBuild = Convert.ToInt32(rk.GetValue("CurrentBuild"));
            _currentMajorVersionNumber = Convert.ToInt32(rk.GetValue("CurrentMajorVersionNumber"));
            _currentMinorVersionNumber = Convert.ToInt32(rk.GetValue("CurrentMinorVersionNumber"));
        }

        public string CurrentVersion
        {
            get { return _currentVersion; }
            set { _currentVersion = value; }
        }

        public string ProductName
        {
            get { return _productName; }
            set { _productName = value; }
        }

        public string InstallationType
        {
            get { return _installationType; }
            set { _installationType = value; }
        }

        public int CurrentBuild
        {
            get { return _currentBuild; }
            set { _currentBuild = value; }
        }

        public int CurrentMajorVersionNumber
        {
            get { return _currentMajorVersionNumber; }
            set { _currentMajorVersionNumber = value; }
        }

        public int CurrentMinorVersionNumber
        {
            get { return _currentMinorVersionNumber; }
            set { _currentMinorVersionNumber = value; }
        }

        public bool IsWindows2016
        {
            get { return (this.CurrentMajorVersionNumber == 10); }
        }

        public bool IsWindows2012R2
        {
            get { return (this.CurrentMajorVersionNumber != 10); }
        }

    }
    #endregion
}