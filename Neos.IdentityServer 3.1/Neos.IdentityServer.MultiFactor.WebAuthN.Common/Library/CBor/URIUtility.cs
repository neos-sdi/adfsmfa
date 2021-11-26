using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//******************************************************************************************************************************************************************************************//
// Public Domain                                                                                                                                                                            //
//                                                                                                                                                                                          //
// Written by Peter O. in 2014.                                                                                                                                                             //
//                                                                                                                                                                                          //
// Any copyright is dedicated to the Public Domain. http://creativecommons.org/publicdomain/zero/1.0/                                                                                       //
//                                                                                                                                                                                          //
// If you like this, you should donate to Peter O. at: http://peteroupc.github.io/                                                                                                          //
//******************************************************************************************************************************************************************************************//
namespace Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor
{
    /// <summary>Contains utility methods for processing Uniform Resource
    /// Identifiers (URIs) and Internationalized Resource Identifiers
    /// (IRIs) under RFC3986 and RFC3987, respectively. In the following
    /// documentation, URIs and IRIs include URI references and IRI
    /// references, for convenience.
    /// <para>There are five components to a URI: scheme, authority, path,
    /// query, and fragment identifier. The generic syntax to these
    /// components is defined in RFC3986 and extended in RFC3987. According
    /// to RFC3986, different URI schemes can further restrict the syntax
    /// of the authority, path, and query component (see also RFC 7320).
    /// However, the syntax of fragment identifiers depends on the media
    /// type (also known as MIME type) of the resource a URI references
    /// (see also RFC 3986 and RFC 7320). As of September 3, 2019, only the
    /// following media types specify a syntax for fragment
    /// identifiers:</para>
    /// <list>
    /// <item>The following application/* media types: epub + zip, pdf,
    /// senml + cbor, senml + json, senml-exi, sensml + cbor, sensml +
    /// json, sensml-exi, smil, vnd.3gpp-v2x-local-service-information,
    /// vnd.3gpp.mcdata-signalling, vnd.collection.doc + json, vnd.hc +
    /// json, vnd.hyper + json, vnd.hyper-item + json, vnd.mason + json,
    /// vnd.microsoft.portable-executable, vnd.oma.bcast.sgdu,
    /// vnd.shootproof + json</item>
    /// <item>The following image/* media types: avci, avcs, heic,
    /// heic-sequence, heif, heif-sequence, hej2k, hsj2, jxra, jxrs, jxsi,
    /// jxss</item>
    /// <item>The XML media types: application/xml,
    /// application/xml-external-parsed-entity, text/xml,
    /// text/xml-external-parsed-entity, application/xml-dtd</item>
    /// <item>All media types with subtypes ending in "+xml" (see RFC 7303)
    /// use XPointer Framework syntax as fragment identifiers, except the
    /// following application/* media types: dicom + xml (syntax not
    /// defined), senml + xml (own syntax), sensml + xml (own syntax), ttml
    /// + xml (own syntax), xliff + xml (own syntax), yang-data + xml
    /// (syntax not defined)</item>
    /// <item>font/collection</item>
    /// <item>multipart/x-mixed-replace</item>
    /// <item>text/plain</item>
    /// <item>text/csv</item>
    /// <item>text/html</item>
    /// <item>text/markdown</item>
    /// <item>text/vnd.a</item></list></summary>
    public static class URIUtility
    {
        /// <summary>Specifies whether certain characters are allowed when
        /// parsing IRIs and URIs.</summary>
        public enum ParseMode
        {
            /// <summary>The rules follow the syntax for parsing IRIs. In
            /// particular, many code points outside the Basic Latin range (U+0000
            /// to U+007F) are allowed. Strings with unpaired surrogate code points
            /// are considered invalid.</summary>
            IRIStrict,

            /// <summary>The rules follow the syntax for parsing IRIs, except that
            /// code points outside the Basic Latin range (U+0000 to U+007F) are
            /// not allowed.</summary>
            URIStrict,

            /// <summary>The rules only check for the appropriate delimiters when
            /// splitting the path, without checking if all the characters in each
            /// component are valid. Even with this mode, strings with unpaired
            /// surrogate code points are considered invalid.</summary>
            IRILenient,

            /// <summary>The rules only check for the appropriate delimiters when
            /// splitting the path, without checking if all the characters in each
            /// component are valid. Code points outside the Basic Latin range
            /// (U+0000 to U+007F) are not allowed.</summary>
            URILenient,

            /// <summary>The rules only check for the appropriate delimiters when
            /// splitting the path, without checking if all the characters in each
            /// component are valid. Unpaired surrogate code points are treated as
            /// though they were replacement characters instead for the purposes of
            /// these rules, so that strings with those code points are not
            /// considered invalid strings.</summary>
            IRISurrogateLenient,
        }

        private const string HexChars = "0123456789ABCDEF";

        private static void AppendAuthority(
            StringBuilder builder,
            string refValue,
            int[] segments)
        {
            if (segments[2] >= 0)
            {
                builder.Append("//");
                builder.Append(
                    refValue.Substring(
                    segments[2],
                    segments[3] - segments[2]));
            }
        }

        private static void AppendFragment(
            StringBuilder builder,
            string refValue,
            int[] segments)
        {
            if (segments[8] >= 0)
            {
                builder.Append('#');
                builder.Append(
                    refValue.Substring(
                    segments[8],
                    segments[9] - segments[8]));
            }
        }

        private static void AppendNormalizedPath(
            StringBuilder builder,
            string refValue,
            int[] segments)
        {
            builder.Append(
                NormalizePath(
                refValue.Substring(
                    segments[4],
                    segments[5] - segments[4])));
        }

        private static void AppendPath(
            StringBuilder builder,
            string refValue,
            int[] segments)
        {
            builder.Append(
                refValue.Substring(
                segments[4],
                segments[5] - segments[4]));
        }

        private static void AppendQuery(
            StringBuilder builder,
            string refValue,
            int[] segments)
        {
            if (segments[6] >= 0)
            {
                builder.Append('?');
                builder.Append(
                    refValue.Substring(
                    segments[6],
                    segments[7] - segments[6]));
            }
        }

        private static void AppendScheme(
            StringBuilder builder,
            string refValue,
            int[] segments)
        {
            if (segments[0] >= 0)
            {
                builder.Append(
                    refValue.Substring(
                    segments[0],
                    segments[1] - segments[0]));
                builder.Append(':');
            }
        }

        /// <summary>Checks a text string representing a URI or IRI and escapes
        /// characters it has that can't appear in URIs or IRIs. The function
        /// is idempotent; that is, calling the function again on the result
        /// with the same mode doesn't change the result.</summary>
        /// <param name='s'>A text string representing a URI or IRI. Can be
        /// null.</param>
        /// <param name='mode'>Has the following meaning: 0 = Encode reserved
        /// code points, code points below U+0021, code points above U+007E,
        /// and square brackets within the authority component, and do the
        /// IRISurrogateLenient check. 1 = Encode code points above U+007E, and
        /// square brackets within the authority component, and do the
        /// IRIStrict check. 2 = Same as 1, except the check is
        /// IRISurrogateLenient. 3 = Same as 0, except that percent characters
        /// that begin illegal percent-encoding are also encoded.</param>
        /// <returns>A form of the URI or IRI that possibly contains escaped
        /// characters, or null if s is null.</returns>
        public static string EscapeURI(string s, int mode)
        {
            if (s == null)
            {
                return null;
            }
            int[] components = null;
            if (mode == 1)
            {
                components = (
                    s == null) ? null : SplitIRI(
                    s,
                    0,
                    s.Length,
                    URIUtility.ParseMode.IRIStrict);
                if (components == null)
                {
                    return null;
                }
            }
            else
            {
                components = (s == null) ? null : SplitIRI(
                    s,
                    0,
                    s.Length,
                    URIUtility.ParseMode.IRISurrogateLenient);
            }
            var index = 0;
            int valueSLength = s.Length;
            var builder = new StringBuilder();
            while (index < valueSLength)
            {
                int c = s[index];
                if ((c & 0xfc00) == 0xd800 && index + 1 < valueSLength &&
                    (s[index + 1] & 0xfc00) == 0xdc00)
                {
                    // Get the Unicode code point for the surrogate pair
                    c = 0x10000 + ((c & 0x3ff) << 10) + (s[index + 1] & 0x3ff);
                    ++index;
                }
                else if ((c & 0xf800) == 0xd800)
                {
                    c = 0xfffd;
                }
                if (mode == 0 || mode == 3)
                {
                    if (c == '%' && mode == 3)
                    {
                        // Check for illegal percent encoding
                        if (index + 2 >= valueSLength || !IsHexChar(s[index + 1]) ||
                            !IsHexChar(s[index + 2]))
                        {
                            PercentEncodeUtf8(builder, c);
                        }
                        else
                        {
                            if (c <= 0xffff)
                            {
                                builder.Append((char)c);
                            }
                            else if (c <= 0x10ffff)
                            {
                                builder.Append((char)((((c - 0x10000) >> 10) & 0x3ff) |
                                    0xd800));
                                builder.Append((char)(((c - 0x10000) & 0x3ff) | 0xdc00));
                            }
                        }
                        ++index;
                        continue;
                    }
                    if (c >= 0x7f || c <= 0x20 || ((c & 0x7f) == c &&
            "{}|^\\`<>\"".IndexOf((char)c) >= 0))
                    {
                        PercentEncodeUtf8(builder, c);
                    }
                    else if (c == '[' || c == ']')
                    {
                        if (components != null && index >= components[2] && index <
                            components[3])
                        {
                            // within the authority component, so don't percent-encode
                            builder.Append((char)c);
                        }
                        else
                        {
                            // percent encode
                            PercentEncodeUtf8(builder, c);
                        }
                    }
                    else
                    {
                        if (c <= 0xffff)
                        {
                            builder.Append((char)c);
                        }
                        else if (c <= 0x10ffff)
                        {
                            builder.Append((char)((((c - 0x10000) >> 10) & 0x3ff) |
                0xd800));
                            builder.Append((char)(((c - 0x10000) & 0x3ff) | 0xdc00));
                        }
                    }
                }
                else if (mode == 1 || mode == 2)
                {
                    if (c >= 0x80)
                    {
                        PercentEncodeUtf8(builder, c);
                    }
                    else if (c == '[' || c == ']')
                    {
                        if (components != null && index >= components[2] && index <
                            components[3])
                        {
                            // within the authority component, so don't percent-encode
                            builder.Append((char)c);
                        }
                        else
                        {
                            // percent encode
                            PercentEncodeUtf8(builder, c);
                        }
                    }
                    else
                    {
                        if (c <= 0xffff)
                        {
                            builder.Append((char)c);
                        }
                        else if (c <= 0x10ffff)
                        {
                            builder.Append((char)((((c - 0x10000) >> 10) & 0x3ff) |
                0xd800));
                            builder.Append((char)(((c - 0x10000) & 0x3ff) | 0xdc00));
                        }
                    }
                }
                ++index;
            }
            return builder.ToString();
        }

