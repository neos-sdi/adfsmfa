using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
