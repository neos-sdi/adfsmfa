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
    /// path='docs/doc[@name="T:PeterO.Numbers.IShiftAccumulator"]/*'/>
  internal interface IShiftAccumulator {
    EInteger ShiftedInt { get; }

    FastInteger GetDigitLength();

    int OlderDiscardedDigits { get; }

    int LastDiscardedDigit { get; }

    FastInteger ShiftedIntFast { get; }

    FastInteger DiscardedDigitCount { get; }

    void TruncateRight(FastInteger bits);

    void ShiftRight(FastInteger bits);

    void ShiftRightInt(int bits);

    void ShiftToDigits(FastInteger bits, FastInteger preShift, bool truncate);
  }
}
