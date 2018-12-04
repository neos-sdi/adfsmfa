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

namespace Neos.IdentityServer.MultiFactor.QrEncoding.DataEncodation
{
	/// <summary>
	/// ISO/IEC 18004:2000 Chapter 8.4.3 Alphanumeric Page 21
	/// </summary>
    internal class AlphanumericEncoder : EncoderBase
    {
        internal AlphanumericEncoder() 
            : base()
        {
        }

        internal override Mode Mode
        {
            get { return Mode.Alphanumeric; }
        }

        internal override BitList GetDataBits(string content)
        {
        	BitList dataBits = new BitList();
        	int contentLength = content.Length;
            for (int i = 0; i < contentLength; i += 2)
            {
                int groupLength = Math.Min(2, contentLength-i);
                int value = GetAlphaNumValue(content, i, groupLength);
                int bitCount = GetBitCountByGroupLength(groupLength);
                dataBits.Add(value, bitCount);
            }
			return dataBits;
        }
        
    	
    	/// <summary>
    	/// Constant from Chapter 8.4.3 Alphanumeric Mode. P.21
    	/// </summary>
    	private const int s_MultiplyFirstChar = 45;
    	
        private static int GetAlphaNumValue(string content, int startIndex, int length)
        {
        	int value = 0;
        	int iMultiplyValue = 1;
        	for (int i = 0 ; i < length; i++)
        	{
        		int positionFromEnd = startIndex + length - i - 1;
        	    int code = AlphanumericTable.ConvertAlphaNumChar(content[positionFromEnd]);
        	    value += code * iMultiplyValue;
        		iMultiplyValue *= s_MultiplyFirstChar;
        	}
        	return value;
        }
        

        protected override int GetBitCountInCharCountIndicator(int version)
        {
            return CharCountIndicatorTable.GetBitCountInCharCountIndicator(Mode.Alphanumeric, version);
        }
        
        /// <summary>
        /// BitCount from chapter 8.4.3. P22
        /// </summary>
        protected int GetBitCountByGroupLength(int groupLength)
        {
            switch (groupLength)
            {
                case 0:
                    return 0;
                case 1:
                    return 6;
                case 2:
                    return 11;
                default:
                    throw new InvalidOperationException(string.Format("Unexpected group length {0}", groupLength));
            }
        }
    }
}