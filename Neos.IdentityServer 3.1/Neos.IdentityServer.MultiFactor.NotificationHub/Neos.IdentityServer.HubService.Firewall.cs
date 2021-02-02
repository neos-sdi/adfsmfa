using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetFwTypeLib;

namespace Neos.IdentityServer.MultiFactor
{
    /// <summary>
    /// MFAFirewall static class
    /// </summary>
    public static class MFAFirewall
    {
        /// <summary>
        /// AddFirewallRules method implmentation
        /// </summary>
        public static void AddFirewallRules(string computers)
        { 
            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            INetFwRule inboundRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
            INetFwRule outboundRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));

            inboundRule.Name = "MFA InBound TCP Rule";
            inboundRule.Enabled = true;
            inboundRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            inboundRule.Protocol = 6; // TCP
            inboundRule.LocalPorts = "5987";  // Set port number
            inboundRule.Description = "MFA InBound TCP Rule";
            inboundRule.Grouping = "MFA";
            inboundRule.RemoteAddresses = computers;
            inboundRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
            inboundRule.Profiles = (int)(MFAFirewallProfiles.Domain);           
            firewallPolicy.Rules.Add(inboundRule);

            outboundRule.Name = "MFA OutBound TCP Rule";
            outboundRule.Enabled = true;
            outboundRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            outboundRule.Protocol = 6; // TCP
            outboundRule.RemotePorts = "5987";  // Set port number
            outboundRule.Description = "MFA OutBound TCP Rule";
            outboundRule.Grouping = "MFA";
            outboundRule.RemoteAddresses = computers;
            outboundRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
            outboundRule.Profiles = (int)(MFAFirewallProfiles.Domain);           
            firewallPolicy.Rules.Add(outboundRule);          
        }

        /// <summary>
        /// RemoveFirewallRules method implmentation
        /// </summary>
        public static void RemoveFirewallRules()
        {
            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

            firewallPolicy.Rules.Remove("MFAIN1");
            firewallPolicy.Rules.Remove("MFAIN2");
            firewallPolicy.Rules.Remove("MFAOUT1");
            firewallPolicy.Rules.Remove("MFAOUT2");
            firewallPolicy.Rules.Remove("MFAIN");
            firewallPolicy.Rules.Remove("MFAOUT");

            firewallPolicy.Rules.Remove("MFATCPIN");
            firewallPolicy.Rules.Remove("MFATCPOUT");
            firewallPolicy.Rules.Remove("MFAUDPIN");
            firewallPolicy.Rules.Remove("MFAUDPOUT");

            firewallPolicy.Rules.Remove("MFA InBound TCP Rule");
            firewallPolicy.Rules.Remove("MFA OutBound TCP Rule");
        }
    }

    /// <summary>
    /// FirewallProfiles emum 
    /// </summary>
    public enum MFAFirewallProfiles
    {
        Domain = 1,
        Private = 2,
        Public = 4
    }
}
