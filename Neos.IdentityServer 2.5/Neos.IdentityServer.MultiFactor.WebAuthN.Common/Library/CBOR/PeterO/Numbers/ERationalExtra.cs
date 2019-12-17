/*
Written by Peter O. in 2013.
Any copyright is dedicated to the Public Domain.
http://creativecommons.org/publicdomain/zero/1.0/
If you like this, you should donate to Peter O.
at: http://peteroupc.github.io/
 */
using System;

namespace PeterO.Numbers {
  public sealed partial class ERational {
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Implicit(PeterO.Numbers.EInteger)~PeterO.Numbers.ERational"]/*'/>
    public static implicit operator ERational(EInteger eint) {
      return FromEInteger(eint);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Implicit(PeterO.Numbers.EDecimal)~PeterO.Numbers.ERational"]/*'/>
    public static implicit operator ERational(EDecimal eint) {
      return FromEDecimal(eint);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Implicit(PeterO.Numbers.EFloat)~PeterO.Numbers.ERational"]/*'/>
    public static implicit operator ERational(EFloat eint) {
      return FromEFloat(eint);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.FromDecimal(System.Decimal)"]/*'/>
    public static ERational FromDecimal(decimal eint) {
      return FromEDecimal(EDecimal.FromDecimal(eint));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Implicit(System.Decimal)~PeterO.Numbers.ERational"]/*'/>
    public static implicit operator ERational(decimal eint) {
      return FromDecimal(eint);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Implicit(System.Single)~PeterO.Numbers.ERational"]/*'/>
    public static implicit operator ERational(float eint) {
      return ERational.FromSingle(eint);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Implicit(System.Double)~PeterO.Numbers.ERational"]/*'/>
    public static implicit operator ERational(double eint) {
      return ERational.FromDouble(eint);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Addition(PeterO.Numbers.ERational,PeterO.Numbers.ERational)"]/*'/>
    public static ERational operator +(ERational bthis, ERational augend) {
      if (bthis == null) {
        throw new ArgumentNullException(nameof(bthis));
      }
      return bthis.Add(augend);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Subtraction(PeterO.Numbers.ERational,PeterO.Numbers.ERational)"]/*'/>
    public static ERational operator -(
   ERational bthis,
   ERational subtrahend) {
      if (bthis == null) {
        throw new ArgumentNullException(nameof(bthis));
      }
      return bthis.Subtract(subtrahend);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Multiply(PeterO.Numbers.ERational,PeterO.Numbers.ERational)"]/*'/>
    public static ERational operator *(
    ERational operand1,
    ERational operand2) {
      if (operand1 == null) {
        throw new ArgumentNullException(nameof(operand1));
      }
      return operand1.Multiply(operand2);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Division(PeterO.Numbers.ERational,PeterO.Numbers.ERational)"]/*'/>
    public static ERational operator /(
   ERational dividend,
   ERational divisor) {
      if (dividend == null) {
        throw new ArgumentNullException(nameof(dividend));
      }
      return dividend.Divide(divisor);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Modulus(PeterO.Numbers.ERational,PeterO.Numbers.ERational)"]/*'/>
    public static ERational operator %(
   ERational dividend,
   ERational divisor) {
      if (dividend == null) {
        throw new ArgumentNullException(nameof(dividend));
      }
      return dividend.Remainder(divisor);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_UnaryNegation(PeterO.Numbers.ERational)"]/*'/>
    public static ERational operator -(ERational bigValue) {
      if (bigValue == null) {
        throw new ArgumentNullException(nameof(bigValue));
      }
      return bigValue.Negate();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToDecimal"]/*'/>
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

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Explicit(PeterO.Numbers.ERational)~System.Decimal"]/*'/>
    public static explicit operator decimal(
      ERational extendedNumber) {
      return extendedNumber.ToDecimal();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Explicit(PeterO.Numbers.ERational)~PeterO.Numbers.EInteger"]/*'/>
    public static explicit operator EInteger(ERational bigValue) {
      return bigValue.ToEInteger();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Explicit(PeterO.Numbers.ERational)~System.Double"]/*'/>
    public static explicit operator double(ERational bigValue) {
      return bigValue.ToDouble();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Explicit(PeterO.Numbers.ERational)~System.Single"]/*'/>
    public static explicit operator float(ERational bigValue) {
      return bigValue.ToSingle();
    }

    // Begin integer conversions

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Explicit(PeterO.Numbers.ERational)~System.Byte"]/*'/>
public static explicit operator byte(ERational input) {
 return input.ToByteChecked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Implicit(System.Byte)~PeterO.Numbers.ERational"]/*'/>
public static implicit operator ERational(byte inputByte) {
 return ERational.FromByte(inputByte);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToSByteChecked"]/*'/>
[CLSCompliant(false)]
public sbyte ToSByteChecked() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
return this.IsZero ? ((sbyte)0) : this.ToEInteger().ToSByteChecked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToSByteUnchecked"]/*'/>
[CLSCompliant(false)]
public sbyte ToSByteUnchecked() {
 return this.IsFinite ? this.ToEInteger().ToSByteUnchecked() : (sbyte)0;
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToSByteIfExact"]/*'/>
[CLSCompliant(false)]
public sbyte ToSByteIfExact() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
 return this.IsZero ? ((sbyte)0) :
   this.ToEIntegerIfExact().ToSByteChecked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.FromSByte(System.SByte)"]/*'/>
[CLSCompliant(false)]
public static ERational FromSByte(sbyte inputSByte) {
 var val = (int)inputSByte;
 return FromInt32(val);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Explicit(PeterO.Numbers.ERational)~System.SByte"]/*'/>
[CLSCompliant(false)]
public static explicit operator sbyte(ERational input) {
 return input.ToSByteChecked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Implicit(System.SByte)~PeterO.Numbers.ERational"]/*'/>
[CLSCompliant(false)]
public static implicit operator ERational(sbyte inputSByte) {
 return ERational.FromSByte(inputSByte);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Explicit(PeterO.Numbers.ERational)~System.Int16"]/*'/>
public static explicit operator short(ERational input) {
 return input.ToInt16Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Implicit(System.Int16)~PeterO.Numbers.ERational"]/*'/>
public static implicit operator ERational(short inputInt16) {
 return ERational.FromInt16(inputInt16);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToUInt16Checked"]/*'/>
[CLSCompliant(false)]
public ushort ToUInt16Checked() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
return this.IsZero ? ((ushort)0) : this.ToEInteger().ToUInt16Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToUInt16Unchecked"]/*'/>
[CLSCompliant(false)]
public ushort ToUInt16Unchecked() {
 return this.IsFinite ? this.ToEInteger().ToUInt16Unchecked() : (ushort)0;
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToUInt16IfExact"]/*'/>
[CLSCompliant(false)]
public ushort ToUInt16IfExact() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
 return this.IsZero ? ((ushort)0) :
   this.ToEIntegerIfExact().ToUInt16Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.FromUInt16(System.UInt16)"]/*'/>
[CLSCompliant(false)]
public static ERational FromUInt16(ushort inputUInt16) {
 int val = ((int)inputUInt16) & 0xffff;
 return FromInt32(val);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Explicit(PeterO.Numbers.ERational)~System.UInt16"]/*'/>
[CLSCompliant(false)]
public static explicit operator ushort(ERational input) {
 return input.ToUInt16Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Implicit(System.UInt16)~PeterO.Numbers.ERational"]/*'/>
[CLSCompliant(false)]
public static implicit operator ERational(ushort inputUInt16) {
 return ERational.FromUInt16(inputUInt16);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Explicit(PeterO.Numbers.ERational)~System.Int32"]/*'/>
public static explicit operator int(ERational input) {
 return input.ToInt32Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Implicit(System.Int32)~PeterO.Numbers.ERational"]/*'/>
public static implicit operator ERational(int inputInt32) {
 return ERational.FromInt32(inputInt32);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToUInt32Checked"]/*'/>
[CLSCompliant(false)]
public uint ToUInt32Checked() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
return this.IsZero ? ((uint)0) : this.ToEInteger().ToUInt32Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToUInt32Unchecked"]/*'/>
[CLSCompliant(false)]
public uint ToUInt32Unchecked() {
 return this.IsFinite ? this.ToEInteger().ToUInt32Unchecked() : (uint)0;
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToUInt32IfExact"]/*'/>
[CLSCompliant(false)]
public uint ToUInt32IfExact() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
 return this.IsZero ? ((uint)0) :
   this.ToEIntegerIfExact().ToUInt32Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.FromUInt32(System.UInt32)"]/*'/>
[CLSCompliant(false)]
public static ERational FromUInt32(uint inputUInt32) {
 long val = ((long)inputUInt32) & 0xffffffffL;
 return FromInt64(val);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Explicit(PeterO.Numbers.ERational)~System.UInt32"]/*'/>
[CLSCompliant(false)]
public static explicit operator uint(ERational input) {
 return input.ToUInt32Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Implicit(System.UInt32)~PeterO.Numbers.ERational"]/*'/>
[CLSCompliant(false)]
public static implicit operator ERational(uint inputUInt32) {
 return ERational.FromUInt32(inputUInt32);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Explicit(PeterO.Numbers.ERational)~System.Int64"]/*'/>
public static explicit operator long(ERational input) {
 return input.ToInt64Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Implicit(System.Int64)~PeterO.Numbers.ERational"]/*'/>
public static implicit operator ERational(long inputInt64) {
 return ERational.FromInt64(inputInt64);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToUInt64Checked"]/*'/>
[CLSCompliant(false)]
public ulong ToUInt64Checked() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
return this.IsZero ? ((ulong)0) : this.ToEInteger().ToUInt64Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToUInt64Unchecked"]/*'/>
[CLSCompliant(false)]
public ulong ToUInt64Unchecked() {
 return this.IsFinite ? this.ToEInteger().ToUInt64Unchecked() : (ulong)0;
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToUInt64IfExact"]/*'/>
[CLSCompliant(false)]
public ulong ToUInt64IfExact() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
 return this.IsZero ? ((ulong)0) :
   this.ToEIntegerIfExact().ToUInt64Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.FromUInt64(System.UInt64)"]/*'/>
[CLSCompliant(false)]
public static ERational FromUInt64(ulong inputUInt64) {
 return FromEInteger(EInteger.FromUInt64(inputUInt64));
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Explicit(PeterO.Numbers.ERational)~System.UInt64"]/*'/>
[CLSCompliant(false)]
public static explicit operator ulong(ERational input) {
 return input.ToUInt64Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.op_Implicit(System.UInt64)~PeterO.Numbers.ERational"]/*'/>
[CLSCompliant(false)]
public static implicit operator ERational(ulong inputUInt64) {
 return ERational.FromUInt64(inputUInt64);
}

// End integer conversions
  }
}
