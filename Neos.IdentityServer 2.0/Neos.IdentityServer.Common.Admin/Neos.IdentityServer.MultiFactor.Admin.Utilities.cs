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
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using Neos.IdentityServer.MultiFactor.Administration.Resources;
using Neos.IdentityServer.MultiFactor.QrEncoding;
using Neos.IdentityServer.MultiFactor.QrEncoding.Windows.Render;
using System.Diagnostics;
using System.Threading;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Security.Principal;
using System.Management.Automation.Host;
using System.Management.Automation;



namespace Neos.IdentityServer.MultiFactor.Administration
{
    #region RemoteAdminService
    /// <summary>
    /// RemoteAdminService Class
    /// </summary>
    public static class ManagementAdminService
    {
        private static ADFSServiceManager _manager = null;

        private static UsersFilterObject _filter = new UsersFilterObject();
        private static UsersPagingObject _paging = new UsersPagingObject();
        private static UsersOrderObject _order = new UsersOrderObject();

        private static string EventLogSource = "ADFS MFA Administration"; 
        private static string EventLogGroup = "Application";

        /// <summary>
        /// RemoteAdminService static constructor
        /// </summary>
        static ManagementAdminService()
        {
            if (!EventLog.SourceExists(EventLogSource))
                EventLog.CreateEventSource(ManagementAdminService.EventLogSource, ManagementAdminService.EventLogGroup);
        }

        /// <summary>
        /// Filter Property
        /// </summary>
        public static UsersFilterObject Filter
        {
            get { return _filter; }
            set { _filter = value;  }
        }

        /// <summary>
        /// Paging Property
        /// </summary>
        public static UsersPagingObject Paging
        {
            get { return _paging; }
        }

        /// <summary>
        /// Order property
        /// </summary>
        public static UsersOrderObject Order
        {
            get { return _order; }
        }

        /// <summary>
        /// ADFSManager property
        /// </summary>
        public static ADFSServiceManager ADFSManager
        {
            get { return _manager; }
        }

        /// <summary>
        /// Config property
        /// </summary>
        public static MFAConfig Config
        {
            get { return _manager.Config; }
        }

