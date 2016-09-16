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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using Neos.IdentityServer.MultiFactor.QrEncoding.Windows.Render;

namespace Neos.IdentityServer.MultiFactor.QrEncoding.Windows.WPF
{
    public class QrCodeGeoControl : Control
    {
        private QrCode m_QrCode = new QrCode();
        private int m_width = 21;
        private bool m_isLocked = false;

        public event EventHandler QrMatrixChanged;

      

        #region LightBrush
        public static readonly DependencyProperty LightBrushProperty =
            DependencyProperty.Register("LightBrush", typeof(Brush), typeof(QrCodeGeoControl), new UIPropertyMetadata(Brushes.White));
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("Qr Code")]
        public Brush LightBrush
        {
            get { return (Brush)GetValue(LightBrushProperty); }
            set { SetValue(LightBrushProperty, value); }
        }
        #endregion

        #region DarkBrush
        public static readonly DependencyProperty DarkBrushProperty =
            DependencyProperty.Register("DarkBrush", typeof(Brush), typeof(QrCodeGeoControl), new UIPropertyMetadata(Brushes.Black));
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("Qr Code")]
        public Brush DarkBrush
        {
            get { return (Brush)GetValue(DarkBrushProperty); }
            set { SetValue(DarkBrushProperty, value); }
        }
        #endregion

        #region QuietZone
        public static readonly DependencyProperty QuietZoneModuleProperty =
            DependencyProperty.Register("QuietZoneModule", typeof(QuietZoneModules), typeof(QrCodeGeoControl),
            new UIPropertyMetadata(QuietZoneModules.Two, new PropertyChangedCallback(OnQuietZonePixelSizeChanged)));
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("Qr Code")]
        public QuietZoneModules QuietZoneModule
        {
            get { return (QuietZoneModules)GetValue(QuietZoneModuleProperty); }
            set { SetValue(QuietZoneModuleProperty, value); }
        }

        #endregion

        #region ErrorCorrectionLevel
        public static readonly DependencyProperty ErrorCorrecLeveltProperty =
            DependencyProperty.Register("ErrorCorrectLevel", typeof(ErrorCorrectionLevel), typeof(QrCodeGeoControl),
            new UIPropertyMetadata(ErrorCorrectionLevel.M, new PropertyChangedCallback(OnMatrixValueChanged)));
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("Qr Code")]
        public ErrorCorrectionLevel ErrorCorrectLevel
        {
            get { return (ErrorCorrectionLevel)GetValue(ErrorCorrecLeveltProperty); }
            set { SetValue(ErrorCorrecLeveltProperty, value); }
        }
        #endregion

        #region Text
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(QrCodeGeoControl),
            new UIPropertyMetadata(string.Empty, new PropertyChangedCallback(OnMatrixValueChanged)));
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Category("Qr Code")]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        #endregion

        #region QrGeometry
        public static readonly DependencyProperty QrGeometryProperty =
            DependencyProperty.Register("QrGeometry", typeof(Geometry), typeof(QrCodeGeoControl), null);

        public Geometry QrGeometry
        {
            get { return (Geometry)GetValue(QrGeometryProperty); }
            private set { SetValue(QrGeometryProperty, value); }
        }
        #endregion

        /// <summary>
        /// Occure when ErrorCorrectLevel or Text changed
        /// </summary>
        private static void OnMatrixValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            QrCodeGeoControl geoControl = (QrCodeGeoControl)d;
            geoControl.UpdateGeometry();
            geoControl.UpdatePadding();
        }

        /// <summary>
        /// QrCode matrix cache updated.
        /// </summary>
        protected virtual void OnQrMatrixChanged(EventArgs e)
        {
            if (QrMatrixChanged != null)
                QrMatrixChanged(this, e);
        }

        /// <summary>
        /// Update Geometry if is unlocked. 
        /// </summary>
        internal void UpdateGeometry()
        {
            if (m_isLocked)
                return;
            new QrEncoder(ErrorCorrectLevel).TryEncode(Text, out m_QrCode);
            OnQrMatrixChanged(new EventArgs());
            m_width = m_QrCode.Matrix == null ? 21 : m_QrCode.Matrix.Width;
            QrGeometry = new DrawingBrushRenderer(new FixedCodeSize(200, QuietZoneModule), DarkBrush, LightBrush).DrawGeometry(m_QrCode.Matrix, 0, 0);
        }

        private static void OnQuietZonePixelSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((QrCodeGeoControl)d).UpdatePadding();
        }

        /// <summary>
        /// This method is use to update QuietZone after use method SetQuietZoneModule
        /// </summary>
        internal void UpdatePadding()
        {
            if (m_isLocked)
                return;
            double length = ActualWidth < ActualHeight ? ActualWidth : ActualHeight;
            double moduleSize = length / (m_width + 2 * (int)QuietZoneModule);
            this.Padding = new Thickness(moduleSize * (int)QuietZoneModule);
        }

        static QrCodeGeoControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(QrCodeGeoControl), new FrameworkPropertyMetadata(typeof(QrCodeGeoControl)));
            HorizontalAlignmentProperty.OverrideMetadata(typeof(QrCodeGeoControl), new FrameworkPropertyMetadata(HorizontalAlignment.Center));
            VerticalAlignmentProperty.OverrideMetadata(typeof(QrCodeGeoControl), new FrameworkPropertyMetadata(VerticalAlignment.Center));
        }

        public QrCodeGeoControl()
        {
            this.UpdateGeometry();
            this.UpdatePadding();
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            double width = arrangeBounds.Width < arrangeBounds.Height ? arrangeBounds.Width : arrangeBounds.Height;
            double moduleSize = width / (m_width + 2 * (int)QuietZoneModule);
            this.Padding = new Thickness(moduleSize * (int)QuietZoneModule);
            return base.ArrangeOverride(arrangeBounds);
        }

        /// <summary>
        /// If Class is locked, it won't update Geometry or quietzone.
        /// </summary>
        public void Lock()
        {
            m_isLocked = true;
        }

        /// <summary>
        /// Unlock class will cause class to update Geometry and quietZone. 
        /// </summary>
        public void Unlock()
        {
            m_isLocked = false;
            this.UpdateGeometry();
            this.UpdatePadding();
        }

        /// <summary>
        /// Return whether if class is locked
        /// </summary>
        public bool IsLocked
        { get { return m_isLocked; } }


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
