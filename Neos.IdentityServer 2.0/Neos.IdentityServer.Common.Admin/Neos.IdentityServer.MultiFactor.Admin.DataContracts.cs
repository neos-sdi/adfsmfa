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
// https://adfsmfa.codeplex.com                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Runtime.Serialization;
using System.Management.Automation.Host;
using System.Xml;

namespace Neos.IdentityServer.MultiFactor.Administration
{
    #region MMC Classes
    /// <summary>
    /// ServiceOperationStatus enum
    /// </summary>
    public enum ServiceOperationStatus
    {
        OperationUnknown,
        OperationPending,
        OperationRunning,
        OperationStopped,
        OperationInError
    }

    /// <summary>
    /// ServiceOperationStatus enum
    /// </summary>
    public enum ConfigOperationStatus
    {
        ConfigUnknown,
        ConfigLoaded,
        ConfigIsDirty,
        ConfigSaved,
        ConfigInError
    }

    /// <summary>
    /// UsersFilterField enum
    /// </summary>
    public enum UsersFilterField
    {
        UserName = 0,
        Email = 1,
        PhoneNumber = 2
    }

    /// <summary>
    /// UsersOrderField enum
    /// </summary>
    public enum UsersOrderField
    {
        None = 0,
        UserName = 1,
        Email = 2,
        Phone = 3,
        CreationDate = 4,
        ID = 5
    }

    /// <summary>
    /// UsersFilterFieldItem class
    /// </summary>
    public class UsersFilterFieldItem
    {
        public UsersFilterField ID { get; set; }
        public String Label { get; set; }
    }

    /// <summary>
    /// UsersFilterFieldList class implémentation
    /// </summary>
    public class UsersFilterFieldList : BindingList<UsersFilterFieldItem>
    {
        public UsersFilterFieldList()
        {
            this.Add(new UsersFilterFieldItem() { ID = UsersFilterField.UserName, Label = "Compte utilisateur" });
            this.Add(new UsersFilterFieldItem() { ID = UsersFilterField.Email, Label = "Adresse email" });
            this.Add(new UsersFilterFieldItem() { ID = UsersFilterField.PhoneNumber, Label = "N° de téléphone" });
        }
    }

    /// <summary>
    /// UsersFilterOperators
    /// </summary>
    public enum UsersFilterOperator
    {
        Equal = 0,
        StartWith = 1,
        Contains = 2,
        NotEqual = 3,
        EndsWith = 4,
        NotContains = 5
    }

    /// <summary>
    /// UsersFilterOperatorItem class
    /// </summary>
    public class UsersFilterOperatorItem
    {
        public UsersFilterOperator ID { get; set; }
        public String Label { get; set; }
    }

    /// <summary>
    /// UsersFilterOperatorList class implémentation
    /// </summary>
    public class UsersFilterOperatorList : BindingList<UsersFilterOperatorItem>
    {
        public UsersFilterOperatorList()
        {
            this.Add(new UsersFilterOperatorItem() { ID = UsersFilterOperator.Equal, Label = "Est égal à" });
            this.Add(new UsersFilterOperatorItem() { ID = UsersFilterOperator.StartWith, Label = "Commence par" });
            this.Add(new UsersFilterOperatorItem() { ID = UsersFilterOperator.Contains, Label = "Contient" });
            this.Add(new UsersFilterOperatorItem() { ID = UsersFilterOperator.NotEqual, Label = "N'est pas égal à" });
            this.Add(new UsersFilterOperatorItem() { ID = UsersFilterOperator.EndsWith, Label = "Termine par" });
            this.Add(new UsersFilterOperatorItem() { ID = UsersFilterOperator.NotContains, Label = "Ne contient pas" });
        }
    }

    /// <summary>
    /// UsersPreferredMethod
    /// </summary>
    public enum UsersPreferredMethod
    {
        Choose = 0,
        Application = 1,
        Email = 2,
        Phone = 3,
        None = 4
    }

    /// <summary>
    /// UsersPreferredMethodItem class implémentation
    /// </summary>
    public class UsersPreferredMethodItem
    {
        public UsersPreferredMethod ID { get; set; }
        public String Label { get; set; }
    }

