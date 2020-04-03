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
using System.Linq;
using System.Text;

namespace Neos.IdentityServer.MultiFactor.QrEncoding.Masking
{
    internal class PatternFactory
    {
        internal Pattern CreateByType(MaskPatternType maskPatternType)
        {
            switch (maskPatternType)
            {
                case MaskPatternType.Type0:
                    return new Pattern0();

                case MaskPatternType.Type1:
                    return new Pattern1();

                case MaskPatternType.Type2:
                    return new Pattern2();

                case MaskPatternType.Type3:
                    return new Pattern3();

                case MaskPatternType.Type4:
                    return new Pattern4();

                case MaskPatternType.Type5:
                    return new Pattern5();

                case MaskPatternType.Type6:
                    return new Pattern6();

                case MaskPatternType.Type7:
                    return new Pattern7();
            }

            throw new ArgumentException(string.Format("Usupported pattern type {0}", maskPatternType), "maskPatternType");
        }

        internal IEnumerable<Pattern> AllPatterns()
        {
            foreach (MaskPatternType patternType in Enum.GetValues(typeof(MaskPatternType)))
            {
                yield return CreateByType(patternType);
            }
        }
    }
}
