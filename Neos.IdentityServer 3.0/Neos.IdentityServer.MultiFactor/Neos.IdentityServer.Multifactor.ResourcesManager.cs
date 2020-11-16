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
using Microsoft.IdentityServer.Web.Authentication.External;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor
{
    public class ResourcesLocale
    {
        private CultureInfo resourceCulture;
        private ResourceManager _SHtml;
        private ResourceManager _SErrors;
        private ResourceManager _SInfos;
        private ResourceManager _STitle;
        private ResourceManager _SCheck;

        /// <summary>
        /// ResourceManager constructor
        /// </summary>
        public ResourcesLocale(int Lcid)
        {
            try
            {
                resourceCulture = new CultureInfo(Lcid);
            }
            catch (CultureNotFoundException)
            {
                resourceCulture = new CultureInfo("en");
            }
            catch (Exception)
            {
                resourceCulture = new CultureInfo("en");
            }
        }

        /// <summary>
        /// Culture property implmentation
        /// </summary>
        public CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
        }

        private ResourceManager GetResourceManager(string resourcename)
        {
            char sep = Path.DirectorySeparatorChar;
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep +"MFA" + sep + "ResourceSet" + sep + resourcename + "." + Culture.Name + ".resources"))
                return ResourceManager.CreateFileBasedResourceManager(resourcename, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "ResourceSet", null);
            else if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "ResourceSet" + sep + resourcename + "." + Culture.TwoLetterISOLanguageName + ".resources"))
                return ResourceManager.CreateFileBasedResourceManager(resourcename, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "ResourceSet", null);
            else if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "ResourceSet" + sep + resourcename + ".en-us.resources"))
                return ResourceManager.CreateFileBasedResourceManager(resourcename, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "ResourceSet", null);
            else if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "ResourceSet" + sep + resourcename + ".en.resources"))
                return ResourceManager.CreateFileBasedResourceManager(resourcename, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "ResourceSet", null);
            else if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "ResourceSet" + sep + resourcename + ".resources"))
                return ResourceManager.CreateFileBasedResourceManager(resourcename, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "ResourceSet", null);
            else
                return new ResourceManager(resourcename, typeof(ResourcesLocale).Assembly);
        }

        /// <summary>
        /// SHhtml property
        /// </summary>
        private ResourceManager SHhtml
        {
            get
            {
                if (_SHtml == null)
                {
                    _SHtml = GetResourceManager("Neos.IdentityServer.MultiFactor.Resources.SHtml");
                }
                return _SHtml;
            }
        }

        /// <summary>
        /// SErrors property 
        /// </summary>
        private ResourceManager SErrors
        {
            get
            {
                if (_SErrors == null)
                {
                    _SErrors = GetResourceManager("Neos.IdentityServer.MultiFactor.Resources.SErrors");
                }
                return _SErrors;
            }
        }

        /// <summary>
        /// SInfos property
        /// </summary>
        private ResourceManager SInfos
        {
            get
            {
                if (_SInfos == null)
                {
                    _SInfos = GetResourceManager("Neos.IdentityServer.MultiFactor.Resources.SInfos");
                }
                return _SInfos;
            }
        }

        /// <summary>
        /// title_strings property
        /// </summary>
        private ResourceManager STitle
        {
            get
            {
                if (_STitle == null)
                {
                    _STitle = GetResourceManager("Neos.IdentityServer.MultiFactor.Resources.STitle");
                }
                return _STitle;
            }
        }

        /// <summary>
        /// SCheck property
        /// </summary>
        private ResourceManager SCheck
        {
            get
            {
                if (_SCheck == null)
                {
                    _SCheck = GetResourceManager("Neos.IdentityServer.MultiFactor.Resources.SCheck");
                }
                return _SCheck;
            }
        }

        /// <summary>
        /// GetString method implementation
        /// </summary>
        public virtual string GetString(ResourcesLocaleKind kind, string name)
        {
            switch (kind)
            {
                case ResourcesLocaleKind.Errors:
                    return SErrors.GetString(name, this.Culture);
                case ResourcesLocaleKind.Html:
                    return SHhtml.GetString(name, this.Culture);
                case ResourcesLocaleKind.Informations:
                    return SInfos.GetString(name, this.Culture);
                case ResourcesLocaleKind.Titles:
                    return STitle.GetString(name, this.Culture);
                case ResourcesLocaleKind.Validation:
                    return SCheck.GetString(name, this.Culture);
                default:
                    return string.Empty;
            }
        }
    }

    public enum ResourcesLocaleKind
    {
        Titles = 1,
        Informations = 2,
        Errors = 3,
        Html = 4,
        Validation = 5
    }
}
