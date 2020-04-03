/*
Written by Peter O. in 2013.
Any copyright is dedicated to the Public Domain.
http://creativecommons.org/publicdomain/zero/1.0/
If you like this, you should donate to Peter O.
at: http://peteroupc.github.io/
 */
using System;

namespace PeterO.Numbers {
  internal sealed class FastInteger : IComparable<FastInteger> {
    private sealed class MutableNumber {
      private int[] data;
      private int wordCount;

      internal static MutableNumber FromEInteger(EInteger bigintVal) {
        var mnum = new MutableNumber(0);
        if (bigintVal.Sign < 0) {
          throw new ArgumentException("bigintVal's sign (" + bigintVal.Sign +
            ") is less than " + "0 ");
        }
        byte[] bytes = bigintVal.ToBytes(true);
        int len = bytes.Length;
        int newWordCount = Math.Max(4, (len / 4) + 1);
        if (newWordCount > mnum.data.Length) {
          mnum.data = new int[newWordCount];
        }
        mnum.wordCount = newWordCount;
        unchecked {
          for (var i = 0; i < len; i += 4) {
            int x = ((int)bytes[i]) & 0xff;
            if (i + 1 < len) {
              x |= (((int)bytes[i + 1]) & 0xff) << 8;
            }
            if (i + 2 < len) {
              x |= (((int)bytes[i + 2]) & 0xff) << 16;
            }
            if (i + 3 < len) {
              x |= (((int)bytes[i + 3]) & 0xff) << 24;
            }
            mnum.data[i >> 2] = x;
          }
        }
        // Calculate the correct data length
        while (mnum.wordCount != 0 && mnum.data[mnum.wordCount - 1] == 0) {
          --mnum.wordCount;
        }
        return mnum;
      }

      internal MutableNumber(int val) {
        if (val < 0) {
          throw new ArgumentException("val (" + val + ") is less than " + "0 ");
        }
        this.data = new int[4];
        this.wordCount = (val == 0) ? 0 : 1;
        this.data[0] = val;
      }

      internal MutableNumber SetInt(int val) {
        if (val < 0) {
          throw new ArgumentException("val (" + val + ") is less than " + "0 ");
        }
        this.wordCount = (val == 0) ? 0 : 1;
        this.data[0] = val;
        return this;
      }

      internal EInteger ToEInteger() {
        if (this.wordCount == 1 && (this.data[0] >> 31) == 0) {
          return (EInteger)((int)this.data[0]);
        }
        if (this.wordCount == 2 && (this.data[1] >> 31) == 0) {
          long longV = unchecked((long)this.data[0]);
          longV &= 0xffffffffL;
          longV |= unchecked(((long)this.data[1]) << 32);
          return EInteger.FromInt64(longV);
        }
        return EInteger.FromInts(this.data, this.wordCount);
        /* var bytes = new byte[(this.wordCount >> 2) + 1];
        var i = 0;
        var j = 0;
        for (i = 0, j = 0; i < this.wordCount; ++i) {
          int d = this.data[i];
          bytes[j++] = unchecked((byte)(d));
          bytes[j++] = unchecked((byte)(d >> 8));
          bytes[j++] = unchecked((byte)(d >> 16));
          bytes[j++] = unchecked((byte)(d >> 24));
        }
        bytes[bytes.Length - 1] = (byte)0;
        return EInteger.FromBytes(bytes, true);*/
      }

      internal int[] GetLastWordsInternal(int numWords32Bit) {
        var ret = new int[numWords32Bit];
        Array.Copy(this.data, ret, Math.Min(numWords32Bit, this.wordCount));
        return ret;
      }

      internal bool CanFitInInt32() {
        return this.wordCount == 0 || (this.wordCount == 1 && (this.data[0] >>
        31) == 0);
      }

      internal int ToInt32() {
        return this.wordCount == 0 ? 0 : this.data[0];
      }

    public static MutableNumber FromLong(long longVal) {
      if (longVal < 0) {
        throw new ArgumentException();
      }
      if (longVal == 0) {
 return new MutableNumber(0);
}
      var mbi = new MutableNumber(0);
      mbi.data[0] = unchecked((int)longVal);
        int mbd = unchecked((int)(longVal >> 32));
      mbi.data[1] = mbd;
      mbi.wordCount = (mbd == 0) ? 1 : 2;
      return mbi;
    }

