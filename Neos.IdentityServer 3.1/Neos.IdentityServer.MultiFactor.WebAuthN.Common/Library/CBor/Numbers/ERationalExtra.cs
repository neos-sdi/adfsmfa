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
  public sealed partial class ERational {
    /// <summary>Converts a boolean value (true or false) to an
    /// arbitrary-precision rational number.</summary>
    /// <param name='boolValue'>Either true or false.</param>
    /// <returns>1 if <paramref name='boolValue'/> is true; otherwise,
    /// 0.</returns>
    public static explicit operator ERational(bool boolValue) {
      return FromBoolean(boolValue);
    }

    /// <summary>Converts an arbitrary-precision integer to an
    /// arbitrary-precision rational number.</summary>
    /// <param name='eint'>An arbitrary-precision integer.</param>
    /// <returns>An arbitrary-precision rational number.</returns>
    public static implicit operator ERational(EInteger eint) {
      return FromEInteger(eint);
    }

    /// <summary>Converts an arbitrary-precision decimal floating-point
    /// number to an arbitrary-precision rational number.</summary>
    /// <param name='eint'>The parameter <paramref name='eint'/> is an
    /// arbitrary-precision decimal floating-point number.</param>
    /// <returns>An arbitrary-precision rational number.</returns>
    public static implicit operator ERational(EDecimal eint) {
      return FromEDecimal(eint);
    }

    /// <summary>Converts an arbitrary-precision binary floating-point
    /// number to an arbitrary-precision rational number.</summary>
    /// <param name='eint'>An arbitrary-precision binary floating-point
    /// number.</param>
    /// <returns>An arbitrary-precision rational number.</returns>
    public static implicit operator ERational(EFloat eint) {
      return FromEFloat(eint);
    }

    /// <summary>Converts a <c>decimal</c> under the Common Language
    /// Infrastructure (usually a.NET Framework decimal) to a rational
    /// number.</summary>
    /// <param name='eint'>The number to convert as a <c>decimal</c> under
    /// the Common Language Infrastructure (usually a.NET Framework
    /// decimal).</param>
    /// <returns>An arbitrary-precision rational number.</returns>
    public static ERational FromDecimal(decimal eint) {
      return FromEDecimal(EDecimal.FromDecimal(eint));
    }

    /// <summary>Converts a <c>decimal</c> under the Common Language
    /// Infrastructure (usually a.NET Framework decimal). to an
    /// arbitrary-precision rational number.</summary>
    /// <param name='eint'>A <c>decimal</c> under the Common Language
    /// Infrastructure (usually a.NET Framework decimal).</param>
    /// <returns>An arbitrary-precision rational number.</returns>
    public static implicit operator ERational(decimal eint) {
      return FromDecimal(eint);
    }

    /// <summary>Converts a 32-bit binary floating-point number to a
    /// rational number.</summary>
    /// <param name='eint'>The parameter <paramref name='eint'/> is a
    /// 32-bit binary floating-point number.</param>
    /// <returns>The value of <paramref name='eint'/> as an
    /// arbitrary-precision rational number.</returns>
    public static implicit operator ERational(float eint) {
      return ERational.FromSingle(eint);
    }

    /// <summary>Converts a 64-bit floating-point number to an
    /// arbitrary-precision rational number.</summary>
    /// <param name='eint'>The parameter <paramref name='eint'/> is a
    /// 64-bit floating-point number.</param>
    /// <returns>An arbitrary-precision rational number.</returns>
    public static implicit operator ERational(double eint) {
      return ERational.FromDouble(eint);
    }

    /// <summary>Adds an arbitrary-precision rational number and another
    /// arbitrary-precision rational number and returns the
    /// result.</summary>
    /// <param name='bthis'>The first operand.</param>
    /// <param name='augend'>The second operand.</param>
    /// <returns>The sum of the two numbers, that is, an
    /// arbitrary-precision rational number plus another
    /// arbitrary-precision rational number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter "otherValue"
    /// is null.</exception>
    public static ERational operator +(ERational bthis, ERational augend) {
      if (bthis == null) {
        throw new ArgumentNullException(nameof(bthis));
      }
      return bthis.Add(augend);
    }

    /// <summary>Subtracts an arbitrary-precision rational number from this
    /// instance.</summary>
    /// <param name='bthis'>The first operand.</param>
    /// <param name='subtrahend'>The second operand.</param>
    /// <returns>The difference of the two objects.</returns>
    /// <exception cref='ArgumentNullException'>The parameter "otherValue"
    /// is null.</exception>
    public static ERational operator -(
      ERational bthis,
      ERational subtrahend) {
      if (bthis == null) {
        throw new ArgumentNullException(nameof(bthis));
      }
      return bthis.Subtract(subtrahend);
    }

    /// <summary>Adds one to an arbitrary-precision rational
    /// number.</summary>
    /// <param name='bthis'>An arbitrary-precision rational number.</param>
    /// <returns>The number given in <paramref name='bthis'/> plus
    /// one.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bthis'/> is null.</exception>
    public static ERational operator ++(ERational bthis) {
      if (bthis == null) {
        throw new ArgumentNullException(nameof(bthis));
      }
      return bthis.Add(1);
    }

    /// <summary>Subtracts one from an arbitrary-precision rational
    /// number.</summary>
    /// <param name='bthis'>An arbitrary-precision rational number.</param>
    /// <returns>The number given in <paramref name='bthis'/> minus
    /// one.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bthis'/> is null.</exception>
    public static ERational operator --(ERational bthis) {
      if (bthis == null) {
        throw new ArgumentNullException(nameof(bthis));
      }
      return bthis.Subtract(1);
    }

    /// <summary>Multiplies an arbitrary-precision rational number by
    /// another arbitrary-precision rational number and returns the
    /// result.</summary>
    /// <param name='operand1'>The first operand.</param>
    /// <param name='operand2'>The second operand.</param>
    /// <returns>The product of the two numbers, that is, an
    /// arbitrary-precision rational number times another
    /// arbitrary-precision rational number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter "otherValue"
    /// is null.</exception>
    public static ERational operator *(
      ERational operand1,
      ERational operand2) {
      if (operand1 == null) {
        throw new ArgumentNullException(nameof(operand1));
      }
      return operand1.Multiply(operand2);
    }

    /// <summary>Divides an arbitrary-precision rational number by the
    /// value of another arbitrary-precision rational number
    /// object.</summary>
    /// <param name='dividend'>An arbitrary-precision rational number
    /// serving as the dividend.</param>
    /// <param name='divisor'>An arbitrary-precision rational number
    /// serving as the divisor.</param>
    /// <returns>The quotient of the two objects.</returns>
    /// <exception cref='ArgumentNullException'>The parameter "otherValue"
    /// is null.</exception>
    public static ERational operator /(
      ERational dividend,
      ERational divisor) {
      if (dividend == null) {
        throw new ArgumentNullException(nameof(dividend));
      }
      return dividend.Divide(divisor);
    }

    /// <summary>Returns the remainder that would result when an
    /// arbitrary-precision rational number is divided by another
    /// arbitrary-precision rational number.</summary>
    /// <param name='dividend'>The dividend.</param>
    /// <param name='divisor'>The divisor.</param>
    /// <returns>The remainder that would result when an
    /// arbitrary-precision rational number is divided by another
    /// arbitrary-precision rational number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter "otherValue"
    /// is null.</exception>
    public static ERational operator %(
      ERational dividend,
      ERational divisor) {
      if (dividend == null) {
        throw new ArgumentNullException(nameof(dividend));
      }
      return dividend.Remainder(divisor);
    }

    /// <summary>Returns an arbitrary-precision rational number with the
    /// same value as the given one but with its sign reversed.</summary>
    /// <param name='bigValue'>An arbitrary-precision rational number to
    /// negate.</param>
    /// <returns>An arbitrary-precision rational number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bigValue'/> is null.</exception>
    public static ERational operator -(ERational bigValue) {
      if (bigValue == null) {
        throw new ArgumentNullException(nameof(bigValue));
      }
      return bigValue.Negate();
    }

    /// <summary>Converts this value to a <c>decimal</c> under the Common
    /// Language Infrastructure (usually a.NET Framework decimal).
    /// Currently, converts this value to the precision and range of a.NET
    /// Framework decimal.</summary>
    /// <returns>A <c>decimal</c> under the Common Language Infrastructure
    /// (usually a.NET Framework decimal).</returns>
    public decimal ToDecimal() {
      ERational extendedNumber = this;
      if (extendedNumber.IsInfinity() || extendedNumber.IsNaN()) {
        throw new OverflowException("This object's value is out of range");
      }
      try {
        EDecimal newDecimal = EDecimal.FromEInteger(extendedNumber.Numerator)
          .Divide(
            EDecimal.FromEInteger(extendedNumber.Denominator),
            EContext.CliDecimal.WithTraps(EContext.FlagOverflow));
        return (decimal)newDecimal;
      } catch (ETrapException ex) {
        throw new OverflowException("This object's value is out of range", ex);
      }
    }

    /// <summary>Converts an arbitrary-precision rational number to a
    /// <c>decimal</c> under the Common Language Infrastructure (see
    /// <see cref='PeterO.Numbers.EDecimal'>"Forms of numbers"</see>
    /// ).</summary>
    /// <param name='extendedNumber'>The number to convert as an
    /// arbitrary-precision rational number.</param>
    /// <returns>A <c>decimal</c> under the Common Language Infrastructure
    /// (usually a.NET Framework decimal).</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='extendedNumber'/> is null.</exception>
    public static explicit operator decimal(
      ERational extendedNumber) {
      if (extendedNumber == null) {
        throw new ArgumentNullException(nameof(extendedNumber));
      }
      return extendedNumber.ToDecimal();
    }

    /// <summary>Converts an arbitrary-precision rational number to an
    /// arbitrary-precision integer. Any fractional part in the value will
    /// be discarded when converting to an arbitrary-precision
    /// integer.</summary>
    /// <param name='bigValue'>An arbitrary-precision rational
    /// number.</param>
    /// <returns>An arbitrary-precision integer.</returns>
    /// <exception cref='OverflowException'>This object's value is infinity
    /// or not-a-number (NaN).</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bigValue'/> is null.</exception>
    public static explicit operator EInteger(ERational bigValue) {
      if (bigValue == null) {
        throw new ArgumentNullException(nameof(bigValue));
      }
      return bigValue.ToEInteger();
    }

    /// <summary>Converts an arbitrary-precision rational number to a
    /// 64-bit floating-point number. The half-even rounding mode is
    /// used.</summary>
    /// <param name='bigValue'>The number to convert as an
    /// arbitrary-precision rational number.</param>
    /// <returns>The closest 64-bit floating-point number to this value.
    /// The return value can be positive infinity or negative infinity if
    /// this value exceeds the range of a 64-bit floating point
    /// number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bigValue'/> is null.</exception>
    public static explicit operator double(ERational bigValue) {
      if (bigValue == null) {
        throw new ArgumentNullException(nameof(bigValue));
      }
      return bigValue.ToDouble();
    }

    /// <summary>Converts an arbitrary-precision rational number to a
    /// 32-bit binary floating-point number. The half-even rounding mode is
    /// used.</summary>
    /// <param name='bigValue'>The number to convert as an
    /// arbitrary-precision rational number.</param>
    /// <returns>The closest 32-bit binary floating-point number to this
    /// value. The return value can be positive infinity or negative
    /// infinity if this value exceeds the range of a 32-bit floating point
    /// number.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bigValue'/> is null.</exception>
    public static explicit operator float(ERational bigValue) {
      if (bigValue == null) {
        throw new ArgumentNullException(nameof(bigValue));
      }
      return bigValue.ToSingle();
    }

    // Begin integer conversions

    /// <summary>Converts an arbitrary-precision rational number to a byte
    /// (from 0 to 255) if it can fit in a byte (from 0 to 255) after
    /// converting it to an integer by discarding its fractional
    /// part.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// rational number.</param>
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
    public static explicit operator byte(ERational input) {
      if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToByteChecked();
    }

    /// <summary>Converts a byte (from 0 to 255) to an arbitrary-precision
    /// rational number.</summary>
    /// <param name='inputByte'>The number to convert as a byte (from 0 to
    /// 255).</param>
    /// <returns>The value of <paramref name='inputByte'/> as an
    /// arbitrary-precision rational number.</returns>
    public static implicit operator ERational(byte inputByte) {
      return ERational.FromByte(inputByte);
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
      return this.IsZero ? ((sbyte)0) :
        this.ToEInteger().ToSByteChecked();
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
      return this.IsZero ? ((sbyte)0) :
        this.ToEIntegerIfExact().ToSByteChecked();
    }

    /// <summary>Converts an 8-bit signed integer to an arbitrary-precision
    /// rational number.</summary>
    /// <param name='inputSByte'>The number to convert as an 8-bit signed
    /// integer.</param>
    /// <returns>This number's value as an arbitrary-precision rational
    /// number.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static ERational FromSByte(sbyte inputSByte)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            var val = (int)inputSByte;
      return FromInt32(val);
    }

    /// <summary>Converts an arbitrary-precision rational number to an
    /// 8-bit signed integer if it can fit in an 8-bit signed integer after
    /// converting it to an integer by discarding its fractional
    /// part.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// rational number.</param>
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
        public static explicit operator sbyte(ERational input)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToSByteChecked();
    }

    /// <summary>Converts an 8-bit signed integer to an arbitrary-precision
    /// rational number.</summary>
    /// <param name='inputSByte'>The number to convert as an 8-bit signed
    /// integer.</param>
    /// <returns>The value of <paramref name='inputSByte'/> as an
    /// arbitrary-precision rational number.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static implicit operator ERational(sbyte inputSByte)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            return ERational.FromSByte(inputSByte);
    }

    /// <summary>Converts an arbitrary-precision rational number to a
    /// 16-bit signed integer if it can fit in a 16-bit signed integer
    /// after converting it to an integer by discarding its fractional
    /// part.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// rational number.</param>
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
    public static explicit operator short(ERational input) {
      if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToInt16Checked();
    }

    /// <summary>Converts a 16-bit signed integer to an arbitrary-precision
    /// rational number.</summary>
    /// <param name='inputInt16'>The number to convert as a 16-bit signed
    /// integer.</param>
    /// <returns>The value of <paramref name='inputInt16'/> as an
    /// arbitrary-precision rational number.</returns>
    public static implicit operator ERational(short inputInt16) {
      return ERational.FromInt16(inputInt16);
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
      return this.IsZero ?
        ((ushort)0) : this.ToEInteger().ToUInt16Checked();
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
      return this.IsZero ? ((ushort)0) :
        this.ToEIntegerIfExact().ToUInt16Checked();
    }

    /// <summary>Converts a 16-bit unsigned integer to an
    /// arbitrary-precision rational number.</summary>
    /// <param name='inputUInt16'>The number to convert as a 16-bit
    /// unsigned integer.</param>
    /// <returns>This number's value as an arbitrary-precision rational
    /// number.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static ERational FromUInt16(ushort inputUInt16)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            int val = ((int)inputUInt16) & 0xffff;
      return FromInt32(val);
    }

    /// <summary>Converts an arbitrary-precision rational number to a
    /// 16-bit unsigned integer if it can fit in a 16-bit unsigned integer
    /// after converting it to an integer by discarding its fractional
    /// part.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// rational number.</param>
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
        public static explicit operator ushort(ERational input)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToUInt16Checked();
    }

    /// <summary>Converts a 16-bit unsigned integer to an
    /// arbitrary-precision rational number.</summary>
    /// <param name='inputUInt16'>The number to convert as a 16-bit
    /// unsigned integer.</param>
    /// <returns>The value of <paramref name='inputUInt16'/> as an
    /// arbitrary-precision rational number.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static implicit operator ERational(ushort inputUInt16)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            return ERational.FromUInt16(inputUInt16);
    }

    /// <summary>Converts an arbitrary-precision rational number to a
    /// 32-bit signed integer if it can fit in a 32-bit signed integer
    /// after converting it to an integer by discarding its fractional
    /// part.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// rational number.</param>
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
    public static explicit operator int(ERational input) {
      if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToInt32Checked();
    }

    /// <summary>Converts a 32-bit signed integer to an arbitrary-precision
    /// rational number.</summary>
    /// <param name='inputInt32'>The number to convert as a 32-bit signed
    /// integer.</param>
    /// <returns>The value of <paramref name='inputInt32'/> as an
    /// arbitrary-precision rational number.</returns>
    public static implicit operator ERational(int inputInt32) {
      return ERational.FromInt32(inputInt32);
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
      return this.IsZero ? 0U : this.ToEInteger().ToUInt32Checked();
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
      return this.IsZero ? 0U :
        this.ToEIntegerIfExact().ToUInt32Checked();
    }

    /// <summary>Converts a 32-bit signed integer to an arbitrary-precision
    /// rational number.</summary>
    /// <param name='inputUInt32'>The number to convert as a 32-bit signed
    /// integer.</param>
    /// <returns>This number's value as an arbitrary-precision rational
    /// number.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static ERational FromUInt32(uint inputUInt32)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            long val = ((long)inputUInt32) & 0xffffffffL;
      return FromInt64(val);
    }

    /// <summary>Converts an arbitrary-precision rational number to a
    /// 32-bit signed integer if it can fit in a 32-bit signed integer
    /// after converting it to an integer by discarding its fractional
    /// part.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// rational number.</param>
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
        public static explicit operator uint(ERational input)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToUInt32Checked();
    }

    /// <summary>Converts a 32-bit signed integer to an arbitrary-precision
    /// rational number.</summary>
    /// <param name='inputUInt32'>The number to convert as a 32-bit signed
    /// integer.</param>
    /// <returns>The value of <paramref name='inputUInt32'/> as an
    /// arbitrary-precision rational number.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static implicit operator ERational(uint inputUInt32)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            return ERational.FromUInt32(inputUInt32);
    }

    /// <summary>Converts an arbitrary-precision rational number to a
    /// 64-bit signed integer if it can fit in a 64-bit signed integer
    /// after converting it to an integer by discarding its fractional
    /// part.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// rational number.</param>
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
    public static explicit operator long(ERational input) {
      if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToInt64Checked();
    }

    /// <summary>Converts a 64-bit signed integer to an arbitrary-precision
    /// rational number.</summary>
    /// <param name='inputInt64'>The number to convert as a 64-bit signed
    /// integer.</param>
    /// <returns>The value of <paramref name='inputInt64'/> as an
    /// arbitrary-precision rational number.</returns>
    public static implicit operator ERational(long inputInt64) {
      return ERational.FromInt64(inputInt64);
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
      return this.IsZero ? 0UL :
        this.ToEInteger().ToUInt64Checked();
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
      return this.IsZero ? 0UL :
        this.ToEIntegerIfExact().ToUInt64Checked();
    }

    /// <summary>Converts a 64-bit unsigned integer to an
    /// arbitrary-precision rational number.</summary>
    /// <param name='inputUInt64'>The number to convert as a 64-bit
    /// unsigned integer.</param>
    /// <returns>This number's value as an arbitrary-precision rational
    /// number.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static ERational FromUInt64(ulong inputUInt64)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            return FromEInteger(EInteger.FromUInt64(inputUInt64));
    }

    /// <summary>Converts an arbitrary-precision rational number to a
    /// 64-bit unsigned integer if it can fit in a 64-bit unsigned integer
    /// after converting it to an integer by discarding its fractional
    /// part.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// rational number.</param>
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
        public static explicit operator ulong(ERational input)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToUInt64Checked();
    }

    /// <summary>Converts a 64-bit unsigned integer to an
    /// arbitrary-precision rational number.</summary>
    /// <param name='inputUInt64'>The number to convert as a 64-bit
    /// unsigned integer.</param>
    /// <returns>The value of <paramref name='inputUInt64'/> as an
    /// arbitrary-precision rational number.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static implicit operator ERational(ulong inputUInt64)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            return ERational.FromUInt64(inputUInt64);
    }

    // End integer conversions
  }
}