    /// <summary>
    /// UsersPreferredMethodItem class implémentation
    /// </summary>
    public class UsersPreferredMethodList : BindingList<UsersPreferredMethodItem>
    {

        public UsersPreferredMethodList() : this(false) { }

        public UsersPreferredMethodList(bool allowNone)
        {
            this.Add(new UsersPreferredMethodItem() { ID = UsersPreferredMethod.Choose, Label = "Choisir la meilleure méthode" });
            this.Add(new UsersPreferredMethodItem() { ID = UsersPreferredMethod.Application, Label = "Utiliser une application sur mobile ou pc" });
            this.Add(new UsersPreferredMethodItem() { ID = UsersPreferredMethod.Email, Label = "Recevoir un email" });
            this.Add(new UsersPreferredMethodItem() { ID = UsersPreferredMethod.Phone, Label = "Recevoir un message sur mon téléphone" });
            if (allowNone)
                this.Add(new UsersPreferredMethodItem() { ID = UsersPreferredMethod.None, Label = "(aucune)" });
        }
    }

    /// <summary>
    /// UsersOrderObject class
    /// </summary>
    public class UsersOrderObject
    {
        private UsersOrderField _order = UsersOrderField.UserName;
        private SortDirection _sortorder = SortDirection.Ascending;

        public UsersOrderField Column
        {
            get { return _order; }
            set { _order = value; }
        }

        public SortDirection Direction
        {
            get { return _sortorder; }
            set { _sortorder = value; }
        }
    }

    /// <summary>
    /// UserPagingObject class
    /// </summary>
    public class UsersPagingObject
    {
        private int _currentpage = 0;
        private int _pagesize = 50;
        private bool _isactive = false;
        private bool _isrecurse = false;

        public void Clear()
        {
            _currentpage = 0;
            _pagesize = 50;
            _isactive = false;
            _isrecurse = false;
        }

        public bool isActive
        {
            get { return _isactive; }
        }

        public int CurrentPage
        {
            get { return _currentpage; }
            set 
            {
                if (value < 0)
                    value = 0;
                _currentpage = value;
                _isactive = _currentpage > 0;
            }
        }

        public int PageSize
        {
            get { return _pagesize; }
            set 
            {
                if (value > 0)
                {
                    if (value >= int.MaxValue)
                        _pagesize = int.MaxValue;
                    else
                        _pagesize = value;
                }
            }
        }

        public bool IsRecurse
        {
            get { return _isrecurse; }
            set { _isrecurse = value; }
        }
    }

    /// <summary>
    /// UsersFilterObject class
    /// </summary>
    [DataContract, Serializable]
    public class UsersFilterObject
    {
        private UsersFilterField filterfield = UsersFilterField.UserName;
        private UsersFilterOperator filteroperator = UsersFilterOperator.Contains;
        private UsersPreferredMethod filtermethod = UsersPreferredMethod.None;
        private string filtervalue = string.Empty;
        private bool enabledonly = true;
        private bool filterisactive = true;


