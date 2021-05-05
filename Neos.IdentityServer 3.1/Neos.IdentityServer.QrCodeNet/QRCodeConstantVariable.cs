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

namespace Neos.IdentityServer.MultiFactor.QrEncoding
{
	/// <summary>
	/// Contain most of common constant variables. S
	/// </summary>
    public static class QRCodeConstantVariable
    {
        public const int MinVersion = 1;
        public const int MaxVersion = 40;
        
        public const string DefaultEncoding = "iso-8859-1"; 
        public const string UTF8Encoding = "utf-8";
        
        /// <summary>
		/// ISO/IEC 18004:2006(E) Page 45 Chapter Generating the error correction codewords
		/// Primative Polynomial = Bin 100011101 = Dec 285
		/// </summary>
        public const int QRCodePrimitive = 285;
        
        internal const int TerminatorNPaddingBit = 0;
        
        internal const int TerminatorLength = 4;
        
        /// <summary>
        /// 0xEC
        /// </summary>
        internal const int PadeCodewordsOdd = 0xec;
        internal static bool[] PadeOdd = new bool[]{true, true, true, false, 
        	                                        true, true, false, false};
        
        /// <summary>
        /// 0x11
        /// </summary>
        internal const int PadeCodewordsEven = 0x11;
        internal static bool[] PadeEven = new bool[]{false, false, false, true, 
        	                                          false, false, false, true};
        /// <summary>
        /// URL:http://en.wikipedia.org/wiki/Byte-order_mark
        /// </summary>
        public static byte[] UTF8ByteOrderMark
        {
        	get
        	{
        		return new byte[]{0xEF, 0xBB, 0xBF};
        	}
        }
        
        internal const int PositionStencilWidth = 7;
        
        
    }
}
