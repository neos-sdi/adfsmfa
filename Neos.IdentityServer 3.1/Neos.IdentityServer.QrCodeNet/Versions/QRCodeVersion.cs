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

namespace Neos.IdentityServer.MultiFactor.QrEncoding.Versions
{
	internal struct QRCodeVersion
	{
		internal int VersionNum { get; private set;}
		
		internal int TotalCodewords { get; private set;}
		
		internal int DimensionForVersion { get; private set;}
		
		private ErrorCorrectionBlocks[] m_ECBlocks;
		
		/// <param name="ecblocks1">L</param>
		/// <param name="ecblocks2">M</param>
		/// <param name="ecblocks3">Q</param>
		/// <param name="ecblocks4">H</param>
		internal QRCodeVersion(int versionNum, int totalCodewords, ErrorCorrectionBlocks ecblocksL, ErrorCorrectionBlocks ecblocksM, ErrorCorrectionBlocks ecblocksQ, ErrorCorrectionBlocks ecblocksH)
			: this()
		{
			this.VersionNum = versionNum;
			this.TotalCodewords = totalCodewords;
			this.m_ECBlocks = new ErrorCorrectionBlocks[]{ecblocksL, ecblocksM, ecblocksQ, ecblocksH};
			this.DimensionForVersion = 17 + versionNum * 4;
		}
		
		/// <summary>
		/// Get Error Correction Blocks by level
		/// </summary>
		//[method
		internal ErrorCorrectionBlocks GetECBlocksByLevel(Neos.IdentityServer.MultiFactor.QrEncoding.ErrorCorrectionLevel ECLevel)
		{
			switch(ECLevel)
			{
				case ErrorCorrectionLevel.L:
					return m_ECBlocks[0];
				case ErrorCorrectionLevel.M:
					return m_ECBlocks[1];
				case ErrorCorrectionLevel.Q:
					return m_ECBlocks[2];
				case ErrorCorrectionLevel.H:
					return m_ECBlocks[3];
				default:
					throw new System.ArgumentOutOfRangeException("Invalide ErrorCorrectionLevel");
			}
			
		}
		
	}
}