        /// <summary>
        /// implicit conversion to byte array
        /// </summary>
        public static explicit operator byte[](UsersFilterObject filterobj)
        {
            if (filterobj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, filterobj);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// implicit conversion from ResultNode
        /// </summary>
        public static explicit operator UsersFilterObject(byte[] data)
        {
            if (data == null)
                return null;
            using (MemoryStream memStream = new MemoryStream())
            {
                BinaryFormatter binForm = new BinaryFormatter();
                memStream.Write(data, 0, data.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                return (UsersFilterObject)binForm.Deserialize(memStream);
            }
        }

        public void Clear()
        {
            filterfield = UsersFilterField.UserName;
            filteroperator = UsersFilterOperator.Contains;
            filtermethod = UsersPreferredMethod.None;
            filtervalue = string.Empty;
            enabledonly = false;
            filterisactive = false;
        }

        [DataMember]
        public UsersFilterField FilterField
        {
            get { return filterfield; }
            set { filterfield = value; }
        }

        [DataMember]
        public UsersFilterOperator FilterOperator
        {
            get { return filteroperator; }
            set { filteroperator = value; }
        }

        [DataMember]
        public UsersPreferredMethod FilterMethod
        {
            get { return filtermethod; }
            set 
            { 
                filtermethod = value;
                CheckForActiveFilter();
            }
        }

        [DataMember]
        public string FilterValue
        {
            get { return filtervalue; }
            set 
            { 
                filtervalue = value;
                CheckForActiveFilter();
            }
        }

        [DataMember]
        public bool EnabledOnly
        {
            get { return enabledonly; }
            set 
            { 
                enabledonly = value;
                CheckForActiveFilter();
            }
        }

        public bool FilterisActive
        {
            get { return filterisactive; }
         //   set { filterisactive = value; }
        }

        private void CheckForActiveFilter()
        {
            filterisactive = false;
            if (string.Empty != filtervalue)
                filterisactive = true;
            if (filtermethod != UsersPreferredMethod.None)
                filterisactive = true;
            if (enabledonly)
                filterisactive = true;
        }
    }

    [Serializable]
    public class MMCRegistration : Registration
    {
        /// <summary>
        /// IsApplied
        /// </summary>
        public bool IsApplied
        {
            get;
            set;
        }
    }

    [Serializable]
    public class MMCRegistrationList : List<MMCRegistration>
    {
        /// <summary>
        /// implicit conversion to byte array
        /// </summary>
        public static implicit operator byte[](MMCRegistrationList registrations)
        {
            if (registrations == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, registrations);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// implicit conversion from ResultNode
        /// </summary>
        public static implicit operator MMCRegistrationList(byte[] data)
        {
            if (data == null)
                return null;
            using (MemoryStream memStream = new MemoryStream())
            {
                BinaryFormatter binForm = new BinaryFormatter();
                memStream.Write(data, 0, data.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                return (MMCRegistrationList)binForm.Deserialize(memStream);
            }
        }
    }

    [Serializable]
    public enum MMCSecretKeyFormat
    {
        [EnumMember]
        RNG = 0,
        [EnumMember]
        RSA = 1,
        [EnumMember]
        CUSTOM = 2
    }

    [Serializable]
    public enum MMCTemplateMode
    {
        Free = 0,                        // (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.BypassUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);
        Open = 1,                        // (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);
        Default = 2,                     // (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);
        Managed = 3,                     // (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowProvideInformations | UserFeaturesOptions.AllowChangePassword);
        Strict = 4,                      // (UserFeaturesOptions.AllowProvideInformations);
        Administrative = 5,              // (UserFeaturesOptions.AdministrativeMode);
        Custom = 6                       // Empty 
    }

    /// <summary>
    /// MMCTemplateModeItem class
    /// </summary>
    public class MMCTemplateModeItem
    {
        public MMCTemplateMode ID { get; set; }
        public String Label { get; set; }
    }

    /// <summary>
    /// MMCTemplateModeList class implémentation
    /// </summary>
    public class MMCTemplateModeList : BindingList<MMCTemplateModeItem>
    {
        public MMCTemplateModeList()
        {
            this.Add(new MMCTemplateModeItem() { ID = MMCTemplateMode.Free, Label = "Free template" });
            this.Add(new MMCTemplateModeItem() { ID = MMCTemplateMode.Open, Label = "Open template" });
            this.Add(new MMCTemplateModeItem() { ID = MMCTemplateMode.Default, Label = "Default template" });
            this.Add(new MMCTemplateModeItem() { ID = MMCTemplateMode.Managed, Label = "Managed template" });
            this.Add(new MMCTemplateModeItem() { ID = MMCTemplateMode.Strict, Label = "Strict template" });
            this.Add(new MMCTemplateModeItem() { ID = MMCTemplateMode.Administrative, Label = "Administrative template" });
            this.Add(new MMCTemplateModeItem() { ID = MMCTemplateMode.Custom, Label = "Custom template" });
        }
    }

    [Serializable]
    public class MMCConfig
    {
        public bool IsDirty { get; set; }
        public int RefreshScan { get; set; }
        public int DeliveryWindow { get; set; }
        public int TOTPShadows { get; set; }
        public bool MailEnabled { get; set; }
        public bool SMSEnabled { get; set; }
        public bool AppsEnabled { get; set; }
        public HashMode Algorithm { get; set; }
        public string Issuer { get; set; }
        public bool UseActiveDirectory { get; set; }
        public bool CustomUpdatePassword { get; set; }
        public string DefaultCountryCode { get; set; }
        public string AdminContact { get; set; }
        public UserFeaturesOptions UserFeatures { get; set; }
        public MFAConfigAdvertising AdvertisingDays { get; set; }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementAdminService.Initialize(host, true);
            MFAConfig cfg = ManagementAdminService.ADFSManager.ReadConfiguration(host);
            AdminContact = cfg.AdminContact;
            IsDirty = cfg.IsDirty;
            RefreshScan = cfg.RefreshScan;
            DeliveryWindow = cfg.DeliveryWindow;
            TOTPShadows = cfg.TOTPShadows;
            MailEnabled = cfg.MailEnabled;
            SMSEnabled = cfg.SMSEnabled;
            AppsEnabled = cfg.AppsEnabled;
            Algorithm = cfg.Algorithm;
            Issuer = cfg.Issuer;
            UseActiveDirectory = cfg.UseActiveDirectory;
            CustomUpdatePassword = cfg.CustomUpdatePassword;
            DefaultCountryCode = cfg.DefaultCountryCode;
            AdminContact = cfg.AdminContact;
            UserFeatures = cfg.UserFeatures;
            AdvertisingDays = cfg.AdvertisingDays;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementAdminService.Initialize(true);
            MFAConfig cfg = ManagementAdminService.ADFSManager.Config;
            cfg.AdminContact = AdminContact;
            cfg.IsDirty = IsDirty;
            cfg.RefreshScan = RefreshScan;
            cfg.DeliveryWindow = DeliveryWindow;
            cfg.TOTPShadows = TOTPShadows;
            cfg.MailEnabled = MailEnabled;
            cfg.SMSEnabled = SMSEnabled;
            cfg.AppsEnabled = AppsEnabled;
            cfg.Algorithm = Algorithm;
            cfg.Issuer = Issuer;
            cfg.UseActiveDirectory = UseActiveDirectory;
            cfg.CustomUpdatePassword = CustomUpdatePassword;
            cfg.DefaultCountryCode = DefaultCountryCode;
            cfg.AdminContact = AdminContact;
            cfg.UserFeatures = UserFeatures;
            cfg.AdvertisingDays = AdvertisingDays;
            ManagementAdminService.ADFSManager.WriteConfiguration(host);
            using (MailSlotClient mailslot = new MailSlotClient())
            {
                mailslot.SendNotification(0xAA);
            }
        }

        /// <summary>
        /// SetTemplate method implmentation
        /// </summary>
        public void SetTemplate(PSHost host, MMCTemplateMode mode)
        {
            ManagementAdminService.Initialize(true);
            MFAConfig cfg = ManagementAdminService.ADFSManager.Config;
            switch (mode)
            {
                case MMCTemplateMode.Free:
                    cfg.UserFeatures = (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.BypassUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);
                    break;
                case MMCTemplateMode.Open:
                    cfg.UserFeatures = (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);
                    break;
                case MMCTemplateMode.Default:
                    cfg.UserFeatures = (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);
                    break;
                case MMCTemplateMode.Managed:
                    cfg.UserFeatures = (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowProvideInformations | UserFeaturesOptions.AllowChangePassword);
                    break;
                case MMCTemplateMode.Strict:
                    cfg.UserFeatures = (UserFeaturesOptions.AllowProvideInformations);
                    break;
                case MMCTemplateMode.Administrative:
                    cfg.UserFeatures = (UserFeaturesOptions.AdministrativeMode);
                    break;
            }
            ManagementAdminService.ADFSManager.WriteConfiguration(host);
            using (MailSlotClient mailslot = new MailSlotClient())
            {
                mailslot.SendNotification(0xAA);
            }
        }
    }

    [Serializable]
    public class MMCConfigSQL
    {
        public bool IsDirty { get; set; }
        public string ConnectionString { get; set; }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementAdminService.Initialize(host, true);
            MFAConfig cfg = ManagementAdminService.ADFSManager.ReadConfiguration(host);
            SQLServerHost sql = cfg.Hosts.SQLServerHost;
            IsDirty = cfg.IsDirty;
            ConnectionString = sql.ConnectionString;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementAdminService.Initialize(true);
            MFAConfig cfg = ManagementAdminService.ADFSManager.Config;
            SQLServerHost sql = cfg.Hosts.SQLServerHost;
            cfg.IsDirty = IsDirty;
            sql.ConnectionString = ConnectionString;
            ManagementAdminService.ADFSManager.WriteConfiguration(host);
            using (MailSlotClient mailslot = new MailSlotClient())
            {
                mailslot.SendNotification(0xAA);
            }
        }
    }

    [Serializable]
    public class MMCConfigADDS
    {
        public bool IsDirty { get; set; }
        public string Account { get; set; }
        public string DomainAddress { get; set; }
        public string KeyAttribute { get; set; }
        public string MailAttribute { get; set; }
        public string MethodAttribute { get; set; }
        public string NotifCheckDateAttribute { get; set; }
        public string NotifCreateDateAttribute { get; set; }
        public string NotifValidityAttribute { get; set; }
        public string Password { get; set; }
        public string PhoneAttribute { get; set; }
        public string TOTPAttribute { get; set; }
        public string TOTPEnabledAttribute { get; set; }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementAdminService.Initialize(host, true);
            MFAConfig cfg = ManagementAdminService.ADFSManager.ReadConfiguration(host);
            ADDSHost adds = cfg.Hosts.ActiveDirectoryHost;
            IsDirty = cfg.IsDirty;
            Account = adds.Account;
            Password = adds.Password;
            DomainAddress = adds.DomainAddress;
            KeyAttribute = adds.keyAttribute;
            MailAttribute = adds.mailAttribute;
            MethodAttribute = adds.methodAttribute;
            NotifCheckDateAttribute = adds.notifcheckdateattribute;
            NotifCreateDateAttribute = adds.notifcreatedateAttribute;
            NotifValidityAttribute = adds.notifvalidityAttribute;
            PhoneAttribute = adds.phoneAttribute;
            TOTPAttribute = adds.totpAttribute;
            TOTPEnabledAttribute = adds.totpEnabledAttribute;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementAdminService.Initialize(true);
            MFAConfig cfg = ManagementAdminService.ADFSManager.Config;
            ADDSHost adds = cfg.Hosts.ActiveDirectoryHost;
            cfg.IsDirty = IsDirty;
            adds.Account = Account;
            adds.Password = adds.Password;
            adds.DomainAddress = adds.DomainAddress;
            adds.keyAttribute = KeyAttribute;
            adds.mailAttribute = MailAttribute;
            adds.methodAttribute = MethodAttribute;
            adds.notifcheckdateattribute = NotifCheckDateAttribute;
            adds.notifcreatedateAttribute = NotifCreateDateAttribute;
            adds.notifvalidityAttribute = NotifValidityAttribute;
            adds.phoneAttribute = PhoneAttribute;
            adds.totpAttribute = TOTPAttribute;
            adds.totpEnabledAttribute = TOTPEnabledAttribute;
            ManagementAdminService.ADFSManager.WriteConfiguration(host);
            using (MailSlotClient mailslot = new MailSlotClient())
            {
                mailslot.SendNotification(0xAA);
            }
        }
    }

