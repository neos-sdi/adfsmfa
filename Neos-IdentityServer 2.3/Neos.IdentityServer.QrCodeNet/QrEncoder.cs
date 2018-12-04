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
using System.Collections.Generic;

namespace Neos.IdentityServer.MultiFactor.QrEncoding
{
    public class QrEncoder
    {
        public ErrorCorrectionLevel ErrorCorrectionLevel { get; set; }

        /// <summary>
        /// Default QrEncoder will set ErrorCorrectionLevel as M
        /// </summary>
        public QrEncoder()
            : this(ErrorCorrectionLevel.M)
        {

        }

        /// <summary>
        /// QrEncoder with parameter ErrorCorrectionLevel. 
        /// </summary>
        /// <param name="errorCorrectionLevel"></param>
        public QrEncoder(ErrorCorrectionLevel errorCorrectionLevel)
        {
            ErrorCorrectionLevel = errorCorrectionLevel;
        }

        /// <summary>
        /// Encode string content to QrCode matrix
        /// </summary>
        /// <exception cref="InputOutOfBoundaryException">
        /// This exception for string content is null, empty or too large</exception>
        public QrCode Encode(string content)
        {
        	if(string.IsNullOrEmpty(content))
        	{
        		throw new InputOutOfBoundaryException("Input should not be empty or null");
        	}
        	else
        		return new QrCode(QRCodeEncode.Encode(content, ErrorCorrectionLevel));
        }
        
        /// <summary>
        /// Try to encode content
        /// </summary>
        /// <returns>False if input content is empty, null or too large.</returns>
        public bool TryEncode(string content, out QrCode qrCode)
        {
        	try
        	{
        		qrCode = this.Encode(content);
        		return true;
        	}
        	catch(InputOutOfBoundaryException)
        	{
        		qrCode = new QrCode();
        		return false;
        	}
        }

        /// <summary>
        /// Encode byte array content to QrCode matrix
        /// </summary>
        /// <exception cref="InputOutOfBoundaryException">
        /// This exception for string content is null, empty or too large</exception>
        public QrCode Encode(IEnumerable<byte> content)
        {
            if (content == null)
            {
                throw new InputOutOfBoundaryException("Input should not be empty or null");
            }
            else
                return new QrCode(QRCodeEncode.Encode(content, ErrorCorrectionLevel));
        }

        /// <summary>
        /// Try to encode content
        /// </summary>
        /// <returns>False if input content is empty, null or too large.</returns>
        public bool TryEncode(IEnumerable<byte> content, out QrCode qrCode)
        {
            try
            {
                qrCode = this.Encode(content);
                return true;
            }
            catch (InputOutOfBoundaryException)
            {
                qrCode = new QrCode();
                return false;
            }
        }

    }
}
