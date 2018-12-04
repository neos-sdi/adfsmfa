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

namespace Neos.IdentityServer.MultiFactor.QrEncoding.Windows.Render
{
    public class FixedModuleSize : ISizeCalculation
    {
        private int m_ModuleSize;
        private int m_QuietZoneModule;

        /// <summary>
        /// Module pixel size. Have to greater than zero
        /// </summary>
        public int ModuleSize
        {
            get
            {
                return m_ModuleSize;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("ModuleSize", value, "ModuleSize can not be equal or less than zero");
                m_ModuleSize = value;
            }
        }

        /// <summary>
        /// Number of quietZone modules
        /// </summary>
        public QuietZoneModules QuietZoneModules
        {
            get
            {
                return (QuietZoneModules)m_QuietZoneModule;
            }
            set
            {
                m_QuietZoneModule = (int)value;
            }
        }

        /// <summary>
        /// FixedModuleSize is strategy for rendering QrCode with fixed module pixel size.
        /// </summary>
        /// <param name="moduleSize">Module pixel size</param>
        /// <param name="quietZoneModules">number of quiet zone modules</param>
        public FixedModuleSize(int moduleSize, QuietZoneModules quietZoneModules)
        {
            m_ModuleSize = moduleSize;
            m_QuietZoneModule = (int)quietZoneModules;
        }

        /// <summary>
        /// Interface function that use by Rendering class.
        /// </summary>
        /// <param name="matrixWidth">QrCode matrix width</param>
        /// <returns>Module pixel size and QrCode pixel width</returns>
        public DrawingSize GetSize(int matrixWidth)
        {
            int width = (m_QuietZoneModule * 2 + matrixWidth) * m_ModuleSize;
            return new DrawingSize(m_ModuleSize, width, (QuietZoneModules)m_QuietZoneModule);
        }
    }
}
