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

using Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor.Numbers;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor
{
  internal interface ICBORNumber {
    bool IsPositiveInfinity(Object obj);

    bool IsInfinity(Object obj);

    bool IsNegativeInfinity(Object obj);

    bool IsNaN(Object obj);

    bool IsNegative(Object obj);

    double AsDouble(Object obj);

    object Negate(Object obj);

    object Abs(Object obj);

    EDecimal AsEDecimal(Object obj);

    EFloat AsEFloat(Object obj);

    ERational AsERational(Object obj);

    float AsSingle(Object obj);

    EInteger AsEInteger(Object obj);

    long AsInt64(Object obj);

    bool CanFitInSingle(Object obj);

    bool CanFitInDouble(Object obj);

    bool CanFitInInt32(Object obj);

    bool CanFitInInt64(Object obj);

    bool CanTruncatedIntFitInInt64(Object obj);

    bool CanTruncatedIntFitInInt32(Object obj);

    int AsInt32(Object obj, int minValue, int maxValue);

    bool IsNumberZero(Object obj);

    int Sign(Object obj);

    bool IsIntegral(Object obj);
  }
}
