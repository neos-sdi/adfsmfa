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

        [Parameter(Mandatory = false, Position=0, ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Identity
        {
            get { return _identity; }
            set { _identity = value; }
        }

        [Parameter(ParameterSetName = "Data")]
        public int MaxRows
        {
            get { return _maxrows; }
            set { _maxrows = value; }
        }

        [Parameter(ParameterSetName = "Identity")]
        [Parameter(ParameterSetName = "Data")]
        [Alias("All")]
        public SwitchParameter IncludeDisabled
        {
            get { return !_filter.EnabledOnly; }
            set { _filter.EnabledOnly = !value; }
        }

        [Parameter(ParameterSetName = "Data")]
        [Alias("Field", "ATTR")]
        public UsersFilterField FilterField
        {
            get { return _filter.FilterField; }
            set { _filter.FilterField = value; }
        }

        [Parameter(ParameterSetName = "Data")]
        [Alias("Operator", "OP")]
        public UsersFilterOperator FilterOperator
        {
            get { return _filter.FilterOperator; }
            set {_filter.FilterOperator = value; }
        }

        [Parameter(ParameterSetName = "Data")]
        [Alias("Value", "VAL")]
        public string FilterValue
        {
            get { return _filter.FilterValue; }
            set { _filter.FilterValue = value; }
        }

        [Parameter(ParameterSetName = "Data")]
        [Alias("Method")]
        public UsersPreferredMethod FilterMethod
        {
            get { return _filter.FilterMethod; }
            set { _filter.FilterMethod = value; }
        }

        [Parameter(ParameterSetName = "Data")]
        public int PagingSize
        {
            get;
            set;
        }

        [Parameter(ParameterSetName = "Data")]
        public int CurrentPage
        {
            get;
            set;
        }

        /// <summary>
        /// ShowTotalCount property
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter ShowCount
        {
            get;
            set;
        }

        [Parameter(ParameterSetName = "Data")]
        public UsersOrderField SortOrder
        {
            get { return _order.Column; }
            set { _order.Column = value; }
        }

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

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Identity
        {
            get { return _identity; }
            set { _identity = value; }
        }

        [Parameter(Mandatory = false, ParameterSetName = "Identity")]
        [Parameter(ParameterSetName = "Data")]
        [Alias("Email")]
        public string MailAddress
        {
            get { return _mailaddress; }
            set { _mailaddress = value; }
        }

        [Parameter(Mandatory = false, ParameterSetName = "Identity")]
        [Parameter(ParameterSetName = "Data")]
        [Alias("Phone")]
        public string PhoneNumber
        {
            get { return _phonenumber; }
            set { _phonenumber = value; }
        }

        [Parameter(ParameterSetName = "Identity")]
        [Parameter(ParameterSetName = "Data")]
        [ValidateRange(UsersPreferredMethod.Choose, UsersPreferredMethod.None)]
        public UsersPreferredMethod Method
        {
            get { return _method; }
            set { _method = value; }
        }

        [Parameter(ParameterSetName = "Identity")]
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        [Parameter(ParameterSetName = "Identity")]
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter ResetKey
        {
            get { return _resetkey; }
            set { _resetkey = value; }
        }

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

    [Cmdlet(VerbsCommon.Add, "MFAUsers", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
    public sealed class AddMFAUser : MFACmdlet
    {
        private string _identity = string.Empty;
        private string _mailaddress = string.Empty;
        private string _phonenumber = string.Empty;
        private UsersPreferredMethod _method = UsersPreferredMethod.None;
        private bool _enabled = true;

        PSRegistration[] _data = null;


        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Identity
        {
            get { return _identity; }
            set { _identity = value; }
        }

        [Parameter(Mandatory = false, ParameterSetName = "Identity")]
        [Alias("Email")]
        public string MailAddress
        {
            get { return _mailaddress; }
            set { _mailaddress = value; }
        }

        [Parameter(Mandatory = false, ParameterSetName = "Identity")]
        [Alias("Phone")]
        public string PhoneNumber
        {
            get { return _phonenumber; }
            set { _phonenumber = value; }
        }

        [Parameter(ParameterSetName = "Identity")]
        [ValidateRange(UsersPreferredMethod.Choose, UsersPreferredMethod.None)]
        public UsersPreferredMethod Method
        {
            get { return _method; }
            set { _method = value; }
        }

        [Parameter(ParameterSetName = "Identity")]
        public SwitchParameter Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

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

    [Cmdlet(VerbsCommon.Remove, "MFAUsers", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
    public sealed class DeleteMFAUser : MFACmdlet
    {
        string _identity = string.Empty;
        PSRegistration[] _data = null;

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Identity
        {
            get { return _identity; }
            set { _identity = value; }
        }

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

    [Cmdlet(VerbsLifecycle.Enable, "MFAUsers", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
    [OutputType(typeof(PSRegistration[]))]
    public sealed class EnableMFAUser : MFACmdlet
    {
        string _identity = string.Empty;
        PSRegistration[] _data = null;

        [Parameter(Mandatory = true, Position=0, ParameterSetName="Identity")]
        [ValidateNotNullOrEmpty()]
        public string Identity
        {
            get { return _identity; }
            set { _identity = value; }
        }

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

    [Cmdlet(VerbsLifecycle.Disable, "MFAUsers", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
    [OutputType(typeof(PSRegistration[]))]
    public sealed class DisableMFAUser : MFACmdlet
    {
        string _identity = string.Empty;
        PSRegistration[] _data = null;

        [Parameter(Mandatory = true, Position = 0, ParameterSetName="Identity")]
        [ValidateNotNullOrEmpty()]
        public string Identity
        {
            get { return _identity; }
            set { _identity = value; }
        }

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

    [Cmdlet(VerbsCommon.Get, "MFAComputers", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
    [OutputType(typeof(ADFSServerHost))]
    public sealed class GetMFAComputers : MFACmdlet
    {
        string _identity = string.Empty;
        private ADFSServerHost[] _list = null;

        [Parameter(Mandatory = false, Position = 0, ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Identity
        {
            get { return _identity; }
            set { _identity = value; }
        }

        /// <summary>
        /// ShowTotalCount property
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

    [Cmdlet(VerbsLifecycle.Restart, "MFAComputer", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
    public sealed class RestartMFAComputer : MFACmdlet
    {
        string _identity = string.Empty;

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

    [Cmdlet(VerbsLifecycle.Restart, "MFAFarm", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
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

    [Cmdlet(VerbsLifecycle.Register, "MFASystem", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class RegisterMFASystem : MFACmdlet, IDynamicParameters
    {
        private BackupFilePathDynamicParameters Dyn;
        private MMCSecretKeyFormat _fmt = MMCSecretKeyFormat.RNG;
        private int _duration = 5;

        /// <summary>
        /// Activate property
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter Activate
        {
            get;
            set;
        }

        /// <summary>
        /// RestartFarm property
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter RestartFarm
        {
            get;
            set;
        }

        /// <summary>
        /// AllowUpgrade property
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter AllowUpgrade
        {
            get;
            set;
        }

        /// <summary>
        /// KeysFormat property
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public MMCSecretKeyFormat KeysFormat
        {
            get { return _fmt; }
            set { _fmt = value;}
        }

        /// <summary>
        /// RSACertificateDuration property
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public int RSACertificateDuration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        /// <summary>
        /// GetDynamicParameters implementation
        /// </summary>
        /// <returns></returns>
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

    public class BackupFilePathDynamicParameters 
    {
        /// <summary>
        /// BackupFileName property
        /// </summary>
        [Parameter(ParameterSetName = "Data", Mandatory=true)]
        public string BackupFilePath
        {
            get;
            set;
        }
    }

    [Cmdlet(VerbsLifecycle.Unregister, "MFASystem", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class UnRegisterMFASystem : MFACmdlet
    {
        /// <summary>
        /// BackupFileName property
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public string BackupFilePath
        {
            get;
            set;
        }

        /// <summary>
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

    [Cmdlet(VerbsLifecycle.Install, "MFACertificate", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class InstallMFACertificate : MFACmdlet
    {
        private int _duration = 5;
        private SwitchParameter _restart = true;

        /// <summary>
        /// RSACertificateDuration property
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public int RSACertificateDuration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        /// <summary>
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

    [Cmdlet(VerbsCommon.Set, "MFAConfig", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class SetMFAConfig : MFACmdlet
    {
        private PSConfig _config;
        private MMCConfig _target; 

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

    [Cmdlet(VerbsCommon.Set, "MFATemplate", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class SetMFATemplateMode : MFACmdlet
    {
        private PSTemplateMode _template;
        private MMCTemplateMode _target;
        private MMCConfig _config;

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
            if (_template != null)
            {
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

    [Cmdlet(VerbsCommon.Set, "MFAConfigSQL", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class SetMFAConfigSQL : MFACmdlet
    {
        private PSConfigSQL _config;
        private MMCConfigSQL _target;

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

    [Cmdlet(VerbsCommon.Set, "MFAConfigADDS", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class SetMFAConfigADDS : MFACmdlet
    {
        private PSConfigADDS _config;
        private MMCConfigADDS _target;

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

    [Cmdlet(VerbsCommon.Set, "MFAConfigMails", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class SetMFAConfigMails : MFACmdlet
    {
        private PSConfigMail _config;
        private MMCConfigMail _target;

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

    [Cmdlet(VerbsCommon.Set, "MFAConfigKeys", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class SetMFAConfigKeys : MFACmdlet
    {
        private PSKeysConfig _config;
        private MMCKeysConfig _target;

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

    [Cmdlet(VerbsCommon.Set, "MFAExternalOTPProvider", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class SetMFAExternalOTPProvider : MFACmdlet
    {
        private PSExternalOTPProvider _config;
        private MMCExternalOTPProvider _target;

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

    [Cmdlet(VerbsCommon.New, "MFADatabase", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class NewMFADatabase : MFACmdlet
    {
        private string _servername;
        private string _databasename;
        private string _username;
        private string _password;
        private PSConfigSQL _config;

        [Parameter(ParameterSetName = "Data")]
        [ValidateNotNullOrEmpty()]
        public string ServerName
        {
            get { return _servername; }
            set { _servername = value; }
        }

        [Parameter(ParameterSetName = "Data")]
        [ValidateNotNullOrEmpty()]
        public string DatabaseName
        {
            get { return _databasename; }
            set { _databasename = value; }
        }

        [Parameter(ParameterSetName = "Data")]
        [ValidateNotNullOrEmpty()]
        public string UserName
        {
            get { return _username; }
            set { _username = value; }
        }

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

    [Cmdlet(VerbsCommon.New, "MFASecretKeysDatabase", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class NewMFASecretKeysDatabase : MFACmdlet
    {
        private string _servername;
        private string _databasename;
        private string _username;
        private string _password;
        private PSConfigSQL _config;

        [Parameter(ParameterSetName = "Data")]
        [ValidateNotNullOrEmpty()]
        public string ServerName
        {
            get { return _servername; }
            set { _servername = value; }
        }

        [Parameter(ParameterSetName = "Data")]
        [ValidateNotNullOrEmpty()]
        public string DatabaseName
        {
            get { return _databasename; }
            set { _databasename = value; }
        }

        [Parameter(ParameterSetName = "Data")]
        [ValidateNotNullOrEmpty()]
        public string UserName
        {
            get { return _username; }
            set { _username = value; }
        }

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
