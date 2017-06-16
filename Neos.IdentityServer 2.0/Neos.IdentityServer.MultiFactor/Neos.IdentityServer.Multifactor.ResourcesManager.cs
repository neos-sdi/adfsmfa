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
        private ResourceManager _html_strings;
        private ResourceManager _errors_strings;
        private ResourceManager _infos_strings;
        private ResourceManager _title_strings;
        private ResourceManager _valid_strings;

        /// <summary>
        /// ResourceManager constructor
        /// </summary>
        public ResourcesLocale(int lcid)
        {
            resourceCulture = new CultureInfo(lcid);
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

        /// <summary>
        /// html_strings property
        /// </summary>
        private ResourceManager html_strings
        {
            get
            {
                if (_html_strings == null)
                {
                    _html_strings = new ResourceManager("Neos.IdentityServer.MultiFactor.Resources.html_strings", typeof(ResourcesLocale).Assembly);
                }
                return _html_strings;
            }
        }

        /// <summary>
        /// errors_strings property 
        /// </summary>
        private ResourceManager errors_strings
        {
            get
            {
                if (_errors_strings == null)
                {
                    _errors_strings = new ResourceManager("Neos.IdentityServer.MultiFactor.Resources.errors_strings", typeof(ResourcesLocale).Assembly);
                }
                return _errors_strings;
            }
        }

        /// <summary>
        /// infos_strings property
        /// </summary>
        private ResourceManager infos_strings
        {
            get
            {
                if (_infos_strings == null)
                {
                    _infos_strings = new ResourceManager("Neos.IdentityServer.MultiFactor.Resources.infos_strings", typeof(ResourcesLocale).Assembly);
                }
                return _infos_strings;
            }
        }

        /// <summary>
        /// title_strings property
        /// </summary>
        private ResourceManager title_strings
        {
            get
            {
                if (_title_strings == null)
                {
                    _title_strings = new ResourceManager("Neos.IdentityServer.MultiFactor.Resources.title_strings", typeof(ResourcesLocale).Assembly);
                }
                return _title_strings;
            }
        }

        /// <summary>
        /// valid_strings property
        /// </summary>
        private ResourceManager valid_strings
        {
            get
            {
                if (_valid_strings == null)
                {
                    _valid_strings = new ResourceManager("Neos.IdentityServer.MultiFactor.Resources.valid_strings", typeof(ResourcesLocale).Assembly);
                }
                return _valid_strings;
            }
        }

        public virtual string GetString(ResourcesLocaleKind kind, string name)
        {
            switch (kind)
            {
                case ResourcesLocaleKind.Errors:
                    return errors_strings.GetString(name, this.Culture);
                case ResourcesLocaleKind.Html:
                    return html_strings.GetString(name, this.Culture);
                case ResourcesLocaleKind.Informations:
                    return infos_strings.GetString(name, this.Culture);
                case ResourcesLocaleKind.Titles:
                    return title_strings.GetString(name, this.Culture);
                case ResourcesLocaleKind.Validation:
                    return valid_strings.GetString(name, this.Culture);
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