      internal MutableNumber Copy() {
        var mbi = new MutableNumber(0);
        if (this.wordCount > mbi.data.Length) {
          mbi.data = new int[this.wordCount];
        }
        Array.Copy(this.data, mbi.data, this.wordCount);
        mbi.wordCount = this.wordCount;
        return mbi;
      }

        internal MutableNumber Multiply(int multiplicand) {
        if (multiplicand < 0) {
          throw new ArgumentException("multiplicand (" + multiplicand +
            ") is less than " + "0 ");
        }
        if (multiplicand != 0) {
          var carry = 0;
          if (this.wordCount == 0) {
            if (this.data.Length == 0) {
              this.data = new int[4];
            }
            this.data[0] = 0;
            this.wordCount = 1;
          }
          int result0, result1, result2, result3;
          if (multiplicand < 65536) {
            if (this.wordCount == 2 && (this.data[1] >> 16) == 0) {
              long longV = unchecked((long)this.data[0]);
              longV &= 0xffffffffL;
              longV |= unchecked(((long)this.data[1]) << 32);
              longV = unchecked(longV * multiplicand);
              this.data[0] = unchecked((int)longV);
              this.data[1] = unchecked((int)(longV >> 32));
              carry = 0;
            } else if (this.wordCount == 1) {
              long longV = unchecked((long)this.data[0]);
              longV &= 0xffffffffL;
              longV = unchecked(longV * multiplicand);
              this.data[0] = unchecked((int)longV);
              carry = unchecked((int)(longV >> 32));
            } else {
            for (var i = 0; i < this.wordCount; ++i) {
              int x0 = this.data[i];
              int x1 = x0;
              int y0 = multiplicand;
              x0 &= 65535;
              x1 = (x1 >> 16) & 65535;
              int temp = unchecked(x0 * y0);  // a * c
              result1 = (temp >> 16) & 65535;
              result0 = temp & 65535;
              result2 = 0;
              temp = unchecked(x1 * y0);  // b * c
              result2 += (temp >> 16) & 65535;
              result1 += temp & 65535;
              result2 += (result1 >> 16) & 65535;
              result1 &= 65535;
              result3 = (result2 >> 16) & 65535;
              result2 &= 65535;
              // Add carry
              x0 = unchecked((int)(result0 | (result1 << 16)));
              x1 = unchecked((int)(result2 | (result3 << 16)));
              int x2 = unchecked(x0 + carry);
              if (((x2 >> 31) == (x0 >> 31)) ? ((x2 & Int32.MaxValue) < (x0 &
              Int32.MaxValue)) : ((x2 >> 31) == 0)) {
                // Carry in addition
                x1 = unchecked(x1 + 1);
              }
              this.data[i] = x2;
              carry = x1;
            }
            }
          } else {
            if (this.wordCount == 1) {
              long longV = unchecked((long)this.data[0]);
              longV &= 0xffffffffL;
              longV = unchecked(longV * multiplicand);
              this.data[0] = unchecked((int)longV);
              carry = unchecked((int)(longV >> 32));
            } else {
            for (var i = 0; i < this.wordCount; ++i) {
              int x0 = this.data[i];
              int x1 = x0;
              int y0 = multiplicand;
              int y1 = y0;
              x0 &= 65535;
              y0 &= 65535;
              x1 = (x1 >> 16) & 65535;
              y1 = (y1 >> 16) & 65535;
              int temp = unchecked(x0 * y0);  // a * c
              result1 = (temp >> 16) & 65535;
              result0 = temp & 65535;
              temp = unchecked(x0 * y1);  // a * d
              result2 = (temp >> 16) & 65535;
              result1 += temp & 65535;
              result2 += (result1 >> 16) & 65535;
              result1 &= 65535;
              temp = unchecked(x1 * y0);  // b * c
              result2 += (temp >> 16) & 65535;
              result1 += temp & 65535;
              result2 += (result1 >> 16) & 65535;
              result1 &= 65535;
              result3 = (result2 >> 16) & 65535;
              result2 &= 65535;
              temp = unchecked(x1 * y1);  // b * d
              result3 += (temp >> 16) & 65535;
              result2 += temp & 65535;
              result3 += (result2 >> 16) & 65535;
              result2 &= 65535;
              // Add carry
              x0 = unchecked((int)(result0 | (result1 << 16)));
              x1 = unchecked((int)(result2 | (result3 << 16)));
              int x2 = unchecked(x0 + carry);
              if (((x2 >> 31) == (x0 >> 31)) ? ((x2 & Int32.MaxValue) < (x0 &
              Int32.MaxValue)) : ((x2 >> 31) == 0)) {
                // Carry in addition
                x1 = unchecked(x1 + 1);
              }
              this.data[i] = x2;
              carry = x1;
            }
            }
          }
          if (carry != 0) {
            if (this.wordCount >= this.data.Length) {
              var newdata = new int[this.wordCount + 20];
              Array.Copy(this.data, 0, newdata, 0, this.data.Length);
              this.data = newdata;
            }
            this.data[this.wordCount] = carry;
            ++this.wordCount;
          }
          // Calculate the correct data length
          while (this.wordCount != 0 && this.data[this.wordCount - 1] == 0) {
            --this.wordCount;
          }
        } else {
          if (this.data.Length > 0) {
            this.data[0] = 0;
          }
          this.wordCount = 0;
        }
        return this;
      }