    [Serializable]
    public class MMCConfigMail
    {
        public bool IsDirty { get; set; }
        public string From { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string Company { get; set; }
        public List<MMCConfigMailFileName> MailOTPContent { get; set; }
        public List<MMCConfigMailFileName> MailAdminContent { get; set; }
        public List<MMCConfigMailFileName> MailKeyContent { get; set; }

        public MMCConfigMail()
        {
            this.MailOTPContent = new List<MMCConfigMailFileName>();
            this.MailAdminContent = new List<MMCConfigMailFileName>();
            this.MailKeyContent = new List<MMCConfigMailFileName>();
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementAdminService.Initialize(host, true);
            MFAConfig cfg = ManagementAdminService.ADFSManager.ReadConfiguration(host);
            SendMail mail = cfg.SendMail;
            IsDirty = cfg.IsDirty;
            From  = mail.From;
            UserName = mail.UserName;
            Password = mail.Password;
            Host = mail.Host;
            Port = mail.Port; 
            UseSSL = mail.UseSSL;
            Company = mail.Company;
            MailOTPContent.Clear();
            foreach(SendMailFileName itm in mail.MailOTPContent)
            {
                MailOTPContent.Add((MMCConfigMailFileName)itm);
            }
            MailAdminContent.Clear();
            foreach(SendMailFileName itm in mail.MailAdminContent)
            {
                MailAdminContent.Add((MMCConfigMailFileName)itm);
            }
            MailKeyContent.Clear();
            foreach(SendMailFileName itm in mail.MailKeyContent)
            {
                MailKeyContent.Add((MMCConfigMailFileName)itm);
            }
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementAdminService.Initialize(true);
            MFAConfig cfg = ManagementAdminService.ADFSManager.Config;
            SendMail mail = cfg.SendMail;
            cfg.IsDirty = IsDirty;
            mail.From = From;
            mail.UserName = UserName;
            mail.Password = Password;
            mail.Host = Host;
            mail.Port = Port;
            mail.UseSSL = UseSSL;
            mail.Company = Company;
            mail.MailOTPContent.Clear();
            foreach (MMCConfigMailFileName itm in MailOTPContent)
            {
                mail.MailOTPContent.Add((SendMailFileName)itm);
            }
            mail.MailAdminContent.Clear();
            foreach (MMCConfigMailFileName itm in MailAdminContent)
            {
                mail.MailAdminContent.Add((SendMailFileName)itm);
            }
            mail.MailKeyContent.Clear();
            foreach (MMCConfigMailFileName itm in MailKeyContent)
            {
                mail.MailKeyContent.Add((SendMailFileName)itm);
            }
            ManagementAdminService.ADFSManager.WriteConfiguration(host);
            using (MailSlotClient mailslot = new MailSlotClient())
            {
                mailslot.SendNotification(0xAA);
            }
        }
    }

