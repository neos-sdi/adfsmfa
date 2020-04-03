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
using Neos.IdentityServer.MultiFactor.QrEncoding.Positioning;
using Neos.IdentityServer.MultiFactor.QrEncoding.EncodingRegion;

namespace Neos.IdentityServer.MultiFactor.QrEncoding.Masking
{
    public static class MatrixExtensions
    {
        public static TriStateMatrix Xor(this TriStateMatrix first, Pattern second, ErrorCorrectionLevel errorlevel)
        {
        	TriStateMatrix result = XorMatrix(first, second);
        	result.EmbedFormatInformation(errorlevel, second);
        	return result;
        }
        
        
        private static TriStateMatrix XorMatrix(TriStateMatrix first, BitMatrix second)
        {
        	int width = first.Width;
        	TriStateMatrix maskedMatrix = new TriStateMatrix(width);
        	for(int x = 0; x < width; x++)
        	{
        		for(int y = 0; y < width; y++)
        		{
        			MatrixStatus states = first.MStatus(x, y);
        			switch(states)
        			{
        				case MatrixStatus.NoMask:
        					maskedMatrix[x, y, MatrixStatus.NoMask] = first[x, y];
        					break;
        				case MatrixStatus.Data:
        					maskedMatrix[x, y, MatrixStatus.Data] = first[x, y] ^ second[x, y];
        					break;
        				default:
        					throw new ArgumentException("TristateMatrix has None value cell.", "first");
        			}
        		}
        	}
        	
        	return maskedMatrix;
        }

        public static TriStateMatrix Apply(this TriStateMatrix matrix, Pattern pattern, ErrorCorrectionLevel errorlevel)
        {
            return matrix.Xor(pattern, errorlevel);
        }

        public static TriStateMatrix Apply(this Pattern pattern, TriStateMatrix matrix, ErrorCorrectionLevel errorlevel)
        {
            return matrix.Xor(pattern, errorlevel);
        }
    }
}
