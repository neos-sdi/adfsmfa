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
	internal class Penalty2 : Penalty
    {
        
        internal override int PenaltyCalculate(BitMatrix matrix)
        {
        	int width = matrix.Width;
        	bool topR = false;
        	
        	int x = 0;
        	int y = 0;
        	int penalty = 0;
        	
        	while( y < (width - 1))
        	{
        		while( x < (width - 1))
        		{
        			topR = matrix[x + 1, y];
        			
        			if(topR == matrix[x + 1, y + 1])	//Bottom Right
        			{
        				if(topR == matrix[x, y + 1])	//Bottom Left
        				{
        					if(topR == matrix[x, y])	//Top Left
        					{
        						penalty += 3;
        						x += 1;
        					}
        					else
        						x += 1;
        					
        				}
        				else
        					x += 1;
        			}
        			else
        			{
        				x += 2;
        			}
        		}
        		
        		x = 0;
        		y ++;
        	}
        	return penalty;
        }
        
    }
}

