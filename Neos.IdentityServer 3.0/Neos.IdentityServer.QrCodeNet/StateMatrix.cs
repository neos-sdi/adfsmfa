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

namespace Neos.IdentityServer.MultiFactor.QrEncoding
{
	public sealed class StateMatrix
	{
		private MatrixStatus[,] m_matrixStatus;
		private readonly int m_Width;
		
		public StateMatrix(int width)
		{
			m_Width = width;
			m_matrixStatus = new MatrixStatus[width, width];
		}
		
		public MatrixStatus this[int x, int y]
		{
			get
			{
				return m_matrixStatus[x, y];
			}
			set
			{
				m_matrixStatus[x, y] = value;
			}
		}
		
		internal MatrixStatus this[MatrixPoint point]
		{
			get
			{ return this[point.X, point.Y]; }
			set
			{ this[point.X, point.Y] = value; }
		}
		
		public int Width
		{
			get { return m_Width; }
		}
		
		public int Height
		{
			get { return this.Width; }
		}
	}
}
