//******************************************************************************************************************************************************************************************//
// Copyright (c) 2017 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor.Administration
{
    public class SQLAdminService : IAdministrationService
    {
        MFAConfig _config = null;
        string _connectionstring;
        int _deliverywindow = 300;

        /// <summary>
        /// AdminService constructor
        /// </summary>
        public SQLAdminService(MFAConfig config)
        {
            _config = config;
            _connectionstring = _config.Hosts.SQLServerHost.ConnectionString;
            _deliverywindow = _config.DeliveryWindow;
        }

        /// <summary>
        /// GetUserRegistration method implementation
        /// </summary>
        public MMCRegistration GetUserRegistration(string upn)
        {
            string request = "SELECT ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS WHERE UPN=@UPN";
            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(prm);
            prm.Value = upn;

            MMCRegistration reg = new MMCRegistration();
            con.Open();
            try
            {
                SqlDataReader rd = sql.ExecuteReader();
                if (rd.Read())
                {
                    reg.ID = rd.GetInt64(0).ToString();
                    reg.UPN = rd.GetString(1);
                    if (!rd.IsDBNull(2))
                        reg.MailAddress = rd.GetString(2);
                    if (!rd.IsDBNull(3))
                        reg.PhoneNumber = rd.GetString(3);
                    reg.Enabled = rd.GetBoolean(4);
                    reg.CreationDate = rd.GetDateTime(5);
                    reg.PreferredMethod = (RegistrationPreferredMethod)rd.GetInt32(6);
                    reg.IsRegistered = true;
                    return reg;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        /// <summary>
        /// SetUserRegistration method implementation
        /// </summary>
        public void SetUserRegistration(MMCRegistration reg)
        {
            if (!HasRegistration(reg.UPN))
                throw new Exception("The user "+reg.UPN+ " cannot be updated ! \r User not found !");

            string request = "UPDATE REGISTRATIONS SET MAILADDRESS = @MAILADDRESS, PHONENUMBER = @PHONENUMBER, ENABLED= @ENABLED, METHOD=@METHOD WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm2 = new SqlParameter("@MAILADDRESS", SqlDbType.VarChar);
            sql.Parameters.Add(prm2);
            if (string.IsNullOrEmpty(reg.MailAddress))
                prm2.Value = DBNull.Value;
            else
                prm2.Value = reg.MailAddress;

            SqlParameter prm3 = new SqlParameter("@PHONENUMBER", SqlDbType.VarChar);
            sql.Parameters.Add(prm3);
            if (string.IsNullOrEmpty(reg.PhoneNumber))
                prm3.Value = DBNull.Value;
            else
                prm3.Value = reg.PhoneNumber;

            SqlParameter prmb = new SqlParameter("@ENABLED", SqlDbType.Bit);
            sql.Parameters.Add(prmb);
            prmb.Value = reg.Enabled;

            SqlParameter prm4 = new SqlParameter("@METHOD", SqlDbType.Int);
            sql.Parameters.Add(prm4);
            prm4.Value = reg.PreferredMethod;

            SqlParameter prm5 = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(prm5);
            prm5.Value = reg.UPN;
            con.Open();
            try
            {
                int res = sql.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Log.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        /// <summary>
        /// AddUserRegistration method implementation
        /// </summary>
        public MMCRegistration AddUserRegistration(MMCRegistration reg)
        {
            if (HasRegistration(reg.UPN))
            {
                SetUserRegistration(reg);
                return GetUserRegistration(reg.UPN);
            }

            string request = "INSERT INTO REGISTRATIONS (UPN, MAILADDRESS, PHONENUMBER, ENABLED, METHOD) VALUES (@UPN, @MAILADDRESS, @PHONENUMBER, @ENABLED, @METHOD)";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm2 = new SqlParameter("@MAILADDRESS", SqlDbType.VarChar);
            sql.Parameters.Add(prm2);
            if (string.IsNullOrEmpty(reg.MailAddress))
                prm2.Value = DBNull.Value;
            else
                prm2.Value = reg.MailAddress;

            SqlParameter prm3 = new SqlParameter("@PHONENUMBER", SqlDbType.VarChar);
            sql.Parameters.Add(prm3);
            if (string.IsNullOrEmpty(reg.PhoneNumber))
                prm3.Value = DBNull.Value;
            else
                prm3.Value = reg.PhoneNumber;

            SqlParameter prmb = new SqlParameter("@ENABLED", SqlDbType.Bit);
            sql.Parameters.Add(prmb);
            prmb.Value = reg.Enabled;

            SqlParameter prm4 = new SqlParameter("@METHOD", SqlDbType.Int);
            sql.Parameters.Add(prm4);
            prm4.Value = reg.PreferredMethod;

            SqlParameter prm5 = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(prm5);
            prm5.Value = reg.UPN;
            con.Open();
            try
            {
                int res = sql.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Log.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            finally
            {
                con.Close();
            }
            return GetUserRegistration(reg.UPN);
        }

        /// <summary>
        /// DeleteUserRegistration method implementation
        /// </summary>
        public bool DeleteUserRegistration(MMCRegistration reg)
        {
            if (!HasRegistration(reg.UPN))
                throw new Exception("The user " + reg.UPN + " cannot be deleted ! \r User not found !");

            string request = "DELETE FROM REGISTRATIONS WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(prm);
            prm.Value = reg.UPN;
            con.Open();
            try
            {
                int res = sql.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Log.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            finally
            {
                con.Close();
            }

            string request2 = "DELETE FROM NOTIFICATIONS WHERE REGISTRATIONID=@REGISTRATIONID";
            SqlCommand sql2 = new SqlCommand(request2, con);

            SqlParameter prm2 = new SqlParameter("@REGISTRATIONID", SqlDbType.BigInt);
            sql2.Parameters.Add(prm2);
            prm2.Value = reg.ID;
            con.Open();
            try
            {
                int res = sql2.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Log.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            finally
            {
                con.Close();
            }

            return true;
        }

        /// <summary>
        /// EnableUserRegistration method implementation
        /// </summary>
        public MMCRegistration EnableUserRegistration(MMCRegistration reg)
        {
            if (!HasRegistration(reg.UPN))
                throw new Exception("The user " + reg.UPN + " cannot be updated ! \r User not found !");

            string request = "UPDATE REGISTRATIONS SET ENABLED=1 WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm5 = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(prm5);
            prm5.Value = reg.UPN;
            con.Open();
            try
            {
                int res = sql.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Log.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            finally
            {
                con.Close();
            }
            reg.Enabled = true;
            return reg;
        }

        /// <summary>
        /// DisableUserRegistration method implementation
        /// </summary>
        public MMCRegistration DisableUserRegistration(MMCRegistration reg)
        {
            if (!HasRegistration(reg.UPN))
                throw new Exception("The user " + reg.UPN + " cannot be updated ! \r User not found !");

            string request = "UPDATE REGISTRATIONS SET ENABLED=0 WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm5 = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(prm5);
            prm5.Value = reg.UPN;
            con.Open();
            try
            {
                int res = sql.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Log.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            finally
            {
                con.Close();
            }
            reg.Enabled = false;
            return reg;
        }

        /// <summary>
        /// GetUserRegistration method implementation
        /// </summary>
        public MMCRegistrationList GetUserRegistrations(UsersFilterObject filter, UsersOrderObject order, UsersPagingObject paging, int maxrows = 20000)
        {
            Dictionary<int, string> fliedlsvalues = new Dictionary<int, string> 
            {
                {0, " UPN "} ,
                {1, " MAILADDRESS "},
                {2, " PHONENUMBER "}
            };

            Dictionary<int, string> operatorsvalues = new Dictionary<int, string> 
            { 
                {0, " = @FILTERVALUE "},
                {1, " LIKE @FILTERVALUE +'%' "},
                {2, " LIKE '%'+ @FILTERVALUE +'%' "},
                {3, " <> @FILTERVALUE "},
                {4, " LIKE '%'+ @FILTERVALUE "},
                {5, " NOT LIKE '%' + @FILTERVALUE +'%' "}
            };

            Dictionary<int, string> nulloperatorsvalues = new Dictionary<int, string> 
            { 
                {0, " IS NULL "},
                {1, " IS NULL "},
                {2, " IS NULL "},
                {3, " IS NOT NULL "},
                {4, " IS NOT NULL "},
                {5, " IS NOT NULL "}
            };

            Dictionary<int, string> methodvalues = new Dictionary<int, string> 
            { 
                {0, " METHOD = 0 "},
                {1, " METHOD = 1 "},
                {2, " METHOD = 2 "},
                {3, " METHOD = 3 "}
            };

            string request = string.Empty;
            switch (order.Column)
            {
                case UsersOrderField.UserName:
                    if (order.Direction == SortDirection.Ascending)
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY UPN) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS";
                    else
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY UPN DESC) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS";
                    break;
                case UsersOrderField.Email:
                    if (order.Direction == SortDirection.Ascending)
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY MAILADDRESS) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS";
                    else
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY MAILADDRESS DESC) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS";
                    break;
                case UsersOrderField.Phone:
                    if (order.Direction == SortDirection.Ascending)
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY PHONENUMBER) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS";
                    else
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY PHONENUMBER DESC) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS";
                    break;
                case UsersOrderField.CreationDate:
                    if (order.Direction == SortDirection.Ascending)
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY CREATIONDATE) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS";
                    else
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY CREATIONDATE DESC) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS";
                    break;
                default:
                    if (order.Direction == SortDirection.Ascending)
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY ID) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS";
                    else
                        request = "SELECT ROW_NUMBER() OVER(ORDER BY ID DESC) AS NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS";
                    break;
            }
            
            bool hasparameter = (string.Empty!=filter.FilterValue);
            bool hasmethod = false;
            if (filter.FilterisActive)
            {
                if (hasparameter)
                {
                    request += " WHERE";
                    string strfields = string.Empty;
                    string stroperator = string.Empty;
                    fliedlsvalues.TryGetValue((int)filter.FilterField, out strfields);
                    request += strfields;
                    if (filter.FilterValue!=null)
                        operatorsvalues.TryGetValue((int)filter.FilterOperator, out stroperator);
                    else
                        nulloperatorsvalues.TryGetValue((int)filter.FilterOperator, out stroperator);
                    request += stroperator;
                }
                if (filter.FilterMethod != UsersPreferredMethod.None)
                {
                    string strmethod = string.Empty;
                    methodvalues.TryGetValue((int)filter.FilterMethod, out strmethod);
                    if (hasparameter)
                        request += " AND "+strmethod;
                    else
                        request += " WHERE " + strmethod;
                    hasmethod = true;
                }
                if (filter.EnabledOnly)
                {
                    if ((hasparameter) || (hasmethod))
                        request += " AND ENABLED=1";
                    else
                        request += " WHERE ENABLED=1";
                }
            }
            if (paging.isActive)
            {
                request = "SELECT TOP " + maxrows.ToString() + " NUMBER, ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM (" + request;

                request += ") AS TBL WHERE NUMBER BETWEEN " + ((paging.CurrentPage-1) * paging.PageSize + 1) + " AND  " + (paging.CurrentPage) * paging.PageSize;
            }

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);
            if ((hasparameter) && (filter.FilterValue!=null))
            {
                SqlParameter prm = new SqlParameter("@FILTERVALUE", SqlDbType.VarChar);
                sql.Parameters.Add(prm);
                prm.Value = filter.FilterValue;
            }

            MMCRegistrationList regs = new MMCRegistrationList();
            con.Open();
            try
            {
                int i = 0;
                SqlDataReader rd = sql.ExecuteReader();
                while (rd.Read())
                {
                    MMCRegistration reg = new MMCRegistration();
                    reg.ID = rd.GetInt64(1).ToString();

                    reg.UPN = rd.GetString(2);
                    if (!rd.IsDBNull(3))
                        reg.MailAddress = rd.GetString(3);
                    if (!rd.IsDBNull(4))
                        reg.PhoneNumber = rd.GetString(4);
                    reg.Enabled = rd.GetBoolean(5);
                    reg.CreationDate = rd.GetDateTime(6);
                    reg.PreferredMethod = (RegistrationPreferredMethod)rd.GetInt32(7);
                    reg.IsRegistered = true;
                    regs.Add(reg);
                    i++;
                }
                return regs;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        /// <summary>
        /// GetAllUserRegistration method implementation
        /// </summary>
        public MMCRegistrationList GetAllUserRegistrations(UsersOrderObject order, int maxrows = 20000, bool enabledonly = false)
        {
            string request = string.Empty;
            if (enabledonly)
                request = "SELECT TOP " + maxrows.ToString() + " ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS WHERE ENABLED=1";
            else
                request = "SELECT TOP " + maxrows.ToString() + " ID, UPN, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS";

            switch (order.Column)
            {
                case UsersOrderField.UserName:
                    request += " ORDER BY UPN";
                    break;
                case UsersOrderField.Email:
                    request += " ORDER BY MAILADDRESS";
                    break;
                case UsersOrderField.Phone:
                    request += " ORDER BY PHONENUMBER";
                    break;
                case UsersOrderField.CreationDate:
                    request += " ORDER BY ID";
                    break;
                default:
                    request += " ORDER BY CREATIONDATE";
                    break;
            }
            if (order.Direction == SortDirection.Descending)
                request += " DESC";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            MMCRegistrationList regs = new MMCRegistrationList();
            con.Open();
            try
            {
                SqlDataReader rd = sql.ExecuteReader();
                while (rd.Read())
                {
                    MMCRegistration reg = new MMCRegistration();
                    reg.ID = rd.GetInt64(0).ToString();
                    reg.UPN = rd.GetString(1);
                    if (!rd.IsDBNull(2))
                        reg.MailAddress = rd.GetString(2);
                    if (!rd.IsDBNull(3))
                        reg.PhoneNumber = rd.GetString(3);
                    reg.Enabled = rd.GetBoolean(4);
                    reg.CreationDate = rd.GetDateTime(5);
                    reg.PreferredMethod = (RegistrationPreferredMethod)rd.GetInt32(6);
                    reg.IsRegistered = true;
                    regs.Add(reg);
                }
                return regs;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        /// <summary>
        /// GetUserRegistrationsCount method implmentation
        /// </summary>
        public int GetUserRegistrationsCount(UsersFilterObject filter)
        {
            Dictionary<int, string> fliedlsvalues = new Dictionary<int, string> 
            {
                {0, " UPN "} ,
                {1, " MAILADDRESS "},
                {2, " PHONENUMBER "}
            };

            Dictionary<int, string> operatorsvalues = new Dictionary<int, string> 
            { 
                {0, " = @FILTERVALUE "},
                {1, " LIKE @FILTERVALUE +'%' "},
                {2, " LIKE '%'+ @FILTERVALUE +'%' "},
                {3, " <> @FILTERVALUE "},
                {4, " LIKE '%'+ @FILTERVALUE "},
                {5, " NOT LIKE '%' + @FILTERVALUE +'%' "}
            };

            Dictionary<int, string> nulloperatorsvalues = new Dictionary<int, string> 
            { 
                {0, " IS NULL "},
                {1, " IS NULL "},
                {2, " IS NULL "},
                {3, " IS NOT NULL "},
                {4, " IS NOT NULL "},
                {5, " IS NOT NULL "}
            };

            Dictionary<int, string> methodvalues = new Dictionary<int, string> 
            { 
                {0, " METHOD = 0 "},
                {1, " METHOD = 1 "},
                {2, " METHOD = 2 "},
                {3, " METHOD = 3 "}
            };

            string request = "SELECT COUNT(ID) FROM REGISTRATIONS";

            bool hasparameter = (string.Empty != filter.FilterValue);
            bool hasmethod = false;
            if (filter.FilterisActive)
            {
                if (hasparameter)
                {
                    request += " WHERE";
                    string strfields = string.Empty;
                    string stroperator = string.Empty;
                    fliedlsvalues.TryGetValue((int)filter.FilterField, out strfields);
                    request += strfields;
                    if (filter.FilterValue != null)
                        operatorsvalues.TryGetValue((int)filter.FilterOperator, out stroperator);
                    else
                        nulloperatorsvalues.TryGetValue((int)filter.FilterOperator, out stroperator);
                    request += stroperator;
                }
                if (filter.FilterMethod != UsersPreferredMethod.None)
                {
                    string strmethod = string.Empty;
                    methodvalues.TryGetValue((int)filter.FilterMethod, out strmethod);
                    if (hasparameter)
                        request += " AND " + strmethod;
                    else
                        request += " WHERE " + strmethod;
                    hasmethod = true;
                }
                if (filter.EnabledOnly)
                {
                    if ((hasparameter) || (hasmethod))
                        request += " AND ENABLED=1";
                    else
                        request += " WHERE ENABLED=1";
                }
            }

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);
            if ((hasparameter) && (filter.FilterValue != null))
            {
                SqlParameter prm = new SqlParameter("@FILTERVALUE", SqlDbType.VarChar);
                sql.Parameters.Add(prm);
                prm.Value = filter.FilterValue;
            }

            con.Open();
            try
            {
                return (int)sql.ExecuteScalar();
            }
            catch (Exception ex)
            {
                Log.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        /// <summary>
        /// HasRegistration method implementation
        /// </summary>
        private bool HasRegistration(string upn)
        {
            string request = "SELECT ID, UPN FROM REGISTRATIONS WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(prm);
            prm.Value = upn;
            con.Open();
            try
            {
                SqlDataReader rd = sql.ExecuteReader();
                return rd.Read();
            }
            catch (Exception ex)
            {
                Log.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }
    }
}
