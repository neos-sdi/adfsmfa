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
using Neos.IdentityServer.MultiFactor.QrEncoding.Positioning;

namespace Neos.IdentityServer.MultiFactor.QrEncoding.EncodingRegion
{
	/// <summary>
    /// Embed version information for version larger or equal to 7. 
    /// </summary>
    /// <remarks>ISO/IEC 18004:2000 Chapter 8.10 Page 54</remarks>
	internal static class VersionInformation
	{
		private const int s_VIRectangleHeight = 3;
		private const int s_VIRectangleWidth = 6;
		
		/// <summary>
		/// Embed version information to Matrix
		/// Only for version greater or equal to 7
		/// </summary>
		/// <param name="tsMatrix">Matrix</param>
		/// <param name="version">version number</param>
		internal static void EmbedVersionInformation(this TriStateMatrix tsMatrix, int version)
		{
			if(version < 7)
				return;
			BitList versionInfo = VersionInfoBitList(version);
			
			int matrixWidth = tsMatrix.Width;
			//1 cell between version info and position stencil
			int shiftLength = QRCodeConstantVariable.PositionStencilWidth + s_VIRectangleHeight + 1;
			//Reverse order input
			int viIndex = s_LengthDataBits + s_LengthECBits - 1;
			
			for(int viWidth = 0; viWidth < s_VIRectangleWidth; viWidth++)
			{
				for(int viHeight = 0; viHeight < s_VIRectangleHeight; viHeight++)
				{
					bool bit = versionInfo[viIndex];
					viIndex--;
					//Bottom left
					tsMatrix[viWidth, (matrixWidth - shiftLength + viHeight), MatrixStatus.NoMask] = bit;
					//Top right
					tsMatrix[(matrixWidth - shiftLength + viHeight), viWidth, MatrixStatus.NoMask] = bit;
				}
			}
			
		}
		
		private const int s_LengthDataBits = 6;
		private const int s_LengthECBits = 12;
		private const int s_VersionBCHPoly = 0x1f25;
		
		private static BitList VersionInfoBitList(int version)
		{
			BitList result = new BitList();
			result.Add(version, s_LengthDataBits);
			result.Add(BCHCalculator.CalculateBCH(version, s_VersionBCHPoly), s_LengthECBits);
			
			if(result.Count != (s_LengthECBits + s_LengthDataBits))
				throw new Exception("Version Info creation error. Result is not 18 bits");
			return result;
		}
	}
}
