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
  internal sealed class FastIntegerFixed : IComparable<FastIntegerFixed> {
    // NOTE: Integer modes are mutually exclusive
    private enum IntegerMode : byte {
      SmallValue = 0,
      LargeValue = 2,
    }
    private const int CacheFirst = -24;
    private const int CacheLast = 128;

    private readonly int smallValue; // if integerMode is 0
    private readonly EInteger largeValue; // if integerMode is 2
    private readonly IntegerMode integerMode;

    public static readonly FastIntegerFixed Zero = new FastIntegerFixed(
      IntegerMode.SmallValue,
      0,
      null);
    public static readonly FastIntegerFixed One = new FastIntegerFixed(
      IntegerMode.SmallValue,
      1,
      null);

    private static readonly FastIntegerFixed[] Cache =
FastIntegerFixedCache(CacheFirst,
  CacheLast);

    private static FastIntegerFixed[] FastIntegerFixedCache(
      int first,
      int last) {
#if DEBUG
if (first < -65535) {
  throw new ArgumentException("first (" + first + ") is not greater or equal" +
"\u0020to " + (-65535));
}
if (first > 65535) {
  throw new ArgumentException("first (" + first + ") is not less or equal to" +
"\u002065535");
}
if (last < -65535) {
  throw new ArgumentException("last (" + last + ") is not greater or equal" +
"\u0020to -65535");
}
if (last > 65535) {
  throw new ArgumentException("last (" + last + ") is not less or equal to" +
"65535");
}
#endif
FastIntegerFixed[] cache = new FastIntegerFixed[(last - first) + 1];
for (int i = first; i <= last; ++i) {
  if (i == 0) {
    cache[i - first] = Zero;
  } else if (i == 1) {
    cache[i - first] = One;
  } else {
 cache[i - first] = new FastIntegerFixed(IntegerMode.SmallValue, i, null);
}
}
return cache;
    }

    private FastIntegerFixed(
      IntegerMode integerMode,
      int smallValue,
      EInteger largeValue) {
      this.integerMode = integerMode;
      this.smallValue = smallValue;
      this.largeValue = largeValue;
    }

    public override bool Equals(object obj) {
      if (!(obj is FastIntegerFixed fi)) {
        return false;
      }
      if (this.integerMode != fi.integerMode) {
        return false;
      }
      switch (this.integerMode) {
        case IntegerMode.SmallValue:
          return this.smallValue == fi.smallValue;
        case IntegerMode.LargeValue:
          return this.largeValue.Equals(fi.largeValue);
        default: return true;
      }
    }

    public override int GetHashCode() {
      int hash = this.integerMode.GetHashCode();
      switch (this.integerMode) {
        case IntegerMode.SmallValue:
          hash = unchecked((hash * 31) + this.smallValue);
          break;
        case IntegerMode.LargeValue:
          hash = unchecked((hash * 31) + this.largeValue.GetHashCode());
          break;
      }
      return hash;
    }

    internal static FastIntegerFixed FromInt32(int intVal) {
return (intVal >= CacheFirst && intVal <= CacheLast) ?
Cache[intVal - CacheFirst] :
      new FastIntegerFixed(IntegerMode.SmallValue, intVal, null);
    }

    internal static FastIntegerFixed FromInt64(long longVal) {
      return (longVal >= Int32.MinValue && longVal <= Int32.MaxValue) ?
FromInt32((int)longVal) : new FastIntegerFixed(
          IntegerMode.LargeValue,
          0,
          EInteger.FromInt64(longVal));
    }

    internal static FastIntegerFixed FromBig(EInteger bigintVal) {
      return bigintVal.CanFitInInt32() ?
FromInt32(bigintVal.ToInt32Unchecked()) : new
        FastIntegerFixed(IntegerMode.LargeValue, 0, bigintVal);
    }

    internal int ToInt32() {
      return (this.integerMode == IntegerMode.SmallValue) ?
        this.smallValue : this.largeValue.ToInt32Unchecked();
    }

    public static FastIntegerFixed FromFastInteger(FastInteger fi) {
      if (fi.CanFitInInt32()) {
        return FromInt32(fi.ToInt32());
      } else {
        return FastIntegerFixed.FromBig(fi.ToEInteger());
      }
    }

    public FastInteger ToFastInteger() {
      if (this.integerMode == IntegerMode.SmallValue) {
        return new FastInteger(this.smallValue);
      } else {
        return FastInteger.FromBig(this.largeValue);
      }
    }

    public FastIntegerFixed Increment() {
      if (this.integerMode == IntegerMode.SmallValue && this.smallValue !=
Int32.MaxValue) {
        return FromInt32(this.smallValue + 1);
      } else {
        return Add(this, FastIntegerFixed.One);
      }
    }

    public int Mod(int value) {
      if (value < 0) {
        throw new NotSupportedException();
      }
      if (this.integerMode == IntegerMode.SmallValue && this.smallValue >= 0) {
        return this.smallValue % value;
      } else {
        EInteger retval = this.ToEInteger().Remainder(EInteger.FromInt32(
  value));
        return retval.ToInt32Checked();
      }
    }

    public static FastIntegerFixed Add(FastIntegerFixed a,
      FastIntegerFixed b) {
      if (a.integerMode == IntegerMode.SmallValue &&
           b.integerMode == IntegerMode.SmallValue) {
        if (a.smallValue == 0) {
          return b;
        }
        if (b.smallValue == 0) {
          return a;
        }
        if (((a.smallValue | b.smallValue) >> 30) == 0) {
          return FromInt32(a.smallValue + b.smallValue);
        }
        if ((a.smallValue < 0 && b.smallValue >= Int32.MinValue -
            a.smallValue) || (a.smallValue > 0 && b.smallValue <=
            Int32.MaxValue - a.smallValue)) {
          return FromInt32(a.smallValue + b.smallValue);
        }
      }
      EInteger bigA = a.ToEInteger();
      EInteger bigB = b.ToEInteger();
      return FastIntegerFixed.FromBig(bigA.Add(bigB));
    }

    public static FastIntegerFixed Subtract(
      FastIntegerFixed a,
      FastIntegerFixed b) {
      if (a.integerMode == IntegerMode.SmallValue && b.integerMode ==
IntegerMode.SmallValue) {
        if (b.smallValue == 0) {
          return a;
        }
        if (
          (b.smallValue < 0 && Int32.MaxValue + b.smallValue >= a.smallValue) ||
          (b.smallValue > 0 && Int32.MinValue + b.smallValue <=
            a.smallValue)) {
          return FromInt32(a.smallValue - b.smallValue);
        }
      }
      EInteger bigA = a.ToEInteger();
      EInteger bigB = b.ToEInteger();
      return FastIntegerFixed.FromBig(bigA.Subtract(bigB));
    }

    public FastIntegerFixed Add(int ib) {
      FastIntegerFixed a = this;
      if (this.integerMode == IntegerMode.SmallValue) {
        if (ib == 0) {
          return this;
        }
        if (this.smallValue == 0) {
          return FromInt32(ib);
        }
        if (((a.smallValue | ib) >> 30) == 0) {
          return FromInt32(a.smallValue + ib);
        }
        if ((a.smallValue < 0 && ib >= Int32.MinValue -
            a.smallValue) || (a.smallValue > 0 && ib <=
            Int32.MaxValue - a.smallValue)) {
          return FromInt32(a.smallValue + ib);
        }
      }
      EInteger bigA = a.ToEInteger();
      return FastIntegerFixed.FromBig(bigA.Add(ib));
    }

    public FastIntegerFixed Subtract(int ib) {
      if (ib == 0) {
        return this;
      }
      if (this.integerMode == IntegerMode.SmallValue) {
        if (
          (ib < 0 && Int32.MaxValue + ib >= this.smallValue) ||
          (ib > 0 && Int32.MinValue + ib <= this.smallValue)) {
          return FromInt32(this.smallValue - ib);
        }
      }
      EInteger bigA = this.ToEInteger();
      return FastIntegerFixed.FromBig(bigA.Subtract(ib));
    }

    public FastIntegerFixed Add(
      FastIntegerFixed b) {
      return Add(this, b);
    }

    public FastIntegerFixed Subtract(
      FastIntegerFixed b) {
      return Subtract(this, b);
    }

    public FastIntegerFixed Add(
      EInteger b) {
      if (this.integerMode == IntegerMode.SmallValue && b.CanFitInInt32()) {
        return this.Add(b.ToInt32Unchecked());
      } else {
        return FastIntegerFixed.FromBig(
           this.ToEInteger().Add(b));
      }
    }

    public FastIntegerFixed Subtract(
      EInteger b) {
      if (this.integerMode == IntegerMode.SmallValue && b.CanFitInInt32()) {
        return this.Subtract(b.ToInt32Unchecked());
      } else {
        return FastIntegerFixed.FromBig(
           this.ToEInteger().Subtract(b));
      }
    }

    public FastIntegerFixed Abs() {
      switch (this.integerMode) {
        case IntegerMode.SmallValue:
          if (this.smallValue == Int32.MinValue) {
            return FastIntegerFixed.FromInt32(Int32.MaxValue).Increment();
          } else if (this.smallValue < 0) {
            return FastIntegerFixed.FromInt32(-this.smallValue);
          } else {
            return this;
          }
        case IntegerMode.LargeValue:
          return this.largeValue.Sign < 0 ? new
            FastIntegerFixed(IntegerMode.LargeValue, 0, this.largeValue.Abs()) :
            this;
        default: throw new InvalidOperationException();
      }
    }

    public FastIntegerFixed Negate() {
      switch (this.integerMode) {
        case IntegerMode.SmallValue:
          if (this.smallValue == Int32.MinValue) {
            return FastIntegerFixed.FromInt32(Int32.MaxValue).Increment();
          } else {
            return FastIntegerFixed.FromInt32(-this.smallValue);
          }
        case IntegerMode.LargeValue:
          return new FastIntegerFixed(
            IntegerMode.LargeValue,
            0,
            this.largeValue.Negate());
        default: throw new InvalidOperationException();
      }
    }

    public int CompareTo(EInteger evalue) {
      switch (this.integerMode) {
        case IntegerMode.SmallValue:
          return -evalue.CompareTo(this.smallValue);
        case IntegerMode.LargeValue:
          return this.largeValue.CompareTo(evalue);
        default: throw new InvalidOperationException();
      }
    }

    public int CompareTo(FastInteger fint) {
      switch (this.integerMode) {
        case IntegerMode.SmallValue:
          return -fint.CompareToInt(this.smallValue);
        case IntegerMode.LargeValue:
          return -fint.CompareTo(this.largeValue);
        default: throw new InvalidOperationException();
      }
    }

    public int CompareTo(FastIntegerFixed val) {
      switch (this.integerMode) {
        case IntegerMode.SmallValue:
          switch (val.integerMode) {
            case IntegerMode.SmallValue:
              int vsv = val.smallValue;
              return (this.smallValue == vsv) ? 0 : (this.smallValue < vsv ?
-1 :
                  1);
            case IntegerMode.LargeValue:
              return -val.largeValue.CompareTo(this.smallValue);
          }
          break;
        case IntegerMode.LargeValue:
          return this.largeValue.CompareTo(val.ToEInteger());
      }
      throw new InvalidOperationException();
    }

    internal FastIntegerFixed Copy() {
      switch (this.integerMode) {
        case IntegerMode.SmallValue:
          return FromInt32(this.smallValue);
        case IntegerMode.LargeValue:
          return FastIntegerFixed.FromBig(this.largeValue);
        default: throw new InvalidOperationException();
      }
    }

    internal bool IsEvenNumber {
      get {
        switch (this.integerMode) {
          case IntegerMode.SmallValue:
            return (this.smallValue & 1) == 0;
          case IntegerMode.LargeValue:
            return this.largeValue.IsEven;
          default:
            throw new InvalidOperationException();
        }
      }
    }

    internal bool CanFitInInt32() {
      return this.integerMode == IntegerMode.SmallValue ||
this.largeValue.CanFitInInt32();
    }

    /// <summary>This is an internal API.</summary>
    /// <returns>A text string.</returns>
    public override string ToString() {
      switch (this.integerMode) {
        case IntegerMode.SmallValue:
          return FastInteger.IntToString(this.smallValue);
        case IntegerMode.LargeValue:
          return this.largeValue.ToString();
        default: return String.Empty;
      }
    }

    internal int Sign {
      get {
        switch (this.integerMode) {
          case IntegerMode.SmallValue:
            return (this.smallValue == 0) ? 0 : ((this.smallValue < 0) ? -1 :
                1);
          case IntegerMode.LargeValue:
            return this.largeValue.Sign;
          default:
            return 0;
        }
      }
    }

    internal bool IsValueZero {
      get {
        switch (this.integerMode) {
          case IntegerMode.SmallValue:
            return this.smallValue == 0;
          case IntegerMode.LargeValue:
            return this.largeValue.IsZero;
          default:
            return false;
        }
      }
    }

    internal bool CanFitInInt64() {
      switch (this.integerMode) {
        case IntegerMode.SmallValue:
          return true;
        case IntegerMode.LargeValue:
          return this.largeValue
            .CanFitInInt64();
        default: throw new InvalidOperationException();
      }
    }

    internal long ToInt64() {
      switch (this.integerMode) {
        case IntegerMode.SmallValue:
          return (long)this.smallValue;
        case IntegerMode.LargeValue:
          return this.largeValue
            .ToInt64Unchecked();
        default: throw new InvalidOperationException();
      }
    }

    internal int CompareToInt64(long valLong) {
      switch (this.integerMode) {
        case IntegerMode.SmallValue:
          return (valLong == this.smallValue) ? 0 : (this.smallValue <
valLong ? -1 :
              1);
        case IntegerMode.LargeValue:
          return this.largeValue.CompareTo(valLong);
        default: return 0;
      }
    }

    internal int CompareToInt(int val) {
      switch (this.integerMode) {
        case IntegerMode.SmallValue:
          return (val == this.smallValue) ? 0 : (this.smallValue < val ? -1 :
              1);
        case IntegerMode.LargeValue:
          return this.largeValue.CompareTo((EInteger)val);
        default: return 0;
      }
    }

    internal EInteger ToEInteger() {
      switch (this.integerMode) {
        case IntegerMode.SmallValue:
          return EInteger.FromInt32(this.smallValue);
        case IntegerMode.LargeValue:
          return this.largeValue;
        default: throw new InvalidOperationException();
      }
    }
  }
}
