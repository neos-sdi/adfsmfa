/*
Written by Peter O. in 2013.
Any copyright is dedicated to the Public Domain.
http://creativecommons.org/publicdomain/zero/1.0/
If you like this, you should donate to Peter O.
at: http://peteroupc.github.io/
 */
using System;
using System.Text;

namespace PeterO.Numbers {
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:PeterO.Numbers.EDecimal"]/*'/>
  public sealed partial class EDecimal : IComparable<EDecimal>,
  IEquatable<EDecimal> {
    //----------------------------------------------------------------

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.EDecimal.NaN"]/*'/>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
                "CA2104", Justification = "EDecimal is immutable")]
#endif
    public static readonly EDecimal NaN = CreateWithFlags(
        EInteger.Zero,
        EInteger.Zero,
        BigNumberFlags.FlagQuietNaN);

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.EDecimal.NegativeInfinity"]/*'/>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
                "CA2104", Justification = "EDecimal is immutable")]
#endif
    public static readonly EDecimal NegativeInfinity =
      CreateWithFlags(
        EInteger.Zero,
        EInteger.Zero,
        BigNumberFlags.FlagInfinity | BigNumberFlags.FlagNegative);

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.EDecimal.NegativeZero"]/*'/>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
                "CA2104", Justification = "EDecimal is immutable")]
#endif
    public static readonly EDecimal NegativeZero =
      CreateWithFlags(
        EInteger.Zero,
        EInteger.Zero,
        BigNumberFlags.FlagNegative);

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.EDecimal.One"]/*'/>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
                "CA2104", Justification = "EDecimal is immutable")]
#endif
    public static readonly EDecimal One =
      EDecimal.Create(EInteger.One, EInteger.Zero);

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.EDecimal.PositiveInfinity"]/*'/>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
                "CA2104", Justification = "EDecimal is immutable")]
#endif
    public static readonly EDecimal PositiveInfinity =
      CreateWithFlags(
        EInteger.Zero,
        EInteger.Zero,
        BigNumberFlags.FlagInfinity);

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.EDecimal.SignalingNaN"]/*'/>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
                "CA2104", Justification = "EDecimal is immutable")]
#endif
public static readonly EDecimal SignalingNaN =
      CreateWithFlags(
        EInteger.Zero,
        EInteger.Zero,
        BigNumberFlags.FlagSignalingNaN);

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.EDecimal.Ten"]/*'/>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
                "CA2104", Justification = "EDecimal is immutable")]
#endif

    public static readonly EDecimal Ten =
      EDecimal.Create((EInteger)10, EInteger.Zero);

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.EDecimal.Zero"]/*'/>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
                "CA2104", Justification = "EDecimal is immutable")]
#endif
    public static readonly EDecimal Zero =
      EDecimal.Create(EInteger.Zero, EInteger.Zero);

    private const int MaxSafeInt = 214748363;

    private static readonly IRadixMath<EDecimal> ExtendedMathValue = new
      RadixMath<EDecimal>(new DecimalMathHelper());

