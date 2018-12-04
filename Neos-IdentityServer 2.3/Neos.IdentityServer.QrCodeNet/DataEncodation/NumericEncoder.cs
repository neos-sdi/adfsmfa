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
using System.Collections;

namespace Neos.IdentityServer.MultiFactor.QrEncoding.DataEncodation
{
	/// <remarks>ISO/IEC 18004:2000 Chapter 8.4.2 Page 19</remarks>
    internal class NumericEncoder : EncoderBase
    {
        internal NumericEncoder() 
            : base()
        {
        }

        internal override Mode Mode
        {
            get { return Mode.Numeric; }
        }

        internal override BitList GetDataBits(string content)
        {
        	BitList dataBits = new BitList();
        	int contentLength = content.Length;
            for (int i = 0; i < contentLength; i += 3)
            {
                int groupLength = Math.Min(3, contentLength-i);
                int value = GetDigitGroupValue(content, i, groupLength);
                int bitCount = GetBitCountByGroupLength(groupLength);
                dataBits.Add(value, bitCount);
            }

            return dataBits;
        }
        
        
        protected override int GetBitCountInCharCountIndicator(int version)
        {
            return CharCountIndicatorTable.GetBitCountInCharCountIndicator(Mode.Numeric, version);
        }

        private int GetDigitGroupValue(string content, int startIndex, int length)
        {
            int value=0;
            int iThPowerOf10 = 1;
            for (int i = 0 ; i < length; i++)
            {
                int positionFromEnd = startIndex + length - i - 1;
                int digit = content[positionFromEnd] - '0';
                value += digit * iThPowerOf10;
                iThPowerOf10 *= 10;
            }
            return value;
        }
        
        private bool TryGetDigitGroupValue(string content, int startIndex, int length, out int value)
        {
            value=0;
            int iThPowerOf10 = 1;
            for (int i = 0 ; i < length; i++)
            {
                int positionFromEnd = startIndex + length - i - 1;
                int digit = content[positionFromEnd] - '0';
                //If not numeric. 
                if(digit < 0 || digit > 9)
                	return false;
                value += digit * iThPowerOf10;
                iThPowerOf10 *= 10;
            }
            return true;
        }

        protected int GetBitCountByGroupLength(int groupLength)
        {
            switch (groupLength)
            {
                case 0:
                    return 0;
                case 1:
                    return 4;
                case 2:
                    return 7;
                case 3:
                    return 10;
                default:
                    throw new InvalidOperationException("Unexpected group length:" + groupLength.ToString());
            }
        }
    }
}
