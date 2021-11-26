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
  // <summary>Implements arithmetic methods that support
  // traps.</summary>
  // <typeparam name='T'>Data type for a numeric value in a particular
  // radix.</typeparam>
  internal class TrappableRadixMath<T> : IRadixMath<T> {
    private readonly IRadixMath<T> math;

    public TrappableRadixMath(IRadixMath<T> math) {
      #if DEBUG
      if (math == null) {
        throw new ArgumentNullException(nameof(math));
      }
      #endif
      this.math = math;
    }

    public T DivideToIntegerNaturalScale(
      T thisValue,
      T divisor,
      EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.DivideToIntegerNaturalScale(
        thisValue,
        divisor,
        tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T DivideToIntegerZeroScale(
      T thisValue,
      T divisor,
      EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.DivideToIntegerZeroScale(thisValue, divisor, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T Abs(T value, EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.Abs(value, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T Negate(T value, EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.Negate(value, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T Remainder(
      T thisValue,
      T divisor,
      EContext ctx,
      bool roundAfterDivide) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.Remainder(
        thisValue,
        divisor,
        tctx,
        roundAfterDivide);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public IRadixMathHelper<T> GetHelper() {
      return this.math.GetHelper();
    }

    public T RemainderNear(T thisValue, T divisor, EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.RemainderNear(thisValue, divisor, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T Pi(EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.Pi(tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T Power(T thisValue, T pow, EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.Power(thisValue, pow, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T Ln(T thisValue, EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.Ln(thisValue, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T Exp(T thisValue, EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.Exp(thisValue, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T SquareRoot(T thisValue, EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.SquareRoot(thisValue, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T NextMinus(T thisValue, EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.NextMinus(thisValue, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T NextToward(T thisValue, T otherValue, EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.NextToward(thisValue, otherValue, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T NextPlus(T thisValue, EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.NextPlus(thisValue, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T DivideToExponent(
      T thisValue,
      T divisor,
      EInteger desiredExponent,
      EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.DivideToExponent(
        thisValue,
        divisor,
        desiredExponent,
        tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T Divide(T thisValue, T divisor, EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.Divide(thisValue, divisor, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T MinMagnitude(T a, T b, EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.MinMagnitude(a, b, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T MaxMagnitude(T a, T b, EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.MaxMagnitude(a, b, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T Max(T a, T b, EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.Max(a, b, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T Min(T a, T b, EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.Min(a, b, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T Multiply(T thisValue, T other, EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.Multiply(thisValue, other, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T MultiplyAndAdd(
      T thisValue,
      T multiplicand,
      T augend,
      EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.MultiplyAndAdd(
        thisValue,
        multiplicand,
        augend,
        tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T Plus(T thisValue, EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.Plus(thisValue, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T RoundToPrecision(T thisValue, EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.RoundToPrecision(thisValue, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T Quantize(T thisValue, T otherValue, EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.Quantize(thisValue, otherValue, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T RoundToExponentExact(
      T thisValue,
      EInteger expOther,
      EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.RoundToExponentExact(thisValue, expOther, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T RoundToExponentSimple(
      T thisValue,
      EInteger expOther,
      EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.RoundToExponentSimple(thisValue, expOther, ctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T RoundToExponentNoRoundedFlag(
      T thisValue,
      EInteger exponent,
      EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.RoundToExponentNoRoundedFlag(
        thisValue,
        exponent,
        ctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T Reduce(T thisValue, EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.Reduce(thisValue, ctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T Add(T thisValue, T other, EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.Add(thisValue, other, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T CompareToWithContext(
      T thisValue,
      T otherValue,
      bool treatQuietNansAsSignaling,
      EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.CompareToWithContext(
        thisValue,
        otherValue,
        treatQuietNansAsSignaling,
        tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    // <summary>Compares a T object with this instance.</summary>
    // <param name='thisValue'></param>
    // <returns>Zero if the values are equal; a negative number if this
    // instance is less, or a positive number if this instance is
    // greater.</returns>
    public int CompareTo(T thisValue, T otherValue) {
      return this.math.CompareTo(thisValue, otherValue);
    }

    public T RoundAfterConversion(T thisValue, EContext ctx) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.RoundAfterConversion(thisValue, tctx);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T SignalOverflow(EContext ctx, bool neg) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.SignalOverflow(tctx, neg);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }

    public T AddEx(
      T thisValue,
      T other,
      EContext ctx,
      bool roundToOperandPrecision) {
      EContext tctx = (ctx == null) ? ctx : ctx.GetNontrapping();
      T result = this.math.AddEx(
        thisValue,
        other,
        ctx,
        roundToOperandPrecision);
      return ctx == null ? result : ctx.TriggerTraps(result, tctx);
    }
  }
}
