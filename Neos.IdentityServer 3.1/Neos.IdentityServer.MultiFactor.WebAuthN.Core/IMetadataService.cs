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
using System;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    /// <summary>
    /// The FIDO2 metadata service.
    /// </summary>
    public interface IMetadataService
    {
        uint Timeout { get; set; }
        int TimestampDriftTolerance { get; set; }

        /// <summary>
        /// Gets the metadata TOC (table-of-content) payload entry by a guid.
        /// </summary>
        /// <param name="aaguid">The Authenticator Attestation GUID.</param>
        /// <returns>Returns the entry; Otherwise <c>null</c>.</returns>
        MetadataBLOBPayloadEntry GetEntry(Guid aaguid);

        /// <summary>
        /// Gets a value indicating whether the internal access token is valid.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if access token is valid, or <c>false</c> if the access token is equal to an invalid token value.
        /// </returns>
        bool ConformanceTesting();

        /// <summary>
        /// Gets a value indicating whether the metadata service is initialized.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if the metadata service is initialized, or <c>false</c> if the metadata service is not initialized.
        /// </returns>
        bool IsInitialized();
        /// <summary>
        /// Initializes the metadata service.
        /// </summary>
        void Initialize();
        /// <summary>
        /// NeedToReload the metadata service.
        /// </summary>
        bool NeedToReload { get; set; }
    }
}
