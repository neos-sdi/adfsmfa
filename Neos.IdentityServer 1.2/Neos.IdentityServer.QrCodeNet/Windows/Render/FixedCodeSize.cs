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
    public class FixedCodeSize : ISizeCalculation
    {
        private int m_QrCodeWidth;

        private int m_QuietZoneModules = 2;

        /// <summary>
        /// QrCodeWidth is pixel size of QrCode you would like to print out. 
        /// It have to be greater than QrCode's matrix width(include quiet zone).
        /// QrCode matrix width is between 25 ~ 182(version 1 ~ 40).
        /// </summary>
        public int QrCodeWidth
        {
            get
            {
                return m_QrCodeWidth;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("QrCodeWidth", value, "QrCodeWidth can not be equal or less than zero");
                m_QrCodeWidth = value;
            }
        }

        /// <summary>
        /// Number of quietZone modules
        /// </summary>
        public QuietZoneModules QuietZoneModules
        {
            get
            {
                return (QuietZoneModules)m_QuietZoneModules;
            }
            set
            {
                m_QuietZoneModules = (int)value;
            }
        }

        /// <summary>
        /// FixedCodeSize is strategy for rendering QrCode at fixed Size. 
        /// </summary>
        /// <param name="qrCodeWidth">Fixed size for QrCode pixel width. 
        /// Pixel width have to be bigger than QrCode's matrix width(include quiet zone)
        /// QrCode matrix width is between 25 ~ 182(version 1 ~ 40).</param>
        public FixedCodeSize(int qrCodeWidth, QuietZoneModules quietZone)
        {
            m_QrCodeWidth = qrCodeWidth;
            m_QuietZoneModules = (int)quietZone;
        }

        /// <summary>
        /// Interface function that use by Rendering class.
        /// </summary>
        /// <param name="matrixWidth">QrCode matrix width</param>
        /// <returns>Module pixel size and QrCode pixel width</returns>
        public DrawingSize GetSize(int matrixWidth)
        {
            int moduleSize = m_QrCodeWidth / (matrixWidth + m_QuietZoneModules * 2);
            return new DrawingSize(moduleSize, m_QrCodeWidth, (QuietZoneModules)m_QuietZoneModules);
        }
    }
}