      internal int Sign {
        get {
          return this.wordCount == 0 ? 0 : 1;
        }
      }

     internal bool IsEvenNumber {
        get {
          return this.wordCount == 0 || (this.data[0] & 1) == 0;
        }
      }

      internal int CompareToInt(int val) {
        if (val < 0 || this.wordCount > 1) {
          return 1;
        }
        if (this.wordCount == 0) {
          // this value is 0
          return (val == 0) ? 0 : -1;
        }
        if (this.data[0] == val) {
          return 0;
        }
        return (((this.data[0] >> 31) == (val >> 31)) ? ((this.data[0] &
        Int32.MaxValue) < (val & Int32.MaxValue)) :
                  ((this.data[0] >> 31) == 0)) ? -1 : 1;
      }

      internal MutableNumber SubtractInt(int other) {
        if (other < 0) {
     throw new ArgumentException("other (" + other + ") is less than " +
            "0 ");
        }
      if (other != 0) {
          unchecked {
            // Ensure a length of at least 1
            if (this.wordCount == 0) {
              if (this.data.Length == 0) {
                this.data = new int[4];
              }
              this.data[0] = 0;
              this.wordCount = 1;
            }
            int borrow;
            int u;
            int a = this.data[0];
            u = a - other;
            borrow = ((((a >> 31) == (u >> 31)) ?
                    ((a & Int32.MaxValue) < (u & Int32.MaxValue)) :
                    ((a >> 31) == 0)) || (a == u && other != 0)) ? 1 : 0;
            this.data[0] = (int)u;
            if (borrow != 0) {
              for (int i = 1; i < this.wordCount; ++i) {
                u = this.data[i] - borrow;
                borrow = (((this.data[i] >> 31) == (u >> 31)) ?
                ((this.data[i] & Int32.MaxValue) < (u & Int32.MaxValue)) :
                    ((this.data[i] >> 31) == 0)) ? 1 : 0;
                this.data[i] = (int)u;
              }
            }
            // Calculate the correct data length
            while (this.wordCount != 0 && this.data[this.wordCount - 1] == 0) {
              --this.wordCount;
            }
          }
        }
        return this;
      }

      internal MutableNumber Subtract(MutableNumber other) {
        unchecked {
          {
       // Console.WriteLine("" + this.data.Length + " " +
             // (other.data.Length));
            int neededSize = (this.wordCount > other.wordCount) ?
            this.wordCount : other.wordCount;
            if (this.data.Length < neededSize) {
              var newdata = new int[neededSize + 20];
              Array.Copy(this.data, 0, newdata, 0, this.data.Length);
              this.data = newdata;
            }
            neededSize = (this.wordCount < other.wordCount) ? this.wordCount :
            other.wordCount;
            var u = 0;
            var borrow = 0;
            for (var i = 0; i < neededSize; ++i) {
              int a = this.data[i];
              u = (a - other.data[i]) - borrow;
              borrow = ((((a >> 31) == (u >> 31)) ? ((a & Int32.MaxValue) <
              (u & Int32.MaxValue)) :
                    ((a >> 31) == 0)) || (a == u && other.data[i] !=
                    0)) ? 1 : 0;
              this.data[i] = (int)u;
            }
            if (borrow != 0) {
              for (int i = neededSize; i < this.wordCount; ++i) {
                int a = this.data[i];
                u = (a - other.data[i]) - borrow;
                borrow = ((((a >> 31) == (u >> 31)) ? ((a & Int32.MaxValue) <
                (u & Int32.MaxValue)) :
                    ((a >> 31) == 0)) || (a == u && other.data[i] !=
                    0)) ? 1 : 0;
                this.data[i] = (int)u;
              }
            }
            // Calculate the correct data length
            while (this.wordCount != 0 && this.data[this.wordCount - 1] == 0) {
              --this.wordCount;
            }
            return this;
          }
        }
      }

