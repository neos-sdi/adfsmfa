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
using Neos.IdentityServer.MultiFactor.QrEncoding.Versions;

namespace Neos.IdentityServer.MultiFactor.QrEncoding.DataEncodation.InputRecognition
{
	public static class InputRecognise
	{
		
		/// <summary>
		/// Use to recognise which mode and encoding name to use for input string. 
		/// </summary>
		/// <param name="content">input string content</param>
		/// <param name="encodingName">Output encoding name</param>
		/// <returns>Mode and Encoding name</returns>
		public static RecognitionStruct Recognise(string content)
		{
			int contentLength = content.Length;
			
			int tryEncodePos = ModeEncodeCheck.TryEncodeKanji(content, contentLength);
			
			if(tryEncodePos == -2)
				return new RecognitionStruct(Mode.EightBitByte, QRCodeConstantVariable.UTF8Encoding);
			else if(tryEncodePos == -1)
				return new RecognitionStruct(Mode.Kanji, QRCodeConstantVariable.DefaultEncoding);
			
			tryEncodePos = ModeEncodeCheck.TryEncodeAlphaNum(content, 0, contentLength);
			
			if(tryEncodePos == -2)
				return new RecognitionStruct(Mode.Numeric, QRCodeConstantVariable.DefaultEncoding);
			else if(tryEncodePos == -1)
				return new RecognitionStruct(Mode.Alphanumeric, QRCodeConstantVariable.DefaultEncoding);
			
			
			string encodingName = EightBitByteRecognision(content, tryEncodePos, contentLength);
			return new RecognitionStruct(Mode.EightBitByte, encodingName);
		}
		
		private static string EightBitByteRecognision(string content, int startPos, int contentLength)
		{
			if(string.IsNullOrEmpty(content))
				throw new ArgumentNullException("content", "Input content is null or empty");
			
			ECISet eciSets = new ECISet(ECISet.AppendOption.NameToValue);
			
			Dictionary<string, int> eciSet = eciSets.GetECITable();
			
			
			//we will not check for utf8 encoding. 
			eciSet.Remove(QRCodeConstantVariable.UTF8Encoding);
			eciSet.Remove(QRCodeConstantVariable.DefaultEncoding);
			
			int scanPos = startPos;
			//default encoding as priority
			scanPos = ModeEncodeCheck.TryEncodeEightBitByte(content, QRCodeConstantVariable.DefaultEncoding, scanPos, contentLength);
			if(scanPos == -1)
				return QRCodeConstantVariable.DefaultEncoding;
			
			foreach(KeyValuePair<string, int> kvp in eciSet)
			{
				scanPos = ModeEncodeCheck.TryEncodeEightBitByte(content, kvp.Key, scanPos, contentLength);
				if(scanPos == -1)
				{
					return kvp.Key;
				}

			}
			
			if(scanPos == -1)
				throw new ArgumentException("foreach Loop check give wrong result.");
			else
				return QRCodeConstantVariable.UTF8Encoding;
			
		}
		
	}
}
