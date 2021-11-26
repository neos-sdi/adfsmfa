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
/*
TODO: In next major version, maybe convert EDecimal.One/Ten/Zero to fields
rather than properties
*/
namespace Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor.Numbers
{
  /// <summary>
  ///  Represents an arbitrary-precision decimal
  /// floating-point number. (The "E" stands for "extended",
  /// meaning that instances of this class can be values
  /// other than numbers proper, such as infinity and
  /// not-a-number.)
  /// <para><b>About decimal arithmetic</b>
  /// </para>
  /// <para>Decimal (base-10) arithmetic, such as that provided by this
  /// class, is appropriate for calculations involving such real-world
  /// data as prices and other sums of money, tax rates, and
  /// measurements. These calculations often involve multiplying or
  /// dividing one decimal with another decimal, or performing other
  /// operations on decimal numbers. Many of these calculations also rely
  /// on rounding behavior in which the result after rounding is an
  /// arbitrary-precision decimal number (for example, multiplying a
  /// price by a premium rate, then rounding, should result in a decimal
  /// amount of money).</para>
  /// <para>On the other hand, most implementations of <c>float</c>
  ///  and
  /// <c>double</c>
  ///  , including in C# and Java, store numbers in a binary
  /// (base-2) floating-point format and use binary floating-point
  /// arithmetic. Many decimal numbers can't be represented exactly in
  /// binary floating-point format (regardless of its length). Applying
  /// binary arithmetic to numbers intended to be decimals can sometimes
  /// lead to unintuitive results, as is shown in the description for the
  /// FromDouble() method of this class.</para>
  /// <para><b>About EDecimal instances</b>
  /// </para>
  /// <para>Each instance of this class consists of an integer
  /// significand and an integer exponent, both arbitrary-precision. The
  /// value of the number equals significand * 10^exponent.</para>
  /// <para>The significand is the value of the digits that make up a
  /// number, ignoring the decimal point and exponent. For example, in
  /// the number 2356.78, the significand is 235678. The exponent is
  /// where the "floating" decimal point of the number is located. A
  /// positive exponent means "move it to the right", and a negative
  /// exponent means "move it to the left." In the example 2, 356.78, the
  /// exponent is -2, since it has 2 decimal places and the decimal point
  /// is "moved to the left by 2." Therefore, in the arbitrary-precision
  /// decimal representation, this number would be stored as 235678 *
  /// 10^-2.</para>
  /// <para>The significand and exponent format preserves trailing zeros
  /// in the number's value. This may give rise to multiple ways to store
  /// the same value. For example, 1.00 and 1 would be stored
  /// differently, even though they have the same value. In the first
  /// case, 100 * 10^-2 (100 with decimal point moved left by 2), and in
  /// the second case, 1 * 10^0 (1 with decimal point moved 0).</para>
  /// <para>This class also supports values for negative zero,
  /// not-a-number (NaN) values, and infinity. <b>Negative zero</b>
  ///  is
  /// generally used when a negative number is rounded to 0; it has the
  /// same mathematical value as positive zero. <b>Infinity</b>
  ///  is
  /// generally used when a non-zero number is divided by zero, or when a
  /// very high or very low number can't be represented in a given
  /// exponent range. <b>Not-a-number</b>
  ///  is generally used to signal
  /// errors.</para>
  /// <para>This class implements the General Decimal Arithmetic
  /// Specification version 1.70 except part of chapter 6(
  /// <c>http://speleotrove.com/decimal/decarith.html</c>
  ///  ).</para>
  /// <para><b>Errors and Exceptions</b>
  /// </para>
  /// <para>Passing a signaling NaN to any arithmetic operation shown
  /// here will signal the flag FlagInvalid and return a quiet NaN, even
  /// if another operand to that operation is a quiet NaN, unless the
  /// operation's documentation expressly states that another result
  /// happens when a signaling NaN is passed to that operation.</para>
  /// <para>Passing a quiet NaN to any arithmetic operation shown here
  /// will return a quiet NaN, unless the operation's documentation
  /// expressly states that another result happens when a quiet NaN is
  /// passed to that operation. Invalid operations will also return a
  /// quiet NaN, as stated in the individual methods.</para>
  /// <para>Unless noted otherwise, passing a null arbitrary-precision
  /// decimal argument to any method here will throw an exception.</para>
  /// <para>When an arithmetic operation signals the flag FlagInvalid,
  /// FlagOverflow, or FlagDivideByZero, it will not throw an exception
  /// too, unless the flag's trap is enabled in the arithmetic context
  /// (see EContext's Traps property).</para>
  /// <para>If an operation requires creating an intermediate value that
  /// might be too big to fit in memory (or might require more than 2
  /// gigabytes of memory to store -- due to the current use of a 32-bit
  /// integer internally as a length), the operation may signal an
  /// invalid-operation flag and return not-a-number (NaN). In certain
  /// rare cases, the CompareTo method may throw OutOfMemoryException
  /// (called OutOfMemoryError in Java) in the same circumstances.</para>
  /// <para><b>Serialization</b>
  /// </para>
  /// <para>An arbitrary-precision decimal value can be serialized
  /// (converted to a stable format) in one of the following ways:</para>
  /// <list><item>By calling the toString() method, which will always
  /// return distinct strings for distinct arbitrary-precision decimal
  /// values.</item>
  ///  <item>By calling the UnsignedMantissa, Exponent, and
  /// IsNegative properties, and calling the IsInfinity, IsQuietNaN, and
  /// IsSignalingNaN methods. The return values combined will uniquely
  /// identify a particular arbitrary-precision decimal value.</item>
  /// </list>
  /// <para><b>Thread safety</b>
  /// </para>
  /// <para>Instances of this class are immutable, so they are inherently
  /// safe for use by multiple threads. Multiple instances of this object
  /// with the same properties are interchangeable, so they should not be
  /// compared using the "==" operator (which might only check if each
  /// side of the operator is the same instance).</para>
  /// <para><b>Comparison considerations</b>
  /// </para>
  /// <para>This class's natural ordering (under the CompareTo method) is
  /// not consistent with the Equals method. This means that two values
  /// that compare as equal under the CompareTo method might not be equal
  /// under the Equals method. The CompareTo method compares the
  /// mathematical values of the two instances passed to it (and
  /// considers two different NaN values as equal), while two instances
  /// with the same mathematical value, but different exponents, will be
  /// considered unequal under the Equals method.</para>
  /// <para><b>Security note</b>
  /// </para>
  /// <para>It is not recommended to implement security-sensitive
  /// algorithms using the methods in this class, for several
  /// reasons:</para>
  /// <list><item><c>EDecimal</c>
  ///  objects are immutable, so they can't be
  /// modified, and the memory they occupy is not guaranteed to be
  /// cleared in a timely fashion due to garbage collection. This is
  /// relevant for applications that use many-digit-long numbers as
  /// secret parameters.</item>
  ///  <item>The methods in this class
  /// (especially those that involve arithmetic) are not guaranteed to be
  /// "constant-time" (non-data-dependent) for all relevant inputs.
  /// Certain attacks that involve encrypted communications have
  /// exploited the timing and other aspects of such communications to
  /// derive keying material or cleartext indirectly.</item>
  ///  </list>
  /// <para>Applications should instead use dedicated security libraries
  /// to handle big numbers in security-sensitive algorithms.</para>
  /// <para><b>Reproducibility note</b>
  /// </para>
  /// <para>Some applications, such as simulations, care about results
  /// that are reproducible, bit for bit, across computers and across
  /// runs of the application. Bruce Dawson, in "Floating-Point
  /// Determinism" ( <c>https://randomascii.wordpress.com/</c>
  /// <c>2013/07/16/floating-point-determinism/</c>
  ///  ), identified many
  /// reproducibility issues with floating-point numbers, and here is how
  /// they relate to the EDecimal and EFloat classes of this
  /// library:</para>
  /// <list><item>Runtime floating-point settings: All the settings that
  /// change how EDecimal and EFloat behave are given as parameters to
  /// the appropriate methods, especially via EContext objects, which
  /// specify the precision, rounding, and exponent range of numbers,
  /// among other things. The EDecimal and EFloat classes avoid the use
  /// of "native" floating-point data types (except for methods that
  /// convert to or from <c>float</c>
  ///  , <c>double</c>
  ///  , or
  /// <c>System.Decimal</c>
  ///  ). Such "native" types are often subject to
  /// runtime settings that change how floating-point math behaves with
  /// them, and these settings are often not accessible to .NET or Java
  /// code.</item>
  ///  <item>Non-associativity and intermediate precisions:
  /// In general, EDecimal and EFloat use "unlimited" precision in their
  /// calculations unless specified otherwise by an EContext object.
  /// However, by limiting the precision of EDecimal, EFloat, and other
  /// floating-point numbers in this way, operations such as addition and
  /// multiplication on three or more numbers can be
  /// <i>non-associative</i>
  ///  , meaning the result can change depending on
  /// the order in which those numbers are added or multiplied. This
  /// property means that if an algorithm does not ensure such numbers
  /// are added or multiplied in the same order every time, its results
  /// may not be reproducible across computers or across runs of the
  /// application. This non-associativity problem can happen, for
  /// example, if an application splits a calculation across several
  /// threads and combines their results in the end. The problems with an
  /// unspecified order of operations (in the same line of code) and
  /// intermediate precisions (problems present in C and C++, for
  /// example) don't exist with method calls to EDecimal and EFloat
  /// methods, especially since they require limited-precision support to
  /// be declared explicitly via EContext.</item>
  ///  <item>fmadd
  /// instruction: EDecimal and EFloat include a MultiplyAndAdd method
  /// with the same semantics as in the General Decimal Arithmetic
  /// Specification, which requires delivering correctly rounded results
  /// for this method.</item>
  ///  <item>Square root estimate: Not applicable
  /// since EDecimal and EFloat don't include any estimates to square
  /// root.</item>
  ///  <item>Transcendental functions: This includes
  /// logarithms, exponentials, and the Pi method. For these functions,
  /// results are not guaranteed to always be correctly rounded. When
  /// using transcendentals, an application that cares about
  /// reproducibility should choose one version of this library and stick
  /// to it; this at least has the advantage that the implementation will
  /// be the same across computers, unlike with "native" floating-point
  /// types where the choice of implementation is often not within the
  /// application's control.</item>
  ///  <item>Conversions: Conversions
  /// between EDecimal or EFloat and text strings have the same
  /// implementation across computers for the same version of this
  /// library (see also the advice for transcendentals above). But as for
  /// the ToDouble, ToSingle, FromDouble, and FromSingle methods, note
  /// that some implementations of Java and.NET may or may not support
  /// preserving the value of subnormal numbers (numbers other than zero
  /// with the lowest possible exponent) or the payloads held in a
  /// not-a-number (NaN) value of float or double; thus these methods
  /// should not be considered reproducible across computers.</item>
  /// <item>Compiler differences: Not applicable where these classes
  /// don't use "native" floating-point types.</item>
  ///  <item>Uninitialized
  /// data; per-processor code: Not applicable.</item>
  ///  </list>
  /// <para><b>Forms of numbers</b>
  /// </para>
  /// <para>There are several other types of numbers that are mentioned
  /// in this class and elsewhere in this documentation. For reference,
  /// they are specified here.</para>
  /// <para><b>Unsigned integer</b>
  ///  : An integer that's always 0 or
  /// greater, with the following maximum values:</para>
  /// <list><item>8-bit unsigned integer, or <i>byte</i>
  ///  : 255.</item>
  /// <item>16-bit unsigned integer: 65535.</item>
  ///  <item>32-bit unsigned
  /// integer: (2 <sup>32</sup>
  ///  -1).</item>
  ///  <item>64-bit unsigned
  /// integer: (2 <sup>64</sup>
  ///  -1).</item>
  ///  </list>
  /// <para><b>Signed integer</b>
  ///  : An integer in <i>two's-complement
  /// form</i>
  ///  , with the following ranges:</para>
  /// <list><item>8-bit signed integer: -128 to 127.</item>
  ///  <item>16-bit
  /// signed integer: -32768 to 32767.</item>
  ///  <item>32-bit signed
  /// integer: -2 <sup>31</sup>
  ///  to (2 <sup>31</sup>
  ///  - 1).</item>
  /// <item>64-bit signed integer: -2 <sup>63</sup>
  ///  to (2 <sup>63</sup>
  ///  -
  /// 1).</item>
  ///  </list>
  /// <para><b>Two's complement form</b>
  ///  : In <i>two's-complement
  /// form</i>
  ///  , nonnegative numbers have the highest (most significant)
  /// bit set to zero, and negative numbers have that bit (and all bits
  /// beyond) set to one, and a negative number is stored in such form by
  /// decreasing its absolute value by 1 and swapping the bits of the
  /// resulting number.</para>
  /// <para><b>64-bit floating-point number</b>
  ///  : A 64-bit binary
  /// floating-point number, in the form <i>significand</i>
  ///  * 2
  /// <sup><i>exponent</i>
  ///  </sup>
  /// . The significand is 53 bits long
  /// (Precision) and the exponent ranges from -1074 (EMin) to 971
  /// (EMax). The number is stored in the following format (commonly
  /// called the IEEE 754 format):</para>
  /// <code>|C|BBB...BBB|AAAAAA...AAAAAA|</code>
  /// <list><item>A. Low 52 bits (Precision minus 1 bits): Lowest bits of
  /// the significand.</item>
  ///  <item>B. Next 11 bits: Exponent area:
  /// <list><item>If all bits are ones, the final stored value is
  /// infinity (positive or negative depending on the C bit) if all bits
  /// in area A are zeros, or not-a-number (NaN) otherwise.</item>
  /// <item>If all bits are zeros, the final stored value is a subnormal
  /// number, the exponent is EMin, and the highest bit of the
  /// significand is zero.</item>
  ///  <item>If any other number, the exponent
  /// is this value reduced by 1, then raised by EMin, and the highest
  /// bit of the significand is one.</item>
  ///  </list>
  ///  </item>
  ///  <item>C.
  /// Highest bit: If one, this is a negative number.</item>
  ///  </list>
  /// <para>The elements described above are in the same order as the
  /// order of each bit of each element, that is, either most significant
  /// first or least significant first.</para>
  /// <para><b>32-bit binary floating-point number</b>
  ///  : A 32-bit binary
  /// number which is stored similarly to a <i>64-bit floating-point
  /// number</i>
  ///  , except that:</para>
  /// <list><item>Precision is 24 bits.</item>
  ///  <item>EMin is -149.</item>
  /// <item>EMax is 104.</item>
  ///  <item>A. The low 23 bits (Precision minus
  /// 1 bits) are the lowest bits of the significand.</item>
  ///  <item>B. The
  /// next 8 bits are the exponent area.</item>
  ///  <item>C. If the highest
  /// bit is one, this is a negative number.</item>
  ///  </list>
  /// <para><b>.NET Framework decimal</b>
  ///  : A 128-bit decimal
  /// floating-point number, in the form <i>significand</i>
  ///  * 10 <sup>-
  /// <i>scale</i>
  ///  </sup>
  ///  , where the scale ranges from 0 to 28. The
  /// number is stored in the following format:</para>
  /// <list><item>Low 96 bits are the significand, as a 96-bit unsigned
  /// integer (all 96-bit values are allowed, up to (2 <sup>96</sup>
  /// -1)).</item>
  ///  <item>Next 16 bits are unused.</item>
  ///  <item>Next 8
  /// bits are the scale, stored as an 8-bit unsigned integer.</item>
  /// <item>Next 7 bits are unused.</item>
  ///  <item>If the highest bit is
  /// one, it's a negative number.</item>
  ///  </list>
  /// <para>The elements described above are in the same order as the
  /// order of each bit of each element, that is, either most significant
  /// first or least significant first.</para>
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Design",
      "CA1036",
      Justification = "Awaiting advice at dotnet/dotnet-api-docs#2937.")]
  public sealed partial class EDecimal : IComparable<EDecimal>,
    IEquatable<EDecimal> {
    private const int RepeatDivideThreshold = 10000;
    internal const int MaxSafeInt = 214748363;

    //----------------------------------------------------------------

    /// <summary>A not-a-number value.</summary>
    #if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Security",
        "CA2104", Justification = "EDecimal is immutable")]
    #endif
    public static readonly EDecimal NaN = CreateWithFlags(
        EInteger.Zero,
        EInteger.Zero,
        (byte)BigNumberFlags.FlagQuietNaN);

    /// <summary>Negative infinity, less than any other number.</summary>
    #if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Security",
        "CA2104", Justification = "EDecimal is immutable")]
    #endif
    public static readonly EDecimal NegativeInfinity =
      CreateWithFlags(
        EInteger.Zero,
        EInteger.Zero,
        BigNumberFlags.FlagInfinity | BigNumberFlags.FlagNegative);

    /// <summary>Represents the number negative zero.</summary>
    #if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Security",
        "CA2104", Justification = "EDecimal is immutable")]
    #endif
    public static readonly EDecimal NegativeZero =
      CreateWithFlags(
        EInteger.Zero,
        EInteger.Zero,
        BigNumberFlags.FlagNegative);

    /// <summary>Represents the number 1.</summary>
    #if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Security",
        "CA2104", Justification = "EDecimal is immutable")]
    #endif
    public static readonly EDecimal One = new EDecimal(
      FastIntegerFixed.FromInt32(1),
      FastIntegerFixed.Zero,
      (byte)0);

    /// <summary>Positive infinity, greater than any other
    /// number.</summary>
    #if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Security",
        "CA2104", Justification = "EDecimal is immutable")]
    #endif
    public static readonly EDecimal PositiveInfinity =
      CreateWithFlags(
        EInteger.Zero,
        EInteger.Zero,
        BigNumberFlags.FlagInfinity);

    /// <summary>A not-a-number value that signals an invalid operation
    /// flag when it's passed as an argument to any arithmetic operation in
    /// arbitrary-precision decimal.</summary>
    #if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Security",
        "CA2104", Justification = "EDecimal is immutable")]
    #endif
    public static readonly EDecimal SignalingNaN =
      CreateWithFlags(
        EInteger.Zero,
        EInteger.Zero,
        BigNumberFlags.FlagSignalingNaN);

    /// <summary>Represents the number 10.</summary>
    #if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Security",
        "CA2104", Justification = "EDecimal is immutable")]
    #endif
    public static readonly EDecimal Ten = new EDecimal(
      FastIntegerFixed.FromInt32(10),
      FastIntegerFixed.Zero,
      (byte)0);

    /// <summary>Represents the number 0.</summary>
    #if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Security",
        "CA2104", Justification = "EDecimal is immutable")]
    #endif
    public static readonly EDecimal Zero = new EDecimal(
      FastIntegerFixed.Zero,
      FastIntegerFixed.Zero,
      (byte)0);

    private const int CacheFirst = -24;
    private const int CacheLast = 128;
    private static readonly EDecimal[] Cache = EDecimalCache(CacheFirst,
        CacheLast);

    internal static EDecimal FromCache(int v) {
      return Cache[v - CacheFirst];
    }

    private static EDecimal[] EDecimalCache(int first, int last) {
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

      var cache = new EDecimal[(last - first) + 1];
      int i;
      for (i = first; i <= last; ++i) {
        if (i == 0) {
          cache[i - first] = Zero;
        } else if (i == 1) {
          cache[i - first] = One;
        } else if (i == 10) {
          cache[i - first] = Ten;
        } else {
          cache[i - first] = new EDecimal(
            FastIntegerFixed.FromInt32(Math.Abs(i)),
            FastIntegerFixed.Zero,
            (byte)(i < 0 ? BigNumberFlags.FlagNegative : 0));
        }
      }
      return cache;
    }

    private static readonly DecimalMathHelper HelperValue = new
    DecimalMathHelper();

    private static readonly IRadixMath<EDecimal> ExtendedMathValue = new
    RadixMath<EDecimal>(HelperValue);
    //----------------------------------------------------------------
    private static readonly IRadixMath<EDecimal> MathValue = new
    TrappableRadixMath<EDecimal>(
      new ExtendedOrSimpleRadixMath<EDecimal>(HelperValue));

    private static readonly int[] ValueTenPowers = {
      1, 10, 100, 1000, 10000, 100000,
      1000000, 10000000, 100000000,
      1000000000,
    };

    private readonly FastIntegerFixed unsignedMantissa;
    private readonly FastIntegerFixed exponent;
    private readonly byte flags;

    internal EDecimal(
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

    /// <summary>Creates a copy of this arbitrary-precision binary
    /// number.</summary>
    /// <returns>An arbitrary-precision decimal floating-point
    /// number.</returns>
    public EDecimal Copy() {
      return new EDecimal(
          this.unsignedMantissa.Copy(),
          this.exponent.Copy(),
          this.flags);
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
    /// infinity or NaN).</summary>
    /// <value><c>true</c> if this object is finite (not infinity or NaN);
    /// otherwise, <c>false</c>.</value>
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

    /// <summary>Returns whether this object's value is an
    /// integer.</summary>
    /// <returns><c>true</c> if this object's value is an integer;
    /// otherwise, <c>false</c>.</returns>
    public bool IsInteger() {
      if (!this.IsFinite) {
        return false;
      }
      if (this.IsZero || this.exponent.CompareToInt(0) >= 0) {
        return true;
      } else {
        EDecimal r = this.Reduce(null);
        return r.exponent.CompareToInt(0) >= 0;
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
            this.unsignedMantissa.IsValueZero) ? 0 : (((this.flags &
                BigNumberFlags.FlagNegative) != 0) ? -1 : 1);
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

    internal static EDecimal ChangeExponent(EDecimal ret, EInteger exponent) {
      return new EDecimal(
          ret.unsignedMantissa,
          FastIntegerFixed.FromBig(exponent),
          (byte)ret.flags);
    }

    /// <summary>Returns a number with the value
    /// <c>exponent*10^significand</c>.</summary>
    /// <param name='mantissaSmall'>Desired value for the
    /// significand.</param>
    /// <param name='exponentSmall'>Desired value for the exponent.</param>
    /// <returns>An arbitrary-precision decimal number.</returns>
    public static EDecimal Create(int mantissaSmall, int exponentSmall) {
      if (exponentSmall == 0 && mantissaSmall >= CacheFirst &&
        mantissaSmall <= CacheLast) {
        return Cache[mantissaSmall - CacheFirst];
      }
      if (mantissaSmall < 0) {
        if (mantissaSmall == Int32.MinValue) {
          FastIntegerFixed fi = FastIntegerFixed.FromInt64(Int32.MinValue);
          return new EDecimal(
              fi.Negate(),
              FastIntegerFixed.FromInt32(exponentSmall),
              (byte)BigNumberFlags.FlagNegative);
        }
        return new EDecimal(
            FastIntegerFixed.FromInt32(-mantissaSmall),
            FastIntegerFixed.FromInt32(exponentSmall),
            (byte)BigNumberFlags.FlagNegative);
      } else if (mantissaSmall == 0) {
        return new EDecimal(
            FastIntegerFixed.Zero,
            FastIntegerFixed.FromInt32(exponentSmall),
            (byte)0);
      } else {
        return new EDecimal(
            FastIntegerFixed.FromInt32(mantissaSmall),
            FastIntegerFixed.FromInt32(exponentSmall),
            (byte)0);
      }
    }

    /// <summary>Creates a number with the value
    /// <c>exponent*10^significand</c>.</summary>
    /// <param name='mantissa'>Desired value for the significand.</param>
    /// <param name='exponentSmall'>Desired value for the exponent.</param>
    /// <returns>An arbitrary-precision decimal number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='mantissa'/> is null.</exception>
    public static EDecimal Create(
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
      return new EDecimal(
          sign < 0 ? fi.Negate() : fi,
          FastIntegerFixed.FromInt32(exponentSmall),
          (byte)((sign < 0) ? BigNumberFlags.FlagNegative : 0));
    }

    /// <summary>Creates a number with the value
    /// <c>exponent*10^significand</c>.</summary>
    /// <param name='mantissa'>Desired value for the significand.</param>
    /// <param name='exponentLong'>Desired value for the exponent.</param>
    /// <returns>An arbitrary-precision decimal number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='mantissa'/> is null.</exception>
    public static EDecimal Create(
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
      return new EDecimal(
          sign < 0 ? fi.Negate() : fi,
          FastIntegerFixed.FromInt64(exponentLong),
          (byte)((sign < 0) ? BigNumberFlags.FlagNegative : 0));
    }

    /// <summary>Creates a number with the value
    /// <c>exponent*10^significand</c>.</summary>
    /// <param name='mantissa'>Desired value for the significand.</param>
    /// <param name='exponent'>Desired value for the exponent.</param>
    /// <returns>An arbitrary-precision decimal number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='mantissa'/> or <paramref name='exponent'/> is
    /// null.</exception>
    public static EDecimal Create(
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
      return new EDecimal(
          sign < 0 ? fi.Negate() : fi,
          FastIntegerFixed.FromBig(exponent),
          (byte)((sign < 0) ? BigNumberFlags.FlagNegative : 0));
    }

    /// <summary>Creates a number with the value
    /// <c>exponent*10^significand</c>.</summary>
    /// <param name='mantissaLong'>Desired value for the
    /// significand.</param>
    /// <param name='exponentSmall'>Desired value for the exponent.</param>
    /// <returns>An arbitrary-precision decimal number.</returns>
    public static EDecimal Create(
      long mantissaLong,
      int exponentSmall) {
      return Create(mantissaLong, (long)exponentSmall);
    }

    /// <summary>Creates a number with the value
    /// <c>exponent*10^significand</c>.</summary>
    /// <param name='mantissaLong'>Desired value for the
    /// significand.</param>
    /// <param name='exponentLong'>Desired value for the exponent.</param>
    /// <returns>An arbitrary-precision decimal number.</returns>
    public static EDecimal Create(
      long mantissaLong,
      long exponentLong) {
      if (mantissaLong >= Int32.MinValue && mantissaLong <= Int32.MaxValue &&
        exponentLong >= Int32.MinValue && exponentLong <= Int32.MaxValue) {
        return Create((int)mantissaLong, (int)exponentLong);
      } else if (mantissaLong == Int64.MinValue) {
        FastIntegerFixed fi = FastIntegerFixed.FromInt64(mantissaLong);
        return new EDecimal(
            fi.Negate(),
            FastIntegerFixed.FromInt64(exponentLong),
            (byte)((mantissaLong < 0) ? BigNumberFlags.FlagNegative : 0));
      } else {
        FastIntegerFixed fi = FastIntegerFixed.FromInt64(Math.Abs(
              mantissaLong));
        return new EDecimal(
            fi,
            FastIntegerFixed.FromInt64(exponentLong),
            (byte)((mantissaLong < 0) ? BigNumberFlags.FlagNegative : 0));
      }
    }

    /// <summary>Creates a not-a-number arbitrary-precision decimal
    /// number.</summary>
    /// <param name='diag'>An integer, 0 or greater, to use as diagnostic
    /// information associated with this object. If none is needed, should
    /// be zero. To get the diagnostic information from another
    /// arbitrary-precision decimal floating-point number, use that
    /// object's <c>UnsignedMantissa</c> property.</param>
    /// <returns>A quiet not-a-number.</returns>
    public static EDecimal CreateNaN(EInteger diag) {
      return CreateNaN(diag, false, false, null);
    }

    /// <summary>Creates a not-a-number arbitrary-precision decimal
    /// number.</summary>
    /// <param name='diag'>An integer, 0 or greater, to use as diagnostic
    /// information associated with this object. If none is needed, should
    /// be zero. To get the diagnostic information from another
    /// arbitrary-precision decimal floating-point number, use that
    /// object's <c>UnsignedMantissa</c> property.</param>
    /// <param name='signaling'>Whether the return value will be signaling
    /// (true) or quiet (false).</param>
    /// <param name='negative'>Whether the return value is
    /// negative.</param>
    /// <param name='ctx'>An arithmetic context to control the precision
    /// (in decimal digits) of the diagnostic information. The rounding and
    /// exponent range of this context will be ignored. Can be null. The
    /// only flag that can be signaled in this context is FlagInvalid,
    /// which happens if diagnostic information needs to be truncated and
    /// too much memory is required to do so.</param>
    /// <returns>An arbitrary-precision decimal number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='diag'/> is null or is less than 0.</exception>
    public static EDecimal CreateNaN(
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
        var ef = new EDecimal(
          FastIntegerFixed.FromBig(diag),
          FastIntegerFixed.Zero,
          (byte)flags).RoundToPrecision(ctx);

        int newFlags = ef.flags;
        newFlags &= ~BigNumberFlags.FlagQuietNaN;
        newFlags |= signaling ? BigNumberFlags.FlagSignalingNaN :
          BigNumberFlags.FlagQuietNaN;
        return new EDecimal(
            ef.unsignedMantissa,
            ef.exponent,
            (byte)newFlags);
      }
      flags |= signaling ? BigNumberFlags.FlagSignalingNaN :
        BigNumberFlags.FlagQuietNaN;
      return new EDecimal(
          FastIntegerFixed.FromBig(diag),
          FastIntegerFixed.Zero,
          (byte)flags);
    }

    /// <summary>Creates an arbitrary-precision decimal number from a
    /// 64-bit binary floating-point number. This method computes the exact
    /// value of the floating point number, not an approximation, as is
    /// often the case by converting the floating point number to a string
    /// first. Remember, though, that the exact value of a 64-bit binary
    /// floating-point number is not always the value that results when
    /// passing a literal decimal number (for example, calling
    /// <c>EDecimal.FromDouble(0.1)</c> ), since not all decimal numbers
    /// can be converted to exact binary numbers (in the example given, the
    /// resulting arbitrary-precision decimal will be the value of the
    /// closest "double" to 0.1, not 0.1 exactly). To create an
    /// arbitrary-precision decimal number from a decimal value, use
    /// FromString instead in most cases (for example:
    /// <c>EDecimal.FromString("0.1")</c> ).</summary>
    /// <param name='dbl'>The parameter <paramref name='dbl'/> is a 64-bit
    /// floating-point number.</param>
    /// <returns>An arbitrary-precision decimal number with the same value
    /// as <paramref name='dbl'/>.</returns>
    public static EDecimal FromDouble(double dbl) {
      long lvalue = BitConverter.ToInt64(
          BitConverter.GetBytes((double)dbl),
          0);
      return FromDoubleBits(lvalue);
    }

    /// <summary>Creates an arbitrary-precision decimal number from a
    /// 64-bit binary floating-point number, encoded in the IEEE 754
    /// binary64 format. This method computes the exact value of the
    /// floating point number, not an approximation, as is often the case
    /// by converting the floating point number to a string first.
    /// Remember, though, that the exact value of a 64-bit binary
    /// floating-point number is not always the value that results when
    /// passing a literal decimal number, since not all decimal numbers can
    /// be converted to exact binary numbers (in the example given, the
    /// resulting arbitrary-precision decimal will be the value of the
    /// closest "double" to 0.1, not 0.1 exactly). To create an
    /// arbitrary-precision decimal number from a decimal value, use
    /// FromString instead in most cases.</summary>
    /// <param name='dblBits'>The parameter <paramref name='dblBits'/> is a
    /// 64-bit signed integer.</param>
    /// <returns>An arbitrary-precision decimal number with the same value
    /// as <paramref name='dblBits'/>.</returns>
    public static EDecimal FromDoubleBits(long dblBits) {
      var value = new int[] {
        unchecked((int)(dblBits & 0xffffffffL)),
        unchecked((int)((dblBits >> 32) & 0xffffffffL)),
      };
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
            FastIntegerFixed.FromInt64(lvalue),
            FastIntegerFixed.Zero,
            (byte)flags);
      }
      value[1] &= 0xfffff;

      // Mask out the exponent and sign
      if (floatExponent == 0) {
        ++floatExponent;
      } else {
        value[1] |= 0x100000;
      }
      if ((value[1] | value[0]) != 0) {
        floatExponent += NumberUtility.ShiftAwayTrailingZerosTwoElements(
            value);
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
        EInteger bigexp = NumberUtility.FindPowerOfFive(-floatExponent);
        bigmantissa *= (EInteger)bigexp;
        if (neg) {
          bigmantissa = -(EInteger)bigmantissa;
        }
        return EDecimal.Create(bigmantissa, (EInteger)floatExponent);
      }
    }

    /// <summary>Converts an arbitrary-precision integer to an arbitrary
    /// precision decimal.</summary>
    /// <param name='bigint'>An arbitrary-precision integer.</param>
    /// <returns>An arbitrary-precision decimal number with the exponent
    /// set to 0.</returns>
    public static EDecimal FromEInteger(EInteger bigint) {
      return EDecimal.Create(bigint, EInteger.Zero);
    }

    /// <summary>Converts an arbitrary-precision binary floating-point
    /// number to an arbitrary precision decimal.</summary>
    /// <param name='ef'>An arbitrary-precision binary floating-point
    /// number.</param>
    /// <returns>An arbitrary-precision decimal number.</returns>
    [Obsolete("Renamed to FromEFloat.")]
    public static EDecimal FromExtendedFloat(EFloat ef) {
      return FromEFloat(ef);
    }

    /// <summary>Creates an arbitrary-precision decimal number from an
    /// arbitrary-precision binary floating-point number.</summary>
    /// <param name='bigfloat'>An arbitrary-precision binary floating-point
    /// number.</param>
    /// <returns>An arbitrary-precision decimal number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bigfloat'/> is null.</exception>
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
      EInteger bigSignedMantissa = bigfloat.Mantissa;
      if (bigSignedMantissa.IsZero) {
        return bigfloat.IsNegative ? EDecimal.NegativeZero :
          EDecimal.Zero;
      }
      if (bigintExp.IsZero) {
        // Integer
        return EDecimal.FromEInteger(bigSignedMantissa);
      }
      if (bigintExp.Sign > 0) {
        // Scaled integer
        FastInteger intcurexp = FastInteger.FromBig(bigintExp);
        EInteger bigmantissa = bigSignedMantissa;
        bool neg = bigmantissa.Sign < 0;
        if (neg) {
          bigmantissa = -(EInteger)bigmantissa;
        }
        while (intcurexp.Sign > 0) {
          var shift = 1000000;
          if (intcurexp.CompareToInt(1000000) < 0) {
            shift = intcurexp.ToInt32();
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
        EInteger bigmantissa = bigSignedMantissa;
        EInteger negbigintExp = -(EInteger)bigintExp;
        negbigintExp = NumberUtility.FindPowerOfFiveFromBig(negbigintExp);
        bigmantissa *= (EInteger)negbigintExp;
        return EDecimal.Create(bigmantissa, bigintExp);
      }
    }

    /// <summary>Converts a boolean value (true or false) to an
    /// arbitrary-precision decimal number.</summary>
    /// <param name='boolValue'>Either true or false.</param>
    /// <returns>The number 1 if <paramref name='boolValue'/> is true;
    /// otherwise, 0.</returns>
    public static EDecimal FromBoolean(bool boolValue) {
      return boolValue ? EDecimal.One : EDecimal.Zero;
    }

    /// <summary>Creates an arbitrary-precision decimal number from a
    /// 32-bit signed integer.</summary>
    /// <param name='valueSmaller'>The parameter <paramref
    /// name='valueSmaller'/> is a 32-bit signed integer.</param>
    /// <returns>An arbitrary-precision decimal number with the exponent
    /// set to 0.</returns>
    public static EDecimal FromInt32(int valueSmaller) {
      if (valueSmaller >= CacheFirst && valueSmaller <= CacheLast) {
        return Cache[valueSmaller - CacheFirst];
      }
      if (valueSmaller == Int32.MinValue) {
        return Create((EInteger)valueSmaller, EInteger.Zero);
      }
      if (valueSmaller < 0) {
        return new EDecimal(
            FastIntegerFixed.FromInt32(valueSmaller).Negate(),
            FastIntegerFixed.Zero,
            (byte)BigNumberFlags.FlagNegative);
      } else {
        return new EDecimal(
            FastIntegerFixed.FromInt32(valueSmaller),
            FastIntegerFixed.Zero,
            (byte)0);
      }
    }

    /// <summary>Converts an unsigned integer expressed as a 64-bit signed
    /// integer to an arbitrary-precision decimal number.</summary>
    /// <param name='longerValue'>A 64-bit signed integer. If this value is
    /// 0 or greater, the return value will represent it. If this value is
    /// less than 0, the return value will store 2^64 plus this value
    /// instead.</param>
    /// <returns>An arbitrary-precision decimal number with the exponent
    /// set to 0. If <paramref name='longerValue'/> is 0 or greater, the
    /// return value will represent it. If <paramref name='longerValue'/>
    /// is less than 0, the return value will store 2^64 plus this value
    /// instead.</returns>
    public static EDecimal FromInt64AsUnsigned(long longerValue) {
      return longerValue >= 0 ? FromInt64(longerValue) :
        FromEInteger(EInteger.FromInt64AsUnsigned(longerValue));
    }

    /// <summary>Creates an arbitrary-precision decimal number from a
    /// 64-bit signed integer.</summary>
    /// <param name='valueSmall'>The parameter <paramref
    /// name='valueSmall'/> is a 64-bit signed integer.</param>
    /// <returns>This number's value as an arbitrary-precision decimal
    /// number with the exponent set to 0.</returns>
    public static EDecimal FromInt64(long valueSmall) {
      if (valueSmall >= CacheFirst && valueSmall <= CacheLast) {
        return Cache[(int)(valueSmall - CacheFirst)];
      }
      if (valueSmall > Int32.MinValue && valueSmall <= Int32.MaxValue) {
        if (valueSmall < 0) {
          return new EDecimal(
              FastIntegerFixed.FromInt32((int)valueSmall).Negate(),
              FastIntegerFixed.Zero,
              (byte)BigNumberFlags.FlagNegative);
        } else {
          return new EDecimal(
              FastIntegerFixed.FromInt32((int)valueSmall),
              FastIntegerFixed.Zero,
              (byte)0);
        }
      }
      var bigint = (EInteger)valueSmall;
      return EDecimal.Create(bigint, EInteger.Zero);
    }

    /// <summary>Creates an arbitrary-precision decimal number from a
    /// 32-bit binary floating-point number. This method computes the exact
    /// value of the floating point number, not an approximation, as is
    /// often the case by converting the floating point number to a string
    /// first. Remember, though, that the exact value of a 32-bit binary
    /// floating-point number is not always the value that results when
    /// passing a literal decimal number (for example, calling
    /// <c>EDecimal.FromSingle(0.1f)</c> ), since not all decimal numbers
    /// can be converted to exact binary numbers (in the example given, the
    /// resulting arbitrary-precision decimal will be the the value of the
    /// closest "float" to 0.1, not 0.1 exactly). To create an
    /// arbitrary-precision decimal number from a decimal value, use
    /// FromString instead in most cases (for example:
    /// <c>EDecimal.FromString("0.1")</c> ).</summary>
    /// <param name='flt'>The parameter <paramref name='flt'/> is a 32-bit
    /// binary floating-point number.</param>
    /// <returns>An arbitrary-precision decimal number with the same value
    /// as <paramref name='flt'/>.</returns>
    public static EDecimal FromSingle(float flt) {
      int value = BitConverter.ToInt32(BitConverter.GetBytes((float)flt), 0);
      return FromSingleBits(value);
    }

    /// <summary>Creates an arbitrary-precision decimal number from a
    /// 32-bit binary floating-point number encoded in the IEEE 754
    /// binary32 format. This method computes the exact value of the
    /// floating point number, not an approximation, as is often the case
    /// by converting the floating point number to a string first.
    /// Remember, though, that the exact value of a 32-bit binary
    /// floating-point number is not always the value that results when
    /// passing a literal decimal number, since not all decimal numbers can
    /// be converted to exact binary numbers (in the example given, the
    /// resulting arbitrary-precision decimal will be the the value of the
    /// closest "float" to 0.1, not 0.1 exactly). To create an
    /// arbitrary-precision decimal number from a decimal value, use
    /// FromString instead in most cases.</summary>
    /// <param name='value'>A 32-bit binary floating-point number encoded
    /// in the IEEE 754 binary32 format.</param>
    /// <returns>An arbitrary-precision decimal number with the same value
    /// as <paramref name='value'/>.</returns>
    public static EDecimal FromSingleBits(int value) {
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
          (quiet ? BigNumberFlags.FlagQuietNaN :
            BigNumberFlags.FlagSignalingNaN);
        return valueFpMantissa == 0 ? (quiet ? NaN : SignalingNaN) :
          new EDecimal(
            FastIntegerFixed.FromInt32(valueFpMantissa),
            FastIntegerFixed.Zero,
            (byte)value);
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

    /// <summary>Creates an arbitrary-precision decimal number from a
    /// sequence of <c>char</c> s that represents a number. See
    /// <c>FromString(String, int, int, EContext)</c> for more information.
    /// Note that calling the overload that takes an EContext is often much
    /// faster than creating the EDecimal then calling
    /// <c>RoundToPrecision</c> on that EDecimal, especially if the context
    /// specifies a precision limit and exponent range.</summary>
    /// <param name='chars'>A sequence that represents a number.</param>
    /// <returns>An arbitrary-precision decimal number with the same value
    /// as the given sequence of <c>char</c> s.</returns>
    /// <exception cref='FormatException'>The parameter <paramref
    /// name='chars'/> is not a correctly formatted number
    /// sequence.</exception>
    public static EDecimal FromString(char[] chars) {
      return FromString(chars, 0, chars == null ? 0 : chars.Length, null);
    }

    /// <summary>Creates an arbitrary-precision decimal number from a
    /// sequence of <c>char</c> s that represents a number. See
    /// <c>FromString(String, int, int, EContext)</c> for more
    /// information.</summary>
    /// <param name='chars'>A sequence of <c>char</c> s that represents a
    /// number.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed. Note that providing a context is often much faster
    /// than creating the EDecimal without a context then calling
    /// <c>RoundToPrecision</c> on that EDecimal, especially if the context
    /// specifies a precision limit and exponent range.</param>
    /// <returns>An arbitrary-precision decimal number with the same value
    /// as the given sequence of <c>char</c> s.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='chars'/> is null.</exception>
    public static EDecimal FromString(char[] chars, EContext ctx) {
      return FromString(chars, 0, chars == null ? 0 : chars.Length, ctx);
    }

    /// <summary>Creates an arbitrary-precision decimal number from a
    /// sequence of <c>char</c> s that represents a number. See
    /// <c>FromString(String, int, int, EContext)</c> for more information.
    /// Note that calling the overload that takes an EContext is often much
    /// faster than creating the EDecimal then calling
    /// <c>RoundToPrecision</c> on that EDecimal, especially if the context
    /// specifies a precision limit and exponent range.</summary>
    /// <param name='chars'>A sequence that represents a number.</param>
    /// <param name='offset'>An index starting at 0 showing where the
    /// desired portion of <paramref name='chars'/> begins.</param>
    /// <param name='length'>The length, in code units, of the desired
    /// portion of <paramref name='chars'/> (but not more than <paramref
    /// name='chars'/> 's length).</param>
    /// <returns>An arbitrary-precision decimal number with the same value
    /// as the given sequence of <c>char</c> s.</returns>
    /// <exception cref='FormatException'>The parameter <paramref
    /// name='chars'/> is not a correctly formatted number
    /// sequence.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='chars'/> is null.</exception>
    /// <exception cref='ArgumentException'>Either <paramref
    /// name='offset'/> or <paramref name='length'/> is less than 0 or
    /// greater than <paramref name='chars'/> 's length, or <paramref
    /// name='chars'/> 's length minus <paramref name='offset'/> is less
    /// than <paramref name='length'/>.</exception>
    public static EDecimal FromString(
      char[] chars,
      int offset,
      int length) {
      return FromString(chars, offset, length, null);
    }

    /// <summary>
    /// <para>Creates an arbitrary-precision decimal number from a sequence
    /// of <c>char</c> s that represents a number.</para>
    /// <para>The format of the sequence generally consists of:</para>
    /// <list type=''>
    /// <item>An optional plus sign ("+" , U+002B) or minus sign ("-",
    /// U+002D) (if the minus sign, the value is negative.)</item>
    /// <item>One or more digits, with a single optional decimal point
    /// (".", U+002E) before or after those digits or between two of them.
    /// These digits may begin with any number of zeros.</item>
    /// <item>Optionally, "E"/"e" followed by an optional (positive
    /// exponent) or "-" (negative exponent) and followed by one or more
    /// digits specifying the exponent (these digits may begin with any
    /// number of zeros).</item></list>
    /// <para>The sequence can also be "-INF", "-Infinity", "Infinity",
    /// "INF", quiet NaN ("NaN" /"-NaN") followed by any number of digits
    /// (these digits may begin with any number of zeros), or signaling NaN
    /// ("sNaN" /"-sNaN") followed by any number of digits (these digits
    /// may begin with any number of zeros), all where the letters can be
    /// any combination of basic upper-case and/or basic lower-case
    /// letters.</para>
    /// <para>All characters mentioned above are the corresponding
    /// characters in the Basic Latin range. In particular, the digits must
    /// be the basic digits 0 to 9 (U+0030 to U+0039). The sequence is not
    /// allowed to contain white space characters, including
    /// spaces.</para></summary>
    /// <param name='chars'>A sequence of <c>char</c> s, a portion of which
    /// represents a number.</param>
    /// <param name='offset'>An index starting at 0 showing where the
    /// desired portion of <paramref name='chars'/> begins.</param>
    /// <param name='length'>The length, in code units, of the desired
    /// portion of <paramref name='chars'/> (but not more than <paramref
    /// name='chars'/> 's length).</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed. Note that providing a context is often much faster
    /// than creating the EDecimal without a context then calling
    /// <c>RoundToPrecision</c> on that EDecimal, especially if the context
    /// specifies a precision limit and exponent range.</param>
    /// <returns>An arbitrary-precision decimal number with the same value
    /// as the given sequence of <c>char</c> s.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='chars'/> is null.</exception>
    /// <exception cref='ArgumentException'>Either <paramref
    /// name='offset'/> or <paramref name='length'/> is less than 0 or
    /// greater than <paramref name='chars'/> 's length, or <paramref
    /// name='chars'/> 's length minus <paramref name='offset'/> is less
    /// than <paramref name='length'/>.</exception>
    public static EDecimal FromString(
      char[] chars,
      int offset,
      int length,
      EContext ctx) {
      if (chars == null) {
        throw new ArgumentNullException(nameof(chars));
      }
      return EDecimalCharArrayString.FromString(
          chars,
          offset,
          length,
          ctx,
          true);
    }

    /// <summary>Creates an arbitrary-precision decimal number from a
    /// sequence of bytes (interpreted as text) that represents a number.
    /// See <c>FromString(String, int, int, EContext)</c> for more
    /// information. Note that calling the overload that takes an EContext
    /// is often much faster than creating the EDecimal then calling
    /// <c>RoundToPrecision</c> on that EDecimal, especially if the context
    /// specifies a precision limit and exponent range.</summary>
    /// <param name='bytes'>A sequence that represents a number.</param>
    /// <returns>An arbitrary-precision decimal number with the same value
    /// as the given sequence of bytes (interpreted as text).</returns>
    /// <exception cref='FormatException'>The parameter <paramref
    /// name='bytes'/> is not a correctly formatted number
    /// sequence.</exception>
    public static EDecimal FromString(byte[] bytes) {
      return FromString(bytes, 0, bytes == null ? 0 : bytes.Length, null);
    }

    /// <summary>Creates an arbitrary-precision decimal number from a
    /// sequence of bytes (interpreted as text) that represents a number.
    /// See <c>FromString(String, int, int, EContext)</c> for more
    /// information.</summary>
    /// <param name='bytes'>A sequence of bytes (interpreted as text) that
    /// represents a number.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed. Note that providing a context is often much faster
    /// than creating the EDecimal without a context then calling
    /// <c>RoundToPrecision</c> on that EDecimal, especially if the context
    /// specifies a precision limit and exponent range.</param>
    /// <returns>An arbitrary-precision decimal number with the same value
    /// as the given sequence of bytes (interpreted as text).</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bytes'/> is null.</exception>
    public static EDecimal FromString(byte[] bytes, EContext ctx) {
      return FromString(bytes, 0, bytes == null ? 0 : bytes.Length, ctx);
    }

    /// <summary>Creates an arbitrary-precision decimal number from a
    /// sequence of bytes (interpreted as text) that represents a number.
    /// See <c>FromString(String, int, int, EContext)</c> for more
    /// information. Note that calling the overload that takes an EContext
    /// is often much faster than creating the EDecimal then calling
    /// <c>RoundToPrecision</c> on that EDecimal, especially if the context
    /// specifies a precision limit and exponent range.</summary>
    /// <param name='bytes'>A sequence that represents a number.</param>
    /// <param name='offset'>An index starting at 0 showing where the
    /// desired portion of <paramref name='bytes'/> begins.</param>
    /// <param name='length'>The length, in bytes, of the desired portion
    /// of <paramref name='bytes'/> (but not more than <paramref
    /// name='bytes'/> 's length).</param>
    /// <returns>An arbitrary-precision decimal number with the same value
    /// as the given sequence of bytes (interpreted as text).</returns>
    /// <exception cref='FormatException'>The parameter <paramref
    /// name='bytes'/> is not a correctly formatted number
    /// sequence.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bytes'/> is null.</exception>
    /// <exception cref='ArgumentException'>Either <paramref
    /// name='offset'/> or <paramref name='length'/> is less than 0 or
    /// greater than <paramref name='bytes'/> 's length, or <paramref
    /// name='bytes'/> 's length minus <paramref name='offset'/> is less
    /// than <paramref name='length'/>.</exception>
    public static EDecimal FromString(
      byte[] bytes,
      int offset,
      int length) {
      return FromString(bytes, offset, length, null);
    }

    /// <summary>
    /// <para>Creates an arbitrary-precision decimal number from a sequence
    /// of bytes (interpreted as text) that represents a number. Each byte
    /// in the sequence has to be a code point in the Basic Latin range
    /// (0x00 to 0x7f or U+0000 to U+007F) of the Unicode Standard.</para>
    /// <para>The format of the sequence generally consists of:</para>
    /// <list type=''>
    /// <item>An optional plus sign ("+" , U+002B) or minus sign ("-",
    /// U+002D) (if the minus sign, the value is negative.)</item>
    /// <item>One or more digits, with a single optional decimal point
    /// (".", U+002E) before or after those digits or between two of them.
    /// These digits may begin with any number of zeros.</item>
    /// <item>Optionally, "E"/"e" followed by an optional (positive
    /// exponent) or "-" (negative exponent) and followed by one or more
    /// digits specifying the exponent (these digits may begin with any
    /// number of zeros).</item></list>
    /// <para>The sequence can also be "-INF", "-Infinity", "Infinity",
    /// "INF", quiet NaN ("NaN" /"-NaN") followed by any number of digits
    /// (these digits may begin with any number of zeros), or signaling NaN
    /// ("sNaN" /"-sNaN") followed by any number of digits (these digits
    /// may begin with any number of zeros), all where the letters can be
    /// any combination of basic upper-case and/or basic lower-case
    /// letters.</para>
    /// <para>All characters mentioned above are the corresponding
    /// characters in the Basic Latin range. In particular, the digits must
    /// be the basic digits 0 to 9 (U+0030 to U+0039). The sequence is not
    /// allowed to contain white space characters, including
    /// spaces.</para></summary>
    /// <param name='bytes'>A sequence of bytes (interpreted as text), a
    /// portion of which represents a number.</param>
    /// <param name='offset'>An index starting at 0 showing where the
    /// desired portion of <paramref name='bytes'/> begins.</param>
    /// <param name='length'>The length, in code units, of the desired
    /// portion of <paramref name='bytes'/> (but not more than <paramref
    /// name='bytes'/> 's length).</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed. Note that providing a context is often much faster
    /// than creating the EDecimal without a context then calling
    /// <c>RoundToPrecision</c> on that EDecimal, especially if the context
    /// specifies a precision limit and exponent range.</param>
    /// <returns>An arbitrary-precision decimal number with the same value
    /// as the given sequence of bytes (interpreted as text).</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bytes'/> is null.</exception>
    /// <exception cref='ArgumentException'>Either <paramref
    /// name='offset'/> or <paramref name='length'/> is less than 0 or
    /// greater than <paramref name='bytes'/> 's length, or <paramref
    /// name='bytes'/> 's length minus <paramref name='offset'/> is less
    /// than <paramref name='length'/>.</exception>
    public static EDecimal FromString(
      byte[] bytes,
      int offset,
      int length,
      EContext ctx) {
      if (bytes == null) {
        throw new ArgumentNullException(nameof(bytes));
      }
      return EDecimalByteArrayString.FromString(
          bytes,
          offset,
          length,
          ctx,
          true);
    }

    /// <summary>Creates an arbitrary-precision decimal number from a text
    /// string that represents a number. See <c>FromString(String, int,
    /// int, EContext)</c> for more information. Note that calling the
    /// overload that takes an EContext is often much faster than creating
    /// the EDecimal then calling <c>RoundToPrecision</c> on that EDecimal,
    /// especially if the context specifies a precision limit and exponent
    /// range.</summary>
    /// <param name='str'>A string that represents a number.</param>
    /// <returns>An arbitrary-precision decimal number with the same value
    /// as the given string.</returns>
    /// <exception cref='FormatException'>The parameter <paramref
    /// name='str'/> is not a correctly formatted number
    /// string.</exception>
    public static EDecimal FromString(string str) {
      return FromString(str, 0, str == null ? 0 : str.Length, null);
    }

    /// <summary>Creates an arbitrary-precision decimal number from a text
    /// string that represents a number. See <c>FromString(String, int,
    /// int, EContext)</c> for more information.</summary>
    /// <param name='str'>A string that represents a number.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed. Note that providing a context is often much faster
    /// than creating the EDecimal without a context then calling
    /// <c>RoundToPrecision</c> on that EDecimal, especially if the context
    /// specifies a precision limit and exponent range.</param>
    /// <returns>An arbitrary-precision decimal number with the same value
    /// as the given string.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='str'/> is null.</exception>
    public static EDecimal FromString(string str, EContext ctx) {
      return FromString(str, 0, str == null ? 0 : str.Length, ctx);
    }

    /// <summary>Creates an arbitrary-precision decimal number from a text
    /// string that represents a number. See <c>FromString(String, int,
    /// int, EContext)</c> for more information. Note that calling the
    /// overload that takes an EContext is often much faster than creating
    /// the EDecimal then calling <c>RoundToPrecision</c> on that EDecimal,
    /// especially if the context specifies a precision limit and exponent
    /// range.</summary>
    /// <param name='str'>A string that represents a number.</param>
    /// <param name='offset'>An index starting at 0 showing where the
    /// desired portion of <paramref name='str'/> begins.</param>
    /// <param name='length'>The length, in code units, of the desired
    /// portion of <paramref name='str'/> (but not more than <paramref
    /// name='str'/> 's length).</param>
    /// <returns>An arbitrary-precision decimal number with the same value
    /// as the given string.</returns>
    /// <exception cref='FormatException'>The parameter <paramref
    /// name='str'/> is not a correctly formatted number
    /// string.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='str'/> is null.</exception>
    /// <exception cref='ArgumentException'>Either <paramref
    /// name='offset'/> or <paramref name='length'/> is less than 0 or
    /// greater than <paramref name='str'/> 's length, or <paramref
    /// name='str'/> 's length minus <paramref name='offset'/> is less than
    /// <paramref name='length'/>.</exception>
    public static EDecimal FromString(
      string str,
      int offset,
      int length) {
      return FromString(str, offset, length, null);
    }

    /// <summary>
    /// <para>Creates an arbitrary-precision decimal number from a text
    /// string that represents a number.</para>
    /// <para>The format of the string generally consists of:</para>
    /// <list type=''>
    /// <item>An optional plus sign ("+" , U+002B) or minus sign ("-",
    /// U+002D) (if the minus sign, the value is negative.)</item>
    /// <item>One or more digits, with a single optional decimal point
    /// (".", U+002E) before or after those digits or between two of them.
    /// These digits may begin with any number of zeros.</item>
    /// <item>Optionally, "E"/"e" followed by an optional (positive
    /// exponent) or "-" (negative exponent) and followed by one or more
    /// digits specifying the exponent (these digits may begin with any
    /// number of zeros).</item></list>
    /// <para>The string can also be "-INF", "-Infinity", "Infinity",
    /// "INF", quiet NaN ("NaN" /"-NaN") followed by any number of digits
    /// (these digits may begin with any number of zeros), or signaling NaN
    /// ("sNaN" /"-sNaN") followed by any number of digits (these digits
    /// may begin with any number of zeros), all where the letters can be
    /// any combination of basic upper-case and/or basic lower-case
    /// letters.</para>
    /// <para>All characters mentioned above are the corresponding
    /// characters in the Basic Latin range. In particular, the digits must
    /// be the basic digits 0 to 9 (U+0030 to U+0039). The string is not
    /// allowed to contain white space characters, including
    /// spaces.</para></summary>
    /// <param name='str'>A text string, a portion of which represents a
    /// number.</param>
    /// <param name='offset'>An index starting at 0 showing where the
    /// desired portion of <paramref name='str'/> begins.</param>
    /// <param name='length'>The length, in code units, of the desired
    /// portion of <paramref name='str'/> (but not more than <paramref
    /// name='str'/> 's length).</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed. Note that providing a context is often much faster
    /// than creating the EDecimal without a context then calling
    /// <c>RoundToPrecision</c> on that EDecimal, especially if the context
    /// specifies a precision limit and exponent range.</param>
    /// <returns>An arbitrary-precision decimal number with the same value
    /// as the given string.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='str'/> is null.</exception>
    /// <exception cref='ArgumentException'>Either <paramref
    /// name='offset'/> or <paramref name='length'/> is less than 0 or
    /// greater than <paramref name='str'/> 's length, or <paramref
    /// name='str'/> 's length minus <paramref name='offset'/> is less than
    /// <paramref name='length'/>.</exception>
    public static EDecimal FromString(
      string str,
      int offset,
      int length,
      EContext ctx) {
      int tmpoffset = offset;
      if (str == null) {
        throw new ArgumentNullException(nameof(str));
      }
      return EDecimalTextString.FromString(str, offset, length, ctx, true);
    }

    internal static EDecimal SignalUnderflow(EContext ec, bool negative, bool
      zeroSignificand) {
      EInteger eTiny = ec.EMin.Subtract(ec.Precision.Subtract(1));
      eTiny = eTiny.Subtract(2); // subtract 2 from proper eTiny to
      // trigger underflow (2, rather than 1, because of HalfUp mode)
      EDecimal ret = EDecimal.Create(
          zeroSignificand ? EInteger.Zero : EInteger.One,
          eTiny);
      if (negative) {
        ret = ret.Negate();
      }
      return ret.RoundToPrecision(ec);
    }

    internal static EDecimal SignalOverflow(EContext ec, bool negative, bool
      zeroSignificand) {
      if (zeroSignificand) {
        EDecimal ret = EDecimal.Create(EInteger.Zero, ec.EMax);
        if (negative) {
          ret = ret.Negate();
        }
        return ret.RoundToPrecision(ec);
      } else {
        return GetMathValue(ec).SignalOverflow(ec, negative);
      }
    }

    // 1 = Overflow; 2 = Underflow, adjust significand to 1; 0 = None;
    // 3 = Underflow, adjust significant to have precision
    internal static int CheckOverflowUnderflow(
      EContext ec,
      int precisionInt,
      EInteger exponent) {
      // NOTE: precisionInt is an Int32 because the maximum supported
      // length of a string fits in an Int32
      // NOTE: Not guaranteed to catch all overflows or underflows.
      if (exponent == null) {
        throw new ArgumentNullException(nameof(exponent));
      }
      if (precisionInt < 0) {
        throw new ArgumentException("doesn't satisfy precision.Sign>= 0");
      }
      // "Precision" is the number of digits in a number starting with
      // the first nonzero digit
      if (ec == null || !ec.HasExponentRange) {
        return 0;
      }
      if (ec.AdjustExponent) {
        // If precision is in bits, this is too difficult to determine,
        // so ignore precision
        if (ec.IsPrecisionInBits) {
          if (exponent.CompareTo(ec.EMax) > 0) {
            return 1; // Overflow
          }
        } else {
          EInteger adjExponent = exponent.Add(precisionInt).Subtract(1);
          if (adjExponent.CompareTo(ec.EMax) > 0) {
            return 1; // Overflow
          }
          if (ec.HasMaxPrecision) {
            EInteger etiny = ec.EMin.Subtract(ec.Precision.Subtract(1));
            etiny = etiny.Subtract(1); // Buffer in case of rounding
            // DebugUtility.Log("adj: adjexp=" + adjExponent + " exp=" + exponent + "
            // etiny="+etiny);
            if (adjExponent.CompareTo(etiny) < 0) {
              return 2; // Underflow to zero
            }
          } else {
            EInteger etiny = ec.EMin.Subtract(precisionInt - 1);
            etiny = etiny.Subtract(1); // Buffer in case of rounding
            // DebugUtility.Log("adj: adjexp=" + adjExponent + " exp=" + exponent + "
            // etiny="+etiny);
            if (adjExponent.CompareTo(etiny) < 0) {
              return 3; // Underflow to zero
            }
          }
        }
      } else {
        // Exponent range is independent of precision
        if (exponent.CompareTo(ec.EMax) > 0) {
          return 1; // Overflow
        }
        if (!ec.IsPrecisionInBits) {
          EInteger adjExponent = exponent.Add(precisionInt).Subtract(1);
          EInteger etiny = ec.HasMaxPrecision ?
            ec.EMin.Subtract(ec.Precision.Subtract(1)) :
            ec.EMin.Subtract(precisionInt - 1);
          etiny = etiny.Subtract(1); // Buffer in case of rounding
          // DebugUtility.Log("noadj: adjexp=" + adjExponent + " exp=" + exponent + "
          // etiny="+etiny);
          if (adjExponent.CompareTo(etiny) < 0) {
            return 2; // Underflow to zero
          }
        }
      }
      return 0;
    }

    /// <summary>Gets the greater value between two decimal
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
    public static EDecimal Max(
      EDecimal first,
      EDecimal second,
      EContext ctx) {
      if (first == null) {
        throw new ArgumentNullException(nameof(first));
      }
      if (second == null) {
        throw new ArgumentNullException(nameof(second));
      }
      return GetMathValue(ctx).Max(first, second, ctx);
    }

    /// <summary>Gets the greater value between two decimal
    /// numbers.</summary>
    /// <param name='first'>An arbitrary-precision decimal number.</param>
    /// <param name='second'>Another arbitrary-precision decimal
    /// number.</param>
    /// <returns>The larger value of the two numbers. If one is positive
    /// zero and the other is negative zero, returns the positive zero. If
    /// the two numbers are positive and have the same value, returns the
    /// one with the larger exponent. If the two numbers are negative and
    /// have the same value, returns the one with the smaller
    /// exponent.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='first'/> or <paramref name='second'/> is null.</exception>
    public static EDecimal Max(
      EDecimal first,
      EDecimal second) {
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
    public static EDecimal MaxMagnitude(
      EDecimal first,
      EDecimal second,
      EContext ctx) {
      if (first == null) {
        throw new ArgumentNullException(nameof(first));
      }
      if (second == null) {
        throw new ArgumentNullException(nameof(second));
      }
      return GetMathValue(ctx).MaxMagnitude(first, second, ctx);
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
    public static EDecimal MaxMagnitude(
      EDecimal first,
      EDecimal second) {
      if (first == null) {
        throw new ArgumentNullException(nameof(first));
      }
      if (second == null) {
        throw new ArgumentNullException(nameof(second));
      }
      return MaxMagnitude(first, second, null);
    }

    /// <summary>Gets the lesser value between two decimal
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
    public static EDecimal Min(
      EDecimal first,
      EDecimal second,
      EContext ctx) {
      if (first == null) {
        throw new ArgumentNullException(nameof(first));
      }
      if (second == null) {
        throw new ArgumentNullException(nameof(second));
      }
      return GetMathValue(ctx).Min(first, second, ctx);
    }

    /// <summary>Gets the lesser value between two decimal
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
    public static EDecimal Min(
      EDecimal first,
      EDecimal second) {
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
    public static EDecimal MinMagnitude(
      EDecimal first,
      EDecimal second,
      EContext ctx) {
      if (first == null) {
        throw new ArgumentNullException(nameof(first));
      }
      if (second == null) {
        throw new ArgumentNullException(nameof(second));
      }
      return GetMathValue(ctx).MinMagnitude(first, second, ctx);
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
    public static EDecimal MinMagnitude(
      EDecimal first,
      EDecimal second) {
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
    public static EDecimal PI(EContext ctx) {
      return GetMathValue(ctx).Pi(ctx);
    }

    /// <summary>Finds the absolute value of this object (if it's negative,
    /// it becomes positive).</summary>
    /// <returns>An arbitrary-precision decimal number. Returns signaling
    /// NaN if this value is signaling NaN. (In this sense, this method is
    /// similar to the "copy-abs" operation in the General Decimal
    /// Arithmetic Specification, except this method does not necessarily
    /// return a copy of this object.).</returns>
    public EDecimal Abs() {
      if (this.IsNegative) {
        var er = new EDecimal(
          this.unsignedMantissa,
          this.exponent,
          (byte)(this.flags & ~BigNumberFlags.FlagNegative));
        return er;
      }
      return this;
    }

    /// <summary>Returns a number with the same value as this one, but
    /// copying the sign (positive or negative) of another number. (This
    /// method is similar to the "copy-sign" operation in the General
    /// Decimal Arithmetic Specification, except this method does not
    /// necessarily return a copy of this object.).</summary>
    /// <param name='other'>A number whose sign will be copied.</param>
    /// <returns>An arbitrary-precision decimal number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='other'/> is null.</exception>
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
    public EDecimal Abs(EContext context) {
      return ((context == null || context == EContext.UnlimitedHalfEven) ?
          ExtendedMathValue : MathValue).Abs(this, context);
    }

    /// <summary>Adds this arbitrary-precision decimal floating-point
    /// number and another arbitrary-precision decimal floating-point
    /// number and returns the result. The exponent for the result is the
    /// lower of this arbitrary-precision decimal floating-point number's
    /// exponent and the other arbitrary-precision decimal floating-point
    /// number's exponent.</summary>
    /// <param name='otherValue'>An arbitrary-precision decimal
    /// number.</param>
    /// <returns>The sum of the two numbers, that is, this
    /// arbitrary-precision decimal floating-point number plus another
    /// arbitrary-precision decimal floating-point number. If this
    /// arbitrary-precision decimal floating-point number is not-a-number
    /// (NaN), returns NaN.</returns>
    public EDecimal Add(EDecimal otherValue) {
      if (this.IsFinite && otherValue != null && otherValue.IsFinite &&
        ((this.flags | otherValue.flags) & BigNumberFlags.FlagNegative) == 0 &&
        this.exponent.CompareTo(otherValue.exponent) == 0) {
        FastIntegerFixed result = FastIntegerFixed.Add(
            this.unsignedMantissa,
            otherValue.unsignedMantissa);
        return new EDecimal(result, this.exponent, (byte)0);
      }
      return this.Add(otherValue, EContext.UnlimitedHalfEven);
    }

    /// <summary>Adds this arbitrary-precision decimal floating-point
    /// number and another arbitrary-precision decimal floating-point
    /// number and returns the result.</summary>
    /// <param name='otherValue'>The number to add to.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and no
    /// rounding is needed.</param>
    /// <returns>The sum of the two numbers, that is, this
    /// arbitrary-precision decimal floating-point number plus another
    /// arbitrary-precision decimal floating-point number. If this
    /// arbitrary-precision decimal floating-point number is not-a-number
    /// (NaN), returns NaN.</returns>
    public EDecimal Add(
      EDecimal otherValue,
      EContext ctx) {
      return GetMathValue(ctx).Add(this, otherValue, ctx);
    }

    /// <summary>Compares the mathematical values of this object and
    /// another object, accepting NaN values. This method currently uses
    /// the rules given in the CompareToValue method, so that it it is not
    /// consistent with the Equals method, but it may change in a future
    /// version to use the rules for the CompareToTotal method
    /// instead.</summary>
    /// <param name='other'>An arbitrary-precision decimal number.</param>
    /// <returns>Less than 0 if this object's value is less than the other
    /// value, or greater than 0 if this object's value is greater than the
    /// other value or if <paramref name='other'/> is null, or 0 if both
    /// values are equal.
    /// <para>This implementation returns a positive number if <paramref
    /// name='other'/> is null, to conform to the.NET definition of
    /// CompareTo. This is the case even in the Java version of this
    /// library, for consistency's sake, even though implementations of
    /// <c>Comparable.compareTo()</c> in Java ought to throw an exception
    /// if they receive a null argument rather than treating null as less
    /// or greater than any object.</para>.</returns>
    public int CompareTo(EDecimal other) {
      return this.CompareToValue(other);
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
      return this.CompareToValue(EDecimal.FromInt32(intOther));
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
    /// a 32-bit signed integer.</param>
    /// <returns>Less than 0 if this object's value is less than the other
    /// value, or greater than 0 if this object's value is greater than the
    /// other value, or 0 if both values are equal.</returns>
    public int CompareToValue(int intOther) {
      return this.CompareToValue(EDecimal.FromInt32(intOther));
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
      return this.CompareToValue(EDecimal.FromInt64(intOther));
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
      return this.CompareToValue(EDecimal.FromInt64(intOther));
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
    /// <param name='other'>An arbitrary-precision decimal number.</param>
    /// <returns>Less than 0 if this object's value is less than the other
    /// value, or greater than 0 if this object's value is greater than the
    /// other value or if <paramref name='other'/> is null, or 0 if both
    /// values are equal.
    /// <para>This implementation returns a positive number if <paramref
    /// name='other'/> is null, to conform to the.NET definition of
    /// CompareTo. This is the case even in the Java version of this
    /// library, for consistency's sake, even though implementations of
    /// <c>Comparable.compareTo()</c> in Java ought to throw an exception
    /// if they receive a null argument rather than treating null as less
    /// or greater than any object.</para>.</returns>
    public int CompareToValue(EDecimal other) {
      return ExtendedMathValue.CompareTo(this, other);
    }

    /// <summary>Compares an arbitrary-precision binary floating-point
    /// number with this instance.</summary>
    /// <param name='other'>The other object to compare. Can be
    /// null.</param>
    /// <returns>Zero if the values are equal; a negative number if this
    /// instance is less; or a positive number if this instance is greater.
    /// Returns 0 if both values are NaN (even signaling NaN) and 1 if this
    /// value is NaN (even signaling NaN) and the other isn't, or if the
    /// other value is null.
    /// <para>This implementation returns a positive number if <paramref
    /// name='other'/> is null, to conform to the.NET definition of
    /// CompareTo. This is the case even in the Java version of this
    /// library, for consistency's sake, even though implementations of
    /// <c>Comparable.compareTo()</c> in Java ought to throw an exception
    /// if they receive a null argument rather than treating null as less
    /// or greater than any object.</para>.</returns>
    public int CompareToBinary(EFloat other) {
      return CompareEDecimalToEFloat(this, other);
    }
    private static int CompareEDecimalToEFloat(EDecimal ed, EFloat ef) {
      if (ef == null) {
        return 1;
      }
      if (ed.IsNaN()) {
        return ef.IsNaN() ? 0 : 1;
      }
      int signA = ed.Sign;
      int signB = ef.Sign;
      if (signA != signB) {
        return (signA < signB) ? -1 : 1;
      }
      if (signB == 0 || signA == 0) {
        // Special case: Either operand is zero
        return 0;
      }
      if (ed.IsInfinity()) {
        if (ef.IsInfinity()) {
          // if we get here, this only means that
          // both are positive infinity or both
          // are negative infinity
          return 0;
        }
        return ed.IsNegative ? -1 : 1;
      }
      if (ef.IsInfinity()) {
        return ef.IsNegative ? 1 : -1;
      }
      // At this point, both numbers are finite and
      // have the same sign
      #if DEBUG
      if (!ed.IsFinite) {
        throw new InvalidOperationException("doesn't satisfy this.IsFinite");
      }
      if (!ef.IsFinite) {
        throw new InvalidOperationException("doesn't satisfy other.IsFinite");
      }
      #endif
      // DebugUtility.Log("efexp=" + ef.Exponent);
      if (ef.Exponent.CompareTo((EInteger)(-1000)) < 0) {
        // For very low exponents (less than -1000), the conversion to
        // decimal can take very long, so try this approach
        if (ef.Abs(null).CompareTo(EFloat.One) < 0) {
          // Abs less than 1
          if (ed.Abs(null).CompareTo(EDecimal.One) >= 0) {
            // Abs 1 or more
            return (signA > 0) ? 1 : -1;
          }
        }
        // DebugUtility.Log("edexp=" + ed.Exponent + ", efexp=" +
        // ef.Exponent);
        EInteger bitCount = ef.Mantissa.GetUnsignedBitLengthAsEInteger();
        EInteger absexp = ef.Exponent.Abs();
        if (absexp.CompareTo(bitCount) > 0) {
          // Float's absolute value is less than 1, so do a trial comparison
          // using a different EFloat with the same significand but
          // with an exponent changed to be closer to 0
          EFloat trial = EFloat.Create(ef.Mantissa, EInteger.FromInt32(
                -1000));
          int trialcmp = CompareEDecimalToEFloat(ed, trial);
          // DebugUtility.Log("trialcmp result="+trialcmp);
          if (ef.Sign < 0 && trialcmp < 0) {
            // if float and decimal are negative and
            // decimal is less than trial float (which in turn is
            // less than the actual float), then the decimal is
            // less than the actual float
            return -1;
          }
          if (ef.Sign > 0 && trialcmp > 0) {
            // if float and decimal are positive and
            // decimal is greater than trial float (which in turn is
            // greater than the actual float), then the decimal is
            // greater than the actual float
            return 1;
          }
        }
        EInteger[] thisAdjExpBounds = GetAdjustedExponentDecimalBounds(ed);
        EInteger otherAdjExp = GetAdjustedExponentBinary(ef);
        // DebugUtility.Log("taexp=" + thisAdjExp + ", oaexp=" + otherAdjExp);
        // DebugUtility.Log("td=" + ed.ToDouble() + ", tf=" + ef.ToDouble());
        if (thisAdjExpBounds[0].Sign < 0 &&
          thisAdjExpBounds[0].CompareTo(-1000) >= 0 &&
          otherAdjExp.CompareTo(-4000) < 0) {
          // With these exponent combinations, the binary's absolute
          // value is less than the decimal's
          return (signA > 0) ? 1 : -1;
        }
        if (thisAdjExpBounds[1].Sign < 0 &&
          thisAdjExpBounds[1].CompareTo(-1000) < 0 &&
          otherAdjExp.CompareTo(-1000) < 0) {
          // Check the ratio of the negative binary exponent to
          // the negative decimal exponent.
          // If the ratio times 1000, rounded down, is less than 3321, the
          // binary's absolute value is
          // greater. If it's greater than 3322, the decimal's absolute value is
          // greater.
          // (If the two absolute values are equal, the ratio will approach
          // ln(10)/ln(2), or about 3.32193, as the exponents get higher and
          // higher.) If it's 3321 to 3322, the two numbers being compared may or may
          // not be equal. This check assumes that both exponents are less than
          // -1000, when the ratio between exponents of equal values is
          // close to ln(10)/ln(2).
          EInteger ratio;
          EInteger adjexp;
          EInteger divisor1 = thisAdjExpBounds[0].Add(1).Abs();
          EInteger divisor2 = thisAdjExpBounds[1].Add(1).Abs();
          otherAdjExp = otherAdjExp.Add(1).Abs();
          adjexp = EInteger.Min(divisor1, divisor2);
          ratio = otherAdjExp.Multiply(1000).Divide(adjexp);
          // DebugUtility.Log("taexp=" + adjexp + ", oaexp=" +
          // otherAdjExp + " ratio=" + ratio);
          if (ratio.CompareTo(3321) < 0) {
            // Binary abs. value is greater
            return (signA > 0) ? -1 : 1;
          }
          adjexp = EInteger.Max(divisor1, divisor2);
          ratio = otherAdjExp.Multiply(1000).Divide(adjexp);
          if (ratio.CompareTo(3322) > 0) {
            return (signA > 0) ? 1 : -1;
          }
        }
      }
      if (ef.Exponent.CompareTo(1000) > 0) {
        // Very high exponents
        EInteger bignum = EInteger.One.ShiftLeft(999);
        if (ed.Abs(null).CompareToValue(EDecimal.FromEInteger(bignum)) <=
          0) {
          // this object's absolute value is less
          return (signA > 0) ? -1 : 1;
        }
        // NOTE: The following check assumes that both
        // operands are nonzero
        EInteger[] thisAdjExpBounds = GetAdjustedExponentDecimalBounds(ed);
        EInteger otherAdjExp = GetAdjustedExponentBinary(ef);
        if (thisAdjExpBounds[0].Sign > 0 &&
          thisAdjExpBounds[0].CompareTo(otherAdjExp) >= 0) {
          // This object's adjusted exponent lower bound
          // is greater and is positive;
          // so this (decimal) object's absolute value is greater,
          // since exponents have a greater value in decimal than in binary
          return (signA > 0) ? 1 : -1;
        }
        if (thisAdjExpBounds[1].Sign > 0 &&
          thisAdjExpBounds[1].CompareTo(1000) < 0 &&
          otherAdjExp.CompareTo(4000) >= 0) {
          // With these exponent combinations (using the adjusted exponent
          // upper bound), the binary's absolute
          // value is greater than the decimal's
          return (signA > 0) ? -1 : 1;
        }
        if (thisAdjExpBounds[0].Sign > 0 &&
          thisAdjExpBounds[0].CompareTo(1000) >= 0 &&
          otherAdjExp.CompareTo(1000) >= 0) {
          // Check the ratio of the binary exponent to the decimal exponent.
          // If the ratio times 1000, rounded down, is less than 3321, the
          // decimal's absolute value is
          // greater. If it's 3322 or greater, the binary's absolute value is
          // greater.
          // (If the two absolute values are equal, the ratio will approach
          // ln(10)/ln(2), or about 3.32193, as the exponents get higher and
          // higher.) This check assumes that both exponents are 1000 or
          // greater, when the ratio between exponents of equal values is
          // close to ln(10)/ln(2).
          EInteger ratio;
          EInteger adjexp;
          EInteger divisor1 = thisAdjExpBounds[0].Add(1);
          EInteger divisor2 = thisAdjExpBounds[1].Add(1);
          otherAdjExp = otherAdjExp.Add(1);
          adjexp = EInteger.Min(divisor1, divisor2);
          ratio = otherAdjExp.Multiply(1000).Divide(adjexp);
          if (ratio.CompareTo(3321) < 0) {
            // Decimal abs. value is greater
            return (signA > 0) ? 1 : -1;
          }
          adjexp = EInteger.Max(divisor1, divisor2);
          ratio = otherAdjExp.Multiply(1000).Divide(adjexp);
          if (ratio.CompareTo(3322) >= 0) {
            return (signA > 0) ? -1 : 1;
          }
        }
      }
      EDecimal otherDec = EDecimal.FromEFloat(ef);
      /* DebugUtility.Log("Traditional compare");
      DebugUtility.Log("ef="+ef);
      string ed0=""+ed;
      string od0=""+otherDec;
      if (!ed0.Equals(od0)) {
        DebugUtility.Log("ed="+ed0);
        DebugUtility.Log("od="+od0);
      }
      */
      return ed.CompareTo(otherDec);
    }

    /// <summary>Compares the mathematical values of this object and
    /// another object, treating quiet NaN as signaling.
    /// <para>In this method, negative zero and positive zero are
    /// considered equal.</para>
    /// <para>If this object or the other object is a quiet NaN or
    /// signaling NaN, this method will return a quiet NaN and will signal
    /// a FlagInvalid flag.</para></summary>
    /// <param name='other'>An arbitrary-precision decimal number.</param>
    /// <param name='ctx'>An arithmetic context. The precision, rounding,
    /// and exponent range are ignored. If <c>HasFlags</c> of the context
    /// is true, will store the flags resulting from the operation (the
    /// flags are in addition to the pre-existing flags). Can be
    /// null.</param>
    /// <returns>Quiet NaN if this object or the other object is NaN, or 0
    /// if both objects have the same value, or -1 if this object is less
    /// than the other value, or a 1 if this object is greater.
    /// <para>This implementation returns a positive number if <paramref
    /// name='other'/> is null, to conform to the.NET definition of
    /// CompareTo. This is the case even in the Java version of this
    /// library, for consistency's sake, even though implementations of
    /// <c>Comparable.compareTo()</c> in Java ought to throw an exception
    /// if they receive a null argument rather than treating null as less
    /// or greater than any object.</para>.</returns>
    public EDecimal CompareToSignal(
      EDecimal other,
      EContext ctx) {
      return GetMathValue(ctx).CompareToWithContext(this, other, true, ctx);
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
    /// <param name='other'>An arbitrary-precision decimal number to
    /// compare with this one.</param>
    /// <returns>The number 0 if both objects have the same value (ignoring
    /// their signs), or -1 if this object is less than the other value
    /// (ignoring their signs), or 1 if this object is greater (ignoring
    /// their signs).
    /// <para>This implementation returns a positive number if <paramref
    /// name='other'/> is null, to conform to the.NET definition of
    /// CompareTo. This is the case even in the Java version of this
    /// library, for consistency's sake, even though implementations of
    /// <c>Comparable.compareTo()</c> in Java ought to throw an exception
    /// if they receive a null argument rather than treating null as less
    /// or greater than any object.</para>.</returns>
    public int CompareToTotalMagnitude(EDecimal other) {
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
    /// <param name='other'>An arbitrary-precision decimal number to
    /// compare with this one.</param>
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
    public int CompareToTotal(EDecimal other, EContext ctx) {
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
    /// <param name='other'>An arbitrary-precision decimal number to
    /// compare with this one.</param>
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
    public int CompareToTotalMagnitude(EDecimal other, EContext ctx) {
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
    /// <param name='other'>An arbitrary-precision decimal number to
    /// compare with this one.</param>
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

    /// <summary>Compares the mathematical values of this object and
    /// another object.
    /// <para>In this method, negative zero and positive zero are
    /// considered equal.</para>
    /// <para>If this object or the other object is a quiet NaN or
    /// signaling NaN, this method returns a quiet NaN, and will signal a
    /// FlagInvalid flag if either is a signaling NaN.</para></summary>
    /// <param name='other'>An arbitrary-precision decimal number.</param>
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
    public EDecimal CompareToWithContext(
      EDecimal other,
      EContext ctx) {
      return GetMathValue(ctx).CompareToWithContext(this, other, false, ctx);
    }

    /// <summary>Divides this arbitrary-precision decimal floating-point
    /// number by another arbitrary-precision decimal floating-point number
    /// and returns the result; returns NaN instead if the result would
    /// have a nonterminating decimal expansion (including 1/3, 1/12, 1/7,
    /// 2/3, and so on); if this is not desired, use DivideToExponent, or
    /// use the Divide overload that takes an EContext.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <returns>The result of dividing this arbitrary-precision decimal
    /// floating-point number by another arbitrary-precision decimal
    /// floating-point number. Returns infinity if the divisor (this
    /// arbitrary-precision decimal floating-point number) is 0 and the
    /// dividend (the other arbitrary-precision decimal floating-point
    /// number) is nonzero. Returns not-a-number (NaN) if the divisor and
    /// the dividend are 0. Returns NaN if the result can't be exact
    /// because it would have a nonterminating binary expansion (examples
    /// include 1 divided by any multiple of 3, such as 1/3 or 1/12). If
    /// this is not desired, use DivideToExponent instead, or use the
    /// Divide overload that takes an <c>EContext</c> (such as
    /// <c>EContext.Decimal128</c> ) instead.</returns>
    public EDecimal Divide(EDecimal divisor) {
      return this.Divide(
          divisor,
          EContext.ForRounding(ERounding.None));
    }

    /// <summary>Divides this arbitrary-precision decimal floating-point
    /// number by another arbitrary-precision decimal floating-point number
    /// and returns the result.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and no
    /// rounding is needed.</param>
    /// <returns>The result of dividing this arbitrary-precision decimal
    /// floating-point number by another arbitrary-precision decimal
    /// floating-point number. Signals FlagDivideByZero and returns
    /// infinity if the divisor (this arbitrary-precision decimal
    /// floating-point number) is 0 and the dividend (the other
    /// arbitrary-precision decimal floating-point number) is nonzero.
    /// Signals FlagInvalid and returns not-a-number (NaN) if the divisor
    /// and the dividend are 0; or, either <paramref name='ctx'/> is null
    /// or <paramref name='ctx'/> 's precision is 0, and the result would
    /// have a nonterminating decimal expansion (examples include 1 divided
    /// by any multiple of 3, such as 1/3 or 1/12); or, the rounding mode
    /// is ERounding.None and the result is not exact.</returns>
    public EDecimal Divide(
      EDecimal divisor,
      EContext ctx) {
      return GetMathValue(ctx).Divide(this, divisor, ctx);
    }

    /// <summary>Calculates the quotient and remainder using the
    /// DivideToIntegerNaturalScale and the formula in
    /// RemainderNaturalScale.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <returns>A 2 element array consisting of the quotient and remainder
    /// in that order.</returns>
    [Obsolete("Renamed to DivRemNaturalScale.")]
    public EDecimal[] DivideAndRemainderNaturalScale(EDecimal
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
    public EDecimal[] DivideAndRemainderNaturalScale(
      EDecimal divisor,
      EContext ctx) {
      return this.DivRemNaturalScale(divisor, ctx);
    }

    /// <summary>Divides this arbitrary-precision decimal floating-point
    /// number by another arbitrary-precision decimal floating-point number
    /// and returns a two-item array containing the result of the division
    /// and the remainder, in that order. The result of division is
    /// calculated as though by <c>DivideToIntegerNaturalScale</c>, and
    /// the remainder is calculated as though by
    /// <c>RemainderNaturalScale</c>.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <returns>An array of two items: the first is the result of the
    /// division as an arbitrary-precision decimal floating-point number,
    /// and the second is the remainder as an arbitrary-precision decimal
    /// floating-point number. The result of division is the result of the
    /// method on the two operands, and the remainder is the result of the
    /// Remainder method on the two operands.</returns>
    public EDecimal[] DivRemNaturalScale(EDecimal
      divisor) {
      return this.DivRemNaturalScale(divisor, null);
    }

    /// <summary>Divides this arbitrary-precision decimal floating-point
    /// number by another arbitrary-precision decimal floating-point number
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
    /// division as an arbitrary-precision decimal floating-point number,
    /// and the second is the remainder as an arbitrary-precision decimal
    /// floating-point number. The result of division is the result of the
    /// method on the two operands, and the remainder is the result of the
    /// Remainder method on the two operands.</returns>
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

    /// <summary>Divides two arbitrary-precision decimal numbers, and gives
    /// a particular exponent to the result.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='desiredExponentSmall'>The desired exponent. A negative
    /// number places the cutoff point to the right of the usual decimal
    /// point (so a negative number means the number of decimal places to
    /// round to). A positive number places the cutoff point to the left of
    /// the usual decimal point.</param>
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
    public EDecimal DivideToExponent(
      EDecimal divisor,
      long desiredExponentSmall,
      EContext ctx) {
      return this.DivideToExponent(
          divisor,
          (EInteger)desiredExponentSmall,
          ctx);
    }

    /// <summary>Divides two arbitrary-precision decimal numbers, and gives
    /// a particular exponent (expressed as a 32-bit signed integer) to the
    /// result, using the half-even rounding mode.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='desiredExponentInt'>The desired exponent. A negative
    /// number places the cutoff point to the right of the usual decimal
    /// point (so a negative number means the number of decimal places to
    /// round to). A positive number places the cutoff point to the left of
    /// the usual decimal point.</param>
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
    public EDecimal DivideToExponent(
      EDecimal divisor,
      int desiredExponentInt,
      EContext ctx) {
      return this.DivideToExponent(
          divisor,
          (EInteger)desiredExponentInt,
          ctx);
    }

    /// <summary>Divides two arbitrary-precision decimal numbers, and gives
    /// a particular exponent to the result.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='desiredExponentSmall'>The desired exponent. A negative
    /// number places the cutoff point to the right of the usual decimal
    /// point (so a negative number means the number of decimal places to
    /// round to). A positive number places the cutoff point to the left of
    /// the usual decimal point.</param>
    /// <param name='rounding'>The rounding mode to use if the result must
    /// be scaled down to have the same exponent as this value.</param>
    /// <returns>The quotient of the two objects. Signals FlagDivideByZero
    /// and returns infinity if the divisor is 0 and the dividend is
    /// nonzero. Signals FlagInvalid and returns not-a-number (NaN) if the
    /// divisor and the dividend are 0. Signals FlagInvalid and returns
    /// not-a-number (NaN) if the rounding mode is ERounding.None and the
    /// result is not exact.</returns>
    public EDecimal DivideToExponent(
      EDecimal divisor,
      long desiredExponentSmall,
      ERounding rounding) {
      return this.DivideToExponent(
          divisor,
          (EInteger)desiredExponentSmall,
          EContext.ForRounding(rounding));
    }

    /// <summary>Divides two arbitrary-precision decimal numbers, and gives
    /// a particular exponent (expressed as a 32-bit signed integer) to the
    /// result, using the half-even rounding mode.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='desiredExponentInt'>The desired exponent. A negative
    /// number places the cutoff point to the right of the usual decimal
    /// point (so a negative number means the number of decimal places to
    /// round to). A positive number places the cutoff point to the left of
    /// the usual decimal point.</param>
    /// <param name='rounding'>The rounding mode to use if the result must
    /// be scaled down to have the same exponent as this value.</param>
    /// <returns>The quotient of the two objects. Signals FlagDivideByZero
    /// and returns infinity if the divisor is 0 and the dividend is
    /// nonzero. Signals FlagInvalid and returns not-a-number (NaN) if the
    /// divisor and the dividend are 0. Signals FlagInvalid and returns
    /// not-a-number (NaN) if the rounding mode is ERounding.None and the
    /// result is not exact.</returns>
    public EDecimal DivideToExponent(
      EDecimal divisor,
      int desiredExponentInt,
      ERounding rounding) {
      return this.DivideToExponent(
          divisor,
          (EInteger)desiredExponentInt,
          EContext.ForRounding(rounding));
    }

    /// <summary>Divides two arbitrary-precision decimal numbers, and gives
    /// a particular exponent to the result.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='exponent'>The desired exponent. A negative number
    /// places the cutoff point to the right of the usual decimal point (so
    /// a negative number means the number of decimal places to round to).
    /// A positive number places the cutoff point to the left of the usual
    /// decimal point.</param>
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
    public EDecimal DivideToExponent(
      EDecimal divisor,
      EInteger exponent,
      EContext ctx) {
      return GetMathValue(ctx).DivideToExponent(this, divisor, exponent, ctx);
    }

    /// <summary>Divides two arbitrary-precision decimal numbers, and gives
    /// a particular exponent to the result, using the half-even rounding
    /// mode.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='exponent'>The desired exponent. A negative number
    /// places the cutoff point to the right of the usual decimal point (so
    /// a negative number means the number of decimal places to round to).
    /// A positive number places the cutoff point to the left of the usual
    /// decimal point.</param>
    /// <returns>The quotient of the two objects. Signals FlagDivideByZero
    /// and returns infinity if the divisor is 0 and the dividend is
    /// nonzero. Signals FlagInvalid and returns not-a-number (NaN) if the
    /// divisor and the dividend are 0.</returns>
    public EDecimal DivideToExponent(
      EDecimal divisor,
      EInteger exponent) {
      return this.DivideToExponent(divisor, exponent, ERounding.HalfEven);
    }

    /// <summary>Divides two arbitrary-precision decimal numbers, and gives
    /// a particular exponent (expressed as a 64-bit signed integer) to the
    /// result, using the half-even rounding mode.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='desiredExponentSmall'>The desired exponent. A negative
    /// number places the cutoff point to the right of the usual decimal
    /// point (so a negative number means the number of decimal places to
    /// round to). A positive number places the cutoff point to the left of
    /// the usual decimal point.</param>
    /// <returns>The quotient of the two objects. Signals FlagDivideByZero
    /// and returns infinity if the divisor is 0 and the dividend is
    /// nonzero. Signals FlagInvalid and returns not-a-number (NaN) if the
    /// divisor and the dividend are 0.</returns>
    public EDecimal DivideToExponent(
      EDecimal divisor,
      long desiredExponentSmall) {
      return this.DivideToExponent(
          divisor,
          desiredExponentSmall,
          ERounding.HalfEven);
    }

    /// <summary>Divides two arbitrary-precision decimal numbers, and gives
    /// a particular exponent (expressed as a 32-bit signed integer) to the
    /// result, using the half-even rounding mode.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='desiredExponentInt'>The desired exponent. A negative
    /// number places the cutoff point to the right of the usual decimal
    /// point (so a negative number means the number of decimal places to
    /// round to). A positive number places the cutoff point to the left of
    /// the usual decimal point.</param>
    /// <returns>The quotient of the two objects. Signals FlagDivideByZero
    /// and returns infinity if the divisor is 0 and the dividend is
    /// nonzero. Signals FlagInvalid and returns not-a-number (NaN) if the
    /// divisor and the dividend are 0.</returns>
    public EDecimal DivideToExponent(
      EDecimal divisor,
      int desiredExponentInt) {
      return this.DivideToExponent(
          divisor,
          desiredExponentInt,
          ERounding.HalfEven);
    }

    /// <summary>Divides two arbitrary-precision decimal numbers, and gives
    /// a particular exponent to the result.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='desiredExponent'>The desired exponent. A negative
    /// number places the cutoff point to the right of the usual decimal
    /// point (so a negative number means the number of decimal places to
    /// round to). A positive number places the cutoff point to the left of
    /// the usual decimal point.</param>
    /// <param name='rounding'>The rounding mode to use if the result must
    /// be scaled down to have the same exponent as this value.</param>
    /// <returns>The quotient of the two objects. Signals FlagDivideByZero
    /// and returns infinity if the divisor is 0 and the dividend is
    /// nonzero. Returns not-a-number (NaN) if the divisor and the dividend
    /// are 0. Returns NaN if the rounding mode is ERounding.None and the
    /// result is not exact.</returns>
    public EDecimal DivideToExponent(
      EDecimal divisor,
      EInteger desiredExponent,
      ERounding rounding) {
      return this.DivideToExponent(
          divisor,
          desiredExponent,
          EContext.ForRounding(rounding));
    }

    /// <summary>Divides two arbitrary-precision decimal numbers, and
    /// returns the integer part of the result, rounded down, with the
    /// preferred exponent set to this value's exponent minus the divisor's
    /// exponent.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <returns>The integer part of the quotient of the two objects.
    /// Signals FlagDivideByZero and returns infinity if the divisor is 0
    /// and the dividend is nonzero. Signals FlagInvalid and returns
    /// not-a-number (NaN) if the divisor and the dividend are 0.</returns>
    public EDecimal DivideToIntegerNaturalScale(EDecimal
      divisor) {
      return this.DivideToIntegerNaturalScale(
          divisor,
          EContext.ForRounding(ERounding.Down));
    }

    /// <summary>Divides this object by another object, and returns the
    /// integer part of the result (which is initially rounded down), with
    /// the preferred exponent set to this value's exponent minus the
    /// divisor's exponent.</summary>
    /// <param name='divisor'>The parameter <paramref name='divisor'/> is
    /// an arbitrary-precision decimal floating-point number.</param>
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
    public EDecimal DivideToIntegerNaturalScale(
      EDecimal divisor,
      EContext ctx) {
      return GetMathValue(ctx).DivideToIntegerNaturalScale(
          this,
          divisor,
          ctx);
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
    public EDecimal DivideToIntegerZeroScale(
      EDecimal divisor,
      EContext ctx) {
      return GetMathValue(ctx).DivideToIntegerZeroScale(this, divisor, ctx);
    }

    /// <summary>Divides this object by another decimal number and returns
    /// a result with the same exponent as this object (the
    /// dividend).</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='rounding'>The rounding mode to use if the result must
    /// be scaled down to have the same exponent as this value.</param>
    /// <returns>The quotient of the two numbers. Signals FlagDivideByZero
    /// and returns infinity if the divisor is 0 and the dividend is
    /// nonzero. Signals FlagInvalid and returns not-a-number (NaN) if the
    /// divisor and the dividend are 0. Signals FlagInvalid and returns
    /// not-a-number (NaN) if the rounding mode is ERounding.None and the
    /// result is not exact.</returns>
    public EDecimal DivideToSameExponent(
      EDecimal divisor,
      ERounding rounding) {
      return this.DivideToExponent(
          divisor,
          this.exponent.ToEInteger(),
          EContext.ForRounding(rounding));
    }

    /// <summary>Determines whether this object's significand, exponent,
    /// and properties are equal to those of another object. Not-a-number
    /// values are considered equal if the rest of their properties are
    /// equal.</summary>
    /// <param name='other'>An arbitrary-precision decimal number.</param>
    /// <returns><c>true</c> if this object's significand and exponent are
    /// equal to those of another object; otherwise, <c>false</c>.</returns>
    public bool Equals(EDecimal other) {
      return this.EqualsInternal(other);
    }

    /// <summary>Determines whether this object's significand, exponent,
    /// and properties are equal to those of another object and that other
    /// object is an arbitrary-precision decimal number. Not-a-number
    /// values are considered equal if the rest of their properties are
    /// equal.</summary>
    /// <param name='obj'>The parameter <paramref name='obj'/> is an
    /// arbitrary object.</param>
    /// <returns><c>true</c> if the objects are equal; otherwise,
    /// <c>false</c>. In this method, two objects are not equal if they
    /// don't have the same type or if one is null and the other
    /// isn't.</returns>
    public override bool Equals(object obj) {
      return this.EqualsInternal(obj as EDecimal);
    }

    /// <summary>Finds e (the base of natural logarithms) raised to the
    /// power of this object's value.</summary>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// <i>This parameter can't be null, as the exponential function's
    /// results are generally not exact.</i> (Unlike in the General Decimal
    /// Arithmetic Specification, any rounding mode is allowed.).</param>
    /// <returns>Exponential of this object. If this object's value is 1,
    /// returns an approximation to " e" within the given precision.
    /// Signals FlagInvalid and returns not-a-number (NaN) if the parameter
    /// <paramref name='ctx'/> is null or the precision is unlimited (the
    /// context's Precision property is 0).</returns>
    public EDecimal Exp(EContext ctx) {
      return GetMathValue(ctx).Exp(this, ctx);
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
    public EDecimal ExpM1(EContext ctx) {
      EDecimal value = this;
      if (value.IsNaN()) {
        return value.Plus(ctx);
      }
      if (ctx == null || !ctx.HasMaxPrecision) {
        return EDecimal.SignalingNaN.Plus(ctx);
      }
      if (ctx.Traps != 0) {
        EContext tctx = ctx.GetNontrapping();
        EDecimal ret = value.ExpM1(tctx);
        return ctx.TriggerTraps(ret, tctx);
      } else if (ctx.IsSimplified) {
        EContext tmpctx = ctx.WithSimplified(false).WithBlankFlags();
        EDecimal ret = value.PreRound(ctx).ExpM1(tmpctx);
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
          return EDecimal.NegativeInfinity;
        } else if (value.IsPositiveInfinity()) {
          return EDecimal.PositiveInfinity;
        } else if (value.IsNegativeInfinity()) {
          return EDecimal.FromInt32(-1).Plus(ctx);
        } else if (value.CompareTo(0) == 0) {
          return EDecimal.FromInt32(0).Plus(ctx);
        }
        int flags = ctx.Flags;
        EContext tmpctx = null;
        EDecimal ret;
        {
          EInteger prec = ctx.Precision.Add(3);
          tmpctx = ctx.WithBigPrecision(prec).WithBlankFlags();
          if (value.Abs().CompareTo(EDecimal.Create(5, -1)) < 0) {
            ret = value.Exp(tmpctx).Add(EDecimal.FromInt32(-1), ctx);
            EDecimal oldret = ret;
            while (true) {
              prec = prec.Add(ctx.Precision).Add(3);
              tmpctx = ctx.WithBigPrecision(prec).WithBlankFlags();
              ret = value.Exp(tmpctx).Add(EDecimal.FromInt32(-1), ctx);
              if (ret.CompareTo(0) != 0 && ret.CompareTo(oldret) == 0) {
                break;
              }
              oldret = ret;
            }
          } else {
            ret = value.Exp(tmpctx).Add(EDecimal.FromInt32(-1), ctx);
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
      var hashCode = 964453631;
      unchecked {
        hashCode += 964453723 * this.exponent.GetHashCode();
        hashCode += 964453939 * this.unsignedMantissa.GetHashCode();
        hashCode += 964453967 * this.flags;
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
            BigNumberFlags.FlagNegative)) == (BigNumberFlags.FlagInfinity |
          BigNumberFlags.FlagNegative);
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
    /// generally not exact.</i> (Unlike in the General Decimal Arithmetic
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
    public EDecimal Log(EContext ctx) {
      return GetMathValue(ctx).Ln(this, ctx);
    }

    /// <summary>Finds the base-10 logarithm of this object, that is, the
    /// power (exponent) that the number 10 must be raised to in order to
    /// equal this object's value.</summary>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// <i>This parameter can't be null, as the ln function's results are
    /// generally not exact.</i> (Unlike in the General Decimal Arithmetic
    /// Specification, any rounding mode is allowed.).</param>
    /// <returns>Ln(this object)/Ln(10). Signals the flag FlagInvalid and
    /// returns not-a-number (NaN) if this object is less than 0. Signals
    /// FlagInvalid and returns not-a-number (NaN) if the parameter
    /// <paramref name='ctx'/> is null or the precision is unlimited (the
    /// context's Precision property is 0).</returns>
    public EDecimal Log10(EContext ctx) {
      return this.LogN(EDecimal.FromInt32(10), ctx);
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
    public EDecimal Log1P(EContext ctx) {
      EDecimal value = this;
      if (value.IsNaN()) {
        return value.Plus(ctx);
      }
      if (ctx == null || !ctx.HasMaxPrecision ||
        (value.CompareTo(-1) < 0)) {
        return EDecimal.SignalingNaN.Plus(ctx);
      }
      if (ctx.Traps != 0) {
        EContext tctx = ctx.GetNontrapping();
        EDecimal ret = value.Log1P(tctx);
        return ctx.TriggerTraps(ret, tctx);
      } else if (ctx.IsSimplified) {
        EContext tmpctx = ctx.WithSimplified(false).WithBlankFlags();
        EDecimal ret = value.PreRound(ctx).Log1P(tmpctx);
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
          return EDecimal.NegativeInfinity;
        } else if (value.IsPositiveInfinity()) {
          return EDecimal.PositiveInfinity;
        }
        if (value.CompareTo(0) == 0) {
          return EDecimal.FromInt32(0).Plus(ctx);
        }
        int flags = ctx.Flags;
        EContext tmpctx = null;
        EDecimal ret;
        // DebugUtility.Log("cmp=" +
        // value.CompareTo(EDecimal.Create(1, -1)) +
        // " add=" + value.Add(EDecimal.FromInt32(1)));
        if (value.CompareTo(EDecimal.Create(5, -1)) < 0) {
          ret = value.Add(EDecimal.FromInt32(1)).Log(ctx);
        } else {
          tmpctx = ctx.WithBigPrecision(ctx.Precision.Add(3)).WithBlankFlags();
          ret = value.Add(EDecimal.FromInt32(1), tmpctx).Log(ctx);
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
    /// is a Numbers.EDecimal object.</param>
    /// <param name='ctx'>The parameter <paramref name='ctx'/> is a
    /// Numbers.EContext object.</param>
    /// <returns>Ln(this object)/Ln(baseValue). Signals the flag
    /// FlagInvalid and returns not-a-number (NaN) if this object is less
    /// than 0. Signals FlagInvalid and returns not-a-number (NaN) if the
    /// parameter <paramref name='ctx'/> is null or the precision is
    /// unlimited (the context's Precision property is 0).</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='baseValue'/> is null.</exception>
    public EDecimal LogN(EDecimal baseValue, EContext ctx) {
      EDecimal value = this;
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
        return EDecimal.SignalingNaN.Plus(ctx);
      }
      if (ctx.Traps != 0) {
        EContext tctx = ctx.GetNontrapping();
        EDecimal ret = value.LogN(baseValue, tctx);
        return ctx.TriggerTraps(ret, tctx);
      } else if (ctx.IsSimplified) {
        EContext tmpctx = ctx.WithSimplified(false).WithBlankFlags();
        EDecimal ret = value.PreRound(ctx).LogN(baseValue.PreRound(ctx),
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
          return baseValue.CompareTo(1) < 0 ? EDecimal.PositiveInfinity :
            EDecimal.NegativeInfinity;
        } else if (value.IsPositiveInfinity()) {
          return baseValue.CompareTo(1) < 0 ? EDecimal.NegativeInfinity :
            EDecimal.PositiveInfinity;
        }
        if (baseValue.CompareTo(10) == 0) {
          EDecimal ev = value.Reduce(null);
          if (ev.UnsignedMantissa.CompareTo(1) == 0) {
            return EDecimal.FromEInteger(ev.Exponent).Plus(ctx);
          }
        } else if (value.CompareTo(1) == 0) {
          return EDecimal.FromInt32(0).Plus(ctx);
        } else if (value.CompareTo(baseValue) == 0) {
          return EDecimal.FromInt32(1).Plus(ctx);
        }
        int flags = ctx.Flags;
        EContext tmpctx =
          ctx.WithBigPrecision(ctx.Precision.Add(5)).WithBlankFlags();
        EDecimal ret = value.Log(tmpctx).Divide(baseValue.Log(tmpctx), ctx);
        if (ret.IsInteger() && !ret.IsZero) {
          flags |= EContext.FlagRounded | EContext.FlagInexact;
          if (baseValue.Pow(ret).CompareToValue(value) == 0) {
            EDecimal rtmp = ret.Quantize(EDecimal.FromInt32(1),
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

    /// <summary>Returns a number similar to this number but with the
    /// decimal point moved to the left.</summary>
    /// <param name='places'>The number of decimal places to move the
    /// decimal point to the left. If this number is negative, instead
    /// moves the decimal point to the right by this number's absolute
    /// value.</param>
    /// <returns>A number whose exponent is decreased by <paramref
    /// name='places'/>, but not to more than 0.</returns>
    public EDecimal MovePointLeft(int places) {
      return this.MovePointLeft((EInteger)places, null);
    }

    /// <summary>Returns a number similar to this number but with the
    /// decimal point moved to the left.</summary>
    /// <param name='places'>The number of decimal places to move the
    /// decimal point to the left. If this number is negative, instead
    /// moves the decimal point to the right by this number's absolute
    /// value.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed.</param>
    /// <returns>A number whose exponent is decreased by <paramref
    /// name='places'/>, but not to more than 0.</returns>
    public EDecimal MovePointLeft(int places, EContext ctx) {
      return this.MovePointLeft((EInteger)places, ctx);
    }

    /// <summary>Returns a number similar to this number but with the
    /// decimal point moved to the left.</summary>
    /// <param name='bigPlaces'>The number of decimal places to move the
    /// decimal point to the left. If this number is negative, instead
    /// moves the decimal point to the right by this number's absolute
    /// value.</param>
    /// <returns>A number whose exponent is decreased by <paramref
    /// name='bigPlaces'/>, but not to more than 0.</returns>
    public EDecimal MovePointLeft(EInteger bigPlaces) {
      return this.MovePointLeft(bigPlaces, null);
    }

    /// <summary>Returns a number similar to this number but with the
    /// decimal point moved to the left.</summary>
    /// <param name='bigPlaces'>The number of decimal places to move the
    /// decimal point to the left. If this number is negative, instead
    /// moves the decimal point to the right by this number's absolute
    /// value.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed.</param>
    /// <returns>A number whose exponent is decreased by <paramref
    /// name='bigPlaces'/>, but not to more than 0.</returns>
    public EDecimal MovePointLeft(
      EInteger bigPlaces,
      EContext ctx) {
      return (!this.IsFinite) ? this.RoundToPrecision(ctx) :
        this.MovePointRight(-(EInteger)bigPlaces, ctx);
    }

    /// <summary>Returns a number similar to this number but with the
    /// decimal point moved to the right.</summary>
    /// <param name='places'>The number of decimal places to move the
    /// decimal point to the right. If this number is negative, instead
    /// moves the decimal point to the left by this number's absolute
    /// value.</param>
    /// <returns>A number whose exponent is increased by <paramref
    /// name='places'/>, but not to more than 0.</returns>
    public EDecimal MovePointRight(int places) {
      return this.MovePointRight((EInteger)places, null);
    }

    /// <summary>Returns a number similar to this number but with the
    /// decimal point moved to the right.</summary>
    /// <param name='places'>The number of decimal places to move the
    /// decimal point to the right. If this number is negative, instead
    /// moves the decimal point to the left by this number's absolute
    /// value.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed.</param>
    /// <returns>A number whose exponent is increased by <paramref
    /// name='places'/>, but not to more than 0.</returns>
    public EDecimal MovePointRight(int places, EContext ctx) {
      return this.MovePointRight((EInteger)places, ctx);
    }

    /// <summary>Returns a number similar to this number but with the
    /// decimal point moved to the right.</summary>
    /// <param name='bigPlaces'>The number of decimal places to move the
    /// decimal point to the right. If this number is negative, instead
    /// moves the decimal point to the left by this number's absolute
    /// value.</param>
    /// <returns>A number whose exponent is increased by <paramref
    /// name='bigPlaces'/>, but not to more than 0.</returns>
    public EDecimal MovePointRight(EInteger bigPlaces) {
      return this.MovePointRight(bigPlaces, null);
    }

    /// <summary>Returns a number similar to this number but with the
    /// decimal point moved to the right.</summary>
    /// <param name='bigPlaces'>The number of decimal places to move the
    /// decimal point to the right. If this number is negative, instead
    /// moves the decimal point to the left by this number's absolute
    /// value.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed.</param>
    /// <returns>A number whose exponent is increased by <paramref
    /// name='bigPlaces'/>, but not to more than 0.</returns>
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

    /// <summary>Multiplies this arbitrary-precision decimal floating-point
    /// number by another arbitrary-precision decimal floating-point number
    /// and returns the result. The exponent for the result is this
    /// arbitrary-precision decimal floating-point number's exponent plus
    /// the other arbitrary-precision decimal floating-point number's
    /// exponent.</summary>
    /// <param name='otherValue'>Another decimal number.</param>
    /// <returns>The product of the two numbers, that is, this
    /// arbitrary-precision decimal floating-point number times another
    /// arbitrary-precision decimal floating-point number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='otherValue'/> is null.</exception>
    public EDecimal Multiply(EDecimal otherValue) {
      if (otherValue == null) {
        throw new ArgumentNullException(nameof(otherValue));
      }
      if (this.IsFinite && otherValue.IsFinite) {
        int newflags = otherValue.flags ^ this.flags;
        if (this.unsignedMantissa.CanFitInInt32() &&
          otherValue.unsignedMantissa.CanFitInInt32()) {
          int integerA = this.unsignedMantissa.ToInt32();
          int integerB = otherValue.unsignedMantissa.ToInt32();
          long longA = ((long)integerA) * ((long)integerB);
          FastIntegerFixed exp = FastIntegerFixed.Add(
              this.exponent,
              otherValue.exponent);
          if ((longA >> 31) == 0) {
            return new EDecimal(
                FastIntegerFixed.FromInt32((int)longA),
                exp,
                (byte)newflags);
          } else {
            return new EDecimal(
                FastIntegerFixed.FromBig((EInteger)longA),
                exp,
                (byte)newflags);
          }
        } else {
          EInteger eintA = this.unsignedMantissa.ToEInteger().Multiply(
              otherValue.unsignedMantissa.ToEInteger());
          return new EDecimal(
              FastIntegerFixed.FromBig(eintA),
              FastIntegerFixed.Add(this.exponent, otherValue.exponent),
              (byte)newflags);
        }
      }
      return this.Multiply(otherValue, EContext.UnlimitedHalfEven);
    }

    /// <summary>Multiplies this arbitrary-precision decimal floating-point
    /// number by another arbitrary-precision decimal floating-point number
    /// and returns the result.</summary>
    /// <param name='op'>Another decimal number.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and rounding
    /// isn't needed.</param>
    /// <returns>The product of the two numbers, that is, this
    /// arbitrary-precision decimal floating-point number times another
    /// arbitrary-precision decimal floating-point number.</returns>
    public EDecimal Multiply(EDecimal op, EContext ctx) {
      return GetMathValue(ctx).Multiply(this, op, ctx);
    }

    /// <summary>Adds this arbitrary-precision decimal floating-point
    /// number and a 64-bit signed integer and returns the result. The
    /// exponent for the result is the lower of this arbitrary-precision
    /// decimal floating-point number's exponent and the other 64-bit
    /// signed integer's exponent.</summary>
    /// <param name='longValue'>The parameter <paramref name='longValue'/>
    /// is a 64-bit signed integer.</param>
    /// <returns>The sum of the two numbers, that is, this
    /// arbitrary-precision decimal floating-point number plus a 64-bit
    /// signed integer. If this arbitrary-precision decimal floating-point
    /// number is not-a-number (NaN), returns NaN.</returns>
    public EDecimal Add(long longValue) {
      return this.Add(EDecimal.FromInt64(longValue));
    }

    /// <summary>Subtracts a 64-bit signed integer from this
    /// arbitrary-precision decimal floating-point number and returns the
    /// result. The exponent for the result is the lower of this
    /// arbitrary-precision decimal floating-point number's exponent and
    /// the other 64-bit signed integer's exponent.</summary>
    /// <param name='longValue'>The parameter <paramref name='longValue'/>
    /// is a 64-bit signed integer.</param>
    /// <returns>The difference between the two numbers, that is, this
    /// arbitrary-precision decimal floating-point number minus a 64-bit
    /// signed integer. If this arbitrary-precision decimal floating-point
    /// number is not-a-number (NaN), returns NaN.</returns>
    public EDecimal Subtract(long longValue) {
      return this.Subtract(EDecimal.FromInt64(longValue));
    }

    /// <summary>Multiplies this arbitrary-precision decimal floating-point
    /// number by a 64-bit signed integer and returns the result. The
    /// exponent for the result is this arbitrary-precision decimal
    /// floating-point number's exponent plus the other 64-bit signed
    /// integer's exponent.</summary>
    /// <param name='longValue'>The parameter <paramref name='longValue'/>
    /// is a 64-bit signed integer.</param>
    /// <returns>The product of the two numbers, that is, this
    /// arbitrary-precision decimal floating-point number times a 64-bit
    /// signed integer.</returns>
    public EDecimal Multiply(long longValue) {
      return this.Multiply(EDecimal.FromInt64(longValue));
    }

    /// <summary>Divides this arbitrary-precision decimal floating-point
    /// number by a 64-bit signed integer and returns the result; returns
    /// NaN instead if the result would have a nonterminating decimal
    /// expansion (including 1/3, 1/12, 1/7, 2/3, and so on); if this is
    /// not desired, use DivideToExponent, or use the Divide overload that
    /// takes an EContext.</summary>
    /// <param name='longValue'>The parameter <paramref name='longValue'/>
    /// is a 64-bit signed integer.</param>
    /// <returns>The result of dividing this arbitrary-precision decimal
    /// floating-point number by a 64-bit signed integer. Returns infinity
    /// if the divisor (this arbitrary-precision decimal floating-point
    /// number) is 0 and the dividend (the other 64-bit signed integer) is
    /// nonzero. Returns not-a-number (NaN) if the divisor and the dividend
    /// are 0. Returns NaN if the result can't be exact because it would
    /// have a nonterminating binary expansion (examples include 1 divided
    /// by any multiple of 3, such as 1/3 or 1/12). If this is not desired,
    /// use DivideToExponent instead, or use the Divide overload that takes
    /// an <c>EContext</c> (such as <c>EContext.Decimal128</c> )
    /// instead.</returns>
    public EDecimal Divide(long longValue) {
      return this.Divide(EDecimal.FromInt64(longValue));
    }

    /// <summary>Adds this arbitrary-precision decimal floating-point
    /// number and a 32-bit signed integer and returns the result. The
    /// exponent for the result is the lower of this arbitrary-precision
    /// decimal floating-point number's exponent and the other 32-bit
    /// signed integer's exponent.</summary>
    /// <param name='intValue'>A 32-bit signed integer to add to this
    /// object.</param>
    /// <returns>The sum of the two numbers, that is, this
    /// arbitrary-precision decimal floating-point number plus a 32-bit
    /// signed integer. If this arbitrary-precision decimal floating-point
    /// number is not-a-number (NaN), returns NaN.</returns>
    public EDecimal Add(int intValue) {
      return this.Add(EDecimal.FromInt32(intValue));
    }

    /// <summary>Subtracts a 32-bit signed integer from this
    /// arbitrary-precision decimal floating-point number and returns the
    /// result. The exponent for the result is the lower of this
    /// arbitrary-precision decimal floating-point number's exponent and
    /// the other 32-bit signed integer's exponent.</summary>
    /// <param name='intValue'>A 32-bit signed integer to subtract from
    /// this object.</param>
    /// <returns>The difference between the two numbers, that is, this
    /// arbitrary-precision decimal floating-point number minus a 32-bit
    /// signed integer. If this arbitrary-precision decimal floating-point
    /// number is not-a-number (NaN), returns NaN.</returns>
    public EDecimal Subtract(int intValue) {
      return (intValue == Int32.MinValue) ?
        this.Subtract(EDecimal.FromInt32(intValue)) : this.Add(-intValue);
    }

    /// <summary>Multiplies this arbitrary-precision decimal floating-point
    /// number by a 32-bit signed integer and returns the result. The
    /// exponent for the result is this arbitrary-precision decimal
    /// floating-point number's exponent plus the other 32-bit signed
    /// integer's exponent.</summary>
    /// <param name='intValue'>A 32-bit signed integer to multiply this
    /// object by.</param>
    /// <returns>The product of the two numbers, that is, this
    /// arbitrary-precision decimal floating-point number times a 32-bit
    /// signed integer.</returns>
    public EDecimal Multiply(int intValue) {
      return this.Multiply(EDecimal.FromInt32(intValue));
    }

    /// <summary>Divides this arbitrary-precision decimal floating-point
    /// number by a 32-bit signed integer and returns the result; returns
    /// NaN instead if the result would have a nonterminating decimal
    /// expansion (including 1/3, 1/12, 1/7, 2/3, and so on); if this is
    /// not desired, use DivideToExponent, or use the Divide overload that
    /// takes an EContext.</summary>
    /// <param name='intValue'>A 32-bit signed integer, the divisor, to
    /// divide this object by.</param>
    /// <returns>The result of dividing this arbitrary-precision decimal
    /// floating-point number by a 32-bit signed integer. Returns infinity
    /// if the divisor (this arbitrary-precision decimal floating-point
    /// number) is 0 and the dividend (the other 32-bit signed integer) is
    /// nonzero. Returns not-a-number (NaN) if the divisor and the dividend
    /// are 0. Returns NaN if the result can't be exact because it would
    /// have a nonterminating binary expansion (examples include 1 divided
    /// by any multiple of 3, such as 1/3 or 1/12). If this is not desired,
    /// use DivideToExponent instead, or use the Divide overload that takes
    /// an <c>EContext</c> (such as <c>EContext.Decimal128</c> )
    /// instead.</returns>
    public EDecimal Divide(int intValue) {
      return this.Divide(EDecimal.FromInt32(intValue));
    }

    /// <summary>Multiplies by one decimal number, and then adds another
    /// decimal number.</summary>
    /// <param name='multiplicand'>The value to multiply.</param>
    /// <param name='augend'>The value to add.</param>
    /// <returns>An arbitrary-precision decimal floating-point
    /// number.</returns>
    public EDecimal MultiplyAndAdd(
      EDecimal multiplicand,
      EDecimal augend) {
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
    public EDecimal MultiplyAndAdd(
      EDecimal op,
      EDecimal augend,
      EContext ctx) {
      return GetMathValue(ctx).MultiplyAndAdd(this, op, augend, ctx);
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

    /// <summary>Gets an object with the same value as this one, but with
    /// the sign reversed.</summary>
    /// <returns>An arbitrary-precision decimal number. If this value is
    /// positive zero, returns negative zero. Returns signaling NaN if this
    /// value is signaling NaN. (In this sense, this method is similar to
    /// the "copy-negate" operation in the General Decimal Arithmetic
    /// Specification, except this method does not necessarily return a
    /// copy of this object.).</returns>
    public EDecimal Negate() {
      return new EDecimal(
          this.unsignedMantissa,
          this.exponent,
          (byte)(this.flags ^ BigNumberFlags.FlagNegative));
    }

    /// <summary>Returns an arbitrary-precision decimal number with the
    /// same value as this object but with the sign reversed.</summary>
    /// <param name='context'>An arithmetic context to control the
    /// precision, rounding, and exponent range of the result. If
    /// <c>HasFlags</c> of the context is true, will also store the flags
    /// resulting from the operation (the flags are in addition to the
    /// pre-existing flags). Can be null, in which case the precision is
    /// unlimited and rounding isn't needed.</param>
    /// <returns>An arbitrary-precision decimal number. If this value is
    /// positive zero, returns positive zero. Signals FlagInvalid and
    /// returns quiet NaN if this value is signaling NaN.</returns>
    public EDecimal Negate(EContext context) {
      return ((context == null || context == EContext.UnlimitedHalfEven) ?
          ExtendedMathValue : MathValue).Negate(this, context);
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
    public EDecimal NextMinus(EContext ctx) {
      return GetMathValue(ctx).NextMinus(this, ctx);
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
    public EDecimal NextPlus(EContext ctx) {
      return GetMathValue(ctx).NextPlus(this, ctx);
    }

    /// <summary>Finds the next value that is closer to the other object's
    /// value than this object's value. Returns a copy of this value with
    /// the same sign as the other value if both values are
    /// equal.</summary>
    /// <param name='otherValue'>An arbitrary-precision decimal number that
    /// the return value will approach.</param>
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
    public EDecimal NextToward(
      EDecimal otherValue,
      EContext ctx) {
      return GetMathValue(ctx)
        .NextToward(this, otherValue, ctx);
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
    public EDecimal Plus(EContext ctx) {
      return GetMathValue(ctx).Plus(this, ctx);
    }

    /// <summary>Raises this object's value to the given
    /// exponent.</summary>
    /// <param name='exponent'>An arbitrary-precision decimal number
    /// expressing the exponent to raise this object's value to.</param>
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
    public EDecimal Pow(EDecimal exponent, EContext ctx) {
      return GetMathValue(ctx).Power(this, exponent, ctx);
    }

    /// <summary>Raises this object's value to the given exponent, using
    /// unlimited precision.</summary>
    /// <param name='exponent'>An arbitrary-precision decimal number
    /// expressing the exponent to raise this object's value to.</param>
    /// <returns>This^exponent. Returns not-a-number (NaN) if the exponent
    /// has a fractional part.</returns>
    public EDecimal Pow(EDecimal exponent) {
      return this.Pow(exponent, null);
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
    public EDecimal Pow(int exponentSmall, EContext ctx) {
      return this.Pow(EDecimal.FromInt64(exponentSmall), ctx);
    }

    /// <summary>Raises this object's value to the given
    /// exponent.</summary>
    /// <param name='exponentSmall'>The exponent to raise this object's
    /// value to.</param>
    /// <returns>This^exponent. Returns not-a-number (NaN) if this object
    /// and exponent are both 0.</returns>
    public EDecimal Pow(int exponentSmall) {
      return this.Pow(EDecimal.FromInt64(exponentSmall), null);
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
        this.unsignedMantissa.ToEInteger().GetDigitCountAsEInteger();
    }

    /// <summary>
    ///  Returns an arbitrary-precision decimal number with the
    /// same value but a new exponent.
    /// <para>Note that this is not always the same as rounding to a given
    /// number of decimal places, since it can fail if the difference
    /// between this value's exponent and the desired exponent is too big,
    /// depending on the maximum precision. If rounding to a number of
    /// decimal places is desired, it's better to use the RoundToExponent
    /// and RoundToIntegral methods instead.</para>
    /// <para><b>Remark:</b>
    ///  This method can be used to implement
    /// fixed-point decimal arithmetic, in which each decimal number has a
    /// fixed number of digits after the decimal point. The following code
    /// example returns a fixed-point number with up to 20 digits before
    /// and exactly 5 digits after the decimal point:</para>
    /// <code> &#x2f;&#x2a; After performing arithmetic operations, adjust
    /// &#x2f;&#x2a; the number to 5&#x2a;&#x2f;&#x2a;&#x2f;
    /// &#x2f;&#x2a;&#x2a;&#x2f;
    /// digits after the decimal point number = number.Quantize(
    /// EInteger.FromInt32(-5), &#x2f;&#x2a; five digits after the decimal
    /// point&#x2a;&#x2f;
    /// EContext.ForPrecision(25) &#x2f;&#x2a; 25-digit
    /// precision);&#x2a;&#x2f;</code>
    /// <para>A fixed-point decimal arithmetic in which no digits come
    /// after the decimal point (a desired exponent of 0) is considered an
    /// "integer arithmetic".</para>
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
    /// <returns>An arbitrary-precision decimal number with the same value
    /// as this object but with the exponent changed. Signals FlagInvalid
    /// and returns not-a-number (NaN) if this object is infinity, if the
    /// rounded result can't fit the given precision, or if the context
    /// defines an exponent range and the given exponent is outside that
    /// range.</returns>
    public EDecimal Quantize(
      EInteger desiredExponent,
      EContext ctx) {
      return this.Quantize(
          EDecimal.Create(EInteger.One, desiredExponent),
          ctx);
    }

    /// <summary>Returns an arbitrary-precision decimal number with the
    /// same value as this one but a new exponent.
    /// <para><b>Remark:</b> This method can be used to implement
    /// fixed-point decimal arithmetic, in which a fixed number of digits
    /// come after the decimal point. A fixed-point decimal arithmetic in
    /// which no digits come after the decimal point (a desired exponent of
    /// 0) is considered an "integer arithmetic" .</para></summary>
    /// <param name='desiredExponentInt'>The desired exponent for the
    /// result. The exponent is the number of fractional digits in the
    /// result, expressed as a negative number. Can also be positive, which
    /// eliminates lower-order places from the number. For example, -3
    /// means round to the thousandth (10^-3, 0.0001), and 3 means round to
    /// the thousand (10^3, 1000). A value of 0 rounds the number to an
    /// integer.</param>
    /// <param name='rounding'>A rounding mode to use in case the result
    /// needs to be rounded to fit the given exponent.</param>
    /// <returns>An arbitrary-precision decimal number with the same value
    /// as this object but with the exponent changed. Returns not-a-number
    /// (NaN) if this object is infinity, or if the rounding mode is
    /// ERounding.None and the result is not exact.</returns>
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

    /// <summary>
    ///  Returns an arbitrary-precision decimal number with the
    /// same value but a new exponent.
    /// <para>Note that this is not always the same as rounding to a given
    /// number of decimal places, since it can fail if the difference
    /// between this value's exponent and the desired exponent is too big,
    /// depending on the maximum precision. If rounding to a number of
    /// decimal places is desired, it's better to use the RoundToExponent
    /// and RoundToIntegral methods instead.</para>
    /// <para><b>Remark:</b>
    ///  This method can be used to implement
    /// fixed-point decimal arithmetic, in which each decimal number has a
    /// fixed number of digits after the decimal point. The following code
    /// example returns a fixed-point number with up to 20 digits before
    /// and exactly 5 digits after the decimal point:</para>
    /// <code>/* After performing arithmetic operations, adjust the number to 5
    /// digits
    /// after the decimal point */ number = number.Quantize(-5, /* five digits
    /// after the decimal point */EContext.ForPrecision(25) /* 25-digit
    /// precision*/);</code>
    /// <para>A fixed-point decimal arithmetic in which no digits come
    /// after the decimal point (a desired exponent of 0) is considered an
    /// "integer arithmetic".</para>
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
    /// <returns>An arbitrary-precision decimal number with the same value
    /// as this object but with the exponent changed. Signals FlagInvalid
    /// and returns not-a-number (NaN) if this object is infinity, if the
    /// rounded result can't fit the given precision, or if the context
    /// defines an exponent range and the given exponent is outside that
    /// range.</returns>
    public EDecimal Quantize(
      int desiredExponentInt,
      EContext ctx) {
      if (ctx == null ||
        (!ctx.HasExponentRange && !ctx.HasFlagsOrTraps &&
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

    /// <summary>Returns an arbitrary-precision decimal number with the
    /// same value as this object but with the same exponent as another
    /// decimal number.
    /// <para>Note that this is not always the same as rounding to a given
    /// number of decimal places, since it can fail if the difference
    /// between this value's exponent and the desired exponent is too big,
    /// depending on the maximum precision. If rounding to a number of
    /// decimal places is desired, it's better to use the RoundToExponent
    /// and RoundToIntegral methods instead.</para>
    /// <para><b>Remark:</b> This method can be used to implement
    /// fixed-point decimal arithmetic, in which a fixed number of digits
    /// come after the decimal point. A fixed-point decimal arithmetic in
    /// which no digits come after the decimal point (a desired exponent of
    /// 0) is considered an "integer arithmetic" .</para></summary>
    /// <param name='otherValue'>An arbitrary-precision decimal number
    /// containing the desired exponent of the result. The significand is
    /// ignored. The exponent is the number of fractional digits in the
    /// result, expressed as a negative number. Can also be positive, which
    /// eliminates lower-order places from the number. For example, -3
    /// means round to the thousandth (10^-3, 0.0001), and 3 means round to
    /// the thousands-place (10^3, 1000). A value of 0 rounds the number to
    /// an integer. The following examples for this parameter express a
    /// desired exponent of 3: <c>10e3</c>, <c>8888e3</c>, <c>4.56e5</c>.</param>
    /// <param name='ctx'>An arithmetic context to control precision and
    /// rounding of the result. If <c>HasFlags</c> of the context is true,
    /// will also store the flags resulting from the operation (the flags
    /// are in addition to the pre-existing flags). Can be null, in which
    /// case the default rounding mode is HalfEven.</param>
    /// <returns>An arbitrary-precision decimal number with the same value
    /// as this object but with the exponent changed. Signals FlagInvalid
    /// and returns not-a-number (NaN) if the result can't fit the given
    /// precision without rounding, or if the arithmetic context defines an
    /// exponent range and the given exponent is outside that
    /// range.</returns>
    public EDecimal Quantize(
      EDecimal otherValue,
      EContext ctx) {
      return GetMathValue(ctx).Quantize(this, otherValue, ctx);
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
    public EDecimal Reduce(EContext ctx) {
      return GetMathValue(ctx).Reduce(this, ctx);
    }

    /// <summary>Returns the remainder that would result when this
    /// arbitrary-precision decimal floating-point number is divided by
    /// another arbitrary-precision decimal floating-point number. The
    /// remainder is the number that remains when the absolute value of
    /// this arbitrary-precision decimal floating-point number is divided
    /// (as though by DivideToIntegerZeroScale) by the absolute value of
    /// the other arbitrary-precision decimal floating-point number; the
    /// remainder has the same sign (positive or negative) as this
    /// arbitrary-precision decimal floating-point number.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='ctx'>An arithmetic context object to control the
    /// precision, rounding, and exponent range of the result, and of the
    /// intermediate integer division. If <c>HasFlags</c> of the context is
    /// true, will also store the flags resulting from the operation (the
    /// flags are in addition to the pre-existing flags). Can be null, in
    /// which the precision is unlimited.</param>
    /// <returns>The remainder that would result when this
    /// arbitrary-precision decimal floating-point number is divided by
    /// another arbitrary-precision decimal floating-point number. Signals
    /// FlagDivideByZero and returns infinity if the divisor (this
    /// arbitrary-precision decimal floating-point number) is 0 and the
    /// dividend (the other arbitrary-precision decimal floating-point
    /// number) is nonzero. Signals FlagInvalid and returns not-a-number
    /// (NaN) if the divisor and the dividend are 0, or if the result of
    /// the division doesn't fit the given precision.</returns>
    public EDecimal Remainder(
      EDecimal divisor,
      EContext ctx) {
      return GetMathValue(ctx).Remainder(this, divisor, ctx, true);
    }

    /// <summary>Finds the remainder that results when dividing two
    /// arbitrary-precision decimal numbers, except the intermediate
    /// division is not adjusted to fit the precision of the given
    /// arithmetic context. The value of this object is divided by the
    /// absolute value of the other object; the remainder has the same sign
    /// (positive or negative) as this object's value.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <param name='ctx'>An arithmetic context object to control the
    /// precision, rounding, and exponent range of the result, but not also
    /// of the intermediate integer division. If <c>HasFlags</c> of the
    /// context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which the precision is unlimited.</param>
    /// <returns>The remainder of the two numbers. Signals FlagInvalid and
    /// returns not-a-number (NaN) if the divisor is 0, or if the result
    /// doesn't fit the given precision.</returns>
    public EDecimal RemainderNoRoundAfterDivide(
      EDecimal divisor,
      EContext ctx) {
      return GetMathValue(ctx).Remainder(this, divisor, ctx, false);
    }

    /// <summary>Calculates the remainder of a number by the formula
    /// <c>"this" - (("this" / "divisor") * "divisor")</c>.</summary>
    /// <param name='divisor'>The number to divide by.</param>
    /// <returns>An arbitrary-precision decimal number.</returns>
    public EDecimal RemainderNaturalScale(EDecimal divisor) {
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
    /// <returns>An arbitrary-precision decimal number.</returns>
    public EDecimal RemainderNaturalScale(
      EDecimal divisor,
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
    public EDecimal RemainderNear(
      EDecimal divisor,
      EContext ctx) {
      return GetMathValue(ctx)
        .RemainderNear(this, divisor, ctx);
    }

    /// <summary>Returns an arbitrary-precision decimal number with the
    /// same value as this object but rounded to a new exponent if
    /// necessary. The resulting number's Exponent property will not
    /// necessarily be the given exponent; use the Quantize method instead
    /// to give the result a particular exponent.</summary>
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
    /// <returns>An arbitrary-precision decimal number rounded to the
    /// closest value representable in the given precision. If the result
    /// can't fit the precision, additional digits are discarded to make it
    /// fit. Signals FlagInvalid and returns not-a-number (NaN) if the
    /// arithmetic context defines an exponent range, the new exponent must
    /// be changed to the given exponent when rounding, and the given
    /// exponent is outside of the valid range of the arithmetic
    /// context.</returns>
    public EDecimal RoundToExponent(
      EInteger exponent,
      EContext ctx) {
      return GetMathValue(ctx)
        .RoundToExponentSimple(this, exponent, ctx);
    }

    /// <summary>Returns an arbitrary-precision decimal number with the
    /// same value as this object but rounded to a new exponent if
    /// necessary, using the HalfEven rounding mode. The resulting number's
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
    /// <returns>An arbitrary-precision decimal number rounded to the
    /// closest value representable for the given exponent.</returns>
    public EDecimal RoundToExponent(
      EInteger exponent) {
      return this.RoundToExponent(
          exponent,
          EContext.ForRounding(ERounding.HalfEven));
    }

    /// <summary>Returns an arbitrary-precision decimal number with the
    /// same value as this object but rounded to a new exponent if
    /// necessary, using the given rounding mode. The resulting number's
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
    /// <param name='rounding'>Desired mode for rounding this number's
    /// value.</param>
    /// <returns>An arbitrary-precision decimal number rounded to the
    /// closest value representable for the given exponent.</returns>
    public EDecimal RoundToExponent(
      EInteger exponent,
      ERounding rounding) {
      return this.RoundToExponent(
          exponent,
          EContext.ForRounding(rounding));
    }

    /// <summary>Returns an arbitrary-precision decimal number with the
    /// same value as this object but rounded to a new exponent if
    /// necessary, using the HalfEven rounding mode. The resulting number's
    /// Exponent property will not necessarily be the given exponent; use
    /// the Quantize method instead to give the result a particular
    /// exponent.</summary>
    /// <param name='exponentSmall'>The minimum exponent the result can
    /// have. This is the maximum number of fractional digits in the
    /// result, expressed as a negative number. Can also be positive, which
    /// eliminates lower-order places from the number. For example, -3
    /// means round to the thousandth (10^-3, 0.0001), and 3 means round to
    /// the thousand (10^3, 1000). A value of 0 rounds the number to an
    /// integer.</param>
    /// <returns>An arbitrary-precision decimal number rounded to the
    /// closest value representable for the given exponent.</returns>
    public EDecimal RoundToExponent(
      int exponentSmall) {
      return this.RoundToExponent(exponentSmall, ERounding.HalfEven);
    }

    /// <summary>Returns an arbitrary-precision decimal number with the
    /// same value as this object but rounded to a new exponent if
    /// necessary. The resulting number's Exponent property will not
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
    /// <returns>An arbitrary-precision decimal number rounded to the
    /// closest value representable in the given precision. If the result
    /// can't fit the precision, additional digits are discarded to make it
    /// fit. Signals FlagInvalid and returns not-a-number (NaN) if the
    /// arithmetic context defines an exponent range, the new exponent must
    /// be changed to the given exponent when rounding, and the given
    /// exponent is outside of the valid range of the arithmetic
    /// context.</returns>
    public EDecimal RoundToExponent(
      int exponentSmall,
      EContext ctx) {
      if (ctx == null ||
        (!ctx.HasExponentRange && !ctx.HasFlagsOrTraps &&
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

    /// <summary>Returns an arbitrary-precision decimal number with the
    /// same value as this object but rounded to a new exponent if
    /// necessary. The resulting number's Exponent property will not
    /// necessarily be the given exponent; use the Quantize method instead
    /// to give the result a particular exponent.</summary>
    /// <param name='exponentSmall'>The minimum exponent the result can
    /// have. This is the maximum number of fractional digits in the
    /// result, expressed as a negative number. Can also be positive, which
    /// eliminates lower-order places from the number. For example, -3
    /// means round to the thousandth (10^-3, 0.0001), and 3 means round to
    /// the thousand (10^3, 1000). A value of 0 rounds the number to an
    /// integer.</param>
    /// <param name='rounding'>The desired mode to use to round the given
    /// number to the given exponent.</param>
    /// <returns>An arbitrary-precision decimal number rounded to the given
    /// negative number of decimal places.</returns>
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

    /// <summary>Returns an arbitrary-precision decimal number with the
    /// same value as this object but rounded to the given exponent
    /// represented as an arbitrary-precision integer, and signals an
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
    /// <returns>An arbitrary-precision decimal number rounded to the
    /// closest value representable in the given precision. Signals
    /// FlagInvalid and returns not-a-number (NaN) if the result can't fit
    /// the given precision without rounding. Signals FlagInvalid and
    /// returns not-a-number (NaN) if the arithmetic context defines an
    /// exponent range, the new exponent must be changed to the given
    /// exponent when rounding, and the given exponent is outside of the
    /// valid range of the arithmetic context.</returns>
    public EDecimal RoundToExponentExact(
      EInteger exponent,
      EContext ctx) {
      return GetMathValue(ctx)
        .RoundToExponentExact(this, exponent, ctx);
    }

    /// <summary>Returns an arbitrary-precision decimal number with the
    /// same value as this object but rounded to the given exponent
    /// represented as a 32-bit signed integer, and signals an inexact flag
    /// if the result would be inexact. The resulting number's Exponent
    /// property will not necessarily be the given exponent; use the
    /// Quantize method instead to give the result a particular
    /// exponent.</summary>
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
    /// <returns>An arbitrary-precision decimal number rounded to the
    /// closest value representable in the given precision. Signals
    /// FlagInvalid and returns not-a-number (NaN) if the result can't fit
    /// the given precision without rounding. Signals FlagInvalid and
    /// returns not-a-number (NaN) if the arithmetic context defines an
    /// exponent range, the new exponent must be changed to the given
    /// exponent when rounding, and the given exponent is outside of the
    /// valid range of the arithmetic context.</returns>
    public EDecimal RoundToExponentExact(
      int exponentSmall,
      EContext ctx) {
      return this.RoundToExponentExact((EInteger)exponentSmall, ctx);
    }

    /// <summary>Returns an arbitrary-precision decimal number with the
    /// same value as this object but rounded to the given exponent
    /// represented as a 32-bit signed integer, and signals an inexact flag
    /// if the result would be inexact. The resulting number's Exponent
    /// property will not necessarily be the given exponent; use the
    /// Quantize method instead to give the result a particular
    /// exponent.</summary>
    /// <param name='exponentSmall'>The minimum exponent the result can
    /// have. This is the maximum number of fractional digits in the
    /// result, expressed as a negative number. Can also be positive, which
    /// eliminates lower-order places from the number. For example, -3
    /// means round to the thousandth (10^-3, 0.0001), and 3 means round to
    /// the thousand (10^3, 1000). A value of 0 rounds the number to an
    /// integer.</param>
    /// <param name='rounding'>Desired mode for rounding this object's
    /// value.</param>
    /// <returns>An arbitrary-precision decimal number rounded to the
    /// closest value representable using the given exponent.</returns>
    public EDecimal RoundToExponentExact(
      int exponentSmall,
      ERounding rounding) {
      return this.RoundToExponentExact(
          (EInteger)exponentSmall,
          EContext.Unlimited.WithRounding(rounding));
    }

    /// <summary>Returns an arbitrary-precision decimal number with the
    /// same value as this object but rounded to an integer, and signals an
    /// inexact flag if the result would be inexact. The resulting number's
    /// Exponent property will not necessarily be 0; use the Quantize
    /// method instead to give the result an exponent of 0.</summary>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the default rounding mode is
    /// HalfEven.</param>
    /// <returns>An arbitrary-precision decimal number rounded to the
    /// closest integer representable in the given precision. Signals
    /// FlagInvalid and returns not-a-number (NaN) if the result can't fit
    /// the given precision without rounding. Signals FlagInvalid and
    /// returns not-a-number (NaN) if the arithmetic context defines an
    /// exponent range, the new exponent must be changed to 0 when
    /// rounding, and 0 is outside of the valid range of the arithmetic
    /// context.</returns>
    public EDecimal RoundToIntegerExact(EContext ctx) {
      return GetMathValue(ctx).RoundToExponentExact(this, EInteger.Zero, ctx);
    }

    /// <summary>Returns an arbitrary-precision decimal number with the
    /// same value as this object but rounded to an integer, without adding
    /// the <c>FlagInexact</c> or <c>FlagRounded</c> flags. The resulting
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
    /// <returns>An arbitrary-precision decimal number rounded to the
    /// closest integer representable in the given precision. If the result
    /// can't fit the precision, additional digits are discarded to make it
    /// fit. Signals FlagInvalid and returns not-a-number (NaN) if the
    /// arithmetic context defines an exponent range, the new exponent must
    /// be changed to 0 when rounding, and 0 is outside of the valid range
    /// of the arithmetic context.</returns>
    public EDecimal RoundToIntegerNoRoundedFlag(EContext ctx) {
      return GetMathValue(ctx)
        .RoundToExponentNoRoundedFlag(this, EInteger.Zero, ctx);
    }

    /// <summary>Returns an arbitrary-precision decimal number with the
    /// same value as this object but rounded to an integer, and signals an
    /// inexact flag if the result would be inexact.</summary>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the default rounding mode is
    /// HalfEven.</param>
    /// <returns>An arbitrary-precision decimal number rounded to the
    /// closest integer representable in the given precision. Signals
    /// FlagInvalid and returns not-a-number (NaN) if the result can't fit
    /// the given precision without rounding. Signals FlagInvalid and
    /// returns not-a-number (NaN) if the arithmetic context defines an
    /// exponent range, the new exponent must be changed to 0 when
    /// rounding, and 0 is outside of the valid range of the arithmetic
    /// context.</returns>
    [Obsolete("Renamed to RoundToIntegerExact.")]
    public EDecimal RoundToIntegralExact(EContext ctx) {
      return GetMathValue(ctx).RoundToExponentExact(this, EInteger.Zero, ctx);
    }

    /// <summary>Returns an arbitrary-precision decimal number with the
    /// same value as this object but rounded to an integer, without adding
    /// the <c>FlagInexact</c> or <c>FlagRounded</c> flags.</summary>
    /// <param name='ctx'>An arithmetic context to control precision and
    /// rounding of the result. If <c>HasFlags</c> of the context is true,
    /// will also store the flags resulting from the operation (the flags
    /// are in addition to the pre-existing flags), except that this
    /// function will never add the <c>FlagRounded</c> and
    /// <c>FlagInexact</c> flags (the only difference between this and
    /// RoundToExponentExact). Can be null, in which case the default
    /// rounding mode is HalfEven.</param>
    /// <returns>An arbitrary-precision decimal number rounded to the
    /// closest integer representable in the given precision. If the result
    /// can't fit the precision, additional digits are discarded to make it
    /// fit. Signals FlagInvalid and returns not-a-number (NaN) if the
    /// arithmetic context defines an exponent range, the new exponent must
    /// be changed to 0 when rounding, and 0 is outside of the valid range
    /// of the arithmetic context.</returns>
    [Obsolete("Renamed to RoundToIntegerNoRoundedFlag.")]
    public EDecimal RoundToIntegralNoRoundedFlag(EContext ctx) {
      return GetMathValue(ctx)
        .RoundToExponentNoRoundedFlag(this, EInteger.Zero, ctx);
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
    public EDecimal RoundToPrecision(EContext ctx) {
      return GetMathValue(ctx).RoundToPrecision(this, ctx);
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
    public EDecimal PreRound(EContext ctx) {
      return NumberUtility.PreRound(this, ctx, GetMathValue(ctx));
    }

    /// <summary>Returns a number similar to this number but with the scale
    /// adjusted.</summary>
    /// <param name='places'>The power of 10 to scale by.</param>
    /// <returns>A number whose exponent is increased by <paramref
    /// name='places'/>. For example, if <paramref name='places'/> is 5,
    /// "78E-2" becomes "78E+3" and has a bigger value.</returns>
    public EDecimal ScaleByPowerOfTen(int places) {
      return this.ScaleByPowerOfTen((EInteger)places, null);
    }

    /// <summary>Returns a number similar to this number but with the scale
    /// adjusted.</summary>
    /// <param name='places'>The power of 10 to scale by.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and no
    /// rounding is needed.</param>
    /// <returns>A number whose exponent is generally increased by
    /// <paramref name='places'/>. For example, in general, if <paramref
    /// name='places'/> is 5, "78E-2" becomes "78E+3" and has a bigger
    /// value.</returns>
    public EDecimal ScaleByPowerOfTen(int places, EContext ctx) {
      return this.ScaleByPowerOfTen((EInteger)places, ctx);
    }

    /// <summary>Returns a number similar to this number but with the scale
    /// adjusted.</summary>
    /// <param name='bigPlaces'>The power of 10 to scale by.</param>
    /// <returns>A number whose exponent is increased by <paramref
    /// name='bigPlaces'/>. For example, if <paramref name='bigPlaces'/>
    /// is 5, "78E-2" becomes "78E+3" and has a bigger value.</returns>
    public EDecimal ScaleByPowerOfTen(EInteger bigPlaces) {
      return this.ScaleByPowerOfTen(bigPlaces, null);
    }

    /// <summary>Returns a number similar to this number but with its scale
    /// adjusted.</summary>
    /// <param name='bigPlaces'>The power of 10 to scale by.</param>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// Can be null, in which case the precision is unlimited and no
    /// rounding is needed.</param>
    /// <returns>A number whose exponent is generally increased by
    /// <paramref name='bigPlaces'/>. For example, in general, if
    /// <paramref name='bigPlaces'/> is 5, "78E-2" becomes "78E+3" and has
    /// a bigger value.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bigPlaces'/> is null.</exception>
    public EDecimal ScaleByPowerOfTen(
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
      return CreateWithFlags(
          this.unsignedMantissa,
          FastIntegerFixed.FromBig(bigExp),
          this.flags).RoundToPrecision(ctx);
    }

    /// <summary>Finds the square root of this object's value.</summary>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// <i>This parameter can't be null, as the square root function's
    /// results are generally not exact for many inputs.</i> (Unlike in the
    /// General Decimal Arithmetic Specification, any rounding mode is
    /// allowed.).</param>
    /// <returns>The square root. Signals the flag FlagInvalid and returns
    /// NaN if this object is less than 0 (the square root would be a
    /// complex number, but the return value is still NaN). Signals
    /// FlagInvalid and returns not-a-number (NaN) if the parameter
    /// <paramref name='ctx'/> is null or the precision is unlimited (the
    /// context's Precision property is 0).</returns>
    public EDecimal Sqrt(EContext ctx) {
      return GetMathValue(ctx).SquareRoot(this, ctx);
    }

    /// <summary>Finds the square root of this object's value.</summary>
    /// <param name='ctx'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. If <c>HasFlags</c> of
    /// the context is true, will also store the flags resulting from the
    /// operation (the flags are in addition to the pre-existing flags).
    /// <i>This parameter can't be null, as the square root function's
    /// results are generally not exact for many inputs.</i> (Unlike in the
    /// General Decimal Arithmetic Specification, any rounding mode is
    /// allowed.).</param>
    /// <returns>The square root. Signals the flag FlagInvalid and returns
    /// NaN if this object is less than 0 (the square root would be a
    /// complex number, but the return value is still NaN). Signals
    /// FlagInvalid and returns not-a-number (NaN) if the parameter
    /// <paramref name='ctx'/> is null or the precision is unlimited (the
    /// context's Precision property is 0).</returns>
    [Obsolete("Renamed to Sqrt.")]
    public EDecimal SquareRoot(EContext ctx) {
      return GetMathValue(ctx).SquareRoot(this, ctx);
    }

    /// <summary>Subtracts an arbitrary-precision decimal floating-point
    /// number from this arbitrary-precision decimal floating-point number
    /// and returns the result. The exponent for the result is the lower of
    /// this arbitrary-precision decimal floating-point number's exponent
    /// and the other arbitrary-precision decimal floating-point number's
    /// exponent.</summary>
    /// <param name='otherValue'>The number to subtract from this
    /// instance's value.</param>
    /// <returns>The difference between the two numbers, that is, this
    /// arbitrary-precision decimal floating-point number minus another
    /// arbitrary-precision decimal floating-point number. If this
    /// arbitrary-precision decimal floating-point number is not-a-number
    /// (NaN), returns NaN.</returns>
    public EDecimal Subtract(EDecimal otherValue) {
      return this.Subtract(otherValue, EContext.UnlimitedHalfEven);
    }

    /// <summary>Subtracts an arbitrary-precision decimal floating-point
    /// number from this arbitrary-precision decimal floating-point number
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
    /// arbitrary-precision decimal floating-point number minus another
    /// arbitrary-precision decimal floating-point number. If this
    /// arbitrary-precision decimal floating-point number is not-a-number
    /// (NaN), returns NaN.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='otherValue'/> is null.</exception>
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

    private static long IntegerToDoubleBits(long v, int expshift, bool neg) {
      int nexp = expshift;
      while (v < (1L << 52)) {
        v <<= 1;
        --nexp;
      }
      // Clear the high bits where the exponent and sign are
      v &= 0xfffffffffffffL;
      // NOTE: Assumed not to be subnormal
      v |= (long)(nexp + 1075) << 52;
      if (neg) {
        v |= unchecked((long)(1L << 63));
      }
      return v;
    }

    private static int IntegerToSingleBits(int iv, int expshift, bool neg) {
      int nexp = expshift;
      while (iv < (1 << 23)) {
        iv <<= 1;
        --nexp;
      }
      // Clear the high bits where the exponent and sign are
      iv &= 0x7fffff;
      // NOTE: Assumed not to be subnormal
      iv |= (nexp + 150) << 23;
      if (neg) {
        iv |= 1 << 31;
      }
      return iv;
    }

    /// <summary>Converts this value to its closest equivalent as a 64-bit
    /// floating-point number encoded in the IEEE 754 binary64 format,
    /// using the half-even rounding mode.
    /// <para>If this value is a NaN, sets the high bit of the binary64
    /// value's significand area for a quiet NaN, and clears it for a
    /// signaling NaN. Then the other bits of the significand area are set
    /// to the lowest bits of this object's unsigned significand, and the
    /// next-highest bit of the significand area is set if those bits are
    /// all zeros and this is a signaling NaN.</para></summary>
    /// <returns>The closest 64-bit floating-point number to this value,
    /// encoded in the IEEE 754 binary64 format. The return value can be
    /// positive infinity or negative infinity, encoded in the IEEE 754
    /// binary64 format, if this value exceeds the range of a 64-bit
    /// floating point number.</returns>
    public long ToDoubleBits() {
      if (this.IsPositiveInfinity()) {
        return unchecked((long)0x7ff0000000000000L);
      }
      if (this.IsNegativeInfinity()) {
        return unchecked((long)0xfff0000000000000L);
      }
      if (this.IsNegative && this.IsZero) {
        return 1L << 63;
      }
      if (this.IsZero) {
        return 0;
      }
      if (this.IsFinite) {
        if (this.exponent.CompareToInt(0) == 0 &&
          this.unsignedMantissa.CanFitInInt64()) {
          long v = this.unsignedMantissa.ToInt64();
          if (v < (1L << 53)) {
            // This integer fits exactly in double
            return IntegerToDoubleBits(v, 0, this.IsNegative);
          }
        }
        if (this.exponent.CompareToInt(0) < 0 &&
          this.exponent.CompareToInt(-8) >= 0 &&
          this.unsignedMantissa.CanFitInInt32()) {
          int m = this.unsignedMantissa.ToInt32();
          int iex = -this.exponent.ToInt32();
          int vtp = ValueTenPowers[iex];
          if (m != Int32.MinValue) {
            if (m % vtp == 0) {
              // Will fit in double without rounding
              // DebugUtility.Log("m=" + m + " vtp=" + vtp);
              return IntegerToDoubleBits(m / vtp, 0, this.IsNegative);
            }
            // Shift significand to be a 53-bit number (which
            // can fit exactly in a double)
            long am = Math.Abs(m);
            var expshift = 0;
            while (am < (1 << 52)) {
              am <<= 1;
              --expshift;
            }
            int divdCount = NumberUtility.BitLength(m);
            int divsCount = NumberUtility.BitLength(vtp);
            int dividendShift = (divdCount <= divsCount) ? ((divsCount -
                  divdCount) + 53 + 1) : Math.Max(0,
                (53 + 1) - (divdCount - divsCount));
            long lquo = -1;
            long lrem = -1;
            if (divsCount + dividendShift > 63) {
              EInteger eim = EInteger.FromInt32(m).ShiftLeft(dividendShift);
              EInteger[] divrem3 = eim.DivRem(EInteger.FromInt32(vtp));
              EInteger equo = divrem3[0];
              EInteger erem = divrem3[1];
              if (equo.CanFitInInt64() && erem.CanFitInInt64()) {
                lquo = equo.ToInt64Checked();
                lrem = erem.ToInt64Checked();
              }
            } else {
              long ldividend = ((long)m) << dividendShift;
              lquo = ldividend / vtp;
              lrem = ldividend - (lquo * vtp);
            }
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
              // Clear the high bits where the exponent and sign are
              lquo &= 0xfffffffffffffL;
              // NOTE: Assumed not to be subnormal
              lquo |= (long)(nexp + 1075) << 52;
              if (this.IsNegative) {
                lquo |= unchecked((long)(1L << 63));
              }
              return lquo;
            }
          }
        }
        if (this.exponent.CompareToInt(309) > 0) {
          // Very high exponent, treat as infinity
          return this.IsNegative ? unchecked((long)0xfff0000000000000L) :
            0x7ff0000000000000L;
        }
      }
      return this.ToEFloat(EContext.Binary64).ToDoubleBits();
    }

    /// <summary>Converts this value to its closest equivalent as a 64-bit
    /// floating-point number, using the half-even rounding mode.
    /// <para>If this value is a NaN, sets the high bit of the 64-bit
    /// floating point number's significand area for a quiet NaN, and
    /// clears it for a signaling NaN. Then the other bits of the
    /// significand area are set to the lowest bits of this object's
    /// unsigned significand, and the next-highest bit of the significand
    /// area is set if those bits are all zeros and this is a signaling
    /// NaN. Unfortunately, in the.NET implementation, the return value of
    /// this method may be a quiet NaN even if a signaling NaN would
    /// otherwise be generated.</para></summary>
    /// <returns>The closest 64-bit floating-point number to this value.
    /// The return value can be positive infinity or negative infinity if
    /// this value exceeds the range of a 64-bit floating point
    /// number.</returns>
    public double ToDouble() {
      long value = this.ToDoubleBits();
      return BitConverter.ToDouble(BitConverter.GetBytes((long)value), 0);
    }

    /// <summary>Converts this value to an arbitrary-precision integer,
    /// discarding the fractional part in this value. Note that depending
    /// on the value, especially the exponent, generating the
    /// arbitrary-precision integer may require a huge amount of memory.
    /// Use the ToSizedEInteger method to convert a number to an EInteger
    /// only if the integer fits in a bounded bit range; that method will
    /// throw an exception on overflow.</summary>
    /// <returns>An arbitrary-precision integer.</returns>
    /// <exception cref='OverflowException'>This object's value is infinity
    /// or not-a-number (NaN).</exception>
    /// <exception cref='NotSupportedException'>There is not enough memory
    /// to store the value as an EInteger.</exception>
    public EInteger ToEInteger() {
      return this.ToEIntegerInternal(false);
    }

    /// <summary>Converts this value to an arbitrary-precision integer,
    /// checking whether the fractional part of the value would be lost.
    /// Note that depending on the value, especially the exponent,
    /// generating the arbitrary-precision integer may require a huge
    /// amount of memory. Use the ToSizedEIntegerIfExact method to convert
    /// a number to an EInteger only if the integer fits in a bounded bit
    /// range; that method will throw an exception on overflow.</summary>
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
    /// checking whether the fractional part of the value would be lost.
    /// Note that depending on the value, especially the exponent,
    /// generating the arbitrary-precision integer may require a huge
    /// amount of memory. Use the ToSizedEIntegerIfExact method to convert
    /// a number to an EInteger only if the integer fits in a bounded bit
    /// range; that method will throw an exception on overflow.</summary>
    /// <returns>An arbitrary-precision integer.</returns>
    /// <exception cref='OverflowException'>This object's value is infinity
    /// or not-a-number (NaN).</exception>
    /// <exception cref='ArithmeticException'>This object's value is not an
    /// exact integer.</exception>
    public EInteger ToEIntegerIfExact() {
      return this.ToEIntegerInternal(true);
    }

    /// <summary>Same as ToString(), except that when an exponent is used
    /// it will be a multiple of 3.</summary>
    /// <returns>A text string.</returns>
    public string ToEngineeringString() {
      return this.ToStringInternal(1);
    }

    /// <summary>Creates a binary floating-point number from this object's
    /// value. Note that if the binary floating-point number contains a
    /// negative exponent, the resulting value might not be exact, in which
    /// case the resulting binary floating-point number will be an
    /// approximation of this decimal number's value, using the half-even
    /// rounding mode.</summary>
    /// <returns>An arbitrary-precision binary floating-point
    /// number.</returns>
    [Obsolete("Renamed to ToEFloat.")]
    public EFloat ToExtendedFloat() {
      return this.ToEFloat(EContext.UnlimitedHalfEven);
    }

    /// <summary>Creates a binary floating-point number from this object's
    /// value. Note that if the binary floating-point number contains a
    /// negative exponent, the resulting value might not be exact, in which
    /// case the resulting binary floating-point number will be an
    /// approximation of this decimal number's value, using the half-even
    /// rounding mode.</summary>
    /// <returns>An arbitrary-precision binary floating-point
    /// number.</returns>
    public EFloat ToEFloat() {
      return this.ToEFloat(EContext.UnlimitedHalfEven);
    }

    /// <summary>Converts this value to a string as though with the
    /// ToString method, but without using exponential notation.</summary>
    /// <returns>A text string.</returns>
    public string ToPlainString() {
      return this.ToStringInternal(2);
    }

    /// <summary>Converts this value to its closest equivalent as a 32-bit
    /// floating-point number encoded in the IEEE 754 binary32 format,
    /// using the half-even rounding mode.
    /// <para>If this value is a NaN, sets the high bit of the 32-bit
    /// floating point number's significand area for a quiet NaN, and
    /// clears it for a signaling NaN. Then the other bits of the
    /// significand area are set to the lowest bits of this object's
    /// unsigned significand, and the next-highest bit of the significand
    /// area is set if those bits are all zeros and this is a signaling
    /// NaN.</para></summary>
    /// <returns>The closest 32-bit binary floating-point number to this
    /// value, encoded in the IEEE 754 binary32 format. The return value
    /// can be positive infinity or negative infinity if this value exceeds
    /// the range of a 32-bit floating point number.</returns>
    public int ToSingleBits() {
      if (this.IsPositiveInfinity()) {
        return 0x7f800000;
      }
      if (this.IsNegativeInfinity()) {
        return unchecked((int)0xff800000);
      }
      if (this.IsNegative && this.IsZero) {
        return (int)1 << 31;
      }
      if (this.IsZero) {
        return 0;
      }
      if (this.IsFinite) {
        if (this.exponent.CompareToInt(0) == 0 &&
          this.unsignedMantissa.CanFitInInt32()) {
          int v = this.unsignedMantissa.ToInt32();
          if (v < (1 << 24)) {
            // This integer fits exactly in float
            return IntegerToSingleBits(v, 0, this.IsNegative);
          }
        }
        if (this.exponent.CompareToInt(0) < 0 &&
          this.exponent.CompareToInt(-6) >= 0 &&
          this.unsignedMantissa.CanFitInInt32()) {
          int m = this.unsignedMantissa.ToInt32();
          int iex = -this.exponent.ToInt32();
          int vtp = ValueTenPowers[iex];
          if (m >= -(1 << 23) && m < (1 << 23)) {
            if (m % vtp == 0) {
              return IntegerToSingleBits(m / vtp, 0, this.IsNegative);
            }
            // Shift significand to be a 24-bit number (which
            // can fit exactly in a single)
            long am = Math.Abs(m);
            while (am < (1 << 23)) {
              am <<= 1;
            }
            int divdCount = NumberUtility.BitLength(m);
            int divsCount = NumberUtility.BitLength(vtp);
            int dividendShift = (divdCount <= divsCount) ? ((divsCount -
                  divdCount) + 24 + 1) : Math.Max(0,
                (24 + 1) - (divdCount - divsCount));
            long lquo = -1;
            long lrem = -1;
            if (divsCount + dividendShift > 63) {
              EInteger eim = EInteger.FromInt32(m).ShiftLeft(dividendShift);
              EInteger[] divrem3 = eim.DivRem(EInteger.FromInt32(vtp));
              EInteger equo = divrem3[0];
              EInteger erem = divrem3[1];
              if (equo.CanFitInInt64() && erem.CanFitInInt64()) {
                lquo = equo.ToInt64Checked();
                lrem = erem.ToInt64Checked();
              }
            } else {
              long ldividend = ((long)m) << dividendShift;
              lquo = ldividend / vtp;
              lrem = ldividend - (lquo * vtp);
            }
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
              var smallmantissa = (int)(lquo & 0x7fffff);
              // NOTE: Assumed not to be subnormal
              smallmantissa |= (nexp + 150) << 23;
              if (this.IsNegative) {
                smallmantissa |= 1 << 31;
              }
              return smallmantissa;
            }
          }
        }
        if (this.exponent.CompareToInt(39) > 0) {
          // Very high exponent, treat as infinity
          return this.IsNegative ? unchecked((int)0xff800000) :
            0x7f800000;
        }
      }
      return this.ToEFloat(EContext.Binary32).ToSingleBits();
    }

    /// <summary>Converts this value to its closest equivalent as a 32-bit
    /// floating-point number, using the half-even rounding mode.
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

    /// <summary>Converts this value to a text string. Returns a value
    /// compatible with this class's FromString method.</summary>
    /// <returns>A string representation of this object. The text string
    /// will be in exponential notation (expressed as a number 1 or
    /// greater, but less than 10, times a power of 10) if this object's
    /// Exponent property is greater than 0 or if the number's first
    /// nonzero decimal digit is more than five digits after the decimal
    /// point.</returns>
    public override string ToString() {
      return this.ToStringInternal(0);
    }

    /// <summary>Returns the unit in the last place. The significand will
    /// be 1 and the exponent will be this number's exponent. Returns 1
    /// with an exponent of 0 if this number is infinity or not-a-number
    /// (NaN).</summary>
    /// <returns>An arbitrary-precision decimal number.</returns>
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
      return new EDecimal(
          mantissa,
          exponent,
          (byte)flags);
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
      return new EDecimal(
          FastIntegerFixed.FromBig(mantissa),
          FastIntegerFixed.FromBig(exponent),
          (byte)flags);
    }
    private static bool AppendString(
      StringBuilder builder,
      char c,
      FastInteger count) {
      if (count.CompareToInt(Int32.MaxValue) > 0 || count.Sign < 0) {
        throw new NotSupportedException();
      }
      int icount = count.ToInt32();
      if (icount > RepeatDivideThreshold) {
        var sb2 = new StringBuilder(RepeatDivideThreshold);
        for (var i = 0; i < RepeatDivideThreshold; ++i) {
          builder.Append(c);
        }
        string sb2str = sb2.ToString();
        int rem, count2;
        count2 = icount / RepeatDivideThreshold;
        rem = icount % RepeatDivideThreshold;
        for (var i = 0; i < count2; ++i) {
          builder.Append(sb2str);
        }
        for (var i = 0; i < rem; ++i) {
          builder.Append(c);
        }
      } else {
        for (var i = 0; i < icount; ++i) {
          builder.Append(c);
        }
      }
      return true;
    }

    internal static IRadixMath<EDecimal> GetMathValue(EContext ctx) {
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

    private static EInteger[] GetAdjustedExponentDecimalBounds(
      EDecimal ed) {
      if (!ed.IsFinite) {
        return new EInteger[] { EInteger.Zero, EInteger.Zero };
      }
      if (ed.IsZero) {
        return new EInteger[] { EInteger.Zero, EInteger.Zero };
      }
      EInteger retEInt = ed.Exponent;
      EInteger[] sigPrecBounds = NumberUtility.DecimalDigitLengthBoundsAsEI(
          ed.UnsignedMantissa);
      EInteger eia = retEInt.Add(sigPrecBounds[0].Subtract(1));
      EInteger eib = retEInt.Add(sigPrecBounds[1].Subtract(1));
      return new EInteger[] {
        EInteger.Min(eia, eib),
        EInteger.Max(eia, eib),
      };
    }

    private static EInteger GetAdjustedExponentBinary(EFloat ef) {
      if (!ef.IsFinite) {
        return EInteger.Zero;
      }
      if (ef.IsZero) {
        return EInteger.Zero;
      }
      EInteger retEInt = ef.Exponent;
      EInteger valueEiPrecision =
        ef.UnsignedMantissa.GetSignedBitLengthAsEInteger();
      retEInt = retEInt.Add(valueEiPrecision.Subtract(1));
      return retEInt;
    }

    private EDecimal RoundToExponentFast(
      int exponentSmall,
      ERounding rounding) {
      if (this.IsFinite && this.exponent.CanFitInInt32() &&
        this.unsignedMantissa.CanFitInInt32()) {
        int thisExponentSmall = this.exponent.ToInt32();
        if (thisExponentSmall == exponentSmall) {
          return this;
        }
        if (thisExponentSmall >= -100 && thisExponentSmall <= 100 &&
          exponentSmall >= -100 && exponentSmall <= 100) {
          if (rounding == ERounding.Down) {
            int diff = exponentSmall - thisExponentSmall;
            if (diff >= 1 && diff <= 9) {
              int thisMantissaSmall = this.unsignedMantissa.ToInt32();
              thisMantissaSmall /= ValueTenPowers[diff];
              return new EDecimal(
                  FastIntegerFixed.FromInt32(thisMantissaSmall),
                  FastIntegerFixed.FromInt32(exponentSmall),
                  this.flags);
            }
          } else if (rounding == ERounding.HalfEven) {
            int diff = exponentSmall - thisExponentSmall;
            int thisMantissaSmall = this.unsignedMantissa.ToInt32();
            if (diff >= 1 && diff <= 9 && thisMantissaSmall != Int32.MaxValue) {
              int pwr = ValueTenPowers[diff - 1];
              int div = thisMantissaSmall / pwr;
              int div2 = (div > 43698) ? (div / 10) : ((div * 26215) >> 18);
              // var div2 = (int)(((long)div*3435973837L) >> 35); // Division by 10
              int rem = div - (div2 * 10);
              if (rem > 5) {
                ++div2;
              } else if (rem == 5) {
                if ((div2 & 1) == 1 || (thisMantissaSmall - (div * pwr)) != 0) {
                  ++div2;
                }
              }
              return new EDecimal(
                  FastIntegerFixed.FromInt32(div2),
                  FastIntegerFixed.FromInt32(exponentSmall),
                  this.flags);
            }
          }
        }
      }
      return null;
    }

    private bool IsIntegerPartZero() {
      // Returns whether the number, once its fractional part
      // is discarded, is zero.
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
        EInteger umantissa = this.UnsignedMantissa;
        EInteger[] bounds =
          NumberUtility.DecimalDigitLengthBoundsAsEI(umantissa);
        EInteger digitCountUpper = bounds[1];
        EInteger digitCountLower = bounds[0];
        EInteger bigexponent = this.Exponent;
        return (digitCountUpper.CompareTo(bigexponent.Abs()) < 0) ? true :
          ((digitCountLower.CompareTo(bigexponent.Abs()) > 0) ? false :
            (this.CompareTo(-1) > 0 && this.CompareTo(1) < 0));
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
        EInteger exponent = this.Exponent;
        EInteger exponentBitSize = exponent.GetUnsignedBitLengthAsEInteger();
        if (exponentBitSize.CompareTo(64) > 0) {
          throw new NotSupportedException(
            "Not enough memory to store as EInteger.");
        }
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
        if (exact) {
          if (!acc.TruncateRightExact(bigexponent)) {
            // Some digits were discarded
            throw new ArithmeticException("Not an exact integer");
          }
        } else {
          acc.TruncateRightSimple(bigexponent);
        }
        bigmantissa = acc.ShiftedInt;
        if (this.IsNegative) {
          bigmantissa = -bigmantissa;
        }
        return bigmantissa;
      }
    }

    private static EInteger PowerOfRadixBitsLowerBound(EInteger e) {
      return e.Abs().Multiply(332).Divide(100).Add(1);
    }
    private static EInteger PowerOfRadixBitsUpperBound(EInteger e) {
      return e.Abs().Multiply(333).Divide(100).Add(1);
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
        // x * 10^y
        long imantbits = mant.GetSignedBitLengthAsInt64();
        if (imantbits >= maxBitLength) {
          throw new OverflowException("Value out of range");
        }
        if (exp.CompareTo(0x100000) < 0 && imantbits < 0x100000) {
          // Lower bound of bit count in 10^exp based on ln(10^exp)/ln(2)
          long expBitsLowerBound = (exp.ToInt64Checked() * 332 / 100) + 1;
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
        // x * 10^-y. Check for trivial overflow cases before
        // running ToEInteger.
        exp = exp.Abs();
        long imantbits = mant.GetSignedBitLengthAsInt64();
        if (exp.CompareTo(0x100000) < 0 && imantbits < 0x100000) {
          long expBitsUpperBound = (exp.ToInt64Checked() * 333 / 100) + 1;
          long expBitsLowerBound = (exp.ToInt64Checked() * 332 / 100) + 1;
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
        } else if (imantbits < 0x100000 && exp.CompareTo(0x400000) >= 0) {
          // (mant / 10^exp) would be less than one, so not exact
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

    private static bool HasTerminatingBinaryExpansion(EInteger
      den) {
      if (den.IsZero) {
        return false;
      }
      if (den.GetUnsignedBit(0) && den.CompareTo(EInteger.One) != 0) {
        return false;
      }
      // NOTE: Equivalent to (den >> lowBit(den)) == 1
      return den.GetUnsignedBitLengthAsEInteger()
        .Equals(den.GetLowBitAsEInteger().Add(1));
    }

    private EFloat WithThisSign(EFloat ef) {
      return this.IsNegative ? ef.Negate() : ef;
    }

    /// <summary>Creates a binary floating-point number from this object's
    /// value. Note that if the binary floating-point number contains a
    /// negative exponent, the resulting value might not be exact, in which
    /// case the resulting binary floating-point number will be an
    /// approximation of this decimal number's value.</summary>
    /// <param name='ec'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. The precision is in
    /// bits, and an example of this parameter is <c>EContext.Binary64</c>. Can be null.</param>
    /// <returns>An arbitrary-precision float floating-point
    /// number.</returns>
    public EFloat ToEFloat(EContext ec) {
      EInteger bigintExp = this.Exponent;
      EInteger bigUnsignedMantissa = this.UnsignedMantissa;
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
      if (bigUnsignedMantissa.IsZero) {
        return this.IsNegative ? EFloat.NegativeZero.RoundToPrecision(ec) :
          EFloat.Zero.RoundToPrecision(ec);
      }
      if (bigintExp.IsZero) {
        // Integer
        // DebugUtility.Log("Integer");
        return this.WithThisSign(EFloat.FromEInteger(bigUnsignedMantissa))
          .RoundToPrecision(ec);
      }
      EContext b64 = EContext.Binary64;
      if (ec != null && ec.HasMaxPrecision &&
        ec.HasExponentRange &&
        !ec.IsSimplified && ec.EMax.CompareTo(b64.EMax) <= 0 &&
        ec.EMin.CompareTo(b64.EMin) >= 0 &&
        ec.Precision.CompareTo(b64.Precision) <= 0) {
        // Quick check for overflow or underflow
        EInteger[] bounds =
          NumberUtility.DecimalDigitLengthBoundsAsEI(bigUnsignedMantissa);
        EInteger digitCountUpper = bounds[1];
        EInteger adjexpLowerBound = bigintExp;
        EInteger adjexpUpperBound = bigintExp.Add(
            digitCountUpper.Subtract(1));
        if (adjexpUpperBound.CompareTo(-326) < 0) {
          // Underflow to zero
          EInteger eTiny = ec.EMin.Subtract(ec.Precision.Subtract(1));
          eTiny = eTiny.Subtract(2); // subtract 2 from proper eTiny to
          // trigger underflow (2, rather than 1, because of HalfUp mode)
          EFloat ret = EFloat.Create(EInteger.FromInt32(
                this.IsNegative ? -1 : 1),
              eTiny);
          return ret.RoundToPrecision(ec);
        } else if (adjexpLowerBound.CompareTo(309) > 0) {
          return EFloat.GetMathValue().SignalOverflow(ec, this.IsNegative);
        }
        EInteger digitCountLower = bounds[0];
        if (bigintExp.Sign >= 0 &&
          digitCountLower.Subtract(2).CompareTo(309) > 0) {
          return EFloat.GetMathValue().SignalOverflow(ec, this.IsNegative);
        } else if (digitCountLower.Add(bigintExp).Subtract(2).CompareTo(309) >
          0) {
          return EFloat.GetMathValue().SignalOverflow(ec, this.IsNegative);
        }
      }
      if (bigintExp.Sign > 0) {
        // Scaled integer
        // --- Optimizations for Binary32 and Binary64
        if (ec == EContext.Binary32) {
          if (bigintExp.CompareTo(39) > 0) {
            return this.IsNegative ? EFloat.NegativeInfinity :
              EFloat.PositiveInfinity;
          }
        } else if (ec == EContext.Binary64) {
          if (bigintExp.CompareTo(309) > 0) {
            return this.IsNegative ? EFloat.NegativeInfinity :
              EFloat.PositiveInfinity;
          }
        }
        // --- End optimizations for Binary32 and Binary64
        // DebugUtility.Log("Scaled integer");
        EInteger bigmantissa = bigUnsignedMantissa;
        bigintExp = NumberUtility.FindPowerOfTenFromBig(bigintExp);
        bigmantissa *= (EInteger)bigintExp;
        return this.WithThisSign(EFloat.FromEInteger(bigmantissa))
          .RoundToPrecision(ec);
      } else {
        // Fractional number
        // DebugUtility.Log("Fractional");
        EInteger scale = bigintExp;
        EInteger bigmantissa = bigUnsignedMantissa;
        EInteger negscale = -scale;
        EInteger divisor = NumberUtility.FindPowerOfTenFromBig(negscale);
        ec = ec ?? EContext.UnlimitedHalfEven;
        if (ec.HasMaxPrecision) {
          EFloat efNum = EFloat.FromEInteger(bigmantissa);
          if (this.Sign < 0) {
            efNum = efNum.Negate();
          }
          EFloat efDen = EFloat.FromEInteger(divisor);
          return efNum.Divide(efDen, ec);
        } else if (!ec.HasFlagsOrTraps) {
          EFloat efNum = EFloat.FromEInteger(bigmantissa);
          if (this.Sign < 0) {
            efNum = efNum.Negate();
          }
          EFloat efDen = EFloat.FromEInteger(divisor);
          EFloat ret = efNum.Divide(efDen, ec);
          if (!ret.IsNaN()) {
            return ret;
          }
          EInteger bitprec = bigmantissa.GetUnsignedBitLengthAsEInteger();
          bitprec = EInteger.Max(bitprec, EInteger.FromInt32(53));
          return efNum.Divide(
              efDen,
              ec.WithBigPrecision(bitprec));
        } else if (ec.Traps != 0) {
          EContext tctx = ec.GetNontrapping();
          EFloat ret = this.ToEFloat(tctx);
          return ec.TriggerTraps(ret, tctx);
        } else {
          EContext tmpctx = ec.WithBlankFlags();
          EFloat efNum = EFloat.FromEInteger(bigmantissa);
          if (this.Sign < 0) {
            efNum = efNum.Negate();
          }
          EFloat efDen = EFloat.FromEInteger(divisor);
          EFloat ret = efNum.Divide(efDen, tmpctx);
          if (!ret.IsNaN()) {
            ec.Flags |= tmpctx.Flags;
            return ret;
          }
          EInteger bitprec = bigmantissa.GetUnsignedBitLengthAsEInteger();
          bitprec = EInteger.Max(bitprec, EInteger.FromInt32(53));
          tmpctx = ec.WithBigPrecision(bitprec).WithBlankFlags();
          ret = efNum.Divide(efDen, tmpctx);
          ec.Flags |= tmpctx.Flags;
          return ret;
        }
      }
    }

    // TODO: Add option to always display exponential notation
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
      string mantissaString;
      if (scaleSign == 0) {
        mantissaString = this.unsignedMantissa.ToString();
        return negative ? "-" + mantissaString : mantissaString;
      }
      bool iszero = this.unsignedMantissa.IsValueZero;
      if (mode == 2 && iszero && scaleSign < 0) {
        // special case for zero in plain
        mantissaString = this.unsignedMantissa.ToString();
        return negative ? "-" + mantissaString : mantissaString;
      }
      if (mode == 0 && this.unsignedMantissa.CanFitInInt32() &&
        this.exponent.CanFitInInt32()) {
        int intExp = this.exponent.ToInt32();
        int intMant = this.unsignedMantissa.ToInt32();
        if (intMant < 1000 && intExp == -2) {
          int a, b, c;
          var i = 0;
          a = intMant % 10;
          intMant /= 10;
          b = intMant % 10;
          intMant /= 10;
          c = intMant;
          int clength = (negative ? 1 : 0) + 4;
          var chars = new char[clength];
          if (negative) {
            chars[i++] = '-';
          }
          chars[i++] = (char)(0x30 + c);
          chars[i++] = '.';
          chars[i++] = (char)(0x30 + b);
          chars[i++] = (char)(0x30 + a);
          return new String(chars, 0, clength);
        } else if (intMant < 100 && intExp == -1) {
          int a, b;
          var i = 0;
          a = intMant % 10;
          intMant /= 10;
          b = intMant;
          int clength = (negative ? 1 : 0) + 3;
          var chars = new char[clength];
          if (negative) {
            chars[i++] = '-';
          }
          chars[i++] = (char)(0x30 + b);
          chars[i++] = '.';
          chars[i++] = (char)(0x30 + a);
          return new String(chars, 0, clength);
        }
      }
      mantissaString = this.unsignedMantissa.ToString();
      if (mode == 0 && mantissaString.Length < 100 &&
        this.exponent.CanFitInInt32()) {
        int intExp = this.exponent.ToInt32();
        if (intExp > -100 && intExp < 100) {
          int adj = (intExp + mantissaString.Length) - 1;
          if (scaleSign >= 0 && adj >= -6) {
            if (scaleSign > 0) {
              int ms = mantissaString.Length;
              int dp = intExp + ms;
              if (dp < 0) {
                dp = -dp;
                int clength = 2 + dp + (negative ? 1 : 0) + ms;
                var chars = new char[clength];
                var i = 0;
                if (negative) {
                  chars[i++] = '-';
                }
                chars[i++] = '0';
                chars[i++] = '.';
                for (var j = 0; j < dp; ++j) {
                  chars[i++] = '0';
                }
                for (var j = 0; j < ms; ++j) {
                  chars[i++] = mantissaString[j];
                }
                return new String(chars, 0, clength);
              } else if (dp == 0) {
                int clength = 2 + (negative ? 1 : 0) + ms;
                var chars = new char[clength];
                var i = 0;
                if (negative) {
                  chars[i++] = '-';
                }
                chars[i++] = '0';
                chars[i++] = '.';
                for (var j = 0; j < ms; ++j) {
                  chars[i++] = mantissaString[j];
                }
                return new String(chars, 0, clength);
              } else if (dp > 0 && dp <= ms) {
                int clength = 1 + (negative ? 1 : 0) + ms;
                var chars = new char[clength];
                var i = 0;
                var j = 0;
                if (negative) {
                  chars[i++] = '-';
                }
                for (j = 0; j < dp; ++j) {
                  chars[i++] = mantissaString[j];
                }
                chars[i++] = '.';
                for (j = dp; j < ms; ++j) {
                  chars[i++] = mantissaString[j];
                }
                return new String(chars, 0, clength);
              }
            }
          }
        }
      }
      StringBuilder builder = null;
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
        int intphase = adjustedExponent.Copy().Abs().Remainder(3).ToInt32();
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
              0 ? Int32.MaxValue : tmpFast.ToInt32());
            if (negative) {
              builder.Append('-');
            }
            builder.Append("0.");
            AppendString(builder, '0', decimalPoint.Copy().Negate());
            builder.Append(mantissaString);
          } else if (cmp == 0) {
            #if DEBUG
            if (!decimalPoint.CanFitInInt32()) {
              throw new ArgumentException("doesn't satisfy" +
                "\u0020decimalPoint.CanFitInInt32()");
            }
            if (decimalPoint.ToInt32() != 0) {
              throw new ArgumentException("doesn't satisfy" +
                "\u0020decimalPoint.ToInt32() == 0");
            }
            #endif

            var tmpFast = new FastInteger(mantissaString.Length).AddInt(6);
            builder = new StringBuilder(tmpFast.CompareToInt(Int32.MaxValue) >
              0 ? Int32.MaxValue : tmpFast.ToInt32());
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
            int tmpInt = insertionPoint.ToInt32();
            if (tmpInt < 0) {
              tmpInt = 0;
            }
            var tmpFast = new FastInteger(mantissaString.Length).AddInt(6);
            builder = new StringBuilder(tmpFast.CompareToInt(Int32.MaxValue) >
              0 ? Int32.MaxValue : tmpFast.ToInt32());
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
            int tmpInt = decimalPoint.ToInt32();
            if (tmpInt < 0) {
              tmpInt = 0;
            }
            var tmpFast = new FastInteger(mantissaString.Length).AddInt(6);
            builder = new StringBuilder(tmpFast.CompareToInt(Int32.MaxValue) >
              0 ? Int32.MaxValue : tmpFast.ToInt32());
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
            int tmpInt = tmp.ToInt32();
            if (tmp.Sign < 0) {
              tmpInt = 0;
            }
            var tmpFast = new FastInteger(mantissaString.Length).AddInt(6);
            builder = new StringBuilder(tmpFast.CompareToInt(Int32.MaxValue) >
              0 ? Int32.MaxValue : tmpFast.ToInt32());
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
          builder.Append(adjustedExponent.ToString());
        }
        return builder.ToString();
      }
    }

    private sealed class DecimalMathHelper : IRadixMathHelper<EDecimal> {
      /// <summary>This is an internal method.</summary>
      /// <returns>A 32-bit signed integer.</returns>
      public int GetRadix() {
        return 10;
      }

      /// <summary>This is an internal method.</summary>
      /// <param name='value'>An arbitrary-precision decimal number.</param>
      /// <returns>A 32-bit signed integer.</returns>
      public int GetSign(EDecimal value) {
        return value.Sign;
      }

      /// <summary>This is an internal method.</summary>
      /// <param name='value'>An arbitrary-precision decimal number.</param>
      /// <returns>An arbitrary-precision integer.</returns>
      public EInteger GetMantissa(EDecimal value) {
        return value.unsignedMantissa.ToEInteger();
      }

      /// <summary>This is an internal method.</summary>
      /// <param name='value'>An arbitrary-precision decimal number.</param>
      /// <returns>An arbitrary-precision integer.</returns>
      public EInteger GetExponent(EDecimal value) {
        return value.exponent.ToEInteger();
      }

      public FastIntegerFixed GetMantissaFastInt(EDecimal value) {
        return value.unsignedMantissa;
      }

      public FastIntegerFixed GetExponentFastInt(EDecimal value) {
        return value.exponent;
      }

      public FastInteger GetDigitLength(EInteger ei) {
        long i64 = ei.GetDigitCountAsInt64();
        if (i64 != Int64.MaxValue) {
          return FastInteger.FromInt64(i64);
        } else {
          return FastInteger.FromBig(ei.GetDigitCountAsEInteger());
        }
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
              fastInt.ToInt32(),
              lastDigit,
              olderDigits);
        } else {
          return new DigitShiftAccumulator(
              fastInt.ToEInteger(),
              lastDigit,
              olderDigits);
        }
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
        EInteger elowbit = den.GetLowBitAsEInteger();
        den = den.ShiftRight(elowbit);
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
        FastInteger fastlowbit = FastInteger.FromBig(elowbit);
        if (fiveShift.CompareTo(fastlowbit) > 0) {
          return fiveShift;
        } else {
          return fastlowbit;
        }
      }

      public FastIntegerFixed MultiplyByRadixPowerFastInt(
        FastIntegerFixed fbigint,
        FastIntegerFixed fpower) {
        // DebugUtility.Log("mbrp "+fbigint+"/"+fpower);
        if (fbigint.IsValueZero) {
          return fbigint;
        }
        bool fitsInInt32 = fpower.CanFitInInt32();
        int powerInt = fitsInInt32 ? fpower.ToInt32() : 0;
        if (fitsInInt32 && powerInt == 0) {
          return fbigint;
        }
        EInteger bigint = fbigint.ToEInteger();
        EInteger ret = null;
        if (bigint.CompareTo(1) != 0) {
          if (fitsInInt32) {
            ret = NumberUtility.MultiplyByPowerOfTen(bigint, powerInt);
          } else {
            EInteger eipower = fpower.ToEInteger();
            ret = NumberUtility.MultiplyByPowerOfTen(bigint, eipower);
          }
        } else {
          ret = fitsInInt32 ? NumberUtility.FindPowerOfTen(powerInt) :
            NumberUtility.FindPowerOfTenFromBig(fpower.ToEInteger());
        }
        return FastIntegerFixed.FromBig(ret);
      }

      public EInteger MultiplyByRadixPower(
        EInteger bigint,
        FastInteger power) {
        if (bigint.IsZero) {
          return bigint;
        }
        bool fitsInInt32 = power.CanFitInInt32();
        int powerInt = fitsInInt32 ? power.ToInt32() : 0;
        if (fitsInInt32 && powerInt == 0) {
          return bigint;
        }
        if (bigint.CompareTo(1) != 0) {
          if (fitsInInt32) {
            return NumberUtility.MultiplyByPowerOfTen(bigint, powerInt);
          } else {
            EInteger eipower = power.ToEInteger();
            return NumberUtility.MultiplyByPowerOfTen(bigint, eipower);
          }
        }
        return fitsInInt32 ? NumberUtility.FindPowerOfTen(powerInt) :
          NumberUtility.FindPowerOfTenFromBig(power.ToEInteger());
      }

      /// <summary>This is an internal method.</summary>
      /// <param name='value'>An arbitrary-precision decimal number.</param>
      /// <returns>A 32-bit signed integer.</returns>
      public int GetFlags(EDecimal value) {
        return ((int)value.flags) & 0xff;
      }

      /// <summary>This is an internal method.</summary>
      /// <param name='mantissa'>The parameter <paramref name='mantissa'/> is
      /// a Numbers.EInteger object.</param>
      /// <param name='exponent'>The parameter <paramref name='exponent'/> is
      /// an internal parameter.</param>
      /// <param name='flags'>The parameter <paramref name='flags'/> is an
      /// internal parameter.</param>
      /// <returns>An arbitrary-precision decimal number.</returns>
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

      /// <summary>This is an internal method.</summary>
      /// <returns>A 32-bit signed integer.</returns>
      public int GetArithmeticSupport() {
        return BigNumberFlags.FiniteAndNonFinite;
      }

      /// <summary>This is an internal method.</summary>
      /// <param name='val'>The parameter <paramref name='val'/> is a 32-bit
      /// signed integer.</param>
      /// <returns>An arbitrary-precision decimal number.</returns>
      public EDecimal ValueOf(int val) {
        return (val == 0) ? Zero : ((val == 1) ? One : FromInt64(val));
      }
    }

    /// <summary>Returns one added to this arbitrary-precision decimal
    /// number.</summary>
    /// <returns>The given arbitrary-precision decimal number plus
    /// one.</returns>
    public EDecimal Increment() {
      return this.Add(1);
    }

    /// <summary>Returns one subtracted from this arbitrary-precision
    /// decimal number.</summary>
    /// <returns>The given arbitrary-precision decimal number minus
    /// one.</returns>
    public EDecimal Decrement() {
      return this.Subtract(1);
    }

    // Begin integer conversions
    private void CheckTrivialOverflow(int maxDigits) {
      if (this.IsZero) {
        return;
      }
      if (this.exponent.Sign < 0) {
        EInteger bigexponent = this.Exponent;
        EInteger bigmantissa = this.UnsignedMantissa;
        bigexponent = bigexponent.Abs();
        bigmantissa = bigmantissa.Abs();
        EInteger lowerBound =
          NumberUtility.DecimalDigitLengthBoundsAsEI(bigmantissa)[0];
        if (lowerBound.Subtract(bigexponent).CompareTo(maxDigits) > 0) {
          throw new OverflowException("Value out of range");
        }
      } else {
        if (this.exponent.CompareToInt(maxDigits) >= 0) {
          throw new OverflowException("Value out of range");
        }
      }
    }

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
      this.CheckTrivialOverflow(3);
      if (this.IsIntegerPartZero()) {
        return (byte)0;
      } else if (this.IsNegative) {
        throw new OverflowException("Value out of range");
      }
      return this.ToEInteger().ToByteChecked();
    }

    /// <summary>Converts this number's value to an integer by discarding
    /// its fractional part, and returns the least-significant bits of its
    /// two's-complement form as a byte (from 0 to 255).</summary>
    /// <returns>This number, converted to a byte (from 0 to 255). Returns
    /// 0 if this value is infinity or not-a-number.</returns>
    public byte ToByteUnchecked() {
      if (this.IsFinite) {
        if (this.IsIntegerPartZero()) {
          return (byte)0;
        }
        if (this.exponent.CompareToInt(8) >= 0) {
          /* Whether positive or negative, 10^x mod 256 is always 0
              for x >= 8 */ return (byte)0;
        }
        return this.ToEInteger().ToByteUnchecked();
      }
      return (byte)0;
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
      if (this.IsNegative && !this.IsZero) {
        throw new OverflowException("Value out of range");
      }
      this.CheckTrivialOverflow(3);
      return this.ToEIntegerIfExact().ToByteChecked();
    }

    /// <summary>Converts a byte (from 0 to 255) to an arbitrary-precision
    /// decimal number.</summary>
    /// <param name='inputByte'>The number to convert as a byte (from 0 to
    /// 255).</param>
    /// <returns>This number's value as an arbitrary-precision decimal
    /// number.</returns>
    public static EDecimal FromByte(byte inputByte) {
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
      this.CheckTrivialOverflow(5);
      return this.IsIntegerPartZero() ? ((short)0) :
        this.ToEInteger().ToInt16Checked();
    }

    /// <summary>Converts this number's value to an integer by discarding
    /// its fractional part, and returns the least-significant bits of its
    /// two's-complement form as a 16-bit signed integer.</summary>
    /// <returns>This number, converted to a 16-bit signed integer. Returns
    /// 0 if this value is infinity or not-a-number.</returns>
    public short ToInt16Unchecked() {
      if (this.IsFinite) {
        if (this.IsIntegerPartZero()) {
          return (short)0;
        }
        if (this.exponent.CompareToInt(16) >= 0) {
          /* Whether positive or negative, 10^x mod 65536 is always 0
              for x >= 16 */ return (short)0;
        }
        return this.ToEInteger().ToInt16Unchecked();
      }
      return (short)0;
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
      this.CheckTrivialOverflow(5);
      return this.ToEIntegerIfExact().ToInt16Checked();
    }

    /// <summary>Converts a 16-bit signed integer to an arbitrary-precision
    /// decimal number.</summary>
    /// <param name='inputInt16'>The number to convert as a 16-bit signed
    /// integer.</param>
    /// <returns>This number's value as an arbitrary-precision decimal
    /// number.</returns>
    public static EDecimal FromInt16(short inputInt16) {
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
      this.CheckTrivialOverflow(10);
      return this.IsIntegerPartZero() ? ((int)0) :
        this.ToEInteger().ToInt32Checked();
    }

    /// <summary>Converts this number's value to an integer by discarding
    /// its fractional part, and returns the least-significant bits of its
    /// two's-complement form as a 32-bit signed integer.</summary>
    /// <returns>This number, converted to a 32-bit signed integer. Returns
    /// 0 if this value is infinity or not-a-number.</returns>
    public int ToInt32Unchecked() {
      if (this.IsFinite) {
        if (this.IsIntegerPartZero()) {
          return 0;
        }
        if (this.exponent.CompareToInt(32) >= 0) {
          /* Whether positive or negative, 10^x mod 2^32 is always 0
              for x >= 32 */ return 0;
        }
        return this.ToEInteger().ToInt32Unchecked();
      }
      return 0;
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
      if (this.IsZero) {
        return (int)0;
      }
      this.CheckTrivialOverflow(10);
      return this.ToEIntegerIfExact().ToInt32Checked();
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
      this.CheckTrivialOverflow(19);
      return this.IsIntegerPartZero() ? 0L : this.ToEInteger().ToInt64Checked();
    }

    /// <summary>Converts this number's value to an integer by discarding
    /// its fractional part, and returns the least-significant bits of its
    /// two's-complement form as a 64-bit signed integer.</summary>
    /// <returns>This number, converted to a 64-bit signed integer. Returns
    /// 0 if this value is infinity or not-a-number.</returns>
    public long ToInt64Unchecked() {
      if (this.IsFinite) {
        if (this.IsIntegerPartZero()) {
          return 0L;
        }
        if (this.exponent.CompareToInt(64) >= 0) {
          /* Whether positive or negative, 10^x mod 2^64 is always 0
              for x >= 64 */ return 0L;
        }
        return this.ToEInteger().ToInt64Unchecked();
      }
      return 0L;
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
      if (this.IsZero) {
        return 0L;
      }
      this.CheckTrivialOverflow(19);
      return this.ToEIntegerIfExact().ToInt64Checked();
    }
    // End integer conversions
  }
}
