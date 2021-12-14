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
using Neos.IdentityServer.MultiFactor.Common;
using Neos.IdentityServer.MultiFactor.Data;
using Neos.IdentityServer.MultiFactor.QrEncoding;
using Neos.IdentityServer.MultiFactor.QrEncoding.Windows.Render;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
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
using System.Web;
using System.Windows.Threading;


namespace Neos.IdentityServer.MultiFactor
{
    #region ADDS Instance Activators
    /// <summary>
    /// ADDSDataRepositoryCreator class
    /// </summary>
    public static class ADDSDataRepositoryActivator
    {
        /// <summary>
        /// CreateInstance method implementation
        /// </summary>
        public static DataRepositoryService CreateInstance(BaseDataHost host, int deliverywindow)
        {
            try
            {
                Assembly assembly = Assembly.Load("Neos.IdentityServer.MultiFactor.Data, Version=3.0.0.0, Culture=neutral, "+ Utilities.GetAssemblyPublicKey());
                Type _typetoload = assembly.GetType("Neos.IdentityServer.MultiFactor.Data.ADDSKeysRepositoryService");
                Type _ancestor = _typetoload.BaseType;
                if (_ancestor.IsClass && _ancestor.IsAbstract && (_ancestor == typeof(DataRepositoryService)))
                    if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IWebAuthNDataRepositoryService") != null)
                        return (Activator.CreateInstance(_typetoload, BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic, new object[] { host, deliverywindow }) as DataRepositoryService);
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// ADDSKeysRepositoryCreator class
    /// </summary>
    public static class ADDSKeysRepositoryActivator
    {
        /// <summary>
        /// CreateInstance method implementation
        /// </summary>
        public static KeysRepositoryService CreateInstance(BaseDataHost host, int deliverywindow)
        {
            try
            {
                Assembly assembly = Assembly.Load("Neos.IdentityServer.MultiFactor.Data, Version=3.0.0.0, Culture=neutral, " + Utilities.GetAssemblyPublicKey());
                Type _typetoload = assembly.GetType("Neos.IdentityServer.MultiFactor.Data.ADDSKeysRepositoryService");
                Type _ancestor = _typetoload.BaseType;
                if (_ancestor.IsClass && _ancestor.IsAbstract && (_ancestor == typeof(KeysRepositoryService)))
                    return (Activator.CreateInstance(_typetoload, new object[] { host, deliverywindow }) as KeysRepositoryService);
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// ADDSKeys2RepositoryCreator class
    /// </summary>
    public static class ADDSKeys2RepositoryActivator
    {
        /// <summary>
        /// CreateInstance method implementation
        /// </summary>
        public static KeysRepositoryService CreateInstance(BaseDataHost host, int deliverywindow)
        {
            try
            {
                Assembly assembly = Assembly.Load("Neos.IdentityServer.MultiFactor.Data, Version=3.0.0.0, Culture=neutral, " + Utilities.GetAssemblyPublicKey());
                Type _typetoload = assembly.GetType("Neos.IdentityServer.MultiFactor.Data.ADDSKeys2RepositoryService");
                Type _ancestor = _typetoload.BaseType;
                if (_ancestor.IsClass && _ancestor.IsAbstract && (_ancestor == typeof(KeysRepositoryService)))
                    return (Activator.CreateInstance(_typetoload, new object[] { host, deliverywindow }) as KeysRepositoryService);
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
    #endregion

    #region SQL Instances Activators
    /// <summary>
    /// SQLDataRepositoryActivator class
    /// </summary>
    public static class SQLDataRepositoryActivator
    {
        /// <summary>
        /// CreateInstance method implementation
        /// </summary>
        public static DataRepositoryService CreateInstance(BaseDataHost host, int deliverywindow)
        {
            try
            {
                Assembly assembly = Assembly.Load("Neos.IdentityServer.MultiFactor.Data, Version=3.0.0.0, Culture=neutral, " + Utilities.GetAssemblyPublicKey());
                Type _typetoload = assembly.GetType("Neos.IdentityServer.MultiFactor.Data.SQLDataRepositoryService");
                Type _ancestor = _typetoload.BaseType;
                if (_ancestor.IsClass && _ancestor.IsAbstract && (_ancestor == typeof(DataRepositoryService)))
                    if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IWebAuthNDataRepositoryService") != null)
                        return (Activator.CreateInstance(_typetoload, new object[] { host, deliverywindow }) as DataRepositoryService);
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// SQLKeysRepositoryActivator class
    /// </summary>
    public static class SQLKeysRepositoryActivator
    {
        /// <summary>
        /// CreateInstance method implementation
        /// </summary>
        public static KeysRepositoryService CreateInstance(BaseDataHost host, int deliverywindow)
        {
            try
            {
                Assembly assembly = Assembly.Load("Neos.IdentityServer.MultiFactor.Data, Version=3.0.0.0, Culture=neutral, " + Utilities.GetAssemblyPublicKey());
                Type _typetoload = assembly.GetType("Neos.IdentityServer.MultiFactor.Data.SQLKeysRepositoryService");
                Type _ancestor = _typetoload.BaseType;
                if (_ancestor.IsClass && _ancestor.IsAbstract && (_ancestor == typeof(KeysRepositoryService)))
                    return (Activator.CreateInstance(_typetoload, new object[] { host, deliverywindow }) as KeysRepositoryService);
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// SQLKeys2RepositoryActivator class
    /// </summary>
    public static class SQLKeys2RepositoryActivator
    {
        /// <summary>
        /// CreateInstance method implementation
        /// </summary>
        public static KeysRepositoryService CreateInstance(BaseDataHost host, int deliverywindow)
        {
            try
            {
                Assembly assembly = Assembly.Load("Neos.IdentityServer.MultiFactor.Data, Version=3.0.0.0, Culture=neutral, " + Utilities.GetAssemblyPublicKey());
                Type _typetoload = assembly.GetType("Neos.IdentityServer.MultiFactor.Data.SQLKeys2RepositoryService");
                Type _ancestor = _typetoload.BaseType;
                if (_ancestor.IsClass && _ancestor.IsAbstract && (_ancestor == typeof(KeysRepositoryService)))
                    return (Activator.CreateInstance(_typetoload, new object[] { host, deliverywindow }) as KeysRepositoryService);
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
    #endregion

    #region Custom Instances Activators
    /// <summary>
    /// CustomDataRepositoryActivator class
    /// </summary>
    public static class CustomDataRepositoryActivator
    {
        /// <summary>
        /// CreateInstance method implementation
        /// </summary>
        public static DataRepositoryService CreateInstance(CustomStoreHost host, int deliverywindow)
        {
            try
            {
                Assembly assembly = Assembly.Load(Utilities.ParseAssembly(host.DataRepositoryFullyQualifiedImplementation));
                Type _typetoload = assembly.GetType(Utilities.ParseType(host.DataRepositoryFullyQualifiedImplementation));
                Type _ancestor = _typetoload.BaseType;
                if (_ancestor.IsClass && _ancestor.IsAbstract && (_ancestor == typeof(DataRepositoryService)))
                    if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IWebAuthNDataRepositoryService") != null)
                        return (Activator.CreateInstance(_typetoload, new object[] { host, deliverywindow }) as DataRepositoryService);
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// CustomKeysRepositoryActivator class
    /// </summary>
    public static class CustomKeysRepositoryActivator
    {
        /// <summary>
        /// CreateInstance method implementation
        /// </summary>
        public static KeysRepositoryService CreateInstance(CustomStoreHost host, int deliverywindow)
        {
            try
            {
                Assembly assembly = Assembly.Load(Utilities.ParseAssembly(host.KeysRepositoryFullyQualifiedImplementation));
                Type _typetoload = assembly.GetType(Utilities.ParseType(host.KeysRepositoryFullyQualifiedImplementation));
                Type _ancestor = _typetoload.BaseType;
                if (_ancestor.IsClass && _ancestor.IsAbstract && (_ancestor == typeof(KeysRepositoryService)))
                    return (Activator.CreateInstance(_typetoload, new object[] { host, deliverywindow }) as KeysRepositoryService);
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
    #endregion

    #region RuntimeAuthProvider
    /// <summary>
    /// RuntimeAuthProvider
    /// </summary>
    internal static class RuntimeAuthProvider
    {
        private static Dictionary<PreferredMethod, IExternalProvider> _providers = new Dictionary<PreferredMethod, IExternalProvider>();

        /// <summary>
        /// Providers property implementation
        /// </summary>
        private static Dictionary<PreferredMethod, IExternalProvider> Providers
        {
            get
            {
                if (_providers == null)
                    _providers = new Dictionary<PreferredMethod, IExternalProvider>();

                return _providers;
            }
        } 

        /// <summary>
        /// GetAuthenticationProvider method implementation
        /// </summary>
        internal static IExternalProvider GetAuthenticationProvider(MFAConfig cfg, AuthenticationContext ctx)
        {
            IExternalProvider provider = null;
            switch (ctx.PreferredMethod)
            {
                case PreferredMethod.Code:
                    provider = GetProviderInstance(PreferredMethod.Code);
                    if (provider == null)
                    {
                        try
                        {

                            if (string.IsNullOrEmpty(cfg.OTPProvider.FullQualifiedImplementation))
                                provider = new NeosOTPProvider();
                            else
                                provider = LoadExternalProvider(cfg.OTPProvider.FullQualifiedImplementation);
                            if (provider == null)
                                provider = new NeosPlugProvider(PreferredMethod.Code);
                            if (provider.Kind == PreferredMethod.Code)
                            {
                                AddOrUpdateProvider(PreferredMethod.Code, provider);
                                OTPProviderParams initdata = new OTPProviderParams(cfg.OTPProvider)
                                {
                                    Config = cfg
                                };
                                provider.Initialize(initdata);
                            }
                            else
                                throw new Exception("Invalid Provider Type !");
                        }
                        catch (Exception)
                        {
                            provider = new NeosPlugProvider(PreferredMethod.Code);
                            AddOrUpdateProvider(PreferredMethod.Code, provider);
                            OTPProviderParams initdata = new OTPProviderParams(cfg.OTPProvider)
                            {
                                Config = cfg
                            };
                            provider.Initialize(initdata);
                        }
                    }
                    break;
                case PreferredMethod.Email:
                    provider = GetProviderInstance(PreferredMethod.Email);
                    if (provider == null)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(cfg.MailProvider.FullQualifiedImplementation))
                                provider = new NeosMailProvider();
                            else
                                provider = LoadExternalProvider(cfg.MailProvider.FullQualifiedImplementation);
                            if (provider == null)
                                provider = new NeosPlugProvider(PreferredMethod.Email);
                            if (provider.Kind == PreferredMethod.Email)
                            {
                                AddOrUpdateProvider(PreferredMethod.Email, provider);
                                provider.Initialize(new MailProviderParams(cfg.MailProvider));
                            }
                            else
                                throw new Exception("Invalid Provider Type !");
                        }
                        catch (Exception)
                        {
                            provider = new NeosPlugProvider(PreferredMethod.Email);
                            AddOrUpdateProvider(PreferredMethod.Email, provider);
                            provider.Initialize(new MailProviderParams(cfg.MailProvider));
                        }
                    }
                    break;
                case PreferredMethod.External:
                    provider = GetProviderInstance(PreferredMethod.External);
                    if (provider == null)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(cfg.ExternalProvider.FullQualifiedImplementation))
                                provider = new NeosPlugProvider(PreferredMethod.External);
                            else if (IsLegacyExternalWrapper(cfg.ExternalProvider.FullQualifiedImplementation))
                                provider = new NeosLegacySMSProvider();
                            else
                                provider = LoadExternalProvider(cfg.ExternalProvider.FullQualifiedImplementation);
                            if (provider == null)
                                provider = new NeosPlugProvider(PreferredMethod.External);
                            if (provider.Kind == PreferredMethod.External)
                            {
                                AddOrUpdateProvider(PreferredMethod.External, provider);
                                provider.Initialize(new ExternalProviderParams(cfg.ExternalProvider));
                            }
                            else
                                throw new Exception("Invalid Provider Type !");
                        }
                        catch (Exception)
                        {
                            provider = new NeosPlugProvider(PreferredMethod.External);
                            AddOrUpdateProvider(PreferredMethod.External, provider);
                            provider.Initialize(new ExternalProviderParams(cfg.ExternalProvider));
                        }
                    }
                    break;
                case PreferredMethod.Azure:
                    provider = GetProviderInstance(PreferredMethod.Azure);
                    if (provider == null)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(cfg.AzureProvider.FullQualifiedImplementation))
                                provider = LoadAzureProvider();
                            else
                                provider = LoadExternalProvider(cfg.AzureProvider.FullQualifiedImplementation);
                            if (provider == null)
                                provider = new NeosPlugProvider(PreferredMethod.Azure);
                            if (provider.Kind == PreferredMethod.Azure)
                            {
                                AddOrUpdateProvider(PreferredMethod.Azure, provider);
                                provider.Initialize(new AzureProviderParams(cfg.AzureProvider, cfg.Hosts.ADFSFarm.FarmIdentifier, cfg.Issuer));
                            }
                            else
                                throw new Exception("Invalid Provider Type !");
                        }
                        catch (Exception)
                        {
                            provider = new NeosPlugProvider(PreferredMethod.Azure);
                            AddOrUpdateProvider(PreferredMethod.Azure, provider);
                            provider.Initialize(new AzureProviderParams(cfg.AzureProvider, cfg.Hosts.ADFSFarm.FarmIdentifier, cfg.Issuer));
                        }
                    }
                    break;
                case PreferredMethod.Biometrics:
                    provider = GetProviderInstance(PreferredMethod.Biometrics);
                    if (provider == null)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(cfg.WebAuthNProvider.FullQualifiedImplementation))
                                provider = LoadWebAuthNProvider(cfg.WebAuthNProvider.Options.ConstrainedMetadataRepository);
                            else
                                provider = LoadWebAuthNProvider(cfg.WebAuthNProvider.Options.ConstrainedMetadataRepository, cfg.WebAuthNProvider.FullQualifiedImplementation);
                            if (provider == null)
                                provider = new NeosPlugProvider(PreferredMethod.Biometrics);
                            if (provider.Kind == PreferredMethod.Biometrics)
                            {
                                AddOrUpdateProvider(PreferredMethod.Biometrics, provider);
                                provider.Initialize(new WebAuthNProviderParams(cfg, cfg.WebAuthNProvider));
                            }
                        }
                        catch (Exception)
                        {
                            provider = new NeosPlugProvider(PreferredMethod.Biometrics);
                            AddOrUpdateProvider(PreferredMethod.Biometrics, provider);
                            provider.Initialize(new WebAuthNProviderParams(cfg, cfg.WebAuthNProvider));
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
            Assembly assembly = Assembly.Load(@"Neos.IdentityServer.MultiFactor.SAS.Azure, Version=3.0.0.0, Culture=neutral, " + Utilities.GetAssemblyPublicKey());
            Type _typetoload = assembly.GetType(@"Neos.IdentityServer.MultiFactor.SAS.NeosAzureProvider");
            if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IExternalProvider") != null)
                return (IExternalProvider)Activator.CreateInstance(_typetoload, true); // Allow Calling internal Constructors
            else
                return null;
        }

        /// <summary>
        /// LoadWebAuthNProvider method implmentation
        /// </summary>
        private static IExternalProvider LoadWebAuthNProvider(bool constrained)
        {
            Assembly assembly = Assembly.Load(@"Neos.IdentityServer.MultiFactor.WebAuthN.Provider, Version=3.0.0.0, Culture=neutral, " + Utilities.GetAssemblyPublicKey());
            Type _typetoload = assembly.GetType(@"Neos.IdentityServer.MultiFactor.WebAuthN.NeosWebAuthNProvider");
            if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IExternalProvider") != null)
                return (IExternalProvider)Activator.CreateInstance(_typetoload, new object[] { constrained }); // Allow Calling internal Constructors
            else
                return null;
        }

        /// <summary>
        /// LoadWebAuthNProvider method implmentation
        /// </summary>
        private static IExternalProvider LoadWebAuthNProvider(bool constrained, string AssemblyFulldescription)
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
                return (IExternalProvider)Activator.CreateInstance(_typetoload, new object[] { constrained }); // Allow Calling internal Constructors
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
                Providers.Add(method, prov);
            else
                Providers[method] = prov;
        }

        /// <summary>
        /// DisableProvider method provider
        /// </summary>
        private static void RemoveProvider(PreferredMethod method)
        {
            if (GetProviderInstance(method) != null)
                Providers.Remove(method);
        }

        /// <summary>
        /// GetProvider method provider
        /// </summary>
        internal static IExternalProvider GetProvider(PreferredMethod method)
        {
            IExternalProvider pp = GetProviderInstance(method);
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
            foreach (IExternalProvider pp in Providers.Values)
            {
                if (pp.Enabled)
                {
                    if (!(pp is NeosPlugProvider))
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
            foreach (IExternalProvider pp in Providers.Values)
            {
                if (pp.Enabled)
                {
                    if (!(pp is NeosPlugProvider))
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
            foreach (IExternalProvider pp in Providers.Values)
            {
                if (pp.Enabled)
                {
                    if (!(pp is NeosPlugProvider))
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
            if (Providers.ContainsKey(method))
                return Providers.First(x => x.Key == method).Value;
            return null;
        }

        /// <summary>
        /// IsProviderAvailable method provider
        /// </summary>
        internal static bool IsProviderAvailable(AuthenticationContext ctx, PreferredMethod method)
        {
            IExternalProvider prov = GetProvider(method);
            if ((prov == null) || (prov is NeosPlugProvider))
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
            if ((prov == null) || (prov is NeosPlugProvider))
                return false;
            else
            {
                if (prov.Enabled)
                {
                    if (prov.PinRequired)
                    {
                        if (ctx.PinCode <= 0)
                            return false;
                    }
                    if ((prov is IWebAuthNProvider web) && (web.PinRequirements != WebAuthNPinRequirements.Null))
                    {
                        if (ctx.PinCode <= 0)
                            return false;
                    }
                    if ((prov.Kind==PreferredMethod.Biometrics) && (ctx.BioNotSupported))
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
                if ((prov == null) || (prov is NeosPlugProvider))
                    return false;
                else
                {
                    if (!prov.Enabled)
                        continue;
                    if (prov.PinRequired)
                        return true;
                    if (prov is IWebAuthNProvider web)
                        return web.PinRequirements != WebAuthNPinRequirements.Null;
                }
            }
            return false;
        }

        /// <summary>
        /// LoadProviders() method implementation
        /// </summary>
        internal static void LoadProviders(MFAConfig cfg)
        {
            Providers.Clear();
            foreach (PreferredMethod meth in Enum.GetValues(typeof(PreferredMethod)))
            {
                IExternalProvider provider = null;
                switch (meth)
                {
                    case PreferredMethod.Code:
                        provider = GetProviderInstance(PreferredMethod.Code);
                        if (provider == null)
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(cfg.OTPProvider.FullQualifiedImplementation))
                                    provider = new NeosOTPProvider();
                                else
                                    provider = LoadExternalProvider(cfg.OTPProvider.FullQualifiedImplementation);
                                if (provider == null)
                                    provider = new NeosPlugProvider(PreferredMethod.Code);
                                if (provider.Kind == PreferredMethod.Code)
                                {
                                    AddOrUpdateProvider(PreferredMethod.Code, provider);
                                    OTPProviderParams initdata = new OTPProviderParams(cfg.OTPProvider)
                                    {
                                        Config = cfg
                                    };
                                    provider.Initialize(initdata);
                                }
                                else
                                    throw new Exception("Invalid Provider Type !");
                            }
                            catch (Exception)
                            {
                                provider = new NeosPlugProvider(PreferredMethod.Code);
                                AddOrUpdateProvider(PreferredMethod.Code, provider);
                                OTPProviderParams initdata = new OTPProviderParams(cfg.OTPProvider)
                                {
                                    Config = cfg
                                };
                                provider.Initialize(initdata);
                            }
                        }
                        break;
                    case PreferredMethod.Email:
                        provider = GetProviderInstance(PreferredMethod.Email);
                        if (provider == null)
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(cfg.MailProvider.FullQualifiedImplementation))
                                    provider = new NeosMailProvider();
                                else
                                    provider = LoadExternalProvider(cfg.MailProvider.FullQualifiedImplementation);
                                if (provider == null)
                                    provider = new NeosPlugProvider(PreferredMethod.Email);
                                if (provider.Kind == PreferredMethod.Email)
                                {
                                    AddOrUpdateProvider(PreferredMethod.Email, provider);
                                    provider.Initialize(new MailProviderParams(cfg.MailProvider));
                                }
                                else
                                    throw new Exception("Invalid Provider Type !");
                            }
                            catch (Exception)
                            {
                                provider = new NeosPlugProvider(PreferredMethod.Email);
                                AddOrUpdateProvider(PreferredMethod.Email, provider);
                                provider.Initialize(new MailProviderParams(cfg.MailProvider));
                            }
                        }
                        break;
                    case PreferredMethod.External:
                        provider = GetProviderInstance(PreferredMethod.External);
                        if (provider == null)
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(cfg.ExternalProvider.FullQualifiedImplementation))
                                    provider = new NeosPlugProvider(PreferredMethod.External);
                                else if (IsLegacyExternalWrapper(cfg.ExternalProvider.FullQualifiedImplementation))
                                    provider = new NeosLegacySMSProvider();
                                else
                                    provider = LoadExternalProvider(cfg.ExternalProvider.FullQualifiedImplementation);
                                if (provider == null)
                                    provider = new NeosPlugProvider(PreferredMethod.External);
                                if (provider.Kind == PreferredMethod.External)
                                {
                                    AddOrUpdateProvider(PreferredMethod.External, provider);
                                    provider.Initialize(new ExternalProviderParams(cfg.ExternalProvider));
                                }
                                else
                                    throw new Exception("Invalid Provider Type !");
                            }
                            catch (Exception)
                            {
                                provider = new NeosPlugProvider(PreferredMethod.External);
                                AddOrUpdateProvider(PreferredMethod.External, provider);
                                provider.Initialize(new ExternalProviderParams(cfg.ExternalProvider));
                            }
                        }
                        break;
                    case PreferredMethod.Azure:
                        provider = GetProviderInstance(PreferredMethod.Azure);
                        if (provider == null)
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(cfg.AzureProvider.FullQualifiedImplementation))
                                    provider = LoadAzureProvider();
                                else
                                    provider = LoadExternalProvider(cfg.AzureProvider.FullQualifiedImplementation);
                                if (provider == null)
                                    provider = new NeosPlugProvider(PreferredMethod.Azure);
                                if (provider.Kind == PreferredMethod.Azure)
                                {
                                    AddOrUpdateProvider(PreferredMethod.Azure, provider);
                                    provider.Initialize(new AzureProviderParams(cfg.AzureProvider, cfg.Hosts.ADFSFarm.FarmIdentifier, cfg.Issuer));
                                }
                                else
                                    throw new Exception("Invalid Provider Type !");
                            }
                            catch (Exception)
                            {
                                provider = new NeosPlugProvider(PreferredMethod.Azure);
                                AddOrUpdateProvider(PreferredMethod.Azure, provider);
                                provider.Initialize(new AzureProviderParams(cfg.AzureProvider, cfg.Hosts.ADFSFarm.FarmIdentifier, cfg.Issuer));
                            }
                        }
                        break;
                    case PreferredMethod.Biometrics:
                        provider = GetProviderInstance(PreferredMethod.Biometrics);
                        if (provider == null)
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(cfg.WebAuthNProvider.FullQualifiedImplementation))
                                    provider = LoadWebAuthNProvider(cfg.WebAuthNProvider.Options.ConstrainedMetadataRepository);
                                else
                                    provider = LoadWebAuthNProvider(cfg.WebAuthNProvider.Options.ConstrainedMetadataRepository, cfg.WebAuthNProvider.FullQualifiedImplementation);
                                if (provider == null)
                                    provider = new NeosPlugProvider(PreferredMethod.Biometrics);
                                if (provider.Kind == PreferredMethod.Biometrics)
                                {
                                    AddOrUpdateProvider(PreferredMethod.Biometrics, provider);
                                    provider.Initialize(new WebAuthNProviderParams(cfg, cfg.WebAuthNProvider));
                                }
                                else
                                    throw new Exception("Invalid Provider Type !");
                            }
                            catch (Exception)
                            {
                                provider = new NeosPlugProvider(PreferredMethod.Biometrics);
                                AddOrUpdateProvider(PreferredMethod.Biometrics, provider);
                                provider.Initialize(new WebAuthNProviderParams(cfg, cfg.WebAuthNProvider));
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
            Providers.Clear();
        }

        /// <summary>
        /// IsUIElementRequired method implementation
        /// </summary>
        public static bool IsUIElementRequired(AuthenticationContext ctx, RequiredMethodElements element)
        {
            foreach (KeyValuePair<PreferredMethod, IExternalProvider> prov in Providers)
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
                    client = CustomDataRepositoryActivator.CreateInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow);
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
                    client = CustomDataRepositoryActivator.CreateInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow);
                    break;
            }
            client.OnKeyDataEvent += KeyDataEvent;
            if (reg.PIN <= 0)
                reg.PIN = Convert.ToInt32(cfg.DefaultPin);
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
                    client = CustomDataRepositoryActivator.CreateInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow);
                    break;
            }
            if (reg.PIN <= 0)
                reg.PIN = Convert.ToInt32(cfg.DefaultPin);
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
                    client = CustomDataRepositoryActivator.CreateInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow);
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
                    client = CustomDataRepositoryActivator.CreateInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow);
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
                    client = CustomDataRepositoryActivator.CreateInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow);
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
                    client = CustomDataRepositoryActivator.CreateInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow);
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
                    client = CustomDataRepositoryActivator.CreateInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow);
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
                    client = CustomDataRepositoryActivator.CreateInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow);
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
                    client = CustomDataRepositoryActivator.CreateInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
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
                    client = CustomDataRepositoryActivator.CreateInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
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
                    client = CustomDataRepositoryActivator.CreateInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
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
                    client = CustomDataRepositoryActivator.CreateInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
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
                    client = CustomDataRepositoryActivator.CreateInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
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
                    client = CustomDataRepositoryActivator.CreateInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
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
                    client = CustomDataRepositoryActivator.CreateInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
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
                    client = CustomDataRepositoryActivator.CreateInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow) as IWebAuthNDataRepositoryService;
                    break;
            }
            return client.GetUsersByCredentialId(user, credentialId);
        }
        #endregion

        #region IDataRepositoryADDSConnection & IDataRepositorySQLConnection
        /// <summary>
        /// CheckADDSConnection method implmentation //ici
        /// </summary>
        internal static bool CheckADDSConnection(MFAConfig config, string domainname, string username, string password)
        {
            try
            {
                DataRepositoryService dt = GetADDSDataRepository(config, domainname, username, password);
                return (dt as IDataRepositoryADDSConnection).CheckConnection(domainname, username, password);
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("CheckADDSConnection error : {0}", ex.Message), EventLogEntryType.Error, 5000);
                throw ex;
            }
        }

        /// <summary>
        /// CheckADDSAttribute method implmentation
        /// </summary>
        internal static bool CheckADDSAttribute(MFAConfig config, string domainname, string username, string password, string attributename, int multivalued)
        {
            try
            {
                DataRepositoryService dt = GetADDSDataRepository(config, domainname, username, password);
                return (dt as IDataRepositoryADDSConnection).CheckAttribute(domainname, username, password, attributename, multivalued);
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("CheckADDSAttribute error : {0}, {1}", attributename, ex.Message), EventLogEntryType.Error, 5000);
                throw ex;
            }
        }

        /// <summary>
        /// CheckSQLConnection method implmentation
        /// </summary>
        internal static bool CheckSQLConnection(MFAConfig config, string connectionstring, string username, string password)
        {
            try
            {
                DataRepositoryService dt = GetDataRepository(config, DataRepositoryKind.SQL);
                return (dt as IDataRepositorySQLConnection).CheckConnection(connectionstring, username, password);
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("CheckSQLConnection error : {0}", ex.Message), EventLogEntryType.Error, 5000);
                throw ex;
            }
        }

        /// <summary>
        /// CheckKeysConnection method implmentation
        /// </summary>
        internal static bool CheckKeysConnection(MFAConfig config, string connectionstring, string username, string password)
        {
            try
            {
                ISecretKeyManager dt = GetKeysRepository(config);
                if (dt is IDataRepositorySQLConnection)
                    return (dt as IDataRepositorySQLConnection).CheckConnection(connectionstring, username, password);
                return false;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("CheckKeysConnection error : {0}", ex.Message), EventLogEntryType.Error, 5000);
                throw ex;
            }
        }
        #endregion

        #region KeysReposotory management 
        /// <summary>
        /// GetKeysRepository method implmentation
        /// </summary>
        private static ISecretKeyManager GetKeysRepository(MFAConfig config)
        {
            try
            {
                if (KeysManager.Manager != null)
                    return KeysManager.Manager;
                else
                    return null;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("GetKeysRepository error : {0}", ex.Message), EventLogEntryType.Error, 5000);
                throw ex;
            }
        }

        /// <summary>
        /// GetDataRepository method implementation
        /// </summary>
        private static DataRepositoryService GetDataRepository(MFAConfig cfg, DataRepositoryKind kind)
        {
            try
            {
                switch (kind)
                {
                    case DataRepositoryKind.SQL:
                        return new SQLDataRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                    case DataRepositoryKind.Custom:
                        return CustomDataRepositoryActivator.CreateInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow);
                    default:
                        return new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                }
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("GetDataRepository error : {0} {1}", kind.ToString(), ex.Message), EventLogEntryType.Error, 5000);
                throw ex;
            }
        }

        /// <summary>
        /// GetDataRepository method implementation
        /// </summary>
        private static DataRepositoryService GetADDSDataRepository(MFAConfig cfg, string domain, string account, string password)
        {
            try
            { 
                return new ADDSDataRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow, domain, account, password);
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("GetADDSDataRepository error : {0}", ex.Message), EventLogEntryType.Error, 5000);
                throw ex;
            }
        }
        #endregion

        #region Keys management
        /// <summary>
        /// GetUserKey method implementation
        /// </summary>
        internal static string GetUserKey(MFAConfig config, string upn)
        {
            try
            {
                return KeysManager.ReadKey(upn);
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("GetUserKey error : {0} {1}", upn, ex.Message), EventLogEntryType.Error, 5000);
                throw ex;
            }           
        }

        /// <summary>
        /// GetEncodedUserKey method implementation
        /// </summary>
        internal static string GetEncodedUserKey(MFAConfig config, string upn)
        {
            try
            { 
                return KeysManager.EncodedKey(upn);
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("GetEncodedUserKey error : {0} {1}", upn, ex.Message), EventLogEntryType.Error, 5000);
                throw ex;
            }
        }

        /// <summary>
        /// NewUserKey method implementation
        /// </summary>
        internal static string NewUserKey(MFAConfig config, string upn)
        {
            try
            { 
                return KeysManager.NewKey(upn);
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("NewUserKey error : {0} {1}", upn, ex.Message), EventLogEntryType.Error, 5000);
                throw ex;
            }
        }

        /// <summary>
        /// RemoveUserKey method implmentation
        /// </summary>
        internal static bool RemoveUserKey(MFAConfig config, string upn)
        {
            try
            {
                return KeysManager.RemoveKey(upn);
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("RemoveUserKey error : {0} {1}", upn, ex.Message), EventLogEntryType.Error, 5000);
                throw ex;
            }
        }
        #endregion

        #region Password Management
        /// <summary>
        /// ChangePassword method implmentation
        /// </summary>
        internal static void ChangePassword(MFAConfig cfg, string username, string oldpassword, string newpassword)
        {
            try
            { 
                string samusername = ADDSUtils.GetSAMAccountForUser(cfg.Hosts.ActiveDirectoryHost, username);
                if (string.IsNullOrEmpty(samusername))
                    return;
                string dns = samusername.Substring(0, samusername.IndexOf('\\'));

                if ((!string.IsNullOrEmpty(cfg.Hosts.ActiveDirectoryHost.Account)) && (!string.IsNullOrEmpty(cfg.Hosts.ActiveDirectoryHost.Password)))
                {
                    using (var ctx = new PrincipalContext(ContextType.Domain, dns, cfg.Hosts.ActiveDirectoryHost.Account, cfg.Hosts.ActiveDirectoryHost.Password))
                    {
                        using (var user = UserPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, samusername))
                        {
                            if (user == null)
                                return;
                            user.ChangePassword(oldpassword, newpassword);
                        }
                    }
                }
                else
                {
                    using (var ctx = new PrincipalContext(ContextType.Domain, dns))
                    {
                        using (var user = UserPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, samusername))
                        {
                            if (user == null)
                                return;
                            user.ChangePassword(oldpassword, newpassword);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("ChangePassword error : {0} {1}", username, ex.Message), EventLogEntryType.Error, 5000);
                throw ex;
            }
        }

        /// <summary>
        /// MustChangePasswordSoon method implmentation
        /// </summary>
        internal static bool MustChangePasswordSoon(MFAConfig cfg, AuthenticationContext usercontext, out DateTime max)
        {
            max = DateTime.Now;
            try
            {                 
                if (((UserPasswordFeatures)usercontext.PasswordFeatures).Equals(UserPasswordFeatures.PasswordNone))
                    return false;
                if (((UserPasswordFeatures)usercontext.PasswordFeatures).HasFlag(UserPasswordFeatures.PasswordNotRequired))
                    return false;
                if (((UserPasswordFeatures)usercontext.PasswordFeatures).HasFlag(UserPasswordFeatures.PasswordNeverExpires))
                    return false;
                if (!((UserPasswordFeatures)usercontext.PasswordFeatures).HasFlag(UserPasswordFeatures.PasswordHasValue))
                    return false;
                if (!((UserPasswordFeatures)usercontext.PasswordFeatures).HasFlag(UserPasswordFeatures.PasswordCanBeChanged))
                    return false;

                double warnmaxage = cfg.KeysConfig.WarnPasswordExpirationBeforeInDays;

               /* if (usercontext.PasswordMaxAge < DateTime.Now)  // Locked User
                {
                    max = usercontext.PasswordMaxAge;
                    return true;
                } */
                if (usercontext.PasswordMaxAge >= DateTime.Now)  // Warn Zone
                {
                    if (usercontext.PasswordMaxAge.AddDays(-warnmaxage) <= DateTime.Now)
                    {
                        max = usercontext.PasswordMaxAge;
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("MustChangePasswordSoon error : {0} {1}", usercontext.UPN, ex.Message), EventLogEntryType.Error, 5000);
                return false;
            }
        }

        /// <summary>
        /// IsUserPasswordExpired method implmentation
        /// </summary>
        internal static bool IsUserPasswordExpired(MFAConfig cfg, AuthenticationContext usercontext)
        {
            try
            { 
                PasswordPolicyResults result = PasswordPolicyManager.GetPasswordPolicyForUser(cfg, usercontext);
                if (result == null)
                    return false;
                usercontext.PasswordFeatures = (byte)result.Features;
                usercontext.PasswordMaxAge = result.MaxAge;
                usercontext.PasswordMinAge = result.MinAge;
                if (((UserPasswordFeatures)usercontext.PasswordFeatures).Equals(UserPasswordFeatures.PasswordNone))
                    return false;
                if (((UserPasswordFeatures)usercontext.PasswordFeatures).HasFlag(UserPasswordFeatures.PasswordNotRequired))
                    return false;
                if (((UserPasswordFeatures)usercontext.PasswordFeatures).HasFlag(UserPasswordFeatures.PasswordNeverExpires))
                    return false;
                if (!((UserPasswordFeatures)usercontext.PasswordFeatures).HasFlag(UserPasswordFeatures.PasswordHasValue))
                    return false;
                if (usercontext.PasswordMaxAge < DateTime.Now)  // Locked User
                    return cfg.KeysConfig.LockUserOnPasswordExpiration;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("IsUserPasswordExpired error : {0} {1}", usercontext.UPN, ex.Message), EventLogEntryType.Error, 5000);
                return false;
            }
        }

        /// <summary>
        /// CanChangePassword method implmentation
        /// </summary>
        internal static bool CanChangePassword(MFAConfig cfg, AuthenticationContext usercontext)
        {
            try
            {
                if (((UserPasswordFeatures)usercontext.PasswordFeatures).Equals(UserPasswordFeatures.PasswordNone))
                    return false;
                if (((UserPasswordFeatures)usercontext.PasswordFeatures).HasFlag(UserPasswordFeatures.PasswordCanBeChanged))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("CanChangePassword error : {0} {1}", usercontext.UPN, ex.Message), EventLogEntryType.Error, 5000);
                return false;
            }
        }
        #endregion
    }
    #endregion

    #region Passwords Policies
    /// <summary>
    /// PasswordPolicyResults implementation
    /// </summary>
    internal class PasswordPolicyResults
    {
        private DateTime? _lastpasswordchanged = DateTime.MinValue;

        public PasswordPolicyResults(UserPasswordFeatures feat = UserPasswordFeatures.PasswordNone)
        {
            Features = feat;
            MaxDays = 0;
            MinDays = 0;
        }

        public UserPasswordFeatures Features { get; set; }
        public long MaxDays { get; set; }
        public long MinDays { get; set; }
        public override string ToString() => $"({Features}, {MinDays}, {MaxDays})";

        /// <summary>
        /// MaxAge method / compute max age
        /// </summary>
        public DateTime MaxAge
        {
            get
            {
                if (_lastpasswordchanged==null)
                    return DateTime.MaxValue;
                if (!_lastpasswordchanged.HasValue)
                    return DateTime.MaxValue;
                if (Features.HasFlag(UserPasswordFeatures.UseMFARules))
                {
                    if (MaxDays == 0)
                        return DateTime.MaxValue;
                    else
                        return _lastpasswordchanged.Value.AddDays(MaxDays);
                }
                else if (Features.HasFlag(UserPasswordFeatures.UseGPORules))
                {
                    if (MaxDays == 0)
                        return DateTime.MaxValue;
                    else
                        return _lastpasswordchanged.Value.AddDays(MaxDays);
                }
                else
                    return DateTime.MaxValue;
            }
        }

        /// <summary>
        /// MinAge method / compute min age
        /// </summary>
        public DateTime MinAge
        {
            get
            {
                if (_lastpasswordchanged == null)
                    return DateTime.MinValue;
                if (!_lastpasswordchanged.HasValue)
                    return DateTime.MinValue;
                if (Features.HasFlag(UserPasswordFeatures.UseGPORules))
                {
                    if (MinDays == 0)
                        return DateTime.MinValue;
                    else
                        return _lastpasswordchanged.Value.AddDays(MinDays);
                }
                else
                    return DateTime.MinValue;
            }
        }

        /// <summary>
        /// SetUserLastPasswordChange method implmentation
        /// </summary>
        public void SetUserLastPasswordChange(DateTime? last)
        {
            if (last.HasValue)
                _lastpasswordchanged = last;
            else
                _lastpasswordchanged = DateTime.MinValue;
        }
    }

    /// <summary>
    /// PasswordPolicyManager class implementation
    /// </summary>
    internal static class PasswordPolicyManager
    {
        internal static PasswordPolicyResults GetPasswordPolicyForUser(MFAConfig cfg, AuthenticationContext context)
        {
            PasswordPolicyResults result = null;
            if (!cfg.KeysConfig.UsePasswordPolicy)
                return null;
            if (cfg.KeysConfig.UsePSOPasswordPolicy)
                result = new PasswordPolicyResults(UserPasswordFeatures.UseGPORules);
            else
                result = new PasswordPolicyResults(UserPasswordFeatures.UseMFARules);
            try
            {
                if (!cfg.KeysConfig.UsePasswordPolicy)
                    return result;
                string samusername = ADDSUtils.GetSAMAccountForUser(cfg.Hosts.ActiveDirectoryHost, context.UPN);
                if (string.IsNullOrEmpty(samusername))
                    return result;
                string dns = samusername.Substring(0, samusername.IndexOf('\\'));

                if ((!string.IsNullOrEmpty(cfg.Hosts.ActiveDirectoryHost.Account)) && (!string.IsNullOrEmpty(cfg.Hosts.ActiveDirectoryHost.Password)))
                {
                    using (var ctx = new PrincipalContext(ContextType.Domain, dns, cfg.Hosts.ActiveDirectoryHost.Account, cfg.Hosts.ActiveDirectoryHost.Password))
                    {
                        using (var user = UserPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, samusername))
                        {
                            if (user == null)
                                return result;
                            object obj = user.GetUnderlyingObject();
                            if (user.PasswordNotRequired)
                                result.Features |= UserPasswordFeatures.PasswordNotRequired;
                            if (user.PasswordNeverExpires)
                                result.Features |= UserPasswordFeatures.PasswordNeverExpires;
                            if (user.LastPasswordSet.HasValue)
                                result.Features |= UserPasswordFeatures.PasswordHasValue;
                            if (!user.UserCannotChangePassword)
                            {
                                if (!cfg.KeysConfig.UsePSOPasswordPolicy)
                                {
                                    result.MaxDays = cfg.KeysConfig.MaxPasswordAgeInDays;
                                    result.MinDays = 0;
                                    result.SetUserLastPasswordChange(user.LastPasswordSet);
                                }
                                else
                                {
                                    PasswordPolicyResults tmpresult = GetMaxPasswordAge(cfg, dns, user);
                                    result.MaxDays = tmpresult.MaxDays;
                                    result.MinDays = tmpresult.MinDays;
                                    result.SetUserLastPasswordChange(user.LastPasswordSet);
                                }
                                if ((result.MinAge < DateTime.Now) && (result.MaxAge >= DateTime.Now))
                                    result.Features |= UserPasswordFeatures.PasswordCanBeChanged;
                            }
                        }
                    }
                }
                else
                {
                    using (var ctx = new PrincipalContext(ContextType.Domain, dns))
                    {
                        using (var user = UserPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, samusername))
                        {
                            if (user == null)
                                return result;
                            object obj = user.GetUnderlyingObject();
                            if (user.PasswordNotRequired)
                                result.Features |= UserPasswordFeatures.PasswordNotRequired;
                            if (user.PasswordNeverExpires)
                                result.Features |= UserPasswordFeatures.PasswordNeverExpires;
                            if (user.LastPasswordSet.HasValue)
                                result.Features |= UserPasswordFeatures.PasswordHasValue;
                            if (!user.UserCannotChangePassword)
                            {
                                result.Features |= UserPasswordFeatures.PasswordCanBeChanged;

                                if (!cfg.KeysConfig.UsePSOPasswordPolicy)
                                {
                                    result.MaxDays = cfg.KeysConfig.MaxPasswordAgeInDays;
                                    result.MinDays = 0;
                                    result.SetUserLastPasswordChange(user.LastPasswordSet);
                                }
                                else
                                {
                                    PasswordPolicyResults tmpresult = GetMaxPasswordAge(cfg, dns, user);
                                    result.MaxDays = tmpresult.MaxDays;
                                    result.MinDays = tmpresult.MinDays;
                                    result.SetUserLastPasswordChange(user.LastPasswordSet);
                                }
                                if ((result.MinAge < DateTime.Now) && (result.MaxAge >= DateTime.Now))
                                    result.Features |= UserPasswordFeatures.PasswordCanBeChanged;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("GetPasswordPolicyForUser error : {0}", ex.Message), EventLogEntryType.Error, 5000);
                return result;
            }
            return result;
        }

        /// <summary>
        /// GetMaxPasswordAge method implmentation
        /// </summary>
        private static PasswordPolicyResults GetMaxPasswordAge(MFAConfig cfg, string dns, UserPrincipal user)
        {
            try
            {
                PasswordPolicyResults result = GetPSOMaxPassordAge(cfg, dns, user);
                if (result == null)
                    result = GetDDPMaxPasswordAge(cfg, dns, user);
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// GetDDPMaxPasswordAge method implmentation
        /// </summary>
        private static PasswordPolicyResults GetDDPMaxPasswordAge(MFAConfig cfg, string dns, UserPrincipal user)
        {
            PasswordPolicyResults result = new PasswordPolicyResults();
            try
            {
                using (Domain dom = Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain, dns, cfg.Hosts.ActiveDirectoryHost.Account, cfg.Hosts.ActiveDirectoryHost.Password)))
                {
                    DirectorySearcher searcher = new DirectorySearcher(dom.GetDirectoryEntry());
                    SearchResultCollection results;
                    searcher.PropertiesToLoad.Add("maxPwdAge");
                    searcher.PropertiesToLoad.Add("minPwdAge");
                    results = searcher.FindAll();

                    if (results.Count >= 1)
                    {
                        if (results[0].Properties.Contains("maxPwdAge"))
                        {
                            long longage = (long)results[0].Properties["maxPwdAge"][0];
                            TimeSpan age = TimeSpan.FromTicks(Math.Abs(longage));
                            result.MaxDays = age.Days;
                        }
                        if (results[0].Properties.Contains("minPwdAge"))
                        {
                            long longage = (long)results[0].Properties["minPwdAge"][0];
                            TimeSpan age = TimeSpan.FromTicks(Math.Abs(longage));
                            result.MinDays = age.Days;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("GetDDPMaxPassordAge error : {0}", ex.Message), EventLogEntryType.Error, 5000);
                return null;
            }
            return result;
        }

        /// <summary>
        /// GetPSOMaxPassordAge method implementation
        /// </summary>
        private static PasswordPolicyResults GetPSOMaxPassordAge(MFAConfig cfg, string dns, UserPrincipal user)
        {
            PasswordPolicyResults result = new PasswordPolicyResults();
            try
            {
                using (DirectoryEntry entry = user.GetUnderlyingObject() as DirectoryEntry)
                {
                    DirectorySearcher mySearcher = new DirectorySearcher(entry);
                    SearchResultCollection results;
                    mySearcher.PropertiesToLoad.Add("msDS-ResultantPSO");
                    results = mySearcher.FindAll();

                    if (results.Count >= 1)
                    {
                        if (results[0].Properties.Contains("msDS-ResultantPSO"))
                        {
                            string pso = results[0].Properties["msDS-ResultantPSO"][0].ToString();
                            using (DirectoryEntry dir = ADDSUtils.GetDirectoryEntry(dns, cfg.Hosts.ActiveDirectoryHost.Account, cfg.Hosts.ActiveDirectoryHost.Password, pso))
                            {
                                var searchForPassPolicy = new DirectorySearcher(dir)
                                {
                                    Filter = @"(objectClass=msDS-PasswordSettings)",
                                    SearchScope = System.DirectoryServices.SearchScope.Subtree
                                };

                                searchForPassPolicy.PropertiesToLoad.AddRange(new string[] { "msDS-MaximumPasswordAge" });
                                searchForPassPolicy.PropertiesToLoad.AddRange(new string[] { "msDS-MinimumPasswordAge" });
                                var policies = searchForPassPolicy.FindAll();

                                long maxage =  (long)policies[0].Properties["msDS-MaximumPasswordAge"][0];
                                TimeSpan xage = TimeSpan.FromTicks(Math.Abs(maxage));
                                result.MaxDays = xage.Days;

                                long minage = (long)policies[0].Properties["msDS-MinimumPasswordAge"][0];
                                TimeSpan mage = TimeSpan.FromTicks(Math.Abs(minage));
                                result.MinDays = mage.Days;

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("GetPSOMaxPassordAge error : {0}", ex.Message), EventLogEntryType.Error, 5000);
                return null;
            }
            return result;
        }
    }
    #endregion

    #region TOTP Utils
    /// <summary>
    /// TOTP static class
    /// </summary>
    public partial class TOTP
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
        public TOTP(HashMode mode = HashMode.SHA1, int duration = 30, int digits = 6)
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
        public TOTP(byte[] asecret, string aid, HashMode mode = HashMode.SHA1, int duration = 30, int digits = 6)
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
        public TOTP(string ssecret, string aid, HashMode mode = HashMode.SHA1, int duration = 30, int digits = 6)
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
        /// Constructor **
        /// </summary>
        public TOTP(byte[] ssecret, string aid, DateTime datetime, HashMode mode = HashMode.SHA1, int duration = 30, int digits = 6)
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
        public TOTP(string ssecret, string aid, DateTime datetime, HashMode mode = HashMode.SHA1, int duration = 30, int digits = 6)
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
        private int SecondsToGo
        {
            get { return _secondsToGo; }
            set
            {
                _secondsToGo = value;
                if (SecondsToGo == _duration)
                    Compute(RequestedDatetime);
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
        private Int64 Timestamp
        {
            get { return _timestamp; }
            set { _timestamp = value; }
        }

        /// <summary>
        /// RequestedDatetime property implmentation
        /// </summary>
        private DateTime RequestedDatetime
        {
            get { return _datetime; }
            set { _datetime = value; }
        }

        /// <summary>
        /// Hmac property implementation
        /// </summary>
        private byte[] Hmac
        {
            get { return _hmac; }
            set { _hmac = value; }
        }

        /// <summary>
        /// Duration property implementation
        /// </summary>
        private int Duration
        {
            get { return _duration; }
        }

        /// <summary>
        /// DigitsCount property implementation
        /// </summary>
        private int DigitsCount
        {
            get { return _digits; }
        }

        /// <summary>
        /// HmacPart1 property implementation
        /// </summary>
        private byte[] HmacPart1
        {
            get { return _hmac.Take(Offset).ToArray(); }
        }

        /// <summary>
        /// HmacPart2 property implmentation 
        /// </summary>
        private byte[] HmacPart2
        {
            get { return _hmac.Skip(Offset).Take(4).ToArray(); }
        }

        /// <summary>
        /// HmacPart3 property implmentation
        /// </summary>
        private byte[] HmacPart3
        {
            get { return _hmac.Skip(Offset + 4).ToArray(); }
        }

        /// <summary>
        /// Offset property implmentation
        /// </summary>
        private int Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }

        /// <summary>
        /// OTP property implementation
        /// </summary>
        public int OTP
        {
            get { return _oneTimePassword; }
            private set { _oneTimePassword = value; }
        }

        /// <summary>
        /// ToString property implementation
        /// </summary>
        public override string ToString()
        {
            return _oneTimePassword.ToString("D" + _digits.ToString());
        }

        /// <summary>
        /// Compute method implmentation
        /// </summary>
        public void Compute(DateTime date)
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
                case 4:
                    OTP = (((Hmac[Offset + 0] & 0x7f) << 24) | ((Hmac[Offset + 1] & 0xff) << 16) | ((Hmac[Offset + 2] & 0xff) << 8) | (Hmac[Offset + 3] & 0xff)) % 10000;
                    break;
                case 5:
                    OTP = (((Hmac[Offset + 0] & 0x7f) << 24) | ((Hmac[Offset + 1] & 0xff) << 16) | ((Hmac[Offset + 2] & 0xff) << 8) | (Hmac[Offset + 3] & 0xff)) % 100000;
                    break;
                case 6:
                    OTP = (((Hmac[Offset + 0] & 0x7f) << 24) | ((Hmac[Offset + 1] & 0xff) << 16) | ((Hmac[Offset + 2] & 0xff) << 8) | (Hmac[Offset + 3] & 0xff)) % 1000000;
                    break;
                case 7:
                    OTP = (((Hmac[Offset + 0] & 0x7f) << 24) | ((Hmac[Offset + 1] & 0xff) << 16) | ((Hmac[Offset + 2] & 0xff) << 8) | (Hmac[Offset + 3] & 0xff)) % 10000000;
                    break;
                case 8:
                    OTP = (((Hmac[Offset + 0] & 0x7f) << 24) | ((Hmac[Offset + 1] & 0xff) << 16) | ((Hmac[Offset + 2] & 0xff) << 8) | (Hmac[Offset + 3] & 0xff)) % 100000000;
                    break;
                default:
                    OTP = (((Hmac[Offset + 0] & 0x7f) << 24) | ((Hmac[Offset + 1] & 0xff) << 16) | ((Hmac[Offset + 2] & 0xff) << 8) | (Hmac[Offset + 3] & 0xff)) % 1000000;
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
        /// <summary>
        /// Initialize method implementation
        /// </summary>
        internal static void Initialize(MFAConfig cfg)
        {
            Trace.TraceInformation("KeysManager.Initialize()");
            KeysRepositoryService KeysStorage = null;
            BaseKeysManagerParams KeysManagerParams = null;
            switch (cfg.KeysConfig.KeyFormat)
            {
                case SecretKeyFormat.RNG:
                    Manager = new RNGKeyManagerActivator().CreateInstance(cfg.KeysConfig.KeyVersion);
                    KeysManagerParams = new RNGKeysManagerParams(cfg.KeysConfig.XORSecret);
                    KeysManagerParams.PatchFromSecurityConfig(cfg.KeysConfig);
                    KeysStorage = InitializeStorage(cfg, false);
                    Manager.Initialize(KeysStorage, KeysManagerParams);
                    IsLoaded = true;
                    break;
                case SecretKeyFormat.RSA:
                    if (!cfg.KeysConfig.CertificatePerUser)
                    {
                        Manager = new RSAKeyManagerActivator().CreateInstance(cfg.KeysConfig.KeyVersion);
                        KeysManagerParams = new RSAKeysManagerParams(cfg.KeysConfig.XORSecret);
                        KeysManagerParams.PatchFromSecurityConfig(cfg.KeysConfig);
                        KeysStorage = InitializeStorage(cfg, false);
                    }
                    else
                    {
                        Manager = new RSA2KeyManagerActivator().CreateInstance(cfg.KeysConfig.KeyVersion);
                        KeysManagerParams = new RSA2KeysManagerParams(cfg.KeysConfig.XORSecret);
                        KeysManagerParams.PatchFromSecurityConfig(cfg.KeysConfig);
                        KeysStorage = InitializeStorage(cfg, true);
                    }
                    Manager.Initialize(KeysStorage, KeysManagerParams);
                    IsLoaded = true;
                    break;
                case SecretKeyFormat.AES:
                    KeysManagerParams = new AESKeysManagerParams(cfg.KeysConfig.XORSecret);
                    KeysManagerParams.PatchFromSecurityConfig(cfg.KeysConfig);
                    KeysStorage = InitializeStorage(cfg, false);
                    if (((AESKeysManagerParams)KeysManagerParams).AESKeyGenerator==AESKeyGeneratorMode.ECDH_P256)
                        Manager = new ECDHP256KeyManagerActivator().CreateInstance(cfg.KeysConfig.KeyVersion);
                    else
                        Manager = new AESKeyManagerActivator().CreateInstance(cfg.KeysConfig.KeyVersion);
                    Manager.Initialize(KeysStorage, KeysManagerParams);
                    IsLoaded = true;
                    break;
                case SecretKeyFormat.CUSTOM:
                    if (!string.IsNullOrEmpty(cfg.KeysConfig.CustomFullyQualifiedImplementation))
                    {
                        ISecretKeyManagerActivator _creator = Utilities.LoadExternalKeyManagerActivator(cfg.KeysConfig.CustomFullyQualifiedImplementation);
                        if (_creator != null)
                            Manager = _creator.CreateInstance(cfg.KeysConfig.KeyVersion);
                        else
                            Manager = Utilities.LoadExternalKeyManager(cfg.KeysConfig.CustomFullyQualifiedImplementation);
                        if (Manager == null)
                            IsLoaded = false;
                        else
                        {
                            KeysManagerParams = new CustomKeysManagerParams(cfg.KeysConfig.XORSecret);
                            KeysManagerParams.PatchFromSecurityConfig(cfg.KeysConfig);
                            KeysStorage = InitializeStorage(cfg, false);
                            Manager.Initialize(KeysStorage, KeysManagerParams);
                            IsLoaded = true;
                        }
                    }
                    else
                        IsLoaded = false;
                    break;
                default:
                    throw new NotImplementedException("SecretKeyManager not found !");
            }
        }

        /// <summary>
        /// InitializeStorage storage implementation
        /// </summary>
        private static KeysRepositoryService InitializeStorage(MFAConfig cfg, bool usercert)
        {
            switch (cfg.StoreMode)
            {
                case DataRepositoryKind.ADDS:
                    if (usercert)
                        return new ADDSKeys2RepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                    else
                        return new ADDSKeysRepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                case DataRepositoryKind.SQL:
                    if (usercert)
                        return new SQLKeys2RepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                    else
                        return new SQLKeysRepositoryService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                case DataRepositoryKind.Custom:
                    return CustomKeysRepositoryActivator.CreateInstance(cfg.Hosts.CustomStoreHost, cfg.DeliveryWindow);
                default:
                    return new ADDSKeys2RepositoryService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
            }
        }

        /// <summary>
        /// Manager property implementation
        /// </summary>
        public static ISecretKeyManager Manager { get; private set; }

        /// <summary>
        /// IsLoaded property implementation
        /// </summary>
        public static bool IsLoaded { get; set; } = true;

        /// <summary>
        /// EnsureKey method implementation
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
            return Manager.NewKey(upn);
        }

        /// <summary>
        /// ReadKey method implementation
        /// </summary>
        internal static string ReadKey(string upn)
        {
            return Manager.ReadKey(upn);
        }

        /// <summary>
        /// EncodedKey method implementation
        /// </summary>
        public static string EncodedKey(string upn)
        {
            return Manager.EncodedKey(upn);
        }

        /// <summary>
        /// ProbeKey method implementation
        /// </summary>
        public static byte[] ProbeKey(string upn)
        {
            return Manager.ProbeKey(upn);
        }

        /// <summary>
        /// RemoveKey method implementation
        /// </summary>
        internal static bool RemoveKey(string upn)
        {
            return Manager.RemoveKey(upn);
        }

        /// <summary>
        /// CheckKey method implmentation
        /// </summary>
        public static bool ValidateKey(string upn)
        {
            return Manager.ValidateKey(upn);
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
            if (string.IsNullOrEmpty(to))
                return;
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
                    lock (lck)
                    {
                        ResourcesLocale Resources = new ResourcesLocale(culture.LCID);
                        htmlres = Resources.GetString(ResourcesLocaleKind.CommonMail, "MailOTPContent");
                    }
                }
                string html = StripEmailContent(htmlres);
                string name = string.Empty;
                if (upn.IndexOf('@') >= 0)
                    name = upn.Remove(2, upn.IndexOf('@') - 2).Insert(2, "*********");
                else
                    name = upn;
                MailMessage Message = new MailMessage(mail.From, to)
                {
                    BodyEncoding = UTF8Encoding.UTF8,
                    IsBodyHtml = true,
                    Body = string.Format(html, mail.Company, name, code, to)
                };

                if (mail.DeliveryNotifications)
                    Message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure | DeliveryNotificationOptions.Delay;
                else
                    Message.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;

                lock (lck)
                {
                    Group titlegrp = Regex.Match(html, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"];
                    if (titlegrp != null)
                        Message.Subject = string.Format(titlegrp.Value, mail.Company, name, code, to);
                    if (Message.Subject == string.Empty)
                    {
                        ResourcesLocale Resources = new ResourcesLocale(culture.LCID);
                        Message.Subject = Resources.GetString(ResourcesLocaleKind.CommonMail, "MailOTPTitle");
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
            if (string.IsNullOrEmpty(to))
                return;
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
                        htmlres = Resources.GetString(ResourcesLocaleKind.CommonMail, "MailAdminContent");
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
                        Message.Subject = string.Format(Resources.GetString(ResourcesLocaleKind.CommonMail, "MailAdminTitle"), user.UPN);
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
        public static void SendKeyByEmail(string to, string upn, string key, MailProvider mail, MFAConfig config, CultureInfo culture)
        {
            if (config == null)
                return;
            if (config.MailProvider == null)
                return;
            if (string.IsNullOrEmpty(to))
                return;
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
                    lock (lck)
                    {
                        ResourcesLocale Resources = new ResourcesLocale(culture.LCID);
                        htmlres = Resources.GetString(ResourcesLocaleKind.CommonMail, "MailKeyContent");
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

                    MailMessage Message = new MailMessage(mail.From, to);
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
                            Message.Subject = string.Format(titlegrp.Value, mail.Company, upn, key, inlineLogo.ContentId, to);
                        if (Message.Subject == string.Empty)
                        {
                            ResourcesLocale Resources = new ResourcesLocale(culture.LCID);
                            Message.Subject = Resources.GetString(ResourcesLocaleKind.CommonMail, "MailKeyTitle");
                        }
                    }
                    Message.Priority = MailPriority.High;

                    string body = string.Format(html, mail.Company, upn, key, inlineLogo.ContentId, to);
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
                    string tmp = mail.MailNotifications.Where(c => c.LCID.Equals(ctry) || c.ParentLCID.Equals(ctry) && c.Enabled).Select(s => s.FileName).FirstOrDefault();
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
                        htmlres = Resources.GetString(ResourcesLocaleKind.CommonMail, "MailNotifications");
                    }
                }
                string html = StripEmailContent(htmlres);
                MailMessage Message = new MailMessage(mail.From, user.MailAddress)
                {
                    BodyEncoding = UTF8Encoding.UTF8,
                    IsBodyHtml = true,
                    Body = string.Format(html, user.UPN, mail.Company, user.MailAddress)
                };

                if (mail.DeliveryNotifications)
                    Message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure | DeliveryNotificationOptions.Delay;
                else
                    Message.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;

                lock (lck)
                {
                    Group titlegrp = Regex.Match(html, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"];
                    if (titlegrp != null)
                        Message.Subject = string.Format(titlegrp.Value, user.UPN, mail.Company, user.MailAddress);
                    if (Message.Subject == string.Empty)
                    {
                        ResourcesLocale Resources = new ResourcesLocale(culture.LCID);
                        Message.Subject = string.Format(Resources.GetString(ResourcesLocaleKind.CommonMail, "MailNotificationsTitle"), user.UPN);
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
        /// GetQRSize method implmentation
        /// </summary>
        private static GraphicsRenderer GetQRRenderer(MFAConfig config, BitMatrix matrix)
        {
            GraphicsRenderer renderer = new GraphicsRenderer(new FixedModuleSize(4, QuietZoneModules.Four));
            DrawingSize drwsize = renderer.SizeCalculator.GetSize(matrix.Width);
            renderer.SizeCalculator = new FixedCodeSize(drwsize.CodeWidth, drwsize.QuietZoneModules);           
            return renderer;
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
                var render = GetQRRenderer(config, matrix);
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
            var render = GetQRRenderer(config, matrix); 
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

    #region Configuration Utilities
    /// <summary>
    /// CFGUtilities class
    /// </summary>
    internal static class CFGUtilities
    {
        private static readonly char sep = Path.DirectorySeparatorChar;
        internal static string ConfigCacheFile = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "Config" + sep + "config.db";        

        #region ReadConfiguration
        /// <summary>
        /// ReadConfiguration method implementation
        /// </summary>
        internal static MFAConfig ReadConfiguration(PSHost Host = null)
        {
            MFAConfig config = null;
            try
            {
                config = ReadConfigurationFromCache();
            }
            catch
            {
                File.Delete(ConfigCacheFile);
                config = null;
            }
            if (config == null)
                config = ReadConfigurationFromDatabase(Host);

            if (config != null)
            {
                if (!File.Exists(ConfigCacheFile))
                {
                    WriteConfigurationToCache(config);
                    BroadcastNotification(config, NotificationsKind.ConfigurationCreated, Environment.MachineName, true, true);
                }
            }
            ClaimsUtilities.LoadIdentityClaim();
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
                    using (SystemEncryption MSIS = new SystemEncryption())
                    {
                        config.KeysConfig.XORSecret = MSIS.Decrypt(config.KeysConfig.XORSecret, "Pass Phrase Encryption");
                        config.Hosts.ActiveDirectoryHost.Password = MSIS.Decrypt(config.Hosts.ActiveDirectoryHost.Password, "ADDS Super Account Password");
                        config.Hosts.SQLServerHost.SQLPassword = MSIS.Decrypt(config.Hosts.SQLServerHost.SQLPassword, "SQL Super Account Password");
                        config.MailProvider.Password = MSIS.Decrypt(config.MailProvider.Password, "Mail Provider Account Password");
                        config.DefaultPin = MSIS.Decrypt(config.DefaultPin.ToString(), "Default Users Pin");
                        config.AdministrationPin = MSIS.Decrypt(config.AdministrationPin.ToString(), "Administration Pin");
                    };
                    ADDSUtils.LoadForests(config.Hosts.ActiveDirectoryHost.DomainName, config.Hosts.ActiveDirectoryHost.Account, config.Hosts.ActiveDirectoryHost.Password, config.Hosts.ActiveDirectoryHost.UseSSL, true);
                    KeysManager.Initialize(config);  // Important
                    RuntimeAuthProvider.LoadProviders(config);
                    ClientSIDsProxy.Initialize(config);
                }
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("Error when reading configuration from ADFS Database : {0}", ex.Message), EventLogEntryType.Error, 1000);
                throw ex;
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
            if (!File.Exists(CFGUtilities.ConfigCacheFile))
                return null;
            try
            {
                XmlConfigSerializer xmlserializer = new XmlConfigSerializer(typeof(MFAConfig));
                using (FileStream fs = new FileStream(CFGUtilities.ConfigCacheFile, FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[fs.Length];
                    int n = fs.Read(bytes, 0, (int)fs.Length);
                    fs.Close();

                    byte[] byt = null;
                    using (AESSystemEncryption aes = new AESSystemEncryption())
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
                            using (SystemEncryption MSIS = new SystemEncryption())
                            {
                                config.KeysConfig.XORSecret = MSIS.Decrypt(config.KeysConfig.XORSecret, "Pass Phrase Encryption");
                                config.Hosts.ActiveDirectoryHost.Password = MSIS.Decrypt(config.Hosts.ActiveDirectoryHost.Password, "ADDS Super Account Password");
                                config.Hosts.SQLServerHost.SQLPassword = MSIS.Decrypt(config.Hosts.SQLServerHost.SQLPassword, "SQL Super Account Password");
                                config.MailProvider.Password = MSIS.Decrypt(config.MailProvider.Password, "Mail Provider Account Password");
                                config.DefaultPin = MSIS.Decrypt(config.DefaultPin, "Default Users Pin");
                                config.AdministrationPin = MSIS.Decrypt(config.AdministrationPin, "Administration Pin");
                            };
                            ADDSUtils.LoadForests(config.Hosts.ActiveDirectoryHost.DomainName, config.Hosts.ActiveDirectoryHost.Account, config.Hosts.ActiveDirectoryHost.Password, config.Hosts.ActiveDirectoryHost.UseSSL, true);
                            KeysManager.Initialize(config);  // Important
                            RuntimeAuthProvider.LoadProviders(config);
                        }
                    }
                }
                return config;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("Error when reading configuration from Cache file : {0}", ex.Message), EventLogEntryType.Error, 1000);
                throw ex;
            }
        }

        /// <summary>
        /// ReadConfigurationFromADFSStore method implementation
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
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("Error when reading configuration from ADFS Database : {0}", ex.Message), EventLogEntryType.Error, 1000);
                throw ex;
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
                    using (SystemEncryption MSIS = new SystemEncryption())
                    {
                        config.KeysConfig.XORSecret = MSIS.Encrypt(config.KeysConfig.XORSecret, "Pass Phrase Encryption");
                        config.Hosts.ActiveDirectoryHost.Password = MSIS.Encrypt(config.Hosts.ActiveDirectoryHost.Password, "ADDS Super Account Password");
                        config.Hosts.SQLServerHost.SQLPassword = MSIS.Encrypt(config.Hosts.SQLServerHost.SQLPassword, "SQL Super Account Password");
                        config.MailProvider.Password = MSIS.Encrypt(config.MailProvider.Password, "Mail Provider Account Password");
                        config.DefaultPin = MSIS.Encrypt(config.DefaultPin, "Default Users Pin");
                        config.AdministrationPin = MSIS.Encrypt(config.AdministrationPin, "Administration Pin");
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
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("Error when saving configuration to ADFS Database : {0}", ex.Message), EventLogEntryType.Error, 1000);
                throw ex;
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
            try
            {
                if (encrypt)
                {
                    using (SystemEncryption MSIS = new SystemEncryption())
                    {
                        config.KeysConfig.XORSecret = MSIS.Encrypt(config.KeysConfig.XORSecret, "Pass Phrase Encryption");
                        config.Hosts.ActiveDirectoryHost.Password = MSIS.Encrypt(config.Hosts.ActiveDirectoryHost.Password, "ADDS Super Account Password");
                        config.Hosts.SQLServerHost.SQLPassword = MSIS.Encrypt(config.Hosts.SQLServerHost.SQLPassword, "SQL Super Account Password");
                        config.MailProvider.Password = MSIS.Encrypt(config.MailProvider.Password, "Mail Provider Account Password");
                        config.DefaultPin = MSIS.Encrypt(config.DefaultPin, "Default Users Pin");
                        config.AdministrationPin = MSIS.Encrypt(config.AdministrationPin, "Administration Pin");
                    };
                }
                XmlConfigSerializer xmlserializer = new XmlConfigSerializer(typeof(MFAConfig));
                MemoryStream stm = new MemoryStream();
                using (StreamReader reader = new StreamReader(stm))
                {
                    xmlserializer.Serialize(stm, config);
                    stm.Position = 0;
                    byte[] byt = null;
                    using (AESSystemEncryption aes = new AESSystemEncryption())
                    {
                        byt = aes.Encrypt(stm.ToArray());
                    }
                    using (FileStream fs = new FileStream(CFGUtilities.ConfigCacheFile, FileMode.Create, FileAccess.ReadWrite))
                    {
                        fs.Write(byt, 0, byt.Length);
                        fs.Close();
                    }
                    return config;
                }
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("Error when saving configuration to the Cache File : {0}", ex.Message), EventLogEntryType.Error, 1000);
                throw ex;
            }
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
        #endregion

        #region Notifications
        /// <summary>
        /// BroadcastNotification method implmentation
        /// </summary>
        internal static void BroadcastNotification(MFAConfig cfg, NotificationsKind kind, string message, bool local = true, bool dispatch = true)
        {
            if (string.IsNullOrEmpty(message))
                message = Environment.MachineName;
            message = GetMachineName(message);
            WebAdminManagerClient.BroadcastNotification(cfg, kind, message, local, dispatch);
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
        /// GetAssemblyPublicKey method implmentation
        /// </summary>
        public static string GetAssemblyPublicKey()
        {
            string assemblyname = Assembly.GetExecutingAssembly().FullName;
            string[] str = assemblyname.Split(',');
            return str[str.Length - 1];
        }

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
        internal static bool CheckForReplay(MFAConfig config, AuthenticationContext usercontext, int totp)
        {
            bool OK = false;
            try
            {
                ReplayRecord message = new ReplayRecord()
                {
                    MustDispatch = (config.Hosts.ADFSFarm.Servers.Count > 1),
                    ReplayLevel = config.ReplayLevel,
                    Code = totp,
                    UserIPAdress = usercontext.IPAddress,
                    UserName = usercontext.UPN,
                    UserLogon = usercontext.LogonDate,
                    DeliveryWindow = config.DeliveryWindow
                };
                string fqdn = Dns.GetHostEntry("localhost").HostName;
                List<string> srv = (from servers in config.Hosts.ADFSFarm.Servers
                                    where (servers.FQDN.ToLower() != fqdn.ToLower())
                                    select servers.FQDN.ToLower()).ToList<string>();
                OK = ReplayManagerClient.Check(srv, message);
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
        /// LoadExternalKeyManager method implmentation
        /// </summary>
        internal static ISecretKeyManager LoadExternalKeyManager(string AssemblyFulldescription)
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
        /// LoadExternalKeyManagerActivator method implmentation
        /// </summary>
        internal static ISecretKeyManagerActivator LoadExternalKeyManagerActivator(string AssemblyFulldescription)
        {
            Assembly assembly = Assembly.Load(Utilities.ParseAssembly(AssemblyFulldescription));
            Type _typetoload = assembly.GetType(Utilities.ParseType(AssemblyFulldescription));
            ISecretKeyManagerActivator wrapper = null;
            try
            {
                if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("ISecretKeyManagerActivator") != null)
                {
                    object o = Activator.CreateInstance(_typetoload, true); // Allow Calling internal Constructors
                    if (o is ISecretKeyManagerActivator)
                        wrapper = o as ISecretKeyManagerActivator;
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
            return type[0].Trim();
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
                return false;
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
                return false;
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
                    return email.Remove(0, email.IndexOf('@') + 1);
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
                    if ((prov3 != null) && (prov3.Enabled) && (!(prov3 is NeosPlugProvider)))
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
                    if (usercontext.BioNotSupported)
                        goto case PreferredMethod.Pin;
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
        /// PatchLanguageIfNeeded method implementation
        /// </summary>
        public static void PatchLanguageIfNeeded(MFAConfig cfg, AuthenticationContext ctx, string[] userlanguages)
        {
            if (string.IsNullOrEmpty(cfg.ForcedLanguage))
            {
                if (cfg.UseOfUserLanguages)
                    Utilities.PatchUserLcid(ctx, userlanguages);
                else
                    Utilities.ValidateUserLcid(ctx);
            }
            else
            {
                try
                {
                    ctx.Lcid = new CultureInfo(cfg.ForcedLanguage).LCID;
                }
                catch
                {
                    ctx.Lcid = new CultureInfo("en").LCID;
                }
            }
        }

        /// <summary>
        /// CheckForUserAgent method implementation
        /// </summary>
        internal static void CheckForUserAgent(MFAConfig config, AuthenticationContext usercontext, string useragent)
        {
            usercontext.BrowserDetected = useragent;
            if (!string.IsNullOrEmpty(usercontext.BrowserDetected))
            {
                if (CheckForbiddenBrowsers(config.WebAuthNProvider.Configuration, usercontext))
                {
                    usercontext.BioNotSupported = true;
                    usercontext.DirectLogin = false;
                    return;
                }
                if (CheckInitiatedBrowsers(config.WebAuthNProvider.Configuration, usercontext))
                {
                    usercontext.BioNotSupported = false;
                    usercontext.DirectLogin = false;
                    return;
                }
            }
            else
            {
                usercontext.BioNotSupported = false;
                usercontext.DirectLogin = false;
                return;
            }
            usercontext.BioNotSupported = false;
            usercontext.DirectLogin = config.WebAuthNProvider.DirectLogin;
        }

        /// <summary>
        /// BrowserDetection method implmentation
        /// Checking userAgent browser's value. userAgentData is not supported by many browsers
        /// </summary>
        internal static string BrowserDetection(string useragent)
        {
            if (string.IsNullOrEmpty(useragent))
            {
                return "Unknown";
            }
            if (useragent.ToLower().IndexOf("firefox") > -1)
            {
                return "Firefox";
            }
            if (useragent.ToLower().IndexOf("samsungbrowser") > -1)
            {
                return "Samsung";
            }
            if (useragent.ToLower().IndexOf("cldc") > -1)
            {
                return "Nokia";
            }
            if ((useragent.ToLower().IndexOf("opera") > -1) || (useragent.ToUpper().IndexOf("OPR") > -1))
            {
                return "Opera";
            }
            if ((useragent.ToLower().IndexOf("trident") > -1) || (useragent.ToLower().IndexOf("msie") > -1) || (useragent.ToLower().IndexOf("windows phone") > -1))
            {
                return "IE";
            }
            if (useragent.ToLower().IndexOf("edge") > -1)
            {
                return "EdgeLegacy";
            }
            if (useragent.ToLower().IndexOf("edg") > -1)
            {
                return "Edge";
            }
            if (useragent.ToLower().IndexOf("chrome") > -1)
            {
                return "Chrome";
            }
            if (useragent.ToLower().IndexOf("safari") > -1)
            {
                return "Safari";
            }
            return "Unknown";
        }

        /// <summary>
        /// CheckForbiddenBrowsers method implmentation
        /// </summary>
        private static bool CheckForbiddenBrowsers(WebAuthNProviderConfig config, AuthenticationContext usercontext)
        {
            string[] data = config.ForbiddenBrowsers.Split(';');
            foreach (string s in data)
            {
                string x = s.Trim();
                if (string.IsNullOrEmpty(x))
                    continue;
                if (x.StartsWith("#"))
                    continue;
                if (usercontext.BrowserDetected.ToLower().Equals(x.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// CheckInitiatedBrowsers method implementation
        /// </summary>
        private static bool CheckInitiatedBrowsers(WebAuthNProviderConfig config, AuthenticationContext usercontext)
        {
            string[] data = config.InitiatedBrowsers.Split(';');
            foreach (string s in data)
            {
                string x = s.Trim();
                if (string.IsNullOrEmpty(x))
                    continue;
                if (x.StartsWith("#"))
                    continue;
                if (usercontext.BrowserDetected.ToLower().Equals(x.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// IsNoCounterDevice method implmentation
        /// </summary>
        internal static bool IsNoCounterDevice(WebAuthNProviderConfig config, AuthenticationContext usercontext)
        {
            string[] data = config.NoCounterBrowsers.Split(';');
            foreach (string s in data)
            {
                string x = s.Trim();
                if (string.IsNullOrEmpty(x))
                    continue;
                if (x.StartsWith("#"))
                    continue;
                if (usercontext.BrowserDetected.ToLower().Equals(x.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
    
        /// <summary>
        /// PatchUserLcid method implementation
        /// </summary>
        private static void PatchUserLcid(AuthenticationContext ctx, string[] userlanguages)
        {
            try
            {
                foreach (string st in userlanguages)
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
                ValidateUserLcid(ctx);
            }
        }

        /// <summary>
        /// ValidateUserLcid method implementation
        /// </summary>
        private static void ValidateUserLcid(AuthenticationContext ctx)
        {
            try
            {
                ctx.Lcid = new CultureInfo(ctx.Lcid).LCID;
            }
            catch
            {
                ctx.Lcid = new CultureInfo("en").LCID;
            }
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
        public string NickName { get; set; }
    }
    #endregion
}