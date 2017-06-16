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
//******************************************************************************************************************************************************************************************//
// This source is subject to the Microsoft Public License.                                                                                                                                  // 
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.                                                                                                                             //
// All other rights reserved.                                                                                                                                                               //
//                                                                                                                                                                                          //
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,                                                                                                              // 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED                                                                                                                    //
// WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.                                                                                                                   //
//******************************************************************************************************************************************************************************************//
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor
{
    #region BaseMailSlotServer
    public abstract class BaseMailSlotServer<T, S> : IDisposable where T : BaseMailSlotMessage<T, S>
    {
        internal const int MAILSLOT_WAIT_FOREVER = -1;
        internal const int MAILSLOT_NO_MESSAGE = -1;
        private SafeMailslotHandle _mailslot = null;
        private SECURITY_ATTRIBUTES _securityattr = null;
        private List<BaseMailSlotMessage<T, S>> _messages = new List<BaseMailSlotMessage<T, S>>();
        private CancellationTokenSource _canceltokensource = null;
        private string _appname = string.Empty;
        private string _machinename = string.Empty;
        private int _processid = 0;
        private int _scanduration = 3000;
        private Task _task = null;

        public BaseMailSlotServer(int scanduration = 3000)
        {
            Process process = Process.GetCurrentProcess();
            _processid = process.Id;
            _appname = "MGR";
            _machinename = Environment.MachineName;
            _scanduration = scanduration;
            _securityattr = CreateMailslotSecurity();
            _mailslot = NativeMethod.CreateMailslot(this.MailSlotName, 0, MAILSLOT_WAIT_FOREVER, _securityattr);
            if (_mailslot.IsInvalid)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            _canceltokensource = new CancellationTokenSource();
        }

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public BaseMailSlotServer(string appname, int scanduration = 3000)
        {
            Process process = Process.GetCurrentProcess();
            _processid = process.Id;
            _appname = appname;
            _machinename = Environment.MachineName;
            _scanduration = scanduration;
            _securityattr = CreateMailslotSecurity();
            _mailslot = NativeMethod.CreateMailslot(this.MailSlotName, 0, MAILSLOT_WAIT_FOREVER, _securityattr);
            if (_mailslot.IsInvalid)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            _canceltokensource = new CancellationTokenSource();
        }

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public BaseMailSlotServer(string appname, int processid, int scanduration = 3000)
        {
            _processid = processid;
            _appname = appname;
            _machinename = Environment.MachineName;
            _scanduration = scanduration;
            _securityattr = CreateMailslotSecurity();
            _mailslot = NativeMethod.CreateMailslot(this.MailSlotName, 0, MAILSLOT_WAIT_FOREVER, _securityattr);
            if (_mailslot.IsInvalid)
                throw new Win32Exception(Marshal.GetLastWin32Error());
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
        /// MachineNname property
        /// </summary>
        public string MachineName
        {
            get { return _machinename; }
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
        /// Start method implentation
        /// </summary>
        public virtual void Start()
        {
            _canceltokensource = new CancellationTokenSource();
            _task = new Task(() => ReadMailSlots(), _canceltokensource.Token, TaskCreationOptions.LongRunning);
            _task.Start();
        }

        /// <summary>
        /// Stop method implementation
        /// </summary>
        public virtual void Stop()
        {
            _canceltokensource.Cancel(true);
            _task.Dispose();
         //   _task = null;
        }

        /// <summary>
        /// ReadMailSlots method implementation
        /// </summary>
        private void ReadMailSlots()
        {
            while (_mailslot != null)
            {
                if (_canceltokensource.Token.IsCancellationRequested)
                    return;
                _messages.Clear();
                if (internalReadMailslot())
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
        }

        /// <summary>
        /// ReadMailslot method implementation
        /// </summary>
        private bool internalReadMailslot()
        {
            int cbMessageBytes = 0;
            int cbBytesRead = 0;
            int cMessages = 0;
            int nMessageId = 0;

            bool succeeded = false;
            if (_canceltokensource.Token.IsCancellationRequested)
                return succeeded;

            _messages.Clear();
            succeeded = NativeMethod.GetMailslotInfo(_mailslot, IntPtr.Zero, out cbMessageBytes, out cMessages, IntPtr.Zero);
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
                using (MemoryStream memStream = new MemoryStream())
                {
                    BinaryFormatter binForm = new BinaryFormatter();
                    memStream.Write(bBuffer, 0, bBuffer.Length);
                    memStream.Seek(0, SeekOrigin.Begin);
                    S flatted = (S)binForm.Deserialize(memStream);
                    BaseMailSlotMessage<T, S> msg = new BaseMailSlotMessage<T, S>();
                    msg.FlatData = flatted;
                    _messages.Add(msg);
                }
                succeeded = NativeMethod.GetMailslotInfo(_mailslot, IntPtr.Zero, out cbMessageBytes, out cMessages, IntPtr.Zero);
                if (!succeeded)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
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
                _canceltokensource.Cancel();
                if (_mailslot != null)
                {
                    _mailslot.Close();
                    _mailslot = null;
                }
            }
        }

        /// <summary>
        /// CreateMailslotSecurity() implementation
        /// </summary>
        /// <see cref="http://msdn.microsoft.com/en-us/library/aa365600.aspx"/>
        private SECURITY_ATTRIBUTES CreateMailslotSecurity()
        {
            // Define the SDDL for the security descriptor.
            string sddl = "D:" +        // Discretionary ACL
                "(A;OICI;GRGW;;;AU)" +  // Allow read/write to authenticated users
                "(A;OICI;GA;;;BA)";     // Allow full control to administrators

            SafeLocalMemHandle pSecurityDescriptor = null;
            if (!NativeMethod.ConvertStringSecurityDescriptorToSecurityDescriptor(sddl, 1, out pSecurityDescriptor, IntPtr.Zero))
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
    public class MailSlotServer : BaseMailSlotServer<MailSlotMessage, MailSlotMessageStruct>
    {
        public delegate void MailSlotMessageArrivedEvent(MailSlotServer maislotserver, MailSlotMessage message);
        public delegate void MFAConfigurationChangedEvent(MailSlotServer maislotserver, MailSlotMessage message);
        public delegate void MFASystemChangedEvent(MailSlotServer maislotserver, MailSlotMessage message);
        public event MailSlotMessageArrivedEvent MailSlotMessageArrived;
        public event MFAConfigurationChangedEvent ConfigurationChanged;
        public event MFASystemChangedEvent SystemChanged;

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public MailSlotServer(string appname, int scanduration = 3000): base(appname, scanduration)
        {
        }

        /// <summary>
        /// MailSlotName property
        /// </summary>
        protected override string MailSlotName
        {
            get { return @"\\.\mailslot\mfa\cfg\" + this.ApplicationName + @"\" + this.ProcessID.ToString(); }
        }

        public override void Start()
        {
            using (MailSlotClientDispatcher mailslot = new MailSlotClientDispatcher())
            {
                mailslot.Register(this.MachineName, this.ApplicationName, this.ProcessID);
            }
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
            using (MailSlotClientDispatcher mailslot = new MailSlotClientDispatcher())
            {
                mailslot.UnRegister(this.MachineName, this.ApplicationName, this.ProcessID);
            }
        }

        /// <summary>
        /// DoOnMessage method implmentation
        /// </summary>
        protected override void DoOnMessage(MailSlotMessage message)
        {
            switch (message.Operation)
            {
                case 0:
                    this.MailSlotMessageArrived(this, message);
                    break;
                case 1:
                    this.ConfigurationChanged(this, message);
                    break;
                case 2:
                    this.SystemChanged(this, message);
                    break;
            }
        }

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
                base.Dispose(disposing);
                using (MailSlotClientDispatcher mailslot = new MailSlotClientDispatcher())
                {
                    mailslot.UnRegister(this.MachineName, this.ApplicationName, this.ProcessID);
                }
            }
        }
    }
    #endregion

    #region MailSlotServerDispatcher
    public class MailSlotServerDispatcher : BaseMailSlotServer<MailSlotMessage, MailSlotMessageStruct>
    {
        /// <summary>
        /// Constructor implementation
        /// </summary>
        public MailSlotServerDispatcher(string appname, int processid, int scanduration = 3000):base(appname, processid, scanduration) { }

        /// <summary>
        /// MailSlotName property
        /// </summary>
        protected override string MailSlotName
        {
            get { return @"\\.\mailslot\mfa\cfg\" + ApplicationName; }
        }

        /// <summary>
        /// DoOnMessage method implmentation
        /// </summary>
        protected override void DoOnMessage(MailSlotMessage message)
        {
            if (message.Sender.ToLower().Equals(ApplicationName.ToLower()))// || (message.MachineName.ToLower().Equals(MachineName.ToLower())))
            {
                using (MailSlotClientForwarder mailslotcli = new MailSlotClientForwarder(this.ApplicationName, this.ProcessID))
                {
                    mailslotcli.SendNotification(message);
                }
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
                    mailslot.UnRegister(this.MachineName, this.ApplicationName, this.ProcessID);
                }
                base.Dispose(disposing);
            }
        }
    }
    #endregion

    #region MailSlotServerManager
    public class MailSlotServerManager : BaseMailSlotServer<MailSlotDispMessage, MailSlotMessageDispStruct>
    {
        private List<MailSlotServerDispatcher> _mailslotlst = new List<MailSlotServerDispatcher>();

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public MailSlotServerManager(int scanduration = 3000): base(scanduration) { }

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public MailSlotServerManager(string appname, int scanduration = 3000) : base(appname, scanduration) { }

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
        /// <param name="message"></param>
        protected override void DoOnMessage(MailSlotDispMessage message)
        {
            if (message.MachineName.ToLower().Equals(this.MachineName.ToLower()))
            {
                if (message.Operation == 1)
                {
                    int index = this._mailslotlst.FindIndex(c => c.ApplicationName.Equals(message.Sender));
                    if (index < 0)
                    {
                        MailSlotServerDispatcher m = new MailSlotServerDispatcher(message.Sender, message.ProcessID, this.ScanDuration);
                        _mailslotlst.Add(m);
                        m.Start();
                    }
                }
                else
                {
                    int index = this._mailslotlst.FindIndex(c => c.ApplicationName.Equals(message.Sender));
                    if (index < 0)
                    {
                        MailSlotServerDispatcher m = _mailslotlst[index];
                        m.Stop();
                        _mailslotlst.Remove(m);
                    }
                }
            }
        }
    }
    #endregion
    #endregion

    #region Client MailSlots
    #region MailSlotClient
    public class MailSlotClient: IDisposable
    {
        private string _mailslotname = @"\\*\mailslot\mfa\cfg";
        internal const int MAILSLOT_WAIT_FOREVER = -1;
        internal const int MAILSLOT_NO_MESSAGE = -1;
        private SafeMailslotHandle _mailslot = null;
        private string _appname = string.Empty;
        private string _machinename = string.Empty;
        private int _processid = 0;


        public MailSlotClient(string appname)
        {
            Process process = Process.GetCurrentProcess();
            _appname = appname;
            _machinename = Environment.MachineName;
            _processid = process.Id;
            _mailslotname = @"\\*\mailslot\mfa\cfg\"+_appname;
            _mailslot = NativeMethod.CreateFile(_mailslotname, FileDesiredAccess.GENERIC_WRITE, FileShareMode.FILE_SHARE_READ, IntPtr.Zero, FileCreationDisposition.OPEN_EXISTING,  0, IntPtr.Zero);
            if (_mailslot.IsInvalid)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

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
                if (_mailslot != null)
                {
                    _mailslot.Close();
                    _mailslot = null;
                }
            }
        }

        /// <summary>
        /// SendNotification method implmentation
        /// Send generic notification
        /// </summary>
        public void SendNotification(String text = "", byte sub = 0)
        {
            MailSlotMessage message = new MailSlotMessage();
            message.Sender = _appname;
            message.MachineName = _machinename;
            message.ProcessID = _processid;
            message.Text = text;
            message.Operation = 0;
            message.SubOperation = sub;
            WriteMailslot(message);
        }

        /// <summary>
        /// SendConfigurationChanged method implmentation
        /// Send configuration notification
        /// </summary>
        public void SendConfigurationChanged(String text = "", byte sub = 0)
        {
            MailSlotMessage message = new MailSlotMessage();
            message.Sender = _appname;
            message.MachineName = _machinename;
            message.ProcessID = _processid;
            message.Text = text;
            message.Operation = 1;
            message.SubOperation = sub;
            WriteMailslot(message);
        }
        /// <summary>
        /// SendSystemChanged method implmentation
        /// Send system notification
        /// </summary>
        public void SendSystemChanged(String text = "", byte sub = 0)
        {
            MailSlotMessage message = new MailSlotMessage();
            message.Sender = _appname;
            message.MachineName = _machinename;
            message.ProcessID = _processid;
            message.Text = text;
            message.Operation = 2;
            message.SubOperation = sub;
            WriteMailslot(message);
        }

        /// <summary>
        /// WriteMailslot method implementation
        /// </summary>
        private void WriteMailslot(MailSlotMessage message)
        {
            int cbMessageBytes = 0;         
            int cbBytesWritten = 0;         
            byte[] bMessage = message;
            cbMessageBytes = bMessage.Length;
            bool succeeded = NativeMethod.WriteFile(_mailslot, bMessage, cbMessageBytes, out cbBytesWritten, IntPtr.Zero);
            if (!succeeded || cbMessageBytes != cbBytesWritten)
               throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
    #endregion

    #region MailSlotClientDispatcher
    internal class MailSlotClientDispatcher : IDisposable
    {
        private string _mailslotname = @"\\*\mailslot\mfa\dispatcher";
        internal const int MAILSLOT_WAIT_FOREVER = -1;
        internal const int MAILSLOT_NO_MESSAGE = -1;
        private SafeMailslotHandle _mailslot = null;


        public MailSlotClientDispatcher()
        {
            _mailslot = NativeMethod.CreateFile(_mailslotname, FileDesiredAccess.GENERIC_WRITE, FileShareMode.FILE_SHARE_READ, IntPtr.Zero, FileCreationDisposition.OPEN_EXISTING, 0, IntPtr.Zero);
            if (_mailslot.IsInvalid)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

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
                if (_mailslot != null)
                {
                    _mailslot.Close();
                    _mailslot = null;
                }
            }
        }

        public void Register(string machine, string application, int processid)
        {
            MailSlotDispMessage msg = new MailSlotDispMessage();
            msg.MachineName = machine;
            msg.Sender = application;
            msg.ProcessID = processid;
            msg.Operation = 1;
            WriteMailslot(msg);
        }

        public void UnRegister(string machine, string application, int processid)
        {
            MailSlotDispMessage msg = new MailSlotDispMessage();
            msg.MachineName = machine;
            msg.Sender = application;
            msg.ProcessID = processid;
            msg.Operation = 0;
            WriteMailslot(msg);
        }

        /// <summary>
        /// WriteMailslot method implementation
        /// </summary>
        private void WriteMailslot(MailSlotDispMessage message)
        {
            int cbMessageBytes = 0;
            int cbBytesWritten = 0;
            byte[] bMessage = message;
            cbMessageBytes = bMessage.Length;
            bool succeeded = NativeMethod.WriteFile(_mailslot, bMessage, cbMessageBytes, out cbBytesWritten, IntPtr.Zero);
            if (!succeeded || cbMessageBytes != cbBytesWritten)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
    #endregion

    #region MailSlotClientForwarder
    public class MailSlotClientForwarder : IDisposable
    {
        private string _mailslotname = @"\\*\mailslot\mfa\cfg";
        internal const int MAILSLOT_WAIT_FOREVER = -1;
        internal const int MAILSLOT_NO_MESSAGE = -1;
        private SafeMailslotHandle _mailslot = null;
        private string _appname = string.Empty;
        private string _machinename = string.Empty;
        private int _processid = 0;

        public MailSlotClientForwarder(string appname, int processid)
        {
            _appname = appname;
            _machinename = Environment.MachineName;
            _processid = processid;
            _mailslotname = @"\\*\mailslot\mfa\cfg\" + _appname + @"\" + _processid.ToString();
            _mailslot = NativeMethod.CreateFile(_mailslotname, FileDesiredAccess.GENERIC_WRITE, FileShareMode.FILE_SHARE_READ, IntPtr.Zero, FileCreationDisposition.OPEN_EXISTING, 0, IntPtr.Zero);
            if (_mailslot.IsInvalid)
                throw new Win32Exception();
        }

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
                if (_mailslot != null)
                {
                    _mailslot.Close();
                    _mailslot = null;
                }
            }
        }

        /// <summary>
        /// SendNotification method implmentation
        /// Send generic notification
        /// </summary>
        public void SendNotification(MailSlotMessage message)
        {
            WriteMailslot(message);
        }

        /// <summary>
        /// WriteMailslot method implementation
        /// </summary>
        private void WriteMailslot(MailSlotMessage message)
        {
            int cbMessageBytes = 0;
            int cbBytesWritten = 0;
            byte[] bMessage = message;
            cbMessageBytes = bMessage.Length;
            bool succeeded = NativeMethod.WriteFile(_mailslot, bMessage, cbMessageBytes, out cbBytesWritten, IntPtr.Zero);
            if (!succeeded || cbMessageBytes != cbBytesWritten)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
    #endregion
    #endregion

    #region MailSlotMessage
    public enum MailSlotMessageKind
    {
        Default = 0,
        Configuration = 1
    }

    [Serializable]
    public struct MailSlotMessageStruct
    {
        public byte Operation;
        public byte SubOperation;
        public int ProcessID;
        public string MachineName;
        public string Sender;
        public string Text;

        static public implicit operator MailSlotMessageStruct(MailSlotMessage value)
        {
            MailSlotMessageStruct outstr = default(MailSlotMessageStruct);
            outstr.Operation = value.Operation;
            outstr.SubOperation = value.SubOperation;
            outstr.MachineName = value.MachineName;
            outstr.Sender = value.Sender;
            outstr.ProcessID = value.ProcessID;
            outstr.Text = value.Text; 
            return outstr;
        }

        static public implicit operator MailSlotMessage(MailSlotMessageStruct flatted)
        {
            MailSlotMessage outobj = new MailSlotMessage();
            outobj.Operation = flatted.Operation;
            outobj.SubOperation = flatted.SubOperation;
            outobj.MachineName = flatted.MachineName;
            outobj.Sender = flatted.Sender;
            outobj.ProcessID = flatted.ProcessID;
            outobj.Text = flatted.Text;
            return outobj;
        }
    }

    [Serializable]
    public struct MailSlotMessageDispStruct
    {
        public byte Operation;
        public int ProcessID;
        public string MachineName;
        public string Sender;

        static public implicit operator MailSlotMessageDispStruct(MailSlotDispMessage value)
        {
            MailSlotDispMessage outobj = new MailSlotDispMessage();
            outobj.Operation = value.Operation;
            outobj.MachineName = value.MachineName;
            outobj.Sender = value.Sender;
            outobj.ProcessID = value.ProcessID;
            return outobj;
        }

        static public implicit operator MailSlotDispMessage(MailSlotMessageDispStruct flatted)
        {
            MailSlotDispMessage outobj = new MailSlotDispMessage();
            outobj.Operation = flatted.Operation;
            outobj.MachineName = flatted.MachineName;
            outobj.Sender = flatted.Sender;
            outobj.ProcessID = flatted.ProcessID;
            return outobj;
        }
    }

    [DataContract, Serializable]
    public struct BaseMailSlotMessage
    {
        /// <summary>
        /// Constructor implementation
        /// </summary>
        public BaseMailSlotMessage()
        {

        }

        /// <summary>
        /// implicit conversion to byte array
        /// </summary>
        public static implicit operator byte[](BaseMailSlotMessage filterobj)
        {
            if (filterobj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, filterobj.FlatData);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// implicit conversion from ResultNode
        /// </summary>
        public static implicit operator BaseMailSlotMessage<T, S>(byte[] data)
        {
            if (data == null)
                return default(BaseMailSlotMessage<T,S>);

            using (MemoryStream memStream = new MemoryStream())
            {
                BinaryFormatter binForm = new BinaryFormatter();
                memStream.Write(data, 0, data.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                S flatted = (S)binForm.Deserialize(memStream);
                BaseMailSlotMessage<T, S> obj = new BaseMailSlotMessage<T, S>();
                obj.FlatData = flatted;
                return obj;
            }
        }

        [DataMember]
        public virtual byte Operation { get; set; }

        [DataMember]
        public virtual int ProcessID { get; set; }

        [DataMember]
        public virtual string MachineName { get; set; }

        [DataMember]
        public virtual string Sender { get; set; }

        public virtual S FlatData { get; set; }
    }


    [DataContract, Serializable]
    public struct MailSlotDispMessage : BaseMailSlotMessage
    {
        private byte[] _machinename;
        private byte[] _sender;

        [DataMember]
        public override byte Operation { get; set; }

        [DataMember]
        public override int ProcessID { get; set; }

        [DataMember]
        public override string MachineName
        {
            get { return ASCIIEncoding.ASCII.GetString(_machinename); }
            set
            {
                int l = 15;
                if (value != null)
                {
                    if (value.Length < 15)
                        l = value.Length;
                    _machinename = ASCIIEncoding.ASCII.GetBytes(value.Substring(0, l));
                }
                else
                    _machinename = null;
            }
        }

        [DataMember]
        public override string Sender
        {
            get { return ASCIIEncoding.ASCII.GetString(_sender); }
            set
            {
                int l = 3;
                if (value != null)
                {
                    if (value.Length < 3)
                        l = value.Length;
                    _sender = ASCIIEncoding.ASCII.GetBytes(value.Substring(0, l));
                }
                else
                    _sender = null;
            }
        }

        /// <summary>
        /// FlatData  property implmentation
        /// </summary>
        public override MailSlotMessageDispStruct FlatData 
        { 
            get
            {
                MailSlotMessageDispStruct obj = default(MailSlotMessageDispStruct);
                obj.MachineName = this.MachineName;
                obj.Operation = this.Operation;
                obj.ProcessID = this.ProcessID;
                obj.Sender = this.Sender;
                return obj;
            }
            set
            {
                MailSlotMessageDispStruct obj = value;
                this.MachineName = obj.MachineName;
                this.Operation = obj.Operation;
                this.ProcessID = obj.ProcessID;
                this.Sender = obj.Sender;
            }
        }
    }

    [DataContract, Serializable]
    public class MailSlotMessage : BaseMailSlotMessage<MailSlotMessage, MailSlotMessageStruct>
    {
        private byte[] _machinename;
        private byte[] _sender;
        private byte[] _text;
      
        [DataMember]
        public override byte Operation { get; set; }

        [DataMember]
        public byte SubOperation { get; set; }

        [DataMember]
        public override int ProcessID { get; set; }

        [DataMember]
        public override string MachineName 
        {
            get { return ASCIIEncoding.ASCII.GetString(_machinename); }
            set 
            {
                int l = 15;
                if (value != null)
                {
                    if (value.Length < 15)
                        l = value.Length;
                    _machinename = ASCIIEncoding.ASCII.GetBytes(value.Substring(0, l));
                }
                else
                    _machinename = null;
            }
        }

        [DataMember]
        public override string Sender 
        {
            get { return ASCIIEncoding.ASCII.GetString(_sender); }
            set 
            {
                int l = 3;
                if (value != null)
                {
                    if (value.Length < 3)
                        l = value.Length;
                    _sender = ASCIIEncoding.ASCII.GetBytes(value.Substring(0, l));
                }
                else
                    _sender = null;
            }
        }

        [DataMember]
        public string Text
        {
            get { return ASCIIEncoding.ASCII.GetString(_text); }
            set 
            {
                int l = 32;
                if (value != null)
                {
                    if (value.Length < 32)
                        l = value.Length;
                    _text = ASCIIEncoding.ASCII.GetBytes(value.Substring(0, l));
                }
                else
                    _text = null;
            }
        }

        /// <summary>
        /// FlatData  property implmentation
        /// </summary>
        public override MailSlotMessageStruct FlatData
        {
            get
            {
                MailSlotMessageStruct obj = default(MailSlotMessageStruct);
                obj.MachineName = this.MachineName;
                obj.Operation = this.Operation;
                obj.ProcessID = this.ProcessID;
                obj.Sender = this.Sender;
                obj.SubOperation = this.SubOperation;
                obj.Text = this.Text;
                return obj;
            }
            set
            {
                MailSlotMessageStruct obj = value;
                this.MachineName = obj.MachineName;
                this.Operation = obj.Operation;
                this.ProcessID = obj.ProcessID;
                this.Sender = obj.Sender;
                this.SubOperation = obj.SubOperation;
                this.Text = obj.Text;
            }
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
    [SecurityCritical(SecurityCriticalScope.Everything),
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
        public static extern SafeMailslotHandle CreateMailslot(string mailslotName, uint nMaxMessageSize, int lReadTimeout, SECURITY_ATTRIBUTES securityAttributes);

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
}
