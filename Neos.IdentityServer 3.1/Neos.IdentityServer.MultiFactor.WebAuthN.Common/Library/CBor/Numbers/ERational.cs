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
  /// <summary>Represents an arbitrary-precision rational number. This
  /// class can't be inherited. (The "E" stands for "extended", meaning
  /// that instances of this class can be values other than numbers
  /// proper, such as infinity and not-a-number.) In this class, a
  /// rational number consists of a numerator and denominator, each an
  /// arbitrary-precision integer (EInteger), and this class does not
  /// automatically convert rational numbers to lowest terms.
  /// <para><b>Thread safety:</b> Instances of this class are immutable,
  /// so they are inherently safe for use by multiple threads. Multiple
  /// instances of this object with the same properties are
  /// interchangeable, so they should not be compared using the "=="
  /// operator (which might only check if each side of the operator is
  /// the same instance).</para></summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Design",
      "CA1036",
      Justification = "Awaiting advice at dotnet/dotnet-api-docs#2937.")]
  public sealed partial class ERational : IComparable<ERational>,
    IEquatable<ERational> {
    /// <summary>A not-a-number value.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Security",
        "CA2104",
        Justification = "ERational is immutable")]
    public static readonly ERational NaN = new ERational(
      FastIntegerFixed.Zero,
      FastIntegerFixed.One,
      (byte)BigNumberFlags.FlagQuietNaN);

    /// <summary>Negative infinity, less than any other number.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Security",
        "CA2104",
        Justification = "ERational is immutable")]
    public static readonly ERational NegativeInfinity =
      new ERational(
        FastIntegerFixed.Zero,
        FastIntegerFixed.One,
        (byte)(BigNumberFlags.FlagInfinity | BigNumberFlags.FlagNegative));

    /// <summary>A rational number for negative zero.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Security",
        "CA2104",
        Justification = "ERational is immutable")]
    public static readonly ERational NegativeZero =
      new ERational(
          FastIntegerFixed.Zero,
          FastIntegerFixed.One,
          (byte)BigNumberFlags.FlagNegative);

    /// <summary>The rational number one.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Security",
        "CA2104",
        Justification = "ERational is immutable")]
    public static readonly ERational One = FromEInteger(EInteger.One);

    /// <summary>Positive infinity, greater than any other
    /// number.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Security",
        "CA2104",
        Justification = "ERational is immutable")]
    public static readonly ERational PositiveInfinity =
      new ERational(
        FastIntegerFixed.Zero,
        FastIntegerFixed.One,
        (byte)BigNumberFlags.FlagInfinity);

    /// <summary>A signaling not-a-number value.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Security",
        "CA2104",
        Justification = "ERational is immutable")]
    public static readonly ERational SignalingNaN =
      new ERational(
        FastIntegerFixed.Zero,
        FastIntegerFixed.One,
        (byte)BigNumberFlags.FlagSignalingNaN);

    /// <summary>The rational number ten.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Security",
        "CA2104",
        Justification = "ERational is immutable")]
    public static readonly ERational Ten = FromEInteger((EInteger)10);

    /// <summary>A rational number for zero.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Security",
        "CA2104",
        Justification = "ERational is immutable")]
    public static readonly ERational Zero = FromEInteger(EInteger.Zero);

    private readonly FastIntegerFixed denominator;

    private readonly byte flags;
    private readonly FastIntegerFixed unsignedNumerator;

    private ERational(
      FastIntegerFixed numerator,
      FastIntegerFixed denominator,
      byte flags) {
      #if DEBUG
      if (numerator == null) {
        throw new ArgumentNullException(nameof(numerator));
      }
      if (denominator == null) {
        throw new ArgumentNullException(nameof(denominator));
      }
      if (denominator.IsValueZero) {
        throw new ArgumentException("Denominator is zero.");
      }
      #endif
      this.unsignedNumerator = numerator;
      this.denominator = denominator;
      this.flags = flags;
    }

    /// <summary>Initializes a new instance of the
    /// <see cref='PeterO.Numbers.ERational'/> class.</summary>
    /// <param name='numerator'>An arbitrary-precision integer serving as
    /// the numerator.</param>
    /// <param name='denominator'>An arbitrary-precision integer serving as
    /// the denominator.</param>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='numerator'/> or <paramref name='denominator'/> is
    /// null.</exception>
    /// <exception cref='ArgumentException'>Denominator is
    /// zero.</exception>
    [Obsolete("Use the Create method instead.")]
    public ERational(EInteger numerator, EInteger denominator) {
      if (numerator == null) {
        throw new ArgumentNullException(nameof(numerator));
      }
      if (denominator == null) {
        throw new ArgumentNullException(nameof(denominator));
      }
      if (denominator.IsZero) {
        throw new ArgumentException("denominator is zero");
      }
      bool numNegative = numerator.Sign < 0;
      bool denNegative = denominator.Sign < 0;
      this.flags = (byte)((numNegative != denNegative) ?
        BigNumberFlags.FlagNegative : 0);
      if (numNegative) {
        numerator = numerator.Negate();
      }
      if (denNegative) {
        denominator = denominator.Negate();
      }
      this.unsignedNumerator = FastIntegerFixed.FromBig(numerator);
      this.denominator = FastIntegerFixed.FromBig(denominator);
    }

    /// <summary>Creates a copy of this arbitrary-precision rational
    /// number.</summary>
    /// <returns>An arbitrary-precision rational number.</returns>
    public ERational Copy() {
      return new ERational(
          this.unsignedNumerator,
          this.denominator,
          this.flags);
    }

    /// <summary>Gets this object's denominator.</summary>
    /// <value>This object's denominator.</value>
    public EInteger Denominator {
      get {
        return this.denominator.ToEInteger();
      }
    }

    /// <summary>Gets a value indicating whether this object is finite (not
    /// infinity or NaN).</summary>
    /// <value><c>true</c> if this object is finite (not infinity or NaN);
    /// otherwise, <c>false</c>.</value>
    public bool IsFinite {
      get {
        return !this.IsNaN() && !this.IsInfinity();
      }
    }

    /// <summary>Gets a value indicating whether this object's value is
    /// negative (including negative zero).</summary>
    /// <value><c>true</c> if this object's value is negative (including
    /// negative zero); otherwise, <c>false</c>. <c>true</c> if this
    /// object's value is negative; otherwise, <c>false</c>.</value>
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
        return ((this.flags & (BigNumberFlags.FlagInfinity |
                BigNumberFlags.FlagNaN)) == 0) &&
