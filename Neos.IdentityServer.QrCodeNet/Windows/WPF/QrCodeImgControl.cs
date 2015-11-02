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
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using Neos.IdentityServer.MultiFactor.QrEncoding.Windows.Render;

namespace Neos.IdentityServer.MultiFactor.QrEncoding.Windows.WPF
{
    public class QrCodeImgControl : Control
    {
        private QrCode m_QrCode = new QrCode();
        private bool m_isLocked = false;
        private bool m_isFreezed = false;

        public event EventHandler QrMatrixChanged;

        #region WBitmap
        public static readonly DependencyProperty WBitmapProperty =
            DependencyProperty.Register("WBitmap",
            typeof(WriteableBitmap),
            typeof(QrCodeImgControl),
            new UIPropertyMetadata(null, null));

        public WriteableBitmap WBitmap
        {
            get { return (WriteableBitmap)GetValue(WBitmapProperty); }
            private set { SetValue(WBitmapProperty, value); }
        }
        #endregion

        #region QrCodeWidthInch
        public static readonly DependencyProperty QrCodeWidthInchProperty =
            DependencyProperty.Register("QrCodeWidth", typeof(double), typeof(QrCodeImgControl),
            new UIPropertyMetadata(2.08, new PropertyChangedCallback(OnVisualValueChanged)));

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("QrCode")]
        public double QrCodeWidthInch
        {
            get { return (double)GetValue(QrCodeWidthInchProperty); }
            set { SetValue(QrCodeWidthInchProperty, value); }
        }

        #endregion

