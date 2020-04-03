/*
Written by Peter O. in 2013.
Any copyright is dedicated to the Public Domain.
http://creativecommons.org/publicdomain/zero/1.0/
If you like this, you should donate to Peter O.
at: http://peteroupc.github.io/
 */
using System;

namespace PeterO.Numbers {
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:PeterO.Numbers.EFloat"]/*'/>
  public sealed partial class EFloat : IComparable<EFloat>,
  IEquatable<EFloat> {
    //----------------------------------------------------------------

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.EFloat.NaN"]/*'/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "EFloat is immutable")]
    public static readonly EFloat NaN = CreateWithFlags(
      EInteger.Zero,
      EInteger.Zero,
      BigNumberFlags.FlagQuietNaN);

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.EFloat.NegativeInfinity"]/*'/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "EFloat is immutable")]
    public static readonly EFloat NegativeInfinity = CreateWithFlags(
      EInteger.Zero,
      EInteger.Zero,
      BigNumberFlags.FlagInfinity | BigNumberFlags.FlagNegative);

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.EFloat.NegativeZero"]/*'/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "EFloat is immutable")]
    public static readonly EFloat NegativeZero = CreateWithFlags(
      EInteger.Zero,
      EInteger.Zero,
      BigNumberFlags.FlagNegative);

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.EFloat.One"]/*'/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "EFloat is immutable")]
    public static readonly EFloat One =
      EFloat.Create(EInteger.One, EInteger.Zero);

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.EFloat.PositiveInfinity"]/*'/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "EFloat is immutable")]
    public static readonly EFloat PositiveInfinity = CreateWithFlags(
      EInteger.Zero,
      EInteger.Zero,
      BigNumberFlags.FlagInfinity);

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.EFloat.SignalingNaN"]/*'/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "EFloat is immutable")]
    public static readonly EFloat SignalingNaN = CreateWithFlags(
      EInteger.Zero,
      EInteger.Zero,
      BigNumberFlags.FlagSignalingNaN);

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.EFloat.Ten"]/*'/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "EFloat is immutable")]
    public static readonly EFloat Ten =
      EFloat.Create((EInteger)10, EInteger.Zero);

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.EFloat.Zero"]/*'/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "EFloat is immutable")]
    public static readonly EFloat Zero =
      EFloat.Create(EInteger.Zero, EInteger.Zero);
    //----------------------------------------------------------------
    private static readonly IRadixMath<EFloat> MathValue = new
      TrappableRadixMath<EFloat>(
        new ExtendedOrSimpleRadixMath<EFloat>(new BinaryMathHelper()));

    private readonly EInteger exponent;
    private readonly int flags;
    private readonly EInteger unsignedMantissa;

    private EFloat(
      EInteger unsignedMantissa,
      EInteger exponent,
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
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.EFloat.Exponent"]/*'/>
    public EInteger Exponent {
      get {
        return this.exponent;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.EFloat.IsFinite"]/*'/>
    public bool IsFinite {
      get {
        return (this.flags & (BigNumberFlags.FlagInfinity |
                    BigNumberFlags.FlagNaN)) == 0;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.EFloat.IsNegative"]/*'/>
    public bool IsNegative {
      get {
        return (this.flags & BigNumberFlags.FlagNegative) != 0;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.EFloat.IsZero"]/*'/>
    public bool IsZero {
      get {
        return ((this.flags & BigNumberFlags.FlagSpecial) == 0) &&
          this.unsignedMantissa.IsZero;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.EFloat.Mantissa"]/*'/>
    public EInteger Mantissa {
      get {
        return this.IsNegative ? (-(EInteger)this.unsignedMantissa) :
          this.unsignedMantissa;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.EFloat.Sign"]/*'/>
    public int Sign {
      get {
        return (((this.flags & BigNumberFlags.FlagSpecial) == 0) &&
                this.unsignedMantissa.IsZero) ? 0 :
          (((this.flags & BigNumberFlags.FlagNegative) != 0) ? -1 : 1);
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.EFloat.UnsignedMantissa"]/*'/>
    public EInteger UnsignedMantissa {
      get {
        return this.unsignedMantissa;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Create(System.Int32,System.Int32)"]/*'/>
    public static EFloat Create(int mantissaSmall, int exponentSmall) {
      return Create((EInteger)mantissaSmall, (EInteger)exponentSmall);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Create(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger)"]/*'/>
    public static EFloat Create(
      EInteger mantissa,
      EInteger exponent) {
      if (mantissa == null) {
        throw new ArgumentNullException(nameof(mantissa));
      }
      if (exponent == null) {
        throw new ArgumentNullException(nameof(exponent));
      }
      int sign = mantissa.Sign;
      return new EFloat(
        sign < 0 ? (-(EInteger)mantissa) : mantissa,
        exponent,
        (sign < 0) ? BigNumberFlags.FlagNegative : 0);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.CreateNaN(PeterO.Numbers.EInteger)"]/*'/>
    public static EFloat CreateNaN(EInteger diag) {
      return CreateNaN(diag, false, false, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.CreateNaN(PeterO.Numbers.EInteger,System.Boolean,System.Boolean,PeterO.Numbers.EContext)"]/*'/>
    public static EFloat CreateNaN(
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
        EFloat ef = CreateWithFlags(
          diag,
          EInteger.Zero,
          flags).RoundToPrecision(ctx);
        int newFlags = ef.flags;
        newFlags &= ~BigNumberFlags.FlagQuietNaN;
        newFlags |= signaling ? BigNumberFlags.FlagSignalingNaN :
          BigNumberFlags.FlagQuietNaN;
        return new EFloat(ef.unsignedMantissa, ef.exponent, newFlags);
      }
      flags |= signaling ? BigNumberFlags.FlagSignalingNaN :
        BigNumberFlags.FlagQuietNaN;
      return CreateWithFlags(diag, EInteger.Zero, flags);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.FromDouble(System.Double)"]/*'/>
    public static EFloat FromDouble(double dbl) {
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
        if (lvalue == 0) {
          return quiet ? NaN : SignalingNaN;
        }
        value[0] = (neg ? BigNumberFlags.FlagNegative : 0) |
       (quiet ? BigNumberFlags.FlagQuietNaN : BigNumberFlags.FlagSignalingNaN);
        return CreateWithFlags(
          EInteger.FromInt64(lvalue),
          EInteger.Zero,
          value[0]);
      }
      value[1] &= 0xfffff;  // Mask out the exponent and sign
      if (floatExponent == 0) {
        ++floatExponent;
      } else {
        value[1] |= 0x100000;
      }
      if ((value[1] | value[0]) != 0) {
        floatExponent += NumberUtility.ShiftAwayTrailingZerosTwoElements(value);
      } else {
        return neg ? EFloat.NegativeZero : EFloat.Zero;
      }
      lvalue = unchecked((value[0] & 0xffffffffL) | ((long)value[1] << 32));
      return CreateWithFlags(
        EInteger.FromInt64(lvalue),
        (EInteger)(floatExponent - 1075),
        neg ? BigNumberFlags.FlagNegative : 0);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.FromEInteger(PeterO.Numbers.EInteger)"]/*'/>
    public static EFloat FromEInteger(EInteger bigint) {
      return EFloat.Create(bigint, EInteger.Zero);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.FromSingle(System.Single)"]/*'/>
    public static EFloat FromSingle(float flt) {
      int value = BitConverter.ToInt32(BitConverter.GetBytes((float)flt), 0);
      bool neg = (value >> 31) != 0;
      var floatExponent = (int)((value >> 23) & 0xff);
      int valueFpMantissa = value & 0x7fffff;
      EInteger bigmant;
      if (floatExponent == 255) {
        if (valueFpMantissa == 0) {
          return neg ? NegativeInfinity : PositiveInfinity;
        }
        // Treat high bit of mantissa as quiet/signaling bit
        bool quiet = (valueFpMantissa & 0x400000) != 0;
        valueFpMantissa &= 0x3fffff;
        bigmant = (EInteger)valueFpMantissa;
        value = (neg ? BigNumberFlags.FlagNegative : 0) | (quiet ?
                BigNumberFlags.FlagQuietNaN : BigNumberFlags.FlagSignalingNaN);
        if (bigmant.IsZero) {
          return quiet ? NaN : SignalingNaN;
        }
        return CreateWithFlags(
          bigmant,
          EInteger.Zero,
          value);
      }
      if (floatExponent == 0) {
        ++floatExponent;
      } else {
        valueFpMantissa |= 1 << 23;
      }
      if (valueFpMantissa == 0) {
        return neg ? EFloat.NegativeZero : EFloat.Zero;
      }
      while ((valueFpMantissa & 1) == 0) {
        ++floatExponent;
        valueFpMantissa >>= 1;
      }
      if (neg) {
        valueFpMantissa = -valueFpMantissa;
      }
      bigmant = (EInteger)valueFpMantissa;
      return EFloat.Create(
        bigmant,
        (EInteger)(floatExponent - 150));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.FromString(System.String,System.Int32,System.Int32,PeterO.Numbers.EContext)"]/*'/>
    public static EFloat FromString(
      string str,
      int offset,
      int length,
      EContext ctx) {
      if (str == null) {
        throw new ArgumentNullException(nameof(str));
      }
      return EDecimal.FromString(
        str,
        offset,
        length,
        EContext.Unlimited.WithSimplified(ctx != null && ctx.IsSimplified))
        .ToEFloat(ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.FromString(System.String)"]/*'/>
    public static EFloat FromString(string str) {
      return FromString(str, 0, str == null ? 0 : str.Length, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.FromString(System.String,PeterO.Numbers.EContext)"]/*'/>
    public static EFloat FromString(string str, EContext ctx) {
      return FromString(str, 0, str == null ? 0 : str.Length, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.FromString(System.String,System.Int32,System.Int32)"]/*'/>
    public static EFloat FromString(string str, int offset, int length) {
      return FromString(str, offset, length, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Max(PeterO.Numbers.EFloat,PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public static EFloat Max(
      EFloat first,
      EFloat second,
      EContext ctx) {
      return MathValue.Max(first, second, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Max(PeterO.Numbers.EFloat,PeterO.Numbers.EFloat)"]/*'/>
    public static EFloat Max(
      EFloat first,
      EFloat second) {
      return Max(first, second, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.MaxMagnitude(PeterO.Numbers.EFloat,PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public static EFloat MaxMagnitude(
      EFloat first,
      EFloat second,
      EContext ctx) {
      return MathValue.MaxMagnitude(first, second, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.MaxMagnitude(PeterO.Numbers.EFloat,PeterO.Numbers.EFloat)"]/*'/>
    public static EFloat MaxMagnitude(
      EFloat first,
      EFloat second) {
      return MaxMagnitude(first, second, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Min(PeterO.Numbers.EFloat,PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public static EFloat Min(
      EFloat first,
      EFloat second,
      EContext ctx) {
      return MathValue.Min(first, second, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Min(PeterO.Numbers.EFloat,PeterO.Numbers.EFloat)"]/*'/>
    public static EFloat Min(
      EFloat first,
      EFloat second) {
      return Min(first, second, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.MinMagnitude(PeterO.Numbers.EFloat,PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public static EFloat MinMagnitude(
      EFloat first,
      EFloat second,
      EContext ctx) {
      return MathValue.MinMagnitude(first, second, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.MinMagnitude(PeterO.Numbers.EFloat,PeterO.Numbers.EFloat)"]/*'/>
    public static EFloat MinMagnitude(
      EFloat first,
      EFloat second) {
      return MinMagnitude(first, second, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.PI(PeterO.Numbers.EContext)"]/*'/>
    public static EFloat PI(EContext ctx) {
      return MathValue.Pi(ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Abs"]/*'/>
    public EFloat Abs() {
      if (this.IsNegative) {
        var er = new EFloat(
  this.unsignedMantissa,
  this.exponent,
  this.flags & ~BigNumberFlags.FlagNegative);
        return er;
      }
      return this;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Abs(PeterO.Numbers.EContext)"]/*'/>
    public EFloat Abs(EContext context) {
      return MathValue.Abs(this, context);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Add(System.Int32)"]/*'/>
public EFloat Add(int intValue) {
 return this.Add(EFloat.FromInt32(intValue));
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Subtract(System.Int32)"]/*'/>
public EFloat Subtract(int intValue) {
 return (intValue == Int32.MinValue) ?
   this.Subtract(EFloat.FromInt32(intValue)) : this.Add(-intValue);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Multiply(System.Int32)"]/*'/>
public EFloat Multiply(int intValue) {
 return this.Multiply(EFloat.FromInt32(intValue));
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Divide(System.Int32)"]/*'/>
public EFloat Divide(int intValue) {
 return this.Divide(EFloat.FromInt32(intValue));
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Add(PeterO.Numbers.EFloat)"]/*'/>
    public EFloat Add(EFloat otherValue) {
      return this.Add(otherValue, EContext.UnlimitedHalfEven);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Add(PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public EFloat Add(
      EFloat otherValue,
      EContext ctx) {
      return MathValue.Add(this, otherValue, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.CompareTo(PeterO.Numbers.EFloat)"]/*'/>
    public int CompareTo(EFloat other) {
      return MathValue.CompareTo(this, other);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.CompareToSignal(PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public EFloat CompareToSignal(
      EFloat other,
      EContext ctx) {
      return MathValue.CompareToWithContext(this, other, true, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.CompareToTotal(PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public int CompareToTotal(EFloat other, EContext ctx) {
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
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.CompareToTotal(PeterO.Numbers.EFloat)"]/*'/>
    public int CompareToTotal(EFloat other) {
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
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.CompareToTotalMagnitude(PeterO.Numbers.EFloat)"]/*'/>
    public int CompareToTotalMagnitude(EFloat other) {
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
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.CompareToWithContext(PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public EFloat CompareToWithContext(
      EFloat other,
      EContext ctx) {
      return MathValue.CompareToWithContext(this, other, false, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.CopySign(PeterO.Numbers.EFloat)"]/*'/>
    public EFloat CopySign(EFloat other) {
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
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Divide(PeterO.Numbers.EFloat)"]/*'/>
    public EFloat Divide(EFloat divisor) {
      return this.Divide(
        divisor,
        EContext.ForRounding(ERounding.None));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Divide(PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public EFloat Divide(
      EFloat divisor,
      EContext ctx) {
      return MathValue.Divide(this, divisor, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.DivideAndRemainderNaturalScale(PeterO.Numbers.EFloat)"]/*'/>
    [Obsolete("Renamed to DivRemNaturalScale.")]
    public EFloat[] DivideAndRemainderNaturalScale(EFloat
      divisor) {
      return this.DivRemNaturalScale(divisor, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.DivideAndRemainderNaturalScale(PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    [Obsolete("Renamed to DivRemNaturalScale.")]
    public EFloat[] DivideAndRemainderNaturalScale(
      EFloat divisor,
      EContext ctx) {
      return this.DivRemNaturalScale(divisor, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.DivideToExponent(PeterO.Numbers.EFloat,System.Int64,PeterO.Numbers.EContext)"]/*'/>
    public EFloat DivideToExponent(
      EFloat divisor,
      long desiredExponentSmall,
      EContext ctx) {
      return this.DivideToExponent(
        divisor,
        EInteger.FromInt64(desiredExponentSmall),
        ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.DivideToExponent(PeterO.Numbers.EFloat,System.Int64,PeterO.Numbers.ERounding)"]/*'/>
    public EFloat DivideToExponent(
      EFloat divisor,
      long desiredExponentSmall,
      ERounding rounding) {
      return this.DivideToExponent(
        divisor,
        EInteger.FromInt64(desiredExponentSmall),
        EContext.ForRounding(rounding));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.DivideToExponent(PeterO.Numbers.EFloat,PeterO.Numbers.EInteger,PeterO.Numbers.EContext)"]/*'/>
    public EFloat DivideToExponent(
      EFloat divisor,
      EInteger exponent,
      EContext ctx) {
      return MathValue.DivideToExponent(this, divisor, exponent, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.DivideToExponent(PeterO.Numbers.EFloat,PeterO.Numbers.EInteger,PeterO.Numbers.ERounding)"]/*'/>
    public EFloat DivideToExponent(
      EFloat divisor,
      EInteger desiredExponent,
      ERounding rounding) {
      return this.DivideToExponent(
        divisor,
        desiredExponent,
        EContext.ForRounding(rounding));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.DivideToIntegerNaturalScale(PeterO.Numbers.EFloat)"]/*'/>
    public EFloat DivideToIntegerNaturalScale(
      EFloat divisor) {
      return this.DivideToIntegerNaturalScale(
        divisor,
        EContext.ForRounding(ERounding.Down));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.DivideToIntegerNaturalScale(PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public EFloat DivideToIntegerNaturalScale(
      EFloat divisor,
      EContext ctx) {
      return MathValue.DivideToIntegerNaturalScale(this, divisor, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.DivideToIntegerZeroScale(PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public EFloat DivideToIntegerZeroScale(
      EFloat divisor,
      EContext ctx) {
      return MathValue.DivideToIntegerZeroScale(this, divisor, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.DivideToSameExponent(PeterO.Numbers.EFloat,PeterO.Numbers.ERounding)"]/*'/>
    public EFloat DivideToSameExponent(
      EFloat divisor,
      ERounding rounding) {
      return this.DivideToExponent(
        divisor,
        this.exponent,
        EContext.ForRounding(rounding));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.DivRemNaturalScale(PeterO.Numbers.EFloat)"]/*'/>
    public EFloat[] DivRemNaturalScale(EFloat divisor) {
      return this.DivRemNaturalScale(divisor, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.DivRemNaturalScale(PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public EFloat[] DivRemNaturalScale(
      EFloat divisor,
      EContext ctx) {
      var result = new EFloat[2];
      result[0] = this.DivideToIntegerNaturalScale(divisor, null);
      result[1] = this.Subtract(
        result[0].Multiply(divisor, null),
        ctx);
      result[0] = result[0].RoundToPrecision(ctx);
      return result;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Equals(PeterO.Numbers.EFloat)"]/*'/>
    public bool Equals(EFloat other) {
      return this.EqualsInternal(other);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Equals(System.Object)"]/*'/>
    public override bool Equals(object obj) {
      return this.EqualsInternal(obj as EFloat);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.EqualsInternal(PeterO.Numbers.EFloat)"]/*'/>
    public bool EqualsInternal(EFloat otherValue) {
      if (otherValue == null) {
        return false;
      }
      return this.exponent.Equals(otherValue.exponent) &&
        this.unsignedMantissa.Equals(otherValue.unsignedMantissa) &&
        this.flags == otherValue.flags;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Exp(PeterO.Numbers.EContext)"]/*'/>
    public EFloat Exp(EContext ctx) {
      return MathValue.Exp(this, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.GetHashCode"]/*'/>
    public override int GetHashCode() {
      var hashCode = 403796923;
      unchecked {
        hashCode += 403797019 * this.exponent.GetHashCode();
        hashCode += 403797059 * this.unsignedMantissa.GetHashCode();
        hashCode += 403797127 * this.flags;
      }
      return hashCode;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.IsInfinity"]/*'/>
    public bool IsInfinity() {
      return (this.flags & BigNumberFlags.FlagInfinity) != 0;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.IsNaN"]/*'/>
    public bool IsNaN() {
      return (this.flags & (BigNumberFlags.FlagQuietNaN |
                    BigNumberFlags.FlagSignalingNaN)) != 0;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.IsNegativeInfinity"]/*'/>
    public bool IsNegativeInfinity() {
      return (this.flags & (BigNumberFlags.FlagInfinity |
                    BigNumberFlags.FlagNegative)) ==
        (BigNumberFlags.FlagInfinity | BigNumberFlags.FlagNegative);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.IsPositiveInfinity"]/*'/>
    public bool IsPositiveInfinity() {
      return (this.flags & (BigNumberFlags.FlagInfinity |
                BigNumberFlags.FlagNegative)) == BigNumberFlags.FlagInfinity;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.IsQuietNaN"]/*'/>
    public bool IsQuietNaN() {
      return (this.flags & BigNumberFlags.FlagQuietNaN) != 0;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.IsSignalingNaN"]/*'/>
    public bool IsSignalingNaN() {
      return (this.flags & BigNumberFlags.FlagSignalingNaN) != 0;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Log(PeterO.Numbers.EContext)"]/*'/>
    public EFloat Log(EContext ctx) {
      return MathValue.Ln(this, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Log10(PeterO.Numbers.EContext)"]/*'/>
    public EFloat Log10(EContext ctx) {
      return MathValue.Log10(this, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.MovePointLeft(System.Int32)"]/*'/>
    public EFloat MovePointLeft(int places) {
      return this.MovePointLeft((EInteger)places, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.MovePointLeft(System.Int32,PeterO.Numbers.EContext)"]/*'/>
    public EFloat MovePointLeft(int places, EContext ctx) {
      return this.MovePointLeft((EInteger)places, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.MovePointLeft(PeterO.Numbers.EInteger)"]/*'/>
    public EFloat MovePointLeft(EInteger bigPlaces) {
      return this.MovePointLeft(bigPlaces, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.MovePointLeft(PeterO.Numbers.EInteger,PeterO.Numbers.EContext)"]/*'/>
    public EFloat MovePointLeft(
  EInteger bigPlaces,
  EContext ctx) {
      return (!this.IsFinite) ? this.RoundToPrecision(ctx) :
        this.MovePointRight(-(EInteger)bigPlaces, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.MovePointRight(System.Int32)"]/*'/>
    public EFloat MovePointRight(int places) {
      return this.MovePointRight((EInteger)places, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.MovePointRight(System.Int32,PeterO.Numbers.EContext)"]/*'/>
    public EFloat MovePointRight(int places, EContext ctx) {
      return this.MovePointRight((EInteger)places, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.MovePointRight(PeterO.Numbers.EInteger)"]/*'/>
    public EFloat MovePointRight(EInteger bigPlaces) {
      return this.MovePointRight(bigPlaces, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.MovePointRight(PeterO.Numbers.EInteger,PeterO.Numbers.EContext)"]/*'/>
    public EFloat MovePointRight(
  EInteger bigPlaces,
  EContext ctx) {
      if (!this.IsFinite) {
        return this.RoundToPrecision(ctx);
      }
      EInteger bigExp = this.Exponent;
      bigExp += bigPlaces;
      if (bigExp.Sign > 0) {
        EInteger mant = NumberUtility.ShiftLeft(
          this.unsignedMantissa,
          bigExp);
        return CreateWithFlags(
  mant,
  EInteger.Zero,
  this.flags).RoundToPrecision(ctx);
      }
      return CreateWithFlags(
        this.unsignedMantissa,
        bigExp,
        this.flags).RoundToPrecision(ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Multiply(PeterO.Numbers.EFloat)"]/*'/>
    public EFloat Multiply(EFloat otherValue) {
      if (this.IsFinite && otherValue.IsFinite) {
        EInteger exp = this.exponent.Add(otherValue.exponent);
        int newflags = otherValue.flags ^ this.flags;
        if (this.unsignedMantissa.CanFitInInt32() &&
          otherValue.unsignedMantissa.CanFitInInt32()) {
          int integerA = this.unsignedMantissa.ToInt32Unchecked();
          int integerB = otherValue.unsignedMantissa.ToInt32Unchecked();
          long longA = ((long)integerA) * ((long)integerB);
          return CreateWithFlags((EInteger)longA, exp, newflags);
        } else {
          EInteger eintA = this.unsignedMantissa.Multiply(
           otherValue.unsignedMantissa);
          return CreateWithFlags(eintA, exp, newflags);
        }
      }
      return this.Multiply(otherValue, EContext.UnlimitedHalfEven);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Multiply(PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public EFloat Multiply(
      EFloat op,
      EContext ctx) {
      return MathValue.Multiply(this, op, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.MultiplyAndAdd(PeterO.Numbers.EFloat,PeterO.Numbers.EFloat)"]/*'/>
    public EFloat MultiplyAndAdd(
      EFloat multiplicand,
      EFloat augend) {
      return this.MultiplyAndAdd(multiplicand, augend, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.MultiplyAndAdd(PeterO.Numbers.EFloat,PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public EFloat MultiplyAndAdd(
      EFloat op,
      EFloat augend,
      EContext ctx) {
      return MathValue.MultiplyAndAdd(this, op, augend, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.MultiplyAndSubtract(PeterO.Numbers.EFloat,PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public EFloat MultiplyAndSubtract(
      EFloat op,
      EFloat subtrahend,
      EContext ctx) {
      if (op == null) {
        throw new ArgumentNullException(nameof(op));
      }
      if (subtrahend == null) {
        throw new ArgumentNullException(nameof(subtrahend));
      }
      EFloat negated = subtrahend;
      if ((subtrahend.flags & BigNumberFlags.FlagNaN) == 0) {
        int newflags = subtrahend.flags ^ BigNumberFlags.FlagNegative;
        negated = CreateWithFlags(
          subtrahend.unsignedMantissa,
          subtrahend.exponent,
          newflags);
      }
      return MathValue.MultiplyAndAdd(this, op, negated, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Negate"]/*'/>
    public EFloat Negate() {
      return new EFloat(
  this.unsignedMantissa,
  this.exponent,
  this.flags ^ BigNumberFlags.FlagNegative);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Negate(PeterO.Numbers.EContext)"]/*'/>
    public EFloat Negate(EContext context) {
      return MathValue.Negate(this, context);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.NextMinus(PeterO.Numbers.EContext)"]/*'/>
    public EFloat NextMinus(EContext ctx) {
      return MathValue.NextMinus(this, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.NextPlus(PeterO.Numbers.EContext)"]/*'/>
    public EFloat NextPlus(EContext ctx) {
      return MathValue.NextPlus(this, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.NextToward(PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public EFloat NextToward(
      EFloat otherValue,
      EContext ctx) {
      return MathValue.NextToward(this, otherValue, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Plus(PeterO.Numbers.EContext)"]/*'/>
    public EFloat Plus(EContext ctx) {
      return MathValue.Plus(this, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Pow(PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public EFloat Pow(EFloat exponent, EContext ctx) {
      return MathValue.Power(this, exponent, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Pow(System.Int32,PeterO.Numbers.EContext)"]/*'/>
    public EFloat Pow(int exponentSmall, EContext ctx) {
      return this.Pow(EFloat.FromInt64(exponentSmall), ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Pow(System.Int32)"]/*'/>
    public EFloat Pow(int exponentSmall) {
      return this.Pow(EFloat.FromInt64(exponentSmall), null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Precision"]/*'/>
    public EInteger Precision() {
      if (!this.IsFinite) {
        return EInteger.Zero;
      }
      if (this.IsZero) {
        return EInteger.One;
      }
      int bitlen = this.unsignedMantissa.GetSignedBitLength();
      return (EInteger)bitlen;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Quantize(PeterO.Numbers.EInteger,PeterO.Numbers.EContext)"]/*'/>
    public EFloat Quantize(
      EInteger desiredExponent,
      EContext ctx) {
      return this.Quantize(
        EFloat.Create(EInteger.One, desiredExponent),
        ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Quantize(System.Int32,PeterO.Numbers.EContext)"]/*'/>
    public EFloat Quantize(
      int desiredExponentInt,
      EContext ctx) {
      return this.Quantize(
        EFloat.Create(EInteger.One, (EInteger)desiredExponentInt),
        ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Quantize(PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public EFloat Quantize(
      EFloat otherValue,
      EContext ctx) {
      return MathValue.Quantize(this, otherValue, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Reduce(PeterO.Numbers.EContext)"]/*'/>
    public EFloat Reduce(EContext ctx) {
      return MathValue.Reduce(this, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Remainder(PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public EFloat Remainder(
      EFloat divisor,
      EContext ctx) {
      return MathValue.Remainder(this, divisor, ctx, true);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Remainder(PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public EFloat RemainderNoRoundAfterDivide(
      EFloat divisor,
      EContext ctx) {
      return MathValue.Remainder(this, divisor, ctx, false);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.RemainderNaturalScale(PeterO.Numbers.EFloat)"]/*'/>
    public EFloat RemainderNaturalScale(
      EFloat divisor) {
      return this.RemainderNaturalScale(divisor, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.RemainderNaturalScale(PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public EFloat RemainderNaturalScale(
      EFloat divisor,
      EContext ctx) {
      return this.Subtract(
        this.DivideToIntegerNaturalScale(divisor, null).Multiply(divisor, null),
        ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.RemainderNear(PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public EFloat RemainderNear(
      EFloat divisor,
      EContext ctx) {
      return MathValue.RemainderNear(this, divisor, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.RoundToExponent(PeterO.Numbers.EInteger,PeterO.Numbers.EContext)"]/*'/>
    public EFloat RoundToExponent(
      EInteger exponent,
      EContext ctx) {
      return MathValue.RoundToExponentSimple(this, exponent, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.RoundToExponent(System.Int32,PeterO.Numbers.EContext)"]/*'/>
    public EFloat RoundToExponent(
      int exponentSmall,
      EContext ctx) {
      return this.RoundToExponent((EInteger)exponentSmall, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.RoundToExponentExact(PeterO.Numbers.EInteger,PeterO.Numbers.EContext)"]/*'/>
    public EFloat RoundToExponentExact(
      EInteger exponent,
      EContext ctx) {
      return MathValue.RoundToExponentExact(this, exponent, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.RoundToExponentExact(PeterO.Numbers.EInteger,PeterO.Numbers.ERounding)"]/*'/>
    public EFloat RoundToExponentExact(
      EInteger exponent,
      ERounding rounding) {
       return MathValue.RoundToExponentExact(
  this,
  exponent,
  EContext.Unlimited.WithRounding(rounding));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.RoundToExponentExact(System.Int32,PeterO.Numbers.EContext)"]/*'/>
    public EFloat RoundToExponentExact(
      int exponentSmall,
      EContext ctx) {
      return this.RoundToExponentExact((EInteger)exponentSmall, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.RoundToIntegerExact(PeterO.Numbers.EContext)"]/*'/>
    public EFloat RoundToIntegerExact(EContext ctx) {
      return MathValue.RoundToExponentExact(this, EInteger.Zero, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.RoundToIntegerNoRoundedFlag(PeterO.Numbers.EContext)"]/*'/>
    public EFloat RoundToIntegerNoRoundedFlag(EContext ctx) {
      return MathValue.RoundToExponentNoRoundedFlag(this, EInteger.Zero, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.RoundToIntegralExact(PeterO.Numbers.EContext)"]/*'/>
    [Obsolete("Renamed to RoundToIntegerExact.")]
    public EFloat RoundToIntegralExact(EContext ctx) {
      return MathValue.RoundToExponentExact(this, EInteger.Zero, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.RoundToIntegralNoRoundedFlag(PeterO.Numbers.EContext)"]/*'/>
    [Obsolete("Renamed to RoundToIntegerNoRoundedFlag.")]
    public EFloat RoundToIntegralNoRoundedFlag(EContext ctx) {
      return MathValue.RoundToExponentNoRoundedFlag(this, EInteger.Zero, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.RoundToPrecision(PeterO.Numbers.EContext)"]/*'/>
    public EFloat RoundToPrecision(EContext ctx) {
      return MathValue.RoundToPrecision(this, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ScaleByPowerOfTwo(System.Int32)"]/*'/>
    public EFloat ScaleByPowerOfTwo(int places) {
      return this.ScaleByPowerOfTwo((EInteger)places, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ScaleByPowerOfTwo(System.Int32,PeterO.Numbers.EContext)"]/*'/>
    public EFloat ScaleByPowerOfTwo(int places, EContext ctx) {
      return this.ScaleByPowerOfTwo((EInteger)places, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ScaleByPowerOfTwo(PeterO.Numbers.EInteger)"]/*'/>
    public EFloat ScaleByPowerOfTwo(EInteger bigPlaces) {
      return this.ScaleByPowerOfTwo(bigPlaces, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ScaleByPowerOfTwo(PeterO.Numbers.EInteger,PeterO.Numbers.EContext)"]/*'/>
    public EFloat ScaleByPowerOfTwo(
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
        bigExp,
        this.flags).RoundToPrecision(ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Sqrt(PeterO.Numbers.EContext)"]/*'/>
    public EFloat Sqrt(EContext ctx) {
      return MathValue.SquareRoot(this, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.SquareRoot(PeterO.Numbers.EContext)"]/*'/>
    [Obsolete("Renamed to Sqrt.")]
    public EFloat SquareRoot(EContext ctx) {
      return MathValue.SquareRoot(this, ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Subtract(PeterO.Numbers.EFloat)"]/*'/>
    public EFloat Subtract(EFloat otherValue) {
      return this.Subtract(otherValue, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Subtract(PeterO.Numbers.EFloat,PeterO.Numbers.EContext)"]/*'/>
    public EFloat Subtract(
      EFloat otherValue,
      EContext ctx) {
      if (otherValue == null) {
        throw new ArgumentNullException(nameof(otherValue));
      }
      EFloat negated = otherValue;
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
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToDouble"]/*'/>
    public double ToDouble() {
      if (this.IsPositiveInfinity()) {
        return Double.PositiveInfinity;
      }
      if (this.IsNegativeInfinity()) {
        return Double.NegativeInfinity;
      }
      if (this.IsNaN()) {
        int[] nan = { 0, 0x7ff00000 };
        if (this.IsNegative) {
          nan[1] |= unchecked((int)(1 << 31));
        }
        if (this.IsQuietNaN()) {
          nan[1] |= 0x80000;
        } else if (this.UnsignedMantissa.IsZero) {
          // Set the 0x40000 bit to keep the mantissa from
          // being zero if this is a signaling NaN
          nan[1] |= 0x40000;
        }
        if (!this.UnsignedMantissa.IsZero) {
          // Copy diagnostic information
          int[] words = FastInteger.GetLastWords(this.UnsignedMantissa, 2);
          nan[0] = words[0];
          nan[1] |= words[1] & 0x7ffff;
          if ((words[0] | (words[1] & 0x7ffff)) == 0 && !this.IsQuietNaN()) {
            // Set the 0x40000 bit to keep the mantissa from
            // being zero if this is a signaling NaN
            nan[1] |= 0x40000;
          }
        }
        return Extras.IntegersToDouble(nan);
      }
      EFloat thisValue = this.RoundToPrecision(EContext.Binary64);
      if (!thisValue.IsFinite) {
        return thisValue.ToDouble();
      }
      EInteger mant = thisValue.unsignedMantissa;
      if (thisValue.IsNegative && mant.IsZero) {
        return Extras.IntegersToDouble(new[] { 0, unchecked((int)(1 << 31)) });
      } else if (mant.IsZero) {
        return 0.0;
      }
      // DebugUtility.Log("-->" + (//
      // thisValue.unsignedMantissa.ToRadixString(2)) + ", " + (//
      // thisValue.exponent));
      int bitLength = mant.GetUnsignedBitLength();
      int expo = thisValue.exponent.ToInt32Checked();
      var subnormal = false;
      if (bitLength < 53) {
        int diff = 53 - bitLength;
        expo -= diff;
        if (expo < -1074) {
          // DebugUtility.Log("Diff changed from " + diff + " to " + (diff -
          // (-1074 - expo)));
          diff -= -1074 - expo;
          expo = -1074;
          subnormal = true;
        }
        mant <<= diff;
        bitLength += diff;
      }
      // DebugUtility.Log("2->" + (mant.ToRadixString(2)) + ", " + expo);
      int[] mantissaBits;
      mantissaBits = FastInteger.GetLastWords(mant, 2);
      // Clear the high bits where the exponent and sign are
      mantissaBits[1] &= 0xfffff;
      if (!subnormal) {
        int smallexponent = (expo + 1075) << 20;
        mantissaBits[1] |= smallexponent;
      }
      if (this.IsNegative) {
        mantissaBits[1] |= unchecked((int)(1 << 31));
      }
      return Extras.IntegersToDouble(mantissaBits);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToEDecimal"]/*'/>
    public EDecimal ToEDecimal() {
      return EDecimal.FromEFloat(this);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToEInteger"]/*'/>
    public EInteger ToEInteger() {
      return this.ToEIntegerInternal(false);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToEIntegerExact"]/*'/>
    [Obsolete("Renamed to ToEIntegerIfExact.")]
    public EInteger ToEIntegerExact() {
      return this.ToEIntegerInternal(true);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToEIntegerIfExact"]/*'/>
    public EInteger ToEIntegerIfExact() {
      return this.ToEIntegerInternal(true);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToEngineeringString"]/*'/>
    public string ToEngineeringString() {
      return this.ToEDecimal().ToEngineeringString();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToExtendedDecimal"]/*'/>
    [Obsolete("Renamed to ToEDecimal.")]
    public EDecimal ToExtendedDecimal() {
      return EDecimal.FromEFloat(this);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToPlainString"]/*'/>
    public string ToPlainString() {
      return this.ToEDecimal().ToPlainString();
    }

    private string ToDebugString() {
      return "[" + this.Mantissa.ToRadixString(2) +
        "," + this.Mantissa.GetUnsignedBitLength() +
        "," + this.Exponent + "]";
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToShortestString(PeterO.Numbers.EContext)"]/*'/>
    public string ToShortestString(EContext ctx) {
      if (ctx == null || !ctx.HasMaxPrecision) {
        return this.ToString();
      }
      if (this.IsNaN()) {
        return CreateNaN(
  this.UnsignedMantissa,
  this.IsSignalingNaN(),
  this.IsNegative,
  ctx).ToString();
      }
      if (this.IsInfinity()) {
        return this.RoundToPrecision(ctx).ToString();
      }
      EContext ctx2 = ctx.WithNoFlags();
      EFloat valueEfRnd = this.RoundToPrecision(ctx);
      if (valueEfRnd.IsInfinity()) {
        return valueEfRnd.ToString();
      }
      // NOTE: The original EFloat is converted to decimal,
      // not the rounded version, to avoid double rounding issues
      bool mantissaIsPowerOfTwo = this.unsignedMantissa.IsPowerOfTwo;
      EDecimal dec = this.ToEDecimal();
      if (ctx.Precision.CompareTo(EInteger.FromInt32(10)) >= 0) {
        // Preround the decimal so the significand has closer to the
        // number of decimal digits of the maximum possible
        // decimal significand, to speed up further rounding
        EInteger roundedPrec = ctx.Precision.ShiftRight(1).Add(
          EInteger.FromInt32(3));
        dec = dec.RoundToPrecision(
          ctx2.WithRounding(ERounding.Odd).WithBigPrecision(roundedPrec));
      }
      // int precision = dec.UnsignedMantissa.GetDigitCount();
      EInteger eprecision = EInteger.Zero;
      while (true) {
        EInteger nextPrecision = eprecision.Add(EInteger.One);
        EContext nextCtx = ctx2.WithBigPrecision(nextPrecision);
        EDecimal nextDec = dec.RoundToPrecision(nextCtx);
        EFloat newFloat = nextDec.ToEFloat(ctx2);
        if (newFloat.CompareTo(valueEfRnd) == 0) {
          if (mantissaIsPowerOfTwo) {
            nextPrecision = eprecision;
            nextCtx = ctx2.WithBigPrecision(nextPrecision);
            EDecimal nextDec2 = dec.RoundToPrecision(nextCtx);
            nextDec2 = nextDec2.NextPlus(nextCtx);
            newFloat = nextDec2.ToEFloat(ctx2);
            if (newFloat.CompareTo(valueEfRnd) == 0) {
              nextDec = nextDec2;
            }
          }
          return (nextDec.Exponent.Sign > 0 &&
              nextDec.Abs().CompareTo(EDecimal.FromInt32(10000000)) < 0) ?
                nextDec.ToPlainString() : nextDec.ToString();
        }
        eprecision = nextPrecision;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToSingle"]/*'/>
    public float ToSingle() {
      if (this.IsPositiveInfinity()) {
        return Single.PositiveInfinity;
      }
      if (this.IsNegativeInfinity()) {
        return Single.NegativeInfinity;
      }
      if (this.IsNaN()) {
        var nan = 0x7f800000;
        if (this.IsNegative) {
          nan |= unchecked((int)(1 << 31));
        }
        // IsQuietNaN(): the quiet bit for X86 at least
        // If signaling NaN and mantissa is 0: set 0x200000
        // bit to keep the mantissa from being zero
        if (this.IsQuietNaN()) {
          nan |= 0x400000;
        } else if (this.UnsignedMantissa.IsZero) {
          nan |= 0x200000;
        }
        if (!this.UnsignedMantissa.IsZero) {
          // Transfer diagnostic information
          EInteger bigdata = this.UnsignedMantissa % (EInteger)0x400000;
          var intData = (int)bigdata;
          nan |= intData;
          if (intData == 0 && !this.IsQuietNaN()) {
            nan |= 0x200000;
          }
        }
        return BitConverter.ToSingle(BitConverter.GetBytes(nan), 0);
      }
      EFloat thisValue = this.RoundToPrecision(EContext.Binary32);
      if (!thisValue.IsFinite) {
        return thisValue.ToSingle();
      }
      EInteger mant = thisValue.unsignedMantissa;
      if (thisValue.IsNegative && mant.IsZero) {
        return BitConverter.ToSingle(BitConverter.GetBytes((int)1 << 31), 0);
      } else if (mant.IsZero) {
        return 0.0f;
      }
      // DebugUtility.Log("-->" + (//
      // thisValue.unsignedMantissa.ToRadixString(2)) + ", " + (//
      // thisValue.exponent));
      int bitLength = mant.GetUnsignedBitLength();
      int expo = thisValue.exponent.ToInt32Checked();
      var subnormal = false;
      if (bitLength < 24) {
        int diff = 24 - bitLength;
        expo -= diff;
        if (expo < -149) {
          // DebugUtility.Log("Diff changed from " + diff + " to " + (diff -
          // (-149 - expo)));
          diff -= -149 - expo;
          expo = -149;
          subnormal = true;
        }
        mant <<= diff;
        bitLength += diff;
      }
      // DebugUtility.Log("2->" + (mant.ToRadixString(2)) + ", " + expo);
      int smallmantissa = ((int)mant.ToInt32Checked()) & 0x7fffff;
      if (!subnormal) {
          smallmantissa |= (expo + 150) << 23;
      }
      if (this.IsNegative) {
          smallmantissa |= 1 << 31;
      }
      return BitConverter.ToSingle(
          BitConverter.GetBytes((int)smallmantissa),
          0);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToString"]/*'/>
    public override string ToString() {
      return EDecimal.FromEFloat(this).ToString();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.Ulp"]/*'/>
    public EFloat Ulp() {
      return (!this.IsFinite) ? EFloat.One :
        EFloat.Create(EInteger.One, this.exponent);
    }

    internal static EFloat CreateWithFlags(
      EInteger mantissa,
      EInteger exponent,
      int flags) {
      if (mantissa == null) {
        throw new ArgumentNullException(nameof(mantissa));
      }
      if (exponent == null) {
        throw new ArgumentNullException(nameof(exponent));
      }
      int sign = mantissa == null ? 0 : mantissa.Sign;
      return new EFloat(
        sign < 0 ? (-(EInteger)mantissa) : mantissa,
        exponent,
        flags);
    }

    private EInteger ToEIntegerInternal(bool exact) {
      if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      if (this.IsZero) {
        return EInteger.Zero;
      }
      int expsign = this.Exponent.Sign;
      if (expsign == 0) {
        // Integer
        return this.Mantissa;
      }
      if (expsign > 0) {
        // Integer with trailing zeros
        EInteger curexp = this.Exponent;
        EInteger bigmantissa = this.Mantissa;
        if (bigmantissa.IsZero) {
          return bigmantissa;
        }
        bool neg = bigmantissa.Sign < 0;
        if (neg) {
          bigmantissa = -bigmantissa;
        }
        bigmantissa = NumberUtility.ShiftLeft(bigmantissa, curexp);
        if (neg) {
          bigmantissa = -bigmantissa;
        }
        return bigmantissa;
      } else {
        if (exact && !this.unsignedMantissa.IsEven) {
          // Mantissa is odd and will have to shift a nonzero
          // number of bits, so can't be an exact integer
          throw new ArithmeticException("Not an exact integer");
        }
        FastInteger bigexponent = FastInteger.FromBig(this.Exponent).Negate();
        EInteger bigmantissa = this.unsignedMantissa;
        var acc = new BitShiftAccumulator(bigmantissa, 0, 0);
        acc.ShiftRight(bigexponent);
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

    private sealed class BinaryMathHelper : IRadixMathHelper<EFloat> {
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.BinaryMathHelper.GetRadix"]/*'/>
      public int GetRadix() {
        return 2;
      }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.BinaryMathHelper.GetSign(PeterO.Numbers.EFloat)"]/*'/>
      public int GetSign(EFloat value) {
        return value.Sign;
      }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.BinaryMathHelper.GetMantissa(PeterO.Numbers.EFloat)"]/*'/>
      public EInteger GetMantissa(EFloat value) {
        return value.unsignedMantissa;
      }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.BinaryMathHelper.GetExponent(PeterO.Numbers.EFloat)"]/*'/>
      public EInteger GetExponent(EFloat value) {
        return value.exponent;
      }

      public FastIntegerFixed GetMantissaFastInt(EFloat value) {
        return FastIntegerFixed.FromBig(value.unsignedMantissa);
      }

      public FastIntegerFixed GetExponentFastInt(EFloat value) {
        return FastIntegerFixed.FromBig(value.exponent);
      }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.BinaryMathHelper.CreateShiftAccumulatorWithDigits(PeterO.Numbers.EInteger,System.Int32,System.Int32)"]/*'/>
      public IShiftAccumulator CreateShiftAccumulatorWithDigits(
        EInteger bigint,
        int lastDigit,
        int olderDigits) {
        return new BitShiftAccumulator(bigint, lastDigit, olderDigits);
      }

      public IShiftAccumulator CreateShiftAccumulatorWithDigitsFastInt(
        FastIntegerFixed fastInt,
        int lastDigit,
        int olderDigits) {
        if (fastInt.CanFitInInt32()) {
     return new BitShiftAccumulator(
  fastInt.AsInt32(),
  lastDigit,
  olderDigits);
        } else {
  return new BitShiftAccumulator(
  fastInt.ToEInteger(),
  lastDigit,
  olderDigits);
        }
      }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.BinaryMathHelper.CreateShiftAccumulator(PeterO.Numbers.EInteger)"]/*'/>
      public IShiftAccumulator CreateShiftAccumulator(EInteger bigint) {
        return new BitShiftAccumulator(bigint, 0, 0);
      }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.BinaryMathHelper.DivisionShift(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger)"]/*'/>
      public FastInteger DivisionShift(EInteger num, EInteger den) {
        if (den.IsZero) {
          return null;
        }
        if (den.GetUnsignedBit(0) && den.CompareTo(EInteger.One) != 0) {
          return null;
        }
        int lowBit = den.GetLowBit();
        den >>= lowBit;
        return den.Equals(EInteger.One) ? new FastInteger(lowBit) : null;
      }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.BinaryMathHelper.MultiplyByRadixPower(PeterO.Numbers.EInteger,PeterO.Numbers.FastInteger)"]/*'/>
      public EInteger MultiplyByRadixPower(
        EInteger bigint,
        FastInteger power) {
        EInteger tmpbigint = bigint;
        if (power.Sign <= 0) {
          return tmpbigint;
        }
        if (tmpbigint.Sign < 0) {
          tmpbigint = -tmpbigint;
          if (power.CanFitInInt32()) {
            tmpbigint = NumberUtility.ShiftLeftInt(tmpbigint, power.AsInt32());
            tmpbigint = -tmpbigint;
          } else {
            tmpbigint = NumberUtility.ShiftLeft(
              tmpbigint,
              power.AsEInteger());
            tmpbigint = -tmpbigint;
          }
          return tmpbigint;
        }
        return power.CanFitInInt32() ? NumberUtility.ShiftLeftInt(
          tmpbigint,
          power.AsInt32()) : NumberUtility.ShiftLeft(
          tmpbigint,
          power.AsEInteger());
      }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.BinaryMathHelper.GetFlags(PeterO.Numbers.EFloat)"]/*'/>
      public int GetFlags(EFloat value) {
        return value.flags;
      }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.BinaryMathHelper.CreateNewWithFlags(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger,System.Int32)"]/*'/>
      public EFloat CreateNewWithFlags(
        EInteger mantissa,
        EInteger exponent,
        int flags) {
        return EFloat.CreateWithFlags(mantissa, exponent, flags);
      }

      public EFloat CreateNewWithFlagsFastInt(
        FastIntegerFixed fmantissa,
        FastIntegerFixed fexponent,
        int flags) {
        return CreateWithFlags(
  fmantissa.ToEInteger(),
  fexponent.ToEInteger(),
  flags);
      }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.BinaryMathHelper.GetArithmeticSupport"]/*'/>
      public int GetArithmeticSupport() {
        return BigNumberFlags.FiniteAndNonFinite;
      }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.BinaryMathHelper.ValueOf(System.Int32)"]/*'/>
      public EFloat ValueOf(int val) {
        return FromInt64(val);
      }
    }
        // Begin integer conversions

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToByteChecked"]/*'/>
public byte ToByteChecked() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
return this.IsZero ? ((byte)0) : this.ToEInteger().ToByteChecked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToByteUnchecked"]/*'/>
public byte ToByteUnchecked() {
 return this.IsFinite ? this.ToEInteger().ToByteUnchecked() : (byte)0;
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToByteIfExact"]/*'/>
public byte ToByteIfExact() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
 return this.IsZero ? ((byte)0) : this.ToEIntegerIfExact().ToByteChecked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.FromByte(System.Byte)"]/*'/>
public static EFloat FromByte(byte inputByte) {
 int val = ((int)inputByte) & 0xff;
 return FromInt32(val);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToInt16Checked"]/*'/>
public short ToInt16Checked() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
return this.IsZero ? ((short)0) : this.ToEInteger().ToInt16Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToInt16Unchecked"]/*'/>
public short ToInt16Unchecked() {
 return this.IsFinite ? this.ToEInteger().ToInt16Unchecked() : (short)0;
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToInt16IfExact"]/*'/>
public short ToInt16IfExact() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
 return this.IsZero ? ((short)0) :
   this.ToEIntegerIfExact().ToInt16Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.FromInt16(System.Int16)"]/*'/>
public static EFloat FromInt16(short inputInt16) {
 var val = (int)inputInt16;
 return FromInt32(val);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToInt32Checked"]/*'/>
public int ToInt32Checked() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
return this.IsZero ? ((int)0) : this.ToEInteger().ToInt32Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToInt32Unchecked"]/*'/>
public int ToInt32Unchecked() {
 return this.IsFinite ? this.ToEInteger().ToInt32Unchecked() : (int)0;
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToInt32IfExact"]/*'/>
public int ToInt32IfExact() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
 return this.IsZero ? ((int)0) : this.ToEIntegerIfExact().ToInt32Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.FromInt32(System.Int32)"]/*'/>
public static EFloat FromInt32(int inputInt32) {
 return FromEInteger(EInteger.FromInt32(inputInt32));
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToInt64Checked"]/*'/>
public long ToInt64Checked() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
return this.IsZero ? ((long)0) : this.ToEInteger().ToInt64Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToInt64Unchecked"]/*'/>
public long ToInt64Unchecked() {
 return this.IsFinite ? this.ToEInteger().ToInt64Unchecked() : (long)0;
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToInt64IfExact"]/*'/>
public long ToInt64IfExact() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
 return this.IsZero ? ((long)0) : this.ToEIntegerIfExact().ToInt64Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.FromInt64(System.Int64)"]/*'/>
public static EFloat FromInt64(long inputInt64) {
 return FromEInteger(EInteger.FromInt64(inputInt64));
}

// End integer conversions
  }
}
