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

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor.Numbers
{
#pragma warning disable CS0618 // certain ERounding values are obsolete
  internal class RadixMath<T> : IRadixMath<T> {
    // Use given exponent
    private const int IntegerModeFixedScale = 1;
    // Use flexible exponent
    private const int IntegerModeRegular = 0;

    private const int SafeMin32 = -0x3ffffffe;
    private const int SafeMax32 = 0x3ffffffe;
    private const long SafeMin64 = -0x3ffffffffffffffeL;
    private const long SafeMax64 = 0x3ffffffffffffffeL;

    private static readonly int[] BitMasks = {
      0x7fffffff, 0x3fffffff, 0x1fffffff,
      0xfffffff, 0x7ffffff, 0x3ffffff, 0x1ffffff,
      0xffffff, 0x7fffff, 0x3fffff, 0x1fffff,
      0xfffff, 0x7ffff, 0x3ffff, 0x1ffff,
      0xffff, 0x7fff, 0x3fff, 0x1fff,
      0xfff, 0x7ff, 0x3ff, 0x1ff,
      0xff, 0x7f, 0x3f, 0x1f,
      0xf, 0x7, 0x3, 0x1,
    };

    private static readonly long[] BitMasks64 = {
      0x7fffffffffffffffL, 0x3fffffffffffffffL, 0x1fffffffffffffffL,
      0xfffffffffffffffL, 0x7ffffffffffffffL, 0x3ffffffffffffffL,
      0x1ffffffffffffffL,
      0xffffffffffffffL, 0x7fffffffffffffL, 0x3fffffffffffffL,
      0x1fffffffffffffL,
      0xfffffffffffffL, 0x7ffffffffffffL, 0x3ffffffffffffL, 0x1ffffffffffffL,
      0xffffffffffffL, 0x7fffffffffffL, 0x3fffffffffffL, 0x1fffffffffffL,
      0xfffffffffffL, 0x7ffffffffffL, 0x3ffffffffffL, 0x1ffffffffffL,
      0xffffffffffL, 0x7fffffffffL, 0x3fffffffffL, 0x1fffffffffL,
      0xfffffffffL, 0x7ffffffffL, 0x3ffffffffL, 0x1ffffffffL,
      0xffffffffL, 0x7fffffff, 0x3fffffff, 0x1fffffff,
      0xfffffff, 0x7ffffff, 0x3ffffff, 0x1ffffff,
      0xffffff, 0x7fffff, 0x3fffff, 0x1fffff,
      0xfffff, 0x7ffff, 0x3ffff, 0x1ffff,
      0xffff, 0x7fff, 0x3fff, 0x1fff,
      0xfff, 0x7ff, 0x3ff, 0x1ff,
      0xff, 0x7f, 0x3f, 0x1f,
      0xf, 0x7, 0x3, 0x1,
    };

    private static readonly int[] OverflowMaxes = {
      2147483647, 214748364, 21474836,
      2147483, 214748, 21474, 2147, 214, 21, 2,
    };

    private static readonly EInteger ValueMinusOne = EInteger.Zero -
      EInteger.One;

    private static readonly int[] ValueTenPowers = {
      1, 10, 100, 1000, 10000, 100000,
      1000000, 10000000, 100000000,
      1000000000,
    };

    private static readonly long[] OverflowMaxes64 = {
      9223372036854775807L, 922337203685477580L,
      92233720368547758L, 9223372036854775L,
      922337203685477L, 92233720368547L,
      9223372036854L, 922337203685L,
      92233720368L, 9223372036L,
      922337203L, 92233720, 9223372,
      922337, 92233, 9223, 922, 92, 9,
    };

    private static readonly long[] ValueTenPowers64 = {
      1, 10, 100, 1000,
      10000, 100000, 1000000,
      10000000, 100000000, 1000000000,
      10000000000L, 100000000000L,
      1000000000000L, 10000000000000L,
      100000000000000L, 1000000000000000L,
      10000000000000000L, 100000000000000000L,
      1000000000000000000L,
    };

    private readonly IRadixMathHelper<T> helper;
    private readonly int support;
    private readonly int thisRadix;

    // Conservative maximum base-10 radix power for
    // TryMultiplyByRadixPower; derived from
    // Int32.MaxValue*8/3 (8 is the number of bits in a byte;
    // 3 is a conservative estimate of log(10)/log(2).)
    private static EInteger valueMaxDigits = (EInteger)5726623058L;

    public RadixMath(IRadixMathHelper<T> helper) {
      this.helper = helper;
      this.support = helper.GetArithmeticSupport();
      this.thisRadix = helper.GetRadix();
    }

    public T Add(T thisValue, T other, EContext ctx) {
      if ((object)thisValue == null) {
        throw new ArgumentNullException(nameof(thisValue));
      }
      if ((object)other == null) {
        throw new ArgumentNullException(nameof(other));
      }
      return this.AddEx(thisValue, other, ctx, false);
    }
    private FastInteger DigitLengthUpperBoundForBitPrecision(FastInteger prec) {
      FastInteger result;
      if (this.thisRadix == 2) {
        result = prec;
      } else {
        if (this.thisRadix == 10 && prec.CompareToInt(2135) <= 0) {
          int value = checked(1 + ((prec.ToInt32() * 631305) >> 21));
          result = new FastInteger(value);
        } else if (this.thisRadix == 10 && prec.CompareToInt(6432162) <= 0) {
          // Approximation of ln(2)/ln(10)
          int value = 1 + (int)(((long)prec.ToInt32() * 661971961083L) >> 41);
          result = new FastInteger(value);
        } else {
          return this.helper.GetDigitLength(
              EInteger.One.ShiftLeft(prec.ToEInteger()).Subtract(1));
        }
      }
      return result;
    }
    private T AddEx32Bit(
      int expcmp,
      FastIntegerFixed op1Exponent,
      FastIntegerFixed op1Mantissa,
      FastIntegerFixed op2Exponent,
      FastIntegerFixed op2Mantissa,
      FastIntegerFixed resultExponent,
      int thisFlags,
      int otherFlags,
      EContext ctx) {
      T retval = default(T);
      if ((expcmp == 0 || (op1Exponent.CanFitInInt32() &&
            op2Exponent.CanFitInInt32())) &&
        op1Mantissa.CanFitInInt32() && op2Mantissa.CanFitInInt32() &&
        (thisFlags & BigNumberFlags.FlagNegative) == (otherFlags &
          BigNumberFlags.FlagNegative)) {
        int negflag = thisFlags & BigNumberFlags.FlagNegative;
        var e1int = 0;
        var e2int = 0;
        if (expcmp != 0) {
          e1int = op1Exponent.ToInt32();
          e2int = op2Exponent.ToInt32();
        }
        int m1, m2;
        var haveRetval = false;
        if (expcmp == 0 || (e1int >= SafeMin32 && e1int <= SafeMax32 &&
            e2int >= SafeMin32 && e2int <= SafeMax32)) {
          int ediff = (expcmp == 0) ? 0 : ((e1int > e2int) ? (e1int - e2int) :
              (e2int - e1int));
          int radix = this.thisRadix;
          if (expcmp == 0) {
            m1 = op1Mantissa.ToInt32();
            m2 = op2Mantissa.ToInt32();
            if (m2 <= Int32.MaxValue - m1) {
              m1 += m2;
              retval = this.helper.CreateNewWithFlagsFastInt(
                  FastIntegerFixed.FromInt32(m1),
                  resultExponent,
                  negflag);
              haveRetval = true;
            }
          } else if (ediff <= 9 && radix == 10) {
            int power = ValueTenPowers[ediff];
            int maxoverflow = OverflowMaxes[ediff];
            if (expcmp > 0) {
              m1 = op1Mantissa.ToInt32();
              m2 = op2Mantissa.ToInt32();
              if (m1 == 0) {
                retval = this.helper.CreateNewWithFlagsFastInt(
                    op2Mantissa,
                    op2Exponent,
                    otherFlags);
              } else if (m1 <= maxoverflow) {
                m1 *= power;
                if (m2 <= Int32.MaxValue - m1) {
                  m1 += m2;
                  retval = this.helper.CreateNewWithFlagsFastInt(
                      FastIntegerFixed.FromInt32(m1),
                      resultExponent,
                      negflag);
                  haveRetval = true;
                }
              }
            } else {
              m1 = op1Mantissa.ToInt32();
              m2 = op2Mantissa.ToInt32();
              if (m2 == 0) {
                retval = this.helper.CreateNewWithFlagsFastInt(
                    op1Mantissa,
                    op1Exponent,
                    thisFlags);
              }
              if (m2 <= maxoverflow) {
                m2 *= power;
                if (m1 <= Int32.MaxValue - m2) {
                  m2 += m1;
                  retval = this.helper.CreateNewWithFlagsFastInt(
                      FastIntegerFixed.FromInt32(m2),
                      resultExponent,
                      negflag);
                  haveRetval = true;
                }
              }
            }
          } else if (ediff <= 30 && radix == 2) {
            int mask = BitMasks[ediff];
            if (expcmp > 0) {
              m1 = op1Mantissa.ToInt32();
              m2 = op2Mantissa.ToInt32();
              if (m1 == 0) {
                retval = this.helper.CreateNewWithFlagsFastInt(
                    op2Mantissa,
                    op2Exponent,
                    otherFlags);
              } else if ((m1 & mask) == m1) {
                m1 <<= ediff;
                if (m2 <= Int32.MaxValue - m1) {
                  m1 += m2;
                  retval = this.helper.CreateNewWithFlagsFastInt(
                      FastIntegerFixed.FromInt32(m1),
                      resultExponent,
                      negflag);
                  haveRetval = true;
                }
              }
            } else {
              m1 = op1Mantissa.ToInt32();
              m2 = op2Mantissa.ToInt32();
              if (m2 == 0) {
                retval = this.helper.CreateNewWithFlagsFastInt(
                    op1Mantissa,
                    op1Exponent,
                    thisFlags);
              } else if ((m2 & mask) == m2) {
                m2 <<= ediff;
                if (m1 <= Int32.MaxValue - m2) {
                  m2 += m1;
                  retval = this.helper.CreateNewWithFlagsFastInt(
                      FastIntegerFixed.FromInt32(m2),
                      resultExponent,
                      negflag);
                  haveRetval = true;
                }
              }
            }
          }
        }
        if (haveRetval) {
          if (!IsNullOrSimpleContext(ctx)) {
            if (resultExponent.IsValueZero &&
              this.IsNullOrInt32FriendlyContext(ctx)) {
              return retval;
            }
            retval = this.RoundToPrecision(retval, ctx);
          }
          return retval;
        }
      }
      if ((thisFlags & BigNumberFlags.FlagNegative) != 0 &&
        (otherFlags & BigNumberFlags.FlagNegative) == 0) {
        FastIntegerFixed fftmp;
        fftmp = op1Exponent;
        op1Exponent = op2Exponent;
        op2Exponent = fftmp;
        fftmp = op1Mantissa;
        op1Mantissa = op2Mantissa;
        op2Mantissa = fftmp;
        int tmp;
        tmp = thisFlags;
        thisFlags = otherFlags;
        otherFlags = tmp;
        expcmp = -expcmp;
        resultExponent = expcmp < 0 ? op1Exponent : op2Exponent;
      }
      if ((expcmp == 0 || (op1Exponent.CanFitInInt32() &&
            op2Exponent.CanFitInInt32())) &&
        op1Mantissa.CanFitInInt32() && op2Mantissa.CanFitInInt32() &&
        (thisFlags & BigNumberFlags.FlagNegative) == 0 &&
        (otherFlags & BigNumberFlags.FlagNegative) != 0 &&
        !op2Mantissa.IsValueZero && !op1Mantissa.IsValueZero) {
        var e1int = 0;
        var e2int = 0;
        var result = 0;
        if (expcmp != 0) {
          e1int = op1Exponent.ToInt32();
          e2int = op2Exponent.ToInt32();
        }
        int m1, m2;
        var haveRetval = false;
        if (expcmp == 0 || (e1int >= SafeMin32 && e1int <= SafeMax32 &&
            e2int >= SafeMin32 && e2int <= SafeMax32)) {
          int ediff = (expcmp == 0) ? 0 : ((e1int > e2int) ? (e1int - e2int) :
              (e2int - e1int));
          int radix = this.thisRadix;
          if (expcmp == 0) {
            m1 = op1Mantissa.ToInt32();
            m2 = op2Mantissa.ToInt32();
            if (Int32.MinValue + m2 <= m1 && m1 >= m2) {
              m1 -= m2;
              result = m1;
              retval = this.helper.CreateNewWithFlagsFastInt(
                  FastIntegerFixed.FromInt32(m1),
                  resultExponent,
                  0);
              haveRetval = true;
            }
          } else if (radix == 10 && ediff <= 9) {
            int power = ValueTenPowers[ediff];
            int maxoverflow = OverflowMaxes[ediff];
            m1 = op1Mantissa.ToInt32();
            m2 = op2Mantissa.ToInt32();
            var negbit = false;
            var multed = false;
            if (expcmp < 0) {
              if (m2 <= maxoverflow) {
                m2 *= power;
                multed = true;
              }
            } else {
              if (m1 <= maxoverflow) {
                m1 *= power;
                multed = true;
              }
            }
            if (multed && Int32.MinValue + m2 <= m1) {
              m1 -= m2;
              if (m1 != Int32.MinValue) {
                negbit = m1 < 0;
                result = Math.Abs(m1);
                retval = this.helper.CreateNewWithFlagsFastInt(
                    FastIntegerFixed.FromInt32(result),
                    resultExponent,
                    negbit ? BigNumberFlags.FlagNegative : 0);
                haveRetval = true;
              }
            }
          }
        }
        if (haveRetval && result != 0) {
          if (!IsNullOrSimpleContext(ctx)) {
            if (resultExponent.IsValueZero &&
              this.IsNullOrInt32FriendlyContext(ctx)) {
              return retval;
            }
            retval = this.RoundToPrecision(retval, ctx);
          }
          return retval;
        }
        if (haveRetval && result == 0) {
          if (resultExponent.IsValueZero &&
            this.IsNullOrInt32FriendlyContext(ctx)) {
            return retval;
          }
          // DebugUtility.Log("haveRetval, result=0, [" + thisFlags + "," +
          // op1Mantissa + "," + op1Exponent + "] + [" + otherFlags + "," +
          // op2Mantissa + "," + op2Exponent + "], retval="+retval);
        }
      }
      return default(T);
    }

    private T AddEx64Bit(
      long expcmp,
      FastIntegerFixed op1Exponent,
      FastIntegerFixed op1Mantissa,
      FastIntegerFixed op2Exponent,
      FastIntegerFixed op2Mantissa,
      FastIntegerFixed resultExponent,
      int thisFlags,
      int otherFlags,
      EContext ctx) {
      T retval = default(T);
      if ((expcmp == 0 || (op1Exponent.CanFitInInt64() &&
            op2Exponent.CanFitInInt64())) &&
        op1Mantissa.CanFitInInt64() && op2Mantissa.CanFitInInt64() &&
        (thisFlags & BigNumberFlags.FlagNegative) == (otherFlags &
          BigNumberFlags.FlagNegative)) {
        int negflag = thisFlags & BigNumberFlags.FlagNegative;
        long e1long = 0;
        long e2long = 0;
        if (expcmp != 0) {
          e1long = op1Exponent.ToInt64();
          e2long = op2Exponent.ToInt64();
        }
        long m1, m2;
        var haveRetval = false;
        if (expcmp == 0 || (e1long >= SafeMin64 && e1long <= SafeMax64 &&
            e2long >= SafeMin64 && e2long <= SafeMax64)) {
          long ediffLong = (expcmp == 0) ? 0 : ((e1long > e2long) ?
              (e1long - e2long) : (e2long - e1long));
          int radix = this.thisRadix;
          if (expcmp == 0) {
            m1 = op1Mantissa.ToInt64();
            m2 = op2Mantissa.ToInt64();
            if (m2 <= Int64.MaxValue - m1) {
              m1 += m2;
              retval = this.helper.CreateNewWithFlagsFastInt(
                  FastIntegerFixed.FromInt64(m1),
                  resultExponent,
                  negflag);
              haveRetval = true;
            }
          } else if (ediffLong < ValueTenPowers64.Length && radix == 10) {
            long power = ValueTenPowers64[(int)ediffLong];
            long maxoverflow = OverflowMaxes64[(int)ediffLong];
            if (expcmp > 0) {
              m1 = op1Mantissa.ToInt64();
              m2 = op2Mantissa.ToInt64();
              if (m1 == 0) {
                retval = this.helper.CreateNewWithFlagsFastInt(
                    op2Mantissa,
                    op2Exponent,
                    otherFlags);
              } else if (m1 <= maxoverflow) {
                m1 *= power;
                if (m2 <= Int64.MaxValue - m1) {
                  m1 += m2;
                  retval = this.helper.CreateNewWithFlagsFastInt(
                      FastIntegerFixed.FromInt64(m1),
                      resultExponent,
                      negflag);
                  haveRetval = true;
                }
              }
            } else {
              m1 = op1Mantissa.ToInt64();
              m2 = op2Mantissa.ToInt64();
              if (m2 == 0) {
                retval = this.helper.CreateNewWithFlagsFastInt(
                    op1Mantissa,
                    op1Exponent,
                    thisFlags);
              }
              if (m2 <= maxoverflow) {
                m2 *= power;
                if (m1 <= Int64.MaxValue - m2) {
                  m2 += m1;
                  retval = this.helper.CreateNewWithFlagsFastInt(
                      FastIntegerFixed.FromInt64(m2),
                      resultExponent,
                      negflag);
                  haveRetval = true;
                }
              }
            }
          } else if (ediffLong < BitMasks64.Length && radix == 2) {
            long mask = BitMasks64[(int)ediffLong];
            if (expcmp > 0) {
              m1 = op1Mantissa.ToInt64();
              m2 = op2Mantissa.ToInt64();
              if (m1 == 0) {
                retval = this.helper.CreateNewWithFlagsFastInt(
                    op2Mantissa,
                    op2Exponent,
                    otherFlags);
              } else if ((m1 & mask) == m1) {
                m1 <<= (int)ediffLong;
                if (m2 <= Int64.MaxValue - m1) {
                  m1 += m2;
                  retval = this.helper.CreateNewWithFlagsFastInt(
                      FastIntegerFixed.FromInt64(m1),
                      resultExponent,
                      negflag);
                  haveRetval = true;
                }
              }
            } else {
              m1 = op1Mantissa.ToInt64();
              m2 = op2Mantissa.ToInt64();
              if (m2 == 0) {
                retval = this.helper.CreateNewWithFlagsFastInt(
                    op1Mantissa,
                    op1Exponent,
                    thisFlags);
              } else if ((m2 & mask) == m2) {
                m2 <<= (int)ediffLong;
                if (m1 <= Int64.MaxValue - m2) {
                  m2 += m1;
                  retval = this.helper.CreateNewWithFlagsFastInt(
                      FastIntegerFixed.FromInt64(m2),
                      resultExponent,
                      negflag);
                  haveRetval = true;
                }
              }
            }
          }
        }
        if (haveRetval) {
          if (!IsNullOrSimpleContext(ctx)) {
            retval = this.RoundToPrecision(retval, ctx);
          }
          return retval;
        }
      }
      if ((thisFlags & BigNumberFlags.FlagNegative) != 0 &&
        (otherFlags & BigNumberFlags.FlagNegative) == 0) {
        FastIntegerFixed fftmp;
        fftmp = op1Exponent;
        op1Exponent = op2Exponent;
        op2Exponent = fftmp;
        fftmp = op1Mantissa;
        op1Mantissa = op2Mantissa;
        op2Mantissa = fftmp;
        int tmp;
        tmp = thisFlags;
        thisFlags = otherFlags;
        otherFlags = tmp;
        expcmp = -expcmp;
        resultExponent = expcmp < 0 ? op1Exponent : op2Exponent;
      }
      if ((expcmp == 0 || (op1Exponent.CanFitInInt64() &&
            op2Exponent.CanFitInInt64())) &&
        op1Mantissa.CanFitInInt64() && op2Mantissa.CanFitInInt64() &&
        (thisFlags & BigNumberFlags.FlagNegative) == 0 &&
        (otherFlags & BigNumberFlags.FlagNegative) != 0 &&
        !op2Mantissa.IsValueZero && !op1Mantissa.IsValueZero) {
        long e1long = 0;
        long e2long = 0;
        long result = 0;
        if (expcmp != 0) {
          e1long = op1Exponent.ToInt64();
          e2long = op2Exponent.ToInt64();
        }
        long m1, m2;
        var haveRetval = false;
        if (expcmp == 0 || (e1long >= SafeMin64 && e1long <= SafeMax64 &&
            e2long >= SafeMin64 && e2long <= SafeMax64)) {
          long ediffLong = (expcmp == 0) ? 0 : ((e1long > e2long) ?
              (e1long - e2long) : (e2long - e1long));
          int radix = this.thisRadix;
          if (expcmp == 0) {
            m1 = op1Mantissa.ToInt64();
            m2 = op2Mantissa.ToInt64();
            if (Int64.MinValue + m2 <= m1 && m1 >= m2) {
              m1 -= m2;
              result = m1;
              retval = this.helper.CreateNewWithFlagsFastInt(
                  FastIntegerFixed.FromInt64(m1),
                  resultExponent,
                  0);
              haveRetval = true;
            }
          } else if (radix == 10 && ediffLong < ValueTenPowers64.Length) {
            long power = ValueTenPowers64[(int)ediffLong];
            long maxoverflow = OverflowMaxes64[(int)ediffLong];
            m1 = op1Mantissa.ToInt64();
            m2 = op2Mantissa.ToInt64();
            var negbit = false;
            var multed = false;
            if (expcmp < 0) {
              if (m2 <= maxoverflow) {
                m2 *= power;
                multed = true;
              }
            } else {
              if (m1 <= maxoverflow) {
                m1 *= power;
                multed = true;
              }
            }
            if (multed && Int64.MinValue + m2 <= m1) {
              m1 -= m2;
              if (m1 != Int64.MinValue) {
                negbit = m1 < 0;
                result = Math.Abs(m1);
                retval = this.helper.CreateNewWithFlagsFastInt(
                    FastIntegerFixed.FromInt64(result),
                    resultExponent,
                    negbit ? BigNumberFlags.FlagNegative : 0);
                haveRetval = true;
              }
            }
          }
        }
        if (haveRetval && result != 0) {
          if (!IsNullOrSimpleContext(ctx)) {
            retval = this.RoundToPrecision(retval, ctx);
          }
          return retval;
        }
      }
      return default(T);
    }

    public T AddEx(
      T thisValue,
      T other,
      EContext ctx,
      bool roundToOperandPrecision) {
      int thisFlags = this.helper.GetFlags(thisValue);
      int otherFlags = this.helper.GetFlags(other);
      if (((thisFlags | otherFlags) & BigNumberFlags.FlagSpecial) != 0) {
        T result = this.HandleNotANumber(thisValue, other, ctx);
        if ((object)result != (object)default(T)) {
          return result;
        }
        if ((thisFlags & BigNumberFlags.FlagInfinity) != 0) {
          if ((otherFlags & BigNumberFlags.FlagInfinity) != 0) {
            if ((thisFlags & BigNumberFlags.FlagNegative) != (otherFlags &
                BigNumberFlags.FlagNegative)) {
              return this.SignalInvalid(ctx);
            }
          }
          return thisValue;
        }
        if ((otherFlags & BigNumberFlags.FlagInfinity) != 0) {
          return other;
        }
      }
      FastIntegerFixed op1Exponent = this.helper.GetExponentFastInt(thisValue);
      FastIntegerFixed op2Exponent = this.helper.GetExponentFastInt(other);
      FastIntegerFixed op1Mantissa = this.helper.GetMantissaFastInt(thisValue);
      FastIntegerFixed op2Mantissa = this.helper.GetMantissaFastInt(other);
      int expcmp = op1Exponent.CompareTo(op2Exponent);
      FastIntegerFixed resultExponent = expcmp < 0 ? op1Exponent : op2Exponent;
      T retval = default(T);
      if ((thisFlags & BigNumberFlags.FlagNegative) == 0 &&
        (otherFlags & BigNumberFlags.FlagNegative) == 0) {
        if (expcmp < 0 && op2Mantissa.IsValueZero) {
          return IsNullOrSimpleContext(ctx) ?
            thisValue : this.RoundToPrecision(thisValue, ctx);
        } else if (expcmp >= 0 && op1Mantissa.IsValueZero) {
          return IsNullOrSimpleContext(ctx) ?
            other : this.RoundToPrecision(other, ctx);
        }
      }
      if (!roundToOperandPrecision) {
        retval = this.AddEx32Bit(
            expcmp,
            op1Exponent,
            op1Mantissa,
            op2Exponent,
            op2Mantissa,
            resultExponent,
            thisFlags,
            otherFlags,
            ctx);
        if ((object)retval != (object)default(T)) {
          return retval;
        }
        retval = this.AddEx64Bit(
            expcmp,
            op1Exponent,
            op1Mantissa,
            op2Exponent,
            op2Mantissa,
            resultExponent,
            thisFlags,
            otherFlags,
            ctx);
        if ((object)retval != (object)default(T)) {
          return retval;
        }
      }
      if (expcmp == 0) {
        retval = this.AddCore2(
            op1Mantissa,
            op2Mantissa,
            op1Exponent,
            thisFlags,
            otherFlags,
            ctx);
        if (!IsNullOrSimpleContext(ctx)) {
          retval = this.RoundToPrecision(retval, ctx);
        }
      } else {
        retval = this.AddExDiffExp(
            op1Exponent,
            op1Mantissa,
            op2Exponent,
            op2Mantissa,
            thisFlags,
            otherFlags,
            ctx,
            expcmp,
            roundToOperandPrecision);
      }
      return retval;
    }

    public int CompareTo(T thisValue, T otherValue) {
      if (otherValue == null) {
        return 1;
      }
      int thisFlags = this.helper.GetFlags(thisValue);
      int otherFlags = this.helper.GetFlags(otherValue);
      if (((thisFlags | otherFlags) & BigNumberFlags.FlagSpecial) != 0) {
        return CompareToHandleSpecial2(
            thisFlags,
            otherFlags);
      }
      return CompareToInternal(thisValue, otherValue, true, this.helper);
    }

    public T CompareToWithContext(
      T thisValue,
      T otherValue,
      bool treatQuietNansAsSignaling,
      EContext ctx) {
      if (otherValue == null) {
        return this.SignalInvalid(ctx);
      }
      T result = this.CompareToHandleSpecial(
          thisValue,
          otherValue,
          treatQuietNansAsSignaling,
          ctx);
      if ((object)result != (object)default(T)) {
        return result;
      }
      int cmp = CompareToInternal(thisValue, otherValue, false, this.helper);
      return (cmp == -2) ? this.SignalInvalidWithMessage(
          ctx,
          "Out of memory ") :
        this.ValueOf(this.CompareTo(thisValue, otherValue), null);
    }

    public T Divide(T thisValue, T divisor, EContext ctx) {
      return this.DivideInternal(
          thisValue,
          divisor,
          ctx,
          IntegerModeRegular,
          EInteger.Zero);
    }

    public T DivideToExponent(
      T thisValue,
      T divisor,
      EInteger desiredExponent,
      EContext ctx) {
      if (ctx != null && !ctx.ExponentWithinRange(desiredExponent)) {
        return this.SignalInvalidWithMessage(
            ctx,
            "Exponent not within exponent range: " + desiredExponent);
      }
      EContext ctx2 = (ctx == null) ?
        EContext.ForRounding(ERounding.HalfDown) :
        ctx.WithUnlimitedExponents().WithPrecision(0);
      T ret = this.DivideInternal(
          thisValue,
          divisor,
          ctx2,
          IntegerModeFixedScale,
          desiredExponent);
      if (!ctx2.HasMaxPrecision && this.IsFinite(ret)) {
        // If a precision is given, call Quantize to ensure
        // that the value fits the precision
        ret = this.Quantize(ret, ret, ctx2);
        if ((ctx2.Flags & EContext.FlagInvalid) != 0) {
          ctx2.Flags = EContext.FlagInvalid;
        }
      }
      if (ctx != null && ctx.HasFlags) {
        ctx.Flags |= ctx2.Flags;
      }
      return ret;
    }

    public T DivideToIntegerNaturalScale(
      T thisValue,
      T divisor,
      EContext ctx) {
      FastInteger desiredScale =
        FastInteger.FromBig(this.helper.GetExponent(thisValue))
        .SubtractBig(this.helper.GetExponent(divisor));
      EContext ctx2 =
        EContext.ForRounding(ERounding.Down).WithBigPrecision(ctx == null ?
          EInteger.Zero : ctx.Precision).WithBlankFlags();
      T ret = this.DivideInternal(
          thisValue,
          divisor,
          ctx2,
          IntegerModeFixedScale,
          EInteger.Zero);
      if ((ctx2.Flags & (EContext.FlagInvalid |
            EContext.FlagDivideByZero)) != 0) {
        if (ctx != null && ctx.HasFlags) {
          ctx.Flags |= EContext.FlagInvalid | EContext.FlagDivideByZero;
        }
        return ret;
      }
      bool neg = (this.helper.GetSign(thisValue) < 0) ^
        (this.helper.GetSign(divisor) < 0);

      // Now the exponent's sign can only be 0 or positive
      if (this.helper.GetMantissa(ret).IsZero) {
        // Value is 0, so just change the exponent
        // to the preferred one
        EInteger dividendExp = this.helper.GetExponent(thisValue);
        EInteger divisorExp = this.helper.GetExponent(divisor);
        ret = this.helper.CreateNewWithFlags(
            EInteger.Zero,
            dividendExp - (EInteger)divisorExp,
            this.helper.GetFlags(ret));
      } else {
        if (desiredScale.Sign < 0) {
          // Desired scale is negative, shift left
          desiredScale.Negate();
          EInteger bigmantissa = this.helper.GetMantissa(ret);
          bigmantissa = this.TryMultiplyByRadixPower(bigmantissa,
              desiredScale);
          if (bigmantissa == null) {
            return this.SignalInvalidWithMessage(
                ctx,
                "Result requires too much memory");
          }
          EInteger exponentDivisor = this.helper.GetExponent(divisor);
          ret = this.helper.CreateNewWithFlags(
              bigmantissa,
              this.helper.GetExponent(thisValue) - (EInteger)exponentDivisor,
              this.helper.GetFlags(ret));
        } else if (desiredScale.Sign > 0) {
          // Desired scale is positive, shift away zeros
          // but not after scale is reached
          EInteger bigmantissa = this.helper.GetMantissa(ret);
          FastInteger fastexponent =
            FastInteger.FromBig(this.helper.GetExponent(ret));
          var bigradix = (EInteger)this.thisRadix;
          while (true) {
            if (desiredScale.CompareTo(fastexponent) == 0) {
              break;
            }
            EInteger bigrem;
            EInteger bigquo;
            {
              EInteger[] divrem = bigmantissa.DivRem(bigradix);
              bigquo = divrem[0];
              bigrem = divrem[1];
            }
            if (!bigrem.IsZero) {
              break;
            }
            bigmantissa = bigquo;
            fastexponent.Increment();
          }
          ret = this.helper.CreateNewWithFlags(
              bigmantissa,
              fastexponent.ToEInteger(),
              this.helper.GetFlags(ret));
        }
      }
      if (ctx != null) {
        ret = this.RoundToPrecision(ret, ctx);
      }
      ret = this.EnsureSign(ret, neg);
      return ret;
    }

    private T SignalUnderflow(EContext ec, bool negative, bool
      zeroSignificand) {
      EInteger eTiny = ec.EMin.Subtract(ec.Precision.Subtract(1));
      eTiny = eTiny.Subtract(2); // subtract 2 from proper eTiny to
      // trigger underflow (2, rather than 1, because of HalfUp mode)
      T ret = this.helper.CreateNewWithFlags(
          zeroSignificand ? EInteger.Zero : EInteger.One,
          eTiny,
          negative ? BigNumberFlags.FlagNegative : 0);
      // DebugUtility.Log(ret+" underflow "+ec);
      return this.RoundToPrecision(ret, ec);
    }

    public T DivideToIntegerZeroScale(
      T thisValue,
      T divisor,
      EContext ctx) {
      EContext ctx2 = EContext.ForRounding(ERounding.Down)
        .WithBigPrecision(ctx == null ? EInteger.Zero :
          ctx.Precision).WithBlankFlags();
      T ret = this.DivideInternal(
          thisValue,
          divisor,
          ctx2,
          IntegerModeFixedScale,
          EInteger.Zero);
      if ((ctx2.Flags & (EContext.FlagInvalid |
            EContext.FlagDivideByZero)) != 0) {
        if (ctx.HasFlags) {
          ctx.Flags |= ctx2.Flags & (EContext.FlagInvalid |
              EContext.FlagDivideByZero);
        }
        return ret;
      }
      if (ctx != null) {
        ctx2 = ctx.WithBlankFlags().WithUnlimitedExponents();
        ret = this.RoundToPrecision(ret, ctx2);
        if ((ctx2.Flags & EContext.FlagRounded) != 0) {
          return this.SignalInvalid(ctx);
        }
      }
      return ret;
    }

    private EInteger WorkingDigits(EInteger workingBits) {
      int radix = this.thisRadix;
      if (radix <= 2) {
        { return workingBits;
        }
      }
      int ibits = NumberUtility.BitLength(radix) - 1;
      return workingBits.Divide(ibits).Add(1);
    }

    public T Exp(T thisValue, EContext ctx) {
      return this.Exp(thisValue, ctx, ctx == null ? null : ctx.Precision);
    }

    private T Exp(T thisValue, EContext ctx, EInteger workingPrecision) {
      if (ctx == null) {
        return this.SignalInvalidWithMessage(ctx, "ctx is null");
      }
      if (!ctx.HasMaxPrecision) {
        return this.SignalInvalidWithMessage(
            ctx,
            "ctx has unlimited precision");
      }
      int flags = this.helper.GetFlags(thisValue);
      if ((flags & BigNumberFlags.FlagSignalingNaN) != 0) {
        // NOTE: Returning a signaling NaN is independent of
        // rounding mode
        return this.SignalingNaNInvalid(thisValue, ctx);
      }
      if ((flags & BigNumberFlags.FlagQuietNaN) != 0) {
        // NOTE: Returning a quiet NaN is independent of
        // rounding mode
        return this.ReturnQuietNaN(thisValue, ctx);
      }
      EContext ctxCopy = ctx.WithBlankFlags();
      if ((flags & BigNumberFlags.FlagInfinity) != 0) {
        if ((flags & BigNumberFlags.FlagNegative) != 0) {
          T retval = this.helper.CreateNewWithFlags(
              EInteger.Zero,
              EInteger.Zero,
              0);
          retval = this.RoundToPrecision(
              retval,
              ctxCopy);
          if (ctx.HasFlags) {
            ctx.Flags |= ctxCopy.Flags;
          }
          return retval;
        }
        return thisValue;
      }
      int sign = this.helper.GetSign(thisValue);
      T one = this.helper.ValueOf(1);
      EInteger guardDigits = this.thisRadix == 2 ?
        workingPrecision.Add(10) : (EInteger)10;
      EContext ctxdiv = SetPrecisionIfLimited(
          ctx,
          workingPrecision + guardDigits)
        .WithRounding(ERounding.HalfEven).WithBlankFlags();
      if (sign == 0) {
        thisValue = this.RoundToPrecision(one, ctxCopy);
      } else if (sign > 0 && this.CompareTo(thisValue, one) <= 0) {
        T closeToZero = this.Divide(
            this.helper.ValueOf(1),
            this.helper.ValueOf(0x800),
            null);
        if (this.IsFinite(closeToZero) &&
          this.CompareTo(thisValue, closeToZero) <= 0) {
          // Call ExpInternal for magnitudes close to 0, to avoid
          // issues when thisValue's magnitude is extremely
          // close to 0
          thisValue = this.ExpInternalVeryCloseToZero(
              thisValue,
              ctxdiv.Precision,
              ctxCopy);
          if (ctx.HasFlags) {
            ctx.Flags |= EContext.FlagInexact |
              EContext.FlagRounded | ctxCopy.Flags;
          }
          return thisValue;
        }
        thisValue = this.ExpInternal(
            thisValue,
            ctxdiv.Precision,
            ctxCopy);
        if (ctx.HasFlags) {
          ctx.Flags |= EContext.FlagInexact | EContext.FlagRounded;
        }
      } else if (sign < 0) {
        T closeToZero = this.Divide(
            this.helper.ValueOf(-1),
            this.helper.ValueOf(0x800),
            null);
        // DebugUtility.Log("ctz="+closeToZero+", wp="+
        // workingPrecision+
        // " ctxp="+ctx.Precision);
        if (this.IsFinite(closeToZero) &&
          this.CompareTo(thisValue, closeToZero) >= 0) {
          // Call ExpInternal for magnitudes close to 0, to avoid
          // issues when thisValue's magnitude is extremely
          // close to 0
          // DebugUtility.Log("very ctx: thisValue="+thisValue);
          thisValue = this.ExpInternalVeryCloseToZero(
              thisValue,
              ctxdiv.Precision,
              ctxCopy);
          if (ctx.HasFlags) {
            ctx.Flags |= EContext.FlagInexact |
              EContext.FlagRounded | ctxCopy.Flags;
          }
          return thisValue;
        }
        // DebugUtility.Log("ordinary: thisValue="+thisValue);
        // exp(x) = 1/exp(-x) where x<0
        T val = this.Exp(this.NegateRaw(thisValue), ctxdiv);
        if ((ctxdiv.Flags & EContext.FlagOverflow) != 0 ||
          !this.IsFinite(val)) {
          // Overflow, try again with expanded exponent range
          EInteger newMax;
          ctxdiv.Flags = 0;
          newMax = ctx.EMax;
          EInteger eintExpdiff = ctx.EMin;
          eintExpdiff = newMax - (EInteger)eintExpdiff;
          newMax += (EInteger)eintExpdiff;
          ctxdiv = ctxdiv.WithBigExponentRange(ctxdiv.EMin, newMax);
          thisValue = this.Exp(this.NegateRaw(thisValue), ctxdiv);
          if ((ctxdiv.Flags & EContext.FlagOverflow) != 0) {
            // Still overflowed, so trigger underflow
            return this.SignalUnderflow(ctx, false, false);
          }
        } else {
          thisValue = val;
        }
        thisValue = this.Divide(one, thisValue, ctxCopy);
        // DebugUtility.Log("end= " + thisValue);
        if (ctx.HasFlags) {
          ctx.Flags |= EContext.FlagInexact |
            EContext.FlagRounded;
        }
      } else {
        T intpart = default(T);
        var haveIntPart = false;
        if (ctx.HasExponentRange && this.thisRadix >= 2 &&
          this.thisRadix <= 12 &&
          this.CompareTo(thisValue, this.helper.ValueOf(10)) > 0) {
          // Calculated with ceil(ln(radix))+1 (radixes 0 and 1 are
          // not used and have entries of 1)
          int[] upperDivisors = {
            1, 1, 71, 111, 140, 162, 181, 196, 209, 221, 232, 241, 250,
          };
          // Calculate an upper bound on the overflow threshold
          // for exp
          EInteger maxexp = ctx.EMax.Add(ctx.Precision);
          maxexp = maxexp.Multiply(upperDivisors[this.thisRadix])
            .Divide(100).Add(2);
          maxexp = EInteger.Max(EInteger.FromInt32(10), maxexp);
          T mxe = this.helper.CreateNewWithFlags(
              maxexp,
              EInteger.Zero,
              0);
          if (this.CompareTo(thisValue, mxe) > 0) {
            // Greater than overflow bound, so this is an overflow
            // DebugUtility.Log("thisValue > mxe: " + thisValue + " " + mxe);
            return this.SignalOverflow(ctx, false);
          }
        }
        if (ctx.HasExponentRange &&
          this.CompareTo(thisValue, this.helper.ValueOf(50000)) > 0) {
          // Try to check for overflow quickly
          // Do a trial powering using a lower number than e,
          // and a power of 50000
          this.PowerIntegral(
            this.helper.ValueOf(2),
            (EInteger)50000,
            ctxCopy);
          if ((ctxCopy.Flags & EContext.FlagOverflow) != 0) {
            // The trial powering caused overflow, so exp will
            // cause overflow as well
            return this.SignalOverflow(ctx, false);
          }
          intpart = this.Quantize(
              thisValue,
              one,
              EContext.ForRounding(ERounding.Down));
          if (!this.GetHelper().GetExponent(intpart).IsZero) {
            throw new ArgumentException("integer part not zero, as expected");
          }
          haveIntPart = true;
          ctxCopy.Flags = 0;
          // Now do the same using the integer part of the operand
          // as the power
          this.PowerIntegral(
            this.helper.ValueOf(2),
            this.helper.GetMantissa(intpart),
            ctxCopy);
          if ((ctxCopy.Flags & EContext.FlagOverflow) != 0) {
            // The trial powering caused overflow, so exp will
            // cause overflow as well
            return this.SignalOverflow(ctx, false);
          }
          ctxCopy.Flags = 0;
        }
        if (!haveIntPart) {
          intpart = this.Quantize(
              thisValue,
              one,
              EContext.ForRounding(ERounding.Down));
          if (!this.GetHelper().GetExponent(intpart).IsZero) {
            throw new ArgumentException("integer part not zero, as expected");
          }
        }
        T fracpart = this.Add(thisValue, this.NegateRaw(intpart), null);
        // DebugUtility.Log("fracpart0=" + fracpart);
        ctxdiv = SetPrecisionIfLimited(ctxdiv, ctxdiv.Precision + guardDigits)
          .WithBlankFlags();
        fracpart = this.Add(
            one,
            this.Divide(fracpart, intpart, ctxdiv),
            null);
        ctxdiv.Flags = 0;
        // DebugUtility.Log("fracpart1=" + fracpart);
        EInteger workingPrec = ctxdiv.Precision;
        workingPrec = workingPrec.Add(
            this.WorkingDigits(EInteger.FromInt32(40)));
        // DebugUtility.Log("intpart=" + intpart + " wp=" + workingPrec);
        thisValue = this.ExpInternal(fracpart, workingPrec, ctxdiv);
        // DebugUtility.Log("thisValue=" + thisValue);
        if ((ctxdiv.Flags & EContext.FlagUnderflow) != 0) {
          if (ctx.HasFlags) {
            ctx.Flags |= ctxdiv.Flags;
          }
        }
        if (ctx.HasFlags) {
          ctx.Flags |= EContext.FlagInexact |
            EContext.FlagRounded;
        }
        thisValue = this.PowerIntegral(
            thisValue,
            this.helper.GetMantissa(intpart),
            ctxCopy);
        // DebugUtility.Log(" -->" + thisValue);
      }
      if (ctx.HasFlags) {
        ctx.Flags |= ctxCopy.Flags;
      }
      return thisValue;
    }

    public IRadixMathHelper<T> GetHelper() {
      return this.helper;
    }

    public static EFloat FastLn(EFloat x, EContext ctx) {
      /* #if DEBUG ((ef) == null) {
          throw new ArgumentNullException(nameof(ef));
        }
        if ((ctx) == null) {
          throw new ArgumentNullException(nameof(ctx));
        }
      #endif

      */
      // Fast log for contexts of precision 53 bits or less
      if (x.CompareTo(EFloat.Create(32, -6)) >= 0 &&
        x.CompareTo(EFloat.Create(36, -6)) < 0) {
        return EFloat.Create(-7918475170148451L, -47)
          .MultiplyAndAdd(x, EFloat.Create(5842854079153127L, -44), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-7855987447712801L, -43), ctx)
          .MultiplyAndAdd(x, EFloat.Create(3178826684731201L, -41), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-3446209805793071L, -41), ctx)
          .MultiplyAndAdd(x, EFloat.Create(5269250416501899L, -42), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-1456756048094669L, -41), ctx)
          .MultiplyAndAdd(x, EFloat.Create(589048828844673L, -41), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-5626160540257247L, -46), ctx)
          .MultiplyAndAdd(x, EFloat.Create(5306429958415307L, -48), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-8023390364436687L, -51), ctx);
      }
      if (x.CompareTo(EFloat.Create(36, -6)) >= 0 &&
        x.CompareTo(EFloat.Create(40, -6)) < 0) {
        return EFloat.Create(-649418159759275L, -45)
          .MultiplyAndAdd(x, EFloat.Create(8569695812135613L, -46), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-3219836323271541L, -43), ctx)
          .MultiplyAndAdd(x, EFloat.Create(1456356315564023L, -41), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-7059686721514865L, -43), ctx)
          .MultiplyAndAdd(x, EFloat.Create(6033379619755303L, -43), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-7458850461699891L, -44), ctx)
          .MultiplyAndAdd(x, EFloat.Create(6743646686636803L, -45), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-281293242157611L, -42), ctx)
          .MultiplyAndAdd(x, EFloat.Create(4746007495118267L, -48), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-7772015102064253L, -51), ctx);
      }
      if (x.CompareTo(EFloat.Create(40, -6)) >= 0 &&
        x.CompareTo(EFloat.Create(44, -6)) < 0) {
        return EFloat.Create(5559026033201687L, -50)
          .MultiplyAndAdd(x, EFloat.Create(-4617856151292203L, -47), ctx)
          .MultiplyAndAdd(x, EFloat.Create(54117074379353L, -39), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-6186785536082459L, -45), ctx)
          .MultiplyAndAdd(x, EFloat.Create(7306510509645715L, -45), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-2995764726321697L, -44), ctx)
          .MultiplyAndAdd(x, EFloat.Create(6986795845479189L, -46), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-5891564005530805L, -47), ctx)
          .MultiplyAndAdd(x, EFloat.Create(15091899246223L, -40), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-7320823715054069L, -51), ctx);
      }
      if (x.CompareTo(EFloat.Create(44, -6)) >= 0 &&
        x.CompareTo(EFloat.Create(48, -6)) < 0) {
        return EFloat.Create(612197579983455L, -48)
          .MultiplyAndAdd(x, EFloat.Create(-1114006258063177L, -46), ctx)
          .MultiplyAndAdd(x, EFloat.Create(457577809503393L, -43), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-1790557502154387L, -44), ctx)
          .MultiplyAndAdd(x, EFloat.Create(4632494137994963L, -45), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-4161053891636247L, -45), ctx)
          .MultiplyAndAdd(x, EFloat.Create(2657563185521199L, -45), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-4909589327505907L, -47), ctx)
          .MultiplyAndAdd(x, EFloat.Create(7053693369648581L, -49), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-3557744849045649L, -50), ctx);
      }
      if (x.CompareTo(EFloat.Create(48, -6)) >= 0 &&
        x.CompareTo(EFloat.Create(52, -6)) < 0) {
        return EFloat.Create(577499201531193L, -49)
          .MultiplyAndAdd(x, EFloat.Create(-1142306702241897L, -47), ctx)
          .MultiplyAndAdd(x, EFloat.Create(8160604872283537L, -48), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-4339153427527017L, -46), ctx)
          .MultiplyAndAdd(x, EFloat.Create(6101799781923291L, -46), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-5958127120148891L, -46), ctx)
          .MultiplyAndAdd(x, EFloat.Create(8273521206806363L, -47), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-4154027270256105L, -47), ctx)
          .MultiplyAndAdd(x, EFloat.Create(3244106922381301L, -48), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-13529886537447L, -42), ctx);
      }
      if (x.CompareTo(EFloat.Create(52, -6)) >= 0 &&
        x.CompareTo(EFloat.Create(56, -6)) < 0) {
        return EFloat.Create(1154075304800921L, -51)
          .MultiplyAndAdd(x, EFloat.Create(-2465640916317121L, -49), ctx)
          .MultiplyAndAdd(x, EFloat.Create(74318910129327L, -42), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-85366471369779L, -41), ctx)
          .MultiplyAndAdd(x, EFloat.Create(259329022146413L, -42), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-4376322035869763L, -46), ctx)
          .MultiplyAndAdd(x, EFloat.Create(3282099616186431L, -46), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-3560066267427385L, -47), ctx)
          .MultiplyAndAdd(x, EFloat.Create(1501608713011209L, -47), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-26381046771207L, -43), ctx);
      }
      if (x.CompareTo(EFloat.Create(56, -6)) >= 0 &&
        x.CompareTo(EFloat.Create(60, -6)) < 0) {
        return EFloat.Create(37824989738239L, -47)
          .MultiplyAndAdd(x, EFloat.Create(-43408559199581L, -44), ctx)
          .MultiplyAndAdd(x, EFloat.Create(2878790570900291L, -48), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-7105058961533699L, -48), ctx)
          .MultiplyAndAdd(x, EFloat.Create(5797162642745407L, -47), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-6569041813188869L, -47), ctx)
          .MultiplyAndAdd(x, EFloat.Create(661617942907567L, -44), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-6168232135736261L, -48), ctx)
          .MultiplyAndAdd(x, EFloat.Create(43675806283161L, -42), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-6591942829339363L, -51), ctx);
      }
      if (x.CompareTo(EFloat.Create(60, -6)) >= 0 &&
        x.CompareTo(EFloat.Create(63, -6)) < 0) {
        return EFloat.Create(-6156921697102261L, -55)
          .MultiplyAndAdd(x, EFloat.Create(211488681190339L, -47), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-6644421976470021L, -50), ctx)
          .MultiplyAndAdd(x, EFloat.Create(7668093965389463L, -49), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-5761162710156971L, -48), ctx)
          .MultiplyAndAdd(x, EFloat.Create(369347589996043L, -44), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-8524061902531777L, -49), ctx)
          .MultiplyAndAdd(x, EFloat.Create(4683735041389899L, -49), ctx)
          .MultiplyAndAdd(x, EFloat.Create(-6208425595264589L, -51), ctx);
      }
      return null;
    }

    public T Ln(T thisValue, EContext ctx) {
      if (ctx == null) {
        return this.SignalInvalidWithMessage(ctx, "ctx is null");
      }
      if (!ctx.HasMaxPrecision) {
        return this.SignalInvalidWithMessage(
            ctx,
            "ctx has unlimited precision");
      }
      int flags = this.helper.GetFlags(thisValue);
      if ((flags & BigNumberFlags.FlagSignalingNaN) != 0) {
        // NOTE: Returning a signaling NaN is independent of
        // rounding mode
        return this.SignalingNaNInvalid(thisValue, ctx);
      }
      if ((flags & BigNumberFlags.FlagQuietNaN) != 0) {
        // NOTE: Returning a quiet NaN is independent of
        // rounding mode
        return this.ReturnQuietNaN(thisValue, ctx);
      }
      int sign = this.helper.GetSign(thisValue);
      if (sign < 0) {
        return this.SignalInvalid(ctx);
      }
      if ((flags & BigNumberFlags.FlagInfinity) != 0) {
        return thisValue;
      }
      EContext ctxCopy = ctx.WithBlankFlags();
      T one = this.helper.ValueOf(1);
      ERounding intermedRounding = ERounding.HalfEven;
      if (sign == 0) {
        return this.helper.CreateNewWithFlags(
            EInteger.Zero,
            EInteger.Zero,
            BigNumberFlags.FlagNegative | BigNumberFlags.FlagInfinity);
      } else {
        int cmpOne = this.CompareTo(thisValue, one);
        EContext ctxdiv = null;
        if (cmpOne == 0) {
          // Equal to 1
          thisValue = this.RoundToPrecision(
              this.helper.CreateNewWithFlags(EInteger.Zero, EInteger.Zero, 0),
              ctxCopy);
        } else if (cmpOne < 0) {
          // Less than 1
          T half = this.Divide(one, this.helper.ValueOf(2), ctxCopy);
          if (this.CompareTo(thisValue, half) >= 0 &&
            this.helper.GetRadix() == 2 && ctx.Precision.CompareTo(53) <= 0) {
            if (thisValue is EFloat) {
              var ef = thisValue as EFloat;
              ef = FastLn(ef, ctxCopy);
              if (ef != null) {
                thisValue = this.helper.CreateNewWithFlags(
                    ef.UnsignedMantissa,
                    ef.Exponent,
                    BigNumberFlags.FlagNegative);
                if (ctx.HasFlags) {
                  ctx.Flags |= EContext.FlagInexact;
                  ctx.Flags |= EContext.FlagRounded;
                }
                return thisValue;
              }
            }
          }
          T quarter = this.Divide(one, this.helper.ValueOf(4), ctxCopy);
          FastInteger error;
          error = (this.CompareTo(thisValue, quarter) < 0) ?
            new FastInteger(20) : new FastInteger(10);
          EInteger bigError = error.ToEInteger();
          ctxdiv = SetPrecisionIfLimited(ctx, ctx.Precision + bigError)
            .WithRounding(intermedRounding).WithBlankFlags();
          T threeQuarters = this.Multiply(
              quarter,
              this.helper.ValueOf(3),
              null);
          if (this.CompareTo(thisValue, threeQuarters) <= 0) {
            // Three quarters or less
            var roots = new FastInteger(0);
            // Take square root until this value
            // is 3/4 or more
            while (this.CompareTo(thisValue, threeQuarters) < 0) {
              thisValue = this.SquareRoot(
                  thisValue,
                  ctxdiv.WithUnlimitedExponents());
              // DebugUtility.Log("--> " +thisValue);
              roots.Increment();
            }
            for (var i = 0; i < 6; ++i) {
              thisValue = this.SquareRoot(
                  thisValue,
                  ctxdiv.WithUnlimitedExponents());
              // DebugUtility.Log("--> " +thisValue);
              roots.Increment();
            }
            // DebugUtility.Log("LnInternal AA " +(thisValue as
            // EDecimal)?.ToDouble());
            thisValue = this.LnInternal(thisValue, ctxdiv.Precision, ctxdiv);
            EInteger bigintRoots = PowerOfTwo(roots);
            // Multiply back 2^X, where X is the number
            // of square root calls
            thisValue = this.Multiply(
                thisValue,
                this.helper.CreateNewWithFlags(bigintRoots, EInteger.Zero, 0),
                ctxCopy);
          } else {
            T smallfrac = this.Divide(one, this.helper.ValueOf(16), ctxdiv);
            T closeToOne = this.Add(one, this.NegateRaw(smallfrac), null);
            if (this.CompareTo(thisValue, closeToOne) >= 0) {
              // This value is close to 1, so use a higher working precision
              error = this.helper.GetDigitLength(this.helper.GetMantissa(
                    thisValue));
              error = error.Copy();
              error.AddInt(6);
              error.AddBig(ctx.Precision);
              bigError = error.ToEInteger();
              // DebugUtility.Log("LnInternalCloseToOne error="+error);
              // DebugUtility.Log("LnInternalCloseToOne B " +(thisValue as
              // EDecimal)?.ToDouble());
              thisValue = this.LnInternalCloseToOne2(
                  thisValue,
                  error.ToEInteger(),
                  ctxCopy);
            } else {
              // DebugUtility.Log("LnInternal A " +(thisValue as
              // EDecimal)?.ToDouble());
              thisValue = this.LnInternal(
                  thisValue,
                  ctxdiv.Precision,
                  ctxCopy);
            }
          }
          if (ctx.HasFlags) {
            ctxCopy.Flags |= EContext.FlagInexact;
            ctxCopy.Flags |= EContext.FlagRounded;
          }
        } else {
          // Greater than 1
          // T hundred = this.helper.ValueOf(100);
          T two = this.helper.ValueOf(2);
          // DebugUtility.Log("thisValue=" + thisValue +
          // " hundredcmp=" + this.CompareTo(thisValue, hundred) +
          // " twocmp=" + this.CompareTo(thisValue, two));
          if (this.CompareTo(thisValue, two) > 0 &&
            this.helper.GetRadix() == 2) {
            T half = this.Divide(this.helper.ValueOf(1),
                this.helper.ValueOf(2),
                EContext.Unlimited);
            FastIntegerFixed fmant = this.helper.GetMantissaFastInt(thisValue);
            EInteger fexp =
              this.helper.GetExponentFastInt(thisValue).ToEInteger();
            EInteger fbits =
              fmant.ToEInteger().GetUnsignedBitLengthAsEInteger();
            EInteger adjval = EInteger.One;
            adjval = fbits.Negate(); // fexp.Subtract(fbits.Add(fexp));
            EInteger adjbits = EInteger.Zero;
            T reduced = default(T);
            if (fexp.Sign > 0) {
              reduced = this.helper.CreateNewWithFlags(fmant.ToEInteger(),
                  adjval,
                  0);
              adjbits = fexp.Add(fbits);
            } else {
              reduced = this.helper.CreateNewWithFlags(fmant.ToEInteger(),
                  adjval,
                  0);
              adjbits = fexp.Add(fbits);
            }
            T addval = adjbits.Sign < 0 ? this.helper.CreateNewWithFlags(
                adjbits.Abs(),
                EInteger.Zero,
                BigNumberFlags.FlagNegative) : this.helper.CreateNewWithFlags(
                adjbits.Abs(),
                EInteger.Zero,
                0);
            EInteger cprec = ctx.Precision.Add(10);
            ctxdiv = SetPrecisionIfLimited(ctx, cprec)
              .WithRounding(intermedRounding).WithBlankFlags();
            #if DEBUG
            if (this.CompareTo(reduced, one) >= 0 ||
              this.CompareTo(reduced, half) < 0) {
              throw new InvalidOperationException(
                "thisValue = " + thisValue + "\n" +
                "fexp = " + fexp + "\n" + "fbits = " + fbits + "\n" +
                "adjval = " + adjval + "\n" + "reduced = " + reduced + "\n");
            }
            #endif
            // DebugUtility.Log("thisValue = " + thisValue + "\n" +
            // "fexp = " + fexp + "\n" + "fbits = " + fbits + "\n" +
            // "adjval = " + adjval + "\n" + "reduced = " + reduced + "\n");
            reduced = this.Ln(reduced, ctxdiv);
            thisValue = this.MultiplyAndAdd(
                this.Ln(two, ctxdiv),
                addval,
                reduced,
                ctxCopy);
          } else if (this.CompareTo(thisValue, two) >= 0) {
            // 2 or greater
            var roots = new FastInteger(0);
            FastInteger error;
            EInteger bigError;
            FastIntegerFixed fmant = this.helper.GetMantissaFastInt(thisValue);
            FastIntegerFixed[] bounds = NumberUtility.DigitLengthBoundsFixed(
                this.helper,
                fmant);
            // DebugUtility.Log("thisValue "+thisValue);
            // DebugUtility.Log("bounds "+bounds[1]+" ctxprec="+ctx.Precision);
            error = new FastInteger(10);
            if (this.CompareTo(thisValue,
                this.helper.ValueOf(10000000)) >= 0) {
              if (this.helper.GetRadix() == 2) {
                error = new FastInteger(32);
              }
              error = new FastInteger(16);
            }
            bigError = error.ToEInteger();
            EInteger cprec = EInteger.Max(bounds[1].ToEInteger(), ctx.Precision)
              .Add(bigError);
            // DebugUtility.Log("cprec prec " + (// ctx.Precision) + " bounds " +
            // (bounds[1].ToEInteger()));
            ctxdiv = SetPrecisionIfLimited(ctx, cprec)
              .WithRounding(intermedRounding).WithBlankFlags();
            T oldThisValue = thisValue;
            // Take square root until this value
            // is close to 1
            while (this.CompareTo(thisValue, two) >= 0) {
              thisValue = this.SquareRoot(
                  thisValue,
                  ctxdiv.WithUnlimitedExponents());
              roots.Increment();
            }
            var iterCount = 8;
            if (this.helper.GetRadix() == 2 && cprec.CompareTo(300) > 0) {
              iterCount = 36;
            } else if (this.helper.GetRadix() > 2 && cprec.CompareTo(100) > 0) {
              iterCount = 36;
            }
            for (int i = 0; i < iterCount; ++i) {
              thisValue = this.SquareRoot(
                  thisValue,
                  ctxdiv.WithUnlimitedExponents());
              // DebugUtility.Log("--> " +thisValue);
              roots.Increment();
            }
            // DebugUtility.Log("rootcount="+roots);
            // Find -Ln(1/thisValue)
            /*if (thisValue is EDecimal) {
   DebugUtility.Log("LnInternalCloseToOne C " + ((thisValue as
EDecimal)?.ToDouble()));
 } else {
 DebugUtility.Log("LnInternalCloseToOne C " + ((thisValue as
EFloat)?.ToDouble()));
}
            */ thisValue = this.Divide(one, thisValue, ctxdiv);
            // DebugUtility.Log("LnInternalCloseToOne C prec " + ctxdiv.Precision);
            thisValue = this.LnInternalCloseToOne2(
                thisValue,
                ctxdiv.Precision,
                ctxdiv);
            thisValue = this.NegateRaw(thisValue);
            // DebugUtility.Log("After LnInternal " +thisValue +
            // " roots="+roots);
            EInteger bigintRoots = PowerOfTwo(roots);
            // Multiply back 2^X, where X is the number
            // of square root calls
            /* DebugUtility.Log("After LnInternal Mult<ctxdiv> " +this.Multiply(
                thisValue,
                this.helper.CreateNewWithFlags(bigintRoots, EInteger.Zero, 0),
                ctxdiv));
             DebugUtility.Log("After LnInternal Mult<ei3> " +this.Multiply(
                thisValue,
                this.helper.CreateNewWithFlags(bigintRoots, EInteger.Zero, 0),
                ctxCopy.WithRounding(intermedRounding)));
            */ thisValue = this.Multiply(
                thisValue,
                this.helper.CreateNewWithFlags(bigintRoots, EInteger.Zero, 0),
                ctxCopy);
            // DebugUtility.Log("After LnInternal Mult " +(thisValue as
            // EDecimal)?.ToDouble());
            // DebugUtility.Log("ctx=" + ctxCopy + " ");
          } else {
            FastInteger error;
            EInteger bigError;
            error = new FastInteger(10);
            bigError = error.ToEInteger();
            ctxdiv = SetPrecisionIfLimited(ctx, ctx.Precision + bigError)
              .WithRounding(intermedRounding).WithBlankFlags();
            T smallfrac = this.Divide(one, this.helper.ValueOf(16), ctxdiv);
            T closeToOne = this.Add(one, smallfrac, null);
            if (this.CompareTo(thisValue, closeToOne) < 0) {
              error = this.helper.GetDigitLength(this.helper.GetMantissa(
                    thisValue));
              error = error.Copy();
              error.AddInt(6);
              error.AddBig(ctx.Precision);
              // DebugUtility.Log("using error precision: " + error + ", " +
              // thisValue);
              bigError = error.ToEInteger();
              // Greater than 1 and close to 1, will require a higher working
              // precision
              // DebugUtility.Log("LnInternalCloseToOne D " +(thisValue as
              // EDecimal)?.ToDouble());
              thisValue = this.LnInternalCloseToOne2(
                  thisValue,
                  error.ToEInteger(),
                  ctxCopy);
            } else {
              // Find -Ln(1/thisValue)
              thisValue = this.Divide(one, thisValue, ctxdiv);
              // DebugUtility.Log("LnInternal B " +(thisValue as
              // EDecimal)?.ToDouble());
              /* // thisValue = this.LnInternal(
                // thisValue, // ctxdiv.getPrecision()
                //,
                ctxCopy); */ thisValue = this.Ln(thisValue, ctxCopy);
              thisValue = this.NegateRaw(thisValue);
            }
          }
          if (ctx.HasFlags) {
            ctxCopy.Flags |= EContext.FlagInexact;
            ctxCopy.Flags |= EContext.FlagRounded;
          }
        }
      }
      if (ctx.HasFlags) {
        ctx.Flags |= ctxCopy.Flags;
      }
      return thisValue;
    }

    public T Max(T a, T b, EContext ctx) {
      if (a == null) {
        throw new ArgumentNullException(nameof(a));
      }
      if (b == null) {
        throw new ArgumentNullException(nameof(b));
      }
      // Handle infinity and NaN
      T result = this.MinMaxHandleSpecial(a, b, ctx, false, false);
      if ((object)result != (object)default(T)) {
        return result;
      }
      int cmp = this.CompareTo(a, b);
      if (cmp != 0) {
        return cmp < 0 ? this.RoundToPrecision(b, ctx) :
          this.RoundToPrecision(a, ctx);
      }
      int flagNegA = this.helper.GetFlags(a) & BigNumberFlags.FlagNegative;
      return (flagNegA != (this.helper.GetFlags(b) &
            BigNumberFlags.FlagNegative)) ? ((flagNegA != 0) ?
          this.RoundToPrecision(b, ctx) : this.RoundToPrecision(a, ctx)) :
        ((flagNegA == 0) ? (this.helper.GetExponent(a).CompareTo(
              this.helper.GetExponent(
                b)) > 0 ? this.RoundToPrecision(a, ctx) :
            this.RoundToPrecision(b, ctx)) : (this.helper.GetExponent(
              a).CompareTo(
              this.helper.GetExponent(
                b)) > 0 ? this.RoundToPrecision(b, ctx) :
            this.RoundToPrecision(a, ctx)));
    }

    public T MaxMagnitude(T a, T b, EContext ctx) {
      if (a == null) {
        throw new ArgumentNullException(nameof(a));
      }
      if (b == null) {
        throw new ArgumentNullException(nameof(b));
      }
      // Handle infinity and NaN
      T result = this.MinMaxHandleSpecial(a, b, ctx, false, true);
      if ((object)result != (object)default(T)) {
        return result;
      }
      int cmp = this.CompareTo(this.AbsRaw(a), this.AbsRaw(b));
      return (cmp == 0) ? this.Max(a, b, ctx) : ((cmp > 0) ?
          this.RoundToPrecision(
            a,
            ctx) : this.RoundToPrecision(
            b,
            ctx));
    }

    public T Min(T a, T b, EContext ctx) {
      if (a == null) {
        throw new ArgumentNullException(nameof(a));
      }
      if (b == null) {
        throw new ArgumentNullException(nameof(b));
      }
      // Handle infinity and NaN
      T result = this.MinMaxHandleSpecial(a, b, ctx, true, false);
      if ((object)result != (object)default(T)) {
        return result;
      }
      int cmp = this.CompareTo(a, b);
      if (cmp != 0) {
        return cmp > 0 ? this.RoundToPrecision(b, ctx) :
          this.RoundToPrecision(a, ctx);
      }
      int signANeg = this.helper.GetFlags(a) & BigNumberFlags.FlagNegative;
      return (signANeg != (this.helper.GetFlags(b) &
            BigNumberFlags.FlagNegative)) ? ((signANeg != 0) ?
          this.RoundToPrecision(a, ctx) : this.RoundToPrecision(b, ctx)) :
        ((signANeg == 0) ? (this.helper.GetExponent(a).CompareTo(
              this.helper.GetExponent(
                b)) > 0 ? this.RoundToPrecision(b, ctx) :
            this.RoundToPrecision(a, ctx)) : (this.helper.GetExponent(
              a).CompareTo(
              this.helper.GetExponent(
                b)) > 0 ? this.RoundToPrecision(a, ctx) :
            this.RoundToPrecision(b, ctx)));
    }

    public T MinMagnitude(T a, T b, EContext ctx) {
      if (a == null) {
        throw new ArgumentNullException(nameof(a));
      }
      if (b == null) {
        throw new ArgumentNullException(nameof(b));
      }
      // Handle infinity and NaN
      T result = this.MinMaxHandleSpecial(a, b, ctx, true, true);
      if ((object)result != (object)default(T)) {
        return result;
      }
      int cmp = this.CompareTo(this.AbsRaw(a), this.AbsRaw(b));
      return (cmp == 0) ? this.Min(a, b, ctx) : ((cmp < 0) ?
          this.RoundToPrecision(
            a,
            ctx) : this.RoundToPrecision(
            b,
            ctx));
    }

    public T Multiply(T thisValue, T other, EContext ctx) {
      int thisFlags = this.helper.GetFlags(thisValue);
      int otherFlags = this.helper.GetFlags(other);
      if (((thisFlags | otherFlags) & BigNumberFlags.FlagSpecial) != 0) {
        T result = this.HandleNotANumber(thisValue, other, ctx);
        if ((object)result != (object)default(T)) {
          return result;
        }
        if ((thisFlags & BigNumberFlags.FlagInfinity) != 0) {
          // Attempt to multiply infinity by 0
          bool negflag = ((thisFlags & BigNumberFlags.FlagNegative) != 0) ^
            ((otherFlags & BigNumberFlags.FlagNegative) != 0);
          return ((otherFlags & BigNumberFlags.FlagSpecial) == 0 &&
              this.helper.GetMantissa(other).IsZero) ? this.SignalInvalid(
              ctx) : this.EnsureSign(
              thisValue,
              negflag);
        }
        if ((otherFlags & BigNumberFlags.FlagInfinity) != 0) {
          // Attempt to multiply infinity by 0
          bool negflag = ((thisFlags & BigNumberFlags.FlagNegative) != 0) ^
            ((otherFlags & BigNumberFlags.FlagNegative) != 0);
          return ((thisFlags & BigNumberFlags.FlagSpecial) == 0 &&
              this.helper.GetMantissa(thisValue).IsZero) ?
            this.SignalInvalid(ctx) : this.EnsureSign(other, negflag);
        }
      }
      EInteger bigintOp2 = this.helper.GetExponent(other);
      EInteger newexp = this.helper.GetExponent(thisValue) +
        (EInteger)bigintOp2;
      EInteger mantissaOp2 = this.helper.GetMantissa(other);
      // DebugUtility.Log("" + (this.helper.GetMantissa(thisValue)) + "," +
      // (this.helper.GetExponent(thisValue)) + " -> " + mantissaOp2 +", " +
      // (bigintOp2));
      thisFlags = (thisFlags & BigNumberFlags.FlagNegative) ^ (otherFlags &
          BigNumberFlags.FlagNegative);
      T ret =
        this.helper.CreateNewWithFlags(
          this.helper.GetMantissa(thisValue) * (EInteger)mantissaOp2,
          newexp,
          thisFlags);
      if (ctx != null && ctx != EContext.UnlimitedHalfEven) {
        ret = this.RoundToPrecision(ret, ctx);
      }
      return ret;
    }

    public T MultiplyAndAdd(
      T thisValue,
      T multiplicand,
      T augend,
      EContext ctx) {
      if (multiplicand == null) {
        throw new ArgumentNullException(nameof(multiplicand));
      }
      if (augend == null) {
        throw new ArgumentNullException(nameof(augend));
      }
      EContext ctx2 = EContext.UnlimitedHalfEven.WithBlankFlags();
      T ret = this.MultiplyAddHandleSpecial(
          thisValue,
          multiplicand,
          augend,
          ctx);
      if ((object)ret != (object)default(T)) {
        return ret;
      }
      T product = this.Multiply(thisValue, multiplicand, ctx2);
      ret = this.Add(product, augend, ctx);
      if (ctx != null && ctx.HasFlags) {
        ctx.Flags |= ctx2.Flags;
      }
      return ret;
    }

    public T Negate(T value, EContext ctx) {
      int flags = this.helper.GetFlags(value);
      if ((flags & BigNumberFlags.FlagSignalingNaN) != 0) {
        return this.SignalingNaNInvalid(value, ctx);
      }
      if ((flags & BigNumberFlags.FlagQuietNaN) != 0) {
        return this.ReturnQuietNaN(value, ctx);
      }
      EInteger mant = this.helper.GetMantissa(value);
      T zero;
      if ((flags & BigNumberFlags.FlagInfinity) == 0 && mant.IsZero) {
        // Negate(-0) is treated as subtract(0, -0), which in turn is treated
        // as add (0, 0), so that the result is positive 0 since both
        // operands to add are positive
        // Negate(0) is treated as subtract(0, 0), which in turn is treated
        // as add(0, -0), so that the result is positive 0 since both
        // operands to add are positive
        bool nonnegative, floor;
        nonnegative = (flags & BigNumberFlags.FlagNegative) == 0;
        floor = ctx != null && ctx.Rounding == ERounding.Floor;
        if (floor && nonnegative) {
          zero = this.helper.CreateNewWithFlags(
              mant,
              this.helper.GetExponent(value),
              flags | BigNumberFlags.FlagNegative);
        } else {
          zero = this.helper.CreateNewWithFlags(
              mant,
              this.helper.GetExponent(value),
              flags & ~BigNumberFlags.FlagNegative);
        }
        // DebugUtility.Log("" + (// value) + " -> " + zero + " (nonneg=" +
        // nonnegative + ", floor=" + floor + ")");
        return this.RoundToPrecision(zero, ctx);
      }
      flags ^= BigNumberFlags.FlagNegative;
      T ret = this.helper.CreateNewWithFlags(
          mant,
          this.helper.GetExponent(value),
          flags);
      return this.RoundToPrecision(ret, ctx);
    }

    public T NextMinus(T thisValue, EContext ctx) {
      if (ctx == null) {
        return this.SignalInvalidWithMessage(ctx, "ctx is null");
      }
      if (!ctx.HasMaxPrecision) {
        return this.SignalInvalidWithMessage(
            ctx,
            "ctx has unlimited precision");
      }
      if (!ctx.HasExponentRange) {
        return this.SignalInvalidWithMessage(
            ctx,
            "doesn't satisfy ctx.HasExponentRange");
      }
      int flags = this.helper.GetFlags(thisValue);
      if ((flags & BigNumberFlags.FlagSignalingNaN) != 0) {
        return this.SignalingNaNInvalid(thisValue, ctx);
      }
      if ((flags & BigNumberFlags.FlagQuietNaN) != 0) {
        return this.ReturnQuietNaN(thisValue, ctx);
      }
      if ((flags & BigNumberFlags.FlagInfinity) != 0) {
        if ((flags & BigNumberFlags.FlagNegative) != 0) {
          return thisValue;
        } else {
          EInteger bigexp2 = ctx.EMax;
          EInteger bigprec = ctx.Precision;
          if (ctx.AdjustExponent) {
            bigexp2 += EInteger.One;
            bigexp2 -= (EInteger)bigprec;
          }
          EInteger overflowMant = this.TryMultiplyByRadixPower(
              EInteger.One,
              FastInteger.FromBig(ctx.Precision));
          if (overflowMant == null) {
            return this.SignalInvalidWithMessage(
                ctx,
                "Result requires too much memory");
          }
          overflowMant -= EInteger.One;
          return this.helper.CreateNewWithFlags(overflowMant, bigexp2, 0);
        }
      }
      FastInteger minexp = FastInteger.FromBig(ctx.EMin);
      if (ctx.AdjustExponent) {
        minexp.SubtractBig(ctx.Precision).Increment();
      }
      FastInteger bigexp =
        FastInteger.FromBig(this.helper.GetExponent(thisValue));
      if (bigexp.CompareTo(minexp) <= 0) {
        // Use a smaller exponent if the input exponent is already
        // very small
        minexp = bigexp.Copy().SubtractInt(2);
      }
      T quantum = this.helper.CreateNewWithFlags(
          EInteger.One,
          minexp.ToEInteger(),
          BigNumberFlags.FlagNegative);
      EContext ctx2;
      ctx2 = ctx.WithRounding(ERounding.Floor);
      return this.Add(thisValue, quantum, ctx2);
    }

    public T NextPlus(T thisValue, EContext ctx) {
      if (ctx == null) {
        return this.SignalInvalidWithMessage(ctx, "ctx is null");
      }
      if (!ctx.HasMaxPrecision) {
        return this.SignalInvalidWithMessage(
            ctx,
            "ctx has unlimited precision");
      }
      if (!ctx.HasExponentRange) {
        return this.SignalInvalidWithMessage(
            ctx,
            "doesn't satisfy ctx.HasExponentRange");
      }
      int flags = this.helper.GetFlags(thisValue);
      if ((flags & BigNumberFlags.FlagSignalingNaN) != 0) {
        return this.SignalingNaNInvalid(thisValue, ctx);
      }
      if ((flags & BigNumberFlags.FlagQuietNaN) != 0) {
        return this.ReturnQuietNaN(thisValue, ctx);
      }
      if ((flags & BigNumberFlags.FlagInfinity) != 0) {
        if ((flags & BigNumberFlags.FlagNegative) != 0) {
          EInteger bigexp2 = ctx.EMax;
          EInteger bigprec = ctx.Precision;
          if (ctx.AdjustExponent) {
            bigexp2 += EInteger.One;
            bigexp2 -= (EInteger)bigprec;
          }
          EInteger overflowMant = this.TryMultiplyByRadixPower(
              EInteger.One,
              FastInteger.FromBig(ctx.Precision));
          if (overflowMant == null) {
            return this.SignalInvalidWithMessage(
                ctx,
                "Result requires too much memory");
          }
          overflowMant -= EInteger.One;
          return this.helper.CreateNewWithFlags(
              overflowMant,
              bigexp2,
              BigNumberFlags.FlagNegative);
        }
        return thisValue;
      }
      FastInteger minexp = FastInteger.FromBig(ctx.EMin);
      if (ctx.AdjustExponent) {
        minexp.SubtractBig(ctx.Precision).Increment();
      }
      FastInteger bigexp =
        FastInteger.FromBig(this.helper.GetExponent(thisValue));
      if (bigexp.CompareTo(minexp) <= 0) {
        // Use a smaller exponent if the input exponent is already
        // very small
        minexp = bigexp.Copy().SubtractInt(2);
      }
      T quantum = this.helper.CreateNewWithFlags(
          EInteger.One,
          minexp.ToEInteger(),
          0);
      EContext ctx2;
      T val = thisValue;
      ctx2 = ctx.WithRounding(ERounding.Ceiling);
      return this.Add(val, quantum, ctx2);
    }

    public T NextToward(T thisValue, T otherValue, EContext ctx) {
      if (ctx == null) {
        return this.SignalInvalidWithMessage(ctx, "ctx is null");
      }
      if (!ctx.HasMaxPrecision) {
        return this.SignalInvalidWithMessage(
            ctx,
            "ctx has unlimited precision");
      }
      if (!ctx.HasExponentRange) {
        return this.SignalInvalidWithMessage(
            ctx,
            "doesn't satisfy ctx.HasExponentRange");
      }
      int thisFlags = this.helper.GetFlags(thisValue);
      int otherFlags = this.helper.GetFlags(otherValue);
      if (((thisFlags | otherFlags) & BigNumberFlags.FlagSpecial) != 0) {
        T result = this.HandleNotANumber(thisValue, otherValue, ctx);
        if ((object)result != (object)default(T)) {
          return result;
        }
      }
      EContext ctx2;
      int cmp = this.CompareTo(thisValue, otherValue);
      if (cmp == 0) {
        return this.RoundToPrecision(
            this.EnsureSign(
              thisValue,
              (otherFlags & BigNumberFlags.FlagNegative) != 0),
            ctx.WithNoFlags());
      } else {
        if ((thisFlags & BigNumberFlags.FlagInfinity) != 0) {
          if ((thisFlags & (BigNumberFlags.FlagInfinity |
                BigNumberFlags.FlagNegative)) == (otherFlags &
              (BigNumberFlags.FlagInfinity | BigNumberFlags.FlagNegative))) {
            // both values are the same infinity
            return thisValue;
          } else {
            EInteger bigexp2 = ctx.EMax;
            EInteger bigprec = ctx.Precision;
            if (ctx.AdjustExponent) {
              bigexp2 += EInteger.One;
              bigexp2 -= (EInteger)bigprec;
            }
            EInteger overflowMant = this.TryMultiplyByRadixPower(
                EInteger.One,
                FastInteger.FromBig(ctx.Precision));
            if (overflowMant == null) {
              return this.SignalInvalidWithMessage(
                  ctx,
                  "Result requires too much memory");
            }
            overflowMant -= EInteger.One;
            return this.helper.CreateNewWithFlags(
                overflowMant,
                bigexp2,
                thisFlags & BigNumberFlags.FlagNegative);
          }
        }
        FastInteger minexp = FastInteger.FromBig(ctx.EMin);
        if (ctx.AdjustExponent) {
          minexp.SubtractBig(ctx.Precision).Increment();
        }
        FastInteger bigexp =
          FastInteger.FromBig(this.helper.GetExponent(thisValue));
        if (bigexp.CompareTo(minexp) < 0) {
          // Use a smaller exponent if the input exponent is already
          // very small
          minexp = bigexp.Copy().SubtractInt(2);
        } else {
          // Ensure the exponent is lower than the exponent range
          // (necessary to flag underflow correctly)
          minexp.SubtractInt(2);
        }
        T quantum = this.helper.CreateNewWithFlags(
            EInteger.One,
            minexp.ToEInteger(),
            (cmp > 0) ? BigNumberFlags.FlagNegative : 0);
        T val = thisValue;
        ctx2 = ctx.WithRounding((cmp > 0) ? ERounding.Floor :
            ERounding.Ceiling).WithBlankFlags();
        val = this.Add(val, quantum, ctx2);
        if ((ctx2.Flags & (EContext.FlagOverflow |
              EContext.FlagUnderflow)) == 0) {
          // Don't set flags except on overflow or underflow,
          // in accordance with the DecTest test cases
          ctx2.Flags = 0;
        }
        if ((ctx2.Flags & EContext.FlagUnderflow) != 0) {
          EInteger bigmant = this.helper.GetMantissa(val);
          EInteger maxmant = this.TryMultiplyByRadixPower(
              EInteger.One,
              FastInteger.FromBig(ctx.Precision).Decrement());
          if (maxmant == null) {
            return this.SignalInvalidWithMessage(
                ctx,
                "Result requires too much memory");
          }
          if (bigmant.CompareTo(maxmant) >= 0 ||
            ctx.Precision.CompareTo(EInteger.One) == 0) {
            // don't treat max-precision results as having underflowed
            ctx2.Flags = 0;
          }
        }
        if (ctx.HasFlags) {
          ctx.Flags |= ctx2.Flags;
        }
        return val;
      }
    }

    public T Pi(EContext ctx) {
      if (ctx == null) {
        return this.SignalInvalidWithMessage(ctx, "ctx is null");
      }
      if (!ctx.HasMaxPrecision) {
        return this.SignalInvalidWithMessage(
            ctx,
            "ctx has unlimited precision");
      }
      // Gauss-Legendre algorithm
      T a = this.helper.ValueOf(1);
      EContext ctxdiv = SetPrecisionIfLimited(
          ctx,
          ctx.Precision + (EInteger)10)
        .WithRounding(ERounding.HalfEven);
      T two = this.helper.ValueOf(2);
      T b = this.Divide(a, this.SquareRoot(two, ctxdiv), ctxdiv);
      T four = this.helper.ValueOf(4);
      T half = ((this.thisRadix & 1) == 0) ?
        this.helper.CreateNewWithFlags(
          (EInteger)(this.thisRadix / 2),
          ValueMinusOne,
          0) : default(T);
      T t = this.Divide(a, four, ctxdiv);
      var more = true;
      var lastCompare = 0;
      var vacillations = 0;
      T lastGuess = default(T);
      T guess = default(T);
      EInteger powerTwo = EInteger.One;
      while (more) {
        lastGuess = guess;
        T aplusB = this.Add(a, b, null);
        T newA = (half == null) ? this.Divide(aplusB, two, ctxdiv) :
          this.Multiply(aplusB, half, null);
        T valueAMinusNewA = this.Add(a, this.NegateRaw(newA), null);
        if (!a.Equals(b)) {
          T atimesB = this.Multiply(a, b, ctxdiv);
          b = this.SquareRoot(atimesB, ctxdiv);
        }
        a = newA;
        guess = this.Multiply(aplusB, aplusB, null);
        guess = this.Divide(guess, this.Multiply(t, four, null), ctxdiv);
        T newGuess = guess;
        if ((object)lastGuess != (object)default(T)) {
          int guessCmp = this.CompareTo(lastGuess, newGuess);
          if (guessCmp == 0) {
            more = false;
          } else if ((guessCmp > 0 && lastCompare < 0) || (lastCompare > 0 &&
              guessCmp < 0)) {
            // Guesses are vacillating
            ++vacillations;
            more &= vacillations <= 3;
          }
          lastCompare = guessCmp;
        }
        if (more) {
          T tmpT = this.Multiply(valueAMinusNewA, valueAMinusNewA, null);
          tmpT = this.Multiply(
              tmpT,
              this.helper.CreateNewWithFlags(powerTwo, EInteger.Zero, 0),
              null);
          t = this.Add(t, this.NegateRaw(tmpT), ctxdiv);
          powerTwo <<= 1;
        }
        guess = newGuess;
      }
      return this.RoundToPrecision(guess, ctx);
    }

    public T Plus(T thisValue, EContext context) {
      return this.RoundToPrecisionInternal(
          thisValue,
          0,
          0,
          null,
          true,
          context);
    }

    public T Power(T thisValue, T pow, EContext ctx) {
      T ret = this.HandleNotANumber(thisValue, pow, ctx);
      if ((object)ret != (object)default(T)) {
        return ret;
      }
      int thisSign = this.helper.GetSign(thisValue);
      int powSign = this.helper.GetSign(pow);
      int thisFlags = this.helper.GetFlags(thisValue);
      int powFlags = this.helper.GetFlags(pow);
      if (thisSign == 0 && powSign == 0) {
        // Both operands are zero: invalid
        return this.SignalInvalid(ctx);
      }
      if (thisSign < 0 && (powFlags & BigNumberFlags.FlagInfinity) != 0) {
        // This value is negative and power is infinity: invalid
        return this.SignalInvalid(ctx);
      }
      if (thisSign > 0 && (thisFlags & BigNumberFlags.FlagInfinity) == 0 &&
        (powFlags & BigNumberFlags.FlagInfinity) != 0) {
        // Power is infinity and this value is greater than
        // zero and not infinity
        int cmp = this.CompareTo(thisValue, this.helper.ValueOf(1));
        if (cmp < 0) {
          // Value is less than 1
          if (powSign < 0) {
            // Power is negative infinity, return positive infinity
            return this.helper.CreateNewWithFlags(
                EInteger.Zero,
                EInteger.Zero,
                BigNumberFlags.FlagInfinity);
          }
          // Power is positive infinity, return 0
          return this.RoundToPrecision(
              this.helper.CreateNewWithFlags(EInteger.Zero, EInteger.Zero, 0),
              ctx);
        }
        if (cmp == 0) {
          // Extend the precision of the mantissa as much as possible,
          // in the special case that this value is 1
          return this.ExtendPrecision(this.helper.ValueOf(1), ctx);
        }
        // Value is greater than 1
        if (powSign > 0) {
          // Power is positive infinity, return positive infinity
          return pow;
        }
        // Power is negative infinity, return 0
        return this.RoundToPrecision(
            this.helper.CreateNewWithFlags(EInteger.Zero, EInteger.Zero, 0),
            ctx);
      }
      EInteger powExponent = this.helper.GetExponent(pow);
      bool isPowIntegral = powExponent.Sign > 0;
      var isPowOdd = false;
      T powInt = default(T);
      if (!isPowIntegral) {
        powInt = this.Quantize(
            pow,
            this.helper.CreateNewWithFlags(EInteger.Zero, EInteger.Zero, 0),
            EContext.ForRounding(ERounding.Down));
        isPowIntegral = this.CompareTo(powInt, pow) == 0;
        isPowOdd = !this.helper.GetMantissa(powInt).IsEven;
      } else {
        if (powExponent.Equals(EInteger.Zero)) {
          isPowOdd = !this.helper.GetMantissa(powInt).IsEven;
        } else if (this.thisRadix % 2 == 0) {
          // Never odd for even radixes
          isPowOdd = false;
        } else {
          // DebugUtility.Log("trying to quantize " + pow);
          powInt = this.Quantize(
              pow,
              this.helper.CreateNewWithFlags(EInteger.Zero, EInteger.Zero, 0),
              EContext.ForRounding(ERounding.Down));
          isPowOdd = !this.helper.GetMantissa(powInt).IsEven;
        }
      }
      // DebugUtility.Log("pow=" + pow + " powint=" + powInt);
      bool isResultNegative = (thisFlags & BigNumberFlags.FlagNegative) != 0 &&
        (powFlags & BigNumberFlags.FlagInfinity) == 0 && isPowIntegral &&
        isPowOdd;
      if (thisSign == 0 && powSign != 0) {
        int infinityFlags = (powSign < 0) ? BigNumberFlags.FlagInfinity : 0;
        if (isResultNegative) {
          infinityFlags |= BigNumberFlags.FlagNegative;
        }
        thisValue = this.helper.CreateNewWithFlags(
            EInteger.Zero,
            EInteger.Zero,
            infinityFlags);
        if ((infinityFlags & BigNumberFlags.FlagInfinity) == 0) {
          thisValue = this.RoundToPrecision(thisValue, ctx);
        }
        return thisValue;
      }
      if ((!isPowIntegral || powSign < 0) && (ctx == null ||
          !ctx.HasMaxPrecision)) {
        // TODO: In next major version, support the case when:
        // - ctx is null or has unlimited precision, and
        // - thisValue is less than 0.
        // This case is trivial: divide 1 by thisValue^abs(pow).
        const string ValueOutputMessage =
          "ctx is null or has unlimited precision, " +
          "and pow's exponent is not an integer or is negative";
        return this.SignalInvalidWithMessage(
            ctx,
            ValueOutputMessage);
      }
      if (thisSign < 0 && !isPowIntegral) {
        return this.SignalInvalid(ctx);
      }
      if ((thisFlags & BigNumberFlags.FlagInfinity) != 0) {
        // This value is infinity
        int negflag = isResultNegative ? BigNumberFlags.FlagNegative : 0;
        return (powSign > 0) ? this.RoundToPrecision(
            this.helper.CreateNewWithFlags(
              EInteger.Zero,
              EInteger.Zero,
              negflag | BigNumberFlags.FlagInfinity),
            ctx) : ((powSign < 0) ? this.RoundToPrecision(
              this.helper.CreateNewWithFlags(
                EInteger.Zero,
                EInteger.Zero,
                negflag),
              ctx) : this.RoundToPrecision(
              this.helper.CreateNewWithFlags(EInteger.One, EInteger.Zero, 0),
              ctx));
      }
      if (powSign == 0) {
        return
          this.RoundToPrecision(
            this.helper.CreateNewWithFlags(EInteger.One, EInteger.Zero, 0),
            ctx);
      }
      if (isPowIntegral) {
        EInteger signedMant;
        // Special case for 1 in certain cases
        if (this.CompareTo(thisValue, this.helper.ValueOf(1)) == 0 &&
          isPowIntegral) {
          EInteger thisExponent = this.helper.GetExponent(thisValue);
          if (thisExponent.Sign == 0) {
            return (!this.IsWithinExponentRangeForPow(pow, ctx)) ?
              this.SignalInvalid(ctx) : this.helper.ValueOf(1);
          } else if (powExponent.Sign == 0) {
            if (!this.IsWithinExponentRangeForPow(pow, ctx)) {
              return this.SignalInvalid(ctx);
            }
            signedMant = this.helper.GetMantissa(powInt).Abs();
            if (powSign < 0) {
               // Use this line because in this case, where
               // thisValue is 1 and power is a negative integer, the reciprocal of 1
               // is used, which will have an exponent of 0, according to the
               // General Decimal Arithmetic Specification
               return this.PowerIntegral(this.helper.ValueOf(1), signedMant, ctx);
            } else {
               return this.PowerIntegral(thisValue, signedMant, ctx);
            }
          }
        }
        // Very high values of pow and a very high exponent
        if (powExponent.CompareTo(10) > 0 &&
          this.CompareTo(pow, this.helper.ValueOf(99999999)) > 0) {
          EContext ctxCopy = ctx.WithBlankFlags().WithTraps(0);
          // DebugUtility.Log("changing pow to 9999999*, ctx="+ctxCopy);
          // Try doing Power with a smaller value for pow
          T result = this.Power(
              thisValue,
              this.helper.ValueOf(isPowOdd ? 99999999 : 99999998),
              ctxCopy);
          if ((ctxCopy.Flags & EContext.FlagOverflow) != 0) {
            // Caused overflow
            if (ctx.HasFlags) {
              ctx.Flags |= ctxCopy.Flags;
            }
            return result;
          }
        }
        if ((object)powInt == (object)default(T)) {
          // DebugUtility.Log("no powInt, quantizing "+pow);
          powInt = this.Quantize(
              pow,
              this.helper.CreateNewWithFlags(EInteger.Zero, EInteger.Zero, 0),
              EContext.ForRounding(ERounding.Down));
        }
        signedMant = this.helper.GetMantissa(powInt);
        if (powSign < 0) {
          signedMant = -signedMant;
        }
        // DebugUtility.Log("tv=" + thisValue + " mant=" + signedMant);
        return this.PowerIntegral(thisValue, signedMant, ctx);
      }
      // Special case for 1
      if (this.CompareTo(thisValue, this.helper.ValueOf(1)) == 0
        // && powSign > 0
) {
        // DebugUtility.Log("Special case 1B");
        return (!this.IsWithinExponentRangeForPow(pow, ctx)) ?
          this.SignalInvalid(ctx) :
          this.ExtendPrecision(this.helper.ValueOf(1), ctx);
      }
      #if DEBUG
      if (ctx == null) {
        throw new ArgumentNullException(nameof(ctx));
      }
      #endif
      // Special case for 0.5
      if (this.thisRadix == 10 || this.thisRadix == 2) {
        T half = (this.thisRadix == 10) ? this.helper.CreateNewWithFlags(
            (EInteger)5,
            ValueMinusOne,
            0) : this.helper.CreateNewWithFlags(
              EInteger.One,
              ValueMinusOne,
              0);
        if (this.CompareTo(pow, half) == 0 &&
          this.IsWithinExponentRangeForPow(pow, ctx) &&
          this.IsWithinExponentRangeForPow(thisValue, ctx)) {
          EContext ctxCopy = ctx.WithBlankFlags();
          thisValue = this.SquareRoot(thisValue, ctxCopy);
          ctxCopy.Flags |= EContext.FlagInexact;
          ctxCopy.Flags |= EContext.FlagRounded;
          if ((ctxCopy.Flags & EContext.FlagSubnormal) != 0) {
            ctxCopy.Flags |= EContext.FlagUnderflow;
          }
          thisValue = this.ExtendPrecision(thisValue, ctxCopy);
          if (ctx.HasFlags) {
            ctx.Flags |= ctxCopy.Flags;
          }
          return thisValue;
        }
      }
      EInteger upperBoundInt = NumberUtility.IntegerDigitLengthUpperBound(
         this.helper,
         powInt);
      upperBoundInt = EInteger.Min(EInteger.FromInt32(50), upperBoundInt);
      EInteger guardDigits = this.WorkingDigits(EInteger.FromInt32(15));
      guardDigits = guardDigits.Add(upperBoundInt);
      /*if (upperBoundInt.CompareTo(50) > 0) {
      DebugUtility.Log("guardDigits=" + guardDigits +
        " upperBoundInt=" + upperBoundInt + " powint=" + powInt);
      }*/ EContext ctxdiv = SetPrecisionIfLimited(
          ctx,
          ctx.Precision + guardDigits);
      if (ctx.Rounding != ERounding.Ceiling &&
        ctx.Rounding != ERounding.Floor) {
        ctxdiv = ctxdiv.WithRounding(ctx.Rounding)
          .WithBlankFlags();
      } else {
        ctxdiv = ctxdiv.WithRounding(ERounding.Up)
          .WithBlankFlags();
      }
      T lnresult = this.Ln(thisValue, ctxdiv);
      // DebugUtility.Log("rounding="+ctxdiv.Rounding);
      // DebugUtility.Log("before mul="+lnresult);
      lnresult = this.Multiply(lnresult, pow, ctxdiv);
      EInteger workingPrecision = ctxdiv.Precision;
      // Now use original precision and rounding mode
      ctxdiv = ctx.WithBlankFlags();
      // DebugUtility.Log("before exp="+lnresult);
      lnresult = this.Exp(lnresult, ctxdiv);
      // DebugUtility.Log("after exp.="+lnresult);
      if ((ctxdiv.Flags & (EContext.FlagClamped |
            EContext.FlagOverflow)) != 0) {
        if (!this.IsWithinExponentRangeForPow(thisValue, ctx)) {
          return this.SignalInvalid(ctx);
        }
        if (!this.IsWithinExponentRangeForPow(pow, ctx)) {
          return this.SignalInvalid(ctx);
        }
      }
      if (ctx.HasFlags) {
        ctx.Flags |= ctxdiv.Flags;
      }
      return lnresult;
    }

    private bool IsSubnormal(T value, EContext ctx) {
      bool flag = ctx == null || !ctx.HasMaxPrecision;
      bool result;
      if (flag) {
        result = false;
      } else {
        FastInteger fastInteger = FastInteger.FromBig(
            this.helper.GetExponent(value));
        FastInteger val = FastInteger.FromBig(ctx.EMin);
        bool adjustExponent = ctx.AdjustExponent;
        if (adjustExponent) {
          FastInteger digitLength =
            this.helper.GetDigitLength(this.helper.GetMantissa(value));
          fastInteger.Add(digitLength).SubtractInt(1);
        }
        result = fastInteger.CompareTo(val) < 0;
      }
      return result;
    }

    public T Quantize(T thisValue, T otherValue, EContext ctx) {
      int thisFlags = this.helper.GetFlags(thisValue);
      int otherFlags = this.helper.GetFlags(otherValue);
      if (((thisFlags | otherFlags) & BigNumberFlags.FlagSpecial) != 0) {
        T result = this.HandleNotANumber(thisValue, otherValue, ctx);
        if ((object)result != (object)default(T)) {
          return result;
        }
        if (((thisFlags & otherFlags) & BigNumberFlags.FlagInfinity) != 0) {
          return this.RoundToPrecision(thisValue, ctx);
        }
        // At this point, it's only the case that either value
        // is infinity
        return this.SignalInvalid(ctx);
      }
      EInteger expOther = this.helper.GetExponent(otherValue);
      if (ctx != null && !ctx.ExponentWithinRange(expOther)) {
        // DebugUtility.Log("exp not within range");
        return this.SignalInvalidWithMessage(
            ctx,
            "Exponent not within exponent range: " + expOther);
      }
      EContext tmpctx = (ctx == null ?
          EContext.ForRounding(ERounding.HalfEven) :
          ctx.Copy()).WithBlankFlags();
      EInteger mantThis = this.helper.GetMantissa(thisValue);
      EInteger expThis = this.helper.GetExponent(thisValue);
      int expcmp = expThis.CompareTo(expOther);
      int negativeFlag = this.helper.GetFlags(thisValue) &
        BigNumberFlags.FlagNegative;
      T ret = default(T);
      if (expcmp == 0) {
        // DebugUtility.Log("exp same");
        ret = this.RoundToPrecision(thisValue, tmpctx);
      } else if (mantThis.IsZero) {
        // DebugUtility.Log("mant is 0");
        ret = this.helper.CreateNewWithFlags(
            EInteger.Zero,
            expOther,
            negativeFlag);
        ret = this.RoundToPrecision(ret, tmpctx);
      } else if (expcmp > 0) {
        // Other exponent is less
        // DebugUtility.Log("other exp less");
        FastInteger radixPower =
          FastInteger.FromBig(expThis).SubtractBig(expOther);
        if (tmpctx.Precision.Sign > 0 &&
          radixPower.CompareTo(FastInteger.FromBig(tmpctx.Precision)
            .AddInt(10)) > 0) {
          // Radix power is much too high for the current precision
          // DebugUtility.Log("result too high for prec:" +
          // tmpctx.Precision + " radixPower= " + radixPower);
          return this.SignalInvalidWithMessage(
              ctx,
              "Result too high for current precision");
        }
        mantThis = this.TryMultiplyByRadixPower(mantThis, radixPower);
        if (mantThis == null) {
          return this.SignalInvalidWithMessage(
              ctx,
              "Result requires too much memory");
        }
        ret = this.helper.CreateNewWithFlags(mantThis, expOther, negativeFlag);
        ret = this.RoundToPrecision(ret, tmpctx);
      } else {
        // Other exponent is greater
        // DebugUtility.Log("other exp greater");
        FastInteger shift = FastInteger.FromBig(expOther).SubtractBig(
            expThis);
        ret = this.RoundToPrecisionInternal(
            thisValue,
            0,
            0,
            shift,
            false,
            tmpctx);
      }
      if ((tmpctx.Flags & EContext.FlagOverflow) != 0) {
        // DebugUtility.Log("overflow occurred");
        return this.SignalInvalid(ctx);
      }
      if (ret == null || !this.helper.GetExponent(ret).Equals(expOther)) {
        // DebugUtility.Log("exp not same "+ret);
        return this.SignalInvalid(ctx);
      }
      ret = this.EnsureSign(ret, negativeFlag != 0);
      if (ctx != null && ctx.HasFlags) {
        int flags = tmpctx.Flags;
        flags &= ~EContext.FlagUnderflow;
        bool flag12 = expcmp < 0 && !this.helper.GetMantissa(ret).IsZero &&
          this.IsSubnormal(ret, ctx);
        if (flag12) {
          flags |= EContext.FlagSubnormal;
        }
        ctx.Flags |= flags;
      }
      return ret;
    }

    public T Reduce(T thisValue, EContext ctx) {
      return this.ReduceToPrecisionAndIdealExponent(
          thisValue,
          ctx,
          null,
          null);
    }

    public T Remainder(
      T thisValue,
      T divisor,
      EContext ctx,
      bool roundAfterDivide) {
      EContext ctx2 = ctx == null ? null : ctx.WithBlankFlags();
      T ret = this.RemainderHandleSpecial(thisValue, divisor, ctx2);
      if ((object)ret != (object)default(T)) {
        TransferFlags(ctx, ctx2);
        return ret;
      }
      ret = this.DivideToIntegerZeroScale(
          thisValue,
          divisor,
          roundAfterDivide ? ctx2 : null);
      if ((ctx2.Flags & EContext.FlagInvalid) != 0) {
        return this.SignalInvalid(ctx);
      }
      ret = this.Add(
          thisValue,
          this.NegateRaw(this.Multiply(ret, divisor, null)),
          ctx2);
      ret = this.EnsureSign(
          ret,
          (this.helper.GetFlags(thisValue) & BigNumberFlags.FlagNegative) != 0);

      TransferFlags(
        ctx,
        ctx2);
      return ret;
    }

    public T RemainderNear(T thisValue, T divisor, EContext ctx) {
      EContext ctx2 = ctx == null ?
        EContext.ForRounding(ERounding.HalfEven).WithBlankFlags() :
        ctx.WithRounding(ERounding.HalfEven).WithBlankFlags();
      T ret = this.RemainderHandleSpecial(thisValue, divisor, ctx2);
      if ((object)ret != (object)default(T)) {
        TransferFlags(ctx, ctx2);
        return ret;
      }
      ret = this.DivideInternal(
          thisValue,
          divisor,
          ctx2,
          IntegerModeFixedScale,
          EInteger.Zero);
      if ((ctx2.Flags & EContext.FlagInvalid) != 0) {
        return this.SignalInvalid(ctx);
      }
      ctx2 = ctx2.WithBlankFlags();
      ret = this.RoundToPrecision(ret, ctx2);
      if ((ctx2.Flags & (EContext.FlagRounded |
            EContext.FlagInvalid)) != 0) {
        return this.SignalInvalid(ctx);
      }
      ctx2 = ctx == null ? EContext.UnlimitedHalfEven.WithBlankFlags() :
        ctx.WithBlankFlags();
      T ret2 = this.Add(
          thisValue,
          this.NegateRaw(this.Multiply(ret, divisor, null)),
          ctx2);
      if ((ctx2.Flags & EContext.FlagInvalid) != 0) {
        return this.SignalInvalid(ctx);
      }
      if (this.helper.GetFlags(ret2) == 0 &&
        this.helper.GetMantissa(ret2).IsZero) {
        ret2 = this.EnsureSign(
          ret2,
          (this.helper.GetFlags(thisValue) & BigNumberFlags.FlagNegative) != 0);
      }
      TransferFlags(
        ctx,
        ctx2);
      return ret2;
    }

    public T RoundAfterConversion(T thisValue, EContext ctx) {
      // DebugUtility.Log("RM RoundAfterConversion");
      return this.RoundToPrecision(thisValue, ctx);
    }

    public T RoundToExponentExact(
      T thisValue,
      EInteger expOther,
      EContext ctx) {
      if (this.helper.GetExponent(thisValue).CompareTo(expOther) >= 0) {
        return this.RoundToPrecision(thisValue, ctx);
      } else {
        EContext pctx = (ctx == null) ? null :
          ctx.WithPrecision(0).WithBlankFlags();
        T ret = this.Quantize(
            thisValue,
            this.helper.CreateNewWithFlags(EInteger.One, expOther, 0),
            pctx);
        if (ctx != null && ctx.HasFlags) {
          ctx.Flags |= pctx.Flags;
        }
        return ret;
      }
    }

    public T RoundToExponentNoRoundedFlag(
      T thisValue,
      EInteger exponent,
      EContext ctx) {
      EContext pctx = (ctx == null) ? null : ctx.WithBlankFlags();
      T ret = this.RoundToExponentExact(thisValue, exponent, pctx);
      if (ctx != null && ctx.HasFlags) {
        ctx.Flags |= pctx.Flags & ~(EContext.FlagInexact |
            EContext.FlagRounded);
      }
      return ret;
    }

    public T RoundToExponentSimple(
      T thisValue,
      EInteger expOther,
      EContext ctx) {
      int thisFlags = this.helper.GetFlags(thisValue);
      if ((thisFlags & BigNumberFlags.FlagSpecial) != 0) {
        T result = this.HandleNotANumber(thisValue, thisValue, ctx);
        if ((object)result != (object)default(T)) {
          return result;
        }
        if ((thisFlags & BigNumberFlags.FlagInfinity) != 0) {
          return thisValue;
        }
      }
      if (this.helper.GetExponent(thisValue).CompareTo(expOther) >= 0) {
        return this.RoundToPrecision(thisValue, ctx);
      } else {
        if (ctx != null && !ctx.ExponentWithinRange(expOther)) {
          return this.SignalInvalidWithMessage(
              ctx,
              "Exponent not within exponent range: " + expOther);
        }
        FastInteger shift = FastInteger.FromBig(expOther)
          .SubtractBig(this.helper.GetExponent(thisValue));
        if (shift.Sign == 0 && IsSimpleContext(ctx)) {
          return thisValue;
        }
        EInteger bigmantissa = this.helper.GetMantissa(thisValue);
        if (IsSimpleContext(ctx) && ctx.Rounding == ERounding.Down) {
          EInteger shiftedmant = shift.CanFitInInt32() ?
            bigmantissa.ShiftRight((int)shift.ToInt32()) :
            bigmantissa.ShiftRight(shift.ToEInteger());
          return this.helper.CreateNewWithFlags(
              shiftedmant,
              expOther,
              thisFlags);
        } else {
          IShiftAccumulator accum =
            this.helper.CreateShiftAccumulatorWithDigits(bigmantissa, 0, 0);
          accum.TruncateOrShiftRight(
            shift,
            false);
          bigmantissa = accum.ShiftedInt;
          thisValue = this.helper.CreateNewWithFlags(
              bigmantissa,
              expOther,
              thisFlags);
          return this.RoundToPrecisionInternal(
              thisValue,
              accum.LastDiscardedDigit,
              accum.OlderDiscardedDigits,
              null,
              false,
              ctx);
        }
      }
    }

    public T RoundToPrecision(T thisValue, EContext context) {
      return this.RoundToPrecisionInternal(
          thisValue,
          0,
          0,
          null,
          false,
          context);
    }

    private T Root(T thisValue, int root, EContext ctx) {
      if (ctx == null) {
        return this.SignalInvalidWithMessage(ctx, "ctx is null");
      }
      if (!ctx.HasMaxPrecision) {
        return this.SignalInvalidWithMessage(
            ctx,
            "ctx has unlimited precision");
      }
      T ret = this.SquareRootHandleSpecial(thisValue, ctx);
      if ((object)ret != (object)default(T)) {
        return ret;
      }
      EContext ctxtmp = ctx.WithBlankFlags();
      EInteger currentExp = this.helper.GetExponent(thisValue);
      EInteger origExp = currentExp;
      EInteger idealExp;
      idealExp = currentExp;
      idealExp /= EInteger.FromInt32(root);
      if (currentExp.Sign < 0 && !currentExp.IsEven) {
        // Round towards negative infinity; BigInteger's
        // division operation rounds towards zero
        idealExp -= EInteger.One;
      }
      // DebugUtility.Log("curr=" + currentExp + " ideal=" + idealExp);
      if (this.helper.GetSign(thisValue) == 0) {
        ret = this.RoundToPrecision(
            this.helper.CreateNewWithFlags(
              EInteger.Zero,
              idealExp,
              this.helper.GetFlags(thisValue)),
            ctxtmp);
        if (ctx.HasFlags) {
          ctx.Flags |= ctxtmp.Flags;
        }
        return ret;
      }
      EInteger mantissa = this.helper.GetMantissa(thisValue);
      FastInteger digitCount = this.helper.GetDigitLength(mantissa);
      FastInteger targetPrecision = FastInteger.FromBig(ctx.Precision);
      FastInteger precision = targetPrecision.Copy().Multiply(root).AddInt(2);
      var rounded = false;
      var inexact = false;
      if (digitCount.CompareTo(precision) < 0) {
        FastInteger diff = precision.Copy().Subtract(digitCount);
        // DebugUtility.Log(diff);
        if ((!diff.IsEvenNumber) ^ (!origExp.IsEven)) {
          diff.Increment();
        }
        EInteger bigdiff = diff.ToEInteger();
        currentExp -= (EInteger)bigdiff;
        mantissa = this.TryMultiplyByRadixPower(mantissa, diff);
        if (mantissa == null) {
          return this.SignalInvalidWithMessage(
              ctx,
              "Result requires too much memory");
        }
      }
      EInteger[] sr = mantissa.RootRem(root);
      digitCount = this.helper.GetDigitLength(sr[0]);
      EInteger rootRemainder = sr[1];
      // DebugUtility.Log("I " + mantissa + " -> " + sr[0] + " [target="+
      // targetPrecision + "], (zero= " + rootRemainder.IsZero +") "
      mantissa = sr[0];
      if (!rootRemainder.IsZero) {
        rounded = true;
        inexact = true;
      }
      EInteger oldexp = currentExp;
      currentExp = currentExp.ShiftRight(1);
      if (oldexp.Sign < 0 && !oldexp.IsEven) {
        // Round towards negative infinity; BigInteger's
        // division operation rounds towards zero
        currentExp -= EInteger.One;
      }
      T retval = this.helper.CreateNewWithFlags(mantissa, currentExp, 0);
      // DebugUtility.Log("idealExp= " + idealExp + ", curr" + currentExp
      // +" guess= " + mantissa);
      retval = this.RoundToPrecisionInternal(
          retval,
          0,
          inexact ? 1 : 0,
          null,
          false,
          ctxtmp);
      currentExp = this.helper.GetExponent(retval);
      // DebugUtility.Log("guess I " + guess + " idealExp=" + idealExp
      // +", curr " + currentExp + " clamped= " +
      // (ctxtmp.Flags&PrecisionContext.FlagClamped));
      if ((ctxtmp.Flags & EContext.FlagUnderflow) == 0) {
        int expcmp = currentExp.CompareTo(idealExp);
        if (expcmp <= 0 || !this.IsFinite(retval)) {
          retval = this.ReduceToPrecisionAndIdealExponent(
              retval,
              ctx.HasExponentRange ? ctxtmp : null,
              inexact ? targetPrecision : null,
              FastInteger.FromBig(idealExp));
        }
      }
      if (ctx.HasFlags) {
        if (ctx.ClampNormalExponents &&
          !this.helper.GetExponent(retval).Equals(idealExp) && (ctxtmp.Flags &
            EContext.FlagInexact) == 0) {
          ctx.Flags |= EContext.FlagClamped;
        }
        rounded |= (ctxtmp.Flags & EContext.FlagOverflow) != 0;
        // DebugUtility.Log("guess II " + guess);
        currentExp = this.helper.GetExponent(retval);
        if (rounded) {
          ctxtmp.Flags |= EContext.FlagRounded;
        } else {
          if (currentExp.CompareTo(idealExp) > 0) {
            // Greater than the ideal, treat as rounded anyway
            ctxtmp.Flags |= EContext.FlagRounded;
          } else {
            // DebugUtility.Log("idealExp= " + idealExp + ", curr" +
            // currentExp + " (II)");
            ctxtmp.Flags &= ~EContext.FlagRounded;
          }
        }
        if (inexact) {
          ctxtmp.Flags |= EContext.FlagRounded;
          ctxtmp.Flags |= EContext.FlagInexact;
        }
        ctx.Flags |= ctxtmp.Flags;
      }
      return retval;
    }

    public T SquareRoot(T thisValue, EContext ctx) {
      if (ctx == null) {
        return this.SignalInvalidWithMessage(ctx, "ctx is null");
      }
      if (!ctx.HasMaxPrecision) {
        return this.SignalInvalidWithMessage(
            ctx,
            "ctx has unlimited precision");
      }
      T ret = this.SquareRootHandleSpecial(thisValue, ctx);
      if ((object)ret != (object)default(T)) {
        return ret;
      }
      EContext ctxtmp = ctx.WithBlankFlags();
      EInteger currentExp = this.helper.GetExponent(thisValue);
      EInteger origExp = currentExp;
      EInteger idealExp;
      idealExp = currentExp;
      idealExp /= (EInteger)2;
      if (currentExp.Sign < 0 && !currentExp.IsEven) {
        // Round towards negative infinity; BigInteger's
        // division operation rounds towards zero
        idealExp -= EInteger.One;
      }
      // DebugUtility.Log("curr=" + currentExp + " ideal=" + idealExp);
      if (this.helper.GetSign(thisValue) == 0) {
        ret = this.RoundToPrecision(
            this.helper.CreateNewWithFlags(
              EInteger.Zero,
              idealExp,
              this.helper.GetFlags(thisValue)),
            ctxtmp);
        if (ctx.HasFlags) {
          ctx.Flags |= ctxtmp.Flags;
        }
        return ret;
      }
      EInteger mantissa = this.helper.GetMantissa(thisValue);
      FastInteger digitCount = this.helper.GetDigitLength(mantissa);
      FastInteger targetPrecision = FastInteger.FromBig(ctx.Precision);
      FastInteger precision = targetPrecision.Copy().Multiply(2).AddInt(2);
      var rounded = false;
      var inexact = false;
      if (digitCount.CompareTo(precision) < 0) {
        FastInteger diff = precision.Copy().Subtract(digitCount);
        // DebugUtility.Log("precisiondiff=" + diff);
        if ((!diff.IsEvenNumber) ^ (!origExp.IsEven)) {
          diff.Increment();
        }
        EInteger bigdiff = diff.ToEInteger();
        currentExp -= (EInteger)bigdiff;
        mantissa = this.TryMultiplyByRadixPower(mantissa, diff);
        if (mantissa == null) {
          return this.SignalInvalidWithMessage(
              ctx,
              "Result requires too much memory");
        }
      }
      EInteger[] sr = mantissa.SqrtRem();
      digitCount = this.helper.GetDigitLength(sr[0]);
      EInteger squareRootRemainder = sr[1];
      // DebugUtility.Log("I " + mantissa + " -> " + sr[0] +
      // " [target="+
      // targetPrecision + "], (zero= " +
      // squareRootRemainder.IsZero +") ");
      mantissa = sr[0];
      if (!squareRootRemainder.IsZero) {
        rounded = true;
        inexact = true;
      }
      EInteger oldexp = currentExp;
      currentExp = currentExp.ShiftRight(1);
      if (oldexp.Sign < 0 && !oldexp.IsEven) {
        // Round towards negative infinity; BigInteger's
        // division operation rounds towards zero
        currentExp -= EInteger.One;
      }
      T retval = this.helper.CreateNewWithFlags(mantissa, currentExp, 0);
      // DebugUtility.Log("idealExp= " + idealExp + ", curr" + currentExp
      // +" guess= " + mantissa);
      retval = this.RoundToPrecisionInternal(
          retval,
          0,
          inexact ? 1 : 0,
          null,
          false,
          ctxtmp);
      currentExp = this.helper.GetExponent(retval);
      // DebugUtility.Log("guess I " + guess + " idealExp=" + idealExp
      // +", curr " + currentExp + " clamped= " +
      // (ctxtmp.Flags&PrecisionContext.FlagClamped));
      if ((ctxtmp.Flags & EContext.FlagUnderflow) == 0) {
        int expcmp = currentExp.CompareTo(idealExp);
        if (expcmp <= 0 || !this.IsFinite(retval)) {
          retval = this.ReduceToPrecisionAndIdealExponent(
              retval,
              ctx.HasExponentRange ? ctxtmp : null,
              inexact ? targetPrecision : null,
              FastInteger.FromBig(idealExp));
        }
      }
      if (ctx.HasFlags) {
        if (ctx.ClampNormalExponents &&
          !this.helper.GetExponent(retval).Equals(idealExp) && (ctxtmp.Flags &
            EContext.FlagInexact) == 0) {
          ctx.Flags |= EContext.FlagClamped;
        }
        rounded |= (ctxtmp.Flags & EContext.FlagOverflow) != 0;
        // DebugUtility.Log("guess II " + guess);
        currentExp = this.helper.GetExponent(retval);
        if (rounded) {
          ctxtmp.Flags |= EContext.FlagRounded;
        } else {
          if (currentExp.CompareTo(idealExp) > 0) {
            // Greater than the ideal, treat as rounded anyway
            ctxtmp.Flags |= EContext.FlagRounded;
          } else {
            // DebugUtility.Log("idealExp= " + idealExp + ", curr" +
            // currentExp + " (II)");
            ctxtmp.Flags &= ~EContext.FlagRounded;
          }
        }
        if (inexact) {
          ctxtmp.Flags |= EContext.FlagRounded;
          ctxtmp.Flags |= EContext.FlagInexact;
        }
        ctx.Flags |= ctxtmp.Flags;
      }
      return retval;
    }

    private static int CompareToFast(
      int e1int,
      int e2int,
      int expcmp,
      int signA,
      FastIntegerFixed op1Mantissa,
      FastIntegerFixed op2Mantissa,
      int radix) {
      int m1, m2;
      // DebugUtility.Log("" + (// e1int) + " " + e2int + ", expcmp=" +
      // expcmp + ", signA=" + signA + ", om=" + op1Mantissa + ", " +
      // op2Mantissa);
      if (e1int >= SafeMin32 && e1int <= SafeMax32 &&
        e2int >= SafeMin32 && e2int <= SafeMax32) {
        int ediff = (e1int > e2int) ? (e1int - e2int) : (e2int - e1int);
        if (ediff <= 9 && radix == 10) {
          int power = ValueTenPowers[ediff];
          int maxoverflow = OverflowMaxes[ediff];
          if (expcmp > 0) {
            m1 = op1Mantissa.ToInt32();
            m2 = op2Mantissa.ToInt32();
            if (m1 <= maxoverflow) {
              m1 *= power;
              return (m1 == m2) ? 0 : ((m1 < m2) ? -signA : signA);
            }
          } else {
            m1 = op1Mantissa.ToInt32();
            m2 = op2Mantissa.ToInt32();
            if (m2 <= maxoverflow) {
              m2 *= power;
              return (m1 == m2) ? 0 : ((m1 < m2) ? -signA : signA);
            }
          }
        } else if (ediff <= 30 && radix == 2) {
          int mask = BitMasks[ediff];
          if (expcmp > 0) {
            m1 = op1Mantissa.ToInt32();
            m2 = op2Mantissa.ToInt32();
            if ((m1 & mask) == m1) {
              m1 <<= ediff;
              return (m1 == m2) ? 0 : ((m1 < m2) ? -signA : signA);
            }
          } else {
            m1 = op1Mantissa.ToInt32();
            m2 = op2Mantissa.ToInt32();
            if ((m2 & mask) == m2) {
              m2 <<= ediff;
              return (m1 == m2) ? 0 : ((m1 < m2) ? -signA : signA);
            }
          }
        }
      }
      return 2;
    }

    private static int CompareToFast64(
      int e1int,
      int e2int,
      int expcmp,
      int signA,
      FastIntegerFixed op1Mantissa,
      FastIntegerFixed op2Mantissa,
      int radix) {
      long m1, m2;
      // DebugUtility.Log("" + (// e1int) + " " + e2int + ", expcmp=" +
      // expcmp + ", signA=" + signA + ", om=" + op1Mantissa + ", " +
      // op2Mantissa);
      if (e1int >= SafeMin32 && e1int <= SafeMax32 &&
        e2int >= SafeMin32 && e2int <= SafeMax32) {
        long ediffLong = (e1int > e2int) ? (e1int - e2int) : (e2int - e1int);
        if (ediffLong <= 18 && radix == 10) {
          long power = ValueTenPowers64[(int)ediffLong];
          long maxoverflow = OverflowMaxes64[(int)ediffLong];
          if (expcmp > 0) {
            m1 = op1Mantissa.ToInt64();
            m2 = op2Mantissa.ToInt64();
            // DebugUtility.Log("overflowmax " + maxoverflow + " for " + m1);
            if (m1 <= maxoverflow) {
              m1 *= power;
              return (m1 == m2) ? 0 : ((m1 < m2) ? -signA : signA);
            }
          } else {
            m1 = op1Mantissa.ToInt64();
            m2 = op2Mantissa.ToInt64();
            // DebugUtility.Log("overflowmax " + maxoverflow + " for " + m2);
            if (m2 <= maxoverflow) {
              m2 *= power;
              return (m1 == m2) ? 0 : ((m1 < m2) ? -signA : signA);
            }
          }
        } else if (ediffLong <= 62 && radix == 2) {
          long mask = BitMasks64[(int)ediffLong];
          if (expcmp > 0) {
            m1 = op1Mantissa.ToInt64();
            m2 = op2Mantissa.ToInt64();
            if ((m1 & mask) == m1) {
              m1 <<= (int)ediffLong;
              return (m1 == m2) ? 0 : ((m1 < m2) ? -signA : signA);
            }
          } else {
            m1 = op1Mantissa.ToInt64();
            m2 = op2Mantissa.ToInt64();
            if ((m2 & mask) == m2) {
              m2 <<= (int)ediffLong;
              return (m1 == m2) ? 0 : ((m1 < m2) ? -signA : signA);
            }
          }
        }
      }
      return 2;
    }

    private static int CompareToSlow<TMath>(
      EInteger op1Exponent,
      EInteger op2Exponent,
      int expcmp,
      int signA,
      EInteger op1Mantissa,
      EInteger op2Mantissa,
      IRadixMathHelper<TMath> helper,
      bool reportOOM) {
      #if DEBUG
      if (op1Mantissa.IsZero) {
        throw new InvalidOperationException();
      }
      if (op2Mantissa.IsZero) {
        throw new InvalidOperationException();
      }
      #endif
      long bitExp1 = op1Exponent.GetUnsignedBitLengthAsInt64();
      long bitExp2 = op2Exponent.GetUnsignedBitLengthAsInt64();
      if (bitExp1 < Int64.MaxValue && bitExp2 < Int64.MaxValue &&
        helper.GetRadix() <= 10 && op1Exponent.Sign == op2Exponent.Sign && (
          (bitExp2 > bitExp1 && (bitExp2 - bitExp1) > 128) ||
          (bitExp1 > bitExp2 && (bitExp1 - bitExp2) > 128))) {
        // Bit difference in two exponents means exponent difference
        // is so big that the digit counts of the two significands
        // can't keep up (that is, exponent difference is greater than 2^128,
        // which is more than the maximum number of bits that
        // a significand can currently have).
        bool op2bigger = op1Exponent.Sign < 0 ? (bitExp2 < bitExp1) :
          (bitExp2 > bitExp1);
        if (op2bigger) {
          // operand 2 has greater magnitude
          return signA < 0 ? 1 : -1;
        } else {
          // operand 1 has greater magnitude
          return signA < 0 ? -1 : 1;
        }
      }
      FastInteger fastOp1Exp = FastInteger.FromBig(op1Exponent);
      FastInteger fastOp2Exp = FastInteger.FromBig(op2Exponent);
      FastInteger expdiff = fastOp1Exp.Copy().Subtract(fastOp2Exp).Abs();
      // Check if exponent difference is too big for
      // radix-power calculation to work quickly
      if (expdiff.CompareToInt(200) >= 0) {
        EInteger op1MantAbs = op1Mantissa;
        EInteger op2MantAbs = op2Mantissa;
        FastInteger[] op1DigitBounds =
          NumberUtility.DigitLengthBounds(helper, op1MantAbs);
        FastInteger[] op2DigitBounds =
          NumberUtility.DigitLengthBounds(helper, op2MantAbs);
        FastInteger op2ExpUpperBound = fastOp2Exp.Copy().Add(
            op2DigitBounds[1]);
        FastInteger op1ExpLowerBound = fastOp1Exp.Copy().Add(
            op1DigitBounds[0]);
        if (op2ExpUpperBound.CompareTo(op1ExpLowerBound) < 0) {
          // Operand 2's magnitude can't reach highest digit of operand 1,
          // meaning operand 1 has a greater magnitude
          return signA < 0 ? -1 : 1;
        }
        FastInteger op1ExpUpperBound = fastOp1Exp.Copy().Add(
            op1DigitBounds[1]);
        FastInteger op2ExpLowerBound = fastOp2Exp.Copy().Add(
            op2DigitBounds[0]);
        // DebugUtility.Log("1ub="+op1ExpUpperBound +
        // " 2lb="+op2ExpLowerBound);
        if (op1ExpUpperBound.CompareTo(op2ExpLowerBound) < 0) {
          // Operand 1's magnitude can't reach highest digit of operand 2,
          // meaning operand 2 has a greater magnitude
          return signA < 0 ? 1 : -1;
        }
        FastInteger precision1 =
          op1DigitBounds[0].CompareTo(op1DigitBounds[1]) == 0 ?
          op1DigitBounds[0] : helper.GetDigitLength(op1MantAbs);
        FastInteger precision2 =
          op2DigitBounds[0].CompareTo(op2DigitBounds[1]) == 0 ?
          op2DigitBounds[0] : helper.GetDigitLength(op2MantAbs);
        FastInteger exp1 = fastOp1Exp.Copy().Add(precision1).Decrement();
        FastInteger exp2 = fastOp2Exp.Copy().Add(precision2).Decrement();
        int adjcmp = exp1.CompareTo(exp2);
        if (adjcmp != 0) {
          // DebugUtility.Log("cmp=" + ((signA < 0) ? -adjcmp : adjcmp));
          return (signA < 0) ? -adjcmp : adjcmp;
        }
        FastInteger maxPrecision = null;
        maxPrecision = (precision1.CompareTo(precision2) > 0) ? precision1 :
          precision2;
        // If exponent difference is greater than the
        // maximum precision of the two operands
        if (expdiff.Copy().CompareTo(maxPrecision) > 0) {
          int expcmp2 = fastOp1Exp.CompareTo(fastOp2Exp);
          if (expcmp2 < 0) {
            if (!op2MantAbs.IsZero) {
              // first operand's exponent is less
              // and second operand isn't zero
              // second mantissa will be shifted by the exponent
              // difference
              FastInteger digitLength1 = helper.GetDigitLength(
                  op1MantAbs);
              if (fastOp1Exp.Copy().Add(digitLength1).AddInt(2)
                .CompareTo(fastOp2Exp) < 0) {
                // first operand's mantissa can't reach the
                // second operand's mantissa, so the exponent can be
                // raised without affecting the result
                FastInteger tmp = fastOp2Exp.Copy()
                  .SubtractInt(8).Subtract(digitLength1).Subtract(
                    maxPrecision);
                FastInteger newDiff = tmp.Copy().Subtract(fastOp2Exp).Abs();
                if (newDiff.CompareTo(expdiff) < 0) {
                  // At this point, both operands have the same sign
                  // DebugUtility.Log("cmp case 1=" + ((signA < 0) ? 1 : -1));
                  return (signA < 0) ? 1 : -1;
                }
              }
            }
          } else if (expcmp2 > 0) {
            if (!op1MantAbs.IsZero) {
              // first operand's exponent is greater
              // and second operand isn't zero
              // first mantissa will be shifted by the exponent
              // difference
              FastInteger digitLength2 = helper.GetDigitLength(
                  op2MantAbs);
              if (fastOp2Exp.Copy()
                .Add(digitLength2).AddInt(2).CompareTo(fastOp1Exp) <
                0) {
                // second operand's mantissa can't reach the
                // first operand's mantissa, so the exponent can be
                // raised without affecting the result
                FastInteger tmp = fastOp1Exp.Copy()
                  .SubtractInt(8).Subtract(digitLength2).Subtract(
                    maxPrecision);
                FastInteger newDiff = tmp.Copy().Subtract(fastOp1Exp).Abs();
                if (newDiff.CompareTo(expdiff) < 0) {
                  // At this point, both operands have the same sign
                  // DebugUtility.Log("cmp case 2=" + ((signA < 0) ? -1 : 1));
                  return (signA < 0) ? -1 : 1;
                }
              }
            }
          }
          expcmp = op1Exponent.CompareTo((EInteger)op2Exponent);
        }
        // DebugUtility.Log("must rescale, expcmp=" + (expcmp));
      }
      if (expcmp > 0) {
        // if ((op1Exponent-op2Exponent).Abs() > 10) {
        // DebugUtility.Log("" + op1Mantissa + " " + op2Mantissa + " [exp="
        // + op1Exponent + " " + op2Exponent + "]");
        // }
        EInteger newmant = RescaleByExponentDiff(
            op1Mantissa,
            op1Exponent,
            op2Exponent,
            helper);
        if (newmant == null) {
          if (reportOOM) {
            throw new OutOfMemoryException("Result requires too much memory");
          }
          return -2;
        }
        int mantcmp = newmant.CompareTo(op2Mantissa);
        return (signA < 0) ? -mantcmp : mantcmp;
      } else {
        // if ((op1Exponent-op2Exponent).Abs() > 10) {
        // DebugUtility.Log("" + op1Mantissa + " " + op2Mantissa + " [exp="
        // + op1Exponent + " " + op2Exponent + "]");
        // }
        EInteger newmant = RescaleByExponentDiff(
            op2Mantissa,
            op1Exponent,
            op2Exponent,
            helper);
        if (newmant == null) {
          if (reportOOM) {
            throw new OutOfMemoryException("Result requires too much memory");
          }
          return -2;
        }
        int mantcmp = op1Mantissa.CompareTo(newmant);
        return (signA < 0) ? -mantcmp : mantcmp;
      }
    }

    private static bool IsNullOrSimpleContext(EContext ctx) {
      return ctx == null || ctx == EContext.UnlimitedHalfEven ||
        (!ctx.HasExponentRange && !ctx.HasMaxPrecision && ctx.Traps == 0 &&
          !ctx.HasFlags);
    }

    private static bool IsSimpleContext(EContext ctx) {
      return ctx != null && (ctx == EContext.UnlimitedHalfEven ||
          (!ctx.HasExponentRange && !ctx.HasMaxPrecision && ctx.Traps == 0 &&
            !ctx.HasFlags));
    }

    private static EInteger PowerOfTwo(FastInteger fi) {
      if (fi.Sign <= 0) {
        return EInteger.One;
      }
      if (fi.CanFitInInt32()) {
        return EInteger.One.ShiftLeft(fi.ToInt32());
      } else {
        return EInteger.One.ShiftLeft(fi.ToEInteger());
      }
    }

    // Calculated as floor(ln(i)*100/ln(2)), except the
    // entry at i is 0.
    private static readonly int[] BitsPerDigit = {
      0, 0, 100, 158, 200, 232, 258, 280, 300, 316, 332,
    };

    private static FastIntegerFixed RescaleByExponentDiff<TMath>(
      FastIntegerFixed mantissa,
      FastIntegerFixed fe1,
      FastIntegerFixed fe2,
      IRadixMathHelper<TMath> helper) {
      if (mantissa.Sign == 0) {
        return FastIntegerFixed.FromInt32(0);
      }
      // DebugUtility.Log("RescaleByExponentDiff "+fe1+" "+fe2);
      FastIntegerFixed eidiff = fe1.Subtract(fe2).Abs();
      EInteger eiBitCount =
        mantissa.ToEInteger().GetUnsignedBitLengthAsEInteger();
      EInteger eidiffBigInt = eidiff.ToEInteger();
      // NOTE: For radix 10, each digit fits less than 1 byte; the
      // supported byte length is thus less than the maximum value
      // of a 32-bit integer (2GB).
      if (helper.GetRadix() <= 10) {
        int radix = helper.GetRadix();
        eiBitCount = eiBitCount.Add(eidiffBigInt.Multiply(BitsPerDigit[radix])
            .Divide(100));
        // DebugUtility.Log(""+eiBitCount);
        if (eiBitCount.CompareTo(Int32.MaxValue) > 0) {
          return null;
        }
      }
      return helper.MultiplyByRadixPowerFastInt(mantissa, eidiff);
    }

    private static EInteger RescaleByExponentDiff<TMath>(
      EInteger mantissa,
      EInteger e1,
      EInteger e2,
      IRadixMathHelper<TMath> helper) {
      if (mantissa.Sign == 0) {
        return EInteger.Zero;
      }
      // DebugUtility.Log("RescaleByExponentDiff "+e1+" "+e2);
      FastInteger diff = FastInteger.FromBig(e1).SubtractBig(e2).Abs();
      EInteger eiBitCount = mantissa.GetUnsignedBitLengthAsEInteger();
      EInteger eidiffBigInt = diff.ToEInteger();
      // NOTE: For radix 10, each digit fits less than 1 byte; the
      // supported byte length is thus less than the maximum value
      // of a 32-bit integer (2GB).
      if (helper.GetRadix() <= 10) {
        int radix = helper.GetRadix();
        eiBitCount = eiBitCount.Add(eidiffBigInt.Multiply(BitsPerDigit[radix])
            .Divide(100));
        // DebugUtility.Log(""+eiBitCount);
        if (eiBitCount.CompareTo(Int32.MaxValue) > 0) {
          return null;
        }
      }
      return helper.MultiplyByRadixPower(mantissa, diff);
    }

    private static EContext SetPrecisionIfLimited(
      EContext ctx,
      EInteger bigPrecision) {
      return (ctx == null || !ctx.HasMaxPrecision) ? ctx :
        ctx.WithBigPrecision(bigPrecision);
    }

    private static void TransferFlags(
      EContext ctxDst,
      EContext ctxSrc) {
      if (ctxDst != null && ctxDst.HasFlags) {
        if ((ctxSrc.Flags & (EContext.FlagInvalid |
              EContext.FlagDivideByZero)) != 0) {
          ctxDst.Flags |= ctxSrc.Flags & (EContext.FlagInvalid |
              EContext.FlagDivideByZero);
        } else {
          ctxDst.Flags |= ctxSrc.Flags;
        }
      }
    }

    public T Abs(T value, EContext ctx) {
      int flags = this.helper.GetFlags(value);
      if ((flags & BigNumberFlags.FlagSignalingNaN) != 0) {
        return this.SignalingNaNInvalid(value, ctx);
      } else if ((flags & BigNumberFlags.FlagQuietNaN) != 0) {
        return this.ReturnQuietNaN(
            value,
            ctx);
      }
      T ret = ((flags & BigNumberFlags.FlagNegative) != 0) ?
        this.helper.CreateNewWithFlags(
          this.helper.GetMantissa(value),
          this.helper.GetExponent(value),
          flags & ~BigNumberFlags.FlagNegative) :
        value;
      return this.RoundToPrecision(ret, ctx);
    }

    private T AbsRaw(T value) {
      return this.EnsureSign(value, false);
    }

    // mant1 and mant2 are assumed to be nonnegative
    private T AddCore2(
      FastIntegerFixed mant1,
      FastIntegerFixed mant2,
      FastIntegerFixed exponent,
      int flags1,
      int flags2,
      EContext ctx) {
      #if DEBUG
      if (mant1.Sign < 0) {
        throw new InvalidOperationException();
      }
      if (mant2.Sign < 0) {
        throw new InvalidOperationException();
      }
      #endif
      bool neg1 = (flags1 & BigNumberFlags.FlagNegative) != 0;
      bool neg2 = (flags2 & BigNumberFlags.FlagNegative) != 0;
      var negResult = false;
      // DebugUtility.Log("neg1=" + neg1 + " neg2=" + neg2);
      if (neg1 != neg2) {
        // Signs are different, treat as a subtraction
        mant1 = FastIntegerFixed.Subtract(mant1, mant2);
        int mant1Sign = mant1.Sign;
        if (mant1Sign < 0) {
          negResult = !neg1;
          mant1 = mant1.Negate();
        } else if (mant1Sign == 0) {
          // Result is negative zero
          negResult = neg1 ^ neg2;
          if (negResult) {
            negResult &= (neg1 && neg2) || ((neg1 ^ neg2) && ctx != null &&
                ctx.Rounding == ERounding.Floor);
          }
        } else {
          negResult = neg1;
        }
      } else {
        // Signs are same, treat as an addition
        mant1 = FastIntegerFixed.Add(mant1, mant2);
        negResult = neg1;
        if (negResult && mant1.IsValueZero) {
          // Result is negative zero
          negResult &= (neg1 && neg2) || ((neg1 ^ neg2) && ctx != null &&
              ctx.Rounding == ERounding.Floor);
        }
      }
      // DebugUtility.Log("mant1= " + mant1 + " exp= " + exponent +" neg= "+
      // (negResult));
      return this.helper.CreateNewWithFlagsFastInt(
          mant1,
          exponent,
          negResult ? BigNumberFlags.FlagNegative : 0);
    }

    // mant1 and mant2 are assumed to be nonnegative
    private T AddCore(
      EInteger mant1,
      EInteger mant2,
      EInteger exponent,
      int flags1,
      int flags2,
      EContext ctx) {
      #if DEBUG
      if (mant1.Sign < 0) {
        throw new InvalidOperationException();
      }
      if (mant2.Sign < 0) {
        throw new InvalidOperationException();
      }
      #endif
      bool neg1 = (flags1 & BigNumberFlags.FlagNegative) != 0;
      bool neg2 = (flags2 & BigNumberFlags.FlagNegative) != 0;
      var negResult = false;
      // DebugUtility.Log("neg1=" + neg1 + " neg2=" + neg2);
      if (neg1 != neg2) {
        // Signs are different, treat as a subtraction
        // DebugUtility.Log("sub " + mant1 + " " + mant2);
        mant1 = mant1.Subtract(mant2);
        int mant1Sign = mant1.Sign;
        negResult = neg1 ^ (mant1Sign == 0 ? neg2 : (mant1Sign < 0));
        if (mant1Sign < 0) {
          mant1 = mant1.Negate();
        }
      } else {
        // Signs are same, treat as an addition
        // DebugUtility.Log("add " + mant1 + " " + mant2);
        mant1 = mant1.Add(mant2);
        negResult = neg1;
      }
      if (negResult && mant1.IsZero) {
        // Result is negative zero
        negResult &= (neg1 && neg2) || ((neg1 ^ neg2) && ctx != null &&
            ctx.Rounding == ERounding.Floor);
      }
      // DebugUtility.Log("mant1= " + mant1 + " exp= " + exponent +" neg= "+
      // (negResult));
      return this.helper.CreateNewWithFlags(
          mant1,
          exponent,
          negResult ? BigNumberFlags.FlagNegative : 0);
    }

    // mant1 and mant2 are assumed to be nonnegative
    private T AddCore(
      FastIntegerFixed fmant1,
      FastIntegerFixed fmant2,
      FastIntegerFixed exponent,
      int flags1,
      int flags2,
      EContext ctx) {
      #if DEBUG
      if (fmant1.Sign < 0) {
        throw new InvalidOperationException();
      }
      if (fmant2.Sign < 0) {
        throw new InvalidOperationException();
      }
      #endif
      bool neg1 = (flags1 & BigNumberFlags.FlagNegative) != 0;
      bool neg2 = (flags2 & BigNumberFlags.FlagNegative) != 0;
      var negResult = false;
      // DebugUtility.Log("neg1=" + neg1 + " neg2=" + neg2);
      if (neg1 != neg2) {
        // Signs are different, treat as a subtraction
        fmant1 = fmant1.Subtract(fmant2);
        int mant1Sign = fmant1.Sign;
        negResult = neg1 ^ (mant1Sign == 0 ? neg2 : (mant1Sign < 0));
        if (mant1Sign < 0) {
          fmant1 = fmant1.Negate();
        }
      } else {
        // Signs are same, treat as an addition
        fmant1 = fmant1.Add(fmant2);
        negResult = neg1;
      }
      if (negResult && fmant1.IsValueZero) {
        // Result is negative zero
        negResult &= (neg1 && neg2) || ((neg1 ^ neg2) && ctx != null &&
            ctx.Rounding == ERounding.Floor);
      }
      return this.helper.CreateNewWithFlagsFastInt(
          fmant1,
          exponent,
          negResult ? BigNumberFlags.FlagNegative : 0);
    }

    private static FastInteger ToFastInteger(FastIntegerFixed fif) {
      if (fif.CanFitInInt32()) {
        return new FastInteger(fif.ToInt32());
      } else {
        return FastInteger.FromBig(fif.ToEInteger());
      }
    }

    private T AddExDiffExp(
      FastIntegerFixed op1Exponent,
      FastIntegerFixed op1Mantissa,
      FastIntegerFixed op2Exponent,
      FastIntegerFixed op2Mantissa,
      int thisFlags,
      int otherFlags,
      EContext ctx,
      int expcmp,
      bool roundToOperandPrecision) {
      /* #if DEBUG (!(op1Mantissa.Sign >= 0)) {
        throw new ArgumentException("doesn't satisfy op1Mantissa.Sign >= 0");
      }
      if (!(op2Mantissa.Sign >= 0)) {
        throw new ArgumentException("doesn't satisfy op2Mantissa.Sign >= 0");
      }
      #endif

      */ T retval = default(T);
      // choose the minimum exponent
      FastIntegerFixed resultExponent = expcmp < 0 ?
        op1Exponent : op2Exponent;
      // DebugUtility.Log("[" + op1Mantissa + "," + op1Exponent + "], [" +
      // op2Mantissa + ", " + op2Exponent + "] -> " + resultExponent);
      if (ctx != null && ctx.HasMaxPrecision && ctx.Precision.Sign > 0) {
        FastIntegerFixed expdiff = op1Exponent.Subtract(op2Exponent).Abs();
        // Check if exponent difference is too big for
        // radix-power calculation to work quickly
        bool op2IsZero = op2Mantissa.IsValueZero;
        bool op1IsZero = op1Mantissa.IsValueZero;
        int thisSign = op1IsZero ? 0 : (((thisFlags &
                BigNumberFlags.FlagNegative) != 0) ? -1 : 1);
        int otherSign = op2IsZero ? 0 : (((otherFlags &
                BigNumberFlags.FlagNegative) != 0) ? -1 : 1);
        bool moreDistantThanPrecision = expdiff.CompareTo(ctx.Precision) > 0;
        // If exponent difference is greater than the precision
        if (moreDistantThanPrecision) {
          int expcmp2 = op1Exponent.CompareTo(op2Exponent);
          if (expcmp2 < 0) {
            if (!op2IsZero) {
              // first operand's exponent is less
              // and second operand isn't zero
              // second mantissa will be shifted by the exponent
              // difference
              // _________________________111111111111|_
              // ___222222222222222|____________________
              FastIntegerFixed digitLength1 =
                NumberUtility.DigitLengthBoundsFixed(this.helper,
                  op1Mantissa)[1];
              // DebugUtility.Log("dl1="+digitLength1);
              if (op1Exponent.Add(digitLength1).Add(2)
                .CompareTo(op2Exponent) < 0) {
                // first operand's mantissa can't reach the
                // second operand's mantissa, so the exponent can be
                // raised without affecting the result
                FastIntegerFixed tmp = op2Exponent.Subtract(4)
                  .Subtract(digitLength1).Subtract(ctx.Precision);
                // DebugUtility.Log("tmp="+tmp);
                FastIntegerFixed newDiff = tmp.Subtract(op2Exponent).Abs();
                // DebugUtility.Log("newdiff="+newDiff + " expdiff="+expdiff);
                if (newDiff.CompareTo(expdiff) < 0) {
                  // First operand can be treated as almost zero
                  bool sameSign = thisSign == otherSign;
                  FastIntegerFixed digitLength2 =
                    NumberUtility.DigitLengthFixed(this.helper, op2Mantissa);
                  // DebugUtility.Log("dl2="+digitLength2);
                  if (digitLength2.CompareTo(ctx.Precision) < 0) {
                    // Second operand's precision too short, extend
                    // it to the full precision
                    FastIntegerFixed precisionDiff =
                      FastIntegerFixed.FromBig(ctx.Precision)
                      .Subtract(digitLength2);
                    if (!op1IsZero && !sameSign) {
                      precisionDiff = precisionDiff.Add(2);
                    }
                    op2Mantissa = this.TryMultiplyByRadixPowerFastInt(
                        op2Mantissa,
                        precisionDiff);
                    if (op2Mantissa == null) {
                      return this.SignalInvalidWithMessage(
                          ctx,
                          "Result requires too much memory");
                    }
                    op2Exponent = op2Exponent.Subtract(precisionDiff);
                    if (!op1IsZero && !sameSign) {
                      op2Mantissa = op2Mantissa.Subtract(1);
                    }
                    int hoflags = otherFlags;
                    T other = this.helper.CreateNewWithFlagsFastInt(
                        op2Mantissa,
                        op2Exponent,
                        hoflags);
                    FastIntegerFixed shift = digitLength2
                      .Subtract(ctx.Precision);
                    if (op1IsZero && ctx != null && ctx.HasFlags) {
                      ctx.Flags |= EContext.FlagRounded;
                    }
                    // DebugUtility.Log("Second op's prec too short:
                    // op2Mantissa=" + op2Mantissa + " precdiff= " +
                    // (precisionDiff));
                    return this.RoundToPrecisionInternal(
                        other,
                        (op1IsZero || sameSign) ? 0 : 1,
                        (op1IsZero && !sameSign) ? 0 : 1,
                        ToFastInteger(shift),
                        false,
                        ctx);
                  } else if (!op1IsZero && !sameSign) {
                    op2Mantissa = this.TryMultiplyByRadixPowerFastInt(
                        op2Mantissa,
                        FastIntegerFixed.FromInt32(2));
                    if (op2Mantissa == null) {
                      return this.SignalInvalidWithMessage(
                          ctx,
                          "Result requires too much memory");
                    }
                    op2Exponent = op2Exponent.Subtract(2);
                    op2Mantissa = op2Mantissa.Subtract(1);
                    T other = this.helper.CreateNewWithFlagsFastInt(
                        op2Mantissa,
                        op2Exponent,
                        otherFlags);
                    FastIntegerFixed shift =
                      digitLength2.Subtract(ctx.Precision);
                    return this.RoundToPrecisionInternal(
                        other,
                        0,
                        0,
                        ToFastInteger(shift),
                        false,
                        ctx);
                  } else {
                    FastIntegerFixed shift2 =
                      digitLength2.Subtract(ctx.Precision);
                    if (!sameSign && ctx != null && ctx.HasFlags) {
                      ctx.Flags |= EContext.FlagRounded;
                    }
                    T other = this.helper.CreateNewWithFlagsFastInt(
                        op2Mantissa,
                        op2Exponent,
                        otherFlags);
                    return this.RoundToPrecisionInternal(
                        other,
                        0,
                        sameSign ? 1 : 0,
                        ToFastInteger(shift2),
                        false,
                        ctx);
                  }
                }
              }
            }
          } else if (expcmp2 > 0) {
            if (!op1IsZero) {
              // first operand's exponent is greater
              // and first operand isn't zero
              // first mantissa will be shifted by the exponent
              // difference
              // __111111111111|
              // ____________________222222222222222|
              FastIntegerFixed digitLength2 =
                NumberUtility.DigitLengthBoundsFixed(
                  this.helper,
                  op2Mantissa)[1];
              if (op2Exponent.Add(digitLength2).Add(2)
                .CompareTo(op1Exponent) < 0) {
                // second operand's mantissa can't reach the
                // first operand's mantissa, so the exponent can be
                // raised without affecting the result
                FastIntegerFixed tmp = op1Exponent.Subtract(4)
                  .Subtract(digitLength2).Subtract(ctx.Precision);
                FastIntegerFixed newDiff = tmp.Subtract(op1Exponent).Abs();
                if (newDiff.CompareTo(expdiff) < 0) {
                  // Second operand can be treated as almost zero
                  bool sameSign = thisSign == otherSign;
                  digitLength2 = NumberUtility.DigitLengthFixed(this.helper,
                      op1Mantissa);
                  if (digitLength2.CompareTo(ctx.Precision) < 0) {
                    // First operand's precision too short; extend it
                    // to the full precision
                    FastIntegerFixed precisionDiff =
                      FastIntegerFixed.FromBig(ctx.Precision)
                      .Subtract(digitLength2);
                    if (!op2IsZero && !sameSign) {
                      precisionDiff = precisionDiff.Add(2);
                    }
                    op1Mantissa = this.TryMultiplyByRadixPowerFastInt(
                        op1Mantissa,
                        precisionDiff);
                    if (op1Mantissa == null) {
                      return this.SignalInvalidWithMessage(
                          ctx,
                          "Result requires too much memory");
                    }
                    op1Exponent = op1Exponent.Subtract(precisionDiff);
                    if (!op2IsZero && !sameSign) {
                      op1Mantissa = op1Mantissa.Subtract(1);
                    }
                    T thisValue = this.helper.CreateNewWithFlagsFastInt(
                        op1Mantissa,
                        op1Exponent,
                        thisFlags);
                    FastIntegerFixed shift =
                      digitLength2.Subtract(ctx.Precision);
                    if (op2IsZero && ctx != null && ctx.HasFlags) {
                      ctx.Flags |= EContext.FlagRounded;
                    }
                    // DebugUtility.Log("thisValue D"+thisValue);
                    return this.RoundToPrecisionInternal(
                        thisValue,
                        (op2IsZero || sameSign) ? 0 : 1,
                        (op2IsZero && !sameSign) ? 0 : 1,
                        ToFastInteger(shift),
                        false,
                        ctx);
                  } else if (!op2IsZero && !sameSign) {
                    op1Mantissa = this.TryMultiplyByRadixPowerFastInt(
                        op1Mantissa,
                        FastIntegerFixed.FromInt32(2));
                    if (op1Mantissa == null) {
                      return this.SignalInvalidWithMessage(
                          ctx,
                          "Result requires too much memory");
                    }
                    op1Exponent = op1Exponent.Subtract(2);
                    op1Mantissa = op1Mantissa.Subtract(1);
                    T thisValue = this.helper.CreateNewWithFlagsFastInt(
                        op1Mantissa,
                        op1Exponent,
                        thisFlags);
                    FastIntegerFixed shift =
                      digitLength2.Subtract(ctx.Precision);
                    return this.RoundToPrecisionInternal(
                        thisValue,
                        0,
                        0,
                        ToFastInteger(shift),
                        false,
                        ctx);
                  } else {
                    FastIntegerFixed shift2 =
                      digitLength2.Subtract(ctx.Precision);
                    if (!sameSign && ctx != null && ctx.HasFlags) {
                      ctx.Flags |= EContext.FlagRounded;
                    }
                    T thisValue = this.helper.CreateNewWithFlagsFastInt(
                        op1Mantissa,
                        op1Exponent,
                        thisFlags);
                    return this.RoundToPrecisionInternal(
                        thisValue,
                        0,
                        sameSign ? 1 : 0,
                        ToFastInteger(shift2),
                        false,
                        ctx);
                  }
                }
              }
            }
          }
          expcmp = op1Exponent.CompareTo(op2Exponent);
          resultExponent = expcmp < 0 ? op1Exponent : op2Exponent;
        }
      }
      if (expcmp > 0) {
        // DebugUtility.Log("expcmp>0 op2m="+op2Mantissa+" exps="+
        // op1Exponent+"/"+op2Exponent);
        op1Mantissa = RescaleByExponentDiff(
            op1Mantissa,
            op1Exponent,
            op2Exponent,
            this.helper);
        if (op1Mantissa == null) {
          return this.SignalInvalidWithMessage(
              ctx,
              "Result requires too much memory");
        }
        retval = this.AddCore(
            op1Mantissa,
            op2Mantissa,
            resultExponent,
            thisFlags,
            otherFlags,
            ctx);
        // DebugUtility.Log("expcmp="+expcmp+" retval="+retval);
      } else {
        // DebugUtility.Log("expcmp<= 0 op2m="+op2Mantissa+" exps="+
        // op1Exponent+"/"+op2Exponent);
        op2Mantissa = RescaleByExponentDiff(
            op2Mantissa,
            op1Exponent,
            op2Exponent,
            this.helper);
        // DebugUtility.Log("op1m="+op1Mantissa+
        // " op2m="+op2Mantissa);
        if (op2Mantissa == null) {
          return this.SignalInvalidWithMessage(
              ctx,
              "Result requires too much memory");
        }
        retval = this.AddCore(
            op1Mantissa,
            op2Mantissa,
            resultExponent,
            thisFlags,
            otherFlags,
            ctx);
        // DebugUtility.Log("expcmp="+expcmp+" retval="+retval);
      }
      if (roundToOperandPrecision && ctx != null && ctx.HasMaxPrecision) {
        FastInteger digitLength1 = this.helper.GetDigitLength(
            op1Mantissa.ToEInteger());
        FastInteger digitLength2 =
          this.helper.GetDigitLength(op2Mantissa.ToEInteger());
        FastInteger maxDigitLength = (digitLength1.CompareTo(digitLength2) >
            0) ? digitLength1 :

          digitLength2;
        maxDigitLength.SubtractBig(ctx.Precision);
        // DebugUtility.Log("retval= " + retval + " maxdl=" +
        // maxDigitLength + " prec= " + (ctx.Precision));
        return (maxDigitLength.Sign > 0) ? this.RoundToPrecisionInternal(
            retval,
            0,
            0,
            maxDigitLength,
            false,
            ctx) : this.RoundToPrecision(retval, ctx);
        // DebugUtility.Log("retval now " + retval);
      } else {
        return IsNullOrSimpleContext(ctx) ? retval :
          this.RoundToPrecision(retval, ctx);
      }
    }

    private T CompareToHandleSpecial(
      T thisValue,
      T other,
      bool treatQuietNansAsSignaling,
      EContext ctx) {
      int thisFlags = this.helper.GetFlags(thisValue);
      int otherFlags = this.helper.GetFlags(other);
      if (((thisFlags | otherFlags) & BigNumberFlags.FlagSpecial) != 0) {
        // Check this value then the other value for signaling NaN
        if ((thisFlags & BigNumberFlags.FlagSignalingNaN) != 0) {
          return this.SignalingNaNInvalid(thisValue, ctx);
        }
        if ((otherFlags & BigNumberFlags.FlagSignalingNaN) != 0) {
          return this.SignalingNaNInvalid(other, ctx);
        }
        if (treatQuietNansAsSignaling) {
          if ((thisFlags & BigNumberFlags.FlagQuietNaN) != 0) {
            return this.SignalingNaNInvalid(thisValue, ctx);
          }
          if ((otherFlags & BigNumberFlags.FlagQuietNaN) != 0) {
            return this.SignalingNaNInvalid(other, ctx);
          }
        } else {
          // Check this value then the other value for quiet NaN
          if ((thisFlags & BigNumberFlags.FlagQuietNaN) != 0) {
            return this.ReturnQuietNaN(thisValue, ctx);
          }
          if ((otherFlags & BigNumberFlags.FlagQuietNaN) != 0) {
            return this.ReturnQuietNaN(other, ctx);
          }
        }
        if ((thisFlags & BigNumberFlags.FlagInfinity) != 0) {
          // thisValue is infinity
          return ((thisFlags & (BigNumberFlags.FlagInfinity |
                  BigNumberFlags.FlagNegative)) == (otherFlags &
                (BigNumberFlags.FlagInfinity |
                  BigNumberFlags.FlagNegative))) ? this.ValueOf(0, null) :
            (((thisFlags & BigNumberFlags.FlagNegative) == 0) ? this.ValueOf(
                1,
                null) : this.ValueOf(-1, null));
        }
        if ((otherFlags & BigNumberFlags.FlagInfinity) != 0) {
          // the other value is infinity
          return ((thisFlags & (BigNumberFlags.FlagInfinity |
                  BigNumberFlags.FlagNegative)) == (otherFlags &
                (BigNumberFlags.FlagInfinity |
                  BigNumberFlags.FlagNegative))) ? this.ValueOf(0, null) :
            (((otherFlags & BigNumberFlags.FlagNegative) == 0) ?
              this.ValueOf(-1, null) : this.ValueOf(1, null));
        }
      }
      return default(T);
    }

    private static int CompareToHandleSpecial2(
      int thisFlags,
      int otherFlags) {
      // Assumes either value is NaN and/or infinity
      {
        if ((thisFlags & BigNumberFlags.FlagNaN) != 0) {
          if ((otherFlags & BigNumberFlags.FlagNaN) != 0) {
            return 0;
          }
          // Consider NaN to be greater
          return 1;
        }
        if ((otherFlags & BigNumberFlags.FlagNaN) != 0) {
          // Consider this to be less than NaN
          return -1;
        }
        if ((thisFlags & BigNumberFlags.FlagInfinity) != 0) {
          // thisValue is infinity
          return ((thisFlags & (BigNumberFlags.FlagInfinity |
                  BigNumberFlags.FlagNegative)) == (otherFlags &
                (BigNumberFlags.FlagInfinity |
                  BigNumberFlags.FlagNegative))) ? 0 :
            (((thisFlags & BigNumberFlags.FlagNegative) == 0) ? 1 : -1);
        }
        if ((otherFlags & BigNumberFlags.FlagInfinity) != 0) {
          // the other value is infinity
          return ((thisFlags & (BigNumberFlags.FlagInfinity |
                  BigNumberFlags.FlagNegative)) == (otherFlags &
                (BigNumberFlags.FlagInfinity |
                  BigNumberFlags.FlagNegative))) ? 0 :
            (((otherFlags & BigNumberFlags.FlagNegative) == 0) ? -1 : 1);
        }
      }
      return 2;
    }

    private static int CompareToInternal<TMath>(
      TMath thisValue,
      TMath otherValue,
      bool reportOOM,
      IRadixMathHelper<TMath> helper) {
      int signA = helper.GetSign(thisValue);
      int signB = helper.GetSign(otherValue);
      if (signA != signB) {
        return (signA < signB) ? -1 : 1;
      }
      if (signB == 0 || signA == 0) {
        // Special case: Either operand is zero
        return 0;
      }
      FastIntegerFixed op1Exponent = helper.GetExponentFastInt(thisValue);
      FastIntegerFixed op2Exponent = helper.GetExponentFastInt(otherValue);
      FastIntegerFixed op1Mantissa = helper.GetMantissaFastInt(thisValue);
      FastIntegerFixed op2Mantissa = helper.GetMantissaFastInt(otherValue);
      int expcmp = op1Exponent.CompareTo(op2Exponent);
      // At this point, the signs are equal so we can compare
      // their absolute values instead
      int mantcmp = op1Mantissa.CompareTo(op2Mantissa);
      if (mantcmp == 0) {
        // Special case: Mantissas are equal
        return signA < 0 ? -expcmp : expcmp;
      }
      if (expcmp == 0) {
        return signA < 0 ? -mantcmp : mantcmp;
      }
      if (op1Exponent.CanFitInInt32() && op2Exponent.CanFitInInt32()) {
        if (op1Mantissa.CanFitInInt32() && op2Mantissa.CanFitInInt32()) {
          int e1int = op1Exponent.ToInt32();
          int e2int = op2Exponent.ToInt32();
          int c = CompareToFast(
              e1int,
              e2int,
              expcmp,
              signA,
              op1Mantissa,
              op2Mantissa,
              helper.GetRadix());
          if (c <= 1) {
            return c;
          }
        } else if (op1Mantissa.CanFitInInt64() && op2Mantissa.CanFitInInt64()) {
          int e1int = op1Exponent.ToInt32();
          int e2int = op2Exponent.ToInt32();
          int c = CompareToFast64(
              e1int,
              e2int,
              expcmp,
              signA,
              op1Mantissa,
              op2Mantissa,
              helper.GetRadix());
          if (c <= 1) {
            return c;
          }
        }
      }
      return CompareToSlow(
          op1Exponent.ToEInteger(),
          op2Exponent.ToEInteger(),
          expcmp,
          signA,
          op1Mantissa.ToEInteger(),
          op2Mantissa.ToEInteger(),
          helper,
          reportOOM);
    }

    private static string Chop(string str) {
      return (str.Length < 100) ? str : (str.Substring(0, 100) + "...");
    }

    private T DivideInternal(
      T thisValue,
      T divisor,
      EContext ctx,
      int integerMode,
      EInteger desiredExponent) {
      T ret = this.DivisionHandleSpecial(thisValue, divisor, ctx);
      if ((object)ret != (object)default(T)) {
        return ret;
      }
      int signA = this.helper.GetSign(thisValue);
      int signB = this.helper.GetSign(divisor);
      if (signB == 0) {
        if (signA == 0) {
          return this.SignalInvalid(ctx);
        }
        bool flagsNeg = ((this.helper.GetFlags(thisValue) &
              BigNumberFlags.FlagNegative) != 0) ^
          ((this.helper.GetFlags(divisor) &
              BigNumberFlags.FlagNegative) != 0);
        return this.SignalDivideByZero(ctx, flagsNeg);
      }
      int radix = this.thisRadix;
      if (signA == 0) {
        T retval = default(T);
        if (integerMode == IntegerModeFixedScale) {
          int newflags = (this.helper.GetFlags(thisValue) &
              BigNumberFlags.FlagNegative) ^ (this.helper.GetFlags(divisor) &
              BigNumberFlags.FlagNegative);
          retval = this.helper.CreateNewWithFlags(
              EInteger.Zero,
              desiredExponent,
              newflags);
        } else {
          EInteger dividendExp = this.helper.GetExponent(thisValue);
          EInteger divisorExp = this.helper.GetExponent(divisor);
          int newflags = (this.helper.GetFlags(thisValue) &
              BigNumberFlags.FlagNegative) ^ (this.helper.GetFlags(divisor) &
              BigNumberFlags.FlagNegative);
          retval = this.helper.CreateNewWithFlags(
              EInteger.Zero,
              dividendExp - (EInteger)divisorExp,
              newflags);
          retval = this.RoundToPrecision(retval, ctx);
        }
        return retval;
      } else {
        EInteger mantissaDividend = this.helper.GetMantissa(thisValue);
        EInteger mantissaDivisor = this.helper.GetMantissa(divisor);
        FastIntegerFixed expDividend =
          this.helper.GetExponentFastInt(thisValue);
        FastIntegerFixed expDivisor = this.helper.GetExponentFastInt(divisor);
        bool resultNeg = (this.helper.GetFlags(thisValue) &
            BigNumberFlags.FlagNegative) != (this.helper.GetFlags(divisor) &
            BigNumberFlags.FlagNegative);
        FastInteger expdiff = FastIntegerFixed.Subtract(expDividend,
            expDivisor).ToFastInteger();
        EInteger eintPrecision = (ctx == null || !ctx.HasMaxPrecision) ?
          EInteger.Zero : ctx.Precision;
        if (integerMode == IntegerModeFixedScale) {
          FastInteger shift;
          EInteger rem;
          FastInteger fastDesiredExponent =
            FastInteger.FromBig(desiredExponent);
          if (ctx != null && ctx.HasFlags &&
            fastDesiredExponent.CompareTo(expdiff) > 0) {
            // Treat as rounded if the desired exponent is greater
            // than the "ideal" exponent
            ctx.Flags |= EContext.FlagRounded;
          }
          if (expdiff.CompareTo(fastDesiredExponent) <= 0) {
            shift = fastDesiredExponent.Copy().Subtract(expdiff);
            EInteger quo;
            {
              EInteger[] divrem = mantissaDividend.DivRem(mantissaDivisor);
              quo = divrem[0];
              rem = divrem[1];
            }
            return this.RoundToScale(
                quo,
                rem,
                mantissaDivisor,
                desiredExponent,
                shift,
                resultNeg,
                ctx);
          }
          if (ctx != null && ctx.Precision.Sign != 0 &&
            expdiff.Copy().SubtractInt(8).CompareTo(eintPrecision) >
            0) {
            // NOTE: 8 guard digits
            // Result would require a too-high precision since
            // exponent difference is much higher
            return this.SignalInvalidWithMessage(
                ctx,
                "Result can't fit the precision");
          } else {
            shift = expdiff.Copy().Subtract(fastDesiredExponent);
            mantissaDividend =
              this.TryMultiplyByRadixPower(mantissaDividend, shift);
            if (mantissaDividend == null) {
              return this.SignalInvalidWithMessage(
                  ctx,
                  "Result requires too much memory");
            }
            EInteger quo;
            {
              EInteger[] divrem = mantissaDividend.DivRem(mantissaDivisor);
              quo = divrem[0];
              rem = divrem[1];
            }
            return this.RoundToScale(
                quo,
                rem,
                mantissaDivisor,
                desiredExponent,
                new FastInteger(0),
                resultNeg,
                ctx);
          }
        }
        if (integerMode == IntegerModeRegular) {
          EInteger rem = null;
          EInteger quo = null;
          FastInteger natexp = null;
          bool binaryOpt = this.thisRadix == 2 &&
            expDivisor.CompareToInt(0) == 0 && expDividend.CompareToInt(0) ==
            0 &&
            ctx != null && ctx.HasMaxPrecision && ctx.Precision.CompareTo(53)
            <= 0 &&
            mantissaDividend.CanFitInInt64() && mantissaDivisor.CanFitInInt64();
          if (binaryOpt) {
            EInteger absdivd = mantissaDividend.Abs();
            EInteger absdivs = mantissaDivisor.Abs();
            int maxprec = ctx.Precision.ToInt32Checked();
            var divdCount = (int)absdivd.GetUnsignedBitLengthAsInt64();
            var divsCount = (int)mantissaDivisor.GetUnsignedBitLengthAsInt64();
            int dividendShift = (divdCount <= divsCount) ? ((divsCount -
                  divdCount) + maxprec + 1) : Math.Max(0,
                (maxprec + 1) - (divdCount - divsCount));
            absdivd = absdivd.ShiftLeft(dividendShift);
            EInteger[] divrem3 = absdivd.DivRem(absdivs);
            quo = divrem3[0];
            rem = divrem3[1];
            if (ctx == EContext.Binary64 && quo.CanFitInInt64() &&
              rem.CanFitInInt64()) {
              long lquo = quo.ToInt64Checked();
              long lrem = rem.ToInt64Checked();
              int nexp = -dividendShift;
              if (lquo >= (1L << 53)) {
                while (lquo >= (1L << 54)) {
                  lrem |= lquo & 1L;
                  lquo >>= 1;
                  ++nexp;
                }
                if ((lquo & 3L) == 3 && lrem == 0) {
                  lquo >>= 1;
                  ++lquo;
                  ++nexp;
                } else if ((lquo & 1L) != 0 && lrem != 0) {
                  lquo >>= 1;
                  ++lquo;
                  ++nexp;
                } else {
                  lquo >>= 1;
                  ++nexp;
                }
                while (lquo >= (1L << 53)) {
                  lquo >>= 1;
                  ++nexp;
                }
                return this.helper.CreateNewWithFlags(
                    EInteger.FromInt64(lquo),
                    EInteger.FromInt64(nexp),
                    resultNeg ? BigNumberFlags.FlagNegative : 0);
              }
            }
            if (ctx == EContext.Binary32 && quo.CanFitInInt64() &&
              rem.CanFitInInt64()) {
              long lquo = quo.ToInt64Checked();
              long lrem = rem.ToInt64Checked();
              int nexp = -dividendShift;
              if (lquo >= (1L << 24)) {
                while (lquo >= (1L << 25)) {
                  lrem |= lquo & 1L;
                  lquo >>= 1;
                  ++nexp;
                }
                if ((lquo & 3L) == 3 && lrem == 0) {
                  lquo >>= 1;
                  ++lquo;
                  ++nexp;
                } else if ((lquo & 1L) != 0 && lrem != 0) {
                  lquo >>= 1;
                  ++lquo;
                  ++nexp;
                } else {
                  lquo >>= 1;
                  ++nexp;
                }
                while (lquo >= (1L << 24)) {
                  lquo >>= 1;
                  ++nexp;
                }
                return this.helper.CreateNewWithFlags(
                    EInteger.FromInt64(lquo),
                    EInteger.FromInt64(nexp),
                    resultNeg ? BigNumberFlags.FlagNegative : 0);
              }
            }
            natexp = new FastInteger(-dividendShift);
            // Console.WriteLine(quo.GetUnsignedBitLengthAsInt64()+" "+
            // rem.GetUnsignedBitLengthAsInt64()+" "+
            // (ctx == EContext.Binary64));
          }
          if (!binaryOpt) {
            EInteger[] divrem = mantissaDividend.DivRem(mantissaDivisor);
            quo = divrem[0];
            rem = divrem[1];
            if (rem.IsZero) {
              // Dividend is divisible by divisor
              // Console.WriteLine("divisible dividend: quo length=" +
              // quo.GetUnsignedBitLengthAsInt64());
              quo = quo.Abs();
              T fi = this.helper.CreateNewWithFlagsFastInt(
                  FastIntegerFixed.FromBig(quo),
                  FastIntegerFixed.FromFastInteger(expdiff),
                  resultNeg ? BigNumberFlags.FlagNegative : 0);
              return this.RoundToPrecision(fi, ctx);
            }
          }
          if (ctx != null && ctx.HasMaxPrecision) {
            if (!binaryOpt) {
              EInteger divid = mantissaDividend;
              FastInteger shift = FastInteger.FromBig(ctx.Precision);
              FastInteger[] dividBounds =
                NumberUtility.DigitLengthBounds(this.helper, mantissaDividend);
              FastInteger[] divisBounds =
                NumberUtility.DigitLengthBounds(this.helper, mantissaDivisor);
              if (dividBounds[0].Copy().Subtract(divisBounds[1])
                .CompareTo(shift) > 0) {
                // Dividend is already bigger than divisor by at least
                // shift digits, so no need to shift
                shift.SetInt(0);
              } else {
                FastInteger shiftCalc = divisBounds[0].Copy().Subtract(
                    dividBounds[1]).AddInt(2).Add(shift);
                if (shiftCalc.CompareToInt(0) <= 0) {
                  // No need to shift
                  shift.SetInt(0);
                } else {
                  shift = shiftCalc;
                  divid = this.TryMultiplyByRadixPower(divid, shift);
                  if (divid == null) {
                    return this.SignalInvalidWithMessage(
                        ctx,
                        "Result requires too much memory");
                  }
                }
              }
              if (shift.Sign != 0 || quo == null) {
                // if shift isn't zero, recalculate the quotient
                // and remainder
                EInteger[] divrem2 = divid.DivRem(mantissaDivisor);
                quo = divrem2[0];
                rem = divrem2[1];
              }
              natexp = expdiff.Copy().Subtract(shift);
            }
            // DebugUtility.Log(String.Format("" + divid + "" +
            // mantissaDivisor + " -> quo= " + quo + " rem= " +
            // (rem)));
            int[] digitStatus = this.RoundToScaleStatus(
                rem,
                mantissaDivisor,
                ctx);
            if (digitStatus == null) {
              // NOTE: Can only happen for ERounding.None
              return this.SignalInvalidWithMessage(
                  ctx,
                  "Rounding was required");
            }
            T retval2 = this.helper.CreateNewWithFlags(
                quo,
                natexp.ToEInteger(),
                resultNeg ? BigNumberFlags.FlagNegative : 0);
            if ((ctx == null || !ctx.HasFlagsOrTraps) &&
              (digitStatus[0] | digitStatus[1]) != 0) {
              // Context doesn't care about flags, and
              // we already know the result is inexact, so no
              // need to create a blank flag context to find that out
              return this.RoundToPrecisionInternal(
                  retval2,
                  digitStatus[0],
                  digitStatus[1],
                  null,
                  false,
                  ctx);
            }
            EContext ctxcopy = ctx.WithBlankFlags();
            retval2 = this.RoundToPrecisionInternal(
                retval2,
                digitStatus[0],
                digitStatus[1],
                null,
                false,
                ctxcopy);
            if ((ctxcopy.Flags & EContext.FlagInexact) != 0) {
              if (ctx.HasFlags) {
                ctx.Flags |= ctxcopy.Flags;
              }
              return retval2;
            }
            if (ctx.HasFlags) {
              ctx.Flags |= ctxcopy.Flags & ~EContext.FlagRounded;
            }
            retval2 = this.ReduceToPrecisionAndIdealExponent(
                retval2,
                ctx,
                rem.IsZero ? null : FastInteger.FromBig(eintPrecision),
                expdiff);
            return retval2;
          }
        }
        // Rest of method assumes unlimited precision
        // and IntegerModeRegular
        var adjust = new FastInteger(0);
        var result = new FastInteger(0);
        int mantcmp = mantissaDividend.CompareTo(mantissaDivisor);
        if (mantcmp == 0) {
          result = new FastInteger(1);
          mantissaDividend = EInteger.Zero;
        } else {
          EInteger gcd = mantissaDividend.Gcd(mantissaDivisor);
          // DebugUtility.Log("mgcd/den1=" + mantissaDividend + "/" + (//
          // mantissaDivisor) + "/" + gcd);
          if (gcd.CompareTo(EInteger.One) != 0) {
            mantissaDividend /= gcd;
            mantissaDivisor /= gcd;
          }
          // DebugUtility.Log("mgcd/den2=" + mantissaDividend + "/" + (//
          // mantissaDivisor) + "/" + gcd);
          FastInteger divShift = this.helper.DivisionShift(
              mantissaDividend,
              mantissaDivisor);

          if (divShift == null) {
            return this.SignalInvalidWithMessage(
                ctx,
                "Result would have a nonterminating expansion");
          }
          mantissaDividend = this.helper.MultiplyByRadixPower(
              mantissaDividend,
              divShift);
          adjust = divShift.Copy();
          // DebugUtility.Log("mant " + mantissaDividend + " " +
          // (// mantissaDivisor));
          EInteger[] quorem = mantissaDividend.DivRem(mantissaDivisor);
          #if DEBUG
          if (!quorem[1].IsZero) {
            throw new ArgumentException("doesn't satisfy quorem[1].IsZero");
          }
          #endif

          mantissaDividend = quorem[1];
          result = FastInteger.FromBig(quorem[0]);
        }
        // mantissaDividend now has the remainder
        FastInteger exp = expdiff.Copy().Subtract(adjust);
        ERounding rounding = (ctx == null) ? ERounding.HalfEven : ctx.Rounding;
        var lastDiscarded = 0;
        var olderDiscarded = 0;
        if (!mantissaDividend.IsZero) {
          if (rounding == ERounding.HalfDown || rounding ==
            ERounding.HalfEven || rounding == ERounding.HalfUp) {
            int cmpHalf = CompareToHalf(mantissaDividend, mantissaDivisor);
            if (cmpHalf == 0) {
              // remainder is exactly half
              lastDiscarded = radix / 2;
              olderDiscarded = 0;
            } else if (cmpHalf > 0) {
              // remainder is greater than half
              lastDiscarded = radix / 2;
              olderDiscarded = 1;
            } else {
              // remainder is less than half
              lastDiscarded = 0;
              olderDiscarded = 1;
            }
          } else {
            if (rounding == ERounding.None) {
              return this.SignalInvalidWithMessage(
                  ctx,
                  "Rounding was required");
            }
            lastDiscarded = 1;
            olderDiscarded = 1;
          }
        }
        EInteger bigResult = result.ToEInteger();
        if (ctx != null && ctx.HasFlags && exp.CompareTo(expdiff) > 0) {
          // Treat as rounded if the true exponent is greater
          // than the "ideal" exponent
          ctx.Flags |= EContext.FlagRounded;
        }
        EInteger bigexp = exp.ToEInteger();
        T retval = this.helper.CreateNewWithFlags(
            bigResult,
            bigexp,
            resultNeg ? BigNumberFlags.FlagNegative : 0);
        return this.RoundToPrecisionInternal(
            retval,
            lastDiscarded,
            olderDiscarded,
            null,
            false,
            ctx);
      }
    }

    private T DivisionHandleSpecial(
      T thisValue,
      T other,
      EContext ctx) {
      int thisFlags = this.helper.GetFlags(thisValue);
      int otherFlags = this.helper.GetFlags(other);
      if (((thisFlags | otherFlags) & BigNumberFlags.FlagSpecial) != 0) {
        T result = this.HandleNotANumber(thisValue, other, ctx);
        if ((object)result != (object)default(T)) {
          return result;
        }
        if ((thisFlags & BigNumberFlags.FlagInfinity) != 0 && (otherFlags &
            BigNumberFlags.FlagInfinity) != 0) {
          // Attempt to divide infinity by infinity
          return this.SignalInvalid(ctx);
        }
        if ((thisFlags & BigNumberFlags.FlagInfinity) != 0) {
          return this.EnsureSign(
              thisValue,
              ((thisFlags ^ otherFlags) & BigNumberFlags.FlagNegative) != 0);
        }
        if ((otherFlags & BigNumberFlags.FlagInfinity) != 0) {
          // Divisor is infinity, so result will be epsilon
          if (ctx != null && ctx.HasExponentRange && ctx.Precision.Sign > 0) {
            if (ctx.HasFlags) {
              ctx.Flags |= EContext.FlagClamped;
            }
            EInteger bigexp = ctx.EMin;
            EInteger bigprec = ctx.Precision;
            if (ctx.AdjustExponent) {
              bigexp -= (EInteger)bigprec;
              bigexp += EInteger.One;
            }
            thisFlags = (thisFlags ^ otherFlags) & BigNumberFlags.FlagNegative;
            return this.helper.CreateNewWithFlags(
                EInteger.Zero,
                bigexp,
                thisFlags);
          }
          thisFlags = (thisFlags ^ otherFlags) & BigNumberFlags.FlagNegative;
          result = this.helper.CreateNewWithFlags(
              EInteger.Zero,
              EInteger.Zero,
              thisFlags);
          return this.RoundToPrecision(
              result,
              ctx);
        }
      }
      return default(T);
    }

    private T EnsureSign(T val, bool negative) {
      if (val == null) {
        return val;
      }
      int flags = this.helper.GetFlags(val);
      if ((negative && (flags & BigNumberFlags.FlagNegative) == 0) ||
        (!negative && (flags & BigNumberFlags.FlagNegative) != 0)) {
        flags &= ~BigNumberFlags.FlagNegative;
        flags |= negative ? BigNumberFlags.FlagNegative : 0;
        return this.helper.CreateNewWithFlags(
            this.helper.GetMantissa(val),
            this.helper.GetExponent(val),
            flags);
      }
      return val;
    }

    private T ExpInternalVeryCloseToZero(
      T thisValue,
      EInteger workingPrecision,
      EContext ctx) {
      // NOTE: Assumes 'thisValue' is very close to zero
      // and either positive or negative.
      // DebugUtility.Log("ExpInternalVeryCloseToZero");
      T zero = this.helper.ValueOf(0);
      int cmpZero = this.CompareTo(thisValue, zero);
      if (cmpZero == 0) {
        // NOTE: Should not happen here, because
        // the check for zero should have happened earlier
        throw new InvalidOperationException();
      }
      T one = this.helper.ValueOf(1);
      int precisionAdd = this.thisRadix == 2 ? 18 : 12;
      EContext ctxdiv = SetPrecisionIfLimited(
          ctx,
          workingPrecision + (EInteger)precisionAdd)
        .WithRounding(ERounding.HalfEven);
      var bigintN = (EInteger)2;
      EInteger facto = EInteger.One;
      T guess;
      // Guess starts with thisValue
      guess = thisValue;
      // DebugUtility.Log("startguess="+guess);
      T lastGuess = guess;
      T pow = thisValue;
      var more = true;
      var lastCompare = 0;
      var vacillations = 0;
      int maxvac = cmpZero < 0 ? 10 : 3;
      while (true) {
        lastGuess = guess;
        // Iterate by:
        // newGuess = guess + (thisValue^n/factorial(n))
        // (n starts at 2 and increases by 1 after
        // each iteration)
        pow = this.Multiply(pow, thisValue, ctxdiv);
        facto *= (EInteger)bigintN;
        T tmp = this.Divide(
            pow,
            this.helper.CreateNewWithFlags(facto, EInteger.Zero, 0),
            ctxdiv);
        T newGuess = this.Add(guess, tmp, ctxdiv);
        // DebugUtility.Log("newguess " + newGuess);
        // DebugUtility.Log("newguessN " + NextPlus(newGuess,ctxdiv));
        {
          int guessCmp = this.CompareTo(lastGuess, newGuess);
          // DebugUtility.Log("guessCmp = " + guessCmp + ", vac=" + vacillations);
          if (guessCmp == 0) {
            more = false;
          } else if ((guessCmp > 0 && lastCompare < 0) || (lastCompare > 0 &&
              guessCmp < 0)) {
            // Guesses are vacillating
            ++vacillations;
            more &= vacillations <= maxvac ||
              (cmpZero < 0 ? guessCmp >= 0 : guessCmp <= 0);
          }
          lastCompare = guessCmp;
        }
        if (more) {
          bigintN += EInteger.One;
          guess = newGuess;
        } else {
          T ret = newGuess;
          // Add 1 at end
          ret = this.Add(one, ret, ctx);
          return ret;
        }
      }
    }

    private T ExpInternal(
      T thisValue,
      EInteger workingPrecision,
      EContext ctx) {
      // DebugUtility.Log("ExpInternal " +(thisValue as
      // EDecimal)?.ToDouble()+", wp=" +workingPrecision);
      T zero = this.helper.ValueOf(0);
      if (this.CompareTo(thisValue, zero) == 0) {
        // NOTE: Should not happen here, because
        // the check for zero should have happened earlier
        throw new InvalidOperationException();
      }
      T one = this.helper.ValueOf(1);
      int precisionAdd = this.thisRadix == 2 ? 18 : 12;
      EContext ctxdiv = SetPrecisionIfLimited(
          ctx,
          workingPrecision + (EInteger)precisionAdd)
        .WithRounding(ERounding.HalfEven);
      var bigintN = (EInteger)2;
      EInteger facto = EInteger.One;

      T guess;
      // Guess starts with thisValue
      // guess = thisValue;
      // Guess starts with 1 + thisValue
      guess = this.Add(one, thisValue, ctxdiv);
      // DebugUtility.Log(ctxdiv.ToString());
      // DebugUtility.Log("tv="+(thisValue as EDecimal)?.ToDouble());
      // DebugUtility.Log("initial="+thisValue);
      // DebugUtility.Log("startguess="+guess);
      T lastGuess = guess;
      T pow = thisValue;
      var more = true;
      var lastCompare = 0;
      var vacillations = 0;
      while (true) {
        lastGuess = guess;
        // Iterate by:
        // newGuess = guess + (thisValue^n/factorial(n))
        // (n starts at 2 and increases by 1 after
        // each iteration)
        pow = this.Multiply(pow, thisValue, ctxdiv);
        facto *= (EInteger)bigintN;
        T tmp = this.Divide(
            pow,
            this.helper.CreateNewWithFlags(facto, EInteger.Zero, 0),
            ctxdiv);
        T newGuess = this.Add(guess, tmp, ctxdiv);
        // DebugUtility.Log("newguess " +
        // this.helper.GetMantissa(newGuess));
        // DebugUtility.Log("newguessN " + NextPlus(newGuess,ctxdiv));
        {
          int guessCmp = this.CompareTo(lastGuess, newGuess);
          // DebugUtility.Log("guessCmp = " + guessCmp);
          if (guessCmp == 0) {
            more = false;
          } else if ((guessCmp > 0 && lastCompare < 0) || (lastCompare > 0 &&
              guessCmp < 0)) {
            // Guesses are vacillating
            ++vacillations;
            more &= vacillations <= 3 || guessCmp <= 0;
          }
          lastCompare = guessCmp;
        }
        if (more) {
          bigintN += EInteger.One;
          guess = newGuess;
        } else {
          T ret = this.Add(guess, tmp, ctx);
          // DebugUtility.Log("final... " + ret);
          return ret;
        }
      }
    }

    private T ExtendPrecision(T thisValue, EContext ctx) {
      if (ctx == null || !ctx.HasMaxPrecision) {
        return this.RoundToPrecision(thisValue, ctx);
      }
      EInteger mant = this.helper.GetMantissa(thisValue);
      FastInteger digits = this.helper.GetDigitLength(mant);
      FastInteger fastPrecision = FastInteger.FromBig(ctx.Precision);
      EInteger exponent = this.helper.GetExponent(thisValue);
      if (digits.CompareTo(fastPrecision) < 0) {
        fastPrecision.Subtract(digits);
        mant = this.TryMultiplyByRadixPower(mant, fastPrecision);
        if (mant == null) {
          return this.SignalInvalidWithMessage(
              ctx,
              "Result requires too much memory");
        }
        EInteger bigPrec = fastPrecision.ToEInteger();
        exponent -= (EInteger)bigPrec;
      }
      if (ctx != null && ctx.HasFlags) {
        ctx.Flags |= EContext.FlagRounded;
        ctx.Flags |= EContext.FlagInexact;
      }
      return this.RoundToPrecision(
          this.helper.CreateNewWithFlags(mant, exponent, 0),
          ctx);
    }

    private T HandleNotANumber(T thisValue, T other, EContext ctx) {
      int thisFlags = this.helper.GetFlags(thisValue);
      int otherFlags = this.helper.GetFlags(other);
      // Check this value then the other value for signaling NaN
      if ((thisFlags & BigNumberFlags.FlagSignalingNaN) != 0) {
        return this.SignalingNaNInvalid(thisValue, ctx);
      }
      if ((otherFlags & BigNumberFlags.FlagSignalingNaN) != 0) {
        return this.SignalingNaNInvalid(other, ctx);
      }
      // Check this value then the other value for quiet NaN
      return ((thisFlags & BigNumberFlags.FlagQuietNaN) != 0) ?
        this.ReturnQuietNaN(thisValue, ctx) : (((otherFlags &
              BigNumberFlags.FlagQuietNaN) != 0) ? this.ReturnQuietNaN(
                other,
                ctx) : default(T));
    }

    private bool IsFinite(T val) {
      return (this.helper.GetFlags(val) & BigNumberFlags.FlagSpecial) == 0;
    }

    private bool IsNegative(T val) {
      return (this.helper.GetFlags(val) & BigNumberFlags.FlagNegative) != 0;
    }

    private bool IsWithinExponentRangeForPow(
      T thisValue,
      EContext ctx) {
      if (ctx == null || !ctx.HasExponentRange) {
        return true;
      }
      FastInteger digits = this.helper.GetDigitLength(
          this.helper.GetMantissa(thisValue));
      EInteger exp = this.helper.GetExponent(thisValue);
      FastInteger fi = FastInteger.FromBig(exp);
      if (ctx.AdjustExponent) {
        fi.Add(digits);
        fi.Decrement();
      }
      // DebugUtility.Log("" + exp + " -> " + fi);
      if (fi.Sign < 0) {
        fi.Negate().Divide(2).Negate();
        // DebugUtility.Log("" + exp + " II -> " + fi);
      }
      exp = fi.ToEInteger();
      return exp.CompareTo(ctx.EMin) >= 0 && exp.CompareTo(ctx.EMax) <= 0;
    }

    private T LnInternalCloseToOne2(
      T thisValue,
      EInteger workingPrecision,
      EContext ctx) {
      // Assumes 'thisValue' is close to 1
      var more = true;
      var lastCompare = 0;
      var vacillations = 0;
      string dbg = String.Empty;
      // if (thisValue is EDecimal) {
      // dbg="" + (thisValue as EDecimal)?.ToDouble();
      // }
      // DebugUtility.Log("workingprec=" + workingPrecision);
      EContext ctxdiv = SetPrecisionIfLimited(
          ctx,
          workingPrecision + (EInteger)6).WithRounding(ERounding.HalfEven);
      T rzlo = this.Add(thisValue, this.helper.ValueOf(-1), null);
      T rzhi = this.Add(thisValue, this.helper.ValueOf(1), null);
      // (thisValue - 1) / (thisValue + 1)
      T rz = this.Divide(rzlo, rzhi, ctxdiv);
      // (thisValue - 1) * 2 / (thisValue + 1)
      T guess = this.Add(rz, rz, null);
      T rzterm = rz;
      T lastGuess = default(T);
      T lastDiff = default(T);
      var haveLastDiff = false;
      EInteger denom = EInteger.FromInt32(3);
      EInteger iterations = EInteger.Zero;
      while (more) {
        lastGuess = guess;
        rzterm = this.Multiply(rzterm, rz, ctxdiv);
        rzterm = this.Multiply(rzterm, rz, ctxdiv);
        T rd = this.Divide(
            this.Multiply(rzterm, this.helper.ValueOf(2), ctxdiv),
            this.helper.CreateNewWithFlags(denom, EInteger.Zero, 0),
            ctxdiv);
        if (haveLastDiff && this.CompareTo(lastDiff, rd) == 0) {
          // iterate is the same as before, so break
          break;
        }
        T newGuess = this.Add(guess, rd, ctxdiv);
        int guessCmp = this.CompareTo(lastGuess, newGuess);
        bool overprec = iterations.CompareTo(workingPrecision) >= 0;
        if (guessCmp == 0) {
          more = false;
        } else if ((guessCmp > 0 && lastCompare < 0) || (lastCompare > 0 &&
            guessCmp < 0)) {
          // Guesses are vacillating
          ++vacillations;
          more &= !overprec || vacillations <= 3 || guessCmp <= 0;
        }
        lastCompare = guessCmp;
        lastDiff = this.AbsRaw(rd);
        haveLastDiff = true;
        if (more) {
          denom = denom.Add(2);
          iterations += EInteger.One;
          guess = newGuess;
        } else {
          guess = this.Add(guess, rd, ctx);
        }
      }
      // DebugUtility.Log("iterations="+iterations+", "+dbg);
      return guess;
    }

    private T LnInternal(
      T thisValue,
      EInteger workingPrecision,
      EContext ctx) {
      var more = true;
      var lastCompare = 0;
      var vacillations = 0;
      EContext ctxdiv = SetPrecisionIfLimited(
          ctx,
          workingPrecision + (EInteger)6)
        .WithRounding(ERounding.HalfEven);
      T z = this.Add(
          this.NegateRaw(thisValue),
          this.helper.ValueOf(1),
          null);
      T zpow = this.Multiply(z, z, ctxdiv);
      T guess = this.NegateRaw(z);
      T lastGuess = default(T);
      var denom = (EInteger)2;
      while (more) {
        lastGuess = guess;
        T tmp = this.Divide(
            zpow,
            this.helper.CreateNewWithFlags(denom, EInteger.Zero, 0),
            ctxdiv);
        T newGuess = this.Add(guess, this.NegateRaw(tmp), ctxdiv);
        {
          int guessCmp = this.CompareTo(lastGuess, newGuess);
          if (guessCmp == 0) {
            more = false;
          } else if ((guessCmp > 0 && lastCompare < 0) || (lastCompare > 0 &&
              guessCmp < 0)) {
            // Guesses are vacillating
            ++vacillations;
            more &= vacillations <= 3 || guessCmp <= 0;
          }
          lastCompare = guessCmp;
        }
        guess = newGuess;
        if (more) {
          zpow = this.Multiply(zpow, z, ctxdiv);
          denom += EInteger.One;
        }
      }
      return this.RoundToPrecision(guess, ctx);
    }

    private T MinMaxHandleSpecial(
      T thisValue,
      T otherValue,
      EContext ctx,
      bool isMinOp,
      bool compareAbs) {
      int thisFlags = this.helper.GetFlags(thisValue);
      int otherFlags = this.helper.GetFlags(otherValue);
      if (((thisFlags | otherFlags) & BigNumberFlags.FlagSpecial) != 0) {
        // Check this value then the other value for signaling NaN
        if ((thisFlags & BigNumberFlags.FlagSignalingNaN) != 0) {
          return this.SignalingNaNInvalid(thisValue, ctx);
        }
        if ((otherFlags & BigNumberFlags.FlagSignalingNaN) != 0) {
          return this.SignalingNaNInvalid(otherValue, ctx);
        }
        // Check this value then the other value for quiet NaN
        if ((thisFlags & BigNumberFlags.FlagQuietNaN) != 0) {
          if ((otherFlags & BigNumberFlags.FlagQuietNaN) != 0) {
            // both values are quiet NaN
            return this.ReturnQuietNaN(thisValue, ctx);
          }
          // return "other" for being numeric
          return this.RoundToPrecision(otherValue, ctx);
        }
        if ((otherFlags & BigNumberFlags.FlagQuietNaN) != 0) {
          // At this point, "thisValue" can't be NaN,
          // return "thisValue" for being numeric
          return this.RoundToPrecision(thisValue, ctx);
        }
        if ((thisFlags & BigNumberFlags.FlagInfinity) != 0) {
          if (compareAbs && (otherFlags & BigNumberFlags.FlagInfinity) == 0) {
            // treat this as larger
            return isMinOp ? this.RoundToPrecision(otherValue, ctx) :
              thisValue;
          }
          // This value is infinity
          if (isMinOp) {
            // if negative, will be less than every other number
            return ((thisFlags & BigNumberFlags.FlagNegative) != 0) ?
              thisValue : this.RoundToPrecision(otherValue, ctx);
            // if positive, will be greater
          }
          // if positive, will be greater than every other number
          return ((thisFlags & BigNumberFlags.FlagNegative) == 0) ?
            thisValue : this.RoundToPrecision(otherValue, ctx);
        }
        if ((otherFlags & BigNumberFlags.FlagInfinity) != 0) {
          if (compareAbs) {
            // treat this as larger (the first value
            // won't be infinity at this point
            return isMinOp ? this.RoundToPrecision(thisValue, ctx) :
              otherValue;
          }
          return isMinOp ? (((otherFlags & BigNumberFlags.FlagNegative) ==
                0) ? this.RoundToPrecision(thisValue, ctx) :
              otherValue) : (((otherFlags & BigNumberFlags.FlagNegative) !=
                0) ? this.RoundToPrecision(thisValue, ctx) : otherValue);
        }
      }
      return default(T);
    }

    private T MultiplyAddHandleSpecial(
      T op1,
      T op2,
      T op3,
      EContext ctx) {
      int op1Flags = this.helper.GetFlags(op1);
      // Check operands in order for signaling NaN
      if ((op1Flags & BigNumberFlags.FlagSignalingNaN) != 0) {
        return this.SignalingNaNInvalid(op1, ctx);
      }
      int op2Flags = this.helper.GetFlags(op2);
      if ((op2Flags & BigNumberFlags.FlagSignalingNaN) != 0) {
        return this.SignalingNaNInvalid(op2, ctx);
      }
      int op3Flags = this.helper.GetFlags(op3);
      // Check operands in order for quiet NaN
      if ((op1Flags & BigNumberFlags.FlagQuietNaN) != 0) {
        if ((op3Flags & BigNumberFlags.FlagSignalingNaN) != 0) {
          return this.SignalingNaNInvalid(op3, ctx);
        } else {
          return this.ReturnQuietNaN(op1, ctx);
        }
      }
      if ((op2Flags & BigNumberFlags.FlagQuietNaN) != 0) {
        if ((op3Flags & BigNumberFlags.FlagSignalingNaN) != 0) {
          return this.SignalingNaNInvalid(op3, ctx);
        } else {
          return this.ReturnQuietNaN(op2, ctx);
        }
      }
      // Check multiplying infinity by 0 (important to check
      // now before checking third operand for quiet NaN because
      // this signals invalid operation and the operation starts
      // with multiplying only the first two operands)
      if ((op1Flags & BigNumberFlags.FlagInfinity) != 0) {
        // Attempt to multiply infinity by 0
        if ((op2Flags & BigNumberFlags.FlagSpecial) == 0 &&
          this.helper.GetMantissa(op2).IsZero) {
          return this.SignalInvalid(ctx);
        }
      }
      if ((op2Flags & BigNumberFlags.FlagInfinity) != 0) {
        // Attempt to multiply infinity by 0
        if ((op1Flags & BigNumberFlags.FlagSpecial) == 0 &&
          this.helper.GetMantissa(op1).IsZero) {
          return this.SignalInvalid(ctx);
        }
      }
      // Now check third operand for signaling NaN
      if ((op3Flags & BigNumberFlags.FlagSignalingNaN) != 0) {
        return this.SignalingNaNInvalid(op3, ctx);
      }
      // Now check third operand for quiet NaN
      return ((op3Flags & BigNumberFlags.FlagQuietNaN) != 0) ?
        this.ReturnQuietNaN(op3, ctx) : default(T);
    }

    private T NegateRaw(T val) {
      if (val == null) {
        return val;
      }
      int sign = this.helper.GetFlags(val) & BigNumberFlags.FlagNegative;
      return this.helper.CreateNewWithFlags(
          this.helper.GetMantissa(val),
          this.helper.GetExponent(val),
          sign == 0 ? BigNumberFlags.FlagNegative : 0);
    }

    private T PowerIntegral(
      T thisValue,
      EInteger powIntBig,
      EContext ctx) {
      int sign = powIntBig.Sign;
      T one = this.helper.ValueOf(1);
      if (sign == 0) {
        // however 0 to the power of 0 is undefined
        return this.RoundToPrecision(one, ctx);
      }
      if (powIntBig.Equals(EInteger.One)) {
        return this.RoundToPrecision(thisValue, ctx);
      }
      if (powIntBig.CompareTo(2) == 0) {
        return this.Multiply(thisValue, thisValue, ctx);
      }
      if (powIntBig.CompareTo(3) == 0) {
        return this.Multiply(
            thisValue,
            this.Multiply(thisValue, thisValue, null),
            ctx);
      }
      bool retvalNeg = this.IsNegative(thisValue) && !powIntBig.IsEven;
      FastInteger error = this.helper.GetDigitLength(powIntBig.Abs());
      error = error.Copy();
      error.AddInt(18);
      EInteger bigError = error.ToEInteger();
      /*DUL("thisValue=" + thisValue +
      " powInt=" + powIntBig);*/
      EContext ctxdiv = ctx == null ? ctx : SetPrecisionIfLimited(
          ctx,
          ctx.Precision + (EInteger)bigError)
        .WithRounding(ERounding.HalfEven).WithBlankFlags();
      if (sign < 0) {
        // Use the reciprocal for negative powers
        thisValue = this.Divide(one, thisValue, ctxdiv);
        // DebugUtility.Log("-->recip thisValue=" + thisValue +
        // " powInt=" + powIntBig + " flags=" + (ctxdiv == null ? -1 :
        // ctxdiv.Flags));
        if ((ctxdiv.Flags & EContext.FlagOverflow) != 0) {
          return this.SignalOverflow(ctx, retvalNeg);
        }
        powIntBig = -powIntBig;
      }
      T r = one;
      while (!powIntBig.IsZero) {
        if (!powIntBig.IsEven) {
          // DebugUtility.Log("-->RT thisValue=" + thisValue +
          // " powInt=" + powIntBig + " flags=" +
          // (ctxdiv == null ? -1 : ctxdiv.Flags));
          r = this.Multiply(r, thisValue, ctxdiv);
          if (ctxdiv != null && (ctxdiv.Flags & EContext.FlagOverflow) != 0) {
            return this.SignalOverflow(ctx, retvalNeg);
          }
        }
        powIntBig >>= 1;
        if (!powIntBig.IsZero) {
          if (ctxdiv != null) {
            ctxdiv.Flags &= ~EContext.FlagOverflow;
          }
          // DebugUtility.Log("-->TT thisValue=" + thisValue +
          // " powInt=" + powIntBig + " flags=" +
          // (ctxdiv == null ? -1 : ctxdiv.Flags));
          T tmp = this.Multiply(thisValue, thisValue, ctxdiv);
          if (ctxdiv != null && (ctxdiv.Flags & EContext.FlagOverflow) != 0) {
            // Avoid multiplying too huge numbers with
            // limited exponent range
            return this.SignalOverflow(ctx, retvalNeg);
          }
          thisValue = tmp;
        }
        // DebugUtility.Log("r="+r);
      }
      if (ctx != null && ctx.HasFlags) {
        ctx.Flags |= ctxdiv.Flags & (
            EContext.FlagUnderflow |
            EContext.FlagSubnormal | EContext.FlagInexact |
            EContext.FlagRounded | EContext.FlagClamped);
      }
      return this.RoundToPrecision(r, ctx);
    }

    private T ReduceToPrecisionAndIdealExponent(
      T thisValue,
      EContext ctx,
      FastInteger precision,
      FastInteger idealExp) {
      T ret = this.RoundToPrecision(thisValue, ctx);
      if (ret != null && (this.helper.GetFlags(ret) &
          BigNumberFlags.FlagSpecial) == 0) {
        EInteger bigmant = this.helper.GetMantissa(ret);
        FastInteger exp = FastInteger.FromBig(this.helper.GetExponent(ret));
        int radix = this.thisRadix;
        if (bigmant.IsZero) {
          exp = new FastInteger(0);
        } else {
          FastInteger digits = (precision == null) ? null :
            this.helper.GetDigitLength(bigmant);
          bigmant = NumberUtility.ReduceTrailingZeros(
              bigmant,
              exp,
              radix,
              digits,
              precision,
              idealExp);
        }
        int flags = this.helper.GetFlags(thisValue);
        ret = this.helper.CreateNewWithFlags(
            bigmant,
            exp.ToEInteger(),
            flags);
        if (ctx != null && ctx.ClampNormalExponents) {
          EContext ctxtmp = ctx.WithBlankFlags();
          ret = this.RoundToPrecision(ret, ctxtmp);
          if (ctx.HasFlags) {
            ctx.Flags |= ctxtmp.Flags & ~EContext.FlagClamped;
          }
        }
        ret = this.EnsureSign(ret, (flags & BigNumberFlags.FlagNegative) != 0);
      }
      return ret;
    }

    private T RemainderHandleSpecial(
      T thisValue,
      T other,
      EContext ctx) {
      int thisFlags = this.helper.GetFlags(thisValue);
      int otherFlags = this.helper.GetFlags(other);
      if (((thisFlags | otherFlags) & BigNumberFlags.FlagSpecial) != 0) {
        T result = this.HandleNotANumber(thisValue, other, ctx);
        if ((object)result != (object)default(T)) {
          return result;
        }
        if ((thisFlags & BigNumberFlags.FlagInfinity) != 0) {
          return this.SignalInvalid(ctx);
        }
        if ((otherFlags & BigNumberFlags.FlagInfinity) != 0) {
          return this.RoundToPrecision(thisValue, ctx);
        }
      }
      return this.helper.GetMantissa(other).IsZero ? this.SignalInvalid(ctx) :
        default(T);
    }

    private T ReturnQuietNaN(T thisValue, EContext ctx) {
      EInteger mant = this.helper.GetMantissa(thisValue);
      var mantChanged = false;
      if (!mant.IsZero && ctx != null && ctx.HasMaxPrecision) {
        FastInteger compPrecision = FastInteger.FromBig(ctx.Precision);
        if (this.helper.GetDigitLength(mant).CompareTo(compPrecision) >= 0) {
          // Mant's precision is higher than the maximum precision
          EInteger limit = this.TryMultiplyByRadixPower(
              EInteger.One,
              compPrecision);
          if (limit == null) {
            // Limit can't be allocated
            return this.SignalInvalidWithMessage(
                ctx,
                "Result requires too much memory");
          }
          if (mant.CompareTo(limit) >= 0) {
            mant %= (EInteger)limit;
            mantChanged = true;
          }
        }
      }
      int flags = this.helper.GetFlags(thisValue);
      if (!mantChanged && (flags & BigNumberFlags.FlagQuietNaN) != 0) {
        return thisValue;
      }
      flags &= BigNumberFlags.FlagNegative;
      flags |= BigNumberFlags.FlagQuietNaN;
      return this.helper.CreateNewWithFlags(mant, EInteger.Zero, flags);
    }

    private bool IsNullOrInt32FriendlyContext(EContext ctx) {
      return ctx == null || (
          (!ctx.HasFlags && ctx.Traps == 0) &&
          (!ctx.HasExponentRange ||
            (ctx.EMin.CompareTo(-10) < 0 && ctx.EMax.Sign >= 0)) &&
          ctx.Rounding != ERounding.Floor && (!ctx.HasMaxPrecision ||
            (this.thisRadix >= 10 && !ctx.IsPrecisionInBits &&
              ctx.Precision.CompareTo(10) >= 0) ||
            ((this.thisRadix >= 2 || ctx.IsPrecisionInBits) &&
              ctx.Precision.CompareTo(32) >= 0)));
    }

    private bool RoundGivenAccum(
      IShiftAccumulator accum,
      ERounding rounding,
      bool neg) {
      var incremented = false;
      int radix = this.thisRadix;
      int lastDiscarded = accum.LastDiscardedDigit;
      int olderDiscarded = accum.OlderDiscardedDigits;
      // NOTE: HalfUp, HalfEven, and HalfDown care about
      // the identity of the last discarded digit
      if (rounding == ERounding.HalfUp) {
        incremented |= lastDiscarded >= (radix / 2);
      } else if (rounding == ERounding.HalfEven) {
        if (lastDiscarded >= (radix / 2)) {
          if (lastDiscarded > (radix / 2) || olderDiscarded != 0) {
            incremented = true;
          } else {
            incremented |= accum.ShiftedIntMod(2) == 1;
          }
        }
      } else if (rounding == ERounding.HalfDown) {
        incremented |= lastDiscarded > (radix / 2) || (lastDiscarded ==
            (radix / 2) && olderDiscarded != 0);
      } else if (rounding == ERounding.Ceiling) {
        incremented |= !neg && (lastDiscarded | olderDiscarded) != 0;
      } else if (rounding == ERounding.Floor) {
        incremented |= neg && (lastDiscarded | olderDiscarded) != 0;
      } else if (rounding == ERounding.Up) {
        incremented |= (lastDiscarded | olderDiscarded) != 0;
      } else if (rounding == ERounding.Odd ||
        (rounding == ERounding.OddOrZeroFiveUp && radix == 2)) {
        incremented |= (lastDiscarded | olderDiscarded) != 0 &&
          (accum.ShiftedIntMod(2) == 0);
      } else if (rounding == ERounding.ZeroFiveUp ||
        (rounding == ERounding.OddOrZeroFiveUp && radix != 2)) {
        if ((lastDiscarded | olderDiscarded) != 0) {
          if (radix == 2) {
            incremented = true;
          } else {
            int lastDigit = accum.ShiftedIntMod(radix);
            if (lastDigit == 0 || lastDigit == (radix / 2)) {
              incremented = true;
            }
          }
        }
      }
      return incremented;
    }

    private bool RoundGivenDigits(
      int lastDiscarded,
      int olderDiscarded,
      ERounding rounding,
      bool neg,
      FastInteger fastNumber) {
      var incremented = false;
      int radix = this.thisRadix;
      // NOTE: HalfUp, HalfEven, and HalfDown care about
      // the identity of the last discarded digit
      if (rounding == ERounding.HalfUp) {
        incremented |= lastDiscarded >= (radix / 2);
      } else if (rounding == ERounding.HalfEven) {
        if (lastDiscarded >= (radix / 2)) {
          if (lastDiscarded > (radix / 2) || olderDiscarded != 0) {
            incremented = true;
          } else {
            incremented |= !fastNumber.IsEvenNumber;
          }
        }
      } else if (rounding == ERounding.HalfDown) {
        incremented |= lastDiscarded > (radix / 2) || (lastDiscarded ==
            (radix / 2) && olderDiscarded != 0);
      } else if (rounding == ERounding.Ceiling) {
        incremented |= !neg && (lastDiscarded | olderDiscarded) != 0;
      } else if (rounding == ERounding.Floor) {
        incremented |= neg && (lastDiscarded | olderDiscarded) != 0;
      } else if (rounding == ERounding.Up) {
        incremented |= (lastDiscarded | olderDiscarded) != 0;
      } else if (rounding == ERounding.Odd ||
        (rounding == ERounding.OddOrZeroFiveUp && radix == 2)) {
        incremented |= (lastDiscarded | olderDiscarded) != 0 &&
          fastNumber.IsEvenNumber;
      } else if (rounding == ERounding.ZeroFiveUp ||
        (rounding == ERounding.OddOrZeroFiveUp && radix != 2)) {
        if ((lastDiscarded | olderDiscarded) != 0) {
          if (radix == 2) {
            incremented = true;
          } else {
            int lastDigit = FastIntegerFixed.FromFastInteger(
                fastNumber).Mod(radix);
            if (lastDigit == 0 || lastDigit == (radix / 2)) {
              incremented = true;
            }
          }
        }
      }
      return incremented;
    }

    private bool RoundGivenDigits(
      int lastDiscarded,
      int olderDiscarded,
      ERounding rounding,
      bool neg,
      long longNumber) {
      var incremented = false;
      int radix = this.thisRadix;
      // NOTE: HalfUp, HalfEven, and HalfDown care about
      // the identity of the last discarded digit
      if (rounding == ERounding.HalfUp) {
        incremented |= lastDiscarded >= (radix / 2);
      } else if (rounding == ERounding.HalfEven) {
        if (lastDiscarded >= (radix / 2)) {
          if (lastDiscarded > (radix / 2) || olderDiscarded != 0) {
            incremented = true;
          } else {
            incremented |= (longNumber & 1) != 0;
          }
        }
      } else if (rounding == ERounding.HalfDown) {
        incremented |= lastDiscarded > (radix / 2) || (lastDiscarded ==
            (radix / 2) && olderDiscarded != 0);
      } else if (rounding == ERounding.Ceiling) {
        incremented |= !neg && (lastDiscarded | olderDiscarded) != 0;
      } else if (rounding == ERounding.Floor) {
        incremented |= neg && (lastDiscarded | olderDiscarded) != 0;
      } else if (rounding == ERounding.Up) {
        incremented |= (lastDiscarded | olderDiscarded) != 0;
      } else if (rounding == ERounding.Odd ||
        (rounding == ERounding.OddOrZeroFiveUp && radix == 2)) {
        incremented |= (lastDiscarded | olderDiscarded) != 0 &&
          ((longNumber & 1) == 0);
      } else if (rounding == ERounding.ZeroFiveUp ||
        (rounding == ERounding.OddOrZeroFiveUp && radix != 2)) {
        if ((lastDiscarded | olderDiscarded) != 0) {
          if (radix == 2) {
            incremented = true;
          } else {
            var lastDigit = (int)(longNumber % radix);
            if (lastDigit == 0 || lastDigit == (radix / 2)) {
              incremented = true;
            }
          }
        }
      }
      return incremented;
    }

    private static readonly EContext DefaultUnlimited =
      EContext.UnlimitedHalfEven.WithRounding(ERounding.HalfEven);

    // private static int compareFast = 0;
    // private static int compareSlow = 0;
    // private static int compareNone = 0;
    private T RoundToPrecisionInternal(
      T thisValue,
      int lastDiscarded,
      int olderDiscarded,
      FastInteger shift,
      bool adjustNegativeZero,
      EContext ctx) {
      bool mantissaWasZero;
      bool nonHalfRounding;
      var finalizing = false;
      int flags;
      bool unlimitedPrecisionExp = ctx == null ||
        (!ctx.HasMaxPrecision && !ctx.HasExponentRange);
      int thisFlags = this.helper.GetFlags(thisValue);
      if ((thisFlags & BigNumberFlags.FlagSpecial) != 0) {
        if ((thisFlags & BigNumberFlags.FlagSignalingNaN) != 0) {
          if (ctx != null && ctx.HasFlags) {
            ctx.Flags |= EContext.FlagInvalid;
          }
          return this.ReturnQuietNaN(thisValue, ctx);
        }
        if ((thisFlags & BigNumberFlags.FlagQuietNaN) != 0) {
          return this.ReturnQuietNaN(thisValue, ctx);
        }
        if ((thisFlags & BigNumberFlags.FlagInfinity) != 0) {
          return thisValue;
        }
      }
      // If context has unlimited precision and exponent range,
      // and no discarded digits or shifting
      if (unlimitedPrecisionExp && (lastDiscarded | olderDiscarded) == 0 &&
        (shift == null || shift.IsValueZero)) {
        if (!(adjustNegativeZero &&
            (thisFlags & BigNumberFlags.FlagNegative) != 0 &&
            this.helper.GetMantissa(thisValue).IsZero)) {
          return thisValue;
        }
      }
      // If context has unlimited precision and exponent range,
      // and no flags, traps, or shifting
      if (unlimitedPrecisionExp && (ctx == null || !ctx.HasFlagsOrTraps) &&
        (shift == null || shift.IsValueZero)) {
        ERounding er = (ctx == null) ? ERounding.HalfDown : ctx.Rounding;
        bool negative = (thisFlags & BigNumberFlags.FlagNegative) != 0;
        bool negzero = adjustNegativeZero && negative &&
          this.helper.GetMantissa(thisValue).IsZero &&
          (er != ERounding.Floor);
        if (!negzero) {
          if (er == ERounding.Down) {
            return thisValue;
          }
          if (this.thisRadix == 10 && er == ERounding.HalfEven) {
            if (lastDiscarded < 5) {
              return thisValue;
            } else if (lastDiscarded > 5 || olderDiscarded != 0) {
              FastIntegerFixed bm = this.helper.GetMantissaFastInt(thisValue);
              return this.helper.CreateNewWithFlagsFastInt(
                  FastIntegerFixed.Add(bm, FastIntegerFixed.One),
                  this.helper.GetExponentFastInt(thisValue),
                  thisFlags);
            }
          }
          if (this.thisRadix == 2 && (er == ERounding.HalfEven) &&
            lastDiscarded == 0) {
            return thisValue;
          }
          if (!this.RoundGivenDigits(
              lastDiscarded,
              olderDiscarded,
              er,
              negative,
              FastInteger.FromBig(this.helper.GetMantissa(thisValue)))) {
            return thisValue;
          } else {
            FastIntegerFixed bm = this.helper.GetMantissaFastInt(thisValue);
            return this.helper.CreateNewWithFlagsFastInt(
                FastIntegerFixed.Add(bm, FastIntegerFixed.One),
                this.helper.GetExponentFastInt(thisValue),
                thisFlags);
          }
        }
      }
      if ((lastDiscarded | olderDiscarded) == 0 &&
        (shift == null || shift.IsValueZero)) {
        // DebugUtility.Log("fastpath for "+ctx+", "+thisValue);
        FastIntegerFixed expabs = this.helper.GetExponentFastInt(thisValue);
        if (expabs.IsValueZero && this.IsNullOrInt32FriendlyContext(ctx)) {
          FastIntegerFixed mantabs = this.helper.GetMantissaFastInt(thisValue);
          if (mantabs.IsValueZero && adjustNegativeZero &&
            (thisFlags & BigNumberFlags.FlagNegative) != 0) {
            return this.helper.ValueOf(0);
          }
          if (mantabs.CanFitInInt32()) {
            // compareFast++;
            return thisValue;
          }
        }
      }
      ctx = ctx ?? DefaultUnlimited;
      ERounding rounding = ctx.Rounding;
      bool neg;
      // Check for common binary floating-point contexts, including those
      // covered by Binary32 and Binary64.
      // In the precision check below, no need to check if precision is
      // less than 0, since the EContext object should already ensure this
      if (this.thisRadix == 2 && ctx.HasMaxPrecision && ctx.HasExponentRange &&
        ctx.Precision.CompareTo(53) <= 0 && ctx.EMin.CompareTo(-2000) >= 0 &&
        ctx.EMax.CompareTo(2000) < 0 && (shift == null ||
          shift.CompareToInt(64) < 0)) {
        int intPrecision = ctx.Precision.ToInt32Checked();
        int intEMax = ctx.EMax.ToInt32Checked();
        int intEMin = ctx.EMin.ToInt32Checked();
        int intShift = (shift == null) ? 0 : shift.ToInt32();
        FastIntegerFixed fmant = this.helper.GetMantissaFastInt(thisValue);
        FastIntegerFixed fexp = this.helper.GetExponentFastInt(thisValue);
        if (fmant.CanFitInInt64() && fexp.CanFitInInt32()) {
          long mantlong = fmant.ToInt64();
          int explong = fexp.ToInt32();
          int adjustedExp;
          int normalMin;
          int intDigitCount;
          long origmant = mantlong;
          // get the exponent range
          // Fast path to check if rounding is necessary at all
          // NOTE: At this point, the number won't be infinity or NaN
          if (shift == null || shift.IsValueZero) {
            if (adjustNegativeZero && (thisFlags &
                BigNumberFlags.FlagNegative) !=
              0 && mantlong == 0 && (ctx.Rounding != ERounding.Floor)) {
              // Change negative zero to positive zero
              // except if the rounding mode is Floor
              thisValue = this.EnsureSign(thisValue, false);
              thisFlags = 0;
            }
            intDigitCount = NumberUtility.BitLength(mantlong);
            if (intDigitCount < intPrecision) {
              var withRounding = false;
              var stillWithinPrecision = false;
              if (ctx.HasFlags && (lastDiscarded | olderDiscarded) != 0) {
                ctx.Flags |= EContext.FlagInexact | EContext.FlagRounded;
              }
              // NOTE: accum includes lastDiscarded and olderDiscarded
              if (!this.RoundGivenDigits(
                  lastDiscarded,
                  olderDiscarded,
                  ctx.Rounding,
                  (thisFlags & BigNumberFlags.FlagNegative) != 0,
                  mantlong)) {
                stillWithinPrecision = true;
              } else {
                withRounding = true;
                ++mantlong;
                intDigitCount = NumberUtility.BitLength(mantlong);
                if (intDigitCount < intPrecision ||
                  (intDigitCount == intPrecision && (this.thisRadix & 1) == 0 &&
                    (mantlong & 1) != 0)) {
                  stillWithinPrecision = true;
                } else {
                  long radixPower = 1L << intPrecision;
                  stillWithinPrecision = mantlong < radixPower;
                }
              }
              if (stillWithinPrecision) {
                if (!ctx.HasExponentRange) {
                  return withRounding ? this.helper.CreateNewWithFlagsFastInt(
                      FastIntegerFixed.FromInt64(mantlong),
                      this.helper.GetExponentFastInt(thisValue),
                      thisFlags) : thisValue;
                }
                int bigexp = explong;
                if (ctx == null || ctx.AdjustExponent) {
                  adjustedExp = bigexp + intPrecision - 1;
                  normalMin = intEMin + intPrecision - 1;
                } else {
                  adjustedExp = bigexp;
                  normalMin = intEMin;
                }
                if (adjustedExp <= intEMax && adjustedExp >= normalMin) {
                  return withRounding ? this.helper.CreateNewWithFlagsFastInt(
                      FastIntegerFixed.FromInt64(mantlong),
                      FastIntegerFixed.FromInt32(bigexp),
                      thisFlags) : thisValue;
                }
              }
            }
          }
          // Relatively slow path follows
          // Console.WriteLine("slowpath mantexp="+mantlong+","+explong);
          neg = (thisFlags & BigNumberFlags.FlagNegative) != 0;
          if (adjustNegativeZero && neg && (ctx.Rounding != ERounding.Floor) &&
            origmant == 0) {
            // Change negative zero to positive zero
            // except if the rounding mode is Floor
            thisValue = this.EnsureSign(thisValue, false);
            thisFlags = 0;
            neg = false;
          }
          mantissaWasZero = mantlong == 0 &&
            (lastDiscarded | olderDiscarded) == 0;
          flags = 0;
          intDigitCount = NumberUtility.BitLength(mantlong);
          nonHalfRounding = rounding != ERounding.HalfEven &&
            rounding != ERounding.HalfUp && rounding != ERounding.HalfDown;
          var intDiscardedBits = 0;
          if (intDigitCount > intPrecision) {
            int bitShift = intDigitCount - intPrecision;
            intDiscardedBits += bitShift;
            olderDiscarded |= lastDiscarded;
            // Get the bottommost shift minus 1 bits
            olderDiscarded |= (bitShift > 1 && (mantlong <<
                  (64 - bitShift + 1)) != 0) ? 1 : 0;
            // Get the bit just above that bit
            lastDiscarded = (int)((mantlong >> (bitShift - 1)) & 0x01);
            olderDiscarded = (olderDiscarded != 0) ? 1 : 0;
            mantlong >>= bitShift;
          } else if (intShift > 0 && mantlong != 0) {
            olderDiscarded |= lastDiscarded;
            if (intShift > intDigitCount) {
              olderDiscarded |= 1;
              lastDiscarded = 0;
              mantlong = 0;
            } else {
              // bottommost intShift minus 1 bits
              olderDiscarded |= (intShift > 1 && (mantlong <<
                    (64 - intShift + 1)) != 0) ? 1 : 0;
              lastDiscarded = (int)((mantlong >> (intShift - 1)) & 0x01);
              mantlong >>= intShift;
            }
            intDiscardedBits += intShift;
          }
          explong += intDiscardedBits;
          long intFinalMantissa = 0;
          var intFinalExponent = 0;
          finalizing = false;
          adjustedExp = explong;
          if (ctx.AdjustExponent) {
            adjustedExp += NumberUtility.BitLength(mantlong) - 1;
          }
          if (adjustedExp > intEMax) {
            if (mantissaWasZero) {
              if (ctx.HasFlags) {
                ctx.Flags |= flags | EContext.FlagClamped;
              }
              if (ctx.ClampNormalExponents && ctx.AdjustExponent) {
                // Clamp exponents to eMax + 1 - precision
                // if directed
                intEMax = Math.Min(intEMax, intEMax + 1 - intPrecision);
              }
              return this.helper.CreateNewWithFlagsFastInt(
                  FastIntegerFixed.FromInt64(mantlong),
                  FastIntegerFixed.FromInt32(intEMax),
                  thisFlags);
            }
            return this.SignalOverflow(ctx, neg);
          } else if (adjustedExp < intEMin) {
            // Subnormal
            int etiny = intEMin;
            if (ctx.AdjustExponent) {
              etiny -= intPrecision - 1;
            }
            if (ctx.HasFlags && mantlong != 0) {
              int newAdjExponent = adjustedExp;
              if (this.RoundGivenDigits(
                  lastDiscarded,
                  olderDiscarded,
                  rounding,
                  neg,
                  mantlong)) {
                long intEarlyRounded = mantlong + 1;
                if ((intEarlyRounded >> intPrecision) != 0) {
                  newAdjExponent = ctx.AdjustExponent ?
                    explong + intPrecision - 1 : explong;
                } else {
                  newAdjExponent = ctx.AdjustExponent ?
                    explong + NumberUtility.BitLength(intEarlyRounded) - 1 :
                    explong;
                }
              }
              if (newAdjExponent < intEMin) {
                flags |= EContext.FlagSubnormal;
              }
            }
            int subExp = explong;
            if (subExp < etiny) {
              int intExpDiff = etiny - subExp;
              intDigitCount = NumberUtility.BitLength(mantlong);
              if (mantlong != 0) {
                olderDiscarded |= lastDiscarded;
                if (intExpDiff > intDigitCount) {
                  olderDiscarded |= 1;
                  lastDiscarded = 0;
                  mantlong = 0;
                } else {
                  // bottommost expdiff minus 1 bits
                  olderDiscarded |= (intExpDiff > 1 && (mantlong <<
                        (64 - intExpDiff + 1)) != 0) ? 1 : 0;
                  lastDiscarded = (int)((mantlong >> (intExpDiff - 1)) & 0x01);
                  mantlong >>= intExpDiff;
                }
                intDiscardedBits += intExpDiff;
              }
              /* DebugUtility.Log("mantlong now {0}, ld={1}, od={2} [ed={3},
                 flags={4}]",EInteger.FromInt64(mantlong).ToRadixString(2),
                 lastDiscarded,
                 olderDiscarded, expdiff, flags);
              */
              bool nonZeroDiscardedDigits = (lastDiscarded | olderDiscarded) !=
                0;
              if (intDiscardedBits > 0 || nonZeroDiscardedDigits) {
                if (!mantissaWasZero) {
                  flags |= EContext.FlagRounded;
                }
                if (nonZeroDiscardedDigits) {
                  flags |= EContext.FlagInexact | EContext.FlagRounded;

                  if (rounding == ERounding.None) {
                    return this.SignalInvalidWithMessage(
                        ctx,
                        "Rounding was required");
                  }
                }
                if (this.RoundGivenDigits(
                    lastDiscarded,
                    olderDiscarded,
                    rounding,
                    neg,
                    mantlong)) {
                  ++mantlong;
                }
              }
              if (ctx.HasFlags) {
                if (mantlong == 0) {
                  flags |= EContext.FlagClamped;
                }
                if ((flags & (EContext.FlagSubnormal |
                      EContext.FlagInexact)) == (EContext.FlagSubnormal |
                    EContext.FlagInexact)) {
                  flags |= EContext.FlagUnderflow |
                    EContext.FlagRounded;
                }
              }
              // Finalize result of rounding operation
              intFinalMantissa = mantlong;
              intFinalExponent = etiny;
              finalizing = true;
            }
          }
          if (!finalizing) {
            var recheckOverflow = false;
            bool doRounding = intDiscardedBits != 0 ||
              (lastDiscarded | olderDiscarded) != 0;
            if (doRounding) {
              if (mantlong != 0) {
                flags |= EContext.FlagRounded;
              }
              if ((lastDiscarded | olderDiscarded) != 0) {
                flags |= EContext.FlagInexact | EContext.FlagRounded;
                if (rounding == ERounding.None) {
                  return this.SignalInvalidWithMessage(
                      ctx,
                      "Rounding was required");
                }
              }
              if (this.RoundGivenDigits(
                  lastDiscarded,
                  olderDiscarded,
                  rounding,
                  neg,
                  mantlong)) {
                ++mantlong;
                // Check if mantissa's precision is now greater
                // than the one set by the context
                if ((mantlong >> intPrecision) != 0) {
                  mantlong >>= 1;
                  ++explong;
                  recheckOverflow = true;
                }
              }
            }
            if (recheckOverflow) {
              // Check for overflow again
              adjustedExp = explong;
              if (ctx.AdjustExponent) {
                adjustedExp += NumberUtility.BitLength(mantlong) - 1;
              }
              if (adjustedExp > intEMax) {
                return this.SignalOverflow(ctx, neg);
              }
            }
            intFinalMantissa = mantlong;
            intFinalExponent = explong;
          }
          if (ctx.ClampNormalExponents) {
            // Clamp exponents to eMax + 1 - precision
            // if directed
            int clampExp = intEMax;
            if (ctx.AdjustExponent) {
              clampExp += intPrecision - 1;
            }
            if (intFinalExponent > clampExp) {
              if (intFinalMantissa != 0) {
                int expdiff = intFinalExponent - clampExp;
                // DebugUtility.Log("Clamping " + exp + " to " + clampExp);
                intFinalMantissa <<= expdiff;
              }
              if (ctx.HasFlags) {
                flags |= EContext.FlagClamped;
              }
              intFinalExponent = clampExp;
            }
          }
          if (ctx.HasFlags) {
            ctx.Flags |= flags;
          }
          return this.helper.CreateNewWithFlagsFastInt(
              FastIntegerFixed.FromInt64(intFinalMantissa),
              FastIntegerFixed.FromInt64(intFinalExponent),
              neg ? BigNumberFlags.FlagNegative : 0);
        }
      }

      // Binary precision of potentially non-binary numbers
      // binaryPrec means whether precision is the number of bits and not
      // digits
      bool binaryPrec = ctx.IsPrecisionInBits && this.thisRadix != 2 &&
        !ctx.Precision.IsZero;
      // get the precision
      FastInteger fastPrecision = FastInteger.FromBig(ctx.Precision);
      IShiftAccumulator accum = null;
      FastInteger fastAdjustedExp;
      FastInteger fastNormalMin;
      FastInteger fastEMin = null;
      FastInteger fastEMax = null;
      FastIntegerFixed fastEMaxFixed = null;
      // get the exponent range
      if (ctx != null && ctx.HasExponentRange) {
        fastEMax = FastInteger.FromBig(ctx.EMax);
        fastEMaxFixed = FastIntegerFixed.FromBig(ctx.EMax);
        fastEMin = FastInteger.FromBig(ctx.EMin);
      }
      bool unlimitedPrec = !ctx.HasMaxPrecision;
      if (!binaryPrec) {
        // Fast path to check if rounding is necessary at all
        // NOTE: At this point, the number won't be infinity or NaN
        if (!unlimitedPrec && (shift == null || shift.IsValueZero)) {
          FastIntegerFixed mantabs = this.helper.GetMantissaFastInt(
              thisValue);
          if (adjustNegativeZero && (thisFlags & BigNumberFlags.FlagNegative) !=
            0 && mantabs.IsValueZero && (ctx.Rounding != ERounding.Floor)) {
            // Change negative zero to positive zero
            // except if the rounding mode is Floor
            thisValue = this.EnsureSign(thisValue, false);
            thisFlags = 0;
          }
          accum = this.helper.CreateShiftAccumulatorWithDigitsFastInt(
              mantabs,
              lastDiscarded,
              olderDiscarded);
          FastInteger estDigitCount = accum.OverestimateDigitLength();
          // NOTE: Overestimating the digit count will catch most,
          // but not all, numbers that fit fastPrecision, and will not
          // catch any numbers that don't fit fastPrecision
          // DebugUtility.Log("estDigitCount=" + estDigitCount + ", " +
          // fastPrecision);
          if (estDigitCount.CompareTo(fastPrecision) <= 0) {
            var withRounding = false;
            var stillWithinPrecision = false;
            if (ctx.HasFlags && (lastDiscarded | olderDiscarded) != 0) {
              ctx.Flags |= EContext.FlagInexact | EContext.FlagRounded;
            }
            if (!this.RoundGivenAccum(
                accum,
                ctx.Rounding,
                (thisFlags & BigNumberFlags.FlagNegative) != 0)) {
              stillWithinPrecision = true;
            } else {
              withRounding = true;
              mantabs = mantabs.Increment();
              FastInteger digitCount = accum.GetDigitLength();
              int precisionCmp = digitCount.CompareTo(fastPrecision);
              if (precisionCmp < 0 ||
                (precisionCmp == 0 && (this.thisRadix & 1) == 0 &&
                  !mantabs.IsEvenNumber)) {
                stillWithinPrecision = true;
              } else {
                EInteger radixPower =
                  this.TryMultiplyByRadixPower(EInteger.One, fastPrecision);
                // DebugUtility.Log("now " + mantabs + "," + fastPrecision);
                if (radixPower == null) {
                  return this.SignalInvalidWithMessage(
                      ctx,
                      "Result requires too much memory");
                }
                stillWithinPrecision = mantabs.CompareTo(radixPower) <
                  0;
              }
            }
            if (stillWithinPrecision) {
              if (!ctx.HasExponentRange) {
                return withRounding ? this.helper.CreateNewWithFlagsFastInt(
                    mantabs,
                    this.helper.GetExponentFastInt(thisValue),
                    thisFlags) : thisValue;
              }
              FastIntegerFixed bigexp =
                this.helper.GetExponentFastInt(thisValue);
              if (ctx == null || ctx.AdjustExponent) {
                fastAdjustedExp = bigexp.ToFastInteger()
                  .Add(fastPrecision).Decrement();
                fastNormalMin = fastEMin.Copy()
                  .Add(fastPrecision).Decrement();
              } else {
                fastAdjustedExp = bigexp.ToFastInteger();
                fastNormalMin = fastEMin;
              }
              // DebugUtility.Log("{0}->{1},{2}"
              // , fastAdjustedExp, fastEMax, fastNormalMin);
              if (fastAdjustedExp.CompareTo(fastEMax) <= 0 &&
                fastAdjustedExp.CompareTo(fastNormalMin) >= 0) {
                return withRounding ? this.helper.CreateNewWithFlagsFastInt(
                    mantabs,
                    bigexp,
                    thisFlags) : thisValue;
              }
            }
          }
        }
      }
      // compareSlow++;
      neg = (thisFlags & BigNumberFlags.FlagNegative) != 0;
      if (adjustNegativeZero && neg && (rounding != ERounding.Floor) &&
        this.helper.GetMantissa(thisValue).IsZero) {
        // Change negative zero to positive zero
        // except if the rounding mode is Floor
        thisValue = this.EnsureSign(thisValue, false);
        thisFlags = 0;
        neg = false;
      }
      FastIntegerFixed bigmantissa = this.helper.GetMantissaFastInt(thisValue);
      mantissaWasZero = bigmantissa.IsValueZero && (lastDiscarded |
          olderDiscarded) == 0;
      FastIntegerFixed expfixed = this.helper.GetExponentFastInt(thisValue);
      FastInteger exp = expfixed.ToFastInteger();
      flags = 0;
      if (accum == null) {
        accum = this.helper.CreateShiftAccumulatorWithDigitsFastInt(
            bigmantissa,
            lastDiscarded,
            olderDiscarded);
      }
      #if DEBUG
      if (!accum.DiscardedDigitCount.IsValueZero) {
        throw new ArgumentException(
          "doesn't satisfy accum.DiscardedDigitCount.IsValueZero");
      }
      #endif
      FastInteger bitLength = fastPrecision;
      if (binaryPrec) {
        fastPrecision =
          this.DigitLengthUpperBoundForBitPrecision(fastPrecision);
      }
      nonHalfRounding = rounding != ERounding.HalfEven &&
        rounding != ERounding.HalfUp && rounding != ERounding.HalfDown;
      if (ctx != null && ctx.HasMaxPrecision &&
        ctx.HasExponentRange) {
        long estMantDigits = bigmantissa.CanFitInInt32() ?
          10 : bigmantissa.ToEInteger().GetUnsignedBitLengthAsInt64();
        if (estMantDigits > 128) {
          // Get bounds on stored precision
          FastIntegerFixed[] bounds = NumberUtility.DigitLengthBoundsFixed(
              this.helper,
              bigmantissa);
          FastIntegerFixed lowExpBound = expfixed;
          if (ctx.AdjustExponent) {
            lowExpBound = lowExpBound.Add(bounds[0]).Subtract(2);
          }
          FastIntegerFixed highExpBound = expfixed;
          highExpBound = highExpBound.Add(bounds[1]);
          FastIntegerFixed fpf =
            FastIntegerFixed.FromFastInteger(fastPrecision);
          /* string
          ch1=""+lowExpBound;ch1=ch1.Substring(0,Math.Min(12,ch1.Length));
          string
          ch2=""+highExpBound;ch2=ch2.Substring(0,Math.Min(12,ch2.Length));
          DebugUtility.Log("exp="+expfixed);
          DebugUtility.Log("bounds="+ch1+"/"+ch2+"/"+fastEMax+
            " fpf="+fastPrecision + " highexp=" +highExpBound.Add(fpf).Add(4));
          */ if (lowExpBound.CompareTo(fastEMax) > 0) {
            // Overflow.
            return this.SignalOverflow(ctx, neg);
          }
          FastIntegerFixed underflowBound = highExpBound.Add(fpf).Add(4);
          // FastIntegerFixed underflowBound2 = highExpBound.Add(bounds[1]).Add(4);
          // if (underflowBound2.CompareTo(underflowBound) > 0) {
          // underflowBound = underflowBound2;
          // }
          // DebugUtility.Log("underflowBound="+underflowBound);
          if (underflowBound.CompareTo(fastEMin) < 0) {
            // Underflow.
            // NOTE: Due to estMantDigits check
            // above, we know significand is neither zero nor 1(
            // SignalUnderflow will pass significands of 0 or 1 to
            // RoundToPrecision).
            return this.SignalUnderflow(ctx, neg, false);
          }
          /*
           DebugUtility.Log("mantbits=" +
               bigmantissa.ToEInteger().GetUnsignedBitLengthAsInt64() +
               " shift=" + shift + " fastprec=" + fastPrecision +
               " expbits=" + exp.ToEInteger().GetUnsignedBitLengthAsInt64() +
               " expsign=" + exp.ToEInteger().CompareTo(0)); */
        }
      }
      if (!unlimitedPrec) {
        accum.ShiftToDigits(fastPrecision, shift, nonHalfRounding);
      } else {
        if (shift != null && shift.Sign != 0) {
          accum.TruncateOrShiftRight(shift, nonHalfRounding);
        }
        fastPrecision = accum.GetDigitLength();
      }
      if (binaryPrec) {
        while (bitLength.CompareTo(
            accum.ShiftedInt.GetUnsignedBitLengthAsEInteger()) < 0) {
          accum.ShiftRightInt(1);
        }
      }
      FastInteger discardedBits = accum.DiscardedDigitCount.Copy();
      exp.Add(discardedBits);
      FastIntegerFixed finalMantissa = null;
      FastIntegerFixed finalExponent = null;
      finalizing = false;
      FastInteger adjExponent;
      adjExponent = ctx.AdjustExponent ?
        exp.Copy().Add(accum.GetDigitLength()).Decrement() : exp.Copy();
      if (binaryPrec) {
        // NOTE: Binary precision case only
        if (fastEMax != null && adjExponent.CompareTo(fastEMax) == 0) {
          // May or may not be an overflow depending on the mantissa
          FastInteger expdiff =
            fastPrecision.Copy().Subtract(accum.GetDigitLength());
          EInteger currMantissa = accum.ShiftedInt;
          currMantissa = this.TryMultiplyByRadixPower(currMantissa, expdiff);
          if (currMantissa == null) {
            return this.SignalInvalidWithMessage(
                ctx,
                "Result requires too much memory");
          }

          if (bitLength.CompareTo(
              currMantissa.GetUnsignedBitLengthAsEInteger()) < 0) {
            // Mantissa too high, treat as overflow
            adjExponent.Increment();
          }
        }
      }
      if (fastEMax != null && adjExponent.CompareTo(fastEMax) > 0) {
        if (mantissaWasZero) {
          if (ctx.HasFlags) {
            ctx.Flags |= flags | EContext.FlagClamped;
          }
          if (ctx.ClampNormalExponents && ctx.AdjustExponent) {
            // Clamp exponents to eMax + 1 - precision
            // if directed
            FastInteger clampExp = fastEMax.Copy();
            clampExp.Increment().Subtract(fastPrecision);
            if (fastEMax.CompareTo(clampExp) > 0) {
              fastEMax = clampExp;
            }
          }
          return this.helper.CreateNewWithFlagsFastInt(
              bigmantissa,
              FastIntegerFixed.FromFastInteger(fastEMax),
              thisFlags);
        }
        return this.SignalOverflow(ctx, neg);
      } else if (fastEMin != null && adjExponent.CompareTo(fastEMin) < 0) {
        // Subnormal
        FastInteger fastETiny = fastEMin;
        if (ctx.AdjustExponent) {
          fastETiny = fastETiny.Copy().Subtract(fastPrecision).Increment();
        }
        if (ctx.HasFlags && !accum.ShiftedInt.IsZero) {
          FastInteger newAdjExponent = adjExponent;
          if (this.RoundGivenAccum(accum, rounding, neg)) {
            EInteger earlyRounded = accum.ShiftedInt + EInteger.One;
            if (!unlimitedPrec && (earlyRounded.IsEven ||
                (this.thisRadix & 1) != 0)) {
              FastInteger newDigitLength =
                this.helper.GetDigitLength(earlyRounded);
              // Ensure newDigitLength doesn't exceed precision
              if (binaryPrec || newDigitLength.CompareTo(fastPrecision) >
                0) {
                newDigitLength = fastPrecision.Copy();
              }
              newAdjExponent = ctx.AdjustExponent ?
                exp.Copy().Add(newDigitLength).Decrement() : exp;
            }
          }
          if (newAdjExponent.CompareTo(fastEMin) < 0) {
            // DebugUtility.Log("subnormal");
            flags |= EContext.FlagSubnormal;
          }
        }
        // DebugUtility.Log("exp=" + exp + " eTiny=" + fastETiny);
        FastInteger subExp = exp.Copy();
        // DebugUtility.Log("exp=" + subExp + " eTiny=" + fastETiny);
        if (subExp.CompareTo(fastETiny) < 0) {
          // DebugUtility.Log("Less than ETiny");
          FastInteger expdiff = fastETiny.Copy().Subtract(subExp);
          // DebugUtility.Log("<ETiny: " + (accum.ShiftedInt));
          accum.TruncateOrShiftRight(
            expdiff,
            nonHalfRounding);
          // DebugUtility.Log("<ETiny2: " + (accum.ShiftedInt));
          FastInteger newmantissa = accum.ShiftedIntFast;
          bool nonZeroDiscardedDigits = (accum.LastDiscardedDigit |
              accum.OlderDiscardedDigits) != 0;
          // if (rounding == ERounding.None) {
          // DebugUtility.Log("<nzdd= " + nonZeroDiscardedDigits);
          // }
          if (accum.DiscardedDigitCount.Sign != 0 || nonZeroDiscardedDigits) {
            if (!mantissaWasZero) {
              flags |= EContext.FlagRounded;
            }
            if (nonZeroDiscardedDigits) {
              flags |= EContext.FlagInexact | EContext.FlagRounded;
              if (rounding == ERounding.None) {
                return this.SignalInvalidWithMessage(
                    ctx,
                    "Rounding was required");
              }
            }

            if (this.RoundGivenAccum(accum, rounding, neg)) {
              newmantissa.Increment();
            }
          }
          if (ctx.HasFlags) {
            if (newmantissa.IsValueZero) {
              flags |= EContext.FlagClamped;
            }
            if ((flags & (EContext.FlagSubnormal |
                  EContext.FlagInexact)) == (EContext.FlagSubnormal |
                EContext.FlagInexact)) {
              flags |= EContext.FlagUnderflow |
                EContext.FlagRounded;
            }
          }
          // Finalize result of rounding operation
          finalMantissa = FastIntegerFixed.FromFastInteger(newmantissa);
          finalExponent = FastIntegerFixed.FromFastInteger(fastETiny);
          finalizing = true;
        }
      }
      if (!finalizing) {
        // DebugUtility.Log("" + accum.ShiftedInt + ", exp=" +
        // adjExponent + "/" + fastEMin);
        var recheckOverflow = false;
        bool doRounding = accum.DiscardedDigitCount.Sign != 0 ||
          (accum.LastDiscardedDigit | accum.OlderDiscardedDigits) != 0;
        EInteger bigmantissaEInteger = accum.ShiftedInt;
        if (doRounding) {
          if (!bigmantissaEInteger.IsZero) {
            flags |= EContext.FlagRounded;
          }
          if ((accum.LastDiscardedDigit | accum.OlderDiscardedDigits) != 0) {
            flags |= EContext.FlagInexact | EContext.FlagRounded;
            if (rounding == ERounding.None) {
              return this.SignalInvalidWithMessage(
                  ctx,
                  "Rounding was required");
            }
          }
          if (this.RoundGivenAccum(accum, rounding, neg)) {
            // DebugUtility.Log("recheck overflow {0} {1} / {2} [prec={3}]"
            // , adjExponent, fastEMax, accum.ShiftedInt, fastPrecision);
            bigmantissaEInteger = bigmantissaEInteger.Add(1);
            recheckOverflow |= binaryPrec;
            // Check if mantissa's precision is now greater
            // than the one set by the context
            if (!unlimitedPrec &&
              (bigmantissaEInteger.IsEven || (this.thisRadix & 1) != 0) &&
              (binaryPrec || accum.GetDigitLength().CompareTo(fastPrecision) >=

                0)) {
              accum = this.helper.CreateShiftAccumulatorWithDigits(
                  bigmantissaEInteger,
                  0,
                  0);
              FastInteger newDigitLength = accum.GetDigitLength();
              if (binaryPrec || newDigitLength.CompareTo(fastPrecision) > 0) {
                FastInteger neededShift =
                  newDigitLength.Copy().Subtract(fastPrecision);
                accum.TruncateOrShiftRight(
                  neededShift,
                  nonHalfRounding);
                if (binaryPrec) {
                  while (bitLength.CompareTo(
                      accum.ShiftedInt.GetUnsignedBitLengthAsEInteger()) < 0) {
                    accum.ShiftRightInt(1);
                  }
                }
                if (accum.DiscardedDigitCount.Sign != 0) {
                  exp.Add(accum.DiscardedDigitCount);
                  discardedBits.Add(accum.DiscardedDigitCount);
                  bigmantissaEInteger = accum.ShiftedInt;
                  recheckOverflow |= !binaryPrec;
                }
              }
            }
          }
        }
        if (fastEMax != null && recheckOverflow) {
          // Check for overflow again
          // DebugUtility.Log("recheck overflow2 {0} {1} / {2}"
          // , adjExponent, fastEMax, accum.ShiftedInt);
          adjExponent = exp.Copy();
          if (ctx.AdjustExponent) {
            adjExponent.Add(accum.GetDigitLength()).Decrement();
          }
          if (binaryPrec && fastEMax != null &&
            adjExponent.CompareTo(fastEMax) == 0) {
            // May or may not be an overflow depending on the mantissa
            // (uses accumulator from previous steps, including the check
            // if the mantissa now exceeded the precision)
            FastInteger expdiff =
              fastPrecision.Copy().Subtract(accum.GetDigitLength());
            EInteger currMantissa = accum.ShiftedInt;
            currMantissa = this.TryMultiplyByRadixPower(currMantissa, expdiff);
            if (currMantissa == null) {
              return this.SignalInvalidWithMessage(
                  ctx,
                  "Result requires too much memory");
            }
            if (bitLength.CompareTo(
                currMantissa.GetUnsignedBitLengthAsEInteger()) < 0) {
              // Mantissa too high, treat as overflow
              adjExponent.Increment();
            }
          }
          if (adjExponent.CompareTo(fastEMax) > 0) {
            return this.SignalOverflow(ctx, neg);
          }
        }
        finalMantissa = FastIntegerFixed.FromBig(bigmantissaEInteger);
        finalExponent = FastIntegerFixed.FromFastInteger(exp);
      }
      // Finalize the result of the rounding operation
      if (ctx.ClampNormalExponents) {
        // Clamp exponents to eMax + 1 - precision
        // if directed
        FastInteger clampExp = fastEMax.Copy();
        if (ctx.AdjustExponent) {
          clampExp.Increment().Subtract(fastPrecision);
        }
        if (exp.CompareTo(clampExp) > 0) {
          if (!finalMantissa.IsValueZero) {
            FastIntegerFixed expdiff = FastIntegerFixed.Subtract(
                finalExponent,
                FastIntegerFixed.FromFastInteger(clampExp));
            // DebugUtility.Log("Clamping " + exp + " to " + clampExp);
            finalMantissa = this.TryMultiplyByRadixPowerFastInt(
                finalMantissa,
                expdiff);
            if (finalMantissa == null) {
              return this.SignalInvalidWithMessage(
                  ctx,
                  "Result requires too much memory");
            }
          }
          if (ctx.HasFlags) {
            flags |= EContext.FlagClamped;
          }
          finalExponent = FastIntegerFixed.FromFastInteger(clampExp);
        }
      }
      if (ctx.HasFlags) {
        ctx.Flags |= flags;
      }
      return this.helper.CreateNewWithFlagsFastInt(
          finalMantissa,
          finalExponent,
          neg ? BigNumberFlags.FlagNegative : 0);
    }

    // Compare bigLeft with half of toCompareWith, while avoiding
    // the need to compute half of toCompareWith in many cases.
    // Assumes both inputs are positive.
    private static int CompareToHalf(EInteger bigLeft, EInteger toCompareWith) {
      #if DEBUG
      if (!(bigLeft.Sign > 0 && toCompareWith.Sign > 0)) {
        throw new ArgumentException("doesn't satisfy bigLeft.Sign > 0 && " +
          "toCompareWith.Sign > 0");
      }
      #endif
      long a = bigLeft.GetUnsignedBitLengthAsInt64();
      long b = toCompareWith.GetUnsignedBitLengthAsInt64();
      if (a != Int64.MaxValue && b != Int64.MaxValue) {
        if (b - 1 > a) {
          return -1;
        }
        if (a - 1 > b) {
          return 1;
        }
      }
      int cmp = bigLeft.CompareTo(toCompareWith.ShiftRight(1));
      return (cmp == 0 && !toCompareWith.IsEven) ? -1 : cmp;
    }

    private T RoundToScale(
      EInteger mantissa,
      EInteger remainder,
      EInteger divisor,
      EInteger desiredExponent,
      FastInteger shift,
      bool neg,
      EContext ctx) {
      #if DEBUG
      if (mantissa.Sign < 0) {
        throw new ArgumentException("doesn't satisfy mantissa.Sign>= 0");
      }
      if (remainder.Sign < 0) {
        throw new ArgumentException("doesn't satisfy remainder.Sign>= 0");
      }
      if (divisor.Sign < 0) {
        throw new ArgumentException("doesn't satisfy divisor.Sign>= 0");
      }
      #endif
      ERounding rounding = (ctx == null) ? ERounding.HalfEven : ctx.Rounding;
      var lastDiscarded = 0;
      var olderDiscarded = 0;
      if (!remainder.IsZero) {
        if (rounding == ERounding.HalfDown || rounding == ERounding.HalfUp ||
          rounding == ERounding.HalfEven) {
          int cmpHalf = CompareToHalf(remainder, divisor);
          if (cmpHalf == 0) {
            // remainder is exactly half
            lastDiscarded = this.thisRadix / 2;
            olderDiscarded = 0;
          } else if (cmpHalf > 0) {
            // remainder is greater than half
            lastDiscarded = this.thisRadix / 2;
            olderDiscarded = 1;
          } else {
            // remainder is less than half
            lastDiscarded = 0;
            olderDiscarded = 1;
          }
        } else {
          // Rounding mode doesn't care about
          // whether remainder is exactly half
          if (rounding == ERounding.None) {
            return this.SignalInvalidWithMessage(
                ctx,
                "Rounding was required");
          }
          lastDiscarded = 1;
          olderDiscarded = 1;
        }
      }
      var flags = 0;
      EInteger newmantissa = mantissa;
      if (shift.IsValueZero) {
        if ((lastDiscarded | olderDiscarded) != 0) {
          flags |= EContext.FlagInexact | EContext.FlagRounded;
          if (rounding == ERounding.None) {
            return this.SignalInvalidWithMessage(
                ctx,
                "Rounding was required");
          }
          FastInteger fastNewMantissa = FastInteger.FromBig(newmantissa);
          if (
            this.RoundGivenDigits(
              lastDiscarded,
              olderDiscarded,
              rounding,
              neg,
              fastNewMantissa)) {
            newmantissa += EInteger.One;
          }
        }
      } else {
        IShiftAccumulator accum = this.helper.CreateShiftAccumulatorWithDigits(
            mantissa,
            lastDiscarded,
            olderDiscarded);
        accum.TruncateOrShiftRight(
          shift,
          false);
        newmantissa = accum.ShiftedInt;
        if (accum.DiscardedDigitCount.Sign != 0 ||
          (accum.LastDiscardedDigit | accum.OlderDiscardedDigits) !=
          0) {
          if (!mantissa.IsZero) {
            flags |= EContext.FlagRounded;
          }
          if ((accum.LastDiscardedDigit | accum.OlderDiscardedDigits) != 0) {
            flags |= EContext.FlagInexact | EContext.FlagRounded;
            if (rounding == ERounding.None) {
              return this.SignalInvalidWithMessage(
                  ctx,
                  "Rounding was required");
            }
          }
          if (this.RoundGivenAccum(accum, rounding, neg)) {
            newmantissa += EInteger.One;
          }
        }
      }
      if (ctx != null && ctx.HasFlags) {
        ctx.Flags |= flags;
      }
      return this.helper.CreateNewWithFlags(
          newmantissa,
          desiredExponent,
          neg ? BigNumberFlags.FlagNegative : 0);
    }

    private int[] RoundToScaleStatus(
      EInteger remainder,
      EInteger divisor,
      EContext ctx) {
      ERounding rounding = (ctx == null) ? ERounding.HalfEven : ctx.Rounding;
      var lastDiscarded = 0;
      var olderDiscarded = 0;
      if (!remainder.IsZero) {
        if (rounding == ERounding.HalfDown || rounding == ERounding.HalfUp ||
          rounding == ERounding.HalfEven) {
          int cmpHalf = CompareToHalf(remainder, divisor);
          if (cmpHalf == 0) {
            // remainder is exactly half
            lastDiscarded = this.thisRadix / 2;
            olderDiscarded = 0;
          } else if (cmpHalf > 0) {
            // remainder is greater than half
            lastDiscarded = this.thisRadix / 2;
            olderDiscarded = 1;
          } else {
            // remainder is less than half
            lastDiscarded = 0;
            olderDiscarded = 1;
          }
        } else {
          // Rounding mode doesn't care about
          // whether remainder is exactly half
          if (rounding == ERounding.None) {
            // Rounding was required
            return null;
          }
          lastDiscarded = 1;
          olderDiscarded = 1;
        }
      }
      return new[] { lastDiscarded, olderDiscarded };
    }

    private T SignalDivideByZero(EContext ctx, bool neg) {
      if (ctx != null && ctx.HasFlags) {
        ctx.Flags |= EContext.FlagDivideByZero;
      }
      if (this.support == BigNumberFlags.FiniteOnly) {
        throw new DivideByZeroException("Division by zero");
      }
      int flags = BigNumberFlags.FlagInfinity |
        (neg ? BigNumberFlags.FlagNegative : 0);
      return this.helper.CreateNewWithFlags(
          EInteger.Zero,
          EInteger.Zero,
          flags);
    }

    private T SignalingNaNInvalid(T value, EContext ctx) {
      if (ctx != null && ctx.HasFlags) {
        ctx.Flags |= EContext.FlagInvalid;
      }
      return this.ReturnQuietNaN(value, ctx);
    }

    private T SignalInvalid(EContext ctx) {
      if (ctx != null && ctx.HasFlags) {
        ctx.Flags |= EContext.FlagInvalid;
      }
      if (this.support == BigNumberFlags.FiniteOnly) {
        throw new ArithmeticException("Invalid operation");
      }
      return this.helper.CreateNewWithFlags(
          EInteger.Zero,
          EInteger.Zero,
          BigNumberFlags.FlagQuietNaN);
    }

    private T SignalInvalidWithMessage(EContext ctx, string str) {
      if (ctx != null && ctx.HasFlags) {
        ctx.Flags |= EContext.FlagInvalid;
      }
      if (this.support == BigNumberFlags.FiniteOnly) {
        throw new ArithmeticException(str);
      }
      // if (str.IndexOf("Rounding was") < 0) {
      // throw new ArithmeticException(str);
      // } else {
      // DebugUtility.Log(str);
      // }
      return this.helper.CreateNewWithFlags(
          EInteger.Zero,
          EInteger.Zero,
          BigNumberFlags.FlagQuietNaN);
    }

    public T SignalOverflow(EContext ctx, bool neg) {
      if (ctx != null) {
        ERounding roundingOnOverflow = ctx.Rounding;
        if (ctx.HasFlags) {
          ctx.Flags |= EContext.FlagOverflow |
            EContext.FlagInexact | EContext.FlagRounded;
        }
        if (roundingOnOverflow == ERounding.None) {
          return this.SignalInvalidWithMessage(
              ctx,
              "Rounding was required");
        }
        if (ctx.HasMaxPrecision && ctx.HasExponentRange &&
          (roundingOnOverflow == ERounding.Down ||
            roundingOnOverflow == ERounding.ZeroFiveUp ||
            roundingOnOverflow == ERounding.OddOrZeroFiveUp ||
            roundingOnOverflow == ERounding.Odd ||
            (roundingOnOverflow == ERounding.Ceiling && neg) ||
            (roundingOnOverflow == ERounding.Floor && !neg))) {
          // Set to the highest possible value for
          // the given precision
          EInteger overflowMant = EInteger.Zero;
          FastInteger fastPrecision = FastInteger.FromBig(ctx.Precision);
          overflowMant = this.TryMultiplyByRadixPower(
              EInteger.One,
              fastPrecision);
          if (overflowMant == null) {
            return this.SignalInvalidWithMessage(
                ctx,
                "Result requires too much memory");
          }
          overflowMant -= EInteger.One;
          FastInteger clamp = FastInteger.FromBig(ctx.EMax);
          if (ctx.AdjustExponent) {
            clamp.Increment().Subtract(fastPrecision);
          }
          return this.helper.CreateNewWithFlags(
              overflowMant,
              clamp.ToEInteger(),
              neg ? BigNumberFlags.FlagNegative : 0);
        }
      }
      return this.support == BigNumberFlags.FiniteOnly ? default(T) :
        this.helper.CreateNewWithFlags(
          EInteger.Zero,
          EInteger.Zero,
          (neg ? BigNumberFlags.FlagNegative : 0) | BigNumberFlags.FlagInfinity);
    }

    private T SquareRootHandleSpecial(T thisValue, EContext ctx) {
      int thisFlags = this.helper.GetFlags(thisValue);
      if ((thisFlags & BigNumberFlags.FlagSpecial) != 0) {
        if ((thisFlags & BigNumberFlags.FlagSignalingNaN) != 0) {
          return this.SignalingNaNInvalid(thisValue, ctx);
        }
        if ((thisFlags & BigNumberFlags.FlagQuietNaN) != 0) {
          return this.ReturnQuietNaN(thisValue, ctx);
        }
        if ((thisFlags & BigNumberFlags.FlagInfinity) != 0) {
          // Square root of infinity
          return ((thisFlags & BigNumberFlags.FlagNegative) != 0) ?
            this.SignalInvalid(ctx) : thisValue;
        }
      }
      int sign = this.helper.GetSign(thisValue);
      return (sign < 0) ? this.SignalInvalid(ctx) : default(T);
    }

    private EInteger TryMultiplyByRadixPower(
      EInteger bi,
      int radixPowerInt) {
      if (bi.IsZero || radixPowerInt == 0) {
        return bi;
      }
      return this.helper.MultiplyByRadixPower(bi,
          new FastInteger(radixPowerInt));
    }

    private EInteger TryMultiplyByRadixPower(
      EInteger bi,
      FastInteger radixPower) {
      if (bi.IsZero) {
        return bi;
      }
      if (!radixPower.CanFitInInt32()) {
        // NOTE: For radix 10, each digit fits less than 1 byte; the
        // supported byte length is thus less than the maximum value
        // of a 32-bit integer (2GB).
        FastInteger fastBI = FastInteger.FromBig(valueMaxDigits);
        if (this.thisRadix != 10 || radixPower.CompareTo(fastBI) > 0) {
          return null;
        }
      }
      return this.helper.MultiplyByRadixPower(bi, radixPower);
    }

    private FastIntegerFixed TryMultiplyByRadixPowerFastInt(
      FastIntegerFixed bi,
      FastIntegerFixed radixPower) {
      if (bi.IsValueZero) {
        return bi;
      }
      if (!radixPower.CanFitInInt32()) {
        // NOTE: For radix 10, each digit fits less than 1 byte; the
        // supported byte length is thus less than the maximum value
        // of a 32-bit integer (2GB).
        FastIntegerFixed fastBI = FastIntegerFixed.FromBig(valueMaxDigits);
        if (this.thisRadix != 10 || radixPower.CompareTo(fastBI) > 0) {
          return null;
        }
        return FastIntegerFixed.FromBig(this.helper.MultiplyByRadixPower(
              bi.ToEInteger(),
              FastInteger.FromBig(radixPower.ToEInteger())));
      } else {
        return FastIntegerFixed.FromBig(this.helper.MultiplyByRadixPower(
              bi.ToEInteger(),
              new FastInteger(radixPower.ToInt32())));
      }
    }

    private FastIntegerFixed TryMultiplyByRadixPowerFastInt(
      FastIntegerFixed bi,
      FastInteger radixPower) {
      if (bi.IsValueZero) {
        return bi;
      }
      if (!radixPower.CanFitInInt32()) {
        // NOTE: For radix 10, each digit fits less than 1 byte; the
        // supported byte length is thus less than the maximum value
        // of a 32-bit integer (2GB).
        FastInteger fastBI = FastInteger.FromBig(valueMaxDigits);
        if (this.thisRadix != 10 || radixPower.CompareTo(fastBI) > 0) {
          return null;
        }
      }
      return FastIntegerFixed.FromBig(this.helper.MultiplyByRadixPower(
            bi.ToEInteger(),
            radixPower));
    }

    private T ValueOf(int value, EContext ctx) {
      return (ctx == null || !ctx.HasExponentRange ||
          ctx.ExponentWithinRange(EInteger.Zero)) ?
        this.helper.ValueOf(value) :
        this.RoundToPrecision(this.helper.ValueOf(value), ctx);
    }
  }
}
