//******************************************************************************************************************************************************************************************//
// Copyright (c) 2008-2015 PhoneFactor, Inc.                                                                                                                                                //
//                                                                                                                                                                                          //
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),                                       //
// to deal in the Software without restriction, including without limitation the  rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,  // 
// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:                                                                                   //
//                                                                                                                                                                                          //
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.                                                           //
//                                                                                                                                                                                          //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,                                      //
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT  SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,                           //
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER  DEALINGS IN THE SOFTWARE.                              //
//                                                                                                                                                                                          //
//                                                                                                                                                                                          //
// pf_auth.cs: An SDK for authenticating with PhoneFactor for .NET 2.0.                                                                                                                     //
// version: 2.20                                                                                                                                                                            //
//******************************************************************************************************************************************************************************************//
// Copyright (c) 2016 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
using System.Xml;
using System.Net;
using System.Text;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections;
using System.Threading;
using Neos.IdentityServer.MultiFactor;
using System.Collections.Generic;
using System.Linq;

namespace Neos.IdentityServer.Multifactor.SMS
{
    public class PhoneFactorParams
    {
        public string Username = "";
        public string CountryCode = "1";
        public string Phone = "";
        public string Extension = "";
        public bool AllowInternationalCalls = false;
        public string Mode = PhoneFactor.MODE_SMS_ONE_WAY_OTP;
        public string Pin = "";
        public string Sha1PinHash = "";
        public string Sha1Salt = "";
        public string Language = "en";
        public string SmsText = "<$otp$>";
        public string ApplicationName = "";
        public string DeviceToken = "";
        public string NotificationType = "";
        public string AccountName = "";
        public string Hostname = "adfsmfa-hostname";
        public string IpAddress = "255.255.255.255";
        //  public string CertFilePath = "c:\\cert_key.p12";
        public string RequestId = "";
        public string ResponseUrl = "";
    }

    public static class PhoneFactor
    {
        public const string MODE_STANDARD = "standard";
        public const string MODE_SMS_TWO_WAY_OTP = "sms_two_way_otp";
        public const string MODE_SMS_TWO_WAY_OTP_PLUS_PIN = "sms_two_way_otp_plus_pin";
        public const string MODE_SMS_ONE_WAY_OTP = "sms_one_way_otp";
        public const string MODE_SMS_ONE_WAY_OTP_PLUS_PIN = "sms_one_way_otp_plus_pin";

        private static string LICENSE_KEY = "YOUR_LICENSE_KEY";
        private static string GROUP_KEY = "YOUR_GROUP_KEY";
        private static string CERT_THUMBPRINT = "YOUR_CERTIFICATE_THUMPRINT";

        private static string mCurrentTarget = "https://pfd.phonefactor.net/pfd/pfd.pl";
        private static Queue mTargets = new Queue(new object[] { "https://pfd2.phonefactor.net/pfd/pfd.pl" });
        private static Mutex mMutex = new Mutex();