        /// <summary>
        ///  Determines whether the string is a valid IRI with a
        /// scheme component. This can be used to check for
        /// relative IRI references.
        /// <para>The following cases return true:</para>
        /// <code>xx-x:mm example:/ww</code>
        ///  The following cases return false:
        /// <code>x@y:/z /x/y/z example.xyz</code>
        ///  .
        /// </summary>
        /// <param name='refValue'>A string representing an IRI to
        /// check.</param>
        /// <returns><c>true</c>
        ///  if the string is a valid IRI with a scheme
        /// component; otherwise, <c>false</c>
        /// .</returns>
        public static bool HasScheme(string refValue)
        {
            int[] segments = (refValue == null) ? null : SplitIRI(
                refValue,
                0,
                refValue.Length,
                URIUtility.ParseMode.IRIStrict);
            return segments != null && segments[0] >= 0;
        }

        /// <summary>
        ///  Determines whether the string is a valid URI with a
        /// scheme component. This can be used to check for
        /// relative URI references. The following cases return
        /// true:
        /// <code>http://example/z xx-x:mm example:/ww</code>
        ///  The following cases return false:
        /// <code>x@y:/z /x/y/z example.xyz</code>
        ///  .
        /// </summary>
        /// <param name='refValue'>A string representing an IRI to
        /// check.</param>
        /// <returns><c>true</c>
        ///  if the string is a valid URI with a scheme
        /// component; otherwise, <c>false</c>
        /// .</returns>
        public static bool HasSchemeForURI(string refValue)
        {
            int[] segments = (refValue == null) ? null : SplitIRI(
                refValue,
                0,
                refValue.Length,
                URIUtility.ParseMode.URIStrict);
            return segments != null && segments[0] >= 0;
        }

        private static bool IsHexChar(char c)
        {
            return (c >= 'a' && c <= 'f') ||
                (c >= 'A' && c <= 'F') || (c >= '0' && c <= '9');
        }

        private static int ToHex(char b1)
        {
            if (b1 >= '0' && b1 <= '9')
            {
                return b1 - '0';
            }
            else if (b1 >= 'A' && b1 <= 'F')
            {
                return b1 + 10 - 'A';
            }
            else
            {
                return (b1 >= 'a' && b1 <= 'f') ? (b1 + 10 - 'a') : 1;
            }
        }

        /// <summary>Decodes percent-encoding (of the form "%XX" where X is a
        /// hexadecimal digit) in the given string. Successive percent-encoded
        /// bytes are assumed to form characters in UTF-8.</summary>
        /// <param name='str'>A string that may contain percent encoding. May
        /// be null.</param>
        /// <returns>The string in which percent-encoding was
        /// decoded.</returns>
        public static string PercentDecode(string str)
        {
            return (str == null) ? null : PercentDecode(str, 0, str.Length);
        }

        /// <summary>Decodes percent-encoding (of the form "%XX" where X is a
        /// hexadecimal digit) in the given string, with an option to fail
        /// rather than replace invalid encoding. Successive percent-encoded
        /// bytes are assumed to form characters in UTF-8.</summary>
        /// <param name='str'>A string that may contain percent encoding. May
        /// be null.</param>
        /// <param name='replace'>Indicates whether to replace invalid encoding
        /// with U+FFFD, the replacement character. If false, returns null if
        /// invalid encoding is found.</param>
        /// <returns>The string in which percent-encoding was decoded. Returns
        /// null if "str" is null or if "replace" is true and the string has an
        /// invalid encoding.</returns>
        public static string PercentDecode(string str, bool replace)
        {
            return (str == null) ? null : PercentDecode(str, 0, str.Length, replace);
        }

        /// <summary>Decodes percent-encoding (of the form "%XX" where X is a
        /// hexadecimal digit) in the given portion of a string. Successive
        /// percent-encoded bytes are assumed to form characters in
        /// UTF-8.</summary>
        /// <param name='str'>A string a portion of which may contain percent
        /// encoding. May be null.</param>
        /// <param name='index'>Index starting at 0 showing where the desired
        /// portion of <paramref name='str'/> begins.</param>
        /// <param name='endIndex'>Index starting at 0 showing where the
        /// desired portion of <paramref name='str'/> ends. The character
        /// before this index is the last character.</param>
        /// <returns>The portion of the given string in which percent-encoding
        /// was decoded. Returns null if <paramref name='str'/> is
        /// null.</returns>
        public static string PercentDecode(string str, int index, int endIndex)
        {
            return PercentDecode(str, index, endIndex, true);
        }

