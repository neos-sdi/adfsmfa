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
  /// <summary>Exception thrown for arithmetic trap errors. (The "E"
  /// stands for "extended", and has this prefix to group it with the
  /// other classes common to this library, particularly EDecimal,
  /// EFloat, and ERational.).
  /// <para>This library may throw exceptions of this type in certain
  /// cases, notably when errors occur, and may supply messages to those
  /// exceptions (the message can be accessed through the <c>Message</c>
  /// property in.NET or the <c>getMessage()</c> method in Java). These
  /// messages are intended to be read by humans to help diagnose the
  /// error (or other cause of the exception); they are not intended to
  /// be parsed by computer programs, and the exact text of the messages
  /// may change at any time between versions of this
  /// library.</para></summary>
  #if NET20 || NET40
  [Serializable]
  #endif
  public sealed class ETrapException : ArithmeticException {
    private readonly Object result;
    private readonly EContext ctx;

    /// <summary>Gets the arithmetic context used during the operation that
    /// triggered the trap. May be null.</summary>
    /// <value>The arithmetic context used during the operation that
    /// triggered the trap. May be null.</value>
    public EContext Context {
      get {
        return this.ctx;
      }
    }

    private readonly int error;

    private readonly int errors;

    /// <summary>Initializes a new instance of the
    /// <see cref='PeterO.Numbers.ETrapException'/> class.</summary>
    public ETrapException() : this(FlagToMessage(EContext.FlagInvalid)) {
    }

    /// <summary>Initializes a new instance of the
    /// <see cref='PeterO.Numbers.ETrapException'/> class.</summary>
    /// <param name='message'>The parameter <paramref name='message'/> is a
    /// text string.</param>
    public ETrapException(string message) : base(message) {
      this.error = EContext.FlagInvalid;
      this.errors = EContext.FlagInvalid;
      this.ctx = null;
      this.result = null;
    }

    /// <summary>Initializes a new instance of the
    /// <see cref='PeterO.Numbers.ETrapException'/> class.</summary>
    /// <param name='message'>The parameter <paramref name='message'/> is a
    /// text string.</param>
    /// <param name='innerException'>The parameter <paramref
    /// name='innerException'/> is an Exception object.</param>
    public ETrapException(string message, Exception innerException)
      : base(message, innerException) {
      this.error = EContext.FlagInvalid;
      this.errors = EContext.FlagInvalid;
      this.ctx = (this.ctx == null) ? null : this.ctx.Copy();
      this.result = null;
    }

    /// <summary>Gets the defined result of the operation that caused the
    /// trap.</summary>
    /// <value>The defined result of the operation that caused the
    /// trap.</value>
    public Object Result {
      get {
        return this.result;
      }
    }

    /// <summary>Gets the flag that specifies the primary kind of error in
    /// one or more operations (EContext.FlagXXX). This will only be one
    /// flag, such as <c>FlagInexact</c> or FlagSubnormal.</summary>
    /// <value>The flag that specifies the primary kind of error in one or
    /// more operations.</value>
    public int Error {
      get {
        return this.error;
      }
    }

    /// <summary>Gets the flags that were signaled as the result of one or
    /// more operations. This includes the flag specified in the "flag"
    /// parameter, but can include other flags. For instance, if "flag" is
    /// <c>EContext.FlagInexact</c>, this parameter might be
    /// <c>EContext.FlagInexact | EContext.FlagRounded</c>.</summary>
    /// <value>The flags that specify the errors in one or more
    /// operations.</value>
    public int Errors {
      get {
        return this.errors;
      }
    }

    /// <summary>Returns whether this trap exception specifies all the
    /// flags given. (Flags are signaled in a trap exception as the result
    /// of one or more operations involving arbitrary-precision numbers,
    /// such as multiplication of two EDecimals.).</summary>
    /// <param name='flag'>A combination of one or more flags, such as
    /// <c>EContext.FlagInexact | EContext.FlagRounded</c>.</param>
    /// <returns>True if this exception pertains to all of the flags given
    /// in <paramref name='flag'/> ; otherwise, false.</returns>
    public bool HasError(int flag) {
      return (this.Error & flag) == flag;
    }

    private static string FlagToMessage(int flags) {
      var sb = new System.Text.StringBuilder();
      var first = true;
      for (var i = 0; i < 32; ++i) {
        int flag = 1 << i;
        if ((flags & flag) != 0) {
          if (!first) {
            sb.Append(", ");
          }
          first = false;
          string str = (flag == EContext.FlagClamped) ? "Clamped" : ((flag ==
                EContext.FlagDivideByZero) ? "DivideByZero" : ((flag ==
                  EContext.FlagInexact) ? "Inexact" : ((flag ==
                    EContext.FlagInvalid) ? "Invalid" : ((flag ==
                      EContext.FlagOverflow) ? "Overflow" : ((flag ==
                        EContext.FlagRounded) ? "Rounded" : ((flag ==
                          EContext.FlagSubnormal) ? "Subnormal" : ((flag ==
                            EContext.FlagUnderflow) ? "Underflow" :
"Trap")))))));
          sb.Append(str);
        }
      }
      return sb.ToString();
    }

    /// <summary>Initializes a new instance of the
    /// <see cref='PeterO.Numbers.ETrapException'/> class.</summary>
    /// <param name='flag'>The flag that specifies the kind of error from
    /// one or more operations (EContext.FlagXXX). This will only be one
    /// flag, such as <c>FlagInexact</c> or FlagSubnormal.</param>
    /// <param name='ctx'>The arithmetic context used during the operation
    /// that triggered the trap. Can be null.</param>
    /// <param name='result'>The defined result of the operation that
    /// caused the trap.</param>
    public ETrapException(int flag, EContext ctx, Object result)
      : this(flag, flag, ctx, result) {
    }

    /// <summary>Initializes a new instance of the
    /// <see cref='PeterO.Numbers.ETrapException'/> class.</summary>
    /// <param name='flags'>Specifies the flags that were signaled as the
    /// result of one or more operations. This includes the flag specified
    /// in the "flag" parameter, but can include other flags. For instance,
    /// if "flag" is <c>EContext.FlagInexact</c>, this parameter might be
    /// <c>EContext.FlagInexact | EContext.FlagRounded</c>.</param>
    /// <param name='flag'>Specifies the flag that specifies the primary
    /// kind of error from one or more operations (EContext.FlagXXX). This
    /// will only be one flag, such as <c>FlagInexact</c> or
    /// FlagSubnormal.</param>
    /// <param name='ctx'>The arithmetic context used during the operation
    /// that triggered the trap. Can be null.</param>
    /// <param name='result'>The defined result of the operation that
    /// caused the trap.</param>
    /// <exception cref='ArgumentException'>The parameter <paramref
    /// name='flags'/> doesn't include all the flags in the <paramref
    /// name='flag'/> parameter.</exception>
    public ETrapException(int flags, int flag, EContext ctx, Object result)
      : base(FlagToMessage(flags)) {
      if ((flags & flag) != flag) {
        throw new ArgumentException("flags doesn't include flag");
      }
      this.error = flag;
      this.errors = flags;
      this.ctx = (ctx == null) ? null : ctx.Copy();
      this.result = result;
    }

    #if NET20 || NET40
    private ETrapException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context)
      : base(info, context) {
    }
    #endif
  }
}
