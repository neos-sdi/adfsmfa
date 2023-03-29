//******************************************************************************************************************************************************************************************//
// Copyright (c) 2023 redhook (adfsmfa@gmail.com)                                                                                                                                    //                        
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
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceProcess;
using System.Xml;

namespace Neos.IdentityServer.MultiFactor
{
    #region ReplayServiceHost
    /// <summary>
    /// ReplayServiceHost Class
    /// </summary>
    public class ReplayServiceHost : ServiceHost
    {
        /// <summary>
        /// ReplayServiceHost Constructor
        /// </summary>
        public ReplayServiceHost(IDependency dep, Type serviceType, params Uri[] baseAddresses) : base(serviceType, baseAddresses)
        {
            if (dep == null)
            {
                throw new ArgumentNullException("dep");
            }

            foreach (var cd in this.ImplementedContracts.Values)
            {
                cd.Behaviors.Add(new ReplayInstanceProvider(serviceType, dep));
            }
        }
    }
    #endregion

    #region WebThemesServiceHost
    /// <summary>
    /// WebThemesServiceHost Class
    /// </summary>
    public class WebThemesServiceHost : ServiceHost
    {
        /// <summary>
        /// WebThemesServiceHost Constructor
        /// </summary>
        public WebThemesServiceHost(IDependency dep, Type serviceType, params Uri[] baseAddresses) : base(serviceType, baseAddresses)
        {
            if (dep == null)
            {
                throw new ArgumentNullException("dep");
            }

            foreach (var cd in this.ImplementedContracts.Values)
            {
                cd.Behaviors.Add(new WebThemesInstanceProvider(serviceType, dep));
            }
        }
    }
    #endregion

    #region WebAdminServiceHost
    /// <summary>
    /// WebAdminServiceHost Class
    /// </summary>
    public class WebAdminServiceHost : ServiceHost
    {
        /// <summary>
        /// WebAdminServiceHost Constructor
        /// </summary>
        public WebAdminServiceHost(IDependency dep, Type serviceType, params Uri[] baseAddresses) : base(serviceType, baseAddresses)
        {
            if (dep == null)
            {
                throw new ArgumentNullException("dep");
            }

            foreach (var cd in this.ImplementedContracts.Values)
            {
                cd.Behaviors.Add(new WebAdminInstanceProvider(serviceType, dep));
            }
        }
    }
    #endregion

    #region NTServiceHost
    /// <summary>
    /// NTServiceHost Class
    /// </summary>
    public class NTServiceHost : ServiceHost
    {
        /// <summary>
        /// NTServiceHost Constructor
        /// </summary>
        public NTServiceHost(IDependency dep, Type serviceType, params Uri[] baseAddresses) : base(serviceType, baseAddresses)
        {
            if (dep == null)
            {
                throw new ArgumentNullException("dep");
            }

            foreach (var cd in this.ImplementedContracts.Values)
            {
                cd.Behaviors.Add(new NTServiceInstanceProvider(serviceType, dep));
            }
        }
    }
    #endregion

    #region ReplayInstanceProvider
    /// <summary>
    /// ReplayInstanceProvider Class
    /// </summary>
    public class ReplayInstanceProvider : IInstanceProvider, IContractBehavior
    {
        private readonly IDependency dep;
        private readonly Type type;

        /// <summary>
        /// ReplayInstanceProvider Constructor
        /// </summary>
        public ReplayInstanceProvider(Type serviceType, IDependency dep)
        {
            this.dep = dep ?? throw new ArgumentNullException("dep");
            this.type = serviceType ?? throw new ArgumentNullException("servicetype");
        }

        #region IInstanceProvider Members
        /// <summary>
        /// GetInstance method implementation
        /// </summary>
        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return this.GetInstance(instanceContext);
        }

        /// <summary>
        /// GetInstance method implementation
        /// </summary>
        public object GetInstance(InstanceContext instanceContext)
        {
            return Activator.CreateInstance(type, this.dep);
        }