        /// <summary>
        /// Initialize method 
        /// </summary>
        public static void Initialize(bool loadconfig = false)
        {
            Initialize(null, loadconfig);
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public static void Initialize(PSHost host = null, bool loadconfig = false)
        {
            if (_manager == null)
            {
                _manager = new ADFSServiceManager();
                _manager.Initialize();
            }
            if (loadconfig)
            {
                try
                {
                    _manager.EnsureLocalConfiguration(host);
                }
                catch (CmdletInvocationException cm)
                {
                    EventLog.WriteEntry(EventLogSource, errors_strings.ErrorMFAUnAuthorized +"\r\r"+ cm.Message, EventLogEntryType.Error, 30901);
                    throw cm;
                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry(EventLogSource, string.Format(errors_strings.ErrorLoadingMFAConfiguration, ex.Message), EventLogEntryType.Error, 30900);
                    throw ex;
                }
            }
        }

        #region Utilities method
        /// <summary>
        /// IsValidEmail method implementation
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// IsValidPhone method implmentation
        /// </summary>
        public static bool IsValidPhone(string phonenumber)
        {
            string pho   = @"^\+(?:[0-9] ?){6,14}[0-9]$";
            string pho10 = @"^\d{10}$";
            string phous = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$";

            if (string.IsNullOrEmpty(phonenumber))
                return false;
            Regex rgx1 = new Regex(pho);
            bool match1 = rgx1.IsMatch(phonenumber);
            Regex rgx2 = new Regex(pho10);
            bool match2 = rgx2.IsMatch(phonenumber);
            Regex rgx3 = new Regex(phous);
            bool match3 = rgx3.IsMatch(phonenumber);
            return (match1) || (match2) || (match3);
        }

        /// <summary>
        /// GetQRCodeValue method implementation
        /// </summary>
        public static string GetQRCodeValue(string upn, string secret)
        {
            return QRUtilities.GetQRCodeValue(upn, secret, Config);
        }

        /// <summary>
        /// SendKeyByEmail method implementation
        /// </summary>
        public static void SendKeyByEmail(string email, string upn, string key)
        {
            MailUtilities.SendKeyByEmail(email, upn, key, Config.SendMail, Config, CultureInfo.CurrentUICulture);
        }
        #endregion

        #region Data Operations
        /// <summary>
        /// EnsureService() method implmentation
        /// </summary>
        internal static void EnsureService()
        {
         //   SetCultureInfo();
            if (_manager == null)
                Initialize(null, true);
        }

        /// <summary>
        /// GetService() method implmentation
        /// </summary>
        public static IAdministrationService GetService()
        {
            EnsureService();
            if (!Config.UseActiveDirectory)
                return new SQLAdminService(Config);
            else
                return new ADDSAdminService(Config);
        }

        /// <summary>
        /// GetUser method implementation
        /// </summary>
        internal static MMCRegistrationList GetUser(MMCRegistrationList registrations)
        {
            EnsureService();
            IAdministrationService svc = null;
            if (!Config.UseActiveDirectory)
               svc = new SQLAdminService(Config);
            else
               svc = new ADDSAdminService(Config);

            MMCRegistrationList lst = new MMCRegistrationList();
            foreach(MMCRegistration reg in registrations)
            {
                MMCRegistration ret = svc.GetUserRegistration(reg.UPN);
                lst.Add(ret);
            }
            return lst;
        }

        /// <summary>
        /// SetUser method implementation
        /// </summary>
        public static void SetUser(MMCRegistrationList registrations)
        {
            EnsureService();
            IAdministrationService svc = null;
            if (!Config.UseActiveDirectory)
                svc = new SQLAdminService(Config);
            else
                svc = new ADDSAdminService(Config);

            foreach (MMCRegistration reg in registrations)
            {
                svc.SetUserRegistration(reg);
            }
        }

        /// <summary>
        /// AddUser method implmentation
        /// </summary>
        public static MMCRegistrationList AddUser(MMCRegistrationList registrations)
        {
            EnsureService();
            IAdministrationService svc = null;
            if (!Config.UseActiveDirectory)
                svc = new SQLAdminService(Config);
            else
                svc = new ADDSAdminService(Config);

            MMCRegistrationList lst = new MMCRegistrationList();
            foreach(MMCRegistration reg in registrations)
            {
                KeysManager.NewKey(reg.UPN);
                MMCRegistration ret = svc.AddUserRegistration(reg);
                lst.Add(ret);
            }
            return lst;
        }

        /// <summary>
        /// DeleteUser method implmentation
        /// </summary>
        public static bool DeleteUser(MMCRegistrationList registrations)
        {
            EnsureService();
            bool _ret = true;
            IAdministrationService svc = null;
            if (!Config.UseActiveDirectory)
                svc = new SQLAdminService(Config);
            else
                svc = new ADDSAdminService(Config);

            foreach(MMCRegistration reg in registrations)
            {
                bool ret = svc.DeleteUserRegistration(reg);
                if (!ret)
                    _ret = false;
                KeysManager.RemoveKey(reg.UPN);
            }
            return _ret;
        }

        /// <summary>
        /// EnableUser method implmentation
        /// </summary>
        public static MMCRegistrationList EnableUser(MMCRegistrationList registrations)
        {
            EnsureService();
            IAdministrationService svc = null;
            if (!Config.UseActiveDirectory)
                svc = new SQLAdminService(Config);
            else
                svc = new ADDSAdminService(Config);

            MMCRegistrationList lst = new MMCRegistrationList();
            foreach(MMCRegistration reg in registrations)
            {
                lst.Add(svc.EnableUserRegistration(reg));
            }
            return lst;
        }

        /// <summary>
        /// DisableUser method implmentation 
        /// </summary>
        public static MMCRegistrationList DisableUser(MMCRegistrationList registrations)
        {
            EnsureService();
            IAdministrationService svc = null;
            if (!Config.UseActiveDirectory)
                svc = new SQLAdminService(Config);
            else
                svc = new ADDSAdminService(Config);

            MMCRegistrationList lst = new MMCRegistrationList();
            foreach(MMCRegistration reg in registrations)
            {
                MMCRegistration res = svc.DisableUserRegistration(reg);
                lst.Add(res);
            }
            return lst;
        }

        /// <summary>
        /// GetUsers method implementation
        /// </summary>
        public static MMCRegistrationList GetUsers(int maxrows = 20000)
        {
            EnsureService();
            IAdministrationService svc = null;
            if (!Config.UseActiveDirectory)
                svc = new SQLAdminService(Config);
            else
                svc = new ADDSAdminService(Config);
            return svc.GetUserRegistrations(Filter, Order, Paging, maxrows);
        }

        /// <summary>
        /// GetAllUsers method implementation
        /// </summary>
        public static MMCRegistrationList GetAllUsers(bool enabledonly = false)
        {
            EnsureService();
            IAdministrationService svc = null;
            if (!Config.UseActiveDirectory)
                svc = new SQLAdminService(Config);
            else
                svc = new ADDSAdminService(Config);
            return svc.GetAllUserRegistrations(Order, int.MaxValue, enabledonly);
        }

        /// <summary>
        /// GetUsers method implementation
        /// </summary>
        public static int GetUsersCount(int maxrows = 20000)
        {
            EnsureService();
            IAdministrationService svc = null;
            if (!Config.UseActiveDirectory)
                svc = new SQLAdminService(Config);
            else
                svc = new ADDSAdminService(Config);
            return svc.GetUserRegistrationsCount(Filter);
        }
        #endregion

    }

    public static class ADFSManagementRights
    {
        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool IsSystem()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            return identity.IsSystem;
        }

        public static bool AllowedGroup(string group)
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(group);
        }
    }
    #endregion
}
