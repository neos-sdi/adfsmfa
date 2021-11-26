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
  /// <summary>A class that implements additional operations on
  /// arbitrary-precision binary floating-point numbers.</summary>
  public static class EFloats {
    private const int BinaryRadix = 2;

    /// <summary>Returns the number 2, the binary radix.</summary>
    /// <param name='ec'>Specifies an arithmetic context for rounding the
    /// number 2. Can be null.</param>
    /// <returns>The number 2, or the closest representable number to 2 in
    /// the arithmetic context.</returns>
    public static EFloat Radix(EContext ec) {
      return EFloat.FromInt32(BinaryRadix).RoundToPrecision(ec);
    }

    /// <summary>Creates a binary floating-point number from a 32-bit
    /// signed integer.</summary>
    /// <param name='i32'>The parameter <paramref name='i32'/> is a 32-bit
    /// signed integer.</param>
    /// <param name='ec'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. Can be null.</param>
    /// <returns>An arbitrary-precision binary floating-point number with
    /// the closest representable value to the given integer.</returns>
    public static EFloat Int32ToEFloat(int i32, EContext ec) {
      // NOTE: Not a miscellaneous operation in the General Decimal
      // Arithmetic Specification 1.70, but required since some of the
      // miscellaneous operations here return integers
      return EFloat.FromInt32(i32).RoundToPrecision(ec);
    }

    /// <summary>Converts a boolean value (either true or false) to an
    /// arbitrary-precision binary floating-point number.</summary>
    /// <param name='b'>Either true or false.</param>
    /// <param name='ec'>A context used for rounding the result. Can be
    /// null.</param>
    /// <returns>Either 1 if <paramref name='b'/> is true, or 0 if
    /// <paramref name='b'/> is false.. The result will be rounded as
    /// specified by the given context, if any.</returns>
    public static EFloat BooleanToEFloat(bool b, EContext ec) {
      // NOTE: Not a miscellaneous operation in the General Decimal
      // Arithmetic Specification 1.70, but required since some of the
      // miscellaneous operations here return booleans
      return EFloat.FromInt32(b ? 1 : 0).RoundToPrecision(ec);
    }

    /// <summary>Returns whether the given arbitrary-precision number
    /// object is in a canonical form. For the current version of EFloat,
    /// all EFloat objects are in a canonical form.</summary>
    /// <param name='ed'>An arbitrary-precision number object.</param>
    /// <returns>Always <c>true</c>.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Usage",
      "CA1801",
      Justification = "Parameter 'ed' is deliberately unused.")]
    public static bool IsCanonical(EFloat ed) {
      // Deliberately unused because all objects are in a canonical
      // form regardless of their value. Removing the parameter
      // or renaming it to be a "discard" parameter would be a
      // breaking change, though.
      return true;
    }

    /// <summary>Returns whether the given arbitrary-precision number
    /// object is neither null nor infinity nor not-a-number
    /// (NaN).</summary>
    /// <param name='ed'>An arbitrary-precision number object.</param>
    /// <returns>Either <c>true</c> if the given arbitrary-precision number
    /// object is neither null nor infinity nor not-a-number (NaN), or
    /// <c>false</c> otherwise.</returns>
    public static bool IsFinite(EFloat ed) {
      return ed != null && ed.IsFinite;
    }

    /// <summary>Returns whether the given arbitrary-precision number
    /// object is positive or negative infinity.</summary>
    /// <param name='ed'>An arbitrary-precision number object.</param>
    /// <returns>Either <c>true</c> if the given arbitrary-precision number
    /// object is positive or negative infinity, or <c>false</c>
    /// otherwise.</returns>
    public static bool IsInfinite(EFloat ed) {
      return ed != null && ed.IsInfinity();
    }

    /// <summary>Returns whether the given arbitrary-precision number
    /// object is a not-a-number (NaN).</summary>
    /// <param name='ed'>An arbitrary-precision number object.</param>
    /// <returns>Either <c>true</c> or <c>false</c>.</returns>
    public static bool IsNaN(EFloat ed) {
      return ed != null && ed.IsNaN();
    }

    /// <summary>Returns whether the given number is a
    /// <i>normal</i> number. A
    /// <i>subnormal number</i> is a nonzero finite number whose Exponent
    /// property (or the number's exponent when that number is expressed in
    /// scientific notation with one digit before the radix point) is less
    /// than the minimum possible exponent for that number. A
    /// <i>normal number</i> is nonzero and finite, but not
    /// subnormal.</summary>
    /// <param name='ed'>An arbitrary-precision number object.</param>
    /// <param name='ec'>A context specifying the exponent range of
    /// arbitrary-precision numbers. Can be null. If AdjustExponent of the
    /// given context is <c>true</c>, a nonzero number is normal if the
    /// number's exponent (when that number is expressed in scientific
    /// notation with one nonzero digit before the radix point) is at least
    /// the given context's EMax property (e.g., if EMax is -100, 2.3456 *
    /// 10
    /// <sup>-99</sup> is normal, but 2.3456 * 10
    /// <sup>-102</sup> is not). If AdjustExponent of the given context is
    /// <c>false</c>, a nonzero number is subnormal if the number's
    /// Exponent property is at least given context's EMax property (e.g.,
    /// if EMax is -100, 23456 * 10
    /// <sup>-99</sup> is normal, but 23456 * 10
    /// <sup>-102</sup> is not).</param>
    /// <returns>Either <c>true</c> if the given number is subnormal, or
    /// <c>false</c> otherwise. Returns <c>true</c> if the given context is
    /// null or HasExponentRange of the given context is <c>false</c>.</returns>
    public static bool IsNormal(EFloat ed, EContext ec) {
      return ed != null && ed.IsFinite && !ed.IsZero && !IsSubnormal(ed, ec);
    }

    /// <summary>Returns whether the given arbitrary-precision number
    /// object is a quiet not-a-number (NaN).</summary>
    /// <param name='ed'>An arbitrary-precision number object.</param>
    /// <returns>Either <c>true</c> or <c>false</c>.</returns>
    public static bool IsQuietNaN(EFloat ed) {
      return ed != null && ed.IsQuietNaN();
    }

    /// <summary>Returns whether the given arbitrary-precision number
    /// object is negative (including negative infinity, negative
    /// not-a-number [NaN], or negative zero).</summary>
    /// <param name='ed'>An arbitrary-precision number object.</param>
    /// <returns>Either <c>true</c> or <c>false</c>.</returns>
    public static bool IsSigned(EFloat ed) {
      return ed != null && ed.IsNegative;
    }

    /// <summary>Returns whether the given arbitrary-precision number
    /// object is a signaling not-a-number (NaN).</summary>
    /// <param name='ed'>An arbitrary-precision number object.</param>
    /// <returns>Either <c>true</c> or <c>false</c>.</returns>
    public static bool IsSignalingNaN(EFloat ed) {
      return ed != null && ed.IsSignalingNaN();
    }

    /// <summary>Converts a number class identifier (ranging from 0 through
    /// 9) to a text string. An arbitrary-precision number object can
    /// belong in one of ten number classes.</summary>
    /// <param name='nc'>An integer identifying a number class.</param>
    /// <returns>A text string identifying the given number class as
    /// follows: 0 = "+Normal"; 1 = "-Normal", 2 = "+Subnormal", 3 =
    /// "-Subnormal", 4 = "+Zero", 5 = "-Zero", 6 = "+Infinity", 7 =
    /// "-Infinity", 8 = "NaN", 9 = "sNaN".</returns>
    /// <exception cref='ArgumentException'>The parameter <paramref
    /// name='nc'/> is less than 0 or greater than 9.</exception>
    public static string NumberClassString(int nc) {
      return EDecimals.NumberClassString(nc);
    }

    /// <summary>Finds the number class for an arbitrary-precision binary
    /// number object.</summary>
    /// <param name='ed'>An arbitrary-precision binary number
    /// object.</param>
    /// <param name='ec'>A context object that specifies the precision and
    /// exponent range of arbitrary-precision numbers. This is used only to
    /// distinguish between normal and subnormal numbers. Can be
    /// null.</param>
    /// <returns>A 32-bit signed integer identifying the given number
    /// object, number class as follows: 0 = positive normal; 1 = negative
    /// normal, 2 = positive subnormal, 3 = negative subnormal, 4 =
    /// positive zero, 5 = negative zero, 6 = positive infinity, 7 =
    /// negative infinity, 8 = quiet not-a-number (NaN), 9 = signaling
    /// NaN.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='ed'/> is null.</exception>
    public static int NumberClass(EFloat ed, EContext ec) {
      if (ed == null) {
        throw new ArgumentNullException(nameof(ed));
      }
      if (ed.IsQuietNaN()) {
        return 8;
      }
      if (ed.IsNaN()) {
        return 9;
      }
      if (ed.IsInfinity()) {
        return ed.IsNegative ? 7 : 6;
      }
      if (ed.IsZero) {
        return ed.IsNegative ? 5 : 4;
      }
      return IsSubnormal(ed, ec) ? (ed.IsNegative ? 3 : 2) :
        (ed.IsNegative ? 1 : 0);
    }

    /// <summary>Returns whether the given number is a
    /// <i>subnormal</i> number. A
    /// <i>subnormal number</i> is a nonzero finite number whose Exponent
    /// property (or the number's exponent when that number is expressed in
    /// scientific notation with one digit before the radix point) is less
    /// than the minimum possible exponent for that number.</summary>
    /// <param name='ed'>An arbitrary-precision number object.</param>
    /// <param name='ec'>A context specifying the exponent range of
    /// arbitrary-precision numbers. Can be null. If AdjustExponent of the
    /// given context is <c>true</c>, a nonzero number is subnormal if the
    /// number's exponent (when that number is expressed in scientific
    /// notation with one nonzero digit before the radix point) is less
    /// than the given context's EMax property (e.g., if EMax is -100,
    /// 2.3456 * 10
    /// <sup>-102</sup> is subnormal, but 2.3456 * 10
    /// <sup>-99</sup> is not). If AdjustExponent of the given context is
    /// <c>false</c>, a nonzero number is subnormal if the number's
    /// Exponent property is less than the given context's EMax property
    /// (e.g., if EMax is -100, 23456 * 10
    /// <sup>-102</sup> is subnormal, but 23456 * 10
    /// <sup>-99</sup> is not).</param>
    /// <returns>Either <c>true</c> if the given number is subnormal, or
    /// <c>false</c> otherwise. Returns <c>false</c> if the given context
    /// is null or HasExponentRange of the given context is <c>false</c>.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='ed'/> is null.</exception>
    public static bool IsSubnormal(EFloat ed, EContext ec) {
      if (ed == null) {
        throw new ArgumentNullException(nameof(ed));
      }
      if (ed.IsFinite && ec != null && !ed.IsZero && ec.HasExponentRange) {
        if (ec.AdjustExponent) {
          return ed.Exponent.Add(ed.Precision().Subtract(1)).CompareTo(
              ec.EMin) < 0;
        } else {
          return ed.Exponent.CompareTo(ec.EMin) < 0;
        }
      }
      return false;
    }

    /// <summary>Returns whether the given arbitrary-precision number
    /// object is zero (positive zero or negative zero).</summary>
    /// <param name='ed'>An arbitrary-precision number object.</param>
    /// <returns><c>true</c> if the given number has a value of zero
    /// (positive zero or negative zero); otherwise, <c>false</c>.</returns>
    public static bool IsZero(EFloat ed) {
      return ed != null && ed.IsZero;
    }

    /// <summary>Returns the base-2 exponent of an arbitrary-precision
    /// binary number (when that number is expressed in scientific notation
    /// with one nonzero digit before the radix point). For example,
    /// returns 3 for the numbers <c>1.11b * 2^3</c> and <c>111 * 2^1</c>.</summary>
    /// <param name='ed'>An arbitrary-precision binary number.</param>
    /// <param name='ec'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. Can be null.</param>
    /// <returns>The base-2 exponent of the given number (when that number
    /// is expressed in scientific notation with one nonzero digit before
    /// the radix point). Signals DivideByZero and returns negative
    /// infinity if <paramref name='ed'/> is zero. Returns positive
    /// infinity if <paramref name='ed'/> is positive infinity or negative
    /// infinity.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='ed'/> is null.</exception>
    public static EFloat LogB(EFloat ed, EContext ec) {
      if (ed == null) {
        throw new ArgumentNullException(nameof(ed));
      }
      if (ed.IsNaN()) {
        return ed.RoundToPrecision(ec);
      }
      if (ed.IsInfinity()) {
        return EFloat.PositiveInfinity;
      }
      if (ed.IsZero) {
        return EFloat.FromInt32(-1).Divide(EFloat.Zero, ec);
      }
      EInteger ei = ed.Exponent.Add(ed.Precision().Subtract(1));
      return EFloat.FromEInteger(ei).RoundToPrecision(ec);
    }

    /// <summary>Finds an arbitrary-precision binary number whose binary
    /// point is moved a given number of places.</summary>
    /// <param name='ed'>An arbitrary-precision binary number.</param>
    /// <param name='ed2'>The number of binary places to move the binary
    /// point of "ed". This must be an integer with an exponent of
    /// 0.</param>
    /// <param name='ec'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. Can be null.</param>
    /// <returns>The given arbitrary-precision binary number whose binary
    /// point is moved the given number of places. Signals an invalid
    /// operation and returns not-a-number (NaN) if <paramref name='ed2'/>
    /// is infinity or NaN, has an Exponent property other than 0. Signals
    /// an invalid operation and returns not-a-number (NaN) if <paramref
    /// name='ec'/> defines a limited precision and exponent range and if
    /// <paramref name='ed2'/> 's absolute value is greater than twice the
    /// sum of the context's EMax property and its Precision
    /// property.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='ed'/> or <paramref name='ed2'/> is null.</exception>
    public static EFloat ScaleB(EFloat ed, EFloat ed2, EContext ec) {
      if (ed == null) {
        throw new ArgumentNullException(nameof(ed));
      }
      if (ed2 == null) {
        throw new ArgumentNullException(nameof(ed2));
      }
      if (ed.IsNaN() || ed2.IsNaN()) {
        return ed.Add(ed2, ec);
      }
      if (!ed2.IsFinite || ed2.Exponent.Sign != 0) {
        return InvalidOperation(ec);
      }
      EInteger scale = ed2.Mantissa;
      if (ec != null && ec.HasMaxPrecision && ec.HasExponentRange) {
        EInteger exp = ec.EMax.Add(ec.Precision).Multiply(2);
        if (scale.Abs().CompareTo(exp.Abs()) > 0) {
          return InvalidOperation(ec);
        }
      }
      if (ed.IsInfinity()) {
        return ed;
      }
      if (scale.IsZero) {
        return ed.RoundToPrecision(ec);
      }
      EFloat ret = EFloat.Create(
          ed.UnsignedMantissa,
          ed.Exponent.Add(scale));
      if (ed.IsNegative) {
        ret = ret.Negate();
      }
      return ret.RoundToPrecision(ec);
    }

    /// <summary>Shifts the bits of an arbitrary-precision binary floating
    /// point number's significand.</summary>
    /// <param name='ed'>An arbitrary-precision binary floating point
    /// number containing the significand to shift.</param>
    /// <param name='ed2'>An arbitrary-precision number indicating the
    /// number of bits to shift the first operand's significand. Must be an
    /// integer with an exponent of 0. If this parameter is positive, the
    /// significand is shifted to the left by the given number of bits. If
    /// this parameter is negative, the significand is shifted to the right
    /// by the given number of bits.</param>
    /// <param name='ec'>An arithmetic context to control the precision of
    /// arbitrary-precision numbers. Can be null.</param>
    /// <returns>An arbitrary-precision binary number whose significand is
    /// shifted the given number of bits. Signals an invalid operation and
    /// returns NaN (not-a-number) if <paramref name='ed2'/> is a signaling
    /// NaN or if <paramref name='ed2'/> is not an integer, is negative,
    /// has an exponent other than 0, or has an absolute value that exceeds
    /// the maximum precision specified in the context.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='ed'/> or <paramref name='ed2'/> is null.</exception>
    public static EFloat Shift(EFloat ed, EFloat ed2, EContext ec) {
      if (ed == null) {
        throw new ArgumentNullException(nameof(ed));
      }
      if (ed2 == null) {
        throw new ArgumentNullException(nameof(ed2));
      }
      if (ed.IsNaN() || ed2.IsNaN()) {
        return ed.Add(ed2, ec);
      }
      if (!ed2.IsFinite || ed2.Exponent.Sign != 0) {
        return InvalidOperation(ec);
      }
      EInteger shift = ed2.Mantissa;
      if (ec != null) {
        if (shift.Abs().CompareTo(ec.Precision) > 0) {
          return InvalidOperation(ec);
        }
      }
      if (ed.IsInfinity()) {
        // NOTE: Must check for validity of second
        // parameter first, before checking if first
        // parameter is infinity here
        return ed;
      }
      EInteger mant = ed.UnsignedMantissa;
      if (mant.IsZero) {
        return ed.RoundToPrecision(ec);
      }
      EInteger mantprec = ed.Precision();
      if (shift.Sign < 0) {
        if (shift.Abs().CompareTo(mantprec) < 0) {
          EInteger divisor = EInteger.One.ShiftLeft(shift.Abs());
          mant = mant.Divide(divisor);
        } else {
          mant = EInteger.Zero;
        }
        EFloat ret = EFloat.Create(mant, ed.Exponent);
        return ed.IsNegative ? ret.Negate() : ret;
      } else {
        EInteger mult = EInteger.One.ShiftLeft(shift);
        mant = mant.Multiply(mult);
        if (ec != null && ec.HasMaxPrecision) {
          EInteger mod = EInteger.One.ShiftLeft(ec.Precision);
          mant = mant.Remainder(mod);
        }
        EFloat ret = EFloat.Create(mant, ed.Exponent);
        return ed.IsNegative ? ret.Negate() : ret;
      }
    }

    /// <summary>Rotates the bits of an arbitrary-precision binary number's
    /// significand.</summary>
    /// <param name='ed'>An arbitrary-precision number containing the
    /// significand to rotate. If this significand contains more bits than
    /// the precision, the most-significant bits are chopped off the
    /// significand.</param>
    /// <param name='ed2'>An arbitrary-precision number indicating the
    /// number of bits to rotate the first operand's significand. Must be
    /// an integer with an exponent of 0. If this parameter is positive,
    /// the significand is shifted to the left by the given number of bits
    /// and the most-significant bits shifted out of the significand become
    /// the least-significant bits instead. If this parameter is negative,
    /// the number is shifted by the given number of bits and the
    /// least-significant bits shifted out of the significand become the
    /// most-significant bits instead.</param>
    /// <param name='ec'>An arithmetic context to control the precision of
    /// arbitrary-precision numbers. If this parameter is null or specifies
    /// an unlimited precision, this method has the same behavior as
    /// <c>Shift</c>.</param>
    /// <returns>An arbitrary-precision binary number whose significand is
    /// rotated the given number of bits. Signals an invalid operation and
    /// returns NaN (not-a-number) if <paramref name='ed2'/> is a signaling
    /// NaN or if <paramref name='ed2'/> is not an integer, is negative,
    /// has an exponent other than 0, or has an absolute value that exceeds
    /// the maximum precision specified in the context.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='ed2'/> or <paramref name='ed'/> is null.</exception>
    public static EFloat Rotate(EFloat ed, EFloat ed2, EContext ec) {
      if (ec == null || !ec.HasMaxPrecision) {
        return Shift(ed, ed2, ec);
      }
      if (ed2 == null) {
        throw new ArgumentNullException(nameof(ed2));
      }
      if (ed == null) {
        throw new ArgumentNullException(nameof(ed));
      }
      if (ed.IsNaN() || ed2.IsNaN()) {
        return ed.Add(ed2, ec);
      }
      if (!ed2.IsFinite || ed2.Exponent.Sign != 0) {
        return InvalidOperation(ec);
      }
      EInteger shift = ed2.Mantissa;
      if (shift.Abs().CompareTo(ec.Precision) > 0) {
        return InvalidOperation(ec);
      }
      if (ed.IsInfinity()) {
        // NOTE: Must check for validity of second
        // parameter first, before checking if first
        // parameter is infinity here
        return ed;
      }
      EInteger mant = ed.UnsignedMantissa;
      EInteger mantprec = ed.Precision();
      if (ec != null && ec.HasMaxPrecision && mantprec.CompareTo(
  ec.Precision) > 0) {
        mant = mant.Remainder(EInteger.One.ShiftLeft(ec.Precision));
        mantprec = ec.Precision;
      }
      if (mant.IsZero) {
        return ed.RoundToPrecision(ec);
      }
      EInteger rightShift = shift.Sign < 0 ? shift.Abs() :
        ec.Precision.Subtract(shift);
      EInteger leftShift = ec.Precision.Subtract(rightShift);
      EInteger mantRight = EInteger.Zero;
      EInteger mantLeft = EInteger.Zero;
      // Right shift
      if (rightShift.CompareTo(mantprec) < 0) {
        EInteger divisor = EInteger.One.ShiftLeft(rightShift);
        mantRight = mant.Divide(divisor);
      } else {
        mantRight = EInteger.Zero;
      }
      // Left shift
      if (leftShift.IsZero) {
        mantLeft = mant;
      } else if (leftShift.CompareTo(ec.Precision) == 0) {
        mantLeft = EInteger.Zero;
      } else {
        EInteger mult = EInteger.One.ShiftLeft(leftShift);
        mantLeft = mant.Multiply(mult);
        EInteger mod = EInteger.One.ShiftLeft(ec.Precision);
        mantLeft = mantLeft.Remainder(mod);
      }
      EFloat ret = EFloat.Create(mantRight.Add(mantLeft), ed.Exponent);
      return ed.IsNegative ? ret.Negate() : ret;
    }

    /// <summary>Compares the values of one arbitrary-precision number
    /// object and another object, imposing a total ordering on all
    /// possible values. In this method:
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
    /// <param name='ed'>The first arbitrary-precision number to
    /// compare.</param>
    /// <param name='other'>The second arbitrary-precision number to
    /// compare.</param>
    /// <param name='ec'>An arithmetic context. Flags will be set in this
    /// context only if <c>HasFlags</c> and <c>IsSimplified</c> of the
    /// context are true and only if an operand needed to be rounded before
    /// carrying out the operation. Can be null.</param>
    /// <returns>The number 0 if both objects are null or equal, or -1 if
    /// the first object is null or less than the other object, or 1 if the
    /// first object is greater or the other object is null. Does not
    /// signal flags if either value is signaling NaN.</returns>
    public static int CompareTotal(EFloat ed, EFloat other, EContext ec) {
      return (ed == null) ? (other == null ? 0 : -1) : ((other == null) ? 1 :
          ed.CompareToTotal(other, ec));
    }

    /// <summary>Compares the absolute values of two arbitrary-precision
    /// number objects, imposing a total ordering on all possible values
    /// (ignoring their signs). In this method:
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
    /// <param name='ed'>The first arbitrary-precision number to
    /// compare.</param>
    /// <param name='other'>The second arbitrary-precision number to
    /// compare.</param>
    /// <param name='ec'>An arithmetic context. Flags will be set in this
    /// context only if <c>HasFlags</c> and <c>IsSimplified</c> of the
    /// context are true and only if an operand needed to be rounded before
    /// carrying out the operation. Can be null.</param>
    /// <returns>The number 0 if both objects are null or equal (ignoring
    /// their signs), or -1 if the first object is null or less than the
    /// other object (ignoring their signs), or 1 if the first object is
    /// greater (ignoring their signs) or the other object is null. Does
    /// not signal flags if either value is signaling NaN.</returns>
    public static int CompareTotalMagnitude(
      EFloat ed,
      EFloat other,
      EContext ec) {
      return (ed == null) ? (other == null ? 0 : -1) : ((other == null) ? 1 :
          ed.CompareToTotalMagnitude(other, ec));
    }

    /// <summary>Creates a copy of the given arbitrary-precision number
    /// object.</summary>
    /// <param name='ed'>An arbitrary-precision number object to
    /// copy.</param>
    /// <returns>A copy of the given arbitrary-precision number
    /// object.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='ed'/> is null.</exception>
    public static EFloat Copy(EFloat ed) {
      if (ed == null) {
        throw new ArgumentNullException(nameof(ed));
      }
      return ed.Copy();
    }

    /// <summary>Returns a canonical version of the given
    /// arbitrary-precision number object. In this method, this method
    /// behaves like the Copy method.</summary>
    /// <param name='ed'>An arbitrary-precision number object.</param>
    /// <returns>A copy of the parameter <paramref name='ed'/>.</returns>
    public static EFloat Canonical(EFloat ed) {
      return Copy(ed);
    }

    /// <summary>Returns an arbitrary-precision number object with the same
    /// value as the given number object but with a nonnegative sign (that
    /// is, the given number object's absolute value).</summary>
    /// <param name='ed'>An arbitrary-precision number object.</param>
    /// <returns>An arbitrary-precision number object with the same value
    /// as the given number object but with a nonnegative sign.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='ed'/> is null.</exception>
    public static EFloat CopyAbs(EFloat ed) {
      if (ed == null) {
        throw new ArgumentNullException(nameof(ed));
      }
      return Copy(ed.Abs());
    }

    /// <summary>Returns an arbitrary-precision number object with the sign
    /// reversed from the given number object.</summary>
    /// <param name='ed'>An arbitrary-precision number object.</param>
    /// <returns>An arbitrary-precision number object with the sign
    /// reversed from the given number object.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='ed'/> is null.</exception>
    public static EFloat CopyNegate(EFloat ed) {
      if (ed == null) {
        throw new ArgumentNullException(nameof(ed));
      }
      return Copy(ed.Negate());
    }

    /// <summary>Returns an arbitrary-precision number object with the same
    /// value as the first given number object but with a the same sign
    /// (positive or negative) as the second given number object.</summary>
    /// <param name='ed'>An arbitrary-precision number object with the
    /// value the result will have.</param>
    /// <param name='other'>The parameter <paramref name='other'/> is an
    /// arbitrary-precision binary floating-point number.</param>
    /// <returns>An arbitrary-precision number object with the same value
    /// as the first given number object but with a the same sign (positive
    /// or negative) as the second given number object.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='ed'/> or <paramref name='other'/> is null.</exception>
    public static EFloat CopySign(EFloat ed, EFloat other) {
      if (ed == null) {
        throw new ArgumentNullException(nameof(ed));
      }
      if (other == null) {
        throw new ArgumentNullException(nameof(other));
      }
      return ed.IsNegative == other.IsNegative ? Copy(ed) : CopyNegate(ed);
    }

    private static EFloat InvalidOperation(EContext ec) {
      return EFloat.SignalingNaN.Plus(ec);
    }

    /// <summary>Returns whether two arbitrary-precision numbers have the
    /// same exponent, they both are not-a-number (NaN), or they both are
    /// infinity (positive and/or negative).</summary>
    /// <param name='ed1'>The first arbitrary-precision number.</param>
    /// <param name='ed2'>The second arbitrary-precision number.</param>
    /// <returns>Either <c>true</c> if the given arbitrary-precision
    /// numbers have the same exponent, they both are not-a-number (NaN),
    /// or they both are infinity (positive and/or negative); otherwise,
    /// <c>false</c>.</returns>
    public static bool SameQuantum(EFloat ed1, EFloat ed2) {
      if (ed1 == null || ed2 == null) {
        return false;
      }
      if (ed1.IsFinite && ed2.IsFinite) {
        return ed1.Exponent.Equals(ed2.Exponent);
      } else {
        return (ed1.IsNaN() && ed2.IsNaN()) || (ed1.IsInfinity() &&
            ed2.IsInfinity());
      }
    }

    /// <summary>Returns an arbitrary-precision number with the same value
    /// as this one but with certain trailing zeros removed from its
    /// significand. If the number's exponent is 0, it is returned
    /// unchanged (but may be rounded depending on the arithmetic context);
    /// if that exponent is greater 0, its trailing zeros are removed from
    /// the significand (then rounded if necessary); if that exponent is
    /// less than 0, its trailing zeros are removed from the significand
    /// until the exponent reaches 0 (then the number is rounded if
    /// necessary).</summary>
    /// <param name='ed1'>An arbitrary-precision number.</param>
    /// <param name='ec'>An arithmetic context to control the precision,
    /// rounding, and exponent range of the result. Can be null.</param>
    /// <returns>An arbitrary-precision number with the same value as this
    /// one but with certain trailing zeros removed from its significand.
    /// If <paramref name='ed1'/> is not-a-number (NaN) or infinity, it is
    /// generally returned unchanged.</returns>
    public static EFloat Trim(EFloat ed1, EContext ec) {
      EFloat ed = ed1;
      if (ed1 == null) {
        return InvalidOperation(ec);
      }
      if (ed.IsSignalingNaN()) {
        return EFloat.CreateNaN(
          ed.UnsignedMantissa,
          true,
          ed.IsNegative,
          ec);
      }
      if (ed.IsFinite) {
        if (ed.IsZero) {
          return (ed.IsNegative ? EFloat.NegativeZero :
              EFloat.Zero).RoundToPrecision(ec);
        } else if (ed.Exponent.Sign > 0) {
          return ed.Reduce(ec);
        } else if (ed.Exponent.Sign == 0) {
          return ed.RoundToPrecision(ec);
        } else {
          EInteger exp = ed.Exponent;
          EInteger mant = ed.UnsignedMantissa;
          bool neg = ed.IsNegative;
          var trimmed = false;
          EInteger radixint = EInteger.FromInt32(BinaryRadix);
          while (exp.Sign < 0 && mant.Sign > 0) {
            EInteger[] divrem = mant.DivRem(radixint);
            int rem = divrem[1].ToInt32Checked();
            if (rem != 0) {
              break;
            }
            mant = divrem[0];
            exp = exp.Add(1);
            trimmed = true;
          }
          if (!trimmed) {
            return ed.RoundToPrecision(ec);
          }
          EFloat ret = EFloat.Create(mant, exp);
          if (neg) {
            ret = ret.Negate();
          }
          return ret.RoundToPrecision(ec);
        }
      } else {
        return ed1.Plus(ec);
      }
    }

    /// <summary>Returns an arbitrary-precision binary number with the same
    /// value as this object but with the given exponent, expressed as an
    /// arbitrary-precision binary number.
    /// <para>Note that this is not always the same as rounding to a given
    /// number of binary places, since it can fail if the difference
    /// between this value's exponent and the desired exponent is too big,
    /// depending on the maximum precision. If rounding to a number of
    /// binary places is desired, it's better to use the RoundToExponent
    /// and RoundToIntegral methods instead.</para>
    /// <para><b>Remark:</b> This method can be used to implement
    /// fixed-point binary arithmetic, in which a fixed number of digits
    /// come after the binary point. A fixed-point binary arithmetic in
    /// which no digits come after the binary point (a desired exponent of
    /// 0) is considered an "integer arithmetic" .</para></summary>
    /// <param name='ed'>An arbitrary-precision binary number whose
    /// exponent is to be changed.</param>
    /// <param name='scale'>The desired exponent of the result, expressed
    /// as an arbitrary-precision binary number. The exponent is the number
    /// of fractional digits in the result, expressed as a negative number.
    /// Can also be positive, which eliminates lower-order places from the
    /// number. For example, -3 means round to the sixteenth (10b^-3,
    /// 0.0001b), and 3 means round to the sixteens-place (10b^3, 1000b). A
    /// value of 0 rounds the number to an integer.</param>
    /// <param name='ec'>An arithmetic context to control precision and
    /// rounding of the result. If <c>HasFlags</c> of the context is true,
    /// will also store the flags resulting from the operation (the flags
    /// are in addition to the pre-existing flags). Can be null, in which
    /// case the default rounding mode is HalfEven.</param>
    /// <returns>An arbitrary-precision binary number with the same value
    /// as this object but with the exponent changed. Signals FlagInvalid
    /// and returns not-a-number (NaN) if the result can't fit the given
    /// precision without rounding, or if the arithmetic context defines an
    /// exponent range and the given exponent is outside that
    /// range.</returns>
    public static EFloat Rescale(EFloat ed, EFloat scale, EContext ec) {
      if (ed == null || scale == null) {
        return InvalidOperation(ec);
      }
      if (!scale.IsFinite) {
        return ed.Quantize(scale, ec);
      }
      if (scale.Exponent.IsZero) {
        return ed.Quantize(EFloat.Create(EInteger.One, scale.Mantissa), ec);
      } else {
        EContext tec = ec == null ? null : ec.WithTraps(0).WithBlankFlags();
        EFloat rv = scale.RoundToExponentExact(0, tec);
        if (!rv.IsFinite || (tec.Flags & EContext.FlagInexact) != 0) {
          if (ec != null && ec.IsSimplified) {
            // In simplified arithmetic, round scale to trigger
            // appropriate error conditions
            scale = scale.RoundToPrecision(ec);
          }
          return InvalidOperation(ec);
        }
        EFloat rounded = scale.Quantize(0, tec);
        return ed.Quantize(
            EFloat.Create(EInteger.One, rounded.Mantissa),
            ec);
      }
    }

    // Logical Operations

    /// <summary>Performs a logical AND operation on two binary numbers in
    /// the form of
    /// <i>logical operands</i>. A <c>logical operand</c> is a
    /// non-negative base-2 number with an Exponent property of 0 (examples
    /// include the base-2 numbers <c>01001</c> and <c>111001</c> ). The
    /// logical AND operation sets each bit of the result to 1 if the
    /// corresponding bits of each logical operand are both 1, and to 0
    /// otherwise. For example, <c>01001 AND 111010=01000</c>.</summary>
    /// <param name='ed1'>The first logical operand to the logical AND
    /// operation.</param>
    /// <param name='ed2'>The second logical operand to the logical AND
    /// operation.</param>
    /// <param name='ec'>An arithmetic context to control the maximum
    /// precision of arbitrary-precision numbers. If a logical operand
    /// passed to this method has more bits than the maximum precision
    /// specified in this context, the operand's most significant bits that
    /// exceed that precision are discarded. This parameter can be
    /// null.</param>
    /// <returns>The result of the logical AND operation as a logical
    /// operand. Signals an invalid operation and returns not-a-number
    /// (NaN) if <paramref name='ed1'/>, <paramref name='ed2'/>, or both
    /// are not logical operands.</returns>
    public static EFloat And(EFloat ed1, EFloat ed2, EContext ec) {
      byte[] logi1 = EDecimals.FromLogical(ed1, ec, 2);
      if (logi1 == null) {
        return InvalidOperation(ec);
      }
      byte[] logi2 = EDecimals.FromLogical(ed2, ec, 2);
      if (logi2 == null) {
        return InvalidOperation(ec);
      }
      byte[] smaller = logi1.Length < logi2.Length ? logi1 : logi2;
      byte[] bigger = logi1.Length < logi2.Length ? logi2 : logi1;
      for (var i = 0; i < smaller.Length; ++i) {
        smaller[i] &= bigger[i];
      }
      return EFloat.FromEInteger(
          EDecimals.ToLogical(
            smaller,
            2)).RoundToPrecision(ec);
    }

    /// <summary>Performs a logical NOT operation on a binary number in the
    /// form of a
    /// <i>logical operand</i>. A <c>logical operand</c> is a non-negative
    /// base-2 number with an Exponent property of 0 (examples include
    /// <c>01001</c> and <c>111001</c> ). The logical NOT operation sets
    /// each bit of the result to 1 if the corresponding bit is 0, and to 0
    /// otherwise; it can set no more bits than the maximum precision,
    /// however. For example, if the maximum precision is 8 bits, then
    /// <c>NOT 111010=11000101</c>.</summary>
    /// <param name='ed1'>The operand to the logical NOT operation.</param>
    /// <param name='ec'>An arithmetic context to control the maximum
    /// precision of arbitrary-precision numbers. If a logical operand
    /// passed to this method has more bits than the maximum precision
    /// specified in this context, the operand's most significant bits that
    /// exceed that precision are discarded. This parameter cannot be null
    /// and must specify a maximum precision (unlimited precision contexts
    /// are not allowed).</param>
    /// <returns>The result of the logical NOT operation as a logical
    /// operand. Signals an invalid operation and returns not-a-number
    /// (NaN) if <paramref name='ed1'/> is not a logical operand.</returns>
    public static EFloat Invert(EFloat ed1, EContext ec) {
      if (ec == null || !ec.HasMaxPrecision) {
        return InvalidOperation(ec);
      }
      EInteger ei = EInteger.One.ShiftLeft(ec.Precision).Subtract(1);
      byte[] smaller = EDecimals.FromLogical(ed1, ec, 2);
      if (smaller == null) {
        return InvalidOperation(ec);
      }
      byte[] bigger = ei.ToBytes(true);
      #if DEBUG
      if (smaller.Length > bigger.Length) {
        throw new ArgumentException("smaller.Length(" + smaller.Length +
          ") is not less or equal to " + bigger.Length);
      }
      #endif

      for (var i = 0; i < smaller.Length; ++i) {
        bigger[i] ^= smaller[i];
      }
      return EFloat.FromEInteger(
          EDecimals.ToLogical(
            bigger,
            2)).RoundToPrecision(ec);
    }

    /// <summary>Performs a logical exclusive-OR (XOR) operation on two
    /// binary numbers in the form of
    /// <i>logical operands</i>. A <c>logical operand</c> is a
    /// non-negative base-2 number with an Exponent property of 0 (examples
    /// include the base-2 numbers <c>01001</c> and <c>111001</c> ). The
    /// logical exclusive-OR operation sets each digit of the result to 1
    /// if either corresponding digit of the logical operands, but not
    /// both, is 1, and to 0 otherwise. For example, <c>01001 XOR 111010 =
    /// 101010</c>.</summary>
    /// <param name='ed1'>The first logical operand to the logical
    /// exclusive-OR operation.</param>
    /// <param name='ed2'>The second logical operand to the logical
    /// exclusive-OR operation.</param>
    /// <param name='ec'>An arithmetic context to control the maximum
    /// precision of arbitrary-precision numbers. If a logical operand
    /// passed to this method has more bits than the maximum precision
    /// specified in this context, the operand's most significant bits that
    /// exceed that precision are discarded. This parameter can be
    /// null.</param>
    /// <returns>The result of the logical exclusive-OR operation as a
    /// logical operand. Signals an invalid operation and returns
    /// not-a-number (NaN) if <paramref name='ed1'/>, <paramref
    /// name='ed2'/>, or both are not logical operands.</returns>
    public static EFloat Xor(EFloat ed1, EFloat ed2, EContext ec) {
      byte[] logi1 = EDecimals.FromLogical(ed1, ec, 2);
      if (logi1 == null) {
        return InvalidOperation(ec);
      }
      byte[] logi2 = EDecimals.FromLogical(ed2, ec, 2);
      if (logi2 == null) {
        return InvalidOperation(ec);
      }
      byte[] smaller = logi1.Length < logi2.Length ? logi1 : logi2;
      byte[] bigger = logi1.Length < logi2.Length ? logi2 : logi1;
      for (var i = 0; i < smaller.Length; ++i) {
        bigger[i] ^= smaller[i];
      }
      return EFloat.FromEInteger(
          EDecimals.ToLogical(
            bigger,
            2)).RoundToPrecision(ec);
    }

    /// <summary>Performs a logical OR operation on two binary numbers in
    /// the form of
    /// <i>logical operands</i>. A <c>logical operand</c> is a
    /// non-negative base-2 number with an Exponent property of 0 (examples
    /// include the base-2 numbers <c>01001</c> and <c>111001</c> ). The
    /// logical OR operation sets each bit of the result to 1 if either or
    /// both of the corresponding bits of each logical operand are 1, and
    /// to 0 otherwise. For example, <c>01001 OR 111010=111011</c>.</summary>
    /// <param name='ed1'>The first logical operand to the logical OR
    /// operation.</param>
    /// <param name='ed2'>The second logical operand to the logical OR
    /// operation.</param>
    /// <param name='ec'>An arithmetic context to control the maximum
    /// precision of arbitrary-precision numbers. If a logical operand
    /// passed to this method has more bits than the maximum precision
    /// specified in this context, the operand's most significant bits that
    /// exceed that precision are discarded. This parameter can be
    /// null.</param>
    /// <returns>The result of the logical OR operation as a logical
    /// operand. Signals an invalid operation and returns not-a-number
    /// (NaN) if <paramref name='ed1'/>, <paramref name='ed2'/>, or both
    /// are not logical operands.</returns>
    public static EFloat Or(EFloat ed1, EFloat ed2, EContext ec) {
      byte[] logi1 = EDecimals.FromLogical(ed1, ec, 2);
      if (logi1 == null) {
        return InvalidOperation(ec);
      }
      byte[] logi2 = EDecimals.FromLogical(ed2, ec, 2);
      if (logi2 == null) {
        return InvalidOperation(ec);
      }
      byte[] smaller = logi1.Length < logi2.Length ? logi1 : logi2;
      byte[] bigger = logi1.Length < logi2.Length ? logi2 : logi1;
      for (var i = 0; i < smaller.Length; ++i) {
        bigger[i] |= smaller[i];
      }
      return EFloat.FromEInteger(
          EDecimals.ToLogical(
            bigger,
            2)).RoundToPrecision(ec);
    }
  }
}
