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

namespace Neos.IdentityServer.MultiFactor.QrEncoding.common
{
	internal static class BitListExtensions
	{
		internal static byte[] ToByteArray(this BitList bitList)
		{
			int bitLength = bitList.Count;
			if((bitLength & 0x7) != 0)
				throw new ArgumentException("bitList count % 8 is not equal to zero");
			
			int numByte = bitLength >> 3;
			
			byte[] result = new byte[numByte];
			
			for(int bitIndex = 0; bitIndex < bitLength; bitIndex++)
			{
				int numBitsInLastByte = bitIndex & 0x7;
				
				if(numBitsInLastByte == 0)
					result[bitIndex >> 3] = 0;
				result[bitIndex >> 3] |= (byte)(ToBit(bitList[bitIndex]) << InverseShiftValue(numBitsInLastByte));
			}
			
			return result;
			
		}
		
		internal static BitList ToBitList(byte[] bArray)
		{
			int bLength = bArray.Length;
			BitList result = new BitList();
			for(int bIndex = 0; bIndex < bLength; bIndex++)
			{
				result.Add((int)bArray[bIndex], 8);
			}
			return result;
		}
		
		private static int ToBit(bool bit)
		{
			switch(bit)
			{
				case true:
					return 1;
				case false:
					return 0;
				default:
					throw new ArgumentException("Invalide bit value");
			}
		}
		
		private static int InverseShiftValue(int numBitsInLastByte)
		{
			return 7 - numBitsInLastByte;
		}
		
	}
}
