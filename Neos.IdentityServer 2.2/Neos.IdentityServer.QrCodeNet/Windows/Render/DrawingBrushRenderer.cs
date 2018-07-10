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
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Neos.IdentityServer.MultiFactor.QrEncoding.Windows.Render
{
    public class DrawingBrushRenderer
    {
        private Brush m_DarkBrush;
        private Brush m_LightBrush;

        private ISizeCalculation m_ISize;

        /// <summary>
        /// Initialize Renderer. Default brushes will be black and white.
        /// </summary>
        /// <param name="fixedModuleSize"></param>
        public DrawingBrushRenderer(ISizeCalculation iSize)
            : this(iSize, Brushes.Black, Brushes.White)
        {
        }

        /// <summary>
        /// Initialize Renderer.
        /// </summary>
        public DrawingBrushRenderer(ISizeCalculation iSize, Brush darkBrush, Brush lightBrush)
        {
            m_ISize = iSize;
            m_DarkBrush = darkBrush;
            m_LightBrush = lightBrush;
        }

        /// <summary>
        /// Draw QrCode to DrawingBrush
        /// </summary>
        /// <returns>DrawingBrush, Stretch = uniform</returns>
        /// <remarks>LightBrush will not use by this method, DrawingBrush will only contain DarkBrush part.
        /// Use LightBrush to fill background of main uielement for more flexible placement</remarks>
        public DrawingBrush DrawBrush(BitMatrix QrMatrix)
        {
            if (QrMatrix == null)
            {
                return ConstructDrawingBrush(null);
            }


            GeometryDrawing qrCodeDrawing = ConstructQrDrawing(QrMatrix, 0, 0);

            return ConstructDrawingBrush(qrCodeDrawing);
        }

        /// <summary>
        /// Construct QrCode geometry. It will only include geometry for Dark colour module
        /// </summary>
        /// <returns>QrCode dark colour module geometry. Size = QrMatrix width x width</returns>
        public StreamGeometry DrawGeometry(BitMatrix QrMatrix, int offsetX, int offSetY)
        {
            int width = QrMatrix == null ? 21 : QrMatrix.Width;

            StreamGeometry qrCodeStream = new StreamGeometry();
            qrCodeStream.FillRule = FillRule.EvenOdd;

            if (QrMatrix == null)
                return qrCodeStream;

            using (StreamGeometryContext qrCodeCtx = qrCodeStream.Open())
            {
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
                                qrCodeCtx.DrawRectGeometry(new Int32Rect(preX + offsetX, y + offSetY, x - preX + 1, 1));
                                preX = -1;
                            }
                        }
                        else if (!QrMatrix[x, y] && preX != -1)
                        {
                            //Here will be first light module after sequence of dark module.
                            //Draw previews sequence of dark Module
                            qrCodeCtx.DrawRectGeometry(new Int32Rect(preX + offsetX, y + offSetY, x - preX, 1));
                            preX = -1;
                        }
                    }
                }
            }
            qrCodeStream.Freeze();

            return qrCodeStream;
        }

        /// <summary>
        /// Construct DrawingBrush with input Drawing
        /// </summary>
        /// <returns>DrawingBrush where Stretch = uniform</returns>
        private DrawingBrush ConstructDrawingBrush(Drawing drawing)
        {
            DrawingBrush qrCodeBrush = new DrawingBrush();
            qrCodeBrush.Stretch = Stretch.Uniform;
            qrCodeBrush.Drawing = drawing;
            return qrCodeBrush;
        }


        private GeometryDrawing ConstructQrDrawing(BitMatrix QrMatrix, int offsetX, int offSetY)
        {
            StreamGeometry qrCodeStream = DrawGeometry(QrMatrix, offsetX, offSetY);

            GeometryDrawing qrCodeDrawing = new GeometryDrawing();
            qrCodeDrawing.Brush = m_DarkBrush;

            qrCodeDrawing.Geometry = qrCodeStream;

            return qrCodeDrawing;
        }

        private GeometryDrawing ConstructQZDrawing(int width)
        {
            StreamGeometry quietZoneSG = new StreamGeometry();
            quietZoneSG.FillRule = FillRule.EvenOdd;

            using (StreamGeometryContext qrCodeCtx = quietZoneSG.Open())
            {
                qrCodeCtx.DrawRectGeometry(new Int32Rect(0, 0, width, width));
            }
            quietZoneSG.Freeze();

            GeometryDrawing quietZoneDrawing = new GeometryDrawing();
            quietZoneDrawing.Brush = m_LightBrush;
            quietZoneDrawing.Geometry = quietZoneSG;

            return quietZoneDrawing;
        }

        /// <summary>
        /// Write image file to stream
        /// Default DPI will be 96, 96
        /// </summary>
        public void WriteToStream(BitMatrix QrMatrix, ImageFormatEnum imageFormat, Stream stream)
        {
            this.WriteToStream(QrMatrix, imageFormat, stream, new Point(96, 96));
        }

        /// <summary>
        /// Write image file to stream
        /// </summary>
        /// <param name="DPI">DPI = DPI.X, DPI.Y(Dots per inch)</param>
        public void WriteToStream(BitMatrix QrMatrix, ImageFormatEnum imageFormat, Stream stream, Point DPI)
        {
            BitmapSource bitmapSource = WriteToBitmapSource(QrMatrix, DPI);

            BitmapEncoder encoder = imageFormat.ChooseEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            encoder.Save(stream);
        }

        public BitmapSource WriteToBitmapSource(BitMatrix QrMatrix, Point DPI)
        {
            int width = QrMatrix == null ? 21 : QrMatrix.Width;
            DrawingSize dSize = m_ISize.GetSize(width);
            int quietZone = (int)dSize.QuietZoneModules;

            GeometryDrawing quietZoneDrawing = ConstructQZDrawing(2 * quietZone + width);
            GeometryDrawing qrDrawing = ConstructQrDrawing(QrMatrix, quietZone, quietZone);

            DrawingGroup qrGroup = new DrawingGroup();
            qrGroup.Children.Add(quietZoneDrawing);
            qrGroup.Children.Add(qrDrawing);

            DrawingBrush qrBrush = ConstructDrawingBrush(qrGroup);

            PixelFormat pixelFormat = PixelFormats.Pbgra32;
            RenderTargetBitmap renderbmp = new RenderTargetBitmap(dSize.CodeWidth, dSize.CodeWidth, DPI.X, DPI.Y, pixelFormat);

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext dContext = drawingVisual.RenderOpen())
            {
                dContext.DrawRectangle(qrBrush, null, new Rect(0, 0, (dSize.CodeWidth / DPI.X) * 96, (dSize.CodeWidth / DPI.Y) * 96));
            }
            
            renderbmp.Render(drawingVisual);

            return renderbmp;
        }

        public Brush DarkBrush
        {
            get { return m_DarkBrush; }
            set { m_DarkBrush = value; }
        }

        public Brush LightBrush
        {
            get { return m_LightBrush; }
            set { m_LightBrush = value; }
        }

        public ISizeCalculation ISize
        {
            get { return m_ISize; }
            set { m_ISize = value; }
        }
    }
}
