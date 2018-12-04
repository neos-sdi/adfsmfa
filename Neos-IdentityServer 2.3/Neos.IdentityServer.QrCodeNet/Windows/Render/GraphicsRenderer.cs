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

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System;

namespace Neos.IdentityServer.MultiFactor.QrEncoding.Windows.Render
{
    public class GraphicsRenderer
    {
        private Brush m_DarkBrush;
        private Brush m_LightBrush;

        private ISizeCalculation m_iSize;

        /// <summary>
        /// Initialize Renderer. Default brushes will be black and white.
        /// </summary>
        /// <param name="iSize">The way of calculate Size</param>
        public GraphicsRenderer(ISizeCalculation iSize)
            : this(iSize, Brushes.Black, Brushes.White)
        {
        }

        /// <summary>
        /// Initialize Renderer
        /// </summary>
        /// <param name="iSize">The way of calculate Size</param>
        public GraphicsRenderer(ISizeCalculation iSize, Brush darkBrush, Brush lightBrush)
        {
            m_iSize = iSize;

            m_DarkBrush = darkBrush;
            m_LightBrush = lightBrush;
        }

        /// <summary>
        /// Drawing Bitmatrix to winform graphics.
        /// Default position will be 0, 0
        /// </summary>
        /// <param name="matrix">Draw background only for null matrix</param>
        /// <exception cref="ArgumentNullException">DarkBrush or LightBrush is null</exception>
        public void Draw(Graphics graphics, BitMatrix QrMatrix)
        {
            this.Draw(graphics, QrMatrix, new Point(0, 0));
        }

        /// <summary>
        /// Drawing Bitmatrix to winform graphics.
        /// </summary>
        /// <param name="QrMatrix">Draw background only for null matrix</param>
        /// <exception cref="ArgumentNullException">DarkBrush or LightBrush is null</exception>
        public void Draw(Graphics graphics, BitMatrix QrMatrix, Point offset)
        {
            int width = QrMatrix == null ? 21 : QrMatrix.Width;

            DrawingSize size = m_iSize.GetSize(width);

            graphics.FillRectangle(m_LightBrush, offset.X, offset.Y, size.CodeWidth, size.CodeWidth);

            if(QrMatrix == null || size.ModuleSize == 0)
                return;

            int padding = (size.CodeWidth - (size.ModuleSize * width)) / 2;

            int preX = -1;

            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (QrMatrix[x, y])
                    {
                        //Set start point if preX == -1
                        if (preX == -1)
                            preX = x;
                        //If this is last module in that row. Draw rectangle
                        if (x == width - 1)
                        {
                            Point modulePosition = new Point(preX * size.ModuleSize + padding + offset.X,
                                y * size.ModuleSize + padding + offset.Y);
                            Size rectSize = new Size((x - preX + 1) * size.ModuleSize, size.ModuleSize);
                            graphics.FillRectangle(m_DarkBrush, modulePosition.X, modulePosition.Y, rectSize.Width, rectSize.Height);
                            preX = -1;
                        }
                    }
                    else if (!QrMatrix[x, y] && preX != -1)
                    {
                        //Here will be first light module after sequence of dark module.
                        //Draw previews sequence of dark Module
                        Point modulePosition = new Point(preX * size.ModuleSize + padding + offset.X,
                            y * size.ModuleSize + padding + offset.Y);
                        Size rectSize = new Size((x - preX) * size.ModuleSize, size.ModuleSize);
                        graphics.FillRectangle(m_DarkBrush, modulePosition.X, modulePosition.Y, rectSize.Width, rectSize.Height);
                        preX = -1;
                    }
                }
            }

        }

        /// <summary>
        /// Saves QrCode Image to specified stream in the specified format
        /// </summary>
        /// <exception cref="ArgumentNullException">Stream or Format is null</exception>
        /// <remarks>You should avoid saving an image to the same stream that was used to construct. Doing so might damage the stream
        /// If any additional data has been written to the stream before saving the image, the image data in the stream will be corrupted</remarks>
        public void WriteToStream(BitMatrix QrMatrix, ImageFormat imageFormat, Stream stream, Point DPI)
        {
            if (imageFormat == ImageFormat.Emf || imageFormat == ImageFormat.Wmf)
            {
                this.CreateMetaFile(QrMatrix, stream);
            }
            else if (imageFormat != ImageFormat.Exif
                && imageFormat != ImageFormat.Icon
                && imageFormat != ImageFormat.MemoryBmp)
            {
                DrawingSize size = m_iSize.GetSize(QrMatrix == null ? 21 : QrMatrix.Width);

                using (Bitmap bitmap = new Bitmap(size.CodeWidth, size.CodeWidth))
                {
                    if(DPI.X != 96 || DPI.Y != 96)
                        bitmap.SetResolution(DPI.X, DPI.Y);
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        this.Draw(graphics, QrMatrix);
                        bitmap.Save(stream, imageFormat);
                    }
                }
            }
        }

        public void WriteToStream(BitMatrix QrMatrix, ImageFormat imageFormat, Stream stream)
        {
            this.WriteToStream(QrMatrix, imageFormat, stream, new Point(96, 96));
        }

        /// <summary>
        /// Using MetaFile Class to create metafile. 
        /// temp control create to use as object to get temp graphics for Hdc. 
        /// Drawing on the metaGraphics will record as vector metaFile. 
        /// </summary>
        private void CreateMetaFile(BitMatrix QrMatrix, Stream stream)
        {
            using (Control gControl = new Control())
            using (Graphics newGraphics = gControl.CreateGraphics())
            {
                IntPtr hdc = newGraphics.GetHdc();
                using (Metafile metaFile = new Metafile(stream, hdc))
                using (Graphics metaGraphics = Graphics.FromImage(metaFile))
                {
                    this.Draw(metaGraphics, QrMatrix);
                }
            }

        }

        /// <summary>
        /// DarkBrush for drawing Dark module of QrCode
        /// </summary>
        public Brush DarkBrush
        {
            set
            {
                m_DarkBrush = value;
            }
            get
            {
                return m_DarkBrush;
            }
        }

        /// <summary>
        /// LightBrush for drawing Light module and QuietZone of QrCode
        /// </summary>
        public Brush LightBrush
        {
            set
            {
                m_LightBrush = value;
            }
            get
            {
                return m_LightBrush;
            }
        }

        /// <summary>
        /// ISizeCalculation for the way to calculate QrCode's pixel size.
        /// Ex for ISizeCalculation:FixedCodeSize, FixedModuleSize
        /// </summary>
        public ISizeCalculation SizeCalculator
        {
            set
            {
                m_iSize = value;
            }
            get
            {
                return m_iSize;
            }
        }


    }
}
