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

namespace Neos.IdentityServer.MultiFactor.QrEncoding.Masking.Scoring
{
	/// <summary>
	/// ISO/IEC 18004:2000 Chapter 8.8.2 Page 52
	/// </summary>
	internal class Penalty4 : Penalty
	{
		/// <summary>
		/// Calculate penalty value for Fourth rule.
		/// Perform O(n) search for available x modules
		/// </summary>
		internal override int PenaltyCalculate(BitMatrix matrix)
		{
			int width = matrix.Width;
			int DarkBitCount = 0;
			
			for(int j = 0; j < width; j++)
			{
				for(int i = 0; i < width; i++)
				{
					if(matrix[i, j])
						DarkBitCount++;
				}
			}
			
			int MatrixCount = width * width;
			
			double ratio = (double)DarkBitCount / MatrixCount;
			
			return System.Math.Abs((int)(ratio*100 -50)) / 5 * 10;
			
		}
	}
}
