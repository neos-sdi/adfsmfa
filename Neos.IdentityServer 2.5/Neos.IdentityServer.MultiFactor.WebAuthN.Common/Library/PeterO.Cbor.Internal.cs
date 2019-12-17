//******************************************************************************************************************************************************************************************//
// Copyright (c) 2019 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
using Neos.IdentityServer.MultiFactor.WebAuthN.Library.Chaos;


#pragma warning disable 618

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor
{
    #region Base64
    internal static class Base64
    {
        private const string Base64URL =
          "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

        private const string Base64Classic =
          "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

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
                throw new ArgumentException("offset (" + offset + ") is less than " +
                            "0 ");
            }
            if (offset > data.Length)
            {
                throw new ArgumentException("offset (" + offset + ") is more than " +
                            data.Length);
            }
            if (count < 0)
            {
                throw new ArgumentException("count (" + count + ") is less than " +
                            "0 ");
            }
            if (count > data.Length)
            {
                throw new ArgumentException("count (" + count + ") is more than " +
                            data.Length);
            }
            if (data.Length - offset < count)
            {
                throw new ArgumentException("data's length minus " + offset + " (" +
                        (data.Length - offset) + ") is less than " + count);
            }
            string alphabet = classic ? Base64Classic : Base64URL;
            int length = offset + count;
            int i = offset;
            char[] buffer = new char[4];
            for (i = offset; i < (length - 2); i += 3)
            {
                buffer[0] = (char)alphabet[(data[i] >> 2) & 63];
                buffer[1] = (char)alphabet[((data[i] & 3) << 4) +
                        ((data[i + 1] >> 4) & 15)];
                buffer[2] = (char)alphabet[((data[i + 1] & 15) << 2) + ((data[i +
                        2] >> 6) & 3)];
                buffer[3] = (char)alphabet[data[i + 2] & 63];
                writer.WriteCodePoint((int)buffer[0]);
                writer.WriteCodePoint((int)buffer[1]);
                writer.WriteCodePoint((int)buffer[2]);
                writer.WriteCodePoint((int)buffer[3]);
            }
            int lenmod3 = count % 3;
            if (lenmod3 != 0)
            {
                i = length - lenmod3;
                buffer[0] = (char)alphabet[(data[i] >> 2) & 63];
                if (lenmod3 == 2)
                {
                    buffer[1] = (char)alphabet[((data[i] & 3) << 4) + ((data[i + 1] >>
                          4) & 15)];
                    buffer[2] = (char)alphabet[(data[i + 1] & 15) << 2];
                    writer.WriteCodePoint((int)buffer[0]);
                    writer.WriteCodePoint((int)buffer[1]);
                    writer.WriteCodePoint((int)buffer[2]);
                    if (padding)
                    {
                        writer.WriteCodePoint((int)'=');
                    }
                }
                else
                {
                    buffer[1] = (char)alphabet[(data[i] & 3) << 4];
                    writer.WriteCodePoint((int)buffer[0]);
                    writer.WriteCodePoint((int)buffer[1]);
                    if (padding)
                    {
                        writer.WriteCodePoint((int)'=');
                        writer.WriteCodePoint((int)'=');
                    }
                }
            }
        }
    }
    #endregion

    #region CBORCanonical
    internal static class CBORCanonical
    {
        private sealed class CtapComparer : IComparer<CBORObject>
        {
            public int Compare(CBORObject a, CBORObject b)
            {
                byte[] abs;
                byte[] bbs;
                bool bothBytes = false;
                if (a.Type == CBORType.ByteString && b.Type == CBORType.ByteString)
                {
                    abs = a.GetByteString();
                    bbs = b.GetByteString();
                    bothBytes = true;
                }
                else
                {
                    abs = CtapCanonicalEncode(a);
                    bbs = CtapCanonicalEncode(b);
                }
                if (!bothBytes && (abs[0] & 0xe0) != (bbs[0] & 0xe0))
                {
                    // different major types
                    return (abs[0] & 0xe0) < (bbs[0] & 0xe0) ? -1 : 1;
                }
                if (abs.Length != bbs.Length)
                {
                    // different lengths
                    return abs.Length < bbs.Length ? -1 : 1;
                }
                for (int i = 0; i < abs.Length; ++i)
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

        public static byte[] CtapCanonicalEncode(CBORObject a)
        {
            CBORObject cbor = a.Untag();
            CBORType valueAType = cbor.Type;
            try
            {
                if (valueAType == CBORType.Array)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        CBORObject.WriteValue(ms, 4, cbor.Count);
                        for (int i = 0; i < cbor.Count; ++i)
                        {
                            byte[] bytes = CtapCanonicalEncode(cbor[i]);
                            ms.Write(bytes, 0, bytes.Length);
                        }
                        return ms.ToArray();
                    }
                }
                else if (valueAType == CBORType.Map)
                {
                    List<CBORObject> sortedKeys = new List<CBORObject>();
                    foreach (CBORObject key in cbor.Keys)
                    {
                        sortedKeys.Add(key);
                    }
                    sortedKeys.Sort(new CtapComparer());
                    using (MemoryStream ms = new MemoryStream())
                    {
                        CBORObject.WriteValue(ms, 5, cbor.Count);
                        foreach (CBORObject key in sortedKeys)
                        {
                            byte[] bytes = CtapCanonicalEncode(key);
                            ms.Write(bytes, 0, bytes.Length);
                            bytes = CtapCanonicalEncode(cbor[key]);
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
            else if (valueAType == CBORType.Number)
            {
                if (cbor.CanFitInInt64())
                {
                    return cbor.EncodeToBytes(CBOREncodeOptions.Default);
                }
                else
                {
                    cbor = CBORObject.FromObject(cbor.AsDouble());
                    return cbor.EncodeToBytes(CBOREncodeOptions.Default);
                }
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
        private const int MaxSafeInt = 214748363;

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORDataUtilities.ParseJSONNumber(System.String)"]/*'/>
        public static CBORObject ParseJSONNumber(string str)
        {
            return ParseJSONNumber(str, false, false);
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORDataUtilities.ParseJSONNumber(System.String,System.Boolean,System.Boolean)"]/*'/>
        public static CBORObject ParseJSONNumber(
          string str,
          bool integersOnly,
          bool positiveOnly)
        {
            return ParseJSONNumber(str, integersOnly, positiveOnly, false);
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORDataUtilities.ParseJSONNumber(System.String,System.Boolean,System.Boolean,System.Boolean)"]/*'/>
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
            int offset = 0;
            bool negative = false;
            if (str[0] == '-' && !positiveOnly)
            {
                negative = true;
                ++offset;
            }
            int mantInt = 0;
            FastInteger2 mant = null;
            int mantBuffer = 0;
            int mantBufferMult = 1;
            int expBuffer = 0;
            int expBufferMult = 1;
            bool haveDecimalPoint = false;
            bool haveDigits = false;
            bool haveDigitsAfterDecimal = false;
            bool haveExponent = false;
            int newScaleInt = 0;
            FastInteger2 newScale = null;
            int i = offset;
            // Ordinary number
            if (i < str.Length && str[i] == '0')
            {
                ++i;
                haveDigits = true;
                if (i == str.Length)
                {
                    if (preserveNegativeZero && negative)
                    {
                        return CBORObject.FromObject(
                         EDecimal.NegativeZero);
                    }
                    return CBORObject.FromObject(0);
                }
                if (!integersOnly)
                {
                    if (str[i] == '.')
                    {
                        haveDecimalPoint = true;
                        ++i;
                    }
                    else if (str[i] == 'E' || str[i] == 'e')
                    {
                        haveExponent = true;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            for (; i < str.Length; ++i)
            {
                if (str[i] >= '0' && str[i] <= '9')
                {
                    int thisdigit = (int)(str[i] - '0');
                    if (mantInt > MaxSafeInt)
                    {
                        if (mant == null)
                        {
                            mant = new FastInteger2(mantInt);
                            mantBuffer = thisdigit;
                            mantBufferMult = 10;
                        }
                        else
                        {
                            if (mantBufferMult >= 1000000000)
                            {
                                mant.Multiply(mantBufferMult).AddInt(mantBuffer);
                                mantBuffer = thisdigit;
                                mantBufferMult = 10;
                            }
                            else
                            {
                                mantBufferMult *= 10;
                                mantBuffer = (mantBuffer << 3) + (mantBuffer << 1);
                                mantBuffer += thisdigit;
                            }
                        }
                    }
                    else
                    {
                        mantInt *= 10;
                        mantInt += thisdigit;
                    }
                    haveDigits = true;
                    if (haveDecimalPoint)
                    {
                        haveDigitsAfterDecimal = true;
                        if (newScaleInt == Int32.MinValue)
                        {
                            newScale = newScale ?? (new FastInteger2(newScaleInt));
                            newScale.AddInt(-1);
                        }
                        else
                        {
                            --newScaleInt;
                        }
                    }
                }
                else if (!integersOnly && str[i] == '.')
                {
                    if (!haveDigits)
                    {
                        // no digits before the decimal point
                        return null;
                    }
                    if (haveDecimalPoint)
                    {
                        return null;
                    }
                    haveDecimalPoint = true;
                }
                else if (!integersOnly && (str[i] == 'E' || str[i] == 'e'))
                {
                    haveExponent = true;
                    ++i;
                    break;
                }
                else
                {
                    return null;
                }
            }
            if (!haveDigits || (haveDecimalPoint && !haveDigitsAfterDecimal))
            {
                return null;
            }
            if (mant != null && (mantBufferMult != 1 || mantBuffer != 0))
            {
                mant.Multiply(mantBufferMult).AddInt(mantBuffer);
            }
            if (haveExponent)
            {
                FastInteger2 exp = null;
                int expInt = 0;
                offset = 1;
                haveDigits = false;
                if (i == str.Length)
                {
                    return null;
                }
                if (str[i] == '+' || str[i] == '-')
                {
                    if (str[i] == '-')
                    {
                        offset = -1;
                    }
                    ++i;
                }
                for (; i < str.Length; ++i)
                {
                    if (str[i] >= '0' && str[i] <= '9')
                    {
                        haveDigits = true;
                        int thisdigit = (int)(str[i] - '0');
                        if (expInt > MaxSafeInt)
                        {
                            if (exp == null)
                            {
                                exp = new FastInteger2(expInt);
                                expBuffer = thisdigit;
                                expBufferMult = 10;
                            }
                            else
                            {
                                if (expBufferMult >= 1000000000)
                                {
                                    exp.Multiply(expBufferMult).AddInt(expBuffer);
                                    expBuffer = thisdigit;
                                    expBufferMult = 10;
                                }
                                else
                                {
                                    // multiply expBufferMult and expBuffer each by 10
                                    expBufferMult = (expBufferMult << 3) + (expBufferMult << 1);
                                    expBuffer = (expBuffer << 3) + (expBuffer << 1);
                                    expBuffer += thisdigit;
                                }
                            }
                        }
                        else
                        {
                            expInt *= 10;
                            expInt += thisdigit;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                if (!haveDigits)
                {
                    return null;
                }
                if (exp != null && (expBufferMult != 1 || expBuffer != 0))
                {
                    exp.Multiply(expBufferMult).AddInt(expBuffer);
                }
                if (offset >= 0 && newScaleInt == 0 && newScale == null && exp == null)
                {
                    newScaleInt = expInt;
                }
                else if (exp == null)
                {
                    newScale = newScale ?? (new FastInteger2(newScaleInt));
                    if (offset < 0)
                    {
                        newScale.SubtractInt(expInt);
                    }
                    else if (expInt != 0)
                    {
                        newScale.AddInt(expInt);
                    }
                }
                else
                {
                    newScale = newScale ?? (new FastInteger2(newScaleInt));
                    if (offset < 0)
                    {
                        newScale.Subtract(exp);
                    }
                    else
                    {
                        newScale.Add(exp);
                    }
                }
            }
            if (i != str.Length)
            {
                // End of the string wasn't reached, so isn't a number
                return null;
            }
            if ((newScale == null && newScaleInt == 0) || (newScale != null &&
                          newScale.Sign == 0))
            {
                // No fractional part
                if (mant != null && mant.CanFitInInt32())
                {
                    mantInt = mant.AsInt32();
                    mant = null;
                }
                if (mant == null)
                {
                    // NOTE: mantInt can only be 0 or greater, so overflow is impossible
#if DEBUG
                    if (mantInt < 0)
                    {
                        throw new ArgumentException("mantInt (" + mantInt +
                          ") is less than 0");
                    }
#endif

                    if (negative)
                    {
                        mantInt = -mantInt;
                        if (preserveNegativeZero && mantInt == 0)
                        {
                            return CBORObject.FromObject(
                              EDecimal.NegativeZero);
                        }
                    }
                    return CBORObject.FromObject(mantInt);
                }
                else
                {
                    EInteger bigmant2 = mant.AsBigInteger();
                    if (negative)
                    {
                        bigmant2 = -(EInteger)bigmant2;
                    }
                    return CBORObject.FromObject(bigmant2);
                }
            }
            else
            {
                EInteger bigmant = (mant == null) ? ((EInteger)mantInt) :
                  mant.AsBigInteger();
                EInteger bigexp = (newScale == null) ? ((EInteger)newScaleInt) :
                  newScale.AsBigInteger();
                if (negative)
                {
                    bigmant = -(EInteger)bigmant;
                }
                EDecimal edec;
                edec = EDecimal.Create(
                  bigmant,
                  bigexp);
                if (negative && preserveNegativeZero && bigmant.IsZero)
                {
                    EDecimal negzero = EDecimal.NegativeZero;
                    negzero = negzero.Quantize(bigexp, null);
                    edec = negzero.Subtract(edec);
                }
                return CBORObject.FromObject(edec);
            }
        }
    }
    #endregion

    #region CBORDateConverter
    internal class CBORDateConverter : ICBORToFromConverter<DateTime>
    {
        private static string DateTimeToString(DateTime bi)
        {
            int[] lesserFields = new int[7];
            EInteger[] year = new EInteger[1];
            PropertyMap.BreakDownDateTime(bi, year, lesserFields);
            return CBORUtilities.ToAtomDateTimeString(year[0], lesserFields, true);
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

    #region CBORDouble
    internal class CBORDouble : ICBORNumber
    {
        public bool IsPositiveInfinity(object obj)
        {
            return Double.IsPositiveInfinity((double)obj);
        }

        public bool IsInfinity(object obj)
        {
            return Double.IsInfinity((double)obj);
        }

        public bool IsNegativeInfinity(object obj)
        {
            return Double.IsNegativeInfinity((double)obj);
        }

        public bool IsNaN(object obj)
        {
            return Double.IsNaN((double)obj);
        }

        public double AsDouble(object obj)
        {
            return (double)obj;
        }

        public EDecimal AsExtendedDecimal(object obj)
        {
            return EDecimal.FromDouble((double)obj);
        }

        public EFloat AsExtendedFloat(object obj)
        {
            return EFloat.FromDouble((double)obj);
        }

        public float AsSingle(object obj)
        {
            return (float)(double)obj;
        }

        public EInteger AsEInteger(object obj)
        {
            return CBORUtilities.BigIntegerFromDouble((double)obj);
        }

        public long AsInt64(object obj)
        {
            double fltItem = (double)obj;
            if (Double.IsNaN(fltItem))
            {
                throw new OverflowException("This object's value is out of range");
            }
            fltItem = (fltItem < 0) ? Math.Ceiling(fltItem) : Math.Floor(fltItem);
            if (fltItem >= -9223372036854775808.0 && fltItem <
            9223372036854775808.0)
            {
                return (long)fltItem;
            }
            throw new OverflowException("This object's value is out of range");
        }

        public bool CanFitInSingle(object obj)
        {
            double fltItem = (double)obj;
            if (Double.IsNaN(fltItem))
            {
                return true;
            }
            float sing = (float)fltItem;
            return (double)sing == (double)fltItem;
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

        public bool CanTruncatedIntFitInInt64(object obj)
        {
            double fltItem = (double)obj;
            if (Double.IsNaN(fltItem) || Double.IsInfinity(fltItem))
            {
                return false;
            }
            double fltItem2 = (fltItem < 0) ? Math.Ceiling(fltItem) :
            Math.Floor(fltItem);
            return fltItem2 >= -9223372036854775808.0 && fltItem2 <
            9223372036854775808.0;
        }

        public bool CanTruncatedIntFitInInt32(object obj)
        {
            double fltItem = (double)obj;
            if (Double.IsNaN(fltItem) || Double.IsInfinity(fltItem))
            {
                return false;
            }
            double fltItem2 = (fltItem < 0) ? Math.Ceiling(fltItem) :
            Math.Floor(fltItem);
            return fltItem2 >= Int32.MinValue && fltItem2 <= Int32.MaxValue;
        }

        public int AsInt32(object obj, int minValue, int maxValue)
        {
            double fltItem = (double)obj;
            if (Double.IsNaN(fltItem))
            {
                throw new OverflowException("This object's value is out of range");
            }
            fltItem = (fltItem < 0) ? Math.Ceiling(fltItem) : Math.Floor(fltItem);
            if (fltItem >= minValue && fltItem <= maxValue)
            {
                int ret = (int)fltItem;
                return ret;
            }
            throw new OverflowException("This object's value is out of range");
        }

        public bool IsZero(object obj)
        {
            return (double)obj == 0.0;
        }

        public int Sign(object obj)
        {
            double flt = (double)obj;
            return Double.IsNaN(flt) ? 2 : ((double)flt == 0.0 ? 0 : (flt < 0.0f ?
            -1 : 1));
        }

        public bool IsIntegral(object obj)
        {
            double fltItem = (double)obj;
            if (Double.IsNaN(fltItem) || Double.IsInfinity(fltItem))
            {
                return false;
            }
            double fltItem2 = (fltItem < 0) ? Math.Ceiling(fltItem) :
            Math.Floor(fltItem);
            return fltItem == fltItem2;
        }

        public object Negate(object obj)
        {
            double val = (double)obj;
            return -val;
        }

        public object Abs(object obj)
        {
            double val = (double)obj;
            return (val < 0) ? -val : obj;
        }

        public ERational AsExtendedRational(object obj)
        {
            return ERational.FromDouble((double)obj);
        }

        public bool IsNegative(object obj)
        {
            double dbl = (double)obj;
            long lvalue = BitConverter.ToInt64(
        BitConverter.GetBytes((double)dbl),
        0);
            return (lvalue >> 63) != 0;
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

        public EDecimal AsExtendedDecimal(object obj)
        {
            return EDecimal.FromEInteger((EInteger)obj);
        }

        public EFloat AsExtendedFloat(object obj)
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
            EInteger bi = (EInteger)obj;
            if (bi.CompareTo(CBORObject.Int64MaxValue) > 0 ||
                bi.CompareTo(CBORObject.Int64MinValue) < 0)
            {
                throw new OverflowException("This object's value is out of range");
            }
            return (long)bi;
        }

        public bool CanFitInSingle(object obj)
        {
            EInteger bigintItem = (EInteger)obj;
            EFloat ef = EFloat.FromEInteger(bigintItem);
            EFloat ef2 = EFloat.FromSingle(ef.ToSingle());
            return ef.CompareTo(ef2) == 0;
        }

        public bool CanFitInDouble(object obj)
        {
            EInteger bigintItem = (EInteger)obj;
            EFloat ef = EFloat.FromEInteger(bigintItem);
            EFloat ef2 = EFloat.FromDouble(ef.ToDouble());
            return ef.CompareTo(ef2) == 0;
        }

        public bool CanFitInInt32(object obj)
        {
            EInteger bi = (EInteger)obj;
            return bi.CanFitInInt32();
        }

        public bool CanFitInInt64(object obj)
        {
            EInteger bi = (EInteger)obj;
            return bi.GetSignedBitLength() <= 63;
        }

        public bool CanTruncatedIntFitInInt64(object obj)
        {
            return this.CanFitInInt64(obj);
        }

        public bool CanTruncatedIntFitInInt32(object obj)
        {
            return this.CanFitInInt32(obj);
        }

        public bool IsZero(object obj)
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
            EInteger bi = (EInteger)obj;
            if (bi.CanFitInInt32())
            {
                int ret = (int)bi;
                if (ret >= minValue && ret <= maxValue)
                {
                    return ret;
                }
            }
            throw new OverflowException("This object's value is out of range");
        }

        public object Negate(object obj)
        {
            EInteger bigobj = (EInteger)obj;
            bigobj = -(EInteger)bigobj;
            return bigobj;
        }

        public object Abs(object obj)
        {
            return ((EInteger)obj).Abs();
        }

        public ERational AsExtendedRational(object obj)
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
        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="F:CBOREncodeOptions.None"]/*'/>
        [Obsolete("Use 'new CBOREncodeOptions(true,true)' instead. Option classes in this library will follow the form seen in JSONOptions in a later version; the approach used in this class is too complicated. 'CBOREncodeOptions.Default' contains recommended default options that may be adopted by certain CBORObject methods in the next major version.")]
        public static readonly CBOREncodeOptions None =
            new CBOREncodeOptions(0);

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="F:CBOREncodeOptions.Default"]/*'/>
        public static readonly CBOREncodeOptions Default =
          new CBOREncodeOptions(false, false);

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="F:CBOREncodeOptions.NoIndefLengthStrings"]/*'/>
        [Obsolete("Use 'new CBOREncodeOptions(false,true)' instead. Option classes in this library will follow the form seen in JSONOptions in a later version; the approach used in this class is too complicated.")]
        public static readonly CBOREncodeOptions NoIndefLengthStrings =
            new CBOREncodeOptions(1);

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="F:CBOREncodeOptions.NoDuplicateKeys"]/*'/>
        [Obsolete("Use 'new CBOREncodeOptions(true,false)' instead. Option classes in this library will follow the form seen in JSONOptions in a later version; the approach used in this class is too complicated.")]
        public static readonly CBOREncodeOptions NoDuplicateKeys =
            new CBOREncodeOptions(2);

        private readonly int value;

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBOREncodeOptions.#ctor"]/*'/>
        public CBOREncodeOptions() : this(false, false)
        {
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBOREncodeOptions.#ctor(System.Boolean,System.Boolean)"]/*'/>
        public CBOREncodeOptions(
      bool useIndefLengthStrings,
      bool allowDuplicateKeys) :
            this(useIndefLengthStrings, allowDuplicateKeys, false)
        {
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBOREncodeOptions.#ctor(System.Boolean,System.Boolean,System.Boolean)"]/*'/>
        public CBOREncodeOptions(
      bool useIndefLengthStrings,
      bool allowDuplicateKeys,
      bool ctap2Canonical)
        {
            int val = 0;
            if (!useIndefLengthStrings)
            {
                val |= 1;
            }
            if (!allowDuplicateKeys)
            {
                val |= 2;
            }
            this.value = val;
            this.Ctap2Canonical = ctap2Canonical;
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="P:CBOREncodeOptions.UseIndefLengthStrings"]/*'/>
        public bool UseIndefLengthStrings
        {
            get
            {
                return (this.value & 1) == 0;
            }
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="P:CBOREncodeOptions.AllowDuplicateKeys"]/*'/>
        public bool AllowDuplicateKeys
        {
            get
            {
                return (this.value & 2) == 0;
            }
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="P:CBOREncodeOptions.Ctap2Canonical"]/*'/>
        public bool Ctap2Canonical { get; private set; }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="P:CBOREncodeOptions.Value"]/*'/>
        [Obsolete("Option classes in this library will follow the form seen in JSONOptions in a later version; the approach used in this class is too complicated.")]
        public int Value
        {
            get
            {
                return this.value;
            }
        }

        private CBOREncodeOptions(int value) :
        this((value & 1) == 0, (value & 2) == 0)
        {
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBOREncodeOptions.Or(CBOREncodeOptions)"]/*'/>
        [Obsolete("May be removed in a later version. Option classes in this library will follow the form seen in JSONOptions in a later version; the approach used in this class is too complicated.")]
        public CBOREncodeOptions Or(CBOREncodeOptions o)
        {
            return new CBOREncodeOptions(this.value | o.value);
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBOREncodeOptions.And(CBOREncodeOptions)"]/*'/>
        [Obsolete("May be removed in a later version. Option classes in this library will follow the form seen in JSONOptions in a later version; the approach used in this class is too complicated.")]
        public CBOREncodeOptions And(CBOREncodeOptions o)
        {
            return new CBOREncodeOptions(this.value & o.value);
        }
    }

    #endregion

    #region CBORException
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:CBORException"]/*'/>
    public class CBORException : Exception
    {
        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORException.#ctor"]/*'/>
        public CBORException()
        {
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORException.#ctor(System.String)"]/*'/>
        public CBORException(string message) : base(message)
        {
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORException.#ctor(System.String,System.Exception)"]/*'/>
        public CBORException(string message, Exception innerException) :
          base(message, innerException)
        {
        }
    }

    #endregion

    #region CBORExtendedDecimal
    internal class CBORExtendedDecimal : ICBORNumber
    {
        public bool IsPositiveInfinity(object obj)
        {
            EDecimal ed = (EDecimal)obj;
            return ed.IsPositiveInfinity();
        }

        public bool IsInfinity(object obj)
        {
            EDecimal ed = (EDecimal)obj;
            return ed.IsInfinity();
        }

        public bool IsNegativeInfinity(object obj)
        {
            EDecimal ed = (EDecimal)obj;
            return ed.IsNegativeInfinity();
        }

        public bool IsNaN(object obj)
        {
            EDecimal ed = (EDecimal)obj;
            return ed.IsNaN();
        }

        public double AsDouble(object obj)
        {
            EDecimal ed = (EDecimal)obj;
            return ed.ToDouble();
        }

        public EDecimal AsExtendedDecimal(object obj)
        {
            EDecimal ed = (EDecimal)obj;
            return ed;
        }

        public EFloat AsExtendedFloat(object obj)
        {
            EDecimal ed = (EDecimal)obj;
            return ed.ToEFloat();
        }

        public float AsSingle(object obj)
        {
            EDecimal ed = (EDecimal)obj;
            return ed.ToSingle();
        }

        public EInteger AsEInteger(object obj)
        {
            EDecimal ed = (EDecimal)obj;
            return ed.ToEInteger();
        }

        public long AsInt64(object obj)
        {
            EDecimal ef = (EDecimal)obj;
            if (this.CanTruncatedIntFitInInt64(obj))
            {
                EInteger bi = ef.ToEInteger();
                return (long)bi;
            }
            throw new OverflowException("This object's value is out of range");
        }

        public bool CanFitInSingle(object obj)
        {
            EDecimal ef = (EDecimal)obj;
            return (!ef.IsFinite) ||
            (ef.CompareTo(EDecimal.FromSingle(ef.ToSingle())) == 0);
        }

        public bool CanFitInDouble(object obj)
        {
            EDecimal ef = (EDecimal)obj;
            return (!ef.IsFinite) ||
            (ef.CompareTo(EDecimal.FromDouble(ef.ToDouble())) == 0);
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
            EDecimal ef = (EDecimal)obj;
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
            return bi.GetSignedBitLength() <= 63;
        }

        public bool CanTruncatedIntFitInInt32(object obj)
        {
            EDecimal ef = (EDecimal)obj;
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

        public bool IsZero(object obj)
        {
            EDecimal ed = (EDecimal)obj;
            return ed.IsZero;
        }

        public int Sign(object obj)
        {
            EDecimal ed = (EDecimal)obj;
            return ed.IsNaN() ? 2 : ed.Sign;
        }

        public bool IsIntegral(object obj)
        {
            EDecimal ed = (EDecimal)obj;
            return ed.IsFinite && ((ed.Exponent.Sign >= 0) ||
            (ed.CompareTo(EDecimal.FromEInteger(ed.ToEInteger())) ==
            0));
        }

        public int AsInt32(object obj, int minValue, int maxValue)
        {
            EDecimal ef = (EDecimal)obj;
            if (this.CanTruncatedIntFitInInt32(obj))
            {
                EInteger bi = ef.ToEInteger();
                int ret = (int)bi;
                if (ret >= minValue && ret <= maxValue)
                {
                    return ret;
                }
            }
            throw new OverflowException("This object's value is out of range");
        }

        public object Negate(object obj)
        {
            EDecimal ed = (EDecimal)obj;
            return ed.Negate();
        }

        public object Abs(object obj)
        {
            EDecimal ed = (EDecimal)obj;
            return ed.Abs();
        }

        public ERational AsExtendedRational(object obj)
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
            EFloat ef = (EFloat)obj;
            return ef.IsPositiveInfinity();
        }

        public bool IsInfinity(object obj)
        {
            EFloat ef = (EFloat)obj;
            return ef.IsInfinity();
        }

        public bool IsNegativeInfinity(object obj)
        {
            EFloat ef = (EFloat)obj;
            return ef.IsNegativeInfinity();
        }

        public bool IsNaN(object obj)
        {
            EFloat ef = (EFloat)obj;
            return ef.IsNaN();
        }

        public double AsDouble(object obj)
        {
            EFloat ef = (EFloat)obj;
            return ef.ToDouble();
        }

        public EDecimal AsExtendedDecimal(object obj)
        {
            EFloat ef = (EFloat)obj;
            return ef.ToEDecimal();
        }

        public EFloat AsExtendedFloat(object obj)
        {
            EFloat ef = (EFloat)obj;
            return ef;
        }

        public float AsSingle(object obj)
        {
            EFloat ef = (EFloat)obj;
            return ef.ToSingle();
        }

        public EInteger AsEInteger(object obj)
        {
            EFloat ef = (EFloat)obj;
            return ef.ToEInteger();
        }

        public long AsInt64(object obj)
        {
            EFloat ef = (EFloat)obj;
            if (this.CanTruncatedIntFitInInt64(obj))
            {
                EInteger bi = ef.ToEInteger();
                return (long)bi;
            }
            throw new OverflowException("This object's value is out of range");
        }

        public bool CanFitInSingle(object obj)
        {
            EFloat ef = (EFloat)obj;
            return (!ef.IsFinite) ||
            (ef.CompareTo(EFloat.FromSingle(ef.ToSingle())) == 0);
        }

        public bool CanFitInDouble(object obj)
        {
            EFloat ef = (EFloat)obj;
            return (!ef.IsFinite) ||
            (ef.CompareTo(EFloat.FromDouble(ef.ToDouble())) == 0);
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
            EFloat ef = (EFloat)obj;
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
            return bi.GetSignedBitLength() <= 63;
        }

        public bool CanTruncatedIntFitInInt32(object obj)
        {
            EFloat ef = (EFloat)obj;
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

        public bool IsZero(object obj)
        {
            EFloat ef = (EFloat)obj;
            return ef.IsZero;
        }

        public int Sign(object obj)
        {
            EFloat ef = (EFloat)obj;
            return ef.IsNaN() ? 2 : ef.Sign;
        }

        public bool IsIntegral(object obj)
        {
            EFloat ef = (EFloat)obj;
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
            EFloat ef = (EFloat)obj;
            if (this.CanTruncatedIntFitInInt32(obj))
            {
                EInteger bi = ef.ToEInteger();
                int ret = (int)bi;
                if (ret >= minValue && ret <= maxValue)
                {
                    return ret;
                }
            }
            throw new OverflowException("This object's value is out of range");
        }

        public object Negate(object obj)
        {
            EFloat ed = (EFloat)obj;
            return ed.Negate();
        }

        public object Abs(object obj)
        {
            EFloat ed = (EFloat)obj;
            return ed.Abs();
        }

        public ERational AsExtendedRational(object obj)
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
            ERational er = (ERational)obj;
            return er.ToDouble();
        }

        public EDecimal AsExtendedDecimal(object obj)
        {
            ERational er = (ERational)obj;
            return

        er.ToEDecimalExactIfPossible(EContext.Decimal128.WithUnlimitedExponents());
        }

        public EFloat AsExtendedFloat(object obj)
        {
            ERational er = (ERational)obj;
            return

        er.ToEFloatExactIfPossible(EContext.Binary128.WithUnlimitedExponents());
        }

        public float AsSingle(object obj)
        {
            ERational er = (ERational)obj;
            return er.ToSingle();
        }

        public EInteger AsEInteger(object obj)
        {
            ERational er = (ERational)obj;
            return er.ToEInteger();
        }

        public long AsInt64(object obj)
        {
            ERational ef = (ERational)obj;
            if (ef.IsFinite)
            {
                EInteger bi = ef.ToEInteger();
                if (bi.GetSignedBitLength() <= 63)
                {
                    return (long)bi;
                }
            }
            throw new OverflowException("This object's value is out of range");
        }

        public bool CanFitInSingle(object obj)
        {
            ERational ef = (ERational)obj;
            return (!ef.IsFinite) ||
            (ef.CompareTo(ERational.FromSingle(ef.ToSingle())) == 0);
        }

        public bool CanFitInDouble(object obj)
        {
            ERational ef = (ERational)obj;
            return (!ef.IsFinite) ||
            (ef.CompareTo(ERational.FromDouble(ef.ToDouble())) == 0);
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
            ERational ef = (ERational)obj;
            if (!ef.IsFinite)
            {
                return false;
            }
            EInteger bi = ef.ToEInteger();
            return bi.GetSignedBitLength() <= 63;
        }

        public bool CanTruncatedIntFitInInt32(object obj)
        {
            ERational ef = (ERational)obj;
            if (!ef.IsFinite)
            {
                return false;
            }
            EInteger bi = ef.ToEInteger();
            return bi.CanFitInInt32();
        }

        public bool IsZero(object obj)
        {
            ERational ef = (ERational)obj;
            return ef.IsZero;
        }

        public int Sign(object obj)
        {
            ERational ef = (ERational)obj;
            return ef.Sign;
        }

        public bool IsIntegral(object obj)
        {
            ERational ef = (ERational)obj;
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
            ERational ef = (ERational)obj;
            if (ef.IsFinite)
            {
                EInteger bi = ef.ToEInteger();
                if (bi.CanFitInInt32())
                {
                    int ret = (int)bi;
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
            ERational ed = (ERational)obj;
            return ed.Negate();
        }

        public object Abs(object obj)
        {
            ERational ed = (ERational)obj;
            return ed.Abs();
        }

        public ERational AsExtendedRational(object obj)
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
            long val = (long)obj;
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

        public EDecimal AsExtendedDecimal(object obj)
        {
            return EDecimal.FromInt64((long)obj);
        }

        public EFloat AsExtendedFloat(object obj)
        {
            return EFloat.FromInt64((long)obj);
        }

        public ERational AsExtendedRational(object obj)
        {
            return ERational.FromInt64((long)obj);
        }

        public int AsInt32(object obj, int minValue, int maxValue)
        {
            long val = (long)obj;
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
            long intItem = (long)obj;
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
            long val = (long)obj;
            return val >= Int32.MinValue && val <= Int32.MaxValue;
        }

        public bool CanFitInInt64(object obj)
        {
            return true;
        }

        public bool CanFitInSingle(object obj)
        {
            long intItem = (long)obj;
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
            long val = (long)obj;
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

        public bool IsZero(object obj)
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
            long val = (long)obj;
            return (val == 0) ? 0 : ((val < 0) ? -1 : 1);
        }
    }
    #endregion

    #region CBORJson
    internal sealed class CBORJson
    {
        // JSON parsing methods
        private static int SkipWhitespaceJSON(CharacterInputWithCount reader)
        {
            while (true)
            {
                int c = reader.ReadChar();
                if (c == -1 || (c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09))
                {
                    return c;
                }
            }
        }

        private readonly CharacterInputWithCount reader;
        private StringBuilder sb;

        private string NextJSONString()
        {
            int c;
            this.sb = this.sb ?? new StringBuilder();
            this.sb.Remove(0, this.sb.Length);
            while (true)
            {
                c = this.reader.ReadChar();
                if (c == -1 || c < 0x20)
                {
                    this.reader.RaiseError("Unterminated string");
                }
                switch (c)
                {
                    case '\\':
                        c = this.reader.ReadChar();
                        switch (c)
                        {
                            case '\\':
                                this.sb.Append('\\');
                                break;
                            case '/':
                                // Now allowed to be escaped under RFC 8259
                                this.sb.Append('/');
                                break;
                            case '\"':
                                this.sb.Append('\"');
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
                                {  // Unicode escape
                                    c = 0;
                                    // Consists of 4 hex digits
                                    for (int i = 0; i < 4; ++i)
                                    {
                                        int ch = this.reader.ReadChar();
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
                                            this.reader.RaiseError("Invalid Unicode escaped character");
                                        }
                                    }
                                    if ((c & 0xf800) != 0xd800)
                                    {
                                        // Non-surrogate
                                        this.sb.Append((char)c);
                                    }
                                    else if ((c & 0xfc00) == 0xd800)
                                    {
                                        int ch = this.reader.ReadChar();
                                        if (ch != '\\' || this.reader.ReadChar() != 'u')
                                        {
                                            this.reader.RaiseError("Invalid escaped character");
                                        }
                                        int c2 = 0;
                                        for (int i = 0; i < 4; ++i)
                                        {
                                            ch = this.reader.ReadChar();
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
                                                this.reader.RaiseError("Invalid Unicode escaped character");
                                            }
                                        }
                                        if ((c2 & 0xfc00) != 0xdc00)
                                        {
                                            this.reader.RaiseError("Unpaired surrogate code point");
                                        }
                                        else
                                        {
                                            this.sb.Append((char)c);
                                            this.sb.Append((char)c2);
                                        }
                                    }
                                    else
                                    {
                                        this.reader.RaiseError("Unpaired surrogate code point");
                                    }
                                    break;
                                }
                            default:
                                {
                                    this.reader.RaiseError("Invalid escaped character");
                                    break;
                                }
                        }
                        break;
                    case 0x22:  // double quote
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
                                this.sb.Append((char)((((c - 0x10000) >> 10) & 0x3ff) +
                                    0xd800));
                                this.sb.Append((char)(((c - 0x10000) & 0x3ff) + 0xdc00));
                            }
                            break;
                        }
                }
            }
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
                this.reader.RaiseError("Unexpected end of data");
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
                        nextChar[0] = SkipWhitespaceJSON(this.reader);
                        return obj;
                    }
                case '{':
                    {
                        // Parse an object
                        obj = this.ParseJSONObject(depth + 1);
                        nextChar[0] = SkipWhitespaceJSON(this.reader);
                        return obj;
                    }
                case '[':
                    {
                        // Parse an array
                        obj = this.ParseJSONArray(depth + 1);
                        nextChar[0] = SkipWhitespaceJSON(this.reader);
                        return obj;
                    }
                case 't':
                    {
                        // Parse true
                        if (this.reader.ReadChar() != 'r' || this.reader.ReadChar() != 'u' ||
                            this.reader.ReadChar() != 'e')
                        {
                            this.reader.RaiseError("Value can't be parsed.");
                        }
                        nextChar[0] = SkipWhitespaceJSON(this.reader);
                        return CBORObject.True;
                    }
                case 'f':
                    {
                        // Parse false
                        if (this.reader.ReadChar() != 'a' || this.reader.ReadChar() != 'l' ||
                            this.reader.ReadChar() != 's' || this.reader.ReadChar() != 'e')
                        {
                            this.reader.RaiseError("Value can't be parsed.");
                        }
                        nextChar[0] = SkipWhitespaceJSON(this.reader);
                        return CBORObject.False;
                    }
                case 'n':
                    {
                        // Parse null
                        if (this.reader.ReadChar() != 'u' || this.reader.ReadChar() != 'l' ||
                            this.reader.ReadChar() != 'l')
                        {
                            this.reader.RaiseError("Value can't be parsed.");
                        }
                        nextChar[0] = SkipWhitespaceJSON(this.reader);
                        return CBORObject.Null;
                    }
                case '-':
                    {
                        // Parse a negative number
                        bool lengthTwo = true;
                        c = this.reader.ReadChar();
                        if (c < '0' || c > '9')
                        {
                            this.reader.RaiseError("JSON number can't be parsed.");
                        }
                        int cval = -(c - '0');
                        int cstart = c;
                        StringBuilder sb = null;
                        c = this.reader.ReadChar();
                        while (c == '-' || c == '+' || c == '.' || (c >= '0' && c <= '9') ||
                               c == 'e' || c == 'E')
                        {
                            if (lengthTwo)
                            {
                                sb = new StringBuilder();
                                sb.Append((char)'-');
                                sb.Append((char)cstart);
                                lengthTwo = false;
                            }
                            sb.Append((char)c);
                            c = this.reader.ReadChar();
                        }
                        if (lengthTwo)
                        {
                            obj = cval == 0 ?
                            CBORDataUtilities.ParseJSONNumber("-0", true, false, true) :
                              CBORObject.FromObject(cval);
                        }
                        else
                        {
                            str = sb.ToString();
                            obj = CBORDataUtilities.ParseJSONNumber(str);
                            if (obj == null)
                            {
                                this.reader.RaiseError("JSON number can't be parsed. " + str);
                            }
                        }
                        if (c == -1 || (c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09))
                        {
                            nextChar[0] = c;
                        }
                        else
                        {
                            nextChar[0] = SkipWhitespaceJSON(this.reader);
                        }
                        return obj;
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
                        // Parse a number
                        bool lengthOne = true;
                        int cval = c - '0';
                        int cstart = c;
                        StringBuilder sb = null;
                        c = this.reader.ReadChar();
                        while (c == '-' || c == '+' || c == '.' || (c >= '0' && c <= '9') ||
                               c == 'e' || c == 'E')
                        {
                            if (lengthOne)
                            {
                                sb = new StringBuilder();
                                sb.Append((char)cstart);
                                lengthOne = false;
                            }
                            sb.Append((char)c);
                            c = this.reader.ReadChar();
                        }
                        if (lengthOne)
                        {
                            obj = CBORObject.FromObject(cval);
                        }
                        else
                        {
                            str = sb.ToString();
                            obj = CBORDataUtilities.ParseJSONNumber(str);
                            if (obj == null)
                            {
                                this.reader.RaiseError("JSON number can't be parsed. " + str);
                            }
                        }
                        if (c == -1 || (c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09))
                        {
                            nextChar[0] = c;
                        }
                        else
                        {
                            nextChar[0] = SkipWhitespaceJSON(this.reader);
                        }
                        return obj;
                    }
                default:
                    this.reader.RaiseError("Value can't be parsed.");
                    break;
            }
            return null;
        }

        private readonly bool noDuplicates;

        public CBORJson(CharacterInputWithCount reader, bool noDuplicates)
        {
            this.reader = reader;
            this.sb = null;
            this.noDuplicates = noDuplicates;
        }

        public CBORObject ParseJSON(bool objectOrArrayOnly, int[] nextchar)
        {
            int c;
            CBORObject ret;
            c = SkipWhitespaceJSON(this.reader);
            if (c == '[')
            {
                ret = this.ParseJSONArray(0);
                nextchar[0] = SkipWhitespaceJSON(this.reader);
                return ret;
            }
            if (c == '{')
            {
                ret = this.ParseJSONObject(0);
                nextchar[0] = SkipWhitespaceJSON(this.reader);
                return ret;
            }
            if (objectOrArrayOnly)
            {
                this.reader.RaiseError("A JSON object must begin with '{' or '['");
            }
            return this.NextJSONValue(c, nextchar, 0);
        }

        internal static CBORObject ParseJSONValue(
          CharacterInputWithCount reader,
          bool noDuplicates,
          bool objectOrArrayOnly,
          int[] nextchar)
        {
            CBORJson cj = new CBORJson(reader, noDuplicates);
            return cj.ParseJSON(objectOrArrayOnly, nextchar);
        }

        private CBORObject ParseJSONObject(int depth)
        {
            // Assumes that the last character read was '{'
            if (depth > 1000)
            {
                this.reader.RaiseError("Too deeply nested");
            }
            int c;
            CBORObject key = null;
            CBORObject obj;
            int[] nextchar = new int[1];
            bool seenComma = false;
            Dictionary<CBORObject, CBORObject> myHashMap = new Dictionary<CBORObject, CBORObject>();
            while (true)
            {
                c = SkipWhitespaceJSON(this.reader);
                switch (c)
                {
                    case -1:
                        this.reader.RaiseError("A JSONObject must end with '}'");
                        break;
                    case '}':
                        if (seenComma)
                        {
                            // Situation like '{"0"=>1,}'
                            this.reader.RaiseError("Trailing comma");
                            return null;
                        }
                        return CBORObject.FromRaw(myHashMap);
                    default:
                        {
                            // Read the next string
                            if (c < 0)
                            {
                                this.reader.RaiseError("Unexpected end of data");
                                return null;
                            }
                            if (c != '"')
                            {
                                this.reader.RaiseError("Expected a string as a key");
                                return null;
                            }
                            // Parse a string that represents the object's key
                            // The tokenizer already checked the string for invalid
                            // surrogate pairs, so just call the CBORObject
                            // constructor directly
                            obj = CBORObject.FromRaw(this.NextJSONString());
                            key = obj;
                            if (this.noDuplicates && myHashMap.ContainsKey(obj))
                            {
                                this.reader.RaiseError("Key already exists: " + key);
                                return null;
                            }
                            break;
                        }
                }
                if (SkipWhitespaceJSON(this.reader) != ':')
                {
                    this.reader.RaiseError("Expected a ':' after a key");
                }
                // NOTE: Will overwrite existing value
                myHashMap[key] = this.NextJSONValue(
                  SkipWhitespaceJSON(this.reader),
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
                        this.reader.RaiseError("Expected a ',' or '}'");
                        break;
                }
            }
        }

        internal CBORObject ParseJSONArray(int depth)
        {
            // Assumes that the last character read was '['
            if (depth > 1000)
            {
                this.reader.RaiseError("Too deeply nested");
            }
            List<CBORObject> myArrayList = new List<CBORObject>();
            bool seenComma = false;
            int[] nextchar = new int[1];
            while (true)
            {
                int c = SkipWhitespaceJSON(this.reader);
                if (c == ']')
                {
                    if (seenComma)
                    {
                        // Situation like '[0,1,]'
                        this.reader.RaiseError("Trailing comma");
                    }
                    return CBORObject.FromRaw(myArrayList);
                }
                if (c == ',')
                {
                    // Situation like '[,0,1,2]' or '[0,,1]'
                    this.reader.RaiseError("Empty array element");
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
                        this.reader.RaiseError("Expected a ',' or ']'");
                        break;
                }
            }
        }

        private const string Hex16 = "0123456789ABCDEF";

        internal static void WriteJSONStringUnquoted(
          string str,
          StringOutput sb)
        {
            // Surrogates were already verified when this
            // string was added to the CBOR object; that check
            // is not repeated here
            bool first = true;
            for (int i = 0; i < str.Length; ++i)
            {
                char c = str[i];
                if (c == '\\' || c == '"')
                {
                    if (first)
                    {
                        first = false;
                        sb.WriteString(str, 0, i);
                    }
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
                    if (first)
                    {
                        first = false;
                        sb.WriteString(str, 0, i);
                    }
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
                else if (!first)
                {
                    if ((c & 0xfc00) == 0xd800)
                    {
                        sb.WriteString(str, i, 2);
                        ++i;
                    }
                    else
                    {
                        sb.WriteCodePoint((int)c);
                    }
                }
            }
            if (first)
            {
                sb.WriteString(str);
            }
        }

        internal static void WriteJSONToInternal(
          CBORObject obj,
          StringOutput writer,
          JSONOptions options)
        {
            int type = obj.ItemType;
            object thisItem = obj.ThisItem;
            switch (type)
            {
                case CBORObject.CBORObjectTypeSimpleValue:
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
                        writer.WriteString("null");
                        return;
                    }
                case CBORObject.CBORObjectTypeSingle:
                    {
                        float f = (float)thisItem;
                        if (Single.IsNegativeInfinity(f) ||
                            Single.IsPositiveInfinity(f) || Single.IsNaN(f))
                        {
                            writer.WriteString("null");
                            return;
                        }
                        writer.WriteString(
                          CBORObject.TrimDotZero(
                            CBORUtilities.SingleToString(f)));
                        return;
                    }
                case CBORObject.CBORObjectTypeDouble:
                    {
                        double f = (double)thisItem;
                        if (Double.IsNegativeInfinity(f) || Double.IsPositiveInfinity(f) ||
                            Double.IsNaN(f))
                        {
                            writer.WriteString("null");
                            return;
                        }
                        string dblString = CBORUtilities.DoubleToString(f);
                        writer.WriteString(
                          CBORObject.TrimDotZero(dblString));
                        return;
                    }
                case CBORObject.CBORObjectTypeInteger:
                    {
                        long longItem = (long)thisItem;
                        writer.WriteString(CBORUtilities.LongToString(longItem));
                        return;
                    }
                case CBORObject.CBORObjectTypeBigInteger:
                    {
                        writer.WriteString(((EInteger)thisItem).ToString());
                        return;
                    }
                case CBORObject.CBORObjectTypeExtendedDecimal:
                    {
                        EDecimal dec = (EDecimal)thisItem;
                        if (dec.IsInfinity() || dec.IsNaN())
                        {
                            writer.WriteString("null");
                        }
                        else
                        {
                            writer.WriteString(dec.ToString());
                        }
                        return;
                    }
                case CBORObject.CBORObjectTypeExtendedFloat:
                    {
                        EFloat flo = (EFloat)thisItem;
                        if (flo.IsInfinity() || flo.IsNaN())
                        {
                            writer.WriteString("null");
                            return;
                        }
                        if (flo.IsFinite &&
                            flo.Exponent.Abs().CompareTo((EInteger)2500) > 0)
                        {
                            // Too inefficient to convert to a decimal number
                            // from a bigfloat with a very high exponent,
                            // so convert to double instead
                            double f = flo.ToDouble();
                            if (Double.IsNegativeInfinity(f) ||
                                Double.IsPositiveInfinity(f) || Double.IsNaN(f))
                            {
                                writer.WriteString("null");
                                return;
                            }
                            string dblString =
                                CBORUtilities.DoubleToString(f);
                            writer.WriteString(
                              CBORObject.TrimDotZero(dblString));
                            return;
                        }
                        writer.WriteString(flo.ToString());
                        return;
                    }
                case CBORObject.CBORObjectTypeByteString:
                    {
                        byte[] byteArray = (byte[])thisItem;
                        if (byteArray.Length == 0)
                        {
                            writer.WriteString("\"\"");
                            return;
                        }
                        writer.WriteCodePoint((int)'\"');
                        if (obj.HasTag(22))
                        {
                            Base64.WriteBase64(
                              writer,
                              byteArray,
                              0,
                              byteArray.Length,
                              options.Base64Padding);
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
                            Base64.WriteBase64URL(
                              writer,
                              byteArray,
                              0,
                              byteArray.Length,
                              options.Base64Padding);
                        }
                        writer.WriteCodePoint((int)'\"');
                        break;
                    }
                case CBORObject.CBORObjectTypeTextString:
                    {
                        string thisString = (string)thisItem;
                        if (thisString.Length == 0)
                        {
                            writer.WriteString("\"\"");
                            return;
                        }
                        writer.WriteCodePoint((int)'\"');
                        WriteJSONStringUnquoted(thisString, writer);
                        writer.WriteCodePoint((int)'\"');
                        break;
                    }
                case CBORObject.CBORObjectTypeArray:
                    {
                        bool first = true;
                        writer.WriteCodePoint((int)'[');
                        foreach (CBORObject i in obj.AsList())
                        {
                            if (!first)
                            {
                                writer.WriteCodePoint((int)',');
                            }
                            WriteJSONToInternal(i, writer, options);
                            first = false;
                        }
                        writer.WriteCodePoint((int)']');
                        break;
                    }
                case CBORObject.CBORObjectTypeExtendedRational:
                    {
                        ERational dec = (ERational)thisItem;
                        EDecimal f = dec.ToEDecimalExactIfPossible(
                          EContext.Decimal128.WithUnlimitedExponents());
                        if (!f.IsFinite)
                        {
                            writer.WriteString("null");
                        }
                        else
                        {
                            writer.WriteString(f.ToString());
                        }
                        break;
                    }
                case CBORObject.CBORObjectTypeMap:
                    {
                        bool first = true;
                        bool hasNonStringKeys = false;
                        IDictionary<CBORObject, CBORObject> objMap = obj.AsMap();
                        foreach (KeyValuePair<CBORObject, CBORObject> entry in objMap)
                        {
                            CBORObject key = entry.Key;
                            if (key.ItemType != CBORObject.CBORObjectTypeTextString)
                            {
                                hasNonStringKeys = true;
                                break;
                            }
                        }
                        if (!hasNonStringKeys)
                        {
                            writer.WriteCodePoint((int)'{');
                            foreach (KeyValuePair<CBORObject, CBORObject> entry in objMap)
                            {
                                CBORObject key = entry.Key;
                                CBORObject value = entry.Value;
                                if (!first)
                                {
                                    writer.WriteCodePoint((int)',');
                                }
                                writer.WriteCodePoint((int)'\"');
                                WriteJSONStringUnquoted((string)key.ThisItem, writer);
                                writer.WriteCodePoint((int)'\"');
                                writer.WriteCodePoint((int)':');
                                WriteJSONToInternal(value, writer, options);
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
                            foreach (KeyValuePair<CBORObject, CBORObject> entry in objMap)
                            {
                                CBORObject key = entry.Key;
                                CBORObject value = entry.Value;
                                string str = (key.ItemType == CBORObject.CBORObjectTypeTextString) ?
                                       ((string)key.ThisItem) : key.ToJSONString();
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
                                WriteJSONStringUnquoted((string)key, writer);
                                writer.WriteCodePoint((int)'\"');
                                writer.WriteCodePoint((int)':');
                                WriteJSONToInternal(value, writer, options);
                                first = false;
                            }
                            writer.WriteCodePoint((int)'}');
                        }
                        break;
                    }
                default: throw new InvalidOperationException("Unexpected item type");
            }
        }
    }
    #endregion

    #region CBORSingle
    internal sealed class CBORSingle : ICBORNumber
    {
        private const float SingleOneLsh64 = 9223372036854775808f;

        public bool IsPositiveInfinity(object obj)
        {
            return Single.IsPositiveInfinity((float)obj);
        }

        public bool IsInfinity(object obj)
        {
            return Single.IsInfinity((float)obj);
        }

        public bool IsNegativeInfinity(object obj)
        {
            return Single.IsNegativeInfinity((float)obj);
        }

        public bool IsNaN(object obj)
        {
            return Single.IsNaN((float)obj);
        }

        public double AsDouble(object obj)
        {
            return (double)(float)obj;
        }

        public EDecimal AsExtendedDecimal(object obj)
        {
            return EDecimal.FromSingle((float)obj);
        }

        public EFloat AsExtendedFloat(object obj)
        {
            return EFloat.FromSingle((float)obj);
        }

        public float AsSingle(object obj)
        {
            return (float)obj;
        }

        public EInteger AsEInteger(object obj)
        {
            return CBORUtilities.BigIntegerFromSingle((float)obj);
        }

        public long AsInt64(object obj)
        {
            float fltItem = (float)obj;
            if (Single.IsNaN(fltItem))
            {
                throw new OverflowException("This object's value is out of range");
            }
            fltItem = (fltItem < 0) ? (float)Math.Ceiling(fltItem) :
              (float)Math.Floor(fltItem);
            if (fltItem >= -SingleOneLsh64 && fltItem < SingleOneLsh64)
            {
                return (long)fltItem;
            }
            throw new OverflowException("This object's value is out of range");
        }

        public bool CanFitInSingle(object obj)
        {
            return true;
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

        public bool CanTruncatedIntFitInInt64(object obj)
        {
            float fltItem = (float)obj;
            if (Single.IsNaN(fltItem) || Single.IsInfinity(fltItem))
            {
                return false;
            }
            float fltItem2 = (fltItem < 0) ? (float)Math.Ceiling(fltItem) :
              (float)Math.Floor(fltItem);
            return fltItem2 >= -SingleOneLsh64 && fltItem2 <
              SingleOneLsh64;
        }

        public bool CanTruncatedIntFitInInt32(object obj)
        {
            float fltItem = (float)obj;
            if (Single.IsNaN(fltItem) || Single.IsInfinity(fltItem))
            {
                return false;
            }
            float fltItem2 = (fltItem < 0) ? (float)Math.Ceiling(fltItem) :
              (float)Math.Floor(fltItem);
            // Convert float to double to avoid precision loss when
            // converting Int32.MinValue/MaxValue to float
            return (double)fltItem2 >= Int32.MinValue && (double)fltItem2 <=
              Int32.MaxValue;
        }

        public int AsInt32(object obj, int minValue, int maxValue)
        {
            float fltItem = (float)obj;
            if (Single.IsNaN(fltItem))
            {
                throw new OverflowException("This object's value is out of range");
            }
            fltItem = (fltItem < 0) ? (float)Math.Ceiling(fltItem) :
              (float)Math.Floor(fltItem);
            // Convert float to double to avoid precision loss when
            // converting Int32.MinValue/MaxValue to float
            if ((double)fltItem >= Int32.MinValue && (double)fltItem <=
                Int32.MaxValue)
            {
                int ret = (int)fltItem;
                return ret;
            }
            throw new OverflowException("This object's value is out of range");
        }

        public bool IsZero(object obj)
        {
            return (float)obj == 0.0f;
        }

        public int Sign(object obj)
        {
            float flt = (float)obj;
            return Single.IsNaN(flt) ? 2 : (flt == 0.0f ? 0 : (flt < 0.0f ? -1 : 1));
        }

        public bool IsIntegral(object obj)
        {
            float fltItem = (float)obj;
            if (Single.IsNaN(fltItem) || Single.IsInfinity(fltItem))
            {
                return false;
            }
            float fltItem2 = (fltItem < 0) ? (float)Math.Ceiling(fltItem) :
              (float)Math.Floor(fltItem);
            return fltItem == fltItem2;
        }

        public object Negate(object obj)
        {
            float val = (float)obj;
            return -val;
        }

        public object Abs(object obj)
        {
            float val = (float)obj;
            return (val < 0) ? -val : obj;
        }

        public ERational AsExtendedRational(object obj)
        {
            return ERational.FromSingle((float)obj);
        }

        public bool IsNegative(object obj)
        {
            float val = (float)obj;
            int ivalue = BitConverter.ToInt32(BitConverter.GetBytes((float)val), 0);
            return (ivalue >> 31) != 0;
        }
    }

    #endregion

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

    #region CBORType
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:CBORType"]/*'/>
    public enum CBORType
    {
        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="F:CBORType.Number"]/*'/>
        Number,

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="F:CBORType.Boolean"]/*'/>
        Boolean,

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="F:CBORType.SimpleValue"]/*'/>
        SimpleValue,

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="F:CBORType.ByteString"]/*'/>
        ByteString,

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="F:CBORType.TextString"]/*'/>
        TextString,

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="F:CBORType.Array"]/*'/>
        Array,

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="F:CBORType.Map"]/*'/>
        Map
    }

    #endregion

    #region CBORTagGenericString
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
    #endregion

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

    #region CBORTypeMapper
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:CBORTypeMapper"]/*'/>
    public sealed class CBORTypeMapper
    {
        private readonly IList<string> typePrefixes;
        private readonly IList<string> typeNames;
        private readonly IDictionary<Object, ConverterInfo>
          converters;

        /// <summary>Initializes a new instance of the CBORTypeMapper
        /// class.</summary>
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
        /// <exception cref='T:System.ArgumentNullException'>The parameter
        /// <paramref name='type'/> or <paramref name='converter'/> is
        /// null.</exception>
        /// <exception cref='T:System.ArgumentException'>"Converter doesn't
        /// contain a proper ToCBORObject method".</exception>
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
            ConverterInfo ci = new ConverterInfo
            {
                Converter = converter,
                ToObject = PropertyMap.FindOneArgumentMethod(
              converter,
              "ToCBORObject",
              type)
            };
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
            if (convinfo.FromObject == null)
            {
                return null;
            }
            return PropertyMap.InvokeOneArgumentMethod(
              convinfo.FromObject,
              convinfo.Converter,
              cbor);
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
            if (convinfo == null)
            {
                return null;
            }
            return (CBORObject)PropertyMap.InvokeOneArgumentMethod(
              convinfo.ToObject,
              convinfo.Converter,
              obj);
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeMapper.FilterTypeName(System.String)"]/*'/>
        public bool FilterTypeName(string typeName)
        {
            if (String.IsNullOrEmpty(typeName))
            {
                return false;
            }
            foreach (string prefix in this.typePrefixes)
            {
                if (typeName.Length >= prefix.Length &&
                  typeName.Substring(0, prefix.Length).Equals(prefix))
                {
                    return true;
                }
            }
            foreach (string name in this.typeNames)
            {
                if (typeName.Equals(name))
                {
                    return true;
                }
            }
            return false;
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeMapper.AddTypePrefix(System.String)"]/*'/>
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

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:CBORTypeMapper.AddTypeName(System.String)"]/*'/>
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
            /// <include file='../../docs.xml'
            /// path='docs/doc[@name="P:CBORObject.ConverterInfo.ToObject"]/*'/>
            public object ToObject { get; set; }

            public object FromObject { get; set; }

            /// <summary>Gets a value not documented yet.</summary>
            /// <value>An internal API value.</value>
            public object Converter { get; set; }
        }
    }

    #endregion

    #region CBORUriConverter
    internal class CBORUriConverter : ICBORToFromConverter<Uri>
    {
        private CBORObject ValidateObject(CBORObject obj)
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

        public Uri FromCBORObject(CBORObject obj)
        {
            if (obj.HasMostOuterTag(32))
            {
                this.ValidateObject(obj);
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
            return CBORObject.FromObjectAndTag(uriString, (int)32);
        }
    }

    #endregion

    #region JSONOptions
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:JSONOptions"]/*'/>
    public sealed class JSONOptions
    {
        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:JSONOptions.#ctor"]/*'/>
        public JSONOptions() : this(false)
        {
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:JSONOptions.#ctor(System.Boolean)"]/*'/>
        public JSONOptions(bool base64Padding)
        {
            this.Base64Padding = base64Padding;
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="F:JSONOptions.Default"]/*'/>
        public static readonly JSONOptions Default = new JSONOptions();

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="P:JSONOptions.Base64Padding"]/*'/>
        public bool Base64Padding { get; private set; }
    }
    #endregion

    #region PODOptions
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:PODOptions"]/*'/>
    public class PODOptions
    {
        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:PODOptions.#ctor"]/*'/>
        public PODOptions() : this(true, true)
        {
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:PODOptions.#ctor(System.Boolean,System.Boolean)"]/*'/>
        public PODOptions(bool removeIsPrefix, bool useCamelCase)
        {
#pragma warning disable 618
            this.RemoveIsPrefix = removeIsPrefix;
#pragma warning restore 618
            this.UseCamelCase = useCamelCase;
        }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="F:PODOptions.Default"]/*'/>
        public static readonly PODOptions Default = new PODOptions();

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="P:PODOptions.RemoveIsPrefix"]/*'/>
        [Obsolete("Property name conversion may change, making this property obsolete.")]
        public bool RemoveIsPrefix { get; private set; }

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="P:PODOptions.UseCamelCase"]/*'/>
        public bool UseCamelCase { get; private set; }
    }
    #endregion

    #region PropertyMap
    internal static class PropertyMap
    {
        // TODO: Remove in next major version
        internal const bool DateTimeCompatHack = true;

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

            public string GetAdjustedName(bool removeIsPrefix, bool useCamelCase)
            {
                string thisName = this.Name;
                // Convert 'IsXYZ' to 'XYZ'
                if (removeIsPrefix && thisName.Length >= 3 && thisName[0] == 'I' &&
                  thisName[1] == 's' && thisName[2] >= 'A' && thisName[2] <= 'Z')
                {
                    // NOTE (Jun. 17, 2017, Peter O.): Was "== 'Z'", which was a
                    // bug reported
                    // by GitHub user "richardschneider". See peteroupc/CBOR#17.
                    thisName = thisName.Substring(2);
                }
                // Convert to camel case
                if (useCamelCase && thisName[0] >= 'A' && thisName[0] <= 'Z')
                {
                    StringBuilder sb = new System.Text.StringBuilder();
                    sb.Append((char)(thisName[0] + 0x20));
                    sb.Append(thisName.Substring(1));
                    thisName = sb.ToString();
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
                foreach (PropertyInfo pi in GetTypeProperties(t))
                {
                    if (pi.CanRead && (pi.CanWrite || anonymous) &&
                    pi.GetIndexParameters().Length == 0)
                    {
                        PropertyData pd = new PropertyMap.PropertyData()
                        {
                            Name = pi.Name,
                            Prop = pi
                        };
                        ret.Add(pd);
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

        // Inappropriate to mark these obsolete; they're
        // just non-publicly-visible methods to convert to
        // and from legacy arbitrary-precision classes
#pragma warning disable 618
        public static BigInteger ToLegacy(EInteger ei)
        {
            return BigInteger.ToLegacy(ei);
        }

        public static ExtendedDecimal ToLegacy(EDecimal ed)
        {
            return ExtendedDecimal.ToLegacy(ed);
        }

        public static ExtendedFloat ToLegacy(EFloat ef)
        {
            return ExtendedFloat.ToLegacy(ef);
        }

        public static ExtendedRational ToLegacy(ERational er)
        {
            return ExtendedRational.ToLegacy(er);
        }

        public static EInteger FromLegacy(BigInteger ei)
        {
            return BigInteger.FromLegacy(ei);
        }

        public static EDecimal FromLegacy(ExtendedDecimal ed)
        {
            return ExtendedDecimal.FromLegacy(ed);
        }

        public static EFloat FromLegacy(ExtendedFloat ef)
        {
            return ExtendedFloat.FromLegacy(ef);
        }

        public static ERational FromLegacy(ExtendedRational er)
        {
            return ExtendedRational.FromLegacy(er);
        }
#pragma warning restore 618
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

        public static CBORObject FromArray(
      Object arrObj,
      PODOptions options)
        {
            Array arr = (Array)arrObj;
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
                for (int i = 0; i < len; ++i)
                {
                    obj.Add(
                  CBORObject.FromObject(
                  arr.GetValue(i),
                  options));
                }
                return obj;
            }
            int[] index = new int[rank];
            int[] dimensions = new int[rank];
            for (int i = 0; i < rank; ++i)
            {
                dimensions[i] = arr.GetLength(i);
            }
            if (!FirstElement(index, dimensions))
            {
                return obj;
            }
            obj = BuildCBORArray(dimensions);
            do
            {
                CBORObject o = CBORObject.FromObject(
                 arr.GetValue(index),
                 options);
                SetCBORObject(obj, index, o);
            } while (NextElement(index, dimensions));
            return obj;
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
            // TODO: Make Set(object, object) work with arrays
            // in next major version
            ret[ilen] = obj;
        }

        public static object EnumToObject(Enum value)
        {
            Type t = Enum.GetUnderlyingType(value.GetType());
            if (t.Equals(typeof(ulong)))
            {
                byte[] data = new byte[13];
                ulong uvalue = Convert.ToUInt64(value);
                data[0] = (byte)(uvalue & 0xff);
                data[1] = (byte)((uvalue >> 8) & 0xff);
                data[2] = (byte)((uvalue >> 16) & 0xff);
                data[3] = (byte)((uvalue >> 24) & 0xff);
                data[4] = (byte)((uvalue >> 32) & 0xff);
                data[5] = (byte)((uvalue >> 40) & 0xff);
                data[6] = (byte)((uvalue >> 48) & 0xff);
                data[7] = (byte)((uvalue >> 56) & 0xff);
                data[8] = (byte)0;
                return EInteger.FromBytes(data, true);
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

        public static object TypeToObject(CBORObject objThis, Type t)
        {
            if (t.Equals(typeof(DateTime)))
            {
                return new CBORTag0().FromCBORObject(objThis);
            }
            if (t.Equals(typeof(Guid)))
            {
                return new CBORTag37().FromCBORObject(objThis);
            }
            if (t.Equals(typeof(int)))
            {
                return objThis.AsInt32();
            }
            if (t.Equals(typeof(long)))
            {
                return objThis.AsInt64();
            }
            if (t.Equals(typeof(double)))
            {
                return objThis.AsDouble();
            }
            if (t.Equals(typeof(float)))
            {
                return objThis.AsSingle();
            }
            if (t.Equals(typeof(bool)))
            {
                return objThis.IsTrue;
            }

            if (t.FullName != null &&
               (StartsWith(t.FullName, "System.Win32.") ||
               StartsWith(t.FullName, "System.IO.")))
            {
                throw new NotSupportedException("Type " + t.FullName + " not supported");
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
        if (t.IsGenericType) {
          Type td = t.GetGenericTypeDefinition();
          isList = td.Equals(typeof(List<>)) || td.Equals(typeof(IList<>)) ||
            td.Equals(typeof(ICollection<>)) ||
            td.Equals(typeof(IEnumerable<>));
            } else {
          throw new NotImplementedException();
        }
        isList = isList && t.GetGenericArguments().Length == 1;
        if (isList) {
          objectType = t.GetGenericArguments()[0];
          Type listType = typeof(List<>).MakeGenericType(objectType);
          listObject = Activator.CreateInstance(listType);
        }
#else
                if (t.GetTypeInfo().IsGenericType)
                {
                    Type td = t.GetGenericTypeDefinition();
                    isList = (td.Equals(typeof(List<>)) ||
            td.Equals(typeof(IList<>)) ||
            td.Equals(typeof(ICollection<>)) ||
            td.Equals(typeof(IEnumerable<>)));
                }
                else
                {
                    throw new NotImplementedException();
                }
                isList = (isList && t.GenericTypeArguments.Length == 1);
                if (isList)
                {
                    objectType = t.GenericTypeArguments[0];
                    Type listType = typeof(List<>).MakeGenericType(objectType);
                    listObject = Activator.CreateInstance(listType);
                }
#endif
                if (listObject != null)
                {
                    System.Collections.IList ie = (System.Collections.IList)listObject;
                    foreach (CBORObject value in objThis.Values)
                    {
                        ie.Add(value.ToObject(objectType));
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
                if (dictObject != null)
                {
                    System.Collections.IDictionary idic =
                      (System.Collections.IDictionary)dictObject;
                    foreach (CBORObject key in objThis.Keys)
                    {
                        CBORObject value = objThis[key];
                        idic.Add(
              key.ToObject(keyType),
              value.ToObject(valueType));
                    }
                    return dictObject;
                }
                List<KeyValuePair<string, CBORObject>> values = new List<KeyValuePair<string, CBORObject>>();
                foreach (string key in PropertyMap.GetPropertyNames(
                           t,
                           true,
                           true))
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
                return PropertyMap.ObjectWithProperties(
            t,
            values,
            true,
            true);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static object ObjectWithProperties(
          Type t,
          IEnumerable<KeyValuePair<string, CBORObject>> keysValues)
        {
            return ObjectWithProperties(t, keysValues, true, true);
        }

        public static object ObjectWithProperties(
             Type t,
             IEnumerable<KeyValuePair<string, CBORObject>> keysValues,
             bool removeIsPrefix,
      bool useCamelCase)
        {
            object o = null;
#if NET20 || NET40
      foreach (var ci in t.GetConstructors()) {
#else
            foreach (ConstructorInfo ci in t.GetTypeInfo().DeclaredConstructors)
            {
#endif
                if (ci.IsPublic)
                {
                    int nump = ci.GetParameters().Length;
                    o = ci.Invoke(new object[nump]);
                    break;
                }
            }
            o = o ?? Activator.CreateInstance(t);
            Dictionary<string, CBORObject> dict = new Dictionary<string, CBORObject>();
            foreach (KeyValuePair<string, CBORObject> kv in keysValues)
            {
                string name = kv.Key;
                dict[name] = kv.Value;
            }
            foreach (PropertyData key in GetPropertyList(o.GetType()))
            {
                string name = key.GetAdjustedName(removeIsPrefix, useCamelCase);
                if (dict.ContainsKey(name))
                {
                    object dobj = dict[name].ToObject(key.Prop.PropertyType);
                    key.Prop.SetValue(o, dobj, null);
                }
            }
            return o;
        }

        public static IEnumerable<KeyValuePair<string, object>>
        GetProperties(Object o)
        {
            return GetProperties(o, true, true);
        }

        public static IEnumerable<string>
        GetPropertyNames(Type t, bool removeIsPrefix, bool useCamelCase)
        {
            foreach (PropertyData key in GetPropertyList(t))
            {
                yield return key.GetAdjustedName(removeIsPrefix, useCamelCase);
            }
        }

        public static IEnumerable<KeyValuePair<string, object>>
        GetProperties(Object o, bool removeIsPrefix, bool useCamelCase)
        {
            foreach (PropertyData key in GetPropertyList(o.GetType()))
            {
                yield return new KeyValuePair<string, object>(
          key.GetAdjustedName(removeIsPrefix, useCamelCase),
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
    #endregion

    #region PropertyMap2
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
            int index = (int)smallIndex;
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
            int index = (int)bigIndex;
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
            if (this.outputStream != null)
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
            else
            {
                this.builder.Append(str, index, length);
            }
        }

        public void WriteCodePoint(int codePoint)
        {
            if (codePoint < 0)
            {
                throw new ArgumentException("codePoint (" + codePoint +
                        ") is less than 0");
            }
            if (codePoint > 0x10ffff)
            {
                throw new ArgumentException("codePoint (" + codePoint +
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
                    this.outputStream.WriteByte((byte)(0xc0 | ((codePoint >> 6) & 0x1f)));
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
                    this.outputStream.WriteByte((byte)(0x80 | ((codePoint >> 6) & 0x3f)));
                    this.outputStream.WriteByte((byte)(0x80 | (codePoint & 0x3f)));
                }
                else
                {
                    this.outputStream.WriteByte((byte)(0xf0 | ((codePoint >> 18) &
                              0x08)));
                    this.outputStream.WriteByte((byte)(0x80 | ((codePoint >> 12) &
                              0x3f)));
                    this.outputStream.WriteByte((byte)(0x80 | ((codePoint >> 6) & 0x3f)));
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
                    this.builder.Append((char)((((codePoint - 0x10000) >> 10) &
                              0x3ff) + 0xd800));
                    this.builder.Append((char)(((codePoint - 0x10000) & 0x3ff) + 0xdc00));
                }
            }
        }
    }

    #endregion

    #region StringRefs
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:StringRefs"]/*'/>
    internal class StringRefs
    {
        private readonly List<List<CBORObject>> stack;

        public StringRefs()
        {
            this.stack = new List<List<CBORObject>>();
            List<CBORObject> firstItem = new List<CBORObject>();
            this.stack.Add(firstItem);
        }

        public void Push()
        {
            List<CBORObject> firstItem = new List<CBORObject>();
            this.stack.Add(firstItem);
        }

        public void Pop()
        {
#if DEBUG
            if (this.stack.Count <= 0)
            {
                throw new ArgumentException("this.stack.Count (" + this.stack.Count +
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
                throw new ArgumentException("lengthHint (" + lengthHint +
                            ") is less than " + "0 ");
            }
#endif
            bool addStr = false;
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
            int index = (int)smallIndex;
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
            int index = (int)bigIndex;
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
