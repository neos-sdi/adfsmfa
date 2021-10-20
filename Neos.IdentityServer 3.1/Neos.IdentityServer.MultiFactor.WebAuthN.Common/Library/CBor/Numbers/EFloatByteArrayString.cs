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
  internal static class EFloatByteArrayString {
    internal static EFloat FromString(
      byte[] chars,
      int offset,
      int length,
      EContext ctx,
      bool throwException) {
      if (chars == null) {
        if (!throwException) {
          return null;
        } else {
          throw new ArgumentNullException(nameof(chars));
        }
      }
      if (offset < 0) {
        if (!throwException) {
          return null;
        } else { throw new FormatException("offset(" + offset + ") is not" +
"\u0020greater" + "\u0020or equal to 0");
}
      }
      if (offset > chars.Length) {
        if (!throwException) {
          return null;
        } else { throw new FormatException("offset(" + offset + ") is not" +
"\u0020less" + "\u0020or" + "\u0020equal to " + chars.Length);
}
      }
      if (length < 0) {
        if (!throwException) {
          return null;
        } else { throw new FormatException("length(" + length + ") is not" +
"\u0020greater or" + "\u0020equal to 0");
}
      }
      if (length > chars.Length) {
        if (!throwException) {
          return null;
        } else { throw new FormatException("length(" + length + ") is not" +
"\u0020less" + "\u0020or" + "\u0020equal to " + chars.Length);
}
      }
      if (chars.Length - offset < length) {
        if (!throwException) {
          return null;
        } else {
  throw new FormatException("str's length minus " + offset + "(" +
(chars.Length - offset) + ") is not greater or equal to " + length);
 }
      }
      EContext b64 = EContext.Binary64;
      if (ctx != null && ctx.HasMaxPrecision && ctx.HasExponentRange &&
        !ctx.IsSimplified && ctx.EMax.CompareTo(b64.EMax) <= 0 &&
        ctx.EMin.CompareTo(b64.EMin) >= 0 &&
        ctx.Precision.CompareTo(b64.Precision) <= 0) {
        int tmpoffset = offset;
        int endpos = offset + length;
        if (length == 0) {
          if (!throwException) {
            return null;
          } else {
            throw new FormatException();
          }
        }
        if (chars[tmpoffset] == '-' || chars[tmpoffset] == '+') {
          ++tmpoffset;
        }
        if (tmpoffset < endpos && ((chars[tmpoffset] >= '0' &&
              chars[tmpoffset] <= '9') || chars[tmpoffset] == '.')) {
          EFloat ef = DoubleEFloatFromString(
            chars,
            offset,
            length,
            ctx,
            throwException);
          if (ef != null) {
            return ef;
          }
        }
      }
      return EDecimal.FromString(
          chars,
          offset,
          length,
          EContext.Unlimited.WithSimplified(ctx != null && ctx.IsSimplified))
        .ToEFloat(ctx);
    }

    internal static EFloat DoubleEFloatFromString(
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
      if (length == 0) {
        if (!throwException) {
          return null;
        } else {
          throw new FormatException();
        }
      }
      int endStr = tmpoffset + length;
      var negative = false;
      var haveDecimalPoint = false;
      var haveDigits = false;
      var haveExponent = false;
      var newScaleInt = 0;
      var digitStart = 0;
      int i = tmpoffset;
      long mantissaLong = 0L;
      // Ordinary number
      if (chars[i] == '+' || chars[i] == '-') {
        if (chars[i] == '-') {
          negative = true;
        }
        ++i;
      }
      digitStart = i;
      int digitEnd = i;
      int decimalDigitStart = i;
      var haveNonzeroDigit = false;
      var decimalPrec = 0;
      int decimalDigitEnd = i;
      var nonzeroBeyondMax = false;
      var lastdigit = -1;
      // 768 is maximum precision of a decimal
      // half-ULP in double format
      var maxDecimalPrec = 768;
      if (length > 21) {
        int eminInt = ctx.EMin.ToInt32Checked();
        int emaxInt = ctx.EMax.ToInt32Checked();
        int precInt = ctx.Precision.ToInt32Checked();
        if (eminInt >= -14 && emaxInt <= 15) {
          maxDecimalPrec = (precInt <= 11) ? 21 : 63;
        } else if (eminInt >= -126 && emaxInt <= 127) {
          maxDecimalPrec = (precInt <= 24) ? 113 : 142;
        }
      }
      for (; i < endStr; ++i) {
        byte ch = chars[i];
        if (ch >= '0' && ch <= '9') {
          var thisdigit = (int)(ch - '0');
          haveDigits = true;
          haveNonzeroDigit |= thisdigit != 0;
          if (decimalPrec > maxDecimalPrec) {
            if (thisdigit != 0) {
              nonzeroBeyondMax = true;
            }
            if (!haveDecimalPoint) {
              // NOTE: Absolute value will not be more than
              // the byte[] portion's length, so will fit comfortably
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
            // the portion's length, so will fit comfortably
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
      EFloat ef1, ef2;
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
          // length without remaining beyond Int32 range
          if (expoffset < 0) {
            return EFloat.SignalUnderflow(ctx, negative, zeroMantissa);
          } else {
            return EFloat.SignalOverflow(ctx, negative, zeroMantissa);
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
        mantissaLong != Int64.MaxValue && (ctx == null ||
          !ctx.HasFlagsOrTraps)) {
        if (mantissaLong == 0) {
          EFloat ef = EFloat.Create(
              EInteger.Zero,
              EInteger.FromInt32(expInt));
          if (negative) {
            ef = ef.Negate();
          }
          return ef.RoundToPrecision(ctx);
        }
        var finalexp = (long)expInt + (long)newScaleInt;
        long ml = mantissaLong;
        if (finalexp >= -22 && finalexp <= 44) {
          var iexp = (int)finalexp;
          while (ml <= 900719925474099L && iexp > 22) {
            ml *= 10;
            --iexp;
          }
          int iabsexp = Math.Abs(iexp);
          if (ml < 9007199254740992L && iabsexp == 0) {
            return EFloat.FromInt64(negative ?
                -mantissaLong : mantissaLong).RoundToPrecision(ctx);
          } else if (ml < 9007199254740992L && iabsexp <= 22) {
            EFloat efn =
              EFloat.FromEInteger(NumberUtility.FindPowerOfTen(iabsexp));
            if (negative) {
              ml = -ml;
            }
            EFloat efml = EFloat.FromInt64(ml);
            if (iexp < 0) {
              return efml.Divide(efn, ctx);
            } else {
              return efml.Multiply(efn, ctx);
            }
          }
        }
        long adjexpUpperBound = finalexp + (decimalPrec - 1);
        long adjexpLowerBound = finalexp;
        if (adjexpUpperBound < -326) {
          return EFloat.SignalUnderflow(ctx, negative, zeroMantissa);
        } else if (adjexpLowerBound > 309) {
          return EFloat.SignalOverflow(ctx, negative, zeroMantissa);
        }
        if (negative) {
          mantissaLong = -mantissaLong;
        }
        long absfinalexp = Math.Abs(finalexp);
        ef1 = EFloat.Create(mantissaLong, (int)0);
        ef2 = EFloat.FromEInteger(NumberUtility.FindPowerOfTen(absfinalexp));
        if (finalexp < 0) {
          EFloat efret = ef1.Divide(ef2, ctx);
          /* Console.WriteLine("div " + ef1 + "/" + ef2 + " -> " + (efret));
          */ return efret;
        } else {
          return ef1.Multiply(ef2, ctx);
        }
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
      EInteger adjExpUpperBound = exp.Add(decimalPrec).Subtract(1);
      EInteger adjExpLowerBound = exp;
      // DebugUtility.Log("exp=" + adjExpLowerBound + "~" + (adjExpUpperBound));
      if (adjExpUpperBound.CompareTo(-326) < 0) {
        return EFloat.SignalUnderflow(ctx, negative, zeroMantissa);
      } else if (adjExpLowerBound.CompareTo(309) > 0) {
        return EFloat.SignalOverflow(ctx, negative, zeroMantissa);
      }
      if (zeroMantissa) {
        EFloat ef = EFloat.Create(
            EInteger.Zero,
            exp);
        if (negative) {
          ef = ef.Negate();
        }
        return ef.RoundToPrecision(ctx);
      } else if (decimalDigitStart != decimalDigitEnd) {
        if (digitEnd - digitStart == 1 && chars[digitStart] == '0') {
          mant = EInteger.FromSubstring(
              chars,
              decimalDigitStart,
              decimalDigitEnd);
        } else {
          byte[] ctmpstr = Extras.CharsConcat(
              chars,
              digitStart,
              digitEnd - digitStart,
              chars,
              decimalDigitStart,
              decimalDigitEnd - decimalDigitStart);
          mant = EInteger.FromString(ctmpstr);
        }
      } else {
        mant = EInteger.FromSubstring(chars, digitStart, digitEnd);
      }
      if (nonzeroBeyondMax) {
        mant = mant.Multiply(10).Add(1);
      }
      if (negative) {
        mant = mant.Negate();
      }
      return EDecimal.Create(mant, exp).ToEFloat(ctx);
    }
  }
}
