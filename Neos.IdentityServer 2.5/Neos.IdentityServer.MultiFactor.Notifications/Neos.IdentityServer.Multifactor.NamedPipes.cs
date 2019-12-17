//******************************************************************************************************************************************************************************************//
// Copyright (c) 2019 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
#define smallsvr
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.IO.Pipes;
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
    public delegate byte[] OnNamedPipeEncryptBytesEvent(byte[] clearvalue);
    public delegate byte[] OnNamedPipeDecryptBytesEvent(byte[] cryptedvalue);

    public delegate bool OnReloadConfigurationEvent(string requestor, string value);
    public delegate NamedPipeRegistryRecord OnRequestServerConfigurationEvent(string requestor);
    public delegate bool OnCheckForReplayEvent(NamedPipeNotificationReplayRecord record);

    /// <summary>
    /// PipeServer class implementation
    /// </summary>
    public class PipeServer
    {
        private NamedPipeServerStream ConfigPipeServer;
#if smallsvr
        private static int numthreads = 1;
#else
        private static int numthreads = 4;
#endif
        private bool MustExit = false;

        public event OnNamedPipeEncryptEvent OnEncrypt;
        public event OnNamedPipeDecryptEvent OnDecrypt;
        public event OnNamedPipeEncryptBytesEvent OnEncryptBytes;
        public event OnNamedPipeDecryptBytesEvent OnDecryptBytes;

        public event OnReloadConfigurationEvent OnReloadConfiguration;
        public event OnRequestServerConfigurationEvent OnRequestServerConfiguration;
        public event OnCheckForReplayEvent OnCheckForReplay;
        public event OnCheckForReplayEvent OnCheckForRemoteReplay;


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
            if (this.OnDecryptBytes == null)
                OnDecryptBytes += PipeServerOnDecryptBytes;
            if (this.OnEncryptBytes == null)
                OnEncryptBytes += PipeServerOnEncryptBytes;

            try
            {
                PipeSecurity pipeSecurity = CreatePipeServerSecurity();
                ConfigPipeServer = new NamedPipeServerStream("adfsmfaconfig", PipeDirection.InOut, numthreads, PipeTransmissionMode.Message, PipeOptions.None, 0x4000, 0x4000, pipeSecurity, HandleInheritability.Inheritable, PipeAccessRights.ChangePermissions);

                Thread[] tasks = new Thread[numthreads];

                for (int i = 0; i < numthreads; i++)
                {
                    tasks[i] = new Thread(PipeServerConfigThread);
                    tasks[i].Start();
                }
                Thread.Sleep(250);
            }
            catch (Exception ex )
            {
                LogForSlots.WriteEntry("PipeServer Start Error : " + ex.Message, EventLogEntryType.Error, 8880);
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
                LogForSlots.WriteEntry("PipeServer Stop Error : " + ex.Message, EventLogEntryType.Error, 8880);
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
            byte[] byt = XORUtilities.XOREncryptOrDecrypt(System.Text.Encoding.UTF8.GetBytes(clearvalue), Proofkey);
            return System.Convert.ToBase64String(byt);
        }

        /// <summary>
        /// PipeServerOnDecrypt method implementation
        /// </summary>
        private string PipeServerOnDecrypt(string cryptedvalue)
        {
            byte[] byt = XORUtilities.XOREncryptOrDecrypt(System.Convert.FromBase64String(cryptedvalue), Proofkey);
            return System.Text.Encoding.UTF8.GetString(byt);
        }

        /// <summary>
        /// PipeServerOnEncryptBytes method implementation
        /// </summary>
        private byte[] PipeServerOnEncryptBytes(byte[] clearvalue)
        {
            return XORUtilities.XOREncryptOrDecrypt(clearvalue, Proofkey);
        }

        /// <summary>
        /// PipeServerOnDecryptBytes method implementation
        /// </summary>
        private byte[] PipeServerOnDecryptBytes(byte[] cryptedvalue)
        {
            return XORUtilities.XOREncryptOrDecrypt(cryptedvalue, Proofkey); 
        }

        /// <summary>
        /// PipeServerConfigThread method implmentation
        /// </summary>
        private void PipeServerConfigThread()
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
                            object obj = ByteArrayToObject<object>(OnDecryptBytes(ss.ReadData()));

                            if (obj is NamedPipeReloadConfigRecord)
                            {
                                NamedPipeReloadConfigRecord encrypted = (NamedPipeReloadConfigRecord)obj;
                                bool b = false;
                                if (OnReloadConfiguration != null)
                                {
                                    b = OnReloadConfiguration(encrypted.Requestor, encrypted.Message);
                                    ss.WriteData(OnEncryptBytes(ObjectToByteArray<bool>(b)));
                                }
                            }
                            else if (obj is NamedPipeServerConfigRecord)
                            {
                                NamedPipeServerConfigRecord encrypted = (NamedPipeServerConfigRecord)obj;
                                NamedPipeRegistryRecord reg;
                                if (OnRequestServerConfiguration != null)
                                {
                                    reg = OnRequestServerConfiguration(encrypted.Requestor);
                                    ss.WriteData(OnEncryptBytes(ObjectToByteArray<NamedPipeRegistryRecord>(reg)));
                                }
                            }
                            else if (obj is NamedPipeNotificationReplayRecord)
                            {
                                NamedPipeNotificationReplayRecord encrypted = (NamedPipeNotificationReplayRecord)obj;
                                bool b = false;
                                if (OnCheckForReplay != null)
                                {
                                    b = OnCheckForReplay(encrypted);
                                    if ((b) && (encrypted.MustDispatch))
                                    {
                                        bool c = false;
                                        if (OnCheckForRemoteReplay != null)
                                        {
                                            c = OnCheckForRemoteReplay(encrypted);
                                            ss.WriteData(OnEncryptBytes(ObjectToByteArray<bool>(c)));
                                        }
                                        else
                                            ss.WriteData(OnEncryptBytes(ObjectToByteArray<bool>(b)));
                                    }
                                    else if ((b) && (!encrypted.MustDispatch))
                                        ss.WriteData(OnEncryptBytes(ObjectToByteArray<bool>(true)));
                                    else
                                        ss.WriteData(OnEncryptBytes(ObjectToByteArray<bool>(false)));
                                }
                            }
                        }
                    }
                    catch (IOException e)
                    {
                        LogForSlots.WriteEntry("PipeServer Error : " + e.Message, EventLogEntryType.Error, 8880);
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
        public event OnNamedPipeEncryptBytesEvent OnEncryptBytes;
        public event OnNamedPipeDecryptBytesEvent OnDecryptBytes;


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
        public PipeClient(string key, List<string> servers, bool removelocal = false) : this(key)
        {
            if (removelocal)
                Servers = servers;
            else
                _servers = servers;
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
                if (OnDecryptBytes == null)
                    OnDecryptBytes += PipeClientOnDecryptBytes;
                if (OnEncryptBytes == null)
                    OnEncryptBytes += PipeClientOnEncryptBytes;

                Task<bool>[] taskArray = new Task<bool>[_servers.Count];

                for (int i = 0; i < taskArray.Length; i++)
                {
                    string servername = _servers[i];
                    taskArray[i] = Task<bool>.Factory.StartNew((svr) =>
                    {
                        NamedPipeClientStream ClientStream = new NamedPipeClientStream(svr.ToString(), "adfsmfaconfig", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
                        try
                        {
                            ClientStream.Connect(3000);
                            PipeStreamData ss = new PipeStreamData(ClientStream);
                            ss.WriteString(OnEncrypt(Proofkey));
                            if (OnDecrypt(ss.ReadString()) == Proofkey)
                            {
                                NamedPipeReloadConfigRecord xdata = new NamedPipeReloadConfigRecord(requestor, value);
                                ss.WriteData(OnEncryptBytes(ObjectToByteArray(xdata)));
                                return ByteArrayToObject<bool>(OnDecryptBytes(ss.ReadData()));
                            }
                        }
                        catch (IOException e)
                        {
                            LogForSlots.WriteEntry("PipeClient Error : " + e.Message, EventLogEntryType.Error, 8810);
                            return false;
                        }
                        catch (TimeoutException e)
                        {
                            LogForSlots.WriteEntry("PipeClient Error : " + e.Message, EventLogEntryType.Error, 8811);
                            return false;
                        }
                        finally
                        {
                            ClientStream.Close();
                        }
                        return false;
                    }, servername);
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
                LogForSlots.WriteEntry("PipeClient Error : " + e.Message, EventLogEntryType.Error, 8889);
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
                if (OnDecryptBytes == null)
                    OnDecryptBytes += PipeClientOnDecryptBytes;
                if (OnEncryptBytes == null)
                    OnEncryptBytes += PipeClientOnEncryptBytes;

                string servername = _servers[0];
                Task<NamedPipeRegistryRecord> task = Task<NamedPipeRegistryRecord>.Factory.StartNew((svr) =>
                {
                    NamedPipeClientStream ClientStream = new NamedPipeClientStream(svr.ToString(), "adfsmfaconfig", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
                    try
                    {
                        ClientStream.Connect(10000);
                        PipeStreamData ss = new PipeStreamData(ClientStream);
                        ss.WriteString(OnEncrypt(Proofkey));
                        if (OnDecrypt(ss.ReadString()) == Proofkey)
                        {
                            NamedPipeServerConfigRecord xdata = new NamedPipeServerConfigRecord(requestor);
                            ss.WriteData(OnEncryptBytes(ObjectToByteArray(xdata)));
                            return ByteArrayToObject<NamedPipeRegistryRecord>(OnDecryptBytes(ss.ReadData()));
                        }
                    }
                    catch (IOException e)
                    {
                        LogForSlots.WriteEntry("PipeClient Error : " + e.Message, EventLogEntryType.Error, 8870);
                    }
                    catch (TimeoutException e)
                    {
                        LogForSlots.WriteEntry("PipeClient Error : " + e.Message, EventLogEntryType.Error, 8871);
                    }
                    finally
                    {
                        ClientStream.Close();
                    }
                    return default(NamedPipeRegistryRecord);
                }, servername);               
                task.Wait();
                return task.Result;
            }
            catch (Exception e)
            {
                LogForSlots.WriteEntry("PipeClient Error : " + e.Message, EventLogEntryType.Error, 8889);
                return default(NamedPipeRegistryRecord);
            }
        }

        /// <summary>
        /// DoCheckForReplay method implementation
        /// </summary>
        public bool DoCheckForReplay(NamedPipeReplayRecord requestor)
        {
            try
            {
                if (OnDecrypt == null)
                    OnDecrypt += PipeClientOnDecrypt;
                if (OnEncrypt == null)
                    OnEncrypt += PipeClientOnEncrypt;
                if (OnDecryptBytes == null)
                    OnDecryptBytes += PipeClientOnDecryptBytes;
                if (OnEncryptBytes == null)
                    OnEncryptBytes += PipeClientOnEncryptBytes;

                string servername = _servers[0];
                Task<bool> task = Task<bool>.Factory.StartNew((svr) =>
                {
                    NamedPipeClientStream ClientStream = new NamedPipeClientStream(svr.ToString(), "adfsmfaconfig", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
                    try
                    {
                        ClientStream.Connect(3000);
                        PipeStreamData ss = new PipeStreamData(ClientStream);
                        ss.WriteString(OnEncrypt(Proofkey));
                        if (OnDecrypt(ss.ReadString()) == Proofkey)
                        {
                            NamedPipeNotificationReplayRecord xdata = new NamedPipeNotificationReplayRecord(requestor);
                            ss.WriteData(OnEncryptBytes(ObjectToByteArray(xdata)));
                            return ByteArrayToObject<bool>(OnDecryptBytes(ss.ReadData()));
                        }
                    }
                    catch (IOException e)
                    {
                        LogForSlots.WriteEntry("PipeClient Error : " + e.Message, EventLogEntryType.Error, 8881);
                        return true;
                    }
                    catch (TimeoutException e)
                    {
                        LogForSlots.WriteEntry("PipeClient Error : " + e.Message, EventLogEntryType.Error, 8882);
                        return true;
                    }
                    finally
                    {
                        ClientStream.Close();
                    }
                    return false;
                }, servername);
                task.Wait();
                return task.Result;
            }
            catch (Exception e)
            {
                LogForSlots.WriteEntry("PipeClient Error : " + e.Message, EventLogEntryType.Error, 8889);
                return false;
            }
        }

        /// <summary>
        /// DoCheckForRemoteReplay method implementation
        /// </summary>
        public bool DoCheckForRemoteReplay(NamedPipeNotificationReplayRecord record)
        {
            try
            {
                if (_servers.Count > 0)
                {
                    if (OnDecrypt == null)
                        OnDecrypt += PipeClientOnDecrypt;
                    if (OnEncrypt == null)
                        OnEncrypt += PipeClientOnEncrypt;
                    if (OnDecryptBytes == null)
                        OnDecryptBytes += PipeClientOnDecryptBytes;
                    if (OnEncryptBytes == null)
                        OnEncryptBytes += PipeClientOnEncryptBytes;

                    record.MustDispatch = false;

                    Task<bool>[] tasks = new Task<bool>[_servers.Count];
                    for (int i = 0; i <= _servers.Count-1; i++)
                    {
                        string servername = _servers[i];
                        tasks[i] = Task<bool>.Factory.StartNew((svr) =>
                        {
                            NamedPipeClientStream ClientStream = new NamedPipeClientStream(svr.ToString(), "adfsmfaconfig", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
                            try
                            {
                                ClientStream.Connect(10000);
                                PipeStreamData ss = new PipeStreamData(ClientStream);
                                ss.WriteString(OnEncrypt(Proofkey));
                                if (OnDecrypt(ss.ReadString()) == Proofkey)
                                {
                                    ss.WriteData(OnEncryptBytes(ObjectToByteArray(record)));
                                    return ByteArrayToObject<bool>(OnDecryptBytes(ss.ReadData()));
                                }
                            }
                            catch (IOException e)
                            {
                                LogForSlots.WriteEntry("PipeClient Error : " + e.Message, EventLogEntryType.Error, 8861);
                                return true;
                            }
                            catch (TimeoutException e)
                            {
                                LogForSlots.WriteEntry("PipeClient Error : " + e.Message, EventLogEntryType.Error, 8862);
                                return true;
                            }
                            finally
                            {
                                ClientStream.Close();
                            }
                            return false;
                        }, servername);
                    }
                    Task.WaitAll(tasks);
                    foreach (Task<bool> ts in tasks)
                    {
                        if (!ts.Result)
                            return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                LogForSlots.WriteEntry("PipeClient Error : " + e.Message, EventLogEntryType.Error, 8889);
                return false;
            }
        }


        /// <summary>
        /// PipeClientOnEncrypt method implementation
        /// </summary>
        private string PipeClientOnEncrypt(string clearvalue)
        {
            byte[] byt = XORUtilities.XOREncryptOrDecrypt(System.Text.Encoding.UTF8.GetBytes(clearvalue), Proofkey);
            return System.Convert.ToBase64String(byt);
        }

        /// <summary>
        /// PipeClientOnDecrypt method implementation
        /// </summary>
        private string PipeClientOnDecrypt(string cryptedvalue)
        {
            byte[] byt = XORUtilities.XOREncryptOrDecrypt(System.Convert.FromBase64String(cryptedvalue), Proofkey);
            return System.Text.Encoding.UTF8.GetString(byt);
        }

        /// <summary>
        /// PipeClientOnDecryptBytes method implementation
        /// </summary>
        private byte[] PipeClientOnDecryptBytes(byte[] cryptedvalue)
        {
            return XORUtilities.XOREncryptOrDecrypt(cryptedvalue, Proofkey);
        }

        /// <summary>
        /// PipeClientOnEncryptBytes method implementation
        /// </summary>
        private byte[] PipeClientOnEncryptBytes(byte[] clearvalue)
        {
            return XORUtilities.XOREncryptOrDecrypt(clearvalue, Proofkey);
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
    [Serializable]
    public struct NamedPipeReplayRecord
    {
        public bool MustDispatch;
        public int Totp;
        public ReplayLevel ReplayLevel;
        public string UserIPAdress;
        public string UserName;
        public DateTime UserLogon;
        public int DeliveryWindow;
    }

    /// <summary>
    /// NamedPipeNotificationReplayRecord class implementation
    /// </summary>
    [Serializable]
    public struct NamedPipeNotificationReplayRecord
    {
        public bool MustDispatch;
        public int Totp;
        public ReplayLevel ReplayLevel;
        public string UserIPAdress;
        public string UserName;
        public DateTime UserLogon;
        public int DeliveryWindow;

        public NamedPipeNotificationReplayRecord(NamedPipeReplayRecord record)
        {
            MustDispatch = record.MustDispatch;
            Totp = record.Totp;
            ReplayLevel = record.ReplayLevel;
            UserIPAdress = record.UserIPAdress;
            UserLogon = record.UserLogon;
            UserName = record.UserName;
            DeliveryWindow = record.DeliveryWindow;
        }
    }
}
