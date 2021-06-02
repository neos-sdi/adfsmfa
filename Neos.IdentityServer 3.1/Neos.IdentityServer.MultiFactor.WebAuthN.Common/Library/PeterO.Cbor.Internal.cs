//******************************************************************************************************************************************************************************************//
// Copyright (c) 2021 @redhook62 (adfsmfa@gmail.com)                                                                                                                                    //                        
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
//                                                                                                                                                                                          //
// Written by Peter O.                                                                                                                                                                      //
//                                                                                                                                                                                          //
// Any copyright is dedicated to the Public Domain.                                                                                                                                         //
// http://creativecommons.org/publicdomain/zero/1.0/                                                                                                                                        //
// If you like this, you should donate to Peter O. at: http://peteroupc.github.io/                                                                                                          //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using System;
using System.Text;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;

#pragma warning disable 618

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor
{
    #region Base64
    internal static class Base64
    {
        private const string Base64URL = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

        private const string Base64Classic = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        public static void WriteBase64(
          StringOutput writer,
          byte[] data,
          int offset,
          int count,
          bool padding)
        {
            WriteBase64(writer, data, offset, count, true, padding);
        }

        public static void WriteBase64URL(
          StringOutput writer,
          byte[] data,
          int offset,
          int count,
          bool padding)
        {
            WriteBase64(writer, data, offset, count, false, padding);
        }

        private static void WriteBase64(
          StringOutput writer,
          byte[] data,
          int offset,
          int count,
          bool classic,
          bool padding)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            if (offset < 0)
            {
                throw new ArgumentException("offset(" + offset + ") is less than " +
                  "0 ");
            }
            if (offset > data.Length)
            {
                throw new ArgumentException("offset(" + offset + ") is more than " +
                  data.Length);
            }
            if (count < 0)
            {
                throw new ArgumentException("count(" + count + ") is less than " +
                  "0 ");
            }
            if (count > data.Length)
            {
                throw new ArgumentException("count(" + count + ") is more than " +
                  data.Length);
            }
            if (data.Length - offset < count)
            {
                throw new ArgumentException("data's length minus " + offset + "(" +
                  (data.Length - offset) + ") is less than " + count);
            }
            string alphabet = classic ? Base64Classic : Base64URL;
            int length = offset + count;
            int i = offset;
            var buffer = new byte[32];
            var bufferOffset = 0;
            for (i = offset; i < (length - 2); i += 3)
            {
                if (bufferOffset >= buffer.Length)
                {
                    writer.WriteAscii(buffer, 0, bufferOffset);
                    bufferOffset = 0;
                }
                buffer[bufferOffset++] = (byte)alphabet[(data[i] >> 2) & 63];
                buffer[bufferOffset++] = (byte)alphabet[((data[i] & 3) << 4) +
                    ((data[i + 1] >> 4) & 15)];
                buffer[bufferOffset++] = (byte)alphabet[((data[i + 1] & 15) << 2) +
        ((data[i +
                          2] >> 6) & 3)];
                buffer[bufferOffset++] = (byte)alphabet[data[i + 2] & 63];
            }
            int lenmod3 = count % 3;
            if (lenmod3 != 0)
            {
                if (bufferOffset >= buffer.Length)
                {
                    writer.WriteAscii(buffer, 0, bufferOffset);
                    bufferOffset = 0;
                }
                i = length - lenmod3;
                buffer[bufferOffset++] = (byte)alphabet[(data[i] >> 2) & 63];
                if (lenmod3 == 2)
                {
                    buffer[bufferOffset++] = (byte)alphabet[((data[i] & 3) << 4) +
          ((data[i + 1] >>
                            4) & 15)];
                    buffer[bufferOffset++] = (byte)alphabet[(data[i + 1] & 15) << 2];
                    if (padding)
                    {
                        buffer[bufferOffset++] = (byte)'=';
                    }
                }
                else
                {
                    buffer[bufferOffset++] = (byte)alphabet[(data[i] & 3) << 4];
                    if (padding)
                    {
                        buffer[bufferOffset++] = (byte)'=';
                        buffer[bufferOffset++] = (byte)'=';
                    }
                }
            }
            if (bufferOffset >= 0)
            {
                writer.WriteAscii(buffer, 0, bufferOffset);
            }
        }
    }

    #endregion

    #region CBORCanonical
    internal static class CBORCanonical
    {
        internal static readonly IComparer<CBORObject> Comparer =
          new CtapComparer();

        private static readonly IComparer<KeyValuePair<byte[], byte[]>>
        ByteComparer = new CtapByteComparer();

        private sealed class CtapByteComparer : IComparer<KeyValuePair<byte[],
          byte[]>>
        {
            public int Compare(
              KeyValuePair<byte[], byte[]> kva,
              KeyValuePair<byte[], byte[]> kvb)
            {
                byte[] bytesA = kva.Key;
                byte[] bytesB = kvb.Key;
                if (bytesA == null)
                {
                    return bytesB == null ? 0 : -1;
                }
                if (bytesB == null)
                {
                    return 1;
                }
                if (bytesA.Length == 0)
                {
                    return bytesB.Length == 0 ? 0 : -1;
                }
                if (bytesB.Length == 0)
                {
                    return 1;
                }
                if (bytesA == bytesB)
                {
                    // NOTE: Assumes reference equality of CBORObjects
                    return 0;
                }
                // check major types
                if (((int)bytesA[0] & 0xe0) != ((int)bytesB[0] & 0xe0))
                {
                    return ((int)bytesA[0] & 0xe0) < ((int)bytesB[0] & 0xe0) ? -1 : 1;
                }
                // check lengths
                if (bytesA.Length != bytesB.Length)
                {
                    return bytesA.Length < bytesB.Length ? -1 : 1;
                }
                // check bytes
                for (var i = 0; i < bytesA.Length; ++i)
                {
                    if (bytesA[i] != bytesB[i])
                    {
                        int ai = ((int)bytesA[i]) & 0xff;
                        int bi = ((int)bytesB[i]) & 0xff;
                        return (ai < bi) ? -1 : 1;
                    }
                }
                return 0;
            }
        }

        private sealed class CtapComparer : IComparer<CBORObject>
        {
            private static int MajorType(CBORObject a)
            {
                if (a.IsTagged)
                {
                    return 6;
                }
                switch (a.Type)
                {
                    case CBORType.Integer:
                        return a.AsNumber().IsNegative() ? 1 : 0;
                    case CBORType.SimpleValue:
                    case CBORType.Boolean:
                    case CBORType.FloatingPoint:
                        return 7;
                    case CBORType.ByteString:
                        return 2;
                    case CBORType.TextString:
                        return 3;
                    case CBORType.Array:
                        return 4;
                    case CBORType.Map:
                        return 5;
                    default: throw new InvalidOperationException();
                }
            }

            public int Compare(CBORObject a, CBORObject b)
            {
                if (a == null)
                {
                    return b == null ? 0 : -1;
                }
                if (b == null)
                {
                    return 1;
                }
                if (a == b)
                {
                    // NOTE: Assumes reference equality of CBORObjects
                    return 0;
                }
                a = a.Untag();
                b = b.Untag();
                byte[] abs;
                byte[] bbs;
                int amt = MajorType(a);
                int bmt = MajorType(b);
                if (amt != bmt)
                {
                    return amt < bmt ? -1 : 1;
                }
                // DebugUtility.Log("a="+a);
                // DebugUtility.Log("b="+b);
                if (amt == 2)
                {
                    // Both objects are byte strings
                    abs = a.GetByteString();
                    bbs = b.GetByteString();
                }
                else
                {
                    // Might store arrays or maps, where
                    // canonical encoding can fail due to too-deep
                    // nesting
                    abs = CtapCanonicalEncode(a);
                    bbs = CtapCanonicalEncode(b);
                }
                if (abs.Length != bbs.Length)
                {
                    // different lengths
                    return abs.Length < bbs.Length ? -1 : 1;
                }
                for (var i = 0; i < abs.Length; ++i)
                {
                    if (abs[i] != bbs[i])
                    {
                        int ai = ((int)abs[i]) & 0xff;
                        int bi = ((int)bbs[i]) & 0xff;
                        return (ai < bi) ? -1 : 1;
                    }
                }
                return 0;
            }
        }

        private static bool IsArrayOrMap(CBORObject a)
        {
            return a.Type == CBORType.Array || a.Type == CBORType.Map;
        }

        public static byte[] CtapCanonicalEncode(CBORObject a)
        {
            return CtapCanonicalEncode(a, 0);
        }

        private static bool ByteArraysEqual(byte[] bytesA, byte[] bytesB)
        {
            if (bytesA == bytesB)
            {
                return true;
            }
            if (bytesA == null || bytesB == null)
            {
                return false;
            }
            if (bytesA.Length == bytesB.Length)
            {
                for (var j = 0; j < bytesA.Length; ++j)
                {
                    if (bytesA[j] != bytesB[j])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private static void CheckDepth(CBORObject cbor, int depth)
        {
            if (cbor.Type == CBORType.Array)
            {
                for (var i = 0; i < cbor.Count; ++i)
                {
                    if (depth >= 3 && IsArrayOrMap(cbor[i]))
                    {
                        throw new CBORException("Nesting level too deep");
                    }
                    CheckDepth(cbor[i], depth + 1);
                }
            }
            else if (cbor.Type == CBORType.Map)
            {
                foreach (CBORObject key in cbor.Keys)
                {
                    if (depth >= 3 && (IsArrayOrMap(key) ||
                        IsArrayOrMap(cbor[key])))
                    {
                        throw new CBORException("Nesting level too deep");
                    }
                    CheckDepth(key, depth + 1);
                    CheckDepth(cbor[key], depth + 1);
                }
            }
        }

        private static byte[] CtapCanonicalEncode(CBORObject a, int depth)
        {
            CBORObject cbor = a.Untag();
            CBORType valueAType = cbor.Type;
            try
            {
                if (valueAType == CBORType.Array)
                {
                    using (var ms = new MemoryStream())
                    {
                        CBORObject.WriteValue(ms, 4, cbor.Count);
                        for (var i = 0; i < cbor.Count; ++i)
                        {
                            if (depth >= 3 && IsArrayOrMap(cbor[i]))
                            {
                                throw new CBORException("Nesting level too deep");
                            }
                            byte[] bytes = CtapCanonicalEncode(cbor[i], depth + 1);
                            ms.Write(bytes, 0, bytes.Length);
                        }
                        return ms.ToArray();
                    }
                }
                else if (valueAType == CBORType.Map)
                {
                    KeyValuePair<byte[], byte[]> kv1;
                    List<KeyValuePair<byte[], byte[]>> sortedKeys;
                    sortedKeys = new List<KeyValuePair<byte[], byte[]>>();
                    foreach (CBORObject key in cbor.Keys)
                    {
                        if (depth >= 3 && (IsArrayOrMap(key) ||
                            IsArrayOrMap(cbor[key])))
                        {
                            throw new CBORException("Nesting level too deep");
                        }
                        CheckDepth(key, depth + 1);
                        CheckDepth(cbor[key], depth + 1);
                        // Check if key and value can be canonically encoded
                        // (will throw an exception if they cannot)
                        kv1 = new KeyValuePair<byte[], byte[]>(
                          CtapCanonicalEncode(key, depth + 1),
                          CtapCanonicalEncode(cbor[key], depth + 1));
                        sortedKeys.Add(kv1);
                    }
                    sortedKeys.Sort(ByteComparer);
                    using (var ms = new MemoryStream())
                    {
                        CBORObject.WriteValue(ms, 5, cbor.Count);
                        byte[] lastKey = null;
                        for (var i = 0; i < sortedKeys.Count; ++i)
                        {
                            kv1 = sortedKeys[i];
                            byte[] bytes = kv1.Key;
                            if (lastKey != null && ByteArraysEqual(bytes, lastKey))
                            {
                                throw new CBORException("duplicate canonical CBOR key");
                            }
                            lastKey = bytes;
                            ms.Write(bytes, 0, bytes.Length);
                            bytes = kv1.Value;
                            ms.Write(bytes, 0, bytes.Length);
                        }
                        return ms.ToArray();
                    }
                }
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException(ex.ToString(), ex);
            }
            if (valueAType == CBORType.SimpleValue ||
              valueAType == CBORType.Boolean || valueAType == CBORType.ByteString ||
              valueAType == CBORType.TextString)
            {
                return cbor.EncodeToBytes(CBOREncodeOptions.Default);
            }
            else if (valueAType == CBORType.FloatingPoint)
            {
                long bits = cbor.AsDoubleBits();
                return new byte[] {
          (byte)0xfb,
          (byte)((bits >> 56) & 0xffL),
          (byte)((bits >> 48) & 0xffL),
          (byte)((bits >> 40) & 0xffL),
          (byte)((bits >> 32) & 0xffL),
          (byte)((bits >> 24) & 0xffL),
          (byte)((bits >> 16) & 0xffL),
          (byte)((bits >> 8) & 0xffL),
          (byte)(bits & 0xffL),
        };
            }
            else if (valueAType == CBORType.Integer)
            {
                return cbor.EncodeToBytes(CBOREncodeOptions.Default);
            }
            else
            {
                throw new ArgumentException("Invalid CBOR type.");
            }
        }
    }
    #endregion

    #region CBORDataUtilities
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:CBORDataUtilities"]/*'/>
    public static class CBORDataUtilities
    {
        private const string HexAlphabet = "0123456789ABCDEF";

        private const long DoubleNegInfinity = unchecked((long)(0xfffL << 52));
        private const long DoublePosInfinity = unchecked((long)(0x7ffL << 52));

        internal static string ToStringHelper(CBORObject obj, int depth)
        {
            StringBuilder sb = null;
            string simvalue = null;
            CBORType type = obj.Type;
            CBORObject curobject;
            if (obj.IsTagged)
            {
                if (sb == null)
                {
                    if (type == CBORType.TextString)
                    {
                        // The default capacity of StringBuilder may be too small
                        // for many strings, so set a suggested capacity
                        // explicitly
                        string str = obj.AsString();
                        sb = new StringBuilder(Math.Min(str.Length, 4096) + 16);
                    }
                    else
                    {
                        sb = new StringBuilder();
                    }
                }
                // Append opening tags if needed
                curobject = obj;
                while (curobject.IsTagged)
                {
                    EInteger ei = curobject.MostOuterTag;
                    sb.Append(ei.ToString());
                    sb.Append('(');
                    curobject = curobject.UntagOne();
                }
            }
            switch (type)
            {
                case CBORType.SimpleValue:
                    sb = sb ?? new StringBuilder();
                    if (obj.IsUndefined)
                    {
                        sb.Append("undefined");
                    }
                    else if (obj.IsNull)
                    {
                        sb.Append("null");
                    }
                    else
                    {
                        sb.Append("simple(");
                        int thisItemInt = obj.SimpleValue;
                        char c;
                        if (thisItemInt >= 100)
                        {
                            // NOTE: '0'-'9' have ASCII code 0x30-0x39
                            c = (char)(0x30 + ((thisItemInt / 100) % 10));
                            sb.Append(c);
                        }
                        if (thisItemInt >= 10)
                        {
                            c = (char)(0x30 + ((thisItemInt / 10) % 10));
                            sb.Append(c);
                            c = (char)(0x30 + (thisItemInt % 10));
                        }
                        else
                        {
                            c = (char)(0x30 + thisItemInt);
                        }
                        sb.Append(c);
                        sb.Append(")");
                    }
                    break;
                case CBORType.Boolean:
                case CBORType.Integer:
                    simvalue = obj.Untag().ToJSONString();
                    if (sb == null)
                    {
                        return simvalue;
                    }
                    sb.Append(simvalue);
                    break;
                case CBORType.FloatingPoint:
                    {
                        long bits = obj.AsDoubleBits();
                        simvalue = bits == DoubleNegInfinity ? "-Infinity" : (
                            bits == DoublePosInfinity ? "Infinity" : (
                              CBORUtilities.DoubleBitsNaN(bits) ? "NaN" :
              obj.Untag().ToJSONString()));
                        if (sb == null)
                        {
                            return simvalue;
                        }
                        sb.Append(simvalue);
                        break;
                    }
                case CBORType.ByteString:
                    {
                        sb = sb ?? new StringBuilder();
                        sb.Append("h'");
                        byte[] data = obj.GetByteString();
                        int length = data.Length;
                        for (var i = 0; i < length; ++i)
                        {
                            sb.Append(HexAlphabet[(data[i] >> 4) & 15]);
                            sb.Append(HexAlphabet[data[i] & 15]);
                        }
                        sb.Append("'");
                        break;
                    }
                case CBORType.TextString:
                    {
                        sb = sb == null ? new StringBuilder() : sb;
                        sb.Append('\"');
                        string ostring = obj.AsString();
                        sb.Append(ostring);
                        /*
                        for (var i = 0; i < ostring.Length; ++i) {
                          if (ostring[i] >= 0x20 && ostring[i] <= 0x7f) {
                            sb.Append(ostring[i]);
                          } else {
                               sb.Append("\\u");
                               sb.Append(HexAlphabet[(ostring[i] >> 12) & 15]);
                               sb.Append(HexAlphabet[(ostring[i] >> 8) & 15]);
                               sb.Append(HexAlphabet[(ostring[i] >> 4) & 15]);
                               sb.Append(HexAlphabet[ostring[i] & 15]);
                           }
                        }
                        */
                        sb.Append('\"');
                        break;
                    }
                case CBORType.Array:
                    {
                        sb = sb ?? new StringBuilder();
                        var first = true;
                        sb.Append("[");
                        if (depth >= 50)
                        {
                            sb.Append("...");
                        }
                        else
                        {
                            for (var i = 0; i < obj.Count; ++i)
                            {
                                if (!first)
                                {
                                    sb.Append(", ");
                                }
                                sb.Append(ToStringHelper(obj[i], depth + 1));
                                first = false;
                            }
                        }
                        sb.Append("]");
                        break;
                    }
                case CBORType.Map:
                    {
                        sb = sb ?? new StringBuilder();
                        var first = true;
                        sb.Append("{");
                        if (depth >= 50)
                        {
                            sb.Append("...");
                        }
                        else
                        {
                            ICollection<KeyValuePair<CBORObject, CBORObject>> entries =
                              obj.Entries;
                            foreach (KeyValuePair<CBORObject, CBORObject> entry
                              in entries)
                            {
                                CBORObject key = entry.Key;
                                CBORObject value = entry.Value;
                                if (!first)
                                {
                                    sb.Append(", ");
                                }
                                sb.Append(ToStringHelper(key, depth + 1));
                                sb.Append(": ");
                                sb.Append(ToStringHelper(value, depth + 1));
                                first = false;
                            }
                        }
                        sb.Append("}");
                        break;
                    }
                default:
                    {
                        sb = sb ?? new StringBuilder();
                        sb.Append("???");
                        break;
                    }
            }
            // Append closing tags if needed
            curobject = obj;
            while (curobject.IsTagged)
            {
                sb.Append(')');
                curobject = curobject.UntagOne();
            }
            return sb.ToString();
        }

        internal static readonly JSONOptions DefaultOptions =
          new JSONOptions(String.Empty);
        private static readonly JSONOptions PreserveNegZeroNo =
          new JSONOptions("preservenegativezero=0");
        private static readonly JSONOptions PreserveNegZeroYes =
          new JSONOptions("preservenegativezero=1");

        /// <summary>Parses a number whose format follows the JSON
        /// specification. The method uses a JSONOptions with all default
        /// properties except for a PreserveNegativeZero property of
        /// false.</summary>
        /// <param name='str'>A text string to parse as a JSON number.</param>
        /// <returns>A CBOR object that represents the parsed number. Returns
        /// positive zero if the number is a zero that starts with a minus sign
        /// (such as "-0" or "-0.0"). Returns null if the parsing fails,
        /// including if the string is null or empty.</returns>
        public static CBORObject ParseJSONNumber(string str)
        {
            // TODO: Preserve negative zeros in next major version
            return ParseJSONNumber(str, PreserveNegZeroNo);
        }

        /// <summary>Parses a number whose format follows the JSON
        /// specification (RFC 8259). The method uses a JSONOptions with all
        /// default properties except for a PreserveNegativeZero property of
        /// false.</summary>
        /// <param name='str'>A text string to parse as a JSON number.</param>
        /// <param name='integersOnly'>If true, no decimal points or exponents
        /// are allowed in the string. The default is false.</param>
        /// <param name='positiveOnly'>If true, only positive numbers are
        /// allowed (the leading minus is disallowed). The default is
        /// false.</param>
        /// <returns>A CBOR object that represents the parsed number. Returns
        /// positive zero if the number is a zero that starts with a minus sign
        /// (such as "-0" or "-0.0"). Returns null if the parsing fails,
        /// including if the string is null or empty.</returns>
        /// <remarks>Roughly speaking, a valid JSON number consists of an
        /// optional minus sign, one or more basic digits (starting with 1 to 9
        /// unless there is only one digit and that digit is 0), an optional
        /// decimal point (".", full stop) with one or more basic digits, and
        /// an optional letter E or e with an optional plus or minus sign and
        /// one or more basic digits (the exponent). A text string representing
        /// a valid JSON number is not allowed to contain white space
        /// characters, including spaces.</remarks>
        [Obsolete("Call the one-argument version of this method instead. If this" +
            "\u0020method call used positiveOnly = true, check that the string" +
            "\u0020does not" + "\u0020begin" +
            "\u0020with '-' before calling that version. If this method call used" +
            "\u0020integersOnly" +
            "\u0020= true, check that the string does not contain '.', 'E', or" +
            "\u0020'e'" + "\u0020before" + "\u0020calling that version.")]
        public static CBORObject ParseJSONNumber(
          string str,
          bool integersOnly,
          bool positiveOnly)
        {
            if (String.IsNullOrEmpty(str))
            {
                return null;
            }
            if (integersOnly)
            {
                for (var i = 0; i < str.Length; ++i)
                {
                    if (str[i] >= '0' && str[i] <= '9' && (i > 0 || str[i] != '-'))
                    {
                        return null;
                    }
                }
            }
            return (positiveOnly && str[0] == '-') ? null :
              ParseJSONNumber(
                str,
                0,
                str.Length,
                PreserveNegZeroNo);
        }

        /// <summary>Parses a number whose format follows the JSON
        /// specification (RFC 8259).</summary>
        /// <param name='str'>A text string to parse as a JSON number.</param>
        /// <param name='integersOnly'>If true, no decimal points or exponents
        /// are allowed in the string. The default is false.</param>
        /// <param name='positiveOnly'>If true, the leading minus is disallowed
        /// in the string. The default is false.</param>
        /// <param name='preserveNegativeZero'>If true, returns positive zero
        /// if the number is a zero that starts with a minus sign (such as "-0"
        /// or "-0.0"). Otherwise, returns negative zero in this case. The
        /// default is false.</param>
        /// <returns>A CBOR object that represents the parsed number. Returns
        /// null if the parsing fails, including if the string is null or
        /// empty.</returns>
        /// <remarks>Roughly speaking, a valid JSON number consists of an
        /// optional minus sign, one or more basic digits (starting with 1 to 9
        /// unless there is only one digit and that digit is 0), an optional
        /// decimal point (".", full stop) with one or more basic digits, and
        /// an optional letter E or e with an optional plus or minus sign and
        /// one or more basic digits (the exponent). A text string representing
        /// a valid JSON number is not allowed to contain white space
        /// characters, including spaces.</remarks>
        [Obsolete("Instead, call ParseJSONNumber(str, jsonoptions) with" +
            "\u0020a JSONOptions that sets preserveNegativeZero to the" +
            "\u0020desired value, either true or false. If this" +
            "\u0020method call used positiveOnly = true, check that the string" +
            "\u0020does not" + "\u0020begin" +
            "\u0020with '-' before calling that version. If this method call used" +
            "\u0020integersOnly" +
            "\u0020= true, check that the string does not contain '.', 'E', or" +
            "\u0020'e'" + "\u0020before" + "\u0020calling that version.")]
        public static CBORObject ParseJSONNumber(
          string str,
          bool integersOnly,
          bool positiveOnly,
          bool preserveNegativeZero)
        {
            if (String.IsNullOrEmpty(str))
            {
                return null;
            }
            if (integersOnly)
            {
                for (var i = 0; i < str.Length; ++i)
                {
                    if (str[i] >= '0' && str[i] <= '9' && (i > 0 || str[i] != '-'))
                    {
                        return null;
                    }
                }
            }
            JSONOptions jo = preserveNegativeZero ? PreserveNegZeroYes :
              PreserveNegZeroNo;
            return (positiveOnly && str[0] == '-') ? null :
              ParseJSONNumber(str,
                0,
                str.Length,
                jo);
        }

        /// <summary>Parses a number whose format follows the JSON
        /// specification (RFC 8259) and converts that number to a CBOR
        /// object.</summary>
        /// <param name='str'>A text string to parse as a JSON number.</param>
        /// <param name='options'>An object containing options to control how
        /// JSON numbers are decoded to CBOR objects. Can be null, in which
        /// case a JSONOptions object with all default properties is used
        /// instead.</param>
        /// <returns>A CBOR object that represents the parsed number. Returns
        /// null if the parsing fails, including if the string is null or
        /// empty.</returns>
        /// <remarks>Roughly speaking, a valid JSON number consists of an
        /// optional minus sign, one or more basic digits (starting with 1 to 9
        /// unless there is only one digit and that digit is 0), an optional
        /// decimal point (".", full stop) with one or more basic digits, and
        /// an optional letter E or e with an optional plus or minus sign and
        /// one or more basic digits (the exponent). A text string representing
        /// a valid JSON number is not allowed to contain white space
        /// characters, including spaces.</remarks>
        public static CBORObject ParseJSONNumber(
          string str,
          JSONOptions options)
        {
            return String.IsNullOrEmpty(str) ? null :
              ParseJSONNumber(str,
                0,
                str.Length,
                options);
        }

        /// <summary>Parses a number whose format follows the JSON
        /// specification (RFC 8259) from a portion of a text string, and
        /// converts that number to a CBOR object.</summary>
        /// <param name='str'>A text string containing the portion to parse as
        /// a JSON number.</param>
        /// <param name='offset'>An index, starting at 0, showing where the
        /// desired portion of <paramref name='str'/> begins.</param>
        /// <param name='count'>The length, in code units, of the desired
        /// portion of <paramref name='str'/> (but not more than <paramref
        /// name='str'/> 's length).</param>
        /// <returns>A CBOR object that represents the parsed number. Returns
        /// null if the parsing fails, including if the string is null or
        /// empty.</returns>
        /// <exception cref='ArgumentException'>Either <paramref
        /// name='offset'/> or <paramref name='count'/> is less than 0 or
        /// greater than <paramref name='str'/> 's length, or <paramref
        /// name='str'/> 's length minus <paramref name='offset'/> is less than
        /// <paramref name='count'/>.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='str'/> is null.</exception>
        /// <remarks>Roughly speaking, a valid JSON number consists of an
        /// optional minus sign, one or more basic digits (starting with 1 to 9
        /// unless there is only one digit and that digit is 0), an optional
        /// decimal point (".", full stop) with one or more basic digits, and
        /// an optional letter E or e with an optional plus or minus sign and
        /// one or more basic digits (the exponent). A text string representing
        /// a valid JSON number is not allowed to contain white space
        /// characters, including spaces.</remarks>
        public static CBORObject ParseJSONNumber(
          string str,
          int offset,
          int count)
        {
            return String.IsNullOrEmpty(str) ? null :
              ParseJSONNumber(str,
                offset,
                count,
                JSONOptions.Default);
        }

        internal static CBORObject ParseSmallNumberAsNegative(
          int digit,
          JSONOptions options)
        {
#if DEBUG
            if (digit <= 0)
            {
                throw new ArgumentException("digit (" + digit + ") is not greater" +
                  "\u0020than 0");
            }
#endif

            if (options != null && options.NumberConversion ==
              JSONOptions.ConversionMode.Double)
            {
                return CBORObject.FromFloatingPointBits(
                   CBORUtilities.IntegerToDoubleBits(-digit),
                   8);
            }
            else if (options != null && options.NumberConversion ==
            JSONOptions.ConversionMode.Decimal128)
            {
                return CBORObject.FromObject(EDecimal.FromInt32(-digit));
            }
            else
            {
                // NOTE: Assumes digit is greater than zero, so PreserveNegativeZeros is
                // irrelevant
                return CBORObject.FromObject(-digit);
            }
        }

        internal static CBORObject ParseSmallNumber(int digit, JSONOptions
          options)
        {
#if DEBUG
            if (digit < 0)
            {
                throw new ArgumentException("digit (" + digit + ") is not greater" +
                  "\u0020or equal to 0");
            }
#endif

            if (options != null && options.NumberConversion ==
              JSONOptions.ConversionMode.Double)
            {
                return CBORObject.FromFloatingPointBits(
                   CBORUtilities.IntegerToDoubleBits(digit),
                   8);
            }
            else if (options != null && options.NumberConversion ==
            JSONOptions.ConversionMode.Decimal128)
            {
                return CBORObject.FromObject(EDecimal.FromInt32(digit));
            }
            else
            {
                // NOTE: Assumes digit is nonnegative, so PreserveNegativeZeros is irrelevant
                return CBORObject.FromObject(digit);
            }
        }

        /// <summary>Parses a number whose format follows the JSON
        /// specification (RFC 8259) and converts that number to a CBOR
        /// object.</summary>
        /// <param name='str'>A text string to parse as a JSON number.</param>
        /// <param name='offset'>An index, starting at 0, showing where the
        /// desired portion of <paramref name='str'/> begins.</param>
        /// <param name='count'>The length, in code units, of the desired
        /// portion of <paramref name='str'/> (but not more than <paramref
        /// name='str'/> 's length).</param>
        /// <param name='options'>An object containing options to control how
        /// JSON numbers are decoded to CBOR objects. Can be null, in which
        /// case a JSONOptions object with all default properties is used
        /// instead.</param>
        /// <returns>A CBOR object that represents the parsed number. Returns
        /// null if the parsing fails, including if the string is null or empty
        /// or <paramref name='count'/> is 0 or less.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='str'/> is null.</exception>
        /// <exception cref='ArgumentException'>Unsupported conversion
        /// kind.</exception>
        /// <remarks>Roughly speaking, a valid JSON number consists of an
        /// optional minus sign, one or more basic digits (starting with 1 to 9
        /// unless there is only one digit and that digit is 0), an optional
        /// decimal point (".", full stop) with one or more basic digits, and
        /// an optional letter E or e with an optional plus or minus sign and
        /// one or more basic digits (the exponent). A text string representing
        /// a valid JSON number is not allowed to contain white space
        /// characters, including spaces.</remarks>
        public static CBORObject ParseJSONNumber(
          string str,
          int offset,
          int count,
          JSONOptions options)
        {
            return CBORDataUtilitiesTextString.ParseJSONNumber(
              str,
              offset,
              count,
              options,
              null);
        }

        /// <summary>Parses a number from a byte sequence whose format follows
        /// the JSON specification (RFC 8259) and converts that number to a
        /// CBOR object.</summary>
        /// <param name='bytes'>A sequence of bytes to parse as a JSON
        /// number.</param>
        /// <param name='offset'>An index, starting at 0, showing where the
        /// desired portion of <paramref name='bytes'/> begins.</param>
        /// <param name='count'>The length, in code units, of the desired
        /// portion of <paramref name='bytes'/> (but not more than <paramref
        /// name='bytes'/> 's length).</param>
        /// <param name='options'>An object containing options to control how
        /// JSON numbers are decoded to CBOR objects. Can be null, in which
        /// case a JSONOptions object with all default properties is used
        /// instead.</param>
        /// <returns>A CBOR object that represents the parsed number. Returns
        /// null if the parsing fails, including if the byte sequence is null
        /// or empty or <paramref name='count'/> is 0 or less.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='bytes'/> is null.</exception>
        /// <exception cref='ArgumentException'>Unsupported conversion
        /// kind.</exception>
        /// <remarks>Roughly speaking, a valid JSON number consists of an
        /// optional minus sign, one or more basic digits (starting with 1 to 9
        /// unless there is only one digit and that digit is 0), an optional
        /// decimal point (".", full stop) with one or more basic digits, and
        /// an optional letter E or e with an optional plus or minus sign and
        /// one or more basic digits (the exponent). A byte sequence
        /// representing a valid JSON number is not allowed to contain white
        /// space characters, including spaces.</remarks>
        public static CBORObject ParseJSONNumber(
          byte[] bytes,
          int offset,
          int count,
          JSONOptions options)
        {
            return CBORDataUtilitiesByteArrayString.ParseJSONNumber(
              bytes,
              offset,
              count,
              options,
              null);
        }

        /// <summary>Parses a number from a byte sequence whose format follows
        /// the JSON specification (RFC 8259) and converts that number to a
        /// CBOR object.</summary>
        /// <param name='bytes'>A sequence of bytes to parse as a JSON
        /// number.</param>
        /// <param name='options'>An object containing options to control how
        /// JSON numbers are decoded to CBOR objects. Can be null, in which
        /// case a JSONOptions object with all default properties is used
        /// instead.</param>
        /// <returns>A CBOR object that represents the parsed number. Returns
        /// null if the parsing fails, including if the byte sequence is null
        /// or empty.</returns>
        /// <remarks>Roughly speaking, a valid JSON number consists of an
        /// optional minus sign, one or more basic digits (starting with 1 to 9
        /// unless there is only one digit and that digit is 0), an optional
        /// decimal point (".", full stop) with one or more basic digits, and
        /// an optional letter E or e with an optional plus or minus sign and
        /// one or more basic digits (the exponent). A byte sequence
        /// representing a valid JSON number is not allowed to contain white
        /// space characters, including spaces.</remarks>
        public static CBORObject ParseJSONNumber(
          byte[] bytes,
          JSONOptions options)
        {
            return (bytes == null || bytes.Length == 0) ? null :
              ParseJSONNumber(bytes,
                0,
                bytes.Length,
                options);
        }

        /// <summary>Parses a number whose format follows the JSON
        /// specification (RFC 8259) from a portion of a byte sequence, and
        /// converts that number to a CBOR object.</summary>
        /// <param name='bytes'>A sequence of bytes to parse as a JSON
        /// number.</param>
        /// <param name='offset'>An index, starting at 0, showing where the
        /// desired portion of <paramref name='bytes'/> begins.</param>
        /// <param name='count'>The length, in code units, of the desired
        /// portion of <paramref name='bytes'/> (but not more than <paramref
        /// name='bytes'/> 's length).</param>
        /// <returns>A CBOR object that represents the parsed number. Returns
        /// null if the parsing fails, including if the byte sequence is null
        /// or empty.</returns>
        /// <exception cref='ArgumentException'>Either <paramref
        /// name='offset'/> or <paramref name='count'/> is less than 0 or
        /// greater than <paramref name='bytes'/> 's length, or <paramref
        /// name='bytes'/> 's length minus <paramref name='offset'/> is less
        /// than <paramref name='count'/>.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='bytes'/> is null.</exception>
        /// <remarks>Roughly speaking, a valid JSON number consists of an
        /// optional minus sign, one or more basic digits (starting with 1 to 9
        /// unless there is only one digit and that digit is 0), an optional
        /// decimal point (".", full stop) with one or more basic digits, and
        /// an optional letter E or e with an optional plus or minus sign and
        /// one or more basic digits (the exponent). A byte sequence
        /// representing a valid JSON number is not allowed to contain white
        /// space characters, including spaces.</remarks>
        public static CBORObject ParseJSONNumber(
          byte[] bytes,
          int offset,
          int count)
        {
            return (bytes == null || bytes.Length == 0) ? null :
              ParseJSONNumber(bytes,
                offset,
                count,
                JSONOptions.Default);
        }

        /// <summary>Parses a number from a byte sequence whose format follows
        /// the JSON specification. The method uses a JSONOptions with all
        /// default properties except for a PreserveNegativeZero property of
        /// false.</summary>
        /// <param name='bytes'>A byte sequence to parse as a JSON
        /// number.</param>
        /// <returns>A CBOR object that represents the parsed number. Returns
        /// positive zero if the number is a zero that starts with a minus sign
        /// (such as "-0" or "-0.0"). Returns null if the parsing fails,
        /// including if the byte sequence is null or empty.</returns>
        public static CBORObject ParseJSONNumber(byte[] bytes)
        {
            // TODO: Preserve negative zeros in next major version
            return ParseJSONNumber(bytes, PreserveNegZeroNo);
        }

        /// <summary>Parses a number from a sequence of <c>char</c> s whose
        /// format follows the JSON specification (RFC 8259) and converts that
        /// number to a CBOR object.</summary>
        /// <param name='chars'>A sequence of <c>char</c> s to parse as a JSON
        /// number.</param>
        /// <param name='offset'>An index, starting at 0, showing where the
        /// desired portion of <paramref name='chars'/> begins.</param>
        /// <param name='count'>The length, in code units, of the desired
        /// portion of <paramref name='chars'/> (but not more than <paramref
        /// name='chars'/> 's length).</param>
        /// <param name='options'>An object containing options to control how
        /// JSON numbers are decoded to CBOR objects. Can be null, in which
        /// case a JSONOptions object with all default properties is used
        /// instead.</param>
        /// <returns>A CBOR object that represents the parsed number. Returns
        /// null if the parsing fails, including if the sequence of <c>char</c>
        /// s is null or empty or <paramref name='count'/> is 0 or
        /// less.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='chars'/> is null.</exception>
        /// <exception cref='ArgumentException'>Unsupported conversion
        /// kind.</exception>
        /// <remarks>Roughly speaking, a valid JSON number consists of an
        /// optional minus sign, one or more basic digits (starting with 1 to 9
        /// unless there is only one digit and that digit is 0), an optional
        /// decimal point (".", full stop) with one or more basic digits, and
        /// an optional letter E or e with an optional plus or minus sign and
        /// one or more basic digits (the exponent). A sequence of <c>char</c>
        /// s representing a valid JSON number is not allowed to contain white
        /// space characters, including spaces.</remarks>
        public static CBORObject ParseJSONNumber(
          char[] chars,
          int offset,
          int count,
          JSONOptions options)
        {
            return CBORDataUtilitiesCharArrayString.ParseJSONNumber(
              chars,
              offset,
              count,
              options,
              null);
        }

        /// <summary>Parses a number from a sequence of <c>char</c> s whose
        /// format follows the JSON specification (RFC 8259) and converts that
        /// number to a CBOR object.</summary>
        /// <param name='chars'>A sequence of <c>char</c> s to parse as a JSON
        /// number.</param>
        /// <param name='options'>An object containing options to control how
        /// JSON numbers are decoded to CBOR objects. Can be null, in which
        /// case a JSONOptions object with all default properties is used
        /// instead.</param>
        /// <returns>A CBOR object that represents the parsed number. Returns
        /// null if the parsing fails, including if the sequence of <c>char</c>
        /// s is null or empty.</returns>
        /// <remarks>Roughly speaking, a valid JSON number consists of an
        /// optional minus sign, one or more basic digits (starting with 1 to 9
        /// unless there is only one digit and that digit is 0), an optional
        /// decimal point (".", full stop) with one or more basic digits, and
        /// an optional letter E or e with an optional plus or minus sign and
        /// one or more basic digits (the exponent). A sequence of <c>char</c>
        /// s representing a valid JSON number is not allowed to contain white
        /// space characters, including spaces.</remarks>
        public static CBORObject ParseJSONNumber(
          char[] chars,
          JSONOptions options)
        {
            return (chars == null || chars.Length == 0) ? null :
              ParseJSONNumber(chars,
                0,
                chars.Length,
                options);
        }

        /// <summary>Parses a number whose format follows the JSON
        /// specification (RFC 8259) from a portion of a sequence of
        /// <c>char</c> s, and converts that number to a CBOR object.</summary>
        /// <param name='chars'>A sequence of <c>char</c> s to parse as a JSON
        /// number.</param>
        /// <param name='offset'>An index, starting at 0, showing where the
        /// desired portion of <paramref name='chars'/> begins.</param>
        /// <param name='count'>The length, in code units, of the desired
        /// portion of <paramref name='chars'/> (but not more than <paramref
        /// name='chars'/> 's length).</param>
        /// <returns>A CBOR object that represents the parsed number. Returns
        /// null if the parsing fails, including if the sequence of <c>char</c>
        /// s is null or empty.</returns>
        /// <exception cref='ArgumentException'>Either <paramref
        /// name='offset'/> or <paramref name='count'/> is less than 0 or
        /// greater than <paramref name='chars'/> 's length, or <paramref
        /// name='chars'/> 's length minus <paramref name='offset'/> is less
        /// than <paramref name='count'/>.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='chars'/> is null.</exception>
        /// <remarks>Roughly speaking, a valid JSON number consists of an
        /// optional minus sign, one or more basic digits (starting with 1 to 9
        /// unless there is only one digit and that digit is 0), an optional
        /// decimal point (".", full stop) with one or more basic digits, and
        /// an optional letter E or e with an optional plus or minus sign and
        /// one or more basic digits (the exponent). A sequence of <c>char</c>
        /// s representing a valid JSON number is not allowed to contain white
        /// space characters, including spaces.</remarks>
        public static CBORObject ParseJSONNumber(
          char[] chars,
          int offset,
          int count)
        {
            return (chars == null || chars.Length == 0) ? null :
              ParseJSONNumber(chars,
                offset,
                count,
                JSONOptions.Default);
        }

        /// <summary>Parses a number from a sequence of <c>char</c> s whose
        /// format follows the JSON specification. The method uses a
        /// JSONOptions with all default properties except for a
        /// PreserveNegativeZero property of false.</summary>
        /// <param name='chars'>A sequence of <c>char</c> s to parse as a JSON
        /// number.</param>
        /// <returns>A CBOR object that represents the parsed number. Returns
        /// positive zero if the number is a zero that starts with a minus sign
        /// (such as "-0" or "-0.0"). Returns null if the parsing fails,
        /// including if the sequence of <c>char</c> s is null or
        /// empty.</returns>
        public static CBORObject ParseJSONNumber(char[] chars)
        {
            // TODO: Preserve negative zeros in next major version
            return ParseJSONNumber(chars, PreserveNegZeroNo);
        }
    }
    #endregion

    #region CBORDateConverter
    internal class CBORDateConverter : ICBORToFromConverter<DateTime>
    {
        private static string DateTimeToString(DateTime bi)
        {
            var lesserFields = new int[7];
            var year = new EInteger[1];
            PropertyMap.BreakDownDateTime(bi, year, lesserFields);
            return CBORUtilities.ToAtomDateTimeString(year[0], lesserFields);
        }

        public DateTime FromCBORObject(CBORObject obj)
        {
            if (obj.HasMostOuterTag(0))
            {
                try
                {
                    return StringToDateTime(obj.AsString());
                }
                catch (OverflowException ex)
                {
                    throw new CBORException(ex.Message, ex);
                }
                catch (InvalidOperationException ex)
                {
                    throw new CBORException(ex.Message, ex);
                }
                catch (ArgumentException ex)
                {
                    throw new CBORException(ex.Message, ex);
                }
            }
            else if (obj.HasMostOuterTag(1))
            {
                if (!obj.IsNumber || !obj.AsNumber().IsFinite())
                {
                    throw new CBORException("Not a finite number");
                }
                EDecimal dec;
                dec = (EDecimal)obj.ToObject(typeof(EDecimal));
                var lesserFields = new int[7];
                var year = new EInteger[1];
                CBORUtilities.BreakDownSecondsSinceEpoch(
                  dec,
                  year,
                  lesserFields);
                return PropertyMap.BuildUpDateTime(year[0], lesserFields);
            }
            throw new CBORException("Not tag 0 or 1");
        }

        public static DateTime StringToDateTime(string str)
        {
            var lesserFields = new int[7];
            var year = new EInteger[1];
            CBORUtilities.ParseAtomDateTimeString(str, year, lesserFields);
            return PropertyMap.BuildUpDateTime(year[0], lesserFields);
        }

        public CBORObject ToCBORObject(DateTime obj)
        {
            return CBORObject.FromObjectAndTag(DateTimeToString(obj), 0);
        }
    }
    #endregion

    #region CBORDouble
    internal class CBORDoubleBits : ICBORNumber
    {
        public bool IsPositiveInfinity(object obj)
        {
            return ((long)obj) == (0x7ffL << 52);
        }

        public bool IsInfinity(object obj)
        {
            return (((long)obj) & ~(1L << 63)) == (0x7ffL << 52);
        }

        public bool IsNegativeInfinity(object obj)
        {
            return ((long)obj) == (0xfffL << 52);
        }

        public bool IsNaN(object obj)
        {
            return CBORUtilities.DoubleBitsNaN((long)obj);
        }

        public double AsDouble(object obj)
        {
            return CBORUtilities.Int64BitsToDouble((long)obj);
        }

        public EDecimal AsEDecimal(object obj)
        {
            return EDecimal.FromDoubleBits((long)obj);
        }

        public EFloat AsEFloat(object obj)
        {
            return EFloat.FromDoubleBits((long)obj);
        }

        public float AsSingle(object obj)
        {
            return CBORUtilities.Int32BitsToSingle(
              CBORUtilities.DoubleToRoundedSinglePrecision((long)obj));
        }

        public EInteger AsEInteger(object obj)
        {
            return CBORUtilities.EIntegerFromDoubleBits((long)obj);
        }

        public long AsInt64(object obj)
        {
            if (this.IsNaN(obj) || this.IsInfinity(obj))
            {
                throw new OverflowException("This object's value is out of range");
            }
            long b = DoubleBitsRoundDown((long)obj);
            bool neg = (b >> 63) != 0;
            b &= ~(1L << 63);
            if (b == 0)
            {
                return 0;
            }
            if (neg && b == (0x43eL << 52))
            {
                return Int64.MinValue;
            }
            if ((b >> 52) >= 0x43e)
            {
                throw new OverflowException("This object's value is out of range");
            }
            var exp = (int)(b >> 52);
            long mant = b & ((1L << 52) - 1);
            mant |= 1L << 52;
            int shift = 52 - (exp - 0x3ff);
            if (shift < 0)
            {
                mant <<= -shift;
            }
            else
            {
                mant >>= shift;
            }
            if (neg)
            {
                mant = -mant;
            }
            return mant;
        }

        public bool CanFitInSingle(object obj)
        {
            return this.IsNaN(obj) ||
      CBORUtilities.DoubleRetainsSameValueInSingle((long)obj);
        }

        public bool CanFitInDouble(object obj)
        {
            return true;
        }

        public bool CanFitInInt32(object obj)
        {
            return this.IsIntegral(obj) && this.CanTruncatedIntFitInInt32(obj);
        }

        public bool CanFitInInt64(object obj)
        {
            return this.IsIntegral(obj) && this.CanTruncatedIntFitInInt64(obj);
        }

        private static long DoubleBitsRoundDown(long bits)
        {
            long origbits = bits;
            bits &= ~(1L << 63);
            if (bits == 0)
            {
                return origbits;
            }
            // Infinity and NaN
            if (bits >= unchecked((long)(0x7ffL << 52)))
            {
                return origbits;
            }
            // Beyond non-integer range
            if ((bits >> 52) >= 0x433)
            {
                return origbits;
            }
            // Less than 1
            if ((bits >> 52) <= 0x3fe)
            {
                return (origbits >> 63) != 0 ? (1L << 63) : 0;
            }
            var exp = (int)(bits >> 52);
            long mant = bits & ((1L << 52) - 1);
            int shift = 52 - (exp - 0x3ff);
            return ((mant >> shift) << shift) | (origbits & (0xfffL << 52));
        }

        public bool CanTruncatedIntFitInInt64(object obj)
        {
            if (this.IsNaN(obj) || this.IsInfinity(obj))
            {
                return false;
            }
            long b = DoubleBitsRoundDown((long)obj);
            bool neg = (b >> 63) != 0;
            b &= ~(1L << 63);
            return (neg && b == (0x43eL << 52)) || ((b >> 52) < 0x43e);
        }

        public bool CanTruncatedIntFitInInt32(object obj)
        {
            if (this.IsNaN(obj) || this.IsInfinity(obj))
            {
                return false;
            }
            long b = DoubleBitsRoundDown((long)obj);
            bool neg = (b >> 63) != 0;
            b &= ~(1L << 63);
            return (neg && b == (0x41eL << 52)) || ((b >> 52) < 0x41e);
        }

        public int AsInt32(object obj, int minValue, int maxValue)
        {
            if (this.IsNaN(obj) || this.IsInfinity(obj))
            {
                throw new OverflowException("This object's value is out of range");
            }
            long b = DoubleBitsRoundDown((long)obj);
            bool neg = (b >> 63) != 0;
            b &= ~(1L << 63);
            if (b == 0)
            {
                return 0;
            }
            // Beyond non-integer range (thus beyond int32 range)
            if ((b >> 52) >= 0x433)
            {
                throw new OverflowException("This object's value is out of range");
            }
            var exp = (int)(b >> 52);
            long mant = b & ((1L << 52) - 1);
            mant |= 1L << 52;
            int shift = 52 - (exp - 0x3ff);
            mant >>= shift;
            if (neg)
            {
                mant = -mant;
            }
            if (mant < minValue || mant > maxValue)
            {
                throw new OverflowException("This object's value is out of range");
            }
            return (int)mant;
        }

        public bool IsNumberZero(object obj)
        {
            return (((long)obj) & ~(1L << 63)) == 0;
        }

        public int Sign(object obj)
        {
            return this.IsNaN(obj) ? (-2) : ((((long)obj) >> 63) != 0 ? -1 : 1);
        }

        public bool IsIntegral(object obj)
        {
            return CBORUtilities.IsIntegerValue((long)obj);
        }

        public object Negate(object obj)
        {
            return ((long)obj) ^ (1L << 63);
        }

        public object Abs(object obj)
        {
            return ((long)obj) & ~(1L << 63);
        }

        public ERational AsERational(object obj)
        {
            return ERational.FromDoubleBits((long)obj);
        }

        public bool IsNegative(object obj)
        {
            return (((long)obj) >> 63) != 0;
        }
    }
    #endregion

    #region CBORInterger
    internal class CBOREInteger : ICBORNumber
    {
        public bool IsPositiveInfinity(object obj)
        {
            return false;
        }

        public bool IsInfinity(object obj)
        {
            return false;
        }

        public bool IsNegativeInfinity(object obj)
        {
            return false;
        }

        public bool IsNaN(object obj)
        {
            return false;
        }

        public double AsDouble(object obj)
        {
            return EFloat.FromEInteger((EInteger)obj).ToDouble();
        }

        public EDecimal AsEDecimal(object obj)
        {
            return EDecimal.FromEInteger((EInteger)obj);
        }

        public EFloat AsEFloat(object obj)
        {
            return EFloat.FromEInteger((EInteger)obj);
        }

        public float AsSingle(object obj)
        {
            return EFloat.FromEInteger((EInteger)obj).ToSingle();
        }

        public EInteger AsEInteger(object obj)
        {
            return (EInteger)obj;
        }

        public long AsInt64(object obj)
        {
            var bi = (EInteger)obj;
            if (!bi.CanFitInInt64())
            {
                throw new OverflowException("This object's value is out of range");
            }
            return (long)bi;
        }

        public bool CanFitInSingle(object obj)
        {
            var bigintItem = (EInteger)obj;
            EFloat ef = EFloat.FromEInteger(bigintItem);
            EFloat ef2 = EFloat.FromSingle(ef.ToSingle());
            return ef.CompareTo(ef2) == 0;
        }

        public bool CanFitInDouble(object obj)
        {
            var bigintItem = (EInteger)obj;
            EFloat ef = EFloat.FromEInteger(bigintItem);
            EFloat ef2 = EFloat.FromDouble(ef.ToDouble());
            return ef.CompareTo(ef2) == 0;
        }

        public bool CanFitInInt32(object obj)
        {
            var bi = (EInteger)obj;
            return bi.CanFitInInt32();
        }

        public bool CanFitInInt64(object obj)
        {
            var bi = (EInteger)obj;
            return bi.CanFitInInt64();
        }

        public bool CanTruncatedIntFitInInt64(object obj)
        {
            return this.CanFitInInt64(obj);
        }

        public bool CanTruncatedIntFitInInt32(object obj)
        {
            return this.CanFitInInt32(obj);
        }

        public bool IsNumberZero(object obj)
        {
            return ((EInteger)obj).IsZero;
        }

        public int Sign(object obj)
        {
            return ((EInteger)obj).Sign;
        }

        public bool IsIntegral(object obj)
        {
            return true;
        }

        public int AsInt32(object obj, int minValue, int maxValue)
        {
            var bi = (EInteger)obj;
            if (bi.CanFitInInt32())
            {
                var ret = (int)bi;
                if (ret >= minValue && ret <= maxValue)
                {
                    return ret;
                }
            }
            throw new OverflowException("This object's value is out of range");
        }

        public object Negate(object obj)
        {
            var bigobj = (EInteger)obj;
            bigobj = -(EInteger)bigobj;
            return bigobj;
        }

        public object Abs(object obj)
        {
            return ((EInteger)obj).Abs();
        }

        public ERational AsERational(object obj)
        {
            return ERational.FromEInteger((EInteger)obj);
        }

        public bool IsNegative(object obj)
        {
            return ((EInteger)obj).Sign < 0;
        }
    }

    #endregion

    #region CBOREncodeOptions
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:CBOREncodeOptions"]/*'/>
    public sealed class CBOREncodeOptions
    {
        /// <summary>Default options for CBOR objects. Disallow duplicate keys,
        /// and always encode strings using definite-length encoding.</summary>
        public static readonly CBOREncodeOptions Default =
          new CBOREncodeOptions(false, false);

        /// <summary>Default options for CBOR objects serialized using the
        /// CTAP2 canonicalization (used in Web Authentication, among other
        /// specifications). Disallow duplicate keys, and always encode strings
        /// using definite-length encoding.</summary>
        public static readonly CBOREncodeOptions DefaultCtap2Canonical =
          new CBOREncodeOptions(false, false, true);

        /// <summary>Initializes a new instance of the
        /// <see cref='PeterO.Cbor.CBOREncodeOptions'/> class.</summary>
        public CBOREncodeOptions() : this(false, false)
        {
        }

        /// <summary>Initializes a new instance of the
        /// <see cref='PeterO.Cbor.CBOREncodeOptions'/> class.</summary>
        /// <param name='useIndefLengthStrings'>A value indicating whether to
        /// always encode strings with a definite-length encoding.</param>
        /// <param name='allowDuplicateKeys'>A value indicating whether to
        /// disallow duplicate keys when reading CBOR objects from a data
        /// stream.</param>
        public CBOREncodeOptions(
          bool useIndefLengthStrings,
          bool allowDuplicateKeys)
          : this(useIndefLengthStrings, allowDuplicateKeys, false)
        {
        }

        /// <summary>Initializes a new instance of the
        /// <see cref='PeterO.Cbor.CBOREncodeOptions'/> class.</summary>
        /// <param name='useIndefLengthStrings'>A value indicating whether to
        /// encode strings with a definite-length encoding in certain
        /// cases.</param>
        /// <param name='allowDuplicateKeys'>A value indicating whether to
        /// allow duplicate keys when reading CBOR objects from a data
        /// stream.</param>
        /// <param name='ctap2Canonical'>A value indicating whether CBOR
        /// objects are written out using the CTAP2 canonical CBOR encoding
        /// form, which is useful for implementing Web Authentication.</param>
        public CBOREncodeOptions(
          bool useIndefLengthStrings,
          bool allowDuplicateKeys,
          bool ctap2Canonical)
        {
            this.ResolveReferences = false;
            this.AllowEmpty = false;
            this.UseIndefLengthStrings = useIndefLengthStrings;
            this.AllowDuplicateKeys = allowDuplicateKeys;
            this.Ctap2Canonical = ctap2Canonical;
        }

        /// <summary>Initializes a new instance of the
        /// <see cref='PeterO.Cbor.CBOREncodeOptions'/> class.</summary>
        /// <param name='paramString'>A string setting forth the options to
        /// use. This is a semicolon-separated list of options, each of which
        /// has a key and a value separated by an equal sign ("="). Whitespace
        /// and line separators are not allowed to appear between the
        /// semicolons or between the equal signs, nor may the string begin or
        /// end with whitespace. The string can be empty, but cannot be null.
        /// The following is an example of this parameter:
        /// <c>allowduplicatekeys=true;ctap2Canonical=true</c>. The key can be
        /// any one of the following where the letters can be any combination
        /// of basic upper-case and/or basic lower-case letters:
        /// <c>allowduplicatekeys</c>, <c>ctap2canonical</c>,
        /// <c>resolvereferences</c>, <c>useindeflengthstrings</c>,
        /// <c>allowempty</c>. Keys other than these are ignored. (Keys are
        /// compared using a basic case-insensitive comparison, in which two
        /// strings are equal if they match after converting the basic
        /// upper-case letters A to Z (U+0041 to U+005A) in both strings to
        /// basic lower-case letters.) If two or more key/value pairs have
        /// equal keys (in a basic case-insensitive comparison), the value
        /// given for the last such key is used. The four keys just given can
        /// have a value of <c>1</c>, <c>true</c>, <c>yes</c>, or <c>on</c>
        /// (where the letters can be any combination of basic upper-case
        /// and/or basic lower-case letters), which means true, and any other
        /// value meaning false. For example, <c>allowduplicatekeys=Yes</c> and
        /// <c>allowduplicatekeys=1</c> both set the <c>AllowDuplicateKeys</c>
        /// property to true. In the future, this class may allow other keys to
        /// store other kinds of values, not just true or false.</param>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='paramString'/> is null.</exception>
        public CBOREncodeOptions(string paramString)
        {
            if (paramString == null)
            {
                throw new ArgumentNullException(nameof(paramString));
            }
            var parser = new OptionsParser(paramString);
            this.ResolveReferences = parser.GetBoolean("resolvereferences",
                false);
            this.UseIndefLengthStrings = parser.GetBoolean(
              "useindeflengthstrings",
              false);
            this.AllowDuplicateKeys = parser.GetBoolean("allowduplicatekeys",
                false);
            this.AllowEmpty = parser.GetBoolean("allowempty", false);
            this.Ctap2Canonical = parser.GetBoolean("ctap2canonical", false);
        }

        /// <summary>Gets the values of this options object's properties in
        /// text form.</summary>
        /// <returns>A text string containing the values of this options
        /// object's properties. The format of the string is the same as the
        /// one described in the String constructor for this class.</returns>
        public override string ToString()
        {
            return new System.Text.StringBuilder()
              .Append("allowduplicatekeys=")
              .Append(this.AllowDuplicateKeys ? "true" : "false")
              .Append(";useindeflengthstrings=")
              .Append(this.UseIndefLengthStrings ? "true" : "false")
              .Append(";ctap2canonical=")
              .Append(this.Ctap2Canonical ? "true" : "false")
              .Append(";resolvereferences=")
              .Append(this.ResolveReferences ? "true" : "false")
              .Append(";allowempty=").Append(this.AllowEmpty ? "true" : "false")
              .ToString();
        }

        /// <summary>Gets a value indicating whether to resolve references to
        /// sharable objects and sharable strings in the process of decoding a
        /// CBOR object. Enabling this property, however, can cause a security
        /// risk if a decoded CBOR object is then re-encoded.</summary>
        /// <value>A value indicating whether to resolve references to sharable
        /// objects and sharable strings. The default is false.</value>
        /// <remarks>
        /// <para><b>About sharable objects and references</b></para>
        /// <para>Sharable objects are marked with tag 28, and references to
        /// those objects are marked with tag 29 (where a reference of 0 means
        /// the first sharable object in the CBOR stream, a reference of 1
        /// means the second, and so on). Sharable strings (byte strings and
        /// text strings) appear within an enclosing object marked with tag
        /// 256, and references to them are marked with tag 25; in general, a
        /// string is sharable only if storing its reference rather than the
        /// string would save space.</para>
        /// <para>Note that unlike most other tags, these tags generally care
        /// about the relative order in which objects appear in a CBOR stream;
        /// thus they are not interoperable with CBOR implementations that
        /// follow the generic CBOR data model (since they may list map keys in
        /// an unspecified order). Interoperability problems with these tags
        /// can be reduced by not using them to mark keys or values of a map or
        /// to mark objects within those keys or values.</para>
        /// <para><b>Security Note</b></para>
        /// <para>When this property is enabled and a decoded CBOR object
        /// contains references to sharable CBOR objects within it, those
        /// references will be replaced with the sharable objects they refer to
        /// (but without making a copy of those objects). However, if shared
        /// references are deeply nested and used multiple times, these
        /// references can result in a CBOR object that is orders of magnitude
        /// bigger than if shared references weren't resolved, and this can
        /// cause a denial of service when the decoded CBOR object is then
        /// serialized (e.g., with <c>EncodeToBytes()</c>, <c>ToString()</c>,
        /// <c>ToJSONString()</c>, or <c>WriteTo</c> ), because object
        /// references are expanded in the process.</para>
        /// <para>For example, the following object in CBOR diagnostic
        /// notation, <c>[28(["xxx", "yyy"]), 28([29(0), 29(0), 29(0)]),
        /// 28([29(1), 29(1)]), 28([29(2), 29(2)]), 28([29(3), 29(3)]),
        /// 28([29(4), 29(4)]), 28([29(5), 29(5)])]</c>, expands to a CBOR
        /// object with a serialized size of about 1831 bytes when this
        /// property is enabled, as opposed to about 69 bytes when this
        /// property is disabled.</para>
        /// <para>One way to mitigate security issues with this property is to
        /// limit the maximum supported size a CBORObject can have once
        /// serialized to CBOR or JSON. This can be done by passing a so-called
        /// "limited memory stream" to the <c>WriteTo</c> or <c>WriteJSONTo</c>
        /// methods when serializing the object to JSON or CBOR. A "limited
        /// memory stream" is a <c>Stream</c> (or <c>OutputStream</c> in Java)
        /// that throws an exception if it would write more bytes than a given
        /// maximum size or would seek past that size. (See the documentation
        /// for <c>CBORObject.WriteTo</c> or <c>CBORObject.WriteJSONTo</c> for
        /// example code.) Another mitigation is to check the CBOR object's
        /// type before serializing it, since only arrays and maps can have the
        /// security problem described here, or to check the maximum nesting
        /// depth of a CBOR array or map before serializing
        /// it.</para></remarks>
        public bool ResolveReferences
        {
            get;
            private set;
        }

        /// <summary>Gets a value indicating whether to encode strings with an
        /// indefinite-length encoding under certain circumstances.</summary>
        /// <value>A value indicating whether to encode strings with an
        /// indefinite-length encoding under certain circumstances. The default
        /// is false.</value>
        public bool UseIndefLengthStrings
        {
            get;
            private set;
        }

        /// <summary>Gets a value indicating whether decoding a CBOR object
        /// will return <c>null</c> instead of a CBOR object if the stream has
        /// no content or the end of the stream is reached before decoding
        /// begins. Used only when decoding CBOR objects.</summary>
        /// <value>A value indicating whether decoding a CBOR object will
        /// return <c>null</c> instead of a CBOR object if the stream has no
        /// content or the end of the stream is reached before decoding begins.
        /// The default is false.</value>
        public bool AllowEmpty
        {
            get;
            private set;
        }

        /// <summary>Gets a value indicating whether to allow duplicate keys
        /// when reading CBOR objects from a data stream. Used only when
        /// decoding CBOR objects. If this property is <c>true</c> and a CBOR
        /// map has two or more values with the same key, the last value of
        /// that key set forth in the CBOR map is taken.</summary>
        /// <value>A value indicating whether to allow duplicate keys when
        /// reading CBOR objects from a data stream. The default is
        /// false.</value>
        public bool AllowDuplicateKeys
        {
            get;
            private set;
        }

        /// <summary>Gets a value indicating whether CBOR objects:
        /// <list>
        /// <item>When encoding, are written out using the CTAP2 canonical CBOR
        /// encoding form, which is useful for implementing Web
        /// Authentication.</item>
        /// <item>When decoding, are checked for compliance with the CTAP2
        /// canonical encoding form.</item></list> In this form, CBOR tags are
        /// not used, map keys are written out in a canonical order, a maximum
        /// depth of four levels of arrays and/or maps is allowed, duplicate
        /// map keys are not allowed when decoding, and floating-point numbers
        /// are written out in their 64-bit encoding form regardless of whether
        /// their value can be encoded without loss in a smaller form. This
        /// implementation allows CBOR objects whose canonical form exceeds
        /// 1024 bytes, the default maximum size for CBOR objects in that form
        /// according to the FIDO Client-to-Authenticator Protocol 2
        /// specification.</summary>
        /// <value><c>true</c> if CBOR objects are written out using the CTAP2
        /// canonical CBOR encoding form; otherwise, <c>false</c>. The default
        /// is <c>false</c>.</value>
        public bool Ctap2Canonical
        {
            get;
            private set;
        }
    }

    #endregion

    #region CBORException
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:CBORException"]/*'/>
    #if NET20 || NET40
    [Serializable]
    #endif
    public sealed class CBORException : Exception
    {
        /// <summary>Initializes a new instance of the
        /// <see cref='PeterO.Cbor.CBORException'/> class.</summary>
        public CBORException()
        {
        }

        /// <summary>Initializes a new instance of the
        /// <see cref='PeterO.Cbor.CBORException'/> class.</summary>
        /// <param name='message'>The parameter <paramref name='message'/> is a
        /// text string.</param>
        public CBORException(string message) : base(message)
        {
        }

        /// <summary>Initializes a new instance of the
        /// <see cref='PeterO.Cbor.CBORException'/> class. Uses the given
        /// message and inner exception.</summary>
        /// <param name='message'>The parameter <paramref name='message'/> is a
        /// text string.</param>
        /// <param name='innerException'>The parameter <paramref
        /// name='innerException'/> is an Exception object.</param>
        public CBORException(string message, Exception innerException)
          : base(message, innerException)
        {
        }

#if NET20 || NET40
    private CBORException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context)
      : base(info, context) {
    }
#endif
    }

    #endregion

    #region CBORExtendedDecimal
    internal class CBORExtendedDecimal : ICBORNumber
    {
        public bool IsPositiveInfinity(object obj)
        {
            var ed = (EDecimal)obj;
            return ed.IsPositiveInfinity();
        }

        public bool IsInfinity(object obj)
        {
            var ed = (EDecimal)obj;
            return ed.IsInfinity();
        }

        public bool IsNegativeInfinity(object obj)
        {
            var ed = (EDecimal)obj;
            return ed.IsNegativeInfinity();
        }

        public bool IsNaN(object obj)
        {
            var ed = (EDecimal)obj;
            return ed.IsNaN();
        }

        public double AsDouble(object obj)
        {
            var ed = (EDecimal)obj;
            return ed.ToDouble();
        }

        public EDecimal AsEDecimal(object obj)
        {
            var ed = (EDecimal)obj;
            return ed;
        }

        public EFloat AsEFloat(object obj)
        {
            var ed = (EDecimal)obj;
            return ed.ToEFloat();
        }

        public float AsSingle(object obj)
        {
            var ed = (EDecimal)obj;
            return ed.ToSingle();
        }

        public EInteger AsEInteger(object obj)
        {
            var ed = (EDecimal)obj;
            return ed.ToEInteger();
        }

        public long AsInt64(object obj)
        {
            var ef = (EDecimal)obj;
            if (this.CanTruncatedIntFitInInt64(obj))
            {
                EInteger bi = ef.ToEInteger();
                return (long)bi;
            }
            throw new OverflowException("This object's value is out of range");
        }

        public bool CanFitInSingle(object obj)
        {
            var ef = (EDecimal)obj;
            return (!ef.IsFinite) || (ef.CompareTo(EDecimal.FromSingle(
                  ef.ToSingle())) == 0);
        }

        public bool CanFitInDouble(object obj)
        {
            var ef = (EDecimal)obj;
            return (!ef.IsFinite) || (ef.CompareTo(EDecimal.FromDouble(
                  ef.ToDouble())) == 0);
        }

        public bool CanFitInInt32(object obj)
        {
            return this.IsIntegral(obj) && this.CanTruncatedIntFitInInt32(obj);
        }

        public bool CanFitInInt64(object obj)
        {
            return this.IsIntegral(obj) && this.CanTruncatedIntFitInInt64(obj);
        }

        public bool CanTruncatedIntFitInInt64(object obj)
        {
            var ef = (EDecimal)obj;
            if (!ef.IsFinite)
            {
                return false;
            }
            if (ef.IsZero)
            {
                return true;
            }
            if (ef.Exponent.CompareTo((EInteger)21) >= 0)
            {
                return false;
            }
            EInteger bi = ef.ToEInteger();
            return bi.CanFitInInt64();
        }

        public bool CanTruncatedIntFitInInt32(object obj)
        {
            var ef = (EDecimal)obj;
            if (!ef.IsFinite)
            {
                return false;
            }
            if (ef.IsZero)
            {
                return true;
            }
            if (ef.Exponent.CompareTo((EInteger)11) >= 0)
            {
                return false;
            }
            EInteger bi = ef.ToEInteger();
            return bi.CanFitInInt32();
        }

        public bool IsNumberZero(object obj)
        {
            var ed = (EDecimal)obj;
            return ed.IsZero;
        }

        public int Sign(object obj)
        {
            var ed = (EDecimal)obj;
            return ed.IsNaN() ? 2 : ed.Sign;
        }

        public bool IsIntegral(object obj)
        {
            var ed = (EDecimal)obj;
            return ed.IsFinite && ((ed.Exponent.Sign >= 0) ||
      (ed.CompareTo(EDecimal.FromEInteger(ed.ToEInteger())) ==

                  0));
        }

        public int AsInt32(object obj, int minValue, int maxValue)
        {
            var ef = (EDecimal)obj;
            if (this.CanTruncatedIntFitInInt32(obj))
            {
                EInteger bi = ef.ToEInteger();
                var ret = (int)bi;
                if (ret >= minValue && ret <= maxValue)
                {
                    return ret;
                }
            }
            throw new OverflowException("This object's value is out of range");
        }

        public object Negate(object obj)
        {
            var ed = (EDecimal)obj;
            return ed.Negate();
        }

        public object Abs(object obj)
        {
            var ed = (EDecimal)obj;
            return ed.Abs();
        }

        public ERational AsERational(object obj)
        {
            return ERational.FromEDecimal((EDecimal)obj);
        }

        public bool IsNegative(object obj)
        {
            return ((EDecimal)obj).IsNegative;
        }
    }
    #endregion

    #region CBORExtendedFloat
    internal class CBORExtendedFloat : ICBORNumber
    {
        public bool IsPositiveInfinity(object obj)
        {
            var ef = (EFloat)obj;
            return ef.IsPositiveInfinity();
        }

        public bool IsInfinity(object obj)
        {
            var ef = (EFloat)obj;
            return ef.IsInfinity();
        }

        public bool IsNegativeInfinity(object obj)
        {
            var ef = (EFloat)obj;
            return ef.IsNegativeInfinity();
        }

        public bool IsNaN(object obj)
        {
            var ef = (EFloat)obj;
            return ef.IsNaN();
        }

        public double AsDouble(object obj)
        {
            var ef = (EFloat)obj;
            return ef.ToDouble();
        }

        public EDecimal AsEDecimal(object obj)
        {
            var ef = (EFloat)obj;
            return ef.ToEDecimal();
        }

        public EFloat AsEFloat(object obj)
        {
            var ef = (EFloat)obj;
            return ef;
        }

        public float AsSingle(object obj)
        {
            var ef = (EFloat)obj;
            return ef.ToSingle();
        }

        public EInteger AsEInteger(object obj)
        {
            var ef = (EFloat)obj;
            return ef.ToEInteger();
        }

        public long AsInt64(object obj)
        {
            var ef = (EFloat)obj;
            if (this.CanTruncatedIntFitInInt64(obj))
            {
                EInteger bi = ef.ToEInteger();
                return (long)bi;
            }
            throw new OverflowException("This object's value is out of range");
        }

        public bool CanFitInSingle(object obj)
        {
            var ef = (EFloat)obj;
            return (!ef.IsFinite) || (ef.CompareTo(EFloat.FromSingle(
                  ef.ToSingle())) == 0);
        }

        public bool CanFitInDouble(object obj)
        {
            var ef = (EFloat)obj;
            return (!ef.IsFinite) || (ef.CompareTo(EFloat.FromDouble(
                  ef.ToDouble())) == 0);
        }

        public bool CanFitInInt32(object obj)
        {
            return this.IsIntegral(obj) && this.CanTruncatedIntFitInInt32(obj);
        }

        public bool CanFitInInt64(object obj)
        {
            return this.IsIntegral(obj) && this.CanTruncatedIntFitInInt64(obj);
        }

        public bool CanTruncatedIntFitInInt64(object obj)
        {
            var ef = (EFloat)obj;
            if (!ef.IsFinite)
            {
                return false;
            }
            if (ef.IsZero)
            {
                return true;
            }
            if (ef.Exponent.CompareTo((EInteger)65) >= 0)
            {
                return false;
            }
            EInteger bi = ef.ToEInteger();
            return bi.CanFitInInt64();
        }

        public bool CanTruncatedIntFitInInt32(object obj)
        {
            var ef = (EFloat)obj;
            if (!ef.IsFinite)
            {
                return false;
            }
            if (ef.IsZero)
            {
                return true;
            }
            if (ef.Exponent.CompareTo((EInteger)33) >= 0)
            {
                return false;
            }
            EInteger bi = ef.ToEInteger();
            return bi.CanFitInInt32();
        }

        public bool IsNumberZero(object obj)
        {
            var ef = (EFloat)obj;
            return ef.IsZero;
        }

        public int Sign(object obj)
        {
            var ef = (EFloat)obj;
            return ef.IsNaN() ? 2 : ef.Sign;
        }

        public bool IsIntegral(object obj)
        {
            var ef = (EFloat)obj;
            if (!ef.IsFinite)
            {
                return false;
            }
            if (ef.Exponent.Sign >= 0)
            {
                return true;
            }
            EFloat ef2 = EFloat.FromEInteger(ef.ToEInteger());
            return ef2.CompareTo(ef) == 0;
        }

        public int AsInt32(object obj, int minValue, int maxValue)
        {
            var ef = (EFloat)obj;
            if (this.CanTruncatedIntFitInInt32(obj))
            {
                EInteger bi = ef.ToEInteger();
                var ret = (int)bi;
                if (ret >= minValue && ret <= maxValue)
                {
                    return ret;
                }
            }
            throw new OverflowException("This object's value is out of range");
        }

        public object Negate(object obj)
        {
            var ed = (EFloat)obj;
            return ed.Negate();
        }

        public object Abs(object obj)
        {
            var ed = (EFloat)obj;
            return ed.Abs();
        }

        public ERational AsERational(object obj)
        {
            return ERational.FromEFloat((EFloat)obj);
        }

        public bool IsNegative(object obj)
        {
            return ((EFloat)obj).IsNegative;
        }
    }
    #endregion

    #region CBORExtendedRational
    internal class CBORExtendedRational : ICBORNumber
    {
        public bool IsPositiveInfinity(object obj)
        {
            return ((ERational)obj).IsPositiveInfinity();
        }

        public bool IsInfinity(object obj)
        {
            return ((ERational)obj).IsInfinity();
        }

        public bool IsNegativeInfinity(object obj)
        {
            return ((ERational)obj).IsNegativeInfinity();
        }

        public bool IsNaN(object obj)
        {
            return ((ERational)obj).IsNaN();
        }

        public double AsDouble(object obj)
        {
            var er = (ERational)obj;
            return er.ToDouble();
        }

        public EDecimal AsEDecimal(object obj)
        {
            var er = (ERational)obj;
            return

              er.ToEDecimalExactIfPossible(
                EContext.Decimal128.WithUnlimitedExponents());
        }

        public EFloat AsEFloat(object obj)
        {
            var er = (ERational)obj;
            return

              er.ToEFloatExactIfPossible(
                EContext.Binary128.WithUnlimitedExponents());
        }

        public float AsSingle(object obj)
        {
            var er = (ERational)obj;
            return er.ToSingle();
        }

        public EInteger AsEInteger(object obj)
        {
            var er = (ERational)obj;
            return er.ToEInteger();
        }

        public long AsInt64(object obj)
        {
            var ef = (ERational)obj;
            if (ef.IsFinite)
            {
                EInteger bi = ef.ToEInteger();
                if (bi.CanFitInInt64())
                {
                    return (long)bi;
                }
            }
            throw new OverflowException("This object's value is out of range");
        }

        public bool CanFitInSingle(object obj)
        {
            var ef = (ERational)obj;
            return (!ef.IsFinite) || (ef.CompareTo(ERational.FromSingle(
                  ef.ToSingle())) == 0);
        }

        public bool CanFitInDouble(object obj)
        {
            var ef = (ERational)obj;
            return (!ef.IsFinite) || (ef.CompareTo(ERational.FromDouble(
                  ef.ToDouble())) == 0);
        }

        public bool CanFitInInt32(object obj)
        {
            return this.IsIntegral(obj) && this.CanTruncatedIntFitInInt32(obj);
        }

        public bool CanFitInInt64(object obj)
        {
            return this.IsIntegral(obj) && this.CanTruncatedIntFitInInt64(obj);
        }

        public bool CanTruncatedIntFitInInt64(object obj)
        {
            var ef = (ERational)obj;
            if (!ef.IsFinite)
            {
                return false;
            }
            EInteger bi = ef.ToEInteger();
            return bi.CanFitInInt64();
        }

        public bool CanTruncatedIntFitInInt32(object obj)
        {
            var ef = (ERational)obj;
            if (!ef.IsFinite)
            {
                return false;
            }
            EInteger bi = ef.ToEInteger();
            return bi.CanFitInInt32();
        }

        public bool IsNumberZero(object obj)
        {
            var ef = (ERational)obj;
            return ef.IsZero;
        }

        public int Sign(object obj)
        {
            var ef = (ERational)obj;
            return ef.Sign;
        }

        public bool IsIntegral(object obj)
        {
            var ef = (ERational)obj;
            if (!ef.IsFinite)
            {
                return false;
            }
            if (ef.Denominator.Equals(EInteger.One))
            {
                return true;
            }
            // A rational number is integral if the remainder
            // of the numerator divided by the denominator is 0
            EInteger denom = ef.Denominator;
            EInteger rem = ef.Numerator % (EInteger)denom;
            return rem.IsZero;
        }

        public int AsInt32(object obj, int minValue, int maxValue)
        {
            var ef = (ERational)obj;
            if (ef.IsFinite)
            {
                EInteger bi = ef.ToEInteger();
                if (bi.CanFitInInt32())
                {
                    var ret = (int)bi;
                    if (ret >= minValue && ret <= maxValue)
                    {
                        return ret;
                    }
                }
            }
            throw new OverflowException("This object's value is out of range");
        }

        public object Negate(object obj)
        {
            var ed = (ERational)obj;
            return ed.Negate();
        }

        public object Abs(object obj)
        {
            var ed = (ERational)obj;
            return ed.Abs();
        }

        public ERational AsERational(object obj)
        {
            return (ERational)obj;
        }

        public bool IsNegative(object obj)
        {
            return ((ERational)obj).IsNegative;
        }
    }
    #endregion

    #region CBORInteger
    internal class CBORInteger : ICBORNumber
    {
        public object Abs(object obj)
        {
            var val = (long)obj;
            return (val == Int32.MinValue) ? (EInteger.One << 63) : ((val < 0) ?
                -val : obj);
        }

        public EInteger AsEInteger(object obj)
        {
            return (EInteger)(long)obj;
        }

        public double AsDouble(object obj)
        {
            return (double)(long)obj;
        }

        public EDecimal AsEDecimal(object obj)
        {
            return EDecimal.FromInt64((long)obj);
        }

        public EFloat AsEFloat(object obj)
        {
            return EFloat.FromInt64((long)obj);
        }

        public ERational AsERational(object obj)
        {
            return ERational.FromInt64((long)obj);
        }

        public int AsInt32(object obj, int minValue, int maxValue)
        {
            var val = (long)obj;
            if (val >= minValue && val <= maxValue)
            {
                return (int)val;
            }
            throw new OverflowException("This object's value is out of range");
        }

        public long AsInt64(object obj)
        {
            return (long)obj;
        }

        public float AsSingle(object obj)
        {
            return (float)(long)obj;
        }

        public bool CanFitInDouble(object obj)
        {
            var intItem = (long)obj;
            if (intItem == Int64.MinValue)
            {
                return true;
            }
            intItem = (intItem < 0) ? -intItem : intItem;
            while (intItem >= (1L << 53) && (intItem & 1) == 0)
            {
                intItem >>= 1;
            }
            return intItem < (1L << 53);
        }

        public bool CanFitInInt32(object obj)
        {
            var val = (long)obj;
            return val >= Int32.MinValue && val <= Int32.MaxValue;
        }

        public bool CanFitInInt64(object obj)
        {
            return true;
        }

        public bool CanFitInSingle(object obj)
        {
            var intItem = (long)obj;
            if (intItem == Int64.MinValue)
            {
                return true;
            }
            intItem = (intItem < 0) ? -intItem : intItem;
            while (intItem >= (1L << 24) && (intItem & 1) == 0)
            {
                intItem >>= 1;
            }
            return intItem < (1L << 24);
        }

        public bool CanTruncatedIntFitInInt32(object obj)
        {
            var val = (long)obj;
            return val >= Int32.MinValue && val <= Int32.MaxValue;
        }

        public bool CanTruncatedIntFitInInt64(object obj)
        {
            return true;
        }

        public bool IsInfinity(object obj)
        {
            return false;
        }

        public bool IsIntegral(object obj)
        {
            return true;
        }

        public bool IsNaN(object obj)
        {
            return false;
        }

        public bool IsNegative(object obj)
        {
            return ((long)obj) < 0;
        }

        public bool IsNegativeInfinity(object obj)
        {
            return false;
        }

        public bool IsPositiveInfinity(object obj)
        {
            return false;
        }

        public bool IsNumberZero(object obj)
        {
            return ((long)obj) == 0;
        }

        public object Negate(object obj)
        {
            return (((long)obj) == Int64.MinValue) ? (EInteger.One << 63) :
      (-((long)obj));
        }

        public int Sign(object obj)
        {
            var val = (long)obj;
            return (val == 0) ? 0 : ((val < 0) ? -1 : 1);
        }
    }
    #endregion

    #region CBORJson
    internal sealed class CBORJson
    {
        // JSON parsing methods
        private int SkipWhitespaceJSON()
        {
            while (true)
            {
                int c = this.ReadChar();
                if (c == -1 || (c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09))
                {
                    return c;
                }
            }
        }

        // JSON parsing methods
        private int SkipWhitespaceJSON(int lastChar)
        {
            while (lastChar == 0x20 || lastChar == 0x0a || lastChar == 0x0d ||
              lastChar == 0x09)
            {
                lastChar = this.ReadChar();
            }
            return lastChar;
        }

        public void SkipToEnd()
        {
            if (this.jsonSequenceMode)
            {
                while (this.ReadChar() >= 0)
                {
                    // Loop
                }
            }
        }

        public int ReadChar()
        {
            if (this.jsonSequenceMode)
            {
                if (this.recordSeparatorSeen)
                {
                    return -1;
                }
                int rc = this.reader.ReadChar();
                if (rc == 0x1e)
                {
                    this.recordSeparatorSeen = true;
                    return -1;
                }
                return rc;
            }
            else
            {
                return this.reader.ReadChar();
            }
        }

        private void RaiseError(string str)
        {
            this.reader.RaiseError(str);
        }

        private readonly JSONOptions options;
        private CharacterInputWithCount reader;
        private StringBuilder sb;
        private bool jsonSequenceMode;
        private bool recordSeparatorSeen;

        private string NextJSONString()
        {
            int c;
            this.sb = this.sb ?? new StringBuilder();
            this.sb.Remove(0, this.sb.Length);
            while (true)
            {
                c = this.ReadChar();
                if (c == -1 || c < 0x20)
                {
                    this.RaiseError("Unterminated string");
                }
                switch (c)
                {
                    case '\\':
                        c = this.ReadChar();
                        switch (c)
                        {
                            case '\\':
                            case '/':
                            case '\"':
                                // Slash is now allowed to be escaped under RFC 8259
                                this.sb.Append((char)c);
                                break;
                            case 'b':
                                this.sb.Append('\b');
                                break;
                            case 'f':
                                this.sb.Append('\f');
                                break;
                            case 'n':
                                this.sb.Append('\n');
                                break;
                            case 'r':
                                this.sb.Append('\r');
                                break;
                            case 't':
                                this.sb.Append('\t');
                                break;
                            case 'u':
                                { // Unicode escape
                                    c = 0;
                                    // Consists of 4 hex digits
                                    for (var i = 0; i < 4; ++i)
                                    {
                                        int ch = this.ReadChar();
                                        if (ch >= '0' && ch <= '9')
                                        {
                                            c <<= 4;
                                            c |= ch - '0';
                                        }
                                        else if (ch >= 'A' && ch <= 'F')
                                        {
                                            c <<= 4;
                                            c |= ch + 10 - 'A';
                                        }
                                        else if (ch >= 'a' && ch <= 'f')
                                        {
                                            c <<= 4;
                                            c |= ch + 10 - 'a';
                                        }
                                        else
                                        {
                                            this.RaiseError(
                                              "Invalid Unicode escaped character");
                                        }
                                    }
                                    if ((c & 0xf800) != 0xd800)
                                    {
                                        // Non-surrogate
                                        this.sb.Append((char)c);
                                    }
                                    else if ((c & 0xfc00) == 0xd800)
                                    {
                                        int ch = this.ReadChar();
                                        if (ch != '\\' || this.ReadChar() != 'u')
                                        {
                                            this.RaiseError("Invalid escaped character");
                                        }
                                        var c2 = 0;
                                        for (var i = 0; i < 4; ++i)
                                        {
                                            ch = this.ReadChar();
                                            if (ch >= '0' && ch <= '9')
                                            {
                                                c2 <<= 4;
                                                c2 |= ch - '0';
                                            }
                                            else if (ch >= 'A' && ch <= 'F')
                                            {
                                                c2 <<= 4;
                                                c2 |= ch + 10 - 'A';
                                            }
                                            else if (ch >= 'a' && ch <= 'f')
                                            {
                                                c2 <<= 4;
                                                c2 |= ch + 10 - 'a';
                                            }
                                            else
                                            {
                                                this.RaiseError(
                                                  "Invalid Unicode escaped character");
                                            }
                                        }
                                        if ((c2 & 0xfc00) != 0xdc00)
                                        {
                                            this.RaiseError("Unpaired surrogate code point");
                                        }
                                        else
                                        {
                                            this.sb.Append((char)c);
                                            this.sb.Append((char)c2);
                                        }
                                    }
                                    else
                                    {
                                        this.RaiseError("Unpaired surrogate code point");
                                    }
                                    break;
                                }
                            default:
                                {
                                    this.RaiseError("Invalid escaped character");
                                    break;
                                }
                        }
                        break;
                    case 0x22: // double quote
                        return this.sb.ToString();
                    default:
                        {
                            // NOTE: Assumes the character reader
                            // throws an error on finding illegal surrogate
                            // pairs in the string or invalid encoding
                            // in the stream
                            if ((c >> 16) == 0)
                            {
                                this.sb.Append((char)c);
                            }
                            else
                            {
                                this.sb.Append((char)((((c - 0x10000) >> 10) & 0x3ff) |
                                    0xd800));
                                this.sb.Append((char)(((c - 0x10000) & 0x3ff) | 0xdc00));
                            }
                            break;
                        }
                }
            }
        }

        private CBORObject NextJSONNegativeNumber(
          int[] nextChar,
          int depth)
        {
            string str;
            CBORObject obj;
            int c = this.ReadChar();
            if (c < '0' || c > '9')
            {
                this.RaiseError("JSON number can't be parsed.");
            }
            int cval = -(c - '0');
            int cstart = c;
            c = this.ReadChar();
            this.sb = this.sb ?? new StringBuilder();
            this.sb.Remove(0, this.sb.Length);
            this.sb.Append('-');
            this.sb.Append((char)cstart);
            var charbuf = new char[32];
            var charbufptr = 0;
            while (c == '-' || c == '+' || c == '.' || (c >= '0' && c <= '9') ||
              c == 'e' || c == 'E')
            {
                charbuf[charbufptr++] = (char)c;
                if (charbufptr >= 32)
                {
                    this.sb.Append(charbuf, 0, 32);
                    charbufptr = 0;
                }
                c = this.ReadChar();
            }
            if (charbufptr > 0)
            {
                this.sb.Append(charbuf, 0, charbufptr);
            }
            // DebugUtility.Log("--nega=" + sw.ElapsedMilliseconds + " ms");
            // check if character can validly appear after a JSON number
            if (c != ',' && c != ']' && c != '}' && c != -1 &&
              c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09)
            {
                this.RaiseError("Invalid character after JSON number");
            }
            str = this.sb.ToString();
            // DebugUtility.Log("negb=" + sw.ElapsedMilliseconds + " ms");
            obj = CBORDataUtilities.ParseJSONNumber(str, this.options);
            // DebugUtility.Log("negc=" + sw.ElapsedMilliseconds + " ms");
            if (obj == null)
            {
                string errstr = (str.Length <= 100) ? str : (str.Substring(0,
                      100) + "...");
                this.RaiseError("JSON number can't be parsed. " + errstr);
            }
            if (c == 0x20 || c == 0x0a || c == 0x0d || c == 0x09)
            {
                nextChar[0] = this.SkipWhitespaceJSON();
            }
            else if (this.jsonSequenceMode && depth == 0)
            {
                nextChar[0] = c;
                this.RaiseError("JSON whitespace expected after top-level " +
                  "number in JSON sequence");
            }
            else
            {
                nextChar[0] = c;
            }
            return obj;
        }

        private CBORObject NextJSONValue(
          int firstChar,
          int[] nextChar,
          int depth)
        {
            string str;
            int c = firstChar;
            CBORObject obj = null;
            if (c < 0)
            {
                this.RaiseError("Unexpected end of data");
            }
            switch (c)
            {
                case '"':
                    {
                        // Parse a string
                        // The tokenizer already checked the string for invalid
                        // surrogate pairs, so just call the CBORObject
                        // constructor directly
                        obj = CBORObject.FromRaw(this.NextJSONString());
                        nextChar[0] = this.SkipWhitespaceJSON();
                        return obj;
                    }
                case '{':
                    {
                        // Parse an object
                        obj = this.ParseJSONObject(depth + 1);
                        nextChar[0] = this.SkipWhitespaceJSON();
                        return obj;
                    }
                case '[':
                    {
                        // Parse an array
                        obj = this.ParseJSONArray(depth + 1);
                        nextChar[0] = this.SkipWhitespaceJSON();
                        return obj;
                    }
                case 't':
                    {
                        // Parse true
                        if ((c = this.ReadChar()) != 'r' || (c = this.ReadChar()) != 'u' ||
                          (c = this.ReadChar()) != 'e')
                        {
                            this.RaiseError("Value can't be parsed.");
                        }
                        c = this.ReadChar();
                        if (c == 0x20 || c == 0x0a || c == 0x0d || c == 0x09)
                        {
                            nextChar[0] = this.SkipWhitespaceJSON();
                        }
                        else if (this.jsonSequenceMode && depth == 0)
                        {
                            nextChar[0] = c;
                            this.RaiseError("JSON whitespace expected after top-level " +
                              "number in JSON sequence");
                        }
                        else
                        {
                            nextChar[0] = c;
                        }
                        return CBORObject.True;
                    }
                case 'f':
                    {
                        // Parse false
                        if ((c = this.ReadChar()) != 'a' || (c = this.ReadChar()) != 'l' ||
                          (c = this.ReadChar()) != 's' || (c = this.ReadChar()) != 'e')
                        {
                            this.RaiseError("Value can't be parsed.");
                        }
                        c = this.ReadChar();
                        if (c == 0x20 || c == 0x0a || c == 0x0d || c == 0x09)
                        {
                            nextChar[0] = this.SkipWhitespaceJSON();
                        }
                        else if (this.jsonSequenceMode && depth == 0)
                        {
                            nextChar[0] = c;
                            this.RaiseError("JSON whitespace expected after top-level " +
                              "number in JSON sequence");
                        }
                        else
                        {
                            nextChar[0] = c;
                        }
                        return CBORObject.False;
                    }
                case 'n':
                    {
                        // Parse null
                        if ((c = this.ReadChar()) != 'u' || (c = this.ReadChar()) != 'l' ||
                          (c = this.ReadChar()) != 'l')
                        {
                            this.RaiseError("Value can't be parsed.");
                        }
                        c = this.ReadChar();
                        if (c == 0x20 || c == 0x0a || c == 0x0d || c == 0x09)
                        {
                            nextChar[0] = this.SkipWhitespaceJSON();
                        }
                        else if (this.jsonSequenceMode && depth == 0)
                        {
                            nextChar[0] = c;
                            this.RaiseError("JSON whitespace expected after top-level " +
                              "number in JSON sequence");
                        }
                        else
                        {
                            nextChar[0] = c;
                        }
                        return CBORObject.Null;
                    }
                case '-':
                    {
                        // Parse a negative number
                        return this.NextJSONNegativeNumber(nextChar, depth);
                    }
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    {
                        // Parse a nonnegative number
                        int cval = c - '0';
                        int cstart = c;
                        var needObj = true;
                        c = this.ReadChar();
                        if (!(c == '-' || c == '+' || c == '.' || (c >= '0' && c <= '9') ||
                            c == 'e' || c == 'E'))
                        {
                            // Optimize for common case where JSON number
                            // is a single digit without sign or exponent
                            obj = CBORDataUtilities.ParseSmallNumber(cval, this.options);
                            needObj = false;
                        }
                        else if (c >= '0' && c <= '9')
                        {
                            int csecond = c;
                            if (cstart == '0')
                            {
                                // Leading zero followed by any digit is not allowed
                                this.RaiseError("JSON number can't be parsed.");
                            }
                            cval = (cval * 10) + (int)(c - '0');
                            c = this.ReadChar();
                            if (c >= '0' && c <= '9')
                            {
                                var digits = 2;
                                var ctmp = new int[10];
                                ctmp[0] = cstart;
                                ctmp[1] = csecond;
                                while (digits < 9 && (c >= '0' && c <= '9'))
                                {
                                    cval = (cval * 10) + (int)(c - '0');
                                    ctmp[digits++] = c;
                                    c = this.ReadChar();
                                }
                                if (c == 'e' || c == 'E' || c == '.' || (c >= '0' && c <= '9'))
                                {
                                    // Not an all-digit number, or too long
                                    this.sb = this.sb ?? new StringBuilder();
                                    this.sb.Remove(0, this.sb.Length);
                                    for (var vi = 0; vi < digits; ++vi)
                                    {
                                        this.sb.Append((char)ctmp[vi]);
                                    }
                                }
                                else
                                {
                                    obj = CBORDataUtilities.ParseSmallNumber(cval, this.options);
                                    needObj = false;
                                }
                            }
                            else if (!(c == '-' || c == '+' || c == '.' || c == 'e' || c
                              == 'E'))
                            {
                                // Optimize for common case where JSON number
                                // is two digits without sign, decimal point, or exponent
                                obj = CBORDataUtilities.ParseSmallNumber(cval, this.options);
                                needObj = false;
                            }
                            else
                            {
                                this.sb = this.sb ?? new StringBuilder();
                                this.sb.Remove(0, this.sb.Length);
                                this.sb.Append((char)cstart);
                                this.sb.Append((char)csecond);
                            }
                        }
                        else
                        {
                            this.sb = this.sb ?? new StringBuilder();
                            this.sb.Remove(0, this.sb.Length);
                            this.sb.Append((char)cstart);
                        }
                        if (needObj)
                        {
                            var charbuf = new char[32];
                            var charbufptr = 0;
                            while (
                              c == '-' || c == '+' || c == '.' || (c >= '0' && c <= '9') ||
                              c == 'e' || c == 'E')
                            {
                                charbuf[charbufptr++] = (char)c;
                                if (charbufptr >= 32)
                                {
                                    this.sb.Append(charbuf, 0, 32);
                                    charbufptr = 0;
                                }
                                c = this.ReadChar();
                            }
                            if (charbufptr > 0)
                            {
                                this.sb.Append(charbuf, 0, charbufptr);
                            }
                            // check if character can validly appear after a JSON number
                            if (c != ',' && c != ']' && c != '}' && c != -1 &&
                              c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09)
                            {
                                this.RaiseError("Invalid character after JSON number");
                            }
                            str = this.sb.ToString();
                            obj = CBORDataUtilities.ParseJSONNumber(str, this.options);
                            if (obj == null)
                            {
                                string errstr = (str.Length <= 100) ? str : (str.Substring(0,
                                      100) + "...");
                                this.RaiseError("JSON number can't be parsed. " + errstr);
                            }
                        }
                        if (c == 0x20 || c == 0x0a || c == 0x0d || c == 0x09)
                        {
                            nextChar[0] = this.SkipWhitespaceJSON();
                        }
                        else if (this.jsonSequenceMode && depth == 0)
                        {
                            nextChar[0] = c;
                            this.RaiseError("JSON whitespace expected after top-level " +
                              "number in JSON sequence");
                        }
                        else
                        {
                            nextChar[0] = c;
                        }
                        return obj;
                    }
                default:
                    this.RaiseError("Value can't be parsed.");
                    break;
            }
            return null;
        }

        public CBORJson(CharacterInputWithCount reader, JSONOptions options)
        {
            this.reader = reader;
            this.sb = null;
            this.options = options;
            this.jsonSequenceMode = false;
            this.recordSeparatorSeen = false;
        }

        public CBORObject ParseJSON(int[] nextChar)
        {
            int c;
            CBORObject ret;
            c = this.jsonSequenceMode ? this.SkipWhitespaceJSON(nextChar[0]) :
              this.SkipWhitespaceJSON();
            if (c == '[')
            {
                ret = this.ParseJSONArray(0);
                nextChar[0] = this.SkipWhitespaceJSON();
                return ret;
            }
            if (c == '{')
            {
                ret = this.ParseJSONObject(0);
                nextChar[0] = this.SkipWhitespaceJSON();
                return ret;
            }
            return this.NextJSONValue(c, nextChar, 0);
        }

        private void SetJSONSequenceMode()
        {
            this.jsonSequenceMode = true;
            this.recordSeparatorSeen = false;
        }

        private void ResetJSONSequenceMode()
        {
            this.jsonSequenceMode = true;
            this.recordSeparatorSeen = false;
        }

        internal static CBORObject ParseJSONValue(
          CharacterInputWithCount reader,
          JSONOptions options,
          int[] nextChar)
        {
            var cj = new CBORJson(reader, options);
            return cj.ParseJSON(nextChar);
        }

        internal bool SkipRecordSeparators(int[] nextChar, bool
          recordSeparatorSeen)
        {
            if (this.jsonSequenceMode)
            {
                while (true)
                {
                    int rc = this.reader.ReadChar();
                    nextChar[0] = rc;
                    if (rc == 0x1e)
                    {
                        recordSeparatorSeen = true;
                    }
                    else
                    {
                        return recordSeparatorSeen;
                    }
                }
            }
            else
            {
                nextChar[0] = -1;
                return false;
            }
        }

        internal static CBORObject[] ParseJSONSequence(
          CharacterInputWithCount reader,
          JSONOptions options,
          int[] nextChar)
        {
            var cj = new CBORJson(reader, options);
            cj.SetJSONSequenceMode();
            bool seenSeparator = cj.SkipRecordSeparators(nextChar, false);
            if (nextChar[0] >= 0 && !seenSeparator)
            {
                // Stream is not empty and did not begin with
                // record separator
                cj.RaiseError("Not a JSON text sequence");
            }
            else if (nextChar[0] < 0 && !seenSeparator)
            {
                // Stream is empty
                return new CBORObject[0];
            }
            else if (nextChar[0] < 0)
            {
                // Stream had only record separators, so we found
                // a truncated JSON text
                return new CBORObject[] { null };
            }
            var list = new List<CBORObject>();
            while (true)
            {
                CBORObject co;
                try
                {
                    co = cj.ParseJSON(nextChar);
                }
                catch (CBORException)
                {
                    cj.SkipToEnd();
                    co = null;
                }
                if (co != null && nextChar[0] >= 0)
                {
                    // End of JSON text not reached
                    cj.SkipToEnd();
                    co = null;
                }
                list.Add(co);
                if (!cj.recordSeparatorSeen)
                {
                    // End of the stream was reached
                    nextChar[0] = -1;
                    break;
                }
                else
                {
                    // A record separator was seen, so
                    // another JSON text follows
                    cj.ResetJSONSequenceMode();
                    cj.SkipRecordSeparators(nextChar, true);
                    if (nextChar[0] < 0)
                    {
                        // Rest of stream had only record separators, so we found
                        // a truncated JSON text
                        list.Add(null);
                        break;
                    }
                }
            }
            return (CBORObject[])list.ToArray();
        }

        private CBORObject ParseJSONObject(int depth)
        {
            // Assumes that the last character read was '{'
            if (depth > 1000)
            {
                this.RaiseError("Too deeply nested");
            }
            int c;
            CBORObject key = null;
            CBORObject obj;
            var nextChar = new int[1];
            var seenComma = false;
            var myHashMap = new SortedDictionary<CBORObject, CBORObject>();
            while (true)
            {
                c = this.SkipWhitespaceJSON();
                switch (c)
                {
                    case -1:
                        this.RaiseError("A JSON object must end with '}'");
                        break;
                    case '}':
                        if (seenComma)
                        {
                            // Situation like '{"0"=>1,}'
                            this.RaiseError("Trailing comma");
                            return null;
                        }
                        return CBORObject.FromRaw(myHashMap);
                    default:
                        {
                            // Read the next string
                            if (c < 0)
                            {
                                this.RaiseError("Unexpected end of data");
                                return null;
                            }
                            if (c != '"')
                            {
                                this.RaiseError("Expected a string as a key");
                                return null;
                            }
                            // Parse a string that represents the object's key.
                            // The tokenizer already checked the string for invalid
                            // surrogate pairs, so just call the CBORObject
                            // constructor directly
                            obj = CBORObject.FromRaw(this.NextJSONString());
                            key = obj;
                            if (!this.options.AllowDuplicateKeys &&
                              myHashMap.ContainsKey(obj))
                            {
                                this.RaiseError("Key already exists: " + key);
                                return null;
                            }
                            break;
                        }
                }
                if (this.SkipWhitespaceJSON() != ':')
                {
                    this.RaiseError("Expected a ':' after a key");
                }
                // NOTE: Will overwrite existing value
                myHashMap[key] = this.NextJSONValue(
                    this.SkipWhitespaceJSON(),
                    nextChar,
                    depth);
                switch (nextChar[0])
                {
                    case ',':
                        seenComma = true;
                        break;
                    case '}':
                        return CBORObject.FromRaw(myHashMap);
                    default:
                        this.RaiseError("Expected a ',' or '}'");
                        break;
                }
            }
        }

        internal CBORObject ParseJSONArray(int depth)
        {
            // Assumes that the last character read was '['
            if (depth > 1000)
            {
                this.RaiseError("Too deeply nested");
            }
            var myArrayList = new List<CBORObject>();
            var seenComma = false;
            var nextChar = new int[1];
            while (true)
            {
                int c = this.SkipWhitespaceJSON();
                if (c == ']')
                {
                    if (seenComma)
                    {
                        // Situation like '[0,1,]'
                        this.RaiseError("Trailing comma");
                    }
                    return CBORObject.FromRaw(myArrayList);
                }
                if (c == ',')
                {
                    // Situation like '[,0,1,2]' or '[0,,1]'
                    this.RaiseError("Empty array element");
                }
                myArrayList.Add(
                  this.NextJSONValue(
                    c,
                    nextChar,
                    depth));
                c = nextChar[0];
                switch (c)
                {
                    case ',':
                        seenComma = true;
                        break;
                    case ']':
                        return CBORObject.FromRaw(myArrayList);
                    default:
                        this.RaiseError("Expected a ',' or ']'");
                        break;
                }
            }
        }
    }
    #endregion

    #region CBORJson2
    internal sealed class CBORJson2
    {
        // JSON parsing method
        private int SkipWhitespaceJSON()
        {
            while (this.index < this.endPos)
            {
                byte c = this.bytes[this.index++];
                if (c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09)
                {
                    return c;
                }
            }
            return -1;
        }

        internal void RaiseError(string str)
        {
            throw new CBORException(str + " (approx. offset: " +
              Math.Max(0, this.index - 1) + ")");
        }

        private readonly byte[] bytes;
        private readonly JSONOptions options;
        private int index;
        private int endPos;
        private static byte[] valueEmptyBytes = new byte[0];

        private byte[] NextJSONString()
        {
            int c;
            int startIndex = this.index;
            int batchIndex = startIndex;
            int batchEnd = startIndex;
            byte[] jbytes = this.bytes;
            while (true)
            {
                if (this.index >= this.endPos)
                {
                    this.RaiseError("Unterminated string");
                }
                c = ((int)jbytes[this.index++]) & 0xff;
                if (c < 0x20)
                {
                    this.RaiseError("Invalid character in string literal");
                }
                if (c == '\\')
                {
                    batchEnd = this.index - 1;
                    break;
                }
                else if (c == 0x22)
                {
                    int isize = (this.index - startIndex) - 1;
                    if (isize == 0)
                    {
                        return valueEmptyBytes;
                    }
                    var buf = new byte[isize];
                    Array.Copy(jbytes, startIndex, buf, 0, isize);
                    return buf;
                }
                else if (c < 0x80)
                {
                    continue;
                }
                else if (c >= 0xc2 && c <= 0xdf)
                {
                    int c1 = this.index < this.endPos ?
                      ((int)this.bytes[this.index++]) & 0xff : -1;
                    if (c1 < 0x80 || c1 > 0xbf)
                    {
                        this.RaiseError("Invalid encoding");
                    }
                }
                else if (c >= 0xe0 && c <= 0xef)
                {
                    int c1 = this.index < this.endPos ?
                      ((int)this.bytes[this.index++]) & 0xff : -1;
                    int c2 = this.index < this.endPos ?
                      ((int)this.bytes[this.index++]) & 0xff : -1;
                    int lower = (c == 0xe0) ? 0xa0 : 0x80;
                    int upper = (c == 0xed) ? 0x9f : 0xbf;
                    if (c1 < lower || c1 > upper || c2 < 0x80 || c2 > 0xbf)
                    {
                        this.RaiseError("Invalid encoding");
                    }
                }
                else if (c >= 0xf0 && c <= 0xf4)
                {
                    int c1 = this.index < this.endPos ?
                      ((int)this.bytes[this.index++]) & 0xff : -1;
                    int c2 = this.index < this.endPos ?
                      ((int)this.bytes[this.index++]) & 0xff : -1;
                    int c3 = this.index < this.endPos ?
                      ((int)this.bytes[this.index++]) & 0xff : -1;
                    int lower = (c == 0xf0) ? 0x90 : 0x80;
                    int upper = (c == 0xf4) ? 0x8f : 0xbf;
                    if (c1 < lower || c1 > upper || c2 < 0x80 || c2 > 0xbf ||
                      c3 < 0x80 || c3 > 0xbf)
                    {
                        this.RaiseError("Invalid encoding");
                    }
                }
                else
                {
                    this.RaiseError("Invalid encoding");
                }
            }
            using (var ms = new MemoryStream())
            {
                if (batchEnd > batchIndex)
                {
                    ms.Write(jbytes, batchIndex, batchEnd - batchIndex);
                    this.index = batchEnd;
                    batchIndex = batchEnd;
                }
                else
                {
                    this.index = startIndex;
                    batchIndex = startIndex;
                }
                while (true)
                {
                    batchEnd = this.index;
                    c = this.index < this.endPos ? ((int)jbytes[this.index++]) &
                      0xff : -1;
                    if (c == -1)
                    {
                        this.RaiseError("Unterminated string");
                    }
                    if (c < 0x20)
                    {
                        this.RaiseError("Invalid character in string literal");
                    }
                    switch (c)
                    {
                        case '\\':
                            c = this.index < this.endPos ? ((int)jbytes[this.index++]) &
                              0xff : -1;
                            switch (c)
                            {
                                case '\\':
                                case '/':
                                case '\"':
                                    // Slash is now allowed to be escaped under RFC 8259
                                    if (batchEnd > batchIndex)
                                    {
                                        ms.Write(jbytes, batchIndex, batchEnd - batchIndex);
                                    }
                                    batchIndex = this.index;
                                    ms.WriteByte((byte)c);
                                    break;
                                case 'b':
                                    if (batchEnd > batchIndex)
                                    {
                                        ms.Write(jbytes, batchIndex, batchEnd - batchIndex);
                                    }
                                    batchIndex = this.index;
                                    ms.WriteByte((byte)'\b');
                                    break;
                                case 'f':
                                    if (batchEnd > batchIndex)
                                    {
                                        ms.Write(jbytes, batchIndex, batchEnd - batchIndex);
                                    }
                                    batchIndex = this.index;
                                    ms.WriteByte((byte)'\f');
                                    break;
                                case 'n':
                                    if (batchEnd > batchIndex)
                                    {
                                        ms.Write(jbytes, batchIndex, batchEnd - batchIndex);
                                    }
                                    batchIndex = this.index;
                                    ms.WriteByte((byte)'\n');
                                    break;
                                case 'r':
                                    if (batchEnd > batchIndex)
                                    {
                                        ms.Write(jbytes, batchIndex, batchEnd - batchIndex);
                                    }
                                    batchIndex = this.index;
                                    ms.WriteByte((byte)'\r');
                                    break;
                                case 't':
                                    if (batchEnd > batchIndex)
                                    {
                                        ms.Write(jbytes, batchIndex, batchEnd - batchIndex);
                                    }
                                    batchIndex = this.index;
                                    ms.WriteByte((byte)'\t');
                                    break;
                                case 'u':
                                    { // Unicode escape
                                        c = 0;
                                        // Consists of 4 hex digits
                                        for (var i = 0; i < 4; ++i)
                                        {
                                            int ch = this.index < this.endPos ?
                                              (int)jbytes[this.index++] : -1;
                                            if (ch >= '0' && ch <= '9')
                                            {
                                                c <<= 4;
                                                c |= ch - '0';
                                            }
                                            else if (ch >= 'A' && ch <= 'F')
                                            {
                                                c <<= 4;
                                                c |= ch + 10 - 'A';
                                            }
                                            else if (ch >= 'a' && ch <= 'f')
                                            {
                                                c <<= 4;
                                                c |= ch + 10 - 'a';
                                            }
                                            else
                                            {
                                                this.RaiseError(
                                                  "Invalid Unicode escaped character");
                                            }
                                        }
                                        if ((c & 0xf800) != 0xd800)
                                        {
                                            // Non-surrogate
                                            if (batchEnd > batchIndex)
                                            {
                                                ms.Write(jbytes, batchIndex, batchEnd - batchIndex);
                                            }
                                            batchIndex = this.index;
                                            int ic = c;
                                            if (c >= 0x800)
                                            {
                                                ms.WriteByte((byte)(0xe0 | ((ic >> 12) & 0x0f)));
                                                ms.WriteByte((byte)(0x80 | ((ic >> 6) & 0x3f)));
                                                ms.WriteByte((byte)(0x80 | (ic & 0x3f)));
                                            }
                                            else if (c >= 0x80)
                                            {
                                                ms.WriteByte((byte)(0xc0 | ((ic >> 6) & 0x1f)));
                                                ms.WriteByte((byte)(0x80 | (ic & 0x3f)));
                                            }
                                            else
                                            {
                                                ms.WriteByte((byte)ic);
                                            }
                                        }
                                        else if ((c & 0xfc00) == 0xd800)
                                        {
                                            int ch;
                                            if (this.index >= this.endPos - 1 ||
                                              jbytes[this.index] != (byte)'\\' ||
                                              jbytes[this.index + 1] != (byte)0x75)
                                            {
                                                this.RaiseError("Invalid escaped character");
                                            }
                                            this.index += 2;
                                            var c2 = 0;
                                            for (var i = 0; i < 4; ++i)
                                            {
                                                ch = this.index < this.endPos ?
                                                  ((int)jbytes[this.index++]) & 0xff : -1;
                                                if (ch >= '0' && ch <= '9')
                                                {
                                                    c2 <<= 4;
                                                    c2 |= ch - '0';
                                                }
                                                else if (ch >= 'A' && ch <= 'F')
                                                {
                                                    c2 <<= 4;
                                                    c2 |= ch + 10 - 'A';
                                                }
                                                else if (ch >= 'a' && ch <= 'f')
                                                {
                                                    c2 <<= 4;
                                                    c2 |= ch + 10 - 'a';
                                                }
                                                else
                                                {
                                                    this.RaiseError(
                                                      "Invalid Unicode escaped character");
                                                }
                                            }
                                            if ((c2 & 0xfc00) != 0xdc00)
                                            {
                                                this.RaiseError("Unpaired surrogate code point");
                                            }
                                            else
                                            {
                                                if (batchEnd > batchIndex)
                                                {
                                                    ms.Write(jbytes, batchIndex, batchEnd - batchIndex);
                                                }
                                                batchIndex = this.index;
                                                int ic = 0x10000 + (((int)c & 0x3ff) << 10) +
                                                  ((int)c2 & 0x3ff);
                                                ms.WriteByte((byte)(0xf0 | ((ic >> 18) & 0x07)));
                                                ms.WriteByte((byte)(0x80 | ((ic >> 12) & 0x3f)));
                                                ms.WriteByte((byte)(0x80 | ((ic >> 6) & 0x3f)));
                                                ms.WriteByte((byte)(0x80 | (ic & 0x3f)));
                                            }
                                        }
                                        else
                                        {
                                            this.RaiseError("Unpaired surrogate code point");
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        this.RaiseError("Invalid escaped character");
                                        break;
                                    }
                            }
                            break;
                        case 0x22: // double quote
                            if (batchEnd > batchIndex)
                            {
                                ms.Write(jbytes, batchIndex, batchEnd - batchIndex);
                            }
                            return ms.ToArray();
                        default:
                            {
                                if (c <= 0x7f)
                                {
                                    // Deliberately empty
                                }
                                else if (c >= 0xc2 && c <= 0xdf)
                                {
                                    int c1 = this.index < this.endPos ?
                                      ((int)jbytes[this.index++]) & 0xff : -1;
                                    if (c1 < 0x80 || c1 > 0xbf)
                                    {
                                        this.RaiseError("Invalid encoding");
                                    }
                                }
                                else if (c >= 0xe0 && c <= 0xef)
                                {
                                    int c1 = this.index < this.endPos ?
                                      ((int)jbytes[this.index++]) & 0xff : -1;
                                    int c2 = this.index < this.endPos ?
                                      ((int)jbytes[this.index++]) & 0xff : -1;
                                    int lower = (c == 0xe0) ? 0xa0 : 0x80;
                                    int upper = (c == 0xed) ? 0x9f : 0xbf;
                                    if (c1 < lower || c1 > upper || c2 < 0x80 || c2 > 0xbf)
                                    {
                                        this.RaiseError("Invalid encoding");
                                    }
                                }
                                else if (c >= 0xf0 && c <= 0xf4)
                                {
                                    int c1 = this.index < this.endPos ?
                                      ((int)jbytes[this.index++]) & 0xff : -1;
                                    int c2 = this.index < this.endPos ?
                                      ((int)jbytes[this.index++]) & 0xff : -1;
                                    int c3 = this.index < this.endPos ?
                                      ((int)jbytes[this.index++]) & 0xff : -1;
                                    int lower = (c == 0xf0) ? 0x90 : 0x80;
                                    int upper = (c == 0xf4) ? 0x8f : 0xbf;
                                    if (c1 < lower || c1 > upper || c2 < 0x80 || c2 > 0xbf ||
                                      c3 < 0x80 || c3 > 0xbf)
                                    {
                                        this.RaiseError("Invalid encoding");
                                    }
                                }
                                else
                                {
                                    this.RaiseError("Invalid encoding");
                                }
                                break;
                            }
                    }
                }
            }
        }

        private CBORObject NextJSONNegativeNumber(
          int[] nextChar)
        {
            // Assumes the last character read was '-'
            CBORObject obj;
            int numberStartIndex = this.index - 1;
            int c = this.index < this.endPos ? ((int)this.bytes[this.index++]) &
              0xff : -1;
            if (c < '0' || c > '9')
            {
                this.RaiseError("JSON number can't be parsed.");
            }
            int cstart = c;
            while (c == '-' || c == '+' || c == '.' || (c >= '0' && c <= '9') ||
              c == 'e' || c == 'E')
            {
                c = this.index < this.endPos ? ((int)this.bytes[this.index++]) &
                  0xff : -1;
            }
            // check if character can validly appear after a JSON number
            if (c != ',' && c != ']' && c != '}' && c != -1 &&
              c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09)
            {
                this.RaiseError("Invalid character after JSON number");
            }
            int numberEndIndex = c < 0 ?
              this.endPos : this.index - 1;
            if (numberEndIndex - numberStartIndex == 2 && cstart != '0')
            {
                // Negative single digit other than negative zero
                obj = CBORDataUtilities.ParseSmallNumberAsNegative((int)(cstart
                      - '0'),
                    this.options);
            }
            else
            {
                obj = CBORDataUtilities.ParseJSONNumber(
                    this.bytes,
                    numberStartIndex,
                    numberEndIndex - numberStartIndex,
                    this.options);
#if DEBUG
                if ((
          (EDecimal)obj.ToObject(
          typeof(EDecimal))).CompareToValue(EDecimal.FromString(this.bytes,
                   numberStartIndex,
                   numberEndIndex - numberStartIndex)) != 0)
                {
                    this.RaiseError(String.Empty + obj);
                }
#endif
                if (obj == null)
                {
                    string errstr = String.Empty;
                    // errstr = (str.Length <= 100) ? str : (str.Substring(0,
                    // 100) + "...");
                    this.RaiseError("JSON number can't be parsed. " + errstr);
                }
            }
            if (c == -1 || (c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09))
            {
                nextChar[0] = c;
            }
            else
            {
                nextChar[0] = this.SkipWhitespaceJSON();
            }
            return obj;
        }

        private CBORObject NextJSONNonnegativeNumber(int c, int[] nextChar)
        {
            // Assumes the last character read was a digit
            CBORObject obj = null;
            int cval = c - '0';
            int cstart = c;
            int startIndex = this.index - 1;
            var needObj = true;
            int numberStartIndex = this.index - 1;
            c = this.index < this.endPos ? ((int)this.bytes[this.index++]) &
              0xff : -1;
            if (!(c == '-' || c == '+' || c == '.' || (c >= '0' && c <= '9') ||
                c == 'e' || c == 'E'))
            {
                // Optimize for common case where JSON number
                // is a single digit without sign or exponent
                obj = CBORDataUtilities.ParseSmallNumber(cval, this.options);
                needObj = false;
            }
            else if (c >= '0' && c <= '9')
            {
                int csecond = c;
                if (cstart == '0')
                {
                    // Leading zero followed by any digit is not allowed
                    this.RaiseError("JSON number can't be parsed.");
                }
                cval = (cval * 10) + (int)(c - '0');
                c = this.index < this.endPos ? ((int)this.bytes[this.index++]) &
                  0xff : -1;
                if (c >= '0' && c <= '9')
                {
                    var digits = 2;
                    while (digits < 9 && (c >= '0' && c <= '9'))
                    {
                        cval = (cval * 10) + (int)(c - '0');
                        c = this.index < this.endPos ?
                          ((int)this.bytes[this.index++]) & 0xff : -1;
                        ++digits;
                    }
                    if (!(c == 'e' || c == 'E' || c == '.' || (c >= '0' && c <=
                          '9')))
                    {
                        // All-digit number that's short enough
                        obj = CBORDataUtilities.ParseSmallNumber(cval, this.options);
#if DEBUG
                        if ((
              (EDecimal)obj.ToObject(
              typeof(EDecimal))).CompareToValue(EDecimal.FromInt32(cval)) !=
            0)
                        {
                            this.RaiseError(String.Empty + obj);
                        }
#endif
                        needObj = false;
                    }
                }
                else if (!(c == '-' || c == '+' || c == '.' || c == 'e' || c
                  == 'E'))
                {
                    // Optimize for common case where JSON number
                    // is two digits without sign, decimal point, or exponent
                    obj = CBORDataUtilities.ParseSmallNumber(cval, this.options);
#if DEBUG
                    if ((
            (EDecimal)obj.ToObject(
            typeof(EDecimal))).CompareToValue(EDecimal.FromInt32(cval)) !=
          0)
                    {
                        this.RaiseError(String.Empty + obj);
                    }
#endif
                    needObj = false;
                }
            }
            if (needObj)
            {
                while (c == '-' || c == '+' || c == '.' || (c >= '0' && c <= '9') ||
                  c == 'e' || c == 'E')
                {
                    c = this.index < this.endPos ? ((int)this.bytes[this.index++]) &
                      0xff : -1;
                }
                // check if character can validly appear after a JSON number
                if (c != ',' && c != ']' && c != '}' && c != -1 &&
                  c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09)
                {
                    this.RaiseError("Invalid character after JSON number");
                }
                int numberEndIndex = c < 0 ? this.endPos : this.index - 1;
                obj = CBORDataUtilities.ParseJSONNumber(
                   this.bytes,
                   numberStartIndex,
                   numberEndIndex - numberStartIndex,
                   this.options);
#if DEBUG
                if ((
          (EDecimal)obj.ToObject(
          typeof(EDecimal))).CompareToValue(EDecimal.FromString(this.bytes,
                   numberStartIndex,
                   numberEndIndex - numberStartIndex)) != 0)
                {
                    this.RaiseError(String.Empty + obj);
                }
#endif
                if (obj == null)
                {
                    string errstr = String.Empty;
                    // errstr = (str.Length <= 100) ? str : (str.Substring(0,
                    // 100) + "...");
                    this.RaiseError("JSON number can't be parsed. " + errstr);
                }
            }
            if (c == -1 || (c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09))
            {
                nextChar[0] = c;
            }
            else
            {
                nextChar[0] = this.SkipWhitespaceJSON();
            }
            return obj;
        }

        private CBORObject NextJSONValue(
          int firstChar,
          int[] nextChar,
          int depth)
        {
            int c = firstChar;
            CBORObject obj = null;
            if (c < 0)
            {
                this.RaiseError("Unexpected end of data");
            }
            switch (c)
            {
                case '"':
                    {
                        // Parse a string
                        // The tokenizer already checked the string for invalid
                        // surrogate pairs, so just call the CBORObject
                        // constructor directly
                        obj = CBORObject.FromRawUtf8(this.NextJSONString());
                        nextChar[0] = this.SkipWhitespaceJSON();
                        return obj;
                    }
                case '{':
                    {
                        // Parse an object
                        obj = this.ParseJSONObject(depth + 1);
                        nextChar[0] = this.SkipWhitespaceJSON();
                        return obj;
                    }
                case '[':
                    {
                        // Parse an array
                        obj = this.ParseJSONArray(depth + 1);
                        nextChar[0] = this.SkipWhitespaceJSON();
                        return obj;
                    }
                case 't':
                    {
                        // Parse true
                        if (this.endPos - this.index <= 2 ||
                          this.bytes[this.index] != (byte)0x72 ||
                          this.bytes[this.index + 1] != (byte)0x75 ||
                          this.bytes[this.index + 2] != (byte)0x65)
                        {
                            this.RaiseError("Value can't be parsed.");
                        }
                        this.index += 3;
                        nextChar[0] = this.SkipWhitespaceJSON();
                        return CBORObject.True;
                    }
                case 'f':
                    {
                        // Parse false
                        if (this.endPos - this.index <= 3 ||
                          this.bytes[this.index] != (byte)0x61 ||
                          this.bytes[this.index + 1] != (byte)0x6c ||
                          this.bytes[this.index + 2] != (byte)0x73 ||
                          this.bytes[this.index + 3] != (byte)0x65)
                        {
                            this.RaiseError("Value can't be parsed.");
                        }
                        this.index += 4;
                        nextChar[0] = this.SkipWhitespaceJSON();
                        return CBORObject.False;
                    }
                case 'n':
                    {
                        // Parse null
                        if (this.endPos - this.index <= 2 ||
                          this.bytes[this.index] != (byte)0x75 ||
                          this.bytes[this.index + 1] != (byte)0x6c ||
                          this.bytes[this.index + 2] != (byte)0x6c)
                        {
                            this.RaiseError("Value can't be parsed.");
                        }
                        this.index += 3;
                        nextChar[0] = this.SkipWhitespaceJSON();
                        return CBORObject.Null;
                    }
                case '-':
                    {
                        // Parse a negative number
                        return this.NextJSONNegativeNumber(nextChar);
                    }
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    {
                        // Parse a nonnegative number
                        return this.NextJSONNonnegativeNumber(c, nextChar);
                    }
                default:
                    this.RaiseError("Value can't be parsed.");
                    break;
            }
            return null;
        }

        public CBORJson2(byte[] bytes, int index, int endPos, JSONOptions
          options)
        {
#if DEBUG
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }
            if (index < 0)
            {
                throw new ArgumentException("index (" + index + ") is not greater or" +
                  "\u0020equal to 0");
            }
            if (index > bytes.Length)
            {
                throw new ArgumentException("index (" + index + ") is not less or" +
                  "\u0020equal to " + bytes.Length);
            }
            if (endPos < 0)
            {
                throw new ArgumentException("endPos (" + endPos + ") is not greater" +
                  "\u0020or equal to 0");
            }
            if (endPos > bytes.Length)
            {
                throw new ArgumentException("endPos (" + endPos + ") is not less or" +
                  "\u0020equal to " + bytes.Length);
            }
            if (endPos < index)
            {
                throw new ArgumentException("endPos (" + endPos + ") is not greater" +
                  "\u0020or equal to " + index);
            }
#endif

            this.bytes = bytes;
            this.index = index;
            this.endPos = endPos;
            this.options = options;
        }

        public CBORObject ParseJSON(int[] nextchar)
        {
            int c;
            CBORObject ret;
            c = this.SkipWhitespaceJSON();
            if (c == '[')
            {
                ret = this.ParseJSONArray(0);
                nextchar[0] = this.SkipWhitespaceJSON();
                return ret;
            }
            if (c == '{')
            {
                ret = this.ParseJSONObject(0);
                nextchar[0] = this.SkipWhitespaceJSON();
                return ret;
            }
            return this.NextJSONValue(c, nextchar, 0);
        }

        internal static CBORObject ParseJSONValue(
          byte[] bytes,
          int index,
          int endPos,
          JSONOptions options)
        {
            var nextchar = new int[1];
            var cj = new CBORJson2(bytes, index, endPos, options);
            CBORObject obj = cj.ParseJSON(nextchar);
            if (nextchar[0] != -1)
            {
                cj.RaiseError("End of bytes not reached");
            }
            return obj;
        }

        internal static CBORObject ParseJSONValue(
          byte[] bytes,
          int index,
          int endPos,
          JSONOptions options,
          int[] nextchar)
        {
#if DEBUG
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }
            if (index < 0)
            {
                throw new ArgumentException("index (" + index + ") is not greater or" +
                  "\u0020equal to 0");
            }
            if (index > bytes.Length)
            {
                throw new ArgumentException("index (" + index + ") is not less or" +
                  "\u0020equal to " + bytes.Length);
            }
            if (endPos < 0)
            {
                throw new ArgumentException("endPos (" + endPos + ") is not greater" +
                  "\u0020or equal to 0");
            }
            if (endPos > bytes.Length)
            {
                throw new ArgumentException("endPos (" + endPos + ") is not less or" +
                  "\u0020equal to " + bytes.Length);
            }
            if (endPos < index)
            {
                throw new ArgumentException("endPos (" + endPos + ") is not greater" +
                  "\u0020or equal to " + index);
            }
#endif

            var cj = new CBORJson2(bytes, index, endPos, options);
            return cj.ParseJSON(nextchar);
        }

        private CBORObject ParseJSONObject(int depth)
        {
            // Assumes that the last character read was '{'
            if (depth > 1000)
            {
                this.RaiseError("Too deeply nested");
            }
            int c;
            CBORObject key = null;
            CBORObject obj;
            var nextchar = new int[1];
            var seenComma = false;
            var myHashMap = new SortedDictionary<CBORObject, CBORObject>();
            while (true)
            {
                c = this.SkipWhitespaceJSON();
                switch (c)
                {
                    case -1:
                        this.RaiseError("A JSON object must end with '}'");
                        break;
                    case '}':
                        if (seenComma)
                        {
                            // Situation like '{"0"=>1,}'
                            this.RaiseError("Trailing comma");
                            return null;
                        }
                        return CBORObject.FromRaw(myHashMap);
                    default:
                        {
                            // Read the next string
                            if (c < 0)
                            {
                                this.RaiseError("Unexpected end of data");
                                return null;
                            }
                            if (c != '"')
                            {
                                this.RaiseError("Expected a string as a key");
                                return null;
                            }
                            // Parse a string that represents the object's key
                            // The tokenizer already checked the string for invalid
                            // surrogate pairs, so just call the CBORObject
                            // constructor directly
                            obj = CBORObject.FromRawUtf8(this.NextJSONString());
                            key = obj;
                            if (!this.options.AllowDuplicateKeys &&
                              myHashMap.ContainsKey(obj))
                            {
                                this.RaiseError("Key already exists: " + key);
                                return null;
                            }
                            break;
                        }
                }
                if (this.SkipWhitespaceJSON() != ':')
                {
                    this.RaiseError("Expected a ':' after a key");
                }
                // NOTE: Will overwrite existing value
                myHashMap[key] = this.NextJSONValue(
                    this.SkipWhitespaceJSON(),
                    nextchar,
                    depth);
                switch (nextchar[0])
                {
                    case ',':
                        seenComma = true;
                        break;
                    case '}':
                        return CBORObject.FromRaw(myHashMap);
                    default:
                        this.RaiseError("Expected a ',' or '}'");
                        break;
                }
            }
        }

        internal CBORObject ParseJSONArray(int depth)
        {
            // Assumes that the last character read was '['
            if (depth > 1000)
            {
                this.RaiseError("Too deeply nested");
            }
            var myArrayList = new List<CBORObject>();
            var seenComma = false;
            var nextchar = new int[1];
            while (true)
            {
                int c = this.SkipWhitespaceJSON();
                if (c == ']')
                {
                    if (seenComma)
                    {
                        // Situation like '[0,1,]'
                        this.RaiseError("Trailing comma");
                    }
                    return CBORObject.FromRaw(myArrayList);
                }
                if (c == ',')
                {
                    // Situation like '[,0,1,2]' or '[0,,1]'
                    this.RaiseError("Empty array element");
                }
                myArrayList.Add(
                  this.NextJSONValue(
                    c,
                    nextchar,
                    depth));
                c = nextchar[0];
                switch (c)
                {
                    case ',':
                        seenComma = true;
                        break;
                    case ']':
                        return CBORObject.FromRaw(myArrayList);
                    default:
                        this.RaiseError("Expected a ',' or ']'");
                        break;
                }
            }
        }
    }
    #endregion

    #region CBORJson3
    internal sealed class CBORJson3
    {
        // JSON parsing method
        private int SkipWhitespaceJSON()
        {
            while (this.index < this.endPos)
            {
                char c = this.jstring[this.index++];
                if (c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09)
                {
                    return c;
                }
            }
            return -1;
        }

        internal void RaiseError(string str)
        {
            throw new CBORException(str + " (approx. offset: " +
              Math.Max(0, this.index - 1) + ")");
        }

        // NOTE: Differs from CBORJson2
        private readonly string jstring;
        private readonly JSONOptions options;
        private StringBuilder sb;
        private int index;
        private int endPos;

        private string NextJSONString()
        {
            int c;
            int startIndex = this.index;
            var endIndex = -1;
            int ep = this.endPos;
            string js = this.jstring;
            int idx = this.index;
            while (true)
            {
                c = idx < ep ? ((int)js[idx++]) & 0xffff : -1;
                if (c == -1 || c < 0x20)
                {
                    this.index = idx;
                    this.RaiseError("Unterminated string");
                }
                else if (c == '"')
                {
                    int iend = idx - 1;
                    this.index = idx;
                    return js.Substring(
                        startIndex,
                        iend - startIndex);
                }
                else if (c == '\\' || (c & 0xf800) == 0xd800)
                {
                    this.index = idx - 1;
                    endIndex = this.index;
                    break;
                }
            }
            this.sb = this.sb ?? new StringBuilder();
            this.sb.Remove(0, this.sb.Length);
            this.sb.Append(js, startIndex, endIndex - startIndex);
            while (true)
            {
                c = this.index < ep ? ((int)js[this.index++]) &
                  0xffff : -1;
                if (c == -1 || c < 0x20)
                {
                    this.RaiseError("Unterminated string");
                }
                switch (c)
                {
                    case '\\':
                        endIndex = this.index - 1;
                        c = this.index < ep ? ((int)js[this.index++]) &
                          0xffff : -1;
                        switch (c)
                        {
                            case '\\':
                            case '/':
                            case '\"':
                                // Slash is now allowed to be escaped under RFC 8259
                                this.sb.Append((char)c);
                                break;
                            case 'b':
                                this.sb.Append('\b');
                                break;
                            case 'f':
                                this.sb.Append('\f');
                                break;
                            case 'n':
                                this.sb.Append('\n');
                                break;
                            case 'r':
                                this.sb.Append('\r');
                                break;
                            case 't':
                                this.sb.Append('\t');
                                break;
                            case 'u':
                                { // Unicode escape
                                    c = 0;
                                    // Consists of 4 hex digits
                                    for (var i = 0; i < 4; ++i)
                                    {
                                        int ch = this.index < ep ?
                                          ((int)js[this.index++]) : -1;
                                        if (ch >= '0' && ch <= '9')
                                        {
                                            c <<= 4;
                                            c |= ch - '0';
                                        }
                                        else if (ch >= 'A' && ch <= 'F')
                                        {
                                            c <<= 4;
                                            c |= ch + 10 - 'A';
                                        }
                                        else if (ch >= 'a' && ch <= 'f')
                                        {
                                            c <<= 4;
                                            c |= ch + 10 - 'a';
                                        }
                                        else
                                        {
                                            this.RaiseError(
                                              "Invalid Unicode escaped character");
                                        }
                                    }
                                    if ((c & 0xf800) != 0xd800)
                                    {
                                        // Non-surrogate
                                        this.sb.Append((char)c);
                                    }
                                    else if ((c & 0xfc00) == 0xd800)
                                    {
                                        int ch = this.index < ep ? ((int)js[this.index++]) : -1;
                                        if (ch != '\\' || (this.index < ep ?
                                            ((int)js[this.index++]) : -1) != 'u')
                                        {
                                            this.RaiseError("Invalid escaped character");
                                        }
                                        var c2 = 0;
                                        for (var i = 0; i < 4; ++i)
                                        {
                                            ch = this.index < ep ?
                                              ((int)js[this.index++]) : -1;
                                            if (ch >= '0' && ch <= '9')
                                            {
                                                c2 <<= 4;
                                                c2 |= ch - '0';
                                            }
                                            else if (ch >= 'A' && ch <= 'F')
                                            {
                                                c2 <<= 4;
                                                c2 |= ch + 10 - 'A';
                                            }
                                            else if (ch >= 'a' && ch <= 'f')
                                            {
                                                c2 <<= 4;
                                                c2 |= ch + 10 - 'a';
                                            }
                                            else
                                            {
                                                this.RaiseError(
                                                  "Invalid Unicode escaped character");
                                            }
                                        }
                                        if ((c2 & 0xfc00) != 0xdc00)
                                        {
                                            this.RaiseError("Unpaired surrogate code point");
                                        }
                                        else
                                        {
                                            this.sb.Append((char)c);
                                            this.sb.Append((char)c2);
                                        }
                                    }
                                    else
                                    {
                                        this.RaiseError("Unpaired surrogate code point");
                                    }
                                    break;
                                }
                            default:
                                {
                                    this.RaiseError("Invalid escaped character");
                                    break;
                                }
                        }
                        break;
                    case 0x22: // double quote
                        return this.sb.ToString();
                    default:
                        {
                            // NOTE: Differs from CBORJson2
                            if ((c & 0xf800) != 0xd800)
                            {
                                // Non-surrogate
                                this.sb.Append((char)c);
                            }
                            else if ((c & 0xfc00) == 0xd800 && this.index < ep &&
                            (js[this.index] & 0xfc00) == 0xdc00)
                            {
                                // Surrogate pair
                                this.sb.Append((char)c);
                                this.sb.Append(js[this.index]);
                                ++this.index;
                            }
                            else
                            {
                                this.RaiseError("Unpaired surrogate code point");
                            }
                            break;
                        }
                }
            }
        }

        private CBORObject NextJSONNegativeNumber(
          int[] nextChar)
        {
            // Assumes the last character read was '-'
            // DebugUtility.Log("js=" + (jstring));
            CBORObject obj;
            int numberStartIndex = this.index - 1;
            int c = this.index < this.endPos ? ((int)this.jstring[this.index++]) &
              0xffff : -1;
            if (c < '0' || c > '9')
            {
                this.RaiseError("JSON number can't be parsed.");
            }
            if (this.index < this.endPos && c != '0')
            {
                // Check for negative single-digit
                int c2 = ((int)this.jstring[this.index]) & 0xffff;
                if (c2 == ',' || c2 == ']' || c2 == '}')
                {
                    ++this.index;
                    obj = CBORDataUtilities.ParseSmallNumberAsNegative(
                        c - '0',
                        this.options);
                    nextChar[0] = c2;
                    return obj;
                }
                else if (c2 == 0x20 || c2 == 0x0a || c2 == 0x0d || c2 == 0x09)
                {
                    ++this.index;
                    obj = CBORDataUtilities.ParseSmallNumberAsNegative(
                        c - '0',
                        this.options);
                    nextChar[0] = this.SkipWhitespaceJSON();
                    return obj;
                }
            }
            // NOTE: Differs from CBORJson2, notably because the whole
            // rest of the string is checked whether the beginning of the rest
            // is a JSON number
            var endIndex = new int[1];
            endIndex[0] = numberStartIndex;
            obj = CBORDataUtilitiesTextString.ParseJSONNumber(
                this.jstring,
                numberStartIndex,
                this.endPos - numberStartIndex,
                this.options,
                endIndex);
            int numberEndIndex = endIndex[0];
            this.index = numberEndIndex >= this.endPos ? this.endPos :
              (numberEndIndex + 1);
            if (obj == null)
            {
                int strlen = numberEndIndex - numberStartIndex;
                string errstr = this.jstring.Substring(numberStartIndex,
                    Math.Min(100, strlen));
                if (strlen > 100)
                {
                    errstr += "...";
                }
                this.RaiseError("JSON number can't be parsed. " + errstr);
            }
#if DEBUG
            if (numberEndIndex < numberStartIndex)
            {
                throw new ArgumentException("numberEndIndex (" + numberEndIndex +
                  ") is not greater or equal to " + numberStartIndex);
            }
#endif
            c = numberEndIndex >= this.endPos ? -1 : this.jstring[numberEndIndex];
            // check if character can validly appear after a JSON number
            if (c != ',' && c != ']' && c != '}' && c != -1 &&
              c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09)
            {
                this.RaiseError("Invalid character after JSON number");
            }
            // DebugUtility.Log("endIndex="+endIndex[0]+", "+
            // this.jstring.Substring(endIndex[0],
            // Math.Min(20, this.endPos-endIndex[0])));
            if (c == -1 || (c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09))
            {
                nextChar[0] = c;
            }
            else
            {
                nextChar[0] = this.SkipWhitespaceJSON();
            }
            return obj;
        }

        private CBORObject NextJSONNonnegativeNumber(int c, int[] nextChar)
        {
            // Assumes the last character read was a digit
            CBORObject obj = null;
            int cval = c - '0';
            int cstart = c;
            int startIndex = this.index - 1;
            var needObj = true;
            int numberStartIndex = this.index - 1;
            // DebugUtility.Log("js=" + (jstring));
            c = this.index < this.endPos ? ((int)this.jstring[this.index++]) &
              0xffff : -1;
            if (!(c == '-' || c == '+' || c == '.' || (c >= '0' && c <= '9') ||
                c == 'e' || c == 'E'))
            {
                // Optimize for common case where JSON number
                // is a single digit without sign or exponent
                obj = CBORDataUtilities.ParseSmallNumber(cval, this.options);
                needObj = false;
            }
            else if (c >= '0' && c <= '9')
            {
                int csecond = c;
                if (cstart == '0')
                {
                    // Leading zero followed by any digit is not allowed
                    this.RaiseError("JSON number can't be parsed.");
                }
                cval = (cval * 10) + (int)(c - '0');
                c = this.index < this.endPos ? ((int)this.jstring[this.index++]) : -1;
                if (c >= '0' && c <= '9')
                {
                    var digits = 2;
                    while (digits < 9 && (c >= '0' && c <= '9'))
                    {
                        cval = (cval * 10) + (int)(c - '0');
                        c = this.index < this.endPos ?
                          ((int)this.jstring[this.index++]) : -1;
                        ++digits;
                    }
                    if (!(c == 'e' || c == 'E' || c == '.' || (c >= '0' && c <=
                          '9')))
                    {
                        // All-digit number that's short enough
                        obj = CBORDataUtilities.ParseSmallNumber(cval, this.options);
                        needObj = false;
                    }
                }
                else if (!(c == '-' || c == '+' || c == '.' || c == 'e' || c
                  == 'E'))
                {
                    // Optimize for common case where JSON number
                    // is two digits without sign, decimal point, or exponent
                    obj = CBORDataUtilities.ParseSmallNumber(cval, this.options);
                    needObj = false;
                }
            }
            if (needObj)
            {
                // NOTE: Differs from CBORJson2, notably because the whole
                // rest of the string is checked whether the beginning of the rest
                // is a JSON number
                var endIndex = new int[1];
                endIndex[0] = numberStartIndex;
                obj = CBORDataUtilitiesTextString.ParseJSONNumber(
                    this.jstring,
                    numberStartIndex,
                    this.endPos - numberStartIndex,
                    this.options,
                    endIndex);
                int numberEndIndex = endIndex[0];
                this.index = numberEndIndex >= this.endPos ? this.endPos :
                  (numberEndIndex + 1);
                if (obj == null)
                {
                    int strlen = numberEndIndex - numberStartIndex;
                    string errstr = this.jstring.Substring(numberStartIndex,
                        Math.Min(100, strlen));
                    if (strlen > 100)
                    {
                        errstr += "...";
                    }
                    this.RaiseError("JSON number can't be parsed. " + errstr);
                }
#if DEBUG
                if (numberEndIndex < numberStartIndex)
                {
                    throw new ArgumentException("numberEndIndex (" + numberEndIndex +
                      ") is not greater or equal to " + numberStartIndex);
                }
#endif

                c = numberEndIndex >= this.endPos ? -1 : this.jstring[numberEndIndex];
                // check if character can validly appear after a JSON number
                if (c != ',' && c != ']' && c != '}' && c != -1 &&
                  c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09)
                {
                    this.RaiseError("Invalid character after JSON number");
                }
                // DebugUtility.Log("endIndex="+endIndex[0]+", "+
                // this.jstring.Substring(endIndex[0],
                // Math.Min(20, this.endPos-endIndex[0])));
            }
            if (c == -1 || (c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09))
            {
                nextChar[0] = c;
            }
            else
            {
                nextChar[0] = this.SkipWhitespaceJSON();
            }
            return obj;
        }

        private CBORObject NextJSONValue(
          int firstChar,
          int[] nextChar,
          int depth)
        {
            int c = firstChar;
            CBORObject obj = null;
            if (c < 0)
            {
                this.RaiseError("Unexpected end of data");
            }
            switch (c)
            {
                case '"':
                    {
                        // Parse a string
                        // The tokenizer already checked the string for invalid
                        // surrogate pairs, so just call the CBORObject
                        // constructor directly
                        obj = CBORObject.FromRaw(this.NextJSONString());
                        nextChar[0] = this.SkipWhitespaceJSON();
                        return obj;
                    }
                case '{':
                    {
                        // Parse an object
                        obj = this.ParseJSONObject(depth + 1);
                        nextChar[0] = this.SkipWhitespaceJSON();
                        return obj;
                    }
                case '[':
                    {
                        // Parse an array
                        obj = this.ParseJSONArray(depth + 1);
                        nextChar[0] = this.SkipWhitespaceJSON();
                        return obj;
                    }
                case 't':
                    {
                        // Parse true
                        if (this.endPos - this.index <= 2 ||
                          (((int)this.jstring[this.index]) & 0xFF) != 'r' ||
                          (((int)this.jstring[this.index + 1]) & 0xFF) != 'u' ||
                          (((int)this.jstring[this.index + 2]) & 0xFF) != 'e')
                        {
                            this.RaiseError("Value can't be parsed.");
                        }
                        this.index += 3;
                        nextChar[0] = this.SkipWhitespaceJSON();
                        return CBORObject.True;
                    }
                case 'f':
                    {
                        // Parse false
                        if (this.endPos - this.index <= 3 ||
                          (((int)this.jstring[this.index]) & 0xFF) != 'a' ||
                          (((int)this.jstring[this.index + 1]) & 0xFF) != 'l' ||
                          (((int)this.jstring[this.index + 2]) & 0xFF) != 's' ||
                          (((int)this.jstring[this.index + 3]) & 0xFF) != 'e')
                        {
                            this.RaiseError("Value can't be parsed.");
                        }
                        this.index += 4;
                        nextChar[0] = this.SkipWhitespaceJSON();
                        return CBORObject.False;
                    }
                case 'n':
                    {
                        // Parse null
                        if (this.endPos - this.index <= 2 ||
                          (((int)this.jstring[this.index]) & 0xFF) != 'u' ||
                          (((int)this.jstring[this.index + 1]) & 0xFF) != 'l' ||
                          (((int)this.jstring[this.index + 2]) & 0xFF) != 'l')
                        {
                            this.RaiseError("Value can't be parsed.");
                        }
                        this.index += 3;
                        nextChar[0] = this.SkipWhitespaceJSON();
                        return CBORObject.Null;
                    }
                case '-':
                    {
                        // Parse a negative number
                        return this.NextJSONNegativeNumber(nextChar);
                    }
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    {
                        // Parse a nonnegative number
                        return this.NextJSONNonnegativeNumber(c, nextChar);
                    }
                default:
                    this.RaiseError("Value can't be parsed.");
                    break;
            }
            return null;
        }

        public CBORJson3(string jstring, int index, int endPos, JSONOptions
          options)
        {
#if DEBUG
            if (jstring == null)
            {
                throw new ArgumentNullException(nameof(jstring));
            }
            if (index < 0)
            {
                throw new ArgumentException("index (" + index + ") is not greater or" +
                  "\u0020equal to 0");
            }
            if (index > jstring.Length)
            {
                throw new ArgumentException("index (" + index + ") is not less or" +
                  "\u0020equal to " + jstring.Length);
            }
            if (endPos < 0)
            {
                throw new ArgumentException("endPos (" + endPos + ") is not greater" +
                  "\u0020or equal to 0");
            }
            if (endPos > jstring.Length)
            {
                throw new ArgumentException("endPos (" + endPos + ") is not less or" +
                  "\u0020equal to " + jstring.Length);
            }
            if (endPos < index)
            {
                throw new ArgumentException("endPos (" + endPos + ") is not greater" +
                  "\u0020or equal to " + index);
            }
#endif
            this.sb = null;
            this.jstring = jstring;
            this.index = index;
            this.endPos = endPos;
            this.options = options;
        }

        public CBORObject ParseJSON(int[] nextchar)
        {
            int c;
            CBORObject ret;
            c = this.SkipWhitespaceJSON();
            if (c == '[')
            {
                ret = this.ParseJSONArray(0);
                nextchar[0] = this.SkipWhitespaceJSON();
                return ret;
            }
            if (c == '{')
            {
                ret = this.ParseJSONObject(0);
                nextchar[0] = this.SkipWhitespaceJSON();
                return ret;
            }
            return this.NextJSONValue(c, nextchar, 0);
        }

        internal static CBORObject ParseJSONValue(
          string jstring,
          int index,
          int endPos,
          JSONOptions options)
        {
            var nextchar = new int[1];
            var cj = new CBORJson3(jstring, index, endPos, options);
            CBORObject obj = cj.ParseJSON(nextchar);
            if (nextchar[0] != -1)
            {
                cj.RaiseError("End of string not reached");
            }
            return obj;
        }

        internal static CBORObject ParseJSONValue(
          string jstring,
          int index,
          int endPos,
          JSONOptions options,
          int[] nextchar)
        {
#if DEBUG
            if (jstring == null)
            {
                throw new ArgumentNullException(nameof(jstring));
            }
            if (index < 0)
            {
                throw new ArgumentException("index (" + index + ") is not greater or" +
                  "\u0020equal to 0");
            }
            if (index > jstring.Length)
            {
                throw new ArgumentException("index (" + index + ") is not less or" +
                  "\u0020equal to " + jstring.Length);
            }
            if (endPos < 0)
            {
                throw new ArgumentException("endPos (" + endPos + ") is not greater" +
                  "\u0020or equal to 0");
            }
            if (endPos > jstring.Length)
            {
                throw new ArgumentException("endPos (" + endPos + ") is not less or" +
                  "\u0020equal to " + jstring.Length);
            }
            if (endPos < index)
            {
                throw new ArgumentException("endPos (" + endPos + ") is not greater" +
                  "\u0020or equal to " + index);
            }
#endif

            var cj = new CBORJson3(jstring, index, endPos, options);
            return cj.ParseJSON(nextchar);
        }

        private CBORObject ParseJSONObject(int depth)
        {
            // Assumes that the last character read was '{'
            if (depth > 1000)
            {
                this.RaiseError("Too deeply nested");
            }
            int c;
            CBORObject key = null;
            CBORObject obj;
            var nextchar = new int[1];
            var seenComma = false;
            var myHashMap = new SortedDictionary<CBORObject, CBORObject>();
            while (true)
            {
                c = this.SkipWhitespaceJSON();
                switch (c)
                {
                    case -1:
                        this.RaiseError("A JSON object must end with '}'");
                        break;
                    case '}':
                        if (seenComma)
                        {
                            // Situation like '{"0"=>1,}'
                            this.RaiseError("Trailing comma");
                            return null;
                        }
                        return CBORObject.FromRaw(myHashMap);
                    default:
                        {
                            // Read the next string
                            if (c < 0)
                            {
                                this.RaiseError("Unexpected end of data");
                                return null;
                            }
                            if (c != '"')
                            {
                                this.RaiseError("Expected a string as a key");
                                return null;
                            }
                            // Parse a string that represents the object's key
                            // The tokenizer already checked the string for invalid
                            // surrogate pairs, so just call the CBORObject
                            // constructor directly
                            obj = CBORObject.FromRaw(this.NextJSONString());
                            key = obj;
                            if (!this.options.AllowDuplicateKeys &&
                              myHashMap.ContainsKey(obj))
                            {
                                this.RaiseError("Key already exists: " + key);
                                return null;
                            }
                            break;
                        }
                }
                if (this.SkipWhitespaceJSON() != ':')
                {
                    this.RaiseError("Expected a ':' after a key");
                }
                // NOTE: Will overwrite existing value
                myHashMap[key] = this.NextJSONValue(
                    this.SkipWhitespaceJSON(),
                    nextchar,
                    depth);
                switch (nextchar[0])
                {
                    case ',':
                        seenComma = true;
                        break;
                    case '}':
                        return CBORObject.FromRaw(myHashMap);
                    default:
                        this.RaiseError("Expected a ',' or '}'");
                        break;
                }
            }
        }

        internal CBORObject ParseJSONArray(int depth)
        {
            // Assumes that the last character read was '['
            if (depth > 1000)
            {
                this.RaiseError("Too deeply nested");
            }
            var myArrayList = new List<CBORObject>();
            var seenComma = false;
            var nextchar = new int[1];
            while (true)
            {
                int c = this.SkipWhitespaceJSON();
                if (c == ']')
                {
                    if (seenComma)
                    {
                        // Situation like '[0,1,]'
                        this.RaiseError("Trailing comma");
                    }
                    return CBORObject.FromRaw(myArrayList);
                }
                if (c == ',')
                {
                    // Situation like '[,0,1,2]' or '[0,,1]'
                    this.RaiseError("Empty array element");
                }
                myArrayList.Add(
                  this.NextJSONValue(
                    c,
                    nextchar,
                    depth));
                c = nextchar[0];
                switch (c)
                {
                    case ',':
                        seenComma = true;
                        break;
                    case ']':
                        return CBORObject.FromRaw(myArrayList);
                    default:
                        this.RaiseError("Expected a ',' or ']'");
                        break;
                }
            }
        }
    }
    #endregion

    #region CBORJsonWriter
    internal static class CBORJsonWriter
    {
        private const string Hex16 = "0123456789ABCDEF";

        internal static void WriteJSONStringUnquoted(
          string str,
          StringOutput sb,
          JSONOptions options)
        {
            var i = 0;
            for (; i < str.Length; ++i)
            {
                char c = str[i];
                if (c < 0x20 || c >= 0x7f || c == '\\' || c == '"')
                {
                    sb.WriteString(str, 0, i);
                    break;
                }
            }
            if (i == str.Length)
            {
                sb.WriteString(str, 0, i);
                return;
            }
            // int bufferlen = Math.Min(Math.Max(4, str.Length), 64);
            // byte[] buffer = new byte[bufferlen];
            // int bufferpos = 0;
            for (; i < str.Length; ++i)
            {
                char c = str[i];
                if (c == '\\' || c == '"')
                {
                    sb.WriteCodePoint((int)'\\');
                    sb.WriteCodePoint((int)c);
                }
                else if (c < 0x20 || (c >= 0x7f && (c == 0x2028 || c == 0x2029 ||
                    (c >= 0x7f && c <= 0xa0) || c == 0xfeff || c == 0xfffe ||
                    c == 0xffff)))
                {
                    // Control characters, and also the line and paragraph separators
                    // which apparently can't appear in JavaScript (as opposed to
                    // JSON) strings
                    if (c == 0x0d)
                    {
                        sb.WriteString("\\r");
                    }
                    else if (c == 0x0a)
                    {
                        sb.WriteString("\\n");
                    }
                    else if (c == 0x08)
                    {
                        sb.WriteString("\\b");
                    }
                    else if (c == 0x0c)
                    {
                        sb.WriteString("\\f");
                    }
                    else if (c == 0x09)
                    {
                        sb.WriteString("\\t");
                    }
                    else if (c == 0x85)
                    {
                        sb.WriteString("\\u0085");
                    }
                    else if (c >= 0x100)
                    {
                        sb.WriteString("\\u");
                        sb.WriteCodePoint((int)Hex16[(int)((c >> 12) & 15)]);
                        sb.WriteCodePoint((int)Hex16[(int)((c >> 8) & 15)]);
                        sb.WriteCodePoint((int)Hex16[(int)((c >> 4) & 15)]);
                        sb.WriteCodePoint((int)Hex16[(int)(c & 15)]);
                    }
                    else
                    {
                        sb.WriteString("\\u00");
                        sb.WriteCodePoint((int)Hex16[(int)(c >> 4)]);
                        sb.WriteCodePoint((int)Hex16[(int)(c & 15)]);
                    }
                }
                else if ((c & 0xfc00) == 0xd800)
                {
                    if (i >= str.Length - 1 || (str[i + 1] & 0xfc00) != 0xdc00)
                    {
                        // NOTE: RFC 8259 doesn't prohibit any particular
                        // error-handling behavior when a writer of JSON
                        // receives a string with an unpaired surrogate.
                        if (options.ReplaceSurrogates)
                        {
                            // Replace unpaired surrogate with U+FFFD
                            sb.WriteCodePoint(0xfffd);
                        }
                        else
                        {
                            throw new CBORException("Unpaired surrogate in string");
                        }
                    }
                    else
                    {
                        sb.WriteString(str, i, 2);
                        ++i;
                    }
                }
                else
                {
                    sb.WriteCodePoint((int)c);
                }
            }
        }

        internal static void WriteJSONToInternal(
          CBORObject obj,
          StringOutput writer,
          JSONOptions options)
        {
            if (obj.Type == CBORType.Array || obj.Type == CBORType.Map)
            {
                var stack = new List<CBORObject>();
                WriteJSONToInternal(obj, writer, options, stack);
            }
            else
            {
                WriteJSONToInternal(obj, writer, options, null);
            }
        }

        private static void PopRefIfNeeded(
            IList<CBORObject> stack,
            bool pop)
        {
            if (pop && stack != null)
            {
                stack.RemoveAt(stack.Count - 1);
            }
        }

        private static bool CheckCircularRef(
          IList<CBORObject> stack,
          CBORObject parent,
          CBORObject child)
        {
            if (child.Type != CBORType.Array && child.Type != CBORType.Map)
            {
                return false;
            }
            CBORObject childUntag = child.Untag();
            if (parent.Untag() == childUntag)
            {
                throw new CBORException("Circular reference in CBOR object");
            }
            if (stack != null)
            {
                foreach (CBORObject o in stack)
                {
                    if (o.Untag() == childUntag)
                    {
                        throw new CBORException("Circular reference in CBOR object");
                    }
                }
            }
            stack.Add(child);
            return true;
        }

        internal static void WriteJSONToInternal(
          CBORObject obj,
          StringOutput writer,
          JSONOptions options,
          IList<CBORObject> stack)
        {
            if (obj.IsNumber)
            {
                writer.WriteString(CBORNumber.FromCBORObject(obj).ToJSONString());
                return;
            }
            switch (obj.Type)
            {
                case CBORType.Integer:
                case CBORType.FloatingPoint:
                    {
                        CBORObject untaggedObj = obj.Untag();
                        writer.WriteString(
                          CBORNumber.FromCBORObject(untaggedObj).ToJSONString());
                        break;
                    }
                case CBORType.Boolean:
                    {
                        if (obj.IsTrue)
                        {
                            writer.WriteString("true");
                            return;
                        }
                        if (obj.IsFalse)
                        {
                            writer.WriteString("false");
                            return;
                        }
                        return;
                    }
                case CBORType.SimpleValue:
                    {
                        writer.WriteString("null");
                        return;
                    }
                case CBORType.ByteString:
                    {
                        byte[] byteArray = obj.GetByteString();
                        if (byteArray.Length == 0)
                        {
                            writer.WriteString("\"\"");
                            return;
                        }
                        writer.WriteCodePoint((int)'\"');
                        if (obj.HasTag(22))
                        {
                            // Base64 with padding
                            Base64.WriteBase64(
                              writer,
                              byteArray,
                              0,
                              byteArray.Length,
                              true);
                        }
                        else if (obj.HasTag(23))
                        {
                            // Write as base16
                            for (int i = 0; i < byteArray.Length; ++i)
                            {
                                writer.WriteCodePoint((int)Hex16[(byteArray[i] >> 4) & 15]);
                                writer.WriteCodePoint((int)Hex16[byteArray[i] & 15]);
                            }
                        }
                        else
                        {
                            // Base64url no padding
                            Base64.WriteBase64URL(
                              writer,
                              byteArray,
                              0,
                              byteArray.Length,
                              false);
                        }
                        writer.WriteCodePoint((int)'\"');
                        break;
                    }
                case CBORType.TextString:
                    {
                        string thisString = obj.AsString();
                        if (thisString.Length == 0)
                        {
                            writer.WriteString("\"\"");
                            return;
                        }
                        writer.WriteCodePoint((int)'\"');
                        WriteJSONStringUnquoted(thisString, writer, options);
                        writer.WriteCodePoint((int)'\"');
                        break;
                    }
                case CBORType.Array:
                    {
                        writer.WriteCodePoint((int)'[');
                        for (var i = 0; i < obj.Count; ++i)
                        {
                            if (i > 0)
                            {
                                writer.WriteCodePoint((int)',');
                            }
                            bool pop = CheckCircularRef(stack, obj, obj[i]);
                            WriteJSONToInternal(obj[i], writer, options, stack);
                            PopRefIfNeeded(stack, pop);
                        }
                        writer.WriteCodePoint((int)']');
                        break;
                    }
                case CBORType.Map:
                    {
                        var first = true;
                        var hasNonStringKeys = false;
                        ICollection<KeyValuePair<CBORObject, CBORObject>> entries =
                          obj.Entries;
                        foreach (KeyValuePair<CBORObject, CBORObject> entry in entries)
                        {
                            CBORObject key = entry.Key;
                            if (key.Type != CBORType.TextString ||
                              key.IsTagged)
                            {
                                // treat a non-text-string item or a tagged item
                                // as having non-string keys
                                hasNonStringKeys = true;
                                break;
                            }
                        }
                        if (!hasNonStringKeys)
                        {
                            writer.WriteCodePoint((int)'{');
                            foreach (KeyValuePair<CBORObject, CBORObject> entry in entries)
                            {
                                CBORObject key = entry.Key;
                                CBORObject value = entry.Value;
                                if (!first)
                                {
                                    writer.WriteCodePoint((int)',');
                                }
                                writer.WriteCodePoint((int)'\"');
                                WriteJSONStringUnquoted(key.AsString(), writer, options);
                                writer.WriteCodePoint((int)'\"');
                                writer.WriteCodePoint((int)':');
                                bool pop = CheckCircularRef(stack, obj, value);
                                WriteJSONToInternal(value, writer, options, stack);
                                PopRefIfNeeded(stack, pop);
                                first = false;
                            }
                            writer.WriteCodePoint((int)'}');
                        }
                        else
                        {
                            // This map has non-string keys
                            IDictionary<string, CBORObject> stringMap = new
                            Dictionary<string, CBORObject>();
                            // Copy to a map with String keys, since
                            // some keys could be duplicates
                            // when serialized to strings
                            foreach (KeyValuePair<CBORObject, CBORObject> entry
                              in entries)
                            {
                                CBORObject key = entry.Key;
                                CBORObject value = entry.Value;
                                string str = null;
                                switch (key.Type)
                                {
                                    case CBORType.TextString:
                                        str = key.AsString();
                                        break;
                                    case CBORType.Array:
                                    case CBORType.Map:
                                        {
                                            var sb = new StringBuilder();
                                            var sw = new StringOutput(sb);
                                            bool pop = CheckCircularRef(stack, obj, key);
                                            WriteJSONToInternal(key, sw, options, stack);
                                            PopRefIfNeeded(stack, pop);
                                            str = sb.ToString();
                                            break;
                                        }
                                    default:
                                        str = key.ToJSONString(options);
                                        break;
                                }
                                if (stringMap.ContainsKey(str))
                                {
                                    throw new CBORException(
                                      "Duplicate JSON string equivalents of map" +
                                      "\u0020keys");
                                }
                                stringMap[str] = value;
                            }
                            first = true;
                            writer.WriteCodePoint((int)'{');
                            foreach (KeyValuePair<string, CBORObject> entry in stringMap)
                            {
                                string key = entry.Key;
                                CBORObject value = entry.Value;
                                if (!first)
                                {
                                    writer.WriteCodePoint((int)',');
                                }
                                writer.WriteCodePoint((int)'\"');
                                WriteJSONStringUnquoted((string)key, writer, options);
                                writer.WriteCodePoint((int)'\"');
                                writer.WriteCodePoint((int)':');
                                bool pop = CheckCircularRef(stack, obj, value);
                                WriteJSONToInternal(value, writer, options, stack);
                                PopRefIfNeeded(stack, pop);
                                first = false;
                            }
                            writer.WriteCodePoint((int)'}');
                        }
                        break;
                    }
                default:
                    throw new InvalidOperationException("Unexpected item" +
                      "\u0020type");
            }
        }
    }
    #endregion

    #region CBORNumber
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Design",
      "CA1036",
      Justification = "Arbitrary size.")]
    public sealed partial class CBORNumber : IComparable<CBORNumber>
    {
        /// <summary>Specifies the underlying form of this CBOR number
        /// object.</summary>
        public enum NumberKind
        {
            /// <summary>A 64-bit signed integer.</summary>
            Integer,

            /// <summary>A 64-bit binary floating-point number.</summary>
            Double,

            /// <summary>An arbitrary-precision integer.</summary>
            EInteger,

            /// <summary>An arbitrary-precision decimal number.</summary>
            EDecimal,

            /// <summary>An arbitrary-precision binary number.</summary>
            EFloat,

            /// <summary>An arbitrary-precision rational number.</summary>
            ERational,
        }

        private static readonly ICBORNumber[] NumberInterfaces = {
      new CBORInteger(),
      new CBORDoubleBits(),
      new CBOREInteger(),
      new CBORExtendedDecimal(),
      new CBORExtendedFloat(),
      new CBORExtendedRational(),
    };

        private readonly NumberKind kind;
        private readonly object value;
        internal CBORNumber(NumberKind kind, object value)
        {
            this.kind = kind;
            this.value = value;
        }

        internal ICBORNumber GetNumberInterface()
        {
            return GetNumberInterface(this.kind);
        }

        internal static ICBORNumber GetNumberInterface(CBORObject obj)
        {
            CBORNumber num = CBORNumber.FromCBORObject(obj);
            return (num == null) ? null : num.GetNumberInterface();
        }

        internal object GetValue()
        {
            return this.value;
        }

        internal static ICBORNumber GetNumberInterface(NumberKind kind)
        {
            switch (kind)
            {
                case NumberKind.Integer:
                    return NumberInterfaces[0];
                case NumberKind.Double:
                    return NumberInterfaces[1];
                case NumberKind.EInteger:
                    return NumberInterfaces[2];
                case NumberKind.EDecimal:
                    return NumberInterfaces[3];
                case NumberKind.EFloat:
                    return NumberInterfaces[4];
                case NumberKind.ERational:
                    return NumberInterfaces[5];
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>Converts this object's value to a CBOR object.</summary>
        /// <returns>A CBOR object that stores this object's value.</returns>
        public CBORObject ToCBORObject()
        {
            return CBORObject.FromObject(this.value);
        }

        /// <summary>Gets this value's sign: -1 if nonzero and negative; 1 if
        /// nonzero and positive; 0 if zero. Not-a-number (NaN) values are
        /// positive or negative depending on what sign is stored in their
        /// underlying forms.</summary>
        /// <value>This value's sign.</value>
        public int Sign
        {
            get
            {
                return this.IsNaN() ? (this.IsNegative() ? -1 : 1) :
                  this.GetNumberInterface().Sign(this.value);
            }
        }

        internal static bool IsNumber(CBORObject o)
        {
            if (IsUntaggedInteger(o))
            {
                return true;
            }
            else if (!o.IsTagged && o.Type == CBORType.FloatingPoint)
            {
                return true;
            }
            else if (o.HasOneTag(2) || o.HasOneTag(3))
            {
                return o.Type == CBORType.ByteString;
            }
            else if (o.HasOneTag(4) ||
       o.HasOneTag(5) ||
       o.HasOneTag(264) ||
       o.HasOneTag(265) ||
       o.HasOneTag(268) ||
       o.HasOneTag(269))
            {
                return CheckBigFracToNumber(o,
                    o.MostOuterTag.ToInt32Checked());
            }
            else if (o.HasOneTag(30) ||
            o.HasOneTag(270))
            {
                return CheckRationalToNumber(o,
                    o.MostOuterTag.ToInt32Checked());
            }
            else
            {
                return false;
            }
        }

        /// <summary>Creates a CBOR number object from a CBOR object
        /// representing a number (that is, one for which the IsNumber property
        /// in.NET or the isNumber() method in Java returns true).</summary>
        /// <param name='o'>The parameter is a CBOR object representing a
        /// number.</param>
        /// <returns>A CBOR number object, or null if the given CBOR object is
        /// null or does not represent a number.</returns>
        public static CBORNumber FromCBORObject(CBORObject o)
        {
            if (o == null)
            {
                return null;
            }
            if (IsUntaggedInteger(o))
            {
                if (o.CanValueFitInInt64())
                {
                    return new CBORNumber(NumberKind.Integer, o.AsInt64Value());
                }
                else
                {
                    return new CBORNumber(NumberKind.EInteger, o.AsEIntegerValue());
                }
            }
            else if (!o.IsTagged && o.Type == CBORType.FloatingPoint)
            {
                return CBORNumber.FromDoubleBits(o.AsDoubleBits());
            }
            if (o.HasOneTag(2) || o.HasOneTag(3))
            {
                return BignumToNumber(o);
            }
            else if (o.HasOneTag(4) ||
       o.HasOneTag(5) ||
       o.HasOneTag(264) ||
       o.HasOneTag(265) ||
       o.HasOneTag(268) ||
       o.HasOneTag(269))
            {
                return BigFracToNumber(o,
                    o.MostOuterTag.ToInt32Checked());
            }
            else if (o.HasOneTag(30) ||
            o.HasOneTag(270))
            {
                return RationalToNumber(o,
                    o.MostOuterTag.ToInt32Checked());
            }
            else
            {
                return null;
            }
        }

        private static bool IsUntaggedInteger(CBORObject o)
        {
            return !o.IsTagged && o.Type == CBORType.Integer;
        }

        private static bool IsUntaggedIntegerOrBignum(CBORObject o)
        {
            return IsUntaggedInteger(o) || ((o.HasOneTag(2) || o.HasOneTag(3)) &&
                o.Type == CBORType.ByteString);
        }

        private static EInteger IntegerOrBignum(CBORObject o)
        {
            if (IsUntaggedInteger(o))
            {
                return o.AsEIntegerValue();
            }
            else
            {
                CBORNumber n = BignumToNumber(o);
                return n.GetNumberInterface().AsEInteger(n.GetValue());
            }
        }

        private static CBORNumber RationalToNumber(
          CBORObject o,
          int tagName)
        {
            if (o.Type != CBORType.Array)
            {
                return null; // "Big fraction must be an array";
            }
            if (tagName == 270)
            {
                if (o.Count != 3)
                {
                    return null; // "Extended big fraction requires exactly 3 items";
                }
                if (!IsUntaggedInteger(o[2]))
                {
                    return null; // "Third item must be an integer";
                }
            }
            else
            {
                if (o.Count != 2)
                {
                    return null; // "Big fraction requires exactly 2 items";
                }
            }
            if (!IsUntaggedIntegerOrBignum(o[0]))
            {
                return null; // "Numerator is not an integer or bignum";
            }
            if (!IsUntaggedIntegerOrBignum(o[1]))
            {
                return null; // "Denominator is not an integer or bignum");
            }
            EInteger numerator = IntegerOrBignum(o[0]);
            EInteger denominator = IntegerOrBignum(o[1]);
            if (denominator.Sign <= 0)
            {
                return null; // "Denominator may not be negative or zero");
            }
            ERational erat = ERational.Create(numerator, denominator);
            if (tagName == 270)
            {
                if (numerator.Sign < 0)
                {
                    return null; // "Numerator may not be negative");
                }
                if (!o[2].CanValueFitInInt32())
                {
                    return null; // "Invalid options";
                }
                int options = o[2].AsInt32Value();
                switch (options)
                {
                    case 0:
                        break;
                    case 1:
                        erat = erat.Negate();
                        break;
                    case 2:
                        if (!numerator.IsZero || denominator.CompareTo(1) != 0)
                        {
                            return null; // "invalid values");
                        }
                        erat = ERational.PositiveInfinity;
                        break;
                    case 3:
                        if (!numerator.IsZero || denominator.CompareTo(1) != 0)
                        {
                            return null; // "invalid values");
                        }
                        erat = ERational.NegativeInfinity;
                        break;
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        if (denominator.CompareTo(1) != 0)
                        {
                            return null; // "invalid values");
                        }
                        erat = ERational.CreateNaN(
                            numerator,
                            options >= 6,
                            options == 5 || options == 7);
                        break;
                    default: return null; // "Invalid options");
                }
            }
            return CBORNumber.FromObject(erat);
        }

        private static bool CheckRationalToNumber(
          CBORObject o,
          int tagName)
        {
            if (o.Type != CBORType.Array)
            {
                return false;
            }
            if (tagName == 270)
            {
                if (o.Count != 3)
                {
                    return false;
                }
                if (!IsUntaggedInteger(o[2]))
                {
                    return false;
                }
            }
            else
            {
                if (o.Count != 2)
                {
                    return false;
                }
            }
            if (!IsUntaggedIntegerOrBignum(o[0]))
            {
                return false;
            }
            if (!IsUntaggedIntegerOrBignum(o[1]))
            {
                return false;
            }
            EInteger denominator = IntegerOrBignum(o[1]);
            if (denominator.Sign <= 0)
            {
                return false;
            }
            if (tagName == 270)
            {
                EInteger numerator = IntegerOrBignum(o[0]);
                if (numerator.Sign < 0 || !o[2].CanValueFitInInt32())
                {
                    return false;
                }
                int options = o[2].AsInt32Value();
                switch (options)
                {
                    case 0:
                    case 1:
                        return true;
                    case 2:
                    case 3:
                        return numerator.IsZero && denominator.CompareTo(1) == 0;
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        return denominator.CompareTo(1) == 0;
                    default:
                        return false;
                }
            }
            return true;
        }

        private static bool CheckBigFracToNumber(
          CBORObject o,
          int tagName)
        {
            if (o.Type != CBORType.Array)
            {
                return false;
            }
            if (tagName == 268 || tagName == 269)
            {
                if (o.Count != 3)
                {
                    return false;
                }
                if (!IsUntaggedInteger(o[2]))
                {
                    return false;
                }
            }
            else
            {
                if (o.Count != 2)
                {
                    return false;
                }
            }
            if (tagName == 4 || tagName == 5)
            {
                if (!IsUntaggedInteger(o[0]))
                {
                    return false;
                }
            }
            else
            {
                if (!IsUntaggedIntegerOrBignum(o[0]))
                {
                    return false;
                }
            }
            if (!IsUntaggedIntegerOrBignum(o[1]))
            {
                return false;
            }
            if (tagName == 268 || tagName == 269)
            {
                EInteger exponent = IntegerOrBignum(o[0]);
                EInteger mantissa = IntegerOrBignum(o[1]);
                if (mantissa.Sign < 0 || !o[2].CanValueFitInInt32())
                {
                    return false;
                }
                int options = o[2].AsInt32Value();
                switch (options)
                {
                    case 0:
                    case 1:
                        return true;
                    case 2:
                    case 3:
                        return exponent.IsZero && mantissa.IsZero;
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        return exponent.IsZero;
                    default:
                        return false;
                }
            }
            return true;
        }

        private static CBORNumber BigFracToNumber(
          CBORObject o,
          int tagName)
        {
            if (o.Type != CBORType.Array)
            {
                return null; // "Big fraction must be an array");
            }
            if (tagName == 268 || tagName == 269)
            {
                if (o.Count != 3)
                {
                    return null; // "Extended big fraction requires exactly 3 items");
                }
                if (!IsUntaggedInteger(o[2]))
                {
                    return null; // "Third item must be an integer");
                }
            }
            else
            {
                if (o.Count != 2)
                {
                    return null; // "Big fraction requires exactly 2 items");
                }
            }
            if (tagName == 4 || tagName == 5)
            {
                if (!IsUntaggedInteger(o[0]))
                {
                    return null; // "Exponent is not an integer");
                }
            }
            else
            {
                if (!IsUntaggedIntegerOrBignum(o[0]))
                {
                    return null; // "Exponent is not an integer or bignum");
                }
            }
            if (!IsUntaggedIntegerOrBignum(o[1]))
            {
                return null; // "Mantissa is not an integer or bignum");
            }
            EInteger exponent = IntegerOrBignum(o[0]);
            EInteger mantissa = IntegerOrBignum(o[1]);
            bool isdec = tagName == 4 || tagName == 264 || tagName == 268;
            EDecimal edec = isdec ? EDecimal.Create(mantissa, exponent) : null;
            EFloat efloat = !isdec ? EFloat.Create(mantissa, exponent) : null;
            if (tagName == 268 || tagName == 269)
            {
                if (mantissa.Sign < 0)
                {
                    return null; // "Mantissa may not be negative");
                }
                if (!o[2].CanValueFitInInt32())
                {
                    return null; // "Invalid options");
                }
                int options = o[2].AsInt32Value();
                switch (options)
                {
                    case 0:
                        break;
                    case 1:
                        if (isdec)
                        {
                            edec = edec.Negate();
                        }
                        else
                        {
                            efloat = efloat.Negate();
                        }
                        break;
                    case 2:
                        if (!exponent.IsZero || !mantissa.IsZero)
                        {
                            return null; // "invalid values");
                        }
                        if (isdec)
                        {
                            edec = EDecimal.PositiveInfinity;
                        }
                        else
                        {
                            efloat = EFloat.PositiveInfinity;
                        }
                        break;
                    case 3:
                        if (!exponent.IsZero || !mantissa.IsZero)
                        {
                            return null; // "invalid values");
                        }
                        if (isdec)
                        {
                            edec = EDecimal.NegativeInfinity;
                        }
                        else
                        {
                            efloat = EFloat.NegativeInfinity;
                        }
                        break;
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        if (!exponent.IsZero)
                        {
                            return null; // "invalid values");
                        }
                        if (isdec)
                        {
                            edec = EDecimal.CreateNaN(
                                mantissa,
                                options >= 6,
                                options == 5 || options == 7,
                                null);
                        }
                        else
                        {
                            efloat = EFloat.CreateNaN(
                                mantissa,
                                options >= 6,
                                options == 5 || options == 7,
                                null);
                        }
                        break;
                    default:
                        return null; // "Invalid options");
                }
            }
            if (isdec)
            {
                return CBORNumber.FromObject(edec);
            }
            else
            {
                return CBORNumber.FromObject(efloat);
            }
        }

        /// <summary>Gets the underlying form of this CBOR number
        /// object.</summary>
        /// <value>The underlying form of this CBOR number object.</value>
        public NumberKind Kind
        {
            get
            {
                return this.kind;
            }
        }

        /// <summary>Returns whether this object's value, converted to an
        /// integer by discarding its fractional part, would be -(2^31) or
        /// greater, and less than 2^31.</summary>
        /// <returns><c>true</c> if this object's value, converted to an
        /// integer by discarding its fractional part, would be -(2^31) or
        /// greater, and less than 2^31; otherwise, <c>false</c>.</returns>
        public bool CanTruncatedIntFitInInt32()
        {
            return
              this.GetNumberInterface().CanTruncatedIntFitInInt32(this.GetValue());
        }

        /// <summary>Returns whether this object's value, converted to an
        /// integer by discarding its fractional part, would be -(2^63) or
        /// greater, and less than 2^63.</summary>
        /// <returns><c>true</c> if this object's value, converted to an
        /// integer by discarding its fractional part, would be -(2^63) or
        /// greater, and less than 2^63; otherwise, <c>false</c>.</returns>
        public bool CanTruncatedIntFitInInt64()
        {
            switch (this.kind)
            {
                case NumberKind.Integer:
                    return true;
                default:
                    return

                      this.GetNumberInterface()
                      .CanTruncatedIntFitInInt64(this.GetValue());
            }
        }

        /// <summary>Returns whether this object's value can be converted to a
        /// 32-bit floating point number without its value being rounded to
        /// another numerical value.</summary>
        /// <returns><c>true</c> if this object's value can be converted to a
        /// 32-bit floating point number without its value being rounded to
        /// another numerical value, or if this is a not-a-number value, even
        /// if the value's diagnostic information can' t fit in a 32-bit
        /// floating point number; otherwise, <c>false</c>.</returns>
        public bool CanFitInSingle()
        {
            return this.GetNumberInterface().CanFitInSingle(this.GetValue());
        }

        /// <summary>Returns whether this object's value can be converted to a
        /// 64-bit floating point number without its value being rounded to
        /// another numerical value.</summary>
        /// <returns><c>true</c> if this object's value can be converted to a
        /// 64-bit floating point number without its value being rounded to
        /// another numerical value, or if this is a not-a-number value, even
        /// if the value's diagnostic information can't fit in a 64-bit
        /// floating point number; otherwise, <c>false</c>.</returns>
        public bool CanFitInDouble()
        {
            return this.GetNumberInterface().CanFitInDouble(this.GetValue());
        }

        /// <summary>Gets a value indicating whether this CBOR object
        /// represents a finite number.</summary>
        /// <returns><c>true</c> if this CBOR object represents a finite
        /// number; otherwise, <c>false</c>.</returns>
        public bool IsFinite()
        {
            switch (this.kind)
            {
                case NumberKind.Integer:
                case NumberKind.EInteger:
                    return true;
                default:
                    return !this.IsInfinity() && !this.IsNaN();
            }
        }

        /// <summary>Gets a value indicating whether this object represents an
        /// integer number, that is, a number without a fractional part.
        /// Infinity and not-a-number are not considered integers.</summary>
        /// <returns><c>true</c> if this object represents an integer number,
        /// that is, a number without a fractional part; otherwise,
        /// <c>false</c>.</returns>
        public bool IsInteger()
        {
            switch (this.kind)
            {
                case NumberKind.Integer:
                case NumberKind.EInteger:
                    return true;
                default:
                    return this.GetNumberInterface().IsIntegral(this.GetValue());
            }
        }

        /// <summary>Gets a value indicating whether this object is a negative
        /// number.</summary>
        /// <returns><c>true</c> if this object is a negative number;
        /// otherwise, <c>false</c>.</returns>
        public bool IsNegative()
        {
            return this.GetNumberInterface().IsNegative(this.GetValue());
        }

        /// <summary>Gets a value indicating whether this object's value equals
        /// 0.</summary>
        /// <returns><c>true</c> if this object's value equals 0; otherwise,
        /// <c>false</c>.</returns>
        public bool IsZero()
        {
            switch (this.kind)
            {
                case NumberKind.Integer:
                    {
                        var thisValue = (long)this.value;
                        return thisValue == 0;
                    }
                default: return this.GetNumberInterface().IsNumberZero(this.GetValue());
            }
        }

        /// <summary>Converts this object to an arbitrary-precision integer.
        /// See the ToObject overload taking a type for more
        /// information.</summary>
        /// <returns>The closest arbitrary-precision integer to this
        /// object.</returns>
        /// <exception cref='ArithmeticException'>This value is infinity or
        /// not-a-number.</exception>
        public EInteger ToEInteger()
        {
            return this.GetNumberInterface().AsEInteger(this.GetValue());
        }

        /// <summary>Converts this object to an arbitrary-precision integer if
        /// its value is an integer.</summary>
        /// <returns>The arbitrary-precision integer given by object.</returns>
        /// <exception cref='ArithmeticException'>This value is infinity or
        /// not-a-number or is not an exact integer.</exception>
        public EInteger ToEIntegerIfExact()
        {
            if (!this.IsInteger())
            {
                throw new ArithmeticException("Not an integer");
            }
            return this.ToEInteger();
        }

        // Begin integer conversions

        /// <summary>Converts this number's value to a byte (from 0 to 255) if
        /// it can fit in a byte (from 0 to 255) after converting it to an
        /// integer by discarding its fractional part.</summary>
        /// <returns>This number's value, truncated to a byte (from 0 to
        /// 255).</returns>
        /// <exception cref='OverflowException'>This value is infinity or
        /// not-a-number, or the number, once converted to an integer by
        /// discarding its fractional part, is less than 0 or greater than
        /// 255.</exception>
        public byte ToByteChecked()
        {
            if (!this.IsFinite())
            {
                throw new OverflowException("Value is infinity or NaN");
            }
            return this.ToEInteger().ToByteChecked();
        }

        /// <summary>Converts this number's value to an integer by discarding
        /// its fractional part, and returns the least-significant bits of its
        /// two's-complement form as a byte (from 0 to 255).</summary>
        /// <returns>This number, converted to a byte (from 0 to 255). Returns
        /// 0 if this value is infinity or not-a-number.</returns>
        public byte ToByteUnchecked()
        {
            return this.IsFinite() ? this.ToEInteger().ToByteUnchecked() : (byte)0;
        }

        /// <summary>Converts this number's value to a byte (from 0 to 255) if
        /// it can fit in a byte (from 0 to 255) without rounding to a
        /// different numerical value.</summary>
        /// <returns>This number's value as a byte (from 0 to 255).</returns>
        /// <exception cref='ArithmeticException'>This value is infinity or
        /// not-a-number, is not an exact integer, or is less than 0 or greater
        /// than 255.</exception>
        public byte ToByteIfExact()
        {
            if (!this.IsFinite())
            {
                throw new OverflowException("Value is infinity or NaN");
            }
            if (this.IsZero())
            {
                return (byte)0;
            }
            if (this.IsNegative())
            {
                throw new OverflowException("Value out of range");
            }
            return this.ToEIntegerIfExact().ToByteChecked();
        }

        /// <summary>Converts a byte (from 0 to 255) to an arbitrary-precision
        /// decimal number.</summary>
        /// <param name='inputByte'>The number to convert as a byte (from 0 to
        /// 255).</param>
        /// <returns>This number's value as an arbitrary-precision decimal
        /// number.</returns>
        public static CBORNumber FromByte(byte inputByte)
        {
            int val = ((int)inputByte) & 0xff;
            return FromObject((long)val);
        }

        /// <summary>Converts this number's value to a 16-bit signed integer if
        /// it can fit in a 16-bit signed integer after converting it to an
        /// integer by discarding its fractional part.</summary>
        /// <returns>This number's value, truncated to a 16-bit signed
        /// integer.</returns>
        /// <exception cref='OverflowException'>This value is infinity or
        /// not-a-number, or the number, once converted to an integer by
        /// discarding its fractional part, is less than -32768 or greater than
        /// 32767.</exception>
        public short ToInt16Checked()
        {
            if (!this.IsFinite())
            {
                throw new OverflowException("Value is infinity or NaN");
            }
            return this.ToEInteger().ToInt16Checked();
        }

        /// <summary>Converts this number's value to an integer by discarding
        /// its fractional part, and returns the least-significant bits of its
        /// two's-complement form as a 16-bit signed integer.</summary>
        /// <returns>This number, converted to a 16-bit signed integer. Returns
        /// 0 if this value is infinity or not-a-number.</returns>
        public short ToInt16Unchecked()
        {
            return this.IsFinite() ? this.ToEInteger().ToInt16Unchecked() : (short)0;
        }

        /// <summary>Converts this number's value to a 16-bit signed integer if
        /// it can fit in a 16-bit signed integer without rounding to a
        /// different numerical value.</summary>
        /// <returns>This number's value as a 16-bit signed integer.</returns>
        /// <exception cref='ArithmeticException'>This value is infinity or
        /// not-a-number, is not an exact integer, or is less than -32768 or
        /// greater than 32767.</exception>
        public short ToInt16IfExact()
        {
            if (!this.IsFinite())
            {
                throw new OverflowException("Value is infinity or NaN");
            }
            return this.IsZero() ? ((short)0) :
              this.ToEIntegerIfExact().ToInt16Checked();
        }

        /// <summary>Converts a 16-bit signed integer to an arbitrary-precision
        /// decimal number.</summary>
        /// <param name='inputInt16'>The number to convert as a 16-bit signed
        /// integer.</param>
        /// <returns>This number's value as an arbitrary-precision decimal
        /// number.</returns>
        public static CBORNumber FromInt16(short inputInt16)
        {
            var val = (int)inputInt16;
            return FromObject((long)val);
        }

        /// <summary>Converts this number's value to a 32-bit signed integer if
        /// it can fit in a 32-bit signed integer after converting it to an
        /// integer by discarding its fractional part.</summary>
        /// <returns>This number's value, truncated to a 32-bit signed
        /// integer.</returns>
        /// <exception cref='OverflowException'>This value is infinity or
        /// not-a-number, or the number, once converted to an integer by
        /// discarding its fractional part, is less than -2147483648 or greater
        /// than 2147483647.</exception>
        public int ToInt32Checked()
        {
            if (!this.IsFinite())
            {
                throw new OverflowException("Value is infinity or NaN");
            }
            return this.ToEInteger().ToInt32Checked();
        }

        /// <summary>Converts this number's value to an integer by discarding
        /// its fractional part, and returns the least-significant bits of its
        /// two's-complement form as a 32-bit signed integer.</summary>
        /// <returns>This number, converted to a 32-bit signed integer. Returns
        /// 0 if this value is infinity or not-a-number.</returns>
        public int ToInt32Unchecked()
        {
            return this.IsFinite() ? this.ToEInteger().ToInt32Unchecked() : (int)0;
        }

        /// <summary>Converts this number's value to a 32-bit signed integer if
        /// it can fit in a 32-bit signed integer without rounding to a
        /// different numerical value.</summary>
        /// <returns>This number's value as a 32-bit signed integer.</returns>
        /// <exception cref='ArithmeticException'>This value is infinity or
        /// not-a-number, is not an exact integer, or is less than -2147483648
        /// or greater than 2147483647.</exception>
        public int ToInt32IfExact()
        {
            if (!this.IsFinite())
            {
                throw new OverflowException("Value is infinity or NaN");
            }
            return this.IsZero() ? ((int)0) :
              this.ToEIntegerIfExact().ToInt32Checked();
        }

        /// <summary>Converts this number's value to a 64-bit signed integer if
        /// it can fit in a 64-bit signed integer after converting it to an
        /// integer by discarding its fractional part.</summary>
        /// <returns>This number's value, truncated to a 64-bit signed
        /// integer.</returns>
        /// <exception cref='OverflowException'>This value is infinity or
        /// not-a-number, or the number, once converted to an integer by
        /// discarding its fractional part, is less than -9223372036854775808
        /// or greater than 9223372036854775807.</exception>
        public long ToInt64Checked()
        {
            if (!this.IsFinite())
            {
                throw new OverflowException("Value is infinity or NaN");
            }
            return this.ToEInteger().ToInt64Checked();
        }

        /// <summary>Converts this number's value to an integer by discarding
        /// its fractional part, and returns the least-significant bits of its
        /// two's-complement form as a 64-bit signed integer.</summary>
        /// <returns>This number, converted to a 64-bit signed integer. Returns
        /// 0 if this value is infinity or not-a-number.</returns>
        public long ToInt64Unchecked()
        {
            return this.IsFinite() ? this.ToEInteger().ToInt64Unchecked() : 0L;
        }

        /// <summary>Converts this number's value to a 64-bit signed integer if
        /// it can fit in a 64-bit signed integer without rounding to a
        /// different numerical value.</summary>
        /// <returns>This number's value as a 64-bit signed integer.</returns>
        /// <exception cref='ArithmeticException'>This value is infinity or
        /// not-a-number, is not an exact integer, or is less than
        /// -9223372036854775808 or greater than
        /// 9223372036854775807.</exception>
        public long ToInt64IfExact()
        {
            if (!this.IsFinite())
            {
                throw new OverflowException("Value is infinity or NaN");
            }
            return this.IsZero() ? 0L :
              this.ToEIntegerIfExact().ToInt64Checked();
        }
        // End integer conversions
        private static CBORNumber BignumToNumber(CBORObject o)
        {
            if (o.Type != CBORType.ByteString)
            {
                return null; // "Byte array expected");
            }
            bool negative = o.HasMostInnerTag(3);
            byte[] data = o.GetByteString();
            if (data.Length <= 7)
            {
                long x = 0;
                for (var i = 0; i < data.Length; ++i)
                {
                    x <<= 8;
                    x |= ((long)data[i]) & 0xff;
                }
                if (negative)
                {
                    x = -x;
                    --x;
                }
                return new CBORNumber(NumberKind.Integer, x);
            }
            int neededLength = data.Length;
            byte[] bytes;
            EInteger bi;
            var extended = false;
            if (((data[0] >> 7) & 1) != 0)
            {
                // Increase the needed length
                // if the highest bit is set, to
                // distinguish negative and positive
                // values
                ++neededLength;
                extended = true;
            }
            bytes = new byte[neededLength];
            for (var i = 0; i < data.Length; ++i)
            {
                bytes[i] = data[data.Length - 1 - i];
                if (negative)
                {
                    bytes[i] = (byte)((~((int)bytes[i])) & 0xff);
                }
            }
            if (extended)
            {
                bytes[bytes.Length - 1] = negative ? (byte)0xff : (byte)0;
            }
            bi = EInteger.FromBytes(bytes, true);
            if (bi.CanFitInInt64())
            {
                return new CBORNumber(NumberKind.Integer, bi.ToInt64Checked());
            }
            else
            {
                return new CBORNumber(NumberKind.EInteger, bi);
            }
        }

        /// <summary>Returns the value of this object in text form.</summary>
        /// <returns>A text string representing the value of this
        /// object.</returns>
        public override string ToString()
        {
            switch (this.kind)
            {
                case NumberKind.Integer:
                    {
                        var longItem = (long)this.value;
                        return CBORUtilities.LongToString(longItem);
                    }
                case NumberKind.Double:
                    {
                        var longItem = (long)this.value;
                        return CBORUtilities.DoubleBitsToString(longItem);
                    }
                default:
                    return (this.value == null) ? String.Empty :
                      this.value.ToString();
            }
        }

        internal string ToJSONString()
        {
            switch (this.kind)
            {
                case NumberKind.Double:
                    {
                        var f = (long)this.value;
                        if (!CBORUtilities.DoubleBitsFinite(f))
                        {
                            return "null";
                        }
                        string dblString = CBORUtilities.DoubleBitsToString(f);
                        return CBORUtilities.TrimDotZero(dblString);
                    }
                case NumberKind.Integer:
                    {
                        var longItem = (long)this.value;
                        return CBORUtilities.LongToString(longItem);
                    }
                case NumberKind.EInteger:
                    {
                        object eiobj = this.value;
                        return ((EInteger)eiobj).ToString();
                    }
                case NumberKind.EDecimal:
                    {
                        var dec = (EDecimal)this.value;
                        if (dec.IsInfinity() || dec.IsNaN())
                        {
                            return "null";
                        }
                        else
                        {
                            return dec.ToString();
                        }
                    }
                case NumberKind.EFloat:
                    {
                        var flo = (EFloat)this.value;
                        if (flo.IsInfinity() || flo.IsNaN())
                        {
                            return "null";
                        }
                        if (flo.IsFinite &&
                          flo.Exponent.Abs().CompareTo((EInteger)2500) > 0)
                        {
                            // Too inefficient to convert to a decimal number
                            // from a bigfloat with a very high exponent,
                            // so convert to double instead
                            long f = flo.ToDoubleBits();
                            if (!CBORUtilities.DoubleBitsFinite(f))
                            {
                                return "null";
                            }
                            string dblString = CBORUtilities.DoubleBitsToString(f);
                            return CBORUtilities.TrimDotZero(dblString);
                        }
                        return flo.ToString();
                    }
                case NumberKind.ERational:
                    {
                        var dec = (ERational)this.value;
                        string nnstr = dec.Numerator.ToString();
                        string dnstr = dec.Denominator.ToString();
                        // DebugUtility.Log(
                        // "numlen="+nnstr.Length +
                        // " denlen="+dnstr.Length +
                        // "\nstart="+DateTime.UtcNow);
                        EDecimal f = dec.ToEDecimalExactIfPossible(
                            EContext.Decimal128.WithUnlimitedExponents());
                        // DebugUtility.Log(
                        // " end="+DateTime.UtcNow);
                        if (!f.IsFinite)
                        {
                            return "null";
                        }
                        else
                        {
                            return f.ToString();
                        }
                    }
                default: throw new InvalidOperationException();
            }
        }

        internal static CBORNumber FromObject(int intValue)
        {
            return new CBORNumber(NumberKind.Integer, (long)intValue);
        }
        internal static CBORNumber FromObject(long longValue)
        {
            return new CBORNumber(NumberKind.Integer, longValue);
        }
        internal static CBORNumber FromDoubleBits(long doubleBits)
        {
            return new CBORNumber(NumberKind.Double, doubleBits);
        }
        internal static CBORNumber FromObject(EInteger eivalue)
        {
            return new CBORNumber(NumberKind.EInteger, eivalue);
        }
        internal static CBORNumber FromObject(EFloat value)
        {
            return new CBORNumber(NumberKind.EFloat, value);
        }
        internal static CBORNumber FromObject(EDecimal value)
        {
            return new CBORNumber(NumberKind.EDecimal, value);
        }
        internal static CBORNumber FromObject(ERational value)
        {
            return new CBORNumber(NumberKind.ERational, value);
        }

        /// <summary>Returns whether this object's numerical value is an
        /// integer, is -(2^31) or greater, and is less than 2^31.</summary>
        /// <returns><c>true</c> if this object's numerical value is an
        /// integer, is -(2^31) or greater, and is less than 2^31; otherwise,
        /// <c>false</c>.</returns>
        public bool CanFitInInt32()
        {
            ICBORNumber icn = this.GetNumberInterface();
            object gv = this.GetValue();
            if (!icn.CanFitInInt64(gv))
            {
                return false;
            }
            long v = icn.AsInt64(gv);
            return v >= Int32.MinValue && v <= Int32.MaxValue;
        }

        /// <summary>Returns whether this object's numerical value is an
        /// integer, is -(2^63) or greater, and is less than 2^63.</summary>
        /// <returns><c>true</c> if this object's numerical value is an
        /// integer, is -(2^63) or greater, and is less than 2^63; otherwise,
        /// <c>false</c>.</returns>
        public bool CanFitInInt64()
        {
            return this.GetNumberInterface().CanFitInInt64(this.GetValue());
        }

        /// <summary>Gets a value indicating whether this object represents
        /// infinity.</summary>
        /// <returns><c>true</c> if this object represents infinity; otherwise,
        /// <c>false</c>.</returns>
        public bool IsInfinity()
        {
            return this.GetNumberInterface().IsInfinity(this.GetValue());
        }

        /// <summary>Gets a value indicating whether this object represents
        /// positive infinity.</summary>
        /// <returns><c>true</c> if this object represents positive infinity;
        /// otherwise, <c>false</c>.</returns>
        public bool IsPositiveInfinity()
        {
            return this.GetNumberInterface().IsPositiveInfinity(this.GetValue());
        }

        /// <summary>Gets a value indicating whether this object represents
        /// negative infinity.</summary>
        /// <returns><c>true</c> if this object represents negative infinity;
        /// otherwise, <c>false</c>.</returns>
        public bool IsNegativeInfinity()
        {
            return this.GetNumberInterface().IsNegativeInfinity(this.GetValue());
        }

        /// <summary>Gets a value indicating whether this object represents a
        /// not-a-number value.</summary>
        /// <returns><c>true</c> if this object represents a not-a-number
        /// value; otherwise, <c>false</c>.</returns>
        public bool IsNaN()
        {
            return this.GetNumberInterface().IsNaN(this.GetValue());
        }

        /// <summary>Converts this object to a decimal number.</summary>
        /// <returns>A decimal number for this object's value.</returns>
        public EDecimal ToEDecimal()
        {
            return this.GetNumberInterface().AsEDecimal(this.GetValue());
        }

        /// <summary>Converts this object to an arbitrary-precision binary
        /// floating point number. See the ToObject overload taking a type for
        /// more information.</summary>
        /// <returns>An arbitrary-precision binary floating-point number for
        /// this object's value.</returns>
        public EFloat ToEFloat()
        {
            return this.GetNumberInterface().AsEFloat(this.GetValue());
        }

        /// <summary>Converts this object to a rational number. See the
        /// ToObject overload taking a type for more information.</summary>
        /// <returns>A rational number for this object's value.</returns>
        public ERational ToERational()
        {
            return this.GetNumberInterface().AsERational(this.GetValue());
        }

        /// <summary>Returns the absolute value of this CBOR number.</summary>
        /// <returns>This object's absolute value without its negative
        /// sign.</returns>
        public CBORNumber Abs()
        {
            switch (this.kind)
            {
                case NumberKind.Integer:
                    {
                        var longValue = (long)this.value;
                        if (longValue == Int64.MinValue)
                        {
                            return FromObject(EInteger.FromInt64(longValue).Negate());
                        }
                        else
                        {
                            return longValue >= 0 ? this : new CBORNumber(
                                this.kind,
                                Math.Abs(longValue));
                        }
                    }
                case NumberKind.EInteger:
                    {
                        var eivalue = (EInteger)this.value;
                        return eivalue.Sign >= 0 ? this : FromObject(eivalue.Abs());
                    }
                default:
                    return new CBORNumber(this.kind,
                        this.GetNumberInterface().Abs(this.GetValue()));
            }
        }

        /// <summary>Returns a CBOR number with the same value as this one but
        /// with the sign reversed.</summary>
        /// <returns>A CBOR number with the same value as this one but with the
        /// sign reversed.</returns>
        public CBORNumber Negate()
        {
            switch (this.kind)
            {
                case NumberKind.Integer:
                    {
                        var longValue = (long)this.value;
                        if (longValue == 0)
                        {
                            return FromObject(EDecimal.NegativeZero);
                        }
                        else if (longValue == Int64.MinValue)
                        {
                            return FromObject(EInteger.FromInt64(longValue).Negate());
                        }
                        else
                        {
                            return new CBORNumber(this.kind, -longValue);
                        }
                    }
                case NumberKind.EInteger:
                    {
                        var eiValue = (EInteger)this.value;
                        if (eiValue.IsZero)
                        {
                            return FromObject(EDecimal.NegativeZero);
                        }
                        else
                        {
                            return FromObject(eiValue.Negate());
                        }
                    }
                default:
                    return new CBORNumber(this.kind,
                 this.GetNumberInterface().Negate(this.GetValue()));
            }
        }

        private static ERational CheckOverflow(
          ERational e1,
          ERational e2,
          ERational eresult)
        {
            if (e1.IsFinite && e2.IsFinite && eresult.IsNaN())
            {
                throw new OutOfMemoryException("Result might be too big to fit in" +
                  "\u0020memory");
            }
            return eresult;
        }

        private static EDecimal CheckOverflow(EDecimal e1, EDecimal e2, EDecimal
          eresult)
        {
            // DebugUtility.Log("ED e1.Exp="+e1.Exponent);
            // DebugUtility.Log("ED e2.Exp="+e2.Exponent);
            if (e1.IsFinite && e2.IsFinite && eresult.IsNaN())
            {
                throw new OutOfMemoryException("Result might be too big to fit in" +
                  "\u0020memory");
            }
            return eresult;
        }

        private static EFloat CheckOverflow(EFloat e1, EFloat e2, EFloat eresult)
        {
            if (e1.IsFinite && e2.IsFinite && eresult.IsNaN())
            {
                throw new OutOfMemoryException("Result might be too big to fit in" +
                  "\u0020memory");
            }
            return eresult;
        }

        private static NumberKind GetConvertKind(CBORNumber a, CBORNumber b)
        {
            NumberKind typeA = a.kind;
            NumberKind typeB = b.kind;
            NumberKind convertKind = NumberKind.EInteger;
            if (!a.IsFinite())
            {
                convertKind = (typeB == NumberKind.Integer || typeB ==
                    NumberKind.EInteger) ? ((typeA == NumberKind.Double) ?
        NumberKind.EFloat :
                    typeA) : ((typeB == NumberKind.Double) ? NumberKind.EFloat : typeB);
            }
            else if (!b.IsFinite())
            {
                convertKind = (typeA == NumberKind.Integer || typeA ==
                    NumberKind.EInteger) ? ((typeB == NumberKind.Double) ?
        NumberKind.EFloat :
                    typeB) : ((typeA == NumberKind.Double) ? NumberKind.EFloat : typeA);
            }
            else if (typeA == NumberKind.ERational || typeB ==
    NumberKind.ERational)
            {
                convertKind = NumberKind.ERational;
            }
            else if (typeA == NumberKind.EDecimal || typeB == NumberKind.EDecimal)
            {
                convertKind = NumberKind.EDecimal;
            }
            else
            {
                convertKind = (typeA == NumberKind.EFloat || typeB ==
                    NumberKind.EFloat ||
                    typeA == NumberKind.Double || typeB == NumberKind.Double) ?
                    NumberKind.EFloat : NumberKind.EInteger;
            }
            return convertKind;
        }

        /// <summary>Returns the sum of this number and another
        /// number.</summary>
        /// <param name='b'>The number to add with this one.</param>
        /// <returns>The sum of this number and another number.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='b'/> is null.</exception>
        /// <exception cref='OutOfMemoryException'>The exact result of the
        /// operation might be too big to fit in memory (or might require more
        /// than 2 gigabytes of memory to store).</exception>
        public CBORNumber Add(CBORNumber b)
        {
            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }
            CBORNumber a = this;
            object objA = a.value;
            object objB = b.value;
            NumberKind typeA = a.kind;
            NumberKind typeB = b.kind;
            if (typeA == NumberKind.Integer && typeB == NumberKind.Integer)
            {
                var valueA = (long)objA;
                var valueB = (long)objB;
                if ((valueA < 0 && valueB < Int64.MinValue - valueA) ||
                  (valueA > 0 && valueB > Int64.MaxValue - valueA))
                {
                    // would overflow, convert to EInteger
                    return CBORNumber.FromObject(
                        EInteger.FromInt64(valueA).Add(EInteger.FromInt64(valueB)));
                }
                return new CBORNumber(NumberKind.Integer, valueA + valueB);
            }
            NumberKind convertKind = GetConvertKind(a, b);
            if (convertKind == NumberKind.ERational)
            {
                // DebugUtility.Log("Rational/Rational");
                ERational e1 = GetNumberInterface(typeA).AsERational(objA);
                ERational e2 = GetNumberInterface(typeB).AsERational(objB);
                // DebugUtility.Log("conv Rational/Rational");
                return new CBORNumber(NumberKind.ERational,
                    CheckOverflow(
                      e1,
                      e2,
                      e1.Add(e2)));
            }
            if (convertKind == NumberKind.EDecimal)
            {
                // DebugUtility.Log("Decimal/Decimal");
                EDecimal e1 = GetNumberInterface(typeA).AsEDecimal(objA);
                EDecimal e2 = GetNumberInterface(typeB).AsEDecimal(objB);
                // DebugUtility.Log("ED e1.Exp="+e1.Exponent);
                // DebugUtility.Log("ED e2.Exp="+e2.Exponent);
                return new CBORNumber(NumberKind.EDecimal,
                    CheckOverflow(
                      e1,
                      e2,
                      e1.Add(e2)));
            }
            if (convertKind == NumberKind.EFloat)
            {
                // DebugUtility.Log("Float/Float");
                EFloat e1 = GetNumberInterface(typeA).AsEFloat(objA);
                EFloat e2 = GetNumberInterface(typeB).AsEFloat(objB);
                // DebugUtility.Log("EF e1.Exp="+e1.Exponent);
                // DebugUtility.Log("EF e2.Exp="+e2.Exponent);
                return new CBORNumber(NumberKind.EFloat,
                    CheckOverflow(
                      e1,
                      e2,
                      e1.Add(e2)));
            }
            else
            {
                // DebugUtility.Log("type=" + typeA + "/" + typeB + " finite=" +
                // (// this.IsFinite()) + "/" + (b.IsFinite()));
                EInteger b1 = GetNumberInterface(typeA).AsEInteger(objA);
                EInteger b2 = GetNumberInterface(typeB).AsEInteger(objB);
                return new CBORNumber(NumberKind.EInteger, b1 + (EInteger)b2);
            }
        }

        /// <summary>Returns a number that expresses this number minus
        /// another.</summary>
        /// <param name='b'>The second operand to the subtraction.</param>
        /// <returns>A CBOR number that expresses this number minus the given
        /// number.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='b'/> is null.</exception>
        /// <exception cref='OutOfMemoryException'>The exact result of the
        /// operation might be too big to fit in memory (or might require more
        /// than 2 gigabytes of memory to store).</exception>
        public CBORNumber Subtract(CBORNumber b)
        {
            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }
            CBORNumber a = this;
            object objA = a.value;
            object objB = b.value;
            NumberKind typeA = a.kind;
            NumberKind typeB = b.kind;
            if (typeA == NumberKind.Integer && typeB == NumberKind.Integer)
            {
                var valueA = (long)objA;
                var valueB = (long)objB;
                if ((valueB < 0 && Int64.MaxValue + valueB < valueA) ||
                  (valueB > 0 && Int64.MinValue + valueB > valueA))
                {
                    // would overflow, convert to EInteger
                    return CBORNumber.FromObject(
                        EInteger.FromInt64(valueA).Subtract(EInteger.FromInt64(
                            valueB)));
                }
                return new CBORNumber(NumberKind.Integer, valueA - valueB);
            }
            NumberKind convertKind = GetConvertKind(a, b);
            if (convertKind == NumberKind.ERational)
            {
                ERational e1 = GetNumberInterface(typeA).AsERational(objA);
                ERational e2 = GetNumberInterface(typeB).AsERational(objB);
                return new CBORNumber(NumberKind.ERational,
                    CheckOverflow(
                      e1,
                      e2,
                      e1.Subtract(e2)));
            }
            if (convertKind == NumberKind.EDecimal)
            {
                EDecimal e1 = GetNumberInterface(typeA).AsEDecimal(objA);
                EDecimal e2 = GetNumberInterface(typeB).AsEDecimal(objB);
                return new CBORNumber(NumberKind.EDecimal,
                    CheckOverflow(
                      e1,
                      e2,
                      e1.Subtract(e2)));
            }
            if (convertKind == NumberKind.EFloat)
            {
                EFloat e1 = GetNumberInterface(typeA).AsEFloat(objA);
                EFloat e2 = GetNumberInterface(typeB).AsEFloat(objB);
                return new CBORNumber(NumberKind.EFloat,
                    CheckOverflow(
                      e1,
                      e2,
                      e1.Subtract(e2)));
            }
            else
            {
                // DebugUtility.Log("type=" + typeA + "/" + typeB + " finite=" +
                // (// this.IsFinite()) + "/" + (b.IsFinite()));
                EInteger b1 = GetNumberInterface(typeA).AsEInteger(objA);
                EInteger b2 = GetNumberInterface(typeB).AsEInteger(objB);
                return new CBORNumber(NumberKind.EInteger, b1 - (EInteger)b2);
            }
        }

        /// <summary>Returns a CBOR number expressing the product of this
        /// number and the given number.</summary>
        /// <param name='b'>The second operand to the multiplication
        /// operation.</param>
        /// <returns>A number expressing the product of this number and the
        /// given number.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='b'/> is null.</exception>
        /// <exception cref='OutOfMemoryException'>The exact result of the
        /// operation might be too big to fit in memory (or might require more
        /// than 2 gigabytes of memory to store).</exception>
        public CBORNumber Multiply(CBORNumber b)
        {
            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }
            CBORNumber a = this;
            object objA = a.value;
            object objB = b.value;
            NumberKind typeA = a.kind;
            NumberKind typeB = b.kind;
            if (typeA == NumberKind.Integer && typeB == NumberKind.Integer)
            {
                var valueA = (long)objA;
                var valueB = (long)objB;
                bool apos = valueA > 0L;
                bool bpos = valueB > 0L;
                if (
                  (apos && ((!bpos && (Int64.MinValue / valueA) > valueB) ||
                      (bpos && valueA > (Int64.MaxValue / valueB)))) ||
                  (!apos && ((!bpos && valueA != 0L &&
                        (Int64.MaxValue / valueA) > valueB) ||
                      (bpos && valueA < (Int64.MinValue / valueB)))))
                {
                    // would overflow, convert to EInteger
                    var bvalueA = (EInteger)valueA;
                    var bvalueB = (EInteger)valueB;
                    return CBORNumber.FromObject(bvalueA * (EInteger)bvalueB);
                }
                return CBORNumber.FromObject(valueA * valueB);
            }
            NumberKind convertKind = GetConvertKind(a, b);
            if (convertKind == NumberKind.ERational)
            {
                ERational e1 = GetNumberInterface(typeA).AsERational(objA);
                ERational e2 = GetNumberInterface(typeB).AsERational(objB);
                return CBORNumber.FromObject(CheckOverflow(e1, e2, e1.Multiply(e2)));
            }
            if (convertKind == NumberKind.EDecimal)
            {
                EDecimal e1 = GetNumberInterface(typeA).AsEDecimal(objA);
                EDecimal e2 = GetNumberInterface(typeB).AsEDecimal(objB);
                return CBORNumber.FromObject(CheckOverflow(e1, e2, e1.Multiply(e2)));
            }
            if (convertKind == NumberKind.EFloat)
            {
                EFloat e1 = GetNumberInterface(typeA).AsEFloat(objA);
                EFloat e2 = GetNumberInterface(typeB).AsEFloat(objB);
                return new CBORNumber(NumberKind.EFloat,
                    CheckOverflow(
                      e1,
                      e2,
                      e1.Multiply(e2)));
            }
            else
            {
                // DebugUtility.Log("type=" + typeA + "/" + typeB + " finite=" +
                // (// this.IsFinite()) + "/" + (b.IsFinite()));
                EInteger b1 = GetNumberInterface(typeA).AsEInteger(objA);
                EInteger b2 = GetNumberInterface(typeB).AsEInteger(objB);
                return new CBORNumber(NumberKind.EInteger, b1 * (EInteger)b2);
            }
        }

        /// <summary>Returns the quotient of this number and another
        /// number.</summary>
        /// <param name='b'>The right-hand side (divisor) to the division
        /// operation.</param>
        /// <returns>The quotient of this number and another one.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='b'/> is null.</exception>
        /// <exception cref='OutOfMemoryException'>The exact result of the
        /// operation might be too big to fit in memory (or might require more
        /// than 2 gigabytes of memory to store).</exception>
        public CBORNumber Divide(CBORNumber b)
        {
            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }
            CBORNumber a = this;
            object objA = a.value;
            object objB = b.value;
            NumberKind typeA = a.kind;
            NumberKind typeB = b.kind;
            if (typeA == NumberKind.Integer && typeB == NumberKind.Integer)
            {
                var valueA = (long)objA;
                var valueB = (long)objB;
                if (valueB == 0)
                {
                    return (valueA == 0) ? CBORNumber.FromObject(EDecimal.NaN) :
                      ((valueA < 0) ? CBORNumber.FromObject(EDecimal.NegativeInfinity) :

                        CBORNumber.FromObject(EDecimal.PositiveInfinity));
                }
                if (valueA == Int64.MinValue && valueB == -1)
                {
                    return new CBORNumber(NumberKind.Integer, valueA).Negate();
                }
                long quo = valueA / valueB;
                long rem = valueA - (quo * valueB);
                return (rem == 0) ? new CBORNumber(NumberKind.Integer, quo) :
                  new CBORNumber(NumberKind.ERational,
                    ERational.Create(
                      (EInteger)valueA,
                      (EInteger)valueB));
            }
            NumberKind convertKind = GetConvertKind(a, b);
            if (convertKind == NumberKind.ERational)
            {
                ERational e1 = GetNumberInterface(typeA).AsERational(objA);
                ERational e2 = GetNumberInterface(typeB).AsERational(objB);
                return new CBORNumber(NumberKind.ERational,
                    CheckOverflow(
                      e1,
                      e2,
                      e1.Divide(e2)));
            }
            if (convertKind == NumberKind.EDecimal)
            {
                EDecimal e1 = GetNumberInterface(typeA).AsEDecimal(objA);
                EDecimal e2 = GetNumberInterface(typeB).AsEDecimal(objB);
                if (e1.IsZero && e2.IsZero)
                {
                    return new CBORNumber(NumberKind.EDecimal, EDecimal.NaN);
                }
                EDecimal eret = e1.Divide(e2, null);
                // If either operand is infinity or NaN, the result
                // is already exact. Likewise if the result is a finite number.
                if (!e1.IsFinite || !e2.IsFinite || eret.IsFinite)
                {
                    return new CBORNumber(NumberKind.EDecimal, eret);
                }
                ERational er1 = GetNumberInterface(typeA).AsERational(objA);
                ERational er2 = GetNumberInterface(typeB).AsERational(objB);
                return new CBORNumber(NumberKind.ERational,
                    CheckOverflow(
                      er1,
                      er2,
                      er1.Divide(er2)));
            }
            if (convertKind == NumberKind.EFloat)
            {
                EFloat e1 = GetNumberInterface(typeA).AsEFloat(objA);
                EFloat e2 = GetNumberInterface(typeB).AsEFloat(objB);
                if (e1.IsZero && e2.IsZero)
                {
                    return CBORNumber.FromObject(EDecimal.NaN);
                }
                EFloat eret = e1.Divide(e2, null);
                // If either operand is infinity or NaN, the result
                // is already exact. Likewise if the result is a finite number.
                if (!e1.IsFinite || !e2.IsFinite || eret.IsFinite)
                {
                    return CBORNumber.FromObject(eret);
                }
                ERational er1 = GetNumberInterface(typeA).AsERational(objA);
                ERational er2 = GetNumberInterface(typeB).AsERational(objB);
                return new CBORNumber(NumberKind.ERational,
                    CheckOverflow(
                      er1,
                      er2,
                      er1.Divide(er2)));
            }
            else
            {
                // DebugUtility.Log("type=" + typeA + "/" + typeB + " finite=" +
                // (// this.IsFinite()) + "/" + (b.IsFinite()));
                EInteger b1 = GetNumberInterface(typeA).AsEInteger(objA);
                EInteger b2 = GetNumberInterface(typeB).AsEInteger(objB);
                if (b2.IsZero)
                {
                    return b1.IsZero ? CBORNumber.FromObject(EDecimal.NaN) : ((b1.Sign <
                          0) ? CBORNumber.FromObject(EDecimal.NegativeInfinity) :
                        CBORNumber.FromObject(EDecimal.PositiveInfinity));
                }
                EInteger bigrem;
                EInteger bigquo;
                {
                    EInteger[] divrem = b1.DivRem(b2);
                    bigquo = divrem[0];
                    bigrem = divrem[1];
                }
                return bigrem.IsZero ? CBORNumber.FromObject(bigquo) :
                  new CBORNumber(NumberKind.ERational, ERational.Create(b1, b2));
            }
        }

        /// <summary>Returns the remainder when this number is divided by
        /// another number.</summary>
        /// <param name='b'>The right-hand side (dividend) of the remainder
        /// operation.</param>
        /// <returns>The remainder when this number is divided by the other
        /// number.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='b'/> is null.</exception>
        /// <exception cref='OutOfMemoryException'>The exact result of the
        /// operation might be too big to fit in memory (or might require more
        /// than 2 gigabytes of memory to store).</exception>
        public CBORNumber Remainder(CBORNumber b)
        {
            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }
            object objA = this.value;
            object objB = b.value;
            NumberKind typeA = this.kind;
            NumberKind typeB = b.kind;
            if (typeA == NumberKind.Integer && typeB == NumberKind.Integer)
            {
                var valueA = (long)objA;
                var valueB = (long)objB;
                return (valueA == Int64.MinValue && valueB == -1) ?
                  CBORNumber.FromObject(0) : CBORNumber.FromObject(valueA % valueB);
            }
            NumberKind convertKind = GetConvertKind(this, b);
            if (convertKind == NumberKind.ERational)
            {
                ERational e1 = GetNumberInterface(typeA).AsERational(objA);
                ERational e2 = GetNumberInterface(typeB).AsERational(objB);
                return CBORNumber.FromObject(CheckOverflow(e1, e2, e1.Remainder(e2)));
            }
            if (convertKind == NumberKind.EDecimal)
            {
                EDecimal e1 = GetNumberInterface(typeA).AsEDecimal(objA);
                EDecimal e2 = GetNumberInterface(typeB).AsEDecimal(objB);
                return CBORNumber.FromObject(CheckOverflow(e1, e2, e1.Remainder(e2,
                        null)));
            }
            if (convertKind == NumberKind.EFloat)
            {
                EFloat e1 = GetNumberInterface(typeA).AsEFloat(objA);
                EFloat e2 = GetNumberInterface(typeB).AsEFloat(objB);
                return CBORNumber.FromObject(CheckOverflow(e1, e2, e1.Remainder(e2,
                        null)));
            }
            else
            {
                // DebugUtility.Log("type=" + typeA + "/" + typeB + " finite=" +
                // (// this.IsFinite()) + "/" + (b.IsFinite()));
                EInteger b1 = GetNumberInterface(typeA).AsEInteger(objA);
                EInteger b2 = GetNumberInterface(typeB).AsEInteger(objB);
                return CBORNumber.FromObject(b1 % (EInteger)b2);
            }
        }

        /// <summary>Compares two CBOR numbers. In this implementation, the two
        /// numbers' mathematical values are compared. Here, NaN (not-a-number)
        /// is considered greater than any number.</summary>
        /// <param name='other'>A value to compare with. Can be null.</param>
        /// <returns>A negative number, if this value is less than the other
        /// object; or 0, if both values are equal; or a positive number, if
        /// this value is less than the other object or if the other object is
        /// null.
        /// <para>This implementation returns a positive number if <paramref
        /// name='other'/> is null, to conform to the.NET definition of
        /// CompareTo. This is the case even in the Java version of this
        /// library, for consistency's sake, even though implementations of
        /// <c>Comparable.compareTo()</c> in Java ought to throw an exception
        /// if they receive a null argument rather than treating null as less
        /// or greater than any object.</para>.</returns>
        public int CompareTo(CBORNumber other)
        {
            if (other == null)
            {
                return 1;
            }
            if (this == other)
            {
                return 0;
            }
            var cmp = 0;
            NumberKind typeA = this.kind;
            NumberKind typeB = other.kind;
            object objA = this.value;
            object objB = other.value;
            if (typeA == typeB)
            {
                switch (typeA)
                {
                    case NumberKind.Integer:
                        {
                            var a = (long)objA;
                            var b = (long)objB;
                            cmp = (a == b) ? 0 : ((a < b) ? -1 : 1);
                            break;
                        }
                    case NumberKind.EInteger:
                        {
                            var bigintA = (EInteger)objA;
                            var bigintB = (EInteger)objB;
                            cmp = bigintA.CompareTo(bigintB);
                            break;
                        }
                    case NumberKind.Double:
                        {
                            var a = (long)objA;
                            var b = (long)objB;
                            // Treat NaN as greater than all other numbers
                            cmp = CBORUtilities.DoubleBitsNaN(a) ?
                              (CBORUtilities.DoubleBitsNaN(b) ? 0 : 1) :
                              (CBORUtilities.DoubleBitsNaN(b) ?
                                -1 : (((a < 0) != (b < 0)) ? ((a < b) ? -1 : 1) :
                                  ((a == b) ? 0 : (((a < b) ^ (a < 0)) ? -1 : 1))));
                            break;
                        }
                    case NumberKind.EDecimal:
                        {
                            cmp = ((EDecimal)objA).CompareTo((EDecimal)objB);
                            break;
                        }
                    case NumberKind.EFloat:
                        {
                            cmp = ((EFloat)objA).CompareTo(
                                (EFloat)objB);
                            break;
                        }
                    case NumberKind.ERational:
                        {
                            cmp = ((ERational)objA).CompareTo(
                                (ERational)objB);
                            break;
                        }
                    default:
                        throw new InvalidOperationException(
                   "Unexpected data type");
                }
            }
            else
            {
                int s1 = GetNumberInterface(typeA).Sign(objA);
                int s2 = GetNumberInterface(typeB).Sign(objB);
                if (s1 != s2 && s1 != 2 && s2 != 2)
                {
                    // if both types are numbers
                    // and their signs are different
                    return (s1 < s2) ? -1 : 1;
                }
                if (s1 == 2 && s2 == 2)
                {
                    // both are NaN
                    cmp = 0;
                }
                else if (s1 == 2)
                {
                    // first object is NaN
                    return 1;
                }
                else if (s2 == 2)
                {
                    // second object is NaN
                    return -1;
                }
                else
                {
                    // DebugUtility.Log("a=" + this + " b=" + other);
                    if (typeA == NumberKind.ERational)
                    {
                        ERational e1 =
                          GetNumberInterface(typeA).AsERational(objA);
                        if (typeB == NumberKind.EDecimal)
                        {
                            EDecimal e2 =
                              GetNumberInterface(typeB).AsEDecimal(objB);
                            cmp = e1.CompareToDecimal(e2);
                        }
                        else
                        {
                            EFloat e2 = GetNumberInterface(typeB).AsEFloat(objB);
                            cmp = e1.CompareToBinary(e2);
                        }
                    }
                    else if (typeB == NumberKind.ERational)
                    {
                        ERational e2 = GetNumberInterface(typeB).AsERational(objB);
                        if (typeA == NumberKind.EDecimal)
                        {
                            EDecimal e1 =
                              GetNumberInterface(typeA).AsEDecimal(objA);
                            cmp = e2.CompareToDecimal(e1);
                            cmp = -cmp;
                        }
                        else
                        {
                            EFloat e1 =
                              GetNumberInterface(typeA).AsEFloat(objA);
                            cmp = e2.CompareToBinary(e1);
                            cmp = -cmp;
                        }
                    }
                    else if (typeA == NumberKind.EDecimal ||
                    typeB == NumberKind.EDecimal)
                    {
                        EDecimal e1 = null;
                        EDecimal e2 = null;
                        if (typeA == NumberKind.EFloat)
                        {
                            var ef1 = (EFloat)objA;
                            e2 = (EDecimal)objB;
                            cmp = e2.CompareToBinary(ef1);
                            cmp = -cmp;
                        }
                        else if (typeB == NumberKind.EFloat)
                        {
                            var ef1 = (EFloat)objB;
                            e2 = (EDecimal)objA;
                            cmp = e2.CompareToBinary(ef1);
                        }
                        else
                        {
                            e1 = GetNumberInterface(typeA).AsEDecimal(objA);
                            e2 = GetNumberInterface(typeB).AsEDecimal(objB);
                            cmp = e1.CompareTo(e2);
                        }
                    }
                    else if (typeA == NumberKind.EFloat || typeB ==
                    NumberKind.EFloat || typeA == NumberKind.Double || typeB ==
                    NumberKind.Double)
                    {
                        EFloat e1 =
                          GetNumberInterface(typeA).AsEFloat(objA);
                        EFloat e2 = GetNumberInterface(typeB).AsEFloat(objB);
                        cmp = e1.CompareTo(e2);
                    }
                    else
                    {
                        EInteger b1 = GetNumberInterface(typeA).AsEInteger(objA);
                        EInteger b2 = GetNumberInterface(typeB).AsEInteger(objB);
                        cmp = b1.CompareTo(b2);
                    }
                }
            }
            return cmp;
        }
    }

    public sealed partial class CBORNumber
    {
        /* The "==" and "!=" operators are not overridden in the .NET version to be
          consistent with Equals, for the following reason: Objects with this
        type can have arbitrary size (e.g., they can
          be arbitrary-precision integers), and
        comparing
          two of them for equality can be much more complicated and take much
          more time than the default behavior of reference equality.
        */

        /// <summary>Returns whether one object's value is less than
        /// another's.</summary>
        /// <param name='a'>The left-hand side of the comparison.</param>
        /// <param name='b'>The right-hand side of the comparison.</param>
        /// <returns><c>true</c> if the first object's value is less than the
        /// other's; otherwise, <c>false</c>.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='a'/> is null.</exception>
        public static bool operator <(CBORNumber a, CBORNumber b)
        {
            return a == null ? b != null : a.CompareTo(b) < 0;
        }

        /// <summary>Returns whether one object's value is up to
        /// another's.</summary>
        /// <param name='a'>The left-hand side of the comparison.</param>
        /// <param name='b'>The right-hand side of the comparison.</param>
        /// <returns><c>true</c> if one object's value is up to another's;
        /// otherwise, <c>false</c>.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='a'/> is null.</exception>
        public static bool operator <=(CBORNumber a, CBORNumber b)
        {
            return a == null || a.CompareTo(b) <= 0;
        }

        /// <summary>Returns whether one object's value is greater than
        /// another's.</summary>
        /// <param name='a'>The left-hand side of the comparison.</param>
        /// <param name='b'>The right-hand side of the comparison.</param>
        /// <returns><c>true</c> if one object's value is greater than
        /// another's; otherwise, <c>false</c>.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='a'/> is null.</exception>
        public static bool operator >(CBORNumber a, CBORNumber b)
        {
            return a != null && a.CompareTo(b) > 0;
        }

        /// <summary>Returns whether one object's value is at least
        /// another's.</summary>
        /// <param name='a'>The left-hand side of the comparison.</param>
        /// <param name='b'>The right-hand side of the comparison.</param>
        /// <returns><c>true</c> if one object's value is at least another's;
        /// otherwise, <c>false</c>.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='a'/> is null.</exception>
        public static bool operator >=(CBORNumber a, CBORNumber b)
        {
            return a == null ? b == null : a.CompareTo(b) >= 0;
        }

        /// <summary>Converts this number's value to an 8-bit signed integer if
        /// it can fit in an 8-bit signed integer after converting it to an
        /// integer by discarding its fractional part.</summary>
        /// <returns>This number's value, truncated to an 8-bit signed
        /// integer.</returns>
        /// <exception cref='OverflowException'>This value is infinity or
        /// not-a-number, or the number, once converted to an integer by
        /// discarding its fractional part, is less than -128 or greater than
        /// 127.</exception>
        public sbyte ToSByteChecked()
        {
            if (!this.IsFinite())
            {
                throw new OverflowException("Value is infinity or NaN");
            }
            return this.ToEInteger().ToSByteChecked();
        }

        /// <summary>Converts this number's value to an integer by discarding
        /// its fractional part, and returns the least-significant bits of its
        /// two's-complement form as an 8-bit signed integer.</summary>
        /// <returns>This number, converted to an 8-bit signed integer. Returns
        /// 0 if this value is infinity or not-a-number.</returns>
        public sbyte ToSByteUnchecked()
        {
            return this.IsFinite() ? this.ToEInteger().ToSByteUnchecked() : (sbyte)0;
        }

        /// <summary>Converts this number's value to an 8-bit signed integer if
        /// it can fit in an 8-bit signed integer without rounding to a
        /// different numerical value.</summary>
        /// <returns>This number's value as an 8-bit signed integer.</returns>
        /// <exception cref='ArithmeticException'>This value is infinity or
        /// not-a-number, is not an exact integer, or is less than -128 or
        /// greater than 127.</exception>
        public sbyte ToSByteIfExact()
        {
            if (!this.IsFinite())
            {
                throw new OverflowException("Value is infinity or NaN");
            }
            return this.IsZero() ? ((sbyte)0) :
      this.ToEIntegerIfExact().ToSByteChecked();
        }

        /// <summary>Converts this number's value to a 16-bit unsigned integer
        /// if it can fit in a 16-bit unsigned integer after converting it to
        /// an integer by discarding its fractional part.</summary>
        /// <returns>This number's value, truncated to a 16-bit unsigned
        /// integer.</returns>
        /// <exception cref='OverflowException'>This value is infinity or
        /// not-a-number, or the number, once converted to an integer by
        /// discarding its fractional part, is less than 0 or greater than
        /// 65535.</exception>
        public ushort ToUInt16Checked()
        {
            if (!this.IsFinite())
            {
                throw new OverflowException("Value is infinity or NaN");
            }
            return this.ToEInteger().ToUInt16Checked();
        }

        /// <summary>Converts this number's value to an integer by discarding
        /// its fractional part, and returns the least-significant bits of its
        /// two's-complement form as a 16-bit unsigned integer.</summary>
        /// <returns>This number, converted to a 16-bit unsigned integer.
        /// Returns 0 if this value is infinity or not-a-number.</returns>
        public ushort ToUInt16Unchecked()
        {
            return this.IsFinite() ? this.ToEInteger().ToUInt16Unchecked() :
      (ushort)0;
        }

        /// <summary>Converts this number's value to a 16-bit unsigned integer
        /// if it can fit in a 16-bit unsigned integer without rounding to a
        /// different numerical value.</summary>
        /// <returns>This number's value as a 16-bit unsigned
        /// integer.</returns>
        /// <exception cref='ArithmeticException'>This value is infinity or
        /// not-a-number, is not an exact integer, or is less than 0 or greater
        /// than 65535.</exception>
        public ushort ToUInt16IfExact()
        {
            if (!this.IsFinite())
            {
                throw new OverflowException("Value is infinity or NaN");
            }
            if (this.IsZero())
            {
                return (ushort)0;
            }
            if (this.IsNegative())
            {
                throw new OverflowException("Value out of range");
            }
            return this.ToEIntegerIfExact().ToUInt16Checked();
        }

        /// <summary>Converts this number's value to a 32-bit signed integer if
        /// it can fit in a 32-bit signed integer after converting it to an
        /// integer by discarding its fractional part.</summary>
        /// <returns>This number's value, truncated to a 32-bit signed
        /// integer.</returns>
        /// <exception cref='OverflowException'>This value is infinity or
        /// not-a-number, or the number, once converted to an integer by
        /// discarding its fractional part, is less than 0 or greater than
        /// 4294967295.</exception>
        public uint ToUInt32Checked()
        {
            if (!this.IsFinite())
            {
                throw new OverflowException("Value is infinity or NaN");
            }
            return this.ToEInteger().ToUInt32Checked();
        }

        /// <summary>Converts this number's value to an integer by discarding
        /// its fractional part, and returns the least-significant bits of its
        /// two's-complement form as a 32-bit signed integer.</summary>
        /// <returns>This number, converted to a 32-bit signed integer. Returns
        /// 0 if this value is infinity or not-a-number.</returns>
        public uint ToUInt32Unchecked()
        {
            return this.IsFinite() ? this.ToEInteger().ToUInt32Unchecked() : 0U;
        }

        /// <summary>Converts this number's value to a 32-bit signed integer if
        /// it can fit in a 32-bit signed integer without rounding to a
        /// different numerical value.</summary>
        /// <returns>This number's value as a 32-bit signed integer.</returns>
        /// <exception cref='ArithmeticException'>This value is infinity or
        /// not-a-number, is not an exact integer, or is less than 0 or greater
        /// than 4294967295.</exception>
        public uint ToUInt32IfExact()
        {
            if (!this.IsFinite())
            {
                throw new OverflowException("Value is infinity or NaN");
            }
            if (this.IsZero())
            {
                return 0U;
            }
            if (this.IsNegative())
            {
                throw new OverflowException("Value out of range");
            }
            return this.ToEIntegerIfExact().ToUInt32Checked();
        }

        /// <summary>Converts this number's value to a 64-bit unsigned integer
        /// if it can fit in a 64-bit unsigned integer after converting it to
        /// an integer by discarding its fractional part.</summary>
        /// <returns>This number's value, truncated to a 64-bit unsigned
        /// integer.</returns>
        /// <exception cref='OverflowException'>This value is infinity or
        /// not-a-number, or the number, once converted to an integer by
        /// discarding its fractional part, is less than 0 or greater than
        /// 18446744073709551615.</exception>
        public ulong ToUInt64Checked()
        {
            if (!this.IsFinite())
            {
                throw new OverflowException("Value is infinity or NaN");
            }
            return this.ToEInteger().ToUInt64Checked();
        }

        /// <summary>Converts this number's value to an integer by discarding
        /// its fractional part, and returns the least-significant bits of its
        /// two's-complement form as a 64-bit unsigned integer.</summary>
        /// <returns>This number, converted to a 64-bit unsigned integer.
        /// Returns 0 if this value is infinity or not-a-number.</returns>
        public ulong ToUInt64Unchecked()
        {
            return this.IsFinite() ? this.ToEInteger().ToUInt64Unchecked() : 0UL;
        }

        /// <summary>Converts this number's value to a 64-bit unsigned integer
        /// if it can fit in a 64-bit unsigned integer without rounding to a
        /// different numerical value.</summary>
        /// <returns>This number's value as a 64-bit unsigned
        /// integer.</returns>
        /// <exception cref='ArithmeticException'>This value is infinity or
        /// not-a-number, is not an exact integer, or is less than 0 or greater
        /// than 18446744073709551615.</exception>
        public ulong ToUInt64IfExact()
        {
            if (!this.IsFinite())
            {
                throw new OverflowException("Value is infinity or NaN");
            }
            if (this.IsZero())
            {
                return 0UL;
            }
            if (this.IsNegative())
            {
                throw new OverflowException("Value out of range");
            }
            return this.ToEIntegerIfExact().ToUInt64Checked();
        }
    }
    #endregion

    /*
    #region CBORTag0
    internal class CBORTag0 : ICBORTag, ICBORToFromConverter<DateTime>
    {
        private static string DateTimeToString(DateTime bi)
        {
            int[] lesserFields = new int[7];
            EInteger[] year = new EInteger[1];
            PropertyMap.BreakDownDateTime(bi, year, lesserFields);
            // TODO: Change to true in next major version
            return CBORUtilities.ToAtomDateTimeString(year[0], lesserFields, false);
        }

        internal static void AddConverter()
        {
            // TODO: FromObject with Dates has different behavior
            // in Java version, which has to be retained until
            // the next major version for backward compatibility.
            // However, since ToObject is new, we can convert
            // to Date in the .NET and Java versions
            if (PropertyMap.DateTimeCompatHack)
            {
                CBORObject.AddConverter(typeof(DateTime), new CBORTag0());
            }
        }

        public CBORTypeFilter GetTypeFilter()
        {
            return CBORTypeFilter.TextString;
        }

        public CBORObject ValidateObject(CBORObject obj)
        {
            if (obj.Type != CBORType.TextString)
            {
                throw new CBORException("Not a text string");
            }
            return obj;
        }

        public DateTime FromCBORObject(CBORObject obj)
        {
            if (obj.HasMostOuterTag(0))
            {
                return StringToDateTime(obj.AsString());
            }
            else if (obj.HasMostOuterTag(1))
            {
                if (!obj.IsFinite)
                {
                    throw new CBORException("Not a finite number");
                }
                EDecimal dec = obj.AsEDecimal();
                int[] lesserFields = new int[7];
                EInteger[] year = new EInteger[1];
                CBORUtilities.BreakDownSecondsSinceEpoch(
                        dec,
                        year,
                        lesserFields);
                return PropertyMap.BuildUpDateTime(year[0], lesserFields);
            }
            throw new CBORException("Not tag 0 or 1");
        }

        public static DateTime StringToDateTime(string str)
        {
            int[] lesserFields = new int[7];
            EInteger[] year = new EInteger[1];
            CBORUtilities.ParseAtomDateTimeString(str, year, lesserFields);
            return PropertyMap.BuildUpDateTime(year[0], lesserFields);
        }

        public CBORObject ToCBORObject(DateTime obj)
        {
            return CBORObject.FromObjectAndTag(DateTimeToString(obj), 0);
        }
    }

    #endregion

    #region CBORTag1
    internal class CBORTag1 : ICBORTag, ICBORConverter<DateTime>
    {
        public CBORTypeFilter GetTypeFilter()
        {
            return
            CBORTypeFilter.UnsignedInteger.WithNegativeInteger().WithFloatingPoint();
        }

        public CBORObject ValidateObject(CBORObject obj)
        {
            if (!obj.IsFinite)
            {
                throw new CBORException("Not a valid date");
            }
            return obj;
        }

        public CBORObject ToCBORObject(DateTime obj)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
    #endregion

    #region CBORTag2
    internal class CBORTag2 : ICBORTag
    {
        public CBORTypeFilter GetTypeFilter()
        {
            return CBORTypeFilter.ByteString;
        }

        private static CBORObject FromObjectAndInnerTags(
      object objectValue,
      CBORObject objectWithTags)
        {
            CBORObject newObject = CBORObject.FromObject(objectValue);
            if (!objectWithTags.IsTagged)
            {
                return newObject;
            }
            objectWithTags = objectWithTags.UntagOne();
            if (!objectWithTags.IsTagged)
            {
                return newObject;
            }
            EInteger[] tags = objectWithTags.GetAllTags();
            for (int i = tags.Length - 1; i >= 0; --i)
            {
                newObject = CBORObject.FromObjectAndTag(newObject, tags[i]);
            }
            return newObject;
        }

        internal static CBORObject ConvertToBigNum(CBORObject o, bool negative)
        {
            if (o.Type != CBORType.ByteString)
            {
                throw new CBORException("Byte array expected");
            }
            byte[] data = o.GetByteString();
            if (data.Length <= 7)
            {
                long x = 0;
                for (int i = 0; i < data.Length; ++i)
                {
                    x <<= 8;
                    x |= ((long)data[i]) & 0xff;
                }
                if (negative)
                {
                    x = -x;
                    --x;
                }
                return FromObjectAndInnerTags(x, o);
            }
            int neededLength = data.Length;
            byte[] bytes;
            EInteger bi;
            bool extended = false;
            if (((data[0] >> 7) & 1) != 0)
            {
                // Increase the needed length
                // if the highest bit is set, to
                // distinguish negative and positive
                // values
                ++neededLength;
                extended = true;
            }
            bytes = new byte[neededLength];
            for (int i = 0; i < data.Length; ++i)
            {
                bytes[i] = data[data.Length - 1 - i];
                if (negative)
                {
                    bytes[i] = (byte)((~((int)bytes[i])) & 0xff);
                }
            }
            if (extended)
            {
                bytes[bytes.Length - 1] = negative ? (byte)0xff : (byte)0;
            }
            bi = EInteger.FromBytes(bytes, true);
            // NOTE: Here, any tags are discarded; when called from
            // the Read method, "o" will have no tags anyway (beyond tag 2),
            // and when called from FromObjectAndTag, we prefer
            // flexibility over throwing an error if the input
            // object contains other tags. The tag 2 is also discarded
            // because we are returning a "natively" supported CBOR object.
            return CBORObject.FromObject(bi);
        }

        public CBORObject ValidateObject(CBORObject obj)
        {
            return ConvertToBigNum(obj, false);
        }
    }
    #endregion

    #region CBORTag3
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:CBORTag3"]/*'/>
    internal class CBORTag3 : ICBORTag
    {
        public CBORTypeFilter GetTypeFilter()
        {
            return CBORTypeFilter.ByteString;
        }

        public CBORObject ValidateObject(CBORObject obj)
        {
            return CBORTag2.ConvertToBigNum(obj, true);
        }
    }

    #endregion

    #region CBORTag4
    internal class CBORTag4 : ICBORTag
    {
        public CBORTag4() : this(false)
        {
        }

        private readonly bool extended;

        public CBORTag4(bool extended)
        {
            this.extended = extended;
        }

        public CBORTypeFilter GetTypeFilter()
        {
            return this.extended ? CBORTag5.ExtendedFilter : CBORTag5.Filter;
        }

        public CBORObject ValidateObject(CBORObject obj)
        {
            return CBORTag5.ConvertToDecimalFrac(obj, true, this.extended);
        }
    }

    #endregion

    #region CBORTag5
    internal class CBORTag5 : ICBORTag
    {
        internal static readonly CBORTypeFilter Filter = new
        CBORTypeFilter().WithArrayExactLength(
      2,
      CBORTypeFilter.UnsignedInteger.WithNegativeInteger(),
      CBORTypeFilter.UnsignedInteger.WithNegativeInteger().WithTags(2, 3));

        internal static readonly CBORTypeFilter ExtendedFilter = new
        CBORTypeFilter().WithArrayExactLength(
      2,
      CBORTypeFilter.UnsignedInteger.WithNegativeInteger().WithTags(2, 3),
      CBORTypeFilter.UnsignedInteger.WithNegativeInteger().WithTags(2, 3));

        public CBORTag5() : this(false)
        {
        }

        private readonly bool extended;

        public CBORTag5(bool extended)
        {
            this.extended = extended;
        }

        public CBORTypeFilter GetTypeFilter()
        {
            return this.extended ? ExtendedFilter : Filter;
        }

        internal static CBORObject ConvertToDecimalFrac(
          CBORObject o,
          bool isDecimal,
          bool extended)
        {
            if (o.Type != CBORType.Array)
            {
                throw new CBORException("Big fraction must be an array");
            }
            if (o.Count != 2)
            {
                throw new CBORException("Big fraction requires exactly 2 items");
            }
            if (!o[0].IsIntegral)
            {
                throw new CBORException("Exponent is not an integer");
            }
            if (!o[1].IsIntegral)
            {
                throw new CBORException("Mantissa is not an integer");
            }
            EInteger exponent = o[0].AsEInteger();
            EInteger mantissa = o[1].AsEInteger();
            if (exponent.GetSignedBitLength() > 64 && !extended)
            {
                throw new CBORException("Exponent is too big");
            }
            if (exponent.IsZero)
            {
                // Exponent is 0, so return mantissa instead
                return CBORObject.FromObject(mantissa);
            }
            // NOTE: Discards tags. See comment in CBORTag2.
            return isDecimal ?
            CBORObject.FromObject(EDecimal.Create(mantissa, exponent)) :
            CBORObject.FromObject(EFloat.Create(mantissa, exponent));
        }

        public CBORObject ValidateObject(CBORObject obj)
        {
            return ConvertToDecimalFrac(obj, false, this.extended);
        }
    }
    #endregion

    #region CBORTag26
    internal class CBORTag26 : ICBORTag
    {
        public CBORTypeFilter GetTypeFilter()
        {
            return new CBORTypeFilter().WithArrayMinLength(1, CBORTypeFilter.Any);
        }

        public CBORObject ValidateObject(CBORObject obj)
        {
            if (obj.Type != CBORType.Array || obj.Count < 1)
            {
                throw new CBORException("Not an array, or is an empty array.");
            }
            return obj;
        }
    }

    #endregion

    #region CBORTag27
    internal class CBORTag27 : ICBORTag
    {
        public CBORTypeFilter GetTypeFilter()
        {
            return new CBORTypeFilter().WithArrayMinLength(1, CBORTypeFilter.Any);
        }

        public CBORObject ValidateObject(CBORObject obj)
        {
            if (obj.Type != CBORType.Array || obj.Count < 1)
            {
                throw new CBORException("Not an array, or is an empty array.");
            }
            return obj;
        }
    }
    #endregion

    #region CBORTag30
    internal class CBORTag30 : ICBORTag
    {
        public CBORTypeFilter GetTypeFilter()
        {
            return new CBORTypeFilter().WithArrayExactLength(
              2,
              CBORTypeFilter.UnsignedInteger.WithNegativeInteger().WithTags(2, 3),
              CBORTypeFilter.UnsignedInteger.WithTags(2));
        }

        public CBORObject ValidateObject(CBORObject obj)
        {
            if (obj.Type != CBORType.Array)
            {
                throw new CBORException("Rational number must be an array");
            }
            if (obj.Count != 2)
            {
                throw new CBORException("Rational number requires exactly 2 items");
            }
            CBORObject first = obj[0];
            CBORObject second = obj[1];
            if (!first.IsIntegral)
            {
                throw new CBORException("Rational number requires integer numerator");
            }
            if (!second.IsIntegral)
            {
                throw new CBORException("Rational number requires integer denominator");
            }
            if (second.Sign <= 0)
            {
                throw new CBORException("Rational number requires denominator greater than 0");
            }
            EInteger denom = second.AsEInteger();
            // NOTE: Discards tags. See comment in CBORTag2.
            return denom.Equals(EInteger.One) ?
            CBORObject.FromObject(first.AsEInteger()) :
            CBORObject.FromObject(
        ERational.Create(
        first.AsEInteger(),
        denom));
        }
    }
    #endregion

    #region CBORTag32
    internal class CBORTag32 : ICBORTag, ICBORConverter<Uri>
    {
        public CBORTypeFilter GetTypeFilter()
        {
            return CBORTypeFilter.TextString;
        }

        public CBORObject ValidateObject(CBORObject obj)
        {
            if (obj.Type != CBORType.TextString)
            {
                throw new CBORException("URI must be a text string");
            }
            if (!URIUtility.IsValidIRI(obj.AsString()))
            {
                throw new CBORException("String is not a valid URI/IRI");
            }
            return obj;
        }

        internal static void AddConverter()
        {
            CBORObject.AddConverter(typeof(Uri), new CBORTag32());
        }

        public CBORObject ToCBORObject(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }
            string uriString = uri.ToString();
            return CBORObject.FromObjectAndTag(uriString, (int)32);
        }
    }
    #endregion

    #region CBORTag37
    internal class CBORTag37 : ICBORTag, ICBORToFromConverter<Guid>
    {
        public CBORTypeFilter GetTypeFilter()
        {
            return CBORTypeFilter.ByteString;
        }

        public CBORObject ValidateObject(CBORObject obj)
        {
            if (obj.Type != CBORType.ByteString)
            {
                throw new CBORException("UUID must be a byte string");
            }
            byte[] bytes = obj.GetByteString();
            if (bytes.Length != 16)
            {
                throw new CBORException("UUID must be 16 bytes long");
            }
            return obj;
        }

        internal static void AddConverter()
        {
            CBORObject.AddConverter(typeof(Guid), new CBORTag37());
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTag37.ToCBORObject(System.Guid)"]/*'/>
        public CBORObject ToCBORObject(Guid obj)
        {
            byte[] bytes = PropertyMap.UUIDToBytes(obj);
            return CBORObject.FromObjectAndTag(bytes, (int)37);
        }

        public Guid FromCBORObject(CBORObject obj)
        {
            if (!obj.HasMostOuterTag(37))
            {
                throw new CBORException("Must have outermost tag 37");
            }
            this.ValidateObject(obj);
            byte[] bytes = obj.GetByteString();
            char[] guidChars = new char[36];
            string hex = "0123456789abcdef";
            int index = 0;
            for (int i = 0; i < 16; ++i)
            {
                if (i == 4 || i == 6 || i == 8 || i == 10)
                {
                    guidChars[index++] = '-';
                }
                guidChars[index++] = hex[(int)(bytes[i] >> 4) & 15];
                guidChars[index++] = hex[(int)bytes[i] & 15];
            }
            string guidString = new String(guidChars);
            return new Guid(guidString);
        }
    }
    #endregion

    #region CBORTagAny
    internal class CBORTagAny : ICBORTag
    {
        public CBORTypeFilter GetTypeFilter()
        {
            return CBORTypeFilter.Any;
        }

        public CBORObject ValidateObject(CBORObject obj)
        {
#if DEBUG
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
#endif
            return obj;
        }
    }

    #endregion

    #region CBORTagUnsigned
    internal class CBORTagUnsigned : ICBORTag
    {
        public CBORTypeFilter GetTypeFilter()
        {
            return CBORTypeFilter.UnsignedInteger;
        }

        public CBORObject ValidateObject(CBORObject obj)
        {
#if DEBUG
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
#endif
            if (!obj.IsIntegral || !obj.CanFitInInt64() || obj.Sign < 0)
            {
                throw new CBORException("Not a 64-bit unsigned integer");
            }
            return obj;
        }
    }
    #endregion
    */

    #region CBORType
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:CBORType"]/*'/>
    public enum CBORType
    {
        /// <summary>This property is no longer used.</summary>
        [Obsolete("Since version 4.0, CBORObject.Type no longer returns this" +
    "\u0020value for any CBOR object - this is a breaking change from " +
    "earlier versions." +
    "\u0020Instead, use the IsNumber property of CBORObject to determine" +
    " whether a CBOR object represents a number, or use the two " +
    "new CBORType values instead. CBORType.Integer " +
    "covers CBOR objects representing" +
    "\u0020integers of" +
    "\u0020major type 0 and 1. " +
    "CBORType.FloatingPoint covers CBOR objects representing " +
    "16-, 32-, and 64-bit floating-point numbers. CBORType.Number " +
    "may be removed in version 5.0 or later.")]
        Number,

        /// <summary>The simple values true and false.</summary>
        Boolean,

        /// <summary>A "simple value" other than floating point values, true,
        /// and false.</summary>
        SimpleValue,

        /// <summary>An array of bytes.</summary>
        ByteString,

        /// <summary>A text string.</summary>
        TextString,

        /// <summary>An array of CBOR objects.</summary>
        Array,

        /// <summary>A map of CBOR objects.</summary>
        Map,

        /// <summary>An integer in the interval [-(2^64), 2^64 - 1], or an
        /// integer of major type 0 and 1.</summary>
        Integer,

        /// <summary>A 16-, 32-, or 64-bit binary floating-point
        /// number.</summary>
        FloatingPoint,
    }
    #endregion

    #region CBORTagGenericString
    /*
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:CBORTagGenericString"]/*'/>
    internal class CBORTagGenericString : ICBORTag
    {
        public CBORTypeFilter GetTypeFilter()
        {
            return CBORTypeFilter.TextString;
        }

        public CBORObject ValidateObject(CBORObject obj)
        {
            if (obj.Type == CBORType.TextString)
            {
                throw new CBORException("Not a text string");
            }
            return obj;
        }
    }
    */
    #endregion

    /*
    #region CBORTypeFilter
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:CBORTypeFilter"]/*'/>
    public sealed class CBORTypeFilter
    {
        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="F:CBORTypeFilter.Any"]/*'/>
        public static readonly CBORTypeFilter Any = new CBORTypeFilter().WithAny();

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="F:CBORTypeFilter.ByteString"]/*'/>
        public static readonly CBORTypeFilter ByteString = new
          CBORTypeFilter().WithByteString();

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="F:CBORTypeFilter.NegativeInteger"]/*'/>
        public static readonly CBORTypeFilter NegativeInteger = new
          CBORTypeFilter().WithNegativeInteger();

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="F:CBORTypeFilter.None"]/*'/>
        public static readonly CBORTypeFilter None = new CBORTypeFilter();

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="F:CBORTypeFilter.TextString"]/*'/>
        public static readonly CBORTypeFilter TextString = new
          CBORTypeFilter().WithTextString();

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="F:CBORTypeFilter.UnsignedInteger"]/*'/>
        public static readonly CBORTypeFilter UnsignedInteger = new
          CBORTypeFilter().WithUnsignedInteger();

        private bool any;
        private bool anyArrayLength;
        private int arrayLength;
        private bool arrayMinLength;
        private CBORTypeFilter[] elements;
        private bool floatingpoint;
        private EInteger[] tags;
        private int types;

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeFilter.ArrayIndexAllowed(System.Int32)"]/*'/>
        public bool ArrayIndexAllowed(int index)
        {
            return (this.types & (1 << 4)) != 0 && index >= 0 &&
                 (this.anyArrayLength ||
                 ((this.arrayMinLength || index < this.arrayLength) && index >=
                             0));
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeFilter.ArrayLengthMatches(System.Int32)"]/*'/>
        public bool ArrayLengthMatches(int length)
        {
            return (this.types & (1 << 4)) != 0 && (this.anyArrayLength ||
                      (this.arrayMinLength ? this.arrayLength >= length :
                      this.arrayLength == length));
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeFilter.ArrayLengthMatches(System.Int64)"]/*'/>
        public bool ArrayLengthMatches(long length)
        {
            return (this.types & (1 << 4)) != 0 && (this.anyArrayLength ||
                      (this.arrayMinLength ? this.arrayLength >= length :
                      this.arrayLength == length));
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeFilter.ArrayLengthMatches(PeterO.Numbers.EInteger)"]/*'/>
        public bool ArrayLengthMatches(EInteger bigLength)
        {
            if (bigLength == null)
            {
                throw new ArgumentNullException(nameof(bigLength));
            }
            return ((this.types & (1 << 4)) == 0) && (this.anyArrayLength ||
              ((!this.arrayMinLength &&
              bigLength.CompareTo((EInteger)this.arrayLength) == 0) ||
              (this.arrayMinLength &&
              bigLength.CompareTo((EInteger)this.arrayLength) >= 0)));
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeFilter.GetSubFilter(System.Int32)"]/*'/>
        public CBORTypeFilter GetSubFilter(int index)
        {
            if (this.anyArrayLength || this.any)
            {
                return Any;
            }
            if (index < 0)
            {
                return None;
            }
            if (index >= this.arrayLength)
            {
                // Index is out of bounds
                return this.arrayMinLength ? Any : None;
            }
            if (this.elements == null)
            {
                return Any;
            }
            if (index >= this.elements.Length)
            {
                // Index is greater than the number of elements for
                // which a type is defined
                return Any;
            }
            return this.elements[index];
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeFilter.GetSubFilter(System.Int64)"]/*'/>
        public CBORTypeFilter GetSubFilter(long index)
        {
            if (this.anyArrayLength || this.any)
            {
                return Any;
            }
            if (index < 0)
            {
                return None;
            }
            if (index >= this.arrayLength)
            {
                // Index is out of bounds
                return this.arrayMinLength ? Any : None;
            }
            if (this.elements == null)
            {
                return Any;
            }
            // NOTE: Index shouldn't be greater than Int32.MaxValue,
            // since the length is an int
            if (index >= this.elements.Length)
            {
                // Index is greater than the number of elements for
                // which a type is defined
                return Any;
            }
            return this.elements[(int)index];
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeFilter.MajorTypeMatches(System.Int32)"]/*'/>
        public bool MajorTypeMatches(int type)
        {
#if DEBUG
            if (type < 0)
            {
                throw new ArgumentException("type (" + type + ") is less than 0");
            }
            if (type > 7)
            {
                throw new ArgumentException("type (" + type + ") is more than " + "7");
            }
#endif

            return type >= 0 && type <= 7 && (this.types & (1 << type)) != 0;
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeFilter.NonFPSimpleValueAllowed"]/*'/>
        public bool NonFPSimpleValueAllowed()
        {
            return this.MajorTypeMatches(7) && !this.floatingpoint;
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeFilter.TagAllowed(System.Int32)"]/*'/>
        public bool TagAllowed(int tag)
        {
            return this.any || this.TagAllowed((EInteger)tag);
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeFilter.TagAllowed(System.Int64)"]/*'/>
        public bool TagAllowed(long longTag)
        {
            return this.any || this.TagAllowed((EInteger)longTag);
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeFilter.TagAllowed(PeterO.Numbers.EInteger)"]/*'/>
        public bool TagAllowed(EInteger bigTag)
        {
            if (bigTag == null)
            {
                throw new ArgumentNullException(nameof(bigTag));
            }
            if (bigTag.Sign < 0)
            {
                return false;
            }
            if (this.any)
            {
                return true;
            }
            if ((this.types & (1 << 6)) == 0)
            {
                return false;
            }
            if (this.tags == null)
            {
                return true;
            }
            foreach (EInteger tag in this.tags)
            {
                if (bigTag.Equals(tag))
                {
                    return true;
                }
            }
            return false;
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeFilter.WithArrayAnyLength"]/*'/>
        public CBORTypeFilter WithArrayAnyLength()
        {
            if (this.any)
            {
                return this;
            }
            if (this.arrayLength < 0)
            {
                throw new ArgumentException("this.arrayLength (" + this.arrayLength +
                  ") is less than 0");
            }
            if (this.arrayLength < this.elements.Length)
            {
                throw new ArgumentException("this.arrayLength (" + this.arrayLength +
                  ") is less than " + this.elements.Length);
            }
            CBORTypeFilter filter = this.Copy();
            filter.types |= 1 << 4;
            filter.anyArrayLength = true;
            return filter;
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeFilter.WithArrayExactLength(System.Int32,CBORTypeFilter[])"]/*'/>
        public CBORTypeFilter WithArrayExactLength(
      int arrayLength,
      params CBORTypeFilter[] elements)
        {
            if (this.any)
            {
                return this;
            }
            if (elements == null)
            {
                throw new ArgumentNullException(nameof(elements));
            }
            if (arrayLength < 0)
            {
                throw new ArgumentException("arrayLength (" + arrayLength +
                  ") is less than 0");
            }
            if (arrayLength < elements.Length)
            {
                throw new ArgumentException("arrayLength (" + arrayLength +
                  ") is less than " + elements.Length);
            }
            CBORTypeFilter filter = this.Copy();
            filter.types |= 1 << 4;
            filter.arrayLength = arrayLength;
            filter.arrayMinLength = false;
            filter.elements = new CBORTypeFilter[elements.Length];
            Array.Copy(elements, filter.elements, elements.Length);
            return filter;
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeFilter.WithArrayMinLength(System.Int32,CBORTypeFilter[])"]/*'/>
        public CBORTypeFilter WithArrayMinLength(
      int arrayLength,
      params CBORTypeFilter[] elements)
        {
            if (this.any)
            {
                return this;
            }
            if (elements == null)
            {
                throw new ArgumentNullException(nameof(elements));
            }
            if (arrayLength < 0)
            {
                throw new ArgumentException("arrayLength (" + arrayLength +
                  ") is less than 0");
            }
            if (arrayLength < elements.Length)
            {
                throw new ArgumentException("arrayLength (" + arrayLength +
                  ") is less than " + elements.Length);
            }
            CBORTypeFilter filter = this.Copy();
            filter.types |= 1 << 4;
            filter.arrayLength = arrayLength;
            filter.arrayMinLength = true;
            filter.elements = new CBORTypeFilter[elements.Length];
            Array.Copy(elements, filter.elements, elements.Length);
            return filter;
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeFilter.WithByteString"]/*'/>
        public CBORTypeFilter WithByteString()
        {
            return this.WithType(2).WithTags(25);
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeFilter.WithFloatingPoint"]/*'/>
        public CBORTypeFilter WithFloatingPoint()
        {
            if (this.any)
            {
                return this;
            }
            CBORTypeFilter filter = this.Copy();
            filter.types |= 1 << 4;
            filter.floatingpoint = true;
            return filter;
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeFilter.WithMap"]/*'/>
        public CBORTypeFilter WithMap()
        {
            return this.WithType(5);
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeFilter.WithNegativeInteger"]/*'/>
        public CBORTypeFilter WithNegativeInteger()
        {
            return this.WithType(1);
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeFilter.WithTags(System.Int32[])"]/*'/>
        public CBORTypeFilter WithTags(params int[] tags)
        {
            if (this.any)
            {
                return this;
            }
            CBORTypeFilter filter = this.Copy();
            filter.types |= 1 << 6;  // Always include the "tag" major type
            int startIndex = 0;
            if (filter.tags != null)
            {
                EInteger[] newTags = new EInteger[tags.Length + filter.tags.Length];
                Array.Copy(filter.tags, newTags, filter.tags.Length);
                startIndex = filter.tags.Length;
                filter.tags = newTags;
            }
            else
            {
                filter.tags = new EInteger[tags.Length];
            }
            for (int i = 0; i < tags.Length; ++i)
            {
                filter.tags[startIndex + i] = (EInteger)tags[i];
            }
            return filter;
        }

        internal CBORTypeFilter WithTags(params EInteger[] tags)
        {
            if (this.any)
            {
                return this;
            }
            for (int i = 0; i < tags.Length; ++i)
            {
                if (tags[i] == null)
                {
                    throw new ArgumentNullException(nameof(tags));
                }
            }
            CBORTypeFilter filter = this.Copy();
            filter.types |= 1 << 6;  // Always include the "tag" major type
            int startIndex = 0;
            if (filter.tags != null)
            {
                EInteger[] newTags = new EInteger[tags.Length + filter.tags.Length];
                Array.Copy(filter.tags, newTags, filter.tags.Length);
                startIndex = filter.tags.Length;
                filter.tags = newTags;
            }
            else
            {
                filter.tags = new EInteger[tags.Length];
            }
            Array.Copy(tags, 0, filter.tags, startIndex, tags.Length);
            return filter;
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeFilter.WithTextString"]/*'/>
        public CBORTypeFilter WithTextString()
        {
            return this.WithType(3).WithTags(25);
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeFilter.WithUnsignedInteger"]/*'/>
        public CBORTypeFilter WithUnsignedInteger()
        {
            return this.WithType(0);
        }

        private CBORTypeFilter Copy()
        {
            CBORTypeFilter filter = new CBORTypeFilter
            {
                any = this.any,
                types = this.types,
                floatingpoint = this.floatingpoint,
                arrayLength = this.arrayLength,
                anyArrayLength = this.anyArrayLength,
                arrayMinLength = this.arrayMinLength,
                elements = this.elements,
                tags = this.tags
            };
            return filter;
        }

        private CBORTypeFilter WithAny()
        {
            if (this.any)
            {
                return this;
            }
            CBORTypeFilter filter = this.Copy();
            filter.any = true;
            filter.anyArrayLength = true;
            filter.types = 0xff;
            return filter;
        }

        private CBORTypeFilter WithType(int type)
        {
            if (this.any)
            {
                return this;
            }
            CBORTypeFilter filter = this.Copy();
            filter.types |= 1 << type;
            return filter;
        }
    }
    #endregion
    */

    #region CBORTypeMapper
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:CBORTypeMapper"]/*'/>
    /// <summary>Holds converters to customize the serialization and
    /// deserialization behavior of <c>CBORObject.FromObject</c> and
    /// <c>CBORObject#ToObject</c>, as well as type filters for
    /// <c>ToObject</c>.</summary>
    public sealed class CBORTypeMapper
    {
        private readonly IList<string> typePrefixes;
        private readonly IList<string> typeNames;
        private readonly IDictionary<Object, ConverterInfo>
        converters;

        /// <summary>Initializes a new instance of the
        /// <see cref='PeterO.Cbor.CBORTypeMapper'/> class.</summary>
        public CBORTypeMapper()
        {
            this.typePrefixes = new List<string>();
            this.typeNames = new List<string>();
            this.converters = new Dictionary<Object, ConverterInfo>();
        }

        /// <summary>Registers an object that converts objects of a given type
        /// to CBOR objects (called a CBOR converter).</summary>
        /// <param name='type'>A Type object specifying the type that the
        /// converter converts to CBOR objects.</param>
        /// <param name='converter'>The parameter <paramref name='converter'/>
        /// is an ICBORConverter object.</param>
        /// <typeparam name='T'>Must be the same as the "type"
        /// parameter.</typeparam>
        /// <returns>This object.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='type'/> or <paramref name='converter'/> is null.</exception>
        /// <exception cref='ArgumentException'>Converter doesn't contain a
        /// proper ToCBORObject method".</exception>
        public CBORTypeMapper AddConverter<T>(
          Type type,
          ICBORConverter<T> converter)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }
            var ci = new ConverterInfo();
            ci.Converter = converter;
            ci.ToObject = PropertyMap.FindOneArgumentMethod(
              converter,
              "ToCBORObject",
              type);
            if (ci.ToObject == null)
            {
                throw new ArgumentException(
                  "Converter doesn't contain a proper ToCBORObject method");
            }
            ci.FromObject = PropertyMap.FindOneArgumentMethod(
                converter,
                "FromCBORObject",
                typeof(CBORObject));
            this.converters[type] = ci;
            return this;
        }

        internal object ConvertBackWithConverter(
          CBORObject cbor,
          Type type)
        {
            ConverterInfo convinfo = null;
            if (this.converters.ContainsKey(type))
            {
                convinfo = this.converters[type];
            }
            else
            {
                return null;
            }
            if (convinfo == null)
            {
                return null;
            }
            return (convinfo.FromObject == null) ? null :
              PropertyMap.CallFromObject(convinfo, cbor);
        }

        internal CBORObject ConvertWithConverter(object obj)
        {
            Object type = obj.GetType();
            ConverterInfo convinfo = null;
            if (this.converters.ContainsKey(type))
            {
                convinfo = this.converters[type];
            }
            else
            {
                return null;
            }
            return (convinfo == null) ? null :
              PropertyMap.CallToObject(convinfo, obj);
        }

        /// <summary>Returns whether the given Java or.NET type name fits the
        /// filters given in this mapper.</summary>
        /// <param name='typeName'>The fully qualified name of a Java or.NET
        /// class (e.g., <c>java.math.BigInteger</c> or
        /// <c>System.Globalization.CultureInfo</c> ).</param>
        /// <returns>Either <c>true</c> if the given Java or.NET type name fits
        /// the filters given in this mapper, or <c>false</c>
        /// otherwise.</returns>
        public bool FilterTypeName(string typeName)
        {
            if (String.IsNullOrEmpty(typeName))
            {
                return false;
            }
            foreach (string prefix in this.typePrefixes)
            {
                if (typeName.Length >= prefix.Length &&
                  typeName.Substring(0, prefix.Length).Equals(prefix,
                    StringComparison.Ordinal))
                {
                    return true;
                }
            }
            foreach (string name in this.typeNames)
            {
                if (typeName.Equals(name, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Adds a prefix of a Java or.NET type for use in type
        /// matching. A type matches a prefix if its fully qualified name is or
        /// begins with that prefix, using codepoint-by-codepoint
        /// (case-sensitive) matching.</summary>
        /// <param name='prefix'>The prefix of a Java or.NET type (e.g.,
        /// `java.math.` or `System.Globalization`).</param>
        /// <returns>This object.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='prefix'/> is null.</exception>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='prefix'/> is empty.</exception>
        public CBORTypeMapper AddTypePrefix(string prefix)
        {
            if (prefix == null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }
            if (prefix.Length == 0)
            {
                throw new ArgumentException("prefix" + " is empty.");
            }
            this.typePrefixes.Add(prefix);
            return this;
        }

        /// <summary>Adds the fully qualified name of a Java or.NET type for
        /// use in type matching.</summary>
        /// <param name='name'>The fully qualified name of a Java or.NET class
        /// (e.g., <c>java.math.BigInteger</c> or
        /// <c>System.Globalization.CultureInfo</c> ).</param>
        /// <returns>This object.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='name'/> is null.</exception>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='name'/> is empty.</exception>
        public CBORTypeMapper AddTypeName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (name.Length == 0)
            {
                throw new ArgumentException("name" + " is empty.");
            }
            this.typeNames.Add(name);
            return this;
        }

        internal sealed class ConverterInfo
        {
            public object ToObject
            {
                get;
                set;
            }

            public object FromObject
            {
                get;
                set;
            }

            public object Converter
            {
                get;
                set;
            }
        }
    }

    #endregion

    #region CBORUriConverter
    internal class CBORUriConverter : ICBORToFromConverter<Uri>
    {
        private static CBORObject ValidateObject(CBORObject obj)
        {
            if (obj.Type != CBORType.TextString)
            {
                throw new CBORException("URI/IRI must be a text string");
            }
            bool isiri = obj.HasMostOuterTag(266);
            bool isiriref = obj.HasMostOuterTag(267);
            if (
              isiriref && !URIUtility.IsValidIRI(
                obj.AsString(),
                URIUtility.ParseMode.IRIStrict))
            {
                throw new CBORException("String is not a valid IRI Reference");
            }
            if (
              isiri && (!URIUtility.IsValidIRI(
                  obj.AsString(),
                  URIUtility.ParseMode.IRIStrict) ||
                !URIUtility.HasScheme(obj.AsString())))
            {
                throw new CBORException("String is not a valid IRI");
            }
            if (!URIUtility.IsValidIRI(
                obj.AsString(),
                URIUtility.ParseMode.URIStrict) ||
              !URIUtility.HasScheme(obj.AsString()))
            {
                throw new CBORException("String is not a valid URI");
            }
            return obj;
        }

        public Uri FromCBORObject(CBORObject obj)
        {
            if (obj.HasMostOuterTag(32) ||
                   obj.HasMostOuterTag(266) ||
                   obj.HasMostOuterTag(267))
            {
                ValidateObject(obj);
                try
                {
                    return new Uri(obj.AsString());
                }
                catch (Exception ex)
                {
                    throw new CBORException(ex.Message, ex);
                }
            }
            throw new CBORException();
        }

        public CBORObject ToCBORObject(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }
            string uriString = uri.ToString();
            var nonascii = false;
            for (var i = 0; i < uriString.Length; ++i)
            {
                nonascii |= uriString[i] >= 0x80;
            }
            int tag = nonascii ? 266 : 32;
            if (!URIUtility.HasScheme(uriString))
            {
                tag = 267;
            }
            return CBORObject.FromObjectAndTag(uriString, (int)tag);
        }
    }

    #endregion

    #region JSONOptions
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:JSONOptions"]/*'/>
    /// <summary>Includes options to control how CBOR objects are converted
    /// to JSON.</summary>
    public sealed class JSONOptions
    {
        /// <summary>Specifies how JSON numbers are converted to CBOR when
        /// decoding JSON.</summary>
        public enum ConversionMode
        {
            /// <summary>JSON numbers are decoded to CBOR using the full precision
            /// given in the JSON text. The number will be converted to a CBOR
            /// object as follows: If the number's exponent is 0 (after shifting
            /// the decimal point to the end of the number without changing its
            /// value), using the rules given in the
            /// <c>CBORObject.FromObject(EInteger)</c> method; otherwise, using the
            /// rules given in the <c>CBORObject.FromObject(EDecimal)</c> method.
            /// An exception in version 4.x involves negative zeros; if the
            /// negative zero's exponent is 0, it's written as a CBOR
            /// floating-point number; otherwise the negative zero is written as an
            /// EDecimal.</summary>
            Full,

            /// <summary>JSON numbers are decoded to CBOR as their closest-rounded
            /// approximation as 64-bit binary floating-point numbers. (In some
            /// cases, numbers extremely close to zero may underflow to positive or
            /// negative zero, and numbers of extremely large magnitude may
            /// overflow to infinity.).</summary>
            Double,

            /// <summary>A JSON number is decoded to CBOR either as a CBOR integer
            /// (major type 0 or 1) if the JSON number represents an integer at
            /// least -(2^53)+1 and less than 2^53, or as their closest-rounded
            /// approximation as 64-bit binary floating-point numbers otherwise.
            /// For example, the JSON number 0.99999999999999999999999999999999999
            /// is not an integer, so it's converted to its closest floating-point
            /// approximation, namely 1.0. (In some cases, numbers extremely close
            /// to zero may underflow to positive or negative zero, and numbers of
            /// extremely large magnitude may overflow to infinity.).</summary>
            IntOrFloat,

            /// <summary>A JSON number is decoded to CBOR either as a CBOR integer
            /// (major type 0 or 1) if the number's closest-rounded approximation
            /// as a 64-bit binary floating-point number represents an integer at
            /// least -(2^53)+1 and less than 2^53, or as that approximation
            /// otherwise. For example, the JSON number
            /// 0.99999999999999999999999999999999999 is the integer 1 when rounded
            /// to its closest floating-point approximation (1.0), so it's
            /// converted to the CBOR integer 1 (major type 0). (In some cases,
            /// numbers extremely close to zero may underflow to zero, and numbers
            /// of extremely large magnitude may overflow to infinity.).</summary>
            IntOrFloatFromDouble,

            /// <summary>JSON numbers are decoded to CBOR as their closest-rounded
            /// approximation to an IEEE 854 decimal128 value, using the rules for
            /// the EDecimal form of that approximation as given in the
            /// <c>CBORObject.FromObject(EDecimal)</c> method.</summary>
            Decimal128,
        }

        /// <summary>Initializes a new instance of the
        /// <see cref='PeterO.Cbor.JSONOptions'/> class with default
        /// options.</summary>
        public JSONOptions() : this(String.Empty)
        {
        }

        /// <summary>Initializes a new instance of the
        /// <see cref='PeterO.Cbor.JSONOptions'/> class with the given value
        /// for the Base64Padding option.</summary>
        /// <param name='base64Padding'>Whether padding is included when
        /// writing data in base64url or traditional base64 format to
        /// JSON.</param>
        [Obsolete("Use the string constructor instead.")]
        public JSONOptions(bool base64Padding)
          : this("base64Padding=" + (base64Padding ? "1" : "0"))
        {
        }

        /// <summary>Initializes a new instance of the
        /// <see cref='PeterO.Cbor.JSONOptions'/> class with the given values
        /// for the options.</summary>
        /// <param name='base64Padding'>Whether padding is included when
        /// writing data in base64url or traditional base64 format to
        /// JSON.</param>
        /// <param name='replaceSurrogates'>Whether surrogate code points not
        /// part of a surrogate pair (which consists of two consecutive
        /// <c>char</c> s forming one Unicode code point) are each replaced
        /// with a replacement character (U+FFFD). The default is false; an
        /// exception is thrown when such code points are encountered.</param>
#pragma warning disable CS0618
        [Obsolete("Use the string constructor instead.")]
        public JSONOptions(bool base64Padding, bool replaceSurrogates)
          : this("base64Padding=" + (base64Padding ? "1" : "0") +
               ";replacesurrogates=" + (replaceSurrogates ? "1" : "0"))
        {
        }

        /// <summary>Initializes a new instance of the
        /// <see cref='PeterO.Cbor.JSONOptions'/> class.</summary>
        /// <param name='paramString'>A string setting forth the options to
        /// use. This is a semicolon-separated list of options, each of which
        /// has a key and a value separated by an equal sign ("="). Whitespace
        /// and line separators are not allowed to appear between the
        /// semicolons or between the equal signs, nor may the string begin or
        /// end with whitespace. The string can be empty, but cannot be null.
        /// The following is an example of this parameter:
        /// <c>base64padding=false;replacesurrogates=true</c>. The key can be
        /// any one of the following where the letters can be any combination
        /// of basic upper-case and/or basic lower-case letters:
        /// <c>base64padding</c>, <c>replacesurrogates</c>,
        /// <c>allowduplicatekeys</c>, <c>preservenegativezero</c>,
        /// <c>numberconversion</c>. Other keys are ignored. (Keys are
        /// compared using a basic case-insensitive comparison, in which two
        /// strings are equal if they match after converting the basic
        /// upper-case letters A to Z (U+0041 to U+005A) in both strings to
        /// basic lower-case letters.) If two or more key/value pairs have
        /// equal keys (in a basic case-insensitive comparison), the value
        /// given for the last such key is used. The first four keys just given
        /// can have a value of <c>1</c>, <c>true</c>, <c>yes</c>, or
        /// <c>on</c> (where the letters can be any combination of basic
        /// upper-case and/or basic lower-case letters), which means true, and
        /// any other value meaning false. The last key,
        /// <c>numberconversion</c>, can have a value of any name given in the
        /// <c>JSONOptions.ConversionMode</c> enumeration (where the letters
        /// can be any combination of basic upper-case and/or basic lower-case
        /// letters), and any other value is unrecognized. (If the
        /// <c>numberconversion</c> key is not given, its value is treated as
        /// <c>full</c>. If that key is given, but has an unrecognized value,
        /// an exception is thrown.) For example, <c>base64padding=Yes</c> and
        /// <c>base64padding=1</c> both set the <c>Base64Padding</c> property
        /// to true, and <c>numberconversion=double</c> sets the
        /// <c>NumberConversion</c> property to <c>ConversionMode.Double</c>
        /// .</param>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='paramString'/> is null. In the future, this class may allow
        /// other keys to store other kinds of values, not just true or
        /// false.</exception>
        /// <exception cref='ArgumentException'>An unrecognized value for
        /// <c>numberconversion</c> was given.</exception>
        public JSONOptions(string paramString)
        {
            if (paramString == null)
            {
                throw new ArgumentNullException(nameof(paramString));
            }
            var parser = new OptionsParser(paramString);
            this.PreserveNegativeZero = parser.GetBoolean(
              "preservenegativezero",
              true);
            this.AllowDuplicateKeys = parser.GetBoolean(
              "allowduplicatekeys",
              false);
            this.Base64Padding = parser.GetBoolean("base64padding", true);
            this.ReplaceSurrogates = parser.GetBoolean(
              "replacesurrogates",
              false);
            this.NumberConversion = ToNumberConversion(parser.GetLCString(
              "numberconversion",
              null));
        }

        /// <summary>Gets the values of this options object's properties in
        /// text form.</summary>
        /// <returns>A text string containing the values of this options
        /// object's properties. The format of the string is the same as the
        /// one described in the String constructor for this class.</returns>
        public override string ToString()
        {
            return new StringBuilder()
              .Append("base64padding=").Append(this.Base64Padding ? "true" : "false")
              .Append(";replacesurrogates=")
              .Append(this.ReplaceSurrogates ? "true" : "false")
              .Append(this.PreserveNegativeZero ? "true" : "false")
              .Append(";numberconversion=").Append(this.FromNumberConversion())
              .Append(";allowduplicatekeys=")
              .Append(this.AllowDuplicateKeys ? "true" : "false")
              .ToString();
        }

        /// <summary>The default options for converting CBOR objects to
        /// JSON.</summary>
        public static readonly JSONOptions Default = new JSONOptions();

        /// <summary>Gets a value indicating whether the Base64Padding property
        /// is true. This property has no effect; in previous versions, this
        /// property meant that padding was written out when writing base64url
        /// or traditional base64 to JSON.</summary>
        /// <value>A value indicating whether the Base64Padding property is
        /// true.</value>
        [Obsolete("This property now has no effect. This library now includes " +
    "\u0020necessary padding when writing traditional base64 to JSON and" +
    "\u0020includes no padding when writing base64url to JSON, in " +
    "\u0020accordance with the revision of the CBOR specification.")]
        public bool Base64Padding
        {
            get;
            private set;
        }

        private string FromNumberConversion()
        {
            ConversionMode kind = this.NumberConversion;
            if (kind == ConversionMode.Full)
            {
                return "full";
            }
            if (kind == ConversionMode.Double)
            {
                return "double";
            }
            if (kind == ConversionMode.Decimal128)
            {
                return "decimal128";
            }
            if (kind == ConversionMode.IntOrFloat)
            {
                return "intorfloat";
            }
            return (kind == ConversionMode.IntOrFloatFromDouble) ?
      "intorfloatfromdouble" : "full";
        }

        private static ConversionMode ToNumberConversion(string str)
        {
            if (str != null)
            {
                if (str.Equals("full", StringComparison.Ordinal))
                {
                    return ConversionMode.Full;
                }
                if (str.Equals("double", StringComparison.Ordinal))
                {
                    return ConversionMode.Double;
                }
                if (str.Equals("decimal128", StringComparison.Ordinal))
                {
                    return ConversionMode.Decimal128;
                }
                if (str.Equals("intorfloat", StringComparison.Ordinal))
                {
                    return ConversionMode.IntOrFloat;
                }
                if (str.Equals("intorfloatfromdouble", StringComparison.Ordinal))
                {
                    return ConversionMode.IntOrFloatFromDouble;
                }
            }
            else
            {
                return ConversionMode.Full;
            }
            throw new ArgumentException("Unrecognized conversion mode");
        }

        /// <summary>Gets a value indicating whether the JSON decoder should
        /// preserve the distinction between positive zero and negative zero
        /// when the decoder decodes JSON to a floating-point number format
        /// that makes this distinction. For a value of <c>false</c>, if the
        /// result of parsing a JSON string would be a floating-point negative
        /// zero, that result is a positive zero instead. (Note that this
        /// property has no effect for conversion kind
        /// <c>IntOrFloatFromDouble</c>, where floating-point zeros are not
        /// possible.).</summary>
        /// <value>A value indicating whether to preserve the distinction
        /// between positive zero and negative zero when decoding JSON. The
        /// default is true.</value>
        public bool PreserveNegativeZero
        {
            get;
            private set;
        }

        /// <summary>Gets a value indicating how JSON numbers are decoded to
        /// CBOR.</summary>
        /// <value>A value indicating how JSON numbers are decoded to CBOR. The
        /// default is <c>ConversionMode.Full</c>.</value>
        public ConversionMode NumberConversion
        {
            get;
            private set;
        }

        /// <summary>Gets a value indicating whether to allow duplicate keys
        /// when reading JSON. Used only when decoding JSON. If this property
        /// is <c>true</c> and a JSON object has two or more values with the
        /// same key, the last value of that key set forth in the JSON object
        /// is taken.</summary>
        /// <value>A value indicating whether to allow duplicate keys when
        /// reading JSON. The default is false.</value>
        public bool AllowDuplicateKeys
        {
            get;
            private set;
        }

        /// <summary>Gets a value indicating whether surrogate code points not
        /// part of a surrogate pair (which consists of two consecutive
        /// <c>char</c> s forming one Unicode code point) are each replaced
        /// with a replacement character (U+FFFD). If false, an exception is
        /// thrown when such code points are encountered.</summary>
        /// <value>True, if surrogate code points not part of a surrogate pair
        /// are each replaced with a replacement character, or false if an
        /// exception is thrown when such code points are encountered. The
        /// default is false.</value>
        public bool ReplaceSurrogates
        {
            get;
            private set;
        }
    }
    #endregion

    #region PODOptions
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:PODOptions"]/*'/>
    /// <summary>Options for converting "plain old data" objects (better
    /// known as POCOs in .NET or POJOs in Java) to CBOR objects.</summary>
    public class PODOptions
    {
        /// <summary>Initializes a new instance of the
        /// <see cref='PeterO.Cbor.PODOptions'/> class.</summary>
        public PODOptions() : this(true, true)
        {
        }

        /// <summary>Initializes a new instance of the
        /// <see cref='PeterO.Cbor.PODOptions'/> class.</summary>
        /// <param name='removeIsPrefix'>The parameter is not used.</param>
        /// <param name='useCamelCase'>The value of the "UseCamelCase"
        /// property.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
          "Microsoft.Usage",
          "CA1801",
          Justification = "'removeIsPrefix' is present for backward compatibility.")]
        public PODOptions(bool removeIsPrefix, bool useCamelCase)
        {
            this.UseCamelCase = useCamelCase;
        }

        /// <summary>Initializes a new instance of the
        /// <see cref='PeterO.Cbor.PODOptions'/> class.</summary>
        /// <param name='paramString'>A string setting forth the options to
        /// use. This is a semicolon-separated list of options, each of which
        /// has a key and a value separated by an equal sign ("="). Whitespace
        /// and line separators are not allowed to appear between the
        /// semicolons or between the equal signs, nor may the string begin or
        /// end with whitespace. The string can be empty, but cannot be null.
        /// The following is an example of this parameter:
        /// <c>usecamelcase=true</c>. The key can be any one of the following
        /// where the letters can be any combination of basic upper-case and/or
        /// basic lower-case letters: <c>usecamelcase</c>. Other keys are
        /// ignored. (Keys are compared using a basic case-insensitive
        /// comparison, in which two strings are equal if they match after
        /// converting the basic upper-case letters A to Z (U+0041 to U+005A)
        /// in both strings to basic lower-case letters.) If two or more
        /// key/value pairs have equal keys (in a basic case-insensitive
        /// comparison), the value given for the last such key is used. The key
        /// just given can have a value of <c>1</c>, <c>true</c>, <c>yes</c>
        /// , or <c>on</c> (where the letters can be any combination of basic
        /// upper-case and/or basic lower-case letters), which means true, and
        /// any other value meaning false. For example, <c>usecamelcase=Yes</c>
        /// and <c>usecamelcase=1</c> both set the <c>UseCamelCase</c> property
        /// to true. In the future, this class may allow other keys to store
        /// other kinds of values, not just true or false.</param>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='paramString'/> is null.</exception>
        public PODOptions(string paramString)
        {
            if (paramString == null)
            {
                throw new ArgumentNullException(nameof(paramString));
            }
            var parser = new OptionsParser(paramString);
            this.UseCamelCase = parser.GetBoolean("usecamelcase", true);
        }

        /// <summary>Gets the values of this options object's properties in
        /// text form.</summary>
        /// <returns>A text string containing the values of this options
        /// object's properties. The format of the string is the same as the
        /// one described in the String constructor for this class.</returns>
        public override string ToString()
        {
            return new System.Text.StringBuilder()
              .Append("usecamelcase=").Append(this.UseCamelCase ? "true" :
      "false")
              .ToString();
        }

        /// <summary>The default settings for "plain old data"
        /// options.</summary>
        public static readonly PODOptions Default = new PODOptions();

        /// <summary>
        /// <para>Gets a value indicating whether property, field, and method
        /// names are converted to camel case before they are used as keys.
        /// This option changes the behavior of key name serialization as
        /// follows. If "useCamelCase" is <c>false</c> :</para>
        /// <list>
        /// <item>In the .NET version, all key names are capitalized, meaning
        /// the first letter in the name is converted to a basic upper-case
        /// letter if it's a basic lower-case letter ("a" to "z"). (For
        /// example, "Name" and "IsName" both remain unchanged.)</item>
        /// <item>In the Java version, all field names are capitalized, and for
        /// each eligible method name, the word "get" or "set" is removed from
        /// the name if the name starts with that word, then the name is
        /// capitalized. (For example, "getName" and "setName" both become
        /// "Name", and "isName" becomes "IsName".)</item></list>
        /// <para>If "useCamelCase" is <c>true</c> :</para>
        /// <list>
        /// <item>In the .NET version, for each eligible property or field
        /// name, the word "Is" is removed from the name if the name starts
        /// with that word, then the name is converted to camel case, meaning
        /// the first letter in the name is converted to a basic lower-case
        /// letter if it's a basic upper-case letter ("A" to "Z"). (For
        /// example, "Name" and "IsName" both become "name".)</item>
        /// <item>In the Java version: For each eligible method name, the word
        /// "get", "set", or "is" is removed from the name if the name starts
        /// with that word, then the name is converted to camel case. (For
        /// example, "getName", "setName", and "isName" all become "name".) For
        /// each eligible field name, the word "is" is removed from the name if
        /// the name starts with that word, then the name is converted to camel
        /// case. (For example, "name" and "isName" both become
        /// "name".)</item></list>
        /// <para>In the description above, a name "starts with" a word if that
        /// word begins the name and is followed by a character other than a
        /// basic digit or basic lower-case letter, that is, other than "a" to
        /// "z" or "0" to "9".</para></summary>
        /// <value><c>true</c> If the names are converted to camel case;
        /// otherwise, <c>false</c>. This property is <c>true</c> by
        /// default.</value>
        public bool UseCamelCase
        {
            get;
            private set;
        }
    }
    #endregion

    #region PropertyMap
    internal static class PropertyMap
    {
        private sealed class ReadOnlyWrapper<T> : ICollection<T>
        {
            private readonly ICollection<T> o;
            public ReadOnlyWrapper(ICollection<T> o)
            {
                this.o = o;
            }
            public void Add(T v)
            {
                throw new NotSupportedException();
            }
            public void Clear()
            {
                throw new NotSupportedException();
            }
            public void CopyTo(T[] a, int off)
            {
                this.o.CopyTo(a, off);
            }
            public bool Remove(T v)
            {
                throw new NotSupportedException();
            }
            public bool Contains(T v)
            {
                return this.o.Contains(v);
            }
            public int Count
            {
                get
                {
                    return this.o.Count;
                }
            }
            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }
            public IEnumerator<T> GetEnumerator()
            {
                return this.o.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)this.o).GetEnumerator();
            }
        }

        private sealed class PropertyData
        {
            private string name;

            public string Name
            {
                get
                {
                    return this.name;
                }

                set
                {
                    this.name = value;
                }
            }

            private MemberInfo prop;

            public Type PropertyType
            {
                get
                {
                    var pr = this.prop as PropertyInfo;
                    if (pr != null)
                    {
                        return pr.PropertyType;
                    }
                    var fi = this.prop as FieldInfo;
                    return (fi != null) ? fi.FieldType : null;
                }
            }

            public object GetValue(object obj)
            {
                var pr = this.prop as PropertyInfo;
                if (pr != null)
                {
                    return pr.GetValue(obj, null);
                }
                var fi = this.prop as FieldInfo;
                return (fi != null) ? fi.GetValue(obj) : null;
            }

            public void SetValue(object obj, object value)
            {
                var pr = this.prop as PropertyInfo;
                if (pr != null)
                {
                    pr.SetValue(obj, value, null);
                }
                var fi = this.prop as FieldInfo;
                if (fi != null)
                {
                    fi.SetValue(obj, value);
                }
            }

#if NET20 || NET40
      public static bool HasUsableGetter(PropertyInfo pi) {
        return pi != null && pi.CanRead && !pi.GetGetMethod().IsStatic &&
          pi.GetGetMethod().IsPublic;
      }

      public static bool HasUsableSetter(PropertyInfo pi) {
        return pi != null && pi.CanWrite && !pi.GetSetMethod().IsStatic &&
          pi.GetSetMethod().IsPublic;
      }
#else
            public static bool HasUsableGetter(PropertyInfo pi)
            {
                return pi != null && pi.CanRead && !pi.GetMethod.IsStatic &&
                  pi.GetMethod.IsPublic;
            }

            public static bool HasUsableSetter(PropertyInfo pi)
            {
                return pi != null && pi.CanWrite && !pi.SetMethod.IsStatic &&
                  pi.SetMethod.IsPublic;
            }
#endif
            public bool HasUsableGetter()
            {
                var pr = this.prop as PropertyInfo;
                if (pr != null)
                {
                    return HasUsableGetter(pr);
                }
                var fi = this.prop as FieldInfo;
                return fi != null && fi.IsPublic && !fi.IsStatic &&
                  !fi.IsInitOnly && !fi.IsLiteral;
            }

            public bool HasUsableSetter()
            {
                var pr = this.prop as PropertyInfo;
                if (pr != null)
                {
                    return HasUsableSetter(pr);
                }
                var fi = this.prop as FieldInfo;
                return fi != null && fi.IsPublic && !fi.IsStatic &&
                  !fi.IsInitOnly && !fi.IsLiteral;
            }

            public string GetAdjustedName(bool useCamelCase)
            {
                string thisName = this.Name;
                if (useCamelCase)
                {
                    if (CBORUtilities.NameStartsWithWord(thisName, "Is"))
                    {
                        thisName = thisName.Substring(2);
                    }
                    thisName = CBORUtilities.FirstCharLower(thisName);
                }
                else
                {
                    thisName = CBORUtilities.FirstCharUpper(thisName);
                }
                return thisName;
            }

            public MemberInfo Prop
            {
                get
                {
                    return this.prop;
                }

                set
                {
                    this.prop = value;
                }
            }
        }

#if NET40 || NET20
    private static bool IsGenericType(Type type) {
      return type.IsGenericType;
    }

    private static bool IsClassOrValueType(Type type) {
      return type.IsClass || type.IsValueType;
    }

    private static Type FirstGenericArgument(Type type) {
      return type.GetGenericArguments()[0];
    }

    private static IEnumerable<PropertyInfo> GetTypeProperties(Type t) {
      return t.GetProperties(BindingFlags.Public |
          BindingFlags.Instance);
    }

    private static IEnumerable<FieldInfo> GetTypeFields(Type t) {
      return t.GetFields(BindingFlags.Public | BindingFlags.Instance);
    }

    private static IEnumerable<Type> GetTypeInterfaces(Type t) {
      return t.GetInterfaces();
    }

    private static bool IsAssignableFrom(Type superType, Type subType) {
      return superType.IsAssignableFrom(subType);
    }

    private static MethodInfo GetTypeMethod(
      Type t,
      string name,
      Type[] parameters) {
      return t.GetMethod(name, parameters);
    }

    private static bool HasCustomAttribute(
      Type t,
      string name) {
      foreach (var attr in t.GetCustomAttributes(false)) {
        if (attr.GetType().FullName.Equals(name,
            StringComparison.Ordinal)) {
          return true;
        }
      }
      return false;
    }
#else
        private static bool IsGenericType(Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        private static bool IsClassOrValueType(Type type)
        {
            return type.GetTypeInfo().IsClass || type.GetTypeInfo().IsValueType;
        }

        private static Type FirstGenericArgument(Type type)
        {
            return type.GenericTypeArguments[0];
        }

        private static bool IsAssignableFrom(Type superType, Type subType)
        {
            return superType.GetTypeInfo().IsAssignableFrom(subType.GetTypeInfo());
        }

        private static IEnumerable<PropertyInfo> GetTypeProperties(Type t)
        {
            return t.GetRuntimeProperties();
        }

        private static IEnumerable<FieldInfo> GetTypeFields(Type t)
        {
            return t.GetRuntimeFields();
        }

        private static IEnumerable<Type> GetTypeInterfaces(Type t)
        {
            return t.GetTypeInfo().ImplementedInterfaces;
        }

        private static MethodInfo GetTypeMethod(
          Type t,
          string name,
          Type[] parameters)
        {
            return t.GetRuntimeMethod(name, parameters);
        }

        private static bool HasCustomAttribute(
          Type t,
          string name)
        {
            foreach (var attr in t.GetTypeInfo().GetCustomAttributes())
            {
                if (attr.GetType().FullName.Equals(name, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }
#endif

        private static readonly IDictionary<Type, IList<PropertyData>>
        ValuePropertyLists = new Dictionary<Type, IList<PropertyData>>();

        private static string RemoveIsPrefix(string pn)
        {
            return CBORUtilities.NameStartsWithWord(pn, "Is") ? pn.Substring(2) :
              pn;
        }

        private static IList<PropertyData> GetPropertyList(Type t)
        {
            lock (ValuePropertyLists)
            {
                IList<PropertyData> ret = new List<PropertyData>();
                if (ValuePropertyLists.ContainsKey(t))
                {
                    return ValuePropertyLists[t];
                }
                bool anonymous = HasCustomAttribute(
                    t,
                    "System.Runtime.CompilerServices.CompilerGeneratedAttribute") ||
                  HasCustomAttribute(
                    t,
                    "Microsoft.FSharp.Core.CompilationMappingAttribute");
                var names = new SortedDictionary<string, int>();
                foreach (PropertyInfo pi in GetTypeProperties(t))
                {
                    var pn = RemoveIsPrefix(pi.Name);
                    if (names.ContainsKey(pn))
                    {
                        ++names[pn];
                    }
                    else
                    {
                        names[pn] = 1;
                    }
                }
                foreach (FieldInfo pi in GetTypeFields(t))
                {
                    var pn = RemoveIsPrefix(pi.Name);
                    if (names.ContainsKey(pn))
                    {
                        ++names[pn];
                    }
                    else
                    {
                        names[pn] = 1;
                    }
                }
                foreach (FieldInfo fi in GetTypeFields(t))
                {
                    PropertyData pd = new PropertyMap.PropertyData()
                    {
                        Name = fi.Name,
                        Prop = fi,
                    };
                    if (pd.HasUsableGetter() || pd.HasUsableSetter())
                    {
                        var pn = RemoveIsPrefix(pd.Name);
                        // Ignore ambiguous properties
                        if (names.ContainsKey(pn) && names[pn] > 1)
                        {
                            continue;
                        }
                        ret.Add(pd);
                    }
                }
                foreach (PropertyInfo pi in GetTypeProperties(t))
                {
                    if (pi.CanRead && (pi.CanWrite || anonymous) &&
                      pi.GetIndexParameters().Length == 0)
                    {
                        if (PropertyData.HasUsableGetter(pi) ||
                          PropertyData.HasUsableSetter(pi))
                        {
                            var pn = RemoveIsPrefix(pi.Name);
                            // Ignore ambiguous properties
                            if (names.ContainsKey(pn) && names[pn] > 1)
                            {
                                continue;
                            }
                            PropertyData pd = new PropertyMap.PropertyData()
                            {
                                Name = pi.Name,
                                Prop = pi,
                            };
                            ret.Add(pd);
                        }
                    }
                }
                ValuePropertyLists.Add(
                  t,
                  ret);
                return ret;
            }
        }

        public static IList<CBORObject> ListFromArray(CBORObject[] array)
        {
            return new List<CBORObject>(array);
        }

        public static bool ExceedsKnownLength(Stream inStream, long size)
        {
            return (inStream is MemoryStream) && (size > (inStream.Length -
                  inStream.Position));
        }

        public static void SkipStreamToEnd(Stream inStream)
        {
            if (inStream is MemoryStream)
            {
                inStream.Position = inStream.Length;
            }
        }

        public static bool FirstElement(int[] dimensions)
        {
            foreach (var d in dimensions)
            {
                if (d == 0)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool NextElement(int[] index, int[] dimensions)
        {
            for (var i = dimensions.Length - 1; i >= 0; --i)
            {
                if (dimensions[i] > 0)
                {
                    ++index[i];
                    if (index[i] >= dimensions[i])
                    {
                        index[i] = 0;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static CBORObject BuildCBORArray(int[] dimensions)
        {
            int zeroPos = dimensions.Length;
            for (var i = 0; i < dimensions.Length; ++i)
            {
                if (dimensions[i] == 0)
                {
                    {
                        zeroPos = i;
                    }
                    break;
                }
            }
            int arraydims = zeroPos - 1;
            if (arraydims <= 0)
            {
                return CBORObject.NewArray();
            }
            var stack = new CBORObject[zeroPos];
            var index = new int[zeroPos];
            var stackpos = 0;
            CBORObject ret = CBORObject.NewArray();
            stack[0] = ret;
            index[0] = 0;
            for (var i = 0; i < dimensions[0]; ++i)
            {
                ret.Add(CBORObject.NewArray());
            }
            ++stackpos;
            while (stackpos > 0)
            {
                int curindex = index[stackpos - 1];
                if (curindex < stack[stackpos - 1].Count)
                {
                    CBORObject subobj = stack[stackpos - 1][curindex];
                    if (stackpos < zeroPos)
                    {
                        stack[stackpos] = subobj;
                        index[stackpos] = 0;
                        for (var i = 0; i < dimensions[stackpos]; ++i)
                        {
                            subobj.Add(CBORObject.NewArray());
                        }
                        ++index[stackpos - 1];
                        ++stackpos;
                    }
                    else
                    {
                        ++index[stackpos - 1];
                    }
                }
                else
                {
                    --stackpos;
                }
            }
            return ret;
        }

        public static CBORObject FromArray(
          Object arrObj,
          PODOptions options,
          CBORTypeMapper mapper,
          int depth)
        {
            var arr = (Array)arrObj;
            int rank = arr.Rank;
            if (rank == 0)
            {
                return CBORObject.NewArray();
            }
            CBORObject obj = null;
            if (rank == 1)
            {
                // Most common case: the array is one-dimensional
                obj = CBORObject.NewArray();
                int len = arr.GetLength(0);
                for (var i = 0; i < len; ++i)
                {
                    obj.Add(
                      CBORObject.FromObject(
                        arr.GetValue(i),
                        options,
                        mapper,
                        depth + 1));
                }
                return obj;
            }
            var index = new int[rank];
            var dimensions = new int[rank];
            for (var i = 0; i < rank; ++i)
            {
                dimensions[i] = arr.GetLength(i);
            }
            if (!FirstElement(dimensions))
            {
                return obj;
            }
            obj = BuildCBORArray(dimensions);
            do
            {
                CBORObject o = CBORObject.FromObject(
                    arr.GetValue(index),
                    options,
                    mapper,
                    depth + 1);
                SetCBORObject(obj, index, o);
            } while (NextElement(index, dimensions));
            return obj;
        }

        private static CBORObject GetCBORObject(CBORObject cbor, int[] index)
        {
            CBORObject ret = cbor;
            foreach (var i in index)
            {
                ret = ret[i];
            }
            return ret;
        }

        private static void SetCBORObject(
          CBORObject cbor,
          int[] index,
          CBORObject obj)
        {
            CBORObject ret = cbor;
            for (var i = 0; i < index.Length - 1; ++i)
            {
                ret = ret[index[i]];
            }
            int ilen = index[index.Length - 1];
            while (ilen >= ret.Count)
            {
                {
                    ret.Add(CBORObject.Null);
                }
            }
            ret[ilen] = obj;
        }

        public static Array FillArray(
          Array arr,
          Type elementType,
          CBORObject cbor,
          CBORTypeMapper mapper,
          PODOptions options,
          int depth)
        {
            int rank = arr.Rank;
            if (rank == 0)
            {
                return arr;
            }
            if (rank == 1)
            {
                int len = arr.GetLength(0);
                for (var i = 0; i < len; ++i)
                {
                    object item = cbor[i].ToObject(
                        elementType,
                        mapper,
                        options,
                        depth + 1);
                    arr.SetValue(
                      item,
                      i);
                }
                return arr;
            }
            var index = new int[rank];
            var dimensions = new int[rank];
            for (var i = 0; i < rank; ++i)
            {
                dimensions[i] = arr.GetLength(i);
            }
            if (!FirstElement(dimensions))
            {
                return arr;
            }
            do
            {
                object item = GetCBORObject(
                    cbor,
                    index).ToObject(
                    elementType,
                    mapper,
                    options,
                    depth + 1);
                arr.SetValue(
                  item,
                  index);
            } while (NextElement(index, dimensions));
            return arr;
        }

        public static int[] GetDimensions(CBORObject obj)
        {
            if (obj.Type != CBORType.Array)
            {
                throw new CBORException();
            }
            // Common cases
            if (obj.Count == 0)
            {
                return new int[] { 0 };
            }
            if (obj[0].Type != CBORType.Array)
            {
                return new int[] { obj.Count };
            }
            // Complex cases
            var list = new List<int>();
            list.Add(obj.Count);
            while (obj.Type == CBORType.Array &&
              obj.Count > 0 && obj[0].Type == CBORType.Array)
            {
                list.Add(obj[0].Count);
                obj = obj[0];
            }
            return list.ToArray();
        }

        public static object ObjectToEnum(CBORObject obj, Type enumType)
        {
            Type utype = Enum.GetUnderlyingType(enumType);
            object ret = null;
            if (obj.IsNumber && obj.AsNumber().IsInteger())
            {
                ret = Enum.ToObject(enumType, TypeToIntegerObject(obj, utype));
                if (!Enum.IsDefined(enumType, ret))
                {
                    string estr = ret.ToString();
                    if (estr == null || estr.Length == 0 || estr[0] == '-' ||
          (estr[0] >= '0' && estr[0] <= '9'))
                    {
                        throw new CBORException("Unrecognized enum value: " +
                          obj.ToString());
                    }
                }
                return ret;
            }
            else if (obj.Type == CBORType.TextString)
            {
                var nameString = obj.AsString();
                foreach (var name in Enum.GetNames(enumType))
                {
                    if (nameString.Equals(name, StringComparison.Ordinal))
                    {
                        return Enum.Parse(enumType, name);
                    }
                }
                throw new CBORException("Not found: " + obj.ToString());
            }
            else
            {
                throw new CBORException("Unrecognized enum value: " +
                  obj.ToString());
            }
        }

        public static object EnumToObject(Enum value)
        {
            return value.ToString();
        }

        public static object EnumToObjectAsInteger(Enum value)
        {
            Type t = Enum.GetUnderlyingType(value.GetType());
            if (t.Equals(typeof(ulong)))
            {
                ulong uvalue = Convert.ToUInt64(value,
                    CultureInfo.InvariantCulture);
                return EInteger.FromUInt64(uvalue);
            }
            return t.Equals(typeof(long)) ? Convert.ToInt64(value,
                CultureInfo.InvariantCulture) : (t.Equals(typeof(uint)) ?
                Convert.ToInt64(value,
                  CultureInfo.InvariantCulture) :
                Convert.ToInt32(value, CultureInfo.InvariantCulture));
        }

        public static ICollection<KeyValuePair<TKey, TValue>>
        GetEntries<TKey, TValue>(
          IDictionary<TKey, TValue> dict)
        {
            var c = (ICollection<KeyValuePair<TKey, TValue>>)dict;
            return new ReadOnlyWrapper<KeyValuePair<TKey, TValue>>(c);
        }

        public static object FindOneArgumentMethod(
          object obj,
          string name,
          Type argtype)
        {
            return GetTypeMethod(obj.GetType(), name, new[] { argtype });
        }

        public static object InvokeOneArgumentMethod(
          object methodInfo,
          object obj,
          object argument)
        {
            var mi = (MethodInfo)methodInfo;
            return mi.Invoke(obj, new[] { argument });
        }

        public static byte[] UUIDToBytes(Guid guid)
        {
            var bytes2 = new byte[16];
            var bytes = guid.ToByteArray();
            Array.Copy(bytes, bytes2, 16);
            // Swap the bytes to conform with the UUID RFC
            bytes2[0] = bytes[3];
            bytes2[1] = bytes[2];
            bytes2[2] = bytes[1];
            bytes2[3] = bytes[0];
            bytes2[4] = bytes[5];
            bytes2[5] = bytes[4];
            bytes2[6] = bytes[7];
            bytes2[7] = bytes[6];
            return bytes2;
        }

        private static bool StartsWith(string str, string pfx)
        {
            return str != null && str.Length >= pfx.Length &&
              str.Substring(0, pfx.Length).Equals(pfx, StringComparison.Ordinal);
        }

        // TODO: Replace* Legacy with AsNumber methods
        // in next major version
        private static object TypeToIntegerObject(CBORObject objThis, Type t)
        {
            if (t.Equals(typeof(int)))
            {
                return objThis.AsInt32();
            }
            if (t.Equals(typeof(short)))
            {
                return objThis.AsNumber().ToInt16Checked();
            }
            if (t.Equals(typeof(ushort)))
            {
                return objThis.AsUInt16Legacy();
            }
            if (t.Equals(typeof(byte)))
            {
                return objThis.AsByteLegacy();
            }
            if (t.Equals(typeof(sbyte)))
            {
                return objThis.AsSByteLegacy();
            }
            if (t.Equals(typeof(long)))
            {
                return objThis.AsNumber().ToInt64Checked();
            }
            if (t.Equals(typeof(uint)))
            {
                return objThis.AsUInt32Legacy();
            }
            if (t.Equals(typeof(ulong)))
            {
                return objThis.AsUInt64Legacy();
            }
            throw new CBORException("Type not supported");
        }

        public static object TypeToObject(
          CBORObject objThis,
          Type t,
          CBORTypeMapper mapper,
          PODOptions options,
          int depth)
        {
            if (t.Equals(typeof(int)))
            {
                return objThis.AsInt32();
            }
            if (t.Equals(typeof(short)))
            {
                return objThis.AsNumber().ToInt16Checked();
            }
            if (t.Equals(typeof(ushort)))
            {
                return objThis.AsUInt16Legacy();
            }
            if (t.Equals(typeof(byte)))
            {
                return objThis.AsByteLegacy();
            }
            if (t.Equals(typeof(sbyte)))
            {
                return objThis.AsSByteLegacy();
            }
            if (t.Equals(typeof(long)))
            {
                return objThis.AsNumber().ToInt64Checked();
            }
            if (t.Equals(typeof(uint)))
            {
                return objThis.AsUInt32Legacy();
            }
            if (t.Equals(typeof(ulong)))
            {
                return objThis.AsUInt64Legacy();
            }
            if (t.Equals(typeof(double)))
            {
                return objThis.AsDouble();
            }
            if (t.Equals(typeof(decimal)))
            {
                return objThis.AsDecimal();
            }
            if (t.Equals(typeof(float)))
            {
                return objThis.AsSingle();
            }
            if (t.Equals(typeof(bool)))
            {
                return objThis.AsBoolean();
            }
            if (t.Equals(typeof(char)))
            {
                if (objThis.Type == CBORType.TextString)
                {
                    string s = objThis.AsString();
                    if (s.Length != 1)
                    {
                        throw new CBORException("Can't convert to char");
                    }
                    return s[0];
                }
                if (objThis.IsNumber && objThis.AsNumber().CanFitInInt32())
                {
                    int c = objThis.AsNumber().ToInt32IfExact();
                    if (c < 0 || c >= 0x10000)
                    {
                        throw new CBORException("Can't convert to char");
                    }
                    return (char)c;
                }
                throw new CBORException("Can't convert to char");
            }
            if (t.Equals(typeof(DateTime)))
            {
                return new CBORDateConverter().FromCBORObject(objThis);
            }
            if (t.Equals(typeof(Guid)))
            {
                return new CBORUuidConverter().FromCBORObject(objThis);
            }
            if (t.Equals(typeof(Uri)))
            {
                return new CBORUriConverter().FromCBORObject(objThis);
            }
            if (IsAssignableFrom(typeof(Enum), t))
            {
                return ObjectToEnum(objThis, t);
            }
            if (IsGenericType(t))
            {
                Type td = t.GetGenericTypeDefinition();
                // Nullable types
                if (td.Equals(typeof(Nullable<>)))
                {
                    Type nullableType = Nullable.GetUnderlyingType(t);
                    if (objThis.IsNull)
                    {
                        return Activator.CreateInstance(t);
                    }
                    else
                    {
                        object wrappedObj = objThis.ToObject(
                            nullableType,
                            mapper,
                            options,
                            depth + 1);
                        return Activator.CreateInstance(
                            t,
                            wrappedObj);
                    }
                }
            }
            if (objThis.Type == CBORType.ByteString)
            {
                if (t.Equals(typeof(byte[])))
                {
                    byte[] bytes = objThis.GetByteString();
                    var byteret = new byte[bytes.Length];
                    Array.Copy(bytes, 0, byteret, 0, byteret.Length);
                    return byteret;
                }
            }
            if (objThis.Type == CBORType.Array)
            {
                Type objectType = typeof(object);
                var isList = false;
                object listObject = null;
                object genericListObject = null;
#if NET40 || NET20
        if (IsAssignableFrom(typeof(Array), t)) {
          Type elementType = t.GetElementType();
          Array array = Array.CreateInstance(
              elementType,
              GetDimensions(objThis));
          return FillArray(
              array,
              elementType,
              objThis,
              mapper,
              options,
              depth);
        }
        if (t.IsGenericType) {
          Type td = t.GetGenericTypeDefinition();
          isList = td.Equals(typeof(List<>)) || td.Equals(typeof(IList<>)) ||
            td.Equals(typeof(ICollection<>)) ||
            td.Equals(typeof(IEnumerable<>));
        }
        isList = isList && t.GetGenericArguments().Length == 1;
        if (isList) {
          objectType = t.GetGenericArguments()[0];
          Type listType = typeof(List<>).MakeGenericType(objectType);
          listObject = Activator.CreateInstance(listType);
        }
#else
                if (IsAssignableFrom(typeof(Array), t))
                {
                    Type elementType = t.GetElementType();
                    Array array = Array.CreateInstance(
                        elementType,
                        GetDimensions(objThis));
                    return FillArray(
                        array,
                        elementType,
                        objThis,
                        mapper,
                        options,
                        depth);
                }
                if (t.GetTypeInfo().IsGenericType)
                {
                    Type td = t.GetGenericTypeDefinition();
                    isList = td.Equals(typeof(List<>)) || td.Equals(typeof(IList<>)) ||
                      td.Equals(typeof(ICollection<>)) ||
                      td.Equals(typeof(IEnumerable<>));
                }
                isList = isList && t.GenericTypeArguments.Length == 1;
                if (isList)
                {
                    objectType = t.GenericTypeArguments[0];
                    Type listType = typeof(List<>).MakeGenericType(objectType);
                    listObject = Activator.CreateInstance(listType);
                }
#endif
                if (listObject == null)
                {
                    if (t.Equals(typeof(IList)) ||
                      t.Equals(typeof(ICollection)) || t.Equals(typeof(IEnumerable)))
                    {
                        listObject = new List<object>();
                        objectType = typeof(object);
                    }
                    else if (IsClassOrValueType(t))
                    {
                        var implementsList = false;
                        foreach (var interf in GetTypeInterfaces(t))
                        {
                            if (IsGenericType(interf) &&
                              interf.GetGenericTypeDefinition().Equals(typeof(IList<>)))
                            {
                                if (implementsList)
                                {
                                    implementsList = false;
                                    break;
                                }
                                else
                                {
                                    implementsList = true;
                                    objectType = FirstGenericArgument(interf);
                                }
                            }
                        }
                        if (implementsList)
                        {
                            // DebugUtility.Log("assignable from list<>");
                            genericListObject = Activator.CreateInstance(t);
                        }
                        else
                        {
                            // DebugUtility.Log("not assignable from list<> " + t);
                        }
                    }
                }
                if (genericListObject != null)
                {
                    object addMethod = FindOneArgumentMethod(
                        genericListObject,
                        "Add",
                        objectType);
                    if (addMethod == null)
                    {
                        throw new CBORException();
                    }
                    foreach (CBORObject value in objThis.Values)
                    {
                        PropertyMap.InvokeOneArgumentMethod(
                          addMethod,
                          genericListObject,
                          value.ToObject(objectType, mapper, options, depth + 1));
                    }
                    return genericListObject;
                }
                if (listObject != null)
                {
                    System.Collections.IList ie = (System.Collections.IList)listObject;
                    foreach (CBORObject value in objThis.Values)
                    {
                        ie.Add(value.ToObject(objectType, mapper, options, depth + 1));
                    }
                    return listObject;
                }
            }
            if (objThis.Type == CBORType.Map)
            {
                var isDict = false;
                Type keyType = null;
                Type valueType = null;
                object dictObject = null;
#if NET40 || NET20
        isDict = t.IsGenericType;
        if (t.IsGenericType) {
          Type td = t.GetGenericTypeDefinition();
          isDict = td.Equals(typeof(Dictionary<,>)) ||
            td.Equals(typeof(IDictionary<,>));
        }
        // DebugUtility.Log("list=" + isDict);
        isDict = isDict && t.GetGenericArguments().Length == 2;
        // DebugUtility.Log("list=" + isDict);
        if (isDict) {
          keyType = t.GetGenericArguments()[0];
          valueType = t.GetGenericArguments()[1];
          Type listType = typeof(Dictionary<,>).MakeGenericType(
              keyType,
              valueType);
          dictObject = Activator.CreateInstance(listType);
        }
#else
                isDict = t.GetTypeInfo().IsGenericType;
                if (t.GetTypeInfo().IsGenericType)
                {
                    Type td = t.GetGenericTypeDefinition();
                    isDict = td.Equals(typeof(Dictionary<,>)) ||
                      td.Equals(typeof(IDictionary<,>));
                }
                // DebugUtility.Log("list=" + isDict);
                isDict = isDict && t.GenericTypeArguments.Length == 2;
                // DebugUtility.Log("list=" + isDict);
                if (isDict)
                {
                    keyType = t.GenericTypeArguments[0];
                    valueType = t.GenericTypeArguments[1];
                    Type listType = typeof(Dictionary<,>).MakeGenericType(
                        keyType,
                        valueType);
                    dictObject = Activator.CreateInstance(listType);
                }
#endif
                if (dictObject == null)
                {
                    if (t.Equals(typeof(IDictionary)))
                    {
                        dictObject = new Dictionary<object, object>();
                        keyType = typeof(object);
                        valueType = typeof(object);
                    }
                }
                if (dictObject != null)
                {
                    System.Collections.IDictionary idic =
                      (System.Collections.IDictionary)dictObject;
                    foreach (CBORObject key in objThis.Keys)
                    {
                        CBORObject value = objThis[key];
                        idic.Add(
                          key.ToObject(keyType, mapper, options, depth + 1),
                          value.ToObject(valueType, mapper, options, depth + 1));
                    }
                    return dictObject;
                }
                if (mapper != null)
                {
                    if (!mapper.FilterTypeName(t.FullName))
                    {
                        throw new CBORException("Type " + t.FullName +
                          " not supported");
                    }
                }
                else
                {
                    if (t.FullName != null && (
                        StartsWith(t.FullName, "Microsoft.Win32.") ||
                        StartsWith(t.FullName, "System.IO.")))
                    {
                        throw new CBORException("Type " + t.FullName +
                          " not supported");
                    }
                    if (StartsWith(t.FullName, "System.") &&
                      !HasCustomAttribute(t, "System.SerializableAttribute"))
                    {
                        throw new CBORException("Type " + t.FullName +
                          " not supported");
                    }
                }
                var values = new List<KeyValuePair<string, CBORObject>>();
                var propNames = PropertyMap.GetPropertyNames(
                    t,
                    options != null ? options.UseCamelCase : true);
                foreach (string key in propNames)
                {
                    if (objThis.ContainsKey(key))
                    {
                        CBORObject cborValue = objThis[key];
                        var dict = new KeyValuePair<string, CBORObject>(
                          key,
                          cborValue);
                        values.Add(dict);
                    }
                }
                return PropertyMap.ObjectWithProperties(
                    t,
                    values,
                    mapper,
                    options,
                    depth);
            }
            else
            {
                throw new CBORException();
            }
        }

        public static object ObjectWithProperties(
          Type t,
          IEnumerable<KeyValuePair<string, CBORObject>> keysValues,
          CBORTypeMapper mapper,
          PODOptions options,
          int depth)
        {
            try
            {
                object o = Activator.CreateInstance(t);
                var dict = new SortedDictionary<string, CBORObject>();
                foreach (var kv in keysValues)
                {
                    var name = kv.Key;
                    dict[name] = kv.Value;
                }
                foreach (PropertyData key in GetPropertyList(o.GetType()))
                {
                    if (!key.HasUsableSetter() || !key.HasUsableGetter())
                    {
                        // Require properties to have both a setter and
                        // a getter to be eligible for setting
                        continue;
                    }
                    var name = key.GetAdjustedName(options != null ?
                        options.UseCamelCase : true);
                    if (dict.ContainsKey(name))
                    {
                        object dobj = dict[name].ToObject(
                            key.PropertyType,
                            mapper,
                            options,
                            depth + 1);
                        key.SetValue(
                          o,
                          dobj);
                    }
                }
                return o;
            }
            catch (Exception ex)
            {
                throw new CBORException(ex.Message, ex);
            }
        }

        public static CBORObject CallToObject(
          CBORTypeMapper.ConverterInfo convinfo,
          object obj)
        {
            return (CBORObject)PropertyMap.InvokeOneArgumentMethod(
                convinfo.ToObject,
                convinfo.Converter,
                obj);
        }

        public static object CallFromObject(
          CBORTypeMapper.ConverterInfo convinfo,
          CBORObject obj)
        {
            return (object)PropertyMap.InvokeOneArgumentMethod(
                convinfo.FromObject,
                convinfo.Converter,
                obj);
        }

        public static IEnumerable<KeyValuePair<string, object>> GetProperties(
          Object o)
        {
            return GetProperties(o, true);
        }

        public static IEnumerable<string> GetPropertyNames(Type t, bool
          useCamelCase)
        {
            foreach (PropertyData key in GetPropertyList(t))
            {
                yield return key.GetAdjustedName(useCamelCase);
            }
        }

        public static IEnumerable<KeyValuePair<string, object>> GetProperties(
          Object o,
          bool useCamelCase)
        {
            foreach (PropertyData key in GetPropertyList(o.GetType()))
            {
                if (!key.HasUsableGetter())
                {
                    continue;
                }
                yield return new KeyValuePair<string, object>(
                    key.GetAdjustedName(useCamelCase),
                    key.GetValue(o));
            }
        }

        public static void BreakDownDateTime(
          DateTime bi,
          EInteger[] year,
          int[] lf)
        {
#if NET20
      DateTime dt = bi.ToUniversalTime();
#else
            DateTime dt = TimeZoneInfo.ConvertTime(bi, TimeZoneInfo.Utc);
#endif
            year[0] = EInteger.FromInt32(dt.Year);
            lf[0] = dt.Month;
            lf[1] = dt.Day;
            lf[2] = dt.Hour;
            lf[3] = dt.Minute;
            lf[4] = dt.Second;
            // lf[5] is the number of nanoseconds
            lf[5] = (int)(dt.Ticks % 10000000L) * 100;
        }

        public static DateTime BuildUpDateTime(EInteger year, int[] dt)
        {
            return new DateTime(
                year.ToInt32Checked(),
                dt[0],
                dt[1],
                dt[2],
                dt[3],
                dt[4],
                DateTimeKind.Utc)
              .AddMinutes(-dt[6]).AddTicks((long)(dt[5] / 100));
        }
    }
    #endregion

    #region PropertyMap2
    /*
    internal static class PropertyMap2
    {
        private sealed class PropertyData
        {
            private string name;

            public string Name
            {
                get
                {
                    return this.name;
                }

                set
                {
                    this.name = value;
                }
            }

            private PropertyInfo prop;
#if NET20 || NET40
            public static bool HasUsableGetter(PropertyInfo pi) {
        return pi != null && pi.CanRead && !pi.GetGetMethod().IsStatic &&
                    pi.GetGetMethod().IsPublic;
      }

      public static bool HasUsableSetter(PropertyInfo pi) {
        return pi != null && pi.CanWrite && !pi.GetSetMethod().IsStatic &&
           pi.GetSetMethod().IsPublic;
      }
#else
            public static bool HasUsableGetter(PropertyInfo pi)
            {
                return pi != null && pi.CanRead && !pi.GetMethod.IsStatic &&
                     pi.GetMethod.IsPublic;
            }

            public static bool HasUsableSetter(PropertyInfo pi)
            {
                return pi != null && pi.CanWrite && !pi.SetMethod.IsStatic &&
                     pi.SetMethod.IsPublic;
            }
#endif
            public bool HasUsableGetter()
            {
                return HasUsableGetter(this.prop);
            }

            public bool HasUsableSetter()
            {
                return HasUsableSetter(this.prop);
            }

            public string GetAdjustedName(bool useCamelCase)
            {
                string thisName = this.Name;
                if (useCamelCase)
                {
                    if (CBORUtilities.NameStartsWithWord(thisName, "Is"))
                    {
                        thisName = thisName.Substring(2);
                    }
                    thisName = CBORUtilities.FirstCharLower(thisName);
                }
                else
                {
                    thisName = CBORUtilities.FirstCharUpper(thisName);
                }
                return thisName;
            }

            public PropertyInfo Prop
            {
                get
                {
                    return this.prop;
                }

                set
                {
                    this.prop = value;
                }
            }
        }

#if NET40 || NET20
    private static IEnumerable<PropertyInfo> GetTypeProperties(Type t) {
      return t.GetProperties(BindingFlags.Public |
        BindingFlags.Instance);
    }

    private static bool IsAssignableFrom(Type superType, Type subType) {
      return superType.IsAssignableFrom(subType);
    }

    private static MethodInfo GetTypeMethod(
  Type t,
  string name,
  Type[] parameters) {
      return t.GetMethod(name, parameters);
    }

    private static bool HasCustomAttribute(
  Type t,
  string name) {
#if NET40 || NET20
      foreach (var attr in t.GetCustomAttributes(false)) {
#else
    foreach (var attr in t.CustomAttributes) {
#endif
        if (attr.GetType().FullName.Equals(name)) {
          return true;
        }
      }
      return false;
    }
#else
        private static bool IsAssignableFrom(Type superType, Type subType)
        {
            return superType.GetTypeInfo().IsAssignableFrom(subType.GetTypeInfo());
        }

        private static IEnumerable<PropertyInfo> GetTypeProperties(Type t)
        {
            return t.GetRuntimeProperties();
        }

        private static MethodInfo GetTypeMethod(
      Type t,
      string name,
      Type[] parameters)
        {
            return t.GetRuntimeMethod(name, parameters);
        }

        private static bool HasCustomAttribute(
      Type t,
      string name)
        {
            foreach (Attribute attr in t.GetTypeInfo().GetCustomAttributes())
            {
                if (attr.GetType().FullName.Equals(name))
                {
                    return true;
                }
            }
            return false;
        }

#endif

        private static readonly IDictionary<Type, IList<PropertyData>>
          ValuePropertyLists = new Dictionary<Type, IList<PropertyData>>();

        private static string RemoveIsPrefix(string pn)
        {
            return CBORUtilities.NameStartsWithWord(pn, "Is") ? pn.Substring(2) : pn;
        }

        private static IList<PropertyData> GetPropertyList(Type t)
        {
            lock (ValuePropertyLists)
            {
                if (
         ValuePropertyLists.TryGetValue(
         t,
         out IList<PropertyData> ret))
                {
                    return ret;
                }
                ret = new List<PropertyData>();
                bool anonymous = HasCustomAttribute(
                  t,
                  "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
                Dictionary<string, int> names = new Dictionary<string, int>();
                foreach (PropertyInfo pi in GetTypeProperties(t))
                {
                    string pn = RemoveIsPrefix(pi.Name);
                    if (names.ContainsKey(pn))
                    {
                        ++names[pn];
                    }
                    else
                    {
                        names[pn] = 1;
                    }
                }
                foreach (PropertyInfo pi in GetTypeProperties(t))
                {
                    if (pi.CanRead && (pi.CanWrite || anonymous) &&
                    pi.GetIndexParameters().Length == 0)
                    {
                        if (PropertyData.HasUsableGetter(pi) ||
                            PropertyData.HasUsableSetter(pi))
                        {
                            string pn = RemoveIsPrefix(pi.Name);
                            // Ignore ambiguous properties
                            if (names.ContainsKey(pn) && names[pn] > 1)
                            {
                                continue;
                            }
                            PropertyData pd = new PropertyMap2.PropertyData()
                            {
                                Name = pi.Name,
                                Prop = pi
                            };
                            ret.Add(pd);
                        }
                    }
                }
                ValuePropertyLists.Add(t, ret);
                return ret;
            }
        }

        public static bool ExceedsKnownLength(Stream inStream, long size)
        {
            return (inStream is MemoryStream) && (size > (inStream.Length -
              inStream.Position));
        }

        public static void SkipStreamToEnd(Stream inStream)
        {
            if (inStream is MemoryStream)
            {
                inStream.Position = inStream.Length;
            }
        }

        public static bool FirstElement(int[] index, int[] dimensions)
        {
            foreach (int d in dimensions)
            {
                if (d == 0)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool NextElement(int[] index, int[] dimensions)
        {
            for (int i = dimensions.Length - 1; i >= 0; --i)
            {
                if (dimensions[i] > 0)
                {
                    ++index[i];
                    if (index[i] >= dimensions[i])
                    {
                        index[i] = 0;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static CBORObject BuildCBORArray(int[] dimensions)
        {
            int zeroPos = dimensions.Length;
            for (int i = 0; i < dimensions.Length; ++i)
            {
                if (dimensions[i] == 0)
                {
                    {
                        zeroPos = i;
                    }
                    break;
                }
            }
            int arraydims = zeroPos - 1;
            if (arraydims <= 0)
            {
                return CBORObject.NewArray();
            }
            CBORObject[] stack = new CBORObject[zeroPos];
            int[] index = new int[zeroPos];
            int stackpos = 0;
            CBORObject ret = CBORObject.NewArray();
            stack[0] = ret;
            index[0] = 0;
            for (int i = 0; i < dimensions[0]; ++i)
            {
                ret.Add(CBORObject.NewArray());
            }
            ++stackpos;
            while (stackpos > 0)
            {
                int curindex = index[stackpos - 1];
                if (curindex < stack[stackpos - 1].Count)
                {
                    CBORObject subobj = stack[stackpos - 1][curindex];
                    if (stackpos < zeroPos)
                    {
                        stack[stackpos] = subobj;
                        index[stackpos] = 0;
                        for (int i = 0; i < dimensions[stackpos]; ++i)
                        {
                            subobj.Add(CBORObject.NewArray());
                        }
                        ++index[stackpos - 1];
                        ++stackpos;
                    }
                    else
                    {
                        ++index[stackpos - 1];
                    }
                }
                else
                {
                    --stackpos;
                }
            }
            return ret;
        }

        private static CBORObject GetCBORObject(CBORObject cbor, int[] index)
        {
            CBORObject ret = cbor;
            foreach (int i in index)
            {
                ret = ret[i];
            }
            return ret;
        }

        private static void SetCBORObject(
      CBORObject cbor,
      int[] index,
      CBORObject obj)
        {
            CBORObject ret = cbor;
            for (int i = 0; i < index.Length - 1; ++i)
            {
                ret = ret[index[i]];
            }
            int ilen = index[index.Length - 1];
            while (ilen >= ret.Count)
            {
                {
                    ret.Add(CBORObject.Null);
                }
            }
            ret[ilen] = obj;
        }

        public static Array FillArray(
      Array arr,
      Type elementType,
      CBORObject cbor,
      CBORTypeMapper mapper,
      PODOptions options,
      int depth)
        {
            int rank = arr.Rank;
            if (rank == 0)
            {
                return arr;
            }
            if (rank == 1)
            {
                int len = arr.GetLength(0);
                for (int i = 0; i < len; ++i)
                {
                    object item = cbor[i].ToObject(
               elementType,
               mapper,
               options,
               depth + 1);
                    arr.SetValue(item, i);
                }
                return arr;
            }
            int[] index = new int[rank];
            int[] dimensions = new int[rank];
            for (int i = 0; i < rank; ++i)
            {
                dimensions[i] = arr.GetLength(i);
            }
            if (!FirstElement(index, dimensions))
            {
                return arr;
            }
            do
            {
                object item = GetCBORObject(
          cbor,
          index).ToObject(
          elementType,
          mapper,
          options,
          depth + 1);
                arr.SetValue(item, index);
            } while (NextElement(index, dimensions));
            return arr;
        }

        public static int[] GetDimensions(CBORObject obj)
        {
            if (obj.Type != CBORType.Array)
            {
                throw new CBORException();
            }
            // Common cases
            if (obj.Count == 0)
            {
                return new int[] { 0 };
            }
            if (obj[0].Type != CBORType.Array)
            {
                return new int[] { obj.Count };
            }
            // Complex cases
            List<int> list = new List<int>
            {
                obj.Count
            };
            while (obj.Type == CBORType.Array &&
                  obj.Count > 0 && obj[0].Type == CBORType.Array)
            {
                list.Add(obj[0].Count);
                obj = obj[0];
            }
            return list.ToArray();
        }

        public static object ObjectToEnum(CBORObject obj, Type enumType)
        {
            Type utype = Enum.GetUnderlyingType(enumType);
            object ret = null;
            if (obj.Type == CBORType.Number && obj.IsIntegral)
            {
                ret = Enum.ToObject(enumType, TypeToIntegerObject(obj, utype));
                if (!Enum.IsDefined(enumType, ret))
                {
                    throw new CBORException("Unrecognized enum value: " +
                    obj.ToString());
                }
                return ret;
            }
            else if (obj.Type == CBORType.TextString)
            {
                string nameString = obj.AsString();
                foreach (string name in Enum.GetNames(enumType))
                {
                    if (nameString.Equals(name))
                    {
                        return Enum.Parse(enumType, name);
                    }
                }
                throw new CBORException("Not found: " + obj.ToString());
            }
            else
            {
                throw new CBORException("Unrecognized enum value: " +
                   obj.ToString());
            }
        }

        public static object EnumToObject(Enum value)
        {
            return value.ToString();
        }

        public static object EnumToObjectAsInteger(Enum value)
        {
            Type t = Enum.GetUnderlyingType(value.GetType());
            if (t.Equals(typeof(ulong)))
            {
                ulong uvalue = Convert.ToUInt64(value);
                return EInteger.FromUInt64(uvalue);
            }
            return t.Equals(typeof(long)) ? Convert.ToInt64(value) :
            (t.Equals(typeof(uint)) ? Convert.ToInt64(value) :
            Convert.ToInt32(value));
        }

        public static object FindOneArgumentMethod(
      object obj,
      string name,
      Type argtype)
        {
            return GetTypeMethod(obj.GetType(), name, new[] { argtype });
        }

        public static object InvokeOneArgumentMethod(
      object methodInfo,
      object obj,
      object argument)
        {
            return ((MethodInfo)methodInfo).Invoke(obj, new[] { argument });
        }

        public static byte[] UUIDToBytes(Guid guid)
        {
            byte[] bytes2 = new byte[16];
            byte[] bytes = guid.ToByteArray();
            Array.Copy(bytes, bytes2, 16);
            // Swap the bytes to conform with the UUID RFC
            bytes2[0] = bytes[3];
            bytes2[1] = bytes[2];
            bytes2[2] = bytes[1];
            bytes2[3] = bytes[0];
            bytes2[4] = bytes[5];
            bytes2[5] = bytes[4];
            bytes2[6] = bytes[7];
            bytes2[7] = bytes[6];
            return bytes2;
        }

        private static bool StartsWith(string str, string pfx)
        {
            return str != null && str.Length >= pfx.Length &&
              str.Substring(0, pfx.Length).Equals(pfx);
        }

        private static object TypeToIntegerObject(CBORObject objThis, Type t)
        {
            if (t.Equals(typeof(int)))
            {
                return objThis.AsInt32();
            }
            if (t.Equals(typeof(short)))
            {
                return objThis.AsInt16();
            }
            if (t.Equals(typeof(ushort)))
            {
                return objThis.AsUInt16();
            }
            if (t.Equals(typeof(byte)))
            {
                return objThis.AsByte();
            }
            if (t.Equals(typeof(sbyte)))
            {
                return objThis.AsSByte();
            }
            if (t.Equals(typeof(long)))
            {
                return objThis.AsInt64();
            }
            if (t.Equals(typeof(uint)))
            {
                return objThis.AsUInt32();
            }
            if (t.Equals(typeof(ulong)))
            {
                return objThis.AsUInt64();
            }
            throw new CBORException("Type not supported");
        }

        public static object TypeToObject(
             CBORObject objThis,
             Type t,
             CBORTypeMapper mapper,
             PODOptions options,
             int depth)
        {
            if (t.Equals(typeof(int)))
            {
                return objThis.AsInt32();
            }
            if (t.Equals(typeof(short)))
            {
                return objThis.AsInt16();
            }
            if (t.Equals(typeof(ushort)))
            {
                return objThis.AsUInt16();
            }
            if (t.Equals(typeof(byte)))
            {
                return objThis.AsByte();
            }
            if (t.Equals(typeof(sbyte)))
            {
                return objThis.AsSByte();
            }
            if (t.Equals(typeof(long)))
            {
                return objThis.AsInt64();
            }
            if (t.Equals(typeof(uint)))
            {
                return objThis.AsUInt32();
            }
            if (t.Equals(typeof(ulong)))
            {
                return objThis.AsUInt64();
            }
            if (t.Equals(typeof(double)))
            {
                return objThis.AsDouble();
            }
            if (t.Equals(typeof(decimal)))
            {
                return objThis.AsDecimal();
            }
            if (t.Equals(typeof(float)))
            {
                return objThis.AsSingle();
            }
            if (t.Equals(typeof(bool)))
            {
                return objThis.AsBoolean();
            }
            if (t.Equals(typeof(char)))
            {
                if (objThis.Type == CBORType.TextString)
                {
                    string s = objThis.AsString();
                    if (s.Length != 1)
                    {
                        throw new CBORException("Can't convert to char");
                    }
                    return s[0];
                }
                if (objThis.IsIntegral && objThis.CanTruncatedIntFitInInt32())
                {
                    int c = objThis.AsInt32();
                    if (c < 0 || c >= 0x10000)
                    {
                        throw new CBORException("Can't convert to char");
                    }
                    return (char)c;
                }
                throw new CBORException("Can't convert to char");
            }
            if (t.Equals(typeof(DateTime)))
            {
                return new CBORDateConverter().FromCBORObject(objThis);
            }
            if (t.Equals(typeof(Guid)))
            {
                return new CBORUuidConverter().FromCBORObject(objThis);
            }
            if (t.Equals(typeof(Uri)))
            {
                return new CBORUriConverter().FromCBORObject(objThis);
            }
            if (t.Equals(typeof(EDecimal)))
            {
                return objThis.AsEDecimal();
            }
            if (t.Equals(typeof(EFloat)))
            {
                return objThis.AsEFloat();
            }
            if (t.Equals(typeof(EInteger)))
            {
                return objThis.AsEInteger();
            }
            if (t.Equals(typeof(ERational)))
            {
                return objThis.AsERational();
            }
            if (IsAssignableFrom(typeof(Enum), t))
            {
                return ObjectToEnum(objThis, t);
            }
            if (objThis.Type == CBORType.ByteString)
            {
                if (t.Equals(typeof(byte[])))
                {
                    byte[] bytes = objThis.GetByteString();
                    byte[] byteret = new byte[bytes.Length];
                    Array.Copy(bytes, 0, byteret, 0, byteret.Length);
                    return byteret;
                }
            }
            if (objThis.Type == CBORType.Array)
            {
                Type objectType = typeof(object);
                bool isList = false;
                object listObject = null;
#if NET40 || NET20
    if (IsAssignableFrom(typeof(Array), t)) {
Type elementType = t.GetElementType();
          Array array = Array.CreateInstance(
        elementType,
        GetDimensions(objThis));
    return FillArray(array, elementType, objThis, mapper, options, depth);
        }
        if (t.IsGenericType) {
          Type td = t.GetGenericTypeDefinition();
          isList = td.Equals(typeof(List<>)) || td.Equals(typeof(IList<>)) ||
            td.Equals(typeof(ICollection<>)) ||
            td.Equals(typeof(IEnumerable<>));
        }
        isList = isList && t.GetGenericArguments().Length == 1;
        if (isList) {
          objectType = t.GetGenericArguments()[0];
          Type listType = typeof(List<>).MakeGenericType(objectType);
          listObject = Activator.CreateInstance(listType);
        }
#else
                if (IsAssignableFrom(typeof(Array), t))
                {
                    Type elementType = t.GetElementType();
                    Array array = Array.CreateInstance(
                  elementType,
                  GetDimensions(objThis));
                    return FillArray(array, elementType, objThis, mapper, options, depth);
                }
                if (t.GetTypeInfo().IsGenericType)
                {
                    Type td = t.GetGenericTypeDefinition();
                    isList = (td.Equals(typeof(List<>)) ||
            td.Equals(typeof(IList<>)) ||
            td.Equals(typeof(ICollection<>)) ||
            td.Equals(typeof(IEnumerable<>)));
                }
                isList = (isList && t.GenericTypeArguments.Length == 1);
                if (isList)
                {
                    objectType = t.GenericTypeArguments[0];
                    Type listType = typeof(List<>).MakeGenericType(objectType);
                    listObject = Activator.CreateInstance(listType);
                }
#endif
                if (listObject == null)
                {
                    if (t.Equals(typeof(IList)) ||
                      t.Equals(typeof(ICollection)) || t.Equals(typeof(IEnumerable)))
                    {
                        listObject = new List<object>();
                        objectType = typeof(object);
                    }
                }
                if (listObject != null)
                {
                    System.Collections.IList ie = (System.Collections.IList)listObject;
                    foreach (CBORObject value in objThis.Values)
                    {
                        ie.Add(value.ToObject(objectType, mapper, options, depth + 1));
                    }
                    return listObject;
                }
            }
            if (objThis.Type == CBORType.Map)
            {
                bool isDict = false;
                Type keyType = null;
                Type valueType = null;
                object dictObject = null;
#if NET40 || NET20
        isDict = t.IsGenericType;
        if (t.IsGenericType) {
          Type td = t.GetGenericTypeDefinition();
          isDict = td.Equals(typeof(Dictionary<,>)) ||
            td.Equals(typeof(IDictionary<,>));
        }
        // DebugUtility.Log("list=" + isDict);
        isDict = isDict && t.GetGenericArguments().Length == 2;
        // DebugUtility.Log("list=" + isDict);
        if (isDict) {
          keyType = t.GetGenericArguments()[0];
          valueType = t.GetGenericArguments()[1];
          Type listType = typeof(Dictionary<,>).MakeGenericType(
            keyType,
            valueType);
          dictObject = Activator.CreateInstance(listType);
        }
#else
                isDict = (t.GetTypeInfo().IsGenericType);
                if (t.GetTypeInfo().IsGenericType)
                {
                    Type td = t.GetGenericTypeDefinition();
                    isDict = (td.Equals(typeof(Dictionary<,>)) ||
            td.Equals(typeof(IDictionary<,>)));
                }
                //DebugUtility.Log("list=" + isDict);
                isDict = (isDict && t.GenericTypeArguments.Length == 2);
                //DebugUtility.Log("list=" + isDict);
                if (isDict)
                {
                    keyType = t.GenericTypeArguments[0];
                    valueType = t.GenericTypeArguments[1];
                    Type listType = typeof(Dictionary<,>).MakeGenericType(
                      keyType,
                      valueType);
                    dictObject = Activator.CreateInstance(listType);
                }
#endif
                if (dictObject == null)
                {
                    if (t.Equals(typeof(IDictionary)))
                    {
                        dictObject = new Dictionary<object, object>();
                        keyType = typeof(object);
                        valueType = typeof(object);
                    }
                }
                if (dictObject != null)
                {
                    System.Collections.IDictionary idic =
                      (System.Collections.IDictionary)dictObject;
                    foreach (CBORObject key in objThis.Keys)
                    {
                        CBORObject value = objThis[key];
                        idic.Add(
              key.ToObject(keyType, mapper, options, depth + 1),
              value.ToObject(valueType, mapper, options, depth + 1));
                    }
                    return dictObject;
                }
                if (mapper != null)
                {
                    if (!mapper.FilterTypeName(t.FullName))
                    {
                        throw new CBORException("Type " + t.FullName +
                              " not supported");
                    }
                }
                else
                {
                    if (t.FullName != null && (StartsWith(t.FullName, "Microsoft.Win32."
             ) ||
                          StartsWith(t.FullName, "System.IO.")))
                    {
                        throw new CBORException("Type " + t.FullName +
                              " not supported");
                    }
                    if (StartsWith(t.FullName, "System.") &&
                      !HasCustomAttribute(t, "System.SerializableAttribute"))
                    {
                        throw new CBORException("Type " + t.FullName +
                              " not supported");
                    }
                }
                List<KeyValuePair<string, CBORObject>> values = new List<KeyValuePair<string, CBORObject>>();
                foreach (string key in PropertyMap2.GetPropertyNames(
                           t,
                           options != null ? options.UseCamelCase : true))
                {
                    if (objThis.ContainsKey(key))
                    {
                        CBORObject cborValue = objThis[key];
                        KeyValuePair<string, CBORObject> dict = new KeyValuePair<string, CBORObject>(
                          key,
                          cborValue);
                        values.Add(dict);
                    }
                }
                return PropertyMap2.ObjectWithProperties(
            t,
            values,
            mapper,
            options,
            depth);
            }
            else
            {
                throw new CBORException();
            }
        }

        public static object ObjectWithProperties(
             Type t,
             IEnumerable<KeyValuePair<string, CBORObject>> keysValues,
           CBORTypeMapper mapper,
           PODOptions options,
     int depth)
        {
            object o = Activator.CreateInstance(t);
            Dictionary<string, CBORObject> dict = new Dictionary<string, CBORObject>();
            foreach (KeyValuePair<string, CBORObject> kv in keysValues)
            {
                string name = kv.Key;
                dict[name] = kv.Value;
            }
            foreach (PropertyData key in GetPropertyList(o.GetType()))
            {
                if (!key.HasUsableSetter() || !key.HasUsableGetter())
                {
                    // Require properties to have both a setter and
                    // a getter to be eligible for setting
                    continue;
                }
                string name = key.GetAdjustedName(options != null ? options.UseCamelCase :
                       true);
                if (dict.ContainsKey(name))
                {
                    object dobj = dict[name].ToObject(
            key.Prop.PropertyType,
            mapper,
            options,
            depth + 1);
                    key.Prop.SetValue(o, dobj, null);
                }
            }
            return o;
        }

        public static IEnumerable<KeyValuePair<string, object>>
        GetProperties(Object o)
        {
            return GetProperties(o, true);
        }

        public static IEnumerable<string>
        GetPropertyNames(Type t, bool useCamelCase)
        {
            foreach (PropertyData key in GetPropertyList(t))
            {
                yield return key.GetAdjustedName(useCamelCase);
            }
        }

        public static IEnumerable<KeyValuePair<string, object>>
        GetProperties(Object o, bool useCamelCase)
        {
            foreach (PropertyData key in GetPropertyList(o.GetType()))
            {
                if (!key.HasUsableGetter())
                {
                    continue;
                }
                yield return new KeyValuePair<string, object>(
          key.GetAdjustedName(useCamelCase),
          key.Prop.GetValue(o, null));
            }
        }

        public static void BreakDownDateTime(
      DateTime bi,
      EInteger[] year,
      int[] lf)
        {
#if NET20
      DateTime dt = bi.ToUniversalTime();
#else
            DateTime dt = TimeZoneInfo.ConvertTime(bi, TimeZoneInfo.Utc);
#endif
            year[0] = EInteger.FromInt32(dt.Year);
            lf[0] = dt.Month;
            lf[1] = dt.Day;
            lf[2] = dt.Hour;
            lf[3] = dt.Minute;
            lf[4] = dt.Second;
            // lf[5] is the number of nanoseconds
            lf[5] = (int)(dt.Ticks % 10000000L) * 100;
        }

        public static DateTime BuildUpDateTime(EInteger year, int[] dt)
        {
            return new DateTime(
        year.ToInt32Checked(),
        dt[0],
        dt[1],
        dt[2],
        dt[3],
        dt[4],
        DateTimeKind.Utc).AddMinutes(-dt[6]).AddTicks((long)(dt[5] / 100));
        }
    }
    */
    #endregion

    #region SharedRefs
    internal class SharedRefs
    {
        private readonly IList<CBORObject> sharedObjects;

        public SharedRefs()
        {
            this.sharedObjects = new List<CBORObject>();
        }

        public void AddObject(CBORObject obj)
        {
            this.sharedObjects.Add(obj);
        }

        public CBORObject GetObject(long smallIndex)
        {
            if (smallIndex < 0)
            {
                throw new CBORException("Unexpected index");
            }
            if (smallIndex > Int32.MaxValue)
            {
                throw new CBORException("Index " + smallIndex +
                  " is bigger than supported ");
            }
            var index = (int)smallIndex;
            if (index >= this.sharedObjects.Count)
            {
                throw new CBORException("Index " + index + " is not valid");
            }
            return this.sharedObjects[index];
        }

        public CBORObject GetObject(EInteger bigIndex)
        {
            if (bigIndex.Sign < 0)
            {
                throw new CBORException("Unexpected index");
            }
            if (!bigIndex.CanFitInInt32())
            {
                throw new CBORException("Index " + bigIndex +
                  " is bigger than supported ");
            }
            var index = (int)bigIndex;
            if (index >= this.sharedObjects.Count)
            {
                throw new CBORException("Index " + index + " is not valid");
            }
            return this.sharedObjects[index];
        }
    }
    #endregion

    #region StringOutput
    internal sealed class StringOutput
    {
        private readonly StringBuilder builder;
        private readonly Stream outputStream;

        public StringOutput(StringBuilder builder)
        {
            this.builder = builder;
            this.outputStream = null;
        }

        public StringOutput(Stream outputStream)
        {
            this.outputStream = outputStream;
            this.builder = null;
        }

        public void WriteString(string str)
        {
            if (this.outputStream != null)
            {
                if (str.Length == 1)
                {
                    this.WriteCodePoint((int)str[0]);
                }
                else
                {
                    if (DataUtilities.WriteUtf8(
                      str,
                      0,
                      str.Length,
                      this.outputStream,
                      false) < 0)
                    {
                        throw new ArgumentException("str has an unpaired surrogate");
                    }
                }
            }
            else
            {
                this.builder.Append(str);
            }
        }

        public void WriteString(string str, int index, int length)
        {
            if (this.outputStream == null)
            {
                this.builder.Append(str, index, length);
            }
            else
            {
                if (length == 1)
                {
                    this.WriteCodePoint((int)str[index]);
                }
                else
                {
                    if (
                      DataUtilities.WriteUtf8(
                        str,
                        index,
                        length,
                        this.outputStream,
                        false) < 0)
                    {
                        throw new ArgumentException("str has an unpaired surrogate");
                    }
                }
            }
        }

        public void WriteAscii(byte[] bytes, int index, int length)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }
            if (index < 0)
            {
                throw new ArgumentException("\"index\" (" + index + ") is not" +
        "\u0020greater or equal to 0");
            }
            if (index > bytes.Length)
            {
                throw new ArgumentException("\"index\" (" + index + ") is not less" +
        "\u0020or equal to " + bytes.Length);
            }
            if (length < 0)
            {
                throw new ArgumentException(" (" + length + ") is not greater or" +
        "\u0020equal to 0");
            }
            if (length > bytes.Length)
            {
                throw new ArgumentException(" (" + length + ") is not less or equal" +
        "\u0020to " + bytes.Length);
            }
            if (bytes.Length - index < length)
            {
                throw new ArgumentException("\"bytes\" + \"'s length minus \" +" +
        "\u0020index (" + (bytes.Length - index) + ") is not greater or equal to " +
        length);
            }
            if (this.outputStream == null)
            {
                DataUtilities.ReadUtf8FromBytes(
                  bytes,
                  index,
                  length,
                  this.builder,
                  false);
            }
            else
            {
                for (var i = 0; i < length; ++i)
                {
                    byte b = bytes[i + index];
                    if ((((int)b) & 0x7f) != b)
                    {
                        throw new ArgumentException("str is non-ASCII");
                    }
                }
                this.outputStream.Write(bytes, index, length);
            }
        }

        public void WriteCodePoint(int codePoint)
        {
            if ((codePoint >> 7) == 0)
            {
                // Code point is in the Basic Latin range (U+0000 to U+007F)
                if (this.outputStream == null)
                {
                    this.builder.Append((char)codePoint);
                }
                else
                {
                    this.outputStream.WriteByte((byte)codePoint);
                }
                return;
            }
            if (codePoint < 0)
            {
                throw new ArgumentException("codePoint(" + codePoint +
                  ") is less than 0");
            }
            if (codePoint > 0x10ffff)
            {
                throw new ArgumentException("codePoint(" + codePoint +
                  ") is more than " + 0x10ffff);
            }
            if (this.outputStream != null)
            {
                if (codePoint < 0x80)
                {
                    this.outputStream.WriteByte((byte)codePoint);
                }
                else if (codePoint <= 0x7ff)
                {
                    this.outputStream.WriteByte((byte)(0xc0 | ((codePoint >> 6) &
                          0x1f)));
                    this.outputStream.WriteByte((byte)(0x80 | (codePoint & 0x3f)));
                }
                else if (codePoint <= 0xffff)
                {
                    if ((codePoint & 0xf800) == 0xd800)
                    {
                        throw new ArgumentException("ch is a surrogate");
                    }
                    this.outputStream.WriteByte((byte)(0xe0 | ((codePoint >> 12) &
                          0x0f)));
                    this.outputStream.WriteByte((byte)(0x80 | ((codePoint >> 6) &
                          0x3f)));
                    this.outputStream.WriteByte((byte)(0x80 | (codePoint & 0x3f)));
                }
                else
                {
                    this.outputStream.WriteByte((byte)(0xf0 | ((codePoint >> 18) &
                          0x07)));
                    this.outputStream.WriteByte((byte)(0x80 | ((codePoint >> 12) &
                          0x3f)));
                    this.outputStream.WriteByte((byte)(0x80 | ((codePoint >> 6) &
                          0x3f)));
                    this.outputStream.WriteByte((byte)(0x80 | (codePoint & 0x3f)));
                }
            }
            else
            {
                if ((codePoint & 0xfff800) == 0xd800)
                {
                    throw new ArgumentException("ch is a surrogate");
                }
                if (codePoint <= 0xffff)
                {
                    {
                        this.builder.Append((char)codePoint);
                    }
                }
                else if (codePoint <= 0x10ffff)
                {
                    this.builder.Append((char)((((codePoint - 0x10000) >> 10) & 0x3ff) |
                        0xd800));
                    this.builder.Append((char)(((codePoint - 0x10000) & 0x3ff) |
                        0xdc00));
                }
            }
        }
    }

    #endregion

    #region StringRefs
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:StringRefs"]/*'/>
    /// <summary>Implements CBOR string references, described at
    /// <c>http://cbor.schmorp.de/stringref</c>.</summary>
    internal class StringRefs
    {
        private readonly List<List<CBORObject>> stack;

        public StringRefs()
        {
            this.stack = new List<List<CBORObject>>();
            var firstItem = new List<CBORObject>();
            this.stack.Add(firstItem);
        }

        public void Push()
        {
            var firstItem = new List<CBORObject>();
            this.stack.Add(firstItem);
        }

        public void Pop()
        {
#if DEBUG
            if (this.stack.Count <= 0)
            {
                throw new ArgumentException("this.stack.Count(" + this.stack.Count +
                  ") is not greater than " + "0 ");
            }
#endif
            this.stack.RemoveAt(this.stack.Count - 1);
        }

        public void AddStringIfNeeded(CBORObject str, int lengthHint)
        {
#if DEBUG
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            if (!(str.Type == CBORType.ByteString || str.Type ==
                CBORType.TextString))
            {
                throw new
                ArgumentException(
                  "doesn't satisfy str.Type== ByteString or TextString");
            }
            if (lengthHint < 0)
            {
                throw new ArgumentException("lengthHint(" + lengthHint +
                  ") is less than " + "0 ");
            }
#endif
            var addStr = false;
            List<CBORObject> lastList = this.stack[this.stack.Count - 1];
            if (lastList.Count < 24)
            {
                addStr |= lengthHint >= 3;
            }
            else if (lastList.Count < 256)
            {
                addStr |= lengthHint >= 4;
            }
            else if (lastList.Count < 65536)
            {
                addStr |= lengthHint >= 5;
            }
            else
            {
                // NOTE: lastList's size can't be higher than (2^64)-1
                addStr |= lengthHint >= 7;
            }
            // NOTE: An additional branch, with lengthHint >= 11, would
            // be needed if the size could be higher than (2^64)-1
            if (addStr)
            {
                lastList.Add(str);
            }
        }

        public CBORObject GetString(long smallIndex)
        {
            if (smallIndex < 0)
            {
                throw new CBORException("Unexpected index");
            }
            if (smallIndex > Int32.MaxValue)
            {
                throw new CBORException("Index " + smallIndex +
                  " is bigger than supported ");
            }
            var index = (int)smallIndex;
            List<CBORObject> lastList = this.stack[this.stack.Count - 1];
            if (index >= lastList.Count)
            {
                throw new CBORException("Index " + index + " is not valid");
            }
            CBORObject ret = lastList[index];
            // Byte strings are mutable, so make a copy
            return (ret.Type == CBORType.ByteString) ?
              CBORObject.FromObject(ret.GetByteString()) : ret;
        }

        public CBORObject GetString(EInteger bigIndex)
        {
            if (bigIndex.Sign < 0)
            {
                throw new CBORException("Unexpected index");
            }
            if (!bigIndex.CanFitInInt32())
            {
                throw new CBORException("Index " + bigIndex +
                  " is bigger than supported ");
            }
            var index = (int)bigIndex;
            List<CBORObject> lastList = this.stack[this.stack.Count - 1];
            if (index >= lastList.Count)
            {
                throw new CBORException("Index " + index + " is not valid");
            }
            CBORObject ret = lastList[index];
            // Byte strings are mutable, so make a copy
            return (ret.Type == CBORType.ByteString) ?
              CBORObject.FromObject(ret.GetByteString()) : ret;
        }
    }
    #endregion
}
