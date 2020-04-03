/*
Written by Peter O. in 2013.
Any copyright is dedicated to the Public Domain.
http://creativecommons.org/publicdomain/zero/1.0/
If you like this, you should donate to Peter O.
at: http://peteroupc.github.io/
 */
using System;

namespace PeterO.Numbers {
  public sealed partial class EFloat {
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Implicit(System.Single)~PeterO.Numbers.EFloat"]/*'/>
    public static implicit operator EFloat(float flt) {
      return FromSingle(flt);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Implicit(System.Double)~PeterO.Numbers.EFloat"]/*'/>
    public static implicit operator EFloat(double dbl) {
      return FromDouble(dbl);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Implicit(PeterO.Numbers.EInteger)~PeterO.Numbers.EFloat"]/*'/>
    public static implicit operator EFloat(EInteger eint) {
      return FromEInteger(eint);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Addition(PeterO.Numbers.EFloat,PeterO.Numbers.EFloat)"]/*'/>
    public static EFloat operator +(EFloat bthis, EFloat otherValue) {
      if (bthis == null) {
        throw new ArgumentNullException(nameof(bthis));
      }
      return bthis.Add(otherValue);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Subtraction(PeterO.Numbers.EFloat,PeterO.Numbers.EFloat)"]/*'/>
    public static EFloat operator -(
   EFloat bthis,
   EFloat subtrahend) {
      if (bthis == null) {
        throw new ArgumentNullException(nameof(bthis));
      }
      return bthis.Subtract(subtrahend);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Multiply(PeterO.Numbers.EFloat,PeterO.Numbers.EFloat)"]/*'/>
    public static EFloat operator *(
    EFloat operand1,
    EFloat operand2) {
      if (operand1 == null) {
        throw new ArgumentNullException(nameof(operand1));
      }
      return operand1.Multiply(operand2);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Division(PeterO.Numbers.EFloat,PeterO.Numbers.EFloat)"]/*'/>
    public static EFloat operator /(
   EFloat dividend,
   EFloat divisor) {
      if (dividend == null) {
        throw new ArgumentNullException(nameof(dividend));
      }
      return dividend.Divide(divisor);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Modulus(PeterO.Numbers.EFloat,PeterO.Numbers.EFloat)"]/*'/>
    public static EFloat operator %(
   EFloat dividend,
   EFloat divisor) {
      if (dividend == null) {
        throw new ArgumentNullException(nameof(dividend));
      }
      return dividend.Remainder(divisor, null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_UnaryNegation(PeterO.Numbers.EFloat)"]/*'/>
    public static EFloat operator -(EFloat bigValue) {
      if (bigValue == null) {
        throw new ArgumentNullException(nameof(bigValue));
      }
      return bigValue.Negate();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Explicit(PeterO.Numbers.EFloat)~PeterO.Numbers.EInteger"]/*'/>
    public static explicit operator EInteger(EFloat bigValue) {
      return bigValue.ToEInteger();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Explicit(PeterO.Numbers.EFloat)~System.Double"]/*'/>
    public static explicit operator double(EFloat bigValue) {
      return bigValue.ToDouble();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Explicit(PeterO.Numbers.EFloat)~System.Single"]/*'/>
    public static explicit operator float(EFloat bigValue) {
      return bigValue.ToSingle();
    }
    // Begin integer conversions

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Explicit(PeterO.Numbers.EFloat)~System.Byte"]/*'/>
public static explicit operator byte(EFloat input) {
 return input.ToByteChecked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Implicit(System.Byte)~PeterO.Numbers.EFloat"]/*'/>
public static implicit operator EFloat(byte inputByte) {
 return EFloat.FromByte(inputByte);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToSByteChecked"]/*'/>
[CLSCompliant(false)]
public sbyte ToSByteChecked() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
return this.IsZero ? ((sbyte)0) : this.ToEInteger().ToSByteChecked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToSByteUnchecked"]/*'/>
[CLSCompliant(false)]
public sbyte ToSByteUnchecked() {
 return this.IsFinite ? this.ToEInteger().ToSByteUnchecked() : (sbyte)0;
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToSByteIfExact"]/*'/>
[CLSCompliant(false)]
public sbyte ToSByteIfExact() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
 return this.IsZero ? ((sbyte)0) :
   this.ToEIntegerIfExact().ToSByteChecked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.FromSByte(System.SByte)"]/*'/>
[CLSCompliant(false)]
public static EFloat FromSByte(sbyte inputSByte) {
 var val = (int)inputSByte;
 return FromInt32(val);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Explicit(PeterO.Numbers.EFloat)~System.SByte"]/*'/>
[CLSCompliant(false)]
public static explicit operator sbyte(EFloat input) {
 return input.ToSByteChecked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Implicit(System.SByte)~PeterO.Numbers.EFloat"]/*'/>
[CLSCompliant(false)]
public static implicit operator EFloat(sbyte inputSByte) {
 return EFloat.FromSByte(inputSByte);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Explicit(PeterO.Numbers.EFloat)~System.Int16"]/*'/>
public static explicit operator short(EFloat input) {
 return input.ToInt16Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Implicit(System.Int16)~PeterO.Numbers.EFloat"]/*'/>
public static implicit operator EFloat(short inputInt16) {
 return EFloat.FromInt16(inputInt16);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToUInt16Checked"]/*'/>
[CLSCompliant(false)]
public ushort ToUInt16Checked() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
return this.IsZero ? ((ushort)0) : this.ToEInteger().ToUInt16Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToUInt16Unchecked"]/*'/>
[CLSCompliant(false)]
public ushort ToUInt16Unchecked() {
 return this.IsFinite ? this.ToEInteger().ToUInt16Unchecked() : (ushort)0;
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToUInt16IfExact"]/*'/>
[CLSCompliant(false)]
public ushort ToUInt16IfExact() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
 return this.IsZero ? ((ushort)0) :
   this.ToEIntegerIfExact().ToUInt16Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.FromUInt16(System.UInt16)"]/*'/>
[CLSCompliant(false)]
public static EFloat FromUInt16(ushort inputUInt16) {
 int val = ((int)inputUInt16) & 0xffff;
 return FromInt32(val);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Explicit(PeterO.Numbers.EFloat)~System.UInt16"]/*'/>
[CLSCompliant(false)]
public static explicit operator ushort(EFloat input) {
 return input.ToUInt16Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Implicit(System.UInt16)~PeterO.Numbers.EFloat"]/*'/>
[CLSCompliant(false)]
public static implicit operator EFloat(ushort inputUInt16) {
 return EFloat.FromUInt16(inputUInt16);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Explicit(PeterO.Numbers.EFloat)~System.Int32"]/*'/>
public static explicit operator int(EFloat input) {
 return input.ToInt32Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Implicit(System.Int32)~PeterO.Numbers.EFloat"]/*'/>
public static implicit operator EFloat(int inputInt32) {
 return EFloat.FromInt32(inputInt32);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToUInt32Checked"]/*'/>
[CLSCompliant(false)]
public uint ToUInt32Checked() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
return this.IsZero ? ((uint)0) : this.ToEInteger().ToUInt32Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToUInt32Unchecked"]/*'/>
[CLSCompliant(false)]
public uint ToUInt32Unchecked() {
 return this.IsFinite ? this.ToEInteger().ToUInt32Unchecked() : (uint)0;
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToUInt32IfExact"]/*'/>
[CLSCompliant(false)]
public uint ToUInt32IfExact() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
 return this.IsZero ? ((uint)0) :
   this.ToEIntegerIfExact().ToUInt32Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.FromUInt32(System.UInt32)"]/*'/>
[CLSCompliant(false)]
public static EFloat FromUInt32(uint inputUInt32) {
 long val = ((long)inputUInt32) & 0xffffffffL;
 return FromInt64(val);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Explicit(PeterO.Numbers.EFloat)~System.UInt32"]/*'/>
[CLSCompliant(false)]
public static explicit operator uint(EFloat input) {
 return input.ToUInt32Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Implicit(System.UInt32)~PeterO.Numbers.EFloat"]/*'/>
[CLSCompliant(false)]
public static implicit operator EFloat(uint inputUInt32) {
 return EFloat.FromUInt32(inputUInt32);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Explicit(PeterO.Numbers.EFloat)~System.Int64"]/*'/>
public static explicit operator long(EFloat input) {
 return input.ToInt64Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Implicit(System.Int64)~PeterO.Numbers.EFloat"]/*'/>
public static implicit operator EFloat(long inputInt64) {
 return EFloat.FromInt64(inputInt64);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToUInt64Checked"]/*'/>
[CLSCompliant(false)]
public ulong ToUInt64Checked() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
return this.IsZero ? ((ulong)0) : this.ToEInteger().ToUInt64Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToUInt64Unchecked"]/*'/>
[CLSCompliant(false)]
public ulong ToUInt64Unchecked() {
 return this.IsFinite ? this.ToEInteger().ToUInt64Unchecked() : (ulong)0;
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.ToUInt64IfExact"]/*'/>
[CLSCompliant(false)]
public ulong ToUInt64IfExact() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
 return this.IsZero ? ((ulong)0) :
   this.ToEIntegerIfExact().ToUInt64Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.FromUInt64(System.UInt64)"]/*'/>
[CLSCompliant(false)]
public static EFloat FromUInt64(ulong inputUInt64) {
 return FromEInteger(EInteger.FromUInt64(inputUInt64));
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Explicit(PeterO.Numbers.EFloat)~System.UInt64"]/*'/>
[CLSCompliant(false)]
public static explicit operator ulong(EFloat input) {
 return input.ToUInt64Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EFloat.op_Implicit(System.UInt64)~PeterO.Numbers.EFloat"]/*'/>
[CLSCompliant(false)]
public static implicit operator EFloat(ulong inputUInt64) {
 return EFloat.FromUInt64(inputUInt64);
}

// End integer conversions
  }
}
