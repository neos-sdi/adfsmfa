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
  internal class CBORDateConverter : ICBORToFromConverter<DateTime> {
    private static string DateTimeToString(DateTime bi) {
      var lesserFields = new int[7];
      var year = new EInteger[1];
      PropertyMap.BreakDownDateTime(bi, year, lesserFields);
      return CBORUtilities.ToAtomDateTimeString(year[0], lesserFields);
    }

    public DateTime FromCBORObject(CBORObject obj) {
      if (obj.HasMostOuterTag(0)) {
        try {
          return StringToDateTime(obj.AsString());
        } catch (OverflowException ex) {
          throw new CBORException(ex.Message, ex);
        } catch (ArgumentException ex) {
          throw new CBORException(ex.Message, ex);
        }
      } else if (obj.HasMostOuterTag(1)) {
        if (!obj.IsNumber || !obj.AsNumber().IsFinite()) {
          throw new CBORException("Not a finite number");
        }
        EDecimal dec;
        dec = (EDecimal)obj.ToObject(typeof(EDecimal));
        var lesserFields = new int[7];
        var year = new EInteger[1];
        CBORUtilities.BreakDownSecondsSinceEpoch(
          dec,
          year,
          lesserFields);
        return PropertyMap.BuildUpDateTime(year[0], lesserFields);
      }
      throw new CBORException("Not tag 0 or 1");
    }

    public static DateTime StringToDateTime(string str) {
      var lesserFields = new int[7];
      var year = new EInteger[1];
      CBORUtilities.ParseAtomDateTimeString(str, year, lesserFields);
      return PropertyMap.BuildUpDateTime(year[0], lesserFields);
    }

    public CBORObject ToCBORObject(DateTime obj) {
      return CBORObject.FromObjectAndTag(DateTimeToString(obj), 0);
    }
  }
}
