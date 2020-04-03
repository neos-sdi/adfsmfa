using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor.Administration
{
    /// <summary>
    /// SvcController class implementation
    /// WinRM implementation
    /// </summary>
    public class SvcController
    {
        /// <summary>
        /// SvcController constructor
        /// </summary>
        public SvcController()
        {
            this.ServiceName = string.Empty;
            this.MachineName = "localhost";
        }

        /// <summary>
        /// SvcController constructor
        /// </summary>
        public SvcController(string name)
        {
            this.ServiceName = name;
            this.MachineName = "localhost";
        }

        /// <summary>
        /// SvcController constructor
        /// </summary>
        public SvcController(string name, string machineName)
        {
            this.ServiceName = name;
            this.MachineName = machineName;
        }

        /// <summary>
        /// Status property implemenatation
        /// </summary>
        public ServiceControllerStatus Status { get; private set; }
        
        public string ServiceName { get; private set; }

        public string DisplayName { get; private set; }

        public string MachineName { get; private set; }

        public bool CanStop { get; private set; } = true;

        public bool CanShutdown { get; private set; } = true;

        public bool CanPauseAndContinue { get; private set; } = true;

        /// <summary>
        /// Close method implementation
        /// </summary>
        public void Close()
        {
            this.ServiceName = string.Empty;
            this.MachineName = "localhost";
        }

        /// <summary>
        /// Continue method implementation
        /// PowerShell Resume-Service
        /// </summary>
        public void Continue()
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                bool islocal = (Dns.GetHostEntry("LocalHost").HostName.ToLower().Equals(MachineName.ToLower()));
                if (!islocal)
                {
                    WSManConnectionInfo connectionInfo = new WSManConnectionInfo(false, MachineName, 5985, "/wsman", "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", null, 30000);
                    SPRunSpace = RunspaceFactory.CreateRunspace(connectionInfo);
                    connectionInfo.AuthenticationMechanism = AuthenticationMechanism.NegotiateWithImplicitCredential;
                }
                else
                {
                    SPRunSpace = RunspaceFactory.CreateRunspace();
                }

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command cmd = new Command("Resume-Service " + this.ServiceName, true);
                pipeline.Commands.Add(cmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
        }

        /// <summary>
        /// Pause method imlementation
        /// PowerShell Suspend-Service
        /// </summary>
        public void Pause()
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                bool islocal = (Dns.GetHostEntry("LocalHost").HostName.ToLower().Equals(MachineName.ToLower()));
                if (!islocal)
                {
                    WSManConnectionInfo connectionInfo = new WSManConnectionInfo(false, MachineName, 5985, "/wsman", "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", null, 30000);
                    SPRunSpace = RunspaceFactory.CreateRunspace(connectionInfo);
                    connectionInfo.AuthenticationMechanism = AuthenticationMechanism.NegotiateWithImplicitCredential;
                }
                else
                {
                    SPRunSpace = RunspaceFactory.CreateRunspace();
                }

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command cmd = new Command("Suspend-Service " + this.ServiceName, true);
                pipeline.Commands.Add(cmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
        }

        /// <summary>
        /// Start method implmentation
        /// PowerShell Start-Service
        /// </summary>
        public void Start()
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                bool islocal = (Dns.GetHostEntry("LocalHost").HostName.ToLower().Equals(MachineName.ToLower()));
                if (!islocal)
                {
                    WSManConnectionInfo connectionInfo = new WSManConnectionInfo(false, MachineName, 5985, "/wsman", "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", null, 30000);
                    SPRunSpace = RunspaceFactory.CreateRunspace(connectionInfo);
                    connectionInfo.AuthenticationMechanism = AuthenticationMechanism.NegotiateWithImplicitCredential;
                }
                else
                {
                    SPRunSpace = RunspaceFactory.CreateRunspace();
                }

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command cmd = new Command("Start-Service " + this.ServiceName, true);
                pipeline.Commands.Add(cmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
        }

        /// <summary>
        /// Stop method implementation
        /// PowserShell Stop-Service
        /// </summary>
        public void Stop()
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                bool islocal = (Dns.GetHostEntry("LocalHost").HostName.ToLower().Equals(MachineName.ToLower()));
                if (!islocal)
                {
                    WSManConnectionInfo connectionInfo = new WSManConnectionInfo(false, MachineName, 5985, "/wsman", "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", null, 30000);
                    SPRunSpace = RunspaceFactory.CreateRunspace(connectionInfo);
                    connectionInfo.AuthenticationMechanism = AuthenticationMechanism.NegotiateWithImplicitCredential;
                }
                else
                {
                    SPRunSpace = RunspaceFactory.CreateRunspace();
                }

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command cmd = new Command("Stop-Service " + this.ServiceName, true);
                pipeline.Commands.Add(cmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
        }

        /// <summary>
        /// Refresh method implementation
        /// PowserShell Get-Service
        /// </summary>
        public void Refresh()
        {
            Open();
        }

        /// <summary>
        /// WaitForStatus method implementation
        /// </summary>
        public void WaitForStatus(ServiceControllerStatus desiredStatus)
        {
            while (this.Status!=desiredStatus)
            {
                Open();
            }
        }

        /// <summary>
        /// WaitForStatus method implementation
        /// </summary>
        public void WaitForStatus(ServiceControllerStatus desiredStatus, TimeSpan timeout)
        {
            TimeSpan duration = new TimeSpan(0, 0, 0);
            while (this.Status != desiredStatus)
            {
                Open();
                Thread.Sleep(1000);
                duration.Add(new TimeSpan(0, 0, 1));
                if (timeout < duration)
                    break;
            }
        }

        #region Private methods
        private void Open()
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                bool islocal = (Dns.GetHostEntry("LocalHost").HostName.ToLower().Equals(MachineName.ToLower()));
                if (!islocal)
                {
                    WSManConnectionInfo connectionInfo = new WSManConnectionInfo(false, MachineName, 5985, "/wsman", "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", null, 30000);
                    SPRunSpace = RunspaceFactory.CreateRunspace(connectionInfo);
                    connectionInfo.AuthenticationMechanism = AuthenticationMechanism.NegotiateWithImplicitCredential;
                }
                else
                {
                    SPRunSpace = RunspaceFactory.CreateRunspace();
                }

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command cmd = new Command("Get-Service " + this.ServiceName, true);
                pipeline.Commands.Add(cmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();
                foreach (var result in PSOutput)
                {
                    ServiceName = result.Members["Name"].Value.ToString();
                    DisplayName = result.Members["DisplayName"].Value.ToString();
                    string _status = result.Members["Status"].Value.ToString();
                    Status = StringToStatus(_status);
                    CanPauseAndContinue = Convert.ToBoolean(result.Members["CanPauseAndContinue"].Value);
                    CanShutdown = Convert.ToBoolean(result.Members["CanShutdown"].Value);
                    CanStop = Convert.ToBoolean(result.Members["CanStop"].Value);
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
        }        

        /// <summary>
        /// StringToStatus method implementation
        /// </summary>
        private ServiceControllerStatus StringToStatus(string value)
        {
            if (value.ToLower().Equals("stopped"))
                return ServiceControllerStatus.Stopped;
            if (value.ToLower().Equals("startpending"))
                return ServiceControllerStatus.StartPending;
            if (value.ToLower().Equals("stoppending"))
                return ServiceControllerStatus.StopPending;
            if (value.ToLower().Equals("running"))
                return ServiceControllerStatus.Running;
            if (value.ToLower().Equals("continuepending"))
                return ServiceControllerStatus.ContinuePending;
            if (value.ToLower().Equals("pausepending"))
                return ServiceControllerStatus.PausePending;
            if (value.ToLower().Equals("paused"))
                return ServiceControllerStatus.Paused;
            return ServiceControllerStatus.Running;
        }
        #endregion
    }
}
