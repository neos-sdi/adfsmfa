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
using System.Text;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor.Numbers
{
  internal static class ERationalTextString {
    private const int MaxSafeInt = EDecimal.MaxSafeInt;

    public static ERational FromString(
      string chars,
      int offset,
      int length,
      bool throwException) {
      int tmpoffset = offset;
      if (chars == null) {
        if (!throwException) {
          return null;
        } else {
          throw new ArgumentNullException(nameof(chars));
        }
      }
      if (tmpoffset < 0) {
        if (!throwException) {
          return null;
        } else { throw new FormatException("offset(" + tmpoffset + ") is" +
"\u0020less" + "\u0020than " + "0");
}
      }
      if (tmpoffset > chars.Length) {
        if (!throwException) {
          return null;
        } else { throw new FormatException("offset(" + tmpoffset + ") is" +
"\u0020more" + "\u0020than " + chars.Length);
}
      }
      if (length < 0) {
        if (!throwException) {
          return null;
        } else {
  throw new FormatException("length(" + length + ") is less than " + "0");
 }
      }
      if (length > chars.Length) {
        if (!throwException) {
          return null;
        } else {
  throw new FormatException("length(" + length + ") is more than " +
chars.Length);
 }
      }
      if (chars.Length - tmpoffset < length) {
        if (!throwException) {
          return null;
        } else { throw new FormatException("chars's length minus " +
tmpoffset + "(" + (chars.Length - tmpoffset) + ") is less than " + length);
}
      }
      if (length == 0) {
        if (!throwException) {
          return null;
        } else {
          throw new FormatException();
        }
      }
      var negative = false;
      int endStr = tmpoffset + length;
      if (chars[tmpoffset] == '+' || chars[tmpoffset] == '-') {
        negative = chars[tmpoffset] == '-';
        ++tmpoffset;
      }
      var numerInt = 0;
      EInteger numer = null;
      var haveDigits = false;
      var haveDenominator = false;
      var ndenomInt = 0;
      EInteger ndenom = null;
      int i = tmpoffset;
      if (i + 8 == endStr) {
        if ((chars[i] == 'I' || chars[i] == 'i') &&
          (chars[i + 1] == 'N' || chars[i + 1] == 'n') &&
          (chars[i + 2] == 'F' || chars[i + 2] == 'f') &&
          (chars[i + 3] == 'I' || chars[i + 3] == 'i') && (chars[i + 4] ==
            'N' ||
            chars[i + 4] == 'n') && (chars[i + 5] == 'I' || chars[i + 5] ==
            'i') &&
          (chars[i + 6] == 'T' || chars[i + 6] == 't') && (chars[i + 7] ==
            'Y' || chars[i + 7] == 'y')) {
          return negative ? ERational.NegativeInfinity :
            ERational.PositiveInfinity;
        }
      }
      if (i + 3 == endStr) {
        if ((chars[i] == 'I' || chars[i] == 'i') &&
          (chars[i + 1] == 'N' || chars[i + 1] == 'n') && (chars[i + 2] ==
            'F' || chars[i + 2] == 'f')) {
          return negative ? ERational.NegativeInfinity :
            ERational.PositiveInfinity;
        }
      }
      var numerStart = 0;
      if (i + 3 <= endStr) {
        // Quiet NaN
        if ((chars[i] == 'N' || chars[i] == 'n') && (chars[i + 1] == 'A' ||
            chars[i +
              1] == 'a') && (chars[i + 2] == 'N' || chars[i + 2] == 'n')) {
          if (i + 3 == endStr) {
            return (!negative) ? ERational.NaN : ERational.NaN.Negate();
          }
          i += 3;
          numerStart = i;
          for (; i < endStr; ++i) {
            if (chars[i] >= '0' && chars[i] <= '9') {
              var thisdigit = (int)(chars[i] - '0');
              if (numerInt <= MaxSafeInt) {
                numerInt *= 10;
                numerInt += thisdigit;
              }
            } else {
              if (!throwException) {
                return null;
              } else {
                throw new FormatException();
              }
            }
          }
          if (numerInt > MaxSafeInt) {
            numer = EInteger.FromSubstring(chars, numerStart, endStr);
            return ERational.CreateNaN(numer, false, negative);
          } else {
            return ERational.CreateNaN(
                EInteger.FromInt32(numerInt),
                false,
                negative);
          }
        }
      }
      if (i + 4 <= endStr) {
        // Signaling NaN
        if ((chars[i] == 'S' || chars[i] == 's') && (chars[i + 1] == 'N' ||
            chars[i +
              1] == 'n') && (chars[i + 2] == 'A' || chars[i + 2] == 'a') &&
          (chars[i + 3] == 'N' || chars[i + 3] == 'n')) {
          if (i + 4 == endStr) {
            return (!negative) ? ERational.SignalingNaN :
              ERational.SignalingNaN.Negate();
          }
          i += 4;
          numerStart = i;
          for (; i < endStr; ++i) {
            if (chars[i] >= '0' && chars[i] <= '9') {
              var thisdigit = (int)(chars[i] - '0');
              haveDigits = haveDigits || thisdigit != 0;
              if (numerInt <= MaxSafeInt) {
                numerInt *= 10;
                numerInt += thisdigit;
              }
            } else {
              if (!throwException) {
                return null;
              } else {
                throw new FormatException();
              }
            }
          }
          int flags3 = (negative ? BigNumberFlags.FlagNegative : 0) |
            BigNumberFlags.FlagSignalingNaN;
          if (numerInt > MaxSafeInt) {
            numer = EInteger.FromSubstring(chars, numerStart, endStr);
            return ERational.CreateNaN(numer, true, negative);
          } else {
            return ERational.CreateNaN(
                EInteger.FromInt32(numerInt),
                true,
                negative);
          }
        }
      }
      // Ordinary number
      numerStart = i;
      int numerEnd = i;
      for (; i < endStr; ++i) {
        if (chars[i] >= '0' && chars[i] <= '9') {
          var thisdigit = (int)(chars[i] - '0');
          numerEnd = i + 1;
          if (numerInt <= MaxSafeInt) {
            numerInt *= 10;
            numerInt += thisdigit;
          }
          haveDigits = true;
        } else if (chars[i] == '/') {
          haveDenominator = true;
          ++i;
          break;
        } else {
          if (!throwException) {
            return null;
          } else {
            throw new FormatException();
          }
        }
      }
      if (!haveDigits) {
        if (!throwException) {
          return null;
        } else {
          throw new FormatException();
        }
      }
      if (numerInt > MaxSafeInt) {
        numer = EInteger.FromSubstring(chars, numerStart, numerEnd);
      }
      if (haveDenominator) {
        EInteger denom = null;
        var denomInt = 0;
        tmpoffset = 1;
        haveDigits = false;
        if (i == endStr) {
          if (!throwException) {
            return null;
          } else {
            throw new FormatException();
          }
        }
        numerStart = i;
        for (; i < endStr; ++i) {
          if (chars[i] >= '0' && chars[i] <= '9') {
            haveDigits = true;
            var thisdigit = (int)(chars[i] - '0');
            numerEnd = i + 1;
            if (denomInt <= MaxSafeInt) {
              denomInt *= 10;
              denomInt += thisdigit;
            }
          } else {
            if (!throwException) {
              return null;
            } else {
              throw new FormatException();
            }
          }
        }
        if (!haveDigits) {
          if (!throwException) {
            return null;
          } else {
            throw new FormatException();
          }
        }
        if (denomInt > MaxSafeInt) {
          denom = EInteger.FromSubstring(chars, numerStart, numerEnd);
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
        if (!throwException) {
          return null;
        } else {
          throw new FormatException();
        }
      }
      if (ndenom == null ? (ndenomInt == 0) : ndenom.IsZero) {
        if (!throwException) {
          return null;
        } else {
          throw new FormatException();
        }
      }
      ERational erat = ERational.Create(
          numer == null ? (EInteger)numerInt : numer,
          ndenom == null ? (EInteger)ndenomInt : ndenom);
      return negative ? erat.Negate() : erat;
    }
  }
}
