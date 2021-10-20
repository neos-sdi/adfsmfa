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
  internal sealed class DigitShiftAccumulator : IShiftAccumulator {
    private static readonly long[] TenPowersLong = {
      1L, 10L, 100L, 1000L, 10000L, 100000L,
      1000000L, 10000000L, 100000000L,
      1000000000L,
      10000000000L,
      100000000000L,
      1000000000000L,
      10000000000000L,
      100000000000000L,
      1000000000000000L,
      10000000000000000L,
      100000000000000000L,
      1000000000000000000L,
    };

    private static readonly EInteger ValueTen = (EInteger)10;

    private static readonly int[] ValueTenPowers = {
      1, 10, 100, 1000, 10000, 100000,
      1000000, 10000000, 100000000,
    };

    private int bitLeftmost;

    private int bitsAfterLeftmost;
    private FastInteger discardedDigitCount;
    private bool isSmall;
    private FastInteger knownDigitLength;

    private EInteger shiftedBigInt;

    private int shiftedSmall;

    public override string ToString() {
      return "[this.bitLeftmost=" + this.bitLeftmost +
        ", this.bitsAfterLeftmost=" + this.bitsAfterLeftmost +
        ", this.discardedDigitCount=" + this.discardedDigitCount +
        ", this.isSmall=" + this.isSmall + ", this.knownDigitLength=" +
        this.knownDigitLength + ", this.shiftedBigInt=" +
        this.shiftedBigInt + ", this.shiftedSmall=" +
        this.shiftedSmall + "]";
    }

    public DigitShiftAccumulator(
      EInteger bigint,
      int lastDiscarded,
      int olderDiscarded) {
      if (bigint.CanFitInInt32()) {
        this.shiftedSmall = (int)bigint;
        if (this.shiftedSmall < 0) {
          throw new ArgumentException("shiftedSmall(" + this.shiftedSmall +
            ") is less than 0");
        }
        this.isSmall = true;
      } else {
        this.shiftedBigInt = bigint;
        this.isSmall = false;
      }
      this.bitsAfterLeftmost = (olderDiscarded != 0) ? 1 : 0;
      this.bitLeftmost = lastDiscarded;
    }

    public DigitShiftAccumulator(
      int smallint,
      int lastDiscarded,
      int olderDiscarded) {
      this.shiftedSmall = smallint;
      if (this.shiftedSmall < 0) {
        throw new ArgumentException("shiftedSmall(" + this.shiftedSmall +
          ") is less than 0");
      }
      this.isSmall = true;
      this.bitsAfterLeftmost = (olderDiscarded != 0) ? 1 : 0;
      this.bitLeftmost = lastDiscarded;
    }

    public FastInteger DiscardedDigitCount {
      get {
        this.discardedDigitCount = this.discardedDigitCount ?? new
FastInteger(0);
        return this.discardedDigitCount;
      }
    }

    public int LastDiscardedDigit {
      get {
        return this.bitLeftmost;
      }
    }

    public int OlderDiscardedDigits {
      get {
        return this.bitsAfterLeftmost;
      }
    }

    public EInteger ShiftedInt {
      get {
        return this.isSmall ? ((EInteger)this.shiftedSmall) :
          this.shiftedBigInt;
      }
    }

    public int ShiftedIntMod(int mod) {
#if DEBUG
      if (mod < 1) {
        throw new ArgumentException("mod (" + mod +
           ") is not greater or equal to 1");
      }
#endif
      switch (mod) {
        case 1:
          return 0;
        case 2:
          return this.isSmall ? (this.shiftedSmall & 1) :
            (this.shiftedBigInt.IsEven ? 0 : 1);
        default:
          return this.isSmall ? (this.shiftedSmall % mod) :
            this.shiftedBigInt.Mod(mod).ToInt32Checked();
      }
    }

    public FastInteger ShiftedIntFast {
      get {
        return this.isSmall ? new FastInteger(this.shiftedSmall) :
          FastInteger.FromBig(this.shiftedBigInt);
      }
    }

    public FastInteger GetDigitLength() {
      this.knownDigitLength = this.knownDigitLength ??
        this.CalcKnownDigitLength();
      return this.knownDigitLength;
    }

    public FastInteger OverestimateDigitLength() {
      // If digit length is known, return it
      if (this.knownDigitLength != null) {
        return this.knownDigitLength;
      }
      if (this.isSmall) {
        // Can easily be calculated without estimation
        return this.GetDigitLength();
      } else {
        return NumberUtility.DecimalDigitLengthBounds(this.shiftedBigInt)[1];
      }
    }

    private FastInteger UnderestimateDigitLength() {
      // If digit length is known, return it
      if (this.knownDigitLength != null) {
        return this.knownDigitLength;
      }
      if (this.isSmall) {
        // Can easily be calculated without estimation
        return this.GetDigitLength();
      } else {
        return NumberUtility.DecimalDigitLengthBounds(this.shiftedBigInt)[0];
      }
    }

    public void ShiftRight(FastInteger fastint) {
      if (fastint == null) {
        throw new ArgumentNullException(nameof(fastint));
      }
      if (fastint.CanFitInInt32()) {
        int fi = fastint.ToInt32();
        if (fi < 0) {
          return;
        }
        this.ShiftRightInt(fi);
      } else {
        if (fastint.Sign <= 0) {
          return;
        }
        EInteger digitsToShift = fastint.ToEInteger();
        while (digitsToShift.Sign > 0) {
          if (digitsToShift.CompareTo(1000000) >= 0 &&
             (this.isSmall ||
              this.shiftedBigInt.GetUnsignedBitLengthAsEInteger()
               .CompareTo(digitsToShift) < 0)) {
            // Bit length is less than digits to shift, and digits to shift is >= 1000000,
            // so whole number would be shifted
            this.discardedDigitCount = this.discardedDigitCount ?? new
FastInteger(0);
            this.discardedDigitCount.AddBig(digitsToShift);
            this.bitsAfterLeftmost |= this.bitLeftmost;
            this.bitsAfterLeftmost |= (this.isSmall ? this.shiftedSmall == 0 :
this.shiftedBigInt.IsZero) ? 0 : 1;
            this.bitLeftmost = 0;
            this.knownDigitLength = new FastInteger(1);
            this.isSmall = true;
            this.shiftedSmall = 0;
            return;
          }
          var count = 1000000;
          if (digitsToShift.CompareTo(1000000) < 0) {
            count = (int)digitsToShift;
          }
          this.ShiftRightInt(count);
          digitsToShift -= (EInteger)count;
          if (this.isSmall ? this.shiftedSmall == 0 :
            this.shiftedBigInt.IsZero) {
            return;
          }
        }
      }
    }

    public void ShiftRightInt(int digits) {
      // <summary>Shifts a number to the right, gathering information on
      // whether the last digit discarded is set and whether the discarded
      // digits to the right of that digit are set. Assumes that the big
      // integer being shifted is positive.</summary>
      if (this.isSmall) {
        this.ShiftRightSmall(digits);
      } else {
        this.ShiftRightBig(digits, false, false);
      }
    }

    public void ShiftToDigits(
      FastInteger bits,
      FastInteger preShift,
      bool truncate) {
      #if DEBUG
      if (bits.Sign < 0) {
        throw new ArgumentException("bits's sign(" + bits.Sign +
          ") is less than 0");
      }
      #endif
      if (preShift != null && preShift.Sign > 0) {
        FastInteger kdl = this.knownDigitLength ?? this.CalcKnownDigitLength();
        this.knownDigitLength = kdl;
        // DebugUtility.Log("bits=" + bits + " pre=" + preShift + " known=" +
        // (//kdl) + " [" + this.shiftedBigInt + "]");
        if (kdl.CompareTo(bits) <= 0) {
          // Known digit length is already small enough
          this.TruncateOrShiftRight(preShift, truncate);
          return;
        } else {
          FastInteger bitDiff = kdl.Copy().Subtract(bits);
          // DebugUtility.Log("bitDiff=" + bitDiff);
          int cmp = bitDiff.CompareTo(preShift);
          if (cmp <= 0) {
            // Difference between desired digit length and current
            // length is smaller than the shift, make it the shift
            this.TruncateOrShiftRight(preShift, truncate);
            return;
          } else {
            this.TruncateOrShiftRight(bitDiff, truncate);
            return;
          }
        }
      }
      if (bits.CanFitInInt32()) {
        int intval = bits.ToInt32();
        if (intval < 0) {
          throw new ArgumentException("intval(" + intval + ") is less than " +
            "0");
        }
        if (this.isSmall) {
          this.ShiftToDigitsSmall(intval);
        } else {
          this.ShiftToDigitsBig(intval, truncate);
        }
      } else {
        FastInteger kdl = this.knownDigitLength ?? this.CalcKnownDigitLength();
        this.knownDigitLength = kdl;
        EInteger bigintDiff = kdl.ToEInteger();
        EInteger bitsBig = bits.ToEInteger();
        bigintDiff -= (EInteger)bitsBig;
        if (bigintDiff.Sign > 0) {
          // current length is greater than the
          // desired bit length
          this.ShiftRight(FastInteger.FromBig(bigintDiff));
        }
      }
    }

    public bool TruncateRightExact(FastInteger fastint) {
      if (fastint == null) {
        throw new ArgumentNullException(nameof(fastint));
      }
      if (fastint.CanFitInInt32()) {
        if (fastint.Sign < 0) {
          return (this.bitLeftmost | this.bitsAfterLeftmost) == 0;
        }
        if (!this.isSmall && !this.shiftedBigInt.CanFitInInt64()) {
          #if DEBUG
          if (this.shiftedBigInt == null) {
            throw new InvalidOperationException();
          }
          #endif
          int a = fastint.ToInt32();
          if (a > 10) {
            this.ShiftRightBig(10, true, true);
            if ((this.bitLeftmost | this.bitsAfterLeftmost) != 0) {
              return false;
            }
            if (this.isSmall) {
              this.ShiftRightInt(a - 10);
            } else {
              this.ShiftRightBig(a - 10, true, true);
            }
          } else {
            this.ShiftRightBig(a, true, true);
          }
          return (this.bitLeftmost | this.bitsAfterLeftmost) == 0;
        }
      }
      this.TruncateOrShiftRight(fastint, true);
      return (this.bitLeftmost | this.bitsAfterLeftmost) == 0;
    }

    public void TruncateRightSimple(FastInteger fastint) {
      if (fastint == null) {
        throw new ArgumentNullException(nameof(fastint));
      }
      if (fastint.CanFitInInt32()) {
        if (fastint.Sign < 0) {
          return;
        }
        if (!this.isSmall && !this.shiftedBigInt.CanFitInInt64()) {
          this.ShiftRightBig(fastint.ToInt32(), true, true);
          return;
        }
      }
      this.TruncateOrShiftRight(fastint, true);
    }

    public void TruncateOrShiftRight(FastInteger fastint, bool truncate) {
      // 'Truncate' is true if the caller doesn't care about the exact identity
      // of the last digit and the discarded digits.
      if (fastint == null) {
        throw new ArgumentNullException(nameof(fastint));
      }
      if (truncate && fastint.CanFitInInt32()) {
        int fi = fastint.ToInt32();
        if (fi < 0) {
          return;
        }
        if (!this.isSmall) {
          if (this.shiftedBigInt.CanFitInInt64()) {
            this.TruncateRightLong(this.shiftedBigInt.ToInt64Checked(), fi);
          } else {
            this.ShiftRightBig(fi, true, false);
          }
        } else {
          this.TruncateRightSmall(fi);
        }
      } else {
        this.ShiftRight(fastint);
      }
    }

    private static int FastParseLong(string str, int offset, int length) {
      // Assumes the string is length 9 or less and contains
      // only the digits '0' through '9'
      if (length > 9) {
        throw new ArgumentException("length(" + length + ") is more than " +
          "9 ");
      }
      var ret = 0;
      for (var i = 0; i < length; ++i) {
        var digit = (int)(str[offset + i] - '0');
        ret *= 10;
        ret += digit;
      }
      return ret;
    }

    private static EInteger DivideByPowerOfTen(EInteger ei, int pow) {
      // if (pow > 100) {
      // int mid = pow / 2;
      // ei = DivideByPowerOfTen(ei, pow - mid);
      // return DivideByPowerOfTen(ei, mid);
      // } else {
      return ei.Divide(NumberUtility.FindPowerOfTen(pow));
      // }
    }

    private FastInteger CalcKnownDigitLength() {
      if (this.isSmall) {
        int kb = NumberUtility.DecimalDigitLength(this.shiftedSmall);
        return new FastInteger(kb);
      } else {
        long digits = this.shiftedBigInt.GetDigitCountAsInt64();
        if (digits == Int64.MaxValue) {
           return
FastInteger.FromBig(this.shiftedBigInt.GetDigitCountAsEInteger());
        } else if (digits < Int32.MaxValue) {
          return new FastInteger((int)digits);
        } else {
           return FastInteger.FromBig(EInteger.FromInt64(digits));
        }
      }
    }

    private void UpdateKnownLengthInt(int digits) {
      if (this.knownDigitLength != null) {
        this.knownDigitLength.SubtractInt(digits);
        if (this.knownDigitLength.CompareToInt(1) < 0) {
          this.knownDigitLength.SetInt(1);
        }
      }
    }

    private void UpdateKnownLength(FastInteger digitsShiftedFast) {
      if (this.knownDigitLength != null) {
        this.knownDigitLength.Subtract(digitsShiftedFast);
        if (this.knownDigitLength.CompareToInt(1) < 0) {
          this.knownDigitLength.SetInt(1);
        }
      }
    }

    private void ShiftRightBig(int digits, bool truncate, bool simple) {
      #if DEBUG
      if (this.shiftedBigInt == null) {
        throw new InvalidOperationException();
      }
      #endif
      if (digits <= 0) {
        return;
      }
      // DebugUtility.Log("ShiftRightBig "+digits+" "+truncate+" "+
      // simple+" "+this);
      if (this.shiftedBigInt.IsZero) {
        this.discardedDigitCount = this.discardedDigitCount ?? new
FastInteger(0);
        this.discardedDigitCount.AddInt(digits);
        this.bitsAfterLeftmost |= this.bitLeftmost;
        this.bitLeftmost = 0;
        this.knownDigitLength = new FastInteger(1);
        return;
      }
      if (truncate) {
        EInteger bigquo;
        {
          // To avoid having to calculate a very big power of 10,
          // or the digit length of a very big integer,
          // check the digit count to see if doing so can be avoided
          EInteger bigBitLength =
            this.shiftedBigInt.GetUnsignedBitLengthAsEInteger();
          var bigPower = false;
          if (digits > 50 &&
              bigBitLength.CompareTo(100) > 0 &&
              bigBitLength.Add(5).CompareTo(digits) < 0) {
            // Has much fewer bits than digits to shift, so all of them
            // will be shifted to the right
            bigPower = true;
          } else {
          // NOTE: Overflowing bigBitLength will be MaxValue, which is OK
          // for the use of this variable
          int bitLength = bigBitLength.CanFitInInt32() ?
            bigBitLength.ToInt32Checked() : Int32.MaxValue;
          // 10^48 has 160 bits; 10^98 has 326; bit length is cheaper
          // to calculate than base-10 digit length
          if ((digits > 50 && bitLength < 160) ||
              (digits > 100 && bitLength < 326)) {
            bigPower = true;
          } else {
            FastInteger digitsUpperBound = this.OverestimateDigitLength();
            bigPower = digitsUpperBound.Copy().SubtractInt(digits)
              .CompareToInt(-2) < 0;
          }
          }
          if (bigPower) {
            // Power of 10 to be divided would be much bigger
            this.discardedDigitCount = this.discardedDigitCount ??
              new FastInteger(0);
            this.discardedDigitCount.AddInt(digits);
            this.bitsAfterLeftmost |= this.bitLeftmost;
            this.bitsAfterLeftmost |= this.shiftedBigInt.IsZero ? 0 : 1;
            this.bitLeftmost = 0;
            this.knownDigitLength = new FastInteger(1);
            this.isSmall = true;
            this.shiftedSmall = 0;
            return;
          }
        }
        if (!simple || (this.ShiftedIntMod(2) == 0 && this.bitLeftmost == 0)) {
          EInteger[] quorem = this.shiftedBigInt.DivRem(
              NumberUtility.FindPowerOfTen(digits));
          bigquo = quorem[0];
          this.bitLeftmost |= quorem[1].IsZero ? 0 : 1;
        } else {
          this.bitLeftmost = 1;
          bigquo = this.shiftedBigInt.Divide(
              NumberUtility.FindPowerOfTen(digits));
        }
        this.bitsAfterLeftmost |= this.bitLeftmost;
        this.discardedDigitCount = this.discardedDigitCount == null ?
          new FastInteger(digits) : this.discardedDigitCount.AddInt(digits);
        if (bigquo.IsZero) {
          // Shifted all the way to 0
          this.isSmall = true;
          this.shiftedBigInt = null;
          this.shiftedSmall = 0;
          this.knownDigitLength = new FastInteger(1);
        } else if (bigquo.CanFitInInt32()) {
          this.isSmall = true;
          this.shiftedSmall = bigquo.ToInt32Unchecked();
          this.shiftedBigInt = null;
          this.UpdateKnownLengthInt(digits);
        } else {
          this.isSmall = false;
          this.shiftedBigInt = bigquo;
          this.UpdateKnownLengthInt(digits);
        }
        return;
      }
      if (digits == 1) {
        EInteger bigrem;
        EInteger bigquo;
        EInteger[] divrem = this.shiftedBigInt.DivRem((EInteger)10);
        bigquo = divrem[0];
        bigrem = divrem[1];
        this.bitsAfterLeftmost |= this.bitLeftmost;
        this.bitLeftmost = (int)bigrem;
        this.shiftedBigInt = bigquo;
        this.discardedDigitCount = this.discardedDigitCount ?? new
FastInteger(0);
        this.discardedDigitCount.AddInt(digits);
        this.UpdateKnownLengthInt(digits);
        return;
      }
      if (digits >= 2 && digits <= 8) {
        EInteger bigrem;
        EInteger bigquo;
        EInteger[] divrem =
          this.shiftedBigInt.DivRem(NumberUtility.FindPowerOfTen(digits));
        bigquo = divrem[0];
        bigrem = divrem[1];
        var intRem = (int)bigrem;
        int smallPower = ValueTenPowers[digits - 1];
        int leftBit = intRem / smallPower;
        int otherBits = intRem - (leftBit * smallPower);
        this.bitsAfterLeftmost |= otherBits | this.bitLeftmost;
        this.bitLeftmost = leftBit;
        this.shiftedBigInt = bigquo;
        this.discardedDigitCount = (this.discardedDigitCount != null) ?
          this.discardedDigitCount.AddInt(digits) : new FastInteger(digits);
        this.UpdateKnownLengthInt(digits);
        this.bitsAfterLeftmost = (this.bitsAfterLeftmost != 0) ? 1 : 0;
        if (this.shiftedBigInt.CanFitInInt32()) {
          this.isSmall = true;
          this.shiftedSmall = this.shiftedBigInt.ToInt32Unchecked();
          this.shiftedBigInt = null;
        }
        return;
      }
      this.knownDigitLength = this.knownDigitLength ??
        this.CalcKnownDigitLength();
      // DebugUtility.Log("kdl="+this.knownDigitLength);
      if (new FastInteger(digits).Decrement().CompareTo(
         this.knownDigitLength) >= 0) {
        // Shifting more bits than available
        this.bitsAfterLeftmost |= this.shiftedBigInt.IsZero ? 0 : 1;
        this.isSmall = true;
        this.shiftedSmall = 0;
        this.knownDigitLength = new FastInteger(1);
        this.discardedDigitCount = this.discardedDigitCount ?? new
FastInteger(0);
        this.discardedDigitCount.AddInt(digits);
        this.bitsAfterLeftmost |= this.bitLeftmost;
        this.bitLeftmost = 0;
        return;
      }
      if (this.shiftedBigInt.CanFitInInt32()) {
        this.isSmall = true;
        this.shiftedSmall = (int)this.shiftedBigInt;
        this.ShiftRightSmall(digits);
        return;
      }
      if (this.shiftedBigInt.CanFitInInt64()) {
        this.ShiftRightLong(this.shiftedBigInt.ToInt64Unchecked(), digits);
        return;
      }
      EInteger sbi = this.shiftedBigInt;
      EInteger[] divrem1 = sbi.DivRem(NumberUtility.FindPowerOfTen(
        digits - 1));
      EInteger[] divrem2 = divrem1[0].DivRem(10);
      // DebugUtility.Log("divrem " + (// divrem1[0]) + " " + divrem1[1] + " / " +
      // divrem2[0] + " " + (divrem2[1]));
      this.bitsAfterLeftmost |= this.bitLeftmost;
      this.bitsAfterLeftmost |= divrem1[1].IsZero ? 0 : 1;
      this.bitLeftmost = divrem2[1].ToInt32Checked();
      this.discardedDigitCount = (this.discardedDigitCount != null) ?
          this.discardedDigitCount.AddInt(digits) : new FastInteger(digits);
      this.UpdateKnownLengthInt(digits);
      if (divrem2[0].CanFitInInt32()) {
        this.isSmall = true;
        this.shiftedSmall = divrem2[0].ToInt32Checked();
      } else {
        this.isSmall = false;
        this.shiftedBigInt = divrem2[0];
      }
    }

    private void ShiftRightLong(long shiftedLong, int digits) {
      if (digits <= 0) {
        return;
      }
      if (shiftedLong == 0) {
        this.shiftedSmall = 0;
        this.isSmall = true;
        this.discardedDigitCount = this.discardedDigitCount ?? new
FastInteger(0);
        this.discardedDigitCount.AddInt(digits);
        this.bitsAfterLeftmost |= this.bitLeftmost;
        this.bitLeftmost = 0;
        this.knownDigitLength = new FastInteger(1);
        return;
      }

      if (digits >= 2 && digits <= 8) {
        if (shiftedLong >= ValueTenPowers[digits]) {
          long bigPower = ValueTenPowers[digits];
          long smallPower = ValueTenPowers[digits - 1];
          this.discardedDigitCount = this.discardedDigitCount ?? new
FastInteger(
              0);
          this.discardedDigitCount.AddInt(digits);
          long div = shiftedLong / bigPower;
          long rem = shiftedLong - (div * bigPower);
          long rem2 = rem / smallPower;
          this.bitLeftmost = (int)rem2;
          this.bitsAfterLeftmost |= ((rem - (rem2 * smallPower)) == 0) ? 0 : 1;
          this.isSmall = div <= Int32.MaxValue;
          if (this.isSmall) {
            this.shiftedSmall = (int)div;
            this.knownDigitLength = (div < 10) ? new FastInteger(1) :
              new FastInteger(NumberUtility.DecimalDigitLength(div));
          } else {
            this.shiftedBigInt = EInteger.FromInt64(div);
            this.knownDigitLength = (div < 10) ? new FastInteger(1) :
              this.CalcKnownDigitLength();
          }
          return;
        } else if (this.shiftedSmall >= ValueTenPowers[digits - 1]) {
          int smallPower = ValueTenPowers[digits - 1];
          if (this.discardedDigitCount != null) {
            this.discardedDigitCount.AddInt(digits);
          } else {
            this.discardedDigitCount = new FastInteger(digits);
          }
          long rem = shiftedLong;
          long rem2 = rem / smallPower;
          this.bitLeftmost = (int)rem2;
          this.bitsAfterLeftmost |= ((rem - (rem2 * smallPower)) == 0) ? 0 : 1;
          this.isSmall = true;
          this.shiftedSmall = 0;
          this.knownDigitLength = new FastInteger(1);
          return;
        } else {
          if (this.discardedDigitCount != null) {
            this.discardedDigitCount.AddInt(digits);
          } else {
            this.discardedDigitCount = new FastInteger(digits);
          }
          this.bitLeftmost = 0;
          this.bitsAfterLeftmost |= (shiftedLong == 0) ? 0 : 1;
          this.isSmall = true;
          this.shiftedSmall = 0;
          this.knownDigitLength = new FastInteger(1);
          return;
        }
      }
      this.knownDigitLength = new FastInteger(
        NumberUtility.DecimalDigitLength(shiftedLong));
      if (this.discardedDigitCount != null) {
        this.discardedDigitCount.AddInt(digits);
      } else {
        this.discardedDigitCount = new FastInteger(digits);
      }
      var digitsShifted = 0;
      while (digits > 0) {
        if (shiftedLong == 0) {
          this.bitsAfterLeftmost |= this.bitLeftmost;
          this.bitLeftmost = 0;
          break;
        } else {
          long newShift = (shiftedLong < 43698) ? ((shiftedLong * 26215) >>
              18) : (shiftedLong / 10);
          var digit = (int)(shiftedLong - (newShift * 10));
          this.bitsAfterLeftmost |= this.bitLeftmost;
          this.bitLeftmost = digit;
          --digits;
          ++digitsShifted;
          shiftedLong = newShift;
        }
      }
      this.isSmall = shiftedLong <= Int32.MaxValue;
      if (this.isSmall) {
        this.shiftedSmall = (int)shiftedLong;
      } else {
        this.shiftedBigInt = EInteger.FromInt64(shiftedLong);
      }
      this.UpdateKnownLengthInt(digitsShifted);
      this.bitsAfterLeftmost = (this.bitsAfterLeftmost != 0) ? 1 : 0;
    }

    private void ShiftToDigitsBig(int digits, bool truncate) {
      // Shifts a number until it reaches the given number of digits,
      // gathering information on whether the last digit discarded is set
      // and whether the discarded digits to the right of that digit are set.
      // Assumes that the big integer being shifted is positive.
      // 'Truncate' is true if the caller doesn't care about the exact identity
      // of the last digit and the discarded digits.
      if (this.knownDigitLength != null) {
        if (this.knownDigitLength.CompareToInt(digits) <= 0) {
          return;
        }
      }
      // DebugUtility.Log("ShiftToDigitsBig(" + digits + ")");
      // var sw = new System.Diagnostics.Stopwatch();sw.Restart();
      string str;
      FastInteger estDigitLength = this.UnderestimateDigitLength();
      bool haveKnownDigitLength = this.knownDigitLength != null;
      #if DEBUG
      // if (this.CalcKnownDigitLength().CompareTo(estDigitLength) < 0) {
      // throw new InvalidOperationException();
      // }
      #endif
      if (estDigitLength.CompareToInt(digits) <= 0) {
        if (!haveKnownDigitLength) {
          this.GetDigitLength();
          this.ShiftToDigitsBig(digits, truncate);
        }
        #if DEBUG
        if (this.knownDigitLength.CompareToInt(digits) > 0) {
          throw new InvalidOperationException();
        }
        #endif
        return;
      }
      FastInteger digitDiff = estDigitLength.Copy().SubtractInt(digits);
      if (truncate && digitDiff.CanFitInInt32()) {
        // DebugUtility.Log("d=" + sw.ElapsedMilliseconds + " ms");
        this.TruncateOrShiftRight(digitDiff, truncate);
        if (!haveKnownDigitLength) {
          this.GetDigitLength();
          this.ShiftToDigitsBig(digits, truncate);
        }
        #if DEBUG
        if (this.knownDigitLength.CompareToInt(digits) > 0) {
          throw new InvalidOperationException();
        }
        #endif
        return;
      }
      if (digitDiff.CompareToInt(1) == 0) {
        EInteger bigrem;
        EInteger bigquo;
        EInteger[] divrem = this.shiftedBigInt.DivRem(ValueTen);
        bigquo = divrem[0];
        bigrem = divrem[1];
        this.bitsAfterLeftmost |= this.bitLeftmost;
        this.bitLeftmost = (int)bigrem;
        this.shiftedBigInt = bigquo;
        this.discardedDigitCount = this.discardedDigitCount ?? new
FastInteger(0);
        this.discardedDigitCount.Add(digitDiff);
        this.bitsAfterLeftmost = (this.bitsAfterLeftmost != 0) ? 1 : 0;
        if (!haveKnownDigitLength) {
          this.GetDigitLength();
          this.ShiftToDigitsBig(digits, truncate);
        } else {
          this.UpdateKnownLength(digitDiff);
        }
        #if DEBUG
        if (this.knownDigitLength.CompareToInt(digits) > 0) {
          throw new InvalidOperationException();
        }
        #endif
        return;
      }
      if (digitDiff.CompareToInt(9) <= 0) {
        EInteger bigrem;
        int diffInt = digitDiff.ToInt32();
        EInteger radixPower = NumberUtility.FindPowerOfTen(diffInt);
        EInteger bigquo;
        EInteger[] divrem = this.shiftedBigInt.DivRem(radixPower);
        bigquo = divrem[0];
        bigrem = divrem[1];
        var rem = (int)bigrem;
        this.bitsAfterLeftmost |= this.bitLeftmost;
        for (var i = 0; i < diffInt; ++i) {
          if (i == diffInt - 1) {
            this.bitLeftmost = rem % 10;
          } else {
            int intQuot = (rem < 43698) ? ((rem * 26215) >> 18) : (rem / 10);
            this.bitsAfterLeftmost |= rem - (intQuot * 10);
            rem = intQuot;
          }
        }
        this.shiftedBigInt = bigquo;
        this.discardedDigitCount = this.discardedDigitCount ?? new
FastInteger(0);
        this.discardedDigitCount.Add(digitDiff);
        this.bitsAfterLeftmost = (this.bitsAfterLeftmost != 0) ? 1 : 0;
        if (!haveKnownDigitLength) {
          this.GetDigitLength();
          this.ShiftToDigitsBig(digits, truncate);
        } else {
          this.UpdateKnownLength(digitDiff);
        }
        #if DEBUG
        if (this.knownDigitLength.CompareToInt(digits) > 0) {
          throw new InvalidOperationException();
        }
        #endif
        return;
      }
      // DebugUtility.Log("e1=" + sw.ElapsedMilliseconds + " ms");
      if (digitDiff.CanFitInInt32()) {
        #if DEBUG
        if (!(digitDiff.CompareToInt(2) > 0)) {
          throw new ArgumentException(
            "doesn't satisfy digitDiff.CompareToInt(2)>0");
        }
        #endif
        EInteger bigrem = null;
        EInteger bigquo;
        EInteger[] divrem;
        EInteger radixPower;
        int power = digitDiff.ToInt32() - 1;
        if (!this.shiftedBigInt.IsEven || this.bitsAfterLeftmost != 0) {
          // DebugUtility.Log("f=" + sw.ElapsedMilliseconds + " ms [pow=" + power +
          // "]");
          bigquo = this.shiftedBigInt;
          // DebugUtility.Log("fa=" + sw.ElapsedMilliseconds + " ms [" + (//
          // (!this.shiftedBigInt.IsEven || this.bitsAfterLeftmost != 0)) + "]");
          this.bitsAfterLeftmost |= 1;
          bigquo = bigquo.Divide(NumberUtility.FindPowerOfTen(power));
          // DebugUtility.Log("faa=" + sw.ElapsedMilliseconds + " ms");
        } else {
          // DebugUtility.Log("fb=" + sw.ElapsedMilliseconds + " ms [pow=" + power +
          // "]");
          radixPower = NumberUtility.FindPowerOfTen(power);
          // DebugUtility.Log("fc=" + sw.ElapsedMilliseconds + " ms [" + (//
          // (!this.shiftedBigInt.IsEven || this.bitsAfterLeftmost != 0)) + "]");
          divrem = this.shiftedBigInt.DivRem(radixPower);
          bigquo = divrem[0];
          bigrem = divrem[1];
          this.bitsAfterLeftmost |= this.bitLeftmost;
          if (!bigrem.IsZero) {
            this.bitsAfterLeftmost |= 1;
          }
        }
        // DebugUtility.Log("g=" + sw.ElapsedMilliseconds + " ms");
        EInteger bigquo2;
        divrem = bigquo.DivRem(ValueTen);
        bigquo2 = divrem[0];
        bigrem = divrem[1];
        this.bitLeftmost = (int)bigrem;
        this.shiftedBigInt = bigquo2;
        this.discardedDigitCount = this.discardedDigitCount ?? new
FastInteger(0);
        this.discardedDigitCount.Add(digitDiff);
        this.bitsAfterLeftmost = (this.bitsAfterLeftmost != 0) ? 1 : 0;
        // DebugUtility.Log("h=" + sw.ElapsedMilliseconds + " ms");
        if (!haveKnownDigitLength) {
          this.GetDigitLength();
          this.ShiftToDigitsBig(digits, truncate);
        } else {
          this.UpdateKnownLength(digitDiff);
        }
        #if DEBUG
        if (this.knownDigitLength.CompareToInt(digits) > 0) {
          throw new InvalidOperationException();
        }
        #endif
        return;
      }
      str = this.shiftedBigInt.ToString();
      // DebugUtility.Log("sdb=" + str + " digits="+digits);
      // NOTE: Will be 1 if the value is 0
      int digitLength = str.Length;
      this.knownDigitLength = new FastInteger(digitLength);
      // Shift by the difference in digit length
      if (digitLength > digits) {
        int digitShift = digitLength - digits;
        this.UpdateKnownLengthInt(digitShift);
        var newLength = (int)(digitLength - digitShift);
        // Console.WriteLine("dlen= " + digitLength + " dshift=" +
        // digitShift + " newlen= " + newLength);
        this.discardedDigitCount = this.discardedDigitCount ?? new
FastInteger(0);
        if (digitShift <= Int32.MaxValue) {
          this.discardedDigitCount.AddInt((int)digitShift);
        } else {
          this.discardedDigitCount.AddBig((EInteger)digitShift);
        }
        for (int i = str.Length - 1; i >= 0; --i) {
          this.bitsAfterLeftmost |= this.bitLeftmost;
          this.bitLeftmost = (int)(str[i] - '0');
          --digitShift;
          if (digitShift <= 0) {
            break;
          }
        }
        if (newLength <= 9) {
          this.isSmall = true;
          this.shiftedSmall = FastParseLong(str, 0, newLength);
        } else {
          this.shiftedBigInt = EInteger.FromSubstring(str, 0, newLength);
        }
        this.bitsAfterLeftmost = (this.bitsAfterLeftmost != 0) ? 1 : 0;
      }
      #if DEBUG
      if (this.knownDigitLength.CompareToInt(digits) > 0) {
        throw new InvalidOperationException();
      }
      #endif
    }

    private void ShiftToDigitsSmall(int digits) {
      var kb = 0;
      int v2 = this.shiftedSmall;
      kb = (v2 >= 1000000000) ? 10 : ((v2 >= 100000000) ? 9 : ((v2 >=
              10000000) ? 8 : ((v2 >= 1000000) ? 7 : ((v2 >= 100000) ? 6 :
                ((v2 >= 10000) ? 5 : ((v2 >= 1000) ? 4 : ((v2 >= 100) ? 3 : ((v2
                >= 10) ? 2 : 1))))))));
      this.knownDigitLength = new FastInteger(kb);
      if (kb > digits) {
        var digitShift = (int)(kb - digits);
        this.UpdateKnownLengthInt(digitShift);
        this.discardedDigitCount = this.discardedDigitCount != null ?
          this.discardedDigitCount.AddInt(digitShift) :
          new FastInteger(digitShift);
        for (var i = 0; i < digitShift; ++i) {
          var digit = (int)(this.shiftedSmall % 10);
          this.shiftedSmall /= 10;
          this.bitsAfterLeftmost |= this.bitLeftmost;
          this.bitLeftmost = digit;
        }
        this.bitsAfterLeftmost = (this.bitsAfterLeftmost != 0) ? 1 : 0;
      }
    }

    private void TruncateRightLong(long shiftedLong, int digits) {
      if (digits <= 0) {
        return;
      }
      if (shiftedLong == 0 || digits >= 21) {
        this.discardedDigitCount = this.discardedDigitCount ?? new
FastInteger(0);
        this.discardedDigitCount.AddInt(digits);
        this.bitsAfterLeftmost |= this.bitLeftmost;
        this.bitLeftmost = shiftedLong == 0 ? 0 : 1;
        this.shiftedSmall = 0;
        this.isSmall = true;
        this.knownDigitLength = new FastInteger(1);
        return;
      }
      if (digits >= 1 && digits <= TenPowersLong.Length - 1) {
        if (shiftedLong >= TenPowersLong[digits]) {
          long bigPower = TenPowersLong[digits];
          if (this.discardedDigitCount != null) {
            this.discardedDigitCount.AddInt(digits);
          } else {
            this.discardedDigitCount = new FastInteger(digits);
          }
          long quo = shiftedLong / bigPower;
          this.bitsAfterLeftmost |= this.bitLeftmost;
          this.bitLeftmost = (shiftedLong & 1) == 1 ? 1 :
            (shiftedLong - (quo * bigPower) == 0 ? 0 : 1);
          shiftedLong = quo;
          this.isSmall = shiftedLong <= Int32.MaxValue;
          if (this.isSmall) {
            this.shiftedSmall = (int)shiftedLong;
          } else {
            this.shiftedBigInt = EInteger.FromInt64(shiftedLong);
          }
          this.UpdateKnownLengthInt(digits);
          return;
        } else {
          if (this.discardedDigitCount != null) {
            this.discardedDigitCount.AddInt(digits);
          } else {
            this.discardedDigitCount = new FastInteger(digits);
          }
          this.bitsAfterLeftmost |= this.bitLeftmost;
          this.bitLeftmost = shiftedLong == 0 ? 0 : 1;
          shiftedLong = 0;
          this.isSmall = shiftedLong <= Int32.MaxValue;
          if (this.isSmall) {
            this.shiftedSmall = (int)shiftedLong;
          } else {
            this.shiftedBigInt = EInteger.FromInt64(shiftedLong);
          }
          this.UpdateKnownLengthInt(digits);
          return;
        }
      }
      this.ShiftRightInt(digits);
    }

    private void ShiftRightSmall(int digits) {
      if (digits <= 0) {
        return;
      }
      if (this.shiftedSmall == 0) {
        this.discardedDigitCount = this.discardedDigitCount ?? new
FastInteger(0);
        this.discardedDigitCount.AddInt(digits);
        this.bitsAfterLeftmost |= this.bitLeftmost;
        this.bitLeftmost = 0;
        this.knownDigitLength = new FastInteger(1);
        return;
      }
      if (digits >= 2 && digits <= 8) {
        if (this.shiftedSmall >= ValueTenPowers[digits]) {
          int bigPower = ValueTenPowers[digits];
          int smallPower = ValueTenPowers[digits - 1];
          this.discardedDigitCount = this.discardedDigitCount ?? new
FastInteger(
              0);
          this.discardedDigitCount.AddInt(digits);
          int div = this.shiftedSmall / bigPower;
          int rem = this.shiftedSmall - (div * bigPower);
          int rem2 = rem / smallPower;
          this.bitLeftmost = rem2;
          this.bitsAfterLeftmost |= rem - (rem2 * smallPower);
          this.shiftedSmall = div;
          this.knownDigitLength = (div < 10) ? new FastInteger(1) :
            this.CalcKnownDigitLength();
          return;
        } else if (this.shiftedSmall >= ValueTenPowers[digits - 1]) {
          int smallPower = ValueTenPowers[digits - 1];
          if (this.discardedDigitCount != null) {
            this.discardedDigitCount.AddInt(digits);
          } else {
            this.discardedDigitCount = new FastInteger(digits);
          }
          int rem = this.shiftedSmall;
          int rem2 = rem / smallPower;
          this.bitLeftmost = rem2;
          this.bitsAfterLeftmost |= rem - (rem2 * smallPower);
          this.shiftedSmall = 0;
          this.knownDigitLength = new FastInteger(1);
          return;
        } else {
          if (this.discardedDigitCount != null) {
            this.discardedDigitCount.AddInt(digits);
          } else {
            this.discardedDigitCount = new FastInteger(digits);
          }
          int rem = this.shiftedSmall;
          this.bitLeftmost = 0;
          this.bitsAfterLeftmost |= rem;
          this.shiftedSmall = 0;
          this.knownDigitLength = new FastInteger(1);
          return;
        }
      }
      int v2 = this.shiftedSmall;
      int kb = (v2 >= 1000000000) ? 10 : ((v2 >= 100000000) ? 9 : ((v2 >=
              10000000) ? 8 : ((v2 >= 1000000) ? 7 : ((v2 >= 100000) ? 6 :
                ((v2 >= 10000) ? 5 : ((v2 >= 1000) ? 4 : ((v2 >= 100) ? 3 : ((v2
                >= 10) ? 2 : 1))))))));
      this.knownDigitLength = new FastInteger(kb);
      if (this.discardedDigitCount != null) {
        this.discardedDigitCount.AddInt(digits);
      } else {
        this.discardedDigitCount = new FastInteger(digits);
      }
      var digitsShifted = 0;
      while (digits > 0) {
        if (this.shiftedSmall == 0) {
          this.bitsAfterLeftmost |= this.bitLeftmost;
          this.bitLeftmost = 0;
          this.knownDigitLength = new FastInteger(1);
          break;
        } else {
          var digit = (int)(this.shiftedSmall % 10);
          this.bitsAfterLeftmost |= this.bitLeftmost;
          this.bitLeftmost = digit;
          --digits;
          ++digitsShifted;
          this.shiftedSmall /= 10;
        }
      }
      this.UpdateKnownLengthInt(digitsShifted);
      this.bitsAfterLeftmost = (this.bitsAfterLeftmost != 0) ? 1 : 0;
    }

    private void TruncateRightSmall(int digits) {
      if (digits <= 0) {
        return;
      }
      if (this.shiftedSmall == 0 || digits >= 11) {
        this.discardedDigitCount = this.discardedDigitCount ?? new
FastInteger(0);
        this.discardedDigitCount.AddInt(digits);
        this.bitsAfterLeftmost |= this.bitLeftmost;
        this.bitLeftmost = (this.shiftedSmall == 0) ? 0 : 1;
        this.shiftedSmall = 0;
        this.knownDigitLength = new FastInteger(1);
        return;
      }
      if (digits >= 1 && digits <= 8) {
        if (this.shiftedSmall >= ValueTenPowers[digits]) {
          int bigPower = ValueTenPowers[digits];
          if (this.discardedDigitCount != null) {
            this.discardedDigitCount.AddInt(digits);
          } else {
            this.discardedDigitCount = new FastInteger(digits);
          }
          this.bitsAfterLeftmost |= this.bitLeftmost;
          if ((this.shiftedSmall & 1) == 1) {
            this.bitLeftmost = 1;
            this.shiftedSmall /= bigPower;
          } else {
            int quo = this.shiftedSmall / bigPower;
            int rem = this.shiftedSmall - (quo * bigPower);
            this.shiftedSmall = quo;
            this.bitLeftmost |= (rem == 0) ? 0 : 1;
          }
          this.UpdateKnownLengthInt(digits);
          return;
        } else {
          if (this.discardedDigitCount != null) {
            this.discardedDigitCount.AddInt(digits);
          } else {
            this.discardedDigitCount = new FastInteger(digits);
          }
          this.bitsAfterLeftmost |= this.bitLeftmost;
          this.bitLeftmost = (this.shiftedSmall == 0) ? 0 : 1;
          this.shiftedSmall = 0;
          this.knownDigitLength = new FastInteger(1);
          return;
        }
      }
      this.ShiftRightSmall(digits);
    }
  }
}
