//******************************************************************************************************************************************************************************************//
// Copyright (c) 2021 abergs (https://github.com/abergs/fido2-net-lib)                                                                                                                      //                        
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
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Objects
{
    /// <summary>
    /// Authenticators may implement various transports for communicating with clients. This enumeration defines hints as to how clients might communicate with a particular authenticator in order to obtain an assertion for a specific credential. Note that these hints represent the WebAuthn Relying Party's best belief as to how an authenticator may be reached. A Relying Party may obtain a list of transports hints from some attestation statement formats or via some out-of-band mechanism; it is outside the scope of this specification to define that mechanism. 
    /// https://w3c.github.io/webauthn/#transport
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AuthenticatorTransport
    {
        /// <summary>
        /// Indicates the respective authenticator can be contacted over removable USB.
        /// </summary>
        [EnumMember(Value = "usb")]
        Usb,

        /// <summary>
        /// Indicates the respective authenticator can be contacted over Near Field Communication (NFC).
        /// </summary>
        [EnumMember(Value = "nfc")]
        Nfc,

        /// <summary>
        /// Indicates the respective authenticator can be contacted over Bluetooth Smart(Bluetooth Low Energy / BLE)
        /// </summary>
        [EnumMember(Value = "ble")]
        Ble,

        /// <summary>
        /// Indicates the respective authenticator is contacted using a client device-specific transport.These authenticators are not removable from the client device.
        /// </summary>
        [EnumMember(Value = "internal")]
        Internal,

        /// <summary>
        /// Indicates the respective authenticator can be contacted over removable Lightning.
        /// </summary>
        [EnumMember(Value = "lightning")]
        Lightning
    }

    [Flags]
    public enum FIDOU2FTransports
    {
        bluetoothRadio = 128,
        bluetoothLowEnergyRadio = 64,
        uSB = 32,
        nFC = 16,
        uSBInternal = 8
    }
}
