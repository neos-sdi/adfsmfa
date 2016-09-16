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
using System.Linq;
using System.Collections.Generic;
using Neos.IdentityServer.MultiFactor.QrEncoding.common;
using Neos.IdentityServer.MultiFactor.QrEncoding.Versions;
using Neos.IdentityServer.MultiFactor.QrEncoding.ReedSolomon;

namespace Neos.IdentityServer.MultiFactor.QrEncoding.ErrorCorrection
{
	internal static class ECGenerator
	{
		/// <summary>
		/// Generate error correction blocks. Then out put with codewords BitList
		/// ISO/IEC 18004/2006 P45, 46. Chapter 6.6 Constructing final message codewords sequence.
		/// </summary>
		/// <param name="dataCodewords">Datacodewords from DataEncodation.DataEncode</param>
		/// <param name="numTotalBytes">Total number of bytes</param>
		/// <param name="numDataBytes">Number of data bytes</param>
		/// <param name="numECBlocks">Number of Error Correction blocks</param>
		/// <returns>codewords BitList contain datacodewords and ECCodewords</returns>
		internal static BitList FillECCodewords(BitList dataCodewords, VersionDetail vd)
		{
			List<byte> dataCodewordsByte = dataCodewords.List;
			int ecBlockGroup2 = vd.ECBlockGroup2;
			int ecBlockGroup1 = vd.ECBlockGroup1;
			int numDataBytesGroup1 = vd.NumDataBytesGroup1;
			int numDataBytesGroup2 = vd.NumDataBytesGroup2;
			
			int ecBytesPerBlock = vd.NumECBytesPerBlock;
			
			int dataBytesOffset = 0;
			byte[][] dByteJArray = new byte[vd.NumECBlocks][];
			byte[][] ecByteJArray = new byte[vd.NumECBlocks][];
			
			GaloisField256 gf256 = GaloisField256.QRCodeGaloisField;
			GeneratorPolynomial generator = new GeneratorPolynomial(gf256);
			
			for(int blockID = 0; blockID < vd.NumECBlocks; blockID++)
			{
				if(blockID < ecBlockGroup1)
				{
					dByteJArray[blockID] = new byte[numDataBytesGroup1];
					for(int index = 0; index < numDataBytesGroup1; index++)
					{
						dByteJArray[blockID][index] = dataCodewordsByte[dataBytesOffset + index];
					}
					dataBytesOffset += numDataBytesGroup1;
				}
				else
				{
					dByteJArray[blockID] = new byte[numDataBytesGroup2];
					for(int index = 0; index < numDataBytesGroup2; index++)
					{
						dByteJArray[blockID][index] = dataCodewordsByte[dataBytesOffset + index];
					}
					dataBytesOffset += numDataBytesGroup2;
				}
				
				ecByteJArray[blockID] = ReedSolomonEncoder.Encode(dByteJArray[blockID], ecBytesPerBlock, generator);
			}
			if(vd.NumDataBytes != dataBytesOffset)
				throw new ArgumentException("Data bytes does not match offset");
			
			BitList codewords = new BitList();
			
			int maxDataLength = ecBlockGroup1 == vd.NumECBlocks ? numDataBytesGroup1 : numDataBytesGroup2;
		
			for(int dataID = 0; dataID < maxDataLength; dataID++)
			{
				for(int blockID = 0; blockID < vd.NumECBlocks; blockID++)
				{
					if( !(dataID == numDataBytesGroup1 && blockID < ecBlockGroup1) )
						codewords.Add((int)dByteJArray[blockID][dataID], 8);
				}
			}
			
			for(int ECID = 0; ECID < ecBytesPerBlock; ECID++)
			{
				for(int blockID = 0; blockID < vd.NumECBlocks; blockID++)
				{
					codewords.Add((int)ecByteJArray[blockID][ECID], 8);
				}
			}
			
			if( vd.NumTotalBytes != codewords.Count >> 3)
				throw new ArgumentException(string.Format("total bytes: {0}, actual bits: {1}", vd.NumTotalBytes, codewords.Count));
			
			return codewords;
			
		}
		
		
		
		
	}
}
