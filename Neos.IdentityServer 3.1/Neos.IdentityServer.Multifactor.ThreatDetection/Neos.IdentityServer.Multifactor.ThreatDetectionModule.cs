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
// https://adfsmfa.codeplex.com                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
//#define testintranet
using Microsoft.IdentityServer.Public;
using Microsoft.IdentityServer.Public.ThreatDetectionFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor
{
    /// <summary>
    /// ThreatAnalyzer class implementation
    /// </summary>
    public class ThreatAnalyzer : ThreatDetectionModule, IRequestReceivedThreatDetectionModule, IPreAuthenticationThreatDetectionModule, IPostAuthenticationThreatDetectionModule
    {
        private List<string> _blockedIPs = new List<string>();

        public ThreatAnalyzer()
        {
        }

        public override string VendorName => "adfsmfa";
        public override string ModuleIdentifier => "ThreatkAnalyzer";

        /// <summary>
        /// OnAuthenticationPipelineLoad method override
        /// </summary>
        public override void OnAuthenticationPipelineLoad(ThreatDetectionLogger logger, ThreatDetectionModuleConfiguration configData)
        {
            try
            {
                ReadConfigFile(logger, configData);
            }
            catch (Exception ex)
            {
                logger?.WriteAdminLogErrorMessage(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// OnAuthenticationPipelineUnload method override
        /// </summary>
        public override void OnAuthenticationPipelineUnload(ThreatDetectionLogger logger)
        {
            _blockedIPs.Clear();
        }

        /// <summary>
        /// OnConfigurationUpdate method override
        /// </summary>
        public override void OnConfigurationUpdate(ThreatDetectionLogger logger, ThreatDetectionModuleConfiguration configData)
        {
            ReadConfigFile(logger, configData);
        }

        /// <summary>
        /// EvaluateRequest method implentation for interface IRequestReceivedThreatDetectionModule
        /// </summary>
        public Task<ThrottleStatus> EvaluateRequest(ThreatDetectionLogger logger, RequestContext requestContext)
        {
            try
            {
                if (requestContext.LocalEndPointAbsolutePath.ToLower().StartsWith("/adfs/proxy"))
                   return Task.FromResult<ThrottleStatus>(ThrottleStatus.Allow);

                if (requestContext.ClientLocation.HasValue && requestContext.ClientLocation.Value == NetworkLocation.Extranet)
                {
                    foreach (IPAddress clientIpAddress in requestContext.ClientIpAddresses)
                    {
                        logger?.WriteDebugMessage($"Block saw IP {clientIpAddress}");
                        string ss = clientIpAddress.ToString();
                        string ff = string.Empty;
                        int i = IndexOfNth(ss, '.', 2);
                        if (i != -1)
                            ff = ss.Substring(0, i);
                        else
                            ff = ss;
                        List<string> filtered = _blockedIPs.Where(x => x.StartsWith(ff)).ToList();
                        foreach (string s in filtered)
                        {
                            try
                            {
                                if (this.IsInSubnet(clientIpAddress, s))
                                {
                                    logger?.WriteDebugMessage($"Blocked request from IP {clientIpAddress}");
                                    return Task.FromResult<ThrottleStatus>(ThrottleStatus.Block);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger?.WriteDebugMessage($"Error {ex.Message} for IP {clientIpAddress}");
                            }
                        }
                    }
                }
                return Task.FromResult<ThrottleStatus>(ThrottleStatus.Allow);
            }
            catch
            {
                return Task.FromResult<ThrottleStatus>(ThrottleStatus.Block);
            }
        }

        /// <summary>
        /// EvaluatePreAuthentication method implentation for interface IPreAuthenticationThreatDetectionModule
        /// </summary>
        public Task<ThrottleStatus> EvaluatePreAuthentication(ThreatDetectionLogger logger, RequestContext requestContext, SecurityContext securityContext, ProtocolContext protocolContext, IList<Claim> additionalClaims)
        {
            return Task.FromResult<ThrottleStatus>(ThrottleStatus.Allow);
        }

        /// <summary>
        /// EvaluatePostAuthentication method implentation for interface IPostAuthenticationThreatDetectionModule
        /// </summary>
        public Task<RiskScore> EvaluatePostAuthentication(ThreatDetectionLogger logger, RequestContext requestContext, SecurityContext securityContext, ProtocolContext protocolContext, AuthenticationResult authenticationResult, IList<Claim> additionalClaims)
        {
            return Task.FromResult<RiskScore>(RiskScore.NotEvaluated);
        }

        /// <summary>
        /// ReadConfigFile method implementation
        /// </summary>
        private void ReadConfigFile(ThreatDetectionLogger logger, ThreatDetectionModuleConfiguration configData)
        {
            List<string> ipAddressSet = new List<string>();
            _blockedIPs.Clear();
            using (var sr = new StreamReader(configData.ReadData()))
            {
                while (sr.Peek() >= 0)
                {
                    string line = string.Empty;
                    try
                    {
                        line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line))
                            continue;
                        if (line.StartsWith("#"))
                            continue;
                        string[] values = line.Split(';');
                        foreach (string s in values)
                        {
                            string ipAddress = s;
                            if (string.IsNullOrEmpty(ipAddress.Trim()))
                                continue;
                            if (!ipAddress.Contains('/'))
                                ipAddress += "/32";
                            logger?.WriteDebugMessage($"Loaded IP {ipAddress}");
                            ipAddressSet.Add(ipAddress);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.WriteAdminLogErrorMessage($"Failed reading IP {line} exception {ex}");
                    }
                }
            }
            _blockedIPs = ipAddressSet;
        }

        /// <summary>
        /// IsInSubnet method implementation
        /// </summary>
        private bool IsInSubnet(IPAddress address, string subnetMask)
        {
              var slashIdx = subnetMask.IndexOf("/");
              if (slashIdx == -1)
              { 
                  throw new NotSupportedException("Only SubNetMasks with a given prefix length are supported.");
              }
              var maskAddress = IPAddress.Parse(subnetMask.Substring(0, slashIdx));
              if (maskAddress.AddressFamily != address.AddressFamily)
                 return false;

              int maskLength = int.Parse(subnetMask.Substring(slashIdx + 1));
              if (maskAddress.AddressFamily == AddressFamily.InterNetwork)
              {
                  var maskAddressBits = BitConverter.ToUInt32(maskAddress.GetAddressBytes().Reverse().ToArray(), 0);
                  var ipAddressBits = BitConverter.ToUInt32(address.GetAddressBytes().Reverse().ToArray(), 0);
                  uint mask = uint.MaxValue << (32 - maskLength);
                  return (maskAddressBits & mask) == (ipAddressBits & mask);
              }

              if (maskAddress.AddressFamily == AddressFamily.InterNetworkV6)
              {
                  var maskAddressBits = new BitArray(maskAddress.GetAddressBytes());
                  var ipAddressBits = new BitArray(address.GetAddressBytes());
                  if (maskAddressBits.Length != ipAddressBits.Length)
                  {
                      throw new ArgumentException("Length of IP Address and Subnet Mask do not match.");
                  }
                  for (int i = 0; i < maskLength; i++)
                  {
                      if (ipAddressBits[i] != maskAddressBits[i])
                      {
                          return false;
                      }
                  }
                  return true;
              }
              throw new NotSupportedException("Only InterNetworkV6 or InterNetwork address families are supported."); 
        }

        /// <summary>
        /// IndexOfNth method implementation
        /// </summary>
        private static int IndexOfNth(string str, char c, int n)
        {
            int s = -1;
            for (int i = 0; i < n; i++)
            {
                s = str.IndexOf(c, s + 1);
                if (s == -1) break;
            }
            return s;
        }
    }
}
