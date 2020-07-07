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
using System.Collections.Generic;
using System.Globalization;
using Microsoft.IdentityServer.Web.Authentication.External;

namespace Neos.IdentityServer.MultiFactor
{
    public class AuthenticationAdapterMetadata : IAuthenticationAdapterMetadata
    {
        private readonly string _desc = "Adfs Multi Factor Authentication";

        /// <summary>
        /// AuthenticationAdapterMetadata constructor
        /// </summary>
        public AuthenticationAdapterMetadata()
        {

        }

        /// <summary>
        /// AdminName property implementation
        /// </summary>
        public string AdminName
        {
            get { return "Multi Factor Authentication Extension"; }
        }

        /// <summary>
        /// AuthenticationMethods property implementation
        /// </summary>
        public string[] AuthenticationMethods
        {
            get 
            {
                return new string[] 
                { 
                    "http://schemas.microsoft.com/ws/2012/12/authmethod/none",
                    "http://schemas.microsoft.com/ws/2012/12/authmethod/otp", 
                    "http://schemas.microsoft.com/ws/2012/12/authmethod/email",
                    "http://schemas.microsoft.com/ws/2012/12/authmethod/sms",
                    "http://schemas.microsoft.com/ws/2012/12/authmethod/smsotp",      
                    "http://schemas.microsoft.com/ws/2012/12/authmethod/smsreply",
                    "http://schemas.microsoft.com/ws/2012/12/authmethod/phoneappnotification",
                    "http://schemas.microsoft.com/ws/2012/12/authmethod/phoneconfirmation",
                    "http://schemas.microsoft.com/ws/2012/12/authmethod/phoneapplication",
                    "http://schemas.microsoft.com/ws/2012/12/authmethod/phoneconfirmation",
                    "http://schemas.microsoft.com/ws/2012/12/authmethod/voicebiometrics",
                    "http://schemas.microsoft.com/ws/2012/12/authmethod/kba",
                    "http://schemas.microsoft.com/ws/2012/12/authmethod/windowshello",
                    "http://schemas.microsoft.com/ws/2012/12/authmethod/fido",
                    "http://schemas.microsoft.com/ws/2012/12/authmethod/webauthN"
                };
            } 
        }

        /// <summary>
        /// AvailableLcids property implementation
        /// </summary>
        public int[] AvailableLcids
        {

            get
            {
                return new int[]
                {
                    new CultureInfo("en").LCID,
                    new CultureInfo("ar").LCID, new CultureInfo("bg").LCID,  new CultureInfo("bs").LCID, new CultureInfo("cs").LCID,  new CultureInfo("da").LCID,
                    new CultureInfo("de").LCID, new CultureInfo("el").LCID, new CultureInfo("en-GB").LCID, new CultureInfo("en-US").LCID, new CultureInfo("es").LCID,
                    new CultureInfo("es-MX").LCID, new CultureInfo("et").LCID, new CultureInfo("fi").LCID, new CultureInfo("fr").LCID, new CultureInfo("fr-CA").LCID,
                    new CultureInfo("fr-FR").LCID, new CultureInfo("he").LCID, new CultureInfo("hr").LCID, new CultureInfo("hu").LCID, new CultureInfo("it").LCID,
                    new CultureInfo("ja").LCID, new CultureInfo("ko").LCID, new CultureInfo("lt").LCID, new CultureInfo("lv").LCID, new CultureInfo("nl").LCID,
                    new CultureInfo("no").LCID, new CultureInfo("pl").LCID, new CultureInfo("pt").LCID, new CultureInfo("pt-br").LCID, new CultureInfo("ro").LCID,
                    new CultureInfo("ru").LCID, new CultureInfo("sk").LCID, new CultureInfo("sl").LCID, new CultureInfo("sr-Latn-RS").LCID, new CultureInfo("sv").LCID,
                    new CultureInfo("th").LCID, new CultureInfo("tr").LCID, new CultureInfo("uk").LCID, new CultureInfo("zh-HANS").LCID, new CultureInfo("zh-HANT").LCID
                };
            }
        }