        #region QuietZoneModule
        public static readonly DependencyProperty QuietZoneModuleProperty =
            DependencyProperty.Register("QuietZoneModule", typeof(QuietZoneModules), typeof(QrCodeImgControl),
            new UIPropertyMetadata(QuietZoneModules.Two, new PropertyChangedCallback(OnVisualValueChanged)));

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("QrCode")]
        public QuietZoneModules QuietZoneModule
        {
            get { return (QuietZoneModules)GetValue(QuietZoneModuleProperty); }
            set { SetValue(QuietZoneModuleProperty, value); }
        }

        #endregion

        #region LightColor
        public static readonly DependencyProperty LightColorProperty =
            DependencyProperty.Register("LightColor", typeof(Color), typeof(QrCodeImgControl),
            new UIPropertyMetadata(Colors.White, new PropertyChangedCallback(OnVisualValueChanged)));

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("QrCode")]
        public Color LightColor
        {
            get { return (Color)GetValue(LightColorProperty); }
            set { SetValue(LightColorProperty, value); }
        }

        #endregion

        #region DarkColor
        public static readonly DependencyProperty DarkColorProperty =
            DependencyProperty.Register("DarkColor", typeof(Color), typeof(QrCodeImgControl),
            new UIPropertyMetadata(Colors.Black, new PropertyChangedCallback(OnVisualValueChanged)));

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("QrCode")]
        public Color DarkColor
        {
            get { return (Color)GetValue(DarkColorProperty); }
            set { SetValue(DarkColorProperty, value); }
        }

        #endregion

        #region ErrorCorrectionLevel
        public static readonly DependencyProperty ErrorCorrectLevelProperty =
            DependencyProperty.Register("ErrorCorrectLevel", typeof(ErrorCorrectionLevel), typeof(QrCodeImgControl),
            new UIPropertyMetadata(ErrorCorrectionLevel.M, new PropertyChangedCallback(OnQrValueChanged)));

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("QrCode")]
        public ErrorCorrectionLevel ErrorCorrectLevel
        {
            get { return (ErrorCorrectionLevel)GetValue(ErrorCorrectLevelProperty); }
            set { SetValue(ErrorCorrectLevelProperty, value); }
        }
        #endregion

        #region Text
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(QrCodeImgControl),
            new UIPropertyMetadata(string.Empty, new PropertyChangedCallback(OnQrValueChanged)));

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("QrCode")]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        #endregion

        #region IsGrayImage
        public static readonly DependencyProperty IsGrayImageProperty =
            DependencyProperty.Register("IsGrayImage", typeof(bool), typeof(QrCodeImgControl),
            new UIPropertyMetadata(true, new PropertyChangedCallback(OnVisualValueChanged)));

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("QrCode")]
        public bool IsGrayImage
        {
            get { return (bool)GetValue(IsGrayImageProperty); }
            set { SetValue(IsGrayImageProperty, value); }
        }

        #endregion

        private int m_DpiX = 96;
        private int m_DpiY = 96;

        static QrCodeImgControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(QrCodeImgControl), new FrameworkPropertyMetadata(typeof(QrCodeImgControl)));
            HorizontalAlignmentProperty.OverrideMetadata(typeof(QrCodeImgControl), new FrameworkPropertyMetadata(HorizontalAlignment.Center));
            VerticalAlignmentProperty.OverrideMetadata(typeof(QrCodeImgControl), new FrameworkPropertyMetadata(VerticalAlignment.Center));
        }

        public QrCodeImgControl()
        {
            MatrixPoint dpi = this.GetDPI();
            m_DpiX = dpi.X;
            m_DpiY = dpi.Y;
            this.EncodeAndUpdateBitmap();
        }

        public MatrixPoint GetDPI()
        {
            int dpix, dpiy;
            PresentationSource source = PresentationSource.FromVisual(this);
            if (source != null)
            {
                Matrix dpi = source.CompositionTarget.TransformToDevice;
                dpix = (int)(96 * dpi.M11);
                dpiy = (int)(96 * dpi.M22);
                return new MatrixPoint(dpix, dpiy);
            }
            else
                return new MatrixPoint(m_DpiX, m_DpiY);
        }

        #region ReDraw Bitmap, Update Qr Cache

        private void CreateBitmap()
        {
            int pixelWidth = (int)QrCodeWidthInch * m_DpiX;
            int suitableWidth = m_QrCode.Matrix == null ? CalculateSuitableWidth(pixelWidth, 21)
                : CalculateSuitableWidth(pixelWidth, m_QrCode.Matrix.Width);
            PixelFormat pFormat = IsGrayImage ? PixelFormats.Gray8 : PixelFormats.Pbgra32;

            if (WBitmap == null)
            {
                WBitmap = new WriteableBitmap(suitableWidth, suitableWidth, m_DpiX, m_DpiY, pFormat, null);
                return;
            }

            if (WBitmap.PixelHeight != suitableWidth || WBitmap.PixelWidth != suitableWidth || WBitmap.Format != pFormat)
            {
                WBitmap = null;
                WBitmap = new WriteableBitmap(suitableWidth, suitableWidth, m_DpiX, m_DpiY, pFormat, null);
            }
        }

        private int CalculateSuitableWidth(int width, int bitMatrixWidth)
        {
            FixedCodeSize isize = new FixedCodeSize(width, QuietZoneModule);
            DrawingSize dSize = isize.GetSize(bitMatrixWidth);
            int gap = dSize.CodeWidth - dSize.ModuleSize * (bitMatrixWidth + 2 * (int)QuietZoneModule);

            if (gap == 0)
                return width;
            else if (dSize.CodeWidth / gap < 4)
                return (dSize.ModuleSize + 1) * (bitMatrixWidth + 2 * (int)QuietZoneModule);
            else
                return dSize.ModuleSize * (bitMatrixWidth + 2 * (int)QuietZoneModule);
        }

        private void UpdateSource()
        {
            this.CreateBitmap();

            if (WBitmap.PixelWidth != 0 && WBitmap.PixelHeight != 0)
            {
                WBitmap.Clear(LightColor);

                if (m_QrCode.Matrix != null)
                {
                    //WBitmap.
                    new WriteableBitmapRenderer(new FixedCodeSize(WBitmap.PixelWidth, QuietZoneModule), DarkColor, LightColor).DrawDarkModule(WBitmap, m_QrCode.Matrix, 0, 0);
                }
            }
        }

        private void UpdateQrCodeCache()
        {
            new QrEncoder(ErrorCorrectLevel).TryEncode(this.Text, out m_QrCode);
            OnQrMatrixChanged(new EventArgs());
        }

        #endregion

        #region Event method

        private static void OnVisualValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((QrCodeImgControl)d).UpdateBitmap();
        }

        /// <summary>
        /// Encode and Update bitmap when ErrorCorrectlevel or Text changed. 
        /// </summary>
        private static void OnQrValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((QrCodeImgControl)d).EncodeAndUpdateBitmap();
        }

        #endregion

        #region Update method

        internal void EncodeAndUpdateBitmap()
        {
            if (!IsLocked)
            {
                this.UpdateQrCodeCache();
                this.UpdateBitmap();
            }
        }

        internal void UpdateBitmap()
        {
            if(!IsFreezed)
                this.UpdateSource();
        }

        #endregion

        #region Lock Freeze

        /// <summary>
        /// If Class is locked, it won't update QrMatrix cache.
        /// </summary>
        public void Lock()
        {
            m_isLocked = true;
        }

        /// <summary>
        /// Unlock class will cause class to update QrMatrix Cache and redraw bitmap. 
        /// </summary>
        public void Unlock()
        {
            m_isLocked = false;
            this.EncodeAndUpdateBitmap();
        }

        /// <summary>
        /// Return whether if class is locked
        /// </summary>
        public bool IsLocked
        { get { return m_isLocked; } }

        /// <summary>
        /// Freeze Class, Any value change to Brush, QuietZoneModule won't cause immediately redraw bitmap. 
        /// </summary>
        public void Freeze()
        { m_isFreezed = true; }

        /// <summary>
        /// Unfreeze and redraw immediately. 
        /// </summary>
        public void UnFreeze()
        {
            m_isFreezed = false;
            this.UpdateBitmap();
        }

        /// <summary>
        /// Return whether if class is freezed. 
        /// </summary>
        public bool IsFreezed
        { get { return m_isFreezed; } }

        #endregion

        /// <summary>
        /// QrCode matrix cache updated.
        /// </summary>
        protected virtual void OnQrMatrixChanged(EventArgs e)
        {
            if (QrMatrixChanged != null)
                QrMatrixChanged(this, e);
        }

        /// <summary>
        /// Get Qr SquareBitMatrix as two dimentional bool array.
        /// It will be deep copy of control's internal bitmatrix. 
        /// </summary>
        /// <returns>null if matrix is null, else full matrix</returns>
        public SquareBitMatrix GetQrMatrix()
        {
            if (m_QrCode.Matrix == null)
                return null;
            else
            {
                BitMatrix matrix = m_QrCode.Matrix;
                bool[,] internalArray = matrix.InternalArray;
                return new SquareBitMatrix(internalArray);
            }
        }

    }
}
