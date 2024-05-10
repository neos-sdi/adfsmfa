//******************************************************************************************************************************************************************************************//
// Copyright (c) 2024 redhook (adfsmfa@gmail.com)                                                                                                                                    //                        
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
using Neos.IdentityServer.MultiFactor.Data;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace Neos.IdentityServer.MultiFactor
{
    public class MemoryMap<T> where T : class, new()
    {
        public string MemoryMapName { get; }
        public string MutexName { get; }

        private MemoryMappedFile memoryMappedFile;
    
        private const int maxsize = 1024 * 1024 * 32;
        private bool isopen = false;

        /// <summary>
        /// MemoryMap constructor
        /// </summary>
        public MemoryMap(string memoryMapName)
        {
           
            MemoryMapName = @"Global\" + memoryMapName;
            MutexName = MemoryMapName + "-Mutex";
        }

        /// <summary>
        /// Open method implementation
        /// </summary>
        public void Open()
        {
            if (!isopen)
            {
                var securitySettings = new MemoryMappedFileSecurity();
                // securitySettings.AddAccessRule(new AccessRule<MemoryMappedFileRights>(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MemoryMappedFileRights.FullControl, AccessControlType.Allow));
                securitySettings.AddAccessRule(new AccessRule<MemoryMappedFileRights>(new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), MemoryMappedFileRights.FullControl, AccessControlType.Allow));
                securitySettings.AddAccessRule(new AccessRule<MemoryMappedFileRights>(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null), MemoryMappedFileRights.FullControl, AccessControlType.Allow));
                if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAccountSID))
                    securitySettings.AddAccessRule(new AccessRule<MemoryMappedFileRights>(new SecurityIdentifier(ClientSIDsProxy.ADFSAccountSID), MemoryMappedFileRights.FullControl, AccessControlType.Allow));
                if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSServiceSID))
                    securitySettings.AddAccessRule(new AccessRule<MemoryMappedFileRights>(new SecurityIdentifier(ClientSIDsProxy.ADFSServiceSID), MemoryMappedFileRights.FullControl, AccessControlType.Allow));
                if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAdminGroupSID))
                    securitySettings.AddAccessRule(new AccessRule<MemoryMappedFileRights>(new SecurityIdentifier(ClientSIDsProxy.ADFSAdminGroupSID), MemoryMappedFileRights.FullControl, AccessControlType.Allow));

                memoryMappedFile = MemoryMappedFile.CreateOrOpen(MemoryMapName, maxsize, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.DelayAllocatePages, securitySettings, HandleInheritability.None);
                isopen = true;
            }
        }

        /// <summary>
        /// Close method implementation
        /// </summary>
        public void Close()
        {
            if (isopen)
            {
                memoryMappedFile.Dispose();
                isopen = false;
            }
        }

        ///
        public T GetData()
        {
            Mutex mutex = GetMutex(MutexName);
            try
            {
                using (var stm = memoryMappedFile.CreateViewStream(0, 0, MemoryMappedFileAccess.Read))
                {
                    stm.Position = 0;
                    return Deserialize(stm);
                }
            }
            catch (Exception) // Empty Stream or security problem
            {
                return new T();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// SetData method implementation
        /// </summary>
        public void SetData(T data)
        {
            Mutex mutex = GetMutex(MutexName);
            try
            {
                if (!data.GetType().IsSerializable)
                    throw new ArgumentException("Type is not serializable.", nameof(data));
                var obj = Serialize(data);
                using (var stm = memoryMappedFile.CreateViewStream())
                {
                    stm.Position = 0;
                    stm.Write(obj, 0, obj.Length);
                }
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Serialize method implementation
        /// </summary>
        private static byte[] Serialize(T obj)
        {
            IFormatter formatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                formatter.Serialize(memoryStream, obj);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Deserialize method implementation
        /// </summary>
        private static T Deserialize(Stream data)
        {
            IFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(data) as T;               
        }

        /// <summary>
        /// GetMutex method implementation
        /// </summary>
        private Mutex GetMutex(string mutexName)
        {
            if (Mutex.TryOpenExisting(MutexName, out Mutex mutex))
            {
                mutex.WaitOne();
            }
            else
            {
                var mutexsecurity = new MutexSecurity();
               // mutexsecurity.AddAccessRule(new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow));
                mutexsecurity.AddAccessRule(new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), MutexRights.FullControl, AccessControlType.Allow));
                mutexsecurity.AddAccessRule(new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null), MutexRights.FullControl, AccessControlType.Allow));
                if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAccountSID))
                    mutexsecurity.AddAccessRule(new MutexAccessRule(new SecurityIdentifier(ClientSIDsProxy.ADFSAccountSID), MutexRights.FullControl, AccessControlType.Allow));
                if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSServiceSID))
                    mutexsecurity.AddAccessRule(new MutexAccessRule(new SecurityIdentifier(ClientSIDsProxy.ADFSServiceSID), MutexRights.FullControl, AccessControlType.Allow));
                if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAdminGroupSID))
                    mutexsecurity.AddAccessRule(new MutexAccessRule(new SecurityIdentifier(ClientSIDsProxy.ADFSAdminGroupSID), MutexRights.FullControl, AccessControlType.Allow));

                mutex = new Mutex(true, MutexName, out bool mutexCreated, mutexsecurity);
                if (!mutexCreated)
                    mutex.WaitOne();
            }
            return mutex;
        }
    }
}
