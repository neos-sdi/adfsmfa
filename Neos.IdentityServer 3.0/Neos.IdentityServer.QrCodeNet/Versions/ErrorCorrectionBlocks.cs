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
	internal struct ErrorCorrectionBlocks
	{
		internal int NumErrorCorrectionCodewards { get; private set; }
		
		internal int NumBlocks { get; private set; }
		
		internal int ErrorCorrectionCodewordsPerBlock { get; private set;}
		
		private ErrorCorrectionBlock[] m_ECBlock;
		
		internal ErrorCorrectionBlocks(int numErrorCorrectionCodewords, ErrorCorrectionBlock ecBlock)
			: this()
		{
			this.NumErrorCorrectionCodewards = numErrorCorrectionCodewords;
			this.m_ECBlock = new ErrorCorrectionBlock[]{ecBlock};
			
			this.initialize();
		}
		
		internal ErrorCorrectionBlocks(int numErrorCorrectionCodewords, ErrorCorrectionBlock ecBlock1, ErrorCorrectionBlock ecBlock2)
			: this()
		{
			this.NumErrorCorrectionCodewards = numErrorCorrectionCodewords;
			this.m_ECBlock = new ErrorCorrectionBlock[]{ecBlock1, ecBlock2};
			
			this.initialize();
		}
		
		/// <summary>
		/// Get Error Correction Blocks
		/// </summary>
		internal ErrorCorrectionBlock[] GetECBlocks()
		{
			return m_ECBlock;
		}
		
		/// <summary>
		/// Initialize for NumBlocks and ErrorCorrectionCodewordsPerBlock
		/// </summary>
		private void initialize()
		{
			if(m_ECBlock == null)
				throw new System.ArgumentNullException("ErrorCorrectionBlocks array doesn't contain any value");
			
			NumBlocks = 0;
			int blockLength = m_ECBlock.Length;
			for(int i = 0; i < blockLength; i++)
			{
				NumBlocks += m_ECBlock[i].NumErrorCorrectionBlock;
			}
			
			
			ErrorCorrectionCodewordsPerBlock = NumErrorCorrectionCodewards / NumBlocks;
		}
	}
}
