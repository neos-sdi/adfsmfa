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
	internal class Penalty3 : Penalty
    {
        /// <summary>
		/// Calculate penalty value for Third rule.
		/// </summary>
        internal override int PenaltyCalculate(BitMatrix matrix)
        {
            return PenaltyCalculation(matrix, true) + PenaltyCalculation(matrix, false);
        }
        
        private int PenaltyCalculation(BitMatrix matrix, bool isHorizontal)
        {
        	int i = 0;
        	int j = 1;
        	int penalty = 0;
        	int width = matrix.Width;
        	bool bit;
        	while(i < width)
        	{
        		while(j < width - 5)
        		{
        			
        			bit = isHorizontal ? matrix[j + 4, i]
        				: matrix[i, j + 4];
        			if(!bit)
        			{
        				bit = isHorizontal ? matrix[j, i]
        					: matrix[i, j];
        				if(!bit)
        				{
        					penalty += PatternCheck(matrix, i, j, isHorizontal);
        					j += 4;
        				}
        				else
        					j += 4;
        			}
        			else
        			{
        				for(int num = 4; num > 0; num--)
        				{
        					bit = bit = isHorizontal ? matrix[j + num, i]
        						: matrix[i, j + num];
        					if(!bit)
        					{
        						j += num;
        						break;
        					}
        					if(num == 1)
        						j += 5;
        				}
        			}
        		}
        		j = 0;
        		i++;
        	}
        	return penalty;
        }
        
        private int PatternCheck(BitMatrix matrix, int i, int j, bool isHorizontal)
        {
        	bool bit;
        	for(int num = 3; num >= 1; num--)
        	{
        		bit = isHorizontal ? matrix[j + num, i]
        			: matrix[i, j + num];
        		if(!bit)
        			return 0;
        	}
        	//Check for left side and right side x ( xoxxxox ).
        	if((j - 1) < 0 || (j + 1) >= matrix.Width)
        		return 0;
        	bit = isHorizontal ? matrix[j + 5, i]
        		: matrix[i, j + 5];
        	if(!bit)
        		return 0;
        	bit = isHorizontal ? matrix[j - 1, i]
        		: matrix[i, j - 1];
        	if(!bit)
        		return 0;
        	
        	if((j - 5) >= 0)
        	{
        		for(int num = -2; num >= -5; num--)
        		{
        			bit = isHorizontal ? matrix[j + num, i]
        				: matrix[i, j + num];
        			if(bit)
        				break;
        			if(num == -5)
        				return 40;
        		}
        	}
        	
        	if((j + 9) < matrix.Width)
        	{
        		for(int num = 6; num <= 9; num++)
        		{
        			bit = isHorizontal ? matrix[j + num, i]
        				: matrix[i, j + num];
        			if(bit)
        				return 0;
        		}
        		return 40;
        	}
        	else
        		return 0;
        	
        }

    }
}
