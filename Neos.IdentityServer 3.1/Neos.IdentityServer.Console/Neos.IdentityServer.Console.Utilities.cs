//******************************************************************************************************************************************************************************************//
// Copyright (c) 2021 @redhook62 (adfsmfa@gmail.com)                                                                                                                                    //                        
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Text.RegularExpressions;
using Neos.IdentityServer.Console.Resources;
using Neos.IdentityServer.MultiFactor;
using Neos.IdentityServer.MultiFactor.Administration;
using Neos.IdentityServer.MultiFactor.Data;

namespace Neos.IdentityServer.Console
{
    #region MMCManagementService
    /// <summary>
    /// MMCService Class
    /// </summary>
    internal static class MMCService
    {
        private static string EventLogSource = "ADFS MFA MMC"; 
        private static string EventLogGroup = "Application";

        /// <summary>
        /// RemoteAdminService static constructor
        /// </summary>
        static MMCService()
        {
            if (!EventLog.SourceExists(EventLogSource))
                EventLog.CreateEventSource(MMCService.EventLogSource, MMCService.EventLogGroup);
        }

        /// <summary>
        /// Filter Property
        /// </summary>
        public static DataFilterObject Filter { get; set; } = new DataFilterObject();

        /// <summary>
        /// Paging Property
        /// </summary>
        public static DataPagingObject Paging { get; } = new DataPagingObject();

        /// <summary>
        /// Order property
        /// </summary>
        public static DataOrderObject Order { get; } = new DataOrderObject();

        /// <summary>
        /// IsSQLEncrypted property
        /// </summary>
        public static bool IsSQLEncrypted
        {
            get 
            { 
                EnsureService();
                if (ManagementService.Config.StoreMode == DataRepositoryKind.SQL)
                    return ManagementService.Config.Hosts.SQLServerHost.IsAlwaysEncrypted;
                else
                    return false;
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
            return QRUtilities.GetQRCodeValue(upn, secret, ManagementService.Config);
        }

        /// <summary>
        /// GetQRCodeString method implementation
        /// </summary>
        public static string GetQRCodeString(string upn, string secret)
        {
            return QRUtilities.GetQRCodeString(upn, secret, ManagementService.Config);
        }

        /// <summary>
        /// SendKeyByEmail method implementation
        /// </summary>
        public static void SendKeyByEmail(string email, string upn, string key)
        {
            CultureInfo info = null;
            try
            {
                info = CultureInfo.CurrentUICulture;
            }
            catch
            {
                info = new CultureInfo(ManagementService.Config.DefaultCountryCode);
            }
            MailUtilities.SendKeyByEmail(email, upn, key, ManagementService.Config.MailProvider, ManagementService.Config, info);
        }

        /// <summary>
        /// GetEncodedUserKey method implmentation
        /// </summary>
        internal static string GetEncodedUserKey(string upn)
        {
            EnsureService();
            try
            {
                return ManagementService.GetEncodedUserKey(upn);
            }
            catch 
            {
                return null;
            }
        }

        /// <summary>
        /// NewUserKey method implementation
        /// </summary>
        internal static void NewUserKey(string upn)
        {
            EnsureService();
            ManagementService.NewUserKey(upn);
        }
        #endregion

        #region Data Operations
        /// <summary>
        /// EnsureService() method implmentation
        /// </summary>
        internal static void EnsureService()
        {
            ManagementService.Initialize(true);
        }

        /// <summary>
        /// GetUser method implementation
        /// </summary>
        internal static MFAUserList GetUser(MFAUserList registrations)
        {
            EnsureService();
            MFAUserList lst = new MFAUserList();
            foreach(MFAUser reg in registrations)
            {
                MFAUser ret = ManagementService.GetUserRegistration(reg.UPN);
                lst.Add(ret);
            }
            return lst;
        }

        /// <summary>
        /// GetUserStoredCredentials method implementation
        /// </summary>
        internal static List<WebAuthNCredentialInformation> GetUserStoredCredentials(string upn)
        {
            IExternalProvider prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Biometrics);
            IWebAuthNProvider web = prov as IWebAuthNProvider;

            return web.GetUserStoredCredentials(upn);           
        }

        /// <summary>
        /// RemoveUserStoredCredentials method implementation
        /// </summary>
        internal static void RemoveUserStoredCredentials(string upn, string credentialid)
        {
            IExternalProvider prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Biometrics);
            IWebAuthNProvider web = prov as IWebAuthNProvider;

            web.RemoveUserStoredCredentials(upn, credentialid);
        }

        /// <summary>
        /// SetUser method implementation
        /// </summary>
        public static void SetUser(MFAUserList registrations)
        {
            EnsureService();
            foreach (MFAUser reg in registrations)
            {
                ManagementService.SetUserRegistration(reg, false, false, false);
            }
        }

        /// <summary>
        /// AddUser method implmentation
        /// </summary>
        public static MFAUserList AddUser(MFAUserList registrations)
        {
            EnsureService();
            MFAUserList lst = new MFAUserList();
            foreach(MFAUser reg in registrations)
            {
                lst.Add(ManagementService.AddUserRegistration(reg, false, false, false));
            }
            return lst;
        }

        /// <summary>
        /// DeleteUser method implmentation
        /// </summary>
        public static bool DeleteUser(MFAUserList registrations)
        {
            EnsureService();
            bool _ret = true;

            foreach(MFAUser reg in registrations)
            {
                bool tmp = ManagementService.DeleteUserRegistration(reg);
                if (!tmp)
                    _ret = false;
            }
            return _ret;
        }

        /// <summary>
        /// EnableUser method implmentation
        /// </summary>
        public static MFAUserList EnableUser(MFAUserList registrations)
        {
            EnsureService();
            MFAUserList lst = new MFAUserList();
            foreach(MFAUser reg in registrations)
            {
                lst.Add(ManagementService.EnableUserRegistration(reg));
            }
            return lst;
        }

        /// <summary>
        /// DisableUser method implmentation 
        /// </summary>
        public static MFAUserList DisableUser(MFAUserList registrations)
        {
            EnsureService();
            MFAUserList lst = new MFAUserList();
            foreach(MFAUser reg in registrations)
            {
                lst.Add(ManagementService.DisableUserRegistration(reg));
            }
            return lst;
        }

        /// <summary>
        /// GetUsers method implementation
        /// </summary>
        public static MFAUserList GetUsers()
        {
            EnsureService();
            return ManagementService.GetUserRegistrations(Filter, Order, Paging);
        }

        /// <summary>
        /// GetAllUsers method implementation
        /// </summary>
        public static MFAUserList GetAllUsers(bool enabledonly = false)
        {
            EnsureService();
            return ManagementService.GetAllUserRegistrations(Order, enabledonly);
        }

        /// <summary>
        /// GetUsers method implementation
        /// </summary>
        public static int GetUsersCount(int maxrows = 20000)
        {
            EnsureService();
            return ManagementService.GetUserRegistrationsCount(Filter);
        }
        #endregion
    }
    #endregion
}
