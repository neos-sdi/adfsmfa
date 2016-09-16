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
    public class SquareBitMatrix : BitMatrix
    {
        private readonly bool[,] m_InternalArray;

        private readonly int m_Width;

        public SquareBitMatrix(int width)
        {
            m_InternalArray = new bool[width, width];
            m_Width = width;
        }

        internal SquareBitMatrix(bool[,] internalArray)
        {
            m_InternalArray = internalArray;
            int width = internalArray.GetLength(0);
            m_Width = width;
        }

        public static bool CreateSquareBitMatrix(bool[,] internalArray, out SquareBitMatrix triStateMatrix)
        {
            triStateMatrix = null;
            if (internalArray == null)
                return false;

            if (internalArray.GetLength(0) == internalArray.GetLength(1))
            {
                triStateMatrix = new SquareBitMatrix(internalArray);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Return value will be internal array itself. Not deep/shallow copy. 
        /// </summary>
        public override bool[,] InternalArray
        {
            get 
            {
                bool[,] deepCopyArray = new bool[m_Width, m_Width];
                for (int x = 0; x < m_Width; x++)
                    for (int y = 0; y < m_Width; y++)
                        deepCopyArray[x, y] = m_InternalArray[x, y];
                return deepCopyArray;
            }
        }

        

        public override bool this[int i, int j]
        {
            get
            {
                return m_InternalArray[i, j];
            }
            set
            {
                m_InternalArray[i, j] = value;
            }
        }
       
         public override int Height
        {
            get { return Width; }
        }

        public override int Width
        {
            get { return m_Width; }
        }
    }
}
