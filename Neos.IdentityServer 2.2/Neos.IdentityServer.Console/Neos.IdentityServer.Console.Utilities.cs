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
using System.Diagnostics;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Text.RegularExpressions;
using Neos.IdentityServer.Console.Resources;
using Neos.IdentityServer.MultiFactor;
using Neos.IdentityServer.MultiFactor.Administration;

namespace Neos.IdentityServer.Console
{
    #region MMCManagementService
    /// <summary>
    /// MMCService Class
    /// </summary>
    internal static class MMCService
    {
        private static DataFilterObject _filter = new DataFilterObject();
        private static DataPagingObject _paging = new DataPagingObject();
        private static DataOrderObject _order = new DataOrderObject();

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
        public static DataFilterObject Filter
        {
            get { return _filter; }
            set { _filter = value;  }
        }

        /// <summary>
        /// Paging Property
        /// </summary>
        public static DataPagingObject Paging
        {
            get { return _paging; }
        }

        /// <summary>
        /// Order property
        /// </summary>
        public static DataOrderObject Order
        {
            get { return _order; }
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
            MailUtilities.SendKeyByEmail(email, upn, key, ManagementService.Config.MailProvider, ManagementService.Config, CultureInfo.CurrentUICulture);
        }

        /// <summary>
        /// GetEncodedUserKey method implmentation
        /// </summary>
        internal static string GetEncodedUserKey(string upn)
        {
            EnsureService();
            return ManagementService.GetEncodedUserKey(upn);
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
        internal static RegistrationList GetUser(RegistrationList registrations)
        {
            EnsureService();
            RegistrationList lst = new RegistrationList();
            foreach(Registration reg in registrations)
            {
                Registration ret = ManagementService.GetUserRegistration(reg.UPN);
                lst.Add(ret);
            }
            return lst;
        }

        /// <summary>
        /// SetUser method implementation
        /// </summary>
        public static void SetUser(RegistrationList registrations)
        {
            EnsureService();
            foreach (Registration reg in registrations)
            {
                ManagementService.SetUserRegistration(reg, false);
            }
        }

        /// <summary>
        /// AddUser method implmentation
        /// </summary>
        public static RegistrationList AddUser(RegistrationList registrations)
        {
            EnsureService();
            RegistrationList lst = new RegistrationList();
            foreach(Registration reg in registrations)
            {
                lst.Add(ManagementService.AddUserRegistration(reg, false));
            }
            return lst;
        }

        /// <summary>
        /// DeleteUser method implmentation
        /// </summary>
        public static bool DeleteUser(RegistrationList registrations)
        {
            EnsureService();
            bool _ret = true;

            foreach(Registration reg in registrations)
            {
                bool tmp = ManagementService.DeleteUserRegistration(reg);
                if (!tmp)
                    _ret = false;
            }
            return _ret;
        }

        /// <summary>
        /// CheckAttribute method implementation
        /// </summary>
        internal static bool CheckAttribute(string attribute, int choice)
        {
            EnsureService();
            return ManagementService.CheckRepositoryAttribute(attribute, choice); 
        }

        /// <summary>
        /// EnableUser method implmentation
        /// </summary>
        public static RegistrationList EnableUser(RegistrationList registrations)
        {
            EnsureService();
            RegistrationList lst = new RegistrationList();
            foreach(Registration reg in registrations)
            {
                lst.Add(ManagementService.EnableUserRegistration(reg));
            }
            return lst;
        }

        /// <summary>
        /// DisableUser method implmentation 
        /// </summary>
        public static RegistrationList DisableUser(RegistrationList registrations)
        {
            EnsureService();
            RegistrationList lst = new RegistrationList();
            foreach(Registration reg in registrations)
            {
                lst.Add(ManagementService.DisableUserRegistration(reg));
            }
            return lst;
        }

        /// <summary>
        /// GetUsers method implementation
        /// </summary>
        public static RegistrationList GetUsers()
        {
            EnsureService();
            return ManagementService.GetUserRegistrations(Filter, Order, Paging);
        }

        /// <summary>
        /// GetAllUsers method implementation
        /// </summary>
        public static RegistrationList GetAllUsers(bool enabledonly = false)
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