        /// <summary>Decodes percent-encoding (of the form "%XX" where X is a
        /// hexadecimal digit) in the given portion of a string, with an option
        /// to fail rather than replace invalid encoding. Successive
        /// percent-encoded bytes are assumed to form characters in
        /// UTF-8.</summary>
        /// <param name='str'>A string a portion of which may contain percent
        /// encoding. May be null.</param>
        /// <param name='index'>Index starting at 0 showing where the desired
        /// portion of <paramref name='str'/> begins.</param>
        /// <param name='endIndex'>Index starting at 0 showing where the
        /// desired portion of <paramref name='str'/> ends. The character
        /// before this index is the last character.</param>
        /// <param name='replace'>Indicates whether to replace invalid encoding
        /// with U+FFFD, the replacement character. If false, returns null if
        /// invalid encoding is found.</param>
        /// <returns>The portion of the given string in which percent-encoding
        /// was decoded. Returns null if <paramref name='str'/> is null or if
        /// "replace" is true and the portion of the string has an invalid
        /// encoding.</returns>
        /// <exception cref='ArgumentException'>doesn't satisfy
        /// lastIndex&amp;gt;= index.</exception>
        public static string PercentDecode(
            string str,
            int index,
            int endIndex,
            bool replace)
        {
            if (str == null)
            {
                return null;
            }
            // Quick check
            var quickCheck = true;
            var lastIndex = index;
            int i = index;
            for (; i < endIndex; ++i)
            {
                if (str[i] >= 0xd800 || str[i] == '%')
                {
                    quickCheck = false;
                    lastIndex = i;
                    break;
                }
            }
            if (quickCheck)
            {
                return str.Substring(index, endIndex - index);
            }
#if DEBUG
            if (!(lastIndex >= index))
            {
                throw new ArgumentException("doesn't satisfy lastIndex>= index");
            }
#endif

            var retString = new StringBuilder();
            retString.Append(str, index, lastIndex - index);
            var cp = 0;
            var bytesSeen = 0;
            var bytesNeeded = 0;
            var lower = 0x80;
            var upper = 0xbf;
            var markedPos = -1;
            for (i = lastIndex; i < endIndex; ++i)
            {
                int c = str[i];
                if ((c & 0xfc00) == 0xd800 && i + 1 < endIndex &&
                    (str[i + 1] & 0xfc00) == 0xdc00)
                {
                    // Get the Unicode code point for the surrogate pair
                    c = 0x10000 + ((c & 0x3ff) << 10) + (str[i + 1] & 0x3ff);
                    ++i;
                }
                else if ((c & 0xf800) == 0xd800)
                {
                    if (!replace)
                    {
                        return null;
                    }
                    c = 0xfffd;
                }
                if (c == '%')
                {
                    if (i + 2 < endIndex)
                    {
                        int a = ToHex(str[i + 1]);
                        int b = ToHex(str[i + 2]);
                        if (a >= 0 && b >= 0)
                        {
                            b = (a * 16) + b;
                            i += 2;
                            // b now contains the byte read
                            if (bytesNeeded == 0)
                            {
                                // this is the lead byte
                                if (b < 0x80)
                                {
                                    retString.Append((char)b);
                                    continue;
                                }
                                else if (b >= 0xc2 && b <= 0xdf)
                                {
                                    markedPos = i;
                                    bytesNeeded = 1;
                                    cp = b - 0xc0;
                                }
                                else if (b >= 0xe0 && b <= 0xef)
                                {
                                    markedPos = i;
                                    lower = (b == 0xe0) ? 0xa0 : 0x80;
                                    upper = (b == 0xed) ? 0x9f : 0xbf;
                                    bytesNeeded = 2;
                                    cp = b - 0xe0;
                                }
                                else if (b >= 0xf0 && b <= 0xf4)
                                {
                                    markedPos = i;
                                    lower = (b == 0xf0) ? 0x90 : 0x80;
                                    upper = (b == 0xf4) ? 0x8f : 0xbf;
                                    bytesNeeded = 3;
                                    cp = b - 0xf0;
                                }
                                else
                                {
                                    // illegal byte in UTF-8
                                    if (!replace)
                                    {
                                        return null;
                                    }
                                    retString.Append('\uFFFD');
                                    continue;
                                }
                                cp <<= 6 * bytesNeeded;
                                continue;
                            }
                            else
                            {
                                // this is a second or further byte
                                if (b < lower || b > upper)
                                {
                                    // illegal trailing byte
                                    if (!replace)
                                    {
                                        return null;
                                    }
                                    cp = bytesNeeded = bytesSeen = 0;
                                    lower = 0x80;
                                    upper = 0xbf;
                                    i = markedPos; // reset to the last marked position
                                    retString.Append('\uFFFD');
                                    continue;
                                }
                                // reset lower and upper for the third
                                // and further bytes
                                lower = 0x80;
                                upper = 0xbf;
                                ++bytesSeen;
                                cp += (b - 0x80) << (6 * (bytesNeeded - bytesSeen));
                                markedPos = i;
                                if (bytesSeen != bytesNeeded)
                                {
                                    // continue if not all bytes needed
                                    // were read yet
                                    continue;
                                }
                                int ret = cp;
                                cp = 0;
                                bytesSeen = 0;
                                bytesNeeded = 0;
                                // append the Unicode character
                                if (ret <= 0xffff)
                                {
                                    retString.Append((char)ret);
                                }
                                else
                                {
                                    retString.Append((char)((((ret - 0x10000) >> 10) & 0x3ff) |
                                        0xd800));
                                    retString.Append((char)(((ret - 0x10000) & 0x3ff) |
                    0xdc00));
                                }
                                continue;
                            }
                        }
                    }
                }
                if (bytesNeeded > 0)
                {
                    // we expected further bytes here,
                    // so emit a replacement character instead
                    if (!replace)
                    {
                        return null;
                    }
                    bytesNeeded = 0;
                    retString.Append('\uFFFD');
                }
                // append the code point as is
                if (c <= 0xffff)
                {
                    {
                        retString.Append((char)c);
                    }
                }
                else if (c <= 0x10ffff)
                {
                    retString.Append((char)((((c - 0x10000) >> 10) & 0x3ff) | 0xd800));
                    retString.Append((char)(((c - 0x10000) & 0x3ff) | 0xdc00));
                }
            }
            if (bytesNeeded > 0)
            {
                // we expected further bytes here,
                // so emit a replacement character instead
                if (!replace)
                {
                    return null;
                }
                bytesNeeded = 0;
                retString.Append('\uFFFD');
            }
            return retString.ToString();
        }

        /// <summary>Encodes characters other than "unreserved" characters for
        /// URIs.</summary>
        /// <param name='s'>A string to encode.</param>
        /// <returns>The encoded string.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='s'/> is null.</exception>
        public static string EncodeStringForURI(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }
            var index = 0;
            var builder = new StringBuilder();
            while (index < s.Length)
            {
                int c = s[index];
                if ((c & 0xfc00) == 0xd800 && index + 1 < s.Length &&
                    (s[index + 1] & 0xfc00) == 0xdc00)
                {
                    // Get the Unicode code point for the surrogate pair
                    c = 0x10000 + ((c & 0x3ff) << 10) + (s[index + 1] & 0x3ff);
                }
                else if ((c & 0xf800) == 0xd800)
                {
                    c = 0xfffd;
                }
                if (c >= 0x10000)
                {
                    ++index;
                }
                if ((c & 0x7F) == c && ((c >= 'A' && c <= 'Z') ||
                    (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') ||
        "-_.~".IndexOf((char)c) >= 0))
                {
                    builder.Append((char)c);
                    ++index;
                }
                else
                {
                    PercentEncodeUtf8(builder, c);
                    ++index;
                }
            }
            return builder.ToString();
        }

        private static bool IsIfragmentChar(int c)
        {
            // '%' omitted
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                (c >= '0' && c <= '9') || ((c & 0x7F) == c &&
        "/?-._~:@!$&'()*+,;=".IndexOf((char)c) >= 0) ||

                (c >= 0xa0 && c <= 0xd7ff) || (c >= 0xf900 && c <= 0xfdcf) ||
                (c >= 0xfdf0 && c <= 0xffef) ||
                (c >= 0xe1000 && c <= 0xefffd) || (c >= 0x10000 && c <= 0xdfffd &&
                (c & 0xfffe) != 0xfffe);
        }

        private static bool IsIpchar(int c)
        {
            // '%' omitted
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                (c >= '0' && c <= '9') || ((c & 0x7F) == c &&
        "/-._~:@!$&'()*+,;=".IndexOf((char)c) >= 0) ||

                (c >= 0xa0 && c <= 0xd7ff) || (c >= 0xf900 && c <= 0xfdcf) ||
                (c >= 0xfdf0 && c <= 0xffef) ||
                (c >= 0xe1000 && c <= 0xefffd) || (c >= 0x10000 && c <= 0xdfffd &&
                (c & 0xfffe) != 0xfffe);
        }

        private static bool IsIqueryChar(int c)
        {
            // '%' omitted
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                (c >= '0' && c <= '9') || ((c & 0x7F) == c &&
        "/?-._~:@!$&'()*+,;=".IndexOf((char)c) >= 0) ||

                (c >= 0xa0 && c <= 0xd7ff) || (c >= 0xe000 && c <= 0xfdcf) ||
                (c >= 0xfdf0 && c <= 0xffef) ||
                (c >= 0x10000 && c <= 0x10fffd && (c & 0xfffe) != 0xfffe &&
                !(c >= 0xe0000 && c <= 0xe0fff));
        }

        private static bool IsIRegNameChar(int c)
        {
            // '%' omitted
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                (c >= '0' && c <= '9') || ((c & 0x7F) == c &&
        "-._~!$&'()*+,;=".IndexOf((char)c) >= 0) ||

                (c >= 0xa0 && c <= 0xd7ff) || (c >= 0xf900 && c <= 0xfdcf) ||
                (c >= 0xfdf0 && c <= 0xffef) ||
                (c >= 0xe1000 && c <= 0xefffd) || (c >= 0x10000 && c <= 0xdfffd &&
                (c & 0xfffe) != 0xfffe);
        }

        private static bool IsIUserInfoChar(int c)
        {
            // '%' omitted
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                (c >= '0' && c <= '9') || ((c & 0x7F) == c &&
        "-._~:!$&'()*+,;=".IndexOf((char)c) >= 0) ||

                (c >= 0xa0 && c <= 0xd7ff) || (c >= 0xf900 && c <= 0xfdcf) ||
                (c >= 0xfdf0 && c <= 0xffef) ||
                (c >= 0xe1000 && c <= 0xefffd) || (c >= 0x10000 && c <= 0xdfffd &&
                (c & 0xfffe) != 0xfffe);
        }

