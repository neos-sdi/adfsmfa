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
using System.Drawing;
using Neos.IdentityServer.MultiFactor.QrEncoding.Windows.Render;
using System.ComponentModel;

namespace Neos.IdentityServer.MultiFactor.QrEncoding.Windows.Forms
{
    public class QrCodeGraphicControl : Control
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
                    if (!m_isFreezed)
                        this.Invalidate();
                }
            }
        }

        protected virtual void OnDarkBrushChanged(EventArgs e)
        {
            if(DarkBrushChanged != null)
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
                    if (!m_isFreezed)
                        this.Invalidate();
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
                    if(!m_isFreezed)
                        this.Invalidate();
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

                    this.UpdateQrCodeCache();
                    OnErrorCorrectLevelChanged(new EventArgs());
                }
            }
        }

        protected virtual void OnErrorCorrectLevelChanged(EventArgs e)
        {
            if (ErrorCorrectLevelChanged != null)
                ErrorCorrectLevelChanged(this, e);
        }

        #endregion

        protected override void OnPaint(PaintEventArgs e)
        {
            
            int offsetX, offsetY, width;
            if (this.Width <= this.Height)
            {
                offsetX = 0;
                offsetY = (this.Height - this.Width) / 2;
                width = this.Width;
            }
            else
            {
                offsetX = (this.Width - this.Height) / 2;
                offsetY = 0;
                width = this.Height;
            }

            new GraphicsRenderer(new FixedCodeSize(width, m_QuietZoneModule), m_darkBrush, m_LightBrush).Draw(e.Graphics, m_QrCode.Matrix, new Point(offsetX, offsetY));

            base.OnPaint(e);
        }

        internal void UpdateQrCodeCache()
        {
            if (!m_isLocked)
            {
                new QrEncoder(m_ErrorCorrectLevel).TryEncode(Text, out m_QrCode);
                OnQrMatrixChanged(new EventArgs());
                this.Invalidate();
            }
        }

        protected virtual void OnQrMatrixChanged(EventArgs e)
        {
            if (QrMatrixChanged != null)
                QrMatrixChanged(this, e);
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
        /// Unlock Class, then update QrCodeMatrix and repaint
        /// </summary>
        public void UnLock()
        { 
            m_isLocked = false;
            UpdateQrCodeCache();
        }

        /// <summary>
        /// Freeze Class, Any value change to Brush, QuietZoneModule won't cause immediately repaint. 
        /// </summary>
        /// <remarks>It won't stop any repaint cause by other action.</remarks>
        public void Freeze()
        { m_isFreezed = true; }

        /// <summary>
        /// Unfreeze and repaint immediately. 
        /// </summary>
        public void UnFreeze()
        { 
            m_isFreezed = false;
            Invalidate();
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
