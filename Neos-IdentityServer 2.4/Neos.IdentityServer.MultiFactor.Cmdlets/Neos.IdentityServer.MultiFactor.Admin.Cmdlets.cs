using Neos.IdentityServer.MultiFactor.Cmdlets;
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
    using Neos.IdentityServer.MultiFactor;
    using System.Collections.ObjectModel;
    using MFA;
    using System.Net;

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
        [Parameter(Mandatory = false, Position=0, ParameterSetName = "Identity")]
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
        [ValidateSet("Choose", "Code", "Email", "External", "Azure")]
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
            ManagementService.Initialize(this.Host, true);
            if (!ManagementService.Config.UseActiveDirectory) 
            {
                if (ManagementService.Config.Hosts.SQLServerHost.IsAlwaysEncrypted) 
                {
                    if (_filter.FilterField == DataFilterField.UserName)
                        return new PSDataFieldMixedParameters(_filter, _order, this.Host);
                    else
                        return new PSDataFieldCryptedParameters(_filter, _order, this.Host);
                }
                else
                    return new PSDataFieldParameters(_filter, _order, this.Host);
            }
            else
                return new PSDataFieldParameters(_filter, _order, this.Host);
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
        private PSHost Host;

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
        private PSHost Host;

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
        private PSHost Host;

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
        private PSPreferredMethod _method = PSPreferredMethod.None;
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
        [ValidateSet("Choose", "Code", "Email", "External", "Azure")]
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
                        if (this.Method != PSPreferredMethod.None)
                            reg.PreferredMethod = this.Method;
                        if (this.Pin > -1)
                            reg.PIN = this.Pin;
                        reg.Enabled = this.Enabled;

                        if (string.IsNullOrEmpty(reg.MailAddress) && ManagementService.Config.MailProvider.Enabled)
                           this.Host.UI.WriteWarningLine(string.Format(errors_strings.ErrorEmailNotProvided, reg.UPN));
                        if (string.IsNullOrEmpty(reg.PhoneNumber) && ManagementService.Config.ExternalProvider.Enabled)
                            this.Host.UI.WriteWarningLine(string.Format(errors_strings.ErrorPhoneNotProvided, reg.UPN));

                        ManagementService.Initialize(this.Host, true);
                        PSRegistration ret = (PSRegistration)ManagementService.SetUserRegistration((Registration)reg, this.ResetKey, false, this.EmailForNewKey);

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
        private PSPreferredMethod _method = PSPreferredMethod.None;
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
        [ValidateSet("Choose", "Code", "Email", "External", "Azure")]
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
                        if (this.Method != PSPreferredMethod.None)
                            reg.PreferredMethod = this.Method;
                        if (this.Pin > -1)
                            reg.PIN = this.Pin;
                        reg.Enabled = this.Enabled; 

                        if (string.IsNullOrEmpty(reg.MailAddress) && ManagementService.Config.MailProvider.Enabled)
                            this.Host.UI.WriteWarningLine(string.Format(errors_strings.ErrorEmailNotProvided, reg.UPN));
                        if (string.IsNullOrEmpty(reg.PhoneNumber) && ManagementService.Config.ExternalProvider.Enabled)
                            this.Host.UI.WriteWarningLine(string.Format(errors_strings.ErrorPhoneNotProvided, reg.UPN));

                        ManagementService.Initialize(this.Host, true);
                        PSRegistration ret = (PSRegistration)ManagementService.AddUserRegistration((Registration)reg, !this.NoNewKey, false, this.EmailForNewKey);

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

                        ManagementService.DeleteUserRegistration((Registration)reg, true);

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

                        PSRegistration res = (PSRegistration)ManagementService.EnableUserRegistration((Registration)reg);

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
            int i= 0;
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

                        PSRegistration res = (PSRegistration)ManagementService.DisableUserRegistration((Registration)reg);
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
    [PrimaryServerRequired]
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
                ADFSServiceManager svc = ManagementService.ADFSManager;
                svc.RegisterADFSComputer(hh, ServerName);
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
    ///   <para>UnRegister-MFAComputer</para>
    /// </example>
    [Cmdlet(VerbsLifecycle.Unregister, "MFAComputer", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired]
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
                ADFSServiceManager svc = ManagementService.ADFSManager;
                svc.UnRegisterADFSComputer(hh, ServerName);
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
    [PrimaryServerRequired]
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
                if (ShouldProcess("Restart MFA Service"))
                {
                    if (string.IsNullOrEmpty(Identity))
                        Identity = "local";
                    PSHost hh = GetHostForVerbose();
                    ADFSServiceManager svc = ManagementService.ADFSManager;
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
    [PrimaryServerRequired]
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
                ManagementService.Initialize(this.Host, true);
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
                    ADFSServiceManager svc = ManagementService.ADFSManager;
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
    ///   <para>Register-MFASystem -Activate -RestartFarm -KeysFormat RSA -RSACertificateDuration 10</para>
    ///   <para>Create a new empty configuration for MFA. Activation and Restart of services. Key Format is set to RSA with a cetificate for 10 Years</para>
    /// </example>
    /// <example>
    ///   <para>Register-MFASystem -Activate -RestartFarm -KeysFormat CUSTOM -RSACertificateDuration 10</para>
    ///   <para>Create a new empty configuration for MFA. Activation and Restart of services. Key Format is set to RSA CUSTOM with a cetificate for 10 Years, you must create a database for storing user keys and certificates with New-MFASecretKeysDatabase Cmdlet</para>
    /// </example>
    /// <example>
    ///   <para>Register-MFASystem -Activate -RestartFarm -AllowUpgrade -BackupFilePath c:\temp\myconfig 1.2.xml</para>
    ///   <para>Upgrade from previous versions, a copy of the current version is saved in spécified backup file </para>
    /// </example>
    [Cmdlet(VerbsLifecycle.Register, "MFASystem", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired]
    public sealed class RegisterMFASystem : MFACmdlet, IDynamicParameters
    {
        private BackupFilePathDynamicParameters Dyn;
        private PSSecretKeyFormat _fmt = PSSecretKeyFormat.RNG;
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
        [ValidateSet("RNG", "RSA", "CUSTOM")]
        public PSSecretKeyFormat KeysFormat
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
                if (ShouldProcess("Register-MFASystem", "Changing Keyformat invalidate all users keys ! When choosing CUSTOM KeyFormat, additional steps are required (Set-MFAConfigkeys and New-MFASecretKeysDatabase)" ))
                {
                    PSHost hh = GetHostForVerbose();
                    ADFSServiceManager svc = ManagementService.ADFSManager;
                    if (this.AllowUpgrade)
                    {
                        if (svc.RegisterMFAProvider(hh, this.Activate, this.RestartFarm, true, Dyn.BackupFilePath, (Neos.IdentityServer.MultiFactor.SecretKeyFormat)this.KeysFormat, this.RSACertificateDuration))
                            this.Host.UI.WriteLine(ConsoleColor.Green, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosSystemRegistered));
                        else
                            this.Host.UI.WriteLine(ConsoleColor.Yellow, this.Host.UI.RawUI.BackgroundColor, String.Format(infos_strings.InfosSystemAlreadyInitialized));
                    }
                    else
                        if (svc.RegisterMFAProvider(hh, this.Activate, this.RestartFarm, false, null, (Neos.IdentityServer.MultiFactor.SecretKeyFormat)this.KeysFormat, this.RSACertificateDuration))
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
        [Parameter(ParameterSetName = "Data", Mandatory = true)]
        public string BackupFilePath
        {
            get;
            set;
        }
    }

    /// <summary>
    /// <para type="synopsis">Update MFA Configuration with a configuration file.</para>
    /// <para type="description">Update Configuration of MFA components. Activate it</para>
    /// </summary>
    /// <example>
    ///   <para>Import-MFASystemConfiguration -ImportFilePath c:\temp\mysavedconfig.xml</para>
    ///   <para>Import a new configuration for MFA with a file. You must complete configuration with other CmdLets</para>
    /// </example>
    /// <example>
    ///   <para>Import-MFASystemConfiguration -Activate -RestartFarm -ImportFilePath c:\temp\mysavedconfig.xml</para>
    ///   <para>Update MFA configuration with the specified file. Activation and Restart of services is available. </para>
    /// </example>
    [Cmdlet("Import", "MFASystemConfiguration", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired]
    public sealed class ImportMFASystemConfiguration : MFACmdlet
    {
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
        /// <para type="description">Set the name of the backup file.</para>
        /// BackupFileName property
        /// </summary>
        [Parameter(ParameterSetName = "Data", Mandatory = true)]
        public string ImportFilePath
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
                if (ShouldProcess("Import-MFASystemConfiguration", "Import and ovveride the current MFA configuration with : " + ImportFilePath + " ? "))
                {
                    PSHost hh = GetHostForVerbose();
                    ADFSServiceManager svc = ManagementService.ADFSManager;
                    if (svc.ImportMFAProviderConfiguration(hh, this.Activate, this.RestartFarm, ImportFilePath))
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

    /// <summary>
    /// <para type="synopsis">Save current MFA Configuration to a file.</para>
    /// <para type="description">Save current Configuration of MFA components</para>
    /// </summary>
    /// <example>
    ///   <para>Export-MFASystemConfiguration -ExportFilePath c:\temp\mysavedconfig.xml</para>
    ///   <para>Export current MFA configuration to the specified file.</para>
    /// </example>
    [Cmdlet("Export", "MFASystemConfiguration", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired]
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
    [PrimaryServerRequired]
    public sealed class UnRegisterMFASystem : MFACmdlet
    {
        /// <summary>
        /// <para type="description">Set the name of the backup file.</para>
        /// BackupFileName property
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Data")]
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
    [PrimaryServerRequired]
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

    /// <summary>
    /// <para type="synopsis">Disable MFA.</para>
    /// <para type="description">Disable MFA System (if initialized)</para>
    /// </summary>
    /// <example>
    ///   <para>Disable-MFASystem</para>
    ///   <para>Disable MFA configuration from ADFS. </para>
    /// </example>
    [Cmdlet(VerbsLifecycle.Disable, "MFASystem", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired]
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
    [PrimaryServerRequired]
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
                    ChoiceDescription c1 = new ChoiceDescription("&Yes") ;
                    ChoiceDescription c2 = new ChoiceDescription("&No") ;
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
    [PrimaryServerRequired]
    public sealed class SetMFAConfig : MFACmdlet
    {
        private PSConfig _config;
        private FlatConfig _target;

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
                    _target = (FlatConfig)_config;
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
                this.WriteWarning(string.Format(infos_strings.InfosWarningAboutTheme, "ADFS 2012r2/2016/2019"));
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
    /// <para type="synopsis">Set ADFS Theme.</para>
    /// <para type="description">Set ADFS Theme.</para>
    /// </summary>
    /// <example>
    ///   <para>Set-MFAThemeMode -UIKind Default -Theme yourcompatibletheme</para>
    ///   <para>Set-MFAThemeMode -UIKind Default2019 -Theme yourcompatibletheme</para>
    ///   <para>Set-MFAThemeMode -UIKind Default2019 -Theme yourcompatibletheme -Paginated</para>
    ///   <para>Change policy template for MFA configuration</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "MFAEncryptionVersion", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired]
    public sealed class MFAEncryptionVersion : MFACmdlet
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
            set { _usepaginated=value; } 
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

                FlatConfigSQL cf = new FlatConfigSQL();
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
    [PrimaryServerRequired]
    public sealed class SetMFAConfigSQL : MFACmdlet
    {
        private PSConfigSQL _config;
        private FlatConfigSQL _target;

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
                    _target = (FlatConfigSQL)_config;
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

                FlatConfigADDS cf = new FlatConfigADDS();
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
    [PrimaryServerRequired]
    public sealed class SetMFAConfigADDS : MFACmdlet
    {
        private PSConfigADDS _config;
        private FlatConfigADDS _target;

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
                    _target = (FlatConfigADDS)_config;
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
    /// <para type="synopsis">Get MFA Provider Configuration.</para>
    /// <para type="description">Get MFA Provider configuration options.</para>
    /// <para type="description"></para>
    /// </summary>
    /// <example>
    ///   <para>Get-MFAProvider -ProviderType External</para>
    ///   <para>Get MFA Provider configuration for (Code, Email, External, Azure, Biometrics)</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "MFAProvider", SupportsShouldProcess = true, SupportsPaging = false, ConfirmImpact = ConfirmImpact.Medium, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    public sealed class GetMFAProvider : MFACmdlet
    {
        private PSConfigTOTPProvider _config0;
        private PSConfigMailProvider _config1;
        private PSConfigExternalProvider _config2;
        private PSConfigAzureProvider _config3;

        private PSProviderType _providertype = PSProviderType.Code;

        /// <summary>
        /// <para type="description">Provider Type parameter, (Code, Email, External, Azure, Biometrics) Required.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data", ValueFromPipeline = true)]
        [ValidateRange(PSProviderType.Code, PSProviderType.Azure)]
        public PSProviderType ProviderType
        {
            get { return _providertype; }
            set { _providertype = value; }
        }

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
                        FlatOTPProvider cf0 = new FlatOTPProvider();
                        cf0.Load(this.Host);
                        _config0 = (PSConfigTOTPProvider)cf0;
                        break;
                    case PSProviderType.Email:
                        FlatConfigMail cf1 = new FlatConfigMail();
                        cf1.Load(this.Host);
                        _config1 = (PSConfigMailProvider)cf1;
                        break;
                    case PSProviderType.External:
                        FlatExternalProvider cf2 = new FlatExternalProvider();
                        cf2.Load(this.Host); 
                        _config2 = (PSConfigExternalProvider)cf2;
                        break;
                    case PSProviderType.Azure:
                        FlatAzureProvider cf3 = new FlatAzureProvider();
                        cf3.Load(this.Host);
                        _config3 = (PSConfigAzureProvider)cf3;
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
            base.StopProcessing();
        }
    }

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
    [Cmdlet(VerbsCommon.Set, "MFAProvider", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired]
    public sealed class SetMFAProvider : MFACmdlet, IDynamicParameters
    {
        private TOTPDynamicParameters _target0;
        private MailDynamicParameters _target1;
        private ExternalDynamicParameters _target2;
        private AzureDynamicParameters _target3;

        private PSProviderType _providertype = PSProviderType.Code;

        /// <summary>
        /// <para type="description">Provider Type parameter, (Code, Email, External, Azure, Biometrics) Required.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data", ValueFromPipeline = true)]
        [ValidateRange(PSProviderType.Code, PSProviderType.Azure)]
        public PSProviderType ProviderType
        {
            get { return _providertype; }
            set { _providertype = value; }
        }

        /// <summary>
        /// <para type="description">Set the value of Provider configuration.</para>
        /// GetDynamicParameters implementation
        /// </summary>
        public object GetDynamicParameters()
        {
            switch (ProviderType)
            {
                case PSProviderType.Code:
                    _target0 = new TOTPDynamicParameters();
                    return _target0;
                case PSProviderType.Email:
                    _target1 = new MailDynamicParameters();
                    return _target1;
                case PSProviderType.External:
                    _target2 = new ExternalDynamicParameters();
                    return _target2;
                case PSProviderType.Azure:
                    _target3 = new AzureDynamicParameters();
                    return _target3;
                default:
                    break;
            }
            return null;
        }

        /// <summary>
        /// ProcessRecord method override
        /// </summary>
        protected override void ProcessRecord()
        {
            if (ShouldProcess("MFA Providers Configuration"))
            {
                try
                {
                    switch (ProviderType)
                    {
                        case PSProviderType.Code:
                            if ((_target0.Data) is PSConfigTOTPProvider)
                                ((FlatOTPProvider)(_target0.Data)).Update(this.Host);
                            else
                                throw new Exception("Invalid Profider Type !");
                            break;
                        case PSProviderType.Email:
                            if ((_target1.Data) is PSConfigMailProvider)
                                ((FlatConfigMail)(_target1.Data)).Update(this.Host);
                            else
                                throw new Exception("Invalid Profider Type !");
                            break;
                        case PSProviderType.External:
                            if ((_target2.Data) is PSConfigExternalProvider)
                                ((FlatExternalProvider)(_target2.Data)).Update(this.Host);
                            else
                                throw new Exception("Invalid Profider Type !");
                            break;
                        case PSProviderType.Azure:
                            if ((_target3.Data) is PSConfigAzureProvider)
                                ((FlatAzureProvider)(_target3.Data)).Update(this.Host);
                            else
                                throw new Exception("Invalid Profider Type !");
                            break;
                        default:
                            break;
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
            _target0 = null;
            _target1 = null;
            _target2 = null;
            _target3 = null;
            base.EndProcessing();
        }

        /// <summary>
        /// StopProcessing method implementation
        /// </summary>
        protected override void StopProcessing()
        {
            _target0 = null;
            _target1 = null;
            _target2 = null;
            _target3 = null;
            base.StopProcessing();
        }
    }

    /// <summary>
    /// <para type="description">Set TOTP Provider configuration data.</para>
    /// </summary>
    public class TOTPDynamicParameters
    {
        /// <summary>
        /// <para type="description">Set the value of TOTP Provider.</para>
        /// Data property
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Data", ValueFromPipeline = true)]
        public PSConfigTOTPProvider Data { get; set; }
    }

    /// <summary>
    /// <para type="description">Set Mail Provider configuration data.</para>
    /// </summary>
    public class MailDynamicParameters
    {
        /// <summary>
        /// <para type="description">Set the value of Email Provider.</para>
        /// Data property
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Data", ValueFromPipeline = true)]
        public PSConfigMailProvider Data { get; set; }
    }

    /// <summary>
    /// <para type="description">Set External Provider configuration data.</para>
    /// </summary>
    public class ExternalDynamicParameters
    {
        /// <summary>
        /// <para type="description">Set the value of SMS provider.</para>
        /// Data property
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Data", ValueFromPipeline = true)]
        public PSConfigExternalProvider Data { get; set; }
    }

    /// <summary>
    /// <para type="description">Set Azure Provider configuration data.</para>
    /// </summary>
    public class AzureDynamicParameters
    {
        /// <summary>
        /// <para type="description">Set the value of Azure provider.</para>
        /// Data property
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Data", ValueFromPipeline = true)]
        public PSConfigAzureProvider Data { get; set; }
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
    [OutputType(typeof(PSConfigKeys))]
    public sealed class GetMFAConfigKeys : MFACmdlet
    {
        private PSConfigKeys _config;

        /// <summary>
        /// BeginProcessing method implementation
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {

                FlatKeysConfig cf = new FlatKeysConfig();
                cf.Load(this.Host);
                _config = (PSConfigKeys)cf;
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
    ///   <para>$cfg.KeyFormat = [MFA.PSSecretKeyFormat]::RSA</para>
    ///   <para>Set-MFAConfigKeys $cfg</para>
    ///   <para>Set MFA Secret Keys configuration options, modity values and finally Update configuration.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "MFAConfigKeys", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired]
    public sealed class SetMFAConfigKeys : MFACmdlet
    {
        private PSConfigKeys _config;
        private FlatKeysConfig _target;

        /// <summary>
        /// <para type="description">Config parameter, a variable of type PSKeyConfig.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Data", ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty()]
        public PSConfigKeys Config
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
                    _target = (FlatKeysConfig)_config;
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
    [PrimaryServerRequired]
    public sealed class NewMFADatabase : MFACmdlet, IDynamicParameters
    {
        private string _servername;
        private string _databasename;
        private string _username;
        private string _password;
        private SQLEncryptedDatabaseDynamicParameters Dyn;
        private PSConfigSQL _config;

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
                FlatConfigSQL cf = new FlatConfigSQL();
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

    /// <summary>
    /// <para type="synopsis">Create a new SQL Database for storing RSA keys and certificates.</para>
    /// <para type="description">Create a new SQL Database for storing RSA keys and certificates, can be used with ADDS and SQL configuration.</para>
    /// </summary>
    /// <example>
    ///   <para>New-MFASecretKeysDatabase -ServerName SQLServer\Instance -DatabaseName MFAKeysDatabase -UserName sqlaccount -Password pass</para>
    ///   <para>New-MFASecretKeysDatabase -ServerName SQLServer\Instance -DatabaseName MFAKeysDatabase -UserName Domain\ADFSaccount</para>
    ///   <para>New-MFASecretKeysDatabase -ServerName SQLServer\Instance -DatabaseName MFAKeysDatabase -UserName Domain\ADFSManagedAccount$</para>
    ///   <para></para>
    ///   <para>When using SQL Server 2016 (and up) Always encrypted columsn</para>
    ///   <para>New-MFASecretKeysDatabase -ServerName SQLServer\Instance -DatabaseName MFAKeysDatabase -UserName Domain\ADFSaccount -Encrypted</para>
    ///   <para>New-MFASecretKeysDatabase -ServerName SQLServer\Instance -DatabaseName MFAKeysDatabase -UserName Domain\ADFSaccount -Encrypted -EncryptedKeyName mykey</para>
    ///   <para>New-MFASecretKeysDatabase -ServerName SQLServer\Instance -DatabaseName MFAKeysDatabase -UserName Domain\ADFSManagedAccount$ -Encrypted -ReuseCertificate -ThumbPrint 0123456789ABCDEF...</para>
    ///   <para>New-MFASecretKeysDatabase -ServerName SQLServer\Instance -DatabaseName MFAKeysDatabase -UserName Domain\ADFSManagedAccount$ -Encrypted -ReuseCertificate -ThumbPrint 0123456789ABCDEF... -EncryptedKeyName mykey</para>
    ///   <para></para>
    ///   <para>(Get-MFAConfigKeys).KeyFormat must be equal to CUSTOM to be effective</para>
    ///   <para>Create a new database for MFA Secret Keys, grant rights to the specified account</para>
    /// </example>
    [Cmdlet(VerbsCommon.New, "MFASecretKeysDatabase", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Data")]
    [PrimaryServerRequired]
    public sealed class NewMFASecretKeysDatabase : MFACmdlet, IDynamicParameters
    {
        private string _servername;
        private string _databasename;
        private string _username;
        private string _password;
        private SQLEncryptedDatabaseDynamicParameters Dyn;
        private PSConfigSQL _config;

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
                FlatConfigSQL cf = new FlatConfigSQL();
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
                    ADFSServiceManager svc = ManagementService.ADFSManager;
                    if (!Encrypted)
                    {
                        svc.CreateMFASecretKeysDatabase(hh, ServerName, DatabaseName, UserName, Password);
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
                        svc.CreateMFAEncryptedSecretKeysDatabase(hh, ServerName, DatabaseName, UserName, Password, Dyn.EncryptKeyName, thumb);
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
    [Cmdlet("Import", "MFAUsersCSV", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
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
                    ImportUsersCSV imp = new ImportUsersCSV(ManagementService.ADFSManager.Config);
                    imp.FileName = this.LitteralPath;
                    imp.ForceNewKey = this.NewKey;
                    imp.SendEmail = this.SendEmail;
                    imp.DisableAll = this.DisableAll;
                    imp.NoLogFile = this.NoLogFile;
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
    [Cmdlet("Import", "MFAUsersXML", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
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
                    ImportUsersXML imp = new ImportUsersXML(ManagementService.ADFSManager.Config);
                    imp.FileName = this.LitteralPath;
                    imp.ForceNewKey = this.NewKey;
                    imp.SendEmail = this.SendEmail;
                    imp.DisableAll = this.DisableAll;
                    imp.NoLogFile = this.NoLogFile;
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
    [Cmdlet("Import", "MFAUsersADDS", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None, DefaultParameterSetName = "Identity")]
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
            if (ShouldProcess("Import Users in MFA with XML file"))
            {
                try
                {
                    ImportUsersADDS imp = new ImportUsersADDS(ManagementService.ADFSManager.Config);
                    imp.ForceNewKey = this.NewKey;
                    imp.SendEmail = this.SendEmail;
                    imp.DisableAll = this.DisableAll;
                    imp.NoLogFile = this.NoLogFile;
                    imp.CreatedSince = this.CreatedSince;
                    imp.ModifiedSince = this.ModifiedSince;
                    imp.DomainName = this.DomainName;
                    imp.LDAPPath = this.LDAPPath;
                    imp.MailAttribute = this.MailAttributes;
                    imp.PhoneAttribute = this.PhoneAttribute;
                    imp.Method = (PreferredMethod)this.Method;
                    imp.UserName = this.UserName;
                    imp.Password = this.Password;

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
    ///   <para>Set-MFAFirewallRules -ComputersAllowed '172.16.0.1, 172.17.0.50'</para>
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
    #endregion
}
