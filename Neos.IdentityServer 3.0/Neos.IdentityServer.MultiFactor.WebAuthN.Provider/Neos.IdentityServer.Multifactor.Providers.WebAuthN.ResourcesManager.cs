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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    public class ResourcesLocale
    {
        private CultureInfo resourceCulture;
        private ResourceManager _CSHtml;

        /// <summary>
        /// ResourceManager constructor
        /// </summary>
        public ResourcesLocale(int lcid)
        {
            try
            {
                resourceCulture = new CultureInfo(lcid);
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
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "ResourceSet" + sep + resourcename + "." + Culture.Name + ".resources"))
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
        /// CSHhtml property
        /// </summary>
        private ResourceManager CSHhtml
        {
            get
            {
                if (_CSHtml == null)
                {
                    _CSHtml = GetResourceManager("Neos.IdentityServer.MultiFactor.WebAuthN.Resources.CSHtml");
                }
                return _CSHtml;
            }
        }


        /// <summary>
        /// GetString method implementation
        /// </summary>
        public virtual string GetString(ResourcesLocaleKind kind, string name)
        {
            switch (kind)
            {
                case ResourcesLocaleKind.Html:
                    return CSHhtml.GetString(name, this.Culture);
                default:
                    return string.Empty;
            }
        }
    }

    public enum ResourcesLocaleKind
    {
        Html = 1,
        Errors = 2
    }
}
