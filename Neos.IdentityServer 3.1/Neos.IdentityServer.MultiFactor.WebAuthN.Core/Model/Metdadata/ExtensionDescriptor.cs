//******************************************************************************************************************************************************************************************//
// Copyright (c) 2021 abergs (https://github.com/abergs/fido2-net-lib)                                                                                                                      //                        
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
using Newtonsoft.Json;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    /// <summary>
    /// This descriptor contains an extension supported by the authenticator. 
    /// </summary>
    /// <remarks>
    /// <see href="https://fidoalliance.org/specs/fido-v2.0-rd-20180702/fido-metadata-statement-v2.0-rd-20180702.html#extensiondescriptor-dictionary"/>
    /// </remarks>
    public class ExtensionDescriptor
    {
        /// <summary>
        /// Gets or sets the identifier that identifies the extension.
        /// </summary>
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the tag.
        /// <para>This field may be empty.</para>
        /// </summary>
        /// <remarks>
        /// The TAG of the extension if this was assigned. TAGs are assigned to extensions if they could appear in an assertion. 
        /// </remarks>
        [JsonProperty("tag")]
        public ushort Tag { get; set; }
        /// <summary>
        /// Gets or sets arbitrary data further describing the extension and/or data needed to correctly process the extension. 
        /// <para>This field may be empty.</para>
        /// </summary>
        /// <remarks>
        /// This field MAY be missing or it MAY be empty.
        /// </remarks>
        [JsonProperty("data")]
        public string Data { get; set; }
        /// <summary>
        /// Gets or sets a value indication whether an unknown extensions must be ignored (<c>false</c>) or must lead to an error (<c>true</c>) when the extension is to be processed by the FIDO Server, FIDO Client, ASM, or FIDO Authenticator. 
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        ///     <item>A value of false indicates that unknown extensions MUST be ignored.</item>
        ///     <item>A value of true indicates that unknown extensions MUST result in an error.</item>
        /// </list>
        /// </remarks>
        [JsonProperty("fail_if_unknown", Required = Required.Always)]
        public bool Fail_If_Unknown { get; set; }
    }
}
