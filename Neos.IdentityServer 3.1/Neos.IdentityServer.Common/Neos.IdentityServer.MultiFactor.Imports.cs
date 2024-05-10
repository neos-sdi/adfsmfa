//******************************************************************************************************************************************************************************************//
// Copyright (c) 2024 redhook (adfsmfa@gmail.com)                                                                                                                                        //                        
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
//                                                                                                                                                                                          //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using Neos.IdentityServer.MultiFactor.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;

namespace Neos.IdentityServer.MultiFactor.Administration
{
    public class ImportUsersBase
    {
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
        public MFAConfig Config { get; set; }

        /// <summary>
        /// ForceNewKey property
        /// </summary>
        public bool ForceNewKey { get; set; } = false;

        /// <summary>
        /// ForceNewKey property
        /// </summary>
        public bool SendEmail { get; set; } = false;

        /// <summary>
        /// DisableAll property
        /// </summary>
        public bool DisableAll { get; set; } = false;

        /// <summary>
        /// DisableAll property
        /// </summary>
        public bool NoLogFile { get; set; } = false;

        /// <summary>
        /// RecordsCount property
        /// </summary>
        public int RecordsCount { get; set; } = 0;

        /// <summary>
        /// ErrorsCount property
        /// </summary>
        public int ErrorsCount { get; set; } = 0;

