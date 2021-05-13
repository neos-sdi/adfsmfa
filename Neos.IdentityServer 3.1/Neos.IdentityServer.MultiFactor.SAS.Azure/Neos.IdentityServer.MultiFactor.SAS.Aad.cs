//******************************************************************************************************************************************************************************************//
// Copyright (c) 2020 @redhook62 (adfsmfa@gmail.com)                                                                                                                                    //                        
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
//                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//

using Microsoft.Win32;
using Neos.IdentityServer.MultiFactor.Common;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using ADAL = Microsoft.IdentityServer.Aad.Sas.Adal.Net;

namespace Neos.IdentityServer.MultiFactor.SAS
{
    internal class NeosSasProvider : ISasProvider
	{
		private const string _sasserveiceUrl = "https://adnotifications.windowsazure.com/StrongAuthenticationService.svc/Connector";
		private const string _sasstsUrl = "https://login.microsoftonline.com";
		private const string _sasresourceUri = "https://adnotifications.windowsazure.com/StrongAuthenticationService.svc/Connector";
    
		private readonly string _tenantId;
		private readonly string _clientId;
        private readonly string _thumbprint;

		private readonly string _serviceUrl;
		private readonly string _resourceUri;
        private readonly string _stsstsUrl;
        private readonly ADAL.AuthenticationContext authContext;
		private ADAL.ClientAssertionCertificate _clientAssertion;
		private DateTime _certTimestamp;


        /// <summary>
        /// NeosSasProvider Constructor
        /// </summary>
		public NeosSasProvider(string tenantId, string clientId, string thumbprint)
		{
			this._tenantId = tenantId;
			this._clientId = clientId;
            this._thumbprint = thumbprint;
			this.GetClientCredentials();
			using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\ADFS", false))
			{
				if (registryKey != null)
				{
                    this._serviceUrl = registryKey.GetValue("SasUrl", _sasserveiceUrl).ToString();
                    this._stsstsUrl = registryKey.GetValue("StsUrl", _sasstsUrl).ToString();
                    this._resourceUri = registryKey.GetValue("ResourceUri", _sasresourceUri).ToString();
				}
			}
            this.authContext = new ADAL.AuthenticationContext(string.Format(CultureInfo.InvariantCulture, "{0}/{1}", new object[] { this._stsstsUrl, this._tenantId }));
		}

        #region ISasProvider methods
        /// <summary>
        /// GetAvailableAuthenticationMethods implmentation
        /// </summary>
        public GetAvailableAuthenticationMethodsResponse GetAvailableAuthenticationMethods(GetAvailableAuthenticationMethodsRequest request)
        {
            return CreateMessage<GetAvailableAuthenticationMethodsRequest, GetAvailableAuthenticationMethodsResponse>(string.Format(CultureInfo.InvariantCulture, "{0}/{1}", new object[] { this._serviceUrl, "GetAvailableAuthenticationMethods"}), request, new Action<HttpWebRequest>(this.SetAuthenticationHeader));
        }

        /// <summary>
        /// BeginTwoWayAuthentication method implmentation
        /// </summary>
        public BeginTwoWayAuthenticationResponse BeginTwoWayAuthentication(BeginTwoWayAuthenticationRequest request)
        {
            return CreateMessage<BeginTwoWayAuthenticationRequest, BeginTwoWayAuthenticationResponse>(string.Format(CultureInfo.InvariantCulture, "{0}/{1}", new object[] {this._serviceUrl, "BeginTwoWayAuthentication" }), request, new Action<HttpWebRequest>(this.SetAuthenticationHeader));
        }

        /// <summary>
        /// EndTwoWayAuthentication method implmentation
        /// </summary>
        public EndTwoWayAuthenticationResponse EndTwoWayAuthentication(EndTwoWayAuthenticationRequest request)
        {
            return CreateMessage<EndTwoWayAuthenticationRequest, EndTwoWayAuthenticationResponse>(string.Format(CultureInfo.InvariantCulture, "{0}/{1}", new object[] { this._serviceUrl, "EndTwoWayAuthentication"}), request, new Action<HttpWebRequest>(this.SetAuthenticationHeader));
        }

