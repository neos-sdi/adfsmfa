//******************************************************************************************************************************************************************************************//
// Copyright (c) 2020 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
using Microsoft.IdentityServer.Web.Authentication.External;
using Microsoft.Win32;
using Neos.IdentityServer.MultiFactor.Common;
using Neos.IdentityServer.MultiFactor.Data;
using Neos.IdentityServer.MultiFactor.QrEncoding;
using Neos.IdentityServer.MultiFactor.QrEncoding.Windows.Render;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Threading;
using System.Xml.Serialization;


namespace Neos.IdentityServer.MultiFactor
{
    #region RuntimeAuthProvider
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
                        try
                        {

                            if (string.IsNullOrEmpty(cfg.OTPProvider.FullQualifiedImplementation))
                                provider = new NeosOTPProvider();
                            else
                                provider = LoadExternalProvider(cfg.OTPProvider.FullQualifiedImplementation);
                            if (provider != null)
                            {
                                if (provider.Kind == PreferredMethod.Code)
                                {
                                    AddOrUpdateProvider(ctx.PreferredMethod, provider);
                                    provider.Initialize(new OTPProviderParams(cfg.OTPProvider));
                                }
                                else
                                    throw new Exception("Invalid Provider Kind !");
                            }
                            else
                                throw new Exception("Invalid Provider Kind !");
                        }
                        catch (Exception)
                        {
                            provider = null;
                        }
                    }
                    break;
                case PreferredMethod.Email:
                    provider = GetProviderInstance(ctx.PreferredMethod);
                    if (provider == null)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(cfg.MailProvider.FullQualifiedImplementation))
                                provider = new NeosMailProvider();
                            else
                                provider = LoadExternalProvider(cfg.MailProvider.FullQualifiedImplementation);
                            if (provider != null)
                            {
                                if (provider.Kind == PreferredMethod.Email)
                                {
                                    AddOrUpdateProvider(ctx.PreferredMethod, provider);
                                    provider.Initialize(new MailProviderParams(cfg.MailProvider));
                                }
                                else
                                    throw new Exception("Invalid Provider Kind !");
                            }
                            else
                                throw new Exception("Invalid Provider Kind !");
                        }
                        catch (Exception)
                        {
                            provider = null;
                        }
                    }
                    break;
                case PreferredMethod.External:
                    provider = GetProviderInstance(ctx.PreferredMethod);
                    if (provider == null)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(cfg.ExternalProvider.FullQualifiedImplementation))
                                provider = new NeosPlugExternalProvider();
                            else if (IsLegacyExternalWrapper(cfg.ExternalProvider.FullQualifiedImplementation))
                                provider = new NeosLegacySMSProvider();
                            else
                                provider = LoadExternalProvider(cfg.ExternalProvider.FullQualifiedImplementation);
                            if (provider != null)
                            {
                                if (provider.Kind == PreferredMethod.External)
                                {
                                    AddOrUpdateProvider(ctx.PreferredMethod, provider);
                                    provider.Initialize(new ExternalProviderParams(cfg.ExternalProvider));
                                }
                                else
                                    throw new Exception("Invalid Provider Kind !");
                            }
                            else
                                throw new Exception("Invalid Provider Kind !");
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
                            if (string.IsNullOrEmpty(cfg.AzureProvider.FullQualifiedImplementation))
                                provider = LoadAzureProvider();
                            else
                                provider = LoadExternalProvider(cfg.AzureProvider.FullQualifiedImplementation);
                            if (provider != null)
                            {
                                if (provider.Kind == PreferredMethod.Azure)
                                {
                                    AddOrUpdateProvider(ctx.PreferredMethod, provider);
                                    provider.Initialize(new AzureProviderParams(cfg.AzureProvider, cfg.Hosts.ADFSFarm.FarmIdentifier, cfg.Issuer));
                                }
                                else
                                    throw new Exception("Invalid Provider Kind !");
                            }
                            else
                                throw new Exception("Invalid Provider Kind !");
                        }
                        catch (Exception)
                        {
                            provider = null;
                        }
                    }
                    break;
                case PreferredMethod.Biometrics:
                    provider = GetProviderInstance(ctx.PreferredMethod);
                    if (provider == null)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(cfg.WebAuthNProvider.FullQualifiedImplementation))
                                provider = LoadWebAuthNProvider();
                            else
                                provider = LoadExternalProvider(cfg.WebAuthNProvider.FullQualifiedImplementation);
                            if (provider != null)
                            {
                                if (provider.Kind == PreferredMethod.Biometrics)
                                {
                                    AddOrUpdateProvider(ctx.PreferredMethod, provider);
                                    provider.Initialize(new WebAuthNProviderParams(cfg, cfg.WebAuthNProvider));
                                }
                                else
                                    throw new Exception("Invalid Provider Kind !");
                            }
                            else
                                throw new Exception("Invalid Provider Kind !");
                        }
                        catch (Exception)
                        {
                            provider = null;
                        }
                    }
                    break;
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
            Assembly assembly = Assembly.Load(@"Neos.IdentityServer.MultiFactor.SAS.Azure, Version=3.0.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2");
            Type _typetoload = assembly.GetType(@"Neos.IdentityServer.MultiFactor.SAS.NeosAzureProvider");
            if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IExternalProvider") != null)
                return (IExternalProvider)Activator.CreateInstance(_typetoload, true); // Allow Calling internal Constructors
            else
                return null;
        }

        /// <summary>
        /// LoadWebAuthNProvider method implmentation
        /// </summary>
        private static IExternalProvider LoadWebAuthNProvider()
        {
            Assembly assembly = Assembly.Load(@"Neos.IdentityServer.MultiFactor.WebAuthN.Provider, Version=3.0.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2");
            Type _typetoload = assembly.GetType(@"Neos.IdentityServer.MultiFactor.WebAuthN.NeosWebAuthNProvider");
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
            if (string.IsNullOrEmpty(AssemblyFulldescription))
                return null;
            Assembly assembly = Assembly.Load(Utilities.ParseAssembly(AssemblyFulldescription));
            if (assembly == null)
                return null;

            Type _typetoload = assembly.GetType(Utilities.ParseType(AssemblyFulldescription));
            if (_typetoload == null)
                return null;

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
            if (string.IsNullOrEmpty(AssemblyFulldescription))
                return false;
            Assembly assembly = Assembly.Load(Utilities.ParseAssembly(AssemblyFulldescription));
            if (assembly == null)
                return false;

            Type _typetoload = assembly.GetType(Utilities.ParseType(AssemblyFulldescription));
            if (_typetoload == null)
                return false;

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
        /// GeActiveProvidersList method implementation
        /// </summary>
        internal static List<IExternalProvider> GeActiveProvidersList()
        {
            List<IExternalProvider> temp = new List<IExternalProvider>();
            foreach (IExternalProvider pp in _lst.Values)
            {
                if (pp.Enabled)
                {
                    if (!(pp is NeosPlugExternalProvider))
                        temp.Add(pp);
                }
            }
            return temp;
        }

        /// <summary>
        /// GetActiveProvidersCount method implementation
        /// </summary>
        internal static int GetActiveProvidersCount()
        {
            int i = 0;
            foreach (IExternalProvider pp in _lst.Values)
            {
                if (pp.Enabled)
                {
                    if (!(pp is NeosPlugExternalProvider))
                        i++;
                }
            }
            return i;
        }

        /// <summary>
        /// GetFirstActiveProvider method implementation
        /// </summary>
        internal static IExternalProvider GetFirstActiveProvider()
        {
            foreach (IExternalProvider pp in _lst.Values)
            {
                if (pp.Enabled)
                {
                    if (!(pp is NeosPlugExternalProvider))
                        return pp;
                }
            }
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
            if ((prov == null) || (prov is NeosPlugExternalProvider))
                return false;
            else
                return prov.IsAvailable(ctx);
        }

        /// <summary>
        /// IsProviderAvailableForUser method provider
        /// </summary>
        internal static bool IsProviderAvailableForUser(AuthenticationContext ctx, PreferredMethod method)
        {
            IExternalProvider prov = GetProvider(method);
            if ((prov == null) || (prov is NeosPlugExternalProvider))
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
        /// IsPinCodeRequired method provider
        /// </summary>
        internal static bool IsPinCodeRequired(AuthenticationContext ctx)
        {
            foreach (IExternalProvider prov in Providers.Values)
            {
                if ((prov == null) || (prov is NeosPlugExternalProvider))
                    return false;
                else
                {
                    if (prov.PinRequired)
                        return true;
                }
            }
            return false;
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
                            try
                            {
                                if (string.IsNullOrEmpty(cfg.OTPProvider.FullQualifiedImplementation))
                                    provider = new NeosOTPProvider();
                                else
                                    provider = LoadExternalProvider(cfg.OTPProvider.FullQualifiedImplementation);
                                if (provider != null)
                                {
                                    if (provider.Kind == PreferredMethod.Code)
                                    {
                                        AddOrUpdateProvider(meth, provider);
                                        provider.Initialize(new OTPProviderParams(cfg.OTPProvider));
                                    }
                                    else
                                        throw new Exception("Invalid Provider Kind !");
                                }
                                else
                                    throw new Exception("Invalid Provider Kind !");
                            }
                            catch (Exception)
                            {
                                provider = null;
                            }
                        }
                        break;
                    case PreferredMethod.Email:
                        provider = GetProviderInstance(meth);
                        if (provider == null)
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(cfg.MailProvider.FullQualifiedImplementation))
                                    provider = new NeosMailProvider();
                                else
                                    provider = LoadExternalProvider(cfg.MailProvider.FullQualifiedImplementation);
                                if (provider != null)
                                {
                                    if (provider.Kind == PreferredMethod.Email)
                                    {
                                        AddOrUpdateProvider(meth, provider);
                                        provider.Initialize(new MailProviderParams(cfg.MailProvider));
                                    }
                                    else
                                        throw new Exception("Invalid Provider Kind !");
                                }
                                else
                                    throw new Exception("Invalid Provider Kind !");
                            }
                            catch (Exception)
                            {
                                provider = null;
                            }
                        }
                        break;
                    case PreferredMethod.External:
                        provider = GetProviderInstance(meth);
                        if (provider == null)
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(cfg.ExternalProvider.FullQualifiedImplementation))
                                    provider = new NeosPlugExternalProvider();
                                else if (IsLegacyExternalWrapper(cfg.ExternalProvider.FullQualifiedImplementation))
                                    provider = new NeosLegacySMSProvider();
                                else
                                    provider = LoadExternalProvider(cfg.ExternalProvider.FullQualifiedImplementation);
                                if (provider != null)
                                {
                                    if (provider.Kind == PreferredMethod.External)
                                    {
                                        AddOrUpdateProvider(meth, provider);
                                        provider.Initialize(new ExternalProviderParams(cfg.ExternalProvider));
                                    }
                                    else
                                        throw new Exception("Invalid Provider Kind !");
                                }
                                else
                                    throw new Exception("Invalid Provider Kind !");
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
                                if (string.IsNullOrEmpty(cfg.AzureProvider.FullQualifiedImplementation))
                                    provider = LoadAzureProvider();
                                else
                                    provider = LoadExternalProvider(cfg.AzureProvider.FullQualifiedImplementation);
                                if (provider != null)
                                {
                                    if (provider.Kind == PreferredMethod.Azure)
                                    {
                                        AddOrUpdateProvider(meth, provider);
                                        provider.Initialize(new AzureProviderParams(cfg.AzureProvider, cfg.Hosts.ADFSFarm.FarmIdentifier, cfg.Issuer));
                                    }
                                    else
                                        throw new Exception("Invalid Provider Kind !");
                                }
                                else
                                    throw new Exception("Invalid Provider Kind !");
                            }
                            catch (Exception)
                            {
                                provider = null;
                            }
                        }
                        break;
                    case PreferredMethod.Biometrics:
                        provider = GetProviderInstance(meth);
                        if (provider == null)
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(cfg.WebAuthNProvider.FullQualifiedImplementation))
                                    provider = LoadWebAuthNProvider();
                                else
                                    provider = LoadExternalProvider(cfg.WebAuthNProvider.FullQualifiedImplementation);
                                if (provider != null)
                                {
                                    if (provider.Kind == PreferredMethod.Biometrics)
                                    {
                                        AddOrUpdateProvider(meth, provider);
                                        provider.Initialize(new WebAuthNProviderParams(cfg, cfg.WebAuthNProvider));
                                    }
                                    else
                                        throw new Exception("Invalid Provider Kind !");
                                }
                                else
                                    throw new Exception("Invalid Provider Kind !");
                            }
                            catch (Exception)
                            {
                                provider = null;
                            }
                        }
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
    #endregion

    #region RuntimeRepository
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

        #region DataRepositoryService
        /// <summary>
        /// GetMFAUser method implementation
        /// </summary>
        internal static MFAUser GetMFAUser(MFAConfig cfg, string upn)
        {
            DataRepositoryService client = null;
            switch (cfg.StoreMode)
            {
                case DataRepositoryKind.ADDS:
                    client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                    break;
                case DataRepositoryKind.SQL:
                    client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                    break;
                case DataRepositoryKind.Custom:
                    client = CustomDataRepositoryCreator.CreateDataRepositoryInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow);
                    break;
            }
            client.OnKeyDataEvent += KeyDataEvent;
            return client.GetMFAUser(upn);
        }

        /// <summary>
        /// SetMFAUser method implementation
        /// </summary>
        internal static MFAUser SetMFAUser(MFAConfig cfg, MFAUser reg, bool resetkey = false, bool caninsert = true, bool email = false)
        {
            if (reg == null)
                new ArgumentNullException("user information");
            DataRepositoryService client = null;
            switch (cfg.StoreMode)
            {
                case DataRepositoryKind.ADDS:
                    client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                    break;
                case DataRepositoryKind.SQL:
                    client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                    break;
                case DataRepositoryKind.Custom:
                    client = CustomDataRepositoryCreator.CreateDataRepositoryInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow);
                    break;
            }
            client.OnKeyDataEvent += KeyDataEvent;
            if (reg.PIN <= 0)
                reg.PIN = cfg.DefaultPin;
            if (reg.OverrideMethod == null)
                reg.OverrideMethod = string.Empty;
            MFAUser newreg = client.SetMFAUser(reg, resetkey, caninsert);
            if (newreg != null)
            {
                if ((email) && (resetkey))
                {
                    if (!string.IsNullOrEmpty(newreg.MailAddress))
                    {
                        string qrcode = KeysManager.EncodedKey(newreg.UPN);
                        CultureInfo info = null;
                        try
                        {
                            info = CultureInfo.CurrentUICulture;
                        }
                        catch
                        {
                            info = new CultureInfo(cfg.DefaultCountryCode);
                        }
                        MailUtilities.SendKeyByEmail(newreg.MailAddress, newreg.UPN, qrcode, cfg.MailProvider, cfg, info);
                    }
                }
            }
            return newreg;
        }

        /// <summary>
        /// AddMFAUser method implementation
        /// </summary>
        internal static MFAUser AddMFAUser(MFAConfig cfg, MFAUser reg, bool resetkey = true, bool canupdate = true, bool email = false)
        {
            if (reg == null)
                new ArgumentNullException("user information");
            DataRepositoryService client = null;
            switch (cfg.StoreMode)
            {
                case DataRepositoryKind.ADDS:
                    client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                    break;
                case DataRepositoryKind.SQL:
                    client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                    break;
                case DataRepositoryKind.Custom:
                    client = CustomDataRepositoryCreator.CreateDataRepositoryInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow);
                    break;
            }
            if (reg.PIN <= 0)
                reg.PIN = cfg.DefaultPin;
            if (reg.OverrideMethod == null)
                reg.OverrideMethod = string.Empty;
            client.OnKeyDataEvent += KeyDataEvent;
            MFAUser newreg = client.AddMFAUser(reg, resetkey, canupdate);
            if (newreg != null)
            {
                if (email)
                {
                    if (!string.IsNullOrEmpty(newreg.MailAddress))
                    {
                        string qrcode = KeysManager.EncodedKey(newreg.UPN);
                        CultureInfo info = null;
                        try
                        {
                            info = CultureInfo.CurrentUICulture;
                        }
                        catch
                        {
                            info = new CultureInfo(cfg.DefaultCountryCode);
                        }
                        MailUtilities.SendKeyByEmail(newreg.MailAddress, newreg.UPN, qrcode, cfg.MailProvider, cfg, info);
                    }
                }
            }
            return newreg;
        }

        /// <summary>
        /// DeleteMFAUser method implementation
        /// </summary>
        internal static bool DeleteMFAUser(MFAConfig cfg, MFAUser reg, bool dropkey = true)
        {
            if (reg == null)
                new ArgumentNullException("user information");
            DataRepositoryService client = null;
            switch (cfg.StoreMode)
            {
                case DataRepositoryKind.ADDS:
                    client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                    break;
                case DataRepositoryKind.SQL:
                    client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                    break;
                case DataRepositoryKind.Custom:
                    client = CustomDataRepositoryCreator.CreateDataRepositoryInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow);
                    break;
            }
            client.OnKeyDataEvent += KeyDataEvent;
            return client.DeleteMFAUser(reg, dropkey);
        }

        /// <summary>
        /// EnableMFAUser method implementation
        /// </summary>
        internal static MFAUser EnableMFAUser(MFAConfig cfg, MFAUser reg)
        {
            if (reg == null)
                new ArgumentNullException("user information");
            DataRepositoryService client = null;
            switch (cfg.StoreMode)
            {
                case DataRepositoryKind.ADDS:
                    client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                    break;
                case DataRepositoryKind.SQL:
                    client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                    break;
                case DataRepositoryKind.Custom:
                    client = CustomDataRepositoryCreator.CreateDataRepositoryInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow);
                    break;
            }
            client.OnKeyDataEvent += KeyDataEvent;
            return client.EnableMFAUser(reg);
        }

        /// <summary>
        /// DisableMFAUser method implementation
        /// </summary>
        internal static MFAUser DisableMFAUser(MFAConfig cfg, MFAUser reg)
        {
            if (reg == null)
                new ArgumentNullException("user information");
            DataRepositoryService client = null;
            switch (cfg.StoreMode)
            {
                case DataRepositoryKind.ADDS:
                    client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                    break;
                case DataRepositoryKind.SQL:
                    client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                    break;
                case DataRepositoryKind.Custom:
                    client = CustomDataRepositoryCreator.CreateDataRepositoryInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow);
                    break;
            }
            client.OnKeyDataEvent += KeyDataEvent;
            return client.DisableMFAUser(reg);
        }

        /// <summary>
        /// GetMFAUsers method implementation
        /// </summary>
        internal static MFAUserList GetMFAUsers(MFAConfig cfg, DataFilterObject filter, DataOrderObject order, DataPagingObject paging)
        {
            DataRepositoryService client = null;
            switch (cfg.StoreMode)
            {
                case DataRepositoryKind.ADDS:
                    client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                    break;
                case DataRepositoryKind.SQL:
                    client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                    break;
                case DataRepositoryKind.Custom:
                    client = CustomDataRepositoryCreator.CreateDataRepositoryInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow);
                    break;
            }
            client.OnKeyDataEvent += KeyDataEvent;
            return client.GetMFAUsers(filter, order, paging);
        }

        /// <summary>
        /// GetAllMFAUsers method implementation
        /// </summary>
        internal static MFAUserList GetAllMFAUsers(MFAConfig cfg, DataOrderObject order, bool enabledonly = false)
        {
            DataRepositoryService client = null;
            switch (cfg.StoreMode)
            {
                case DataRepositoryKind.ADDS:
                    client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                    break;
                case DataRepositoryKind.SQL:
                    client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                    break;
                case DataRepositoryKind.Custom:
                    client = CustomDataRepositoryCreator.CreateDataRepositoryInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow);
                    break;
            }
            client.OnKeyDataEvent += KeyDataEvent;
            return client.GetMFAUsersAll(order, enabledonly);
        }

        /// <summary>
        /// GetMFAUsersCount method implementation
        /// </summary>
        internal static int GetMFAUsersCount(MFAConfig cfg, DataFilterObject filter)
        {
            DataRepositoryService client = null;
            switch (cfg.StoreMode)
            {
                case DataRepositoryKind.ADDS:
                    client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                    break;
                case DataRepositoryKind.SQL:
                    client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                    break;
                case DataRepositoryKind.Custom:
                    client = CustomDataRepositoryCreator.CreateDataRepositoryInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow);
                    break;
            }
            client.OnKeyDataEvent += KeyDataEvent;
            return client.GetMFAUsersCount(filter);
        }
        #endregion

        #region IWebAuthNDataRepositoryService
        /// <summary>
        /// GetUser method implementation
        /// </summary>
        internal static MFAWebAuthNUser GetUser(MFAConfig cfg, string username)
        {
            IWebAuthNDataRepositoryService client = null;
            switch (cfg.StoreMode)
            {
                case DataRepositoryKind.ADDS:
                    client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
                case DataRepositoryKind.SQL:
                    client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
                case DataRepositoryKind.Custom:
                    client = CustomDataRepositoryCreator.CreateDataRepositoryInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
            }
            return client.GetUser(cfg.WebAuthNProvider.Configuration.ChallengeSize, username);
        }

        /// <summary>
        /// GetCredentialsByUser method implementation
        /// </summary>
        internal static List<MFAUserCredential> GetCredentialsByUser(MFAConfig cfg, MFAWebAuthNUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user information");
            IWebAuthNDataRepositoryService client = null;
            switch (cfg.StoreMode)
            {
                case DataRepositoryKind.ADDS:
                    client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
                case DataRepositoryKind.SQL:
                    client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
                case DataRepositoryKind.Custom:
                    client = CustomDataRepositoryCreator.CreateDataRepositoryInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
            }
            return client.GetCredentialsByUser(user);
        }

        /// <summary>
        /// GetCredentialById method implementation
        /// </summary>
        internal static MFAUserCredential GetCredentialById(MFAConfig cfg, MFAWebAuthNUser user, byte[] id)
        {
            if (user == null)
                throw new ArgumentNullException("user information");
            IWebAuthNDataRepositoryService client = null;
            switch (cfg.StoreMode)
            {
                case DataRepositoryKind.ADDS:
                    client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
                case DataRepositoryKind.SQL:
                    client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
                case DataRepositoryKind.Custom:
                    client = CustomDataRepositoryCreator.CreateDataRepositoryInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
            }
            return client.GetCredentialById(user, id);
        }

        /// <summary>
        /// GetCredentialsByUserHandle method implementation
        /// </summary>
        internal static List<MFAUserCredential> GetCredentialsByUserHandle(MFAConfig cfg, MFAWebAuthNUser user, byte[] userHandle)
        {
            if (user == null)
                throw new ArgumentNullException("user information");
            IWebAuthNDataRepositoryService client = null;
            switch (cfg.StoreMode)
            {
                case DataRepositoryKind.ADDS:
                    client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
                case DataRepositoryKind.SQL:
                    client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
                case DataRepositoryKind.Custom:
                    client = CustomDataRepositoryCreator.CreateDataRepositoryInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
            }
            return client.GetCredentialsByUserHandle(user, userHandle);
        }

        /// <summary>
        /// UpdateCounter method implementation
        /// </summary>
        internal static void UpdateCounter(MFAConfig cfg, MFAWebAuthNUser user, byte[] credentialId, uint counter)
        {
            if (user == null)
                throw new ArgumentNullException("user information");
            IWebAuthNDataRepositoryService client = null;
            switch (cfg.StoreMode)
            {
                case DataRepositoryKind.ADDS:
                    client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
                case DataRepositoryKind.SQL:
                    client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
                case DataRepositoryKind.Custom:
                    client = CustomDataRepositoryCreator.CreateDataRepositoryInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
            }
            client.UpdateCounter(user, credentialId, counter);
        }

        /// <summary>
        /// AddUserCredential method implementation
        /// </summary>
        internal static void AddUserCredential(MFAConfig cfg, MFAWebAuthNUser user, MFAUserCredential credential)
        {
            if (user == null)
                throw new ArgumentNullException("user information");
            IWebAuthNDataRepositoryService client = null;
            switch (cfg.StoreMode)
            {
                case DataRepositoryKind.ADDS:
                    client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
                case DataRepositoryKind.SQL:
                    client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
                case DataRepositoryKind.Custom:
                    client = CustomDataRepositoryCreator.CreateDataRepositoryInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
            }
            client.AddUserCredential(user, credential);
        }

        /// <summary>
        /// RemoveUserCredential method implementation
        /// </summary>
        internal static void RemoveUserCredential(MFAConfig cfg, MFAWebAuthNUser user, string credentialid)
        {
            if (user == null)
                throw new ArgumentNullException("user information");
            IWebAuthNDataRepositoryService client = null;
            switch (cfg.StoreMode)
            {
                case DataRepositoryKind.ADDS:
                    client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
                case DataRepositoryKind.SQL:
                    client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
                case DataRepositoryKind.Custom:
                    client = CustomDataRepositoryCreator.CreateDataRepositoryInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
            }
            client.RemoveUserCredential(user, credentialid);
        }

        /// <summary>
        /// GetUsersByCredentialId method implementation
        /// </summary>
        internal static List<MFAWebAuthNUser> GetUsersByCredentialId(MFAConfig cfg, MFAWebAuthNUser user, byte[] credentialId)
        {
            if (user == null)
                throw new ArgumentNullException("user information");
            IWebAuthNDataRepositoryService client = null;
            switch (cfg.StoreMode)
            {
                case DataRepositoryKind.ADDS:
                    client = new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
                case DataRepositoryKind.SQL:
                    client = new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
                case DataRepositoryKind.Custom:
                    client = CustomDataRepositoryCreator.CreateDataRepositoryInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
            }
            return client.GetUsersByCredentialId(user, credentialId);
        }
        #endregion

        #region IDataRepositoryADDSConnection & IDataRepositorySQLConnection
        /// <summary>
        /// CheckADDSConnection method implmentation
        /// </summary>
        internal static bool CheckADDSConnection(MFAConfig config, string domainname, string username, string password)
        {
            DataRepositoryService dt = GetADDSDataRepository(config, domainname, username, password);
            return (dt as IDataRepositoryADDSConnection).CheckConnection(domainname, username, password);
        }

        /// <summary>
        /// CheckADDSAttribute method implmentation
        /// </summary>
        internal static bool CheckADDSAttribute(MFAConfig config,  string domainname, string username, string password, string attributename, int multivalued)
        {
            DataRepositoryService dt = GetDataRepository(config, DataRepositoryKind.ADDS);
            return (dt as IDataRepositoryADDSConnection).CheckAttribute(domainname, username, password, attributename, multivalued);
        }

        /// <summary>
        /// CheckSQLConnection method implmentation
        /// </summary>
        internal static bool CheckSQLConnection(MFAConfig config, string connectionstring)
        {
            DataRepositoryService dt = GetDataRepository(config, DataRepositoryKind.SQL);
            return (dt as IDataRepositorySQLConnection).CheckConnection(connectionstring);
        }

        /// <summary>
        /// CheckKeysConnection method implmentation
        /// </summary>
        internal static bool CheckKeysConnection(MFAConfig config, string connectionstring)
        {
            ISecretKeyManager dt = GetKeysRepository(config);
            if (dt is IDataRepositorySQLConnection)
                return (dt as IDataRepositorySQLConnection).CheckConnection(connectionstring);
            return false;
        }
        #endregion

        #region KeysReposotory management 
        /// <summary>
        /// GetKeysRepository method implmentation
        /// </summary>
        private static ISecretKeyManager GetKeysRepository(MFAConfig config)
        {
            if (KeysManager.Manager != null)
                return KeysManager.Manager;
            else
                return null;
        }

        /// <summary>
        /// GetDataRepository method implementation
        /// </summary>
        private static DataRepositoryService GetDataRepository(MFAConfig cfg, DataRepositoryKind kind)
        {
            switch (kind)
            {
                case DataRepositoryKind.SQL:
                    return new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                case DataRepositoryKind.Custom:
                    return CustomDataRepositoryCreator.CreateDataRepositoryInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow);
                default:
                    return new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
            }
        }

        /// <summary>
        /// GetDataRepository method implementation
        /// </summary>
        private static DataRepositoryService GetADDSDataRepository(MFAConfig cfg, string domain, string account, string password)
        {
            return new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow, domain, account, password);
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

        #region Password Management
        /// <summary>
        /// ChangePassword method implmentation
        /// </summary>
        internal static void ChangePassword(MFAConfig cfg, string username, string oldpassword, string newpassword)
        {
            ADDSForestUtils utl = new ADDSForestUtils();
            string dns = utl.GetForestDNSForUPN(username);
            if ((!string.IsNullOrEmpty(cfg.Hosts.ActiveDirectoryHost.Account)) && (!string.IsNullOrEmpty(cfg.Hosts.ActiveDirectoryHost.Password)))
            {
                using (var ctx = new PrincipalContext(ContextType.Domain, dns, cfg.Hosts.ActiveDirectoryHost.Account, cfg.Hosts.ActiveDirectoryHost.Password))
                {
                    using (var user = UserPrincipal.FindByIdentity(ctx, IdentityType.UserPrincipalName, username))
                    {
                        user.ChangePassword(oldpassword, newpassword);
                    }
                }
            }
            else
            {
                using (var ctx = new PrincipalContext(ContextType.Domain, dns))
                {
                    using (var user = UserPrincipal.FindByIdentity(ctx, IdentityType.UserPrincipalName, username))
                    {
                        user.ChangePassword(oldpassword, newpassword);
                    }
                }
            }
        }
        /// <summary>
        /// CanChangePassword method implmentation
        /// </summary>
        internal static bool CanChangePassword(MFAConfig cfg, string username)
        {
            ADDSForestUtils utl = new ADDSForestUtils();
            string dns = utl.GetForestDNSForUPN(username);
            if ((!string.IsNullOrEmpty(cfg.Hosts.ActiveDirectoryHost.Account)) && (!string.IsNullOrEmpty(cfg.Hosts.ActiveDirectoryHost.Password)))
            {
                using (var ctx = new PrincipalContext(ContextType.Domain, dns, cfg.Hosts.ActiveDirectoryHost.Account, cfg.Hosts.ActiveDirectoryHost.Password))
                {
                    UserPrincipal user = UserPrincipal.FindByIdentity(ctx, IdentityType.UserPrincipalName, username);
                    if (user == null)
                        return false;
                }
            }
            else
            {
                using (var ctx = new PrincipalContext(ContextType.Domain, dns))
                {
                    UserPrincipal user = UserPrincipal.FindByIdentity(ctx, IdentityType.UserPrincipalName, username);
                    if (user == null)
                        return false;
                }
            }
            return true;
        }
        #endregion
    }
    #endregion

    #region TOTP Utils
    /// <summary>
    /// OTPGenerator static class
    /// </summary>
    public partial class OTPGenerator
    {
        private int _secondsToGo;
        private int _digits;
        private int _duration;
        private string _identity;
        private byte[] _secret;
        private Int64 _timestamp;
        private byte[] _hmac;
        private int _offset;
        private int _oneTimePassword;
        private DateTime _datetime;
        private HashMode _mode = HashMode.SHA1;

        /// <summary>
        /// Constructor
        /// </summary>
        public OTPGenerator(HashMode mode = HashMode.SHA1, int duration = 30, int digits = 6)
        {
            RequestedDatetime = DateTime.UtcNow;
            _mode = mode;
            _digits = digits;
            _duration = duration;
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            timer.Tick += (s, e) => SecondsToGo = duration - Convert.ToInt32(GetUnixTimestamp(RequestedDatetime) % duration);
            timer.IsEnabled = true;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OTPGenerator(byte[] asecret, string aid, HashMode mode = HashMode.SHA1, int duration = 30, int digits = 6)
        {
            RequestedDatetime = DateTime.UtcNow;
            _mode = mode;
            _digits = digits;
            _duration = duration;
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            timer.Tick += (s, e) => SecondsToGo = duration - Convert.ToInt32(GetUnixTimestamp(RequestedDatetime) % duration);
            timer.IsEnabled = true;

            Secret = asecret;
            Identity = aid;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OTPGenerator(string ssecret, string aid, HashMode mode = HashMode.SHA1, int duration = 30, int digits = 6)
        {
            RequestedDatetime = DateTime.UtcNow;
            byte[] asecret = Base32.GetBytesFromString(ssecret);
            _mode = mode;
            _digits = digits;
            _duration = duration;
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            timer.Tick += (s, e) => SecondsToGo = duration - Convert.ToInt32(GetUnixTimestamp(RequestedDatetime) % duration);
            timer.IsEnabled = true;

            Secret = asecret;
            Identity = aid;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OTPGenerator(byte[] ssecret, string aid, DateTime datetime, HashMode mode = HashMode.SHA1, int duration = 30, int digits = 6)
        {
            RequestedDatetime = datetime;
            _mode = mode;
            _digits = digits;
            _duration = duration;
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            timer.Tick += (s, e) => SecondsToGo = duration - Convert.ToInt32(GetUnixTimestamp(RequestedDatetime) % duration);
            timer.IsEnabled = true;

            Secret = ssecret;
            Identity = aid;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OTPGenerator(string ssecret, string aid, DateTime datetime, HashMode mode = HashMode.SHA1, int duration = 30, int digits = 6)
        {
            RequestedDatetime = datetime;
            byte[] asecret = Base32.GetBytesFromString(ssecret);
            _mode = mode;
            _digits = digits;
            _duration = duration;
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            timer.Tick += (s, e) => SecondsToGo = duration - Convert.ToInt32(GetUnixTimestamp(RequestedDatetime) % duration);
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
                if (SecondsToGo == _duration)
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
        /// Duration property implementation
        /// </summary>
        public int Duration
        {
            get { return _duration; }
        }

        /// <summary>
        /// DigitsCount property implementation
        /// </summary>
        public int DigitsCount
        {
            get { return _digits; }
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
        /// OTP property implementation
        /// </summary>
        public string Digits
        {
            get { return _oneTimePassword.ToString("D"+_digits.ToString()); }
        }

        /// <summary>
        /// ComputeOneTimePassword method implmentation
        /// </summary>
        public void ComputeOTP(DateTime date)
        {
            // https://tools.ietf.org/html/rfc4226
            Timestamp = Convert.ToInt64(GetUnixTimestamp(date) / _duration);
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
            switch (this.DigitsCount)
            {
                case 4: OTP = (((Hmac[Offset + 0] & 0x7f) << 24) | ((Hmac[Offset + 1] & 0xff) << 16) | ((Hmac[Offset + 2] & 0xff) << 8) | (Hmac[Offset + 3] & 0xff)) % 10000;
                    break;
                case 5: OTP = (((Hmac[Offset + 0] & 0x7f) << 24) | ((Hmac[Offset + 1] & 0xff) << 16) | ((Hmac[Offset + 2] & 0xff) << 8) | (Hmac[Offset + 3] & 0xff)) % 100000;
                    break;
                default: OTP = (((Hmac[Offset + 0] & 0x7f) << 24) | ((Hmac[Offset + 1] & 0xff) << 16) | ((Hmac[Offset + 2] & 0xff) << 8) | (Hmac[Offset + 3] & 0xff)) % 1000000;
                    break;
                case 7: OTP =  (((Hmac[Offset + 0] & 0x7f) << 24) | ((Hmac[Offset + 1] & 0xff) << 16) | ((Hmac[Offset + 2] & 0xff) << 8) | (Hmac[Offset + 3] & 0xff)) % 10000000;
                    break;
                case 8: OTP =  (((Hmac[Offset + 0] & 0x7f) << 24) | ((Hmac[Offset + 1] & 0xff) << 16) | ((Hmac[Offset + 2] & 0xff) << 8) | (Hmac[Offset + 3] & 0xff)) % 100000000;
                    break;
            }
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
        private static readonly char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

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
        public static byte[] GetBytesFromString(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>
        /// GetStringFromByteArray method
        /// </summary>
        public static string GetStringFromByteArray(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            string str = new string(chars);
            return str; 
        }
    }
    #endregion

    #region KeysManager
    /// <summary>
    /// KeysManager static class
    /// </summary>
    public static class KeysManager
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
                    _manager = new RNGKeyManagerCreator().CreateInstance(cfg.KeysConfig.KeyVersion);
                    _manager.Initialize(cfg);
                    _isloaded = true;
                    break;
                case SecretKeyFormat.RSA:
                    if (!cfg.KeysConfig.CertificatePerUser)
                        _manager = new RSAKeyManagerCreator().CreateInstance(cfg.KeysConfig.KeyVersion);
                    else
                        _manager = new RSA2KeyManagerCreator().CreateInstance(cfg.KeysConfig.KeyVersion);
                    _manager.Initialize(cfg);
                    _isloaded = true;
                    break;
                default:
                    throw new NotImplementedException("SecretKeyManager not found !");
            }
        }

        /// <summary>
        /// Manager manager property implementation
        /// </summary>
        public static ISecretKeyManager Manager
        {
            get{ return _manager; }
        }

        /// <summary>
        /// IsLoaded property
        /// </summary>
        public static bool IsLoaded
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
        public static string EncodedKey(string upn)
        {
            return _manager.EncodedKey(upn);
        }

        /// <summary>
        /// ProbeKey method implementation
        /// </summary>
        public static byte[] ProbeKey(string upn)
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
        public static bool ValidateKey(string upn)
        {
            return _manager.ValidateKey(upn);
        }
    }
    #endregion

    #region Mail Utilities
    /// <summary>
    /// MailUtilities class
    /// </summary>
    public static class MailUtilities
    {
        private static readonly object lck = 0;

        /// <summary>
        /// SetCultureInfo method implementation
        /// </summary>
       /* internal static void SetCultureInfo(int lcid)
        {
            ResourcesLocale Resources = new ResourcesLocale(lcid);
        } */

        /// <summary>
        /// SendMail method implementation
        /// </summary>
        private static void SendMail(MailMessage Message, MailProvider mail)
        {
            SmtpClient client = new SmtpClient
            {
                Host = mail.Host,
                Port = mail.Port,
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = mail.UseSSL
            };
            if (!mail.Anonymous)
                client.Credentials = new NetworkCredential(mail.UserName, mail.Password);
            client.Send(Message);
        }

        /// <summary>
        /// SendOTPByEmail method implementation
        /// </summary>
        public static void SendOTPByEmail(string to, string upn, string code, MailProvider mail, CultureInfo culture)
        {
            string htmlres = string.Empty;
            try
            { 
                if (mail.MailOTPContent != null)
                {
                    int ctry = culture.LCID;
                    string tmp = mail.MailOTPContent.Where(c => c.LCID.Equals(ctry) || c.ParentLCID.Equals(ctry) && c.Enabled).Select(s => s.FileName).FirstOrDefault();
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
                        ResourcesLocale Resources = new ResourcesLocale(culture.LCID);
                        htmlres = Resources.GetString(ResourcesLocaleKind.Mail, "MailOTPContent");
                    }
                }
                string html = StripEmailContent(htmlres);
                string name = upn.Remove(2, upn.IndexOf('@') - 2).Insert(2, "*********");
                MailMessage Message = new MailMessage(mail.From, to)
                {
                    BodyEncoding = UTF8Encoding.UTF8,
                    IsBodyHtml = true,
                    Body = string.Format(html, mail.Company, name, code)
                };

                if (mail.DeliveryNotifications)
                    Message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure | DeliveryNotificationOptions.Delay;
                else
                    Message.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;

                lock (lck)
                {
                    Group titlegrp = Regex.Match(html, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"];
                    if (titlegrp != null)
                        Message.Subject = string.Format(titlegrp.Value, mail.Company, name, code); 
                    if (Message.Subject == string.Empty)
                    {
                        ResourcesLocale Resources = new ResourcesLocale(culture.LCID);
                        Message.Subject = Resources.GetString(ResourcesLocaleKind.Mail, "MailOTPTitle");
                    }
                }
                SendMail(Message, mail);
            }
            catch (SmtpException sm)
            {
                Log.WriteEntry(string.Format("Error Sending Email (TOTP) for user {0} with status code {1} : {2}", upn, sm.StatusCode.ToString(), sm.Message), EventLogEntryType.Error, 3700);
                throw sm;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("Error Sending Email (TOTP) for user {0}: {1}", upn, ex.Message), EventLogEntryType.Error, 3700);
                throw ex;
            }
        }

        /// <summary>
        /// SendInscriptionMail method implementation
        /// </summary>
        public static void SendInscriptionMail(string to, MFAUser user, MailProvider mail, CultureInfo culture)
        {
            string htmlres = string.Empty;
            try
            { 
                if (mail.MailAdminContent != null)
                {
                    int ctry = culture.LCID;
                    string tmp = mail.MailAdminContent.Where(c => c.LCID.Equals(ctry) || c.ParentLCID.Equals(ctry) && c.Enabled).Select(s => s.FileName).FirstOrDefault();
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
                        ResourcesLocale Resources = new ResourcesLocale(culture.LCID);
                        htmlres = Resources.GetString(ResourcesLocaleKind.Mail, "MailAdminContent");
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

                if (mail.DeliveryNotifications)
                    Message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure | DeliveryNotificationOptions.Delay;
                else
                    Message.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;

                lock (lck)
                {
                    Group titlegrp = Regex.Match(html, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"];
                    if (titlegrp != null)
                        Message.Subject = string.Format(titlegrp.Value, mail.Company, user.UPN, user.MailAddress, user.PhoneNumber, user.PreferredMethod); 
                    if (Message.Subject == string.Empty)
                    {
                        ResourcesLocale Resources = new ResourcesLocale(culture.LCID);
                        Message.Subject = string.Format(Resources.GetString(ResourcesLocaleKind.Mail, "MailAdminTitle"), user.UPN);
                    }
                }
                SendMail(Message, mail);
            }
            catch (SmtpException sm)
            {
                Log.WriteEntry(string.Format("Error Sending Email (INSCRIPTION) for user {0} with status code {1} : {2}", user.UPN, sm.StatusCode.ToString(), sm.Message), EventLogEntryType.Error, 3700);
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("Error Sending Email (INSCRIPTION) for user {0}: {1}", user.UPN, ex.Message), EventLogEntryType.Error, 3701);
            }
        }

        /// <summary>
        /// SendKeyByEmail method implementation
        /// </summary>
        public static void SendKeyByEmail(string email, string upn, string key, MailProvider mail, MFAConfig config, CultureInfo culture)
        {
            string htmlres = string.Empty;
            try
            {
                if (mail.MailKeyContent != null)
                {
                    int ctry = culture.LCID;
                    string tmp = mail.MailKeyContent.Where(c => c.LCID.Equals(ctry) || c.ParentLCID.Equals(ctry) && c.Enabled).Select(s => s.FileName).FirstOrDefault();
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
                        ResourcesLocale Resources = new ResourcesLocale(culture.LCID);
                        htmlres = Resources.GetString(ResourcesLocaleKind.Mail, "MailKeyContent");
                    }
                }

                string sendermail = GetUserBusinessEmail(upn);
                string html = StripEmailContent(htmlres);
                using (Stream qrcode = QRUtilities.GetQRCodeStream(upn, key, config))
                {
                    qrcode.Position = 0;
                    var inlineLogo = new LinkedResource(qrcode, "image/png")
                    {
                        ContentId = Guid.NewGuid().ToString()
                    };

                    MailMessage Message = new MailMessage(mail.From, email);
                    if (!string.IsNullOrEmpty(sendermail))
                        Message.CC.Add(sendermail);
                    Message.BodyEncoding = UTF8Encoding.UTF8;
                    Message.IsBodyHtml = true;

                    if (mail.DeliveryNotifications)
                        Message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure | DeliveryNotificationOptions.Delay;
                    else
                        Message.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;

                    lock (lck)
                    {
                        Group titlegrp = Regex.Match(html, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"];
                        if (titlegrp != null)
                            Message.Subject = string.Format(titlegrp.Value, mail.Company, upn, key, inlineLogo.ContentId); 
                        if (Message.Subject == string.Empty)
                        {
                            ResourcesLocale Resources = new ResourcesLocale(culture.LCID);
                            Message.Subject = Resources.GetString(ResourcesLocaleKind.Mail, "MailKeyTitle");
                        }
                    }
                    Message.Priority = MailPriority.High;

                    string body = string.Format(html, mail.Company, upn, key, inlineLogo.ContentId);
                    var view = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                    view.LinkedResources.Add(inlineLogo);
                    Message.AlternateViews.Add(view);
                    SendMail(Message, mail);
                }
            }
            catch (SmtpException sm)
            {
                Log.WriteEntry(string.Format("Error Sending Email (KEY) for user {0} with status code {1} : {2}", upn, sm.StatusCode.ToString(), sm.Message), EventLogEntryType.Error, 3700);
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("Error Sending Email (KEY) for user {0}: {1}", upn, ex.Message), EventLogEntryType.Error, 3702);
            }
        }

        /// <summary>
        /// SendNotificationByEmail method implementation
        /// </summary>
        public static void SendNotificationByEmail(MFAConfig config, MFAUser user, MailProvider mailprov, CultureInfo culture)
        {
            try
            {
                if (config == null)
                    return;
                if (!config.ChangeNotificationsOn)
                    return;
                if (config.MailProvider == null)
                    return;
                if (string.IsNullOrEmpty(user.MailAddress))
                    return;
                MailProvider mail = config.MailProvider;
                string htmlres = string.Empty;
                if (mail.MailNotifications != null)
                {
                    int ctry = culture.LCID;
                    string tmp = mail.MailNotifications.Where( c => c.LCID.Equals(ctry) || c.ParentLCID.Equals(ctry) && c.Enabled ).Select(s => s.FileName).FirstOrDefault();
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
                        ResourcesLocale Resources = new ResourcesLocale(culture.LCID);
                        htmlres = Resources.GetString(ResourcesLocaleKind.Mail, "MailNotifications");
                    }
                }
                string html = StripEmailContent(htmlres);
                MailMessage Message = new MailMessage(mail.From, user.MailAddress)
                {
                    BodyEncoding = UTF8Encoding.UTF8,
                    IsBodyHtml = true,
                    Body = string.Format(html, user.UPN, mail.Company)
                };

                if (mail.DeliveryNotifications)
                    Message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure | DeliveryNotificationOptions.Delay;
                else
                    Message.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;

                lock (lck)
                {
                    Group titlegrp = Regex.Match(html, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"];
                    if (titlegrp != null)
                        Message.Subject = string.Format(titlegrp.Value, user.UPN, mail.Company);
                    if (Message.Subject==string.Empty)
                    {
                        ResourcesLocale Resources = new ResourcesLocale(culture.LCID);
                        Message.Subject = string.Format(Resources.GetString(ResourcesLocaleKind.Mail, "MailNotificationsTitle"), user.UPN);
                    }
                }
                SendMail(Message, mail);
            }
            catch (SmtpException sm)
            {
                Log.WriteEntry(string.Format("Error Sending Email (NOTIF) for user {0} with status code {1} : {2}", user.UPN, sm.StatusCode.ToString(), sm.Message), EventLogEntryType.Error, 3700);
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("Error Sending Email (NOTIF) for user {0}: {1}", user.UPN, ex.Message), EventLogEntryType.Error, 3703);
            }
        }

        /// <summary>
        /// GetUserBusinessEmail method implmentation
        /// </summary>
        internal static string GetUserBusinessEmail(string username)
        {
            try
            {
                using (var ctx = new PrincipalContext(ContextType.Domain))
                {
                    using (var user = UserPrincipal.FindByIdentity(ctx, IdentityType.UserPrincipalName, username))
                    {
                        return user.EmailAddress;
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// ExportMailTemplates method implementation
        /// </summary>
        internal static void ExportMailTemplates(MFAConfig config, int lcid)
        {
            string htmlpath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\MFA\\MailTemplates\\" + lcid.ToString();
            ResourcesLocale Resources = new ResourcesLocale(lcid);
            MailProvider mailprov = config.MailProvider;


            string htmltemp1 = Resources.GetString(ResourcesLocaleKind.Mail, "MailOTPContent");
            if (!string.IsNullOrEmpty(htmltemp1))
            {
                if (!Directory.Exists(htmlpath))
                    Directory.CreateDirectory(htmlpath);  // Voir ACL
                File.WriteAllText(htmlpath + "\\MailOTPContent.html", htmltemp1, Encoding.UTF8);
                if (!mailprov.MailOTPContent.Exists(c => c.LCID.Equals(lcid)))
                    mailprov.MailOTPContent.Add(new SendMailFileName(lcid, htmlpath + "\\MailOTPContent.html"));
            }

            string htmltemp2 = Resources.GetString(ResourcesLocaleKind.Mail, "MailKeyContent");
            if (!string.IsNullOrEmpty(htmltemp2))
            {
                if (!Directory.Exists(htmlpath))
                    Directory.CreateDirectory(htmlpath);  // Voir ACL
                File.WriteAllText(htmlpath + "\\MailKeyContent.html", htmltemp2, Encoding.UTF8);
                if (!mailprov.MailKeyContent.Exists(c => c.LCID.Equals(lcid)))
                    mailprov.MailKeyContent.Add(new SendMailFileName(lcid, htmlpath + "\\MailKeyContent.html"));
            }            
            string htmltemp3 = Resources.GetString(ResourcesLocaleKind.Mail, "MailAdminContent");
            if (!string.IsNullOrEmpty(htmltemp3))
            {
                if (!Directory.Exists(htmlpath))
                    Directory.CreateDirectory(htmlpath);  // Voir ACL
                File.WriteAllText(htmlpath + "\\MailAdminContent.html", htmltemp3, Encoding.UTF8);
                if (!mailprov.MailAdminContent.Exists(c => c.LCID.Equals(lcid)))
                    mailprov.MailAdminContent.Add(new SendMailFileName(lcid, htmlpath + "\\MailAdminContent.html"));
            }
            string htmltemp4 = Resources.GetString(ResourcesLocaleKind.Mail, "MailNotifications");
            if (!string.IsNullOrEmpty(htmltemp4))
            {
                if (!Directory.Exists(htmlpath))
                    Directory.CreateDirectory(htmlpath);  // Voir ACL
                File.WriteAllText(htmlpath + "\\MailNotifications.html", htmltemp4, Encoding.UTF8);
                if (!mailprov.MailNotifications.Exists(c => c.LCID.Equals(lcid)))
                    mailprov.MailNotifications.Add(new SendMailFileName(lcid, htmlpath + "\\MailNotifications.html"));
            }
            return;
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
    #endregion

    #region QR Utilities
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
            string Content = string.Empty;
            if (RuntimeAuthProvider.GetProvider(PreferredMethod.Code) is ITOTPProviderParameters prv)
                Content = string.Format("otpauth://totp/{0}:{1}?secret={2}&issuer={3}&algorithm={4}&digits={5}&period={6}", config.QRIssuer, HttpUtility.UrlEncode(UPN), QRString, config.QRIssuer, config.OTPProvider.Algorithm, prv.Digits.ToString(), prv.Duration.ToString());
            else
                Content = string.Format("otpauth://totp/{0}:{1}?secret={2}&issuer={3}&algorithm={4}", config.QRIssuer, HttpUtility.UrlEncode(UPN), QRString, config.QRIssuer, config.OTPProvider.Algorithm);

            var encoder = new QrEncoding.QrEncoder(ErrorCorrectionLevel.L);
            if (!encoder.TryEncode(Content, out QrCode qr))
                return string.Empty;
            BitMatrix matrix = qr.Matrix;
            using (MemoryStream ms = new MemoryStream())
            {
                var render = new GraphicsRenderer(new FixedModuleSize(3, QuietZoneModules.Zero));
                render.WriteToStream(matrix, ImageFormat.Png, ms);
                ms.Position = 0;
                return ConvertToBase64(ms);
            }
        }

        /// <summary>
        /// GetQRCodeValue method implmentation
        /// </summary>
        public static string GetQRCodeValue(string UPN, string QRString, MFAConfig config)
        {
            if (RuntimeAuthProvider.GetProvider(PreferredMethod.Code) is ITOTPProviderParameters prv)
                return string.Format("otpauth://totp/{0}:{1}?secret={2}&issuer={3}&algorithm={4}&digits={5}&period={6}", config.QRIssuer, HttpUtility.UrlEncode(UPN), QRString, config.QRIssuer, config.OTPProvider.Algorithm, prv.Digits.ToString(), prv.Duration.ToString());
            else
                return string.Format("otpauth://totp/{0}:{1}?secret={2}&issuer={3}&algorithm={4}", config.QRIssuer, HttpUtility.UrlEncode(UPN), QRString, config.QRIssuer, config.OTPProvider.Algorithm);
        }

        /// <summary>
        /// GetQRCodeStream method implmentation
        /// </summary>
        public static Stream GetQRCodeStream(string UPN, string QRString, MFAConfig config)
        {
            string Content = string.Empty;
            if (RuntimeAuthProvider.GetProvider(PreferredMethod.Code) is ITOTPProviderParameters prv)
                Content = string.Format("otpauth://totp/{0}:{1}?secret={2}&issuer={3}&algorithm={4}&digits={5}&period={6}", config.QRIssuer, HttpUtility.UrlEncode(UPN), QRString, config.QRIssuer, config.OTPProvider.Algorithm, prv.Digits.ToString(), prv.Duration.ToString());
            else
                Content = string.Format("otpauth://totp/{0}:{1}?secret={2}&issuer={3}&algorithm={4}", config.QRIssuer, HttpUtility.UrlEncode(UPN), QRString, config.QRIssuer, config.OTPProvider.Algorithm);

            var encoder = new QrEncoding.QrEncoder(ErrorCorrectionLevel.L);
            if (!encoder.TryEncode(Content, out QrCode qr))
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
    #endregion

    #region XmlConfigSerializer
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
    #endregion

    #region Configuration Utilities
    /// <summary>
    /// CFGUtilities class
    /// </summary>
    internal static class CFGUtilities
    {
        internal static string configcachedir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\MFA\\Config\\config.db";
        internal static byte[] configcachekey = null;

        /// <summary>
        /// CFGUtilities static constructor
        /// </summary>
        static CFGUtilities()
        {
            configcachekey = ReadConfigurationKey();
        }

        internal static byte[] Key
        {
            get { return configcachekey;  }
        }

        #region ReadConfiguration
        /// <summary>
        /// ReadConfiguration method implementation
        /// </summary>
        internal static MFAConfig ReadConfiguration(PSHost Host = null)
        {
            bool haserror = false;
            MFAConfig config = null;
            try
            {
                config = ReadConfigurationFromCache();
            }
            catch
            {
                haserror = true;
                File.Delete(CFGUtilities.configcachedir);
                config = null;
            }
            if (config == null)
            {
                config = ReadConfigurationFromDatabase(Host);
                if ((haserror) && (config!=null))
                    WriteConfigurationToCache(config);
            }
            return config;
        }

        /// <summary>
        /// ReadConfigurationFromDatabase method implementation
        /// </summary>
        internal static MFAConfig ReadConfigurationFromDatabase(PSHost Host = null)
        {
            MFAConfig config = null;
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            string pth = Path.GetTempPath() + Path.GetRandomFileName();
            try
            {
                try
                {
                    SPRunSpace = RunspaceFactory.CreateRunspace();

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
                    if (SPPowerShell != null)
                        SPPowerShell.Dispose();
                }

                FileStream stm = new FileStream(pth, FileMode.Open, FileAccess.Read);
                XmlConfigSerializer xmlserializer = new XmlConfigSerializer(typeof(MFAConfig));
                using (StreamReader reader = new StreamReader(stm))
                {
                    config = (MFAConfig)xmlserializer.Deserialize(stm);
                    if ((!config.OTPProvider.Enabled) && (!config.MailProvider.Enabled) && (!config.ExternalProvider.Enabled) && (!config.AzureProvider.Enabled))
                        config.OTPProvider.Enabled = true;   // always let an active option eg : aplication in this case
                    using (AESEncryption MSIS = new AESEncryption())
                    {
                        config.KeysConfig.XORSecret = MSIS.Decrypt(config.KeysConfig.XORSecret);
                        config.Hosts.ActiveDirectoryHost.Password = MSIS.Decrypt(config.Hosts.ActiveDirectoryHost.Password);
                        config.MailProvider.Password = MSIS.Decrypt(config.MailProvider.Password);
                    };
                    Certs.InitializeAccountsSID(config.Hosts.ActiveDirectoryHost.DomainName, config.Hosts.ActiveDirectoryHost.Account, config.Hosts.ActiveDirectoryHost.Password);
                    KeysManager.Initialize(config);  // Important
                    RuntimeAuthProvider.LoadProviders(config);
                }
            }
            finally
            {
                File.Delete(pth);
            }
            return config;
        }

        /// <summary>
        /// ReadConfigurationFromCache method implementation
        /// </summary>
        internal static MFAConfig ReadConfigurationFromCache()
        {
            MFAConfig config = null;
            if (!File.Exists(CFGUtilities.configcachedir))
                return null;
            XmlConfigSerializer xmlserializer = new XmlConfigSerializer(typeof(MFAConfig));
            using (FileStream fs = new FileStream(CFGUtilities.configcachedir, FileMode.Open, FileAccess.Read))
            {

                byte[] bytes = new byte[fs.Length];
                int n = fs.Read(bytes, 0, (int)fs.Length);
                fs.Close();

                byte[] byt = null;
                using (AESEncryption aes = new AESEncryption())
                {
                    byt = aes.Decrypt(bytes);
                }

                using (MemoryStream ms = new MemoryStream(byt))
                {
                    using (StreamReader reader = new StreamReader(ms))
                    {
                        config = (MFAConfig)xmlserializer.Deserialize(ms);
                        if ((!config.OTPProvider.Enabled) && (!config.MailProvider.Enabled) && (!config.ExternalProvider.Enabled) && (!config.AzureProvider.Enabled))
                            config.OTPProvider.Enabled = true;   // always let an active option eg : aplication in this case
                        using (AESEncryption MSIS = new AESEncryption())
                        {
                            config.KeysConfig.XORSecret = MSIS.Decrypt(config.KeysConfig.XORSecret);
                            config.Hosts.ActiveDirectoryHost.Password = MSIS.Decrypt(config.Hosts.ActiveDirectoryHost.Password);
                            config.MailProvider.Password = MSIS.Decrypt(config.MailProvider.Password);
                        };
                        Certs.InitializeAccountsSID(config.Hosts.ActiveDirectoryHost.DomainName, config.Hosts.ActiveDirectoryHost.Account, config.Hosts.ActiveDirectoryHost.Password);
                        KeysManager.Initialize(config);  // Important
                        RuntimeAuthProvider.LoadProviders(config);
                    }
                }
            }
            return config;
        }

        /// <summary>
        /// ReadConfigurationFromDatabase method implementation
        /// </summary>
        internal static MFAConfig ReadConfigurationFromADFSStore(PSHost Host = null)
        {
            MFAConfig config = null;
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            string pth = Path.GetTempPath() + Path.GetRandomFileName();
            try
            {
                try
                {
                    SPRunSpace = RunspaceFactory.CreateRunspace();

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
                    if (SPPowerShell != null)
                        SPPowerShell.Dispose();
                }

                FileStream stm = new FileStream(pth, FileMode.Open, FileAccess.Read);
                XmlConfigSerializer xmlserializer = new XmlConfigSerializer(typeof(MFAConfig));
                using (StreamReader reader = new StreamReader(stm))
                {
                    config = (MFAConfig)xmlserializer.Deserialize(stm);
                }
            }
            finally
            {
                File.Delete(pth);
            }
            return config;
        }
        #endregion

        #region WriteConfiguration
        /// <summary>
        /// WriteConfiguration method implementation
        /// </summary>
        internal static MFAConfig WriteConfiguration(PSHost Host, MFAConfig config)
        {
            MFAConfig cfg = WriteConfigurationToDatabase(Host, config);
            if (cfg != null)
            {
                WriteConfigurationToCache(cfg, false);
            }
            return config;
        }

        /// <summary>
        /// WriteConfigurationToDatabase method implementation
        /// </summary>
        internal static MFAConfig WriteConfigurationToDatabase(PSHost Host, MFAConfig config, bool encrypt = true)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            string pth = Path.GetTempPath() + Path.GetRandomFileName();
            try
            {
                if (encrypt)
                {
                    using (AESEncryption MSIS = new AESEncryption())
                    {
                        config.KeysConfig.XORSecret = MSIS.Encrypt(config.KeysConfig.XORSecret);
                        config.Hosts.ActiveDirectoryHost.Password = MSIS.Encrypt(config.Hosts.ActiveDirectoryHost.Password);
                        config.MailProvider.Password = MSIS.Encrypt(config.MailProvider.Password);
                    };
                }
                config.LastUpdated = DateTime.UtcNow;
                FileStream stm = new FileStream(pth, FileMode.CreateNew, FileAccess.ReadWrite);
                XmlConfigSerializer xmlserializer = new XmlConfigSerializer(typeof(MFAConfig));
                stm.Position = 0;
                using (StreamReader reader = new StreamReader(stm))
                {
                    xmlserializer.Serialize(stm, config);
                }
                try
                {
                    SPRunSpace = RunspaceFactory.CreateRunspace();

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
                    if (SPPowerShell != null)
                        SPPowerShell.Dispose();
                }
            }
            finally
            {
                File.Delete(pth);
            }
            return config;
        }

        /// <summary>
        /// WriteConfigurationToCache method implementation
        /// </summary>
        internal static MFAConfig WriteConfigurationToCache(MFAConfig config, bool encrypt = true)
        {
            if (encrypt)
            {
                using (AESEncryption MSIS = new AESEncryption())
                {
                    config.KeysConfig.XORSecret = MSIS.Encrypt(config.KeysConfig.XORSecret);
                    config.Hosts.ActiveDirectoryHost.Password = MSIS.Encrypt(config.Hosts.ActiveDirectoryHost.Password);
                    config.MailProvider.Password = MSIS.Encrypt(config.MailProvider.Password);
                };
            }
            XmlConfigSerializer xmlserializer = new XmlConfigSerializer(typeof(MFAConfig));
            MemoryStream stm = new MemoryStream();
            using (StreamReader reader = new StreamReader(stm))
            {
                xmlserializer.Serialize(stm, config);
                stm.Position = 0;
                byte[] byt = null;
                using (AESEncryption aes = new AESEncryption())
                {
                    byt = aes.Encrypt(stm.ToArray());
                }
                using (FileStream fs = new FileStream(CFGUtilities.configcachedir, FileMode.Create, FileAccess.ReadWrite))
                {
                    fs.Write(byt, 0, byt.Length);
                    fs.Close();
                }
                return config;
            }
        }
        #endregion

        #region Admin Key Reader
        /// <summary>
        /// ReadConfigurationKey method implmentation
        /// </summary>
        internal static byte[] ReadConfigurationKey()
        {
            if (configcachekey != null)
                return configcachekey;

            Domain dom = Domain.GetCurrentDomain();
            Guid gd = dom.GetDirectoryEntry().Guid;

            byte[] _key = new byte[32];
            byte[] _gd = new byte[16];
            Buffer.BlockCopy(gd.ToByteArray(), 0, _gd, 0, 16);
            Buffer.BlockCopy(_gd, 0, _key, 0, 16);
            Array.Reverse(_gd);
            Buffer.BlockCopy(_gd, 0, _key, 16, 16);
            return _key;
        }
        #endregion

        #region Informations
        /// <summary>
        /// IsWIDConfiguration method implmentation
        /// </summary>
        internal static bool IsWIDConfiguration(PSHost host = null)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("(Get-AdfsProperties).ArtifactDbConnection", true);
                pipeline.Commands.Add(exportcmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();
                foreach (var result in PSOutput)
                {
                    string cnxstring = result.BaseObject.ToString();
                    return cnxstring.ToLower().Contains("##wid");
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
            return true;
        }

        /// <summary>
        /// IsPrimaryComputer method implmentation
        /// </summary>
        internal static bool IsPrimaryComputer(PSHost host = null)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            string nodetype = string.Empty;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("(Get-AdfsSyncProperties).Role", true);
                pipeline.Commands.Add(exportcmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();
                foreach (var result in PSOutput)
                {
                    nodetype = result.BaseObject.ToString();
                    break;
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
            return nodetype.ToLower().Equals("primarycomputer");
        }
        #endregion

        #region Notifications
        /// <summary>
        /// BroadcastNotification method implmentation
        /// </summary>
        internal static void BroadcastNotification(MFAConfig cfg, NotificationsKind kind, string message, bool all)
        {
            if (string.IsNullOrEmpty(message))
                message = Environment.MachineName;
            message = GetMachineName(message);
            if (all)
            {
                switch (kind)
                {
                    case NotificationsKind.ConfigurationReload:
                        PushNotification(NotificationsKind.ConfigurationReload, message); // A voir 
                        break;
                    case NotificationsKind.ConfigurationCreated:
                        WriteConfigurationToCache(cfg);
                        PushNotification(NotificationsKind.ConfigurationCreated, message);
                        break;
                    case NotificationsKind.ConfigurationDeleted:
                        File.Delete(CFGUtilities.configcachedir);
                        PushNotification(NotificationsKind.ConfigurationDeleted, message);
                        break;
                    case NotificationsKind.ServiceStatusInError:
                        PushNotification(NotificationsKind.ServiceStatusInError, message);
                        break;
                    case NotificationsKind.ServiceStatusPending:
                        PushNotification(NotificationsKind.ServiceStatusPending, message);
                        break;
                    case NotificationsKind.ServiceStatusRunning:
                        PushNotification(NotificationsKind.ServiceStatusRunning, message);
                        break;
                    case NotificationsKind.ServiceStatusStopped:
                        PushNotification(NotificationsKind.ServiceStatusStopped, message);
                        break;
                }
            }
            foreach ( ADFSServerHost srv in cfg.Hosts.ADFSFarm.Servers)
            {
                if (Environment.MachineName.ToLower().Equals(srv.MachineName.ToLower()))
                    continue;
                switch (kind)
                {
                    case NotificationsKind.ConfigurationReload:
                        PushRemoteConfiguration(cfg, srv.FQDN);
                        PushRemoteNotification(cfg, srv.FQDN, NotificationsKind.ConfigurationReload, message);
                        break;
                    case NotificationsKind.ConfigurationCreated:
                        CreateRemoteConfiguration(cfg, srv.FQDN);
                        PushRemoteNotification(cfg, srv.FQDN, NotificationsKind.ConfigurationCreated, message);
                        break;
                    case NotificationsKind.ConfigurationDeleted:
                        DeleteRemoteConfiguration(cfg, srv.FQDN);
                        PushRemoteNotification(cfg, srv.FQDN, NotificationsKind.ConfigurationDeleted, message);
                        break;
                    case NotificationsKind.ServiceStatusInError:
                        PushRemoteNotification(cfg, srv.FQDN, NotificationsKind.ServiceStatusInError, message);
                        break;
                    case NotificationsKind.ServiceStatusPending:
                        PushRemoteNotification(cfg, srv.FQDN, NotificationsKind.ServiceStatusPending, message);
                        break;
                    case NotificationsKind.ServiceStatusRunning:
                        PushRemoteNotification(cfg, srv.FQDN, NotificationsKind.ServiceStatusRunning, message);
                        break;
                }
            }
        }

        /// <summary>
        /// PopNotification method implmentation
        /// </summary>
        internal static void PopNotification(NotificationsKind kind, string message)
        {
            switch (kind)
            {
                case NotificationsKind.ConfigurationReload:
                    PushNotification(NotificationsKind.ConfigurationReload, message);
                    break;
                case NotificationsKind.ConfigurationCreated:
                    PushNotification(NotificationsKind.ConfigurationCreated, message);
                    break;
                case NotificationsKind.ConfigurationDeleted:
                    PushNotification(NotificationsKind.ConfigurationDeleted, message);
                    break;
                case NotificationsKind.ServiceStatusInError:
                    PushNotification(NotificationsKind.ServiceStatusInError, message);
                    break;
                case NotificationsKind.ServiceStatusPending:
                    PushNotification(NotificationsKind.ServiceStatusPending, message);
                    break;
                case NotificationsKind.ServiceStatusRunning:
                    PushNotification(NotificationsKind.ServiceStatusRunning, message);
                    break;
                case NotificationsKind.ServiceStatusStopped:
                    PushNotification(NotificationsKind.ServiceStatusStopped, message);
                    break;
            }
        }

        /// <summary>
        /// PushNotification method implementation
        /// Push local notification
        /// </summary>
        internal static void PushNotification(NotificationsKind kind, string message)
        {
            using (MailSlotClient mailslot = new MailSlotClient())
            {
                if (string.IsNullOrEmpty(message))
                    mailslot.Text = Environment.MachineName;
                else
                    mailslot.Text = message;
                mailslot.SendNotification(kind);
            }
        }

        #region Remote operations
        /// <summary>
        /// PushRemoteNotification method implmentation
        /// Push Remote Notification using wsman
        /// </summary>
        internal static void PushRemoteNotification(MFAConfig cfg, string servername, NotificationsKind kind, string message)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                WSManConnectionInfo connectionInfo = new WSManConnectionInfo(false, servername, cfg.WsMan.Port, "/"+ cfg.WsMan.AppName, cfg.WsMan.ShellUri, null, cfg.WsMan.TimeOut);
                SPRunSpace = RunspaceFactory.CreateRunspace(connectionInfo);
                connectionInfo.AuthenticationMechanism = AuthenticationMechanism.NegotiateWithImplicitCredential;

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                string psscript = string.Concat("[System.Reflection.Assembly]::Load('Neos.IdentityServer.MultiFactor.Administration, Version=3.0.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2');",
                                                "[Neos.IdentityServer.MultiFactor.Administration.NotificationsEvents]::PopNotification($kind, $message);");

                PSCommand command = new PSCommand();
                command.AddCommand("Set-Variable");
                command.AddParameter("Name", "kind");
                command.AddParameter("Value", (byte)kind);
                command.AddCommand("Set-Variable");
                command.AddParameter("Name", "message");
                command.AddParameter("Value", message);
                SPPowerShell.Commands = command;
                SPPowerShell.AddScript(psscript);
                SPPowerShell.Invoke();
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
        /// GetMachineName
        /// </summary>
        private static string GetMachineName(string dnsname)
        {
            string[] st = dnsname.Split('.');
            if (st.Length > 0)
                return st[0];
            else
                return dnsname;
        }

        /// <summary>
        /// CreateRemoteConfiguration method implmentation
        /// Create New Configuration file cache on Remote server using wsman
        /// </summary>
        internal static void CreateRemoteConfiguration(MFAConfig cfg, string servername)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                string filePath = CFGUtilities.configcachedir;
                using (var fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[fs.Length];
                    int bytesRead = fs.Read(buffer, 0, Convert.ToInt32(fs.Length));

                    WSManConnectionInfo connectionInfo = new WSManConnectionInfo(false, servername, cfg.WsMan.Port, "/" + cfg.WsMan.AppName, cfg.WsMan.ShellUri, null, cfg.WsMan.TimeOut);
                    SPRunSpace = RunspaceFactory.CreateRunspace(connectionInfo);
                    connectionInfo.AuthenticationMechanism = AuthenticationMechanism.NegotiateWithImplicitCredential;

                    string psscript = string.Concat("$fs=[IO.File]::Open($filepath, [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::Write);",
                                                    "$fs.Write($bytes, 0, $bytes.Length);",
                                                    "$fs.Flush();",
                                                    "$fs.Dispose();");

                    SPPowerShell = PowerShell.Create();
                    SPPowerShell.Runspace = SPRunSpace;
                    SPRunSpace.Open();

                    PSCommand command = new PSCommand();
                    command.AddCommand("Set-Variable");
                    command.AddParameter("Name", "bytes");
                    command.AddParameter("Value", buffer);
                    command.AddCommand("Set-Variable");
                    command.AddParameter("Name", "filepath");
                    command.AddParameter("Value", filePath);
                    SPPowerShell.Commands = command;
                    SPPowerShell.AddScript(psscript);
                    SPPowerShell.Invoke();
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
        /// PushRemoteConfiguration method implmentation
        /// Push Configuration file cache to Remote server using wsman
        /// </summary>
        internal static void PushRemoteConfiguration(MFAConfig cfg, string servername)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                string filePath = CFGUtilities.configcachedir;
                using (var fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[fs.Length];
                    int bytesRead = fs.Read(buffer, 0, Convert.ToInt32(fs.Length));

                    WSManConnectionInfo connectionInfo = new WSManConnectionInfo(false, servername, cfg.WsMan.Port, "/" + cfg.WsMan.AppName, cfg.WsMan.ShellUri, null, cfg.WsMan.TimeOut);
                    SPRunSpace = RunspaceFactory.CreateRunspace(connectionInfo);
                    connectionInfo.AuthenticationMechanism = AuthenticationMechanism.NegotiateWithImplicitCredential;

                    string psscript = string.Concat("$fs=[IO.File]::Open($filepath, [IO.FileMode]::Create, [IO.FileAccess]::Write);",
                                                    "$fs.Write($bytes, 0, $bytes.Length);",
                                                    "$fs.Flush();",
                                                    "$fs.Dispose();");

                    SPPowerShell = PowerShell.Create();
                    SPPowerShell.Runspace = SPRunSpace;
                    SPRunSpace.Open();

                    PSCommand command = new PSCommand();
                    command.AddCommand("Set-Variable");
                    command.AddParameter("Name", "bytes");
                    command.AddParameter("Value", buffer);
                    command.AddCommand("Set-Variable");
                    command.AddParameter("Name", "filepath");
                    command.AddParameter("Value", filePath);
                    SPPowerShell.Commands = command;
                    SPPowerShell.AddScript(psscript);
                    SPPowerShell.Invoke();
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
        /// DeleteRemoteConfiguration method implmentation
        /// Delete Configuration file cache on Remote server using wsman
        /// </summary>
        internal static void DeleteRemoteConfiguration(MFAConfig cfg, string servername)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                string filePath = CFGUtilities.configcachedir;

                WSManConnectionInfo connectionInfo = new WSManConnectionInfo(false, servername, cfg.WsMan.Port, "/" + cfg.WsMan.AppName, cfg.WsMan.ShellUri, null, cfg.WsMan.TimeOut);
                SPRunSpace = RunspaceFactory.CreateRunspace(connectionInfo);
                connectionInfo.AuthenticationMechanism = AuthenticationMechanism.NegotiateWithImplicitCredential;

                string psscript = @"$fs=[IO.File]::Delete($filepath);";

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                PSCommand command = new PSCommand();
                command.AddCommand("Set-Variable");
                command.AddParameter("Name", "filepath");
                command.AddParameter("Value", filePath);
                SPPowerShell.Commands = command;
                SPPowerShell.AddScript(psscript);
                SPPowerShell.Invoke();
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
        }
        #endregion
        #endregion
    }
    #endregion

    #region Utilities
    /// <summary>
    /// Utilities class
    /// </summary>
    public static class Utilities
    {
        
        /// <summary>
        /// GetRandomOTP  method implementation
        /// </summary>
        public static int GetRandomOTP()
        {
            RandomNumberGenerator rnd = new RNGCryptoServiceProvider();
            byte[] buffer = new byte[4];
            rnd.GetBytes(buffer);
            uint val = BitConverter.ToUInt32(buffer, 0) % 1000000;
            return Convert.ToInt32(val);
        }

        /// <summary>
        /// GetEmailOTP method implmentation
        /// </summary>
        internal static int GetEmailOTP(AuthenticationContext ctx, MailProvider mail, CultureInfo culture)
        {
            MFAUser reg = (MFAUser)ctx;
            int otpres = GetRandomOTP();
            MailUtilities.SendOTPByEmail(reg.MailAddress, reg.UPN, otpres.ToString("D"), mail, culture);
            ctx.Notification = (int)AuthenticationResponseKind.EmailOTP;
            return otpres;
        }

        /// <summary>
        /// CheckForReplay method implementation
        /// </summary>
        internal static bool CheckForReplay(MFAConfig config, AuthenticationContext usercontext, HttpListenerRequest request, int totp)
        {
            bool OK = false;
            ReplayClient replaymanager = new ReplayClient();
            try
            {
                replaymanager.Initialize();
                IReplay client = replaymanager.Open();
                try
                {
                    ReplayRecord message = new ReplayRecord()
                    {
                        MustDispatch = (config.Hosts.ADFSFarm.Servers.Count > 1),
                        ReplayLevel = config.ReplayLevel,
                        Code = totp,
                        UserIPAdress = request.RemoteEndPoint.Address.ToString(),
                        UserName = usercontext.UPN,
                        UserLogon = usercontext.LogonDate,
                        DeliveryWindow = config.DeliveryWindow
                    };
                    string fqdn = Dns.GetHostEntry("localhost").HostName;
                    List<string> srv = (from servers in config.Hosts.ADFSFarm.Servers
                                        where (servers.FQDN.ToLower() != fqdn.ToLower())
                                        select servers.FQDN.ToLower()).ToList<string>();
                    OK = client.Check(srv, message);
                }
                catch (Exception)
                {
                    replaymanager.UnInitialize();
                    return true;
                }
                finally
                {
                    replaymanager.Close(client);
                }
            }
            catch (Exception)
            {
                return true;
            }
            return OK;
        }

        /// <summary>
        /// LoadExternalProviderPluggin method implmentation
        /// </summary>
        internal static IExternalProvider LoadExternalProviderPluggin(string fqiassembly)
        {
            try
            {
                Assembly assembly = Assembly.Load(ParseAssembly(fqiassembly));
                Type _typetoload = assembly.GetType(ParseType(fqiassembly));
                if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IExternalProvider") != null)
                    return (Activator.CreateInstance(_typetoload, true) as IExternalProvider); // Allow Calling internal Constructors
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// LoadLegacyExternalProviderPluggin method implmentation
        /// </summary>
        internal static IExternalOTPProvider LoadLegacyExternalProviderPluggin(string fqiassembly)
        {
            try
            {
                Assembly assembly = Assembly.Load(ParseAssembly(fqiassembly));
                Type _typetoload = assembly.GetType(ParseType(fqiassembly));
                if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IExternalOTPProvider") != null)
                    return (Activator.CreateInstance(_typetoload, true) as IExternalOTPProvider); // Allow Calling internal Constructors
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// LoadExternalStoragePluggin method implmentation
        /// </summary>
        internal static bool CheckExternalStoragePluggin(CustomStoreHost host, int deliverywindow)
        {
            try
            {
                Assembly assembly = Assembly.Load(ParseAssembly(host.DataRepositoryFullyQualifiedImplementation));
                Type _typetoload = assembly.GetType(ParseType(host.DataRepositoryFullyQualifiedImplementation));
                Type _ancestor = _typetoload.BaseType;
                if (_ancestor.IsClass && _ancestor.IsAbstract && (_ancestor == typeof(DataRepositoryService)))
                   if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IWebAuthNDataRepositoryService") != null)
                      return ((Activator.CreateInstance(_typetoload, new object[] { host, deliverywindow }) as DataRepositoryService) != null); 
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// LoadExternalStoragePluggin method implmentation
        /// </summary>
        internal static bool CheckExternalKeysStoragePluggin(CustomStoreHost host, int deliverywindow)
        {
            try
            {
                Assembly assembly = Assembly.Load(ParseAssembly(host.KeysRepositoryFullyQualifiedImplementation));
                Type _typetoload = assembly.GetType(ParseType(host.KeysRepositoryFullyQualifiedImplementation));
                Type _ancestor = _typetoload.BaseType;
                if (_ancestor.IsClass && _ancestor.IsAbstract && (_ancestor == typeof(KeysRepositoryService)))
                    return ((Activator.CreateInstance(_typetoload, new object[] { host, deliverywindow }) as KeysRepositoryService) != null); 
                return false;
            }
            catch (Exception)
            {
                return false;
            }
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
        /// LoadExternalKeyManagerWrapper method implmentation
        /// </summary>
        internal static ISecretKeyManagerCreator LoadExternalKeyManagerCreator(string AssemblyFulldescription)
        {
            Assembly assembly = Assembly.Load(Utilities.ParseAssembly(AssemblyFulldescription));
            Type _typetoload = assembly.GetType(Utilities.ParseType(AssemblyFulldescription));
            ISecretKeyManagerCreator wrapper = null;
            try
            {
                if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("ISecretKeyManagerCreator") != null)
                {
                    object o = Activator.CreateInstance(_typetoload, true); // Allow Calling internal Constructors
                    if (o is ISecretKeyManagerCreator)
                        wrapper = o as ISecretKeyManagerCreator;
                }
            }
            catch
            {
                return null;
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
        public static bool ValidateEmail(string email, bool checkempty = false)
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
        /// ChedkEmailValidity method implementation
        /// </summary>
        public static bool ChedkEmailValidity(string email, List<string> allowed, List<string> blocked, bool checkempty = false)
        {
            try
            {
                string dom = GetEmailDomain(email);

                if (allowed.Count > 0)
                {
                    bool isallowed = false;
                    foreach (string s in allowed)
                    {
                        if (dom.ToLower().EndsWith(s))
                            isallowed = true;
                    }
                    if (!isallowed)
                        return false;
                }
                else if (blocked.Count > 0)
                {
                    bool isblocked = false;
                    foreach (string s in blocked)
                    {
                        if (dom.ToLower().EndsWith(s))
                            isblocked = true;
                    }
                    if (isblocked)
                        return false;
                }
                return ValidateEmail(email, checkempty);
            }
            catch (Exception ex)
            {
                Log.WriteEntry(ex.Message, EventLogEntryType.Error, 800);
                throw ex;
            }
        }

        /// <summary>
        /// ValidatePhoneNumber method implementation
        /// </summary>
        public static bool ValidatePhoneNumber(string phone, bool checkempty = false)
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
        /// StripDisplayKey method implmentation
        /// </summary>
        public static string StripDisplayKey(string dkey)
        {
            if ((dkey != null) && (dkey.Length >= 5))
                return dkey.Substring(0, 5) + " ... (truncated for security reasons) ... ";
            else
                return " ... (invalid key) ... ";
        }

        /// <summary>
        /// StripEmailAddress method
        /// </summary>
        public static string StripEmailAddress(string email)
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
        /// StripEmailAddress method
        /// </summary>
        internal static string GetEmailDomain(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                    return string.Empty;
                else
                    return email.Remove(0, email.IndexOf('@') +1);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// StripPhoneNumer method
        /// </summary>
        public static string StripPhoneNumber(string phone)
        {
            try
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[]
		        {
			        Regex.Replace(phone.Substring(0, phone.Length - 2), "[0-9]", "x"),
                    phone.Substring(phone.Length - 2)
		        });
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// StripPhoneNumer method
        /// </summary>
        public static string StripPinCode(int ipin)
        {
            try
            {
                string pin = ipin.ToString();
                return string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[]
                {
                    Regex.Replace(pin.Substring(0, pin.Length - 1), "[0-9]", "x"),
                    pin.Substring(pin.Length - 1)
                });
            }
            catch
            {
                return "****";
            }
        }

        /// <summary>
        /// CanCancelWizard method implementation
        /// </summary>
        public static bool CanCancelWizard(AuthenticationContext usercontext, IExternalProvider prov, ProviderPageMode mode)
        {
            switch (usercontext.WizContext)
            {
                case WizardContextMode.Invitation:
                case WizardContextMode.Registration:
                    return false;
                case WizardContextMode.ForceWizard:
                    if ((usercontext.UIMode == mode) && (prov.ForceEnrollment == ForceWizardMode.Strict))
                        return false;
                    else
                        return true;
                default:
                    return true;               
            }
        }
        
        /// <summary>
        /// FindNextWizardToPlay method implementation
        /// </summary>
        public static PreferredMethod FindNextWizardToPlay(AuthenticationContext usercontext, ref bool isrequired)
        {
            PreferredMethod current = usercontext.EnrollPageID;
            int v = (int)current;
            v++;
            current = (PreferredMethod)v;
            switch (current)
            {
                case PreferredMethod.Choose:
                    goto case PreferredMethod.Code;
                case PreferredMethod.Code:
                    IExternalProvider prov1 = RuntimeAuthProvider.GetProvider(PreferredMethod.Code);
                    if ((prov1 != null) && (prov1.Enabled))
                    {
                        isrequired = prov1.IsRequired;
                        usercontext.EnrollPageID = PreferredMethod.Code;
                        return PreferredMethod.Code;
                    }
                    else
                        goto case PreferredMethod.Email;
                case PreferredMethod.Email:
                    IExternalProvider prov2 = RuntimeAuthProvider.GetProvider(PreferredMethod.Email);
                    if ((prov2 != null) && (prov2.Enabled))
                    {
                        isrequired = prov2.IsRequired;
                        usercontext.EnrollPageID = PreferredMethod.Email;
                        return PreferredMethod.Email;
                    }
                    else
                        goto case PreferredMethod.External;
                case PreferredMethod.External:
                    IExternalProvider prov3 = RuntimeAuthProvider.GetProvider(PreferredMethod.External);
                    if ((prov3 != null) && (prov3.Enabled) && (!(prov3 is NeosPlugExternalProvider)))
                    {
                        isrequired = prov3.IsRequired;
                        usercontext.EnrollPageID = PreferredMethod.External;
                        return PreferredMethod.External;
                    }
                    else
                        goto case PreferredMethod.Azure;
                case PreferredMethod.Azure:
                    goto case PreferredMethod.Biometrics;
                case PreferredMethod.Biometrics:
                    IExternalProvider prov4 = RuntimeAuthProvider.GetProvider(PreferredMethod.Biometrics);
                    if ((prov4 != null) && (prov4.Enabled))
                    {
                        isrequired = prov4.IsRequired;
                        usercontext.EnrollPageID = PreferredMethod.Biometrics;
                        return PreferredMethod.Biometrics;
                    }
                    else
                        goto case PreferredMethod.Pin;
                case PreferredMethod.Pin:
                    if (RuntimeAuthProvider.IsPinCodeRequired(usercontext))
                    {
                        isrequired = true;
                        usercontext.EnrollPageID = PreferredMethod.Pin;
                        return PreferredMethod.Pin;
                    }
                    else
                        goto case PreferredMethod.None;
                case PreferredMethod.None:
                default:
                    usercontext.EnrollPageID = PreferredMethod.Choose;
                    return PreferredMethod.None;
            }
        }

        /// <summary>
        /// PatchUserLcid method implementation
        /// </summary>
        public static void PatchUserLcid(AuthenticationContext ctx, string[] userlanguages)
        {
            try
            {
                foreach(string st in userlanguages)
                {
                    try
                    {
                        string[] dec = st.Split(';');
                        CultureInfo cult = new CultureInfo(dec[0]);
                        ctx.Lcid = cult.LCID;
                        break;
                    }
                    catch (CultureNotFoundException)
                    {
                        continue;
                    }
                }
            }
            catch
            {
               // Let the usercontext LCID as is
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
    #endregion

    #region ADFS Version
    /// <summary>
    /// RegistryVersion class
    /// </summary>
    internal class RegistryVersion
    {
        /// <summary>
        /// RegistryVersion constructor
        /// </summary>
        public RegistryVersion(bool loadlocal = true)
        {
            if (loadlocal)
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion");

                CurrentVersion = Convert.ToString(rk.GetValue("CurrentVersion"));
                ProductName = Convert.ToString(rk.GetValue("ProductName"));
                InstallationType = Convert.ToString(rk.GetValue("InstallationType"));
                CurrentBuild = Convert.ToInt32(rk.GetValue("CurrentBuild"));
                CurrentMajorVersionNumber = Convert.ToInt32(rk.GetValue("CurrentMajorVersionNumber"));
                CurrentMinorVersionNumber = Convert.ToInt32(rk.GetValue("CurrentMinorVersionNumber"));
            }
        }
        /// <summary>
        /// CurrentVersion property implementation
        /// </summary>
        public string CurrentVersion { get; set; }

        /// <summary>
        /// ProductName property implementation
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// InstallationType property implementation
        /// </summary>
        public string InstallationType { get; set; }

        /// <summary>
        /// CurrentBuild property implementation
        /// </summary>
        public int CurrentBuild { get; set; }

        /// <summary>
        /// CurrentMajorVersionNumber property implementation
        /// </summary>
        public int CurrentMajorVersionNumber { get; set; }

        /// <summary>
        /// CurrentMinorVersionNumber property implementation
        /// </summary>
        public int CurrentMinorVersionNumber { get; set; }

        /// <summary>
        /// IsWindows2019 property implementation
        /// </summary>
        public bool IsWindows2019
        {
            get { return ((this.CurrentMajorVersionNumber == 10) && (this.CurrentBuild >= 17763)); }
        }

        /// <summary>
        /// IsWindows2016 property implementation
        /// </summary>
        public bool IsWindows2016
        {
            get { return ((this.CurrentMajorVersionNumber == 10) && ((this.CurrentBuild >= 14393) && (this.CurrentBuild < 17763))); }
        }

        /// <summary>
        /// IsWindows2012R2 property implementation
        /// </summary>
        public bool IsWindows2012R2
        {
            get { return ((this.CurrentMajorVersionNumber == 0) && ((this.CurrentBuild >= 9600) && (this.CurrentBuild < 14393))); }
        }        
    }
    #endregion

    #region WebAuthNCredentialInformation
    /// <summary>
    /// WebAuthNCredentialInformation class
    /// </summary>
    public class WebAuthNCredentialInformation
    {
        public WebAuthNCredentialInformation()
        {
        }

        public string CredentialID { get; set; }
        public string CredType { get; set; }
        public DateTime RegDate { get; set; }
        public Guid AaGuid { get; set; }
        public string Type { get; set; }
        public uint SignatureCounter { get; set; }
    }
    #endregion
}