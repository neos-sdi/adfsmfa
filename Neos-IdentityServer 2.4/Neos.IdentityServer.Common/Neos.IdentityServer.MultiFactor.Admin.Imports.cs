using Neos.IdentityServer.MultiFactor.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Neos.IdentityServer.MultiFactor.Administration
{
    public class ImportUsersBase
    {
        private  MFAConfig _config;
        private bool _forcenewKey = false;
        private bool _sendemail = false;
        private int _recordcount = 0;
        private int _errorscount = 0;
        private bool _disableall = false;
        private bool _nologfile = false;

        /// <summary>
        /// ImportUsersBase constructor
        /// </summary>
        public ImportUsersBase()
        {

        }

        /// <summary>
        /// ImportUsersBase constructor
        /// </summary>
        public ImportUsersBase(MFAConfig config)
        {
            Config = config;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get { return _config; }
            set {_config = value; }
        }

        /// <summary>
        /// ForceNewKey property
        /// </summary>
        public bool ForceNewKey
        {
            get { return _forcenewKey; }
            set { _forcenewKey = value; }
        }

        /// <summary>
        /// ForceNewKey property
        /// </summary>
        public bool SendEmail
        {
            get { return _sendemail; }
            set { _sendemail = value; }
        }

        /// <summary>
        /// DisableAll property
        /// </summary>
        public bool DisableAll
        {
            get { return _disableall; }
            set { _disableall = value; }
        }

        /// <summary>
        /// DisableAll property
        /// </summary>
        public bool NoLogFile
        {
            get { return _nologfile; }
            set { _nologfile = value; }
        }

        /// <summary>
        /// RecordsCount property
        /// </summary>
        public int RecordsCount
        {
            get { return _recordcount; }
            set { _recordcount = value; }
        }

        /// <summary>
        /// ErrorsCount property
        /// </summary>
        public int ErrorsCount
        {
            get { return _errorscount; }
            set { _errorscount = value; }
        }

        /// <summary>
        /// RecordsImported property
        /// </summary>
        public int RecordsImported
        {
            get { return _recordcount - _errorscount; }
        }

        /// <summary>
        /// InitializeTrace method implementation
        /// </summary>
        protected virtual TraceListener InitializeTrace(string filename)
        {
            DefaultTraceListener listen = new DefaultTraceListener();
            listen.Name = "MFATrace";
            Trace.Listeners.Add(listen);
            listen.TraceOutputOptions = TraceOptions.DateTime;
            if (!NoLogFile)
                listen.LogFileName = filename + "_" + DateTime.Now.ToFileTime()+".log";
            return listen;
        }

        /// <summary>
        /// FinalizeTrace method implmentation
        /// </summary>
        protected virtual void FinalizeTrace(TraceListener listen)
        {
            Trace.Flush();
            Trace.Close();
            listen.Close();
            Trace.Listeners.Remove("MFATrace");
        }

        /// <summary>
        /// DoImport base method
        /// </summary>
        public virtual bool DoImport()
        {
            return false;
        }
    }

    #region import ADDS
    public class ImportUsersADDS : ImportUsersBase
    {
        /// <summary>
        /// DomainName property
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// UserName property
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Password property
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// LDAPPath property
        /// </summary>
        public string LDAPPath { get; set; }

        /// <summary>
        /// CreatedSince property
        /// </summary>
        public DateTime? CreatedSince { get; set; }

        /// <summary>
        /// ModifiedSince property
        /// </summary>
        public DateTime? ModifiedSince { get; set; }

        /// <summary>
        /// MailAttribute property
        /// </summary>
        public string MailAttribute { get; set; }

        /// <summary>
        /// PhoneAttribute property
        /// </summary>
        public string PhoneAttribute { get; set; }

        /// <summary>
        /// Method  property
        /// </summary>
        public PreferredMethod Method { get; set; }

        /// <summary>
        /// ImportUsersCSV constructor
        /// </summary>
        public ImportUsersADDS(MFAConfig cfg) : base(cfg) { }


        /// <summary>
        /// DoImport() method implmentation
        /// </summary>
        public override bool DoImport()
        {
            string filename = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\MFA\\adimport-" + DateTime.Now.ToFileTime().ToString() + ".log";
            TraceListener listen = InitializeTrace(filename);
            try
            {
                ADDSHost adht = Config.Hosts.ActiveDirectoryHost;
                if (string.IsNullOrEmpty(DomainName))
                    DomainName = adht.DomainName;
                if (string.IsNullOrEmpty(UserName))
                    UserName = adht.Account;
                if (string.IsNullOrEmpty(Password))
                    Password = adht.Password;

                DataRepositoryService client = new ADDSDataRepositoryService(adht, Config.DeliveryWindow);
                Trace.WriteLine("");
                Trace.WriteLine(string.Format("Importing for AD : {0}", LDAPPath));
                Trace.Indent();
                Trace.WriteLine("Querying users from AD");
                RegistrationList lst = client.GetImportUserRegistrations(DomainName, UserName, Password, LDAPPath, CreatedSince, ModifiedSince, MailAttribute, PhoneAttribute, Method, DisableAll);
                Trace.WriteLine(string.Format("Querying return {0} users from AD", lst.Count.ToString()));

                DataRepositoryService client2 = null;
                if (Config.UseActiveDirectory)
                {
                    Trace.WriteLine("");
                    Trace.WriteLine("Importing ADDS Mode");
                    Trace.Indent();
                    client2 = new ADDSDataRepositoryService(Config.Hosts.ActiveDirectoryHost, Config.DeliveryWindow);
                }
                else
                {
                    Trace.WriteLine("");
                    Trace.WriteLine("Importing SQL Mode");
                    Trace.Indent();
                    client2 = new SQLDataRepositoryService(Config.Hosts.SQLServerHost, Config.DeliveryWindow);
                }
                client2.OnKeyDataEvent += KeyDataEvent;
                foreach (Registration reg in lst)
                {
                    Trace.TraceInformation(string.Format("Importing user {0} from AD", reg.UPN));
                    try
                    {
                        reg.PIN = Config.DefaultPin;
                        client2.AddUserRegistration(reg, ForceNewKey, true);
                        Trace.TraceInformation(string.Format("User {0} Imported in MFA", reg.UPN));
                    }
                    catch (Exception ex)
                    {
                        ErrorsCount++;
                        Trace.TraceError("Error importing Record N° {0} \r\r {1}", (RecordsCount + 1).ToString(), ex.Message);
                    }
                    finally
                    {
                        RecordsCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(string.Format("Error importing from AD \r\r {0}", ex.Message));
                return false;
            }
            finally
            {
                Trace.Unindent();
                FinalizeTrace(listen);
            }
            return true;
        }

        /// <summary>
        /// KeyDataEvent method implementation
        /// </summary>
        internal void KeyDataEvent(string user, KeysDataManagerEventKind kind)
        {
            switch (kind)
            {
                case KeysDataManagerEventKind.add:
                    KeysManager.NewKey(user);
                    break;
                case KeysDataManagerEventKind.Get:
                    KeysManager.ReadKey(user);
                    break;
                case KeysDataManagerEventKind.Remove:
                    KeysManager.RemoveKey(user);
                    break;
            }
        }
    }
    #endregion

    #region Import CSV
    public class ImportUsersCSV : ImportUsersBase
    {
        /// <summary>
        /// ImportUsersCSV constructor
        /// </summary>
        public ImportUsersCSV(MFAConfig cfg) : base(cfg) { }

        /// <summary>
        /// FileName property implementation
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// DoImport method implementation
        /// </summary>
        public override bool DoImport()
        {
            if (Config == null)
                throw new ArgumentNullException("Config", "MFA Configuration must be passed to the import process !");
            if (!File.Exists(FileName))
                throw new ArgumentException("Invalid FileName for import process !", "FileName");

            var contents = File.ReadAllText(FileName).Split('\n');
            var csv = from line in contents select line.Split(';').ToArray();
            var header = csv.First(r => r.Length > 1 && r.Last().Trim().Length > 0);
            int?[] ids = GetCSVHeadersId(header);

            var listen = InitializeTrace(FileName);
            try
            {
                Trace.WriteLine("");
                Trace.WriteLine(string.Format("Importing file : {0}", FileName));
                Trace.Indent();
                foreach (var row in csv.Skip(1).TakeWhile(r => r.Length > 1 && r.Last().Trim().Length > 0))
                {
                    Trace.TraceInformation("Importing record N° {0}", (RecordsCount+1).ToString());
                    try
                    {
                        Registration reg = new Registration();
                        if ((ids[0].HasValue) && (!string.IsNullOrEmpty(row[ids[0].Value])))
                            reg.UPN = row[ids[0].Value];
                        else
                            throw new InvalidDataException("upn must be provided !");

                        if ((ids[1].HasValue) && (!string.IsNullOrEmpty(row[ids[1].Value])))
                            reg.MailAddress = row[ids[1].Value];
                        else if (Config.MailProvider.Enabled)
                            throw new InvalidDataException("email must be provided !");

                        if ((ids[2].HasValue) && (!string.IsNullOrEmpty(row[ids[2].Value])))
                            reg.PhoneNumber = row[ids[2].Value];
                        else if (Config.ExternalProvider.Enabled)
                            throw new InvalidDataException("mobile must be provided !");

                        if ((ids[3].HasValue) && (!string.IsNullOrEmpty(row[ids[3].Value])))
                            reg.PreferredMethod = (PreferredMethod)Enum.Parse(typeof(PreferredMethod), row[ids[3].Value]);
                        else
                            reg.PreferredMethod = PreferredMethod.Choose;

                        if (DisableAll)
                            reg.Enabled = false;
                        else if ((ids[4].HasValue) && (!string.IsNullOrEmpty(row[ids[4].Value])))
                            reg.Enabled = bool.Parse(row[ids[4].Value]);
                        else
                            reg.Enabled = true;

                        RuntimeRepository.AddUserRegistration(Config, reg, ForceNewKey, true, SendEmail); // Can also Update
                        Trace.TraceInformation("Record N° {0} imported for user : {1} !", (RecordsCount+1).ToString(), reg.UPN);
                    }
                    catch (Exception ex)
                    {
                        ErrorsCount++;
                        Trace.TraceError("Error importing Record N° {0} \r\r {1}", (RecordsCount + 1).ToString(), ex.Message);
                    }
                    finally
                    {
                        RecordsCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                return false;
            }
            finally
            {
                Trace.Unindent();
                FinalizeTrace(listen);
            }
            return true;
        }

        /// <summary>
        /// GetCSVHeadersId implmentation
        /// </summary>
        private int?[] GetCSVHeadersId(string[] header)
        {
            int?[] tmp = new int?[5] { null, null, null, null, null };
            int pos = 0;

            foreach (string s in header)
            {
                string hs = s.Replace("\r", "").Replace("\n", "");
                if (hs.ToLower().Equals("upn"))
                    tmp[0] = pos;
                else if (hs.ToLower().Equals("email"))
                    tmp[1] = pos;
                else if (hs.ToLower().Equals("mobile"))
                    tmp[2] = pos;
                else if (hs.ToLower().Equals("method"))
                    tmp[3] = pos;
                else if (hs.ToLower().Equals("enabled"))
                    tmp[4] = pos;
                pos++;
            }
            return tmp;
        }
    }
    #endregion

    #region imports XML
    public class ImportUsersXML : ImportUsersBase
    {
        /// <summary>
        /// ImportUsersCSV constructor
        /// </summary>
        public ImportUsersXML(MFAConfig cfg) : base(cfg) { }

        /// <summary>
        /// FileName property implementation
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// DoImport method implementation
        /// </summary>
        public override bool DoImport()
        {
            if (Config == null)
                throw new ArgumentNullException("Config", "MFA Configuration must be passed to the import process !");
            if (!File.Exists(FileName))
                throw new ArgumentException("Invalid FileName for import process !", "FileName");

            var xml = XDocument.Load(FileName);
            var listen = InitializeTrace(FileName);
            try
            {
                Trace.WriteLine("");
                Trace.WriteLine(string.Format("Importing file : {0}", FileName));
                Trace.Indent();

                foreach (var row in xml.Root.Descendants("User"))
                {
                    Trace.TraceInformation("Importing record N° {0}", (RecordsCount+1).ToString());     
                    try
                    {
                        Registration reg = new Registration();
                        if (row.Attribute("upn") != null)
                            reg.UPN = row.Attribute("upn").Value;
                        else
                            throw new InvalidDataException("upn must be provided !");

                        if (row.Attribute("email") != null)
                            reg.MailAddress = row.Attribute("email").Value;
                        else if (Config.MailProvider.Enabled)
                            throw new InvalidDataException("email must be provided !");

                        if (row.Attribute("mobile") != null)
                            reg.PhoneNumber = row.Attribute("mobile").Value;
                        else if (Config.ExternalProvider.Enabled)
                            throw new InvalidDataException("mobile must be provided !");

                        if (row.Attribute("method") != null)
                            reg.PreferredMethod = (PreferredMethod)Enum.Parse(typeof(PreferredMethod), row.Attribute("method").Value);
                        else
                            reg.PreferredMethod = PreferredMethod.Choose;

                        if (DisableAll)
                            reg.Enabled = false;
                        else if (row.Attribute("enabled") != null)
                            reg.Enabled = bool.Parse(row.Attribute("enabled").Value);
                        else
                            reg.Enabled = true;

                        RuntimeRepository.AddUserRegistration(Config, reg, ForceNewKey, true, SendEmail);
                        Trace.TraceInformation("Record N° {0} imported for user : {1} !", (RecordsCount+1).ToString(), reg.UPN);
                    }
                    catch (Exception ex)
                    {
                        ErrorsCount++;
                        Trace.TraceError("Error importing Record N° {0} \r\r {1}", (RecordsCount + 1).ToString(), ex.Message);
                    }
                    finally
                    {
                        RecordsCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                return false;
            } 
            finally
            {
                Trace.Unindent();
                FinalizeTrace(listen);
            }
            return true;
        }
    }
    #endregion
}
