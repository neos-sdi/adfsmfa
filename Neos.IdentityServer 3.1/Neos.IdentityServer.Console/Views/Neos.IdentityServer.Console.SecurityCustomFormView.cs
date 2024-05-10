//******************************************************************************************************************************************************************************************//
// Copyright (c) 2024 redhook (adfsmfa@gmail.com)                                                                                                                                    //                        
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ManagementConsole;
using Neos.IdentityServer.MultiFactor;
using Neos.IdentityServer.MultiFactor.Administration;
using System.Diagnostics;
using Microsoft.ManagementConsole.Advanced;
using System.Windows.Forms;
using System.Threading;

namespace Neos.IdentityServer.Console
{
    /// <summary>
    /// ServiceSQLFormView Class
    /// </summary>
    public class ServiceSecurityCustomFormView : FormView
    {
        private ServiceCustomSecurityScopeNode CustomScopeNode = null;
        private SecurityCustomViewControl CustomSecurityControl = null;

        /// <summary>
        /// ServiceSecurityCustomFormView constructor
        /// </summary>
        public ServiceSecurityCustomFormView()
        {

        }

        /// <summary>
        /// Initialize method override
        /// </summary>
        protected override void OnInitialize(AsyncStatus status)
        {
            CustomSecurityControl = (SecurityCustomViewControl)this.Control;
            CustomScopeNode = (ServiceCustomSecurityScopeNode)this.ScopeNode;
            CustomScopeNode.CustomSecurityFormView = this;

            ActionsPaneItems.Clear();
            SelectionData.ActionsPaneItems.Clear();
            SelectionData.ActionsPaneHelpItems.Clear();
            SelectionData.EnabledStandardVerbs = (StandardVerbs.Delete | StandardVerbs.Properties);
            ModeActionsPaneItems.Clear();

            base.OnInitialize(status);
        }

        /// <summary>
        /// Refresh() method implementation
        /// </summary>
        internal void Refresh()
        {
            if (CustomSecurityControl != null)
                CustomSecurityControl.RefreshData();
        }

        /// <summary>
        /// DoCancel() method implementation
        /// </summary>
        internal void DoCancel()
        {
            if (CustomSecurityControl != null)
                CustomSecurityControl.CancelData();
        }

        /// <summary>
        /// DoSave() method implmentation
        /// </summary>
        internal void DoSave()
        {
            if (CustomSecurityControl != null)
                CustomSecurityControl.SaveData();
        }
    }
}
