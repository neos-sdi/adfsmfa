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
