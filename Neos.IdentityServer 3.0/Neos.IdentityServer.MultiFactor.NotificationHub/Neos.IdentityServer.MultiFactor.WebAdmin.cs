using Neos.IdentityServer.MultiFactor.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor
{
    #region WebThemeManager
    /// <summary>
    /// WebThemeManager class implementation
    /// </summary>
    public class WebAdminManager
    {
        private static Dictionary<string, bool> _servers = new Dictionary<string, bool>();
        private static ACLParametersRecord _aclrecord = new ACLParametersRecord();
        private EventLog _log;

        /// <summary>
        /// Constructor implmentation
        /// </summary>
        public WebAdminManager(IDependency dep)
        {
            _log = dep.GetEventLog();
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public bool Initialize(Dictionary<string, bool> servers)
        {
            _servers = servers;
            return true;
        }

        /// <summary>
        /// DispatchACLs method implmentation
        /// </summary>
        public ACLParametersRecord GetAdministrativeACL(string domain, string account, string password, string path)
        {
            try
            {
                if (_aclrecord.Loaded != true)
                {
                    if (IsPrimaryComputer(_servers))
                        _aclrecord = Certs.FetchACLs(domain, account, password, path);
                    else
                        _aclrecord = DoRequestAdministrativeACL(_servers, domain, account, password, path);
                }
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service GetAdministrative ACL method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                _aclrecord.Loaded = false;
            }
            return _aclrecord;
        }

        /// <summary>
        /// DoRequestAdministrativeACL method implementation
        /// </summary>
        private ACLParametersRecord DoRequestAdministrativeACL(Dictionary<string, bool> servers, string domain, string account, string password, string path)
        {
            string fqdn = Dns.GetHostEntry("localhost").HostName;
            List<string> servernames = (from server in servers
                                        where (server.Key.ToLower() != fqdn.ToLower() && (server.Value == true))
                                        select server.Key.ToLower()).ToList<string>();
            if (servernames != null)
            {
                foreach (string srvfqdn in servernames)
                {
                    WebAdminClient webthemeclient = new WebAdminClient();
                    try
                    {
                        webthemeclient.Initialize(srvfqdn);
                        IWebAdminServices client = webthemeclient.Open();
                        try
                        {
                            ACLParametersRecord rec = client.GetAdministrativeACL(domain, account, password, path);
                            if (rec.Loaded)
                               Certs.ApplyACLs(rec, path);
                            return rec;
                        }
                        catch (Exception e)
                        {
                            webthemeclient.UnInitialize();
                            _log.WriteEntry(string.Format("Error calling  DoRequestAdministrativeACL method : {0} => {1}.", srvfqdn, e.Message), EventLogEntryType.Error, 2011);
                        }
                        finally
                        {
                            webthemeclient.Close(client);
                        }
                    }
                    catch (Exception e)
                    {
                        _log.WriteEntry(string.Format("Error calling  DoRequestAdministrativeACL method : {0} => {1}.", srvfqdn, e.Message), EventLogEntryType.Error, 2011);
                        return new ACLParametersRecord() { Loaded = false };
                    }
                }
            }
            return new ACLParametersRecord() { Loaded = false };
        }

        /// <summary>
        /// IsPrimaryComputer method implementation
        /// </summary>
        private bool IsPrimaryComputer(Dictionary<string, bool> servers)
        {
            string fqdn = Dns.GetHostEntry("localhost").HostName;
            List<string> servernames = (from server in servers
                                        where (server.Key.ToLower() == fqdn.ToLower() && (server.Value == true))
                                        select server.Key.ToLower()).ToList<string>();
            return (servernames.Count == 1);
        }

    }
    #endregion
}
