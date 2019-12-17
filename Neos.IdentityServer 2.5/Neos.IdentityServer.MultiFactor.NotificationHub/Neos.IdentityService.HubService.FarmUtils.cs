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
    internal class FarmUtilities
    {
        #region Servers Configuration
        /// <summary>
        /// InitFarmNodeConfiguration method implementation
        /// </summary>        
        public string InitServerNodeConfiguration()
        {
            string result = null;
            try
            {
                RegistryVersion reg = new RegistryVersion();
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
        /// UnPackServerNodeConfiguration method implementation
        /// </summary>
        public ADFSServerHost UnPackServerNodeConfiguration(string data)
        {
            ADFSServerHost host = new ADFSServerHost();
            string[] ps = data.Split('|');
            RegistryVersion reg = new RegistryVersion();
            reg.VersionFromString(ps[0]);

            host.CurrentBuild = reg.CurrentBuild;
            host.CurrentMajorVersionNumber = reg.CurrentMajorVersionNumber;
            host.CurrentMinorVersionNumber = reg.CurrentMinorVersionNumber;
            host.CurrentVersion = reg.CurrentVersion;
            host.InstallationType = reg.InstallationType;
            host.ProductName = reg.ProductName;

            string[] px = ps[1].Split(';');
            host.FQDN = px[0];
            host.BehaviorLevel = Convert.ToInt32(px[1]);
            host.HeartbeatTmeStamp = Convert.ToDateTime(px[2]);
            host.NodeType = px[3];

            return host;
        }

        /// <summary>
        /// InitServerNodeConfiguration2012 method implementation
        /// </summary>
        private string InitServerNodeConfiguration2012(RegistryVersion reg)
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

            string result = reg.VersionAsString();
            result += "|";
            result += Dns.GetHostEntry("LocalHost").HostName + ";";
            result += "1;";
            result += new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0, DateTimeKind.Local)+";";
            result += nodetype + ";";
            return result;
        }

        /// <summary>
        /// InitServerNodeConfiguration2016 method implementation
        /// </summary>
        private string InitServerNodeConfiguration2016(RegistryVersion reg)
        {
            string result = string.Empty;
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
                    result = reg.VersionAsString();
                    result += "|";
                    result += item.Members["FQDN"].Value.ToString() + ";";
                    result += item.Members["BehaviorLevel"].Value.ToString()+";";
                    result += item.Members["HeartbeatTimeStamp"].Value.ToString()+";";
                    result += item.Members["NodeType"].Value.ToString()+";";
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
            return result;
        }

        /// <summary>
        /// InitServerNodeConfiguration2019 method implementation
        /// </summary>
        private string InitServerNodeConfiguration2019(RegistryVersion reg)
        {
            string result = string.Empty;
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
                    result = reg.VersionAsString();
                    result += "|";
                    result += item.Members["FQDN"].Value.ToString() + ";";
                    result += item.Members["BehaviorLevel"].Value.ToString() + ";";
                    result += item.Members["HeartbeatTimeStamp"].Value.ToString() + ";";
                    result += item.Members["NodeType"].Value.ToString() + ";";
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
            return result;
        }

        private string GetMachineName(string FQDN)
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
