using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;


namespace Neos.IdentityServer.MultiFactor
{
    public delegate string OnNamedPipeEncryptEvent(string clearvalue);
    public delegate string OnNamedPipeDecryptEvent(string cryptedvalue);
    public delegate void OnNamedPipeMessageEvent(NotificationsKind notif, string username, string application, string value);
    public delegate void OnNamedPipeReplayEvent(IPAddress userIPAdress, int userIPPort, int userCurrentRetries, int userMaxRetries, int deliveryWindow, DateTime userLogon, string userName);

    /// <summary>
    /// PipeServer class implementation
    /// </summary>
    public class PipeServer
    {
        private NamedPipeServerStream ConfigPipeServer;
        private static int numthreads = 1;
        private bool MustExit = false;
        private NamedPipeNotificationRecord _result;

        public event OnNamedPipeEncryptEvent OnEncrypt;
        public event OnNamedPipeDecryptEvent OnDecrypt;
        public event OnNamedPipeMessageEvent OnMessage;


        /// <summary>
        /// PipeServer constructor
        /// </summary>
        public PipeServer()
        {
            Proofkey = XORUtilities.XORKey;
        }

        /// <summary>
        /// PipeServer constructor
        /// </summary>
        public PipeServer(string proof)
        {
            Proofkey = proof;
        }

        /// <summary>
        /// Proofkey property implementation
        /// </summary>
        public string Proofkey { get; set; } = string.Empty;

        /// <summary>
        /// Result property implementation
        /// </summary>
        public NamedPipeNotificationRecord Result { get => _result; set => _result = value; }

        /// <summary>
        /// Start method implementation
        /// </summary>
        public void Start()
        {
            if (this.OnDecrypt==null)
                OnDecrypt += PipeServerOnDecrypt;
            if (this.OnEncrypt == null)
                OnEncrypt += PipeServerOnEncrypt;
            try
            {
                PipeSecurity pipeSecurity = CreatePipeServerSecurity();
                ConfigPipeServer = new NamedPipeServerStream("adfsmfaconfig", PipeDirection.InOut, numthreads, PipeTransmissionMode.Message, PipeOptions.None, 0x4000, 0x4000, pipeSecurity, HandleInheritability.Inheritable, PipeAccessRights.ChangePermissions);

                Thread[] threadservers = new Thread[numthreads];

                for (int i = 0; i < numthreads; i++)
                {
                    threadservers[i] = new Thread(PipeServerConfigThread);
                    threadservers[i].Start();
                }
                Thread.Sleep(250);
            }
            catch (Exception ex )
            {
                LogForSlots.WriteEntry("PipeServer Start Error : " + ex.Message, EventLogEntryType.Error, 8888);
            }
        }

        /// <summary>
        /// Stop method implementation
        /// </summary>
        public void Stop()
        {
            try
            {
                MustExit = true;
                if (ConfigPipeServer != null)
                {
                    ConfigPipeServer.WaitForPipeDrain();
                    ConfigPipeServer.Disconnect();
                }
            }
            catch (Exception ex)
            {
                LogForSlots.WriteEntry("PipeServer Stop Error : " + ex.Message, EventLogEntryType.Error, 8888);
                if (ConfigPipeServer != null)
                    ConfigPipeServer.Close();
            }
            finally
            {
                if (ConfigPipeServer != null)
                {
                    ConfigPipeServer.Close();
                    ConfigPipeServer.Dispose();
                }
                ConfigPipeServer = null;
            }
        }

        /// <summary>
        /// PipeServerOnEncrypt method implementation
        /// </summary>
        private string PipeServerOnEncrypt(string clearvalue)
        {
            return clearvalue;
        }

        /// <summary>
        /// PipeServerOnDecrypt method implementation
        /// </summary>
        private string PipeServerOnDecrypt(string cryptedvalue)
        {
            return cryptedvalue;
        }

