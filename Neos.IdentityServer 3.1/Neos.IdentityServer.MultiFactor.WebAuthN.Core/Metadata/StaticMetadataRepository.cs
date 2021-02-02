//******************************************************************************************************************************************************************************************//
// Copyright (c) 2020 abergs (https://github.com/abergs/fido2-net-lib)                                                                                                                      //                        
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Neos.IdentityServer.MultiFactor.WebAuthN.AttestationFormat;
using Newtonsoft.Json;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Metadata
{
    public class StaticMetadataRepository : IMetadataRepository
    {
        protected readonly IDictionary<Guid, MetadataTOCPayloadEntry> _entries;
        protected MetadataTOCPayload _toc;
        protected readonly HttpClient _httpClient;
        protected readonly DateTime? _cacheUntil;

        // from https://developers.yubico.com/U2F/yubico-u2f-ca-certs.txt
        protected const string YUBICO_ROOT = "MIIDHjCCAgagAwIBAgIEG0BT9zANBgkqhkiG9w0BAQsFADAuMSwwKgYDVQQDEyNZ" +
                                "dWJpY28gVTJGIFJvb3QgQ0EgU2VyaWFsIDQ1NzIwMDYzMTAgFw0xNDA4MDEwMDAw" +
                                "MDBaGA8yMDUwMDkwNDAwMDAwMFowLjEsMCoGA1UEAxMjWXViaWNvIFUyRiBSb290" +
                                "IENBIFNlcmlhbCA0NTcyMDA2MzEwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEK" +
                                "AoIBAQC/jwYuhBVlqaiYWEMsrWFisgJ+PtM91eSrpI4TK7U53mwCIawSDHy8vUmk" +
                                "5N2KAj9abvT9NP5SMS1hQi3usxoYGonXQgfO6ZXyUA9a+KAkqdFnBnlyugSeCOep" +
                                "8EdZFfsaRFtMjkwz5Gcz2Py4vIYvCdMHPtwaz0bVuzneueIEz6TnQjE63Rdt2zbw" +
                                "nebwTG5ZybeWSwbzy+BJ34ZHcUhPAY89yJQXuE0IzMZFcEBbPNRbWECRKgjq//qT" +
                                "9nmDOFVlSRCt2wiqPSzluwn+v+suQEBsUjTGMEd25tKXXTkNW21wIWbxeSyUoTXw" +
                                "LvGS6xlwQSgNpk2qXYwf8iXg7VWZAgMBAAGjQjBAMB0GA1UdDgQWBBQgIvz0bNGJ" +
                                "hjgpToksyKpP9xv9oDAPBgNVHRMECDAGAQH/AgEAMA4GA1UdDwEB/wQEAwIBBjAN" +
                                "BgkqhkiG9w0BAQsFAAOCAQEAjvjuOMDSa+JXFCLyBKsycXtBVZsJ4Ue3LbaEsPY4" +
                                "MYN/hIQ5ZM5p7EjfcnMG4CtYkNsfNHc0AhBLdq45rnT87q/6O3vUEtNMafbhU6kt" +
                                "hX7Y+9XFN9NpmYxr+ekVY5xOxi8h9JDIgoMP4VB1uS0aunL1IGqrNooL9mmFnL2k" +
                                "LVVee6/VR6C5+KSTCMCWppMuJIZII2v9o4dkoZ8Y7QRjQlLfYzd3qGtKbw7xaF1U" +
                                "sG/5xUb/Btwb2X2g4InpiB/yt/3CpQXpiWX/K4mBvUKiGn05ZsqeY1gx4g0xLBqc" +
                                "U9psmyPzK+Vsgw2jeRQ5JlKDyqE0hebfC1tvFu0CCrJFcw==";


        public StaticMetadataRepository(DateTime? cacheUntil = null)
        {
            _httpClient = new HttpClient();
            _entries = new Dictionary<Guid, MetadataTOCPayloadEntry>();
            _cacheUntil = cacheUntil;
        }

        public async Task<MetadataStatement> GetMetadataStatement(MetadataTOCPayloadEntry entry)
        {
            if (_toc == null)
                await GetToc();

            if (!string.IsNullOrEmpty(entry.AaGuid) && Guid.TryParse(entry.AaGuid, out Guid parsedAaGuid))
            {
                if (_entries.ContainsKey(parsedAaGuid))
                    return _entries[parsedAaGuid].MetadataStatement;
            }

            return null;
        }

        protected async Task<string> DownloadStringAsync(string url)
        {
            return await _httpClient.GetStringAsync(url);
        }


        public async Task<MetadataTOCPayload> GetToc()
        {
            #region Yubico
            // Yubico Security Key (5.1.x)
            var yubico = new MetadataTOCPayloadEntry
            {
                AaGuid = "f8a011f3-8c0a-4d15-8006-17111f9edc7d",
                Hash = "",
                StatusReports = new StatusReport[]
                {
                    new StatusReport() { Status = AuthenticatorStatus.FIDO_CERTIFIED }
                },
                MetadataStatement = new MetadataStatement
                {
                    AttestationTypes = new ushort[]
                    {
                        (ushort)MetadataAttestationType.ATTESTATION_BASIC_FULL
                    },
                    Hash = "",
                    Description = "Yubico Security Key (5.1.x)",
                    AttestationRootCertificates = new string[]
                    {
                        YUBICO_ROOT
                    }
                }
            };
            _entries.Add(new Guid(yubico.AaGuid), yubico);

            // YubiKey 5 (5.1.x) USB and NFC AAGUID values from https://support.yubico.com/support/solutions/articles/15000014219-yubikey-5-series-technical-manual#AAGUID_Valuesxf002do

            // Yubikey 5 & YubiKey 5C & YubiKey 5C Nano & YubiKey 5 Nano (5.1.x) 
            var yubikey5usb = new MetadataTOCPayloadEntry
            {
                AaGuid = "cb69481e-8ff7-4039-93ec-0a2729a154a8",
                Hash = "",
                StatusReports = new StatusReport[]
                {
                    new StatusReport
                    {
                        Status = AuthenticatorStatus.FIDO_CERTIFIED
                    }
                },
                MetadataStatement = new MetadataStatement
                {
                    AttestationTypes = new ushort[]
                    {
                        (ushort)MetadataAttestationType.ATTESTATION_BASIC_FULL
                    },
                    Hash = "",
                    Description = "Yubikey 5, YubiKey 5C, YubiKey 5C Nano, YubiKey 5 Nano (5.1.x)",
                    AttachmentHint = 6,
                    AttestationRootCertificates = new string[]
                    {
                        YUBICO_ROOT
                    }
                }
            };
            _entries.Add(new Guid(yubikey5usb.AaGuid), yubikey5usb);

            // Yubikey 5 & YubiKey 5C & YubiKey 5C Nano & YubiKey 5 Nano (5.2.x) 
            var yubikey5usb52 = new MetadataTOCPayloadEntry
            {
                AaGuid = "ee882879-721c-4913-9775-3dfcce97072a",
                Hash = "",
                StatusReports = new StatusReport[]
                {
                    new StatusReport
                    {
                        Status = AuthenticatorStatus.FIDO_CERTIFIED
                    }
                },
                MetadataStatement = new MetadataStatement
                {
                    AttestationTypes = new ushort[]
                    {
                        (ushort)MetadataAttestationType.ATTESTATION_BASIC_FULL
                    },
                    Hash = "",
                    Description = "Yubikey 5, YubiKey 5C, YubiKey 5C Nano, YubiKey 5 Nano (5.2.x)",
                    AttachmentHint = 6,
                    AttestationRootCertificates = new string[]
                    {
                        YUBICO_ROOT
                    }
                }
            };
            _entries.Add(new Guid(yubikey5usb52.AaGuid), yubikey5usb52);

            // YubiKey 5 NFC (5.1.x)
            var yubikey5nfc = new MetadataTOCPayloadEntry
            {
                AaGuid = "fa2b99dc-9e39-4257-8f92-4a30d23c4118",
                Hash = "",
                StatusReports = new StatusReport[]
                {
                    new StatusReport
                    {
                        Status = AuthenticatorStatus.FIDO_CERTIFIED
                    }
                },
                MetadataStatement = new MetadataStatement
                {
                    AttestationTypes = new ushort[]
                    {
                        (ushort)MetadataAttestationType.ATTESTATION_BASIC_FULL
                    },
                    Hash = "",
                    Description = "Yubico YubiKey 5 NFC (5.1.x)",
                    AttachmentHint = 30,
                    AttestationRootCertificates = new string[]
                    {
                        YUBICO_ROOT
                    }
                }
            };
            _entries.Add(new Guid(yubikey5nfc.AaGuid), yubikey5nfc);

            // YubiKey 5 NFC (5.2.x)
            var yubikey5nfc52 = new MetadataTOCPayloadEntry
            {
                AaGuid = "2fc0579f-8113-47ea-b116-bb5a8db9202a",
                Hash = "",
                StatusReports = new StatusReport[]
                {
                    new StatusReport
                    {
                        Status = AuthenticatorStatus.FIDO_CERTIFIED
                    }
                },
                MetadataStatement = new MetadataStatement
                {
                    AttestationTypes = new ushort[]
                    {
                        (ushort)MetadataAttestationType.ATTESTATION_BASIC_FULL
                    },
                    Hash = "",
                    Description = "Yubico YubiKey 5 NFC (5.2.x)",
                    AttachmentHint = 30,
                    AttestationRootCertificates = new string[]
                    {
                        YUBICO_ROOT
                    }
                }
            };
            _entries.Add(new Guid(yubikey5nfc52.AaGuid), yubikey5nfc52);

            // YubiKey 5Ci (5.2.x)
            var yubikey5Ci = new MetadataTOCPayloadEntry
            {
                AaGuid = "c5ef55ff-ad9a-4b9f-b580-adebafe026d",
                Hash = "",
                StatusReports = new StatusReport[] { new StatusReport() { Status = AuthenticatorStatus.FIDO_CERTIFIED } },
                MetadataStatement = new MetadataStatement
                {
                    Description = "Yubikey 5Ci (5.2.x)",
                    AttachmentHint = 6,
                    AttestationTypes = new ushort[] { (ushort)MetadataAttestationType.ATTESTATION_BASIC_FULL },
                    Hash = "",
                    AttestationRootCertificates = new string[]
                    {
                        YUBICO_ROOT
                    }
                }
            };
            _entries.Add(new Guid(yubikey5Ci.AaGuid), yubikey5Ci);

            // Security Key 5.2.x
            var yubicoSecuriyKey52 = new MetadataTOCPayloadEntry
            {
                AaGuid = "b92c3f9a-c014-4056-887f-140a2501163b",
                Hash = "",
                StatusReports = new StatusReport[] { new StatusReport() { Status = AuthenticatorStatus.FIDO_CERTIFIED } },
                MetadataStatement = new MetadataStatement
                {
                    Description = "Yubico Security Key (5.2.x)",
                    AttachmentHint = 6,
                    AttestationTypes = new ushort[] { (ushort)MetadataAttestationType.ATTESTATION_BASIC_FULL },
                    Hash = "",
                    AttestationRootCertificates = new string[]
                    {
                        YUBICO_ROOT
                    }
                }
            };
            _entries.Add(new Guid(yubicoSecuriyKey52.AaGuid), yubicoSecuriyKey52);

            // Security Key NFC 5.1.x
            var yubicoSecuriyKeyNfc = new MetadataTOCPayloadEntry  
            {
                AaGuid = "6d44ba9b-f6ec-2e49-b930-0c8fe920cb73",
                Hash = "",
                StatusReports = new StatusReport[] { new StatusReport() { Status = AuthenticatorStatus.FIDO_CERTIFIED } },
                MetadataStatement = new MetadataStatement
                {
                    Description = "Yubico Security Key NFC (5.1.x)",
                    AttachmentHint = 30,
                    AttestationTypes = new ushort[] { (ushort)MetadataAttestationType.ATTESTATION_BASIC_FULL },
                    Hash = "",
                    AttestationRootCertificates = new string[]
                    {
                        YUBICO_ROOT
                    }
                }
            };
            _entries.Add(new Guid(yubicoSecuriyKeyNfc.AaGuid), yubicoSecuriyKeyNfc);

            // Security Key NFC 5.2.x
            var yubicoSecuriyKeyNfc52 = new MetadataTOCPayloadEntry  
            {
                AaGuid = "149a2021-8ef6-4133-96b8-81f8d5b7f1f5",
                Hash = "",
                StatusReports = new StatusReport[] { new StatusReport() { Status = AuthenticatorStatus.FIDO_CERTIFIED } },
                MetadataStatement = new MetadataStatement
                {
                    Description = "Yubico Security Key NFC (5.2.x)",
                    AttachmentHint = 30,
                    AttestationTypes = new ushort[] { (ushort)MetadataAttestationType.ATTESTATION_BASIC_FULL },
                    Hash = "",
                    AttestationRootCertificates = new string[]
                    {
                        YUBICO_ROOT
                    }
                }
            };
            _entries.Add(new Guid(yubicoSecuriyKeyNfc52.AaGuid), yubicoSecuriyKeyNfc52);

            #endregion

            #region Microsoft Hello
            var msftWhfbSoftware = new MetadataTOCPayloadEntry
            {
                AaGuid = "6028B017-B1D4-4C02-B4B3-AFCDAFC96BB2",
                Hash = "",
                StatusReports = new StatusReport[]
                {
                    new StatusReport
                    {
                        Status = AuthenticatorStatus.NOT_FIDO_CERTIFIED
                    }
                },
                MetadataStatement = new MetadataStatement
                {
                    AttestationTypes = new ushort[]
                    {
                        (ushort)MetadataAttestationType.ATTESTATION_BASIC_FULL
                    },
                    Hash = "",
                    Description = "Windows Hello software authenticator"
                }
            };
            _entries.Add(new Guid(msftWhfbSoftware.AaGuid), msftWhfbSoftware);

            var msftWhfbSoftwareVbs = new MetadataTOCPayloadEntry
            {
                AaGuid = "6E96969E-A5CF-4AAD-9B56-305FE6C82795",
                Hash = "",
                StatusReports = new StatusReport[]
                {
                    new StatusReport
                    {
                        Status = AuthenticatorStatus.NOT_FIDO_CERTIFIED
                    }
                },
                MetadataStatement = new MetadataStatement
                {
                    AttestationTypes = new ushort[]
                    {
                        (ushort)MetadataAttestationType.ATTESTATION_BASIC_FULL
                    },
                    Hash = "",
                    Description = "Windows Hello VBS software authenticator"
                }
            };
            _entries.Add(new Guid(msftWhfbSoftwareVbs.AaGuid), msftWhfbSoftwareVbs);

            var msftWhfbHardware = new MetadataTOCPayloadEntry
            {
                AaGuid = "08987058-CADC-4B81-B6E1-30DE50DCBE96",
                Hash = "",
                StatusReports = new StatusReport[]
                {
                    new StatusReport
                    {
                        Status = AuthenticatorStatus.NOT_FIDO_CERTIFIED
                    }
                },
                MetadataStatement = new MetadataStatement
                {
                    AttestationTypes = new ushort[]
                    {
                        (ushort)MetadataAttestationType.ATTESTATION_BASIC_FULL
                    },
                    Hash = "",
                    Description = "Windows Hello hardware authenticator"
                }
            };
            _entries.Add(new Guid(msftWhfbHardware.AaGuid), msftWhfbHardware);

            var msftWhfbHardwareVbs = new MetadataTOCPayloadEntry
            {
                AaGuid = "9DDD1817-AF5A-4672-A2B9-3E3DD95000A9",
                Hash = "",
                StatusReports = new StatusReport[]
                {
                    new StatusReport
                    {
                        Status = AuthenticatorStatus.NOT_FIDO_CERTIFIED
                    }
                },
                MetadataStatement = new MetadataStatement
                {
                    AttestationTypes = new ushort[]
                    {
                        (ushort)MetadataAttestationType.ATTESTATION_BASIC_FULL
                    },
                    Hash = "",
                    Description = "Windows Hello VBS hardware authenticator"
                }
            };
            _entries.Add(new Guid(msftWhfbHardwareVbs.AaGuid), msftWhfbHardwareVbs);
            #endregion
           
            #region Solo
            var solostatement = await DownloadStringAsync("https://raw.githubusercontent.com/solokeys/solo/master/metadata/Solo-FIDO2-CTAP2-Authenticator.json");
            var soloMetadataStatement = JsonConvert.DeserializeObject<MetadataStatement>(solostatement);
            var soloKeysSolo = new MetadataTOCPayloadEntry
            {
                AaGuid = soloMetadataStatement.AaGuid,
                Url = "https://raw.githubusercontent.com/solokeys/solo/master/metadata/Solo-FIDO2-CTAP2-Authenticator.json",
                StatusReports = new StatusReport[]
                {
                    new StatusReport
                    {
                        Status = AuthenticatorStatus.NOT_FIDO_CERTIFIED
                    }
                },
                MetadataStatement = soloMetadataStatement
            };
            _entries.Add(new Guid(soloKeysSolo.AaGuid), soloKeysSolo);

            var soloTapStatement = await DownloadStringAsync("https://raw.githubusercontent.com/solokeys/solo/master/metadata/SoloTap-FIDO2-CTAP2-Authenticator.json");
            var soloTapMetadataStatement = JsonConvert.DeserializeObject<MetadataStatement>(soloTapStatement);
            var soloTapMetadata = new MetadataTOCPayloadEntry
            {
                AaGuid = soloTapMetadataStatement.AaGuid,
                Url = "https://raw.githubusercontent.com/solokeys/solo/master/metadata/SoloTap-FIDO2-CTAP2-Authenticator.json",
                StatusReports = new StatusReport[]
                {
                    new StatusReport
                    {
                        Status = AuthenticatorStatus.NOT_FIDO_CERTIFIED
                    }
                },
                MetadataStatement = soloTapMetadataStatement
            };
            _entries.Add(new Guid(soloTapMetadata.AaGuid), soloTapMetadata);

            var soloSomuStatement = await DownloadStringAsync("https://raw.githubusercontent.com/solokeys/solo/master/metadata/Somu-FIDO2-CTAP2-Authenticator.json");
            var soloSomuMetadataStatement = JsonConvert.DeserializeObject<MetadataStatement>(soloSomuStatement);
            var soloSomuMetadata = new MetadataTOCPayloadEntry
            {
                AaGuid = soloSomuMetadataStatement.AaGuid,
                Url = "https://raw.githubusercontent.com/solokeys/solo/master/metadata/Somu-FIDO2-CTAP2-Authenticator.json",
                StatusReports = new StatusReport[]
                {
                    new StatusReport
                    {
                        Status = AuthenticatorStatus.NOT_FIDO_CERTIFIED
                    }
                },
                MetadataStatement = soloSomuMetadataStatement
            };
            _entries.Add(new Guid(soloSomuMetadata.AaGuid), soloSomuMetadata);
            
            #endregion

            foreach (var entry in _entries)
            {
                entry.Value.MetadataStatement.AaGuid = entry.Value.AaGuid;
            }

            _toc = new MetadataTOCPayload()
            {
                Entries = _entries.Select(o => o.Value).ToArray(),
                NextUpdate = _cacheUntil?.ToString("yyyy-MM-dd") ?? "", //Results in no caching
                LegalHeader = "Static FAKE",
                Number = 1
            };

            return _toc;
        } 
    }
}
