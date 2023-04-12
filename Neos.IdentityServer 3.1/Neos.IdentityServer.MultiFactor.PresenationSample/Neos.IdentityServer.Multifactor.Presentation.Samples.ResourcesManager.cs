//******************************************************************************************************************************************************************************************//
// Copyright (c) 2023 redhook (adfsmfa@gmail.com)                                                                                                                                    //                        
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
using Microsoft.IdentityServer.Web.Authentication.External;
using Neos.IdentityServer.MultiFactor.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor.Samples
{
    public class CustomResourcesLocale : ResourcesLocale
    {
        /// <summary>
        /// ResourceManager constructor
        /// </summary>
        public CustomResourcesLocale(int lcid) : base(lcid)
        {
        }

        /// <summary>
        /// LoadResources method override
        /// </summary>
        public override void LoadResources()
        {
            base.LoadResources();
            /* if yo want to reuse existing resources
             * you must call the ancestor method and add your custom resources
             * ResourcesList.Add(ResourcesLocaleKind.CustomUIHtml, GetResourceManager(typeof(CustomResourcesLocale).Assembly, "Neos.IdentityServer.MultiFactor.Samples.Resources.CustHtml"));
             */

            /* if you want to replace ALL existing ressources
             * Add your ressources like that
             * ResourcesList.Add(ResourcesLocaleKind.UIHtml, GetResourceManager(typeof(CustomResourcesLocale).Assembly, "Neos.IdentityServer.MultiFactor.Samples.Resources.SHtml"));
             * ResourcesList.Add(ResourcesLocaleKind.UIErrors, GetResourceManager(typeof(CustomResourcesLocale).Assembly, "Neos.IdentityServer.MultiFactor.Samples.Resources.SErrors"));
             * ResourcesList.Add(ResourcesLocaleKind.UIInformations, GetResourceManager(typeof(CustomResourcesLocale).Assembly, "Neos.IdentityServer.MultiFactor.Samples.Resources.SInfos"));
             * ResourcesList.Add(ResourcesLocaleKind.UITitles, GetResourceManager(typeof(CustomResourcesLocale).Assembly, "Neos.IdentityServer.MultiFactor.Samples.Resources.STitle"));
             * ResourcesList.Add(ResourcesLocaleKind.UIValidation, GetResourceManager(typeof(CustomResourcesLocale).Assembly, "Neos.IdentityServer.MultiFactor.Samples.Resources.SCheck"));
             */
        }
    }
}