        /// <summary>
        /// PipeServerConfigThread method implmentation
        /// </summary>
        private void PipeServerConfigThread(object data)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            try
            {
                while (!MustExit)
                {
                    ConfigPipeServer.WaitForConnection();
                    try
                    {
                        PipeStreamData ss = new PipeStreamData(ConfigPipeServer);
                        if (this.OnDecrypt(ss.ReadString()) == this.Proofkey)
                        {
                            ss.WriteString(this.OnEncrypt(Proofkey));
                            NamedPipeNotificationRecord xdata = ByteArrayToObject(ss.ReadData());

                            Result = new NamedPipeNotificationRecord(xdata.Kind, xdata.Application, this.OnDecrypt(xdata.Message));
                            string username = ConfigPipeServer.GetImpersonationUserName();
                            ExcuteInClientContext(username);
                        }
                        else
                            MustExit = true;
                    }
                    catch (IOException e)
                    {
                        LogForSlots.WriteEntry("PipeServer Error : " + e.Message, EventLogEntryType.Error, 8888);
                        ConfigPipeServer.Close();
                    }
                    finally
                    {
                        ConfigPipeServer.WaitForPipeDrain();
                        ConfigPipeServer.Disconnect();
                    }
                }
            }
            finally
            {
                ConfigPipeServer.Close();
            }
        }

        /// <summary>
        /// ByteArrayToObject method implementation
        /// </summary>
        private NamedPipeNotificationRecord ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            NamedPipeNotificationRecord obj = (NamedPipeNotificationRecord)binForm.Deserialize(memStream);
            return obj;
        }

        /// <summary>
        /// ExcuteInClientContext method
        /// </summary>
        public void ExcuteInClientContext(string username)
        {
            if (this.OnMessage == null)
                return;
            this.OnMessage(Result.Kind, username, Result.Application, Result.Message);
        }

        /// <summary>
        /// CreatePipeServerSecurity method implementation
        /// </summary>
        private PipeSecurity CreatePipeServerSecurity()
        {
            SecurityIdentifier dom = GetDomainSid();
            PipeSecurity pipeSecurity = new PipeSecurity();
            SecurityIdentifier id1 = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
            SecurityIdentifier id2 = new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null);
            SecurityIdentifier id3 = new SecurityIdentifier(WellKnownSidType.AccountDomainAdminsSid, dom);
            SecurityIdentifier id4 = new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null);
            SecurityIdentifier id5 = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

            // Allow Everyone read and write access to the pipe. 
            pipeSecurity.SetAccessRule(new PipeAccessRule(id1, PipeAccessRights.ReadWrite, AccessControlType.Allow));
            pipeSecurity.SetAccessRule(new PipeAccessRule(id2, PipeAccessRights.ReadWrite, AccessControlType.Allow));
            pipeSecurity.SetAccessRule(new PipeAccessRule(id3, PipeAccessRights.ReadWrite, AccessControlType.Allow));
            pipeSecurity.SetAccessRule(new PipeAccessRule(id4, PipeAccessRights.ReadWrite, AccessControlType.Allow));
            pipeSecurity.SetAccessRule(new PipeAccessRule(id5, PipeAccessRights.ReadWrite, AccessControlType.Allow));
            return pipeSecurity;
        }

        /// <summary>
        /// GetDomainSid method implmentation
        /// </summary>
        private SecurityIdentifier GetDomainSid()
        {
            SecurityIdentifier DomainSID = null;
            try
            {
                DirectoryEntry de = Domain.GetCurrentDomain().GetDirectoryEntry();
                byte[] sid  = (byte[])de.InvokeGet("objectSID");
                DomainSID = new SecurityIdentifier(sid, 0);
            }
            catch (Exception ex)
            {
                LogForSlots.WriteEntry("PipeServer Error for DomainSID : " + ex.Message, EventLogEntryType.Error, 8887);
            }
            return DomainSID.AccountDomainSid;
        }
    }

