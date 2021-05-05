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

namespace Neos.IdentityServer.MultiFactor.QrEncoding.ReedSolomon
{
	/// <summary>
	/// Description of GaloisField256.
	/// </summary>
	internal sealed class GaloisField256
	{
		private int[] antiLogTable;
		private int[] logTable;
		
		private readonly int m_primitive;
		
		internal int Primitive
		{
			get
			{
				return m_primitive;
			}
		}
		
		
		internal GaloisField256(int primitive)
		{
			antiLogTable = new int[256];
			logTable = new int[256];
			
			m_primitive = primitive;
			
			int gfx = 1;
			//Power cycle is from 0 to 254. 2^255 = 1 = 2^0 
			//Value cycle is from 1 to 255. Thus there should not have Log(0).			
			for(int powers = 0; powers < 256; powers++)
			{
				antiLogTable[powers] = gfx;
				if(powers != 255)
					logTable[gfx] = powers;
				gfx <<= 1;		//gfx = gfx * 2 where alpha is 2.
				
				if(gfx > 255)
				{
					gfx ^= primitive;
				}
			}
		}
		
		internal static GaloisField256 QRCodeGaloisField
		{
			get
			{
				return new GaloisField256(QRCodeConstantVariable.QRCodePrimitive);
			}
		}
		
		/// <returns>
		/// Powers of a in GF table. Where a = 2
		/// </returns>
		internal int Exponent(int PowersOfa)
		{
			return antiLogTable[PowersOfa];
		}
		
		/// <returns>
		/// log ( power of a) in GF table. Where a = 2
		/// </returns>
		internal int Log(int gfValue)
		{
			if( gfValue == 0)
				throw new ArgumentException("GaloisField value will not equal to 0, Log method");
			return logTable[gfValue];
		}
		
		internal int inverse(int gfValue)
		{
			if( gfValue == 0 )
				throw new ArgumentException("GaloisField value will not equal to 0, Inverse method");
			return this.Exponent(255 - this.Log(gfValue));
		}
		
		internal int Addition(int gfValueA, int gfValueB)
		{
			return gfValueA ^ gfValueB;
		}
		
		internal int Subtraction(int gfValueA, int gfValueB)
		{
			//Subtraction is same as addition. 
			return this.Addition(gfValueA, gfValueB);
		}
		
		/// <returns>
		/// Product of two values. 
		/// In other words. a multiply b
		/// </returns>
		internal int Product(int gfValueA, int gfValueB)
		{
			if (gfValueA == 0 || gfValueB == 0)
			{
				return 0;
			}
			if (gfValueA == 1)
			{
				return gfValueB;
			}
			if (gfValueB == 1)
			{
				return gfValueA;
			}
			
			return Exponent((Log(gfValueA) + Log(gfValueB)) % 255);
		}
		
		/// <returns>
		/// Quotient of two values. 
		/// In other words. a devided b
		/// </returns>
		internal int Quotient(int gfValueA, int gfValueB)
		{
			if(gfValueA == 0)
				return 0;
			if(gfValueB == 0)
				throw new ArgumentException("gfValueB can not be zero");
			if(gfValueB == 1)
				return gfValueA;
			return Exponent(Math.Abs(Log(gfValueA) - Log(gfValueB)) % 255);
		}
		
	}
}
