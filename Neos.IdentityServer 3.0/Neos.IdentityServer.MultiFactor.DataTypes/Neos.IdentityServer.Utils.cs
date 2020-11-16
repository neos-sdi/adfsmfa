//******************************************************************************************************************************************************************************************//
// Copyright (c) 2020 @redhook62 (adfsmfa@gmail.com)                                                                                                                                    //                        
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
using CERTENROLLLib;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;

namespace Neos.IdentityServer.MultiFactor.Data
{
    /// <summary>
    /// Certs class implmentation
    /// </summary>
    public static class Certs
    {
        public static string ADFSAccountSID = string.Empty;
        public static string ADFSServiceSID = string.Empty;
        public static string ADFSAdminGroupSID = string.Empty;
        private static RegistryVersion _RegistryVersion;

        static Certs()
        {
            _RegistryVersion = new RegistryVersion();
        }

        /// <summary>
        /// GetCertificate method implementation
        /// </summary>
        public static X509Certificate2 GetCertificate(string value, StoreLocation location)
        {
            X509Certificate2 data = null;
            X509Store store = new X509Store(location);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            try
            {
                X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
                X509Certificate2Collection findCollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByThumbprint, value, false);

                foreach (X509Certificate2 x509 in findCollection)
                {
                    data = x509;
                    break;
                }
            }
            catch
            {
                data = null;
            }
            finally
            {
                store.Close();
            }
            return data;
        }