this.unsignedNumerator.IsValueZero;
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
      if (this.denominator.IsEvenNumber &&
           !this.unsignedNumerator.IsEvenNumber) {
        // Even denominator, odd numerator, so not an integer
        return false;
      }
      EInteger rem = this.Numerator.Remainder(this.Denominator);
      return rem.IsZero;
    }

    /// <summary>Gets this object's numerator.</summary>
    /// <value>This object's numerator. If this object is a not-a-number
    /// value, returns the diagnostic information (which will be negative
    /// if this object is negative).</value>
    public EInteger Numerator {
      get {
        return this.IsNegative ? this.unsignedNumerator.Negate().ToEInteger() :
          this.unsignedNumerator.ToEInteger();
      }
    }

    /// <summary>Gets the sign of this rational number.</summary>
    /// <value>The sign of this rational number.</value>
    public int Sign {
      get {
        return ((this.flags & (BigNumberFlags.FlagInfinity |
                BigNumberFlags.FlagNaN)) != 0) ? (this.IsNegative ? -1 : 1) :
          (this.unsignedNumerator.IsValueZero ? 0 : (this.IsNegative ? -1 : 1));
      }
    }

    /// <summary>Gets this object's numerator with the sign
    /// removed.</summary>
    /// <value>This object's numerator. If this object is a not-a-number
    /// value, returns the diagnostic information.</value>
    public EInteger UnsignedNumerator {
      get {
        return this.unsignedNumerator.ToEInteger();
      }
    }

    /// <summary>Creates a rational number with the given numerator and
    /// denominator.</summary>
    /// <param name='numeratorSmall'>The numerator.</param>
    /// <param name='denominatorSmall'>The denominator.</param>
    /// <returns>An arbitrary-precision rational number.</returns>
    /// <exception cref='ArgumentException'>The denominator is
    /// zero.</exception>
    public static ERational Create(
      int numeratorSmall,
      int denominatorSmall) {
      return Create((EInteger)numeratorSmall, (EInteger)denominatorSmall);
    }

    /// <summary>Creates a rational number with the given numerator and
    /// denominator.</summary>
    /// <param name='numeratorLong'>The numerator.</param>
    /// <param name='denominatorLong'>The denominator.</param>
    /// <returns>An arbitrary-precision rational number.</returns>
    /// <exception cref='ArgumentException'>The denominator is
    /// zero.</exception>
    public static ERational Create(
      long numeratorLong,
      long denominatorLong) {
      return Create((EInteger)numeratorLong, (EInteger)denominatorLong);
    }

    /// <summary>Creates a rational number with the given numerator and
    /// denominator.</summary>
    /// <param name='numerator'>The numerator.</param>
    /// <param name='denominator'>The denominator.</param>
    /// <returns>An arbitrary-precision rational number.</returns>
    /// <exception cref='ArgumentException'>The denominator is
    /// zero.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='numerator'/> or <paramref name='denominator'/> is
    /// null.</exception>
    public static ERational Create(
      EInteger numerator,
      EInteger denominator) {
      if (numerator == null) {
        throw new ArgumentNullException(nameof(numerator));
      }
      if (denominator == null) {
        throw new ArgumentNullException(nameof(denominator));
      }
      if (denominator.IsZero) {
        throw new ArgumentException("denominator is zero");
      }
      bool numNegative = numerator.Sign < 0;
      bool denNegative = denominator.Sign < 0;
      var bflags = (byte)((numNegative != denNegative) ?
        BigNumberFlags.FlagNegative : 0);
      if (numNegative) {
        numerator = numerator.Negate();
      }
      if (denNegative) {
        denominator = denominator.Negate();
      }
      return new ERational(
         FastIntegerFixed.FromBig(numerator),
         FastIntegerFixed.FromBig(denominator),
         bflags);
    }

    /// <summary>Creates a not-a-number arbitrary-precision rational
    /// number.</summary>
    /// <param name='diag'>An integer, 0 or greater, to use as diagnostic
    /// information associated with this object. If none is needed, should
    /// be zero. To get the diagnostic information from another
    /// arbitrary-precision rational number, use that object's
    /// <c>UnsignedNumerator</c> property.</param>
    /// <returns>An arbitrary-precision rational number.</returns>
    /// <exception cref='ArgumentException'>The parameter <paramref
    /// name='diag'/> is less than 0.</exception>
    public static ERational CreateNaN(EInteger diag) {
      return CreateNaN(diag, false, false);
    }

    /// <summary>Creates a not-a-number arbitrary-precision rational
    /// number.</summary>
    /// <param name='diag'>An integer, 0 or greater, to use as diagnostic
    /// information associated with this object. If none is needed, should
    /// be zero. To get the diagnostic information from another
    /// arbitrary-precision rational number, use that object's
    /// <c>UnsignedNumerator</c> property.</param>
    /// <param name='signaling'>Whether the return value will be signaling
    /// (true) or quiet (false).</param>
    /// <param name='negative'>Whether the return value is
    /// negative.</param>
    /// <returns>An arbitrary-precision rational number.</returns>
    /// <exception cref='ArgumentException'>The parameter <paramref
    /// name='diag'/> is less than 0.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='diag'/> is null.</exception>
    public static ERational CreateNaN(
      EInteger diag,
      bool signaling,
      bool negative) {
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
      flags |= signaling ? BigNumberFlags.FlagSignalingNaN :
        BigNumberFlags.FlagQuietNaN;
      return new ERational(FastIntegerFixed.FromBig(diag),
  FastIntegerFixed.One,
  (byte)flags);
    }

    /// <summary>Converts a 64-bit floating-point number to a rational
    /// number. This method computes the exact value of the floating point
    /// number, not an approximation, as is often the case by converting
    /// the number to a string.</summary>
    /// <param name='flt'>The parameter <paramref name='flt'/> is a 64-bit
    /// floating-point number.</param>
    /// <returns>A rational number with the same value as <paramref
    /// name='flt'/>.</returns>
    public static ERational FromDouble(double flt) {
      return FromEFloat(EFloat.FromDouble(flt));
    }

    /// <summary>Converts an arbitrary-precision decimal number to a
    /// rational number.</summary>
    /// <param name='ef'>The number to convert as an arbitrary-precision
    /// decimal number.</param>
    /// <returns>An arbitrary-precision rational number.</returns>
    [Obsolete("Renamed to FromEDecimal.")]
    public static ERational FromExtendedDecimal(EDecimal ef) {
      return FromEDecimal(ef);
    }

    /// <summary>Converts an arbitrary-precision binary floating-point
    /// number to a rational number.</summary>
    /// <param name='ef'>The number to convert as an arbitrary-precision
    /// binary floating-point number.</param>
    /// <returns>An arbitrary-precision rational number.</returns>
    [Obsolete("Renamed to FromEFloat.")]
    public static ERational FromExtendedFloat(EFloat ef) {
      return FromEFloat(ef);
    }

    /// <summary>Converts an arbitrary-precision decimal number to a
    /// rational number.</summary>
    /// <param name='ef'>The number to convert as an arbitrary-precision
    /// decimal number.</param>
    /// <returns>An arbitrary-precision rational number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='ef'/> is null.</exception>
    public static ERational FromEDecimal(EDecimal ef) {
      if (ef == null) {
        throw new ArgumentNullException(nameof(ef));
      }
      if (!ef.IsFinite) {
        return ef.IsInfinity() ? (ef.IsNegative ? NegativeInfinity :
PositiveInfinity) : CreateNaN(
                 ef.UnsignedMantissa,
                 ef.IsSignalingNaN(),
                 ef.IsNegative);
      }
      EInteger num = ef.Mantissa;
      EInteger exp = ef.Exponent;
      if (exp.IsZero) {
        return FromEInteger(num);
      }
      bool neg = num.Sign < 0;
      num = num.Abs();
      EInteger den = EInteger.One;
      if (exp.Sign < 0) {
        exp = -(EInteger)exp;
        den = NumberUtility.FindPowerOfTenFromBig(exp);
      } else {
        EInteger powerOfTen = NumberUtility.FindPowerOfTenFromBig(exp);
        num *= (EInteger)powerOfTen;
      }
      if (neg) {
        num = -(EInteger)num;
      }
      return ERational.Create(num, den);
    }

    /// <summary>Converts an arbitrary-precision binary floating-point
    /// number to a rational number.</summary>
    /// <param name='ef'>The number to convert as an arbitrary-precision
    /// binary floating-point number.</param>
    /// <returns>An arbitrary-precision rational number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='ef'/> is null.</exception>
    public static ERational FromEFloat(EFloat ef) {
      if (ef == null) {
        throw new ArgumentNullException(nameof(ef));
      }
      if (!ef.IsFinite) {
        return ef.IsInfinity() ? (ef.IsNegative ? NegativeInfinity :
PositiveInfinity) : CreateNaN(
                 ef.UnsignedMantissa,
                 ef.IsSignalingNaN(),
                 ef.IsNegative);
      }
      EInteger num = ef.Mantissa;
      EInteger exp = ef.Exponent;
      if (exp.IsZero) {
        return FromEInteger(num);
      }
      bool neg = num.Sign < 0;
      num = num.Abs();
      EInteger den = EInteger.One;
      if (exp.Sign < 0) {
        exp = -(EInteger)exp;
        den = den.ShiftLeft(exp);
      } else {
        num = num.ShiftLeft(exp);
      }
      if (neg) {
        num = -(EInteger)num;
      }
      return ERational.Create(num, den);
    }

    /// <summary>Converts an arbitrary-precision integer to a rational
    /// number.</summary>
    /// <param name='bigint'>The number to convert as an
    /// arbitrary-precision integer.</param>
    /// <returns>The exact value of the integer as a rational
    /// number.</returns>
    public static ERational FromEInteger(EInteger bigint) {
      return ERational.Create(bigint, EInteger.One);
    }

    /// <summary>Converts a 32-bit binary floating-point number to a
    /// rational number. This method computes the exact value of the
    /// floating point number, not an approximation, as is often the case
    /// by converting the number to a string.</summary>
    /// <param name='flt'>The parameter <paramref name='flt'/> is a 32-bit
    /// binary floating-point number.</param>
    /// <returns>A rational number with the same value as <paramref
    /// name='flt'/>.</returns>
    public static ERational FromSingle(float flt) {
      return FromEFloat(EFloat.FromSingle(flt));
    }

    /// <summary>Creates a binary rational number from a 32-bit
    /// floating-point number encoded in the IEEE 754 binary32 format. This
    /// method computes the exact value of the floating point number, not
    /// an approximation, as is often the case by converting the number to
    /// a string.</summary>
    /// <param name='value'>A 32-bit integer encoded in the IEEE 754
    /// binary32 format.</param>
    /// <returns>A rational number with the same floating-point value as
    /// <paramref name='value'/>.</returns>
    public static ERational FromSingleBits(int value) {
      return FromEFloat(EFloat.FromSingleBits(value));
    }

    /// <summary>Creates a binary rational number from a 64-bit
    /// floating-point number encoded in the IEEE 754 binary64 format. This
    /// method computes the exact value of the floating point number, not
    /// an approximation, as is often the case by converting the number to
    /// a string.</summary>
    /// <param name='value'>A 64-bit integer encoded in the IEEE 754
    /// binary64 format.</param>
    /// <returns>A rational number with the same floating-point value as
    /// <paramref name='value'/>.</returns>
    public static ERational FromDoubleBits(long value) {
      return FromEFloat(EFloat.FromDoubleBits(value));
    }

    /// <summary>Creates a rational number from a text string that
    /// represents a number. See <c>FromString(String, int, int)</c> for
    /// more information.</summary>
    /// <param name='str'>A string that represents a number.</param>
    /// <returns>An arbitrary-precision rational number with the same value
    /// as the given string.</returns>
    /// <exception cref='FormatException'>The parameter <paramref
    /// name='str'/> is not a correctly formatted number
    /// string.</exception>
    public static ERational FromString(string str) {
      return FromString(str, 0, str == null ? 0 : str.Length);
    }

    /// <summary>
    /// <para>Creates a rational number from a text string that represents
    /// a number.</para>
    /// <para>The format of the string generally consists of:</para>
    /// <list type=''>
    /// <item>An optional plus sign ("+" , U+002B) or minus sign ("-",
    /// U+002D) (if '-' , the value is negative.)</item>
    /// <item>The numerator in the form of one or more digits (these digits
    /// may begin with any number of zeros).</item>
    /// <item>Optionally, "/" followed by the denominator in the form of
    /// one or more digits (these digits may begin with any number of
    /// zeros). If a denominator is not given, it's equal to
    /// 1.</item></list>
    /// <para>The string can also be "-INF", "-Infinity", "Infinity",
    /// "INF", quiet NaN ("NaN" /"-NaN") followed by any number of digits,
    /// or signaling NaN ("sNaN" /"-sNaN") followed by any number of
    /// digits, all in any combination of upper and lower case.</para>
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
    /// <returns>An arbitrary-precision rational number.</returns>
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
    public static ERational FromString(
      string str,
      int offset,
      int length) {
       return ERationalTextString.FromString(str, offset, length, true);
    }

    /// <summary>Creates a rational number from a sequence of <c>char</c> s
    /// that represents a number. See <c>FromString(String, int, int)</c>
    /// for more information.</summary>
    /// <param name='chars'>A sequence of <c>char</c> s that represents a
    /// number.</param>
    /// <returns>An arbitrary-precision rational number with the same value
    /// as the given sequence of <c>char</c> s.</returns>
    /// <exception cref='FormatException'>The parameter <paramref
    /// name='chars'/> is not a correctly formatted sequence of <c>char</c>
    /// s.</exception>
    public static ERational FromString(char[] chars) {
      return FromString(chars, 0, chars == null ? 0 : chars.Length);
    }

    /// <summary>
    /// <para>Creates a rational number from a sequence of <c>char</c> s
    /// that represents a number.</para>
    /// <para>The format of the sequence of <c>char</c> s generally
    /// consists of:</para>
    /// <list type=''>
    /// <item>An optional plus sign ("+" , U+002B) or minus sign ("-",
    /// U+002D) (if '-' , the value is negative.)</item>
    /// <item>The numerator in the form of one or more digits (these digits
    /// may begin with any number of zeros).</item>
    /// <item>Optionally, "/" followed by the denominator in the form of
    /// one or more digits (these digits may begin with any number of
    /// zeros). If a denominator is not given, it's equal to
    /// 1.</item></list>
    /// <para>The sequence of <c>char</c> s can also be "-INF",
    /// "-Infinity", "Infinity", "INF", quiet NaN ("NaN" /"-NaN") followed
    /// by any number of digits, or signaling NaN ("sNaN" /"-sNaN")
    /// followed by any number of digits, all in any combination of upper
    /// and lower case.</para>
    /// <para>All characters mentioned above are the corresponding
    /// characters in the Basic Latin range. In particular, the digits must
    /// be the basic digits 0 to 9 (U+0030 to U+0039). The sequence of
    /// <c>char</c> s is not allowed to contain white space characters,
    /// including spaces.</para></summary>
    /// <param name='chars'>A sequence of <c>char</c> s, a portion of which
    /// represents a number.</param>
    /// <param name='offset'>An index starting at 0 showing where the
    /// desired portion of <paramref name='chars'/> begins.</param>
    /// <param name='length'>The length, in code units, of the desired
    /// portion of <paramref name='chars'/> (but not more than <paramref
    /// name='chars'/> 's length).</param>
    /// <returns>An arbitrary-precision rational number.</returns>
    /// <exception cref='FormatException'>The parameter <paramref
    /// name='chars'/> is not a correctly formatted sequence of <c>char</c>
    /// s.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='chars'/> is null.</exception>
    /// <exception cref='ArgumentException'>Either <paramref
    /// name='offset'/> or <paramref name='length'/> is less than 0 or
    /// greater than <paramref name='chars'/> 's length, or <paramref
    /// name='chars'/> 's length minus <paramref name='offset'/> is less
    /// than <paramref name='length'/>.</exception>
    public static ERational FromString(
      char[] chars,
      int offset,
      int length) {
       return ERationalCharArrayString.FromString(chars, offset, length, true);
    }

    /// <summary>Creates a rational number from a sequence of bytes that
    /// represents a number. See <c>FromString(String, int, int)</c> for
    /// more information.</summary>
    /// <param name='bytes'>A sequence of bytes that represents a
    /// number.</param>
    /// <returns>An arbitrary-precision rational number with the same value
    /// as the given sequence of bytes.</returns>
    /// <exception cref='FormatException'>The parameter <paramref
    /// name='bytes'/> is not a correctly formatted sequence of
    /// bytes.</exception>
    public static ERational FromString(byte[] bytes) {
      return FromString(bytes, 0, bytes == null ? 0 : bytes.Length);
    }

    /// <summary>
    /// <para>Creates a rational number from a sequence of bytes that
    /// represents a number.</para>
    /// <para>The format of the sequence of bytes generally consists
    /// of:</para>
    /// <list type=''>
    /// <item>An optional plus sign ("+" , U+002B) or minus sign ("-",
    /// U+002D) (if '-' , the value is negative.)</item>
    /// <item>The numerator in the form of one or more digits (these digits
    /// may begin with any number of zeros).</item>
    /// <item>Optionally, "/" followed by the denominator in the form of
    /// one or more digits (these digits may begin with any number of
    /// zeros). If a denominator is not given, it's equal to
    /// 1.</item></list>
    /// <para>The sequence of bytes can also be "-INF", "-Infinity",
    /// "Infinity", "INF", quiet NaN ("NaN" /"-NaN") followed by any number
    /// of digits, or signaling NaN ("sNaN" /"-sNaN") followed by any
    /// number of digits, all in any combination of upper and lower
    /// case.</para>
    /// <para>All characters mentioned above are the corresponding
    /// characters in the Basic Latin range. In particular, the digits must
    /// be the basic digits 0 to 9 (U+0030 to U+0039). The sequence of
    /// bytes is not allowed to contain white space characters, including
    /// spaces.</para></summary>
    /// <param name='bytes'>A sequence of bytes, a portion of which
    /// represents a number.</param>
    /// <param name='offset'>An index starting at 0 showing where the
    /// desired portion of <paramref name='bytes'/> begins.</param>
    /// <param name='length'>The length, in code units, of the desired
    /// portion of <paramref name='bytes'/> (but not more than <paramref
    /// name='bytes'/> 's length).</param>
    /// <returns>An arbitrary-precision rational number.</returns>
    /// <exception cref='FormatException'>The parameter <paramref
    /// name='bytes'/> is not a correctly formatted sequence of
    /// bytes.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bytes'/> is null.</exception>
    /// <exception cref='ArgumentException'>Either <paramref
    /// name='offset'/> or <paramref name='length'/> is less than 0 or
    /// greater than <paramref name='bytes'/> 's length, or <paramref
    /// name='bytes'/> 's length minus <paramref name='offset'/> is less
    /// than <paramref name='length'/>.</exception>
    public static ERational FromString(
      byte[] bytes,
      int offset,
      int length) {
       return ERationalByteArrayString.FromString(bytes, offset, length, true);
    }

    /// <summary>Compares the absolute values of this object and another
    /// object, imposing a total ordering on all possible values (ignoring
    /// their signs). In this method:
    /// <list>
    /// <item>For objects with the same value, the one with the higher
    /// denominator has a greater "absolute value".</item>
    /// <item>Negative zero and positive zero are considered equal.</item>
    /// <item>Quiet NaN has a higher "absolute value" than signaling NaN.
    /// If both objects are quiet NaN or both are signaling NaN, the one
    /// with the higher diagnostic information has a greater "absolute
    /// value".</item>
    /// <item>NaN has a higher "absolute value" than infinity.</item>
    /// <item>Infinity has a higher "absolute value" than any finite
    /// number.</item></list></summary>
    /// <param name='other'>An arbitrary-precision rational number to
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
    public int CompareToTotalMagnitude(ERational other) {
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
        cmp = this.unsignedNumerator.CompareTo(
            other.unsignedNumerator);
        return cmp;
      } else if (valueIThis == 1) {
        return 0;
      } else {
        cmp = this.Abs().CompareTo(other.Abs());
        if (cmp == 0) {
          cmp = this.denominator.CompareTo(
              other.denominator);
          return cmp;
        }
        return cmp;
      }
    }

    /// <summary>Compares the values of this object and another object,
    /// imposing a total ordering on all possible values. In this method:
    /// <list>
    /// <item>For objects with the same value, the one with the higher
    /// denominator has a greater "absolute value".</item>
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
    /// <param name='other'>An arbitrary-precision rational number to
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
    public int CompareToTotal(ERational other) {
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
        cmp = this.unsignedNumerator.CompareTo(
            other.unsignedNumerator);
        return neg1 ? -cmp : cmp;
      } else if (valueIThis == 1) {
        return 0;
      } else {
        cmp = this.CompareTo(other);
        if (cmp == 0) {
          cmp = this.denominator.CompareTo(
              other.denominator);
          return neg1 ? -cmp : cmp;
        }
        return cmp;
      }
    }

    /// <summary>Returns the absolute value of this rational number, that
    /// is, a number with the same value as this one but as a nonnegative
    /// number.</summary>
    /// <returns>An arbitrary-precision rational number.</returns>
    public ERational Abs() {
      if (this.IsNegative) {
        return new ERational(
            this.unsignedNumerator,
            this.denominator,
            (byte)(this.flags & ~BigNumberFlags.FlagNegative));
      }
      return this;
    }

    /// <summary>Adds this arbitrary-precision rational number and another
    /// arbitrary-precision rational number and returns the
    /// result.</summary>
    /// <param name='otherValue'>Another arbitrary-precision rational
    /// number.</param>
    /// <returns>The sum of the two numbers, that is, this
    /// arbitrary-precision rational number plus another
    /// arbitrary-precision rational number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='otherValue'/> is null.</exception>
    public ERational Add(ERational otherValue) {
      if (otherValue == null) {
        throw new ArgumentNullException(nameof(otherValue));
      }
      if (this.IsSignalingNaN()) {
        return CreateNaN(this.UnsignedNumerator, false, this.IsNegative);
      }
      if (otherValue.IsSignalingNaN()) {
        return CreateNaN(
            otherValue.UnsignedNumerator,
            false,
            otherValue.IsNegative);
      }
      if (this.IsQuietNaN()) {
        return this;
      }
      if (otherValue.IsQuietNaN()) {
        return otherValue;
      }
      if (this.IsInfinity()) {
        return otherValue.IsInfinity() ? ((this.IsNegative ==
              otherValue.IsNegative) ? this : NaN) : this;
      }
      if (otherValue.IsInfinity()) {
        return otherValue;
      }
      EInteger ad = this.Numerator * (EInteger)otherValue.Denominator;
      EInteger bc = this.Denominator * (EInteger)otherValue.Numerator;
      EInteger bd = this.Denominator * (EInteger)otherValue.Denominator;
      ad += (EInteger)bc;
      return ERational.Create(ad, bd);
    }

    /// <summary>Compares the mathematical value of an arbitrary-precision
    /// rational number with that of this instance. This method currently
    /// uses the rules given in the CompareToValue method, so that it it is
    /// not consistent with the Equals method, but it may change in a
    /// future version to use the rules for the CompareToTotal method
    /// instead.</summary>
    /// <param name='other'>An arbitrary-precision rational number.</param>
    /// <returns>Zero if the values are equal; a negative number if this
    /// instance is less, or a positive number if this instance is greater.
    /// <para>This implementation returns a positive number if <paramref
    /// name='other'/> is null, to conform to the.NET definition of
    /// CompareTo. This is the case even in the Java version of this
    /// library, for consistency's sake, even though implementations of
    /// <c>Comparable.compareTo()</c> in Java ought to throw an exception
    /// if they receive a null argument rather than treating null as less
    /// or greater than any object.</para>.</returns>
    public int CompareTo(ERational other) {
      return this.CompareToValue(other);
    }

    /// <summary>Compares the mathematical value of an arbitrary-precision
    /// rational number with that of this instance. In this method, NaN
    /// values are greater than any other ERational value, and two NaN
    /// values (even if their payloads differ) are treated as equal by this
    /// method. This method is not consistent with the Equals
    /// method.</summary>
    /// <param name='other'>An arbitrary-precision rational number.</param>
    /// <returns>Zero if the values are equal; a negative number if this
    /// instance is less, or a positive number if this instance is greater.
    /// <para>This implementation returns a positive number if <paramref
    /// name='other'/> is null, to conform to the.NET definition of
    /// CompareTo. This is the case even in the Java version of this
    /// library, for consistency's sake, even though implementations of
    /// <c>Comparable.compareTo()</c> in Java ought to throw an exception
    /// if they receive a null argument rather than treating null as less
    /// or greater than any object.</para>.</returns>
    public int CompareToValue(ERational other) {
      if (other == null) {
        return 1;
      }
      if (this == other) {
        return 0;
      }
      if (this.IsNaN()) {
        return other.IsNaN() ? 0 : 1;
      }
      if (other.IsNaN()) {
        return -1;
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
        throw new InvalidOperationException("doesn't satisfy this.IsFinite");
      }
      if (!other.IsFinite) {
        throw new InvalidOperationException("doesn't satisfy other.IsFinite");
      }
      #endif

      int dencmp = this.denominator.CompareTo(other.denominator);
      // At this point, the signs are equal so we can compare
      // their absolute values instead
      int numcmp = this.unsignedNumerator.CompareTo(other.unsignedNumerator);
      if (signA < 0) {
        numcmp = -numcmp;
      }
      if (numcmp == 0) {
        // Special case: numerators are equal, so the
        // number with the lower denominator is greater
        return signA < 0 ? dencmp : -dencmp;
      }
      if (dencmp == 0) {
        // denominators are equal
        return numcmp;
      }
      EInteger ea = this.Numerator;
      EInteger eb = this.Denominator;
      EInteger ec = other.Numerator;
      EInteger ed = other.Denominator;
      int cmpThis = ea.Abs().CompareTo(eb);
      int cmpOther = ec.Abs().CompareTo(ed);
      if (cmpThis == 0 && cmpOther == 0) {
        // Both numbers' absolute values are 1
        return 0;
      } else if (cmpThis == 0) {
        // This number's abs is 1, the other's isn't.
        return signA < 0 ? cmpOther : -cmpOther;
      } else if (cmpOther == 0) {
        // The other number's abs is 1, this one's isn't.
        return signA < 0 ? -cmpThis : cmpThis;
      } else if (cmpThis < 0 && cmpOther > 0) {
        return signA < 0 ? 1 : -1;
      } else if (cmpThis > 0 && cmpOther < 0) {
        return signA < 0 ? -1 : 1;
      }
      // Compare the number of bits of the products
      EInteger bitsADUpper = ea.GetUnsignedBitLengthAsEInteger().Add(
          ed.GetUnsignedBitLengthAsEInteger());
      EInteger bitsBCUpper = eb.GetUnsignedBitLengthAsEInteger().Add(
          ec.GetUnsignedBitLengthAsEInteger());
      EInteger bitsADLower = bitsADUpper.Subtract(1);
      EInteger bitsBCLower = bitsBCUpper.Subtract(1);
      if (bitsADLower.CompareTo(bitsBCUpper) > 0) {
        return signA < 0 ? -1 : 1;
      }
      if (bitsBCLower.CompareTo(bitsADUpper) > 0) {
        return signA < 0 ? 1 : -1;
      }
      EInteger ad = ea.Multiply(ed);
      EInteger bc = eb.Multiply(ec);
      return ad.CompareTo(bc);
    }

    /// <summary>Gets the greater value between two rational
    /// numbers.</summary>
    /// <param name='first'>An arbitrary-precision rational number.</param>
    /// <param name='second'>Another arbitrary-precision rational
    /// number.</param>
    /// <returns>The larger value of the two numbers. If one is positive
    /// zero and the other is negative zero, returns the positive zero. If
    /// the two numbers are positive and have the same value, returns the
    /// one with the larger denominator. If the two numbers are negative
    /// and have the same value, returns the one with the smaller
    /// denominator.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='first'/> or <paramref name='second'/> is null.</exception>
    public static ERational Max(
      ERational first,
      ERational second) {
      if (first == null) {
        throw new ArgumentNullException(nameof(first));
      }
      if (second == null) {
        throw new ArgumentNullException(nameof(second));
      }
      int cmp = first.CompareToValue(second);
      if (cmp == 0) {
        if (first.IsNegative) {
          return (!second.IsNegative) ? second :
(first.Denominator.CompareTo(second.Denominator) > 0 ?

              first : second);
        } else {
          return second.IsNegative ? first :
(first.Denominator.CompareTo(second.Denominator) < 0 ?

              first : second);
        }
      }
      return cmp > 0 ? first : second;
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
    public static ERational MaxMagnitude(
      ERational first,
      ERational second) {
      if (first == null) {
        throw new ArgumentNullException(nameof(first));
      }
      if (second == null) {
        throw new ArgumentNullException(nameof(second));
      }
      int cmp = first.Abs().CompareToValue(second.Abs());
      return (cmp == 0) ? Max(first, second) : (cmp > 0 ? first : second);
    }

    /// <summary>Gets the lesser value between two rational
    /// numbers.</summary>
    /// <param name='first'>The first value to compare.</param>
    /// <param name='second'>The second value to compare.</param>
    /// <returns>The smaller value of the two numbers. If one is positive
    /// zero and the other is negative zero, returns the negative zero. If
    /// the two numbers are positive and have the same value, returns the
    /// one with the smaller denominator. If the two numbers are negative
    /// and have the same value, returns the one with the larger
    /// denominator.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='first'/> or <paramref name='second'/> is null.</exception>
    public static ERational Min(
      ERational first,
      ERational second) {
      if (first == null) {
        throw new ArgumentNullException(nameof(first));
      }
      if (second == null) {
        throw new ArgumentNullException(nameof(second));
      }
      int cmp = first.CompareToValue(second);
      if (cmp == 0) {
        if (first.IsNegative) {
          return (!second.IsNegative) ? first : (
              first.Denominator.CompareTo(second.Denominator) < 0 ?
              first : second);
        } else {
          return second.IsNegative ? second : (
              first.Denominator.CompareTo(second.Denominator) > 0 ?
              first : second);
        }
      }
      return cmp < 0 ? first : second;
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
    public static ERational MinMagnitude(
      ERational first,
      ERational second) {
      if (first == null) {
        throw new ArgumentNullException(nameof(first));
      }
      if (second == null) {
        throw new ArgumentNullException(nameof(second));
      }
      int cmp = first.Abs().CompareToValue(second.Abs());
      return (cmp == 0) ? Min(first, second) : (cmp < 0 ? first : second);
    }

    /// <summary>Compares the mathematical value of an arbitrary-precision
    /// rational number with that of this instance. This method currently
    /// uses the rules given in the CompareToValue method, so that it it is
    /// not consistent with the Equals method, but it may change in a
    /// future version to use the rules for the CompareToTotal method
    /// instead.</summary>
    /// <param name='intOther'>The parameter <paramref name='intOther'/> is
    /// a 32-bit signed integer.</param>
    /// <returns>Zero if the values are equal; a negative number if this
    /// instance is less, or a positive number if this instance is
    /// greater.</returns>
    public int CompareTo(int intOther) {
      return this.CompareToValue(ERational.FromInt32(intOther));
    }

    /// <summary>Compares the mathematical value of an arbitrary-precision
    /// rational number with that of this instance. In this method, NaN
    /// values are greater than any other ERational value, and two NaN
    /// values (even if their payloads differ) are treated as equal by this
    /// method. This method is not consistent with the Equals
    /// method.</summary>
    /// <param name='intOther'>The parameter <paramref name='intOther'/> is
    /// a 32-bit signed integer.</param>
    /// <returns>Zero if the values are equal; a negative number if this
    /// instance is less, or a positive number if this instance is
    /// greater.</returns>
    public int CompareToValue(int intOther) {
      return this.CompareToValue(ERational.FromInt32(intOther));
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

    /// <summary>Compares an arbitrary-precision binary floating-point
    /// number with this instance. In this method, NaN values are greater
    /// than any other ERational or EFloat value, and two NaN values (even
    /// if their payloads differ) are treated as equal by this
    /// method.</summary>
    /// <param name='other'>An arbitrary-precision binary floating-point
    /// number.</param>
    /// <returns>Zero if the values are equal; a negative number if this
    /// instance is less, or a positive number if this instance is greater.
    /// <para>This implementation returns a positive number if <paramref
    /// name='other'/> is null, to conform to the.NET definition of
    /// CompareTo. This is the case even in the Java version of this
    /// library, for consistency's sake, even though implementations of
    /// <c>Comparable.compareTo()</c> in Java ought to throw an exception
    /// if they receive a null argument rather than treating null as less
    /// or greater than any object.</para>.</returns>
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
        throw new InvalidOperationException("doesn't satisfy this.IsFinite");
      }
      if (!other.IsFinite) {
        throw new InvalidOperationException("doesn't satisfy other.IsFinite");
      }
      #endif
      EInteger bigExponent = other.Exponent;
      if (bigExponent.IsZero) {
        // Special case: other has exponent 0
        EInteger otherMant = other.Mantissa;
        EInteger bcx = this.Denominator * (EInteger)otherMant;
        return this.Numerator.CompareTo(bcx);
      }
      if (bigExponent.Abs().CompareTo((EInteger)1000) > 0) {
        // Other has a high absolute value of exponent, so try different
        // approaches to
        // comparison
        EInteger thisRem;
        EInteger thisInt;
        {
          EInteger[] divrem = this.UnsignedNumerator.DivRem(this.Denominator);
          thisInt = divrem[0];
          thisRem = divrem[1];
        }
        EFloat otherAbs = other.Abs();
        EFloat thisIntDec = EFloat.FromEInteger(thisInt);
        if (thisRem.IsZero) {
          // This object's value is an integer
          // Console.WriteLine("Shortcircuit IV");
          int ret = thisIntDec.CompareTo(otherAbs);
          return this.IsNegative ? -ret : ret;
        }
        if (thisIntDec.CompareTo(otherAbs) > 0) {
          // Truncated absolute value is greater than other's untruncated
          // absolute value
          // Console.WriteLine("Shortcircuit I");
          return this.IsNegative ? -1 : 1;
        }
        // Round up
        thisInt = thisInt.Add(EInteger.One);
        thisIntDec = EFloat.FromEInteger(thisInt);
        if (thisIntDec.CompareTo(otherAbs) < 0) {
          // Absolute value rounded up is less than other's unrounded
          // absolute value
          // Console.WriteLine("Shortcircuit II");
          return this.IsNegative ? 1 : -1;
        }
        thisIntDec = EFloat.FromEInteger(this.UnsignedNumerator).Divide(
            EFloat.FromEInteger(this.Denominator),
            EContext.ForPrecisionAndRounding(256, ERounding.Down));
        if (thisIntDec.CompareTo(otherAbs) > 0) {
          // Truncated absolute value is greater than other's untruncated
          // absolute value
          // Console.WriteLine("Shortcircuit III");
          return this.IsNegative ? -1 : 1;
        }
        if (other.Exponent.Sign > 0) {
          // NOTE: if unsigned numerator is 0, bitLength will return
          // 0 instead of 1, but the possibility of 0 was already excluded
          EInteger bigDigitCount =
            this.UnsignedNumerator.GetSignedBitLengthAsEInteger()
            .Subtract(1);
          if (bigDigitCount.CompareTo(other.Exponent) < 0) {
            // Numerator's digit count minus 1 is less than the other's
            // exponent,
            // and other's exponent is positive, so this value's absolute
            // value is less
            return this.IsNegative ? 1 : -1;
          }
        }
      }
      // Convert to rational number and use usual rational number
      // comparison
      // Console.WriteLine("no shortcircuit");
      // Console.WriteLine(this);
      // Console.WriteLine(other);
      ERational otherRational = ERational.FromEFloat(other);
      return this.CompareToValue(otherRational);
    }

    /// <summary>Compares an arbitrary-precision decimal number with this
    /// instance.</summary>
    /// <param name='other'>An arbitrary-precision decimal number.</param>
    /// <returns>Zero if the values are equal; a negative number if this
    /// instance is less, or a positive number if this instance is greater.
    /// <para>This implementation returns a positive number if <paramref
    /// name='other'/> is null, to conform to the.NET definition of
    /// CompareTo. This is the case even in the Java version of this
    /// library, for consistency's sake, even though implementations of
    /// <c>Comparable.compareTo()</c> in Java ought to throw an exception
    /// if they receive a null argument rather than treating null as less
    /// or greater than any object.</para>.</returns>
    public int CompareToDecimal(EDecimal other) {
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
        throw new InvalidOperationException("doesn't satisfy this.IsFinite");
      }
      if (!other.IsFinite) {
        throw new InvalidOperationException("doesn't satisfy other.IsFinite");
      }
      #endif

      if (other.Exponent.IsZero) {
        // Special case: other has exponent 0
        EInteger otherMant = other.Mantissa;
        EInteger bcx = this.Denominator * (EInteger)otherMant;
        return this.Numerator.CompareTo(bcx);
      }
      if (other.Exponent.Abs().CompareTo((EInteger)50) > 0) {
        // Other has a high absolute value of exponent, so try different
        // approaches to
        // comparison
        EInteger thisRem;
        EInteger thisInt;
        {
          EInteger[] divrem = this.UnsignedNumerator.DivRem(this.Denominator);
          thisInt = divrem[0];
          thisRem = divrem[1];
        }
        EDecimal otherAbs = other.Abs();
        EDecimal thisIntDec = EDecimal.FromEInteger(thisInt);
        if (thisRem.IsZero) {
          // This object's value is an integer
          // Console.WriteLine("Shortcircuit IV");
          int ret = thisIntDec.CompareTo(otherAbs);
          return this.IsNegative ? -ret : ret;
        }
        if (thisIntDec.CompareTo(otherAbs) > 0) {
          // Truncated absolute value is greater than other's untruncated
          // absolute value
          // Console.WriteLine("Shortcircuit I");
          return this.IsNegative ? -1 : 1;
        }
        // Round up
        thisInt = thisInt.Add(EInteger.One);
        thisIntDec = EDecimal.FromEInteger(thisInt);
        if (thisIntDec.CompareTo(otherAbs) < 0) {
          // Absolute value rounded up is less than other's unrounded
          // absolute value
          // Console.WriteLine("Shortcircuit II");
          return this.IsNegative ? 1 : -1;
        }
        // Conservative approximation of this rational number's absolute value,
        // as a decimal number. The true value will be greater or equal.
        thisIntDec = EDecimal.FromEInteger(this.UnsignedNumerator).Divide(
            EDecimal.FromEInteger(this.Denominator),
            EContext.ForPrecisionAndRounding(20, ERounding.Down));
        if (thisIntDec.CompareTo(otherAbs) > 0) {
          // Truncated absolute value is greater than other's untruncated
          // absolute value
          // Console.WriteLine("Shortcircuit III");
          return this.IsNegative ? -1 : 1;
        }
        // Console.WriteLine("---" + this + " " + other);
        if (other.Exponent.Sign > 0) {
          EInteger bigDigitCount =
            this.UnsignedNumerator.GetDigitCountAsEInteger()
            .Subtract(1);
          if (bigDigitCount.CompareTo(other.Exponent) < 0) {
            // Numerator's digit count minus 1 is less than the other's
            // exponent,
            // and other's exponent is positive, so this value's absolute
            // value is less
            return this.IsNegative ? 1 : -1;
          }
        }
      }
      // Convert to rational number and use usual rational number
      // comparison
      ERational otherRational = ERational.FromEDecimal(other);
      return this.CompareToValue(otherRational);
    }

    /// <summary>Returns a number with the same value as this one, but
    /// copying the sign (positive or negative) of another
    /// number.</summary>
    /// <param name='other'>A number whose sign will be copied.</param>
    /// <returns>An arbitrary-precision rational number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='other'/> is null.</exception>
    public ERational CopySign(ERational other) {
      if (other == null) {
        throw new ArgumentNullException(nameof(other));
      }
      if (this.IsNegative) {
        return other.IsNegative ? this : this.Negate();
      } else {
        return other.IsNegative ? this.Negate() : this;
      }
    }

    /// <summary>Divides this arbitrary-precision rational number by
    /// another arbitrary-precision rational number and returns the
    /// result.</summary>
    /// <param name='otherValue'>An arbitrary-precision rational
    /// number.</param>
    /// <returns>The result of dividing this arbitrary-precision rational
    /// number by another arbitrary-precision rational number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='otherValue'/> is null.</exception>
    public ERational Divide(ERational otherValue) {
      if (otherValue == null) {
        throw new ArgumentNullException(nameof(otherValue));
      }
      if (this.IsSignalingNaN()) {
        return CreateNaN(this.UnsignedNumerator, false, this.IsNegative);
      }
      if (otherValue.IsSignalingNaN()) {
        return CreateNaN(
            otherValue.UnsignedNumerator,
            false,
            otherValue.IsNegative);
      }
      if (this.IsQuietNaN()) {
        return this;
      }
      if (otherValue.IsQuietNaN()) {
        return otherValue;
      }
      bool resultNeg = this.IsNegative ^ otherValue.IsNegative;
      if (this.IsInfinity()) {
        return otherValue.IsInfinity() ? NaN : (resultNeg ? NegativeInfinity :
            PositiveInfinity);
      }
      if (otherValue.IsInfinity()) {
        return resultNeg ? NegativeZero : Zero;
      }
      if (otherValue.IsZero) {
        return this.IsZero ? NaN : (resultNeg ? NegativeInfinity :
            PositiveInfinity);
      }
      if (this.IsZero) {
        return resultNeg ? NegativeZero : Zero;
      }
      EInteger ad = this.Numerator * (EInteger)otherValue.Denominator;
      EInteger bc = this.Denominator * (EInteger)otherValue.Numerator;
      return Create(ad, bc);
    }

    /// <summary>Determines whether this object's numerator, denominator,
    /// and properties are equal to those of another object and that other
    /// object is an arbitrary-precision rational number. Not-a-number
    /// values are considered equal if the rest of their properties are
    /// equal. This is not the same as value equality. Notably, two
    /// ERationals with the same value, but of which one is in lowest terms
    /// and the other is not, are compared as unequal by this method
    /// (example: 1/2 vs. 5/10).</summary>
    /// <param name='obj'>The parameter <paramref name='obj'/> is an
    /// arbitrary object.</param>
    /// <returns><c>true</c> if the objects are equal; otherwise,
    /// <c>false</c>. In this method, two objects are not equal if they
    /// don't have the same type or if one is null and the other
    /// isn't.</returns>
    public override bool Equals(object obj) {
      var other = obj as ERational;
      return (
          other != null) && (
          Object.Equals(
            this.unsignedNumerator,
            other.unsignedNumerator) && Object.Equals(
            this.denominator,
            other.denominator) && this.flags == other.flags);
    }

    /// <summary>Determines whether this object's numerator, denominator,
    /// and properties are equal to those of another object. Not-a-number
    /// values are considered equal if the rest of their properties are
    /// equal.</summary>
    /// <param name='other'>An arbitrary-precision rational number to
    /// compare to.</param>
    /// <returns>Either <c>true</c> or <c>false</c>.</returns>
    public bool Equals(ERational other) {
      return this.Equals((object)other);
    }

    /// <summary>Returns the hash code for this instance. No application or
    /// process IDs are used in the hash code calculation.</summary>
    /// <returns>A 32-bit signed integer.</returns>
    public override int GetHashCode() {
      var hashCode = 1857066527;
      unchecked {
        if (this.unsignedNumerator != null) {
          hashCode += 1857066539 * this.unsignedNumerator.GetHashCode();
        }
        if (this.denominator != null) {
          hashCode += 1857066551 * this.denominator.GetHashCode();
        }
        hashCode += 1857066623 * this.flags;
      }
      return hashCode;
    }

    /// <summary>Gets a value indicating whether this object's value is
    /// infinity.</summary>
    /// <returns><c>true</c> if this object's value is infinity; otherwise,
    /// <c>false</c>.</returns>
    public bool IsInfinity() {
      return (this.flags & BigNumberFlags.FlagInfinity) != 0;
    }

    /// <summary>Returns whether this object is a not-a-number
    /// value.</summary>
    /// <returns><c>true</c> if this object is a not-a-number value;
    /// otherwise, <c>false</c>.</returns>
    public bool IsNaN() {
      return (this.flags & BigNumberFlags.FlagNaN) != 0;
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

    /// <summary>Returns whether this object is a quiet not-a-number
    /// value.</summary>
    /// <returns><c>true</c> if this object is a quiet not-a-number value;
    /// otherwise, <c>false</c>.</returns>
    public bool IsQuietNaN() {
      return (this.flags & BigNumberFlags.FlagQuietNaN) != 0;
    }

    /// <summary>Returns whether this object is a signaling not-a-number
    /// value (which causes an error if the value is passed to any
    /// arithmetic operation in this class).</summary>
    /// <returns><c>true</c> if this object is a signaling not-a-number
    /// value (which causes an error if the value is passed to any
    /// arithmetic operation in this class); otherwise, <c>false</c>.</returns>
    public bool IsSignalingNaN() {
      return (this.flags & BigNumberFlags.FlagSignalingNaN) != 0;
    }

    /// <summary>Multiplies this arbitrary-precision rational number by
    /// another arbitrary-precision rational number and returns the
    /// result.</summary>
    /// <param name='otherValue'>An arbitrary-precision rational
    /// number.</param>
    /// <returns>The product of the two numbers, that is, this
    /// arbitrary-precision rational number times another
    /// arbitrary-precision rational number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='otherValue'/> is null.</exception>
    public ERational Multiply(ERational otherValue) {
      if (otherValue == null) {
        throw new ArgumentNullException(nameof(otherValue));
      }
      if (this.IsSignalingNaN()) {
        return CreateNaN(this.UnsignedNumerator, false, this.IsNegative);
      }
      if (otherValue.IsSignalingNaN()) {
        return CreateNaN(
            otherValue.UnsignedNumerator,
            false,
            otherValue.IsNegative);
      }
      if (this.IsQuietNaN()) {
        return this;
      }
      if (otherValue.IsQuietNaN()) {
        return otherValue;
      }
      bool resultNeg = this.IsNegative ^ otherValue.IsNegative;
      if (this.IsInfinity()) {
        return otherValue.IsZero ? NaN : (resultNeg ? NegativeInfinity :
            PositiveInfinity);
      }
      if (otherValue.IsInfinity()) {
        return this.IsZero ? NaN : (resultNeg ? NegativeInfinity :
            PositiveInfinity);
      }
      EInteger ac = this.Numerator * (EInteger)otherValue.Numerator;
      EInteger bd = this.Denominator * (EInteger)otherValue.Denominator;
      return ac.IsZero ? (resultNeg ? NegativeZero : Zero) :
        Create(ac, bd);
    }

    /// <summary>Returns a rational number with the same value as this one
    /// but with the sign reversed.</summary>
    /// <returns>An arbitrary-precision rational number.</returns>
    public ERational Negate() {
      return new ERational(
          this.unsignedNumerator,
          this.denominator,
          (byte)(this.flags ^ BigNumberFlags.FlagNegative));
    }

    /// <summary>Returns the remainder that would result when this
    /// arbitrary-precision rational number is divided by another
    /// arbitrary-precision rational number.</summary>
    /// <param name='otherValue'>An arbitrary-precision rational
    /// number.</param>
    /// <returns>The remainder that would result when this
    /// arbitrary-precision rational number is divided by another
    /// arbitrary-precision rational number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='otherValue'/> is null.</exception>
    public ERational Remainder(ERational otherValue) {
      if (otherValue == null) {
        throw new ArgumentNullException(nameof(otherValue));
      }
      if (this.IsSignalingNaN()) {
        return CreateNaN(this.UnsignedNumerator, false, this.IsNegative);
      }
      if (otherValue.IsSignalingNaN()) {
        return CreateNaN(
            otherValue.UnsignedNumerator,
            false,
            otherValue.IsNegative);
      }
      if (this.IsQuietNaN()) {
        return this;
      }
      if (otherValue.IsQuietNaN()) {
        return otherValue;
      }
      if (this.IsInfinity()) {
        return NaN;
      }
      if (otherValue.IsInfinity()) {
        return this;
      }
      if (otherValue.IsZero) {
        return NaN;
      }
      if (this.IsZero) {
        return this;
      }
      EInteger ad = this.Numerator * (EInteger)otherValue.Denominator;
      EInteger bc = this.Denominator * (EInteger)otherValue.Numerator;
      EInteger quo = ad / (EInteger)bc; // Find the integer quotient
      EInteger tnum = quo * (EInteger)otherValue.Numerator;
      EInteger tden = otherValue.Denominator;
      EInteger thisDen = this.Denominator;
      ad = this.Numerator * (EInteger)tden;
      bc = thisDen * (EInteger)tnum;
      tden *= (EInteger)thisDen;
      ad -= (EInteger)bc;
      return Create(ad, tden);
    }

    /// <summary>Subtracts an arbitrary-precision rational number from this
    /// arbitrary-precision rational number and returns the
    /// result.</summary>
    /// <param name='otherValue'>An arbitrary-precision rational
    /// number.</param>
    /// <returns>The difference between the two numbers, that is, this
    /// arbitrary-precision rational number minus another
    /// arbitrary-precision rational number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='otherValue'/> is null.</exception>
    public ERational Subtract(ERational otherValue) {
      if (otherValue == null) {
        throw new ArgumentNullException(nameof(otherValue));
      }
      if (this.IsSignalingNaN()) {
        return CreateNaN(this.UnsignedNumerator, false, this.IsNegative);
      }
      if (otherValue.IsSignalingNaN()) {
        return CreateNaN(
            otherValue.UnsignedNumerator,
            false,
            otherValue.IsNegative);
      }
      if (this.IsQuietNaN()) {
        return this;
      }
      if (otherValue.IsQuietNaN()) {
        return otherValue;
      }
      if (this.IsInfinity()) {
        if (otherValue.IsInfinity()) {
          return (this.IsNegative != otherValue.IsNegative) ?
            (this.IsNegative ? PositiveInfinity : NegativeInfinity) : NaN;
        }
        return this.IsNegative ? PositiveInfinity : NegativeInfinity;
      }
      if (otherValue.IsInfinity()) {
        return otherValue.IsNegative ? PositiveInfinity : NegativeInfinity;
      }
      EInteger ad = this.Numerator * (EInteger)otherValue.Denominator;
      EInteger bc = this.Denominator * (EInteger)otherValue.Numerator;
      EInteger bd = this.Denominator * (EInteger)otherValue.Denominator;
      ad -= (EInteger)bc;
      return ERational.Create(ad, bd);
    }

    /// <summary>Converts this value to a 64-bit floating-point number. The
    /// half-even rounding mode is used.</summary>
    /// <returns>The closest 64-bit floating-point number to this value.
    /// The return value can be positive infinity or negative infinity if
    /// this value exceeds the range of a 64-bit floating point
    /// number.</returns>
    public double ToDouble() {
      if (!this.IsFinite) {
        return this.ToEFloat(EContext.Binary64).ToDouble();
      }
      if (this.IsNegative && this.IsZero) {
        return EFloat.NegativeZero.ToDouble();
      }
      return EFloat.FromEInteger(this.Numerator)
        .Divide(EFloat.FromEInteger(this.Denominator), EContext.Binary64)
        .ToDouble();
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
      if (!this.IsFinite) {
        return this.ToEFloat(EContext.Binary64).ToDoubleBits();
      }
      if (this.IsNegative && this.IsZero) {
        return EFloat.NegativeZero.ToDoubleBits();
      }
      return EFloat.FromEInteger(this.Numerator)
        .Divide(EFloat.FromEInteger(this.Denominator), EContext.Binary64)
        .ToDoubleBits();
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
      if (!this.IsFinite) {
        return this.ToEFloat(EContext.Binary32).ToSingleBits();
      }
      if (this.IsNegative && this.IsZero) {
        return EFloat.NegativeZero.ToSingleBits();
      }
      return EFloat.FromEInteger(this.Numerator)
        .Divide(EFloat.FromEInteger(this.Denominator), EContext.Binary32)
        .ToSingleBits();
    }

    /// <summary>Converts this value to its form in lowest terms. For
    /// example, (8/4) becomes (4/1).</summary>
    /// <returns>An arbitrary-precision rational with the same value as
    /// this one but in lowest terms. Returns this object if it is infinity
    /// or NaN. Returns ERational.NegativeZero if this object is a negative
    /// zero. Returns ERational.Zero if this object is a positive
    /// zero.</returns>
    /// <exception cref='OverflowException'>This object's value is infinity
    /// or not-a-number (NaN).</exception>
    public ERational ToLowestTerms() {
      if (!this.IsFinite) {
        return this;
      }
      if (this.IsZero) {
        return this.IsNegative ? NegativeZero : Zero;
      }
      EInteger num = this.Numerator;
      EInteger den = this.Denominator;
      EInteger gcd = num.Abs().Gcd(den);
      return Create(num.Divide(gcd), den.Divide(gcd));
    }

    /// <summary>Converts this value to an arbitrary-precision integer by
    /// dividing the numerator by the denominator, discarding its
    /// fractional part, and checking whether the resulting integer
    /// overflows the given signed bit count.</summary>
    /// <param name='maxBitLength'>The maximum number of signed bits the
    /// integer can have. The integer's value may not be less than
    /// -(2^maxBitLength) or greater than (2^maxBitLength) - 1.</param>
    /// <returns>An arbitrary-precision integer.</returns>
    /// <exception cref='OverflowException'>This object's value is infinity
    /// or not-a-number (NaN), or this number's value, once converted to an
    /// integer by dividing the numerator by the denominator and discarding
    /// its fractional part, is less than -(2^maxBitLength) or greater than
    /// (2^maxBitLength) - 1.</exception>
    public EInteger ToSizedEInteger(int maxBitLength) {
      if (maxBitLength < 0) {
        throw new ArgumentException("maxBitLength (" + maxBitLength + ") is" +
          "\u0020not greater or equal to 0");
      }
      if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      EInteger unum = this.UnsignedNumerator;
      EInteger uden = this.Denominator;
      if (unum.CompareTo(uden) < 0) {
        return EInteger.Zero;
      }
      EInteger numBits = unum.GetUnsignedBitLengthAsEInteger();
      EInteger denBits = uden.GetUnsignedBitLengthAsEInteger();
      if (numBits.Subtract(2).Subtract(denBits).CompareTo(maxBitLength) >
0) {
        throw new OverflowException("Value out of range");
      }
      unum = this.ToEInteger();
      if (unum.GetSignedBitLengthAsInt64() > maxBitLength) {
        throw new OverflowException("Value out of range");
      }
      return unum;
    }

    /// <summary>Converts this value to an arbitrary-precision integer,
    /// only if this number's value is an exact integer and that integer
    /// does not overflow the given signed bit count.</summary>
    /// <param name='maxBitLength'>The maximum number of signed bits the
    /// integer can have. The integer's value may not be less than
    /// -(2^maxBitLength) or greater than (2^maxBitLength) - 1.</param>
    /// <returns>An arbitrary-precision integer.</returns>
    /// <exception cref='OverflowException'>This object's value is infinity
    /// or not-a-number (NaN), or this number's value as an integer is less
    /// than -(2^maxBitLength) or greater than (2^maxBitLength) -
    /// 1.</exception>
    /// <exception cref='ArithmeticException'>This object's value is not an
    /// exact integer.</exception>
    public EInteger ToSizedEIntegerIfExact(int maxBitLength) {
      if (maxBitLength < 0) {
        throw new ArgumentException("maxBitLength (" + maxBitLength + ") is" +
          "\u0020not greater or equal to 0");
      }
      if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      EInteger unum = this.UnsignedNumerator;
      EInteger uden = this.Denominator;
      if (unum.IsZero) {
        return EInteger.Zero;
      }
      if (unum.CompareTo(uden) < 0) {
        throw new ArithmeticException("Value is not an integer");
      }
      EInteger numBits = unum.GetUnsignedBitLengthAsEInteger();
      EInteger denBits = uden.GetUnsignedBitLengthAsEInteger();
      if (numBits.Subtract(2).Subtract(denBits).CompareTo(maxBitLength) >
0) {
        throw new OverflowException("Value out of range");
      }
      unum = this.ToEIntegerIfExact();
      if (unum.GetSignedBitLengthAsInt64() > maxBitLength) {
        throw new OverflowException("Value out of range");
      }
      return unum;
    }

    /// <summary>Converts this value to an arbitrary-precision integer by
    /// dividing the numerator by the denominator and discarding the
    /// fractional part of the result.</summary>
    /// <returns>An arbitrary-precision integer.</returns>
    /// <exception cref='OverflowException'>This object's value is infinity
    /// or not-a-number (NaN).</exception>
    public EInteger ToEInteger() {
      if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      return this.Numerator.Divide(this.Denominator);
    }

    /// <summary>Converts this value to an arbitrary-precision integer,
    /// checking whether the value is an exact integer.</summary>
    /// <returns>An arbitrary-precision integer.</returns>
    /// <exception cref='OverflowException'>This object's value is infinity
    /// or not-a-number (NaN).</exception>
    [Obsolete("Renamed to ToEIntegerIfExact.")]
    public EInteger ToEIntegerExact() {
      return this.ToEIntegerIfExact();
    }

    /// <summary>Converts this value to an arbitrary-precision integer,
    /// checking whether the value is an exact integer.</summary>
    /// <returns>An arbitrary-precision integer.</returns>
    /// <exception cref='OverflowException'>This object's value is infinity
    /// or not-a-number (NaN).</exception>
    public EInteger ToEIntegerIfExact() {
      if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      if (this.denominator.IsEvenNumber &&
           !this.unsignedNumerator.IsEvenNumber) {
        // Even denominator, odd numerator, so not an integer
        throw new ArithmeticException("Value is not an integer");
      }
      EInteger rem;
      EInteger quo;
      EInteger[] divrem = this.Numerator.DivRem(this.Denominator);
      quo = divrem[0];
      rem = divrem[1];
      if (!rem.IsZero) {
        throw new ArithmeticException("Value is not an integer");
      }
      return quo;
    }

    /// <summary>Converts this rational number to an arbitrary-precision
    /// decimal number.</summary>
    /// <returns>The exact value of the rational number, or not-a-number
    /// (NaN) if the result can't be exact because it has a nonterminating
    /// decimal expansion.</returns>
    public EDecimal ToEDecimal() {
      return this.ToEDecimal(null);
    }

    /// <summary>Converts this rational number to an arbitrary-precision
    /// decimal number and rounds the result to the given
    /// precision.</summary>
    /// <param name='ctx'>An arithmetic context object to control the
    /// precision, rounding, and exponent range of the result. If HasFlags
    /// of the context is true, will also store the flags resulting from
    /// the operation (the flags are in addition to the pre-existing
    /// flags). Can be null, in which case the precision is unlimited and
    /// no rounding is needed.</param>
    /// <returns>The value of the rational number, rounded to the given
    /// precision. Returns not-a-number (NaN) if the context is null and
    /// the result can't be exact because it has a nonterminating decimal
    /// expansion.</returns>
    public EDecimal ToEDecimal(EContext ctx) {
      if (this.IsNaN()) {
        return EDecimal.CreateNaN(
            this.UnsignedNumerator,
            this.IsSignalingNaN(),
            this.IsNegative,
            ctx);
      }
      if (this.IsPositiveInfinity()) {
        return EDecimal.PositiveInfinity.RoundToPrecision(ctx);
      }
      if (this.IsNegativeInfinity()) {
        return EDecimal.NegativeInfinity.RoundToPrecision(ctx);
      }
      EDecimal ef = (this.IsNegative && this.IsZero) ?
        EDecimal.NegativeZero : EDecimal.FromEInteger(this.Numerator);
      return ef.Divide(EDecimal.FromEInteger(this.Denominator), ctx);
    }

    /// <summary>Converts this rational number to an arbitrary-precision
    /// decimal number, but if the result would have a nonterminating
    /// decimal expansion, rounds that result to the given
    /// precision.</summary>
    /// <param name='ctx'>An arithmetic context object to control the
    /// precision, rounding, and exponent range of the result. This context
    /// will be used only if the exact result would have a nonterminating
    /// decimal expansion. If HasFlags of the context is true, will also
    /// store the flags resulting from the operation (the flags are in
    /// addition to the pre-existing flags). Can be null, in which case the
    /// precision is unlimited and no rounding is needed.</param>
    /// <returns>The exact value of the rational number if possible;
    /// otherwise, the rounded version of the result if a context is given.
    /// Returns not-a-number (NaN) if the context is null and the result
    /// can't be exact because it has a nonterminating decimal
    /// expansion.</returns>
    public EDecimal ToEDecimalExactIfPossible(EContext
      ctx) {
      if (ctx == null) {
        return this.ToEDecimal(null);
      }
      if (this.IsNaN()) {
        return EDecimal.CreateNaN(
            this.UnsignedNumerator,
            this.IsSignalingNaN(),
            this.IsNegative,
            ctx);
      }
      if (this.IsPositiveInfinity()) {
        return EDecimal.PositiveInfinity.RoundToPrecision(ctx);
      }
      if (this.IsNegativeInfinity()) {
        return EDecimal.NegativeInfinity.RoundToPrecision(ctx);
      }
      if (this.IsNegative && this.IsZero) {
        return EDecimal.NegativeZero;
      }
      EInteger num = this.Numerator;
      EInteger den = this.Denominator;
      EDecimal valueEdNum = (this.IsNegative && this.IsZero) ?
        EDecimal.NegativeZero : EDecimal.FromEInteger(num);
      EDecimal valueEdDen = EDecimal.FromEInteger(den);
      EDecimal ed = valueEdNum.Divide(valueEdDen, null);
      if (ed.IsNaN()) {
        // Result would be inexact, try again using the precision context
        ed = valueEdNum.Divide(valueEdDen, ctx);
      }
      return ed;
    }

    /// <summary>Converts this rational number to an arbitrary-precision
    /// decimal number.</summary>
    /// <returns>The exact value of the rational number, or not-a-number
    /// (NaN) if the result can't be exact because it has a nonterminating
    /// decimal expansion.</returns>
    [Obsolete("Renamed to ToEDecimal.")]
    public EDecimal ToExtendedDecimal() {
      return this.ToEDecimal();
    }

    /// <summary>Converts this rational number to an arbitrary-precision
    /// decimal number and rounds the result to the given
    /// precision.</summary>
    /// <param name='ctx'>An arithmetic context object to control the
    /// precision, rounding, and exponent range of the result. If HasFlags
    /// of the context is true, will also store the flags resulting from
    /// the operation (the flags are in addition to the pre-existing
    /// flags). Can be null, in which case the precision is unlimited and
    /// no rounding is needed.</param>
    /// <returns>The value of the rational number, rounded to the given
    /// precision. Returns not-a-number (NaN) if the context is null and
    /// the result can't be exact because it has a nonterminating decimal
    /// expansion.</returns>
    [Obsolete("Renamed to ToEDecimal.")]
    public EDecimal ToExtendedDecimal(EContext ctx) {
      return this.ToEDecimal(ctx);
    }

    /// <summary>Converts this rational number to an arbitrary-precision
    /// decimal number, but if the result would have a nonterminating
    /// decimal expansion, rounds that result to the given
    /// precision.</summary>
    /// <param name='ctx'>An arithmetic context object to control the
    /// precision, rounding, and exponent range of the result. This context
    /// will be used only if the exact result would have a nonterminating
    /// decimal expansion. If HasFlags of the context is true, will also
    /// store the flags resulting from the operation (the flags are in
    /// addition to the pre-existing flags). Can be null, in which case the
    /// precision is unlimited and no rounding is needed.</param>
    /// <returns>The exact value of the rational number if possible;
    /// otherwise, the rounded version of the result if a context is given.
    /// Returns not-a-number (NaN) if the context is null and the result
    /// can't be exact because it has a nonterminating decimal
    /// expansion.</returns>
    [Obsolete("Renamed to ToEDecimalExactIfPossible.")]
    public EDecimal ToExtendedDecimalExactIfPossible(EContext ctx) {
      return this.ToEDecimalExactIfPossible(ctx);
    }

    /// <summary>Converts this rational number to a binary floating-point
    /// number.</summary>
    /// <returns>The exact value of the rational number, or not-a-number
    /// (NaN) if the result can't be exact because it has a nonterminating
    /// binary expansion.</returns>
    public EFloat ToEFloat() {
      return this.ToEFloat(null);
    }

    /// <summary>Converts this rational number to a binary floating-point
    /// number and rounds that result to the given precision.</summary>
    /// <param name='ctx'>An arithmetic context object to control the
    /// precision, rounding, and exponent range of the result. If HasFlags
    /// of the context is true, will also store the flags resulting from
    /// the operation (the flags are in addition to the pre-existing
    /// flags). Can be null, in which case the precision is unlimited and
    /// no rounding is needed.</param>
    /// <returns>The value of the rational number, rounded to the given
    /// precision. Returns not-a-number (NaN) if the context is null and
    /// the result can't be exact because it has a nonterminating binary
    /// expansion.</returns>
    public EFloat ToEFloat(EContext ctx) {
      if (this.IsNaN()) {
        return EFloat.CreateNaN(
            this.UnsignedNumerator,
            this.IsSignalingNaN(),
            this.IsNegative,
            ctx);
      }
      if (this.IsPositiveInfinity()) {
        return EFloat.PositiveInfinity.RoundToPrecision(ctx);
      }
      if (this.IsNegativeInfinity()) {
        return EFloat.NegativeInfinity.RoundToPrecision(ctx);
      }
      EFloat ef = (this.IsNegative && this.IsZero) ?
        EFloat.NegativeZero : EFloat.FromEInteger(this.Numerator);
      return ef.Divide(EFloat.FromEInteger(this.Denominator), ctx);
    }

    /// <summary>Converts this rational number to a binary floating-point
    /// number, but if the result would have a nonterminating binary
    /// expansion, rounds that result to the given precision.</summary>
    /// <param name='ctx'>An arithmetic context object to control the
    /// precision, rounding, and exponent range of the result. This context
    /// will be used only if the exact result would have a nonterminating
    /// binary expansion. If HasFlags of the context is true, will also
    /// store the flags resulting from the operation (the flags are in
    /// addition to the pre-existing flags). Can be null, in which case the
    /// precision is unlimited and no rounding is needed.</param>
    /// <returns>The exact value of the rational number if possible;
    /// otherwise, the rounded version of the result if a context is given.
    /// Returns not-a-number (NaN) if the context is null and the result
    /// can't be exact because it has a nonterminating binary
    /// expansion.</returns>
    public EFloat ToEFloatExactIfPossible(EContext ctx) {
      if (ctx == null) {
        return this.ToEFloat(null);
      }
      if (this.IsNaN()) {
        return EFloat.CreateNaN(
            this.UnsignedNumerator,
            this.IsSignalingNaN(),
            this.IsNegative,
            ctx);
      }
      if (this.IsPositiveInfinity()) {
        return EFloat.PositiveInfinity.RoundToPrecision(ctx);
      }
      if (this.IsNegativeInfinity()) {
        return EFloat.NegativeInfinity.RoundToPrecision(ctx);
      }
      if (this.IsZero) {
        return this.IsNegative ? EFloat.NegativeZero :
          EFloat.Zero;
      }
      EFloat valueEdNum = (this.IsNegative && this.IsZero) ?
        EFloat.NegativeZero : EFloat.FromEInteger(this.Numerator);
      EFloat valueEdDen = EFloat.FromEInteger(this.Denominator);
      EFloat ed = valueEdNum.Divide(valueEdDen, null);
      if (ed.IsNaN()) {
        // Result would be inexact, try again using the precision context
        ed = valueEdNum.Divide(valueEdDen, ctx);
      }
      return ed;
    }

    /// <summary>Converts this rational number to a binary floating-point
    /// number.</summary>
    /// <returns>The exact value of the rational number, or not-a-number
    /// (NaN) if the result can't be exact because it has a nonterminating
    /// binary expansion.</returns>
    [Obsolete("Renamed to ToEFloat.")]
    public EFloat ToExtendedFloat() {
      return this.ToEFloat();
    }

    /// <summary>Converts this rational number to a binary floating-point
    /// number and rounds that result to the given precision.</summary>
    /// <param name='ctx'>An arithmetic context object to control the
    /// precision, rounding, and exponent range of the result. If HasFlags
    /// of the context is true, will also store the flags resulting from
    /// the operation (the flags are in addition to the pre-existing
    /// flags). Can be null, in which case the precision is unlimited and
    /// no rounding is needed.</param>
    /// <returns>The value of the rational number, rounded to the given
    /// precision. Returns not-a-number (NaN) if the context is null and
    /// the result can't be exact because it has a nonterminating binary
    /// expansion.</returns>
    [Obsolete("Renamed to ToEFloat.")]
    public EFloat ToExtendedFloat(EContext ctx) {
      return this.ToEFloat(ctx);
    }

    /// <summary>Converts this rational number to a binary floating-point
    /// number, but if the result would have a nonterminating binary
    /// expansion, rounds that result to the given precision.</summary>
    /// <param name='ctx'>An arithmetic context object to control the
    /// precision, rounding, and exponent range of the result. This context
    /// will be used only if the exact result would have a nonterminating
    /// binary expansion. If HasFlags of the context is true, will also
    /// store the flags resulting from the operation (the flags are in
    /// addition to the pre-existing flags). Can be null, in which case the
    /// precision is unlimited and no rounding is needed.</param>
    /// <returns>The exact value of the rational number if possible;
    /// otherwise, the rounded version of the result if a context is given.
    /// Returns not-a-number (NaN) if the context is null and the result
    /// can't be exact because it has a nonterminating binary
    /// expansion.</returns>
    [Obsolete("Renamed to ToEFloatExactIfPossible.")]
    public EFloat ToExtendedFloatExactIfPossible(EContext ctx) {
      return this.ToEFloatExactIfPossible(ctx);
    }

    /// <summary>Converts this value to a 32-bit binary floating-point
    /// number. The half-even rounding mode is used.</summary>
    /// <returns>The closest 32-bit binary floating-point number to this
    /// value. The return value can be positive infinity or negative
    /// infinity if this value exceeds the range of a 32-bit floating point
    /// number.</returns>
    public float ToSingle() {
      if (!this.IsFinite) {
        return this.ToEFloat(EContext.Binary32).ToSingle();
      }
      if (this.IsNegative && this.IsZero) {
        return EFloat.NegativeZero.ToSingle();
      }
      return EFloat.FromEInteger(this.Numerator)
        .Divide(EFloat.FromEInteger(this.Denominator), EContext.Binary32)
        .ToSingle();
    }

    /// <summary>Converts this object to a text string.</summary>
    /// <returns>A string representation of this object. If this object's
    /// value is infinity or not-a-number, the result is the analogous
    /// return value of the <c>EDecimal.ToString</c> method. Otherwise, the
    /// return value has the following form:
    /// <c>[-]numerator/denominator</c>.</returns>
    public override string ToString() {
      if (!this.IsFinite) {
        if (this.IsSignalingNaN()) {
          if (this.unsignedNumerator.IsValueZero) {
            return this.IsNegative ? "-sNaN" : "sNaN";
          }
          return this.IsNegative ? "-sNaN" + this.unsignedNumerator :
            "sNaN" + this.unsignedNumerator;
        }
        if (this.IsQuietNaN()) {
          if (this.unsignedNumerator.IsValueZero) {
            return this.IsNegative ? "-NaN" : "NaN";
          }
          return this.IsNegative ? "-NaN" + this.unsignedNumerator :
            "NaN" + this.unsignedNumerator;
        }
        if (this.IsInfinity()) {
          return this.IsNegative ? "-Infinity" : "Infinity";
        }
      }
      return (this.unsignedNumerator.IsValueZero && this.IsNegative) ? ("-0/" +
          this.Denominator) : (this.Numerator + "/" + this.Denominator);
    }

    /// <summary>Adds one to an arbitrary-precision rational
    /// number.</summary>
    /// <returns>The given arbitrary-precision rational number plus
    /// one.</returns>
    public ERational Increment() {
      return this.Add(FromInt32(1));
    }

    /// <summary>Subtracts one from an arbitrary-precision rational
    /// number.</summary>
    /// <returns>The given arbitrary-precision rational number minus
    /// one.</returns>
    public ERational Decrement() {
      return this.Subtract(FromInt32(1));
    }

    /// <summary>Adds this arbitrary-precision rational number and a 32-bit
    /// signed integer and returns the result.</summary>
    /// <param name='v'>A 32-bit signed integer.</param>
    /// <returns>The sum of the two numbers, that is, this
    /// arbitrary-precision rational number plus a 32-bit signed
    /// integer.</returns>
    public ERational Add(int v) {
      return this.Add(FromInt32(v));
    }

    /// <summary>Subtracts a 32-bit signed integer from this
    /// arbitrary-precision rational number and returns the
    /// result.</summary>
    /// <param name='v'>The parameter <paramref name='v'/> is a 32-bit
    /// signed integer.</param>
    /// <returns>The difference between the two numbers, that is, this
    /// arbitrary-precision rational number minus a 32-bit signed
    /// integer.</returns>
    public ERational Subtract(int v) {
      return this.Subtract(FromInt32(v));
    }

    /// <summary>Multiplies this arbitrary-precision rational number by a
    /// 32-bit signed integer and returns the result.</summary>
    /// <param name='v'>The parameter <paramref name='v'/> is a 32-bit
    /// signed integer.</param>
    /// <returns>The product of the two numbers, that is, this
    /// arbitrary-precision rational number times a 32-bit signed
    /// integer.</returns>
    public ERational Multiply(int v) {
      return this.Multiply(FromInt32(v));
    }

    /// <summary>Divides this arbitrary-precision rational number by a
    /// 32-bit signed integer and returns the result.</summary>
    /// <param name='v'>The parameter <paramref name='v'/> is a 32-bit
    /// signed integer.</param>
    /// <returns>The result of dividing this arbitrary-precision rational
    /// number by a 32-bit signed integer.</returns>
    /// <exception cref='ArithmeticException'>The parameter <paramref
    /// name='v'/> is zero.</exception>
    public ERational Divide(int v) {
      return this.Divide(FromInt32(v));
    }

    /// <summary>Returns the remainder that would result when this
    /// arbitrary-precision rational number is divided by a 32-bit signed
    /// integer.</summary>
    /// <param name='v'>The divisor.</param>
    /// <returns>The remainder that would result when this
    /// arbitrary-precision rational number is divided by a 32-bit signed
    /// integer.</returns>
    /// <exception cref='ArgumentException'>The parameter <paramref
    /// name='v'/> is zero.</exception>
    public ERational Remainder(int v) {
      return this.Remainder(FromInt32(v));
    }

    /// <summary>Adds this arbitrary-precision rational number and a 64-bit
    /// signed integer and returns the result.</summary>
    /// <param name='v'>A 64-bit signed integer.</param>
    /// <returns>The sum of the two numbers, that is, this
    /// arbitrary-precision rational number plus a 64-bit signed
    /// integer.</returns>
    public ERational Add(long v) {
      return this.Add(FromInt64(v));
    }

    /// <summary>Subtracts a 64-bit signed integer from this
    /// arbitrary-precision rational number and returns the
    /// result.</summary>
    /// <param name='v'>The parameter <paramref name='v'/> is a 64-bit
    /// signed integer.</param>
    /// <returns>The difference between the two numbers, that is, this
    /// arbitrary-precision rational number minus a 64-bit signed
    /// integer.</returns>
    public ERational Subtract(long v) {
      return this.Subtract(FromInt64(v));
    }

    /// <summary>Multiplies this arbitrary-precision rational number by a
    /// 64-bit signed integer and returns the result.</summary>
    /// <param name='v'>The parameter <paramref name='v'/> is a 64-bit
    /// signed integer.</param>
    /// <returns>The product of the two numbers, that is, this
    /// arbitrary-precision rational number times a 64-bit signed
    /// integer.</returns>
    public ERational Multiply(long v) {
      return this.Multiply(FromInt64(v));
    }

    /// <summary>Divides this arbitrary-precision rational number by a
    /// 64-bit signed integer and returns the result.</summary>
    /// <param name='v'>The parameter <paramref name='v'/> is a 64-bit
    /// signed integer.</param>
    /// <returns>The result of dividing this arbitrary-precision rational
    /// number by a 64-bit signed integer.</returns>
    /// <exception cref='ArithmeticException'>The parameter <paramref
    /// name='v'/> is zero.</exception>
    public ERational Divide(long v) {
      return this.Divide(FromInt64(v));
    }

    /// <summary>Returns the remainder that would result when this
    /// arbitrary-precision rational number is divided by a 64-bit signed
    /// integer.</summary>
    /// <param name='v'>The divisor.</param>
    /// <returns>The remainder that would result when this
    /// arbitrary-precision rational number is divided by a 64-bit signed
    /// integer.</returns>
    /// <exception cref='ArgumentException'>The parameter <paramref
    /// name='v'/> is zero.</exception>
    public ERational Remainder(long v) {
      return this.Remainder(FromInt64(v));
    }

    // Begin integer conversions
    private void CheckTrivialOverflow(int maxBits) {
      if (this.IsZero) {
        return;
      }
      if (!this.IsFinite) {
        throw new OverflowException("Value out of range");
      }
      EInteger bignum = this.UnsignedNumerator;
      EInteger bigden = this.Denominator;
      EInteger numbits = bignum.GetUnsignedBitLengthAsEInteger();
      EInteger denbits = bigden.GetUnsignedBitLengthAsEInteger();
      if (numbits.CompareTo(denbits.Add(1).Add(maxBits)) > 0) {
        throw new OverflowException("Value out of range");
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
      if (this.IsNegative && !this.IsZero &&
        this.UnsignedNumerator.CompareTo(this.Denominator) >= 0) {
        throw new OverflowException("Value out of range");
      }
      this.CheckTrivialOverflow(8);
      return this.IsZero ? ((byte)0) : this.ToEInteger().ToByteChecked();
    }

    /// <summary>Converts this number's value to an integer (using
    /// ToEInteger), and returns the least-significant bits of that
    /// integer's two's-complement form as a byte (from 0 to
    /// 255).</summary>
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
      if (this.IsNegative && !this.IsZero &&
        this.UnsignedNumerator.CompareTo(this.Denominator) >= 0) {
        throw new OverflowException("Value out of range");
      }
      this.CheckTrivialOverflow(8);
      return this.IsZero ? ((byte)0) : this.ToEIntegerIfExact().ToByteChecked();
    }

    /// <summary>Converts a byte (from 0 to 255) to an arbitrary-precision
    /// rational number.</summary>
    /// <param name='inputByte'>The number to convert as a byte (from 0 to
    /// 255).</param>
    /// <returns>This number's value as an arbitrary-precision rational
    /// number.</returns>
    public static ERational FromByte(byte inputByte) {
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
      this.CheckTrivialOverflow(15);
      return this.IsZero ? ((short)0) : this.ToEInteger().ToInt16Checked();
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
      this.CheckTrivialOverflow(15);
      return this.IsZero ? ((short)0) :
        this.ToEIntegerIfExact().ToInt16Checked();
    }

    /// <summary>Converts a 16-bit signed integer to an arbitrary-precision
    /// rational number.</summary>
    /// <param name='inputInt16'>The number to convert as a 16-bit signed
    /// integer.</param>
    /// <returns>This number's value as an arbitrary-precision rational
    /// number.</returns>
    public static ERational FromInt16(short inputInt16) {
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
      this.CheckTrivialOverflow(31);
      return this.IsZero ? ((int)0) : this.ToEInteger().ToInt32Checked();
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
      this.CheckTrivialOverflow(31);
      return this.IsZero ? ((int)0) : this.ToEIntegerIfExact().ToInt32Checked();
    }

    /// <summary>Converts a boolean value (true or false) to an
    /// arbitrary-precision rational number.</summary>
    /// <param name='boolValue'>Either true or false.</param>
    /// <returns>The number 1 if <paramref name='boolValue'/> is true;
    /// otherwise, 0.</returns>
    public static ERational FromBoolean(bool boolValue) {
      return FromInt32(boolValue ? 1 : 0);
    }

    /// <summary>Converts a 32-bit signed integer to an arbitrary-precision
    /// rational number.</summary>
    /// <param name='inputInt32'>The number to convert as a 32-bit signed
    /// integer.</param>
    /// <returns>This number's value as an arbitrary-precision rational
    /// number.</returns>
    public static ERational FromInt32(int inputInt32) {
      return FromEInteger(EInteger.FromInt32(inputInt32));
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
      this.CheckTrivialOverflow(63);
      return this.IsZero ? 0L : this.ToEInteger().ToInt64Checked();
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
      this.CheckTrivialOverflow(63);
      return this.IsZero ? 0L : this.ToEIntegerIfExact().ToInt64Checked();
    }

    /// <summary>Converts an unsigned integer expressed as a 64-bit signed
    /// integer to an arbitrary-precision rational number.</summary>
    /// <param name='longerValue'>A 64-bit signed integer. If this value is
    /// 0 or greater, the return value will represent it. If this value is
    /// less than 0, the return value will store 2^64 plus this value
    /// instead.</param>
    /// <returns>An arbitrary-precision rational number. If <paramref
    /// name='longerValue'/> is 0 or greater, the return value will
    /// represent it. If <paramref name='longerValue'/> is less than 0, the
    /// return value will store 2^64 plus this value instead.</returns>
    public static ERational FromInt64AsUnsigned(long longerValue) {
      return longerValue >= 0 ? FromInt64(longerValue) :
           FromEInteger(EInteger.FromInt64AsUnsigned(longerValue));
    }

    /// <summary>Converts a 64-bit signed integer to an arbitrary-precision
    /// rational number.</summary>
    /// <param name='inputInt64'>The number to convert as a 64-bit signed
    /// integer.</param>
    /// <returns>This number's value as an arbitrary-precision rational
    /// number.</returns>
    public static ERational FromInt64(long inputInt64) {
      return FromEInteger(EInteger.FromInt64(inputInt64));
    }

    // End integer conversions
  }
}
