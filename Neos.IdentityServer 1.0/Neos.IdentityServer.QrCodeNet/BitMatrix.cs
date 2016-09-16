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

using Neos.IdentityServer.MultiFactor.QrEncoding.Positioning;

namespace Neos.IdentityServer.MultiFactor.QrEncoding
{
    public abstract class BitMatrix
    {
        public abstract bool this[int i, int j] { get; set; }
        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract bool[,] InternalArray { get; }

        internal MatrixSize Size
        {
            get { return new MatrixSize(Width, Height); }
        }

        internal bool this[MatrixPoint point]
        {
            get { return this[point.X, point.Y]; }
            set { this[point.X, point.Y] = value; }
        }

        internal void CopyTo(TriStateMatrix target, MatrixRectangle sourceArea, MatrixPoint targetPoint, MatrixStatus mstatus)
        {
            for (int j = 0; j < sourceArea.Size.Height; j++)
            {
                for (int i = 0; i < sourceArea.Size.Width; i++)
                {
                    bool value = this[sourceArea.Location.X + i, sourceArea.Location.Y + j];
                    target[targetPoint.X + i, targetPoint.Y + j, mstatus] = value;
                }
            }
        }

        internal void CopyTo(TriStateMatrix target, MatrixPoint targetPoint, MatrixStatus mstatus)
        {
            CopyTo(target, new MatrixRectangle(new MatrixPoint(0,0), new MatrixSize(Width, Height)), targetPoint, mstatus);
        }
    }
}
