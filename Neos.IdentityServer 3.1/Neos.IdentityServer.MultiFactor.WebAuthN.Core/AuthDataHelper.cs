//******************************************************************************************************************************************************************************************//
// Copyright (c) 2020 abergs (https://github.com/abergs/fido2-net-lib)                                                                                                                      //                        
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
using System.Linq;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    /// <summary>
    /// Helper functions that implements https://w3c.github.io/webauthn/#authenticator-data
    /// </summary>
    public static class AuthDataHelper
    {
        public static byte[] GetSizedByteArray(byte[] ab, ref int offset, ushort len = 0)
        {
            if ((0 == len) && ((offset + 2) <= ab.Length))
            {
                // len = BitConverter.ToUInt16(ab.Slice(offset, 2).ToArray().Reverse().ToArray(), 0);
                byte[] dest = new byte[2];
                Array.Copy(ab, offset, dest, 0, 2);
                len = BitConverter.ToUInt16(dest.ToArray().Reverse().ToArray(), 0);
                offset += 2;
            }
            byte[] result = null;
            if ((0 < len) && ((offset + len) <= ab.Length)) 
            {
                // result = ab.Slice(offset, len).ToArray();
                byte[] dest = new byte[len];
                Array.Copy(ab, offset, dest, 0, len);
                result = dest.ToArray();
                offset += len;
            }
            return result;
        }
    }
}
