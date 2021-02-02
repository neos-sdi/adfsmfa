//******************************************************************************************************************************************************************************************//
//                                                                                                                                                                                          //
// Written by Peter O.                                                                                                                                                                      //
//                                                                                                                                                                                          //
// Any copyright is dedicated to the Public Domain.                                                                                                                                         //
// http://creativecommons.org/publicdomain/zero/1.0/                                                                                                                                        //
// If you like this, you should donate to Peter O. at: http://peteroupc.github.io/                                                                                                          //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using System;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor
{
    #region ICBORConverter   
    /// <summary>Interface implemented by classes that convert objects of
    /// arbitrary types to CBOR objects.</summary>
    /// <typeparam name='T'>Type to convert to a CBOR object.</typeparam>
    public interface ICBORConverter<T>
    {
        /// <summary>Converts an object to a CBOR object.</summary>
        /// <param name='obj'>An object to convert to a CBOR object.</param>
        /// <returns>A CBOR object.</returns>
        CBORObject ToCBORObject(T obj);
    }
    #endregion

    #region ICBORNumber
    /// <summary>This is an internal API.</summary>
    internal interface ICBORNumber
    {
        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        bool IsPositiveInfinity(Object obj);

        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        bool IsInfinity(Object obj);

        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        bool IsNegativeInfinity(Object obj);

        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        bool IsNaN(Object obj);

        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        bool IsNegative(Object obj);

        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        double AsDouble(Object obj);

        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        object Negate(Object obj);

        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        object Abs(Object obj);

        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        EDecimal AsEDecimal(Object obj);

        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        EFloat AsEFloat(Object obj);

        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        ERational AsERational(Object obj);

        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        float AsSingle(Object obj);

        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        EInteger AsEInteger(Object obj);

        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        long AsInt64(Object obj);

        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        bool CanFitInSingle(Object obj);

        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        bool CanFitInDouble(Object obj);

        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        bool CanFitInInt32(Object obj);

        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        bool CanFitInInt64(Object obj);

        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        bool CanTruncatedIntFitInInt64(Object obj);

        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        bool CanTruncatedIntFitInInt32(Object obj);

        /// <summary>This is an internal API.</summary>
        /// <returns>The return value is an internal value.</returns>
        int AsInt32(Object obj, int minValue, int maxValue);

        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        bool IsNumberZero(Object obj);

        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        int Sign(Object obj);

        /// <summary>This is an internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns>The return value is an internal value.</returns>
        bool IsIntegral(Object obj);
    }
    #endregion

    /*
    #region ICBORTag
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:ICBORTag"]/*'/>
    public interface ICBORTag
    {
        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:ICBORTag.GetTypeFilter"]/*'/>
        CBORTypeFilter GetTypeFilter();

        /// <include file='../../docs.xml'
        /// path='docs/doc[@name="M:ICBORTag.ValidateObject(CBORObject)"]/*'/>
        CBORObject ValidateObject(CBORObject obj);
    }
    #endregion
    */

    #region ICBORToFromConverter
    /// <summary>Classes that implement this interface can support
    /// conversions from CBOR objects to a custom type and back.</summary>
    /// <typeparam name='T'>Type of objects to convert to and from CBOR
    /// objects.</typeparam>
    public interface ICBORToFromConverter<T> : ICBORConverter<T>
    {
        /// <summary>Converts a CBOR object to a custom type.</summary>
        /// <param name='obj'>A CBOR object to convert to the custom
        /// type.</param>
        /// <returns>An object of the custom type after conversion.</returns>
        T FromCBORObject(CBORObject obj);
    }
    #endregion

    #region ICharacterInput
    /// <summary>An interface for reading Unicode characters from a data
    /// source.</summary>
    internal interface ICharacterInput
    {
        /// <summary>Reads a Unicode character from a data source.</summary>
        /// <returns>Either a Unicode code point (from 0-0xd7ff or from 0xe000
        /// to 0x10ffff), or the value -1 indicating the end of the
        /// source.</returns>
        int ReadChar();

        /// <summary>Reads a sequence of Unicode code points from a data
        /// source.</summary>
        /// <param name='chars'>Output buffer.</param>
        /// <param name='index'>Index in the output buffer to start writing
        /// to.</param>
        /// <param name='length'>Maximum number of code points to
        /// write.</param>
        /// <returns>Either a Unicode code point (from 0-0xd7ff or from 0xe000
        /// to 0x10ffff), or the value -1 indicating the end of the
        /// source.</returns>
        /// <exception cref='ArgumentException'>Either &#x22;index&#x22; or
        /// &#x22;length&#x22; is less than 0 or greater than
        /// &#x22;chars&#x22;&#x27;s length, or &#x22;chars&#x22;&#x27;s length
        /// minus &#x22;index&#x22; is less than
        /// &#x22;length&#x22;.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='chars'/> is null.</exception>
        int Read(int[] chars, int index, int length);
    }
    #endregion
}
