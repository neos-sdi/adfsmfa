using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace Neos.IdentityServer.MultiFactor.QrEncoding.Windows.Render
{
    public class WriteableBitmapRenderer
    {
        public Color DarkColor { get; set; }
        public Color LightColor { get; set; }

        public ISizeCalculation ISize { get; set; }

        /// <summary>
        /// Initialize renderer
        /// </summary>
        /// <param name="iSize">Size calculation strategy</param>
        public WriteableBitmapRenderer(ISizeCalculation iSize)
            : this(iSize, Colors.Black, Colors.White)
        { }

        /// <summary>
        /// Initialize renderer
        /// </summary>
        /// <param name="iSize">Size calculation strategy</param>
        /// <param name="darkColor">Color for dark module</param>
        /// <param name="lightColor">Color for light module</param>
        public WriteableBitmapRenderer(ISizeCalculation iSize, Color darkColor, Color lightColor)
        {
            this.LightColor = lightColor;
            this.DarkColor = darkColor;
            this.ISize = iSize;
        }

        /// <summary>
        /// Draw QrCode at given writeable bitmap
        /// </summary>
        public void Draw(WriteableBitmap wBitmap, BitMatrix matrix)
        {
            this.Draw(wBitmap, matrix, 0, 0);
        }

        /// <summary>
        /// Draw QrCode at given writeable bitmap at offset location
        /// </summary>
        public void Draw(WriteableBitmap wBitmap, BitMatrix matrix, int offsetX, int offsetY)
        {
            DrawingSize size = matrix == null ? ISize.GetSize(21) : ISize.GetSize(matrix.Width);
            if (wBitmap == null)
                wBitmap = new WriteableBitmap(size.CodeWidth + offsetX, size.CodeWidth + offsetY, 96, 96, PixelFormats.Gray8, null);
            else if (wBitmap.PixelHeight == 0 || wBitmap.PixelWidth == 0)
                return; //writeablebitmap contains no pixel.
            this.DrawQuietZone(wBitmap, size.CodeWidth, offsetX, offsetY);
            if (matrix == null)
                return;

            this.DrawDarkModule(wBitmap, matrix, offsetX, offsetY);
        }

        /// <summary>
        /// Draw quiet zone at offset x,y
        /// </summary>
        private void DrawQuietZone(WriteableBitmap wBitmap, int pixelWidth, int offsetX, int offsetY)
        {
            wBitmap.FillRectangle(new Int32Rect(offsetX, offsetY, pixelWidth, pixelWidth), LightColor);
        }

        /// <summary>
        /// Draw qrCode dark modules at given position. (It will also include quiet zone area. Set it to zero to exclude quiet zone)
        /// </summary>
        /// <exception cref="ArgumentNullException">Bitmatrix, wBitmap should not equal to null</exception>
        /// <exception cref="ArgumentOutOfRangeException">wBitmap's pixel width or height should not equal to zero</exception>
        public void DrawDarkModule(WriteableBitmap wBitmap, BitMatrix matrix, int offsetX, int offsetY)
        {
            if (matrix == null)
                throw new ArgumentNullException("Bitmatrix");

            DrawingSize size = ISize.GetSize(matrix.Width);

            if (wBitmap == null)
                throw new ArgumentNullException("wBitmap");
            else if (wBitmap.PixelHeight == 0 || wBitmap.PixelWidth == 0)
                throw new ArgumentOutOfRangeException("wBitmap", "WriteableBitmap's pixelHeight or PixelWidth are equal to zero");

            int padding = (size.CodeWidth - size.ModuleSize * matrix.Width) / 2;

            int preX = -1;
            int moduleSize = size.ModuleSize;

            if (moduleSize == 0)
                return;

            for (int y = 0; y < matrix.Width; y++)
            {
                for (int x = 0; x < matrix.Width; x++)
                {
                    if (matrix[x, y])
                    {
                        if (preX == -1)
                            preX = x;
                        if (x == matrix.Width - 1)
                        {
                            Int32Rect moduleArea =
                                new Int32Rect(preX * moduleSize + padding + offsetX,
                                    y * moduleSize + padding + offsetY,
                                    (x - preX + 1) * moduleSize,
                                    moduleSize);
                            wBitmap.FillRectangle(moduleArea, DarkColor);
                            preX = -1;
                        }
                    }
                    else if (preX != -1)
                    {
                        Int32Rect moduleArea =
                            new Int32Rect(preX * moduleSize + padding + offsetX,
                                y * moduleSize + padding + offsetY,
                                (x - preX) * moduleSize,
                                moduleSize);
                        wBitmap.FillRectangle(moduleArea, DarkColor);
                        preX = -1;
                    }
                }
            }


        }

        public void WriteToStream(BitMatrix qrMatrix, ImageFormatEnum imageFormat, Stream stream)
        {
            DrawingSize dSize = ISize.GetSize(qrMatrix == null ? 21 : qrMatrix.Width);

            WriteableBitmap wBitmap = new WriteableBitmap(dSize.CodeWidth, dSize.CodeWidth, 96, 96, PixelFormats.Gray8, null);

            this.Draw(wBitmap, qrMatrix);

            BitmapEncoder encoder = imageFormat.ChooseEncoder();
            encoder.Frames.Add(BitmapFrame.Create(wBitmap));
            encoder.Save(stream);

        }

    }
}