        /// <summary>
        /// Descriptions property implementation
        /// </summary>
        public Dictionary<int, string> Descriptions
        {
            get
            {
                Dictionary<int, string> result = new Dictionary<int, string>();
                result.Add(new CultureInfo("en").LCID, _desc);  // en
                result.Add(new CultureInfo("ar").LCID, _desc);  // ar
                result.Add(new CultureInfo("bg").LCID, _desc);  // bg
                result.Add(new CultureInfo("bs").LCID, _desc);  // bs
                result.Add(new CultureInfo("cs").LCID, _desc);  // cs
                result.Add(new CultureInfo("da").LCID, _desc);  // da
                result.Add(new CultureInfo("de").LCID, _desc);  // de
                result.Add(new CultureInfo("el").LCID, _desc);  // el
                result.Add(new CultureInfo("en-GB").LCID, _desc);  // en-GB
                result.Add(new CultureInfo("en-US").LCID, _desc);  // en-US
                result.Add(new CultureInfo("es").LCID, _desc);  // es
                result.Add(new CultureInfo("es-MX").LCID, _desc);  // es-MX
                result.Add(new CultureInfo("et").LCID, _desc);  // et
                result.Add(new CultureInfo("fi").LCID, _desc);  // fi
                result.Add(new CultureInfo("fr").LCID, _desc);  // fr
                result.Add(new CultureInfo("fr-CA").LCID, _desc);  // fr-CA
                result.Add(new CultureInfo("fr-FR").LCID, _desc);  // fr-FR
                result.Add(new CultureInfo("he").LCID, _desc);  // he
                result.Add(new CultureInfo("hr").LCID, _desc);  // hr
                result.Add(new CultureInfo("hu").LCID, _desc);  // hu
                result.Add(new CultureInfo("it").LCID, _desc);  // it
                result.Add(new CultureInfo("ja").LCID, _desc);  // ja
                result.Add(new CultureInfo("ko").LCID, _desc);  // ko
                result.Add(new CultureInfo("lt").LCID, _desc);  // lt
                result.Add(new CultureInfo("lv").LCID, _desc);  // lv
                result.Add(new CultureInfo("nl").LCID, _desc);  // nl
                result.Add(new CultureInfo("no").LCID, _desc);  // no
                result.Add(new CultureInfo("pl").LCID, _desc);  // pl
                result.Add(new CultureInfo("pt").LCID, _desc);  // pt
                result.Add(new CultureInfo("pt-br").LCID, _desc);  // pt-BR
                result.Add(new CultureInfo("ro").LCID, _desc);  // ro
                result.Add(new CultureInfo("ru").LCID, _desc);  // ru
                result.Add(new CultureInfo("sk").LCID, _desc);  // sk
                result.Add(new CultureInfo("sl").LCID, _desc);  // sl
                result.Add(new CultureInfo("sr-Latn-RS").LCID, _desc);  // sr-Latn-RS
                result.Add(new CultureInfo("sv").LCID, _desc);  // sv
                result.Add(new CultureInfo("th").LCID, _desc);  // th
                result.Add(new CultureInfo("tr").LCID, _desc);  // tr
                result.Add(new CultureInfo("uk").LCID, _desc);  // uk
                result.Add(new CultureInfo("zh-HANS").LCID, _desc);  // zh-HANS
                result.Add(new CultureInfo("zh-HANT").LCID, _desc);  // zh-HANT
                return result;
            }
        }