        /// <summary>
        /// Initialize method implementation
        /// Loads your Phone-Factor attributes for authentication
        /// </summary>
        public static void Initialize(ExternalOTPProvider sms)
        {
            try
            {
                string data = sms.Parmeters.Value;
                Dictionary<string, string> Values = data.Split(',').Select(value => value.Split('=')).ToDictionary(s => s[0].Trim(), s => s[1].Trim());
                LICENSE_KEY = Values["LICENSE_KEY"];
                GROUP_KEY = Values["GROUP_KEY"];
                CERT_THUMBPRINT = Values["CERT_THUMBPRINT"];
                return ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Authenticate method implmentation
        /// </summary>
        public static bool Authenticate(PhoneFactorParams pfAuthParams, out string otp, out int callStatus, out int errorId)
        {
            return InternalAuthenticate(pfAuthParams, false, out otp, out callStatus, out errorId);
        }

        /// <summary>
        /// Authenticate method implementation overload
        /// </summary>
        public static bool Authenticate(PhoneFactorParams pfAuthParams, out int callStatus, out int errorId)
        {
            return InternalAuthenticate(pfAuthParams, false, out callStatus, out errorId);
        }


        /// <summary>
        /// InternalAuthenticate method implementation
        /// </summary>
        private static bool InternalAuthenticate(PhoneFactorParams pfAuthParams, bool asynchronous, out int call_status, out int error_id)
        {
            string otp = "";
            return InternalAuthenticate(pfAuthParams, asynchronous, out otp, out call_status, out error_id);
        }

        /// <summary>
        /// InternalAuthenticate method implementation 
        /// </summary>
        private static bool InternalAuthenticate(PhoneFactorParams pfAuthParams, bool asynchronous, out string otp, out int call_status, out int error_id)
        {
            if (pfAuthParams.CountryCode.Length == 0) 
                pfAuthParams.CountryCode = "1";
            if (pfAuthParams.Hostname.Length == 0) 
                pfAuthParams.Hostname = "adfsmfa-hostname";
            if (pfAuthParams.IpAddress.Length == 0) 
                pfAuthParams.IpAddress = "255.255.255.255";
            pfAuthParams.Mode = pfAuthParams.Mode.ToLower();
            if (pfAuthParams.Mode != MODE_SMS_TWO_WAY_OTP && pfAuthParams.Mode != MODE_SMS_TWO_WAY_OTP_PLUS_PIN  && pfAuthParams.Mode != MODE_SMS_ONE_WAY_OTP && pfAuthParams.Mode != MODE_SMS_ONE_WAY_OTP_PLUS_PIN)
                pfAuthParams.Mode = MODE_SMS_ONE_WAY_OTP;

            otp = "";
            call_status = 0;
            error_id = 0;

            string auth_message = CreateMessage(pfAuthParams, asynchronous);

            int tries = 1;
            if (mMutex.WaitOne())
            {
                try 
                { 
                    tries = mTargets.Count + 1; 
                }
                finally 
                { 
                    mMutex.ReleaseMutex(); 
                }
            }
            for (int i = 0; i < tries; i++)
            {
                string response;
                if (SendMessage(mCurrentTarget, auth_message, out response))
                {
                    string request_id_out = "";
                    bool authenticated = ReceiveResponse(response, out request_id_out, out otp, out call_status, out error_id);
                    return authenticated;
                }
                else
                {
                    if (mMutex.WaitOne())
                    {
                        try
                        {
                            mTargets.Enqueue(mCurrentTarget);
                            mCurrentTarget = mTargets.Dequeue().ToString();
                        }
                        finally 
                        { 
                            mMutex.ReleaseMutex(); 
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// CreateMessage method implmentation
        /// </summary>
        private static string CreateMessage(PhoneFactorParams pfAuthParams, bool asynchronous)
        {
            bool sms     = pfAuthParams.Mode == MODE_SMS_TWO_WAY_OTP || pfAuthParams.Mode == MODE_SMS_TWO_WAY_OTP_PLUS_PIN || pfAuthParams.Mode == MODE_SMS_ONE_WAY_OTP || pfAuthParams.Mode == MODE_SMS_ONE_WAY_OTP_PLUS_PIN;
            bool two_way = pfAuthParams.Mode == MODE_SMS_TWO_WAY_OTP || pfAuthParams.Mode == MODE_SMS_TWO_WAY_OTP_PLUS_PIN;
            bool otp     = pfAuthParams.Mode == MODE_SMS_TWO_WAY_OTP || pfAuthParams.Mode == MODE_SMS_ONE_WAY_OTP;

            XmlDocument doc = new XmlDocument();

            // start message
            // <pfpMessage></pfpMessage>
            XmlElement root = doc.CreateElement("pfpMessage");
            root.SetAttribute("version", "1.5");
            doc.AppendChild(root);

            // message header
            // <header></header>
            XmlElement header = doc.CreateElement("header");
            root.AppendChild(header);
            XmlElement source = doc.CreateElement("source");
            header.AppendChild(source);
            XmlElement component = doc.CreateElement("component");
            component.SetAttribute("type", "pfsdk");
            source.AppendChild(component);
            XmlElement element = doc.CreateElement("host");
            element.SetAttribute("ip", pfAuthParams.IpAddress);
            element.SetAttribute("hostname", pfAuthParams.Hostname);
            component.AppendChild(element);

            // request
            // <request></request>
            XmlElement request = doc.CreateElement("request");
            Random random = new Random();
            if (pfAuthParams.RequestId == null || pfAuthParams.RequestId.Length == 0) pfAuthParams.RequestId = random.Next(10000).ToString();
            request.SetAttribute("request-id", pfAuthParams.RequestId);
            request.SetAttribute("language", pfAuthParams.Language);
            request.SetAttribute("async", asynchronous ? "1" : "0");
            request.SetAttribute("response-url", pfAuthParams.ResponseUrl);
            root.AppendChild(request);
            XmlElement auth_request = doc.CreateElement("authenticationRequest");
            if (sms) auth_request.SetAttribute("mode", "sms");

            request.AppendChild(auth_request);
            XmlElement customer = doc.CreateElement("customer");
            auth_request.AppendChild(customer);
            element = doc.CreateElement("licenseKey");
            element.InnerText = LICENSE_KEY;
            customer.AppendChild(element);
            element = doc.CreateElement("groupKey");
            element.InnerText = GROUP_KEY;
            customer.AppendChild(element);
            element = doc.CreateElement("countryCode");
            element.InnerText = pfAuthParams.CountryCode;
            auth_request.AppendChild(element);
            element = doc.CreateElement("authenticationType");
            element.InnerText = "pfsdk";
            auth_request.AppendChild(element);
            element = doc.CreateElement("username");
            element.InnerText = pfAuthParams.Username;
            auth_request.AppendChild(element);
            element = doc.CreateElement("phonenumber");
            element.SetAttribute("userCanChangePhone", "no");
            element.InnerText = pfAuthParams.Phone;
            if (pfAuthParams.Extension != null && pfAuthParams.Extension.Length != 0)
            {
                element.SetAttribute("extension", pfAuthParams.Extension);
            }
            auth_request.AppendChild(element);
            element = doc.CreateElement("allowInternationalCalls");
            element.InnerText = pfAuthParams.AllowInternationalCalls ? "yes" : "no";
            auth_request.AppendChild(element);
            element = doc.CreateElement("applicationName");
            element.InnerText = pfAuthParams.ApplicationName;
            auth_request.AppendChild(element);

            XmlElement pin_info = doc.CreateElement("pinInfo");
            XmlElement pin_element = doc.CreateElement("pin");
            if (pfAuthParams.Mode == MODE_SMS_TWO_WAY_OTP_PLUS_PIN)
            {
                string pinFormat;
                string pinFormatted;
                if (pfAuthParams.Sha1PinHash.Length == 0)
                {
                    pinFormat = "plainText";
                    pinFormatted = pfAuthParams.Pin;
                }
                else
                {
                    pinFormat = "sha1";
                    pinFormatted = pfAuthParams.Sha1PinHash;
                }

                pin_element.SetAttribute("pinFormat", pinFormat);
                if (pfAuthParams.Sha1PinHash.Length != 0)
                {
                    pin_element.SetAttribute("sha1Salt", pfAuthParams.Sha1Salt);
                }
                pin_element.SetAttribute("pinChangeRequired", "no");
                pin_element.InnerText = pinFormatted;
            }

            if (sms)
            {
                XmlElement sms_info = doc.CreateElement("smsInfo");
                sms_info.SetAttribute("direction", two_way ? "two-way" : "one-way");
                sms_info.SetAttribute("mode", otp ? "otp" : "otp-pin");
                element = doc.CreateElement("message");
                element.InnerText = pfAuthParams.SmsText;
                sms_info.AppendChild(element);
                if (pfAuthParams.Mode == MODE_SMS_TWO_WAY_OTP_PLUS_PIN) 
                    sms_info.AppendChild(pin_element);
                auth_request.AppendChild(sms_info);
                pin_info.SetAttribute("pinMode", MODE_STANDARD);
            }
            auth_request.AppendChild(pin_info);

            return doc.InnerXml;
        }

        /// <summary>
        /// SendMessage method implmentation
        /// </summary>
        private static bool SendMessage(string target, string message, out string body)
        {
            body = "";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(target);
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";

            // Set certificate
           // X509Certificate2 cert = new X509Certificate2(cert_file_path, CERT_PASSWORD, X509KeyStorageFlags.MachineKeySet);

            X509Certificate2 cert = GetCertificate(CERT_THUMBPRINT, StoreLocation.LocalMachine);

            request.ClientCertificates.Add(cert);
            request.AuthenticationLevel = AuthenticationLevel.MutualAuthRequired;

            try
            {
                // Set Message
                byte[] postBytes = Encoding.UTF8.GetBytes(message);
                request.ContentType = "text/xml; charset=utf-8";
                request.ContentLength = postBytes.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(postBytes, 0, postBytes.Length);
                requestStream.Close();

                // Post message and read response
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(stream);
                body = streamReader.ReadToEnd();
                streamReader.Close();
                stream.Close();
                response.Close();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// ReceiveResponse method implmenbtation
        /// </summary>
        public static bool ReceiveResponse(string xml, out string request_id, out string otp, out int call_status, out int error_id)
        {
            request_id = "";
            otp = "";
            call_status = 0;
            error_id = 0;

            if (xml.Length == 0) return false;

            TextReader textReader = new StringReader(xml);
            XmlDocument doc = new XmlDocument();
            doc.Load(textReader);

            XmlNode response = doc.GetElementsByTagName("response")[0];
            request_id = response.Attributes["request-id"].Value;

            XmlNode status = doc.GetElementsByTagName("status")[0];
            if (status.Attributes["disposition"].Value == "fail")
            {
                error_id = Convert.ToInt32(status["error-id"].InnerText);
                return false;
            }

            XmlNode otpNode = doc.GetElementsByTagName("otp")[0];
            if (otpNode != null)
            {
                otp = otpNode.InnerText;
            }

            XmlNode auth_response = doc.GetElementsByTagName("authenticationResponse")[0];
            call_status = Convert.ToInt32(auth_response["callStatus"].InnerText);

            if (auth_response["authenticated"].InnerText == "no")
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// GetCertificate method implementation
        /// </summary>
        private static X509Certificate2 GetCertificate(string thumprint, StoreLocation location)
        {
            X509Certificate2 data = null;
            X509Store store = new X509Store(location);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            try
            {
                X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
                X509Certificate2Collection findCollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByThumbprint, thumprint, false);
                foreach (X509Certificate2 x509 in findCollection)
                {
                    data = x509;
                    break;
                }
            }
            catch
            {
                data = null;
            }
            finally
            {
                store.Close();
            }
            return data;
        }
    }
}