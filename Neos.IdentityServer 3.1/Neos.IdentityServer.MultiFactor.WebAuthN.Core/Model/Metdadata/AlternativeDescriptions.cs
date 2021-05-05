//******************************************************************************************************************************************************************************************//
// Copyright (c) 2020 abergs (https://github.com/abergs/fido2-net-lib)                                                                                                                      //                        
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
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    /// <summary>
    /// This descriptor contains description in alternative languages.
    /// </summary>
    /// <remarks>
    /// <see href="https://fidoalliance.org/specs/fido-v2.0-rd-20180702/fido-metadata-statement-v2.0-rd-20180702.html#alternativedescriptions-dictionary"/>
    /// </remarks>
    public class AlternativeDescriptions
    {
        /// <summary>
        /// Gets or sets alternative descriptions of the authenticator.
        /// <para>
        /// Contains IETF language codes as key (e.g. "ru-RU", "de", "fr-FR") and a localized description as value.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Contains IETF language codes, defined by a primary language subtag, 
        /// followed by a region subtag based on a two-letter country code from [ISO3166] alpha-2 (usually written in upper case).
        /// <para>Each description SHALL NOT exceed a maximum length of 200 characters.</para>
        /// <para>Description values can contain any UTF-8 characters.</para>
        /// </remarks>
        [JsonProperty("alternativeDescriptions")]
        public Dictionary<string, string> IETFLanguageCodesMembers { get; set; }
    }
}
