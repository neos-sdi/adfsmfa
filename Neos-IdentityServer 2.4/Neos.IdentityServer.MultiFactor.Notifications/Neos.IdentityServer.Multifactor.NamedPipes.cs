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
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor
{
    public delegate string OnNamedPipeEncryptEvent(string clearvalue);
    public delegate string OnNamedPipeDecryptEvent(string cryptedvalue);
    public delegate bool OnReloadConfigurationEvent(string requestor, string value);
    public delegate NamedPipeRegistryRecord OnRequestServerConfigurationEvent(string requestor);
    public delegate void OnNamedPipeReplayEvent(IPAddress userIPAdress, int userIPPort, int userCurrentRetries, int userMaxRetries, int deliveryWindow, DateTime userLogon, string userName);

    /// <summary>
    /// PipeServer class implementation
    /// </summary>
    public class PipeServer
    {
        private NamedPipeServerStream ConfigPipeServer;
        private static int numthreads = 1;
        private bool MustExit = false;

        public event OnNamedPipeEncryptEvent OnEncrypt;
        public event OnNamedPipeDecryptEvent OnDecrypt;
        public event OnReloadConfigurationEvent OnReloadConfiguration;
        public event OnRequestServerConfigurationEvent OnRequestServerConfiguration;


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
                        if (OnDecrypt(ss.ReadString()) == this.Proofkey)
                        {
                            ss.WriteString(this.OnEncrypt(Proofkey));
                            object obj = ByteArrayToObject<object>(ss.ReadData());

                            if (obj is NamedPipeReloadConfigRecord)
                            {
                                NamedPipeReloadConfigRecord encrypted = (NamedPipeReloadConfigRecord)obj;
                                bool b = false;
                                if (OnReloadConfiguration != null)
                                {
                                    b = OnReloadConfiguration(encrypted.Requestor, OnDecrypt(encrypted.Message));
                                    ss.WriteData(ObjectToByteArray<bool>(b));
                                }
                            }
                            else if (obj is NamedPipeServerConfigRecord)
                            {
                                NamedPipeServerConfigRecord encrypted = (NamedPipeServerConfigRecord)obj;
                                NamedPipeRegistryRecord reg;
                                if (OnRequestServerConfiguration != null)
                                {
                                    reg = OnRequestServerConfiguration(encrypted.Requestor);
                                    ss.WriteData(ObjectToByteArray<NamedPipeRegistryRecord>(reg));
                                }
                            }
                        }
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
        /// ObjectToByteArray method implementation
        /// </summary>
        private byte[] ObjectToByteArray<T>(T obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// ByteArrayToObject method implementation
        /// </summary>
        private T ByteArrayToObject<T>(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            T obj = (T)binForm.Deserialize(memStream);
            return obj;
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

    /// <summary>
    /// PipeClient class implmentation
    /// </summary>
    public class PipeClient
    {
        private List<string> _servers = new List<string>();

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
        /// DoReloadConfiguration method implementation
        /// </summary>
        public bool DoReloadConfiguration(string requestor, string value)
        {
            try
            {
                if (OnDecrypt == null)
                    OnDecrypt += PipeClientOnDecrypt;
                if (OnEncrypt == null)
                    OnEncrypt += PipeClientOnEncrypt;

                Task<bool>[] taskArray = new Task<bool>[_servers.Count];

                for (int i = 0; i < taskArray.Length; i++)
                {
                    NamedPipeClientStream ClientStream = new NamedPipeClientStream(_servers[i], "adfsmfaconfig", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);

                    taskArray[i] = Task<bool>.Factory.StartNew(() =>
                    {
                        try
                        {
                            ClientStream.Connect();
                            PipeStreamData ss = new PipeStreamData(ClientStream);
                            ss.WriteString(OnEncrypt(Proofkey));
                            if (OnDecrypt(ss.ReadString()) == Proofkey)
                            {
                                NamedPipeReloadConfigRecord xdata = new NamedPipeReloadConfigRecord(requestor, OnEncrypt(value));
                                ss.WriteData(ObjectToByteArray(xdata));
                                return ByteArrayToObject<bool>(ss.ReadData());
                            }
                        }
                        catch (IOException e)
                        {
                            LogForSlots.WriteEntry("PipeClient Error : " + e.Message, EventLogEntryType.Error, 8888);
                            ClientStream.Close();
                        }
                        finally
                        {
                            ClientStream.Close();
                        }
                        return false;
                    });
                }
                Task.WaitAll(taskArray);
                for (int i = 0; i < taskArray.Length; i++)
                {
                    Task<bool> tsk = taskArray[i];
                    if (tsk == null)
                        return false;
                    if (!tsk.Result)
                        return false;
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
        /// DoRequestServerConfiguration method implementation
        /// </summary>
        public NamedPipeRegistryRecord DoRequestServerConfiguration(string requestor)
        {
            try
            {
                if (OnDecrypt == null)
                    OnDecrypt += PipeClientOnDecrypt;
                if (OnEncrypt == null)
                    OnEncrypt += PipeClientOnEncrypt;

                Task<NamedPipeRegistryRecord>task = null;

                NamedPipeClientStream ClientStream = new NamedPipeClientStream(_servers[0], "adfsmfaconfig", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
                task = Task<NamedPipeRegistryRecord>.Factory.StartNew(() =>
                {
                    try
                    {
                        ClientStream.Connect();
                        PipeStreamData ss = new PipeStreamData(ClientStream);
                        ss.WriteString(OnEncrypt(Proofkey));
                        if (OnDecrypt(ss.ReadString()) == Proofkey)
                        {
                            NamedPipeServerConfigRecord xdata = new NamedPipeServerConfigRecord(requestor);
                            ss.WriteData(ObjectToByteArray(xdata));
                            return ByteArrayToObject<NamedPipeRegistryRecord>(ss.ReadData());
                        }
                    }
                    catch (IOException e)
                    {
                        LogForSlots.WriteEntry("PipeClient Error : " + e.Message, EventLogEntryType.Error, 8888);
                        ClientStream.Close();
                    }
                    finally
                    {
                        ClientStream.Close();
                    }
                    return default(NamedPipeRegistryRecord);
                });               
                task.Wait();
                return task.Result;
            }
            catch (Exception e)
            {
                LogForSlots.WriteEntry("PipeClient Error : " + e.Message, EventLogEntryType.Error, 8888);
                return default(NamedPipeRegistryRecord);
            }
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
        /// ObjectToByteArray method implementation
        /// </summary>
        private byte[] ObjectToByteArray<T>(T obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// ByteArrayToObject method implementation
        /// </summary>
        private T ByteArrayToObject<T>(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            T obj = (T)binForm.Deserialize(memStream);
            return obj;
        }
    }

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
    /// NamedPipeRegistryRecord struct implementation
    /// </summary>
    [Serializable]
    public struct NamedPipeRegistryRecord
    {
        public string FQDN;
        public string MachineName;
        public string CurrentVersion;
        public string ProductName;
        public string InstallationType;
        public string NodeType;
        public int CurrentBuild;
        public int CurrentMajorVersionNumber;
        public int CurrentMinorVersionNumber;
        public int BehaviorLevel;
        public DateTime HeartbeatTimestamp;
        public bool IsWindows2012R2;
        public bool IsWindows2016;
        public bool IsWindows2019;
    }


    /// <summary>
    /// NamedPipeReloadConfigRecord class implementation
    /// </summary>
    [Serializable]
    public struct NamedPipeReloadConfigRecord
    {
        public string Message;
        public string Requestor;

        public NamedPipeReloadConfigRecord(string requestor, string message)
        {
            Message = message;
            Requestor = requestor;
        }
    }

    /// <summary>
    /// NamedPipeServerConfigRecord class implementation
    /// </summary>
    [Serializable]
    public struct NamedPipeServerConfigRecord
    {
        public string Requestor;

        public NamedPipeServerConfigRecord(string requestor)
        {
            Requestor = requestor;
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
