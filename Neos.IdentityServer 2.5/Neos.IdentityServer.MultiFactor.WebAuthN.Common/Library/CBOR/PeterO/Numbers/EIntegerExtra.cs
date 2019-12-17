/*
Written by Peter O. in 2013.
Any copyright is dedicated to the Public Domain.
http://creativecommons.org/publicdomain/zero/1.0/
If you like this, you should donate to Peter O.
at: http://peteroupc.github.io/
 */
using System;

namespace PeterO.Numbers {
  public sealed partial class EInteger {
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.FromUInt64(System.UInt64)"]/*'/>
    [CLSCompliant(false)]
    public static EInteger FromUInt64(ulong ulongValue) {
      if (ulongValue <= Int64.MaxValue) {
        return FromInt64((long)ulongValue);
      } else {
        ulongValue &= (1UL << 63) - 1;
        return EInteger.One.ShiftLeft(63).Add(FromInt64((long)ulongValue));
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_Addition(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger)"]/*'/>
    public static EInteger operator +(EInteger bthis, EInteger augend) {
      if (bthis == null) {
        throw new ArgumentNullException(nameof(bthis));
      }
      return bthis.Add(augend);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_Subtraction(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger)"]/*'/>
    public static EInteger operator -(
     EInteger bthis,
     EInteger subtrahend) {
      if (bthis == null) {
        throw new ArgumentNullException(nameof(bthis));
      }
      return bthis.Subtract(subtrahend);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_Multiply(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger)"]/*'/>
    public static EInteger operator *(
      EInteger operand1,
      EInteger operand2) {
      if (operand1 == null) {
        throw new ArgumentNullException(nameof(operand1));
      }
      return operand1.Multiply(operand2);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_Division(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger)"]/*'/>
    public static EInteger operator /(
     EInteger dividend,
     EInteger divisor) {
      if (dividend == null) {
        throw new ArgumentNullException(nameof(dividend));
      }
      return dividend.Divide(divisor);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_Modulus(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger)"]/*'/>
    public static EInteger operator %(
     EInteger dividend,
     EInteger divisor) {
      if (dividend == null) {
        throw new ArgumentNullException(nameof(dividend));
      }
      return dividend.Remainder(divisor);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_LeftShift(PeterO.Numbers.EInteger,System.Int32)"]/*'/>
    public static EInteger operator <<(EInteger bthis, int bitCount) {
      if (bthis == null) {
        throw new ArgumentNullException(nameof(bthis));
      }
      return bthis.ShiftLeft(bitCount);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ModPow(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger,PeterO.Numbers.EInteger)"]/*'/>
    public static EInteger ModPow(
  EInteger bigintValue,
  EInteger pow,
  EInteger mod) {
      if (bigintValue == null) {
        throw new ArgumentNullException(nameof(bigintValue));
      }
      return bigintValue.ModPow(pow, mod);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_RightShift(PeterO.Numbers.EInteger,System.Int32)"]/*'/>
    public static EInteger operator >>(EInteger bthis, int smallValue) {
      if (bthis == null) {
        throw new ArgumentNullException(nameof(bthis));
      }
      return bthis.ShiftRight(smallValue);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_UnaryNegation(PeterO.Numbers.EInteger)"]/*'/>
    public static EInteger operator -(EInteger bigValue) {
      if (bigValue == null) {
        throw new ArgumentNullException(nameof(bigValue));
      }
      return bigValue.Negate();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ToUInt64Checked"]/*'/>
    [CLSCompliant(false)]
    public ulong ToUInt64Checked() {
      if (this.negative || this.wordCount > 4) {
        throw new OverflowException("This object's value is out of range");
      }
      long ret = this.ToInt64Unchecked();
      if (this.GetSignedBit(63)) {
        ret |= 1L << 63;
      }
      return unchecked((ulong)ret);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ToUInt64Unchecked"]/*'/>
    [CLSCompliant(false)]
    public ulong ToUInt64Unchecked() {
      long ret = this.ToInt64Unchecked();
      if (this.GetSignedBit(63)) {
        ret |= 1L << 63;
      }
      return unchecked((ulong)ret);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_LessThan(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger)"]/*'/>
    public static bool operator <(EInteger thisValue, EInteger otherValue) {
      return (thisValue == null) ? (otherValue != null) :
        (thisValue.CompareTo(otherValue) < 0);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_LessThanOrEqual(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger)"]/*'/>
    public static bool operator <=(
    EInteger thisValue,
    EInteger otherValue) {
      return (thisValue == null) || (thisValue.CompareTo(otherValue) <= 0);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_GreaterThan(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger)"]/*'/>
    public static bool operator >(EInteger thisValue, EInteger otherValue) {
      return (thisValue != null) && (thisValue.CompareTo(otherValue) > 0);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_GreaterThanOrEqual(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger)"]/*'/>
    public static bool operator >=(
    EInteger thisValue,
    EInteger otherValue) {
      return (thisValue == null) ? (otherValue == null) :
        (thisValue.CompareTo(otherValue) >= 0);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_OnesComplement(PeterO.Numbers.EInteger)"]/*'/>
    public static EInteger operator ~(
    EInteger thisValue) {
      return Not(thisValue);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_BitwiseAnd(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger)"]/*'/>
    public static EInteger operator &(
    EInteger thisValue,
    EInteger otherValue) {
      return And(thisValue, otherValue);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_BitwiseOr(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger)"]/*'/>
    public static EInteger operator |(
    EInteger thisValue,
    EInteger otherValue) {
      return Or(thisValue, otherValue);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_ExclusiveOr(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger)"]/*'/>
    public static EInteger operator ^(
    EInteger a,
    EInteger b) {
      return Xor(a, b);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.GetBits(System.Int32,System.Int32)"]/*'/>
    public long GetBits(int index, int numberBits) {
      if (numberBits < 0 || numberBits > 64) {
        throw new ArgumentOutOfRangeException("numberBits");
      }
      long v = 0;
      for (int j = 0; j < numberBits; ++j) {
        v |= (long)(this.GetSignedBit((int)(index + j)) ? 1 : 0) << j;
      }
      return v;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.DivRem(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger,PeterO.Numbers.EInteger@)"]/*'/>
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

    private static void OrWords(short[] r, short[] a, short[] b, int n) {
      for (var i = 0; i < n; ++i) {
        r[i] = unchecked((short)(a[i] | b[i]));
      }
    }

    private static void XorWords(short[] r, short[] a, short[] b, int n) {
      for (var i = 0; i < n; ++i) {
        r[i] = unchecked((short)(a[i] ^ b[i]));
      }
    }

    private static void NotWords(short[] r, int n) {
      for (var i = 0; i < n; ++i) {
        r[i] = unchecked((short)(~r[i]));
      }
    }

    private static void AndWords(short[] r, short[] a, short[] b, int n) {
      for (var i = 0; i < n; ++i) {
        r[i] = unchecked((short)(a[i] & b[i]));
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Equals(PeterO.Numbers.EInteger)"]/*'/>
    public bool Equals(EInteger other) {
      return (other != null) && (this.CompareTo(other) == 0);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Not(PeterO.Numbers.EInteger)"]/*'/>
    public static EInteger Not(EInteger valueA) {
      if (valueA == null) {
        throw new ArgumentNullException(nameof(valueA));
      }
      if (valueA.wordCount == 0) {
        return EInteger.FromInt32(-1);
      }
      var valueXaNegative = false; int valueXaWordCount = 0;
      var valueXaReg = new short[valueA.wordCount];
      Array.Copy(valueA.words, valueXaReg, valueXaReg.Length);
      valueXaWordCount = valueA.wordCount;
      if (valueA.negative) {
        TwosComplement(valueXaReg, 0, (int)valueXaReg.Length);
      }
      NotWords(valueXaReg, (int)valueXaReg.Length);
      if (valueA.negative) {
        TwosComplement(valueXaReg, 0, (int)valueXaReg.Length);
      }
      valueXaNegative = !valueA.negative;
      valueXaWordCount = CountWords(valueXaReg);
      return (valueXaWordCount == 0) ? EInteger.Zero : (new
        EInteger(valueXaWordCount, valueXaReg, valueXaNegative));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.And(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger)"]/*'/>
    public static EInteger And(EInteger a, EInteger b) {
      if (a == null) {
        throw new ArgumentNullException(nameof(a));
      }
      if (b == null) {
        throw new ArgumentNullException(nameof(b));
      }
      if (b.IsZero || a.IsZero) {
        return Zero;
      }
      var valueXaNegative = false; int valueXaWordCount = 0;
      var valueXaReg = new short[a.wordCount];
      Array.Copy(a.words, valueXaReg, valueXaReg.Length);
      var valueXbNegative = false;
      var valueXbReg = new short[b.wordCount];
      Array.Copy(b.words, valueXbReg, valueXbReg.Length);
      valueXaNegative = a.negative;
      valueXaWordCount = a.wordCount;
      valueXbNegative = b.negative;
      valueXaReg = CleanGrow(
  valueXaReg,
  Math.Max(valueXaReg.Length, valueXbReg.Length));
      valueXbReg = CleanGrow(
  valueXbReg,
  Math.Max(valueXaReg.Length, valueXbReg.Length));
      if (valueXaNegative) {
        {
          TwosComplement(valueXaReg, 0, (int)valueXaReg.Length);
        }
      }
      if (valueXbNegative) {
        {
          TwosComplement(valueXbReg, 0, (int)valueXbReg.Length);
        }
      }
      valueXaNegative &= valueXbNegative;
      AndWords(valueXaReg, valueXaReg, valueXbReg, (int)valueXaReg.Length);
      if (valueXaNegative) {
        {
          TwosComplement(valueXaReg, 0, (int)valueXaReg.Length);
        }
      }
      valueXaWordCount = CountWords(valueXaReg);
      return (valueXaWordCount == 0) ? EInteger.Zero : (new
        EInteger(valueXaWordCount, valueXaReg, valueXaNegative));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Or(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger)"]/*'/>
    public static EInteger Or(EInteger first, EInteger second) {
      if (first == null) {
        throw new ArgumentNullException(nameof(first));
      }
      if (second == null) {
        throw new ArgumentNullException(nameof(second));
      }
      if (first.wordCount == 0) {
        return second;
      }
      if (second.wordCount == 0) {
        return first;
      }
      var valueXaNegative = false; int valueXaWordCount = 0;
      var valueXaReg = new short[first.wordCount];
      Array.Copy(first.words, valueXaReg, valueXaReg.Length);
      var valueXbNegative = false;
      var valueXbReg = new short[second.wordCount];
      Array.Copy(second.words, valueXbReg, valueXbReg.Length);
      valueXaNegative = first.negative;
      valueXaWordCount = first.wordCount;
      valueXbNegative = second.negative;
      valueXaReg = CleanGrow(
  valueXaReg,
  Math.Max(valueXaReg.Length, valueXbReg.Length));
      valueXbReg = CleanGrow(
  valueXbReg,
  Math.Max(valueXaReg.Length, valueXbReg.Length));
      if (valueXaNegative) {
        TwosComplement(valueXaReg, 0, (int)valueXaReg.Length);
      }
      if (valueXbNegative) {
        TwosComplement(valueXbReg, 0, (int)valueXbReg.Length);
      }
      valueXaNegative |= valueXbNegative;
      OrWords(valueXaReg, valueXaReg, valueXbReg, (int)valueXaReg.Length);
      if (valueXaNegative) {
        TwosComplement(valueXaReg, 0, (int)valueXaReg.Length);
      }
      valueXaWordCount = CountWords(valueXaReg);
      return (valueXaWordCount == 0) ? EInteger.Zero : (new
        EInteger(valueXaWordCount, valueXaReg, valueXaNegative));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Xor(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger)"]/*'/>
    public static EInteger Xor(EInteger a, EInteger b) {
      if (a == null) {
        throw new ArgumentNullException(nameof(a));
      }
      if (b == null) {
        throw new ArgumentNullException(nameof(b));
      }
      if (a == b) {
        return EInteger.Zero;
      }
      if (a.wordCount == 0) {
        return b;
      }
      if (b.wordCount == 0) {
        return a;
      }
      var valueXaNegative = false; int valueXaWordCount = 0;
      var valueXaReg = new short[a.wordCount];
      Array.Copy(a.words, valueXaReg, valueXaReg.Length);
      var valueXbNegative = false;
      var valueXbReg = new short[b.wordCount];
      Array.Copy(b.words, valueXbReg, valueXbReg.Length);
      valueXaNegative = a.negative;
      valueXaWordCount = a.wordCount;
      valueXbNegative = b.negative;
      valueXaReg = CleanGrow(
  valueXaReg,
  Math.Max(valueXaReg.Length, valueXbReg.Length));
      valueXbReg = CleanGrow(
  valueXbReg,
  Math.Max(valueXaReg.Length, valueXbReg.Length));
      if (valueXaNegative) {
        TwosComplement(valueXaReg, 0, (int)valueXaReg.Length);
      }
      if (valueXbNegative) {
        TwosComplement(valueXbReg, 0, (int)valueXbReg.Length);
      }
      valueXaNegative ^= valueXbNegative;
      XorWords(valueXaReg, valueXaReg, valueXbReg, (int)valueXaReg.Length);
      if (valueXaNegative) {
        TwosComplement(valueXaReg, 0, (int)valueXaReg.Length);
      }
      valueXaWordCount = CountWords(valueXaReg);
      return (valueXaWordCount == 0) ? EInteger.Zero : (new
        EInteger(valueXaWordCount, valueXaReg, valueXaNegative));
    }
    // Begin integer conversions

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_Explicit(PeterO.Numbers.EInteger)~System.Byte"]/*'/>
    public static explicit operator byte(EInteger input) {
      return input.ToByteChecked();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_Implicit(System.Byte)~PeterO.Numbers.EInteger"]/*'/>
    public static implicit operator EInteger(byte inputByte) {
      return EInteger.FromByte(inputByte);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ToSByteChecked"]/*'/>
    [CLSCompliant(false)]
    public sbyte ToSByteChecked() {
      int val = this.ToInt32Checked();
      if (val < -128 || val > 127) {
        throw new OverflowException("This object's value is out of range");
      }
      return unchecked((sbyte)(val & 0xff));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ToSByteUnchecked"]/*'/>
    [CLSCompliant(false)]
    public sbyte ToSByteUnchecked() {
      int val = this.ToInt32Unchecked();
      return unchecked((sbyte)(val & 0xff));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.FromSByte(System.SByte)"]/*'/>
    [CLSCompliant(false)]
    public static EInteger FromSByte(sbyte inputSByte) {
      var val = (int)inputSByte;
      return FromInt32(val);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_Explicit(PeterO.Numbers.EInteger)~System.SByte"]/*'/>
    [CLSCompliant(false)]
    public static explicit operator sbyte(EInteger input) {
      return input.ToSByteChecked();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_Implicit(System.SByte)~PeterO.Numbers.EInteger"]/*'/>
    [CLSCompliant(false)]
    public static implicit operator EInteger(sbyte inputSByte) {
      return EInteger.FromSByte(inputSByte);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_Explicit(PeterO.Numbers.EInteger)~System.Int16"]/*'/>
    public static explicit operator short(EInteger input) {
      return input.ToInt16Checked();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_Implicit(System.Int16)~PeterO.Numbers.EInteger"]/*'/>
    public static implicit operator EInteger(short inputInt16) {
      return EInteger.FromInt16(inputInt16);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ToUInt16Checked"]/*'/>
    [CLSCompliant(false)]
    public ushort ToUInt16Checked() {
      int val = this.ToInt32Checked();
      if (val < 0 || val > 65535) {
        throw new OverflowException("This object's value is out of range");
      }
      return unchecked((ushort)(val & 0xffff));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ToUInt16Unchecked"]/*'/>
    [CLSCompliant(false)]
    public ushort ToUInt16Unchecked() {
      int val = this.ToInt32Unchecked();
      return unchecked((ushort)(val & 0xffff));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.FromUInt16(System.UInt16)"]/*'/>
    [CLSCompliant(false)]
    public static EInteger FromUInt16(ushort inputUInt16) {
      int val = ((int)inputUInt16) & 0xffff;
      return FromInt32(val);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_Explicit(PeterO.Numbers.EInteger)~System.UInt16"]/*'/>
    [CLSCompliant(false)]
    public static explicit operator ushort(EInteger input) {
      return input.ToUInt16Checked();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_Implicit(System.UInt16)~PeterO.Numbers.EInteger"]/*'/>
    [CLSCompliant(false)]
    public static implicit operator EInteger(ushort inputUInt16) {
      return EInteger.FromUInt16(inputUInt16);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_Explicit(PeterO.Numbers.EInteger)~System.Int32"]/*'/>
    public static explicit operator int(EInteger input) {
      return input.ToInt32Checked();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_Implicit(System.Int32)~PeterO.Numbers.EInteger"]/*'/>
    public static implicit operator EInteger(int inputInt32) {
      return EInteger.FromInt32(inputInt32);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ToUInt32Checked"]/*'/>
    [CLSCompliant(false)]
    public uint ToUInt32Checked() {
      long val = this.ToInt64Checked();
      if (val < 0 || val > 4294967295L) {
        throw new OverflowException("This object's value is out of range");
      }
      return unchecked((uint)(val & 0xffffffffL));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ToUInt32Unchecked"]/*'/>
    [CLSCompliant(false)]
    public uint ToUInt32Unchecked() {
      long val = this.ToInt64Unchecked();
      return unchecked((uint)(val & 0xffffffffL));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.FromUInt32(System.UInt32)"]/*'/>
    [CLSCompliant(false)]
    public static EInteger FromUInt32(uint inputUInt32) {
      long val = ((long)inputUInt32) & 0xffffffffL;
      return FromInt64(val);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_Explicit(PeterO.Numbers.EInteger)~System.UInt32"]/*'/>
    [CLSCompliant(false)]
    public static explicit operator uint(EInteger input) {
      return input.ToUInt32Checked();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_Implicit(System.UInt32)~PeterO.Numbers.EInteger"]/*'/>
    [CLSCompliant(false)]
    public static implicit operator EInteger(uint inputUInt32) {
      return EInteger.FromUInt32(inputUInt32);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_Explicit(PeterO.Numbers.EInteger)~System.Int64"]/*'/>
    public static explicit operator long(EInteger input) {
      return input.ToInt64Checked();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_Implicit(System.Int64)~PeterO.Numbers.EInteger"]/*'/>
    public static implicit operator EInteger(long inputInt64) {
      return EInteger.FromInt64(inputInt64);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_Explicit(PeterO.Numbers.EInteger)~System.UInt64"]/*'/>
    [CLSCompliant(false)]
    public static explicit operator ulong(EInteger input) {
      return input.ToUInt64Checked();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.op_Implicit(System.UInt64)~PeterO.Numbers.EInteger"]/*'/>
    [CLSCompliant(false)]
    public static implicit operator EInteger(ulong inputUInt64) {
      return EInteger.FromUInt64(inputUInt64);
    }

    // End integer conversions
  }
}
