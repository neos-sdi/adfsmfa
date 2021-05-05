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
    internal abstract class PatternStencilBase : BitMatrix
    {
        public int Version { get; private set; }

        internal PatternStencilBase(int version)
        {
            Version = version;
        }

        protected const bool o = false;
        protected const bool x = true;

        public abstract bool[,] Stencil { get; }

        public override bool this[int i, int j]
        {
            get { return Stencil[i, j]; }
            set { throw new NotSupportedException(); }
        }

        public override int Width
        {
            get { return Stencil.GetLength(0); }
        }

        public override int Height
        {
            get { return Stencil.GetLength(1); }
        }

        public override bool[,] InternalArray
        {
            get { throw new NotImplementedException(); }
        }

        public abstract void ApplyTo(TriStateMatrix matrix);
    }
}