      public int CompareTo(MutableNumber other) {
        if (this.wordCount != other.wordCount) {
          return (this.wordCount < other.wordCount) ? -1 : 1;
        }
        int valueN = this.wordCount;
        while (unchecked(valueN--) != 0) {
          int an = this.data[valueN];
          int bn = other.data[valueN];
          // Unsigned less-than check
          if (((an >> 31) == (bn >> 31)) ?
              ((an & Int32.MaxValue) < (bn & Int32.MaxValue)) :
              ((an >> 31) == 0)) {
            return -1;
          }
          if (an != bn) {
            return 1;
          }
        }
        return 0;
      }

       internal MutableNumber Add(int augend) {
        if (augend < 0) {
   throw new ArgumentException("augend (" + augend + ") is less than " +
            "0 ");
        }
        unchecked {
        if (augend != 0) {
          var carry = 0;
          // Ensure a length of at least 1
          if (this.wordCount == 0) {
            if (this.data.Length == 0) {
              this.data = new int[4];
            }
            this.data[0] = 0;
            this.wordCount = 1;
          }
          for (var i = 0; i < this.wordCount; ++i) {
            int u;
            int a = this.data[i];
            u = (a + augend) + carry;
            carry = ((((u >> 31) == (a >> 31)) ? ((u & Int32.MaxValue) < (a &
            Int32.MaxValue)) :
                    ((u >> 31) == 0)) || (u == a && augend != 0)) ? 1 : 0;
            this.data[i] = u;
            if (carry == 0) {
              return this;
            }
            augend = 0;
          }
          if (carry != 0) {
            if (this.wordCount >= this.data.Length) {
              var newdata = new int[this.wordCount + 20];
              Array.Copy(this.data, 0, newdata, 0, this.data.Length);
              this.data = newdata;
            }
            this.data[this.wordCount] = carry;
            ++this.wordCount;
          }
        }
        // Calculate the correct data length
        while (this.wordCount != 0 && this.data[this.wordCount - 1] == 0) {
          --this.wordCount;
        }
        return this;
      }
    }
    }

    private int smallValue;  // if integerMode is 0
    private MutableNumber mnum;  // if integerMode is 1
    private EInteger largeValue;  // if integerMode is 2
    private int integerMode;

    private bool frozen;

    private static readonly EInteger ValueInt32MinValue =
      (EInteger)Int32.MinValue;

    private static readonly EInteger ValueInt32MaxValue =
      (EInteger)Int32.MaxValue;

    private static readonly EInteger ValueNegativeInt32MinValue =
    -(EInteger)ValueInt32MinValue;

    internal FastInteger(int value) {
      this.smallValue = value;
    }

    internal FastInteger Copy() {
      var fi = new FastInteger(this.smallValue);
      fi.integerMode = this.integerMode;
      fi.largeValue = this.largeValue;
      fi.mnum = (this.mnum == null || this.integerMode != 1) ? null :
      this.mnum.Copy();
      return fi;
    }

    internal static FastInteger CopyFrozen(FastInteger value) {
      var fi = new FastInteger(value.smallValue);
      fi.integerMode = value.integerMode;
      fi.largeValue = value.largeValue;
      fi.mnum = (value.mnum == null || value.integerMode != 1) ? null :
      value.mnum.Copy();
      fi.frozen = true;
      return fi;
    }

    internal static FastInteger FromBig(EInteger bigintVal) {
      if (bigintVal.CanFitInInt32()) {
        return new FastInteger(bigintVal.ToInt32Unchecked());
      }
      if (bigintVal.Sign > 0) {
        var fi = new FastInteger(0);
        fi.integerMode = 1;
        fi.mnum = MutableNumber.FromEInteger(bigintVal);
        return fi;
      } else {
        var fi = new FastInteger(0);
        fi.integerMode = 2;
        fi.largeValue = bigintVal;
        return fi;
      }
    }

