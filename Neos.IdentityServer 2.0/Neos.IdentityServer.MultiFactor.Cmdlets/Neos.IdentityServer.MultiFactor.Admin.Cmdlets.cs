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
namespace Neos.IdentityServer.MultiFactor.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Threading;
    using System.DirectoryServices;
    using Neos.IdentityServer.MultiFactor.Cmdlets.Ressources;
    using System.Management.Automation.Host;
    using System.Collections.ObjectModel;

    /// <summary>
    /// MFACmdlet class
    /// </summary>
    public class MFACmdlet: PSCmdlet
    {
        /// <summary>
        /// GetHostForVerbose() method 
        /// </summary>
        public PSHost GetHostForVerbose()
        {
            try
            {
                var b = this.MyInvocation.BoundParameters["Verbose"];
                SwitchParameter verb = (SwitchParameter)b;
                if (verb.IsPresent)
                    return this.Host;
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }
    }
 
    #region Cmdlets region
    /// <summary>
    /// <para type="synopsis">Get MFA Users.</para>
    /// <para type="description">Get a collection of users registered with MFA.</para>
    /// <para type="description">You can specifiy an Identity (upn), specify filters or use paging.</para>
    /// <para type="description">You can update users with Set-MFAUsers.</para>
    /// </summary>
    /// <example>
    ///   <para>Get all users registered with MFA, store results in variable.</para>
    ///   <para>$users = Get-MFAUsers</para>
    /// </example>
    /// <example>
    ///   <para>Get all users registered with MFA, incuding disabled users.</para>
    ///   <para>Get-MFAUsers -all</para> 
    /// </example>
    /// <example>
    ///   <para>get all users registered with MFA whose the upn start with "neos", including disabled, display result count.</para>
    ///   <para>Get-MFAUsers -FilterValue neos* -FilterOperator StartWith -IncludeDisabled -ShowCount</para>
    ///   <para>Get-MFAUsers -Value neos* -Operator StartWith -All -ShowCount -SortOrder UserName</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "MFAUsers", SupportsShouldProcess = true, SupportsPaging = false, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
    [OutputType(typeof(PSRegistration[]))]
    public sealed class GetMFAUser : MFACmdlet
    {
        string _identity = string.Empty;
        private int _maxrows = 20000;
        private PSRegistration[] _list = null;
        private UsersFilterObject _filter = new UsersFilterObject();
        private UsersPagingObject _paging = new UsersPagingObject();
        private UsersOrderObject _order = new UsersOrderObject();

        /// <summary>
        /// <para type="description">identity of the user to selected (upn).</para>
        /// </summary>
        [Parameter(Mandatory = false, Position=0, ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Identity
        {
            get { return _identity; }
            set { _identity = value; }
        }

        /// <summary>
        /// <para type="description">MaxRows to return.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public int MaxRows
        {
            get { return _maxrows; }
            set { _maxrows = value; }
        }

        /// <summary>
        /// <para type="description">Include disabled users in results.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [Parameter(ParameterSetName = "Data")]
        [Alias("All")]
        public SwitchParameter IncludeDisabled
        {
            get { return !_filter.EnabledOnly; }
            set { _filter.EnabledOnly = !value; }
        }

        /// <summary>
        /// <para type="description">When using filtering this is the property for filter. alias (Field, ATTR).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        [Alias("Field", "ATTR")]
        public UsersFilterField FilterField
        {
            get { return _filter.FilterField; }
            set { _filter.FilterField = value; }
        }

        /// <summary>
        /// <para type="description">When using filtering this is the operator property for filter. alias (Operator, OP).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        [Alias("Operator", "OP")]
        public UsersFilterOperator FilterOperator
        {
            get { return _filter.FilterOperator; }
            set {_filter.FilterOperator = value; }
        }

        /// <summary>
        /// <para type="description">When using filtering this is the value property for filter (can contains wilcards). alias (Value, VAL).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        [Alias("Value", "VAL")]
        public string FilterValue
        {
            get { return _filter.FilterValue; }
            set { _filter.FilterValue = value; }
        }

        /// <summary>
        /// <para type="description">When using filtering this is the Preferred method filter (Phone, Email, ...). alias (Method).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        [Alias("Method")]
        public UsersPreferredMethod FilterMethod
        {
            get { return _filter.FilterMethod; }
            set { _filter.FilterMethod = value; }
        }

        /// <summary>
        /// <para type="description">When using paging this is the Page size.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public int PagingSize
        {
            get;
            set;
        }

        /// <summary>
        /// <para type="description">When using paging this is the Current Page.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public int CurrentPage
        {
            get;
            set;
        }

        /// <summary>
        /// ShowTotalCount property
        /// <para type="description">Show the numers of rows selected.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter ShowCount
        {
            get;
            set;
        }

        /// <summary>
        /// <para type="description">When using sorting this is the Filed user for ordering results.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public UsersOrderField SortOrder
        {
            get { return _order.Column; }
            set { _order.Column = value; }
        }

        /// <summary>
        /// <para type="description">When using sorting this is sort direction (asc, desc).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public SortDirection SortDirection
        {
            get { return _order.Direction; }
            set { _order.Direction = value; }
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (string.IsNullOrEmpty(_identity))
            {
                try
                {
                    _paging.Clear();
                    _paging.CurrentPage = CurrentPage;
                    _paging.PageSize = PagingSize;

                    ManagementAdminService.Initialize(this.Host, true);
                    IAdministrationService intf = ManagementAdminService.GetService();
                    PSRegistrationList mmc = (PSRegistrationList)intf.GetUserRegistrations(_filter, _order, _paging, MaxRows);
                    _list = mmc.ToArray();
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "1000", ErrorCategory.OperationStopped, _list));
                }
            }
            else
            {
                try
                {
                    ManagementAdminService.Initialize(this.Host, true);
                    IAdministrationService intf = ManagementAdminService.GetService();
                    PSRegistration ret = (PSRegistration)intf.GetUserRegistration(Identity);
                    if (ret == null)
                        throw new Exception(string.Format(errors_strings.ErrorUserNotFound, this.Identity));
                    _list = new PSRegistration[] { ret };
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "1001", ErrorCategory.OperationStopped, _list));
                }
            }
        }

        /// <summary>
        /// EndProcessing method implementation
        /// </summary>
        protected override void EndProcessing()
        {
            _list = null;
            base.EndProcessing();
        }

        /// <summary>
        /// ProcessInternalRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (ShouldProcess(string.Format("{0} Users", _list.LongLength.ToString())))
                {
                    foreach (PSRegistration reg in _list)
                    {
                        WriteObject(reg);
                    }
                }
                if (ShowCount)
                {
                    this.Host.UI.WriteLine(ConsoleColor.Yellow, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosRecordsCount, _list.LongLength.ToString()));
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "1002", ErrorCategory.OperationStopped, _list));
            }
        }

        /// <summary>
        /// StopProcessing method implementation
        /// </summary>
        protected override void StopProcessing()
        {
            _list = null;
            base.StopProcessing();
        }
    }

    /// <summary>
    /// <para type="synopsis">Update MFA Users.</para>
    /// <para type="description">Update a collection of users registered with MFA.</para>
    /// <para type="description">You can specifiy an Identity (upn), or pass a collection.</para>
    /// </summary>
    /// <example>
    ///   <para>Update all users in collection.</para>
    ///   <para>Set-MFAUsers $users</para>
    /// </example>
    /// <example>
    ///   <para>Update a specific user.</para>
    ///   <para>Set-MFAUsers -Identity user@domain.com -Email user@mailbox.com -Phone 0606050403 -Method Code</para> 
    /// </example>
    /// <example>
    ///   <para>Update users and reset SecretKey.</para>
    ///   <para>Set-MFAUsers -Identity user@domain.com -ResetKey</para> 
    ///   <para>Set-MFAUsers $users -ResetKey</para> 
    /// </example>
    [Cmdlet(VerbsCommon.Set, "MFAUsers", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
    public sealed class SetMFAUser : MFACmdlet
    {
        private string _identity = string.Empty;
        private string _mailaddress = string.Empty;
        private string _phonenumber = string.Empty;
        private UsersPreferredMethod _method = UsersPreferredMethod.None;
        private bool _enabled = true;
        private bool _resetkey = false;

        PSRegistration[] _data = null;

        /// <summary>
        /// <para type="description">identity of the updated user (upn).</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Identity
        {
            get { return _identity; }
            set { _identity = value; }
        }

        /// <summary>
        /// <para type="description">email address of the updated users.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "Identity")]
        [Parameter(ParameterSetName = "Data")]
        [Alias("Email")]
        public string MailAddress
        {
            get { return _mailaddress; }
            set { _mailaddress = value; }
        }

        /// <summary>
        /// <para type="description">phone number of the updated users.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "Identity")]
        [Parameter(ParameterSetName = "Data")]
        [Alias("Phone")]
        public string PhoneNumber
        {
            get { return _phonenumber; }
            set { _phonenumber = value; }
        }

        /// <summary>
        /// <para type="description">MFA Method for the updated users.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [Parameter(ParameterSetName = "Data")]
        [ValidateRange(UsersPreferredMethod.Choose, UsersPreferredMethod.None)]
        public UsersPreferredMethod Method
        {
            get { return _method; }
            set { _method = value; }
        }

        /// <summary>
        /// <para type="description">Enabled status for the selected users.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        /// <summary>
        /// <para type="description">Regenerate a new secret key for the selected users.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter ResetKey
        {
            get { return _resetkey; }
            set { _resetkey = value; }
        }

        /// <summary>
        /// <para type="description">Collection of users to update.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data", ValueFromPipeline = true, DontShow = true)]
        [ValidateNotNullOrEmpty()]
        public PSRegistration[] Data
        {
            get { return _data; }
            set { _data = value; }
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (!string.IsNullOrEmpty(_identity))
            {
                try
                {
                    ManagementAdminService.Initialize(this.Host, true);
                    IAdministrationService intf = ManagementAdminService.GetService();
                    PSRegistration res = (PSRegistration)intf.GetUserRegistration(Identity);
                    if (res == null)
                        throw new Exception(string.Format(errors_strings.ErrorUserNotFound, this.Identity));
                    Data = new PSRegistration[] { res };
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "1003", ErrorCategory.OperationStopped, this));
                }
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            int i = 0;
            IAdministrationService intf = ManagementAdminService.GetService();
            ProgressRecord prog = null;
            if (_data.Length >= 10)
            {
                prog = new ProgressRecord(this.GetHashCode(), "Set-MFAUsers", "Operation running");
                prog.PercentComplete = 0;
                prog.RecordType = ProgressRecordType.Processing;
            }
            foreach (PSRegistration reg in _data)
            {
                if (ShouldProcess(string.Format("\"{0}\"", reg.UPN)))
                {
                    if (_data.Length >= 10)
                    {
                        prog.PercentComplete = ((i / _data.Length) * 100);
                        prog.CurrentOperation = string.Format("\"{0}\" ", reg.UPN);
                        this.WriteProgress(prog);
                    }
                    try
                    {

                        if (MailAddress != string.Empty)
                            reg.MailAddress = this.MailAddress;
                        if (PhoneNumber != string.Empty)
                            reg.PhoneNumber = this.PhoneNumber;
                        if (this.Method != UsersPreferredMethod.None)
                            reg.PreferredMethod = (RegistrationPreferredMethod)this.Method;
                        reg.Enabled = this.Enabled;

                        if (string.IsNullOrEmpty(reg.MailAddress) && ManagementAdminService.Config.MailEnabled)
                           this.Host.UI.WriteWarningLine(string.Format(errors_strings.ErrorEmailNotProvided, reg.UPN));
                        if (string.IsNullOrEmpty(reg.PhoneNumber) && ManagementAdminService.Config.SMSEnabled)
                            this.Host.UI.WriteWarningLine(string.Format(errors_strings.ErrorPhoneNotProvided, reg.UPN));
                        intf.SetUserRegistration((MMCRegistration)reg);
                        if (this.ResetKey)
                        {
                            KeysManager.NewKey(reg.UPN);
                            this.WriteVerbose(string.Format(infos_strings.InfosUserHasNewKey, reg.UPN));
                        }
                        this. WriteVerbose(string.Format(infos_strings.InfosUserUpdated, reg.UPN));
                    }
                    catch (Exception Ex)
                    {
                        this.Host.UI.WriteErrorLine(string.Format(errors_strings.ErrorUpdatingUser, reg.UPN, Ex.Message));
                    }
                }
                i++;
            }
            if (_data.Length >= 10)
                prog.RecordType = ProgressRecordType.Completed;
        }
    }

    /// <summary>
    /// <para type="synopsis">Add MFA Users.</para>
    /// <para type="description">Add user(s) to MFA System.</para>
    /// <para type="description">You can specifiy an Identity (upn), or pass a collection.</para>
    /// </summary>
    /// <example>
    ///   <para>Add all users in collection.</para>
    ///   <para>Add-MFAUsers $users</para>
    /// </example>
    /// <example>
    ///   <para>Add a specific user.</para>
    ///   <para>Add-MFAUsers -Identity user@domain.com -Email user@mailbox.com -Phone 0606050403 -Method Code</para> 
    /// </example>
    [Cmdlet(VerbsCommon.Add, "MFAUsers", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
    public sealed class AddMFAUser : MFACmdlet
    {
        private string _identity = string.Empty;
        private string _mailaddress = string.Empty;
        private string _phonenumber = string.Empty;
        private UsersPreferredMethod _method = UsersPreferredMethod.None;
        private bool _enabled = true;

        PSRegistration[] _data = null;

        /// <summary>
        /// <para type="description">identity of the new user (upn).</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Identity
        {
            get { return _identity; }
            set { _identity = value; }
        }

        /// <summary>
        /// <para type="description">email address of the new user.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "Identity")]
        [Alias("Email")]
        public string MailAddress
        {
            get { return _mailaddress; }
            set { _mailaddress = value; }
        }

        /// <summary>
        /// <para type="description">phone number of the new user.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "Identity")]
        [Alias("Phone")]
        public string PhoneNumber
        {
            get { return _phonenumber; }
            set { _phonenumber = value; }
        }

        /// <summary>
        /// <para type="description">MFA Method for the new users.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateRange(UsersPreferredMethod.Choose, UsersPreferredMethod.None)]
        public UsersPreferredMethod Method
        {
            get { return _method; }
            set { _method = value; }
        }

        /// <summary>
        /// <para type="description">Enabled status for the new users.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public SwitchParameter Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        /// <summary>
        /// <para type="description">Collection of users to add.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data", ValueFromPipeline = true, DontShow = true)]
        [ValidateNotNullOrEmpty()]
        public PSRegistration[] Data
        {
            get { return _data; }
            set { _data = value; }
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (!string.IsNullOrEmpty(_identity))
            {
                try
                {
                    ManagementAdminService.Initialize(this.Host, true);
                    IAdministrationService intf = ManagementAdminService.GetService();

                    PSRegistration res = (PSRegistration)intf.GetUserRegistration(Identity);
                    if (res != null)
                        throw new Exception(string.Format(errors_strings.ErrorUserExists, this.Identity));
                    res = new PSRegistration();
                    res.UPN = Identity;
                    Data = new PSRegistration[] { res };
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "1004", ErrorCategory.OperationStopped, this));
                }
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            int i = 0;
            IAdministrationService intf = ManagementAdminService.GetService();
            ProgressRecord prog = null;
            if (_data.Length >= 10)
            {
                prog = new ProgressRecord(this.GetHashCode(), "Add-MFAUsers", "Operation running");
                prog.PercentComplete = 0;
                prog.RecordType = ProgressRecordType.Processing;
            }
            foreach (PSRegistration reg in _data)
            {
                if (ShouldProcess(string.Format("\"{0}\"", reg.UPN)))
                {
                    if (_data.Length >= 10)
                    {
                        prog.PercentComplete = ((i / _data.Length) * 100);
                        prog.CurrentOperation = string.Format("\"{0}\" ", reg.UPN);
                        this.WriteProgress(prog);
                    }
                    try
                    {
                        if (MailAddress != string.Empty)
                            reg.MailAddress = this.MailAddress;
                        if (PhoneNumber != string.Empty)
                            reg.PhoneNumber = this.PhoneNumber;
                        if (this.Method != UsersPreferredMethod.None)
                            reg.PreferredMethod = (RegistrationPreferredMethod)this.Method;
                        reg.Enabled = this.Enabled; 

                        if (string.IsNullOrEmpty(reg.MailAddress) && ManagementAdminService.Config.MailEnabled)
                            this.Host.UI.WriteWarningLine(string.Format(errors_strings.ErrorEmailNotProvided, reg.UPN));
                        if (string.IsNullOrEmpty(reg.PhoneNumber) && ManagementAdminService.Config.SMSEnabled)
                            this.Host.UI.WriteWarningLine(string.Format(errors_strings.ErrorPhoneNotProvided, reg.UPN));

                        PSRegistration ret = (PSRegistration)intf.AddUserRegistration((MMCRegistration)reg);
                        if (ret != null)
                        {
                            KeysManager.NewKey(reg.UPN);
                            this.WriteVerbose(string.Format(infos_strings.InfosUserHasNewKey, reg.UPN));
                            this.WriteObject(ret);
                            this.WriteVerbose(string.Format(infos_strings.InfosUserAdded, reg.UPN));
                        }
                    }
                    catch (Exception Ex)
                    {
                        this.Host.UI.WriteErrorLine(string.Format(errors_strings.ErrorAddingUser, reg.UPN, Ex.Message));
                    }
                }
                i++;
            }
            if (_data.Length >= 10)
                prog.RecordType = ProgressRecordType.Completed;
        }
    }

    /// <summary>
    /// <para type="synopsis">Delete MFA Users.</para>
    /// <para type="description">Remove user(s) from MFA System.</para>
    /// <para type="description">You can specifiy an Identity (upn), or pass a collection.</para>
    /// </summary>
    /// <example>
    ///   <para>remove all users in collection.</para>
    ///   <para>Remove-MFAUsers $users</para>
    /// </example>
    /// <example>
    ///   <para>Remove a specific user.</para>
    ///   <para>Remove-MFAUsers -Identity user@domain.com</para> 
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "MFAUsers", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
    public sealed class DeleteMFAUser : MFACmdlet
    {
        string _identity = string.Empty;
        PSRegistration[] _data = null;

        /// <summary>
        /// <para type="description">identity of the user to be removed (upn).</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Identity
        {
            get { return _identity; }
            set { _identity = value; }
        }

        /// <summary>
        /// <para type="description">Collection of users to be removed from MFA.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data", ValueFromPipeline = true, DontShow = true)]
        [ValidateNotNullOrEmpty()]
        public PSRegistration[] Data
        {
            get { return _data; }
            set { _data = value; }
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (!string.IsNullOrEmpty(_identity))
            {
                try
                {
                    ManagementAdminService.Initialize(this.Host, true);
                    IAdministrationService intf = ManagementAdminService.GetService();

                    PSRegistration res = (PSRegistration)intf.GetUserRegistration(Identity);
                    if (res == null)
                        throw new Exception(string.Format(errors_strings.ErrorUserNotFound, this.Identity));
                    Data = new PSRegistration[] { res };
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "1005", ErrorCategory.OperationStopped, this));
                }
            }
        }

        /// <summary>
        /// ProcessInternalRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            int i = 0;
            IAdministrationService intf = ManagementAdminService.GetService();
            ProgressRecord prog = null;
            if (_data.Length >= 10)
            {
                prog = new ProgressRecord(this.GetHashCode(), "Remove-MFAUsers", "Operation running");
                prog.PercentComplete = 0;
                prog.RecordType = ProgressRecordType.Processing;
            }
            foreach (PSRegistration reg in _data)
            {
                if (ShouldProcess(string.Format("\"{0}\"", reg.UPN)))
                {
                    try
                    {
                        if (_data.Length >= 10)
                        {
                            prog.PercentComplete = ((i / _data.Length) * 100);
                            prog.CurrentOperation = string.Format("\"{0}\" ", reg.UPN);
                            this.WriteProgress(prog);
                        }

                        intf.DeleteUserRegistration((MMCRegistration)reg);
                        this.WriteVerbose(string.Format(infos_strings.InfosUserDeleted, reg.UPN));
                    }
                    catch (Exception Ex)
                    {
                        this.Host.UI.WriteErrorLine(string.Format(errors_strings.ErrorDeletingUser, reg.UPN, Ex.Message));
                    }
                }
                i++;
            }
            if (_data.Length >= 10)
                prog.RecordType = ProgressRecordType.Completed;
        }
    }

    /// <summary>
    /// <para type="synopsis">Enable MFA Users.</para>
    /// <para type="description">Enable user(s) in MFA System.</para>
    /// <para type="description">You can specifiy an Identity (upn), or pass a collection.</para>
    /// </summary>
    /// <example>
    ///   <para>enable all users in collection.</para>
    ///   <para>Enable-MFAUsers $users</para>
    /// </example>
    /// <example>
    ///   <para>Enable a specific user.</para>
    ///   <para>Enable-MFAUsers -Identity user@domain.com</para> 
    /// </example>
    [Cmdlet(VerbsLifecycle.Enable, "MFAUsers", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
    [OutputType(typeof(PSRegistration[]))]
    public sealed class EnableMFAUser : MFACmdlet
    {
        string _identity = string.Empty;
        PSRegistration[] _data = null;

        /// <summary>
        /// <para type="description">identity of the user to be enabled (upn).</para>
        /// </summary>
        [Parameter(Mandatory = true, Position=0, ParameterSetName="Identity")]
        [ValidateNotNullOrEmpty()]
        public string Identity
        {
            get { return _identity; }
            set { _identity = value; }
        }

        /// <summary>
        /// <para type="description">Collection of users to be enabled for MFA (must be registered before with Add-MFAUsers.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position=0, ParameterSetName="Data", ValueFromPipeline = true, DontShow = true)]
        [ValidateNotNullOrEmpty()]
        public PSRegistration[] Data
        {
            get { return _data; }
            set { _data = value; }
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (!string.IsNullOrEmpty(_identity))
            {
                try
                {
                    ManagementAdminService.Initialize(this.Host, true);
                    IAdministrationService intf = ManagementAdminService.GetService();

                    PSRegistration res = (PSRegistration)intf.GetUserRegistration(Identity);
                    if (res == null)
                        throw new Exception(string.Format(errors_strings.ErrorUserNotFound, this.Identity));
                    Data = new PSRegistration[] { res };
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "1006", ErrorCategory.OperationStopped, this));
                }
            }
        }

        /// <summary>
        /// ProcessInternalRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            int i = 0;
            IAdministrationService intf = ManagementAdminService.GetService();
            ProgressRecord prog = null;
            if (_data.Length >= 10)
            {
                prog = new ProgressRecord(this.GetHashCode(), "Enable-MFAUsers", "Operation running");
                prog.PercentComplete = 0;
                prog.RecordType = ProgressRecordType.Processing;
            }
            foreach (PSRegistration reg in _data)
            {
                if (ShouldProcess(string.Format("\"{0}\"", reg.UPN)))
                {
                    try
                    {
                        if (_data.Length >= 10)
                        {
                            prog.PercentComplete = ((i / _data.Length) * 100);
                            prog.CurrentOperation = string.Format("\"{0}\" ", reg.UPN);
                            this.WriteProgress(prog);
                        }

                        PSRegistration res = (PSRegistration)intf.EnableUserRegistration((MMCRegistration)reg);
                        this.WriteObject(res);
                        this.WriteVerbose(string.Format(infos_strings.InfosUserUpdated, reg.UPN));
                    }
                    catch (Exception Ex)
                    {
                        this.Host.UI.WriteErrorLine(string.Format(errors_strings.ErrorUpdatingUser, reg.UPN, Ex.Message));
                    }
                }
                i++;
            }
            if (_data.Length >= 10)
                prog.RecordType = ProgressRecordType.Completed;
        }
    }

    /// <summary>
    /// <para type="synopsis">Disable MFA Users.</para>
    /// <para type="description">Disable user(s) in MFA System.</para>
    /// <para type="description">You can specifiy an Identity (upn), or pass a collection.</para>
    /// </summary>
    /// <example>
    ///   <para>disable all users in collection.</para>
    ///   <para>Disable-MFAUsers $users</para>
    /// </example>
    /// <example>
    ///   <para>Disable a specific user.</para>
    ///   <para>Disable-MFAUsers -Identity user@domain.com</para> 
    /// </example>
    [Cmdlet(VerbsLifecycle.Disable, "MFAUsers", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
    [OutputType(typeof(PSRegistration[]))]
    public sealed class DisableMFAUser : MFACmdlet
    {
        string _identity = string.Empty;
        PSRegistration[] _data = null;

        /// <summary>
        /// <para type="description">identity of the user to be disabled (upn).</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName="Identity")]
        [ValidateNotNullOrEmpty()]
        public string Identity
        {
            get { return _identity; }
            set { _identity = value; }
        }

        /// <summary>
        /// <para type="description">Collection of users to be disabled for MFA (must be registered before with Add-MFAUsers.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data", ValueFromPipeline = true, DontShow = true)]
        [ValidateNotNullOrEmpty()]
        public PSRegistration[] Data
        {
            get { return _data; }
            set { _data = value; }
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (!string.IsNullOrEmpty(_identity))
            {
                try
                {
                    ManagementAdminService.Initialize(this.Host, true);
                    IAdministrationService intf = ManagementAdminService.GetService();

                    PSRegistration res = (PSRegistration)intf.GetUserRegistration(Identity);
                    if (res == null)
                        throw new Exception(string.Format(errors_strings.ErrorUserNotFound, this.Identity));
                    Data = new PSRegistration[] { res };
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "1007", ErrorCategory.OperationStopped, this));
                }
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            if (_data == null)
                return;
            int i= 0;
            IAdministrationService intf = ManagementAdminService.GetService();
            ProgressRecord prog = null;
            if (_data.Length >= 10)
            {
                prog = new ProgressRecord(this.GetHashCode(), "Disable-MFAUsers", "Operation running");
                prog.PercentComplete = 0;
                prog.RecordType = ProgressRecordType.Processing;
            }
            foreach (PSRegistration reg in _data)
            {
                if (ShouldProcess(string.Format("\"{0}\"", reg.UPN)))
                {
                    try
                    {
                        if (_data.Length >= 10)
                        {
                            prog.PercentComplete = ((i / _data.Length) * 100);
                            prog.CurrentOperation = string.Format("\"{0}\" ", reg.UPN);
                            this.WriteProgress(prog);
                        }

                        PSRegistration res = (PSRegistration)intf.DisableUserRegistration((MMCRegistration)reg);
                        this.WriteObject(res);
                        this.WriteVerbose(string.Format(infos_strings.InfosUserUpdated, reg.UPN));
                    }
                    catch (Exception Ex)
                    {
                        this.Host.UI.WriteErrorLine(string.Format(errors_strings.ErrorUpdatingUser, reg.UPN, Ex.Message));
                    }
                }
                i++;
            }
            if (_data.Length >= 10)
                prog.RecordType = ProgressRecordType.Completed;
        }
    }

    /// <summary>
    /// <para type="synopsis">Get MFA Farm Informations.</para>
    /// <para type="description">Get MFA farm Informations.</para>
    /// <para type="description">You can see Servers, behavior and initilization status.</para>
    /// <para type="description">If not initialized, you must run the Register-MFASystem and optionally Register-MFAComputer (2012R2).</para>
    /// </summary>
    /// <example>
    ///   <para>Get-MFAFarmInformation</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "MFAFarmInformation", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None)]
    [OutputType(typeof(ADFSFarmHost))]
    public sealed class GetMFAFarmInformation : MFACmdlet
    {

        ADFSFarmHost _farm;

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementAdminService.Initialize(this.Host, true);
                _farm = ManagementAdminService.ADFSManager.ADFSFarm;
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3004", ErrorCategory.OperationStopped, _farm));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (ShouldProcess("Farm Information"))
                {
                    WriteObject(_farm);
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3006", ErrorCategory.OperationStopped, _farm));
            }
        }

        /// <summary>
        /// EndProcessing method implementation
        /// </summary>
        protected override void EndProcessing()
        {
            _farm = null;
            base.EndProcessing();
        }
    }

    /// <summary>
    /// <para type="synopsis">Register MFA Computer.</para>
    /// <para type="description">Regiter an ADFS server from your farm.</para>
    /// <para type="description">If your farm is Windows 2016 (behavior 3), Register-MFASystem is doing the job.</para>
    /// <para type="description">For Windows 2012R2, you must register your ADFS server (not proxies).</para>
    /// <para type="description">If you Add a new server to your Farm, you must register it.</para>
    /// <para type="description">Server list is usefull to restart services, and participate to notification system for updating configuration automatically without restarting ADFS service.</para>
    /// <para type="description">This Cmdlet does not support remoting.</para>
    /// </summary>
    /// <example>
    ///   <para>Register-MFAComputer</para>
    /// </example>
    [Cmdlet(VerbsLifecycle.Register, "MFAComputer", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [OutputType(typeof(ADFSServerHost))]
    public sealed class RegisterMFAComputer : MFACmdlet
    {
        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementAdminService.Initialize(this.Host, true);
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3000", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                PSHost hh = GetHostForVerbose();
                ADFSServiceManager svc = ManagementAdminService.ADFSManager;
                ADFSServerHost props = (ADFSServerHost)svc.RegisterADFSComputer(hh);
                this.WriteObject(props);
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3001", ErrorCategory.OperationStopped, this));
            }
        }
    }

    /// <summary>
    /// <para type="synopsis">UnRegister MFA Computer.</para>
    /// <para type="description">UnRegiter an ADFS server from your farm.</para>
    /// <para type="description">If you want to remove a server from your farm, you must Unregister it.</para>
    /// <para type="description">Server list is usefull to restart services, and participate to notification system for updating configuration automatically without restarting ADFS service.</para>
    /// <para type="description">This Cmdlet does not support remoting.</para>
    /// </summary>
    /// <example>
    ///   <para>Register-MFAComputer</para>
    /// </example>
    [Cmdlet(VerbsLifecycle.Unregister, "MFAComputer", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class UnRegisterMFAComputer : MFACmdlet
    {
        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementAdminService.Initialize(this.Host, true);
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3002", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            try 
            {
                PSHost hh = GetHostForVerbose();
                ADFSServiceManager svc = ManagementAdminService.ADFSManager;
                svc.UnRegisterADFSComputer(hh);
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3003", ErrorCategory.OperationStopped, this));
            }
        }
    }

    /// <summary>
    /// <para type="synopsis">Get MFA Computers.</para>
    /// <para type="description">Get ADFS Computers properties.</para>
    /// </summary>
    /// <example>
    ///   <para>Get-MFAComputers</para>
    ///   <para>Get-MFAComputers myadfsserver.domain.com</para>
    ///   <para>Get-MFAComputers -ShowCount</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "MFAComputers", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
    [OutputType(typeof(ADFSServerHost))]
    public sealed class GetMFAComputers : MFACmdlet
    {
        string _identity = string.Empty;
        private ADFSServerHost[] _list = null;

        /// <summary>
        /// <para type="description">identity of the computer (fqdn).</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 0, ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Identity
        {
            get { return _identity; }
            set { _identity = value; }
        }

        /// <summary>
        /// <para type="description">Displaye the count of computers.</para>
        /// ShowCount property
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        [Parameter(ParameterSetName = "Identity")]
        public SwitchParameter ShowCount
        {
            get;
            set;
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (string.IsNullOrEmpty(_identity))
            {
                try
                {
                    ManagementAdminService.Initialize(this.Host, true);
                    ADFSFarmHost lst = ManagementAdminService.ADFSManager.ADFSFarm;
                    _list = lst.Servers.ToArray();
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "3004", ErrorCategory.OperationStopped, _list));
                }
            }
            else
            {
                try
                {
                    ManagementAdminService.Initialize(this.Host, true);
                    ADFSFarmHost lst = ManagementAdminService.ADFSManager.ADFSFarm;
                    ADFSServerHost computer = null;
                    foreach (ADFSServerHost itm in lst.Servers)
                    {
                        if (itm.FQDN.ToLower().Equals(_identity.ToLower()))
                        {
                            computer = itm;
                            break;
                        }
                    }
                    if (computer==null)
                        throw new Exception(string.Format(errors_strings.ErrorComputerNotFound, this.Identity));
                    _list = new ADFSServerHost[] { computer };
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "3005", ErrorCategory.OperationStopped, _list));
                }
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (ShouldProcess("Select Computers"))
                {
                    foreach (ADFSServerHost computer in _list)
                        WriteObject(computer);
                }
                if (ShowCount)
                {
                    this.Host.UI.WriteLine(ConsoleColor.Yellow, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosRecordsCount, _list.LongLength.ToString()));
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3006", ErrorCategory.OperationStopped, _list));
            }
        }

        /// <summary>
        /// EndProcessing method implementation
        /// </summary>
        protected override void EndProcessing()
        {
            _list = null;
            base.EndProcessing();
        }
    }

    /// <summary>
    /// <para type="synopsis">Restart MFA Computer services.</para>
    /// <para type="description">Restart ADFS Services for one ADFS Computer.</para>
    /// </summary>
    /// <example>
    ///   <para>Restart-MFAComputerServices</para>
    ///   <para>Restart-MFAComputerServices myadfsserver.domain.com</para>
    /// </example>
    [Cmdlet(VerbsLifecycle.Restart, "MFAComputerServices", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
    public sealed class RestartMFAComputer : MFACmdlet
    {
        string _identity = string.Empty;

        /// <summary>
        /// <para type="description">identity of the computer (fqdn), if empty local computer is used.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 0, ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Identity
        {
            get { return _identity; }
            set { _identity = value; }
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementAdminService.Initialize(this.Host, true);
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3007", ErrorCategory.OperationStopped, _identity));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (ShouldProcess("Restart MFA Service"))
                {
                    if (string.IsNullOrEmpty(Identity))
                        Identity = "local";
                    PSHost hh = GetHostForVerbose();
                    ADFSServiceManager svc = ManagementAdminService.ADFSManager;
                    if (!svc.RestartServer(hh, this.Identity))
                        this.Host.UI.WriteWarningLine(errors_strings.ErrorInvalidServerName);
                    else
                        this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, string.Format(infos_strings.InfosServerServicesRestarted , this.Identity));
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3008", ErrorCategory.OperationStopped, _identity));
            }
        }

        /// <summary>
        /// EndProcessing method implementation
        /// </summary>
        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }

    /// <summary>
    /// <para type="synopsis">Restart MFA Farm Computers services.</para>
    /// <para type="description">Restart ADFS Services for all of the ADFS Computer in the Farm.</para>
    /// </summary>
    /// <example>
    ///   <para>Restart-MFAFarmServices</para>
    /// </example>
    [Cmdlet(VerbsLifecycle.Restart, "MFAFarmServices", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
    public sealed class RestartMFAFarm : MFACmdlet
    {
        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementAdminService.Initialize(this.Host, true);
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3009", ErrorCategory.OperationStopped, null));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (ShouldProcess("Restart MFA Farm services"))
                {
                    PSHost hh = GetHostForVerbose();
                    ADFSServiceManager svc = ManagementAdminService.ADFSManager;
                    svc.RestartFarm(hh);
                    this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, infos_strings.InfosAllServicesRestarted);
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3010", ErrorCategory.OperationStopped, null));
            }
        }

        /// <summary>
        /// EndProcessing method implementation
        /// </summary>
        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }

    /// <summary>
    /// <para type="synopsis">Register Configuration for MFA.</para>
    /// <para type="description">Register MFA components with ADFS. Activate it, Set Certificates and User Keys format (RNG, RSA, CUSTOM)</para>
    /// </summary>
    /// <example>
    ///   <para>Register-MFASystem</para>
    ///   <para>Create a new empty configuration for MFA. You must complete configuration with other CmdLets</para>
    /// </example>
    /// <example>
    ///   <para>Register-MFASystem -Activate -RestartFarm -KeyFormat RSA -RSACertificateDuration 10</para>
    ///   <para>Create a new empty configuration for MFA. Activation and Restart of services. Key Format is set to RSA with a cetificate for 10 Years</para>
    /// </example>
    /// <example>
    ///   <para>Register-MFASystem -Activate -RestartFarm -KeyFormat CUSTOM -RSACertificateDuration 10</para>
    ///   <para>Create a new empty configuration for MFA. Activation and Restart of services. Key Format is set to RSA CUSTOM with a cetificate for 10 Years, you must create a database for storing user keys and certificates with New-MFASecretKeysDatabase Cmdlet</para>
    /// </example>
    /// <example>
    ///   <para>Register-MFASystem -Activate -RestartFarm -AllowUpgrade -BackupFilePath c:\temp\myconfig 1.2.xml</para>
    ///   <para>Upgrade from previous versions, a copy of the current version is saved in spécified backup file </para>
    /// </example>
    [Cmdlet(VerbsLifecycle.Register, "MFASystem", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class RegisterMFASystem : MFACmdlet, IDynamicParameters
    {
        private BackupFilePathDynamicParameters Dyn;
        private MMCSecretKeyFormat _fmt = MMCSecretKeyFormat.RNG;
        private int _duration = 5;

        /// <summary>
        /// <para type="description">Active MFA as Provider in ADFS, User will be prompted for 2FA immediately.</para>
        /// Activate property
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter Activate
        {
            get;
            set;
        }

        /// <summary>
        /// <para type="description">Restart all Farm ADFS services after registration.</para>
        /// RestartFarm property
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter RestartFarm
        {
            get;
            set;
        }

        /// <summary>
        /// <para type="description">Upgrade configuration parameters only, prior configuration is not lost.</para>
        /// AllowUpgrade property
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter AllowUpgrade
        {
            get;
            set;
        }

        /// <summary>
        /// <para type="description">Change the Users SecretKey format (RNG, RSA, CUSTOM).</para>
        /// KeysFormat property
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public MMCSecretKeyFormat KeysFormat
        {
            get { return _fmt; }
            set { _fmt = value;}
        }

        /// <summary>
        /// <para type="description">Set the cerfificate(s) validity duration.</para>
        /// RSACertificateDuration property
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public int RSACertificateDuration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        /// <summary>
        /// <para type="description">Set the name of the backup file.</para>
        /// GetDynamicParameters implementation
        /// </summary>
        public object GetDynamicParameters()
        {
            if (AllowUpgrade)
            {
                Dyn = new BackupFilePathDynamicParameters();
                return Dyn;
            }
            return null;
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementAdminService.Initialize(this.Host, false);
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3011", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            try 
            {
                if (ShouldProcess("Register-MFASystem"))
                {
                    PSHost hh = GetHostForVerbose();
                    ADFSServiceManager svc = ManagementAdminService.ADFSManager;
                    if (this.AllowUpgrade)
                    {
                        if (svc.RegisterMFAProvider(hh, this.Activate, this.RestartFarm, true, Dyn.BackupFilePath, (RegistrationSecretKeyFormat)this.KeysFormat, this.RSACertificateDuration))
                            this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosSystemRegistered));
                        else
                            this.Host.UI.WriteLine(ConsoleColor.Yellow, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosSystemAlreadyInitialized));
                    }
                    else
                        if (svc.RegisterMFAProvider(hh, this.Activate, this.RestartFarm, false, null, (RegistrationSecretKeyFormat)this.KeysFormat, this.RSACertificateDuration))
                            this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosSystemRegistered));
                        else
                            this.Host.UI.WriteLine(ConsoleColor.Yellow, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosSystemAlreadyInitialized));
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3012", ErrorCategory.OperationStopped, this));
            }
        }
    }

    /// <summary>
    /// <para type="description">Set the name of the backup file.</para>
    /// </summary>
    public class BackupFilePathDynamicParameters 
    {
        /// <summary>
        /// <para type="description">Set the name of the backup file.</para>
        /// BackupFileName property
        /// </summary>
        [Parameter(ParameterSetName = "Data", Mandatory=true)]
        public string BackupFilePath
        {
            get;
            set;
        }
    }

    /// <summary>
    /// <para type="synopsis">UnRegister Configuration for MFA.</para>
    /// <para type="description">UnRegister MFA components from ADFS. DeActivate it</para>
    /// </summary>
    /// <example>
    ///   <para>UnRegister-MFASystem</para>
    ///   <para>Remove MFA configuration from ADFS. </para>
    /// </example>
    /// <example>
    ///   <para>UnRegister-MFASystem -RestartFarm -BackupFilePath c:\temp\myconfig 2.0.xml</para>
    ///   <para>Unregister MFA System, a copy of the current version is saved in spécified backup file </para>
    /// </example>
    [Cmdlet(VerbsLifecycle.Unregister, "MFASystem", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class UnRegisterMFASystem : MFACmdlet
    {
        /// <summary>
        /// <para type="description">Set the name of the backup file.</para>
        /// BackupFileName property
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public string BackupFilePath
        {
            get;
            set;
        }

        /// <summary>
        /// <para type="description">Restart all Farm ADFS services after un-registration.</para>
        /// RestartFarm property
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter RestartFarm
        {
            get;
            set;
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementAdminService.Initialize(this.Host, true);
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3013", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (ShouldProcess("UnRegister-MFASystem"))
                {
                    PSHost hh = GetHostForVerbose();
                    ADFSServiceManager svc = ManagementAdminService.ADFSManager;
                    svc.UnRegisterMFAProvider(hh, this.BackupFilePath, this.RestartFarm);
                    this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosSystemUnRegistered));

                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3014", ErrorCategory.OperationStopped, this));
            }
        }
    }

    /// <summary>
    /// <para type="synopsis">Enable MFA.</para>
    /// <para type="description">Enable MFA System (if initialized)</para>
    /// </summary>
    /// <example>
    ///   <para>Enable-MFASystem</para>
    ///   <para>Enable MFA configuration from ADFS. </para>
    /// </example>
    [Cmdlet(VerbsLifecycle.Enable, "MFASystem", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class EnableMFASystem : MFACmdlet
    {
        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementAdminService.Initialize(this.Host, true);
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3015", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (ShouldProcess("Enable-MFASystem"))
                {
                    PSHost hh = GetHostForVerbose();
                    ADFSServiceManager svc = ManagementAdminService.ADFSManager;
                    svc.EnableMFAProvider(hh);
                    this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosSystemRegistered));
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3016", ErrorCategory.OperationStopped, this));
            }
        }
    }

    /// <summary>
    /// <para type="synopsis">Disable MFA.</para>
    /// <para type="description">Disable MFA System (if initialized)</para>
    /// </summary>
    /// <example>
    ///   <para>Disable-MFASystem</para>
    ///   <para>Disable MFA configuration from ADFS. </para>
    /// </example>
    [Cmdlet(VerbsLifecycle.Disable, "MFASystem", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class DisableMFASystem : MFACmdlet
    {
        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementAdminService.Initialize(this.Host, true);
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3017", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (ShouldProcess("Disable-MFASystem"))
                {
                    PSHost hh = GetHostForVerbose();
                    ADFSServiceManager svc = ManagementAdminService.ADFSManager;
                    svc.DisableMFAProvider(hh);
                    this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosSystemUnRegistered));
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3018", ErrorCategory.OperationStopped, this));
            }
        }
    }

    /// <summary>
    /// <para type="synopsis">Install RSA Certificate.</para>
    /// <para type="description">Install a new RSA Certificate for MFA.</para>
    /// </summary>
    /// <example>
    ///   <para>Install-MFACertificate</para>
    ///   <para>Create a new certificate for RSA User keys.</para>
    /// </example>
    /// <example>
    ///   <para>Install-MFACertificate -RSACertificateDuration 10 -RestartFarm</para>
    ///   <para>Create a new certificate for RSA User keys with specific duration.</para>
    /// </example>
    [Cmdlet(VerbsLifecycle.Install, "MFACertificate", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class InstallMFACertificate : MFACmdlet
    {
        private int _duration = 5;
        private SwitchParameter _restart = true;

        /// <summary>
        /// <para>Duration for the new certificate (Years)</para>
        /// RSACertificateDuration property
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public int RSACertificateDuration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        /// <summary>
        /// <para>Restart Farm Services</para>
        /// RestartFarm property
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter RestartFarm
        {
            get { return _restart; }
            set { _restart = value;}
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementAdminService.Initialize(this.Host, true);
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3019", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (ShouldProcess("Install-MFACertificate"))
                {
                    Collection<ChoiceDescription> col = new Collection<ChoiceDescription>();
                    ChoiceDescription c1 = new ChoiceDescription("&Yes") ;
                    ChoiceDescription c2 = new ChoiceDescription("&No") ;
                    col.Add(c1);
                    col.Add(c2);
                    if (this.Host.UI.PromptForChoice("Install-MFACertificate", infos_strings.InfoAllKeyWillbeReset, col, 1) == 0)
                    {
                        PSHost hh = GetHostForVerbose();
                        ADFSServiceManager svc = ManagementAdminService.ADFSManager;
                        svc.RegisterNewRSACertificate(hh, this.RSACertificateDuration, this.RestartFarm);
                        this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosRSACertificateChanged));
                    }
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3020", ErrorCategory.OperationStopped, this));
            }
        }
    }

    /// <summary>
    /// <para type="synopsis">Get Configuration.</para>
    /// <para type="description">Get main configuration options.</para>
    /// </summary>
    /// <example>
    ///   <para>Get-MFAConfig</para>
    ///   <para>Display MFA configuration</para>
    /// </example>
    /// <example>
    ///   <para>$cfg = Get-MFAConfig</para>
    ///   <para>Get MFA configuration options and store it into variable.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "MFAConfig", SupportsShouldProcess = true, SupportsPaging = false, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [OutputType(typeof(PSConfig))]
    public sealed class GetMFAConfig : MFACmdlet
    {
        private PSConfig _config;
        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {

                MMCConfig cf = new MMCConfig();
                cf.Load(this.Host);
                _config = (PSConfig)cf;
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3021", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// EndProcessing method implementation
        /// </summary>
        protected override void EndProcessing()
        {
            _config = null;
            base.EndProcessing();
        }

        /// <summary>
        /// ProcessInternalRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (ShouldProcess("MFA Configuration"))
                {
                    WriteObject(_config);
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3022", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// StopProcessing method implementation
        /// </summary>
        protected override void StopProcessing()
        {
            _config = null;
            base.StopProcessing();
        }
    }

    /// <summary>
    /// <para type="synopsis">Get Configuration.</para>
    /// <para type="description">Get main configuration options.</para>
    /// </summary>
    /// <example>
    ///   <para>Set-MFAConfig -Config $cfg</para>
    ///   <para>Update MFA configuration</para>
    /// </example>
    /// <example>
    ///   <para>$cfg = Get-MFAConfig</para>
    ///   <para>$cfg.DefaultCountryCode = "us"</para>
    ///   <para>Set-MFAConfig $cfg</para>
    ///   <para>Get MFA configuration options, modity values and finally Update configuration.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "MFAConfig", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class SetMFAConfig : MFACmdlet
    {
        private PSConfig _config;
        private MMCConfig _target;

        /// <summary>
        /// <para type="description">Config parameter, a variable of type PSConfig.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data", ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty()]
        public PSConfig Config
        {
            get { return _config; }
            set { _config = value; }
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (_config != null)
            {
                try
                {
                    _target = (MMCConfig)_config;
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "3023", ErrorCategory.OperationStopped, this));
                }
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ShouldProcess("MFA Configuration"))
            {
                try
                {
                    _target.Update(this.Host);
                    this.WriteVerbose(infos_strings.InfosConfigUpdated);
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "3024", ErrorCategory.OperationStopped, this));
                }
            }
        }
    }

    /// <summary>
    /// <para type="synopsis">Set Policy Template.</para>
    /// <para type="description">Set Policy template, for managing security features like ChangePassword, Auto-registration and more.</para>
    /// </summary>
    /// <example>
    ///   <para>Set-MFAPolicyTemplate -Template Strict</para>
    ///   <para>Change policy template for MFA configuration</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "MFAPolicyTemplate", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class SetMFATemplateMode : MFACmdlet
    {
        private PSTemplateMode _template;
        private MMCTemplateMode _target;
        private MMCConfig _config;

        /// <summary>
        /// <para type="description">Template enumeration member. </para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data", ValueFromPipeline = false)]
        [ValidateNotNullOrEmpty()]
        public PSTemplateMode Template
        {
            get { return _template; }
            set { _template = value; }
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                _config = new MMCConfig();
                _config.Load(this.Host);
                _target = (MMCTemplateMode)_template;
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3023", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ShouldProcess("MFA Template Configuration"))
            {
                try
                {
                    _config.SetTemplate(this.Host, _target);
                    this.WriteVerbose(infos_strings.InfosConfigUpdated);
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "3024", ErrorCategory.OperationStopped, this));
                }
            }
        }

        /// <summary>
        /// StopProcessing method implementation
        /// </summary>
        protected override void StopProcessing()
        {
            _config = null;
            base.StopProcessing();
        }
    }

    /// <summary>
    /// <para type="synopsis">Get SQL Configuration.</para>
    /// <para type="description">Get SQL configuration (UseActiveDirectory==false).</para>
    /// </summary>
    /// <example>
    ///   <para>Get-MFAConfigSQL</para>
    ///   <para>Get MFA configuration when using SQL Database</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "MFAConfigSQL", SupportsShouldProcess = true, SupportsPaging = false, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [OutputType(typeof(PSConfigSQL))]
    public sealed class GetMFAConfigSQL : MFACmdlet
    {
        private PSConfigSQL _config;
        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {

                MMCConfigSQL cf = new MMCConfigSQL();
                cf.Load(this.Host);
                _config = (PSConfigSQL)cf;
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3021", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// EndProcessing method implementation
        /// </summary>
        protected override void EndProcessing()
        {
            _config = null;
            base.EndProcessing();
        }

        /// <summary>
        /// ProcessInternalRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (ShouldProcess("MFA SQL Configuration"))
                {
                    WriteObject(_config);
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3022", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// StopProcessing method implementation
        /// </summary>
        protected override void StopProcessing()
        {
            _config = null;
            base.StopProcessing();
        }
    }

    /// <summary>
    /// <para type="synopsis">Set SQL Configuration.</para>
    /// <para type="description">Set SQL configuration (UseActiveDirectory==false).</para>
    /// </summary>
    /// <example>
    ///   <para>Set-MFAConfigSQL -Config $cfg</para>
    ///   <para>Set MFA configuration when using SQL Database</para>
    /// </example>
    /// <example>
    ///   <para>$cfg = Get-MFAConfigSQL</para>
    ///   <para>$cfg.connectionString = "your SQL connection string"</para>
    ///   <para>Set-MFAConfigSQL $cfg</para>
    ///   <para>Set MFA SQL configuration options, modity values and finally Update configuration.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "MFAConfigSQL", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class SetMFAConfigSQL : MFACmdlet
    {
        private PSConfigSQL _config;
        private MMCConfigSQL _target;

        /// <summary>
        /// <para type="description">Config parameter, a variable of type PSConfigSQL.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data", ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty()]
        public PSConfigSQL Config
        {
            get { return _config; }
            set { _config = value; }
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (_config != null)
            {
                try
                {
                    _target = (MMCConfigSQL)_config;
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "3023", ErrorCategory.OperationStopped, this));
                }
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ShouldProcess("MFA SQL Configuration"))
            {
                try
                {
                    _target.Update(this.Host);
                    this.WriteVerbose(infos_strings.InfosConfigUpdated);
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "3024", ErrorCategory.OperationStopped, this));
                }
            }
        }
    }

    /// <summary>
    /// <para type="synopsis">Get Active Directory Configuration.</para>
    /// <para type="description">Get ADDS configuration (UseActiveDirectory==true).</para>
    /// </summary>
    /// <example>
    ///   <para>Get-MFAConfigADDS</para>
    ///   <para>Get MFA configuration when using Active Directory</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "MFAConfigADDS", SupportsShouldProcess = true, SupportsPaging = false, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [OutputType(typeof(PSConfigADDS))]
    public sealed class GetMFAConfigADDS : MFACmdlet
    {
        private PSConfigADDS _config;
        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {

                MMCConfigADDS cf = new MMCConfigADDS();
                cf.Load(this.Host);
                _config = (PSConfigADDS)cf;
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3021", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// EndProcessing method implementation
        /// </summary>
        protected override void EndProcessing()
        {
            _config = null;
            base.EndProcessing();
        }

        /// <summary>
        /// ProcessInternalRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (ShouldProcess("MFA ADDS Configuration"))
                {
                    WriteObject(_config);
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3022", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// StopProcessing method implementation
        /// </summary>
        protected override void StopProcessing()
        {
            _config = null;
            base.StopProcessing();
        }
    }

    /// <summary>
    /// <para type="synopsis">Set Active Directory Configuration.</para>
    /// <para type="description">Set SQL configuration (UseActiveDirectory==true).</para>
    /// </summary>
    /// <example>
    ///   <para>Set-MFAConfigADDS -Config $cfg</para>
    ///   <para>Set MFA configuration when using Active Directory</para>
    /// </example>
    /// <example>
    ///   <para>$cfg = Get-MFAConfigADDS</para>
    ///   <para>$cfg.MailAttribute = "your ADDS mail attibute"</para>
    ///   <para>Set-MFAConfigADDS $cfg</para>
    ///   <para>Set MFA ADDS configuration options, modity values and finally Update configuration.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "MFAConfigADDS", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class SetMFAConfigADDS : MFACmdlet
    {
        private PSConfigADDS _config;
        private MMCConfigADDS _target;

        /// <summary>
        /// <para type="description">Config parameter, a variable of type PSConfigADDS.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data", ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty()]
        public PSConfigADDS Config
        {
            get { return _config; }
            set { _config = value; }
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (_config != null)
            {
                try
                {
                    _target = (MMCConfigADDS)_config;
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "3023", ErrorCategory.OperationStopped, this));
                }
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ShouldProcess("MFA ADDS Configuration"))
            {
                try
                {
                    _target.Update(this.Host);
                    this.WriteVerbose(infos_strings.InfosConfigUpdated);
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "3024", ErrorCategory.OperationStopped, this));
                }
            }
        }
    }

    /// <summary>
    /// <para type="synopsis">Get SMTP Configuration.</para>
    /// <para type="description">Get SMTP configuration options.</para>
    /// </summary>
    /// <example>
    ///   <para>Get-MFAConfigMails</para>
    ///   <para>Get MFA configuration for SMTP</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "MFAConfigMails", SupportsShouldProcess = true, SupportsPaging = false, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [OutputType(typeof(PSConfigMail))]
    public sealed class GetMFAConfigMails : MFACmdlet
    {
        private PSConfigMail _config;
        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {

                MMCConfigMail cf = new MMCConfigMail();
                cf.Load(this.Host);
                _config = (PSConfigMail)cf;
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3021", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// EndProcessing method implementation
        /// </summary>
        protected override void EndProcessing()
        {
            _config = null;
            base.EndProcessing();
        }

        /// <summary>
        /// ProcessInternalRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (ShouldProcess("MFA Mails Configuration"))
                {
                    WriteObject(_config);
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3022", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// StopProcessing method implementation
        /// </summary>
        protected override void StopProcessing()
        {
            _config = null;
            base.StopProcessing();
        }
    }

    /// <summary>
    /// <para type="synopsis">Set SMTP Configuration.</para>
    /// <para type="description">Set SMTP configuration options.</para>
    /// </summary>
    /// <example>
    ///   <para>Set-MFAConfigMails -Config $cfg</para>
    ///   <para>Set MFA configuration for SMTP</para>
    /// </example>
    /// <example>
    ///   <para>$cfg = Get-MFAConfigMails</para>
    ///   <para>$cfg.Host = smtp.office365.com</para>
    ///   <para>Set-MFAConfigMails $cfg</para>
    ///   <para>Set MFA SMTP configuration options, modity values and finally Update configuration.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "MFAConfigMails", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class SetMFAConfigMails : MFACmdlet
    {
        private PSConfigMail _config;
        private MMCConfigMail _target;

        /// <summary>
        /// <para type="description">Config parameter, a variable of type PSConfigMail.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data", ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty()]
        public PSConfigMail Config
        {
            get { return _config; }
            set { _config = value; }
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (_config != null)
            {
                try
                {
                    _target = (MMCConfigMail)_config;
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "3023", ErrorCategory.OperationStopped, this));
                }
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ShouldProcess("MFA Mails Configuration"))
            {
                try
                {
                    _target.Update(this.Host);
                    this.WriteVerbose(infos_strings.InfosConfigUpdated);
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "3024", ErrorCategory.OperationStopped, this));
                }
            }
        }
    }

    /// <summary>
    /// <para type="synopsis">Get Secret keys Configuration.</para>
    /// <para type="description">Get Secret keys configuration options.</para>
    /// </summary>
    /// <example>
    ///   <para>Get-MFAConfigKeys</para>
    ///   <para>Get MFA Secret Key configuration options</para>
    /// </example>
    /// <example>
    ///  <para>(Get-MFAConfigKeys).ExternalKeyManager.FullQualifiedImplementation</para>
    ///  <para>(Get-MFAConfigKeys).ExternalKeyManager.Parameters</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "MFAConfigKeys", SupportsShouldProcess = true, SupportsPaging = false, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [OutputType(typeof(PSKeysConfig))]
    public sealed class GetMFAConfigKeys : MFACmdlet
    {
        private PSKeysConfig _config;

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {

                MMCKeysConfig cf = new MMCKeysConfig();
                cf.Load(this.Host);
                _config = (PSKeysConfig)cf;
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3021", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// EndProcessing method implementation
        /// </summary>
        protected override void EndProcessing()
        {
            _config = null;
            base.EndProcessing();
        }

        /// <summary>
        /// ProcessInternalRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (ShouldProcess("MFA Secure Keys Configuration"))
                {
                    WriteObject(_config);
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3022", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// StopProcessing method implementation
        /// </summary>
        protected override void StopProcessing()
        {
            _config = null;
            base.StopProcessing();
        }
    }

    /// <summary>
    /// <para type="synopsis">Set Secret Keys Configuration.</para>
    /// <para type="description">Set Secret Keys configuration options.</para>
    /// </summary>
    /// <example>
    ///   <para>Set-MFAConfigKeys -Config $cfg</para>
    ///   <para>Set MFA Secret Keys configuration</para>
    /// </example>
    /// <example>
    ///   <para>$cfg = Get-MFAConfigKeys</para>
    ///   <para>$cfg.KeyFormat = RSA</para>
    ///   <para>Set-MFAConfigKeys $cfg</para>
    ///   <para>Set MFA Secret Keys configuration options, modity values and finally Update configuration.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "MFAConfigKeys", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class SetMFAConfigKeys : MFACmdlet
    {
        private PSKeysConfig _config;
        private MMCKeysConfig _target;

        /// <summary>
        /// <para type="description">Config parameter, a variable of type PSKeyConfig.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data", ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty()]
        public PSKeysConfig Config
        {
            get { return _config; }
            set { _config = value; }
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (_config != null)
            {
                try
                {
                    _target = (MMCKeysConfig)_config;
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "3023", ErrorCategory.OperationStopped, this));
                }
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ShouldProcess("MFA Secure Keys Configuration"))
            {
                try
                {
                    _target.Update(this.Host);
                    this.WriteVerbose(infos_strings.InfosConfigUpdated);
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "3024", ErrorCategory.OperationStopped, this));
                }
            }
        }
    }

    /// <summary>
    /// <para type="synopsis">Get External OTP Provider Configuration.</para>
    /// <para type="description">Get External OTP Provider configuration options.</para>
    /// </summary>
    /// <example>
    ///   <para>Get-MFAExternalOTPProvider</para>
    ///   <para>Get MFA External OTP Provider configuration options</para>
    /// </example>
    /// <example>
    ///  <para>(Get-MFAExternalOTPProvider).FullQualifiedImplementation</para>
    ///  <para>(Get-MFAExternalOTPProvider).Parameters</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "MFAExternalOTPProvider", SupportsShouldProcess = true, SupportsPaging = false, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [OutputType(typeof(PSExternalOTPProvider))]
    public sealed class GetMFAExternalOTPProvider : MFACmdlet
    {
        private PSExternalOTPProvider _config;

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {

                MMCExternalOTPProvider cf = new MMCExternalOTPProvider();
                cf.Load(this.Host);
                _config = (PSExternalOTPProvider)cf;
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3021", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// EndProcessing method implementation
        /// </summary>
        protected override void EndProcessing()
        {
            _config = null;
            base.EndProcessing();
        }

        /// <summary>
        /// ProcessInternalRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (ShouldProcess("MFA External OTP Provider Configuration"))
                {
                    WriteObject(_config);
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3022", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// StopProcessing method implementation
        /// </summary>
        protected override void StopProcessing()
        {
            _config = null;
            base.StopProcessing();
        }
    }

    /// <summary>
    /// <para type="synopsis">Set External OTP Provider Configuration.</para>
    /// <para type="description">Set External OTP Provider configuration options.</para>
    /// </summary>
    /// <example>
    ///   <para>Set-MFAExternalOTPProvider -Config $cfg</para>
    ///   <para>Set MFA External OTP Provider configuration</para>
    /// </example>
    /// <example>
    ///   <para>$cfg = Get-MFAExternalOTPProvider</para>
    ///   <para>$cfg.Parameters.Value = "your parameters as string"</para>
    ///   <para>Set-MFAExternalOTPProvider $cfg</para>
    ///   <para>Set MFA External OTP Provider configuration options, modity values and finally Update configuration.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "MFAExternalOTPProvider", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class SetMFAExternalOTPProvider : MFACmdlet
    {
        private PSExternalOTPProvider _config;
        private MMCExternalOTPProvider _target;

        /// <summary>
        /// <para type="description">Config parameter, a variable of type PSExternalOTPProvider.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data", ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty()]
        public PSExternalOTPProvider Config
        {
            get { return _config; }
            set { _config = value; }
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (_config != null)
            {
                try
                {
                    _target = (MMCExternalOTPProvider)_config;
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "3023", ErrorCategory.OperationStopped, this));
                }
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ShouldProcess("MFA External OTP Provider Configuration"))
            {
                try
                {
                    _target.Update(this.Host);
                    this.WriteVerbose(infos_strings.InfosConfigUpdated);
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "3024", ErrorCategory.OperationStopped, this));
                }
            }
        }
    }

    /// <summary>
    /// <para type="synopsis">Create a new SQL Database for MFA.</para>
    /// <para type="description">Create a new SQL Database for MFA Configuration (UseActiveDirectory==false).</para>
    /// </summary>
    /// <example>
    ///   <para>New-MFADatabase -ServerName SQLServer\Instance -DatabaseName MFADatabase -UserName sqlaccount -Password pass</para>
    ///   <para>New-MFADatabase -ServerName SQLServer\Instance -DatabaseName MFADatabase -UserName Domain\ADFSaccount</para>
    ///   <para>New-MFADatabase -ServerName SQLServer\Instance -DatabaseName MFADatabase -UserName Domain\ADFSManagedAccount$</para>
    ///   <para>Create a new database for MFA, grant rights to the specified account</para>
    /// </example>
    [Cmdlet(VerbsCommon.New, "MFADatabase", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class NewMFADatabase : MFACmdlet
    {
        private string _servername;
        private string _databasename;
        private string _username;
        private string _password;
        private PSConfigSQL _config;

        /// <summary>
        /// <para type="description">SQL ServerName, you must include Instance if needed eg : server\instance.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        [ValidateNotNullOrEmpty()]
        public string ServerName
        {
            get { return _servername; }
            set { _servername = value; }
        }

        /// <summary>
        /// <para type="description">Name of the SQL Database, if database exists an error is thrown.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        [ValidateNotNullOrEmpty()]
        public string DatabaseName
        {
            get { return _databasename; }
            set { _databasename = value; }
        }

        /// <summary>
        /// <para type="description">AccountName, can be a domain, managed account : domain\adfsaccount or domain\adfsaccount$ without password, or an SQL Account with password.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        [ValidateNotNullOrEmpty()]
        public string UserName
        {
            get { return _username; }
            set { _username = value; }
        }

        /// <summary>
        /// <para type="description">Password, only if the account is a SQL account, for ADFS domain do not specify password.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementAdminService.Initialize(this.Host, true);
                MMCConfigSQL cf = new MMCConfigSQL();
                cf.Load(this.Host);
                _config = (PSConfigSQL)cf;
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3021", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (ShouldProcess("MFA SQL Database creation (must be sysadmin or dbcreator an securityadmin)"))
                {
                    PSHost hh = GetHostForVerbose();
                    ADFSServiceManager svc = ManagementAdminService.ADFSManager;
                    svc.CreateMFADatabase(hh, _servername, _databasename, _username, _password);
                    this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, string.Format(infos_strings.InfosDatabaseCreated, _databasename));

                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3022", ErrorCategory.OperationStopped, this));
            }
        }
    }

    /// <summary>
    /// <para type="synopsis">Create a new SQL Database for storing RSA keys and certificates.</para>
    /// <para type="description">Create a new SQL Database for storing RSA keys and certificates, can be used with ADDS and SQL configuration.</para>
    /// </summary>
    /// <example>
    ///   <para>MFASecretKeysDatabase -ServerName SQLServer\Instance -DatabaseName MFAKeysDatabase -UserName sqlaccount -Password pass</para>
    ///   <para>MFASecretKeysDatabase -ServerName SQLServer\Instance -DatabaseName MFAKeysDatabase -UserName Domain\ADFSaccount</para>
    ///   <para>MFASecretKeysDatabase -ServerName SQLServer\Instance -DatabaseName MFAKeysDatabase -UserName Domain\ADFSManagedAccount$</para>
    ///   <para>(Get-MFAConfigKeys).KeyFormat must be equal to CUSTOM to be effective
    ///   <para>Create a new database for MFA Secret Keys, grant rights to the specified account</para>
    /// </example>
    [Cmdlet(VerbsCommon.New, "MFASecretKeysDatabase", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class NewMFASecretKeysDatabase : MFACmdlet
    {
        private string _servername;
        private string _databasename;
        private string _username;
        private string _password;
        private PSConfigSQL _config;

        /// <summary>
        /// <para type="description">SQL ServerName, you must include Instance if needed eg : server\instance.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        [ValidateNotNullOrEmpty()]
        public string ServerName
        {
            get { return _servername; }
            set { _servername = value; }
        }

        /// <summary>
        /// <para type="description">Name of the SQL Database, if database exists an error is thrown.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        [ValidateNotNullOrEmpty()]
        public string DatabaseName
        {
            get { return _databasename; }
            set { _databasename = value; }
        }

        /// <summary>
        /// <para type="description">AccountName, can be a domain, managed account : domain\adfsaccount or domain\adfsaccount$ without password, or an SQL Account with password.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        [ValidateNotNullOrEmpty()]
        public string UserName
        {
            get { return _username; }
            set { _username = value; }
        }

        /// <summary>
        /// <para type="description">Password, only if the account is a SQL account, for ADFS domain do not specify password.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementAdminService.Initialize(this.Host, true);
                MMCConfigSQL cf = new MMCConfigSQL();
                cf.Load(this.Host);
                _config = (PSConfigSQL)cf;
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3021", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (ShouldProcess("MFA Secret Keys Database creation (must be sysadmin or dbcreator an securityadmin)"))
                {
                    PSHost hh = GetHostForVerbose();
                    ADFSServiceManager svc = ManagementAdminService.ADFSManager;
                    svc.CreateMFASecretKeysDatabase(hh, _servername, _databasename, _username, _password);
                    this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, string.Format(infos_strings.InfosDatabaseCreated, _databasename));

                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3022", ErrorCategory.OperationStopped, this));
            }
        }
    }
    #endregion  
}
