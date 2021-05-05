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
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;
using Neos.IdentityServer.MultiFactor.QrEncoding.Windows.Render;


namespace Neos.IdentityServer.MultiFactor.QrEncoding.Windows.Forms
{
    public class QrCodeImgControl : PictureBox
    {
        private QrCode m_QrCode = new QrCode();

        private bool m_isLocked = false;
        private bool m_isFreezed = false;

        public event EventHandler DarkBrushChanged;
        public event EventHandler LightBrushChanged;
        public event EventHandler QuietZoneModuleChanged;
        public event EventHandler ErrorCorrectLevelChanged;
        public event EventHandler QrMatrixChanged;

        #region DarkBrush
        private Brush m_darkBrush = Brushes.Black;
        [Category("Qr Code"), Browsable(true), EditorBrowsable(EditorBrowsableState.Always), RefreshProperties(RefreshProperties.All), Localizable(false)]
        public Brush DarkBrush
        {
            get
            {
                return m_darkBrush;
            }
            set
            {
                if (m_darkBrush != value)
                {
                    m_darkBrush = value;
                    OnDarkBrushChanged(new EventArgs());
                    UpdateImage();
                }
            }
        }

        protected virtual void OnDarkBrushChanged(EventArgs e)
        {
            if (DarkBrushChanged != null)
            {
                DarkBrushChanged(this, e);
            }
        }

        #endregion

        #region LightBrush
        private Brush m_LightBrush = Brushes.White;
        [Category("Qr Code"), Browsable(true), EditorBrowsable(EditorBrowsableState.Always), RefreshProperties(RefreshProperties.All), Localizable(false)]
        public Brush LightBrush
        {
            get
            { return m_LightBrush; }
            set
            {
                if (m_LightBrush != value)
                {
                    m_LightBrush = value;
                    OnLightBrushChanged(new EventArgs());
                    UpdateImage();
                }
            }
        }

        protected virtual void OnLightBrushChanged(EventArgs e)
        {
            if (LightBrushChanged != null)
                LightBrushChanged(this, e);
        }

        #endregion

        #region QuietZoneModule
        private QuietZoneModules m_QuietZoneModule = QuietZoneModules.Two;
        [Category("Qr Code"), Browsable(true), EditorBrowsable(EditorBrowsableState.Always), RefreshProperties(RefreshProperties.All), Localizable(false)]
        public QuietZoneModules QuietZoneModule
        {
            get { return m_QuietZoneModule; }
            set
            {
                if (m_QuietZoneModule != value)
                {
                    m_QuietZoneModule = value;
                    OnQuietZoneModuleChanged(new EventArgs());
                    UpdateImage();
                }
            }
        }

        protected virtual void OnQuietZoneModuleChanged(EventArgs e)
        {
            if (QuietZoneModuleChanged != null)
                QuietZoneModuleChanged(this, e);
        }
        #endregion

        #region ErrorCorrectLevel
        private ErrorCorrectionLevel m_ErrorCorrectLevel = ErrorCorrectionLevel.M;
        [Category("Qr Code"), Browsable(true), EditorBrowsable(EditorBrowsableState.Always), RefreshProperties(RefreshProperties.All), Localizable(false)]
        public ErrorCorrectionLevel ErrorCorrectLevel
        {
            get { return m_ErrorCorrectLevel; }
            set
            {
                if (m_ErrorCorrectLevel != value)
                {
                    m_ErrorCorrectLevel = value;

                    OnErrorCorrectLevelChanged(new EventArgs());

                    UpdateQrCodeCache();
                }
            }
        }

        protected virtual void OnErrorCorrectLevelChanged(EventArgs e)
        {
            if (ErrorCorrectLevelChanged != null)
                ErrorCorrectLevelChanged(this, e);
        }

        #endregion


        #region text

        [Category("Qr Code"), Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Bindable(true), RefreshProperties(RefreshProperties.All),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

        #endregion

        public QrCodeImgControl()
        {
            UpdateImage();
        }


        internal void UpdateQrCodeCache()
        {
            if (!m_isLocked)
            {
                new QrEncoder(m_ErrorCorrectLevel).TryEncode(Text, out m_QrCode);
                OnQrMatrixChanged(new EventArgs());
                this.UpdateImage();
            }
        }

        protected virtual void OnQrMatrixChanged(EventArgs e)
        {
            if (QrMatrixChanged != null)
                QrMatrixChanged(this, e);
        }

        internal void UpdateImage()
        {
            if (!m_isFreezed)
            {
                using(MemoryStream ms = new MemoryStream())
                {
                    int width = this.Width < this.Height ? this.Width : this.Height;
                    int suitableWidth = m_QrCode.Matrix == null ? CalculateSuitableWidth(width, 21) : CalculateSuitableWidth(width, m_QrCode.Matrix.Width);
                    new GraphicsRenderer(new FixedCodeSize(suitableWidth, m_QuietZoneModule), m_darkBrush, m_LightBrush).WriteToStream(m_QrCode.Matrix, ImageFormat.Png, ms);
                    Bitmap bitmap = new Bitmap(ms);
                    this.Image = bitmap;
                }
            }
        }

        private int CalculateSuitableWidth(int width, int bitMatrixWidth)
        {
            FixedCodeSize isize = new FixedCodeSize(width, m_QuietZoneModule);
            DrawingSize dSize = isize.GetSize(bitMatrixWidth);
            int gap = dSize.CodeWidth - dSize.ModuleSize * (bitMatrixWidth + 2 * (int)m_QuietZoneModule);
            if (gap == 0)
                return width;
            else if (dSize.CodeWidth / gap < 6)
                return (dSize.ModuleSize + 1) * (bitMatrixWidth + 2 * (int)m_QuietZoneModule);
            else
                return dSize.ModuleSize * (bitMatrixWidth + 2 * (int)m_QuietZoneModule);
        }


        protected override void OnTextChanged(EventArgs e)
        {
            this.UpdateQrCodeCache();
            base.OnTextChanged(e);
        }


        /// <summary>
        /// Lock Class, that any change to Text or ErrorCorrectLevel won't cause it to update QrCode Matrix
        /// </summary>
        public void Lock()
        { m_isLocked = true; }

        /// <summary>
        /// Unlock Class, then update QrCodeMatrix and redraw image
        /// </summary>
        public void UnLock()
        {
            m_isLocked = false;
            UpdateQrCodeCache();
        }

        /// <summary>
        /// Freeze Class, Any value change to Brush, QuietZoneModule won't cause immediately redraw image. 
        /// </summary>
        public void Freeze()
        { m_isFreezed = true; }

        /// <summary>
        /// Unfreeze and redraw immediately. 
        /// </summary>
        public void UnFreeze()
        {
            m_isFreezed = false;
            UpdateImage();
        }

        /// <summary>
        /// Return whether if class is freezed. 
        /// </summary>
        public bool IsFreezed
        { get { return m_isFreezed; } }

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