    internal int AsInt32() {
      switch (this.integerMode) {
        case 0:
          return this.smallValue;
        case 1:
          return this.mnum.ToInt32();
        case 2:
          return (int)this.largeValue;
        default: throw new InvalidOperationException();
      }
    }

    private void CheckFrozen() {
#if DEBUG
            if (this.frozen) {
 throw new InvalidOperationException();
}
#endif
    }

     public int CompareTo(FastInteger val) {
      switch ((this.integerMode << 2) | val.integerMode) {
          case (0 << 2) | 0: {
            int vsv = val.smallValue;
        return (this.smallValue == vsv) ? 0 : (this.smallValue < vsv ? -1 :
              1);
          }
        case (0 << 2) | 1:
          return -val.mnum.CompareToInt(this.smallValue);
        case (0 << 2) | 2:
          return this.AsEInteger().CompareTo(val.largeValue);
        case (1 << 2) | 0:
          return this.mnum.CompareToInt(val.smallValue);
        case (1 << 2) | 1:
          return this.mnum.CompareTo(val.mnum);
        case (1 << 2) | 2:
          return this.AsEInteger().CompareTo(val.largeValue);
        case (2 << 2) | 0:
        case (2 << 2) | 1:
        case (2 << 2) | 2:
          return this.largeValue.CompareTo(val.AsEInteger());
        default: throw new InvalidOperationException();
      }
    }

    internal FastInteger Abs() {
      if (this.frozen) {
 throw new InvalidOperationException();
}
      switch (this.integerMode) {
        case 0:
          if (this.smallValue == Int32.MinValue) {
            return this.Negate();
          }
          this.smallValue = Math.Abs(this.smallValue);
          return this;
        default:
          return (this.Sign < 0) ? this.Negate() : this;
      }
    }

    internal static int[] GetLastWords(EInteger bigint, int numWords32Bit) {
      return
      MutableNumber.FromEInteger(bigint).GetLastWordsInternal(numWords32Bit);
    }

