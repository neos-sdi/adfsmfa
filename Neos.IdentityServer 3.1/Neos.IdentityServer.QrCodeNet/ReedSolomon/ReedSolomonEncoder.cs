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

namespace Neos.IdentityServer.MultiFactor.QrEncoding.ReedSolomon
{
	internal sealed class ReedSolomonEncoder
	{
		/// <summary>
		/// Encode an array of data codeword with GaloisField 256. 
		/// </summary>
		/// <param name="dataBytes">Array of data codewords for a single block.</param>
		/// <param name="numECBytes">Number of error correction codewords for data codewords</param>
		/// <param name="generatorPoly">Cached or newly create GeneratorPolynomial</param>
		/// <returns>Return error correction codewords array</returns>
		internal static byte[] Encode(byte[] dataBytes, int numECBytes, GeneratorPolynomial generatorPoly)
		{
			int dataLength = dataBytes.Length;
			if(generatorPoly == null)
				throw new ArgumentNullException("generator", "GeneratorPolynomial var is null");
			if(dataLength == 0)
				throw new ArgumentException("There is no data bytes to encode");
			if(numECBytes <= 0)
				throw new ArgumentException("No Error Correction bytes");
			int[] toEncode = ConvertToIntArray(dataBytes, dataLength, numECBytes);
			
			Polynomial generator = generatorPoly.GetGenerator(numECBytes);
			
			Polynomial dataPoly = new Polynomial(generator.GField, toEncode);
			
			PolyDivideStruct divideResult = dataPoly.Divide(generator);
			
			int[] remainderCoeffs = divideResult.Remainder.Coefficients;
			
			return ConvertTosByteArray(remainderCoeffs, numECBytes);
			
		}
		
		/// <summary>
		/// Convert data codewords to int array. And add error correction space at end of that array
		/// </summary>
		/// <param name="dataBytes">data codewords array</param>
		/// <param name="dataLength">data codewords length</param>
		/// <param name="numECBytes">Num of error correction bytes</param>
		/// <returns>Int array for data codewords array follow by error correction space</returns>
		private static int[] ConvertToIntArray(byte[] dataBytes, int dataLength, int numECBytes)
		{
			int[] resultArray = new int[dataLength + numECBytes];
			
			for(int index = 0; index < dataLength; index++)
			{
				resultArray[index] = dataBytes[index] & 0xff;
			}
			
			return resultArray;
		}
		
		/// <summary>
		/// Reassembly error correction codewords. As Polynomial class will eliminate zero monomial at front. 
		/// </summary>
		/// <param name="remainder">Remainder byte array after divide. </param>
		/// <param name="numECBytes">Error correction codewords length</param>
		/// <returns>Error correction codewords</returns>
		private static byte[] ConvertTosByteArray(int[] remainder, int numECBytes)
		{
			int remainderLength = remainder.Length;
			if(remainderLength > numECBytes)
				throw new ArgumentException("Num of remainder bytes can not larger than numECBytes");
			int numZeroCoeffs = numECBytes - remainderLength;
			
			byte[] resultArray = new byte[numECBytes];
			for(int index = 0; index < numZeroCoeffs; index++)
			{
				resultArray[index] = (byte)0;
			}
			
			for(int rIndex = 0; rIndex < remainderLength; rIndex++)
			{
				resultArray[numZeroCoeffs + rIndex] = (byte)remainder[rIndex];
			}
			
			return resultArray;
		}
		
	}
}