private static readonly FastIntegerFixed FastIntZero = new
      FastIntegerFixed(0);
    //----------------------------------------------------------------
    private static readonly IRadixMath<EDecimal> MathValue = new
      TrappableRadixMath<EDecimal>(
        new ExtendedOrSimpleRadixMath<EDecimal>(new
                    DecimalMathHelper()));

    private static readonly int[] ValueTenPowers = {
      1, 10, 100, 1000, 10000, 100000,
      1000000, 10000000, 100000000,
      1000000000
    };

    private readonly FastIntegerFixed exponent;
    private readonly int flags;
    private readonly FastIntegerFixed unsignedMantissa;

    private int sign;

    private EDecimal(
      FastIntegerFixed unsignedMantissa,
      FastIntegerFixed exponent,
      int flags) {
#if DEBUG
      if (unsignedMantissa == null) {
        throw new ArgumentNullException(nameof(unsignedMantissa));
      }
      if (exponent == null) {
        throw new ArgumentNullException(nameof(exponent));
      }
      if (unsignedMantissa.Sign < 0) {
        throw new ArgumentException("unsignedMantissa is less than 0.");
      }
#endif
      this.unsignedMantissa = unsignedMantissa;
      this.exponent = exponent;
      this.flags = flags;
      this.sign = (((this.flags & BigNumberFlags.FlagSpecial) == 0) &&
                this.unsignedMantissa.IsValueZero) ? 0 : (((this.flags &
                    BigNumberFlags.FlagNegative) != 0) ? -1 : 1);
    }

    private EDecimal(
      FastIntegerFixed unsignedMantissa,
      FastIntegerFixed exponent,
      int flags,
      int sign) {
#if DEBUG
      if (unsignedMantissa == null) {
        throw new ArgumentNullException(nameof(unsignedMantissa));
      }
      if (exponent == null) {
        throw new ArgumentNullException(nameof(exponent));
      }
      if (unsignedMantissa.Sign < 0) {
        throw new ArgumentException("unsignedMantissa is less than 0.");
      }
#endif
      this.unsignedMantissa = unsignedMantissa;
      this.exponent = exponent;
      this.flags = flags;
      this.sign = sign;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.EDecimal.Exponent"]/*'/>
    public EInteger Exponent {
      get {
        return this.exponent.ToEInteger();
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.EDecimal.IsFinite"]/*'/>
    public bool IsFinite {
      get {
        return (this.flags & (BigNumberFlags.FlagInfinity |
                    BigNumberFlags.FlagNaN)) == 0;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.EDecimal.IsNegative"]/*'/>
    public bool IsNegative {
      get {
        return (this.flags & BigNumberFlags.FlagNegative) != 0;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.EDecimal.IsZero"]/*'/>
    public bool IsZero {
      get {
        return ((this.flags & BigNumberFlags.FlagSpecial) == 0) &&
          this.sign == 0;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.EDecimal.Mantissa"]/*'/>
    public EInteger Mantissa {
      get {
        return this.IsNegative ? this.unsignedMantissa.ToEInteger().Negate() :
                this.unsignedMantissa.ToEInteger();
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.EDecimal.Sign"]/*'/>
    public int Sign {
      get {
        return this.sign;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.EDecimal.UnsignedMantissa"]/*'/>
    public EInteger UnsignedMantissa {
      get {
        return this.unsignedMantissa.ToEInteger();
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Create(System.Int32,System.Int32)"]/*'/>
    public static EDecimal Create(int mantissaSmall, int exponentSmall) {
      if (mantissaSmall == Int32.MinValue) {
        return Create((EInteger)mantissaSmall, (EInteger)exponentSmall);
      } else if (mantissaSmall < 0) {
        return new EDecimal(
  new FastIntegerFixed(mantissaSmall).Negate(),
  new FastIntegerFixed(exponentSmall),
  BigNumberFlags.FlagNegative,
  -1);
      } else if (mantissaSmall == 0) {
   return new EDecimal(
  FastIntZero,
  new FastIntegerFixed(exponentSmall),
  0,
  0);
      } else {
        return new EDecimal(
  new FastIntegerFixed(mantissaSmall),
  new FastIntegerFixed(exponentSmall),
  0,
  1);
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Create(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger)"]/*'/>
    public static EDecimal Create(
      EInteger mantissa,
      EInteger exponent) {
      if (mantissa == null) {
        throw new ArgumentNullException(nameof(mantissa));
      }
      if (exponent == null) {
        throw new ArgumentNullException(nameof(exponent));
      }
      FastIntegerFixed fi = FastIntegerFixed.FromBig(mantissa);
      int sign = fi.Sign;
      return new EDecimal(
        sign < 0 ? fi.Negate() : fi,
        FastIntegerFixed.FromBig(exponent),
        (sign < 0) ? BigNumberFlags.FlagNegative : 0,
        sign);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.CreateNaN(PeterO.Numbers.EInteger)"]/*'/>
    public static EDecimal CreateNaN(EInteger diag) {
      return CreateNaN(diag, false, false, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.CreateNaN(PeterO.Numbers.EInteger,System.Boolean,System.Boolean,PeterO.Numbers.EContext)"]/*'/>
    public static EDecimal CreateNaN(
      EInteger diag,
      bool signaling,
      bool negative,
      EContext ctx) {
      if (diag == null) {
        throw new ArgumentNullException(nameof(diag));
      }
      if (diag.Sign < 0) {
        throw new
       ArgumentException("Diagnostic information must be 0 or greater, was: " +
                    diag);
      }
      if (diag.IsZero && !negative) {
        return signaling ? SignalingNaN : NaN;
      }
      var flags = 0;
      if (negative) {
        flags |= BigNumberFlags.FlagNegative;
      }
      if (ctx != null && ctx.HasMaxPrecision) {
        flags |= BigNumberFlags.FlagQuietNaN;
        var ef = new EDecimal(
        FastIntegerFixed.FromBig(diag),
        FastIntZero,
        flags,
        negative ? -1 : 1).RoundToPrecision(ctx);
        int newFlags = ef.flags;
        newFlags &= ~BigNumberFlags.FlagQuietNaN;
        newFlags |= signaling ? BigNumberFlags.FlagSignalingNaN :
          BigNumberFlags.FlagQuietNaN;
        return new EDecimal(
  ef.unsignedMantissa,
  ef.exponent,
  newFlags,
  negative ? -1 : 1);
      }
      flags |= signaling ? BigNumberFlags.FlagSignalingNaN :
        BigNumberFlags.FlagQuietNaN;
      return new EDecimal(
        FastIntegerFixed.FromBig(diag),
        FastIntZero,
        flags,
        negative ? -1 : 1);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.FromDouble(System.Double)"]/*'/>
    public static EDecimal FromDouble(double dbl) {
      int[] value = Extras.DoubleToIntegers(dbl);
      var floatExponent = (int)((value[1] >> 20) & 0x7ff);
      bool neg = (value[1] >> 31) != 0;
      long lvalue;
      if (floatExponent == 2047) {
        if ((value[1] & 0xfffff) == 0 && value[0] == 0) {
          return neg ? NegativeInfinity : PositiveInfinity;
        }
        // Treat high bit of mantissa as quiet/signaling bit
        bool quiet = (value[1] & 0x80000) != 0;
        value[1] &= 0x7ffff;
        lvalue = unchecked((value[0] & 0xffffffffL) | ((long)value[1] << 32));
        int flags = (neg ? BigNumberFlags.FlagNegative : 0) | (quiet ?
                BigNumberFlags.FlagQuietNaN : BigNumberFlags.FlagSignalingNaN);
        return lvalue == 0 ? (quiet ? NaN : SignalingNaN) :
          new EDecimal(
            FastIntegerFixed.FromLong(lvalue),
            FastIntZero,
            flags,
            neg ? -1 : 1);
      }
      value[1] &= 0xfffff;

      // Mask out the exponent and sign
      if (floatExponent == 0) {
        ++floatExponent;
      } else {
        value[1] |= 0x100000;
      }
      if ((value[1] | value[0]) != 0) {
        floatExponent += NumberUtility.ShiftAwayTrailingZerosTwoElements(value);
      } else {
        return neg ? EDecimal.NegativeZero : EDecimal.Zero;
      }
      floatExponent -= 1075;
      lvalue = unchecked((value[0] & 0xffffffffL) | ((long)value[1] << 32));
      if (floatExponent == 0) {
        if (neg) {
          lvalue = -lvalue;
        }
        return EDecimal.FromInt64(lvalue);
      }
      if (floatExponent > 0) {
        // Value is an integer
        var bigmantissa = (EInteger)lvalue;
        bigmantissa <<= floatExponent;
        if (neg) {
          bigmantissa = -(EInteger)bigmantissa;
        }
        return EDecimal.FromEInteger(bigmantissa);
      } else {
        // Value has a fractional part
        var bigmantissa = (EInteger)lvalue;
        EInteger exp = NumberUtility.FindPowerOfFive(-floatExponent);
        bigmantissa *= (EInteger)exp;
        if (neg) {
          bigmantissa = -(EInteger)bigmantissa;
        }
        return EDecimal.Create(bigmantissa, (EInteger)floatExponent);
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.FromEInteger(PeterO.Numbers.EInteger)"]/*'/>
    public static EDecimal FromEInteger(EInteger bigint) {
      return EDecimal.Create(bigint, EInteger.Zero);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.FromExtendedFloat(PeterO.Numbers.EFloat)"]/*'/>
    [Obsolete("Renamed to FromEFloat.")]
    public static EDecimal FromExtendedFloat(EFloat ef) {
      return FromEFloat(ef);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.FromEFloat(PeterO.Numbers.EFloat)"]/*'/>
    public static EDecimal FromEFloat(EFloat bigfloat) {
      if (bigfloat == null) {
        throw new ArgumentNullException(nameof(bigfloat));
      }
      if (bigfloat.IsNaN() || bigfloat.IsInfinity()) {
        int flags = (bigfloat.IsNegative ? BigNumberFlags.FlagNegative : 0) |
          (bigfloat.IsInfinity() ? BigNumberFlags.FlagInfinity : 0) |
          (bigfloat.IsQuietNaN() ? BigNumberFlags.FlagQuietNaN : 0) |
          (bigfloat.IsSignalingNaN() ? BigNumberFlags.FlagSignalingNaN : 0);
        return CreateWithFlags(
          bigfloat.UnsignedMantissa,
          bigfloat.Exponent,
          flags);
      }
      EInteger bigintExp = bigfloat.Exponent;
      EInteger bigintMant = bigfloat.Mantissa;
      if (bigintMant.IsZero) {
        return bigfloat.IsNegative ? EDecimal.NegativeZero :
          EDecimal.Zero;
      }
      if (bigintExp.IsZero) {
        // Integer
        return EDecimal.FromEInteger(bigintMant);
      }
      if (bigintExp.Sign > 0) {
        // Scaled integer
        FastInteger intcurexp = FastInteger.FromBig(bigintExp);
        EInteger bigmantissa = bigintMant;
        bool neg = bigmantissa.Sign < 0;
        if (neg) {
          bigmantissa = -(EInteger)bigmantissa;
        }
        while (intcurexp.Sign > 0) {
          var shift = 1000000;
          if (intcurexp.CompareToInt(1000000) < 0) {
            shift = intcurexp.AsInt32();
          }
          bigmantissa <<= shift;
          intcurexp.AddInt(-shift);
        }
        if (neg) {
          bigmantissa = -(EInteger)bigmantissa;
        }
        return EDecimal.FromEInteger(bigmantissa);
      } else {
        // Fractional number
        EInteger bigmantissa = bigintMant;
        EInteger negbigintExp = -(EInteger)bigintExp;
        negbigintExp = NumberUtility.FindPowerOfFiveFromBig(negbigintExp);
        bigmantissa *= (EInteger)negbigintExp;
        return EDecimal.Create(bigmantissa, bigintExp);
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.FromInt32(System.Int32)"]/*'/>
    public static EDecimal FromInt32(int valueSmaller) {
      if (valueSmaller == 0) {
        return EDecimal.Zero;
      }
      if (valueSmaller == Int32.MinValue) {
        return Create((EInteger)valueSmaller, EInteger.Zero);
      }
      if (valueSmaller < 0) {
        return new EDecimal(
  new FastIntegerFixed(valueSmaller).Negate(),
  FastIntZero,
  BigNumberFlags.FlagNegative,
  -1);
      } else {
        return new
            EDecimal(new FastIntegerFixed(valueSmaller), FastIntZero, 0, 1);
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.FromInt64(System.Int64)"]/*'/>
    public static EDecimal FromInt64(long valueSmall) {
      if (valueSmall == 0) {
        return EDecimal.Zero;
      }
      if (valueSmall > Int32.MinValue && valueSmall <= Int32.MaxValue) {
        if (valueSmall < 0) {
          return new EDecimal(
  new FastIntegerFixed((int)valueSmall).Negate(),
  FastIntZero,
  BigNumberFlags.FlagNegative,
  -1);
        } else {
          return new EDecimal(
  new FastIntegerFixed((int)valueSmall),
  FastIntZero,
  0,
  1);
        }
      }
      var bigint = (EInteger)valueSmall;
      return EDecimal.Create(bigint, EInteger.Zero);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.FromSingle(System.Single)"]/*'/>
    public static EDecimal FromSingle(float flt) {
      int value = BitConverter.ToInt32(BitConverter.GetBytes((float)flt), 0);
      bool neg = (value >> 31) != 0;
      var floatExponent = (int)((value >> 23) & 0xff);
      int valueFpMantissa = value & 0x7fffff;
      if (floatExponent == 255) {
        if (valueFpMantissa == 0) {
          return neg ? NegativeInfinity : PositiveInfinity;
        }
        // Treat high bit of mantissa as quiet/signaling bit
        bool quiet = (valueFpMantissa & 0x400000) != 0;
        valueFpMantissa &= 0x3fffff;
        value = (neg ? BigNumberFlags.FlagNegative : 0) |
       (quiet ? BigNumberFlags.FlagQuietNaN : BigNumberFlags.FlagSignalingNaN);
        return valueFpMantissa == 0 ? (quiet ? NaN : SignalingNaN) :
          new EDecimal(
            new FastIntegerFixed(valueFpMantissa),
            FastIntZero,
            value,
            neg ? -1 : 1);
      }
      if (floatExponent == 0) {
        ++floatExponent;
      } else {
        valueFpMantissa |= 1 << 23;
      }
      if (valueFpMantissa == 0) {
        return neg ? EDecimal.NegativeZero : EDecimal.Zero;
      }
      floatExponent -= 150;
      while ((valueFpMantissa & 1) == 0) {
        ++floatExponent;
        valueFpMantissa >>= 1;
      }
      if (floatExponent == 0) {
        if (neg) {
          valueFpMantissa = -valueFpMantissa;
        }
        return EDecimal.FromInt64(valueFpMantissa);
      }
      if (floatExponent > 0) {
        // Value is an integer
        var bigmantissa = (EInteger)valueFpMantissa;
        bigmantissa <<= floatExponent;
        if (neg) {
          bigmantissa = -(EInteger)bigmantissa;
        }
        return EDecimal.FromEInteger(bigmantissa);
      } else {
        // Value has a fractional part
        var bigmantissa = (EInteger)valueFpMantissa;
        EInteger bigexponent = NumberUtility.FindPowerOfFive(-floatExponent);
        bigmantissa *= (EInteger)bigexponent;
        if (neg) {
          bigmantissa = -(EInteger)bigmantissa;
        }
        return EDecimal.Create(bigmantissa, (EInteger)floatExponent);
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.FromString(System.String)"]/*'/>
    public static EDecimal FromString(string str) {
      return FromString(str, 0, str == null ? 0 : str.Length, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.FromString(System.String,PeterO.Numbers.EContext)"]/*'/>
    public static EDecimal FromString(string str, EContext ctx) {
      return FromString(str, 0, str == null ? 0 : str.Length, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.FromString(System.String,System.Int32,System.Int32)"]/*'/>
    public static EDecimal FromString(
      string str,
      int offset,
      int length) {
      return FromString(str, offset, length, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.FromString(System.String,System.Int32,System.Int32,PeterO.Numbers.EContext)"]/*'/>
    public static EDecimal FromString(
      string str,
      int offset,
      int length,
      EContext ctx) {
      int tmpoffset = offset;
      if (str == null) {
        throw new ArgumentNullException(nameof(str));
      }
      if (tmpoffset < 0) {
        throw new FormatException("offset (" + tmpoffset + ") is less than " +
                    "0");
      }
      if (tmpoffset > str.Length) {
        throw new FormatException("offset (" + tmpoffset + ") is more than " +
                    str.Length);
      }
      if (length < 0) {
        throw new FormatException("length (" + length + ") is less than " +
                    "0");
      }
      if (length > str.Length) {
        throw new FormatException("length (" + length + ") is more than " +
                    str.Length);
      }
      if (str.Length - tmpoffset < length) {
        throw new FormatException("str's length minus " + tmpoffset + " (" +
                    (str.Length - tmpoffset) + ") is less than " + length);
      }
      if (length == 0) {
        throw new FormatException();
      }
      var negative = false;
      int endStr = tmpoffset + length;
      if (str[0] == '+' || str[0] == '-') {
        negative = str[0] == '-';
        ++tmpoffset;
      }
      var mantInt = 0;
      FastInteger mant = null;
      var mantBuffer = 0;
      var mantBufferMult = 1;
      var expBuffer = 0;
      var expBufferMult = 1;
      var haveDecimalPoint = false;
      var haveDigits = false;
      var haveExponent = false;
      var newScaleInt = 0;
      FastInteger newScale = null;
      int i = tmpoffset;
      if (i + 8 == endStr) {
        if ((str[i] == 'I' || str[i] == 'i') &&
            (str[i + 1] == 'N' || str[i + 1] == 'n') &&
            (str[i + 2] == 'F' || str[i + 2] == 'f') &&
            (str[i + 3] == 'I' || str[i + 3] == 'i') && (str[i + 4] == 'N' ||
                    str[i + 4] == 'n') && (str[i + 5] ==
                    'I' || str[i + 5] == 'i') &&
            (str[i + 6] == 'T' || str[i + 6] == 't') && (str[i + 7] == 'Y' ||
                    str[i + 7] == 'y')) {
          if (ctx != null && ctx.IsSimplified && i < endStr) {
            throw new FormatException("Infinity not allowed");
          }
          return negative ? NegativeInfinity : PositiveInfinity;
        }
      }
      if (i + 3 == endStr) {
        if ((str[i] == 'I' || str[i] == 'i') &&
            (str[i + 1] == 'N' || str[i + 1] == 'n') && (str[i + 2] == 'F' ||
                    str[i + 2] == 'f')) {
          if (ctx != null && ctx.IsSimplified && i < endStr) {
            throw new FormatException("Infinity not allowed");
          }
          return negative ? NegativeInfinity : PositiveInfinity;
        }
      }
      if (i + 3 <= endStr) {
        // Quiet NaN
        if ((str[i] == 'N' || str[i] == 'n') && (str[i + 1] == 'A' || str[i +
                1] == 'a') && (str[i + 2] == 'N' || str[i + 2] == 'n')) {
          if (ctx != null && ctx.IsSimplified && i < endStr) {
            throw new FormatException("NaN not allowed");
          }
          int flags2 = (negative ? BigNumberFlags.FlagNegative : 0) |
            BigNumberFlags.FlagQuietNaN;
          if (i + 3 == endStr) {
            return (!negative) ? NaN : new EDecimal(
              FastIntZero,
              FastIntZero,
              flags2,
              -1);
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
          for (; i < endStr; ++i) {
            if (str[i] >= '0' && str[i] <= '9') {
              var thisdigit = (int)(str[i] - '0');
              haveDigits = haveDigits || thisdigit != 0;
              if (mantInt > MaxSafeInt) {
                if (mant == null) {
                  mant = new FastInteger(mantInt);
                  mantBuffer = thisdigit;
                  mantBufferMult = 10;
                } else {
                  if (mantBufferMult >= 1000000000) {
                    mant.Multiply(mantBufferMult).AddInt(mantBuffer);
                    mantBuffer = thisdigit;
                    mantBufferMult = 10;
                  } else {
                    // multiply by 10
   mantBufferMult = (mantBufferMult << 3) + (mantBufferMult << 1);
                    mantBuffer = (mantBuffer << 3) + (mantBuffer << 1);
                    mantBuffer += thisdigit;
                  }
                }
              } else {
                // multiply by 10
   mantInt = (mantInt << 3) + (mantInt << 1);
                mantInt += thisdigit;
              }
              if (haveDigits && maxDigits != null) {
                digitCount.Increment();
                if (digitCount.CompareTo(maxDigits) > 0) {
                  // NaN contains too many digits
                  throw new FormatException();
                }
              }
            } else {
              throw new FormatException();
            }
          }
          if (mant != null && (mantBufferMult != 1 || mantBuffer != 0)) {
            mant.Multiply(mantBufferMult).AddInt(mantBuffer);
          }
          EInteger bigmant = (mant == null) ? ((EInteger)mantInt) :
            mant.AsEInteger();
          flags2 = (negative ? BigNumberFlags.FlagNegative : 0) |
            BigNumberFlags.FlagQuietNaN;
          return CreateWithFlags(
            FastIntegerFixed.FromBig(bigmant),
            FastIntZero,
            flags2);
        }
      }
      if (i + 4 <= endStr) {
        // Signaling NaN
        if ((str[i] == 'S' || str[i] == 's') && (str[i + 1] == 'N' || str[i +
                    1] == 'n') && (str[i + 2] == 'A' || str[i + 2] == 'a') &&
                (str[i + 3] == 'N' || str[i + 3] == 'n')) {
          if (ctx != null && ctx.IsSimplified && i < endStr) {
            throw new FormatException("NaN not allowed");
          }
          if (i + 4 == endStr) {
            int flags2 = (negative ? BigNumberFlags.FlagNegative : 0) |
              BigNumberFlags.FlagSignalingNaN;
            return (!negative) ? SignalingNaN :
              new EDecimal(
                FastIntZero,
                FastIntZero,
                flags2,
                -1);
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
          for (; i < endStr; ++i) {
            if (str[i] >= '0' && str[i] <= '9') {
              var thisdigit = (int)(str[i] - '0');
              haveDigits = haveDigits || thisdigit != 0;
              if (mantInt > MaxSafeInt) {
                if (mant == null) {
                  mant = new FastInteger(mantInt);
                  mantBuffer = thisdigit;
                  mantBufferMult = 10;
                } else {
                  if (mantBufferMult >= 1000000000) {
                    mant.Multiply(mantBufferMult).AddInt(mantBuffer);
                    mantBuffer = thisdigit;
                    mantBufferMult = 10;
                  } else {
                    // multiply by 10
   mantBufferMult = (mantBufferMult << 3) + (mantBufferMult << 1);
                    mantBuffer = (mantBuffer << 3) + (mantBuffer << 1);
                    mantBuffer += thisdigit;
                  }
                }
              } else {
                // multiply by 10
   mantInt = (mantInt << 3) + (mantInt << 1);
                mantInt += thisdigit;
              }
              if (haveDigits && maxDigits != null) {
                digitCount.Increment();
                if (digitCount.CompareTo(maxDigits) > 0) {
                  // NaN contains too many digits
                  throw new FormatException();
                }
              }
            } else {
              throw new FormatException();
            }
          }
          if (mant != null && (mantBufferMult != 1 || mantBuffer != 0)) {
            mant.Multiply(mantBufferMult).AddInt(mantBuffer);
          }
          int flags3 = (negative ? BigNumberFlags.FlagNegative : 0) |
            BigNumberFlags.FlagSignalingNaN;
          EInteger bigmant = (mant == null) ? ((EInteger)mantInt) :
            mant.AsEInteger();
          return CreateWithFlags(
            bigmant,
            EInteger.Zero,
            flags3);
        }
      }
      // Ordinary number
      for (; i < endStr; ++i) {
        char ch = str[i];
        if (ch >= '0' && ch <= '9') {
          var thisdigit = (int)(ch - '0');
          if (mantInt > MaxSafeInt) {
            if (mant == null) {
              mant = new FastInteger(mantInt);
              mantBuffer = thisdigit;
              mantBufferMult = 10;
            } else {
              if (mantBufferMult >= 1000000000) {
                mant.Multiply(mantBufferMult).AddInt(mantBuffer);
                mantBuffer = thisdigit;
                mantBufferMult = 10;
              } else {
                // multiply mantBufferMult and mantBuffer each by 10
                mantBufferMult = (mantBufferMult << 3) + (mantBufferMult << 1);
                mantBuffer = (mantBuffer << 3) + (mantBuffer << 1);
                mantBuffer += thisdigit;
              }
            }
          } else {
            // multiply by 10
   mantInt = (mantInt << 3) + (mantInt << 1);
            mantInt += thisdigit;
          }
          haveDigits = true;
          if (haveDecimalPoint) {
            if (newScaleInt == Int32.MinValue) {
newScale = newScale ?? (new FastInteger(newScaleInt));
              newScale.Decrement();
            } else {
              --newScaleInt;
            }
          }
        } else if (ch == '.') {
          if (haveDecimalPoint) {
            throw new FormatException();
          }
          haveDecimalPoint = true;
        } else if (ch == 'E' || ch == 'e') {
          haveExponent = true;
          ++i;
          break;
        } else {
          throw new FormatException();
        }
      }
      if (!haveDigits) {
        throw new FormatException();
      }
      if (mant != null && (mantBufferMult != 1 || mantBuffer != 0)) {
        mant.Multiply(mantBufferMult).AddInt(mantBuffer);
      }
      if (haveExponent) {
        FastInteger exp = null;
        var expInt = 0;
        tmpoffset = 1;
        haveDigits = false;
        if (i == endStr) {
          throw new FormatException();
        }
        if (str[i] == '+' || str[i] == '-') {
          if (str[i] == '-') {
            tmpoffset = -1;
          }
          ++i;
        }
        for (; i < endStr; ++i) {
          char ch = str[i];
          if (ch >= '0' && ch <= '9') {
            haveDigits = true;
            var thisdigit = (int)(ch - '0');
            if (expInt > MaxSafeInt) {
              if (exp == null) {
                exp = new FastInteger(expInt);
                expBuffer = thisdigit;
                expBufferMult = 10;
              } else {
                if (expBufferMult >= 1000000000) {
                  exp.Multiply(expBufferMult).AddInt(expBuffer);
                  expBuffer = thisdigit;
                  expBufferMult = 10;
                } else {
                  // multiply expBufferMult and expBuffer each by 10
                  expBufferMult = (expBufferMult << 3) + (expBufferMult << 1);
                  expBuffer = (expBuffer << 3) + (expBuffer << 1);
                  expBuffer += thisdigit;
                }
              }
            } else {
              expInt *= 10;
              expInt += thisdigit;
            }
          } else {
            throw new FormatException();
          }
        }
        if (!haveDigits) {
          throw new FormatException();
        }
        if (exp != null && (expBufferMult != 1 || expBuffer != 0)) {
          exp.Multiply(expBufferMult).AddInt(expBuffer);
        }
   if (tmpoffset >= 0 && newScaleInt == 0 && newScale == null && exp == null) {
          newScaleInt = expInt;
        } else if (exp == null) {
newScale = newScale ?? (new FastInteger(newScaleInt));
          if (tmpoffset < 0) {
            newScale.SubtractInt(expInt);
          } else if (expInt != 0) {
            newScale.AddInt(expInt);
          }
        } else {
newScale = newScale ?? (new FastInteger(newScaleInt));
          if (tmpoffset < 0) {
            newScale.Subtract(exp);
          } else {
            newScale.Add(exp);
          }
        }
      }
      if (i != endStr) {
        throw new FormatException();
      }
      FastIntegerFixed fastIntScale;
      FastIntegerFixed fastIntMant;
      fastIntScale = (newScale == null) ? (new FastIntegerFixed(newScaleInt)) :
        FastIntegerFixed.FromFastInteger(newScale);
      int sign = negative ? -1 : 1;
      if (mant == null) {
        fastIntMant = new FastIntegerFixed(mantInt);
        if (mantInt == 0) {
 sign = 0;
}
      } else if (mant.CanFitInInt32()) {
        mantInt = mant.AsInt32();
        fastIntMant = new FastIntegerFixed(mantInt);
        if (mantInt == 0) {
 sign = 0;
}
      } else {
        fastIntMant = FastIntegerFixed.FromFastInteger(mant);
      }
      var ret = new EDecimal(
  fastIntMant,
  fastIntScale,
  negative ? BigNumberFlags.FlagNegative : 0,
  sign);
      if (ctx != null) {
        ret = GetMathValue(ctx).RoundAfterConversion(ret, ctx);
      }
      return ret;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Max(PeterO.Numbers.EDecimal,PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public static EDecimal Max(
      EDecimal first,
      EDecimal second,
      EContext ctx) {
      return GetMathValue(ctx).Max(first, second, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Max(PeterO.Numbers.EDecimal,PeterO.Numbers.EDecimal)"]/*'/>
    public static EDecimal Max(
      EDecimal first,
      EDecimal second) {
      return Max(first, second, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.MaxMagnitude(PeterO.Numbers.EDecimal,PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public static EDecimal MaxMagnitude(
      EDecimal first,
      EDecimal second,
      EContext ctx) {
      return GetMathValue(ctx).MaxMagnitude(first, second, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.MaxMagnitude(PeterO.Numbers.EDecimal,PeterO.Numbers.EDecimal)"]/*'/>
    public static EDecimal MaxMagnitude(
      EDecimal first,
      EDecimal second) {
      return MaxMagnitude(first, second, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Min(PeterO.Numbers.EDecimal,PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public static EDecimal Min(
      EDecimal first,
      EDecimal second,
      EContext ctx) {
      return GetMathValue(ctx).Min(first, second, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Min(PeterO.Numbers.EDecimal,PeterO.Numbers.EDecimal)"]/*'/>
    public static EDecimal Min(
      EDecimal first,
      EDecimal second) {
      return Min(first, second, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.MinMagnitude(PeterO.Numbers.EDecimal,PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public static EDecimal MinMagnitude(
      EDecimal first,
      EDecimal second,
      EContext ctx) {
      return GetMathValue(ctx).MinMagnitude(first, second, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.MinMagnitude(PeterO.Numbers.EDecimal,PeterO.Numbers.EDecimal)"]/*'/>
    public static EDecimal MinMagnitude(
      EDecimal first,
      EDecimal second) {
      return MinMagnitude(first, second, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.PI(PeterO.Numbers.EContext)"]/*'/>
    public static EDecimal PI(EContext ctx) {
      return GetMathValue(ctx).Pi(ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Abs"]/*'/>
    public EDecimal Abs() {
      if (this.IsNegative) {
        var er = new EDecimal(
  this.unsignedMantissa,
  this.exponent,
  this.flags & ~BigNumberFlags.FlagNegative,
  Math.Abs(this.sign));
        return er;
      }
      return this;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.CopySign(PeterO.Numbers.EDecimal)"]/*'/>
    public EDecimal CopySign(EDecimal other) {
      if (other == null) {
        throw new ArgumentNullException(nameof(other));
      }
      if (this.IsNegative) {
        return other.IsNegative ? this : this.Negate();
      } else {
        return other.IsNegative ? this.Negate() : this;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Abs(PeterO.Numbers.EContext)"]/*'/>
    public EDecimal Abs(EContext context) {
      return ((context == null || context == EContext.UnlimitedHalfEven) ?
        ExtendedMathValue : MathValue).Abs(this, context);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Add(PeterO.Numbers.EDecimal)"]/*'/>
    public EDecimal Add(EDecimal otherValue) {
      if (this.IsFinite && otherValue != null && otherValue.IsFinite &&
        ((this.flags | otherValue.flags) & BigNumberFlags.FlagNegative) == 0 &&
            this.exponent.CompareTo(otherValue.exponent) == 0) {
        FastIntegerFixed result = FastIntegerFixed.Add(
  this.unsignedMantissa,
  otherValue.unsignedMantissa);
        int sign = result.IsValueZero ? 0 : 1;
        return new EDecimal(result, this.exponent, 0, sign);
      }
      return this.Add(otherValue, EContext.UnlimitedHalfEven);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Add(PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal Add(
      EDecimal otherValue,
      EContext ctx) {
      return GetMathValue(ctx).Add(this, otherValue, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.CompareTo(PeterO.Numbers.EDecimal)"]/*'/>
    public int CompareTo(EDecimal other) {
      return ExtendedMathValue.CompareTo(this, other);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.CompareToBinary(PeterO.Numbers.EFloat)"]/*'/>
    public int CompareToBinary(EFloat other) {
      if (other == null) {
        return 1;
      }
      if (this.IsNaN()) {
        return other.IsNaN() ? 0 : 1;
      }
      int signA = this.Sign;
      int signB = other.Sign;
      if (signA != signB) {
        return (signA < signB) ? -1 : 1;
      }
      if (signB == 0 || signA == 0) {
        // Special case: Either operand is zero
        return 0;
      }
      if (this.IsInfinity()) {
        if (other.IsInfinity()) {
          // if we get here, this only means that
          // both are positive infinity or both
          // are negative infinity
          return 0;
        }
        return this.IsNegative ? -1 : 1;
      }
      if (other.IsInfinity()) {
        return other.IsNegative ? 1 : -1;
      }
      // At this point, both numbers are finite and
      // have the same sign
#if DEBUG
      if (!this.IsFinite) {
        throw new ArgumentException("doesn't satisfy this.IsFinite");
      }
      if (!other.IsFinite) {
        throw new ArgumentException("doesn't satisfy other.IsFinite");
      }
#endif
      if (other.Exponent.CompareTo((EInteger)(-1000)) < 0) {
        // For very low exponents, the conversion to decimal can take
        // very long, so try this approach
        if (other.Abs(null).CompareTo(EFloat.One) < 0) {
          // Abs less than 1
          if (this.Abs(null).CompareTo(EDecimal.One) >= 0) {
            // Abs 1 or more
            return (signA > 0) ? 1 : -1;
          }
        }
      }
      if (other.Exponent.CompareTo((EInteger)1000) > 0) {
        // Very high exponents
        EInteger bignum = EInteger.One.ShiftLeft(999);
        if (this.Abs(null).CompareTo(EDecimal.FromEInteger(bignum)) <=
            0) {
          // this object's absolute value is less
          return (signA > 0) ? -1 : 1;
        }
        // NOTE: The following check assumes that both
        // operands are nonzero
        EInteger thisAdjExp = this.GetAdjustedExponent();
        EInteger otherAdjExp = GetAdjustedExponentBinary(other);
        if (thisAdjExp.Sign > 0 && thisAdjExp.CompareTo(otherAdjExp) >= 0) {
          // This object's adjusted exponent is greater and is positive;
          // so this object's absolute value is greater, since exponents
          // have a greater value in decimal than in binary
          return (signA > 0) ? 1 : -1;
        }
        if (thisAdjExp.Sign > 0 && thisAdjExp.CompareTo((EInteger)1000) >= 0 &&
                otherAdjExp.CompareTo((EInteger)1000) >= 0) {
          thisAdjExp = thisAdjExp.Add(EInteger.One);
          otherAdjExp = thisAdjExp.Add(EInteger.One);
          EInteger ratio = otherAdjExp / thisAdjExp;
          // Check the ratio of the binary exponent to the decimal exponent.
          // If the ratio is less than 3, the decimal's absolute value is
          // greater. If it's 4 or greater, the binary' s absolute value is
          // greater.
          // (If the two absolute values are equal, the ratio will approach
          // ln(10)/ln(2), or about 3.322, as the exponents get higher and
          // higher.) This check assumes that both exponents are 1000 or
          // greater,
          // when the ratio between exponents of equal values is close to
          // ln(10)/ln(2).
          if (ratio.CompareTo((EInteger)3) < 0) {
            // Decimal abs. value is greater
            return (signA > 0) ? 1 : -1;
          }
          if (ratio.CompareTo((EInteger)4) >= 0) {
            return (signA > 0) ? -1 : 1;
          }
        }
      }
      EDecimal otherDec = EDecimal.FromEFloat(other);
      return this.CompareTo(otherDec);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.CompareToSignal(PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal CompareToSignal(
      EDecimal other,
      EContext ctx) {
      return GetMathValue(ctx).CompareToWithContext(this, other, true, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.CompareToTotalMagnitude(PeterO.Numbers.EDecimal)"]/*'/>
    public int CompareToTotalMagnitude(EDecimal other) {
      if (other == null) {
        return -1;
      }
      var valueIThis = 0;
      var valueIOther = 0;
      int cmp;
      if (this.IsSignalingNaN()) {
        valueIThis = 2;
      } else if (this.IsNaN()) {
        valueIThis = 3;
      } else if (this.IsInfinity()) {
        valueIThis = 1;
      }
      if (other.IsSignalingNaN()) {
        valueIOther = 2;
      } else if (other.IsNaN()) {
        valueIOther = 3;
      } else if (other.IsInfinity()) {
        valueIOther = 1;
      }
      if (valueIThis > valueIOther) {
        return 1;
      } else if (valueIThis < valueIOther) {
        return -1;
      }
      if (valueIThis >= 2) {
        cmp = this.unsignedMantissa.CompareTo(
         other.unsignedMantissa);
        return cmp;
      } else if (valueIThis == 1) {
        return 0;
      } else {
        cmp = this.Abs().CompareTo(other.Abs());
        if (cmp == 0) {
          cmp = this.exponent.CompareTo(
           other.exponent);
          return cmp;
        }
        return cmp;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.CompareToTotal(PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public int CompareToTotal(EDecimal other, EContext ctx) {
      if (other == null) {
        return -1;
      }
      if (this.IsSignalingNaN() || other.IsSignalingNaN()) {
        return this.CompareToTotal(other);
      }
      if (ctx != null && ctx.IsSimplified) {
        return this.RoundToPrecision(ctx)
          .CompareToTotal(other.RoundToPrecision(ctx));
      } else {
        return this.CompareToTotal(other);
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.CompareToTotal(PeterO.Numbers.EDecimal)"]/*'/>
    public int CompareToTotal(EDecimal other) {
      if (other == null) {
        return -1;
      }
      bool neg1 = this.IsNegative;
      bool neg2 = other.IsNegative;
      if (neg1 != neg2) {
        return neg1 ? -1 : 1;
      }
      var valueIThis = 0;
      var valueIOther = 0;
      int cmp;
      if (this.IsSignalingNaN()) {
        valueIThis = 2;
      } else if (this.IsNaN()) {
        valueIThis = 3;
      } else if (this.IsInfinity()) {
        valueIThis = 1;
      }
      if (other.IsSignalingNaN()) {
        valueIOther = 2;
      } else if (other.IsNaN()) {
        valueIOther = 3;
      } else if (other.IsInfinity()) {
        valueIOther = 1;
      }
      if (valueIThis > valueIOther) {
        return neg1 ? -1 : 1;
      } else if (valueIThis < valueIOther) {
        return neg1 ? 1 : -1;
      }
      if (valueIThis >= 2) {
        cmp = this.unsignedMantissa.CompareTo(
         other.unsignedMantissa);
        return neg1 ? -cmp : cmp;
      } else if (valueIThis == 1) {
        return 0;
      } else {
        cmp = this.CompareTo(other);
        if (cmp == 0) {
          cmp = this.exponent.CompareTo(
           other.exponent);
          return neg1 ? -cmp : cmp;
        }
        return cmp;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.CompareToWithContext(PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal CompareToWithContext(
      EDecimal other,
      EContext ctx) {
      return GetMathValue(ctx).CompareToWithContext(this, other, false, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Divide(PeterO.Numbers.EDecimal)"]/*'/>
    public EDecimal Divide(EDecimal divisor) {
      return this.Divide(
        divisor,
        EContext.ForRounding(ERounding.None));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Divide(PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal Divide(
      EDecimal divisor,
      EContext ctx) {
      return GetMathValue(ctx).Divide(this, divisor, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DivideAndRemainderNaturalScale(PeterO.Numbers.EDecimal)"]/*'/>
    [Obsolete("Renamed to DivRemNaturalScale.")]
    public EDecimal[] DivideAndRemainderNaturalScale(EDecimal
      divisor) {
      return this.DivRemNaturalScale(divisor, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DivideAndRemainderNaturalScale(PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    [Obsolete("Renamed to DivRemNaturalScale.")]
    public EDecimal[] DivideAndRemainderNaturalScale(
      EDecimal divisor,
      EContext ctx) {
      return this.DivRemNaturalScale(divisor, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DivRemNaturalScale(PeterO.Numbers.EDecimal)"]/*'/>
    public EDecimal[] DivRemNaturalScale(EDecimal
      divisor) {
      return this.DivRemNaturalScale(divisor, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DivRemNaturalScale(PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal[] DivRemNaturalScale(
      EDecimal divisor,
      EContext ctx) {
      var result = new EDecimal[2];
      result[0] = this.DivideToIntegerNaturalScale(divisor, null);
      result[1] = this.Subtract(
        result[0].Multiply(divisor, null),
        ctx);
      result[0] = result[0].RoundToPrecision(ctx);
      return result;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DivideToExponent(PeterO.Numbers.EDecimal,System.Int64,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal DivideToExponent(
      EDecimal divisor,
      long desiredExponentSmall,
      EContext ctx) {
      return this.DivideToExponent(
        divisor,
        (EInteger)desiredExponentSmall,
        ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DivideToExponent(PeterO.Numbers.EDecimal,System.Int32,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal DivideToExponent(
      EDecimal divisor,
      int desiredExponentInt,
      EContext ctx) {
      return this.DivideToExponent(
        divisor,
        (EInteger)desiredExponentInt,
        ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DivideToExponent(PeterO.Numbers.EDecimal,System.Int64,PeterO.Numbers.ERounding)"]/*'/>
    public EDecimal DivideToExponent(
      EDecimal divisor,
      long desiredExponentSmall,
      ERounding rounding) {
      return this.DivideToExponent(
        divisor,
        (EInteger)desiredExponentSmall,
        EContext.ForRounding(rounding));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DivideToExponent(PeterO.Numbers.EDecimal,System.Int32,PeterO.Numbers.ERounding)"]/*'/>
    public EDecimal DivideToExponent(
      EDecimal divisor,
      int desiredExponentInt,
      ERounding rounding) {
      return this.DivideToExponent(
        divisor,
        (EInteger)desiredExponentInt,
        EContext.ForRounding(rounding));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DivideToExponent(PeterO.Numbers.EDecimal,PeterO.Numbers.EInteger,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal DivideToExponent(
      EDecimal divisor,
      EInteger exponent,
      EContext ctx) {
      return GetMathValue(ctx).DivideToExponent(this, divisor, exponent, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DivideToExponent(PeterO.Numbers.EDecimal,PeterO.Numbers.EInteger)"]/*'/>
    public EDecimal DivideToExponent(
      EDecimal divisor,
      EInteger exponent) {
      return this.DivideToExponent(divisor, exponent, ERounding.HalfEven);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DivideToExponent(PeterO.Numbers.EDecimal,System.Int64)"]/*'/>
    public EDecimal DivideToExponent(
      EDecimal divisor,
      long desiredExponentSmall) {
      return this.DivideToExponent(
  divisor,
  desiredExponentSmall,
  ERounding.HalfEven);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DivideToExponent(PeterO.Numbers.EDecimal,System.Int32)"]/*'/>
    public EDecimal DivideToExponent(
      EDecimal divisor,
      int desiredExponentInt) {
 return this.DivideToExponent(
  divisor,
  desiredExponentInt,
  ERounding.HalfEven);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DivideToExponent(PeterO.Numbers.EDecimal,PeterO.Numbers.EInteger,PeterO.Numbers.ERounding)"]/*'/>
    public EDecimal DivideToExponent(
      EDecimal divisor,
      EInteger desiredExponent,
      ERounding rounding) {
      return this.DivideToExponent(
        divisor,
        desiredExponent,
        EContext.ForRounding(rounding));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DivideToIntegerNaturalScale(PeterO.Numbers.EDecimal)"]/*'/>
    public EDecimal DivideToIntegerNaturalScale(EDecimal
                    divisor) {
      return this.DivideToIntegerNaturalScale(
        divisor,
        EContext.ForRounding(ERounding.Down));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DivideToIntegerNaturalScale(PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal DivideToIntegerNaturalScale(
      EDecimal divisor,
      EContext ctx) {
      return GetMathValue(ctx).DivideToIntegerNaturalScale(this, divisor, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DivideToIntegerZeroScale(PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal DivideToIntegerZeroScale(
      EDecimal divisor,
      EContext ctx) {
      return GetMathValue(ctx).DivideToIntegerZeroScale(this, divisor, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DivideToSameExponent(PeterO.Numbers.EDecimal,PeterO.Numbers.ERounding)"]/*'/>
    public EDecimal DivideToSameExponent(
      EDecimal divisor,
      ERounding rounding) {
      return this.DivideToExponent(
        divisor,
        this.exponent.ToEInteger(),
        EContext.ForRounding(rounding));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Equals(PeterO.Numbers.EDecimal)"]/*'/>
    public bool Equals(EDecimal other) {
      return this.EqualsInternal(other);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Equals(System.Object)"]/*'/>
    public override bool Equals(object obj) {
      return this.EqualsInternal(obj as EDecimal);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Exp(PeterO.Numbers.EContext)"]/*'/>
    public EDecimal Exp(EContext ctx) {
      return GetMathValue(ctx).Exp(this, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.GetHashCode"]/*'/>
    public override int GetHashCode() {
      var hashCode = 964453631;
      unchecked {
        hashCode += 964453723 * this.exponent.GetHashCode();
        hashCode += 964453939 * this.unsignedMantissa.GetHashCode();
        hashCode += 964453967 * this.flags;
      }
      return hashCode;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.IsInfinity"]/*'/>
    public bool IsInfinity() {
      return (this.flags & BigNumberFlags.FlagInfinity) != 0;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.IsNaN"]/*'/>
    public bool IsNaN() {
      return (this.flags & (BigNumberFlags.FlagQuietNaN |
                    BigNumberFlags.FlagSignalingNaN)) != 0;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.IsNegativeInfinity"]/*'/>
    public bool IsNegativeInfinity() {
      return (this.flags & (BigNumberFlags.FlagInfinity |
                BigNumberFlags.FlagNegative)) == (BigNumberFlags.FlagInfinity |
                    BigNumberFlags.FlagNegative);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.IsPositiveInfinity"]/*'/>
    public bool IsPositiveInfinity() {
      return (this.flags & (BigNumberFlags.FlagInfinity |
                  BigNumberFlags.FlagNegative)) == BigNumberFlags.FlagInfinity;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.IsQuietNaN"]/*'/>
    public bool IsQuietNaN() {
      return (this.flags & BigNumberFlags.FlagQuietNaN) != 0;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.IsSignalingNaN"]/*'/>
    public bool IsSignalingNaN() {
      return (this.flags & BigNumberFlags.FlagSignalingNaN) != 0;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Log(PeterO.Numbers.EContext)"]/*'/>
    public EDecimal Log(EContext ctx) {
      return GetMathValue(ctx).Ln(this, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Log10(PeterO.Numbers.EContext)"]/*'/>
    public EDecimal Log10(EContext ctx) {
      return GetMathValue(ctx).Log10(this, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.MovePointLeft(System.Int32)"]/*'/>
    public EDecimal MovePointLeft(int places) {
      return this.MovePointLeft((EInteger)places, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.MovePointLeft(System.Int32,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal MovePointLeft(int places, EContext ctx) {
      return this.MovePointLeft((EInteger)places, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.MovePointLeft(PeterO.Numbers.EInteger)"]/*'/>
    public EDecimal MovePointLeft(EInteger bigPlaces) {
      return this.MovePointLeft(bigPlaces, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.MovePointLeft(PeterO.Numbers.EInteger,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal MovePointLeft(
  EInteger bigPlaces,
  EContext ctx) {
      return (!this.IsFinite) ? this.RoundToPrecision(ctx) :
        this.MovePointRight(-(EInteger)bigPlaces, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.MovePointRight(System.Int32)"]/*'/>
    public EDecimal MovePointRight(int places) {
      return this.MovePointRight((EInteger)places, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.MovePointRight(System.Int32,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal MovePointRight(int places, EContext ctx) {
      return this.MovePointRight((EInteger)places, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.MovePointRight(PeterO.Numbers.EInteger)"]/*'/>
    public EDecimal MovePointRight(EInteger bigPlaces) {
      return this.MovePointRight(bigPlaces, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.MovePointRight(PeterO.Numbers.EInteger,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal MovePointRight(
  EInteger bigPlaces,
  EContext ctx) {
      if (!this.IsFinite) {
        return this.RoundToPrecision(ctx);
      }
      EInteger bigExp = this.Exponent;
      bigExp += bigPlaces;
      if (bigExp.Sign > 0) {
        EInteger mant = this.unsignedMantissa.ToEInteger();
        EInteger bigPower = NumberUtility.FindPowerOfTenFromBig(bigExp);
        mant *= bigPower;
        return CreateWithFlags(
  mant,
  EInteger.Zero,
  this.flags).RoundToPrecision(ctx);
      }
      return CreateWithFlags(
        this.unsignedMantissa,
        FastIntegerFixed.FromBig(bigExp),
        this.flags).RoundToPrecision(ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Multiply(PeterO.Numbers.EDecimal)"]/*'/>
    public EDecimal Multiply(EDecimal otherValue) {
      if (this.IsFinite && otherValue.IsFinite) {
        int newflags = otherValue.flags ^ this.flags;
        if (this.unsignedMantissa.CanFitInInt32() &&
          otherValue.unsignedMantissa.CanFitInInt32()) {
          int integerA = this.unsignedMantissa.AsInt32();
          int integerB = otherValue.unsignedMantissa.AsInt32();
          long longA = ((long)integerA) * ((long)integerB);
          int sign = (longA == 0) ? 0 : (newflags == 0 ? 1 : -1);
          FastIntegerFixed exp = FastIntegerFixed.Add(
  this.exponent,
  otherValue.exponent);
          if ((longA >> 31) == 0) {
            return new EDecimal(
  new FastIntegerFixed((int)longA),
  exp,
  newflags,
  sign);
          } else {
            return new EDecimal(
  FastIntegerFixed.FromBig((EInteger)longA),
  exp,
  newflags,
  sign);
          }
        } else {
          EInteger eintA = this.unsignedMantissa.ToEInteger().Multiply(
           otherValue.unsignedMantissa.ToEInteger());
          int sign = eintA.IsZero ? 0 : (newflags == 0 ? 1 : -1);
          return new EDecimal(
  FastIntegerFixed.FromBig(eintA),
  FastIntegerFixed.Add(this.exponent, otherValue.exponent),
  newflags,
  sign);
        }
      }
      return this.Multiply(otherValue, EContext.UnlimitedHalfEven);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Multiply(PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal Multiply(EDecimal op, EContext ctx) {
      return GetMathValue(ctx).Multiply(this, op, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Add(System.Int32)"]/*'/>
public EDecimal Add(int intValue) {
 return this.Add(EDecimal.FromInt32(intValue));
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Subtract(System.Int32)"]/*'/>
public EDecimal Subtract(int intValue) {
 return (intValue == Int32.MinValue) ?
   this.Subtract(EDecimal.FromInt32(intValue)) : this.Add(-intValue);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Multiply(System.Int32)"]/*'/>
public EDecimal Multiply(int intValue) {
 return this.Multiply(EDecimal.FromInt32(intValue));
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Divide(System.Int32)"]/*'/>
public EDecimal Divide(int intValue) {
 return this.Divide(EDecimal.FromInt32(intValue));
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.MultiplyAndAdd(PeterO.Numbers.EDecimal,PeterO.Numbers.EDecimal)"]/*'/>
    public EDecimal MultiplyAndAdd(
      EDecimal multiplicand,
      EDecimal augend) {
      return this.MultiplyAndAdd(multiplicand, augend, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.MultiplyAndAdd(PeterO.Numbers.EDecimal,PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal MultiplyAndAdd(
      EDecimal op,
      EDecimal augend,
      EContext ctx) {
      return GetMathValue(ctx).MultiplyAndAdd(this, op, augend, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.MultiplyAndSubtract(PeterO.Numbers.EDecimal,PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal MultiplyAndSubtract(
      EDecimal op,
      EDecimal subtrahend,
      EContext ctx) {
      if (op == null) {
        throw new ArgumentNullException(nameof(op));
      }
      if (subtrahend == null) {
        throw new ArgumentNullException(nameof(subtrahend));
      }
      EDecimal negated = subtrahend;
      if ((subtrahend.flags & BigNumberFlags.FlagNaN) == 0) {
        int newflags = subtrahend.flags ^ BigNumberFlags.FlagNegative;
        negated = CreateWithFlags(
          subtrahend.unsignedMantissa,
          subtrahend.exponent,
          newflags);
      }
      return GetMathValue(ctx)
        .MultiplyAndAdd(this, op, negated, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Negate"]/*'/>
    public EDecimal Negate() {
      return new EDecimal(
  this.unsignedMantissa,
  this.exponent,
  this.flags ^ BigNumberFlags.FlagNegative,
  -this.sign);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Negate(PeterO.Numbers.EContext)"]/*'/>
    public EDecimal Negate(EContext context) {
      return ((context == null || context == EContext.UnlimitedHalfEven) ?
        ExtendedMathValue : MathValue).Negate(this, context);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.NextMinus(PeterO.Numbers.EContext)"]/*'/>
    public EDecimal NextMinus(EContext ctx) {
      return GetMathValue(ctx).NextMinus(this, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.NextPlus(PeterO.Numbers.EContext)"]/*'/>
    public EDecimal NextPlus(EContext ctx) {
      return GetMathValue(ctx)
        .NextPlus(this, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.NextToward(PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal NextToward(
      EDecimal otherValue,
      EContext ctx) {
      return GetMathValue(ctx)
        .NextToward(this, otherValue, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Plus(PeterO.Numbers.EContext)"]/*'/>
    public EDecimal Plus(EContext ctx) {
      return GetMathValue(ctx).Plus(this, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Pow(PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal Pow(EDecimal exponent, EContext ctx) {
      return GetMathValue(ctx).Power(this, exponent, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Pow(System.Int32,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal Pow(int exponentSmall, EContext ctx) {
      return this.Pow(EDecimal.FromInt64(exponentSmall), ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Pow(System.Int32)"]/*'/>
    public EDecimal Pow(int exponentSmall) {
      return this.Pow(EDecimal.FromInt64(exponentSmall), null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Precision"]/*'/>
    public EInteger Precision() {
      if (!this.IsFinite) {
        return EInteger.Zero;
      }
      if (this.IsZero) {
        return EInteger.One;
      }
      int digcount = this.unsignedMantissa.ToEInteger().GetDigitCount();
      return (EInteger)digcount;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Quantize(PeterO.Numbers.EInteger,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal Quantize(
      EInteger desiredExponent,
      EContext ctx) {
      return this.Quantize(
        EDecimal.Create(EInteger.One, desiredExponent),
        ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Quantize(System.Int32,PeterO.Numbers.ERounding)"]/*'/>
    public EDecimal Quantize(
      int desiredExponentInt,
      ERounding rounding) {
      EDecimal ret = this.RoundToExponentFast(
  desiredExponentInt,
  rounding);
      if (ret != null) {
        return ret;
      }
      return this.Quantize(
      EDecimal.Create(EInteger.One, (EInteger)desiredExponentInt),
      EContext.ForRounding(rounding));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Quantize(System.Int32,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal Quantize(
      int desiredExponentInt,
      EContext ctx) {
      if (ctx == null ||
         (!ctx.HasExponentRange && !ctx.HasFlags && ctx.Traps == 0 &&
          !ctx.HasMaxPrecision && !ctx.IsSimplified)) {
        EDecimal ret = this.RoundToExponentFast(
  desiredExponentInt,
  ctx == null ? ERounding.HalfEven : ctx.Rounding);
        if (ret != null) {
          return ret;
        }
      }
      return this.Quantize(
      EDecimal.Create(EInteger.One, (EInteger)desiredExponentInt),
      ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Quantize(PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal Quantize(
      EDecimal otherValue,
      EContext ctx) {
      return GetMathValue(ctx).Quantize(this, otherValue, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Reduce(PeterO.Numbers.EContext)"]/*'/>
    public EDecimal Reduce(EContext ctx) {
      return GetMathValue(ctx).Reduce(this, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Remainder(PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal Remainder(
      EDecimal divisor,
      EContext ctx) {
      return GetMathValue(ctx).Remainder(this, divisor, ctx, true);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.RemainderNoRoundAfterDivide(PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal RemainderNoRoundAfterDivide(
      EDecimal divisor,
      EContext ctx) {
      return GetMathValue(ctx).Remainder(this, divisor, ctx, false);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.RemainderNaturalScale(PeterO.Numbers.EDecimal)"]/*'/>
    public EDecimal RemainderNaturalScale(EDecimal divisor) {
      return this.RemainderNaturalScale(divisor, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.RemainderNaturalScale(PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal RemainderNaturalScale(
      EDecimal divisor,
      EContext ctx) {
      return this.Subtract(
        this.DivideToIntegerNaturalScale(divisor, null).Multiply(divisor, null),
        ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.RemainderNear(PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal RemainderNear(
      EDecimal divisor,
      EContext ctx) {
      return GetMathValue(ctx)
        .RemainderNear(this, divisor, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.RoundToExponent(PeterO.Numbers.EInteger,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal RoundToExponent(
      EInteger exponent,
      EContext ctx) {
      return GetMathValue(ctx)
        .RoundToExponentSimple(this, exponent, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.RoundToExponent(PeterO.Numbers.EInteger)"]/*'/>
    public EDecimal RoundToExponent(
      EInteger exponent) {
      return this.RoundToExponent(
  exponent,
  EContext.ForRounding(ERounding.HalfEven));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.RoundToExponent(PeterO.Numbers.EInteger,PeterO.Numbers.ERounding)"]/*'/>
    public EDecimal RoundToExponent(
      EInteger exponent,
      ERounding rounding) {
      return this.RoundToExponent(
  exponent,
  EContext.ForRounding(rounding));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.RoundToExponent(System.Int32)"]/*'/>
    public EDecimal RoundToExponent(
      int exponentSmall) {
      return this.RoundToExponent(exponentSmall, ERounding.HalfEven);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.RoundToExponent(System.Int32,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal RoundToExponent(
      int exponentSmall,
      EContext ctx) {
      if (ctx == null ||
         (!ctx.HasExponentRange && !ctx.HasFlags && ctx.Traps == 0 &&
          !ctx.HasMaxPrecision && !ctx.IsSimplified)) {
        EDecimal ret = this.RoundToExponentFast(
  exponentSmall,
  ctx == null ? ERounding.HalfEven : ctx.Rounding);
        if (ret != null) {
          return ret;
        }
      }
      return this.RoundToExponent((EInteger)exponentSmall, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.RoundToExponent(System.Int32,PeterO.Numbers.ERounding)"]/*'/>
    public EDecimal RoundToExponent(
      int exponentSmall,
      ERounding rounding) {
      EDecimal ret = this.RoundToExponentFast(
  exponentSmall,
  rounding);
      if (ret != null) {
        return ret;
      }
      return this.RoundToExponent(
  exponentSmall,
  EContext.ForRounding(rounding));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.RoundToExponentExact(PeterO.Numbers.EInteger,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal RoundToExponentExact(
      EInteger exponent,
      EContext ctx) {
      return GetMathValue(ctx)
        .RoundToExponentExact(this, exponent, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.RoundToExponentExact(System.Int32,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal RoundToExponentExact(
      int exponentSmall,
      EContext ctx) {
      return this.RoundToExponentExact((EInteger)exponentSmall, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.RoundToExponentExact(System.Int32,PeterO.Numbers.ERounding)"]/*'/>
    public EDecimal RoundToExponentExact(
      int exponentSmall,
      ERounding rounding) {
 return this.RoundToExponentExact(
  (EInteger)exponentSmall,
  EContext.Unlimited.WithRounding(rounding));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.RoundToIntegerExact(PeterO.Numbers.EContext)"]/*'/>
    public EDecimal RoundToIntegerExact(EContext ctx) {
      return GetMathValue(ctx).RoundToExponentExact(this, EInteger.Zero, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.RoundToIntegerNoRoundedFlag(PeterO.Numbers.EContext)"]/*'/>
    public EDecimal RoundToIntegerNoRoundedFlag(EContext ctx) {
      return GetMathValue(ctx)
        .RoundToExponentNoRoundedFlag(this, EInteger.Zero, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.RoundToIntegralExact(PeterO.Numbers.EContext)"]/*'/>
    [Obsolete("Renamed to RoundToIntegerExact.")]
    public EDecimal RoundToIntegralExact(EContext ctx) {
      return GetMathValue(ctx).RoundToExponentExact(this, EInteger.Zero, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.RoundToIntegralNoRoundedFlag(PeterO.Numbers.EContext)"]/*'/>
    [Obsolete("Renamed to RoundToIntegerNoRoundedFlag.")]
    public EDecimal RoundToIntegralNoRoundedFlag(EContext ctx) {
      return GetMathValue(ctx)
        .RoundToExponentNoRoundedFlag(this, EInteger.Zero, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.RoundToPrecision(PeterO.Numbers.EContext)"]/*'/>
    public EDecimal RoundToPrecision(EContext ctx) {
      return GetMathValue(ctx).RoundToPrecision(this, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ScaleByPowerOfTen(System.Int32)"]/*'/>
    public EDecimal ScaleByPowerOfTen(int places) {
      return this.ScaleByPowerOfTen((EInteger)places, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ScaleByPowerOfTen(System.Int32,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal ScaleByPowerOfTen(int places, EContext ctx) {
      return this.ScaleByPowerOfTen((EInteger)places, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ScaleByPowerOfTen(PeterO.Numbers.EInteger)"]/*'/>
    public EDecimal ScaleByPowerOfTen(EInteger bigPlaces) {
      return this.ScaleByPowerOfTen(bigPlaces, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ScaleByPowerOfTen(PeterO.Numbers.EInteger,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal ScaleByPowerOfTen(
  EInteger bigPlaces,
  EContext ctx) {
      if (bigPlaces.IsZero) {
        return this.RoundToPrecision(ctx);
      }
      if (!this.IsFinite) {
        return this.RoundToPrecision(ctx);
      }
      EInteger bigExp = this.Exponent;
      bigExp += bigPlaces;
      return CreateWithFlags(
        this.unsignedMantissa,
        FastIntegerFixed.FromBig(bigExp),
        this.flags).RoundToPrecision(ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Sqrt(PeterO.Numbers.EContext)"]/*'/>
    public EDecimal Sqrt(EContext ctx) {
      return GetMathValue(ctx).SquareRoot(this, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.SquareRoot(PeterO.Numbers.EContext)"]/*'/>
    [Obsolete("Renamed to Sqrt.")]
    public EDecimal SquareRoot(EContext ctx) {
      return GetMathValue(ctx).SquareRoot(this, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Subtract(PeterO.Numbers.EDecimal)"]/*'/>
    public EDecimal Subtract(EDecimal otherValue) {
      return this.Subtract(otherValue, EContext.UnlimitedHalfEven);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Subtract(PeterO.Numbers.EDecimal,PeterO.Numbers.EContext)"]/*'/>
    public EDecimal Subtract(
      EDecimal otherValue,
      EContext ctx) {
      if (otherValue == null) {
        throw new ArgumentNullException(nameof(otherValue));
      }
      EDecimal negated = otherValue;
      if ((otherValue.flags & BigNumberFlags.FlagNaN) == 0) {
        int newflags = otherValue.flags ^ BigNumberFlags.FlagNegative;
        negated = CreateWithFlags(
          otherValue.unsignedMantissa,
          otherValue.exponent,
          newflags);
      }
      return this.Add(negated, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToDouble"]/*'/>
    public double ToDouble() {
      if (this.IsPositiveInfinity()) {
        return Double.PositiveInfinity;
      }
      if (this.IsNegativeInfinity()) {
        return Double.NegativeInfinity;
      }
      if (this.IsNegative && this.IsZero) {
        return Extras.IntegersToDouble(new[] { 0, unchecked((int)(1 << 31)) });
      }
      if (this.IsZero) {
        return 0.0;
      }
      if (this.IsFinite) {
       EInteger adjExp = this.GetAdjustedExponent();
        if (adjExp.CompareTo((EInteger)(-326)) < 0) {
          // Very low exponent, treat as 0
        return this.IsNegative ? Extras.IntegersToDouble(new[] { 0,
            unchecked((int)(1 << 31)) }) : 0.0;
       }
       if (adjExp.CompareTo((EInteger)309) > 0) {
        // Very high exponent, treat as infinity
        return this.IsNegative ? Double.NegativeInfinity :
          Double.PositiveInfinity;
       }
      }
      return this.ToEFloat(EContext.Binary64).ToDouble();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToEInteger"]/*'/>
    public EInteger ToEInteger() {
      return this.ToEIntegerInternal(false);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToEIntegerExact"]/*'/>
    [Obsolete("Renamed to ToEIntegerIfExact.")]
    public EInteger ToEIntegerExact() {
      return this.ToEIntegerInternal(true);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToEIntegerIfExact"]/*'/>
    public EInteger ToEIntegerIfExact() {
      return this.ToEIntegerInternal(true);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToEngineeringString"]/*'/>
    public string ToEngineeringString() {
      return this.ToStringInternal(1);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToExtendedFloat"]/*'/>
    [Obsolete("Renamed to ToEFloat.")]
    public EFloat ToExtendedFloat() {
      return this.ToEFloat(EContext.UnlimitedHalfEven);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToEFloat"]/*'/>
    public EFloat ToEFloat() {
      return this.ToEFloat(EContext.UnlimitedHalfEven);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToPlainString"]/*'/>
    public string ToPlainString() {
      return this.ToStringInternal(2);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToSingle"]/*'/>
    public float ToSingle() {
      if (this.IsPositiveInfinity()) {
        return Single.PositiveInfinity;
      }
      if (this.IsNegativeInfinity()) {
        return Single.NegativeInfinity;
      }
      if (this.IsNegative && this.IsZero) {
        return BitConverter.ToSingle(BitConverter.GetBytes((int)1 << 31), 0);
      }
      if (this.IsZero) {
        return 0.0f;
      }
      EInteger adjExp = this.GetAdjustedExponent();
      if (adjExp.CompareTo((EInteger)(-47)) < 0) {
        // Very low exponent, treat as 0
        return this.IsNegative ?
          BitConverter.ToSingle(BitConverter.GetBytes((int)1 << 31), 0) :
          0.0f;
      }
      if (adjExp.CompareTo((EInteger)39) > 0) {
        // Very high exponent, treat as infinity
        return this.IsNegative ? Single.NegativeInfinity :
          Single.PositiveInfinity;
      }
      return this.ToEFloat(EContext.Binary32).ToSingle();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToString"]/*'/>
    public override string ToString() {
      return this.ToStringInternal(0);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.Ulp"]/*'/>
    public EDecimal Ulp() {
      return (!this.IsFinite) ? EDecimal.One :
        EDecimal.Create(EInteger.One, this.Exponent);
    }

    internal static EDecimal CreateWithFlags(
      FastIntegerFixed mantissa,
      FastIntegerFixed exponent,
      int flags) {
      if (mantissa == null) {
        throw new ArgumentNullException(nameof(mantissa));
      }
      if (exponent == null) {
        throw new ArgumentNullException(nameof(exponent));
      }
#if DEBUG
      if (!(mantissa.Sign >= 0)) {
        throw new ArgumentException("doesn't satisfy mantissa.Sign >= 0");
      }
#endif
      int sign = (((flags & BigNumberFlags.FlagSpecial) == 0) &&
                mantissa.IsValueZero) ? 0 : (((flags &
                    BigNumberFlags.FlagNegative) != 0) ? -1 : 1);
      return new EDecimal(
        mantissa,
        exponent,
        flags,
        sign);
    }

    internal static EDecimal CreateWithFlags(
      EInteger mantissa,
      EInteger exponent,
      int flags) {
      if (mantissa == null) {
        throw new ArgumentNullException(nameof(mantissa));
      }
      if (exponent == null) {
        throw new ArgumentNullException(nameof(exponent));
      }
#if DEBUG
      if (!(mantissa.Sign >= 0)) {
        throw new ArgumentException("doesn't satisfy mantissa.Sign >= 0");
      }
#endif
      int sign = (((flags & BigNumberFlags.FlagSpecial) == 0) &&
                mantissa.IsZero) ? 0 : (((flags &
                    BigNumberFlags.FlagNegative) != 0) ? -1 : 1);
      return new EDecimal(
        FastIntegerFixed.FromBig(mantissa),
        FastIntegerFixed.FromBig(exponent),
        flags,
        sign);
    }

    private static bool AppendString(
      StringBuilder builder,
      char c,
      FastInteger count) {
      if (count.CompareToInt(Int32.MaxValue) > 0 || count.Sign < 0) {
        throw new NotSupportedException();
      }
      int icount = count.AsInt32();
      for (int i = icount - 1; i >= 0; --i) {
        builder.Append(c);
      }
      return true;
    }

    private static EInteger GetAdjustedExponentBinary(EFloat ef) {
      if (!ef.IsFinite) {
        return EInteger.Zero;
      }
      if (ef.IsZero) {
        return EInteger.Zero;
      }
      EInteger retEInt = ef.Exponent;
      int smallPrecision = ef.UnsignedMantissa.GetSignedBitLength();
      --smallPrecision;
      retEInt = retEInt.Add(EInteger.FromInt32(smallPrecision));
      return retEInt;
    }

    private static IRadixMath<EDecimal> GetMathValue(EContext ctx) {
      if (ctx == null || ctx == EContext.UnlimitedHalfEven) {
        return ExtendedMathValue;
      }
      return (!ctx.IsSimplified && ctx.Traps == 0) ? ExtendedMathValue :
        MathValue;
    }

    private bool EqualsInternal(EDecimal otherValue) {
      return (otherValue != null) && (this.flags == otherValue.flags &&
                    this.unsignedMantissa.Equals(otherValue.unsignedMantissa) &&
                this.exponent.Equals(otherValue.exponent));
    }

    private EInteger GetAdjustedExponent() {
      if (!this.IsFinite) {
        return EInteger.Zero;
      }
      if (this.IsZero) {
        return EInteger.Zero;
      }
      EInteger retEInt = this.Exponent;
      int smallPrecision = this.UnsignedMantissa.GetDigitCount();
      --smallPrecision;
      retEInt = retEInt.Add(EInteger.FromInt32(smallPrecision));
      return retEInt;
    }

    private EDecimal RoundToExponentFast(
  int exponentSmall,
  ERounding rounding) {
      if (this.IsFinite && this.exponent.CanFitInInt32() &&
        this.unsignedMantissa.CanFitInInt32()) {
        int thisExponentSmall = this.exponent.AsInt32();
        if (thisExponentSmall == exponentSmall) {
          return this;
        }
        int thisMantissaSmall = this.unsignedMantissa.AsInt32();
        if (thisExponentSmall >= -100 && thisExponentSmall <= 100 &&
          exponentSmall >= -100 && exponentSmall <= 100) {
          if (rounding == ERounding.Down) {
            int diff = exponentSmall - thisExponentSmall;
            if (diff >= 1 && diff <= 9) {
              thisMantissaSmall /= ValueTenPowers[diff];
              return new EDecimal(
                new FastIntegerFixed(thisMantissaSmall),
                new FastIntegerFixed(exponentSmall),
                this.flags,
                thisMantissaSmall == 0 ? 0 : ((this.flags == 0) ? 1 : -1));
            }
          } else if (rounding == ERounding.HalfEven &&
              thisMantissaSmall != Int32.MaxValue) {
            int diff = exponentSmall - thisExponentSmall;
            if (diff >= 1 && diff <= 9) {
              int pwr = ValueTenPowers[diff - 1];
              int div = thisMantissaSmall / pwr;
              int div2 = (div > 43698) ? (div / 10) : ((div * 26215) >> 18);
              int rem = div - (div2 * 10);
              if (rem > 5) {
                ++div2;
              } else if (rem == 5 && (thisMantissaSmall - (div * pwr)) != 0) {
                ++div2;
              } else if (rem == 5 && (div2 & 1) == 1) {
                ++div2;
              }
              return new EDecimal(
                new FastIntegerFixed(div2),
                new FastIntegerFixed(exponentSmall),
                this.flags,
                div2 == 0 ? 0 : ((this.flags == 0) ? 1 : -1));
            }
          }
        }
      }
      return null;
    }

    private bool IsIntegerPartZero() {
      if (!this.IsFinite) {
        return false;
      }
      if (this.unsignedMantissa.IsValueZero) {
        return true;
      }
      int sign = this.Exponent.Sign;
      if (sign >= 0) {
        return false;
      } else {
        FastInteger bigexponent = this.exponent.ToFastInteger().Negate();
        EInteger bigmantissa = this.unsignedMantissa.ToEInteger();
        var acc = new DigitShiftAccumulator(bigmantissa, 0, 0);
  return (acc.GetDigitLength().CompareTo(bigexponent) <= 0) ? true :
          false;
      }
    }

    private EInteger ToEIntegerInternal(bool exact) {
      if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      int sign = this.Exponent.Sign;
      if (this.IsZero) {
        return EInteger.Zero;
      }
      if (sign == 0) {
        EInteger bigmantissa = this.Mantissa;
        return bigmantissa;
      }
      if (sign > 0) {
        EInteger bigmantissa = this.Mantissa;
        EInteger bigexponent =
          NumberUtility.FindPowerOfTenFromBig(this.Exponent);
        bigmantissa *= (EInteger)bigexponent;
        return bigmantissa;
      } else {
        if (exact && !this.unsignedMantissa.IsEvenNumber) {
          // Mantissa is odd and will have to shift a nonzero
          // number of digits, so can't be an exact integer
          throw new ArithmeticException("Not an exact integer");
        }
        FastInteger bigexponent = this.exponent.ToFastInteger().Negate();
        EInteger bigmantissa = this.unsignedMantissa.ToEInteger();
        var acc = new DigitShiftAccumulator(bigmantissa, 0, 0);
        acc.TruncateRight(bigexponent);
        if (exact && (acc.LastDiscardedDigit != 0 || acc.OlderDiscardedDigits !=
                    0)) {
          // Some digits were discarded
          throw new ArithmeticException("Not an exact integer");
        }
        bigmantissa = acc.ShiftedInt;
        if (this.IsNegative) {
          bigmantissa = -bigmantissa;
        }
        return bigmantissa;
      }
    }

    private static bool HasTerminatingBinaryExpansion(EInteger
      den) {
      if (den.IsZero) {
        return false;
      }
      if (den.GetUnsignedBit(0) && den.CompareTo(EInteger.One) != 0) {
        return false;
      }
      int lowBit = den.GetLowBit();
      den >>= lowBit;
      return den.Equals(EInteger.One);
    }

    private EFloat WithThisSign(EFloat ef) {
      return this.IsNegative ? ef.Negate() : ef;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToEFloat(PeterO.Numbers.EContext)"]/*'/>
    public EFloat ToEFloat(EContext ec) {
      // TODO: Investigate speeding up Binary64 case
      EInteger bigintExp = this.Exponent;
      EInteger bigintMant = this.UnsignedMantissa;
      if (this.IsNaN()) {
        return EFloat.CreateNaN(
  this.UnsignedMantissa,
  this.IsSignalingNaN(),
  this.IsNegative,
  ec);
      }
      if (this.IsPositiveInfinity()) {
        return EFloat.PositiveInfinity.RoundToPrecision(ec);
      }
      if (this.IsNegativeInfinity()) {
        return EFloat.NegativeInfinity.RoundToPrecision(ec);
      }
      if (bigintMant.IsZero) {
        return this.IsNegative ? EFloat.NegativeZero.RoundToPrecision(ec) :
          EFloat.Zero.RoundToPrecision(ec);
      }
      if (bigintExp.IsZero) {
        // Integer
        // DebugUtility.Log("Integer");
     return this.WithThisSign(EFloat.FromEInteger(bigintMant))
  .RoundToPrecision(ec);
      }
      if (bigintExp.Sign > 0) {
        // Scaled integer
        // DebugUtility.Log("Scaled integer");
        EInteger bigmantissa = bigintMant;
        bigintExp = NumberUtility.FindPowerOfTenFromBig(bigintExp);
        bigmantissa *= (EInteger)bigintExp;
    return this.WithThisSign(EFloat.FromEInteger(bigmantissa))
  .RoundToPrecision(ec);
      } else {
        // Fractional number
        // DebugUtility.Log("Fractional");
        EInteger scale = bigintExp;
        EInteger bigmantissa = bigintMant;
        bool neg = bigmantissa.Sign < 0;
        if (neg) {
          bigmantissa = -(EInteger)bigmantissa;
        }
        EInteger negscale = -scale;
        // DebugUtility.Log("" + negscale);
        EInteger divisor = NumberUtility.FindPowerOfTenFromBig(negscale);
        EInteger desiredHigh;
        EInteger desiredLow;
        var haveCopy = false;
ec = ec ?? EContext.UnlimitedHalfEven;
        EContext originalEc = ec;
        if (!ec.HasMaxPrecision) {
          EInteger num = bigmantissa;
          EInteger den = divisor;
          EInteger gcd = num.Gcd(den);
          if (gcd.CompareTo(EInteger.One) != 0) {
            den /= gcd;
          }
          // DebugUtility.Log("num=" + (num/gcd));
          // DebugUtility.Log("den=" + den);
          if (!HasTerminatingBinaryExpansion(den)) {
            // DebugUtility.Log("Approximate");
            // DebugUtility.Log("=>{0}\r\n->{1}", bigmantissa, divisor);
            ec = ec.WithPrecision(53).WithBlankFlags();
            haveCopy = true;
          } else {
            bigmantissa /= gcd;
            divisor = den;
          }
        }
        // NOTE: Precision added by 2 to accommodate rounding
        // to odd
        EInteger valueEcPrec = ec.HasMaxPrecision ? ec.Precision +
          EInteger.FromInt32(2) : EInteger.Zero;
        var valueEcPrecInt = 0;
        if (!valueEcPrec.CanFitInInt32()) {
          EInteger precm1 = valueEcPrec - EInteger.One;
          desiredLow = EInteger.One;
          while (precm1.Sign > 0) {
           var shift = 1000000;
           if (precm1.CompareTo((EInteger)1000000) < 0) {
            shift = precm1.ToInt32Checked();
           }
           desiredLow <<= shift;
           precm1 -= (EInteger)shift;
          }
          desiredHigh = desiredLow << 1;
        } else {
          int prec = valueEcPrec.ToInt32Checked();
          valueEcPrecInt = prec;
          desiredHigh = EInteger.One << prec;
          int precm1 = prec - 1;
          desiredLow = EInteger.One << precm1;
        }
        // DebugUtility.Log("=>{0}\r\n->{1}", bigmantissa, divisor);
        EInteger[] quorem = ec.HasMaxPrecision ?
          bigmantissa.DivRem(divisor) : null;
        // DebugUtility.Log("=>{0}\r\n->{1}", quorem[0], desiredHigh);
        var adjust = new FastInteger(0);
        if (!ec.HasMaxPrecision) {
          int term = divisor.GetLowBit();
          bigmantissa <<= term;
          adjust.SubtractInt(term);
          quorem = bigmantissa.DivRem(divisor);
        } else if (quorem[0].CompareTo(desiredHigh) >= 0) {
          do {
            var optimized = false;
            if (divisor.CompareTo(bigmantissa) < 0) {
              if (ec.ClampNormalExponents && valueEcPrecInt > 0 &&
                  valueEcPrecInt != Int32.MaxValue) {
               int valueBmBits = bigmantissa.GetUnsignedBitLength();
               int divBits = divisor.GetUnsignedBitLength();
               if (divBits < valueBmBits) {
                int bitdiff = valueBmBits - divBits;
                if (bitdiff > valueEcPrecInt + 1) {
                  bitdiff -= valueEcPrecInt + 1;
                  divisor <<= bitdiff;
                  adjust.AddInt(bitdiff);
                  optimized = true;
                }
               }
              }
            } else {
              if (ec.ClampNormalExponents && valueEcPrecInt > 0) {
                int valueBmBits = bigmantissa.GetUnsignedBitLength();
                int divBits = divisor.GetUnsignedBitLength();
             if (valueBmBits >= divBits && valueEcPrecInt <= Int32.MaxValue -
                  divBits) {
                  int vbb = divBits + valueEcPrecInt;
                  if (valueBmBits < vbb) {
                    valueBmBits = vbb - valueBmBits;
                    divisor <<= valueBmBits;
                    adjust.AddInt(valueBmBits);
                    optimized = true;
                  }
                }
              }
            }
            if (!optimized) {
              divisor <<= 1;
              adjust.Increment();
            }
// DebugUtility.Log("deshigh\n==>" + (//
// bigmantissa) + "\n-->" + (//
// divisor));
// DebugUtility.Log("deshigh " + (//
// bigmantissa.GetUnsignedBitLength()) + "/" + (//
// divisor.GetUnsignedBitLength()));
            quorem = bigmantissa.DivRem(divisor);
            if (quorem[1].IsZero) {
              int valueBmBits = quorem[0].GetUnsignedBitLength();
              int divBits = desiredLow.GetUnsignedBitLength();
              if (valueBmBits < divBits) {
                valueBmBits = divBits - valueBmBits;
                quorem[0] = quorem[0].ShiftLeft(valueBmBits);
                adjust.AddInt(valueBmBits);
              }
            }
  // DebugUtility.Log("quorem[0]="+quorem[0]);
     // DebugUtility.Log("quorem[1]="+quorem[1]);
        // DebugUtility.Log("desiredLow="+desiredLow);
           // DebugUtility.Log("desiredHigh="+desiredHigh);
          } while (quorem[0].CompareTo(desiredHigh) >= 0);
        } else if (quorem[0].CompareTo(desiredLow) < 0) {
          do {
            var optimized = false;
            if (bigmantissa.CompareTo(divisor) < 0) {
              int valueBmBits = bigmantissa.GetUnsignedBitLength();
              int divBits = divisor.GetUnsignedBitLength();
              if (valueBmBits < divBits) {
                valueBmBits = divBits - valueBmBits;
                bigmantissa <<= valueBmBits;
                adjust.SubtractInt(valueBmBits);
                optimized = true;
              }
            } else {
              if (ec.ClampNormalExponents && valueEcPrecInt > 0) {
                int valueBmBits = bigmantissa.GetUnsignedBitLength();
                int divBits = divisor.GetUnsignedBitLength();
             if (valueBmBits >= divBits && valueEcPrecInt <= Int32.MaxValue -
                  divBits) {
                  int vbb = divBits + valueEcPrecInt;
                  if (valueBmBits < vbb) {
                    valueBmBits = vbb - valueBmBits;
                    bigmantissa <<= valueBmBits;
                    adjust.SubtractInt(valueBmBits);
                    optimized = true;
                  }
                }
              }
            }
            if (!optimized) {
              bigmantissa <<= 1;
              adjust.Decrement();
            }
            // DebugUtility.Log("deslow " + (//
            // bigmantissa.GetUnsignedBitLength()) + "/" + (//
            // divisor.GetUnsignedBitLength()));
            quorem = bigmantissa.DivRem(divisor);
            if (quorem[1].IsZero) {
              int valueBmBits = quorem[0].GetUnsignedBitLength();
              int divBits = desiredLow.GetUnsignedBitLength();
              if (valueBmBits < divBits) {
                valueBmBits = divBits - valueBmBits;
                quorem[0] = quorem[0].ShiftLeft(valueBmBits);
                adjust.SubtractInt(valueBmBits);
              }
            }
          } while (quorem[0].CompareTo(desiredLow) < 0);
        }
        // Round to odd to avoid rounding errors
        if (!quorem[1].IsZero && quorem[0].IsEven) {
          quorem[0] = quorem[0].Add(EInteger.One);
        }
        EFloat efret = this.WithThisSign(
  EFloat.Create(
  quorem[0],
  adjust.AsEInteger()));
        // DebugUtility.Log("-->" + (efret.Mantissa.ToRadixString(2)) + " " +
        // (// efret.Exponent));
        efret = efret.RoundToPrecision(ec);
        if (haveCopy && originalEc.HasFlags) {
          originalEc.Flags |= ec.Flags;
        }
        return efret;
      }
    }

    private string ToStringInternal(int mode) {
      bool negative = (this.flags & BigNumberFlags.FlagNegative) != 0;
      if (!this.IsFinite) {
        if ((this.flags & BigNumberFlags.FlagInfinity) != 0) {
          return negative ? "-Infinity" : "Infinity";
        }
        if ((this.flags & BigNumberFlags.FlagSignalingNaN) != 0) {
          return this.unsignedMantissa.IsValueZero ?
            (negative ? "-sNaN" : "sNaN") :
            (negative ? "-sNaN" + this.unsignedMantissa :
                    "sNaN" + this.unsignedMantissa);
        }
        if ((this.flags & BigNumberFlags.FlagQuietNaN) != 0) {
          return this.unsignedMantissa.IsValueZero ? (negative ?
         "-NaN" : "NaN") : (negative ? "-NaN" + this.unsignedMantissa :
              "NaN" + this.unsignedMantissa);
        }
      }
      int scaleSign = -this.exponent.Sign;
      string mantissaString = this.unsignedMantissa.ToString();
      if (scaleSign == 0) {
        return negative ? "-" + mantissaString : mantissaString;
      }
      bool iszero = this.unsignedMantissa.IsValueZero;
      if (mode == 2 && iszero && scaleSign < 0) {
        // special case for zero in plain
        return negative ? "-" + mantissaString : mantissaString;
      }
      StringBuilder builder = null;
      if (mode == 0 && mantissaString.Length < 100 &&
        this.exponent.CanFitInInt32()) {
        int intExp = this.exponent.AsInt32();
        if (intExp > -100 && intExp < 100) {
          int adj = (intExp + mantissaString.Length) - 1;
          if (scaleSign >= 0 && adj >= -6) {
            if (scaleSign > 0) {
              int dp = intExp + mantissaString.Length;
              if (dp < 0) {
                builder = new StringBuilder(mantissaString.Length + 6);
                if (negative) {
                  builder.Append("-0.");
                } else {
                  builder.Append("0.");
                }
                dp = -dp;
                for (var j = 0; j < dp; ++j) {
                  builder.Append('0');
                }
                builder.Append(mantissaString);
                return builder.ToString();
              } else if (dp == 0) {
                builder = new StringBuilder(mantissaString.Length + 6);
                if (negative) {
                  builder.Append("-0.");
                } else {
                  builder.Append("0.");
                }
                builder.Append(mantissaString);
                return builder.ToString();
              } else if (dp > 0 && dp <= mantissaString.Length) {
                builder = new StringBuilder(mantissaString.Length + 6);
                if (negative) {
                  builder.Append('-');
                }
                builder.Append(mantissaString, 0, dp);
                builder.Append('.');
                builder.Append(
                  mantissaString,
                  dp,
                  mantissaString.Length - dp);
                return builder.ToString();
              }
            }
          }
        }
      }
      FastInteger adjustedExponent = FastInteger.FromBig(this.Exponent);
      var builderLength = new FastInteger(mantissaString.Length);
      FastInteger thisExponent = adjustedExponent.Copy();
      adjustedExponent.Add(builderLength).Decrement();
      var decimalPointAdjust = new FastInteger(1);
      var threshold = new FastInteger(-6);
      if (mode == 1) {
        // engineering string adjustments
        FastInteger newExponent = adjustedExponent.Copy();
        bool adjExponentNegative = adjustedExponent.Sign < 0;
        int intphase = adjustedExponent.Copy().Abs().Remainder(3).AsInt32();
        if (iszero && (adjustedExponent.CompareTo(threshold) < 0 || scaleSign <
                    0)) {
          if (intphase == 1) {
            if (adjExponentNegative) {
              decimalPointAdjust.Increment();
              newExponent.Increment();
            } else {
              decimalPointAdjust.AddInt(2);
              newExponent.AddInt(2);
            }
          } else if (intphase == 2) {
            if (!adjExponentNegative) {
              decimalPointAdjust.Increment();
              newExponent.Increment();
            } else {
              decimalPointAdjust.AddInt(2);
              newExponent.AddInt(2);
            }
          }
          threshold.Increment();
        } else {
          if (intphase == 1) {
            if (!adjExponentNegative) {
              decimalPointAdjust.Increment();
              newExponent.Decrement();
            } else {
              decimalPointAdjust.AddInt(2);
              newExponent.AddInt(-2);
            }
          } else if (intphase == 2) {
            if (adjExponentNegative) {
              decimalPointAdjust.Increment();
              newExponent.Decrement();
            } else {
              decimalPointAdjust.AddInt(2);
              newExponent.AddInt(-2);
            }
          }
        }
        adjustedExponent = newExponent;
      }
      if (mode == 2 || (adjustedExponent.CompareTo(threshold) >= 0 &&
                    scaleSign >= 0)) {
        if (scaleSign > 0) {
          FastInteger decimalPoint = thisExponent.Copy().Add(builderLength);
          int cmp = decimalPoint.CompareToInt(0);
          builder = null;
          if (cmp < 0) {
            var tmpFast = new FastInteger(mantissaString.Length).AddInt(6);
            builder = new StringBuilder(tmpFast.CompareToInt(Int32.MaxValue) >
                    0 ? Int32.MaxValue : tmpFast.AsInt32());
            if (negative) {
              builder.Append('-');
            }
            builder.Append("0.");
            AppendString(builder, '0', decimalPoint.Copy().Negate());
            builder.Append(mantissaString);
          } else if (cmp == 0) {
#if DEBUG
            if (!decimalPoint.CanFitInInt32()) {
   throw new
  ArgumentException("doesn't satisfy decimalPoint.CanFitInInt32()");
            }
            if (decimalPoint.AsInt32() != 0) {
    throw new
  ArgumentException("doesn't satisfy decimalPoint.AsInt32() == 0");
            }
#endif

            var tmpFast = new FastInteger(mantissaString.Length).AddInt(6);
            builder = new StringBuilder(tmpFast.CompareToInt(Int32.MaxValue) >
                    0 ? Int32.MaxValue : tmpFast.AsInt32());
            if (negative) {
              builder.Append('-');
            }
            builder.Append("0.");
            builder.Append(mantissaString);
          } else if (decimalPoint.CompareToInt(mantissaString.Length) > 0) {
            FastInteger insertionPoint = builderLength;
            if (!insertionPoint.CanFitInInt32()) {
              throw new NotSupportedException();
            }
            int tmpInt = insertionPoint.AsInt32();
            if (tmpInt < 0) {
              tmpInt = 0;
            }
            var tmpFast = new FastInteger(mantissaString.Length).AddInt(6);
            builder = new StringBuilder(tmpFast.CompareToInt(Int32.MaxValue) >
                    0 ? Int32.MaxValue : tmpFast.AsInt32());
            if (negative) {
              builder.Append('-');
            }
            builder.Append(mantissaString, 0, tmpInt);
            AppendString(
              builder,
              '0',
              decimalPoint.Copy().SubtractInt(builder.Length));
            builder.Append('.');
            builder.Append(
              mantissaString,
              tmpInt,
              mantissaString.Length - tmpInt);
          } else {
            if (!decimalPoint.CanFitInInt32()) {
              throw new NotSupportedException();
            }
            int tmpInt = decimalPoint.AsInt32();
            if (tmpInt < 0) {
              tmpInt = 0;
            }
            var tmpFast = new FastInteger(mantissaString.Length).AddInt(6);
            builder = new StringBuilder(tmpFast.CompareToInt(Int32.MaxValue) >
                    0 ? Int32.MaxValue : tmpFast.AsInt32());
            if (negative) {
              builder.Append('-');
            }
            builder.Append(mantissaString, 0, tmpInt);
            builder.Append('.');
            builder.Append(
              mantissaString,
              tmpInt,
              mantissaString.Length - tmpInt);
          }
          return builder.ToString();
        }
        if (mode == 2 && scaleSign < 0) {
          FastInteger negscale = thisExponent.Copy();
          builder = new StringBuilder();
          if (negative) {
            builder.Append('-');
          }
          builder.Append(mantissaString);
          AppendString(builder, '0', negscale);
          return builder.ToString();
        }
        return (!negative) ? mantissaString : ("-" + mantissaString);
      } else {
        if (mode == 1 && iszero && decimalPointAdjust.CompareToInt(1) > 0) {
          builder = new StringBuilder();
          if (negative) {
            builder.Append('-');
          }
          builder.Append(mantissaString);
          builder.Append('.');
          AppendString(
            builder,
            '0',
            decimalPointAdjust.Copy().Decrement());
        } else {
          FastInteger tmp = decimalPointAdjust.Copy();
          int cmp = tmp.CompareToInt(mantissaString.Length);
          if (cmp > 0) {
            tmp.SubtractInt(mantissaString.Length);
            builder = new StringBuilder();
            if (negative) {
              builder.Append('-');
            }
            builder.Append(mantissaString);
            AppendString(builder, '0', tmp);
          } else if (cmp < 0) {
            // Insert a decimal point at the right place
            if (!tmp.CanFitInInt32()) {
              throw new NotSupportedException();
            }
            int tmpInt = tmp.AsInt32();
            if (tmp.Sign < 0) {
              tmpInt = 0;
            }
            var tmpFast = new FastInteger(mantissaString.Length).AddInt(6);
            builder = new StringBuilder(tmpFast.CompareToInt(Int32.MaxValue) >
                    0 ? Int32.MaxValue : tmpFast.AsInt32());
            if (negative) {
              builder.Append('-');
            }
            builder.Append(mantissaString, 0, tmpInt);
            builder.Append('.');
            builder.Append(
              mantissaString,
              tmpInt,
              mantissaString.Length - tmpInt);
          } else if (adjustedExponent.Sign == 0 && !negative) {
            return mantissaString;
          } else if (adjustedExponent.Sign == 0 && negative) {
            return "-" + mantissaString;
          } else {
            builder = new StringBuilder();
            if (negative) {
              builder.Append('-');
            }
            builder.Append(mantissaString);
          }
        }
        if (adjustedExponent.Sign != 0) {
          builder.Append(adjustedExponent.Sign < 0 ? "E-" : "E+");
          adjustedExponent.Abs();
          var builderReversed = new StringBuilder();
          while (adjustedExponent.Sign != 0) {
            int digit =
              adjustedExponent.Copy().Remainder(10).AsInt32();
            // Each digit is retrieved from right to left
            builderReversed.Append((char)('0' + digit));
            adjustedExponent.Divide(10);
          }
          int count = builderReversed.Length;
          string builderReversedString = builderReversed.ToString();
          for (var i = 0; i < count; ++i) {
            builder.Append(builderReversedString[count - 1 - i]);
          }
        }
        return builder.ToString();
      }
    }

    private sealed class DecimalMathHelper : IRadixMathHelper<EDecimal> {
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DecimalMathHelper.GetRadix"]/*'/>
      public int GetRadix() {
        return 10;
      }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DecimalMathHelper.GetSign(PeterO.Numbers.EDecimal)"]/*'/>
      public int GetSign(EDecimal value) {
        return value.Sign;
      }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DecimalMathHelper.GetMantissa(PeterO.Numbers.EDecimal)"]/*'/>
      public EInteger GetMantissa(EDecimal value) {
        return value.unsignedMantissa.ToEInteger();
      }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DecimalMathHelper.GetExponent(PeterO.Numbers.EDecimal)"]/*'/>
      public EInteger GetExponent(EDecimal value) {
        return value.exponent.ToEInteger();
      }

      public FastIntegerFixed GetMantissaFastInt(EDecimal value) {
        return value.unsignedMantissa;
      }

      public FastIntegerFixed GetExponentFastInt(EDecimal value) {
        return value.exponent;
      }

      public IShiftAccumulator CreateShiftAccumulatorWithDigits(
        EInteger bigint,
        int lastDigit,
        int olderDigits) {
        return new DigitShiftAccumulator(bigint, lastDigit, olderDigits);
      }

      public IShiftAccumulator CreateShiftAccumulatorWithDigitsFastInt(
        FastIntegerFixed fastInt,
        int lastDigit,
        int olderDigits) {
        if (fastInt.CanFitInInt32()) {
   return new DigitShiftAccumulator(
  fastInt.AsInt32(),
  lastDigit,
  olderDigits);
        } else {
return new DigitShiftAccumulator(
  fastInt.ToEInteger(),
  lastDigit,
  olderDigits);
        }
      }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DecimalMathHelper.CreateShiftAccumulator(PeterO.Numbers.EInteger)"]/*'/>
      public IShiftAccumulator CreateShiftAccumulator(EInteger bigint) {
        return new DigitShiftAccumulator(bigint, 0, 0);
      }

    public FastInteger DivisionShift(
        EInteger num,
        EInteger den) {
        if (den.IsZero) {
          return null;
        }
        EInteger gcd = den.Gcd(EInteger.FromInt32(10));
        if (gcd.CompareTo(EInteger.One) == 0) {
          return null;
        }
        if (den.IsZero) {
          return null;
        }
        // Eliminate factors of 2
        int lowBit = den.GetLowBit();
        den >>= lowBit;
        // Eliminate factors of 5
        var fiveShift = new FastInteger(0);
        while (true) {
          EInteger bigrem;
          EInteger bigquo;
          {
            EInteger[] divrem = den.DivRem((EInteger)5);
            bigquo = divrem[0];
            bigrem = divrem[1];
          }
          if (!bigrem.IsZero) {
            break;
          }
          fiveShift.Increment();
          den = bigquo;
        }
        if (den.CompareTo(EInteger.One) != 0) {
          return null;
        }
        if (fiveShift.CompareToInt(lowBit) > 0) {
          return fiveShift;
        } else {
          return new FastInteger(lowBit);
        }
      }

      public EInteger MultiplyByRadixPower(
        EInteger bigint,
        FastInteger power) {
        EInteger tmpbigint = bigint;
        if (tmpbigint.IsZero) {
          return tmpbigint;
        }
        bool fitsInInt32 = power.CanFitInInt32();
        int powerInt = fitsInInt32 ? power.AsInt32() : 0;
        if (fitsInInt32 && powerInt == 0) {
          return tmpbigint;
        }
        EInteger bigtmp = null;
        if (tmpbigint.CompareTo(EInteger.One) != 0) {
          if (fitsInInt32) {
            if (powerInt <= 10) {
              bigtmp = NumberUtility.FindPowerOfTen(powerInt);
              tmpbigint *= (EInteger)bigtmp;
            } else {
              bigtmp = NumberUtility.FindPowerOfFive(powerInt);
              tmpbigint *= (EInteger)bigtmp;
              tmpbigint <<= powerInt;
            }
          } else {
            bigtmp = NumberUtility.FindPowerOfTenFromBig(power.AsEInteger());
            tmpbigint *= (EInteger)bigtmp;
          }
          return tmpbigint;
        }
        return fitsInInt32 ? NumberUtility.FindPowerOfTen(powerInt) :
          NumberUtility.FindPowerOfTenFromBig(power.AsEInteger());
      }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DecimalMathHelper.GetFlags(PeterO.Numbers.EDecimal)"]/*'/>
      public int GetFlags(EDecimal value) {
        return value.flags;
      }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DecimalMathHelper.CreateNewWithFlags(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger,System.Int32)"]/*'/>
      public EDecimal CreateNewWithFlags(
        EInteger mantissa,
        EInteger exponent,
        int flags) {
        return CreateWithFlags(
  FastIntegerFixed.FromBig(mantissa),
  FastIntegerFixed.FromBig(exponent),
  flags);
      }

      public EDecimal CreateNewWithFlagsFastInt(
        FastIntegerFixed fmantissa,
        FastIntegerFixed fexponent,
        int flags) {
        return CreateWithFlags(fmantissa, fexponent, flags);
      }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DecimalMathHelper.GetArithmeticSupport"]/*'/>
      public int GetArithmeticSupport() {
        return BigNumberFlags.FiniteAndNonFinite;
      }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.DecimalMathHelper.ValueOf(System.Int32)"]/*'/>
      public EDecimal ValueOf(int val) {
        return (val == 0) ? Zero : ((val == 1) ? One : FromInt64(val));
      }
    }

    // Begin integer conversions

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToByteChecked"]/*'/>
public byte ToByteChecked() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
if (this.IsIntegerPartZero()) {
 return (byte)0;
}
if (this.IsNegative) {
 throw new OverflowException("Value out of range");
}
if (this.exponent.CompareToInt(3) >= 0) {
throw new OverflowException("Value out of range: ");
}
 return this.ToEInteger().ToByteChecked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToByteUnchecked"]/*'/>
public byte ToByteUnchecked() {
 return this.IsFinite ? this.ToEInteger().ToByteUnchecked() : (byte)0;
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToByteIfExact"]/*'/>
public byte ToByteIfExact() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
 if (this.IsZero) {
 return (byte)0;
}
 if (this.IsNegative) {
throw new OverflowException("Value out of range");
}
if (this.exponent.CompareToInt(3) >= 0) {
throw new OverflowException("Value out of range");
}
 return this.ToEIntegerIfExact().ToByteChecked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.FromByte(System.Byte)"]/*'/>
public static EDecimal FromByte(byte inputByte) {
 int val = ((int)inputByte) & 0xff;
 return FromInt32(val);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToInt16Checked"]/*'/>
public short ToInt16Checked() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
if (this.IsIntegerPartZero()) {
 return (short)0;
}
if (this.exponent.CompareToInt(5) >= 0) {
throw new OverflowException("Value out of range: ");
}
 return this.ToEInteger().ToInt16Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToInt16Unchecked"]/*'/>
public short ToInt16Unchecked() {
 return this.IsFinite ? this.ToEInteger().ToInt16Unchecked() : (short)0;
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToInt16IfExact"]/*'/>
public short ToInt16IfExact() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
 if (this.IsZero) {
 return (short)0;
}
if (this.exponent.CompareToInt(5) >= 0) {
throw new OverflowException("Value out of range");
}
 return this.ToEIntegerIfExact().ToInt16Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.FromInt16(System.Int16)"]/*'/>
public static EDecimal FromInt16(short inputInt16) {
 var val = (int)inputInt16;
 return FromInt32(val);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToInt32Checked"]/*'/>
public int ToInt32Checked() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
if (this.IsIntegerPartZero()) {
 return (int)0;
}
if (this.exponent.CompareToInt(10) >= 0) {
throw new OverflowException("Value out of range: ");
}
 return this.ToEInteger().ToInt32Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToInt32Unchecked"]/*'/>
public int ToInt32Unchecked() {
 return this.IsFinite ? this.ToEInteger().ToInt32Unchecked() : (int)0;
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToInt32IfExact"]/*'/>
public int ToInt32IfExact() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
 if (this.IsZero) {
 return (int)0;
}
if (this.exponent.CompareToInt(10) >= 0) {
throw new OverflowException("Value out of range");
}
 return this.ToEIntegerIfExact().ToInt32Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToInt64Checked"]/*'/>
public long ToInt64Checked() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
if (this.IsIntegerPartZero()) {
 return (long)0;
}
if (this.exponent.CompareToInt(19) >= 0) {
throw new OverflowException("Value out of range: ");
}
 return this.ToEInteger().ToInt64Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToInt64Unchecked"]/*'/>
public long ToInt64Unchecked() {
 return this.IsFinite ? this.ToEInteger().ToInt64Unchecked() : (long)0;
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EDecimal.ToInt64IfExact"]/*'/>
public long ToInt64IfExact() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
 if (this.IsZero) {
 return (long)0;
}
if (this.exponent.CompareToInt(19) >= 0) {
throw new OverflowException("Value out of range");
}
 return this.ToEIntegerIfExact().ToInt64Checked();
}

// End integer conversions
  }
}
