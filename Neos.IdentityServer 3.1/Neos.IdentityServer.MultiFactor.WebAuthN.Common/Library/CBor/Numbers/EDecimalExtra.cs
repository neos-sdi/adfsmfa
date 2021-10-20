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
  public sealed partial class EDecimal {
    /// <summary>Converts this value to a <c>decimal</c> under the Common
    /// Language Infrastructure (see
    /// <see cref='PeterO.Numbers.EDecimal'>"Forms of numbers"</see> ),
    /// using the half-even rounding mode.</summary>
    /// <returns>A <c>decimal</c> under the Common Language Infrastructure
    /// (usually a.NET Framework decimal).</returns>
    public decimal ToDecimal() {
      EDecimal extendedNumber = this;
      if (extendedNumber.IsInfinity() || extendedNumber.IsNaN()) {
        throw new OverflowException("This object's value is out of range");
      }
      decimal ret;
      System.Globalization.NumberStyles
      ns = System.Globalization.NumberStyles.Number |
        System.Globalization.NumberStyles.AllowExponent;
      if (
        Decimal.TryParse(
          this.ToString(),
          ns,
          System.Globalization.CultureInfo.InvariantCulture,
          out ret)) {
        return ret;
      }
      throw new OverflowException("This object's value is out of range");
    }

    /// <summary>Converts a <c>decimal</c> under the Common Language
    /// Infrastructure (see
    /// <see cref='PeterO.Numbers.EDecimal'>"Forms of numbers"</see> ) to
    /// an arbitrary-precision decimal.</summary>
    /// <param name='dec'>A <c>decimal</c> under the Common Language
    /// Infrastructure (usually a.NET Framework decimal).</param>
    /// <returns>An arbitrary-precision decimal floating-point
    /// number.</returns>
    public static EDecimal FromDecimal(decimal dec) {
      return

        EDecimal.FromString(
          dec.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    /// <summary>Converts a boolean value (true or false) to an arbitrary
    /// precision decimal.</summary>
    /// <param name='boolValue'>Either true or false.</param>
    /// <returns>The number 1 if <paramref name='boolValue'/> is true;
    /// otherwise, 0.</returns>
    public static explicit operator EDecimal(bool boolValue) {
      return FromBoolean(boolValue);
    }

    /// <summary>Converts an arbitrary-precision decimal number to a
    /// <c>decimal</c> under the Common Language Infrastructure (see
    /// <see cref='PeterO.Numbers.EDecimal'>"Forms of numbers"</see> ),
    /// using the half-even rounding mode.</summary>
    /// <param name='dec'>The number to convert as an arbitrary-precision
    /// decimal floating-point number.</param>
    /// <returns>A <c>decimal</c> under the Common Language Infrastructure
    /// (usually a.NET Framework decimal).</returns>
    public static implicit operator EDecimal(decimal dec) {
      return FromDecimal(dec);
    }

    /// <summary>Converts an arbitrary-precision decimal's value to a
    /// <c>decimal</c> under the Common Language Infrastructure (see
    /// <see cref='PeterO.Numbers.EDecimal'>"Forms of numbers"</see> ),
    /// using the half-even rounding mode.</summary>
    /// <param name='bigValue'>The parameter <paramref name='bigValue'/> is
    /// an arbitrary-precision decimal floating-point number.</param>
    /// <returns>A <c>decimal</c> under the Common Language Infrastructure
    /// (usually a.NET Framework decimal).</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bigValue'/> is null.</exception>
    public static explicit operator decimal(EDecimal bigValue) {
      if (bigValue == null) {
        throw new ArgumentNullException(nameof(bigValue));
      }
      return bigValue.ToDecimal();
    }

    /// <summary>Converts an arbitrary-precision integer to an arbitrary
    /// precision decimal.</summary>
    /// <param name='eint'>An arbitrary-precision integer.</param>
    /// <returns>An arbitrary-precision decimal number with the exponent
    /// set to 0.</returns>
    public static implicit operator EDecimal(EInteger eint) {
      return FromEInteger(eint);
    }

    /// <summary>Adds an arbitrary-precision decimal floating-point number
    /// and another arbitrary-precision decimal floating-point number and
    /// returns the result.</summary>
    /// <param name='bthis'>The first arbitrary-precision decimal
    /// floating-point number.</param>
    /// <param name='otherValue'>The second decimal binary floating-point
    /// number.</param>
    /// <returns>The sum of the two numbers, that is, an
    /// arbitrary-precision decimal floating-point number plus another
    /// arbitrary-precision decimal floating-point number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bthis'/> or <paramref name='otherValue'/> is
    /// null.</exception>
    public static EDecimal operator +(EDecimal bthis, EDecimal otherValue) {
      if (bthis == null) {
        throw new ArgumentNullException(nameof(bthis));
      }
      return bthis.Add(otherValue);
    }

    /// <summary>Subtracts one arbitrary-precision decimal number from
    /// another and returns the result.</summary>
    /// <param name='bthis'>The first operand.</param>
    /// <param name='subtrahend'>The second operand.</param>
    /// <returns>The difference of the two decimal numbers.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bthis'/> or <paramref name='subtrahend'/> is
    /// null.</exception>
    public static EDecimal operator -(
      EDecimal bthis,
      EDecimal subtrahend) {
      if (bthis == null) {
        throw new ArgumentNullException(nameof(bthis));
      }
      return bthis.Subtract(subtrahend);
    }

    /// <summary>Adds one to an arbitrary-precision decimal
    /// number.</summary>
    /// <param name='bthis'>An arbitrary-precision decimal number.</param>
    /// <returns>The given arbitrary-precision decimal number plus
    /// one.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bthis'/> is null.</exception>
    public static EDecimal operator ++(EDecimal bthis) {
      if (bthis == null) {
        throw new ArgumentNullException(nameof(bthis));
      }
      return bthis.Add(1);
    }

    /// <summary>Subtracts one from an arbitrary-precision decimal
    /// number.</summary>
    /// <param name='bthis'>An arbitrary-precision decimal number.</param>
    /// <returns>The given arbitrary-precision decimal number minus
    /// one.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bthis'/> is null.</exception>
    public static EDecimal operator --(EDecimal bthis) {
      if (bthis == null) {
        throw new ArgumentNullException(nameof(bthis));
      }
      return bthis.Subtract(1);
    }

    /// <summary>Multiplies an arbitrary-precision decimal floating-point
    /// number by another arbitrary-precision decimal floating-point number
    /// and returns the result.</summary>
    /// <param name='operand1'>The first operand.</param>
    /// <param name='operand2'>The second operand.</param>
    /// <returns>The product of the two numbers, that is, an
    /// arbitrary-precision decimal floating-point number times another
    /// arbitrary-precision decimal floating-point number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='operand1'/> or <paramref name='operand2'/> is
    /// null.</exception>
    public static EDecimal operator *(
      EDecimal operand1,
      EDecimal operand2) {
      if (operand1 == null) {
        throw new ArgumentNullException(nameof(operand1));
      }
      return operand1.Multiply(operand2);
    }

    /// <summary>Divides this object by another decimal number and returns
    /// the result. When possible, the result will be exact.</summary>
    /// <param name='dividend'>The number that will be divided by the
    /// divisor.</param>
    /// <param name='divisor'>The number to divide by.</param>
    /// <returns>The quotient of the two numbers. Returns infinity if the
    /// divisor is 0 and the dividend is nonzero. Returns not-a-number
    /// (NaN) if the divisor and the dividend are 0. Returns NaN if the
    /// result can't be exact because it would have a nonterminating
    /// decimal expansion; examples include 1 divided by any multiple of 3,
    /// such as 1/3 or 1/12. If this is not desired, use DivideToExponent
    /// instead, or use the Divide overload that takes an EContext
    /// instead.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='dividend'/> is null.</exception>
    public static EDecimal operator /(
      EDecimal dividend,
      EDecimal divisor) {
      if (dividend == null) {
        throw new ArgumentNullException(nameof(dividend));
      }
      return dividend.Divide(divisor);
    }

    /// <summary>Returns the remainder that would result when an
    /// arbitrary-precision decimal floating-point number is divided by
    /// another arbitrary-precision decimal floating-point number. The
    /// remainder is the number that remains when the absolute value of an
    /// arbitrary-precision decimal floating-point number is divided (as
    /// though by DivideToIntegerZeroScale) by the absolute value of the
    /// other arbitrary-precision decimal floating-point number; the
    /// remainder has the same sign (positive or negative) as this
    /// arbitrary-precision decimal floating-point number.</summary>
    /// <param name='dividend'>The number that will be divided by the
    /// divisor.</param>
    /// <param name='divisor'>The number to divide by.</param>
    /// <returns>The remainder that would result when an
    /// arbitrary-precision decimal floating-point number is divided by
    /// another arbitrary-precision decimal floating-point
    /// number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='dividend'/> is null.</exception>
    public static EDecimal operator %(
      EDecimal dividend,
      EDecimal divisor) {
      if (dividend == null) {
        throw new ArgumentNullException(nameof(dividend));
      }
      return dividend.Remainder(divisor, null);
    }

    /// <summary>Gets an arbitrary-precision decimal number with the same
    /// value as the given one, but with the sign reversed.</summary>
    /// <param name='bigValue'>An arbitrary-precision decimal number to
    /// negate.</param>
    /// <returns>An arbitrary-precision decimal number. If this value is
    /// positive zero, returns negative zero. Returns signaling NaN if this
    /// value is signaling NaN.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bigValue'/> is null.</exception>
    public static EDecimal operator -(EDecimal bigValue) {
      if (bigValue == null) {
        throw new ArgumentNullException(nameof(bigValue));
      }
      return bigValue.Negate();
    }

    /// <summary>Converts an arbitrary-precision decimal floating-point
    /// number to an arbitrary-precision integer. Any fractional part in
    /// this value will be discarded when converting to an
    /// arbitrary-precision integer.</summary>
    /// <param name='bigValue'>The number to convert as an
    /// arbitrary-precision decimal.</param>
    /// <returns>An arbitrary-precision integer.</returns>
    /// <exception cref='OverflowException'>This object's value is infinity
    /// or not-a-number (NaN).</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bigValue'/> is null.</exception>
    public static explicit operator EInteger(EDecimal bigValue) {
      if (bigValue == null) {
        throw new ArgumentNullException(nameof(bigValue));
      }
      return bigValue.ToEInteger();
    }

    /// <summary>Converts this value to its closest equivalent as a 64-bit
    /// floating-point number. The half-even rounding mode is used.
    /// <para>If this value is a NaN, sets the high bit of the 64-bit
    /// floating point number's significand area for a quiet NaN, and
    /// clears it for a signaling NaN. Then the other bits of the
    /// significand area are set to the lowest bits of this object's
    /// unsigned significand, and the next-highest bit of the significand
    /// area is set if those bits are all zeros and this is a signaling
    /// NaN. Unfortunately, in the.NET implementation, the return value of
    /// this method may be a quiet NaN even if a signaling NaN would
    /// otherwise be generated.</para></summary>
    /// <param name='bigValue'>The value to convert to a 64-bit
    /// floating-point number.</param>
    /// <returns>The closest 64-bit floating-point number to this value.
    /// The return value can be positive infinity or negative infinity if
    /// this value exceeds the range of a 64-bit floating point
    /// number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bigValue'/> is null.</exception>
    public static explicit operator double(EDecimal bigValue) {
      if (bigValue == null) {
        throw new ArgumentNullException(nameof(bigValue));
      }
      return bigValue.ToDouble();
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
    /// <param name='bigValue'>The number to convert as an
    /// arbitrary-precision decimal number.</param>
    /// <returns>The closest 32-bit binary floating-point number to this
    /// value. The return value can be positive infinity or negative
    /// infinity if this value exceeds the range of a 32-bit floating point
    /// number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bigValue'/> is null.</exception>
    public static explicit operator float(EDecimal bigValue) {
      if (bigValue == null) {
        throw new ArgumentNullException(nameof(bigValue));
      }
      return bigValue.ToSingle();
    }

    // Begin integer conversions

    /// <summary>Converts an arbitrary-precision decimal number to a byte
    /// (from 0 to 255) if it can fit in a byte (from 0 to 255) after
    /// converting it to an integer by discarding its fractional
    /// part.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// decimal number.</param>
    /// <returns>The value of <paramref name='input'/>, truncated to a
    /// byte (from 0 to 255).</returns>
    /// <exception cref='OverflowException'>The parameter <paramref
    /// name='input'/> is infinity or not-a-number, or the number, once
    /// converted to an integer by discarding its fractional part, is less
    /// than 0 or greater than 255.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='input'/> is null.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Usage",
      "CA2225",
      Justification = "Class implements an alternate method named ToByteChecked.")]
    public static explicit operator byte(EDecimal input) {
      if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToByteChecked();
    }

    /// <summary>Converts a byte (from 0 to 255) to an arbitrary-precision
    /// decimal number.</summary>
    /// <param name='inputByte'>The number to convert as a byte (from 0 to
    /// 255).</param>
    /// <returns>The value of <paramref name='inputByte'/> as an
    /// arbitrary-precision decimal number.</returns>
    public static implicit operator EDecimal(byte inputByte) {
      return EDecimal.FromByte(inputByte);
    }

    /// <summary>Converts this number's value to an 8-bit signed integer if
    /// it can fit in an 8-bit signed integer after converting it to an
    /// integer by discarding its fractional part.</summary>
    /// <returns>This number's value, truncated to an 8-bit signed
    /// integer.</returns>
    /// <exception cref='OverflowException'>This value is infinity or
    /// not-a-number, or the number, once converted to an integer by
    /// discarding its fractional part, is less than -128 or greater than
    /// 127.</exception>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public sbyte ToSByteChecked()
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      if (this.IsIntegerPartZero()) {
        return (sbyte)0;
      }
      if (this.exponent.CompareToInt(3) >= 0) {
        throw new OverflowException("Value out of range");
      }
      return this.ToEInteger().ToSByteChecked();
    }

    /// <summary>Converts this number's value to an integer by discarding
    /// its fractional part, and returns the least-significant bits of its
    /// two's-complement form as an 8-bit signed integer.</summary>
    /// <returns>This number, converted to an 8-bit signed integer. Returns
    /// 0 if this value is infinity or not-a-number.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public sbyte ToSByteUnchecked()
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            return this.IsFinite ? this.ToEInteger().ToSByteUnchecked() : (sbyte)0;
    }

    /// <summary>Converts this number's value to an 8-bit signed integer if
    /// it can fit in an 8-bit signed integer without rounding to a
    /// different numerical value.</summary>
    /// <returns>This number's value as an 8-bit signed integer.</returns>
    /// <exception cref='ArithmeticException'>This value is infinity or
    /// not-a-number, is not an exact integer, or is less than -128 or
    /// greater than 127.</exception>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public sbyte ToSByteIfExact()
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      if (this.IsZero) {
        return (sbyte)0;
      }
      if (this.exponent.CompareToInt(3) >= 0) {
        throw new OverflowException("Value out of range");
      }
      return this.ToEIntegerIfExact().ToSByteChecked();
    }

    /// <summary>Converts an 8-bit signed integer to an arbitrary-precision
    /// decimal number.</summary>
    /// <param name='inputSByte'>The number to convert as an 8-bit signed
    /// integer.</param>
    /// <returns>This number's value as an arbitrary-precision decimal
    /// number.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static EDecimal FromSByte(sbyte inputSByte)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            var val = (int)inputSByte;
      return FromInt32(val);
    }

    /// <summary>Converts an arbitrary-precision decimal number to an 8-bit
    /// signed integer if it can fit in an 8-bit signed integer after
    /// converting it to an integer by discarding its fractional
    /// part.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// decimal number.</param>
    /// <returns>The value of <paramref name='input'/>, truncated to an
    /// 8-bit signed integer.</returns>
    /// <exception cref='OverflowException'>The parameter <paramref
    /// name='input'/> is infinity or not-a-number, or the number, once
    /// converted to an integer by discarding its fractional part, is less
    /// than -128 or greater than 127.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='input'/> is null.</exception>
    [CLSCompliant(false)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Usage",
      "CA2225",
      Justification = "Class implements an alternate method named ToSByteChecked.")]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static explicit operator sbyte(EDecimal input)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToSByteChecked();
    }

    /// <summary>Converts an 8-bit signed integer to an arbitrary-precision
    /// decimal number.</summary>
    /// <param name='inputSByte'>The number to convert as an 8-bit signed
    /// integer.</param>
    /// <returns>The value of <paramref name='inputSByte'/> as an
    /// arbitrary-precision decimal number.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static implicit operator EDecimal(sbyte inputSByte)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            return EDecimal.FromSByte(inputSByte);
    }

    /// <summary>Converts an arbitrary-precision decimal number to a 16-bit
    /// signed integer if it can fit in a 16-bit signed integer after
    /// converting it to an integer by discarding its fractional
    /// part.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// decimal number.</param>
    /// <returns>The value of <paramref name='input'/>, truncated to a
    /// 16-bit signed integer.</returns>
    /// <exception cref='OverflowException'>The parameter <paramref
    /// name='input'/> is infinity or not-a-number, or the number, once
    /// converted to an integer by discarding its fractional part, is less
    /// than -32768 or greater than 32767.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='input'/> is null.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Usage",
      "CA2225",
      Justification = "Class implements an alternate method named ToInt16Checked.")]
    public static explicit operator short(EDecimal input) {
      if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToInt16Checked();
    }

    /// <summary>Converts a 16-bit signed integer to an arbitrary-precision
    /// decimal number.</summary>
    /// <param name='inputInt16'>The number to convert as a 16-bit signed
    /// integer.</param>
    /// <returns>The value of <paramref name='inputInt16'/> as an
    /// arbitrary-precision decimal number.</returns>
    public static implicit operator EDecimal(short inputInt16) {
      return EDecimal.FromInt16(inputInt16);
    }

    /// <summary>Converts this number's value to a 16-bit unsigned integer
    /// if it can fit in a 16-bit unsigned integer after converting it to
    /// an integer by discarding its fractional part.</summary>
    /// <returns>This number's value, truncated to a 16-bit unsigned
    /// integer.</returns>
    /// <exception cref='OverflowException'>This value is infinity or
    /// not-a-number, or the number, once converted to an integer by
    /// discarding its fractional part, is less than 0 or greater than
    /// 65535.</exception>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public ushort ToUInt16Checked()
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      if (this.IsIntegerPartZero()) {
        return (ushort)0;
      }
      if (this.exponent.CompareToInt(5) >= 0) {
        throw new OverflowException("Value out of range");
      }
      return this.ToEInteger().ToUInt16Checked();
    }

    /// <summary>Converts this number's value to an integer by discarding
    /// its fractional part, and returns the least-significant bits of its
    /// two's-complement form as a 16-bit unsigned integer.</summary>
    /// <returns>This number, converted to a 16-bit unsigned integer.
    /// Returns 0 if this value is infinity or not-a-number.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public ushort ToUInt16Unchecked()
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            return this.IsFinite ? this.ToEInteger().ToUInt16Unchecked() : (ushort)0;
    }

    /// <summary>Converts this number's value to a 16-bit unsigned integer
    /// if it can fit in a 16-bit unsigned integer without rounding to a
    /// different numerical value.</summary>
    /// <returns>This number's value as a 16-bit unsigned
    /// integer.</returns>
    /// <exception cref='ArithmeticException'>This value is infinity or
    /// not-a-number, is not an exact integer, or is less than 0 or greater
    /// than 65535.</exception>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public ushort ToUInt16IfExact()
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      if (this.IsZero) {
        return (ushort)0;
      }
      if (this.IsNegative) {
        throw new OverflowException("Value out of range");
      }
      if (this.exponent.CompareToInt(5) >= 0) {
        throw new OverflowException("Value out of range");
      }
      return this.ToEIntegerIfExact().ToUInt16Checked();
    }

    /// <summary>Converts a 16-bit unsigned integer to an
    /// arbitrary-precision decimal number.</summary>
    /// <param name='inputUInt16'>The number to convert as a 16-bit
    /// unsigned integer.</param>
    /// <returns>This number's value as an arbitrary-precision decimal
    /// number.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static EDecimal FromUInt16(ushort inputUInt16)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            int val = ((int)inputUInt16) & 0xffff;
      return FromInt32(val);
    }

    /// <summary>Converts an arbitrary-precision decimal number to a 16-bit
    /// unsigned integer if it can fit in a 16-bit unsigned integer after
    /// converting it to an integer by discarding its fractional
    /// part.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// decimal number.</param>
    /// <returns>The value of <paramref name='input'/>, truncated to a
    /// 16-bit unsigned integer.</returns>
    /// <exception cref='OverflowException'>The parameter <paramref
    /// name='input'/> is infinity or not-a-number, or the number, once
    /// converted to an integer by discarding its fractional part, is less
    /// than 0 or greater than 65535.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='input'/> is null.</exception>
    [CLSCompliant(false)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Usage",
      "CA2225",
      Justification = "Class implements an alternate method named ToUInt16Checked.")]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static explicit operator ushort(EDecimal input)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToUInt16Checked();
    }

    /// <summary>Converts a 16-bit unsigned integer to an
    /// arbitrary-precision decimal number.</summary>
    /// <param name='inputUInt16'>The number to convert as a 16-bit
    /// unsigned integer.</param>
    /// <returns>The value of <paramref name='inputUInt16'/> as an
    /// arbitrary-precision decimal number.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static implicit operator EDecimal(ushort inputUInt16)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            return EDecimal.FromUInt16(inputUInt16);
    }

    /// <summary>Converts an arbitrary-precision decimal number to a 32-bit
    /// signed integer if it can fit in a 32-bit signed integer after
    /// converting it to an integer by discarding its fractional
    /// part.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// decimal number.</param>
    /// <returns>The value of <paramref name='input'/>, truncated to a
    /// 32-bit signed integer.</returns>
    /// <exception cref='OverflowException'>The parameter <paramref
    /// name='input'/> is infinity or not-a-number, or the number, once
    /// converted to an integer by discarding its fractional part, is less
    /// than -2147483648 or greater than 2147483647.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='input'/> is null.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Usage",
      "CA2225",
      Justification = "Class implements an alternate method named ToInt32Checked.")]
    public static explicit operator int(EDecimal input) {
      if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToInt32Checked();
    }

    /// <summary>Converts a 32-bit signed integer to an arbitrary-precision
    /// decimal number.</summary>
    /// <param name='inputInt32'>The number to convert as a 32-bit signed
    /// integer.</param>
    /// <returns>The value of <paramref name='inputInt32'/> as an
    /// arbitrary-precision decimal number.</returns>
    public static implicit operator EDecimal(int inputInt32) {
      return EDecimal.FromInt32(inputInt32);
    }

    /// <summary>Converts this number's value to a 32-bit signed integer if
    /// it can fit in a 32-bit signed integer after converting it to an
    /// integer by discarding its fractional part.</summary>
    /// <returns>This number's value, truncated to a 32-bit signed
    /// integer.</returns>
    /// <exception cref='OverflowException'>This value is infinity or
    /// not-a-number, or the number, once converted to an integer by
    /// discarding its fractional part, is less than 0 or greater than
    /// 4294967295.</exception>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public uint ToUInt32Checked()
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      if (this.IsIntegerPartZero()) {
        return 0U;
      }
      if (this.exponent.CompareToInt(10) >= 0) {
        throw new OverflowException("Value out of range");
      }
      return this.ToEInteger().ToUInt32Checked();
    }

    /// <summary>Converts this number's value to an integer by discarding
    /// its fractional part, and returns the least-significant bits of its
    /// two's-complement form as a 32-bit signed integer.</summary>
    /// <returns>This number, converted to a 32-bit signed integer. Returns
    /// 0 if this value is infinity or not-a-number.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public uint ToUInt32Unchecked()
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            return this.IsFinite ? this.ToEInteger().ToUInt32Unchecked() : 0U;
    }

    /// <summary>Converts this number's value to a 32-bit signed integer if
    /// it can fit in a 32-bit signed integer without rounding to a
    /// different numerical value.</summary>
    /// <returns>This number's value as a 32-bit signed integer.</returns>
    /// <exception cref='ArithmeticException'>This value is infinity or
    /// not-a-number, is not an exact integer, or is less than 0 or greater
    /// than 4294967295.</exception>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public uint ToUInt32IfExact()
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      if (this.IsZero) {
        return 0U;
      }
      if (this.IsNegative) {
        throw new OverflowException("Value out of range");
      }
      if (this.exponent.CompareToInt(10) >= 0) {
        throw new OverflowException("Value out of range");
      }
      return this.ToEIntegerIfExact().ToUInt32Checked();
    }

    /// <summary>Converts a 32-bit signed integer to an arbitrary-precision
    /// decimal number.</summary>
    /// <param name='inputUInt32'>The number to convert as a 32-bit signed
    /// integer.</param>
    /// <returns>This number's value as an arbitrary-precision decimal
    /// number.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static EDecimal FromUInt32(uint inputUInt32)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            long val = ((long)inputUInt32) & 0xffffffffL;
      return FromInt64(val);
    }

    /// <summary>Converts an arbitrary-precision decimal number to a 32-bit
    /// signed integer if it can fit in a 32-bit signed integer after
    /// converting it to an integer by discarding its fractional
    /// part.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// decimal number.</param>
    /// <returns>The value of <paramref name='input'/>, truncated to a
    /// 32-bit signed integer.</returns>
    /// <exception cref='OverflowException'>The parameter <paramref
    /// name='input'/> is infinity or not-a-number, or the number, once
    /// converted to an integer by discarding its fractional part, is less
    /// than 0 or greater than 4294967295.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='input'/> is null.</exception>
    [CLSCompliant(false)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Usage",
      "CA2225",
      Justification = "Class implements an alternate method named ToUInt32Checked.")]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static explicit operator uint(EDecimal input)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToUInt32Checked();
    }

    /// <summary>Converts a 32-bit signed integer to an arbitrary-precision
    /// decimal number.</summary>
    /// <param name='inputUInt32'>The number to convert as a 32-bit signed
    /// integer.</param>
    /// <returns>The value of <paramref name='inputUInt32'/> as an
    /// arbitrary-precision decimal number.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static implicit operator EDecimal(uint inputUInt32)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            return EDecimal.FromUInt32(inputUInt32);
    }

    /// <summary>Converts an arbitrary-precision decimal number to a 64-bit
    /// signed integer if it can fit in a 64-bit signed integer after
    /// converting it to an integer by discarding its fractional
    /// part.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// decimal number.</param>
    /// <returns>The value of <paramref name='input'/>, truncated to a
    /// 64-bit signed integer.</returns>
    /// <exception cref='OverflowException'>The parameter <paramref
    /// name='input'/> is infinity or not-a-number, or the number, once
    /// converted to an integer by discarding its fractional part, is less
    /// than -9223372036854775808 or greater than
    /// 9223372036854775807.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='input'/> is null.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Usage",
      "CA2225",
      Justification = "Class implements an alternate method named ToInt64Checked.")]
    public static explicit operator long(EDecimal input) {
      if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToInt64Checked();
    }

    /// <summary>Converts a 64-bit signed integer to an arbitrary-precision
    /// decimal number.</summary>
    /// <param name='inputInt64'>The number to convert as a 64-bit signed
    /// integer.</param>
    /// <returns>The value of <paramref name='inputInt64'/> as an
    /// arbitrary-precision decimal number.</returns>
    public static implicit operator EDecimal(long inputInt64) {
      return EDecimal.FromInt64(inputInt64);
    }

    /// <summary>Converts this number's value to a 64-bit unsigned integer
    /// if it can fit in a 64-bit unsigned integer after converting it to
    /// an integer by discarding its fractional part.</summary>
    /// <returns>This number's value, truncated to a 64-bit unsigned
    /// integer.</returns>
    /// <exception cref='OverflowException'>This value is infinity or
    /// not-a-number, or the number, once converted to an integer by
    /// discarding its fractional part, is less than 0 or greater than
    /// 18446744073709551615.</exception>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public ulong ToUInt64Checked()
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      if (this.IsIntegerPartZero()) {
        return 0UL;
      }
      if (this.exponent.CompareToInt(20) >= 0) {
        throw new OverflowException("Value out of range");
      }
      return this.ToEInteger().ToUInt64Checked();
    }

    /// <summary>Converts this number's value to an integer by discarding
    /// its fractional part, and returns the least-significant bits of its
    /// two's-complement form as a 64-bit unsigned integer.</summary>
    /// <returns>This number, converted to a 64-bit unsigned integer.
    /// Returns 0 if this value is infinity or not-a-number.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public ulong ToUInt64Unchecked()
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            return this.IsFinite ? this.ToEInteger().ToUInt64Unchecked() : 0UL;
    }

    /// <summary>Converts this number's value to a 64-bit unsigned integer
    /// if it can fit in a 64-bit unsigned integer without rounding to a
    /// different numerical value.</summary>
    /// <returns>This number's value as a 64-bit unsigned
    /// integer.</returns>
    /// <exception cref='ArithmeticException'>This value is infinity or
    /// not-a-number, is not an exact integer, or is less than 0 or greater
    /// than 18446744073709551615.</exception>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public ulong ToUInt64IfExact()
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      if (this.IsZero) {
        return 0UL;
      }
      if (this.IsNegative) {
        throw new OverflowException("Value out of range");
      }
      if (this.exponent.CompareToInt(20) >= 0) {
        throw new OverflowException("Value out of range");
      }
      return this.ToEIntegerIfExact().ToUInt64Checked();
    }

    /// <summary>Converts a 64-bit unsigned integer to an
    /// arbitrary-precision decimal number.</summary>
    /// <param name='inputUInt64'>The number to convert as a 64-bit
    /// unsigned integer.</param>
    /// <returns>This number's value as an arbitrary-precision decimal
    /// number.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static EDecimal FromUInt64(ulong inputUInt64)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            return FromEInteger(EInteger.FromUInt64(inputUInt64));
    }

    /// <summary>Converts an arbitrary-precision decimal number to a 64-bit
    /// unsigned integer if it can fit in a 64-bit unsigned integer after
    /// converting it to an integer by discarding its fractional
    /// part.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// decimal number.</param>
    /// <returns>The value of <paramref name='input'/>, truncated to a
    /// 64-bit unsigned integer.</returns>
    /// <exception cref='OverflowException'>The parameter <paramref
    /// name='input'/> is infinity or not-a-number, or the number, once
    /// converted to an integer by discarding its fractional part, is less
    /// than 0 or greater than 18446744073709551615.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='input'/> is null.</exception>
    [CLSCompliant(false)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Usage",
      "CA2225",
      Justification = "Class implements an alternate method named ToUInt64Checked.")]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static explicit operator ulong(EDecimal input)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToUInt64Checked();
    }

    /// <summary>Converts a 64-bit unsigned integer to an
    /// arbitrary-precision decimal number.</summary>
    /// <param name='inputUInt64'>The number to convert as a 64-bit
    /// unsigned integer.</param>
    /// <returns>The value of <paramref name='inputUInt64'/> as an
    /// arbitrary-precision decimal number.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static implicit operator EDecimal(ulong inputUInt64)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            return EDecimal.FromUInt64(inputUInt64);
    }

    // End integer conversions
  }
}
