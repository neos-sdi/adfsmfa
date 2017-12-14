using Neos.IdentityServer.MultiFactor.Data;
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
using System.Globalization;

namespace Neos.IdentityServer.MultiFactor
{
    #region External Connectors
    /// <summary>
    /// IExternalOTPProvider interface declaration
    /// </summary>
    public interface IExternalOTPProvider
    {
        int GetUserCodeWithExternalSystem(string upn, string phonenumber, string email, ExternalOTPProvider externalsys, CultureInfo culture);
    }

    /// <summary>
    /// IExternalOTPProvider interface declaration
    /// </summary>
    public interface IExternalOTPProvider2: IExternalOTPProvider
    {
        NotificationStatus GetCodeWithExternalSystem(Registration reg, ExternalOTPProvider externalsys, CultureInfo culture, out int otp);
    }

    /// <summary>
    /// ISecretKeyManager interface declaration
    /// </summary>
    public interface ISecretKeyManager
    {
        string Prefix { get; }
        void Initialize(MFAConfig config);
        string NewKey(string upn);
        string ReadKey(string upn);
        string EncodedKey(string upn);
        string ProbeKey(string upn);
        bool RemoveKey(string upn); 
        bool ValidateKey(string upn);
        string StripKeyPrefix(string key);
        string AddKeyPrefix(string key);
        bool HasKeyPrefix(string key);
        KeysRepositoryService KeysStorage { get; }
    }
    #endregion

    
}
