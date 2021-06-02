//******************************************************************************************************************************************************************************************//
// Copyright (c) 2021 @redhook62 (adfsmfa@gmail.com)                                                                                                                                    //                        
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
//                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using Microsoft.ManagementConsole;
using System;
using System.Windows.Forms;

namespace Neos.IdentityServer.Console
{
    public partial class RootViewControl : UserControl, IFormViewControl
    {

        private Control oldParent;
        private RootFormView _frm = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public RootViewControl()
        {
            InitializeComponent();
        }

                /// <summary>
        /// Initialize method 
        /// </summary>
        public void Initialize(FormView view)
        {
            FormView = (RootFormView)view;
            OnInitialize();
        }

        /// <summary>
        /// OnInitialize method
        /// </summary>
        protected virtual void OnInitialize()
        {
        }

        /// <summary>
        /// RefreshData method
        /// </summary>
        internal void RefreshData()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RootViewControl));
            this.GlobalViewlabel.Text = resources.GetString("GlobalViewlabel.Text");
            this.GlobalViewLabelText.Text = resources.GetString("GlobalViewLabelText.Text");
        }

        #region Properties
        /// <summary>
        /// FormView property implementation
        /// </summary>
        protected RootFormView FormView
        {
            get { return _frm; }
            private set { _frm = value; }
        }

        /// <summary>
        /// SnapIn method implementation
        /// </summary>
        protected NamespaceSnapInBase SnapIn
        {
            get { return this.FormView.ScopeNode.SnapIn; }
        }

        /// <summary>
        /// ScopeNode method implementation
        /// </summary>
        protected RootScopeNode ScopeNode
        {
            get { return this.FormView.ScopeNode as RootScopeNode; }
        }

        /// <summary>
        /// OnParentChanged method override
        /// </summary>
        protected override void OnParentChanged(EventArgs e)
        {
            if (Parent != null)
            {
                if (!DesignMode)
                    Size = Parent.ClientSize;
                Parent.SizeChanged += Parent_SizeChanged;
            }
            if (oldParent != null)
            {
                oldParent.SizeChanged -= Parent_SizeChanged;
            }
            oldParent = Parent;
            base.OnParentChanged(e);
        }

        /// <summary>
        /// Parent_SizeChanged event
        /// </summary>
        private void Parent_SizeChanged(object sender, EventArgs e)
        {
            if (!DesignMode)
                Size = Parent.ClientSize;
        }
        #endregion

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.neos-sdi.com");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://adfsmfa.codeplex.com");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/neos-sdi/adfsmfa/releases");
        }
    }
}
