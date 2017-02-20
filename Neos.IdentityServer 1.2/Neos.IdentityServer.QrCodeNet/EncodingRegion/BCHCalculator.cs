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

namespace Neos.IdentityServer.MultiFactor.QrEncoding.EncodingRegion
{
	internal static class BCHCalculator
	{
		/// <summary>
		/// Calculate int length by search for Most significant bit
		/// </summary>
		/// <param name="num">Input Number</param>
		/// <returns>Most significant bit</returns>
		internal static int PosMSB(int num)
		{
			return num == 0 ? 0 : BinarySearchPos(num, 0, 32) + 1;
		}
		
		/// <summary>
		/// Search for right side bit of Most significant bit
		/// </summary>
		/// <param name="num">input number</param>
		/// <param name="lowBoundary">lower boundary. At start should be 0</param>
		/// <param name="highBoundary">higher boundary. At start should be 32</param>
		/// <returns>Most significant bit - 1</returns>
		private static int BinarySearchPos(int num, int lowBoundary, int highBoundary)
		{
			int mid = (lowBoundary + highBoundary) / 2;
			int shiftResult = num >> mid;
			if(shiftResult == 1)
				return mid;
			else if(shiftResult < 1)
				return BinarySearchPos(num, lowBoundary, mid);
			else
				return BinarySearchPos(num, mid, highBoundary);
		}
		
		/// <summary>
		/// With input number and polynomial number. Method will calculate BCH value and return
		/// </summary>
		/// <param name="num">input number</param>
		/// <param name="poly">Polynomial number</param>
		/// <returns>BCH value</returns>
		internal static int CalculateBCH(int num, int poly)
		{
			int polyMSB = PosMSB(poly);
			//num's length will be old length + new length - 1. 
			//Once divide poly number. BCH number will be one length short than Poly number's length.
			num <<= (polyMSB - 1);
			int numMSB = PosMSB(num);
			while( PosMSB(num) >= polyMSB)
			{
				//left shift Poly number to same level as num. Then xor. 
				//Remove most significant bits of num.
				num ^= poly << (numMSB - polyMSB);
				numMSB = PosMSB(num);
			}
			return num;
		}
		
		
	}
}
