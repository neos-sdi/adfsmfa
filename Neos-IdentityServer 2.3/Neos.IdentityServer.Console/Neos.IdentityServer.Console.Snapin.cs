//******************************************************************************************************************************************************************************************//
// Copyright (c) 2019 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
using System.Security.Permissions;
using Microsoft.ManagementConsole;
using Neos.IdentityServer.MultiFactor.Administration;
using System.Diagnostics;
using System.Drawing;
using Microsoft.ManagementConsole.Advanced;
using System.Windows.Forms;
using Neos.IdentityServer.MultiFactor;
using System.Runtime.Serialization;
using System.Threading;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Neos.IdentityServer.Console
{
    /// <summary>
    /// Provides the main entry point for the creation of a snap-in. 
    /// </summary>
    [SnapInSettings("{9627F1F3-A6D2-4cf8-90A2-10F85A7A4EE7}", DisplayName = "MFA", Description = "You can use ADFS MFA to define and configure secure access to ADFS with second authentication factor like email, sms or TOTP application (Google Authenticator, Microsoft Authenticator).", Vendor="Neos-Sdi")]
    [SnapInAbout("Neos.IdentityServer.Console.NativeResources.dll", IconId = 100, VersionId=101, DescriptionId=102, DisplayNameId=103)]
    public class ADFSSnapIn : SnapIn
    {
        private ScopeNode ServiceNode;
        private ScopeNode ServiceGeneralNode;
        private ScopeNode ServiceSQLNode;
        private ScopeNode ServiceADDSNode;
        private ScopeNode ServiceProvidersNode;
        private ScopeNode ServiceTOTPNode;
        private ScopeNode ServiceSMTPNode;
        private ScopeNode ServiceSMSNode;
        private ScopeNode ServiceAzureNode;
       // private ScopeNode ServiceSecurityNode;
        private ScopeNode UsersNode;
        private bool IsPrimary = true;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ADFSSnapIn()
        {

        }

        /// <summary>
        /// OnInitialize method implmentation
        /// </summary>
        protected override void OnInitialize()
        {
            try
            {
                try
                {
                    ManagementService.VerifyPrimaryServer();
                }
                catch 
                {
                    IsPrimary = false;
                }
                ManagementService.Initialize(true);
                BuildNodes();
            }
            catch (Exception ex)
            {
                MessageBoxParameters msgp = new MessageBoxParameters();
                msgp.Caption = "MFA Error";
                msgp.Text = ex.Message;
                msgp.Buttons = MessageBoxButtons.OK;
                msgp.Icon = MessageBoxIcon.Error;
                this.Console.ShowDialog(msgp);
                if (this.RootNode != null)
                    this.RootNode.Children.Clear();
            }
            base.OnInitialize();
        }

        /// <summary>
        /// IsPrimaryServer property
        /// </summary>
        public bool IsPrimaryServer
        {
            get { return IsPrimary; }
        }

        /// <summary>
        /// OnLoadCustomData method implmentation
        /// </summary>
        protected override void OnLoadCustomData(AsyncStatus status, byte[] persistenceData)
        {
            try
            {
                if (persistenceData != null)
                {
                    try
                    {
                        MMCPersistenceData data = (MMCPersistenceData)persistenceData;
                        MMCService.Filter = data.Filter;
                        if (data.Language == 0)
                        {
                            if (CultureInfo.DefaultThreadCurrentUICulture != CultureInfo.InstalledUICulture)
                            {
                                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InstalledUICulture;
                                BuildNodes(false);
                            }
                        }
                        else
                            if (CultureInfo.DefaultThreadCurrentUICulture != new CultureInfo(data.Language))
                            {
                                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(data.Language);
                                BuildNodes(false);
                            }
                    }
                    catch (SerializationException)
                    {
                        //nothing 
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters msgp = new MessageBoxParameters();
                msgp.Text = ex.Message;
                msgp.Buttons = MessageBoxButtons.OK;
                msgp.Icon = MessageBoxIcon.Error;
                this.Console.ShowDialog(msgp);
            }
        }

        /// <summary>
        /// OnSaveCustomData method implmentation
        /// </summary>
        protected override byte[] OnSaveCustomData(SyncStatus status)
        {
            try
            {
                MMCPersistenceData data = new MMCPersistenceData();
                data.Filter = MMCService.Filter;
                if (CultureInfo.DefaultThreadCurrentUICulture.LCID == CultureInfo.InstalledUICulture.LCID)
                    data.Language = 0;
                else
                    data.Language = CultureInfo.DefaultThreadCurrentUICulture.LCID;
                return (byte[])data;
            }
            catch (Exception ex)
            {
                MessageBoxParameters msgp = new MessageBoxParameters();
                msgp.Text = ex.Message;
                msgp.Buttons = MessageBoxButtons.OK;
                msgp.Icon = MessageBoxIcon.Error;
                this.Console.ShowDialog(msgp);
                return null;
            }
        }

        /// <summary>
        /// BuildNodes method 
        /// </summary>
        private void BuildNodes(bool doall = true)
        {
            if (doall)
            {
                this.RootNode = new RootScopeNode();
                FormViewDescription fvr = new FormViewDescription();
                fvr.DisplayName = "MFA Platform";
                fvr.ControlType = typeof(RootViewControl);
                fvr.ViewType = typeof(RootFormView);
                this.RootNode.ViewDescriptions.Add(fvr);
                this.RootNode.ViewDescriptions.DefaultIndex = 0;

                if (IsPrimary)
                {
                    // Service Node
                    this.ServiceNode = new ServiceScopeNode();
                    FormViewDescription fvc = new FormViewDescription();
                    fvc.DisplayName = "MFA Platform Service";
                    fvc.ControlType = typeof(ServiceViewControl);
                    fvc.ViewType = typeof(ServiceFormView);
                    this.ServiceNode.ViewDescriptions.Add(fvc);
                    this.ServiceNode.ViewDescriptions.DefaultIndex = 0;

                    // General Scope
                    this.ServiceGeneralNode = new ServiceGeneralScopeNode();
                    FormViewDescription fvs = new FormViewDescription();
                    fvs.DisplayName = "MFA Platform General Properties";
                    fvs.ControlType = typeof(GeneralViewControl);
                    fvs.ViewType = typeof(GeneralFormView);
                    this.ServiceGeneralNode.ViewDescriptions.Add(fvs);
                    this.ServiceGeneralNode.ViewDescriptions.DefaultIndex = 0;

                    // ADDS Scope
                    this.ServiceADDSNode = new ServiceADDSScopeNode();
                    FormViewDescription fadds = new FormViewDescription();
                    fadds.DisplayName = "MFA Platform Active Directory Properties";
                    fadds.ControlType = typeof(ADDSViewControl);
                    fadds.ViewType = typeof(ServiceADDSFormView);
                    this.ServiceADDSNode.ViewDescriptions.Add(fadds);
                    this.ServiceADDSNode.ViewDescriptions.DefaultIndex = 0;

                    // SQL Scope
                    this.ServiceSQLNode = new ServiceSQLScopeNode();
                    FormViewDescription fsql = new FormViewDescription();
                    fsql.DisplayName = "MFA Platform SQL Server Properties";
                    fsql.ControlType = typeof(SQLViewControl);
                    fsql.ViewType = typeof(ServiceSQLFormView);
                    this.ServiceSQLNode.ViewDescriptions.Add(fsql);
                    this.ServiceSQLNode.ViewDescriptions.DefaultIndex = 0;

                    // Providers Scope
                    this.ServiceProvidersNode = new ServiceProvidersScopeNode();
                    FormViewDescription fprov = new FormViewDescription();
                    fprov.DisplayName = "MFA Providers";
                    fprov.ControlType = typeof(ProvidersViewControl);
                    fprov.ViewType = typeof(ServiceProvidersFormView);
                    this.ServiceProvidersNode.ViewDescriptions.Add(fprov);
                    this.ServiceProvidersNode.ViewDescriptions.DefaultIndex = 0;

                    ManagementService.EnsureService();
                    RuntimeAuthProvider.LoadProviders(ManagementService.Config);

                    IExternalProvider prv0 = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.Code);
                    if (prv0 != null)
                    {
                        // TOTP Scope
                        this.ServiceTOTPNode = new ServiceSecurityScopeNode();
                        FormViewDescription ftotp = new FormViewDescription();
                        ftotp.DisplayName = "MFA Platform TOTP Properties";
                        ftotp.ControlType = typeof(ServiceSecurityViewControl);
                        ftotp.ViewType = typeof(ServiceSecurityFormView);
                        this.ServiceTOTPNode.ViewDescriptions.Add(ftotp);
                        this.ServiceTOTPNode.ViewDescriptions.DefaultIndex = 0;
                    }

                    IExternalProvider prv1 = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.Email);
                    if (prv1 != null)
                    {
                        // SMTP Scope
                        this.ServiceSMTPNode = new ServiceSMTPScopeNode();
                        FormViewDescription fsmtp = new FormViewDescription();
                        fsmtp.DisplayName = "MFA Platform SMTP Properties";
                        fsmtp.ControlType = typeof(SMTPViewControl);
                        fsmtp.ViewType = typeof(ServiceSMTPFormView);
                        this.ServiceSMTPNode.ViewDescriptions.Add(fsmtp);
                        this.ServiceSMTPNode.ViewDescriptions.DefaultIndex = 0;
                    }

                    IExternalProvider prv2 = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.External);
                    if (prv2 != null)
                    {
                        // SMS Scope
                        this.ServiceSMSNode = new ServicePhoneScopeNode();
                        FormViewDescription fsms = new FormViewDescription();
                        fsms.DisplayName = "MFA Platform SMS Properties";
                        fsms.ControlType = typeof(SMSViewControl);
                        fsms.ViewType = typeof(ServiceSMSFormView);
                        this.ServiceSMSNode.ViewDescriptions.Add(fsms);
                        this.ServiceSMSNode.ViewDescriptions.DefaultIndex = 0;
                    }

                    IExternalProvider prv3 = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.Azure);
                    if (prv3 != null)
                    {
                        // Azure Scope
                        this.ServiceAzureNode = new ServiceAzureScopeNode();
                        FormViewDescription faz = new FormViewDescription();
                        faz.DisplayName = "MFA Platform SMS Properties";
                        faz.ControlType = typeof(AzureViewControl);
                        faz.ViewType = typeof(ServiceAzureFormView);
                        this.ServiceAzureNode.ViewDescriptions.Add(faz);
                        this.ServiceAzureNode.ViewDescriptions.DefaultIndex = 0;
                    }
                }

                // Users Scope
                this.UsersNode = new UsersScopeNode();
                FormViewDescription fvu = new FormViewDescription();
                fvu.DisplayName = "MFA Platform Users";
                fvu.ControlType = typeof(UsersListView);
                fvu.ViewType = typeof(UsersFormView);
                this.UsersNode.ViewDescriptions.Add(fvu);
                this.UsersNode.ViewDescriptions.DefaultIndex = 0;

                if (IsPrimary)
                {
                    this.RootNode.Children.Add(this.ServiceNode);
                    this.RootNode.Children.Add(this.ServiceGeneralNode);
                    this.RootNode.Children.Add(this.ServiceADDSNode);
                    this.RootNode.Children.Add(this.ServiceSQLNode);
                    this.RootNode.Children.Add(this.ServiceProvidersNode);
                    if (this.ServiceTOTPNode != null)
                        this.ServiceProvidersNode.Children.Add(this.ServiceTOTPNode);
                    if (this.ServiceSMTPNode != null)
                        this.ServiceProvidersNode.Children.Add(this.ServiceSMTPNode);
                    if (this.ServiceSMSNode != null)
                        this.ServiceProvidersNode.Children.Add(this.ServiceSMSNode);
                    if (this.ServiceAzureNode != null)
                        this.ServiceProvidersNode.Children.Add(this.ServiceAzureNode);
                }
                this.RootNode.Children.Add(this.UsersNode);

                this.IsModified = true;
                this.SmallImages.Add(Neos.IdentityServer.Console.Resources.Neos_IdentityServer_Console_Snapin.folder16, Color.Black);
                this.LargeImages.Add(Neos.IdentityServer.Console.Resources.Neos_IdentityServer_Console_Snapin.folder32, Color.Black);
            }
            else
            {
                RefreshUI();
            }
        }

        /// <summary>
        /// RefreshUI method implmentation
        /// </summary>
        public void RefreshUI()
        {
            ((RefreshableScopeNode)this.RootNode).RefreshUI();
          //  if (IsPrimary)
          //  {
                ((RefreshableScopeNode)this.ServiceNode).RefreshUI();
                ((RefreshableScopeNode)this.ServiceGeneralNode).RefreshUI();
                ((RefreshableScopeNode)this.ServiceADDSNode).RefreshUI();
                ((RefreshableScopeNode)this.ServiceSQLNode).RefreshUI();
                ((RefreshableScopeNode)this.ServiceProvidersNode).RefreshUI();
                ((RefreshableScopeNode)this.ServiceTOTPNode).RefreshUI();
                ((RefreshableScopeNode)this.ServiceSMTPNode).RefreshUI();
                ((RefreshableScopeNode)this.ServiceSMSNode).RefreshUI();
                ((RefreshableScopeNode)this.ServiceAzureNode).RefreshUI();
          //  }
            ((RefreshableScopeNode)this.UsersNode).RefreshUI();
        }
    }

    [Serializable]
    internal class MMCPersistenceData
    {
        private DataFilterObject _filter;
        private int _uilanguage;


        /// <summary>
        /// implicit conversion to byte array
        /// </summary>
        public static explicit operator byte[](MMCPersistenceData filterobj)
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
        public static explicit operator MMCPersistenceData(byte[] data)
        {
            if (data == null)
                return null;
            using (MemoryStream memStream = new MemoryStream())
            {
                BinaryFormatter binForm = new BinaryFormatter();
                memStream.Write(data, 0, data.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                return (MMCPersistenceData)binForm.Deserialize(memStream);
            }
        }

        public DataFilterObject Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        public int Language
        {
            get { return _uilanguage; }
            set { _uilanguage = value; }
        }
    }
} 