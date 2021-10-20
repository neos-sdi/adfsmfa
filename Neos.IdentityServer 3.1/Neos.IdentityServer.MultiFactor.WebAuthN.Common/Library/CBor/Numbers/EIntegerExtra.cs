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
  public sealed partial class EInteger
  {
    /// <summary>Converts a 64-bit unsigned integer to an
    /// arbitrary-precision integer.</summary>
    /// <param name='ulongValue'>The number to convert as a 64-bit unsigned
    /// integer.</param>
    /// <returns>The value of <paramref name='ulongValue'/> as an
    /// arbitrary-precision integer.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static EInteger FromUInt64(ulong ulongValue)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            if (ulongValue <= Int64.MaxValue) {
        return FromInt64((long)ulongValue);
      } else {
        ulongValue &= (1UL << 63) - 1;
        return EInteger.One.ShiftLeft(63).Add(FromInt64((long)ulongValue));
      }
    }

    /// <summary>Adds an arbitrary-precision integer and another
    /// arbitrary-precision integer and returns the result.</summary>
    /// <param name='bthis'>The first operand.</param>
    /// <param name='augend'>The second operand.</param>
    /// <returns>The sum of the two numbers, that is, an
    /// arbitrary-precision integer plus another arbitrary-precision
    /// integer.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bthis'/> is null.</exception>
    public static EInteger operator +(EInteger bthis, EInteger augend) {
      if (bthis == null) {
        throw new ArgumentNullException(nameof(bthis));
      }
      return bthis.Add(augend);
    }

    /// <summary>Subtracts two arbitrary-precision integer
    /// values.</summary>
    /// <param name='bthis'>An arbitrary-precision integer.</param>
    /// <param name='subtrahend'>Another arbitrary-precision
    /// integer.</param>
    /// <returns>The difference of the two objects.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bthis'/> is null.</exception>
    public static EInteger operator -(
      EInteger bthis,
      EInteger subtrahend) {
      if (bthis == null) {
        throw new ArgumentNullException(nameof(bthis));
      }
      return bthis.Subtract(subtrahend);
    }

    /// <summary>Adds one to an arbitrary-precision integer.</summary>
    /// <param name='bthis'>An arbitrary-precision integer.</param>
    /// <returns>The given arbitrary-precision integer plus one.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bthis'/> is null.</exception>
    public static EInteger operator ++(EInteger bthis) {
      if (bthis == null) {
        throw new ArgumentNullException(nameof(bthis));
      }
      return bthis.Add(1);
    }

    /// <summary>Subtracts one from an arbitrary-precision
    /// integer.</summary>
    /// <param name='bthis'>An arbitrary-precision integer.</param>
    /// <returns>The given arbitrary-precision integer minus one.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bthis'/> is null.</exception>
    public static EInteger operator --(EInteger bthis) {
      if (bthis == null) {
        throw new ArgumentNullException(nameof(bthis));
      }
      return bthis.Subtract(1);
    }

    /// <summary>Multiplies an arbitrary-precision integer by another
    /// arbitrary-precision integer and returns the result.</summary>
    /// <param name='operand1'>The first operand.</param>
    /// <param name='operand2'>The second operand.</param>
    /// <returns>The product of the two numbers, that is, an
    /// arbitrary-precision integer times another arbitrary-precision
    /// integer.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='operand1'/> is null.</exception>
    public static EInteger operator *(
      EInteger operand1,
      EInteger operand2) {
      if (operand1 == null) {
        throw new ArgumentNullException(nameof(operand1));
      }
      return operand1.Multiply(operand2);
    }

    /// <summary>Divides an arbitrary-precision integer by the value of an
    /// arbitrary-precision integer object.</summary>
    /// <param name='dividend'>The number that will be divided by the
    /// divisor.</param>
    /// <param name='divisor'>The number to divide by.</param>
    /// <returns>The quotient of the two objects.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='dividend'/> is null.</exception>
    public static EInteger operator /(
      EInteger dividend,
      EInteger divisor) {
      if (dividend == null) {
        throw new ArgumentNullException(nameof(dividend));
      }
      return dividend.Divide(divisor);
    }

    /// <summary>Returns the remainder that would result when an
    /// arbitrary-precision integer is divided by another
    /// arbitrary-precision integer. The remainder is the number that
    /// remains when the absolute value of an arbitrary-precision integer
    /// is divided by the absolute value of the other arbitrary-precision
    /// integer; the remainder has the same sign (positive or negative) as
    /// this arbitrary-precision integer.</summary>
    /// <param name='dividend'>The first operand.</param>
    /// <param name='divisor'>The number to divide by.</param>
    /// <returns>The remainder that would result when an
    /// arbitrary-precision integer is divided by another
    /// arbitrary-precision integer.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='dividend'/> is null.</exception>
    public static EInteger operator %(
      EInteger dividend,
      EInteger divisor) {
      if (dividend == null) {
        throw new ArgumentNullException(nameof(dividend));
      }
      return dividend.Remainder(divisor);
    }

    /// <summary>Returns an arbitrary-precision integer with the bits
    /// shifted to the left by a number of bits. A value of 1 doubles this
    /// value, a value of 2 multiplies it by 4, a value of 3 by 8, a value
    /// of 4 by 16, and so on.</summary>
    /// <param name='bthis'>The arbitrary-precision integer to shift
    /// left.</param>
    /// <param name='bitCount'>The number of bits to shift. Can be
    /// negative, in which case this is the same as shiftRight with the
    /// absolute value of this parameter.</param>
    /// <returns>An arbitrary-precision integer.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bthis'/> is null.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Usage",
      "CA2225",
      Justification = "Class implements an alternate method named ShiftLeft.")]
    public static EInteger operator <<(EInteger bthis, int bitCount) {
      if (bthis == null) {
        throw new ArgumentNullException(nameof(bthis));
      }
      return bthis.ShiftLeft(bitCount);
    }

    /// <summary>Calculates the remainder when an arbitrary-precision
    /// integer raised to a certain power is divided by another
    /// arbitrary-precision integer.</summary>
    /// <param name='bigintValue'>The starting operand.</param>
    /// <param name='pow'>The power to raise this integer by.</param>
    /// <param name='mod'>The integer to divide the raised number
    /// by.</param>
    /// <returns>The value ( <paramref name='bigintValue'/> ^ <paramref
    /// name='pow'/> )% <paramref name='mod'/>.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bigintValue'/> is null.</exception>
    public static EInteger ModPow(
      EInteger bigintValue,
      EInteger pow,
      EInteger mod) {
      if (bigintValue == null) {
        throw new ArgumentNullException(nameof(bigintValue));
      }
      return bigintValue.ModPow(pow, mod);
    }

    /// <summary>Shifts the bits of an arbitrary-precision integer to the
    /// right.</summary>
    /// <param name='bthis'>Another arbitrary-precision integer.</param>
    /// <param name='smallValue'>The parameter <paramref
    /// name='smallValue'/> is a 32-bit signed integer.</param>
    /// <returns>An arbitrary-precision integer.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bthis'/> is null.</exception>
    /// <remarks>For this operation, the arbitrary-precision integer is
    /// treated as a two's-complement form (see
    /// <see cref='PeterO.Numbers.EDecimal'>"Forms of numbers"</see> ).
    /// Thus, for negative values, the arbitrary-precision integer is
    /// sign-extended.</remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Usage",
      "CA2225",
      Justification = "Class implements an alternate method named ShiftRight.")]
    public static EInteger operator >>(EInteger bthis, int smallValue) {
      if (bthis == null) {
        throw new ArgumentNullException(nameof(bthis));
      }
      return bthis.ShiftRight(smallValue);
    }

    /// <summary>Negates an arbitrary-precision integer.</summary>
    /// <param name='bigValue'>An arbitrary-precision integer to
    /// negate.</param>
    /// <returns>An arbitrary-precision integer.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='bigValue'/> is null.</exception>
    public static EInteger operator -(EInteger bigValue) {
      if (bigValue == null) {
        throw new ArgumentNullException(nameof(bigValue));
      }
      return bigValue.Negate();
    }

    /// <summary>Converts this number's value to a 64-bit signed integer if
    /// it can fit in a 64-bit signed integer.</summary>
    /// <returns>This number's value as a 64-bit signed integer.</returns>
    /// <exception cref='OverflowException'>This value is outside the range
    /// of a 64-bit signed integer.</exception>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public ulong ToUInt64Checked()
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            if (this.negative || this.wordCount > 4) {
        throw new OverflowException("This object's value is out of range");
      }
      long ret = this.ToInt64Unchecked();
      if (this.GetSignedBit(63)) {
        ret |= 1L << 63;
      }
      return unchecked((ulong)ret);
    }

    /// <summary>Converts this number to a 64-bit signed integer, returning
    /// the least-significant bits of this number's two's-complement
    /// form.</summary>
    /// <returns>This number, converted to a 64-bit signed
    /// integer.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public ulong ToUInt64Unchecked()
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            long ret = this.ToInt64Unchecked();
      if (this.GetSignedBit(63)) {
        ret |= 1L << 63;
      }
      return unchecked((ulong)ret);
    }

    /// <summary>Determines whether an arbitrary-precision integer is less
    /// than another arbitrary-precision integer.</summary>
    /// <param name='thisValue'>The first arbitrary-precision
    /// integer.</param>
    /// <param name='otherValue'>The second arbitrary-precision
    /// integer.</param>
    /// <returns><c>true</c> if <paramref name='thisValue'/> is less than
    /// <paramref name='otherValue'/> ; otherwise, <c>false</c>.</returns>
    public static bool operator <(EInteger thisValue, EInteger otherValue) {
      return (thisValue == null) ? (otherValue != null) :
(thisValue.CompareTo(otherValue) < 0);
    }

    /// <summary>Determines whether an arbitrary-precision integer is up to
    /// another arbitrary-precision integer.</summary>
    /// <param name='thisValue'>The first arbitrary-precision
    /// integer.</param>
    /// <param name='otherValue'>The second arbitrary-precision
    /// integer.</param>
    /// <returns><c>true</c> if <paramref name='thisValue'/> is up to
    /// <paramref name='otherValue'/> ; otherwise, <c>false</c>.</returns>
    public static bool operator <=(
      EInteger thisValue,
      EInteger otherValue) {
      return (thisValue == null) || (thisValue.CompareTo(otherValue) <= 0);
    }

    /// <summary>Determines whether an arbitrary-precision integer is
    /// greater than another arbitrary-precision integer.</summary>
    /// <param name='thisValue'>The first arbitrary-precision
    /// integer.</param>
    /// <param name='otherValue'>The second arbitrary-precision
    /// integer.</param>
    /// <returns><c>true</c> if <paramref name='thisValue'/> is greater
    /// than <paramref name='otherValue'/> ; otherwise, <c>false</c>.</returns>
    public static bool operator >(EInteger thisValue, EInteger otherValue) {
      return (thisValue != null) && (thisValue.CompareTo(otherValue) > 0);
    }

    /// <summary>Determines whether an arbitrary-precision integer value is
    /// greater than another arbitrary-precision integer.</summary>
    /// <param name='thisValue'>The first arbitrary-precision
    /// integer.</param>
    /// <param name='otherValue'>The second arbitrary-precision
    /// integer.</param>
    /// <returns><c>true</c> if <paramref name='thisValue'/> is at least
    /// <paramref name='otherValue'/> ; otherwise, <c>false</c>.</returns>
    public static bool operator >=(
      EInteger thisValue,
      EInteger otherValue) {
      return (thisValue == null) ? (otherValue == null) :
(thisValue.CompareTo(otherValue) >= 0);
    }

    /// <summary>Returns an arbitrary-precision integer with every bit
    /// flipped.</summary>
    /// <param name='thisValue'>The operand as an arbitrary-precision
    /// integer.</param>
    /// <returns>An arbitrary-precision integer.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='thisValue'/> is null.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Usage",
      "CA2225",
      Justification = "Class implements an alternate method named Xor.")]
    public static EInteger operator ~(
      EInteger thisValue) {
      return Not(thisValue);
    }

    /// <summary>Does an AND operation between two arbitrary-precision
    /// integer values. For each bit of the result, that bit is 1 if the
    /// corresponding bits of the two operands are both 1, or is 0
    /// otherwise.</summary>
    /// <param name='thisValue'>The first operand.</param>
    /// <param name='otherValue'>The second operand.</param>
    /// <returns>The result of the operation.</returns>
    /// <exception cref='ArgumentNullException'>The parameter "a" or "b" is
    /// null.</exception>
    /// <remarks>Each arbitrary-precision integer is treated as a
    /// two's-complement form (see
    /// <see cref='PeterO.Numbers.EDecimal'>"Forms of numbers"</see> ) for
    /// the purposes of this operator.</remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Usage",
      "CA2225",
      Justification = "Class implements an alternate method named And.")]
    public static EInteger operator &(
      EInteger thisValue,
      EInteger otherValue) {
      return And(thisValue, otherValue);
    }

    /// <summary>Does an OR operation between two arbitrary-precision
    /// integer instances. For each bit of the result, that bit is 1 if
    /// either or both of the corresponding bits of the two operands are 1,
    /// or is 0 otherwise.</summary>
    /// <param name='thisValue'>An arbitrary-precision integer.</param>
    /// <param name='otherValue'>Another arbitrary-precision
    /// integer.</param>
    /// <returns>The result of the operation.</returns>
    /// <exception cref='ArgumentNullException'>The parameter "first" or
    /// "second" is null.</exception>
    /// <remarks>Each arbitrary-precision integer is treated as a
    /// two's-complement form (see
    /// <see cref='PeterO.Numbers.EDecimal'>"Forms of numbers"</see> ) for
    /// the purposes of this operator.</remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Usage",
      "CA2225",
      Justification = "Class implements an alternate method named Or.")]
    public static EInteger operator |(
      EInteger thisValue,
      EInteger otherValue) {
      return Or(thisValue, otherValue);
    }

    /// <summary>Finds the exclusive "or" of two arbitrary-precision
    /// integer objects. For each bit of the result, that bit is 1 if
    /// either of the corresponding bits of the two operands, but not both,
    /// is 1, or is 0 otherwise.
    /// <para>Each arbitrary-precision integer is treated as a
    /// two's-complement form (see
    /// <see cref='PeterO.Numbers.EDecimal'>"Forms of numbers"</see> ) for
    /// the purposes of this operator.</para></summary>
    /// <param name='a'>The first arbitrary-precision integer.</param>
    /// <param name='b'>The second arbitrary-precision integer.</param>
    /// <returns>An arbitrary-precision integer in which each bit is set if
    /// it's set in one input integer but not the other.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='a'/> or <paramref name='b'/> is null.</exception>
    public static EInteger operator ^(
      EInteger a,
      EInteger b) {
      return Xor(a, b);
    }

    /// <summary>Retrieves bits from this integer's two's-complement
    /// form.</summary>
    /// <param name='index'>Zero-based index of the first bit to retrieve,
    /// where 0 is the least-significant bit of the number.</param>
    /// <param name='numberBits'>The number of bits to retrieve, starting
    /// with the first. Must be from 0 through 64.</param>
    /// <returns>A 64-bit signed integer containing the bits from this
    /// integer's two's-complement form. The least significant bit is the
    /// first bit, and any unused bits are set to 0.</returns>
    public long GetBits(int index, int numberBits) {
      if (numberBits < 0 || numberBits > 64) {
        throw new ArgumentOutOfRangeException(nameof(numberBits));
      }
      long v = 0;
      for (int j = 0; j < numberBits; ++j) {
        v |= (long)(this.GetSignedBit((int)(index + j)) ? 1 : 0) << j;
      }
      return v;
    }

    /// <summary>Divides this arbitrary-precision integer by another
    /// arbitrary-precision integer and returns a two-item array containing
    /// the result of the division and the remainder, in that order. The
    /// result of the division is rounded down (the fractional part is
    /// discarded). Except if the result of the division is 0, it will be
    /// negative if this arbitrary-precision integer is positive and the
    /// other arbitrary-precision integer is negative, or vice versa, and
    /// will be positive if both are positive or both are negative. The
    /// remainder is the number that remains when the absolute value of
    /// this arbitrary-precision integer is divided by the absolute value
    /// of the other arbitrary-precision integer; the remainder has the
    /// same sign (positive or negative) as this arbitrary-precision
    /// integer.</summary>
    /// <param name='dividend'>The arbitrary-precision integer to be
    /// divided.</param>
    /// <param name='divisor'>The arbitrary-precision integer to divide
    /// by.</param>
    /// <param name='remainder'>An arbitrary-precision integer.</param>
    /// <returns>An array of two items: the first is the result of the
    /// division as an arbitrary-precision integer, and the second is the
    /// remainder as an arbitrary-precision integer. The result of division
    /// is the result of the Divide method on the two operands, and the
    /// remainder is the result of the Remainder method on the two
    /// operands.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='dividend'/> or <paramref name='divisor'/> is
    /// null.</exception>
    [Obsolete("Use the DivRem instance method instead.")]
    public static EInteger DivRem(
      EInteger dividend,
      EInteger divisor,
      out EInteger remainder) {
      if (dividend == null) {
        throw new ArgumentNullException(nameof(dividend));
      }
      if (divisor == null) {
        throw new ArgumentNullException(nameof(divisor));
      }
      EInteger[] result = dividend.DivRem(divisor);
      remainder = result[1];
      return result[0];
    }

    /// <summary>Determines whether this object and another object are
    /// equal.</summary>
    /// <param name='other'>Another arbitrary-precision integer.</param>
    /// <returns><c>true</c> if this object and another object are equal;
    /// otherwise, <c>false</c>.</returns>
    public bool Equals(EInteger other) {
      return (other != null) && (this.CompareTo(other) == 0);
    }

    /// <summary>Returns an arbitrary-precision integer with every bit
    /// flipped.</summary>
    /// <param name='valueA'>The operand as an arbitrary-precision
    /// integer.</param>
    /// <returns>An arbitrary-precision integer.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='valueA'/> is null.</exception>
    public static EInteger Not(EInteger valueA) {
      if (valueA == null) {
        throw new ArgumentNullException(nameof(valueA));
      }
      return valueA.Not();
    }

    /// <summary>Does an AND operation between two arbitrary-precision
    /// integer values.</summary>
    /// <param name='a'>The first arbitrary-precision integer.</param>
    /// <param name='b'>The second arbitrary-precision integer.</param>
    /// <returns>An arbitrary-precision integer in which each bit is set if
    /// the corresponding bits of the two integers are both set.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='a'/> or <paramref name='b'/> is null.</exception>
    /// <remarks>Each arbitrary-precision integer is treated as a
    /// two's-complement form (see
    /// <see cref='PeterO.Numbers.EDecimal'>"Forms of numbers"</see> ) for
    /// the purposes of this operator.</remarks>
    public static EInteger And(EInteger a, EInteger b) {
      if (a == null) {
        throw new ArgumentNullException(nameof(a));
      }
      if (b == null) {
        throw new ArgumentNullException(nameof(b));
      }
      return a.And(b);
    }

    /// <summary>Does an OR operation between two arbitrary-precision
    /// integer instances.</summary>
    /// <param name='first'>The first operand.</param>
    /// <param name='second'>The second operand.</param>
    /// <returns>An arbitrary-precision integer.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='first'/> or <paramref name='second'/> is null.</exception>
    /// <remarks>Each arbitrary-precision integer is treated as a
    /// two's-complement form (see
    /// <see cref='PeterO.Numbers.EDecimal'>"Forms of numbers"</see> ) for
    /// the purposes of this operator.</remarks>
    public static EInteger Or(EInteger first, EInteger second) {
      if (first == null) {
        throw new ArgumentNullException(nameof(first));
      }
      if (second == null) {
        throw new ArgumentNullException(nameof(second));
      }
      return first.Or(second);
    }

    /// <summary>Finds the exclusive "or" of two arbitrary-precision
    /// integer objects.
    /// <para>Each arbitrary-precision integer is treated as a
    /// two's-complement form (see
    /// <see cref='PeterO.Numbers.EDecimal'>"Forms of numbers"</see> ) for
    /// the purposes of this operator.</para></summary>
    /// <param name='a'>The first arbitrary-precision integer.</param>
    /// <param name='b'>The second arbitrary-precision integer.</param>
    /// <returns>An arbitrary-precision integer in which each bit is set if
    /// the corresponding bit is set in one input integer but not in the
    /// other.</returns>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='a'/> or <paramref name='b'/> is null.</exception>
    public static EInteger Xor(EInteger a, EInteger b) {
      if (a == null) {
        throw new ArgumentNullException(nameof(a));
      }
      if (b == null) {
        throw new ArgumentNullException(nameof(b));
      }
      return a.Xor(b);
    }
    // Begin integer conversions

    /// <summary>Converts an arbitrary-precision integer to a byte (from 0
    /// to 255) if it can fit in a byte (from 0 to 255).</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// integer.</param>
    /// <returns>The value of <paramref name='input'/> as a byte (from 0 to
    /// 255).</returns>
    /// <exception cref='OverflowException'>The parameter <paramref
    /// name='input'/> is less than 0 or greater than 255.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='input'/> is null.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Usage",
      "CA2225",
      Justification = "Class implements an alternate method named ToByteChecked.")]
    public static explicit operator byte(EInteger input) {
      if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToByteChecked();
    }

    /// <summary>Converts a byte (from 0 to 255) to an arbitrary-precision
    /// integer.</summary>
    /// <param name='boolValue'>Either <c>true</c> or <c>false</c>.</param>
    /// <returns>The value of <paramref name='boolValue'/> as an
    /// arbitrary-precision integer.</returns>
    public static explicit operator EInteger(bool boolValue) {
      return EInteger.FromBoolean(boolValue);
    }

    /// <summary>Converts a byte (from 0 to 255) to an arbitrary-precision
    /// integer.</summary>
    /// <param name='inputByte'>The number to convert as a byte (from 0 to
    /// 255).</param>
    /// <returns>The value of <paramref name='inputByte'/> as an
    /// arbitrary-precision integer.</returns>
    public static implicit operator EInteger(byte inputByte) {
      return EInteger.FromByte(inputByte);
    }

    /// <summary>Converts this number's value to an 8-bit signed integer if
    /// it can fit in an 8-bit signed integer.</summary>
    /// <returns>This number's value as an 8-bit signed integer.</returns>
    /// <exception cref='OverflowException'>This value is less than -128 or
    /// greater than 127.</exception>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public sbyte ToSByteChecked()
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            int val = this.ToInt32Checked();
      if (val < -128 || val > 127) {
        throw new OverflowException("This object's value is out of range");
      }
      return unchecked((sbyte)(val & 0xff));
    }

    /// <summary>Converts this number to an 8-bit signed integer, returning
    /// the least-significant bits of this number's two's-complement
    /// form.</summary>
    /// <returns>This number, converted to an 8-bit signed
    /// integer.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public sbyte ToSByteUnchecked()
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            int val = this.ToInt32Unchecked();
      return unchecked((sbyte)(val & 0xff));
    }

    /// <summary>Converts an 8-bit signed integer to an arbitrary-precision
    /// integer.</summary>
    /// <param name='inputSByte'>The number to convert as an 8-bit signed
    /// integer.</param>
    /// <returns>This number's value as an arbitrary-precision
    /// integer.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static EInteger FromSByte(sbyte inputSByte)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            var val = (int)inputSByte;
      return FromInt32(val);
    }

    /// <summary>Converts an arbitrary-precision integer to an 8-bit signed
    /// integer if it can fit in an 8-bit signed integer.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// integer.</param>
    /// <returns>The value of <paramref name='input'/> as an 8-bit signed
    /// integer.</returns>
    /// <exception cref='OverflowException'>The parameter <paramref
    /// name='input'/> is less than -128 or greater than 127.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='input'/> is null.</exception>
    [CLSCompliant(false)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Usage",
      "CA2225",
      Justification = "Class implements an alternate method named ToSByteChecked.")]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static explicit operator sbyte(EInteger input)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToSByteChecked();
    }

    /// <summary>Converts an 8-bit signed integer to an arbitrary-precision
    /// integer.</summary>
    /// <param name='inputSByte'>The number to convert as an 8-bit signed
    /// integer.</param>
    /// <returns>The value of <paramref name='inputSByte'/> as an
    /// arbitrary-precision integer.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static implicit operator EInteger(sbyte inputSByte)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            return EInteger.FromSByte(inputSByte);
    }

    /// <summary>Converts an arbitrary-precision integer to a 16-bit signed
    /// integer if it can fit in a 16-bit signed integer.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// integer.</param>
    /// <returns>The value of <paramref name='input'/> as a 16-bit signed
    /// integer.</returns>
    /// <exception cref='OverflowException'>The parameter <paramref
    /// name='input'/> is less than -32768 or greater than
    /// 32767.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='input'/> is null.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Usage",
      "CA2225",
      Justification = "Class implements an alternate method named ToInt16Checked.")]
    public static explicit operator short(EInteger input) {
      if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToInt16Checked();
    }

    /// <summary>Converts a 16-bit signed integer to an arbitrary-precision
    /// integer.</summary>
    /// <param name='inputInt16'>The number to convert as a 16-bit signed
    /// integer.</param>
    /// <returns>The value of <paramref name='inputInt16'/> as an
    /// arbitrary-precision integer.</returns>
    public static implicit operator EInteger(short inputInt16) {
      return EInteger.FromInt16(inputInt16);
    }

    /// <summary>Converts this number's value to a 16-bit unsigned integer
    /// if it can fit in a 16-bit unsigned integer.</summary>
    /// <returns>This number's value as a 16-bit unsigned
    /// integer.</returns>
    /// <exception cref='OverflowException'>This value is less than 0 or
    /// greater than 65535.</exception>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public ushort ToUInt16Checked()
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            int val = this.ToInt32Checked();
      if (val < 0 || val > 65535) {
        throw new OverflowException("This object's value is out of range");
      }
      return unchecked((ushort)(val & 0xffff));
    }

    /// <summary>Converts this number to a 16-bit unsigned integer,
    /// returning the least-significant bits of this number's
    /// two's-complement form.</summary>
    /// <returns>This number, converted to a 16-bit unsigned
    /// integer.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public ushort ToUInt16Unchecked()
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            int val = this.ToInt32Unchecked();
      return unchecked((ushort)(val & 0xffff));
    }

    /// <summary>Converts a 16-bit unsigned integer to an
    /// arbitrary-precision integer.</summary>
    /// <param name='inputUInt16'>The number to convert as a 16-bit
    /// unsigned integer.</param>
    /// <returns>This number's value as an arbitrary-precision
    /// integer.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static EInteger FromUInt16(ushort inputUInt16)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            int val = ((int)inputUInt16) & 0xffff;
      return FromInt32(val);
    }

    /// <summary>Converts an arbitrary-precision integer to a 16-bit
    /// unsigned integer if it can fit in a 16-bit unsigned
    /// integer.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// integer.</param>
    /// <returns>The value of <paramref name='input'/> as a 16-bit unsigned
    /// integer.</returns>
    /// <exception cref='OverflowException'>The parameter <paramref
    /// name='input'/> is less than 0 or greater than 65535.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='input'/> is null.</exception>
    [CLSCompliant(false)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Usage",
      "CA2225",
      Justification = "Class implements an alternate method named ToUInt16Checked.")]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static explicit operator ushort(EInteger input)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToUInt16Checked();
    }

    /// <summary>Converts a 16-bit unsigned integer to an
    /// arbitrary-precision integer.</summary>
    /// <param name='inputUInt16'>The number to convert as a 16-bit
    /// unsigned integer.</param>
    /// <returns>The value of <paramref name='inputUInt16'/> as an
    /// arbitrary-precision integer.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static implicit operator EInteger(ushort inputUInt16)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            return EInteger.FromUInt16(inputUInt16);
    }

    /// <summary>Converts an arbitrary-precision integer to a 32-bit signed
    /// integer if it can fit in a 32-bit signed integer.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// integer.</param>
    /// <returns>The value of <paramref name='input'/> as a 32-bit signed
    /// integer.</returns>
    /// <exception cref='OverflowException'>The parameter <paramref
    /// name='input'/> is less than -2147483648 or greater than
    /// 2147483647.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='input'/> is null.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Usage",
      "CA2225",
      Justification = "Class implements an alternate method named ToInt32Checked.")]
    public static explicit operator int(EInteger input) {
      if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToInt32Checked();
    }

    /// <summary>Converts a 32-bit signed integer to an arbitrary-precision
    /// integer.</summary>
    /// <param name='inputInt32'>The number to convert as a 32-bit signed
    /// integer.</param>
    /// <returns>The value of <paramref name='inputInt32'/> as an
    /// arbitrary-precision integer.</returns>
    public static implicit operator EInteger(int inputInt32) {
      return EInteger.FromInt32(inputInt32);
    }

    /// <summary>Converts this number's value to a 32-bit signed integer if
    /// it can fit in a 32-bit signed integer.</summary>
    /// <returns>This number's value as a 32-bit signed integer.</returns>
    /// <exception cref='OverflowException'>This value is less than 0 or
    /// greater than 4294967295.</exception>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public uint ToUInt32Checked()
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            long val = this.ToInt64Checked();
      if (val < 0 || val > 4294967295L) {
        throw new OverflowException("This object's value is out of range");
      }
      return unchecked((uint)(val & 0xffffffffL));
    }

    /// <summary>Converts this number to a 32-bit signed integer, returning
    /// the least-significant bits of this number's two's-complement
    /// form.</summary>
    /// <returns>This number, converted to a 32-bit signed
    /// integer.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public uint ToUInt32Unchecked()
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            long val = this.ToInt64Unchecked();
      return unchecked((uint)(val & 0xffffffffL));
    }

    /// <summary>Converts a 32-bit signed integer to an arbitrary-precision
    /// integer.</summary>
    /// <param name='inputUInt32'>The number to convert as a 32-bit signed
    /// integer.</param>
    /// <returns>This number's value as an arbitrary-precision
    /// integer.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static EInteger FromUInt32(uint inputUInt32)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            long val = ((long)inputUInt32) & 0xffffffffL;
      return FromInt64(val);
    }

    /// <summary>Converts an arbitrary-precision integer to a 32-bit signed
    /// integer if it can fit in a 32-bit signed integer.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// integer.</param>
    /// <returns>The value of <paramref name='input'/> as a 32-bit signed
    /// integer.</returns>
    /// <exception cref='OverflowException'>The parameter <paramref
    /// name='input'/> is less than 0 or greater than
    /// 4294967295.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='input'/> is null.</exception>
    [CLSCompliant(false)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Usage",
      "CA2225",
      Justification = "Class implements an alternate method named ToUInt32Checked.")]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static explicit operator uint(EInteger input)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToUInt32Checked();
    }

    /// <summary>Converts a 32-bit signed integer to an arbitrary-precision
    /// integer.</summary>
    /// <param name='inputUInt32'>The number to convert as a 32-bit signed
    /// integer.</param>
    /// <returns>The value of <paramref name='inputUInt32'/> as an
    /// arbitrary-precision integer.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static implicit operator EInteger(uint inputUInt32)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            return EInteger.FromUInt32(inputUInt32);
    }

    /// <summary>Converts an arbitrary-precision integer to a 64-bit signed
    /// integer if it can fit in a 64-bit signed integer.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// integer.</param>
    /// <returns>The value of <paramref name='input'/> as a 64-bit signed
    /// integer.</returns>
    /// <exception cref='OverflowException'>The parameter <paramref
    /// name='input'/> is less than -9223372036854775808 or greater than
    /// 9223372036854775807.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='input'/> is null.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Usage",
      "CA2225",
      Justification = "Class implements an alternate method named ToInt64Checked.")]
    public static explicit operator long(EInteger input) {
      if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToInt64Checked();
    }

    /// <summary>Converts a 64-bit signed integer to an arbitrary-precision
    /// integer.</summary>
    /// <param name='inputInt64'>The number to convert as a 64-bit signed
    /// integer.</param>
    /// <returns>The value of <paramref name='inputInt64'/> as an
    /// arbitrary-precision integer.</returns>
    public static implicit operator EInteger(long inputInt64) {
      return EInteger.FromInt64(inputInt64);
    }

    /// <summary>Converts an arbitrary-precision integer to a 64-bit
    /// unsigned integer if it can fit in a 64-bit unsigned
    /// integer.</summary>
    /// <param name='input'>The number to convert as an arbitrary-precision
    /// integer.</param>
    /// <returns>The value of <paramref name='input'/> as a 64-bit unsigned
    /// integer.</returns>
    /// <exception cref='OverflowException'>The parameter <paramref
    /// name='input'/> is less than 0 or greater than
    /// 18446744073709551615.</exception>
    /// <exception cref='ArgumentNullException'>The parameter <paramref
    /// name='input'/> is null.</exception>
    [CLSCompliant(false)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Usage",
      "CA2225",
      Justification = "Class implements an alternate method named ToUInt64Checked.")]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static explicit operator ulong(EInteger input)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            if (input == null) {
        throw new ArgumentNullException(nameof(input));
      }
      return input.ToUInt64Checked();
    }

    /// <summary>Converts a 64-bit unsigned integer to an
    /// arbitrary-precision integer.</summary>
    /// <param name='inputUInt64'>The number to convert as a 64-bit
    /// unsigned integer.</param>
    /// <returns>The value of <paramref name='inputUInt64'/> as an
    /// arbitrary-precision integer.</returns>
    [CLSCompliant(false)]
#pragma warning disable CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
        public static implicit operator EInteger(ulong inputUInt64)
        {
#pragma warning restore CS3021 // Le type ou le membre n'a pas besoin d'un attribut CLSCompliant, car l'assembly n'a pas d'attribut CLSCompliant
            return EInteger.FromUInt64(inputUInt64);
    }

    // End integer conversions
  }
}
