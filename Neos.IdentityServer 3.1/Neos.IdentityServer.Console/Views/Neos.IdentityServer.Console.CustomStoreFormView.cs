//******************************************************************************************************************************************************************************************//
// Copyright (c) 2023 redhook (adfsmfa@gmail.com)                                                                                                                                    //                        
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

namespace Neos.IdentityServer.Console
{
    /// <summary>
    /// ServiceCustomStoreFormView Class
    /// </summary>
    public class ServiceCustomStoreFormView : FormView
    {
        private ServiceCustomStorageScopeNode custScopeNode = null;
        private CustomStoreViewControl custcontrol = null;

        /// <summary>
        /// ServiceCustomStoreFormView constructor
        /// </summary>
        public ServiceCustomStoreFormView()
        {

        }

        /// <summary>
        /// Initialize method override
        /// </summary>
        protected override void OnInitialize(AsyncStatus status)
        {
            custcontrol = (CustomStoreViewControl)this.Control;
            custScopeNode = (ServiceCustomStorageScopeNode)this.ScopeNode;
            custScopeNode.CustomStoreFormView = this;

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
            if (custcontrol != null)
                custcontrol.RefreshData();
        }

        /// <summary>
        /// DoCancel() method implementation
        /// </summary>
        internal void DoCancel()
        {
            if (custcontrol != null)
                custcontrol.CancelData();
        }

        /// <summary>
        /// DoSave() method implmentation
        /// </summary>
        internal void DoSave()
        {
            if (custcontrol != null)
                custcontrol.SaveData();
        }
    }
}
