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
using Neos.IdentityServer.MultiFactor.QrEncoding.DataEncodation.InputRecognition;
using Neos.IdentityServer.MultiFactor.QrEncoding.Versions;
using Neos.IdentityServer.MultiFactor.QrEncoding.Terminate;
using System.Collections.Generic;

namespace Neos.IdentityServer.MultiFactor.QrEncoding.DataEncodation
{
	/// <remarks>ISO/IEC 18004:2000 Chapter 8.1 Page 14
	/// DataEncode is combination of Data analysis and Data encodation step.
	/// Which uses sub functions under several different namespaces</remarks>
	internal static class DataEncode
	{
		internal static EncodationStruct Encode(string content, ErrorCorrectionLevel ecLevel)
		{
			RecognitionStruct recognitionResult = InputRecognise.Recognise(content);
			EncoderBase encoderBase = CreateEncoder(recognitionResult.Mode, recognitionResult.EncodingName);
			
			BitList encodeContent = encoderBase.GetDataBits(content);
			
			int encodeContentLength = encodeContent.Count;
			
			VersionControlStruct vcStruct = 
				VersionControl.InitialSetup(encodeContentLength, recognitionResult.Mode, ecLevel, recognitionResult.EncodingName);
			
			BitList dataCodewords = new BitList();
			//Eci header
			if(vcStruct.isContainECI && vcStruct.ECIHeader != null)
				dataCodewords.Add(vcStruct.ECIHeader);
			//Header
			dataCodewords.Add(encoderBase.GetModeIndicator());
			int numLetter = recognitionResult.Mode == Mode.EightBitByte ? encodeContentLength >> 3 : content.Length;
			dataCodewords.Add(encoderBase.GetCharCountIndicator(numLetter, vcStruct.VersionDetail.Version));
			//Data
			dataCodewords.Add(encodeContent);
			//Terminator Padding
			dataCodewords.TerminateBites(dataCodewords.Count, vcStruct.VersionDetail.NumDataBytes);
			
			int dataCodewordsCount = dataCodewords.Count;
			if((dataCodewordsCount & 0x7) != 0)
				throw new ArgumentException("data codewords is not byte sized.");
			else if(dataCodewordsCount >> 3 != vcStruct.VersionDetail.NumDataBytes)
			{
				throw new ArgumentException("datacodewords num of bytes not equal to NumDataBytes for current version");
			}
			
			EncodationStruct encStruct = new EncodationStruct(vcStruct);
			encStruct.Mode = recognitionResult.Mode;
			encStruct.DataCodewords = dataCodewords;
			return encStruct;
		}


        internal static EncodationStruct Encode(IEnumerable<byte> content, ErrorCorrectionLevel eclevel)
        {
            EncoderBase encoderBase = CreateEncoder(Mode.EightBitByte, QRCodeConstantVariable.DefaultEncoding);

            BitList encodeContent = new BitList(content);

            int encodeContentLength = encodeContent.Count;

            VersionControlStruct vcStruct =
                VersionControl.InitialSetup(encodeContentLength, Mode.EightBitByte, eclevel, QRCodeConstantVariable.DefaultEncoding);

            BitList dataCodewords = new BitList();
            //Eci header
            if (vcStruct.isContainECI && vcStruct.ECIHeader != null)
                dataCodewords.Add(vcStruct.ECIHeader);
            //Header
            dataCodewords.Add(encoderBase.GetModeIndicator());
            int numLetter = encodeContentLength >> 3;
            dataCodewords.Add(encoderBase.GetCharCountIndicator(numLetter, vcStruct.VersionDetail.Version));
            //Data
            dataCodewords.Add(encodeContent);
            //Terminator Padding
            dataCodewords.TerminateBites(dataCodewords.Count, vcStruct.VersionDetail.NumDataBytes);

            int dataCodewordsCount = dataCodewords.Count;
            if ((dataCodewordsCount & 0x7) != 0)
                throw new ArgumentException("data codewords is not byte sized.");
            else if (dataCodewordsCount >> 3 != vcStruct.VersionDetail.NumDataBytes)
            {
                throw new ArgumentException("datacodewords num of bytes not equal to NumDataBytes for current version");
            }

            EncodationStruct encStruct = new EncodationStruct(vcStruct);
            encStruct.Mode = Mode.EightBitByte;
            encStruct.DataCodewords = dataCodewords;
            return encStruct;
        }
		
		private static EncoderBase CreateEncoder(Mode mode, string encodingName)
		{
			switch(mode)
			{
				case Mode.Numeric:
					return new NumericEncoder();
				case Mode.Alphanumeric:
					return new AlphanumericEncoder();
				case Mode.EightBitByte:
					return new EightBitByteEncoder(encodingName);
				case Mode.Kanji:
					return new KanjiEncoder();
				default:
					throw new ArgumentOutOfRangeException("mode", string.Format("Doesn't contain encoder for {0}", mode));
			}
		}
	}
}
