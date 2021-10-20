//******************************************************************************************************************************************************************************************//
// Public Domain                                                                                                                                                                            //
//                                                                                                                                                                                          //
// Written by Peter O. in 2014.                                                                                                                                                             //
//                                                                                                                                                                                          //
// Any copyright is dedicated to the Public Domain. http://creativecommons.org/publicdomain/zero/1.0/                                                                                       //
//                                                                                                                                                                                          //
// If you like this, you should donate to Peter O. at: http://peteroupc.github.io/                                                                                                          //
//******************************************************************************************************************************************************************************************//
using System;
using System.Text;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor.Numbers
{
  internal static class EDecimalByteArrayString {
    private const int MaxSafeInt = EDecimal.MaxSafeInt;

    internal static EDecimal FromString(
      byte[] chars,
      int offset,
      int length,
      EContext ctx,
      bool throwException) {
      int tmpoffset = offset;
      if (chars == null) {
        if (!throwException) {
          return null;
        } else {
          throw new ArgumentNullException(nameof(chars));
        }
      }
      if (tmpoffset < 0) {
        if (!throwException) {
          return null;
        } else { throw new FormatException("offset(" + tmpoffset + ") is" +
"\u0020less" + "\u0020than " + "0");
}
      }
      if (tmpoffset > chars.Length) {
        if (!throwException) {
          return null;
        } else { throw new FormatException("offset(" + tmpoffset + ") is" +
"\u0020more" + "\u0020than " + chars.Length);
}
      }
      if (length <= 0) {
        if (length == 0) {
          if (!throwException) {
            return null;
          } else {
            throw new FormatException("length is 0");
          }
        }
        if (!throwException) {
          return null;
        } else {
  throw new FormatException("length(" + length + ") is less than " + "0");
 }
      }
      if (length > chars.Length) {
        if (!throwException) {
          return null;
        } else {
  throw new FormatException("length(" + length + ") is more than " +
chars.Length);
 }
      }
      if (chars.Length - tmpoffset < length) {
        if (!throwException) {
          return null;
        } else { throw new FormatException("chars's length minus " +
tmpoffset + "(" + (chars.Length - tmpoffset) + ") is less than " + length);
}
      }
      var negative = false;
      int endStr = tmpoffset + length;
      byte c = chars[tmpoffset];
      if (c == '-') {
        negative = true;
        ++tmpoffset;
        if (tmpoffset >= endStr) {
          if (!throwException) {
            return null;
          } else {
            throw new FormatException();
          }
        }
        c = chars[tmpoffset];
      } else if (chars[tmpoffset] == '+') {
        ++tmpoffset;
        if (tmpoffset >= endStr) {
          if (!throwException) {
            return null;
          } else {
            throw new FormatException();
          }
        }
        c = chars[tmpoffset];
      }
      int i = tmpoffset;
      if (c < '0' || c > '9') {
        EDecimal ed = ParseSpecialValue(
          chars,
          i,
          endStr,
          negative,
          ctx,
          throwException);
        if (ed != null) {
          return ed;
        }
      }
      if (ctx != null && ctx.HasMaxPrecision && ctx.HasExponentRange &&
        !ctx.IsSimplified) {
        return ParseOrdinaryNumberLimitedPrecision(
          chars,
          i,
          endStr,
          negative,
          ctx,
          throwException);
      } else {
        return ParseOrdinaryNumber(
          chars,
          i,
          endStr,
          negative,
          ctx,
          throwException);
      }
    }

    private static EDecimal ParseSpecialValue(
      byte[] chars,
      int i,
      int endStr,
      bool negative,
      EContext ctx,
      bool throwException) {
      var mantInt = 0;
      EInteger mant = null;
      var haveDigits = false;
      var digitStart = 0;
      if (i + 8 == endStr) {
        if ((chars[i] == 'I' || chars[i] == 'i') &&
          (chars[i + 1] == 'N' || chars[i + 1] == 'n') &&
          (chars[i + 2] == 'F' || chars[i + 2] == 'f') &&
          (chars[i + 3] == 'I' || chars[i + 3] == 'i') && (chars[i + 4] ==
            'N' ||
            chars[i + 4] == 'n') && (chars[i + 5] == 'I' || chars[i + 5] ==
            'i') &&
          (chars[i + 6] == 'T' || chars[i + 6] == 't') && (chars[i + 7] ==
            'Y' || chars[i + 7] == 'y')) {
          if (ctx != null && ctx.IsSimplified && i < endStr) {
            if (!throwException) {
              return null;
            } else {
              throw new FormatException("Infinity not allowed");
            }
          }
          return negative ? EDecimal.NegativeInfinity :
            EDecimal.PositiveInfinity;
        }
      }
      if (i + 3 == endStr) {
        if ((chars[i] == 'I' || chars[i] == 'i') &&
          (chars[i + 1] == 'N' || chars[i + 1] == 'n') && (chars[i + 2] ==
            'F' || chars[i + 2] == 'f')) {
          if (ctx != null && ctx.IsSimplified && i < endStr) {
            if (!throwException) {
              return null;
            } else {
              throw new FormatException("Infinity not allowed");
            }
          }
          return negative ? EDecimal.NegativeInfinity :
            EDecimal.PositiveInfinity;
        }
      }
      if (i + 3 <= endStr) {
        // Quiet NaN
        if ((chars[i] == 'N' || chars[i] == 'n') && (chars[i + 1] == 'A' ||
            chars[i +
              1] == 'a') && (chars[i + 2] == 'N' || chars[i + 2] == 'n')) {
          if (ctx != null && ctx.IsSimplified && i < endStr) {
            if (!throwException) {
              return null;
            } else {
              throw new FormatException("NaN not allowed");
            }
          }
          int flags2 = (negative ? BigNumberFlags.FlagNegative : 0) |
            BigNumberFlags.FlagQuietNaN;
          if (i + 3 == endStr) {
            return (!negative) ? EDecimal.NaN : new EDecimal(
                FastIntegerFixed.Zero,
                FastIntegerFixed.Zero,
                (byte)flags2);
          }
          i += 3;
          var digitCount = new FastInteger(0);
          FastInteger maxDigits = null;
          haveDigits = false;
          if (ctx != null && ctx.HasMaxPrecision) {
            maxDigits = FastInteger.FromBig(ctx.Precision);
            if (ctx.ClampNormalExponents) {
              maxDigits.Decrement();
            }
          }
          digitStart = i;
          for (; i < endStr; ++i) {
            if (chars[i] >= '0' && chars[i] <= '9') {
              var thisdigit = (int)(chars[i] - '0');
              haveDigits = haveDigits || thisdigit != 0;
              if (mantInt <= MaxSafeInt) {
                // multiply by 10
                mantInt *= 10;
                mantInt += thisdigit;
              }
              if (haveDigits && maxDigits != null) {
                digitCount.Increment();
                if (digitCount.CompareTo(maxDigits) > 0) {
                  // NaN contains too many digits
                  if (!throwException) {
                    return null;
                  } else {
                    throw new FormatException();
                  }
                }
              }
            } else {
              if (!throwException) {
                return null;
              } else {
                throw new FormatException();
              }
            }
          }
          if (mantInt > MaxSafeInt) {
            mant = EInteger.FromSubstring(chars, digitStart, endStr);
          }
          EInteger bigmant = (mant == null) ? ((EInteger)mantInt) :
            mant;
          flags2 = (negative ? BigNumberFlags.FlagNegative : 0) |
            BigNumberFlags.FlagQuietNaN;
          return EDecimal.CreateWithFlags(
              FastIntegerFixed.FromBig(bigmant),
              FastIntegerFixed.Zero,
              flags2);
        }
      }
      if (i + 4 <= endStr) {
        // Signaling NaN
        if ((chars[i] == 'S' || chars[i] == 's') && (chars[i + 1] == 'N' ||
            chars[i +
              1] == 'n') && (chars[i + 2] == 'A' || chars[i + 2] == 'a') &&
          (chars[i + 3] == 'N' || chars[i + 3] == 'n')) {
          if (ctx != null && ctx.IsSimplified && i < endStr) {
            if (!throwException) {
              return null;
            } else {
              throw new FormatException("NaN not allowed");
            }
          }
          if (i + 4 == endStr) {
            int flags2 = (negative ? BigNumberFlags.FlagNegative : 0) |
              BigNumberFlags.FlagSignalingNaN;
            return (!negative) ? EDecimal.SignalingNaN :
              new EDecimal(
                FastIntegerFixed.Zero,
                FastIntegerFixed.Zero,
                (byte)flags2);
          }
          i += 4;
          var digitCount = new FastInteger(0);
          FastInteger maxDigits = null;
          haveDigits = false;
          if (ctx != null && ctx.HasMaxPrecision) {
            maxDigits = FastInteger.FromBig(ctx.Precision);
            if (ctx.ClampNormalExponents) {
              maxDigits.Decrement();
            }
          }
          digitStart = i;
          for (; i < endStr; ++i) {
            if (chars[i] >= '0' && chars[i] <= '9') {
              var thisdigit = (int)(chars[i] - '0');
              haveDigits = haveDigits || thisdigit != 0;
              if (mantInt <= MaxSafeInt) {
                // multiply by 10
                mantInt *= 10;
                mantInt += thisdigit;
              }
              if (haveDigits && maxDigits != null) {
                digitCount.Increment();
                if (digitCount.CompareTo(maxDigits) > 0) {
                  // NaN contains too many digits
                  if (!throwException) {
                    return null;
                  } else {
                    throw new FormatException();
                  }
                }
              }
            } else {
              if (!throwException) {
                return null;
              } else {
                throw new FormatException();
              }
            }
          }
          if (mantInt > MaxSafeInt) {
            mant = EInteger.FromSubstring(chars, digitStart, endStr);
          }
          int flags3 = (negative ? BigNumberFlags.FlagNegative : 0) |
            BigNumberFlags.FlagSignalingNaN;
          EInteger bigmant = (mant == null) ? ((EInteger)mantInt) :
            mant;
          return EDecimal.CreateWithFlags(
              bigmant,
              EInteger.Zero,
              flags3);
        }
      }
      return null;
    }

    private static EDecimal ParseOrdinaryNumberLimitedPrecision(
      byte[] chars,
      int offset,
      int endStr,
      bool negative,
      EContext ctx,
      bool throwException) {
      int tmpoffset = offset;
      if (chars == null) {
        if (!throwException) {
          return null;
        } else {
          throw new ArgumentNullException(nameof(chars));
        }
      }
      if (ctx == null || !ctx.HasMaxPrecision) {
        if (!throwException) {
          return null;
        } else {
          throw new InvalidOperationException();
        }
      }
      var haveDecimalPoint = false;
      var haveDigits = false;
      var haveExponent = false;
      var newScaleInt = 0;
      int i = tmpoffset;
      long mantissaLong = 0L;
      // Ordinary number
      int digitStart = i;
      int digitEnd = i;
      int decimalDigitStart = i;
      var haveNonzeroDigit = false;
      var decimalPrec = 0;
      int decimalDigitEnd = i;
      var nonzeroBeyondMax = false;
      var beyondMax = false;
      var lastdigit = -1;
      EInteger precisionPlusTwo = ctx.Precision.Add(2);
      for (; i < endStr; ++i) {
        byte ch = chars[i];
        if (ch >= '0' && ch <= '9') {
          var thisdigit = (int)(ch - '0');
          haveDigits = true;
          haveNonzeroDigit |= thisdigit != 0;
          if (beyondMax || (precisionPlusTwo.CompareTo(decimalPrec) < 0 &&
              mantissaLong == Int64.MaxValue)) {
            // Well beyond maximum precision, significand is
            // max or bigger
            beyondMax = true;
            if (thisdigit != 0) {
              nonzeroBeyondMax = true;
            }
            if (!haveDecimalPoint) {
              // NOTE: Absolute value will not be more than
              // the sequence portion's length, so will fit comfortably
              // in an 'int'.
              newScaleInt = checked(newScaleInt + 1);
            }
            continue;
          }
          lastdigit = thisdigit;
          if (haveNonzeroDigit) {
            ++decimalPrec;
          }
          if (haveDecimalPoint) {
            decimalDigitEnd = i + 1;
          } else {
            digitEnd = i + 1;
          }
          if (mantissaLong <= 922337203685477580L) {
            mantissaLong *= 10;
            mantissaLong += thisdigit;
          } else {
            mantissaLong = Int64.MaxValue;
          }
          if (haveDecimalPoint) {
            // NOTE: Absolute value will not be more than
            // the sequence portion's length, so will fit comfortably
            // in an 'int'.
            newScaleInt = checked(newScaleInt - 1);
          }
        } else if (ch == '.') {
          if (haveDecimalPoint) {
            if (!throwException) {
              return null;
            } else {
              throw new FormatException();
            }
          }
          haveDecimalPoint = true;
          decimalDigitStart = i + 1;
          decimalDigitEnd = i + 1;
        } else if (ch == 'E' || ch == 'e') {
          haveExponent = true;
          ++i;
          break;
        } else {
          if (!throwException) {
            return null;
          } else {
            throw new FormatException();
          }
        }
      }
      if (!haveDigits) {
        if (!throwException) {
          return null;
        } else {
          throw new FormatException();
        }
      }
      var expInt = 0;
      var expoffset = 1;
      var expDigitStart = -1;
      var expPrec = 0;
      bool zeroMantissa = !haveNonzeroDigit;
      haveNonzeroDigit = false;
      if (haveExponent) {
        haveDigits = false;
        if (i == endStr) {
          if (!throwException) {
            return null;
          } else {
            throw new FormatException();
          }
        }
        if (chars[i] == '+' || chars[i] == '-') {
          if (chars[i] == '-') {
            expoffset = -1;
          }
          ++i;
        }
        expDigitStart = i;
        for (; i < endStr; ++i) {
          byte ch = chars[i];
          if (ch >= '0' && ch <= '9') {
            haveDigits = true;
            var thisdigit = (int)(ch - '0');
            haveNonzeroDigit |= thisdigit != 0;
            if (haveNonzeroDigit) {
              ++expPrec;
            }
            if (expInt <= 214748364) {
              expInt *= 10;
              expInt += thisdigit;
            } else {
              expInt = Int32.MaxValue;
            }
          } else {
            if (!throwException) {
              return null;
            } else {
              throw new FormatException();
            }
          }
        }
        if (!haveDigits) {
          if (!throwException) {
            return null;
          } else {
            throw new FormatException();
          }
        }
        expInt *= expoffset;
        if (expPrec > 12) {
          // Exponent that can't be compensated by digit
          // length without remaining higher than Int32.MaxValue
          if (expoffset < 0) {
            return EDecimal.SignalUnderflow(ctx, negative, zeroMantissa);
          } else {
            return EDecimal.SignalOverflow(ctx, negative, zeroMantissa);
          }
        }
      }
      if (i != endStr) {
        if (!throwException) {
          return null;
        } else {
          throw new FormatException();
        }
      }
      if (expInt != Int32.MaxValue && expInt > -Int32.MaxValue &&
        mantissaLong != Int64.MaxValue) {
        // Low precision, low exponent
        var finalexp = (long)expInt + (long)newScaleInt;
        if (negative) {
          mantissaLong = -mantissaLong;
        }
        EDecimal eret = EDecimal.Create(mantissaLong, finalexp);
        if (negative && zeroMantissa) {
          eret = eret.Negate();
        }
        return eret.RoundToPrecision(ctx);
      }
      EInteger mant = null;
      EInteger exp = (!haveExponent) ? EInteger.Zero :
        EInteger.FromSubstring(chars, expDigitStart, endStr);
      if (expoffset < 0) {
        exp = exp.Negate();
      }
      exp = exp.Add(newScaleInt);
      if (nonzeroBeyondMax) {
        exp = exp.Subtract(1);
        ++decimalPrec;
      }
      if (ctx.HasExponentRange) {
        EInteger adjExpUpperBound = exp.Add(decimalPrec).Subtract(1);
        EInteger adjExpLowerBound = exp;
        EInteger eTiny = ctx.EMin.Subtract(ctx.Precision.Subtract(1));
        eTiny = eTiny.Subtract(1);
        // DebugUtility.Log("exp=" + adjExpLowerBound + "~" +
        // adjExpUpperBound + ", emin={0} emax={1}", ctx.EMin, ctx.EMax);
        if (adjExpUpperBound.CompareTo(eTiny) < 0) {
          return EDecimal.SignalUnderflow(ctx, negative, zeroMantissa);
        } else if (adjExpLowerBound.CompareTo(ctx.EMax) > 0) {
          return EDecimal.SignalOverflow(ctx, negative, zeroMantissa);
        }
      }
      if (zeroMantissa) {
        EDecimal ef = EDecimal.Create(
            EInteger.Zero,
            exp);
        if (negative) {
          ef = ef.Negate();
        }
        return ef.RoundToPrecision(ctx);
      } else if (decimalDigitStart != decimalDigitEnd) {
        byte[] ctmpstr = Extras.CharsConcat(
            chars,
            digitStart,
            digitEnd - digitStart,
            chars,
            decimalDigitStart,
            decimalDigitEnd - decimalDigitStart);
        mant = EInteger.FromString(ctmpstr);
      } else {
        mant = EInteger.FromSubstring(chars, digitStart, digitEnd);
      }
      if (nonzeroBeyondMax) {
        mant = mant.Multiply(10).Add(1);
      }
      if (negative) {
        mant = mant.Negate();
      }
      return EDecimal.Create(mant, exp)
        .RoundToPrecision(ctx);
    }

    private static EDecimal ParseOrdinaryNumberNoContext(
      byte[] chars,
      int i,
      int endStr,
      bool negative,
      bool throwException) {
      // NOTE: Negative sign at beginning was omitted
      // from the sequence portion
      var mantInt = 0;
      EInteger mant = null;
      var haveDecimalPoint = false;
      var haveExponent = false;
      var newScaleInt = 0;
      var haveDigits = false;
      var digitStart = 0;
      EInteger newScale = null;
      // Ordinary number
      if (endStr - i == 1) {
        byte tch = chars[i];
        if (tch >= '0' && tch <= '9') {
          // String portion is a single digit
          var si = (int)(tch - '0');
          return negative ? ((si == 0) ? EDecimal.NegativeZero :
              EDecimal.FromCache(-si)) : EDecimal.FromCache(si);
        }
      }
      digitStart = i;
      int digitEnd = i;
      int decimalDigitStart = i;
      var haveNonzeroDigit = false;
      var decimalPrec = 0;
      var firstdigit = -1;
      int decimalDigitEnd = i;
      var lastdigit = -1;
      var realDigitEnd = -1;
      var realDecimalEnd = -1;
      for (; i < endStr; ++i) {
        byte ch = chars[i];
        if (ch >= '0' && ch <= '9') {
          var thisdigit = (int)(ch - '0');
          haveNonzeroDigit |= thisdigit != 0;
          if (firstdigit < 0) {
            firstdigit = thisdigit;
          }
          haveDigits = true;
          lastdigit = thisdigit;
          if (haveNonzeroDigit) {
            ++decimalPrec;
          }
          if (haveDecimalPoint) {
            decimalDigitEnd = i + 1;
          } else {
            digitEnd = i + 1;
          }
          if (mantInt <= MaxSafeInt) {
            // multiply by 10
            mantInt *= 10;
            mantInt += thisdigit;
          }
          if (haveDecimalPoint) {
            if (newScaleInt == Int32.MinValue ||
              newScaleInt == Int32.MaxValue) {
              newScale = newScale ?? EInteger.FromInt32(newScaleInt);
              newScale = newScale.Subtract(1);
            } else {
              --newScaleInt;
            }
          }
        } else if (ch == '.') {
          if (haveDecimalPoint) {
            if (!throwException) {
              return null;
            } else {
              throw new FormatException();
            }
          }
          haveDecimalPoint = true;
          realDigitEnd = i;
          decimalDigitStart = i + 1;
          decimalDigitEnd = i + 1;
        } else if (ch == 'E' || ch == 'e') {
          realDecimalEnd = i;
          haveExponent = true;
          ++i;
          break;
        } else {
          if (!throwException) {
            return null;
          } else {
            throw new FormatException();
          }
        }
      }
      if (!haveDigits) {
        if (!throwException) {
          return null;
        } else {
          throw new FormatException();
        }
      }
      if (realDigitEnd < 0) {
        realDigitEnd = i;
      }
      if (realDecimalEnd < 0) {
        realDecimalEnd = i;
      }
      EDecimal ret = null;
      EInteger exp = null;
      var expInt = 0;
      var expoffset = 1;
      var expDigitStart = -1;
      var expPrec = 0;
      haveNonzeroDigit = false;
      if (haveExponent) {
        haveDigits = false;
        if (i == endStr) {
          if (!throwException) {
            return null;
          } else {
            throw new FormatException();
          }
        }
        byte ch = chars[i];
        if (ch == '+' || ch == '-') {
          if (ch == '-') {
            expoffset = -1;
          }
          ++i;
        }
        expDigitStart = i;
        for (; i < endStr; ++i) {
          ch = chars[i];
          if (ch >= '0' && ch <= '9') {
            haveDigits = true;
            var thisdigit = (int)(ch - '0');
            haveNonzeroDigit |= thisdigit != 0;
            if (haveNonzeroDigit) {
              ++expPrec;
            }
            if (expInt <= MaxSafeInt) {
              expInt *= 10;
              expInt += thisdigit;
            }
          } else {
            if (!throwException) {
              return null;
            } else {
              throw new FormatException();
            }
          }
        }
        if (!haveDigits) {
          if (!throwException) {
            return null;
          } else {
            throw new FormatException();
          }
        }
      }
      if (i != endStr) {
        if (!throwException) {
          return null;
        } else {
          throw new FormatException();
        }
      }
      // Calculate newScale if exponent is "small"
      if (haveExponent && expInt <= MaxSafeInt) {
        if (expoffset >= 0 && newScaleInt == 0 && newScale == null) {
          newScaleInt = expInt;
        } else if (newScale == null) {
          long tmplong = newScaleInt;
          if (expoffset < 0) {
            tmplong -= expInt;
          } else if (expInt != 0) {
            tmplong += expInt;
          }
          if (tmplong >= Int32.MaxValue && tmplong <= Int32.MinValue) {
            newScaleInt = (int)tmplong;
          } else {
            newScale = EInteger.FromInt64(tmplong);
          }
        } else {
          if (expoffset < 0) {
            newScale = newScale.Subtract(expInt);
          } else if (expInt != 0) {
            newScale = newScale.Add(expInt);
          }
        }
      }
      int de = digitEnd;
      int dde = decimalDigitEnd;
      if (!haveExponent && haveDecimalPoint) {
        // No more than 18 digits
        long lv = 0L;
        int expo = -(dde - decimalDigitStart);
        var digitCount = 0;
        if (mantInt <= MaxSafeInt) {
          lv = mantInt;
        } else {
          var vi = 0;
          for (vi = digitStart; vi < de; ++vi) {
            byte chvi = chars[vi];
            #if DEBUG
            if (!(chvi >= '0' && chvi <= '9')) {
              if (!throwException) {
                return null;
              } else { throw new ArgumentException("doesn't satisfy chvi>=" +
"\u0020'0'" + "\u0020 &&" + "\u0020chvi<= '9'");
}
            }
            #endif
            if (digitCount < 0 || digitCount >= 18) {
              digitCount = -1;
              break;
            } else if (digitCount > 0 || chvi != '0') {
              ++digitCount;
            }
            lv = checked((lv * 10) + (int)(chvi - '0'));
          }
          for (vi = decimalDigitStart; vi < dde; ++vi) {
            byte chvi = chars[vi];
            #if DEBUG
            if (!(chvi >= '0' && chvi <= '9')) {
              if (!throwException) {
                return null;
              } else { throw new ArgumentException("doesn't satisfy chvi>=" +
"\u0020'0'" + "\u0020 &&" + "\u0020chvi<= '9'");
}
            }
            #endif
            if (digitCount < 0 || digitCount >= 18) {
              digitCount = -1;
              break;
            } else if (digitCount > 0 || chvi != '0') {
              ++digitCount;
            }
            lv = checked((lv * 10) + (int)(chvi - '0'));
          }
        }
        if (negative) {
          lv = -lv;
        }
        if (digitCount >= 0 && (!negative || lv != 0)) {
          ret = EDecimal.Create(lv, (long)expo);
          return ret;
        }
      }
      // Parse significand if it's "big"
      if (mantInt > MaxSafeInt) {
        if (haveDecimalPoint) {
          if (digitEnd - digitStart == 1 && firstdigit == 0) {
            mant = EInteger.FromSubstring(
                chars,
                decimalDigitStart,
                decimalDigitEnd);
          } else {
            byte[] cdecstr = Extras.CharsConcat(
                chars,
                digitStart,
                digitEnd - digitStart,
                chars,
                decimalDigitStart,
                decimalDigitEnd - decimalDigitStart);
            mant = EInteger.FromString(cdecstr);
          }
        } else {
          mant = EInteger.FromSubstring(chars, digitStart, digitEnd);
        }
      }
      if (haveExponent && expInt > MaxSafeInt) {
        // Parse exponent if it's "big"
        exp = EInteger.FromSubstring(chars, expDigitStart, endStr);
        newScale = newScale ?? EInteger.FromInt32(newScaleInt);
        newScale = (expoffset < 0) ? newScale.Subtract(exp) :
          newScale.Add(exp);
      }
      FastIntegerFixed fastIntScale;
      FastIntegerFixed fastIntMant;
      fastIntScale = (newScale == null) ? FastIntegerFixed.FromInt32(
          newScaleInt) : FastIntegerFixed.FromBig(newScale);
      if (mant == null) {
        fastIntMant = FastIntegerFixed.FromInt32(mantInt);
      } else if (mant.CanFitInInt32()) {
        mantInt = mant.ToInt32Checked();
        fastIntMant = FastIntegerFixed.FromInt32(mantInt);
      } else {
        fastIntMant = FastIntegerFixed.FromBig(mant);
      }
      ret = new EDecimal(
        fastIntMant,
        fastIntScale,
        (byte)(negative ? BigNumberFlags.FlagNegative : 0));
      return ret;
    }

    private static EDecimal ParseOrdinaryNumber(
      byte[] chars,
      int i,
      int endStr,
      bool negative,
      EContext ctx,
      bool throwException) {
      if (ctx == null) {
        return ParseOrdinaryNumberNoContext(
          chars,
          i,
          endStr,
          negative,
          throwException);
      }
      // NOTE: Negative sign at beginning was omitted
      // from the sequence portion
      var mantInt = 0;
      EInteger mant = null;
      var haveDecimalPoint = false;
      var haveExponent = false;
      var newScaleInt = 0;
      var haveDigits = false;
      var digitStart = 0;
      EInteger newScale = null;
      // Ordinary number
      if (endStr - i == 1) {
        byte tch = chars[i];
        if (tch >= '0' && tch <= '9') {
          // String portion is a single digit
          EDecimal cret;
          var si = (int)(tch - '0');
          cret = negative ? ((si == 0) ? EDecimal.NegativeZero :
              EDecimal.FromCache(-si)) : EDecimal.FromCache(si);
          if (ctx != null) {
            cret = EDecimal.GetMathValue(ctx).RoundAfterConversion(cret, ctx);
          }
          return cret;
        }
      }
      digitStart = i;
      int digitEnd = i;
      int decimalDigitStart = i;
      var haveNonzeroDigit = false;
      var decimalPrec = 0;
      int decimalDigitEnd = i;
      // NOTE: Also check HasFlagsOrTraps here because
      // it's burdensome to determine which flags have
      // to be set when applying the optimization here
      bool roundDown = ctx != null && ctx.HasMaxPrecision &&
        !ctx.IsPrecisionInBits && (ctx.Rounding == ERounding.Down ||
          (negative && ctx.Rounding == ERounding.Ceiling) ||
          (!negative && ctx.Rounding == ERounding.Floor)) &&
        !ctx.HasFlagsOrTraps;
      bool roundHalf = ctx != null && ctx.HasMaxPrecision &&
        !ctx.IsPrecisionInBits && (ctx.Rounding == ERounding.HalfUp ||
          (ctx.Rounding == ERounding.HalfDown) ||
          (ctx.Rounding == ERounding.HalfEven)) &&
        !ctx.HasFlagsOrTraps;
      bool roundUp = ctx != null && ctx.HasMaxPrecision &&
        !ctx.IsPrecisionInBits && (ctx.Rounding == ERounding.Up ||
          (!negative && ctx.Rounding == ERounding.Ceiling) ||
          (negative && ctx.Rounding == ERounding.Floor)) &&
        !ctx.HasFlagsOrTraps;
      var haveIgnoredDigit = false;
      var lastdigit = -1;
      var beyondPrecision = false;
      var ignoreNextDigit = false;
      var zerorun = 0;
      var realDigitEnd = -1;
      var realDecimalEnd = -1;
      // DebugUtility.Log("round half=" + (// roundHalf) +
      // " up=" + roundUp + " down=" + roundDown +
      // " maxprec=" + (ctx != null && ctx.HasMaxPrecision));
      for (; i < endStr; ++i) {
        byte ch = chars[i];
        if (ch >= '0' && ch <= '9') {
          var thisdigit = (int)(ch - '0');
          haveNonzeroDigit |= thisdigit != 0;
          haveDigits = true;
          beyondPrecision |= ctx != null && ctx.HasMaxPrecision &&
            !ctx.IsPrecisionInBits && ctx.Precision.CompareTo(decimalPrec)
            <= 0;
          if (ctx != null) {
            if (ignoreNextDigit) {
              haveIgnoredDigit = true;
              ignoreNextDigit = false;
            }
            if (roundDown && (haveIgnoredDigit || beyondPrecision)) {
              // "Ignored" digit
              haveIgnoredDigit = true;
            } else if (roundUp && beyondPrecision && !haveIgnoredDigit) {
              if (thisdigit > 0) {
                ignoreNextDigit = true;
              } else {
                roundUp = false;
              }
            } else if (roundHalf && beyondPrecision && !haveIgnoredDigit) {
              if (thisdigit >= 1 && thisdigit < 5) {
                ignoreNextDigit = true;
              } else if (thisdigit > 5 || (thisdigit == 5 &&
                  ctx.Rounding == ERounding.HalfUp)) {
                roundHalf = false;
                roundUp = true;
                ignoreNextDigit = true;
              } else {
                roundHalf = false;
              }
            }
          }
          if (haveIgnoredDigit) {
            zerorun = 0;
            if (newScaleInt == Int32.MinValue ||
              newScaleInt == Int32.MaxValue) {
              newScale = newScale ?? EInteger.FromInt32(newScaleInt);
              newScale = newScale.Add(1);
            } else {
              ++newScaleInt;
            }
          } else {
            lastdigit = thisdigit;
            if (beyondPrecision && thisdigit == 0) {
              ++zerorun;
            } else {
              zerorun = 0;
            }
            if (haveNonzeroDigit) {
              ++decimalPrec;
            }
            if (haveDecimalPoint) {
              decimalDigitEnd = i + 1;
            } else {
              digitEnd = i + 1;
            }
            if (mantInt <= MaxSafeInt) {
              // multiply by 10
              mantInt *= 10;
              mantInt += thisdigit;
            }
          }
          if (haveDecimalPoint) {
            if (newScaleInt == Int32.MinValue ||
              newScaleInt == Int32.MaxValue) {
              newScale = newScale ?? EInteger.FromInt32(newScaleInt);
              newScale = newScale.Subtract(1);
            } else {
              --newScaleInt;
            }
          }
        } else if (ch == '.') {
          if (haveDecimalPoint) {
            if (!throwException) {
              return null;
            } else {
              throw new FormatException();
            }
          }
          haveDecimalPoint = true;
          realDigitEnd = i;
          decimalDigitStart = i + 1;
          decimalDigitEnd = i + 1;
        } else if (ch == 'E' || ch == 'e') {
          realDecimalEnd = i;
          haveExponent = true;
          ++i;
          break;
        } else {
          if (!throwException) {
            return null;
          } else {
            throw new FormatException();
          }
        }
      }
      if (!haveDigits) {
        if (!throwException) {
          return null;
        } else {
          throw new FormatException();
        }
      }
      if (realDigitEnd < 0) {
        realDigitEnd = i;
      }
      if (realDecimalEnd < 0) {
        realDecimalEnd = i;
      }
      if (zerorun > 0 && lastdigit == 0 && (ctx == null ||
          !ctx.HasFlagsOrTraps)) {
        decimalPrec -= zerorun;
        var nondec = 0;
        // NOTE: This check is apparently needed for correctness
        if (ctx == null || (!ctx.HasMaxPrecision ||
            decimalPrec - ctx.Precision.ToInt32Checked() > zerorun)) {
          if (haveDecimalPoint) {
            int decdigits = decimalDigitEnd - decimalDigitStart;
            nondec = Math.Min(decdigits, zerorun);
            decimalDigitEnd -= nondec;
            int remain = zerorun - nondec;
            digitEnd -= remain;
            // DebugUtility.Log("remain={0} nondec={1}
            // newScale={2}",remain,nondec,newScaleInt);
            nondec = zerorun;
          } else {
            digitEnd -= zerorun;
            nondec = zerorun;
          }
          if (newScaleInt > Int32.MinValue + nondec &&
            newScaleInt < Int32.MaxValue - nondec) {
            newScaleInt += nondec;
          } else {
            newScale = newScale ?? EInteger.FromInt32(newScaleInt);
            newScale = newScale.Add(nondec);
          }
        }
        // DebugUtility.Log("-->zerorun={0} prec={1} [whole={2}, dec={3}]
        // chars={4}",zerorun,decimalPrec,
        // digitEnd-digitStart, decimalDigitEnd-decimalDigitStart, chars);
      }
      // if (ctx != null) {
      // DebugUtility.Log("roundup [prec=" + decimalPrec + ", ctxprec=" +
      // (// ctx.Precision) + ", chars=" + (// chars.Substring(0, Math.Min(20,
      // chars.Length))) + "] " + (ctx.Rounding));
      // }
      // DebugUtility.Log("digitRange="+digitStart+"-"+digitEnd+
      // "decdigitRange="+decimalDigitStart+"-"+decimalDigitEnd);
      if (
        roundUp && ctx != null &&
        ctx.Precision.CompareTo(decimalPrec) < 0) {
        int precdiff = decimalPrec - ctx.Precision.ToInt32Checked();
        // DebugUtility.Log("precdiff = " + precdiff + " [prec=" + (// decimalPrec) +
        // ",
        // ctxprec=" + ctx.Precision + "]");
        if (precdiff > 1) {
          int precchop = precdiff - 1;
          decimalPrec -= precchop;
          int nondec = precchop;
          // DebugUtility.Log("precchop=" + (precchop));
          if (haveDecimalPoint) {
            int decdigits = decimalDigitEnd - decimalDigitStart;
            // DebugUtility.Log("decdigits=" + decdigits + " decprecchop=" + (decdigits));
            decimalDigitEnd -= nondec;
            int remain = precchop - nondec;
            digitEnd -= remain;
          } else {
            digitEnd -= precchop;
          }
          if (newScaleInt < Int32.MaxValue - nondec) {
            newScaleInt += nondec;
          } else {
            newScale = newScale ?? EInteger.FromInt32(newScaleInt);
            newScale = newScale.Add(nondec);
          }
        }
      }
      EDecimal ret = null;
      EInteger exp = null;
      var expInt = 0;
      var expoffset = 1;
      var expDigitStart = -1;
      var expPrec = 0;
      haveNonzeroDigit = false;
      if (haveExponent) {
        haveDigits = false;
        if (i == endStr) {
          if (!throwException) {
            return null;
          } else {
            throw new FormatException();
          }
        }
        if (chars[i] == '+' || chars[i] == '-') {
          if (chars[i] == '-') {
            expoffset = -1;
          }
          ++i;
        }
        expDigitStart = i;
        for (; i < endStr; ++i) {
          byte ch = chars[i];
          if (ch >= '0' && ch <= '9') {
            haveDigits = true;
            var thisdigit = (int)(ch - '0');
            haveNonzeroDigit |= thisdigit != 0;
            if (haveNonzeroDigit) {
              ++expPrec;
            }
            if (expInt <= MaxSafeInt) {
              expInt *= 10;
              expInt += thisdigit;
            }
          } else {
            if (!throwException) {
              return null;
            } else {
              throw new FormatException();
            }
          }
        }
        if (!haveDigits) {
          if (!throwException) {
            return null;
          } else {
            throw new FormatException();
          }
        }
      }
      if (i != endStr) {
        if (!throwException) {
          return null;
        } else {
          throw new FormatException();
        }
      }
      // Calculate newScale if exponent is "small"
      if (haveExponent && expInt <= MaxSafeInt) {
        if (expoffset >= 0 && newScaleInt == 0 && newScale == null) {
          newScaleInt = expInt;
        } else if (newScale == null) {
          long tmplong = newScaleInt;
          if (expoffset < 0) {
            tmplong -= expInt;
          } else if (expInt != 0) {
            tmplong += expInt;
          }
          if (tmplong >= Int32.MaxValue && tmplong <= Int32.MinValue) {
            newScaleInt = (int)tmplong;
          } else {
            newScale = EInteger.FromInt64(tmplong);
          }
        } else {
          if (expoffset < 0) {
            newScale = newScale.Subtract(expInt);
          } else if (expInt != 0) {
            newScale = newScale.Add(expInt);
          }
        }
      }
      if (ctx != null && (mantInt > MaxSafeInt || (haveExponent &&
            expInt > MaxSafeInt)) && ctx.HasExponentRange) {
        EInteger ns;
        if (expInt <= MaxSafeInt && ctx != null) {
          ns = newScale ?? EInteger.FromInt32(newScaleInt);
        } else {
          EInteger trialExponent = EInteger.FromInt32(MaxSafeInt);
          if (expPrec > 25) {
            // Exponent has many significant digits; use a bigger trial exponent
            trialExponent = EInteger.FromInt64(Int64.MaxValue);
          }
          // Trial exponent; in case of overflow or
          // underflow, the real exponent will also overflow or underflow
          if (expoffset >= 0 && newScaleInt == 0 && newScale == null) {
            ns = trialExponent;
          } else {
            ns = newScale ?? EInteger.FromInt32(newScaleInt);
            ns = (expoffset < 0) ? ns.Subtract(trialExponent) :
              ns.Add(trialExponent);
          }
        }
        int expwithin = EDecimal.CheckOverflowUnderflow(
            ctx,
            decimalPrec,
            ns);
        if (mantInt == 0 && (expwithin == 1 || expwithin == 2 ||
            expwithin == 3)) {
          // Significand is zero
          ret = new EDecimal(
            FastIntegerFixed.FromInt32(0),
            FastIntegerFixed.FromBig(ns),
            (byte)(negative ? BigNumberFlags.FlagNegative : 0));
          return EDecimal.GetMathValue(ctx).RoundAfterConversion(ret, ctx);
        } else if (expwithin == 1) {
          // Exponent indicates overflow
          return EDecimal.GetMathValue(ctx).SignalOverflow(ctx, negative);
        } else if (expwithin == 2 || (expwithin == 3 && mantInt < MaxSafeInt)) {
          // Exponent indicates underflow to zero
          ret = new EDecimal(
            FastIntegerFixed.FromInt32(expwithin == 3 ? mantInt : 1),
            FastIntegerFixed.FromBig(ns),
            (byte)(negative ? BigNumberFlags.FlagNegative : 0));
          return EDecimal.GetMathValue(ctx).RoundAfterConversion(ret, ctx);
        } else if (expwithin == 3 && (ctx == null || ctx.Traps == 0)) {
          // Exponent indicates underflow to zero, adjust exponent
          ret = new EDecimal(
            FastIntegerFixed.FromInt32(1),
            FastIntegerFixed.FromBig(ns),
            (byte)(negative ? BigNumberFlags.FlagNegative : 0));
          ret = EDecimal.GetMathValue(ctx).RoundAfterConversion(ret, ctx);
          ns = ret.Exponent.Subtract(decimalPrec - 1);
          ret = EDecimal.ChangeExponent(ret, ns);
          return ret;
        }
      }
      // DebugUtility.Log("digitRange="+digitStart+"-"+digitEnd+
      // "decdigitRange="+decimalDigitStart+"-"+decimalDigitEnd);
      int de = digitEnd;
      int dde = decimalDigitEnd;
      if (!haveExponent && haveDecimalPoint && newScale == null) {
        // No more than 18 digits
        long lv = 0L;
        int expo = newScaleInt; // -(dde - decimalDigitStart);
        var digitCount = 0;
        if (mantInt <= MaxSafeInt) {
          lv = mantInt;
        } else {
          var vi = 0;
          for (vi = digitStart; vi < de; ++vi) {
            byte chvi = chars[vi];
            #if DEBUG
            if (!(chvi >= '0' && chvi <= '9')) {
              if (!throwException) {
                return null;
              } else { throw new ArgumentException("doesn't satisfy chvi>=" +
"\u0020'0'" + "\u0020 &&" + "\u0020chvi<= '9'");
}
            }
            #endif
            if (digitCount < 0 || digitCount >= 18) {
              digitCount = -1;
              break;
            } else if (digitCount > 0 || chvi != '0') {
              ++digitCount;
            }
            lv = checked((lv * 10) + (int)(chvi - '0'));
          }
          for (vi = decimalDigitStart; vi < dde; ++vi) {
            byte chvi = chars[vi];
            #if DEBUG
            if (!(chvi >= '0' && chvi <= '9')) {
              if (!throwException) {
                return null;
              } else { throw new ArgumentException("doesn't satisfy chvi>=" +
"\u0020'0'" + "\u0020 &&" + "\u0020chvi<= '9'");
}
            }
            #endif
            if (digitCount < 0 || digitCount >= 18) {
              digitCount = -1;
              break;
            } else if (digitCount > 0 || chvi != '0') {
              ++digitCount;
            }
            lv = checked((lv * 10) + (int)(chvi - '0'));
          }
        }
        if (negative) {
          lv = -lv;
        }
        if (digitCount >= 0 && (!negative || lv != 0)) {
          // DebugUtility.Log("lv="+lv+" expo="+expo);
          ret = EDecimal.Create(lv, (long)expo);
          if (ctx != null) {
            ret = EDecimal.GetMathValue(ctx).RoundAfterConversion(ret, ctx);
          }
          return ret;
        }
      }
      // Parse significand if it's "big"
      if (mantInt > MaxSafeInt) {
        if (haveDecimalPoint) {
          if (digitEnd - digitStart == 1 && chars[digitStart] == '0') {
            mant = EInteger.FromSubstring(
                chars,
                decimalDigitStart,
                decimalDigitEnd);
          } else {
            byte[] cdecstr = Extras.CharsConcat(
                chars,
                digitStart,
                digitEnd - digitStart,
                chars,
                decimalDigitStart,
                decimalDigitEnd - decimalDigitStart);
            mant = EInteger.FromString(cdecstr);
          }
        } else {
          mant = EInteger.FromSubstring(chars, digitStart, digitEnd);
        }
      }
      if (haveExponent && expInt > MaxSafeInt) {
        // Parse exponent if it's "big"
        exp = EInteger.FromSubstring(chars, expDigitStart, endStr);
        newScale = newScale ?? EInteger.FromInt32(newScaleInt);
        newScale = (expoffset < 0) ? newScale.Subtract(exp) :
          newScale.Add(exp);
      }
      FastIntegerFixed fastIntScale;
      FastIntegerFixed fastIntMant;
      fastIntScale = (newScale == null) ? FastIntegerFixed.FromInt32(
          newScaleInt) : FastIntegerFixed.FromBig(newScale);
      // DebugUtility.Log("fim="+ Chop(mant) + ", exp=" + fastIntScale);
      if (mant == null) {
        fastIntMant = FastIntegerFixed.FromInt32(mantInt);
      } else if (mant.CanFitInInt32()) {
        mantInt = mant.ToInt32Checked();
        fastIntMant = FastIntegerFixed.FromInt32(mantInt);
      } else {
        fastIntMant = FastIntegerFixed.FromBig(mant);
      }
      ret = new EDecimal(
        fastIntMant,
        fastIntScale,
        (byte)(negative ? BigNumberFlags.FlagNegative : 0));
      if (ctx != null) {
        // DebugUtility.Log("rounding");
        ret = EDecimal.GetMathValue(ctx).RoundAfterConversion(ret, ctx);
      }
      return ret;
    }
  }
}
