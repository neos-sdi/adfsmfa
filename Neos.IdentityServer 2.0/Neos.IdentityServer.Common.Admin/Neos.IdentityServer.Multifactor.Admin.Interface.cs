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
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.ServiceModel;
using Neos.IdentityServer.MultiFactor;

namespace Neos.IdentityServer.MultiFactor.Administration
{
    [ServiceContract]
    public interface IAdministrationService
    {
        [OperationContract]
        MMCRegistration GetUserRegistration(string upn);

        [OperationContract]
        MMCRegistration AddUserRegistration(MMCRegistration reg);

        [OperationContract]
        bool DeleteUserRegistration(MMCRegistration reg);

        [OperationContract]
        void SetUserRegistration(MMCRegistration reg);

        [OperationContract]
        MMCRegistration EnableUserRegistration(MMCRegistration reg);

        [OperationContract]
        MMCRegistration DisableUserRegistration(MMCRegistration reg);

        [OperationContract]
        MMCRegistrationList GetUserRegistrations(UsersFilterObject filter, UsersOrderObject order, UsersPagingObject paging, int maxrows = 20000);

        [OperationContract]
        MMCRegistrationList GetAllUserRegistrations(UsersOrderObject order, int maxrows = 20000, bool enabledonly = false);

        [OperationContract]
        int GetUserRegistrationsCount(UsersFilterObject filter);
    }

    public interface IUserPropertiesDataObject
    {
        MMCRegistrationList GetUserControlData(MMCRegistrationList registrations);
        void SetUserControlData(MMCRegistrationList registrations, bool disablesync);
        bool SyncDisabled {get; set;}
    }
}