        /// <summary>Determines whether the substring is a valid CURIE
        /// reference under RDFA 1.1. (The CURIE reference is the part after
        /// the colon.).</summary>
        /// <param name='s'>A string containing a CURIE reference. Can be
        /// null.</param>
        /// <param name='offset'>An index starting at 0 showing where the
        /// desired portion of "s" begins.</param>
        /// <param name='length'>The number of elements in the desired portion
        /// of "s" (but not more than "s" 's length).</param>
        /// <returns><c>true</c> if the substring is a valid CURIE reference
        /// under RDFA 1; otherwise, <c>false</c>. Returns false if <paramref
        /// name='s'/> is null.</returns>
        /// <exception cref='ArgumentException'>Either <paramref
        /// name='offset'/> or <paramref name='length'/> is less than 0 or
        /// greater than <paramref name='s'/> 's length, or <paramref
        /// name='s'/> 's length minus <paramref name='offset'/> is less than
        /// <paramref name='length'/>.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='s'/> is null.</exception>
        public static bool IsValidCurieReference(
            string s,
            int offset,
            int length)
        {
            if (s == null)
            {
                return false;
            }
            if (offset < 0)
            {
                throw new ArgumentException("offset(" + offset + ") is less than " +
                    "0 ");
            }
            if (offset > s.Length)
            {
                throw new ArgumentException("offset(" + offset + ") is more than " +
                    s.Length);
            }
            if (length < 0)
            {
                throw new ArgumentException(
                    "length (" + length + ") is less than " + "0 ");
            }
            if (length > s.Length)
            {
                throw new ArgumentException(
                    "length (" + length + ") is more than " + s.Length);
            }
            if (s.Length - offset < length)
            {
                throw new ArgumentException(
                    "s's length minus " + offset + " (" + (s.Length - offset) +
                    ") is less than " + length);
            }
            if (length == 0)
            {
                return true;
            }
            int index = offset;
            int valueSLength = offset + length;
            var state = 0;
            if (index + 2 <= valueSLength && s[index] == '/' && s[index + 1] == '/')
            {
                // has an authority, which is not allowed
                return false;
            }
            state = 0; // IRI Path
            while (index < valueSLength)
            {
                // Get the next Unicode character
                int c = s[index];
                if ((c & 0xfc00) == 0xd800 && index + 1 < valueSLength &&
                    (s[index + 1] & 0xfc00) == 0xdc00)
                {
                    // Get the Unicode code point for the surrogate pair
                    c = 0x10000 + ((c & 0x3ff) << 10) + (s[index + 1] & 0x3ff);
                    ++index;
                }
                else if ((c & 0xf800) == 0xd800)
                {
                    // error
                    return false;
                }
                if (c == '%')
                {
                    // Percent encoded character
                    if (index + 2 < valueSLength && IsHexChar(s[index + 1]) &&
                        IsHexChar(s[index + 2]))
                    {
                        index += 3;
                        continue;
                    }
                    return false;
                }
                if (state == 0)
                { // Path
                    if (c == '?')
                    {
                        state = 1; // move to query state
                    }
                    else if (c == '#')
                    {
                        state = 2; // move to fragment state
                    }
                    else if (!IsIpchar(c))
                    {
                        return false;
                    }
                    ++index;
                }
                else if (state == 1)
                { // Query
                    if (c == '#')
                    {
                        state = 2; // move to fragment state
                    }
                    else if (!IsIqueryChar(c))
                    {
                        return false;
                    }
                    ++index;
                }
                else if (state == 2)
                { // Fragment
                    if (!IsIfragmentChar(c))
                    {
                        return false;
                    }
                    ++index;
                }
            }
            return true;
        }

        /// <summary>Builds an internationalized resource identifier (IRI) from
        /// its components.</summary>
        /// <param name='schemeAndAuthority'>String representing a scheme
        /// component, an authority component, or both. Examples of this
        /// parameter include "example://example", "example:", and "//example",
        /// but not "example". Can be null or empty.</param>
        /// <param name='path'>A string representing a path component. Can be
        /// null or empty.</param>
        /// <param name='query'>The query string. Can be null or empty.</param>
        /// <param name='fragment'>The fragment identifier. Can be null or
        /// empty.</param>
        /// <returns>A URI built from the given components.</returns>
        /// <exception cref='ArgumentException'>Invalid schemeAndAuthority
        /// parameter, or the arguments result in an invalid IRI.</exception>
        public static string BuildIRI(
            string schemeAndAuthority,
            string path,
            string query,
            string fragment)
        {
            var builder = new StringBuilder();
            if (!String.IsNullOrEmpty(schemeAndAuthority))
            {
                int[] irisplit = SplitIRI(schemeAndAuthority);
                // NOTE: Path component is always present in URIs;
                // we check here whether path component is empty
                if (irisplit == null || (irisplit[0] < 0 && irisplit[2] < 0) ||
                    irisplit[4] != irisplit[5] || irisplit[6] >= 0 || irisplit[8] >= 0)
                {
                    throw new ArgumentException("invalid schemeAndAuthority");
                }
            }
            if (String.IsNullOrEmpty(path))
            {
                path = String.Empty;
            }
            for (var phase = 0; phase < 3; ++phase)
            {
                string s = path;
                if (phase == 1)
                {
                    s = query;
                    if (query == null)
                    {
                        continue;
                    }
                    builder.Append('?');
                }
                else if (phase == 2)
                {
                    s = fragment;
                    if (fragment == null)
                    {
                        continue;
                    }
                    builder.Append('#');
                }
                var index = 0;
                if (query == null || fragment == null)
                {
                    continue;
                }
                while (index < s.Length)
                {
                    int c = s[index];
                    if ((c & 0xfc00) == 0xd800 && index + 1 < s.Length &&
                        (s[index + 1] & 0xfc00) == 0xdc00)
                    {
                        // Get the Unicode code point for the surrogate pair
                        c = 0x10000 + ((c & 0x3ff) << 10) + (s[index + 1] & 0x3ff);
                    }
                    else if ((c & 0xf800) == 0xd800)
                    {
                        c = 0xfffd;
                    }
                    if (c >= 0x10000)
                    {
                        ++index;
                    }
                    if (c == '%')
                    {
                        if (index + 2 < s.Length && IsHexChar(s[index + 1]) &&
                            IsHexChar(s[index + 2]))
                        {
                            builder.Append('%');
                            builder.Append(s[index + 1]);
                            builder.Append(s[index + 2]);
                            index += 3;
                        }
                        else
                        {
                            builder.Append("%25");
                            ++index;
                        }
                    }
                    else if ((c & 0x7f) == c &&
                    ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') ||
                        (c >= '0' && c <= '9') ||
                        "-_.~/(=):!$&'*+,;@".IndexOf((char)c) >= 0))
                    {
                        // NOTE: Question mark will be percent encoded even though
                        // it can appear in query and fragment strings
                        builder.Append((char)c);
                        ++index;
                    }
                    else
                    {
                        PercentEncodeUtf8(builder, c);
                        ++index;
                    }
                }
            }
            string ret = builder.ToString();
            if (SplitIRI(ret) == null)
            {
                throw new ArgumentException("The arguments result in an invalid IRI.");
            }
            return ret;
        }

        /// <summary>Returns whether a string is a valid IRI according to the
        /// IRIStrict parse mode.</summary>
        /// <param name='s'>A text string. Can be null.</param>
        /// <returns>True if the string is not null and is a valid IRI;
        /// otherwise, false.</returns>
        public static bool IsValidIRI(string s)
        {
            return ((s == null) ? null : SplitIRI(
                s,
                0,
                s.Length,
                URIUtility.ParseMode.IRIStrict)) != null;
        }

        /// <summary>Returns whether a string is a valid IRI according to the
        /// given parse mode.</summary>
        /// <param name='s'>A text string. Can be null.</param>
        /// <param name='parseMode'>The parse mode to use when checking for a
        /// valid IRI.</param>
        /// <returns>True if the string is not null and is a valid IRI;
        /// otherwise, false.</returns>
        public static bool IsValidIRI(string s, URIUtility.ParseMode parseMode)
        {
            return ((s == null) ? null : SplitIRI(
                s,
                0,
                s.Length,
                parseMode)) != null;
        }

        private const string ValueDotSlash = "." + "/";
        private const string ValueSlashDot = "/" + ".";

