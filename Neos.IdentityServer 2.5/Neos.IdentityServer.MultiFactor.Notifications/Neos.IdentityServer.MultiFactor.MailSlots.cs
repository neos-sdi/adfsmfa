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
// This source is subject to the Microsoft Public License.                                                                                                                                  // 
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.                                                                                                                             //
// All other rights reserved.                                                                                                                                                               //
//                                                                                                                                                                                          //
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,                                                                                                              // 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED                                                                                                                    //
// WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.                                                                                                                   //
//******************************************************************************************************************************************************************************************//
#define fastserialize
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor
{
    #region BaseMailSlotServer
    public abstract class BaseMailSlotServer<T> : IDisposable
    {
        internal const int MAILSLOT_WAIT_FOREVER = -1;
        internal const int MAILSLOT_NO_MESSAGE = -1;
        private SafeMailslotHandle _mailslot = null;
        private SECURITY_ATTRIBUTES _securityattr = null;
        private List<T> _messages = new List<T>();
        private CancellationTokenSource _canceltokensource = null;
        private readonly string _appname = string.Empty;
        private readonly int _processid = 0;
        private readonly int _scanduration = 300;
        private bool _multiinstance = true;
        private bool _allowtoself = false;
        private Task _task = null;
        private bool _isstarted;
        
        /// <summary>
        /// constructor implementation
        /// </summary>
        public BaseMailSlotServer(bool multiinstance = false, int scanduration = 3000)
        {
            Process process = Process.GetCurrentProcess();
            _processid = process.Id;
            _appname = "MGR";
            _scanduration = scanduration;
            _multiinstance = multiinstance;
        }

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public BaseMailSlotServer(string appname, bool multiinstance = true, int scanduration = 3000):this(multiinstance, scanduration)
        {
            _appname = appname;
        }

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public BaseMailSlotServer(string appname, int processid, bool multiinstance = true, int scanduration = 3000):this(appname, multiinstance, scanduration)
        {
            _processid = processid;
        }

        /// <summary>
        /// Destructor implementation
        /// </summary>
        ~BaseMailSlotServer()
        {
            Dispose(true);
        }

        /// <summary>
        /// CreateMailSlot() method implmentation
        /// </summary>
        protected virtual void CreateMailSlot()
        {
            _securityattr = CreateMailslotSecurity();
            _mailslot = NativeMethod.CreateMailslot(this.MailSlotName, 0, MAILSLOT_WAIT_FOREVER, _securityattr);
            if (_mailslot.IsInvalid)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// CloseMailSlot() method implmentation
        /// </summary>
        protected virtual void CloseMailSlot()
        {
            if (_mailslot != null)
            {
                _mailslot.Close();
                _mailslot = null;
            }
            if (_securityattr != null)
                _securityattr = null;
        }

        /// <summary>
        /// ApplicationName property
        /// </summary>
        public string ApplicationName
        {
            get { return _appname; }
        }

        /// <summary>
        /// ProcessID property
        /// </summary>
        public int ProcessID
        {
            get { return _processid; }
        }

        /// <summary>
        /// ScanDuration property
        /// </summary>
        public int ScanDuration
        {
            get { return _scanduration; }
        }

        /// <summary>
        /// MailSlotName property
        /// </summary>
        protected abstract string MailSlotName
        {
            get;
        }

        /// <summary>
        /// MultiInstance property
        /// </summary>
        public bool MultiInstance
        {
            get { return _multiinstance; }
            set 
            {
                if (_mailslot==null)
                    _multiinstance = value; 
            }
        }

        /// <summary>
        /// AllowToSelf property
        /// </summary>
        public bool AllowToSelf
        {
            get { return _allowtoself; }
            set 
            {
                if (_mailslot == null)
                    _allowtoself = value; 
            }
        }

        /// <summary>
        /// Start method implentation
        /// </summary>
        public virtual void Start()
        {
            _canceltokensource = new CancellationTokenSource();
            CreateMailSlot();
            _task = new Task(() => ReadMailSlots(), _canceltokensource.Token, TaskCreationOptions.LongRunning);
            _task.Start();
            _isstarted = true;
        }

        /// <summary>
        /// Stop method implementation
        /// </summary>
        public virtual void Stop()
        {
            _isstarted = false;
            _canceltokensource.Cancel(false);
            CloseMailSlot();
        }

        /// <summary>
        /// IsStarted property implementation
        /// </summary>
        public bool IsStarted
        {
            get { return _isstarted; }
        }

        /// <summary>
        /// ReadMailSlots method implementation
        /// </summary>
        private void ReadMailSlots()
        {
            while (_mailslot != null)
            {
                try
                {
                    if (_canceltokensource.Token.IsCancellationRequested)
                        return;
                    _messages.Clear();
                    if (InternalReadMailslot())
                    {
                        foreach (T message in _messages)
                        {
                            if (!_canceltokensource.Token.IsCancellationRequested)
                            {
                                DoOnMessage(message);
                            }
                        }
                    }
                    if (!_canceltokensource.Token.IsCancellationRequested)
                        Thread.Sleep(_scanduration);
                }
                catch (ThreadAbortException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    LogForSlots.WriteEntry("MailSlot Server Loop Error ("+ApplicationName+"): "+ex.Message, EventLogEntryType.Error, 9999);
                }
            }
        }

        /// <summary>
        /// InternalReadMailslot method implementation
        /// </summary>
        private bool InternalReadMailslot()
        {
            int cbBytesRead = 0;
            int nMessageId = 0;

            bool succeeded = false;
            if (_canceltokensource.Token.IsCancellationRequested)
                return succeeded;

            _messages.Clear();
            try
            {
                succeeded = NativeMethod.GetMailslotInfo(_mailslot, IntPtr.Zero, out int cbMessageBytes, out int cMessages, IntPtr.Zero);
                if (!succeeded)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                if (cbMessageBytes == MAILSLOT_NO_MESSAGE)
                    return succeeded;
                while (cMessages != 0)
                {
                    nMessageId++;
                    byte[] bBuffer = new byte[cbMessageBytes];
                    succeeded = NativeMethod.ReadFile(_mailslot, bBuffer, cbMessageBytes, out cbBytesRead, IntPtr.Zero);
                    if (!succeeded)
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    if (bBuffer[0] == 0xFF)
                    {
                        RawSerializer<MailSlotDispatcherMessageStruct> bf = new RawSerializer<MailSlotDispatcherMessageStruct>();
                        dynamic flatted = bf.Deserialize(bBuffer);
                        _messages.Add(flatted);

                    }
                    if (bBuffer[0] == 0xEE)
                    {
                        RawSerializer<MailSlotMessageStruct> bf = new RawSerializer<MailSlotMessageStruct>();
                        dynamic flatted = bf.Deserialize(bBuffer);
                        _messages.Add(flatted);
                    }

                    succeeded = NativeMethod.GetMailslotInfo(_mailslot, IntPtr.Zero, out cbMessageBytes, out cMessages, IntPtr.Zero);
                    if (!succeeded)
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            catch (Exception ex)
            {
                LogForSlots.WriteEntry("MailSlot Server Read Error (" + ApplicationName + "): " + ex.Message, EventLogEntryType.Error, 9999);
                throw ex;
            }
            return succeeded;
        }

        protected abstract void DoOnMessage(T Message);

        /// <summary>
        /// Dispose IDispose method implementation
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose method implementation
        /// </summary>
        internal virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_canceltokensource.IsCancellationRequested)
                    _canceltokensource.Cancel();
                CloseMailSlot();
            }
        }

        /// <summary>
        /// CreateMailslotSecurity() implementation
        /// </summary>
        /// <see cref="http://msdn.microsoft.com/en-us/library/aa365600.aspx"/>
        private SECURITY_ATTRIBUTES CreateMailslotSecurity()
        {
            // Define the SDDL for the security descriptor.
            string sddl = "D:" +           // Discretionary ACL
                "(A;OICI;GRGW;;;AU)" +     // Allow read/write to authenticated users
                "(A;OICI;GA;;;BA)" +       // Allow full control to administrators
                "(A;OICI;GA;;;SY)" +       // Allow full control to local system
                "(A;OICI;GA;;;LS)" +       // Allow full control to local service
                "(A;OICI;GA;;;DA)" +       // Allow full control to domain admins
                "(A;OICI;GA;;;EA)" +       // Allow full control to enterprise admins
                "(A;OICI;GA;;;NS)";        // Allow full control to network services


            if (!NativeMethod.ConvertStringSecurityDescriptorToSecurityDescriptor(sddl, 1, out SafeLocalMemHandle pSecurityDescriptor, IntPtr.Zero))
            {
                throw new Win32Exception();
            }
            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
            sa.nLength = Marshal.SizeOf(sa);
            sa.lpSecurityDescriptor = pSecurityDescriptor;
            sa.bInheritHandle = false;
            return sa;
        }
    }
    #endregion

    #region Server Mailslots
    #region MailSlotServer
    public class MailSlotServer : BaseMailSlotServer<MailSlotMessage>
    {
        public delegate void MailSlotMessageEvent(MailSlotServer maislotserver, MailSlotMessage message);
        public event MailSlotMessageEvent MailSlotMessageArrived;

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public MailSlotServer(string appname, bool multiinstance = true, int scanduration = 3000): base(appname, multiinstance, scanduration)
        {
        }

        /// <summary>
        /// MailSlotName property
        /// </summary>
        protected override string MailSlotName
        {
            get { return @"\\.\mailslot\mfa\cfg_" + this.ApplicationName + @"_" + this.ProcessID.ToString(); }
        }

        /// <summary>
        /// StartMethod implmentation
        /// </summary>
        public override void Start()
        {
            using (MailSlotClientDispatcher mailslot = new MailSlotClientDispatcher())
            {
                mailslot.Register(this.ApplicationName, this.ProcessID, this.MultiInstance, this.AllowToSelf);
            }
            base.Start();
        }

        /// <summary>
        /// Stop() method implementation
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            using (MailSlotClientDispatcher mailslot = new MailSlotClientDispatcher())
            {
                mailslot.UnRegister(this.ApplicationName, this.ProcessID, this.MultiInstance, this.AllowToSelf);
            }
        }

        /// <summary>
        /// DoOnMessage method implmentation
        /// </summary>
        protected override void DoOnMessage(MailSlotMessage message)
        {
            try
            {
                switch (message.Operation)
                {
                    case 0xFE:
                        if (IsStarted)
                            Stop();
                        Start();
                        this.MailSlotMessageArrived(this, message);
                        break;
                    default:
                        this.MailSlotMessageArrived(this, message);
                        break;
                }
            }
            catch (Exception ex)
            {
                LogForSlots.WriteEntry("MailSlot Server Error : "+ex.Message, EventLogEntryType.Error, 9999);
            }
        }
       
        /// <summary>
        /// Dispose method implementation
        /// </summary>
        internal override void Dispose(bool disposing)
        {
            if (disposing)
            {
                using (MailSlotClientDispatcher mailslot = new MailSlotClientDispatcher())
                {
                    mailslot.UnRegister(this.ApplicationName, this.ProcessID, this.MultiInstance, this.AllowToSelf);
                }
            }
            base.Dispose(disposing);
        }
    }
    #endregion

    #region MailSlotServerDispatcher
    public class MailSlotServerDispatcher : BaseMailSlotServer<MailSlotMessage>
    {
        private List<MailSlotDispatcherMessage> _instances = new List<MailSlotDispatcherMessage>();
        private MailSlotServerManager _manager;

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public MailSlotServerDispatcher(MailSlotServerManager manager, string appname, int processid, int scanduration = 3000) : base(appname, processid, true, scanduration) 
        {
            _manager = manager;
        } 

        /// <summary>
        /// MailSlotName property
        /// </summary>
        protected override string MailSlotName
        {
            get { return @"\\.\mailslot\mfa\cfg_" + ApplicationName; }
        }

        /// <summary>
        /// DoOnMessage method implmentation
        /// </summary>
        protected override void DoOnMessage(MailSlotMessage message)
        {
            if (message.IsBroadcast)
            {
                foreach(MailSlotServerDispatcher disp in _manager.MailslotList)
                {
                    if (!disp.ApplicationName.ToUpper().Equals("BDC"))
                    {
                        foreach (MailSlotDispatcherMessage msg in disp._instances)
                        {
                            if (message.ApplicationName.ToLower().Equals(msg.ApplicationName.ToLower()) || message.IsBroadcast)
                            {
                                using (MailSlotClientForwarder mailslotcli = new MailSlotClientForwarder(msg.ApplicationName, msg.TargetID))
                                {
                                    if (!(message.SenderID == msg.TargetID))
                                        mailslotcli.ForwardNotification(message);
                                    else if ((msg.AllowToSelf) && (message.SenderID == msg.TargetID))
                                        mailslotcli.ForwardNotification(message);
                                }
                            }
                        }
                   }
                }
            }
            else
            {
                foreach (MailSlotDispatcherMessage msg in _instances)
                {
                    if (message.ApplicationName.ToLower().Equals(msg.ApplicationName.ToLower()))
                    {
                        using (MailSlotClientForwarder mailslotcli = new MailSlotClientForwarder(msg.ApplicationName, msg.TargetID))
                        {
                            if (!(message.SenderID == msg.TargetID))
                                mailslotcli.ForwardNotification(message);
                            else if ((msg.AllowToSelf) && (message.SenderID == msg.TargetID))
                                mailslotcli.ForwardNotification(message);
                        }
                    }
                }
            }
        }


        public List<MailSlotDispatcherMessage> Instances
        {
            get { return _instances; }
        }


        /// <summary>
        /// AddInstance method implmentation
        /// </summary>
        public int AddInstance(MailSlotDispatcherMessage message)
        {
            bool Ok = true;
            foreach(MailSlotDispatcherMessage msg in _instances)
            {
                if ((!message.MultiInstance) && (message.ApplicationName.ToLower().Equals(msg.ApplicationName.ToLower())))
                {
                    Ok = false;
                    break;
                }
                if (message.ApplicationName.ToLower().Equals(msg.ApplicationName.ToLower()) && (message.TargetID == msg.TargetID)) 
                {
                    Ok = false;
                    break;
                }
            }
            if (Ok)
                _instances.Add(message);
            return _instances.Count;
        }

        /// <summary>
        /// RemoveInstance method implmentation
        /// </summary>
        public int RemoveInstance(MailSlotDispatcherMessage message)
        {
            _instances.RemoveAll(C => message.ApplicationName.ToLower().Equals(C.ApplicationName.ToLower()) && (message.TargetID == C.TargetID));
            return _instances.Count;
        }

        /// <summary>
        /// Dispose method implementation
        /// </summary>
        internal override void Dispose(bool disposing)
        {
            if (disposing)
            {
                using (MailSlotClientDispatcher mailslot = new MailSlotClientDispatcher())
                {
                    mailslot.UnRegister(this.ApplicationName, this.ProcessID, this.MultiInstance, this.AllowToSelf);
                }
            }
            base.Dispose(disposing);
        }
    }
    #endregion

    #region MailSlotServerManager
    public class MailSlotServerManager : BaseMailSlotServer<MailSlotDispatcherMessage>
    {
        private List<MailSlotServerDispatcher> _mailslotlst = new List<MailSlotServerDispatcher>();
        private MailSlotServer _broadcast = null;
        public delegate void MailSlotSystemEvent(MailSlotServerDispatcher mailslotserver);
        public event MailSlotSystemEvent MailSlotSystemMessageArrived;


        /// <summary>
        /// Constructor implementation
        /// </summary>
        public MailSlotServerManager(int scanduration = 3000): base(false, scanduration) 
        {
            _broadcast = new MailSlotServer("BDC", false, scanduration);
        }

        /// <summary>
        /// Start method implentation
        /// </summary>
        public override void Start()
        {
            base.Start();
            _broadcast.Start();
        }

        /// <summary>
        /// Start method implentation
        /// </summary>
        public override void Stop()
        {
            _broadcast.Stop();
            base.Stop();
        }

        /// <summary>
        /// Dispose method implementation
        /// </summary>
        internal override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach(MailSlotServerDispatcher disp in this._mailslotlst)
                {
                    disp.Dispose();
                }
                _broadcast.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// MailslotList property
        /// </summary>
        public List<MailSlotServerDispatcher> MailslotList
        {
            get { return _mailslotlst; }
        }

        /// <summary>
        /// MailSlotName property implementation
        /// </summary>
        protected override string MailSlotName
        {
            get { return @"\\.\mailslot\mfa\dispatcher"; }
        }

        /// <summary>
        /// DoOnMessage method implmentation
        /// </summary>
        protected override void DoOnMessage(MailSlotDispatcherMessage message)
        {
            MailSlotServerDispatcher mdisp = null;
            switch (message.Operation)
            {
                case 0xF0:
                {
                    int index = this._mailslotlst.FindIndex(c => (c.ApplicationName.Equals(message.ApplicationName)));
                    if (index < 0)
                    {
                        mdisp = new MailSlotServerDispatcher(this, message.ApplicationName, this.ProcessID);
                        _mailslotlst.Add(mdisp);
                        mdisp.Start();
                    }
                    else
                        mdisp = _mailslotlst[index];
                    mdisp.AddInstance(message);
                    break;
                }
                case 0xF1:
                {
                    int index = this._mailslotlst.FindIndex(c => (c.ApplicationName.Equals(message.ApplicationName)));
                    if (index >= 0)
                    {
                        mdisp = _mailslotlst[index];

                        if (mdisp.RemoveInstance(message) <= 0)
                        {
                            mdisp.Stop();
                            _mailslotlst.Remove(mdisp);
                        }
                    }
                    break;
                }
                case 0xFF:
                {
                    DumpMailSlotServers(message);
                    break;
                }
            }
        }

        /// <summary>
        /// DumpMailSlotServers method implementation
        /// </summary>
        private void DumpMailSlotServers(MailSlotDispatcherMessage message)
        {
            if (message.Operation == 0xFF)
            {
                foreach (MailSlotServerDispatcher disp in _mailslotlst)
                {
                    MailSlotSystemMessageArrived(disp);
                }
            }
        }
    }
    #endregion
    #endregion

    #region Client MailSlots
    #region MailSlotClient
    public abstract class BaseMailSlotClient<T> : IDisposable
    {
        private SafeMailslotHandle _mailslot = null;
        private string _appname = string.Empty;
        private string _machinename = string.Empty;
        private int _processid = 0;

        public BaseMailSlotClient()
        {
            Process process = Process.GetCurrentProcess();
            MachineName = Environment.MachineName;
            ProcessID = process.Id;
        }

        /// <summary>
        /// CreateMailSlot method implementation
        /// </summary>
        public virtual void CreateMailSlot()
        {
            _mailslot = NativeMethod.CreateFile(MailSlotName, FileDesiredAccess.GENERIC_WRITE, FileShareMode.FILE_SHARE_READ, IntPtr.Zero, FileCreationDisposition.OPEN_EXISTING, 0, IntPtr.Zero);
            if (_mailslot.IsInvalid)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// CreateMailSlot method implementation
        /// </summary>
        public virtual void CloseMailSlot()
        {
            if (_mailslot != null)
            {
                _mailslot.Close();
                _mailslot = null;
            }
        }

        /// <summary>
        /// ApplicationName property 
        /// </summary>
        public string ApplicationName
        {
            get { return _appname; }
            set { _appname = value; }
        }

        /// <summary>
        /// MachineName property
        /// </summary>
        public string MachineName
        {
            get { return _machinename; }
            set { _machinename = value; }
        }

        /// <summary>
        /// ProcessID property
        /// </summary>
        public int ProcessID
        {
            get { return _processid; }
            set { _processid = value; }
        }

        /// <summary>
        /// MailSlotName property
        /// </summary>
        protected abstract string MailSlotName { get; }

        /// <summary>
        /// GetMessage method implementation
        /// </summary>
        protected abstract byte[] GetMessage(T message);

        /// <summary>
        /// WriteMailslot method implementation
        /// </summary>
        protected virtual void WriteMailslot(T message)
        {
            int cbMessageBytes = 0;
            int cbBytesWritten = 0;
            byte[] bMessage = GetMessage(message);
            cbMessageBytes = bMessage.Length;
            try
            {
                CreateMailSlot();
                bool succeeded = NativeMethod.WriteFile(_mailslot, bMessage, cbMessageBytes, out cbBytesWritten, IntPtr.Zero);
                if (!succeeded || cbMessageBytes != cbBytesWritten)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            catch (Exception ex)
            {
                LogForSlots.WriteEntry("MailSlot Client Write Error ("+ ApplicationName +") :" + ex.Message, EventLogEntryType.Error, 9999);
            }
            finally
            {
                CloseMailSlot();
            }
        }

        /// <summary>
        /// Dispose implmentation
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose method implementation
        /// </summary>
        internal virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CloseMailSlot();
            }
        }
    }

    /// <summary>
    /// MailSlotClient class
    /// </summary>
    public class MailSlotClient : BaseMailSlotClient<MailSlotMessage>
    {
        private string _text = string.Empty;

        /// <summary>
        /// Constructor implmentation
        /// </summary>
        public MailSlotClient(string appname = null):base()
        {
            if (appname == null)
                appname = "BDC";
            if (appname.ToUpper().Equals("MGR"))
               throw new ArgumentException("parameter cannot be equal to MGR or BDC", appname);
            ApplicationName = appname.ToUpper();
        }

        /// <summary>
        /// ApplicationName property 
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        /// <summary>
        /// MailSlotName property
        /// </summary>
        protected override string MailSlotName
        {
            get 
            {
                return @"\\.\mailslot\mfa\cfg_" + ApplicationName;
            } 
        }

        /// <summary>
        /// GetMessage method implementation
        /// </summary>
        protected override byte[] GetMessage(MailSlotMessage message)
        {
            return message;
        }

        /// <summary>
        /// SendNotification method implmentation
        /// </summary>
        public virtual void SendNotification(NotificationsKind operation = 0)
        {
            MailSlotMessage message = new MailSlotMessage
            {
                Operation = (byte)operation,
                ApplicationName = this.ApplicationName,
                TimeStamp = DateTime.Now,
                SenderID = ProcessID,
                Text = Text
            };
            WriteMailslot(message);
        }
    }
    #endregion

    #region MailSlotClientDispatcher
    public class MailSlotClientDispatcher : BaseMailSlotClient<MailSlotDispatcherMessage>
    {
        /// <summary>
        /// MailSlotName property
        /// </summary>
        protected override string MailSlotName
        {
            get 
            { 
                return @"\\.\mailslot\mfa\dispatcher";
            }
        }

        /// <summary>
        /// GetMessage method implementation
        /// </summary>
        protected override byte[] GetMessage(MailSlotDispatcherMessage message)
        {
            return message;
        }

        /// <summary>
        /// Register method implementation
        /// </summary>
        public void Register(string application, int processid, bool multiinstance, bool allowtoself)
        {
            MailSlotDispatcherMessage msg = new MailSlotDispatcherMessage
            {
                ApplicationName = application,
                TargetID = processid,
                Operation = 0xF0,
                MultiInstance = multiinstance,
                AllowToSelf = allowtoself
            };
            WriteMailslot(msg);
        }

        /// <summary>
        /// UnRegister method implmentation
        /// </summary>
        public void UnRegister(string application, int processid, bool multiinstance, bool allowtoself)
        {
            MailSlotDispatcherMessage msg = new MailSlotDispatcherMessage
            {
                ApplicationName = application,
                TargetID = processid,
                Operation = 0xF1,
                MultiInstance = multiinstance,
                AllowToSelf = allowtoself
            };
            WriteMailslot(msg);
        }

        /// <summary>
        /// SendNotification
        /// </summary>
        public virtual void SendNotification(byte operation = 0)
        {
            MailSlotDispatcherMessage message = new MailSlotDispatcherMessage
            {
                Operation = operation,
                ApplicationName = "MGR", // this.ApplicationName;
                MultiInstance = false,
                AllowToSelf = false
            };
            WriteMailslot(message);
        }

    }
    #endregion

    #region MailSlotClientForwarder
    internal class MailSlotClientForwarder : BaseMailSlotClient<MailSlotMessage>
    {

        /// <summary>
        /// MailSlotClientForwarder constructor
        /// </summary>
        public MailSlotClientForwarder(string appname, int processid)
        {
            ApplicationName = appname;
            ProcessID = processid;
        }

        /// <summary>
        /// MailSlotName property
        /// </summary>
        protected override string MailSlotName
        {
            get 
            {
                return @"\\.\mailslot\mfa\cfg_" + ApplicationName + @"_" + ProcessID.ToString();
            }
        }

        /// <summary>
        /// GetMessage method implementation
        /// </summary>
        protected override byte[] GetMessage(MailSlotMessage message)
        {
            return message;
        }

        /// <summary>
        /// SendNotification method implmentation
        /// </summary>
        public void ForwardNotification(MailSlotMessage message)
        {
            WriteMailslot(message);
        }
    }
    #endregion

    #endregion

    #region MailSlot Messages
    public class BaseMailSlotMessage<T>
    {
        private string _appname;

        /// <summary>
        /// Operation property implmentation
        /// </summary>
        public byte Operation { get; set; }

        /// <summary>
        /// ApplicationName property implementation
        /// </summary>
        public string ApplicationName
        {
            get
            {
                if (_appname.Length > 3)
                    return _appname.Substring(0, 3);
                else
                    return _appname.Substring(0, _appname.Length);
            }
            set
            {
                if (value != null)
                {
                    if (value.Length > 3)
                        _appname = value.Substring(0, 3);
                    else
                        _appname = value.Substring(0, value.Length);
                }
                else
                    _appname = string.Empty;
            }
        }

    }

    /// <summary>
    /// MailSlotDispMessage class
    /// </summary>
    public class MailSlotDispatcherMessage : BaseMailSlotMessage<MailSlotDispatcherMessage>
    {
        /// <summary>
        /// implicit conversion to byte array
        /// </summary>
        public static implicit operator byte[](MailSlotDispatcherMessage data)
        {
            MailSlotDispatcherMessageStruct flatted = data;
            flatted.MsgType = 0xFF;
            RawSerializer<MailSlotDispatcherMessageStruct> bf = new RawSerializer<MailSlotDispatcherMessageStruct>();
            return bf.Serialize(flatted);
        }

        /// <summary>
        /// implicit conversion from byte array
        /// </summary>
        public static implicit operator MailSlotDispatcherMessage(byte[] data)
        {
            if (data == null)
                return null;
            RawSerializer<MailSlotDispatcherMessageStruct> bf = new RawSerializer<MailSlotDispatcherMessageStruct>();
            return bf.Deserialize(data);
        }

        /// <summary>
        /// implicit conversion to MailSlotDispMessageStruct
        /// </summary>
        public static implicit operator MailSlotDispatcherMessageStruct(MailSlotDispatcherMessage data)
        {
            MailSlotDispatcherMessageStruct flatted = default(MailSlotDispatcherMessageStruct);
            flatted.MsgType = 0xFF;
            flatted.Operation = data.Operation;
            flatted.TargetID = data.TargetID;
            flatted.MultiInstance = data.MultiInstance;
            flatted.AllowToSelf = data.AllowToSelf;
            flatted.ApplicationName = data.ApplicationName;
            return flatted;
        }

        /// <summary>
        /// implicit conversion from MailSlotDispMessageStruct
        /// </summary>
        public static implicit operator MailSlotDispatcherMessage(MailSlotDispatcherMessageStruct data)
        {
            MailSlotDispatcherMessage msg = new MailSlotDispatcherMessage
            {
                Operation = data.Operation,
                TargetID = data.TargetID,
                MultiInstance = data.MultiInstance,
                AllowToSelf = data.AllowToSelf,
                ApplicationName = data.ApplicationName
            };
            return msg;
        }

        public int TargetID { get; set; }
        public bool MultiInstance { get; set; }
        public bool AllowToSelf { get; set; }
    }

    /// <summary>
    /// MailSlotMessage class
    /// </summary>
    public class MailSlotMessage : BaseMailSlotMessage<MailSlotMessage>
    {
        private string _text;

        /// <summary>
        /// implicit conversion to byte array
        /// </summary>
        public static implicit operator byte[](MailSlotMessage data)
        {
            MailSlotMessageStruct flatted = data;
            flatted.MsgType = 0xEE;
            RawSerializer<MailSlotMessageStruct> bf = new RawSerializer<MailSlotMessageStruct>();
            return bf.Serialize(flatted);
        }

        /// <summary>
        /// implicit conversion from ResultNode
        /// </summary>
        public static implicit operator MailSlotMessage(byte[] data)
        {
            if (data == null)
                return null;
            RawSerializer<MailSlotMessageStruct> bf = new RawSerializer<MailSlotMessageStruct>();
            return bf.Deserialize(data);
        }

        /// <summary>
        /// implicit conversion to MailSlotMessageStruct
        /// </summary>
        public static implicit operator MailSlotMessageStruct(MailSlotMessage data)
        {
            MailSlotMessageStruct flatted = default(MailSlotMessageStruct);
            flatted.MsgType = 0xEE;
            flatted.Operation = data.Operation;
            flatted.SenderID = data.SenderID;
            flatted.TimeStamp = data.TimeStamp;
            flatted.ApplicationName = data.ApplicationName;
            flatted.Text = data.Text;
            return flatted;
        }

        /// <summary>
        /// implicit conversion from MailSlotMessageStruct
        /// </summary>
        public static implicit operator MailSlotMessage(MailSlotMessageStruct data)
        {
            MailSlotMessage msg = new MailSlotMessage
            {
                Operation = data.Operation,
                SenderID = data.SenderID,
                TimeStamp = data.TimeStamp,
                ApplicationName = data.ApplicationName,
                Text = data.Text
            };
            return msg;
        }

        public int SenderID { get; set; }

        public DateTime TimeStamp { get; set; }

        public string Text {
            get
            {
                if (!string.IsNullOrEmpty(_text))
                {
                    if (_text.Length > 320)
                        return _text.Substring(0, 320);
                    else
                        return _text.Substring(0, _text.Length);
                }
                else
                    return string.Empty;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (value.Length > 320)
                        _text = value.Substring(0, 320);
                    else
                        _text = value.Substring(0, value.Length);
                }
                else
                    _text = string.Empty;
            }
        }

        /// <summary>
        /// IsBroadcast property
        /// </summary>
        public bool IsBroadcast
        {
            get { return ApplicationName.ToUpper()=="BDC"; }
        }
    }
    #endregion

    #region Raw structures
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public unsafe struct MailSlotDispatcherMessageStruct
    {
        public byte MsgType;

        public byte Operation;

        public Int32 TargetID;

        public bool MultiInstance;

        public bool AllowToSelf;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string ApplicationName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct MailSlotMessageStruct
    {
        public byte MsgType;

        public byte Operation;

        public int SenderID;

        public DateTime TimeStamp;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string ApplicationName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 320)]
        public string Text;
    }

    /// <summary>
    /// RawSerializer (from github)
    /// </summary>
    internal class RawSerializer<T>
    {
        /// <summary>
        /// Deserialize method
        /// </summary>
        public T Deserialize(byte[] rawData)
        {
            return Deserialize(rawData, 0);
        }

        /// <summary>
        /// Deserialize method overload
        /// </summary>
        public T Deserialize(byte[] rawData, int position)
        {
            int rawsize = Marshal.SizeOf(typeof(T));
            if (rawsize > rawData.Length)
                return default(T);

            IntPtr buffer = Marshal.AllocHGlobal(rawsize);
            try
            {
                Marshal.Copy(rawData, position, buffer, rawsize);
                return (T)Marshal.PtrToStructure(buffer, typeof(T));
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        /// <summary>
        /// Serialize method override
        /// </summary>
        public byte[] Serialize(T item)
        {
            int rawSize = Marshal.SizeOf(typeof(T));
            byte[] rawData = new byte[rawSize];
            IntPtr buffer = Marshal.AllocHGlobal(rawSize);
            try
            {
                Marshal.StructureToPtr(item, buffer, false);
                Marshal.Copy(buffer, rawData, 0, rawSize);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
            return rawData;
        }
    }
    #endregion

    #region Native API Signatures and Types
    /// <summary>
    /// Desired Access of File/Device
    /// </summary>
    [Flags]
    internal enum FileDesiredAccess : uint
    {
        GENERIC_READ = 0x80000000,
        GENERIC_WRITE = 0x40000000,
        GENERIC_EXECUTE = 0x20000000,
        GENERIC_ALL = 0x10000000
    }

    /// <summary>
    /// File share mode
    /// </summary>
    [Flags]
    internal enum FileShareMode : uint
    {
        Zero = 0x00000000,  // No sharing
        FILE_SHARE_DELETE = 0x00000004,
        FILE_SHARE_READ = 0x00000001,
        FILE_SHARE_WRITE = 0x00000002
    }

    /// <summary>
    /// File Creation Disposition
    /// </summary>
    internal enum FileCreationDisposition : uint
    {
        CREATE_NEW = 1,
        CREATE_ALWAYS = 2,
        OPEN_EXISTING = 3,
        OPEN_ALWAYS = 4,
        TRUNCATE_EXISTING = 5
    }

    /// <summary>
    /// SafeMailslotHandle implementation
    /// Represents a wrapper class for a mailslot handle. 
    /// </summary>
    [SuppressUnmanagedCodeSecurity,
    HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true),
    SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    internal sealed class SafeMailslotHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeMailslotHandle(): base(true)
        {
        }

        public SafeMailslotHandle(IntPtr preexistingHandle, bool ownsHandle): base(ownsHandle)
        {
            base.SetHandle(preexistingHandle);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success),
        DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        protected override bool ReleaseHandle()
        {
            return CloseHandle(base.handle);
        }
    }

    /// <summary>
    /// SECURITY_ATTRIBUTES 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal class SECURITY_ATTRIBUTES
    {
        public int nLength;
        public SafeLocalMemHandle lpSecurityDescriptor;
        public bool bInheritHandle;
    }

    /// <summary>
    /// SafeLocalMemHandle implementation
    /// Represents a wrapper class for a local memory pointer. 
    /// </summary>
    [SuppressUnmanagedCodeSecurity,
    HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    internal sealed class SafeLocalMemHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeLocalMemHandle(): base(true)
        {
        }

        public SafeLocalMemHandle(IntPtr preexistingHandle, bool ownsHandle): base(ownsHandle)
        {
            base.SetHandle(preexistingHandle);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success),
        DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LocalFree(IntPtr hMem);

        protected override bool ReleaseHandle()
        {
            return (LocalFree(base.handle) == IntPtr.Zero);
        }
    }

    /// <summary>
    /// NativeMethod implementation
    /// The class exposes Windows APIs.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    internal class NativeMethod
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeMailslotHandle CreateMailslot(String mailslotName, uint nMaxMessageSize, int lReadTimeout, SECURITY_ATTRIBUTES securityAttributes);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeMailslotHandle CreateFile(string fileName,  FileDesiredAccess desiredAccess, FileShareMode shareMode, IntPtr securityAttributes, FileCreationDisposition creationDisposition, int flagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReadFile(SafeMailslotHandle handle, byte[] bytes, int numBytesToRead, out int numBytesRead, IntPtr overlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteFile(SafeMailslotHandle handle, byte[] bytes, int numBytesToWrite, out int numBytesWritten, IntPtr overlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMailslotInfo(SafeMailslotHandle hMailslot, IntPtr lpMaxMessageSize, out int lpNextSize, out int lpMessageCount,IntPtr lpReadTimeout);
       
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(string sddlSecurityDescriptor, int sddlRevision, out SafeLocalMemHandle pSecurityDescriptor, IntPtr securityDescriptorSize);
    }
    #endregion

    #region LogForSlots
    /// <summary>
    /// Log class
    /// </summary>
    public static class LogForSlots
    {
        private const string EventLogSource = "ADFS MFA Service";
        private const string EventLogGroup = "Application";
        private static bool logenabled = true;

        /// <summary>
        /// Log constructor
        /// </summary>
        static LogForSlots()
        {
            if (!EventLog.SourceExists(LogForSlots.EventLogSource))
                EventLog.CreateEventSource(LogForSlots.EventLogSource, LogForSlots.EventLogGroup);
        }

        public static bool LogEnabled
        {
            get { return logenabled; }
            set { logenabled = value; }
        }

        /// <summary>
        /// WriteEntry method implementation
        /// </summary>
        public static void WriteEntry(string message, EventLogEntryType type, int eventID)
        {
            if (LogEnabled)
                EventLog.WriteEntry(EventLogSource, message, type, eventID);
        }
    }
    #endregion
}
