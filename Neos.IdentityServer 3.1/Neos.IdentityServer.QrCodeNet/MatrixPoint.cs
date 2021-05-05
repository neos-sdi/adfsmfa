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
    public struct MatrixPoint
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        internal MatrixPoint(int x, int y) 
            : this()
        {
            X = x;
            Y = y;
        }

        public MatrixPoint Offset(MatrixPoint offset)
        {
            return new MatrixPoint(offset.X + this.X, offset.Y + this.Y);
        }

        internal MatrixPoint Offset(int offsetX, int offsetY)
        {
            return Offset(new MatrixPoint(offsetX, offsetY));
        }

        public override string ToString()
        {
            return string.Format("Point({0};{1})", X, Y);
        }
    }
}