        /// <summary>
        /// FriendlyNames property implementation
        /// </summary>
        public Dictionary<int, string> FriendlyNames
        {
            get
            {
                Dictionary<int, string> result = new Dictionary<int, string>();
                result.Add(new CultureInfo("en").LCID, "Sign in using MFA");  // en
                result.Add(new CultureInfo("ar").LCID, "Inicie sesión con MFA");  // ar
                result.Add(new CultureInfo("bg").LCID, "Влезте чрез MFA");  // bg
                result.Add(new CultureInfo("bs").LCID, "Sign in using MFA");  // bs
                result.Add(new CultureInfo("cs").LCID, "Пријавите се користећи МФА");  // cs
                result.Add(new CultureInfo("da").LCID, "Sign in using MFA");  // da
                result.Add(new CultureInfo("de").LCID, "Melden Sie sich mit MFA an");  // de
                result.Add(new CultureInfo("el").LCID, "Prisijunkite naudodami MFA");  // el
                result.Add(new CultureInfo("en-GB").LCID, "Sign in using MFA");  // en-GB
                result.Add(new CultureInfo("en-US").LCID, "Sign in using MFA");  // en-US
                result.Add(new CultureInfo("es").LCID, "Inicie sesión con MFA");  // es
                result.Add(new CultureInfo("es-MX").LCID, "Inicie sesión con MFA");  // es-MX
                result.Add(new CultureInfo("et").LCID, "Sign in using MFA");  // et
                result.Add(new CultureInfo("fi").LCID, "Kirjaudu sisään MFA: n avulla");  // fi
                result.Add(new CultureInfo("fr").LCID, "Se connecter à l'aide de MFA");  // fr
                result.Add(new CultureInfo("fr-CA").LCID, "Se connecter à l'aide de MFA");  // fr-CA
                result.Add(new CultureInfo("fr-FR").LCID, "Se connecter à l'aide de MFA");  // fr-FR
                result.Add(new CultureInfo("he").LCID, "Sign in using MFA");  // he
                result.Add(new CultureInfo("hr").LCID, "Prijavite se koristeći MFA");  // hr
                result.Add(new CultureInfo("hu").LCID, "Jelentkezzen be az MFA segítségével");  // hu
                result.Add(new CultureInfo("it").LCID, "Accedi utilizzando MFA");  // it
                result.Add(new CultureInfo("ja").LCID, "Sign in using MFA");  // ja
                result.Add(new CultureInfo("ko").LCID, "Sign in using MFA");  // ko
                result.Add(new CultureInfo("lt").LCID, "Prisijunkite naudodami MFA");  // lt
                result.Add(new CultureInfo("lv").LCID, "Pierakstieties, izmantojot MFA");  // lv
                result.Add(new CultureInfo("nl").LCID, "Meld u aan met MFA");  // nl
                result.Add(new CultureInfo("no").LCID, "Logg deg på med MFA");  // no
                result.Add(new CultureInfo("pl").LCID, "Zaloguj się przy użyciu MFA");  // pl
                result.Add(new CultureInfo("pt").LCID, "Entrar usando MFA");  // pt
                result.Add(new CultureInfo("pt-BR").LCID, "Entrar usando MFA");  // pt-BR
                result.Add(new CultureInfo("ro").LCID, "Conectați-vă folosind MFA");  // ro
                result.Add(new CultureInfo("ru").LCID, "Войдите, используя MFA");  // ru
                result.Add(new CultureInfo("sk").LCID, "Prihláste sa pomocou nástroja MFA");  // sk
                result.Add(new CultureInfo("sl").LCID, "Sign in using MFA");  // sl
                result.Add(new CultureInfo("sr-Latn-RS").LCID, "Sign in using MFA");  // sr-Latn-RS
                result.Add(new CultureInfo("sv").LCID, "Inicie sesión con MFA");  // sv
                result.Add(new CultureInfo("th").LCID, "เข้าสู่ระบบโดยใช้ MFA");  // th
                result.Add(new CultureInfo("tr").LCID, "MFA kullanarak giriş yapın");  // tr
                result.Add(new CultureInfo("uk").LCID, "Увійдіть за допомогою MFA");  // uk
                result.Add(new CultureInfo("zh-HANS").LCID, "Sign in using MFA");  // zh-HANS
                result.Add(new CultureInfo("zh-HANT").LCID, "Sign in using MFA");  // zh-HANT
                return result;
            }
        }

        /// <summary>
        /// IdentityClaims property implementation
        /// MUST BE ONE OF THE FOLLOWING
        /// "https://schemas.microsoft.com/ws/2008/06/identity/claims/windowsaccountname"
        /// "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn"
        /// "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"
        /// "https://schemas.microsoft.com/ws/2008/06/identity/claims/primarysid"
        /// </summary>
        public string[] IdentityClaims
        {
            get { return new string[] { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn" }; }
        }

        /// <summary>
        /// RequiresIdentity property implementation
        /// </summary>
        public bool RequiresIdentity
        {
            get { return true; }
        }

    }
}
