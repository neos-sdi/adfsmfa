//******************************************************************************************************************************************************************************************//
// Copyright (c) 2015 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
//******************************************************************************************************************************************************************************************//
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Neos.IdentityServer.MultiFactor
{
    public class AdminService : IAdminService
    {
        string _connectionstring;
        int _deliverywindow = 300;

        /// <summary>
        /// AdminService constructor
        /// </summary>
        public AdminService(SQLServerHost host, int deliverywindow)
        {
            _connectionstring = host.ConnectionString;
            _deliverywindow = deliverywindow;
        }

        #region IAdminService
        /// <summary>
        /// GetUserRegistration method implementation
        /// </summary>
        public Registration GetUserRegistration(string upn)
        {
            string request = "SELECT ID, UPN, SECRETKEY, MAILADDRESS, PHONENUMBER, ENABLED, CREATIONDATE, METHOD FROM REGISTRATIONS WHERE UPN=@UPN";
            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm = new SqlParameter("@UPN", SqlDbType.VarChar);
            sql.Parameters.Add(prm);
            prm.Value = upn;

            Registration reg = new Registration();
            con.Open();
            try
            {
                SqlDataReader rd = sql.ExecuteReader();
                if (rd.Read())
                {
                    reg.ID = rd.GetInt64(0).ToString();
                    reg.UPN = rd.GetString(1);
                    if (!rd.IsDBNull(2))
                        reg.SecretKey = rd.GetString(2);
                    if (!rd.IsDBNull(3))
                        reg.MailAddress = rd.GetString(3);
                    if (!rd.IsDBNull(4))
                        reg.PhoneNumber = rd.GetString(4);
                    reg.Enabled = rd.GetBoolean(5);
                    reg.CreationDate = rd.GetDateTime(6);
                    reg.PreferredMethod = (RegistrationPreferredMethod)rd.GetInt32(7);
                    return reg;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        /// <summary>
        /// SetUserRegistration method implementation
        /// </summary>
        public void SetUserRegistration(Registration reg)
        {
            if (HasRegistration(reg.UPN))
                DoUpdateRegistration(reg);
            else
                DoInsertRegistration(reg);
        }

        /// <summary>
        /// SetNotification method implementation
        /// </summary>
        public Notification SetNotification(string registrationid, RegistrationPreferredMethod method)
        {
            Notification notif = new Notification();
            notif.RegistrationID = registrationid;
            if (method == RegistrationPreferredMethod.Email)
                notif.OTP = GetRandomOTP();
            else
                notif.OTP = -1;
            notif.CreationDate = DateTime.Now;
            notif.ValidityDate = notif.CreationDate.AddSeconds(_deliverywindow);
            notif.CheckDate = null;
            if (HasNotification(notif.RegistrationID))
                DoUpdateNotification(notif);
            else
                DoInsertNotification(notif);
            return notif;
        }

        /// <summary>
        /// CheckNotification method implementation
        /// </summary>
        public Notification CheckNotification(string registrationid)
        {
            Notification notif = new Notification();
            notif.RegistrationID = registrationid;
            notif.CheckDate = DateTime.Now;
            return DoCheckNotification(notif);
        }
        #endregion

        #region Private methods
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
                throw new FaultException(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        /// <summary>
        /// DoInsertRegistration method implementation
        /// </summary>
        private void DoInsertRegistration(Registration reg)
        {
            string request = "INSERT INTO REGISTRATIONS (UPN, SECRETKEY, MAILADDRESS, PHONENUMBER, METHOD) VALUES (@UPN, @SECRETKEY, @MAILADDRESS, @PHONENUMBER, @METHOD)";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm1 = new SqlParameter("@SECRETKEY", SqlDbType.VarChar);
            sql.Parameters.Add(prm1);
            prm1.Value = reg.SecretKey;

            SqlParameter prm2 = new SqlParameter("@MAILADDRESS", SqlDbType.VarChar);
            sql.Parameters.Add(prm2);
            prm2.Value = reg.MailAddress;

            SqlParameter prm3 = new SqlParameter("@PHONENUMBER", SqlDbType.VarChar);
            sql.Parameters.Add(prm3);
            prm3.Value = reg.PhoneNumber;

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
                throw new FaultException(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        /// <summary>
        /// DoUpdateRegistration method implementation
        /// </summary>
        private void DoUpdateRegistration(Registration reg)
        {
            string request = "UPDATE REGISTRATIONS SET SECRETKEY = @SECRETKEY, MAILADDRESS = @MAILADDRESS, PHONENUMBER = @PHONENUMBER, METHOD=@METHOD WHERE UPN=@UPN";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm1 = new SqlParameter("@SECRETKEY", SqlDbType.VarChar);
            sql.Parameters.Add(prm1);
            prm1.Value = reg.SecretKey;

            SqlParameter prm2 = new SqlParameter("@MAILADDRESS", SqlDbType.VarChar);
            sql.Parameters.Add(prm2);
            prm2.Value = reg.MailAddress;

            SqlParameter prm3 = new SqlParameter("@PHONENUMBER", SqlDbType.VarChar);
            sql.Parameters.Add(prm3);
            prm3.Value = reg.PhoneNumber;

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
                throw new FaultException(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        /// <summary>
        /// InsertNotification method implementation
        /// </summary>
        /// <param name="regid"></param>
        private bool DoInsertNotification(Notification notif)
        {
            string request = "INSERT INTO NOTIFICATIONS (REGISTRATIONID, OTP, CREATIONDATE, VALIDITYDATE, CHECKDATE) VALUES (@REGISTRATIONID, @OTP, @CREATEDATE, @VALIDITYDATE, NULL)";
            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prmid = new SqlParameter("@REGISTRATIONID", SqlDbType.BigInt);
            sql.Parameters.Add(prmid);
            prmid.Value = notif.RegistrationID;

            SqlParameter prmotp = new SqlParameter("@OTP", SqlDbType.Int);
            sql.Parameters.Add(prmotp);
            prmotp.Value = notif.OTP;

            SqlParameter prmdat = new SqlParameter("@CREATEDATE", SqlDbType.DateTime);
            sql.Parameters.Add(prmdat);
            prmdat.Value = notif.CreationDate;

            SqlParameter prmval = new SqlParameter("@VALIDITYDATE", SqlDbType.DateTime);
            sql.Parameters.Add(prmval);
            prmval.Value = notif.ValidityDate;

            con.Open();
            try
            {
                int res = sql.ExecuteNonQuery();
                return (res == 1);
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        /// <summary>
        /// UpdateNotification method implementation
        /// </summary>
        private bool DoUpdateNotification(Notification notif)
        {
            string request = "UPDATE NOTIFICATIONS  SET OTP=@OTP, CREATIONDATE = @CREATEDATE, VALIDITYDATE=@VALIDITYDATE, CHECKDATE = NULL WHERE REGISTRATIONID=@REGISTRATIONID";
            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prmid = new SqlParameter("@REGISTRATIONID", SqlDbType.BigInt);
            sql.Parameters.Add(prmid);
            prmid.Value = notif.RegistrationID;

            SqlParameter prmotp = new SqlParameter("@OTP", SqlDbType.Int);
            sql.Parameters.Add(prmotp);
            prmotp.Value = notif.OTP;

            SqlParameter prmdat = new SqlParameter("@CREATEDATE", SqlDbType.DateTime);
            sql.Parameters.Add(prmdat);
            prmdat.Value = notif.CreationDate;

            SqlParameter prmval = new SqlParameter("@VALIDITYDATE", SqlDbType.DateTime);
            sql.Parameters.Add(prmval);
            prmval.Value = notif.ValidityDate;

            con.Open();
            try
            {
                int res = sql.ExecuteNonQuery();
                return (res == 1);
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        /// <summary>
        /// DoCheckNotification method implementation
        /// </summary>
        private Notification DoCheckNotification(Notification notif)
        {
            string request = "UPDATE NOTIFICATIONS SET CHECKDATE = @CHECKDATE WHERE REGISTRATIONID=@REGISTRATIONID";
            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prmid = new SqlParameter("@REGISTRATIONID", SqlDbType.BigInt);
            sql.Parameters.Add(prmid);
            prmid.Value = notif.RegistrationID;

            SqlParameter prmdat = new SqlParameter("@CHECKDATE", SqlDbType.DateTime);
            sql.Parameters.Add(prmdat);
            prmdat.Value = notif.CheckDate;

            con.Open();
            try
            {
                int res = sql.ExecuteNonQuery();
                if (res == 1)
                {
                    string select = "SELECT ID, REGISTRATIONID, OTP, CREATIONDATE, VALIDITYDATE, CHECKDATE FROM NOTIFICATIONS WHERE REGISTRATIONID=@REGISTRATIONID";
                    SqlCommand ssql = new SqlCommand(select, con);

                    SqlParameter prm = new SqlParameter("@REGISTRATIONID", SqlDbType.BigInt);
                    ssql.Parameters.Add(prm);
                    prm.Value = notif.RegistrationID;
                    // if (con.State!= ConnectionState.Open) con.Open();
                    try
                    {
                        SqlDataReader rd = ssql.ExecuteReader();
                        if (rd.Read())
                        {
                            notif.ID = rd.GetInt64(0).ToString();
                            notif.RegistrationID = rd.GetInt64(1).ToString();
                            notif.OTP = rd.GetInt32(2);
                            notif.CreationDate = rd.GetDateTime(3);
                            notif.ValidityDate = rd.GetDateTime(4);
                            if (!rd.IsDBNull(5))
                                notif.CheckDate = rd.GetDateTime(5);
                            else
                                notif.CheckDate = null;
                            return notif;
                        }
                        else
                            return null;
                    }
                    catch (Exception ex)
                    {
                        throw new FaultException(ex.Message);
                    }
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        /// <summary>
        /// HasNotification method implementation
        /// </summary>
        private bool HasNotification(string regid)
        {
            string request = "SELECT REGISTRATIONID FROM LAST_NOTIFICATIONS WHERE REGISTRATIONID=@REGISTRATIONID";

            SqlConnection con = new SqlConnection(_connectionstring);
            SqlCommand sql = new SqlCommand(request, con);

            SqlParameter prm = new SqlParameter("@REGISTRATIONID", SqlDbType.BigInt);
            sql.Parameters.Add(prm);
            prm.Value = regid;
            con.Open();
            try
            {
                SqlDataReader rd = sql.ExecuteReader();
                return rd.Read();
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        /// <summary>
        /// GetRandomOTP  method implementation
        /// </summary>
        private int GetRandomOTP()
        {
            Random random = new Random();
            return random.Next(0, 1000000);
        }
        #endregion
    }
}
