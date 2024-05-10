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
using System;
using System.DirectoryServices;
using System.IO;
using System.Security;

namespace Neos.IdentityServer.MultiFactor.Data
{
    public class WebAuthNPublicKeySerialization
    {
        private ADDSHost _host;
        private bool _weakencoding = false;

        public WebAuthNPublicKeySerialization(ADDSHost host = null)
        {
            _host = host;
            if (_host!=null)
                _weakencoding = _host.WeakPublicKeyEncoding;
        }

        /// <summary>
        /// SerializeCredentials method implementation
        /// </summary>
        public string SerializeCredentials(MFAUserCredential creds, string username)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(creds.Descriptor.Id.Length) ;
            writer.Write(creds.Descriptor.Id, 0, creds.Descriptor.Id.Length);
            writer.Write(creds.UserId.Length);
            writer.Write(creds.UserId, 0, creds.UserId.Length);
            writer.Write(creds.PublicKey.Length);
            writer.Write(creds.PublicKey, 0, creds.PublicKey.Length);
            writer.Write(creds.UserHandle.Length);
            writer.Write(creds.UserHandle, 0, creds.UserHandle.Length);
            writer.Write(creds.SignatureCounter);
            writer.Write(creds.CredType);
            writer.Write(creds.RegDate.ToBinary());
            writer.Write(creds.AaGuid.ToByteArray());

            writer.Write((byte)creds.Descriptor.Type.Value);
            if (creds.Descriptor.Transports != null)
            {
                writer.Write(creds.Descriptor.Transports.Length);
                foreach (MFAAuthenticatorTransport tr in creds.Descriptor.Transports)
                {
                    writer.Write((byte)tr);
                }
            }
            else
                writer.Write(0);
            string Descriptor = HexaEncoding.GetHexStringFromByteArray(stream.ToArray());

            string distinguishedName = string.Empty;
            if ((_host != null) && (!_weakencoding))
                distinguishedName = GetMFAdistinguishedName(username);
            else
                distinguishedName = username;
            distinguishedName = CheckSumEncoding.CheckSumAsString(distinguishedName);

            if (string.IsNullOrEmpty(creds.NickName))
                creds.NickName = "None";
            return string.Format("B:{0}:{1}:{2}:{3}", (Descriptor.Length).ToString(), Descriptor, distinguishedName, creds.NickName);
        }

        /// <summary>
        /// DeserializeCredentials method implementation
        /// </summary>
        public MFAUserCredential DeserializeCredentials(string descriptor, string username)
        {
            string distinguishedName = string.Empty;
            if ((_host != null) && (!_weakencoding))
                distinguishedName = GetMFAdistinguishedName(username);
            else
                distinguishedName = username;
            string[] values = descriptor.Split(':');
            string securitydescriptor = values[2];
            if (!CheckUserName(values[3].ToString(), distinguishedName))
                throw new SecurityException("SECURITY ERROR : Invalid Key for user " + username);

            string nickName = string.Empty;
            if (values.Length == 5)
                nickName = values[4].ToString();
            else
                nickName = "None";

            byte[] bytes = HexaEncoding.GetByteArrayFromHexString(securitydescriptor);
            MemoryStream stream = new MemoryStream(bytes);
            BinaryReader reader = new BinaryReader(stream);

            int test = reader.ReadInt32();
            byte[] _DescId = reader.ReadBytes(test);
            byte[] _Userid = reader.ReadBytes(reader.ReadInt32());
            byte[] _PublicKey = reader.ReadBytes(reader.ReadInt32());
            byte[] _UserHandle = reader.ReadBytes(reader.ReadInt32());
            uint _SignatureCounter = reader.ReadUInt32();
            string _CredType = reader.ReadString();
            DateTime _RegDate = DateTime.FromBinary(reader.ReadInt64());
            Guid _AaGuid = new Guid(reader.ReadBytes(16));

            MFAPublicKeyCredentialType? _DescType = (MFAPublicKeyCredentialType)reader.ReadByte();

            int DescTransportCount = reader.ReadInt32();
            MFAAuthenticatorTransport[] _DescTransport = null;
            if (DescTransportCount > 0)
            {
                _DescTransport = new MFAAuthenticatorTransport[DescTransportCount];
                for (int i = 0; i < DescTransportCount; i++)
                {
                    _DescTransport[i] = (MFAAuthenticatorTransport)reader.ReadByte();
                }
            }
            var creds = new MFAUserCredential()
            {
                UserId = _Userid,
                PublicKey = _PublicKey,
                UserHandle = _UserHandle,
                SignatureCounter = _SignatureCounter,
                CredType = _CredType,
                RegDate = _RegDate,
                AaGuid = _AaGuid,
                NickName = nickName
            };
            creds.Descriptor = new MFAPublicKeyCredentialDescriptor(_DescId)
            {
                Type = _DescType,
                Transports = _DescTransport
            };
            return creds;
        }

        /// <summary>
        /// CheckUserName method implementation
        /// </summary>
        private bool CheckUserName(string value, string userName)
        {
            bool validated = false;
            if (userName.ToLower().Equals(value.ToString().ToLower()))
                validated = true;
            else if (value.Equals(CheckSumEncoding.CheckSumAsString(userName)))
                validated = true;
            return validated;
        }

        /// <summary>
        /// GetMFAdistinguishedName method implementation
        /// </summary>
        private string GetMFAdistinguishedName(string upn)
        {
            string ret = string.Empty;
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(_host, _host.Account, _host.Password, upn))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + upn + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("whenCreated");
                        dsusr.PropertiesToLoad.Add("distinguishedName");
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                ret = DirEntry.Properties["distinguishedName"].Value.ToString();
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return ret;
        }
    }
}
