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
//                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using Neos.IdentityServer.MultiFactor.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Neos.IdentityServer.MultiFactor
{
    #region replay service
    [ServiceContract(Namespace ="http://adfsmfa.org", Name ="Replay")]
    public interface IReplay
    {
        [OperationContract]
        bool Check(List<string> computers, ReplayRecord record);

        [OperationContract]
        void Reset(List<string> computers);
    }

    public interface IDependency
    {
        EventLog GetEventLog();
    }
    #endregion

    #region WebThemes service
    [DataContract]
    public enum WebThemeAddressKind
    {
        [EnumMember]
        Illustration = 0,

        [EnumMember]
        CompanyLogo = 1,

        [EnumMember]
        StyleSheet = 2
    }

    /// <summary>
    /// WebThemesParametersRecord class implementation
    /// </summary>
    [DataContract]
    public class WebThemesParametersRecord
    {
        [DataMember]
        public int LCID { get; set; }

        [DataMember]
        public string Identifier { get; set; }
    }

    /// <summary>
    /// WebThemeRecord class implementation
    /// </summary>
    [DataContract]
    public class WebThemeRecord
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Identifier { get; set; }

        [DataMember]
        public string ObjectId { get; set; }

        [DataMember]
        public bool IsBuiltinTheme { get; set; } = false;

        [DataMember]
        public IDictionary<string, string> StyleSheetUri { get; set; }

        [DataMember]
        public IDictionary<string, string> LogoImageUri { get; set; }

        [DataMember]
        public IDictionary<string, string> IllustrationImageUri { get; set; }
    }

    [ServiceContract(Namespace = "http://adfsmfa.org", Name = "WebThemes")]
    public interface IWebThemeManager
    {
        [OperationContract]
        bool Initialize(Dictionary<string, bool> servers, WebThemesParametersRecord context, string request, out string identifier);

        [OperationContract]
        bool HasRelyingPartyTheme(WebThemesParametersRecord context);

        [OperationContract]
        string GetIllustrationAddress(WebThemesParametersRecord context);

        [OperationContract]
        string GetLogoAddress(WebThemesParametersRecord context);

        [OperationContract]
        string GetStyleSheetAddress(WebThemesParametersRecord context);

        [OperationContract]
        Dictionary<WebThemeAddressKind, string> GetAddresses(WebThemesParametersRecord context);

        [OperationContract]
        void DispachTheme(WebThemeRecord theme);

        [OperationContract]
        WebThemeRecord RequestTheme(WebThemesParametersRecord theme);

        [OperationContract]
        void ResetThemes();

        [OperationContract]
        void ResetThemesList(Dictionary<string, bool> servers);

    }
    #endregion

    #region IWebAdminServices
    /// <summary>
    /// SIDsParametersRecord class implementation
    /// </summary>
    [DataContract]
    public class SIDsParametersRecord
    {
        [DataMember]
        public bool Loaded { get; set; }

        [DataMember]
        public bool ADFSSystemServiceAdministrationAllowed { get; set; }

        [DataMember]
        public bool ADFSLocalAdminServiceAdministrationAllowed { get; set; }

        [DataMember]
        public bool ADFSDelegateServiceAdministrationAllowed { get; set; }

        [DataMember]
        public string ADFSAccountSID { get; set; }

        [DataMember]
        public string ADFSAccountName { get; set; }

        [DataMember]
        public string ADFSServiceAccountSID { get; set; }

        [DataMember]
        public string ADFSServiceAccountName { get; set; }

        [DataMember]
        public string ADFSAdministrationGroupSID { get; set; }

        [DataMember]
        public string ADFSAdministrationGroupName { get; set; }

    }

    [ServiceContract(Namespace = "http://adfsmfa.org", Name = "WebAdminService")]
    public interface IWebAdminServices
    {
        [OperationContract]
        SIDsParametersRecord GetSIDsInformations(Dictionary<string, bool> servers);

        [OperationContract]
        SIDsParametersRecord RequestSIDsInformations();

        [OperationContract(IsOneWay = true)] // ??? 
        void PushSIDsInformations(SIDsParametersRecord rec);

        [OperationContract]
        void UpdateDirectoriesACL(string path);

        [OperationContract]
        bool UpdateCertificatesACL(KeyMgtOptions options);

        [OperationContract]
        bool CertificateExists(string thumbprint, byte location);

        [OperationContract]
        string CreateRSACertificate(Dictionary<string, bool> servers, string subject, int years);

        [OperationContract]
        string CreateRSACertificateForSQLEncryption(Dictionary<string, bool> servers, string subject, int years);

        [OperationContract]
        bool CreateADFSCertificate(Dictionary<string, bool> servers, string subject, bool issigning, int years);

        [OperationContract]
        void PushCertificate(string cert);

        [OperationContract]
        int CleanOrphanedPrivateKeys(byte option, int delay);

        [OperationContract]
        void AddFirewallRules(string computers);

        [OperationContract]
        void RemoveFirewallRules();

        [OperationContract]
        bool ExportMailTemplates(Dictionary<string, bool> servers, byte[] config, int lcid, Dictionary<string, string> templates, bool dispatch = true);

        [OperationContract]
        void BroadcastNotification(Dictionary<string, bool> servers, byte[] config, NotificationsKind kind, string message, bool local = true, bool dispatch = true, bool mustwrite = false);

        [OperationContract]
        bool NewMFASystemMasterKey(Dictionary<string, bool> servers, bool deployonly = false, bool deleteonly = false);

        [OperationContract]
        void PushMFASystemMasterkey(byte[] data, bool deleteonly = false);

        [OperationContract]
        bool ExistsMFASystemMasterkey();

        [OperationContract]
        bool NewMFASystemAESCngKey(Dictionary<string, bool> servers, bool deployonly = false, bool deleteonly = false);

        [OperationContract]
        void PushMFASystemAESCngKey(byte[] bobdata, byte[] alicedata, bool deleteonly = false);

        [OperationContract]
        bool ExistsMFASystemAESCngKeys();

        [OperationContract]
        ADFSServerHost GetComputerInformations(string servername = "localhost");

        [OperationContract]
        Dictionary<string, ADFSServerHost> GetAllComputersInformations();

        [OperationContract]
        RegistryVersion GetRegistryInformations();
    }

    [ServiceContract(Namespace = "http://adfsmfa.org", Name = "NTService")]
    public interface INTService
    {
        [OperationContract]
        bool Start(string name, string machinename = null);

        [OperationContract]
        bool Stop(string name, string machinename = null);

        [OperationContract]
        bool Pause(string name, string machinename = null);

        [OperationContract]
        bool Continue(string name, string machinename = null);

        [OperationContract]
        bool IsRunning(string name, string machinename);

        [OperationContract]
        bool Exists(string name, string machinename);
    }
    #endregion
}
