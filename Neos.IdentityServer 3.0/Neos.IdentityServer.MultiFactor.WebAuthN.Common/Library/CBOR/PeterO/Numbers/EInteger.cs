/*
Written in 2013-2016 by Peter O.

Parts of the code were adapted by Peter O. from
public-domain code by Wei Dai.

Parts of the GCD function adapted by Peter O.
from public domain GCD code by Christian
Stigen Larsen (http://csl.sublevel3.org).

Any copyright is dedicated to the Public Domain.
http://creativecommons.org/publicdomain/zero/1.0/
If you like this, you should donate to Peter O.
at: http://peteroupc.github.io/
 */
using System;
using System.Text;

namespace PeterO.Numbers {
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:PeterO.Numbers.EInteger"]/*'/>
  public sealed partial class EInteger : IComparable<EInteger>,
    IEquatable<EInteger> {
    // TODO: Investigate using 32-bit words instead of 16-bit
    private const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private const int RecursiveDivisionLimit = 40;

    private const int RecursionLimit = 10;

    private const int ShortMask = 0xffff;

    private static readonly int[] ValueCharToDigit = { 36, 36, 36, 36, 36, 36,
      36,
      36,
      36, 36, 36, 36, 36, 36, 36, 36,
      36, 36, 36, 36, 36, 36, 36, 36,
      36, 36, 36, 36, 36, 36, 36, 36,
      36, 36, 36, 36, 36, 36, 36, 36,
      36, 36, 36, 36, 36, 36, 36, 36,
      0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 36, 36, 36, 36, 36, 36,
      36, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
      25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 36, 36, 36, 36,
      36, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
      25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 36, 36, 36, 36 };

    private static readonly int[] ValueMaxSafeInts = { 1073741823, 715827881,
      536870911, 429496728, 357913940, 306783377, 268435455, 238609293,
      214748363, 195225785, 178956969, 165191048, 153391688, 143165575,
      134217727, 126322566, 119304646, 113025454, 107374181, 102261125,
      97612892, 93368853, 89478484, 85899344, 82595523, 79536430, 76695843,
      74051159, 71582787, 69273665, 67108863, 65075261, 63161282, 61356674,
      59652322 };

    private static readonly EInteger ValueOne = new EInteger(
      1, new short[] { 1 }, false);

    private static readonly EInteger ValueTen = new EInteger(
      1, new short[] { 10 }, false);

    private static readonly EInteger ValueZero = new EInteger(
      0, new short[] { 0 }, false);

    private readonly bool negative;
    private readonly int wordCount;
    private readonly short[] words;

    private EInteger(int wordCount, short[] reg, bool negative) {
#if DEBUG
      if (wordCount > 0) {
        if (reg == null) {
          throw new InvalidOperationException();
        }
        if (wordCount > reg.Length) {
          throw new InvalidOperationException();
        }
        if (reg[wordCount - 1] == 0) {
          throw new InvalidOperationException();
        }
      }
#endif
      this.wordCount = wordCount;
      this.words = reg;
      this.negative = negative;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.EInteger.One"]/*'/>
    public static EInteger One {
      get {
        return ValueOne;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.EInteger.Ten"]/*'/>
    public static EInteger Ten {
      get {
        return ValueTen;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.EInteger.Zero"]/*'/>
    public static EInteger Zero {
      get {
        return ValueZero;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.EInteger.IsEven"]/*'/>
    public bool IsEven {
      get {
        return !this.GetUnsignedBit(0);
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.EInteger.IsPowerOfTwo"]/*'/>
    public bool IsPowerOfTwo {
      get {
        if (this.negative) {
          return false;
        }
        return (this.wordCount == 0) ? false : (this.GetUnsignedBitLength()
          - 1 == this.GetLowBit());
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.EInteger.IsZero"]/*'/>
    public bool IsZero {
      get {
        return this.wordCount == 0;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.EInteger.Sign"]/*'/>
    public int Sign {
      get {
        return (this.wordCount == 0) ? 0 : (this.negative ? -1 : 1);
      }
    }

    internal static EInteger FromInts(int[] intWords, int count) {
      var words = new short[count << 1];
      var j = 0;
      for (var i = 0; i < count; ++i, j += 2) {
        int w = intWords[i];
        words[j] = unchecked((short)w);
        words[j + 1] = unchecked((short)(w >> 16));
      }
      int newwordCount = words.Length;
      while (newwordCount != 0 && words[newwordCount - 1] == 0) {
        --newwordCount;
      }
      return (newwordCount == 0) ? EInteger.Zero : (new
                    EInteger(
                    newwordCount,
                    words,
                    false));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.FromBytes(System.Byte[],System.Boolean)"]/*'/>
    public static EInteger FromBytes(byte[] bytes, bool littleEndian) {
      if (bytes == null) {
        throw new ArgumentNullException(nameof(bytes));
      }
      if (bytes.Length == 0) {
        return EInteger.Zero;
      }
      int len = bytes.Length;
      int wordLength = ((int)len + 1) >> 1;
      var newreg = new short[wordLength];
      int valueJIndex = littleEndian ? len - 1 : 0;
      bool numIsNegative = (bytes[valueJIndex] & 0x80) != 0;
      bool newnegative = numIsNegative;
      var j = 0;
      if (!numIsNegative) {
        if (littleEndian) {
         bool odd = (len & 1) != 0;
         if (odd) {
           --len;
         }
         for (var i = 0; i < len; i += 2, j++) {
          int index2 = i + 1;
          int nrj = ((int)bytes[i]) & 0xff;
          nrj |= ((int)bytes[index2]) << 8;
          newreg[j] = unchecked((short)nrj);
         }
         if (odd) {
           newreg[len >> 1] = unchecked((short)(((int)bytes[len]) & 0xff));
         }
        } else {
        for (var i = 0; i < len; i += 2, j++) {
          int index = len - 1 - i;
          int index2 = len - 2 - i;
          int nrj = ((int)bytes[index]) & 0xff;
          if (index2 >= 0 && index2 < len) {
            nrj |= ((int)bytes[index2]) << 8;
          }
          newreg[j] = unchecked((short)nrj);
        }
        }
      } else {
        for (var i = 0; i < len; i += 2, j++) {
          int index = littleEndian ? i : len - 1 - i;
          int index2 = littleEndian ? i + 1 : len - 2 - i;
          int nrj = ((int)bytes[index]) & 0xff;
          if (index2 >= 0 && index2 < len) {
            nrj |= ((int)bytes[index2]) << 8;
          } else {
            // sign extend the last byte
            nrj |= 0xff00;
          }
          newreg[j] = unchecked((short)nrj);
        }
        for (; j < newreg.Length; ++j) {
          newreg[j] = unchecked((short)0xffff);  // sign extend remaining words
        }
        TwosComplement(newreg, 0, (int)newreg.Length);
      }
      int newwordCount = newreg.Length;
      while (newwordCount != 0 && newreg[newwordCount - 1] == 0) {
        --newwordCount;
      }
      return (newwordCount == 0) ? EInteger.Zero : (new
                    EInteger(
                    newwordCount,
                    newreg,
                    newnegative));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.FromInt32(System.Int32)"]/*'/>
    public static EInteger FromInt32(int intValue) {
      if (intValue == 0) {
        return ValueZero;
      }
      if (intValue == 1) {
        return ValueOne;
      }
      if (intValue == 10) {
        return ValueTen;
      }
      short[] retreg;
      bool retnegative;
      int retwordcount;
      retnegative = intValue < 0;
      if ((intValue >> 15) == 0) {
        retreg = new short[2];
        if (retnegative) {
          intValue = -intValue;
        }
        retreg[0] = (short)(intValue & 0xffff);
        retwordcount = 1;
      } else if (intValue == Int32.MinValue) {
        retreg = new short[2];
        retreg[0] = 0;
        retreg[1] = unchecked((short)0x8000);
        retwordcount = 2;
      } else {
        unchecked {
          retreg = new short[2];
          if (retnegative) {
            intValue = -intValue;
          }
          retreg[0] = (short)(intValue & 0xffff);
          intValue >>= 16;
          retreg[1] = (short)(intValue & 0xffff);
          retwordcount = (retreg[1] == 0) ? 1 : 2;
        }
      }
      return new EInteger(retwordcount, retreg, retnegative);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.FromInt64(System.Int64)"]/*'/>
    public static EInteger FromInt64(long longerValue) {
      if (longerValue == 0) {
        return ValueZero;
      }
      if (longerValue == 1) {
        return ValueOne;
      }
      if (longerValue == 10) {
        return ValueTen;
      }
      short[] retreg;
      bool retnegative;
      int retwordcount;
      unchecked {
        retnegative = longerValue < 0;
        if ((longerValue >> 16) == 0) {
          retreg = new short[1];
          var intValue = (int)longerValue;
          if (retnegative) {
            intValue = -intValue;
          }
          retreg[0] = (short)(intValue & 0xffff);
          retwordcount = 1;
        } else if ((longerValue >> 31) == 0) {
          retreg = new short[2];
          var intValue = (int)longerValue;
          if (retnegative) {
            intValue = -intValue;
          }
          retreg[0] = (short)(intValue & 0xffff);
          retreg[1] = (short)((intValue >> 16) & 0xffff);
          retwordcount = 2;
        } else if (longerValue == Int64.MinValue) {
          retreg = new short[4];
          retreg[0] = 0;
          retreg[1] = 0;
          retreg[2] = 0;
          retreg[3] = unchecked((short)0x8000);
          retwordcount = 4;
        } else {
          retreg = new short[4];
          long ut = longerValue;
          if (retnegative) {
            ut = -ut;
          }
          retreg[0] = (short)(ut & 0xffff);
          ut >>= 16;
          retreg[1] = (short)(ut & 0xffff);
          ut >>= 16;
          retreg[2] = (short)(ut & 0xffff);
          ut >>= 16;
          retreg[3] = (short)(ut & 0xffff);
          // at this point, the word count can't
          // be 0 (the check for 0 was already done above)
          retwordcount = 4;
          while (retwordcount != 0 &&
                 retreg[retwordcount - 1] == 0) {
            --retwordcount;
          }
        }
      }
      return new EInteger(retwordcount, retreg, retnegative);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.FromRadixString(System.String,System.Int32)"]/*'/>
    public static EInteger FromRadixString(string str, int radix) {
      if (str == null) {
        throw new ArgumentNullException(nameof(str));
      }
      return FromRadixSubstring(str, radix, 0, str.Length);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.FromRadixSubstring(System.String,System.Int32,System.Int32,System.Int32)"]/*'/>
    public static EInteger FromRadixSubstring(
      string str,
      int radix,
      int index,
      int endIndex) {
      if (str == null) {
        throw new ArgumentNullException(nameof(str));
      }
      if (radix < 2) {
        throw new ArgumentException("radix (" + radix +
                    ") is less than 2");
      }
      if (radix > 36) {
        throw new ArgumentException("radix (" + radix +
                    ") is more than 36");
      }
      if (index < 0) {
        throw new ArgumentException("index (" + index + ") is less than " +
                    "0");
      }
      if (index > str.Length) {
        throw new ArgumentException("index (" + index + ") is more than " +
                    str.Length);
      }
      if (endIndex < 0) {
        throw new ArgumentException("endIndex (" + endIndex +
                    ") is less than 0");
      }
      if (endIndex > str.Length) {
        throw new ArgumentException("endIndex (" + endIndex +
                    ") is more than " + str.Length);
      }
      if (endIndex < index) {
        throw new ArgumentException("endIndex (" + endIndex +
                    ") is less than " + index);
      }
      if (index == endIndex) {
        throw new FormatException("No digits");
      }
      var negative = false;
      if (str[index] == '-') {
        ++index;
        if (index == endIndex) {
          throw new FormatException("No digits");
        }
        negative = true;
      }
      // Skip leading zeros
      for (; index < endIndex; ++index) {
        char c = str[index];
        if (c != 0x30) {
          break;
        }
      }
      int effectiveLength = endIndex - index;
      if (effectiveLength == 0) {
        return EInteger.Zero;
      }
      short[] bigint;
      if (radix == 16) {
        // Special case for hexadecimal radix
        int leftover = effectiveLength & 3;
        int wordCount = effectiveLength >> 2;
        if (leftover != 0) {
          ++wordCount;
        }
        bigint = new short[wordCount];
        int currentDigit = wordCount - 1;
        // Get most significant digits if effective
        // length is not divisible by 4
        if (leftover != 0) {
          var extraWord = 0;
          for (int i = 0; i < leftover; ++i) {
            extraWord <<= 4;
            char c = str[index + i];
            int digit = (c >= 0x80) ? 36 : ValueCharToDigit[(int)c];
            if (digit >= 16) {
              throw new FormatException("Illegal character found");
            }
            extraWord |= digit;
          }
          bigint[currentDigit] = unchecked((short)extraWord);
          --currentDigit;
          index += leftover;
        }
#if DEBUG
        if ((endIndex - index) % 4 != 0) {
          throw new ArgumentException(
            "doesn't satisfy (endIndex - index) % 4 == 0");
        }
#endif
        while (index < endIndex) {
          char c = str[index + 3];
          int digit = (c >= 0x80) ? 36 : ValueCharToDigit[(int)c];
          if (digit >= 16) {
            throw new FormatException("Illegal character found");
          }
          int word = digit;
          c = str[index + 2];
          digit = (c >= 0x80) ? 36 : ValueCharToDigit[(int)c];
          if (digit >= 16) {
            throw new FormatException("Illegal character found");
          }

          word |= digit << 4;
          c = str[index + 1];
          digit = (c >= 0x80) ? 36 : ValueCharToDigit[(int)c];
          if (digit >= 16) {
            throw new FormatException("Illegal character found");
          }

          word |= digit << 8;
          c = str[index];
          digit = (c >= 0x80) ? 36 : ValueCharToDigit[(int)c];
          if (digit >= 16) {
            throw new FormatException("Illegal character found");
          }
          word |= digit << 12;
          index += 4;
          bigint[currentDigit] = unchecked((short)word);
          --currentDigit;
        }
      } else if (radix == 2) {
        // Special case for binary radix
        int leftover = effectiveLength & 15;
        int wordCount = effectiveLength >> 4;
        if (leftover != 0) {
          ++wordCount;
        }
        bigint = new short[wordCount];
        int currentDigit = wordCount - 1;
        // Get most significant digits if effective
        // length is not divisible by 4
        if (leftover != 0) {
          var extraWord = 0;
          for (int i = 0; i < leftover; ++i) {
            extraWord <<= 1;
            char c = str[index + i];
            int digit = (c == '0') ? 0 : ((c == '1') ? 1 : 2);
            if (digit >= 2) {
              throw new FormatException("Illegal character found");
            }
            extraWord |= digit;
          }
          bigint[currentDigit] = unchecked((short)extraWord);
          --currentDigit;
          index += leftover;
        }
        while (index < endIndex) {
          var word = 0;
          int idx = index + 15;
          for (var i = 0; i < 16; ++i) {
            char c = str[idx];
            int digit = (c == '0') ? 0 : ((c == '1') ? 1 : 2);
            if (digit >= 2) {
              throw new FormatException("Illegal character found");
            }
            --idx;
            word |= digit << i;
          }
          index += 16;
          bigint[currentDigit] = unchecked((short)word);
          --currentDigit;
        }
      } else {
        bigint = new short[4];
        var haveSmallInt = true;
        int maxSafeInt = ValueMaxSafeInts[radix - 2];
        int maxShortPlusOneMinusRadix = 65536 - radix;
        var smallInt = 0;
        for (int i = index; i < endIndex; ++i) {
          char c = str[i];
          int digit = (c >= 0x80) ? 36 : ValueCharToDigit[(int)c];
          if (digit >= radix) {
            throw new FormatException("Illegal character found");
          }
          if (haveSmallInt && smallInt < maxSafeInt) {
            smallInt *= radix;
            smallInt += digit;
          } else {
            if (haveSmallInt) {
              bigint[0] = unchecked((short)(smallInt & 0xffff));
              bigint[1] = unchecked((short)((smallInt >> 16) & 0xffff));
              haveSmallInt = false;
            }
            // Multiply by the radix
            short carry = 0;
            int n = bigint.Length;
            for (int j = 0; j < n; ++j) {
              int p;
              p = unchecked((((int)bigint[j]) & 0xffff) * radix);
              int p2 = ((int)carry) & 0xffff;
              p = unchecked(p + p2);
              bigint[j] = unchecked((short)p);
              carry = unchecked((short)(p >> 16));
            }
            if (carry != 0) {
              bigint = GrowForCarry(bigint, carry);
            }
            // Add the parsed digit
            if (digit != 0) {
              int d = bigint[0] & 0xffff;
              if (d <= maxShortPlusOneMinusRadix) {
                bigint[0] = unchecked((short)(d + digit));
              } else if (Increment(bigint, 0, bigint.Length, (short)digit) !=
                    0) {
                bigint = GrowForCarry(bigint, (short)1);
              }
            }
          }
        }
        if (haveSmallInt) {
          bigint[0] = unchecked((short)(smallInt & 0xffff));
          bigint[1] = unchecked((short)((smallInt >> 16) & 0xffff));
        }
      }
      int count = CountWords(bigint);
      return (count == 0) ? EInteger.Zero : new EInteger(
        count,
        bigint,
        negative);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.FromString(System.String)"]/*'/>
    public static EInteger FromString(string str) {
      if (str == null) {
        throw new ArgumentNullException(nameof(str));
      }
      return FromRadixSubstring(str, 10, 0, str.Length);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.FromSubstring(System.String,System.Int32,System.Int32)"]/*'/>
    public static EInteger FromSubstring(
      string str,
      int index,
      int endIndex) {
      if (str == null) {
        throw new ArgumentNullException(nameof(str));
      }
      return FromRadixSubstring(str, 10, index, endIndex);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Abs"]/*'/>
    public EInteger Abs() {
      return (this.wordCount == 0 || !this.negative) ? this : new
        EInteger(this.wordCount, this.words, false);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Add(PeterO.Numbers.EInteger)"]/*'/>
    public EInteger Add(EInteger bigintAugend) {
      if (bigintAugend == null) {
        throw new ArgumentNullException(nameof(bigintAugend));
      }
      if (this.wordCount == 0) {
        return bigintAugend;
      }
      if (bigintAugend.wordCount == 0) {
        return this;
      }
      short[] sumreg;
      if (bigintAugend.wordCount == 1 && this.wordCount == 1) {
        if (this.negative == bigintAugend.negative) {
          int intSum = (((int)this.words[0]) & 0xffff) +
            (((int)bigintAugend.words[0]) & 0xffff);
          sumreg = new short[2];
          sumreg[0] = unchecked((short)intSum);
          sumreg[1] = unchecked((short)(intSum >> 16));
          return new EInteger(
            ((intSum >> 16) == 0) ? 1 : 2,
            sumreg,
            this.negative);
        } else {
          int a = ((int)this.words[0]) & 0xffff;
          int b = ((int)bigintAugend.words[0]) & 0xffff;
          if (a == b) {
            return EInteger.Zero;
          }
          if (a > b) {
            a -= b;
            sumreg = new short[2];
            sumreg[0] = unchecked((short)a);
            return new EInteger(1, sumreg, this.negative);
          }
          b -= a;
          sumreg = new short[2];
          sumreg[0] = unchecked((short)b);
          return new EInteger(1, sumreg, !this.negative);
        }
      }
      if ((!this.negative) == (!bigintAugend.negative)) {
        // both nonnegative or both negative
        int addendCount = this.wordCount;
        int augendCount = bigintAugend.wordCount;
        if (augendCount <= 2 && addendCount <= 2 &&
           (this.wordCount < 2 || (this.words[1] >> 15) == 0) &&
           (bigintAugend.wordCount < 2 || (bigintAugend.words[1] >> 15) == 0)) {
          int a = ((int)this.words[0]) & 0xffff;
          if (this.wordCount == 2) {
            a |= (((int)this.words[1]) & 0xffff) << 16;
          }
          int b = ((int)bigintAugend.words[0]) & 0xffff;
          if (bigintAugend.wordCount == 2) {
            b |= (((int)bigintAugend.words[1]) & 0xffff) << 16;
          }
          a = unchecked((int)(a + b));
          sumreg = new short[2];
          sumreg[0] = unchecked((short)(a & 0xffff));
          sumreg[1] = unchecked((short)((a >> 16) & 0xffff));
          int wcount = (sumreg[1] == 0) ? 1 : 2;
          return new EInteger(wcount, sumreg, this.negative);
        }
        if (augendCount <= 2 && addendCount <= 2) {
          int a = ((int)this.words[0]) & 0xffff;
          if (this.wordCount == 2) {
            a |= (((int)this.words[1]) & 0xffff) << 16;
          }
          int b = ((int)bigintAugend.words[0]) & 0xffff;
          if (bigintAugend.wordCount == 2) {
            b |= (((int)bigintAugend.words[1]) & 0xffff) << 16;
          }
          long longResult = ((long)a) & 0xffffffffL;
          longResult += ((long)b) & 0xffffffffL;
          if ((longResult >> 32) == 0) {
            a = unchecked((int)longResult);
            sumreg = new short[2];
            sumreg[0] = unchecked((short)(a & 0xffff));
            sumreg[1] = unchecked((short)((a >> 16) & 0xffff));
            int wcount = (sumreg[1] == 0) ? 1 : 2;
            return new EInteger(wcount, sumreg, this.negative);
          }
        }
        // DebugUtility.Log("" + this + " + " + bigintAugend);
        sumreg = new short[(
          int)Math.Max(
                    this.words.Length,
                    bigintAugend.words.Length)];
        int carry;
        int desiredLength = Math.Max(addendCount, augendCount);
        if (addendCount == augendCount) {
          carry = AddInternal(
            sumreg,
            0,
            this.words,
            0,
            bigintAugend.words,
            0,
            addendCount);
        } else if (addendCount > augendCount) {
          // Addend is bigger
          carry = AddInternal(
            sumreg,
            0,
            this.words,
            0,
            bigintAugend.words,
            0,
            augendCount);
          Array.Copy(
            this.words,
            augendCount,
            sumreg,
            augendCount,
            addendCount - augendCount);
          if (carry != 0) {
            carry = Increment(
              sumreg,
              augendCount,
              addendCount - augendCount,
              (short)carry);
          }
        } else {
          // Augend is bigger
          carry = AddInternal(
            sumreg,
            0,
            this.words,
            0,
            bigintAugend.words,
            0,
            (int)addendCount);
          Array.Copy(
            bigintAugend.words,
            addendCount,
            sumreg,
            addendCount,
            augendCount - addendCount);
          if (carry != 0) {
            carry = Increment(
              sumreg,
              addendCount,
              (int)(augendCount - addendCount),
              (short)carry);
          }
        }
        var needShorten = true;
        if (carry != 0) {
          int nextIndex = desiredLength;
          int len = nextIndex + 1;
          sumreg = CleanGrow(sumreg, len);
          sumreg[nextIndex] = (short)carry;
          needShorten = false;
        }
        int sumwordCount = CountWords(sumreg);
        if (sumwordCount == 0) {
          return EInteger.Zero;
        }
        if (needShorten) {
          sumreg = ShortenArray(sumreg, sumwordCount);
        }
        return new EInteger(sumwordCount, sumreg, this.negative);
      }
      EInteger minuend = this;
      EInteger subtrahend = bigintAugend;
      if (this.negative) {
        // this is negative, b is nonnegative
        minuend = bigintAugend;
        subtrahend = this;
      }
      // Do a subtraction
      int words1Size = minuend.wordCount;
      int words2Size = subtrahend.wordCount;
      var diffNeg = false;
#if DEBUG
      if (words1Size > minuend.words.Length) {
        throw new InvalidOperationException();
      }
      if (words2Size > subtrahend.words.Length) {
        throw new InvalidOperationException();
      }
#endif
      short borrow;
      var diffReg = new short[(
        int)Math.Max(
                    minuend.words.Length,
                    subtrahend.words.Length)];
      if (words1Size == words2Size) {
        if (Compare(minuend.words, 0, subtrahend.words, 0, (int)words1Size) >=
            0) {
          // words1 is at least as high as words2
          SubtractInternal(
            diffReg,
            0,
            minuend.words,
            0,
            subtrahend.words,
            0,
            words1Size);
        } else {
          // words1 is less than words2
          SubtractInternal(
            diffReg,
            0,
            subtrahend.words,
            0,
            minuend.words,
            0,
            words1Size);
          diffNeg = true;  // difference will be negative
        }
      } else if (words1Size > words2Size) {
        // words1 is greater than words2
        borrow = (
          short)SubtractInternal(
          diffReg,
          0,
          minuend.words,
          0,
          subtrahend.words,
          0,
          words2Size);
        Array.Copy(
          minuend.words,
          words2Size,
          diffReg,
          words2Size,
          words1Size - words2Size);
        Decrement(diffReg, words2Size, (int)(words1Size - words2Size), borrow);
      } else {
        // words1 is less than words2
        borrow = (
          short)SubtractInternal(
          diffReg,
          0,
          subtrahend.words,
          0,
          minuend.words,
          0,
          words1Size);
        Array.Copy(
          subtrahend.words,
          words1Size,
          diffReg,
          words1Size,
          words2Size - words1Size);
        Decrement(diffReg, words1Size, (int)(words2Size - words1Size), borrow);
        diffNeg = true;
      }
      int count = CountWords(diffReg);
      if (count == 0) {
        return EInteger.Zero;
      }
      diffReg = ShortenArray(diffReg, count);
      return new EInteger(count, diffReg, diffNeg);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.AsInt32Checked"]/*'/>
    [Obsolete("Renamed to ToInt32Checked.")]
    public int AsInt32Checked() {
      return this.ToInt32Checked();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.AsInt32Unchecked"]/*'/>
    [Obsolete("Renamed to ToInt32Unchecked.")]
    public int AsInt32Unchecked() {
      return this.ToInt32Unchecked();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.AsInt64Checked"]/*'/>
    [Obsolete("Renamed to ToInt64Checked.")]
    public long AsInt64Checked() {
      return this.ToInt64Checked();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.AsInt64Unchecked"]/*'/>
    [Obsolete("Renamed to ToInt64Unchecked.")]
    public long AsInt64Unchecked() {
      return this.ToInt64Unchecked();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.CanFitInInt32"]/*'/>
    public bool CanFitInInt32() {
      int c = this.wordCount;
      if (c > 2) {
        return false;
      }
      if (c == 2 && (this.words[1] & 0x8000) != 0) {
        return this.negative && this.words[1] == unchecked((short)0x8000) &&
          this.words[0] == 0;
      }
      return true;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.CanFitInInt64"]/*'/>
    public bool CanFitInInt64() {
      int c = this.wordCount;
      if (c > 4) {
        return false;
      }
      if (c == 4 && (this.words[3] & 0x8000) != 0) {
        return this.negative && this.words[3] == unchecked((short)0x8000) &&
          this.words[0] == 0;
      }
      return true;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.CompareTo(PeterO.Numbers.EInteger)"]/*'/>
    public int CompareTo(EInteger other) {
      if (other == null) {
        return 1;
      }
      if (this == other) {
        return 0;
      }
      int size = this.wordCount, tempSize = other.wordCount;
      int sa = size == 0 ? 0 : (this.negative ? -1 : 1);
      int sb = tempSize == 0 ? 0 : (other.negative ? -1 : 1);
      if (sa != sb) {
        return (sa < sb) ? -1 : 1;
      }
      if (sa == 0) {
        return 0;
      }
      if (size == tempSize) {
        if (size == 1 && this.words[0] == other.words[0]) {
          return 0;
        } else {
          short[] words1 = this.words;
          short[] words2 = other.words;
          while (unchecked(size--) != 0) {
            int an = ((int)words1[size]) & 0xffff;
            int bn = ((int)words2[size]) & 0xffff;
            if (an > bn) {
              return (sa > 0) ? 1 : -1;
            }
            if (an < bn) {
              return (sa > 0) ? -1 : 1;
            }
          }
          return 0;
        }
      }
      return ((size > tempSize) ^ (sa <= 0)) ? 1 : -1;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Add(System.Int32)"]/*'/>
public EInteger Add(int intValue) {
 if (intValue == 0) {
 return this;
}
 if (this.wordCount == 0) {
 return EInteger.FromInt32(intValue);
}
 if (this.wordCount == 1 && intValue < 65535 && intValue >= -65535) {
        short[] sumreg;
        if (intValue > 0 && !this.negative) {
          int intSum = (((int)this.words[0]) & 0xffff) + intValue;
          sumreg = new short[2];
          sumreg[0] = unchecked((short)intSum);
          sumreg[1] = unchecked((short)(intSum >> 16));
          return new EInteger(
            ((intSum >> 16) == 0) ? 1 : 2,
            sumreg,
            this.negative);
        } else if (intValue < 0 && this.negative) {
          int intSum = (((int)this.words[0]) & 0xffff) - intValue;
          sumreg = new short[2];
          sumreg[0] = unchecked((short)intSum);
          sumreg[1] = unchecked((short)(intSum >> 16));
          return new EInteger(
            ((intSum >> 16) == 0) ? 1 : 2,
            sumreg,
            this.negative);
        } else {
          int a = ((int)this.words[0]) & 0xffff;
          int b = Math.Abs(intValue);
          if (a > b) {
            a -= b;
            sumreg = new short[2];
            sumreg[0] = unchecked((short)a);
            return new EInteger(1, sumreg, this.negative);
          }else if (a == b) {
            return EInteger.Zero;
          } else {
          b -= a;
          sumreg = new short[2];
          sumreg[0] = unchecked((short)b);
          return new EInteger(1, sumreg, !this.negative);
          }
        }
 }
 return this.Add(EInteger.FromInt32(intValue));
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Subtract(System.Int32)"]/*'/>
public EInteger Subtract(int intValue) {
 return (intValue == Int32.MinValue) ?
   this.Subtract(EInteger.FromInt32(intValue)) : ((intValue == 0) ? this :
     this.Add(-intValue));
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Multiply(System.Int32)"]/*'/>
public EInteger Multiply(int intValue) {
 return this.Multiply(EInteger.FromInt32(intValue));
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Divide(System.Int32)"]/*'/>
public EInteger Divide(int intValue) {
 return this.Divide(EInteger.FromInt32(intValue));
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Remainder(System.Int32)"]/*'/>
public EInteger Remainder(int intValue) {
 return this.Remainder(EInteger.FromInt32(intValue));
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.CompareTo(System.Int32)"]/*'/>
public int CompareTo(int intValue) {
 return this.CompareTo(EInteger.FromInt32(intValue));
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Divide(PeterO.Numbers.EInteger)"]/*'/>
    public EInteger Divide(EInteger bigintDivisor) {
      if (bigintDivisor == null) {
        throw new ArgumentNullException(nameof(bigintDivisor));
      }
      int words1Size = this.wordCount;
      int words2Size = bigintDivisor.wordCount;
      // ---- Special cases
      if (words2Size == 0) {
        // Divisor is 0
        throw new DivideByZeroException();
      }
      if (words1Size < words2Size) {
        // Dividend is less than divisor (includes case
        // where dividend is 0)
        return EInteger.Zero;
      }
      if (words1Size <= 2 && words2Size <= 2 && this.CanFitInInt32() &&
          bigintDivisor.CanFitInInt32()) {
        int valueASmall = this.ToInt32Checked();
        int valueBSmall = bigintDivisor.ToInt32Checked();
        if (valueASmall != Int32.MinValue || valueBSmall != -1) {
          int result = valueASmall / valueBSmall;
          return EInteger.FromInt32(result);
        }
      }
      if (words1Size <= 4 && words2Size <= 4 && this.CanFitInInt64() &&
          bigintDivisor.CanFitInInt64()) {
        long valueALong = this.ToInt64Checked();
        long valueBLong = bigintDivisor.ToInt64Checked();
        if (valueALong != Int64.MinValue || valueBLong != -1) {
          long resultLong = valueALong / valueBLong;
          return EInteger.FromInt64(resultLong);
        }
      }
      short[] quotReg;
      int quotwordCount;
      if (words2Size == 1) {
        // divisor is small, use a fast path
        quotReg = new short[this.words.Length];
        quotwordCount = this.wordCount;
        FastDivide(quotReg, this.words, words1Size, bigintDivisor.words[0]);
        while (quotwordCount != 0 && quotReg[quotwordCount - 1] == 0) {
          --quotwordCount;
        }
        return (
          quotwordCount != 0) ? (
          new EInteger(
            quotwordCount,
            quotReg,
            this.negative ^ bigintDivisor.negative)) : EInteger.Zero;
      }
      // ---- General case
      quotReg = new short[((int)(words1Size - words2Size + 1))];
      GeneralDivide(
  this.words,
  0,
  this.wordCount,
  bigintDivisor.words,
  0,
  bigintDivisor.wordCount,
  quotReg,
  0,
  null,
  0);
      quotwordCount = CountWords(quotReg);
      quotReg = ShortenArray(quotReg, quotwordCount);
      return (
        quotwordCount != 0) ? (
        new EInteger(
          quotwordCount,
          quotReg,
          this.negative ^ bigintDivisor.negative)) : EInteger.Zero;
    }

    private static short LinearMultiplySubtractMinuend1Bigger(
      short[] resultArr,
      int resultStart,
      short[] minuendArr,
      int minuendArrStart,
      int factor1,
      short[] factor2,
      int factor2Start,
      int factor2Count) {
      if (factor2Count <= 0 || (factor1 >> 16) != 0) {
        throw new ArgumentException();
      }
      var a = 0;
      var b = 0;
      var cc = 0;
        for (var i = 0; i < factor2Count; ++i) {
          a = unchecked((((int)factor2[factor2Start + i]) & 0xffff) * factor1);
          a = unchecked(a + (cc & 0xffff));
          b = ((int)minuendArr[minuendArrStart + i] & 0xffff) - (a & 0xffff);
          resultArr[resultStart + i] = unchecked((short)b);
          cc = ((a >> 16) & 0xffff) + ((b >> 31) & 1);
        }
      a = cc & 0xffff;
      b = ((int)minuendArr[minuendArrStart + factor2Count] & 0xffff) - a;
      resultArr[resultStart + factor2Count] = unchecked((short)b);
      cc = (b >> 31) & 1;
      return unchecked((short)cc);
    }

    private static void DivideThreeBlocksByTwo(
      short[] valueALow,
      int posALow,
      short[] valueAMidHigh,
      int posAMidHigh,
      short[] b,
      int posB,
      int blockCount,
      short[] quot,
      int posQuot,
      short[] rem,
      int posRem,
      short[] tmp) {
      // NOTE: size of 'quot' equals 'blockCount' * 2
      // NOTE: size of 'rem' equals 'blockCount' * 2
#if DEBUG
      if (quot != null) {
if (posQuot < 0) {
  throw new ArgumentException("posQuot (" + posQuot +
    ") is less than 0");
}
if (posQuot > quot.Length) {
  throw new ArgumentException("posQuot (" + posQuot +
    ") is more than " + quot.Length);
}
if ((blockCount * 2) < 0) {
  throw new ArgumentException("blockCount*2 (" + (blockCount * 2) +
    ") is less than 0");
}
if ((blockCount * 2) > quot.Length) {
  throw new ArgumentException("blockCount*2 (" + (blockCount * 2) +
    ") is more than " + quot.Length);
}
if (quot.Length - posQuot < blockCount * 2) {
  throw new ArgumentException("quot's length minus " + posQuot + " (" +
    (quot.Length - posQuot) + ") is less than " + (blockCount * 2));
}
        }
        if (rem != null) {
if (posRem < 0) {
  throw new ArgumentException("posRem (" + posRem +
    ") is less than 0");
}
if (posRem > rem.Length) {
  throw new ArgumentException("posRem (" + posRem +
    ") is more than " + rem.Length);
}
if ((blockCount * 2) < 0) {
  throw new ArgumentException("blockCount*2 (" + (blockCount * 2) +
    ") is less than 0");
}
if ((blockCount * 2) > rem.Length) {
  throw new ArgumentException("blockCount*2 (" + (blockCount * 2) +
    ") is more than " + rem.Length);
}
if (rem.Length - posRem < blockCount * 2) {
  throw new ArgumentException("rem's length minus " + posRem + " (" +
    (rem.Length - posRem) + ") is less than " + (blockCount * 2));
}
        }
if (tmp.Length < blockCount * 6) {
  throw new ArgumentException("tmp.Length (" + tmp.Length +
    ") is less than " + (blockCount * 6));
}
#endif
      // Implements Algorithm 2 of Burnikel & Ziegler 1998
      int c;
      // If AHigh is less than BHigh
      if (
  WordsCompare(
  valueAMidHigh,
  posAMidHigh + blockCount,
  blockCount,
  b,
  posB + blockCount,
  blockCount) < 0) {
        // Divide AMidHigh by BHigh
        RecursiveDivideInner(
 valueAMidHigh,
 posAMidHigh,
 b,
 posB + blockCount,
 quot,
 posQuot,
 rem,
 posRem,
 blockCount);
        // Copy remainder to temp at block position 4
        Array.Copy(rem, posRem, tmp, blockCount * 4, blockCount);
        Array.Clear(tmp, blockCount * 5, blockCount);
      } else {
        // BHigh is less than AHigh
        // set quotient to all ones
        for (var i = 0; i < blockCount; ++i) {
          quot[posQuot + i] = unchecked((short)0xffff);
        }
        Array.Clear(quot, posQuot + blockCount, blockCount);
        // copy AMidHigh to temp
        Array.Copy(
       valueAMidHigh,
       posAMidHigh,
       tmp,
       blockCount * 4,
       blockCount * 2);
       // subtract BHigh from temp's high block
        SubtractInternal(
  tmp,
  blockCount * 5,
  tmp,
  blockCount * 5,
  b,
  posB + blockCount,
  blockCount);
        // add BHigh to temp
        c = AddInternal(
  tmp,
  blockCount * 4,
  tmp,
  blockCount * 4,
  b,
  posB + blockCount,
  blockCount);
        Increment(tmp, blockCount * 5, blockCount, (short)c);
      }
      AsymmetricMultiply(
  tmp,
  0,
  tmp,
  blockCount * 2,
  quot,
  posQuot,
  blockCount,
  b,
  posB,
  blockCount);
      int bc3 = blockCount * 3;
      Array.Copy(valueALow, posALow, tmp, bc3, blockCount);
      Array.Clear(tmp, blockCount * 2, blockCount);
      c = SubtractInternal(tmp, bc3, tmp, bc3, tmp, 0, blockCount * 3);
      if (c != 0) {
        while (true) {
          c = AddInternal(tmp, bc3, tmp, bc3, b, posB, blockCount * 2);
          c = Increment(tmp, blockCount * 5, blockCount, (short)c);
          Decrement(quot, posQuot, blockCount * 2, (short)1);
          if (c != 0) {
            break;
          }
        }
      }
      Array.Copy(tmp, bc3, rem, posRem, blockCount * 2);
    }

    private static void RecursiveDivideInner(
      short[] a,
      int posA,
      short[] b,
      int posB,
      short[] quot,
      int posQuot,
      short[] rem,
      int posRem,
      int blockSize) {
// NOTE: size of 'a', 'quot', and 'rem' is 'blockSize'*2
// NOTE: size of 'b' is 'blockSize'
#if DEBUG
if (a == null) {
  throw new ArgumentNullException(nameof(a));
}
if (posA < 0) {
  throw new ArgumentException("posA (" + posA +
    ") is less than 0");
}
if (posA > a.Length) {
  throw new ArgumentException("posA (" + posA + ") is more than " +
    a.Length);
}
if ((blockSize * 2) < 0) {
  throw new ArgumentException("(blockSize*2) (" + (blockSize * 2) +
    ") is less than 0");
}
if ((blockSize * 2) > a.Length) {
  throw new ArgumentException("(blockSize*2) (" + (blockSize * 2) +
    ") is more than " + a.Length);
}
if ((a.Length - posA) < (blockSize * 2)) {
  throw new ArgumentException("a's length minus " + posA + " (" +
    (a.Length - posA) + ") is less than " + (blockSize * 2));
}
if (b == null) {
  throw new ArgumentNullException(nameof(b));
}
if (posB < 0) {
  throw new ArgumentException("posB (" + posB +
    ") is less than 0");
}
if (posB > b.Length) {
  throw new ArgumentException("posB (" + posB + ") is more than " +
    b.Length);
}
if (blockSize < 0) {
  throw new ArgumentException("blockSize (" + blockSize +
    ") is less than 0");
}
if (blockSize > b.Length) {
  throw new ArgumentException("blockSize (" + blockSize +
    ") is more than " + b.Length);
}
if (b.Length - posB < blockSize) {
  throw new ArgumentException("b's length minus " + posB + " (" +
    (b.Length - posB) + ") is less than " + blockSize);
}
if (quot != null) {
if (posQuot < 0) {
  throw new ArgumentException("posQuot (" + posQuot +
    ") is less than 0");
}
if (posQuot > quot.Length) {
  throw new ArgumentException("posQuot (" + posQuot +
    ") is more than " + quot.Length);
}
if ((blockSize * 2) < 0) {
  throw new ArgumentException("blockSize*2 (" + (blockSize * 2) +
    ") is less than 0");
}
if ((blockSize * 2) > quot.Length) {
  throw new ArgumentException("blockSize*2 (" + (blockSize * 2) +
    ") is more than " + quot.Length);
}
if (quot.Length - posQuot < blockSize * 2) {
  throw new ArgumentException("quot's length minus " + posQuot + " (" +
    (quot.Length - posQuot) + ") is less than " + (blockSize * 2));
}
}
        if (rem != null) {
if (posRem < 0) {
  throw new ArgumentException("posRem (" + posRem +
    ") is less than 0");
}
if (posRem > rem.Length) {
  throw new ArgumentException("posRem (" + posRem +
    ") is more than " + rem.Length);
}
if ((blockSize * 2) < 0) {
  throw new ArgumentException("blockSize*2 (" + (blockSize * 2) +
    ") is less than 0");
}
if ((blockSize * 2) > rem.Length) {
  throw new ArgumentException("blockSize*2 (" + (blockSize * 2) +
    ") is more than " + rem.Length);
}
if (rem.Length - posRem < (blockSize * 2)) {
  throw new ArgumentException("rem's length minus " + posRem + " (" +
    (rem.Length - posRem) + ") is less than " + (blockSize * 2));
}
}
#endif
      // Implements Algorithm 1 of Burnikel & Ziegler 1998
      if (blockSize < RecursiveDivisionLimit || (blockSize & 1) == 1) {
        GeneralDivide(
  a,
  posA,
  blockSize * 2,
  b,
  posB,
  blockSize,
  quot,
  posQuot,
  rem,
  posRem);
      } else {
                int halfBlock = blockSize >> 1;
        var tmp = new short[halfBlock * 10];
        Array.Clear(quot, posQuot, blockSize * 2);
        Array.Clear(rem, posRem, blockSize);
        DivideThreeBlocksByTwo(
  a,
  posA + halfBlock,
  a,
  posA + blockSize,
  b,
  posB,
  halfBlock,
  tmp,
  halfBlock * 6,
  tmp,
  halfBlock * 8,
  tmp);
        DivideThreeBlocksByTwo(
  a,
  posA,
  tmp,
  halfBlock * 8,
  b,
  posB,
  halfBlock,
  quot,
  posQuot,
  rem,
  posRem,
  tmp);
        Array.Copy(tmp, halfBlock * 6, quot, posQuot + halfBlock, halfBlock);
      }
    }

    private static void RecursiveDivide(
     short[] a,
     int posA,
     int countA,
     short[] b,
     int posB,
     int countB,
     short[] quot,
     int posQuot,
     short[] rem,
     int posRem) {
#if DEBUG
if (countB <= RecursiveDivisionLimit) {
  throw new ArgumentException("countB (" + countB +
    ") is not greater than " +
    RecursiveDivisionLimit);
}
if (a == null) {
  throw new ArgumentNullException(nameof(a));
}
if (posA < 0) {
  throw new ArgumentException("posA (" + posA +
    ") is less than 0");
}
if (posA > a.Length) {
  throw new ArgumentException("posA (" + posA + ") is more than " +
    a.Length);
}
if (countA < 0) {
  throw new ArgumentException("countA (" + countA +
    ") is less than 0");
}
if (countA > a.Length) {
  throw new ArgumentException("countA (" + countA +
    ") is more than " + a.Length);
}
if (a.Length - posA < countA) {
  throw new ArgumentException("a's length minus " + posA + " (" +
    (a.Length - posA) + ") is less than " + countA);
}
if (b == null) {
  throw new ArgumentNullException(nameof(b));
}
if (posB < 0) {
  throw new ArgumentException("posB (" + posB +
    ") is less than 0");
}
if (posB > b.Length) {
  throw new ArgumentException("posB (" + posB + ") is more than " +
    b.Length);
}
if (countB < 0) {
  throw new ArgumentException("countB (" + countB +
    ") is less than 0");
}
if (countB > b.Length) {
  throw new ArgumentException("countB (" + countB +
    ") is more than " + b.Length);
}
if (b.Length - posB < countB) {
  throw new ArgumentException("b's length minus " + posB + " (" +
    (b.Length - posB) + ") is less than " + countB);
}
        if (rem != null) {
if (posRem < 0) {
  throw new ArgumentException("posRem (" + posRem +
    ") is less than 0");
}
if (posRem > rem.Length) {
  throw new ArgumentException("posRem (" + posRem +
    ") is more than " + rem.Length);
}
if (countB < 0) {
  throw new ArgumentException("countB (" + countB +
    ") is less than 0");
}
if (countB > rem.Length) {
  throw new ArgumentException("countB (" + countB +
    ") is more than " + rem.Length);
}
if (rem.Length - posRem < countB) {
  throw new ArgumentException("rem's length minus " + posRem + " (" +
    (rem.Length - posRem) + ") is less than " + countB);
}
}
#endif
      int workPosA, workPosB, i;
      short[] workA = a;
      short[] workB = b;
      workPosA = posA;
      workPosB = posB;
      int blocksB = RecursiveDivisionLimit;
      var shiftB = 0;
      var m = 1;
      while (blocksB < countB) {
        blocksB <<= 1;
        m <<= 1;
      }
      workB = new short[blocksB];
      workPosB = 0;
      Array.Copy(b, posB, workB, blocksB - countB, countB);
      var shiftA = 0;
      var extraWord = 0;
      int wordsA = countA + (blocksB - countB);
      if ((b[countB - 1] & 0x8000) == 0) {
        int x = b[countB - 1];
        while ((x & 0x8000) == 0) {
          ++shiftB;
          x <<= 1;
        }
        x = a[countA - 1];
        while ((x & 0x8000) == 0) {
          ++shiftA;
          x <<= 1;
        }
        if (shiftA < shiftB) {
          // Shifting A would require an extra word
          ++extraWord;
        }
        ShiftWordsLeftByBits(
  workB,
  workPosB + blocksB - countB,
  countB,
  shiftB);
      }
      int blocksA = (wordsA + extraWord + (blocksB - 1)) / blocksB;
      int totalWordsA = blocksA * blocksB;
      workA = new short[totalWordsA];
      workPosA = 0;
      Array.Copy(
  a,
  posA,
  workA,
  workPosA + (blocksB - countB),
  countA);
      ShiftWordsLeftByBits(
  workA,
  workPosA + (blocksB - countB),
  countA + extraWord,
  shiftB);
      // Start division
      // "tmprem" holds temporary space for the following:
      // - blocksB: Remainder
      // - blocksB * 2: Dividend
      // - blocksB * 2: Quotient
      var tmprem = new short[blocksB * 5];
      var size = 0;
      for (i = blocksA - 1; i >= 0; --i) {
        int workAIndex = workPosA + (i * blocksB);
        // Set the low part of the sub-dividend with the working
        // block of the dividend
        Array.Copy(workA, workAIndex, tmprem, blocksB, blocksB);
        // Clear the quotient
        Array.Clear(tmprem, blocksB * 3, blocksB << 1);
        RecursiveDivideInner(
  tmprem,
  blocksB,
  workB,
  workPosB,
  tmprem,
  blocksB * 3,
  tmprem,
  0,
  blocksB);
        if (quot != null) {
          size = Math.Min(blocksB, quot.Length - (i * blocksB));
          // DebugUtility.Log("quot len=" + quot.Length + ",bb=" + blocksB +
          // ",size=" + size + " [" + countA + "," + countB + "]");
          if (size > 0) {
          Array.Copy(
  tmprem,
 blocksB * 3,
 quot,
 posQuot + (i * blocksB),
              size);
          }
        }
        // Set the high part of the sub-dividend with the remainder
        Array.Copy(tmprem, 0, tmprem, blocksB << 1, blocksB);
      }
      if (rem != null) {
        Array.Copy(tmprem, blocksB - countB, rem, posRem, countB);
        ShiftWordsRightByBits(rem, posRem, countB, shiftB);
      }
    }

    private static string WordsToString(short[] a, int pos, int len) {
      while (len != 0 && a[pos + len - 1] == 0) {
                --len;
            }
      if (len == 0) {
 return "\"0\"";
}
      var words = new short[len];
      Array.Copy(a, pos, words, 0, len);
      return "\"" + new EInteger(len, words, false).ToUnoptString() + "\"";
    }

        private static string WordsToStringHex(short[] a, int pos, int len) {
            while (len != 0 && a[pos + len - 1] == 0) {
                --len;
            }
            if (len == 0) {
                return "\"0\"";
            }
            var words = new short[len];
            Array.Copy(a, pos, words, 0, len);
      return "\"" + new EInteger(len, words, false).ToRadixString(16) +
              "\"";
        }

        private static string WordsToString2(
  short[] a,
  int pos,
  int len,
  short[] b,
  int pos2,
  int len2) {
      var words = new short[len + len2];
      Array.Copy(a, pos, words, 0, len);
      Array.Copy(b, pos2, words, len, len2);
       len += len2;
      while (len != 0 && words[len - 1] == 0) {
                --len;
      }
      return (len == 0) ?
  "\"0\"" : ("\"" + new EInteger(
      len,
      words,
      false).ToUnoptString() + "\"");
    }

    private static void GeneralDivide(
     short[] a,
     int posA,
     int countA,
     short[] b,
     int posB,
     int countB,
     short[] quot,
     int posQuot,
     short[] rem,
     int posRem) {
#if DEBUG
      if (!(countA > 0 && countB > 0)) {
 throw new ArgumentException("doesn't satisfy countA>0 && countB>0");
      }
if (a == null) {
  throw new ArgumentNullException(nameof(a));
}
if (posA < 0) {
  throw new ArgumentException("posA (" + posA +
    ") is less than 0");
}
if (posA > a.Length) {
  throw new ArgumentException("posA (" + posA + ") is more than " +
    a.Length);
}
if (countA < 0) {
  throw new ArgumentException("countA (" + countA +
    ") is less than 0");
}
if (countA > a.Length) {
  throw new ArgumentException("countA (" + countA +
    ") is more than " + a.Length);
}
if (a.Length - posA < countA) {
  throw new ArgumentException("a's length minus " + posA + " (" +
    (a.Length - posA) + ") is less than " + countA);
}
if (b == null) {
  throw new ArgumentNullException(nameof(b));
}
if (posB < 0) {
  throw new ArgumentException("posB (" + posB +
    ") is less than 0");
}
if (posB > b.Length) {
  throw new ArgumentException("posB (" + posB + ") is more than " +
    b.Length);
}
if (countB < 0) {
  throw new ArgumentException("countB (" + countB +
    ") is less than 0");
}
if (countB > b.Length) {
  throw new ArgumentException("countB (" + countB +
    ") is more than " + b.Length);
}
if (b.Length - posB < countB) {
  throw new ArgumentException("b's length minus " + posB + " (" +
    (b.Length - posB) + ") is less than " + countB);
}
if (quot != null) {
if (posQuot < 0) {
  throw new ArgumentException("posQuot (" + posQuot +
    ") is less than 0");
}
if (posQuot > quot.Length) {
  throw new ArgumentException("posQuot (" + posQuot +
    ") is more than " + quot.Length);
}
if (countA - countB + 1 < 0) {
  throw new ArgumentException("(countA-countB+1) (" + (countA - countB + 1) +
    ") is less than 0");
}
if (countA - countB + 1 > quot.Length) {
  throw new ArgumentException("(countA-countB+1) (" + (countA - countB + 1) +
    ") is more than " + quot.Length);
}
if ((quot.Length - posQuot) < (countA - countB + 1)) {
  throw new ArgumentException("quot's length minus " + posQuot + " (" +
    (quot.Length - posQuot) + ") is less than " +
    (countA - countB + 1));
}
}
if (rem != null) {
if (posRem < 0) {
  throw new ArgumentException("posRem (" + posRem +
    ") is less than 0");
}
if (posRem > rem.Length) {
  throw new ArgumentException("posRem (" + posRem +
    ") is more than " + rem.Length);
}
if (countB < 0) {
  throw new ArgumentException("countB (" + countB +
    ") is less than 0");
}
if (countB > rem.Length) {
  throw new ArgumentException("countB (" + countB +
    ") is more than " + rem.Length);
}
if (rem.Length - posRem < countB) {
  throw new ArgumentException("rem's length minus " + posRem + " (" +
    (rem.Length - posRem) + ") is less than " + countB);
}
        }
#endif
      int origQuotSize = countA - countB + 1;
      int origCountA = countA;
      int origCountB = countB;
      while (countB > 0 && b[posB + countB - 1] == 0) {
        --countB;
      }
      while (countA > 0 && a[posA + countA - 1] == 0) {
        --countA;
      }
      int newQuotSize = countA - countB + 1;
      if (quot != null) {
          if (newQuotSize < 0 || newQuotSize >= origQuotSize) {
            Array.Clear(quot, posQuot, Math.Max(0, origQuotSize));
          } else {
            Array.Clear(
  quot,
  posQuot + newQuotSize,
  Math.Max(0, origQuotSize - newQuotSize));
          }
      }
      if (rem != null) {
          Array.Clear(rem, posRem + countB, origCountB - countB);
      }
#if DEBUG
      if (countA != 0 && !(a[posA + countA - 1] != 0)) {
        throw new ArgumentException();
      }
      if (countB == 0 || !(b[posB + countB - 1] != 0)) {
        throw new ArgumentException();
      }
#endif
      if (countA < countB) {
        // A is less than B, so quotient is 0, remainder is "a"
        if (quot != null) {
          Array.Clear(quot, posQuot, Math.Max(0, origQuotSize));
        }
        if (rem != null) {
          Array.Copy(a, posA, rem, posRem, origCountA);
        }
        return;
      } else if (countA == countB) {
        int cmp = Compare(a, posA, b, posB, countA);
        if (cmp == 0) {
          // A equals B, so quotient is 1, remainder is 0
          if (quot != null) {
            quot[posQuot] = 1;
            Array.Clear(quot, posQuot + 1, Math.Max(0, origQuotSize - 1));
          }
          if (rem != null) {
            Array.Clear(rem, posRem, countA);
          }
          return;
        } else if (cmp < 0) {
          // A is less than B, so quotient is 0, remainder is "a"
          if (quot != null) {
            Array.Clear(quot, posQuot, Math.Max(0, origQuotSize));
          }
          if (rem != null) {
            Array.Copy(a, posA, rem, posRem, origCountA);
          }
          return;
        }
      }
      if (countB == 1) {
        // Divisor is a single word
        short shortRemainder = FastDivideAndRemainder(
              quot,
              posQuot,
              a,
              posA,
              countA,
              b[posB]);
        if (rem != null) {
          rem[posRem] = shortRemainder;
        }
        return;
      }
      int workPosA, workPosB;
      short[] workAB = null;
      short[] workA = a;
      short[] workB = b;
      workPosA = posA;
      workPosB = posB;
      if (countB > RecursiveDivisionLimit) {
        RecursiveDivide(
  a,
  posA,
  countA,
  b,
  posB,
  countB,
  quot,
  posQuot,
  rem,
  posRem);
        return;
      }
      var sh = 0;
      var noShift = false;
      if ((b[posB + countB - 1] & 0x8000) == 0) {
        // Normalize a and b by shifting both until the high
        // bit of b is the highest bit of the last word
        int x = b[posB + countB - 1];
        if (x == 0) {
          throw new InvalidOperationException();
        }
        while ((x & 0x8000) == 0) {
          ++sh;
          x <<= 1;
        }
        workAB = new short[countA + 1 + countB];
        workPosA = 0;
        workPosB = countA + 1;
        workA = workAB;
        workB = workAB;
        Array.Copy(a, posA, workA, workPosA, countA);
        Array.Copy(b, posB, workB, workPosB, countB);
        ShiftWordsLeftByBits(workA, workPosA, countA + 1, sh);
        ShiftWordsLeftByBits(workB, workPosB, countB, sh);
      } else {
        noShift = true;
        workA = new short[countA + 1];
        workPosA = 0;
        Array.Copy(a, posA, workA, workPosA, countA);
      }
      var c = 0;
      short pieceBHigh = workB[workPosB + countB - 1];
      int pieceBHighInt = ((int)pieceBHigh) & 0xffff;
      int endIndex = workPosA + countA;
#if DEBUG
      // Assert that pieceBHighInt is normalized
      if (!((pieceBHighInt & 0x8000) != 0)) {
    throw new ArgumentException("doesn't satisfy (pieceBHighInt & 0x8000)!=0");
      }
#endif
      short pieceBNextHigh = workB[workPosB + countB - 2];
      int pieceBNextHighInt = ((int)pieceBNextHigh) & 0xffff;
      for (int offset = countA - countB; offset >= 0; --offset) {
        int wpoffset = workPosA + offset;
        int wpaNextHigh = ((int)workA[wpoffset + countB - 1]) & 0xffff;
        var wpaHigh = 0;
        if (!noShift || wpoffset + countB < endIndex) {
          wpaHigh = ((int)workA[wpoffset + countB]) & 0xffff;
        }
        int dividend = unchecked(wpaNextHigh + (wpaHigh << 16));
        int divnext = ((int)workA[wpoffset + countB - 2]) & 0xffff;
        int quorem0 = (dividend >> 31) == 0 ? (dividend / pieceBHighInt) :
         unchecked((int)(((long)dividend & 0xffffffffL) / pieceBHighInt));
        int quorem1 = unchecked(dividend - (quorem0 * pieceBHighInt));
        // DebugUtility.Log("{0:X8}/{1:X4} = {2:X8},{3:X4}",
        // dividend, pieceBHigh, quorem0, quorem1);
        long t = (((long)quorem1) << 16) | (divnext & 0xffffL);
        // NOTE: quorem0 won't be higher than (1<< 16)+1 as long as
        // pieceBHighInt is
        // normalized (see Burnikel & Ziegler 1998). Since the following
        // code block
        // corrects all cases where quorem0 is too high by 2, and all
        // remaining cases
        // will reduce quorem0 by 1 if it's at least (1<< 16), quorem0 will
        // be guaranteed to
        // have a bit length of 16 or less by the end of the code block.
        if ((quorem0 >> 16) != 0 ||
          (unchecked(quorem0 * pieceBNextHighInt) & 0xffffffffL) > t) {
          quorem1 += pieceBHighInt;
          --quorem0;
          if ((quorem1 >> 16) == 0) {
            t = (((long)quorem1) << 16) | (divnext & 0xffffL);
            if ((quorem0 >> 16) != 0 ||
                (unchecked(quorem0 * pieceBNextHighInt) & 0xffffffffL) > t) {
              --quorem0;
              if (rem == null && offset == 0) {
                // We can stop now and break; all cases where quorem0
                // is 2 too big will have been caught by now
                if (quot != null) {
                  quot[posQuot + offset] = unchecked((short)quorem0);
                }
                break;
              }
            }
          }
        }
        int q1 = quorem0 & 0xffff;
#if DEBUG
        int q2 = (quorem0 >> 16) & 0xffff;
        if (q2 != 0) {
          // NOTE: The checks above should have ensured that quorem0 can't
          // be longer than 16 bits.
          throw new InvalidOperationException();
        }
#endif

  c = LinearMultiplySubtractMinuend1Bigger(
  workA,
  wpoffset,
  workA,
  wpoffset,
  q1,
  workB,
  workPosB,
  countB);
        if (c != 0) {
          // T(workA,workPosA,countA+1,"workA X");
          c = AddInternal(
  workA,
  wpoffset,
  workA,
  wpoffset,
  workB,
  workPosB,
  countB);
          c = Increment(workA, wpoffset + countB, 1, (short)c);
          // T(workA,workPosA,countA+1,"workA "+c);
          --quorem0;
        }
        if (quot != null) {
          quot[posQuot + offset] = unchecked((short)quorem0);
        }
      }
      if (rem != null) {
        if (sh != 0) {
          ShiftWordsRightByBits(workA, workPosA, countB + 1, sh);
        }
        Array.Copy(workA, workPosA, rem, posRem, countB);
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.DivRem(PeterO.Numbers.EInteger)"]/*'/>
    public EInteger[] DivRem(EInteger divisor) {
      if (divisor == null) {
        throw new ArgumentNullException(nameof(divisor));
      }
      int words1Size = this.wordCount;
      int words2Size = divisor.wordCount;
      if (words2Size == 0) {
        // Divisor is 0
        throw new DivideByZeroException();
      }

      if (words1Size < words2Size) {
        // Dividend is less than divisor (includes case
        // where dividend is 0)
        return new[] { EInteger.Zero, this };
      }
      if (words2Size == 1) {
        // divisor is small, use a fast path
        var quotient = new short[this.wordCount];
        int smallRemainder;
        switch (divisor.words[0]) {
          case 2:
            smallRemainder = (int)FastDivideAndRemainderTwo(
             quotient,
             0,
             this.words,
             0,
             words1Size);
            break;
          case 10:
            smallRemainder = (int)FastDivideAndRemainderTen(
             quotient,
             0,
             this.words,
             0,
             words1Size);
            break;
          default:
            // DebugUtility.Log("smalldiv=" + (divisor.words[0]));
            smallRemainder = ((int)FastDivideAndRemainder(
              quotient,
              0,
              this.words,
              0,
              words1Size,
              divisor.words[0])) & 0xffff;
            break;
        }
        int count = this.wordCount;
        while (count != 0 &&
               quotient[count - 1] == 0) {
          --count;
        }
        if (count == 0) {
          return new[] { EInteger.Zero, this };
        }
        quotient = ShortenArray(quotient, count);
        var bigquo = new EInteger(
          count,
          quotient,
          this.negative ^ divisor.negative);
        if (this.negative) {
          smallRemainder = -smallRemainder;
        }
        return new[] { bigquo, EInteger.FromInt64(smallRemainder) };
      }
      if (this.CanFitInInt32() && divisor.CanFitInInt32()) {
        long dividendSmall = this.ToInt32Checked();
        long divisorSmall = divisor.ToInt32Checked();
        if (dividendSmall != Int32.MinValue || divisorSmall != -1) {
          long quotientSmall = dividendSmall / divisorSmall;
          long remainderSmall = dividendSmall - (quotientSmall * divisorSmall);
          return new[] {
            EInteger.FromInt64(quotientSmall),
            EInteger.FromInt64(remainderSmall) };
        }
      } else if (this.CanFitInInt64() && divisor.CanFitInInt64()) {
        long dividendLong = this.ToInt64Checked();
        long divisorLong = divisor.ToInt64Checked();
        if (dividendLong != Int64.MinValue || divisorLong != -1) {
          long quotientLong = dividendLong / divisorLong;
          long remainderLong = dividendLong - (quotientLong * divisorLong);
          return new[] {
            EInteger.FromInt64(quotientLong),
            EInteger.FromInt64(remainderLong) };
        }
        // DebugUtility.Log("int64divrem {0}/{1}"
        // , this.ToInt64Checked(), divisor.ToInt64Checked());
      }
      // --- General case
      var bigRemainderreg = new short[((int)words2Size)];
      var quotientreg = new short[((int)(words1Size - words2Size + 1))];
      GeneralDivide(
  this.words,
  0,
  this.wordCount,
  divisor.words,
  0,
  divisor.wordCount,
  quotientreg,
  0,
  bigRemainderreg,
  0);
      int remCount = CountWords(bigRemainderreg);
      int quoCount = CountWords(quotientreg);
      bigRemainderreg = ShortenArray(bigRemainderreg, remCount);
      quotientreg = ShortenArray(quotientreg, quoCount);
      EInteger bigrem = (remCount == 0) ? EInteger.Zero : new
        EInteger(remCount, bigRemainderreg, this.negative);
      EInteger bigquo2 = (quoCount == 0) ? EInteger.Zero : new
        EInteger(quoCount, quotientreg, this.negative ^ divisor.negative);
      return new[] { bigquo2, bigrem };
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Equals(System.Object)"]/*'/>
    public override bool Equals(object obj) {
      var other = obj as EInteger;
      if (other == null) {
        return false;
      }
      if (this.wordCount == other.wordCount) {
        if (this.negative != other.negative) {
          return false;
        }
        for (var i = 0; i < this.wordCount; ++i) {
          if (this.words[i] != other.words[i]) {
            return false;
          }
        }
        return true;
      }
      return false;
    }

    private static EInteger LeftShiftBigIntVar(EInteger ei, EInteger bigShift) {
      if (ei.IsZero) {
        return ei;
      }
      while (bigShift.Sign > 0) {
        var shift = 1000000;
        if (bigShift.CompareTo((EInteger)1000000) < 0) {
          shift = bigShift.ToInt32Checked();
        }
        ei <<= shift;
        bigShift -= (EInteger)shift;
      }
      return ei;
    }

    private static EInteger GcdLong(long u, long v) {
      // Adapted from Christian Stigen Larsen's
      // public domain GCD code
#if DEBUG
      if (!(u >= 0 && v >= 0)) {
        throw new ArgumentException("doesn't satisfy u>= 0 && v>= 0");
      }
#endif

      var shl = 0;
      while (u != 0 && v != 0 && u != v) {
        bool eu = (u & 1L) == 0;
        bool ev = (v & 1L) == 0;
        if (eu && ev) {
          ++shl;
          u >>= 1;
          v >>= 1;
        } else if (eu && !ev) {
          u >>= 1;
        } else if (!eu && ev) {
          v >>= 1;
        } else if (u >= v) {
          u = (u - v) >> 1;
        } else {
          long tmp = u;
          u = (v - u) >> 1;
          v = tmp;
        }
      }
      EInteger eret = (u == 0) ?
        EInteger.FromInt64(v << shl) : EInteger.FromInt64(u << shl);
      return eret;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Gcd(PeterO.Numbers.EInteger)"]/*'/>
    public EInteger Gcd(EInteger bigintSecond) {
      if (bigintSecond == null) {
        throw new ArgumentNullException(nameof(bigintSecond));
      }
      if (this.IsZero) {
        return bigintSecond.Abs();
      }
      EInteger thisValue = this.Abs();
      if (bigintSecond.IsZero) {
        return thisValue;
      }
      bigintSecond = bigintSecond.Abs();
      if (bigintSecond.Equals(EInteger.One) ||
          thisValue.Equals(bigintSecond)) {
        return bigintSecond;
      }
      if (thisValue.Equals(EInteger.One)) {
        return thisValue;
      }
      if (thisValue.CanFitInInt64() && bigintSecond.CanFitInInt64()) {
        long u = thisValue.ToInt64Unchecked();
        long v = bigintSecond.ToInt64Unchecked();
        return GcdLong(u, v);
      } else {
        // Big integer version of code above
        var bshl = 0;
        EInteger ebshl = null;
        short[] bu = thisValue.Copy();
        short[] bv = bigintSecond.Copy();
        int buc = thisValue.wordCount;
        int bvc = bigintSecond.wordCount;
        while (buc != 0 && bvc != 0 && !WordsEqual(bu, buc, bv, bvc)) {
          if (buc <= 3 && bvc <= 3) {
            return GcdLong(
              WordsToLongUnchecked(bu, buc),
              WordsToLongUnchecked(bv, bvc));
          }
          if ((bu[0] & 0x0f) == 0 && (bv[0] & 0x0f) == 0) {
            if (bshl < 0) {
              ebshl += EInteger.FromInt32(4);
            } else if (bshl == Int32.MaxValue - 3) {
              ebshl = EInteger.FromInt32(Int32.MaxValue - 3);
              ebshl += EInteger.FromInt32(4);
              bshl = -1;
            } else {
              bshl += 4;
            }
            buc = WordsShiftRightFour(bu, buc);
            bvc = WordsShiftRightFour(bv, bvc);
            continue;
          }
          bool eu = (bu[0] & 0x01) == 0;
          bool ev = (bv[0] & 0x01) == 0;
          if (eu && ev) {
            if (bshl < 0) {
              ebshl += EInteger.One;
            } else if (bshl == Int32.MaxValue) {
              ebshl = EInteger.FromInt32(Int32.MaxValue);
              ebshl += EInteger.One;
              bshl = -1;
            } else {
              ++bshl;
            }
            buc = WordsShiftRightOne(bu, buc);
            bvc = WordsShiftRightOne(bv, bvc);
          } else if (eu && !ev) {
            buc = (Math.Abs(buc - bvc) > 1 && (bu[0] & 0x0f) == 0) ?
              WordsShiftRightFour(bu, buc) :
WordsShiftRightOne(bu, buc);
          } else if (!eu && ev) {
            if ((bv[0] & 0xff) == 0 && Math.Abs(buc - bvc) > 1) {
              // DebugUtility.Log("bv8");
              bvc = WordsShiftRightEight(bv, bvc);
            } else {
              bvc = (
             (bv[0] & 0x0f) == 0 && Math.Abs(
             buc - bvc) > 1) ?
             WordsShiftRightFour(bv, bvc) : WordsShiftRightOne(bv, bvc);
            }
          } else if (WordsCompare(bu, buc, bv, bvc) >= 0) {
            buc = WordsSubtract(bu, buc, bv, bvc);
            buc = (Math.Abs(buc - bvc) > 1 && (bu[0] & 0x02) == 0) ?
              WordsShiftRightTwo(bu, buc) : WordsShiftRightOne(bu, buc);
          } else {
            short[] butmp = bv;
            short[] bvtmp = bu;
            int buctmp = bvc;
            int bvctmp = buc;
            buctmp = WordsSubtract(butmp, buctmp, bvtmp, bvctmp);
            buctmp = WordsShiftRightOne(butmp, buctmp);
            bu = butmp;
            bv = bvtmp;
            buc = buctmp;
            bvc = bvctmp;
          }
        }
        var valueBuVar = new EInteger(buc, bu, false);
        var valueBvVar = new EInteger(bvc, bv, false);
        if (bshl >= 0) {
          valueBuVar = valueBuVar.IsZero ? (valueBvVar << bshl) : (valueBuVar <<
                   bshl);
        } else {
          valueBuVar = valueBuVar.IsZero ?
  LeftShiftBigIntVar(
 valueBvVar,
 ebshl) : LeftShiftBigIntVar(
 valueBuVar,
 ebshl);
        }
        return valueBuVar;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.GetDigitCount"]/*'/>
    public int GetDigitCount() {
      if (this.IsZero) {
        return 1;
      }
      if (this.HasSmallValue()) {
        long value = this.ToInt64Checked();
        if (value == Int64.MinValue) {
          return 19;
        }
        if (value < 0) {
          value = -value;
        }
        if (value >= 1000000000L) {
          return (value >= 1000000000000000000L) ? 19 : ((value >=
                   100000000000000000L) ? 18 : ((value >= 10000000000000000L) ?
                    17 : ((value >= 1000000000000000L) ? 16 :
                    ((value >= 100000000000000L) ? 15 : ((value
                    >= 10000000000000L) ?
                    14 : ((value >= 1000000000000L) ? 13 : ((value
                    >= 100000000000L) ? 12 : ((value >= 10000000000L) ?
                    11 : ((value >= 1000000000L) ? 10 : 9)))))))));
        } else {
          var v2 = (int)value;
          return (v2 >= 100000000) ? 9 : ((v2 >= 10000000) ? 8 : ((v2 >=
                    1000000) ? 7 : ((v2 >= 100000) ? 6 : ((v2
                    >= 10000) ? 5 : ((v2 >= 1000) ? 4 : ((v2 >= 100) ?
                    3 : ((v2 >= 10) ? 2 : 1)))))));
        }
      }
      int bitlen = this.GetUnsignedBitLength();
      if (bitlen <= 2135) {
        // (x*631305) >> 21 is an approximation
        // to trunc(x*log10(2)) that is correct up
        // to x = 2135; the multiplication would require
        // up to 31 bits in all cases up to 2135
        // (cases up to 63 are already handled above)
        int minDigits = 1 + (((bitlen - 1) * 631305) >> 21);
        int maxDigits = 1 + ((bitlen * 631305) >> 21);
        if (minDigits == maxDigits) {
          // Number of digits is the same for
          // all numbers with this bit length
          return minDigits;
        }
        return this.Abs().CompareTo(NumberUtility.FindPowerOfTen(minDigits)) >=
              0 ? maxDigits : minDigits;
      } else if (bitlen <= 6432162) {
        // Much more accurate approximation
        int minDigits = ApproxLogTenOfTwo(bitlen - 1);
        int maxDigits = ApproxLogTenOfTwo(bitlen);
        if (minDigits == maxDigits) {
          // Number of digits is the same for
          // all numbers with this bit length
          return 1 + minDigits;
        }
        if (bitlen < 50000) {
    return this.Abs().CompareTo(NumberUtility.FindPowerOfTen(minDigits + 1))
           >= 0 ? maxDigits + 1 : minDigits + 1;
        }
      }
      short[] tempReg = null;
      int currentCount = this.wordCount;
      var i = 0;
      while (currentCount != 0) {
        if (currentCount == 1 || (currentCount == 2 && tempReg[1] == 0)) {
          int rest = ((int)tempReg[0]) & 0xffff;
          if (rest >= 10000) {
            i += 5;
          } else if (rest >= 1000) {
            i += 4;
          } else if (rest >= 100) {
            i += 3;
          } else if (rest >= 10) {
            i += 2;
          } else {
            ++i;
          }
          break;
        }
        if (currentCount == 2 && tempReg[1] > 0 && tempReg[1] <= 0x7fff) {
          int rest = ((int)tempReg[0]) & 0xffff;
          rest |= (((int)tempReg[1]) & 0xffff) << 16;
          if (rest >= 1000000000) {
            i += 10;
          } else if (rest >= 100000000) {
            i += 9;
          } else if (rest >= 10000000) {
            i += 8;
          } else if (rest >= 1000000) {
            i += 7;
          } else if (rest >= 100000) {
            i += 6;
          } else if (rest >= 10000) {
            i += 5;
          } else if (rest >= 1000) {
            i += 4;
          } else if (rest >= 100) {
            i += 3;
          } else if (rest >= 10) {
            i += 2;
          } else {
            ++i;
          }
          break;
        } else {
          int wci = currentCount;
          short remainderShort = 0;
          int quo, rem;
          var firstdigit = false;
          short[] dividend = tempReg ?? this.words;
          // Divide by 10000
          while ((wci--) > 0) {
            int curValue = ((int)dividend[wci]) & 0xffff;
            int currentDividend = unchecked((int)(curValue |
                    ((int)remainderShort << 16)));
            quo = currentDividend / 10000;
            if (!firstdigit && quo != 0) {
              firstdigit = true;
              // Since we are dividing from left to right, the first
              // nonzero result is the first part of the
              // new quotient
              bitlen = GetUnsignedBitLengthEx(quo, wci + 1);
              if (bitlen <= 2135) {
                // (x*631305) >> 21 is an approximation
                // to trunc(x*log10(2)) that is correct up
                // to x = 2135; the multiplication would require
                // up to 31 bits in all cases up to 2135
                // (cases up to 64 are already handled above)
                int minDigits = 1 + (((bitlen - 1) * 631305) >> 21);
                int maxDigits = 1 + ((bitlen * 631305) >> 21);
                if (minDigits == maxDigits) {
                  // Number of digits is the same for
                  // all numbers with this bit length
                  // NOTE: The 4 is the number of digits just
                  // taken out of the number, and "i" is the
                  // number of previously known digits
                  return i + minDigits + 4;
                }
                if (minDigits > 1) {
                 int maxDigitEstimate = i + maxDigits + 4;
                 int minDigitEstimate = i + minDigits + 4;
 return this.Abs().CompareTo(NumberUtility.FindPowerOfTen(minDigitEstimate))
                >= 0 ? maxDigitEstimate : minDigitEstimate;
                }
              } else if (bitlen <= 6432162) {
                // Much more accurate approximation
                int minDigits = ApproxLogTenOfTwo(bitlen - 1);
                int maxDigits = ApproxLogTenOfTwo(bitlen);
                if (minDigits == maxDigits) {
                  // Number of digits is the same for
                  // all numbers with this bit length
                  return i + 1 + minDigits + 4;
                }
              }
            }
            if (tempReg == null) {
              if (quo != 0) {
                tempReg = new short[this.wordCount];
                Array.Copy(this.words, tempReg, tempReg.Length);
                // Use the calculated word count during division;
                // zeros that may have occurred in division
                // are not incorporated in the tempReg
                currentCount = wci + 1;
                tempReg[wci] = unchecked((short)quo);
              }
            } else {
              tempReg[wci] = unchecked((short)quo);
            }
            rem = currentDividend - (10000 * quo);
            remainderShort = unchecked((short)rem);
          }
          // Recalculate word count
          while (currentCount != 0 && tempReg[currentCount - 1] == 0) {
            --currentCount;
          }
          i += 4;
        }
      }
      return i;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.GetHashCode"]/*'/>
    public override int GetHashCode() {
      var hashCodeValue = 0;
      unchecked {
        hashCodeValue += 1000000007 * this.Sign;
        if (this.words != null) {
          for (var i = 0; i < this.wordCount; ++i) {
            hashCodeValue += 1000000013 * this.words[i];
          }
        }
      }
      return hashCodeValue;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.GetLowBit"]/*'/>
    public int GetLowBit() {
      var retSetBit = 0;
      for (var i = 0; i < this.wordCount; ++i) {
        int c = ((int)this.words[i]) & 0xffff;
        if (c == 0) {
          retSetBit += 16;
        } else {
          return (((c << 15) & 0xffff) != 0) ? (retSetBit + 0) : ((((c <<
                    14) & 0xffff) != 0) ? (retSetBit + 1) : ((((c <<
                    13) & 0xffff) != 0) ? (retSetBit + 2) : ((((c <<
                    12) & 0xffff) != 0) ? (retSetBit + 3) : ((((c << 11) &
                    0xffff) != 0) ? (retSetBit +
                    4) : ((((c << 10) & 0xffff) != 0) ? (retSetBit +
                    5) : ((((c << 9) & 0xffff) != 0) ? (retSetBit + 6) :
                    ((((c <<
                8) & 0xffff) != 0) ? (retSetBit + 7) : ((((c << 7) & 0xffff) !=
                    0) ? (retSetBit + 8) : ((((c << 6) & 0xffff) !=
                    0) ? (retSetBit + 9) : ((((c <<
                    5) & 0xffff) != 0) ? (retSetBit + 10) : ((((c <<
                    4) & 0xffff) != 0) ? (retSetBit + 11) : ((((c << 3) &
                    0xffff) != 0) ? (retSetBit + 12) : ((((c << 2) & 0xffff) !=
                    0) ? (retSetBit + 13) : ((((c << 1) & 0xffff) !=
                    0) ? (retSetBit + 14) : (retSetBit + 15)))))))))))))));
        }
      }
      return -1;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.GetLowBitAsEInteger"]/*'/>
    public EInteger GetLowBitAsEInteger() {
      long retSetBitLong = 0;
      for (var i = 0; i < this.wordCount; ++i) {
        int c = ((int)this.words[i]) & 0xffff;
        if (c == 0) {
          retSetBitLong += 16;
        } else {
          int rsb = (((c << 15) & 0xffff) != 0) ? 0 : ((((c <<
                    14) & 0xffff) != 0) ? 1 : ((((c <<
                    13) & 0xffff) != 0) ? 2 : ((((c <<
                    12) & 0xffff) != 0) ? 3 : ((((c << 11) &
                    0xffff) != 0) ? 4 : ((((c << 10) & 0xffff) != 0) ? 5 :
                ((((c << 9) & 0xffff) != 0) ? 6 : ((((c <<
            8) & 0xffff) != 0) ? 7 : ((((c << 7) & 0xffff) !=
                0) ? 8 : ((((c << 6) & 0xffff) != 0) ? 9 : ((((c <<
                    5) & 0xffff) != 0) ? 10 : ((((c <<
                    4) & 0xffff) != 0) ? 11 : ((((c << 3) &
                0xffff) != 0) ? 12 : ((((c << 2) & 0xffff) !=
                0) ? 13 : ((((c << 1) & 0xffff) != 0) ? 14 : 15))))))))))))));
          return EInteger.FromInt64(retSetBitLong).Add(
           EInteger.FromInt32(rsb));
        }
      }
      return EInteger.FromInt32(-1);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.GetSignedBit(System.Int32)"]/*'/>
    public bool GetSignedBit(int index) {
      if (index < 0) {
        throw new ArgumentOutOfRangeException("index");
      }
      if (this.wordCount == 0) {
        return false;
      }
      if (this.negative) {
        var tcindex = 0;
        int wordpos = index / 16;
        if (wordpos >= this.words.Length) {
          return true;
        }
        while (tcindex < wordpos && this.words[tcindex] == 0) {
          ++tcindex;
        }
        short tc;
        unchecked {
          tc = this.words[wordpos];
          if (tcindex == wordpos) {
            --tc;
          }
          tc = (short)~tc;
        }
        return (bool)(((tc >> (int)(index & 15)) & 1) != 0);
      }
      return this.GetUnsignedBit(index);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.GetSignedBitLength"]/*'/>
    public int GetSignedBitLength() {
      int wc = this.wordCount;
      if (wc != 0) {
        if (this.negative) {
          return this.Abs().Subtract(EInteger.One).GetSignedBitLength();
        }
        int numberValue = ((int)this.words[wc - 1]) & 0xffff;
        wc = (wc - 1) << 4;
        if (numberValue == 0) {
          return wc;
        }
        wc += 16;
        unchecked {
          if ((numberValue >> 8) == 0) {
            numberValue <<= 8;
            wc -= 8;
          }
          if ((numberValue >> 12) == 0) {
            numberValue <<= 4;
            wc -= 4;
          }
          if ((numberValue >> 14) == 0) {
            numberValue <<= 2;
            wc -= 2;
          }
          return ((numberValue >> 15) == 0) ? wc - 1 : wc;
        }
      }
      return 0;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.GetUnsignedBit(System.Int32)"]/*'/>
    public bool GetUnsignedBit(int index) {
      if (index < 0) {
        throw new ArgumentException("index (" + index + ") is less than 0");
      }
    return ((index >> 4) < this.words.Length) &&
        ((bool)(((this.words[(index >> 4)] >> (int)(index & 15)) & 1) != 0));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.GetUnsignedBitLengthAsEInteger"]/*'/>
    public EInteger GetUnsignedBitLengthAsEInteger() {
      int wc = this.wordCount;
      if (wc != 0) {
        int numberValue = ((int)this.words[wc - 1]) & 0xffff;
        EInteger ebase = EInteger.FromInt32(wc - 1).ShiftLeft(4);
        if (numberValue == 0) {
          return ebase;
        }
        wc = 16;
        unchecked {
          if ((numberValue >> 8) == 0) {
            numberValue <<= 8;
            wc -= 8;
          }
          if ((numberValue >> 12) == 0) {
            numberValue <<= 4;
            wc -= 4;
          }
          if ((numberValue >> 14) == 0) {
            numberValue <<= 2;
            wc -= 2;
          }
          if ((numberValue >> 15) == 0) {
            --wc;
          }
        }
        return ebase.Add(EInteger.FromInt32(wc));
      }
      return EInteger.Zero;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.GetUnsignedBitLength"]/*'/>
    public int GetUnsignedBitLength() {
      int wc = this.wordCount;
      if (wc != 0) {
        int numberValue = ((int)this.words[wc - 1]) & 0xffff;
        wc = (wc - 1) << 4;
        if (numberValue == 0) {
          return wc;
        }
        wc += 16;
        unchecked {
          if ((numberValue >> 8) == 0) {
            numberValue <<= 8;
            wc -= 8;
          }
          if ((numberValue >> 12) == 0) {
            numberValue <<= 4;
            wc -= 4;
          }
          if ((numberValue >> 14) == 0) {
            numberValue <<= 2;
            wc -= 2;
          }
          if ((numberValue >> 15) == 0) {
            --wc;
          }
        }
        return wc;
      }
      return 0;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Mod(PeterO.Numbers.EInteger)"]/*'/>
    public EInteger Mod(EInteger divisor) {
      if (divisor == null) {
        throw new ArgumentNullException(nameof(divisor));
      }
      if (divisor.Sign < 0) {
        throw new ArithmeticException("Divisor is negative");
      }
      EInteger remainderEInt = this.Remainder(divisor);
      if (remainderEInt.Sign < 0) {
        remainderEInt = divisor.Add(remainderEInt);
      }
      return remainderEInt;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ModPow(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger)"]/*'/>
    public EInteger ModPow(EInteger pow, EInteger mod) {
      if (pow == null) {
        throw new ArgumentNullException(nameof(pow));
      }
      if (mod == null) {
        throw new ArgumentNullException(nameof(mod));
      }
      if (pow.Sign < 0) {
        throw new ArgumentException("pow (" + pow + ") is less than 0");
      }
      if (mod.Sign <= 0) {
        throw new ArgumentException("mod (" + mod + ") is not greater than 0");
      }
      EInteger r = EInteger.One;
      EInteger eiv = this;
      while (!pow.IsZero) {
        if (!pow.IsEven) {
          r = (r * (EInteger)eiv).Mod(mod);
        }
        pow >>= 1;
        if (!pow.IsZero) {
          eiv = (eiv * (EInteger)eiv).Mod(mod);
        }
      }
      return r;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Multiply(PeterO.Numbers.EInteger)"]/*'/>
    public EInteger Multiply(EInteger bigintMult) {
      if (bigintMult == null) {
        throw new ArgumentNullException(nameof(bigintMult));
      }
      if (this.wordCount == 0 || bigintMult.wordCount == 0) {
        return EInteger.Zero;
      }
      if (this.wordCount == 1 && this.words[0] == 1) {
        return this.negative ? bigintMult.Negate() : bigintMult;
      }
      if (bigintMult.wordCount == 1 && bigintMult.words[0] == 1) {
        return bigintMult.negative ? this.Negate() : this;
      }
      short[] productreg;
      int productwordCount;
      var needShorten = true;
      if (this.wordCount == 1) {
        int wc;
        if (bigintMult.wordCount == 1) {
          // NOTE: Result can't be 0 here, since checks
          // for 0 were already made earlier in this function
          productreg = new short[2];
          int ba = ((int)this.words[0]) & 0xffff;
          int bb = ((int)bigintMult.words[0]) & 0xffff;
          ba = unchecked(ba * bb);
          productreg[0] = unchecked((short)(ba & 0xffff));
          productreg[1] = unchecked((short)((ba >> 16) & 0xffff));
          short preg = productreg[1];
          wc = (preg == 0) ? 1 : 2;
          return new EInteger(
  wc,
  productreg,
  this.negative ^ bigintMult.negative);
        }
        wc = bigintMult.wordCount;
        int regLength = wc + 1;
        productreg = new short[regLength];
        productreg[wc] = LinearMultiply(
          productreg,
          0,
          bigintMult.words,
          0,
          this.words[0],
          wc);
        productwordCount = productreg.Length;
        needShorten = false;
      } else if (bigintMult.wordCount == 1) {
        int wc = this.wordCount;
        int regLength = wc + 1;
        productreg = new short[regLength];
        productreg[wc] = LinearMultiply(
          productreg,
          0,
          this.words,
          0,
          bigintMult.words[0],
          wc);
        productwordCount = productreg.Length;
        needShorten = false;
      } else if (this.Equals(bigintMult)) {
        int words1Size = this.wordCount;
        productreg = new short[words1Size + words1Size];
        productwordCount = productreg.Length;
        var workspace = new short[words1Size + words1Size];
        RecursiveSquare(
          productreg,
          0,
          workspace,
          0,
          this.words,
          0,
          words1Size);
      } else if (this.wordCount <= 10 && bigintMult.wordCount <= 10) {
        int wc = this.wordCount + bigintMult.wordCount;
        productreg = new short[wc];
        productwordCount = productreg.Length;
        SchoolbookMultiply(
          productreg,
          0,
          this.words,
          0,
          this.wordCount,
          bigintMult.words,
          0,
          bigintMult.wordCount);
        needShorten = false;
      } else {
        int words1Size = this.wordCount;
        int words2Size = bigintMult.wordCount;
        productreg = new short[(words1Size + words2Size)];
        var workspace = new short[words1Size + words2Size];
        productwordCount = productreg.Length;
        AsymmetricMultiply(
          productreg,
          0,
          workspace,
          0,
          this.words,
          0,
          words1Size,
          bigintMult.words,
          0,
          words2Size);
      }
      // Recalculate word count
      while (productwordCount != 0 && productreg[productwordCount - 1] == 0) {
        --productwordCount;
      }
      if (needShorten) {
        productreg = ShortenArray(productreg, productwordCount);
      }
      return new EInteger(
        productwordCount,
        productreg,
        this.negative ^ bigintMult.negative);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Negate"]/*'/>
    public EInteger Negate() {
      return this.wordCount == 0 ? this : new EInteger(
        this.wordCount,
        this.words,
        !this.negative);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Pow(System.Int32)"]/*'/>
    public EInteger Pow(int powerSmall) {
      if (powerSmall < 0) {
        throw new ArgumentException("powerSmall (" + powerSmall +
                    ") is less than 0");
      }
      EInteger thisVar = this;
      if (powerSmall == 0) {
        // however 0 to the power of 0 is undefined
        return EInteger.One;
      }
      if (powerSmall == 1) {
        return this;
      }
      if (powerSmall == 2) {
        return thisVar * (EInteger)thisVar;
      }
      if (powerSmall == 3) {
        return (thisVar * (EInteger)thisVar) * (EInteger)thisVar;
      }
      EInteger r = EInteger.One;
      while (powerSmall != 0) {
        if ((powerSmall & 1) != 0) {
          r *= (EInteger)thisVar;
        }
        powerSmall >>= 1;
        if (powerSmall != 0) {
          thisVar *= (EInteger)thisVar;
        }
      }
      return r;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.PowBigIntVar(PeterO.Numbers.EInteger)"]/*'/>
    public EInteger PowBigIntVar(EInteger power) {
      if (power == null) {
        throw new ArgumentNullException(nameof(power));
      }
      int sign = power.Sign;
      if (sign < 0) {
        throw new ArgumentException(
          "sign (" + sign + ") is less than 0");
      }
      EInteger thisVar = this;
      if (sign == 0) {
        return EInteger.One;
      }
      if (power.Equals(EInteger.One)) {
        return this;
      }
      if (power.wordCount == 1 && power.words[0] == 2) {
        return thisVar * (EInteger)thisVar;
      }
      if (power.wordCount == 1 && power.words[0] == 3) {
        return (thisVar * (EInteger)thisVar) * (EInteger)thisVar;
      }
      EInteger r = EInteger.One;
      while (!power.IsZero) {
        if (!power.IsEven) {
          r *= (EInteger)thisVar;
        }
        power >>= 1;
        if (!power.IsZero) {
          thisVar *= (EInteger)thisVar;
        }
      }
      return r;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Remainder(PeterO.Numbers.EInteger)"]/*'/>
    public EInteger Remainder(EInteger divisor) {
      if (divisor == null) {
        throw new ArgumentNullException(nameof(divisor));
      }
      int words1Size = this.wordCount;
      int words2Size = divisor.wordCount;
      if (words2Size == 0) {
        throw new DivideByZeroException();
      }
      if (words1Size < words2Size) {
        // dividend is less than divisor
        return this;
      }
      if (words2Size == 1) {
        short shortRemainder = FastRemainder(
          this.words,
          this.wordCount,
          divisor.words[0]);
        int smallRemainder = ((int)shortRemainder) & 0xffff;
        if (this.negative) {
          smallRemainder = -smallRemainder;
        }
        return EInteger.FromInt64(smallRemainder);
      }
      if (this.PositiveCompare(divisor) < 0) {
        return this;
      }
      var remainderReg = new short[((int)words2Size)];
      GeneralDivide(
  this.words,
  0,
  this.wordCount,
  divisor.words,
  0,
  divisor.wordCount,
  null,
  0,
  remainderReg,
  0);
      int count = CountWords(remainderReg);
      if (count == 0) {
        return EInteger.Zero;
      }
      remainderReg = ShortenArray(remainderReg, count);
      return new EInteger(count, remainderReg, this.negative);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ShiftLeft(System.Int32)"]/*'/>
    public EInteger ShiftLeft(int numberBits) {
      if (numberBits == 0 || this.wordCount == 0) {
        return this;
      }
      if (numberBits < 0) {
        return (numberBits == Int32.MinValue) ?
          this.ShiftRight(1).ShiftRight(Int32.MaxValue) :
          this.ShiftRight(-numberBits);
      }
      var numWords = (int)this.wordCount;
      var shiftWords = (int)(numberBits >> 4);
      var shiftBits = (int)(numberBits & 15);
      if (!this.negative) {
        var ret = new short[(numWords + BitsToWords((int)numberBits))];
        Array.Copy(this.words, 0, ret, shiftWords, numWords);
        ShiftWordsLeftByBits(
          ret,
          (int)shiftWords,
          numWords + BitsToWords(shiftBits),
          shiftBits);
        return new EInteger(CountWords(ret), ret, false);
      } else {
        var ret = new short[(numWords +
                    BitsToWords((int)numberBits))];
        Array.Copy(this.words, ret, numWords);
        TwosComplement(ret, 0, (int)ret.Length);
        ShiftWordsLeftByWords(ret, 0, numWords + shiftWords, shiftWords);
        ShiftWordsLeftByBits(
          ret,
          (int)shiftWords,
          numWords + BitsToWords(shiftBits),
          shiftBits);
        TwosComplement(ret, 0, (int)ret.Length);
        return new EInteger(CountWords(ret), ret, true);
      }
    }

    private short[] Copy() {
      var words = new short[this.words.Length];
      Array.Copy(this.words, words, this.wordCount);
      return words;
    }

    private static int WordsCompare(
       short[] words,
       int wordCount,
       short[] words2,
       int wordCount2) {
      return WordsCompare(words, 0, wordCount, words2, 0, wordCount2);
    }

    private static int WordsCompare(
       short[] words,
       int pos1,
       int wordCount,
       short[] words2,
       int pos2,
       int wordCount2) {
      // NOTE: Assumes the number is nonnegative
      int size = wordCount;
      if (size == 0) {
        return (wordCount2 == 0) ? 0 : -1;
      } else if (wordCount2 == 0) {
        return 1;
      }
      if (size == wordCount2) {
        if (size == 1 && words[pos1] == words2[pos2]) {
          return 0;
        } else {
          int p1 = pos1 + size - 1;
          int p2 = pos2 + size - 1;
          while (unchecked(size--) != 0) {
            int an = ((int)words[p1]) & 0xffff;
            int bn = ((int)words2[p2]) & 0xffff;
            if (an > bn) {
              return 1;
            }
            if (an < bn) {
              return -1;
            }
            --p1;
            --p2;
          }
          return 0;
        }
      }
      return (size > wordCount2) ? 1 : -1;
    }

    private static long WordsToLongUnchecked(short[] words, int wordCount) {
      // NOTE: Assumes the number is nonnegative
      var c = (int)wordCount;
      if (c == 0) {
        return (long)0;
      }
      long ivv = 0;
      int intRetValue = ((int)words[0]) & 0xffff;
      if (c > 1) {
        intRetValue |= (((int)words[1]) & 0xffff) << 16;
      }
      if (c > 2) {
        int intRetValue2 = ((int)words[2]) & 0xffff;
        if (c > 3) {
          intRetValue2 |= (((int)words[3]) & 0xffff) << 16;
        }
        ivv = ((long)intRetValue) & 0xffffffffL;
        ivv |= ((long)intRetValue2) << 32;
        return ivv;
      }
      ivv = ((long)intRetValue) & 0xffffffffL;
      return ivv;
    }

    private static bool WordsEqual(
       short[] words,
       int wordCount,
       short[] words2,
       int wordCount2) {
      // NOTE: Assumes the number is nonnegative
      if (wordCount == wordCount2) {
        for (var i = 0; i < wordCount; ++i) {
          if (words[i] != words2[i]) {
            return false;
          }
        }
        return true;
      }
      return false;
    }

    private static bool WordsIsEven(short[] words, int wordCount) {
      return wordCount == 0 || (words[0] & 0x01) == 0;
    }

    private static int WordsShiftRightTwo(short[] words, int wordCount) {
      // NOTE: Assumes the number is nonnegative
      if (wordCount != 0) {
        int u;
        var carry = 0;
        for (int i = wordCount - 1; i >= 0; --i) {
          int w = words[i];
          u = ((w & 0xfffc) >> 2) | carry;
          carry = (w << 14) & 0xc000;
          words[i] = unchecked((short)u);
        }
        if (words[wordCount - 1] == 0) {
          --wordCount;
        }
      }
      return wordCount;
    }

    private static int WordsShiftRightEight(short[] words, int wordCount) {
      // NOTE: Assumes the number is nonnegative
      if (wordCount != 0) {
        int u;
        var carry = 0;
        for (int i = wordCount - 1; i >= 0; --i) {
          int w = words[i];
          u = ((w & 0xff00) >> 8) | carry;
          carry = (w << 8) & 0xff00;
          words[i] = unchecked((short)u);
        }
        if (words[wordCount - 1] == 0) {
          --wordCount;
        }
      }
      return wordCount;
    }

    private static int WordsShiftRightFour(short[] words, int wordCount) {
      // NOTE: Assumes the number is nonnegative
      if (wordCount != 0) {
        int u;
        var carry = 0;
        for (int i = wordCount - 1; i >= 0; --i) {
          int w = words[i];
          u = ((w & 0xfff0) >> 4) | carry;
          carry = (w << 12) & 0xf000;
          words[i] = unchecked((short)u);
        }
        if (words[wordCount - 1] == 0) {
          --wordCount;
        }
      }
      return wordCount;
    }

    private static int WordsShiftRightOne(short[] words, int wordCount) {
      // NOTE: Assumes the number is nonnegative
      if (wordCount != 0) {
        int u;
        var carry = 0;
        for (int i = wordCount - 1; i >= 0; --i) {
          int w = words[i];
          u = ((w & 0xfffe) >> 1) | carry;
          carry = (w << 15) & 0x8000;
          words[i] = unchecked((short)u);
        }
        if (words[wordCount - 1] == 0) {
          --wordCount;
        }
      }
      return wordCount;
    }

    private static int WordsSubtract(
  short[] words,
  int wordCount,
  short[] subtrahendWords,
  int subtrahendCount) {
      // NOTE: Assumes this value is at least as high as the subtrahend
      // and both numbers are nonnegative
      var borrow = (short)SubtractInternal(
           words,
           0,
           words,
           0,
           subtrahendWords,
           0,
           subtrahendCount);
      if (borrow != 0) {
        Decrement(
 words,
 subtrahendCount,
 (int)(wordCount - subtrahendCount),
 borrow);
      }
      while (wordCount != 0 && words[wordCount - 1] == 0) {
        --wordCount;
      }
      return wordCount;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ShiftRight(System.Int32)"]/*'/>
    public EInteger ShiftRight(int numberBits) {
      if (numberBits == 0 || this.wordCount == 0) {
        return this;
      }
      if (numberBits < 0) {
        return (numberBits == Int32.MinValue) ?
          this.ShiftLeft(1).ShiftLeft(Int32.MaxValue) :
          this.ShiftLeft(-numberBits);
      }
      var numWords = (int)this.wordCount;
      var shiftWords = (int)(numberBits >> 4);
      var shiftBits = (int)(numberBits & 15);
      short[] ret;
      int retWordCount;
      if (this.negative) {
        ret = new short[this.words.Length];
        Array.Copy(this.words, ret, numWords);
        TwosComplement(ret, 0, (int)ret.Length);
        ShiftWordsRightByWordsSignExtend(ret, 0, numWords, shiftWords);
        if (numWords > shiftWords) {
          ShiftWordsRightByBitsSignExtend(
            ret,
            0,
            numWords - shiftWords,
            shiftBits);
        }
        TwosComplement(ret, 0, (int)ret.Length);
        retWordCount = ret.Length;
      } else {
        if (shiftWords >= numWords) {
          return EInteger.Zero;
        }
        ret = new short[this.words.Length];
        Array.Copy(this.words, shiftWords, ret, 0, numWords - shiftWords);
        if (shiftBits != 0) {
          ShiftWordsRightByBits(ret, 0, numWords - shiftWords, shiftBits);
        }
        retWordCount = numWords - shiftWords;
      }
      while (retWordCount != 0 &&
             ret[retWordCount - 1] == 0) {
        --retWordCount;
      }
      if (retWordCount == 0) {
        return EInteger.Zero;
      }
      if (shiftWords > 2) {
        ret = ShortenArray(ret, retWordCount);
      }
      return new EInteger(retWordCount, ret, this.negative);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Sqrt"]/*'/>
    public EInteger Sqrt() {
      EInteger[] srrem = this.SqrtRemInternal(false);
      return srrem[0];
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.SqrtRem"]/*'/>
    public EInteger[] SqrtRem() {
      return this.SqrtRemInternal(true);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.Subtract(PeterO.Numbers.EInteger)"]/*'/>
    public EInteger Subtract(EInteger subtrahend) {
      if (subtrahend == null) {
        throw new ArgumentNullException(nameof(subtrahend));
      }
      return (this.wordCount == 0) ? subtrahend.Negate() :
        ((subtrahend.wordCount == 0) ? this : this.Add(subtrahend.Negate()));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ToBytes(System.Boolean)"]/*'/>
    public byte[] ToBytes(bool littleEndian) {
      int sign = this.Sign;
      if (sign == 0) {
        return new[] { (byte)0 };
      }
      if (sign > 0) {
        int byteCount = this.ByteCount();
        int byteArrayLength = byteCount;
        if (this.GetUnsignedBit((byteCount * 8) - 1)) {
          ++byteArrayLength;
        }
        var bytes = new byte[byteArrayLength];
        var j = 0;
        for (var i = 0; i < byteCount; i += 2, j++) {
          int index = littleEndian ? i : bytes.Length - 1 - i;
          int index2 = littleEndian ? i + 1 : bytes.Length - 2 - i;
          bytes[index] = (byte)(this.words[j] & 0xff);
          if (index2 >= 0 && index2 < byteArrayLength) {
            bytes[index2] = (byte)((this.words[j] >> 8) & 0xff);
          }
        }
        return bytes;
      } else {
        var regdata = new short[this.words.Length];
        Array.Copy(this.words, regdata, this.words.Length);
        TwosComplement(regdata, 0, (int)regdata.Length);
        int byteCount = regdata.Length * 2;
        for (int i = regdata.Length - 1; i >= 0; --i) {
          if (regdata[i] == unchecked((short)0xffff)) {
            byteCount -= 2;
          } else if ((regdata[i] & 0xff80) == 0xff80) {
            // signed first byte, 0xff second
            --byteCount;
            break;
          } else if ((regdata[i] & 0x8000) == 0x8000) {
            // signed second byte
            break;
          } else {
            // unsigned second byte
            ++byteCount;
            break;
          }
        }
        if (byteCount == 0) {
          byteCount = 1;
        }
        var bytes = new byte[byteCount];
        bytes[littleEndian ? bytes.Length - 1 : 0] = (byte)0xff;
        byteCount = Math.Min(byteCount, regdata.Length * 2);
        var j = 0;
        for (var i = 0; i < byteCount; i += 2, j++) {
          int index = littleEndian ? i : bytes.Length - 1 - i;
          int index2 = littleEndian ? i + 1 : bytes.Length - 2 - i;
          bytes[index] = (byte)(regdata[j] & 0xff);
          if (index2 >= 0 && index2 < byteCount) {
            bytes[index2] = (byte)((regdata[j] >> 8) & 0xff);
          }
        }
        return bytes;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ToInt32Checked"]/*'/>
    public int ToInt32Checked() {
      int count = this.wordCount;
      if (count == 0) {
        return 0;
      }
      if (count > 2) {
        throw new OverflowException();
      }
      if (count == 2 && (this.words[1] & 0x8000) != 0) {
        if (this.negative && this.words[1] == unchecked((short)0x8000) &&
            this.words[0] == 0) {
          return Int32.MinValue;
        }
        throw new OverflowException();
      }
      return this.ToInt32Unchecked();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ToInt32Unchecked"]/*'/>
    public int ToInt32Unchecked() {
      var c = (int)this.wordCount;
      if (c == 0) {
        return 0;
      }
      int intRetValue = ((int)this.words[0]) & 0xffff;
      if (c > 1) {
        intRetValue |= (((int)this.words[1]) & 0xffff) << 16;
      }
      if (this.negative) {
        intRetValue = unchecked(intRetValue - 1);
        intRetValue = unchecked(~intRetValue);
      }
      return intRetValue;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ToInt64Checked"]/*'/>
    public long ToInt64Checked() {
      int count = this.wordCount;
      if (count == 0) {
        return (long)0;
      }
      if (count > 4) {
        throw new OverflowException();
      }
      if (count == 4 && (this.words[3] & 0x8000) != 0) {
        if (this.negative && this.words[3] == unchecked((short)0x8000) &&
            this.words[2] == 0 && this.words[1] == 0 &&
            this.words[0] == 0) {
          return Int64.MinValue;
        }
        throw new OverflowException();
      }
      return this.ToInt64Unchecked();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ToInt64Unchecked"]/*'/>
    public long ToInt64Unchecked() {
      var c = (int)this.wordCount;
      if (c == 0) {
        return (long)0;
      }
      long ivv = 0;
      int intRetValue = ((int)this.words[0]) & 0xffff;
      if (c > 1) {
        intRetValue |= (((int)this.words[1]) & 0xffff) << 16;
      }
      if (c > 2) {
        int intRetValue2 = ((int)this.words[2]) & 0xffff;
        if (c > 3) {
          intRetValue2 |= (((int)this.words[3]) & 0xffff) << 16;
        }
        if (this.negative) {
          if (intRetValue == 0) {
            intRetValue = unchecked(intRetValue - 1);
            intRetValue2 = unchecked(intRetValue2 - 1);
          } else {
            intRetValue = unchecked(intRetValue - 1);
          }
          intRetValue = unchecked(~intRetValue);
          intRetValue2 = unchecked(~intRetValue2);
        }
        ivv = ((long)intRetValue) & 0xffffffffL;
        ivv |= ((long)intRetValue2) << 32;
        return ivv;
      }
      ivv = ((long)intRetValue) & 0xffffffffL;
      if (this.negative) {
        ivv = -ivv;
      }
      return ivv;
    }

    private void ToRadixStringDecimal(
  StringBuilder outputSB,
  bool optimize) {
#if DEBUG
      if (!(!this.negative)) {
        throw new ArgumentException("doesn't satisfy !this.negative");
      }
#endif

      var i = 0;
      if (this.wordCount >= 100 && optimize) {
        var rightBuilder = new StringBuilder();
        int digits = this.wordCount * 3;
        EInteger pow = NumberUtility.FindPowerOfTen(digits);
        // DebugUtility.Log("---divrem " + (this.wordCount));
        EInteger[] divrem = this.DivRem(pow);
        // DebugUtility.Log("" + (divrem[0].GetUnsignedBitLength()) + "," +
        // (// divrem[1].GetUnsignedBitLength()));
        divrem[0].ToRadixStringDecimal(outputSB, optimize);
        divrem[1].ToRadixStringDecimal(rightBuilder, optimize);
        for (i = rightBuilder.Length; i < digits; ++i) {
          outputSB.Append('0');
        }
        outputSB.Append(rightBuilder.ToString());
        return;
      }
      if (this.HasSmallValue()) {
        outputSB.Append(this.SmallValueToString());
        return;
      }
      var tempReg = new short[this.wordCount];
      Array.Copy(this.words, tempReg, tempReg.Length);
      int numWordCount = tempReg.Length;
      while (numWordCount != 0 && tempReg[numWordCount - 1] == 0) {
        --numWordCount;
      }
      var s = new char[(numWordCount << 4) + 1];
      while (numWordCount != 0) {
        if (numWordCount == 1 && tempReg[0] > 0 && tempReg[0] <= 0x7fff) {
          int rest = tempReg[0];
          while (rest != 0) {
            // accurate approximation to rest/10 up to 43698,
            // and rest can go up to 32767
            int newrest = (rest * 26215) >> 18;
            s[i++] = Digits[rest - (newrest * 10)];
            rest = newrest;
          }
          break;
        }
        if (numWordCount == 2 && tempReg[1] > 0 && tempReg[1] <= 0x7fff) {
          int rest = ((int)tempReg[0]) & 0xffff;
          rest |= (((int)tempReg[1]) & 0xffff) << 16;
          while (rest != 0) {
            int newrest = (rest < 43698) ? ((rest * 26215) >> 18) : (rest /
                10);
            s[i++] = Digits[rest - (newrest * 10)];
            rest = newrest;
          }
          break;
        } else {
          int wci = numWordCount;
          short remainderShort = 0;
          int quo, rem;
          // Divide by 10000
          while ((wci--) > 0) {
            int currentDividend = unchecked((int)((((int)tempReg[wci]) &
                  0xffff) | ((int)remainderShort << 16)));
            quo = currentDividend / 10000;
            tempReg[wci] = unchecked((short)quo);
            rem = currentDividend - (10000 * quo);
            remainderShort = unchecked((short)rem);
          }
          int remainderSmall = remainderShort;
          // Recalculate word count
          while (numWordCount != 0 && tempReg[numWordCount - 1] == 0) {
            --numWordCount;
          }
          // accurate approximation to rest/10 up to 16388,
          // and rest can go up to 9999
          int newrest = (remainderSmall * 3277) >> 15;
          s[i++] = Digits[(int)(remainderSmall - (newrest * 10))];
          remainderSmall = newrest;
          newrest = (remainderSmall * 3277) >> 15;
          s[i++] = Digits[(int)(remainderSmall - (newrest * 10))];
          remainderSmall = newrest;
          newrest = (remainderSmall * 3277) >> 15;
          s[i++] = Digits[(int)(remainderSmall - (newrest * 10))];
          remainderSmall = newrest;
          s[i++] = Digits[remainderSmall];
        }
      }
      ReverseChars(s, 0, i);
      outputSB.Append(s, 0, i);
    }

    private string ToUnoptString() {
        if (this.HasSmallValue()) {
          return this.SmallValueToString();
        }
            var sb = new StringBuilder();
            if (this.negative) {
                sb.Append('-');
            }
            this.Abs().ToRadixStringDecimal(sb, false);
            return sb.ToString();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ToRadixString(System.Int32)"]/*'/>
    public string ToRadixString(int radix) {
      if (radix < 2) {
        throw new ArgumentException("radix (" + radix +
                    ") is less than 2");
      }
      if (radix > 36) {
        throw new ArgumentException("radix (" + radix +
                    ") is more than 36");
      }
      if (this.wordCount == 0) {
        return "0";
      }
      if (radix == 10) {
        // Decimal
        if (this.HasSmallValue()) {
          return this.SmallValueToString();
        }
        var sb = new StringBuilder();
        if (this.negative) {
          sb.Append('-');
        }
        this.Abs().ToRadixStringDecimal(sb, true);
        return sb.ToString();
      }
      if (radix == 16) {
        // Hex
        var sb = new System.Text.StringBuilder();
        if (this.negative) {
          sb.Append('-');
        }
        var firstBit = true;
        int word = this.words[this.wordCount - 1];
        for (int i = 0; i < 4; ++i) {
          if (!firstBit || (word & 0xf000) != 0) {
            sb.Append(Digits[(word >> 12) & 0x0f]);
            firstBit = false;
          }
          word <<= 4;
        }
        for (int j = this.wordCount - 2; j >= 0; --j) {
          word = this.words[j];
          for (int i = 0; i < 4; ++i) {
            sb.Append(Digits[(word >> 12) & 0x0f]);
            word <<= 4;
          }
        }
        return sb.ToString();
      }
      if (radix == 2) {
        // Binary
        var sb = new System.Text.StringBuilder();
        if (this.negative) {
          sb.Append('-');
        }
        var firstBit = true;
        int word = this.words[this.wordCount - 1];
        for (int i = 0; i < 16; ++i) {
          if (!firstBit || (word & 0x8000) != 0) {
            sb.Append((word & 0x8000) == 0 ? '0' : '1');
            firstBit = false;
          }
          word <<= 1;
        }
        for (int j = this.wordCount - 2; j >= 0; --j) {
          word = this.words[j];
          for (int i = 0; i < 16; ++i) {
            sb.Append((word & 0x8000) == 0 ? '0' : '1');
            word <<= 1;
          }
        }
        return sb.ToString();
      } else {
        // Other radixes
        var tempReg = new short[this.wordCount];
        Array.Copy(this.words, tempReg, tempReg.Length);
        int numWordCount = tempReg.Length;
        while (numWordCount != 0 && tempReg[numWordCount - 1] == 0) {
          --numWordCount;
        }
        var i = 0;
        var s = new char[(numWordCount << 4) + 1];
        while (numWordCount != 0) {
          if (numWordCount == 1 && tempReg[0] > 0 && tempReg[0] <= 0x7fff) {
            int rest = tempReg[0];
            while (rest != 0) {
              int newrest = rest / radix;
              s[i++] = Digits[rest - (newrest * radix)];
              rest = newrest;
            }
            break;
          }
          if (numWordCount == 2 && tempReg[1] > 0 && tempReg[1] <= 0x7fff) {
            int rest = ((int)tempReg[0]) & 0xffff;
            rest |= (((int)tempReg[1]) & 0xffff) << 16;
            while (rest != 0) {
              int newrest = rest / radix;
              s[i++] = Digits[rest - (newrest * radix)];
              rest = newrest;
            }
            break;
          } else {
            int wci = numWordCount;
            short remainderShort = 0;
            int quo, rem;
            // Divide by radix
            while ((wci--) > 0) {
              int currentDividend = unchecked((int)((((int)tempReg[wci]) &
                    0xffff) | ((int)remainderShort << 16)));
              quo = currentDividend / radix;
              tempReg[wci] = unchecked((short)quo);
              rem = currentDividend - (radix * quo);
              remainderShort = unchecked((short)rem);
            }
            int remainderSmall = remainderShort;
            // Recalculate word count
            while (numWordCount != 0 && tempReg[numWordCount - 1] == 0) {
              --numWordCount;
            }
            s[i++] = Digits[remainderSmall];
          }
        }
        ReverseChars(s, 0, i);
        if (this.negative) {
          var sb = new System.Text.StringBuilder(i + 1);
          sb.Append('-');
          for (var j = 0; j < i; ++j) {
            sb.Append(s[j]);
          }
          return sb.ToString();
        }
        return new String(s, 0, i);
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ToString"]/*'/>
    public override string ToString() {
      if (this.IsZero) {
        return "0";
      }
      return this.HasSmallValue() ? this.SmallValueToString() :
        this.ToRadixString(10);
    }

    private static int AddInternal(
      short[] c,
      int cstart,
      short[] words1,
      int astart,
      short[] words2,
      int bstart,
      int n) {
      unchecked {
        int u;
        u = 0;
        bool evn = (n & 1) == 0;
        int valueNEven = evn ? n : n - 1;
        var i = 0;
        while (i < valueNEven) {
          u = (((int)words1[astart + i]) & 0xffff) +
            (((int)words2[bstart + i]) & 0xffff) + (short)(u >> 16);
          c[cstart + i] = (short)u;
          ++i;
          u = (((int)words1[astart + i]) & 0xffff) +
            (((int)words2[bstart + i]) & 0xffff) + (short)(u >> 16);
          c[cstart + i] = (short)u;
          ++i;
        }
        if (!evn) {
          u = (((int)words1[astart + valueNEven]) & 0xffff) +
            (((int)words2[bstart + valueNEven]) & 0xffff) + (short)(u >> 16);
          c[cstart + valueNEven] = (short)u;
        }
        return ((int)u >> 16) & 0xffff;
      }
    }

    private static int AddUnevenSize(
      short[] c,
      int cstart,
      short[] wordsBigger,
      int astart,
      int acount,
      short[] wordsSmaller,
      int bstart,
      int bcount) {
#if DEBUG
      if (acount < bcount) {
        throw new ArgumentException("acount (" + acount + ") is less than " +
                    bcount);
      }
#endif
      unchecked {
        int u;
        u = 0;
        for (var i = 0; i < bcount; i += 1) {
          u = (((int)wordsBigger[astart + i]) & 0xffff) +
            (((int)wordsSmaller[bstart + i]) & 0xffff) + (short)(u >> 16);
          c[cstart + i] = (short)u;
        }
        for (int i = bcount; i < acount; i += 1) {
          u = (((int)wordsBigger[astart + i]) & 0xffff) + (short)(u >> 16);
          c[cstart + i] = (short)u;
        }
        return ((int)u >> 16) & 0xffff;
      }
    }

    private static int ApproxLogTenOfTwo(int bitlen) {
      int bitlenLow = bitlen & 0xffff;
      int bitlenHigh = (bitlen >> 16) & 0xffff;
      short resultLow = 0;
      short resultHigh = 0;
      unchecked {
        int p; short c; int d;
        p = bitlenLow * 0x84fb; d = ((int)p >> 16) & 0xffff; c = (short)d; d
          = ((int)d >> 16) & 0xffff;
        p = bitlenLow * 0x209a;
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff;
        p = bitlenHigh * 0x84fb;
        p += ((int)c) & 0xffff;
        d += ((int)p >> 16) & 0xffff; c = (short)d; d = ((int)d >> 16) & 0xffff;
        p = bitlenLow * 0x9a;
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff;
        p = bitlenHigh * 0x209a;
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff;
        p = ((int)c) & 0xffff; c = (short)p; resultLow = c; c = (short)d; d
          = ((int)d >> 16) & 0xffff;
        p = bitlenHigh * 0x9a;
        p += ((int)c) & 0xffff;
        resultHigh = (short)p;
        int result = ((int)resultLow) & 0xffff;
        result |= (((int)resultHigh) & 0xffff) << 16;
        return (result & 0x7fffffff) >> 9;
      }
    }

    // Multiplies two operands of different sizes
    private static void AsymmetricMultiply(
      short[] resultArr,
      int resultStart,  // uses words1Count + words2Count
      short[] tempArr,
      int tempStart,  // uses words1Count + words2Count
      short[] words1,
      int words1Start,
      int words1Count,
      short[] words2,
      int words2Start,
      int words2Count) {
      // DebugUtility.Log("AsymmetricMultiply " + words1Count + " " +
      // words2Count + " [r=" + resultStart + " t=" + tempStart + " a=" +
      // words1Start + " b=" + words2Start + "]");
#if DEBUG
      if (resultArr == null) {
        throw new ArgumentNullException(nameof(resultArr));
      }

      if (resultStart < 0) {
        throw new ArgumentException("resultStart (" + resultStart +
                    ") is less than 0");
      }

      if (resultStart > resultArr.Length) {
        throw new ArgumentException("resultStart (" + resultStart +
                    ") is more than " + resultArr.Length);
      }

      if (words1Count + words2Count < 0) {
        throw new ArgumentException("words1Count plus words2Count (" +
                    (words1Count + words2Count) + ") is less than " +
                    "0");
      }

      if (words1Count + words2Count > resultArr.Length) {
        throw new ArgumentException("words1Count plus words2Count (" +
                    (words1Count + words2Count) +
                    ") is more than " + resultArr.Length);
      }

      if (resultArr.Length - resultStart < words1Count + words2Count) {
        throw new ArgumentException("resultArr.Length minus resultStart (" +
                    (resultArr.Length - resultStart) +
                    ") is less than " + (words1Count +
                    words2Count));
      }

      if (tempArr == null) {
        throw new ArgumentNullException(nameof(tempArr));
      }

      if (tempStart < 0) {
        throw new ArgumentException("tempStart (" + tempStart +
                    ") is less than 0");
      }

      if (tempStart > tempArr.Length) {
        throw new ArgumentException("tempStart (" + tempStart +
                    ") is more than " + tempArr.Length);
      }

      if (words1Count + words2Count < 0) {
        throw new ArgumentException("words1Count plus words2Count (" +
                    (words1Count + words2Count) + ") is less than " +
                    "0");
      }

      if (words1Count + words2Count > tempArr.Length) {
        throw new ArgumentException("words1Count plus words2Count (" +
                    (words1Count + words2Count) +
                    ") is more than " + tempArr.Length);
      }

      if (tempArr.Length - tempStart < words1Count + words2Count) {
        throw new ArgumentException("tempArr.Length minus tempStart (" +
                    (tempArr.Length - tempStart) +
                    ") is less than " + (words1Count +
                    words2Count));
      }

      if (words1 == null) {
        throw new ArgumentNullException(nameof(words1));
      }

      if (words1Start < 0) {
        throw new ArgumentException("words1Start (" + words1Start +
                    ") is less than 0");
      }

      if (words1Start > words1.Length) {
        throw new ArgumentException("words1Start (" + words1Start +
                    ") is more than " + words1.Length);
      }

      if (words1Count < 0) {
        throw new ArgumentException("words1Count (" + words1Count +
                    ") is less than 0");
      }

      if (words1Count > words1.Length) {
        throw new ArgumentException("words1Count (" + words1Count +
                    ") is more than " + words1.Length);
      }

      if (words1.Length - words1Start < words1Count) {
        throw new ArgumentException("words1.Length minus words1Start (" +
                    (words1.Length - words1Start) +
                    ") is less than " + words1Count);
      }

      if (words2 == null) {
        throw new ArgumentNullException(nameof(words2));
      }

      if (words2Start < 0) {
        throw new ArgumentException("words2Start (" + words2Start +
                    ") is less than 0");
      }

      if (words2Start > words2.Length) {
        throw new ArgumentException("words2Start (" + words2Start +
                    ") is more than " + words2.Length);
      }

      if (words2Count < 0) {
        throw new ArgumentException("words2Count (" + words2Count +
                    ") is less than 0");
      }

      if (words2Count > words2.Length) {
        throw new ArgumentException("words2Count (" + words2Count +
                    ") is more than " + words2.Length);
      }

      if (words2.Length - words2Start < words2Count) {
        throw new ArgumentException("words2.Length minus words2Start (" +
                    (words2.Length - words2Start) +
                    ") is less than " + words2Count);
      }
#endif

      if (words1Count == words2Count) {
        if (words1Start == words2Start && words1 == words2) {
          // Both operands have the same value and the same word count
          RecursiveSquare(
            resultArr,
            resultStart,
            tempArr,
            tempStart,
            words1,
            words1Start,
            words1Count);
        } else if (words1Count == 2) {
          // Both operands have a word count of 2
          BaselineMultiply2(
            resultArr,
            resultStart,
            words1,
            words1Start,
            words2,
            words2Start);
        } else {
          // Other cases where both operands have the same word count
          SameSizeMultiply(
            resultArr,
            resultStart,
            tempArr,
            tempStart,
            words1,
            words1Start,
            words2,
            words2Start,
            words1Count);
        }
        return;
      }
      if (words1Count > words2Count) {
        // Ensure that words1 is smaller by swapping if necessary
        short[] tmp1 = words1;
        words1 = words2;
        words2 = tmp1;
        int tmp3 = words1Start;
        words1Start = words2Start;
        words2Start = tmp3;
        int tmp2 = words1Count;
        words1Count = words2Count;
        words2Count = tmp2;
      }

      if (words1Count == 1 || (words1Count == 2 && words1[words1Start + 1] ==
                    0)) {
        switch (words1[words1Start]) {
          case 0:
            // words1 is ValueZero, so result is 0
            Array.Clear((short[])resultArr, resultStart, words2Count + 2);
            return;
          case 1:
            Array.Copy(
              words2,
              words2Start,
              resultArr,
              resultStart,
              (int)words2Count);
            resultArr[resultStart + words2Count] = (short)0;
            resultArr[resultStart + words2Count + 1] = (short)0;
            return;
          default:
            resultArr[resultStart + words2Count] = LinearMultiply(
              resultArr,
              resultStart,
              words2,
              words2Start,
              words1[words1Start],
              words2Count);
            resultArr[resultStart + words2Count + 1] = (short)0;
            return;
        }
      }
      if (words1Count == 2 && (words2Count & 1) == 0) {
        int a0 = ((int)words1[words1Start]) & 0xffff;
        int a1 = ((int)words1[words1Start + 1]) & 0xffff;
        resultArr[resultStart + words2Count] = (short)0;
        resultArr[resultStart + words2Count + 1] = (short)0;
        AtomicMultiplyOpt(
          resultArr,
          resultStart,
          a0,
          a1,
          words2,
          words2Start,
          0,
          words2Count);
        AtomicMultiplyAddOpt(
          resultArr,
          resultStart,
          a0,
          a1,
          words2,
          words2Start,
          2,
          words2Count);
        return;
      }
      if (words1Count <= 10 && words2Count <= 10) {
        SchoolbookMultiply(
          resultArr,
          resultStart,
          words1,
          words1Start,
          words1Count,
          words2,
          words2Start,
          words2Count);
      } else {
        int wordsRem = words2Count % words1Count;
        int evenmult = (words2Count / words1Count) & 1;
        int i;
        // DebugUtility.Log("counts=" + words1Count + "," + words2Count +
        // " res=" + (resultStart + words1Count) + " temp=" + (tempStart +
        // (words1Count << 1)) + " rem=" + wordsRem + " evenwc=" + evenmult);
        if (wordsRem == 0) {
          // words2Count is divisible by words1count
          if (evenmult == 0) {
            SameSizeMultiply(
              resultArr,
              resultStart,
              tempArr,
              tempStart,
              words1,
              words1Start,
              words2,
              words2Start,
              words1Count);
            Array.Copy(
              resultArr,
              resultStart + words1Count,
              tempArr,
              (int)(tempStart + (words1Count << 1)),
              words1Count);
            for (i = words1Count << 1; i < words2Count; i += words1Count << 1) {
              SameSizeMultiply(
                tempArr,
                tempStart + words1Count + i,
                tempArr,
                tempStart,
                words1,
                words1Start,
                words2,
                words2Start + i,
                words1Count);
            }
            for (i = words1Count; i < words2Count; i += words1Count << 1) {
              SameSizeMultiply(
                resultArr,
                resultStart + i,
                tempArr,
                tempStart,
                words1,
                words1Start,
                words2,
                words2Start + i,
                words1Count);
            }
          } else {
            for (i = 0; i < words2Count; i += words1Count << 1) {
              SameSizeMultiply(
                resultArr,
                resultStart + i,
                tempArr,
                tempStart,
                words1,
                words1Start,
                words2,
                words2Start + i,
                words1Count);
            }
            for (i = words1Count; i < words2Count; i += words1Count << 1) {
              SameSizeMultiply(
                tempArr,
                tempStart + words1Count + i,
                tempArr,
                tempStart,
                words1,
                words1Start,
                words2,
                words2Start + i,
                words1Count);
            }
          }
          if (
            AddInternal(
              resultArr,
              resultStart + words1Count,
              resultArr,
              resultStart + words1Count,
              tempArr,
              tempStart + (words1Count << 1),
              words2Count - words1Count) != 0) {
            Increment(
              resultArr,
              (int)(resultStart + words2Count),
              words1Count,
              (short)1);
          }
        } else if ((words1Count + words2Count) >= (words1Count << 2)) {
          // DebugUtility.Log("Chunked Linear Multiply long");
          ChunkedLinearMultiply(
            resultArr,
            resultStart,
            tempArr,
            tempStart,
            words2,
            words2Start,
            words2Count,
            words1,
            words1Start,
            words1Count);
        } else if (words1Count + 1 == words2Count ||
                   (words1Count + 2 == words2Count && words2[words2Start +
                    words2Count - 1] == 0)) {
          Array.Clear(
            (short[])resultArr,
            resultStart,
            words1Count + words2Count);
          // Multiply the low parts of each operand
          SameSizeMultiply(
            resultArr,
            resultStart,
            tempArr,
            tempStart,
            words1,
            words1Start,
            words2,
            words2Start,
            words1Count);
          // Multiply the high parts
          // while adding carry from the high part of the product
          short carry = LinearMultiplyAdd(
            resultArr,
            resultStart + words1Count,
            words1,
            words1Start,
            words2[words2Start + words1Count],
            words1Count);
          resultArr[resultStart + words1Count + words1Count] = carry;
        } else {
          var t2 = new short[words1Count << 2];
          // DebugUtility.Log("Chunked Linear Multiply Short");
          ChunkedLinearMultiply(
            resultArr,
            resultStart,
            t2,
            0,
            words2,
            words2Start,
            words2Count,
            words1,
            words1Start,
            words1Count);
        }
      }
    }

    private static void AtomicMultiplyAddOpt(
      short[] c,
      int valueCstart,
      int valueA0,
      int valueA1,
      short[] words2,
      int words2Start,
      int istart,
      int iend) {
      short s;
      int d;
      int first1MinusFirst0 = ((int)valueA1 - valueA0) & 0xffff;
      valueA1 &= 0xffff;
      valueA0 &= 0xffff;
      unchecked {
        if (valueA1 >= valueA0) {
          for (int i = istart; i < iend; i += 4) {
            int b0 = ((int)words2[words2Start + i]) & 0xffff;
            int b1 = ((int)words2[words2Start + i + 1]) & 0xffff;
            int csi = valueCstart + i;
            if (b0 >= b1) {
              s = (short)0;
              d = first1MinusFirst0 * (((int)b0 - b1) & 0xffff);
            } else {
              s = (short)first1MinusFirst0;
              d = (((int)s) & 0xffff) * (((int)b0 - b1) & 0xffff);
            }
            int valueA0B0 = valueA0 * b0;
            int a0b0high = (valueA0B0 >> 16) & 0xffff;
            int tempInt;
            tempInt = valueA0B0 + (((int)c[csi]) & 0xffff);
            c[csi] = (short)(((int)tempInt) & 0xffff);

            int valueA1B1 = valueA1 * b1;
            int a1b1low = valueA1B1 & 0xffff;
            int a1b1high = ((int)(valueA1B1 >> 16)) & 0xffff;
            tempInt = (((int)(tempInt >> 16)) & 0xffff) + (((int)valueA0B0) &
                    0xffff) + (((int)d) & 0xffff) + a1b1low +
              (((int)c[csi + 1]) & 0xffff);
            c[csi + 1] = (short)(((int)tempInt) & 0xffff);

            tempInt = (((int)(tempInt >> 16)) & 0xffff) + a1b1low + a0b0high +
              (((int)(d >> 16)) & 0xffff) +
              a1b1high - (((int)s) & 0xffff) + (((int)c[csi + 2]) & 0xffff);
            c[csi + 2] = (short)(((int)tempInt) & 0xffff);

            tempInt = (((int)(tempInt >> 16)) & 0xffff) + a1b1high +
              (((int)c[csi + 3]) & 0xffff);
            c[csi + 3] = (short)(((int)tempInt) & 0xffff);
            if ((tempInt >> 16) != 0) {
              ++c[csi + 4];
              c[csi + 5] += (short)((c[csi + 4] == 0) ? 1 : 0);
            }
          }
        } else {
          for (int i = istart; i < iend; i += 4) {
            int valueB0 = ((int)words2[words2Start + i]) & 0xffff;
            int valueB1 = ((int)words2[words2Start + i + 1]) & 0xffff;
            int csi = valueCstart + i;
            if (valueB0 > valueB1) {
              s = (short)(((int)valueB0 - valueB1) & 0xffff);
              d = first1MinusFirst0 * (((int)s) & 0xffff);
            } else {
              s = (short)0;
              d = (((int)valueA0 - valueA1) & 0xffff) * (((int)valueB1 -
                    valueB0) & 0xffff);
            }
            int valueA0B0 = valueA0 * valueB0;
            int a0b0high = (valueA0B0 >> 16) & 0xffff;
            int tempInt;
            tempInt = valueA0B0 + (((int)c[csi]) & 0xffff);
            c[csi] = (short)(((int)tempInt) & 0xffff);

            int valueA1B1 = valueA1 * valueB1;
            int a1b1low = valueA1B1 & 0xffff;
            int a1b1high = (valueA1B1 >> 16) & 0xffff;
            tempInt = (((int)(tempInt >> 16)) & 0xffff) + (((int)valueA0B0) &
                    0xffff) + (((int)d) & 0xffff) + a1b1low +
              (((int)c[csi + 1]) & 0xffff);
            c[csi + 1] = (short)(((int)tempInt) & 0xffff);

            tempInt = (((int)(tempInt >> 16)) & 0xffff) + a1b1low + a0b0high +
              (((int)(d >> 16)) & 0xffff) +
              a1b1high - (((int)s) & 0xffff) + (((int)c[csi + 2]) & 0xffff);
            c[csi + 2] = (short)(((int)tempInt) & 0xffff);

            tempInt = (((int)(tempInt >> 16)) & 0xffff) + a1b1high +
              (((int)c[csi + 3]) & 0xffff);
            c[csi + 3] = (short)(((int)tempInt) & 0xffff);
            if ((tempInt >> 16) != 0) {
              ++c[csi + 4];
              c[csi + 5] += (short)((c[csi + 4] == 0) ? 1 : 0);
            }
          }
        }
      }
    }

    private static void AtomicMultiplyOpt(
      short[] c,
      int valueCstart,
      int valueA0,
      int valueA1,
      short[] words2,
      int words2Start,
      int istart,
      int iend) {
      short s;
      int d;
      int first1MinusFirst0 = ((int)valueA1 - valueA0) & 0xffff;
      valueA1 &= 0xffff;
      valueA0 &= 0xffff;
      unchecked {
        if (valueA1 >= valueA0) {
          for (int i = istart; i < iend; i += 4) {
            int valueB0 = ((int)words2[words2Start + i]) & 0xffff;
            int valueB1 = ((int)words2[words2Start + i + 1]) & 0xffff;
            int csi = valueCstart + i;
            if (valueB0 >= valueB1) {
              s = (short)0;
              d = first1MinusFirst0 * (((int)valueB0 - valueB1) & 0xffff);
            } else {
              s = (short)first1MinusFirst0;
              d = (((int)s) & 0xffff) * (((int)valueB0 - valueB1) & 0xffff);
            }
            int valueA0B0 = valueA0 * valueB0;
            c[csi] = (short)(((int)valueA0B0) & 0xffff);
            int a0b0high = (valueA0B0 >> 16) & 0xffff;
            int valueA1B1 = valueA1 * valueB1;
            int tempInt;
            tempInt = a0b0high + (((int)valueA0B0) & 0xffff) + (((int)d) &
                    0xffff) + (((int)valueA1B1) & 0xffff);
            c[csi + 1] = (short)tempInt;
            tempInt = valueA1B1 + (((int)(tempInt >> 16)) & 0xffff) +
              a0b0high + (((int)(d >> 16)) & 0xffff) + (((int)(valueA1B1 >>
                    16)) & 0xffff) - (((int)s) & 0xffff);
            c[csi + 2] = (short)tempInt;
            tempInt >>= 16;
            c[csi + 3] = (short)tempInt;
          }
        } else {
          for (int i = istart; i < iend; i += 4) {
            int valueB0 = ((int)words2[words2Start + i]) & 0xffff;
            int valueB1 = ((int)words2[words2Start + i + 1]) & 0xffff;
            int csi = valueCstart + i;
            if (valueB0 > valueB1) {
              s = (short)(((int)valueB0 - valueB1) & 0xffff);
              d = first1MinusFirst0 * (((int)s) & 0xffff);
            } else {
              s = (short)0;
              d = (((int)valueA0 - valueA1) & 0xffff) * (((int)valueB1 -
                    valueB0) & 0xffff);
            }
            int valueA0B0 = valueA0 * valueB0;
            int a0b0high = (valueA0B0 >> 16) & 0xffff;
            c[csi] = (short)(((int)valueA0B0) & 0xffff);

            int valueA1B1 = valueA1 * valueB1;
            int tempInt;
            tempInt = a0b0high + (((int)valueA0B0) & 0xffff) + (((int)d) &
                    0xffff) + (((int)valueA1B1) & 0xffff);
            c[csi + 1] = (short)tempInt;

            tempInt = valueA1B1 + (((int)(tempInt >> 16)) & 0xffff) +
              a0b0high + (((int)(d >> 16)) & 0xffff) + (((int)(valueA1B1 >>
                    16)) & 0xffff) - (((int)s) & 0xffff);

            c[csi + 2] = (short)tempInt;
            tempInt >>= 16;
            c[csi + 3] = (short)tempInt;
          }
        }
      }
    }
    //---------------------
    // Baseline multiply
    //---------------------
    private static void BaselineMultiply2(
      short[] result,
      int rstart,
      short[] words1,
      int astart,
      short[] words2,
      int bstart) {
      unchecked {
        int p; short c; int d;
        int a0 = ((int)words1[astart]) & 0xffff;
        int a1 = ((int)words1[astart + 1]) & 0xffff;
        int b0 = ((int)words2[bstart]) & 0xffff;
        int b1 = ((int)words2[bstart + 1]) & 0xffff;
        p = a0 * b0; c = (short)p; d = ((int)p >> 16) & 0xffff;
        result[rstart] = c; c = (short)d; d = ((int)d >> 16) & 0xffff;
        p = a0 * b1;
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff;
        p = a1 * b0;
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff; result[rstart + 1] = c;
        p = a1 * b1;
        p += d; result[rstart + 2] = (short)p; result[rstart + 3] = (short)(p >>
                    16);
      }
    }

    private static void BaselineMultiply4(
      short[] result,
      int rstart,
      short[] words1,
      int astart,
      short[] words2,
      int bstart) {
      unchecked {
        const int SMask = ShortMask;
        int p; short c; int d;
        int a0 = ((int)words1[astart]) & SMask;
        int b0 = ((int)words2[bstart]) & SMask;
        p = a0 * b0; c = (short)p; d = ((int)p >> 16) & SMask;
        result[rstart] = c; c = (short)d; d = ((int)d >> 16) & SMask;
        p = a0 * (((int)words2[bstart + 1]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 1]) & SMask) * b0;
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask; result[rstart + 1] = c; c =
          (short)d; d = ((int)d >> 16) & SMask;
        p = a0 * (((int)words2[bstart + 2]) & SMask);

        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 1]) & SMask) * (((int)words2[bstart +
                    1]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 2]) & SMask) * b0;
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask; result[rstart + 2] = c; c =
          (short)d; d = ((int)d >> 16) & SMask;
        p = a0 * (((int)words2[bstart + 3]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 1]) & SMask) * (((int)words2[bstart +
                    2]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;

        p = (((int)words1[astart + 2]) & SMask) * (((int)words2[bstart +
                    1]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 3]) & SMask) * b0;
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask; result[rstart + 3] = c; c =
          (short)d; d = ((int)d >> 16) & SMask;
        p = (((int)words1[astart + 1]) & SMask) * (((int)words2[bstart +
                    3]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 2]) & SMask) * (((int)words2[bstart +
                    2]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 3]) & SMask) * (((int)words2[bstart +
                    1]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask; result[rstart + 4] = c; c =
          (short)d; d = ((int)d >> 16) & SMask;
        p = (((int)words1[astart + 2]) & SMask) * (((int)words2[bstart +
                    3]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 3]) & SMask) * (((int)words2[bstart +
                    2]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask; result[rstart + 5] = c;
        p = (((int)words1[astart + 3]) & SMask) * (((int)words2[bstart +
                    3]) & SMask);
        p += d; result[rstart + 6] = (short)p; result[rstart + 7] = (short)(p >>
                    16);
      }
    }

    private static void BaselineMultiply8(
      short[] result,
      int rstart,
      short[] words1,
      int astart,
      short[] words2,
      int bstart) {
      unchecked {
        int p; short c; int d;
        const int SMask = ShortMask;
        p = (((int)words1[astart]) & SMask) * (((int)words2[bstart]) &
                    SMask); c = (short)p; d = ((int)p >> 16) &
          SMask;
        result[rstart] = c; c = (short)d; d = ((int)d >> 16) & SMask;
        p = (((int)words1[astart]) & SMask) * (((int)words2[bstart + 1]) &
                    SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 1]) & SMask) * (((int)words2[bstart]) &
                    SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask; result[rstart + 1] = c; c =
          (short)d; d = ((int)d >> 16) & SMask;
        p = (((int)words1[astart]) & SMask) * (((int)words2[bstart + 2]) &
                    SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 1]) & SMask) * (((int)words2[bstart +
                    1]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 2]) & SMask) * (((int)words2[bstart]) &
                    SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask; result[rstart + 2] = c; c =
          (short)d; d = ((int)d >> 16) & SMask;
        p = (((int)words1[astart]) & SMask) * (((int)words2[bstart + 3]) &
                    SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 1]) & SMask) * (((int)words2[bstart +
                    2]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 2]) & SMask) * (((int)words2[bstart +
                    1]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 3]) & SMask) * (((int)words2[bstart]) &
                    SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask; result[rstart + 3] = c; c =
          (short)d; d = ((int)d >> 16) & SMask;
        p = (((int)words1[astart]) & SMask) * (((int)words2[bstart + 4]) &
                    SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 1]) & SMask) * (((int)words2[bstart +
                    3]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 2]) & SMask) * (((int)words2[bstart +
                    2]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 3]) & SMask) * (((int)words2[bstart +
                    1]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 4]) & SMask) * (((int)words2[bstart]) &
                    SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask; result[rstart + 4] = c; c =
          (short)d; d = ((int)d >> 16) & SMask;
        p = (((int)words1[astart]) & SMask) * (((int)words2[bstart + 5]) &
                    SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 1]) & SMask) * (((int)words2[bstart +
                    4]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 2]) & SMask) * (((int)words2[bstart +
                    3]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 3]) & SMask) * (((int)words2[bstart +
                    2]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 4]) & SMask) * (((int)words2[bstart +
                    1]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 5]) & SMask) * (((int)words2[bstart]) &
                    SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask; result[rstart + 5] = c; c =
          (short)d; d = ((int)d >> 16) & SMask;
        p = (((int)words1[astart]) & SMask) * (((int)words2[bstart + 6]) &
                    SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 1]) & SMask) * (((int)words2[bstart +
                    5]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 2]) & SMask) * (((int)words2[bstart +
                    4]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 3]) & SMask) * (((int)words2[bstart +
                    3]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 4]) & SMask) * (((int)words2[bstart +
                    2]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 5]) & SMask) * (((int)words2[bstart +
                    1]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 6]) & SMask) * (((int)words2[bstart]) &
                    SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask; result[rstart + 6] = c; c =
          (short)d; d = ((int)d >> 16) & SMask;
        p = (((int)words1[astart]) & SMask) * (((int)words2[bstart + 7]) &
                    SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 1]) & SMask) * (((int)words2[bstart +
                    6]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 2]) & SMask) * (((int)words2[bstart +
                    5]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 3]) & SMask) * (((int)words2[bstart +
                    4]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 4]) & SMask) * (((int)words2[bstart +
                    3]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 5]) & SMask) * (((int)words2[bstart +
                    2]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 6]) & SMask) * (((int)words2[bstart +
                    1]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 7]) & SMask) * (((int)words2[bstart]) &
                    SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask; result[rstart + 7] = c; c =
          (short)d; d = ((int)d >> 16) & SMask;
        p = (((int)words1[astart + 1]) & SMask) * (((int)words2[bstart +
                    7]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 2]) & SMask) * (((int)words2[bstart +
                    6]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 3]) & SMask) * (((int)words2[bstart +
                    5]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 4]) & SMask) * (((int)words2[bstart +
                    4]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 5]) & SMask) * (((int)words2[bstart +
                    3]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 6]) & SMask) * (((int)words2[bstart +
                    2]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 7]) & SMask) * (((int)words2[bstart +
                    1]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask; result[rstart + 8] = c; c =
          (short)d; d = ((int)d >> 16) & SMask;
        p = (((int)words1[astart + 2]) & SMask) * (((int)words2[bstart +
                    7]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 3]) & SMask) * (((int)words2[bstart +
                    6]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 4]) & SMask) * (((int)words2[bstart +
                    5]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 5]) & SMask) * (((int)words2[bstart +
                    4]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 6]) & SMask) * (((int)words2[bstart +
                    3]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 7]) & SMask) * (((int)words2[bstart +
                    2]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask; result[rstart + 9] = c; c =
          (short)d; d = ((int)d >> 16) & SMask;
        p = (((int)words1[astart + 3]) & SMask) * (((int)words2[bstart +
                    7]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 4]) & SMask) * (((int)words2[bstart +
                    6]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 5]) & SMask) * (((int)words2[bstart +
                    5]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 6]) & SMask) * (((int)words2[bstart +
                    4]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 7]) & SMask) * (((int)words2[bstart +
                    3]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask; result[rstart + 10] = c; c =
          (short)d; d = ((int)d >> 16) & SMask;
        p = (((int)words1[astart + 4]) & SMask) * (((int)words2[bstart +
                    7]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 5]) & SMask) * (((int)words2[bstart +
                    6]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 6]) & SMask) * (((int)words2[bstart +
                    5]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 7]) & SMask) * (((int)words2[bstart +
                    4]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask; result[rstart + 11] = c; c =
          (short)d; d = ((int)d >> 16) & SMask;
        p = (((int)words1[astart + 5]) & SMask) * (((int)words2[bstart +
                    7]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 6]) & SMask) * (((int)words2[bstart +
                    6]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 7]) & SMask) * (((int)words2[bstart +
                    5]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask; result[rstart + 12] = c; c =
          (short)d; d = ((int)d >> 16) & SMask;
        p = (((int)words1[astart + 6]) & SMask) * (((int)words2[bstart +
                    7]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask;
        p = (((int)words1[astart + 7]) & SMask) * (((int)words2[bstart +
                    6]) & SMask);
        p += ((int)c) & SMask; c = (short)p;
        d += ((int)p >> 16) & SMask; result[rstart + 13] = c;
        p = (((int)words1[astart + 7]) & SMask) * (((int)words2[bstart +
                    7]) & SMask);
        p += d; result[rstart + 14] = (short)p; result[rstart + 15] =
          (short)(p >> 16);
      }
    }
    //-----------------------------
    // Baseline Square
    //-----------------------------
    private static void BaselineSquare2(
      short[] result,
      int rstart,
      short[] words1,
      int astart) {
      unchecked {
        int p; short c; int d; int e;
        p = (((int)words1[astart]) & 0xffff) * (((int)words1[astart]) &
                    0xffff); result[rstart] = (short)p; e = ((int)p >>
                    16) & 0xffff;
        p = (((int)words1[astart]) & 0xffff) * (((int)words1[astart + 1]) &
                    0xffff); c = (short)p; d = ((int)p >> 16) &
          0xffff;
        d = (int)((d << 1) + (((int)c >> 15) & 1)); c <<=
          1;
        e += ((int)c) & 0xffff; c = (short)e; e = d + (((int)e >> 16) &
                    0xffff); result[rstart + 1] = c;
        p = (((int)words1[astart + 1]) & 0xffff) * (((int)words1[astart +
                    1]) & 0xffff);
        p += e; result[rstart + 2] = (short)p; result[rstart + 3] = (short)(p >>
                    16);
      }
    }

    private static void BaselineSquare4(
      short[] result,
      int rstart,
      short[] words1,
      int astart) {
      unchecked {
        int p; short c; int d; int e;
        p = (((int)words1[astart]) & 0xffff) * (((int)words1[astart]) &
                    0xffff); result[rstart] = (short)p; e = ((int)p >>
                    16) & 0xffff;
        p = (((int)words1[astart]) & 0xffff) * (((int)words1[astart + 1]) &
                    0xffff); c = (short)p; d = ((int)p >> 16) &
          0xffff;
        d = (int)((d << 1) + (((int)c >> 15) & 1)); c <<=
          1;
        e += ((int)c) & 0xffff; c = (short)e; e = d + (((int)e >> 16) &
                    0xffff); result[rstart + 1] = c;
        p = (((int)words1[astart]) & 0xffff) * (((int)words1[astart + 2]) &
                    0xffff); c = (short)p; d = ((int)p >> 16) &
          0xffff;
        d = (int)((d << 1) + (((int)c >> 15) & 1)); c <<=
          1;
        p = (((int)words1[astart + 1]) & 0xffff) * (((int)words1[astart +
                    1]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff;
        e += ((int)c) & 0xffff; c = (short)e; e = d + (((int)e >> 16) &
                    0xffff); result[rstart + 2] = c;
        p = (((int)words1[astart]) & 0xffff) * (((int)words1[astart + 3]) &
                    0xffff); c = (short)p; d = ((int)p >> 16) &
          0xffff;
        p = (((int)words1[astart + 1]) & 0xffff) * (((int)words1[astart +
                    2]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff; d = (int)((d << 1) + (((int)c >> 15) &
                    1)); c <<= 1;
        e += ((int)c) & 0xffff; c = (short)e; e = d + (((int)e >> 16) &
                    0xffff); result[rstart + 3] = c;
        p = (((int)words1[astart + 1]) & 0xffff) * (((int)words1[astart +
                    3]) & 0xffff); c = (short)p; d = ((int)p >>
                    16) & 0xffff;
        d = (int)((d << 1) + (((int)c >> 15) & 1)); c <<= 1;
        p = (((int)words1[astart + 2]) & 0xffff) * (((int)words1[astart +
                    2]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff;
        e += ((int)c) & 0xffff; c = (short)e; e = d + (((int)e >> 16) &
                    0xffff); result[rstart + 4] = c;
        p = (((int)words1[astart + 2]) & 0xffff) * (((int)words1[astart +
                    3]) & 0xffff); c = (short)p; d = ((int)p >>
                    16) & 0xffff;
        d = (int)((d << 1) + (((int)c >> 15) & 1)); c <<= 1;
        e += ((int)c) & 0xffff; c = (short)e; e = d + (((int)e >> 16) &
                    0xffff); result[rstart + (2 * 4) - 3] = c;
        p = (((int)words1[astart + 3]) & 0xffff) * (((int)words1[astart +
                    3]) & 0xffff);
        p += e; result[rstart + 6] = (short)p; result[rstart + 7] = (short)(p >>
                    16);
      }
    }

    private static void BaselineSquare8(
      short[] result,
      int rstart,
      short[] words1,
      int astart) {
      unchecked {
        int p; short c; int d; int e;
        p = (((int)words1[astart]) & 0xffff) * (((int)words1[astart]) &
                    0xffff); result[rstart] = (short)p; e = ((int)p >>
                    16) & 0xffff;
        p = (((int)words1[astart]) & 0xffff) * (((int)words1[astart + 1]) &
                    0xffff); c = (short)p; d = ((int)p >> 16) &
          0xffff;
        d = (int)((d << 1) + (((int)c >> 15) & 1)); c <<=
          1;
        e += ((int)c) & 0xffff; c = (short)e; e = d + (((int)e >> 16) &
                    0xffff); result[rstart + 1] = c;
        p = (((int)words1[astart]) & 0xffff) * (((int)words1[astart + 2]) &
                    0xffff); c = (short)p; d = ((int)p >> 16) &
          0xffff;
        d = (int)((d << 1) + (((int)c >> 15) & 1)); c <<=
          1;
        p = (((int)words1[astart + 1]) & 0xffff) * (((int)words1[astart +
                    1]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff;
        e += ((int)c) & 0xffff; c = (short)e; e = d + (((int)e >> 16) &
                    0xffff); result[rstart + 2] = c;
        p = (((int)words1[astart]) & 0xffff) * (((int)words1[astart + 3]) &
                    0xffff); c = (short)p; d = ((int)p >> 16) &
          0xffff;
        p = (((int)words1[astart + 1]) & 0xffff) * (((int)words1[astart +
                    2]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff; d = (int)((d << 1) + (((int)c >> 15) &
                    1)); c <<= 1;
        e += ((int)c) & 0xffff; c = (short)e; e = d + (((int)e >> 16) &
                    0xffff); result[rstart + 3] = c;
        p = (((int)words1[astart]) & 0xffff) * (((int)words1[astart + 4]) &
                    0xffff); c = (short)p; d = ((int)p >> 16) &
          0xffff;
        p = (((int)words1[astart + 1]) & 0xffff) * (((int)words1[astart +
                    3]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff; d = (int)((d << 1) + (((int)c >> 15) &
                    1)); c <<= 1;
        p = (((int)words1[astart + 2]) & 0xffff) * (((int)words1[astart +
                    2]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff;
        e += ((int)c) & 0xffff; c = (short)e; e = d + (((int)e >> 16) &
                    0xffff); result[rstart + 4] = c;
        p = (((int)words1[astart]) & 0xffff) * (((int)words1[astart + 5]) &
                    0xffff); c = (short)p; d = ((int)p >> 16) &
          0xffff;
        p = (((int)words1[astart + 1]) & 0xffff) * (((int)words1[astart +
                    4]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff;
        p = (((int)words1[astart + 2]) & 0xffff) * (((int)words1[astart +
                    3]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff; d = (int)((d << 1) + (((int)c >> 15) &
                    1)); c <<= 1;
        e += ((int)c) & 0xffff; c = (short)e; e = d + (((int)e >> 16) &
                    0xffff); result[rstart + 5] = c;
        p = (((int)words1[astart]) & 0xffff) * (((int)words1[astart + 6]) &
                    0xffff); c = (short)p; d = ((int)p >> 16) &
          0xffff;
        p = (((int)words1[astart + 1]) & 0xffff) * (((int)words1[astart +
                    5]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff;
        p = (((int)words1[astart + 2]) & 0xffff) * (((int)words1[astart +
                    4]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff; d = (int)((d << 1) + (((int)c >> 15) &
                    1)); c <<= 1;
        p = (((int)words1[astart + 3]) & 0xffff) * (((int)words1[astart +
                    3]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff;
        e += ((int)c) & 0xffff; c = (short)e; e = d + (((int)e >> 16) &
                    0xffff); result[rstart + 6] = c;
        p = (((int)words1[astart]) & 0xffff) * (((int)words1[astart + 7]) &
                    0xffff); c = (short)p; d = ((int)p >> 16) &
          0xffff;
        p = (((int)words1[astart + 1]) & 0xffff) * (((int)words1[astart +
                    6]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff;
        p = (((int)words1[astart + 2]) & 0xffff) * (((int)words1[astart +
                    5]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff;
        p = (((int)words1[astart + 3]) & 0xffff) * (((int)words1[astart +
                    4]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff; d = (int)((d << 1) + (((int)c >> 15) &
                    1)); c <<= 1;
        e += ((int)c) & 0xffff; c = (short)e; e = d + (((int)e >> 16) &
                    0xffff); result[rstart + 7] = c;
        p = (((int)words1[astart + 1]) & 0xffff) * (((int)words1[astart +
                    7]) & 0xffff); c = (short)p; d = ((int)p >>
                    16) & 0xffff;
        p = (((int)words1[astart + 2]) & 0xffff) * (((int)words1[astart +
                    6]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff;
        p = (((int)words1[astart + 3]) & 0xffff) * (((int)words1[astart +
                    5]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff; d = (int)((d << 1) + (((int)c >> 15) &
                    1)); c <<= 1;
        p = (((int)words1[astart + 4]) & 0xffff) * (((int)words1[astart +
                    4]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff;
        e += ((int)c) & 0xffff; c = (short)e; e = d + (((int)e >> 16) &
                    0xffff); result[rstart + 8] = c;
        p = (((int)words1[astart + 2]) & 0xffff) * (((int)words1[astart +
                    7]) & 0xffff); c = (short)p; d = ((int)p >>
                    16) & 0xffff;
        p = (((int)words1[astart + 3]) & 0xffff) * (((int)words1[astart +
                    6]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff;
        p = (((int)words1[astart + 4]) & 0xffff) * (((int)words1[astart +
                    5]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff; d = (int)((d << 1) + (((int)c >> 15) &
                    1)); c <<= 1;
        e += ((int)c) & 0xffff; c = (short)e; e = d + (((int)e >> 16) &
                    0xffff); result[rstart + 9] = c;
        p = (((int)words1[astart + 3]) & 0xffff) * (((int)words1[astart +
                    7]) & 0xffff); c = (short)p; d = ((int)p >>
                    16) & 0xffff;
        p = (((int)words1[astart + 4]) & 0xffff) * (((int)words1[astart +
                    6]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff; d = (int)((d << 1) + (((int)c >> 15) &
                    1)); c <<= 1;
        p = (((int)words1[astart + 5]) & 0xffff) * (((int)words1[astart +
                    5]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff;
        e += ((int)c) & 0xffff; c = (short)e; e = d + (((int)e >> 16) &
                    0xffff); result[rstart + 10] = c;
        p = (((int)words1[astart + 4]) & 0xffff) * (((int)words1[astart +
                    7]) & 0xffff); c = (short)p; d = ((int)p >>
                    16) & 0xffff;
        p = (((int)words1[astart + 5]) & 0xffff) * (((int)words1[astart +
                    6]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff; d = (int)((d << 1) + (((int)c >> 15) &
                    1)); c <<= 1;
        e += ((int)c) & 0xffff; c = (short)e; e = d + (((int)e >> 16) &
                    0xffff); result[rstart + 11] = c;
        p = (((int)words1[astart + 5]) & 0xffff) * (((int)words1[astart +
                    7]) & 0xffff); c = (short)p; d = ((int)p >>
                    16) & 0xffff;
        d = (int)((d << 1) + (((int)c >> 15) & 1)); c <<= 1;
        p = (((int)words1[astart + 6]) & 0xffff) * (((int)words1[astart +
                    6]) & 0xffff);
        p += ((int)c) & 0xffff; c = (short)p;
        d += ((int)p >> 16) & 0xffff;
        e += ((int)c) & 0xffff; c = (short)e; e = d + (((int)e >> 16) &
                    0xffff); result[rstart + 12] = c;
        p = (((int)words1[astart + 6]) & 0xffff) * (((int)words1[astart +
                    7]) & 0xffff); c = (short)p; d = ((int)p >>
                    16) & 0xffff;
        d = (int)((d << 1) + (((int)c >> 15) & 1)); c <<= 1;
        e += ((int)c) & 0xffff; c = (short)e; e = d + (((int)e >> 16) &
                    0xffff); result[rstart + 13] = c;
        p = (((int)words1[astart + 7]) & 0xffff) * (((int)words1[astart +
                    7]) & 0xffff);
        p += e; result[rstart + 14] = (short)p; result[rstart + 15] =
          (short)(p >> 16);
      }
    }

    private static int BitPrecision(short numberValue) {
      if (numberValue == 0) {
        return 0;
      }
      var i = 16;
      unchecked {
        if ((numberValue >> 8) == 0) {
          numberValue <<= 8;
          i -= 8;
        }

        if ((numberValue >> 12) == 0) {
          numberValue <<= 4;
          i -= 4;
        }

        if ((numberValue >> 14) == 0) {
          numberValue <<= 2;
          i -= 2;
        }

        if ((numberValue >> 15) == 0) {
          --i;
        }
      }
      return i;
    }

    private static int BitsToWords(int bitCount) {
      return (bitCount + 15) >> 4;
    }

    private static void ChunkedLinearMultiply(
      short[] productArr,
      int cstart,
      short[] tempArr,
      int tempStart,  // uses bcount*4 space
      short[] words1,
      int astart,
      int acount,  // Equal size or longer
      short[] words2,
      int bstart,
      int bcount) {
#if DEBUG
      if (acount < bcount) {
        throw new ArgumentException("acount (" + acount + ") is less than " +
                    bcount);
      }

      if (productArr == null) {
        throw new ArgumentNullException(nameof(productArr));
      }

      if (cstart < 0) {
        throw new ArgumentException("cstart (" + cstart + ") is less than " +
                    "0");
      }

      if (cstart > productArr.Length) {
        throw new ArgumentException("cstart (" + cstart + ") is more than " +
                    productArr.Length);
      }

      if (acount + bcount < 0) {
        throw new ArgumentException("acount plus bcount (" + (acount + bcount) +
                    ") is less than 0");
      }

      if (acount + bcount > productArr.Length) {
        throw new ArgumentException("acount plus bcount (" + (acount + bcount) +
                    ") is more than " + productArr.Length);
      }

      if (productArr.Length - cstart < acount + bcount) {
        throw new ArgumentException("productArr.Length minus cstart (" +
                    (productArr.Length - cstart) +
                    ") is less than " + (acount + bcount));
      }

      if (tempArr == null) {
        throw new ArgumentNullException(nameof(tempArr));
      }

      if (tempStart < 0) {
        throw new ArgumentException("tempStart (" + tempStart +
                    ") is less than 0");
      }

      if (tempStart > tempArr.Length) {
        throw new ArgumentException("tempStart (" + tempStart +
                    ") is more than " + tempArr.Length);
      }

      if ((bcount * 4) < 0) {
        throw new ArgumentException("bcount * 4 less than 0 (" + (bcount * 4) +
                    ")");
      }

      if ((bcount * 4) > tempArr.Length) {
        throw new ArgumentException("bcount * 4 more than " + tempArr.Length +
                    " (" + (bcount * 4) + ")");
      }

      if (tempArr.Length - tempStart < bcount * 4) {
        throw new ArgumentException("tempArr.Length minus tempStart (" +
                    (tempArr.Length - tempStart) +
                    ") is less than " + (bcount * 4));
      }

      if (words1 == null) {
        throw new ArgumentNullException(nameof(words1));
      }

      if (astart < 0) {
        throw new ArgumentException("astart (" + astart + ") is less than " +
                    "0");
      }

      if (astart > words1.Length) {
        throw new ArgumentException("astart (" + astart + ") is more than " +
                    words1.Length);
      }

      if (acount < 0) {
        throw new ArgumentException("acount (" + acount + ") is less than " +
                    "0");
      }

      if (acount > words1.Length) {
        throw new ArgumentException("acount (" + acount + ") is more than " +
                    words1.Length);
      }

      if (words1.Length - astart < acount) {
        throw new ArgumentException("words1.Length minus astart (" +
                    (words1.Length - astart) + ") is less than " +
                    acount);
      }

      if (words2 == null) {
        throw new ArgumentNullException(nameof(words2));
      }

      if (bstart < 0) {
        throw new ArgumentException("bstart (" + bstart + ") is less than " +
                    "0");
      }

      if (bstart > words2.Length) {
        throw new ArgumentException("bstart (" + bstart + ") is more than " +
                    words2.Length);
      }

      if (bcount < 0) {
        throw new ArgumentException("bcount (" + bcount + ") is less than " +
                    "0");
      }

      if (bcount > words2.Length) {
        throw new ArgumentException("bcount (" + bcount + ") is more than " +
                    words2.Length);
      }

      if (words2.Length - bstart < bcount) {
        throw new ArgumentException("words2.Length minus bstart (" +
                    (words2.Length - bstart) + ") is less than " +
                    bcount);
      }
#endif

      unchecked {
        var carryPos = 0;
        // Set carry to ValueZero
        Array.Clear((short[])productArr, cstart, bcount);
        for (var i = 0; i < acount; i += bcount) {
          int diff = acount - i;
          if (diff > bcount) {
            SameSizeMultiply(
              tempArr,
              tempStart,
              tempArr,
              tempStart + bcount + bcount,
              words1,
              astart + i,
              words2,
              bstart,
              bcount);
            // Add carry
            AddUnevenSize(
              tempArr,
              tempStart,
              tempArr,
              tempStart,
              bcount + bcount,
              productArr,
              cstart + carryPos,
              bcount);
            // Copy product and carry
            Array.Copy(
              tempArr,
              tempStart,
              productArr,
              cstart + i,
              bcount + bcount);
            carryPos += bcount;
          } else {
            AsymmetricMultiply(
              tempArr,
              tempStart,  // uses diff + bcount space
              tempArr,
              tempStart + diff + bcount,  // uses diff + bcount
              words1,
              astart + i,
              diff,
              words2,
              bstart,
              bcount);
            // Add carry
            AddUnevenSize(
              tempArr,
              tempStart,
              tempArr,
              tempStart,
              diff + bcount,
              productArr,
              cstart + carryPos,
              bcount);
            // Copy product without carry
            Array.Copy(
              tempArr,
              tempStart,
              productArr,
              cstart + i,
              diff + bcount);
          }
        }
      }
    }

    private static short[] CleanGrow(short[] a, int size) {
      if (size > a.Length) {
        var newa = new short[size];
        Array.Copy(a, newa, a.Length);
        return newa;
      }
      return a;
    }

    private static int Compare(
      short[] words1,
      int astart,
      short[] words2,
      int bstart,
      int n) {
      while (unchecked(n--) != 0) {
        int an = ((int)words1[astart + n]) & 0xffff;
        int bn = ((int)words2[bstart + n]) & 0xffff;
        if (an > bn) {
          return 1;
        }
        if (an < bn) {
          return -1;
        }
      }
      return 0;
    }

    private static int CompareWithWords1IsOneBigger(
      short[] words1,
      int astart,
      short[] words2,
      int bstart,
      int words1Count) {
      // NOTE: Assumes that words2's count is 1 less
      if (words1[astart + words1Count - 1] != 0) {
        return 1;
      }
      int w1c = words1Count;
      --w1c;
      while (unchecked(w1c--) != 0) {
        int an = ((int)words1[astart + w1c]) & 0xffff;
        int bn = ((int)words2[bstart + w1c]) & 0xffff;
        if (an > bn) {
          return 1;
        }
        if (an < bn) {
          return -1;
        }
      }
      return 0;
    }

    private static int CountWords(short[] array) {
      int n = array.Length;
      while (n != 0 && array[n - 1] == 0) {
        --n;
      }
      return (int)n;
    }

    private static int CountWords(short[] array, int pos, int len) {
      int n = len;
      while (n != 0 && array[pos + n - 1] == 0) {
        --n;
      }
      return (int)n;
    }

    private static int Decrement(
      short[] words1,
      int words1Start,
      int n,
      short words2) {
      unchecked {
        short tmp = words1[words1Start];
        words1[words1Start] = (short)(tmp - words2);
        if ((((int)words1[words1Start]) & 0xffff) <= (((int)tmp) & 0xffff)) {
          return 0;
        }
        for (int i = 1; i < n; ++i) {
          tmp = words1[words1Start + i];
          --words1[words1Start + i];
          if (tmp != 0) {
            return 0;
          }
        }
        return 1;
      }
    }

    private static short Divide32By16(
       int dividendLow,
       short divisorShort,
       bool returnRemainder) {
      int tmpInt;
      var dividendHigh = 0;
      int intDivisor = ((int)divisorShort) & 0xffff;
      for (var i = 0; i < 32; ++i) {
        tmpInt = dividendHigh >> 31;
        dividendHigh <<= 1;
        dividendHigh = unchecked((int)(dividendHigh | ((int)((dividendLow >>
                    31) & 1))));
        dividendLow <<= 1;
        tmpInt |= dividendHigh;
        // unsigned greater-than-or-equal check
        if (((tmpInt >> 31) != 0) || (tmpInt >= intDivisor)) {
          unchecked {
            dividendHigh -= intDivisor;
            ++dividendLow;
          }
        }
      }
      return returnRemainder ? unchecked((short)(((int)dividendHigh) &
                    0xffff)) : unchecked((short)(((int)dividendLow) &
                    0xffff));
    }

    private static short DivideUnsigned(int x, short y) {
      if ((x >> 31) == 0) {
        // x is already nonnegative
        int iy = ((int)y) & 0xffff;
        return unchecked((short)((int)x / iy));
      } else {
        long longX = ((long)x) & 0xffffffffL;
        int iy = ((int)y) & 0xffff;
        return unchecked((short)(longX / iy));
      }
    }

    private static void FastDivide(
      short[] quotientReg,
      short[] dividendReg,
      int count,
      short divisorSmall) {
      switch (divisorSmall) {
        case 2:
          FastDivideAndRemainderTwo(quotientReg, 0, dividendReg, 0, count);
          break;
        case 10:
          FastDivideAndRemainderTen(quotientReg, 0, dividendReg, 0, count);
          break;
        default:
          FastDivideAndRemainder(
       quotientReg,
       0,
       dividendReg,
       0,
       count,
       divisorSmall);
          break;
      }
    }

    private static short FastDivideAndRemainderTwo(
      short[] quotientReg,
      int quotientStart,
      short[] dividendReg,
      int dividendStart,
      int count) {
      int quo;
      var rem = 0;
      int currentDividend;
      int ds = dividendStart + count - 1;
      int qs = quotientStart + count - 1;
      for (var i = 0; i < count; ++i) {
        currentDividend = ((int)dividendReg[ds]) & 0xffff;
        currentDividend |= rem << 16;
        quo = currentDividend >> 1;
        quotientReg[qs] = unchecked((short)quo);
        rem = currentDividend & 1;
        --ds;
        --qs;
      }
      return unchecked((short)rem);
    }

    private static short FastDivideAndRemainderTen(
      short[] quotientReg,
      int quotientStart,
      short[] dividendReg,
      int dividendStart,
      int count) {
      int quo;
      var rem = 0;
      int currentDividend;
      int ds = dividendStart + count - 1;
      int qs = quotientStart + count - 1;
      for (var i = 0; i < count; ++i) {
        currentDividend = ((int)dividendReg[ds]) & 0xffff;
        currentDividend |= rem << 16;
        quo = (currentDividend < 43698) ? ((currentDividend * 26215) >> 18) :
            (currentDividend / 10);
        quotientReg[qs] = unchecked((short)quo);
        rem = currentDividend - (10 * quo);
        --ds;
        --qs;
      }
      return unchecked((short)rem);
    }

    private static short FastDivideAndRemainder(
      short[] quotientReg,
      int quotientStart,
      short[] dividendReg,
      int dividendStart,
      int count,
      short divisorSmall) {
      int idivisor = ((int)divisorSmall) & 0xffff;
      int quo;
      var rem = 0;
      int ds = dividendStart + count - 1;
      int qs = quotientStart + count - 1;
      int currentDividend;
      if (idivisor >= 0x8000) {
        for (var i = 0; i < count; ++i) {
          currentDividend = ((int)dividendReg[ds]) & 0xffff;
          currentDividend |= rem << 16;
          if ((currentDividend >> 31) == 0) {
            quo = currentDividend / idivisor;
            quotientReg[qs] = unchecked((short)quo);
            rem = currentDividend - (idivisor * quo);
          } else {
            quo = ((int)DivideUnsigned(
             currentDividend,
             divisorSmall)) & 0xffff;
            quotientReg[qs] = unchecked((short)quo);
            rem = unchecked(currentDividend - (idivisor * quo));
          }
          --ds;
          --qs;
        }
      } else {
        for (var i = 0; i < count; ++i) {
          currentDividend = ((int)dividendReg[ds]) & 0xffff;
          currentDividend |= rem << 16;
          quo = currentDividend / idivisor;
          quotientReg[qs] = unchecked((short)quo);
          rem = currentDividend - (idivisor * quo);
          --ds;
          --qs;
        }
      }
      return unchecked((short)rem);
    }

    private static short FastRemainder(
      short[] dividendReg,
      int count,
      short divisorSmall) {
      int i = count;
      short remainder = 0;
      while ((i--) > 0) {
        remainder = RemainderUnsigned(
          MakeUint(dividendReg[i], remainder),
          divisorSmall);
      }
      return remainder;
    }

    private static short GetHighHalfAsBorrow(int val) {
      return unchecked((short)(0 - ((val >> 16) & 0xffff)));
    }

    private static int GetLowHalf(int val) {
      return val & 0xffff;
    }

    private static int GetUnsignedBitLengthEx(int numberValue, int wordCount) {
      int wc = wordCount;
      if (wc != 0) {
        wc = (wc - 1) << 4;
        if (numberValue == 0) {
          return wc;
        }
        wc += 16;
        unchecked {
          if ((numberValue >> 8) == 0) {
            numberValue <<= 8;
            wc -= 8;
          }
          if ((numberValue >> 12) == 0) {
            numberValue <<= 4;
            wc -= 4;
          }
          if ((numberValue >> 14) == 0) {
            numberValue <<= 2;
            wc -= 2;
          }
          if ((numberValue >> 15) == 0) {
            --wc;
          }
        }
        return wc;
      }
      return 0;
    }

    private static short[] GrowForCarry(short[] a, short carry) {
      int oldLength = a.Length;
      short[] ret = CleanGrow(a, oldLength + 1);
      ret[oldLength] = carry;
      return ret;
    }

    private static int Increment(
      short[] words1,
      int words1Start,
      int n,
      short words2) {
      unchecked {
        short tmp = words1[words1Start];
        words1[words1Start] = (short)(tmp + words2);
        if ((((int)words1[words1Start]) & 0xffff) >= (((int)tmp) & 0xffff)) {
          return 0;
        }
        for (int i = 1; i < n; ++i) {
          ++words1[words1Start + i];
          if (words1[words1Start + i] != 0) {
            return 0;
          }
        }
        return 1;
      }
    }

    private static short LinearMultiply(
      short[] productArr,
      int cstart,
      short[] words1,
      int astart,
      short words2,
      int n) {
      unchecked {
        short carry = 0;
        int bint = ((int)words2) & 0xffff;
        for (var i = 0; i < n; ++i) {
          int p;
          p = (((int)words1[astart + i]) & 0xffff) * bint;
          p += ((int)carry) & 0xffff;
          productArr[cstart + i] = (short)p;
          carry = (short)(p >> 16);
        }
        return carry;
      }
    }

    private static short LinearMultiplyAdd(
      short[] productArr,
      int cstart,
      short[] words1,
      int astart,
      short words2,
      int n) {
      short carry = 0;
      int bint = ((int)words2) & 0xffff;
      for (var i = 0; i < n; ++i) {
        int p;
        p = unchecked((((int)words1[astart + i]) & 0xffff) * bint);
        p = unchecked(p + (((int)carry) & 0xffff));
        p = unchecked(p + (((int)productArr[cstart + i]) & 0xffff));
        productArr[cstart + i] = unchecked((short)p);
        carry = (short)(p >> 16);
      }
      return carry;
    }

    private static int MakeUint(short first, short second) {
      return unchecked((int)((((int)first) & 0xffff) | ((int)second << 16)));
    }

    private static void RecursiveSquare(
      short[] resultArr,
      int resultStart,
      short[] tempArr,
      int tempStart,
      short[] words1,
      int words1Start,
      int count) {
      if (count <= RecursionLimit) {
        switch (count) {
          case 2:
            BaselineSquare2(resultArr, resultStart, words1, words1Start);
            break;
          case 4:
            BaselineSquare4(resultArr, resultStart, words1, words1Start);
            break;
          case 8:
            BaselineSquare8(resultArr, resultStart, words1, words1Start);
            break;
          default:
            SchoolbookSquare(
  resultArr,
  resultStart,
  words1,
  words1Start,
  count);
            break;
        }
      } else if ((count & 1) == 0) {
        int count2 = count >> 1;
        RecursiveSquare(
          resultArr,
          resultStart,
          tempArr,
          tempStart + count,
          words1,
          words1Start,
          count2);
        RecursiveSquare(
          resultArr,
          resultStart + count,
          tempArr,
          tempStart + count,
          words1,
          words1Start + count2,
          count2);
        SameSizeMultiply(
          tempArr,
          tempStart,
          tempArr,
          tempStart + count,
          words1,
          words1Start,
          words1,
          words1Start + count2,
          count2);
        int carry = AddInternal(
          resultArr,
          resultStart + count2,
          resultArr,
          resultStart + count2,
          tempArr,
          tempStart,
          count);
        carry += AddInternal(
          resultArr,
          resultStart + count2,
          resultArr,
          resultStart + count2,
          tempArr,
          tempStart,
          count);
        Increment(
          resultArr,
          (int)(resultStart + count + count2),
          count2,
          (short)carry);
      } else {
        SameSizeMultiply(
          resultArr,
          resultStart,
          tempArr,
          tempStart,
          words1,
          words1Start,
          words1,
          words1Start,
          count);
      }
    }

    private static short RemainderUnsigned(int x, short y) {
      unchecked {
        int iy = ((int)y) & 0xffff;
        return ((x >> 31) == 0) ? ((short)(((int)x % iy) & 0xffff)) :
          Divide32By16(x, y, true);
      }
    }

    private static void ReverseChars(char[] chars, int offset, int length) {
      int half = length >> 1;
      int right = offset + length - 1;
      for (var i = 0; i < half; i++, right--) {
        char value = chars[offset + i];
        chars[offset + i] = chars[right];
        chars[right] = value;
      }
    }

    // NOTE: Renamed from RecursiveMultiply to better show that
    // this function only takes operands of the same size, as opposed
    // to AsymmetricMultiply.
    private static void SameSizeMultiply(
      short[] resultArr,  // size 2*count
      int resultStart,
      short[] tempArr,  // size 2*count
      int tempStart,
      short[] words1,
      int words1Start,  // size count
      short[] words2,
      int words2Start,  // size count
      int count) {
      // DebugUtility.Log("RecursiveMultiply " + count + " " + count +
      // " [r=" + resultStart + " t=" + tempStart + " a=" + words1Start +
      // " b=" + words2Start + "]");
#if DEBUG
      if (resultArr == null) {
        throw new ArgumentNullException(nameof(resultArr));
      }

      if (resultStart < 0) {
        throw new ArgumentException("resultStart (" + resultStart +
                    ") is less than 0");
      }

      if (resultStart > resultArr.Length) {
        throw new ArgumentException("resultStart (" + resultStart +
                    ") is more than " + resultArr.Length);
      }

      if (count + count < 0) {
        throw new ArgumentException("count plus count (" + (count + count) +
                    ") is less than 0");
      }
      if (count + count > resultArr.Length) {
        throw new ArgumentException("count plus count (" + (count + count) +
                    ") is more than " + resultArr.Length);
      }

      if (resultArr.Length - resultStart < count + count) {
        throw new ArgumentException("resultArr.Length minus resultStart (" +
                    (resultArr.Length - resultStart) +
                    ") is less than " + (count + count));
      }

      if (tempArr == null) {
        throw new ArgumentNullException(nameof(tempArr));
      }

      if (tempStart < 0) {
        throw new ArgumentException("tempStart (" + tempStart +
                    ") is less than 0");
      }

      if (tempStart > tempArr.Length) {
        throw new ArgumentException("tempStart (" + tempStart +
                    ") is more than " + tempArr.Length);
      }

      if (count + count < 0) {
        throw new ArgumentException("count plus count (" + (count + count) +
                    ") is less than 0");
      }

      if (count + count > tempArr.Length) {
        throw new ArgumentException("count plus count (" + (count + count) +
                    ") is more than " + tempArr.Length);
      }

      if (tempArr.Length - tempStart < count + count) {
        throw new ArgumentException("tempArr.Length minus tempStart (" +
                    (tempArr.Length - tempStart) +
                    ") is less than " + (count + count));
      }

      if (words1 == null) {
        throw new ArgumentNullException(nameof(words1));
      }

      if (words1Start < 0) {
        throw new ArgumentException("words1Start (" + words1Start +
                    ") is less than 0");
      }

      if (words1Start > words1.Length) {
        throw new ArgumentException("words1Start (" + words1Start +
                    ") is more than " + words1.Length);
      }

      if (count < 0) {
        throw new ArgumentException("count (" + count + ") is less than " +
                    "0");
      }

      if (count > words1.Length) {
        throw new ArgumentException("count (" + count + ") is more than " +
                    words1.Length);
      }

      if (words1.Length - words1Start < count) {
        throw new ArgumentException("words1.Length minus words1Start (" +
                    (words1.Length - words1Start) + ") is less than " +
                    count);
      }

      if (words2 == null) {
        throw new ArgumentNullException(nameof(words2));
      }

      if (words2Start < 0) {
        throw new ArgumentException("words2Start (" + words2Start +
                    ") is less than 0");
      }

      if (words2Start > words2.Length) {
        throw new ArgumentException("words2Start (" + words2Start +
                    ") is more than " + words2.Length);
      }

      if (count < 0) {
        throw new ArgumentException("count (" + count + ") is less than " +
                    "0");
      }

      if (count > words2.Length) {
        throw new ArgumentException("count (" + count + ") is more than " +
                    words2.Length);
      }

      if (words2.Length - words2Start < count) {
        throw new ArgumentException("words2.Length minus words2Start (" +
                    (words2.Length - words2Start) + ") is less than " +
                    count);
      }
#endif

      if (count <= RecursionLimit) {
        switch (count) {
          case 2:
            BaselineMultiply2(
  resultArr,
  resultStart,
  words1,
  words1Start,
  words2,
  words2Start);
            break;
          case 4:
            BaselineMultiply4(
  resultArr,
  resultStart,
  words1,
  words1Start,
  words2,
  words2Start);
            break;
          case 8:
            BaselineMultiply8(
  resultArr,
  resultStart,
  words1,
  words1Start,
  words2,
  words2Start);
            break;
          default: SchoolbookMultiply(
  resultArr,
  resultStart,
  words1,
  words1Start,
  count,
  words2,
  words2Start,
  count);
            break;
        }
      } else {
        int countA = count;
        while (countA != 0 && words1[words1Start + countA - 1] == 0) {
          --countA;
        }
        int countB = count;
        while (countB != 0 && words2[words2Start + countB - 1] == 0) {
          --countB;
        }
        var offset2For1 = 0;
        var offset2For2 = 0;
        if (countA == 0 || countB == 0) {
          // words1 or words2 is empty, so result is 0
          Array.Clear((short[])resultArr, resultStart, count << 1);
          return;
        }
        // Split words1 and words2 in two parts each
        // Words1 is split into HighA and LowA
        // Words2 is split into HighB and LowB
        if ((count & 1) == 0) {
          // Count is even, so each part will be equal size
          int count2 = count >> 1;
          if (countA <= count2 && countB <= count2) {
            // Both words1 and words2 are smaller than half the
            // count (their high parts are 0)
            // DebugUtility.Log("Can be smaller: " + AN + "," + BN + "," +
            // (count2));
            Array.Clear((short[])resultArr, resultStart + count, count);
            if (count2 == 8) {
              BaselineMultiply8(
                resultArr,
                resultStart,
                words1,
                words1Start,
                words2,
                words2Start);
            } else {
              SameSizeMultiply(
                resultArr,
                resultStart,
                tempArr,
                tempStart,
                words1,
                words1Start,
                words2,
                words2Start,
                count2);
            }
            return;
          }
          int resultMediumHigh = resultStart + count;
          int resultHigh = resultMediumHigh + count2;
          int resultMediumLow = resultStart + count2;
          int tsn = tempStart + count;
          // Find the part of words1 with the higher value
          // so we can compute the absolute value
          offset2For1 = Compare(
            words1,
            words1Start,
            words1,
            words1Start + count2,
            count2) > 0 ? 0 : count2;
          var tmpvar = (int)(words1Start + (count2 ^
                    offset2For1));
          // Abs(LowA - HighA)
          SubtractInternal(
            resultArr,
            resultStart,
            words1,
            words1Start + offset2For1,
            words1,
            tmpvar,
            count2);
          // Find the part of words2 with the higher value
          // so we can compute the absolute value
          offset2For2 = Compare(
            words2,
            words2Start,
            words2,
            words2Start + count2,
            count2) > 0 ? 0 : count2;
          // Abs(LowB - HighB)
          int tmp = words2Start + (count2 ^ offset2For2);
          SubtractInternal(
            resultArr,
            resultMediumLow,
            words2,
            words2Start + offset2For2,
            words2,
            tmp,
            count2);
          // Medium-high/high result = HighA * HighB
          SameSizeMultiply(
            resultArr,
            resultMediumHigh,
            tempArr,
            tsn,
            words1,
            words1Start + count2,
            words2,
            words2Start + count2,
            count2);
          // Temp = Abs(LowA-HighA) * Abs(LowB-HighB)
          SameSizeMultiply(
            tempArr,
            tempStart,
            tempArr,
            tsn,
            resultArr,
            resultStart,
            resultArr,
            resultMediumLow,
            count2);
          // Low/Medium-low result = LowA * LowB
          SameSizeMultiply(
            resultArr,
            resultStart,
            tempArr,
            tsn,
            words1,
            words1Start,
            words2,
            words2Start,
            count2);
          // Medium high result = Low(HighA * HighB) + High(LowA * LowB)
          int c2 = AddInternal(
            resultArr,
            resultMediumHigh,
            resultArr,
            resultMediumHigh,
            resultArr,
            resultMediumLow,
            count2);
          int c3 = c2;
          // Medium low result = Low(HighA * HighB) + High(LowA * LowB) +
          // Low(LowA * LowB)
          c2 += AddInternal(
            resultArr,
            resultMediumLow,
            resultArr,
            resultMediumHigh,
            resultArr,
            resultStart,
            count2);
          // Medium high result = Low(HighA * HighB) + High(LowA * LowB) +
          // High(HighA * HighB)
          c3 += AddInternal(
            resultArr,
            resultMediumHigh,
            resultArr,
            resultMediumHigh,
            resultArr,
            resultHigh,
            count2);
          if (offset2For1 == offset2For2) {
            // If high parts of both words were greater
            // than their low parts
            // or if low parts of both words were greater
            // than their high parts
            // Medium low/Medium high result = Medium low/Medium high result
            // - Low(Temp)
            c3 -= SubtractInternal(
              resultArr,
              resultMediumLow,
              resultArr,
              resultMediumLow,
              tempArr,
              tempStart,
              count);
          } else {
            // Medium low/Medium high result = Medium low/Medium high result
            // + Low(Temp)
            c3 += AddInternal(
              resultArr,
              resultMediumLow,
              resultArr,
              resultMediumLow,
              tempArr,
              tempStart,
              count);
          }
          // Add carry
          c3 += Increment(resultArr, resultMediumHigh, count2, (short)c2);
          if (c3 != 0) {
            Increment(resultArr, resultHigh, count2, (short)c3);
          }
        } else {
          // Count is odd, high part will be 1 shorter
          int countHigh = count >> 1;  // Shorter part
          int countLow = count - countHigh;  // Longer part
          offset2For1 = CompareWithWords1IsOneBigger(
            words1,
            words1Start,
            words1,
            words1Start + countLow,
            countLow) > 0 ? 0 : countLow;
          // Abs(LowA - HighA)
          if (offset2For1 == 0) {
            SubtractWords1IsOneBigger(
              resultArr,
              resultStart,
              words1,
              words1Start,
              words1,
              words1Start + countLow,
              countLow);
          } else {
            SubtractWords2IsOneBigger(
              resultArr,
              resultStart,
              words1,
              words1Start + countLow,
              words1,
              words1Start,
              countLow);
          }
          offset2For2 = CompareWithWords1IsOneBigger(
            words2,
            words2Start,
            words2,
            words2Start + countLow,
            countLow) > 0 ? 0 : countLow;
          // Abs(LowB, HighB)
          if (offset2For2 == 0) {
            SubtractWords1IsOneBigger(
              tempArr,
              tempStart,
              words2,
              words2Start,
              words2,
              words2Start + countLow,
              countLow);
          } else {
            SubtractWords2IsOneBigger(
              tempArr,
              tempStart,
              words2,
              words2Start + countLow,
              words2,
              words2Start,
              countLow);
          }
          // Temp = Abs(LowA-HighA) * Abs(LowB-HighB)
          int shorterOffset = countHigh << 1;
          int longerOffset = countLow << 1;
          SameSizeMultiply(
            tempArr,
            tempStart + shorterOffset,
            resultArr,
            resultStart + shorterOffset,
            resultArr,
            resultStart,
            tempArr,
            tempStart,
            countLow);
          // Save part of temp since temp will overlap in part
          // in the Low/Medium low result multiply
          short resultTmp0 = tempArr[tempStart + shorterOffset];
          short resultTmp1 = tempArr[tempStart + shorterOffset + 1];
          // Medium high/high result = HighA * HighB
          SameSizeMultiply(
            resultArr,
            resultStart + longerOffset,
            resultArr,
            resultStart,
            words1,
            words1Start + countLow,
            words2,
            words2Start + countLow,
            countHigh);
          // Low/Medium low result = LowA * LowB
          SameSizeMultiply(
            resultArr,
            resultStart,
            tempArr,
            tempStart,
            words1,
            words1Start,
            words2,
            words2Start,
            countLow);
          // Restore part of temp
          tempArr[tempStart + shorterOffset] = resultTmp0;
          tempArr[tempStart + shorterOffset + 1] = resultTmp1;
          int countMiddle = countLow << 1;
          // Medium high result = Low(HighA * HighB) + High(LowA * LowB)
          int c2 = AddInternal(
            resultArr,
            resultStart + countMiddle,
            resultArr,
            resultStart + countMiddle,
            resultArr,
            resultStart + countLow,
            countLow);
          int c3 = c2;
          // Medium low result = Low(HighA * HighB) + High(LowA * LowB) +
          // Low(LowA * LowB)
          c2 += AddInternal(
            resultArr,
            resultStart + countLow,
            resultArr,
            resultStart + countMiddle,
            resultArr,
            resultStart,
            countLow);
          // Medium high result = Low(HighA * HighB) + High(LowA * LowB) +
          // High(HighA * HighB)
          c3 += AddUnevenSize(
            resultArr,
            resultStart + countMiddle,
            resultArr,
            resultStart + countMiddle,
            countLow,
            resultArr,
            resultStart + countMiddle + countLow,
            countLow - 2);
          if (offset2For1 == offset2For2) {
            // If high parts of both words were greater
            // than their low parts
            // or if low parts of both words were greater
            // than their high parts
            // Medium low/Medium high result = Medium low/Medium high result
            // - Low(Temp)
            c3 -= SubtractInternal(
              resultArr,
              resultStart + countLow,
              resultArr,
              resultStart + countLow,
              tempArr,
              tempStart + shorterOffset,
              countLow << 1);
          } else {
            // Medium low/Medium high result = Medium low/Medium high result
            // + Low(Temp)
            c3 += AddInternal(
              resultArr,
              resultStart + countLow,
              resultArr,
              resultStart + countLow,
              tempArr,
              tempStart + shorterOffset,
              countLow << 1);
          }
          // Add carry
          c3 += Increment(
            resultArr,
            resultStart + countMiddle,
            countLow,
            (short)c2);
          if (c3 != 0) {
            Increment(
              resultArr,
              resultStart + countMiddle + countLow,
              countLow - 2,
              (short)c3);
          }
        }
      }
    }

    private static void SchoolbookMultiply(
      short[] resultArr,
      int resultStart,
      short[] words1,
      int words1Start,
      int words1Count,
      short[] words2,
      int words2Start,
      int words2Count) {
#if DEBUG
      // Avoid overlaps
      if (resultArr == words1) {
        int m1 = Math.Max(resultStart, words1Start);
        int m2 = Math.Min(
  resultStart + words1Count + words2Count,
  words1Start + words1Count);
        if (m1 < m2) {
          throw new ArgumentException();
        }
      }
      if (resultArr == words2) {
        int m1 = Math.Max(resultStart, words2Start);
        int m2 = Math.Min(
  resultStart + words1Count + words2Count,
  words2Start + words2Count);
        if (m1 < m2) {
          throw new ArgumentException();
        }
      }
if (words1Count <= 0) {
  throw new ArgumentException("words1Count (" + words1Count +
    ") is not greater than 0");
}
if (words2Count <= 0) {
  throw new ArgumentException("words2Count (" + words2Count +
    ") is not greater than 0");
}
#endif

      int cstart, carry, valueBint;
      if (words1Count < words2Count) {
        // words1 is shorter than words2, so put words2 on top
         carry = 0;
        valueBint = ((int)words1[words1Start]) & 0xffff;
        for (int j = 0; j < words2Count; ++j) {
              int p;
          p = unchecked((((int)words2[words2Start + j]) & 0xffff) *
                valueBint);
              p = unchecked(p + (((int)carry) & 0xffff));
              resultArr[resultStart + j] = unchecked((short)p);
              carry = (p >> 16) & 0xffff;
        }
        resultArr[resultStart + words2Count] = unchecked((short)carry);
        for (var i = 1; i < words1Count; ++i) {
          cstart = resultStart + i;
           carry = 0;
          valueBint = ((int)words1[words1Start + i]) & 0xffff;
          for (int j = 0; j < words2Count; ++j) {
              int p;
          p = unchecked((((int)words2[words2Start + j]) & 0xffff) *
                valueBint);
              p = unchecked(p + (((int)carry) & 0xffff));
              p = unchecked(p + (((int)resultArr[cstart + j]) & 0xffff));
              resultArr[cstart + j] = unchecked((short)p);
              carry = (p >> 16) & 0xffff;
          }
          resultArr[cstart + words2Count] = unchecked((short)carry);
        }
      } else {
        // words2 is shorter or the same length as words1
         carry = 0;
        valueBint = ((int)words2[words2Start]) & 0xffff;
        for (int j = 0; j < words1Count; ++j) {
              int p;
          p = unchecked((((int)words1[words1Start + j]) & 0xffff) *
                valueBint);
              p = unchecked(p + (((int)carry) & 0xffff));
              resultArr[resultStart + j] = unchecked((short)p);
              carry = (p >> 16) & 0xffff;
        }
        resultArr[resultStart + words1Count] = unchecked((short)carry);
        for (var i = 1; i < words2Count; ++i) {
          cstart = resultStart + i;
           carry = 0;
          valueBint = ((int)words2[words2Start + i]) & 0xffff;
          for (int j = 0; j < words1Count; ++j) {
              int p;
          p = unchecked((((int)words1[words1Start + j]) & 0xffff) *
                valueBint);
              p = unchecked(p + (((int)carry) & 0xffff));
              p = unchecked(p + (((int)resultArr[cstart + j]) & 0xffff));
              resultArr[cstart + j] = unchecked((short)p);
              carry = (p >> 16) & 0xffff;
          }
          resultArr[cstart + words1Count] = unchecked((short)carry);
        }
      }
    }

    private static void SchoolbookSquare(
      short[] resultArr,
      int resultStart,
      short[] words1,
      int words1Start,
      int words1Count) {
      // Method assumes that resultArr was already zeroed,
      // if resultArr is the same as words1
      int cstart;
      for (var i = 0; i < words1Count; ++i) {
        cstart = resultStart + i;
        unchecked {
          short carry = 0;
          int valueBint = ((int)words1[words1Start + i]) & 0xffff;
          for (int j = 0; j < words1Count; ++j) {
            int p;
            p = (((int)words1[words1Start + j]) & 0xffff) * valueBint;
            p += ((int)carry) & 0xffff;
            if (i != 0) {
              p += ((int)resultArr[cstart + j]) & 0xffff;
            }
            resultArr[cstart + j] = (short)p;
            carry = (short)(p >> 16);
          }
          resultArr[cstart + words1Count] = carry;
        }
      }
    }

    private static short ShiftWordsLeftByBits(
      short[] r,
      int rstart,
      int n,
      int shiftBits) {
#if DEBUG
      if (shiftBits >= 16) {
        throw new ArgumentException("doesn't satisfy shiftBits<16");
      }
#endif
      int u;
      var carry = 0;
      if (shiftBits != 0) {
        int sb16 = 16 - shiftBits;
        int rs = rstart;
        for (var i = 0; i < n; ++i, ++rs) {
          u = r[rs];
          r[rs] = unchecked((short)((u << shiftBits) | carry));
          carry = (u & 0xffff) >> sb16;
        }
      }
      return unchecked((short)carry);
    }

    private static void ShiftWordsLeftByWords(
      short[] r,
      int rstart,
      int n,
      int shiftWords) {
      shiftWords = Math.Min(shiftWords, n);
      if (shiftWords != 0) {
        for (int i = n - 1; i >= shiftWords; --i) {
          r[rstart + i] = r[rstart + i - shiftWords];
        }
        Array.Clear((short[])r, rstart, shiftWords);
      }
    }

    private static short ShiftWordsRightByBits(
      short[] r,
      int rstart,
      int n,
      int shiftBits) {
      short u, carry = 0;
      unchecked {
        if (shiftBits != 0) {
          for (int i = n; i > 0; --i) {
            u = r[rstart + i - 1];
            r[rstart + i - 1] = (short)((((((int)u) & 0xffff) >>
                    (int)shiftBits) & 0xffff) | (((int)carry) &
                    0xffff));
            carry = (short)((((int)u) & 0xffff) << (int)(16 - shiftBits));
          }
        }
        return carry;
      }
    }

    private static short ShiftWordsRightByBitsSignExtend(
      short[] r,
      int rstart,
      int n,
      int shiftBits) {
      unchecked {
        short u, carry = (short)((int)0xffff << (int)(16 - shiftBits));
        if (shiftBits != 0) {
          for (int i = n; i > 0; --i) {
            u = r[rstart + i - 1];
            r[rstart + i - 1] = (short)(((((int)u) & 0xffff) >>
                    (int)shiftBits) | (((int)carry) & 0xffff));
            carry = (short)((((int)u) & 0xffff) << (int)(16 - shiftBits));
          }
        }
        return carry;
      }
    }

    private static void ShiftWordsRightByWordsSignExtend(
      short[] r,
      int rstart,
      int n,
      int shiftWords) {
      shiftWords = Math.Min(shiftWords, n);
      if (shiftWords != 0) {
        for (var i = 0; i + shiftWords < n; ++i) {
          r[rstart + i] = r[rstart + i + shiftWords];
        }
        rstart += n - shiftWords;
        // Sign extend
        for (var i = 0; i < shiftWords; ++i) {
          r[rstart + i] = unchecked((short)0xffff);
        }
      }
    }

    private static short[] ShortenArray(short[] reg, int wordCount) {
      if (reg.Length > 32) {
        int newLength = wordCount;
        if (newLength < reg.Length && (reg.Length - newLength) >= 16) {
          // Reallocate the array if the desired length
          // is much smaller than the current length
          var newreg = new short[newLength];
          Array.Copy(reg, newreg, Math.Min(newLength, reg.Length));
          reg = newreg;
        }
      }
      return reg;
    }

    private static int SubtractWords1IsOneBigger(
      short[] c,
      int cstart,
      short[] words1,
      int astart,
      short[] words2,
      int bstart,
      int words1Count) {
      // Assumes that words2's count is 1 less
      unchecked {
        int u;
        u = 0;
        int cm1 = words1Count - 1;
        for (var i = 0; i < cm1; i += 1) {
          u = (((int)words1[astart]) & 0xffff) - (((int)words2[bstart]) &
                    0xffff) - (int)((u >> 31) & 1);
          c[cstart++] = (short)u;
          ++astart;
          ++bstart;
        }
        u = (((int)words1[astart]) & 0xffff) - (int)((u >> 31) & 1);
        c[cstart++] = (short)u;
        return (int)((u >> 31) & 1);
      }
    }

    private static int SubtractWords2IsOneBigger(
      short[] c,
      int cstart,
      short[] words1,
      int astart,
      short[] words2,
      int bstart,
      int words2Count) {
      // Assumes that words1's count is 1 less
      int u;
      u = 0;
      int cm1 = words2Count - 1;
      for (var i = 0; i < cm1; i += 1) {
        u = unchecked((((int)words1[astart]) & 0xffff) -
              (((int)words2[bstart]) & 0xffff) - (int)((u >> 31) & 1));
        c[cstart++] = unchecked((short)u);
        ++astart;
        ++bstart;
      }
      u = 0 - unchecked((((int)words2[bstart]) & 0xffff) - (int)((u >> 31) &
            1));
      c[cstart++] = unchecked((short)u);
      return (int)((u >> 31) & 1);
    }

    private static int SubtractInternal(
      short[] c,
      int cstart,
      short[] words1,
      int astart,
      short[] words2,
      int bstart,
      int n) {
      var u = 0;
      bool odd = (n & 1) != 0;
      if (odd) {
        --n;
      }
      var mask = 0xffff;
      for (var i = 0; i < n; i += 2) {
        int wb0 = words2[bstart] & mask;
        int wb1 = words2[bstart + 1] & mask;
        int wa0 = words1[astart] & mask;
        int wa1 = words1[astart + 1] & mask;
        u = unchecked(wa0 - wb0 - (int)((u >> 31) & 1));
        c[cstart++] = unchecked((short)u);
        u = unchecked(wa1 - wb1 - (int)((u >> 31) & 1));
        c[cstart++] = unchecked((short)u);
        astart += 2;
        bstart += 2;
      }
      if (odd) {
        u = unchecked((((int)words1[astart]) & mask) -
              (((int)words2[bstart]) & mask) - (int)((u >> 31) & 1));
        c[cstart++] = unchecked((short)u);
        ++astart;
        ++bstart;
      }
      return (int)((u >> 31) & 1);
    }

    private static void TwosComplement(short[] words1, int words1Start, int n) {
      Decrement(words1, words1Start, n, (short)1);
      for (var i = 0; i < n; ++i) {
        words1[words1Start + i] = unchecked((short)(~words1[words1Start + i]));
      }
    }

    private int ByteCount() {
      int wc = this.wordCount;
      if (wc == 0) {
        return 0;
      }
      short s = this.words[wc - 1];
      wc = (wc - 1) << 1;
      return (s == 0) ? wc : (((s >> 8) == 0) ? wc + 1 : wc + 2);
    }

    private bool HasSmallValue() {
      var c = (int)this.wordCount;
      if (c > 4) {
        return false;
      }
      if (c == 4 && (this.words[3] & 0x8000) != 0) {
        return this.negative && this.words[3] == unchecked((short)0x8000) &&
          this.words[2] == 0 && this.words[1] == 0 &&
          this.words[0] == 0;
      }
      return true;
    }

    private int PositiveCompare(EInteger t) {
      int size = this.wordCount, tempSize = t.wordCount;
      return (
        size == tempSize) ? Compare(
        this.words,
        0,
        t.words,
        0,
        (int)size) : (size > tempSize ? 1 : -1);
    }

    private string SmallValueToString() {
      long value = this.ToInt64Unchecked();
      if (value == Int64.MinValue) {
        return "-9223372036854775808";
      }
      if (value == (long)Int32.MinValue) {
        return "-2147483648";
      }
      bool neg = value < 0;
      var count = 0;
      char[] chars;
      int intvalue = unchecked((int)value);
      if ((long)intvalue == value) {
        chars = new char[12];
        count = 11;
        if (neg) {
          intvalue = -intvalue;
        }
        while (intvalue > 43698) {
          int intdivvalue = intvalue / 10;
          char digit = Digits[(int)(intvalue - (intdivvalue * 10))];
          chars[count--] = digit;
          intvalue = intdivvalue;
        }
        while (intvalue > 9) {
          int intdivvalue = (intvalue * 26215) >> 18;
          char digit = Digits[(int)(intvalue - (intdivvalue * 10))];
          chars[count--] = digit;
          intvalue = intdivvalue;
        }
        if (intvalue != 0) {
          chars[count--] = Digits[intvalue];
        }
        if (neg) {
          chars[count] = '-';
        } else {
          ++count;
        }
        return new String(chars, count, 12 - count);
      } else {
        chars = new char[24];
        count = 23;
        if (neg) {
          value = -value;
        }
        while (value > 9) {
          long divvalue = value / 10;
          char digit = Digits[(int)(value - (divvalue * 10))];
          chars[count--] = digit;
          value = divvalue;
        }
        if (value != 0) {
          chars[count--] = Digits[(int)value];
        }
        if (neg) {
          chars[count] = '-';
        } else {
          ++count;
        }
        return new String(chars, count, 24 - count);
      }
    }

    private EInteger[] SqrtRemInternal(bool useRem) {
      if (this.Sign <= 0) {
        return new[] { EInteger.Zero, EInteger.Zero };
      }
      if (this.Equals(EInteger.One)) {
        return new[] { EInteger.One, EInteger.Zero };
      }
      EInteger bigintX;
      EInteger bigintY;
      EInteger thisValue = this;
      int powerBits = (thisValue.GetUnsignedBitLength() + 1) / 2;
      if (thisValue.CanFitInInt32()) {
        int smallValue = thisValue.ToInt32Checked();
        // No need to check for ValueZero; already done above
        var smallintX = 0;
        int smallintY = 1 << powerBits;
        do {
          smallintX = smallintY;
          smallintY = smallValue / smallintX;
          smallintY += smallintX;
          smallintY >>= 1;
        } while (smallintY < smallintX);
        if (!useRem) {
          return new[] { (EInteger)smallintX, null };
        }
        smallintY = smallintX * smallintX;
        smallintY = smallValue - smallintY;
        return new[] {
          (EInteger)smallintX, (EInteger)smallintY };
      }
      if (this.wordCount >= 4) {
        int wordsPerPart = (this.wordCount + 3) >> 2;
        int bitsPerPart = wordsPerPart * 16;
        int totalBits = bitsPerPart * 4;
        int bitLength = this.GetUnsignedBitLength();
        bool bitLengthEven = (bitLength & 1) == 0;
        bigintX = this;
        var shift = 0;
        if (bitLength < totalBits - 1) {
          int targetLength = bitLengthEven ? totalBits : (totalBits - 1);
          shift = targetLength - bitLength;
          bigintX = bigintX.ShiftLeft(shift);
        }
        // DebugUtility.Log("this=" + (this.ToRadixString(16)));
        // DebugUtility.Log("bigx=" + (bigintX.ToRadixString(16)));
        short[] ww = bigintX.words;
        var w1 = new short[wordsPerPart];
        var w2 = new short[wordsPerPart];
        var w3 = new short[wordsPerPart * 2];
        Array.Copy(ww, 0, w1, 0, wordsPerPart);
        Array.Copy(ww, wordsPerPart, w2, 0, wordsPerPart);
        Array.Copy(ww, wordsPerPart * 2, w3, 0, wordsPerPart * 2);
        #if DEBUG
if (!((ww[(wordsPerPart * 4) - 1] & 0xc000) != 0)) {
  throw new
    ArgumentException("doesn't satisfy (ww[wordsPerPart*4-1]&0xC000)!=0");
}
#endif

        var e1 = new EInteger(CountWords(w1), w1, false);
        var e2 = new EInteger(CountWords(w2), w2, false);
        var e3 = new EInteger(CountWords(w3), w3, false);
        EInteger[] srem = e3.SqrtRemInternal(true);
        // DebugUtility.Log("sqrt0({0})[depth={3}] = {1},{2}"
        // , e3, srem[0], srem[1], 0);
        // DebugUtility.Log("sqrt1({0})[depth={3}] = {1},{2}"
        // , e3, srem2[0], srem2[1], 0);
        // if (!srem[0].Equals(srem2[0]) || !srem[1].Equals(srem2[1])) {
  // throw new InvalidOperationException(this.ToString());
   // }
        EInteger[] qrem = srem[1].ShiftLeft(bitsPerPart).Add(e2).DivRem(
           srem[0].ShiftLeft(1));
        EInteger sqroot = srem[0].ShiftLeft(bitsPerPart).Add(qrem[0]);
        EInteger sqrem = qrem[1].ShiftLeft(bitsPerPart).Add(e1).Subtract(
           qrem[0].Multiply(qrem[0]));
        // DebugUtility.Log("sqrem=" + sqrem + ",sqroot=" + sqroot);
        if (sqrem.Sign < 0) {
          if (useRem) {
            sqrem = sqrem.Add(sqroot.ShiftLeft(1)).Subtract(EInteger.One);
          }
          sqroot = sqroot.Subtract(EInteger.One);
          #if DEBUG
if (!(sqroot.Sign >= 0)) {
  throw new ArgumentException("doesn't satisfy sqroot.Sign>= 0");
}
#endif
        }
        /*

  DebugUtility.Log("sqrt({0}) = {1},{2},\n---shift={3},words={4},wpp={5},bxwords={6}",

  this, sqroot, sqrem, shift, this.wordCount, wordsPerPart,
   bigintX.wordCount);
        if (useRem) {
         DebugUtility.Log("srshHalf=" + (sqrem.ShiftRight(shift>>1)));
         DebugUtility.Log("srshFull=" + (sqrem.ShiftRight(shift)));
        }
        */
        var retarr = new EInteger[2];
        retarr[0] = sqroot.ShiftRight(shift >> 1);
        if (useRem) {
          if (shift == 0) {
            retarr[1] = sqrem;
          } else {
            retarr[1] = this.Subtract(retarr[0].Multiply(retarr[0]));
          }
        }
        return retarr;
      }
      bigintX = EInteger.Zero;
      bigintY = EInteger.One << powerBits;
      do {
        bigintX = bigintY;
        // DebugUtility.Log("" + thisValue + " " + bigintX);
        bigintY = thisValue / (EInteger)bigintX;
        bigintY += bigintX;
        bigintY >>= 1;
      } while (bigintY != null && bigintY.CompareTo(bigintX) < 0);
      if (!useRem) {
        return new[] { bigintX, null };
      }
      bigintY = bigintX * (EInteger)bigintX;
      bigintY = thisValue - (EInteger)bigintY;
      return new[] {
        bigintX, bigintY };
    }
    // Begin integer conversions

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ToByteChecked"]/*'/>
    public byte ToByteChecked() {
      int val = this.ToInt32Checked();
      if (val < 0 || val > 255) {
        throw new OverflowException("This object's value is out of range");
      }
      return unchecked((byte)(val & 0xff));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ToByteUnchecked"]/*'/>
    public byte ToByteUnchecked() {
      int val = this.ToInt32Unchecked();
      return unchecked((byte)(val & 0xff));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.FromByte(System.Byte)"]/*'/>
    public static EInteger FromByte(byte inputByte) {
      int val = ((int)inputByte) & 0xff;
      return FromInt32(val);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ToInt16Checked"]/*'/>
    public short ToInt16Checked() {
      int val = this.ToInt32Checked();
      if (val < -32768 || val > 32767) {
        throw new OverflowException("This object's value is out of range");
      }
      return unchecked((short)(val & 0xffff));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.ToInt16Unchecked"]/*'/>
    public short ToInt16Unchecked() {
      int val = this.ToInt32Unchecked();
      return unchecked((short)(val & 0xffff));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.EInteger.FromInt16(System.Int16)"]/*'/>
    public static EInteger FromInt16(short inputInt16) {
      var val = (int)inputInt16;
      return FromInt32(val);
    }

    // End integer conversions
  }
}
