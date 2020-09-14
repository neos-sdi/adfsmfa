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
using System.IO;
using System.Reflection;
using System.Text;

#pragma warning disable 618

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor
{
    #region BigInterger
    /// <include file='../docs.xml'
    /// path='docs/doc[@name="T:PeterO.BigInteger"]/*'/>
    [Obsolete("Use EInteger from PeterO.Numbers/com.upokecenter.numbers and the output of this class's ToString method.")]
    public sealed partial class BigInteger : IComparable<BigInteger>, IEquatable<BigInteger>
    {
        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.BigInteger.ONE"]/*'/>
#if CODE_ANALYSIS
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Microsoft.Security",
    "CA2104",
    Justification = "BigInteger is immutable")]
#endif

        [Obsolete("Use EInteger from PeterO.Numbers/com.upokecenter.numbers.")]
        public static readonly BigInteger ONE = new BigInteger(EInteger.One);

        private static readonly BigInteger ValueOneValue = new
            BigInteger(EInteger.One);

        private readonly EInteger ei;

        internal BigInteger(EInteger ei)
        {
            this.ei = ei ?? throw new ArgumentNullException(nameof(ei));
        }

        internal static BigInteger ToLegacy(EInteger ei)
        {
            return new BigInteger(ei);
        }

        internal static EInteger FromLegacy(BigInteger bei)
        {
            return bei.Ei;
        }

        private static readonly BigInteger ValueZeroValue = new
            BigInteger(EInteger.Zero);

        internal EInteger Ei
        {
            get
            {
                return this.ei;
            }
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.BigInteger.fromBytes(System.Byte[],System.Boolean)"]/*'/>
        public static BigInteger fromBytes(byte[] bytes, bool littleEndian)
        {
            return new BigInteger(EInteger.FromBytes(bytes, littleEndian));
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.BigInteger.fromRadixString(System.String,System.Int32)"]/*'/>
        public static BigInteger fromRadixString(string str, int radix)
        {
            return new BigInteger(EInteger.FromRadixString(str, radix));
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.BigInteger.fromString(System.String)"]/*'/>
        public static BigInteger fromString(string str)
        {
            return new BigInteger(EInteger.FromString(str));
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.BigInteger.valueOf(System.Int64)"]/*'/>
        public static BigInteger valueOf(long longerValue)
        {
            return new BigInteger(EInteger.FromInt64(longerValue));
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.BigInteger.bitLength"]/*'/>
        public int bitLength()
        {
            return this.Ei.GetSignedBitLength();
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.BigInteger.Equals(System.Object)"]/*'/>
        public override bool Equals(object obj)
        {
            return (!(obj is BigInteger bi)) ? false : this.Ei.Equals(bi.Ei);
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.BigInteger.GetHashCode"]/*'/>
        public override int GetHashCode()
        {
            return this.Ei.GetHashCode();
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.BigInteger.toBytes(System.Boolean)"]/*'/>
        public byte[] toBytes(bool littleEndian)
        {
            return this.Ei.ToBytes(littleEndian);
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.BigInteger.toRadixString(System.Int32)"]/*'/>
        public string toRadixString(int radix)
        {
            return this.Ei.ToRadixString(radix);
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.BigInteger.ToString"]/*'/>
        public override string ToString()
        {
            return this.Ei.ToString();
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.BigInteger.CompareTo(PeterO.BigInteger)"]/*'/>
        public int CompareTo(BigInteger other)
        {
            return this.Ei.CompareTo(other?.Ei);
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="P:PeterO.BigInteger.Zero"]/*'/>
        
        [Obsolete("Use EInteger from PeterO.Numbers/com.upokecenter.numbers and the output of this class's ToString method.")]
        public static BigInteger Zero
        {
            get
            {
                return ValueZeroValue;
            }
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="P:PeterO.BigInteger.One"]/*'/>
        
        [Obsolete("Use EInteger from PeterO.Numbers/com.upokecenter.numbers and the output of this class's ToString method.")]
        public static BigInteger One
        {
            get
            {
                return ValueOneValue;
            }
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.BigInteger.Equals(PeterO.BigInteger)"]/*'/>
        [Obsolete("Use EInteger from PeterO.Numbers/com.upokecenter.numbers and the output of this class's ToString method.")]
        public bool Equals(BigInteger other)
        {
            return this.Equals((object)other);
        }
    }
    #endregion

    #region DataUtilities
    /// <include file='../docs.xml'
    /// path='docs/doc[@name="T:PeterO.DataUtilities"]/*'/>
    public static class DataUtilities
    {
        // TODO: In CodePointAt/CodePointBefore, consider adding
        // mode to return -2 or throw an exception on unpaired surrogate
        private const int StreamedStringBufferLength = 4096;

        /// <summary>Generates a text string from a UTF-8 byte array.</summary>
        /// <param name='bytes'>A byte array containing text encoded in
        /// UTF-8.</param>
        /// <param name='replace'>If true, replaces invalid encoding with the
        /// replacement character (U+FFFD). If false, stops processing when
        /// invalid UTF-8 is seen.</param>
        /// <returns>A string represented by the UTF-8 byte array.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='bytes'/> is null.</exception>
        /// <exception cref='ArgumentException'>The string is not valid UTF-8
        /// and <paramref name='replace'/> is false.</exception>
        public static string GetUtf8String(byte[] bytes, bool replace)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }
            var b = new StringBuilder();
            if (ReadUtf8FromBytes(bytes, 0, bytes.Length, b, replace) != 0)
            {
                throw new ArgumentException("Invalid UTF-8");
            }
            return b.ToString();
        }

        /// <summary>Finds the number of Unicode code points in the given text
        /// string. Unpaired surrogate code points increase this number by 1.
        /// This is not necessarily the length of the string in "char"
        /// s.</summary>
        /// <param name='str'>The parameter <paramref name='str'/> is a text
        /// string.</param>
        /// <returns>The number of Unicode code points in the given
        /// string.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='str'/> is null.</exception>
        public static int CodePointLength(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            var i = 0;
            var count = 0;
            while (i < str.Length)
            {
                int c = CodePointAt(str, i);
                ++count;
                i += (c >= 0x10000) ? 2 : 1;
            }
            return count;
        }

        /// <summary>Generates a text string from a portion of a UTF-8 byte
        /// array.</summary>
        /// <param name='bytes'>A byte array containing text encoded in
        /// UTF-8.</param>
        /// <param name='offset'>Offset into the byte array to start
        /// reading.</param>
        /// <param name='bytesCount'>Length, in bytes, of the UTF-8 text
        /// string.</param>
        /// <param name='replace'>If true, replaces invalid encoding with the
        /// replacement character (U+FFFD). If false, stops processing when
        /// invalid UTF-8 is seen.</param>
        /// <returns>A string represented by the UTF-8 byte array.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='bytes'/> is null.</exception>
        /// <exception cref='ArgumentException'>The portion of the byte array
        /// is not valid UTF-8 and <paramref name='replace'/> is
        /// false.</exception>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='offset'/> is less than 0, <paramref name='bytesCount'/> is
        /// less than 0, or offset plus bytesCount is greater than the length
        /// of "data" .</exception>
        public static string GetUtf8String(
          byte[] bytes,
          int offset,
          int bytesCount,
          bool replace)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }
            if (offset < 0)
            {
                throw new ArgumentException("offset(" + offset + ") is less than " +
                  "0");
            }
            if (offset > bytes.Length)
            {
                throw new ArgumentException("offset(" + offset + ") is more than " +
                  bytes.Length);
            }
            if (bytesCount < 0)
            {
                throw new ArgumentException("bytesCount(" + bytesCount +
                  ") is less than 0");
            }
            if (bytesCount > bytes.Length)
            {
                throw new ArgumentException("bytesCount(" + bytesCount +
                  ") is more than " + bytes.Length);
            }
            if (bytes.Length - offset < bytesCount)
            {
                throw new ArgumentException("bytes's length minus " + offset + "(" +
                  (bytes.Length - offset) + ") is less than " + bytesCount);
            }
            var b = new StringBuilder();
            if (ReadUtf8FromBytes(bytes, offset, bytesCount, b, replace) != 0)
            {
                throw new ArgumentException("Invalid UTF-8");
            }
            return b.ToString();
        }

        /// <summary>
        /// <para>Encodes a string in UTF-8 as a byte array. This method does
        /// not insert a byte-order mark (U+FEFF) at the beginning of the
        /// encoded byte array.</para>
        /// <para>REMARK: It is not recommended to use
        /// <c>Encoding.UTF8.GetBytes</c> in.NET, or the <c>getBytes()</c>
        /// method in Java to do this. For instance, <c>getBytes()</c> encodes
        /// text strings in a default (so not fixed) character encoding, which
        /// can be undesirable.</para></summary>
        /// <param name='str'>The parameter <paramref name='str'/> is a text
        /// string.</param>
        /// <param name='replace'>If true, replaces unpaired surrogate code
        /// points with the replacement character (U+FFFD). If false, stops
        /// processing when an unpaired surrogate code point is seen.</param>
        /// <returns>The string encoded in UTF-8.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='str'/> is null.</exception>
        /// <exception cref='ArgumentException'>The string contains an unpaired
        /// surrogate code point and <paramref name='replace'/> is false, or an
        /// internal error occurred.</exception>
        public static byte[] GetUtf8Bytes(string str, bool replace)
        {
            return GetUtf8Bytes(str, replace, false);
        }

        /// <summary>
        /// <para>Encodes a string in UTF-8 as a byte array. This method does
        /// not insert a byte-order mark (U+FEFF) at the beginning of the
        /// encoded byte array.</para>
        /// <para>REMARK: It is not recommended to use
        /// <c>Encoding.UTF8.GetBytes</c> in.NET, or the <c>getBytes()</c>
        /// method in Java to do this. For instance, <c>getBytes()</c> encodes
        /// text strings in a default (so not fixed) character encoding, which
        /// can be undesirable.</para></summary>
        /// <param name='str'>The parameter <paramref name='str'/> is a text
        /// string.</param>
        /// <param name='replace'>If true, replaces unpaired surrogate code
        /// points with the replacement character (U+FFFD). If false, stops
        /// processing when an unpaired surrogate code point is seen.</param>
        /// <param name='lenientLineBreaks'>If true, replaces carriage return
        /// (CR) not followed by line feed (LF) and LF not preceded by CR with
        /// CR-LF pairs.</param>
        /// <returns>The string encoded in UTF-8.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='str'/> is null.</exception>
        /// <exception cref='ArgumentException'>The string contains an unpaired
        /// surrogate code point and <paramref name='replace'/> is false, or an
        /// internal error occurred.</exception>
        public static byte[] GetUtf8Bytes(
          string str,
          bool replace,
          bool lenientLineBreaks)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            if (!lenientLineBreaks && str.Length == 1)
            {
                int c = str[0];
                if ((c & 0xf800) == 0xd800)
                {
                    if (replace)
                    {
                        c = 0xfffd;
                    }
                    else
                    {
                        throw new ArgumentException("Unpaired surrogate code point");
                    }
                }
                if (c <= 0x80)
                {
                    return new byte[] { (byte)c };
                }
                else if (c <= 0x7ff)
                {
                    return new byte[] {
            (byte)(0xc0 | ((c >> 6) & 0x1f)),
            (byte)(0x80 | (c & 0x3f)),
          };
                }
                else
                {
                    return new byte[] {
            (byte)(0xe0 | ((c >> 12) & 0x0f)),
            (byte)(0x80 | ((c >> 6) & 0x3f)),
            (byte)(0x80 | (c & 0x3f)),
          };
                }
            }
            else if (str.Length == 2)
            {
                int c = str[0];
                int c2 = str[1];
                if ((c & 0xfc00) == 0xd800 && (c2 & 0xfc00) == 0xdc00)
                {
                    c = 0x10000 + ((c & 0x3ff) << 10) + (c2 & 0x3ff);
                    return new byte[] {
            (byte)(0xf0 | ((c >> 18) & 0x07)),
            (byte)(0x80 | ((c >> 12) & 0x3f)),
            (byte)(0x80 | ((c >> 6) & 0x3f)),
            (byte)(0x80 | (c & 0x3f)),
          };
                }
                else if (!lenientLineBreaks && c <= 0x80 && c2 <= 0x80)
                {
                    return new byte[] { (byte)c, (byte)c2 };
                }
            }
            try
            {
                using (var ms = new MemoryStream())
                {
                    if (WriteUtf8(str, 0, str.Length, ms, replace, lenientLineBreaks) !=
                      0)
                    {
                        throw new ArgumentException("Unpaired surrogate code point");
                    }
                    return ms.ToArray();
                }
            }
            catch (IOException ex)
            {
                throw new ArgumentException("I/O error occurred", ex);
            }
        }

        /// <summary>Calculates the number of bytes needed to encode a string
        /// in UTF-8.</summary>
        /// <param name='str'>The parameter <paramref name='str'/> is a text
        /// string.</param>
        /// <param name='replace'>If true, treats unpaired surrogate code
        /// points as having 3 UTF-8 bytes (the UTF-8 length of the replacement
        /// character U+FFFD).</param>
        /// <returns>The number of bytes needed to encode the given string in
        /// UTF-8, or -1 if the string contains an unpaired surrogate code
        /// point and <paramref name='replace'/> is false.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='str'/> is null.</exception>
        public static long GetUtf8Length(string str, bool replace)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            long size = 0;
            for (var i = 0; i < str.Length; ++i)
            {
                int c = str[i];
                if (c <= 0x7f)
                {
                    ++size;
                }
                else if (c <= 0x7ff)
                {
                    size += 2;
                }
                else if (c <= 0xd7ff || c >= 0xe000)
                {
                    size += 3;
                }
                else if (c <= 0xdbff)
                { // UTF-16 leading surrogate
                    ++i;
                    if (i >= str.Length || str[i] < 0xdc00 || str[i] > 0xdfff)
                    {
                        if (replace)
                        {
                            size += 3;
                            --i;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    else
                    {
                        size += 4;
                    }
                }
                else
                {
                    if (replace)
                    {
                        size += 3;
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
            return size;
        }

        /// <summary>Gets the Unicode code point just before the given index of
        /// the string.</summary>
        /// <param name='str'>The parameter <paramref name='str'/> is a text
        /// string.</param>
        /// <param name='index'>Index of the current position into the
        /// string.</param>
        /// <returns>The Unicode code point at the previous position. Returns
        /// -1 if <paramref name='index'/> is 0 or less, or is greater than or
        /// equal to the string's length. Returns the replacement character
        /// (U+FFFD) if the code point at the previous position is an unpaired
        /// surrogate code point. If the return value is 65536 (0x10000) or
        /// greater, the code point takes up two UTF-16 code units.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='str'/> is null.</exception>
        public static int CodePointBefore(string str, int index)
        {
            return CodePointBefore(str, index, 0);
        }

        /// <summary>Gets the Unicode code point just before the given index of
        /// the string.</summary>
        /// <param name='str'>The parameter <paramref name='str'/> is a text
        /// string.</param>
        /// <param name='index'>Index of the current position into the
        /// string.</param>
        /// <param name='surrogateBehavior'>Specifies what kind of value to
        /// return if the previous code point is an unpaired surrogate code
        /// point: if 0, return the replacement character (U+FFFD); if 1,
        /// return the value of the surrogate code point; if neither 0 nor 1,
        /// return -1.</param>
        /// <returns>The Unicode code point at the previous position. Returns
        /// -1 if <paramref name='index'/> is 0 or less, or is greater than or
        /// equal to the string's length. Returns a value as specified under
        /// <paramref name='surrogateBehavior'/> if the code point at the
        /// previous position is an unpaired surrogate code point. If the
        /// return value is 65536 (0x10000) or greater, the code point takes up
        /// two UTF-16 code units.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='str'/> is null.</exception>
        public static int CodePointBefore(
          string str,
          int index,
          int surrogateBehavior)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            if (index <= 0)
            {
                return -1;
            }
            if (index > str.Length)
            {
                return -1;
            }
            int c = str[index - 1];
            if ((c & 0xfc00) == 0xdc00 && index - 2 >= 0 &&
              (str[index - 2] & 0xfc00) == 0xd800)
            {
                // Get the Unicode code point for the surrogate pair
                return 0x10000 + ((str[index - 2] & 0x3ff) << 10) + (c & 0x3ff);
            }
            // unpaired surrogate
            if ((c & 0xf800) == 0xd800)
            {
                return (surrogateBehavior == 0) ? 0xfffd : ((surrogateBehavior == 1) ?
                    c : -1);
            }
            return c;
        }

        /// <summary>Gets the Unicode code point at the given index of the
        /// string.</summary>
        /// <param name='str'>The parameter <paramref name='str'/> is a text
        /// string.</param>
        /// <param name='index'>Index of the current position into the
        /// string.</param>
        /// <returns>The Unicode code point at the given position. Returns -1
        /// if <paramref name='index'/> is 0 or less, or is greater than or
        /// equal to the string's length. Returns the replacement character
        /// (U+FFFD) if the code point at that position is an unpaired
        /// surrogate code point. If the return value is 65536 (0x10000) or
        /// greater, the code point takes up two UTF-16 code units.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='str'/> is null.</exception>
        public static int CodePointAt(string str, int index)
        {
            return CodePointAt(str, index, 0);
        }

        /// <summary>Gets the Unicode code point at the given index of the
        /// string.</summary>
        /// <param name='str'>The parameter <paramref name='str'/> is a text
        /// string.</param>
        /// <param name='index'>Index of the current position into the
        /// string.</param>
        /// <param name='surrogateBehavior'>Specifies what kind of value to
        /// return if the code point at the given index is an unpaired
        /// surrogate code point: if 0, return the replacement character (U +
        /// FFFD); if 1, return the value of the surrogate code point; if
        /// neither 0 nor 1, return -1.</param>
        /// <returns>The Unicode code point at the given position. Returns -1
        /// if <paramref name='index'/> is 0 or less, or is greater than or
        /// equal to the string's length. Returns a value as specified under
        /// <paramref name='surrogateBehavior'/> if the code point at that
        /// position is an unpaired surrogate code point. If the return value
        /// is 65536 (0x10000) or greater, the code point takes up two UTF-16
        /// code units.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='str'/> is null.</exception>
        /// <example>
        /// <para>The following example shows how to iterate a text string code
        /// point by code point, terminating the loop when an unpaired
        /// surrogate is found.</para>
        /// <code>for (var i = 0;i&lt;str.Length; ++i) { int codePoint =
        /// DataUtilities.CodePointAt(str, i, 2); if (codePoint &lt; 0) { break; /*
        /// Unpaired surrogate */ } Console.WriteLine("codePoint:"+codePoint); if
        /// (codePoint &gt;= 0x10000) { i++; /* Supplementary code point */ } }</code>
        ///  .
        /// </example>
        public static int CodePointAt(
          string str,
          int index,
          int surrogateBehavior)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            if (index >= str.Length)
            {
                return -1;
            }
            if (index < 0)
            {
                return -1;
            }
            int c = str[index];
            if ((c & 0xfc00) == 0xd800 && index + 1 < str.Length &&
              (str[index + 1] & 0xfc00) == 0xdc00)
            {
                // Get the Unicode code point for the surrogate pair
                c = 0x10000 + ((c & 0x3ff) << 10) + (str[index + 1] & 0x3ff);
            }
            else if ((c & 0xf800) == 0xd800)
            {
                // unpaired surrogate
                return (surrogateBehavior == 0) ? 0xfffd : ((surrogateBehavior == 1) ?
                    c : (-1));
            }
            return c;
        }

        /// <summary>Returns a string with the basic upper-case letters A to Z
        /// (U+0041 to U+005A) converted to the corresponding basic lower-case
        /// letters. Other characters remain unchanged.</summary>
        /// <param name='str'>The parameter <paramref name='str'/> is a text
        /// string.</param>
        /// <returns>The converted string, or null if <paramref name='str'/> is
        /// null.</returns>
        public static string ToLowerCaseAscii(string str)
        {
            if (str == null)
            {
                return null;
            }
            var len = str.Length;
            var c = (char)0;
            var hasUpperCase = false;
            for (var i = 0; i < len; ++i)
            {
                c = str[i];
                if (c >= 'A' && c <= 'Z')
                {
                    hasUpperCase = true;
                    break;
                }
            }
            if (!hasUpperCase)
            {
                return str;
            }
            var builder = new StringBuilder();
            for (var i = 0; i < len; ++i)
            {
                c = str[i];
                if (c >= 'A' && c <= 'Z')
                {
                    builder.Append((char)(c + 0x20));
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        /// <summary>Returns a string with the basic lower-case letters A to Z
        /// (U+0061 to U+007A) converted to the corresponding basic upper-case
        /// letters. Other characters remain unchanged.</summary>
        /// <param name='str'>The parameter <paramref name='str'/> is a text
        /// string.</param>
        /// <returns>The converted string, or null if <paramref name='str'/> is
        /// null.</returns>
        public static string ToUpperCaseAscii(string str)
        {
            if (str == null)
            {
                return null;
            }
            var len = str.Length;
            var c = (char)0;
            var hasLowerCase = false;
            for (var i = 0; i < len; ++i)
            {
                c = str[i];
                if (c >= 'a' && c <= 'z')
                {
                    hasLowerCase = true;
                    break;
                }
            }
            if (!hasLowerCase)
            {
                return str;
            }
            var builder = new StringBuilder();
            for (var i = 0; i < len; ++i)
            {
                c = str[i];
                if (c >= 'a' && c <= 'z')
                {
                    builder.Append((char)(c - 0x20));
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        /// <summary>Compares two strings in Unicode code point order. Unpaired
        /// surrogate code points are treated as individual code
        /// points.</summary>
        /// <param name='strA'>The first string. Can be null.</param>
        /// <param name='strB'>The second string. Can be null.</param>
        /// <returns>A value indicating which string is " less" or " greater" .
        /// 0: Both strings are equal or null. Less than 0: a is null and b
        /// isn't; or the first code point that's different is less in A than
        /// in B; or b starts with a and is longer than a. Greater than 0: b is
        /// null and a isn't; or the first code point that's different is
        /// greater in A than in B; or a starts with b and is longer than
        /// b.</returns>
        public static int CodePointCompare(string strA, string strB)
        {
            if (strA == null)
            {
                return (strB == null) ? 0 : -1;
            }
            if (strB == null)
            {
                return 1;
            }
            int len, ca, cb;
            len = Math.Min(strA.Length, strB.Length);
            for (var i = 0; i < len; ++i)
            {
                ca = strA[i];
                cb = strB[i];
                if (ca == cb)
                {
                    // normal code units and illegal surrogates
                    // are treated as single code points
                    if ((ca & 0xf800) != 0xd800)
                    {
                        continue;
                    }
                    var incindex = false;
                    if (i + 1 < strA.Length && (strA[i + 1] & 0xfc00) == 0xdc00)
                    {
                        ca = 0x10000 + ((ca & 0x3ff) << 10) + (strA[i + 1] & 0x3ff);
                        incindex = true;
                    }
                    if (i + 1 < strB.Length && (strB[i + 1] & 0xfc00) == 0xdc00)
                    {
                        cb = 0x10000 + ((cb & 0x3ff) << 10) + (strB[i + 1] & 0x3ff);
                        incindex = true;
                    }
                    if (ca != cb)
                    {
                        return ca - cb;
                    }
                    if (incindex)
                    {
                        ++i;
                    }
                }
                else
                {
                    if ((ca & 0xf800) != 0xd800 && (cb & 0xf800) != 0xd800)
                    {
                        return ca - cb;
                    }
                    if ((ca & 0xfc00) == 0xd800 && i + 1 < strA.Length &&
                      (strA[i + 1] & 0xfc00) == 0xdc00)
                    {
                        ca = 0x10000 + ((ca & 0x3ff) << 10) + (strA[i + 1] & 0x3ff);
                    }
                    if ((cb & 0xfc00) == 0xd800 && i + 1 < strB.Length &&
                      (strB[i + 1] & 0xfc00) == 0xdc00)
                    {
                        cb = 0x10000 + ((cb & 0x3ff) << 10) + (strB[i + 1] & 0x3ff);
                    }
                    return ca - cb;
                }
            }
            return (strA.Length == strB.Length) ? 0 : ((strA.Length < strB.Length) ?
                -1 : 1);
        }

        /// <summary>Writes a portion of a string in UTF-8 encoding to a data
        /// stream.</summary>
        /// <param name='str'>A string to write.</param>
        /// <param name='offset'>The Index starting at 0 where the string
        /// portion to write begins.</param>
        /// <param name='length'>The length of the string portion to
        /// write.</param>
        /// <param name='stream'>A writable data stream.</param>
        /// <param name='replace'>If true, replaces unpaired surrogate code
        /// points with the replacement character (U+FFFD). If false, stops
        /// processing when an unpaired surrogate code point is seen.</param>
        /// <returns>0 if the entire string portion was written; or -1 if the
        /// string portion contains an unpaired surrogate code point and
        /// <paramref name='replace'/> is false.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='str'/> is null or <paramref name='stream'/> is
        /// null.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        /// <exception cref='ArgumentException'>Either <paramref
        /// name='offset'/> or <paramref name='length'/> is less than 0 or
        /// greater than <paramref name='str'/> 's length, or <paramref
        /// name='str'/> 's length minus <paramref name='offset'/> is less than
        /// <paramref name='length'/>.</exception>
        public static int WriteUtf8(
          string str,
          int offset,
          int length,
          Stream stream,
          bool replace)
        {
            return WriteUtf8(str, offset, length, stream, replace, false);
        }

        /// <summary>Writes a portion of a string in UTF-8 encoding to a data
        /// stream.</summary>
        /// <param name='str'>A string to write.</param>
        /// <param name='offset'>The Index starting at 0 where the string
        /// portion to write begins.</param>
        /// <param name='length'>The length of the string portion to
        /// write.</param>
        /// <param name='stream'>A writable data stream.</param>
        /// <param name='replace'>If true, replaces unpaired surrogate code
        /// points with the replacement character (U+FFFD). If false, stops
        /// processing when an unpaired surrogate code point is seen.</param>
        /// <param name='lenientLineBreaks'>If true, replaces carriage return
        /// (CR) not followed by line feed (LF) and LF not preceded by CR with
        /// CR-LF pairs.</param>
        /// <returns>0 if the entire string portion was written; or -1 if the
        /// string portion contains an unpaired surrogate code point and
        /// <paramref name='replace'/> is false.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='str'/> is null or <paramref name='stream'/> is
        /// null.</exception>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='offset'/> is less than 0, <paramref name='length'/> is less
        /// than 0, or <paramref name='offset'/> plus <paramref name='length'/>
        /// is greater than the string's length.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        public static int WriteUtf8(
          string str,
          int offset,
          int length,
          Stream stream,
          bool replace,
          bool lenientLineBreaks)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            if (offset < 0)
            {
                throw new ArgumentException("offset(" + offset + ") is less than " +
                  "0");
            }
            if (offset > str.Length)
            {
                throw new ArgumentException("offset(" + offset + ") is more than " +
                  str.Length);
            }
            if (length < 0)
            {
                throw new ArgumentException("length(" + length + ") is less than " +
                  "0");
            }
            if (length > str.Length)
            {
                throw new ArgumentException("length(" + length + ") is more than " +
                  str.Length);
            }
            if (str.Length - offset < length)
            {
                throw new ArgumentException("str.Length minus offset(" +
                  (str.Length - offset) + ") is less than " + length);
            }
            if (length == 0)
            {
                return 0;
            }
            int endIndex, c;
            byte[] bytes;
            var retval = 0;
            // Take string portion's length into account when allocating
            // stream buffer, in case it's much smaller than the usual stream
            // string buffer length and to improve performance on small strings
            int bufferLength = Math.Min(StreamedStringBufferLength, length);
            if (bufferLength < StreamedStringBufferLength)
            {
                bufferLength = Math.Min(
                  StreamedStringBufferLength,
                  bufferLength * 3);
            }
            bytes = new byte[bufferLength];
            var byteIndex = 0;
            endIndex = offset + length;
            for (int index = offset; index < endIndex; ++index)
            {
                c = str[index];
                if (c <= 0x7f)
                {
                    if (lenientLineBreaks)
                    {
                        if (c == 0x0d && (index + 1 >= endIndex || str[index + 1] !=
                            0x0a))
                        {
                            // bare CR, convert to CRLF
                            if (byteIndex + 2 > StreamedStringBufferLength)
                            {
                                // Write bytes retrieved so far
                                stream.Write(bytes, 0, byteIndex);
                                byteIndex = 0;
                            }
                            bytes[byteIndex++] = 0x0d;
                            bytes[byteIndex++] = 0x0a;
                            continue;
                        }
                        else if (c == 0x0d)
                        {
                            // CR-LF pair
                            if (byteIndex + 2 > StreamedStringBufferLength)
                            {
                                // Write bytes retrieved so far
                                stream.Write(bytes, 0, byteIndex);
                                byteIndex = 0;
                            }
                            bytes[byteIndex++] = 0x0d;
                            bytes[byteIndex++] = 0x0a;
                            ++index;
                            continue;
                        }
                        if (c == 0x0a)
                        {
                            // bare LF, convert to CRLF
                            if (byteIndex + 2 > StreamedStringBufferLength)
                            {
                                // Write bytes retrieved so far
                                stream.Write(bytes, 0, byteIndex);
                                byteIndex = 0;
                            }
                            bytes[byteIndex++] = 0x0d;
                            bytes[byteIndex++] = 0x0a;
                            continue;
                        }
                    }
                    if (byteIndex >= StreamedStringBufferLength)
                    {
                        // Write bytes retrieved so far
                        stream.Write(bytes, 0, byteIndex);
                        byteIndex = 0;
                    }
                    bytes[byteIndex++] = (byte)c;
                }
                else if (c <= 0x7ff)
                {
                    if (byteIndex + 2 > StreamedStringBufferLength)
                    {
                        // Write bytes retrieved so far
                        stream.Write(bytes, 0, byteIndex);
                        byteIndex = 0;
                    }
                    bytes[byteIndex++] = (byte)(0xc0 | ((c >> 6) & 0x1f));
                    bytes[byteIndex++] = (byte)(0x80 | (c & 0x3f));
                }
                else
                {
                    if ((c & 0xfc00) == 0xd800 && index + 1 < endIndex &&
                      (str[index + 1] & 0xfc00) == 0xdc00)
                    {
                        // Get the Unicode code point for the surrogate pair
                        c = 0x10000 + ((c & 0x3ff) << 10) + (str[index + 1] & 0x3ff);
                        ++index;
                    }
                    else if ((c & 0xf800) == 0xd800)
                    {
                        // unpaired surrogate
                        if (!replace)
                        {
                            retval = -1;
                            break; // write bytes read so far
                        }
                        c = 0xfffd;
                    }
                    if (c <= 0xffff)
                    {
                        if (byteIndex + 3 > StreamedStringBufferLength)
                        {
                            // Write bytes retrieved so far
                            stream.Write(bytes, 0, byteIndex);
                            byteIndex = 0;
                        }
                        bytes[byteIndex++] = (byte)(0xe0 | ((c >> 12) & 0x0f));
                        bytes[byteIndex++] = (byte)(0x80 | ((c >> 6) & 0x3f));
                        bytes[byteIndex++] = (byte)(0x80 | (c & 0x3f));
                    }
                    else
                    {
                        if (byteIndex + 4 > StreamedStringBufferLength)
                        {
                            // Write bytes retrieved so far
                            stream.Write(bytes, 0, byteIndex);
                            byteIndex = 0;
                        }
                        bytes[byteIndex++] = (byte)(0xf0 | ((c >> 18) & 0x07));
                        bytes[byteIndex++] = (byte)(0x80 | ((c >> 12) & 0x3f));
                        bytes[byteIndex++] = (byte)(0x80 | ((c >> 6) & 0x3f));
                        bytes[byteIndex++] = (byte)(0x80 | (c & 0x3f));
                    }
                }
            }
            stream.Write(bytes, 0, byteIndex);
            return retval;
        }

        /// <summary>Writes a string in UTF-8 encoding to a data
        /// stream.</summary>
        /// <param name='str'>A string to write.</param>
        /// <param name='stream'>A writable data stream.</param>
        /// <param name='replace'>If true, replaces unpaired surrogate code
        /// points with the replacement character (U+FFFD). If false, stops
        /// processing when an unpaired surrogate code point is seen.</param>
        /// <returns>0 if the entire string was written; or -1 if the string
        /// contains an unpaired surrogate code point and <paramref
        /// name='replace'/> is false.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='str'/> is null or <paramref name='stream'/> is
        /// null.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        public static int WriteUtf8(string str, Stream stream, bool replace)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            return WriteUtf8(str, 0, str.Length, stream, replace);
        }

        /// <summary>Reads a string in UTF-8 encoding from a byte
        /// array.</summary>
        /// <param name='data'>A byte array containing a UTF-8 text
        /// string.</param>
        /// <param name='offset'>Offset into the byte array to start
        /// reading.</param>
        /// <param name='bytesCount'>Length, in bytes, of the UTF-8 text
        /// string.</param>
        /// <param name='builder'>A string builder object where the resulting
        /// string will be stored.</param>
        /// <param name='replace'>If true, replaces invalid encoding with the
        /// replacement character (U+FFFD). If false, stops processing when
        /// invalid UTF-8 is seen.</param>
        /// <returns>0 if the entire string was read without errors, or -1 if
        /// the string is not valid UTF-8 and <paramref name='replace'/> is
        /// false.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='data'/> is null or <paramref name='builder'/> is
        /// null.</exception>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='offset'/> is less than 0, <paramref name='bytesCount'/> is
        /// less than 0, or offset plus bytesCount is greater than the length
        /// of <paramref name='data'/>.</exception>
        public static int ReadUtf8FromBytes(
          byte[] data,
          int offset,
          int bytesCount,
          StringBuilder builder,
          bool replace)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (offset < 0)
            {
                throw new ArgumentException("offset(" + offset + ") is less than " +
                  "0");
            }
            if (offset > data.Length)
            {
                throw new ArgumentException("offset(" + offset + ") is more than " +
                  data.Length);
            }
            if (bytesCount < 0)
            {
                throw new ArgumentException("bytesCount(" + bytesCount +
                  ") is less than 0");
            }
            if (bytesCount > data.Length)
            {
                throw new ArgumentException("bytesCount(" + bytesCount +
                  ") is more than " + data.Length);
            }
            if (data.Length - offset < bytesCount)
            {
                throw new ArgumentException("data.Length minus offset(" +
                  (data.Length - offset) + ") is less than " + bytesCount);
            }
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            var cp = 0;
            var bytesSeen = 0;
            var bytesNeeded = 0;
            var lower = 0x80;
            var upper = 0xbf;
            int pointer, endpointer, b;
            pointer = offset;
            endpointer = offset + bytesCount;
            while (pointer < endpointer)
            {
                b = data[pointer] & (int)0xff;
                ++pointer;
                if (bytesNeeded == 0)
                {
                    if ((b & 0x7f) == b)
                    {
                        builder.Append((char)b);
                    }
                    else if (b >= 0xc2 && b <= 0xdf)
                    {
                        bytesNeeded = 1;
                        cp = (b - 0xc0) << 6;
                    }
                    else if (b >= 0xe0 && b <= 0xef)
                    {
                        lower = (b == 0xe0) ? 0xa0 : 0x80;
                        upper = (b == 0xed) ? 0x9f : 0xbf;
                        bytesNeeded = 2;
                        cp = (b - 0xe0) << 12;
                    }
                    else if (b >= 0xf0 && b <= 0xf4)
                    {
                        lower = (b == 0xf0) ? 0x90 : 0x80;
                        upper = (b == 0xf4) ? 0x8f : 0xbf;
                        bytesNeeded = 3;
                        cp = (b - 0xf0) << 18;
                    }
                    else
                    {
                        if (replace)
                        {
                            builder.Append((char)0xfffd);
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    continue;
                }
                if (b < lower || b > upper)
                {
                    cp = bytesNeeded = bytesSeen = 0;
                    lower = 0x80;
                    upper = 0xbf;
                    if (replace)
                    {
                        --pointer;
                        builder.Append((char)0xfffd);
                        continue;
                    }
                    return -1;
                }
                else
                {
                    lower = 0x80;
                    upper = 0xbf;
                    ++bytesSeen;
                    cp += (b - 0x80) << (6 * (bytesNeeded - bytesSeen));
                    if (bytesSeen != bytesNeeded)
                    {
                        continue;
                    }
                    int ret, ch, lead, trail;
                    ret = cp;
                    cp = 0;
                    bytesSeen = 0;
                    bytesNeeded = 0;
                    if (ret <= 0xffff)
                    {
                        builder.Append((char)ret);
                    }
                    else
                    {
                        ch = ret - 0x10000;
                        lead = (ch >> 10) + 0xd800;
                        trail = (ch & 0x3ff) + 0xdc00;
                        builder.Append((char)lead);
                        builder.Append((char)trail);
                    }
                }
            }
            if (bytesNeeded != 0)
            {
                if (replace)
                {
                    builder.Append((char)0xfffd);
                }
                else
                {
                    return -1;
                }
            }
            return 0;
        }

        /// <summary>Reads a string in UTF-8 encoding from a data stream in
        /// full and returns that string. Replaces invalid encoding with the
        /// replacement character (U+FFFD).</summary>
        /// <param name='stream'>A readable data stream.</param>
        /// <returns>The string read.</returns>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        public static string ReadUtf8ToString(Stream stream)
        {
            return ReadUtf8ToString(stream, -1, true);
        }

        /// <summary>Reads a string in UTF-8 encoding from a data stream and
        /// returns that string.</summary>
        /// <param name='stream'>A readable data stream.</param>
        /// <param name='bytesCount'>The length, in bytes, of the string. If
        /// this is less than 0, this function will read until the end of the
        /// stream.</param>
        /// <param name='replace'>If true, replaces invalid encoding with the
        /// replacement character (U+FFFD). If false, throws an error if an
        /// unpaired surrogate code point is seen.</param>
        /// <returns>The string read.</returns>
        /// <exception cref='System.IO.IOException'>An I/O error occurred; or,
        /// the string is not valid UTF-8 and <paramref name='replace'/> is
        /// false.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        public static string ReadUtf8ToString(
          Stream stream,
          int bytesCount,
          bool replace)
        {
            var builder = new StringBuilder();
            if (DataUtilities.ReadUtf8(stream, bytesCount, builder, replace) == -1)
            {
                throw new IOException(
                  "Unpaired surrogate code point found.",
                  new ArgumentException("Unpaired surrogate code point found."));
            }
            return builder.ToString();
        }

        /// <summary>Reads a string in UTF-8 encoding from a data
        /// stream.</summary>
        /// <param name='stream'>A readable data stream.</param>
        /// <param name='bytesCount'>The length, in bytes, of the string. If
        /// this is less than 0, this function will read until the end of the
        /// stream.</param>
        /// <param name='builder'>A string builder object where the resulting
        /// string will be stored.</param>
        /// <param name='replace'>If true, replaces invalid encoding with the
        /// replacement character (U+FFFD). If false, stops processing when an
        /// unpaired surrogate code point is seen.</param>
        /// <returns>0 if the entire string was read without errors, -1 if the
        /// string is not valid UTF-8 and <paramref name='replace'/> is false,
        /// or -2 if the end of the stream was reached before the last
        /// character was read completely (which is only the case if <paramref
        /// name='bytesCount'/> is 0 or greater).</returns>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null or <paramref name='builder'/> is
        /// null.</exception>
        public static int ReadUtf8(
          Stream stream,
          int bytesCount,
          StringBuilder builder,
          bool replace)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            int b;
            var cp = 0;
            var bytesSeen = 0;
            var bytesNeeded = 0;
            var lower = 0x80;
            var upper = 0xbf;
            var pointer = 0;
            while (pointer < bytesCount || bytesCount < 0)
            {
                b = stream.ReadByte();
                if (b < 0)
                {
                    if (bytesNeeded != 0)
                    {
                        bytesNeeded = 0;
                        if (replace)
                        {
                            builder.Append((char)0xfffd);
                            if (bytesCount >= 0)
                            {
                                return -2;
                            }
                            break; // end of stream
                        }
                        return -1;
                    }
                    if (bytesCount >= 0)
                    {
                        return -2;
                    }
                    break; // end of stream
                }
                if (bytesCount > 0)
                {
                    ++pointer;
                }
                if (bytesNeeded == 0)
                {
                    if ((b & 0x7f) == b)
                    {
                        builder.Append((char)b);
                    }
                    else if (b >= 0xc2 && b <= 0xdf)
                    {
                        bytesNeeded = 1;
                        cp = (b - 0xc0) << 6;
                    }
                    else if (b >= 0xe0 && b <= 0xef)
                    {
                        lower = (b == 0xe0) ? 0xa0 : 0x80;
                        upper = (b == 0xed) ? 0x9f : 0xbf;
                        bytesNeeded = 2;
                        cp = (b - 0xe0) << 12;
                    }
                    else if (b >= 0xf0 && b <= 0xf4)
                    {
                        lower = (b == 0xf0) ? 0x90 : 0x80;
                        upper = (b == 0xf4) ? 0x8f : 0xbf;
                        bytesNeeded = 3;
                        cp = (b - 0xf0) << 18;
                    }
                    else
                    {
                        if (replace)
                        {
                            builder.Append((char)0xfffd);
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    continue;
                }
                if (b < lower || b > upper)
                {
                    cp = bytesNeeded = bytesSeen = 0;
                    lower = 0x80;
                    upper = 0xbf;
                    if (replace)
                    {
                        builder.Append((char)0xfffd);
                        // "Read" the last byte again
                        if (b < 0x80)
                        {
                            builder.Append((char)b);
                        }
                        else if (b >= 0xc2 && b <= 0xdf)
                        {
                            bytesNeeded = 1;
                            cp = (b - 0xc0) << 6;
                        }
                        else if (b >= 0xe0 && b <= 0xef)
                        {
                            lower = (b == 0xe0) ? 0xa0 : 0x80;
                            upper = (b == 0xed) ? 0x9f : 0xbf;
                            bytesNeeded = 2;
                            cp = (b - 0xe0) << 12;
                        }
                        else if (b >= 0xf0 && b <= 0xf4)
                        {
                            lower = (b == 0xf0) ? 0x90 : 0x80;
                            upper = (b == 0xf4) ? 0x8f : 0xbf;
                            bytesNeeded = 3;
                            cp = (b - 0xf0) << 18;
                        }
                        else
                        {
                            builder.Append((char)0xfffd);
                        }
                        continue;
                    }
                    return -1;
                }
                else
                {
                    lower = 0x80;
                    upper = 0xbf;
                    ++bytesSeen;
                    cp += (b - 0x80) << (6 * (bytesNeeded - bytesSeen));
                    if (bytesSeen != bytesNeeded)
                    {
                        continue;
                    }
                    int ret, ch, lead, trail;
                    ret = cp;
                    cp = 0;
                    bytesSeen = 0;
                    bytesNeeded = 0;
                    if (ret <= 0xffff)
                    {
                        builder.Append((char)ret);
                    }
                    else
                    {
                        ch = ret - 0x10000;
                        lead = (ch >> 10) + 0xd800;
                        trail = (ch & 0x3ff) + 0xdc00;
                        builder.Append((char)lead);
                        builder.Append((char)trail);
                    }
                }
            }
            if (bytesNeeded != 0)
            {
                if (replace)
                {
                    builder.Append((char)0xfffd);
                }
                else
                {
                    return -1;
                }
            }
            return 0;
        }
    }
    #endregion

#if DEBUG
    internal static class DebugUtility
    {
        private static readonly object WriterLock = new Object();
        private static Action<string> writer = null;

        [System.Diagnostics.Conditional("DEBUG")]
        public static void SetWriter(Action<string> wr)
        {
            lock (WriterLock)
            {
                writer = wr;
            }
        }

        private static MethodInfo GetTypeMethod(
          Type t,
          string name,
          Type[] parameters)
        {
#if NET40 || NET20
        return t.GetMethod(name, parameters);
#else
            {
                return t?.GetRuntimeMethod(name, parameters);
            }
#endif
        }

        public static void Log(string str)
        {
            Type type = Type.GetType("System.Console");
            if (type == null)
            {
                Action<string> wr = null;
                lock (WriterLock)
                {
                    wr = writer;
                }
                if (wr != null)
                {
#if !NET20 && !NET40
                    System.Diagnostics.Debug.WriteLine(str);
#endif
                    wr(str);
                    return;
                }
                else
                {
#if !NET20 && !NET40
                    System.Diagnostics.Debug.WriteLine(str);
                    return;
#else
{
 throw new NotSupportedException("System.Console not found");
}
#endif
                }
            }
            var types = new[] { typeof(string) };
            var typeMethod = GetTypeMethod(type, "WriteLine", types);
            if (typeMethod != null)
            {
                typeMethod.Invoke(
                  type,
                  new object[] { str });
            }
            else
            {
                throw new NotSupportedException("System.Console.WriteLine not found");
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Log(string format, params object[] args)
        {
            Log(String.Format(
              System.Globalization.CultureInfo.CurrentCulture,
              format,
              args));
        }
    }
#endif

    #region ExtendedDecimal
    /// <include file='../docs.xml'
    /// path='docs/doc[@name="T:PeterO.ExtendedDecimal"]/*'/>
    [Obsolete("Use EDecimal from PeterO.Numbers/com.upokecenter.numbers and the output of this class's ToString method.")]
    public sealed class ExtendedDecimal : IComparable<ExtendedDecimal>, IEquatable<ExtendedDecimal>
    {
        /// <include file='../docs.xml'
        /// path='docs/doc[@name="P:PeterO.ExtendedDecimal.Exponent"]/*'/>
        public BigInteger Exponent
        {
            get
            {
                return new BigInteger(this.Ed.Exponent);
            }
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="P:PeterO.ExtendedDecimal.UnsignedMantissa"]/*'/>
        public BigInteger UnsignedMantissa
        {
            get
            {
                return new BigInteger(this.Ed.UnsignedMantissa);
            }
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="P:PeterO.ExtendedDecimal.Mantissa"]/*'/>
        public BigInteger Mantissa
        {
            get
            {
                return new BigInteger(this.Ed.Mantissa);
            }
        }

        internal static ExtendedDecimal ToLegacy(EDecimal ei)
        {
            return new ExtendedDecimal(ei);
        }

        internal static EDecimal FromLegacy(ExtendedDecimal bei)
        {
            return bei.Ed;
        }

        #region Equals and GetHashCode implementation

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedDecimal.Equals(PeterO.ExtendedDecimal)"]/*'/>
        [Obsolete("Use EDecimal from PeterO.Numbers/com.upokecenter.numbers.")]
        public bool Equals(ExtendedDecimal other)
        {
            return (other == null) ? false : this.Ed.Equals(other.Ed);
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedDecimal.Equals(System.Object)"]/*'/>
        public override bool Equals(object obj)
        {
            return (!(obj is ExtendedDecimal bi)) ? false : this.Ed.Equals(bi.Ed);
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedDecimal.GetHashCode"]/*'/>
        public override int GetHashCode()
        {
            return this.Ed.GetHashCode();
        }
        #endregion
        private readonly EDecimal ed;

        internal ExtendedDecimal(EDecimal ed)
        {
            this.ed = ed ?? throw new ArgumentNullException(nameof(ed));
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedDecimal.Create(PeterO.BigInteger,PeterO.BigInteger)"]/*'/>
        public static ExtendedDecimal Create(
          BigInteger mantissa,
          BigInteger exponent)
        {
            if (mantissa == null)
            {
                throw new ArgumentNullException(nameof(mantissa));
            }
            if (exponent == null)
            {
                throw new ArgumentNullException(nameof(exponent));
            }
            return new ExtendedDecimal(EDecimal.Create(mantissa.Ei, exponent.Ei));
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedDecimal.FromString(System.String)"]/*'/>
        public static ExtendedDecimal FromString(string str)
        {
            return new ExtendedDecimal(EDecimal.FromString(str));
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedDecimal.ToSingle"]/*'/>
        public float ToSingle()
        {
            return this.Ed.ToSingle();
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedDecimal.ToDouble"]/*'/>
        public double ToDouble()
        {
            return this.Ed.ToDouble();
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedDecimal.ToString"]/*'/>
        public override string ToString()
        {
            return this.Ed.ToString();
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedDecimal.One"]/*'/>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
                "CA2104", Justification = "ExtendedDecimal is immutable")]
#endif
        [Obsolete("Use EDecimal from PeterO.Numbers/com.upokecenter.numbers.")]
        public static readonly ExtendedDecimal One =
          ExtendedDecimal.Create(BigInteger.One, BigInteger.Zero);

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedDecimal.Zero"]/*'/>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
                "CA2104", Justification = "ExtendedDecimal is immutable")]
#endif
        [Obsolete("Use EDecimal from PeterO.Numbers/com.upokecenter.numbers.")]
        public static readonly ExtendedDecimal Zero =
          ExtendedDecimal.Create(BigInteger.Zero, BigInteger.Zero);

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedDecimal.NegativeZero"]/*'/>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
                "CA2104", Justification = "ExtendedDecimal is immutable")]
#endif
        public static readonly ExtendedDecimal NegativeZero =
          new ExtendedDecimal(EDecimal.NegativeZero);

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedDecimal.Ten"]/*'/>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
                "CA2104", Justification = "ExtendedDecimal is immutable")]
#endif
        public static readonly ExtendedDecimal Ten =
          new ExtendedDecimal(EDecimal.Ten);

        //----------------------------------------------------------------

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedDecimal.NaN"]/*'/>
        public static readonly ExtendedDecimal NaN =
          new ExtendedDecimal(EDecimal.NaN);

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedDecimal.SignalingNaN"]/*'/>
        public static readonly ExtendedDecimal SignalingNaN =
          new ExtendedDecimal(EDecimal.SignalingNaN);

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedDecimal.PositiveInfinity"]/*'/>
        public static readonly ExtendedDecimal PositiveInfinity =
          new ExtendedDecimal(EDecimal.PositiveInfinity);

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedDecimal.NegativeInfinity"]/*'/>
        public static readonly ExtendedDecimal NegativeInfinity =
          new ExtendedDecimal(EDecimal.NegativeInfinity);

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedDecimal.IsNaN"]/*'/>
        public bool IsNaN()
        {
            return this.Ed.IsNaN();
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedDecimal.IsInfinity"]/*'/>
        public bool IsInfinity()
        {
            return this.Ed.IsInfinity();
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="P:PeterO.ExtendedDecimal.IsNegative"]/*'/>
        [Obsolete("Use EDecimal from PeterO.Numbers/com.upokecenter.numbers.")]
        public bool IsNegative
        {
            get
            {
                return this.Ed.IsNegative;
            }
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedDecimal.IsQuietNaN"]/*'/>
        [Obsolete("Use EDecimal from PeterO.Numbers/com.upokecenter.numbers.")]
        public bool IsQuietNaN()
        {
            return this.Ed.IsQuietNaN();
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedDecimal.CompareTo(PeterO.ExtendedDecimal)"]/*'/>
        public int CompareTo(ExtendedDecimal other)
        {
            return this.Ed.CompareTo(other?.Ed);
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="P:PeterO.ExtendedDecimal.Sign"]/*'/>
        [Obsolete("Use EDecimal from PeterO.Numbers/com.upokecenter.numbers.")]
        public int Sign
        {
            get
            {
                return this.Ed.Sign;
            }
        }

        internal EDecimal Ed
        {
            get
            {
                return this.ed;
            }
        }
    }
    #endregion

    #region ExtendedFloat
    /// <include file='../docs.xml'
    /// path='docs/doc[@name="T:PeterO.ExtendedFloat"]/*'/>
    [Obsolete(
      "Use EFloat from PeterO.Numbers/com.upokecenter.numbers and the output of this class's ToString method.")]
    public sealed class ExtendedFloat : IComparable<ExtendedFloat>, IEquatable<ExtendedFloat>
    {
        private readonly EFloat ef;

        internal ExtendedFloat(EFloat ef)
        {
            this.ef = ef;
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="P:PeterO.ExtendedFloat.Exponent"]/*'/>
        public BigInteger Exponent
        {
            get
            {
                return new BigInteger(this.Ef.Exponent);
            }
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="P:PeterO.ExtendedFloat.UnsignedMantissa"]/*'/>
        public BigInteger UnsignedMantissa
        {
            get
            {
                return new BigInteger(this.Ef.UnsignedMantissa);
            }
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="P:PeterO.ExtendedFloat.Mantissa"]/*'/>
        public BigInteger Mantissa
        {
            get
            {
                return new BigInteger(this.Ef.Mantissa);
            }
        }

        internal static ExtendedFloat ToLegacy(EFloat ei)
        {
            return new ExtendedFloat(ei);
        }

        internal static EFloat FromLegacy(ExtendedFloat bei)
        {
            return bei.Ef;
        }

        #region Equals and GetHashCode implementation
        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedFloat.EqualsInternal(PeterO.ExtendedFloat)"]/*'/>
        [Obsolete("Use EFloat from PeterO.Numbers/com.upokecenter.numbers.")]
        public bool EqualsInternal(ExtendedFloat otherValue)
        {
            if (otherValue == null)
            {
                throw new ArgumentNullException(nameof(otherValue));
            }
            return this.Ef.EqualsInternal(otherValue.Ef);
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedFloat.Equals(PeterO.ExtendedFloat)"]/*'/>
        [Obsolete("Use EFloat from PeterO.Numbers/com.upokecenter.numbers.")]
        public bool Equals(ExtendedFloat other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }
            return this.Ef.Equals(other.Ef);
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedFloat.Equals(System.Object)"]/*'/>
        public override bool Equals(object obj)
        {
            return (!(obj is ExtendedFloat bi)) ? false : this.Ef.Equals(bi.Ef);
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedFloat.GetHashCode"]/*'/>
        public override int GetHashCode()
        {
            return this.Ef.GetHashCode();
        }
        #endregion

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedFloat.Create(System.Int32,System.Int32)"]/*'/>
        public static ExtendedFloat Create(int mantissaSmall, int exponentSmall)
        {
            return new ExtendedFloat(EFloat.Create(mantissaSmall, exponentSmall));
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedFloat.Create(PeterO.BigInteger,PeterO.BigInteger)"]/*'/>
        public static ExtendedFloat Create(
      BigInteger mantissa,
      BigInteger exponent)
        {
            if (mantissa == null)
            {
                throw new ArgumentNullException(nameof(mantissa));
            }
            if (exponent == null)
            {
                throw new ArgumentNullException(nameof(exponent));
            }
            return new ExtendedFloat(EFloat.Create(mantissa.Ei, exponent.Ei));
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedFloat.FromString(System.String,System.Int32,System.Int32,PeterO.PrecisionContext)"]/*'/>
        [Obsolete("Use EFloat from PeterO.Numbers/com.upokecenter.numbers.")]
        public static ExtendedFloat FromString(
      string str,
      int offset,
      int length,
      PrecisionContext ctx)
        {
            try
            {
                return new ExtendedFloat(
          EFloat.FromString(
          str,
          offset,
          length,
          ctx?.Ec));
            }
            catch (ETrapException ex)
            {
                throw TrapException.Create(ex);
            }
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedFloat.FromString(System.String)"]/*'/>
        public static ExtendedFloat FromString(string str)
        {
            return new ExtendedFloat(EFloat.FromString(str));
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedFloat.ToString"]/*'/>
        public override string ToString()
        {
            return this.Ef.ToString();
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedFloat.One"]/*'/>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "ExtendedFloat is immutable")]
#endif
        [Obsolete("Use EFloat from PeterO.Numbers/com.upokecenter.numbers.")]
        public static readonly ExtendedFloat One =
         new ExtendedFloat(EFloat.One);

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedFloat.Zero"]/*'/>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "ExtendedFloat is immutable")]
#endif
        [Obsolete("Use EFloat from PeterO.Numbers/com.upokecenter.numbers.")]
        public static readonly ExtendedFloat Zero =
         new ExtendedFloat(EFloat.Zero);

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedFloat.NegativeZero"]/*'/>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "ExtendedFloat is immutable")]
#endif
        [Obsolete("Use EFloat from PeterO.Numbers/com.upokecenter.numbers.")]
        public static readonly ExtendedFloat NegativeZero =
         new ExtendedFloat(EFloat.NegativeZero);

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedFloat.Ten"]/*'/>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "ExtendedFloat is immutable")]
#endif

        [Obsolete("Use EFloat from PeterO.Numbers/com.upokecenter.numbers.")]
        public static readonly ExtendedFloat Ten =
         new ExtendedFloat(EFloat.Ten);

        //----------------------------------------------------------------

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedFloat.NaN"]/*'/>
        public static readonly ExtendedFloat NaN =
         new ExtendedFloat(EFloat.NaN);

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedFloat.SignalingNaN"]/*'/>
        public static readonly ExtendedFloat SignalingNaN =
         new ExtendedFloat(EFloat.SignalingNaN);

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedFloat.PositiveInfinity"]/*'/>
        public static readonly ExtendedFloat PositiveInfinity =
         new ExtendedFloat(EFloat.PositiveInfinity);

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedFloat.NegativeInfinity"]/*'/>
        public static readonly ExtendedFloat NegativeInfinity =
         new ExtendedFloat(EFloat.NegativeInfinity);

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedFloat.IsNegativeInfinity"]/*'/>
        [Obsolete("Use EFloat from PeterO.Numbers/com.upokecenter.numbers.")]
        public bool IsNegativeInfinity()
        {
            return this.Ef.IsNegativeInfinity();
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedFloat.IsPositiveInfinity"]/*'/>
        [Obsolete("Use EFloat from PeterO.Numbers/com.upokecenter.numbers.")]
        public bool IsPositiveInfinity()
        {
            return this.Ef.IsPositiveInfinity();
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedFloat.IsNaN"]/*'/>
        public bool IsNaN()
        {
            return this.Ef.IsNaN();
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedFloat.IsInfinity"]/*'/>
        public bool IsInfinity()
        {
            return this.Ef.IsInfinity();
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="P:PeterO.ExtendedFloat.IsNegative"]/*'/>
        [Obsolete("Use EFloat from PeterO.Numbers/com.upokecenter.numbers.")]
        public bool IsNegative
        {
            get
            {
                return this.Ef.IsNegative;
            }
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedFloat.IsQuietNaN"]/*'/>
        [Obsolete("Use EFloat from PeterO.Numbers/com.upokecenter.numbers.")]
        public bool IsQuietNaN()
        {
            return this.Ef.IsQuietNaN();
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedFloat.IsSignalingNaN"]/*'/>
        [Obsolete("Use EFloat from PeterO.Numbers/com.upokecenter.numbers.")]
        public bool IsSignalingNaN()
        {
            return this.Ef.IsSignalingNaN();
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedFloat.CompareTo(PeterO.ExtendedFloat)"]/*'/>
        public int CompareTo(ExtendedFloat other)
        {
            return this.Ef.CompareTo(other?.Ef);
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="P:PeterO.ExtendedFloat.Sign"]/*'/>
        [Obsolete("Use EFloat from PeterO.Numbers/com.upokecenter.numbers.")]
        public int Sign
        {
            get
            {
                return this.Ef.Sign;
            }
        }

        internal EFloat Ef
        {
            get
            {
                return this.ef;
            }
        }
    }
    #endregion

    #region ExtendedRational
    /// <include file='../docs.xml'
    /// path='docs/doc[@name="T:PeterO.ExtendedRational"]/*'/>
    [Obsolete(
      "Use ERational from PeterO.Numbers/com.upokecenter.numbers and the output of this class's ToString method.")]
    public sealed class ExtendedRational : IComparable<ExtendedRational>,
        IEquatable<ExtendedRational>
    {
        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedRational.NaN"]/*'/>
        [Obsolete("Use ERational from PeterO.Numbers/com.upokecenter.numbers.")]
        public static readonly ExtendedRational NaN =
          new ExtendedRational(ERational.NaN);

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedRational.NegativeInfinity"]/*'/>
        public static readonly ExtendedRational NegativeInfinity = new
          ExtendedRational(ERational.NegativeInfinity);

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedRational.NegativeZero"]/*'/>
        public static readonly ExtendedRational NegativeZero =
          new ExtendedRational(ERational.NegativeZero);

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedRational.One"]/*'/>
        public static readonly ExtendedRational One =
          FromBigIntegerInternal(BigInteger.One);

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedRational.PositiveInfinity"]/*'/>
        public static readonly ExtendedRational PositiveInfinity = new
          ExtendedRational(ERational.PositiveInfinity);

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedRational.SignalingNaN"]/*'/>
        [Obsolete("Use ERational from PeterO.Numbers/com.upokecenter.numbers.")]
        public static readonly ExtendedRational SignalingNaN = new
          ExtendedRational(ERational.SignalingNaN);

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedRational.Ten"]/*'/>
        public static readonly ExtendedRational Ten =
          FromBigIntegerInternal(BigInteger.valueOf(10));

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.ExtendedRational.Zero"]/*'/>
        public static readonly ExtendedRational Zero =
          FromBigIntegerInternal(BigInteger.Zero);

        private readonly ERational er;

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedRational.#ctor(PeterO.BigInteger,PeterO.BigInteger)"]/*'/>
        public ExtendedRational(BigInteger numerator, BigInteger denominator)
        {
            this.er = new ERational(numerator.Ei, denominator.Ei);
        }

        internal ExtendedRational(ERational er)
        {
            this.er = er ?? throw new ArgumentNullException(nameof(er));
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="P:PeterO.ExtendedRational.Denominator"]/*'/>
        public BigInteger Denominator
        {
            get
            {
                return new BigInteger(this.Er.Denominator);
            }
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="P:PeterO.ExtendedRational.IsFinite"]/*'/>
        [Obsolete("Use ERational from PeterO.Numbers/com.upokecenter.numbers.")]
        public bool IsFinite
        {
            get
            {
                return this.Er.IsFinite;
            }
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="P:PeterO.ExtendedRational.IsNegative"]/*'/>
        public bool IsNegative
        {
            get
            {
                return this.Er.IsNegative;
            }
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="P:PeterO.ExtendedRational.IsZero"]/*'/>
        [Obsolete("Use ERational from PeterO.Numbers/com.upokecenter.numbers.")]
        public bool IsZero
        {
            get
            {
                return this.Er.IsZero;
            }
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="P:PeterO.ExtendedRational.Numerator"]/*'/>
        public BigInteger Numerator
        {
            get
            {
                return new BigInteger(this.Er.Numerator);
            }
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="P:PeterO.ExtendedRational.Sign"]/*'/>
        [Obsolete("Use ERational from PeterO.Numbers/com.upokecenter.numbers.")]
        public int Sign
        {
            get
            {
                return this.Er.Sign;
            }
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="P:PeterO.ExtendedRational.UnsignedNumerator"]/*'/>
        public BigInteger UnsignedNumerator
        {
            get
            {
                return new BigInteger(this.Er.UnsignedNumerator);
            }
        }

        internal ERational Er
        {
            get
            {
                return this.er;
            }
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedRational.Create(System.Int32,System.Int32)"]/*'/>
        public static ExtendedRational Create(
      int numeratorSmall,
      int denominatorSmall)
        {
            return new ExtendedRational(
        ERational.Create(
        numeratorSmall,
        denominatorSmall));
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedRational.Create(PeterO.BigInteger,PeterO.BigInteger)"]/*'/>
        public static ExtendedRational Create(
      BigInteger numerator,
      BigInteger denominator)
        {
            if (numerator == null)
            {
                throw new ArgumentNullException(nameof(numerator));
            }
            if (denominator == null)
            {
                throw new ArgumentNullException(nameof(denominator));
            }
            return new ExtendedRational(
        ERational.Create(
        numerator.Ei,
        denominator.Ei));
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedRational.ToString"]/*'/>
        public override string ToString()
        {
            return this.Er.ToString();
        }

        internal static ERational FromLegacy(ExtendedRational bei)
        {
            return bei.Er;
        }

        internal static ExtendedRational ToLegacy(ERational ei)
        {
            return new ExtendedRational(ei);
        }

        private static ExtendedRational FromBigIntegerInternal(BigInteger bigint)
        {
            return new ExtendedRational(ERational.FromEInteger(bigint.Ei));
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedRational.CompareTo(PeterO.ExtendedRational)"]/*'/>
        public int CompareTo(ExtendedRational other)
        {
            return this.Er.CompareTo(other?.Er);
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedRational.Equals(PeterO.ExtendedRational)"]/*'/>
        public bool Equals(ExtendedRational other)
        {
            return this.Er.Equals(other?.Er);
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedRational.Equals(System.Object)"]/*'/>
        public override bool Equals(object obj)
        {
            ExtendedRational other = obj as ExtendedRational;
            return this.Er.Equals(other?.Er);
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.ExtendedRational.GetHashCode"]/*'/>
        public override int GetHashCode()
        {
            return this.Er.GetHashCode();
        }
    }
    #endregion

    #region PrecisionContext
    /// <include file='../docs.xml'
    /// path='docs/doc[@name="T:PeterO.PrecisionContext"]/*'/>
    [Obsolete("Use EContext from PeterO.Numbers/com.upokecenter.numbers.")]
    public class PrecisionContext
    {
        private readonly EContext ec;

        internal EContext Ec
        {
            get
            {
                return this.ec;
            }
        }

        internal PrecisionContext(EContext ec)
        {
            this.ec = ec;
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.PrecisionContext.#ctor(System.Int32,PeterO.Rounding,System.Int32,System.Int32,System.Boolean)"]/*'/>
        public PrecisionContext(
     int precision,
     Rounding rounding,
     int exponentMinSmall,
     int exponentMaxSmall,
     bool clampNormalExponents)
        {
            throw new NotSupportedException("This class is now obsolete.");
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.PrecisionContext.ToString"]/*'/>
        public override string ToString()
        {
            return String.Empty;
        }
    }
    #endregion

    #region Rounding
    /// <include file='../docs.xml'
    /// path='docs/doc[@name="T:PeterO.Rounding"]/*'/>
    [Obsolete("Use ERounding from PeterO.Numbers/com.upokecenter.numbers.")]
    public enum Rounding
    {
        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.Rounding.Up"]/*'/>
        Up,

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.Rounding.Down"]/*'/>
        Down,

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.Rounding.Ceiling"]/*'/>
        Ceiling,

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.Rounding.Floor"]/*'/>
        Floor,

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.Rounding.HalfUp"]/*'/>
        HalfUp,

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.Rounding.HalfDown"]/*'/>
        HalfDown,

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.Rounding.HalfEven"]/*'/>
        HalfEven,

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.Rounding.Unnecessary"]/*'/>
        Unnecessary,

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.Rounding.ZeroFiveUp"]/*'/>
        ZeroFiveUp,

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.Rounding.Odd"]/*'/>
        Odd,

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="F:PeterO.Rounding.OddOrZeroFiveUp"]/*'/>
        OddOrZeroFiveUp
    }
    #endregion

    #region TrapException
    /// <include file='../docs.xml'
    /// path='docs/doc[@name="T:PeterO.TrapException"]/*'/>
    [Obsolete(
    "Use ETrapException from PeterO.Numbers/com.upokecenter.numbers.")]
    public class TrapException : ArithmeticException
    {
        private ETrapException ete;

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="P:PeterO.TrapException.Context"]/*'/>
        public PrecisionContext Context
        {
            get
            {
                return new PrecisionContext(this.ete.Context);
            }
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="P:PeterO.TrapException.Result"]/*'/>
        public Object Result
        {
            get
            {
                return this.ete.Result;
            }
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="P:PeterO.TrapException.Error"]/*'/>
        public int Error
        {
            get
            {
                return this.ete.Error;
            }
        }

        private TrapException() : base()
        {
        }

        internal static TrapException Create(ETrapException ete)
        {
            TrapException ex = new TrapException
            {
                ete = ete
            };
            return ex;
        }

        /// <include file='../docs.xml'
        /// path='docs/doc[@name="M:PeterO.TrapException.#ctor(System.Int32,PeterO.PrecisionContext,System.Object)"]/*'/>
        public TrapException(int flag, PrecisionContext ctx, Object result) :
          base(String.Empty)
        {
            Object wrappedResult = result;
            if (result is EDecimal ed)
            {
                wrappedResult = new ExtendedDecimal(ed);
            }
            if (result is ERational er)
            {
                wrappedResult = new ExtendedRational(er);
            }
            if (result is EFloat ef)
            {
                wrappedResult = new ExtendedFloat(ef);
            }
            this.ete = new ETrapException(
        flag,
        ctx?.Ec,
        wrappedResult);
        }
    }
    #endregion

    #region CharacterInputWithCount
    internal class CharacterInputWithCount : ICharacterInput
    {
        private readonly ICharacterInput ci;
        private int offset;

        public CharacterInputWithCount(ICharacterInput ci)
        {
            this.ci = ci;
        }

        public int GetOffset()
        {
            return this.offset;
        }

        public void RaiseError(string str)
        {
            throw new CBORException(this.NewErrorString(str));
        }

        public int Read(int[] chars, int index, int length)
        {
            if (chars == null)
            {
                throw new ArgumentNullException(nameof(chars));
            }
            if (index < 0)
            {
                throw new ArgumentException("index (" + index +
                  ") is less than 0");
            }
            if (index > chars.Length)
            {
                throw new ArgumentException("index (" + index +
                  ") is more than " + chars.Length);
            }
            if (length < 0)
            {
                throw new ArgumentException("length (" + length +
                  ") is less than 0");
            }
            if (length > chars.Length)
            {
                throw new ArgumentException("length (" + length +
                  ") is more than " + chars.Length);
            }
            if (chars.Length - index < length)
            {
                throw new ArgumentException("chars's length minus " + index + " (" +
                  (chars.Length - index) + ") is less than " + length);
            }
            int ret = this.ci.Read(chars, index, length);
            if (ret > 0)
            {
                this.offset += ret;
            }
            return ret;
        }

        public int ReadChar()
        {
            int c = -1;
            try
            {
                c = this.ci.ReadChar();
            }
            catch (InvalidOperationException ex)
            {
                if (ex.InnerException == null)
                {
                    throw new CBORException(
            this.NewErrorString(ex.Message),
            ex);
                }
                else
                {
                    throw new CBORException(
            this.NewErrorString(ex.Message),
            ex.InnerException);
                }
            }
            if (c >= 0)
            {
                ++this.offset;
            }
            return c;
        }

        private string NewErrorString(string str)
        {
            return str + " (offset " + this.GetOffset() + ")";
        }
    }

    #endregion

    #region CharacterReader
    // <include file='../../docs.xml'
    // path='docs/doc[@name="T:CharacterReader"]/*'/>
    internal sealed class CharacterReader : ICharacterInput
    {
        private readonly int mode;
        private readonly bool errorThrow;
        private readonly bool dontSkipUtf8Bom;
        private readonly string str;
        private readonly int strLength;
        private readonly IByteReader stream;

        private int offset;
        private ICharacterInput reader;

        // <summary>Initializes a new instance of the
        // <see cref='PeterO.Cbor.CharacterReader'/> class.</summary>
        // <param name='str'>The parameter <paramref name='str'/> is a text
        // string.</param>
        public CharacterReader(string str) : this(str, false, false)
        {
        }

        // <summary>Initializes a new instance of the
        // <see cref='PeterO.Cbor.CharacterReader'/> class.</summary>
        // <param name='str'>The parameter <paramref name='str'/> is a text
        // string.</param>
        // <param name='skipByteOrderMark'>If true and the first character in
        // the string is U+FEFF, skip that character.</param>
        // <exception cref='ArgumentNullException'>The parameter <paramref
        // name='str'/> is null.</exception>
        public CharacterReader(string str, bool skipByteOrderMark)
          : this(str, skipByteOrderMark, false)
        {
        }

        // <summary>Initializes a new instance of the
        // <see cref='PeterO.Cbor.CharacterReader'/> class.</summary>
        // <param name='str'>The parameter <paramref name='str'/> is a text
        // string.</param>
        // <param name='skipByteOrderMark'>If true and the first character in
        // the string is U+FEFF, skip that character.</param>
        // <param name='errorThrow'>When encountering invalid encoding, throw
        // an exception if this parameter is true, or replace it with U+FFFD
        // (replacement character) if this parameter is false.</param>
        // <exception cref='ArgumentNullException'>The parameter <paramref
        // name='str'/> is null.</exception>
        public CharacterReader(
          string str,
          bool skipByteOrderMark,
          bool errorThrow)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            this.strLength = str.Length;
            this.offset = (skipByteOrderMark && this.strLength > 0 && str[0] ==
                0xfeff) ? 1 : 0;
            this.str = str;
            this.errorThrow = errorThrow;
            this.mode = -1;
            this.dontSkipUtf8Bom = false;
            this.stream = null;
        }

        // <summary>Initializes a new instance of the
        // <see cref='PeterO.Cbor.CharacterReader'/> class.</summary>
        // <param name='str'>The parameter <paramref name='str'/> is a text
        // string.</param>
        // <param name='offset'>An index, starting at 0, showing where the
        // desired portion of <paramref name='str'/> begins.</param>
        // <param name='length'>The length, in code units, of the desired
        // portion of <paramref name='str'/> (but not more than <paramref
        // name='str'/> 's length).</param>
        // <exception cref='ArgumentException'>Either &#x22;offset&#x22; or
        // &#x22;length&#x22; is less than 0 or greater than
        // &#x22;str&#x22;&#x27;s length, or &#x22;str&#x22;&#x27;s length
        // minus &#x22;offset&#x22; is less than
        // &#x22;length&#x22;.</exception>
        // <exception cref='ArgumentNullException'>The parameter <paramref
        // name='str'/> is null.</exception>
        public CharacterReader(string str, int offset, int length)
          : this(str, offset, length, false, false)
        {
        }

        // <summary>Initializes a new instance of the
        // <see cref='PeterO.Cbor.CharacterReader'/> class.</summary>
        // <param name='str'>The parameter <paramref name='str'/> is a text
        // string.</param>
        // <param name='offset'>An index, starting at 0, showing where the
        // desired portion of <paramref name='str'/> begins.</param>
        // <param name='length'>The length, in code units, of the desired
        // portion of <paramref name='str'/> (but not more than <paramref
        // name='str'/> 's length).</param>
        // <param name='skipByteOrderMark'>If true and the first character in
        // the string portion is U+FEFF, skip that character.</param>
        // <param name='errorThrow'>When encountering invalid encoding, throw
        // an exception if this parameter is true, or replace it with U+FFFD
        // (replacement character) if this parameter is false.</param>
        // <exception cref='ArgumentNullException'>The parameter <paramref
        // name='str'/> is null.</exception>
        // <exception cref='ArgumentException'>Either <paramref
        // name='offset'/> or <paramref name='length'/> is less than 0 or
        // greater than <paramref name='str'/> 's length, or <paramref
        // name='str'/> 's length minus <paramref name='offset'/> is less than
        // <paramref name='length'/>.</exception>
        public CharacterReader(
          string str,
          int offset,
          int length,
          bool skipByteOrderMark,
          bool errorThrow)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            if (offset < 0)
            {
                throw new ArgumentException("offset(" + offset +
                  ") is less than 0");
            }
            if (offset > str.Length)
            {
                throw new ArgumentException("offset(" + offset +
                  ") is more than " + str.Length);
            }
            if (length < 0)
            {
                throw new ArgumentException("length(" + length +
                  ") is less than 0");
            }
            if (length > str.Length)
            {
                throw new ArgumentException("length(" + length +
                  ") is more than " + str.Length);
            }
            if (str.Length - offset < length)
            {
                throw new ArgumentException("str's length minus " + offset + "(" +
                  (str.Length - offset) + ") is less than " + length);
            }
            this.strLength = length;
            this.offset = (skipByteOrderMark && length > 0 && str[offset] ==
                0xfeff) ? offset + 1 : 0;
            this.str = str;
            this.errorThrow = errorThrow;
            this.mode = -1;
            this.dontSkipUtf8Bom = false;
            this.stream = null;
        }

        // <summary>Initializes a new instance of the
        // <see cref='PeterO.Cbor.CharacterReader'/> class; will read the
        // stream as UTF-8, skip the byte-order mark (U+FEFF) if it appears
        // first in the stream, and replace invalid byte sequences with
        // replacement characters (U+FFFD).</summary>
        // <param name='stream'>A readable data stream.</param>
        // <exception cref='ArgumentNullException'>The parameter <paramref
        // name='stream'/> is null.</exception>
        public CharacterReader(Stream stream) : this(stream, 0, false)
        {
        }

        // <summary>Initializes a new instance of the
        // <see cref='PeterO.Cbor.CharacterReader'/> class; will skip the
        // byte-order mark (U+FEFF) if it appears first in the stream and a
        // UTF-8 stream is detected.</summary>
        // <param name='stream'>A readable data stream.</param>
        // <param name='mode'>The method to use when detecting encodings other
        // than UTF-8 in the byte stream. This usually involves checking
        // whether the stream begins with a byte-order mark (BOM, U+FEFF) or a
        // non-zero basic code point (U+0001 to U+007F) before reading the
        // rest of the stream. This value can be one of the following:
        // <list>
        // <item>0: UTF-8 only.</item>
        // <item>1: Detect UTF-16 using BOM or non-zero basic code point,
        // otherwise UTF-8.</item>
        // <item>2: Detect UTF-16/UTF-32 using BOM or non-zero basic code
        // point, otherwise UTF-8. (Tries to detect UTF-32 first.)</item>
        // <item>3: Detect UTF-16 using BOM, otherwise UTF-8.</item>
        // <item>4: Detect UTF-16/UTF-32 using BOM, otherwise UTF-8. (Tries to
        // detect UTF-32 first.)</item></list>.</param>
        // <param name='errorThrow'>When encountering invalid encoding, throw
        // an exception if this parameter is true, or replace it with U+FFFD
        // (replacement character) if this parameter is false.</param>
        public CharacterReader(Stream stream, int mode, bool errorThrow)
          : this(stream, mode, errorThrow, false)
        {
        }

        // <summary>Initializes a new instance of the
        // <see cref='PeterO.Cbor.CharacterReader'/> class; will skip the
        // byte-order mark (U+FEFF) if it appears first in the stream and
        // replace invalid byte sequences with replacement characters
        // (U+FFFD).</summary>
        // <param name='stream'>A readable byte stream.</param>
        // <param name='mode'>The method to use when detecting encodings other
        // than UTF-8 in the byte stream. This usually involves checking
        // whether the stream begins with a byte-order mark (BOM, U+FEFF) or a
        // non-zero basic code point (U+0001 to U+007F) before reading the
        // rest of the stream. This value can be one of the following:
        // <list>
        // <item>0: UTF-8 only.</item>
        // <item>1: Detect UTF-16 using BOM or non-zero basic code point,
        // otherwise UTF-8.</item>
        // <item>2: Detect UTF-16/UTF-32 using BOM or non-zero basic code
        // point, otherwise UTF-8. (Tries to detect UTF-32 first.)</item>
        // <item>3: Detect UTF-16 using BOM, otherwise UTF-8.</item>
        // <item>4: Detect UTF-16/UTF-32 using BOM, otherwise UTF-8. (Tries to
        // detect UTF-32 first.)</item></list>.</param>
        // <exception cref='ArgumentNullException'>The parameter <paramref
        // name='stream'/> is null.</exception>
        public CharacterReader(Stream stream, int mode)
          : this(stream, mode, false, false)
        {
        }

        // <summary>Initializes a new instance of the
        // <see cref='PeterO.Cbor.CharacterReader'/> class.</summary>
        // <param name='stream'>A readable byte stream.</param>
        // <param name='mode'>The method to use when detecting encodings other
        // than UTF-8 in the byte stream. This usually involves checking
        // whether the stream begins with a byte-order mark (BOM, U+FEFF) or a
        // non-zero basic code point (U+0001 to U+007F) before reading the
        // rest of the stream. This value can be one of the following:
        // <list>
        // <item>0: UTF-8 only.</item>
        // <item>1: Detect UTF-16 using BOM or non-zero basic code point,
        // otherwise UTF-8.</item>
        // <item>2: Detect UTF-16/UTF-32 using BOM or non-zero basic code
        // point, otherwise UTF-8. (Tries to detect UTF-32 first.)</item>
        // <item>3: Detect UTF-16 using BOM, otherwise UTF-8.</item>
        // <item>4: Detect UTF-16/UTF-32 using BOM, otherwise UTF-8. (Tries to
        // detect UTF-32 first.)</item></list>.</param>
        // <param name='errorThrow'>If true, will throw an exception if
        // invalid byte sequences (in the detected encoding) are found in the
        // byte stream. If false, replaces those byte sequences with
        // replacement characters (U+FFFD) as the stream is read.</param>
        // <param name='dontSkipUtf8Bom'>If the stream is detected as UTF-8
        // (including when "mode" is 0) and this parameter is <c>true</c>,
        // won't skip the BOM character if it occurs at the start of the
        // stream.</param>
        // <exception cref='ArgumentNullException'>The parameter <paramref
        // name='stream'/> is null.</exception>
        public CharacterReader(
          Stream stream,
          int mode,
          bool errorThrow,
          bool dontSkipUtf8Bom)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            this.stream = new WrappedStream(stream);
            this.mode = mode;
            this.errorThrow = errorThrow;
            this.dontSkipUtf8Bom = dontSkipUtf8Bom;
            this.str = String.Empty;
            this.strLength = -1;
        }

        private interface IByteReader
        {
            int ReadByte();
        }

        // <summary>Reads a series of code points from a Unicode stream or a
        // string.</summary>
        // <param name='chars'>An array where the code points that were read
        // will be stored.</param>
        // <param name='index'>An index starting at 0 showing where the
        // desired portion of <paramref name='chars'/> begins.</param>
        // <param name='length'>The number of elements in the desired portion
        // of <paramref name='chars'/> (but not more than <paramref
        // name='chars'/> 's length).</param>
        // <returns>The number of code points read from the stream. This can
        // be less than the <paramref name='length'/> parameter if the end of
        // the stream is reached.</returns>
        // <exception cref='ArgumentNullException'>The parameter <paramref
        // name='chars'/> is null.</exception>
        // <exception cref='ArgumentException'>Either <paramref name='index'/>
        // or <paramref name='length'/> is less than 0 or greater than
        // <paramref name='chars'/> 's length, or <paramref name='chars'/> 's
        // length minus <paramref name='index'/> is less than <paramref
        // name='length'/>.</exception>
        public int Read(int[] chars, int index, int length)
        {
            if (chars == null)
            {
                throw new ArgumentNullException(nameof(chars));
            }
            if (index < 0)
            {
                throw new ArgumentException("index(" + index +
                  ") is less than 0");
            }
            if (index > chars.Length)
            {
                throw new ArgumentException("index(" + index +
                  ") is more than " + chars.Length);
            }
            if (length < 0)
            {
                throw new ArgumentException("length(" + length +
                  ") is less than 0");
            }
            if (length > chars.Length)
            {
                throw new ArgumentException("length(" + length +
                  ") is more than " + chars.Length);
            }
            if (chars.Length - index < length)
            {
                throw new ArgumentException("chars's length minus " + index + "(" +
                  (chars.Length - index) + ") is less than " + length);
            }
            var count = 0;
            for (int i = 0; i < length; ++i)
            {
                int c = this.ReadChar();
                if (c < 0)
                {
                    return count;
                }
                chars[index + i] = c;
                ++count;
            }
            return count;
        }

        // <summary>Reads the next character from a Unicode stream or a
        // string.</summary>
        // <returns>The next character, or -1 if the end of the string or
        // stream was reached.</returns>
        public int ReadChar()
        {
            if (this.reader != null)
            {
                return this.reader.ReadChar();
            }
            if (this.stream != null)
            {
                return this.DetectUnicodeEncoding();
            }
            else
            {
                int c = (this.offset < this.strLength) ? this.str[this.offset] : -1;
                if ((c & 0xfc00) == 0xd800 && this.offset + 1 < this.strLength &&
                  this.str[this.offset + 1] >= 0xdc00 && this.str[this.offset + 1]
                  <= 0xdfff)
                {
                    // Get the Unicode code point for the surrogate pair
                    c = 0x10000 + ((c & 0x3ff) << 10) + (this.str[this.offset + 1] &
                        0x3ff);
                    ++this.offset;
                }
                else if ((c & 0xf800) == 0xd800)
                {
                    // unpaired surrogate
                    if (this.errorThrow)
                    {
                        throw new InvalidOperationException("Unpaired surrogate code" +
                          "\u0020point");
                    }
                    else
                    {
                        c = 0xfffd;
                    }
                }
                ++this.offset;
                return c;
            }
        }

        private int DetectUtf8Or16Or32(int c1)
        {
            int c2, c3, c4;
            if (c1 == 0xff || c1 == 0xfe)
            {
                // Start of a possible byte-order mark
                // FF FE 0 0 --> UTF-32LE
                // FF FE ... --> UTF-16LE
                // FE FF --> UTF-16BE
                c2 = this.stream.ReadByte();
                bool bigEndian = c1 == 0xfe;
                int otherbyte = bigEndian ? 0xff : 0xfe;
                if (c2 == otherbyte)
                {
                    c3 = this.stream.ReadByte();
                    c4 = this.stream.ReadByte();
                    if (!bigEndian && c3 == 0 && c4 == 0)
                    {
                        this.reader = new Utf32Reader(this.stream, false, this.errorThrow);
                        return this.reader.ReadChar();
                    }
                    else
                    {
                        var newReader = new Utf16Reader(
                          this.stream,
                          bigEndian,
                          this.errorThrow);
                        newReader.Unget(c3, c4);
                        this.reader = newReader;
                        return newReader.ReadChar();
                    }
                }
                // Assume UTF-8 here, so the 0xff or 0xfe is invalid
                if (this.errorThrow)
                {
                    throw new InvalidOperationException("Invalid Unicode stream");
                }
                else
                {
                    var utf8reader = new Utf8Reader(this.stream, this.errorThrow);
                    utf8reader.Unget(c2);
                    this.reader = utf8reader;
                    return 0xfffd;
                }
            }
            else if (c1 == 0 && this.mode == 4)
            {
                // Here, the relevant cases are:
                // 0 0 0 NZA --> UTF-32BE (if mode is 4)
                // 0 0 FE FF --> UTF-32BE
                // Anything else is treated as UTF-8
                c2 = this.stream.ReadByte();
                c3 = this.stream.ReadByte();
                c4 = this.stream.ReadByte();
                if (c2 == 0 &&
                  ((c3 == 0xfe && c4 == 0xff) ||
                    (c3 == 0 && c4 >= 0x01 && c4 <= 0x7f)))
                {
                    this.reader = new Utf32Reader(this.stream, true, this.errorThrow);
                    return c3 == 0 ? c4 : this.reader.ReadChar();
                }
                else
                {
                    var utf8reader = new Utf8Reader(this.stream, this.errorThrow);
                    utf8reader.UngetThree(c2, c3, c4);
                    this.reader = utf8reader;
                    return c1;
                }
            }
            else if (this.mode == 2)
            {
                if (c1 >= 0x01 && c1 <= 0x7f)
                {
                    // Nonzero ASCII character
                    c2 = this.stream.ReadByte();
                    if (c2 == 0)
                    {
                        // NZA 0, so UTF-16LE or UTF-32LE
                        c3 = this.stream.ReadByte();
                        c4 = this.stream.ReadByte();
                        if (c3 == 0 && c4 == 0)
                        {
                            this.reader = new Utf32Reader(
                              this.stream,
                              false,
                              this.errorThrow);
                            return c1;
                        }
                        else
                        {
                            var newReader = new Utf16Reader(
                              this.stream,
                              false,
                              this.errorThrow);
                            newReader.Unget(c3, c4);
                            this.reader = newReader;
                            return c1;
                        }
                    }
                    else
                    {
                        // NZA NZ, so UTF-8
                        var utf8reader = new Utf8Reader(this.stream, this.errorThrow);
                        utf8reader.Unget(c2);
                        this.reader = utf8reader;
                        return c1;
                    }
                }
                else if (c1 == 0)
                {
                    // Zero
                    c2 = this.stream.ReadByte();
                    if (c2 >= 0x01 && c2 <= 0x7f)
                    {
                        // 0 NZA, so UTF-16BE
                        var newReader = new Utf16Reader(
                          this.stream,
                          true,
                          this.errorThrow);
                        this.reader = newReader;
                        return c2;
                    }
                    else if (c2 == 0)
                    {
                        // 0 0, so maybe UTF-32BE
                        c3 = this.stream.ReadByte();
                        c4 = this.stream.ReadByte();
                        if (c3 == 0 && c4 >= 0x01 && c4 <= 0x7f)
                        {
                            // 0 0 0 NZA
                            this.reader = new Utf32Reader(
                              this.stream,
                              true,
                              this.errorThrow);
                            return c4;
                        }
                        else if (c3 == 0xfe && c4 == 0xff)
                        {
                            // 0 0 FE FF
                            this.reader = new Utf32Reader(
                              this.stream,
                              true,
                              this.errorThrow);
                            return this.reader.ReadChar();
                        }
                        else
                        {
                            // 0 0 ...
                            var newReader = new Utf8Reader(this.stream, this.errorThrow);
                            newReader.UngetThree(c2, c3, c4);
                            this.reader = newReader;
                            return c1;
                        }
                    }
                    else
                    {
                        // 0 NonAscii, so UTF-8
                        var utf8reader = new Utf8Reader(this.stream, this.errorThrow);
                        utf8reader.Unget(c2);
                        this.reader = utf8reader;
                        return c1;
                    }
                }
            }
            // Use default of UTF-8
            return -2;
        }

        private int DetectUtf8OrUtf16(int c1)
        {
            int mode = this.mode;
            int c2;
            if (c1 == 0xff || c1 == 0xfe)
            {
                c2 = this.stream.ReadByte();
                bool bigEndian = c1 == 0xfe;
                int otherbyte = bigEndian ? 0xff : 0xfe;
                if (c2 == otherbyte)
                {
                    var newReader = new Utf16Reader(
                      this.stream,
                      bigEndian,
                      this.errorThrow);
                    this.reader = newReader;
                    return newReader.ReadChar();
                }
                // Assume UTF-8 here, so the 0xff or 0xfe is invalid
                if (this.errorThrow)
                {
                    throw new InvalidOperationException("Invalid Unicode stream");
                }
                else
                {
                    var utf8reader = new Utf8Reader(this.stream, this.errorThrow);
                    utf8reader.Unget(c2);
                    this.reader = utf8reader;
                    return 0xfffd;
                }
            }
            else if (mode == 1)
            {
                if (c1 >= 0x01 && c1 <= 0x7f)
                {
                    // Nonzero ASCII character
                    c2 = this.stream.ReadByte();
                    if (c2 == 0)
                    {
                        // NZA 0, so UTF-16LE
                        var newReader = new Utf16Reader(
                          this.stream,
                          false,
                          this.errorThrow);
                        this.reader = newReader;
                    }
                    else
                    {
                        // NZA NZ
                        var utf8reader = new Utf8Reader(this.stream, this.errorThrow);
                        utf8reader.Unget(c2);
                        this.reader = utf8reader;
                    }
                    return c1;
                }
                else if (c1 == 0)
                {
                    // Zero
                    c2 = this.stream.ReadByte();
                    if (c2 >= 0x01 && c2 <= 0x7f)
                    {
                        // 0 NZA, so UTF-16BE
                        var newReader = new Utf16Reader(
                          this.stream,
                          true,
                          this.errorThrow);
                        this.reader = newReader;
                        return c2;
                    }
                    else
                    {
                        var utf8reader = new Utf8Reader(this.stream, this.errorThrow);
                        utf8reader.Unget(c2);
                        this.reader = utf8reader;
                        return c1;
                    }
                }
            }
            // Use default of UTF-8
            return -2;
        }

        // Detects a Unicode encoding
        private int DetectUnicodeEncoding()
        {
            int mode = this.mode;
            int c1 = this.stream.ReadByte();
            int c2;
            if (c1 < 0)
            {
                return -1;
            }
            Utf8Reader utf8reader;
            switch (mode)
            {
                case 0:
                    // UTF-8 only
                    utf8reader = new Utf8Reader(this.stream, this.errorThrow);
                    this.reader = utf8reader;
                    utf8reader.Unget(c1);
                    c1 = utf8reader.ReadChar();
                    if (c1 == 0xfeff && !this.dontSkipUtf8Bom)
                    {
                        // Skip BOM
                        c1 = utf8reader.ReadChar();
                    }
                    return c1;
                case 1:
                case 3:
                    c2 = this.DetectUtf8OrUtf16(c1);
                    if (c2 >= -1)
                    {
                        return c2;
                    }
                    break;
                case 2:
                case 4:
                    // UTF-8, UTF-16, or UTF-32
                    c2 = this.DetectUtf8Or16Or32(c1);
                    if (c2 >= -1)
                    {
                        return c2;
                    }
                    break;
            }
            // Default case: assume UTF-8
            utf8reader = new Utf8Reader(this.stream, this.errorThrow);
            this.reader = utf8reader;
            utf8reader.Unget(c1);
            c1 = utf8reader.ReadChar();
            if (!this.dontSkipUtf8Bom && c1 == 0xfeff)
            {
                // Skip BOM
                c1 = utf8reader.ReadChar();
            }
            return c1;
        }

        private sealed class SavedState
        {
            private int[] saved;
            private int savedLength;

            private void Ensure(int size)
            {
                this.saved = this.saved ?? (new int[this.savedLength + size]);
                if (this.savedLength + size < this.saved.Length)
                {
                    var newsaved = new int[this.savedLength + size + 4];
                    Array.Copy(this.saved, 0, newsaved, 0, this.savedLength);
                    this.saved = newsaved;
                }
            }

            public void AddOne(int a)
            {
                this.Ensure(1);
                this.saved[this.savedLength++] = a;
            }

            public void AddTwo(int a, int b)
            {
                this.Ensure(2);
                this.saved[this.savedLength + 1] = a;
                this.saved[this.savedLength] = b;
                this.savedLength += 2;
            }

            public void AddThree(int a, int b, int c)
            {
                this.Ensure(3);
                this.saved[this.savedLength + 2] = a;
                this.saved[this.savedLength + 1] = b;
                this.saved[this.savedLength] = c;
                this.savedLength += 3;
            }

            public int Read(IByteReader input)
            {
                if (this.savedLength > 0)
                {
                    int ret = this.saved[--this.savedLength];
                    return ret;
                }
                return input.ReadByte();
            }
        }

        private sealed class Utf16Reader : ICharacterInput
        {
            private readonly bool bigEndian;
            private readonly IByteReader stream;
            private readonly SavedState state;
            private readonly bool errorThrow;

            public Utf16Reader(IByteReader stream, bool bigEndian, bool errorThrow)
            {
                this.stream = stream;
                this.bigEndian = bigEndian;
                this.state = new SavedState();
                this.errorThrow = errorThrow;
            }

            public void Unget(int c1, int c2)
            {
                this.state.AddTwo(c1, c2);
            }

            public int ReadChar()
            {
                int c1 = this.state.Read(this.stream);
                if (c1 < 0)
                {
                    return -1;
                }
                int c2 = this.state.Read(this.stream);
                if (c2 < 0)
                {
                    this.state.AddOne(-1);
                    if (this.errorThrow)
                    {
                        throw new InvalidOperationException("Invalid UTF-16");
                    }
                    else
                    {
                        return 0xfffd;
                    }
                }
                c1 = this.bigEndian ? ((c1 << 8) | c2) : ((c2 << 8) | c1);
                int surr = c1 & 0xfc00;
                if (surr == 0xd800)
                {
                    surr = c1;
                    c1 = this.state.Read(this.stream);
                    c2 = this.state.Read(this.stream);
                    if (c1 < 0 || c2 < 0)
                    {
                        this.state.AddOne(-1);
                        if (this.errorThrow)
                        {
                            throw new InvalidOperationException("Invalid UTF-16");
                        }
                        else
                        {
                            return 0xfffd;
                        }
                    }
                    int unit2 = this.bigEndian ? ((c1 << 8) | c2) : ((c2 << 8) | c1);
                    if ((unit2 & 0xfc00) == 0xdc00)
                    {
                        return 0x10000 + ((surr & 0x3ff) << 10) + (unit2 & 0x3ff);
                    }
                    this.Unget(c1, c2);
                    if (this.errorThrow)
                    {
                        throw new InvalidOperationException("Invalid UTF-16");
                    }
                    else
                    {
                        return 0xfffd;
                    }
                }
                if (surr == 0xdc00)
                {
                    if (this.errorThrow)
                    {
                        throw new InvalidOperationException("Invalid UTF-16");
                    }
                    else
                    {
                        return 0xfffd;
                    }
                }
                return c1;
            }

            public int Read(int[] chars, int index, int length)
            {
                var count = 0;
                for (int i = 0; i < length; ++i)
                {
                    int c = this.ReadChar();
                    if (c < 0)
                    {
                        return count;
                    }
                    chars[index + i] = c;
                    ++count;
                }
                return count;
            }
        }

        private sealed class Utf32Reader : ICharacterInput
        {
            private readonly bool bigEndian;
            private readonly IByteReader stream;
            private readonly bool errorThrow;
            private readonly SavedState state;

            public Utf32Reader(IByteReader stream, bool bigEndian, bool errorThrow)
            {
                this.stream = stream;
                this.bigEndian = bigEndian;
                this.state = new SavedState();
                this.errorThrow = errorThrow;
            }

            public int ReadChar()
            {
                int c1 = this.state.Read(this.stream);
                if (c1 < 0)
                {
                    return -1;
                }
                int c2 = this.state.Read(this.stream);
                int c3 = this.state.Read(this.stream);
                int c4 = this.state.Read(this.stream);
                if (c2 < 0 || c3 < 0 || c4 < 0)
                {
                    this.state.AddOne(-1);
                    if (this.errorThrow)
                    {
                        throw new InvalidOperationException("Invalid UTF-32");
                    }
                    else
                    {
                        return 0xfffd;
                    }
                }
                c1 = this.bigEndian ? ((c1 << 24) | (c2 << 16) | (c3 << 8) | c4) :
                  ((c4 << 24) | (c3 << 16) | (c2 << 8) | c1);
                if (c1 < 0 || c1 >= 0x110000 || (c1 & 0xfff800) == 0xd800)
                {
                    if (this.errorThrow)
                    {
                        throw new InvalidOperationException("Invalid UTF-32");
                    }
                    else
                    {
                        return 0xfffd;
                    }
                }
                return c1;
            }

            public int Read(int[] chars, int index, int length)
            {
                var count = 0;
                for (int i = 0; i < length; ++i)
                {
                    int c = this.ReadChar();
                    if (c < 0)
                    {
                        return count;
                    }
                    chars[index + i] = c;
                    ++count;
                }
                return count;
            }
        }

        private sealed class Utf8Reader : ICharacterInput
        {
            private readonly IByteReader stream;
            private readonly SavedState state;
            private readonly bool errorThrow;
            private int lastChar;

            public Utf8Reader(IByteReader stream, bool errorThrow)
            {
                this.stream = stream;
                this.lastChar = -1;
                this.state = new SavedState();
                this.errorThrow = errorThrow;
            }

            public void Unget(int ch)
            {
                this.state.AddOne(ch);
            }

            public void UngetThree(int a, int b, int c)
            {
                this.state.AddThree(a, b, c);
            }

            public int ReadChar()
            {
                var cp = 0;
                var bytesSeen = 0;
                var bytesNeeded = 0;
                var lower = 0;
                var upper = 0;
                while (true)
                {
                    int b;
                    if (this.lastChar != -1)
                    {
                        b = this.lastChar;
                        this.lastChar = -1;
                    }
                    else
                    {
                        b = this.state.Read(this.stream);
                    }
                    if (b < 0)
                    {
                        if (bytesNeeded != 0)
                        {
                            bytesNeeded = 0;
                            if (this.errorThrow)
                            {
                                throw new InvalidOperationException("Invalid UTF-8");
                            }
                            else
                            {
                                return 0xfffd;
                            }
                        }
                        return -1;
                    }
                    if (bytesNeeded == 0)
                    {
                        if ((b & 0x7f) == b)
                        {
                            return b;
                        }
                        if (b >= 0xc2 && b <= 0xdf)
                        {
                            bytesNeeded = 1;
                            lower = 0x80;
                            upper = 0xbf;
                            cp = (b - 0xc0) << 6;
                        }
                        else if (b >= 0xe0 && b <= 0xef)
                        {
                            lower = (b == 0xe0) ? 0xa0 : 0x80;
                            upper = (b == 0xed) ? 0x9f : 0xbf;
                            bytesNeeded = 2;
                            cp = (b - 0xe0) << 12;
                        }
                        else if (b >= 0xf0 && b <= 0xf4)
                        {
                            lower = (b == 0xf0) ? 0x90 : 0x80;
                            upper = (b == 0xf4) ? 0x8f : 0xbf;
                            bytesNeeded = 3;
                            cp = (b - 0xf0) << 18;
                        }
                        else
                        {
                            if (this.errorThrow)
                            {
                                throw new InvalidOperationException("Invalid UTF-8");
                            }
                            else
                            {
                                return 0xfffd;
                            }
                        }
                        continue;
                    }
                    if (b < lower || b > upper)
                    {
                        cp = bytesNeeded = bytesSeen = 0;
                        this.state.AddOne(b);
                        if (this.errorThrow)
                        {
                            throw new InvalidOperationException("Invalid UTF-8");
                        }
                        else
                        {
                            return 0xfffd;
                        }
                    }
                    lower = 0x80;
                    upper = 0xbf;
                    ++bytesSeen;
                    cp += (b - 0x80) << (6 * (bytesNeeded - bytesSeen));
                    if (bytesSeen != bytesNeeded)
                    {
                        continue;
                    }
                    int ret = cp;
                    cp = 0;
                    bytesSeen = 0;
                    bytesNeeded = 0;
                    return ret;
                }
            }

            public int Read(int[] chars, int index, int length)
            {
                var count = 0;
                for (int i = 0; i < length; ++i)
                {
                    int c = this.ReadChar();
                    if (c < 0)
                    {
                        return count;
                    }
                    chars[index + i] = c;
                    ++count;
                }
                return count;
            }
        }

        private sealed class WrappedStream : IByteReader
        {
            private readonly Stream stream;

            public WrappedStream(Stream stream)
            {
                this.stream = stream;
            }

            public int ReadByte()
            {
                try
                {
                    return this.stream.ReadByte();
                }
                catch (IOException ex)
                {
                    throw new InvalidOperationException(ex.Message, ex);
                }
            }
        }
    }
    #endregion

    #region FastInteger2
    internal sealed class FastInteger2
    {
        private sealed class MutableNumber
        {
            private int[] data;
            private int wordCount;

            internal MutableNumber(int val)
            {
                if (val < 0)
                {
                    throw new ArgumentException("val (" + val + ") is less than " + "0 ");
                }
                this.data = new int[4];
                this.wordCount = (val == 0) ? 0 : 1;
                this.data[0] = val;
            }

            internal EInteger ToEInteger()
            {
                if (this.wordCount == 1 && (this.data[0] >> 31) == 0)
                {
                    return (EInteger)((int)this.data[0]);
                }
                byte[] bytes = new byte[(this.wordCount * 4) + 1];
                for (int i = 0; i < this.wordCount; ++i)
                {
                    bytes[i * 4] = (byte)(this.data[i] & 0xff);
                    bytes[(i * 4) + 1] = (byte)((this.data[i] >> 8) & 0xff);
                    bytes[(i * 4) + 2] = (byte)((this.data[i] >> 16) & 0xff);
                    bytes[(i * 4) + 3] = (byte)((this.data[i] >> 24) & 0xff);
                }
                bytes[bytes.Length - 1] = (byte)0;
                return EInteger.FromBytes(bytes, true);
            }

            internal int[] GetLastWordsInternal(int numWords32Bit)
            {
                int[] ret = new int[numWords32Bit];
                Array.Copy(this.data, ret, Math.Min(numWords32Bit, this.wordCount));
                return ret;
            }

            internal bool CanFitInInt32()
            {
                return this.wordCount == 0 || (this.wordCount == 1 && (this.data[0] >>
                31) == 0);
            }

            internal int ToInt32()
            {
                return this.wordCount == 0 ? 0 : this.data[0];
            }

            internal MutableNumber Multiply(int multiplicand)
            {
                if (multiplicand < 0)
                {
                    throw new ArgumentException("multiplicand (" + multiplicand +
                      ") is less than " + "0 ");
                }
                if (multiplicand != 0)
                {
                    int carry = 0;
                    if (this.wordCount == 0)
                    {
                        if (this.data.Length == 0)
                        {
                            this.data = new int[4];
                        }
                        this.data[0] = 0;
                        this.wordCount = 1;
                    }
                    int result0, result1, result2, result3;
                    if (multiplicand < 65536)
                    {
                        for (int i = 0; i < this.wordCount; ++i)
                        {
                            int x0 = this.data[i];
                            int x1 = x0;
                            int y0 = multiplicand;
                            x0 &= 65535;
                            x1 = (x1 >> 16) & 65535;
                            int temp = unchecked(x0 * y0);  // a * c
                            result1 = (temp >> 16) & 65535;
                            result0 = temp & 65535;
                            result2 = 0;
                            temp = unchecked(x1 * y0);  // b * c
                            result2 += (temp >> 16) & 65535;
                            result1 += temp & 65535;
                            result2 += (result1 >> 16) & 65535;
                            result1 &= 65535;
                            result3 = (result2 >> 16) & 65535;
                            result2 &= 65535;
                            // Add carry
                            x0 = unchecked((int)(result0 | (result1 << 16)));
                            x1 = unchecked((int)(result2 | (result3 << 16)));
                            int x2 = unchecked(x0 + carry);
                            if (((x2 >> 31) == (x0 >> 31)) ? ((x2 & Int32.MaxValue) < (x0 &
                            Int32.MaxValue)) : ((x2 >> 31) == 0))
                            {
                                // Carry in addition
                                x1 = unchecked(x1 + 1);
                            }
                            this.data[i] = x2;
                            carry = x1;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < this.wordCount; ++i)
                        {
                            int x0 = this.data[i];
                            int x1 = x0;
                            int y0 = multiplicand;
                            int y1 = y0;
                            x0 &= 65535;
                            y0 &= 65535;
                            x1 = (x1 >> 16) & 65535;
                            y1 = (y1 >> 16) & 65535;
                            int temp = unchecked(x0 * y0);  // a * c
                            result1 = (temp >> 16) & 65535;
                            result0 = temp & 65535;
                            temp = unchecked(x0 * y1);  // a * d
                            result2 = (temp >> 16) & 65535;
                            result1 += temp & 65535;
                            result2 += (result1 >> 16) & 65535;
                            result1 &= 65535;
                            temp = unchecked(x1 * y0);  // b * c
                            result2 += (temp >> 16) & 65535;
                            result1 += temp & 65535;
                            result2 += (result1 >> 16) & 65535;
                            result1 &= 65535;
                            result3 = (result2 >> 16) & 65535;
                            result2 &= 65535;
                            temp = unchecked(x1 * y1);  // b * d
                            result3 += (temp >> 16) & 65535;
                            result2 += temp & 65535;
                            result3 += (result2 >> 16) & 65535;
                            result2 &= 65535;
                            // Add carry
                            x0 = unchecked((int)(result0 | (result1 << 16)));
                            x1 = unchecked((int)(result2 | (result3 << 16)));
                            int x2 = unchecked(x0 + carry);
                            if (((x2 >> 31) == (x0 >> 31)) ? ((x2 & Int32.MaxValue) < (x0 &
                            Int32.MaxValue)) : ((x2 >> 31) == 0))
                            {
                                // Carry in addition
                                x1 = unchecked(x1 + 1);
                            }
                            this.data[i] = x2;
                            carry = x1;
                        }
                    }
                    if (carry != 0)
                    {
                        if (this.wordCount >= this.data.Length)
                        {
                            int[] newdata = new int[this.wordCount + 20];
                            Array.Copy(this.data, 0, newdata, 0, this.data.Length);
                            this.data = newdata;
                        }
                        this.data[this.wordCount] = carry;
                        ++this.wordCount;
                    }
                    // Calculate the correct data length
                    while (this.wordCount != 0 && this.data[this.wordCount - 1] == 0)
                    {
                        --this.wordCount;
                    }
                }
                else
                {
                    if (this.data.Length > 0)
                    {
                        this.data[0] = 0;
                    }
                    this.wordCount = 0;
                }
                return this;
            }

            internal int Sign
            {
                get
                {
                    return this.wordCount == 0 ? 0 : 1;
                }
            }

            internal MutableNumber SubtractInt(int other)
            {
                if (other < 0)
                {
                    throw new ArgumentException("other (" + other + ") is less than " +
                           "0 ");
                }
                if (other != 0)
                {
                    unchecked
                    {
                        // Ensure a length of at least 1
                        if (this.wordCount == 0)
                        {
                            if (this.data.Length == 0)
                            {
                                this.data = new int[4];
                            }
                            this.data[0] = 0;
                            this.wordCount = 1;
                        }
                        int borrow;
                        int u;
                        int a = this.data[0];
                        u = a - other;
                        borrow = ((((a >> 31) == (u >> 31)) ?
                                ((a & Int32.MaxValue) < (u & Int32.MaxValue)) :
                                ((a >> 31) == 0)) || (a == u && other != 0)) ? 1 : 0;
                        this.data[0] = (int)u;
                        if (borrow != 0)
                        {
                            for (int i = 1; i < this.wordCount; ++i)
                            {
                                u = this.data[i] - borrow;
                                borrow = (((this.data[i] >> 31) == (u >> 31)) ?
                                ((this.data[i] & Int32.MaxValue) < (u & Int32.MaxValue)) :
                                    ((this.data[i] >> 31) == 0)) ? 1 : 0;
                                this.data[i] = (int)u;
                            }
                        }
                        // Calculate the correct data length
                        while (this.wordCount != 0 && this.data[this.wordCount - 1] == 0)
                        {
                            --this.wordCount;
                        }
                    }
                }
                return this;
            }

            internal MutableNumber Subtract(MutableNumber other)
            {
                unchecked
                {
                    {
                        // Console.WriteLine("" + this.data.Length + " " +
                        // (other.data.Length));
                        int neededSize = (this.wordCount > other.wordCount) ?
                        this.wordCount : other.wordCount;
                        if (this.data.Length < neededSize)
                        {
                            int[] newdata = new int[neededSize + 20];
                            Array.Copy(this.data, 0, newdata, 0, this.data.Length);
                            this.data = newdata;
                        }
                        neededSize = (this.wordCount < other.wordCount) ? this.wordCount :
                        other.wordCount;
                        int u = 0;
                        int borrow = 0;
                        for (int i = 0; i < neededSize; ++i)
                        {
                            int a = this.data[i];
                            u = (a - other.data[i]) - borrow;
                            borrow = ((((a >> 31) == (u >> 31)) ? ((a & Int32.MaxValue) <
                            (u & Int32.MaxValue)) :
                                  ((a >> 31) == 0)) || (a == u && other.data[i] !=
                                  0)) ? 1 : 0;
                            this.data[i] = (int)u;
                        }
                        if (borrow != 0)
                        {
                            for (int i = neededSize; i < this.wordCount; ++i)
                            {
                                int a = this.data[i];
                                u = (a - other.data[i]) - borrow;
                                borrow = ((((a >> 31) == (u >> 31)) ? ((a & Int32.MaxValue) <
                                (u & Int32.MaxValue)) :
                                    ((a >> 31) == 0)) || (a == u && other.data[i] !=
                                    0)) ? 1 : 0;
                                this.data[i] = (int)u;
                            }
                        }
                        // Calculate the correct data length
                        while (this.wordCount != 0 && this.data[this.wordCount - 1] == 0)
                        {
                            --this.wordCount;
                        }
                        return this;
                    }
                }
            }

            internal MutableNumber Add(int augend)
            {
                if (augend < 0)
                {
                    throw new ArgumentException("augend (" + augend + ") is less than " +
                             "0 ");
                }
                unchecked
                {
                    if (augend != 0)
                    {
                        int carry = 0;
                        // Ensure a length of at least 1
                        if (this.wordCount == 0)
                        {
                            if (this.data.Length == 0)
                            {
                                this.data = new int[4];
                            }
                            this.data[0] = 0;
                            this.wordCount = 1;
                        }
                        for (int i = 0; i < this.wordCount; ++i)
                        {
                            int u;
                            int a = this.data[i];
                            u = (a + augend) + carry;
                            carry = ((((u >> 31) == (a >> 31)) ? ((u & Int32.MaxValue) < (a &
                            Int32.MaxValue)) :
                                    ((u >> 31) == 0)) || (u == a && augend != 0)) ? 1 : 0;
                            this.data[i] = u;
                            if (carry == 0)
                            {
                                return this;
                            }
                            augend = 0;
                        }
                        if (carry != 0)
                        {
                            if (this.wordCount >= this.data.Length)
                            {
                                int[] newdata = new int[this.wordCount + 20];
                                Array.Copy(this.data, 0, newdata, 0, this.data.Length);
                                this.data = newdata;
                            }
                            this.data[this.wordCount] = carry;
                            ++this.wordCount;
                        }
                    }
                    // Calculate the correct data length
                    while (this.wordCount != 0 && this.data[this.wordCount - 1] == 0)
                    {
                        --this.wordCount;
                    }
                    return this;
                }
            }
        }

        private int smallValue;  // if integerMode is 0
        private MutableNumber mnum;  // if integerMode is 1
        private EInteger largeValue;  // if integerMode is 2
        private int integerMode;

        internal FastInteger2(int value)
        {
            this.smallValue = value;
        }

        internal int AsInt32()
        {
            switch (this.integerMode)
            {
                case 0:
                    return this.smallValue;
                case 1:
                    return this.mnum.ToInt32();
                case 2:
                    return (int)this.largeValue;
                default: throw new InvalidOperationException();
            }
        }

        internal static EInteger WordsToEInteger(int[] words)
        {
            int wordCount = words.Length;
            if (wordCount == 1 && (words[0] >> 31) == 0)
            {
                return (EInteger)((int)words[0]);
            }
            byte[] bytes = new byte[(wordCount * 4) + 1];
            for (int i = 0; i < wordCount; ++i)
            {
                bytes[(i * 4) + 0] = (byte)(words[i] & 0xff);
                bytes[(i * 4) + 1] = (byte)((words[i] >> 8) & 0xff);
                bytes[(i * 4) + 2] = (byte)((words[i] >> 16) & 0xff);
                bytes[(i * 4) + 3] = (byte)((words[i] >> 24) & 0xff);
            }
            bytes[bytes.Length - 1] = (byte)0;
            return EInteger.FromBytes(bytes, true);
        }

        internal FastInteger2 SetInt(int val)
        {
            this.smallValue = val;
            this.integerMode = 0;
            return this;
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:FastInteger2.Multiply(System.Int32)"]/*'/>
        internal FastInteger2 Multiply(int val)
        {
            if (val == 0)
            {
                this.smallValue = 0;
                this.integerMode = 0;
            }
            else
            {
                switch (this.integerMode)
                {
                    case 0:
                        bool apos = this.smallValue > 0L;
                        bool bpos = val > 0L;
                        if (
                          (apos && ((!bpos && (Int32.MinValue / this.smallValue) > val) ||
                                (bpos && this.smallValue > (Int32.MaxValue / val)))) ||
                          (!apos && ((!bpos && this.smallValue != 0L &&
                                (Int32.MaxValue / this.smallValue) > val) ||
                                (bpos && this.smallValue < (Int32.MinValue / val)))))
                        {
                            // would overflow, convert to large
                            if (apos && bpos)
                            {
                                // if both operands are nonnegative
                                // convert to mutable big integer
                                this.integerMode = 1;
                                this.mnum = new MutableNumber(this.smallValue);
                                this.mnum.Multiply(val);
                            }
                            else
                            {
                                // if either operand is negative
                                // convert to big integer
                                this.integerMode = 2;
                                this.largeValue = (EInteger)this.smallValue;
                                this.largeValue *= (EInteger)val;
                            }
                        }
                        else
                        {
                            smallValue *= val;
                        }
                        break;
                    case 1:
                        if (val < 0)
                        {
                            this.integerMode = 2;
                            this.largeValue = this.mnum.ToEInteger();
                            this.largeValue *= (EInteger)val;
                        }
                        else
                        {
                            mnum.Multiply(val);
                        }
                        break;
                    case 2:
                        this.largeValue *= (EInteger)val;
                        break;
                    default: throw new InvalidOperationException();
                }
            }
            return this;
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:FastInteger2.Subtract(FastInteger2)"]/*'/>
        internal FastInteger2 Subtract(FastInteger2 val)
        {
            EInteger valValue;
            switch (this.integerMode)
            {
                case 0:
                    if (val.integerMode == 0)
                    {
                        int vsv = val.smallValue;
                        if ((vsv < 0 && Int32.MaxValue + vsv < this.smallValue) ||
                            (vsv > 0 && Int32.MinValue + vsv > this.smallValue))
                        {
                            // would overflow, convert to large
                            this.integerMode = 2;
                            this.largeValue = (EInteger)this.smallValue;
                            this.largeValue -= (EInteger)vsv;
                        }
                        else
                        {
                            this.smallValue -= vsv;
                        }
                    }
                    else
                    {
                        integerMode = 2;
                        largeValue = (EInteger)smallValue;
                        valValue = val.AsBigInteger();
                        largeValue -= (EInteger)valValue;
                    }
                    break;
                case 1:
                    if (val.integerMode == 1)
                    {
                        // NOTE: Mutable numbers are
                        // currently always zero or positive
                        this.mnum.Subtract(val.mnum);
                    }
                    else if (val.integerMode == 0 && val.smallValue >= 0)
                    {
                        mnum.SubtractInt(val.smallValue);
                    }
                    else
                    {
                        integerMode = 2;
                        largeValue = mnum.ToEInteger();
                        valValue = val.AsBigInteger();
                        largeValue -= (EInteger)valValue;
                    }
                    break;
                case 2:
                    valValue = val.AsBigInteger();
                    this.largeValue -= (EInteger)valValue;
                    break;
                default: throw new InvalidOperationException();
            }
            return this;
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:FastInteger2.SubtractInt(System.Int32)"]/*'/>
        internal FastInteger2 SubtractInt(int val)
        {
            if (val == Int32.MinValue)
            {
                return this.AddInt(Int32.MaxValue).AddInt(1);
            }
            if (this.integerMode == 0)
            {
                if ((val < 0 && Int32.MaxValue + val < this.smallValue) ||
                        (val > 0 && Int32.MinValue + val > this.smallValue))
                {
                    // would overflow, convert to large
                    this.integerMode = 2;
                    this.largeValue = (EInteger)this.smallValue;
                    this.largeValue -= (EInteger)val;
                }
                else
                {
                    this.smallValue -= val;
                }
                return this;
            }
            return this.AddInt(-val);
        }

        internal FastInteger2 Add(FastInteger2 val)
        {
            EInteger valValue;
            switch (this.integerMode)
            {
                case 0:
                    if (val.integerMode == 0)
                    {
                        if ((this.smallValue < 0 && (int)val.smallValue < Int32.MinValue
                        - this.smallValue) ||
                            (this.smallValue > 0 && (int)val.smallValue > Int32.MaxValue
                            - this.smallValue))
                        {
                            // would overflow
                            if (val.smallValue >= 0)
                            {
                                this.integerMode = 1;
                                this.mnum = new MutableNumber(this.smallValue);
                                this.mnum.Add(val.smallValue);
                            }
                            else
                            {
                                this.integerMode = 2;
                                this.largeValue = (EInteger)this.smallValue;
                                this.largeValue += (EInteger)val.smallValue;
                            }
                        }
                        else
                        {
                            this.smallValue += val.smallValue;
                        }
                    }
                    else
                    {
                        integerMode = 2;
                        largeValue = (EInteger)smallValue;
                        valValue = val.AsBigInteger();
                        largeValue += (EInteger)valValue;
                    }
                    break;
                case 1:
                    if (val.integerMode == 0 && val.smallValue >= 0)
                    {
                        this.mnum.Add(val.smallValue);
                    }
                    else
                    {
                        integerMode = 2;
                        largeValue = mnum.ToEInteger();
                        valValue = val.AsBigInteger();
                        largeValue += (EInteger)valValue;
                    }
                    break;
                case 2:
                    valValue = val.AsBigInteger();
                    this.largeValue += (EInteger)valValue;
                    break;
                default: throw new InvalidOperationException();
            }
            return this;
        }

        internal FastInteger2 AddInt(int val)
        {
            EInteger valValue;
            switch (this.integerMode)
            {
                case 0:
                    if ((this.smallValue < 0 && (int)val < Int32.MinValue -
                  this.smallValue) || (this.smallValue > 0 && (int)val >
                      Int32.MaxValue - this.smallValue))
                    {
                        // would overflow
                        if (val >= 0)
                        {
                            this.integerMode = 1;
                            this.mnum = new MutableNumber(this.smallValue);
                            this.mnum.Add(val);
                        }
                        else
                        {
                            this.integerMode = 2;
                            this.largeValue = (EInteger)this.smallValue;
                            this.largeValue += (EInteger)val;
                        }
                    }
                    else
                    {
                        smallValue += val;
                    }
                    break;
                case 1:
                    if (val >= 0)
                    {
                        this.mnum.Add(val);
                    }
                    else
                    {
                        integerMode = 2;
                        largeValue = mnum.ToEInteger();
                        valValue = (EInteger)val;
                        largeValue += (EInteger)valValue;
                    }
                    break;
                case 2:
                    valValue = (EInteger)val;
                    this.largeValue += (EInteger)valValue;
                    break;
                default: throw new InvalidOperationException();
            }
            return this;
        }

        internal bool CanFitInInt32()
        {
            switch (this.integerMode)
            {
                case 0:
                    return true;
                case 1:
                    return this.mnum.CanFitInInt32();
                case 2:
                    {
                        return this.largeValue.CanFitInInt32();
                    }
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="P:FastInteger2.Sign"]/*'/>
        internal int Sign
        {
            get
            {
                switch (this.integerMode)
                {
                    case 0:
                        return (this.smallValue == 0) ? 0 : ((this.smallValue < 0) ? -1 :
                            1);
                    case 1:
                        return this.mnum.Sign;
                    case 2:
                        return this.largeValue.Sign;
                    default: return 0;
                }
            }
        }

        internal EInteger AsBigInteger()
        {
            switch (this.integerMode)
            {
                case 0:
                    return EInteger.FromInt32(this.smallValue);
                case 1:
                    return this.mnum.ToEInteger();
                case 2:
                    return this.largeValue;
                default: throw new InvalidOperationException();
            }
        }
    }
    #endregion
}