        /// <summary>
        /// RemoveSelfSignedCertificate method implmentation
        /// </summary>
        private static bool RemoveSelfSignedCertificate(X509Certificate2 cert, StoreLocation location = StoreLocation.LocalMachine)
        {
            if (cert != null)
            {
                X509Store store1 = new X509Store(StoreName.My, location);
                X509Store store2 = new X509Store(StoreName.CertificateAuthority, location);
                store1.Open(OpenFlags.MaxAllowed);
                store2.Open(OpenFlags.MaxAllowed);
                try
                {
                    X509Certificate2Collection collection1 = (X509Certificate2Collection)store1.Certificates;
                    X509Certificate2Collection findCollection1 = (X509Certificate2Collection)collection1.Find(X509FindType.FindByThumbprint, cert.Thumbprint, false);
                    foreach (X509Certificate2 x509 in findCollection1)
                    {
                        store1.Remove(x509);
                        x509.Reset();
                    }
                    X509Certificate2Collection collection2 = (X509Certificate2Collection)store2.Certificates;
                    X509Certificate2Collection findCollection2 = (X509Certificate2Collection)collection2.Find(X509FindType.FindByThumbprint, cert.Thumbprint, false);
                    foreach (X509Certificate2 x509 in findCollection2)
                    {
                        store2.Remove(x509);
                        x509.Reset();
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
                finally
                {
                    store1.Close();
                    store2.Close();
                }
            }
            else
                return false;
        }

        /// <summary>
        /// CleanSelfSignedCertificate method implmentation
        /// </summary>
        private static bool CleanSelfSignedCertificate(X509Certificate2 cert, StoreLocation location = StoreLocation.LocalMachine)
        {
            if (cert != null)
            {
                X509Store store = new X509Store(StoreName.CertificateAuthority, location);
                store.Open(OpenFlags.MaxAllowed);
                try
                {
                    X509Certificate2Collection collection2 = (X509Certificate2Collection)store.Certificates;
                    X509Certificate2Collection findCollection2 = (X509Certificate2Collection)collection2.Find(X509FindType.FindByThumbprint, cert.Thumbprint, false);
                    foreach (X509Certificate2 x509 in findCollection2)
                    {
                        store.Remove(x509);
                        x509.Reset();
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
                finally
                {
                    store.Close();
                }
            }
            else
                return false;
        }

        /// <summary>
        /// CreateADFSCertificate method implementation
        /// </summary>
        public static bool CreateADFSCertificate(string subjectName, bool issigning, int years)
        {
            string strcert = string.Empty;
            X509Certificate2 x509 = null;
            try
            {
                if (_RegistryVersion.IsWindows2012R2)
                    strcert = InternalCreateADFSCertificate2012R2(subjectName, issigning, years);
                else
                    strcert = InternalCreateADFSCertificate(subjectName, issigning, years);
                x509 = new X509Certificate2(Convert.FromBase64String(strcert), "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
                CleanSelfSignedCertificate(x509, StoreLocation.LocalMachine);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                x509.Reset();
            }
        }

        /// <summary>
        /// CreateRSACertificate method implementation
        /// </summary>
        public static X509Certificate2 CreateRSACertificate(string subjectName, int years)
        {
            string strcert = string.Empty;
            if (_RegistryVersion.IsWindows2012R2)
                strcert = InternalCreateRSACertificate2012R2(subjectName, years);
            else
                strcert = InternalCreateRSACertificate(subjectName, years);
            X509Certificate2 x509 = new X509Certificate2(Convert.FromBase64String(strcert), "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
            if (CleanSelfSignedCertificate(x509, StoreLocation.LocalMachine))
                return x509;
            else
                return null;
        }

        /// <summary>
        /// CreateRSACertificateForSQLEncryption method implementation
        /// </summary>
        public static X509Certificate2 CreateRSACertificateForSQLEncryption(string subjectName, int years)
        {
            string strcert = string.Empty;
            if (_RegistryVersion.IsWindows2012R2)
                strcert = InternalCreateSQLCertificate2012R2(subjectName, years);
            else
                strcert = InternalCreateSQLCertificate(subjectName, years);
            X509Certificate2 x509 = new X509Certificate2(Convert.FromBase64String(strcert), "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
            if (CleanSelfSignedCertificate(x509, StoreLocation.LocalMachine))
                return x509;
            else
                return null;
        }


        /// <summary>
        /// CreateRSAEncryptionCertificateForUser method implementation
        /// </summary>
        public static X509Certificate2 CreateRSAEncryptionCertificateForUser(string subjectName, int years, string pwd = "")
        {
            string strcert = string.Empty;
            if (_RegistryVersion.IsWindows2012R2)
                strcert = InternalCreateUserRSACertificate2012R2(subjectName, years, pwd);
            else
                strcert = InternalCreateUserRSACertificate(subjectName, years, pwd);
            X509Certificate2 cert = new X509Certificate2(Convert.FromBase64String(strcert), pwd, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
            if (Certs.RemoveSelfSignedCertificate(cert, StoreLocation.CurrentUser))
                return cert;
            else
                return null;
        }

        #region Windows 2016 & 2019
        /// <summary>
        /// InternalCreateRSACertificate method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateRSACertificate(string subjectName, int years)
        {
            string base64encoded = string.Empty;
            CX500DistinguishedName dn = new CX500DistinguishedName();
            CX500DistinguishedName neos = new CX500DistinguishedName();
            dn.Encode("CN=" + subjectName + " " + DateTime.UtcNow.ToString("G") + " GMT", X500NameFlags.XCN_CERT_NAME_STR_NONE);
            neos.Encode("CN=MFA RSA Keys Certificate", X500NameFlags.XCN_CERT_NAME_STR_NONE);

            CX509PrivateKey privateKey = new CX509PrivateKey
            {
                ProviderName = "Microsoft RSA SChannel Cryptographic Provider",
                MachineContext = true,
                Length = 2048,
                KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE, // use is not limited
                ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_FLAG,
                KeyUsage = X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_DECRYPT_FLAG,
                SecurityDescriptor = "D:(A;;FA;;;SY)(A;;FA;;;BA)"
            };
            if (!string.IsNullOrEmpty(ADFSServiceSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSServiceSID + ")";
            if (!string.IsNullOrEmpty(ADFSAccountSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSAccountSID + ")";
            if (!string.IsNullOrEmpty(ADFSAdminGroupSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSAdminGroupSID + ")";
            try
            {
                privateKey.Create();
                CObjectId hashobj = new CObjectId();
                hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID,
                                                    ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY,
                                                    AlgorithmFlags.AlgorithmFlagsNone, "SHA256");

                CObjectId oid = new CObjectId();
                // oid.InitializeFromValue("1.3.6.1.5.5.7.3.1"); // SSL server  
                oid.InitializeFromValue("1.3.6.1.4.1.311.80.1"); // Encryption

                CObjectIds oidlist = new CObjectIds
                {
                    oid
                };

                CObjectId coid = new CObjectId();
                // coid.InitializeFromValue("1.3.6.1.5.5.7.3.2"); // Client auth
                coid.InitializeFromValue("1.3.6.1.5.5.7.3.3"); // Signature
                oidlist.Add(coid);

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);

                certreq.NotAfter = DateTime.Now.AddYears(years);
                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.HashAlgorithm = hashobj; // Specify the hashing algorithm
                certreq.Encode(); // encode the certificate

                // Do the final enrollment process
                CX509Enrollment enroll = new CX509Enrollment();
                enroll.InitializeFromRequest(certreq); // load the certificate
                enroll.CertificateFriendlyName = subjectName; // Optional: add a friendly name

                string csr = enroll.CreateRequest(); // Output the request in base64

                // and install it back as the response
                enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, "");

                // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
                base64encoded = enroll.CreatePFX("", PFXExportOptions.PFXExportChainWithRoot);
            }
            catch (Exception ex)
            {
                if (privateKey != null)
                    privateKey.Delete();
                throw ex;
            }
            finally
            {
                // DO nothing, certificate Key is stored in the system
            }
            return base64encoded;
        }

        /// <summary>
        /// InternalCreateUserRSACertificate method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateUserRSACertificate(string subjectName, int years, string pwd = "")
        {
            string base64encoded = string.Empty;
            CX500DistinguishedName dn = new CX500DistinguishedName();
            CX500DistinguishedName neos = new CX500DistinguishedName();
            dn.Encode("CN=" + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
            neos.Encode("CN=MFA RSA Keys Certificate", X500NameFlags.XCN_CERT_NAME_STR_NONE);

            CX509PrivateKey privateKey = new CX509PrivateKey
            {
                ProviderName = "Microsoft RSA SChannel Cryptographic Provider",
                MachineContext = false,
                Length = 2048,
                KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE, // use is not limited
                ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_FLAG,
                SecurityDescriptor = "D:(A;;FA;;;SY)(A;;FA;;;BA)"
            };
            if (!string.IsNullOrEmpty(ADFSServiceSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSServiceSID + ")";
            if (!string.IsNullOrEmpty(ADFSAccountSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSAccountSID + ")";
            if (!string.IsNullOrEmpty(ADFSAdminGroupSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSAdminGroupSID + ")";
            try
            {
                privateKey.Create();
                CObjectId hashobj = new CObjectId();
                hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID,
                                                    ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY,
                                                    AlgorithmFlags.AlgorithmFlagsNone, "SHA256");

                CObjectId oid = new CObjectId();
                // oid.InitializeFromValue("1.3.6.1.5.5.7.3.1"); // SSL server  
                oid.InitializeFromValue("1.3.6.1.4.1.311.80.1"); // Encryption

                CObjectIds oidlist = new CObjectIds
                {
                    oid
                };

                CObjectId coid = new CObjectId();
                // coid.InitializeFromValue("1.3.6.1.5.5.7.3.2"); // Client auth
                coid.InitializeFromValue("1.3.6.1.5.5.7.3.3"); // Signature
                oidlist.Add(coid);

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextUser, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);

                certreq.NotAfter = DateTime.Now.AddYears(years);
                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.HashAlgorithm = hashobj; // Specify the hashing algorithm
                certreq.Encode(); // encode the certificate

                // Do the final enrollment process
                CX509Enrollment enroll = new CX509Enrollment();
                enroll.InitializeFromRequest(certreq); // load the certificate
                enroll.CertificateFriendlyName = subjectName; // Optional: add a friendly name

                string csr = enroll.CreateRequest(); // Output the request in base64

                // and install it back as the response
                enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, "");

                // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
                base64encoded = enroll.CreatePFX(pwd, PFXExportOptions.PFXExportChainWithRoot);
            }
            catch (Exception ex)
            {
                if (privateKey!=null)
                    privateKey.Delete();
                throw ex;
            }
            finally
            {
                if (privateKey != null)
                    privateKey.Delete(); // Remove Stored elsewhere
            }
            return base64encoded;
        }

        /// <summary>
        /// InternalCreateSQLCertificate method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateSQLCertificate(string subjectName, int years, string pwd = "")
        {
            string base64encoded = string.Empty;
            CX500DistinguishedName dn = new CX500DistinguishedName();
            CX500DistinguishedName neos = new CX500DistinguishedName();
            dn.Encode("CN=" + subjectName + " " + DateTime.UtcNow.ToString("G") + " GMT", X500NameFlags.XCN_CERT_NAME_STR_NONE);
            neos.Encode("CN=Always Encrypted Certificate", X500NameFlags.XCN_CERT_NAME_STR_NONE);

            CX509PrivateKey privateKey = new CX509PrivateKey
            {
                ProviderName = "Microsoft RSA SChannel Cryptographic Provider",
                MachineContext = true,

                Length = 2048,
                KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE, // use is not limited
                ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_FLAG,
                SecurityDescriptor = "D:PAI(A;;0xd01f01ff;;;SY)(A;;0xd01f01ff;;;BA)(A;;0xd01f01ff;;;CO)"
            };
            if (!string.IsNullOrEmpty(ADFSServiceSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSServiceSID + ")";
            if (!string.IsNullOrEmpty(ADFSAccountSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSAccountSID + ")";
            if (!string.IsNullOrEmpty(ADFSAdminGroupSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSAdminGroupSID + ")";
            try
            {
                privateKey.Create();
                CObjectId hashobj = new CObjectId();
                hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID,
                                                    ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY,
                                                    AlgorithmFlags.AlgorithmFlagsNone, "SHA256");


                // 2.5.29.37 – Enhanced Key Usage includes

                CObjectId oid = new CObjectId();
                oid.InitializeFromValue("1.3.6.1.5.5.8.2.2"); // IP security IKE intermediate
                var oidlist = new CObjectIds
                {
                    oid
                };

                CObjectId coid = new CObjectId();
                coid.InitializeFromValue("1.3.6.1.4.1.311.10.3.11"); // Key Recovery
                oidlist.Add(coid);

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);

                certreq.NotAfter = DateTime.Now.AddYears(years);
                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.HashAlgorithm = hashobj; // Specify the hashing algorithm
                certreq.Encode(); // encode the certificate

                // Do the final enrollment process
                CX509Enrollment enroll = new CX509Enrollment();
                enroll.InitializeFromRequest(certreq); // load the certificate
                enroll.CertificateFriendlyName = subjectName; // Optional: add a friendly name

                string csr = enroll.CreateRequest(); // Output the request in base64

                // and install it back as the response
                enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, "");

                // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
                base64encoded = enroll.CreatePFX("", PFXExportOptions.PFXExportChainWithRoot);
            }
            catch (Exception ex)
            {
                if (privateKey!=null)
                    privateKey.Delete();
                throw ex;
            }
            finally
            {
                // DO nothing, certificate Key is stored in the system
            }
            return base64encoded;
        }

        /// <summary>
        /// InternalCreateADFSCertificate method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateADFSCertificate(string subjectName, bool issigning, int years)
        {
            string base64encoded = string.Empty;
            CX500DistinguishedName dn = new CX500DistinguishedName();
            CX500DistinguishedName neos = new CX500DistinguishedName();

            CX509PrivateKey privateKey = new CX509PrivateKey
            {
                ProviderName = "Microsoft Enhanced Cryptographic Provider v1.0",
                MachineContext = true,
                Length = 2048,
                KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE, // use is not limited
                ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_FLAG,              
                SecurityDescriptor = "D:(A;;FA;;;SY)(A;;FA;;;BA)"
            };
            if (issigning)
                privateKey.KeyUsage = X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_SIGNING_FLAG;
            else
                privateKey.KeyUsage = X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_DECRYPT_FLAG;
            if (!string.IsNullOrEmpty(ADFSServiceSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSServiceSID + ")";
            if (!string.IsNullOrEmpty(ADFSAccountSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSAccountSID + ")";
            if (!string.IsNullOrEmpty(ADFSAdminGroupSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSAdminGroupSID + ")";
            try
            {
                privateKey.Create();

                CObjectId hashobj = new CObjectId();
                hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID, ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY, AlgorithmFlags.AlgorithmFlagsNone, "SHA256");
                CObjectIds oidlist = new CObjectIds();
                if (!issigning)
                {
                    CObjectId oid = new CObjectId();
                    oid.InitializeFromValue("1.3.6.1.4.1.311.80.1"); // Encryption
                    oidlist.Add(oid);
                    dn.Encode("CN=ADFS Encrypt - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                    neos.Encode("CN=ADFS Encrypt - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                }
                else
                {
                    CObjectId coid = new CObjectId();
                    coid.InitializeFromValue("1.3.6.1.5.5.7.3.3"); // Signature
                    oidlist.Add(coid);
                    dn.Encode("CN=ADFS Sign - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                    neos.Encode("CN=ADFS Sign - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                }

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);

                certreq.NotAfter = DateTime.Now.AddYears(years);
                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.HashAlgorithm = hashobj; // Specify the hashing algorithm
                certreq.Encode(); // encode the certificate

                // Do the final enrollment process
                CX509Enrollment enroll = new CX509Enrollment();
                enroll.InitializeFromRequest(certreq); // load the certificate
                enroll.CertificateFriendlyName = subjectName; // Optional: add a friendly name

                string csr = enroll.CreateRequest(); // Output the request in base64

                // and install it back as the response
                enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, "");

                // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
                base64encoded = enroll.CreatePFX("", PFXExportOptions.PFXExportEEOnly);
            }
            catch (Exception ex)
            {
                if (privateKey!=null)
                    privateKey.Delete();
                throw ex;
            }
            finally
            {
                // DO nothing, certificate Key is stored in the system
            }
            return base64encoded;
        }
        #endregion

        #region Windows 2012R2
        /// <summary>
        /// InternalCreateRSACertificate2012R2 method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateRSACertificate2012R2(string subjectName, int years)
        {
            string base64encoded = string.Empty;
            CX500DistinguishedName dn = new CX500DistinguishedName();
            CX500DistinguishedName neos = new CX500DistinguishedName();
            dn.Encode("CN=" + subjectName + " " + DateTime.UtcNow.ToString("G") + " GMT", X500NameFlags.XCN_CERT_NAME_STR_NONE);
            neos.Encode("CN=MFA RSA Keys Certificate", X500NameFlags.XCN_CERT_NAME_STR_NONE);

            IX509PrivateKey privateKey = (IX509PrivateKey)Activator.CreateInstance(Type.GetTypeFromProgID("X509Enrollment.CX509PrivateKey"));
            privateKey.ProviderName = "Microsoft RSA SChannel Cryptographic Provider";
            privateKey.MachineContext = true;
            privateKey.Length = 2048;
            privateKey.KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE; // use is not limited
            privateKey.ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_FLAG;
            privateKey.SecurityDescriptor = "D:(A;;FA;;;SY)(A;;FA;;;BA)";

            if (!string.IsNullOrEmpty(ADFSServiceSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSServiceSID + ")";
            if (!string.IsNullOrEmpty(ADFSAccountSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSAccountSID + ")";
            if (!string.IsNullOrEmpty(ADFSAdminGroupSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSAdminGroupSID + ")";
            try
            {
                privateKey.Create();
                CObjectId hashobj = new CObjectId();
                hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID,
                                                    ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY,
                                                    AlgorithmFlags.AlgorithmFlagsNone, "SHA256");

                CObjectId oid = new CObjectId();
                // oid.InitializeFromValue("1.3.6.1.5.5.7.3.1"); // SSL server  
                oid.InitializeFromValue("1.3.6.1.4.1.311.80.1"); // Encryption

                CObjectIds oidlist = new CObjectIds
                {
                    oid
                };

                CObjectId coid = new CObjectId();
                // coid.InitializeFromValue("1.3.6.1.5.5.7.3.2"); // Client auth
                coid.InitializeFromValue("1.3.6.1.5.5.7.3.3"); // Signature
                oidlist.Add(coid);

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);

                certreq.NotAfter = DateTime.Now.AddYears(years);
                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.HashAlgorithm = hashobj; // Specify the hashing algorithm
                certreq.Encode(); // encode the certificate

                // Do the final enrollment process
                CX509Enrollment enroll = new CX509Enrollment();
                enroll.InitializeFromRequest(certreq); // load the certificate
                enroll.CertificateFriendlyName = subjectName; // Optional: add a friendly name

                string csr = enroll.CreateRequest(); // Output the request in base64

                // and install it back as the response
                enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, "");

                // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
                base64encoded = enroll.CreatePFX("", PFXExportOptions.PFXExportChainWithRoot);
            }
            catch (Exception ex)
            {
                if (privateKey!=null)
                    privateKey.Delete();
                throw ex;
            }
            finally
            {
                // DO nothing, certificate Key is stored in the system
            }
            return base64encoded;
        }

        /// <summary>
        /// InternalCreateUserRSACertificate2012R2 method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateUserRSACertificate2012R2(string subjectName, int years, string pwd = "")
        {
            string base64encoded = string.Empty;
            CX500DistinguishedName dn = new CX500DistinguishedName();
            CX500DistinguishedName neos = new CX500DistinguishedName();
            dn.Encode("CN=" + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
            neos.Encode("CN=MFA RSA Keys Certificate", X500NameFlags.XCN_CERT_NAME_STR_NONE);

            IX509PrivateKey privateKey = (IX509PrivateKey)Activator.CreateInstance(Type.GetTypeFromProgID("X509Enrollment.CX509PrivateKey"));
            privateKey.ProviderName = "Microsoft RSA SChannel Cryptographic Provider";
            privateKey.MachineContext = false;
            privateKey.Length = 2048;
            privateKey.KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE; // use is not limited
            privateKey.ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_FLAG;
            privateKey.SecurityDescriptor = "D:(A;;FA;;;SY)(A;;FA;;;BA)";

            if (!string.IsNullOrEmpty(ADFSServiceSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSServiceSID + ")";
            if (!string.IsNullOrEmpty(ADFSAccountSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSAccountSID + ")";
            if (!string.IsNullOrEmpty(ADFSAdminGroupSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSAdminGroupSID + ")";
            try
            {
                privateKey.Create();
                CObjectId hashobj = new CObjectId();
                hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID,
                                                    ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY,
                                                    AlgorithmFlags.AlgorithmFlagsNone, "SHA256");

                CObjectId oid = new CObjectId();
                // oid.InitializeFromValue("1.3.6.1.5.5.7.3.1"); // SSL server  
                oid.InitializeFromValue("1.3.6.1.4.1.311.80.1"); // Encryption

                CObjectIds oidlist = new CObjectIds
                {
                    oid
                };

                CObjectId coid = new CObjectId();
                // coid.InitializeFromValue("1.3.6.1.5.5.7.3.2"); // Client auth
                coid.InitializeFromValue("1.3.6.1.5.5.7.3.3"); // Signature
                oidlist.Add(coid);

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextUser, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);

                certreq.NotAfter = DateTime.Now.AddYears(years);
                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.HashAlgorithm = hashobj; // Specify the hashing algorithm
                certreq.Encode(); // encode the certificate

                // Do the final enrollment process
                CX509Enrollment enroll = new CX509Enrollment();
                enroll.InitializeFromRequest(certreq); // load the certificate
                enroll.CertificateFriendlyName = subjectName; // Optional: add a friendly name

                string csr = enroll.CreateRequest(); // Output the request in base64

                // and install it back as the response
                enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, "");

                // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
                base64encoded = enroll.CreatePFX(pwd, PFXExportOptions.PFXExportChainWithRoot);
            }
            catch (Exception ex)
            {
                if (privateKey!=null)
                    privateKey.Delete();
                throw ex;
            }
            finally
            {
                if (privateKey != null)
                    privateKey.Delete(); // Remove Stored elsewhere
            }
            return base64encoded;
        }

        /// <summary>
        /// InternalCreateSQLCertificate method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateSQLCertificate2012R2(string subjectName, int years, string pwd = "")
        {
            string base64encoded = string.Empty;
            CX500DistinguishedName dn = new CX500DistinguishedName();
            CX500DistinguishedName neos = new CX500DistinguishedName();
            dn.Encode("CN=" + subjectName + " " + DateTime.UtcNow.ToString("G") + " GMT", X500NameFlags.XCN_CERT_NAME_STR_NONE);
            neos.Encode("CN=Always Encrypted Certificate", X500NameFlags.XCN_CERT_NAME_STR_NONE);

            IX509PrivateKey privateKey = (IX509PrivateKey)Activator.CreateInstance(Type.GetTypeFromProgID("X509Enrollment.CX509PrivateKey"));
            privateKey.ProviderName = "Microsoft RSA SChannel Cryptographic Provider";
            privateKey.MachineContext = true;
            privateKey.Length = 2048;
            privateKey.KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE; // use is not limited
            privateKey.ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_FLAG;
            privateKey.SecurityDescriptor = "D:(A;;FA;;;SY)(A;;FA;;;BA)";

            if (!string.IsNullOrEmpty(ADFSServiceSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSServiceSID + ")";
            if (!string.IsNullOrEmpty(ADFSAccountSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSAccountSID + ")";
            if (!string.IsNullOrEmpty(ADFSAdminGroupSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSAdminGroupSID + ")";
            try
            {
                privateKey.Create();
                CObjectId hashobj = new CObjectId();
                hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID,
                                                    ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY,
                                                    AlgorithmFlags.AlgorithmFlagsNone, "SHA256");


                // 2.5.29.37 – Enhanced Key Usage includes

                CObjectId oid = new CObjectId();
                oid.InitializeFromValue("1.3.6.1.5.5.8.2.2"); // IP security IKE intermediate
                var oidlist = new CObjectIds
                {
                    oid
                };

                CObjectId coid = new CObjectId();
                coid.InitializeFromValue("1.3.6.1.4.1.311.10.3.11"); // Key Recovery
                oidlist.Add(coid);

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);

                certreq.NotAfter = DateTime.Now.AddYears(years);
                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.HashAlgorithm = hashobj; // Specify the hashing algorithm
                certreq.Encode(); // encode the certificate

                // Do the final enrollment process
                CX509Enrollment enroll = new CX509Enrollment();
                enroll.InitializeFromRequest(certreq); // load the certificate
                enroll.CertificateFriendlyName = subjectName; // Optional: add a friendly name

                string csr = enroll.CreateRequest(); // Output the request in base64

                // and install it back as the response
                enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, "");

                // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
                base64encoded = enroll.CreatePFX("", PFXExportOptions.PFXExportChainWithRoot);
            }
            catch (Exception ex)
            {
                if (privateKey!=null)
                    privateKey.Delete();
                throw ex;
            }
            finally
            {
                // DO nothing, certificate Key is stored in the system
            }
            return base64encoded;
        }

        /// <summary>
        /// InternalCreateADFSCertificate method implementation
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        private static string InternalCreateADFSCertificate2012R2(string subjectName, bool issigning, int years)
        {
            string base64encoded = string.Empty;
            CX500DistinguishedName dn = new CX500DistinguishedName();
            CX500DistinguishedName neos = new CX500DistinguishedName();

            IX509PrivateKey privateKey = (IX509PrivateKey)Activator.CreateInstance(Type.GetTypeFromProgID("X509Enrollment.CX509PrivateKey"));
            privateKey.ProviderName = "Microsoft Enhanced Cryptographic Provider v1.0";
            privateKey.MachineContext = true;
            privateKey.Length = 2048;
            privateKey.KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE; // use is not limited
            privateKey.ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_FLAG;
            privateKey.SecurityDescriptor = "D:(A;;FA;;;SY)(A;;FA;;;BA)";

            if (issigning)
                privateKey.KeyUsage = X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_SIGNING_FLAG;
            else
                privateKey.KeyUsage = X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_DECRYPT_FLAG;
            if (!string.IsNullOrEmpty(ADFSServiceSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSServiceSID + ")";
            if (!string.IsNullOrEmpty(ADFSAccountSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSAccountSID + ")";
            if (!string.IsNullOrEmpty(ADFSAdminGroupSID))
                privateKey.SecurityDescriptor += "(A;;FA;;;" + ADFSAdminGroupSID + ")";
            try
            {
                privateKey.Create();

                CObjectId hashobj = new CObjectId();
                hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID, ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY, AlgorithmFlags.AlgorithmFlagsNone, "SHA256");
                CObjectIds oidlist = new CObjectIds();
                if (!issigning)
                {
                    CObjectId oid = new CObjectId();
                    oid.InitializeFromValue("1.3.6.1.4.1.311.80.1"); // Encryption
                    oidlist.Add(oid);
                    dn.Encode("CN=ADFS Encrypt - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                    neos.Encode("CN=ADFS Encrypt - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                }
                else
                {
                    CObjectId coid = new CObjectId();
                    coid.InitializeFromValue("1.3.6.1.5.5.7.3.3"); // Signature
                    oidlist.Add(coid);
                    dn.Encode("CN=ADFS Sign - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                    neos.Encode("CN=ADFS Sign - " + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                }

                CX509ExtensionEnhancedKeyUsage eku = new CX509ExtensionEnhancedKeyUsage();
                eku.InitializeEncode(oidlist);

                // Create the self signing request
                CX509CertificateRequestCertificate certreq = new CX509CertificateRequestCertificate();
                certreq.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
                certreq.Subject = dn;
                certreq.Issuer = neos;
                certreq.NotBefore = DateTime.Now.AddDays(-10);

                certreq.NotAfter = DateTime.Now.AddYears(years);
                certreq.X509Extensions.Add((CX509Extension)eku); // add the EKU
                certreq.HashAlgorithm = hashobj; // Specify the hashing algorithm
                certreq.Encode(); // encode the certificate

                // Do the final enrollment process
                CX509Enrollment enroll = new CX509Enrollment();
                enroll.InitializeFromRequest(certreq); // load the certificate
                enroll.CertificateFriendlyName = subjectName; // Optional: add a friendly name

                string csr = enroll.CreateRequest(); // Output the request in base64

                // and install it back as the response
                enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, "");

                // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
                base64encoded = enroll.CreatePFX("", PFXExportOptions.PFXExportEEOnly);
            }
            catch (Exception ex)
            {
                if (privateKey!=null)
                    privateKey.Delete();
                throw ex;
            }
            finally
            {
                // DO nothing, certificate Key is stored in the system
            }
            return base64encoded;
        }
        #endregion

        /// <summary>
        /// KeyMgtOptions
        /// </summary>
        [Flags]
        public enum KeyMgtOptions
        {
            AllCerts = 0x0,
            MFACerts = 0x1,
            ADFSCerts = 0x2,
            SSLCerts = 0x4
        }


        #region Orphaned Keys
        /// <summary>
        /// HasAssociatedCertificate method implementation
        /// </summary>
        private static bool HasAssociatedCertificate(string filename)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.MaxAllowed);
            try
            {
                X509Certificate2Collection collection2 = (X509Certificate2Collection)store.Certificates;
                foreach (X509Certificate2 x509 in collection2)
                {
                    try
                    {
                        string cntName = string.Empty;
                        var rsakey = x509.GetRSAPrivateKey();
                        if (rsakey is RSACng)
                            cntName = ((RSACng)rsakey).Key.UniqueName;
                        else if (rsakey is RSACryptoServiceProvider)
                            cntName = ((RSACryptoServiceProvider)rsakey).CspKeyContainerInfo.UniqueKeyContainerName;
                        if (filename.ToLower().Equals(cntName.ToLower()))
                            return true;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                store.Close();
            }
        }

        /// <summary>
        /// CleanOrphanedPrivateKeys method implementation
        /// </summary>
        public static int CleanOrphanedPrivateKeys()
        {
            int result = 0;
            char sep = Path.DirectorySeparatorChar;
            List<string> paths = new List<string>()
            {
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + sep + "Microsoft" + sep + "Crypto" + sep + "RSA" + sep + "MachineKeys"+ sep,
               // Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Microsoft\Crypto\Keys\"
            };
            foreach (string pth in paths)
            {
                DirectoryInfo dir = new DirectoryInfo(pth);

                foreach (FileInfo fi in dir.GetFiles())
                {
                    try
                    {
                        if (fi.Name.ToLower().StartsWith("f686aace6942fb7f7ceb231212eef4a4_"))  // RDP Key do not drop anyway
                            continue;
                        if (!HasAssociatedCertificate(fi.Name))
                        {
                            fi.Delete();
                            result++;
                        }
                    }
                    catch { }
                }
            }
            return result;
        }

        /// <summary>
        /// CleanOrphanedPrivateKeysRegistry method implementation
        /// </summary>
        public static void CleanOrphanedPrivateKeysRegistry(byte option, int delay)
        {
            if (option == 0x00)  // None
                return;
            if (option == 0x01)  // Enable
            {
                RegistryKey ek = Registry.LocalMachine.OpenSubKey("Software\\MFA", true);
                if (ek == null)
                {
                    ek = Registry.LocalMachine.CreateSubKey("Software\\MFA", true);
                }
                ek.SetValue("PrivateKeysCleanUpEnabled", 1, RegistryValueKind.DWord);
                ek.SetValue("PrivateKeysCleanUpDelay", delay, RegistryValueKind.DWord);

            }
            if (option == 0x02)  // Disable
            {
                RegistryKey dk = Registry.LocalMachine.OpenSubKey("Software\\MFA", true);
                if (dk == null)
                {
                    dk = Registry.LocalMachine.CreateSubKey("Software\\MFA", true);
                }
                dk.SetValue("PrivateKeysCleanUpEnabled", 0, RegistryValueKind.DWord);
            }
        }
        #endregion

        #region ACLs
        /// <summary>
        /// UpdateCertificatesACL method implementation
        /// </summary>
        public static bool UpdateCertificatesACL(KeyMgtOptions options = KeyMgtOptions.AllCerts)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.MaxAllowed);
            try
            {
                X509Certificate2Collection collection2 = (X509Certificate2Collection)store.Certificates;
                foreach (X509Certificate2 x509 in collection2)
                {
                    string fileName = string.Empty;
                    try
                    {
                        bool doit = options.Equals(KeyMgtOptions.AllCerts);
                        if (options.HasFlag(KeyMgtOptions.MFACerts))
                        {
                            if (x509.Subject.ToLower().StartsWith("cn=mfa rsa keys") || x509.Subject.ToLower().StartsWith("cn=mfa sql key"))
                                doit = true;
                        }
                        if (options.HasFlag(KeyMgtOptions.ADFSCerts))
                        {
                            if (x509.Subject.ToLower().StartsWith("cn=adfs"))
                                doit = true;
                        }
                        if (options.HasFlag(KeyMgtOptions.SSLCerts))
                        {
                            if (x509.Subject.ToLower().StartsWith("cn=*."))
                                doit = true;
                        }
                        if (doit)
                        {
                            var rsakey = x509.GetRSAPrivateKey();
                            if (rsakey is RSACng)
                            {
                                fileName = ((RSACng)rsakey).Key.UniqueName;
                            }
                            else if (rsakey is RSACryptoServiceProvider)
                            {
                                fileName = ((RSACryptoServiceProvider)rsakey).CspKeyContainerInfo.UniqueKeyContainerName;
                            }
                            if (!string.IsNullOrEmpty(fileName))
                            {
                                char sep = Path.DirectorySeparatorChar;                                
                                string machinefullpath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + sep + "Microsoft" + sep + "Crypto" + sep + "RSA" + sep + "MachineKeys" + sep + fileName;
                                if (File.Exists(machinefullpath))
                                    InternalUpdateACLs(machinefullpath);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                store.Close();
            }
            return true;
        }

        /// <summary>
        /// InternalUpdateACLs method implementation
        /// </summary>
        private static void InternalUpdateACLs(string fullpath)
        {
            FileSecurity fSecurity = File.GetAccessControl(fullpath, AccessControlSections.Access);

            SecurityIdentifier localsys = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
            fSecurity.AddAccessRule(new FileSystemAccessRule(localsys, FileSystemRights.FullControl, AccessControlType.Allow));

            SecurityIdentifier localacc = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            fSecurity.AddAccessRule(new FileSystemAccessRule(localacc, FileSystemRights.FullControl, AccessControlType.Allow));

            if (!string.IsNullOrEmpty(ADFSAccountSID))
            {
                SecurityIdentifier adfsacc = new SecurityIdentifier(ADFSAccountSID);
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsacc, FileSystemRights.FullControl, AccessControlType.Allow));
            }
            if (!string.IsNullOrEmpty(ADFSServiceSID))
            {
                SecurityIdentifier adfsserv = new SecurityIdentifier(ADFSServiceSID);
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsserv, FileSystemRights.FullControl, AccessControlType.Allow));
            }
            if (!string.IsNullOrEmpty(ADFSAdminGroupSID))
            {
                SecurityIdentifier adfsgroup = new SecurityIdentifier(ADFSAdminGroupSID);
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsgroup, FileSystemRights.FullControl, AccessControlType.Allow));
            }
            File.SetAccessControl(fullpath, fSecurity);
        }
        #endregion

        #region SIDs
        /// <summary>
        /// InitializeAccountsSID method implementation
        /// </summary>
        public static void InitializeAccountsSID(string domain, string account, string password)
        {
            ADFSAccountSID = GetADFSAccountSID(domain, account, password);
            ADFSServiceSID = GetADFSServiceSID();
            ADFSAdminGroupSID = GetADFSAdminsGroupSID(domain, account, password);
        }

        /// <summary>
        /// GetADFSServiceSID method implmentation
        /// </summary>
        private static string GetADFSServiceSID()
        {
            try
            {
                IntPtr ptr = GetServiceSidPtr("adfssrv");
                return new SecurityIdentifier(ptr).Value;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// GetADFSAccountSID() method implmentation
        /// </summary>
        private static string GetADFSAccountSID(string domain, string account, string password)
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\adfssrv");
            try
            {
                if (key != null)
                {
                    string username = key.GetValue("ObjectName").ToString();
                    SecurityIdentifier sid;
                    NTAccount ntaccount;
                    try
                    {
                        ntaccount = new NTAccount(domain, username);
                        sid = (SecurityIdentifier)ntaccount.Translate(typeof(SecurityIdentifier));
                        return sid.Value;
                    }
                    catch (Exception)
                    {
                        ntaccount = new NTAccount(username);
                        sid = (SecurityIdentifier)ntaccount.Translate(typeof(SecurityIdentifier));
                        return sid.Value;
                    }
                }
                else
                    return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
            finally
            {
                key.Close();
            }
        }

        /// <summary>
        /// GetADFSAdminsGroupSID() method implmentation
        /// </summary>
        private static string GetADFSAdminsGroupSID(string domain, string account, string password)
        {                   
            try
            {
                string admingroupname = GetADFSDelegateServiceAdministration();
                if (!string.IsNullOrEmpty(admingroupname))
                {
                    PrincipalContext ctx;
                    if (!string.IsNullOrEmpty(account))
                       ctx = new PrincipalContext(ContextType.Domain, domain, account, password);
                    else
                        ctx = new PrincipalContext(ContextType.Domain, domain);
                    GroupPrincipal group = GroupPrincipal.FindByIdentity(ctx, admingroupname);
                    if (group != null)
                    {
                        SecurityIdentifier sid = group.Sid;
                        ADFSAdminGroupSID = sid.Value;
                        return sid.Value;
                    }
                    else
                        return string.Empty;
                }
                else
                    return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// GetServiceSidPtr method implementation
        /// </summary>
        private static IntPtr GetServiceSidPtr(string service)
        {
            NativeMethods.LSA_UNICODE_STRING lSA_UNICODE_STRING = default(NativeMethods.LSA_UNICODE_STRING);
            lSA_UNICODE_STRING.SetTo(service);
            int cb = 0;
            IntPtr intPtr = IntPtr.Zero;
            IntPtr result;
            try
            {
                uint num = NativeMethods.RtlCreateServiceSid(ref lSA_UNICODE_STRING, IntPtr.Zero, ref cb);
                if (num == 3221225507u)
                {
                    intPtr = Marshal.AllocHGlobal(cb);
                    num = NativeMethods.RtlCreateServiceSid(ref lSA_UNICODE_STRING, intPtr, ref cb);
                }
                if (num != 0u)
                {
                    throw new Win32Exception(Convert.ToInt32(num));
                }
                result = intPtr;
            }
            finally
            {
                lSA_UNICODE_STRING.Dispose();
            }
            return result;
        }

        /// <summary>
        /// IsWIDConfiguration method implmentation
        /// </summary>
        private static string GetADFSDelegateServiceAdministration()
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            string grpname = string.Empty;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("(Get-AdfsProperties).DelegateServiceAdministration", true);
                pipeline.Commands.Add(exportcmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();
                foreach (var result in PSOutput)
                {
                    grpname = result.BaseObject.ToString();
                    return grpname.ToLower();
                }
            }
            catch (Exception)
            {
                grpname = string.Empty;
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
            return grpname;
        }

        /// <summary>
        /// NativeMethod implementation
        /// The class exposes Windows APIs.
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        internal class NativeMethods
        {
            [DllImport("ntdll.dll")]
            internal static extern uint RtlCreateServiceSid(ref NativeMethods.LSA_UNICODE_STRING serviceName, IntPtr serviceSid, ref int serviceSidLength);

            internal struct LSA_UNICODE_STRING : IDisposable
            {
                public ushort Length;
                public ushort MaximumLength;
                public IntPtr Buffer;

                public void SetTo(string str)
                {
                    this.Buffer = Marshal.StringToHGlobalUni(str);
                    this.Length = (ushort)(str.Length * 2);
                    this.MaximumLength = Convert.ToUInt16(this.Length + 2);
                }

                public override string ToString()
                {
                    return Marshal.PtrToStringUni(this.Buffer);
                }

                public void Reset()
                {
                    if (this.Buffer != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(this.Buffer);
                    }
                    this.Buffer = IntPtr.Zero;
                    this.Length = 0;
                    this.MaximumLength = 0;
                }

                public void Dispose()
                {
                    this.Reset();
                }
            }
        }
        #endregion 
    }

    #region Encoding
    /// <summary>
    /// CheckSumEncoding class implementation
    /// </summary>
    public static class CheckSumEncoding
    {
        /// <summary>
        /// EncodeUserID 
        /// </summary>
        public static byte[] EncodeUserID(int challengesize, string username)
        {
            switch (challengesize)
            {
                case 16:
                    return CheckSum128(username);
                case 20:
                    return CheckSum160(username);
                case 32:
                    return CheckSum256(username);
                case 48:
                    return CheckSum384(username);
                case 64:
                    return CheckSum512(username);
                default:
                    return CheckSum128(username);
            }
        }

        /// <summary>
        /// EncodeByteArray 
        /// </summary>
        public static byte[] EncodeByteArray(int challengesize, byte[] data)
        {
            switch (challengesize)
            {
                case 16:
                    return CheckSum128(data);
                case 20:
                    return CheckSum160(data);
                case 32:
                    return CheckSum256(data);
                case 48:
                    return CheckSum384(data);
                case 64:
                    return CheckSum512(data);
                default:
                    return CheckSum128(data);
            }
        }

        /// <summary>
        /// CheckSum128 method implementation
        /// </summary>
        public static byte[] CheckSum128(byte[] value)
        {
            byte[] hash = null;
            using (MD5 md5 = MD5Cng.Create())
            {
                hash = md5.ComputeHash(value);
            }
            return hash;
        }

        /// <summary>
        /// CheckSum128 method implementation
        /// </summary>
        public static byte[] CheckSum128(string value)
        {
            byte[] hash = null;
            using (MD5 md5 = MD5.Create())
            {
                hash = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
            return hash;
        }

        /// <summary>
        /// CheckSum160 method implementation
        /// </summary>
        public static byte[] CheckSum160(byte[] value)
        {
            byte[] hash = null;
            using (SHA1 sha1 = SHA1Cng.Create())
            {
                hash = sha1.ComputeHash(value);
            }
            return hash;
        }

        /// <summary>
        /// CheckSum160 method implementation
        /// </summary>
        public static byte[] CheckSum160(string value)
        {
            byte[] hash = null;
            using (SHA1 sha1 = SHA1Cng.Create())
            {
                hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
            return hash;
        }

        /// <summary>
        /// CheckSum256 method implementation
        /// </summary>
        public static byte[] CheckSum256(byte[] value)
        {
            byte[] hash = null;
            using (SHA256 sha256 = SHA256Cng.Create())
            {
                hash = sha256.ComputeHash(value);
            }
            return hash;
        }

        /// <summary>
        /// CheckSum256 method implementation
        /// </summary>
        public static byte[] CheckSum256(string value)
        {
            byte[] hash = null;
            using (SHA256 sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
            return hash;
        }

        /// <summary>
        /// CheckSum384 method implementation
        /// </summary>
        public static byte[] CheckSum384(byte[] value)
        {
            byte[] hash = null;
            using (SHA384 sha384 = SHA384Cng.Create())
            {
                hash = sha384.ComputeHash(value);
            }
            return hash;
        }

        /// <summary>
        /// CheckSum384 method implementation
        /// </summary>
        public static byte[] CheckSum384(string value)
        {
            byte[] hash = null;
            using (SHA384 sha384 = SHA384Managed.Create())
            {
                hash = sha384.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
            return hash;
        }

        /// <summary>
        /// CheckSum512 method implementation
        /// </summary>
        public static byte[] CheckSum512(byte[] value)
        {
            byte[] hash = null;
            using (SHA512 sha512 = SHA512Cng.Create())
            {
                hash = sha512.ComputeHash(value);
            }
            return hash;
        }

        /// <summary>
        /// CheckSum512 method implementation
        /// </summary>
        public static byte[] CheckSum512(string value)
        {
            byte[] hash = null;
            using (SHA512 sha512 = SHA512Managed.Create())
            {
                hash = sha512.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
            return hash;
        }

        /// <summary>
        /// CheckSum method implementation
        /// </summary>
        public static byte[] CheckSum(string value)
        {
            byte[] hash = null;
            using (MD5 md5 = MD5.Create())
            {
                hash = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
            return hash;
        }

        /// <summary>
        /// CheckSum method implementation
        /// </summary>
        public static string CheckSumAsString(string value)
        {
            string hash = null;
            using (MD5 md5 = MD5.Create())
            {
                hash = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(value)));
            }
            return hash.Replace("-", String.Empty);
        }

    }

    /// <summary>
    /// HexaEncoding static class
    /// </summary>
    public static class HexaEncoding
    {
        /// <summary>
        /// GetByteArrayFromHexString method
        /// </summary>
        public static byte[] GetByteArrayFromHexString(String value)
        {
            int len = value.Length;
            byte[] bytes = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
                bytes[i / 2] = Convert.ToByte(value.Substring(i, 2), 16);
            return bytes;
        }

        /// <summary>
        /// GetHexStringFromByteArray method
        /// </summary>
        public static string GetHexStringFromByteArray(byte[] data)
        {
            int len = data.Length;
            StringBuilder builder = new StringBuilder(len * 2);
            foreach (byte b in data)
            {
                builder.AppendFormat("{0:x2}", b);
            }
            return builder.ToString().ToUpper();
        }
    }
    #endregion

    #region ADFS Version
    internal class RegistryVersion
    {
        /// <summary>
        /// RegistryVersion constructor
        /// </summary>
        public RegistryVersion(bool loadlocal = true)
        {
            if (loadlocal)
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion");

                CurrentVersion = Convert.ToString(rk.GetValue("CurrentVersion"));
                ProductName = Convert.ToString(rk.GetValue("ProductName"));
                InstallationType = Convert.ToString(rk.GetValue("InstallationType"));
                CurrentBuild = Convert.ToInt32(rk.GetValue("CurrentBuild"));
                CurrentMajorVersionNumber = Convert.ToInt32(rk.GetValue("CurrentMajorVersionNumber"));
                CurrentMinorVersionNumber = Convert.ToInt32(rk.GetValue("CurrentMinorVersionNumber"));
            }
        }

        /// <summary>
        /// CurrentVersion property implementation
        /// </summary>
        public string CurrentVersion { get; set; }

        /// <summary>
        /// ProductName property implementation
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// InstallationType property implementation
        /// </summary>
        public string InstallationType { get; set; }

        /// <summary>
        /// CurrentBuild property implementation
        /// </summary>
        public int CurrentBuild { get; set; }

        /// <summary>
        /// CurrentMajorVersionNumber property implementation
        /// </summary>
        public int CurrentMajorVersionNumber { get; set; }

        /// <summary>
        /// CurrentMinorVersionNumber property implementation
        /// </summary>
        public int CurrentMinorVersionNumber { get; set; }

        /// <summary>
        /// IsWindows2019 property implementation
        /// </summary>
        public bool IsWindows2019
        {
            get { return ((this.CurrentMajorVersionNumber == 10) && (this.CurrentBuild >= 17763)); }
        }

        /// <summary>
        /// IsWindows2016 property implementation
        /// </summary>
        public bool IsWindows2016
        {
            get { return ((this.CurrentMajorVersionNumber == 10) && ((this.CurrentBuild >= 14393) && (this.CurrentBuild < 17763))); }
        }

        /// <summary>
        /// IsWindows2012R2 property implementation
        /// </summary>
        public bool IsWindows2012R2
        {
            get { return ((this.CurrentMajorVersionNumber == 0) && ((this.CurrentBuild >= 9600) && (this.CurrentBuild < 14393))); }
        }
    }
    #endregion
}