    [Serializable]
    public class MMCConfigMailFileName
    {

        public int LCID { get; set; }
        public string FileName { get; set; }
        public bool Enabled { get; set; }

        public MMCConfigMailFileName(int lcid, string filename, bool enabled = true)
        {
            this.LCID = lcid;
            this.FileName = filename;
            this.Enabled = enabled;
        }

        public static explicit operator SendMailFileName(MMCConfigMailFileName file)
        {
            if (file == null)
                return null;
            else
                return new SendMailFileName(file.LCID, file.FileName, file.Enabled);
        }

        public static explicit operator MMCConfigMailFileName(SendMailFileName file)
        {
            if (file == null)
                return null;
            else
                return new MMCConfigMailFileName(file.LCID, file.FileName, file.Enabled);
        }
    }

    [Serializable]
    public class MMCKeysConfig
    {
        public bool IsDirty { get; set; }
        public KeyGeneratorMode KeyGenerator  { get; set; }
        public KeySizeMode KeySize { get; set; }
        public RegistrationSecretKeyFormat KeyFormat  { get; set; }
        public string CertificateThumbprint  { get; set; }
        public int CertificateValidity  { get; set; }
        public MMCExternalKeyManager ExternalKeyManager  { get; set; }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementAdminService.Initialize(host, true);
            MFAConfig cfg = ManagementAdminService.ADFSManager.ReadConfiguration(host);
            MFAKeysConfig keys = cfg.KeysConfig;
            IsDirty = cfg.IsDirty;
            this.CertificateThumbprint = keys.CertificateThumbprint;
            this.CertificateValidity = keys.CertificateValidity;
            this.KeyFormat = keys.KeyFormat;
            this.KeyGenerator = keys.KeyGenerator;
            this.KeySize = keys.KeySize;
            this.ExternalKeyManager = (MMCExternalKeyManager)keys.ExternalKeyManager;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementAdminService.Initialize(true);
            MFAConfig cfg = ManagementAdminService.ADFSManager.Config;
            MFAKeysConfig keys = cfg.KeysConfig;
            cfg.IsDirty = true;
            keys.CertificateThumbprint = this.CertificateThumbprint;
            keys.CertificateValidity = this.CertificateValidity;
            keys.KeyFormat = this.KeyFormat;
            keys.KeyGenerator = this.KeyGenerator;
            keys.KeySize = this.KeySize;
            keys.ExternalKeyManager = (MFAExternalKeyManager)this.ExternalKeyManager;
            ManagementAdminService.ADFSManager.WriteConfiguration(host);
            using (MailSlotClient mailslot = new MailSlotClient())
            {
                mailslot.SendNotification(0xAA);
            }
        }
    }

