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
  /// <summary>Common interface for classes that shift a number of digits
  /// and record information on whether a non-zero digit was discarded
  /// this way.</summary>
  internal interface IShiftAccumulator {
    EInteger ShiftedInt {
      get;
    }

    FastInteger GetDigitLength();

    FastInteger OverestimateDigitLength();

    int OlderDiscardedDigits {
      get;
    }

    int LastDiscardedDigit {
      get;
    }

    FastInteger ShiftedIntFast {
      get;
    }

    FastInteger DiscardedDigitCount {
      get;
    }

    void TruncateOrShiftRight(FastInteger bits, bool truncate);

    int ShiftedIntMod(int mod);

    void ShiftRightInt(int bits);

    void ShiftToDigits(FastInteger bits, FastInteger preShift, bool truncate);
  }
}
