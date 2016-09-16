//******************************************************************************************************************************************************************************************//
// Copyright (c) 2015 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
//******************************************************************************************************************************************************************************************//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Neos.IdentityServer.MultiFactor
{
    [ServiceContract]
    public interface IAdminService
    {

        [OperationContract]
        Registration GetUserRegistration(string upn);

        [OperationContract]
        void SetUserRegistration(Registration reg);

        [OperationContract]
        Notification SetNotification(string registrationid, RegistrationPreferredMethod method);

        [OperationContract]
        Notification CheckNotification(string registrationid);
    }

    [DataContract]
    public enum RegistrationPreferredMethod
    {
        [EnumMember]
        Choose = 0,
        [EnumMember]
        ApplicationCode = 1,
        [EnumMember]
        Email = 2,
        [EnumMember]
        Phone = 3
    }

    [DataContract]
    public enum HashMode
    {
        [EnumMember]
        SHA1 = 0,
        [EnumMember]
        SHA256 = 1,
        [EnumMember]
        SHA384 = 2,
        [EnumMember]
        SHA512 = 3
    }

    [DataContract]
    public enum KeyGeneratorMode
    {
        [EnumMember]
        Guid = 0,
        [EnumMember]
        ClientSecret128 = 1,
        [EnumMember]
        ClientSecret256 = 2,
        [EnumMember]
        ClientSecret384 = 3,
        [EnumMember]
        ClientSecret512 = 4,
        [EnumMember]
        Custom = 5
    }


    [DataContract]
    public class Registration
    {
        private string _mail;
        private string _phone;
        private string _secretkey;

        [DataMember]
        public string ID
        {
            get;
            set;
        }

        [DataMember]
        public string UPN
        {
            get;
            set;
        }

        [DataMember]
        public string SecretKey
        {
            get { return _secretkey; }
            set { _secretkey = value; }
        }

        [DataMember]
        public string DisplayKey
        {
            get
            {
                if (_secretkey==null)
                    return string.Empty;
                else
                    return Base32.Encode(_secretkey);
            }
        }

        [DataMember]
        public string MailAddress
        {
            get
            {
                if (string.IsNullOrEmpty(_mail))
                    return string.Empty;
                else
                    return _mail;
            }
            set
            {
                _mail = value;
            }
        }

        [DataMember]
        public string PhoneNumber
        {
            get
            {
                if (string.IsNullOrEmpty(_phone))
                    return string.Empty;
                else
                    return _phone;
            }
            set
            {
                _phone = value;
            }
        }

        [DataMember]
        public bool Enabled
        {
            get;
            set;
        }

        [DataMember]
        public DateTime CreationDate
        {
            get;
            set;
        }

        [DataMember]
        public RegistrationPreferredMethod PreferredMethod
        {
            get;
            set;
        }

    }

    [DataContract]
    public class Notification
    {
        [DataMember]
        public string ID
        {
            get;
            set;
        }

        [DataMember]
        public string RegistrationID
        {
            get;
            set;
        }

        [DataMember]
        public int OTP
        {
            get;
            set;
        }

        [DataMember]
        public DateTime CreationDate
        {
            get;
            set;
        }

        [DataMember]
        public DateTime ValidityDate
        {
            get;
            set;
        }

        [DataMember]
        public Nullable<DateTime> CheckDate
        {
            get;
            set;
        }
    }
}
