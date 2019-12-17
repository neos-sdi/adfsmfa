//******************************************************************************************************************************************************************************************//
// Copyright (c) 2019 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
// https://adfsmfa.codeplex.com                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//

using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Neos.IdentityServer.MultiFactor.NotificationHub
{
    internal static class FarmUtilities
    {
        #region Servers Configuration
        /// <summary>
        /// InitFarmNodeConfiguration method implementation
        /// </summary>        
        public static NamedPipeRegistryRecord InitServerNodeConfiguration(NamedPipeRegistryRecord reg)
        {
            NamedPipeRegistryRecord result = default(NamedPipeRegistryRecord);
            try
            {
                if (reg.IsWindows2019)
                    result = InitServerNodeConfiguration2019(reg);
                else if (reg.IsWindows2016)
                    result = InitServerNodeConfiguration2016(reg);
                else if (reg.IsWindows2012R2)
                    result = InitServerNodeConfiguration2012(reg);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// InitServerNodeConfiguration2012 method implementation
        /// </summary>
        public static string InitServerNodeType()
        {
            string nodetype = string.Empty;
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);
                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("(Get-AdfsSyncProperties).Role", true);
                pipeline.Commands.Add(exportcmd);

                Collection<PSObject> PSOutput = pipeline.Invoke();
                foreach (var prop in PSOutput)
                {
                    nodetype = prop.BaseObject.ToString();
                    break;
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
            return nodetype;
        }

        /// <summary>
        /// InitServerNodeConfiguration2012 method implementation
        /// </summary>
        private static NamedPipeRegistryRecord InitServerNodeConfiguration2012(NamedPipeRegistryRecord reg)
        {
            reg.MachineName = GetMachineName(reg.FQDN);
            reg.BehaviorLevel = 1;
            reg.HeartbeatTimestamp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0, DateTimeKind.Local);
            return reg;
        }

        /// <summary>
        /// InitServerNodeConfiguration2016 method implementation
        /// </summary>
        private static NamedPipeRegistryRecord InitServerNodeConfiguration2016(NamedPipeRegistryRecord reg)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);
                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("(Get-AdfsFarmInformation).FarmNodes", true);

                pipeline.Commands.Add(exportcmd);

                Collection<PSObject> PSOutput = pipeline.Invoke();
                foreach (var item in PSOutput)
                {
                    if (reg.FQDN.ToLower().Equals(item.Members["FQDN"].Value.ToString().ToLower()))
                    { 
                        reg.FQDN = item.Members["FQDN"].Value.ToString();
                        reg.MachineName = GetMachineName(reg.FQDN);
                        reg.BehaviorLevel = Convert.ToInt32(item.Members["BehaviorLevel"].Value);
                        reg.HeartbeatTimestamp = Convert.ToDateTime(item.Members["HeartbeatTimeStamp"].Value);
                        break;
                    }
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
            return reg;
        }

        /// <summary>
        /// InitServerNodeConfiguration2019 method implementation
        /// </summary>
        private static NamedPipeRegistryRecord InitServerNodeConfiguration2019(NamedPipeRegistryRecord reg)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);
                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("(Get-AdfsFarmInformation).FarmNodes", true);

                pipeline.Commands.Add(exportcmd);

                Collection<PSObject> PSOutput = pipeline.Invoke();
                foreach (var item in PSOutput)
                {
                    if (reg.FQDN.ToLower().Equals(item.Members["FQDN"].Value.ToString().ToLower()))
                    {
                        reg.FQDN = item.Members["FQDN"].Value.ToString();
                        reg.MachineName = GetMachineName(reg.FQDN);
                        reg.BehaviorLevel = Convert.ToInt32(item.Members["BehaviorLevel"].Value);
                        reg.HeartbeatTimestamp = Convert.ToDateTime(item.Members["HeartbeatTimeStamp"].Value);
                        break;
                    }
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
            return reg;
        }

        private static string GetMachineName(string FQDN)
        {
            string[] svr = FQDN.Split('.');
            if (svr.Length >= 1)
                return svr[0];
            else
                return string.Empty;
        }
        #endregion
    }
}