#if forreplay
    /// <summary>
    /// PipeServer class implementation
    /// </summary>
    public class PipeReplayServer
    {
        private NamedPipeServerStream ConfigPipeServer;
        private static int numthreads = 1;
        private bool MustExit = false;
        private NamedPipeClientNotificationReplayRecord _result;

        public event OnNamedPipeEncryptEvent OnEncrypt;
        public event OnNamedPipeDecryptEvent OnDecrypt;
        public event OnNamedPipeReplayEvent OnMessage;


        /// <summary>
        /// PipeServer constructor
        /// </summary>
        public PipeReplayServer()
        {
            Proofkey = XORUtilities.XORKey;
        }

        /// <summary>
        /// PipeServer constructor
        /// </summary>
        public PipeReplayServer(string proof)
        {
            Proofkey = proof;
        }

        /// <summary>
        /// Proofkey property implementation
        /// </summary>
        public string Proofkey { get; set; } = string.Empty;

        /// <summary>
        /// Result property implementation
        /// </summary>
        public NamedPipeClientNotificationReplayRecord Result { get => _result; set => _result = value; }

        /// <summary>
        /// Start method implementation
        /// </summary>
        public void Start()
        {
            if (this.OnDecrypt == null)
                OnDecrypt += PipeServerOnDecrypt;
            if (this.OnEncrypt == null)
                OnEncrypt += PipeServerOnEncrypt;
            try
            {
                PipeSecurity pipeSecurity = CreatePipeServerSecurity();
                ConfigPipeServer = new NamedPipeServerStream("adfsmfareplayserver", PipeDirection.InOut, numthreads, PipeTransmissionMode.Message, PipeOptions.None, 0x4000, 0x4000, pipeSecurity, HandleInheritability.Inheritable, PipeAccessRights.ChangePermissions);

                Thread[] threadservers = new Thread[numthreads];

                for (int i = 0; i < numthreads; i++)
                {
                    threadservers[i] = new Thread(PipeServerConfigThread);
                    threadservers[i].Start();
                }
                Thread.Sleep(250);
            }
            catch (Exception ex)
            {
                LogForSlots.WriteEntry("PipeServer Start Error : " + ex.Message, EventLogEntryType.Error, 8888);
            }
        }

        /// <summary>
        /// Stop method implementation
        /// </summary>
        public void Stop()
        {
            try
            {
                MustExit = true;
                if (ConfigPipeServer != null)
                {
                    ConfigPipeServer.WaitForPipeDrain();
                    ConfigPipeServer.Disconnect();
                }
            }
            catch (Exception ex)
            {
                LogForSlots.WriteEntry("PipeServer Stop Error : " + ex.Message, EventLogEntryType.Error, 8888);
                if (ConfigPipeServer != null)
                    ConfigPipeServer.Close();
            }
            finally
            {
                if (ConfigPipeServer != null)
                {
                    ConfigPipeServer.Close();
                    ConfigPipeServer.Dispose();
                }
                ConfigPipeServer = null;
            }
        }

        /// <summary>
        /// PipeServerOnEncrypt method implementation
        /// </summary>
        private string PipeServerOnEncrypt(string clearvalue)
        {
            return clearvalue;
        }

        /// <summary>
        /// PipeServerOnDecrypt method implementation
        /// </summary>
        private string PipeServerOnDecrypt(string cryptedvalue)
        {
            return cryptedvalue;
        }

        /// <summary>
        /// PipeServerConfigThread method implmentation
        /// </summary>
        private void PipeServerConfigThread(object data)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            try
            {
                while (!MustExit)
                {
                    ConfigPipeServer.WaitForConnection();
                    try
                    {
                        PipeStreamData ss = new PipeStreamData(ConfigPipeServer);
                        if (this.OnDecrypt(ss.ReadString()) == this.Proofkey)
                        {
                            ss.WriteString(this.OnEncrypt(Proofkey));
                            NamedPipeClientReplayRecord xdata = ByteArrayToObject(ss.ReadData());

                            Result = new NamedPipeClientNotificationReplayRecord(xdata.UserIPAdress, xdata.UserIPPort, xdata.UserCurrentRetries, xdata.UserMaxRetries, xdata.DeliveryWindow, xdata.UserLogon, this.OnDecrypt(xdata.UserName));
                            string username = ConfigPipeServer.GetImpersonationUserName();
                            ExcuteInClientContext(username);
                        }
                        else
                            MustExit = true;
                    }
                    catch (IOException e)
                    {
                        LogForSlots.WriteEntry("PipeServer Error : " + e.Message, EventLogEntryType.Error, 8888);
                        ConfigPipeServer.Close();
                    }
                    finally
                    {
                        ConfigPipeServer.WaitForPipeDrain();
                        ConfigPipeServer.Disconnect();
                    }
                }
            }
            finally
            {
                ConfigPipeServer.Close();
            }
        }

        /// <summary>
        /// ByteArrayToObject method implementation
        /// </summary>
        private NamedPipeClientReplayRecord ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            NamedPipeClientReplayRecord obj = (NamedPipeClientReplayRecord)binForm.Deserialize(memStream);
            return obj;
        }

        /// <summary>
        /// ExcuteInClientContext method
        /// </summary>
        public void ExcuteInClientContext(string username)
        {
            if (this.OnMessage == null)
                return;
            this.OnMessage(Result.UserIPAdress, Result.UserIPPort, Result.UserCurrentRetries, Result.UserMaxRetries, Result.DeliveryWindow, Result.UserLogon, Result.UserName);
        }

        /// <summary>
        /// CreatePipeServerSecurity method implementation
        /// </summary>
        private PipeSecurity CreatePipeServerSecurity()
        {
            SecurityIdentifier dom = GetDomainSid();
            PipeSecurity pipeSecurity = new PipeSecurity();
            SecurityIdentifier id1 = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
            SecurityIdentifier id2 = new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null);
            SecurityIdentifier id3 = new SecurityIdentifier(WellKnownSidType.AccountDomainAdminsSid, dom);
            SecurityIdentifier id4 = new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null);
            SecurityIdentifier id5 = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

            // Allow Everyone read and write access to the pipe. 
            pipeSecurity.SetAccessRule(new PipeAccessRule(id1, PipeAccessRights.ReadWrite, AccessControlType.Allow));
            pipeSecurity.SetAccessRule(new PipeAccessRule(id2, PipeAccessRights.ReadWrite, AccessControlType.Allow));
            pipeSecurity.SetAccessRule(new PipeAccessRule(id3, PipeAccessRights.ReadWrite, AccessControlType.Allow));
            pipeSecurity.SetAccessRule(new PipeAccessRule(id4, PipeAccessRights.ReadWrite, AccessControlType.Allow));
            pipeSecurity.SetAccessRule(new PipeAccessRule(id5, PipeAccessRights.ReadWrite, AccessControlType.Allow));
            return pipeSecurity;
        }

        /// <summary>
        /// GetDomainSid method implmentation
        /// </summary>
        private SecurityIdentifier GetDomainSid()
        {
            SecurityIdentifier DomainSID = null;
            try
            {
                DirectoryEntry de = Domain.GetCurrentDomain().GetDirectoryEntry();
                byte[] sid = (byte[])de.InvokeGet("objectSID");
                DomainSID = new SecurityIdentifier(sid, 0);
            }
            catch (Exception ex)
            {
                LogForSlots.WriteEntry("PipeServer Error for DomainSID : " + ex.Message, EventLogEntryType.Error, 8887);
            }
            return DomainSID.AccountDomainSid;
        }
    }