        private static string NormalizePath(string path)
        {
            int len = path.Length;
            if (len == 0 || path.Equals("..", StringComparison.Ordinal) ||
                path.Equals(".", StringComparison.Ordinal))
            {
                return String.Empty;
            }
            if (path.IndexOf(ValueSlashDot, StringComparison.Ordinal) < 0 &&
                path.IndexOf(
                ValueDotSlash,
                StringComparison.Ordinal) < 0)
            {
                return path;
            }
            var builder = new StringBuilder();
            var index = 0;
            while (index < len)
            {
                char c = path[index];
                if ((index + 3 <= len && c == '/' && path[index + 1] == '.' &&
                    path[index + 2] == '/') || (index + 2 == len && c == '.' &&
                    path[index + 1] == '.'))
                {
                    // begins with "/./" or is "..";
                    // move index by 2
                    index += 2;
                    continue;
                }
                if (index + 3 <= len && c == '.' &&
                    path[index + 1] == '.' && path[index + 2] == '/')
                {
                    // begins with "../";
                    // move index by 3
                    index += 3;
                    continue;
                }
                if ((index + 2 <= len && c == '.' &&
                    path[index + 1] == '/') || (index + 1 == len && c == '.'))
                {
                    // begins with "./" or is ".";
                    // move index by 1
                    ++index;
                    continue;
                }
                if (index + 2 == len && c == '/' &&
                    path[index + 1] == '.')
                {
                    // is "/."; append '/' and break
                    builder.Append('/');
                    break;
                }
                if (index + 3 == len && c == '/' &&
                    path[index + 1] == '.' && path[index + 2] == '.')
                {
                    // is "/.."; remove last segment,
                    // append "/" and return
                    int index2 = builder.Length - 1;
                    string builderString = builder.ToString();
                    while (index2 >= 0)
                    {
                        if (builderString[index2] == '/')
                        {
                            break;
                        }
                        --index2;
                    }
                    if (index2 < 0)
                    {
                        index2 = 0;
                    }
                    builder.Length = index2;
                    builder.Append('/');
                    break;
                }
                if (index + 4 <= len && c == '/' && path[index + 1] == '.' &&
                    path[index + 2] == '.' && path[index + 3] == '/')
                {
                    // begins with "/../"; remove last segment
                    int index2 = builder.Length - 1;
                    string builderString = builder.ToString();
                    while (index2 >= 0)
                    {
                        if (builderString[index2] == '/')
                        {
                            break;
                        }
                        --index2;
                    }
                    if (index2 < 0)
                    {
                        index2 = 0;
                    }
                    builder.Length = index2;
                    index += 3;
                    continue;
                }
                builder.Append(c);
                ++index;
                while (index < len)
                {
                    // Move the rest of the
                    // path segment until the next '/'
                    c = path[index];
                    if (c == '/')
                    {
                        break;
                    }
                    builder.Append(c);
                    ++index;
                }
            }
            return builder.ToString();
        }

