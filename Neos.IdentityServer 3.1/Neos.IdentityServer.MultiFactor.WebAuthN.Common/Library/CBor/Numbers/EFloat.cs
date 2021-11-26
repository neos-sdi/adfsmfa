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
  /// <summary>Represents an arbitrary-precision binary floating-point
  /// number. (The "E" stands for "extended", meaning that instances of
  /// this class can be values other than numbers proper, such as
  /// infinity and not-a-number.) Each number consists of an integer
  /// significand and an integer exponent, both arbitrary-precision. The
  /// value of the number equals significand * 2^exponent. This class
  /// also supports values for negative zero, not-a-number (NaN) values,
  /// and infinity.
  /// <para>Passing a signaling NaN to any arithmetic operation shown
  /// here will signal the flag FlagInvalid and return a quiet NaN, even
  /// if another operand to that operation is a quiet NaN, unless the
  /// operation's documentation expressly states that another result
  /// happens when a signaling NaN is passed to that operation.</para>
  /// <para>Passing a quiet NaN to any arithmetic operation shown here
  /// will return a quiet NaN, unless the operation's documentation
  /// expressly states that another result happens when a quiet NaN is
  /// passed to that operation.</para>
  /// <para>Unless noted otherwise, passing a null arbitrary-precision
  /// binary floating-point number argument to any method here will throw
  /// an exception.</para>
  /// <para>When an arithmetic operation signals the flag FlagInvalid,
  /// FlagOverflow, or FlagDivideByZero, it will not throw an exception
  /// too, unless the operation's trap is enabled in the arithmetic
  /// context (see EContext's Traps property).</para>
  /// <para>An arbitrary-precision binary floating-point number value can
  /// be serialized in one of the following ways:</para>
  /// <list>
  /// <item>By calling the toString() method. However, not all strings
  /// can be converted back to an arbitrary-precision binary
  /// floating-point number without loss, especially if the string has a
  /// fractional part.</item>
  /// <item>By calling the UnsignedMantissa, Exponent, and IsNegative
  /// properties, and calling the IsInfinity, IsQuietNaN, and
  /// IsSignalingNaN methods. The return values combined will uniquely
  /// identify a particular arbitrary-precision binary floating-point
  /// number value.</item></list>
  /// <para>If an operation requires creating an intermediate value that
  /// might be too big to fit in memory (or might require more than 2
  /// gigabytes of memory to store -- due to the current use of a 32-bit
  /// integer internally as a length), the operation may signal an
  /// invalid-operation flag and return not-a-number (NaN). In certain
  /// rare cases, the CompareTo method may throw OutOfMemoryException
  /// (called OutOfMemoryError in Java) in the same circumstances.</para>
  /// <para><b>Thread safety</b></para>
  /// <para>Instances of this class are immutable, so they are inherently
  /// safe for use by multiple threads. Multiple instances of this object
  /// with the same properties are interchangeable, so they should not be
  /// compared using the "==" operator (which might only check if each
  /// side of the operator is the same instance).</para>
  /// <para><b>Comparison considerations</b></para>
  /// <para>This class's natural ordering (under the CompareTo method) is
  /// not consistent with the Equals method. This means that two values
  /// that compare as equal under the CompareTo method might not be equal
  /// under the Equals method. The CompareTo method compares the
  /// mathematical values of the two instances passed to it (and
  /// considers two different NaN values as equal), while two instances
  /// with the same mathematical value, but different exponents, will be
  /// considered unequal under the Equals method.</para>
  /// <para><b>Security note</b></para>
  /// <para>It is not recommended to implement security-sensitive
  /// algorithms using the methods in this class, for several
  /// reasons:</para>
  /// <list>
  /// <item><c>EFloat</c> objects are immutable, so they can't be
  /// modified, and the memory they occupy is not guaranteed to be
  /// cleared in a timely fashion due to garbage collection. This is
  /// relevant for applications that use many-bit-long numbers as secret
  /// parameters.</item>
  /// <item>The methods in this class (especially those that involve
  /// arithmetic) are not guaranteed to be "constant-time"
  /// (non-data-dependent) for all relevant inputs. Certain attacks that
  /// involve encrypted communications have exploited the timing and
  /// other aspects of such communications to derive keying material or
  /// cleartext indirectly.</item></list>
  /// <para>Applications should instead use dedicated security libraries
  /// to handle big numbers in security-sensitive algorithms.</para>
  /// <para><b>Reproducibility note</b></para>
  /// <para>See the reproducibility note in the EDecimal class's
  /// documentation.</para></summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Design",
      "CA1036",
      Justification = "Awaiting advice at dotnet/dotnet-api-docs#2937.")]
  public sealed partial class EFloat : IComparable<EFloat>,
    IEquatable<EFloat> {
    //-----------------------------------------------
    private const int CacheFirst = -24;
    private const int CacheLast = 128;

    /// <summary>A not-a-number value.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Security",
        "CA2104",
        Justification = "EFloat is immutable")]
    public static readonly EFloat NaN = new EFloat(
      FastIntegerFixed.Zero,
      FastIntegerFixed.Zero,
      (byte)BigNumberFlags.FlagQuietNaN);

    /// <summary>Negative infinity, less than any other number.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Security",
        "CA2104",
        Justification = "EFloat is immutable")]
    public static readonly EFloat NegativeInfinity = new EFloat(
      FastIntegerFixed.Zero,
      FastIntegerFixed.Zero,
      (byte)(BigNumberFlags.FlagInfinity | BigNumberFlags.FlagNegative));

    /// <summary>Represents the number negative zero.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Security",
        "CA2104",
        Justification = "EFloat is immutable")]
    public static readonly EFloat NegativeZero = new EFloat(
      FastIntegerFixed.Zero,
      FastIntegerFixed.Zero,
      (byte)BigNumberFlags.FlagNegative);

    /// <summary>Represents the number 1.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Security",
        "CA2104",
        Justification = "EFloat is immutable")]
    public static readonly EFloat One = new EFloat(
      FastIntegerFixed.One,
      FastIntegerFixed.Zero,
      (byte)0);

    /// <summary>Positive infinity, greater than any other
    /// number.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Security",
        "CA2104",
        Justification = "EFloat is immutable")]
    public static readonly EFloat PositiveInfinity = new EFloat(
      FastIntegerFixed.Zero,
      FastIntegerFixed.Zero,
      (byte)BigNumberFlags.FlagInfinity);

    /// <summary>A not-a-number value that signals an invalid operation
    /// flag when it's passed as an argument to any arithmetic operation in
    /// arbitrary-precision binary floating-point number.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Security",
        "CA2104",
        Justification = "EFloat is immutable")]
    public static readonly EFloat SignalingNaN = new EFloat(
      FastIntegerFixed.Zero,
      FastIntegerFixed.Zero,
      (byte)BigNumberFlags.FlagSignalingNaN);

    /// <summary>Represents the number 10.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Security",
        "CA2104",
        Justification = "EFloat is immutable")]
    public static readonly EFloat Ten = new EFloat(
      FastIntegerFixed.FromInt32(10),
      FastIntegerFixed.Zero,
      (byte)0);

    /// <summary>Represents the number 0.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Security",
        "CA2104",
        Justification = "EFloat is immutable")]
    public static readonly EFloat Zero = new EFloat(
      FastIntegerFixed.Zero,
      FastIntegerFixed.Zero,
      (byte)0);

    private static readonly EFloat[] Cache = EFloatCache(CacheFirst,
        CacheLast);

    private static EFloat[] EFloatCache(int first, int last) {
      #if DEBUG
      if (first < -65535) {
        throw new ArgumentException("first (" + first + ") is not greater" +
          "\u0020or equal" + "\u0020to " + (-65535));
      }
      if (first > 65535) {
        throw new ArgumentException("first (" + first + ") is not less or" +
          "\u0020equal to" + "\u002065535");
      }
      if (last < -65535) {
        throw new ArgumentException("last (" + last + ") is not greater or" +
          "\u0020equal" + "\u0020to " + (-65535));
      }
      if (last > 65535) {
        throw new ArgumentException("last (" + last + ") is not less or" +
          "\u0020equal to" + "65535");
      }
      #endif

      var cache = new EFloat[(last - first) + 1];
      int i;
      for (i = first; i <= last; ++i) {
        if (i == 0) {
          cache[i - first] = Zero;
        } else if (i == 1) {
          cache[i - first] = One;
        } else if (i == 10) {
          cache[i - first] = Ten;
        } else {
          cache[i - first] = new EFloat(
            FastIntegerFixed.FromInt32(Math.Abs(i)),
            FastIntegerFixed.Zero,
            (byte)((i < 0) ? BigNumberFlags.FlagNegative : 0));
        }
      }
      return cache;
    }

    //----------------------------------------------------------------
    private static readonly IRadixMath<EFloat> MathValue = new
    TrappableRadixMath<EFloat>(
      new ExtendedOrSimpleRadixMath<EFloat>(new BinaryMathHelper()));

    internal static IRadixMath<EFloat> GetMathValue() {
      return MathValue;
    }

    private readonly FastIntegerFixed exponent;
    private readonly FastIntegerFixed unsignedMantissa;
    private readonly byte flags;

    private EFloat(
      FastIntegerFixed unsignedMantissa,
      FastIntegerFixed exponent,
      byte flags) {
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

    /// <summary>Gets this object's exponent. This object's value will be
    /// an integer if the exponent is positive or zero.</summary>
    /// <value>This object's exponent. This object's value will be an
    /// integer if the exponent is positive or zero.</value>
    public EInteger Exponent {
      get {
        return this.exponent.ToEInteger();
      }
    }

    /// <summary>Gets a value indicating whether this object is finite (not
    /// infinity or not-a-number, NaN).</summary>
    /// <value><c>true</c> if this object is finite (not infinity or
    /// not-a-number, NaN); otherwise, <c>false</c>.</value>
    public bool IsFinite {
      get {
        return (this.flags & (BigNumberFlags.FlagInfinity |
              BigNumberFlags.FlagNaN)) == 0;
      }
    }

    /// <summary>Gets a value indicating whether this object is negative,
    /// including negative zero.</summary>
    /// <value><c>true</c> if this object is negative, including negative
    /// zero; otherwise, <c>false</c>.</value>
    public bool IsNegative {
      get {
        return (this.flags & BigNumberFlags.FlagNegative) != 0;
      }
    }

    /// <summary>Gets a value indicating whether this object's value equals
    /// 0.</summary>
    /// <value><c>true</c> if this object's value equals 0; otherwise,
    /// <c>false</c>. <c>true</c> if this object's value equals 0;
    /// otherwise, <c>false</c>.</value>
    public bool IsZero {
      get {
        return ((this.flags & BigNumberFlags.FlagSpecial) == 0) &&
          this.unsignedMantissa.IsValueZero;
      }
    }

    /// <summary>Gets this object's unscaled value, or significand, and
    /// makes it negative if this object is negative. If this value is
    /// not-a-number (NaN), that value's absolute value is the NaN's
    /// "payload" (diagnostic information).</summary>
    /// <value>This object's unscaled value. Will be negative if this
    /// object's value is negative (including a negative NaN).</value>
    public EInteger Mantissa {
      get {
        return this.IsNegative ? this.unsignedMantissa.ToEInteger().Negate() :
          this.unsignedMantissa.ToEInteger();
      }
    }

    /// <summary>Gets this value's sign: -1 if negative; 1 if positive; 0
    /// if zero.</summary>
    /// <value>This value's sign: -1 if negative; 1 if positive; 0 if
    /// zero.</value>
    public int Sign {
      get {
        return (((this.flags & BigNumberFlags.FlagSpecial) == 0) &&
            this.unsignedMantissa.IsValueZero) ? 0 :
          (((this.flags & BigNumberFlags.FlagNegative) != 0) ? -1 : 1);
      }
    }

    /// <summary>Gets the absolute value of this object's unscaled value,
    /// or significand. If this value is not-a-number (NaN), that value is
    /// the NaN's "payload" (diagnostic information).</summary>
    /// <value>The absolute value of this object's unscaled value.</value>
    public EInteger UnsignedMantissa {
      get {
        return this.unsignedMantissa.ToEInteger();
      }
    }

    /// <summary>Creates a copy of this arbitrary-precision binary
    /// number.</summary>
    /// <returns>An arbitrary-precision binary floating-point
    /// number.</returns>
    public EFloat Copy() {
      return new EFloat(this.unsignedMantissa, this.exponent, this.flags);
    }

    /// <summary>Returns an arbitrary-precision number with the value
    /// <c>exponent*2^significand</c>.</summary>
    /// <param name='mantissaSmall'>Desired value for the
    /// significand.</param>
    /// <param name='exponentSmall'>Desired value for the exponent.</param>
    /// <returns>An arbitrary-precision binary number.</returns>
    public static EFloat Create(int mantissaSmall, int exponentSmall) {
      if (exponentSmall == 0 && mantissaSmall >= CacheFirst &&
        mantissaSmall <= CacheLast) {
        return Cache[mantissaSmall - CacheFirst];
      }
      if (mantissaSmall < 0) {
        if (mantissaSmall == Int32.MinValue) {
          FastIntegerFixed fi = FastIntegerFixed.FromInt64(Int32.MinValue);
          return new EFloat(
              fi.Negate(),
              FastIntegerFixed.FromInt32(exponentSmall),
              (byte)BigNumberFlags.FlagNegative);
        }
        return new EFloat(
            FastIntegerFixed.FromInt32(-mantissaSmall),
            FastIntegerFixed.FromInt32(exponentSmall),
            (byte)BigNumberFlags.FlagNegative);
      } else if (mantissaSmall == 0) {
        return new EFloat(
            FastIntegerFixed.Zero,
            FastIntegerFixed.FromInt32(exponentSmall),
            (byte)0);
      } else {
        return new EFloat(
            FastIntegerFixed.FromInt32(mantissaSmall),
            FastIntegerFixed.FromInt32(exponentSmall),
            (byte)0);
      }
    }

    /// <summary>Returns an arbitrary-precision number with the value
    /// <c>exponent*2^significand</c>.</summary>
    /// <param name='mantissa'>Desired value for the significand.</param>
    /// <param name='exponentSmall'>Desired value for the exponent.</param>
    /// <returns>An arbitrary-precision binary number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='mantissa'/> is null.</exception>
    public static EFloat Create(
      EInteger mantissa,
      int exponentSmall) {
      if (mantissa == null) {
        throw new ArgumentNullException(nameof(mantissa));
      }
      if (mantissa.CanFitInInt32()) {
        int mantissaSmall = mantissa.ToInt32Checked();
        return Create(mantissaSmall, exponentSmall);
      }
      FastIntegerFixed fi = FastIntegerFixed.FromBig(mantissa);
      int sign = fi.Sign;
      return new EFloat(
          sign < 0 ? fi.Negate() : fi,
          FastIntegerFixed.FromInt32(exponentSmall),
          (byte)((sign < 0) ? BigNumberFlags.FlagNegative : 0));
    }

    /// <summary>Returns an arbitrary-precision number with the value
    /// <c>exponent*2^significand</c>.</summary>
    /// <param name='mantissa'>Desired value for the significand.</param>
    /// <param name='exponentLong'>Desired value for the exponent.</param>
    /// <returns>An arbitrary-precision binary number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='mantissa'/> is null.</exception>
    public static EFloat Create(
      EInteger mantissa,
      long exponentLong) {
      if (mantissa == null) {
        throw new ArgumentNullException(nameof(mantissa));
      }
      if (mantissa.CanFitInInt64()) {
        long mantissaLong = mantissa.ToInt64Checked();
        return Create(mantissaLong, exponentLong);
      }
      FastIntegerFixed fi = FastIntegerFixed.FromBig(mantissa);
      int sign = fi.Sign;
      return new EFloat(
          sign < 0 ? fi.Negate() : fi,
          FastIntegerFixed.FromInt64(exponentLong),
          (byte)((sign < 0) ? BigNumberFlags.FlagNegative : 0));
    }

    /// <summary>Returns an arbitrary-precision number with the value
    /// <c>exponent*2^significand</c>.</summary>
    /// <param name='mantissa'>Desired value for the significand.</param>
    /// <param name='exponent'>Desired value for the exponent.</param>
    /// <returns>An arbitrary-precision binary number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='mantissa'/> or <paramref name='exponent'/> is
    /// null.</exception>
    public static EFloat Create(
      EInteger mantissa,
      EInteger exponent) {
      if (mantissa == null) {
        throw new ArgumentNullException(nameof(mantissa));
      }
      if (exponent == null) {
        throw new ArgumentNullException(nameof(exponent));
      }
      if (mantissa.CanFitInInt32() && exponent.IsZero) {
        int mantissaSmall = mantissa.ToInt32Checked();
        return Create(mantissaSmall, 0);
      }
      FastIntegerFixed fi = FastIntegerFixed.FromBig(mantissa);
      int sign = fi.Sign;
      return new EFloat(
          sign < 0 ? fi.Negate() : fi,
          FastIntegerFixed.FromBig(exponent),
          (byte)((sign < 0) ? BigNumberFlags.FlagNegative : 0));
    }

    /// <summary>Returns an arbitrary-precision number with the value
    /// <c>exponent*2^significand</c>.</summary>
    /// <param name='mantissaLong'>Desired value for the
    /// significand.</param>
    /// <param name='exponentSmall'>Desired value for the exponent.</param>
    /// <returns>An arbitrary-precision binary number.</returns>
    public static EFloat Create(
      long mantissaLong,
      int exponentSmall) {
      return Create(mantissaLong, (long)exponentSmall);
    }

    /// <summary>Returns an arbitrary-precision number with the value
    /// <c>exponent*2^significand</c>.</summary>
    /// <param name='mantissaLong'>Desired value for the
    /// significand.</param>
    /// <param name='exponentLong'>Desired value for the exponent.</param>
    /// <returns>An arbitrary-precision binary number.</returns>
    public static EFloat Create(
      long mantissaLong,
      long exponentLong) {
      if (mantissaLong >= Int32.MinValue && mantissaLong <= Int32.MaxValue &&
        exponentLong >= Int32.MinValue && exponentLong <= Int32.MaxValue) {
        return Create((int)mantissaLong, (int)exponentLong);
      } else if (mantissaLong == Int64.MinValue) {
        FastIntegerFixed fi = FastIntegerFixed.FromInt64(mantissaLong);
        return new EFloat(
            fi.Negate(),
            FastIntegerFixed.FromInt64(exponentLong),
            (byte)((mantissaLong < 0) ? BigNumberFlags.FlagNegative : 0));
      } else {
        FastIntegerFixed fi = FastIntegerFixed.FromInt64(Math.Abs(
              mantissaLong));
        return new EFloat(
            fi,
            FastIntegerFixed.FromInt64(exponentLong),
            (byte)((mantissaLong < 0) ? BigNumberFlags.FlagNegative : 0));
      }
    }

    /// <summary>Creates a not-a-number arbitrary-precision binary
    /// number.</summary>
    /// <param name='diag'>An integer, 0 or greater, to use as diagnostic
    /// information associated with this object. If none is needed, should
    /// be zero. To get the diagnostic information from another
    /// arbitrary-precision binary floating-point number, use that object's
    /// <c>UnsignedMantissa</c> property.</param>
    /// <returns>A quiet not-a-number.</returns>
    public static EFloat CreateNaN(EInteger diag) {
      return CreateNaN(diag, false, false, null);
    }

    /// <summary>Creates a not-a-number arbitrary-precision binary
    /// number.</summary>
    /// <param name='diag'>An integer, 0 or greater, to use as diagnostic
    /// information associated with this object. If none is needed, should
    /// be zero. To get the diagnostic information from another
    /// arbitrary-precision binary floating-point number, use that object's
    /// <c>UnsignedMantissa</c> property.</param>
    /// <param name='signaling'>Whether the return value will be signaling
    /// (true) or quiet (false).</param>
    /// <param name='negative'>Whether the return value is
    /// negative.</param>
    /// <param name='ctx'>An arithmetic context to control the precision
    /// (in binary digits) of the diagnostic information. The rounding and
    /// exponent range of this context will be ignored. Can be null. The
    /// only flag that can be signaled in this context is FlagInvalid,
    /// which happens if diagnostic information needs to be truncated and
    /// too much memory is required to do so.</param>
    /// <returns>An arbitrary-precision binary number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='diag'/> is null or is less than 0.</exception>
    public static EFloat CreateNaN(
      EInteger diag,
      bool signaling,
      bool negative,
      EContext ctx) {
      if (diag == null) {
        throw new ArgumentNullException(nameof(diag));
      }
      if (diag.Sign < 0) {
        throw new ArgumentException("Diagnostic information must be 0 or" +
          "\u0020greater," + "\u0020 was: " + diag);
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
        var ef = new EFloat(
          FastIntegerFixed.FromBig(diag),
          FastIntegerFixed.Zero,
          (byte)flags).RoundToPrecision(ctx);

        int newFlags = ef.flags;
        newFlags &= ~BigNumberFlags.FlagQuietNaN;
        newFlags |= signaling ? BigNumberFlags.FlagSignalingNaN :
          BigNumberFlags.FlagQuietNaN;
        return new EFloat(
            ef.unsignedMantissa,
            ef.exponent,
            (byte)newFlags);
      }
      flags |= signaling ? BigNumberFlags.FlagSignalingNaN :
        BigNumberFlags.FlagQuietNaN;
      return new EFloat(
          FastIntegerFixed.FromBig(diag),
          FastIntegerFixed.Zero,
          (byte)flags);
    }

    /// <summary>Creates a binary floating-point number from a 64-bit
    /// floating-point number encoded in the IEEE 754 binary64 format. This
    /// method computes the exact value of the floating point number, not
    /// an approximation, as is often the case by converting the floating
    /// point number to a string first.</summary>
    /// <param name='dblBits'>The parameter <paramref name='dblBits'/> is a
    /// 64-bit signed integer.</param>
    /// <returns>A binary floating-point number with the same value as the
    /// floating-point number encoded in <paramref
    /// name='dblBits'/>.</returns>
    public static EFloat FromDoubleBits(long dblBits) {
      var floatExponent = (int)((dblBits >> 52) & 0x7ff);
      bool neg = (dblBits >> 63) != 0;
      long lvalue;
      if (floatExponent == 2047) {
        if ((dblBits & ((1L << 52) - 1)) == 0) {
          return neg ? EFloat.NegativeInfinity : EFloat.PositiveInfinity;
        }
        // Treat high bit of mantissa as quiet/signaling bit
        bool quiet = ((dblBits >> 32) & 0x80000) != 0;
        lvalue = dblBits & ((1L << 51) - 1);
        if (lvalue == 0) {
          return quiet ? NaN : SignalingNaN;
        }
        int flags = (neg ? BigNumberFlags.FlagNegative : 0) |
          (quiet ? BigNumberFlags.FlagQuietNaN :
            BigNumberFlags.FlagSignalingNaN);
        return CreateWithFlags(
            EInteger.FromInt64(lvalue),
            EInteger.Zero,
            flags);
      }
      lvalue = dblBits & ((1L << 52) - 1); // Mask out the exponent and sign
      if (floatExponent == 0) {
        ++floatExponent;
      } else {
        lvalue |= 1L << 52;
      }
      if (lvalue != 0) {
        // Shift away trailing zeros
        while ((lvalue & 1L) == 0) {
          lvalue >>= 1;
          ++floatExponent;
        }
      } else {
        return neg ? EFloat.NegativeZero : EFloat.Zero;
      }
      return CreateWithFlags(
          EInteger.FromInt64(lvalue),
          (EInteger)(floatExponent - 1075),
          neg ? BigNumberFlags.FlagNegative : 0);
    }

    /// <summary>Creates a binary floating-point number from a 32-bit
    /// floating-point number. This method computes the exact value of the
    /// floating point number, not an approximation, as is often the case
    /// by converting the floating point number to a string
    /// first.</summary>
    /// <param name='flt'>The parameter <paramref name='flt'/> is a 64-bit
    /// floating-point number.</param>
    /// <returns>A binary floating-point number with the same value as
    /// <paramref name='flt'/>.</returns>
    public static EFloat FromSingle(float flt) {
      return FromSingleBits(
          BitConverter.ToInt32(BitConverter.GetBytes((float)flt), 0));
    }

    /// <summary>Creates a binary floating-point number from a 64-bit
    /// floating-point number. This method computes the exact value of the
    /// floating point number, not an approximation, as is often the case
    /// by converting the floating point number to a string
    /// first.</summary>
    /// <param name='dbl'>The parameter <paramref name='dbl'/> is a 64-bit
    /// floating-point number.</param>
    /// <returns>A binary floating-point number with the same value as
    /// <paramref name='dbl'/>.</returns>
    public static EFloat FromDouble(double dbl) {
      long lvalue = BitConverter.ToInt64(
          BitConverter.GetBytes((double)dbl),
          0);
      return FromDoubleBits(lvalue);
    }

    /// <summary>Converts an arbitrary-precision integer to the same value
    /// as a binary floating-point number.</summary>
    /// <param name='bigint'>An arbitrary-precision integer.</param>
    /// <returns>An arbitrary-precision binary floating-point
    /// number.</returns>
    public static EFloat FromEInteger(EInteger bigint) {
      return EFloat.Create(bigint, (int)0);
    }

    /// <summary>Creates a binary floating-point number from a 32-bit
    /// floating-point number encoded in the IEEE 754 binary32 format. This
    /// method computes the exact value of the floating point number, not
    /// an approximation, as is often the case by converting the floating
    /// point number to a string first.</summary>
    /// <param name='value'>A 32-bit binary floating-point number encoded
    /// in the IEEE 754 binary32 format.</param>
    /// <returns>A binary floating-point number with the same
    /// floating-point value as <paramref name='value'/>.</returns>
    public static EFloat FromSingleBits(int value) {
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

    internal static EFloat SignalUnderflow(EContext ec, bool negative, bool
      zeroSignificand) {
      EInteger eTiny = ec.EMin.Subtract(ec.Precision.Subtract(1));
      eTiny = eTiny.Subtract(2); // subtract 2 from proper eTiny to
      // trigger underflow (2, rather than 1, because of HalfUp mode)
      EFloat ret = EFloat.Create(
          zeroSignificand ? EInteger.Zero : EInteger.One,
          eTiny);
      if (negative) {
        ret = ret.Negate();
      }
      return ret.RoundToPrecision(ec);
    }

    internal static EFloat SignalOverflow(EContext ec, bool negative, bool
      zeroSignificand) {
      if (zeroSignificand) {
        EFloat ret = EFloat.Create(EInteger.Zero, ec.EMax);
        if (negative) {
          ret = ret.Negate();
        }
        return ret.RoundToPrecision(ec);
      } else {
        return MathValue.SignalOverflow(ec, negative);
      }
    }

    /// <summary>Creates a binary floating-point number from a text string
    /// that represents a number. Note that if the string contains a
    /// negative exponent, the resulting value might not be exact, in which
    /// case the resulting binary floating-point number will be an
    /// approximation of this decimal number's value.
    /// <para>The format of the string generally consists of:</para>
    /// <list type=''>
    /// <item>An optional plus sign ("+" , U+002B) or minus sign ("-",
    /// U+002D) (if '-' , the value is negative.)</item>
    /// <item>One or more digits, with a single optional decimal point
    /// (".", U+002E) before or after those digits or between two of them.
    /// These digits may begin with any number of zeros.</item>
    /// <item>Optionally, "E+"/"e+" (positive exponent) or "E-"/"e-"
    /// (negative exponent) plus one or more digits specifying the exponent
    /// (these digits may begin with any number of zeros).</item></list>
    /// <para>The string can also be "-INF", "-Infinity", "Infinity",
    /// "INF", quiet NaN ("NaN") followed by any number of digits (these
    /// digits may begin with any number of zeros), or signaling NaN
    /// ("sNaN") followed by any number of digits (these digits may begin
    /// with any number of zeros), all where the letters can be any
    /// combination of basic upper-case and/or basic lower-case
    /// letters.</para>
    /// <para>All characters mentioned above are the corresponding
    /// characters in the Basic Latin range. In particular, the digits must
    /// be the basic digits 0 to 9 (U+0030 to U+0039). The string is not
    /// allowed to contain white space characters, including
    /// spaces.</para></summary>
    /// <param name='str'>The parameter <paramref name='str'/> is a text
    /// string.</param>
    /// <param name='offset'>An index starting at 0 showing where the
    /// desired portion of <paramref name='str'/> begins.</param>
    /// <param name='length'>The length, in code units, of the desired
    /// portion of <paramref name='str'/> (but not more than <paramref
    /// name='str'/> 's length).</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If HasFlags of the
    /// context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited. Note that
    /// providing a context is often much faster than creating an EDecimal
    /// without a context then calling ToEFloat on that EDecimal,
    /// especially if the context specifies a precision limit and exponent
    /// range.</param>
    /// <returns>The parsed number, converted to arbitrary-precision binary
    /// floating-point number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='str'/> is null.</exception>
    /// <exception cref='FormatException'>The portion given of <paramref
    /// name='str'/> is not a correctly formatted number string; or either
    /// <paramref name='offset'/> or <paramref name='length'/> is less than
    /// 0 or greater than <paramref name='str'/> 's length, or <paramref
    /// name='str'/> 's length minus <paramref name='offset'/> is less than
    /// <paramref name='length'/>.</exception>
    public static EFloat FromString(
      string str,
      int offset,
      int length,
      EContext ctx) {
      return EFloatTextString.FromString(str, offset, length, ctx, true);
    }

    /// <summary>Creates a binary floating-point number from a text string
    /// that represents a number, using an unlimited precision context. For
    /// more information, see the <c>FromString(String, int, int,
    /// EContext)</c> method.</summary>
    /// <param name='str'>A text string to convert to a binary
    /// floating-point number.</param>
    /// <returns>The parsed number, converted to arbitrary-precision binary
    /// floating-point number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='str'/> is null.</exception>
    /// <exception cref='FormatException'>The portion given of <paramref
    /// name='str'/> is not a correctly formatted number
    /// string.</exception>
    public static EFloat FromString(string str) {
      return FromString(str, 0, str == null ? 0 : str.Length, null);
    }

    /// <summary>Creates a binary floating-point number from a text string
    /// that represents a number. For more information, see the
    /// <c>FromString(String, int, int, EContext)</c> method.</summary>
    /// <param name='str'>A text string to convert to a binary
    /// floating-point number.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If HasFlags of the
    /// context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited. Note that
    /// providing a context is often much faster than creating an EDecimal
    /// without a context then calling ToEFloat on that EDecimal,
    /// especially if the context specifies a precision limit and exponent
    /// range.</param>
    /// <returns>The parsed number, converted to arbitrary-precision binary
    /// floating-point number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='str'/> is null.</exception>
    public static EFloat FromString(string str, EContext ctx) {
      return FromString(str, 0, str == null ? 0 : str.Length, ctx);
    }

    /// <summary>Creates a binary floating-point number from a text string
    /// that represents a number. For more information, see the
    /// <c>FromString(String, int, int, EContext)</c> method.</summary>
    /// <param name='str'>The parameter <paramref name='str'/> is a text
    /// string.</param>
    /// <param name='offset'>An index starting at 0 showing where the
    /// desired portion of <paramref name='str'/> begins.</param>
    /// <param name='length'>The length, in code units, of the desired
    /// portion of <paramref name='str'/> (but not more than <paramref
    /// name='str'/> 's length).</param>
    /// <returns>An arbitrary-precision binary floating-point
    /// number.</returns>
    /// <exception cref=' T:System.ArgumentException'>Either <paramref
    /// name=' offset'/> or <paramref name=' length'/> is less than 0 or
    /// greater than <paramref name='str'/> 's length, or <paramref name='
    /// str'/> 's length minus <paramref name='offset'/> is less than
    /// <paramref name='length'/>.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='str'/> is null.</exception>
    /// <exception cref='ArgumentException'>Either <paramref
    /// name='offset'/> or <paramref name='length'/> is less than 0 or
    /// greater than <paramref name='str'/> 's length, or <paramref
    /// name='str'/> 's length minus <paramref name='offset'/> is less than
    /// <paramref name='length'/>.</exception>
    public static EFloat FromString(string str, int offset, int length) {
      return FromString(str, offset, length, null);
    }

    /// <summary>Creates a binary floating-point number from a sequence of
    /// <c>char</c> s that represents a number. Note that if the sequence
    /// contains a negative exponent, the resulting value might not be
    /// exact, in which case the resulting binary floating-point number
    /// will be an approximation of this decimal number's value.
    /// <para>The format of the sequence generally consists of:</para>
    /// <list type=''>
    /// <item>An optional plus sign ("+" , U+002B) or minus sign ("-",
    /// U+002D) (if '-' , the value is negative.)</item>
    /// <item>One or more digits, with a single optional decimal point
    /// (".", U+002E) before or after those digits or between two of them.
    /// These digits may begin with any number of zeros.</item>
    /// <item>Optionally, "E+"/"e+" (positive exponent) or "E-"/"e-"
    /// (negative exponent) plus one or more digits specifying the exponent
    /// (these digits may begin with any number of zeros).</item></list>
    /// <para>The sequence can also be "-INF", "-Infinity", "Infinity",
    /// "INF", quiet NaN ("NaN") followed by any number of digits (these
    /// digits may begin with any number of zeros), or signaling NaN
    /// ("sNaN") followed by any number of digits (these digits may begin
    /// with any number of zeros), all where the letters can be any
    /// combination of basic upper-case and/or basic lower-case
    /// letters.</para>
    /// <para>All characters mentioned above are the corresponding
    /// characters in the Basic Latin range. In particular, the digits must
    /// be the basic digits 0 to 9 (U+0030 to U+0039). The sequence is not
    /// allowed to contain white space characters, including
    /// spaces.</para></summary>
    /// <param name='chars'>A sequence of <c>char</c> s to convert to a
    /// binary floating-point number.</param>
    /// <param name='offset'>An index starting at 0 showing where the
    /// desired portion of <paramref name='chars'/> begins.</param>
    /// <param name='length'>The length, in code units, of the desired
    /// portion of <paramref name='chars'/> (but not more than <paramref
    /// name='chars'/> 's length).</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If HasFlags of the
    /// context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited. Note that
    /// providing a context is often much faster than creating an EDecimal
    /// without a context then calling ToEFloat on that EDecimal,
    /// especially if the context specifies a precision limit and exponent
    /// range.</param>
    /// <returns>The parsed number, converted to arbitrary-precision binary
    /// floating-point number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='chars'/> is null.</exception>
    /// <exception cref='FormatException'>The portion given of <paramref
    /// name='chars'/> is not a correctly formatted number sequence; or
    /// either <paramref name='offset'/> or <paramref name='length'/> is
    /// less than 0 or greater than <paramref name='chars'/> 's length, or
    /// <paramref name='chars'/> 's length minus <paramref name='offset'/>
    /// is less than <paramref name='length'/>.</exception>
    public static EFloat FromString(
      char[] chars,
      int offset,
      int length,
      EContext ctx) {
      return EFloatCharArrayString.FromString(chars, offset, length, ctx, true);
    }

    /// <summary>Creates a binary floating-point number from a sequence of
    /// <c>char</c> s that represents a number, using an unlimited
    /// precision context. For more information, see the
    /// <c>FromString(String, int, int, EContext)</c> method.</summary>
    /// <param name='chars'>A sequence of <c>char</c> s to convert to a
    /// binary floating-point number.</param>
    /// <returns>The parsed number, converted to arbitrary-precision binary
    /// floating-point number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='chars'/> is null.</exception>
    /// <exception cref='FormatException'>The portion given of <paramref
    /// name='chars'/> is not a correctly formatted number
    /// sequence.</exception>
    public static EFloat FromString(char[] chars) {
      return FromString(chars, 0, chars == null ? 0 : chars.Length, null);
    }

    /// <summary>Creates a binary floating-point number from a sequence of
    /// <c>char</c> s that represents a number. For more information, see
    /// the <c>FromString(String, int, int, EContext)</c> method.</summary>
    /// <param name='chars'>A sequence of <c>char</c> s to convert to a
    /// binary floating-point number.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If HasFlags of the
    /// context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited. Note that
    /// providing a context is often much faster than creating an EDecimal
    /// without a context then calling ToEFloat on that EDecimal,
    /// especially if the context specifies a precision limit and exponent
    /// range.</param>
    /// <returns>The parsed number, converted to arbitrary-precision binary
    /// floating-point number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='chars'/> is null.</exception>
    public static EFloat FromString(char[] chars, EContext ctx) {
      return FromString(chars, 0, chars == null ? 0 : chars.Length, ctx);
    }

    /// <summary>Creates a binary floating-point number from a sequence of
    /// <c>char</c> s that represents a number. For more information, see
    /// the <c>FromString(String, int, int, EContext)</c> method.</summary>
    /// <param name='chars'>A sequence of <c>char</c> s to convert to a
    /// binary floating-point number.</param>
    /// <param name='offset'>An index starting at 0 showing where the
    /// desired portion of <paramref name='chars'/> begins.</param>
    /// <param name='length'>The length, in code units, of the desired
    /// portion of <paramref name='chars'/> (but not more than <paramref
    /// name='chars'/> 's length).</param>
    /// <returns>An arbitrary-precision binary floating-point
    /// number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='chars'/> is null.</exception>
    /// <exception cref='ArgumentException'>Either <paramref
    /// name='offset'/> or <paramref name='length'/> is less than 0 or
    /// greater than <paramref name='chars'/> 's length, or <paramref
    /// name='chars'/> 's length minus <paramref name='offset'/> is less
    /// than <paramref name='length'/>.</exception>
    public static EFloat FromString(char[] chars, int offset, int length) {
      return FromString(chars, offset, length, null);
    }

    /// <summary>Creates a binary floating-point number from a sequence of
    /// bytes that represents a number. Note that if the sequence contains
    /// a negative exponent, the resulting value might not be exact, in
    /// which case the resulting binary floating-point number will be an
    /// approximation of this decimal number's value.
    /// <para>The format of the sequence generally consists of:</para>
    /// <list type=''>
    /// <item>An optional plus sign ("+" , U+002B) or minus sign ("-",
    /// U+002D) (if '-' , the value is negative.)</item>
    /// <item>One or more digits, with a single optional decimal point
    /// (".", U+002E) before or after those digits or between two of them.
    /// These digits may begin with any number of zeros.</item>
    /// <item>Optionally, "E+"/"e+" (positive exponent) or "E-"/"e-"
    /// (negative exponent) plus one or more digits specifying the exponent
    /// (these digits may begin with any number of zeros).</item></list>
    /// <para>The sequence can also be "-INF", "-Infinity", "Infinity",
    /// "INF", quiet NaN ("NaN") followed by any number of digits (these
    /// digits may begin with any number of zeros), or signaling NaN
    /// ("sNaN") followed by any number of digits (these digits may begin
    /// with any number of zeros), all where the letters can be any
    /// combination of basic upper-case and/or basic lower-case
    /// letters.</para>
    /// <para>All characters mentioned above are the corresponding
    /// characters in the Basic Latin range. In particular, the digits must
    /// be the basic digits 0 to 9 (U+0030 to U+0039). The sequence is not
    /// allowed to contain white space characters, including
    /// spaces.</para></summary>
    /// <param name='bytes'>A sequence of bytes to convert to a binary
    /// floating-point number.</param>
    /// <param name='offset'>An index starting at 0 showing where the
    /// desired portion of <paramref name='bytes'/> begins.</param>
    /// <param name='length'>The length, in code units, of the desired
    /// portion of <paramref name='bytes'/> (but not more than <paramref
    /// name='bytes'/> 's length).</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If HasFlags of the
    /// context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited. Note that
    /// providing a context is often much faster than creating an EDecimal
    /// without a context then calling ToEFloat on that EDecimal,
    /// especially if the context specifies a precision limit and exponent
    /// range.</param>
    /// <returns>The parsed number, converted to arbitrary-precision binary
    /// floating-point number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bytes'/> is null.</exception>
    /// <exception cref='FormatException'>The portion given of <paramref
    /// name='bytes'/> is not a correctly formatted number sequence; or
    /// either <paramref name='offset'/> or <paramref name='length'/> is
    /// less than 0 or greater than <paramref name='bytes'/> 's length, or
    /// <paramref name='bytes'/> 's length minus <paramref name='offset'/>
    /// is less than <paramref name='length'/>.</exception>
    public static EFloat FromString(
      byte[] bytes,
      int offset,
      int length,
      EContext ctx) {
      return EFloatByteArrayString.FromString(bytes, offset, length, ctx, true);
    }

    /// <summary>Creates a binary floating-point number from a sequence of
    /// bytes that represents a number, using an unlimited precision
    /// context. For more information, see the <c>FromString(String, int,
    /// int, EContext)</c> method.</summary>
    /// <param name='bytes'>A sequence of bytes to convert to a binary
    /// floating-point number.</param>
    /// <returns>The parsed number, converted to arbitrary-precision binary
    /// floating-point number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bytes'/> is null.</exception>
    /// <exception cref='FormatException'>The portion given of <paramref
    /// name='bytes'/> is not a correctly formatted number
    /// sequence.</exception>
    public static EFloat FromString(byte[] bytes) {
      return FromString(bytes, 0, bytes == null ? 0 : bytes.Length, null);
    }

    /// <summary>Creates a binary floating-point number from a sequence of
    /// bytes that represents a number. For more information, see the
    /// <c>FromString(String, int, int, EContext)</c> method.</summary>
    /// <param name='bytes'>A sequence of bytes to convert to a binary
    /// floating-point number.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If HasFlags of the
    /// context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited. Note that
    /// providing a context is often much faster than creating an EDecimal
    /// without a context then calling ToEFloat on that EDecimal,
    /// especially if the context specifies a precision limit and exponent
    /// range.</param>
    /// <returns>The parsed number, converted to arbitrary-precision binary
    /// floating-point number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bytes'/> is null.</exception>
    public static EFloat FromString(byte[] bytes, EContext ctx) {
      return FromString(bytes, 0, bytes == null ? 0 : bytes.Length, ctx);
    }

    /// <summary>Creates a binary floating-point number from a sequence of
    /// bytes that represents a number. For more information, see the
    /// <c>FromString(String, int, int, EContext)</c> method.</summary>
    /// <param name='bytes'>A sequence of bytes to convert to a binary
    /// floating-point number.</param>
    /// <param name='offset'>An index starting at 0 showing where the
    /// desired portion of <paramref name='bytes'/> begins.</param>
    /// <param name='length'>The length, in code units, of the desired
    /// portion of <paramref name='bytes'/> (but not more than <paramref
    /// name='bytes'/> 's length).</param>
    /// <returns>An arbitrary-precision binary floating-point
    /// number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bytes'/> is null.</exception>
    /// <exception cref='ArgumentException'>Either <paramref
    /// name='offset'/> or <paramref name='length'/> is less than 0 or
    /// greater than <paramref name='bytes'/> 's length, or <paramref
    /// name='bytes'/> 's length minus <paramref name='offset'/> is less
    /// than <paramref name='length'/>.</exception>
    public static EFloat FromString(byte[] bytes, int offset, int length) {
      return FromString(bytes, offset, length, null);
    }

    /// <summary>Gets the greater value between two binary floating-point
    /// numbers.</summary>
    /// <param name='first'>The first value to compare.</param>
    /// <param name='second'>The second value to compare.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed.</param>
    /// <returns>The larger value of the two numbers. If one is positive
    /// zero and the other is negative zero, returns the positive zero. If
    /// the two numbers are positive and have the same value, returns the
    /// one with the larger exponent. If the two numbers are negative and
    /// have the same value, returns the one with the smaller
    /// exponent.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='first'/> or <paramref name='second'/> is null.</exception>
    public static EFloat Max(
      EFloat first,
      EFloat second,
      EContext ctx) {
      if (first == null) {
        throw new ArgumentNullException(nameof(first));
      }
      if (second == null) {
        throw new ArgumentNullException(nameof(second));
      }
      return MathValue.Max(first, second, ctx);
    }

    /// <summary>Gets the greater value between two binary floating-point
    /// numbers.</summary>
    /// <param name='first'>The first value to compare.</param>
    /// <param name='second'>The second value to compare.</param>
    /// <returns>The larger value of the two numbers. If one is positive
    /// zero and the other is negative zero, returns the positive zero. If
    /// the two numbers are positive and have the same value, returns the
    /// one with the larger exponent. If the two numbers are negative and
    /// have the same value, returns the one with the smaller
    /// exponent.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='first'/> or <paramref name='second'/> is null.</exception>
    public static EFloat Max(
      EFloat first,
      EFloat second) {
      if (first == null) {
        throw new ArgumentNullException(nameof(first));
      }
      if (second == null) {
        throw new ArgumentNullException(nameof(second));
      }
      return Max(first, second, null);
    }

    /// <summary>Gets the greater value between two values, ignoring their
    /// signs. If the absolute values are equal, has the same effect as
    /// Max.</summary>
    /// <param name='first'>The first value to compare.</param>
    /// <param name='second'>The second value to compare.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed.</param>
    /// <returns>The larger value of the two numbers, ignoring their
    /// signs.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='first'/> or <paramref name='second'/> is null.</exception>
    public static EFloat MaxMagnitude(
      EFloat first,
      EFloat second,
      EContext ctx) {
      if (first == null) {
        throw new ArgumentNullException(nameof(first));
      }
      if (second == null) {
        throw new ArgumentNullException(nameof(second));
      }
      return MathValue.MaxMagnitude(first, second, ctx);
    }

    /// <summary>Gets the greater value between two values, ignoring their
    /// signs. If the absolute values are equal, has the same effect as
    /// Max.</summary>
    /// <param name='first'>The first value to compare.</param>
    /// <param name='second'>The second value to compare.</param>
    /// <returns>The larger value of the two numbers, ignoring their
    /// signs.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='first'/> or <paramref name='second'/> is null.</exception>
    public static EFloat MaxMagnitude(
      EFloat first,
      EFloat second) {
      if (first == null) {
        throw new ArgumentNullException(nameof(first));
      }
      if (second == null) {
        throw new ArgumentNullException(nameof(second));
      }
      return MaxMagnitude(first, second, null);
    }

    /// <summary>Gets the lesser value between two binary floating-point
    /// numbers.</summary>
    /// <param name='first'>The first value to compare.</param>
    /// <param name='second'>The second value to compare.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed.</param>
    /// <returns>The smaller value of the two numbers. If one is positive
    /// zero and the other is negative zero, returns the negative zero. If
    /// the two numbers are positive and have the same value, returns the
    /// one with the smaller exponent. If the two numbers are negative and
    /// have the same value, returns the one with the larger
    /// exponent.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='first'/> or <paramref name='second'/> is null.</exception>
    public static EFloat Min(
      EFloat first,
      EFloat second,
      EContext ctx) {
      if (first == null) {
        throw new ArgumentNullException(nameof(first));
      }
      if (second == null) {
        throw new ArgumentNullException(nameof(second));
      }
      return MathValue.Min(first, second, ctx);
    }

    /// <summary>Gets the lesser value between two binary floating-point
    /// numbers.</summary>
    /// <param name='first'>The first value to compare.</param>
    /// <param name='second'>The second value to compare.</param>
    /// <returns>The smaller value of the two numbers. If one is positive
    /// zero and the other is negative zero, returns the negative zero. If
    /// the two numbers are positive and have the same value, returns the
    /// one with the smaller exponent. If the two numbers are negative and
    /// have the same value, returns the one with the larger
    /// exponent.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='first'/> or <paramref name='second'/> is null.</exception>
    public static EFloat Min(
      EFloat first,
      EFloat second) {
      if (first == null) {
        throw new ArgumentNullException(nameof(first));
      }
      if (second == null) {
        throw new ArgumentNullException(nameof(second));
      }
      return Min(first, second, null);
    }

    /// <summary>Gets the lesser value between two values, ignoring their
    /// signs. If the absolute values are equal, has the same effect as
    /// Min.</summary>
    /// <param name='first'>The first value to compare.</param>
    /// <param name='second'>The second value to compare.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed.</param>
    /// <returns>The smaller value of the two numbers, ignoring their
    /// signs.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='first'/> or <paramref name='second'/> is null.</exception>
    public static EFloat MinMagnitude(
      EFloat first,
      EFloat second,
      EContext ctx) {
      if (first == null) {
        throw new ArgumentNullException(nameof(first));
      }
      if (second == null) {
        throw new ArgumentNullException(nameof(second));
      }
      return MathValue.MinMagnitude(first, second, ctx);
    }

    /// <summary>Gets the lesser value between two values, ignoring their
    /// signs. If the absolute values are equal, has the same effect as
    /// Min.</summary>
    /// <param name='first'>The first value to compare.</param>
    /// <param name='second'>The second value to compare.</param>
    /// <returns>The smaller value of the two numbers, ignoring their
    /// signs.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='first'/> or <paramref name='second'/> is null.</exception>
    public static EFloat MinMagnitude(
      EFloat first,
      EFloat second) {
      if (first == null) {
        throw new ArgumentNullException(nameof(first));
      }
      if (second == null) {
        throw new ArgumentNullException(nameof(second));
      }
      return MinMagnitude(first, second, null);
    }

    /// <summary>Finds the constant π, the circumference of a circle
    /// divided by its diameter.</summary>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// <i>This parameter can't be null, as π can never be represented
    /// exactly.</i>.</param>
    /// <returns>The constant π rounded to the given precision. Signals
    /// FlagInvalid and returns not-a-number (NaN) if the parameter
    /// <paramref name='ctx'/> is null or the precision is unlimited (the
    /// context's Precision property is 0).</returns>
    public static EFloat PI(EContext ctx) {
      return MathValue.Pi(ctx);
    }

    /// <summary>Finds the absolute value of this object (if it's negative,
    /// it becomes positive).</summary>
    /// <returns>An arbitrary-precision binary floating-point number.
    /// Returns signaling NaN if this value is signaling NaN. (In this
    /// sense, this method is similar to the "copy-abs" operation in the
    /// General Decimal Arithmetic Specification, except this method does
    /// not necessarily return a copy of this object.).</returns>
    public EFloat Abs() {
      if (this.IsNegative) {
        var er = new EFloat(
          this.unsignedMantissa,
          this.exponent,
          (byte)(this.flags & ~BigNumberFlags.FlagNegative));
        return er;
      }
      return this;
    }

    /// <summary>Finds the absolute value of this object (if it's negative,
    /// it becomes positive).</summary>
    /// <param name='context'>An arithmetic context to control the
    /// precision, rounding, and exponent range of the result. If
    /// <c>HasFlags</c> of the context is true, will also store the flags
    /// resulting from the operation (the flags are in addition to the
    /// pre-existing flags). Can be null, in which case the precision is
    /// unlimited and no rounding is needed.</param>
    /// <returns>The absolute value of this object. Signals FlagInvalid and
    /// returns quiet NaN if this value is signaling NaN.</returns>
    public EFloat Abs(EContext context) {
      return MathValue.Abs(this, context);
    }

    /// <summary>Adds this arbitrary-precision binary floating-point number
    /// and a 32-bit signed integer and returns the result. The exponent
    /// for the result is the lower of this arbitrary-precision binary
    /// floating-point number's exponent and the other 32-bit signed
    /// integer's exponent.</summary>
    /// <param name='intValue'>The parameter <paramref name='intValue'/> is
    /// a 32-bit signed integer.</param>
    /// <returns>The sum of the two numbers, that is, this
    /// arbitrary-precision binary floating-point number plus a 32-bit
    /// signed integer. If this arbitrary-precision binary floating-point
    /// number is not-a-number (NaN), returns NaN.</returns>
    public EFloat Add(int intValue) {
      return this.Add(EFloat.FromInt32(intValue));
    }

    /// <summary>Subtracts a 32-bit signed integer from this
    /// arbitrary-precision binary floating-point number and returns the
    /// result. The exponent for the result is the lower of this
    /// arbitrary-precision binary floating-point number's exponent and the
    /// other 32-bit signed integer's exponent.</summary>
    /// <param name='intValue'>The parameter <paramref name='intValue'/> is
    /// a 32-bit signed integer.</param>
    /// <returns>The difference between the two numbers, that is, this
    /// arbitrary-precision binary floating-point number minus a 32-bit
    /// signed integer. If this arbitrary-precision binary floating-point
    /// number is not-a-number (NaN), returns NaN.</returns>
    public EFloat Subtract(int intValue) {
      return (intValue == Int32.MinValue) ?
        this.Subtract(EFloat.FromInt32(intValue)) : this.Add(-intValue);
    }

    /// <summary>Multiplies this arbitrary-precision binary floating-point
    /// number by a 32-bit signed integer and returns the result. The
    /// exponent for the result is this arbitrary-precision binary
    /// floating-point number's exponent plus the other 32-bit signed
    /// integer's exponent.</summary>
    /// <param name='intValue'>The parameter <paramref name='intValue'/> is
    /// a 32-bit signed integer.</param>
    /// <returns>The product of the two numbers, that is, this
    /// arbitrary-precision binary floating-point number times a 32-bit
    /// signed integer.</returns>
    /// <example>
    /// <code>EInteger result = EInteger.FromString("5").Multiply(200);</code>
    ///  .
    /// </example>
    public EFloat Multiply(int intValue) {
      return this.Multiply(EFloat.FromInt32(intValue));
    }

    /// <summary>Divides this arbitrary-precision binary floating-point
    /// number by a 32-bit signed integer and returns the result; returns
    /// NaN instead if the result would have a nonterminating binary
    /// expansion (including 1/3, 1/12, 1/7, 2/3, and so on); if this is
    /// not desired, use DivideToExponent, or use the Divide overload that
    /// takes an EContext.</summary>
    /// <param name='intValue'>The divisor.</param>
    /// <returns>The result of dividing this arbitrary-precision binary
    /// floating-point number by a 32-bit signed integer. Returns infinity
    /// if the divisor (this arbitrary-precision binary floating-point
    /// number) is 0 and the dividend (the other 32-bit signed integer) is
    /// nonzero. Returns not-a-number (NaN) if the divisor and the dividend
    /// are 0. Returns NaN if the result can't be exact because it would
    /// have a nonterminating binary expansion (examples include 1 divided
    /// by any multiple of 3, such as 1/3 or 1/12). If this is not desired,
    /// use DivideToExponent instead, or use the Divide overload that takes
    /// an <c>EContext</c> (such as <c>EContext.Binary64</c> )
    /// instead.</returns>
    /// <exception cref='DivideByZeroException'>Attempted to divide by
    /// zero.</exception>
    public EFloat Divide(int intValue) {
      return this.Divide(EFloat.FromInt32(intValue));
    }

    /// <summary>Adds this arbitrary-precision binary floating-point number
    /// and a 64-bit signed integer and returns the result. The exponent
    /// for the result is the lower of this arbitrary-precision binary
    /// floating-point number's exponent and the other 64-bit signed
    /// integer's exponent.</summary>
    /// <param name='longValue'>The parameter <paramref name='longValue'/>
    /// is a 64-bit signed integer.</param>
    /// <returns>The sum of the two numbers, that is, this
    /// arbitrary-precision binary floating-point number plus a 64-bit
    /// signed integer. If this arbitrary-precision binary floating-point
    /// number is not-a-number (NaN), returns NaN.</returns>
    public EFloat Add(long longValue) {
      return this.Add(EFloat.FromInt64(longValue));
    }

    /// <summary>Subtracts a 64-bit signed integer from this
    /// arbitrary-precision binary floating-point number and returns the
    /// result. The exponent for the result is the lower of this
    /// arbitrary-precision binary floating-point number's exponent and the
    /// other 64-bit signed integer's exponent.</summary>
    /// <param name='longValue'>The parameter <paramref name='longValue'/>
    /// is a 64-bit signed integer.</param>
    /// <returns>The difference between the two numbers, that is, this
    /// arbitrary-precision binary floating-point number minus a 64-bit
    /// signed integer. If this arbitrary-precision binary floating-point
    /// number is not-a-number (NaN), returns NaN.</returns>
    public EFloat Subtract(long longValue) {
      return this.Subtract(EFloat.FromInt64(longValue));
    }

    /// <summary>Multiplies this arbitrary-precision binary floating-point
    /// number by a 64-bit signed integer and returns the result. The
    /// exponent for the result is this arbitrary-precision binary
    /// floating-point number's exponent plus the other 64-bit signed
    /// integer's exponent.</summary>
    /// <param name='longValue'>The parameter <paramref name='longValue'/>
    /// is a 64-bit signed integer.</param>
    /// <returns>The product of the two numbers, that is, this
    /// arbitrary-precision binary floating-point number times a 64-bit
    /// signed integer.</returns>
    /// <example>
    /// <code>EInteger result = EInteger.FromString("5").Multiply(200L);</code>
    ///  .
    /// </example>
    public EFloat Multiply(long longValue) {
      return this.Multiply(EFloat.FromInt64(longValue));
    }

    /// <summary>Divides this arbitrary-precision binary floating-point
    /// number by a 64-bit signed integer and returns the result; returns
    /// NaN instead if the result would have a nonterminating binary
    /// expansion (including 1/3, 1/12, 1/7, 2/3, and so on); if this is
    /// not desired, use DivideToExponent, or use the Divide overload that
    /// takes an EContext.</summary>
    /// <param name='longValue'>The parameter <paramref name='longValue'/>
    /// is a 64-bit signed integer.</param>
    /// <returns>The result of dividing this arbitrary-precision binary
    /// floating-point number by a 64-bit signed integer. Returns infinity
    /// if the divisor (this arbitrary-precision binary floating-point
    /// number) is 0 and the dividend (the other 64-bit signed integer) is
    /// nonzero. Returns not-a-number (NaN) if the divisor and the dividend
    /// are 0. Returns NaN if the result can't be exact because it would
    /// have a nonterminating binary expansion (examples include 1 divided
    /// by any multiple of 3, such as 1/3 or 1/12). If this is not desired,
    /// use DivideToExponent instead, or use the Divide overload that takes
    /// an <c>EContext</c> (such as <c>EContext.Binary64</c> )
    /// instead.</returns>
    /// <exception cref='DivideByZeroException'>Attempted to divide by
    /// zero.</exception>
    public EFloat Divide(long longValue) {
      return this.Divide(EFloat.FromInt64(longValue));
    }

    /// <summary>Adds this arbitrary-precision binary floating-point number
    /// and another arbitrary-precision binary floating-point number and
    /// returns the result. The exponent for the result is the lower of
    /// this arbitrary-precision binary floating-point number's exponent
    /// and the other arbitrary-precision binary floating-point number's
    /// exponent.</summary>
    /// <param name='otherValue'>An arbitrary-precision binary
    /// floating-point number.</param>
    /// <returns>The sum of the two numbers, that is, this
    /// arbitrary-precision binary floating-point number plus another
    /// arbitrary-precision binary floating-point number. If this
    /// arbitrary-precision binary floating-point number is not-a-number
    /// (NaN), returns NaN.</returns>
    public EFloat Add(EFloat otherValue) {
      return this.Add(otherValue, EContext.UnlimitedHalfEven);
    }

    /// <summary>Adds this arbitrary-precision binary floating-point number
    /// and another arbitrary-precision binary floating-point number and
    /// returns the result.</summary>
    /// <param name='otherValue'>The number to add to.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and no
    /// rounding is needed.</param>
    /// <returns>The sum of the two numbers, that is, this
    /// arbitrary-precision binary floating-point number plus another
    /// arbitrary-precision binary floating-point number. If this
    /// arbitrary-precision binary floating-point number is not-a-number
    /// (NaN), returns NaN.</returns>
    public EFloat Add(
      EFloat otherValue,
      EContext ctx) {
      return MathValue.Add(this, otherValue, ctx);
    }

    /// <summary>Compares the mathematical values of this object and
    /// another object, accepting NaN values. This method currently uses
    /// the rules given in the CompareToValue method, so that it it is not
    /// consistent with the Equals method, but it may change in a future
    /// version to use the rules for the CompareToTotal method
    /// instead.</summary>
    /// <param name='other'>An arbitrary-precision binary floating-point
    /// number.</param>
    /// <returns>Less than 0 if this object's value is less than the other
    /// value, or greater than 0 if this object's value is greater than the
    /// other value or if <paramref name='other'/> is null, or 0 if both
    /// values are equal.</returns>
    public int CompareTo(EFloat other) {
      return MathValue.CompareTo(this, other);
    }

    /// <summary>Compares the mathematical values of this object and
    /// another object, accepting NaN values.
    /// <para>This method is not consistent with the Equals method because
    /// two different numbers with the same mathematical value, but
    /// different exponents, will compare as equal.</para>
    /// <para>In this method, negative zero and positive zero are
    /// considered equal.</para>
    /// <para>If this object or the other object is a quiet NaN or
    /// signaling NaN, this method will not trigger an error. Instead, NaN
    /// will compare greater than any other number, including infinity. Two
    /// different NaN values will be considered equal.</para></summary>
    /// <param name='other'>An arbitrary-precision binary floating-point
    /// number.</param>
    /// <returns>Less than 0 if this object's value is less than the other
    /// value, or greater than 0 if this object's value is greater than the
    /// other value or if <paramref name='other'/> is null, or 0 if both
    /// values are equal.</returns>
    public int CompareToValue(EFloat other) {
      return MathValue.CompareTo(this, other);
    }

    /// <summary>Compares the mathematical values of this object and
    /// another object, accepting NaN values. This method currently uses
    /// the rules given in the CompareToValue method, so that it it is not
    /// consistent with the Equals method, but it may change in a future
    /// version to use the rules for the CompareToTotal method
    /// instead.</summary>
    /// <param name='intOther'>The parameter <paramref name='intOther'/> is
    /// a 32-bit signed integer.</param>
    /// <returns>Less than 0 if this object's value is less than the other
    /// value, or greater than 0 if this object's value is greater than the
    /// other value, or 0 if both values are equal.</returns>
    public int CompareTo(int intOther) {
      return this.CompareToValue(EFloat.FromInt32(intOther));
    }

    /// <summary>Compares the mathematical values of this object and
    /// another object, accepting NaN values.
    /// <para>This method is not consistent with the Equals method because
    /// two different numbers with the same mathematical value, but
    /// different exponents, will compare as equal.</para>
    /// <para>In this method, negative zero and positive zero are
    /// considered equal.</para>
    /// <para>If this object is a quiet NaN or signaling NaN, this method
    /// will not trigger an error. Instead, NaN will compare greater than
    /// any other number.</para></summary>
    /// <param name='intOther'>The parameter <paramref name='intOther'/> is
    /// a 32-bit signed integer.</param>
    /// <returns>Less than 0 if this object's value is less than the other
    /// value, or greater than 0 if this object's value is greater than the
    /// other value, or 0 if both values are equal.</returns>
    public int CompareToValue(int intOther) {
      return this.CompareToValue(EFloat.FromInt32(intOther));
    }

    /// <summary>Compares the mathematical values of this object and
    /// another object, accepting NaN values.
    /// <para>This method is not consistent with the Equals method because
    /// two different numbers with the same mathematical value, but
    /// different exponents, will compare as equal.</para>
    /// <para>In this method, negative zero and positive zero are
    /// considered equal.</para>
    /// <para>If this object is a quiet NaN or signaling NaN, this method
    /// will not trigger an error. Instead, NaN will compare greater than
    /// any other number, including infinity.</para></summary>
    /// <param name='intOther'>The parameter <paramref name='intOther'/> is
    /// a 64-bit signed integer.</param>
    /// <returns>Less than 0 if this object's value is less than the other
    /// value, or greater than 0 if this object's value is greater than the
    /// other value, or 0 if both values are equal.</returns>
    public int CompareToValue(long intOther) {
      return this.CompareToValue(FromInt64(intOther));
    }

    /// <summary>Compares the mathematical values of this object and
    /// another object, accepting NaN values. This method currently uses
    /// the rules given in the CompareToValue method, so that it it is not
    /// consistent with the Equals method, but it may change in a future
    /// version to use the rules for the CompareToTotal method
    /// instead.</summary>
    /// <param name='intOther'>The parameter <paramref name='intOther'/> is
    /// a 64-bit signed integer.</param>
    /// <returns>Less than 0 if this object's value is less than the other
    /// value, or greater than 0 if this object's value is greater than the
    /// other value, or 0 if both values are equal.</returns>
    public int CompareTo(long intOther) {
      return this.CompareToValue(FromInt64(intOther));
    }

    /// <summary>Compares the mathematical values of this object and
    /// another object, treating quiet NaN as signaling.
    /// <para>In this method, negative zero and positive zero are
    /// considered equal.</para>
    /// <para>If this object or the other object is a quiet NaN or
    /// signaling NaN, this method will return a quiet NaN and will signal
    /// a FlagInvalid flag.</para></summary>
    /// <param name='other'>An arbitrary-precision binary floating-point
    /// number.</param>
    /// <param name='ctx'>An arithmetic context. The precision, rounding,
    /// and exponent range are ignored. If <c>HasFlags</c> of the context
    /// is true, will store the flags resulting from the operation (the
    /// flags are in addition to the pre-existing flags). Can be
    /// null.</param>
    /// <returns>Quiet NaN if this object or the other object is NaN, or 0
    /// if both objects have the same value, or -1 if this object is less
    /// than the other value, or 1 if this object is greater.
    /// <para>This implementation returns a positive number if <paramref
    /// name='other'/> is null, to conform to the.NET definition of
    /// CompareTo. This is the case even in the Java version of this
    /// library, for consistency's sake, even though implementations of
    /// <c>Comparable.compareTo()</c> in Java ought to throw an exception
    /// if they receive a null argument rather than treating null as less
    /// or greater than any object.</para>.</returns>
    public EFloat CompareToSignal(
      EFloat other,
      EContext ctx) {
      return MathValue.CompareToWithContext(this, other, true, ctx);
    }

    /// <summary>Compares the values of this object and another object,
    /// imposing a total ordering on all possible values. In this method:
    /// <list>
    /// <item>For objects with the same value, the one with the higher
    /// exponent has a greater "absolute value".</item>
    /// <item>Negative zero is less than positive zero.</item>
    /// <item>Quiet NaN has a higher "absolute value" than signaling NaN.
    /// If both objects are quiet NaN or both are signaling NaN, the one
    /// with the higher diagnostic information has a greater "absolute
    /// value".</item>
    /// <item>NaN has a higher "absolute value" than infinity.</item>
    /// <item>Infinity has a higher "absolute value" than any finite
    /// number.</item>
    /// <item>Negative numbers are less than positive
    /// numbers.</item></list></summary>
    /// <param name='other'>An arbitrary-precision binary floating-point
    /// number to compare with this one.</param>
    /// <param name='ctx'>An arithmetic context. Flags will be set in this
    /// context only if <c>HasFlags</c> and <c>IsSimplified</c> of the
    /// context are true and only if an operand needed to be rounded before
    /// carrying out the operation. Can be null.</param>
    /// <returns>The number 0 if both objects have the same value, or -1 if
    /// this object is less than the other value, or 1 if this object is
    /// greater. Does not signal flags if either value is signaling NaN.
    /// <para>This implementation returns a positive number if <paramref
    /// name='other'/> is null, to conform to the.NET definition of
    /// CompareTo. This is the case even in the Java version of this
    /// library, for consistency's sake, even though implementations of
    /// <c>Comparable.compareTo()</c> in Java ought to throw an exception
    /// if they receive a null argument rather than treating null as less
    /// or greater than any object.</para>.</returns>
    public int CompareToTotal(EFloat other, EContext ctx) {
      if (other == null) {
        return 1;
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

    /// <summary>Compares the values of this object and another object,
    /// imposing a total ordering on all possible values (ignoring their
    /// signs). In this method:
    /// <list>
    /// <item>For objects with the same value, the one with the higher
    /// exponent has a greater "absolute value".</item>
    /// <item>Negative zero is less than positive zero.</item>
    /// <item>Quiet NaN has a higher "absolute value" than signaling NaN.
    /// If both objects are quiet NaN or both are signaling NaN, the one
    /// with the higher diagnostic information has a greater "absolute
    /// value".</item>
    /// <item>NaN has a higher "absolute value" than infinity.</item>
    /// <item>Infinity has a higher "absolute value" than any finite
    /// number.</item>
    /// <item>Negative numbers are less than positive
    /// numbers.</item></list></summary>
    /// <param name='other'>An arbitrary-precision binary floating-point
    /// number to compare with this one.</param>
    /// <param name='ctx'>An arithmetic context. Flags will be set in this
    /// context only if <c>HasFlags</c> and <c>IsSimplified</c> of the
    /// context are true and only if an operand needed to be rounded before
    /// carrying out the operation. Can be null.</param>
    /// <returns>The number 0 if both objects have the same value (ignoring
    /// their signs), or -1 if this object is less than the other value
    /// (ignoring their signs), or 1 if this object is greater (ignoring
    /// their signs). Does not signal flags if either value is signaling
    /// NaN.
    /// <para>This implementation returns a positive number if <paramref
    /// name='other'/> is null, to conform to the.NET definition of
    /// CompareTo. This is the case even in the Java version of this
    /// library, for consistency's sake, even though implementations of
    /// <c>Comparable.compareTo()</c> in Java ought to throw an exception
    /// if they receive a null argument rather than treating null as less
    /// or greater than any object.</para>.</returns>
    public int CompareToTotalMagnitude(EFloat other, EContext ctx) {
      if (other == null) {
        return 1;
      }
      if (this.IsSignalingNaN() || other.IsSignalingNaN()) {
        return this.CompareToTotalMagnitude(other);
      }
      if (ctx != null && ctx.IsSimplified) {
        return this.RoundToPrecision(ctx)
          .CompareToTotalMagnitude(other.RoundToPrecision(ctx));
      } else {
        return this.CompareToTotalMagnitude(other);
      }
    }

    /// <summary>Compares the values of this object and another object,
    /// imposing a total ordering on all possible values. In this method:
    /// <list>
    /// <item>For objects with the same value, the one with the higher
    /// exponent has a greater "absolute value".</item>
    /// <item>Negative zero is less than positive zero.</item>
    /// <item>Quiet NaN has a higher "absolute value" than signaling NaN.
    /// If both objects are quiet NaN or both are signaling NaN, the one
    /// with the higher diagnostic information has a greater "absolute
    /// value".</item>
    /// <item>NaN has a higher "absolute value" than infinity.</item>
    /// <item>Infinity has a higher "absolute value" than any finite
    /// number.</item>
    /// <item>Negative numbers are less than positive
    /// numbers.</item></list></summary>
    /// <param name='other'>An arbitrary-precision binary floating-point
    /// number to compare with this one.</param>
    /// <returns>The number 0 if both objects have the same value, or -1 if
    /// this object is less than the other value, or 1 if this object is
    /// greater.
    /// <para>This implementation returns a positive number if <paramref
    /// name='other'/> is null, to conform to the.NET definition of
    /// CompareTo. This is the case even in the Java version of this
    /// library, for consistency's sake, even though implementations of
    /// <c>Comparable.compareTo()</c> in Java ought to throw an exception
    /// if they receive a null argument rather than treating null as less
    /// or greater than any object.</para>.</returns>
    public int CompareToTotal(EFloat other) {
      if (other == null) {
        return 1;
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

    /// <summary>Compares the absolute values of this object and another
    /// object, imposing a total ordering on all possible values (ignoring
    /// their signs). In this method:
    /// <list>
    /// <item>For objects with the same value, the one with the higher
    /// exponent has a greater "absolute value".</item>
    /// <item>Negative zero and positive zero are considered equal.</item>
    /// <item>Quiet NaN has a higher "absolute value" than signaling NaN.
    /// If both objects are quiet NaN or both are signaling NaN, the one
    /// with the higher diagnostic information has a greater "absolute
    /// value".</item>
    /// <item>NaN has a higher "absolute value" than infinity.</item>
    /// <item>Infinity has a higher "absolute value" than any finite
    /// number.</item></list></summary>
    /// <param name='other'>An arbitrary-precision binary floating-point
    /// number to compare with this one.</param>
    /// <returns>The number 0 if both objects have the same value, or -1 if
    /// this object is less than the other value, or 1 if this object is
    /// greater.
    /// <para>This implementation returns a positive number if <paramref
    /// name='other'/> is null, to conform to the.NET definition of
    /// CompareTo. This is the case even in the Java version of this
    /// library, for consistency's sake, even though implementations of
    /// <c>Comparable.compareTo()</c> in Java ought to throw an exception
    /// if they receive a null argument rather than treating null as less
    /// or greater than any object.</para>.</returns>
    public int CompareToTotalMagnitude(EFloat other) {
      if (other == null) {
        return 1;
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

    /// <summary>Compares the mathematical values of this object and
    /// another object.
    /// <para>In this method, negative zero and positive zero are
    /// considered equal.</para>
    /// <para>If this object or the other object is a quiet NaN or
    /// signaling NaN, this method returns a quiet NaN, and will signal a
    /// FlagInvalid flag if either is a signaling NaN.</para></summary>
    /// <param name='other'>An arbitrary-precision binary floating-point
    /// number.</param>
    /// <param name='ctx'>An arithmetic context. The precision, rounding,
    /// and exponent range are ignored. If <c>HasFlags</c> of the context
    /// is true, will store the flags resulting from the operation (the
    /// flags are in addition to the pre-existing flags). Can be
    /// null.</param>
    /// <returns>Quiet NaN if this object or the other object is NaN, or 0
    /// if both objects have the same value, or -1 if this object is less
    /// than the other value, or 1 if this object is greater.
    /// <para>This implementation returns a positive number if <paramref
    /// name='other'/> is null, to conform to the.NET definition of
    /// CompareTo. This is the case even in the Java version of this
    /// library, for consistency's sake, even though implementations of
    /// <c>Comparable.compareTo()</c> in Java ought to throw an exception
    /// if they receive a null argument rather than treating null as less
    /// or greater than any object.</para>.</returns>
    public EFloat CompareToWithContext(
      EFloat other,
      EContext ctx) {
      return MathValue.CompareToWithContext(this, other, false, ctx);
    }

    /// <summary>Returns a number with the same value as this one, but
    /// copying the sign (positive or negative) of another number. (This
    /// method is similar to the "copy-sign" operation in the General
    /// Decimal Arithmetic Specification, except this method does not
    /// necessarily return a copy of this object.).</summary>
    /// <param name='other'>A number whose sign will be copied.</param>
    /// <returns>An arbitrary-precision binary floating-point
    /// number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='other'/> is null.</exception>
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

    /// <summary>Divides this arbitrary-precision binary floating-point
    /// number by another arbitrary-precision binary floating-point number
    /// and returns the result; returns NaN instead if the result would
    /// have a nonterminating binary expansion (including 1/3, 1/12, 1/7,
    /// 2/3, and so on); if this is not desired, use DivideToExponent, or
    /// use the Divide overload that takes an EContext.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <returns>The result of dividing this arbitrary-precision binary
    /// floating-point number by another arbitrary-precision binary
    /// floating-point number. Returns infinity if the divisor (this
    /// arbitrary-precision binary floating-point number) is 0 and the
    /// dividend (the other arbitrary-precision binary floating-point
    /// number) is nonzero. Returns not-a-number (NaN) if the divisor and
    /// the dividend are 0. Returns NaN if the result can't be exact
    /// because it would have a nonterminating binary expansion (examples
    /// include 1 divided by any multiple of 3, such as 1/3 or 1/12). If
    /// this is not desired, use DivideToExponent instead, or use the
    /// Divide overload that takes an <c>EContext</c> (such as
    /// <c>EContext.Binary64</c> ) instead.</returns>
    public EFloat Divide(EFloat divisor) {
      return this.Divide(
          divisor,
          EContext.ForRounding(ERounding.None));
    }

    /// <summary>Divides this arbitrary-precision binary floating-point
    /// number by another arbitrary-precision binary floating-point number
    /// and returns the result.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and no
    /// rounding is needed.</param>
    /// <returns>The result of dividing this arbitrary-precision binary
    /// floating-point number by another arbitrary-precision binary
    /// floating-point number. Signals FlagDivideByZero and returns
    /// infinity if the divisor (this arbitrary-precision binary
    /// floating-point number) is 0 and the dividend (the other
    /// arbitrary-precision binary floating-point number) is nonzero.
    /// Signals FlagInvalid and returns not-a-number (NaN) if the divisor
    /// and the dividend are 0; or, either <paramref name='ctx'/> is null
    /// or <paramref name='ctx'/> 's precision is 0, and the result would
    /// have a nonterminating decimal expansion (examples include 1 divided
    /// by any multiple of 3, such as 1/3 or 1/12); or, the rounding mode
    /// is ERounding.None and the result is not exact.</returns>
    public EFloat Divide(
      EFloat divisor,
      EContext ctx) {
      return MathValue.Divide(this, divisor, ctx);
    }

    /// <summary>Calculates the quotient and remainder using the
    /// DivideToIntegerNaturalScale and the formula in
    /// RemainderNaturalScale.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <returns>A 2 element array consisting of the quotient and remainder
    /// in that order.</returns>
    [Obsolete("Renamed to DivRemNaturalScale.")]
    public EFloat[] DivideAndRemainderNaturalScale(EFloat
      divisor) {
      return this.DivRemNaturalScale(divisor, null);
    }

    /// <summary>Calculates the quotient and remainder using the
    /// DivideToIntegerNaturalScale and the formula in
    /// RemainderNaturalScale.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='ctx'>An arithmetic context object to control the
    /// precision, rounding, and exponent range of the result. This context
    /// will be used only in the division portion of the remainder
    /// calculation; as a result, it's possible for the remainder to have a
    /// higher precision than given in this context. Flags will be set on
    /// the given context only if the context's <c>HasFlags</c> is true and
    /// the integer part of the division result doesn't fit the precision
    /// and exponent range without rounding. Can be null, in which the
    /// precision is unlimited and no additional rounding, other than the
    /// rounding down to an integer after division, is needed.</param>
    /// <returns>A 2 element array consisting of the quotient and remainder
    /// in that order.</returns>
    [Obsolete("Renamed to DivRemNaturalScale.")]
    public EFloat[] DivideAndRemainderNaturalScale(
      EFloat divisor,
      EContext ctx) {
      return this.DivRemNaturalScale(divisor, ctx);
    }

    /// <summary>Divides two arbitrary-precision binary floating-point
    /// numbers, and gives a particular exponent to the result.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='desiredExponentSmall'>The desired exponent. A negative
    /// number places the cutoff point to the right of the usual radix
    /// point (so a negative number means the number of binary digit places
    /// to round to). A positive number places the cutoff point to the left
    /// of the usual radix point.</param>
    /// <param name='ctx'>An arithmetic context object to control the
    /// rounding mode to use if the result must be scaled down to have the
    /// same exponent as this value. If the precision given in the context
    /// is other than 0, calls the Quantize method with both arguments
    /// equal to the result of the operation (and can signal FlagInvalid
    /// and return NaN if the result doesn't fit the given precision). If
    /// <c>HasFlags</c> of the context is true, will also store the flags
    /// resulting from the operation (the flags are in addition to the
    /// pre-existing flags). Can be null, in which case the default
    /// rounding mode is HalfEven.</param>
    /// <returns>The quotient of the two objects. Signals FlagDivideByZero
    /// and returns infinity if the divisor is 0 and the dividend is
    /// nonzero. Signals FlagInvalid and returns not-a-number (NaN) if the
    /// divisor and the dividend are 0. Signals FlagInvalid and returns
    /// not-a-number (NaN) if the context defines an exponent range and the
    /// desired exponent is outside that range. Signals FlagInvalid and
    /// returns not-a-number (NaN) if the rounding mode is ERounding.None
    /// and the result is not exact.</returns>
    public EFloat DivideToExponent(
      EFloat divisor,
      long desiredExponentSmall,
      EContext ctx) {
      return this.DivideToExponent(
          divisor,
          EInteger.FromInt64(desiredExponentSmall),
          ctx);
    }

    /// <summary>Divides two arbitrary-precision binary floating-point
    /// numbers, and gives a particular exponent to the result.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='desiredExponentSmall'>The desired exponent. A negative
    /// number places the cutoff point to the right of the usual radix
    /// point (so a negative number means the number of binary digit places
    /// to round to). A positive number places the cutoff point to the left
    /// of the usual radix point.</param>
    /// <param name='rounding'>The rounding mode to use if the result must
    /// be scaled down to have the same exponent as this value.</param>
    /// <returns>The quotient of the two objects. Signals FlagDivideByZero
    /// and returns infinity if the divisor is 0 and the dividend is
    /// nonzero. Signals FlagInvalid and returns not-a-number (NaN) if the
    /// divisor and the dividend are 0. Signals FlagInvalid and returns
    /// not-a-number (NaN) if the rounding mode is ERounding.None and the
    /// result is not exact.</returns>
    public EFloat DivideToExponent(
      EFloat divisor,
      long desiredExponentSmall,
      ERounding rounding) {
      return this.DivideToExponent(
          divisor,
          EInteger.FromInt64(desiredExponentSmall),
          EContext.ForRounding(rounding));
    }

    /// <summary>Divides two arbitrary-precision binary floating-point
    /// numbers, and gives a particular exponent to the result.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='exponent'>The desired exponent. A negative number
    /// places the cutoff point to the right of the usual radix point (so a
    /// negative number means the number of binary digit places to round
    /// to). A positive number places the cutoff point to the left of the
    /// usual radix point.</param>
    /// <param name='ctx'>An arithmetic context object to control the
    /// rounding mode to use if the result must be scaled down to have the
    /// same exponent as this value. If the precision given in the context
    /// is other than 0, calls the Quantize method with both arguments
    /// equal to the result of the operation (and can signal FlagInvalid
    /// and return NaN if the result doesn't fit the given precision). If
    /// <c>HasFlags</c> of the context is true, will also store the flags
    /// resulting from the operation (the flags are in addition to the
    /// pre-existing flags). Can be null, in which case the default
    /// rounding mode is HalfEven.</param>
    /// <returns>The quotient of the two objects. Signals FlagDivideByZero
    /// and returns infinity if the divisor is 0 and the dividend is
    /// nonzero. Signals FlagInvalid and returns not-a-number (NaN) if the
    /// divisor and the dividend are 0. Signals FlagInvalid and returns
    /// not-a-number (NaN) if the context defines an exponent range and the
    /// desired exponent is outside that range. Signals FlagInvalid and
    /// returns not-a-number (NaN) if the rounding mode is ERounding.None
    /// and the result is not exact.</returns>
    public EFloat DivideToExponent(
      EFloat divisor,
      EInteger exponent,
      EContext ctx) {
      return MathValue.DivideToExponent(this, divisor, exponent, ctx);
    }

    /// <summary>Divides two arbitrary-precision binary floating-point
    /// numbers, and gives a particular exponent to the result.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='desiredExponent'>The desired exponent. A negative
    /// number places the cutoff point to the right of the usual radix
    /// point (so a negative number means the number of binary digit places
    /// to round to). A positive number places the cutoff point to the left
    /// of the usual radix point.</param>
    /// <param name='rounding'>The rounding mode to use if the result must
    /// be scaled down to have the same exponent as this value.</param>
    /// <returns>The quotient of the two objects. Signals FlagDivideByZero
    /// and returns infinity if the divisor is 0 and the dividend is
    /// nonzero. Returns not-a-number (NaN) if the divisor and the dividend
    /// are 0. Returns NaN if the rounding mode is ERounding.None and the
    /// result is not exact.</returns>
    public EFloat DivideToExponent(
      EFloat divisor,
      EInteger desiredExponent,
      ERounding rounding) {
      return this.DivideToExponent(
          divisor,
          desiredExponent,
          EContext.ForRounding(rounding));
    }

    /// <summary>Divides two arbitrary-precision binary floating-point
    /// numbers, and returns the integer part of the result, rounded down,
    /// with the preferred exponent set to this value's exponent minus the
    /// divisor's exponent.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <returns>The integer part of the quotient of the two objects.
    /// Signals FlagDivideByZero and returns infinity if the divisor is 0
    /// and the dividend is nonzero. Signals FlagInvalid and returns
    /// not-a-number (NaN) if the divisor and the dividend are 0.</returns>
    public EFloat DivideToIntegerNaturalScale(
      EFloat divisor) {
      return this.DivideToIntegerNaturalScale(
          divisor,
          EContext.ForRounding(ERounding.Down));
    }

    /// <summary>Divides this object by another object, and returns the
    /// integer part of the result (which is initially rounded down), with
    /// the preferred exponent set to this value's exponent minus the
    /// divisor's exponent.</summary>
    /// <param name='divisor'>An arbitrary-precision binary floating-point
    /// number.</param>
    /// <param name='ctx'>The parameter <paramref name='ctx'/> is an
    /// EContext object.</param>
    /// <returns>The integer part of the quotient of the two objects.
    /// Signals FlagInvalid and returns not-a-number (NaN) if the return
    /// value would overflow the exponent range. Signals FlagDivideByZero
    /// and returns infinity if the divisor is 0 and the dividend is
    /// nonzero. Signals FlagInvalid and returns not-a-number (NaN) if the
    /// divisor and the dividend are 0. Signals FlagInvalid and returns
    /// not-a-number (NaN) if the rounding mode is ERounding.None and the
    /// result is not exact.</returns>
    public EFloat DivideToIntegerNaturalScale(
      EFloat divisor,
      EContext ctx) {
      return MathValue.DivideToIntegerNaturalScale(this, divisor, ctx);
    }

    /// <summary>Divides this object by another object, and returns the
    /// integer part of the result, with the exponent set to 0.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='ctx'>An arithmetic context object to control the
    /// precision. The rounding and exponent range settings of this context
    /// are ignored. If <c>HasFlags</c> of the context is true, will also
    /// store the flags resulting from the operation (the flags are in
    /// addition to the pre-existing flags). Can be null, in which case the
    /// precision is unlimited.</param>
    /// <returns>The integer part of the quotient of the two objects. The
    /// exponent will be set to 0. Signals FlagDivideByZero and returns
    /// infinity if the divisor is 0 and the dividend is nonzero. Signals
    /// FlagInvalid and returns not-a-number (NaN) if the divisor and the
    /// dividend are 0, or if the result doesn't fit the given
    /// precision.</returns>
    public EFloat DivideToIntegerZeroScale(
      EFloat divisor,
      EContext ctx) {
      return MathValue.DivideToIntegerZeroScale(this, divisor, ctx);
    }

    /// <summary>Divides this object by another binary floating-point
    /// number and returns a result with the same exponent as this object
    /// (the dividend).</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='rounding'>The rounding mode to use if the result must
    /// be scaled down to have the same exponent as this value.</param>
    /// <returns>The quotient of the two numbers. Signals FlagDivideByZero
    /// and returns infinity if the divisor is 0 and the dividend is
    /// nonzero. Signals FlagInvalid and returns not-a-number (NaN) if the
    /// divisor and the dividend are 0. Signals FlagInvalid and returns
    /// not-a-number (NaN) if the rounding mode is ERounding.None and the
    /// result is not exact.</returns>
    public EFloat DivideToSameExponent(
      EFloat divisor,
      ERounding rounding) {
      return this.DivideToExponent(
          divisor,
          this.exponent.ToEInteger(),
          EContext.ForRounding(rounding));
    }

    /// <summary>Divides this arbitrary-precision binary floating-point
    /// number by another arbitrary-precision binary floating-point number
    /// and returns a two-item array containing the result of the division
    /// and the remainder, in that order. The result of division is
    /// calculated as though by <c>DivideToIntegerNaturalScale</c>, and
    /// the remainder is calculated as though by
    /// <c>RemainderNaturalScale</c>.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <returns>An array of two items: the first is the result of the
    /// division as an arbitrary-precision binary floating-point number,
    /// and the second is the remainder as an arbitrary-precision binary
    /// floating-point number. The result of division is the result of the
    /// method on the two operands, and the remainder is the result of the
    /// Remainder method on the two operands.</returns>
    public EFloat[] DivRemNaturalScale(EFloat divisor) {
      return this.DivRemNaturalScale(divisor, null);
    }

    /// <summary>Divides this arbitrary-precision binary floating-point
    /// number by another arbitrary-precision binary floating-point number
    /// and returns a two-item array containing the result of the division
    /// and the remainder, in that order. The result of division is
    /// calculated as though by <c>DivideToIntegerNaturalScale</c>, and
    /// the remainder is calculated as though by
    /// <c>RemainderNaturalScale</c>.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='ctx'>An arithmetic context object to control the
    /// precision, rounding, and exponent range of the result. This context
    /// will be used only in the division portion of the remainder
    /// calculation; as a result, it's possible for the remainder to have a
    /// higher precision than given in this context. Flags will be set on
    /// the given context only if the context's <c>HasFlags</c> is true and
    /// the integer part of the division result doesn't fit the precision
    /// and exponent range without rounding. Can be null, in which the
    /// precision is unlimited and no additional rounding, other than the
    /// rounding down to an integer after division, is needed.</param>
    /// <returns>An array of two items: the first is the result of the
    /// division as an arbitrary-precision binary floating-point number,
    /// and the second is the remainder as an arbitrary-precision binary
    /// floating-point number. The result of division is the result of the
    /// method on the two operands, and the remainder is the result of the
    /// Remainder method on the two operands.</returns>
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

    /// <summary>Determines whether this object's significand, exponent,
    /// and properties are equal to those of another object. Not-a-number
    /// values are considered equal if the rest of their properties are
    /// equal.</summary>
    /// <param name='other'>An arbitrary-precision binary floating-point
    /// number.</param>
    /// <returns><c>true</c> if this object's significand and exponent are
    /// equal to those of another object; otherwise, <c>false</c>.</returns>
    public bool Equals(EFloat other) {
      return this.EqualsInternal(other);
    }

    /// <summary>Determines whether this object's significand, exponent,
    /// and properties are equal to those of another object and that other
    /// object is an arbitrary-precision binary floating-point number.
    /// Not-a-number values are considered equal if the rest of their
    /// properties are equal.</summary>
    /// <param name='obj'>The parameter <paramref name='obj'/> is an
    /// arbitrary object.</param>
    /// <returns><c>true</c> if the objects are equal; otherwise,
    /// <c>false</c>. In this method, two objects are not equal if they
    /// don't have the same type or if one is null and the other
    /// isn't.</returns>
    public override bool Equals(object obj) {
      return this.EqualsInternal(obj as EFloat);
    }

    /// <summary>Determines whether this object's significand and exponent
    /// are equal to those of another object.</summary>
    /// <param name='otherValue'>An arbitrary-precision binary
    /// floating-point number.</param>
    /// <returns><c>true</c> if this object's significand and exponent are
    /// equal to those of another object; otherwise, <c>false</c>.</returns>
    public bool EqualsInternal(EFloat otherValue) {
      if (otherValue == null) {
        return false;
      }
      return this.exponent.Equals(otherValue.exponent) &&
        this.unsignedMantissa.Equals(otherValue.unsignedMantissa) &&
        this.flags == otherValue.flags;
    }

    /// <summary>Finds e (the base of natural logarithms) raised to the
    /// power of this object's value.</summary>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// <i>This parameter can't be null, as the exponential function's
    /// results are generally not exact.</i> (Unlike in the General Binary
    /// Arithmetic Specification, any rounding mode is allowed.).</param>
    /// <returns>Exponential of this object. If this object's value is 1,
    /// returns an approximation to " e" within the given precision.
    /// Signals FlagInvalid and returns not-a-number (NaN) if the parameter
    /// <paramref name='ctx'/> is null or the precision is unlimited (the
    /// context's Precision property is 0).</returns>
    public EFloat Exp(EContext ctx) {
      return MathValue.Exp(this, ctx);
    }

    /// <summary>Finds e (the base of natural logarithms) raised to the
    /// power of this object's value, and subtracts the result by 1 and
    /// returns the final result, in a way that avoids loss of precision if
    /// the true result is very close to 0.</summary>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// <i>This parameter can't be null, as the exponential function's
    /// results are generally not exact.</i> (Unlike in the General Binary
    /// Arithmetic Specification, any rounding mode is allowed.).</param>
    /// <returns>Exponential of this object, minus 1. Signals FlagInvalid
    /// and returns not-a-number (NaN) if the parameter <paramref
    /// name='ctx'/> is null or the precision is unlimited (the context's
    /// Precision property is 0).</returns>
    public EFloat ExpM1(EContext ctx) {
      EFloat value = this;
      if (value.IsNaN()) {
        return value.Plus(ctx);
      }
      if (ctx == null || !ctx.HasMaxPrecision) {
        return EFloat.SignalingNaN.Plus(ctx);
      }
      if (ctx.Traps != 0) {
        EContext tctx = ctx.GetNontrapping();
        EFloat ret = value.ExpM1(tctx);
        return ctx.TriggerTraps(ret, tctx);
      } else if (ctx.IsSimplified) {
        EContext tmpctx = ctx.WithSimplified(false).WithBlankFlags();
        EFloat ret = value.PreRound(ctx).ExpM1(tmpctx);
        if (ctx.HasFlags) {
          int flags = ctx.Flags;
          ctx.Flags = flags | tmpctx.Flags;
        }
        // DebugUtility.Log("{0} {1} [{4} {5}] -> {2}
        // [{3}]",value,baseValue,ret,ret.RoundToPrecision(ctx),
        // value.Quantize(value, ctx), baseValue.Quantize(baseValue, ctx));
        return ret.RoundToPrecision(ctx);
      } else {
        if (value.CompareTo(-1) == 0) {
          return EFloat.NegativeInfinity;
        } else if (value.IsPositiveInfinity()) {
          return EFloat.PositiveInfinity;
        } else if (value.IsNegativeInfinity()) {
          return EFloat.FromInt32(-1).Plus(ctx);
        } else if (value.CompareTo(0) == 0) {
          return EFloat.FromInt32(0).Plus(ctx);
        }
        int flags = ctx.Flags;
        EContext tmpctx = null;
        EFloat ret;
        // DebugUtility.Log("value=" + (value));
        {
          EInteger prec = ctx.Precision.Add(3);
          tmpctx = ctx.WithBigPrecision(prec).WithBlankFlags();
          if (value.Abs().CompareTo(EFloat.Create(1, -1)) < 0) {
            ret = value.Exp(tmpctx).Add(EFloat.FromInt32(-1), ctx);
            EFloat oldret = ret;
            while (true) {
              prec = prec.Add(ctx.Precision).Add(3);
              tmpctx = ctx.WithBigPrecision(prec).WithBlankFlags();
              ret = value.Exp(tmpctx).Add(EFloat.FromInt32(-1), ctx);
              if (ret.CompareTo(0) != 0 && ret.CompareTo(oldret) == 0) {
                break;
              }
              oldret = ret;
            }
          } else {
            ret = value.Exp(tmpctx).Add(EFloat.FromInt32(-1), ctx);
          }
          flags |= tmpctx.Flags;
        }
        if (ctx.HasFlags) {
          flags |= ctx.Flags;
          ctx.Flags = flags;
        }
        return ret;
      }
    }

    /// <summary>Calculates this object's hash code. No application or
    /// process IDs are used in the hash code calculation.</summary>
    /// <returns>A 32-bit signed integer.</returns>
    public override int GetHashCode() {
      var hashCode = 403796923;
      unchecked {
        hashCode += 403797019 * this.exponent.GetHashCode();
        hashCode += 403797059 * this.unsignedMantissa.GetHashCode();
        hashCode += 403797127 * this.flags;
      }
      return hashCode;
    }

    /// <summary>Gets a value indicating whether this object is positive or
    /// negative infinity.</summary>
    /// <returns><c>true</c> if this object is positive or negative
    /// infinity; otherwise, <c>false</c>.</returns>
    public bool IsInfinity() {
      return (this.flags & BigNumberFlags.FlagInfinity) != 0;
    }

    /// <summary>Gets a value indicating whether this object is not a
    /// number (NaN).</summary>
    /// <returns><c>true</c> if this object is not a number (NaN);
    /// otherwise, <c>false</c>.</returns>
    public bool IsNaN() {
      return (this.flags & (BigNumberFlags.FlagQuietNaN |
            BigNumberFlags.FlagSignalingNaN)) != 0;
    }

    /// <summary>Returns whether this object is negative
    /// infinity.</summary>
    /// <returns><c>true</c> if this object is negative infinity;
    /// otherwise, <c>false</c>.</returns>
    public bool IsNegativeInfinity() {
      return (this.flags & (BigNumberFlags.FlagInfinity |
            BigNumberFlags.FlagNegative)) ==
        (BigNumberFlags.FlagInfinity | BigNumberFlags.FlagNegative);
    }

    /// <summary>Returns whether this object is positive
    /// infinity.</summary>
    /// <returns><c>true</c> if this object is positive infinity;
    /// otherwise, <c>false</c>.</returns>
    public bool IsPositiveInfinity() {
      return (this.flags & (BigNumberFlags.FlagInfinity |
            BigNumberFlags.FlagNegative)) == BigNumberFlags.FlagInfinity;
    }

    /// <summary>Gets a value indicating whether this object is a quiet
    /// not-a-number value.</summary>
    /// <returns><c>true</c> if this object is a quiet not-a-number value;
    /// otherwise, <c>false</c>.</returns>
    public bool IsQuietNaN() {
      return (this.flags & BigNumberFlags.FlagQuietNaN) != 0;
    }

    /// <summary>Gets a value indicating whether this object is a signaling
    /// not-a-number value.</summary>
    /// <returns><c>true</c> if this object is a signaling not-a-number
    /// value; otherwise, <c>false</c>.</returns>
    public bool IsSignalingNaN() {
      return (this.flags & BigNumberFlags.FlagSignalingNaN) != 0;
    }

    /// <summary>Finds the natural logarithm of this object, that is, the
    /// power (exponent) that e (the base of natural logarithms) must be
    /// raised to in order to equal this object's value.</summary>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// <i>This parameter can't be null, as the ln function's results are
    /// generally not exact.</i> (Unlike in the General Binary Arithmetic
    /// Specification, any rounding mode is allowed.).</param>
    /// <returns>Ln(this object). Signals the flag FlagInvalid and returns
    /// NaN if this object is less than 0 (the result would be a complex
    /// number with a real part equal to Ln of this object's absolute value
    /// and an imaginary part equal to pi, but the return value is still
    /// NaN.). Signals FlagInvalid and returns not-a-number (NaN) if the
    /// parameter <paramref name='ctx'/> is null or the precision is
    /// unlimited (the context's Precision property is 0). Signals no flags
    /// and returns negative infinity if this object's value is
    /// 0.</returns>
    public EFloat Log(EContext ctx) {
      return MathValue.Ln(this, ctx);
    }

    /// <summary>Finds the base-10 logarithm of this object, that is, the
    /// power (exponent) that the number 10 must be raised to in order to
    /// equal this object's value.</summary>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// <i>This parameter can't be null, as the ln function's results are
    /// generally not exact.</i> (Unlike in the General Binary Arithmetic
    /// Specification, any rounding mode is allowed.).</param>
    /// <returns>Ln(this object)/Ln(10). Signals the flag FlagInvalid and
    /// returns not-a-number (NaN) if this object is less than 0. Signals
    /// FlagInvalid and returns not-a-number (NaN) if the parameter
    /// <paramref name='ctx'/> is null or the precision is unlimited (the
    /// context's Precision property is 0).</returns>
    public EFloat Log10(EContext ctx) {
      return this.LogN(EFloat.FromInt32(10), ctx);
    }

    /// <summary>Adds 1 to this object's value and finds the natural
    /// logarithm of the result, in a way that avoids loss of precision
    /// when this object's value is between 0 and 1.</summary>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// <i>This parameter can't be null, as the ln function's results are
    /// generally not exact.</i> (Unlike in the General Binary Arithmetic
    /// Specification, any rounding mode is allowed.).</param>
    /// <returns>Ln(1+(this object)). Signals the flag FlagInvalid and
    /// returns NaN if this object is less than -1 (the result would be a
    /// complex number with a real part equal to Ln of 1 plus this object's
    /// absolute value and an imaginary part equal to pi, but the return
    /// value is still NaN.). Signals FlagInvalid and returns not-a-number
    /// (NaN) if the parameter <paramref name='ctx'/> is null or the
    /// precision is unlimited (the context's Precision property is 0).
    /// Signals no flags and returns negative infinity if this object's
    /// value is 0.</returns>
    public EFloat Log1P(EContext ctx) {
      EFloat value = this;
      if (value.IsNaN()) {
        return value.Plus(ctx);
      }
      if (ctx == null || !ctx.HasMaxPrecision ||
        (value.CompareTo(-1) < 0)) {
        return EFloat.SignalingNaN.Plus(ctx);
      }
      if (ctx.Traps != 0) {
        EContext tctx = ctx.GetNontrapping();
        EFloat ret = value.Log1P(tctx);
        return ctx.TriggerTraps(ret, tctx);
      } else if (ctx.IsSimplified) {
        EContext tmpctx = ctx.WithSimplified(false).WithBlankFlags();
        EFloat ret = value.PreRound(ctx).Log1P(tmpctx);
        if (ctx.HasFlags) {
          int flags = ctx.Flags;
          ctx.Flags = flags | tmpctx.Flags;
        }
        // Console.WriteLine("{0} {1} [{4} {5}] -> {2}
        // [{3}]",value,baseValue,ret,ret.RoundToPrecision(ctx),
        // value.Quantize(value, ctx), baseValue.Quantize(baseValue, ctx));
        return ret.RoundToPrecision(ctx);
      } else {
        if (value.CompareTo(-1) == 0) {
          return EFloat.NegativeInfinity;
        } else if (value.IsPositiveInfinity()) {
          return EFloat.PositiveInfinity;
        }
        if (value.CompareTo(0) == 0) {
          return EFloat.FromInt32(0).Plus(ctx);
        }
        int flags = ctx.Flags;
        EContext tmpctx = null;
        EFloat ret;
        // DebugUtility.Log("cmp=" +
        // value.CompareTo(EFloat.Create(1, -1)) +
        // " add=" + value.Add(EFloat.FromInt32(1)));
        if (value.CompareTo(EFloat.Create(1, -1)) < 0) {
          ret = value.Add(EFloat.FromInt32(1)).Log(ctx);
        } else {
          tmpctx = ctx.WithBigPrecision(ctx.Precision.Add(3)).WithBlankFlags();
          // DebugUtility.Log("orig "+value);
          // DebugUtility.Log("sub "+value.Add(EFloat.FromInt32(1),
          // tmpctx).Subtract(value));
          ret = value.Add(EFloat.FromInt32(1), tmpctx).Log(ctx);
          // DebugUtility.Log("ret "+ret);
          flags |= tmpctx.Flags;
        }
        if (ctx.HasFlags) {
          flags |= ctx.Flags;
          ctx.Flags = flags;
        }
        return ret;
      }
    }

    /// <summary>Finds the base-N logarithm of this object, that is, the
    /// power (exponent) that the number N must be raised to in order to
    /// equal this object's value.</summary>
    /// <param name='baseValue'>The parameter <paramref name='baseValue'/>
    /// is a Numbers.EFloat object.</param>
    /// <param name='ctx'>The parameter <paramref name='ctx'/> is a
    /// Numbers.EContext object.</param>
    /// <returns>Ln(this object)/Ln(baseValue). Signals the flag
    /// FlagInvalid and returns not-a-number (NaN) if this object is less
    /// than 0. Signals FlagInvalid and returns not-a-number (NaN) if the
    /// parameter <paramref name='ctx'/> is null or the precision is
    /// unlimited (the context's Precision property is 0).</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='baseValue'/> is null.</exception>
    public EFloat LogN(EFloat baseValue, EContext ctx) {
      EFloat value = this;
      if (baseValue == null) {
        throw new ArgumentNullException(nameof(baseValue));
      }
      if (value.IsNaN()) {
        return value.Plus(ctx);
      }
      if (baseValue.IsNaN()) {
        return baseValue.Plus(ctx);
      }
      if (ctx == null || !ctx.HasMaxPrecision ||
        (value.IsNegative && !value.IsZero) ||
        (baseValue.IsNegative && !baseValue.IsZero)) {
        return EFloat.SignalingNaN.Plus(ctx);
      }
      if (ctx.Traps != 0) {
        EContext tctx = ctx.GetNontrapping();
        EFloat ret = value.LogN(baseValue, tctx);
        return ctx.TriggerTraps(ret, tctx);
      } else if (ctx.IsSimplified) {
        EContext tmpctx = ctx.WithSimplified(false).WithBlankFlags();
        EFloat ret = value.PreRound(ctx).LogN(baseValue.PreRound(ctx),
            tmpctx);
        if (ctx.HasFlags) {
          int flags = ctx.Flags;
          ctx.Flags = flags | tmpctx.Flags;
        }
        // Console.WriteLine("{0} {1} [{4} {5}] -> {2}
        // [{3}]",value,baseValue,ret,ret.RoundToPrecision(ctx),
        // value.Quantize(value, ctx), baseValue.Quantize(baseValue, ctx));
        return ret.RoundToPrecision(ctx);
      } else {
        if (value.IsZero) {
          return baseValue.CompareTo(1) < 0 ? EFloat.PositiveInfinity :
            EFloat.NegativeInfinity;
        } else if (value.IsPositiveInfinity()) {
          return baseValue.CompareTo(1) < 0 ? EFloat.NegativeInfinity :
            EFloat.PositiveInfinity;
        }
        if (baseValue.CompareTo(2) == 0) {
          EFloat ev = value.Reduce(null);
          if (ev.UnsignedMantissa.CompareTo(1) == 0) {
            return EFloat.FromEInteger(ev.Exponent).Plus(ctx);
          }
        } else if (value.CompareTo(1) == 0) {
          return EFloat.FromInt32(0).Plus(ctx);
        } else if (value.CompareTo(baseValue) == 0) {
          return EFloat.FromInt32(1).Plus(ctx);
        }
        int flags = ctx.Flags;
        EContext tmpctx =
          ctx.WithBigPrecision(ctx.Precision.Add(3)).WithBlankFlags();
        EFloat ret = value.Log(tmpctx).Divide(baseValue.Log(tmpctx), ctx);
        if (ret.IsInteger() && !ret.IsZero) {
          flags |= EContext.FlagRounded | EContext.FlagInexact;
          if (baseValue.Pow(ret).CompareToValue(value) == 0) {
            EFloat rtmp = ret.Quantize(EFloat.FromInt32(1),
                ctx.WithNoFlags());
            if (!rtmp.IsNaN()) {
              flags &= ~(EContext.FlagRounded | EContext.FlagInexact);
              ret = rtmp;
            }
          }
        } else {
          flags |= tmpctx.Flags;
        }
        if (ctx.HasFlags) {
          flags |= ctx.Flags;
          ctx.Flags = flags;
        }
        return ret;
      }
    }

    /// <summary>Returns a number similar to this number but with the radix
    /// point moved to the left.</summary>
    /// <param name='places'>The number of binary digit places to move the
    /// radix point to the left. If this number is negative, instead moves
    /// the radix point to the right by this number's absolute
    /// value.</param>
    /// <returns>A number whose exponent is decreased by <paramref
    /// name='places'/>, but not to more than 0.</returns>
    public EFloat MovePointLeft(int places) {
      return this.MovePointLeft((EInteger)places, null);
    }

    /// <summary>Returns a number similar to this number but with the radix
    /// point moved to the left.</summary>
    /// <param name='places'>The number of binary digit places to move the
    /// radix point to the left. If this number is negative, instead moves
    /// the radix point to the right by this number's absolute
    /// value.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed.</param>
    /// <returns>A number whose exponent is decreased by <paramref
    /// name='places'/>, but not to more than 0.</returns>
    public EFloat MovePointLeft(int places, EContext ctx) {
      return this.MovePointLeft((EInteger)places, ctx);
    }

    /// <summary>Returns a number similar to this number but with the radix
    /// point moved to the left.</summary>
    /// <param name='bigPlaces'>The number of binary digit places to move
    /// the radix point to the left. If this number is negative, instead
    /// moves the radix point to the right by this number's absolute
    /// value.</param>
    /// <returns>A number whose exponent is decreased by <paramref
    /// name='bigPlaces'/>, but not to more than 0.</returns>
    public EFloat MovePointLeft(EInteger bigPlaces) {
      return this.MovePointLeft(bigPlaces, null);
    }

    /// <summary>Returns a number similar to this number but with the radix
    /// point moved to the left.</summary>
    /// <param name='bigPlaces'>The number of binary digit places to move
    /// the radix point to the left. If this number is negative, instead
    /// moves the radix point to the right by this number's absolute
    /// value.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed.</param>
    /// <returns>A number whose exponent is decreased by <paramref
    /// name='bigPlaces'/>, but not to more than 0.</returns>
    public EFloat MovePointLeft(
      EInteger bigPlaces,
      EContext ctx) {
      return (!this.IsFinite) ? this.RoundToPrecision(ctx) :
        this.MovePointRight(-(EInteger)bigPlaces, ctx);
    }

    /// <summary>Returns a number similar to this number but with the radix
    /// point moved to the right.</summary>
    /// <param name='places'>The number of binary digit places to move the
    /// radix point to the right. If this number is negative, instead moves
    /// the radix point to the left by this number's absolute
    /// value.</param>
    /// <returns>A number whose exponent is increased by <paramref
    /// name='places'/>, but not to more than 0.</returns>
    public EFloat MovePointRight(int places) {
      return this.MovePointRight((EInteger)places, null);
    }

    /// <summary>Returns a number similar to this number but with the radix
    /// point moved to the right.</summary>
    /// <param name='places'>The number of binary digit places to move the
    /// radix point to the right. If this number is negative, instead moves
    /// the radix point to the left by this number's absolute
    /// value.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed.</param>
    /// <returns>A number whose exponent is increased by <paramref
    /// name='places'/>, but not to more than 0.</returns>
    public EFloat MovePointRight(int places, EContext ctx) {
      return this.MovePointRight((EInteger)places, ctx);
    }

    /// <summary>Returns a number similar to this number but with the radix
    /// point moved to the right.</summary>
    /// <param name='bigPlaces'>The number of binary digit places to move
    /// the radix point to the right. If this number is negative, instead
    /// moves the radix point to the left by this number's absolute
    /// value.</param>
    /// <returns>A number whose exponent is increased by <paramref
    /// name='bigPlaces'/>, but not to more than 0.</returns>
    public EFloat MovePointRight(EInteger bigPlaces) {
      return this.MovePointRight(bigPlaces, null);
    }

    /// <summary>Returns a number similar to this number but with the radix
    /// point moved to the right.</summary>
    /// <param name='bigPlaces'>The number of binary digit places to move
    /// the radix point to the right. If this number is negative, instead
    /// moves the radix point to the left by this number's absolute
    /// value.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed.</param>
    /// <returns>A number whose exponent is increased by <paramref
    /// name='bigPlaces'/>, but not to more than 0.</returns>
    public EFloat MovePointRight(
      EInteger bigPlaces,
      EContext ctx) {
      if (!this.IsFinite) {
        return this.RoundToPrecision(ctx);
      }
      EInteger bigExp = this.Exponent;
      bigExp += bigPlaces;
      if (bigExp.Sign > 0) {
        EInteger mant = this.UnsignedMantissa.ShiftLeft(bigExp);
        return CreateWithFlags(
            mant,
            EInteger.Zero,
            this.flags).RoundToPrecision(ctx);
      }
      return CreateWithFlags(
          this.UnsignedMantissa,
          bigExp,
          this.flags).RoundToPrecision(ctx);
    }

    /// <summary>Multiplies this arbitrary-precision binary floating-point
    /// number by another arbitrary-precision binary floating-point number
    /// and returns the result. The exponent for the result is this
    /// arbitrary-precision binary floating-point number's exponent plus
    /// the other arbitrary-precision binary floating-point number's
    /// exponent.</summary>
    /// <param name='otherValue'>Another binary floating-point
    /// number.</param>
    /// <returns>The product of the two numbers, that is, this
    /// arbitrary-precision binary floating-point number times another
    /// arbitrary-precision binary floating-point number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='otherValue'/> is null.</exception>
    public EFloat Multiply(EFloat otherValue) {
      if (otherValue == null) {
        throw new ArgumentNullException(nameof(otherValue));
      }
      if (this.IsFinite && otherValue.IsFinite) {
        EInteger exp = this.Exponent.Add(otherValue.Exponent);
        int newflags = otherValue.flags ^ this.flags;
        if (this.unsignedMantissa.CanFitInInt32() &&
          otherValue.unsignedMantissa.CanFitInInt32()) {
          int integerA = this.unsignedMantissa.ToInt32();
          int integerB = otherValue.unsignedMantissa.ToInt32();
          long longA = ((long)integerA) * ((long)integerB);
          return CreateWithFlags(longA, exp, newflags);
        } else {
          EInteger eintA = this.UnsignedMantissa.Multiply(
              otherValue.UnsignedMantissa);
          return CreateWithFlags(eintA, exp, newflags);
        }
      }
      return this.Multiply(otherValue, EContext.UnlimitedHalfEven);
    }

    /// <summary>Multiplies this arbitrary-precision binary floating-point
    /// number by another arbitrary-precision binary floating-point number
    /// and returns the result.</summary>
    /// <param name='op'>Another binary floating-point number.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed.</param>
    /// <returns>The product of the two numbers, that is, this
    /// arbitrary-precision binary floating-point number times another
    /// arbitrary-precision binary floating-point number.</returns>
    public EFloat Multiply(
      EFloat op,
      EContext ctx) {
      return MathValue.Multiply(this, op, ctx);
    }

    /// <summary>Multiplies by one binary floating-point number, and then
    /// adds another binary floating-point number.</summary>
    /// <param name='multiplicand'>The value to multiply.</param>
    /// <param name='augend'>The value to add.</param>
    /// <returns>An arbitrary-precision binary floating-point
    /// number.</returns>
    public EFloat MultiplyAndAdd(
      EFloat multiplicand,
      EFloat augend) {
      return this.MultiplyAndAdd(multiplicand, augend, null);
    }

    /// <summary>Multiplies by one value, and then adds another
    /// value.</summary>
    /// <param name='op'>The value to multiply.</param>
    /// <param name='augend'>The value to add.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed. If the precision doesn't indicate a simplified
    /// arithmetic, rounding and precision/exponent adjustment is done only
    /// once, namely, after multiplying and adding.</param>
    /// <returns>The result thisValue * multiplicand + augend.</returns>
    public EFloat MultiplyAndAdd(
      EFloat op,
      EFloat augend,
      EContext ctx) {
      return MathValue.MultiplyAndAdd(this, op, augend, ctx);
    }

    /// <summary>Multiplies by one value, and then subtracts another
    /// value.</summary>
    /// <param name='op'>The value to multiply.</param>
    /// <param name='subtrahend'>The value to subtract.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed. If the precision doesn't indicate a simplified
    /// arithmetic, rounding and precision/exponent adjustment is done only
    /// once, namely, after multiplying and subtracting.</param>
    /// <returns>The result thisValue * multiplicand -
    /// subtrahend.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='op'/> or <paramref name='subtrahend'/> is null.</exception>
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
        negated = new EFloat(
          subtrahend.unsignedMantissa,
          subtrahend.exponent,
          (byte)newflags);
      }
      return MathValue.MultiplyAndAdd(this, op, negated, ctx);
    }

    /// <summary>Gets an object with the same value as this one, but with
    /// the sign reversed.</summary>
    /// <returns>An arbitrary-precision binary floating-point number. If
    /// this value is positive zero, returns negative zero. Returns
    /// signaling NaN if this value is signaling NaN. (In this sense, this
    /// method is similar to the "copy-negate" operation in the General
    /// Decimal Arithmetic Specification, except this method does not
    /// necessarily return a copy of this object.).</returns>
    public EFloat Negate() {
      return new EFloat(
          this.unsignedMantissa,
          this.exponent,
          (byte)(this.flags ^ BigNumberFlags.FlagNegative));
    }

    /// <summary>Returns a binary floating-point number with the same value
    /// as this object but with the sign reversed.</summary>
    /// <param name='context'>An arithmetic context to control the
    /// precision, rounding, and exponent range of the result. If
    /// <c>HasFlags</c> of the context is true, will also store the flags
    /// resulting from the operation (the flags are in addition to the
    /// pre-existing flags). Can be null, in which case the precision is
    /// unlimited and rounding isn't needed.</param>
    /// <returns>An arbitrary-precision binary floating-point number. If
    /// this value is positive zero, returns positive zero. Signals
    /// FlagInvalid and returns quiet NaN if this value is signaling
    /// NaN.</returns>
    public EFloat Negate(EContext context) {
      return MathValue.Negate(this, context);
    }

    /// <summary>Finds the largest value that's smaller than the given
    /// value.</summary>
    /// <param name='ctx'>An arithmetic context object to control the
    /// precision and exponent range of the result. The rounding mode from
    /// this context is ignored. If <c>HasFlags</c> of the context is true,
    /// will also store the flags resulting from the operation (the flags
    /// are in addition to the pre-existing flags).</param>
    /// <returns>Returns the largest value that's less than the given
    /// value. Returns negative infinity if the result is negative
    /// infinity. Signals FlagInvalid and returns not-a-number (NaN) if the
    /// parameter <paramref name='ctx'/> is null, the precision is 0, or
    /// <paramref name='ctx'/> has an unlimited exponent range.</returns>
    public EFloat NextMinus(EContext ctx) {
      return MathValue.NextMinus(this, ctx);
    }

    /// <summary>Finds the smallest value that's greater than the given
    /// value.</summary>
    /// <param name='ctx'>An arithmetic context object to control the
    /// precision and exponent range of the result. The rounding mode from
    /// this context is ignored. If <c>HasFlags</c> of the context is true,
    /// will also store the flags resulting from the operation (the flags
    /// are in addition to the pre-existing flags).</param>
    /// <returns>Returns the smallest value that's greater than the given
    /// value.Signals FlagInvalid and returns not-a-number (NaN) if the
    /// parameter <paramref name='ctx'/> is null, the precision is 0, or
    /// <paramref name='ctx'/> has an unlimited exponent range.</returns>
    public EFloat NextPlus(EContext ctx) {
      return MathValue.NextPlus(this, ctx);
    }

    /// <summary>Finds the next value that is closer to the other object's
    /// value than this object's value. Returns a copy of this value with
    /// the same sign as the other value if both values are
    /// equal.</summary>
    /// <param name='otherValue'>An arbitrary-precision binary
    /// floating-point number that the return value will approach.</param>
    /// <param name='ctx'>An arithmetic context object to control the
    /// precision and exponent range of the result. The rounding mode from
    /// this context is ignored. If <c>HasFlags</c> of the context is true,
    /// will also store the flags resulting from the operation (the flags
    /// are in addition to the pre-existing flags).</param>
    /// <returns>Returns the next value that is closer to the other object'
    /// s value than this object's value. Signals FlagInvalid and returns
    /// NaN if the parameter <paramref name='ctx'/> is null, the precision
    /// is 0, or <paramref name='ctx'/> has an unlimited exponent
    /// range.</returns>
    public EFloat NextToward(
      EFloat otherValue,
      EContext ctx) {
      return MathValue.NextToward(this, otherValue, ctx);
    }

    /// <summary>Rounds this object's value to a given precision, using the
    /// given rounding mode and range of exponent, and also converts
    /// negative zero to positive zero. The idiom
    /// <c>EDecimal.SignalingNaN.Plus(ctx)</c> is useful for triggering an
    /// invalid operation and returning not-a-number (NaN) for custom
    /// arithmetic operations.</summary>
    /// <param name='ctx'>A context for controlling the precision, rounding
    /// mode, and exponent range. Can be null, in which case the precision
    /// is unlimited and rounding isn't needed.</param>
    /// <returns>The closest value to this object's value, rounded to the
    /// specified precision. If <paramref name='ctx'/> is null or the
    /// precision and exponent range are unlimited, returns the same value
    /// as this object (or a quiet NaN if this object is a signaling
    /// NaN).</returns>
    public EFloat Plus(EContext ctx) {
      return MathValue.Plus(this, ctx);
    }

    /// <summary>Raises this object's value to the given exponent, using
    /// unlimited precision.</summary>
    /// <param name='exponent'>An arbitrary-precision binary floating-point
    /// number expressing the exponent to raise this object's value
    /// to.</param>
    /// <returns>This^exponent. Returns not-a-number (NaN) if the exponent
    /// has a fractional part.</returns>
    public EFloat Pow(EFloat exponent) {
      return this.Pow(exponent, null);
    }

    /// <summary>Raises this object's value to the given
    /// exponent.</summary>
    /// <param name='exponent'>An arbitrary-precision binary floating-point
    /// number expressing the exponent to raise this object's value
    /// to.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed.</param>
    /// <returns>This^exponent. Signals the flag FlagInvalid and returns
    /// NaN if this object and exponent are both 0; or if this value is
    /// less than 0 and the exponent either has a fractional part or is
    /// infinity. Signals FlagInvalid and returns not-a-number (NaN) if the
    /// parameter <paramref name='ctx'/> is null or the precision is
    /// unlimited (the context's Precision property is 0), and the exponent
    /// has a fractional part.</returns>
    public EFloat Pow(EFloat exponent, EContext ctx) {
      return MathValue.Power(this, exponent, ctx);
    }

    /// <summary>Raises this object's value to the given
    /// exponent.</summary>
    /// <param name='exponentSmall'>The exponent to raise this object's
    /// value to.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed.</param>
    /// <returns>This^exponent. Signals the flag FlagInvalid and returns
    /// NaN if this object and exponent are both 0.</returns>
    public EFloat Pow(int exponentSmall, EContext ctx) {
      return this.Pow(EFloat.FromInt64(exponentSmall), ctx);
    }

    /// <summary>Raises this object's value to the given
    /// exponent.</summary>
    /// <param name='exponentSmall'>The exponent to raise this object's
    /// value to.</param>
    /// <returns>This^exponent. Returns not-a-number (NaN) if this object
    /// and exponent are both 0.</returns>
    public EFloat Pow(int exponentSmall) {
      return this.Pow(EFloat.FromInt64(exponentSmall), null);
    }

    /// <summary>Finds the number of digits in this number's significand.
    /// Returns 1 if this value is 0, and 0 if this value is infinity or
    /// not-a-number (NaN).</summary>
    /// <returns>An arbitrary-precision integer.</returns>
    public EInteger Precision() {
      if (!this.IsFinite) {
        return EInteger.Zero;
      }
      return this.IsZero ? EInteger.One :
        this.UnsignedMantissa.GetSignedBitLengthAsEInteger();
    }

    /// <summary>Returns whether this object's value is an
    /// integer.</summary>
    /// <returns><c>true</c> if this object's value is an integer;
    /// otherwise, <c>false</c>.</returns>
    public bool IsInteger() {
      if (!this.IsFinite) {
        return false;
      }
      if (this.IsZero || this.Exponent.CompareTo(0) >= 0) {
        return true;
      } else {
        EInteger absexp = this.Exponent.Abs();
        EInteger mant = this.UnsignedMantissa;
        return mant.GetLowBitAsEInteger().CompareTo(absexp) >= 0;
      }
    }

    /// <summary>
    ///  Returns a binary floating-point number with the same
    /// value but a new exponent.
    /// <para>Note that this is not always the same as rounding to a given
    /// number of binary digit places, since it can fail if the difference
    /// between this value's exponent and the desired exponent is too big,
    /// depending on the maximum precision. If rounding to a number of
    /// binary digit places is desired, it's better to use the
    /// RoundToExponent and RoundToIntegral methods instead.</para>
    /// <para><b>Remark:</b>
    ///  This method can be used to implement
    /// fixed-point binary arithmetic, in which each binary floating-point
    /// number has a fixed number of digits after the radix point. The
    /// following code example returns a fixed-point number with up to 20
    /// digits before and exactly 5 digits after the radix point:</para>
    /// <code> &#x2f;&#x2a; After performing arithmetic operations, adjust
    /// &#x2f;&#x2a; the number to 5 &#x2f;&#x2a;
    /// &#x2a;&#x2f;&#x2a;&#x2f;&#x2a;&#x2f;
    /// digits after the radix point number = number.Quantize(
    /// EInteger.FromInt32(-5), &#x2f;&#x2a; five digits after the radix
    /// point&#x2a;&#x2f;
    /// EContext.ForPrecision(25) &#x2f;&#x2a; 25-digit
    /// precision);&#x2a;&#x2f;</code>
    /// <para>A fixed-point binary arithmetic in which no digits come after
    /// the radix point (a desired exponent of 0) is considered an "integer
    /// arithmetic".</para>
    /// </summary>
    /// <param name='desiredExponent'>The desired exponent for the result.
    /// The exponent is the number of fractional digits in the result,
    /// expressed as a negative number. Can also be positive, which
    /// eliminates lower-order places from the number. For example, -3
    /// means round to the thousandth (10^-3, 0.0001), and 3 means round to
    /// the thousand (10^3, 1000). A value of 0 rounds the number to an
    /// integer.</param>
    /// <param name='ctx'>An arithmetic context to control precision and
    /// rounding of the result. If <c>HasFlags</c>
    ///  of the context is true,
    /// will also store the flags resulting from the operation (the flags
    /// are in addition to the pre-existing flags). Can be null, in which
    /// case the default rounding mode is HalfEven.</param>
    /// <returns>A binary floating-point number with the same value as this
    /// object but with the exponent changed. Signals FlagInvalid and
    /// returns not-a-number (NaN) if this object is infinity, if the
    /// rounded result can't fit the given precision, or if the context
    /// defines an exponent range and the given exponent is outside that
    /// range.</returns>
    public EFloat Quantize(
      EInteger desiredExponent,
      EContext ctx) {
      return this.Quantize(
          EFloat.Create(EInteger.One, desiredExponent),
          ctx);
    }

    /// <summary>
    ///  Returns a binary floating-point number with the same
    /// value but a new exponent.
    /// <para>Note that this is not always the same as rounding to a given
    /// number of binary digit places, since it can fail if the difference
    /// between this value's exponent and the desired exponent is too big,
    /// depending on the maximum precision. If rounding to a number of
    /// binary digit places is desired, it's better to use the
    /// RoundToExponent and RoundToIntegral methods instead.</para>
    /// <para><b>Remark:</b>
    ///  This method can be used to implement
    /// fixed-point binary arithmetic, in which each binary floating-point
    /// number has a fixed number of digits after the radix point. The
    /// following code example returns a fixed-point number with up to 20
    /// digits before and exactly 5 digits after the radix point:</para>
    /// <code> &#x2f;&#x2a; After performing arithmetic operations, adjust
    /// &#x2f;&#x2a; the number to 5&#x2a;&#x2f;&#x2a;&#x2f;
    /// digits after the radix point number = number.Quantize(-5, &#x2f;&#x2a; five
    /// digits&#x2a;&#x2f;
    /// after the radix point EContext.ForPrecision(25) &#x2f;&#x2a; 25-digit
    /// precision);&#x2a;&#x2f;</code>
    /// <para>A fixed-point binary arithmetic in which no digits come after
    /// the radix point (a desired exponent of 0) is considered an "integer
    /// arithmetic".</para>
    /// </summary>
    /// <param name='desiredExponentInt'>The desired exponent for the
    /// result. The exponent is the number of fractional digits in the
    /// result, expressed as a negative number. Can also be positive, which
    /// eliminates lower-order places from the number. For example, -3
    /// means round to the thousandth (10^-3, 0.0001), and 3 means round to
    /// the thousand (10^3, 1000). A value of 0 rounds the number to an
    /// integer.</param>
    /// <param name='ctx'>An arithmetic context to control precision and
    /// rounding of the result. If <c>HasFlags</c>
    ///  of the context is true,
    /// will also store the flags resulting from the operation (the flags
    /// are in addition to the pre-existing flags). Can be null, in which
    /// case the default rounding mode is HalfEven.</param>
    /// <returns>A binary floating-point number with the same value as this
    /// object but with the exponent changed. Signals FlagInvalid and
    /// returns not-a-number (NaN) if this object is infinity, if the
    /// rounded result can't fit the given precision, or if the context
    /// defines an exponent range and the given exponent is outside that
    /// range.</returns>
    public EFloat Quantize(
      int desiredExponentInt,
      EContext ctx) {
      return this.Quantize(
          EFloat.Create(1, desiredExponentInt),
          ctx);
    }

    /// <summary>Returns a binary floating-point number with the same value
    /// as this object but with the same exponent as another binary
    /// floating-point number.
    /// <para>Note that this is not always the same as rounding to a given
    /// number of binary digit places, since it can fail if the difference
    /// between this value's exponent and the desired exponent is too big,
    /// depending on the maximum precision. If rounding to a number of
    /// binary digit places is desired, it's better to use the
    /// RoundToExponent and RoundToIntegral methods instead.</para>
    /// <para><b>Remark:</b> This method can be used to implement
    /// fixed-point binary arithmetic, in which a fixed number of digits
    /// come after the radix point. A fixed-point binary arithmetic in
    /// which no digits come after the radix point (a desired exponent of
    /// 0) is considered an "integer arithmetic" .</para></summary>
    /// <param name='otherValue'>A binary floating-point number containing
    /// the desired exponent of the result. The significand is ignored. The
    /// exponent is the number of fractional digits in the result,
    /// expressed as a negative number. Can also be positive, which
    /// eliminates lower-order places from the number. For example, -3
    /// means round to the sixteenth (10b^-3, 0.0001b), and 3 means round
    /// to the sixteen-place (10b^3, 1000b). A value of 0 rounds the number
    /// to an integer.</param>
    /// <param name='ctx'>An arithmetic context to control precision and
    /// rounding of the result. If <c>HasFlags</c> of the context is true,
    /// will also store the flags resulting from the operation (the flags
    /// are in addition to the pre-existing flags). Can be null, in which
    /// case the default rounding mode is HalfEven.</param>
    /// <returns>A binary floating-point number with the same value as this
    /// object but with the exponent changed. Signals FlagInvalid and
    /// returns not-a-number (NaN) if the result can't fit the given
    /// precision without rounding, or if the arithmetic context defines an
    /// exponent range and the given exponent is outside that
    /// range.</returns>
    public EFloat Quantize(
      EFloat otherValue,
      EContext ctx) {
      return MathValue.Quantize(this, otherValue, ctx);
    }

    /// <summary>Returns an object with the same numerical value as this
    /// one but with trailing zeros removed from its significand. For
    /// example, 1.00 becomes 1.
    /// <para>If this object's value is 0, changes the exponent to
    /// 0.</para></summary>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed.</param>
    /// <returns>This value with trailing zeros removed. Note that if the
    /// result has a very high exponent and the context says to clamp high
    /// exponents, there may still be some trailing zeros in the
    /// significand.</returns>
    public EFloat Reduce(EContext ctx) {
      return MathValue.Reduce(this, ctx);
    }

    /// <summary>Returns the remainder that would result when this
    /// arbitrary-precision binary floating-point number is divided by
    /// another arbitrary-precision binary floating-point number. The
    /// remainder is the number that remains when the absolute value of
    /// this arbitrary-precision binary floating-point number is divided
    /// (as though by DivideToIntegerZeroScale) by the absolute value of
    /// the other arbitrary-precision binary floating-point number; the
    /// remainder has the same sign (positive or negative) as this
    /// arbitrary-precision binary floating-point number.</summary>
    /// <param name='divisor'>An arbitrary-precision binary floating-point
    /// number.</param>
    /// <param name='ctx'>The parameter <paramref name='ctx'/> is an
    /// EContext object.</param>
    /// <returns>The remainder that would result when this
    /// arbitrary-precision binary floating-point number is divided by
    /// another arbitrary-precision binary floating-point number. Signals
    /// FlagDivideByZero and returns infinity if the divisor (this
    /// arbitrary-precision binary floating-point number) is 0 and the
    /// dividend (the other arbitrary-precision binary floating-point
    /// number) is nonzero. Signals FlagInvalid and returns not-a-number
    /// (NaN) if the divisor and the dividend are 0, or if the result of
    /// the division doesn't fit the given precision.</returns>
    public EFloat Remainder(
      EFloat divisor,
      EContext ctx) {
      return MathValue.Remainder(this, divisor, ctx, true);
    }

    /// <summary>Finds the remainder that results when dividing two
    /// arbitrary-precision binary floating-point numbers. The remainder is
    /// the value that remains when the absolute value of this object is
    /// divided by the absolute value of the other object; the remainder
    /// has the same sign (positive or negative) as this object's
    /// value.</summary>
    /// <param name='divisor'>An arbitrary-precision binary floating-point
    /// number.</param>
    /// <param name='ctx'>The parameter <paramref name='ctx'/> is an
    /// EContext object.</param>
    /// <returns>The remainder of the two numbers. Signals FlagInvalid and
    /// returns not-a-number (NaN) if the divisor is 0, or if the result
    /// doesn't fit the given precision.</returns>
    public EFloat RemainderNoRoundAfterDivide(
      EFloat divisor,
      EContext ctx) {
      return MathValue.Remainder(this, divisor, ctx, false);
    }

    /// <summary>Calculates the remainder of a number by the formula
    /// <c>"this" - (("this" / "divisor") * "divisor")</c>.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <returns>An arbitrary-precision binary floating-point
    /// number.</returns>
    public EFloat RemainderNaturalScale(
      EFloat divisor) {
      return this.RemainderNaturalScale(divisor, null);
    }

    /// <summary>Calculates the remainder of a number by the formula "this"
    /// - (("this" / "divisor") * "divisor").</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='ctx'>An arithmetic context object to control the
    /// precision, rounding, and exponent range of the result. This context
    /// will be used only in the division portion of the remainder
    /// calculation; as a result, it's possible for the return value to
    /// have a higher precision than given in this context. Flags will be
    /// set on the given context only if the context's <c>HasFlags</c> is
    /// true and the integer part of the division result doesn't fit the
    /// precision and exponent range without rounding. Can be null, in
    /// which the precision is unlimited and no additional rounding, other
    /// than the rounding down to an integer after division, is
    /// needed.</param>
    /// <returns>An arbitrary-precision binary floating-point
    /// number.</returns>
    public EFloat RemainderNaturalScale(
      EFloat divisor,
      EContext ctx) {
      return this.Subtract(
        this.DivideToIntegerNaturalScale(divisor, null).Multiply(divisor, null),
        ctx);
    }

    /// <summary>Finds the distance to the closest multiple of the given
    /// divisor, based on the result of dividing this object's value by
    /// another object's value.
    /// <list type=''>
    /// <item>If this and the other object divide evenly, the result is
    /// 0.</item>
    /// <item>If the remainder's absolute value is less than half of the
    /// divisor's absolute value, the result has the same sign as this
    /// object and will be the distance to the closest multiple.</item>
    /// <item>If the remainder's absolute value is more than half of the
    /// divisor's absolute value, the result has the opposite sign of this
    /// object and will be the distance to the closest multiple.</item>
    /// <item>If the remainder's absolute value is exactly half of the
    /// divisor's absolute value, the result has the opposite sign of this
    /// object if the quotient, rounded down, is odd, and has the same sign
    /// as this object if the quotient, rounded down, is even, and the
    /// result's absolute value is half of the divisor's absolute
    /// value.</item></list> This function is also known as the "IEEE
    /// Remainder" function.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='ctx'>An arithmetic context object to control the
    /// precision. The rounding and exponent range settings of this context
    /// are ignored (the rounding mode is always treated as HalfEven). If
    /// <c>HasFlags</c> of the context is true, will also store the flags
    /// resulting from the operation (the flags are in addition to the
    /// pre-existing flags). Can be null, in which the precision is
    /// unlimited.</param>
    /// <returns>The distance of the closest multiple. Signals FlagInvalid
    /// and returns not-a-number (NaN) if the divisor is 0, or either the
    /// result of integer division (the quotient) or the remainder wouldn't
    /// fit the given precision.</returns>
    public EFloat RemainderNear(
      EFloat divisor,
      EContext ctx) {
      return MathValue.RemainderNear(this, divisor, ctx);
    }

    /// <summary>Returns a binary floating-point number with the same value
    /// as this object but rounded to a new exponent if necessary. The
    /// resulting number's Exponent property will not necessarily be the
    /// given exponent; use the Quantize method instead to give the result
    /// a particular exponent.</summary>
    /// <param name='exponent'>The minimum exponent the result can have.
    /// This is the maximum number of fractional digits in the result,
    /// expressed as a negative number. Can also be positive, which
    /// eliminates lower-order places from the number. For example, -3
    /// means round to the thousandth (10^-3, 0.0001), and 3 means round to
    /// the thousand (10^3, 1000). A value of 0 rounds the number to an
    /// integer.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the default rounding mode is
    /// HalfEven.</param>
    /// <returns>A binary floating-point number rounded to the closest
    /// value representable in the given precision. If the result can't fit
    /// the precision, additional digits are discarded to make it fit.
    /// Signals FlagInvalid and returns not-a-number (NaN) if the
    /// arithmetic context defines an exponent range, the new exponent must
    /// be changed to the given exponent when rounding, and the given
    /// exponent is outside of the valid range of the arithmetic
    /// context.</returns>
    public EFloat RoundToExponent(
      EInteger exponent,
      EContext ctx) {
      return MathValue.RoundToExponentSimple(this, exponent, ctx);
    }

    /// <summary>Returns a binary floating-point number with the same value
    /// as this object but rounded to a new exponent if necessary. The
    /// resulting number's Exponent property will not necessarily be the
    /// given exponent; use the Quantize method instead to give the result
    /// a particular exponent.</summary>
    /// <param name='exponentSmall'>The minimum exponent the result can
    /// have. This is the maximum number of fractional digits in the
    /// result, expressed as a negative number. Can also be positive, which
    /// eliminates lower-order places from the number. For example, -3
    /// means round to the thousandth (10^-3, 0.0001), and 3 means round to
    /// the thousand (10^3, 1000). A value of 0 rounds the number to an
    /// integer.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the default rounding mode is
    /// HalfEven.</param>
    /// <returns>A binary floating-point number rounded to the closest
    /// value representable in the given precision. If the result can't fit
    /// the precision, additional digits are discarded to make it fit.
    /// Signals FlagInvalid and returns not-a-number (NaN) if the
    /// arithmetic context defines an exponent range, the new exponent must
    /// be changed to the given exponent when rounding, and the given
    /// exponent is outside of the valid range of the arithmetic
    /// context.</returns>
    public EFloat RoundToExponent(
      int exponentSmall,
      EContext ctx) {
      return this.RoundToExponent((EInteger)exponentSmall, ctx);
    }

    /// <summary>Returns a binary floating-point number with the same value
    /// as this object but rounded to the given exponent, and signals an
    /// inexact flag if the result would be inexact. The resulting number's
    /// Exponent property will not necessarily be the given exponent; use
    /// the Quantize method instead to give the result a particular
    /// exponent.</summary>
    /// <param name='exponent'>The minimum exponent the result can have.
    /// This is the maximum number of fractional digits in the result,
    /// expressed as a negative number. Can also be positive, which
    /// eliminates lower-order places from the number. For example, -3
    /// means round to the thousandth (10^-3, 0.0001), and 3 means round to
    /// the thousand (10^3, 1000). A value of 0 rounds the number to an
    /// integer.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the default rounding mode is
    /// HalfEven.</param>
    /// <returns>A binary floating-point number rounded to the closest
    /// value representable in the given precision. Signals FlagInvalid and
    /// returns not-a-number (NaN) if the result can't fit the given
    /// precision without rounding. Signals FlagInvalid and returns
    /// not-a-number (NaN) if the arithmetic context defines an exponent
    /// range, the new exponent must be changed to the given exponent when
    /// rounding, and the given exponent is outside of the valid range of
    /// the arithmetic context.</returns>
    public EFloat RoundToExponentExact(
      EInteger exponent,
      EContext ctx) {
      return MathValue.RoundToExponentExact(this, exponent, ctx);
    }

    /// <summary>Returns a binary number with the same value as this object
    /// but rounded to the given exponent. The resulting number's Exponent
    /// property will not necessarily be the given exponent; use the
    /// Quantize method instead to give the result a particular
    /// exponent.</summary>
    /// <param name='exponent'>The minimum exponent the result can have.
    /// This is the maximum number of fractional digits in the result,
    /// expressed as a negative number. Can also be positive, which
    /// eliminates lower-order places from the number. For example, -3
    /// means round to the eighth (10^-1, 1/8), and 3 means round to the
    /// eight (2^3, 8). A value of 0 rounds the number to an
    /// integer.</param>
    /// <param name='rounding'>Desired mode for rounding this object's
    /// value.</param>
    /// <returns>A binary number rounded to the closest value representable
    /// in the given precision.</returns>
    public EFloat RoundToExponentExact(
      EInteger exponent,
      ERounding rounding) {
      return MathValue.RoundToExponentExact(
          this,
          exponent,
          EContext.Unlimited.WithRounding(rounding));
    }

    /// <summary>Returns a binary floating-point number with the same value
    /// as this object but rounded to the given exponent represented as a
    /// 32-bit signed integer, and signals an inexact flag if the result
    /// would be inexact. The resulting number's Exponent property will not
    /// necessarily be the given exponent; use the Quantize method instead
    /// to give the result a particular exponent.</summary>
    /// <param name='exponentSmall'>The minimum exponent the result can
    /// have. This is the maximum number of fractional digits in the
    /// result, expressed as a negative number. Can also be positive, which
    /// eliminates lower-order places from the number. For example, -3
    /// means round to the thousandth (10^-3, 0.0001), and 3 means round to
    /// the thousand (10^3, 1000). A value of 0 rounds the number to an
    /// integer.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the default rounding mode is
    /// HalfEven.</param>
    /// <returns>A binary floating-point number rounded to the closest
    /// value representable in the given precision. Signals FlagInvalid and
    /// returns not-a-number (NaN) if the result can't fit the given
    /// precision without rounding. Signals FlagInvalid and returns
    /// not-a-number (NaN) if the arithmetic context defines an exponent
    /// range, the new exponent must be changed to the given exponent when
    /// rounding, and the given exponent is outside of the valid range of
    /// the arithmetic context.</returns>
    public EFloat RoundToExponentExact(
      int exponentSmall,
      EContext ctx) {
      return this.RoundToExponentExact((EInteger)exponentSmall, ctx);
    }

    /// <summary>Returns a binary floating-point number with the same value
    /// as this object but rounded to an integer, and signals an inexact
    /// flag if the result would be inexact. The resulting number's
    /// Exponent property will not necessarily be 0; use the Quantize
    /// method instead to give the result an exponent of 0.</summary>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the default rounding mode is
    /// HalfEven.</param>
    /// <returns>A binary floating-point number rounded to the closest
    /// integer representable in the given precision. Signals FlagInvalid
    /// and returns not-a-number (NaN) if the result can't fit the given
    /// precision without rounding. Signals FlagInvalid and returns
    /// not-a-number (NaN) if the arithmetic context defines an exponent
    /// range, the new exponent must be changed to 0 when rounding, and 0
    /// is outside of the valid range of the arithmetic context.</returns>
    public EFloat RoundToIntegerExact(EContext ctx) {
      return MathValue.RoundToExponentExact(this, EInteger.Zero, ctx);
    }

    /// <summary>Returns a binary floating-point number with the same value
    /// as this object but rounded to an integer, without adding the
    /// <c>FlagInexact</c> or <c>FlagRounded</c> flags. The resulting
    /// number's Exponent property will not necessarily be 0; use the
    /// Quantize method instead to give the result an exponent of
    /// 0.</summary>
    /// <param name='ctx'>An arithmetic context to control precision and
    /// rounding of the result. If <c>HasFlags</c> of the context is true,
    /// will also store the flags resulting from the operation (the flags
    /// are in addition to the pre-existing flags), except that this
    /// function will never add the <c>FlagRounded</c> and
    /// <c>FlagInexact</c> flags (the only difference between this and
    /// RoundToExponentExact). Can be null, in which case the default
    /// rounding mode is HalfEven.</param>
    /// <returns>A binary floating-point number rounded to the closest
    /// integer representable in the given precision. If the result can't
    /// fit the precision, additional digits are discarded to make it fit.
    /// Signals FlagInvalid and returns not-a-number (NaN) if the
    /// arithmetic context defines an exponent range, the new exponent must
    /// be changed to 0 when rounding, and 0 is outside of the valid range
    /// of the arithmetic context.</returns>
    public EFloat RoundToIntegerNoRoundedFlag(EContext ctx) {
      return MathValue.RoundToExponentNoRoundedFlag(this, EInteger.Zero, ctx);
    }

    /// <summary>Returns a binary floating-point number with the same value
    /// as this object but rounded to an integer, and signals an inexact
    /// flag if the result would be inexact.</summary>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the default rounding mode is
    /// HalfEven.</param>
    /// <returns>A binary floating-point number rounded to the closest
    /// integer representable in the given precision. Signals FlagInvalid
    /// and returns not-a-number (NaN) if the result can't fit the given
    /// precision without rounding. Signals FlagInvalid and returns
    /// not-a-number (NaN) if the arithmetic context defines an exponent
    /// range, the new exponent must be changed to 0 when rounding, and 0
    /// is outside of the valid range of the arithmetic context.</returns>
    [Obsolete("Renamed to RoundToIntegerExact.")]
    public EFloat RoundToIntegralExact(EContext ctx) {
      return MathValue.RoundToExponentExact(this, EInteger.Zero, ctx);
    }

    /// <summary>Returns a binary floating-point number with the same value
    /// as this object but rounded to an integer, without adding the
    /// <c>FlagInexact</c> or <c>FlagRounded</c> flags.</summary>
    /// <param name='ctx'>An arithmetic context to control precision and
    /// rounding of the result. If <c>HasFlags</c> of the context is true,
    /// will also store the flags resulting from the operation (the flags
    /// are in addition to the pre-existing flags), except that this
    /// function will never add the <c>FlagRounded</c> and
    /// <c>FlagInexact</c> flags (the only difference between this and
    /// RoundToExponentExact). Can be null, in which case the default
    /// rounding mode is HalfEven.</param>
    /// <returns>A binary floating-point number rounded to the closest
    /// integer representable in the given precision. If the result can't
    /// fit the precision, additional digits are discarded to make it fit.
    /// Signals FlagInvalid and returns not-a-number (NaN) if the
    /// arithmetic context defines an exponent range, the new exponent must
    /// be changed to 0 when rounding, and 0 is outside of the valid range
    /// of the arithmetic context.</returns>
    [Obsolete("Renamed to RoundToIntegerNoRoundedFlag.")]
    public EFloat RoundToIntegralNoRoundedFlag(EContext ctx) {
      return MathValue.RoundToExponentNoRoundedFlag(this, EInteger.Zero, ctx);
    }

    /// <summary>Rounds this object's value to a given precision, using the
    /// given rounding mode and range of exponent.</summary>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and no
    /// rounding is needed.</param>
    /// <returns>The closest value to this object's value, rounded to the
    /// specified precision. Returns the same value as this object if
    /// <paramref name='ctx'/> is null or the precision and exponent range
    /// are unlimited.</returns>
    public EFloat RoundToPrecision(EContext ctx) {
      return MathValue.RoundToPrecision(this, ctx);
    }

    /// <summary>Returns a number in which the value of this object is
    /// rounded to fit the maximum precision allowed if it has more
    /// significant digits than the maximum precision. The maximum
    /// precision allowed is given in an arithmetic context. This method is
    /// designed for preparing operands to a custom arithmetic operation in
    /// accordance with the "simplified" arithmetic given in Appendix A of
    /// the General Decimal Arithmetic Specification.</summary>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited. Signals the
    /// flag LostDigits if the input number has greater precision than
    /// allowed and was rounded to a different numerical value in order to
    /// fit the precision.</param>
    /// <returns>This object rounded to the given precision. Returns this
    /// object and signals no flags if <paramref name='ctx'/> is null or
    /// specifies an unlimited precision, if this object is infinity or
    /// not-a-number (including signaling NaN), or if the number's value
    /// has no more significant digits than the maximum precision given in
    /// <paramref name='ctx'/>.</returns>
    public EFloat PreRound(EContext ctx) {
      return NumberUtility.PreRound(this, ctx, MathValue);
    }

    /// <summary>Returns a number similar to this number but with the scale
    /// adjusted.</summary>
    /// <param name='places'>The parameter <paramref name='places'/> is a
    /// 32-bit signed integer.</param>
    /// <returns>An arbitrary-precision binary floating-point
    /// number.</returns>
    public EFloat ScaleByPowerOfTwo(int places) {
      return this.ScaleByPowerOfTwo((EInteger)places, null);
    }

    /// <summary>Returns a number similar to this number but with the scale
    /// adjusted.</summary>
    /// <param name='places'>The parameter <paramref name='places'/> is a
    /// 32-bit signed integer.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null.</param>
    /// <returns>An arbitrary-precision binary floating-point
    /// number.</returns>
    public EFloat ScaleByPowerOfTwo(int places, EContext ctx) {
      return this.ScaleByPowerOfTwo((EInteger)places, ctx);
    }

    /// <summary>Returns a number similar to this number but with the scale
    /// adjusted.</summary>
    /// <param name='bigPlaces'>An arbitrary-precision integer.</param>
    /// <returns>A number whose exponent is increased by <paramref
    /// name='bigPlaces'/>.</returns>
    public EFloat ScaleByPowerOfTwo(EInteger bigPlaces) {
      return this.ScaleByPowerOfTwo(bigPlaces, null);
    }

    /// <summary>Returns a number similar to this number but with its scale
    /// adjusted.</summary>
    /// <param name='bigPlaces'>An arbitrary-precision integer.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null.</param>
    /// <returns>An arbitrary-precision binary floating-point
    /// number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bigPlaces'/> is null.</exception>
    public EFloat ScaleByPowerOfTwo(
      EInteger bigPlaces,
      EContext ctx) {
      if (bigPlaces == null) {
        throw new ArgumentNullException(nameof(bigPlaces));
      }
      if (bigPlaces.IsZero) {
        return this.RoundToPrecision(ctx);
      }
      if (!this.IsFinite) {
        return this.RoundToPrecision(ctx);
      }
      EInteger bigExp = this.Exponent;
      bigExp += bigPlaces;
      return new EFloat(
          this.unsignedMantissa,
          FastIntegerFixed.FromBig(bigExp),
          (byte)this.flags).RoundToPrecision(ctx);
    }

    /// <summary>Finds the square root of this object's value.</summary>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// <i>This parameter can't be null, as the square root function's
    /// results are generally not exact for many inputs.</i> (Unlike in the
    /// General Binary Arithmetic Specification, any rounding mode is
    /// allowed.).</param>
    /// <returns>The square root. Signals the flag FlagInvalid and returns
    /// NaN if this object is less than 0 (the square root would be a
    /// complex number, but the return value is still NaN). Signals
    /// FlagInvalid and returns not-a-number (NaN) if the parameter
    /// <paramref name='ctx'/> is null or the precision is unlimited (the
    /// context's Precision property is 0).</returns>
    public EFloat Sqrt(EContext ctx) {
      return MathValue.SquareRoot(this, ctx);
    }

    /// <summary>Finds the square root of this object's value.</summary>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// <i>This parameter can't be null, as the square root function's
    /// results are generally not exact for many inputs.</i> (Unlike in the
    /// General Binary Arithmetic Specification, any rounding mode is
    /// allowed.).</param>
    /// <returns>The square root. Signals the flag FlagInvalid and returns
    /// NaN if this object is less than 0 (the square root would be a
    /// complex number, but the return value is still NaN). Signals
    /// FlagInvalid and returns not-a-number (NaN) if the parameter
    /// <paramref name='ctx'/> is null or the precision is unlimited (the
    /// context's Precision property is 0).</returns>
    [Obsolete("Renamed to Sqrt.")]
    public EFloat SquareRoot(EContext ctx) {
      return MathValue.SquareRoot(this, ctx);
    }

    /// <summary>Subtracts an arbitrary-precision binary floating-point
    /// number from this arbitrary-precision binary floating-point number
    /// and returns the result. The exponent for the result is the lower of
    /// this arbitrary-precision binary floating-point number's exponent
    /// and the other arbitrary-precision binary floating-point number's
    /// exponent.</summary>
    /// <param name='otherValue'>The number to subtract from this
    /// instance's value.</param>
    /// <returns>The difference between the two numbers, that is, this
    /// arbitrary-precision binary floating-point number minus another
    /// arbitrary-precision binary floating-point number. If this
    /// arbitrary-precision binary floating-point number is not-a-number
    /// (NaN), returns NaN.</returns>
    public EFloat Subtract(EFloat otherValue) {
      return this.Subtract(otherValue, null);
    }

    /// <summary>Subtracts an arbitrary-precision binary floating-point
    /// number from this arbitrary-precision binary floating-point number
    /// and returns the result.</summary>
    /// <param name='otherValue'>The number to subtract from this
    /// instance's value.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and no
    /// rounding is needed.</param>
    /// <returns>The difference between the two numbers, that is, this
    /// arbitrary-precision binary floating-point number minus another
    /// arbitrary-precision binary floating-point number. If this
    /// arbitrary-precision binary floating-point number is not-a-number
    /// (NaN), returns NaN.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='otherValue'/> is null.</exception>
    public EFloat Subtract(
      EFloat otherValue,
      EContext ctx) {
      if (otherValue == null) {
        throw new ArgumentNullException(nameof(otherValue));
      }
      EFloat negated = otherValue;
      if ((otherValue.flags & BigNumberFlags.FlagNaN) == 0) {
        int newflags = otherValue.flags ^ BigNumberFlags.FlagNegative;
        negated = new EFloat(
          otherValue.unsignedMantissa,
          otherValue.exponent,
          (byte)newflags);
      }
      return this.Add(negated, ctx);
    }

    /// <summary>Converts this value to a 64-bit floating-point number
    /// encoded in the IEEE 754 binary64 format.</summary>
    /// <returns>This number, converted to a 64-bit floating-point number
    /// encoded in the IEEE 754 binary64 format. The return value can be
    /// positive infinity or negative infinity if this value exceeds the
    /// range of a 64-bit floating point number.</returns>
    public double ToDouble() {
      long value = this.ToDoubleBits();
      return BitConverter.ToDouble(BitConverter.GetBytes((long)value), 0);
    }

    /// <summary>Converts this value to its closest equivalent as 32-bit
    /// floating-point number, expressed as an integer in the IEEE 754
    /// binary32 format. The half-even rounding mode is used.
    /// <para>If this value is a NaN, sets the high bit of the 32-bit
    /// floating point number's significand area for a quiet NaN, and
    /// clears it for a signaling NaN. Then the other bits of the
    /// significand area are set to the lowest bits of this object's
    /// unsigned significand, and the next-highest bit of the significand
    /// area is set if those bits are all zeros and this is a signaling
    /// NaN.</para></summary>
    /// <returns>The closest 32-bit binary floating-point number to this
    /// value, expressed as an integer in the IEEE 754 binary32 format. The
    /// return value can be positive infinity or negative infinity if this
    /// value exceeds the range of a 32-bit floating point
    /// number.</returns>
    public int ToSingleBits() {
      if (this.IsPositiveInfinity()) {
        return 0x7f800000;
      }
      if (this.IsNegativeInfinity()) {
        return unchecked((int)0xff800000);
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
        return nan;
      }
      EFloat thisValue = this;
      // DebugUtility.Log("beforeround=" +thisValue + " ["+
      // thisValue.Mantissa + " " + thisValue.Exponent);
      // Check whether rounding can be avoided for common cases
      // where the value already fits a single
      if (!thisValue.IsFinite ||
        thisValue.unsignedMantissa.CompareToInt(0x1000000) >= 0 ||
        thisValue.exponent.CompareToInt(-95) < 0 ||
        thisValue.exponent.CompareToInt(95) > 0) {
        thisValue = this.RoundToPrecision(EContext.Binary32);
      }
      // DebugUtility.Log("afterround=" +thisValue + " ["+
      // thisValue.Mantissa + " " + thisValue.Exponent);
      if (!thisValue.IsFinite) {
        return thisValue.ToSingleBits();
      }
      int intmant = thisValue.unsignedMantissa.ToInt32();
      if (thisValue.IsNegative && intmant == 0) {
        return (int)1 << 31;
      } else if (intmant == 0) {
        return 0;
      }
      int intBitLength = NumberUtility.BitLength(intmant);
      int expo = thisValue.exponent.ToInt32();
      var subnormal = false;
      if (intBitLength < 24) {
        int diff = 24 - intBitLength;
        expo -= diff;
        if (expo < -149) {
          // DebugUtility.Log("Diff changed from " + diff + " to " + (diff -
          // (-149 - expo)));
          diff -= -149 - expo;
          expo = -149;
          subnormal = true;
        }
        intmant <<= diff;
      }
      // DebugUtility.Log("intmant=" + intmant + " " + intBitLength +
      // " expo=" + expo +
      // " subnormal=" + subnormal);
      int smallmantissa = intmant & 0x7fffff;
      if (!subnormal) {
        smallmantissa |= (expo + 150) << 23;
      }
      if (this.IsNegative) {
        smallmantissa |= 1 << 31;
      }
      return smallmantissa;
    }

    /// <summary>Converts this value to its closest equivalent as a 64-bit
    /// floating-point number, expressed as an integer in the IEEE 754
    /// binary64 format. The half-even rounding mode is used.
    /// <para>If this value is a NaN, sets the high bit of the 64-bit
    /// floating point number's significand area for a quiet NaN, and
    /// clears it for a signaling NaN. Then the other bits of the
    /// significand area are set to the lowest bits of this object's
    /// unsigned significand, and the next-highest bit of the significand
    /// area is set if those bits are all zeros and this is a signaling
    /// NaN.</para></summary>
    /// <returns>The closest 64-bit binary floating-point number to this
    /// value, expressed as an integer in the IEEE 754 binary64 format. The
    /// return value can be positive infinity or negative infinity if this
    /// value exceeds the range of a 64-bit floating point
    /// number.</returns>
    public long ToDoubleBits() {
      if (this.IsPositiveInfinity()) {
        return unchecked((long)0x7ff0000000000000L);
      }
      if (this.IsNegativeInfinity()) {
        return unchecked((long)0xfff0000000000000L);
      }
      if (this.IsNaN()) {
        int[] nan = { 0, 0x7ff00000 };
        if (this.IsNegative) {
          nan[1] |= unchecked((int)(1 << 31));
        }
        if (this.IsQuietNaN()) {
          // Quiet NaN is a NaN in which the highest bit of
          // the mantissa area is set
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
        long lret = unchecked(((long)nan[0]) & 0xffffffffL);
        lret |= unchecked(((long)nan[1]) << 32);
        /*
         DebugUtility.Log("lret={0:X8} {1:X8} {2:X}", nan[0], nan[1], lret);
        */ return lret;
      }
      EFloat thisValue = this;
      // Check whether rounding can be avoided for common cases
      // where the value already fits a double
      if (!thisValue.IsFinite ||
        thisValue.unsignedMantissa.CompareToInt64(1L << 52) >= 0 ||
        thisValue.exponent.CompareToInt(-900) < 0 ||
        thisValue.exponent.CompareToInt(900) > 0) {
        thisValue = this.RoundToPrecision(EContext.Binary64);
      }
      if (!thisValue.IsFinite) {
        return thisValue.ToDoubleBits();
      }
      long longmant = thisValue.unsignedMantissa.ToInt64();
      if (thisValue.IsNegative && longmant == 0) {
        return 1L << 63;
      } else if (longmant == 0) {
        return 0L;
      }
      // DebugUtility.Log("todouble -->" + this);
      long longBitLength = NumberUtility.BitLength(longmant);
      int expo = thisValue.exponent.ToInt32();
      var subnormal = false;
      if (longBitLength < 53) {
        int diff = 53 - (int)longBitLength;
        expo -= diff;
        if (expo < -1074) {
          // DebugUtility.Log("Diff changed from " + diff + " to " + (diff -
          // (-1074 - expo)));
          diff -= -1074 - expo;
          expo = -1074;
          subnormal = true;
        }
        longmant <<= diff;
      }
      // Clear the high bits where the exponent and sign are
      longmant &= 0xfffffffffffffL;
      if (!subnormal) {
        longmant |= (long)(expo + 1075) << 52;
      }
      if (thisValue.IsNegative) {
        longmant |= unchecked((long)(1L << 63));
      }
      return longmant;
    }

    /// <summary>Converts this value to an arbitrary-precision decimal
    /// number.</summary>
    /// <returns>This number, converted to an arbitrary-precision decimal
    /// number.</returns>
    public EDecimal ToEDecimal() {
      return EDecimal.FromEFloat(this);
    }

    /// <summary>Converts this value to an arbitrary-precision integer. Any
    /// fractional part of this value will be discarded when converting to
    /// an arbitrary-precision integer. Note that depending on the value,
    /// especially the exponent, generating the arbitrary-precision integer
    /// may require a huge amount of memory. Use the ToSizedEInteger method
    /// to convert a number to an EInteger only if the integer fits in a
    /// bounded bit range; that method will throw an exception on
    /// overflow.</summary>
    /// <returns>An arbitrary-precision integer.</returns>
    /// <exception cref='OverflowException'>This object's value is infinity
    /// or not-a-number (NaN).</exception>
    public EInteger ToEInteger() {
      return this.ToEIntegerInternal(false);
    }

    /// <summary>Converts this value to an arbitrary-precision integer,
    /// checking whether the value contains a fractional part. Note that
    /// depending on the value, especially the exponent, generating the
    /// arbitrary-precision integer may require a huge amount of memory.
    /// Use the ToSizedEIntegerIfExact method to convert a number to an
    /// EInteger only if the integer fits in a bounded bit range; that
    /// method will throw an exception on overflow.</summary>
    /// <returns>An arbitrary-precision integer.</returns>
    /// <exception cref='OverflowException'>This object's value is infinity
    /// or not-a-number (NaN).</exception>
    /// <exception cref='ArithmeticException'>This object's value is not an
    /// exact integer.</exception>
    [Obsolete("Renamed to ToEIntegerIfExact.")]
    public EInteger ToEIntegerExact() {
      return this.ToEIntegerInternal(true);
    }

    /// <summary>Converts this value to an arbitrary-precision integer,
    /// checking whether the value contains a fractional part. Note that
    /// depending on the value, especially the exponent, generating the
    /// arbitrary-precision integer may require a huge amount of memory.
    /// Use the ToSizedEIntegerIfExact method to convert a number to an
    /// EInteger only if the integer fits in a bounded bit range; that
    /// method will throw an exception on overflow.</summary>
    /// <returns>An arbitrary-precision integer.</returns>
    /// <exception cref='OverflowException'>This object's value is infinity
    /// or not-a-number (NaN).</exception>
    /// <exception cref='ArithmeticException'>This object's value is not an
    /// exact integer.</exception>
    public EInteger ToEIntegerIfExact() {
      return this.ToEIntegerInternal(true);
    }

    /// <summary>Converts this value to an arbitrary-precision decimal
    /// number, then returns the value of that decimal's
    /// ToEngineeringString method.</summary>
    /// <returns>A text string.</returns>
    public string ToEngineeringString() {
      return this.ToEDecimal().ToEngineeringString();
    }

    /// <summary>Converts this value to an arbitrary-precision decimal
    /// number.</summary>
    /// <returns>An arbitrary-precision decimal number.</returns>
    [Obsolete("Renamed to ToEDecimal.")]
    public EDecimal ToExtendedDecimal() {
      return EDecimal.FromEFloat(this);
    }

    /// <summary>Converts this value to a string, but without exponential
    /// notation.</summary>
    /// <returns>A text string.</returns>
    public string ToPlainString() {
      return this.ToEDecimal().ToPlainString();
    }

    private string ToDebugString() {
      return "[" + this.Mantissa.ToRadixString(2) +
        "," + this.Mantissa.GetUnsignedBitLengthAsEInteger() +
        "," + this.Exponent + "]";
    }

    /// <summary>Returns a string representation of this number's value
    /// after rounding that value to the given precision (using the given
    /// arithmetic context, such as <c>EContext.Binary64</c>
    ///  ). If the
    /// number after rounding is neither infinity nor not-a-number (NaN),
    /// returns the shortest decimal form of this number's value (in terms
    /// of decimal digits starting with the first nonzero digit and ending
    /// with the last nonzero digit) that results in the rounded number
    /// after the decimal form is converted to binary floating-point format
    /// (using the given arithmetic context).</summary>
    /// <param name='ctx'>An arithmetic context to control precision (in
    /// bits), rounding, and exponent range of the rounded number. If
    /// <c>HasFlags</c>
    ///  of the context is true, will also store the flags
    /// resulting from the operation (the flags are in addition to the
    /// pre-existing flags). Can be null. If this parameter is null or
    /// defines no maximum precision, returns the same value as the
    /// ToString() method.</param>
    /// <returns>Shortest decimal form of this number's value for the given
    /// arithmetic context. The text string will be in exponential notation
    /// (expressed as a number 1 or greater, but less than 10, times a
    /// power of 10) if the number's first nonzero decimal digit is more
    /// than five digits after the decimal point, or if the number's
    /// exponent is greater than 0 and its value is 10, 000, 000 or
    /// greater.</returns>
    /// <example>
    /// <para>The following example converts an EFloat number to its
    /// shortest round-tripping decimal form using the same precision as
    /// the <c>double</c>
    ///  type in Java and.NET:</para>
    /// <code> String str = efloat.ToShortestString(EContext.Binary64); </code>
    /// </example>
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
      if (this.IsZero) {
        return this.RoundToPrecision(ctx).ToString();
      }
      // NOTE: The original EFloat is converted to decimal,
      // not the rounded version, to avoid double rounding issues
      EDecimal dec = this.ToEDecimal();
      if (ctx.Precision.CompareTo(10) >= 0) {
        // Preround the decimal so the significand has closer to the
        // number of decimal digits of the maximum possible
        // decimal significand, to speed up further rounding
        EInteger roundedPrec = ctx.Precision.ShiftRight(1).Add(
            EInteger.FromInt32(3));
        EInteger dmant = dec.UnsignedMantissa;
        EInteger dexp = dec.Exponent;
        bool dneg = dec.IsNegative;
        var dsa = new DigitShiftAccumulator(dmant, 0, 0);
        dsa.ShiftToDigits(FastInteger.FromBig(roundedPrec), null, false);
        dmant = dsa.ShiftedInt;
        dexp = dexp.Add(dsa.DiscardedDigitCount.ToEInteger());
        if (dsa.LastDiscardedDigit != 0 || dsa.OlderDiscardedDigits != 0) {
          if (dmant.Remainder(10).ToInt32Checked() != 9) {
            dmant = dmant.Add(1);
          }
        }
        dec = EDecimal.Create(dmant, dexp);
        if (dneg) {
          dec = dec.Negate();
        }
      }
      bool mantissaIsPowerOfTwo = this.UnsignedMantissa.IsPowerOfTwo;
      EInteger eprecision = EInteger.Zero;
      while (true) {
        EInteger nextPrecision = eprecision.Add(EInteger.One);
        EContext nextCtx = ctx2.WithBigPrecision(nextPrecision);
        EDecimal nextDec = dec.RoundToPrecision(nextCtx);
        EFloat newFloat = nextDec.ToEFloat(ctx2);
        // DebugUtility.Log("nextDec=" + nextDec);
        if (newFloat.CompareTo(valueEfRnd) == 0) {
          if (mantissaIsPowerOfTwo && eprecision.Sign > 0) {
            nextPrecision = eprecision;
            nextCtx = ctx2.WithBigPrecision(nextPrecision);
            #if DEBUG
            if (!nextCtx.HasMaxPrecision) {
              throw new InvalidOperationException("mant=" + this.Mantissa +
                "," + "\u0020 exp=" + this.Exponent);
            }
            #endif
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

    /// <summary>Converts this value to its closest equivalent as a 32-bit
    /// floating-point number. The half-even rounding mode is used.
    /// <para>If this value is a NaN, sets the high bit of the 32-bit
    /// floating point number's significand area for a quiet NaN, and
    /// clears it for a signaling NaN. Then the other bits of the
    /// significand area are set to the lowest bits of this object's
    /// unsigned significand, and the next-highest bit of the significand
    /// area is set if those bits are all zeros and this is a signaling
    /// NaN. Unfortunately, in the.NET implementation, the return value of
    /// this method may be a quiet NaN even if a signaling NaN would
    /// otherwise be generated.</para></summary>
    /// <returns>The closest 32-bit binary floating-point number to this
    /// value. The return value can be positive infinity or negative
    /// infinity if this value exceeds the range of a 32-bit floating point
    /// number.</returns>
    public float ToSingle() {
      int sb = this.ToSingleBits();
      return BitConverter.ToSingle(BitConverter.GetBytes(sb), 0);
    }

    /// <summary>Converts this number's value to a text string.</summary>
    /// <returns>A string representation of this object. The value is
    /// converted to a decimal number (using the EDecimal.FromEFloat
    /// method) and the decimal form of this number's value is returned.
    /// The text string will be in exponential notation (expressed as a
    /// number 1 or greater, but less than 10, times a power of 10) if the
    /// converted decimal number's exponent (EDecimal's Exponent property)
    /// is greater than 0 or if the number's first nonzero decimal digit is
    /// more than five digits after the decimal point.</returns>
    public override string ToString() {
      return EDecimal.FromEFloat(this).ToString();
    }

    /// <summary>Returns the unit in the last place. The significand will
    /// be 1 and the exponent will be this number's exponent. Returns 1
    /// with an exponent of 0 if this number is infinity or not-a-number
    /// (NaN).</summary>
    /// <returns>An arbitrary-precision binary floating-point
    /// number.</returns>
    public EFloat Ulp() {
      return (!this.IsFinite) ? EFloat.One :
        EFloat.Create(EInteger.One, this.Exponent);
    }

    internal static EFloat CreateWithFlags(
      long mantissa,
      EInteger exponent,
      int flags) {
      if (exponent == null) {
        throw new ArgumentNullException(nameof(exponent));
      }
      return new EFloat(
          FastIntegerFixed.FromInt64(mantissa).Abs(),
          FastIntegerFixed.FromBig(exponent),
          (byte)flags);
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
      return new EFloat(
          FastIntegerFixed.FromBig(mantissa).Abs(),
          FastIntegerFixed.FromBig(exponent),
          (byte)flags);
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
        bigmantissa = bigmantissa.ShiftLeft(curexp);
        if (neg) {
          bigmantissa = -bigmantissa;
        }
        return bigmantissa;
      } else {
        if (exact && !this.unsignedMantissa.IsEvenNumber) {
          // Mantissa is odd and will have to shift a nonzero
          // number of bits, so can't be an exact integer
          throw new ArithmeticException("Not an exact integer");
        }
        FastInteger bigexponent = FastInteger.FromBig(this.Exponent).Negate();
        EInteger bigmantissa = this.UnsignedMantissa;
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

    private static EInteger PowerOfRadixBitsLowerBound(EInteger e) {
      return e.Abs();
    }
    private static EInteger PowerOfRadixBitsUpperBound(EInteger e) {
      return e.Abs().Add(1);
    }

    /// <summary>Converts this value to an arbitrary-precision integer by
    /// discarding its fractional part and checking whether the resulting
    /// integer overflows the given signed bit count.</summary>
    /// <param name='maxBitLength'>The maximum number of signed bits the
    /// integer can have. The integer's value may not be less than
    /// -(2^maxBitLength) or greater than (2^maxBitLength) - 1.</param>
    /// <returns>An arbitrary-precision integer.</returns>
    /// <exception cref='OverflowException'>This object's value is infinity
    /// or not-a-number (NaN), or this number's value, once converted to an
    /// integer by discarding its fractional part, is less than
    /// -(2^maxBitLength) or greater than (2^maxBitLength) - 1.</exception>
    public EInteger ToSizedEInteger(int maxBitLength) {
      return this.ToSizedEInteger(maxBitLength, false);
    }

    /// <summary>Converts this value to an arbitrary-precision integer,
    /// only if this number's value is an exact integer and that integer
    /// does not overflow the given signed bit count.</summary>
    /// <param name='maxBitLength'>The maximum number of signed bits the
    /// integer can have. The integer's value may not be less than
    /// -(2^maxBitLength) or greater than (2^maxBitLength) - 1.</param>
    /// <returns>An arbitrary-precision integer.</returns>
    /// <exception cref='OverflowException'>This object's value is infinity
    /// or not-a-number (NaN), or this number's value, once converted to an
    /// integer by discarding its fractional part, is less than
    /// -(2^maxBitLength) or greater than (2^maxBitLength) - 1.</exception>
    /// <exception cref='ArithmeticException'>This object's value is not an
    /// exact integer.</exception>
    public EInteger ToSizedEIntegerIfExact(int maxBitLength) {
      return this.ToSizedEInteger(maxBitLength, true);
    }

    private EInteger ToSizedEInteger(int maxBitLength, bool exact) {
      if (maxBitLength < 0) {
        throw new ArgumentException("maxBitLength (" + maxBitLength +
          ") is not greater or equal to 0");
      }
      if (!this.IsFinite || this.IsZero) {
        return exact ? this.ToEIntegerIfExact() : this.ToEInteger();
      }
      EInteger mant = this.Mantissa;
      EInteger exp = this.Exponent;
      if (exp.Sign > 0) {
        // x * 2^y
        long imantbits = mant.GetSignedBitLengthAsInt64();
        if (imantbits >= maxBitLength) {
          throw new OverflowException("Value out of range");
        }
        if (exp.CompareTo(0x100000) < 0 && imantbits < 0x100000) {
          // Lower bound of bit count in 2^exp based on ln(2^exp)/ln(2)
          long expBitsLowerBound = exp.ToInt64Checked();
          if ((imantbits - 1) + expBitsLowerBound > maxBitLength) {
            throw new OverflowException("Value out of range");
          }
        } else if (exp.CompareTo(maxBitLength) > 0) {
          // Digits in exp is more than max bit length, so out of range
          throw new OverflowException("Value out of range");
        } else {
          EInteger mantbits = mant.GetSignedBitLengthAsEInteger();
          if (mantbits.Subtract(1).Add(PowerOfRadixBitsLowerBound(exp))
            .CompareTo(maxBitLength) > 0) {
            throw new OverflowException("Value out of range");
          }
        }
        mant = exact ? this.ToEIntegerIfExact() : this.ToEInteger();
      } else if (exp.Sign < 0) {
        // x * 2^-y. Check for trivial overflow cases before
        // running ToEInteger.
        exp = exp.Abs();
        long imantbits = mant.GetSignedBitLengthAsInt64();
        if (exp.CompareTo(0x100000) < 0 && imantbits < 0x100000) {
          long expBitsUpperBound = exp.ToInt64Checked() + 1;
          long expBitsLowerBound = exp.ToInt64Checked();
          if (imantbits - 1 - expBitsUpperBound > maxBitLength) {
            throw new OverflowException("Value out of range");
          }
          if (imantbits + 1 < expBitsLowerBound) {
            // Less than one, so not exact
            if (exact) {
              throw new ArithmeticException("Not an exact integer");
            } else {
              return EInteger.FromInt32(0);
            }
          }
        } else if (imantbits < 0x100000 && exp.CompareTo(0x200000) >= 0) {
          // (mant / 2^exp) would be less than one, so not exact
          if (exact) {
            throw new ArithmeticException("Not an exact integer");
          } else {
            return EInteger.FromInt32(0);
          }
        } else {
          EInteger mantbits = mant.GetSignedBitLengthAsEInteger();
          if (mantbits.Subtract(1).Subtract(PowerOfRadixBitsUpperBound(exp))
            .CompareTo(maxBitLength) > 0) {
            throw new OverflowException("Value out of range");
          }
        }
        mant = exact ? this.ToEIntegerIfExact() : this.ToEInteger();
      }
      if (mant.GetSignedBitLengthAsEInteger().CompareTo(maxBitLength) > 0) {
        throw new OverflowException("Value out of range");
      }
      return mant;
    }

    private sealed class BinaryMathHelper : IRadixMathHelper<EFloat> {
      /// <summary>This is an internal method.</summary>
      /// <returns>A 32-bit signed integer.</returns>
      public int GetRadix() {
        return 2;
      }

      /// <summary>This is an internal method.</summary>
      /// <param name='value'>An arbitrary-precision binary floating-point
      /// number.</param>
      /// <returns>A 32-bit signed integer.</returns>
      public int GetSign(EFloat value) {
        return value.Sign;
      }

      /// <summary>This is an internal method.</summary>
      /// <param name='value'>An arbitrary-precision binary floating-point
      /// number.</param>
      /// <returns>An arbitrary-precision integer.</returns>
      public EInteger GetMantissa(EFloat value) {
        return value.unsignedMantissa.ToEInteger();
      }

      /// <summary>This is an internal method.</summary>
      /// <param name='value'>An arbitrary-precision binary floating-point
      /// number.</param>
      /// <returns>An arbitrary-precision integer.</returns>
      public EInteger GetExponent(EFloat value) {
        return value.exponent.ToEInteger();
      }

      public FastInteger GetDigitLength(EInteger ei) {
        return FastInteger.FromBig(ei.GetUnsignedBitLengthAsEInteger());
      }

      public FastIntegerFixed GetMantissaFastInt(EFloat value) {
        return value.unsignedMantissa;
      }

      public FastIntegerFixed GetExponentFastInt(EFloat value) {
        return value.exponent;
      }

      /// <summary>This is an internal method.</summary>
      /// <param name='bigint'>An arbitrary-precision integer.</param>
      /// <param name='lastDigit'>The parameter <paramref name='lastDigit'/>
      /// is a 32-bit signed integer.</param>
      /// <param name='olderDigits'>The parameter <paramref
      /// name='olderDigits'/> is a 32-bit signed integer.</param>
      /// <returns>An IShiftAccumulator object.</returns>
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
              fastInt.ToInt32(),
              lastDigit,
              olderDigits);
        } else {
          return new BitShiftAccumulator(
              fastInt.ToEInteger(),
              lastDigit,
              olderDigits);
        }
      }

      /// <summary>This is an internal method.</summary>
      /// <param name='num'>An arbitrary-precision integer.</param>
      /// <param name='den'>Another arbitrary-precision integer.</param>
      /// <returns>A FastInteger object.</returns>
      public FastInteger DivisionShift(EInteger num, EInteger den) {
        if (den.IsZero) {
          return null;
        }
        if (den.GetUnsignedBit(0) && den.CompareTo(EInteger.One) != 0) {
          return null;
        }
        EInteger valueELowBit = den.GetLowBitAsEInteger();
        return
          den.GetUnsignedBitLengthAsEInteger().Equals(valueELowBit.Add(1)) ?
          FastInteger.FromBig(valueELowBit) : null;
      }

      /// <summary>This is an internal method.</summary>
      /// <param name='bigint'>Another arbitrary-precision integer.</param>
      /// <param name='power'>A fast integer.</param>
      /// <returns>An arbitrary-precision integer.</returns>
      public EInteger MultiplyByRadixPower(
        EInteger bigint,
        FastInteger power) {
        EInteger tmpbigint = bigint;
        if (power.Sign <= 0) {
          return tmpbigint;
        }
        if (tmpbigint.Sign < 0) {
          tmpbigint = -tmpbigint;
          tmpbigint = power.ShiftEIntegerLeftByThis(tmpbigint);
          tmpbigint = -tmpbigint;
          return tmpbigint;
        }
        return power.ShiftEIntegerLeftByThis(tmpbigint);
      }

      public FastIntegerFixed MultiplyByRadixPowerFastInt(
        FastIntegerFixed fbigint,
        FastIntegerFixed fpower) {
        if (fpower.Sign <= 0) {
          return fbigint;
        }
        EInteger ei = this.MultiplyByRadixPower(
            fbigint.ToEInteger(),
            FastInteger.FromBig(fpower.ToEInteger()));
        return FastIntegerFixed.FromBig(ei);
      }

      /// <summary>This is an internal method.</summary>
      /// <param name='value'>An arbitrary-precision binary floating-point
      /// number.</param>
      /// <returns>A 32-bit signed integer.</returns>
      public int GetFlags(EFloat value) {
        return value.flags;
      }

      /// <summary>This is an internal method.</summary>
      /// <param name='mantissa'>The parameter <paramref name='mantissa'/> is
      /// a Numbers.EInteger object.</param>
      /// <param name='exponent'>The parameter <paramref name='exponent'/> is
      /// an internal parameter.</param>
      /// <param name='flags'>The parameter <paramref name='flags'/> is an
      /// internal parameter.</param>
      /// <returns>An arbitrary-precision binary floating-point
      /// number.</returns>
      public EFloat CreateNewWithFlags(
        EInteger mantissa,
        EInteger exponent,
        int flags) {
        return new EFloat(FastIntegerFixed.FromBig(mantissa),
            FastIntegerFixed.FromBig(exponent),
            (byte)flags);
      }

      public EFloat CreateNewWithFlagsFastInt(
        FastIntegerFixed fmantissa,
        FastIntegerFixed fexponent,
        int flags) {
        return new EFloat(
            fmantissa,
            fexponent,
            (byte)flags);
      }

      /// <summary>This is an internal method.</summary>
      /// <returns>A 32-bit signed integer.</returns>
      public int GetArithmeticSupport() {
        return BigNumberFlags.FiniteAndNonFinite;
      }

      /// <summary>This is an internal method.</summary>
      /// <param name='val'>The parameter <paramref name='val'/> is a 32-bit
      /// signed integer.</param>
      /// <returns>An arbitrary-precision binary floating-point
      /// number.</returns>
      public EFloat ValueOf(int val) {
        return FromInt32(val);
      }
    }

    /// <summary>Returns one added to this arbitrary-precision binary
    /// floating-point number.</summary>
    /// <returns>The given arbitrary-precision binary floating-point number
    /// plus one.</returns>
    public EFloat Increment() {
      return this.Add(1);
    }

    /// <summary>Returns one subtracted from this arbitrary-precision
    /// binary floating-point number.</summary>
    /// <returns>The given arbitrary-precision binary floating-point number
    /// minus one.</returns>
    public EFloat Decrement() {
      return this.Subtract(1);
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
    public byte ToByteChecked() {
      if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      return this.IsZero ? ((byte)0) :
        this.ToEInteger().ToByteChecked();
    }

    /// <summary>Converts this number's value to an integer by discarding
    /// its fractional part, and returns the least-significant bits of its
    /// two's-complement form as a byte (from 0 to 255).</summary>
    /// <returns>This number, converted to a byte (from 0 to 255). Returns
    /// 0 if this value is infinity or not-a-number.</returns>
    public byte ToByteUnchecked() {
      return this.IsFinite ? this.ToEInteger().ToByteUnchecked() : (byte)0;
    }

    /// <summary>Converts this number's value to a byte (from 0 to 255) if
    /// it can fit in a byte (from 0 to 255) without rounding to a
    /// different numerical value.</summary>
    /// <returns>This number's value as a byte (from 0 to 255).</returns>
    /// <exception cref='ArithmeticException'>This value is infinity or
    /// not-a-number, is not an exact integer, or is less than 0 or greater
    /// than 255.</exception>
    public byte ToByteIfExact() {
      if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      return this.IsZero ? ((byte)0) : this.ToEIntegerIfExact().ToByteChecked();
    }

    /// <summary>Converts a byte (from 0 to 255) to an arbitrary-precision
    /// binary floating-point number.</summary>
    /// <param name='inputByte'>The number to convert as a byte (from 0 to
    /// 255).</param>
    /// <returns>This number's value as an arbitrary-precision binary
    /// floating-point number.</returns>
    public static EFloat FromByte(byte inputByte) {
      int val = ((int)inputByte) & 0xff;
      return FromInt32(val);
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
    public short ToInt16Checked() {
      if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      return this.IsZero ? ((short)0) :
        this.ToEInteger().ToInt16Checked();
    }

    /// <summary>Converts this number's value to an integer by discarding
    /// its fractional part, and returns the least-significant bits of its
    /// two's-complement form as a 16-bit signed integer.</summary>
    /// <returns>This number, converted to a 16-bit signed integer. Returns
    /// 0 if this value is infinity or not-a-number.</returns>
    public short ToInt16Unchecked() {
      return this.IsFinite ? this.ToEInteger().ToInt16Unchecked() : (short)0;
    }

    /// <summary>Converts this number's value to a 16-bit signed integer if
    /// it can fit in a 16-bit signed integer without rounding to a
    /// different numerical value.</summary>
    /// <returns>This number's value as a 16-bit signed integer.</returns>
    /// <exception cref='ArithmeticException'>This value is infinity or
    /// not-a-number, is not an exact integer, or is less than -32768 or
    /// greater than 32767.</exception>
    public short ToInt16IfExact() {
      if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      return this.IsZero ? ((short)0) :
        this.ToEIntegerIfExact().ToInt16Checked();
    }

    /// <summary>Converts a 16-bit signed integer to an arbitrary-precision
    /// binary floating-point number.</summary>
    /// <param name='inputInt16'>The number to convert as a 16-bit signed
    /// integer.</param>
    /// <returns>This number's value as an arbitrary-precision binary
    /// floating-point number.</returns>
    public static EFloat FromInt16(short inputInt16) {
      var val = (int)inputInt16;
      return FromInt32(val);
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
    public int ToInt32Checked() {
      if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      return this.IsZero ? ((int)0) :
        this.ToEInteger().ToInt32Checked();
    }

    /// <summary>Converts this number's value to an integer by discarding
    /// its fractional part, and returns the least-significant bits of its
    /// two's-complement form as a 32-bit signed integer.</summary>
    /// <returns>This number, converted to a 32-bit signed integer. Returns
    /// 0 if this value is infinity or not-a-number.</returns>
    public int ToInt32Unchecked() {
      return this.IsFinite ? this.ToEInteger().ToInt32Unchecked() : (int)0;
    }

    /// <summary>Converts this number's value to a 32-bit signed integer if
    /// it can fit in a 32-bit signed integer without rounding to a
    /// different numerical value.</summary>
    /// <returns>This number's value as a 32-bit signed integer.</returns>
    /// <exception cref='ArithmeticException'>This value is infinity or
    /// not-a-number, is not an exact integer, or is less than -2147483648
    /// or greater than 2147483647.</exception>
    public int ToInt32IfExact() {
      if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      return this.IsZero ? ((int)0) : this.ToEIntegerIfExact().ToInt32Checked();
    }

    /// <summary>Converts a boolean value (either true or false) to an
    /// arbitrary-precision binary floating-point number.</summary>
    /// <param name='boolValue'>Either true or false.</param>
    /// <returns>The number 1 if <paramref name='boolValue'/> is true,
    /// otherwise, 0.</returns>
    public static EFloat FromBoolean(bool boolValue) {
      return boolValue ? EFloat.One : EFloat.Zero;
    }

    /// <summary>Converts a 32-bit signed integer to an arbitrary-precision
    /// binary floating-point number.</summary>
    /// <param name='inputInt32'>The number to convert as a 32-bit signed
    /// integer.</param>
    /// <returns>This number's value as an arbitrary-precision binary
    /// floating-point number.</returns>
    public static EFloat FromInt32(int inputInt32) {
      if (inputInt32 >= CacheFirst && inputInt32 <= CacheLast) {
        return Cache[inputInt32 - CacheFirst];
      }
      if (inputInt32 == Int32.MinValue) {
        return FromEInteger(EInteger.FromInt32(inputInt32));
      }
      return new EFloat(
          FastIntegerFixed.FromInt32(Math.Abs(inputInt32)),
          FastIntegerFixed.Zero,
          (byte)((inputInt32 < 0) ? BigNumberFlags.FlagNegative : 0));
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
    public long ToInt64Checked() {
      if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      return this.IsZero ? 0L :
        this.ToEInteger().ToInt64Checked();
    }

    /// <summary>Converts this number's value to an integer by discarding
    /// its fractional part, and returns the least-significant bits of its
    /// two's-complement form as a 64-bit signed integer.</summary>
    /// <returns>This number, converted to a 64-bit signed integer. Returns
    /// 0 if this value is infinity or not-a-number.</returns>
    public long ToInt64Unchecked() {
      return this.IsFinite ? this.ToEInteger().ToInt64Unchecked() : 0L;
    }

    /// <summary>Converts this number's value to a 64-bit signed integer if
    /// it can fit in a 64-bit signed integer without rounding to a
    /// different numerical value.</summary>
    /// <returns>This number's value as a 64-bit signed integer.</returns>
    /// <exception cref='ArithmeticException'>This value is infinity or
    /// not-a-number, is not an exact integer, or is less than
    /// -9223372036854775808 or greater than
    /// 9223372036854775807.</exception>
    public long ToInt64IfExact() {
      if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      return this.IsZero ? 0L : this.ToEIntegerIfExact().ToInt64Checked();
    }

    /// <summary>Converts an unsigned integer expressed as a 64-bit signed
    /// integer to an arbitrary-precision binary number.</summary>
    /// <param name='longerValue'>A 64-bit signed integer. If this value is
    /// 0 or greater, the return value will represent it. If this value is
    /// less than 0, the return value will store 2^64 plus this value
    /// instead.</param>
    /// <returns>An arbitrary-precision binary number with the exponent set
    /// to 0. If <paramref name='longerValue'/> is 0 or greater, the return
    /// value will represent it. If <paramref name='longerValue'/> is less
    /// than 0, the return value will store 2^64 plus this value
    /// instead.</returns>
    public static EFloat FromInt64AsUnsigned(long longerValue) {
      return longerValue >= 0 ? FromInt64(longerValue) :
           FromEInteger(EInteger.FromInt64AsUnsigned(longerValue));
    }

    /// <summary>Converts a 64-bit signed integer to an arbitrary-precision
    /// binary floating-point number.</summary>
    /// <param name='inputInt64'>The number to convert as a 64-bit signed
    /// integer.</param>
    /// <returns>This number's value as an arbitrary-precision binary
    /// floating-point number with the exponent set to 0.</returns>
    public static EFloat FromInt64(long inputInt64) {
      if (inputInt64 >= CacheFirst && inputInt64 <= CacheLast) {
        return Cache[(int)inputInt64 - CacheFirst];
      }
      if (inputInt64 == Int64.MinValue) {
        return FromEInteger(EInteger.FromInt64(inputInt64));
      }
      return new EFloat(
          FastIntegerFixed.FromInt64(Math.Abs(inputInt64)),
          FastIntegerFixed.Zero,
          (byte)((inputInt64 < 0) ? BigNumberFlags.FlagNegative : 0));
    }

    // End integer conversions
  }
}
