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
  /// <summary>Specifies the mode to use when "shortening" numbers that
  /// otherwise can't fit a given number of digits, so that the shortened
  /// number has about the same value. This "shortening" is known as
  /// rounding. (The "E" stands for "extended", and has this prefix to
  /// group it with the other classes common to this library,
  /// particularly EDecimal, EFloat, and ERational.).</summary>
  public enum ERounding {
    /// <summary>Indicates that rounding will not be used. If rounding to
    /// an inexact value is required, the rounding operation will report an
    /// error.</summary>
    None,

    /// <summary>If there is a fractional part, the number is rounded to
    /// the closest representable number away from zero.</summary>
    Up,

    /// <summary>The fractional part is discarded (the number is
    /// truncated).</summary>
    Down,

    /// <summary>Rounded to the nearest number; if the fractional part is
    /// exactly half, the number is rounded to the closest representable
    /// number away from zero. This is the most familiar rounding mode for
    /// many people.</summary>
    HalfUp,

    /// <summary>Rounded to the nearest number; if the fractional part is
    /// exactly half, it is discarded.</summary>
    HalfDown,

    /// <summary>Rounded to the nearest number; if the fractional part is
    /// exactly half, the number is rounded to the closest representable
    /// number that is even. This is sometimes also known as "banker's
    /// rounding".</summary>
    HalfEven,

    /// <summary>If there is a fractional part, the number is rounded to
    /// the highest representable number that's closest to it.</summary>
    Ceiling,

    /// <summary>If there is a fractional part, the number is rounded to
    /// the lowest representable number that's closest to it.</summary>
    Floor,

    /// <summary>If there is a fractional part and the whole number part is
    /// even, the number is rounded to the closest representable odd number
    /// away from zero.</summary>
    [Obsolete("Consider using ERounding.OddOrZeroFiveUp instead.")]
    Odd,

    /// <summary>If there is a fractional part and if the last digit before
    /// rounding is 0 or half the radix, the number is rounded to the
    /// closest representable number away from zero; otherwise the
    /// fractional part is discarded. In overflow, the fractional part is
    /// always discarded.</summary>
    [Obsolete("Use ERounding.OddOrZeroFiveUp instead.")]
    ZeroFiveUp,

    /// <summary>For binary floating point numbers, this is the same as
    /// Odd. For other bases (including decimal numbers), this is the same
    /// as ZeroFiveUp. This rounding mode is useful for rounding
    /// intermediate results at a slightly higher precision (at least 2
    /// bits more for binary) than the final precision.</summary>
    OddOrZeroFiveUp,
  }
}