        /// <summary>
        /// RecordsImported property
        /// </summary>
        public int RecordsImported
        {
            get { return RecordsCount; }
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
        /// ImportUsersADDS constructor
        /// </summary>
        public ImportUsersADDS(MFAConfig cfg) : base(cfg)
        {
            Parameters = new UsersADDSRecord();
        }

        /// <summary>
        /// Parameters Property
        /// </summary>
        public UsersADDSRecord Parameters;
        
        /// <summary>
        /// DoImport() method implmentation
        /// </summary>
        public override bool DoImport()
        {
            char sep = Path.DirectorySeparatorChar;
            string filename = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "adimport-" + DateTime.Now.ToFileTime().ToString() + ".log";
            TraceListener listen = InitializeTrace(filename);
            try
            {
                ADDSHost adht = Config.Hosts.ActiveDirectoryHost;
                if (string.IsNullOrEmpty(Parameters.DomainName))
                    Parameters.DomainName = adht.DomainName;
                if (string.IsNullOrEmpty(Parameters.UserName))
                    Parameters.UserName = adht.Account;
                if (string.IsNullOrEmpty(Parameters.Password))
                    Parameters.Password = adht.Password;

                DataRepositoryService client = null;
                switch (Config.StoreMode)
                {
                    case DataRepositoryKind.ADDS:
                        client = new ADDSDataRepositoryService(Config.Hosts.ActiveDirectoryHost, Config.DeliveryWindow);
                        break;
                    case DataRepositoryKind.SQL:
                        client = new SQLDataRepositoryService(Config.Hosts.SQLServerHost, Config.DeliveryWindow);
                        break;
                    case DataRepositoryKind.Custom:
                        client = CustomDataRepositoryActivator.CreateInstance(Config.Hosts.CustomStoreHost, Config.DeliveryWindow);
                        break;
                }

                Trace.WriteLine("");
                Trace.WriteLine(string.Format("Importing from AD : {0}", Parameters.LDAPPath));
                if (!string.IsNullOrEmpty(Parameters.LDAPFilter))
                    Trace.WriteLine(string.Format("Filtering from AD : {0}", Parameters.LDAPFilter));
                if (Parameters.NoRecurse)
                    Trace.WriteLine(string.Format("No Recursion on ADDS Groups : {0}", Parameters.NoRecurse));
                Trace.WriteLine("");
                Trace.WriteLine("");

                Trace.Indent();
                Trace.WriteLine("Query users from AD");
                MFAUserList lst = client.ImportMFAUsers(Parameters, DisableAll);
                Trace.WriteLine(string.Format("Query returns {0} user(s) from AD", lst.Count.ToString()));

                DataRepositoryService client2 = null;
                switch (Config.StoreMode)
                {
                    case DataRepositoryKind.ADDS:
                        Trace.WriteLine("");
                        Trace.WriteLine("Importing ADDS Mode");
                        Trace.Indent();
                        client2 = new ADDSDataRepositoryService(Config.Hosts.ActiveDirectoryHost, Config.DeliveryWindow);
                        break;
                    case DataRepositoryKind.SQL:
                        Trace.WriteLine("");
                        Trace.WriteLine("Importing SQL Mode");
                        Trace.Indent();
                        client2 = new SQLDataRepositoryService(Config.Hosts.SQLServerHost, Config.DeliveryWindow);
                        break;
                    case DataRepositoryKind.Custom:
                        
                        Trace.WriteLine("");
                        Trace.WriteLine("Importing Custom Store Mode");
                        Trace.Indent();
                        client2 = CustomDataRepositoryActivator.CreateInstance(Config.Hosts.CustomStoreHost, Config.DeliveryWindow);
                        break;
                }
                client2.OnKeyDataEvent += KeyDataEvent;
                foreach (MFAUser reg in lst)
                {
                    try
                    {
                        MFAUser ext = client2.GetMFAUser(reg.UPN);
                        if (ext == null)
                        {
                            reg.PIN = Convert.ToInt32(Config.DefaultPin);
                            reg.PreferredMethod = Config.DefaultProviderMethod;
                        }
                        else
                        {
                            if (this.Parameters.Method != PreferredMethod.None)
                                reg.PreferredMethod = this.Parameters.Method;
                            else
                                reg.PreferredMethod = ext.PreferredMethod;
                        }
                        if (reg.PIN <= 0)
                            reg.PIN = Convert.ToInt32(Config.DefaultPin);

                        if (!Utilities.ValidateEmail(reg.MailAddress, (Config.MailProvider.Enabled && Config.MailProvider.IsRequired)))
                            throw new Exception(string.Format("invalid mail address for user : {0}", reg.UPN));
                        if (!Utilities.ValidatePhoneNumber(reg.PhoneNumber, (Config.ExternalProvider.Enabled && Config.ExternalProvider.IsRequired)))
                            throw new Exception(string.Format("invalid phone number for user : {0}", reg.UPN));

                        client2.AddMFAUser(reg, ForceNewKey, true);
                        Trace.TraceInformation(string.Format("User {0} Imported in MFA", reg.UPN));
                        if (!string.IsNullOrEmpty(reg.MailAddress))
                        {
                            if (SendEmail)
                            {
                                string qrcode = KeysManager.EncodedKey(reg.UPN);
                                CultureInfo info = null;
                                try
                                {
                                    info = CultureInfo.CurrentUICulture;
                                }
                                catch
                                {
                                    info = new CultureInfo(Config.DefaultCountryCode);
                                }
                                MailUtilities.SendKeyByEmail(reg.MailAddress, reg.UPN, qrcode, Config.MailProvider, Config, info);
                                Trace.TraceInformation(string.Format("Sending Sensitive mail for User {0}", reg.UPN));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorsCount++;
                        Trace.TraceError("Error importing User {0} : {1}", reg.UPN, ex.Message);
                    }
                    finally
                    {
                        RecordsCount++;
                    }
                }
                Trace.Unindent();
                Trace.WriteLine("");
                Trace.WriteLine(string.Format("Imported {0} User(s) from AD Source", RecordsCount));
            }
            catch (Exception ex)
            {
                Trace.Unindent();
                Trace.TraceError(string.Format("Error importing from AD : {0}", ex.Message));
                Log.WriteEntry(string.Format("Error importing from AD : {0}", ex.Message), EventLogEntryType.Error, 20000);
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
        /// DoCleanUp() method implmentation
        /// </summary>
        public bool DoCleanUp()
        {
            char sep = Path.DirectorySeparatorChar;
            string filename = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "adcleanup-" + DateTime.Now.ToFileTime().ToString() + ".log";
            TraceListener listen = InitializeTrace(filename);
            try
            {
                ADDSHost adht = Config.Hosts.ActiveDirectoryHost;
                if (string.IsNullOrEmpty(Parameters.DomainName))
                    Parameters.DomainName = adht.DomainName;
                if (string.IsNullOrEmpty(Parameters.UserName))
                    Parameters.UserName = adht.Account;
                if (string.IsNullOrEmpty(Parameters.Password))
                    Parameters.Password = adht.Password;

                DataRepositoryService client = null;
                switch (Config.StoreMode)
                {
                    case DataRepositoryKind.ADDS:
                        client = new ADDSDataRepositoryService(Config.Hosts.ActiveDirectoryHost, Config.DeliveryWindow);
                        break;
                    case DataRepositoryKind.SQL:
                        client = new SQLDataRepositoryService(Config.Hosts.SQLServerHost, Config.DeliveryWindow);
                        break;
                    case DataRepositoryKind.Custom:
                        client = CustomDataRepositoryActivator.CreateInstance(Config.Hosts.CustomStoreHost, Config.DeliveryWindow);
                        break;
                }

                Trace.WriteLine("");
                Trace.WriteLine("Clean Up from AD");
                Trace.Indent();
                Trace.WriteLine("Query deleted users from AD");
                List<string> lst = client.CleanMFAUsers(Parameters);
                Trace.WriteLine(string.Format("Query returns {0} deleted user(s) from AD", lst.Count.ToString()));

                DataRepositoryService client2 = null;
                switch (Config.StoreMode)
                {
                    case DataRepositoryKind.ADDS:
                        Trace.WriteLine("");
                        Trace.WriteLine("Clean Up ADDS Mode");
                        Trace.Indent();
                        client2 = new ADDSDataRepositoryService(Config.Hosts.ActiveDirectoryHost, Config.DeliveryWindow);
                        break;
                    case DataRepositoryKind.SQL:
                        Trace.WriteLine("");
                        Trace.WriteLine("Clean Up SQL Mode");
                        Trace.Indent();
                        client2 = new SQLDataRepositoryService(Config.Hosts.SQLServerHost, Config.DeliveryWindow);
                        break;
                    case DataRepositoryKind.Custom:

                        Trace.WriteLine("");
                        Trace.WriteLine("Clean Up Custom Store Mode");
                        Trace.Indent();
                        client2 = CustomDataRepositoryActivator.CreateInstance(Config.Hosts.CustomStoreHost, Config.DeliveryWindow);
                        break;
                }
                client2.OnKeyDataEvent += KeyDataEvent;
                foreach (string del in lst)
                {
                    MFAUser reg = client2.GetMFAUser(del);
                    if (reg != null)
                    {
                        try
                        {
                            if (reg.UPN.ToLower().Equals(del.ToLower()))
                            {
                                client2.DeleteMFAUser(reg);
                                Trace.TraceInformation(string.Format("User {0} Removed from MFA", reg.UPN));
                                RecordsCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorsCount++;
                            Trace.TraceError("Error Cleaning User {0} from MFA : {1}", reg.UPN, ex.Message);
                        }
                    }
                }
                Trace.Unindent();
                Trace.WriteLine("");
                Trace.WriteLine(string.Format("Cleaned {0} Deleted MFA User(s) from AD Source", RecordsCount));
            }
            catch (Exception ex)
            {
                Trace.Unindent();
                Trace.TraceError(string.Format("Error Cleaning Up from AD : {0}", ex.Message));
                Log.WriteEntry(string.Format("Error Cleaning Up from AD : {0}", ex.Message), EventLogEntryType.Error, 20000);
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
                case KeysDataManagerEventKind.Add:
                    KeysManager.NewKey(user);
                    break;
                case KeysDataManagerEventKind.Get:
                    KeysManager.ReadKey(user);
                    break;
                case KeysDataManagerEventKind.Remove:
                    KeysManager.RemoveKey(user, true);
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
                        MFAUser reg = new MFAUser();
                        reg.PIN = Convert.ToInt32(Config.DefaultPin);

                        if ((ids[0].HasValue) && (!string.IsNullOrEmpty(row[ids[0].Value])))
                            reg.UPN = row[ids[0].Value];
                        else
                            throw new InvalidDataException("upn must be provided !");

                        if ((ids[1].HasValue) && (!string.IsNullOrEmpty(row[ids[1].Value])))
                            reg.MailAddress = row[ids[1].Value];

                        if ((ids[2].HasValue) && (!string.IsNullOrEmpty(row[ids[2].Value])))
                            reg.PhoneNumber = row[ids[2].Value];

                        if (!Utilities.ValidateEmail(reg.MailAddress, (Config.MailProvider.Enabled && Config.MailProvider.IsRequired)))
                            throw new Exception(string.Format("invalid mail address for user : {0}", reg.UPN));
                        if (!Utilities.ValidatePhoneNumber(reg.PhoneNumber, (Config.ExternalProvider.Enabled && Config.ExternalProvider.IsRequired)))
                            throw new Exception(string.Format("invalid phone number for user : {0}", reg.UPN));

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

                        RuntimeRepository.AddMFAUser(Config, reg, ForceNewKey, true, SendEmail); // Can also Update
                        Trace.TraceInformation("Record N° {0} imported for user : {1} !", (RecordsCount+1).ToString(), reg.UPN);
                    }
                    catch (Exception ex)
                    {
                        ErrorsCount++;
                        Trace.TraceError("Error importing Record N° {0} : {1}", (RecordsCount + 1).ToString(), ex.Message);
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
                        MFAUser reg = new MFAUser();
                        reg.PIN = Convert.ToInt32(Config.DefaultPin);
                        if (row.Attribute("upn") != null)
                            reg.UPN = row.Attribute("upn").Value;
                        else
                            throw new InvalidDataException("upn must be provided !");

                        if (row.Attribute("email") != null)
                            reg.MailAddress = row.Attribute("email").Value;

                        if (row.Attribute("mobile") != null)
                            reg.PhoneNumber = row.Attribute("mobile").Value;

                        if (!Utilities.ValidateEmail(reg.MailAddress, (Config.MailProvider.Enabled && Config.MailProvider.IsRequired)))
                            throw new Exception(string.Format("invalid mail address for user : {0}", reg.UPN));
                        if (!Utilities.ValidatePhoneNumber(reg.PhoneNumber, (Config.ExternalProvider.Enabled && Config.ExternalProvider.IsRequired)))
                            throw new Exception(string.Format("invalid phone number for user : {0}", reg.UPN));

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

                        RuntimeRepository.AddMFAUser(Config, reg, ForceNewKey, true, SendEmail);
                        Trace.TraceInformation("Record N° {0} imported for user : {1} !", (RecordsCount+1).ToString(), reg.UPN);
                    }
                    catch (Exception ex)
                    {
                        ErrorsCount++;
                        Trace.TraceError("Error importing Record N° {0} : {1}", (RecordsCount + 1).ToString(), ex.Message);
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
