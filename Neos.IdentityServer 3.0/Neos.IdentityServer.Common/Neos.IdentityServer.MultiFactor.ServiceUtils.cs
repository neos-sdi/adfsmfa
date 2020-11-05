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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Net;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.IO.Compression;
using System.Xml;
using System.IO;
using System.ServiceModel.Configuration;
using System.Configuration;
using System.ServiceProcess;
using System.ServiceModel.Activation;
using System.Diagnostics;

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
        public ReplayServiceHost(IDependency dep, Type serviceType, params Uri[] baseAddresses): base(serviceType, baseAddresses)
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
    /// ReplayServiceHost Class
    /// </summary>
    public class WebThemesServiceHost : ServiceHost
    {
        /// <summary>
        /// ReplayServiceHost Constructor
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
            var disposable = instance as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
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
            if (disposable != null)
            {
                disposable.Dispose();
            }
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
    public class EventLogDependency: IDependency
    {
        private EventLog _eventlog;
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
        private bool useEncryption = true;

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

                Servicehost = new ReplayServiceHost(new EventLogDependency(svc.EventLog), typeof(T), new Uri(string.Format("net.tcp://{0}:5987/ReplayService", Dns.GetHostEntry("localhost").HostName)));
                ServiceMetadataBehavior smb = Servicehost.Description.Behaviors.Find<ServiceMetadataBehavior>();
                ServiceDebugBehavior dbg = Servicehost.Description.Behaviors.Find<ServiceDebugBehavior>();
                ServiceThrottlingBehavior prf = Servicehost.Description.Behaviors.Find<ServiceThrottlingBehavior>();
                if (smb == null)
                {
                    smb = new ServiceMetadataBehavior();
                    smb.HttpGetEnabled = false;
                    smb.HttpsGetEnabled = false;
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
                    dbg = new ServiceDebugBehavior();
                    dbg.HttpHelpPageEnabled = false;
                    dbg.HttpsHelpPageEnabled = false;
                    dbg.IncludeExceptionDetailInFaults = true;
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
                    prf = new ServiceThrottlingBehavior();
                    prf.MaxConcurrentCalls = 256;
                    prf.MaxConcurrentInstances = 256;
                    prf.MaxConcurrentSessions = 256;
                    Servicehost.Description.Behaviors.Add(prf);
                }
                else
                {
                    prf.MaxConcurrentCalls = 250;
                    prf.MaxConcurrentInstances = 250;
                    prf.MaxConcurrentSessions = 250;
                }

                NetTcpBinding tcp = new NetTcpBinding(SecurityMode.None);
                tcp.MaxConnections = 256;

                if (useEncryption)
                {
                    CustomBinding custombinding = new CustomBinding(tcp);
                    custombinding.Name = "MFABinding";
                    var currentEncoder = custombinding.Elements.Find<MessageEncodingBindingElement>();
                    if (currentEncoder != default(MessageEncodingBindingElement))
                    {
                        custombinding.Elements.Remove(currentEncoder);
                        custombinding.Elements.Insert(0, new ServicesMessageEncodingBindingElement());
                    }
                    Servicehost.AddServiceEndpoint(typeof(IReplay), custombinding, "");
                }
                else
                    Servicehost.AddServiceEndpoint(typeof(IReplay), tcp, "");
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
        private bool useEncryption = true;

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

                Servicehost = new WebThemesServiceHost(new EventLogDependency(svc.EventLog), typeof(T), new Uri(string.Format("net.tcp://{0}:5987/WebThemesService", Dns.GetHostEntry("localhost").HostName)));
                ServiceMetadataBehavior smb = Servicehost.Description.Behaviors.Find<ServiceMetadataBehavior>();
                ServiceDebugBehavior dbg = Servicehost.Description.Behaviors.Find<ServiceDebugBehavior>();
                ServiceThrottlingBehavior prf = Servicehost.Description.Behaviors.Find<ServiceThrottlingBehavior>();
                if (smb == null)
                {
                    smb = new ServiceMetadataBehavior();
                    smb.HttpGetEnabled = false;
                    smb.HttpsGetEnabled = false;
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
                    dbg = new ServiceDebugBehavior();
                    dbg.HttpHelpPageEnabled = false;
                    dbg.HttpsHelpPageEnabled = false;
                    dbg.IncludeExceptionDetailInFaults = true;
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
                    prf = new ServiceThrottlingBehavior();
                    prf.MaxConcurrentCalls = 256;
                    prf.MaxConcurrentInstances = 256;
                    prf.MaxConcurrentSessions = 256;
                    Servicehost.Description.Behaviors.Add(prf);
                }
                else
                {
                    prf.MaxConcurrentCalls = 250;
                    prf.MaxConcurrentInstances = 250;
                    prf.MaxConcurrentSessions = 250;
                }

                NetTcpBinding tcp = new NetTcpBinding(SecurityMode.None);
                tcp.MaxConnections = 256;

                if (useEncryption)
                {
                    CustomBinding custombinding = new CustomBinding(tcp);
                    custombinding.Name = "MFABindingThemes";
                    var currentEncoder = custombinding.Elements.Find<MessageEncodingBindingElement>();
                    if (currentEncoder != default(MessageEncodingBindingElement))
                    {
                        custombinding.Elements.Remove(currentEncoder);
                        custombinding.Elements.Insert(0, new ServicesMessageEncodingBindingElement());
                    }
                    Servicehost.AddServiceEndpoint(typeof(IWebThemeManager), custombinding, "");
                }
                else
                    Servicehost.AddServiceEndpoint(typeof(IWebThemeManager), tcp, "");
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
        private bool useEncryption = true;

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
            NetTcpBinding tcp = new NetTcpBinding(SecurityMode.None);
            tcp.MaxConnections = 256;
            if (useEncryption)
            {               
                CustomBinding custombinding = new CustomBinding(tcp);
                custombinding.Name = "MFABinding";
                var currentEncoder = custombinding.Elements.Find<MessageEncodingBindingElement>();
                if (currentEncoder != default(MessageEncodingBindingElement))
                {
                    custombinding.Elements.Remove(currentEncoder);
                    custombinding.Elements.Insert(0, new ServicesMessageEncodingBindingElement());
                }
                _factory = new ChannelFactory<IReplay>(custombinding, new EndpointAddress(string.Format("net.tcp://{0}:5987/ReplayService", servername)));                
            }
            else
                _factory = new ChannelFactory<IReplay>(tcp, new EndpointAddress(string.Format("net.tcp://{0}:5987/ReplayService", servername)));
            IsInitialized = true;
        }

        /// <summary>
        /// UnInitialize method implementation
        /// </summary>
        public void UnInitialize()
        {
            if (_factory != null)
            {
                if (_factory.State==CommunicationState.Opened)
                    _factory.Close();
            }
            IsInitialized = false;
        }

        /// <summary>
        /// Open method implementation
        /// </summary>
        public IReplay Open()
        {
            return _factory.CreateChannel();
        }

        /// <summary>
        /// Close method implementation
        /// </summary>
        public void Close(IReplay client)
        {
            if (client != null)
                ((IClientChannel)client).Close();
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
        private bool useEncryption = true;

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
            NetTcpBinding tcp = new NetTcpBinding(SecurityMode.None);
            tcp.MaxConnections = 256;
            if (useEncryption)
            {
                CustomBinding custombinding = new CustomBinding(tcp);
                custombinding.Name = "MFABindingThemes";
                var currentEncoder = custombinding.Elements.Find<MessageEncodingBindingElement>();
                if (currentEncoder != default(MessageEncodingBindingElement))
                {
                    custombinding.Elements.Remove(currentEncoder);
                    custombinding.Elements.Insert(0, new ServicesMessageEncodingBindingElement());
                }
                _factory = new ChannelFactory<IWebThemeManager>(custombinding, new EndpointAddress(string.Format("net.tcp://{0}:5987/WebThemesService", servername)));
            }
            else
                _factory = new ChannelFactory<IWebThemeManager>(tcp, new EndpointAddress(string.Format("net.tcp://{0}:5987/WebThemesService", servername)));
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
            }
            IsInitialized = false;
        }

        /// <summary>
        /// Open method implementation
        /// </summary>
        public IWebThemeManager Open()
        {
            return _factory.CreateChannel();
        }

        /// <summary>
        /// Close method implementation
        /// </summary>
        public void Close(IWebThemeManager client)
        {
            if (client != null)
                ((IClientChannel)client).Close();
        }
    }
    #endregion

    #region ServicesMessageEncoderFactory
    /// <summary>
    /// ServicesMessageEncoderFactory class
    /// </summary>
    internal class ServicesMessageEncoderFactory : MessageEncoderFactory
    {
        MessageEncoder encoder;

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
            static string ReplayContentType = " application/soap+xml";
            static object _lck = new object();

            /// <summary>
            /// innerEncoder variable
            /// </summary>
            MessageEncoder innerEncoder;


            /// <summary>
            /// ReplayMessageEncoder Constructor
            /// </summary>
            /// <param name="messageEncoder"></param>
            internal SercicesMessageEncoder(MessageEncoder messageEncoder) : base()
            {
                if (messageEncoder == null)
                    throw new ArgumentNullException("messageEncoder", "A valid message encoder must be passed to the Replay Encoder");
                innerEncoder = messageEncoder;
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
    public sealed class ServicesMessageEncodingBindingElement: MessageEncodingBindingElement, IPolicyExportExtension
    {
        /// <summary>
        /// ServicesMessageEncodingBindingElement Constructor
        /// </summary>
        public ServicesMessageEncodingBindingElement(): this(new TextMessageEncodingBindingElement()) { }

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
}
