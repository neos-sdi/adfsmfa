/*
Written by Peter O. in 2014.
Any copyright is dedicated to the Public Domain.
http://creativecommons.org/publicdomain/zero/1.0/
If you like this, you should donate to Peter O.
at: http://peteroupc.github.io/
 */
using System;

namespace PeterO.Numbers {
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:PeterO.Numbers.ERational"]/*'/>
  public sealed partial class ERational : IComparable<ERational>,
    IEquatable<ERational> {
    private const int MaxSafeInt = 214748363;

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.ERational.NaN"]/*'/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "ERational is immutable")]
    public static readonly ERational NaN = CreateWithFlags(
  EInteger.Zero,
  EInteger.One,
  BigNumberFlags.FlagQuietNaN);

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.ERational.NegativeInfinity"]/*'/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "ERational is immutable")]
    public static readonly ERational NegativeInfinity =
      CreateWithFlags(
  EInteger.Zero,
  EInteger.One,
  BigNumberFlags.FlagInfinity | BigNumberFlags.FlagNegative);

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.ERational.NegativeZero"]/*'/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "ERational is immutable")]
    public static readonly ERational NegativeZero =
      FromEInteger(EInteger.Zero).ChangeSign(false);

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.ERational.One"]/*'/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "ERational is immutable")]
    public static readonly ERational One = FromEInteger(EInteger.One);

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.ERational.PositiveInfinity"]/*'/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "ERational is immutable")]
    public static readonly ERational PositiveInfinity =
      CreateWithFlags(
  EInteger.Zero,
  EInteger.One,
  BigNumberFlags.FlagInfinity);

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.ERational.SignalingNaN"]/*'/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "ERational is immutable")]
    public static readonly ERational SignalingNaN =
      CreateWithFlags(
  EInteger.Zero,
  EInteger.One,
  BigNumberFlags.FlagSignalingNaN);

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.ERational.Ten"]/*'/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "ERational is immutable")]
    public static readonly ERational Ten = FromEInteger((EInteger)10);

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.ERational.Zero"]/*'/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Security",
      "CA2104",
      Justification = "ERational is immutable")]
    public static readonly ERational Zero = FromEInteger(EInteger.Zero);

    private EInteger denominator;

    private int flags;
    private EInteger unsignedNumerator;

    private ERational() {
}

    private void Initialize(EInteger numerator, EInteger denominator) {
            if (numerator == null) {
                throw new ArgumentNullException(nameof(numerator));
            }
            if (denominator == null) {
                throw new ArgumentNullException(nameof(denominator));
            }
            if (denominator.IsZero) {
                throw new ArgumentException("denominator is zero");
            }
            bool numNegative = numerator.Sign < 0;
            bool denNegative = denominator.Sign < 0;
      this.flags = (numNegative != denNegative) ?
              BigNumberFlags.FlagNegative : 0;
            if (numNegative) {
                numerator = -numerator;
            }
            if (denNegative) {
                denominator = -denominator;
            }
#if DEBUG
      if (denominator.IsZero) {
        throw new ArgumentException("doesn't satisfy !denominator.IsZero");
      }
#endif
            this.unsignedNumerator = numerator;
            this.denominator = denominator;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.#ctor(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger)"]/*'/>
  [Obsolete("Use the ERational.Create method instead. This constructor will be private or unavailable in version 1.0.")]
    public ERational(EInteger numerator, EInteger denominator) {
      this.Initialize(numerator, denominator);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.ERational.Denominator"]/*'/>
    public EInteger Denominator {
      get {
        return this.denominator;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.ERational.IsFinite"]/*'/>
    public bool IsFinite {
      get {
        return !this.IsNaN() && !this.IsInfinity();
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.ERational.IsNegative"]/*'/>
    public bool IsNegative {
      get {
        return (this.flags & BigNumberFlags.FlagNegative) != 0;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.ERational.IsZero"]/*'/>
    public bool IsZero {
      get {
        return ((this.flags & (BigNumberFlags.FlagInfinity |
          BigNumberFlags.FlagNaN)) == 0) && this.unsignedNumerator.IsZero;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.ERational.Numerator"]/*'/>
    public EInteger Numerator {
      get {
        return this.IsNegative ? (-(EInteger)this.unsignedNumerator) :
          this.unsignedNumerator;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.ERational.Sign"]/*'/>
    public int Sign {
      get {
        return ((this.flags & (BigNumberFlags.FlagInfinity |
          BigNumberFlags.FlagNaN)) != 0) ? (this.IsNegative ? -1 : 1) :
          (this.unsignedNumerator.IsZero ? 0 : (this.IsNegative ? -1 : 1));
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="P:PeterO.Numbers.ERational.UnsignedNumerator"]/*'/>
    public EInteger UnsignedNumerator {
      get {
        return this.unsignedNumerator;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.Create(System.Int32,System.Int32)"]/*'/>
    public static ERational Create(
  int numeratorSmall,
  int denominatorSmall) {
      return Create((EInteger)numeratorSmall, (EInteger)denominatorSmall);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.Create(PeterO.Numbers.EInteger,PeterO.Numbers.EInteger)"]/*'/>
    public static ERational Create(
  EInteger numerator,
  EInteger denominator) {
            var er = new ERational();
      er.Initialize(numerator, denominator);
            return er;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.CreateNaN(PeterO.Numbers.EInteger)"]/*'/>
    public static ERational CreateNaN(EInteger diag) {
      return CreateNaN(diag, false, false);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.CreateNaN(PeterO.Numbers.EInteger,System.Boolean,System.Boolean)"]/*'/>
    public static ERational CreateNaN(
  EInteger diag,
  bool signaling,
  bool negative) {
      if (diag == null) {
        throw new ArgumentNullException(nameof(diag));
      }
      if (diag.Sign < 0) {
        throw new
  ArgumentException("Diagnostic information must be 0 or greater, was: " +
          diag);
      }
      if (diag.IsZero && !negative) {
        return signaling ? SignalingNaN : NaN;
      }
      var flags = 0;
      if (negative) {
        flags |= BigNumberFlags.FlagNegative;
      }
      flags |= signaling ? BigNumberFlags.FlagSignalingNaN :
        BigNumberFlags.FlagQuietNaN;
      ERational er = ERational.Create(diag, EInteger.One);
      er.flags = flags;
      return er;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.FromDouble(System.Double)"]/*'/>
    public static ERational FromDouble(double flt) {
      return FromEFloat(EFloat.FromDouble(flt));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.FromExtendedDecimal(PeterO.Numbers.EDecimal)"]/*'/>
    [Obsolete("Renamed to FromEDecimal.")]
    public static ERational FromExtendedDecimal(EDecimal ef) {
      return FromEDecimal(ef);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.FromExtendedFloat(PeterO.Numbers.EFloat)"]/*'/>
    [Obsolete("Renamed to FromEFloat.")]
    public static ERational FromExtendedFloat(EFloat ef) {
      return FromEFloat(ef);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.FromEDecimal(PeterO.Numbers.EDecimal)"]/*'/>
    public static ERational FromEDecimal(EDecimal ef) {
      if (ef == null) {
        throw new ArgumentNullException(nameof(ef));
      }
      if (!ef.IsFinite) {
        ERational er = ERational.Create(ef.Mantissa, EInteger.One);
        var flags = 0;
        if (ef.IsNegative) {
          flags |= BigNumberFlags.FlagNegative;
        }
        if (ef.IsInfinity()) {
          flags |= BigNumberFlags.FlagInfinity;
        }
        if (ef.IsSignalingNaN()) {
          flags |= BigNumberFlags.FlagSignalingNaN;
        }
        if (ef.IsQuietNaN()) {
          flags |= BigNumberFlags.FlagQuietNaN;
        }
        er.flags = flags;
        return er;
      }
      EInteger num = ef.Mantissa;
      EInteger exp = ef.Exponent;
      if (exp.IsZero) {
        return FromEInteger(num);
      }
      bool neg = num.Sign < 0;
      num = num.Abs();
      EInteger den = EInteger.One;
      if (exp.Sign < 0) {
        exp = -(EInteger)exp;
        den = NumberUtility.FindPowerOfTenFromBig(exp);
      } else {
        EInteger powerOfTen = NumberUtility.FindPowerOfTenFromBig(exp);
        num *= (EInteger)powerOfTen;
      }
      if (neg) {
        num = -(EInteger)num;
      }
      return ERational.Create(num, den);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.FromEFloat(PeterO.Numbers.EFloat)"]/*'/>
    public static ERational FromEFloat(EFloat ef) {
      if (ef == null) {
        throw new ArgumentNullException(nameof(ef));
      }
      if (!ef.IsFinite) {
        ERational er = ERational.Create(ef.Mantissa, EInteger.One);
        var flags = 0;
        if (ef.IsNegative) {
          flags |= BigNumberFlags.FlagNegative;
        }
        if (ef.IsInfinity()) {
          flags |= BigNumberFlags.FlagInfinity;
        }
        if (ef.IsSignalingNaN()) {
          flags |= BigNumberFlags.FlagSignalingNaN;
        }
        if (ef.IsQuietNaN()) {
          flags |= BigNumberFlags.FlagQuietNaN;
        }
        er.flags = flags;
        return er;
      }
      EInteger num = ef.Mantissa;
      EInteger exp = ef.Exponent;
      if (exp.IsZero) {
        return FromEInteger(num);
      }
      bool neg = num.Sign < 0;
      num = num.Abs();
      EInteger den = EInteger.One;
      if (exp.Sign < 0) {
        exp = -(EInteger)exp;
        den = NumberUtility.ShiftLeft(den, exp);
      } else {
        num = NumberUtility.ShiftLeft(num, exp);
      }
      if (neg) {
        num = -(EInteger)num;
      }
      return ERational.Create(num, den);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.FromEInteger(PeterO.Numbers.EInteger)"]/*'/>
    public static ERational FromEInteger(EInteger bigint) {
      return ERational.Create(bigint, EInteger.One);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.FromSingle(System.Single)"]/*'/>
    public static ERational FromSingle(float flt) {
      return FromEFloat(EFloat.FromSingle(flt));
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.FromString(System.String)"]/*'/>
    public static ERational FromString(string str) {
      return FromString(str, 0, str == null ? 0 : str.Length);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.FromString(System.String,System.Int32,System.Int32)"]/*'/>
    public static ERational FromString(
      string str,
      int offset,
      int length) {
      int tmpoffset = offset;
      if (str == null) {
        throw new ArgumentNullException(nameof(str));
      }
      if (tmpoffset < 0) {
        throw new FormatException("offset (" + tmpoffset + ") is less than " +
                    "0");
      }
      if (tmpoffset > str.Length) {
        throw new FormatException("offset (" + tmpoffset + ") is more than " +
                    str.Length);
      }
      if (length < 0) {
        throw new FormatException("length (" + length + ") is less than " +
                    "0");
      }
      if (length > str.Length) {
        throw new FormatException("length (" + length + ") is more than " +
                    str.Length);
      }
      if (str.Length - tmpoffset < length) {
        throw new FormatException("str's length minus " + tmpoffset + " (" +
                    (str.Length - tmpoffset) + ") is less than " + length);
      }
      if (length == 0) {
        throw new FormatException();
      }
      var negative = false;
      int endStr = tmpoffset + length;
      if (str[0] == '+' || str[0] == '-') {
        negative = str[0] == '-';
        ++tmpoffset;
      }
      var numerInt = 0;
      FastInteger numer = null;
      var numerBuffer = 0;
      var numerBufferMult = 1;
      var denomBuffer = 0;
      var denomBufferMult = 1;
      var haveDigits = false;
      var haveDenominator = false;
      var ndenomInt = 0;
      FastInteger ndenom = null;
      int i = tmpoffset;
      if (i + 8 == endStr) {
        if ((str[i] == 'I' || str[i] == 'i') &&
            (str[i + 1] == 'N' || str[i + 1] == 'n') &&
            (str[i + 2] == 'F' || str[i + 2] == 'f') &&
            (str[i + 3] == 'I' || str[i + 3] == 'i') && (str[i + 4] == 'N' ||
                    str[i + 4] == 'n') && (str[i + 5] ==
                    'I' || str[i + 5] == 'i') &&
            (str[i + 6] == 'T' || str[i + 6] == 't') && (str[i + 7] == 'Y' ||
                    str[i + 7] == 'y')) {
          return negative ? NegativeInfinity : PositiveInfinity;
        }
      }
      if (i + 3 == endStr) {
        if ((str[i] == 'I' || str[i] == 'i') &&
            (str[i + 1] == 'N' || str[i + 1] == 'n') && (str[i + 2] == 'F' ||
                    str[i + 2] == 'f')) {
          return negative ? NegativeInfinity : PositiveInfinity;
        }
      }
      if (i + 3 <= endStr) {
        // Quiet NaN
        if ((str[i] == 'N' || str[i] == 'n') && (str[i + 1] == 'A' || str[i +
                1] == 'a') && (str[i + 2] == 'N' || str[i + 2] == 'n')) {
          if (i + 3 == endStr) {
            return (!negative) ? NaN : NaN.Negate();
          }
          i += 3;
          for (; i < endStr; ++i) {
            if (str[i] >= '0' && str[i] <= '9') {
              var thisdigit = (int)(str[i] - '0');
              haveDigits = haveDigits || thisdigit != 0;
              if (numerInt > MaxSafeInt) {
                if (numer == null) {
                  numer = new FastInteger(numerInt);
                  numerBuffer = thisdigit;
                  numerBufferMult = 10;
                } else {
                  if (numerBufferMult >= 1000000000) {
                    numer.Multiply(numerBufferMult).AddInt(numerBuffer);
                    numerBuffer = thisdigit;
                    numerBufferMult = 10;
                  } else {
                    numerBufferMult *= 10;
                    numerBuffer = (numerBuffer << 3) + (numerBuffer << 1);
                    numerBuffer += thisdigit;
                  }
                }
              } else {
                numerInt *= 10;
                numerInt += thisdigit;
              }
            } else {
              throw new FormatException();
            }
          }
          if (numer != null && (numerBufferMult != 1 || numerBuffer != 0)) {
            numer.Multiply(numerBufferMult).AddInt(numerBuffer);
          }
          EInteger bignumer = (numer == null) ? ((EInteger)numerInt) :
            numer.AsEInteger();
          return CreateNaN(bignumer, false, negative);
        }
      }
      if (i + 4 <= endStr) {
        // Signaling NaN
        if ((str[i] == 'S' || str[i] == 's') && (str[i + 1] == 'N' || str[i +
                    1] == 'n') && (str[i + 2] == 'A' || str[i + 2] == 'a') &&
                (str[i + 3] == 'N' || str[i + 3] == 'n')) {
          if (i + 4 == endStr) {
            return (!negative) ? SignalingNaN : SignalingNaN.Negate();
          }
          i += 4;
          for (; i < endStr; ++i) {
            if (str[i] >= '0' && str[i] <= '9') {
              var thisdigit = (int)(str[i] - '0');
              haveDigits = haveDigits || thisdigit != 0;
              if (numerInt > MaxSafeInt) {
                if (numer == null) {
                  numer = new FastInteger(numerInt);
                  numerBuffer = thisdigit;
                  numerBufferMult = 10;
                } else {
                  if (numerBufferMult >= 1000000000) {
                    numer.Multiply(numerBufferMult).AddInt(numerBuffer);
                    numerBuffer = thisdigit;
                    numerBufferMult = 10;
                  } else {
                    numerBufferMult *= 10;
                    numerBuffer = (numerBuffer << 3) + (numerBuffer << 1);
                    numerBuffer += thisdigit;
                  }
                }
              } else {
                numerInt *= 10;
                numerInt += thisdigit;
              }
            } else {
              throw new FormatException();
            }
          }
          if (numer != null && (numerBufferMult != 1 || numerBuffer != 0)) {
            numer.Multiply(numerBufferMult).AddInt(numerBuffer);
          }
          int flags3 = (negative ? BigNumberFlags.FlagNegative : 0) |
            BigNumberFlags.FlagSignalingNaN;
          EInteger bignumer = (numer == null) ? ((EInteger)numerInt) :
            numer.AsEInteger();
          return CreateWithFlags(
            bignumer,
            EInteger.One,
            flags3);
        }
      }
      // Ordinary number
      for (; i < endStr; ++i) {
        if (str[i] >= '0' && str[i] <= '9') {
          var thisdigit = (int)(str[i] - '0');
          if (numerInt > MaxSafeInt) {
            if (numer == null) {
              numer = new FastInteger(numerInt);
              numerBuffer = thisdigit;
              numerBufferMult = 10;
            } else {
              if (numerBufferMult >= 1000000000) {
                numer.Multiply(numerBufferMult).AddInt(numerBuffer);
                numerBuffer = thisdigit;
                numerBufferMult = 10;
              } else {
                // multiply numerBufferMult and numerBuffer each by 10
             numerBufferMult = (numerBufferMult << 3) + (numerBufferMult <<
                  1);
                numerBuffer = (numerBuffer << 3) + (numerBuffer << 1);
                numerBuffer += thisdigit;
              }
            }
          } else {
            numerInt *= 10;
            numerInt += thisdigit;
          }
          haveDigits = true;
        } else if (str[i] == '/') {
          haveDenominator = true;
          ++i;
          break;
        } else {
          throw new FormatException();
        }
      }
      if (!haveDigits) {
        throw new FormatException();
      }
      if (numer != null && (numerBufferMult != 1 || numerBuffer != 0)) {
        numer.Multiply(numerBufferMult).AddInt(numerBuffer);
      }
      if (haveDenominator) {
        FastInteger denom = null;
        var denomInt = 0;
        tmpoffset = 1;
        haveDigits = false;
        if (i == endStr) {
          throw new FormatException();
        }
        for (; i < endStr; ++i) {
          if (str[i] >= '0' && str[i] <= '9') {
            haveDigits = true;
            var thisdigit = (int)(str[i] - '0');
            if (denomInt > MaxSafeInt) {
              if (denom == null) {
                denom = new FastInteger(denomInt);
                denomBuffer = thisdigit;
                denomBufferMult = 10;
              } else {
                if (denomBufferMult >= 1000000000) {
                  denom.Multiply(denomBufferMult).AddInt(denomBuffer);
                  denomBuffer = thisdigit;
                  denomBufferMult = 10;
                } else {
                  // multiply denomBufferMult and denomBuffer each by 10
             denomBufferMult = (denomBufferMult << 3) + (denomBufferMult <<
                    1);
                  denomBuffer = (denomBuffer << 3) + (denomBuffer << 1);
                  denomBuffer += thisdigit;
                }
              }
            } else {
              denomInt *= 10;
              denomInt += thisdigit;
            }
          } else {
            throw new FormatException();
          }
        }
        if (!haveDigits) {
          throw new FormatException();
        }
        if (denom != null && (denomBufferMult != 1 || denomBuffer != 0)) {
          denom.Multiply(denomBufferMult).AddInt(denomBuffer);
        }
        if (denom == null) {
          ndenomInt = denomInt;
        } else {
          ndenom = denom;
        }
      } else {
        ndenomInt = 1;
      }
      if (i != endStr) {
        throw new FormatException();
      }
      if (ndenom == null ? (ndenomInt == 0) : ndenom.IsValueZero) {
        throw new FormatException();
      }
      ERational erat = Create(
        numer == null ? (EInteger)numerInt : numer.AsEInteger(),
        ndenom == null ? (EInteger)ndenomInt : ndenom.AsEInteger());
      return negative ? erat.Negate() : erat;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.CompareToTotalMagnitude(PeterO.Numbers.ERational)"]/*'/>
    public int CompareToTotalMagnitude(ERational other) {
      if (other == null) {
        return -1;
      }
      var valueIThis = 0;
      var valueIOther = 0;
      int cmp;
      if (this.IsSignalingNaN()) {
        valueIThis = 2;
      } else if (this.IsNaN()) {
        valueIThis = 3;
      } else if (this.IsInfinity()) {
        valueIThis = 1;
      }
      if (other.IsSignalingNaN()) {
        valueIOther = 2;
      } else if (other.IsNaN()) {
        valueIOther = 3;
      } else if (other.IsInfinity()) {
        valueIOther = 1;
      }
      if (valueIThis > valueIOther) {
        return 1;
      } else if (valueIThis < valueIOther) {
        return -1;
      }
      if (valueIThis >= 2) {
        cmp = this.unsignedNumerator.CompareTo(
         other.unsignedNumerator);
        return cmp;
      } else if (valueIThis == 1) {
        return 0;
      } else {
        cmp = this.Abs().CompareTo(other.Abs());
        if (cmp == 0) {
          cmp = this.denominator.CompareTo(
           other.denominator);
          return cmp;
        }
        return cmp;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.CompareToTotal(PeterO.Numbers.ERational)"]/*'/>
    public int CompareToTotal(ERational other) {
      if (other == null) {
        return -1;
      }
      bool neg1 = this.IsNegative;
      bool neg2 = other.IsNegative;
      if (neg1 != neg2) {
        return neg1 ? -1 : 1;
      }
      var valueIThis = 0;
      var valueIOther = 0;
      int cmp;
      if (this.IsSignalingNaN()) {
        valueIThis = 2;
      } else if (this.IsNaN()) {
        valueIThis = 3;
      } else if (this.IsInfinity()) {
        valueIThis = 1;
      }
      if (other.IsSignalingNaN()) {
        valueIOther = 2;
      } else if (other.IsNaN()) {
        valueIOther = 3;
      } else if (other.IsInfinity()) {
        valueIOther = 1;
      }
      if (valueIThis > valueIOther) {
        return neg1 ? -1 : 1;
      } else if (valueIThis < valueIOther) {
        return neg1 ? 1 : -1;
      }
      if (valueIThis >= 2) {
        cmp = this.unsignedNumerator.CompareTo(
         other.unsignedNumerator);
        return neg1 ? -cmp : cmp;
      } else if (valueIThis == 1) {
        return 0;
      } else {
        cmp = this.CompareTo(other);
        if (cmp == 0) {
          cmp = this.denominator.CompareTo(
           other.denominator);
          return neg1 ? -cmp : cmp;
        }
        return cmp;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.Abs"]/*'/>
    public ERational Abs() {
      if (this.IsNegative) {
     ERational er = ERational.Create(
  this.unsignedNumerator,
  this.denominator);
        er.flags = this.flags & ~BigNumberFlags.FlagNegative;
        return er;
      }
      return this;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.Add(PeterO.Numbers.ERational)"]/*'/>
    public ERational Add(ERational otherValue) {
      if (otherValue == null) {
        throw new ArgumentNullException(nameof(otherValue));
      }
      if (this.IsSignalingNaN()) {
        return CreateNaN(this.unsignedNumerator, false, this.IsNegative);
      }
      if (otherValue.IsSignalingNaN()) {
        return CreateNaN(
      otherValue.unsignedNumerator,
      false,
      otherValue.IsNegative);
      }
      if (this.IsQuietNaN()) {
        return this;
      }
      if (otherValue.IsQuietNaN()) {
        return otherValue;
      }
      if (this.IsInfinity()) {
        return otherValue.IsInfinity() ? ((this.IsNegative ==
          otherValue.IsNegative) ? this : NaN) : this;
      }
      if (otherValue.IsInfinity()) {
        return otherValue;
      }
      EInteger ad = this.Numerator * (EInteger)otherValue.Denominator;
      EInteger bc = this.Denominator * (EInteger)otherValue.Numerator;
      EInteger bd = this.Denominator * (EInteger)otherValue.Denominator;
      ad += (EInteger)bc;
      return ERational.Create(ad, bd);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.CompareTo(PeterO.Numbers.ERational)"]/*'/>
    public int CompareTo(ERational other) {
      if (other == null) {
        return 1;
      }
      if (this == other) {
        return 0;
      }
      if (this.IsNaN()) {
        return other.IsNaN() ? 0 : 1;
      }
      if (other.IsNaN()) {
        return -1;
      }
      int signA = this.Sign;
      int signB = other.Sign;
      if (signA != signB) {
        return (signA < signB) ? -1 : 1;
      }
      if (signB == 0 || signA == 0) {
        // Special case: Either operand is zero
        return 0;
      }
      if (this.IsInfinity()) {
        if (other.IsInfinity()) {
          // if we get here, this only means that
          // both are positive infinity or both
          // are negative infinity
          return 0;
        }
        return this.IsNegative ? -1 : 1;
      }
      if (other.IsInfinity()) {
        return other.IsNegative ? 1 : -1;
      }
      // At this point, both numbers are finite and
      // have the same sign
#if DEBUG
      if (!this.IsFinite) {
        throw new ArgumentException("doesn't satisfy this.IsFinite");
      }
      if (!other.IsFinite) {
        throw new ArgumentException("doesn't satisfy other.IsFinite");
      }
#endif

      int dencmp = this.denominator.CompareTo(other.denominator);
      // At this point, the signs are equal so we can compare
      // their absolute values instead
      int numcmp = this.unsignedNumerator.CompareTo(other.unsignedNumerator);
      if (signA < 0) {
        numcmp = -numcmp;
      }
      if (numcmp == 0) {
        // Special case: numerators are equal, so the
        // number with the lower denominator is greater
        return signA < 0 ? dencmp : -dencmp;
      }
      if (dencmp == 0) {
        // denominators are equal
        return numcmp;
      }
      EInteger ad = this.Numerator * (EInteger)other.Denominator;
      EInteger bc = this.Denominator * (EInteger)other.Numerator;
      return ad.CompareTo(bc);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.CompareToBinary(PeterO.Numbers.EFloat)"]/*'/>
    public int CompareToBinary(EFloat other) {
      if (other == null) {
        return 1;
      }
      if (this.IsNaN()) {
        return other.IsNaN() ? 0 : 1;
      }
      int signA = this.Sign;
      int signB = other.Sign;
      if (signA != signB) {
        return (signA < signB) ? -1 : 1;
      }
      if (signB == 0 || signA == 0) {
        // Special case: Either operand is zero
        return 0;
      }
      if (this.IsInfinity()) {
        if (other.IsInfinity()) {
          // if we get here, this only means that
          // both are positive infinity or both
          // are negative infinity
          return 0;
        }
        return this.IsNegative ? -1 : 1;
      }
      if (other.IsInfinity()) {
        return other.IsNegative ? 1 : -1;
      }
      // At this point, both numbers are finite and
      // have the same sign
#if DEBUG
      if (!this.IsFinite) {
        throw new ArgumentException("doesn't satisfy this.IsFinite");
      }
      if (!other.IsFinite) {
        throw new ArgumentException("doesn't satisfy other.IsFinite");
      }
#endif
      EInteger bigExponent = other.Exponent;
      if (bigExponent.IsZero) {
        // Special case: other has exponent 0
        EInteger otherMant = other.Mantissa;
        EInteger bcx = this.Denominator * (EInteger)otherMant;
        return this.Numerator.CompareTo(bcx);
      }
      if (bigExponent.Abs().CompareTo((EInteger)1000) > 0) {
        // Other has a high absolute value of exponent, so try different
        // approaches to
        // comparison
        EInteger thisRem;
        EInteger thisInt;
        {
          EInteger[] divrem = this.UnsignedNumerator.DivRem(this.Denominator);
          thisInt = divrem[0];
          thisRem = divrem[1];
        }
        EFloat otherAbs = other.Abs();
        EFloat thisIntDec = EFloat.FromEInteger(thisInt);
        if (thisRem.IsZero) {
          // This object's value is an integer
          // Console.WriteLine("Shortcircuit IV");
          int ret = thisIntDec.CompareTo(otherAbs);
          return this.IsNegative ? -ret : ret;
        }
        if (thisIntDec.CompareTo(otherAbs) > 0) {
          // Truncated absolute value is greater than other's untruncated
          // absolute value
          // Console.WriteLine("Shortcircuit I");
          return this.IsNegative ? -1 : 1;
        }
        // Round up
        thisInt = thisInt.Add(EInteger.One);
        thisIntDec = EFloat.FromEInteger(thisInt);
        if (thisIntDec.CompareTo(otherAbs) < 0) {
          // Absolute value rounded up is less than other's unrounded
          // absolute value
          // Console.WriteLine("Shortcircuit II");
          return this.IsNegative ? 1 : -1;
        }
        thisIntDec = EFloat.FromEInteger(this.UnsignedNumerator).Divide(
            EFloat.FromEInteger(this.Denominator),
            EContext.ForPrecisionAndRounding(256, ERounding.Down));
        if (thisIntDec.CompareTo(otherAbs) > 0) {
          // Truncated absolute value is greater than other's untruncated
          // absolute value
          // Console.WriteLine("Shortcircuit III");
          return this.IsNegative ? -1 : 1;
        }
        if (other.Exponent.Sign > 0) {
          // NOTE: if unsigned numerator is 0, bitLength will return
          // 0 instead of 1, but the possibility of 0 was already excluded
          int digitCount = this.UnsignedNumerator.GetSignedBitLength();
          --digitCount;
          var bigDigitCount = (EInteger)digitCount;
          if (bigDigitCount.CompareTo(other.Exponent) < 0) {
            // Numerator's digit count minus 1 is less than the other' s
            // exponent,
            // and other's exponent is positive, so this value's absolute
            // value is less
            return this.IsNegative ? 1 : -1;
          }
        }
      }
      // Convert to rational number and use usual rational number
      // comparison
      // Console.WriteLine("no shortcircuit");
      // Console.WriteLine(this);
      // Console.WriteLine(other);
      ERational otherRational = ERational.FromEFloat(other);
      EInteger ad = this.Numerator * (EInteger)otherRational.Denominator;
      EInteger bc = this.Denominator * (EInteger)otherRational.Numerator;
      return ad.CompareTo(bc);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.CompareToDecimal(PeterO.Numbers.EDecimal)"]/*'/>
    public int CompareToDecimal(EDecimal other) {
      if (other == null) {
        return 1;
      }
      if (this.IsNaN()) {
        return other.IsNaN() ? 0 : 1;
      }
      int signA = this.Sign;
      int signB = other.Sign;
      if (signA != signB) {
        return (signA < signB) ? -1 : 1;
      }
      if (signB == 0 || signA == 0) {
        // Special case: Either operand is zero
        return 0;
      }
      if (this.IsInfinity()) {
        if (other.IsInfinity()) {
          // if we get here, this only means that
          // both are positive infinity or both
          // are negative infinity
          return 0;
        }
        return this.IsNegative ? -1 : 1;
      }
      if (other.IsInfinity()) {
        return other.IsNegative ? 1 : -1;
      }
      // At this point, both numbers are finite and
      // have the same sign
#if DEBUG
      if (!this.IsFinite) {
        throw new ArgumentException("doesn't satisfy this.IsFinite");
      }
      if (!other.IsFinite) {
        throw new ArgumentException("doesn't satisfy other.IsFinite");
      }
#endif

      if (other.Exponent.IsZero) {
        // Special case: other has exponent 0
        EInteger otherMant = other.Mantissa;
        EInteger bcx = this.Denominator * (EInteger)otherMant;
        return this.Numerator.CompareTo(bcx);
      }
      if (other.Exponent.Abs().CompareTo((EInteger)50) > 0) {
        // Other has a high absolute value of exponent, so try different
        // approaches to
        // comparison
        EInteger thisRem;
        EInteger thisInt;
        {
          EInteger[] divrem = this.UnsignedNumerator.DivRem(this.Denominator);
          thisInt = divrem[0];
          thisRem = divrem[1];
        }
        EDecimal otherAbs = other.Abs();
        EDecimal thisIntDec = EDecimal.FromEInteger(thisInt);
        if (thisRem.IsZero) {
          // This object's value is an integer
          // Console.WriteLine("Shortcircuit IV");
          int ret = thisIntDec.CompareTo(otherAbs);
          return this.IsNegative ? -ret : ret;
        }
        if (thisIntDec.CompareTo(otherAbs) > 0) {
          // Truncated absolute value is greater than other's untruncated
          // absolute value
          // Console.WriteLine("Shortcircuit I");
          return this.IsNegative ? -1 : 1;
        }
        // Round up
        thisInt = thisInt.Add(EInteger.One);
        thisIntDec = EDecimal.FromEInteger(thisInt);
        if (thisIntDec.CompareTo(otherAbs) < 0) {
          // Absolute value rounded up is less than other's unrounded
          // absolute value
          // Console.WriteLine("Shortcircuit II");
          return this.IsNegative ? 1 : -1;
        }
        // Conservative approximation of this rational number's absolute value,
        // as a decimal number. The true value will be greater or equal.
        thisIntDec = EDecimal.FromEInteger(this.UnsignedNumerator).Divide(
              EDecimal.FromEInteger(this.Denominator),
              EContext.ForPrecisionAndRounding(20, ERounding.Down));
        if (thisIntDec.CompareTo(otherAbs) > 0) {
          // Truncated absolute value is greater than other's untruncated
          // absolute value
          // Console.WriteLine("Shortcircuit III");
          return this.IsNegative ? -1 : 1;
        }
        // Console.WriteLine("---" + this + " " + other);
        if (other.Exponent.Sign > 0) {
          int digitCount = this.UnsignedNumerator.GetDigitCount();
          --digitCount;
          var bigDigitCount = (EInteger)digitCount;
          if (bigDigitCount.CompareTo(other.Exponent) < 0) {
            // Numerator's digit count minus 1 is less than the other' s
            // exponent,
            // and other's exponent is positive, so this value's absolute
            // value is less
            return this.IsNegative ? 1 : -1;
          }
        }
      }
      // Convert to rational number and use usual rational number
      // comparison
      // Console.WriteLine("no shortcircuit");
      // Console.WriteLine(this);
      // Console.WriteLine(other);
      ERational otherRational = ERational.FromEDecimal(other);
      EInteger ad = this.Numerator * (EInteger)otherRational.Denominator;
      EInteger bc = this.Denominator * (EInteger)otherRational.Numerator;
      return ad.CompareTo(bc);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.CopySign(PeterO.Numbers.ERational)"]/*'/>
    public ERational CopySign(ERational other) {
      if (other == null) {
        throw new ArgumentNullException(nameof(other));
      }
      if (this.IsNegative) {
        return other.IsNegative ? this : this.Negate();
      } else {
        return other.IsNegative ? this.Negate() : this;
      }
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.Divide(PeterO.Numbers.ERational)"]/*'/>
    public ERational Divide(ERational otherValue) {
      if (otherValue == null) {
        throw new ArgumentNullException(nameof(otherValue));
      }
      if (this.IsSignalingNaN()) {
        return CreateNaN(this.unsignedNumerator, false, this.IsNegative);
      }
      if (otherValue.IsSignalingNaN()) {
        return CreateNaN(
      otherValue.unsignedNumerator,
      false,
      otherValue.IsNegative);
      }
      if (this.IsQuietNaN()) {
        return this;
      }
      if (otherValue.IsQuietNaN()) {
        return otherValue;
      }
      bool resultNeg = this.IsNegative ^ otherValue.IsNegative;
      if (this.IsInfinity()) {
        return otherValue.IsInfinity() ? NaN : (resultNeg ? NegativeInfinity :
          PositiveInfinity);
      }
      if (otherValue.IsInfinity()) {
        return resultNeg ? NegativeZero : Zero;
      }
      if (otherValue.IsZero) {
        return this.IsZero ? NaN : (resultNeg ? NegativeInfinity :
                PositiveInfinity);
      }
      if (this.IsZero) {
        return resultNeg ? NegativeZero : Zero;
      }
      EInteger ad = this.Numerator * (EInteger)otherValue.Denominator;
      EInteger bc = this.Denominator * (EInteger)otherValue.Numerator;
      return ERational.Create(ad, bc).ChangeSign(resultNeg);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.Equals(System.Object)"]/*'/>
    public override bool Equals(object obj) {
      var other = obj as ERational;
      return (
  other != null) && (
  Object.Equals(
  this.unsignedNumerator,
  other.unsignedNumerator) && Object.Equals(
  this.denominator,
  other.denominator) && this.flags == other.flags);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.Equals(PeterO.Numbers.ERational)"]/*'/>
    public bool Equals(ERational other) {
      return this.Equals((object)other);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.GetHashCode"]/*'/>
    public override int GetHashCode() {
      var hashCode = 1857066527;
      unchecked {
        if (this.unsignedNumerator != null) {
          hashCode += 1857066539 * this.unsignedNumerator.GetHashCode();
        }
        if (this.denominator != null) {
          hashCode += 1857066551 * this.denominator.GetHashCode();
        }
        hashCode += 1857066623 * this.flags;
      }
      return hashCode;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.IsInfinity"]/*'/>
    public bool IsInfinity() {
      return (this.flags & BigNumberFlags.FlagInfinity) != 0;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.IsNaN"]/*'/>
    public bool IsNaN() {
      return (this.flags & BigNumberFlags.FlagNaN) != 0;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.IsNegativeInfinity"]/*'/>
    public bool IsNegativeInfinity() {
      return (this.flags & (BigNumberFlags.FlagInfinity |
        BigNumberFlags.FlagNegative)) ==
        (BigNumberFlags.FlagInfinity | BigNumberFlags.FlagNegative);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.IsPositiveInfinity"]/*'/>
    public bool IsPositiveInfinity() {
      return (this.flags & (BigNumberFlags.FlagInfinity |
        BigNumberFlags.FlagNegative)) == BigNumberFlags.FlagInfinity;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.IsQuietNaN"]/*'/>
    public bool IsQuietNaN() {
      return (this.flags & BigNumberFlags.FlagQuietNaN) != 0;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.IsSignalingNaN"]/*'/>
    public bool IsSignalingNaN() {
      return (this.flags & BigNumberFlags.FlagSignalingNaN) != 0;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.Multiply(PeterO.Numbers.ERational)"]/*'/>
    public ERational Multiply(ERational otherValue) {
      if (otherValue == null) {
        throw new ArgumentNullException(nameof(otherValue));
      }
      if (this.IsSignalingNaN()) {
        return CreateNaN(this.unsignedNumerator, false, this.IsNegative);
      }
      if (otherValue.IsSignalingNaN()) {
        return CreateNaN(
      otherValue.unsignedNumerator,
      false,
      otherValue.IsNegative);
      }
      if (this.IsQuietNaN()) {
        return this;
      }
      if (otherValue.IsQuietNaN()) {
        return otherValue;
      }
      bool resultNeg = this.IsNegative ^ otherValue.IsNegative;
      if (this.IsInfinity()) {
        return otherValue.IsZero ? NaN : (resultNeg ? NegativeInfinity :
          PositiveInfinity);
      }
      if (otherValue.IsInfinity()) {
        return this.IsZero ? NaN : (resultNeg ? NegativeInfinity :
                PositiveInfinity);
      }
      EInteger ac = this.Numerator * (EInteger)otherValue.Numerator;
      EInteger bd = this.Denominator * (EInteger)otherValue.Denominator;
      return ac.IsZero ? (resultNeg ? NegativeZero : Zero) :
               ERational.Create(ac, bd).ChangeSign(resultNeg);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.Negate"]/*'/>
    public ERational Negate() {
      ERational er = ERational.Create(this.unsignedNumerator, this.denominator);
      er.flags = this.flags ^ BigNumberFlags.FlagNegative;
      return er;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.Remainder(PeterO.Numbers.ERational)"]/*'/>
    public ERational Remainder(ERational otherValue) {
      if (otherValue == null) {
        throw new ArgumentNullException(nameof(otherValue));
      }
      if (this.IsSignalingNaN()) {
        return CreateNaN(this.unsignedNumerator, false, this.IsNegative);
      }
      if (otherValue.IsSignalingNaN()) {
        return CreateNaN(
      otherValue.unsignedNumerator,
      false,
      otherValue.IsNegative);
      }
      if (this.IsQuietNaN()) {
        return this;
      }
      if (otherValue.IsQuietNaN()) {
        return otherValue;
      }
      bool resultNeg = this.IsNegative ^ otherValue.IsNegative;
      if (this.IsInfinity()) {
        return NaN;
      }
      if (otherValue.IsInfinity()) {
        return this;
      }
      if (otherValue.IsZero) {
        return NaN;
      }
      if (this.IsZero) {
        return this;
      }
      EInteger ad = this.Numerator * (EInteger)otherValue.Denominator;
      EInteger bc = this.Denominator * (EInteger)otherValue.Numerator;
      EInteger quo = ad / (EInteger)bc;  // Find the integer quotient
      EInteger tnum = quo * (EInteger)otherValue.Numerator;
      EInteger tden = otherValue.Denominator;
      EInteger thisDen = this.Denominator;
      ad = this.Numerator * (EInteger)tden;
      bc = thisDen * (EInteger)tnum;
      tden *= (EInteger)thisDen;
      ad -= (EInteger)bc;
      return ERational.Create(ad, tden).ChangeSign(resultNeg);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.Subtract(PeterO.Numbers.ERational)"]/*'/>
    public ERational Subtract(ERational otherValue) {
      if (otherValue == null) {
        throw new ArgumentNullException(nameof(otherValue));
      }
      if (this.IsSignalingNaN()) {
        return CreateNaN(this.unsignedNumerator, false, this.IsNegative);
      }
      if (otherValue.IsSignalingNaN()) {
        return CreateNaN(
      otherValue.unsignedNumerator,
      false,
      otherValue.IsNegative);
      }
      if (this.IsQuietNaN()) {
        return this;
      }
      if (otherValue.IsQuietNaN()) {
        return otherValue;
      }
      if (this.IsInfinity()) {
        if (otherValue.IsInfinity()) {
          return (this.IsNegative != otherValue.IsNegative) ?
            (this.IsNegative ? PositiveInfinity : NegativeInfinity) : NaN;
        }
        return this.IsNegative ? PositiveInfinity : NegativeInfinity;
      }
      if (otherValue.IsInfinity()) {
        return otherValue.IsNegative ? PositiveInfinity : NegativeInfinity;
      }
      EInteger ad = this.Numerator * (EInteger)otherValue.Denominator;
      EInteger bc = this.Denominator * (EInteger)otherValue.Numerator;
      EInteger bd = this.Denominator * (EInteger)otherValue.Denominator;
      ad -= (EInteger)bc;
      return ERational.Create(ad, bd);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToDouble"]/*'/>
    public double ToDouble() {
      if (!this.IsFinite) {
        return this.ToEFloat(EContext.Binary64).ToDouble();
      }
      if (this.IsNegative && this.IsZero) {
        return EFloat.NegativeZero.ToDouble();
      }
      return EFloat.FromEInteger(this.Numerator)
        .Divide(EFloat.FromEInteger(this.denominator), EContext.Binary64)
        .ToDouble();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToEInteger"]/*'/>
    public EInteger ToEInteger() {
      if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      return this.Numerator / (EInteger)this.denominator;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToEIntegerExact"]/*'/>
    [Obsolete("Renamed to ToEIntegerIfExact.")]
    public EInteger ToEIntegerExact() {
      return this.ToEIntegerIfExact();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToEIntegerIfExact"]/*'/>
    public EInteger ToEIntegerIfExact() {
      if (!this.IsFinite) {
        throw new OverflowException("Value is infinity or NaN");
      }
      EInteger rem;
      EInteger quo;
      {
        EInteger[] divrem = this.Numerator.DivRem(this.denominator);
        quo = divrem[0];
        rem = divrem[1];
      }
      if (!rem.IsZero) {
        throw new ArithmeticException("Value is not an integral value");
      }
      return quo;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToEDecimal"]/*'/>
    public EDecimal ToEDecimal() {
      return this.ToEDecimal(null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToEDecimal(PeterO.Numbers.EContext)"]/*'/>
    public EDecimal ToEDecimal(EContext ctx) {
      if (this.IsNaN()) {
        return EDecimal.CreateNaN(
  this.unsignedNumerator,
  this.IsSignalingNaN(),
  this.IsNegative,
  ctx);
      }
      if (this.IsPositiveInfinity()) {
        return EDecimal.PositiveInfinity.RoundToPrecision(ctx);
      }
      if (this.IsNegativeInfinity()) {
        return EDecimal.NegativeInfinity.RoundToPrecision(ctx);
      }
      EDecimal ef = (this.IsNegative && this.IsZero) ?
 EDecimal.NegativeZero : EDecimal.FromEInteger(this.Numerator);
      return ef.Divide(EDecimal.FromEInteger(this.Denominator), ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToEDecimalExactIfPossible(PeterO.Numbers.EContext)"]/*'/>
    public EDecimal ToEDecimalExactIfPossible(EContext
          ctx) {
      if (ctx == null) {
        return this.ToEDecimal(null);
      }
      if (this.IsNaN()) {
        return EDecimal.CreateNaN(
  this.unsignedNumerator,
  this.IsSignalingNaN(),
  this.IsNegative,
  ctx);
      }
      if (this.IsPositiveInfinity()) {
        return EDecimal.PositiveInfinity.RoundToPrecision(ctx);
      }
      if (this.IsNegativeInfinity()) {
        return EDecimal.NegativeInfinity.RoundToPrecision(ctx);
      }
      if (this.IsNegative && this.IsZero) {
        return EDecimal.NegativeZero;
      }
      EDecimal valueEdNum = (this.IsNegative && this.IsZero) ?
 EDecimal.NegativeZero : EDecimal.FromEInteger(this.Numerator);
      EDecimal valueEdDen = EDecimal.FromEInteger(this.Denominator);
      EDecimal ed = valueEdNum.Divide(valueEdDen, null);
      if (ed.IsNaN()) {
        // Result would be inexact, try again using the precision context
        ed = valueEdNum.Divide(valueEdDen, ctx);
      }
      return ed;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToExtendedDecimal"]/*'/>
    [Obsolete("Renamed to ToEDecimal.")]
    public EDecimal ToExtendedDecimal() {
      return this.ToEDecimal();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToExtendedDecimal(PeterO.Numbers.EContext)"]/*'/>
    [Obsolete("Renamed to ToEDecimal.")]
    public EDecimal ToExtendedDecimal(EContext ctx) {
      return this.ToEDecimal(ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToExtendedDecimalExactIfPossible(PeterO.Numbers.EContext)"]/*'/>
    [Obsolete("Renamed to ToEDecimalExactIfPossible.")]
    public EDecimal ToExtendedDecimalExactIfPossible(EContext ctx) {
      return this.ToEDecimalExactIfPossible(ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToEFloat"]/*'/>
    public EFloat ToEFloat() {
      return this.ToEFloat(null);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToEFloat(PeterO.Numbers.EContext)"]/*'/>
    public EFloat ToEFloat(EContext ctx) {
      if (this.IsNaN()) {
        return EFloat.CreateNaN(
  this.unsignedNumerator,
  this.IsSignalingNaN(),
  this.IsNegative,
  ctx);
      }
      if (this.IsPositiveInfinity()) {
        return EFloat.PositiveInfinity.RoundToPrecision(ctx);
      }
      if (this.IsNegativeInfinity()) {
        return EFloat.NegativeInfinity.RoundToPrecision(ctx);
      }
      EFloat ef = (this.IsNegative && this.IsZero) ?
     EFloat.NegativeZero : EFloat.FromEInteger(this.Numerator);
      return ef.Divide(EFloat.FromEInteger(this.Denominator), ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToEFloatExactIfPossible(PeterO.Numbers.EContext)"]/*'/>
    public EFloat ToEFloatExactIfPossible(EContext ctx) {
      if (ctx == null) {
        return this.ToEFloat(null);
      }
      if (this.IsNaN()) {
        return EFloat.CreateNaN(
  this.unsignedNumerator,
  this.IsSignalingNaN(),
  this.IsNegative,
  ctx);
      }
      if (this.IsPositiveInfinity()) {
        return EFloat.PositiveInfinity.RoundToPrecision(ctx);
      }
      if (this.IsNegativeInfinity()) {
        return EFloat.NegativeInfinity.RoundToPrecision(ctx);
      }
      if (this.IsZero) {
        return this.IsNegative ? EFloat.NegativeZero :
            EFloat.Zero;
      }
      EFloat valueEdNum = (this.IsNegative && this.IsZero) ?
     EFloat.NegativeZero : EFloat.FromEInteger(this.Numerator);
      EFloat valueEdDen = EFloat.FromEInteger(this.Denominator);
      EFloat ed = valueEdNum.Divide(valueEdDen, null);
      if (ed.IsNaN()) {
        // Result would be inexact, try again using the precision context
        ed = valueEdNum.Divide(valueEdDen, ctx);
      }
      return ed;
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToExtendedFloat"]/*'/>
    [Obsolete("Renamed to ToEFloat.")]
    public EFloat ToExtendedFloat() {
      return this.ToEFloat();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToExtendedFloat(PeterO.Numbers.EContext)"]/*'/>
    [Obsolete("Renamed to ToEFloat.")]
    public EFloat ToExtendedFloat(EContext ctx) {
      return this.ToEFloat(ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToExtendedFloatExactIfPossible(PeterO.Numbers.EContext)"]/*'/>
    [Obsolete("Renamed to ToEFloatExactIfPossible.")]
    public EFloat ToExtendedFloatExactIfPossible(EContext ctx) {
      return this.ToEFloatExactIfPossible(ctx);
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToSingle"]/*'/>
    public float ToSingle() {
      return
  this.ToEFloat(EContext.Binary32.WithRounding(ERounding.Odd))
        .ToSingle();
    }

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToString"]/*'/>
    public override string ToString() {
      if (!this.IsFinite) {
        if (this.IsSignalingNaN()) {
          if (this.unsignedNumerator.IsZero) {
            return this.IsNegative ? "-sNaN" : "sNaN";
          }
          return this.IsNegative ? "-sNaN" + this.unsignedNumerator :
              "sNaN" + this.unsignedNumerator;
        }
        if (this.IsQuietNaN()) {
          if (this.unsignedNumerator.IsZero) {
            return this.IsNegative ? "-NaN" : "NaN";
          }
          return this.IsNegative ? "-NaN" + this.unsignedNumerator :
              "NaN" + this.unsignedNumerator;
        }
        if (this.IsInfinity()) {
          return this.IsNegative ? "-Infinity" : "Infinity";
        }
      }
      return (this.Numerator.IsZero && this.IsNegative) ? ("-0/" +
        this.Denominator) : (this.Numerator + "/" + this.Denominator);
    }

    private static ERational CreateWithFlags(
  EInteger numerator,
  EInteger denominator,
  int flags) {
      ERational er = ERational.Create(numerator, denominator);
      er.flags = flags;
      return er;
    }

    private ERational ChangeSign(bool negative) {
      if (negative) {
        this.flags |= BigNumberFlags.FlagNegative;
      } else {
        this.flags &= ~BigNumberFlags.FlagNegative;
      }
      return this;
    }

        // Begin integer conversions

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToByteChecked"]/*'/>
public byte ToByteChecked() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
return this.IsZero ? ((byte)0) : this.ToEInteger().ToByteChecked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToByteUnchecked"]/*'/>
public byte ToByteUnchecked() {
 return this.IsFinite ? this.ToEInteger().ToByteUnchecked() : (byte)0;
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToByteIfExact"]/*'/>
public byte ToByteIfExact() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
 return this.IsZero ? ((byte)0) : this.ToEIntegerIfExact().ToByteChecked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.FromByte(System.Byte)"]/*'/>
public static ERational FromByte(byte inputByte) {
 int val = ((int)inputByte) & 0xff;
 return FromInt32(val);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToInt16Checked"]/*'/>
public short ToInt16Checked() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
return this.IsZero ? ((short)0) : this.ToEInteger().ToInt16Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToInt16Unchecked"]/*'/>
public short ToInt16Unchecked() {
 return this.IsFinite ? this.ToEInteger().ToInt16Unchecked() : (short)0;
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToInt16IfExact"]/*'/>
public short ToInt16IfExact() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
 return this.IsZero ? ((short)0) :
   this.ToEIntegerIfExact().ToInt16Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.FromInt16(System.Int16)"]/*'/>
public static ERational FromInt16(short inputInt16) {
 var val = (int)inputInt16;
 return FromInt32(val);
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToInt32Checked"]/*'/>
public int ToInt32Checked() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
return this.IsZero ? ((int)0) : this.ToEInteger().ToInt32Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToInt32Unchecked"]/*'/>
public int ToInt32Unchecked() {
 return this.IsFinite ? this.ToEInteger().ToInt32Unchecked() : (int)0;
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToInt32IfExact"]/*'/>
public int ToInt32IfExact() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
 return this.IsZero ? ((int)0) : this.ToEIntegerIfExact().ToInt32Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.FromInt32(System.Int32)"]/*'/>
public static ERational FromInt32(int inputInt32) {
 return FromEInteger(EInteger.FromInt32(inputInt32));
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToInt64Checked"]/*'/>
public long ToInt64Checked() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
return this.IsZero ? ((long)0) : this.ToEInteger().ToInt64Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToInt64Unchecked"]/*'/>
public long ToInt64Unchecked() {
 return this.IsFinite ? this.ToEInteger().ToInt64Unchecked() : (long)0;
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.ToInt64IfExact"]/*'/>
public long ToInt64IfExact() {
 if (!this.IsFinite) {
 throw new OverflowException("Value is infinity or NaN");
}
 return this.IsZero ? ((long)0) : this.ToEIntegerIfExact().ToInt64Checked();
}

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="M:PeterO.Numbers.ERational.FromInt64(System.Int64)"]/*'/>
public static ERational FromInt64(long inputInt64) {
 return FromEInteger(EInteger.FromInt64(inputInt64));
}

// End integer conversions
  }
}
