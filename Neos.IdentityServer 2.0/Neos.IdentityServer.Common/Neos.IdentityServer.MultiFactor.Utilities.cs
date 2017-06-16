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
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Threading;
using System.Xml.Serialization;
using Neos.IdentityServer.MultiFactor;
using Neos.IdentityServer.MultiFactor.QrEncoding;
using Neos.IdentityServer.MultiFactor.QrEncoding.Windows.Render;
using Neos.IdentityServer.MultiFactor.Resources;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Neos.IdentityServer.MultiFactor
{
    /// <summary>
    /// RepositoryService class implementation
    /// </summary>
    public static class RepositoryService
    {
        private static MailSlotServer _mailslotserver;

        /// <summary>
        /// GetUserRegistration method implementation
        /// </summary>
        public static Registration GetUserRegistration(string upn, MFAConfig cfg)
        {
            Registration res = null;
            if (cfg.UseActiveDirectory)
            {
                ADDSAdminService client = new ADDSAdminService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                res = client.GetUserRegistration(upn);
            }
            else
            {
                SQLAdminService client = new SQLAdminService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                res = client.GetUserRegistration(upn);
            }
            return res;
        }

        /// <summary>
        /// SetUserRegistration method implementation
        /// </summary>
        public static void SetUserRegistration(Registration registration, MFAConfig cfg)
        {
            if (cfg.UseActiveDirectory)
            {
                ADDSAdminService client = new ADDSAdminService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                client.SetUserRegistration(registration);
            }
            else
            {
                SQLAdminService client = new SQLAdminService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                client.SetUserRegistration(registration);
            }
        }

        /// <summary>
        /// SetNotification method implmentation
        /// </summary>
        public static Notification SetNotification(Registration registration, MFAConfig config, int otp)
        {
            if (config.UseActiveDirectory)
            {
                ADDSAdminService client = new ADDSAdminService(config.Hosts.ActiveDirectoryHost, config.DeliveryWindow);
                return client.SetNotification(registration, config, otp);
            }
            else
            {
                SQLAdminService client = new SQLAdminService(config.Hosts.SQLServerHost, config.DeliveryWindow);
                return client.SetNotification(registration, config, otp);
            }
        }

        /// <summary>
        /// CheckNotification method implementation
        /// </summary>
        public static Notification CheckNotification(Registration registration, MFAConfig cfg)
        {
            if (cfg.UseActiveDirectory)
            {
                ADDSAdminService client = new ADDSAdminService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                return client.CheckNotification(registration.ID);
            }
            else
            {
                SQLAdminService client = new SQLAdminService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                return client.CheckNotification(registration.ID);
            }
        }

        /// <summary>
        /// ChangePassword method implmentation
        /// </summary>
        public static void ChangePassword(string username, string oldpassword, string newpassword)
        {
            using (var ctx = new PrincipalContext(ContextType.Domain))
            {
                using (var user = UserPrincipal.FindByIdentity(ctx, IdentityType.UserPrincipalName, username))
                {
                    user.ChangePassword(oldpassword, newpassword);
                }
            }
        }

        /// <summary>
        /// GetUserKey method implementation
        /// </summary>
        public static string GetUserKey(string upn, MFAConfig cfg)
        {
            if (cfg.UseActiveDirectory)
            {
                ADDSAdminService client = new ADDSAdminService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                return client.GetUserKey(upn);
            }
            else
            {
                SQLAdminService client = new SQLAdminService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                return client.GetUserKey(upn);
            }
        }

        /// <summary>
        /// NewUserKey method implementation
        /// </summary>
        public static string NewUserKey(string upn, string secretkey, MFAConfig cfg)
        {
            if (cfg.UseActiveDirectory)
            {
                ADDSAdminService client = new ADDSAdminService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                return client.NewUserKey(upn, secretkey);
            }
            else
            {
                SQLAdminService client = new SQLAdminService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                return client.NewUserKey(upn, secretkey);
            }
        }

        /// <summary>
        /// RemoveUserKey method implmentation
        /// </summary>
        public static bool RemoveUserKey(string upn, MFAConfig cfg)
        {
            if (cfg.UseActiveDirectory)
            {
                ADDSAdminService client = new ADDSAdminService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                return client.RemoveUserKey(upn);
            }
            else
            {
                SQLAdminService client = new SQLAdminService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                return client.RemoveUserKey(upn);
            }
        }

        /// <summary>
        /// MailslotServer property implementation
        /// </summary>
        public static MailSlotServer MailslotServer
        {
            get
            {
                if (_mailslotserver == null)
                {
                    _mailslotserver = new MailSlotServer("MFA");
                }
                return RepositoryService._mailslotserver;
            }
        }
    }       

    /// <summary>
    /// Log class
    /// </summary>
    public static class Log
    {
        private const string EventLogSource = "ADFS MFA Service";
        private const string EventLogGroup = "Application";

        /// <summary>
        /// Log constructor
        /// </summary>
        static Log()
        {
            if (!EventLog.SourceExists(Log.EventLogSource))
                EventLog.CreateEventSource(Log.EventLogSource, Log.EventLogGroup);
        }

        /// <summary>
        /// WriteEntry method implementation
        /// </summary>
        public static void WriteEntry(string message, EventLogEntryType type, int eventID)
        {
            EventLog.WriteEntry(EventLogSource, message, type, eventID);
        }
    }

    /// <summary>
    /// OTPGenerator static class
    /// </summary>
    public partial class OTPGenerator
    {
        private int _secondsToGo;
        private string _identity;
        private byte[] _secret;
        private Int64 _timestamp;
        private byte[] _hmac;
        private int _offset;
        private int _oneTimePassword;
        private DateTime _datetime;
        private HashMode _mode = HashMode.SHA1;

        public static int TOTPDuration = 30;

        /// <summary>
        /// Constructor
        /// </summary>
        public OTPGenerator(HashMode mode = HashMode.SHA1)
        {
            RequestedDatetime = DateTime.UtcNow;
            _mode = mode;
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += (s, e) => SecondsToGo = TOTPDuration - Convert.ToInt32(GetUnixTimestamp(RequestedDatetime) % TOTPDuration);
            timer.IsEnabled = true;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OTPGenerator(byte[] asecret, string aid, HashMode mode = HashMode.SHA1)
        {
            RequestedDatetime = DateTime.UtcNow;
            _mode = mode;
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += (s, e) => SecondsToGo = TOTPDuration - Convert.ToInt32(GetUnixTimestamp(RequestedDatetime) % TOTPDuration);
            timer.IsEnabled = true;

            Secret = asecret;
            Identity = aid;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OTPGenerator(string ssecret, string aid, HashMode mode = HashMode.SHA1)
        {
            RequestedDatetime = DateTime.UtcNow;
            byte[] asecret = Base32.GetBytesFromString(ssecret);
            _mode = mode;
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += (s, e) => SecondsToGo = TOTPDuration - Convert.ToInt32(GetUnixTimestamp(RequestedDatetime) % TOTPDuration);
            timer.IsEnabled = true;

            Secret = asecret;
            Identity = aid;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OTPGenerator(string ssecret, string aid, DateTime datetime, HashMode mode = HashMode.SHA1)
        {
            RequestedDatetime = datetime;
            byte[] asecret = Base32.GetBytesFromString(ssecret);
            _mode = mode;
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += (s, e) => SecondsToGo = TOTPDuration - Convert.ToInt32(GetUnixTimestamp(RequestedDatetime) % TOTPDuration);
            timer.IsEnabled = true;

            Secret = asecret;
            Identity = aid;
        }

        /// <summary>
        /// SecondsToGo property implmentation
        /// </summary>
        public int SecondsToGo
        {
            get { return _secondsToGo; }
            private set 
            { 
                _secondsToGo = value;
                if (SecondsToGo == TOTPDuration)
                    ComputeOTP(RequestedDatetime); 
            }
        }
        
        /// <summary>
        /// Identity property implementation
        /// </summary>
        public string Identity
        {
            get { return _identity; }
            set { _identity = value; }
        }
       
        /// <summary>
        /// Secret propertry implmentation
        /// </summary>
        private byte[] Secret
        {
            get { return _secret; }
            set { _secret = value; }
        }

        /// <summary>
        /// Timestamp property implmentation
        /// </summary>
        public Int64 Timestamp
        {
            get { return _timestamp; }
            private set { _timestamp = value; }
        }

        /// <summary>
        /// RequestedDatetime property implmentation
        /// </summary>
        public DateTime RequestedDatetime
        {
            get { return _datetime; }
            private set { _datetime = value; }
        }

        /// <summary>
        /// Hmac property implementation
        /// </summary>
        public byte[] Hmac
        {
            get { return _hmac; }
            private set { _hmac = value; }
        }

        /// <summary>
        /// HmacPart1 property implementation
        /// </summary>
        public byte[] HmacPart1
        {
            get { return _hmac.Take(Offset).ToArray(); }
        }

        /// <summary>
        /// HmacPart2 property implmentation 
        /// </summary>
        public byte[] HmacPart2
        {
            get { return _hmac.Skip(Offset).Take(4).ToArray(); }
        }

        /// <summary>
        /// HmacPart3 property implmentation
        /// </summary>
        public byte[] HmacPart3
        {
            get { return _hmac.Skip(Offset + 4).ToArray(); }
        }

        /// <summary>
        /// Offset property implmentation
        /// </summary>
        public int Offset
        {
            get { return _offset; }
            private set { _offset = value; }
        }

        /// <summary>
        /// OTP property implementation
        /// </summary>
        public int OTP
        {
            get { return _oneTimePassword; }
            set { _oneTimePassword = value; }
        }

        /// <summary>
        /// ComputeOneTimePassword method implmentation
        /// </summary>
        public void ComputeOTP(DateTime date)
        {
            // https://tools.ietf.org/html/rfc4226
            Timestamp = Convert.ToInt64(GetUnixTimestamp(date) / TOTPDuration);
            var data = BitConverter.GetBytes(Timestamp).Reverse().ToArray();
            switch (_mode)
            {
                case HashMode.SHA1:
                    Hmac = new HMACSHA1(Secret).ComputeHash(data);
                    break;
                case HashMode.SHA256:
                    Hmac = new HMACSHA256(Secret).ComputeHash(data);
                    break;
                case HashMode.SHA384:
                    Hmac = new HMACSHA384(Secret).ComputeHash(data);
                    break;
                case HashMode.SHA512:
                    Hmac = new HMACSHA512(Secret).ComputeHash(data);
                    break;
            }
            Offset = Hmac.Last() & 0x0F;
            OTP = (((Hmac[Offset + 0] & 0x7f) << 24) | ((Hmac[Offset + 1] & 0xff) << 16) | ((Hmac[Offset + 2] & 0xff) << 8) | (Hmac[Offset + 3] & 0xff)) % 1000000;
        }

        /// <summary>
        /// GetUnixTimestamp method implementation
        /// </summary>
        private static Int64 GetUnixTimestamp(DateTime date)
        {
            return Convert.ToInt64(Math.Round((date - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds));
        }
    }

    /// <summary>
    /// Base32 static class
    /// </summary>
    public static class Base32
    {
        private const int IN_BYTE_SIZE = 8;
        private const int OUT_BYTE_SIZE = 5;
        private static char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

        /// <summary>
        /// Encode method implmentation
        /// </summary>
        public static string Encode(byte[] data)
        {
            int i = 0, index = 0, digit = 0;
            int current_byte, next_byte;
            StringBuilder result = new StringBuilder((data.Length + 7) * IN_BYTE_SIZE / OUT_BYTE_SIZE);

            while (i < data.Length)
            {
                current_byte = (data[i] >= 0) ? data[i] : (data[i] + 256); // Unsign

                /* Is the current digit going to span a byte boundary? */
                if (index > (IN_BYTE_SIZE - OUT_BYTE_SIZE))
                {
                    if ((i + 1) < data.Length)
                        next_byte = (data[i + 1] >= 0) ? data[i + 1] : (data[i + 1] + 256);
                    else
                        next_byte = 0;

                    digit = current_byte & (0xFF >> index);
                    index = (index + OUT_BYTE_SIZE) % IN_BYTE_SIZE;
                    digit <<= index;
                    digit |= next_byte >> (IN_BYTE_SIZE - index);
                    i++;
                }
                else
                {
                    digit = (current_byte >> (IN_BYTE_SIZE - (index + OUT_BYTE_SIZE))) & 0x1F;
                    index = (index + OUT_BYTE_SIZE) % IN_BYTE_SIZE;
                    if (index == 0)
                        i++;
                }
                result.Append(alphabet[digit]);
            }
            return result.ToString();
        }

        /// <summary>
        /// Encode method overload
        /// </summary>
        public static string Encode(string data)
        {
            byte[] result = GetBytesFromString(data); 
            return Encode(result);
        }

        /// <summary>
        /// EncodeString method
        /// </summary>
        public static string EncodeString(string data)
        {
            byte[] bytes = new byte[data.Length * sizeof(char)];
            System.Buffer.BlockCopy(data.ToCharArray(), 0, bytes, 0, bytes.Length);
            return Encode(bytes);
        }

        /// <summary>
        /// GetBytesFromString method
        /// </summary>
        internal static byte[] GetBytesFromString(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>
        /// GetStringFromByteArray method
        /// </summary>
        internal static string GetStringFromByteArray(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            string str = new string(chars);
            return str; 
        }
    }

    /// <summary>
    /// KeyGenerator static class
    /// </summary>
    public static class KeysManager
    {
        private static ISecretKeyManager _manager;

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public static void Initialize(MFAConfig cfg)
        {
            switch (cfg.KeysConfig.KeyFormat)
            {
                case RegistrationSecretKeyFormat.RNG:
                    _manager = new RNGKeyManager();
                    _manager.Initialize(cfg);
                    break;
                case RegistrationSecretKeyFormat.RSA:
                    _manager = new RSAKeyManager();
                    _manager.Initialize(cfg);
                    break;
                case RegistrationSecretKeyFormat.CUSTOM:
                    _manager = Utilities.LoadExternalKeyManagerWrapper(cfg.KeysConfig.ExternalKeyManager.FullQualifiedImplementation);
                    if (_manager==null)
                        throw new NotImplementedException("CUSTOM SecretKeyManager not found !");
                    _manager.Initialize(cfg);
                    break;
                default:
                    throw new NotImplementedException("CUSTOM SecretKeyManager not found !");
            }
        }

        /// <summary>
        /// EnsureKey method iplementation
        /// </summary>
        public static void EnsureKey(string upn) 
        {
            string key = ReadKey(upn);
            if (string.IsNullOrEmpty(key))
            {
                NewKey(upn);
            }
        }

        /// <summary>
        /// NewKey method implmentation
        /// </summary>
        public static string NewKey(string upn)
        {
            return _manager.NewKey(upn);
        }

        /// <summary>
        /// ReadKey method implementation
        /// </summary>
        public static string ReadKey(string upn)
        {
            return _manager.ReadKey(upn);
        }

        /// <summary>
        /// EncodedKey method implementation
        /// </summary>
        public static string EncodedKey(string upn)
        {
            return _manager.EncodedKey(upn);
        }

        /// <summary>
        /// ProbeKey method implementation
        /// </summary>
        public static string ProbeKey(string upn)
        {
            return _manager.ProbeKey(upn);
        }

        /// <summary>
        /// RemoveKey method implementation
        /// </summary>
        public static bool RemoveKey(string upn)
        {
            return _manager.RemoveKey(upn);
        }

        /// <summary>
        /// CheckKey method implmentation
        /// </summary>
        public static bool ValidateKey(string upn)
        {
            return _manager.ValidateKey(upn);
        }

        /// <summary>
        /// StripKeyPrefix method implementation
        /// </summary>
        public static string StripKeyPrefix(string key)
        {
            return _manager.StripKeyPrefix(key);
        }

        /// <summary>
        /// AddKeyPrefix method implementation
        /// </summary>
        public static string AddKeyPrefix(string key)
        {
            return _manager.AddKeyPrefix(key);
        }

        /// <summary>
        /// HasKeyPrefix method implementation
        /// </summary>
        public static bool HasKeyPrefix(string key)
        {
            return _manager.HasKeyPrefix(key);
        }
    }

    /// <summary>
    /// RNGKeyManager class
    /// </summary>
    public class RNGKeyManager: ISecretKeyManager
    {
        private KeyGeneratorMode _mode = KeyGeneratorMode.ClientSecret512;
        private KeySizeMode _ksize = KeySizeMode.KeySize1024;
        private MFAConfig _config = null;
        private int MAX_PROBE_LEN = 128;

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public void Initialize(MFAConfig config)
        {
            _config = config;
            _mode = config.KeysConfig.KeyGenerator;
            _ksize = config.KeysConfig.KeySize;
        }

        /// <summary>
        /// Prefix property
        /// </summary>
        public string Prefix
        {
            get { return "rng://"; }
        }

        /// <summary>
        /// NewKey method implementation
        /// </summary>
        public string NewKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return null;
            string lupn = upn.ToLower();
            RandomNumberGenerator cryptoRandomDataGenerator = new RNGCryptoServiceProvider();
            byte[] buffer = null;
            string crypted = string.Empty;
            switch (_mode)
            {
                case KeyGeneratorMode.ClientSecret128:
                    buffer = new byte[16];
                    cryptoRandomDataGenerator.GetBytes(buffer);
                    crypted = AddKeyPrefix(Convert.ToBase64String(buffer));
                    break;
                case KeyGeneratorMode.ClientSecret256:
                    buffer = new byte[32];
                    cryptoRandomDataGenerator.GetBytes(buffer);
                    crypted = AddKeyPrefix(Convert.ToBase64String(buffer));
                    break;
                case KeyGeneratorMode.ClientSecret384:
                    buffer = new byte[48];
                    cryptoRandomDataGenerator.GetBytes(buffer);
                    crypted = AddKeyPrefix(Convert.ToBase64String(buffer));
                    break;
                case KeyGeneratorMode.ClientSecret512:
                    buffer = new byte[64];
                    cryptoRandomDataGenerator.GetBytes(buffer);
                    crypted = AddKeyPrefix(Convert.ToBase64String(buffer));
                    break;
                default:
                    buffer = Guid.NewGuid().ToByteArray();
                    cryptoRandomDataGenerator.GetBytes(buffer);
                    crypted = AddKeyPrefix(Convert.ToBase64String(buffer));
                    break;
            }
            RepositoryService.NewUserKey(lupn, crypted, _config);
            return crypted;
        }

        /// <summary>
        /// ValidateKey method implementation
        /// </summary>
        public bool ValidateKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return false;
            string lupn = upn.ToLower();
            string key = ReadKey(lupn);
            if (HasKeyPrefix(key))
                return true;
            else if (key.Equals(StripKeyPrefix(key)))
                return true; // Old RNG without prefix for compatibility
            else
                return false;
        }

        /// <summary>
        /// ReadKey method implementation
        /// </summary>
        public string ReadKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return null;
            string lupn = upn.ToLower();
            return RepositoryService.GetUserKey(lupn, _config);
        }

        /// <summary>
        /// EncodedKey method implementation
        /// </summary>
        public string EncodedKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return null;
            string lupn = upn.ToLower();
            string full = StripKeyPrefix(ReadKey(lupn));
            if (string.IsNullOrEmpty(full))
                return null;
            if (full.Length > MAX_PROBE_LEN)
                return Base32.Encode(full.Substring(0, MAX_PROBE_LEN));
            else
                return Base32.Encode(full);
        }

        /// <summary>
        /// ProbeKey method implmentation
        /// </summary>
        public string ProbeKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return null;
            string lupn = upn.ToLower();
            string full = StripKeyPrefix(ReadKey(lupn));
            if (string.IsNullOrEmpty(full))
                return null;
            if (full.Length > MAX_PROBE_LEN)
                return full.Substring(0, MAX_PROBE_LEN);
            else
                return full;
        }

        /// <summary>
        /// RemoveKey method implementation
        /// </summary>
        public bool RemoveKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return false;
            string lupn = upn.ToLower();
            return RepositoryService.RemoveUserKey(lupn, _config);
        }
        
        /// <summary>
        /// StripKeyPrefix method implementation
        /// </summary>
        public virtual string StripKeyPrefix(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            if (key.StartsWith(this.Prefix))
                key = key.Replace(this.Prefix, "");
            return key;
        }

        /// <summary>
        /// AddKeyPrefix method implementation
        /// </summary>
        public virtual string AddKeyPrefix(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            if (!key.StartsWith(this.Prefix))
                key = this.Prefix + key;
            return key;
        }

        /// <summary>
        /// HasKeyPrefix method implementation
        /// </summary>
        public virtual bool HasKeyPrefix(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
            return key.StartsWith(this.Prefix);
        }
    }

    /// <summary>
    /// RSAKeyManager class
    /// </summary>
    public class RSAKeyManager : ISecretKeyManager
    {
        private string _certificatethumbprint;
        private MFAConfig _config = null;
        private static Encryption _cryptoRSADataProvider = null;
        private KeySizeMode _ksize = KeySizeMode.KeySize1024;
        private int MAX_PROBE_LEN = 0;

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public void Initialize(MFAConfig config)
        {
            _config = config;
            _certificatethumbprint = config.KeysConfig.CertificateThumbprint;
            _ksize = config.KeysConfig.KeySize;
            switch (_ksize)
            {
                case KeySizeMode.KeySize512:
                    MAX_PROBE_LEN = 64;
                    break;
                case KeySizeMode.KeySize1024:
                    MAX_PROBE_LEN = 128;
                    break;
                case KeySizeMode.KeySize2048:
                    MAX_PROBE_LEN = 256;
                    break;
                default:
                    MAX_PROBE_LEN = 128;
                    break;
            }
        }

        /// <summary>
        /// Prefix property
        /// </summary>
        public string Prefix
        {
            get { return "rsa://";  }
        }

        /// <summary>
        /// NewKey method implementation
        /// </summary>
        public string NewKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return null;
            string lupn = upn.ToLower();
            string strcert = string.Empty;
            if (_cryptoRSADataProvider == null)
                _cryptoRSADataProvider = new Encryption(_certificatethumbprint);
            string crypted = AddKeyPrefix(_cryptoRSADataProvider.Encrypt(lupn));
            RepositoryService.NewUserKey(lupn, crypted, _config);
            return crypted;
        }

        /// <summary>
        /// ValidateKey method implmentation
        /// </summary>
        public bool ValidateKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return false;
            string lupn = upn.ToLower();
            string key = ReadKey(lupn);
            if (HasKeyPrefix(key))
            {
                if (_cryptoRSADataProvider == null)
                    _cryptoRSADataProvider = new Encryption(_certificatethumbprint);
                key = StripKeyPrefix(key);
                string user = _cryptoRSADataProvider.Decrypt(key);
                if (string.IsNullOrEmpty(user))
                    return false; // Key corrupted
                if (user.ToLower().Equals(lupn))
                    return true;  // OK RSA
                else
                    return false; // Key corrupted
            }
            else
                return false; 
        }

        /// <summary>
        /// ReadKey method implementation
        /// </summary>
        public string ReadKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return null;
            string lupn = upn.ToLower();
            return RepositoryService.GetUserKey(lupn, _config);
        }

        /// <summary>
        /// EncodedKey method implementation
        /// </summary>
        public string EncodedKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return null;
            string lupn = upn.ToLower();
            string full = StripKeyPrefix(ReadKey(lupn));
            if (full.Length > MAX_PROBE_LEN)
                return Base32.Encode(full.Substring(0, MAX_PROBE_LEN));
            else
                return Base32.Encode(full);
        }

        /// <summary>
        /// ProbeKey method implmentation
        /// </summary>
        public string ProbeKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return null;
            string lupn = upn.ToLower();
            string full = StripKeyPrefix(ReadKey(lupn));
            if (full.Length > MAX_PROBE_LEN)
                return full.Substring(0, MAX_PROBE_LEN);
            else
                return full;
        }

        /// <summary>
        /// RemoveKey method implementation
        /// </summary>
        public bool RemoveKey(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return false;
            string lupn = upn.ToLower();
            return RepositoryService.RemoveUserKey(lupn, _config);
        }
        
        /// <summary>
        /// StripKeyPrefix method implementation
        /// </summary>
        public virtual string StripKeyPrefix(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            if (key.StartsWith(this.Prefix))
                key = key.Replace(this.Prefix, "");
            return key;
        }

        /// <summary>
        /// AddKeyPrefix method implementation
        /// </summary>
        public virtual string AddKeyPrefix(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            if (!key.StartsWith(this.Prefix))
                key = this.Prefix + key;
            return key;
        }

        /// <summary>
        /// HasKeyPrefix method implementation
        /// </summary>
        public virtual bool HasKeyPrefix(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
            return key.StartsWith(this.Prefix);
        }
    }

    /// <summary>
    /// MailUtilities class
    /// </summary>
    public static class MailUtilities
    {
        private static object lck = 0;
        /// <summary>
        /// SetCultureInfo method implementation
        /// </summary>
        internal static void SetCultureInfo(int lcid)
        {
            System.Globalization.CultureInfo inf = new System.Globalization.CultureInfo(lcid);
            mail_strings.Culture = inf;
        }

        /// <summary>
        /// SendMail method implementation
        /// </summary>
        private static void SendMail(MailMessage Message, SendMail mail)
        {
            SmtpClient client = new SmtpClient();
            client.Host = mail.Host;
            client.Port = mail.Port;
            client.UseDefaultCredentials = false;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = mail.UseSSL;
            client.Credentials = new NetworkCredential(mail.UserName, mail.Password);
            client.Send(Message);
        }

        /// <summary>
        /// SendOTPByEmail method implementation
        /// </summary>
        public static void SendOTPByEmail(string to, string upn, string code, SendMail mail, CultureInfo culture)
        {
            string htmlres = string.Empty;
            if (mail.MailOTPContent != null)
            {
                int ctry = culture.LCID;
                string tmp = mail.MailOTPContent.Where(c => c.LCID.Equals(ctry) && c.Enabled).Select(s => s.FileName).FirstOrDefault();
                if (!string.IsNullOrEmpty(tmp))
                {
                    if (File.Exists(tmp))
                    {
                        FileStream fileStream = new FileStream(tmp, FileMode.Open, FileAccess.Read);

                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            htmlres = reader.ReadToEnd();
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(htmlres))
            {
                lock(lck)
                {
                    mail_strings.Culture = culture;
                    htmlres = mail_strings.MailOTPContent;
                }
            }
            string html = StripEmailContent(htmlres);
            string name = upn.Remove(2, upn.IndexOf('@') - 2).Insert(2, "*********");
            MailMessage Message = new MailMessage(mail.From, to);
            Message.BodyEncoding = UTF8Encoding.UTF8;
            Message.IsBodyHtml = true;
            Message.Body = string.Format(html, mail.Company, name, code);
            Message.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;
            lock(lck)
            {
                mail_strings.Culture = culture;
                Message.Subject = mail_strings.MailOTPTitle;
            }
            SendMail(Message, mail);
        }

        /// <summary>
        /// SendInscriptionMail method implementation
        /// </summary>
        public static void SendInscriptionMail(string to, Registration user, SendMail mail, CultureInfo culture)
        {
            string htmlres = string.Empty;
            if (mail.MailAdminContent != null)
            {
                int ctry = culture.LCID;
                string tmp = mail.MailAdminContent.Where(c => c.LCID.Equals(ctry) && c.Enabled).Select(s => s.FileName).FirstOrDefault();
                if (!string.IsNullOrEmpty(tmp))
                {
                    if (File.Exists(tmp))
                    {
                        FileStream fileStream = new FileStream(tmp, FileMode.Open, FileAccess.Read);

                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            htmlres = reader.ReadToEnd();
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(htmlres))
            {
                lock (lck)
                {
                    mail_strings.Culture = culture;
                    htmlres = mail_strings.MailAdminContent;
                }
            }
            string sendermail = GetUserBusinessEmail(user.UPN);
            string html = StripEmailContent(htmlres);
            MailMessage Message = new MailMessage(mail.From, to);
            if (!string.IsNullOrEmpty(sendermail))
                Message.CC.Add(sendermail);
            Message.BodyEncoding = UTF8Encoding.UTF8;
            Message.IsBodyHtml = true;
            Message.Body = string.Format(htmlres, mail.Company, user.UPN, user.MailAddress, user.PhoneNumber, user.PreferredMethod);
            Message.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;
            lock (lck)
            {
                mail_strings.Culture = culture;
                Message.Subject = string.Format(mail_strings.MailAdminTitle, user.UPN);
            }
            SendMail(Message, mail);
        }

        /// <summary>
        /// SendKeyByEmail method implementation
        /// </summary>
        public static void SendKeyByEmail(string email, string upn, string key, SendMail mail, MFAConfig config, CultureInfo culture)
        {
            string htmlres = string.Empty;
            if (mail.MailKeyContent != null)
            {
                int ctry = culture.LCID;
                string tmp = mail.MailKeyContent.Where(c => c.LCID.Equals(ctry) && c.Enabled).Select(s => s.FileName).FirstOrDefault();
                if (!string.IsNullOrEmpty(tmp))
                {
                    if (File.Exists(tmp))
                    {
                        FileStream fileStream = new FileStream(tmp, FileMode.Open, FileAccess.Read);

                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            htmlres = reader.ReadToEnd();
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(htmlres))
            {
                lock(lck)
                {
                    mail_strings.Culture = culture;
                    htmlres = mail_strings.MailKeyContent;
                }
            }

            string sendermail = GetUserBusinessEmail(upn);
            string html = StripEmailContent(htmlres);
            using (Stream qrcode = QRUtilities.GetQRCodeStream(upn, key, config))
            {
                qrcode.Position = 0;
                var inlineLogo = new LinkedResource(qrcode, "image/png");
                inlineLogo.ContentId = Guid.NewGuid().ToString();

                MailMessage Message = new MailMessage(mail.From, email);
                if (!string.IsNullOrEmpty(sendermail))
                    Message.CC.Add(sendermail);
                Message.BodyEncoding = UTF8Encoding.UTF8;
                Message.IsBodyHtml = true;

                Message.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;
                lock (lck)
                {
                    mail_strings.Culture = culture;
                    Message.Subject = mail_strings.MailKeyTitle;
                }
                Message.Priority = MailPriority.High;

                string body = string.Format(html, mail.Company, upn, key, inlineLogo.ContentId);
                var view = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                view.LinkedResources.Add(inlineLogo);
                Message.AlternateViews.Add(view);
                SendMail(Message, mail);
            }
        }

        /// <summary>
        /// GetUserBusinessEmail method implmentation
        /// </summary>
        internal static string GetUserBusinessEmail(string username)
        {
            using (var ctx = new PrincipalContext(ContextType.Domain))
            {
                using (var user = UserPrincipal.FindByIdentity(ctx, IdentityType.UserPrincipalName, username))
                {
                    return user.EmailAddress;
                }
            }
        }

        #region private methods
          /// <summary>
        /// StripPhoneNumer method
        /// </summary>
        public static string StripPhoneNumber(string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                    return "* ** ** ** **";
                else
                    return "* ** ** ** " + phone.Substring(phone.Length - 2, 2);
            }
            catch
            {
                return "* ** ** ** **";
            }
        }

        /// <summary>
        /// StripEmailAddress method
        /// </summary>
        public static string StripEmailAddress(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                    return string.Empty;
                else
                    return email.Remove(2, email.IndexOf('@') - 2).Insert(2, "*********");
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// StripEmailDomain method
        /// </summary>
        public static string StripEmailDomain(string email)
        {
            try
            {

                if (string.IsNullOrEmpty(email))
                    return string.Empty;
                else
                    return email.Substring(email.IndexOf("@", 0));
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// StripEmailContent method 
        /// </summary>
        internal static string StripEmailContent(string content)
        {
            return content.Replace("<![CDATA[", "").Replace("]]>", "");
        }
        #endregion
    }

    /// <summary>
    /// QRUtilities static class
    /// </summary>
    public static class QRUtilities
    {
        #region QRUtilities
        /// <summary>
        /// ConvertToBase64 method implmentation
        /// </summary>
        private static string ConvertToBase64(Stream stream)
        {
            Byte[] inArray = new Byte[(int)stream.Length];
            stream.Read(inArray, 0, (int)stream.Length);
            return Convert.ToBase64String(inArray);
        }

        /// <summary>
        /// ConvertToBase64Stream 
        /// </summary>
        private static Stream ConvertToBase64Stream(Stream stream)
        {
            byte[] inArray = new Byte[(int)stream.Length];
            stream.Read(inArray, 0, (int)stream.Length);
            return new MemoryStream(inArray);
        }

        /// <summary>
        /// GetQRCodeString method implmentation
        /// </summary>
        public static string GetQRCodeString(string UPN, string QRString, MFAConfig config)
        {
            string result = string.Empty;
            string Content = string.Format("otpauth://totp/{0}:{1}?secret={2}&issuer={0}&algorithm={3}", config.Issuer, UPN, QRString, config.Algorithm);

            var encoder = new QrEncoding.QrEncoder(ErrorCorrectionLevel.L);
            QrCode qr;
            if (!encoder.TryEncode(Content, out qr))
                return string.Empty;
            BitMatrix matrix = qr.Matrix;
            using (MemoryStream ms = new MemoryStream())
            {
                var render = new GraphicsRenderer(new FixedModuleSize(3, QuietZoneModules.Zero));
                render.WriteToStream(matrix, ImageFormat.Png, ms);
                ms.Position = 0;
                result = ConvertToBase64(ms);
            }
            return result;
        }

        /// <summary>
        /// GetQRCodeValue method implmentation
        /// </summary>
        public static string GetQRCodeValue(string UPN, string QRString, MFAConfig config)
        {
            string result = string.Empty;
            string Content = string.Format("otpauth://totp/{0}:{1}?secret={2}&issuer={0}&algorithm={3}", config.Issuer, UPN, QRString, config.Algorithm);
            return Content;
        }

        /// <summary>
        /// GetQRCodeStream method implmentation
        /// </summary>
        public static Stream GetQRCodeStream(string UPN, string QRString, MFAConfig config)
        {
            string result = string.Empty;
            string Content = string.Format("otpauth://totp/{0}:{1}?secret={2}&issuer={0}&algorithm={3}", config.Issuer, UPN, QRString, config.Algorithm);

            var encoder = new QrEncoding.QrEncoder(ErrorCorrectionLevel.L);
            QrCode qr;
            if (!encoder.TryEncode(Content, out qr))
                return null;
            BitMatrix matrix = qr.Matrix;
            var render = new GraphicsRenderer(new FixedModuleSize(3, QuietZoneModules.Zero));
            using (MemoryStream ms = new MemoryStream())
            {
                render.WriteToStream(matrix, ImageFormat.Png, ms);
                ms.Position = 0;
                return ConvertToBase64Stream(ms);
            }
        }
        #endregion
    }

    /// <summary>
    /// XmlConfigSerializer class
    /// </summary>
    public class XmlConfigSerializer : XmlSerializer
    {
        public XmlConfigSerializer(Type type): base(type)
        {
            this.UnknownAttribute += OnUnknownAttribute;
            this.UnknownElement += OnUnknownElement;
            this.UnknownNode += OnUnknownNode;
        }

        /// <summary>
        /// OnUnknownNode method implementation
        /// </summary>
        private void OnUnknownNode(object sender, XmlNodeEventArgs e)
        {
            Log.WriteEntry("Xml Serialization error : Unknow Node : "+e.Name+ " Position ("+ e.LineNumber.ToString()+", "+e.LinePosition.ToString()+")", EventLogEntryType.Warning, 700);
        }

        /// <summary>
        /// OnUnknownElement method implementation
        /// </summary>
        private void OnUnknownElement(object sender, XmlElementEventArgs e)
        {
            Log.WriteEntry("Xml Serialization error : Unknow Element : "+e.Element.Name+" at Position (" + e.LineNumber.ToString() + ", " + e.LinePosition.ToString() + ")", EventLogEntryType.Warning, 701);
        }

        /// <summary>
        /// OnUnknownAttribute method implementation
        /// </summary>
        private void OnUnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            Log.WriteEntry("Xml Serialization error : Unknow Attibute : "+e.Attr.Name+" at Position (" + e.LineNumber.ToString() + ", " + e.LinePosition.ToString() + ")", EventLogEntryType.Warning, 702);
        }
    }

    /// <summary>
    /// CFGUtilities class
    /// </summary>
    public static class CFGUtilities
    {
        /// <summary>
        /// internalReadConfiguration method implementation
        /// </summary>
        public static MFAConfig ReadConfiguration(PSHost Host = null)
        {
            MFAConfig config = null;
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            string pth = Path.GetTempPath() + Path.GetRandomFileName();
            try
            {
                try
                {
                    RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                    SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                    SPPowerShell = PowerShell.Create();
                    SPPowerShell.Runspace = SPRunSpace;
                    SPRunSpace.Open();

                    Pipeline pipeline = SPRunSpace.CreatePipeline();
                    Command exportcmd = new Command("Export-AdfsAuthenticationProviderConfigurationData", false);
                    CommandParameter NParam = new CommandParameter("Name", "MultifactorAuthenticationProvider");
                    exportcmd.Parameters.Add(NParam);
                    CommandParameter PParam = new CommandParameter("FilePath", pth);
                    exportcmd.Parameters.Add(PParam);
                    pipeline.Commands.Add(exportcmd);
                    Collection<PSObject> PSOutput = pipeline.Invoke();
                }
                finally
                {
                    if (SPRunSpace != null)
                        SPRunSpace.Close();
                }

                FileStream stm = new FileStream(pth, FileMode.Open, FileAccess.Read);
                XmlConfigSerializer xmlserializer = new XmlConfigSerializer(typeof(MFAConfig));
                using (StreamReader reader = new StreamReader(stm))
                {
                    config = (MFAConfig)xmlserializer.Deserialize(stm);
                    KeysManager.Initialize(config);
                }
            }
            finally
            {
                if (File.Exists(pth))
                    File.Delete(pth);
            }
            return config;
        }

        /// <summary>
        /// internalWriteConfiguration method implementation
        /// </summary>
        public static MFAConfig WriteConfiguration(PSHost Host, MFAConfig config)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            string pth = Path.GetTempPath() + Path.GetRandomFileName();
            try
            {
                FileStream stm = new FileStream(pth, FileMode.CreateNew, FileAccess.ReadWrite);
                XmlConfigSerializer xmlserializer = new XmlConfigSerializer(typeof(MFAConfig));
                stm.Position = 0;
                using (StreamReader reader = new StreamReader(stm))
                {
                    xmlserializer.Serialize(stm, config);
                }

                try
                {
                    RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                    SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                    SPPowerShell = PowerShell.Create();
                    SPPowerShell.Runspace = SPRunSpace;
                    SPRunSpace.Open();

                    Pipeline pipeline = SPRunSpace.CreatePipeline();
                    Command exportcmd = new Command("Import-AdfsAuthenticationProviderConfigurationData", false);
                    CommandParameter NParam = new CommandParameter("Name", "MultifactorAuthenticationProvider");
                    exportcmd.Parameters.Add(NParam);
                    CommandParameter PParam = new CommandParameter("FilePath", pth);
                    exportcmd.Parameters.Add(PParam);
                    pipeline.Commands.Add(exportcmd);
                    Collection<PSObject> PSOutput = pipeline.Invoke();
                }
                finally
                {
                    if (SPRunSpace != null)
                        SPRunSpace.Close();
                }
            }
            finally
            {
                if (File.Exists(pth))
                    File.Delete(pth);
            }
            return config;
        }
    }

    /// <summary>
    /// Utilities class
    /// </summary>
    public static class Utilities
    {
        private static IExternalOTPProvider _wrapper = null;

        /// <summary>
        /// GetRandomOTP  method implementation
        /// </summary>
        internal static int GetRandomOTP()
        {
            Random random = new Random();
            return random.Next(1, 1000000);
        }

        /// <summary>
        /// GetEmailOTP method implmentation
        /// </summary>
        public static int GetEmailOTP(Registration reg, SendMail mail, CultureInfo culture)
        {
            int otpres = GetRandomOTP();
            MailUtilities.SendOTPByEmail(reg.MailAddress, reg.UPN, otpres.ToString("D"), mail, culture);
            return otpres;
        }

        /// <summary>
        /// GetPhoneOTP()
        /// </summary>
        /// <returns></returns>
        public static int GetPhoneOTP(Registration reg, MFAConfig config, CultureInfo culture)
        {
            if (config.ExternalOTPProvider == null)
                return NotificationStatus.Error;
            if (reg.PreferredMethod == RegistrationPreferredMethod.Phone)
            {
                if (_wrapper == null)
                   _wrapper = LoadSMSWrapper(config.ExternalOTPProvider.FullQualifiedImplementation);
                if (_wrapper != null)
                   return _wrapper.GetUserCodeWithExternalSystem(reg.UPN, reg.PhoneNumber, reg.MailAddress, config.ExternalOTPProvider, culture);
                else
                    return NotificationStatus.Error;
            }
            else
                return NotificationStatus.Error;
        }

        /// <summary>
        /// LoadSMSwrapper method implmentation
        /// </summary>
        public static IExternalOTPProvider LoadSMSWrapper(string AssemblyFulldescription)
        {
            Assembly assembly = Assembly.Load(ParseAssembly(AssemblyFulldescription));
            Type _typetoload = assembly.GetType(ParseType(AssemblyFulldescription));
            IExternalOTPProvider wrapper = null;
            if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IExternalOTPProvider") != null)
            {
                object o = Activator.CreateInstance(_typetoload);
                if (o != null)
                    wrapper = o as IExternalOTPProvider;
            }
            return wrapper;
        }

        /// <summary>
        /// LoadExternalKeyManagerWrapper method implmentation
        /// </summary>
        public static ISecretKeyManager LoadExternalKeyManagerWrapper(string AssemblyFulldescription)
        {
            Assembly assembly = Assembly.Load(ParseAssembly(AssemblyFulldescription));
            Type _typetoload = assembly.GetType(ParseType(AssemblyFulldescription));
            ISecretKeyManager wrapper = null;
            if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("ISecretKeyManager") != null)
            {
                object o = Activator.CreateInstance(_typetoload);
                if (o != null)
                    wrapper = o as ISecretKeyManager;
            }
            return wrapper;
        }

        /// <summary>
        /// ParseType method implmentation
        /// </summary>
        private static string ParseAssembly(string AssemblyFulldescription)
        {
            int cnt = AssemblyFulldescription.IndexOf(',');
            return AssemblyFulldescription.Remove(0, cnt).TrimStart(new char[] { ',', ' ' });
        }

        /// <summary>
        /// ParseType method implmentation
        /// </summary>
        private static string ParseType(string AssemblyFulldescription)
        {
            string[] type = AssemblyFulldescription.Split(new char[] { ',' });
            return type[0];
        }
    }
}