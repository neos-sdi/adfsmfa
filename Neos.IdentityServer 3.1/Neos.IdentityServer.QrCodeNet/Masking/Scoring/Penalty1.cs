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
	internal class Penalty1 : Penalty
    {

		/// <summary>
		/// Calculate penalty value for first rule.
		/// </summary>
        internal override int PenaltyCalculate(BitMatrix matrix)
        {
            MatrixSize size = matrix.Size;
            int penaltyValue = 0;

            penaltyValue = PenaltyCalculation(matrix, true) + PenaltyCalculation(matrix, false);
            return penaltyValue;
        }

        
        private int PenaltyCalculation(BitMatrix matrix, bool isHorizontal)
        {
            int penalty = 0;
            int numSameBitCell = 0;

            int width = matrix.Width;

            int i = 0;
            int j = 0;

            while (i < width)
            {
                while (j < width - 4)
                {
                    bool preBit = isHorizontal ? matrix[j + 4, i]
                        : matrix[i, j + 4];
                    numSameBitCell = 1;

                    for (int x = 1; x <= 4; x++)
                    {
                        bool bit = isHorizontal ? matrix[j + 4 - x, i]
                            : matrix[i, j + 4 - x];
                        if (bit == preBit)
                        {
                            numSameBitCell++;
                        }
                        else
                        {
                            break;
                        }

                    }

                    if (numSameBitCell == 1)
                        j += 4;
                    else
                    {
                        int x = 5;
                        while ((j + x) < width)
                        {
                            bool bit = isHorizontal ? matrix[j + x, i]
                                : matrix[i, j + x];
                            if (bit == preBit)
                                numSameBitCell++;
                            else
                            {
                                break;
                            }
                            x++;
                        }
                        if (numSameBitCell >= 5)
                        {
                            penalty += (3 + (numSameBitCell - 5));
                        }

                        j += x;
                    }

                }
                j = 0;
                i++;
            }

            return penalty;
        }

        

    }
}