        private static int ParseIPLiteral(string s, int offset, int endOffset)
        {
            int index = offset;
            if (offset == endOffset)
            {
                return -1;
            }
            // Assumes that the character before offset
            // is a '['
            if (s[index] == 'v')
            {
                // IPvFuture
                ++index;
                var hex = false;
                while (index < endOffset)
                {
                    char c = s[index];
                    if (IsHexChar(c))
                    {
                        hex = true;
                    }
                    else
                    {
                        break;
                    }
                    ++index;
                }
                if (!hex)
                {
                    return -1;
                }
                if (index >= endOffset || s[index] != '.')
                {
                    return -1;
                }
                ++index;
                hex = false;
                while (index < endOffset)
                {
                    char c = s[index];
                    if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                        (c >= '0' && c <= '9') || ((c & 0x7F) == c &&
                        ":-._~!$&'()*+,;=".IndexOf(c) >= 0))
                    {
                        hex = true;
                    }
                    else
                    {
                        break;
                    }
                    ++index;
                }
                if (!hex)
                {
                    return -1;
                }
                if (index >= endOffset || s[index] != ']')
                {
                    return -1;
                }
                ++index;
                return index;
            }
            if (s[index] == ':' ||
                IsHexChar(s[index]))
            {
                int startIndex = index;
                while (index < endOffset && ((s[index] >= 65 && s[index] <= 70) ||
                    (s[index] >= 97 && s[index] <= 102) || (s[index] >= 48 && s[index]
                        <= 58) || (s[index] == 46)))
                {
                    ++index;
                }
                if (index >= endOffset || (s[index] != ']' && s[index] != '%'))
                {
                    return -1;
                }
                // NOTE: Array is initialized to zeros
                var addressParts = new int[8];
                int ipEndIndex = index;
                var doubleColon = false;
                var doubleColonPos = 0;
                var totalParts = 0;
                var ipv4part = false;
                index = startIndex;
                // DebugUtility.Log(s.Substring(startIndex, ipEndIndex-startIndex));
                for (var part = 0; part < 8; ++part)
                {
                    if (!doubleColon &&
                        ipEndIndex - index > 1 && s[index] == ':' &&
                        s[index + 1] == ':')
                    {
                        doubleColon = true;
                        doubleColonPos = part;
                        index += 2;
                        if (index == ipEndIndex)
                        {
                            break;
                        }
                    }
                    var hex = 0;
                    var haveHex = false;
                    int curindex = index;
                    for (var i = 0; i < 4; ++i)
                    {
                        if (IsHexChar(s[index]))
                        {
                            hex = (hex << 4) | ToHex(s[index]);
                            haveHex = true;
                            ++index;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (!haveHex)
                    {
                        return -1;
                    }
                    if (index < ipEndIndex && s[index] == '.' && part < 7)
                    {
                        ipv4part = true;
                        index = curindex;
                        break;
                    }
                    addressParts[part] = hex;
                    ++totalParts;
                    if (index < ipEndIndex && s[index] != ':')
                    {
                        return -1;
                    }
                    if (index == ipEndIndex && doubleColon)
                    {
                        break;
                    }
                    // Skip single colon, but not double colon
                    if (index < ipEndIndex && (index + 1 >= ipEndIndex ||
                        s[index + 1] != ':'))
                    {
                        ++index;
                    }
                }
                if (index != ipEndIndex && !ipv4part)
                {
                    return -1;
                }
                if (doubleColon || ipv4part)
                {
                    if (ipv4part)
                    {
                        var ipparts = new int[4];
                        for (var part = 0; part < 4; ++part)
                        {
                            if (part > 0)
                            {
                                if (index < ipEndIndex && s[index] == '.')
                                {
                                    ++index;
                                }
                                else
                                {
                                    return -1;
                                }
                            }
                            if (index + 1 < ipEndIndex && s[index] == '0' &&
                                (s[index + 1] >= '0' && s[index + 1] <= '9'))
                            {
                                return -1;
                            }
                            var dec = 0;
                            var haveDec = false;
                            int curindex = index;
                            for (var i = 0; i < 4; ++i)
                            {
                                if (s[index] >= '0' && s[index] <= '9')
                                {
                                    dec = (dec * 10) + ((int)s[index] - '0');
                                    haveDec = true;
                                    ++index;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            if (!haveDec || dec > 255)
                            {
                                return -1;
                            }
                            ipparts[part] = dec;
                        }
                        if (index != ipEndIndex)
                        {
                            return -1;
                        }
                        addressParts[totalParts] = (ipparts[0] << 8) | ipparts[1];
                        addressParts[totalParts + 1] = (ipparts[2] << 8) | ipparts[3];
                        totalParts += 2;
                        if (!doubleColon && totalParts != 8)
                        {
                            return -1;
                        }
                    }
                    if (doubleColon)
                    {
                        int resid = 8 - totalParts;
                        if (resid == 0)
                        {
                            // Purported IPv6 address contains
                            // 8 parts and a double colon
                            return -1;
                        }
                        var newAddressParts = new int[8];
                        Array.Copy(addressParts, newAddressParts, doubleColonPos);
                        Array.Copy(
                            addressParts,
                            doubleColonPos,
                            newAddressParts,
                            doubleColonPos + resid,
                            totalParts - doubleColonPos);
                        Array.Copy(newAddressParts, addressParts, 8);
                    }
                }
                else if (totalParts != 8)
                {
                    return -1;
                }

                // DebugUtility.Log("{0:X4}:{0:X4}:{0:X4}:{0:X4}:{0:X4}:" +
                // "{0:X4}:{0:X4}:{0:X4}"
                // ,
                // addressParts[0], addressParts[1], addressParts[2],
                // addressParts[3], addressParts[4], addressParts[5],
                // addressParts[6], addressParts[7]);
                if (s[index] == '%')
                {
                    if (index + 2 < endOffset && s[index + 1] == '2' &&
                        s[index + 2] == '5' && (addressParts[0] & 0xFFC0) == 0xFE80)
                    {
                        // Zone identifier in an IPv6 address
                        // (see RFC6874)
                        // NOTE: Allowed only if address has prefix fe80::/10
                        index += 3;
                        var haveChar = false;
                        while (index < endOffset)
                        {
                            char c = s[index];
                            if (c == ']')
                            {
                                return haveChar ? index + 1 : -1;
                            }
                            if (c == '%')
                            {
                                if (index + 2 < endOffset && IsHexChar(s[index + 1]) &&
                                    IsHexChar(s[index + 2]))
                                {
                                    index += 3;
                                    haveChar = true;
                                    continue;
                                }
                                return -1;
                            }
                            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                                (c >= '0' && c <= '9') || c == '.' || c == '_' || c == '-' ||
                                c == '~')
                            {
                                // unreserved character under RFC3986
                                ++index;
                                haveChar = true;
                                continue;
                            }
                            return -1;
                        }
                        return -1;
                    }
                    return -1;
                }
                ++index;
                return index;
            }
            return -1;
        }

        private static string PathParent(
            string refValue,
            int startIndex,
            int endIndex)
        {
            if (startIndex > endIndex)
            {
                return String.Empty;
            }
            --endIndex;
            while (endIndex >= startIndex)
            {
                if (refValue[endIndex] == '/')
                {
                    return refValue.Substring(startIndex, (endIndex + 1) - startIndex);
                }
                --endIndex;
            }
            return String.Empty;
        }

        private static void PercentEncode(StringBuilder buffer, int b)
        {
            buffer.Append('%');
            buffer.Append(HexChars[(b >> 4) & 0x0f]);
            buffer.Append(HexChars[b & 0x0f]);
        }

        private static void PercentEncodeUtf8(StringBuilder buffer, int cp)
        {
            if (cp <= 0x7f)
            {
                buffer.Append('%');
                buffer.Append(HexChars[(cp >> 4) & 0x0f]);
                buffer.Append(HexChars[cp & 0x0f]);
            }
            else if (cp <= 0x7ff)
            {
                PercentEncode(buffer, 0xc0 | ((cp >> 6) & 0x1f));
                PercentEncode(buffer, 0x80 | (cp & 0x3f));
            }
            else if (cp <= 0xffff)
            {
                PercentEncode(buffer, 0xe0 | ((cp >> 12) & 0x0f));
                PercentEncode(buffer, 0x80 | ((cp >> 6) & 0x3f));
                PercentEncode(buffer, 0x80 | (cp & 0x3f));
            }
            else
            {
                PercentEncode(buffer, 0xf0 | ((cp >> 18) & 0x07));
                PercentEncode(buffer, 0x80 | ((cp >> 12) & 0x3f));
                PercentEncode(buffer, 0x80 | ((cp >> 6) & 0x3f));
                PercentEncode(buffer, 0x80 | (cp & 0x3f));
            }
        }

        /// <summary>Resolves a URI or IRI relative to another URI or
        /// IRI.</summary>
        /// <param name='refValue'>A string representing a URI or IRI
        /// reference. Example: <c>dir/file.txt</c>.</param>
        /// <param name='absoluteBase'>A string representing an absolute URI or
        /// IRI reference. Can be null. Example:
        /// <c>http://example.com/my/path/</c>.</param>
        /// <returns>The resolved IRI, or null if <paramref name='refValue'/>
        /// is null or is not a valid IRI. If <paramref name='absoluteBase'/>
        /// is null or is not a valid IRI, returns refValue. Example:
        /// <c>http://example.com/my/path/dir/file.txt</c>.</returns>
        public static string RelativeResolve(string refValue,
            string absoluteBase)
        {
            return RelativeResolve(
                refValue,
                absoluteBase,
                URIUtility.ParseMode.IRIStrict);
        }

        /// <summary>Resolves a URI or IRI relative to another URI or
        /// IRI.</summary>
        /// <param name='refValue'>A string representing a URI or IRI
        /// reference. Example: <c>dir/file.txt</c>. Can be null.</param>
        /// <param name='absoluteBase'>A string representing an absolute URI or
        /// IRI reference. Can be null. Example:
        /// <c>http://example.com/my/path/</c>.</param>
        /// <param name='parseMode'>Parse mode that specifies whether certain
        /// characters are allowed when parsing IRIs and URIs.</param>
        /// <returns>The resolved IRI, or null if <paramref name='refValue'/>
        /// is null or is not a valid IRI. If <paramref name='absoluteBase'/>
        /// is null or is not a valid IRI, returns refValue.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='refValue'/> or <paramref name='absoluteBase'/> or <paramref
        /// name='refValue'/> or <paramref name='refValue'/> is
        /// null.</exception>
        public static string RelativeResolve(
            string refValue,
            string absoluteBase,
            URIUtility.ParseMode parseMode)
        {
            int[] segments = (refValue == null) ? null : SplitIRI(
                refValue,
                0,
                refValue.Length,
                parseMode);
            if (segments == null)
            {
                return null;
            }
            int[] segmentsBase = (
                absoluteBase == null) ? null : SplitIRI(
                absoluteBase,
                0,
                absoluteBase.Length,
                parseMode);
            if (segmentsBase == null)
            {
                return refValue;
            }
            var builder = new StringBuilder();
            if (segments[0] >= 0)
            { // scheme present
                if (refValue == null)
                {
                    throw new InvalidOperationException();
                }
                AppendScheme(builder, refValue, segments);
                AppendAuthority(builder, refValue, segments);
                AppendNormalizedPath(builder, refValue, segments);
                AppendQuery(builder, refValue, segments);
                AppendFragment(builder, refValue, segments);
            }
            else if (segments[2] >= 0)
            { // authority present
                if (absoluteBase == null)
                {
                    throw new InvalidOperationException();
                }
                AppendScheme(builder, absoluteBase, segmentsBase);
                if (refValue == null)
                {
                    throw new ArgumentNullException(nameof(refValue));
                }
                AppendAuthority(builder, refValue, segments);
                AppendNormalizedPath(builder, refValue, segments);
                AppendQuery(builder, refValue, segments);
                AppendFragment(builder, refValue, segments);
            }
            else if (segments[4] == segments[5])
            {
                if (absoluteBase == null)
                {
                    throw new ArgumentNullException(nameof(absoluteBase));
                }
                AppendScheme(builder, absoluteBase, segmentsBase);
                AppendAuthority(builder, absoluteBase, segmentsBase);
                AppendPath(builder, absoluteBase, segmentsBase);
                if (segments[6] >= 0)
                {
                    if (refValue == null)
                    {
                        throw new ArgumentNullException(nameof(refValue));
                    }
                    AppendQuery(builder, refValue, segments);
                }
                else
                {
                    AppendQuery(builder, absoluteBase, segmentsBase);
                }
                if (refValue == null)
                {
                    throw new ArgumentNullException(nameof(refValue));
                }
                AppendFragment(builder, refValue, segments);
            }
            else
            {
                if (absoluteBase == null)
                {
                    throw new InvalidOperationException();
                }
                AppendScheme(builder, absoluteBase, segmentsBase);
                AppendAuthority(builder, absoluteBase, segmentsBase);
                if (refValue == null)
                {
                    throw new InvalidOperationException();
                }
                if (segments[4] < segments[5] && refValue[segments[4]] == '/')
                {
                    AppendNormalizedPath(builder, refValue, segments);
                }
                else
                {
                    var merged = new StringBuilder();
                    if (segmentsBase[2] >= 0 && segmentsBase[4] == segmentsBase[5])
                    {
                        merged.Append('/');
                        if (absoluteBase == null)
                        {
                            throw new InvalidOperationException();
                        }
                        AppendPath(merged, refValue, segments);
                        builder.Append(NormalizePath(merged.ToString()));
                    }
                    else
                    {
                        merged.Append(
                            PathParent(
                            absoluteBase,
                            segmentsBase[4],
                            segmentsBase[5]));
                        AppendPath(merged, refValue, segments);
                        builder.Append(NormalizePath(merged.ToString()));
                    }
                }
                AppendQuery(builder, refValue, segments);
                AppendFragment(builder, refValue, segments);
            }
            return builder.ToString();
        }

        private static string ToLowerCaseAscii(string str)
        {
            if (str == null)
            {
                return null;
            }
            int len = str.Length;
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

        /// <summary>Parses an Internationalized Resource Identifier (IRI)
        /// reference under RFC3987. If the IRI reference is syntactically
        /// valid, splits the string into its components and returns an array
        /// containing those components.</summary>
        /// <param name='s'>A string that contains an IRI. Can be null.</param>
        /// <returns>If the string is a valid IRI reference, returns an array
        /// of five strings. Each of the five pairs corresponds to the IRI's
        /// scheme, authority, path, query, or fragment identifier,
        /// respectively. If a component is absent, the corresponding element
        /// will be null. If the string is null or is not a valid IRI, returns
        /// null.</returns>
        public static string[] SplitIRIToStrings(string s)
        {
            int[] indexes = SplitIRI(s);
            if (indexes == null)
            {
                return null;
            }
            if (s == null)
            {
                // Should not happen because indexes would be null
                // if s is null
                throw new InvalidOperationException();
            }
            string s1 = indexes[0] < 0 ? null : s.Substring(
                indexes[0],
                indexes[1] - indexes[0]);
            string s2 = indexes[2] < 0 ? null : s.Substring(
                indexes[2],
                indexes[3] - indexes[2]);
            string s3 = indexes[4] < 0 ? null : s.Substring(
                indexes[4],
                indexes[5] - indexes[4]);
            string s4 = indexes[6] < 0 ? null : s.Substring(
                indexes[6],
                indexes[7] - indexes[6]);
            string s5 = indexes[8] < 0 ? null : s.Substring(
                indexes[8],
                indexes[9] - indexes[8]);
            return new string[] {
    s1 == null ? null : ToLowerCaseAscii(s1),
    s2, s3, s4, s5,
    };
        }

        /// <summary>Parses an Internationalized Resource Identifier (IRI)
        /// reference under RFC3987. If the IRI reference is syntactically
        /// valid, splits the string into its components and returns an array
        /// containing the indices into the components.</summary>
        /// <param name='s'>A string that contains an IRI. Can be null.</param>
        /// <returns>If the string is a valid IRI reference, returns an array
        /// of 10 integers. Each of the five pairs corresponds to the start and
        /// end index of the IRI's scheme, authority, path, query, or fragment
        /// identifier, respectively. The scheme, authority, query, and
        /// fragment identifier, if present, will each be given without the
        /// ending colon, the starting "//", the starting "?", and the starting
        /// "#", respectively. If a component is absent, both indices in that
        /// pair will be -1. If the string is null or is not a valid IRI,
        /// returns null.</returns>
        public static int[] SplitIRI(string s)
        {
            return (s == null) ? null : SplitIRI(
                s,
                0,
                s.Length,
                URIUtility.ParseMode.IRIStrict);
        }

        /// <summary>Parses a substring that represents an Internationalized
        /// Resource Identifier (IRI) under RFC3987. If the IRI is
        /// syntactically valid, splits the string into its components and
        /// returns an array containing the indices into the
        /// components.</summary>
        /// <param name='s'>A string that contains an IRI. Can be null.</param>
        /// <param name='offset'>An index starting at 0 showing where the
        /// desired portion of "s" begins.</param>
        /// <param name='length'>The length of the desired portion of "s" (but
        /// not more than "s" 's length).</param>
        /// <param name='parseMode'>Parse mode that specifies whether certain
        /// characters are allowed when parsing IRIs and URIs.</param>
        /// <returns>If the string is a valid IRI, returns an array of 10
        /// integers. Each of the five pairs corresponds to the start and end
        /// index of the IRI's scheme, authority, path, query, or fragment
        /// component, respectively. The scheme, authority, query, and fragment
        /// components, if present, will each be given without the ending
        /// colon, the starting "//", the starting "?", and the starting "#",
        /// respectively. If a component is absent, both indices in that pair
        /// will be -1 (an index won't be less than 0 in any other case). If
        /// the string is null or is not a valid IRI, returns null.</returns>
        /// <exception cref='ArgumentException'>Either <paramref
        /// name='offset'/> or <paramref name='length'/> is less than 0 or
        /// greater than <paramref name='s'/> 's length, or <paramref
        /// name='s'/> 's length minus <paramref name='offset'/> is less than
        /// <paramref name='length'/>.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='s'/> is null.</exception>
        public static int[] SplitIRI(
            string s,
            int offset,
            int length,
            URIUtility.ParseMode parseMode)
        {
            if (s == null)
            {
                return null;
            }
            if (offset < 0)
            {
                throw new ArgumentException("offset(" + offset +
                    ") is less than 0");
            }
            if (offset > s.Length)
            {
                throw new ArgumentException("offset(" + offset +
                    ") is more than " + s.Length);
            }
            if (length < 0)
            {
                throw new ArgumentException("length(" + length +
                    ") is less than 0");
            }
            if (length > s.Length)
            {
                throw new ArgumentException("length(" + length +
                    ") is more than " + s.Length);
            }
            if (s.Length - offset < length)
            {
                throw new ArgumentException("s's length minus " + offset + "(" +
                    (s.Length - offset) + ") is less than " + length);
            }
            int[] retval = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
            if (length == 0)
            {
                retval[4] = 0;
                retval[5] = 0;
                return retval;
            }
            bool asciiOnly = parseMode == URIUtility.ParseMode.URILenient ||
                parseMode == URIUtility.ParseMode.URIStrict;
            bool strict = parseMode == URIUtility.ParseMode.URIStrict || parseMode ==
                URIUtility.ParseMode.IRIStrict;
            int index = offset;
            int valueSLength = offset + length;
            var scheme = false;
            // scheme
            while (index < valueSLength)
            {
                int c = s[index];
                if (index > offset && c == ':')
                {
                    scheme = true;
                    retval[0] = offset;
                    retval[1] = index;
                    ++index;
                    break;
                }
                if (strict && index == offset && !((c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z')))
                {
                    break;
                }
                if (strict && index > offset &&
                    !((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' &&
                        c <= '9') || c == '+' || c == '-' || c == '.'))
                {
                    break;
                }
                if (!strict && (c == '#' || c == ':' || c == '?' || c == '/'))
                {
                    break;
                }
                ++index;
            }
            if (!scheme)
            {
                index = offset;
            }
            var state = 0;
            if (index + 2 <= valueSLength && s[index] == '/' && s[index + 1] == '/')
            {
                // authority
                // (index + 2, valueSLength)
                index += 2;
                int authorityStart = index;
                retval[2] = authorityStart;
                retval[3] = valueSLength;
                state = 0; // userinfo
                            // Check for userinfo
                while (index < valueSLength)
                {
                    int c = s[index];
                    if (asciiOnly && c >= 0x80)
                    {
                        return null;
                    }
                    if ((c & 0xfc00) == 0xd800 && index + 1 < valueSLength &&
                        (s[index + 1] & 0xfc00) == 0xdc00)
                    {
                        // Get the Unicode code point for the surrogate pair
                        c = 0x10000 + ((c & 0x3ff) << 10) + (s[index + 1] & 0x3ff);
                        ++index;
                    }
                    else if ((c & 0xf800) == 0xd800)
                    {
                        if (parseMode == URIUtility.ParseMode.IRISurrogateLenient)
                        {
                            c = 0xfffd;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    if (c == '%' && (state == 0 || state == 1) && strict)
                    {
                        // Percent encoded character (except in port)
                        if (index + 2 < valueSLength && IsHexChar(s[index + 1]) &&
                            IsHexChar(s[index + 2]))
                        {
                            index += 3;
                            continue;
                        }
                        return null;
                    }
                    if (state == 0)
                    { // User info
                        if (c == '/' || c == '?' || c == '#')
                        {
                            // not user info
                            state = 1;
                            index = authorityStart;
                            continue;
                        }
                        if (strict && c == '@')
                        {
                            // is user info
                            ++index;
                            state = 1;
                            continue;
                        }
                        if (strict && IsIUserInfoChar(c))
                        {
                            ++index;
                            if (index == valueSLength)
                            {
                                // not user info
                                state = 1;
                                index = authorityStart;
                                continue;
                            }
                        }
                        else
                        {
                            // not user info
                            state = 1;
                            index = authorityStart;
                            continue;
                        }
                    }
                    else if (state == 1)
                    { // host
                        if (c == '/' || c == '?' || c == '#')
                        {
                            // end of authority
                            retval[3] = index;
                            break;
                        }
                        if (!strict)
                        {
                            ++index;
                        }
                        else if (c == '[')
                        {
                            ++index;
                            index = ParseIPLiteral(s, index, valueSLength);
                            if (index < 0)
                            {
                                return null;
                            }
                            continue;
                        }
                        else if (c == ':')
                        {
                            // port
                            state = 2;
                            ++index;
                        }
                        else if (IsIRegNameChar(c))
                        {
                            // is valid host name char
                            // (note: IPv4 addresses included
                            // in ireg-name)
                            ++index;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else if (state == 2)
                    { // Port
                        if (c == '/' || c == '?' || c == '#')
                        {
                            // end of authority
                            retval[3] = index;
                            break;
                        }
                        if (c >= '0' && c <= '9')
                        {
                            ++index;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            var colon = false;
            var segment = false;
            bool fullyRelative = index == offset;
            retval[4] = index; // path offsets
            retval[5] = valueSLength;
            state = 0; // IRI Path
            while (index < valueSLength)
            {
                // Get the next Unicode character
                int c = s[index];
                if (asciiOnly && c >= 0x80)
                {
                    return null;
                }
                if ((c & 0xfc00) == 0xd800 && index + 1 < valueSLength &&
                    (s[index + 1] & 0xfc00) == 0xdc00)
                {
                    // Get the Unicode code point for the surrogate pair
                    c = 0x10000 + ((c & 0x3ff) << 10) + (s[index + 1] & 0x3ff);
                    ++index;
                }
                else if ((c & 0xf800) == 0xd800)
                {
                    // error
                    return null;
                }
                if (c == '%' && strict)
                {
                    // Percent encoded character
                    if (index + 2 < valueSLength && IsHexChar(s[index + 1]) &&
                        IsHexChar(s[index + 2]))
                    {
                        index += 3;
                        continue;
                    }
                    return null;
                }
                if (state == 0)
                { // Path
                    if (c == ':' && fullyRelative)
                    {
                        colon = true;
                    }
                    else if (c == '/' && fullyRelative && !segment)
                    {
                        // noscheme path can't have colon before slash
                        if (strict && colon)
                        {
                            return null;
                        }
                        segment = true;
                    }
                    if (c == '?')
                    {
                        retval[5] = index;
                        retval[6] = index + 1;
                        retval[7] = valueSLength;
                        state = 1; // move to query state
                    }
                    else if (c == '#')
                    {
                        retval[5] = index;
                        retval[8] = index + 1;
                        retval[9] = valueSLength;
                        state = 2; // move to fragment state
                    }
                    else if (strict && !IsIpchar(c))
                    {
                        return null;
                    }
                    ++index;
                }
                else if (state == 1)
                { // Query
                    if (c == '#')
                    {
                        retval[7] = index;
                        retval[8] = index + 1;
                        retval[9] = valueSLength;
                        state = 2; // move to fragment state
                    }
                    else if (strict && !IsIqueryChar(c))
                    {
                        return null;
                    }
                    ++index;
                }
                else if (state == 2)
                { // Fragment
                    if (strict && !IsIfragmentChar(c))
                    {
                        return null;
                    }
                    ++index;
                }
            }
            if (strict && fullyRelative && colon && !segment)
            {
                return null; // ex. "x@y:z"
            }
            return retval;
        }

        /// <summary>Parses an Internationalized Resource Identifier (IRI)
        /// reference under RFC3987. If the IRI is syntactically valid, splits
        /// the string into its components and returns an array containing the
        /// indices into the components.</summary>
        /// <param name='s'>A string representing an IRI. Can be null.</param>
        /// <param name='parseMode'>The parameter <paramref name='parseMode'/>
        /// is a ParseMode object.</param>
        /// <returns>If the string is a valid IRI reference, returns an array
        /// of 10 integers. Each of the five pairs corresponds to the start and
        /// end index of the IRI's scheme, authority, path, query, or fragment
        /// identifier, respectively. The scheme, authority, query, and
        /// fragment identifier, if present, will each be given without the
        /// ending colon, the starting "//", the starting "?", and the starting
        /// "#", respectively. If a component is absent, both indices in that
        /// pair will be -1. If the string is null or is not a valid IRI,
        /// returns null.</returns>
        public static int[] SplitIRI(string s, URIUtility.ParseMode parseMode)
        {
            return (s == null) ? null : SplitIRI(s, 0, s.Length, parseMode);
        }

        private static bool PathHasDotComponent(string path)
        {
            if (path == null || path.Length == 0)
            {
                return false;
            }
            path = PercentDecode(path);
            if (path.Equals("..", StringComparison.Ordinal))
            {
                return true;
            }
            if (path.Equals(".", StringComparison.Ordinal))
            {
                return true;
            }
            if (path.IndexOf(ValueSlashDot, StringComparison.Ordinal) < 0 &&
                path.IndexOf(
                ValueDotSlash,
                StringComparison.Ordinal) < 0)
            {
                return false;
            }
            var index = 0;
            var len = path.Length;
            while (index < len)
            {
                char c = path[index];
                if ((index + 3 <= len && c == '/' && path[index + 1] == '.' &&
                    path[index + 2] == '/') || (index + 2 == len && c == '.' &&
                    path[index + 1] == '.'))
                {
                    // begins with "/./" or is "..";
                    return true;
                }
                if (index + 3 <= len && c == '.' &&
                    path[index + 1] == '.' && path[index + 2] == '/')
                {
                    // begins with "../";
                    return true;
                }
                if ((index + 2 <= len && c == '.' &&
                    path[index + 1] == '/') || (index + 1 == len && c == '.'))
                {
                    // begins with "./" or is ".";
                    return true;
                }
                if (index + 2 == len && c == '/' && path[index + 1] == '.')
                {
                    // is "/."
                    return true;
                }
                if (index + 3 == len && c == '/' &&
                    path[index + 1] == '.' && path[index + 2] == '.')
                {
                    // is "/.."
                    return true;
                }
                if (index + 4 <= len && c == '/' && path[index + 1] == '.' &&
                    path[index + 2] == '.' && path[index + 3] == '/')
                {
                    // begins with "/../"
                    return true;
                }
                ++index;
                while (index < len)
                {
                    // Move the rest of the
                    // path segment until the next '/'
                    c = path[index];
                    if (c == '/')
                    {
                        break;
                    }
                    ++index;
                }
            }
            return false;
        }

        private static string UriPath(string uri, URIUtility.ParseMode parseMode)
        {
            int[] indexes = SplitIRI(uri, parseMode);
            return (
                indexes == null) ? null : uri.Substring(
                indexes[4],
                indexes[5] - indexes[4]);
        }

        /// <summary>Extracts the scheme, the authority, and the path component
        /// (up to and including the last "/" in the path if any) from the
        /// given URI or IRI, using the IRIStrict parse mode to check the URI
        /// or IRI. Any "./" or "../" in the path is not condensed.</summary>
        /// <param name='uref'>A text string representing a URI or IRI. Can be
        /// null.</param>
        /// <returns>The directory path of the URI or IRI. Returns null if
        /// <paramref name='uref'/> is null or not a valid URI or
        /// IRI.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='uref'/> is null.</exception>
        public static string DirectoryPath(string uref)
        {
            return DirectoryPath(uref, URIUtility.ParseMode.IRIStrict);
        }

        /// <summary>Extracts the scheme, the authority, and the path component
        /// (up to and including the last "/" in the path if any) from the
        /// given URI or IRI, using the given parse mode to check the URI or
        /// IRI. Any "./" or "../" in the path is not condensed.</summary>
        /// <param name='uref'>A text string representing a URI or IRI. Can be
        /// null.</param>
        /// <param name='parseMode'>The parse mode to use to check the URI or
        /// IRI.</param>
        /// <returns>The directory path of the URI or IRI. Returns null if
        /// <paramref name='uref'/> is null or not a valid URI or
        /// IRI.</returns>
        public static string DirectoryPath(string uref, URIUtility.ParseMode
            parseMode)
        {
            int[] indexes = SplitIRI(uref, parseMode);
            if (indexes == null)
            {
                return null;
            }
            if (uref == null)
            {
                throw new InvalidOperationException();
            }
            string schemeAndAuthority = uref.Substring(0, indexes[4]);
            string path = uref.Substring(indexes[4], indexes[5] - indexes[4]);
            if (path.Length > 0)
            {
                for (int i = path.Length - 1; i >= 0; --i)
                {
                    if (path[i] == '/')
                    {
                        return schemeAndAuthority + path.Substring(0, i + 1);
                    }
                }
                return schemeAndAuthority + path;
            }
            else
            {
                return schemeAndAuthority;
            }
        }

        /// <summary>Resolves a URI or IRI relative to another URI or IRI, but
        /// only if the resolved URI has no "." or ".." component in its path
        /// and only if resolved URI's directory path matches that of the
        /// second URI or IRI.</summary>
        /// <param name='refValue'>A string representing a URI or IRI
        /// reference. Example: <c>dir/file.txt</c>.</param>
        /// <param name='absoluteBase'>A string representing an absolute URI
        /// reference. Example: <c>http://example.com/my/path/</c>.</param>
        /// <returns>The resolved IRI, or null if <paramref name='refValue'/>
        /// is null or is not a valid IRI, or <paramref name='refValue'/> if
        /// <paramref name='absoluteBase'/> is null or an empty string, or null
        /// if <paramref name='absoluteBase'/> is neither null nor empty and is
        /// not a valid IRI. Returns null instead if the resolved IRI has no
        /// "." or ".." component in its path or if the resolved URI's
        /// directory path does not match that of <paramref
        /// name='absoluteBase'/>. Example:
        /// <c>http://example.com/my/path/dir/file.txt</c>.</returns>
        public static string RelativeResolveWithinBaseURI(
            string refValue,
            string absoluteBase)
        {
            if (!String.IsNullOrEmpty(absoluteBase) &&
                SplitIRI(absoluteBase, URIUtility.ParseMode.IRIStrict) == null)
            {
                return null;
            }
            string rel = RelativeResolve(refValue, absoluteBase);
            if (rel == null)
            {
                return null;
            }
            if (refValue == null)
            {
                throw new InvalidOperationException();
            }
            string relpath = UriPath(refValue, URIUtility.ParseMode.IRIStrict);
            if (PathHasDotComponent(relpath))
            {
                // Resolved path has a dot component in it (usually
                // because that component is percent-encoded)
                return null;
            }
            string absuri = DirectoryPath(absoluteBase);
            string reluri = DirectoryPath(rel);
            return (absuri == null || reluri == null ||
                !absuri.Equals(reluri, StringComparison.Ordinal)) ? null : rel;
        }
    }
}