        /// <summary>
        /// GetActivationCode method implmentation
        /// </summary>
        public GetActivationCodeResponse GetActivationCode(GetActivationCodeRequest request)
        {
            return CreateMessage<GetActivationCodeRequest, GetActivationCodeResponse>(string.Format(CultureInfo.InvariantCulture, "{0}/{1}", new object[] { this._serviceUrl, "GetActivationCode"}), request, new Action<HttpWebRequest>(this.SetAuthenticationHeader));
        }

        /// <summary>
        /// GetActivationStatus method implmentation
        /// </summary>
        public GetActivationStatusResponse GetActivationStatus(GetActivationStatusRequest request)
        {
            return CreateMessage<GetActivationStatusRequest, GetActivationStatusResponse>(string.Format(CultureInfo.InvariantCulture, "{0}/{1}", new object[] { this._serviceUrl, "GetActivationStatus"}), request, new Action<HttpWebRequest>(this.SetAuthenticationHeader));
        }
        #endregion

        #region private methods
        /// <summary>
        /// SetAuthenticationHeader method implementation
        /// </summary>
        private void SetAuthenticationHeader(HttpWebRequest request)
        {
            if (this._clientAssertion != null)
            {
                if ((DateTime.Now.ToUniversalTime() - this._certTimestamp).TotalHours >= 24.0) // Reload every day
                {
                    this.GetClientCredentials();
                }
                ADAL.AuthenticationResult authenticationResult = null;
                try
                {
                    var taskResult = this.authContext.AcquireTokenAsync(this._resourceUri, this._clientAssertion);
                    authenticationResult = taskResult.Result;
                    request.Headers.Add(HttpRequestHeader.Authorization.ToString(), new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken).ToString());
                }
                catch (ADAL.AdalException ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// GetClientCredentials method implmentation
        /// </summary>
		private void GetClientCredentials()
		{
            try
            {
                X509Certificate2 azureCertificate = GetCertificate(this._thumbprint, StoreLocation.LocalMachine);
                this._clientAssertion = new ADAL.ClientAssertionCertificate(this._clientId, azureCertificate);
                this._certTimestamp = DateTime.Now.ToUniversalTime();
                azureCertificate.Reset();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// CreateMessage method implementation
        /// </summary>
        private TResponse CreateMessage<TRequest, TResponse>(string url, TRequest request, Action<HttpWebRequest> httpRequestModifier)
        {
            TResponse result = default(TResponse);
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            try
            {
                DataContractSerializer DCSRequest = new DataContractSerializer(typeof(TRequest));
                DataContractSerializer DCSResponse = new DataContractSerializer(typeof(TResponse));

                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "text/xml";
                httpWebRequest.CookieContainer = new CookieContainer();
                httpWebRequest.AllowAutoRedirect = true;
                httpWebRequest.Proxy = null;

                httpRequestModifier?.Invoke(httpWebRequest);

                Stream streamRequest;
                streamRequest = httpWebRequest.GetRequestStream();
                try
                {
                    DCSRequest.WriteObject(streamRequest, request);
                }
                finally
                {
                    if (streamRequest != null)
                        streamRequest.Close();
                    if (streamRequest != null)
                        streamRequest.Dispose();
                }

                Stream streamResponse = null;
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                try
                {
                    streamResponse = httpWebResponse.GetResponseStream();
                    if (streamResponse != null)
                        result = (TResponse)((object)DCSResponse.ReadObject(streamResponse));
                }
                finally
                {
                    if (streamResponse != null)
                        streamResponse.Close();

                    if (httpWebResponse != null)
                        httpWebResponse.Close();
                }
            }
            catch (Exception)
            {
                if (httpWebRequest != null)
                    httpWebRequest.Abort();
                throw;
            }
            return result;
        }

        /// <summary>
        /// GetCertificate method implementation
        /// </summary>
        private X509Certificate2 GetCertificate(string value, StoreLocation location)
        {
            X509Certificate2 data = null;
            X509Store store = new X509Store(location);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            try
            {
                X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
                X509Certificate2Collection findCollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByThumbprint, value, false);

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
        #endregion
    }
}