#endif

    /// <summary>
    /// PipeClient class implmentation
    /// </summary>
    public class PipeClient
    {
        private List<string> _servers = new List<string>();
        private List<NamedPipeClientRecord> _pipestreams = new List<NamedPipeClientRecord>();

        public event OnNamedPipeEncryptEvent OnEncrypt;
        public event OnNamedPipeDecryptEvent OnDecrypt;

        /// <summary>
        /// PipeClient Constructor
        /// </summary>
        public PipeClient()
        {
            Proofkey = XORUtilities.XORKey;
        }

        /// <summary>
        /// PipeClient Constructor
        /// </summary>
        public PipeClient(string key)
        {
            Proofkey = key;
        }

        /// <summary>
        /// PipeClient Constructor
        /// </summary>
        public PipeClient(List<string> servers):this()
        {
            Servers = servers;
        }

        /// <summary>
        /// PipeClient Constructor
        /// </summary>
        public PipeClient(string key, List<string> servers) : this(key)
        {
            Servers = servers;
        }

        /// <summary>
        /// Proofkey property
        /// </summary>
        public string Proofkey { get; set; } = string.Empty;

        /// <summary>
        /// Servers property implementation
        /// </summary>
        public List<string> Servers
        {
            get { return _servers; }
            set
            {
                value.RemoveAll(item => item.ToLower() == Environment.MachineName.ToLower());
                if (value.Count > 0)
                {
                    _servers.Clear();
                    _servers.AddRange(value);
                }
            }
        }

        /// <summary>
        /// SendMessage method implementation
        /// </summary>
        public bool SendMessage(NotificationsKind notif, string application, string value)
        {
            try
            {
                if (this.OnDecrypt == null)
                    OnDecrypt += PipeClientOnDecrypt;
                if (this.OnEncrypt == null)
                    OnEncrypt += PipeClientOnEncrypt;
                for (int i = 0; i < _servers.Count; i++)
                {
                    Thread tstreams = new Thread(PipeClientConfigThread);
                    NamedPipeClientRecord rec;
                    rec.ThreadId = tstreams.ManagedThreadId;
                    rec.MachineName = _servers[i];
                    rec.Message = value;
                    rec.Application = application;
                    rec.NotificationsKind = notif;
                    rec.ClientStream = new NamedPipeClientStream(_servers[i], "adfsmfaconfig", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
                    _pipestreams.Add(rec);
                    tstreams.Start();
                }
            }
            catch (Exception e)
            {
                LogForSlots.WriteEntry("PipeClient Error : " + e.Message, EventLogEntryType.Error, 8888);
                return false;
            }
            return true;
        }

        /// <summary>
        /// PipeClientOnEncrypt method implementation
        /// </summary>
        private string PipeClientOnEncrypt(string clearvalue)
        {
            return clearvalue;
        }

        /// <summary>
        /// PipeClientOnDecrypt method implementation
        /// </summary>
        private string PipeClientOnDecrypt(string cryptedvalue)
        {
            return cryptedvalue;
        }

        /// <summary>
        /// PipeClientConfigThread method implmentation
        /// </summary>
        private void PipeClientConfigThread(object data)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            NamedPipeClientStream ConfigPipeClient = _pipestreams.Find(item => item.ThreadId == threadId).ClientStream;
            string Message = _pipestreams.Find(item => item.ThreadId == threadId).Message;
            NotificationsKind Kind = _pipestreams.Find(item => item.ThreadId == threadId).NotificationsKind;
            string Appli = _pipestreams.Find(item => item.ThreadId == threadId).Application;

            try
            {
                ConfigPipeClient.Connect();
                PipeStreamData ss = new PipeStreamData(ConfigPipeClient);
                ss.WriteString(this.OnEncrypt(Proofkey));
                if (this.OnDecrypt(ss.ReadString()) == Proofkey)
                {
                    NamedPipeNotificationRecord xdata = new NamedPipeNotificationRecord(Kind, Appli, this.OnEncrypt(Message));
                    ss.WriteData(ObjectToByteArray(xdata));
                }
            }
            catch (IOException e)
            {
                LogForSlots.WriteEntry("PipeClient Error : " + e.Message, EventLogEntryType.Error, 8888);
                ConfigPipeClient.Close();
            }
            finally
            {
                ConfigPipeClient.Close();
            }
        }

        /// <summary>
        /// ObjectToByteArray method implementation
        /// </summary>
        private byte[] ObjectToByteArray(NamedPipeNotificationRecord obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }

#if forreplay
    /// <summary>
    /// PipeReplayClient class implmentation
    /// </summary>
    public class PipeReplayClient
    {
        private List<string> _servers = new List<string>();
        private List<NamedPipeClientReplayRecord> _pipestreams = new List<NamedPipeClientReplayRecord>();

        public event OnNamedPipeEncryptEvent OnEncrypt;
        public event OnNamedPipeDecryptEvent OnDecrypt;

        /// <summary>
        /// PipeClient Constructor
        /// </summary>
        public PipeReplayClient()
        {
            Proofkey = XORUtilities.XORKey;
        }

        /// <summary>
        /// PipeClient Constructor
        /// </summary>
        public PipeReplayClient(string key)
        {
            Proofkey = key;
        }

        /// <summary>
        /// PipeClient Constructor
        /// </summary>
        public PipeReplayClient(List<string> servers) : this()
        {
            Servers = servers;
        }

        /// <summary>
        /// PipeClient Constructor
        /// </summary>
        public PipeReplayClient(string key, List<string> servers) : this(key)
        {
            Servers = servers;
        }

        /// <summary>
        /// Proofkey property
        /// </summary>
        public string Proofkey { get; set; } = string.Empty;

        /// <summary>
        /// Servers property implementation
        /// </summary>
        public List<string> Servers
        {
            get { return _servers; }
            set
            {
                value.RemoveAll(item => item.ToLower() == Environment.MachineName.ToLower());
                if (value.Count > 0)
                {
                    _servers.Clear();
                    _servers.AddRange(value);
                }
            }
        }

        /// <summary>
        /// CheckForReplay method implementation
        /// </summary>
        public bool CheckForReplay(NamedPipeClientReplayRecord rec)
        {
            try
            {
                if (this.OnDecrypt == null)
                    OnDecrypt += PipeClientOnDecrypt;
                if (this.OnEncrypt == null)
                    OnEncrypt += PipeClientOnEncrypt;
                NamedPipeClientStream ConfigPipeClient = new NamedPipeClientStream(".", "adfsmfareplayserver", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
                try
                {
                    ConfigPipeClient.Connect();
                    PipeStreamData ss = new PipeStreamData(ConfigPipeClient);
                    ss.WriteString(this.OnEncrypt(Proofkey));
                    if (this.OnDecrypt(ss.ReadString()) == Proofkey)
                    {
                        NamedPipeClientNotificationReplayRecord xdata = new NamedPipeClientNotificationReplayRecord(rec.UserIPAdress, rec.UserIPPort, rec.UserCurrentRetries, rec.UserMaxRetries, rec.DeliveryWindow, rec.UserLogon, this.OnEncrypt(rec.UserName));
                        ss.WriteData(ObjectToByteArray(xdata));
                    }
                }
                catch (IOException e)
                {
                    LogForSlots.WriteEntry("PipeClient Error : " + e.Message, EventLogEntryType.Error, 8888);
                    ConfigPipeClient.Close();
                }
                finally
                {
                    ConfigPipeClient.Close();
                }
            }
            catch (Exception e)
            {
                LogForSlots.WriteEntry("PipeClient Error : " + e.Message, EventLogEntryType.Error, 8888);
                return false;
            }
            return true;
        }
        /// <summary>
        /// PipeClientOnEncrypt method implementation
        /// </summary>
        private string PipeClientOnEncrypt(string clearvalue)
        {
            return clearvalue;
        }

        /// <summary>
        /// PipeClientOnDecrypt method implementation
        /// </summary>
        private string PipeClientOnDecrypt(string cryptedvalue)
        {
            return cryptedvalue;
        }

        /// <summary>
        /// PipeClientConfigThread method implmentation
        /// </summary>
     /*   private void PipeClientConfigThread(object data)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            NamedPipeClientStream ConfigPipeClient = _pipestreams[0].ClientStream;

            try
            {
                ConfigPipeClient.Connect();
                PipeStreamData ss = new PipeStreamData(ConfigPipeClient);
                ss.WriteString(this.OnEncrypt(Proofkey));
                if (this.OnDecrypt(ss.ReadString()) == Proofkey)
                {
                    NamedPipeClientReplayRecord param = _pipestreams[0];
                    NamedPipeClientNotificationReplayRecord xdata = new NamedPipeClientNotificationReplayRecord(param.UserIPAdress, param.UserIPPort, param.UserCurrentRetries, param.UserMaxRetries, param.DeliveryWindow, param.UserLogon, this.OnEncrypt(param.UserName));
                    ss.WriteData(ObjectToByteArray(xdata));
                }
            }
            catch (IOException e)
            {
                LogForSlots.WriteEntry("PipeClient Error : " + e.Message, EventLogEntryType.Error, 8888);
                ConfigPipeClient.Close();
            }
            finally
            {
                ConfigPipeClient.Close();
            }
        } */

        /// <summary>
        /// ObjectToByteArray method implementation
        /// </summary>
        private byte[] ObjectToByteArray(NamedPipeClientNotificationReplayRecord obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
#endif

    /// <summary>
    /// PipeStreamData class implementation
    /// </summary>
    public class PipeStreamData
    {
        private Stream ioStream;      

        /// <summary>
        /// PipeStreamData constructor
        /// </summary>
        public PipeStreamData(Stream ioStream)
        {
            this.ioStream = ioStream;
        }

#region Strings methods
        /// <summary>
        /// ReadString ReadData method implementation
        /// </summary>
        public string ReadString()
        {
            UnicodeEncoding streamEncoding = new UnicodeEncoding();
            int len = 0;
            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);
            return streamEncoding.GetString(inBuffer);
        }

        /// <summary>
        /// WriteString method implementation
        /// </summary>
        public int WriteString(string outString)
        {
            UnicodeEncoding streamEncoding = new UnicodeEncoding();
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();
            return outBuffer.Length + 2;
        }
#endregion

#region Bytes methods
        /// <summary>
        /// ReadData ReadData method implementation
        /// </summary>
        public byte[] ReadData()
        {
            int len = 0;

            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);
            return inBuffer;
        }

        /// <summary>
        /// WriteData method implementation
        /// </summary>
        public int WriteData(byte[] outBuffer)
        {
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();
            return outBuffer.Length + 2;
        }
#endregion
    }

    /// <summary>
    /// NamedPipeClientRecord class implementation
    /// </summary>
    public struct NamedPipeClientRecord
    {
        public int ThreadId;
        public string MachineName;
        public string Application;
        public NotificationsKind NotificationsKind;
        public string Message;
        public NamedPipeClientStream ClientStream;
    }

    /// <summary>
    /// NamedPipeNotificationRecord class implementation
    /// </summary>
    [Serializable]
    public struct NamedPipeNotificationRecord
    {
        public string Application;
        public NotificationsKind Kind;
        public string Message;

        public NamedPipeNotificationRecord(NotificationsKind kind, String application, string msg)
        {
            Application = application;
            Kind = kind;
            Message = msg;
        }
    }

    /// <summary>
    /// NamedPipeClientReplayRecord class implementation
    /// </summary>
    public struct NamedPipeClientReplayRecord
    {
        public IPAddress UserIPAdress;
        public int UserIPPort;
        public int UserCurrentRetries;
        public int UserMaxRetries;
        public int DeliveryWindow;
        public DateTime UserLogon;
        public string UserName;
        public NamedPipeClientStream ClientStream;
    }

    /// <summary>
    /// NamedPipeNotificationRecord class implementation
    /// </summary>
    [Serializable]
    public struct NamedPipeClientNotificationReplayRecord
    {
        public IPAddress UserIPAdress;
        public int UserIPPort;
        public int UserCurrentRetries;
        public int UserMaxRetries;
        public int DeliveryWindow;
        public DateTime UserLogon;
        public string UserName;

        public NamedPipeClientNotificationReplayRecord(IPAddress userIPAdress, int userIPPort, int userCurrentRetries, int userMaxRetries, int deliveryWindow, DateTime userLogon, string userName)
        {
            UserIPAdress = userIPAdress;
            UserIPPort = userIPPort;
            UserCurrentRetries = userCurrentRetries;
            UserMaxRetries = userMaxRetries;
            DeliveryWindow = deliveryWindow;
            UserLogon = userLogon;
            UserName = userName;
        }
    }
}
