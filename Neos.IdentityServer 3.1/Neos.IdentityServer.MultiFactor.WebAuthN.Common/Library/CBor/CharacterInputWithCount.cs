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
using System.IO;
using Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor.Numbers;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor
{
  internal class CharacterInputWithCount : ICharacterInput {
    private readonly ICharacterInput ci;
    private int offset;

    public CharacterInputWithCount(ICharacterInput ci) {
      this.ci = ci;
    }

    public int GetOffset() {
      return this.offset;
    }

    public void RaiseError(string str) {
      throw new CBORException(this.NewErrorString(str));
    }

    public void RaiseError(Exception ex) {
      if (ex.InnerException == null) {
        throw new CBORException(
            this.NewErrorString(ex.Message),
            ex);
      } else {
        throw new CBORException(
            this.NewErrorString(ex.Message),
            ex.InnerException);
      }
    }

    public int Read(int[] chars, int index, int length) {
      if (chars == null) {
        throw new ArgumentNullException(nameof(chars));
      }
      if (index < 0) {
        throw new ArgumentException("index(" + index +
          ") is less than 0");
      }
      if (index > chars.Length) {
        throw new ArgumentException("index(" + index +
          ") is more than " + chars.Length);
      }
      if (length < 0) {
        throw new ArgumentException("length(" + length +
          ") is less than 0");
      }
      if (length > chars.Length) {
        throw new ArgumentException("length(" + length +
          ") is more than " + chars.Length);
      }
      if (chars.Length - index < length) {
        throw new ArgumentException("chars's length minus " + index + "(" +
          (chars.Length - index) + ") is less than " + length);
      }
      int ret = this.ci.Read(chars, index, length);
      if (ret > 0) {
        this.offset += ret;
      }
      return ret;
    }

    public int ReadChar() {
      var c = -1;
      try {
        c = this.ci.ReadChar();
      } catch (InvalidOperationException ex) {
        this.RaiseError(ex);
      }
      if (c >= 0) {
        ++this.offset;
      }
      return c;
    }

    private string NewErrorString(string str) {
      return str + " (offset " + this.GetOffset() + ")";
    }
  }
}
