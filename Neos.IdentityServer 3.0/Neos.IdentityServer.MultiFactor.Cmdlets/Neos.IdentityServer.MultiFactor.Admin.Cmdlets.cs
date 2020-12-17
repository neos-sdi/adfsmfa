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
using Neos.IdentityServer.MultiFactor.Cmdlets;

namespace MFA
{
    using Neos.IdentityServer.MultiFactor;
    using Neos.IdentityServer.MultiFactor.Administration;
    using Neos.IdentityServer.MultiFactor.Cmdlets.Ressources;
    using System;
    using System.Collections.ObjectModel;
    using System.DirectoryServices;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Net;

    #region Cmdlet Base
    /// <summary>
    /// MFACmdlet class
    /// </summary>
    public class MFACmdlet : PSCmdlet
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

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            Type type = this.GetType();
            object[] atts = type.GetCustomAttributes(typeof(PrimaryServerRequiredAttribute), true);
            if (atts.Length > 0)
            {
                ManagementService.VerifyPrimaryServer();
            }
            object[] att2 = type.GetCustomAttributes(typeof(ADFS2019RequiredAttribute), true);
            if (att2.Length > 0)
            {
                ManagementService.VerifyADFSServer2019();
            }
            object[] att3 = type.GetCustomAttributes(typeof(AdministratorsRightsRequiredAttribute), true);
            if (att3.Length > 0)
            {
                ManagementService.VerifyADFSAdministrationRights();
            }
        }

    }
    #endregion

    #region Get-MFAUsers
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
    ///   <para>Get-MFAUsers -FilterValue neos -FilterOperator StartWith -IncludeDisabled -ShowCount</para>
    ///   <para>Get-MFAUsers -Value neos -Operator StartWith -All -ShowCount -SortOrder UserName</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "MFAUsers", SupportsShouldProcess = true, SupportsPaging = false, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
    [OutputType(typeof(PSRegistration[]))]
    public sealed class GetMFAUser : MFACmdlet, IDynamicParameters
    {
        string _identity = string.Empty;
        private PSRegistration[] _list = null;
        private DataFilterObject _filter = new DataFilterObject();
        private DataPagingObject _paging = new DataPagingObject();
        private DataOrderObject _order = new DataOrderObject();

        /// <summary>
        /// <para type="description">identity of the user to selected (upn).</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 0, ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Identity
        {
            get { return _identity; }
            set { _identity = value; }
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
        [ValidateSet("Choose", "Code", "Email", "External", "Azure", "Biometrics")]
        public PSPreferredMethod FilterMethod
        {
            get { return ((PSPreferredMethod)_filter.FilterMethod); }
            set { _filter.FilterMethod = ((PreferredMethod)value); }
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
        /// <para type="description">When using sorting this is sort direction (asc, desc).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public SortDirection SortDirection
        {
            get { return _order.Direction; }
            set { _order.Direction = value; }
        }

        /// <summary>
        /// <para type="description">Filters and Orders Properties.</para>
        /// <para type="description">When using sorting this is the Filed user for ordering results.</para>
        /// <para type="description">When using filtering this is the operator property for filter. alias (Operator, OP).</para>
        /// </summary>
        public object GetDynamicParameters()
        {
            try
            {
                ManagementService.Initialize(this.Host, true);
                switch (ManagementService.Config.StoreMode)
                {
                    case DataRepositoryKind.SQL:
                        if (ManagementService.Config.Hosts.SQLServerHost.IsAlwaysEncrypted)
                        {
                            if (_filter.FilterField == DataFilterField.UserName)
                                return new PSDataFieldMixedParameters(_filter, _order, this.Host);
                            else
                                return new PSDataFieldCryptedParameters(_filter, _order, this.Host);
                        }
                        else
                            return new PSDataFieldParameters(_filter, _order, this.Host);
                    case DataRepositoryKind.Custom:
                        return new PSDataFieldParameters(_filter, _order, this.Host);
                    default:
                        return new PSDataFieldParameters(_filter, _order, this.Host);
                }
            }
            catch
            {
                return null;
            }
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

                    ManagementService.Initialize(this.Host, true);
                    PSRegistrationList mmc = (PSRegistrationList)ManagementService.GetUserRegistrations(_filter, _order, _paging);
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
                    ManagementService.Initialize(this.Host);
                    PSRegistration ret = (PSRegistration)ManagementService.GetUserRegistration(Identity);

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
    /// <para type="description">Standard Dynamic Parameter.</para>
    /// </summary>
    public class PSDataFieldParameters
    {
        private DataFilterObject FilterObject;
        private DataOrderObject OrderObject;
        private readonly PSHost Host;

        /// <summary>
        /// PSDataFieldParameters constructor
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="order"></param>
        /// <param name="host"></param>
        public PSDataFieldParameters(DataFilterObject filter, DataOrderObject order, PSHost host)
        {
            this.FilterObject = filter;
            this.OrderObject = order;
            this.Host = host;
        }

        /// <summary>
        /// <para type="description">When using filtering this is the property for filter. alias (Field, ATTR).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        [Alias("Field", "ATTR")]
        public PSDataFilterField FilterField
        {
            get
            {
                return ((PSDataFilterField)FilterObject.FilterField);
            }
            set
            {
                FilterObject.FilterField = (DataFilterField)value;
            }
        }

        /// <summary>
        /// <para type="description">SortOrder property.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public PSDataOrderField SortOrder
        {
            get
            {
                return ((PSDataOrderField)OrderObject.Column);
            }
            set
            {
                OrderObject.Column = (DataOrderField)value;
            }
        }

        /// <summary>
        /// <para type="description">FilterOperator property.</para>
        /// Data property
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        [Alias("Operator", "OP")]
        public PSDataFilterOperator FilterOperator
        {
            get
            {
                return ((PSDataFilterOperator)FilterObject.FilterOperator);
            }
            set
            {
                FilterObject.FilterOperator = (DataFilterOperator)value;
            }
        }
    }

    /// <summary>
    /// <para type="description">Mixed Dynamic Parameter.</para>
    /// </summary>
    public class PSDataFieldMixedParameters
    {
        private DataFilterObject FilterObject;
        private DataOrderObject OrderObject;
        private readonly PSHost Host;

        /// <summary>
        /// PSDataFieldMixedParameters constructor
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="order"></param>
        /// <param name="host"></param>
        public PSDataFieldMixedParameters(DataFilterObject filter, DataOrderObject order, PSHost host)
        {
            this.FilterObject = filter;
            this.OrderObject = order;
            this.Host = host;
        }

        /// <summary>
        /// <para type="description">When using filtering this is the property for filter. alias (Field, ATTR).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        [Alias("Field", "ATTR")]
        public PSDataFilterField FilterField
        {
            get
            {
                return ((PSDataFilterField)FilterObject.FilterField);
            }
            set
            {
                if (value != PSDataFilterField.UserName)
                {
                    if ((FilterObject.FilterOperator != DataFilterOperator.Equal) && (FilterObject.FilterOperator != DataFilterOperator.NotEqual))
                    {
                        FilterObject.FilterOperator = DataFilterOperator.Equal;
                        this.Host.UI.WriteLine(ConsoleColor.Yellow, this.Host.UI.RawUI.BackgroundColor, "Filter Operator reset to Equal, When using Encrypted Columns (SQL2016) Only Equal and NotEqual operators are supported. This apply to Email and Phone");
                    }
                    FilterObject.FilterField = (DataFilterField)value;
                }
                else
                    FilterObject.FilterField = (DataFilterField)value;
            }
        }

        /// <summary>
        /// <para type="description">SortOrder property.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public PSDataOrderCryptedField SortOrder
        {
            get
            {
                return ((PSDataOrderCryptedField)OrderObject.Column);
            }
            set
            {
                OrderObject.Column = (DataOrderField)value;
            }
        }

        /// <summary>
        /// <para type="description">FilterOperator Property</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        [Alias("Operator", "OP")]
        public PSDataFilterOperator FilterOperator
        {
            get
            {
                return ((PSDataFilterOperator)FilterObject.FilterOperator);
            }
            set
            {
                if (FilterObject.FilterField != DataFilterField.UserName)
                {
                    if ((value != PSDataFilterOperator.Equal) && (value != PSDataFilterOperator.NotEqual))
                    {
                        FilterObject.FilterOperator = DataFilterOperator.Equal;
                        this.Host.UI.WriteLine(ConsoleColor.Yellow, this.Host.UI.RawUI.BackgroundColor, "Filter Operator reset to Equal, When using Encrypted Columns (SQL2016) Only Equal and NotEqual operators are supported. This apply to Email and Phone");
                    }
                    else
                        FilterObject.FilterOperator = (DataFilterOperator)value;
                }
                else
                    FilterObject.FilterOperator = (DataFilterOperator)value;
            }
        }
    }

    /// <summary>
    /// <para type="description">Crypted Dynamic Parameter.</para>
    /// </summary>
    public class PSDataFieldCryptedParameters
    {
        private DataFilterObject FilterObject;
        private DataOrderObject OrderObject;
        private readonly PSHost Host;

        /// <summary>
        /// PSDataFieldCryptedParameters constructor
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="order"></param>
        /// <param name="host"></param>
        public PSDataFieldCryptedParameters(DataFilterObject filter, DataOrderObject order, PSHost host)
        {
            this.FilterObject = filter;
            this.OrderObject = order;
            this.Host = host;
        }

        /// <summary>
        /// <para type="description">When using filtering this is the property for filter. alias (Field, ATTR).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        [Alias("Field", "ATTR")]
        public PSDataFilterCryptedField FilterField
        {
            get
            {
                return ((PSDataFilterCryptedField)FilterObject.FilterField);
            }
            set
            {
                FilterObject.FilterField = (DataFilterField)value;
            }
        }


        /// <summary>
        /// <para type="description">SortOrder property.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public PSDataOrderCryptedField SortOrder
        {
            get
            {
                return ((PSDataOrderCryptedField)OrderObject.Column);
            }
            set
            {
                OrderObject.Column = (DataOrderField)value;
            }
        }


        /// <summary>
        /// <para type="description">FilterOperator Property</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        [Alias("Operator", "OP")]
        public PSDataFilterCryptedOperator FilterOperator
        {
            get
            {
                return ((PSDataFilterCryptedOperator)FilterObject.FilterOperator);
            }
            set
            {
                FilterObject.FilterOperator = (DataFilterOperator)value;
            }
        }

    }
    #endregion

    #region Set-MFAUsers
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
    ///   <para>Set-MFAUsers -Identity user@domain.com -Email user@mailbox.com -Phone 0606050403 -PIN 2451 -Method Code -ResetKey</para> 
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
        private PSPreferredMethod _method = PSPreferredMethod.Choose;
        private bool _enabled = true;
        private bool _emailfornewkey = false;
        private int _pincode = -1;
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
        [ValidateSet("Choose", "Code", "Email", "External", "Azure", "Biometrics")]
        public PSPreferredMethod Method
        {
            get { return _method; }
            set { _method = value; }
        }

        /// <summary>
        /// <para type="description">Pin code of selected users.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public int Pin
        {
            get { return _pincode; }
            set { _pincode = value; }
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
        /// <para type="description">EmailForNewKey allow email when updating Key for MFA.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter EmailForNewKey
        {
            get { return _emailfornewkey; }
            set { _emailfornewkey = value; }
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
                    ManagementService.Initialize(this.Host, true);
                    PSRegistration res = (PSRegistration)ManagementService.GetUserRegistration(Identity);
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
            ProgressRecord prog = null;
            if (_data.Length >= 10)
            {
                prog = new ProgressRecord(this.GetHashCode(), "Set-MFAUsers", "Operation running")
                {
                    PercentComplete = 0,
                    RecordType = ProgressRecordType.Processing
                };
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
                        if (this.Method != PSPreferredMethod.Choose)
                            reg.PreferredMethod = this.Method;
                        if (this.Pin > -1)
                            reg.PIN = this.Pin;
                        reg.Enabled = this.Enabled;

                        if (string.IsNullOrEmpty(reg.MailAddress) && ManagementService.Config.MailProvider.Enabled && ManagementService.Config.MailProvider.IsRequired)
                            this.Host.UI.WriteWarningLine(string.Format(errors_strings.ErrorEmailNotProvided, reg.UPN));
                        if (string.IsNullOrEmpty(reg.PhoneNumber) && ManagementService.Config.ExternalProvider.Enabled && ManagementService.Config.ExternalProvider.IsRequired)
                            this.Host.UI.WriteWarningLine(string.Format(errors_strings.ErrorPhoneNotProvided, reg.UPN));

                        ManagementService.Initialize(this.Host, true);
                        PSRegistration ret = (PSRegistration)ManagementService.SetUserRegistration((MFAUser)reg, this.ResetKey, false, this.EmailForNewKey);

                        if (ret != null)
                        {
                            if (this.ResetKey)
                                this.WriteVerbose(string.Format(infos_strings.InfosUserHasNewKey, ret.UPN));
                            this.WriteObject(ret);
                            this.WriteVerbose(string.Format(infos_strings.InfosUserUpdated, ret.UPN));
                        }
                        else
                            throw new Exception(string.Format(errors_strings.ErrorUserNotFound, reg.UPN));
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
    #endregion

    #region Add-MFAUsers
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
    ///   <para>Add-MFAUsers -Identity user@domain.com -Email user@mailbox.com -Phone 0606050403 -PIN 2451 -Method Code -NoNewKey</para> 
    ///   <para>Add-MFAUsers $users -Method Code -NNewKey</para> 
    /// </example>
    [Cmdlet(VerbsCommon.Add, "MFAUsers", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
    public sealed class AddMFAUser : MFACmdlet
    {
        private string _identity = string.Empty;
        private string _mailaddress = string.Empty;
        private string _phonenumber = string.Empty;
        private PSPreferredMethod _method = PSPreferredMethod.Choose;
        private int _pincode = -1;
        private bool _enabled = true;
        private bool _emailfornewkey = false;
        private bool _nonewkey = false;

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
        [Parameter(ParameterSetName = "Data")]
        [ValidateSet("Choose", "Code", "Email", "External", "Azure", "Biometrics")]
        public PSPreferredMethod Method
        {
            get { return _method; }
            set { _method = value; }
        }

        /// <summary>
        /// <para type="description">Pin code of selected users.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public int Pin
        {
            get { return _pincode; }
            set { _pincode = value; }
        }

        /// <summary>
        /// <para type="description">Enabled status for the new users.</para>
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
        public SwitchParameter NoNewKey
        {
            get { return _nonewkey; }
            set { _nonewkey = value; }
        }

        /// <summary>
        /// <para type="description">EmailForNewKey allow Key email when create a new Key for MFA.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter EmailForNewKey
        {
            get { return _emailfornewkey; }
            set { _emailfornewkey = value; }
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
                    ManagementService.Initialize(this.Host, true);
                    PSRegistration res = (PSRegistration)ManagementService.GetUserRegistration(Identity);

                    if (res != null)
                        throw new Exception(string.Format(errors_strings.ErrorUserExists, this.Identity));
                    res = new PSRegistration
                    {
                        UPN = Identity
                    };
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
            ProgressRecord prog = null;
            if (_data.Length >= 10)
            {
                prog = new ProgressRecord(this.GetHashCode(), "Add-MFAUsers", "Operation running")
                {
                    PercentComplete = 0,
                    RecordType = ProgressRecordType.Processing
                };
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
                        if (this.Method != PSPreferredMethod.Choose)
                            reg.PreferredMethod = this.Method;
                        if (this.Pin > -1)
                            reg.PIN = this.Pin;
                        reg.Enabled = this.Enabled;

                        if (string.IsNullOrEmpty(reg.MailAddress) && ManagementService.Config.MailProvider.Enabled && ManagementService.Config.MailProvider.IsRequired)
                            this.Host.UI.WriteWarningLine(string.Format(errors_strings.ErrorEmailNotProvided, reg.UPN));
                        if (string.IsNullOrEmpty(reg.PhoneNumber) && ManagementService.Config.ExternalProvider.Enabled && ManagementService.Config.ExternalProvider.IsRequired)
                            this.Host.UI.WriteWarningLine(string.Format(errors_strings.ErrorPhoneNotProvided, reg.UPN));

                        ManagementService.Initialize(this.Host, true);
                        PSRegistration ret = (PSRegistration)ManagementService.AddUserRegistration((MFAUser)reg, !this.NoNewKey, false, this.EmailForNewKey);

                        if (ret != null)
                        {
                            this.WriteVerbose(string.Format(infos_strings.InfosUserHasNewKey, reg.UPN));
                            this.WriteObject(ret);
                            this.WriteVerbose(string.Format(infos_strings.InfosUserAdded, reg.UPN));
                        }
                        else
                            throw new Exception(string.Format(errors_strings.ErrorUserNotFound, reg.UPN));
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
    #endregion

    #region Remove-MFAUsers
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
                    ManagementService.Initialize(this.Host, true);
                    PSRegistration res = (PSRegistration)ManagementService.GetUserRegistration(Identity);

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
            ProgressRecord prog = null;
            if (_data.Length >= 10)
            {
                prog = new ProgressRecord(this.GetHashCode(), "Remove-MFAUsers", "Operation running")
                {
                    PercentComplete = 0,
                    RecordType = ProgressRecordType.Processing
                };
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

                        ManagementService.DeleteUserRegistration((MFAUser)reg, true);

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
    #endregion

    #region Enable-MFAUsers
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
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Identity
        {
            get { return _identity; }
            set { _identity = value; }
        }

        /// <summary>
        /// <para type="description">Collection of users to be enabled for MFA (must be registered before with Add-MFAUsers.</para>
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
                    ManagementService.Initialize(this.Host, true);
                    PSRegistration res = (PSRegistration)ManagementService.GetUserRegistration(Identity);

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
            ProgressRecord prog = null;
            if (_data.Length >= 10)
            {
                prog = new ProgressRecord(this.GetHashCode(), "Enable-MFAUsers", "Operation running")
                {
                    PercentComplete = 0,
                    RecordType = ProgressRecordType.Processing
                };
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

                        PSRegistration res = (PSRegistration)ManagementService.EnableUserRegistration((MFAUser)reg);

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
    #endregion

    #region Disable-MFAUsers
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
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Identity")]
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
                    ManagementService.Initialize(this.Host, true);
                    PSRegistration res = (PSRegistration)ManagementService.GetUserRegistration(Identity);
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
            int i = 0;
            ProgressRecord prog = null;
            if (_data.Length >= 10)
            {
                prog = new ProgressRecord(this.GetHashCode(), "Disable-MFAUsers", "Operation running")
                {
                    PercentComplete = 0,
                    RecordType = ProgressRecordType.Processing
                };
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

                        PSRegistration res = (PSRegistration)ManagementService.DisableUserRegistration((MFAUser)reg);
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
    #endregion

    #region Get-MFAFarmInformation
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
    [PrimaryServerRequired]
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
                ManagementService.Initialize(this.Host, true);
                _farm = ManagementService.ADFSManager.ADFSFarm;
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
    #endregion

    #region Register-MFAComputer
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
    ///   <para>Register-MFAComputer -ServerName otheradfs.domain.com</para>
    /// </example>
    [Cmdlet(VerbsLifecycle.Register, "MFAComputer", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired, AdministratorsRightsRequired]
    public sealed class RegisterMFAComputer : MFACmdlet
    {
        private string _servername;

        /// <summary>
        /// <para type="description">Server Name to add in MFA farm.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data")]
        public string ServerName
        {
            get { return _servername; }
            set { _servername = value; }
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, true);
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
                if (ManagementService.RegisterADFSComputer(hh, ServerName))
                    this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfoComputerRegistered, ServerName));
                else
                    this.Host.UI.WriteLine(ConsoleColor.Yellow, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfoComputerRegistered, ServerName));
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3001", ErrorCategory.OperationStopped, this));
            }
        }
    }
    #endregion

    #region UnRegister-MFAComputer
    /// <summary>
    /// <para type="synopsis">UnRegister MFA Computer.</para>
    /// <para type="description">UnRegiter an ADFS server from your farm.</para>
    /// <para type="description">If you want to remove a server from your farm, you must Unregister it.</para>
    /// <para type="description">Server list is usefull to restart services, and participate to notification system for updating configuration automatically without restarting ADFS service.</para>
    /// <para type="description">This Cmdlet does not support remoting.</para>
    /// </summary>
    /// <example>
    ///   <para>UnRegister-MFAComputer</para>
    /// </example>
    [Cmdlet(VerbsLifecycle.Unregister, "MFAComputer", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired, AdministratorsRightsRequired]
    public sealed class UnRegisterMFAComputer : MFACmdlet
    {
        private string _servername;

        /// <summary>
        /// <para type="description">Server Name to add in MFA farm.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data")]
        public string ServerName
        {
            get { return _servername; }
            set { _servername = value; }
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, true);
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
                if (ManagementService.UnRegisterADFSComputer(hh, ServerName))
                    this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfoComputerUnRegistered, ServerName));
                else
                    this.Host.UI.WriteLine(ConsoleColor.Yellow, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfoComputerNotUnRegistered, ServerName));
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3003", ErrorCategory.OperationStopped, this));
            }
        }
    }
    #endregion

    #region Get-MFAComputers
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
    [PrimaryServerRequired]
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
                    ManagementService.Initialize(this.Host, true);
                    ADFSFarmHost lst = ManagementService.ADFSManager.ADFSFarm;
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
                    ManagementService.Initialize(this.Host, true);
                    ADFSFarmHost lst = ManagementService.ADFSManager.ADFSFarm;
                    ADFSServerHost computer = null;
                    foreach (ADFSServerHost itm in lst.Servers)
                    {
                        if (itm.FQDN.ToLower().Equals(_identity.ToLower()))
                        {
                            computer = itm;
                            break;
                        }
                    }
                    if (computer == null)
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
    #endregion

    #region Restart-MFAComputerServices
    /// <summary>
    /// <para type="synopsis">Restart ADFS services.</para>
    /// <para type="description">Restart ADFS Services for one ADFS Computer.</para>
    /// </summary>
    /// <example>
    ///   <para>Restart-MFAComputerServices</para>
    ///   <para>Restart-MFAComputerServices myadfsserver.domain.com</para>
    /// </example>
    [Cmdlet(VerbsLifecycle.Restart, "MFAComputerServices", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
    [AdministratorsRightsRequired]  // ICI
    public sealed class RestartMFAComputerServices : MFACmdlet
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
                ManagementService.Initialize(this.Host, true);
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
                if (ShouldProcess("Restart ADFS Service"))
                {
                    if (string.IsNullOrEmpty(Identity))
                        Identity = "local";
                    PSHost hh = GetHostForVerbose();
                    if (!ManagementService.RestartADFSService(hh, this.Identity))
                        this.Host.UI.WriteWarningLine(errors_strings.ErrorInvalidServerName);
                    else
                        this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, string.Format(infos_strings.InfosADFSServicesRestarted, this.Identity));
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
    #endregion    

    #region Register-MFASystem
    /// <summary>
    /// <para type="synopsis">Register Configuration for MFA.</para>
    /// <para type="description">Register MFA components with ADFS. Activate it</para>
    /// </summary>
    /// <example>
    ///   <para>Register-MFASystem</para>
    ///   <para>Create a new empty configuration for MFA. You must complete configuration with other CmdLets</para>
    /// </example>
    [Cmdlet(VerbsLifecycle.Register, "MFASystem", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired, AdministratorsRightsRequired]
    public sealed class RegisterMFASystem : MFACmdlet
    {
        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, false);
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
                if (ShouldProcess("Register-MFASystem", "Register MFA System with ADFS"))
                {
                    PSHost hh = GetHostForVerbose();
                    ADFSServiceManager svc = ManagementService.ADFSManager;
                    if (svc.RegisterMFASystem(hh))
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
    #endregion

    #region Import-MFASystemConfiguration
    /// <summary>
    /// <para type="synopsis">Update MFA Configuration with a configuration file.</para>
    /// <para type="description">Update Configuration of MFA components. Activate it</para>
    /// </summary>
    /// <example>
    ///   <para>Import-MFASystemConfiguration -ImportFilePath c:\temp\mysavedconfig.xml</para>
    ///   <para>Import a new configuration for MFA with a file. You must complete configuration with other CmdLets</para>
    /// </example>
    [Cmdlet(VerbsData.Import, "MFASystemConfiguration", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired, AdministratorsRightsRequired]
    public sealed class ImportMFASystemConfiguration : MFACmdlet
    {
        /// <summary>
        /// <para type="description">Set the name of the backup file.</para>
        /// BackupFileName property
        /// </summary>
        [Parameter(ParameterSetName = "Data", Mandatory = true)]
        public string ImportFilePath { get; set; }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, false);
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
                if (ShouldProcess("Import-MFASystemConfiguration", "Import and ovveride the current MFA configuration with : " + ImportFilePath + " ? "))
                {
                    PSHost hh = GetHostForVerbose();
                    ADFSServiceManager svc = ManagementService.ADFSManager;
                    if (svc.ImportMFAProviderConfiguration(hh, ImportFilePath))
                        this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosSystemImported, ImportFilePath));
                    else
                        this.Host.UI.WriteLine(ConsoleColor.Yellow, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosSystemInInvalidState));
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3012", ErrorCategory.OperationStopped, this));
            }
        }
    }
    #endregion

    #region Export-MFASystemConfiguration
    /// <summary>
    /// <para type="synopsis">Save current MFA Configuration to a file.</para>
    /// <para type="description">Save current Configuration of MFA components</para>
    /// </summary>
    /// <example>
    ///   <para>Export-MFASystemConfiguration -ExportFilePath c:\temp\mysavedconfig.xml</para>
    ///   <para>Export current MFA configuration to the specified file.</para>
    /// </example>
    [Cmdlet(VerbsData.Export, "MFASystemConfiguration", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired, AdministratorsRightsRequired]
    public sealed class ExportMFASystemConfiguration : MFACmdlet
    {
        /// <summary>
        /// <para type="description">Set the name of the export file.</para>
        /// ExportFilePath property
        /// </summary>
        [Parameter(ParameterSetName = "Data", Mandatory = true)]
        public string ExportFilePath
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
                ManagementService.Initialize(this.Host, false);
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
                if (ShouldProcess("Export-MFASystemConfiguration", "Export current MFA configuration to file : " + ExportFilePath + " ?"))
                {
                    PSHost hh = GetHostForVerbose();
                    ADFSServiceManager svc = ManagementService.ADFSManager;
                    svc.ExportMFAProviderConfiguration(hh, ExportFilePath);
                    this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosSystemExported, ExportFilePath));
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3012", ErrorCategory.OperationStopped, this));
            }
        }
    }
    #endregion

    #region UnRegister-MFASystem
    /// <summary>
    /// <para type="synopsis">UnRegister Configuration for MFA.</para>
    /// <para type="description">UnRegister MFA components from ADFS. DeActivate it</para>
    /// </summary>
    /// <example>
    ///   <para>UnRegister-MFASystem</para>
    ///   <para>Remove MFA configuration from ADFS. </para>
    /// </example>
    [Cmdlet(VerbsLifecycle.Unregister, "MFASystem", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired, AdministratorsRightsRequired]
    public sealed class UnRegisterMFASystem : MFACmdlet
    {
        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, true);
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
                    ADFSServiceManager svc = ManagementService.ADFSManager;
                    if (svc.UnRegisterMFASystem(hh))
                        this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosSystemUnRegistered));
                    else
                        this.Host.UI.WriteLine(ConsoleColor.Yellow, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosSystemUnRegistered));
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3014", ErrorCategory.OperationStopped, this));
            }
        }
    }
    #endregion

    #region Enable-MFASystem
    /// <summary>
    /// <para type="synopsis">Enable MFA.</para>
    /// <para type="description">Enable MFA System (if initialized)</para>
    /// </summary>
    /// <example>
    ///   <para>Enable-MFASystem</para>
    ///   <para>Enable MFA configuration from ADFS. </para>
    /// </example>
    [Cmdlet(VerbsLifecycle.Enable, "MFASystem", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired, AdministratorsRightsRequired]
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
                ManagementService.Initialize(this.Host, true);
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
                    ADFSServiceManager svc = ManagementService.ADFSManager;
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
    #endregion

    #region Disable-MFASystem
    /// <summary>
    /// <para type="synopsis">Disable MFA.</para>
    /// <para type="description">Disable MFA System (if initialized)</para>
    /// </summary>
    /// <example>
    ///   <para>Disable-MFASystem</para>
    ///   <para>Disable MFA configuration from ADFS. </para>
    /// </example>
    [Cmdlet(VerbsLifecycle.Disable, "MFASystem", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired, AdministratorsRightsRequired]
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
                ManagementService.Initialize(this.Host, true);
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
                    ADFSServiceManager svc = ManagementService.ADFSManager;
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
    #endregion

    #region Get-MFAConfig
    /// <summary>
    /// <para type="synopsis">Get Configuration.</para>
    /// <para type="description">Get main configuration options.</para>
    /// </summary>
    /// <example>
    ///   <para>Get-MFAConfig</para>
    ///   <para>Display MFA general configuration</para>
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
                FlatConfig cf = new FlatConfig();
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
                if (ShouldProcess("MFA General Configuration"))
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
    #endregion

    #region Set-MFAConfig
    /// <summary>
    /// <para type="synopsis">Set General Configuration.</para>
    /// <para type="description">Set main configuration options.</para>
    /// </summary>
    /// <example>
    ///   <para>** Using PowserShell Variables</para>
    ///   <para>$cfg = Get-MFAConfig</para>
    ///   <para>$cfg.DefaultCountryCode = "us"</para>
    ///   <para>Set-MFAConfig $cfg</para>
    /// </example>
    /// <example>
    ///   <para>** Using configuration options with completion.</para>
    ///   <para>Set-MFAConfig -AdminContact username@domainname -UserFeatures AdministrativeMode, AllowEnrollment, AllowProvideInformations</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "MFAConfig", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired]
    public sealed class SetMFAConfig : MFACmdlet
    {
        private PSConfig _config;
        private FlatConfig _target;
        private string _admincontact;
        private string _issuer;
        private string _defaultcountrycode;
        private PSPreferredMethod _defaultprovidermethod;
        private PSUserFeaturesOptions _userfeatures;
        private bool _customupdatepassword;
        private bool _keepmyselectedoptionon;
        private bool _changenotificationson;
        private bool _useofuserlanguages;
        private PSAdvertisingDays _advertisingdays;
        private bool _admincontactchanged = false;
        private bool _issuerchanged = false;
        private bool _defaultcountrycodechanged = false;
        private bool _defaultprovidermethodchanged = false;
        private bool _userfeatureschanged = false;
        private bool _customupdatepasswordchanged = false;
        private bool _keepmyselectedoptiononchanged = false;
        private bool _changenotificationsonchanged = false;
        private bool _useofuserlanguageschanged = false;
        private bool _advertisingdayschanged = false;

        /// <summary>
        /// <para type="description">Config parameter, a variable of type PSConfig.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data", ValueFromPipeline = true, DontShow = true)]
        [ValidateNotNullOrEmpty()]
        public PSConfig Config
        {
            get { return _config; }
            set { _config = value; }
        }

        /// <summary>
        /// <para type="description">Administrators email, used in administrative emails sent to users.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string AdminContact
        {
            get { return _admincontact; }
            set
            {
                _admincontact = value;
                _admincontactchanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Issuer description (eg "my company").</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Issuer
        {
            get { return _issuer; }
            set
            {
                _issuer = value;
                _issuerchanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Default contry code, used for SMS calls .</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string DefaultCountryCode
        {
            get { return _defaultcountrycode; }
            set
            {
                _defaultcountrycode = value;
                _defaultcountrycodechanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Default provider when User method equals "Choose".</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateSet("Choose", "Code", "Email", "External", "Azure", "Biometrics")]
        public PSPreferredMethod DefaultProviderMethod
        {
            get { return _defaultprovidermethod; }
            set
            {
                _defaultprovidermethod = value;
                _defaultprovidermethodchanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Policy attributes for users management and registration.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public PSUserFeaturesOptions UserFeatures
        {
            get { return _userfeatures; }
            set
            {
                _userfeatures = value;
                _userfeatureschanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Use or not our implementation for changing user password,if not we are using /ADFS/Portal/updatepasswor.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public bool CustomUpdatePassword
        {
            get { return _customupdatepassword; }
            set
            {
                _customupdatepassword = value;
                _customupdatepasswordchanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Let user to change the default MFA provider for later use.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public bool KeepMySelectedOptionOn
        {
            get { return _keepmyselectedoptionon; }
            set
            {
                _keepmyselectedoptionon = value;
                _keepmyselectedoptiononchanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Send email notifications when a user update his configuration.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public bool ChangeNotificationsOn
        {
            get { return _changenotificationson; }
            set
            {
                _changenotificationson = value;
                _changenotificationsonchanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Use of User's browser laguages instead or standard localization features.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public bool UseOfUserLanguages
        {
            get { return _useofuserlanguages; }
            set
            {
                _useofuserlanguages = value;
                _useofuserlanguageschanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Policy attributes for warnings to users.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public PSAdvertisingDays AdvertisingDays
        {
            get { return _advertisingdays; }
            set
            {
                _advertisingdays = value;
                _advertisingdayschanged = true;
            }
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
                    _target = (FlatConfig)_config;
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "3023", ErrorCategory.OperationStopped, this));
                }
            }
            else
            {
                try
                {
                    ManagementService.Initialize(this.Host, true);
                    _target = new FlatConfig();
                    _target.Load(this.Host);
                    if (!string.IsNullOrEmpty(AdminContact) && _admincontactchanged)
                        _target.AdminContact = this.AdminContact;
                    if (!string.IsNullOrEmpty(Issuer) && _issuerchanged)
                        _target.Issuer = this.Issuer;
                    if (!string.IsNullOrEmpty(DefaultCountryCode) && _defaultcountrycodechanged)
                        _target.DefaultCountryCode = this.DefaultCountryCode;
                    if ((_target.DefaultProviderMethod != (PreferredMethod)this.DefaultProviderMethod) && _defaultprovidermethodchanged)
                        _target.DefaultProviderMethod = (PreferredMethod)this.DefaultProviderMethod;
                    if ((_target.UserFeatures != (UserFeaturesOptions)this.UserFeatures) && _userfeatureschanged)
                        _target.UserFeatures = (UserFeaturesOptions)this.UserFeatures;
                    if ((_target.CustomUpdatePassword != this.CustomUpdatePassword) && _customupdatepasswordchanged)
                        _target.CustomUpdatePassword = this.CustomUpdatePassword;
                    if ((_target.KeepMySelectedOptionOn != this.KeepMySelectedOptionOn) && _keepmyselectedoptiononchanged)
                        _target.KeepMySelectedOptionOn = this.KeepMySelectedOptionOn;
                    if ((_target.ChangeNotificationsOn != this.ChangeNotificationsOn) && _changenotificationsonchanged)
                        _target.ChangeNotificationsOn = this.ChangeNotificationsOn;
                    if ((_target.UseOfUserLanguages != this.UseOfUserLanguages) && _useofuserlanguageschanged)
                        _target.UseOfUserLanguages = this.UseOfUserLanguages;
                    if (_advertisingdayschanged && (_target.AdvertisingDays != (FlatAdvertising)this.AdvertisingDays))
                        _target.AdvertisingDays = (FlatAdvertising)this.AdvertisingDays;
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
            if (ShouldProcess("Set MFA General Configuration"))
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
    #endregion

    #region Get-MFAStore
    /// <summary>
    /// <para type="synopsis">Get MFA Store Configuration.</para>
    /// <para type="description">Get MFA Store configuration options.</para>
    /// <para type="description"></para>
    /// </summary>
    /// <example>
    ///   <para>Get-MFAStore -Store ADDS</para>
    ///   <para>Get-MFAStore -Store SQL</para>
    /// </example>
    /// <example>
    ///   <para>$c = Get-MFAStore -Store ADDS</para>
    ///   <para>$c = Get-MFAStore -Store SQL</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "MFAStore", SupportsShouldProcess = true, SupportsPaging = false, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [OutputType(typeof(PSADDSStore), typeof(PSSQLStore))]
    public sealed class GetMFAStore : MFACmdlet
    {
        private PSADDSStore _config0;
        private PSSQLStore _config1;
        private PSCustomStore _config2;

        /// <summary>
        /// <para type="description">Store Configuration mode, (ADDS, SQL) Required.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data", ValueFromPipeline = true)]
       // [ValidateSet("ADDS", "SQL", "Custom")]
        [ValidateRange(PSStoreMode.ADDS, PSStoreMode.Custom)]
        public PSStoreMode Store { get; set; } = PSStoreMode.ADDS;

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                switch (Store)
                {
                    case PSStoreMode.ADDS:
                        FlatADDSStore cf0 = new FlatADDSStore();
                        cf0.Load(this.Host);
                        _config0 = (PSADDSStore)cf0;
                        break;
                    case PSStoreMode.SQL:
                        FlatSQLStore cf1 = new FlatSQLStore();
                        cf1.Load(this.Host);
                        _config1 = (PSSQLStore)cf1;
                        break;
                    case PSStoreMode.Custom:
                        FlatCustomStore cf2 = new FlatCustomStore();
                        cf2.Load(this.Host);
                        _config2 = (PSCustomStore)cf2;
                        break;
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3021", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessInternalRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (ShouldProcess("MFA Store Configuration"))
                {
                    switch (Store)
                    {
                        case PSStoreMode.ADDS:
                            WriteObject(_config0);
                            break;
                        case PSStoreMode.SQL:
                            WriteObject(_config1);
                            break;
                        case PSStoreMode.Custom:
                            WriteObject(_config2);
                            break;
                    }
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
            _config0 = null;
            _config1 = null;
            base.StopProcessing();
        }

        /// <summary>
        /// EndProcessing method implementation
        /// </summary>
        protected override void EndProcessing()
        {
            _config0 = null;
            _config1 = null;
            base.EndProcessing();
        }
    }
    #endregion

    #region Set-MFAStore
    /// <summary>
    /// <para type="synopsis">Set MFA Store Configuration.</para>
    /// <para type="description">Set MFA Store configuration options.</para>
    /// </summary>
    /// <example>
    ///   <para>** Using PowserShell Variables</para>
    ///   <para>$cfg = Get-MFAStore -Store ADDS</para>
    ///   <para>$cfg.Active = $false</para>
    ///   <para>Set-MFAStore -Store ADDS $cfg</para>
    /// </example>
    /// <example>
    ///   <para>** Using auto completion</para>
    ///   <para>Set-MFAStore -Store ADDS -Active $true -MaxRows 10000 -UserFeatures</para>
    /// </example>
    /// <example>
    ///   <para>** Using PowserShell Variables</para>
    ///   <para>$cfg = Get-MFAStore -Store SQL</para>
    ///   <para>$cfg.MaxRows = 5000</para>
    ///   <para>Set-MFAStore -Store SQL $cfg</para>
    /// </example>
    /// <example>
    ///   <para>** Using auto completion</para>
    ///   <para>Set-MFAStore -Store SQL -Active $false -MaxRows 10000 -ConnectionString = "your connect string"</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "MFAStore", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired]
    public sealed class SetMFAStore : MFACmdlet, IDynamicParameters
    {
        private ADDSStoreDynamicParameters _config0;
        private SQLStoreDynamicParameters _config1;
        private CustomStoreDynamicParameters _config2;

        private PSStoreMode _store = PSStoreMode.ADDS;
        private PSBaseStore _config;
        private bool _configchanged = false;
        private FlatADDSStore _target0;
        private FlatSQLStore _target1;
        private FlatCustomStore _target2;


        /// <summary>
        /// <para type="description">Store Configuration mode, (ADDS, SQL) Required.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Identity")]
        [ValidateRange(PSStoreMode.ADDS, PSStoreMode.Custom)]
        public PSStoreMode Store
        {
            get { return _store; }
            set { _store = value; }
        }

        /// <summary>
        /// <para type="description">Config parameter, a variable of type PSConfigSQL.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Data", ValueFromPipeline = true, DontShow = true)]
        [ValidateNotNullOrEmpty()]
        public PSBaseStore Config
        {
            get { return _config; }
            set
            {
                _config = value;
                _configchanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Set the value of Provider configuration.</para>
        /// GetDynamicParameters implementation
        /// </summary>
        public object GetDynamicParameters()
        {
            if (!_configchanged)
            {
                switch (Store)
                {
                    case PSStoreMode.ADDS:
                        _config0 = new ADDSStoreDynamicParameters();
                        return _config0;
                    case PSStoreMode.SQL:
                        _config1 = new SQLStoreDynamicParameters();
                        return _config1;
                    case PSStoreMode.Custom:
                        _config2 = new CustomStoreDynamicParameters();
                        return _config2;

                }
            }
            return null;
        }

        /// <summary>
        /// BeginProcessing method override
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                if (!_configchanged)
                {
                    ManagementService.Initialize(this.Host, true);
                    switch (Store)
                    {
                        case PSStoreMode.ADDS:
                            _target0 = new FlatADDSStore();
                            _target0.Load(this.Host);
                            if (_config0.ActiveChanged)
                                _target0.Active = _config0.Active;
                            if (!string.IsNullOrEmpty(_config0.KeyAttribute) && _config0.KeyAttributeChanged)
                                _target0.KeyAttribute = _config0.KeyAttribute;
                            if (!string.IsNullOrEmpty(_config0.MailAttribute) && _config0.MailAttributeChanged)
                                _target0.MailAttribute =_config0.MailAttribute;
                            if (!string.IsNullOrEmpty(_config0.PhoneAttribute) && _config0.PhoneAttributeChanged)
                                _target0.PhoneAttribute = _config0.PhoneAttribute;
                            if (!string.IsNullOrEmpty(_config0.MethodAttribute) && _config0.MethodAttributeChanged)
                                _target0.MethodAttribute = _config0.MethodAttribute;
                            if (!string.IsNullOrEmpty(_config0.OverrideMethodAttribute) && _config0.OverrideMethodAttributeChanged)
                                _target0.OverrideMethodAttribute = _config0.OverrideMethodAttribute;
                            if (!string.IsNullOrEmpty(_config0.EnabledAttribute) && _config0.EnabledAttributeChanged)
                                _target0.EnabledAttribute = _config0.EnabledAttribute;
                            if (!string.IsNullOrEmpty(_config0.PinAttribute) && _config0.PinAttributeChanged)
                                _target0.PinAttribute = _config0.PinAttribute;
                            if (!string.IsNullOrEmpty(_config0.PublicKeyCredentialAttribute) && _config0.PublicKeyCredentialAttributeChanged)
                                _target0.PublicKeyCredentialAttribute = _config0.PublicKeyCredentialAttribute;
                            if (!string.IsNullOrEmpty(_config0.ClientCertificateAttribute) && _config0.ClientCertificateAttributeChanged)
                                _target0.ClientCertificateAttribute = _config0.ClientCertificateAttribute;
                            if (!string.IsNullOrEmpty(_config0.RSACertificateAttribute) && _config0.RSACertificateAttributeChanged)
                                _target0.RSACertificateAttribute = _config0.RSACertificateAttribute;
                            if (_config0.MaxRowsChanged)
                                _target0.MaxRows = _config0.MaxRows;
                            if (_config0.UseSSLChanged)
                                _target0.UseSSL = _config0.UseSSL;
                            break;
                        case PSStoreMode.SQL:
                            _target1 = new FlatSQLStore();
                            _target1.Load(this.Host);
                            if (_config1.ActiveChanged)
                                _target1.Active = _config1.Active;
                            if (_config1.MaxRowsChanged)
                                _target1.MaxRows = _config1.MaxRows;
                            if (!string.IsNullOrEmpty(_config1.ConnectionString) && _config1.ConnectionStringChanged)
                                _target1.ConnectionString = _config1.ConnectionString;
                            if (_config1.IsAlwaysEncryptedChanged)
                                _target1.IsAlwaysEncrypted = _config1.IsAlwaysEncrypted;
                            if (!string.IsNullOrEmpty(_config1.AECCertificateThumbPrint) && _config1.AECCertificateThumbPrintChanged)
                                _target1.ThumbPrint = _config1.AECCertificateThumbPrint;
                            if (_config1.AECCertificateValidityChanged)
                                _target1.CertificateValidity = _config1.AECCertificateValidity;
                            if (_config1.AECCertificateReuseChanged)
                                _target1.CertReuse = _config1.AECCertificateReuse;
                            if (!string.IsNullOrEmpty(_config1.AECCryptoKeyName) && _config1.AECCryptoKeyNameChanged)
                                _target1.KeyName = _config1.AECCryptoKeyName;
                            break;
                        case PSStoreMode.Custom:
                            _target2 = new FlatCustomStore();
                            _target2.Load(this.Host);
                            if (_config2.ActiveChanged)
                                _target2.Active = _config2.Active;
                            if (_config2.MaxRowsChanged)
                                _target2.MaxRows = _config2.MaxRows;
                            if (_config2.ConnectionStringChanged)
                                _target2.ConnectionString = _config2.ConnectionString;
                            if (_config2.DataImplementationChanged)
                                _target2.DataRepositoryFullyQualifiedImplementation = _config2.DataRepositoryFullyQualifiedImplementation;
                            if (_config2.KeysImplementationChanged)
                                _target2.KeysRepositoryFullyQualifiedImplementation = _config2.KeysRepositoryFullyQualifiedImplementation;
                            if (_config2.ParametersChanged)
                                _target2.Parameters = _config2.Parameters;
                            break;
                    }
                }
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
            if (ShouldProcess("MFA Store Configuration"))
            {
                try
                {
                    if (_configchanged)
                    {
                        switch (Store)
                        {
                            case PSStoreMode.ADDS:
                                if (Config is PSADDSStore)
                                    ((FlatADDSStore)((PSADDSStore)Config)).Update(this.Host);
                                else
                                    throw new Exception("Invalid DataStore Type !");
                                break;
                            case PSStoreMode.SQL:
                                if (Config is PSSQLStore)
                                    ((FlatSQLStore)((PSSQLStore)Config)).Update(this.Host);
                                else
                                    throw new Exception("Invalid DataStore Type !");
                                break;
                            case PSStoreMode.Custom:
                                if (Config is PSCustomStore)
                                    ((FlatCustomStore)((PSCustomStore)Config)).Update(this.Host);
                                else
                                    throw new Exception("Invalid DataStore Type !");
                                break;
                        }
                    }
                    else
                    {
                        switch (Store)
                        {
                            case PSStoreMode.ADDS:
                                if (_target0 != null) 
                                    _target0.Update(this.Host);
                                else
                                    throw new Exception("Invalid ADDS DataStore Type !");
                                break;
                            case PSStoreMode.SQL:
                                if (_target1 != null)
                                    _target1.Update(this.Host);
                                else
                                    throw new Exception("Invalid SQL DataStore  Type !");
                                break;
                            case PSStoreMode.Custom:
                                if (_target2 != null)
                                    _target2.Update(this.Host);
                                else
                                    throw new Exception("Invalid Custom DataStore Type !");
                                break;
                        }
                    }
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
    /// <para type="description">Set TOTP Provider configuration data.</para>
    /// </summary>
    public class ADDSStoreDynamicParameters
    {
        private bool _active;
        private string _keyattribute;
        private string _mailattribute;
        private string _phoneattribute;
        private string _methodattribute;
        private string _overridemethodattribute;
        private string _enabledattribute;
        private string _pinattribute;
        private string _publickeycredentialattribute;
        private string _clientcertificateattribute;
        private string _rsacertificateattribute;
        private int _maxrows = 10000;
        private bool _usessl = false;

        internal bool ActiveChanged { get; private set; } = false;
        internal bool KeyAttributeChanged { get; private set; } = false;
        internal bool MailAttributeChanged { get; private set; } = false;
        internal bool PhoneAttributeChanged { get; private set; } = false;
        internal bool MethodAttributeChanged { get; private set; } = false;
        internal bool OverrideMethodAttributeChanged { get; private set; } = false;
        internal bool EnabledAttributeChanged { get; private set; } = false;
        internal bool PinAttributeChanged { get; private set; } = false;
        internal bool PublicKeyCredentialAttributeChanged { get; private set; } = false;
        internal bool ClientCertificateAttributeChanged { get; private set; } = false;
        internal bool RSACertificateAttributeChanged { get; private set; } = false;
        internal bool MaxRowsChanged { get; private set; } = false;
        internal bool UseSSLChanged { get; private set; } = false;

        /// <summary>
        /// <para type="description">If true, users metadata are stored in ADDS attributes.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool Active
        {
            get { return _active; }
            set
            {
                _active = value;
                ActiveChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">ADDS attribute name user to store user secret key (default msDS-cloudExtensionAttribute10).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string KeyAttribute
        {
            get { return _keyattribute; }
            set
            {
                _keyattribute = value;
                KeyAttributeChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">ADDS attribute name used to store user custom mail address (default msDS-cloudExtensionAttribute11).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string MailAttribute
        {
            get { return _mailattribute; }
            set
            {
                _mailattribute = value;
                MailAttributeChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">ADDS attribute name used to store user phone number (default msDS-cloudExtensionAttribute12).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string PhoneAttribute
        {
            get { return _phoneattribute; }
            set
            {
                _phoneattribute = value;
                PhoneAttributeChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">ADDS attribute name used to store user preferred method (Code, Phone, Mail) (default msDS-cloudExtensionAttribute13).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string MethodAttribute
        {
            get { return _methodattribute; }
            set
            {
                _methodattribute = value;
                MethodAttributeChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">ADDS attribute name used to store user preferred method (Code, Phone, Mail) (default msDS-cloudExtensionAttribute14).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string OverrideMethodAttribute
        {
            get { return _overridemethodattribute; }
            set
            {
                _overridemethodattribute = value;
                OverrideMethodAttributeChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">ADDS attribute name used to store user status with MFA (default msDS-cloudExtensionAttribute18).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string EnabledAttribute
        {
            get { return _enabledattribute; }
            set
            {
                _enabledattribute = value;
                EnabledAttributeChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">ADDS attribute name used to store user pin code (default msDS-cloudExtensionAttribute15).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string PinAttribute
        {
            get { return _pinattribute; }
            set
            {
                _pinattribute = value;
                PinAttributeChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">ADDS attribute name used to store multiple user Public Keys Credential (recommended msDS-KeyCredentialLink or othetMailbox).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string PublicKeyCredentialAttribute
        {
            get { return _publickeycredentialattribute; }
            set
            {
                _publickeycredentialattribute = value;
                PublicKeyCredentialAttributeChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">ADDS attribute name used to store Client Certificate (default msDS-cloudExtensionAttribute16).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string ClientCertificateAttribute
        {
            get { return _clientcertificateattribute; }
            set
            {
                _clientcertificateattribute = value;
                ClientCertificateAttributeChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">ADDS attribute name used to store RSA Certificate (default msDS-cloudExtensionAttribute17).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string RSACertificateAttribute
        {
            get { return _rsacertificateattribute; }
            set
            {
                _rsacertificateattribute = value;
                RSACertificateAttributeChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">ADDS value indicating the max row per request.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateRange(-1, 1000000)]
        public int MaxRows
        {
            get { return _maxrows; }
            set
            {
                _maxrows = value;
                MaxRowsChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">ADDS value indicating if we must use ldap or ldaps for requests.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public bool UseSSL
        {
            get { return _usessl; }
            set
            {
                _usessl = value;
                UseSSLChanged = true;
            }
        }
    }

    /// <summary>
    /// <para type="description">Set value for SQL Store.</para>
    /// </summary>
    public class SQLStoreDynamicParameters
    {
        private bool _active;
        private int _maxrows;
        private string _connectionstring;
        private string _aeccertificatethumbprint;
        private bool _isalwaysencrypted;
        private int _certificatevalidity;
        private bool _certificatereuse;
        private string _cryptokeyname;

        internal bool ActiveChanged { get; private set; } = false;
        internal bool MaxRowsChanged { get; private set; } = false;
        internal bool ConnectionStringChanged { get; private set; } = false;
        internal bool IsAlwaysEncryptedChanged { get; private set; } = false;
        internal bool AECCertificateThumbPrintChanged { get; private set; } = false;
        internal bool AECCertificateValidityChanged { get; private set; } = false;
        internal bool AECCertificateReuseChanged { get; private set; }
        internal bool AECCryptoKeyNameChanged { get; private set; }



        /// <summary>
        /// <para type="description">If true, users metadata are stored in SQL database.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool Active
        {
            get { return _active; }
            set
            {
                _active = value;
                ActiveChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Get or Set the connection string used to access MFA SQL Database.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string ConnectionString
        {
            get { return _connectionstring; }
            set
            {
                _connectionstring = value;
                ConnectionStringChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Get or Set the max rows limit used to access MFA SQL Database.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateRange(-1, 1000000)]
        public int MaxRows
        {
            get { return _maxrows; }
            set
            {
                _maxrows = value;
                MaxRowsChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Get or Set the SQLServer 2016 and up Always Encrypted feature. default = false.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool IsAlwaysEncrypted
        {
            get { return _isalwaysencrypted; }
            set
            {
                _isalwaysencrypted = value;
                IsAlwaysEncryptedChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Get or Set the validity of the generated certificate (in years, 5 per default).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateRange(1, 100)]
        public int AECCertificateValidity
        {
            get { return _certificatevalidity; }
            set
            {
                _certificatevalidity = value;
                AECCertificateValidityChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Get or Set the SQLServer 2016 and up Always Encrypted feature Thumprint.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string AECCertificateThumbPrint
        {
            get { return _aeccertificatethumbprint; }
            set
            {
                _aeccertificatethumbprint = value;
                AECCertificateThumbPrintChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Get or Set the indicating if we are reusing an existing certificate.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool AECCertificateReuse
        {
            get { return _certificatereuse; }
            set
            {
                _certificatereuse = value;
                AECCertificateReuseChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Get or Set the SQLServer 2016 crypting key name.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string AECCryptoKeyName
        {
            get { return _cryptokeyname; }
            set
            {
                _cryptokeyname = value;
                AECCryptoKeyNameChanged = true;
            }
        }
    }

    /// <summary>
    /// <para type="description">Set value for Custom Store.</para>
    /// </summary>
    public class CustomStoreDynamicParameters
    {
        private bool _active;
        private int _maxrows;
        private string _connectionstring;
        private string _datafullqualifiedimplementation;
        private string _keysfullqualifiedimplementation;
        private string _parameters;

        internal bool ActiveChanged { get; private set; } = false;
        internal bool MaxRowsChanged { get; private set; } = false;
        internal bool ConnectionStringChanged { get; private set; } = false;
        internal bool DataImplementationChanged { get; private set; } = false;
        internal bool KeysImplementationChanged { get; private set; } = false;
        internal bool ParametersChanged { get; private set; } = false;


        /// <summary>
        /// <para type="description">If true, users metadata are stored in Custom Store.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool Active
        {
            get { return _active; }
            set
            {
                _active = value;
                ActiveChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Set the connection string used to access MFA Custom Store.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string ConnectionString
        {
            get { return _connectionstring; }
            set
            {
                _connectionstring = value;
                ConnectionStringChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Set the max rows limit used to access MFA Custom Store.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateRange(-1, 1000000)]
        public int MaxRows
        {
            get { return _maxrows; }
            set
            {
                _maxrows = value;
                MaxRowsChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Set the Assembly description used to access MFA Custom Store.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public string DataRepositoryFullyQualifiedImplementation
        {
            get { return _datafullqualifiedimplementation; }
            set
            {
                _datafullqualifiedimplementation = value;
                DataImplementationChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Set the Assembly description used to access MFA Custom Keys Store.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public string KeysRepositoryFullyQualifiedImplementation
        {
            get { return _keysfullqualifiedimplementation; }
            set
            {
                _keysfullqualifiedimplementation = value;
                KeysImplementationChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Set the Assembly Parameters used to access MFA Custom Store.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public string Parameters
        {
            get { return _parameters; }
            set
            {
                _parameters = value;
                ParametersChanged = true;
            }
        }
    }
    #endregion

    #region Set-MFAActiveDirectoryTemplate
    /// <summary>
    /// <para type="synopsis">Set basic Firewall rules for MFA inter servers communication.</para>
    /// <para type="description">Set basic Firewall rules for MFA inter servers communication.</para>
    /// </summary>
    /// <example>
    ///   <para>Set-MFAActiveDirectoryTemplate -Kind SchemaAll | Schema2016 | SchemaMFA </para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "MFAActiveDirectoryTemplate", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class SetMFAActiveDirectoryTemplate : MFACmdlet
    {
        /// <summary>
        /// <para type="description">Template type for ADDS Attributes configuration.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Data")]
        public PSADDSTemplateKind Kind { get; set; } = PSADDSTemplateKind.SchemaAll;

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ShouldProcess("ADDS Attributes template Configuration"))
            {
                try
                {
                    PSADDSStore.SetADDSAttributesTemplate(Kind);
                    this.WriteVerbose(infos_strings.InfosConfigUpdated);
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "4025", ErrorCategory.OperationStopped, this));
                }
            }
        }
    }

    #endregion

    #region Get-MFAProvider
    /// <summary>
    ///     <para type="synopsis">Get MFA Provider Configuration.</para>
    ///     <para type="description">Get MFA Provider configuration options.</para>
    ///     <para type="description"></para>
    /// </summary>
    /// <example>
    ///     <para>Get-MFAProvider -ProviderType External</para>
    ///     <para>Get MFA Provider configuration for (Code, Email, External, Azure, Biometrics)</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "MFAProvider", SupportsShouldProcess = true, SupportsPaging = false, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [OutputType(typeof(PSTOTPProvider), typeof(PSMailProvider), typeof(PSExternalProvider), typeof(PSAzureProvider), typeof(PSBiometricProvider))]
    public sealed class GetMFAProvider : MFACmdlet
    {
        private PSTOTPProvider _config0;
        private PSMailProvider _config1;
        private PSExternalProvider _config2;
        private PSAzureProvider _config3;
        private PSBiometricProvider _config4;

        /// <summary>
        /// <para type="description">Provider Type parameter, (Code, Email, External, Azure, Biometrics) Required.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data", ValueFromPipeline = true)]
        [ValidateSet("Code", "Email", "External", "Azure", "Biometrics", IgnoreCase = true)]
        public PSProviderType ProviderType { get; set; } = PSProviderType.Code;

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                switch (ProviderType)
                {
                    case PSProviderType.Code:
                        FlatTOTPProvider cf0 = new FlatTOTPProvider();
                        cf0.Load(this.Host);
                        _config0 = (PSTOTPProvider)cf0;
                        break;
                    case PSProviderType.Email:
                        FlatMailProvider cf1 = new FlatMailProvider();
                        cf1.Load(this.Host);
                        _config1 = (PSMailProvider)cf1;
                        break;
                    case PSProviderType.External:
                        FlatExternalProvider cf2 = new FlatExternalProvider();
                        cf2.Load(this.Host);
                        _config2 = (PSExternalProvider)cf2;
                        break;
                    case PSProviderType.Azure:
                        FlatAzureProvider cf3 = new FlatAzureProvider();
                        cf3.Load(this.Host);
                        _config3 = (PSAzureProvider)cf3;
                        break;
                    case PSProviderType.Biometrics:
                        FlatBiometricProvider cf4 = new FlatBiometricProvider();
                        cf4.Load(this.Host);
                        _config4 = (PSBiometricProvider)cf4;
                        break;

                    default:
                        break;
                }
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
            _config0 = null;
            _config1 = null;
            _config2 = null;
            _config3 = null;
            _config4 = null;
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
                    switch (ProviderType)
                    {
                        case PSProviderType.Code:
                            WriteObject(_config0);
                            break;
                        case PSProviderType.Email:
                            WriteObject(_config1);
                            break;
                        case PSProviderType.External:
                            WriteObject(_config2);
                            break;
                        case PSProviderType.Azure:
                            WriteObject(_config3);
                            break;
                        case PSProviderType.Biometrics:
                            WriteObject(_config4);
                            break;
                        default:
                            break;
                    }
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
            _config0 = null;
            _config1 = null;
            _config2 = null;
            _config3 = null;
            _config4 = null;
            base.StopProcessing();
        }
    }
    #endregion

    #region Set-MFAProvider
    /// <summary>
    /// <para type="synopsis">Set MFA Provider Configuration.</para>
    /// <para type="description">Set MFA Provider configuration options.</para>
    /// </summary>
    /// <example>
    ///   <para>Set-MFAProvider -ProviderType Code -Data $cfg</para>
    /// </example>
    /// <example>
    ///   <para>$cfg = Get-MFAProvider -ProviderType email</para>
    ///   <para>$cfg.Host = smtp.office365.com</para>
    ///   <para>Set-MFAProvider Email $cfg</para>
    /// </example>
    /// <example>
    ///   <para></para>
    ///   <para>$cfg = Get-MFAProvider -ProviderType email</para>
    ///   <para>$cfg.MailOTP.Templates</para>
    ///   <para>$cfg.MailOTP.AddTemplate(1036, "c:\temp\mytemplate.html")</para>
    ///   <para>$cfg.MailOTP.SetTemplate(1033, "c:\temp\mytemplate2.html")</para>
    ///   <para>$cfg.MailOTP.RemoveTemplate(1033)</para>
    ///   <para>Set-MFAProvider -ProviderType email $cfg</para>
    /// </example>
    /// <example>
    ///   <para>Set-MFAProvider -ProviderType email -UserName user@domain.com -Password p@ssw0rd</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "MFAProvider", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired]
    public sealed class SetMFAProvider : MFACmdlet, IDynamicParameters
    {
        private TOTPDynamicParameters _config0;
        private MailDynamicParameters _config1;
        private ExternalDynamicParameters _config2;
        private AzureDynamicParameters _config3;
        private BiometricDynamicParameters _config4;

        private FlatTOTPProvider _target0;
        private FlatMailProvider _target1;
        private FlatExternalProvider _target2;
        private FlatAzureProvider _target3;
        private FlatBiometricProvider _target4;
        private PSBaseProvider _config;

        private bool ConfigChanged { get; set; }

        /// <summary>
        /// <para type="description">Provider Type parameter, (Code, Email, External, Azure, Biometrics) Required.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data", ValueFromPipeline = true)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Identity")]
        [ValidateRange(PSProviderType.Code, PSProviderType.Biometrics)]
        public PSProviderType ProviderType { get; set; } = PSProviderType.Code;

        /// <summary>
        /// <para type="description">Config parameter, a variable of type PSConfigSQL.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Data", ValueFromPipeline = true, DontShow = true)]
        [ValidateNotNullOrEmpty()]
        public PSBaseProvider Config
        {
            get { return _config; }
            set
            {
                _config = value;
                ConfigChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Set the value of Provider configuration.</para>
        /// GetDynamicParameters implementation
        /// </summary>
        public object GetDynamicParameters()
        {
            if (!ConfigChanged)
            {

                switch (ProviderType)
                {
                    case PSProviderType.Code:
                        _config0 = new TOTPDynamicParameters();
                        return _config0;
                    case PSProviderType.Email:
                        _config1 = new MailDynamicParameters();
                        return _config1;
                    case PSProviderType.External:
                        _config2 = new ExternalDynamicParameters();
                        return _config2;
                    case PSProviderType.Azure:
                        _config3 = new AzureDynamicParameters();
                        return _config3;
                    case PSProviderType.Biometrics:
                        _config4 = new BiometricDynamicParameters();
                        return _config4;

                    default:
                        break;
                }
            }
            return null;
        }

        /// <summary>
        /// BeginProcessing method override
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                if (!ConfigChanged)
                {
                    ManagementService.Initialize(this.Host, true);
                    switch (ProviderType)
                    {
                        case PSProviderType.Code:
                            _target0 = new FlatTOTPProvider();
                            _target0.Load(this.Host);
                            if (_config0.EnabledChanged)
                                _target0.Enabled = _config0.Enabled;
                            if (_config0.ForceWizardChanged)
                                _target0.ForceWizard = (ForceWizardMode)_config0.ForceWizard;
                            if (_config0.EnrollWizardChanged)
                                _target0.EnrollWizard = _config0.EnrollWizard;
                            if (_config0.IsRequiredChanged)
                                _target0.IsRequired = _config0.IsRequired;
                            if (_config0.PinRequiredChanged)
                                _target0.PinRequired = _config0.PinRequired;
                            if (_config0.FullyQualifiedImplementationChanged)
                                _target0.FullyQualifiedImplementation= _config0.FullyQualifiedImplementation;
                            if (_config0.ParametersChanged)
                                _target0.Parameters = _config0.Parameters;

                            if (_config0.WizardOptionsChanged)
                                _target0.WizardOptions = (OTPWizardOptions)_config0.WizardOptions;
                            if (_config0.AlgorithmChanged)
                                _target0.Algorithm = (HashMode)_config0.Algorithm;
                            if (_config0.TOTPShadowsChanged)
                                _target0.TOTPShadows = _config0.TOTPShadows;
                            if (_config0.KeySizeModeChanged)
                                _target0.KeySize = (KeySizeMode)_config0.KeySize;
                            if (_config0.SecretFormatChanged)
                                _target0.KeysFormat = (SecretKeyFormat)_config0.KeysFormat;
                            if (_config0.DigitsChanged)
                                _target0.Digits = _config0.Digits;
                            if (_config0.DurationChanged)
                                _target0.Duration = _config0.Duration;
                            break;
                        case PSProviderType.Email:
                            _target1 = new FlatMailProvider();
                            _target1.Load(this.Host);
                            if (_config1.EnabledChanged)
                                _target1.Enabled = _config1.Enabled;
                            if (_config1.ForceWizardChanged)
                                _target1.ForceWizard = (ForceWizardMode)_config1.ForceWizard;
                            if (_config1.EnrollWizardChanged)
                                _target1.EnrollWizard = _config1.EnrollWizard;
                            if (_config1.IsRequiredChanged)
                                _target1.IsRequired = _config1.IsRequired;
                            if (_config1.PinRequiredChanged)
                                _target1.PinRequired = _config1.PinRequired;
                            if (_config1.FullyQualifiedImplementationChanged)
                                _target1.FullyQualifiedImplementation = _config1.FullyQualifiedImplementation;
                            if (_config1.ParametersChanged)
                                _target1.Parameters = _config1.Parameters;

                            if (_config1.FromChanged)
                                _target1.From = _config1.From;
                            if (_config1.UserNameChanged)
                                _target1.UserName = _config1.UserName;
                            if (_config1.PasswordChanged)
                                _target1.Password = _config1.Password;
                            if (_config1.HostChanged)
                                _target1.Host = _config1.Host;
                            if (_config1.PortChanged)
                                _target1.Port = _config1.Port;
                            if (_config1.UseSSLChanged)
                                _target1.UseSSL = _config1.UseSSL;
                            if (_config1.CompanyChanged)
                                _target1.Company = _config1.Company;
                            if (_config1.AnonymousChanged)
                                _target1.Anonymous = _config1.Anonymous;
                            if (_config1.DeliveryNotificationsChanged)
                                _target1.DeliveryNotifications = _config1.DeliveryNotifications;
                            break;
                        case PSProviderType.External:
                            _target2 = new FlatExternalProvider();
                            _target2.Load(this.Host);
                            if (_config2.EnabledChanged)
                                _target2.Enabled = _config2.Enabled;
                            if (_config2.ForceWizardChanged)
                                _target2.ForceWizard = (ForceWizardMode)_config2.ForceWizard;
                            if (_config2.EnrollWizardChanged)
                                _target2.EnrollWizard = _config2.EnrollWizard;
                            if (_config2.IsRequiredChanged)
                                _target2.IsRequired = _config2.IsRequired;
                            if (_config2.PinRequiredChanged)
                                _target2.PinRequired = _config2.PinRequired;
                            if (_config2.FullyQualifiedImplementationChanged)
                                _target2.FullyQualifiedImplementation = _config2.FullyQualifiedImplementation;
                            if (_config2.ParametersChanged)
                                _target2.Parameters = _config2.Parameters;

                            if (_config2.CompanyChanged)
                                _target2.Company = _config2.Company;
                            if (_config2.Sha1SaltChanged)
                                _target2.Sha1Salt = _config2.Sha1Salt;
                            if (_config2.IsTwoWayChanged)
                                _target2.IsTwoWay = _config2.IsTwoWay;
                            if (_config2.TimeOutChanged)
                                _target2.Timeout = _config2.Timeout;
                            break;
                        case PSProviderType.Azure:
                            _target3 = new FlatAzureProvider();
                            _target3.Load(this.Host);
                            if (_config3.EnabledChanged)
                                _target3.Enabled = _config3.Enabled;
                            if (_config3.ForceWizardChanged)
                                _target3.ForceWizard = (ForceWizardMode)_config3.ForceWizard;
                            if (_config3.EnrollWizardChanged)
                                _target3.EnrollWizard = _config3.EnrollWizard;
                            if (_config3.IsRequiredChanged)
                                _target3.IsRequired = _config3.IsRequired;
                            if (_config3.PinRequiredChanged)
                                _target3.PinRequired = _config3.PinRequired;
                            if (_config3.FullyQualifiedImplementationChanged)
                                _target3.FullyQualifiedImplementation = _config3.FullyQualifiedImplementation;
                            if (_config3.ParametersChanged)
                                _target3.Parameters = _config3.Parameters;

                            if (_config3.TenantIdChanged)
                                _target3.TenantId = _config3.TenantId;
                            if (_config3.ThumbPrintChanged)
                                _target3.ThumbPrint = _config3.Thumbprint;
                            break;
                        case PSProviderType.Biometrics:
                            _target4 = new FlatBiometricProvider();
                            _target4.Load(this.Host);
                            if (_config4.EnabledChanged)
                                _target4.Enabled = _config4.Enabled;
                            if (_config4.ForceWizardChanged)
                                _target4.ForceWizard = (ForceWizardMode)_config4.ForceWizard;
                            if (_config4.EnrollWizardChanged)
                                _target4.EnrollWizard = _config4.EnrollWizard;
                            if (_config4.IsRequiredChanged)
                                _target4.IsRequired = _config4.IsRequired;
                            if (_config4.PinRequiredChanged)
                                _target4.PinRequired = _config4.PinRequired;
                            if (_config4.PinRequirementsChanged)
                                _target4.PinRequirements = (WebAuthNPinRequirements)_config4.PinRequirements;
                            if (_config4.FullyQualifiedImplementationChanged)
                                _target4.FullyQualifiedImplementation = _config4.FullyQualifiedImplementation;
                            if (_config4.ParametersChanged)
                                _target4.Parameters = _config4.Parameters;

                            if (_config4.ChallengeSizeChanged)
                                _target4.ChallengeSize = _config4.ChallengeSize;
                            if (_config4.OriginChanged)
                                _target4.Origin = _config4.Origin;
                            if (_config4.ServerNameChanged)
                                _target4.ServerName = _config4.ServerName;
                            if (_config4.ServerDomainChanged)
                                _target4.ServerDomain = _config4.ServerDomain;
                            if (_config4.DirectLoginChanged)
                                _target4.DirectLogin = _config4.DirectLogin;
                            if (_config4.TimeToleranceChanged)
                                _target4.TimestampDriftTolerance = _config4.TimestampDriftTolerance;
                            if (_config4.TimeoutChanged)
                                _target4.Timeout = _config4.Timeout;
                            break;
                    }
                }
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
            const string error = "Invalid Provider Type !";
            if (ShouldProcess("MFA Providers Configuration"))
            {
                try
                {
                    if (ConfigChanged)
                    {
                        switch (ProviderType)
                        {
                            case PSProviderType.Code:
                                if (Config is PSTOTPProvider)
                                    ((FlatTOTPProvider)((PSTOTPProvider)Config)).Update(this.Host);
                                else
                                    throw new Exception(error);
                                break;
                            case PSProviderType.Email:
                                if (Config is PSMailProvider)
                                    ((FlatMailProvider)((PSMailProvider)Config)).Update(this.Host);
                                else
                                    throw new Exception(error);

                                break;
                            case PSProviderType.External:
                                if (Config is PSExternalProvider)
                                    ((FlatExternalProvider)((PSExternalProvider)Config)).Update(this.Host);
                                else
                                    throw new Exception(error);

                                break;
                            case PSProviderType.Azure:
                                if (Config is PSAzureProvider)
                                    ((FlatAzureProvider)((PSAzureProvider)Config)).Update(this.Host);
                                else
                                    throw new Exception(error);

                                break;
                            case PSProviderType.Biometrics:
                                if (Config is PSBiometricProvider)
                                    ((FlatBiometricProvider)((PSBiometricProvider)Config)).Update(this.Host);
                                else
                                    throw new Exception(error);
                                break;
                            default:
                                throw new Exception(error);
                        }
                    }
                    else
                    {
                        switch (ProviderType)
                        {
                            case PSProviderType.Code:
                                if (_target0 != null)
                                    _target0.Update(this.Host);
                                else
                                    throw new Exception(error);
                                break;
                            case PSProviderType.Email:
                                if (_target1 != null)
                                    _target1.Update(this.Host);
                                else
                                    throw new Exception(error);
                                break;
                            case PSProviderType.External:
                                if (_target2 != null)
                                    _target2.Update(this.Host);
                                else
                                    throw new Exception(error);
                                break;
                            case PSProviderType.Azure:
                                if (_target3 != null)
                                    _target3.Update(this.Host);
                                else
                                    throw new Exception(error);
                                break;
                            case PSProviderType.Biometrics:
                                if (_target4 != null)
                                    _target4.Update(this.Host);
                                else
                                    throw new Exception(error);
                                break;
                            default:
                                throw new Exception(error);
                        }
                    }
                    this.WriteVerbose(infos_strings.InfosConfigUpdated);
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "3024", ErrorCategory.OperationStopped, this));
                }
            }
        }

        /// <summary>
        /// EndProcessing method implementation
        /// </summary>
        protected override void EndProcessing()
        {
            _config0 = null;
            _config1 = null;
            _config2 = null;
            _config3 = null;
            _config4 = null;
            base.EndProcessing();
        }

        /// <summary>
        /// StopProcessing method implementation
        /// </summary>
        protected override void StopProcessing()
        {
            _config0 = null;
            _config1 = null;
            _config2 = null;
            _config3 = null;
            _config4 = null;
            base.StopProcessing();
        }
    }

    /// <summary>
    /// <para type="description">Set TOTP Provider configuration data.</para>
    /// </summary>
    public class TOTPDynamicParameters
    {
        private bool _enabled = true;
        private bool _enrollwizard = true;
        private ForceWizardMode _forcewizard = ForceWizardMode.Disabled;
        private bool _pinrequired = false;
        private bool _isrequired = false;
        private string _fullyqualifiedimplementation;
        private string _parameters;
        private int _totpshadows = 2;
        private int _digits = 6;
        private int _duration = 30;
        private PSHashMode _algorithm = PSHashMode.SHA1;
        private PSOTPWizardOptions _wizardoptions = PSOTPWizardOptions.All;
        private PSSecretKeyFormat _secretformat = PSSecretKeyFormat.RNG;
        private PSKeySizeMode _keysizemode = PSKeySizeMode.KeySizeDefault;

        internal bool AlgorithmChanged { get; private set; }
        internal bool TOTPShadowsChanged { get; private set; }
        internal bool ParametersChanged { get; private set; }
        internal bool FullyQualifiedImplementationChanged { get; private set; }
        internal bool IsRequiredChanged { get; private set; }
        internal bool PinRequiredChanged { get; private set; }
        internal bool ForceWizardChanged { get; private set; }
        internal bool EnabledChanged { get; private set; }
        internal bool EnrollWizardChanged { get; private set; }
        internal bool WizardOptionsChanged { get; private set; }      
        internal bool KeySizeModeChanged { get; private set; }
        internal bool SecretFormatChanged { get; private set; }
        internal bool DigitsChanged { get; private set; }
        internal bool DurationChanged { get; private set; }

        /// <summary>
        /// <para type="description">Provider Enabled property.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                EnabledChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Provider Enrollment Wizard Enabled property.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool EnrollWizard
        {
            get { return _enrollwizard; }
            set
            {
                _enrollwizard = value;
                EnrollWizardChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Provider Force Wizard if user dosen't complete during signing.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateSet("Disabled", "Enabled", "Strict", IgnoreCase = true)]
        public ForceWizardMode ForceWizard
        {
            get { return _forcewizard; }
            set
            {
                _forcewizard = value;
                ForceWizardChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Force users to use this provider (mandatory for registration or not).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool IsRequired
        {
            get { return _isrequired; }
            set
            {
                _isrequired = value;
                IsRequiredChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Set if additionnal verification with PIN (locally administered) must be done.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool PinRequired
        {
            get { return _pinrequired; }
            set
            {
                _pinrequired = value;
                PinRequiredChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Full qualified implementation class for replacing default provider.</para>
        /// </summary>       
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string FullyQualifiedImplementation
        {
            get { return _fullyqualifiedimplementation; }
            set
            {
                _fullyqualifiedimplementation = value;
                FullyQualifiedImplementationChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Full qualified implementation parameters for replacing default provider.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Parameters
        {
            get { return _parameters; }
            set
            {
                _parameters = value;
                ParametersChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">TOTP Provider Shadow codes. 2 by default</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateRange(1, 10)]
        public int TOTPShadows
        {
            get { return _totpshadows; }
            set
            {
                _totpshadows = value;
                TOTPShadowsChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">TOTP Provider Hash algorithm. SHA1 by default</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateSet("SHA1", "SHA256", "SHA384", "SHA512", IgnoreCase = true)]
        public PSHashMode Algorithm
        {
            get { return _algorithm; }
            set
            {
                _algorithm = value;
                AlgorithmChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">TOTP Provider Code len between 4 and 8. 6 by default</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateRange(4, 8)]
        public int Digits
        {
            get { return _digits; }
            set
            {
                _digits = value;
                DigitsChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">TOTP Provider Code renew duration in seconds. 30s by default</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateSet("30", "60", "90", "120", "150", "180")]
        public int Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                DurationChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Set TOP Wizard Application list enabled/ disabled.</para>
        /// <para type="description">All, all links are displayed.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateSet("All", "NoMicrosoftAuthenticator", "NoGoogleAuthenticator", "NoAuthyAuthenticator", "NoGooglSearch", IgnoreCase = true)]
        public PSOTPWizardOptions WizardOptions
        {
            get { return _wizardoptions; }
            set
            {
                _wizardoptions = value;
                WizardOptionsChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Type of generated Keys for users (RNG, RSA, CUSTOM RSA).</para>
        /// <para type="description">Changing the key format, invalidate all the users secret keys previously used.</para>
        /// <para type="description">RSA and RSA Custom are using Certificates. Custom RSA must Use Specific database to the keys and certs, one for each user, see New-MFASecretKeysDatabase cmdlet.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateRange(PSSecretKeyFormat.RNG, PSSecretKeyFormat.CUSTOM)]
        public PSSecretKeyFormat KeysFormat
        {
            get { return _secretformat; }
            set
            {
                _secretformat = value;
                SecretFormatChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Used to trim the key at a fixed size, when you use RSA the key is very long, and QRCode is often too big for TOTP Application (1024 is a good size, even if RSA key is 2048 bytes long).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateSet("KeySizeDefault", "KeySize128", "KeySize256", "KeySize384", "KeySize512", "KeySize1024", "KeySize2048", IgnoreCase = true)]
        public PSKeySizeMode KeySize
        {
            get { return _keysizemode; }
            set
            {
                _keysizemode = value;
                KeySizeModeChanged = true;
            }
        }
    }

    /// <summary>
    /// <para type="description">Set Mail Provider configuration data.</para>
    /// </summary>
    public class MailDynamicParameters
    {
        private bool _enabled;
        private bool _enrollwizard;
        private ForceWizardMode _forcewizard = ForceWizardMode.Disabled;
        private bool _pinrequired;
        private bool _isrequired;
        private string _fullyqualifiedimplementation;
        private string _parameters;

        private string _from;
        private string _username;
        private string _password;
        private string _host;
        private int _port;
        private bool _usessl;
        private string _company;
        private bool _anonymous;
        private bool _deliverynotifications;

        internal bool DeliveryNotificationsChanged { get; private set; }
        internal bool AnonymousChanged { get; private set; }
        internal bool CompanyChanged { get; private set; }
        internal bool UseSSLChanged { get; private set; }
        internal bool PortChanged { get; private set; }
        internal bool HostChanged { get; private set; }
        internal bool PasswordChanged { get; private set; }
        internal bool UserNameChanged { get; private set; }
        internal bool FromChanged { get; private set; }
        internal bool ParametersChanged { get; private set; }
        internal bool FullyQualifiedImplementationChanged { get; private set; }
        internal bool IsRequiredChanged { get; private set; }
        internal bool PinRequiredChanged { get; private set; }
        internal bool ForceWizardChanged { get; private set; }
        internal bool EnabledChanged { get; private set; }
        internal bool EnrollWizardChanged { get; private set; }

        /// <summary>
        /// <para type="description">Provider Enabled property.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                EnabledChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Provider Enrollment Wizard Enabled property.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool EnrollWizard
        {
            get { return _enrollwizard; }
            set
            {
                _enrollwizard = value;
                EnrollWizardChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Provider Force Wizard if user dosen't complete during signing.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateSet("Disabled", "Enabled", "Strict", IgnoreCase = true)]
        public ForceWizardMode ForceWizard
        {
            get { return _forcewizard; }
            set
            {
                _forcewizard = value;
                ForceWizardChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Force users to use this provider (mandatory for registration or not).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool IsRequired
        {
            get { return _isrequired; }
            set
            {
                _isrequired = value;
                IsRequiredChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Set if additionnal verification with PIN (locally administered) must be done.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool PinRequired
        {
            get { return _pinrequired; }
            set
            {
                _pinrequired = value;
                PinRequiredChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Full qualified implementation class for replacing default provider.</para>
        /// </summary>       
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string FullyQualifiedImplementation
        {
            get { return _fullyqualifiedimplementation; }
            set
            {
                _fullyqualifiedimplementation = value;
                FullyQualifiedImplementationChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Full qualified implementation parameters for replacing default provider.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Parameters
        {
            get { return _parameters; }
            set
            {
                _parameters = value;
                ParametersChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Mail from property.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string From
        {
            get { return _from; }
            set
            {
                _from = value;
                FromChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Account Name used to access Mail platform.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string UserName
        {
            get { return _username; }
            set
            {
                _username = value;
                UserNameChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Password used with Username to access Mail platform.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                PasswordChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Mail platform Host eg : smtp.office365.com.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Host
        {
            get { return _host; }
            set
            {
                _host = value;
                HostChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Mail platform IP Port eg : 587.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateRange(1, 65535)]
        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                PortChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Mail platform SSL option.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool UseSSL
        {
            get { return _usessl; }
            set
            {
                _usessl = value;
                UseSSLChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Your company description, tis is used in default mails contents.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Company
        {
            get { return _company; }
            set
            {
                _company = value;
                CompanyChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">indicate if connetion is Anonymous.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool Anonymous
        {
            get { return _anonymous; }
            set
            {
                _anonymous = value;
                AnonymousChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">if your want Delivery Failures, Delayed delivery or nothing (default).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool DeliveryNotifications
        {
            get { return _deliverynotifications; }
            set
            {
                _deliverynotifications = value;
                DeliveryNotificationsChanged = true;
            }
        }
    }

    /// <summary>
    /// <para type="description">Set External Provider configuration data.</para>
    /// </summary>
    public class ExternalDynamicParameters
    {
        private bool _enabled;
        private bool _enrollwizard;
        private ForceWizardMode _forcewizard = ForceWizardMode.Disabled;
        private bool _pinrequired;
        private bool _isrequired;
        private string _fullyqualifiedimplementation;
        private string _parameters;
        private string _company;
        private string _sha1salt;
        private bool _istwoway;
        private int _timeout;

        internal bool TimeOutChanged { get; private set; }
        internal bool IsTwoWayChanged { get; private set; }
        internal bool Sha1SaltChanged { get; private set; }
        internal bool CompanyChanged { get; private set; }
        internal bool ParametersChanged { get; private set; }
        internal bool FullyQualifiedImplementationChanged { get; private set; }
        internal bool IsRequiredChanged { get; private set; }
        internal bool PinRequiredChanged { get; private set; }
        internal bool ForceWizardChanged { get; private set; }
        internal bool EnabledChanged { get; private set; }
        internal bool EnrollWizardChanged { get; private set; }

        /// <summary>
        /// <para type="description">Provider Enabled property.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                EnabledChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Provider Enrollment Wizard Enabled property.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool EnrollWizard
        {
            get { return _enrollwizard; }
            set
            {
                _enrollwizard = value;
                EnrollWizardChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Provider Force Wizard if user dosen't complete during signing.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateSet("Disabled", "Enabled", "Strict", IgnoreCase = true)]
        public ForceWizardMode ForceWizard
        {
            get { return _forcewizard; }
            set
            {
                _forcewizard = value;
                ForceWizardChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Force users to use this provider (mandatory for registration or not).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool IsRequired
        {
            get { return _isrequired; }
            set
            {
                _isrequired = value;
                IsRequiredChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Set if additionnal verification with PIN (locally administered) must be done.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool PinRequired
        {
            get { return _pinrequired; }
            set
            {
                _pinrequired = value;
                PinRequiredChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Full qualified implementation class for replacing default provider.</para>
        /// </summary>       
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string FullyQualifiedImplementation
        {
            get { return _fullyqualifiedimplementation; }
            set
            {
                _fullyqualifiedimplementation = value;
                FullyQualifiedImplementationChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Full qualified implementation parameters for replacing default provider.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Parameters
        {
            get { return _parameters; }
            set
            {
                _parameters = value;
                ParametersChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">your company name, can be used to format External message sent to user.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Company
        {
            get { return _company; }
            set
            {
                _company = value;
                CompanyChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Optional Salt value, if your gateway support this feature.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Sha1Salt
        {
            get { return _sha1salt; }
            set
            {
                _sha1salt = value;
                Sha1SaltChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Pass parameter to your implemented provider, indicating if the mode is Request/Response</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool IsTwoWay
        {
            get { return _istwoway; }
            set
            {
                _istwoway = value;
                IsTwoWayChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">TimeOut Before cancelling operation</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateRange(30, 600)]
        public int Timeout
        {
            get { return _timeout; }
            set
            {
                _timeout = value;
                TimeOutChanged = true;
            }
        }
    }

    /// <summary>
    /// <para type="description">Set Azure Provider configuration data.</para>
    /// </summary>
    public class AzureDynamicParameters
    {
        private bool _enabled;
        private bool _enrollwizard;
        private ForceWizardMode _forcewizard = ForceWizardMode.Disabled;
        private bool _pinrequired;
        private bool _isrequired;
        private string _fullyqualifiedimplementation;
        private string _parameters;
        private string _tenantid;
        private string _thumbprint;

        internal bool ThumbPrintChanged { get; private set; }
        internal bool TenantIdChanged { get; private set; }
        internal bool ParametersChanged { get; private set; }
        internal bool FullyQualifiedImplementationChanged { get; private set; }
        internal bool IsRequiredChanged { get; private set; }
        internal bool PinRequiredChanged { get; private set; }
        internal bool ForceWizardChanged { get; private set; }
        internal bool EnabledChanged { get; private set; }
        internal bool EnrollWizardChanged { get; private set; }

        /// <summary>
        /// <para type="description">Provider Enabled property.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                EnabledChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Provider Enrollment Wizard Enabled property.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool EnrollWizard
        {
            get { return _enrollwizard; }
            set
            {
                _enrollwizard = value;
                EnrollWizardChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Provider Force Wizard if user dosen't complete during signing.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateSet("Disabled", "Enabled", "Strict", IgnoreCase = true)]
        public ForceWizardMode ForceWizard
        {
            get { return _forcewizard; }
            set
            {
                _forcewizard = value;
                ForceWizardChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Force users to use this provider (mandatory for registration or not).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool IsRequired
        {
            get { return _isrequired; }
            set
            {
                _isrequired = value;
                IsRequiredChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Set if additionnal verification with PIN (locally administered) must be done.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool PinRequired
        {
            get { return _pinrequired; }
            set
            {
                _pinrequired = value;
                PinRequiredChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Full qualified implementation class for replacing default provider.</para>
        /// </summary>       
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string FullyQualifiedImplementation
        {
            get { return _fullyqualifiedimplementation; }
            set
            {
                _fullyqualifiedimplementation = value;
                FullyQualifiedImplementationChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Full qualified implementation parameters for replacing default provider.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Parameters
        {
            get { return _parameters; }
            set
            {
                _parameters = value;
                ParametersChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">your Azure/o365 tenantId / tenant name.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string TenantId
        {
            get { return _tenantid; }
            set
            {
                _tenantid = value;
                TenantIdChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Thumbprint of the Azure cetificate (Done when configuring Azure MFA.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Thumbprint
        {
            get { return _thumbprint; }
            set
            {
                _thumbprint = value;
                ThumbPrintChanged = true;
            }
        }
    }

    /// <summary>
    /// <para type="description">Set Biometric Provider configuration data.</para>
    /// </summary>
    public class BiometricDynamicParameters
    {
        private bool _enabled;
        private bool _enrollwizard;
        private ForceWizardMode _forcewizard = ForceWizardMode.Disabled;
        private bool _pinrequired;
        private PSWebAuthNPinRequirements _pinrequirements = PSWebAuthNPinRequirements.Null;
        private bool _isrequired;
        private string _fullyqualifiedimplementation;
        private string _parameters;
        private uint _timeout;        
        private int _timetolerance;
        private bool _directlogin;
        private string _serverdomain;
        private string _servername;
        private string _url;
        private int _challengsize;

        internal bool ChallengeSizeChanged { get; private set; }
        internal bool OriginChanged { get; private set; }
        internal bool ServerNameChanged { get; private set; }
        internal bool ServerDomainChanged { get; private set; }
        internal bool DirectLoginChanged { get; private set; }
        internal bool TimeToleranceChanged { get; private set; }
        internal bool TimeoutChanged { get; private set; }
        internal bool ParametersChanged { get; private set; }
        internal bool FullyQualifiedImplementationChanged { get; private set; }
        internal bool IsRequiredChanged { get; private set; }
        internal bool PinRequiredChanged { get; private set; }
        internal bool PinRequirementsChanged { get; private set; }
        internal bool ForceWizardChanged { get; private set; }
        internal bool EnabledChanged { get; private set; }
        internal bool EnrollWizardChanged { get; private set; }

        /// <summary>
        /// <para type="description">Provider Enabled property.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                EnabledChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Provider Enrollment Wizard Enabled property.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool EnrollWizard
        {
            get { return _enrollwizard; }
            set
            {
                _enrollwizard = value;
                EnrollWizardChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Provider Force Wizard if user dosen't complete during signing.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateSet("Disabled", "Enabled", "Strict", IgnoreCase = true)]
        public ForceWizardMode ForceWizard
        {
            get { return _forcewizard; }
            set
            {
                _forcewizard = value;
                ForceWizardChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Force users to use this provider (mandatory for registration or not).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool IsRequired
        {
            get { return _isrequired; }
            set
            {
                _isrequired = value;
                IsRequiredChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Set if additionnal verification with PIN (locally administered) must be done.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool PinRequired
        {
            get { return _pinrequired; }
            set
            {
                _pinrequired = value;
                PinRequiredChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Set if additionnal verification with PIN when user is not verified.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public PSWebAuthNPinRequirements PinRequirements
        {
            get { return _pinrequirements; }
            set
            {
                _pinrequirements = value;
                PinRequirementsChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Full qualified implementation class for replacing default provider.</para>
        /// </summary>       
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string FullyQualifiedImplementation
        {
            get { return _fullyqualifiedimplementation; }
            set
            {
                _fullyqualifiedimplementation = value;
                FullyQualifiedImplementationChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Full qualified implementation parameters for replacing default provider.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Parameters
        {
            get { return _parameters; }
            set
            {
                _parameters = value;
                ParametersChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Timeout property (in milliseconds).</para> 
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateRange(10000, 600000)]
        public uint Timeout
        {
            get { return _timeout; }
            set
            {
                _timeout = value;
                TimeoutChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Timestamp Drift Tolerance property (in miliseconds).</para> 
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateRange(0, 600000)]
        public int TimestampDriftTolerance
        {
            get { return _timetolerance; }
            set
            {
                _timetolerance = value;
                TimeToleranceChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">When Biometrics is default method, authentication directly called a first time</para> 
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool DirectLogin
        {
            get { return _directlogin; }
            set
            {
                _directlogin = value;
                DirectLoginChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Server Domain property.</para> 
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string ServerDomain
        {
            get { return _serverdomain; }
            set
            {
                _serverdomain = value;
                ServerDomainChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Server Name property.</para> 
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string ServerName
        {
            get { return _servername; }
            set
            {
                _servername = value;
                ServerNameChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Server Uri property (url).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Origin
        {
            get { return _url; }
            set
            {
                _url = value;
                OriginChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Challenge Size property (16, 32, 48, 64 bytes) (128, 256, 384, 512 bits).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateSet("16", "32", "48","64")]
        public int ChallengeSize
        {
            get { return _challengsize; }
            set
            {
                _challengsize = value;
                ChallengeSizeChanged = true;
            }
        }
    }
    #endregion

    #region Get-MFASecurity
    /// <summary>
    /// <para type="synopsis">Get MFA Security Configuration.</para>
    /// <para type="description">Get MFA Security configuration options.</para>
    /// </summary>
    /// <example>
    ///   <para>Get-MFASecurity</para>
    /// </example>
    /// <example>
    ///  <para>Get-MFASecurity -Kind RSA</para>
    ///  <para>$c = Get-MFASecurity -Kind BIOMETRIC</para>
    /// </example>
    /// <example>
    ///   <para>$c = Get-MFASecurity -Kind RNG</para>
    ///   <para>$c.KeyGenerator = [MFA.PSKeyGeneratorMode]::ClientSecret256</para>
    ///   <para>Set-MFASecurity -Kind RNG $c</para>
    /// </example>
    /// <example>
    ///   <para>$c = Get-MFASecurity -Kind AES</para>
    ///   <para>$c.AESKeyGenerator = [MFA.PSAESKeyGeneratorMode]::AESSecret1024</para>
    ///   <para>Set-MFASecurity -Kind AES $c</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "MFASecurity", SupportsShouldProcess = true, SupportsPaging = false, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [OutputType(typeof(PSSecurity), typeof(PSRNGSecurity), typeof(PSRSASecurity), typeof(PSBiometricSecurity))]
    public sealed class GetMFASecurity : MFACmdlet
    {
        private PSBaseSecurity _config;
        private PSRNGSecurity _config0;
        private PSRSASecurity _config1;
        private PSBiometricSecurity _config2;
        private PSWsManSecurity _config3;
        private PSAESSecurity _config4;
        private PSCustomSecurity _config5;
        private PSSecurityMode _securitymode = PSSecurityMode.RNG;
        internal bool SecurityModeChanged { get; private set; } = false;

        /// <summary>
        /// <para type="description">Provider Type parameter, (RNG, RSA, CUSTOM, WSMAN).</para>
        /// </summary>
        [Parameter(Position = 0, ParameterSetName = "Data", ValueFromPipeline = true)]
        [ValidateRange(PSSecurityMode.RNG, PSSecurityMode.WSMAN)]
        public PSSecurityMode Kind
        {
            get
            {
                return _securitymode;
            }
            set
            {
                _securitymode = value;
                SecurityModeChanged = true;
            }
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                if (!SecurityModeChanged)
                {
                    FlatSecurity cf = new FlatSecurity();
                    cf.Load(this.Host);
                    _config = (PSSecurity)cf;
                }
                else
                {
                    switch (Kind)
                    {
                        case PSSecurityMode.RNG:
                            FlatRNGSecurity cf0 = new FlatRNGSecurity();
                            cf0.Load(this.Host);
                            _config0 = (PSRNGSecurity)cf0;
                            break;
                        case PSSecurityMode.RSA:
                            FlatRSASecurity cf1 = new FlatRSASecurity();
                            cf1.Load(this.Host);
                            _config1 = (PSRSASecurity)cf1;
                            break;
                        case PSSecurityMode.AES:
                            FlatAESSecurity cf4 = new FlatAESSecurity();
                            cf4.Load(this.Host);
                            _config4 = (PSAESSecurity)cf4;
                            break;
                        case PSSecurityMode.CUSTOM:
                            FlatCustomSecurity cf5 = new FlatCustomSecurity();
                            cf5.Load(this.Host);
                            _config5 = (PSCustomSecurity)cf5;
                            break;

                        case PSSecurityMode.BIOMETRIC:
                            FlatBiometricSecurity cf2 = new FlatBiometricSecurity();
                            cf2.Load(this.Host);
                            _config2 = (PSBiometricSecurity)cf2;
                            break;
                        case PSSecurityMode.WSMAN:
                            FlatWsManSecurity cf3 = new FlatWsManSecurity();
                            cf3.Load(this.Host);
                            _config3 = (PSWsManSecurity)cf3;
                            break;

                    }
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3021", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessInternalRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (ShouldProcess("MFA Security Configuration"))
                {
                    if (!SecurityModeChanged)
                        WriteObject(_config);
                    else
                    {
                        switch (Kind)
                        {
                            case PSSecurityMode.RNG:
                                WriteObject(_config0);
                                break;
                            case PSSecurityMode.RSA:
                                WriteObject(_config1);
                                break;
                            case PSSecurityMode.AES:
                                WriteObject(_config4);
                                break;
                            case PSSecurityMode.CUSTOM:
                                WriteObject(_config5);
                                break;
                            case PSSecurityMode.BIOMETRIC:
                                WriteObject(_config2);
                                break;
                            case PSSecurityMode.WSMAN:
                                WriteObject(_config3);
                                break;
                        }
                    }
                        
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
            _config5 = null;
            _config4 = null;
            _config3 = null;
            _config2 = null;
            _config1 = null;
            _config0 = null;
            _config = null;
            base.StopProcessing();
        }

        /// <summary>
        /// EndProcessing method implementation
        /// </summary>
        protected override void EndProcessing()
        {
            _config5 = null;
            _config4 = null;
            _config3 = null;
            _config2 = null;
            _config1 = null;
            _config0 = null;
            _config = null;
            base.EndProcessing();
        }

    }
    #endregion

    #region Set-MFASecurity
    /// <summary>
    /// <para type="synopsis">Set Secret Keys Configuration.</para>
    /// <para type="description">Set Secret Keys configuration options.</para>
    /// </summary>
    /// <example>
    ///   <para>$cfg = Get-MFASecurity -Kind RSA</para>
    ///   <para>$cfg.CertificatePerUser = $true</para>
    ///   <para>Set-MFASecurity -Kind RSA $cfg</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "MFASecurity", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired]
    public sealed class SetMFASecurity : MFACmdlet, IDynamicParameters
    {
        private PSBaseSecurity _config;

        private SECRNGDynamicParameters _config1;
        private SECRSADynamicParameters _config2;
        private SECBIOMETRICDynamicParameters _config3;
        private SECAESDynamicParameters _config4;
        private SECCUSTOMDynamicParameters _config5;
        private SECWSMANDynamicParameters _config6;

        private FlatRNGSecurity _target1;
        private FlatRSASecurity _target2;
        private FlatBiometricSecurity _target3;
        private FlatAESSecurity _target4;
        private FlatCustomSecurity _target5;
        private FlatWsManSecurity _target6;

        private PSSecurityMode _securitymode = PSSecurityMode.RNG;
        internal bool SecurityModeChanged { get; private set; } = false;


        /// <summary>
        /// <para type="description">Provider Type parameter, (RNG, RSA, BIOMETRIC, CUSTOM, WSMAN).</para>
        /// </summary>
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "Item", ValueFromPipeline = true)]
        [ValidateRange(PSSecurityMode.RNG, PSSecurityMode.WSMAN)]
        public PSSecurityMode Kind
        {
            get
            {
                return _securitymode;
            }
            set
            {
                _securitymode = value;
                SecurityModeChanged = true;
            } 
        }


        /// <summary>
        /// <para type="description">Config parameter, a variable of type PSSecurity.</para>
        /// </summary>
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "Data", ValueFromPipeline = true, DontShow = true)]
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = "Item", ValueFromPipeline = true, DontShow = true)]
        [ValidateNotNullOrEmpty()]
        public PSBaseSecurity Config
        {
            get { return _config; }
            set {_config = value; }
        }

        /// <summary>
        /// <para type="description">Set the value of Provider configuration.</para>
        /// GetDynamicParameters implementation
        /// </summary>
        public object GetDynamicParameters()
        {
            if (SecurityModeChanged)
            {
                switch (Kind)
                {
                    case PSSecurityMode.RNG:
                        _config1 = new SECRNGDynamicParameters();
                        return _config1;
                    case PSSecurityMode.RSA:
                        _config2 = new SECRSADynamicParameters();
                        return _config2;
                    case PSSecurityMode.BIOMETRIC:
                        _config3 = new SECBIOMETRICDynamicParameters();
                        return _config3;
                    case PSSecurityMode.AES:
                        _config4 = new SECAESDynamicParameters();  
                        return _config4;
                    case PSSecurityMode.CUSTOM:
                        _config5 = new SECCUSTOMDynamicParameters();  
                        return _config5;
                    case PSSecurityMode.WSMAN:
                        _config6 = new SECWSMANDynamicParameters();
                        return _config5;
                    default:
                        break;
                }
            }
            return null;
        }

        /// <summary>
        /// BeginProcessing method override
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                if (SecurityModeChanged)
                {
                    ManagementService.Initialize(this.Host, true);
                    switch (Kind)
                    {
                        case PSSecurityMode.RNG:
                            _target1 = new FlatRNGSecurity();
                            _target1.Load(this.Host);
                            if (_config1.KeyGeneratorChanged)
                                _target1.KeyGenerator = (KeyGeneratorMode)_config1.KeyGenerator;
                            break;
                        case PSSecurityMode.RSA:
                            _target2 = new FlatRSASecurity();
                            _target2.Load(this.Host);
                            if (_config2.CertificatePerUserChanged)
                                _target2.CertificatePerUser = _config2.CertificatePerUser;
                            if (_config2.CertificateThumbprintChanged)
                                _target2.CertificateThumbprint = _config2.CertificateThumbprint;
                            if (_config2.CertificateValidityChanged)
                                _target2.CertificateValidity= _config2.CertificateValidity;
                            break;
                        case PSSecurityMode.AES:
                            _target4 = new FlatAESSecurity();
                            _target4.Load(this.Host);
                            if (_config4.KeyGeneratorChanged)
                                _target4.AESKeyGenerator = (AESKeyGeneratorMode)_config4.AESKeyGenerator;
                            break;
                        case PSSecurityMode.CUSTOM: 
                            _target5 = new FlatCustomSecurity();
                            _target5.Load(this.Host);
                            if (_config5.ImplementationChanged)
                                _target5.CustomFullyQualifiedImplementation = _config5.FullyQualifiedImplementation;
                            if (_config5.ParametersChanged)
                                _target5.CustomParameters = _config5.Parameters;
                            break;
                        case PSSecurityMode.BIOMETRIC:
                            _target3 = new FlatBiometricSecurity();
                            _target3.Load(this.Host);
                            if (_config3.AttestationConveyancePreferenceChanged)
                                _target3.AttestationConveyancePreference = (FlatAttestationConveyancePreferenceKind)_config3.AttestationConveyancePreference;
                            if (_config3.AuthenticatorAttachmentChanged)
                                _target3.AuthenticatorAttachment = (FlatAuthenticatorAttachmentKind)_config3.AuthenticatorAttachment;
                            if (_config3.UserVerificationRequirementChanged)
                                _target3.UserVerificationRequirement = (FlatUserVerificationRequirementKind)_config3.UserVerificationRequirement;
                            if (_config3.ExtensionsChanged)
                                _target3.Extensions = _config3.Extensions;
                            if (_config3.LocationChanged)
                                _target3.Location = _config3.Location;
                            if (_config3.RequireResidentKeyChanged)
                                _target3.RequireResidentKey = _config3.RequireResidentKey;
                            if (_config3.UserVerificationIndexChanged)
                                _target3.UserVerificationIndex = _config3.UserVerificationIndex;
                            if (_config3.UserVerificationMethodChanged)
                                _target3.UserVerificationMethod = _config3.UserVerificationMethod;
                            if (_config3.CredProtectChanged)
                                _target3.CredProtect = _config3.CredProtect;
                            if (_config3.EnforceCredProtectChanged)
                                _target3.EnforceCredProtect = _config3.EnforceCredProtect;
                            if (_config3.HmacSecretChanged)
                                _target3.HmacSecret = _config3.HmacSecret;
                            break;
                        case PSSecurityMode.WSMAN:
                            _target6 = new FlatWsManSecurity();
                            _target6.Load(this.Host);
                            if (_config6.PortChanged)
                                _target6.Port = _config6.Port;
                            if (_config6.AppNameChanged)
                                _target6.AppName = _config6.AppName;
                            if (_config6.ShellUriChanged)
                                _target6.ShellUri = _config6.ShellUri;
                            if (_config6.TimeOutChanged)
                                _target6.TimeOut = _config6.TimeOut;
                            break;
                    }
                }
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
            const string error = "Invalid Security Type !";
            if (ShouldProcess("MFA Providers Configuration"))
            {
                try
                {
                    if (SecurityModeChanged)
                    {
                        switch (Kind)
                        {
                            case PSSecurityMode.RNG:
                                if (Config is PSRNGSecurity)
                                    ((FlatRNGSecurity)((PSRNGSecurity)Config)).Update(this.Host);
                                else
                                    throw new Exception(error);
                                break;
                            case PSSecurityMode.RSA:
                                if (Config is PSRSASecurity)
                                    ((FlatRSASecurity)((PSRSASecurity)Config)).Update(this.Host);
                                else
                                    throw new Exception(error);
                                break;
                            case PSSecurityMode.AES:
                                if (Config is PSAESSecurity)
                                    ((FlatAESSecurity)((PSAESSecurity)Config)).Update(this.Host);
                                else
                                    throw new Exception(error);
                                break;
                            case PSSecurityMode.CUSTOM:
                                if (Config is PSCustomSecurity)
                                    ((FlatCustomSecurity)((PSCustomSecurity)Config)).Update(this.Host);
                                else
                                    throw new Exception(error);
                                break;
                            case PSSecurityMode.BIOMETRIC:
                                if (Config is PSBiometricSecurity)
                                    ((FlatBiometricSecurity)((PSBiometricSecurity)Config)).Update(this.Host);
                                else
                                    throw new Exception(error);

                                break;
                            case PSSecurityMode.WSMAN:
                                if (Config is PSWsManSecurity)
                                    ((FlatWsManSecurity)((PSWsManSecurity)Config)).Update(this.Host);
                                else
                                    throw new Exception(error);
                                break;
                            default:
                                throw new Exception(error);
                        }
                    }
                    else
                    {
                        ((FlatSecurity)((PSSecurity)Config)).Update(this.Host);
                    }
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
    /// SECWSMANDynamicParameters class implementation
    /// </summary>
    internal class SECWSMANDynamicParameters
    {
        private int _port;
        private string _appname;
        private string _shelluri;
        private int _timeout;

        internal bool PortChanged { get; private set; }
        internal bool AppNameChanged { get; private set; }
        internal bool ShellUriChanged { get; private set; }
        internal bool TimeOutChanged { get; private set; }

        /// <summary>
        /// <para type="description">WsMan port used (5985(http), 5986(https)).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                PortChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">WsMan Application name used (default (wsman)).</para>
        /// </summary>        
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string AppName
        {
            get { return _appname; }
            set
            {
                _appname = value;
                AppNameChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">WsMan Shell Uri.</para>
        /// </summary>        
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string ShellUri
        {
            get { return _shelluri; }
            set
            {
                _shelluri = value;
                ShellUriChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">WsMan Connection TimeOut (5000 ms).</para>
        /// </summary>        
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public int TimeOut
        {
            get { return _timeout; }
            set
            {
                _timeout = value;
                TimeOutChanged = true;
            }
        }
    }

    /// <summary>
    /// SECBIOMETRICDynamicParameters class implementation
    /// </summary>
    internal class SECBIOMETRICDynamicParameters
    {
        private PSAuthenticatorAttachmentKind _authenticatorattachment;
        private PSAttestationConveyancePreferenceKind _attestationconveyancepreference;
        private PSUserVerificationRequirementKind _userverificationrequirement;
        private bool _extensions;
        private bool _userverificationindex;
        private bool _location;
        private bool _userverificationmethod;
        private bool _requireresidentkey;
        private bool? _hmacsecret;
        private WebAuthNUserVerification? _credprotect;
        private bool? _enforcecredprotect;

        internal bool RequireResidentKeyChanged { get; private set; }
        internal bool UserVerificationMethodChanged { get; private set; }
        internal bool LocationChanged { get; private set; }
        internal bool UserVerificationIndexChanged { get; private set; }
        internal bool ExtensionsChanged { get; private set; }
        internal bool UserVerificationRequirementChanged { get; private set; }
        internal bool AttestationConveyancePreferenceChanged { get; private set; }
        internal bool AuthenticatorAttachmentChanged { get; private set; }
        internal bool HmacSecretChanged { get; private set; }
        internal bool CredProtectChanged { get; private set; }
        internal bool EnforceCredProtectChanged { get; private set; }

        /// <summary>
        /// <para type="description">Authenticator Attachment property (empty, Platform, Crossplatform).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public PSAuthenticatorAttachmentKind AuthenticatorAttachment
        {
            get { return _authenticatorattachment; }
            set
            {
                _authenticatorattachment = value;
                AuthenticatorAttachmentChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Attestation Conveyance Preference property (None, Direct, Indirect).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public PSAttestationConveyancePreferenceKind AttestationConveyancePreference
        {
            get { return _attestationconveyancepreference; }
            set
            {
                _attestationconveyancepreference = value;
                AttestationConveyancePreferenceChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">User Verification Requirement property (Preferred, Required, Discouraged).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public PSUserVerificationRequirementKind UserVerificationRequirement
        {
            get { return _userverificationrequirement; }
            set
            {
                _userverificationrequirement = value;
                UserVerificationRequirementChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Extensions property (boolean) supports extensions ?.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool Extensions
        {
            get { return _extensions; }
            set
            {
                _extensions = value;
                ExtensionsChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">User Verification Index property (boolean).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool UserVerificationIndex
        {
            get { return _userverificationindex; }
            set
            {
                _userverificationindex = value;
                UserVerificationIndexChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Location property (boolean).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool Location 
        {
            get { return _location; }
            set
            {
                _location = value;
                LocationChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">User Verification Method property (boolean).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool UserVerificationMethod
        {
            get { return _userverificationmethod; }
            set
            {
                _userverificationmethod = value;
                UserVerificationMethodChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Require Resident Key property (boolean).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool RequireResidentKey
        {
            get { return _requireresidentkey; }
            set
            {
                _requireresidentkey = value;
                RequireResidentKeyChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Use HMAC enryption (CATP2.1).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public bool? HmacSecret
        {
            get { return _hmacsecret; }
            set
            {
                _hmacsecret = value;
                HmacSecretChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Use Credential Protection (CATP2.1).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public WebAuthNUserVerification? CredProtect
        {
            get { return _credprotect; }
            set
            {
                _credprotect = value;
                CredProtectChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Enforce Credential Protection (CATP2.1).</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public bool? EnforceCredProtect
        {
            get { return _enforcecredprotect; }
            set
            {
                _enforcecredprotect = value;
                EnforceCredProtectChanged = true;
            }
        }
    }

    /// <summary>
    /// SECRSADynamicParameters class implementation
    /// </summary>
    internal class SECRSADynamicParameters
    {
        private int _certvalidity;
        private bool _certperuser;
        private string _thumbprint;

        internal bool CertificateThumbprintChanged { get; private set; }
        internal bool CertificatePerUserChanged { get; private set; }
        internal bool CertificateValidityChanged { get; private set; }

        /// <summary>
        /// <para type="description">Certificate validity duration in Years (5 by default)</para> 
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public int CertificateValidity
        {
            get { return _certvalidity; }
            set
            {
                _certvalidity = value;
                CertificateValidityChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Use a distinct certificate for each user when using KeyFormat==RSA. each certificate is deployed on ADDS or SQL Database</para> 
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public bool CertificatePerUser
        {
            get { return _certperuser; }
            set
            {
                _certperuser = value;
                CertificatePerUserChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Certificate Thumbprint when using KeyFormat==RSA. the certificate is deployed on all ADFS servers in Crypting Certificates store</para> 
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string CertificateThumbprint
        {
            get { return _thumbprint; }
            set
            {
                _thumbprint = value;
                CertificateThumbprintChanged = true;
            }
        }
    }

    /// <summary>
    /// SECRNGDynamicParameters class implementation
    /// </summary>
    internal class SECRNGDynamicParameters
    {
        private PSKeyGeneratorMode _keygenerator;
        internal bool KeyGeneratorChanged { get; private set; }

        /// <summary>
        /// <para type="description">Used when RNG is selected, for choosing the size of the generated random number (128 to 512 bits).</para> 
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public PSKeyGeneratorMode KeyGenerator
        {
            get { return _keygenerator; }
            set
            {
                _keygenerator = value;
                KeyGeneratorChanged = true;
            }
        }
    }

    /// <summary>
    /// SECAESDynamicParameters class implementation
    /// </summary>
    internal class SECAESDynamicParameters
    {
        private PSAESKeyGeneratorMode _keygenerator;
        internal bool KeyGeneratorChanged { get; private set; }

        /// <summary>
        /// <para type="description">Used when RNG is selected, for choosing the size of the generated random number (512 to 1024 bits).</para> 
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public PSAESKeyGeneratorMode AESKeyGenerator
        {
            get { return _keygenerator; }
            set
            {
                _keygenerator = value;
                KeyGeneratorChanged = true;
            }
        }
    }

    /// <summary>
    /// SECCUSTOMDynamicParameters class implementation
    /// </summary>
    internal class SECCUSTOMDynamicParameters
    {
        private string _implementation;
        private string _parameters;
        internal bool ParametersChanged { get; private set; }

        internal bool ImplementationChanged { get; private set; }

        /// <summary>
        /// <para type="description">Used when CUSTOM is selected.</para> 
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string FullyQualifiedImplementation
        {
            get { return _implementation; }
            set
            {
                _implementation = value;
                ImplementationChanged = true;
            }
        }

        /// <summary>
        /// <para type="description">Full qualified implementation parameters for Custom Security.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string Parameters
        {
            get { return _parameters; }
            set
            {
                _parameters = value;
                ParametersChanged = true;
            }
        }
    }

    #endregion

    #region Set-MFAPolicyTemplate
    /// <summary>
    /// <para type="synopsis">Set Policy Template.</para>
    /// <para type="description">Set Policy template, for managing security features like ChangePassword, Auto-registration and more.</para>
    /// </summary>
    /// <example>
    ///   <para>Set-MFAPolicyTemplate -Template Strict</para>
    ///   <para>Change policy template for MFA configuration</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "MFAPolicyTemplate", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired]
    public sealed class SetMFATemplateMode : MFACmdlet
    {
        private PSTemplateMode _template;
        private FlatTemplateMode _target;
        private FlatConfig _config;

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
                ManagementService.Initialize(this.Host, true);
                _config = new FlatConfig();
                _config.Load(this.Host);
                _target = (FlatTemplateMode)_template;
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
                    _config.SetPolicyTemplate(this.Host, _target);
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
    #endregion

    #region Set-MFAThemeMode
    /// <summary>
    /// <para type="synopsis">Set ADFS Theme.</para>
    /// <para type="description">Set ADFS Theme.</para>
    /// </summary>
    /// <example>
    ///   <para>Set-MFAThemeMode -UIKind Default -Theme yourcompatibletheme</para>
    ///   <para>Set-MFAThemeMode -UIKind Default2019 -Theme yourcompatibletheme</para>
    ///   <para>Set-MFAThemeMode -UIKind Default2019 -Theme yourcompatibletheme -Paginated</para>
    ///   <para>Change policy template for MFA configuration</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "MFAThemeMode", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired]
    public sealed class SetMFAThemeMode : MFACmdlet, IDynamicParameters
    {
        private PSUIKind _kind = PSUIKind.Default;
        private string _theme;
        private FlatConfig _config;
        private MFAThemeModeDynamicParameters _dynparam;

        /// <summary>
        /// <para type="description">ADFS UI Mode.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data", ValueFromPipeline = false)]
        [ValidateNotNullOrEmpty()]
        public PSUIKind UIKind
        {
            get { return _kind; }
            set { _kind = value; }
        }

        /// <summary>
        /// <para type="description">ADFS Theme Name.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Data", ValueFromPipeline = false)]
        [ValidateNotNullOrEmpty()]
        public string Theme
        {
            get { return _theme; }
            set { _theme = value; }
        }

        /// <summary>
        /// <para type="description">Set UI Themes 2019 configuration.</para>
        /// GetDynamicParameters implementation
        /// </summary>
        public object GetDynamicParameters()
        {
            if (_kind == PSUIKind.Default2019)
            {
                _dynparam = new MFAThemeModeDynamicParameters();
                return _dynparam;
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
                ManagementService.Initialize(this.Host, true);
                _config = new FlatConfig();
                _config.Load(this.Host);
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
            if (UIKind==PSUIKind.Default2019)
                this.WriteWarning(string.Format(infos_strings.InfosWarningAboutTheme, "ADFS 2019"));
            else
                this.WriteWarning(string.Format(infos_strings.InfosWarningAboutTheme, "ADFS 2019/2016/2012r2"));
            if (ShouldProcess("MFA Theme Configuration"))
            {
                try
                {
                    if (_dynparam != null)
                        _config.SetTheme(this.Host, (int)_kind, _theme, _dynparam.Paginated);
                    else
                        _config.SetTheme(this.Host, (int)_kind, _theme, false);
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
    /// <para type="description">Set MFA Themes Options for Ui 2019.</para>
    /// </summary>
    public class MFAThemeModeDynamicParameters
    {
        private bool _usepaginated = false;

        /// <summary>
        /// <para type="description">Set the value for ADFS 2019 paginated mode.</para>
        /// Paginated property
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Paginated
        {
            get { return _usepaginated; }
            set { _usepaginated = value; }
        }
    }
    #endregion

    #region Reset-MFAThemesList
    /// <summary>
    /// <para type="synopsis">Reset ADFS Relying Parties Themes for MFA (Reload).</para>
    /// <para type="description">Force Reload for ADFS Relying Parties Themes for MFA.</para>
    /// </summary>
    /// <example>
    ///   <para>Reset-MFAThemesList</para>
    /// </example>
    [Cmdlet(VerbsCommon.Reset, "MFAThemesList", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired]
    public sealed class ResetMFAThemesList : MFACmdlet
    {
        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, true);
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
            if (ShouldProcess("MFA Reset/Reload Relying Parties Themes"))
            {
                try
                {
                    ManagementService.ResetWebThemesList(this.Host);
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
            base.StopProcessing();
        }
    }
    #endregion

    #region Set-MFAEncryptionVersion
    /// <summary>
    /// <para type="synopsis">Set ADFS Theme.</para>
    /// <para type="description">Set ADFS Theme.</para>
    /// </summary>
    /// <example>
    ///   <para>Set-MFAEncryptionVersion -Version V2</para>
    ///   <para>Change encryption Librairy Version for MFA configuration</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "MFAEncryptionVersion", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired, AdministratorsRightsRequired]
    public sealed class SetMFAEncryptionVersion : MFACmdlet
    {
        private PSSecretKeyVersion _version = PSSecretKeyVersion.V2;
        private FlatConfig _config;

        /// <summary>
        /// <para type="description">ADFS UI Mode.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data", ValueFromPipeline = false)]
        [ValidateNotNullOrEmpty()]
        public PSSecretKeyVersion Version
        {
            get { return _version; }
            set { _version = value; }
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, true);
                _config = new FlatConfig();
                _config.Load(this.Host);
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
            if (ShouldProcess("MFA Encryption Librairy Configuration"))
            {
                try
                {
                    _config.SetLibraryVersion(this.Host, (int)_version);
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
    #endregion

    #region Set-MFAPrimaryAuthenticationStatus
    /// <summary>
    /// <para type="synopsis">Set MFA Primary Authentication Status enabled/disabled (ADFS 2019 only).</para>
    /// <para type="description">Set MFA Primary Authentication Status.</para>
    /// </summary>
    /// <example>
    ///   <para>MFAPrimaryAuthenticationStatus $true</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "MFAPrimaryAuthenticationStatus", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired, ADFS2019Required, AdministratorsRightsRequired]
    public sealed class SetMFAPrimaryAuthenticationStatus : MFACmdlet
    {
        private FlatConfig _config;

        /// <summary>
        /// <para type="description">Set Primary Authentication Status.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data", ValueFromPipeline = false)]
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// <para type="description">Set Primary Authentication Status Options.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = "Data", ValueFromPipeline = false)]
        public PrimaryAuthOptions Options { get; set; } = PrimaryAuthOptions.None;

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, true);
                _config = new FlatConfig();
                _config.Load(this.Host);
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
            if (ShouldProcess("MFA Primary Authentication Status Configuration"))
            {
                try
                {
                    _config.SetPrimaryAuthenticationStatus(this.Host, Enabled, Options);
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
    #endregion

    #region Register-MFAPrimaryAuthentication
    /// <summary>
    /// <para type="synopsis">Register MFA threat detection system.</para>
    /// <para type="description">Register MFA threat detection system. Activate it</para>
    /// </summary>
    /// <example>
    ///   <para>Register-MFAThreatDetectionSystem</para>
    ///   <para>Only available for ADFS 2019 and Up.</para>
    /// </example>
    [Cmdlet(VerbsLifecycle.Register, "MFAThreatDetectionSystem", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired, ADFS2019Required, AdministratorsRightsRequired]
    public sealed class RegisterMFAThreatDetectionSystem : MFACmdlet
    {
        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, true);
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
                if (ShouldProcess("Register-MFAThreatDetectionSystem", "Register MFA Threat Detection module for ADFS"))
                {
                    PSHost hh = GetHostForVerbose();
                    if (ManagementService.RegisterMFAThreatDetectionSystem(hh))
                        this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosThreatSystemRegistered));
                    else
                        this.Host.UI.WriteLine(ConsoleColor.Yellow, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosThreatSystemNotRegistered));
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3012", ErrorCategory.OperationStopped, this));
            }
        }
    }
    #endregion

    #region UnRegister-MFAThreatDetectionSystem
    /// <summary>
    /// <para type="synopsis">UnRegister MFA threat detection system.</para>
    /// <para type="description">UnRegister MFA threat detection system. DeActivate it</para>
    /// </summary>
    /// <example>
    ///   <para>UnRegister-MFAThreatDetectionSystem</para>
    ///   <para>Only available for ADFS 2019 and Up.</para>    /// </example>
    [Cmdlet(VerbsLifecycle.Unregister, "MFAThreatDetectionSystem", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired, ADFS2019Required, AdministratorsRightsRequired]
    public sealed class UnRegisterMFAThreatDetectionSystem : MFACmdlet
    {
        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, true);
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
                if (ShouldProcess("UnRegister-MFAThreatDetectionSystem", "UnRegister MFA Threat Detection module for ADFS"))
                {
                    PSHost hh = GetHostForVerbose();
                    if (ManagementService.UnRegisterMFAThreatDetectionSystem(hh))
                        this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosThreatSystemUnRegistered));
                    else
                        this.Host.UI.WriteLine(ConsoleColor.Yellow, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosThreatSystemNotUnRegistered));
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3014", ErrorCategory.OperationStopped, this));
            }
        }
    }
    #endregion

    #region Update-MFAThreatDetectionData
    /// <summary>
    /// <para type="synopsis">Update MFA threat detection data.</para>
    /// <para type="description">Update MFA threat detection data.</para>
    /// </summary>
    /// <example>
    ///   <para>Update-MFAThreatDetectionData</para>
    ///   <para>Only available for ADFS 2019 and Up.</para>    /// </example>
    [Cmdlet(VerbsData.Update, "MFAThreatDetectionData", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired, ADFS2019Required, AdministratorsRightsRequired]
    public sealed class UpdateMFAThreatDetectionData : MFACmdlet
    {
        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, true);
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
                if (ShouldProcess("Update-MFAThreatDetectionData", "Update MFA Threat Detection data for ADFS"))
                {
                    PSHost hh = GetHostForVerbose();
                    if (ManagementService.UpdateMFAThreatDetectionData(hh))
                        this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosThreatDataUpdated));
                    else
                        this.Host.UI.WriteLine(ConsoleColor.Yellow, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosThreatDataNotUpdated));
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3014", ErrorCategory.OperationStopped, this));
            }
        }
    }
    #endregion

    #region New-MFADatabase
    /// <summary>
    /// <para type="synopsis">Create a new SQL Database for MFA.</para>
    /// <para type="description">Create a new SQL Database for MFA Configuration (UseActiveDirectory==false).</para>
    /// </summary>
    /// <example>
    ///   <para>New-MFADatabase -ServerName SQLServer\Instance -DatabaseName MFADatabase -UserName sqlaccount -Password pass</para>
    ///   <para>New-MFADatabase -ServerName SQLServer\Instance -DatabaseName MFADatabase -UserName Domain\ADFSaccount</para>
    ///   <para>New-MFADatabase -ServerName SQLServer\Instance -DatabaseName MFADatabase -UserName Domain\ADFSManagedAccount$</para>
    ///   <para></para>
    ///   <para>When using SQL Server 2016 (and up) Always encrypted columsn</para>
    ///   <para>New-MFADatabase -ServerName SQLServer\Instance -DatabaseName MFAKeysDatabase -UserName Domain\ADFSaccount -Encrypted</para>
    ///   <para>New-MFADatabase -ServerName SQLServer\Instance -DatabaseName MFAKeysDatabase -UserName Domain\ADFSaccount -Encrypted -EncryptedKeyName mykey</para>
    ///   <para>New-MFADatabase -ServerName SQLServer\Instance -DatabaseName MFAKeysDatabase -UserName Domain\ADFSManagedAccount$ -Encrypted -ReuseCertificate -ThumbPrint 0123456789ABCDEF...</para>
    ///   <para>New-MFADatabase -ServerName SQLServer\Instance -DatabaseName MFAKeysDatabase -UserName Domain\ADFSManagedAccount$ -Encrypted -ReuseCertificate -ThumbPrint 0123456789ABCDEF... -EncryptedKeyName mykey</para>
    ///   <para>Create a new database for MFA, grant rights to the specified account</para>
    /// </example>
    [Cmdlet(VerbsCommon.New, "MFADatabase", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired, AdministratorsRightsRequired]
    public sealed class NewMFADatabase : MFACmdlet, IDynamicParameters
    {
        private string _servername;
        private string _databasename;
        private string _username;
        private string _password;
        private SQLEncryptedDatabaseDynamicParameters Dyn;
        private PSSQLStore _config;

        /// <summary>
        /// <para type="description">SQL ServerName, you must include Instance if needed eg : server\instance.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Data")]
        [ValidateNotNullOrEmpty()]
        public string ServerName
        {
            get { return _servername; }
            set { _servername = value; }
        }

        /// <summary>
        /// <para type="description">Name of the SQL Database, if database exists an error is thrown.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Data")]
        [ValidateNotNullOrEmpty()]
        public string DatabaseName
        {
            get { return _databasename; }
            set { _databasename = value; }
        }

        /// <summary>
        /// <para type="description">AccountName, can be a domain, managed account : domain\adfsaccount or domain\adfsaccount$ without password, or an SQL Account with password.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Data")]
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
        /// <para type="description">Encrypted, only if Database Server is SQLServer 2016 and Up. Encrypt data columns to be compliant with RGPD. most secure option</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter Encrypted
        {
            get;
            set;
        }

        /// <summary>
        /// <para type="description">Get the values of SQL Database Encryption.</para>
        /// GetDynamicParameters implementation
        /// </summary>
        public object GetDynamicParameters()
        {
            if (Encrypted)
            {
                Dyn = new SQLEncryptedDatabaseDynamicParameters();
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
                ManagementService.Initialize(this.Host, true);
                FlatSQLStore cf = new FlatSQLStore();
                cf.Load(this.Host);
                _config = (PSSQLStore)cf;
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
                if (ShouldProcess("MFA SQL Database creation (must be sysadmin or dbcreator and securityadmin)"))
                {
                    PSHost hh = GetHostForVerbose();
                    ADFSServiceManager svc = ManagementService.ADFSManager;
                    if (!Encrypted)
                    {
                        svc.CreateMFADatabase(hh, ServerName, DatabaseName, UserName, Password);
                        this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, string.Format(infos_strings.InfosDatabaseCreated, DatabaseName));
                    }
                    else
                    {
                        string thumb = string.Empty;
                        if (string.IsNullOrEmpty(Dyn.ThumbPrint))
                        {
                            thumb = svc.RegisterNewSQLCertificate(hh, Dyn.CertificateDuration, Dyn.EncryptKeyName);
                            this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosSQLCertificateChanged, thumb));
                        }
                        else
                            thumb = Dyn.ThumbPrint;
                        svc.CreateMFAEncryptedDatabase(hh, ServerName, DatabaseName, UserName, Password, Dyn.EncryptKeyName, thumb);
                        this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, string.Format(infos_strings.InfosDatabaseCreated, DatabaseName));
                    }
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3022", ErrorCategory.OperationStopped, this));
            }
        }
    }
    #endregion

    #region Upgrade-MFADatabase
    /// <summary>
    /// <para type="synopsis">Create a new SQL Database for MFA.</para>
    /// <para type="description">Create a new SQL Database for MFA Configuration (UseActiveDirectory==false).</para>
    /// </summary>
    /// <example>
    ///   <para>Upgrade-MFADatabase -ServerName SQLServer\Instance -DatabaseName MFADatabase</para>
    ///   <para></para>
    ///   <para>Upgrade an existing database for MFA with encryted columns or not, using rights to the specified account</para>
    /// </example>
    [Cmdlet("Upgrade", "MFADatabase", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired]
    public sealed class UpgradeMFADatabase : MFACmdlet
    {
        private PSSQLStore _config;

        /// <summary>
        /// <para type="description">SQL ServerName, you must include Instance if needed eg : server\instance.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Data")]
        [ValidateNotNullOrEmpty()]
        public string ServerName { get; set; }

        /// <summary>
        /// <para type="description">Name of the SQL Database, if database exists an error is thrown.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Data")]
        [ValidateNotNullOrEmpty()]
        public string DatabaseName { get; set; }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, true);
                FlatSQLStore cf = new FlatSQLStore();
                cf.Load(this.Host);
                _config = (PSSQLStore)cf;
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
                if (ShouldProcess("MFA SQL Database upgrade (must be sysadmin or dbcreator and securityadmin)"))
                {
                    PSHost hh = GetHostForVerbose();
                    ADFSServiceManager svc = ManagementService.ADFSManager;
                    svc.UpgradeMFADatabase(hh, ServerName, DatabaseName);
                    this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, string.Format(infos_strings.InfosDatabaseUpgraded, DatabaseName));
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3022", ErrorCategory.OperationStopped, this));
            }
        }
    }
    #endregion

    #region Install-MFACertificate
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
    [PrimaryServerRequired, AdministratorsRightsRequired]
    public sealed class InstallMFACertificate : MFACmdlet
    {
        private int _duration = 5;
        private SwitchParameter _restart = true;

        /// <summary>
        /// RSACertificateDuration property
        /// <para type="description">Duration for the new certificate (Years)</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public int RSACertificateDuration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        /// <summary>
        /// <para type="description">Restart Farm Services</para>
        /// RestartFarm property
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter RestartFarm
        {
            get { return _restart; }
            set { _restart = value; }
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, true);
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
                    ChoiceDescription c1 = new ChoiceDescription("&Yes");
                    ChoiceDescription c2 = new ChoiceDescription("&No");
                    col.Add(c1);
                    col.Add(c2);
                    if (this.Host.UI.PromptForChoice("Install-MFACertificate", infos_strings.InfoAllKeyWillbeReset, col, 1) == 0)
                    {
                        PSHost hh = GetHostForVerbose();
                        ADFSServiceManager svc = ManagementService.ADFSManager;
                        string thumb = svc.RegisterNewRSACertificate(hh, this.RSACertificateDuration, this.RestartFarm);
                        this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosRSACertificateChanged, thumb));
                    }
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3020", ErrorCategory.OperationStopped, this));
            }
        }
    }
    #endregion

    #region Install-MFACertificateForADFS
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
    [Cmdlet(VerbsLifecycle.Install, "MFACertificateForADFS", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired, AdministratorsRightsRequired]
    public sealed class InstallMFACertificateForADFS : MFACmdlet
    {
        /// <summary>
        /// Subject property
        /// <para type="description">Subject of the certificate</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Data")]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Kind property
        /// <para type="description">Kind  of the certificate</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Data")]
        public PSADFSCertificateKind Kind { get; set; } = PSADFSCertificateKind.Signing;

        /// <summary>
        /// CertificateDuration property
        /// <para type="description">Duration for the new certificate (Years)</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Data")]
        public int CertificateDuration { get; set; } = 5;

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, true);
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
                if (ShouldProcess("Install-MFACertificateForADFS"))
                {
                    PSHost hh = GetHostForVerbose();
                    ADFSServiceManager svc = ManagementService.ADFSManager;
                    if (svc.RegisterNewADFSCertificate(hh, this.Subject, (this.Kind==PSADFSCertificateKind.Signing), this.CertificateDuration))
                        this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, infos_strings.InfosADFSCertificateChanged);
                }
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "3020", ErrorCategory.OperationStopped, this));
            }
        }
    }
    #endregion

    #region Import-MFAUsersCSV
    /// <summary>
    /// <para type="synopsis">Imports users from CSV file in MFA System.</para>
    /// <para type="description">Imports users from CSV file in MFA System, can be used with ADDS and SQL configuration.</para>
    /// </summary>
    /// <example>
    ///   <para>Import-MFAUsersCSV -LitteralPath c:\temp\import.csv</para>
    ///   <para>Import-MFAUsersCSV -LitteralPath c:\temp\import.csv -DisableAll -SendMail -NewKey</para>
    ///   <para></para>
    ///   <para>LitteralPath is required, full file name path</para>
    ///   <para></para>
    ///   <para>NewKey generation of a new Key only if an update occurs, when adding a user, a key is always generated</para>
    ///   <para></para>
    ///   <para>DisableAll Status of Added users set to disabled</para>
    /// </example>
    [Cmdlet(VerbsData.Import, "MFAUsersCSV", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
    public sealed class ImportMFAUsersCSV : MFACmdlet
    {
        /// <summary>
        /// <para type="description">Litteral path to the CSV file to be imported.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public string LitteralPath { get; set; }

        /// <summary>
        /// <para type="description">allow send email when a new Key is generated for MFA.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public SwitchParameter SendEmail { get; set; }

        /// <summary>
        /// <para type="description">Regenerate a new secret key for the updated users.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public SwitchParameter NewKey { get; set; }

        /// <summary>
        /// <para type="description">Imported users are not enabled by default. an extra administrative task required !</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public SwitchParameter DisableAll { get; set; }

        /// <summary>
        /// <para type="description">Suppress log file generation</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public SwitchParameter NoLogFile { get; set; }


        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, true);
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "1013", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ShouldProcess("Import Users in MFA with CSV file"))
            {
                try
                {
                    ImportUsersCSV imp = new ImportUsersCSV(ManagementService.ADFSManager.Config)
                    {
                        FileName = this.LitteralPath,
                        ForceNewKey = this.NewKey,
                        SendEmail = this.SendEmail,
                        DisableAll = this.DisableAll,
                        NoLogFile = this.NoLogFile
                    };
                    this.Host.UI.WriteLine(string.Format("Import Users Starting {0}", DateTime.Now.ToString()));
                    imp.DoImport();
                    this.Host.UI.WriteLine(string.Format("Import Users Finished {0} Users Imported : {1} Errors {2}", DateTime.Now.ToString(), imp.RecordsImported.ToString(), imp.ErrorsCount.ToString()));
                }
                catch (Exception Ex)
                {
                    this.Host.UI.WriteErrorLine(Ex.Message);
                }
            }
        }
    }
    #endregion

    #region Import-MFAUsersXML
    /// <summary>
    /// <para type="synopsis">Imports users from XML file in MFA System.</para>
    /// <para type="description">Imports users from XML file in MFA System, can be used with ADDS and SQL configuration.</para>
    /// </summary>
    /// <example>
    ///   <para>Import-MFAUsersXML -LitteralPath c:\temp\import.xml</para>
    ///   <para>Import-MFAUsersXML -LitteralPath c:\temp\import.xml -DisableAll -SendMail -NewKey</para>
    ///   <para></para>
    ///   <para>LitteralPath is required, full file name path</para>
    ///   <para></para>
    ///   <para>NewKey generation of a new Key only if an update occurs, when adding a user, a key is always generated</para>
    ///   <para></para>
    ///   <para>DisableAll Status of Added users set to disabled</para>
    /// </example>
    [Cmdlet(VerbsData.Import, "MFAUsersXML", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
    public sealed class ImportMFAUsersXML : MFACmdlet
    {
        /// <summary>
        /// <para type="description">Litteral path to the XML file to be imported.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string LitteralPath { get; set; }

        /// <summary>
        /// <para type="description">allow send email when a new Key is generated for MFA.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public SwitchParameter SendEmail { get; set; }

        /// <summary>
        /// <para type="description">Regenerate a new secret key for updated users.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public SwitchParameter NewKey { get; set; }

        /// <summary>
        /// <para type="description">Imported users are not enabled by default. an extra administrative task required !</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public SwitchParameter DisableAll { get; set; } 

        /// <summary>
        /// <para type="description">Imported users are not enabled by default. an extra administrative task required !</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public SwitchParameter NoLogFile { get; set; }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, true);
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "1013", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ShouldProcess("Import Users in MFA with XML file"))
            {
                try
                {
                    ImportUsersXML imp = new ImportUsersXML(ManagementService.ADFSManager.Config)
                    {
                        FileName = this.LitteralPath,
                        ForceNewKey = this.NewKey,
                        SendEmail = this.SendEmail,
                        DisableAll = this.DisableAll,
                        NoLogFile = this.NoLogFile
                    };
                    this.Host.UI.WriteLine(string.Format("Import Users Starting {0}", DateTime.Now.ToString()));
                    imp.DoImport();
                    this.Host.UI.WriteLine(string.Format("Import Users Finished {0} Users Imported : {1} Errors {2}", DateTime.Now.ToString(), imp.RecordsImported.ToString(), imp.ErrorsCount.ToString()));
                }
                catch (Exception Ex)
                {
                    this.Host.UI.WriteErrorLine(Ex.Message);
                }
            }
        }
    }
    #endregion

    #region Import-MFAUsersADDS
    /// <summary>
    /// <para type="synopsis">Imports users from ADDS in MFA System.</para>
    /// <para type="description">Imports users from ADDS in MFA System, can be used with ADDS and SQL configuration.</para>
    /// </summary>
    /// <example>
    ///   <para>Import-MFAUsersADDS -LDAPPath "dc=domain,dc=com"</para>
    ///   <para>Import-MFAUsersADDS -LDAPPath "dc=domain,dc=com" -DisableAll -SendMail -NewKey</para>
    ///   <para>Import-MFAUsersADDS -LDAPPath "dc=domain,dc=com" -Method Code -ModifiedSince ([DateTime]::UtcNow.AddHours(-4))</para>
    ///   <para>Import-MFAUsersADDS -LDAPPath "dc=domain,dc=com" -Method Code -ModifiedSince ([DateTime]::UtcNow.AddMinutes(-30))</para>
    ///   <para></para>
    ///   <para>LitteralPath is required, full file name path</para>
    ///   <para></para>
    ///   <para>NewKey generation of a new Key only if an update occurs, when adding a user, a key is always generated</para>
    ///   <para></para>
    ///   <para>DisableAll Status of Added users set to disabled</para>
    /// </example>
    [Cmdlet(VerbsData.Import, "MFAUsersADDS", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
    public sealed class ImportMFAUsersADDS : MFACmdlet
    {
        /// <summary>
        /// <para type="description">ADDS LDAP path to query (dc=domain,dc=com)</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Identity")]
        [ValidateNotNullOrEmpty()]
        public string LDAPPath { get; set; }

        /// <summary>
        /// <para type="description">ADDS DNS domain or forest</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public string DomainName { get; set; }

        /// <summary>
        /// <para type="description">optionnal UserName used to connect</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public string UserName { get; set; }

        /// <summary>
        /// <para type="description">optionnal Password used to connect</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public string Password { get; set; }

        /// <summary>
        /// <para type="description">Imports only users created since CreatedSince property</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public DateTime? CreatedSince { get; set; }

        /// <summary>
        /// <para type="description">Imports only users modified since ModifiedSince property</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public DateTime? ModifiedSince { get; set; }

        /// <summary>
        /// <para type="description">Mail Attribute for Email or $null</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public string MailAttributes { get; set; }

        /// <summary>
        /// <para type="description">Phone Attribute for Phone or $null</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public string PhoneAttribute { get; set; }

        /// <summary>
        /// <para type="description">Default value for method</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public PSPreferredMethod Method { get; set; }

        /// <summary>
        /// <para type="description">allow send email when a new Key is generated for MFA.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public SwitchParameter SendEmail { get; set; }

        /// <summary>
        /// <para type="description">Regenerate a new secret key for updated users.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public SwitchParameter NewKey { get; set; }

        /// <summary>
        /// <para type="description">Imported users are not enabled by default. an extra administrative task required !</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public SwitchParameter DisableAll { get; set; }

        /// <summary>
        /// <para type="description">Imported users are not enabled by default. an extra administrative task required !</para>
        /// </summary>
        [Parameter(ParameterSetName = "Identity")]
        public SwitchParameter NoLogFile { get; set; }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, true);
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "1013", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ShouldProcess("Import Users in MFA from ADDS"))
            {
                try
                {
                    ImportUsersADDS imp = new ImportUsersADDS(ManagementService.ADFSManager.Config)
                    {
                        ForceNewKey = this.NewKey,
                        SendEmail = this.SendEmail,
                        DisableAll = this.DisableAll,
                        NoLogFile = this.NoLogFile,
                        CreatedSince = this.CreatedSince,
                        ModifiedSince = this.ModifiedSince,
                        DomainName = this.DomainName,
                        LDAPPath = this.LDAPPath,
                        MailAttribute = this.MailAttributes,
                        PhoneAttribute = this.PhoneAttribute,
                        Method = (PreferredMethod)this.Method,
                        UserName = this.UserName,
                        Password = this.Password
                    };

                    this.Host.UI.WriteLine(string.Format("Import Users Starting {0}", DateTime.Now.ToString()));
                    imp.DoImport();
                    this.Host.UI.WriteLine(string.Format("Import Users Finished {0} Users Imported : {1} Errors {2}", DateTime.Now.ToString(), imp.RecordsImported.ToString(), imp.ErrorsCount.ToString()));
                }
                catch (Exception Ex)
                {
                    this.Host.UI.WriteErrorLine(Ex.Message);
                }
            }
        }
    }

    /// <summary>
    /// <para type="description">Set TOTP Provider configuration data.</para>
    /// </summary>
    public class SQLEncryptedDatabaseDynamicParameters
    {
        private int _duration = 5;
        private bool _restart = true;
        private string _encryptedname;
        private string _thumbprint = string.Empty;
        private SwitchParameter _reuse;

        /// <summary>
        /// <para type="description">Restart Farm Services</para>
        /// RestartFarm property
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter RestartFarm
        {
            get { return _restart; }
            set { _restart = value; }
        }

        /// <summary>
        /// <para type="description">EncryptKeyName, SQLServer crypting key, required if encrypted switch is used.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public string EncryptKeyName 
        {
            get
            {
                if (string.IsNullOrEmpty(_encryptedname))
                    return "adfsmfa";
                else
                    return _encryptedname;
            }
            set
            {
                _encryptedname = value;
            }
        }

        /// <summary>
        /// <para type="description">CertificateDuration, SQLServer crypting certificate duration, in years, default = 5. Required if new certificate is requested.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public int CertificateDuration 
        { 
            get { return _duration; } 
            set 
            { 
                if (value<1)
                    _duration = 5;
                else
                    _duration = value;
            }
        }

        /// <summary>
        /// <para type="description">ReuseCertificate, Reuse existing SQLServer crypting certificate,you must also specify a certificate ThumPrint.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter ReuseCertificate
        {
            get { return _reuse; }
            set { _reuse = value; }
        }

        /// <summary>
        /// <para type="description">ThumbPrint, existing SQLServer crypting certificate thumbprint, you must specify switch parameter ReuseCertificate.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public string ThumbPrint
        {
            get
            {
                if (!ReuseCertificate)
                    return string.Empty;
                else
                    return _thumbprint;
            }
            set
            {
                if (ReuseCertificate)
                {
                    ManagementService.Initialize(true);
                    ADFSServiceManager svc = ManagementService.ADFSManager;
                    if (svc.CheckCertificate(value))
                        _thumbprint = value.ToUpper();
                    else
                        throw new ArgumentException("Invalid certificate thumprint !", "ThumbPrint");
                }
            }
        }
    }

    #endregion 

    #region Firewall Rules
    /// <summary>
    /// <para type="synopsis">Set basic Firewall rules for MFA inter servers communication.</para>
    /// <para type="description">Set basic Firewall rules for MFA inter servers communication.</para>
    /// </summary>
    /// <example>
    ///   <para>Set-MFAFirewallRules</para>
    ///   <para>Set-MFAFirewallRules -ComputersAllowed '172.16.100.1, 172.16.100.2'</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "MFAFirewallRules", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class SetMFAFirewallRules : MFACmdlet
    {
        private string _computers;

        /// <summary>
        /// <para type="description">Computers Allowed va firewall inbound rules and outbound rules.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public string ComputersAllowed
        {
            get { return _computers; }
            set { _computers = value; }
        }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (string.IsNullOrEmpty(_computers))
            {
                try
                {
                    ManagementService.Initialize(this.Host, true);
                    _computers = string.Empty;
                    foreach (ADFSServerHost cp in ManagementService.ADFSManager.ADFSFarm.Servers)
                    {
                        IPHostEntry ips = Dns.GetHostEntry(cp.MachineName);
                        foreach(IPAddress ip in ips.AddressList)
                        {
                            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                _computers += ip.MapToIPv4().ToString() + ",";
                        }
                    }
                    _computers = _computers.Remove(_computers.Length-1, 1);
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "4023", ErrorCategory.OperationStopped, this));
                }
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ShouldProcess("MFA Firewall Rules Configuration"))
            {
                try
                {
                    ManagementService.RemoveFirewallRules();
                    ManagementService.AddFirewallRules(_computers);
                    this.WriteVerbose(infos_strings.InfosConfigUpdated);
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "4024", ErrorCategory.OperationStopped, this));
                }
            }
        }
    }
    #endregion
    
    #region Certificates Management
    /// <summary>
    /// <para type="synopsis">Update Acces Control List for MFA Certificates stored in LocalMachine Store. Usefull after you deploy an cetificate in ADFS Farm</para>
    /// <para type="description">Update Acces Control List for MFA Certificates stored in LocalMachine Store. Usefull after you deploy an cetificate in ADFS Farm</para>
    /// </summary>
    /// <example>
    ///   <para>Update-MFACertificatesAccessControlList</para>
    /// </example>
    [Cmdlet(VerbsData.Update, "MFACertificatesAccessControlList", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [AdministratorsRightsRequired]
    public sealed class UpdatetMFACertificatesAccessControlList : MFACmdlet
    {
        /// <summary>
        /// <para type="description">Certificates Kind to check for updating private keys ACL (0 = All, 1 = MFA, 2 = ADFS, 4 = SSL.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public PSKeyMgtOptions CertsKind { get; set; } = PSKeyMgtOptions.AllCerts;

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, true);
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "4023", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ShouldProcess("MFA Certificates ACL Update"))
            {
                try
                {
                    if (ManagementService.UpdateCertificatesACL((Neos.IdentityServer.MultiFactor.Data.Certs.KeyMgtOptions)CertsKind))
                        this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, infos_strings.InfosCertsACLUpdated); 
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "4024", ErrorCategory.OperationStopped, this));
                }
            }
        }
    }

    /// <summary>
    /// <para type="synopsis">Clear keys pairs found in C:\ProgramData\Microsoft\Crypto\RSA\MachineKeys that are not linked with a valid certificate</para>
    /// <para type="description">Clear keys pairs found in C:\ProgramData\Microsoft\Crypto\RSA\MachineKeys that are not linked with a valid certificate</para>
    /// </summary>
    /// <example>
    ///   <para>Clear-MFAOrphanedRSAKeyPairs</para>
    /// </example>
    /// <example>
    ///   <para>Clear-MFAOrphanedRSAKeyPairs -AutoCleanUp Enable -CleanUpDelay 20</para>
    /// </example>
    [Cmdlet(VerbsCommon.Clear, "MFAOrphanedRSAKeyPairs", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [AdministratorsRightsRequired]
    public sealed class ClearMFAOrphanedRSAKeyPairs : MFACmdlet
    {
        /// <summary>
        /// <para type="description">AutoCleanUp for automating clean-up.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "Data")]
        public PSKeyAutoCleanUp AutoCleanUp { get; set; } = PSKeyAutoCleanUp.None;

        /// <summary>
        /// <para type="description">CleanUpDelay period in minutes.</para>
        /// </summary>
        [Parameter(Mandatory=false, ParameterSetName = "Data")]
        public int CleanUpDelay { get; set; } = 20;

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, true);
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "4023", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            string warn = @"This function is intrusive, you should only use it if the C:\ProgramData\ Microsoft\Crypto\RSA\MachineKeys directory is is filled with files. These problems are often related to improperly configured SSL certificates. before any operation you must imperatively hold an export of each certificate with its private key (.pfx) in order to be able to reinstall these certificates is needed.";
            if (ShouldProcess("MFA Remove Certificates orphaned private keys", warn, "CAUTION !!!"))
            {
                if (this.ShouldContinue("Are you sure to process ?", "WARNING !!!"))
                {
                    try
                    {
                        int res = ManagementService.CleanOrphanedPrivateKeys((byte)AutoCleanUp, CleanUpDelay);
                        this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, string.Format(infos_strings.InfosOrphanedDeleted + " : {0}", res));
                        if ((AutoCleanUp == PSKeyAutoCleanUp.Enable) || (AutoCleanUp == PSKeyAutoCleanUp.Disable))
                        {
                            this.Host.UI.WriteLine(ConsoleColor.Red, this.Host.UI.RawUI.BackgroundColor, string.Format(infos_strings.InfosServerServicesRestarted));
                        }
                    }
                    catch (Exception ex)
                    {
                        this.ThrowTerminatingError(new ErrorRecord(ex, "4024", ErrorCategory.OperationStopped, this));
                    }
                }
            }
        }
    }
    #endregion

    #region Config cache mgmt
    /// <summary>
    ///     <para type="synopsis">Update Configuration cache from current server via WinRM</para>
    ///     <para type="description">Update Configuration cache from current server via WinRM</para>
    /// </summary>
    /// <example>
    ///     <para>Refresh-MFAConfigurationCache</para>
    /// </example>
    [PrimaryServerRequired, AdministratorsRightsRequired]
    [Cmdlet("Refresh", "MFAConfigurationCache", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class RefreshMFAConfigurationCache : MFACmdlet
    {
        /// <summary>
        /// <para type="description">Notification message.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "Data")]
        public string Message { get; set; } = Environment.MachineName;

        /// <summary>
        /// <para type="description">Send Notification to remote server and local server.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter Local { get; set; } = false;

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, true);
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "4032", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ShouldProcess("Dispatch Refresh of Configuration Cache to servers ? "))
            {
                try
                {
                    ManagementService.BroadcastNotification((byte)NotificationsKind.ConfigurationReload, Message, Local.IsPresent);
                    this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, infos_strings.InfosNotificationSend);
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "4024", ErrorCategory.OperationStopped, this));
                }
            }
        }
    }

    /// <summary>
    ///     <para type="synopsis">Request New Configuration cache from current server via WinRM</para>
    ///     <para type="description">Request New Configuration cache from current server via WinRM</para>
    /// </summary>
    /// <example>
    ///     <para>Update-MFAConfigurationCache</para>
    /// </example>
    [PrimaryServerRequired, AdministratorsRightsRequired]
    [Cmdlet(VerbsData.Update, "MFAConfigurationCache", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class UpdateMFAConfigurationCache : MFACmdlet
    {
        /// <summary>
        /// <para type="description">Notification message.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "Data")]
        public string Message { get; set; } = Environment.MachineName;

        /// <summary>
        /// <para type="description">Send Notification to remote server and local server.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter Local { get; set; } = false;

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, true);
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "4032", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ShouldProcess("Dispatch New Configuration Cache to servers ? "))
            {
                try
                {
                    ManagementService.BroadcastNotification((byte)NotificationsKind.ConfigurationCreated, Message, Local.IsPresent);
                    this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, infos_strings.InfosNotificationSend);
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "4024", ErrorCategory.OperationStopped, this));
                }
            }
        }
    }

    /// <summary>
    ///     <para type="synopsis">Delete Configuration cache from servers via WinRM</para>
    ///     <para type="description">Delete Configuration cache from server via WinRM</para>
    /// </summary>
    /// <example>
    ///     <para>Clear-MFAConfigurationCache</para>
    /// </example>
    [PrimaryServerRequired, AdministratorsRightsRequired]
    [Cmdlet(VerbsCommon.Clear, "MFAConfigurationCache", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class DeleteMFAConfigurationCache : MFACmdlet
    {
        /// <summary>
        /// <para type="description">Notification message.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "Data")]
        public string Message { get; set; } = Environment.MachineName;

        /// <summary>
        /// <para type="description">Send Notification to remote server and local server.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Data")]
        public SwitchParameter Local { get; set; } = false;

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, true);
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "4032", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ShouldProcess("Dispatch Delete of cConfiguration Cache ? "))
            {
                try
                {
                    ManagementService.BroadcastNotification((byte)NotificationsKind.ConfigurationDeleted, Message, Local.IsPresent);
                    this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, infos_strings.InfosNotificationSend);
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "4024", ErrorCategory.OperationStopped, this));
                }
            }
        }
    }
    #endregion

    #region Update-MFACredentials
    /// <summary>
    ///     <para type="synopsis">Update MFA Crrdentials</para>
    ///     <para type="description">Change MFA Passwords and PassPhrase</para>
    /// </summary>
    /// <example>
    ///     <para>Update-MFACredentials</para>
    ///     <para>Update-MFACredentials -Kind SuperUser -Value mypassord</para>
    /// </example>
    [PrimaryServerRequired, AdministratorsRightsRequired]
    [Cmdlet(VerbsData.Update, "MFACredentials", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
    public sealed class UpdateMFACredentials : MFACmdlet
    {

        /// <summary>
        /// <para type="description">Credential Kind.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Identity")]
        [ValidateRange(PSPasswordSync.All, PSPasswordSync.SystemPassPhrase)]
        public PSPasswordSync Kind { get; set; } = PSPasswordSync.All;

        /// <summary>
        /// <para type="description">Credential/Password value.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = "Identity")]
        public String Value { get; set; } = string.Empty;

        /// <summary>
        /// <para type="description">Clear Credential value or Reset XORKey to default.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 2, ParameterSetName = "Identity")]
        public SwitchParameter ClearCredential { get; set; }

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, false);
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "4032", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ShouldProcess("Change MFA Credentials ? "))
            {
                try
                {
                    ManagementService.SetMFACredentials(Host, (byte)Kind, Value, ClearCredential.IsPresent);
                    this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, infos_strings.InfosCredentialsChanged);
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "4024", ErrorCategory.OperationStopped, this));
                }
            }
        }
    }

    /// <summary>
    /// CredValueDynamicParameters class implementation
    /// </summary>
    internal class CredValueDynamicParameters
    {
    }

    #endregion

    #region Export-MFAMailTemplates
    /// <summary>
    ///     <para type="synopsis">Export and Register Mail Templates as html files for a specific LCID</para>
    ///     <para type="description">Export and Register Mail Templates as html files for a specific LCID</para>
    /// </summary>
    /// <example>
    ///     <para>Export-MFAMailTemplates -LCID 1036</para>
    /// </example>
    [PrimaryServerRequired, AdministratorsRightsRequired]
    [Cmdlet(VerbsData.Export, "MFAMailTemplates", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class ExportMFAMailTemplates : MFACmdlet
    {
        /// <summary>
        /// <para type="description">LCID.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "Data")]
        public int LCID { get; set; } = 1033;

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, false);
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "4032", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ShouldProcess("Export Mail Templates for LCID "+LCID.ToString()+" ? "))
            {
                try
                {
                    ManagementService.ExportMFAMailTemplates(Host, LCID);
                    this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, infos_strings.InfosMailTemplateExported);
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "4024", ErrorCategory.OperationStopped, this));
                }
            }
        }
    }
    #endregion

    #region Install-MFASamples
    /// <summary>
    ///     <para type="synopsis">Install and configure MFA Samples</para>
    /// </summary>
    /// <example>
    ///     <para>Install-MFASamples -Kind Quiz</para>
    /// </example>
    [PrimaryServerRequired, AdministratorsRightsRequired]
    [Cmdlet(VerbsLifecycle.Install, "MFASamples", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class InstallMFASamples : MFACmdlet
    {
        /// <summary>
        /// <para type="description">Kind.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Data")]
        public PSSampleKind Kind { get; set; } = PSSampleKind.QuizProviderSample;

        /// <summary>
        /// <para type="description">Kind.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "Data")]
        public SwitchParameter Reset { get; set; } = false;

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                ManagementService.Initialize(this.Host, false);
            }
            catch (Exception ex)
            {
                this.ThrowTerminatingError(new ErrorRecord(ex, "4032", ErrorCategory.OperationStopped, this));
            }
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ShouldProcess("Install MFA Sample " + Kind.ToString() + " ? "))
            {
                try
                {
                    ManagementService.InstallMFASample(Host, (FlatSampleKind)Kind, Reset.ToBool());
                    if (!Reset)
                        this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, infos_strings.InfosSampleInstalled);
                    else
                        this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, infos_strings.InfosSampleUnInstalled);
                }
                catch (Exception ex)
                {
                    this.ThrowTerminatingError(new ErrorRecord(ex, "4024", ErrorCategory.OperationStopped, this));
                }
            }
        }
    }
    #endregion

    #region Attribute
    /// <summary>
    /// <para type="synopsis">Attribute for Primary Server required for Cmdlets in MFA System.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PrimaryServerRequiredAttribute : Attribute
    {
        /// <summary>
        /// PrimaryServerRequiredAttribute constructor
        /// </summary>
        public PrimaryServerRequiredAttribute()
        {
        }
    }

    /// <summary>
    /// <para type="synopsis">Attribute for checking for ADFS 2019 version for Cmdlets in MFA System.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ADFS2019RequiredAttribute : Attribute
    {
        /// <summary>
        /// ADFS2019RequiredAttribute constructor
        /// </summary>
        public ADFS2019RequiredAttribute()
        {

        }
    }

    /// <summary>
    /// <para type="synopsis">Attribute for checking for ADFS Administration rights for Cmdlets in MFA System.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AdministratorsRightsRequiredAttribute : Attribute
    {
        /// <summary>
        /// AdministratorsRightsRequiredAttribute constructor
        /// </summary>
        public AdministratorsRightsRequiredAttribute()
        {

        }
    }
    #endregion
}
