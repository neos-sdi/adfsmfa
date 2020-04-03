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
using System.Collections.Generic;

namespace Neos.IdentityServer.MultiFactor.QrEncoding.Positioning.Stencils
{
    internal class PositionDetectionPattern : PatternStencilBase
    {
        public PositionDetectionPattern(int version)
            : base(version)
        {
        }

        private static readonly bool[,] s_PositionDetection = new[,]
                                                                         {
                                                                             { o, o, o, o, o, o, o, o, o },
                                                                             { o, x, x, x, x, x, x, x, o }, 
                                                                             { o, x, o, o, o, o, o, x, o }, 
                                                                             { o, x, o, x, x, x, o, x, o }, 
                                                                             { o, x, o, x, x, x, o, x, o }, 
                                                                             { o, x, o, x, x, x, o, x, o }, 
                                                                             { o, x, o, o, o, o, o, x, o }, 
                                                                             { o, x, x, x, x, x, x, x, o },
                                                                             { o, o, o, o, o, o, o, o, o }
                                                                         };

        public override bool[,] Stencil
        {
            get { return s_PositionDetection; }
        }

        public override void ApplyTo(TriStateMatrix matrix)
        {
            MatrixSize size = GetSizeOfSquareWithSeparators();
            
            MatrixPoint leftTopCorner = new MatrixPoint(0, 0);
            this.CopyTo(matrix, new MatrixRectangle(new MatrixPoint(1, 1), size), leftTopCorner, MatrixStatus.NoMask);

            MatrixPoint rightTopCorner = new MatrixPoint(matrix.Width - this.Width + 1, 0);
            this.CopyTo(matrix, new MatrixRectangle(new MatrixPoint(0, 1), size), rightTopCorner, MatrixStatus.NoMask);


            MatrixPoint leftBottomCorner = new MatrixPoint(0, matrix.Width - this.Width + 1);
            this.CopyTo(matrix, new MatrixRectangle(new MatrixPoint(1, 0), size), leftBottomCorner, MatrixStatus.NoMask);
        }

        private MatrixSize GetSizeOfSquareWithSeparators()
        {
            return new MatrixSize(this.Width - 1, this.Height - 1);
        }
    }
}
