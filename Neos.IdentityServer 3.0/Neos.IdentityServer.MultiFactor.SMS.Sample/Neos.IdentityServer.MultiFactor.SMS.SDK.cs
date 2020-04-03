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
//                                                                                                                                                                                          //
// A very small sample using legacy interface to deal with an hardware appliance (8 sim cards : "S100 GATEWAY" see : www.iQSim.com)                                                         //
//                                                                                                                                                                                          //
// You also can develop a full Provider with IExternalProvide interface (right way), see other samples                                                                                      //
//                                                                                                                                                                                          //
// Not Deployed with msi solution (only in source code)                                                                                                                                     //
//                                                                                                                                                                                          //
//*******************************************************************************************************************************************************************************************//

using System;
using System.Xml;
using System.Net;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading;
using Neos.IdentityServer.MultiFactor;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Neos.IdentityServer.MultiFactor.SMS
{
    public class SMSParams
    {
        internal string Password = "";
        internal string IPhost = "";
        internal string SMSText = string.Empty;
        internal string PhoneNumber = string.Empty;
    }

    public static class SMSRuntime
    {
        private static string IPHOST_KEY = "IPHOST";
        private static string PWD_KEY = "PWD";

        private static string _target = "http://{0}/SMS?pwd={1}&number={2}&text={3}";


        internal static string IPhost;
        internal static string Password;
        internal static string SendTarget = string.Empty;

        /// <summary>
        /// Initialize method implementation
        /// Loads your SMS attributes 
        /// </summary>
        public static void Initialize(ExternalOTPProvider sms)
        {
            try
            {
                string data = sms.Parameters.Data;
                Dictionary<string, string> Values = data.Split(',').Select(value => value.Split('=')).ToDictionary(s => s[0].Trim(), s => s[1].Trim());
                try { IPhost = Values[IPHOST_KEY]; } catch { IPhost = "sms.yourdomain.com:8000"; }
                try { Password = Values[PWD_KEY]; } catch { Password = "yourpassword"; }
                return ;
            }
            catch (Exception ex)
            {
                Log.WriteEntry("SMS Initialize : \r" + ex.Message, EventLogEntryType.Error, 10000);
                throw ex;
            }
        }

        /// <summary>
        /// Authenticate method implmentation
        /// </summary>
        public static bool Authenticate(SMSParams smsParams,out int errorId, int timeout = 300)
        {
            return InternalAuthenticate(smsParams, out errorId, timeout);
        }

        /// <summary>
        /// InternalAuthenticate method implementation 
        /// </summary>
        private static bool InternalAuthenticate(SMSParams smsParams, out int error_id, int timeout = 300)
        {
            error_id = 0;

            string smsid;
            if (SendMessage(smsParams, out smsid, out error_id))
                return true;
            else
                return false;
        }

        /// <summary>
        /// CreateMessage method implmentation
        /// </summary>
        private static string CreateMessage(SMSParams smsParams)
        {
            return string.Format(_target, smsParams.IPhost, smsParams.Password, smsParams.PhoneNumber, smsParams.SMSText);
        }

        /// <summary>
        /// SendMessage method implmentation
        /// </summary>
        private static bool SendMessage(SMSParams smsParams, out string smsid, out int error_id)
        {
            smsid = "";
            error_id = 0;

            try
            {
                string url = CreateMessage(smsParams);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                HttpWebResponse response = null;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    Stream dataStream = response.GetResponseStream();                   
                    XmlDocument doc = new XmlDocument();
                    doc.Load(dataStream);
                    XmlNode root = doc.SelectSingleNode("/response");
                    error_id = Convert.ToInt32(root["error_code"].InnerText);
                    smsid = root["smsid"].InnerText;
                }
                finally
                {                   
                    response.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteEntry("SMS SendMessage : \r" + ex.Message, EventLogEntryType.Error, 10001);
                return false;
            }
        }
    }
}