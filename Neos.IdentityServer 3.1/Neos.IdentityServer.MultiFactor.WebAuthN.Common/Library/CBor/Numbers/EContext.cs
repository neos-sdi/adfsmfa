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
  /// <summary>Contains parameters for controlling the precision,
  /// rounding, and exponent range of arbitrary-precision numbers. (The
  /// "E" stands for "extended", and has this prefix to group it with the
  /// other classes common to this library, particularly EDecimal,
  /// EFloat, and ERational.).
  /// <para><b>Thread safety:</b> With one exception, instances of this
  /// class are immutable and are safe to use among multiple threads. The
  /// one exception involves the <c>Flags</c> property. If the context's
  /// <c>HasFlags</c> property (a read-only property) is <c>true</c>,
  /// the <c>Flags</c> property is mutable, thus making the context
  /// mutable. This class doesn't synchronize access to such mutable
  /// contexts, so applications should provide their own synchronization
  /// if a context with the <c>HasFlags</c> property set to <c>true</c>
  /// will be shared among multiple threads and at least one of those
  /// threads needs to write the <c>Flags</c> property (which can happen,
  /// for example, by passing the context to most methods of
  /// <c>EDecimal</c> such as <c>Add</c> ).</para></summary>
  public sealed class EContext {
    /// <summary>Signals that the exponent was adjusted to fit the exponent
    /// range.</summary>
    public const int FlagClamped = 32;

    /// <summary>Signals a division of a nonzero number by zero.</summary>
    public const int FlagDivideByZero = 128;

    /// <summary>Signals that the result was rounded to a different
    /// mathematical value, but as close as possible to the
    /// original.</summary>
    public const int FlagInexact = 1;

    /// <summary>Signals an invalid operation.</summary>
    public const int FlagInvalid = 64;

    /// <summary>Signals that an operand was rounded to a different
    /// mathematical value before an operation.</summary>
    public const int FlagLostDigits = 256;

    /// <summary>Signals that the result is non-zero and the exponent is
    /// higher than the highest exponent allowed.</summary>
    public const int FlagOverflow = 16;

    /// <summary>Signals that the result was rounded to fit the precision;
    /// either the value or the exponent may have changed from the
    /// original.</summary>
    public const int FlagRounded = 2;

    /// <summary>Signals that the result's exponent, before rounding, is
    /// lower than the lowest exponent allowed.</summary>
    public const int FlagSubnormal = 4;

    /// <summary>Signals that the result's exponent, before rounding, is
    /// lower than the lowest exponent allowed, and the result was rounded
    /// to a different mathematical value, but as close as possible to the
    /// original.</summary>
    public const int FlagUnderflow = 8;

    /// <summary>A basic arithmetic context, 9 digits precision, rounding
    /// mode half-up, unlimited exponent range. The default rounding mode
    /// is HalfUp.</summary>
    #if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "This PrecisionContext is immutable")]
    #endif
    public static readonly EContext Basic =
      EContext.ForPrecisionAndRounding(9, ERounding.HalfUp);

    /// <summary>An arithmetic context for Java's BigDecimal format. The
    /// default rounding mode is HalfUp.</summary>
    #if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "This PrecisionContext is immutable")]
    #endif
    public static readonly EContext BigDecimalJava =
      new EContext(0, ERounding.HalfUp, 0, 0, true)
    .WithExponentClamp(true).WithAdjustExponent(false)
    .WithBigExponentRange(
      EInteger.Zero - (EInteger)Int32.MaxValue,
      EInteger.One + (EInteger)Int32.MaxValue);

    /// <summary>An arithmetic context for the IEEE-754-2008 binary128
    /// format, 113 bits precision. The default rounding mode is
    /// HalfEven.</summary>
    #if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "This PrecisionContext is immutable")]
    #endif
    public static readonly EContext Binary128 =
      EContext.ForPrecisionAndRounding(113, ERounding.HalfEven)
      .WithExponentClamp(true).WithExponentRange(-16382, 16383);

    /// <summary>An arithmetic context for the IEEE-754-2008 binary16
    /// format, 11 bits precision. The default rounding mode is
    /// HalfEven.</summary>
    #if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "This PrecisionContext is immutable")]
    #endif
    public static readonly EContext Binary16 =
      EContext.ForPrecisionAndRounding(11, ERounding.HalfEven)
      .WithExponentClamp(true).WithExponentRange(-14, 15);

    /// <summary>An arithmetic context for the IEEE-754-2008 binary32
    /// format, 24 bits precision. The default rounding mode is
    /// HalfEven.</summary>
    #if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "This PrecisionContext is immutable")]
    #endif
    public static readonly EContext Binary32 =
      EContext.ForPrecisionAndRounding(24, ERounding.HalfEven)
      .WithExponentClamp(true).WithExponentRange(-126, 127);

    /// <summary>An arithmetic context for the IEEE-754-2008 binary64
    /// format, 53 bits precision. The default rounding mode is
    /// HalfEven.</summary>
    #if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "This PrecisionContext is immutable")]
    #endif
    public static readonly EContext Binary64 =
      EContext.ForPrecisionAndRounding(53, ERounding.HalfEven)
      .WithExponentClamp(true).WithExponentRange(-1022, 1023);

    /// <summary>An arithmetic context for the.NET Framework decimal format
    /// (see
    /// <see cref='PeterO.Numbers.EDecimal'>"Forms of numbers"</see> ), 96
    /// bits precision, and a valid exponent range of -28 to 0. The default
    /// rounding mode is HalfEven. (The <c>"Cli"</c> stands for "Common
    /// Language Infrastructure", which defined this format as the .NET
    /// Framework decimal format in version 1, but leaves it unspecified in
    /// later versions.).</summary>
    #if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "This PrecisionContext is immutable")]
    #endif
    public static readonly EContext CliDecimal =
      new EContext(96, ERounding.HalfEven, 0, 28, true)
    .WithPrecisionInBits(true);

    /// <summary>An arithmetic context for the IEEE-754-2008 decimal128
    /// format. The default rounding mode is HalfEven.</summary>
    #if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "This PrecisionContext is immutable")]
    #endif
    public static readonly EContext Decimal128 =
      new EContext(34, ERounding.HalfEven, -6143, 6144, true);

    /// <summary>An arithmetic context for the IEEE-754-2008 decimal32
    /// format. The default rounding mode is HalfEven.</summary>
    #if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "This PrecisionContext is immutable")]
    #endif
    public static readonly EContext Decimal32 =
      new EContext(7, ERounding.HalfEven, -95, 96, true);

    /// <summary>An arithmetic context for the IEEE-754-2008 decimal64
    /// format. The default rounding mode is HalfEven.</summary>
    #if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "This PrecisionContext is immutable")]
    #endif
    public static readonly EContext Decimal64 =
      new EContext(16, ERounding.HalfEven, -383, 384, true);

    /// <summary>No specific (theoretical) limit on precision. Rounding
    /// mode HalfUp.</summary>
    #if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "This PrecisionContext is immutable")]
    #endif
    public static readonly EContext Unlimited =
      EContext.ForPrecision(0);

    /// <summary>No specific (theoretical) limit on precision. Rounding
    /// mode HalfEven.</summary>
    #if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "This PrecisionContext is immutable")]
    #endif
    public static readonly EContext UnlimitedHalfEven =
      EContext.ForPrecision(0).WithRounding(ERounding.HalfEven);

    private EContext(
      bool adjustExponent,
      EInteger bigintPrecision,
      bool clampNormalExponents,
      EInteger exponentMax,
      EInteger exponentMin,
      int flags,
      bool hasExponentRange,
      bool hasFlags,
      bool precisionInBits,
      ERounding rounding,
      bool simplified,
      int traps) {
      if (bigintPrecision == null) {
        throw new ArgumentNullException(nameof(bigintPrecision));
      }
      if (exponentMin == null) {
        throw new ArgumentNullException(nameof(exponentMin));
      }
      if (exponentMax == null) {
        throw new ArgumentNullException(nameof(exponentMax));
      }
      if (bigintPrecision.Sign < 0) {
        throw new ArgumentException("precision(" + bigintPrecision +
          ") is less than 0");
      }
      if (exponentMin.CompareTo(exponentMax) > 0) {
        throw new ArgumentException("exponentMinSmall(" + exponentMin +
          ") is more than " + exponentMax);
      }
      this.adjustExponent = adjustExponent;
      this.bigintPrecision = bigintPrecision;
      this.clampNormalExponents = clampNormalExponents;
      this.exponentMax = exponentMax;
      this.exponentMin = exponentMin;
      this.flags = flags;
      this.hasExponentRange = hasExponentRange;
      this.hasFlags = hasFlags;
      this.precisionInBits = precisionInBits;
      this.rounding = rounding;
      this.simplified = simplified;
      this.traps = traps;
    }

    private readonly bool adjustExponent;

    private readonly EInteger bigintPrecision;

    private readonly bool clampNormalExponents;
    private readonly EInteger exponentMax;

    private readonly EInteger exponentMin;

    private readonly bool hasExponentRange;
    private readonly bool hasFlags;

    private readonly bool precisionInBits;

    private readonly ERounding rounding;

    private readonly bool simplified;

    private readonly int traps;

    private int flags;

    /// <summary>Initializes a new instance of the
    /// <see cref='PeterO.Numbers.EContext'/> class.</summary>
    /// <param name='precision'>The value of the Precision
    /// property.</param>
    /// <param name='rounding'>The value of the Rounding property.</param>
    /// <param name='exponentMinSmall'>The value of the EMin
    /// property.</param>
    /// <param name='exponentMaxSmall'>The value of the EMax
    /// property.</param>
    /// <param name='clampNormalExponents'>The value of the
    /// ClampNormalExponents property.</param>
    public EContext(
      int precision,
      ERounding rounding,
      int exponentMinSmall,
      int exponentMaxSmall,
      bool clampNormalExponents) : this(
          true,
          EInteger.FromInt32(precision),
          clampNormalExponents,
          EInteger.FromInt32(exponentMaxSmall),
          EInteger.FromInt32(exponentMinSmall),
          0,
          true,
          false,
          false,
          rounding,
          false,
          0) {
    }

    /// <summary>Initializes a new instance of the
    /// <see cref='PeterO.Numbers.EContext'/> class,.</summary>
    /// <param name='bigintPrecision'>The value of the Precision
    /// property.</param>
    /// <param name='rounding'>The value of the Rounding property.</param>
    /// <param name='exponentMin'>The value of the EMin property.</param>
    /// <param name='exponentMax'>The value of the EMax property.</param>
    /// <param name='clampNormalExponents'>The value of the
    /// ClampNormalExponents property.</param>
    public EContext(
      EInteger bigintPrecision,
      ERounding rounding,
      EInteger exponentMin,
      EInteger exponentMax,
      bool clampNormalExponents) : this(
          true,
          bigintPrecision,
          clampNormalExponents,
          exponentMax,
          exponentMin,
          0,
          true,
          false,
          false,
          rounding,
          false,
          0) {
    }

    /// <summary>Gets a value indicating whether the EMax and EMin
    /// properties refer to the number's Exponent property adjusted to the
    /// number's precision, or just the number's Exponent property. The
    /// default value is true, meaning that EMax and EMin refer to the
    /// adjusted exponent. Setting this value to false (using
    /// WithAdjustExponent) is useful for modeling floating point
    /// representations with an integer significand and an integer
    /// exponent, such as Java's BigDecimal.</summary>
    /// <value><c>true</c> if the EMax and EMin properties refer to the
    /// number's Exponent property adjusted to the number's precision, or
    /// false if they refer to just the number's Exponent property.</value>
    public bool AdjustExponent {
      get {
        return this.adjustExponent;
      }
    }

    /// <summary>Gets a value indicating whether a converted number's
    /// Exponent property will not be higher than EMax + 1 - Precision. If
    /// a number's exponent is higher than that value, but not high enough
    /// to cause overflow, the exponent is clamped to that value and enough
    /// zeros are added to the number's significand to account for the
    /// adjustment. If HasExponentRange is false, this value is always
    /// false.</summary>
    /// <value>If true, a converted number's Exponent property will not be
    /// higher than EMax + 1 - Precision.</value>
    public bool ClampNormalExponents {
      get {
        return this.hasExponentRange && this.clampNormalExponents;
      }
    }

    /// <summary>Gets the highest exponent possible when a converted number
    /// is expressed in scientific notation with one nonzero digit before
    /// the radix point. For example, with a precision of 3 and an EMax of
    /// 100, the maximum value possible is 9.99E + 100. (This is not the
    /// same as the highest possible Exponent property.) If
    /// HasExponentRange is false, this value will be 0.</summary>
    /// <value>The highest exponent possible when a converted number is
    /// expressed in scientific notation with one nonzero digit before the
    /// radix point. For example, with a precision of 3 and an EMax of 100,
    /// the maximum value possible is 9.99E + 100. (This is not the same as
    /// the highest possible Exponent property.) If HasExponentRange is
    /// false, this value will be 0.</value>
    public EInteger EMax {
      get {
        return this.hasExponentRange ? this.exponentMax : EInteger.Zero;
      }
    }

    /// <summary>Gets the lowest exponent possible when a converted number
    /// is expressed in scientific notation with one nonzero digit before
    /// the radix point. For example, with a precision of 3 and an EMin of
    /// -100, the next value that comes after 0 is 0.001E-100. (If
    /// AdjustExponent is false, this property specifies the lowest
    /// possible Exponent property instead.) If HasExponentRange is false,
    /// this value will be 0.</summary>
    /// <value>The lowest exponent possible when a converted number is
    /// expressed in scientific notation with one nonzero digit before the
    /// radix point. For example, with a precision of 3 and an EMin of
    /// -100, the next value that comes after 0 is 0.001E-100. (If
    /// AdjustExponent is false, this property specifies the lowest
    /// possible Exponent property instead.) If HasExponentRange is false,
    /// this value will be 0.</value>
    public EInteger EMin {
      get {
        return this.hasExponentRange ? this.exponentMin : EInteger.Zero;
      }
    }

    /// <summary>Gets or sets the flags that are set from converting
    /// numbers according to this arithmetic context. If <c>HasFlags</c> is
    /// false, this value will be 0. This value is a combination of bit
    /// fields. To retrieve a particular flag, use the AND operation on the
    /// return value of this method. For example: <c>(this.Flags
    /// &amp;EContext.FlagInexact) != 0</c> returns <c>true</c> if the
    /// Inexact flag is set.</summary>
    /// <value>The flags that are set from converting numbers according to
    /// this arithmetic context. If <c>HasFlags</c> is false, this value
    /// will be 0. This value is a combination of bit fields. To retrieve a
    /// particular flag, use the AND operation on the return value of this
    /// method. For example: <c>(this.Flags &amp;EContext.FlagInexact)
    /// !=0</c> returns <c>true</c> if the Inexact flag is set.</value>
    public int Flags {
      get {
        return this.flags;
      }

      set {
        if (!this.HasFlags) {
          throw new InvalidOperationException("Can't set flags");
        }
        this.flags = value;
      }
    }

    /// <summary>Gets a value indicating whether this context defines a
    /// minimum and maximum exponent. If false, converted exponents can
    /// have any exponent and operations can't cause overflow or
    /// underflow.</summary>
    /// <value><c>true</c> if this context defines a minimum and maximum
    /// exponent; otherwise, <c>false</c>.. If false, converted exponents
    /// can have any exponent and operations can't cause overflow or
    /// underflow. <c>true</c> if this context defines a minimum and
    /// maximum exponent; otherwise, <c>false</c>.</value>
    public bool HasExponentRange {
      get {
        return this.hasExponentRange;
      }
    }

    /// <summary>Gets a value indicating whether this context has a mutable
    /// Flags field.</summary>
    /// <value><c>true</c> if this context has a mutable Flags field;
    /// otherwise, <c>false</c>.</value>
    public bool HasFlags {
      get {
        return this.hasFlags;
      }
    }

    /// <summary>Gets a value indicating whether this context defines a
    /// maximum precision. This is the same as whether this context's
    /// Precision property is zero.</summary>
    /// <value><c>true</c> if this context defines a maximum precision;
    /// otherwise, <c>false</c>.</value>
    public bool HasMaxPrecision {
      get {
        return !this.bigintPrecision.IsZero;
      }
    }

    /// <summary>Gets a value indicating whether this context's Precision
    /// property is in bits, rather than digits. The default is
    /// false.</summary>
    /// <value><c>true</c> if this context's Precision property is in bits,
    /// rather than digits; otherwise, <c>false</c>.. The default is
    /// false. <c>true</c> if this context's Precision property is in bits,
    /// rather than digits; otherwise, <c>false</c>. The default is
    /// false.</value>
    public bool IsPrecisionInBits {
      get {
        return this.precisionInBits;
      }
    }

    /// <summary>Gets a value indicating whether to use a "simplified"
    /// arithmetic. In the simplified arithmetic, infinity, not-a-number,
    /// and subnormal numbers are not allowed, and negative zero is treated
    /// the same as positive zero. For further details, see
    /// <a
    ///   href='http://speleotrove.com/decimal/dax3274.html'><c>http://speleotrove.com/decimal/dax3274.html</c></a>
    /// .</summary>
    /// <value><c>true</c> if to use a "simplified" arithmetic; otherwise,
    /// <c>false</c> In the simplified arithmetic, infinity, not-a-number,
    /// and subnormal numbers are not allowed, and negative zero is treated
    /// the same as positive zero. For further details, see
    /// <a
    ///   href='http://speleotrove.com/decimal/dax3274.html'><c>http://speleotrove.com/decimal/dax3274.html</c></a>
    /// . <c>true</c> if a "simplified" arithmetic will be used; otherwise,
    /// <c>false</c>.</value>
    public bool IsSimplified {
      get {
        return this.simplified;
      }
    }

    /// <summary>Gets the maximum length of a converted number in digits,
    /// ignoring the radix point and exponent. For example, if precision is
    /// 3, a converted number's significand can range from 0 to 999 (up to
    /// three digits long). If 0, converted numbers can have any precision.
    /// <para>Not-a-number (NaN) values can carry an optional number, its
    /// payload, that serves as its "diagnostic information", In general,
    /// if an operation requires copying an NaN's payload, only up to as
    /// many digits of that payload as the precision given in this context,
    /// namely the least significant digits, are copied.</para></summary>
    /// <value>The maximum length of a converted number in digits, ignoring
    /// the radix point and exponent. For example, if precision is 3, a
    /// converted number's significand can range from 0 to 999 (up to three
    /// digits long). If 0, converted numbers can have any
    /// precision.</value>
    public EInteger Precision {
      get {
        return this.bigintPrecision;
      }
    }

    /// <summary>Gets the desired rounding mode when converting numbers
    /// that can't be represented in the given precision and exponent
    /// range.</summary>
    /// <value>The desired rounding mode when converting numbers that can't
    /// be represented in the given precision and exponent range.</value>
    public ERounding Rounding {
      get {
        return this.rounding;
      }
    }

    /// <summary>Gets the traps that are set for each flag in the context.
    /// Whenever a flag is signaled, even if <c>HasFlags</c> is false, and
    /// the flag's trap is enabled, the operation will throw a
    /// TrapException.
    /// <para>For example, if Traps equals <c>FlagInexact</c> and
    /// FlagSubnormal, a TrapException will be thrown if an operation's
    /// return value is not the same as the exact result (FlagInexact) or
    /// if the return value's exponent is lower than the lowest allowed
    /// (FlagSubnormal).</para></summary>
    /// <value>The traps that are set for each flag in the context.
    /// Whenever a flag is signaled, even if <c>HasFlags</c> is false, and
    /// the flag's trap is enabled, the operation will throw a
    /// TrapException.
    /// <para>For example, if Traps equals <c>FlagInexact</c> and
    /// FlagSubnormal, a TrapException will be thrown if an operation's
    /// return value is not the same as the exact result (FlagInexact) or
    /// if the return value's exponent is lower than the lowest allowed
    /// (FlagSubnormal).</para>.</value>
    public int Traps {
      get {
        return this.traps;
      }
    }

    /// <summary>Creates a new arithmetic context using the given maximum
    /// number of digits, an unlimited exponent range, and the HalfUp
    /// rounding mode.</summary>
    /// <param name='precision'>Maximum number of digits
    /// (precision).</param>
    /// <returns>A context object for arbitrary-precision arithmetic
    /// settings.</returns>
    public static EContext ForPrecision(int precision) {
      return new EContext(
        precision,
        ERounding.HalfUp,
        0,
        0,
        false).WithUnlimitedExponents();
    }

    /// <summary>Creates a new EContext object initialized with an
    /// unlimited exponent range, and the given rounding mode and maximum
    /// precision.</summary>
    /// <param name='precision'>Maximum number of digits
    /// (precision).</param>
    /// <param name='rounding'>The parameter <paramref name='rounding'/> is
    /// an ERounding object.</param>
    /// <returns>A context object for arbitrary-precision arithmetic
    /// settings.</returns>
    public static EContext ForPrecisionAndRounding(
      int precision,
      ERounding rounding) {
      return new EContext(
        precision,
        rounding,
        0,
        0,
        false).WithUnlimitedExponents();
    }

    private static readonly EContext ForRoundingHalfEven = new EContext(
      0,
      ERounding.HalfEven,
      0,
      0,
      false).WithUnlimitedExponents();

    private static readonly EContext ForRoundingDown = new EContext(
      0,
      ERounding.Down,
      0,
      0,
      false).WithUnlimitedExponents();

    /// <summary>Creates a new EContext object initialized with an
    /// unlimited precision, an unlimited exponent range, and the given
    /// rounding mode.</summary>
    /// <param name='rounding'>The rounding mode for the new precision
    /// context.</param>
    /// <returns>A context object for arbitrary-precision arithmetic
    /// settings.</returns>
    public static EContext ForRounding(ERounding rounding) {
      if (rounding == ERounding.HalfEven) {
        return ForRoundingHalfEven;
      }
      if (rounding == ERounding.Down) {
        return ForRoundingDown;
      }
      return new EContext(
        0,
        rounding,
        0,
        0,
        false).WithUnlimitedExponents();
    }

    /// <summary>Initializes a new EContext that is a copy of another
    /// EContext.</summary>
    /// <returns>A context object for arbitrary-precision arithmetic
    /// settings.</returns>
    public EContext Copy() {
      return new EContext(
        this.adjustExponent,
        this.bigintPrecision,
        this.clampNormalExponents,
        this.exponentMax,
        this.exponentMin,
        this.flags,
        this.hasExponentRange,
        this.hasFlags,
        this.precisionInBits,
        this.rounding,
        this.simplified,
        this.traps);
    }

    /// <summary>Determines whether a number can have the given Exponent
    /// property under this arithmetic context.</summary>
    /// <param name='exponent'>An arbitrary-precision integer indicating
    /// the desired exponent.</param>
    /// <returns><c>true</c> if a number can have the given Exponent
    /// property under this arithmetic context; otherwise, <c>false</c>.
    /// If this context allows unlimited precision, returns true for the
    /// exponent EMax and any exponent less than EMax.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='exponent'/> is null.</exception>
    public bool ExponentWithinRange(EInteger exponent) {
      if (exponent == null) {
        throw new ArgumentNullException(nameof(exponent));
      }
      if (!this.HasExponentRange) {
        return true;
      }
      if (this.bigintPrecision.IsZero) {
        // Only check EMax, since with an unlimited
        // precision, any exponent less than EMin will exceed EMin if
        // the significand is the right size
        // TODO: In next major version, perhaps correct this to check
        // EMin here as well if AdjustExponent is true
        return exponent.CompareTo(this.EMax) <= 0;
      } else {
        EInteger bigint = exponent;
        if (this.adjustExponent) {
          bigint = bigint.Add(this.bigintPrecision).Subtract(1);
        }
        return (bigint.CompareTo(this.EMin) >= 0) &&
(exponent.CompareTo(this.EMax) <= 0);
      }
    }

    /// <summary>Returns this object in a text form intended to be read by
    /// humans. The value returned by this method is not intended to be
    /// parsed by computer programs, and the exact text of the value may
    /// change at any time between versions of this library.</summary>
    /// <returns>A string representation of this object.</returns>
    public override string ToString() {
      return "[PrecisionContext ExponentMax=" + this.exponentMax +
        ", Traps=" + this.traps + ", ExponentMin=" + this.exponentMin +
        ", HasExponentRange=" + this.hasExponentRange + ", BigintPrecision=" +
        this.bigintPrecision + ", Rounding=" + this.rounding +
        ", ClampNormalExponents=" + this.clampNormalExponents +
        ", AdjustExponent=" + this.adjustExponent + ", Flags=" +
        this.flags + ", HasFlags=" + this.hasFlags + ", IsSimplified=" +
this.simplified +
        "]";
    }

    /// <summary>Gets a value indicating whether this context has a mutable
    /// Flags field, one or more trap enablers, or both.</summary>
    /// <value><c>true</c> if this context has a mutable Flags field, one
    /// or more trap enablers, or both; otherwise, <c>false</c>.</value>
    public bool HasFlagsOrTraps {
      get {
        return this.HasFlags || this.Traps != 0;
      }
    }

    /// <summary>Copies this EContext and sets the copy's "AdjustExponent"
    /// property to the given value.</summary>
    /// <param name='adjustExponent'>The new value of the "AdjustExponent"
    /// property for the copy.</param>
    /// <returns>A context object for arbitrary-precision arithmetic
    /// settings.</returns>
    public EContext WithAdjustExponent(bool adjustExponent) {
      return new EContext(
        adjustExponent,
        this.bigintPrecision,
        this.clampNormalExponents,
        this.exponentMax,
        this.exponentMin,
        this.flags,
        this.hasExponentRange,
        this.hasFlags,
        this.precisionInBits,
        this.rounding,
        this.simplified,
        this.traps);
    }

    /// <summary>Copies this arithmetic context and sets the copy's
    /// exponent range.</summary>
    /// <param name='exponentMin'>Desired minimum exponent (EMin).</param>
    /// <param name='exponentMax'>Desired maximum exponent (EMax).</param>
    /// <returns>A context object for arbitrary-precision arithmetic
    /// settings.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='exponentMin'/> is null.</exception>
    /// <exception cref='ArgumentException'>ExponentMin greater than
    /// exponentMax".</exception>
    public EContext WithBigExponentRange(
      EInteger exponentMin,
      EInteger exponentMax) {
      return new EContext(
        this.adjustExponent,
        this.bigintPrecision,
        this.clampNormalExponents,
        exponentMax,
        exponentMin,
        this.flags,
        true,
        this.hasFlags,
        this.precisionInBits,
        this.rounding,
        this.simplified,
        this.traps);
    }

    /// <summary>Copies this EContext with <c>HasFlags</c> set to false, a
    /// Traps value of 0, and a Flags value of 0.</summary>
    /// <returns>A context object for arbitrary-precision arithmetic
    /// settings.</returns>
    public EContext WithNoFlagsOrTraps() {
      return new EContext(
        this.adjustExponent,
        this.bigintPrecision,
        this.clampNormalExponents,
        this.exponentMax,
        this.exponentMin,
        0,
        this.hasExponentRange,
        false,
        this.precisionInBits,
        this.rounding,
        this.simplified,
        0);
    }

    /// <summary>Copies this EContext and gives it a particular precision
    /// value.</summary>
    /// <param name='bigintPrecision'>Desired precision. 0 means unlimited
    /// precision.</param>
    /// <returns>A context object for arbitrary-precision arithmetic
    /// settings.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bigintPrecision'/> is null.</exception>
    public EContext WithBigPrecision(EInteger bigintPrecision) {
      return new EContext(
        this.adjustExponent,
        bigintPrecision,
        this.clampNormalExponents,
        this.exponentMax,
        this.exponentMin,
        this.flags,
        this.hasExponentRange,
        this.hasFlags,
        this.precisionInBits,
        this.rounding,
        this.simplified,
        this.traps);
    }

    /// <summary>Copies this EContext with <c>HasFlags</c> set to true and
    /// a Flags value of 0.</summary>
    /// <returns>A context object for arbitrary-precision arithmetic
    /// settings.</returns>
    public EContext WithBlankFlags() {
      return new EContext(
        this.adjustExponent,
        this.bigintPrecision,
        this.clampNormalExponents,
        this.exponentMax,
        this.exponentMin,
        0,
        this.hasExponentRange,
        true,
        this.precisionInBits,
        this.rounding,
        this.simplified,
        this.traps);
    }

    /// <summary>Copies this arithmetic context and sets the copy's
    /// "ClampNormalExponents" flag to the given value.</summary>
    /// <param name='clamp'>The desired value of the "ClampNormalExponents"
    /// flag.</param>
    /// <returns>A context object for arbitrary-precision arithmetic
    /// settings.</returns>
    public EContext WithExponentClamp(bool clamp) {
      return new EContext(
        this.adjustExponent,
        this.bigintPrecision,
        clamp,
        this.exponentMax,
        this.exponentMin,
        this.flags,
        this.hasExponentRange,
        this.hasFlags,
        this.precisionInBits,
        this.rounding,
        this.simplified,
        this.traps);
    }

    /// <summary>Copies this arithmetic context and sets the copy's
    /// exponent range.</summary>
    /// <param name='exponentMinSmall'>Desired minimum exponent
    /// (EMin).</param>
    /// <param name='exponentMaxSmall'>Desired maximum exponent
    /// (EMax).</param>
    /// <returns>A context object for arbitrary-precision arithmetic
    /// settings.</returns>
    public EContext WithExponentRange(
      int exponentMinSmall,
      int exponentMaxSmall) {
      return this.WithBigExponentRange(
          EInteger.FromInt32(exponentMinSmall),
          EInteger.FromInt32(exponentMaxSmall));
    }

    /// <summary>Copies this EContext with <c>HasFlags</c> set to false and
    /// a Flags value of 0.</summary>
    /// <returns>A context object for arbitrary-precision arithmetic
    /// settings.</returns>
    public EContext WithNoFlags() {
      return new EContext(
        this.adjustExponent,
        this.bigintPrecision,
        this.clampNormalExponents,
        this.exponentMax,
        this.exponentMin,
        0,
        this.hasExponentRange,
        false,
        this.precisionInBits,
        this.rounding,
        this.simplified,
        this.traps);
    }

    /// <summary>Copies this EContext and gives it a particular precision
    /// value.</summary>
    /// <param name='precision'>Desired precision. 0 means unlimited
    /// precision.</param>
    /// <returns>A context object for arbitrary-precision arithmetic
    /// settings.</returns>
    public EContext WithPrecision(int precision) {
      return this.WithBigPrecision(EInteger.FromInt32(precision));
    }

    /// <summary>Copies this EContext and sets the copy's
    /// "IsPrecisionInBits" property to the given value.</summary>
    /// <param name='isPrecisionBits'>The new value of the
    /// "IsPrecisionInBits" property for the copy.</param>
    /// <returns>A context object for arbitrary-precision arithmetic
    /// settings.</returns>
    public EContext WithPrecisionInBits(bool isPrecisionBits) {
      return new EContext(
        this.adjustExponent,
        this.bigintPrecision,
        this.clampNormalExponents,
        this.exponentMax,
        this.exponentMin,
        this.flags,
        this.hasExponentRange,
        this.hasFlags,
        isPrecisionBits,
        this.rounding,
        this.simplified,
        this.traps);
    }

    /// <summary>Copies this EContext with the specified rounding
    /// mode.</summary>
    /// <param name='rounding'>Desired value of the Rounding
    /// property.</param>
    /// <returns>A context object for arbitrary-precision arithmetic
    /// settings.</returns>
    public EContext WithRounding(ERounding rounding) {
      return new EContext(
        this.adjustExponent,
        this.bigintPrecision,
        this.clampNormalExponents,
        this.exponentMax,
        this.exponentMin,
        this.flags,
        this.hasExponentRange,
        this.hasFlags,
        this.precisionInBits,
        rounding,
        this.simplified,
        this.traps);
    }

    /// <summary>Copies this EContext and sets the copy's "IsSimplified"
    /// property to the given value.</summary>
    /// <param name='simplified'>Desired value of the IsSimplified
    /// property.</param>
    /// <returns>A context object for arbitrary-precision arithmetic
    /// settings.</returns>
    public EContext WithSimplified(bool simplified) {
      return new EContext(
        this.adjustExponent,
        this.bigintPrecision,
        this.clampNormalExponents,
        this.exponentMax,
        this.exponentMin,
        this.flags,
        this.hasExponentRange,
        this.hasFlags,
        this.precisionInBits,
        this.rounding,
        simplified,
        this.traps);
    }

    /// <summary>Copies this EContext with Traps set to the given value.
    /// (Also sets HasFlags on the copy to <c>True</c>, but this may
    /// change in version 2.0 of this library.).</summary>
    /// <param name='traps'>Flags representing the traps to enable. See the
    /// property "Traps".</param>
    /// <returns>A context object for arbitrary-precision arithmetic
    /// settings.</returns>
    public EContext WithTraps(int traps) {
      // NOTE: Apparently HasFlags must be set to true because
      // some parts of code may treat HasFlags as HasFlagsOrTraps
      return new EContext(
        this.adjustExponent,
        this.bigintPrecision,
        this.clampNormalExponents,
        this.exponentMax,
        this.exponentMin,
        this.flags,
        this.hasExponentRange,
        true,
        this.precisionInBits,
        this.rounding,
        this.simplified,
        traps);
    }

    /// <summary>Copies this EContext with an unlimited exponent
    /// range.</summary>
    /// <returns>A context object for arbitrary-precision arithmetic
    /// settings.</returns>
    public EContext WithUnlimitedExponents() {
      return new EContext(
        this.adjustExponent,
        this.bigintPrecision,
        this.clampNormalExponents,
        this.exponentMax,
        this.exponentMin,
        this.flags,
        false,
        this.hasFlags,
        this.precisionInBits,
        this.rounding,
        this.simplified,
        this.traps);
    }

  /// <summary>Returns this context if it doesn't set traps, or a context
  /// without traps and with blank flags if it does, so that the
  /// resulting context does not cause trap exceptions to occur. This is
  /// not a general-purpose method; it is intended to support custom
  /// implementations of arithmetic operations.</summary>
  /// <returns>This context if it doesn't set traps, or a context without
  /// traps and with blank flags if it does.</returns>
    public EContext GetNontrapping() {
       return (this.Traps == 0) ? this : this.WithTraps(0).WithBlankFlags();
    }

  /// <summary>Throws trap exceptions if the given context has flags set
  /// that also have traps enabled for them in this context, and adds the
  /// given context's flags to this context if HasFlags for this context
  /// is true. This is not a general-purpose method; it is intended to
  /// support custom implementations of arithmetic operations.</summary>
  /// <param name='result'>The result of the operation.</param>
  /// <param name='trappableContext'>An arithmetic context, usually a
  /// context returned by the GetNontrapping method. Can be null.</param>
  /// <typeparam name='T'>Data type for the result of the
  /// operation.</typeparam>
  /// <returns>The parameter <paramref name='result'/> if no trap
  /// exceptions were thrown.</returns>
    public T TriggerTraps<T>(
      T result,
      EContext trappableContext) {
      if (trappableContext == null || trappableContext.Flags == 0) {
        return result;
      }
      if (this.HasFlags) {
        this.flags |= trappableContext.Flags;
      }
      int traps = this.Traps & trappableContext.Flags;
      if (traps == 0) {
        return result;
      }
      int mutexConditions = traps & (~(
            EContext.FlagClamped | EContext.FlagInexact |
            EContext.FlagRounded | EContext.FlagSubnormal));
      if (mutexConditions != 0) {
        for (var i = 0; i < 32; ++i) {
          int flag = mutexConditions & (1 << i);
          if (flag != 0) {
            throw new ETrapException(traps, flag, this, result);
          }
        }
      }
      if ((traps & EContext.FlagSubnormal) != 0) {
        throw new ETrapException(
          traps,
          traps & EContext.FlagSubnormal,
          this,
          result);
      }
      if ((traps & EContext.FlagInexact) != 0) {
        throw new ETrapException(
          traps,
          traps & EContext.FlagInexact,
          this,
          result);
      }
      if ((traps & EContext.FlagRounded) != 0) {
        throw new ETrapException(
          traps,
          traps & EContext.FlagRounded,
          this,
          result);
      }
      if ((traps & EContext.FlagClamped) != 0) {
        throw new ETrapException(
          traps,
          traps & EContext.FlagClamped,
          this,
          result);
      }
      return result;
    }
  }
}
