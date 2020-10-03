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
using Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.AttestationFormat
{
    internal class None : AttestationFormat
    {
        private readonly IMetadataService _metadataService;
        private readonly bool _requireValidAttestationRoot;

        public None(CBORObject attStmt, byte[] authenticatorData, byte[] clientDataHash, IMetadataService metadataService, bool requireValidAttestationRoot) 
            : base(attStmt, authenticatorData, clientDataHash)
        {
            _metadataService = metadataService;
            _requireValidAttestationRoot = requireValidAttestationRoot;
        }

        public override void Verify()
        {
            if (0 != attStmt.Keys.Count && 0 != attStmt.Values.Count)
                throw new VerificationException("Attestation format none should have no attestation statement");
        }
    }
}
