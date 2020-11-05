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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neos.IdentityServer.MultiFactor.Common
{
    #region WebThemeManager
    public static class WebThemeManager
    {
        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public static void Initialize(MFAConfig config, AuthenticationContext context, Uri request)
        {
            WebThemesClient manager = new WebThemesClient();
            try
            {
                manager.Initialize();
                IWebThemeManager client = manager.Open();
                try
                {
                    WebThemesParametersRecord message = new WebThemesParametersRecord()
                    {
                        Identifier = context.ThemeIdentifier,
                        LCID = context.Lcid
                    };
                    var servernames = (from server in config.Hosts.ADFSFarm.Servers
                                       select (server.FQDN.ToLower(), server.NodeType.ToLower().Equals("primarycomputer")));

                    Dictionary<string, bool> dic = servernames.ToDictionary(pair => pair.Item1, pair => pair.Item2);
                    string identifier = string.Empty;
                    if (client.Initialize(dic, message, request.OriginalString, out identifier))
                        context.ThemeIdentifier = identifier;
                }
                catch (Exception)
                {
                    manager.UnInitialize();
                    return;
                }
                finally
                {
                    manager.Close(client);
                }
            }
            catch (Exception)
            {
                return;
            }
            return;
        }

        /// <summary>
        /// ResetThemesList method implmentation
        /// </summary>
        public static void ResetThemesList(MFAConfig config)
        {
            WebThemesClient manager = new WebThemesClient();
            try
            {
                manager.Initialize();
                IWebThemeManager client = manager.Open();
                try
                {
                    var servernames = (from server in config.Hosts.ADFSFarm.Servers
                                       select (server.FQDN.ToLower(), server.NodeType.ToLower().Equals("primarycomputer")));

                    Dictionary<string, bool> dic = servernames.ToDictionary(pair => pair.Item1, pair => pair.Item2);
                    client.ResetThemesList(dic);
                }
                catch (Exception)
                {
                    manager.UnInitialize();
                    return;
                }
                finally
                {
                    manager.Close(client);
                }
            }
            catch (Exception)
            {
                return;
            }
            return;
        }


        /// <summary>
        /// HasRelyingPartyTheme
        /// </summary>
        public static bool HasRelyingPartyTheme(AuthenticationContext context)
        {
            WebThemesClient manager = new WebThemesClient();
            try
            {
                manager.Initialize();
                IWebThemeManager client = manager.Open();
                try
                {
                    WebThemesParametersRecord message = new WebThemesParametersRecord()
                    {
                        Identifier = context.ThemeIdentifier,
                        LCID = context.Lcid
                    };
                    return client.HasRelyingPartyTheme(message);
                }
                catch (Exception)
                {
                    manager.UnInitialize();
                    return false;
                }
                finally
                {
                    manager.Close(client);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// GetIllustrationAddress method implementation
        /// </summary>
        public static string GetIllustrationAddress(AuthenticationContext context)
        {
            WebThemesClient manager = new WebThemesClient();
            try
            {
                manager.Initialize();
                IWebThemeManager client = manager.Open();
                try
                {
                    WebThemesParametersRecord message = new WebThemesParametersRecord()
                    {
                        Identifier = context.ThemeIdentifier,
                        LCID = context.Lcid
                    };
                    return client.GetIllustrationAddress(message);
                }
                catch (Exception)
                {
                    manager.UnInitialize();
                    return string.Empty; 
                }
                finally
                {
                    manager.Close(client);
                }
            }
            catch (Exception)
            {
                return string.Empty; 
            }
        }

        /// <summary>
        /// GetLogoAddress method implementation
        /// </summary>
        public static string GetLogoAddress(AuthenticationContext context)
        {
            WebThemesClient manager = new WebThemesClient();
            try
            {
                manager.Initialize();
                IWebThemeManager client = manager.Open();
                try
                {
                    WebThemesParametersRecord message = new WebThemesParametersRecord()
                    {
                        Identifier = context.ThemeIdentifier,
                        LCID = context.Lcid
                    };
                    return client.GetLogoAddress(message);
                }
                catch (Exception)
                {
                    manager.UnInitialize();
                    return string.Empty;
                }
                finally
                {
                    manager.Close(client);
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// GetStyleSheetAddress method implementation
        /// </summary>
        public static string GetStyleSheetAddress(AuthenticationContext context)
        {
            WebThemesClient manager = new WebThemesClient();
            try
            {
                manager.Initialize();
                IWebThemeManager client = manager.Open();
                try
                {
                    WebThemesParametersRecord message = new WebThemesParametersRecord()
                    {
                        Identifier = context.ThemeIdentifier,
                        LCID = context.Lcid
                    };
                    return client.GetStyleSheetAddress(message);
                }
                catch (Exception)
                {
                    manager.UnInitialize();
                    return string.Empty;
                }
                finally
                {
                    manager.Close(client);
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// GetAddresses method implmentation
        /// </summary>
        public static Dictionary<WebThemeAddressKind, string> GetAddresses(AuthenticationContext context)
        {
            WebThemesClient manager = new WebThemesClient();
            try
            {
                manager.Initialize();
                IWebThemeManager client = manager.Open();
                try
                {
                    WebThemesParametersRecord message = new WebThemesParametersRecord()
                    {
                        Identifier = context.ThemeIdentifier,
                        LCID = context.Lcid
                    };
                    return client.GetAddresses(message);
                }
                catch (Exception)
                {
                    manager.UnInitialize();
                    return null;
                }
                finally
                {
                    manager.Close(client);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
    #endregion
}
