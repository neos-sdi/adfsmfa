//******************************************************************************************************************************************************************************************//
// Copyright (c) 2024 redhook (adfsmfa@gmail.com)                                                                                                                                        //                        
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
using Neos.IdentityServer.MultiFactor.Common;
using System;
using System.Collections.Generic;

namespace Neos.IdentityServer.MultiFactor
{
    public class RuntimePresentation
    {
        /// <summary>
        /// GetAdministrativeProvider method implementation
        /// </summary>
        public static IExternalAdminProvider GetAdministrativeProvider(MFAConfig cfg)
        {
            return RuntimeAuthProvider.GetAdministrativeProvider(cfg);
        }

        /// <summary>
        /// GetProvider method implementation
        /// </summary>
        public static IExternalProvider GetProvider(PreferredMethod method)
        {
            return RuntimeAuthProvider.GetProvider(method);
        }

        /// <summary>
        /// GetProviderInstance method implementation
        /// </summary>
        public static IExternalProvider GetProviderInstance(PreferredMethod method)
        {
            return RuntimeAuthProvider.GetProviderInstance(method);
        }

        /// <summary>
        /// GeActiveProvidersList method implementation
        /// </summary>
        public static List<IExternalProvider> GeActiveProvidersList()
        {
            return RuntimeAuthProvider.GeActiveProvidersList();
        }

        /// <summary>
        /// GetActiveProvidersCount method implementation
        /// </summary>
        public static int GetActiveProvidersCount()
        {
            return RuntimeAuthProvider.GetActiveProvidersCount();
        }

        /// <summary>
        /// IsProviderAvailable method implementation
        /// </summary>
        public static bool IsProviderAvailable(AuthenticationContext ctx, PreferredMethod method)
        {
            return RuntimeAuthProvider.IsProviderAvailable(ctx, method);
        }

        /// <summary>
        /// IsProviderAvailableForUser method implementation
        /// </summary>
        public static bool IsProviderAvailableForUser(AuthenticationContext ctx, PreferredMethod method)
        {
            return RuntimeAuthProvider.IsProviderAvailableForUser(ctx, method);
        }

        /// <summary>
        /// IsPinCodeRequired method implementation
        /// </summary>
        public static bool IsPinCodeRequired(AuthenticationContext ctx)
        {
            return RuntimeAuthProvider.IsPinCodeRequired(ctx);
        }

        /// <summary>
        /// IsUIElementRequired method implementation
        /// </summary>
        public static bool IsUIElementRequired(AuthenticationContext ctx, RequiredMethodElements element)
        {
            return RuntimeAuthProvider.IsUIElementRequired(ctx, element);
        }

        /// <summary>
        /// MustChangePasswordSoon method implementation
        /// </summary>
        public static bool MustChangePasswordSoon(MFAConfig cfg, AuthenticationContext usercontext, out DateTime max)
        {
            return RuntimeRepository.MustChangePasswordSoon(cfg, usercontext, out max);
        }

        /// <summary>
        /// CanChangePassword method implmentation
        /// </summary>
        public static bool CanChangePassword(MFAConfig cfg, AuthenticationContext usercontext)
        {
            return RuntimeRepository.CanChangePassword(cfg, usercontext);
        }

        /// <summary>
        /// GetUserProperties method implmentation
        /// </summary>
        public static MFAUser GetUserProperties(MFAConfig cfg, string upn)
        {
            return RuntimeRepository.GetMFAUser(cfg, upn);
        }

        /// <summary>
        /// GetNewUserKey method implementation
        /// </summary>
        public static string GetNewUserKey(string upn)
        {
            KeysManager.NewKey(upn);
            return KeysManager.EncodedKey(upn);
        }
    }
}