    internal FastInteger SetInt(int val) {
      this.CheckFrozen();
      this.smallValue = val;
      this.integerMode = 0;
      return this;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.FastInteger.Multiply(System.Int32)"]/*'/>
    internal FastInteger Multiply(int val) {
      this.CheckFrozen();
      if (val == 0) {
        this.smallValue = 0;
        this.integerMode = 0;
      } else {
        switch (this.integerMode) {
          case 0: {
            long amult = ((long)val) * ((long)this.smallValue);
            if (amult > Int32.MaxValue || amult < Int32.MinValue) {
              // would overflow, convert to large
             bool apos = this.smallValue > 0L;
             bool bpos = val > 0L;
              if (apos && bpos) {
                // if both operands are nonnegative
                // convert to mutable big integer
                this.integerMode = 1;
                this.mnum = MutableNumber.FromLong(amult);
              } else {
                // if either operand is negative
                // convert to big integer
                this.integerMode = 2;
                this.largeValue = EInteger.FromInt64(amult);
              }
            } else {
              this.smallValue = unchecked((int)amult);
            }
            break;
          }
          case 1:
            if (val < 0) {
              this.integerMode = 2;
              this.largeValue = this.mnum.ToEInteger();
              this.largeValue *= (EInteger)val;
            } else {
              mnum.Multiply(val);
            }
            break;
          case 2:
            this.largeValue *= (EInteger)val;
            break;
          default: throw new InvalidOperationException();
        }
      }
      return this;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.FastInteger.Negate"]/*'/>
    internal FastInteger Negate() {
      this.CheckFrozen();
      switch (this.integerMode) {
        case 0:
          if (this.smallValue == Int32.MinValue) {
            // would overflow, convert to large
            this.integerMode = 1;
            this.mnum =
            MutableNumber.FromEInteger(ValueNegativeInt32MinValue);
          } else {
            smallValue = -smallValue;
          }
          break;
        case 1:
          this.integerMode = 2;
          this.largeValue = this.mnum.ToEInteger();
          this.largeValue = -(EInteger)this.largeValue;
          break;
        case 2:
          this.largeValue = -(EInteger)this.largeValue;
          break;
        default:
          throw new InvalidOperationException();
      }
      return this;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.FastInteger.Subtract(PeterO.Numbers.FastInteger)"]/*'/>
    internal FastInteger Subtract(FastInteger val) {
      this.CheckFrozen();
      EInteger valValue;
      switch (this.integerMode) {
        case 0:
          if (val.integerMode == 0) {
            int vsv = val.smallValue;
            if ((vsv < 0 && Int32.MaxValue + vsv < this.smallValue) ||
                (vsv > 0 && Int32.MinValue + vsv > this.smallValue)) {
              // would overflow, convert to large
              this.integerMode = 2;
              this.largeValue = (EInteger)this.smallValue;
              this.largeValue -= (EInteger)vsv;
            } else {
              this.smallValue -= vsv;
            }
          } else {
            integerMode = 2;
            largeValue = (EInteger)smallValue;
            valValue = val.AsEInteger();
            largeValue -= (EInteger)valValue;
          }
          break;
        case 1:
          if (val.integerMode == 1) {
            // NOTE: Mutable numbers are
            // currently always zero or positive
            this.mnum.Subtract(val.mnum);
          } else if (val.integerMode == 0 && val.smallValue >= 0) {
            mnum.SubtractInt(val.smallValue);
          } else {
            integerMode = 2;
            largeValue = mnum.ToEInteger();
            valValue = val.AsEInteger();
            largeValue -= (EInteger)valValue;
          }
          break;
        case 2:
          valValue = val.AsEInteger();
          this.largeValue -= (EInteger)valValue;
          break;
        default: throw new InvalidOperationException();
      }
      return this;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.FastInteger.SubtractInt(System.Int32)"]/*'/>
    internal FastInteger SubtractInt(int val) {
      this.CheckFrozen();
      if (val == Int32.MinValue) {
        return this.AddBig(ValueNegativeInt32MinValue);
      }
      if (this.integerMode == 0) {
        if ((val < 0 && Int32.MaxValue + val < this.smallValue) ||
                (val > 0 && Int32.MinValue + val > this.smallValue)) {
          // would overflow, convert to large
          this.integerMode = 2;
          this.largeValue = (EInteger)this.smallValue;
          this.largeValue -= (EInteger)val;
        } else {
          this.smallValue -= val;
        }
        return this;
      }
      return this.AddInt(-val);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.FastInteger.AddBig(PeterO.Numbers.EInteger)"]/*'/>
    internal FastInteger AddBig(EInteger bigintVal) {
      this.CheckFrozen();
      switch (this.integerMode) {
          case 0: {
            return bigintVal.CanFitInInt32() ? this.AddInt((int)bigintVal) :
            this.Add(FastInteger.FromBig(bigintVal));
          }
        case 1:
          this.integerMode = 2;
          this.largeValue = this.mnum.ToEInteger();
          this.largeValue += bigintVal;
          break;
        case 2:
          this.largeValue += bigintVal;
          break;
        default:
          throw new InvalidOperationException();
      }
      return this;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.FastInteger.SubtractBig(PeterO.Numbers.EInteger)"]/*'/>
    internal FastInteger SubtractBig(EInteger bigintVal) {
      this.CheckFrozen();
      if (this.integerMode == 2) {
        this.largeValue -= (EInteger)bigintVal;
        return this;
      } else {
        int sign = bigintVal.Sign;
        if (sign == 0) {
          return this;
        }
        // Check if this value fits an int, except if
        // it's MinValue
        if (sign < 0 && bigintVal.CompareTo(ValueInt32MinValue) > 0) {
          return this.AddInt(-((int)bigintVal));
        }
        if (sign > 0 && bigintVal.CompareTo(ValueInt32MaxValue) <= 0) {
          return this.SubtractInt((int)bigintVal);
        }
        bigintVal = -bigintVal;
        return this.AddBig(bigintVal);
      }
    }

    internal FastInteger Add(FastInteger val) {
      this.CheckFrozen();
      EInteger valValue;
      switch (this.integerMode) {
        case 0:
          if (val.integerMode == 0) {
            if ((this.smallValue < 0 && (int)val.smallValue < Int32.MinValue
            - this.smallValue) ||
                (this.smallValue > 0 && (int)val.smallValue > Int32.MaxValue
                - this.smallValue)) {
              // would overflow
              if (val.smallValue >= 0) {
                this.integerMode = 1;
                this.mnum = new MutableNumber(this.smallValue);
                this.mnum.Add(val.smallValue);
              } else {
                this.integerMode = 2;
                this.largeValue = (EInteger)this.smallValue;
                this.largeValue += (EInteger)val.smallValue;
              }
            } else {
              this.smallValue += val.smallValue;
            }
          } else {
            integerMode = 2;
            largeValue = (EInteger)smallValue;
            valValue = val.AsEInteger();
            largeValue += (EInteger)valValue;
          }
          break;
        case 1:
          if (val.integerMode == 0 && val.smallValue >= 0) {
            this.mnum.Add(val.smallValue);
          } else {
            integerMode = 2;
            largeValue = mnum.ToEInteger();
            valValue = val.AsEInteger();
            largeValue += (EInteger)valValue;
          }
          break;
        case 2:
          valValue = val.AsEInteger();
          this.largeValue += (EInteger)valValue;
          break;
        default: throw new InvalidOperationException();
      }
      return this;
    }

    internal FastInteger Remainder(int divisor) {
      // Mod operator will always result in a
      // number that fits an int for int divisors
      this.CheckFrozen();
      if (divisor != 0) {
        switch (this.integerMode) {
          case 0:
            this.smallValue %= divisor;
            break;
          case 1:
            this.largeValue = this.mnum.ToEInteger();
            this.largeValue %= (EInteger)divisor;
            this.smallValue = (int)this.largeValue;
            this.integerMode = 0;
            break;
          case 2:
            this.largeValue %= (EInteger)divisor;
            this.smallValue = (int)this.largeValue;
            this.integerMode = 0;
            break;
          default:
            throw new InvalidOperationException();
        }
      } else {
        throw new DivideByZeroException();
      }
      return this;
    }

    internal FastInteger Increment() {
      this.CheckFrozen();
      if (this.integerMode == 0) {
        if (this.smallValue != Int32.MaxValue) {
          ++this.smallValue;
        } else {
          this.integerMode = 1;
          this.mnum = MutableNumber.FromEInteger(ValueNegativeInt32MinValue);
        }
        return this;
      }
      return this.AddInt(1);
    }

    internal FastInteger Decrement() {
      this.CheckFrozen();
      if (this.integerMode == 0) {
        if (this.smallValue != Int32.MinValue) {
          --this.smallValue;
        } else {
          this.integerMode = 1;
          this.mnum = MutableNumber.FromEInteger(ValueInt32MinValue);
          this.mnum.SubtractInt(1);
        }
        return this;
      }
      return this.SubtractInt(1);
    }

    internal FastInteger Divide(int divisor) {
      this.CheckFrozen();
      if (divisor != 0) {
        switch (this.integerMode) {
          case 0:
            if (divisor == -1 && this.smallValue == Int32.MinValue) {
              // would overflow, convert to large
              this.integerMode = 1;
              this.mnum =
              MutableNumber.FromEInteger(ValueNegativeInt32MinValue);
            } else {
              smallValue /= divisor;
            }
            break;
          case 1:
            this.integerMode = 2;
            this.largeValue = this.mnum.ToEInteger();
            this.largeValue /= (EInteger)divisor;
            if (this.largeValue.IsZero) {
              this.integerMode = 0;
              this.smallValue = 0;
            }
            break;
          case 2:
            this.largeValue /= (EInteger)divisor;
            if (this.largeValue.IsZero) {
              this.integerMode = 0;
              this.smallValue = 0;
            }
            break;
          default:
            throw new InvalidOperationException();
        }
      } else {
        throw new DivideByZeroException();
      }
      return this;
    }

    internal bool IsEvenNumber {
      get {
        switch (this.integerMode) {
          case 0:
            return (this.smallValue & 1) == 0;
          case 1:
            return this.mnum.IsEvenNumber;
          case 2:
            return this.largeValue.IsEven;
          default:
            throw new InvalidOperationException();
        }
      }
    }

    internal FastInteger AddInt(int val) {
      this.CheckFrozen();
     EInteger valValue;
      switch (this.integerMode) {
        case 0:
          if ((this.smallValue < 0 && (int)val < Int32.MinValue -
        this.smallValue) || (this.smallValue > 0 && (int)val >
            Int32.MaxValue - this.smallValue)) {
            // would overflow
            if (val >= 0) {
              this.integerMode = 1;
              this.mnum = new MutableNumber(this.smallValue);
              this.mnum.Add(val);
            } else {
              this.integerMode = 2;
              this.largeValue = (EInteger)this.smallValue;
              this.largeValue += (EInteger)val;
            }
          } else {
            smallValue += val;
          }
          break;
        case 1:
          if (val >= 0) {
            this.mnum.Add(val);
          } else {
            integerMode = 2;
            largeValue = mnum.ToEInteger();
            valValue = (EInteger)val;
            largeValue += (EInteger)valValue;
          }
          break;
        case 2:
          valValue = (EInteger)val;
          this.largeValue += (EInteger)valValue;
          break;
        default: throw new InvalidOperationException();
      }
      return this;
    }

    internal bool CanFitInInt32() {
      switch (this.integerMode) {
        case 0:
          return true;
        case 1:
          return this.mnum.CanFitInInt32();
          case 2: {
            return this.largeValue.CanFitInInt32();
          }
        default:
          throw new InvalidOperationException();
      }
    }

    internal bool CanFitInInt64() {
      switch (this.integerMode) {
        case 0:
          return true;
        case 1:
          return this.AsEInteger().CanFitInInt64();
          case 2: {
            return this.largeValue.CanFitInInt64();
          }
        default:
          throw new InvalidOperationException();
      }
    }

    internal long AsInt64() {
      switch (this.integerMode) {
        case 0:
          return (long)this.smallValue;
        case 1:
          return this.AsEInteger().ToInt64Unchecked();
          case 2: {
            return this.largeValue.ToInt64Unchecked();
          }
        default:
          throw new InvalidOperationException();
      }
    }

    private static readonly string HexAlphabet = "0123456789ABCDEF";

    internal static string IntToString(int value) {
      if (value == Int32.MinValue) {
        return "-2147483648";
      }
      if (value == 0) {
        return "0";
      }
      bool neg = value < 0;
      var chars = new char[12];
      var count = 11;
      if (neg) {
        value = -value;
      }
      while (value > 43698) {
        int intdivvalue = value / 10;
        char digit = HexAlphabet[(int)(value - (intdivvalue * 10))];
        chars[count--] = digit;
        value = intdivvalue;
      }
      while (value > 9) {
          int intdivvalue = (value * 26215) >> 18;
          char digit = HexAlphabet[(int)(value - (intdivvalue * 10))];
          chars[count--] = digit;
          value = intdivvalue;
      }
      if (value != 0) {
          chars[count--] = HexAlphabet[(int)value];
      }
      if (neg) {
        chars[count] = '-';
      } else {
        ++count;
      }
      return new String(chars, count, 12 - count);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.FastInteger.ToString"]/*'/>
    public override string ToString() {
      switch (this.integerMode) {
        case 0:
          return IntToString(this.smallValue);
        case 1:
          return this.mnum.ToEInteger().ToString();
        case 2:
          return this.largeValue.ToString();
        default: return String.Empty;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.FastInteger.Sign"]/*'/>
    internal int Sign {
      get {
        switch (this.integerMode) {
          case 0:
          return (this.smallValue == 0) ? 0 : ((this.smallValue < 0) ? -1 :
              1);
          case 1:
            return this.mnum.Sign;
          case 2:
            return this.largeValue.Sign;
          default: return 0;
        }
      }
    }

    internal bool IsValueZero {
      get {
        switch (this.integerMode) {
          case 0:
            return this.smallValue == 0;
          case 1:
            return this.mnum.Sign == 0;
          case 2:
            return this.largeValue.IsZero;
          default:
            return false;
        }
      }
    }

    internal int CompareToInt(int val) {
      switch (this.integerMode) {
        case 0:
          return (val == this.smallValue) ? 0 : (this.smallValue < val ? -1 :
          1);
        case 1:
          return this.mnum.ToEInteger().CompareTo((EInteger)val);
        case 2:
          return this.largeValue.CompareTo((EInteger)val);
        default: return 0;
      }
    }

    internal EInteger AsEInteger() {
      switch (this.integerMode) {
        case 0:
          return EInteger.FromInt32(this.smallValue);
        case 1:
          return this.mnum.ToEInteger();
        case 2:
          return this.largeValue;
        default: throw new InvalidOperationException();
      }
    }
  }
}
