//******************************************************************************************************************************************************************************************//
// Copyright (c) 2022 @redhook62 (adfsmfa@gmail.com)                                                                                                                                    //                        
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
using res = Neos.IdentityServer.Console.Resources.Neos_IdentityServer_Console_ServiceFormView;

namespace Neos.IdentityServer.Console
{
    /// <summary>
    /// ServiceFormView Class
    /// </summary>
    public class ServiceFormView : FormView
    {

        private ServiceScopeNode serviceScopeNode = null;
        private ServiceViewControl serviceControl = null;

        /// <summary>
        /// StatusFormView constructor
        /// </summary>
        public ServiceFormView()
        {

        }

        /// <summary>
        /// Initialize method override
        /// </summary>
        protected override void OnInitialize(AsyncStatus status)
        {
            serviceControl = (ServiceViewControl)this.Control;
            serviceScopeNode = (ServiceScopeNode)this.ScopeNode;
            serviceScopeNode.serviceFormView = this;

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
            if (serviceControl != null)
                serviceControl.RefreshData();
        }

        /// <summary>
        /// ManageCerts method implmentation
        /// </summary>
        internal void ManageCerts()
        {
            MessageBoxParameters messageBoxParameters = new MessageBoxParameters
            {
                Caption = "Multi-Factor Authentication",
                Buttons = MessageBoxButtons.YesNo,
                DefaultButton = MessageBoxDefaultButton.Button1,
                Icon = MessageBoxIcon.Question,
                Text = res.CERTIFICATESSECURITYMSG
            };
            if (this.SnapIn.Console.ShowDialog(messageBoxParameters)==DialogResult.Yes)
            {
                try
                {
                    if (ManagementService.UpdateCertificatesACL())
                    {
                        MessageBoxParameters messageBoxParametersResult = new MessageBoxParameters
                        {
                            Caption = "Multi-Factor Authentication",
                            Buttons = MessageBoxButtons.OK,
                            DefaultButton = MessageBoxDefaultButton.Button1,
                            Icon = MessageBoxIcon.Information,
                            Text = res.CERTIFICATESSECURITYMSGRESULT
                        };
                        this.SnapIn.Console.ShowDialog(messageBoxParametersResult);
                    }
                }
                catch (Exception ex)
                {
                    MessageBoxParameters messageBoxParametersError = new MessageBoxParameters
                    {
                        Caption = "Multi-Factor Authentication Error",
                        Buttons = MessageBoxButtons.OK,
                        DefaultButton = MessageBoxDefaultButton.Button1,
                        Icon = MessageBoxIcon.Error,
                        Text = ex.Message
                    };
                    this.SnapIn.Console.ShowDialog(messageBoxParametersError);
                }
            }
            /*
            MessageBoxParameters messageBoxParameters2 = new MessageBoxParameters
            {
                Caption = "Multi-Factor Authentication",
                Buttons = MessageBoxButtons.YesNo,
                DefaultButton = MessageBoxDefaultButton.Button1,
                Icon = MessageBoxIcon.Question,
                Text = res.CERTIFICATESCLEANMSG
            };
            if (this.SnapIn.Console.ShowDialog(messageBoxParameters2)==DialogResult.Yes)
            {
                try
                {
                    int nb = ManagementService.CleanOrphanedPrivateKeys(0x00, 0);
                    MessageBoxParameters messageBoxParametersResult = new MessageBoxParameters
                    {
                        Caption = "Multi-Factor Authentication",
                        Buttons = MessageBoxButtons.OK,
                        DefaultButton = MessageBoxDefaultButton.Button1,
                        Icon = MessageBoxIcon.Information,
                        Text = string.Format(res.CERTIFICATESCLEANMSGCOUNT + " : {0}", nb)
                    };
                    this.SnapIn.Console.ShowDialog(messageBoxParametersResult);
                }
                catch (Exception ex)
                {
                    MessageBoxParameters messageBoxParametersError = new MessageBoxParameters
                    {
                        Caption = "Multi-Factor Authentication Error",
                        Buttons = MessageBoxButtons.OK,
                        DefaultButton = MessageBoxDefaultButton.Button1,
                        Icon = MessageBoxIcon.Error,
                        Text = ex.Message
                    };
                    this.SnapIn.Console.ShowDialog(messageBoxParametersError);
                }
            }*/
        }        
    }
}
