using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor.Data
{
    internal class WebAuthNPublicKeySerialization
    {
        private ADDSHost _host;

        internal WebAuthNPublicKeySerialization(ADDSHost host = null)
        {
            _host = host;
        }

        /// <summary>
        /// SerializeCredentials method implementation
        /// </summary>
        internal string SerializeCredentials(MFAUserCredential creds, string username)
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
            if (_host != null)
                distinguishedName = GetMFAdistinguishedName(username);
            else
                distinguishedName = username;

            return string.Format("B:{0}:{1}:{2}", (Descriptor.Length).ToString(), Descriptor, distinguishedName);
        }

        /// <summary>
        /// DeserializeCredentials method implementation
        /// </summary>
        internal MFAUserCredential DeserializeCredentials(string descriptor, string username)
        {
            string distinguishedName = string.Empty;
            if (_host != null)
                distinguishedName = GetMFAdistinguishedName(username);
            else
                distinguishedName = username;
            string[] values = descriptor.Split(':');
            string value = values[2];
            if (!distinguishedName.ToLower().Equals(values[3].ToString().ToLower()))
                throw new SecurityException("Invalid Key for user " + username);

            byte[] bytes = HexaEncoding.GetByteArrayFromHexString(value);
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
            };
            creds.Descriptor = new MFAPublicKeyCredentialDescriptor(_DescId)
            {
                Type = _DescType,
                Transports = _DescTransport
            };
            return creds;
        }

        /// <summary>
        /// GetMFAdistinguishedName method implementation
        /// </summary>
        private string GetMFAdistinguishedName(string upn)
        {
            string ret = string.Empty;
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, _host.Account, _host.Password, upn))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + upn + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("whenCreated");
                        dsusr.PropertiesToLoad.Add("distinguishedName");

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