    [Serializable]
    public class MMCExternalKeyManager
    {
        public string FullQualifiedImplementation { get; set; }
        public XmlCDataSection Parameters { get; set; }

        public static explicit operator MFAExternalKeyManager(MMCExternalKeyManager mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                MFAExternalKeyManager ret = new MFAExternalKeyManager();
                ret.FullQualifiedImplementation = mgr.FullQualifiedImplementation;
                ret.Parameters = mgr.Parameters;
                return ret;
            }
        }

        public static explicit operator MMCExternalKeyManager(MFAExternalKeyManager mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                MMCExternalKeyManager ret = new MMCExternalKeyManager();
                ret.FullQualifiedImplementation = mgr.FullQualifiedImplementation;
                ret.Parameters = mgr.Parameters;
                return ret;
            }
        }

        public void Load(PSHost host)
        {
            ManagementAdminService.Initialize(host, true);
            MFAConfig cfg = ManagementAdminService.ADFSManager.ReadConfiguration(host);
            MFAKeysConfig otp = cfg.KeysConfig;
            this.FullQualifiedImplementation = otp.ExternalKeyManager.FullQualifiedImplementation;
            this.Parameters = otp.ExternalKeyManager.Parameters;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementAdminService.Initialize(true);
            MFAConfig cfg = ManagementAdminService.ADFSManager.Config;
            MFAKeysConfig otp = cfg.KeysConfig;
            otp.ExternalKeyManager.FullQualifiedImplementation = this.FullQualifiedImplementation;
            otp.ExternalKeyManager.Parameters = this.Parameters;
            ManagementAdminService.ADFSManager.WriteConfiguration(host);
            using (MailSlotClient mailslot = new MailSlotClient())
            {
                mailslot.SendNotification(0xAA);
            }
        }

    }

    [Serializable]
    public class MMCExternalOTPProvider
    {
        public bool IsDirty { get; set; }
        public string Company { get; set; }
        public string Sha1Salt { get; set; }
        public string FullQualifiedImplementation  { get; set; }
        public XmlCDataSection Parameters  { get; set; }
        public bool IsTwoWay  { get; set; }
        public int Timeout  { get; set; }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementAdminService.Initialize(host, true);
            MFAConfig cfg = ManagementAdminService.ADFSManager.ReadConfiguration(host);
            ExternalOTPProvider otp = cfg.ExternalOTPProvider;
            this.IsDirty = cfg.IsDirty;
            this.Company = otp.Company;
            this.FullQualifiedImplementation = otp.FullQualifiedImplementation;
            this.IsTwoWay = otp.IsTwoWay;
            this.Sha1Salt = otp.Sha1Salt;
            this.Timeout = otp.Timeout;
            this.Parameters = otp.Parameters;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementAdminService.Initialize(true);
            MFAConfig cfg = ManagementAdminService.ADFSManager.Config;
            ExternalOTPProvider otp = cfg.ExternalOTPProvider;
            cfg.IsDirty = true;
            otp.Company = this.Company;
            otp.FullQualifiedImplementation = this.FullQualifiedImplementation;
            otp.IsTwoWay = this.IsTwoWay;
            otp.Sha1Salt = this.Sha1Salt;
            otp.Timeout = this.Timeout;
            otp.Parameters = this.Parameters;
            ManagementAdminService.ADFSManager.WriteConfiguration(host);
            using (MailSlotClient mailslot = new MailSlotClient())
            {
                mailslot.SendNotification(0xAA);
            }
        }
    }
    #endregion    
}