        /// <summary>
        /// ReleaseInstance method implementation
        /// </summary>
        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            IDisposable disposable = instance as IDisposable;
            disposable?.Dispose();
        }
        #endregion

        #region IContractBehavior Members
        /// <summary>
        /// AddBindingParameters method
        /// </summary>
        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// ApplyClientBehavior method
        /// </summary>
        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        /// <summary>
        /// ApplyDispatchBehavior method
        /// </summary>
        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
            dispatchRuntime.InstanceProvider = this;
        }

        /// <summary>
        /// Validate method 
        /// </summary>
        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }
        #endregion
    }
    #endregion

    #region WebThemesInstanceProvider
    /// <summary>
    /// WebThemesInstanceProvider Class
    /// </summary>
    public class WebThemesInstanceProvider : IInstanceProvider, IContractBehavior
    {
        private readonly IDependency dep;
        private readonly Type type;

        /// <summary>
        /// ReplayInstanceProvider Constructor
        /// </summary>
        public WebThemesInstanceProvider(Type serviceType, IDependency dep)
        {
            this.dep = dep ?? throw new ArgumentNullException("dep");
            this.type = serviceType ?? throw new ArgumentNullException("servicetype");
        }

        #region IInstanceProvider Members
        /// <summary>
        /// GetInstance method implementation
        /// </summary>
        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return this.GetInstance(instanceContext);
        }

        /// <summary>
        /// GetInstance method implementation
        /// </summary>
        public object GetInstance(InstanceContext instanceContext)
        {
            return Activator.CreateInstance(type, this.dep);
        }

        /// <summary>
        /// ReleaseInstance method implementation
        /// </summary>
        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            var disposable = instance as IDisposable;
            disposable?.Dispose();
        }
        #endregion

        #region IContractBehavior Members
        /// <summary>
        /// AddBindingParameters method
        /// </summary>
        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// ApplyClientBehavior method
        /// </summary>
        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        /// <summary>
        /// ApplyDispatchBehavior method
        /// </summary>
        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
            dispatchRuntime.InstanceProvider = this;
        }

        /// <summary>
        /// Validate method 
        /// </summary>
        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }
        #endregion
    }
    #endregion

    #region WebAdminInstanceProvider
    /// <summary>
    /// WebThemesInstanceProvider Class
    /// </summary>
    public class WebAdminInstanceProvider : IInstanceProvider, IContractBehavior
    {
        private readonly IDependency dep;
        private readonly Type type;

        /// <summary>
        /// ReplayInstanceProvider Constructor
        /// </summary>
        public WebAdminInstanceProvider(Type serviceType, IDependency dep)
        {
            this.dep = dep ?? throw new ArgumentNullException("dep");
            this.type = serviceType ?? throw new ArgumentNullException("servicetype");
        }

        #region IInstanceProvider Members
        /// <summary>
        /// GetInstance method implementation
        /// </summary>
        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return this.GetInstance(instanceContext);
        }

        /// <summary>
        /// GetInstance method implementation
        /// </summary>
        public object GetInstance(InstanceContext instanceContext)
        {
            return Activator.CreateInstance(type, this.dep);
        }

        /// <summary>
        /// ReleaseInstance method implementation
        /// </summary>
        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            IDisposable disposable = instance as IDisposable;
            disposable?.Dispose();
        }
        #endregion

        #region IContractBehavior Members
        /// <summary>
        /// AddBindingParameters method
        /// </summary>
        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// ApplyClientBehavior method
        /// </summary>
        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        /// <summary>
        /// ApplyDispatchBehavior method
        /// </summary>
        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
            dispatchRuntime.InstanceProvider = this;
        }

        /// <summary>
        /// Validate method 
        /// </summary>
        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }
        #endregion
    }
    #endregion

    #region NTServiceInstanceProvider
    /// <summary>
    /// WebThemesInstanceProvider Class
    /// </summary>
    public class NTServiceInstanceProvider : IInstanceProvider, IContractBehavior
    {
        private readonly IDependency dep;
        private readonly Type type;

        /// <summary>
        /// ReplayInstanceProvider Constructor
        /// </summary>
        public NTServiceInstanceProvider(Type serviceType, IDependency dep)
        {
            this.dep = dep ?? throw new ArgumentNullException("dep");
            this.type = serviceType ?? throw new ArgumentNullException("servicetype");
        }

        #region IInstanceProvider Members
        /// <summary>
        /// GetInstance method implementation
        /// </summary>
        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return this.GetInstance(instanceContext);
        }

        /// <summary>
        /// GetInstance method implementation
        /// </summary>
        public object GetInstance(InstanceContext instanceContext)
        {
            return Activator.CreateInstance(type, this.dep);
        }

        /// <summary>
        /// ReleaseInstance method implementation
        /// </summary>
        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            IDisposable disposable = instance as IDisposable;
            disposable?.Dispose();
        }
        #endregion

        #region IContractBehavior Members
        /// <summary>
        /// AddBindingParameters method
        /// </summary>
        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// ApplyClientBehavior method
        /// </summary>
        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        /// <summary>
        /// ApplyDispatchBehavior method
        /// </summary>
        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
            dispatchRuntime.InstanceProvider = this;
        }

        /// <summary>
        /// Validate method 
        /// </summary>
        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }
        #endregion
    }
    #endregion

    #region Dependencies
    /// <summary>
    /// EventLogEventLogDependency class 
    /// </summary>
    public class EventLogDependency : IDependency
    {
        private readonly EventLog _eventlog;
        public EventLogDependency(EventLog eventLog)
        {
            _eventlog = eventLog;
        }

        public EventLog GetEventLog()
        {
            return _eventlog;
        }
    }
    #endregion

    #region ReplayServer
    /// <summary>
    /// ReplayServer class (Server Host)
    /// </summary>
    public class ReplayServer<T>
    {
        private readonly bool useEncryption = true;

        /// <summary>
        /// StartService method implementation
        /// </summary>
        public void StartService(ServiceBase svc)
        {
            try
            {
                Servicebase = svc;
                if (Servicehost != null)
                {
                    if (Servicehost.State == CommunicationState.Opened)
                        Servicehost.Close();
                    Servicehost = null;
                }

                string uri = string.Format("net.tcp://{0}:5987/ReplayService", Dns.GetHostEntry("localhost").HostName);
                Servicehost = new ReplayServiceHost(new EventLogDependency(svc.EventLog), typeof(T), new Uri(uri));
                ServiceMetadataBehavior smb = Servicehost.Description.Behaviors.Find<ServiceMetadataBehavior>();
                ServiceDebugBehavior dbg = Servicehost.Description.Behaviors.Find<ServiceDebugBehavior>();
                ServiceThrottlingBehavior prf = Servicehost.Description.Behaviors.Find<ServiceThrottlingBehavior>();
                if (smb == null)
                {
                    smb = new ServiceMetadataBehavior
                    {
                        HttpGetEnabled = false,
                        HttpsGetEnabled = false
                    };
                    smb.MetadataExporter.PolicyVersion = PolicyVersion.Default;
                    Servicehost.Description.Behaviors.Add(smb);
                }
                else
                {
                    smb.HttpGetEnabled = false;
                    smb.HttpsGetEnabled = false;
                    smb.MetadataExporter.PolicyVersion = PolicyVersion.Default;
                }
                if (dbg == null)
                {
                    dbg = new ServiceDebugBehavior
                    {
                        HttpHelpPageEnabled = false,
                        HttpsHelpPageEnabled = false,
                        IncludeExceptionDetailInFaults = true
                    };
                    Servicehost.Description.Behaviors.Add(dbg);
                }
                else
                {
                    dbg.HttpHelpPageEnabled = false;
                    dbg.HttpsHelpPageEnabled = false;
                    dbg.IncludeExceptionDetailInFaults = false;
                }
                if (prf == null)
                {
                    prf = new ServiceThrottlingBehavior
                    {
                        MaxConcurrentCalls = 256,
                        MaxConcurrentInstances = 256,
                        MaxConcurrentSessions = 256
                    };
                    Servicehost.Description.Behaviors.Add(prf);
                }
                else
                {
                    prf.MaxConcurrentCalls = 250;
                    prf.MaxConcurrentInstances = 250;
                    prf.MaxConcurrentSessions = 250;
                }
                NetTcpBinding tcp = new NetTcpBinding(SecurityMode.Transport);
                tcp.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
                tcp.MaxConnections = 256;

                List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>
                {
                    new MFAAuthorizationPolicy()
                };
                Servicehost.Authorization.ExternalAuthorizationPolicies = policies.AsReadOnly();
                Servicehost.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.UseWindowsGroups;

                ServiceEndpoint svcendpoint = null;
                if (useEncryption)
                {
                    CustomBinding custombinding = new CustomBinding(tcp)
                    {
                        Name = "MFAReplayBinding"
                    };
                    var currentEncoder = custombinding.Elements.Find<MessageEncodingBindingElement>();
                    if (currentEncoder != default(MessageEncodingBindingElement))
                    {
                        custombinding.Elements.Remove(currentEncoder);
                        custombinding.Elements.Insert(0, new ServicesMessageEncodingBindingElement());
                    }
                    svcendpoint = Servicehost.AddServiceEndpoint(typeof(IReplay), custombinding, uri);
                }
                else
                {
                    tcp.Name = "MFAReplayBinding";
                    svcendpoint = Servicehost.AddServiceEndpoint(typeof(IReplay), tcp, uri);
                }
                foreach (OperationDescription operation in svcendpoint.Contract.Operations)
                {
                    var beh = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
                    beh.IgnoreExtensionDataObject = true;
                    beh.MaxItemsInObjectGraph = 131070;
                }
                Servicehost.Open();
                Started = true;
            }
            catch (Exception e)
            {
                Started = false;
                throw e;
            }
        }

        /// <summary>
        /// StopService method implementation
        /// </summary>
        public void StopService()
        {
            try
            {
                if (Servicehost != null)
                {
                    if (Servicehost.State == CommunicationState.Opened)
                        Servicehost.Close();
                    Servicehost = null;
                }
                Started = false;
            }
            catch (Exception e)
            {
                Started = false;
                throw e;
            }
        }

        /// <summary>
        /// Started property implmentation
        /// </summary>
        public bool Started { get; private set; } = false;

        /// <summary>
        /// Servicehost property implementation
        /// </summary>
        public ServiceHost Servicehost { get; private set; }

        /// <summary>
        /// Servicebase property implementation
        /// </summary>
        public ServiceBase Servicebase { get; private set; }

    }
    #endregion

    #region WebThemesServer
    /// <summary>
    /// WebThemesServer class (Server Host)
    /// </summary>
    public class WebThemesServer<T>
    {
        private readonly bool useEncryption = true;

        /// <summary>
        /// StartService method implementation
        /// </summary>
        public void StartService(ServiceBase svc)
        {
            try
            {
                Servicebase = svc;
                if (Servicehost != null)
                {
                    if (Servicehost.State == CommunicationState.Opened)
                        Servicehost.Close();
                    Servicehost = null;
                }

                string uri = string.Format("net.tcp://{0}:5987/WebThemesService", Dns.GetHostEntry("localhost").HostName);
                Servicehost = new WebThemesServiceHost(new EventLogDependency(svc.EventLog), typeof(T), new Uri(uri));
                ServiceMetadataBehavior smb = Servicehost.Description.Behaviors.Find<ServiceMetadataBehavior>();
                ServiceDebugBehavior dbg = Servicehost.Description.Behaviors.Find<ServiceDebugBehavior>();
                ServiceThrottlingBehavior prf = Servicehost.Description.Behaviors.Find<ServiceThrottlingBehavior>();
                if (smb == null)
                {
                    smb = new ServiceMetadataBehavior
                    {
                        HttpGetEnabled = false,
                        HttpsGetEnabled = false
                    };
                    smb.MetadataExporter.PolicyVersion = PolicyVersion.Default;
                    Servicehost.Description.Behaviors.Add(smb);
                }
                else
                {
                    smb.HttpGetEnabled = false;
                    smb.HttpsGetEnabled = false;
                    smb.MetadataExporter.PolicyVersion = PolicyVersion.Default;
                }
                if (dbg == null)
                {
                    dbg = new ServiceDebugBehavior
                    {
                        HttpHelpPageEnabled = false,
                        HttpsHelpPageEnabled = false,
                        IncludeExceptionDetailInFaults = true
                    };
                    Servicehost.Description.Behaviors.Add(dbg);
                }
                else
                {
                    dbg.HttpHelpPageEnabled = false;
                    dbg.HttpsHelpPageEnabled = false;
                    dbg.IncludeExceptionDetailInFaults = false;
                }
                if (prf == null)
                {
                    prf = new ServiceThrottlingBehavior
                    {
                        MaxConcurrentCalls = 256,
                        MaxConcurrentInstances = 256,
                        MaxConcurrentSessions = 256
                    };
                    Servicehost.Description.Behaviors.Add(prf);
                }
                else
                {
                    prf.MaxConcurrentCalls = 250;
                    prf.MaxConcurrentInstances = 250;
                    prf.MaxConcurrentSessions = 250;
                }
                NetTcpBinding tcp = new NetTcpBinding(SecurityMode.Transport);
                tcp.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
                tcp.MaxConnections = 256;

                List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>
                {
                    new MFAAuthorizationPolicy()
                };
                Servicehost.Authorization.ExternalAuthorizationPolicies = policies.AsReadOnly();
                Servicehost.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.UseWindowsGroups;

                ServiceEndpoint svcendpoint = null;
                if (useEncryption)
                {
                    CustomBinding custombinding = new CustomBinding(tcp)
                    {
                        Name = "MFAThemesBinding"
                    };
                    var currentEncoder = custombinding.Elements.Find<MessageEncodingBindingElement>();
                    if (currentEncoder != default(MessageEncodingBindingElement))
                    {
                        custombinding.Elements.Remove(currentEncoder);
                        custombinding.Elements.Insert(0, new ServicesMessageEncodingBindingElement());
                    }
                    svcendpoint = Servicehost.AddServiceEndpoint(typeof(IWebThemeManager), custombinding, uri);
                }
                else
                {
                    tcp.Name = "MFAThemesBinding";
                    svcendpoint = Servicehost.AddServiceEndpoint(typeof(IWebThemeManager), tcp, uri);
                }
                foreach (OperationDescription operation in svcendpoint.Contract.Operations)
                {
                    var beh = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
                    beh.IgnoreExtensionDataObject = true;
                    beh.MaxItemsInObjectGraph = 131070;
                }
                Servicehost.Open();
                Started = true;
            }
            catch (Exception e)
            {
                Started = false;
                throw e;
            }
        }

        /// <summary>
        /// StopService method implementation
        /// </summary>
        public void StopService()
        {
            try
            {
                if (Servicehost != null)
                {
                    if (Servicehost.State == CommunicationState.Opened)
                        Servicehost.Close();
                    Servicehost = null;
                }
                Started = false;
            }
            catch (Exception e)
            {
                Started = false;
                throw e;
            }
        }

        /// <summary>
        /// Started property implmentation
        /// </summary>
        public bool Started { get; private set; } = false;

        /// <summary>
        /// Servicehost property implementation
        /// </summary>
        public ServiceHost Servicehost { get; private set; }

        /// <summary>
        /// Servicebase property implementation
        /// </summary>
        public ServiceBase Servicebase { get; private set; }

    }
    #endregion

    #region WebAdminServer
    /// <summary>
    /// WebAdminServer class (Server Host)
    /// </summary>
    public class WebAdminServer<T>
    {
        private readonly bool useEncryption = true;

        /// <summary>
        /// StartService method implementation
        /// </summary>
        public void StartService(ServiceBase svc)
        {
            try
            {
                Servicebase = svc;
                if (Servicehost != null)
                {
                    if (Servicehost.State == CommunicationState.Opened)
                        Servicehost.Close();
                    Servicehost = null;
                }

                string uri = string.Format("net.tcp://{0}:5987/WebAdminService", Dns.GetHostEntry("localhost").HostName);
                Servicehost = new WebAdminServiceHost(new EventLogDependency(svc.EventLog), typeof(T), new Uri(uri));
                ServiceMetadataBehavior smb = Servicehost.Description.Behaviors.Find<ServiceMetadataBehavior>();
                ServiceDebugBehavior dbg = Servicehost.Description.Behaviors.Find<ServiceDebugBehavior>();
                ServiceThrottlingBehavior prf = Servicehost.Description.Behaviors.Find<ServiceThrottlingBehavior>();
                if (smb == null)
                {
                    smb = new ServiceMetadataBehavior
                    {
                        HttpGetEnabled = false,
                        HttpsGetEnabled = false
                    };
                    smb.MetadataExporter.PolicyVersion = PolicyVersion.Default;
                    Servicehost.Description.Behaviors.Add(smb);
                }
                else
                {
                    smb.HttpGetEnabled = false;
                    smb.HttpsGetEnabled = false;
                    smb.MetadataExporter.PolicyVersion = PolicyVersion.Default;
                }
                if (dbg == null)
                {
                    dbg = new ServiceDebugBehavior
                    {
                        HttpHelpPageEnabled = false,
                        HttpsHelpPageEnabled = false,
                        IncludeExceptionDetailInFaults = true
                    };
                    Servicehost.Description.Behaviors.Add(dbg);
                }
                else
                {
                    dbg.HttpHelpPageEnabled = false;
                    dbg.HttpsHelpPageEnabled = false;
                    dbg.IncludeExceptionDetailInFaults = true;
                }
                if (prf == null)
                {
                    prf = new ServiceThrottlingBehavior
                    {
                        MaxConcurrentCalls = 256,
                        MaxConcurrentInstances = 256,
                        MaxConcurrentSessions = 256
                    };
                    Servicehost.Description.Behaviors.Add(prf);
                }
                else
                {
                    prf.MaxConcurrentCalls = 256;
                    prf.MaxConcurrentInstances = 256;
                    prf.MaxConcurrentSessions = 256;
                }

                NetTcpBinding tcp = new NetTcpBinding(SecurityMode.Transport)
                {
                    MaxConnections = 256,
                    MaxReceivedMessageSize = 2147483647
                };
                tcp.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

                List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>
                {
                    new MFAAuthorizationPolicy()
                };
                Servicehost.Authorization.ExternalAuthorizationPolicies = policies.AsReadOnly();
                Servicehost.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.UseWindowsGroups;

                ServiceEndpoint svcendpoint = null;
                if (useEncryption)
                {
                    CustomBinding custombinding = new CustomBinding(tcp)
                    {
                        Name = "MFAWebAdminBinding"
                    };
                    var currentEncoder = custombinding.Elements.Find<MessageEncodingBindingElement>();
                    if (currentEncoder != default(MessageEncodingBindingElement))
                    {
                        custombinding.Elements.Remove(currentEncoder);
                        custombinding.Elements.Insert(0, new ServicesMessageEncodingBindingElement());
                    }
                    svcendpoint = Servicehost.AddServiceEndpoint(typeof(IWebAdminServices), custombinding, uri);
                }
                else
                {
                    tcp.Name = "MFAWebAdminBinding";
                    svcendpoint = Servicehost.AddServiceEndpoint(typeof(IWebAdminServices), tcp, uri);
                }
                foreach (OperationDescription operation in svcendpoint.Contract.Operations)
                {
                    var beh = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
                    beh.IgnoreExtensionDataObject = true;
                    beh.MaxItemsInObjectGraph = 131070;
                }
                Servicehost.Open();
                Started = true;
            }
            catch (Exception e)
            {
                Started = false;
                throw e;
            }
        }

        /// <summary>
        /// StopService method implementation
        /// </summary>
        public void StopService()
        {
            try
            {
                if (Servicehost != null)
                {
                    if (Servicehost.State == CommunicationState.Opened)
                        Servicehost.Close();
                    Servicehost = null;
                }
                Started = false;
            }
            catch (Exception e)
            {
                Started = false;
                throw e;
            }
        }

        /// <summary>
        /// Started property implmentation
        /// </summary>
        public bool Started { get; private set; } = false;

        /// <summary>
        /// Servicehost property implementation
        /// </summary>
        public ServiceHost Servicehost { get; private set; }

        /// <summary>
        /// Servicebase property implementation
        /// </summary>
        public ServiceBase Servicebase { get; private set; }

    }
    #endregion

    #region NTServiceServer
    /// <summary>
    /// NTServiceServer class (Server Host)
    /// </summary>
    public class NTServiceServer<T>
    {
        private readonly bool useEncryption = true;

        /// <summary>
        /// StartService method implementation
        /// </summary>
        public void StartService(ServiceBase svc)
        {
            try
            {
                Servicebase = svc;
                if (Servicehost != null)
                {
                    if (Servicehost.State == CommunicationState.Opened)
                        Servicehost.Close();
                    Servicehost = null;
                }

                string uri = string.Format("net.tcp://{0}:5987/NTServices", Dns.GetHostEntry("localhost").HostName);
                Servicehost = new WebAdminServiceHost(new EventLogDependency(svc.EventLog), typeof(T), new Uri(uri));
                ServiceMetadataBehavior smb = Servicehost.Description.Behaviors.Find<ServiceMetadataBehavior>();
                ServiceDebugBehavior dbg = Servicehost.Description.Behaviors.Find<ServiceDebugBehavior>();
                ServiceThrottlingBehavior prf = Servicehost.Description.Behaviors.Find<ServiceThrottlingBehavior>();
                if (smb == null)
                {
                    smb = new ServiceMetadataBehavior
                    {
                        HttpGetEnabled = false,
                        HttpsGetEnabled = false
                    };
                    smb.MetadataExporter.PolicyVersion = PolicyVersion.Default;
                    Servicehost.Description.Behaviors.Add(smb);
                }
                else
                {
                    smb.HttpGetEnabled = false;
                    smb.HttpsGetEnabled = false;
                    smb.MetadataExporter.PolicyVersion = PolicyVersion.Default;
                }
                if (dbg == null)
                {
                    dbg = new ServiceDebugBehavior
                    {
                        HttpHelpPageEnabled = false,
                        HttpsHelpPageEnabled = false,
                        IncludeExceptionDetailInFaults = true
                    };
                    Servicehost.Description.Behaviors.Add(dbg);
                }
                else
                {
                    dbg.HttpHelpPageEnabled = false;
                    dbg.HttpsHelpPageEnabled = false;
                    dbg.IncludeExceptionDetailInFaults = false;
                }
                if (prf == null)
                {
                    prf = new ServiceThrottlingBehavior
                    {
                        MaxConcurrentCalls = 256,
                        MaxConcurrentInstances = 256,
                        MaxConcurrentSessions = 256
                    };
                    Servicehost.Description.Behaviors.Add(prf);
                }
                else
                {
                    prf.MaxConcurrentCalls = 250;
                    prf.MaxConcurrentInstances = 250;
                    prf.MaxConcurrentSessions = 250;
                }
                NetTcpBinding tcp = new NetTcpBinding(SecurityMode.Transport)
                {
                    MaxConnections = 256
                };
                tcp.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

                List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>
                {
                    new MFAAuthorizationPolicy()
                };
                Servicehost.Authorization.ExternalAuthorizationPolicies = policies.AsReadOnly();
                Servicehost.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.UseWindowsGroups;

                ServiceEndpoint svcendpoint = null;
                if (useEncryption)
                {
                    CustomBinding custombinding = new CustomBinding(tcp)
                    {
                        Name = "MFAServiceNTBinding"
                    };
                    var currentEncoder = custombinding.Elements.Find<MessageEncodingBindingElement>();
                    if (currentEncoder != default(MessageEncodingBindingElement))
                    {
                        custombinding.Elements.Remove(currentEncoder);
                        custombinding.Elements.Insert(0, new ServicesMessageEncodingBindingElement());
                    }
                    svcendpoint = Servicehost.AddServiceEndpoint(typeof(INTService), custombinding, uri);
                }
                else
                {
                    tcp.Name = "MFAServiceNTBinding";
                    svcendpoint = Servicehost.AddServiceEndpoint(typeof(INTService), tcp, uri);
                }
                foreach (OperationDescription operation in svcendpoint.Contract.Operations)
                {
                    var beh = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
                    beh.IgnoreExtensionDataObject = true;
                    beh.MaxItemsInObjectGraph = 131070;
                }
                Servicehost.Open();
                Started = true;
            }
            catch (Exception e)
            {
                Started = false;
                throw e;
            }
        }

        /// <summary>
        /// StopService method implementation
        /// </summary>
        public void StopService()
        {
            try
            {
                if (Servicehost != null)
                {
                    if (Servicehost.State == CommunicationState.Opened)
                        Servicehost.Close();
                    Servicehost = null;
                }
                Started = false;
            }
            catch (Exception e)
            {
                Started = false;
                throw e;
            }
        }

        /// <summary>
        /// Started property implmentation
        /// </summary>
        public bool Started { get; private set; } = false;

        /// <summary>
        /// Servicehost property implementation
        /// </summary>
        public ServiceHost Servicehost { get; private set; }

        /// <summary>
        /// Servicebase property implementation
        /// </summary>
        public ServiceBase Servicebase { get; private set; }

    }
    #endregion

    #region ReplayClient
    /// <summary>
    /// ReplayClient class (Client Proxy)
    /// </summary>
    public class ReplayClient
    {
        private ChannelFactory<IReplay> _factory = null;
        private readonly bool useEncryption = true;

        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public void Initialize(string servername = "localhost")
        {
            if (_factory != null)
            {
                if (_factory.State == CommunicationState.Opened)
                    _factory.Close();
            }
            NetTcpBinding tcp = new NetTcpBinding(SecurityMode.Transport);
            tcp.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            tcp.MaxConnections = 256;
            tcp.OpenTimeout = new TimeSpan(0, 0, 5);
            tcp.SendTimeout = new TimeSpan(0, 0, 15);

            if (useEncryption)
            {
                CustomBinding custombinding = new CustomBinding(tcp)
                {
                    Name = "MFAClientReplayBinding"
                };
                var currentEncoder = custombinding.Elements.Find<MessageEncodingBindingElement>();
                if (currentEncoder != default(MessageEncodingBindingElement))
                {
                    custombinding.Elements.Remove(currentEncoder);
                    custombinding.Elements.Insert(0, new ServicesMessageEncodingBindingElement());
                }
                _factory = new ChannelFactory<IReplay>(custombinding, new EndpointAddress(string.Format("net.tcp://{0}:5987/ReplayService", servername)));
            }
            else
            {
                tcp.Name = "MFAClientReplayBinding";
                _factory = new ChannelFactory<IReplay>(tcp, new EndpointAddress(string.Format("net.tcp://{0}:5987/ReplayService", servername)));
            }
            foreach (OperationDescription operation in _factory.Endpoint.Contract.Operations)
            {
                var beh = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
                beh.IgnoreExtensionDataObject = true;
                beh.MaxItemsInObjectGraph = 131070;
            }
            IsInitialized = true;
        }

        /// <summary>
        /// UnInitialize method implementation
        /// </summary>
        public void UnInitialize()
        {
            if (_factory != null)
            {
                if (_factory.State == CommunicationState.Opened)
                    _factory.Close();
                else
                    _factory.Abort();
            }
            IsInitialized = false;
        }

        /// <summary>
        /// Open method implementation
        /// </summary>
        public IReplay Open()
        {
            _factory.Credentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Identification;
            return _factory.CreateChannel();
        }

        /// <summary>
        /// Close method implementation
        /// </summary>
        public void Close(IReplay client)
        {
            if (client != null)
            {
                if (((IClientChannel)client).State == CommunicationState.Opened)
                    ((IClientChannel)client).Close();
                else
                    ((IClientChannel)client).Abort();
            }
        }
    }
    #endregion

    #region WebThemesClient
    /// <summary>
    /// WebThemesClient class (Client Proxy)
    /// </summary>
    public class WebThemesClient
    {
        private ChannelFactory<IWebThemeManager> _factory = null;
        private readonly bool useEncryption = true;

        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public void Initialize(string servername = "localhost")
        {
            if (_factory != null)
            {
                if (_factory.State == CommunicationState.Opened)
                    _factory.Close();
            }
            NetTcpBinding tcp = new NetTcpBinding(SecurityMode.Transport);
            tcp.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            tcp.MaxConnections = 256;
            if (useEncryption)
            {
                CustomBinding custombinding = new CustomBinding(tcp)
                {
                    Name = "MFAClientThemesBinding"
                };
                var currentEncoder = custombinding.Elements.Find<MessageEncodingBindingElement>();
                if (currentEncoder != default(MessageEncodingBindingElement))
                {
                    custombinding.Elements.Remove(currentEncoder);
                    custombinding.Elements.Insert(0, new ServicesMessageEncodingBindingElement());
                }
                _factory = new ChannelFactory<IWebThemeManager>(custombinding, new EndpointAddress(string.Format("net.tcp://{0}:5987/WebThemesService", servername)));
            }
            else
            {
                tcp.Name = "MFAClientThemesBinding";
                _factory = new ChannelFactory<IWebThemeManager>(tcp, new EndpointAddress(string.Format("net.tcp://{0}:5987/WebThemesService", servername)));
            }
            foreach (OperationDescription operation in _factory.Endpoint.Contract.Operations)
            {
                var beh = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
                beh.IgnoreExtensionDataObject = true;
                beh.MaxItemsInObjectGraph = 131070;
            }
            IsInitialized = true;
        }

        /// <summary>
        /// UnInitialize method implementation
        /// </summary>
        public void UnInitialize()
        {
            if (_factory != null)
            {
                if (_factory.State == CommunicationState.Opened)
                    _factory.Close();
                else
                    _factory.Abort();
            }
            IsInitialized = false;
        }

        /// <summary>
        /// Open method implementation
        /// </summary>
        public IWebThemeManager Open()
        {
            _factory.Credentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Identification;
            return _factory.CreateChannel();
        }

        /// <summary>
        /// Close method implementation
        /// </summary>
        public void Close(IWebThemeManager client)
        {
            if (client != null)
            {
                if (((IClientChannel)client).State == CommunicationState.Opened)
                    ((IClientChannel)client).Close();
                else
                    ((IClientChannel)client).Abort();
            }
        }
    }
    #endregion

    #region WebAdminClient
    /// <summary>
    /// WebAdminClient class (Client Proxy)
    /// </summary>
    public class WebAdminClient
    {
        private ChannelFactory<IWebAdminServices> _factory = null;
        private readonly bool useEncryption = true;

        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public void Initialize(string servername = "localhost")
        {
            if (_factory != null)
            {
                if (_factory.State == CommunicationState.Opened)
                    _factory.Close();
            }
            NetTcpBinding tcp = new NetTcpBinding(SecurityMode.Transport);
            tcp.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            tcp.MaxReceivedMessageSize = 2147483647;

            if (useEncryption)
            {
                CustomBinding custombinding = new CustomBinding(tcp)
                {
                    Name = "MFAClientWebAdminBinding"
                };
                var currentEncoder = custombinding.Elements.Find<MessageEncodingBindingElement>();
                if (currentEncoder != default(MessageEncodingBindingElement))
                {
                    custombinding.Elements.Remove(currentEncoder);
                    custombinding.Elements.Insert(0, new ServicesMessageEncodingBindingElement());
                }
                _factory = new ChannelFactory<IWebAdminServices>(custombinding, new EndpointAddress(string.Format("net.tcp://{0}:5987/WebAdminService", servername)));
            }
            else
            {
                tcp.Name = "MFAClientWebAdminBinding";
                _factory = new ChannelFactory<IWebAdminServices>(tcp, new EndpointAddress(string.Format("net.tcp://{0}:5987/WebAdminService", servername)));
            }
            foreach (OperationDescription operation in _factory.Endpoint.Contract.Operations)
            {
                var beh = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
                beh.IgnoreExtensionDataObject = true;
                beh.MaxItemsInObjectGraph = 131070;
            }

            IsInitialized = true;
        }

        /// <summary>
        /// UnInitialize method implementation
        /// </summary>
        public void UnInitialize()
        {
            if (_factory != null)
            {
                if (_factory.State == CommunicationState.Opened)
                    _factory.Close();
                else
                    _factory.Abort();
            }
            IsInitialized = false;
        }

        /// <summary>
        /// Open method implementation
        /// </summary>
        public IWebAdminServices Open()
        {
            _factory.Credentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Identification;
            return _factory.CreateChannel();
        }

        /// <summary>
        /// Close method implementation
        /// </summary>
        public void Close(IWebAdminServices client)
        {
            if (client != null)
            {
                if (((IClientChannel)client).State == CommunicationState.Opened)
                    ((IClientChannel)client).Close();
                else
                    ((IClientChannel)client).Abort();
            }
        }
    }
    #endregion

    #region NTServiceClient
    /// <summary>
    /// NTServiceClient class (Client Proxy)
    /// </summary>
    public class NTServiceClient
    {
        private ChannelFactory<INTService> _factory = null;
        private readonly bool useEncryption = true;

        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public void Initialize(string servername = "localhost")
        {
            if (_factory != null)
            {
                if (_factory.State == CommunicationState.Opened)
                    _factory.Close();
            }
            NetTcpBinding tcp = new NetTcpBinding(SecurityMode.Transport);
            tcp.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            if (useEncryption)
            {
                CustomBinding custombinding = new CustomBinding(tcp)
                {
                    Name = "MFAClientServiceNTBinding"
                };
                var currentEncoder = custombinding.Elements.Find<MessageEncodingBindingElement>();
                if (currentEncoder != default(MessageEncodingBindingElement))
                {
                    custombinding.Elements.Remove(currentEncoder);
                    custombinding.Elements.Insert(0, new ServicesMessageEncodingBindingElement());
                }
                _factory = new ChannelFactory<INTService>(custombinding, new EndpointAddress(string.Format("net.tcp://{0}:5987/NTServices", servername)));
            }
            else
            {
                tcp.Name = "MFAClientServiceNTBinding";
                _factory = new ChannelFactory<INTService>(tcp, new EndpointAddress(string.Format("net.tcp://{0}:5987/NTServices", servername)));
            }
            foreach (OperationDescription operation in _factory.Endpoint.Contract.Operations)
            {
                var beh = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
                beh.IgnoreExtensionDataObject = true;
                beh.MaxItemsInObjectGraph = 131070;
            }
            IsInitialized = true;
        }

        /// <summary>
        /// UnInitialize method implementation
        /// </summary>
        public void UnInitialize()
        {
            if (_factory != null)
            {
                if (_factory.State == CommunicationState.Opened)
                    _factory.Close();
                else
                    _factory.Abort();
            }
            IsInitialized = false;
        }

        /// <summary>
        /// Open method implementation
        /// </summary>
        public INTService Open()
        {
            _factory.Credentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Identification;
            return _factory.CreateChannel();
        }

        /// <summary>
        /// Close method implementation
        /// </summary>
        public void Close(INTService client)
        {
            if (client != null)
            {
                if (((IClientChannel)client).State == CommunicationState.Opened)
                    ((IClientChannel)client).Close();
                else
                    ((IClientChannel)client).Abort();
            }
        }
    }
    #endregion

    #region ServicesMessageEncoderFactory
    /// <summary>
    /// ServicesMessageEncoderFactory class
    /// </summary>
    internal class ServicesMessageEncoderFactory : MessageEncoderFactory
    {
        readonly MessageEncoder encoder;

        /// <summary>
        /// ServicesMessageEncoderFactory Class
        /// </summary>
        public ServicesMessageEncoderFactory(MessageEncoderFactory messageEncoderFactory)
        {
            if (messageEncoderFactory == null)
                throw new ArgumentNullException("messageEncoderFactory", "A valid message encoder factory must be passed to the Replay Encoder");
            encoder = new SercicesMessageEncoder(messageEncoderFactory.Encoder);
        }

        /// <summary>
        /// Encoder property override
        /// </summary>
        public override MessageEncoder Encoder
        {
            get { return encoder; }
        }

        /// <summary>
        /// MessageVersion property override
        /// </summary>
        public override MessageVersion MessageVersion
        {
            get { return encoder.MessageVersion; }
        }

        #region SercicesMessageEncoder
        /// <summary>
        /// SercicesMessageEncoder Class
        /// </summary>
        class SercicesMessageEncoder : MessageEncoder
        {
            static readonly string ReplayContentType = " application/soap+xml";

            /// <summary>
            /// innerEncoder variable
            /// </summary>
            readonly MessageEncoder innerEncoder;


            /// <summary>
            /// ReplayMessageEncoder Constructor
            /// </summary>
            /// <param name="messageEncoder"></param>
            internal SercicesMessageEncoder(MessageEncoder messageEncoder) : base()
            {
                innerEncoder = messageEncoder ?? throw new ArgumentNullException("messageEncoder", "A valid message encoder must be passed to the Replay Encoder");
            }

            /// <summary>
            /// ContentType property override
            /// </summary>
            public override string ContentType
            {
                get { return ReplayContentType; }
            }

            /// <summary>
            /// MediaType property override
            /// </summary>
            public override string MediaType
            {
                get { return ReplayContentType; }
            }

            /// <summary>
            /// MessageVersion property override
            /// </summary>
            public override MessageVersion MessageVersion
            {
                get { return innerEncoder.MessageVersion; }
            }

            /// <summary>
            /// EncryptBuffer method implementation (compress)
            /// </summary>
            private ArraySegment<byte> EncryptBuffer(ArraySegment<byte> buffer, BufferManager bufferManager, int messageOffset)
            {
                byte[] unencryptedBytes = new byte[buffer.Count];
                Array.Copy(buffer.Array, buffer.Offset, unencryptedBytes, 0, unencryptedBytes.Length);

                byte[] cryptedBytes = null;
                using (AESSystemEncryption aes = new AESSystemEncryption())
                {
                    cryptedBytes = aes.Encrypt(unencryptedBytes);
                }

                int totalLength = messageOffset + cryptedBytes.Length;
                byte[] bufferManagerBuffer = bufferManager.TakeBuffer(totalLength);
                Array.Clear(bufferManagerBuffer, 0, bufferManagerBuffer.Length);
                Array.Copy(cryptedBytes, 0, bufferManagerBuffer, messageOffset, cryptedBytes.Length);

                bufferManager.ReturnBuffer(buffer.Array);
                ArraySegment<byte> byteArray = new ArraySegment<byte>(bufferManagerBuffer, messageOffset, cryptedBytes.Length);

                return byteArray;
            }

            /// <summary>
            /// DecryptBuffer method implementation (decompress)
            /// </summary>
            private ArraySegment<byte> DecryptBuffer(ArraySegment<byte> buffer, BufferManager bufferManager)
            {
                byte[] cryptedBytes = new byte[buffer.Count];
                Array.Copy(buffer.Array, buffer.Offset, cryptedBytes, 0, cryptedBytes.Length);

                byte[] unencryptedBytes = null;
                using (AESSystemEncryption aes = new AESSystemEncryption())
                {
                    unencryptedBytes = aes.Decrypt(cryptedBytes);
                }

                int totalLength = unencryptedBytes.Length + buffer.Offset;
                byte[] bufferManagerBuffer = bufferManager.TakeBuffer(totalLength);
                Array.Clear(bufferManagerBuffer, 0, bufferManagerBuffer.Length);
                Array.Copy(buffer.Array, 0, bufferManagerBuffer, 0, buffer.Offset);
                Array.Copy(unencryptedBytes, 0, bufferManagerBuffer, buffer.Offset, unencryptedBytes.Length);

                ArraySegment<byte> byteArray = new ArraySegment<byte>(bufferManagerBuffer, buffer.Offset, unencryptedBytes.Length);
                bufferManager.ReturnBuffer(buffer.Array);

                return byteArray;
            }

            /// <summary>
            /// ReadMessage method override
            /// </summary>
            public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
            {
                ArraySegment<byte> DecryptedBuffer = DecryptBuffer(buffer, bufferManager);
                Message returnMessage = innerEncoder.ReadMessage(DecryptedBuffer, bufferManager);
                returnMessage.Properties.Encoder = this;
                return returnMessage;
            }

            /// <summary>
            /// WriteMessage method override
            /// </summary>
            public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
            {
                ArraySegment<byte> buffer = innerEncoder.WriteMessage(message, maxMessageSize, bufferManager, 0);
                return EncryptBuffer(buffer, bufferManager, messageOffset);
            }

            /// <summary>
            /// ReadMessage method override
            /// </summary>
            public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
            {
                byte[] unencryptedBytes = new byte[stream.Length];
                stream.Read(unencryptedBytes, 0, (int)stream.Length);

                byte[] cryptedBytes = null;
                using (AESSystemEncryption aes = new AESSystemEncryption())
                {
                    cryptedBytes = aes.Encrypt(unencryptedBytes);
                }
                MemoryStream stm = new MemoryStream(cryptedBytes);
                return innerEncoder.ReadMessage(stm, maxSizeOfHeaders);
            }

            /// <summary>
            /// WriteMessage method override
            /// </summary>
            public override void WriteMessage(Message message, Stream stream)
            {
                byte[] cryptedBytes = new byte[stream.Length];
                stream.Read(cryptedBytes, 0, (int)stream.Length);
                byte[] unencryptedBytes = null;
                using (AESSystemEncryption aes = new AESSystemEncryption())
                {
                    unencryptedBytes = aes.Decrypt(cryptedBytes);
                }
                MemoryStream stm = new MemoryStream(unencryptedBytes);
                innerEncoder.WriteMessage(message, stm);
                stream.Flush();
            }
        }
        #endregion
    }
    #endregion

    #region ServicesMessageEncodingBindingElement
    /// <summary>
    /// ServicesMessageEncodingBindingElement class
    /// </summary>
    public sealed class ServicesMessageEncodingBindingElement : MessageEncodingBindingElement, IPolicyExportExtension
    {
        /// <summary>
        /// ServicesMessageEncodingBindingElement Constructor
        /// </summary>
        public ServicesMessageEncodingBindingElement() : this(new TextMessageEncodingBindingElement()) { }

        /// <summary>
        /// ServicesMessageEncodingBindingElement Constructor
        /// </summary>
        public ServicesMessageEncodingBindingElement(MessageEncodingBindingElement messageEncoderBindingElement)
        {
            this.InnerMessageEncodingBindingElement = messageEncoderBindingElement;
        }

        /// <summary>
        /// InnerMessageEncodingBindingElement property
        /// </summary>
        public MessageEncodingBindingElement InnerMessageEncodingBindingElement { get; set; }

        /// <summary>
        /// MessageVersion property override
        /// </summary>
        public override MessageVersion MessageVersion
        {
            get { return InnerMessageEncodingBindingElement.MessageVersion; }
            set { InnerMessageEncodingBindingElement.MessageVersion = value; }
        }

        /// <summary>
        /// CreateMessageEncoderFactory method implementation
        /// </summary>
        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new ServicesMessageEncoderFactory(InnerMessageEncodingBindingElement.CreateMessageEncoderFactory());
        }

        /// <summary>
        /// Clone method override
        /// </summary>
        public override BindingElement Clone()
        {
            return new ServicesMessageEncodingBindingElement(this.InnerMessageEncodingBindingElement);
        }

        /// <summary>
        /// GetProperty method implementation
        /// </summary>
        public override T GetProperty<T>(BindingContext context)
        {
            if (typeof(T) == typeof(XmlDictionaryReaderQuotas))
            {
                return InnerMessageEncodingBindingElement.GetProperty<T>(context);
            }
            else
            {
                return base.GetProperty<T>(context);
            }
        }

        /// <summary>
        /// BuildChannelFactory method override
        /// </summary>
        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.BindingParameters.Add(this);
            return context.BuildInnerChannelFactory<TChannel>();
        }

        /// <summary>
        /// BuildChannelListener method override
        /// </summary>
        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.BindingParameters.Add(this);
            return context.BuildInnerChannelListener<TChannel>();
        }

        /// <summary>
        /// CanBuildChannelListener method override
        /// </summary>
        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelListener<TChannel>();
        }

        /// <summary>
        /// ExportPolicy method implementation
        /// </summary>
        public void ExportPolicy(MetadataExporter exporter, PolicyConversionContext policyContext)
        {
            if (policyContext == null)
            {
                throw new ArgumentNullException("policyContext");
            }
            XmlDocument document = new XmlDocument();
            policyContext.GetBindingAssertions().Add(document.CreateElement(ServicesMessageEncodingPolicyConstants.MFAEncodingPrefix, ServicesMessageEncodingPolicyConstants.MFAEncodingName, ServicesMessageEncodingPolicyConstants.MFAEncodingNamespace));
        }
    }
    #endregion

    #region ServicesMessageEncodingElement
    /// <summary>
    /// ServicesMessageEncodingElement Class
    /// </summary>
    public class ServicesMessageEncodingElement : BindingElementExtensionElement
    {
        /// <summary>
        /// ServicesMessageEncodingElement constructor
        /// </summary>
        public ServicesMessageEncodingElement()
        {
        }

        /// <summary>
        /// BindingElementType property override
        /// </summary>
        public override Type BindingElementType
        {
            get { return typeof(ServicesMessageEncodingBindingElement); }
        }

        /// <summary>
        /// InnerMessageEncoding property
        /// </summary>
        [ConfigurationProperty("innerMessageEncoding", DefaultValue = "textMessageEncoding")]
        public string InnerMessageEncoding
        {
            get { return (string)base["innerMessageEncoding"]; }
            set { base["innerMessageEncoding"] = value; }
        }

        /// <summary>
        /// CreateBindingElement method override
        /// </summary>
        protected override BindingElement CreateBindingElement()
        {
            ServicesMessageEncodingBindingElement bindingElement = new ServicesMessageEncodingBindingElement();
            this.ApplyConfiguration(bindingElement);
            return bindingElement;
        }


        /// <summary>
        /// ApplyConfiguration method override
        /// </summary>
        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            ServicesMessageEncodingBindingElement binding = (ServicesMessageEncodingBindingElement)bindingElement;
            PropertyInformationCollection propertyInfo = this.ElementInformation.Properties;
            if (propertyInfo["innerMessageEncoding"].ValueOrigin != PropertyValueOrigin.Default)
            {
                switch (this.InnerMessageEncoding)
                {
                    case "textMessageEncoding":
                        binding.InnerMessageEncodingBindingElement = new TextMessageEncodingBindingElement();
                        break;
                    case "binaryMessageEncoding":
                        binding.InnerMessageEncodingBindingElement = new BinaryMessageEncodingBindingElement();
                        break;
                }
            }
        }
    }
    #endregion

    #region ServicesMessageEncodingPolicyConstants
    /// <summary>
    /// ServicesMessageEncodingPolicyConstants class
    /// </summary>
    public static class ServicesMessageEncodingPolicyConstants
    {
        public const string MFAEncodingName = "adfsmfaEncoding";
        public const string MFAEncodingNamespace = "http://schemas.microsoft.com/ws/06/2004/mspolicy/netmfaservers";
        public const string MFAEncodingPrefix = "mfa";
    }
    #endregion

    #region ServicesMessageEncodingBindingElementImporter
    public class ServicesMessageEncodingBindingElementImporter : IPolicyImportExtension
    {
        /// <summary>
        /// ServicesMessageEncodingBindingElementImporter Constructor
        /// </summary>
        public ServicesMessageEncodingBindingElementImporter()
        {
        }

        /// <summary>
        /// ImportPolicy method implementation
        /// </summary>
        public void ImportPolicy(MetadataImporter importer, PolicyConversionContext context)
        {
            if (importer == null)
            {
                throw new ArgumentNullException("importer");
            }

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ICollection<XmlElement> assertions = context.GetBindingAssertions();
            foreach (XmlElement assertion in assertions)
            {
                if ((assertion.NamespaceURI == ServicesMessageEncodingPolicyConstants.MFAEncodingNamespace) &&
                    (assertion.LocalName == ServicesMessageEncodingPolicyConstants.MFAEncodingName)
                    )
                {
                    assertions.Remove(assertion);
                    context.BindingElements.Add(new ServicesMessageEncodingBindingElement());
                    break;
                }
            }
        }
    }
    #endregion

    #region MFAAuthorizationPolicy
    public class MFAAuthorizationPolicy : IAuthorizationPolicy
    {
        private static string _delegatedgroupname;

        /// <summary>
        /// MFAAuthorizationPolicy constructor implementation
        /// </summary>
        public MFAAuthorizationPolicy()
        {
            if (string.IsNullOrEmpty(_delegatedgroupname))
                _delegatedgroupname = GetDelegatedGroupValue();
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Id property implementation
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Issuer property implementation
        /// </summary>
        public ClaimSet Issuer
        {
            get { return ClaimSet.Windows; }
        }

        /// <summary>
        /// Evaluate method implementation
        /// </summary>
        public bool Evaluate(EvaluationContext context, ref object state)
        {
            MFAAuthState customstate;
            if (state == null)
            {
                customstate = new MFAAuthState();
                state = customstate;
            }
            else
                customstate = (MFAAuthState)state;

            bool bRet;
            if (!customstate.Checked)
            {
                if (!context.Properties.TryGetValue("Identities", out object obj))
                    return false;

                IList<IIdentity> identities = obj as IList<IIdentity>;
                if (obj == null || identities.Count <= 0)
                    return false;
                customstate.Checked = true;
                WindowsIdentity idt = (WindowsIdentity)identities[0];
                WindowsPrincipal win = new WindowsPrincipal(idt);
                if (ADFSManagementRolesChecker.IsAdministrator(win) || ADFSManagementRolesChecker.IsSystem(idt) || ADFSManagementRolesChecker.AllowedGroup(win, _delegatedgroupname))
                {
                    context.Properties["Principal"] = win;
                    return true;
                }
                bRet = true;
            }
            else
                bRet = true;
            return bRet;
        }

        /// <summary>
        /// GetDelegatedGroupValue method implementation
        /// </summary>
        private static string GetDelegatedGroupValue()
        {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey("Software\\MFA", false);
            object obj = rk.GetValue("DelegatedAdminGroup");
            if (obj == null)
                return string.Empty;
            else
                return obj.ToString();
        }
    }

    /// <summary>
    /// ADFSManagementRolesChecker class
    /// </summary>
    internal static class ADFSManagementRolesChecker
    {
        /// <summary>
        /// IsAdministrator method implementation
        /// </summary>
        public static bool IsAdministrator(WindowsPrincipal principal)
        {
            try
            {
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// IsSystem method implementation
        /// </summary>
        public static bool IsSystem(WindowsIdentity identity)
        {
            try
            {
                return identity.IsSystem;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// AllowedGroup method implementation
        /// </summary>
        public static bool AllowedGroup(WindowsPrincipal principal, string group)
        {
            if (string.IsNullOrEmpty(group))
                return false;
            try
            {
                return principal.IsInRole(group);
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// MFAAuthState class implementation
    /// </summary>
    public class MFAAuthState
    {
        public bool Checked { get; set; }
    }
    #endregion
}
