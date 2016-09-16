//******************************************************************************************************************************************************************************************//
// Copyright (c) 2011 George Mamaladze                                                                                                                                                      //
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
using Neos.IdentityServer.MultiFactor.QrEncoding.DataEncodation;
using Neos.IdentityServer.MultiFactor.QrEncoding.ErrorCorrection;
using Neos.IdentityServer.MultiFactor.QrEncoding.Positioning;
using Neos.IdentityServer.MultiFactor.QrEncoding.EncodingRegion;
using Neos.IdentityServer.MultiFactor.QrEncoding.Masking;
using Neos.IdentityServer.MultiFactor.QrEncoding.Masking.Scoring;
using System.Collections.Generic;

namespace Neos.IdentityServer.MultiFactor.QrEncoding
{
	internal static class QRCodeEncode
	{
		internal static BitMatrix Encode(string content, ErrorCorrectionLevel errorLevel)
		{
			EncodationStruct encodeStruct = DataEncode.Encode(content, errorLevel);

            return ProcessEncodationResult(encodeStruct, errorLevel);
			
		}

        internal static BitMatrix Encode(IEnumerable<byte> content, ErrorCorrectionLevel errorLevel)
        {
            EncodationStruct encodeStruct = DataEncode.Encode(content, errorLevel);

            return ProcessEncodationResult(encodeStruct, errorLevel);
        }

        private static BitMatrix ProcessEncodationResult(EncodationStruct encodeStruct, ErrorCorrectionLevel errorLevel)
        {
            BitList codewords = ECGenerator.FillECCodewords(encodeStruct.DataCodewords, encodeStruct.VersionDetail);

            TriStateMatrix triMatrix = new TriStateMatrix(encodeStruct.VersionDetail.MatrixWidth);
            PositioninngPatternBuilder.EmbedBasicPatterns(encodeStruct.VersionDetail.Version, triMatrix);

            triMatrix.EmbedVersionInformation(encodeStruct.VersionDetail.Version);
            triMatrix.EmbedFormatInformation(errorLevel, new Pattern0());
            triMatrix.TryEmbedCodewords(codewords);

            return triMatrix.GetLowestPenaltyMatrix(errorLevel);
        }
		
	}
}
