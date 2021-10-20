//******************************************************************************************************************************************************************************************//
// Copyright (c) 2021 @redhook62 (adfsmfa@gmail.com)                                                                                                                                        //                        
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
//                                                                                                                                                                                          //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using Microsoft.IdentityModel.Tokens;
using Neos.IdentityServer.MultiFactor.Data;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;


namespace Neos.IdentityServer.MultiFactor.WebAuthN.Metadata
{
    public abstract class BaseSystemMetadataRepository: IMetadataRepository
    {        
        protected const string ROOT_CERT ="MIIDXzCCAkegAwIBAgILBAAAAAABIVhTCKIwDQYJKoZIhvcNAQELBQAwTDEgMB4G" +
                                          "A1UECxMXR2xvYmFsU2lnbiBSb290IENBIC0gUjMxEzARBgNVBAoTCkdsb2JhbFNp" +
                                          "Z24xEzARBgNVBAMTCkdsb2JhbFNpZ24wHhcNMDkwMzE4MTAwMDAwWhcNMjkwMzE4" +
                                          "MTAwMDAwWjBMMSAwHgYDVQQLExdHbG9iYWxTaWduIFJvb3QgQ0EgLSBSMzETMBEG" +
                                          "A1UEChMKR2xvYmFsU2lnbjETMBEGA1UEAxMKR2xvYmFsU2lnbjCCASIwDQYJKoZI" +
                                          "hvcNAQEBBQADggEPADCCAQoCggEBAMwldpB5BngiFvXAg7aEyiie/QV2EcWtiHL8" +
                                          "RgJDx7KKnQRfJMsuS+FggkbhUqsMgUdwbN1k0ev1LKMPgj0MK66X17YUhhB5uzsT" +
                                          "gHeMCOFJ0mpiLx9e+pZo34knlTifBtc+ycsmWQ1z3rDI6SYOgxXG71uL0gRgykmm" +
                                          "KPZpO/bLyCiR5Z2KYVc3rHQU3HTgOu5yLy6c+9C7v/U9AOEGM+iCK65TpjoWc4zd" +
                                          "QQ4gOsC0p6Hpsk+QLjJg6VfLuQSSaGjlOCZgdbKfd/+RFO+uIEn8rUAVSNECMWEZ" +
                                          "XriX7613t2Saer9fwRPvm2L7DWzgVGkWqQPabumDk3F2xmmFghcCAwEAAaNCMEAw" +
                                          "DgYDVR0PAQH/BAQDAgEGMA8GA1UdEwEB/wQFMAMBAf8wHQYDVR0OBBYEFI/wS3+o" +
                                          "LkUkrk1Q+mOai97i3Ru8MA0GCSqGSIb3DQEBCwUAA4IBAQBLQNvAUKr+yAzv95ZU" +
                                          "RUm7lgAJQayzE4aGKAczymvmdLm6AC2upArT9fHxD4q/c2dKg8dEe3jgr25sbwMp" +
                                          "jjM5RcOO5LlXbKr8EpbsU8Yt5CRsuZRj+9xTaGdWPoO4zzUhw8lo/s7awlOqzJCK" +
                                          "6fBdRoyV3XpYKBovHd7NADdBj+1EbddTKJd+82cEHhXXipa0095MJ6RMG3NzdvQX" +
                                          "mcIfeg7jLQitChws/zyrVQ4PkX4268NXSb7hLi18YIvDQVETI53O9zJrlAGomecs" +
                                          "Mx86OyXShkDOOyyGeMlhLxS67ttVb9+E7gUJTb0o2HLO02JQZR7rkpeDMdmztcpH" +
                                          "WD9f";

        /// <summary>
        /// IsInitialized property implementation
        /// </summary>
        public bool IsInitialized
        {
            get;
            set;
        }

        /// <summary>
        /// GetBLOB method implementation
        /// </summary>
        public abstract Task<MetadataBLOBPayload> GetBLOB();

        /// <summary>
        /// GetMetadataStatement method implementation
        /// </summary>
        public virtual Task<MetadataStatement> GetMetadataStatement(MetadataBLOBPayload blob, MetadataBLOBPayloadEntry entry)
        {
            return Task.FromResult<MetadataStatement>(entry.MetadataStatement);
        }

        protected abstract Task<string> GetRawBlob();

        protected abstract MetadataBLOBPayload DeserializeAndValidateBlob(BLOBPayloadInformations infos);

        /// <summary>
        /// GetECDsaPublicKey method implementation
        /// </summary>
        protected virtual ECDsaSecurityKey GetECDsaPublicKey(string certString)
        {
            try
            {
                var certBytes = Convert.FromBase64String(certString);
                var cert = new X509Certificate2(certBytes);
                var publicKey = cert.GetECDsaPublicKey();
                return new ECDsaSecurityKey(publicKey);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Could not parse X509 certificate.", ex);
            }
        }

        /// <summary>
        /// GetX509Certificate method implementation
        /// </summary>
        protected virtual X509Certificate2 GetX509Certificate(string certString)
        {
            try
            {
                var certBytes = Convert.FromBase64String(certString);
                return new X509Certificate2(certBytes);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Could not parse X509 certificate.", ex);
            }
        }


        /// <summary>
        /// HasBLOBPayloadCache method implementation
        /// </summary>
        protected virtual bool HasBLOBPayloadCache()
        {
            return WebAdminManagerClient.HasBLOBPayloadCache();
        }

        /// <summary>
        /// ReadBLOBPayloadCache method implementation
        /// </summary>
        protected virtual BLOBPayloadInformations GetBLOBPayloadCache()
        {
            return WebAdminManagerClient.GetBLOBPayloadCache();
        }

        /// <summary>
        /// SetBLOBPayloadCache method implementation
        /// </summary>
        protected virtual void SetBLOBPayloadCache(BLOBPayloadInformations infos)
        {
            WebAdminManagerClient.SetBLOBPayloadCache(infos);
        }
    }
}
