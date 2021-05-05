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
	public struct VersionDetail
	{
		internal int Version { get; private set; }
		internal int NumTotalBytes { get; private set; }
		internal int NumDataBytes { get; private set; }
		internal int NumECBlocks { get; private set; }
		
		internal VersionDetail(int version, int numTotalBytes, int numDataBytes, int numECBlocks)
			: this()
		{
			this.Version = version;
			this.NumTotalBytes = numTotalBytes;
			this.NumDataBytes = numDataBytes;
			this.NumECBlocks = numECBlocks;
		}
		
		/// <summary>
		/// Width for current version
		/// </summary>
		internal int MatrixWidth
		{
			get
			{
				return Width(this.Version);
			}
		}
		
		internal static int Width(int version)
		{
			return 17 + 4 * version;
		}
		
		/// <summary>
		/// number of Error correction blocks for group 1
		/// </summary>
		internal int ECBlockGroup1
		{
			get
			{
				return this.NumECBlocks - this.ECBlockGroup2;
			}
		}
		
		/// <summary>
		/// Number of error correction blocks for group 2
		/// </summary>
		internal int ECBlockGroup2
		{
			get
			{
				return this.NumTotalBytes % this.NumECBlocks;
			}
		}
		
		/// <summary>
		/// Number of data bytes per block for group 1
		/// </summary>
		internal int NumDataBytesGroup1
		{
			get
			{
				return this.NumDataBytes / this.NumECBlocks;
			}
		}
		
		/// <summary>
		/// Number of data bytes per block for group 2
		/// </summary>
		internal int NumDataBytesGroup2
		{
			get
			{
				return this.NumDataBytesGroup1 + 1;
			}
		}
		
		/// <summary>
		/// Number of error correction bytes per block
		/// </summary>
		internal int NumECBytesPerBlock
		{
			get
			{
				return (this.NumTotalBytes - this.NumDataBytes) / this.NumECBlocks;
			}
		}
		
		public override string ToString()
		{
			return this.Version + ";" + this.NumTotalBytes + ";" + this.NumDataBytes + ";" + this.NumECBlocks;
		}
		
	}
}
