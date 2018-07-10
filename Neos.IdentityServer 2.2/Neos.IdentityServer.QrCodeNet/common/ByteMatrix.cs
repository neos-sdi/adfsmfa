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

namespace Neos.IdentityServer.MultiFactor.QrEncoding.Common
{
    public sealed class ByteMatrix
    {
        private readonly sbyte[,] m_Bytes;
        
        internal sbyte this[int x, int y]
        {
            get { return m_Bytes[y, x]; }
            set { m_Bytes[y, x] = value; }
        }
        
        internal int Width
        {
            get { return m_Bytes.GetLength(1); }
        }

        internal int Height
        {
            get { return m_Bytes.GetLength(0); }
        }

        internal ByteMatrix(int width, int height)
        {
            m_Bytes = new sbyte[height, width];
        }

        internal void Clear(sbyte value)
        {
            this.ForAll((x, y, matrix) => { matrix[x, y] = value; });
        }

        internal void ForAll(Action<int, int, ByteMatrix> action)
        {
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    action.Invoke(x, y, this);
                }
            }
        }
    }
}