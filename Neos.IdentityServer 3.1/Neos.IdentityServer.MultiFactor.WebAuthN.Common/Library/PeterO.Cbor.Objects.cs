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
using System.Collections.Generic;
using System.Text;
using System.IO;


#pragma warning disable 618

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor
{
    #region CBORObject
    /// <summary>
    /// <para>Represents an object in Concise Binary Object Representation
    /// (CBOR) and contains methods for reading and writing CBOR data. CBOR
    /// is defined in RFC 7049.</para></summary>
    /// <remarks>
    /// <para><b>Converting CBOR objects</b></para>
    /// <para>There are many ways to get a CBOR object, including from
    /// bytes, objects, streams and JSON, as described below.</para>
    /// <para><b>To and from byte arrays:</b> The
    /// CBORObject.DecodeFromBytes method converts a byte array in CBOR
    /// format to a CBOR object. The EncodeToBytes method converts a CBOR
    /// object to its corresponding byte array in CBOR format.</para>
    /// <para><b>To and from data streams:</b> The CBORObject.Write methods
    /// write many kinds of objects to a data stream, including numbers,
    /// CBOR objects, strings, and arrays of numbers and strings. The
    /// CBORObject.Read method reads a CBOR object from a data
    /// stream.</para>
    /// <para><b>To and from other objects:</b> The
    /// <c>CBORObject.FromObject</c> method converts many kinds of objects
    /// to a CBOR object, including numbers, strings, and arrays and maps
    /// of numbers and strings. Methods like AsNumber and AsString convert
    /// a CBOR object to different types of object. The
    /// <c>CBORObject.ToObject</c> method converts a CBOR object to an
    /// object of a given type; for example, a CBOR array to a native
    /// <c>List</c> (or <c>ArrayList</c> in Java), or a CBOR integer to an
    /// <c>int</c> or <c>long</c>.</para>
    /// <para><b>To and from JSON:</b> This class also doubles as a reader
    /// and writer of JavaScript Object Notation (JSON). The
    /// CBORObject.FromJSONString method converts JSON to a CBOR object,
    /// and the ToJSONString method converts a CBOR object to a JSON
    /// string. (Note that the conversion from CBOR to JSON is not always
    /// without loss and may make it impossible to recover the original
    /// object when converting the JSON back to CBOR. See the ToJSONString
    /// documentation.)</para>
    /// <para>In addition, the CBORObject.WriteJSON method writes many
    /// kinds of objects as JSON to a data stream, including numbers, CBOR
    /// objects, strings, and arrays of numbers and strings. The
    /// CBORObject.Read method reads a CBOR object from a JSON data
    /// stream.</para>
    /// <para><b>Comparison Considerations:</b></para>
    /// <para>Instances of CBORObject should not be compared for equality
    /// using the "==" operator; it's possible to create two CBOR objects
    /// with the same value but not the same reference. (The "==" operator
    /// might only check if each side of the operator is the same
    /// instance.)</para>
    /// <para>This class's natural ordering (under the CompareTo method) is
    /// consistent with the Equals method, meaning that two values that
    /// compare as equal under the CompareTo method are also equal under
    /// the Equals method; this is a change in version 4.0. Two otherwise
    /// equal objects with different tags are not treated as equal by both
    /// CompareTo and Equals. To strip the tags from a CBOR object before
    /// comparing, use the <c>Untag</c> method.</para>
    /// <para><b>Thread Safety:</b></para>
    /// <para>Certain CBOR objects are immutable (their values can't be
    /// changed), so they are inherently safe for use by multiple
    /// threads.</para>
    /// <para>CBOR objects that are arrays, maps, and byte strings (whether
    /// or not they are tagged) are mutable, but this class doesn't attempt
    /// to synchronize reads and writes to those objects by multiple
    /// threads, so those objects are not thread safe without such
    /// synchronization.</para>
    /// <para>One kind of CBOR object is called a map, or a list of
    /// key-value pairs. Keys can be any kind of CBOR object, including
    /// numbers, strings, arrays, and maps. However, untagged text strings
    /// (which means GetTags returns an empty array and the Type property,
    /// or "getType()" in Java, returns TextString) are the most suitable
    /// to use as keys; other kinds of CBOR object are much better used as
    /// map values instead, keeping in mind that some of them are not
    /// thread safe without synchronizing reads and writes to them.</para>
    /// <para>To find the type of a CBOR object, call its Type property (or
    /// "getType()" in Java). The return value can be Integer,
    /// FloatingPoint, Boolean, SimpleValue, or TextString for immutable
    /// CBOR objects, and Array, Map, or ByteString for mutable CBOR
    /// objects.</para>
    /// <para><b>Nesting Depth:</b></para>
    /// <para>The DecodeFromBytes and Read methods can only read objects
    /// with a limited maximum depth of arrays and maps nested within other
    /// arrays and maps. The code sets this maximum depth to 500 (allowing
    /// more than enough nesting for most purposes), but it's possible that
    /// stack overflows in some runtimes might lower the effective maximum
    /// nesting depth. When the nesting depth goes above 500, the
    /// DecodeFromBytes and Read methods throw a CBORException.</para>
    /// <para>The ReadJSON and FromJSONString methods currently have
    /// nesting depths of 1000.</para></remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Design",
        "CA1036",
        Justification = "Mutable in some cases, and arbitrary size.")]
    public sealed partial class CBORObject : IComparable<CBORObject>,
      IEquatable<CBORObject>
    {
        private static CBORObject ConstructSimpleValue(int v)
        {
            return new CBORObject(CBORObjectTypeSimpleValue, v);
        }

        private static CBORObject ConstructIntegerValue(int v)
        {
            return new CBORObject(CBORObjectTypeInteger, (long)v);
        }

        /// <summary>Represents the value false.</summary>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Security",
        "CA2104",
        Justification = "This CBORObject is immutable")]
#endif
        public static readonly CBORObject False =
          CBORObject.ConstructSimpleValue(20);

        /// <summary>A not-a-number value.</summary>
        public static readonly CBORObject NaN = CBORObject.FromObject(Double.NaN);

        /// <summary>The value negative infinity.</summary>
        public static readonly CBORObject NegativeInfinity =
          CBORObject.FromObject(Double.NegativeInfinity);

        /// <summary>Represents the value null.</summary>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Security",
        "CA2104",
        Justification = "This CBORObject is immutable")]
#endif
        public static readonly CBORObject Null =
          CBORObject.ConstructSimpleValue(22);

        /// <summary>The value positive infinity.</summary>
        public static readonly CBORObject PositiveInfinity =
          CBORObject.FromObject(Double.PositiveInfinity);

        /// <summary>Represents the value true.</summary>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Security",
        "CA2104",
        Justification = "This CBORObject is immutable")]
#endif
        public static readonly CBORObject True =
          CBORObject.ConstructSimpleValue(21);

        /// <summary>Represents the value undefined.</summary>
#if CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Security",
        "CA2104",
        Justification = "This CBORObject is immutable")]
#endif
        public static readonly CBORObject Undefined =
          CBORObject.ConstructSimpleValue(23);

        /// <summary>Gets a CBOR object for the number zero.</summary>
        public static readonly CBORObject Zero =
          CBORObject.ConstructIntegerValue(0);

        private const int CBORObjectTypeInteger = 0; // -(2^63).. (2^63-1)
        private const int CBORObjectTypeEInteger = 1; // all other integers
        private const int CBORObjectTypeByteString = 2;
        private const int CBORObjectTypeTextString = 3;
        private const int CBORObjectTypeArray = 4;
        private const int CBORObjectTypeMap = 5;
        private const int CBORObjectTypeTagged = 6;
        private const int CBORObjectTypeSimpleValue = 7;
        private const int CBORObjectTypeDouble = 8;
        private const int CBORObjectTypeTextStringUtf8 = 9;

        private const int StreamedStringBufferLength = 4096;

        private static readonly EInteger UInt64MaxValue =
          (EInteger.One << 64) - EInteger.One;

        private static readonly EInteger[] ValueEmptyTags = new EInteger[0];
        // Expected lengths for each head byte.
        // 0 means length varies. -1 means invalid.
        private static readonly int[] ValueExpectedLengths = {
      1, 1, 1, 1, 1, 1,
      1, 1, 1,
      1, 1, 1, 1, 1, 1, 1, // major type 0
      1, 1, 1, 1, 1, 1, 1, 1, 2, 3, 5, 9, -1, -1, -1, -1,
      1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // major type 1
      1, 1, 1, 1, 1, 1, 1, 1, 2, 3, 5, 9, -1, -1, -1, -1,
      1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, // major type 2
      17, 18, 19, 20, 21, 22, 23, 24, 0, 0, 0, 0, -1, -1, -1, 0,
      1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, // major type 3
      17, 18, 19, 20, 21, 22, 23, 24, 0, 0, 0, 0, -1, -1, -1, 0,
      1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // major type 4
      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, -1, -1, 0,
      1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // major type 5
      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, -1, -1, 0,
      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // major type 6
      0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, -1, -1, -1,
      1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // major type 7
      1, 1, 1, 1, 1, 1, 1, 1, 2, 3, 5, 9, -1, -1, -1, -1,
    };

        private static readonly byte[] ValueFalseBytes = {
      0x66, 0x61, 0x6c,
      0x73, 0x65,
    };

        private static readonly byte[] ValueNullBytes = { 0x6e, 0x75, 0x6c, 0x6c };

        private static readonly byte[] ValueTrueBytes = { 0x74, 0x72, 0x75, 0x65 };

        private static readonly CBORObject[] FixedObjects =
          InitializeFixedObjects();

        private readonly int itemtypeValue;
        private readonly object itemValue;
        private readonly int tagHigh;
        private readonly int tagLow;

        internal CBORObject(CBORObject obj, int tagLow, int tagHigh)
        {
            this.itemtypeValue = CBORObjectTypeTagged;
            this.itemValue = obj;
            this.tagLow = tagLow;
            this.tagHigh = tagHigh;
        }

        internal CBORObject(int type, object item)
        {
#if DEBUG
            if (type == CBORObjectTypeDouble)
            {
                if (!(item is long))
                {
                    throw new ArgumentException("expected long for item type");
                }
            }
            // Check range in debug mode to ensure that Integer and EInteger
            // are unambiguous
            if ((type == CBORObjectTypeEInteger) &&
              ((EInteger)item).CanFitInInt64())
            {
                throw new ArgumentException("arbitrary-precision integer is within" +
                  "\u0020range for Integer");
            }
            if ((type == CBORObjectTypeEInteger) &&
              ((EInteger)item).GetSignedBitLengthAsInt64() > 64)
            {
                throw new ArgumentException("arbitrary-precision integer does not " +
                  "fit major type 0 or 1");
            }
            if (type == CBORObjectTypeArray && !(item is IList<CBORObject>))
            {
                throw new InvalidOperationException();
            }
            // if (type == CBORObjectTypeTextStringUtf8 &&
            // !CBORUtilities.CheckUtf8((byte[])item)) {
            // throw new InvalidOperationException();
            // }
#endif
            this.itemtypeValue = type;
            this.itemValue = item;
            this.tagLow = 0;
            this.tagHigh = 0;
        }

        /// <summary>Gets the number of keys in this map, or the number of
        /// items in this array, or 0 if this item is neither an array nor a
        /// map.</summary>
        /// <value>The number of keys in this map, or the number of items in
        /// this array, or 0 if this item is neither an array nor a
        /// map.</value>
        public int Count
        {
            get
            {
                return (this.Type == CBORType.Array) ? this.AsList().Count :
                  ((this.Type == CBORType.Map) ? this.AsMap().Count : 0);
            }
        }

        /// <summary>Gets the last defined tag for this CBOR data item, or -1
        /// if the item is untagged.</summary>
        /// <value>The last defined tag for this CBOR data item, or -1 if the
        /// item is untagged.</value>
        public EInteger MostInnerTag
        {
            get
            {
                if (!this.IsTagged)
                {
                    return EInteger.FromInt32(-1);
                }
                CBORObject previtem = this;
                var curitem = (CBORObject)this.itemValue;
                while (curitem.IsTagged)
                {
                    previtem = curitem;
                    curitem = (CBORObject)curitem.itemValue;
                }
                if (previtem.tagHigh == 0 && previtem.tagLow >= 0 &&
                  previtem.tagLow < 0x10000)
                {
                    return (EInteger)previtem.tagLow;
                }
                return LowHighToEInteger(
                    previtem.tagLow,
                    previtem.tagHigh);
            }
        }

        /// <summary>Gets a value indicating whether this value is a CBOR false
        /// value, whether tagged or not.</summary>
        /// <value><c>true</c> if this value is a CBOR false value; otherwise,
        /// <c>false</c>.</value>
        public bool IsFalse
        {
            get
            {
                return this.ItemType == CBORObjectTypeSimpleValue && (int)this.ThisItem
                  == 20;
            }
        }

        /// <summary>Gets a value indicating whether this CBOR object
        /// represents a finite number.</summary>
        /// <value><c>true</c> if this CBOR object represents a finite number;
        /// otherwise, <c>false</c>.</value>
        [Obsolete("Instead, use the following: \u0028cbor.IsNumber &&" +
            "\u0020cbor.AsNumber().IsFinite()).")]
        public bool IsFinite
        {
            get
            {
                if (this.IsNumber)
                {
                    CBORNumber cn = this.AsNumber();
                    return !cn.IsInfinity() && !cn.IsNaN();
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>Gets a value indicating whether this object represents an
        /// integer number, that is, a number without a fractional part.
        /// Infinity and not-a-number are not considered integers.</summary>
        /// <value><c>true</c> if this object represents an integer number,
        /// that is, a number without a fractional part; otherwise,
        /// <c>false</c>.</value>
        [Obsolete("Instead, use the following: \u0028cbor.IsNumber &&" +
            "\u0020cbor.AsNumber().IsInteger()).")]
        public bool IsIntegral
        {
            get
            {
                CBORNumber cn = CBORNumber.FromCBORObject(this);
                return (cn != null) &&
                  cn.GetNumberInterface().IsIntegral(cn.GetValue());
            }
        }

        /// <summary>Gets a value indicating whether this CBOR object is a CBOR
        /// null value, whether tagged or not.</summary>
        /// <value><c>true</c> if this value is a CBOR null value; otherwise,
        /// <c>false</c>.</value>
        public bool IsNull
        {
            get
            {
                return this.ItemType == CBORObjectTypeSimpleValue && (int)this.ThisItem
                  == 22;
            }
        }

        /// <summary>Gets a value indicating whether this data item has at
        /// least one tag.</summary>
        /// <value><c>true</c> if this data item has at least one tag;
        /// otherwise, <c>false</c>.</value>
        public bool IsTagged
        {
            get
            {
                return this.itemtypeValue == CBORObjectTypeTagged;
            }
        }

        /// <summary>Gets a value indicating whether this value is a CBOR true
        /// value, whether tagged or not.</summary>
        /// <value><c>true</c> if this value is a CBOR true value; otherwise,
        /// <c>false</c>.</value>
        public bool IsTrue
        {
            get
            {
                return this.ItemType == CBORObjectTypeSimpleValue && (int)this.ThisItem
                  == 21;
            }
        }

        /// <summary>Gets a value indicating whether this value is a CBOR
        /// undefined value, whether tagged or not.</summary>
        /// <value><c>true</c> if this value is a CBOR undefined value;
        /// otherwise, <c>false</c>.</value>
        public bool IsUndefined
        {
            get
            {
                return this.ItemType == CBORObjectTypeSimpleValue && (int)this.ThisItem
                  == 23;
            }
        }

        /// <summary>Gets a value indicating whether this object's value equals
        /// 0.</summary>
        /// <value><c>true</c> if this object's value equals 0; otherwise,
        /// <c>false</c>.</value>
        [Obsolete("Instead, use the following: \u0028cbor.IsNumber &&" +
            "\u0020cbor.AsNumber().IsZero()).")]
        public bool IsZero
        {
            get
            {
                CBORNumber cn = CBORNumber.FromCBORObject(this);
                return cn != null &&
                  cn.GetNumberInterface().IsNumberZero(cn.GetValue());
            }
        }

        /// <summary>Gets a collection of the keys of this CBOR object in an
        /// undefined order.</summary>
        /// <value>A collection of the keys of this CBOR object. To avoid
        /// potential problems, the calling code should not modify the CBOR map
        /// or the returned collection while iterating over the returned
        /// collection.</value>
        /// <exception cref='InvalidOperationException'>This object is not a
        /// map.</exception>
        public ICollection<CBORObject> Keys
        {
            get
            {
                if (this.Type == CBORType.Map)
                {
                    IDictionary<CBORObject, CBORObject> dict = this.AsMap();
                    return dict.Keys;
                }
                throw new InvalidOperationException("Not a map");
            }
        }

        /// <summary>Gets a value indicating whether this object is a negative
        /// number.</summary>
        /// <value><c>true</c> if this object is a negative number; otherwise,
        /// <c>false</c>.</value>
        [Obsolete("Instead, use \u0028cbor.IsNumber() &&" +
            "\u0020cbor.AsNumber().IsNegative()).")]
        public bool IsNegative
        {
            get
            {
                CBORNumber cn = CBORNumber.FromCBORObject(this);
                return (cn != null) &&
                  cn.GetNumberInterface().IsNegative(cn.GetValue());
            }
        }

        /// <summary>Gets the outermost tag for this CBOR data item, or -1 if
        /// the item is untagged.</summary>
        /// <value>The outermost tag for this CBOR data item, or -1 if the item
        /// is untagged.</value>
        public EInteger MostOuterTag
        {
            get
            {
                if (!this.IsTagged)
                {
                    return EInteger.FromInt32(-1);
                }
                if (this.tagHigh == 0 &&
                  this.tagLow >= 0 && this.tagLow < 0x10000)
                {
                    return (EInteger)this.tagLow;
                }
                return LowHighToEInteger(
                    this.tagLow,
                    this.tagHigh);
            }
        }

        /// <summary>Gets this value's sign: -1 if negative; 1 if positive; 0
        /// if zero. Throws an exception if this is a not-a-number
        /// value.</summary>
        /// <value>This value's sign: -1 if negative; 1 if positive; 0 if
        /// zero.</value>
        /// <exception cref='InvalidOperationException'>This object does not
        /// represent a number, or this object is a not-a-number (NaN)
        /// value.</exception>
        [Obsolete("Instead, convert this object to a number with .AsNumber()," +
            "\u0020 and use the Sign property in .NET or the signum method in" +
            "\u0020Java." +
            " Either will treat not-a-number (NaN) values differently than here.")]
        public int Sign
        {
            get
            {
                CBORNumber cn = CBORNumber.FromCBORObject(this);
                if (cn == null || cn.IsNaN())
                {
                    throw new InvalidOperationException(
                      "This object is not a number.");
                }
                return cn.GetNumberInterface().Sign(cn.GetValue());
            }
        }

        /// <summary>Gets the simple value ID of this CBOR object, or -1 if the
        /// object is not a simple value. In this method, objects with a CBOR
        /// type of Boolean or SimpleValue are simple values, whether they are
        /// tagged or not.</summary>
        /// <value>The simple value ID of this object if it's a simple value,
        /// or -1 if this object is not a simple value.</value>
        public int SimpleValue
        {
            get
            {
                return (this.ItemType == CBORObjectTypeSimpleValue) ?
                  ((int)this.ThisItem) : -1;
            }
        }

        /// <summary>Gets a value indicating whether this CBOR object stores a
        /// number (including infinity or a not-a-number or NaN value).
        /// Currently, this is true if this item is untagged and has a CBORType
        /// of Integer or FloatingPoint, or if this item has only one tag and
        /// that tag is 2, 3, 4, 5, 30, 264, 265, 268, 269, or 270 with the
        /// right data type.</summary>
        /// <value>A value indicating whether this CBOR object stores a
        /// number.</value>
        public bool IsNumber
        {
            get
            {
                return CBORNumber.IsNumber(this);
            }
        }

        /// <summary>Gets the general data type of this CBOR object. This
        /// method disregards the tags this object has, if any.</summary>
        /// <value>The general data type of this CBOR object.</value>
        public CBORType Type
        {
            get
            {
                switch (this.ItemType)
                {
                    case CBORObjectTypeInteger:
                    case CBORObjectTypeEInteger:
                        return CBORType.Integer;
                    case CBORObjectTypeDouble:
                        return CBORType.FloatingPoint;
                    case CBORObjectTypeSimpleValue:
                        return ((int)this.ThisItem == 21 || (int)this.ThisItem == 20) ?
                          CBORType.Boolean : CBORType.SimpleValue;
                    case CBORObjectTypeArray:
                        return CBORType.Array;
                    case CBORObjectTypeMap:
                        return CBORType.Map;
                    case CBORObjectTypeByteString:
                        return CBORType.ByteString;
                    case CBORObjectTypeTextString:
                    case CBORObjectTypeTextStringUtf8:
                        return CBORType.TextString;
                    default: throw new InvalidOperationException("Unexpected data type");
                }
            }
        }

        /// <summary>Gets a collection of the key/value pairs stored in this
        /// CBOR object, if it's a map. Returns one entry for each key/value
        /// pair in the map in an undefined order.</summary>
        /// <value>A collection of the key/value pairs stored in this CBOR map,
        /// as a read-only view of those pairs. To avoid potential problems,
        /// the calling code should not modify the CBOR map while iterating
        /// over the returned collection.</value>
        /// <exception cref='InvalidOperationException'>This object is not a
        /// map.</exception>
        public ICollection<KeyValuePair<CBORObject, CBORObject>> Entries
        {
            get
            {
                if (this.Type == CBORType.Map)
                {
                    IDictionary<CBORObject, CBORObject> dict = this.AsMap();
                    return PropertyMap.GetEntries(dict);
                }
                throw new InvalidOperationException("Not a map");
            }
        }

        /// <summary>Gets a collection of the values of this CBOR object, if
        /// it's a map or an array. If this object is a map, returns one value
        /// for each key in the map in an undefined order. If this is an array,
        /// returns all the values of the array in the order they are listed.
        /// (This method can't be used to get the bytes in a CBOR byte string;
        /// for that, use the GetByteString method instead.).</summary>
        /// <value>A collection of the values of this CBOR map or array. To
        /// avoid potential problems, the calling code should not modify the
        /// CBOR map or array or the returned collection while iterating over
        /// the returned collection.</value>
        /// <exception cref='InvalidOperationException'>This object is not a
        /// map or an array.</exception>
        public ICollection<CBORObject> Values
        {
            get
            {
                if (this.Type == CBORType.Map)
                {
                    IDictionary<CBORObject, CBORObject> dict = this.AsMap();
                    return dict.Values;
                }
                if (this.Type == CBORType.Array)
                {
                    IList<CBORObject> list = this.AsList();
                    return new
                      System.Collections.ObjectModel.ReadOnlyCollection<CBORObject>(
                        list);
                }
                throw new InvalidOperationException("Not a map or array");
            }
        }

        private int ItemType
        {
            get
            {
                CBORObject curobject = this;
                while (curobject.itemtypeValue == CBORObjectTypeTagged)
                {
                    curobject = (CBORObject)curobject.itemValue;
                }
                return curobject.itemtypeValue;
            }
        }

        private object ThisItem
        {
            get
            {
                CBORObject curobject = this;
                while (curobject.itemtypeValue == CBORObjectTypeTagged)
                {
                    curobject = (CBORObject)curobject.itemValue;
                }
                return curobject.itemValue;
            }
        }

        /// <summary>Gets the value of a CBOR object by integer index in this
        /// array or by integer key in this map.</summary>
        /// <param name='index'>Index starting at 0 of the element, or the
        /// integer key to this map. (If this is a map, the given index can be
        /// any 32-bit signed integer, even a negative one.).</param>
        /// <returns>The CBOR object referred to by index or key in this array
        /// or map. If this is a CBOR map, returns <c>null</c> (not
        /// <c>CBORObject.Null</c> ) if an item with the given key doesn't
        /// exist.</returns>
        /// <exception cref='InvalidOperationException'>This object is not an
        /// array or map.</exception>
        /// <exception cref='ArgumentException'>This object is an array and the
        /// index is less than 0 or at least the size of the array.</exception>
        /// <exception cref='ArgumentNullException'>The parameter "value" is
        /// null (as opposed to CBORObject.Null).</exception>
        public CBORObject this[int index]
        {
            get
            {
                if (this.Type == CBORType.Array)
                {
                    IList<CBORObject> list = this.AsList();
                    if (index < 0 || index >= list.Count)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }
                    return list[index];
                }
                if (this.Type == CBORType.Map)
                {
                    IDictionary<CBORObject, CBORObject> map = this.AsMap();
                    CBORObject key = CBORObject.FromObject(index);
                    return (!map.ContainsKey(key)) ? null : map[key];
                }
                throw new InvalidOperationException("Not an array or map");
            }

            set
            {
                if (this.Type == CBORType.Array)
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException(nameof(value));
                    }
                    IList<CBORObject> list = this.AsList();
                    if (index < 0 || index >= list.Count)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }
                    list[index] = value;
                }
                else if (this.Type == CBORType.Map)
                {
                    IDictionary<CBORObject, CBORObject> map = this.AsMap();
                    CBORObject key = CBORObject.FromObject(index);
                    map[key] = value;
                }
                else
                {
                    throw new InvalidOperationException("Not an array or map");
                }
            }
        }

        /// <summary>Gets the value of a CBOR object by integer index in this
        /// array or by CBOR object key in this map, or a default value if that
        /// value is not found.</summary>
        /// <param name='key'>An arbitrary object. If this is a CBOR map, this
        /// parameter is converted to a CBOR object serving as the key to the
        /// map or index to the array, and can be null. If this is a CBOR
        /// array, the key must be an integer 0 or greater and less than the
        /// size of the array, and may be any object convertible to a CBOR
        /// integer.</param>
        /// <param name='defaultValue'>A value to return if an item with the
        /// given key doesn't exist, or if the CBOR object is an array and the
        /// key is not an integer 0 or greater and less than the size of the
        /// array.</param>
        /// <returns>The CBOR object referred to by index or key in this array
        /// or map. If this is a CBOR map, returns <c>null</c> (not
        /// <c>CBORObject.Null</c> ) if an item with the given key doesn't
        /// exist.</returns>
        public CBORObject GetOrDefault(object key, CBORObject defaultValue)
        {
            if (this.Type == CBORType.Array)
            {
                var index = 0;
                if (key is int)
                {
                    index = (int)key;
                }
                else
                {
                    CBORObject cborkey = CBORObject.FromObject(key);
                    if (!cborkey.IsNumber || !cborkey.AsNumber().CanFitInInt32())
                    {
                        return defaultValue;
                    }
                    index = cborkey.AsNumber().ToInt32Checked();
                }
                IList<CBORObject> list = this.AsList();
                return (index < 0 || index >= list.Count) ? defaultValue :
                  list[index];
            }
            if (this.Type == CBORType.Map)
            {
                IDictionary<CBORObject, CBORObject> map = this.AsMap();
                CBORObject ckey = CBORObject.FromObject(key);
                return (!map.ContainsKey(ckey)) ? defaultValue : map[ckey];
            }
            return defaultValue;
        }

        /// <summary>Gets the value of a CBOR object by integer index in this
        /// array or by CBOR object key in this map.</summary>
        /// <param name='key'>A CBOR object serving as the key to the map or
        /// index to the array. If this is a CBOR array, the key must be an
        /// integer 0 or greater and less than the size of the array.</param>
        /// <returns>The CBOR object referred to by index or key in this array
        /// or map. If this is a CBOR map, returns <c>null</c> (not
        /// <c>CBORObject.Null</c> ) if an item with the given key doesn't
        /// exist.</returns>
        /// <exception cref='ArgumentNullException'>The key is null (as opposed
        /// to CBORObject.Null); or the set method is called and the value is
        /// null.</exception>
        /// <exception cref='ArgumentException'>This CBOR object is an array
        /// and the key is not an integer 0 or greater and less than the size
        /// of the array.</exception>
        /// <exception cref='InvalidOperationException'>This object is not a
        /// map or an array.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design",
            "CA1043",
            Justification = "Represents a logical data store")]
        public CBORObject this[CBORObject key]
        {
            get
            {
                /* "The CBORObject class represents a logical data store." +
                " Also, an Object indexer is not included here because it's unusual
                for " +
                "CBOR map keys to be anything other than text strings or integers; " +
                "including an Object indexer would introduce the security issues
                present in the FromObject method because of the need to convert to
                CBORObject;" +
                " and this CBORObject indexer is included here because any CBOR
                object " +
                "can serve as a map key, not just integers or text strings." */
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }
                if (this.Type == CBORType.Map)
                {
                    IDictionary<CBORObject, CBORObject> map = this.AsMap();
                    return (!map.ContainsKey(key)) ? null : map[key];
                }
                if (this.Type == CBORType.Array)
                {
                    if (!key.IsNumber || !key.AsNumber().IsInteger())
                    {
                        throw new ArgumentException("Not an integer");
                    }
                    if (!key.AsNumber().CanFitInInt32())
                    {
                        throw new ArgumentOutOfRangeException(nameof(key));
                    }
                    IList<CBORObject> list = this.AsList();
                    int index = key.AsNumber().ToInt32Checked();
                    if (index < 0 || index >= list.Count)
                    {
                        throw new ArgumentOutOfRangeException(nameof(key));
                    }
                    return list[index];
                }
                throw new InvalidOperationException("Not an array or map");
            }

            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                if (this.Type == CBORType.Map)
                {
                    IDictionary<CBORObject, CBORObject> map = this.AsMap();
                    map[key] = value;
                    return;
                }
                if (this.Type == CBORType.Array)
                {
                    if (!key.IsNumber || !key.AsNumber().IsInteger())
                    {
                        throw new ArgumentException("Not an integer");
                    }
                    if (!key.AsNumber().CanFitInInt32())
                    {
                        throw new ArgumentOutOfRangeException(nameof(key));
                    }
                    IList<CBORObject> list = this.AsList();
                    int index = key.AsNumber().ToInt32Checked();
                    if (index < 0 || index >= list.Count)
                    {
                        throw new ArgumentOutOfRangeException(nameof(key));
                    }
                    list[index] = value;
                    return;
                }
                throw new InvalidOperationException("Not an array or map");
            }
        }

        /// <summary>Gets the value of a CBOR object in this map, using a
        /// string as the key.</summary>
        /// <param name='key'>A key that points to the desired value.</param>
        /// <returns>The CBOR object referred to by key in this map. Returns
        /// <c>null</c> if an item with the given key doesn't exist.</returns>
        /// <exception cref='ArgumentNullException'>The key is
        /// null.</exception>
        /// <exception cref='InvalidOperationException'>This object is not a
        /// map.</exception>
        public CBORObject this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }
                CBORObject objkey = CBORObject.FromObject(key);
                return this[objkey];
            }

            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                CBORObject objkey = CBORObject.FromObject(key);
                if (this.Type == CBORType.Map)
                {
                    IDictionary<CBORObject, CBORObject> map = this.AsMap();
                    map[objkey] = value;
                }
                else
                {
                    throw new InvalidOperationException("Not a map");
                }
            }
        }

        /// <summary>Finds the sum of two CBOR numbers.</summary>
        /// <param name='first'>The parameter <paramref name='first'/> is a
        /// CBOR object.</param>
        /// <param name='second'>The parameter <paramref name='second'/> is a
        /// CBOR object.</param>
        /// <returns>A CBOR object.</returns>
        /// <exception cref='ArgumentException'>Either or both operands are not
        /// numbers (as opposed to Not-a-Number, NaN).</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='first'/> or <paramref name='second'/> is null.</exception>
        [Obsolete("Instead, convert both CBOR objects to numbers (with" +

            "\u0020.AsNumber()), and use the first number's .Add() method.")]
        public static CBORObject Addition(CBORObject first, CBORObject second)
        {
            if (first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }
            if (second == null)
            {
                throw new ArgumentNullException(nameof(second));
            }
            CBORNumber numberA = CBORNumber.FromCBORObject(first);
            if (numberA == null)
            {
                throw new ArgumentException(nameof(first) + "does not represent a" +
                  "\u0020number");
            }
            CBORNumber b = CBORNumber.FromCBORObject(second);
            if (b == null)
            {
                throw new ArgumentException(nameof(second) + "does not represent a" +
                  "\u0020number");
            }
            return numberA.Add(b).ToCBORObject();
        }

        /// <summary>
        /// <para>Generates a CBOR object from an array of CBOR-encoded
        /// bytes.</para></summary>
        /// <param name='data'>A byte array in which a single CBOR object is
        /// encoded.</param>
        /// <returns>A CBOR object decoded from the given byte array.</returns>
        /// <exception cref='PeterO.Cbor.CBORException'>There was an error in
        /// reading or parsing the data. This includes cases where not all of
        /// the byte array represents a CBOR object. This exception is also
        /// thrown if the parameter <paramref name='data'/> is
        /// empty.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='data'/> is null.</exception>
        public static CBORObject DecodeFromBytes(byte[] data)
        {
            return DecodeFromBytes(data, CBOREncodeOptions.Default);
        }

        private static readonly CBOREncodeOptions AllowEmptyOptions =
          new CBOREncodeOptions("allowempty=1");

        /// <summary>
        /// <para>Generates a sequence of CBOR objects from an array of
        /// CBOR-encoded bytes.</para></summary>
        /// <param name='data'>A byte array in which any number of CBOR objects
        /// (including zero) are encoded, one after the other. Can be empty,
        /// but cannot be null.</param>
        /// <returns>An array of CBOR objects decoded from the given byte
        /// array. Returns an empty array if <paramref name='data'/> is
        /// empty.</returns>
        /// <exception cref='PeterO.Cbor.CBORException'>There was an error in
        /// reading or parsing the data. This includes cases where the last
        /// CBOR object in the data was read only partly.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='data'/> is null.</exception>
        public static CBORObject[] DecodeSequenceFromBytes(byte[] data)
        {
            return DecodeSequenceFromBytes(data, AllowEmptyOptions);
        }

        /// <summary>
        /// <para>Generates a sequence of CBOR objects from an array of
        /// CBOR-encoded bytes.</para></summary>
        /// <param name='data'>A byte array in which any number of CBOR objects
        /// (including zero) are encoded, one after the other. Can be empty,
        /// but cannot be null.</param>
        /// <param name='options'>Specifies options to control how the CBOR
        /// object is decoded. See
        /// <see cref='PeterO.Cbor.CBOREncodeOptions'/> for more information.
        /// In this method, the AllowEmpty property is treated as always set
        /// regardless of that value as specified in this parameter.</param>
        /// <returns>An array of CBOR objects decoded from the given byte
        /// array. Returns an empty array if <paramref name='data'/> is
        /// empty.</returns>
        /// <exception cref='PeterO.Cbor.CBORException'>There was an error in
        /// reading or parsing the data. This includes cases where the last
        /// CBOR object in the data was read only partly.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='data'/> is null, or the parameter <paramref name='options'/>
        /// is null.</exception>
        public static CBORObject[] DecodeSequenceFromBytes(byte[] data,
          CBOREncodeOptions options)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (data.Length == 0)
            {
                return new CBORObject[0];
            }
            CBOREncodeOptions opt = options;
            if (!opt.AllowEmpty)
            {
                opt = new CBOREncodeOptions(opt.ToString() + ";allowempty=1");
            }
            var cborList = new List<CBORObject>();
            using (var ms = new MemoryStream(data))
            {
                while (true)
                {
                    CBORObject obj = Read(ms, opt);
                    if (obj == null)
                    {
                        break;
                    }
                    cborList.Add(obj);
                }
            }
            return (CBORObject[])cborList.ToArray();
        }

        /// <summary>Generates a list of CBOR objects from an array of bytes in
        /// JavaScript Object Notation (JSON) text sequence format (RFC 7464).
        /// The byte array must be in UTF-8 encoding and may not begin with a
        /// byte-order mark (U+FEFF).</summary>
        /// <param name='bytes'>A byte array in which a JSON text sequence is
        /// encoded.</param>
        /// <returns>A list of CBOR objects read from the JSON sequence.
        /// Objects that could not be parsed are replaced with <c>null</c> (as
        /// opposed to <c>CBORObject.Null</c> ) in the given list.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='bytes'/> is null.</exception>
        /// <exception cref='PeterO.Cbor.CBORException'>The byte array is not
        /// empty and does not begin with a record separator byte (0x1e), or an
        /// I/O error occurred.</exception>
        /// <remarks>Generally, each JSON text in a JSON text sequence is
        /// written as follows: Write a record separator byte (0x1e), then
        /// write the JSON text in UTF-8 (without a byte order mark, U+FEFF),
        /// then write the line feed byte (0x0a). RFC 7464, however, uses a
        /// more liberal syntax for parsing JSON text sequences.</remarks>
        public static CBORObject[] FromJSONSequenceBytes(byte[] bytes)
        {
            return FromJSONSequenceBytes(bytes, JSONOptions.Default);
        }

        /// <summary>Converts this object to a byte array in JavaScript Object
        /// Notation (JSON) format. The JSON text will be written out in UTF-8
        /// encoding, without a byte order mark, to the byte array. See the
        /// overload to ToJSONString taking a JSONOptions argument for further
        /// information.</summary>
        /// <returns>A byte array containing the converted in JSON
        /// format.</returns>
        public byte[] ToJSONBytes()
        {
            return this.ToJSONBytes(JSONOptions.Default);
        }

        /// <summary>Converts this object to a byte array in JavaScript Object
        /// Notation (JSON) format. The JSON text will be written out in UTF-8
        /// encoding, without a byte order mark, to the byte array. See the
        /// overload to ToJSONString taking a JSONOptions argument for further
        /// information.</summary>
        /// <param name='jsonoptions'>Specifies options to control writing the
        /// CBOR object to JSON.</param>
        /// <returns>A byte array containing the converted object in JSON
        /// format.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='jsonoptions'/> is null.</exception>
        public byte[] ToJSONBytes(JSONOptions jsonoptions)
        {
            if (jsonoptions == null)
            {
                throw new ArgumentNullException(nameof(jsonoptions));
            }
            try
            {
                using (var ms = new MemoryStream())
                {
                    this.WriteJSONTo(ms);
                    return ms.ToArray();
                }
            }
            catch (IOException ex)
            {
                throw new CBORException(ex.Message, ex);
            }
        }

        /// <summary>Generates a list of CBOR objects from an array of bytes in
        /// JavaScript Object Notation (JSON) text sequence format (RFC 7464),
        /// using the specified options to control the decoding process. The
        /// byte array must be in UTF-8 encoding and may not begin with a
        /// byte-order mark (U+FEFF).</summary>
        /// <param name='data'>A byte array in which a JSON text sequence is
        /// encoded.</param>
        /// <param name='options'>Specifies options to control the JSON
        /// decoding process.</param>
        /// <returns>A list of CBOR objects read from the JSON sequence.
        /// Objects that could not be parsed are replaced with <c>null</c> (as
        /// opposed to <c>CBORObject.Null</c> ) in the given list.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='data'/> is null.</exception>
        /// <exception cref='PeterO.Cbor.CBORException'>The byte array is not
        /// empty and does not begin with a record separator byte (0x1e), or an
        /// I/O error occurred.</exception>
        /// <remarks>Generally, each JSON text in a JSON text sequence is
        /// written as follows: Write a record separator byte (0x1e), then
        /// write the JSON text in UTF-8 (without a byte order mark, U+FEFF),
        /// then write the line feed byte (0x0a). RFC 7464, however, uses a
        /// more liberal syntax for parsing JSON text sequences.</remarks>
        public static CBORObject[] FromJSONSequenceBytes(byte[] data,
          JSONOptions options)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            try
            {
                using (var ms = new MemoryStream(data))
                {
                    return ReadJSONSequence(ms, options);
                }
            }
            catch (IOException ex)
            {
                throw new CBORException(ex.Message, ex);
            }
        }

        /// <summary>Generates a CBOR object from an array of CBOR-encoded
        /// bytes, using the given <c>CBOREncodeOptions</c>
        ///  object to control
        /// the decoding process.</summary>
        /// <param name='data'>A byte array in which a single CBOR object is
        /// encoded.</param>
        /// <param name='options'>Specifies options to control how the CBOR
        /// object is decoded. See <see cref='PeterO.Cbor.CBOREncodeOptions'/>
        /// for more information.</param>
        /// <returns>A CBOR object decoded from the given byte array. Returns
        /// null (as opposed to CBORObject.Null) if <paramref name='data'/> is
        /// empty and the AllowEmpty property is set on the given options
        /// object.</returns>
        /// <exception cref='PeterO.Cbor.CBORException'>There was an error in
        /// reading or parsing the data. This includes cases where not all of
        /// the byte array represents a CBOR object. This exception is also
        /// thrown if the parameter <paramref name='data'/> is empty unless the
        /// AllowEmpty property is set on the given options object.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='data'/> is null, or the parameter <paramref name='options'/>
        /// is null.</exception>
        /// <example>
        /// <para>The following example (originally written in C# for the.NET
        /// version) implements a method that decodes a text string from a CBOR
        /// byte array. It's successful only if the CBOR object contains an
        /// untagged text string.</para>
        /// <code>private static String DecodeTextString(byte[] bytes) { if (bytes ==
        /// null) { throw new ArgumentNullException(nameof(mapObj));}
        /// if
        /// (bytes.Length == 0 || bytes[0]&lt;0x60 || bytes[0]&gt;0x7f) {throw new
        /// CBORException();} return CBORObject.DecodeFromBytes(bytes,
        /// CBOREncodeOptions.Default).AsString(); }</code>
        ///  .
        /// </example>
        public static CBORObject DecodeFromBytes(
          byte[] data,
          CBOREncodeOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (data.Length == 0)
            {
                if (options.AllowEmpty)
                {
                    return null;
                }
                throw new CBORException("data is empty.");
            }
            var firstbyte = (int)(data[0] & (int)0xff);
            int expectedLength = ValueExpectedLengths[firstbyte];
            // if invalid
            if (expectedLength == -1)
            {
                throw new CBORException("Unexpected data encountered");
            }
            if (expectedLength != 0)
            {
                // if fixed length
                CheckCBORLength(expectedLength, data.Length);
                if (!options.Ctap2Canonical ||
                  (firstbyte >= 0x00 && firstbyte < 0x18) ||
                  (firstbyte >= 0x20 && firstbyte < 0x38))
                {
                    return GetFixedLengthObject(firstbyte, data);
                }
            }
            if (firstbyte == 0xc0 && !options.Ctap2Canonical)
            {
                // value with tag 0
                string s = GetOptimizedStringIfShortAscii(data, 1);
                if (s != null)
                {
                    return new CBORObject(FromObject(s), 0, 0);
                }
            }
            // For objects with variable length,
            // read the object as though
            // the byte array were a stream
            using (var ms = new MemoryStream(data))
            {
                CBORObject o = Read(ms, options);
                CheckCBORLength(
                  (long)data.Length,
                  (long)ms.Position);
                return o;
            }
        }

        /// <summary>Divides a CBORObject object by the value of a CBORObject
        /// object.</summary>
        /// <param name='first'>The parameter <paramref name='first'/> is a
        /// CBOR object.</param>
        /// <param name='second'>The parameter <paramref name='second'/> is a
        /// CBOR object.</param>
        /// <returns>The quotient of the two objects.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='first'/> or <paramref name='second'/> is null.</exception>
        [Obsolete("Instead, convert both CBOR objects to numbers (with" +

            "\u0020.AsNumber()), and use the first number's .Divide() method.")]
        public static CBORObject Divide(CBORObject first, CBORObject second)
        {
            if (first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }
            if (second == null)
            {
                throw new ArgumentNullException(nameof(second));
            }
            CBORNumber a = CBORNumber.FromCBORObject(first);
            if (a == null)
            {
                throw new ArgumentException(nameof(first) + "does not represent a" +
                  "\u0020number");
            }
            CBORNumber b = CBORNumber.FromCBORObject(second);
            if (b == null)
            {
                throw new ArgumentException(nameof(second) + "does not represent a" +
                  "\u0020number");
            }
            return a.Divide(b).ToCBORObject();
        }

        /// <summary>
        /// <para>Generates a CBOR object from a text string in JavaScript
        /// Object Notation (JSON) format.</para>
        /// <para>If a JSON object has duplicate keys, a CBORException is
        /// thrown. This is a change in version 4.0.</para>
        /// <para>Note that if a CBOR object is converted to JSON with
        /// <c>ToJSONString</c>, then the JSON is converted back to CBOR with
        /// this method, the new CBOR object will not necessarily be the same
        /// as the old CBOR object, especially if the old CBOR object uses data
        /// types not supported in JSON, such as integers in map
        /// keys.</para></summary>
        /// <param name='str'>A text string in JSON format. The entire string
        /// must contain a single JSON object and not multiple objects. The
        /// string may not begin with a byte-order mark (U+FEFF).</param>
        /// <param name='offset'>An index, starting at 0, showing where the
        /// desired portion of <paramref name='str'/> begins.</param>
        /// <param name='count'>The length, in code units, of the desired
        /// portion of <paramref name='str'/> (but not more than <paramref
        /// name='str'/> 's length).</param>
        /// <returns>A CBOR object.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='str'/> is null.</exception>
        /// <exception cref='PeterO.Cbor.CBORException'>The string is not in
        /// JSON format.</exception>
        /// <exception cref='ArgumentException'>Either <paramref
        /// name='offset'/> or <paramref name='count'/> is less than 0 or
        /// greater than <paramref name='str'/> 's length, or <paramref
        /// name='str'/> 's length minus <paramref name='offset'/> is less than
        /// <paramref name='count'/>.</exception>
        public static CBORObject FromJSONString(string str, int offset, int count)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            return FromJSONString(str, offset, count, JSONOptions.Default);
        }

        /// <summary>Generates a CBOR object from a text string in JavaScript
        /// Object Notation (JSON) format, using the specified options to
        /// control the decoding process.
        /// <para>Note that if a CBOR object is converted to JSON with
        /// <c>ToJSONString</c>, then the JSON is converted back to CBOR with
        /// this method, the new CBOR object will not necessarily be the same
        /// as the old CBOR object, especially if the old CBOR object uses data
        /// types not supported in JSON, such as integers in map
        /// keys.</para></summary>
        /// <param name='str'>A text string in JSON format. The entire string
        /// must contain a single JSON object and not multiple objects. The
        /// string may not begin with a byte-order mark (U+FEFF).</param>
        /// <param name='jsonoptions'>Specifies options to control the JSON
        /// decoding process.</param>
        /// <returns>A CBOR object containing the JSON data decoded.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='str'/> or <paramref name='jsonoptions'/> is null.</exception>
        /// <exception cref='PeterO.Cbor.CBORException'>The string is not in
        /// JSON format.</exception>
        public static CBORObject FromJSONString(
          string str,
          JSONOptions jsonoptions)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            if (jsonoptions == null)
            {
                throw new ArgumentNullException(nameof(jsonoptions));
            }
            return FromJSONString(str, 0, str.Length, jsonoptions);
        }

        /// <summary>
        /// <para>Generates a CBOR object from a text string in JavaScript
        /// Object Notation (JSON) format.</para>
        /// <para>If a JSON object has duplicate keys, a CBORException is
        /// thrown. This is a change in version 4.0.</para>
        /// <para>Note that if a CBOR object is converted to JSON with
        /// <c>ToJSONString</c>, then the JSON is converted back to CBOR with
        /// this method, the new CBOR object will not necessarily be the same
        /// as the old CBOR object, especially if the old CBOR object uses data
        /// types not supported in JSON, such as integers in map
        /// keys.</para></summary>
        /// <param name='str'>A text string in JSON format. The entire string
        /// must contain a single JSON object and not multiple objects. The
        /// string may not begin with a byte-order mark (U+FEFF).</param>
        /// <returns>A CBOR object.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='str'/> is null.</exception>
        /// <exception cref='PeterO.Cbor.CBORException'>The string is not in
        /// JSON format.</exception>
        public static CBORObject FromJSONString(string str)
        {
            return FromJSONString(str, JSONOptions.Default);
        }

        /// <summary>Generates a CBOR object from a text string in JavaScript
        /// Object Notation (JSON) format, using the specified options to
        /// control the decoding process.
        /// <para>Note that if a CBOR object is converted to JSON with
        /// <c>ToJSONString</c>, then the JSON is converted back to CBOR with
        /// this method, the new CBOR object will not necessarily be the same
        /// as the old CBOR object, especially if the old CBOR object uses data
        /// types not supported in JSON, such as integers in map
        /// keys.</para></summary>
        /// <param name='str'>A text string in JSON format. The entire string
        /// must contain a single JSON object and not multiple objects. The
        /// string may not begin with a byte-order mark (U+FEFF).</param>
        /// <param name='options'>Specifies options to control the decoding
        /// process. This method uses only the AllowDuplicateKeys property of
        /// this object.</param>
        /// <returns>A CBOR object containing the JSON data decoded.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='str'/> or <paramref name='options'/> is null.</exception>
        /// <exception cref='PeterO.Cbor.CBORException'>The string is not in
        /// JSON format.</exception>
        [Obsolete("Instead, use .FromJSONString\u0028str, new" +
            "\u0020JSONOptions\u0028\"allowduplicatekeys=true\")) or" +
            "\u0020.FromJSONString\u0028str," +
            "\u0020 new JSONOptions\u0028\"allowduplicatekeys=false\")), as" +
            "\u0020appropriate.")]
        public static CBORObject FromJSONString(
          string str,
          CBOREncodeOptions options)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            var jsonoptions = new JSONOptions(options.AllowDuplicateKeys ?
              "allowduplicatekeys=1" : "allowduplicatekeys=0");
            return FromJSONString(str, jsonoptions);
        }

        /// <summary>Generates a CBOR object from a text string in JavaScript
        /// Object Notation (JSON) format, using the specified options to
        /// control the decoding process.
        /// <para>Note that if a CBOR object is converted to JSON with
        /// <c>ToJSONString</c>, then the JSON is converted back to CBOR with
        /// this method, the new CBOR object will not necessarily be the same
        /// as the old CBOR object, especially if the old CBOR object uses data
        /// types not supported in JSON, such as integers in map
        /// keys.</para></summary>
        /// <param name='str'>The parameter <paramref name='str'/> is a text
        /// string.</param>
        /// <param name='offset'>An index, starting at 0, showing where the
        /// desired portion of <paramref name='str'/> begins.</param>
        /// <param name='count'>The length, in code units, of the desired
        /// portion of <paramref name='str'/> (but not more than <paramref
        /// name='str'/> 's length).</param>
        /// <param name='jsonoptions'>The parameter <paramref
        /// name='jsonoptions'/> is a Cbor.JSONOptions object.</param>
        /// <returns>A CBOR object containing the JSON data decoded.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='str'/> or <paramref name='jsonoptions'/> is null.</exception>
        /// <exception cref='PeterO.Cbor.CBORException'>The string is not in
        /// JSON format.</exception>
        /// <exception cref='ArgumentException'>Either <paramref
        /// name='offset'/> or <paramref name='count'/> is less than 0 or
        /// greater than <paramref name='str'/> 's length, or <paramref
        /// name='str'/> 's length minus <paramref name='offset'/> is less than
        /// <paramref name='count'/>.</exception>
        public static CBORObject FromJSONString(
          string str,
          int offset,
          int count,
          JSONOptions jsonoptions)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            if (jsonoptions == null)
            {
                throw new ArgumentNullException(nameof(jsonoptions));
            }
            if (count > 0 && str[offset] == 0xfeff)
            {
                throw new CBORException(
                  "JSON object began with a byte order mark (U+FEFF) (offset 0)");
            }
            if (count == 0)
            {
                throw new CBORException("String is empty");
            }
            return CBORJson3.ParseJSONValue(str, offset, offset + count, jsonoptions);
        }

        /// <summary>Converts this CBOR object to an object of an arbitrary
        /// type. See the documentation for the overload of this method taking
        /// a CBORTypeMapper parameter for more information. This method
        /// doesn't use a CBORTypeMapper parameter to restrict which data types
        /// are eligible for Plain-Old-Data serialization.</summary>
        /// <param name='t'>The type, class, or interface that this method's
        /// return value will belong to. To express a generic type in Java, see
        /// the example. <b>Note:</b>
        ///  For security reasons, an application
        /// should not base this parameter on user input or other externally
        /// supplied data. Whenever possible, this parameter should be either a
        /// type specially handled by this method (such as <c>int</c>
        ///  or
        /// <c>String</c>
        ///  ) or a plain-old-data type (POCO or POJO type) within
        /// the control of the application. If the plain-old-data type
        /// references other data types, those types should likewise meet
        /// either criterion above.</param>
        /// <returns>The converted object.</returns>
        /// <exception cref='PeterO.Cbor.CBORException'>The given type
        /// <paramref name='t'/> , or this object's CBOR type, is not
        /// supported, or the given object's nesting is too deep, or another
        /// error occurred when serializing the object.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='t'/> is null.</exception>
        /// <example>
        /// <para>Java offers no easy way to express a generic type, at least
        /// none as easy as C#'s <c>typeof</c>
        ///  operator. The following example,
        /// written in Java, is a way to specify that the return value will be
        /// an ArrayList of String objects.</para>
        /// <code>Type arrayListString = new ParameterizedType() { public Type[]
        /// getActualTypeArguments() { &#x2f;&#x2a; Contains one type parameter,
        /// String&#x2a;&#x2f;
        /// return new Type[] { String.class }; }
        /// public Type getRawType() { /* Raw type is
        /// ArrayList */ return ArrayList.class; }
        /// public Type getOwnerType() {
        /// return null; } };
        /// ArrayList&lt;String&gt; array = (ArrayList&lt;String&gt;)
        /// cborArray.ToObject(arrayListString);</code>
        /// <para>By comparison, the C# version is much shorter.</para>
        /// <code>var array = (List&lt;String&gt;)cborArray.ToObject(
        /// typeof(List&lt;String&gt;));</code>
        ///  .
        /// </example>
        public object ToObject(Type t)
        {
            return this.ToObject(t, null, null, 0);
        }

        /// <summary>Converts this CBOR object to an object of an arbitrary
        /// type. See the documentation for the overload of this method taking
        /// a CBORTypeMapper and PODOptions parameters parameters for more
        /// information.</summary>
        /// <param name='t'>The type, class, or interface that this method's
        /// return value will belong to. To express a generic type in Java, see
        /// the example. <b>Note:</b> For security reasons, an application
        /// should not base this parameter on user input or other externally
        /// supplied data. Whenever possible, this parameter should be either a
        /// type specially handled by this method (such as <c>int</c> or
        /// <c>String</c> ) or a plain-old-data type (POCO or POJO type) within
        /// the control of the application. If the plain-old-data type
        /// references other data types, those types should likewise meet
        /// either criterion above.</param>
        /// <param name='mapper'>This parameter controls which data types are
        /// eligible for Plain-Old-Data deserialization and includes custom
        /// converters from CBOR objects to certain data types.</param>
        /// <returns>The converted object.</returns>
        /// <exception cref='PeterO.Cbor.CBORException'>The given type
        /// <paramref name='t'/>, or this object's CBOR type, is not
        /// supported, or the given object's nesting is too deep, or another
        /// error occurred when serializing the object.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='t'/> is null.</exception>
        public object ToObject(Type t, CBORTypeMapper mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }
            return this.ToObject(t, mapper, null, 0);
        }

        /// <summary>Converts this CBOR object to an object of an arbitrary
        /// type. See the documentation for the overload of this method taking
        /// a CBORTypeMapper and PODOptions parameters for more information.
        /// This method (without a CBORTypeMapper parameter) allows all data
        /// types not otherwise handled to be eligible for Plain-Old-Data
        /// serialization.</summary>
        /// <param name='t'>The type, class, or interface that this method's
        /// return value will belong to. To express a generic type in Java, see
        /// the example. <b>Note:</b> For security reasons, an application
        /// should not base this parameter on user input or other externally
        /// supplied data. Whenever possible, this parameter should be either a
        /// type specially handled by this method (such as <c>int</c> or
        /// <c>String</c> ) or a plain-old-data type (POCO or POJO type) within
        /// the control of the application. If the plain-old-data type
        /// references other data types, those types should likewise meet
        /// either criterion above.</param>
        /// <param name='options'>Specifies options for controlling
        /// deserialization of CBOR objects.</param>
        /// <returns>The converted object.</returns>
        /// <exception cref='NotSupportedException'>The given type <paramref
        /// name='t'/>, or this object's CBOR type, is not
        /// supported.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='t'/> is null.</exception>
        /// <exception cref='PeterO.Cbor.CBORException'>The given object's
        /// nesting is too deep, or another error occurred when serializing the
        /// object.</exception>
        public object ToObject(Type t, PODOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            return this.ToObject(t, null, options, 0);
        }

        /// <summary><para>Converts this CBOR object to an object of an
        /// arbitrary type. The following cases are checked in the logical
        /// order given (rather than the strict order in which they are
        /// implemented by this library):</para>
        ///  <list><item>If the type is
        /// <c>CBORObject</c>
        ///  , return this object.</item>
        ///  <item>If the given
        /// object is <c>CBORObject.Null</c>
        ///  (with or without tags), returns
        /// <c>null</c>
        ///  .</item>
        ///  <item>If the object is of a type corresponding
        /// to a type converter mentioned in the <paramref name='mapper'/>
        /// parameter, that converter will be used to convert the CBOR object
        /// to an object of the given type. Type converters can be used to
        /// override the default conversion behavior of almost any
        /// object.</item>
        ///  <item>If the type is <c>object</c>
        ///  , return this
        /// object.</item>
        ///  <item>If the type is <c>char</c>
        ///  , converts
        /// single-character CBOR text strings and CBOR integers from 0 through
        /// 65535 to a <c>char</c>
        ///  object and returns that <c>char</c>
        /// object.</item>
        ///  <item>If the type is <c>bool</c>
        ///  ( <c>boolean</c>
        ///  in
        /// Java), returns the result of AsBoolean.</item>
        ///  <item>If the type is
        /// <c>short</c>
        ///  , returns this number as a 16-bit signed integer after
        /// converting its value to an integer by discarding its fractional
        /// part, and throws an exception if this object's value is infinity or
        /// a not-a-number value, or does not represent a number (currently
        /// InvalidOperationException, but may change in the next major
        /// version), or if the value, once converted to an integer by
        /// discarding its fractional part, is less than -32768 or greater than
        /// 32767 (currently OverflowException, but may change in the next
        /// major version).</item>
        ///  <item>If the type is <c>long</c>
        ///  , returns
        /// this number as a 64-bit signed integer after converting its value
        /// to an integer by discarding its fractional part, and throws an
        /// exception if this object's value is infinity or a not-a-number
        /// value, or does not represent a number (currently
        /// InvalidOperationException, but may change in the next major
        /// version), or if the value, once converted to an integer by
        /// discarding its fractional part, is less than -2^63 or greater than
        /// 2^63-1 (currently OverflowException, but may change in the next
        /// major version).</item>
        ///  <item>If the type is <c>short</c>
        ///  , the same
        /// rules as for <c>long</c>
        ///  are used, but the range is from -32768
        /// through 32767 and the return type is <c>short</c>
        ///  .</item>
        ///  <item>If
        /// the type is <c>byte</c>
        ///  , the same rules as for <c>long</c>
        ///  are
        /// used, but the range is from 0 through 255 and the return type is
        /// <c>byte</c>
        ///  .</item>
        ///  <item>If the type is <c>sbyte</c>
        ///  , the same
        /// rules as for <c>long</c>
        ///  are used, but the range is from -128
        /// through 127 and the return type is <c>sbyte</c>
        ///  .</item>
        ///  <item>If
        /// the type is <c>ushort</c>
        ///  , the same rules as for <c>long</c>
        ///  are
        /// used, but the range is from 0 through 65535 and the return type is
        /// <c>ushort</c>
        ///  .</item>
        ///  <item>If the type is <c>uint</c>
        ///  , the same
        /// rules as for <c>long</c>
        ///  are used, but the range is from 0 through
        /// 2^31-1 and the return type is <c>uint</c>
        ///  .</item>
        ///  <item>If the
        /// type is <c>ulong</c>
        ///  , the same rules as for <c>long</c>
        ///  are used,
        /// but the range is from 0 through 2^63-1 and the return type is
        /// <c>ulong</c>
        ///  .</item>
        ///  <item>If the type is <c>int</c>
        ///  or a
        /// primitive floating-point type ( <c>float</c>
        ///  , <c>double</c>
        ///  , as
        /// well as <c>decimal</c>
        ///  in.NET), returns the result of the
        /// corresponding As* method.</item>
        ///  <item>If the type is <c>String</c>
        /// , returns the result of AsString.</item>
        ///  <item>If the type is
        /// <c>EFloat</c>
        ///  , <c>EDecimal</c>
        ///  , <c>EInteger</c>
        ///  , or
        /// <c>ERational</c>
        ///  in the <a
        /// href='https://www.nuget.org/packages/PeterO.Numbers'><c>PeterO.Numbers</c>
        /// </a>
        ///  library (in .NET) or the <a
        /// href='https://github.com/peteroupc/numbers-java'><c>com.github.peteroupc/numbers</c>
        /// </a>
        ///  artifact (in Java), converts the given object to a number of
        /// the corresponding type and throws an exception (currently
        /// InvalidOperationException) if the object does not represent a
        /// number (for this purpose, infinity and not-a-number values, but not
        /// <c>CBORObject.Null</c>
        ///  , are considered numbers). Currently, this
        /// is equivalent to the result of <c>AsEFloat()</c>
        ///  ,
        /// <c>AsEDecimal()</c>
        ///  , <c>AsEInteger</c>
        ///  , or <c>AsERational()</c>
        ///  ,
        /// respectively, but may change slightly in the next major version.
        /// Note that in the case of <c>EFloat</c>
        ///  , if this object represents
        /// a decimal number with a fractional part, the conversion may lose
        /// information depending on the number, and if the object is a
        /// rational number with a nonterminating binary expansion, the number
        /// returned is a binary floating-point number rounded to a high but
        /// limited precision. In the case of <c>EDecimal</c>
        ///  , if this object
        /// expresses a rational number with a nonterminating decimal
        /// expansion, returns a decimal number rounded to 34 digits of
        /// precision. In the case of <c>EInteger</c>
        ///  , if this CBOR object
        /// expresses a floating-point number, it is converted to an integer by
        /// discarding its fractional part, and if this CBOR object expresses a
        /// rational number, it is converted to an integer by dividing the
        /// numerator by the denominator and discarding the fractional part of
        /// the result, and this method throws an exception (currently
        /// OverflowException, but may change in the next major version) if
        /// this object expresses infinity or a not-a-number value.</item>
        /// <item>In the.NET version, if the type is a nullable (e.g.,
        /// <c>Nullable&lt;int&gt;</c>
        ///  or <c>int?</c>
        ///  , returns <c>null</c>
        ///  if
        /// this CBOR object is null, or this object's value converted to the
        /// nullable's underlying type, e.g., <c>int</c>
        ///  .</item>
        ///  <item>If the
        /// type is an enumeration ( <c>Enum</c>
        ///  ) type and this CBOR object is
        /// a text string or an integer, returns the appropriate enumerated
        /// constant. (For example, if <c>MyEnum</c>
        ///  includes an entry for
        /// <c>MyValue</c>
        ///  , this method will return <c>MyEnum.MyValue</c>
        ///  if
        /// the CBOR object represents <c>"MyValue"</c>
        ///  or the underlying value
        /// for <c>MyEnum.MyValue</c>
        ///  .) <b>Note:</b>
        ///  If an integer is
        /// converted to a.NET Enum constant, and that integer is shared by
        /// more than one constant of the same type, it is undefined which
        /// constant from among them is returned. (For example, if
        /// <c>MyEnum.Zero=0</c>
        ///  and <c>MyEnum.Null=0</c>
        ///  , converting 0 to
        /// <c>MyEnum</c>
        ///  may return either <c>MyEnum.Zero</c>
        ///  or
        /// <c>MyEnum.Null</c>
        ///  .) As a result, .NET Enum types with constants
        /// that share an underlying value should not be passed to this
        /// method.</item>
        ///  <item>If the type is <c>byte[]</c>
        ///  (a
        /// one-dimensional byte array) and this CBOR object is a byte string,
        /// returns a byte array which this CBOR byte string's data will be
        /// copied to. (This method can't be used to encode CBOR data to a byte
        /// array; for that, use the EncodeToBytes method instead.)</item>
        /// <item>If the type is a one-dimensional or multidimensional array
        /// type and this CBOR object is an array, returns an array containing
        /// the items in this CBOR object.</item>
        ///  <item>If the type is List or
        /// the generic or non-generic IList, ICollection, or IEnumerable, (or
        /// ArrayList, List, Collection, or Iterable in Java), and if this CBOR
        /// object is an array, returns an object conforming to the type,
        /// class, or interface passed to this method, where the object will
        /// contain all items in this CBOR array.</item>
        ///  <item>If the type is
        /// Dictionary or the generic or non-generic IDictionary (or HashMap or
        /// Map in Java), and if this CBOR object is a map, returns an object
        /// conforming to the type, class, or interface passed to this method,
        /// where the object will contain all keys and values in this CBOR
        /// map.</item>
        ///  <item>If the type is an enumeration constant ("enum"),
        /// and this CBOR object is an integer or text string, returns the
        /// enumeration constant with the given number or name, respectively.
        /// (Enumeration constants made up of multiple enumeration constants,
        /// as allowed by .NET, can only be matched by number this way.)</item>
        /// <item>If the type is <c>DateTime</c>
        ///  (or <c>Date</c>
        ///  in Java) ,
        /// returns a date/time object if the CBOR object's outermost tag is 0
        /// or 1. For tag 1, this method treats the CBOR object as a number of
        /// seconds since the start of 1970, which is based on the POSIX
        /// definition of "seconds since the Epoch", a definition that does not
        /// count leap seconds. In this method, this number of seconds assumes
        /// the use of a proleptic Gregorian calendar, in which the rules
        /// regarding the number of days in each month and which years are leap
        /// years are the same for all years as they were in 1970 (including
        /// without regard to transitions from other calendars to the
        /// Gregorian). For tag 1, CBOR objects that express infinity or
        /// not-a-number (NaN) are treated as invalid by this method.</item>
        /// <item>If the type is <c>Uri</c>
        ///  (or <c>URI</c>
        ///  in Java), returns a
        /// URI object if possible.</item>
        ///  <item>If the type is <c>Guid</c>
        ///  (or
        /// <c>UUID</c>
        ///  in Java), returns a UUID object if possible.</item>
        /// <item>Plain-Old-Data deserialization: If the object is a type not
        /// specially handled above, the type includes a zero-parameter
        /// constructor (default or not), this CBOR object is a CBOR map, and
        /// the "mapper" parameter (if any) allows this type to be eligible for
        /// Plain-Old-Data deserialization, then this method checks the given
        /// type for eligible setters as follows:</item>
        ///  <item>(*) In the .NET
        /// version, eligible setters are the public, nonstatic setters of
        /// properties with a public, nonstatic getter. Eligible setters also
        /// include public, nonstatic, non- <c>const</c>
        ///  , non- <c>readonly</c>
        /// fields. If a class has two properties and/or fields of the form "X"
        /// and "IsX", where "X" is any name, or has multiple properties and/or
        /// fields with the same name, those properties and fields are
        /// ignored.</item>
        ///  <item>(*) In the Java version, eligible setters are
        /// public, nonstatic methods starting with "set" followed by a
        /// character other than a basic digit or lower-case letter, that is,
        /// other than "a" to "z" or "0" to "9", that take one parameter. The
        /// class containing an eligible setter must have a public, nonstatic
        /// method with the same name, but starting with "get" or "is" rather
        /// than "set", that takes no parameters and does not return void. (For
        /// example, if a class has "public setValue(String)" and "public
        /// getValue()", "setValue" is an eligible setter. However,
        /// "setValue()" and "setValue(String, int)" are not eligible setters.)
        /// In addition, public, nonstatic, nonfinal fields are also eligible
        /// setters. If a class has two or more otherwise eligible setters
        /// (methods and/or fields) with the same name, but different parameter
        /// type, they are not eligible setters.</item>
        ///  <item>Then, the method
        /// creates an object of the given type and invokes each eligible
        /// setter with the corresponding value in the CBOR map, if any. Key
        /// names in the map are matched to eligible setters according to the
        /// rules described in the <see cref='PeterO.Cbor.PODOptions'/>
        /// documentation. Note that for security reasons, certain types are
        /// not supported even if they contain eligible setters. For the Java
        /// version, the object creation may fail in the case of a nested
        /// nonstatic class.</item>
        ///  </list>
        ///  </summary>
        /// <param name='t'>The type, class, or interface that this method's
        /// return value will belong to. To express a generic type in Java, see
        /// the example. <b>Note:</b>
        ///  For security reasons, an application
        /// should not base this parameter on user input or other externally
        /// supplied data. Whenever possible, this parameter should be either a
        /// type specially handled by this method, such as <c>int</c>
        ///  or
        /// <c>String</c>
        ///  , or a plain-old-data type (POCO or POJO type) within
        /// the control of the application. If the plain-old-data type
        /// references other data types, those types should likewise meet
        /// either criterion above.</param>
        /// <param name='mapper'>This parameter controls which data types are
        /// eligible for Plain-Old-Data deserialization and includes custom
        /// converters from CBOR objects to certain data types. Can be
        /// null.</param>
        /// <param name='options'>Specifies options for controlling
        /// deserialization of CBOR objects.</param>
        /// <returns>The converted object.</returns>
        /// <exception cref='PeterO.Cbor.CBORException'>The given type
        /// <paramref name='t'/> , or this object's CBOR type, is not
        /// supported, or the given object's nesting is too deep, or another
        /// error occurred when serializing the object.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='t'/> or <paramref name='options'/> is null.</exception>
        /// <example>
        /// <para>Java offers no easy way to express a generic type, at least
        /// none as easy as C#'s <c>typeof</c>
        ///  operator. The following example,
        /// written in Java, is a way to specify that the return value will be
        /// an ArrayList of String objects.</para>
        /// <code>Type arrayListString = new ParameterizedType() { public Type[]
        /// getActualTypeArguments() { &#x2f;&#x2a; Contains one type parameter,
        /// String&#x2a;&#x2f;
        /// return new Type[] { String.class }; }
        /// public Type getRawType() { /* Raw type is
        /// ArrayList */ return ArrayList.class; } public Type getOwnerType() {
        /// return null; } }; ArrayList&lt;String&gt; array =
        /// (ArrayList&lt;String&gt;) cborArray.ToObject(arrayListString);</code>
        /// <para>By comparison, the C# version is much shorter.</para>
        /// <code>var array = (List&lt;String&gt;)cborArray.ToObject(
        /// typeof(List&lt;String&gt;));</code>
        ///  .
        /// </example>
        public object ToObject(Type t, CBORTypeMapper mapper, PODOptions
          options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            return this.ToObject(t, mapper, options, 0);
        }

        internal object ToObject(
          Type t,
          CBORTypeMapper mapper,
          PODOptions options,
          int depth)
        {
            ++depth;
            if (depth > 100)
            {
                throw new CBORException("Depth level too high");
            }
            if (t == null)
            {
                throw new ArgumentNullException(nameof(t));
            }
            if (t.Equals(typeof(CBORObject)))
            {
                return this;
            }
            if (this.IsNull)
            {
                return null;
            }
            if (mapper != null)
            {
                object obj = mapper.ConvertBackWithConverter(this, t);
                if (obj != null)
                {
                    return obj;
                }
            }
            if (t.Equals(typeof(object)))
            {
                return this;
            }
            // TODO: In next major version, address inconsistent
            // implementations for EDecimal, EInteger, EFloat,
            // and ERational (perhaps
            // by using EDecimal implementation). Also, these operations
            // might throw InvalidOperationException rather than CBORException.
            // Make them throw CBORException in next major version.
            if (t.Equals(typeof(EDecimal)))
            {
                CBORNumber cn = this.AsNumber();
                return cn.GetNumberInterface().AsEDecimal(cn.GetValue());
            }
            if (t.Equals(typeof(EFloat)))
            {
                CBORNumber cn = CBORNumber.FromCBORObject(this);
                if (cn == null)
                {
                    throw new InvalidOperationException("Not a number type");
                }
                return cn.GetNumberInterface().AsEFloat(cn.GetValue());
            }
            if (t.Equals(typeof(EInteger)))
            {
                CBORNumber cn = CBORNumber.FromCBORObject(this);
                if (cn == null)
                {
                    throw new InvalidOperationException("Not a number type");
                }
                return cn.GetNumberInterface().AsEInteger(cn.GetValue());
            }
            if (t.Equals(typeof(ERational)))
            {
                // NOTE: Will likely be simplified in version 5.0 and later
                if (this.HasMostInnerTag(30) && this.Count != 2)
                {
                    EInteger num, den;
                    num = (EInteger)this[0].ToObject(typeof(EInteger));
                    den = (EInteger)this[1].ToObject(typeof(EInteger));
                    return ERational.Create(num, den);
                }
                CBORNumber cn = CBORNumber.FromCBORObject(this);
                if (cn == null)
                {
                    throw new InvalidOperationException("Not a number type");
                }
                return cn.GetNumberInterface().AsERational(cn.GetValue());
            }
            return t.Equals(typeof(string)) ? this.AsString() :
              PropertyMap.TypeToObject(this, t, mapper, options, depth);
        }

        /// <summary>Generates a CBOR object from a 64-bit signed
        /// integer.</summary>
        /// <param name='value'>The parameter <paramref name='value'/> is a
        /// 64-bit signed integer.</param>
        /// <returns>A CBOR object.</returns>
        public static CBORObject FromObject(long value)
        {
            if (value >= 0L && value < 24L)
            {
                return FixedObjects[(int)value];
            }
            else
            {
                return (value >= -24L && value < 0L) ? FixedObjects[0x20 - (int)(value +
                      1L)] : new CBORObject(CBORObjectTypeInteger, value);
            }
        }

        /// <summary>Generates a CBOR object from a CBOR object.</summary>
        /// <param name='value'>The parameter <paramref name='value'/> is a
        /// CBOR object.</param>
        /// <returns>Same as <paramref name='value'/>, or "CBORObject.Null" is
        /// <paramref name='value'/> is null.</returns>
        public static CBORObject FromObject(CBORObject value)
        {
            return value ?? CBORObject.Null;
        }

        private static int IntegerByteLength(int intValue)
        {
            if (intValue < 0)
            {
                intValue = -(intValue + 1);
            }
            if (intValue > 0xffff)
            {
                return 5;
            }
            else if (intValue > 0xff)
            {
                return 3;
            }
            else
            {
                return (intValue > 23) ? 2 : 1;
            }
        }

        private static int IntegerByteLength(long longValue)
        {
            if (longValue < 0)
            {
                longValue = -(longValue + 1);
            }
            if (longValue > 0xffffffffL)
            {
                return 9;
            }
            else if (longValue > 0xffffL)
            {
                return 5;
            }
            else if (longValue > 0xffL)
            {
                return 3;
            }
            else
            {
                return (longValue > 23L) ? 2 : 1;
            }
        }

        /// <summary>Calculates the number of bytes this CBOR object takes when
        /// serialized as a byte array using the <c>EncodeToBytes()</c> method.
        /// This calculation assumes that integers, lengths of maps and arrays,
        /// lengths of text and byte strings, and tag numbers are encoded in
        /// their shortest form; that floating-point numbers are encoded in
        /// their shortest value-preserving form; and that no indefinite-length
        /// encodings are used.</summary>
        /// <returns>The number of bytes this CBOR object takes when serialized
        /// as a byte array using the <c>EncodeToBytes()</c> method.</returns>
        /// <exception cref='PeterO.Cbor.CBORException'>The CBOR object has an
        /// extremely deep level of nesting, including if the CBOR object is or
        /// has an array or map that includes itself.</exception>
        public long CalcEncodedSize()
        {
            return this.CalcEncodedSize(0);
        }

        private long CalcEncodedSize(int depth)
        {
            if (depth > 1000)
            {
                throw new CBORException("Too deeply nested");
            }
            long size = 0L;
            CBORObject cbor = this;
            while (cbor.IsTagged)
            {
                EInteger etag = cbor.MostOuterTag;
                if (etag.CanFitInInt64())
                {
                    long tag = etag.ToInt64Checked();
                    size = checked(size + IntegerByteLength(tag));
                }
                else
                {
                    size = checked(size + 9);
                }
                cbor = cbor.UntagOne();
            }
            if (cbor.ItemType == CBORObjectTypeTextStringUtf8)
            {
                byte[] bytes = (byte[])this.ThisItem;
                size = checked(size + IntegerByteLength(bytes.Length));
                return checked(size + bytes.Length);
            }
            switch (cbor.Type)
            {
                case CBORType.Integer:
                    {
                        if (cbor.CanValueFitInInt64())
                        {
                            long tag = cbor.AsInt64Value();
                            size = checked(size + IntegerByteLength(tag));
                            return size;
                        }
                        else
                        {
                            return checked(size + 9);
                        }
                    }
                case CBORType.FloatingPoint:
                    {
                        long valueBits = cbor.AsDoubleBits();
                        int bits = CBORUtilities.DoubleToHalfPrecisionIfSameValue(valueBits);
                        if (bits != -1)
                        {
                            return checked(size + 3);
                        }
                        return CBORUtilities.DoubleRetainsSameValueInSingle(valueBits) ?
                          checked(size + 5) : checked(size + 9);
                    }
                case CBORType.Array:
                    size = checked(size + IntegerByteLength(cbor.Count));
                    for (var i = 0; i < cbor.Count; ++i)
                    {
                        long newsize = cbor[i].CalcEncodedSize(depth + 1);
                        size = checked(size + newsize);
                    }
                    return size;
                case CBORType.Map:
                    {
                        ICollection<KeyValuePair<CBORObject, CBORObject>> entries =
                          this.Entries;
                        size = checked(size + IntegerByteLength(entries.Count));
                        foreach (KeyValuePair<CBORObject, CBORObject> entry in entries)
                        {
                            CBORObject key = entry.Key;
                            CBORObject value = entry.Value;
                            size = checked(size + key.CalcEncodedSize(depth + 1));
                            size = checked(size + value.CalcEncodedSize(depth + 1));
                        }
                        return size;
                    }
                case CBORType.TextString:
                    {
                        long ulength = DataUtilities.GetUtf8Length(this.AsString(), false);
                        size = checked(size + IntegerByteLength(ulength));
                        return checked(size + ulength);
                    }
                case CBORType.ByteString:
                    {
                        byte[] bytes = cbor.GetByteString();
                        size = checked(size + IntegerByteLength(bytes.Length));
                        return checked(size + bytes.Length);
                    }
                case CBORType.Boolean:
                    return checked(size + 1);
                case CBORType.SimpleValue:
                    return checked(size + (cbor.SimpleValue >= 24 ? 2 : 1));
                default: throw new InvalidOperationException();
            }
        }

        /// <summary>Generates a CBOR object from an arbitrary-precision
        /// integer. The CBOR object is generated as follows:
        /// <list>
        /// <item>If the number is null, returns CBORObject.Null.</item>
        /// <item>Otherwise, if the number is greater than or equal to -(2^64)
        /// and less than 2^64, the CBOR object will have the object type
        /// Integer and the appropriate value.</item>
        /// <item>Otherwise, the CBOR object will have tag 2 (zero or positive)
        /// or 3 (negative) and the appropriate value.</item></list></summary>
        /// <param name='bigintValue'>An arbitrary-precision integer. Can be
        /// null.</param>
        /// <returns>The given number encoded as a CBOR object. Returns
        /// CBORObject.Null if <paramref name='bigintValue'/> is
        /// null.</returns>
        public static CBORObject FromObject(EInteger bigintValue)
        {
            if ((object)bigintValue == (object)null)
            {
                return CBORObject.Null;
            }
            if (bigintValue.CanFitInInt64())
            {
                return CBORObject.FromObject(bigintValue.ToInt64Checked());
            }
            else
            {
                EInteger bitLength = bigintValue.GetSignedBitLengthAsEInteger();
                if (bitLength.CompareTo(64) <= 0)
                {
                    // Fits in major type 0 or 1
                    return new CBORObject(CBORObjectTypeEInteger, bigintValue);
                }
                else
                {
                    int tag = (bigintValue.Sign < 0) ? 3 : 2;
                    return CBORObject.FromObjectAndTag(
                        EIntegerBytes(bigintValue),
                        tag);
                }
            }
        }

        /// <summary>Generates a CBOR object from an arbitrary-precision binary
        /// floating-point number. The CBOR object is generated as follows
        /// (this is a change in version 4.0):
        /// <list>
        /// <item>If the number is null, returns CBORObject.Null.</item>
        /// <item>Otherwise, if the number expresses infinity, not-a-number, or
        /// negative zero, the CBOR object will have tag 269 and the
        /// appropriate format.</item>
        /// <item>Otherwise, if the number's exponent is at least 2^64 or less
        /// than -(2^64), the CBOR object will have tag 265 and the appropriate
        /// format.</item>
        /// <item>Otherwise, the CBOR object will have tag 5 and the
        /// appropriate format.</item></list></summary>
        /// <param name='bigValue'>An arbitrary-precision binary floating-point
        /// number. Can be null.</param>
        /// <returns>The given number encoded as a CBOR object. Returns
        /// CBORObject.Null if <paramref name='bigValue'/> is null.</returns>
        public static CBORObject FromObject(EFloat bigValue)
        {
            if ((object)bigValue == (object)null)
            {
                return CBORObject.Null;
            }
            CBORObject cbor;
            int tag;
            if (bigValue.IsInfinity() || bigValue.IsNaN() ||
              (bigValue.IsNegative && bigValue.IsZero))
            {
                int options = bigValue.IsNegative ? 1 : 0;
                if (bigValue.IsInfinity())
                {
                    options += 2;
                }
                if (bigValue.IsQuietNaN())
                {
                    options += 4;
                }
                if (bigValue.IsSignalingNaN())
                {
                    options += 6;
                }
                cbor = CBORObject.NewArray(
                    CBORObject.FromObject(bigValue.Exponent),
                    CBORObject.FromObject(bigValue.UnsignedMantissa),
                    CBORObject.FromObject(options));
                tag = 269;
            }
            else
            {
                EInteger exponent = bigValue.Exponent;
                if (exponent.CanFitInInt64())
                {
                    tag = 5;
                    cbor = CBORObject.NewArray(
                        CBORObject.FromObject(exponent.ToInt64Checked()),
                        CBORObject.FromObject(bigValue.Mantissa));
                }
                else
                {
                    tag = (exponent.GetSignedBitLengthAsInt64() > 64) ?
                      265 : 5;
                    cbor = CBORObject.NewArray(
                        CBORObject.FromObject(exponent),
                        CBORObject.FromObject(bigValue.Mantissa));
                }
            }
            return cbor.WithTag(tag);
        }

        /// <summary>Generates a CBOR object from an arbitrary-precision
        /// rational number. The CBOR object is generated as follows (this is a
        /// change in version 4.0):
        /// <list>
        /// <item>If the number is null, returns CBORObject.Null.</item>
        /// <item>Otherwise, if the number expresses infinity, not-a-number, or
        /// negative zero, the CBOR object will have tag 270 and the
        /// appropriate format.</item>
        /// <item>Otherwise, the CBOR object will have tag 30 and the
        /// appropriate format.</item></list></summary>
        /// <param name='bigValue'>An arbitrary-precision rational number. Can
        /// be null.</param>
        /// <returns>The given number encoded as a CBOR object. Returns
        /// CBORObject.Null if <paramref name='bigValue'/> is null.</returns>
        public static CBORObject FromObject(ERational bigValue)
        {
            if ((object)bigValue == (object)null)
            {
                return CBORObject.Null;
            }
            CBORObject cbor;
            int tag;
            if (bigValue.IsInfinity() || bigValue.IsNaN() ||
              (bigValue.IsNegative && bigValue.IsZero))
            {
                int options = bigValue.IsNegative ? 1 : 0;
                if (bigValue.IsInfinity())
                {
                    options += 2;
                }
                if (bigValue.IsQuietNaN())
                {
                    options += 4;
                }
                if (bigValue.IsSignalingNaN())
                {
                    options += 6;
                }
#if DEBUG
                if (!(!bigValue.IsInfinity() || bigValue.UnsignedNumerator.IsZero))
                {
                    throw new InvalidOperationException("doesn't satisfy" +
                      "\u0020!bigValue.IsInfinity() ||" +
                      "\u0020bigValue.UnsignedNumerator.IsZero");
                }
                if (!(!bigValue.IsInfinity() || bigValue.Denominator.CompareTo(1) ==
                    0))
                {
                    throw new InvalidOperationException("doesn't satisfy" +
                      "\u0020!bigValue.IsInfinity() ||" +
                      "\u0020bigValue.Denominator.CompareTo(1)==0");
                }
                if (!(!bigValue.IsNaN() || bigValue.Denominator.CompareTo(1) == 0))
                {
                    throw new InvalidOperationException("doesn't satisfy" +
                      "\u0020!bigValue.IsNaN() ||" +
                      "\u0020bigValue.Denominator.CompareTo(1)==0");
                }
#endif

                cbor = CBORObject.NewArray(
                    FromObject(bigValue.UnsignedNumerator),
                    FromObject(bigValue.Denominator),
                    FromObject(options));
                tag = 270;
            }
            else
            {
                tag = 30;
                cbor = CBORObject.NewArray(
                    CBORObject.FromObject(bigValue.Numerator),
                    CBORObject.FromObject(bigValue.Denominator));
            }
            return cbor.WithTag(tag);
        }

        /// <summary>Generates a CBOR object from a decimal number. The CBOR
        /// object is generated as follows (this is a change in version 4.0):
        /// <list>
        /// <item>If the number is null, returns CBORObject.Null.</item>
        /// <item>Otherwise, if the number expresses infinity, not-a-number, or
        /// negative zero, the CBOR object will have tag 268 and the
        /// appropriate format.</item>
        /// <item>If the number's exponent is at least 2^64 or less than
        /// -(2^64), the CBOR object will have tag 264 and the appropriate
        /// format.</item>
        /// <item>Otherwise, the CBOR object will have tag 4 and the
        /// appropriate format.</item></list></summary>
        /// <param name='bigValue'>An arbitrary-precision decimal number. Can
        /// be null.</param>
        /// <returns>The given number encoded as a CBOR object. Returns
        /// CBORObject.Null if <paramref name='bigValue'/> is null.</returns>
        public static CBORObject FromObject(EDecimal bigValue)
        {
            if ((object)bigValue == (object)null)
            {
                return CBORObject.Null;
            }
            CBORObject cbor;
            int tag;
            if (bigValue.IsInfinity() || bigValue.IsNaN() ||
              (bigValue.IsNegative && bigValue.IsZero))
            {
                int options = bigValue.IsNegative ? 1 : 0;
                if (bigValue.IsInfinity())
                {
                    options += 2;
                }
                if (bigValue.IsQuietNaN())
                {
                    options += 4;
                }
                if (bigValue.IsSignalingNaN())
                {
                    options += 6;
                }
                cbor = CBORObject.NewArray(
                    FromObject(bigValue.Exponent),
                    FromObject(bigValue.UnsignedMantissa),
                    FromObject(options));
                tag = 268;
            }
            else
            {
                EInteger exponent = bigValue.Exponent;
                if (exponent.CanFitInInt64())
                {
                    tag = 4;
                    cbor = CBORObject.NewArray(
                        CBORObject.FromObject(exponent.ToInt64Checked()),
                        CBORObject.FromObject(bigValue.Mantissa));
                }
                else
                {
                    tag = (exponent.GetSignedBitLengthAsInt64() > 64) ?
                      264 : 4;
                    cbor = CBORObject.NewArray(
                        CBORObject.FromObject(exponent),
                        CBORObject.FromObject(bigValue.Mantissa));
                }
            }
            return cbor.WithTag(tag);
        }

        /// <summary>Generates a CBOR object from a text string.</summary>
        /// <param name='strValue'>A text string value. Can be null.</param>
        /// <returns>A CBOR object representing the string, or CBORObject.Null
        /// if stringValue is null.</returns>
        /// <exception cref='ArgumentException'>The string contains an unpaired
        /// surrogate code point.</exception>
        public static CBORObject FromObject(string strValue)
        {
            if (strValue == null)
            {
                return CBORObject.Null;
            }
            if (strValue.Length == 0)
            {
                return GetFixedObject(0x60);
            }
            if (DataUtilities.GetUtf8Length(strValue, false) < 0)
            {
                throw new ArgumentException("String contains an unpaired " +
                  "surrogate code point.");
            }
            return new CBORObject(CBORObjectTypeTextString, strValue);
        }

        /// <summary>Generates a CBOR object from a 32-bit signed
        /// integer.</summary>
        /// <param name='value'>The parameter <paramref name='value'/> is a
        /// 32-bit signed integer.</param>
        /// <returns>A CBOR object.</returns>
        public static CBORObject FromObject(int value)
        {
            if (value >= 0 && value < 24)
            {
                return FixedObjects[value];
            }
            else
            {
                return (value >= -24 && value < 0) ? FixedObjects[0x20 - (value + 1)] :
                  FromObject((long)value);
            }
        }

        /// <summary>Generates a CBOR object from a 16-bit signed
        /// integer.</summary>
        /// <param name='value'>The parameter <paramref name='value'/> is a
        /// 16-bit signed integer.</param>
        /// <returns>A CBOR object generated from the given integer.</returns>
        public static CBORObject FromObject(short value)
        {
            if (value >= 0 && value < 24)
            {
                return FixedObjects[value];
            }
            else
            {
                return (value >= -24 && value < 0) ? FixedObjects[0x20 - (value + 1)] :
                  FromObject((long)value);
            }
        }

        /// <summary>Returns the CBOR true value or false value, depending on
        /// "value".</summary>
        /// <param name='value'>Either <c>true</c> or <c>false</c>.</param>
        /// <returns>CBORObject.True if value is true; otherwise
        /// CBORObject.False.</returns>
        public static CBORObject FromObject(bool value)
        {
            return value ? CBORObject.True : CBORObject.False;
        }

        /// <summary>Generates a CBOR object from a byte (0 to 255).</summary>
        /// <param name='value'>The parameter <paramref name='value'/> is a
        /// byte (from 0 to 255).</param>
        /// <returns>A CBOR object generated from the given integer.</returns>
        public static CBORObject FromObject(byte value)
        {
            return FromObject(((int)value) & 0xff);
        }

        /// <summary>Generates a CBOR object from a 32-bit floating-point
        /// number.</summary>
        /// <param name='value'>The parameter <paramref name='value'/> is a
        /// 32-bit floating-point number.</param>
        /// <returns>A CBOR object generated from the given number.</returns>
        public static CBORObject FromObject(float value)
        {
            long doubleBits = CBORUtilities.SingleToDoublePrecision(
                CBORUtilities.SingleToInt32Bits(value));
            return new CBORObject(CBORObjectTypeDouble, doubleBits);
        }

        /// <summary>Generates a CBOR object from a 64-bit floating-point
        /// number.</summary>
        /// <param name='value'>The parameter <paramref name='value'/> is a
        /// 64-bit floating-point number.</param>
        /// <returns>A CBOR object generated from the given number.</returns>
        public static CBORObject FromObject(double value)
        {
            long doubleBits = CBORUtilities.DoubleToInt64Bits(value);
            return new CBORObject(CBORObjectTypeDouble, doubleBits);
        }

        /// <summary>Generates a CBOR object from an array of 8-bit bytes; the
        /// byte array is copied to a new byte array in this process. (This
        /// method can't be used to decode CBOR data from a byte array; for
        /// that, use the <b>DecodeFromBytes</b> method instead.).</summary>
        /// <param name='bytes'>An array of 8-bit bytes; can be null.</param>
        /// <returns>A CBOR object where each element of the given byte array
        /// is copied to a new array, or CBORObject.Null if the value is
        /// null.</returns>
        public static CBORObject FromObject(byte[] bytes)
        {
            if (bytes == null)
            {
                return CBORObject.Null;
            }
            var newvalue = new byte[bytes.Length];
            Array.Copy(bytes, 0, newvalue, 0, bytes.Length);
            return new CBORObject(CBORObjectTypeByteString, bytes);
        }

        /// <summary>Generates a CBOR object from an array of CBOR
        /// objects.</summary>
        /// <param name='array'>An array of CBOR objects.</param>
        /// <returns>A CBOR object where each element of the given array is
        /// copied to a new array, or CBORObject.Null if the value is
        /// null.</returns>
        public static CBORObject FromObject(CBORObject[] array)
        {
            if (array == null)
            {
                return CBORObject.Null;
            }
            IList<CBORObject> list = new List<CBORObject>();
            foreach (CBORObject cbor in array)
            {
                list.Add(cbor);
            }
            return new CBORObject(CBORObjectTypeArray, list);
        }

        internal static CBORObject FromArrayBackedObject(CBORObject[] array)
        {
            if (array == null)
            {
                return CBORObject.Null;
            }
            IList<CBORObject> list = PropertyMap.ListFromArray(array);
            return new CBORObject(CBORObjectTypeArray, list);
        }

        /// <summary>Generates a CBOR object from an array of 32-bit
        /// integers.</summary>
        /// <param name='array'>An array of 32-bit integers.</param>
        /// <returns>A CBOR array object where each element of the given array
        /// is copied to a new array, or CBORObject.Null if the value is
        /// null.</returns>
        public static CBORObject FromObject(int[] array)
        {
            if (array == null)
            {
                return CBORObject.Null;
            }
            IList<CBORObject> list = new List<CBORObject>(array.Length ==
              Int32.MaxValue ? array.Length : (array.Length + 1));
            foreach (int i in array)
            {
                list.Add(FromObject(i));
            }
            return new CBORObject(CBORObjectTypeArray, list);
        }

        /// <summary>Generates a CBOR object from an array of 64-bit
        /// integers.</summary>
        /// <param name='array'>An array of 64-bit integers.</param>
        /// <returns>A CBOR array object where each element of the given array
        /// is copied to a new array, or CBORObject.Null if the value is
        /// null.</returns>
        public static CBORObject FromObject(long[] array)
        {
            if (array == null)
            {
                return CBORObject.Null;
            }
            IList<CBORObject> list = new List<CBORObject>(array.Length ==
              Int32.MaxValue ? array.Length : (array.Length + 1));
            foreach (long i in array)
            {
                list.Add(FromObject(i));
            }
            return new CBORObject(CBORObjectTypeArray, list);
        }

        /// <summary>Generates a CBORObject from an arbitrary object. See the
        /// overload of this method that takes CBORTypeMapper and PODOptions
        /// arguments.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object, which can be null.
        /// <para><b>NOTE:</b> For security reasons, whenever possible, an
        /// application should not base this parameter on user input or other
        /// externally supplied data unless the application limits this
        /// parameter's inputs to types specially handled by this method (such
        /// as <c>int</c> or <c>String</c> ) and/or to plain-old-data types
        /// (POCO or POJO types) within the control of the application. If the
        /// plain-old-data type references other data types, those types should
        /// likewise meet either criterion above.</para>.</param>
        /// <returns>A CBOR object corresponding to the given object. Returns
        /// CBORObject.Null if the object is null.</returns>
        public static CBORObject FromObject(object obj)
        {
            return FromObject(obj, PODOptions.Default);
        }

        /// <summary>Generates a CBORObject from an arbitrary object. See the
        /// overload of this method that takes CBORTypeMapper and PODOptions
        /// arguments.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.
        /// <para><b>NOTE:</b> For security reasons, whenever possible, an
        /// application should not base this parameter on user input or other
        /// externally supplied data unless the application limits this
        /// parameter's inputs to types specially handled by this method (such
        /// as <c>int</c> or <c>String</c> ) and/or to plain-old-data types
        /// (POCO or POJO types) within the control of the application. If the
        /// plain-old-data type references other data types, those types should
        /// likewise meet either criterion above.</para>.</param>
        /// <param name='options'>An object containing options to control how
        /// certain objects are converted to CBOR objects.</param>
        /// <returns>A CBOR object corresponding to the given object. Returns
        /// CBORObject.Null if the object is null.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='options'/> is null.</exception>
        public static CBORObject FromObject(
          object obj,
          PODOptions options)
        {
            return FromObject(obj, options, null, 0);
        }

        /// <summary>Generates a CBORObject from an arbitrary object. See the
        /// overload of this method that takes CBORTypeMapper and PODOptions
        /// arguments.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.
        /// <para><b>NOTE:</b> For security reasons, whenever possible, an
        /// application should not base this parameter on user input or other
        /// externally supplied data unless the application limits this
        /// parameter's inputs to types specially handled by this method (such
        /// as <c>int</c> or <c>String</c> ) and/or to plain-old-data types
        /// (POCO or POJO types) within the control of the application. If the
        /// plain-old-data type references other data types, those types should
        /// likewise meet either criterion above.</para>.</param>
        /// <param name='mapper'>An object containing optional converters to
        /// convert objects of certain types to CBOR objects.</param>
        /// <returns>A CBOR object corresponding to the given object. Returns
        /// CBORObject.Null if the object is null.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='mapper'/> is null.</exception>
        public static CBORObject FromObject(
          object obj,
          CBORTypeMapper mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }
            return FromObject(obj, PODOptions.Default, mapper, 0);
        }

        /// <summary>
        /// <para>Generates a CBORObject from an arbitrary object, using the
        /// given options to control how certain objects are converted to CBOR
        /// objects. The following cases are checked in the logical order given
        /// (rather than the strict order in which they are implemented by this
        /// library):</para>
        /// <list>
        /// <item><c>null</c> is converted to <c>CBORObject.Null</c>.</item>
        /// <item>A <c>CBORObject</c> is returned as itself.</item>
        /// <item>If the object is of a type corresponding to a type converter
        /// mentioned in the <paramref name='mapper'/> parameter, that
        /// converter will be used to convert the object to a CBOR object. Type
        /// converters can be used to override the default conversion behavior
        /// of almost any object.</item>
        /// <item>A <c>char</c> is converted to an integer (from 0 through
        /// 65535), and returns a CBOR object of that integer. (This is a
        /// change in version 4.0 from previous versions, which converted
        /// <c>char</c>, except surrogate code points from 0xd800 through
        /// 0xdfff, into single-character text strings.)</item>
        /// <item>A <c>bool</c> ( <c>boolean</c> in Java) is converted to
        /// <c>CBORObject.True</c> or <c>CBORObject.False</c>.</item>
        /// <item>A <c>byte</c> is converted to a CBOR integer from 0 through
        /// 255.</item>
        /// <item>A primitive integer type ( <c>int</c>, <c>short</c>,
        /// <c>long</c>, as well as <c>sbyte</c>, <c>ushort</c>, <c>uint</c>
        /// , and <c>ulong</c> in.NET) is converted to the corresponding CBOR
        /// integer.</item>
        /// <item>A primitive floating-point type ( <c>float</c>,
        /// <c>double</c>, as well as <c>decimal</c> in.NET) is converted to
        /// the corresponding CBOR number.</item>
        /// <item>A <c>String</c> is converted to a CBOR text string. To create
        /// a CBOR byte string object from <c>String</c>, see the example
        /// given in
        /// <see
        /// cref='PeterO.Cbor.CBORObject.FromObject(System.Byte[])'/>.</item>
        /// <item>In the.NET version, a nullable is converted to
        /// <c>CBORObject.Null</c> if the nullable's value is <c>null</c>, or
        /// converted according to the nullable's underlying type, if that type
        /// is supported by this method.</item>
        /// <item>A number of type <c>EDecimal</c>, <c>EFloat</c>,
        /// <c>EInteger</c>, and <c>ERational</c> in the
        /// <a
        ///   href='https://www.nuget.org/packages/PeterO.Numbers'><c>PeterO.Numbers</c></a>
        /// library (in .NET) or the
        /// <a
        ///   href='https://github.com/peteroupc/numbers-java'><c>com.github.peteroupc/numbers</c></a>
        /// artifact (in Java) is converted to the corresponding CBOR
        /// number.</item>
        /// <item>An array other than <c>byte[]</c> is converted to a CBOR
        /// array. In the.NET version, a multidimensional array is converted to
        /// an array of arrays.</item>
        /// <item>A <c>byte[]</c> (1-dimensional byte array) is converted to a
        /// CBOR byte string; the byte array is copied to a new byte array in
        /// this process. (This method can't be used to decode CBOR data from a
        /// byte array; for that, use the <b>DecodeFromBytes</b> method
        /// instead.)</item>
        /// <item>An object implementing IDictionary (Map in Java) is converted
        /// to a CBOR map containing the keys and values enumerated.</item>
        /// <item>An object implementing IEnumerable (Iterable in Java) is
        /// converted to a CBOR array containing the items enumerated.</item>
        /// <item>An enumeration ( <c>Enum</c> ) object is converted to its
        /// <i>underlying value</i> in the.NET version, or the result of its
        /// <c>ordinal()</c> method in the Java version.</item>
        /// <item>An object of type <c>DateTime</c>, <c>Uri</c>, or
        /// <c>Guid</c> ( <c>Date</c>, <c>URI</c>, or <c>UUID</c>,
        /// respectively, in Java) will be converted to a tagged CBOR object of
        /// the appropriate kind. <c>DateTime</c> / <c>Date</c> will be
        /// converted to a tag-0 string following the date format used in the
        /// Atom syndication format.</item>
        /// <item>If the object is a type not specially handled above, this
        /// method checks the <paramref name='obj'/> parameter for eligible
        /// getters as follows:</item>
        /// <item>(*) In the .NET version, eligible getters are the public,
        /// nonstatic getters of read/write properties (and also those of
        /// read-only properties in the case of a compiler-generated type or an
        /// F# type). Eligible getters also include public, nonstatic, non-
        /// <c>const</c>, non- <c>readonly</c> fields. If a class has two
        /// properties and/or fields of the form "X" and "IsX", where "X" is
        /// any name, or has multiple properties and/or fields with the same
        /// name, those properties and fields are ignored.</item>
        /// <item>(*) In the Java version, eligible getters are public,
        /// nonstatic methods starting with "get" or "is" (either word followed
        /// by a character other than a basic digit or lower-case letter, that
        /// is, other than "a" to "z" or "0" to "9"), that take no parameters
        /// and do not return void, except that methods named "getClass" are
        /// not eligible getters. In addition, public, nonstatic, nonfinal
        /// fields are also eligible getters. If a class has two otherwise
        /// eligible getters (methods and/or fields) of the form "isX" and
        /// "getX", where "X" is the same in both, or two such getters with the
        /// same name but different return type, they are not eligible
        /// getters.</item>
        /// <item>Then, the method returns a CBOR map with each eligible
        /// getter's name or property name as each key, and with the
        /// corresponding value returned by that getter as that key's value.
        /// Before adding a key-value pair to the map, the key's name is
        /// adjusted according to the rules described in the
        /// <see cref='PeterO.Cbor.PODOptions'/> documentation. Note that for
        /// security reasons, certain types are not supported even if they
        /// contain eligible getters.</item></list>
        /// <para><b>REMARK:</b>.NET enumeration ( <c>Enum</c> ) constants
        /// could also have been converted to text strings with
        /// <c>ToString()</c>, but that method will return multiple names if
        /// the given Enum object is a combination of Enum objects (e.g. if the
        /// object is <c>FileAccess.Read | FileAccess.Write</c> ). More
        /// generally, if Enums are converted to text strings, constants from
        /// Enum types with the <c>Flags</c> attribute, and constants from the
        /// same Enum type that share an underlying value, should not be passed
        /// to this method.</para></summary>
        /// <param name='obj'>An arbitrary object to convert to a CBOR object.
        /// <para><b>NOTE:</b> For security reasons, whenever possible, an
        /// application should not base this parameter on user input or other
        /// externally supplied data unless the application limits this
        /// parameter's inputs to types specially handled by this method (such
        /// as <c>int</c> or <c>String</c> ) and/or to plain-old-data types
        /// (POCO or POJO types) within the control of the application. If the
        /// plain-old-data type references other data types, those types should
        /// likewise meet either criterion above.</para>.</param>
        /// <param name='mapper'>An object containing optional converters to
        /// convert objects of certain types to CBOR objects. Can be
        /// null.</param>
        /// <param name='options'>An object containing options to control how
        /// certain objects are converted to CBOR objects.</param>
        /// <returns>A CBOR object corresponding to the given object. Returns
        /// CBORObject.Null if the object is null.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='options'/> is null.</exception>
        /// <exception cref='PeterO.Cbor.CBORException'>An error occurred while
        /// converting the given object to a CBOR object.</exception>
        public static CBORObject FromObject(
          object obj,
          CBORTypeMapper mapper,
          PODOptions options)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }
            return FromObject(obj, options, mapper, 0);
        }

        internal static CBORObject FromObject(
          object obj,
          PODOptions options,
          CBORTypeMapper mapper,
          int depth)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (depth >= 100)
            {
                throw new CBORException("Nesting depth too high");
            }
            if (obj == null)
            {
                return CBORObject.Null;
            }
            if (obj is CBORObject)
            {
                return FromObject((CBORObject)obj);
            }
            CBORObject objret;
            if (mapper != null)
            {
                objret = mapper.ConvertWithConverter(obj);
                if (objret != null)
                {
                    return objret;
                }
            }
            if (obj is string)
            {
                return FromObject((string)obj);
            }
            if (obj is int)
            {
                return FromObject((int)obj);
            }
            if (obj is long)
            {
                return FromObject((long)obj);
            }
            var eif = obj as EInteger;
            if (eif != null)
            {
                return FromObject(eif);
            }
            var edf = obj as EDecimal;
            if (edf != null)
            {
                return FromObject(edf);
            }
            var eff = obj as EFloat;
            if (eff != null)
            {
                return FromObject(eff);
            }
            var erf = obj as ERational;
            if (erf != null)
            {
                return FromObject(erf);
            }
            if (obj is short)
            {
                return FromObject((short)obj);
            }
            if (obj is char)
            {
                return FromObject((int)(char)obj);
            }
            if (obj is bool)
            {
                return FromObject((bool)obj);
            }
            if (obj is byte)
            {
                return FromObject((byte)obj);
            }
            if (obj is float)
            {
                return FromObject((float)obj);
            }
            if (obj is sbyte)
            {
                return FromObject((sbyte)obj);
            }
            if (obj is ulong)
            {
                return FromObject((ulong)obj);
            }
            if (obj is uint)
            {
                return FromObject((uint)obj);
            }
            if (obj is ushort)
            {
                return FromObject((ushort)obj);
            }
            if (obj is decimal)
            {
                return FromObject((decimal)obj);
            }
            if (obj is double)
            {
                return FromObject((double)obj);
            }
            byte[] bytearr = obj as byte[];
            if (bytearr != null)
            {
                return FromObject(bytearr);
            }
            if (obj is System.Collections.IDictionary)
            {
                // IDictionary appears first because IDictionary includes IEnumerable
                objret = CBORObject.NewMap();
                System.Collections.IDictionary objdic =
                  (System.Collections.IDictionary)obj;
                foreach (object keyPair in (System.Collections.IDictionary)objdic)
                {
                    System.Collections.DictionaryEntry
                    kvp = (System.Collections.DictionaryEntry)keyPair;
                    CBORObject objKey = CBORObject.FromObject(
                        kvp.Key,
                        options,
                        mapper,
                        depth + 1);
                    objret[objKey] = CBORObject.FromObject(
                        kvp.Value,
                        options,
                        mapper,
                        depth + 1);
                }
                return objret;
            }
            if (obj is Array)
            {
                return PropertyMap.FromArray(obj, options, mapper, depth);
            }
            if (obj is System.Collections.IEnumerable)
            {
                objret = CBORObject.NewArray();
                foreach (object element in (System.Collections.IEnumerable)obj)
                {
                    objret.Add(
                      CBORObject.FromObject(
                        element,
                        options,
                        mapper,
                        depth + 1));
                }
                return objret;
            }
            if (obj is Enum)
            {
                return FromObject(PropertyMap.EnumToObjectAsInteger((Enum)obj));
            }
            if (obj is DateTime)
            {
                return new CBORDateConverter().ToCBORObject((DateTime)obj);
            }
            if (obj is Uri)
            {
                return new CBORUriConverter().ToCBORObject((Uri)obj);
            }
            if (obj is Guid)
            {
                return new CBORUuidConverter().ToCBORObject((Guid)obj);
            }
            objret = CBORObject.NewMap();
            foreach (KeyValuePair<string, object> key in
              PropertyMap.GetProperties(
                obj,
                options.UseCamelCase))
            {
                objret[key.Key] = CBORObject.FromObject(
                    key.Value,
                    options,
                    mapper,
                    depth + 1);
            }
            return objret;
        }

        /// <summary>Generates a CBOR object from this one, but gives the
        /// resulting object a tag in addition to its existing tags (the new
        /// tag is made the outermost tag).</summary>
        /// <param name='bigintTag'>Tag number. The tag number 55799 can be
        /// used to mark a "self-described CBOR" object. This document does not
        /// attempt to list all CBOR tags and their meanings. An up-to-date
        /// list can be found at the CBOR Tags registry maintained by the
        /// Internet Assigned Numbers Authority(
        /// <i>iana.org/assignments/cbor-tags</i> ).</param>
        /// <returns>A CBOR object with the same value as this one but given
        /// the tag <paramref name='bigintTag'/> in addition to its existing
        /// tags (the new tag is made the outermost tag).</returns>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='bigintTag'/> is less than 0 or greater than
        /// 2^64-1.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='bigintTag'/> is null.</exception>
        public CBORObject WithTag(EInteger bigintTag)
        {
            if (bigintTag == null)
            {
                throw new ArgumentNullException(nameof(bigintTag));
            }
            if (bigintTag.Sign < 0)
            {
                throw new ArgumentException("tagEInt's sign(" + bigintTag.Sign +
                  ") is less than 0");
            }
            if (bigintTag.CanFitInInt32())
            {
                // Low-numbered, commonly used tags
                return this.WithTag(bigintTag.ToInt32Checked());
            }
            else
            {
                if (bigintTag.CompareTo(UInt64MaxValue) > 0)
                {
                    throw new ArgumentException(
                      "tag more than 18446744073709551615 (" + bigintTag + ")");
                }
                var tagLow = 0;
                var tagHigh = 0;
                byte[] bytes = bigintTag.ToBytes(true);
                for (var i = 0; i < Math.Min(4, bytes.Length); ++i)
                {
                    int b = ((int)bytes[i]) & 0xff;
                    tagLow = unchecked(tagLow | (((int)b) << (i * 8)));
                }
                for (int i = 4; i < Math.Min(8, bytes.Length); ++i)
                {
                    int b = ((int)bytes[i]) & 0xff;
                    tagHigh = unchecked(tagHigh | (((int)b) << (i * 8)));
                }
                return new CBORObject(this, tagLow, tagHigh);
            }
        }

        /// <summary>Generates a CBOR object from an arbitrary object and gives
        /// the resulting object a tag in addition to its existing tags (the
        /// new tag is made the outermost tag).</summary>
        /// <param name='valueOb'>The parameter <paramref name='valueOb'/> is
        /// an arbitrary object, which can be null.
        /// <para><b>NOTE:</b> For security reasons, whenever possible, an
        /// application should not base this parameter on user input or other
        /// externally supplied data unless the application limits this
        /// parameter's inputs to types specially handled by this method (such
        /// as <c>int</c> or <c>String</c> ) and/or to plain-old-data types
        /// (POCO or POJO types) within the control of the application. If the
        /// plain-old-data type references other data types, those types should
        /// likewise meet either criterion above.</para>.</param>
        /// <param name='bigintTag'>Tag number. The tag number 55799 can be
        /// used to mark a "self-described CBOR" object. This document does not
        /// attempt to list all CBOR tags and their meanings. An up-to-date
        /// list can be found at the CBOR Tags registry maintained by the
        /// Internet Assigned Numbers Authority(
        /// <i>iana.org/assignments/cbor-tags</i> ).</param>
        /// <returns>A CBOR object where the object <paramref name='valueOb'/>
        /// is converted to a CBOR object and given the tag <paramref
        /// name='bigintTag'/>. If <paramref name='valueOb'/> is null, returns
        /// a version of CBORObject.Null with the given tag.</returns>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='bigintTag'/> is less than 0 or greater than
        /// 2^64-1.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='bigintTag'/> is null.</exception>
        public static CBORObject FromObjectAndTag(
          object valueOb,
          EInteger bigintTag)
        {
            if (bigintTag == null)
            {
                throw new ArgumentNullException(nameof(bigintTag));
            }
            if (bigintTag.Sign < 0)
            {
                throw new ArgumentException("tagEInt's sign(" + bigintTag.Sign +
                  ") is less than 0");
            }
            if (bigintTag.CompareTo(UInt64MaxValue) > 0)
            {
                throw new ArgumentException(
                  "tag more than 18446744073709551615 (" + bigintTag + ")");
            }
            return FromObject(valueOb).WithTag(bigintTag);
        }

        /// <summary>Generates a CBOR object from an arbitrary object and gives
        /// the resulting object a tag in addition to its existing tags (the
        /// new tag is made the outermost tag).</summary>
        /// <param name='smallTag'>A 32-bit integer that specifies a tag
        /// number. The tag number 55799 can be used to mark a "self-described
        /// CBOR" object. This document does not attempt to list all CBOR tags
        /// and their meanings. An up-to-date list can be found at the CBOR
        /// Tags registry maintained by the Internet Assigned Numbers Authority
        /// (
        /// <i>iana.org/assignments/cbor-tags</i> ).</param>
        /// <returns>A CBOR object with the same value as this one but given
        /// the tag <paramref name='smallTag'/> in addition to its existing
        /// tags (the new tag is made the outermost tag).</returns>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='smallTag'/> is less than 0.</exception>
        public CBORObject WithTag(int smallTag)
        {
            if (smallTag < 0)
            {
                throw new ArgumentException("smallTag(" + smallTag +
                  ") is less than 0");
            }
            return new CBORObject(this, smallTag, 0);
        }

        /// <summary>Generates a CBOR object from an arbitrary object and gives
        /// the resulting object a tag in addition to its existing tags (the
        /// new tag is made the outermost tag).</summary>
        /// <param name='valueObValue'>The parameter <paramref
        /// name='valueObValue'/> is an arbitrary object, which can be null.
        /// <para><b>NOTE:</b> For security reasons, whenever possible, an
        /// application should not base this parameter on user input or other
        /// externally supplied data unless the application limits this
        /// parameter's inputs to types specially handled by this method (such
        /// as <c>int</c> or <c>String</c> ) and/or to plain-old-data types
        /// (POCO or POJO types) within the control of the application. If the
        /// plain-old-data type references other data types, those types should
        /// likewise meet either criterion above.</para>.</param>
        /// <param name='smallTag'>A 32-bit integer that specifies a tag
        /// number. The tag number 55799 can be used to mark a "self-described
        /// CBOR" object. This document does not attempt to list all CBOR tags
        /// and their meanings. An up-to-date list can be found at the CBOR
        /// Tags registry maintained by the Internet Assigned Numbers Authority
        /// (
        /// <i>iana.org/assignments/cbor-tags</i> ).</param>
        /// <returns>A CBOR object where the object <paramref
        /// name='valueObValue'/> is converted to a CBOR object and given the
        /// tag <paramref name='smallTag'/>. If "valueOb" is null, returns a
        /// version of CBORObject.Null with the given tag.</returns>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='smallTag'/> is less than 0.</exception>
        public static CBORObject FromObjectAndTag(
          object valueObValue,
          int smallTag)
        {
            if (smallTag < 0)
            {
                throw new ArgumentException("smallTag(" + smallTag +
                  ") is less than 0");
            }
            return FromObject(valueObValue).WithTag(smallTag);
        }

        /// <summary>Creates a CBOR object from a simple value
        /// number.</summary>
        /// <param name='simpleValue'>The parameter <paramref
        /// name='simpleValue'/> is a 32-bit signed integer.</param>
        /// <returns>A CBOR object.</returns>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='simpleValue'/> is less than 0, greater than 255, or from 24
        /// through 31.</exception>
        public static CBORObject FromSimpleValue(int simpleValue)
        {
            if (simpleValue < 0)
            {
                throw new ArgumentException("simpleValue(" + simpleValue +
                  ") is less than 0");
            }
            if (simpleValue > 255)
            {
                throw new ArgumentException("simpleValue(" + simpleValue +
                  ") is more than " + "255");
            }
            if (simpleValue >= 24 && simpleValue < 32)
            {
                throw new ArgumentException("Simple value is from 24 to 31: " +
                  simpleValue);
            }
            if (simpleValue < 32)
            {
                return FixedObjects[0xe0 + simpleValue];
            }
            return new CBORObject(
                CBORObjectTypeSimpleValue,
                simpleValue);
        }

        /// <summary>Multiplies two CBOR numbers.</summary>
        /// <param name='first'>The parameter <paramref name='first'/> is a
        /// CBOR object.</param>
        /// <param name='second'>The parameter <paramref name='second'/> is a
        /// CBOR object.</param>
        /// <returns>The product of the two numbers.</returns>
        /// <exception cref='ArgumentException'>Either or both operands are not
        /// numbers (as opposed to Not-a-Number, NaN).</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='first'/> or <paramref name='second'/> is null.</exception>
        [Obsolete("Instead, convert both CBOR objects to numbers (with" +
            "\u0020.AsNumber()), and use the first number's .Multiply() method.")]
        public static CBORObject Multiply(CBORObject first, CBORObject second)
        {
            if (first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }
            if (second == null)
            {
                throw new ArgumentNullException(nameof(second));
            }
            CBORNumber a = CBORNumber.FromCBORObject(first);
            if (a == null)
            {
                throw new ArgumentException(nameof(first) + "does not represent a" +
                  "\u0020number");
            }
            CBORNumber b = CBORNumber.FromCBORObject(second);
            if (b == null)
            {
                throw new ArgumentException(nameof(second) + "does not represent a" +
                  "\u0020number");
            }
            return a.Multiply(b).ToCBORObject();
        }

        /// <summary>Creates a new empty CBOR array.</summary>
        /// <returns>A new CBOR array.</returns>
        public static CBORObject NewArray()
        {
            return new CBORObject(CBORObjectTypeArray, new List<CBORObject>());
        }

        internal static CBORObject NewArray(CBORObject o1, CBORObject o2)
        {
            var list = new List<CBORObject>(2);
            list.Add(o1);
            list.Add(o2);
            return new CBORObject(CBORObjectTypeArray, list);
        }

        internal static CBORObject NewArray(
          CBORObject o1,
          CBORObject o2,
          CBORObject o3)
        {
            var list = new List<CBORObject>(2);
            list.Add(o1);
            list.Add(o2);
            list.Add(o3);
            return new CBORObject(CBORObjectTypeArray, list);
        }

        /// <summary>Creates a new empty CBOR map.</summary>
        /// <returns>A new CBOR map.</returns>
        public static CBORObject NewMap()
        {
            return new CBORObject(
                CBORObjectTypeMap,
                new SortedDictionary<CBORObject, CBORObject>());
        }

        /// <summary>
        /// <para>Reads a sequence of objects in CBOR format from a data
        /// stream. This method will read CBOR objects from the stream until
        /// the end of the stream is reached or an error occurs, whichever
        /// happens first.</para></summary>
        /// <param name='stream'>A readable data stream.</param>
        /// <returns>An array containing the CBOR objects that were read from
        /// the data stream. Returns an empty array if there is no unread data
        /// in the stream.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null, or the parameter "options" is
        /// null.</exception>
        /// <exception cref='PeterO.Cbor.CBORException'>There was an error in
        /// reading or parsing the data, including if the last CBOR object was
        /// read only partially.</exception>
        public static CBORObject[] ReadSequence(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            var cborList = new List<CBORObject>();
            while (true)
            {
                CBORObject obj = Read(stream, AllowEmptyOptions);
                if (obj == null)
                {
                    break;
                }
                cborList.Add(obj);
            }
            return (CBORObject[])cborList.ToArray();
        }

        /// <summary>
        /// <para>Reads a sequence of objects in CBOR format from a data
        /// stream. This method will read CBOR objects from the stream until
        /// the end of the stream is reached or an error occurs, whichever
        /// happens first.</para></summary>
        /// <param name='stream'>A readable data stream.</param>
        /// <param name='options'>Specifies the options to use when decoding
        /// the CBOR data stream. See CBOREncodeOptions for more information.
        /// In this method, the AllowEmpty property is treated as set
        /// regardless of the value of that property specified in this
        /// parameter.</param>
        /// <returns>An array containing the CBOR objects that were read from
        /// the data stream. Returns an empty array if there is no unread data
        /// in the stream.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null, or the parameter <paramref
        /// name='options'/> is null.</exception>
        /// <exception cref='PeterO.Cbor.CBORException'>There was an error in
        /// reading or parsing the data, including if the last CBOR object was
        /// read only partially.</exception>
        public static CBORObject[] ReadSequence(Stream stream, CBOREncodeOptions
          options)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            CBOREncodeOptions opt = options;
            if (!opt.AllowEmpty)
            {
                opt = new CBOREncodeOptions(opt.ToString() + ";allowempty=1");
            }
            var cborList = new List<CBORObject>();
            while (true)
            {
                CBORObject obj = Read(stream, opt);
                if (obj == null)
                {
                    break;
                }
                cborList.Add(obj);
            }
            return (CBORObject[])cborList.ToArray();
        }

        /// <summary>
        /// <para>Reads an object in CBOR format from a data stream. This
        /// method will read from the stream until the end of the CBOR object
        /// is reached or an error occurs, whichever happens
        /// first.</para></summary>
        /// <param name='stream'>A readable data stream.</param>
        /// <returns>A CBOR object that was read.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        /// <exception cref='PeterO.Cbor.CBORException'>There was an error in
        /// reading or parsing the data.</exception>
        public static CBORObject Read(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            try
            {
                var reader = new CBORReader(stream);
                return reader.Read();
            }
            catch (IOException ex)
            {
                throw new CBORException("I/O error occurred.", ex);
            }
        }

        /// <summary>Reads an object in CBOR format from a data stream, using
        /// the specified options to control the decoding process. This method
        /// will read from the stream until the end of the CBOR object is
        /// reached or an error occurs, whichever happens first.</summary>
        /// <param name='stream'>A readable data stream.</param>
        /// <param name='options'>Specifies the options to use when decoding
        /// the CBOR data stream. See CBOREncodeOptions for more
        /// information.</param>
        /// <returns>A CBOR object that was read.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        /// <exception cref='PeterO.Cbor.CBORException'>There was an error in
        /// reading or parsing the data.</exception>
        public static CBORObject Read(Stream stream, CBOREncodeOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            try
            {
                var reader = new CBORReader(stream, options);
                return reader.Read();
            }
            catch (IOException ex)
            {
                throw new CBORException("I/O error occurred.", ex);
            }
        }

        /// <summary>Generates a CBOR object from a data stream in JavaScript
        /// Object Notation (JSON) format. The JSON stream may begin with a
        /// byte-order mark (U+FEFF). Since version 2.0, the JSON stream can be
        /// in UTF-8, UTF-16, or UTF-32 encoding; the encoding is detected by
        /// assuming that the first character read must be a byte-order mark or
        /// a nonzero basic character (U+0001 to U+007F). (In previous
        /// versions, only UTF-8 was allowed.).</summary>
        /// <param name='stream'>A readable data stream. The sequence of bytes
        /// read from the data stream must contain a single JSON object and not
        /// multiple objects.</param>
        /// <returns>A CBOR object.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        /// <exception cref='PeterO.Cbor.CBORException'>The data stream
        /// contains invalid encoding or is not in JSON format.</exception>
        public static CBORObject ReadJSON(Stream stream)
        {
            return ReadJSON(stream, JSONOptions.Default);
        }

        /// <summary>Generates a list of CBOR objects from a data stream in
        /// JavaScript Object Notation (JSON) text sequence format (RFC 7464).
        /// The data stream must be in UTF-8 encoding and may not begin with a
        /// byte-order mark (U+FEFF).</summary>
        /// <param name='stream'>A readable data stream. The sequence of bytes
        /// read from the data stream must either be empty or begin with a
        /// record separator byte (0x1e).</param>
        /// <returns>A list of CBOR objects read from the JSON sequence.
        /// Objects that could not be parsed are replaced with <c>null</c> (as
        /// opposed to <c>CBORObject.Null</c> ) in the given list.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        /// <exception cref='PeterO.Cbor.CBORException'>The data stream is not
        /// empty and does not begin with a record separator byte
        /// (0x1e).</exception>
        /// <remarks>Generally, each JSON text in a JSON text sequence is
        /// written as follows: Write a record separator byte (0x1e), then
        /// write the JSON text in UTF-8 (without a byte order mark, U+FEFF),
        /// then write the line feed byte (0x0a). RFC 7464, however, uses a
        /// more liberal syntax for parsing JSON text sequences.</remarks>
        public static CBORObject[] ReadJSONSequence(Stream stream)
        {
            return ReadJSONSequence(stream, JSONOptions.Default);
        }

        /// <summary>Generates a CBOR object from a data stream in JavaScript
        /// Object Notation (JSON) format, using the specified options to
        /// control the decoding process. The JSON stream may begin with a
        /// byte-order mark (U+FEFF). Since version 2.0, the JSON stream can be
        /// in UTF-8, UTF-16, or UTF-32 encoding; the encoding is detected by
        /// assuming that the first character read must be a byte-order mark or
        /// a nonzero basic character (U+0001 to U+007F). (In previous
        /// versions, only UTF-8 was allowed.).</summary>
        /// <param name='stream'>A readable data stream. The sequence of bytes
        /// read from the data stream must contain a single JSON object and not
        /// multiple objects.</param>
        /// <param name='options'>Contains options to control the JSON decoding
        /// process. This method uses only the AllowDuplicateKeys property of
        /// this object.</param>
        /// <returns>A CBOR object containing the JSON data decoded.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        /// <exception cref='PeterO.Cbor.CBORException'>The data stream
        /// contains invalid encoding or is not in JSON format.</exception>
        [Obsolete("Instead, use .ReadJSON\u0028stream, new" +
            "\u0020JSONOptions\u0028\"allowduplicatekeys=true\")) or" +
            "\u0020.ReadJSON\u0028stream, new" +
            "\u0020JSONOptions\u0028\"allowduplicatekeys=false\")), as" +
            "\u0020appropriate.")]
        public static CBORObject ReadJSON(
          Stream stream,
          CBOREncodeOptions options)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            var jsonoptions = new JSONOptions(options.AllowDuplicateKeys ?
              "allowduplicatekeys=1" : "allowduplicatekeys=0");
            return ReadJSON(stream, jsonoptions);
        }

        /// <summary>Generates a list of CBOR objects from a data stream in
        /// JavaScript Object Notation (JSON) text sequence format (RFC 7464).
        /// The data stream must be in UTF-8 encoding and may not begin with a
        /// byte-order mark (U+FEFF).</summary>
        /// <param name='stream'>A readable data stream. The sequence of bytes
        /// read from the data stream must either be empty or begin with a
        /// record separator byte (0x1e).</param>
        /// <param name='jsonoptions'>Specifies options to control how JSON
        /// texts in the stream are decoded to CBOR. See the JSONOptions
        /// class.</param>
        /// <returns>A list of CBOR objects read from the JSON sequence.
        /// Objects that could not be parsed are replaced with <c>null</c> (as
        /// opposed to <c>CBORObject.Null</c> ) in the given list.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        /// <exception cref='PeterO.Cbor.CBORException'>The data stream is not
        /// empty and does not begin with a record separator byte
        /// (0x1e).</exception>
        /// <remarks>Generally, each JSON text in a JSON text sequence is
        /// written as follows: Write a record separator byte (0x1e), then
        /// write the JSON text in UTF-8 (without a byte order mark, U+FEFF),
        /// then write the line feed byte (0x0a). RFC 7464, however, uses a
        /// more liberal syntax for parsing JSON text sequences.</remarks>
        public static CBORObject[] ReadJSONSequence(Stream stream, JSONOptions
          jsonoptions)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (jsonoptions == null)
            {
                throw new ArgumentNullException(nameof(jsonoptions));
            }
            CharacterInputWithCount reader;
            reader = new CharacterInputWithCount(
              new CharacterReader(stream, 0, true, true));
            try
            {
                var nextchar = new int[1];
                CBORObject[] objlist = CBORJson.ParseJSONSequence(
                    reader,
                    jsonoptions,
                    nextchar);
                if (nextchar[0] != -1)
                {
                    reader.RaiseError("End of data stream not reached");
                }
                return objlist;
            }
            catch (CBORException ex)
            {
                var ioex = ex.InnerException as IOException;
                if (ioex != null)
                {
                    throw ioex;
                }
                throw;
            }
        }

        /// <summary>Generates a CBOR object from a data stream in JavaScript
        /// Object Notation (JSON) format, using the specified options to
        /// control the decoding process. The JSON stream may begin with a
        /// byte-order mark (U+FEFF). Since version 2.0, the JSON stream can be
        /// in UTF-8, UTF-16, or UTF-32 encoding; the encoding is detected by
        /// assuming that the first character read must be a byte-order mark or
        /// a nonzero basic character (U+0001 to U+007F). (In previous
        /// versions, only UTF-8 was allowed.).</summary>
        /// <param name='stream'>A readable data stream. The sequence of bytes
        /// read from the data stream must contain a single JSON object and not
        /// multiple objects.</param>
        /// <param name='jsonoptions'>Specifies options to control how the JSON
        /// stream is decoded to CBOR. See the JSONOptions class.</param>
        /// <returns>A CBOR object containing the JSON data decoded.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        /// <exception cref='PeterO.Cbor.CBORException'>The data stream
        /// contains invalid encoding or is not in JSON format.</exception>
        public static CBORObject ReadJSON(
          Stream stream,
          JSONOptions jsonoptions)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (jsonoptions == null)
            {
                throw new ArgumentNullException(nameof(jsonoptions));
            }
            CharacterInputWithCount reader;
            reader = new CharacterInputWithCount(
              new CharacterReader(stream, 2, true));
            try
            {
                var nextchar = new int[1];
                CBORObject obj = CBORJson.ParseJSONValue(
                    reader,
                    jsonoptions,
                    nextchar);
                if (nextchar[0] != -1)
                {
                    reader.RaiseError("End of data stream not reached");
                }
                return obj;
            }
            catch (CBORException ex)
            {
                var ioex = ex.InnerException as IOException;
                if (ioex != null)
                {
                    throw ioex;
                }
                throw;
            }
        }

        /// <summary>
        /// <para>Generates a CBOR object from a byte array in JavaScript
        /// Object Notation (JSON) format.</para>
        /// <para>If a JSON object has duplicate keys, a CBORException is
        /// thrown.</para>
        /// <para>Note that if a CBOR object is converted to JSON with
        /// <c>ToJSONBytes</c>, then the JSON is converted back to CBOR with
        /// this method, the new CBOR object will not necessarily be the same
        /// as the old CBOR object, especially if the old CBOR object uses data
        /// types not supported in JSON, such as integers in map
        /// keys.</para></summary>
        /// <param name='bytes'>A byte array in JSON format. The entire byte
        /// array must contain a single JSON object and not multiple objects.
        /// The byte array may begin with a byte-order mark (U+FEFF). The byte
        /// array can be in UTF-8, UTF-16, or UTF-32 encoding; the encoding is
        /// detected by assuming that the first character read must be a
        /// byte-order mark or a nonzero basic character (U+0001 to
        /// U+007F).</param>
        /// <returns>A CBOR object containing the JSON data decoded.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='bytes'/> is null.</exception>
        /// <exception cref='PeterO.Cbor.CBORException'>The byte array contains
        /// invalid encoding or is not in JSON format.</exception>
        public static CBORObject FromJSONBytes(byte[] bytes)
        {
            return FromJSONBytes(bytes, JSONOptions.Default);
        }

        /// <summary>Generates a CBOR object from a byte array in JavaScript
        /// Object Notation (JSON) format, using the specified options to
        /// control the decoding process.
        /// <para>Note that if a CBOR object is converted to JSON with
        /// <c>ToJSONBytes</c>, then the JSON is converted back to CBOR with
        /// this method, the new CBOR object will not necessarily be the same
        /// as the old CBOR object, especially if the old CBOR object uses data
        /// types not supported in JSON, such as integers in map
        /// keys.</para></summary>
        /// <param name='bytes'>A byte array in JSON format. The entire byte
        /// array must contain a single JSON object and not multiple objects.
        /// The byte array may begin with a byte-order mark (U+FEFF). The byte
        /// array can be in UTF-8, UTF-16, or UTF-32 encoding; the encoding is
        /// detected by assuming that the first character read must be a
        /// byte-order mark or a nonzero basic character (U+0001 to
        /// U+007F).</param>
        /// <param name='jsonoptions'>Specifies options to control how the JSON
        /// data is decoded to CBOR. See the JSONOptions class.</param>
        /// <returns>A CBOR object containing the JSON data decoded.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='bytes'/> or <paramref name='jsonoptions'/> is
        /// null.</exception>
        /// <exception cref='PeterO.Cbor.CBORException'>The byte array contains
        /// invalid encoding or is not in JSON format.</exception>
        public static CBORObject FromJSONBytes(
          byte[] bytes,
          JSONOptions jsonoptions)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }
            if (jsonoptions == null)
            {
                throw new ArgumentNullException(nameof(jsonoptions));
            }
            if (bytes.Length == 0)
            {
                throw new CBORException("Byte array is empty");
            }
            return FromJSONBytes(bytes, 0, bytes.Length, jsonoptions);
        }

        /// <summary>
        /// <para>Generates a CBOR object from a byte array in JavaScript
        /// Object Notation (JSON) format.</para>
        /// <para>If a JSON object has duplicate keys, a CBORException is
        /// thrown.</para>
        /// <para>Note that if a CBOR object is converted to JSON with
        /// <c>ToJSONBytes</c>, then the JSON is converted back to CBOR with
        /// this method, the new CBOR object will not necessarily be the same
        /// as the old CBOR object, especially if the old CBOR object uses data
        /// types not supported in JSON, such as integers in map
        /// keys.</para></summary>
        /// <param name='bytes'>A byte array, the specified portion of which is
        /// in JSON format. The specified portion of the byte array must
        /// contain a single JSON object and not multiple objects. The portion
        /// may begin with a byte-order mark (U+FEFF). The portion can be in
        /// UTF-8, UTF-16, or UTF-32 encoding; the encoding is detected by
        /// assuming that the first character read must be a byte-order mark or
        /// a nonzero basic character (U+0001 to U+007F).</param>
        /// <param name='offset'>An index, starting at 0, showing where the
        /// desired portion of <paramref name='bytes'/> begins.</param>
        /// <param name='count'>The length, in bytes, of the desired portion of
        /// <paramref name='bytes'/> (but not more than <paramref
        /// name='bytes'/> 's length).</param>
        /// <returns>A CBOR object containing the JSON data decoded.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='bytes'/> is null.</exception>
        /// <exception cref='PeterO.Cbor.CBORException'>The byte array contains
        /// invalid encoding or is not in JSON format.</exception>
        /// <exception cref='ArgumentException'>Either <paramref
        /// name='offset'/> or <paramref name='count'/> is less than 0 or
        /// greater than <paramref name='bytes'/> 's length, or <paramref
        /// name='bytes'/> 's length minus <paramref name='offset'/> is less
        /// than <paramref name='count'/>.</exception>
        public static CBORObject FromJSONBytes(byte[] bytes, int offset, int
          count)
        {
            return FromJSONBytes(bytes, offset, count, JSONOptions.Default);
        }

        /// <summary>Generates a CBOR object from a byte array in JavaScript
        /// Object Notation (JSON) format, using the specified options to
        /// control the decoding process.
        /// <para>Note that if a CBOR object is converted to JSON with
        /// <c>ToJSONBytes</c>, then the JSON is converted back to CBOR with
        /// this method, the new CBOR object will not necessarily be the same
        /// as the old CBOR object, especially if the old CBOR object uses data
        /// types not supported in JSON, such as integers in map
        /// keys.</para></summary>
        /// <param name='bytes'>A byte array, the specified portion of which is
        /// in JSON format. The specified portion of the byte array must
        /// contain a single JSON object and not multiple objects. The portion
        /// may begin with a byte-order mark (U+FEFF). The portion can be in
        /// UTF-8, UTF-16, or UTF-32 encoding; the encoding is detected by
        /// assuming that the first character read must be a byte-order mark or
        /// a nonzero basic character (U+0001 to U+007F).</param>
        /// <param name='offset'>An index, starting at 0, showing where the
        /// desired portion of <paramref name='bytes'/> begins.</param>
        /// <param name='count'>The length, in bytes, of the desired portion of
        /// <paramref name='bytes'/> (but not more than <paramref
        /// name='bytes'/> 's length).</param>
        /// <param name='jsonoptions'>Specifies options to control how the JSON
        /// data is decoded to CBOR. See the JSONOptions class.</param>
        /// <returns>A CBOR object containing the JSON data decoded.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='bytes'/> or <paramref name='jsonoptions'/> is
        /// null.</exception>
        /// <exception cref='PeterO.Cbor.CBORException'>The byte array contains
        /// invalid encoding or is not in JSON format.</exception>
        /// <exception cref='ArgumentException'>Either <paramref
        /// name='offset'/> or <paramref name='count'/> is less than 0 or
        /// greater than <paramref name='bytes'/> 's length, or <paramref
        /// name='bytes'/> 's length minus <paramref name='offset'/> is less
        /// than <paramref name='count'/>.</exception>
        public static CBORObject FromJSONBytes(
          byte[] bytes,
          int offset,
          int count,
          JSONOptions jsonoptions)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }
            if (jsonoptions == null)
            {
                throw new ArgumentNullException(nameof(jsonoptions));
            }
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }
            if (offset < 0)
            {
                throw new ArgumentException("offset (" + offset + ") is not greater" +
                  "\u0020or equal to 0");
            }
            if (offset > bytes.Length)
            {
                throw new ArgumentException("offset (" + offset + ") is not less or" +
                  "\u0020equal to " + bytes.Length);
            }
            if (count < 0)
            {
                throw new ArgumentException("count (" + count + ") is not greater or" +
                  "\u0020equal to 0");
            }
            if (count > bytes.Length)
            {
                throw new ArgumentException("count (" + count + ") is not less or" +
                  "\u0020equal to " + bytes.Length);
            }
            if (bytes.Length - offset < count)
            {
                throw new ArgumentException("bytes's length minus " + offset + " (" +
                  (bytes.Length - offset) + ") is not greater or equal to " + count);
            }
            if (count == 0)
            {
                throw new CBORException("Byte array is empty");
            }
            if (bytes[offset] >= 0x01 && bytes[offset] <= 0x7f && count >= 2 &&
              bytes[offset + 1] != 0)
            {
                // UTF-8 JSON bytes
                return CBORJson2.ParseJSONValue(
                    bytes,
                    offset,
                    offset + count,
                    jsonoptions);
            }
            else
            {
                // Other than UTF-8 without byte order mark
                try
                {
                    using (var ms = new MemoryStream(bytes, offset, count))
                    {
                        return ReadJSON(ms, jsonoptions);
                    }
                }
                catch (IOException ex)
                {
                    throw new CBORException(ex.Message, ex);
                }
            }
        }

        /// <summary>Finds the remainder that results when a CBORObject object
        /// is divided by the value of a CBOR object.</summary>
        /// <param name='first'>The parameter <paramref name='first'/> is a
        /// CBOR object.</param>
        /// <param name='second'>The parameter <paramref name='second'/> is a
        /// CBOR object.</param>
        /// <returns>The remainder of the two numbers.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='first'/> or <paramref name='second'/> is null.</exception>
        [Obsolete("Instead, convert both CBOR objects to numbers (with" +
            "\u0020.AsNumber()), and use the first number's .Remainder() method.")]
        public static CBORObject Remainder(CBORObject first, CBORObject second)
        {
            if (first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }
            if (second == null)
            {
                throw new ArgumentNullException(nameof(second));
            }
            CBORNumber a = CBORNumber.FromCBORObject(first);
            if (a == null)
            {
                throw new ArgumentException(nameof(first) + "does not represent a" +
                  "\u0020number");
            }
            CBORNumber b = CBORNumber.FromCBORObject(second);
            if (b == null)
            {
                throw new ArgumentException(nameof(second) + "does not represent a" +
                  "\u0020number");
            }
            return a.Remainder(b).ToCBORObject();
        }

        /// <summary>Finds the difference between two CBOR number
        /// objects.</summary>
        /// <param name='first'>The parameter <paramref name='first'/> is a
        /// CBOR object.</param>
        /// <param name='second'>The parameter <paramref name='second'/> is a
        /// CBOR object.</param>
        /// <returns>The difference of the two objects.</returns>
        /// <exception cref='ArgumentException'>Either or both operands are not
        /// numbers (as opposed to Not-a-Number, NaN).</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='first'/> or <paramref name='second'/> is null.</exception>
        [Obsolete("Instead, convert both CBOR objects to numbers (with" +

            "\u0020.AsNumber()), and use the first number's .Subtract() method.")]
        public static CBORObject Subtract(CBORObject first, CBORObject second)
        {
            if (first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }
            if (second == null)
            {
                throw new ArgumentNullException(nameof(second));
            }
            CBORNumber a = CBORNumber.FromCBORObject(first);
            if (a == null)
            {
                throw new ArgumentException(nameof(first) + "does not represent a" +
                  "\u0020number");
            }
            CBORNumber b = CBORNumber.FromCBORObject(second);
            if (b == null)
            {
                throw new ArgumentException(nameof(second) + "does not represent a" +
                  "\u0020number");
            }
            return a.Subtract(b).ToCBORObject();
        }

        /// <summary>
        /// <para>Writes a text string in CBOR format to a data stream. The
        /// string will be encoded using definite-length encoding regardless of
        /// its length.</para></summary>
        /// <param name='str'>The string to write. Can be null.</param>
        /// <param name='stream'>A writable data stream.</param>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        public static void Write(string str, Stream stream)
        {
            Write(str, stream, CBOREncodeOptions.Default);
        }

        /// <summary>Writes a text string in CBOR format to a data stream,
        /// using the given options to control the encoding process.</summary>
        /// <param name='str'>The string to write. Can be null.</param>
        /// <param name='stream'>A writable data stream.</param>
        /// <param name='options'>Options for encoding the data to
        /// CBOR.</param>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        public static void Write(
          string str,
          Stream stream,
          CBOREncodeOptions options)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (str == null)
            {
                stream.WriteByte(0xf6); // Write null instead of string
            }
            else
            {
                if (!options.UseIndefLengthStrings || options.Ctap2Canonical)
                {
                    // NOTE: Length of a String object won't be higher than the maximum
                    // allowed for definite-length strings
                    long codePointLength = DataUtilities.GetUtf8Length(str, true);
                    WritePositiveInt64(3, codePointLength, stream);
                    DataUtilities.WriteUtf8(str, stream, true);
                }
                else
                {
                    WriteStreamedString(str, stream);
                }
            }
        }

        /// <summary>Writes a binary floating-point number in CBOR format to a
        /// data stream, as though it were converted to a CBOR object via
        /// CBORObject.FromObject(EFloat) and then written out.</summary>
        /// <param name='bignum'>An arbitrary-precision binary floating-point
        /// number. Can be null.</param>
        /// <param name='stream'>A writable data stream.</param>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        public static void Write(EFloat bignum, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (bignum == null)
            {
                stream.WriteByte(0xf6);
                return;
            }
            if ((bignum.IsZero && bignum.IsNegative) || bignum.IsInfinity() ||
              bignum.IsNaN())
            {
                Write(CBORObject.FromObject(bignum), stream);
                return;
            }
            EInteger exponent = bignum.Exponent;
            if (exponent.CanFitInInt64())
            {
                stream.WriteByte(0xc5); // tag 5
                stream.WriteByte(0x82); // array, length 2
            }
            else if (exponent.GetSignedBitLengthAsInt64() > 64)
            {
                stream.WriteByte(0xd9); // tag 265
                stream.WriteByte(0x01);
                stream.WriteByte(0x09);
                stream.WriteByte(0x82); // array, length 2
            }
            else
            {
                stream.WriteByte(0xc5); // tag 5
                stream.WriteByte(0x82); // array, length 2
            }
            Write(
              bignum.Exponent,
              stream);
            Write(bignum.Mantissa, stream);
        }

        /// <summary>Writes a rational number in CBOR format to a data stream,
        /// as though it were converted to a CBOR object via
        /// CBORObject.FromObject(ERational) and then written out.</summary>
        /// <param name='rational'>An arbitrary-precision rational number. Can
        /// be null.</param>
        /// <param name='stream'>A writable data stream.</param>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        public static void Write(ERational rational, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (rational == null)
            {
                stream.WriteByte(0xf6);
                return;
            }
            if (!rational.IsFinite || (rational.IsNegative && rational.IsZero))
            {
                Write(CBORObject.FromObject(rational), stream);
                return;
            }
            stream.WriteByte(0xd8); // tag 30
            stream.WriteByte(0x1e);
            stream.WriteByte(0x82); // array, length 2
            Write(rational.Numerator, stream);
            Write(
              rational.Denominator,
              stream);
        }

        /// <summary>Writes a decimal floating-point number in CBOR format to a
        /// data stream, as though it were converted to a CBOR object via
        /// CBORObject.FromObject(EDecimal) and then written out.</summary>
        /// <param name='bignum'>The arbitrary-precision decimal number to
        /// write. Can be null.</param>
        /// <param name='stream'>Stream to write to.</param>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        public static void Write(EDecimal bignum, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (bignum == null)
            {
                stream.WriteByte(0xf6);
                return;
            }
            if (!bignum.IsFinite || (bignum.IsNegative && bignum.IsZero))
            {
                Write(CBORObject.FromObject(bignum), stream);
                return;
            }
            EInteger exponent = bignum.Exponent;
            if (exponent.CanFitInInt64())
            {
                stream.WriteByte(0xc4); // tag 4
                stream.WriteByte(0x82); // array, length 2
            }
            else if (exponent.GetSignedBitLengthAsInt64() > 64)
            {
                stream.WriteByte(0xd9); // tag 264
                stream.WriteByte(0x01);
                stream.WriteByte(0x08);
                stream.WriteByte(0x82); // array, length 2
            }
            else
            {
                stream.WriteByte(0xc4); // tag 4
                stream.WriteByte(0x82); // array, length 2
            }
            Write(exponent, stream);
            Write(bignum.Mantissa, stream);
        }

        private static byte[] EIntegerBytes(EInteger ei)
        {
            if (ei.IsZero)
            {
                return new byte[] { 0 };
            }
            if (ei.Sign < 0)
            {
                ei = ei.Add(1).Negate();
            }
            byte[] bytes = ei.ToBytes(false);
            var index = 0;
            while (index < bytes.Length && bytes[index] == 0)
            {
                ++index;
            }
            if (index > 0)
            {
                var newBytes = new byte[bytes.Length - index];
                Array.Copy(bytes, index, newBytes, 0, newBytes.Length);
                return newBytes;
            }
            return bytes;
        }

        /// <summary>Writes a arbitrary-precision integer in CBOR format to a
        /// data stream.</summary>
        /// <param name='bigint'>Arbitrary-precision integer to write. Can be
        /// null.</param>
        /// <param name='stream'>A writable data stream.</param>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        public static void Write(EInteger bigint, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if ((object)bigint == (object)null)
            {
                stream.WriteByte(0xf6);
                return;
            }
            var datatype = 0;
            if (bigint.Sign < 0)
            {
                datatype = 1;
                bigint = bigint.Add(EInteger.One);
                bigint = -(EInteger)bigint;
            }
            if (bigint.CanFitInInt64())
            {
                // If the arbitrary-precision integer is representable as a long and in
                // major type 0 or 1, write that major type
                // instead of as a bignum
                WritePositiveInt64(datatype, bigint.ToInt64Checked(), stream);
            }
            else
            {
                // Get a byte array of the arbitrary-precision integer's value,
                // since shifting and doing AND operations is
                // slow with large EIntegers
                byte[] bytes = bigint.ToBytes(true);
                int byteCount = bytes.Length;
                while (byteCount > 0 && bytes[byteCount - 1] == 0)
                {
                    // Ignore trailing zero bytes
                    --byteCount;
                }
                if (byteCount != 0)
                {
                    int half = byteCount >> 1;
                    int right = byteCount - 1;
                    for (var i = 0; i < half; ++i, --right)
                    {
                        byte value = bytes[i];
                        bytes[i] = bytes[right];
                        bytes[right] = value;
                    }
                }
                switch (byteCount)
                {
                    case 0:
                        stream.WriteByte((byte)(datatype << 5));
                        return;
                    case 1:
                        WritePositiveInt(datatype, ((int)bytes[0]) & 0xff, stream);
                        break;
                    case 2:
                        stream.WriteByte((byte)((datatype << 5) | 25));
                        stream.Write(bytes, 0, byteCount);
                        break;
                    case 3:
                        stream.WriteByte((byte)((datatype << 5) | 26));
                        stream.WriteByte((byte)0);
                        stream.Write(bytes, 0, byteCount);
                        break;
                    case 4:
                        stream.WriteByte((byte)((datatype << 5) | 26));
                        stream.Write(bytes, 0, byteCount);
                        break;
                    case 5:
                        stream.WriteByte((byte)((datatype << 5) | 27));
                        stream.WriteByte((byte)0);
                        stream.WriteByte((byte)0);
                        stream.WriteByte((byte)0);
                        stream.Write(bytes, 0, byteCount);
                        break;
                    case 6:
                        stream.WriteByte((byte)((datatype << 5) | 27));
                        stream.WriteByte((byte)0);
                        stream.WriteByte((byte)0);
                        stream.Write(bytes, 0, byteCount);
                        break;
                    case 7:
                        stream.WriteByte((byte)((datatype << 5) | 27));
                        stream.WriteByte((byte)0);
                        stream.Write(bytes, 0, byteCount);
                        break;
                    case 8:
                        stream.WriteByte((byte)((datatype << 5) | 27));
                        stream.Write(bytes, 0, byteCount);
                        break;
                    default:
                        stream.WriteByte((datatype == 0) ?
                   (byte)0xc2 : (byte)0xc3);
                        WritePositiveInt(2, byteCount, stream);
                        stream.Write(bytes, 0, byteCount);
                        break;
                }
            }
        }

        /// <summary>Writes a 64-bit signed integer in CBOR format to a data
        /// stream.</summary>
        /// <param name='value'>The value to write.</param>
        /// <param name='stream'>A writable data stream.</param>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        public static void Write(long value, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (value >= 0)
            {
                WritePositiveInt64(0, value, stream);
            }
            else
            {
                ++value;
                value = -value; // Will never overflow
                WritePositiveInt64(1, value, stream);
            }
        }

        /// <summary>Writes a 32-bit signed integer in CBOR format to a data
        /// stream.</summary>
        /// <param name='value'>The value to write.</param>
        /// <param name='stream'>A writable data stream.</param>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        public static void Write(int value, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            var type = 0;
            if (value < 0)
            {
                ++value;
                value = -value;
                type = 0x20;
            }
            if (value < 24)
            {
                stream.WriteByte((byte)(value | type));
            }
            else if (value <= 0xff)
            {
                byte[] bytes = { (byte)(24 | type), (byte)(value & 0xff) };
                stream.Write(bytes, 0, 2);
            }
            else if (value <= 0xffff)
            {
                byte[] bytes = {
          (byte)(25 | type), (byte)((value >> 8) & 0xff),
          (byte)(value & 0xff),
        };
                stream.Write(bytes, 0, 3);
            }
            else
            {
                byte[] bytes = {
          (byte)(26 | type), (byte)((value >> 24) & 0xff),
          (byte)((value >> 16) & 0xff), (byte)((value >> 8) & 0xff),
          (byte)(value & 0xff),
        };
                stream.Write(bytes, 0, 5);
            }
        }

        /// <summary>Writes a 16-bit signed integer in CBOR format to a data
        /// stream.</summary>
        /// <param name='value'>The value to write.</param>
        /// <param name='stream'>A writable data stream.</param>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        public static void Write(short value, Stream stream)
        {
            Write((long)value, stream);
        }

        /// <summary>Writes a Boolean value in CBOR format to a data
        /// stream.</summary>
        /// <param name='value'>The value to write.</param>
        /// <param name='stream'>A writable data stream.</param>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        public static void Write(bool value, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            stream.WriteByte(value ? (byte)0xf5 : (byte)0xf4);
        }

        /// <summary>Writes a byte (0 to 255) in CBOR format to a data stream.
        /// If the value is less than 24, writes that byte. If the value is 25
        /// to 255, writes the byte 24, then this byte's value.</summary>
        /// <param name='value'>The value to write.</param>
        /// <param name='stream'>A writable data stream.</param>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        public static void Write(byte value, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if ((((int)value) & 0xff) < 24)
            {
                stream.WriteByte(value);
            }
            else
            {
                stream.WriteByte((byte)24);
                stream.WriteByte(value);
            }
        }

        /// <summary>Writes a 32-bit floating-point number in CBOR format to a
        /// data stream. The number is written using the shortest
        /// floating-point encoding possible; this is a change from previous
        /// versions.</summary>
        /// <param name='value'>The value to write.</param>
        /// <param name='stream'>A writable data stream.</param>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        public static void Write(float value, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            WriteFloatingPointBits(
              stream,
              CBORUtilities.SingleToInt32Bits(value),
              4,
              true);
        }

        /// <summary>Writes a 64-bit floating-point number in CBOR format to a
        /// data stream. The number is written using the shortest
        /// floating-point encoding possible; this is a change from previous
        /// versions.</summary>
        /// <param name='value'>The value to write.</param>
        /// <param name='stream'>A writable data stream.</param>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        public static void Write(double value, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            WriteFloatingPointBits(
              stream,
              CBORUtilities.DoubleToInt64Bits(value),
              8,
              true);
        }

        /// <summary>Writes a CBOR object to a CBOR data stream.</summary>
        /// <param name='value'>The value to write. Can be null.</param>
        /// <param name='stream'>A writable data stream.</param>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        public static void Write(CBORObject value, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (value == null)
            {
                stream.WriteByte(0xf6);
            }
            else
            {
                value.WriteTo(stream);
            }
        }

        /// <summary>
        /// <para>Writes a CBOR object to a CBOR data stream. See the
        /// three-parameter Write method that takes a
        /// CBOREncodeOptions.</para></summary>
        /// <param name='objValue'>The arbitrary object to be serialized. Can
        /// be null.</param>
        /// <param name='stream'>A writable data stream.</param>
        public static void Write(object objValue, Stream stream)
        {
            Write(objValue, stream, CBOREncodeOptions.Default);
        }

        /// <summary>Writes an arbitrary object to a CBOR data stream, using
        /// the specified options for controlling how the object is encoded to
        /// CBOR data format. If the object is convertible to a CBOR map or a
        /// CBOR object that contains CBOR maps, the keys to those maps are
        /// written out to the data stream in an undefined order. The example
        /// code given in
        /// <see cref='PeterO.Cbor.CBORObject.WriteTo(System.IO.Stream)'/> can
        /// be used to write out certain keys of a CBOR map in a given order.
        /// Currently, the following objects are supported:
        /// <list type=''>
        /// <item>Lists of CBORObject.</item>
        /// <item>Maps of CBORObject. The keys to the map are written out to
        /// the data stream in an undefined order.</item>
        /// <item>Null.</item>
        /// <item>Byte arrays, which will always be written as definite-length
        /// byte strings.</item>
        /// <item>String objects. The strings will be encoded using
        /// definite-length encoding regardless of their length.</item>
        /// <item>Any object accepted by the FromObject static
        /// methods.</item></list></summary>
        /// <param name='objValue'>The arbitrary object to be serialized. Can
        /// be null.
        /// <para><b>NOTE:</b> For security reasons, whenever possible, an
        /// application should not base this parameter on user input or other
        /// externally supplied data unless the application limits this
        /// parameter's inputs to types specially handled by this method (such
        /// as <c>int</c> or <c>String</c> ) and/or to plain-old-data types
        /// (POCO or POJO types) within the control of the application. If the
        /// plain-old-data type references other data types, those types should
        /// likewise meet either criterion above.</para>.</param>
        /// <param name='output'>A writable data stream.</param>
        /// <param name='options'>CBOR options for encoding the CBOR object to
        /// bytes.</param>
        /// <exception cref='ArgumentException'>The object's type is not
        /// supported.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='options'/> or <paramref name='output'/> is null.</exception>
        public static void Write(
          object objValue,
          Stream output,
          CBOREncodeOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }
            if (objValue == null)
            {
                output.WriteByte(0xf6);
                return;
            }
            if (options.Ctap2Canonical)
            {
                FromObject(objValue).WriteTo(output, options);
                return;
            }
            byte[] data = objValue as byte[];
            if (data != null)
            {
                WritePositiveInt(3, data.Length, output);
                output.Write(data, 0, data.Length);
                return;
            }
            if (objValue is IList<CBORObject>)
            {
                WriteObjectArray(
                  (IList<CBORObject>)objValue,
                  output,
                  options);
                return;
            }
            if (objValue is IDictionary<CBORObject, CBORObject>)
            {
                WriteObjectMap(
                  (IDictionary<CBORObject, CBORObject>)objValue,
                  output,
                  options);
                return;
            }
            FromObject(objValue).WriteTo(output, options);
        }

        /// <summary>Converts an arbitrary object to a text string in
        /// JavaScript Object Notation (JSON) format, as in the ToJSONString
        /// method, and writes that string to a data stream in UTF-8. If the
        /// object is convertible to a CBOR map, or to a CBOR object that
        /// contains CBOR maps, the keys to those maps are written out to the
        /// JSON string in an undefined order. The example code given in
        /// <b>PeterO.Cbor.CBORObject.ToJSONString(PeterO.Cbor.JSONOptions)</b>
        /// can be used to write out certain keys of a CBOR map in a given
        /// order to a JSON string.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object. Can be null.
        /// <para><b>NOTE:</b> For security reasons, whenever possible, an
        /// application should not base this parameter on user input or other
        /// externally supplied data unless the application limits this
        /// parameter's inputs to types specially handled by this method (such
        /// as <c>int</c> or <c>String</c> ) and/or to plain-old-data types
        /// (POCO or POJO types) within the control of the application. If the
        /// plain-old-data type references other data types, those types should
        /// likewise meet either criterion above.</para>.</param>
        /// <param name='outputStream'>A writable data stream.</param>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='outputStream'/> is null.</exception>
        public static void WriteJSON(object obj, Stream outputStream)
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }
            if (obj == null)
            {
                outputStream.Write(ValueNullBytes, 0, ValueNullBytes.Length);
                return;
            }
            if (obj is bool)
            {
                if ((bool)obj)
                {
                    outputStream.Write(ValueTrueBytes, 0, ValueTrueBytes.Length);
                    return;
                }
                outputStream.Write(ValueFalseBytes, 0, ValueFalseBytes.Length);
                return;
            }
            CBORObject.FromObject(obj).WriteJSONTo(outputStream);
        }

        /// <summary>Gets this object's absolute value.</summary>
        /// <returns>This object's absolute without its negative
        /// sign.</returns>
        /// <exception cref='InvalidOperationException'>This object does not
        /// represent a number (for this purpose, infinities and not-a-number
        /// or NaN values, but not CBORObject.Null, are considered
        /// numbers).</exception>
        [Obsolete("Instead, convert this object to a number \u0028with" +
            "\u0020.AsNumber\u0028)), and use that number's .Abs\u0028) method.")]
        public CBORObject Abs()
        {
            CBORNumber cn = CBORNumber.FromCBORObject(this);
            if (cn == null)
            {
                throw new InvalidOperationException("This object is not a number.");
            }
            object oldItem = cn.GetValue();
            object newItem = cn.GetNumberInterface().Abs(oldItem);
            if (oldItem == newItem)
            {
                return this;
            }
            if (newItem is EDecimal)
            {
                return CBORObject.FromObject((EDecimal)newItem);
            }
            if (newItem is EInteger)
            {
                return CBORObject.FromObject((EInteger)newItem);
            }
            if (newItem is EFloat)
            {
                return CBORObject.FromObject((EFloat)newItem);
            }
            var rat = newItem as ERational;
            return (rat != null) ? CBORObject.FromObject(rat) : ((oldItem ==
                  newItem) ? this : CBORObject.FromObject(newItem));
        }

        /// <summary>
        /// <para>Adds a new key and its value to this CBOR map, or adds the
        /// value if the key doesn't exist.</para>
        /// <para>NOTE: This method can't be used to add a tag to an existing
        /// CBOR object. To create a CBOR object with a given tag, call the
        /// <c>CBORObject.FromObjectAndTag</c> method and pass the CBOR object
        /// and the desired tag number to that method.</para></summary>
        /// <param name='key'>An object representing the key, which will be
        /// converted to a CBORObject. Can be null, in which case this value is
        /// converted to CBORObject.Null.</param>
        /// <param name='valueOb'>An object representing the value, which will
        /// be converted to a CBORObject. Can be null, in which case this value
        /// is converted to CBORObject.Null.</param>
        /// <returns>This instance.</returns>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='key'/> already exists in this map.</exception>
        /// <exception cref='InvalidOperationException'>This object is not a
        /// map.</exception>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='key'/> or <paramref name='valueOb'/> has an unsupported
        /// type.</exception>
        public CBORObject Add(object key, object valueOb)
        {
            if (this.Type == CBORType.Map)
            {
                CBORObject mapKey;
                CBORObject mapValue;
                if (key == null)
                {
                    mapKey = CBORObject.Null;
                }
                else
                {
                    mapKey = key as CBORObject;
                    mapKey = mapKey ?? CBORObject.FromObject(key);
                }
                if (valueOb == null)
                {
                    mapValue = CBORObject.Null;
                }
                else
                {
                    mapValue = valueOb as CBORObject;
                    mapValue = mapValue ?? CBORObject.FromObject(valueOb);
                }
                IDictionary<CBORObject, CBORObject> map = this.AsMap();
                if (map.ContainsKey(mapKey))
                {
                    throw new ArgumentException("Key already exists");
                }
                map.Add(
                  mapKey,
                  mapValue);
            }
            else
            {
                throw new InvalidOperationException("Not a map");
            }
            return this;
        }

        /// <summary><para>Adds a new object to the end of this array. (Used to
        /// throw ArgumentNullException on a null reference, but now converts
        /// the null reference to CBORObject.Null, for convenience with the
        /// Object overload of this method).</para>
        ///  <para>NOTE: This method
        /// can't be used to add a tag to an existing CBOR object. To create a
        /// CBOR object with a given tag, call the
        /// <c>CBORObject.FromObjectAndTag</c>
        ///  method and pass the CBOR object
        /// and the desired tag number to that method.</para>
        ///  </summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is a CBOR
        /// object.</param>
        /// <returns>This instance.</returns>
        /// <exception cref='InvalidOperationException'>This object is not an
        /// array.</exception>
        /// <example>
        /// <para>The following example creates a CBOR array and adds several
        /// CBOR objects, one of which has a custom CBOR tag, to that array.
        /// Note the chaining behavior made possible by this method.</para>
        /// <code>CBORObject obj = CBORObject.NewArray() .Add(CBORObject.False)
        /// .Add(CBORObject.FromObject(5)) .Add(CBORObject.FromObject("text
        /// string")) .Add(CBORObject.FromObjectAndTag(9999, 1));</code>
        ///  .
        /// </example>
        public CBORObject Add(CBORObject obj)
        {
            if (this.Type == CBORType.Array)
            {
                IList<CBORObject> list = this.AsList();
                list.Add(obj);
                return this;
            }
            throw new InvalidOperationException("Not an array");
        }

        /// <summary><para>Converts an object to a CBOR object and adds it to
        /// the end of this array.</para>
        ///  <para>NOTE: This method can't be used
        /// to add a tag to an existing CBOR object. To create a CBOR object
        /// with a given tag, call the <c>CBORObject.FromObjectAndTag</c>
        /// method and pass the CBOR object and the desired tag number to that
        /// method.</para>
        ///  </summary>
        /// <param name='obj'>A CBOR object (or an object convertible to a CBOR
        /// object) to add to this CBOR array.</param>
        /// <returns>This instance.</returns>
        /// <exception cref='InvalidOperationException'>This instance is not an
        /// array.</exception>
        /// <exception cref='ArgumentException'>The type of <paramref
        /// name='obj'/> is not supported.</exception>
        /// <example>
        /// <para>The following example creates a CBOR array and adds several
        /// CBOR objects, one of which has a custom CBOR tag, to that array.
        /// Note the chaining behavior made possible by this method.</para>
        /// <code>CBORObject obj = CBORObject.NewArray() .Add(CBORObject.False) .Add(5)
        /// .Add("text string") .Add(CBORObject.FromObjectAndTag(9999, 1));</code>
        ///  .
        /// </example>
        public CBORObject Add(object obj)
        {
            if (this.Type == CBORType.Array)
            {
                IList<CBORObject> list = this.AsList();
                list.Add(CBORObject.FromObject(obj));
                return this;
            }
            throw new InvalidOperationException("Not an array");
        }

        /// <summary>Converts this object to an arbitrary-precision integer.
        /// See the ToObject overload taking a type for more
        /// information.</summary>
        /// <returns>The closest arbitrary-precision integer to this
        /// object.</returns>
        /// <exception cref='InvalidOperationException'>This object does not
        /// represent a number (for the purposes of this method, infinity and
        /// not-a-number values, but not <c>CBORObject.Null</c>, are
        /// considered numbers).</exception>
        /// <exception cref='OverflowException'>This object's value is infinity
        /// or not-a-number (NaN).</exception>
        [Obsolete("Instead, use " + ".ToObject<PeterO.Numbers.EInteger>\u0028) in" +
            "\u0020.NET" +
            " or \u0020.ToObject\u0028com.upokecenter.numbers.EInteger.class) in" +
            "\u0020Java.")]
        public EInteger AsEInteger()
        {
            return (EInteger)this.ToObject(typeof(EInteger));
        }

        /// <summary>Returns false if this object is a CBOR false, null, or
        /// undefined value (whether or not the object has tags); otherwise,
        /// true.</summary>
        /// <returns>False if this object is a CBOR false, null, or undefined
        /// value; otherwise, true.</returns>
        public bool AsBoolean()
        {
            return !this.IsFalse && !this.IsNull && !this.IsUndefined;
        }

        /// <summary>Converts this object to a byte (0 to 255). Floating point
        /// values are converted to integers by discarding their fractional
        /// parts.</summary>
        /// <returns>The closest byte-sized integer to this object.</returns>
        /// <exception cref='InvalidOperationException'>This object does not
        /// represent a number (for this purpose, infinities and not-a-number
        /// or NaN values, but not CBORObject.Null, are considered
        /// numbers).</exception>
        /// <exception cref='OverflowException'>This object's value exceeds the
        /// range of a byte (would be less than 0 or greater than 255 when
        /// converted to an integer by discarding its fractional
        /// part).</exception>
        [Obsolete("Instead, use " + ".ToObject<byte>\u0028) in" +
            "\u0020.NET" + " or \u0020.ToObject\u0028Byte.class) in" +
            "\u0020Java.")]
        public byte AsByte()
        {
            return (byte)this.AsInt32(0, 255);
        }

        internal byte AsByteLegacy()
        {
            return (byte)this.AsInt32(0, 255);
        }

        /// <summary>Converts this object to a 64-bit floating point
        /// number.</summary>
        /// <returns>The closest 64-bit floating point number to this object.
        /// The return value can be positive infinity or negative infinity if
        /// this value exceeds the range of a 64-bit floating point
        /// number.</returns>
        /// <exception cref='InvalidOperationException'>This object does not
        /// represent a number (for this purpose, infinities and not-a-number
        /// or NaN values, but not CBORObject.Null, are considered
        /// numbers).</exception>
        public double AsDouble()
        {
            CBORNumber cn = CBORNumber.FromCBORObject(this);
            if (cn == null)
            {
                throw new InvalidOperationException("Not a number type");
            }
            return cn.GetNumberInterface().AsDouble(cn.GetValue());
        }

        /// <summary>Converts this object to a decimal number.</summary>
        /// <returns>A decimal number for this object's value.</returns>
        /// <exception cref='InvalidOperationException'>This object does not
        /// represent a number (for the purposes of this method, infinity and
        /// not-a-number values, but not <c>CBORObject.Null</c>, are
        /// considered numbers).</exception>
        [Obsolete("Instead, use " + ".ToObject<PeterO.Numbers.EDecimal>\u0028) in" +

            "\u0020.NET" +
            " or \u0020.ToObject\u0028com.upokecenter.numbers.EDecimal.class) in" +
            "\u0020Java.")]
        public EDecimal AsEDecimal()
        {
            return (EDecimal)this.ToObject(typeof(EDecimal));
        }

        /// <summary>Converts this object to an arbitrary-precision binary
        /// floating point number. See the ToObject overload taking a type for
        /// more information.</summary>
        /// <returns>An arbitrary-precision binary floating-point number for
        /// this object's value.</returns>
        /// <exception cref='InvalidOperationException'>This object does not
        /// represent a number (for the purposes of this method, infinity and
        /// not-a-number values, but not <c>CBORObject.Null</c>, are
        /// considered numbers).</exception>
        [Obsolete("Instead, use " +
            ".ToObject<PeterO.Numbers.EFloat>\u0028) in .NET" +

            " or \u0020.ToObject\u0028com.upokecenter.numbers.EFloat.class) in" +
            "\u0020Java.")]
        public EFloat AsEFloat()
        {
            return (EFloat)this.ToObject(typeof(EFloat));
        }

        /// <summary>Converts this object to a rational number. See the
        /// ToObject overload taking a type for more information.</summary>
        /// <returns>A rational number for this object's value.</returns>
        /// <exception cref='InvalidOperationException'>This object does not
        /// represent a number (for the purposes of this method, infinity and
        /// not-a-number values, but not <c>CBORObject.Null</c>, are
        /// considered numbers).</exception>
        [Obsolete("Instead, use " + ".ToObject<PeterO.Numbers.ERational>" +
            "\u0028) in .NET" +
            "\u0020or .ToObject\u0028com.upokecenter.numbers.ERational.class) in" +
            "\u0020Java.")]
        public ERational AsERational()
        {
            return (ERational)this.ToObject(typeof(ERational));
        }

        /// <summary>Converts this object to a 16-bit signed integer. Floating
        /// point values are converted to integers by discarding their
        /// fractional parts.</summary>
        /// <returns>The closest 16-bit signed integer to this
        /// object.</returns>
        /// <exception cref='InvalidOperationException'>This object does not
        /// represent a number (for this purpose, infinities and not-a-number
        /// or NaN values, but not CBORObject.Null, are considered
        /// numbers).</exception>
        /// <exception cref='OverflowException'>This object's value exceeds the
        /// range of a 16-bit signed integer.</exception>
        [Obsolete("Instead, use the following:" +
            "\u0020\u0028cbor.AsNumber().ToInt16Checked()), or" +
            "\u0020.ToObject<short>() in" + "\u0020.NET.")]
        public short AsInt16()
        {
            return (short)this.AsInt32(Int16.MinValue, Int16.MaxValue);
        }

        /// <summary>Converts this object to a 32-bit signed integer if this
        /// CBOR object's type is Integer. This method disregards the tags this
        /// object has, if any.</summary>
        /// <returns>The 32-bit signed integer stored by this object.</returns>
        /// <exception cref='InvalidOperationException'>This object's type is
        /// not <c>CBORType.Integer</c>
        /// .</exception>
        /// <exception cref='OverflowException'>This object's value exceeds the
        /// range of a 32-bit signed integer.</exception>
        /// <example>
        /// <para>The following example code (originally written in C# for
        /// the.NET Framework) shows a way to check whether a given CBOR object
        /// stores a 32-bit signed integer before getting its value.</para>
        /// <code>CBORObject obj = CBORObject.FromInt32(99999);
        /// if (obj.CanValueFitInInt32()) { /* Not an Int32;
        /// handle the error */ Console.WriteLine("Not a 32-bit integer."); } else {
        /// Console.WriteLine("The value is " + obj.AsInt32Value()); }</code>
        ///  .
        /// </example>
        public int AsInt32Value()
        {
            switch (this.ItemType)
            {
                case CBORObjectTypeInteger:
                    {
                        var longValue = (long)this.ThisItem;
                        if (longValue < Int32.MinValue || longValue > Int32.MaxValue)
                        {
                            throw new OverflowException();
                        }
                        return checked((int)longValue);
                    }
                case CBORObjectTypeEInteger:
                    {
                        var ei = (EInteger)this.ThisItem;
                        return ei.ToInt32Checked();
                    }
                default: throw new InvalidOperationException("Not an integer type");
            }
        }

        /// <summary>Converts this object to a 64-bit signed integer if this
        /// CBOR object's type is Integer. This method disregards the tags this
        /// object has, if any.</summary>
        /// <returns>The 64-bit signed integer stored by this object.</returns>
        /// <exception cref='InvalidOperationException'>This object's type is
        /// not <c>CBORType.Integer</c>
        /// .</exception>
        /// <exception cref='OverflowException'>This object's value exceeds the
        /// range of a 64-bit signed integer.</exception>
        /// <example>
        /// <para>The following example code (originally written in C# for
        /// the.NET Framework) shows a way to check whether a given CBOR object
        /// stores a 64-bit signed integer before getting its value.</para>
        /// <code>CBORObject obj = CBORObject.FromInt64(99999);
        /// if (obj.CanValueFitInInt64()) {
        /// &#x2f;&#x2a; Not an Int64; handle the error&#x2a;&#x2f;
        /// Console.WriteLine("Not a 64-bit integer."); } else {
        /// Console.WriteLine("The value is " + obj.AsInt64Value()); }</code>
        ///  .
        /// </example>
        public long AsInt64Value()
        {
            switch (this.ItemType)
            {
                case CBORObjectTypeInteger:
                    return (long)this.ThisItem;
                case CBORObjectTypeEInteger:
                    {
                        var ei = (EInteger)this.ThisItem;
                        return ei.ToInt64Checked();
                    }
                default: throw new InvalidOperationException("Not an integer type");
            }
        }

        /// <summary>Returns whether this CBOR object stores an integer
        /// (CBORType.Integer) within the range of a 64-bit signed integer.
        /// This method disregards the tags this object has, if any.</summary>
        /// <returns><c>true</c> if this CBOR object stores an integer
        /// (CBORType.Integer) whose value is at least -(2^63) and less than
        /// 2^63; otherwise, <c>false</c>.</returns>
        public bool CanValueFitInInt64()
        {
            switch (this.ItemType)
            {
                case CBORObjectTypeInteger:
                    return true;
                case CBORObjectTypeEInteger:
                    {
                        var ei = (EInteger)this.ThisItem;
                        return ei.CanFitInInt64();
                    }
                default: return false;
            }
        }

        /// <summary>Returns whether this CBOR object stores an integer
        /// (CBORType.Integer) within the range of a 32-bit signed integer.
        /// This method disregards the tags this object has, if any.</summary>
        /// <returns><c>true</c> if this CBOR object stores an integer
        /// (CBORType.Integer) whose value is at least -(2^31) and less than
        /// 2^31; otherwise, <c>false</c>.</returns>
        public bool CanValueFitInInt32()
        {
            switch (this.ItemType)
            {
                case CBORObjectTypeInteger:
                    {
                        var elong = (long)this.ThisItem;
                        return elong >= Int32.MinValue && elong <= Int32.MaxValue;
                    }
                case CBORObjectTypeEInteger:
                    {
                        var ei = (EInteger)this.ThisItem;
                        return ei.CanFitInInt32();
                    }
                default:
                    return false;
            }
        }

        /// <summary>Converts this object to an arbitrary-precision integer if
        /// this CBOR object's type is Integer. This method disregards the tags
        /// this object has, if any. (Note that CBOR stores untagged integers
        /// at least -(2^64) and less than 2^64.).</summary>
        /// <returns>The integer stored by this object.</returns>
        /// <exception cref='InvalidOperationException'>This object's type is
        /// not <c>CBORType.Integer</c>.</exception>
        public EInteger AsEIntegerValue()
        {
            switch (this.ItemType)
            {
                case CBORObjectTypeInteger:
                    return EInteger.FromInt64((long)this.ThisItem);
                case CBORObjectTypeEInteger:
                    return (EInteger)this.ThisItem;
                default: throw new InvalidOperationException("Not an integer type");
            }
        }

        /// <summary>Converts this object to the bits of a 64-bit
        /// floating-point number if this CBOR object's type is FloatingPoint.
        /// This method disregards the tags this object has, if any.</summary>
        /// <returns>The bits of a 64-bit floating-point number stored by this
        /// object. The most significant bit is the sign (set means negative,
        /// clear means nonnegative); the next most significant 11 bits are the
        /// exponent area; and the remaining bits are the significand area. If
        /// all the bits of the exponent area are set and the significand area
        /// is 0, this indicates infinity. If all the bits of the exponent area
        /// are set and the significand area is other than 0, this indicates
        /// not-a-number (NaN).</returns>
        /// <exception cref='InvalidOperationException'>This object's type is
        /// not <c>CBORType.FloatingPoint</c>.</exception>
        public long AsDoubleBits()
        {
            switch (this.Type)
            {
                case CBORType.FloatingPoint:
                    return (long)this.ThisItem;
                default:
                    throw new InvalidOperationException("Not a floating-point" +
               "\u0020type");
            }
        }

        /// <summary>Converts this object to a 64-bit floating-point number if
        /// this CBOR object's type is FloatingPoint. This method disregards
        /// the tags this object has, if any.</summary>
        /// <returns>The 64-bit floating-point number stored by this
        /// object.</returns>
        /// <exception cref='InvalidOperationException'>This object's type is
        /// not <c>CBORType.FloatingPoint</c>.</exception>
        public double AsDoubleValue()
        {
            switch (this.Type)
            {
                case CBORType.FloatingPoint:
                    return CBORUtilities.Int64BitsToDouble((long)this.ThisItem);
                default:
                    throw new InvalidOperationException("Not a floating-point" +
               "\u0020type");
            }
        }

        /// <summary>Converts this object to a CBOR number. (NOTE: To determine
        /// whether this method call can succeed, call the <b>IsNumber</b>
        /// property (isNumber() method in Java) before calling this
        /// method.).</summary>
        /// <returns>The number represented by this object.</returns>
        /// <exception cref='InvalidOperationException'>This object does not
        /// represent a number (for this purpose, infinities and not-a-number
        /// or NaN values, but not CBORObject.Null, are considered
        /// numbers).</exception>
        public CBORNumber AsNumber()
        {
            CBORNumber num = CBORNumber.FromCBORObject(this);
            if (num == null)
            {
                throw new InvalidOperationException("Not a number type");
            }
            return num;
        }

        /// <summary>Converts this object to a 32-bit signed integer.
        /// Non-integer number values are converted to integers by discarding
        /// their fractional parts. (NOTE: To determine whether this method
        /// call can succeed, call <b>AsNumber().CanTruncatedIntFitInInt32</b>
        /// before calling this method. See the example.).</summary>
        /// <returns>The closest 32-bit signed integer to this
        /// object.</returns>
        /// <exception cref='InvalidOperationException'>This object does not
        /// represent a number (for this purpose, infinities and not-a-number
        /// or NaN values, but not CBORObject.Null, are considered
        /// numbers).</exception>
        /// <exception cref='OverflowException'>This object's value exceeds the
        /// range of a 32-bit signed integer.</exception>
        /// <example>
        /// <para>The following example code (originally written in C# for
        /// the.NET Framework) shows a way to check whether a given CBOR object
        /// stores a 32-bit signed integer before getting its value.</para>
        /// <code>CBORObject obj = CBORObject.FromInt32(99999);
        /// if (obj.AsNumber().CanTruncatedIntFitInInt32()) {
        /// &#x2f;&#x2a; Not an Int32; handle the error &#x2a;&#x2f;
        /// Console.WriteLine("Not a 32-bit integer."); } else {
        /// Console.WriteLine("The value is " + obj.AsInt32()); }</code>
        ///  .
        /// </example>
        public int AsInt32()
        {
            return this.AsInt32(Int32.MinValue, Int32.MaxValue);
        }

        /// <summary>Converts this object to a 64-bit signed integer.
        /// Non-integer numbers are converted to integers by discarding their
        /// fractional parts. (NOTE: To determine whether this method call can
        /// succeed, call <b>AsNumber().CanTruncatedIntFitInInt64</b>
        ///  before
        /// calling this method. See the example.).</summary>
        /// <returns>The closest 64-bit signed integer to this
        /// object.</returns>
        /// <exception cref='InvalidOperationException'>This object does not
        /// represent a number (for this purpose, infinities and not-a-number
        /// or NaN values, but not CBORObject.Null, are considered
        /// numbers).</exception>
        /// <exception cref='OverflowException'>This object's value exceeds the
        /// range of a 64-bit signed integer.</exception>
        /// <example>
        /// <para>The following example code (originally written in C# for
        /// the.NET Framework) shows a way to check whether a given CBOR object
        /// stores a 64-bit signed integer before getting its value.</para>
        /// <code>CBORObject obj = CBORObject.FromInt64(99999);
        /// if (obj.IsIntegral &amp;&amp; obj.AsNumber().CanFitInInt64()) {
        /// &#x2f;&#x2a; Not an Int64; handle the error &#x2a;&#x2f;
        /// Console.WriteLine("Not a 64-bit integer."); } else {
        /// Console.WriteLine("The value is " + obj.AsInt64()); }</code>
        ///  .
        /// </example>
        [Obsolete("Instead, use the following:" +
            "\u0020\u0028cbor.AsNumber().ToInt64Checked()), or .ToObject<long>()" +
            "\u0020in .NET.")]
        public long AsInt64()
        {
            CBORNumber cn = this.AsNumber();
            return cn.GetNumberInterface().AsInt64(cn.GetValue());
        }

        /// <summary>Converts this object to a 32-bit floating point
        /// number.</summary>
        /// <returns>The closest 32-bit floating point number to this object.
        /// The return value can be positive infinity or negative infinity if
        /// this object's value exceeds the range of a 32-bit floating point
        /// number.</returns>
        /// <exception cref='InvalidOperationException'>This object does not
        /// represent a number (for this purpose, infinities and not-a-number
        /// or NaN values, but not CBORObject.Null, are considered
        /// numbers).</exception>
        public float AsSingle()
        {
            CBORNumber cn = this.AsNumber();
            return cn.GetNumberInterface().AsSingle(cn.GetValue());
        }

        /// <summary>Gets the value of this object as a text string.</summary>
        /// <returns>Gets this object's string.</returns>
        /// <exception cref='InvalidOperationException'>This object's type is
        /// not a text string (for the purposes of this method, infinity and
        /// not-a-number values, but not <c>CBORObject.Null</c>, are
        /// considered numbers). To check the CBOR object for null before
        /// conversion, use the following idiom (originally written in C# for
        /// the.NET version): <c>(cbor == null || cbor.IsNull) ? null :
        /// cbor.AsString()</c>.</exception>
        public string AsString()
        {
            int type = this.ItemType;
            switch (type)
            {
                case CBORObjectTypeTextString:
                    {
                        return (string)this.ThisItem;
                    }
                case CBORObjectTypeTextStringUtf8:
                    {
                        return DataUtilities.GetUtf8String((byte[])this.ThisItem, false);
                    }
                default:
                    throw new InvalidOperationException("Not a text string type");
            }
        }

        /// <summary>Returns whether this object's value can be converted to a
        /// 64-bit floating point number without its value being rounded to
        /// another numerical value.</summary>
        /// <returns><c>true</c> if this object's value can be converted to a
        /// 64-bit floating point number without its value being rounded to
        /// another numerical value, or if this is a not-a-number value, even
        /// if the value's diagnostic information can't fit in a 64-bit
        /// floating point number; otherwise, <c>false</c>.</returns>
        [Obsolete("Instead, use the following: \u0028cbor.IsNumber &&" +
            "\u0020cbor.AsNumber().CanFitInDouble()).")]
        public bool CanFitInDouble()
        {
            CBORNumber cn = CBORNumber.FromCBORObject(this);
            return (cn != null) &&
              cn.GetNumberInterface().CanFitInDouble(cn.GetValue());
        }

        /// <summary>Returns whether this object's numerical value is an
        /// integer, is -(2^31) or greater, and is less than 2^31.</summary>
        /// <returns><c>true</c> if this object's numerical value is an
        /// integer, is -(2^31) or greater, and is less than 2^31; otherwise,
        /// <c>false</c>.</returns>
        [Obsolete("Instead, use " +
            ".CanValueFitInInt32(), if the application allows" +

            "\u0020only CBOR integers, or \u0028cbor.IsNumber &&" +
            "cbor.AsNumber().CanFitInInt32())," +
            "\u0020 if the application allows any CBOR object convertible to an " +
            "integer.")]
        public bool CanFitInInt32()
        {
            if (!this.CanFitInInt64())
            {
                return false;
            }
            long v = this.AsInt64();
            return v >= Int32.MinValue && v <= Int32.MaxValue;
        }

        /// <summary>Returns whether this object's numerical value is an
        /// integer, is -(2^63) or greater, and is less than 2^63.</summary>
        /// <returns><c>true</c> if this object's numerical value is an
        /// integer, is -(2^63) or greater, and is less than 2^63; otherwise,
        /// <c>false</c>.</returns>
        [Obsolete("Instead, use " +
            "CanValueFitInInt64(), if the application allows" +

            "\u0020only CBOR integers, or \u0028cbor.IsNumber &&" +
            "cbor.AsNumber().CanFitInInt64())," +
            "\u0020 if the application allows any CBOR object convertible to an " +
            "integer.")]
        public bool CanFitInInt64()
        {
            CBORNumber cn = CBORNumber.FromCBORObject(this);
            return (cn != null) &&
              cn.GetNumberInterface().CanFitInInt64(cn.GetValue());
        }

        /// <summary>Returns whether this object's value can be converted to a
        /// 32-bit floating point number without its value being rounded to
        /// another numerical value.</summary>
        /// <returns><c>true</c> if this object's value can be converted to a
        /// 32-bit floating point number without its value being rounded to
        /// another numerical value, or if this is a not-a-number value, even
        /// if the value's diagnostic information can' t fit in a 32-bit
        /// floating point number; otherwise, <c>false</c>.</returns>
        [Obsolete("Instead, use the following: \u0028cbor.IsNumber &&" +
            "\u0020cbor.AsNumber().CanFitInSingle()).")]
        public bool CanFitInSingle()
        {
            CBORNumber cn = CBORNumber.FromCBORObject(this);
            return (cn != null) &&
              cn.GetNumberInterface().CanFitInSingle(cn.GetValue());
        }

        /// <summary>Returns whether this object's value, converted to an
        /// integer by discarding its fractional part, would be -(2^31) or
        /// greater, and less than 2^31.</summary>
        /// <returns><c>true</c> if this object's value, converted to an
        /// integer by discarding its fractional part, would be -(2^31) or
        /// greater, and less than 2^31; otherwise, <c>false</c>.</returns>
        [Obsolete("Instead, use the following: \u0028cbor.CanValueFitInInt32()" +
            "\u0020if only integers of any tag are allowed, or" +
            "\u0020\u0028cbor.IsNumber &&" +
            "\u0020cbor.AsNumber().CanTruncatedIntFitInInt32()).")]
        public bool CanTruncatedIntFitInInt32()
        {
            CBORNumber cn = CBORNumber.FromCBORObject(this);
            return (cn != null) &&
              cn.GetNumberInterface().CanTruncatedIntFitInInt32(cn.GetValue());
        }

        /// <summary>Returns whether this object's value, converted to an
        /// integer by discarding its fractional part, would be -(2^63) or
        /// greater, and less than 2^63.</summary>
        /// <returns><c>true</c> if this object's value, converted to an
        /// integer by discarding its fractional part, would be -(2^63) or
        /// greater, and less than 2^63; otherwise, <c>false</c>.</returns>
        [Obsolete("Instead, use the following: \u0028cbor.CanValueFitInInt64()" +
            "\u0020if only integers of any tag are allowed, or" +
            "\u0020\u0028cbor.IsNumber &&" +
            "\u0020cbor.AsNumber().CanTruncatedIntFitInInt64()).")]
        public bool CanTruncatedIntFitInInt64()
        {
            CBORNumber cn = CBORNumber.FromCBORObject(this);
            return cn != null &&
              cn.GetNumberInterface().CanTruncatedIntFitInInt64(cn.GetValue());
        }

        private static string Chop(string str)
        {
            return str.Substring(0, Math.Min(100, str.Length));
        }

        /// <summary>Compares two CBOR objects. This implementation was changed
        /// in version 4.0.
        /// <para>In this implementation:</para>
        /// <list type=''>
        /// <item>The null pointer (null reference) is considered less than any
        /// other object.</item>
        /// <item>If the two objects are both integers (CBORType.Integer) both
        /// floating-point values, both byte strings, both simple values
        /// (including True and False), or both text strings, their CBOR
        /// encodings (as though EncodeToBytes were called on each integer) are
        /// compared as though by a byte-by-byte comparison. (This means, for
        /// example, that positive integers sort before negative
        /// integers).</item>
        /// <item>If both objects have a tag, they are compared first by the
        /// tag's value then by the associated item (which itself can have a
        /// tag).</item>
        /// <item>If both objects are arrays, they are compared item by item.
        /// In this case, if the arrays have different numbers of items, the
        /// array with more items is treated as greater than the other
        /// array.</item>
        /// <item>If both objects are maps, their key-value pairs, sorted by
        /// key in accordance with this method, are compared, where each pair
        /// is compared first by key and then by value. In this case, if the
        /// maps have different numbers of key-value pairs, the map with more
        /// pairs is treated as greater than the other map.</item>
        /// <item>If the two objects have different types, the object whose
        /// type comes first in the order of untagged integers, untagged byte
        /// strings, untagged text strings, untagged arrays, untagged maps,
        /// tagged objects, untagged simple values (including True and False)
        /// and untagged floating point values sorts before the other
        /// object.</item></list>
        /// <para>This method is consistent with the Equals
        /// method.</para></summary>
        /// <param name='other'>A value to compare with.</param>
        /// <returns>A negative number, if this value is less than the other
        /// object; or 0, if both values are equal; or a positive number, if
        /// this value is less than the other object or if the other object is
        /// null.
        /// <para>This implementation returns a positive number if <paramref
        /// name='other'/> is null, to conform to the.NET definition of
        /// CompareTo. This is the case even in the Java version of this
        /// library, for consistency's sake, even though implementations of
        /// <c>Comparable.compareTo()</c> in Java ought to throw an exception
        /// if they receive a null argument rather than treating null as less
        /// or greater than any object.</para>.</returns>
        public int CompareTo(CBORObject other)
        {
            if (other == null)
            {
                return 1;
            }
            if (this == other)
            {
                return 0;
            }
            int typeA = this.itemtypeValue;
            int typeB = other.itemtypeValue;
            object objA = this.itemValue;
            object objB = other.itemValue;
            // DebugUtility.Log("typeA=" + typeA);
            // DebugUtility.Log("typeB=" + typeB);
            // DebugUtility.Log("objA=" + Chop(this.ItemType ==
            // CBORObjectTypeMap ? "(map)" :
            // this.ToString()));
            // DebugUtility.Log("objB=" + Chop(other.ItemType ==
            // CBORObjectTypeMap ? "(map)" :
            // other.ToString()));
            int cmp;
            if (typeA == typeB)
            {
                switch (typeA)
                {
                    case CBORObjectTypeInteger:
                        {
                            var a = (long)objA;
                            var b = (long)objB;
                            if (a >= 0 && b >= 0)
                            {
                                cmp = (a == b) ? 0 : ((a < b) ? -1 : 1);
                            }
                            else if (a <= 0 && b <= 0)
                            {
                                cmp = (a == b) ? 0 : ((a < b) ? 1 : -1);
                            }
                            else if (a < 0 && b >= 0)
                            {
                                // NOTE: Negative integers sort after
                                // nonnegative integers in the bytewise
                                // ordering of CBOR encodings
                                cmp = 1;
                            }
                            else
                            {
#if DEBUG
                                if (!(a >= 0 && b < 0))
                                {
                                    throw new InvalidOperationException(
                                      "doesn't satisfy a>= 0" +
                                      "\u0020b<0");
                                }
#endif
                                cmp = -1;
                            }
                            break;
                        }
                    case CBORObjectTypeEInteger:
                        {
                            cmp = CBORUtilities.ByteArrayCompare(
                                this.EncodeToBytes(),
                                other.EncodeToBytes());
                            break;
                        }
                    case CBORObjectTypeByteString:
                    case CBORObjectTypeTextStringUtf8:
                        {
                            cmp = CBORUtilities.ByteArrayCompareLengthFirst((byte[])objA,
                                (byte[])objB);
                            break;
                        }
                    case CBORObjectTypeTextString:
                        {
                            var strA = (string)objA;
                            var strB = (string)objB;
                            cmp = CBORUtilities.CompareStringsAsUtf8LengthFirst(
                                strA,
                                strB);
                            break;
                        }
                    case CBORObjectTypeArray:
                        {
                            cmp = ListCompare(
                                (List<CBORObject>)objA,
                                (List<CBORObject>)objB);
                            break;
                        }
                    case CBORObjectTypeMap:
                        cmp = MapCompare(
                            (IDictionary<CBORObject, CBORObject>)objA,
                            (IDictionary<CBORObject, CBORObject>)objB);
                        break;
                    case CBORObjectTypeTagged:
                        cmp = this.MostOuterTag.CompareTo(other.MostOuterTag);
                        if (cmp == 0)
                        {
                            cmp = ((CBORObject)objA).CompareTo((CBORObject)objB);
                        }
                        break;
                    case CBORObjectTypeSimpleValue:
                        {
                            var valueA = (int)objA;
                            var valueB = (int)objB;
                            cmp = (valueA == valueB) ? 0 : ((valueA < valueB) ? -1 : 1);
                            break;
                        }
                    case CBORObjectTypeDouble:
                        {
                            cmp = CBORUtilities.ByteArrayCompare(
                                GetDoubleBytes(this.AsDoubleBits(), 0),
                                GetDoubleBytes(other.AsDoubleBits(), 0));
                            break;
                        }
                    default:
                        throw new InvalidOperationException("Unexpected data " +
                   "type");
                }
            }
            else if ((typeB == CBORObjectTypeInteger && typeA ==
              CBORObjectTypeEInteger) || (typeA == CBORObjectTypeInteger && typeB ==
              CBORObjectTypeEInteger))
            {
                cmp = CBORUtilities.ByteArrayCompare(
                    this.EncodeToBytes(),
                    other.EncodeToBytes());
            }
            else if (typeB == CBORObjectTypeTextString && typeA ==
            CBORObjectTypeTextStringUtf8)
            {
                cmp = -CBORUtilities.CompareUtf16Utf8LengthFirst(
                    (string)objB,
                    (byte[])objA);
            }
            else if (typeA == CBORObjectTypeTextString && typeB ==
            CBORObjectTypeTextStringUtf8)
            {
                cmp = CBORUtilities.CompareUtf16Utf8LengthFirst(
                    (string)objA,
                    (byte[])objB);
            }
            else
            {
                int ta = (typeA == CBORObjectTypeTextStringUtf8) ?
                  CBORObjectTypeTextString : typeA;
                int tb = (typeB == CBORObjectTypeTextStringUtf8) ?
                  CBORObjectTypeTextString : typeB;
                /* NOTE: itemtypeValue numbers are ordered such that they
                // correspond to the lexicographical order of their CBOR encodings
                // (with the exception of Integer and EInteger together,
                // and TextString/TextStringUtf8) */
                cmp = (ta < tb) ? -1 : 1;
            }
            // DebugUtility.Log(" -> " + (cmp));
            return cmp;
        }

        /// <summary>Compares this object and another CBOR object, ignoring the
        /// tags they have, if any. See the CompareTo method for more
        /// information on the comparison function.</summary>
        /// <param name='other'>A value to compare with.</param>
        /// <returns>Less than 0, if this value is less than the other object;
        /// or 0, if both values are equal; or greater than 0, if this value is
        /// less than the other object or if the other object is
        /// null.</returns>
        public int CompareToIgnoreTags(CBORObject other)
        {
            return (other == null) ? 1 : ((this == other) ? 0 :
                this.Untag().CompareTo(other.Untag()));
        }

        /// <summary>Determines whether a value of the given key exists in this
        /// object.</summary>
        /// <param name='objKey'>The parameter <paramref name='objKey'/> is an
        /// arbitrary object.</param>
        /// <returns><c>true</c> if the given key is found, or <c>false</c> if
        /// the given key is not found or this object is not a map.</returns>
        public bool ContainsKey(object objKey)
        {
            return (this.Type == CBORType.Map) ?
              this.ContainsKey(CBORObject.FromObject(objKey)) : false;
        }

        /// <summary>Determines whether a value of the given key exists in this
        /// object.</summary>
        /// <param name='key'>An object that serves as the key. If this is
        /// <c>null</c>, checks for <c>CBORObject.Null</c>.</param>
        /// <returns><c>true</c> if the given key is found, or <c>false</c> if
        /// the given key is not found or this object is not a map.</returns>
        public bool ContainsKey(CBORObject key)
        {
            key = key ?? CBORObject.Null;
            if (this.Type == CBORType.Map)
            {
                IDictionary<CBORObject, CBORObject> map = this.AsMap();
                return map.ContainsKey(key);
            }
            return false;
        }

        /// <summary>Determines whether a value of the given key exists in this
        /// object.</summary>
        /// <param name='key'>A text string that serves as the key. If this is
        /// <c>null</c>, checks for <c>CBORObject.Null</c>.</param>
        /// <returns><c>true</c> if the given key (as a CBOR object) is found,
        /// or <c>false</c> if the given key is not found or this object is not
        /// a map.</returns>
        public bool ContainsKey(string key)
        {
            if (this.Type == CBORType.Map)
            {
                CBORObject ckey = key == null ? CBORObject.Null :
                  CBORObject.FromObject(key);
                IDictionary<CBORObject, CBORObject> map = this.AsMap();
                return map.ContainsKey(ckey);
            }
            return false;
        }

        private static byte[] GetDoubleBytes(long valueBits, int tagbyte)
        {
            int bits = CBORUtilities.DoubleToHalfPrecisionIfSameValue(valueBits);
            if (bits != -1)
            {
                return tagbyte != 0 ? new[] {
          (byte)tagbyte, (byte)0xf9,
          (byte)((bits >> 8) & 0xff), (byte)(bits & 0xff),
        } : new[] {
   (byte)0xf9, (byte)((bits >> 8) & 0xff),
   (byte)(bits & 0xff),
 };
            }
            if (CBORUtilities.DoubleRetainsSameValueInSingle(valueBits))
            {
                bits = CBORUtilities.DoubleToRoundedSinglePrecision(valueBits);
                return tagbyte != 0 ? new[] {
          (byte)tagbyte, (byte)0xfa,
          (byte)((bits >> 24) & 0xff), (byte)((bits >> 16) & 0xff),
          (byte)((bits >> 8) & 0xff), (byte)(bits & 0xff),
        } : new[] {
   (byte)0xfa, (byte)((bits >> 24) & 0xff),
   (byte)((bits >> 16) & 0xff), (byte)((bits >> 8) & 0xff),
   (byte)(bits & 0xff),
 };
            }
            // Encode as double precision
            return tagbyte != 0 ? new[] {
        (byte)tagbyte, (byte)0xfb,
        (byte)((valueBits >> 56) & 0xff), (byte)((valueBits >> 48) & 0xff),
        (byte)((valueBits >> 40) & 0xff), (byte)((valueBits >> 32) & 0xff),
        (byte)((valueBits >> 24) & 0xff), (byte)((valueBits >> 16) & 0xff),
        (byte)((valueBits >> 8) & 0xff), (byte)(valueBits & 0xff),
      } : new[] {
   (byte)0xfb, (byte)((valueBits >> 56) & 0xff),
   (byte)((valueBits >> 48) & 0xff), (byte)((valueBits >> 40) & 0xff),
   (byte)((valueBits >> 32) & 0xff), (byte)((valueBits >> 24) & 0xff),
   (byte)((valueBits >> 16) & 0xff), (byte)((valueBits >> 8) & 0xff),
   (byte)(valueBits & 0xff),
 };
        }

        /// <summary>
        /// <para>Writes the binary representation of this CBOR object and
        /// returns a byte array of that representation. If the CBOR object
        /// contains CBOR maps, or is a CBOR map itself, the keys to the map
        /// are written out to the byte array in an undefined order. The
        /// example code given in
        /// <see cref='PeterO.Cbor.CBORObject.WriteTo(System.IO.Stream)'/> can
        /// be used to write out certain keys of a CBOR map in a given order.
        /// For the CTAP2 (FIDO Client-to-Authenticator Protocol 2) canonical
        /// ordering, which is useful for implementing Web Authentication, call
        /// <c>EncodeToBytes(new CBOREncodeOptions("ctap2canonical=true"))</c>
        /// rather than this method.</para></summary>
        /// <returns>A byte array in CBOR format.</returns>
        public byte[] EncodeToBytes()
        {
            return this.EncodeToBytes(CBOREncodeOptions.Default);
        }

        /// <summary>Writes the binary representation of this CBOR object and
        /// returns a byte array of that representation, using the specified
        /// options for encoding the object to CBOR format. For the CTAP2 (FIDO
        /// Client-to-Authenticator Protocol 2) canonical ordering, which is
        /// useful for implementing Web Authentication, call this method as
        /// follows: <c>EncodeToBytes(new
        /// CBOREncodeOptions("ctap2canonical=true"))</c>.</summary>
        /// <param name='options'>Options for encoding the data to
        /// CBOR.</param>
        /// <returns>A byte array in CBOR format.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='options'/> is null.</exception>
        public byte[] EncodeToBytes(CBOREncodeOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (options.Ctap2Canonical)
            {
                return CBORCanonical.CtapCanonicalEncode(this);
            }
            // For some types, a memory stream is a lot of
            // overhead since the amount of memory the types
            // use is fixed and small
            var hasComplexTag = false;
            byte tagbyte = 0;
            bool tagged = this.IsTagged;
            if (this.IsTagged)
            {
                var taggedItem = (CBORObject)this.itemValue;
                if (taggedItem.IsTagged || this.tagHigh != 0 ||
                  (this.tagLow >> 16) != 0 || this.tagLow >= 24)
                {
                    hasComplexTag = true;
                }
                else
                {
                    tagbyte = (byte)(0xc0 + (int)this.tagLow);
                }
            }
            if (!hasComplexTag)
            {
                switch (this.ItemType)
                {
                    case CBORObjectTypeTextString:
                        {
                            byte[] ret = GetOptimizedBytesIfShortAscii(
                                this.AsString(), tagged ? (((int)tagbyte) & 0xff) : -1);
                            if (ret != null)
                            {
                                return ret;
                            }
                            break;
                        }
                    case CBORObjectTypeTextStringUtf8:
                        {
                            if (!tagged && !options.UseIndefLengthStrings)
                            {
                                byte[] bytes = (byte[])this.ThisItem;
                                return SerializeUtf8(bytes);
                            }
                            break;
                        }
                    case CBORObjectTypeSimpleValue:
                        {
                            if (tagged)
                            {
                                var simpleBytes = new byte[] { tagbyte, (byte)0xf4 };
                                if (this.IsFalse)
                                {
                                    simpleBytes[1] = (byte)0xf4;
                                    return simpleBytes;
                                }
                                if (this.IsTrue)
                                {
                                    simpleBytes[1] = (byte)0xf5;
                                    return simpleBytes;
                                }
                                if (this.IsNull)
                                {
                                    simpleBytes[1] = (byte)0xf6;
                                    return simpleBytes;
                                }
                                if (this.IsUndefined)
                                {
                                    simpleBytes[1] = (byte)0xf7;
                                    return simpleBytes;
                                }
                            }
                            else
                            {
                                if (this.IsFalse)
                                {
                                    return new[] { (byte)0xf4 };
                                }
                                if (this.IsTrue)
                                {
                                    return new[] { (byte)0xf5 };
                                }
                                if (this.IsNull)
                                {
                                    return new[] { (byte)0xf6 };
                                }
                                if (this.IsUndefined)
                                {
                                    return new[] { (byte)0xf7 };
                                }
                            }
                            break;
                        }
                    case CBORObjectTypeInteger:
                        {
                            var value = (long)this.ThisItem;
                            byte[] intBytes = null;
                            if (value >= 0)
                            {
                                intBytes = GetPositiveInt64Bytes(0, value);
                            }
                            else
                            {
                                ++value;
                                value = -value; // Will never overflow
                                intBytes = GetPositiveInt64Bytes(1, value);
                            }
                            if (!tagged)
                            {
                                return intBytes;
                            }
                            var ret2 = new byte[intBytes.Length + 1];
                            Array.Copy(intBytes, 0, ret2, 1, intBytes.Length);
                            ret2[0] = tagbyte;
                            return ret2;
                        }
                    case CBORObjectTypeDouble:
                        {
                            return GetDoubleBytes(
                                this.AsDoubleBits(),
                                ((int)tagbyte) & 0xff);
                        }
                }
            }
            try
            {
                using (var ms = new MemoryStream(16))
                {
                    this.WriteTo(ms, options);
                    return ms.ToArray();
                }
            }
            catch (IOException ex)
            {
                throw new CBORException("I/O Error occurred", ex);
            }
        }

        /// <summary>Determines whether this object and another object are
        /// equal and have the same type. Not-a-number values can be considered
        /// equal by this method.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// arbitrary object.</param>
        /// <returns><c>true</c> if the objects are equal; otherwise,
        /// <c>false</c>. In this method, two objects are not equal if they
        /// don't have the same type or if one is null and the other
        /// isn't.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as CBORObject);
        }

        /// <summary>Compares the equality of two CBOR objects. Not-a-number
        /// values can be considered equal by this method.</summary>
        /// <param name='other'>The object to compare.</param>
        /// <returns><c>true</c> if the objects are equal; otherwise,
        /// <c>false</c>. In this method, two objects are not equal if they
        /// don't have the same type or if one is null and the other
        /// isn't.</returns>
        public bool Equals(CBORObject other)
        {
            var otherValue = other as CBORObject;
            if (otherValue == null)
            {
                return false;
            }
            if (this == otherValue)
            {
                return true;
            }
            if (this.itemtypeValue == CBORObjectTypeTextString &&
              otherValue.itemtypeValue == CBORObjectTypeTextStringUtf8)
            {
                return CBORUtilities.StringEqualsUtf8(
                    (string)this.itemValue,
                    (byte[])otherValue.itemValue);
            }
            if (otherValue.itemtypeValue == CBORObjectTypeTextString &&
              this.itemtypeValue == CBORObjectTypeTextStringUtf8)
            {
                return CBORUtilities.StringEqualsUtf8(
                    (string)otherValue.itemValue,
                    (byte[])this.itemValue);
            }
            if (this.itemtypeValue != otherValue.itemtypeValue)
            {
                return false;
            }
            switch (this.itemtypeValue)
            {
                case CBORObjectTypeByteString:
                case CBORObjectTypeTextStringUtf8:
                    return CBORUtilities.ByteArrayEquals(
                        (byte[])this.itemValue,
                        otherValue.itemValue as byte[]);
                case CBORObjectTypeMap:
                    {
                        IDictionary<CBORObject, CBORObject> cbordict =
                          otherValue.itemValue as IDictionary<CBORObject, CBORObject>;
                        return CBORMapEquals(this.AsMap(), cbordict);
                    }
                case CBORObjectTypeArray:
                    return CBORArrayEquals(
                        this.AsList(),
                        otherValue.itemValue as IList<CBORObject>);
                case CBORObjectTypeTagged:
                    return this.tagLow == otherValue.tagLow &&
                      this.tagHigh == otherValue.tagHigh &&
                      Object.Equals(this.itemValue, otherValue.itemValue);
                case CBORObjectTypeDouble:
                    return this.AsDoubleBits() == otherValue.AsDoubleBits();
                default: return Object.Equals(this.itemValue, otherValue.itemValue);
            }
        }

        /// <summary>Gets the backing byte array used in this CBOR object, if
        /// this object is a byte string, without copying the data to a new
        /// byte array. Any changes in the returned array's contents will be
        /// reflected in this CBOR object. Note, though, that the array's
        /// length can't be changed.</summary>
        /// <returns>The byte array held by this CBOR object.</returns>
        /// <exception cref='InvalidOperationException'>This object is not a
        /// byte string.</exception>
        public byte[] GetByteString()
        {
            if (this.ItemType == CBORObjectTypeByteString)
            {
                return (byte[])this.ThisItem;
            }
            throw new InvalidOperationException("Not a byte string");
        }

        /// <summary>Calculates the hash code of this object. The hash code for
        /// a given instance of this class is not guaranteed to be the same
        /// across versions of this class, and no application or process IDs
        /// are used in the hash code calculation.</summary>
        /// <returns>A 32-bit hash code.</returns>
        public override int GetHashCode()
        {
            var hashCode = 651869431;
            unchecked
            {
                if (this.itemValue != null)
                {
                    var itemHashCode = 0;
                    long longValue = 0L;
                    switch (this.itemtypeValue)
                    {
                        case CBORObjectTypeByteString:
                            itemHashCode =
                              CBORUtilities.ByteArrayHashCode(this.GetByteString());
                            break;
                        case CBORObjectTypeTextStringUtf8:
                            itemHashCode = CBORUtilities.Utf8HashCode(
                                (byte[])this.itemValue);
                            break;
                        case CBORObjectTypeMap:
                            itemHashCode = CBORMapHashCode(this.AsMap());
                            break;
                        case CBORObjectTypeArray:
                            itemHashCode = CBORArrayHashCode(this.AsList());
                            break;
                        case CBORObjectTypeTextString:
                            itemHashCode = CBORUtilities.StringHashCode(
                                (string)this.itemValue);
                            break;
                        case CBORObjectTypeSimpleValue:
                            itemHashCode = (int)this.itemValue;
                            break;
                        case CBORObjectTypeDouble:
                            longValue = this.AsDoubleBits();
                            longValue |= longValue >> 32;
                            itemHashCode = unchecked((int)longValue);
                            break;
                        case CBORObjectTypeInteger:
                            longValue = (long)this.itemValue;
                            longValue |= longValue >> 32;
                            itemHashCode = unchecked((int)longValue);
                            break;
                        case CBORObjectTypeTagged:
                            itemHashCode = unchecked(this.tagLow + this.tagHigh);
                            itemHashCode += 651869483 * this.itemValue.GetHashCode();
                            break;
                        default:
                            // EInteger, CBORObject
                            itemHashCode = this.itemValue.GetHashCode();
                            break;
                    }
                    hashCode += 651869479 * itemHashCode;
                }
            }
            return hashCode;
        }

        /// <summary>Gets a list of all tags, from outermost to
        /// innermost.</summary>
        /// <returns>An array of tags, or the empty string if this object is
        /// untagged.</returns>
        public EInteger[] GetAllTags()
        {
            if (!this.IsTagged)
            {
                return ValueEmptyTags;
            }
            CBORObject curitem = this;
            if (curitem.IsTagged)
            {
                var list = new List<EInteger>();
                while (curitem.IsTagged)
                {
                    list.Add(
                      LowHighToEInteger(
                        curitem.tagLow,
                        curitem.tagHigh));
                    curitem = (CBORObject)curitem.itemValue;
                }
                return (EInteger[])list.ToArray();
            }
            return new[] { LowHighToEInteger(this.tagLow, this.tagHigh) };
        }

        /// <summary>Returns whether this object has only one tag.</summary>
        /// <returns><c>true</c> if this object has only one tag; otherwise,
        /// <c>false</c>.</returns>
        public bool HasOneTag()
        {
            return this.IsTagged && !((CBORObject)this.itemValue).IsTagged;
        }

        /// <summary>Returns whether this object has only one tag and that tag
        /// is the given number.</summary>
        /// <param name='tagValue'>The tag number.</param>
        /// <returns><c>true</c> if this object has only one tag and that tag
        /// is the given number; otherwise, <c>false</c>.</returns>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='tagValue'/> is less than 0.</exception>
        public bool HasOneTag(int tagValue)
        {
            return this.HasOneTag() && this.HasMostOuterTag(tagValue);
        }

        /// <summary>Returns whether this object has only one tag and that tag
        /// is the given number, expressed as an arbitrary-precision
        /// integer.</summary>
        /// <param name='bigTagValue'>An arbitrary-precision integer.</param>
        /// <returns><c>true</c> if this object has only one tag and that tag
        /// is the given number; otherwise, <c>false</c>.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='bigTagValue'/> is null.</exception>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='bigTagValue'/> is less than 0.</exception>
        public bool HasOneTag(EInteger bigTagValue)
        {
            return this.HasOneTag() && this.HasMostOuterTag(bigTagValue);
        }

        /// <summary>Gets the number of tags this object has.</summary>
        /// <value>The number of tags this object has.</value>
        public int TagCount
        {
            get
            {
                var count = 0;
                CBORObject curitem = this;
                while (curitem.IsTagged)
                {
                    count = checked(count + 1);
                    curitem = (CBORObject)curitem.itemValue;
                }
                return count;
            }
        }

        /// <summary>Returns whether this object has an innermost tag and that
        /// tag is of the given number.</summary>
        /// <param name='tagValue'>The tag number.</param>
        /// <returns><c>true</c> if this object has an innermost tag and that
        /// tag is of the given number; otherwise, <c>false</c>.</returns>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='tagValue'/> is less than 0.</exception>
        public bool HasMostInnerTag(int tagValue)
        {
            if (tagValue < 0)
            {
                throw new ArgumentException("tagValue(" + tagValue +
                  ") is less than 0");
            }
            return this.IsTagged && this.HasMostInnerTag(
                EInteger.FromInt32(tagValue));
        }

        /// <summary>Returns whether this object has an innermost tag and that
        /// tag is of the given number, expressed as an arbitrary-precision
        /// number.</summary>
        /// <param name='bigTagValue'>The tag number.</param>
        /// <returns><c>true</c> if this object has an innermost tag and that
        /// tag is of the given number; otherwise, <c>false</c>.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='bigTagValue'/> is null.</exception>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='bigTagValue'/> is less than 0.</exception>
        public bool HasMostInnerTag(EInteger bigTagValue)
        {
            if (bigTagValue == null)
            {
                throw new ArgumentNullException(nameof(bigTagValue));
            }
            if (bigTagValue.Sign < 0)
            {
                throw new ArgumentException("bigTagValue(" + bigTagValue +
                  ") is less than 0");
            }
            return (!this.IsTagged) ? false : this.MostInnerTag.Equals(bigTagValue);
        }

        /// <summary>Returns whether this object has an outermost tag and that
        /// tag is of the given number.</summary>
        /// <param name='tagValue'>The tag number.</param>
        /// <returns><c>true</c> if this object has an outermost tag and that
        /// tag is of the given number; otherwise, <c>false</c>.</returns>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='tagValue'/> is less than 0.</exception>
        public bool HasMostOuterTag(int tagValue)
        {
            if (tagValue < 0)
            {
                throw new ArgumentException("tagValue(" + tagValue +
                  ") is less than 0");
            }
            return this.IsTagged && this.tagHigh == 0 && this.tagLow == tagValue;
        }

        /// <summary>Returns whether this object has an outermost tag and that
        /// tag is of the given number.</summary>
        /// <param name='bigTagValue'>The tag number.</param>
        /// <returns><c>true</c> if this object has an outermost tag and that
        /// tag is of the given number; otherwise, <c>false</c>.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='bigTagValue'/> is null.</exception>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='bigTagValue'/> is less than 0.</exception>
        public bool HasMostOuterTag(EInteger bigTagValue)
        {
            if (bigTagValue == null)
            {
                throw new ArgumentNullException(nameof(bigTagValue));
            }
            if (bigTagValue.Sign < 0)
            {
                throw new ArgumentException("bigTagValue(" + bigTagValue +
                  ") is less than 0");
            }
            return (!this.IsTagged) ? false : this.MostOuterTag.Equals(bigTagValue);
        }

        /// <summary>Returns whether this object has a tag of the given
        /// number.</summary>
        /// <param name='tagValue'>The tag value to search for.</param>
        /// <returns><c>true</c> if this object has a tag of the given number;
        /// otherwise, <c>false</c>.</returns>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='tagValue'/> is less than 0.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='tagValue'/> is null.</exception>
        public bool HasTag(int tagValue)
        {
            if (tagValue < 0)
            {
                throw new ArgumentException("tagValue(" + tagValue +
                  ") is less than 0");
            }
            CBORObject obj = this;
            while (true)
            {
                if (!obj.IsTagged)
                {
                    return false;
                }
                if (obj.tagHigh == 0 && tagValue == obj.tagLow)
                {
                    return true;
                }
                obj = (CBORObject)obj.itemValue;
#if DEBUG
                if (obj == null)
                {
                    throw new ArgumentNullException(nameof(tagValue));
                }
#endif
            }
        }

        /// <summary>Returns whether this object has a tag of the given
        /// number.</summary>
        /// <param name='bigTagValue'>The tag value to search for.</param>
        /// <returns><c>true</c> if this object has a tag of the given number;
        /// otherwise, <c>false</c>.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='bigTagValue'/> is null.</exception>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='bigTagValue'/> is less than 0.</exception>
        public bool HasTag(EInteger bigTagValue)
        {
            if (bigTagValue == null)
            {
                throw new ArgumentNullException(nameof(bigTagValue));
            }
            if (bigTagValue.Sign < 0)
            {
                throw new ArgumentException("doesn't satisfy bigTagValue.Sign>= 0");
            }
            EInteger[] bigTags = this.GetAllTags();
            foreach (EInteger bigTag in bigTags)
            {
                if (bigTagValue.Equals(bigTag))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Inserts an object at the specified position in this CBOR
        /// array.</summary>
        /// <param name='index'>Index starting at 0 to insert at.</param>
        /// <param name='valueOb'>An object representing the value, which will
        /// be converted to a CBORObject. Can be null, in which case this value
        /// is converted to CBORObject.Null.</param>
        /// <returns>This instance.</returns>
        /// <exception cref='InvalidOperationException'>This object is not an
        /// array.</exception>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='valueOb'/> has an unsupported type; or <paramref
        /// name='index'/> is not a valid index into this array.</exception>
        public CBORObject Insert(int index, object valueOb)
        {
            if (this.Type == CBORType.Array)
            {
                CBORObject mapValue;
                IList<CBORObject> list = this.AsList();
                if (index < 0 || index > list.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                if (valueOb == null)
                {
                    mapValue = CBORObject.Null;
                }
                else
                {
                    mapValue = valueOb as CBORObject;
                    mapValue = mapValue ?? CBORObject.FromObject(valueOb);
                }
                list.Insert(
                  index,
                  mapValue);
            }
            else
            {
                throw new InvalidOperationException("Not an array");
            }
            return this;
        }

        /// <summary>Gets a value indicating whether this CBOR object
        /// represents infinity.</summary>
        /// <returns><c>true</c> if this CBOR object represents infinity;
        /// otherwise, <c>false</c>.</returns>
        [Obsolete("Instead, use the following: \u0028cbor.IsNumber &&" +
            "\u0020cbor.AsNumber().IsInfinity()).")]
        public bool IsInfinity()
        {
            return this.IsNumber && this.AsNumber().IsInfinity();
        }

        /// <summary>Gets a value indicating whether this CBOR object
        /// represents a not-a-number value (as opposed to whether this object
        /// does not express a number).</summary>
        /// <returns><c>true</c> if this CBOR object represents a not-a-number
        /// value (as opposed to whether this object does not represent a
        /// number as defined by the IsNumber property or <c>isNumber()</c>
        /// method in Java); otherwise, <c>false</c>.</returns>
        [Obsolete("Instead, use the following: \u0028cbor.IsNumber &&" +

            "\u0020cbor.AsNumber().IsNaN()).")]
        public bool IsNaN()
        {
            return this.IsNumber && this.AsNumber().IsNaN();
        }

        /// <summary>Gets a value indicating whether this CBOR object
        /// represents negative infinity.</summary>
        /// <returns><c>true</c> if this CBOR object represents negative
        /// infinity; otherwise, <c>false</c>.</returns>
        [Obsolete("Instead, use the following: \u0028cbor.IsNumber &&" +

            "\u0020cbor.AsNumber().IsNegativeInfinity()).")]
        public bool IsNegativeInfinity()
        {
            CBORNumber cn = CBORNumber.FromCBORObject(this);
            return cn != null &&
              cn.GetNumberInterface().IsNegativeInfinity(cn.GetValue());
        }

        /// <summary>Gets a value indicating whether this CBOR object
        /// represents positive infinity.</summary>
        /// <returns><c>true</c> if this CBOR object represents positive
        /// infinity; otherwise, <c>false</c>.</returns>
        [Obsolete("Instead, use the following: \u0028cbor.IsNumber &&" +

            "\u0020cbor.AsNumber().IsPositiveInfinity()).")]
        public bool IsPositiveInfinity()
        {
            CBORNumber cn = CBORNumber.FromCBORObject(this);
            return cn != null &&
              cn.GetNumberInterface().IsPositiveInfinity(cn.GetValue());
        }

        /// <summary>Gets this object's value with the sign reversed.</summary>
        /// <returns>The reversed-sign form of this number.</returns>
        /// <exception cref='InvalidOperationException'>This object does not
        /// represent a number (for this purpose, infinities and not-a-number
        /// or NaN values, but not CBORObject.Null, are considered
        /// numbers).</exception>
        [Obsolete("Instead, convert this object to a number \u0028with" +

            "\u0020.AsNumber()), and use that number's .Negate() method.")]
        public CBORObject Negate()
        {
            CBORNumber cn = CBORNumber.FromCBORObject(this);
            if (cn == null)
            {
                throw new InvalidOperationException("This object is not a number.");
            }
            object newItem = cn.GetNumberInterface().Negate(cn.GetValue());
            if (newItem is EDecimal)
            {
                return CBORObject.FromObject((EDecimal)newItem);
            }
            if (newItem is EInteger)
            {
                return CBORObject.FromObject((EInteger)newItem);
            }
            if (newItem is EFloat)
            {
                return CBORObject.FromObject((EFloat)newItem);
            }
            var rat = newItem as ERational;
            return (rat != null) ? CBORObject.FromObject(rat) :
              CBORObject.FromObject(newItem);
        }

        /// <summary>Removes all items from this CBOR array or all keys and
        /// values from this CBOR map.</summary>
        /// <exception cref='InvalidOperationException'>This object is not a
        /// CBOR array or CBOR map.</exception>
        public void Clear()
        {
            if (this.Type == CBORType.Array)
            {
                IList<CBORObject> list = this.AsList();
                list.Clear();
            }
            else if (this.Type == CBORType.Map)
            {
                IDictionary<CBORObject, CBORObject> dict = this.AsMap();
                dict.Clear();
            }
            else
            {
                throw new InvalidOperationException("Not a map or array");
            }
        }

        /// <summary>If this object is an array, removes the first instance of
        /// the specified item (once converted to a CBOR object) from the
        /// array. If this object is a map, removes the item with the given key
        /// (once converted to a CBOR object) from the map.</summary>
        /// <param name='obj'>The item or key (once converted to a CBOR object)
        /// to remove.</param>
        /// <returns><c>true</c> if the item was removed; otherwise,
        /// <c>false</c>.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='obj'/> is null (as opposed to CBORObject.Null).</exception>
        /// <exception cref='InvalidOperationException'>The object is not an
        /// array or map.</exception>
        public bool Remove(object obj)
        {
            return this.Remove(CBORObject.FromObject(obj));
        }

        /// <summary>Removes the item at the given index of this CBOR
        /// array.</summary>
        /// <param name='index'>The index, starting at 0, of the item to
        /// remove.</param>
        /// <returns>Returns "true" if the object was removed. Returns "false"
        /// if the given index is less than 0, or is at least as high as the
        /// number of items in the array.</returns>
        /// <exception cref='InvalidOperationException'>This object is not a
        /// CBOR array.</exception>
        public bool RemoveAt(int index)
        {
            if (this.ItemType != CBORObjectTypeArray)
            {
                throw new InvalidOperationException("Not an array");
            }
            if (index < 0 || index >= this.Count)
            {
                return false;
            }
            IList<CBORObject> list = this.AsList();
            list.RemoveAt(index);
            return true;
        }

        /// <summary>If this object is an array, removes the first instance of
        /// the specified item from the array. If this object is a map, removes
        /// the item with the given key from the map.</summary>
        /// <param name='obj'>The item or key to remove.</param>
        /// <returns><c>true</c> if the item was removed; otherwise,
        /// <c>false</c>.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='obj'/> is null (as opposed to CBORObject.Null).</exception>
        /// <exception cref='InvalidOperationException'>The object is not an
        /// array or map.</exception>
        public bool Remove(CBORObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            if (this.Type == CBORType.Map)
            {
                IDictionary<CBORObject, CBORObject> dict = this.AsMap();
                bool hasKey = dict.ContainsKey(obj);
                if (hasKey)
                {
                    dict.Remove(obj);
                    return true;
                }
                return false;
            }
            if (this.Type == CBORType.Array)
            {
                IList<CBORObject> list = this.AsList();
                return list.Remove(obj);
            }
            throw new InvalidOperationException("Not a map or array");
        }

        /// <summary>Maps an object to a key in this CBOR map, or adds the
        /// value if the key doesn't exist. If this is a CBOR array, instead
        /// sets the value at the given index to the given value.</summary>
        /// <param name='key'>If this instance is a CBOR map, this parameter is
        /// an object representing the key, which will be converted to a
        /// CBORObject; in this case, this parameter can be null, in which case
        /// this value is converted to CBORObject.Null. If this instance is a
        /// CBOR array, this parameter must be a 32-bit signed integer(
        /// <c>int</c> ) identifying the index (starting from 0) of the item to
        /// set in the array.</param>
        /// <param name='valueOb'>An object representing the value, which will
        /// be converted to a CBORObject. Can be null, in which case this value
        /// is converted to CBORObject.Null.</param>
        /// <returns>This instance.</returns>
        /// <exception cref='InvalidOperationException'>This object is not a
        /// map or an array.</exception>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='key'/> or <paramref name='valueOb'/> has an unsupported type,
        /// or this instance is a CBOR array and <paramref name='key'/> is less
        /// than 0, is the size of this array or greater, or is not a 32-bit
        /// signed integer ( <c>int</c> ).</exception>
        public CBORObject Set(object key, object valueOb)
        {
            if (this.Type == CBORType.Map)
            {
                CBORObject mapKey;
                CBORObject mapValue;
                if (key == null)
                {
                    mapKey = CBORObject.Null;
                }
                else
                {
                    mapKey = key as CBORObject;
                    mapKey = mapKey ?? CBORObject.FromObject(key);
                }
                if (valueOb == null)
                {
                    mapValue = CBORObject.Null;
                }
                else
                {
                    mapValue = valueOb as CBORObject;
                    mapValue = mapValue ?? CBORObject.FromObject(valueOb);
                }
                IDictionary<CBORObject, CBORObject> map = this.AsMap();
                if (map.ContainsKey(mapKey))
                {
                    map[mapKey] = mapValue;
                }
                else
                {
                    map.Add(mapKey, mapValue);
                }
            }
            else if (this.Type == CBORType.Array)
            {
                if (key is int)
                {
                    IList<CBORObject> list = this.AsList();
                    var index = (int)key;
                    if (index < 0 || index >= this.Count)
                    {
                        throw new ArgumentOutOfRangeException(nameof(key));
                    }
                    CBORObject mapValue;
                    if (valueOb == null)
                    {
                        mapValue = CBORObject.Null;
                    }
                    else
                    {
                        mapValue = valueOb as CBORObject;
                        mapValue = mapValue ?? CBORObject.FromObject(valueOb);
                    }
                    list[index] = mapValue;
                }
                else
                {
                    throw new ArgumentException("Is an array, but key is not int");
                }
            }
            else
            {
                throw new InvalidOperationException("Not a map or array");
            }
            return this;
        }

        /// <summary>Converts this object to a text string in JavaScript Object
        /// Notation (JSON) format. See the overload to ToJSONString taking a
        /// JSONOptions argument for further information.
        /// <para>If the CBOR object contains CBOR maps, or is a CBOR map
        /// itself, the keys to the map are written out to the JSON string in
        /// an undefined order. Map keys other than untagged text strings are
        /// converted to JSON strings before writing them out (for example,
        /// <c>22("Test")</c> is converted to <c>"Test"</c> and <c>true</c> is
        /// converted to <c>"true"</c> ). If, after such conversion, two or
        /// more map keys are identical, this method throws a CBORException.
        /// The example code given in
        /// <b>PeterO.Cbor.CBORObject.ToJSONString(PeterO.Cbor.JSONOptions)</b>
        /// can be used to write out certain keys of a CBOR map in a given
        /// order to a JSON string, or to write out a CBOR object as part of a
        /// JSON text sequence.</para>
        /// <para><b>Warning:</b> In general, if this CBOR object contains
        /// integer map keys or uses other features not supported in JSON, and
        /// the application converts this CBOR object to JSON and back to CBOR,
        /// the application
        /// <i>should not</i> expect the new CBOR object to be exactly the same
        /// as the original. This is because the conversion in many cases may
        /// have to convert unsupported features in JSON to supported features
        /// which correspond to a different feature in CBOR (such as converting
        /// integer map keys, which are supported in CBOR but not JSON, to text
        /// strings, which are supported in both).</para></summary>
        /// <returns>A text string containing the converted object in JSON
        /// format.</returns>
        public string ToJSONString()
        {
            return this.ToJSONString(JSONOptions.Default);
        }

        /// <summary>
        ///  Converts this object to a text string in JavaScript
        /// Object Notation (JSON) format, using the specified
        /// options to control the encoding process. This function
        /// works not only with arrays and maps, but also integers,
        /// strings, byte arrays, and other JSON data types. Notes:
        /// <list type=''><item>If this object contains maps with non-string
        /// keys, the keys are converted to JSON strings before writing the map
        /// as a JSON string.</item>
        ///  <item>If this object represents a number
        /// (the IsNumber property, or isNumber() method in Java, returns
        /// true), then it is written out as a number.</item>
        ///  <item>If the CBOR
        /// object contains CBOR maps, or is a CBOR map itself, the keys to the
        /// map are written out to the JSON string in an undefined order. Map
        /// keys other than untagged text strings are converted to JSON strings
        /// before writing them out (for example, <c>22("Test")</c>
        ///  is
        /// converted to <c>"Test"</c>
        ///  and <c>true</c>
        ///  is converted to
        /// <c>"true"</c>
        ///  ). If, after such conversion, two or more map keys
        /// are identical, this method throws a CBORException.</item>
        ///  <item>If
        /// a number in the form of an arbitrary-precision binary
        /// floating-point number has a very high binary exponent, it will be
        /// converted to a double before being converted to a JSON string. (The
        /// resulting double could overflow to infinity, in which case the
        /// arbitrary-precision binary floating-point number is converted to
        /// null.)</item>
        ///  <item>The string will not begin with a byte-order
        /// mark (U+FEFF); RFC 8259 (the JSON specification) forbids placing a
        /// byte-order mark at the beginning of a JSON string.</item>
        /// <item>Byte strings are converted to Base64 URL without whitespace
        /// or padding by default (see section 4.1 of RFC 7049). A byte string
        /// will instead be converted to traditional base64 without whitespace
        /// and with padding if it has tag 22, or base16 for tag 23. (To create
        /// a CBOR object with a given tag, call the
        /// <c>CBORObject.FromObjectAndTag</c>
        ///  method and pass the CBOR object
        /// and the desired tag number to that method.)</item>
        ///  <item>Rational
        /// numbers will be converted to their exact form, if possible,
        /// otherwise to a high-precision approximation. (The resulting
        /// approximation could overflow to infinity, in which case the
        /// rational number is converted to null.)</item>
        ///  <item>Simple values
        /// other than true and false will be converted to null. (This doesn't
        /// include floating-point numbers.)</item>
        ///  <item>Infinity and
        /// not-a-number will be converted to null.</item>
        ///  </list>
        /// <para><b>Warning:</b>
        ///  In general, if this CBOR object contains
        /// integer map keys or uses other features not supported in JSON, and
        /// the application converts this CBOR object to JSON and back to CBOR,
        /// the application <i>should not</i>
        ///  expect the new CBOR object to be
        /// exactly the same as the original. This is because the conversion in
        /// many cases may have to convert unsupported features in JSON to
        /// supported features which correspond to a different feature in CBOR
        /// (such as converting integer map keys, which are supported in CBOR
        /// but not JSON, to text strings, which are supported in both).</para>
        /// <para>The example code given below (originally written in C# for
        /// the.NET version) can be used to write out certain keys of a CBOR
        /// map in a given order to a JSON string.</para>
        /// <code>/* Generates a JSON string of 'mapObj' whose keys are in the order
        /// given
        /// in 'keys' . Only keys found in 'keys' will be written if they exist in
        /// 'mapObj'. */ private static string KeysToJSONMap(CBORObject mapObj,
        /// IList&lt;CBORObject&gt; keys) { if (mapObj == null) { throw new
        /// ArgumentNullException)nameof(mapObj));}
        /// if (keys == null) { throw new
        /// ArgumentNullException)nameof(keys));}
        /// if (obj.Type != CBORType.Map) {
        /// throw new ArgumentException("'obj' is not a map."); } StringBuilder
        /// builder = new StringBuilder(); var first = true; builder.Append("{");
        /// for (CBORObject key in keys) { if (mapObj.ContainsKey(key)) { if
        /// (!first) {builder.Append(", ");} var keyString=(key.CBORType ==
        /// CBORType.String) ? key.AsString() : key.ToJSONString();
        /// builder.Append(CBORObject.FromObject(keyString) .ToJSONString())
        /// .Append(":").Append(mapObj[key].ToJSONString()); first=false; } } return
        /// builder.Append("}").ToString(); }</code>
        ///  .
        /// </summary>
        /// <param name='options'>Specifies options to control writing the CBOR
        /// object to JSON.</param>
        /// <returns>A text string containing the converted object in JSON
        /// format.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='options'/> is null.</exception>
        public string ToJSONString(JSONOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            CBORType type = this.Type;
            switch (type)
            {
                case CBORType.Boolean:
                case CBORType.SimpleValue:
                    {
                        return this.IsTrue ? "true" : (this.IsFalse ? "false" : "null");
                    }
                case CBORType.Integer:
                    {
                        return this.AsEIntegerValue().ToString();
                    }
                case CBORType.FloatingPoint:
                    {
                        long dblbits = this.AsDoubleBits();
                        return CBORUtilities.DoubleBitsFinite(dblbits) ?
                             CBORUtilities.DoubleBitsToString(dblbits) : "null";
                    }
                default:
                    {
                        var sb = new StringBuilder();
                        try
                        {
                            CBORJsonWriter.WriteJSONToInternal(
                              this,
                              new StringOutput(sb),
                              options);
                        }
                        catch (IOException ex)
                        {
                            // This is truly exceptional
                            throw new InvalidOperationException("Internal error", ex);
                        }
                        return sb.ToString();
                    }
            }
        }

        /// <summary>Returns this CBOR object in a text form intended to be
        /// read by humans. The value returned by this method is not intended
        /// to be parsed by computer programs, and the exact text of the value
        /// may change at any time between versions of this library.
        /// <para>The returned string is not necessarily in JavaScript Object
        /// Notation (JSON); to convert CBOR objects to JSON strings, use the
        /// <b>PeterO.Cbor.CBORObject.ToJSONString(PeterO.Cbor.JSONOptions)</b>
        /// method instead.</para></summary>
        /// <returns>A text representation of this object.</returns>
        public override string ToString()
        {
            return CBORDataUtilities.ToStringHelper(this, 0);
        }

        /// <summary>Gets an object with the same value as this one but without
        /// the tags it has, if any. If this object is an array, map, or byte
        /// string, the data will not be copied to the returned object, so
        /// changes to the returned object will be reflected in this
        /// one.</summary>
        /// <returns>A CBOR object.</returns>
        public CBORObject Untag()
        {
            CBORObject curobject = this;
            while (curobject.itemtypeValue == CBORObjectTypeTagged)
            {
                curobject = (CBORObject)curobject.itemValue;
            }
            return curobject;
        }

        /// <summary>Gets an object with the same value as this one but without
        /// this object's outermost tag, if any. If this object is an array,
        /// map, or byte string, the data will not be copied to the returned
        /// object, so changes to the returned object will be reflected in this
        /// one.</summary>
        /// <returns>A CBOR object.</returns>
        public CBORObject UntagOne()
        {
            return (this.itemtypeValue == CBORObjectTypeTagged) ?
              ((CBORObject)this.itemValue) : this;
        }

        /// <summary>Converts this object to a text string in JavaScript Object
        /// Notation (JSON) format, as in the ToJSONString method, and writes
        /// that string to a data stream in UTF-8. If the CBOR object contains
        /// CBOR maps, or is a CBOR map, the keys to the map are written out to
        /// the JSON string in an undefined order. The example code given in
        /// <b>PeterO.Cbor.CBORObject.ToJSONString(PeterO.Cbor.JSONOptions)</b>
        /// can be used to write out certain keys of a CBOR map in a given
        /// order to a JSON string.</summary>
        /// <param name='outputStream'>A writable data stream.</param>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='outputStream'/> is null.</exception>
        /// <example>
        /// <para>The following example (originally written in C# for the.NET
        /// version) writes out a CBOR object as part of a JSON text sequence
        /// (RFC 7464).</para>
        /// <code>
        /// stream.WriteByte(0x1e); &#x2f;&#x2a; RS &#x2a;&#x2f;
        /// cborObject.WriteJSONTo(stream); &#x2f;&#x2a; JSON &#x2a;&#x2f;
        /// stream.WriteByte(0x0a); &#x2f;&#x2a; LF &#x2a;&#x2f;
        /// </code>
        /// <para>The following example (originally written in C# for the.NET
        /// version) shows how to use the <c>LimitedMemoryStream</c>
        ///  class
        /// (implemented in <i>LimitedMemoryStream.cs</i>
        ///  in the peteroupc/CBOR
        /// open-source repository) to limit the size of supported JSON
        /// serializations of CBOR objects.</para>
        /// <code>
        /// &#x2f;&#x2a; maximum supported JSON size in bytes&#x2a;&#x2f;
        /// var maxSize = 20000;
        /// using (var ms = new LimitedMemoryStream(maxSize)) {
        /// cborObject.WriteJSONTo(ms);
        /// var bytes = ms.ToArray();
        /// }
        /// </code>
        /// <para>The following example (written in Java for the Java version)
        /// shows how to use a subclassed <c>OutputStream</c>
        ///  together with a
        /// <c>ByteArrayOutputStream</c>
        ///  to limit the size of supported JSON
        /// serializations of CBOR objects.</para>
        /// <code>
        /// &#x2f;&#x2a; maximum supported JSON size in bytes&#x2a;&#x2f;
        /// final int maxSize = 20000;
        /// ByteArrayOutputStream ba = new ByteArrayOutputStream();
        /// &#x2f;&#x2a; throws UnsupportedOperationException if too big&#x2a;&#x2f;
        /// cborObject.WriteJSONTo(new FilterOutputStream(ba) {
        /// private int size = 0;
        /// public void write(byte[] b, int off, int len) throws IOException {
        /// if (len>(maxSize-size)) {
        /// throw new UnsupportedOperationException();
        /// }
        /// size+=len; out.write(b, off, len);
        /// }
        /// public void write(byte b) throws IOException {
        /// if (size >= maxSize) {
        /// throw new UnsupportedOperationException();
        /// }
        /// size++; out.write(b);
        /// }
        /// });
        /// byte[] bytes = ba.toByteArray();
        /// </code>
        /// <para>The following example (originally written in C# for the.NET
        /// version) shows how to use a.NET MemoryStream to limit the size of
        /// supported JSON serializations of CBOR objects. The disadvantage is
        /// that the extra memory needed to do so can be wasteful, especially
        /// if the average serialized object is much smaller than the maximum
        /// size given (for example, if the maximum size is 20000 bytes, but
        /// the average serialized object has a size of 50 bytes).</para>
        /// <code>
        /// var backing = new byte[20000]; &#x2f;&#x2a; maximum supported JSON size in
        /// bytes&#x2a;&#x2f;
        /// byte[] bytes1, bytes2;
        /// using (var ms = new MemoryStream(backing)) {
        /// &#x2f;&#x2a; throws NotSupportedException if too big&#x2a;&#x2f;
        /// cborObject.WriteJSONTo(ms);
        /// bytes1 = new byte[ms.Position];
        /// &#x2f;&#x2a; Copy serialized data if successful&#x2a;&#x2f;
        /// System.ArrayCopy(backing, 0, bytes1, 0, (int)ms.Position);
        /// &#x2f;&#x2a; Reset memory stream&#x2a;&#x2f;
        /// ms.Position = 0;
        /// cborObject2.WriteJSONTo(ms);
        /// bytes2 = new byte[ms.Position];
        /// &#x2f;&#x2a; Copy serialized data if successful&#x2a;&#x2f;
        /// System.ArrayCopy(backing, 0, bytes2, 0, (int)ms.Position);
        /// }
        /// </code>
        /// </example>
        public void WriteJSONTo(Stream outputStream)
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }
            CBORJsonWriter.WriteJSONToInternal(
              this,
              new StringOutput(outputStream),
              JSONOptions.Default);
        }

        /// <summary>Converts this object to a text string in JavaScript Object
        /// Notation (JSON) format, as in the ToJSONString method, and writes
        /// that string to a data stream in UTF-8, using the given JSON options
        /// to control the encoding process. If the CBOR object contains CBOR
        /// maps, or is a CBOR map, the keys to the map are written out to the
        /// JSON string in an undefined order. The example code given in
        /// <b>PeterO.Cbor.CBORObject.ToJSONString(PeterO.Cbor.JSONOptions)</b>
        /// can be used to write out certain keys of a CBOR map in a given
        /// order to a JSON string.</summary>
        /// <param name='outputStream'>A writable data stream.</param>
        /// <param name='options'>An object containing the options to control
        /// writing the CBOR object to JSON.</param>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='outputStream'/> is null.</exception>
        public void WriteJSONTo(Stream outputStream, JSONOptions options)
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            CBORJsonWriter.WriteJSONToInternal(
              this,
              new StringOutput(outputStream),
              options);
        }

        /// <summary>Generates a CBOR object from a floating-point number
        /// represented by its bits.</summary>
        /// <param name='floatingBits'>The bits of a floating-point number
        /// number to write.</param>
        /// <param name='byteCount'>The number of bytes of the stored
        /// floating-point number; this also specifies the format of the
        /// "floatingBits" parameter. This value can be 2 if "floatingBits"'s
        /// lowest (least significant) 16 bits identify the floating-point
        /// number in IEEE 754r binary16 format; or 4 if "floatingBits"'s
        /// lowest (least significant) 32 bits identify the floating-point
        /// number in IEEE 754r binary32 format; or 8 if "floatingBits"
        /// identifies the floating point number in IEEE 754r binary64 format.
        /// Any other values for this parameter are invalid.</param>
        /// <returns>A CBOR object storing the given floating-point
        /// number.</returns>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='byteCount'/> is other than 2, 4, or 8.</exception>
        public static CBORObject FromFloatingPointBits(
          long floatingBits,
          int byteCount)
        {
            long value;
            switch (byteCount)
            {
                case 2:
                    value = CBORUtilities.HalfToDoublePrecision(
                        unchecked((int)(floatingBits & 0xffffL)));
                    return new CBORObject(CBORObjectTypeDouble, value);
                case 4:

                    value = CBORUtilities.SingleToDoublePrecision(
                        unchecked((int)(floatingBits & 0xffffffffL)));
                    return new CBORObject(CBORObjectTypeDouble, value);
                case 8:
                    return new CBORObject(CBORObjectTypeDouble, floatingBits);
                default: throw new ArgumentOutOfRangeException(nameof(byteCount));
            }
        }

        /// <summary>Writes the bits of a floating-point number in CBOR format
        /// to a data stream.</summary>
        /// <param name='outputStream'>A writable data stream.</param>
        /// <param name='floatingBits'>The bits of a floating-point number
        /// number to write.</param>
        /// <param name='byteCount'>The number of bytes of the stored
        /// floating-point number; this also specifies the format of the
        /// "floatingBits" parameter. This value can be 2 if "floatingBits"'s
        /// lowest (least significant) 16 bits identify the floating-point
        /// number in IEEE 754r binary16 format; or 4 if "floatingBits"'s
        /// lowest (least significant) 32 bits identify the floating-point
        /// number in IEEE 754r binary32 format; or 8 if "floatingBits"
        /// identifies the floating point number in IEEE 754r binary64 format.
        /// Any other values for this parameter are invalid. This method will
        /// write one plus this many bytes to the data stream.</param>
        /// <returns>The number of 8-bit bytes ordered to be written to the
        /// data stream.</returns>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='byteCount'/> is other than 2, 4, or 8.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='outputStream'/> is null.</exception>
        public static int WriteFloatingPointBits(
          Stream outputStream,
          long floatingBits,
          int byteCount)
        {
            return WriteFloatingPointBits(
                outputStream,
                floatingBits,
                byteCount,
                false);
        }

        /// <summary>Writes the bits of a floating-point number in CBOR format
        /// to a data stream.</summary>
        /// <param name='outputStream'>A writable data stream.</param>
        /// <param name='floatingBits'>The bits of a floating-point number
        /// number to write.</param>
        /// <param name='byteCount'>The number of bytes of the stored
        /// floating-point number; this also specifies the format of the
        /// "floatingBits" parameter. This value can be 2 if "floatingBits"'s
        /// lowest (least significant) 16 bits identify the floating-point
        /// number in IEEE 754r binary16 format; or 4 if "floatingBits"'s
        /// lowest (least significant) 32 bits identify the floating-point
        /// number in IEEE 754r binary32 format; or 8 if "floatingBits"
        /// identifies the floating point number in IEEE 754r binary64 format.
        /// Any other values for this parameter are invalid.</param>
        /// <param name='shortestForm'>If true, writes the shortest form of the
        /// floating-point number that preserves its value. If false, this
        /// method will write the number in the form given by 'floatingBits' by
        /// writing one plus the number of bytes given by 'byteCount' to the
        /// data stream.</param>
        /// <returns>The number of 8-bit bytes ordered to be written to the
        /// data stream.</returns>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='byteCount'/> is other than 2, 4, or 8.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='outputStream'/> is null.</exception>
        public static int WriteFloatingPointBits(
          Stream outputStream,
          long floatingBits,
          int byteCount,
          bool shortestForm)
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }
            if (shortestForm)
            {
                if (byteCount == 8)
                {
                    int bits =
                      CBORUtilities.DoubleToHalfPrecisionIfSameValue(floatingBits);
                    if (bits != -1)
                    {
                        return WriteFloatingPointBits(outputStream, (long)bits, 2, false);
                    }
                    if (CBORUtilities.DoubleRetainsSameValueInSingle(floatingBits))
                    {
                        bits = CBORUtilities.DoubleToRoundedSinglePrecision(floatingBits);
                        return WriteFloatingPointBits(outputStream, (long)bits, 4, false);
                    }
                }
                else if (byteCount == 4)
                {
                    int bits =
                      CBORUtilities.SingleToHalfPrecisionIfSameValue(floatingBits);
                    if (bits != -1)
                    {
                        return WriteFloatingPointBits(outputStream, (long)bits, 2, false);
                    }
                }
            }
            byte[] bytes;
            switch (byteCount)
            {
                case 2:
                    bytes = new byte[] {
            (byte)0xf9,
            (byte)((floatingBits >> 8) & 0xffL),
            (byte)(floatingBits & 0xffL),
          };
                    outputStream.Write(bytes, 0, 3);
                    return 3;
                case 4:
                    bytes = new byte[] {
            (byte)0xfa,
            (byte)((floatingBits >> 24) & 0xffL),
            (byte)((floatingBits >> 16) & 0xffL),
            (byte)((floatingBits >> 8) & 0xffL),
            (byte)(floatingBits & 0xffL),
          };
                    outputStream.Write(bytes, 0, 5);
                    return 5;
                case 8:
                    bytes = new byte[] {
            (byte)0xfb,
            (byte)((floatingBits >> 56) & 0xffL),
            (byte)((floatingBits >> 48) & 0xffL),
            (byte)((floatingBits >> 40) & 0xffL),
            (byte)((floatingBits >> 32) & 0xffL),
            (byte)((floatingBits >> 24) & 0xffL),
            (byte)((floatingBits >> 16) & 0xffL),
            (byte)((floatingBits >> 8) & 0xffL),
            (byte)(floatingBits & 0xffL),
          };
                    outputStream.Write(bytes, 0, 9);
                    return 9;
                default:
                    throw new ArgumentOutOfRangeException(nameof(byteCount));
            }
        }

        /// <summary>Writes a 64-bit binary floating-point number in CBOR
        /// format to a data stream, either in its 64-bit form, or its rounded
        /// 32-bit or 16-bit equivalent.</summary>
        /// <param name='outputStream'>A writable data stream.</param>
        /// <param name='doubleVal'>The double-precision floating-point number
        /// to write.</param>
        /// <param name='byteCount'>The number of 8-bit bytes of the stored
        /// number. This value can be 2 to store the number in IEEE 754r
        /// binary16, rounded to nearest, ties to even; or 4 to store the
        /// number in IEEE 754r binary32, rounded to nearest, ties to even; or
        /// 8 to store the number in IEEE 754r binary64. Any other values for
        /// this parameter are invalid.</param>
        /// <returns>The number of 8-bit bytes ordered to be written to the
        /// data stream.</returns>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='byteCount'/> is other than 2, 4, or 8.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='outputStream'/> is null.</exception>
        public static int WriteFloatingPointValue(
          Stream outputStream,
          double doubleVal,
          int byteCount)
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }
            long bits = 0;
            switch (byteCount)
            {
                case 2:
                    bits = CBORUtilities.DoubleToInt64Bits(doubleVal);
                    bits = CBORUtilities.DoubleToRoundedHalfPrecision(bits);
                    bits &= 0xffffL;
                    return WriteFloatingPointBits(outputStream, bits, 2);
                case 4:
                    bits = CBORUtilities.DoubleToInt64Bits(doubleVal);
                    bits = CBORUtilities.DoubleToRoundedSinglePrecision(bits);
                    bits &= 0xffffffffL;
                    return WriteFloatingPointBits(outputStream, bits, 4);
                case 8:
                    bits = CBORUtilities.DoubleToInt64Bits(doubleVal);
                    return WriteFloatingPointBits(outputStream, bits, 8);
                default: throw new ArgumentOutOfRangeException(nameof(byteCount));
            }
        }

        /// <summary>Writes a 32-bit binary floating-point number in CBOR
        /// format to a data stream, either in its 64- or 32-bit form, or its
        /// rounded 16-bit equivalent.</summary>
        /// <param name='outputStream'>A writable data stream.</param>
        /// <param name='singleVal'>The single-precision floating-point number
        /// to write.</param>
        /// <param name='byteCount'>The number of 8-bit bytes of the stored
        /// number. This value can be 2 to store the number in IEEE 754r
        /// binary16, rounded to nearest, ties to even; or 4 to store the
        /// number in IEEE 754r binary32; or 8 to store the number in IEEE 754r
        /// binary64. Any other values for this parameter are invalid.</param>
        /// <returns>The number of 8-bit bytes ordered to be written to the
        /// data stream.</returns>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='byteCount'/> is other than 2, 4, or 8.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='outputStream'/> is null.</exception>
        public static int WriteFloatingPointValue(
          Stream outputStream,
          float singleVal,
          int byteCount)
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }
            var bits = 0;
            long longbits = 0L;
            switch (byteCount)
            {
                case 2:
                    bits = BitConverter.ToInt32(
                        BitConverter.GetBytes((float)singleVal),
                        0);
                    bits = CBORUtilities.SingleToRoundedHalfPrecision(bits);
                    bits &= 0xffff;
                    return WriteFloatingPointBits(outputStream, bits, 2);
                case 4:
                    bits = BitConverter.ToInt32(
                        BitConverter.GetBytes((float)singleVal),
                        0);
                    longbits = ((long)bits) & 0xffffffffL;
                    return WriteFloatingPointBits(outputStream, longbits, 4);
                case 8:
                    bits = BitConverter.ToInt32(
                        BitConverter.GetBytes((float)singleVal),
                        0);
                    longbits = CBORUtilities.SingleToDoublePrecision(bits);
                    return WriteFloatingPointBits(outputStream, longbits, 8);
                default: throw new ArgumentOutOfRangeException(nameof(byteCount));
            }
        }

        /// <summary>Writes a CBOR major type number and an integer 0 or
        /// greater associated with it to a data stream, where that integer is
        /// passed to this method as a 64-bit signed integer. This is a
        /// low-level method that is useful for implementing custom CBOR
        /// encoding methodologies. This method encodes the given major type
        /// and value in the shortest form allowed for the major
        /// type.</summary>
        /// <param name='outputStream'>A writable data stream.</param>
        /// <param name='majorType'>The CBOR major type to write. This is a
        /// number from 0 through 7 as follows. 0: integer 0 or greater; 1:
        /// negative integer; 2: byte string; 3: UTF-8 text string; 4: array;
        /// 5: map; 6: tag; 7: simple value. See RFC 7049 for details on these
        /// major types.</param>
        /// <param name='value'>An integer 0 or greater associated with the
        /// major type, as follows. 0: integer 0 or greater; 1: the negative
        /// integer's absolute value is 1 plus this number; 2: length in bytes
        /// of the byte string; 3: length in bytes of the UTF-8 text string; 4:
        /// number of items in the array; 5: number of key-value pairs in the
        /// map; 6: tag number; 7: simple value number, which must be in the
        /// interval [0, 23] or [32, 255].</param>
        /// <returns>The number of bytes ordered to be written to the data
        /// stream.</returns>
        /// <exception cref='ArgumentException'>Value is from 24 to 31 and
        /// major type is 7.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='outputStream'/> is null.</exception>
        public static int WriteValue(
          Stream outputStream,
          int majorType,
          long value)
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }
            if (majorType < 0)
            {
                throw new ArgumentException("majorType(" + majorType +
                  ") is less than 0");
            }
            if (majorType > 7)
            {
                throw new ArgumentException("majorType(" + majorType +
                  ") is more than 7");
            }
            if (value < 0)
            {
                throw new ArgumentException("value(" + value +
                  ") is less than 0");
            }
            if (majorType == 7)
            {
                if (value > 255)
                {
                    throw new ArgumentException("value(" + value +
                      ") is more than 255");
                }
                if (value <= 23)
                {
                    outputStream.WriteByte((byte)(0xe0 + (int)value));
                    return 1;
                }
                else if (value < 32)
                {
                    throw new ArgumentException("value is from 24 to 31 and major" +
                      " type is 7");
                }
                else
                {
                    outputStream.WriteByte((byte)0xf8);
                    outputStream.WriteByte((byte)value);
                    return 2;
                }
            }
            else
            {
                return WritePositiveInt64(majorType, value, outputStream);
            }
        }

        /// <summary>Writes a CBOR major type number and an integer 0 or
        /// greater associated with it to a data stream, where that integer is
        /// passed to this method as a 32-bit signed integer. This is a
        /// low-level method that is useful for implementing custom CBOR
        /// encoding methodologies. This method encodes the given major type
        /// and value in the shortest form allowed for the major
        /// type.</summary>
        /// <param name='outputStream'>A writable data stream.</param>
        /// <param name='majorType'>The CBOR major type to write. This is a
        /// number from 0 through 7 as follows. 0: integer 0 or greater; 1:
        /// negative integer; 2: byte string; 3: UTF-8 text string; 4: array;
        /// 5: map; 6: tag; 7: simple value. See RFC 7049 for details on these
        /// major types.</param>
        /// <param name='value'>An integer 0 or greater associated with the
        /// major type, as follows. 0: integer 0 or greater; 1: the negative
        /// integer's absolute value is 1 plus this number; 2: length in bytes
        /// of the byte string; 3: length in bytes of the UTF-8 text string; 4:
        /// number of items in the array; 5: number of key-value pairs in the
        /// map; 6: tag number; 7: simple value number, which must be in the
        /// interval [0, 23] or [32, 255].</param>
        /// <returns>The number of bytes ordered to be written to the data
        /// stream.</returns>
        /// <exception cref='ArgumentException'>Value is from 24 to 31 and
        /// major type is 7.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='outputStream'/> is null.</exception>
        /// <example>
        /// <para>In the following example, an array of three objects is
        /// written as CBOR to a data stream.</para>
        /// <code>&#x2f;&#x2a; array, length 3&#x2a;&#x2f;
        /// CBORObject.WriteValue(stream, 4, 3);
        /// &#x2f;&#x2a; item 1 */
        /// CBORObject.Write("hello world", stream);
        /// CBORObject.Write(25, stream); &#x2f;&#x2a; item 2&#x2a;&#x2f;
        /// CBORObject.Write(false, stream); &#x2f;&#x2a; item 3&#x2a;&#x2f;</code>
        /// <para>In the following example, a map consisting of two key-value
        /// pairs is written as CBOR to a data stream.</para>
        /// <code>CBORObject.WriteValue(stream, 5, 2); &#x2f;&#x2a; map, 2
        /// pairs&#x2a;&#x2f;
        /// CBORObject.Write("number", stream); &#x2f;&#x2a; key 1 */
        /// CBORObject.Write(25, stream); &#x2f;&#x2a; value 1 */
        /// CBORObject.Write("string", stream); &#x2f;&#x2a; key 2&#x2a;&#x2f;
        /// CBORObject.Write("hello", stream); &#x2f;&#x2a; value 2&#x2a;&#x2f;</code>
        /// <para>In the following example (originally written in C# for
        /// the.NET Framework version), a text string is written as CBOR to a
        /// data stream.</para>
        /// <code>string str = "hello world"; byte[] bytes =
        /// DataUtilities.GetUtf8Bytes(str, true); CBORObject.WriteValue(stream, 4,
        /// bytes.Length); stream.Write(bytes, 0, bytes.Length);</code>
        ///  .
        /// </example>
        public static int WriteValue(
          Stream outputStream,
          int majorType,
          int value)
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }
            if (majorType < 0)
            {
                throw new ArgumentException("majorType(" + majorType +
                  ") is less than 0");
            }
            if (majorType > 7)
            {
                throw new ArgumentException("majorType(" + majorType +
                  ") is more than 7");
            }
            if (value < 0)
            {
                throw new ArgumentException("value(" + value +
                  ") is less than 0");
            }
            if (majorType == 7)
            {
                if (value > 255)
                {
                    throw new ArgumentException("value(" + value +
                      ") is more than 255");
                }
                if (value <= 23)
                {
                    outputStream.WriteByte((byte)(0xe0 + value));
                    return 1;
                }
                else if (value < 32)
                {
                    throw new ArgumentException("value is from 24 to 31 and major" +
                      "\u0020type" + "\u0020is 7");
                }
                else
                {
                    outputStream.WriteByte((byte)0xf8);
                    outputStream.WriteByte((byte)value);
                    return 2;
                }
            }
            else
            {
                return WritePositiveInt(majorType, value, outputStream);
            }
        }

        /// <summary>Writes a CBOR major type number and an integer 0 or
        /// greater associated with it to a data stream, where that integer is
        /// passed to this method as an arbitrary-precision integer. This is a
        /// low-level method that is useful for implementing custom CBOR
        /// encoding methodologies. This method encodes the given major type
        /// and value in the shortest form allowed for the major
        /// type.</summary>
        /// <param name='outputStream'>A writable data stream.</param>
        /// <param name='majorType'>The CBOR major type to write. This is a
        /// number from 0 through 7 as follows. 0: integer 0 or greater; 1:
        /// negative integer; 2: byte string; 3: UTF-8 text string; 4: array;
        /// 5: map; 6: tag; 7: simple value. See RFC 7049 for details on these
        /// major types.</param>
        /// <param name='bigintValue'>An integer 0 or greater associated with
        /// the major type, as follows. 0: integer 0 or greater; 1: the
        /// negative integer's absolute value is 1 plus this number; 2: length
        /// in bytes of the byte string; 3: length in bytes of the UTF-8 text
        /// string; 4: number of items in the array; 5: number of key-value
        /// pairs in the map; 6: tag number; 7: simple value number, which must
        /// be in the interval [0, 23] or [32, 255]. For major types 0 to 6,
        /// this number may not be greater than 2^64 - 1.</param>
        /// <returns>The number of bytes ordered to be written to the data
        /// stream.</returns>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='majorType'/> is 7 and value is greater than 255.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='outputStream'/> or <paramref name='bigintValue'/> is
        /// null.</exception>
        public static int WriteValue(
          Stream outputStream,
          int majorType,
          EInteger bigintValue)
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }
            if (bigintValue == null)
            {
                throw new ArgumentNullException(nameof(bigintValue));
            }
            if (bigintValue.Sign < 0)
            {
                throw new ArgumentException("tagEInt's sign(" + bigintValue.Sign +
                  ") is less than 0");
            }
            if (bigintValue.CompareTo(UInt64MaxValue) > 0)
            {
                throw new ArgumentException(
                  "tag more than 18446744073709551615 (" + bigintValue + ")");
            }
            if (bigintValue.CanFitInInt64())
            {
                return WriteValue(
                    outputStream,
                    majorType,
                    bigintValue.ToInt64Checked());
            }
            long longVal = bigintValue.ToInt64Unchecked();
            var highbyte = (int)((longVal >> 56) & 0xff);
            if (majorType < 0)
            {
                throw new ArgumentException("majorType(" + majorType +
                  ") is less than 0");
            }
            if (majorType > 7)
            {
                throw new ArgumentException("majorType(" + majorType +
                  ") is more than 7");
            }
            if (majorType == 7)
            {
                throw new ArgumentException(
                  "majorType is 7 and value is greater" + "\u0020than 255");
            }
            byte[] bytes = new[] {
        (byte)(27 | (majorType << 5)), (byte)highbyte,
        (byte)((longVal >> 48) & 0xff), (byte)((longVal >> 40) & 0xff),
        (byte)((longVal >> 32) & 0xff), (byte)((longVal >> 24) & 0xff),
        (byte)((longVal >> 16) & 0xff), (byte)((longVal >> 8) & 0xff),
        (byte)(longVal & 0xff),
      };
            outputStream.Write(bytes, 0, bytes.Length);
            return bytes.Length;
        }

        /// <summary><para>Writes this CBOR object to a data stream. If the
        /// CBOR object contains CBOR maps, or is a CBOR map, the keys to the
        /// map are written out to the data stream in an undefined order. See
        /// the examples (originally written in C# for the.NET version) for
        /// ways to write out certain keys of a CBOR map in a given order. In
        /// the case of CBOR objects of type FloatingPoint, the number is
        /// written using the shortest floating-point encoding possible; this
        /// is a change from previous versions.</para>
        ///  </summary>
        /// <param name='stream'>A writable data stream.</param>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        /// <example>
        /// <para>The following example shows a method that writes each key of
        /// 'mapObj' to 'outputStream', in the order given in 'keys', where
        /// 'mapObj' is written out in the form of a CBOR <b>definite-length
        /// map</b>
        /// . Only keys found in 'keys' will be written if they exist
        /// in 'mapObj'.</para>
        /// <code>private static void WriteKeysToMap(CBORObject mapObj,
        /// IList&lt;CBORObject&gt; keys, Stream outputStream) { if (mapObj == null)
        /// { throw new ArgumentNullException(nameof(mapObj));}
        /// if (keys == null)
        /// {throw new ArgumentNullException(nameof(keys));}
        /// if (outputStream ==
        /// null) {throw new ArgumentNullException(nameof(outputStream));}
        /// if
        /// (obj.Type!=CBORType.Map) { throw new ArgumentException("'obj' is not a
        /// map."); } int keyCount = 0; for (CBORObject key in keys) { if
        /// (mapObj.ContainsKey(key)) { keyCount++; } }
        /// CBORObject.WriteValue(outputStream, 5, keyCount); for (CBORObject key in
        /// keys) { if (mapObj.ContainsKey(key)) { key.WriteTo(outputStream);
        /// mapObj[key].WriteTo(outputStream); } } }</code>
        /// <para>The following example shows a method that writes each key of
        /// 'mapObj' to 'outputStream', in the order given in 'keys', where
        /// 'mapObj' is written out in the form of a CBOR <b>indefinite-length
        /// map</b>
        /// . Only keys found in 'keys' will be written if they exist
        /// in 'mapObj'.</para>
        /// <code>private static void WriteKeysToIndefMap(CBORObject mapObj,
        /// IList&lt;CBORObject&gt; keys, Stream outputStream) { if (mapObj == null)
        /// { throw new ArgumentNullException(nameof(mapObj));}
        /// if (keys == null)
        /// {throw new ArgumentNullException(nameof(keys));}
        /// if (outputStream ==
        /// null) {throw new ArgumentNullException(nameof(outputStream));}
        /// if
        /// (obj.Type!=CBORType.Map) { throw new ArgumentException("'obj' is not a
        /// map."); } outputStream.WriteByte((byte)0xBF); for (CBORObject key in
        /// keys) { if (mapObj.ContainsKey(key)) { key.WriteTo(outputStream);
        /// mapObj[key].WriteTo(outputStream); } }
        /// outputStream.WriteByte((byte)0xff); }</code>
        /// <para>The following example shows a method that writes out a list
        /// of objects to 'outputStream' as an <b>indefinite-length CBOR
        /// array</b>
        /// .</para>
        /// <code>private static void WriteToIndefArray(IList&lt;object&gt; list,
        /// Stream
        /// outputStream) { if (list == null) { throw new
        /// ArgumentNullException(nameof(list));}
        /// if (outputStream == null) {throw
        /// new ArgumentNullException(nameof(outputStream));}
        /// outputStream.WriteByte((byte)0x9f); for (object item in list) { new
        /// CBORObject(item).WriteTo(outputStream); }
        /// outputStream.WriteByte((byte)0xff); }</code>
        /// <para>The following example (originally written in C# for the.NET
        /// version) shows how to use the <c>LimitedMemoryStream</c>
        ///  class
        /// (implemented in <i>LimitedMemoryStream.cs</i>
        ///  in the peteroupc/CBOR
        /// open-source repository) to limit the size of supported CBOR
        /// serializations.</para>
        /// <code>
        /// &#x2f;&#x2a; maximum supported CBOR size in bytes&#x2a;&#x2f;
        /// var maxSize = 20000;
        /// using (var ms = new LimitedMemoryStream(maxSize)) {
        /// cborObject.WriteTo(ms);
        /// var bytes = ms.ToArray();
        /// }
        /// </code>
        /// <para>The following example (written in Java for the Java version)
        /// shows how to use a subclassed <c>OutputStream</c>
        ///  together with a
        /// <c>ByteArrayOutputStream</c>
        ///  to limit the size of supported CBOR
        /// serializations.</para>
        /// <code>
        /// &#x2f;&#x2a; maximum supported CBOR size in bytes&#x2a;&#x2f;
        /// final int maxSize = 20000;
        /// ByteArrayOutputStream ba = new ByteArrayOutputStream();
        /// &#x2f;&#x2a; throws UnsupportedOperationException if too big&#x2a;&#x2f;
        /// cborObject.WriteTo(new FilterOutputStream(ba) {
        /// private int size = 0;
        /// public void write(byte[] b, int off, int len) throws IOException {
        /// if (len>(maxSize-size)) {
        /// throw new UnsupportedOperationException();
        /// }
        /// size+=len; out.write(b, off, len);
        /// }
        /// public void write(byte b) throws IOException {
        /// if (size >= maxSize) {
        /// throw new UnsupportedOperationException();
        /// }
        /// size++; out.write(b);
        /// }
        /// });
        /// byte[] bytes = ba.toByteArray();
        /// </code>
        /// <para>The following example (originally written in C# for the.NET
        /// version) shows how to use a.NET MemoryStream to limit the size of
        /// supported CBOR serializations. The disadvantage is that the extra
        /// memory needed to do so can be wasteful, especially if the average
        /// serialized object is much smaller than the maximum size given (for
        /// example, if the maximum size is 20000 bytes, but the average
        /// serialized object has a size of 50 bytes).</para>
        /// <code>
        /// var backing = new byte[20000]; &#x2f;&#x2a; maximum supported CBOR size in
        /// bytes&#x2a;&#x2f;
        /// byte[] bytes1, bytes2;
        /// using (var ms = new MemoryStream(backing)) {
        /// &#x2f;&#x2a; throws NotSupportedException if too big&#x2a;&#x2f;
        /// cborObject.WriteTo(ms);
        /// bytes1 = new byte[ms.Position];
        /// &#x2f;&#x2a; Copy serialized data if successful&#x2a;&#x2f;
        /// System.ArrayCopy(backing, 0, bytes1, 0, (int)ms.Position);
        /// &#x2f;&#x2a; Reset memory stream&#x2a;&#x2f;
        /// ms.Position = 0;
        /// cborObject2.WriteTo(ms);
        /// bytes2 = new byte[ms.Position];
        /// &#x2f;&#x2a; Copy serialized data if successful&#x2a;&#x2f;
        /// System.ArrayCopy(backing, 0, bytes2, 0, (int)ms.Position);
        /// }
        /// </code>
        /// </example>
        public void WriteTo(Stream stream)
        {
            this.WriteTo(stream, CBOREncodeOptions.Default);
        }

        /// <summary>Writes this CBOR object to a data stream, using the
        /// specified options for encoding the data to CBOR format. If the CBOR
        /// object contains CBOR maps, or is a CBOR map, the keys to the map
        /// are written out to the data stream in an undefined order. The
        /// example code given in
        /// <see cref='PeterO.Cbor.CBORObject.WriteTo(System.IO.Stream)'/> can
        /// be used to write out certain keys of a CBOR map in a given order.
        /// In the case of CBOR objects of type FloatingPoint, the number is
        /// written using the shortest floating-point encoding possible; this
        /// is a change from previous versions.</summary>
        /// <param name='stream'>A writable data stream.</param>
        /// <param name='options'>Options for encoding the data to
        /// CBOR.</param>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        /// <exception cref='System.IO.IOException'>An I/O error
        /// occurred.</exception>
        /// <exception cref='ArgumentException'>Unexpected data
        /// type".</exception>
        public void WriteTo(Stream stream, CBOREncodeOptions options)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (options.Ctap2Canonical)
            {
                byte[] bytes = CBORCanonical.CtapCanonicalEncode(this);
                stream.Write(bytes, 0, bytes.Length);
                return;
            }
            this.WriteTags(stream);
            int type = this.ItemType;
            switch (type)
            {
                case CBORObjectTypeInteger:
                    {
                        Write((long)this.ThisItem, stream);
                        break;
                    }
                case CBORObjectTypeEInteger:
                    {
                        Write((EInteger)this.ThisItem, stream);
                        break;
                    }
                case CBORObjectTypeByteString:
                case CBORObjectTypeTextStringUtf8:
                    {
                        byte[] arr = (byte[])this.ThisItem;
                        WritePositiveInt(
                          (this.Type == CBORType.ByteString) ? 2 : 3,
                          arr.Length,
                          stream);
                        stream.Write(arr, 0, arr.Length);
                        break;
                    }
                case CBORObjectTypeTextString:
                    {
                        Write((string)this.ThisItem, stream, options);
                        break;
                    }
                case CBORObjectTypeArray:
                    {
                        WriteObjectArray(this.AsList(), stream, options);
                        break;
                    }
                case CBORObjectTypeMap:
                    {
                        WriteObjectMap(this.AsMap(), stream, options);
                        break;
                    }
                case CBORObjectTypeSimpleValue:
                    {
                        int value = this.SimpleValue;
                        if (value < 24)
                        {
                            stream.WriteByte((byte)(0xe0 + value));
                        }
                        else
                        {
#if DEBUG
                            if (value < 32)
                            {
                                throw new ArgumentException("value(" + value +
                                  ") is less than " + "32");
                            }
#endif

                            stream.WriteByte(0xf8);
                            stream.WriteByte((byte)value);
                        }

                        break;
                    }
                case CBORObjectTypeDouble:
                    {
                        WriteFloatingPointBits(stream, this.AsDoubleBits(), 8, true);
                        break;
                    }
                default:
                    {
                        throw new ArgumentException("Unexpected data type");
                    }
            }
        }

        internal static CBORObject FromRaw(byte[] bytes)
        {
            return new CBORObject(CBORObjectTypeByteString, bytes);
        }

        internal static CBORObject FromRawUtf8(byte[] bytes)
        {
            return new CBORObject(CBORObjectTypeTextStringUtf8, bytes);
        }

        internal static CBORObject FromRaw(string str)
        {
#if DEBUG
            if (!CBORUtilities.CheckUtf16(str))
            {
                throw new InvalidOperationException();
            }
#endif
            return new CBORObject(CBORObjectTypeTextString, str);
        }

        internal static CBORObject FromRaw(IList<CBORObject> list)
        {
            return new CBORObject(CBORObjectTypeArray, list);
        }

        internal static CBORObject FromRaw(IDictionary<CBORObject, CBORObject>
          map)
        {
#if DEBUG
            if (!(map is SortedDictionary<CBORObject, CBORObject>))
            {
                throw new InvalidOperationException();
            }
#endif
            return new CBORObject(CBORObjectTypeMap, map);
        }

        internal static int GetExpectedLength(int value)
        {
            return ValueExpectedLengths[value];
        }

        // Generate a CBOR object for head bytes with fixed length.
        // Note that this function assumes that the length of the data
        // was already checked.
        internal static CBORObject GetFixedLengthObject(
          int firstbyte,
          byte[] data)
        {
            CBORObject fixedObj = FixedObjects[firstbyte];
            if (fixedObj != null)
            {
                return fixedObj;
            }
            int majortype = firstbyte >> 5;
            if ((firstbyte & 0x1c) == 0x18)
            {
                // contains 1 to 8 extra bytes of additional information
                long uadditional = 0;
                switch (firstbyte & 0x1f)
                {
                    case 24:
                        uadditional = (int)(data[1] & (int)0xff);
                        break;
                    case 25:
                        uadditional = (data[1] & 0xffL) << 8;
                        uadditional |= (long)(data[2] & 0xffL);
                        break;
                    case 26:
                        uadditional = (data[1] & 0xffL) << 24;
                        uadditional |= (data[2] & 0xffL) << 16;
                        uadditional |= (data[3] & 0xffL) << 8;
                        uadditional |= (long)(data[4] & 0xffL);
                        break;
                    case 27:
                        uadditional = (data[1] & 0xffL) << 56;
                        uadditional |= (data[2] & 0xffL) << 48;
                        uadditional |= (data[3] & 0xffL) << 40;
                        uadditional |= (data[4] & 0xffL) << 32;
                        uadditional |= (data[5] & 0xffL) << 24;
                        uadditional |= (data[6] & 0xffL) << 16;
                        uadditional |= (data[7] & 0xffL) << 8;
                        uadditional |= (long)(data[8] & 0xffL);
                        break;
                    default:
                        throw new CBORException("Unexpected data encountered");
                }
                switch (majortype)
                {
                    case 0:
                        if ((uadditional >> 63) == 0)
                        {
                            // use only if additional's top bit isn't set
                            // (additional is a signed long)
                            return new CBORObject(CBORObjectTypeInteger, uadditional);
                        }
                        else
                        {
                            int low = unchecked((int)(uadditional & 0xffffffffL));
                            int high = unchecked((int)((uadditional >> 32) & 0xffffffffL));
                            return FromObject(LowHighToEInteger(low, high));
                        }
                    case 1:
                        if ((uadditional >> 63) == 0)
                        {
                            // use only if additional's top bit isn't set
                            // (additional is a signed long)
                            return new CBORObject(
                                CBORObjectTypeInteger,
                                -1 - uadditional);
                        }
                        else
                        {
                            int low = unchecked((int)(uadditional & 0xffffffffL));
                            int high = unchecked((int)((uadditional >> 32) & 0xffffffffL));
                            EInteger bigintAdditional = LowHighToEInteger(low, high);
                            EInteger minusOne = -EInteger.One;
                            bigintAdditional = minusOne - (EInteger)bigintAdditional;
                            return FromObject(bigintAdditional);
                        }
                    case 7:
                        if (firstbyte >= 0xf9 && firstbyte <= 0xfb)
                        {
                            var dblbits = (long)uadditional;
                            if (firstbyte == 0xf9)
                            {
                                dblbits = CBORUtilities.HalfToDoublePrecision(
                                    unchecked((int)uadditional));
                            }
                            else if (firstbyte == 0xfa)
                            {
                                dblbits = CBORUtilities.SingleToDoublePrecision(
                                    unchecked((int)uadditional));
                            }
                            return new CBORObject(
                                CBORObjectTypeDouble,
                                dblbits);
                        }
                        if (firstbyte == 0xf8)
                        {
                            if ((int)uadditional < 32)
                            {
                                throw new CBORException("Invalid overlong simple value");
                            }
                            return new CBORObject(
                                CBORObjectTypeSimpleValue,
                                (int)uadditional);
                        }
                        throw new CBORException("Unexpected data encountered");
                    default: throw new CBORException("Unexpected data encountered");
                }
            }
            if (majortype == 2)
            { // short byte string
                var ret = new byte[firstbyte - 0x40];
                Array.Copy(data, 1, ret, 0, firstbyte - 0x40);
                return new CBORObject(CBORObjectTypeByteString, ret);
            }
            if (majortype == 3)
            { // short text string
                var ret = new byte[firstbyte - 0x60];
                Array.Copy(data, 1, ret, 0, firstbyte - 0x60);
                if (!CBORUtilities.CheckUtf8(ret))
                {
                    throw new CBORException("Invalid encoding");
                }
                return new CBORObject(CBORObjectTypeTextStringUtf8, ret);
            }
            if (firstbyte == 0x80)
            {
                // empty array
                return CBORObject.NewArray();
            }
            if (firstbyte == 0xa0)
            {
                // empty map
                return CBORObject.NewMap();
            }
            throw new CBORException("Unexpected data encountered");
        }

        internal static CBORObject GetFixedObject(int value)
        {
            return FixedObjects[value];
        }

        private IList<CBORObject> AsList()
        {
            return (IList<CBORObject>)this.ThisItem;
        }

        private IDictionary<CBORObject, CBORObject> AsMap()
        {
            return (IDictionary<CBORObject, CBORObject>)this.ThisItem;
        }

        private static bool CBORArrayEquals(
          IList<CBORObject> listA,
          IList<CBORObject> listB)
        {
            if (listA == null)
            {
                return listB == null;
            }
            if (listB == null)
            {
                return false;
            }
            int listACount = listA.Count;
            int listBCount = listB.Count;
            if (listACount != listBCount)
            {
                return false;
            }
            for (var i = 0; i < listACount; ++i)
            {
                CBORObject itemA = listA[i];
                CBORObject itemB = listB[i];
                if (!(itemA == null ? itemB == null : itemA.Equals(itemB)))
                {
                    return false;
                }
            }
            return true;
        }

        private static int CBORArrayHashCode(IList<CBORObject> list)
        {
            if (list == null)
            {
                return 0;
            }
            var ret = 19;
            int count = list.Count;
            unchecked
            {
                ret = (ret * 31) + count;
                for (var i = 0; i < count; ++i)
                {
                    ret = (ret * 31) + list[i].GetHashCode();
                }
            }
            return ret;
        }

        private static bool StringEquals(string str, string str2)
        {
            if (str == str2)
            {
                return true;
            }
            if (str.Length != str2.Length)
            {
                return false;
            }
            int count = str.Length;
            for (var i = 0; i < count; ++i)
            {
                if (str[i] != str2[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static bool CBORMapEquals(
          IDictionary<CBORObject, CBORObject> mapA,
          IDictionary<CBORObject, CBORObject> mapB)
        {
            if (mapA == null)
            {
                return mapB == null;
            }
            if (mapB == null)
            {
                return false;
            }
            if (mapA.Count != mapB.Count)
            {
                return false;
            }
            foreach (KeyValuePair<CBORObject, CBORObject> kvp in mapA)
            {
                CBORObject valueB = null;
                bool hasKey = mapB.TryGetValue(kvp.Key, out valueB);
                if (hasKey)
                {
                    CBORObject valueA = kvp.Value;
                    if (!(valueA == null ? valueB == null : valueA.Equals(valueB)))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private static int CBORMapHashCode(IDictionary<CBORObject, CBORObject>
          a)
        {
            // To simplify matters, we use just the count of
            // the map as the basis for the hash code. More complicated
            // hash code calculation would involve the sum of the hash codes of
            // the map's key-value pairs (an approach that works regardless of the order
            // in which map keys are iterated, because wraparound addition
            // is commutative and associative), but this could take much more time
            // to calculate, especially if the keys and values are very big.
            return unchecked(a.Count.GetHashCode() * 19);
        }

        private static void CheckCBORLength(
          long expectedLength,
          long actualLength)
        {
            if (actualLength < expectedLength)
            {
                throw new CBORException("Premature end of data");
            }
            if (actualLength > expectedLength)
            {
                throw new CBORException("Too many bytes");
            }
        }

        private static void CheckCBORLength(int expectedLength, int
          actualLength)
        {
            if (actualLength < expectedLength)
            {
                throw new CBORException("Premature end of data");
            }
            if (actualLength > expectedLength)
            {
                throw new CBORException("Too many bytes");
            }
        }

        private static string ExtendedToString(EFloat ef)
        {
            if (ef.IsFinite && (ef.Exponent.CompareTo((EInteger)2500) > 0 ||
                ef.Exponent.CompareTo((EInteger)(-2500)) < 0))
            {
                // It can take very long to convert a number with a very high
                // or very low exponent to a decimal string, so do this instead
                return ef.Mantissa + "p" + ef.Exponent;
            }
            return ef.ToString();
        }

        private static byte[] GetOptimizedBytesIfShortAscii(
          string str,
          int tagbyteInt)
        {
            byte[] bytes;
            if (str.Length <= 255)
            {
                // The strings will usually be short ASCII strings, so
                // use this optimization
                var offset = 0;
                int length = str.Length;
                int extra = (length < 24) ? 1 : 2;
                if (tagbyteInt >= 0)
                {
                    ++extra;
                }
                bytes = new byte[length + extra];
                if (tagbyteInt >= 0)
                {
                    bytes[offset] = (byte)tagbyteInt;
                    ++offset;
                }
                if (length < 24)
                {
                    bytes[offset] = (byte)(0x60 + str.Length);
                    ++offset;
                }
                else
                {
                    bytes[offset] = (byte)0x78;
                    bytes[offset + 1] = (byte)str.Length;
                    offset += 2;
                }
                var issimple = true;
                for (var i = 0; i < str.Length; ++i)
                {
                    char c = str[i];
                    if (c >= 0x80)
                    {
                        issimple = false;
                        break;
                    }
                    bytes[i + offset] = unchecked((byte)c);
                }
                if (issimple)
                {
                    return bytes;
                }
            }
            return null;
        }

        private static string GetOptimizedStringIfShortAscii(
          byte[] data,
          int offset)
        {
            int length = data.Length;
            if (length > offset)
            {
                var nextbyte = (int)(data[offset] & (int)0xff);
                if (nextbyte >= 0x60 && nextbyte < 0x78)
                {
                    int offsetp1 = 1 + offset;
                    // Check for type 3 string of short length
                    int rightLength = offsetp1 + (nextbyte - 0x60);
                    CheckCBORLength(
                      rightLength,
                      length);
                    // Check for all ASCII text
                    for (int i = offsetp1; i < length; ++i)
                    {
                        if ((data[i] & ((byte)0x80)) != 0)
                        {
                            return null;
                        }
                    }
                    // All ASCII text, so convert to a text string
                    // from a char array without having to
                    // convert from UTF-8 first
                    var c = new char[length - offsetp1];
                    for (int i = offsetp1; i < length; ++i)
                    {
                        c[i - offsetp1] = (char)(data[i] & (int)0xff);
                    }
                    return new String(c);
                }
            }
            return null;
        }

        private static byte[] SerializeUtf8(byte[] utf8)
        {
            byte[] bytes;
            if (utf8.Length < 24)
            {
                bytes = new byte[utf8.Length + 1];
                bytes[0] = (byte)(utf8.Length | 0x60);
                Array.Copy(utf8, 0, bytes, 1, utf8.Length);
                return bytes;
            }
            if (utf8.Length <= 0xffL)
            {
                bytes = new byte[utf8.Length + 2];
                bytes[0] = (byte)0x78;
                bytes[1] = (byte)utf8.Length;
                Array.Copy(utf8, 0, bytes, 2, utf8.Length);
                return bytes;
            }
            if (utf8.Length <= 0xffffL)
            {
                bytes = new byte[utf8.Length + 3];
                bytes[0] = (byte)0x79;
                bytes[1] = (byte)((utf8.Length >> 8) & 0xff);
                bytes[2] = (byte)(utf8.Length & 0xff);
                Array.Copy(utf8, 0, bytes, 3, utf8.Length);
                return bytes;
            }
            byte[] posbytes = GetPositiveInt64Bytes(3, utf8.Length);
            bytes = new byte[utf8.Length + posbytes.Length];
            Array.Copy(posbytes, 0, bytes, 0, posbytes.Length);
            Array.Copy(utf8, 0, bytes, posbytes.Length, utf8.Length);
            return bytes;
        }

        private static byte[] GetPositiveInt64Bytes(int type, long value)
        {
            if (value < 0)
            {
                throw new ArgumentException("value(" + value + ") is less than " +
                  "0");
            }
            if (value < 24)
            {
                return new[] { (byte)((byte)value | (byte)(type << 5)) };
            }
            if (value <= 0xffL)
            {
                return new[] {
          (byte)(24 | (type << 5)), (byte)(value & 0xff),
        };
            }
            if (value <= 0xffffL)
            {
                return new[] {
          (byte)(25 | (type << 5)),
          (byte)((value >> 8) & 0xff), (byte)(value & 0xff),
        };
            }
            if (value <= 0xffffffffL)
            {
                return new[] {
          (byte)(26 | (type << 5)),
          (byte)((value >> 24) & 0xff), (byte)((value >> 16) & 0xff),
          (byte)((value >> 8) & 0xff), (byte)(value & 0xff),
        };
            }
            return new[] {
        (byte)(27 | (type << 5)), (byte)((value >> 56) & 0xff),
        (byte)((value >> 48) & 0xff), (byte)((value >> 40) & 0xff),
        (byte)((value >> 32) & 0xff), (byte)((value >> 24) & 0xff),
        (byte)((value >> 16) & 0xff), (byte)((value >> 8) & 0xff),
        (byte)(value & 0xff),
      };
        }

        private static byte[] GetPositiveIntBytes(int type, int value)
        {
            if (value < 0)
            {
                throw new ArgumentException("value(" + value + ") is less than " +
                  "0");
            }
            if (value < 24)
            {
                return new[] { (byte)((byte)value | (byte)(type << 5)) };
            }
            if (value <= 0xff)
            {
                return new[] {
          (byte)(24 | (type << 5)), (byte)(value & 0xff),
        };
            }
            if (value <= 0xffff)
            {
                return new[] {
          (byte)(25 | (type << 5)),
          (byte)((value >> 8) & 0xff), (byte)(value & 0xff),
        };
            }
            return new[] {
        (byte)(26 | (type << 5)), (byte)((value >> 24) & 0xff),
        (byte)((value >> 16) & 0xff), (byte)((value >> 8) & 0xff),
        (byte)(value & 0xff),
      };
        }

        // Initialize fixed values for certain
        // head bytes
        private static CBORObject[] InitializeFixedObjects()
        {
            var fixedObjects = new CBORObject[256];
            for (var i = 0; i < 0x18; ++i)
            {
                fixedObjects[i] = new CBORObject(CBORObjectTypeInteger, (long)i);
            }
            for (int i = 0x20; i < 0x38; ++i)
            {
                fixedObjects[i] = new CBORObject(
                  CBORObjectTypeInteger,
                  (long)(-1 - (i - 0x20)));
            }
            fixedObjects[0x60] = new CBORObject(
              CBORObjectTypeTextString,
              String.Empty);
            for (int i = 0xe0; i < 0xf8; ++i)
            {
                fixedObjects[i] = new CBORObject(
                  CBORObjectTypeSimpleValue,
                  (int)(i - 0xe0));
            }
            return fixedObjects;
        }

        private static int ListCompare(
          IList<CBORObject> listA,
          IList<CBORObject> listB)
        {
            if (listA == null)
            {
                return (listB == null) ? 0 : -1;
            }
            if (listB == null)
            {
                return 1;
            }
            int listACount = listA.Count;
            int listBCount = listB.Count;
            // NOTE: Compare list counts to conform
            // to bytewise lexicographical ordering
            if (listACount != listBCount)
            {
                return listACount < listBCount ? -1 : 1;
            }
            for (var i = 0; i < listACount; ++i)
            {
                int cmp = listA[i].CompareTo(listB[i]);
                if (cmp != 0)
                {
                    return cmp;
                }
            }
            return 0;
        }

        private static EInteger LowHighToEInteger(int tagLow, int tagHigh)
        {
            byte[] uabytes = null;
            if (tagHigh != 0)
            {
                uabytes = new byte[9];
                uabytes[7] = (byte)((tagHigh >> 24) & 0xff);
                uabytes[6] = (byte)((tagHigh >> 16) & 0xff);
                uabytes[5] = (byte)((tagHigh >> 8) & 0xff);
                uabytes[4] = (byte)(tagHigh & 0xff);
                uabytes[3] = (byte)((tagLow >> 24) & 0xff);
                uabytes[2] = (byte)((tagLow >> 16) & 0xff);
                uabytes[1] = (byte)((tagLow >> 8) & 0xff);
                uabytes[0] = (byte)(tagLow & 0xff);
                uabytes[8] = 0;
                return EInteger.FromBytes(uabytes, true);
            }
            if (tagLow != 0)
            {
                uabytes = new byte[5];
                uabytes[3] = (byte)((tagLow >> 24) & 0xff);
                uabytes[2] = (byte)((tagLow >> 16) & 0xff);
                uabytes[1] = (byte)((tagLow >> 8) & 0xff);
                uabytes[0] = (byte)(tagLow & 0xff);
                uabytes[4] = 0;
                return EInteger.FromBytes(uabytes, true);
            }
            return EInteger.Zero;
        }

        private static int MapCompare(
          IDictionary<CBORObject, CBORObject> mapA,
          IDictionary<CBORObject, CBORObject> mapB)
        {
            if (mapA == null)
            {
                return (mapB == null) ? 0 : -1;
            }
            if (mapB == null)
            {
                return 1;
            }
            if (mapA == mapB)
            {
                return 0;
            }
            int listACount = mapA.Count;
            int listBCount = mapB.Count;
            if (listACount == 0 && listBCount == 0)
            {
                return 0;
            }
            if (listACount == 0)
            {
                return -1;
            }
            if (listBCount == 0)
            {
                return 1;
            }
            // NOTE: Compare map key counts to conform
            // to bytewise lexicographical ordering
            if (listACount != listBCount)
            {
                return listACount < listBCount ? -1 : 1;
            }
            var sortedASet = new List<CBORObject>(mapA.Keys);
            var sortedBSet = new List<CBORObject>(mapB.Keys);
            // DebugUtility.Log("---sorting mapA's keys");
            sortedASet.Sort();
            // DebugUtility.Log("---sorting mapB's keys");
            sortedBSet.Sort();
            // DebugUtility.Log("---done sorting");
            listACount = sortedASet.Count;
            listBCount = sortedBSet.Count;
            // Compare the keys
            /* for (var i = 0; i < listACount; ++i) {
              string str = sortedASet[i].ToString();
              str = str.Substring(0, Math.Min(100, str.Length));
              DebugUtility.Log("A " + i + "=" + str);
            }
            for (var i = 0; i < listBCount; ++i) {
              string str = sortedBSet[i].ToString();
              str = str.Substring(0, Math.Min(100, str.Length));
              DebugUtility.Log("B " + i + "=" + str);
            }*/
            for (var i = 0; i < listACount; ++i)
            {
                CBORObject itemA = sortedASet[i];
                CBORObject itemB = sortedBSet[i];
                if (itemA == null)
                {
                    return -1;
                }
                int cmp = itemA.CompareTo(itemB);
                // string ot = itemA + "/" +
                // (cmp != 0 ? itemB.ToString() : "~") +
                // " -> cmp=" + (cmp);
                // DebugUtility.Log(ot);
                if (cmp != 0)
                {
                    return cmp;
                }
                // Both maps have the same key, so compare
                // the value under that key
                cmp = mapA[itemA].CompareTo(mapB[itemB]);
                // DebugUtility.Log(itemA + "/~" +
                // " -> "+mapA[itemA]+", "+(cmp != 0 ? mapB[itemB].ToString() :
                // "~") + " -> cmp=" + cmp);
                if (cmp != 0)
                {
                    return cmp;
                }
            }
            return 0;
        }

        private static IList<object> PushObject(
          IList<object> stack,
          object parent,
          object child)
        {
            if (stack == null)
            {
                stack = new List<object>(4);
                stack.Add(parent);
            }
            foreach (object o in stack)
            {
                if (o == child)
                {
                    throw new ArgumentException("Circular reference in data" +
                      "\u0020structure");
                }
            }
            stack.Add(child);
            return stack;
        }

        private static int TagsCompare(EInteger[] tagsA, EInteger[] tagsB)
        {
            if (tagsA == null)
            {
                return (tagsB == null) ? 0 : -1;
            }
            if (tagsB == null)
            {
                return 1;
            }
            int listACount = tagsA.Length;
            int listBCount = tagsB.Length;
            int c = Math.Min(listACount, listBCount);
            for (var i = 0; i < c; ++i)
            {
                int cmp = tagsA[i].CompareTo(tagsB[i]);
                if (cmp != 0)
                {
                    return cmp;
                }
            }
            return (listACount != listBCount) ? ((listACount < listBCount) ? -1 : 1) :
              0;
        }

        private static IList<object> WriteChildObject(
          object parentThisItem,
          CBORObject child,
          Stream outputStream,
          IList<object> stack,
          CBOREncodeOptions options)
        {
            if (child == null)
            {
                outputStream.WriteByte(0xf6);
            }
            else
            {
                int type = child.ItemType;
                if (type == CBORObjectTypeArray)
                {
                    stack = PushObject(stack, parentThisItem, child.ThisItem);
                    child.WriteTags(outputStream);
                    WriteObjectArray(child.AsList(), outputStream, stack, options);
                    stack.RemoveAt(stack.Count - 1);
                }
                else if (type == CBORObjectTypeMap)
                {
                    stack = PushObject(stack, parentThisItem, child.ThisItem);
                    child.WriteTags(outputStream);
                    WriteObjectMap(child.AsMap(), outputStream, stack, options);
                    stack.RemoveAt(stack.Count - 1);
                }
                else
                {
                    child.WriteTo(outputStream, options);
                }
            }
            return stack;
        }

        private static void WriteObjectArray(
          IList<CBORObject> list,
          Stream outputStream,
          CBOREncodeOptions options)
        {
            WriteObjectArray(list, outputStream, null, options);
        }

        private static void WriteObjectArray(
          IList<CBORObject> list,
          Stream outputStream,
          IList<object> stack,
          CBOREncodeOptions options)
        {
            object thisObj = list;
            WritePositiveInt(4, list.Count, outputStream);
            foreach (CBORObject i in list)
            {
                stack = WriteChildObject(thisObj, i, outputStream, stack, options);
            }
        }

        private static void WriteObjectMap(
          IDictionary<CBORObject, CBORObject> map,
          Stream outputStream,
          CBOREncodeOptions options)
        {
            WriteObjectMap(map, outputStream, null, options);
        }

        private static void WriteObjectMap(
          IDictionary<CBORObject, CBORObject> map,
          Stream outputStream,
          IList<object> stack,
          CBOREncodeOptions options)
        {
            object thisObj = map;
            WritePositiveInt(5, map.Count, outputStream);
            foreach (KeyValuePair<CBORObject, CBORObject> entry in map)
            {
                CBORObject key = entry.Key;
                CBORObject value = entry.Value;
                stack = WriteChildObject(
                    thisObj,
                    key,
                    outputStream,
                    stack,
                    options);
                stack = WriteChildObject(
                    thisObj,
                    value,
                    outputStream,
                    stack,
                    options);
            }
        }

        private static int WritePositiveInt(int type, int value, Stream s)
        {
            byte[] bytes = GetPositiveIntBytes(type, value);
            s.Write(bytes, 0, bytes.Length);
            return bytes.Length;
        }

        private static int WritePositiveInt64(int type, long value, Stream s)
        {
            byte[] bytes = GetPositiveInt64Bytes(type, value);
            s.Write(bytes, 0, bytes.Length);
            return bytes.Length;
        }

        private static void WriteStreamedString(string str, Stream stream)
        {
            byte[] bytes;
            bytes = GetOptimizedBytesIfShortAscii(str, -1);
            if (bytes != null)
            {
                stream.Write(bytes, 0, bytes.Length);
                return;
            }
            // Take string's length into account when allocating
            // stream buffer, in case it's much smaller than the usual stream
            // string buffer length and to improve performance on small strings
            int bufferLength = Math.Min(StreamedStringBufferLength, str.Length);
            if (bufferLength < StreamedStringBufferLength)
            {
                bufferLength = Math.Min(
                    StreamedStringBufferLength,
                    bufferLength * 3);
            }
            bytes = new byte[bufferLength];
            var byteIndex = 0;
            var streaming = false;
            for (int index = 0; index < str.Length; ++index)
            {
                int c = str[index];
                if (c <= 0x7f)
                {
                    if (byteIndex >= StreamedStringBufferLength)
                    {
                        // Write bytes retrieved so far
                        if (!streaming)
                        {
                            stream.WriteByte((byte)0x7f);
                        }
                        WritePositiveInt(3, byteIndex, stream);
                        stream.Write(bytes, 0, byteIndex);
                        byteIndex = 0;
                        streaming = true;
                    }
                    bytes[byteIndex++] = (byte)c;
                }
                else if (c <= 0x7ff)
                {
                    if (byteIndex + 2 > StreamedStringBufferLength)
                    {
                        // Write bytes retrieved so far - the next three bytes
                        // would exceed the length, and the CBOR spec forbids
                        // splitting characters when generating text strings
                        if (!streaming)
                        {
                            stream.WriteByte((byte)0x7f);
                        }
                        WritePositiveInt(3, byteIndex, stream);
                        stream.Write(bytes, 0, byteIndex);
                        byteIndex = 0;
                        streaming = true;
                    }
                    bytes[byteIndex++] = (byte)(0xc0 | ((c >> 6) & 0x1f));
                    bytes[byteIndex++] = (byte)(0x80 | (c & 0x3f));
                }
                else
                {
                    if ((c & 0xfc00) == 0xd800 && index + 1 < str.Length &&
                      (str[index + 1] & 0xfc00) == 0xdc00)
                    {
                        // Get the Unicode code point for the surrogate pair
                        c = 0x10000 + ((c & 0x3ff) << 10) + (str[index + 1] & 0x3ff);
                        ++index;
                    }
                    else if ((c & 0xf800) == 0xd800)
                    {
                        // unpaired surrogate, write U+FFFD instead
                        c = 0xfffd;
                    }
                    if (c <= 0xffff)
                    {
                        if (byteIndex + 3 > StreamedStringBufferLength)
                        {
                            // Write bytes retrieved so far - the next three bytes
                            // would exceed the length, and the CBOR spec forbids
                            // splitting characters when generating text strings
                            if (!streaming)
                            {
                                stream.WriteByte((byte)0x7f);
                            }
                            WritePositiveInt(3, byteIndex, stream);
                            stream.Write(bytes, 0, byteIndex);
                            byteIndex = 0;
                            streaming = true;
                        }
                        bytes[byteIndex++] = (byte)(0xe0 | ((c >> 12) & 0x0f));
                        bytes[byteIndex++] = (byte)(0x80 | ((c >> 6) & 0x3f));
                        bytes[byteIndex++] = (byte)(0x80 | (c & 0x3f));
                    }
                    else
                    {
                        if (byteIndex + 4 > StreamedStringBufferLength)
                        {
                            // Write bytes retrieved so far - the next four bytes
                            // would exceed the length, and the CBOR spec forbids
                            // splitting characters when generating text strings
                            if (!streaming)
                            {
                                stream.WriteByte((byte)0x7f);
                            }
                            WritePositiveInt(3, byteIndex, stream);
                            stream.Write(bytes, 0, byteIndex);
                            byteIndex = 0;
                            streaming = true;
                        }
                        bytes[byteIndex++] = (byte)(0xf0 | ((c >> 18) & 0x07));
                        bytes[byteIndex++] = (byte)(0x80 | ((c >> 12) & 0x3f));
                        bytes[byteIndex++] = (byte)(0x80 | ((c >> 6) & 0x3f));
                        bytes[byteIndex++] = (byte)(0x80 | (c & 0x3f));
                    }
                }
            }
            WritePositiveInt(3, byteIndex, stream);
            stream.Write(bytes, 0, byteIndex);
            if (streaming)
            {
                stream.WriteByte((byte)0xff);
            }
        }

        private int AsInt32(int minValue, int maxValue)
        {
            CBORNumber cn = CBORNumber.FromCBORObject(this);
            if (cn == null)
            {
                throw new InvalidOperationException("not a number type");
            }
            return cn.GetNumberInterface().AsInt32(
                cn.GetValue(),
                minValue,
                maxValue);
        }

        private void WriteTags(Stream s)
        {
            CBORObject curobject = this;
            while (curobject.IsTagged)
            {
                int low = curobject.tagLow;
                int high = curobject.tagHigh;
                if (high == 0 && (low >> 16) == 0)
                {
                    WritePositiveInt(6, low, s);
                }
                else if (high == 0)
                {
                    long value = ((long)low) & 0xffffffffL;
                    WritePositiveInt64(6, value, s);
                }
                else if ((high >> 16) == 0)
                {
                    long value = ((long)low) & 0xffffffffL;
                    long highValue = ((long)high) & 0xffffffffL;
                    value |= highValue << 32;
                    WritePositiveInt64(6, value, s);
                }
                else
                {
                    byte[] arrayToWrite = {
            (byte)0xdb,
            (byte)((high >> 24) & 0xff), (byte)((high >> 16) & 0xff),
            (byte)((high >> 8) & 0xff), (byte)(high & 0xff),
            (byte)((low >> 24) & 0xff), (byte)((low >> 16) & 0xff),
            (byte)((low >> 8) & 0xff), (byte)(low & 0xff),
          };
                    s.Write(arrayToWrite, 0, 9);
                }
                curobject = (CBORObject)curobject.itemValue;
            }
        }
    }

    // Contains extra methods placed separately
    // because they are not CLS-compliant or they
    // are specific to the .NET version of the library.
    public sealed partial class CBORObject
    {
        /* The "==" and "!=" operators are not overridden in the .NET version to be
          consistent with Equals, for two reasons: (1) This type is mutable in
        certain cases, which can cause different results when comparing with another
          object. (2) Objects with this type can have arbitrary size (e.g., they
        can be byte strings, text strings, arrays, or maps of arbitrary size), and
        comparing
          two of them for equality can be much more complicated and take much
          more time than the default behavior of reference equality.
        */

        /// <summary>Returns whether one object's value is less than
        /// another's.</summary>
        /// <param name='a'>The left-hand side of the comparison.</param>
        /// <param name='b'>The right-hand side of the comparison.</param>
        /// <returns><c>true</c> if one object's value is less than another's;
        /// otherwise, <c>false</c>.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='a'/> is null.</exception>
        public static bool operator <(CBORObject a, CBORObject b)
        {
            return a == null ? b != null : a.CompareTo(b) < 0;
        }

        /// <summary>Returns whether one object's value is up to
        /// another's.</summary>
        /// <param name='a'>The left-hand side of the comparison.</param>
        /// <param name='b'>The right-hand side of the comparison.</param>
        /// <returns><c>true</c> if one object's value is up to another's;
        /// otherwise, <c>false</c>.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='a'/> is null.</exception>
        public static bool operator <=(CBORObject a, CBORObject b)
        {
            return a == null || a.CompareTo(b) <= 0;
        }

        /// <summary>Returns whether one object's value is greater than
        /// another's.</summary>
        /// <param name='a'>The left-hand side of the comparison.</param>
        /// <param name='b'>The right-hand side of the comparison.</param>
        /// <returns><c>true</c> if one object's value is greater than
        /// another's; otherwise, <c>false</c>.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='a'/> is null.</exception>
        public static bool operator >(CBORObject a, CBORObject b)
        {
            return a != null && a.CompareTo(b) > 0;
        }

        /// <summary>Returns whether one object's value is at least
        /// another's.</summary>
        /// <param name='a'>The left-hand side of the comparison.</param>
        /// <param name='b'>The right-hand side of the comparison.</param>
        /// <returns><c>true</c> if one object's value is at least another's;
        /// otherwise, <c>false</c>.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='a'/> is null.</exception>
        public static bool operator >=(CBORObject a, CBORObject b)
        {
            return a == null ? b == null : a.CompareTo(b) >= 0;
        }

        /// <summary>Converts this object to a 16-bit unsigned integer after
        /// discarding any fractional part, if any, from its value.</summary>
        /// <returns>A 16-bit unsigned integer.</returns>
        /// <exception cref='InvalidOperationException'>This object does not
        /// represent a number (for this purpose, infinities and not-a-number
        /// or NaN values, but not CBORObject.Null, are considered
        /// numbers).</exception>
        /// <exception cref='OverflowException'>This object's value, if
        /// converted to an integer by discarding its fractional part, is
        /// outside the range of a 16-bit unsigned integer.</exception>
        [Obsolete("Instead, use the following:" +
            "\u0020(cbor.AsNumber().ToUInt16Checked()), or .ToObject<ushort>().")]
        public ushort AsUInt16()
        {
            return this.AsUInt16Legacy();
        }
        internal ushort AsUInt16Legacy()
        {
            int v = this.AsInt32();
            if (v > UInt16.MaxValue || v < 0)
            {
                throw new OverflowException("This object's value is out of range");
            }
            return (ushort)v;
        }

        /// <summary>Converts this object to a 32-bit unsigned integer after
        /// discarding any fractional part, if any, from its value.</summary>
        /// <returns>A 32-bit unsigned integer.</returns>
        /// <exception cref='InvalidOperationException'>This object does not
        /// represent a number (for this purpose, infinities and not-a-number
        /// or NaN values, but not CBORObject.Null, are considered
        /// numbers).</exception>
        /// <exception cref='OverflowException'>This object's value, if
        /// converted to an integer by discarding its fractional part, is
        /// outside the range of a 32-bit unsigned integer.</exception>
        [Obsolete("Instead, use the following:" +
            "\u0020(cbor.AsNumber().ToUInt32Checked()), or .ToObject<uint>().")]
        public uint AsUInt32()
        {
            return this.AsUInt32Legacy();
        }
        internal uint AsUInt32Legacy()
        {
            ulong v = this.AsUInt64Legacy();
            if (v > UInt32.MaxValue)
            {
                throw new OverflowException("This object's value is out of range");
            }
            return (uint)v;
        }

        /// <summary>Converts this object to an 8-bit signed integer.</summary>
        /// <returns>An 8-bit signed integer.</returns>
        [Obsolete("Instead, use the following:" +
            "\u0020(cbor.AsNumber().ToSByteChecked()), or .ToObject<sbyte>().")]
        public sbyte AsSByte()
        {
            return this.AsSByteLegacy();
        }
        internal sbyte AsSByteLegacy()
        {
            int v = this.AsInt32();
            if (v > SByte.MaxValue || v < SByte.MinValue)
            {
                throw new OverflowException("This object's value is out of range");
            }
            return (sbyte)v;
        }

        /// <summary>Writes a CBOR major type number and an integer 0 or
        /// greater associated with it to a data stream, where that integer is
        /// passed to this method as a 32-bit unsigned integer. This is a
        /// low-level method that is useful for implementing custom CBOR
        /// encoding methodologies. This method encodes the given major type
        /// and value in the shortest form allowed for the major
        /// type.</summary>
        /// <param name='outputStream'>A writable data stream.</param>
        /// <param name='majorType'>The CBOR major type to write. This is a
        /// number from 0 through 7 as follows. 0: integer 0 or greater; 1:
        /// negative integer; 2: byte string; 3: UTF-8 text string; 4: array;
        /// 5: map; 6: tag; 7: simple value. See RFC 7049 for details on these
        /// major types.</param>
        /// <param name='value'>An integer 0 or greater associated with the
        /// major type, as follows. 0: integer 0 or greater; 1: the negative
        /// integer's absolute value is 1 plus this number; 2: length in bytes
        /// of the byte string; 3: length in bytes of the UTF-8 text string; 4:
        /// number of items in the array; 5: number of key-value pairs in the
        /// map; 6: tag number; 7: simple value number, which must be in the
        /// interval [0, 23] or [32, 255].</param>
        /// <returns>The number of bytes ordered to be written to the data
        /// stream.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='outputStream'/> is null.</exception>
        public static int WriteValue(
          Stream outputStream,
          int majorType,
          uint value)
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }
            return WriteValue(outputStream, majorType, (long)value);
        }

        /// <summary>Writes a CBOR major type number and an integer 0 or
        /// greater associated with it to a data stream, where that integer is
        /// passed to this method as a 64-bit unsigned integer. This is a
        /// low-level method that is useful for implementing custom CBOR
        /// encoding methodologies. This method encodes the given major type
        /// and value in the shortest form allowed for the major
        /// type.</summary>
        /// <param name='outputStream'>A writable data stream.</param>
        /// <param name='majorType'>The CBOR major type to write. This is a
        /// number from 0 through 7 as follows. 0: integer 0 or greater; 1:
        /// negative integer; 2: byte string; 3: UTF-8 text string; 4: array;
        /// 5: map; 6: tag; 7: simple value. See RFC 7049 for details on these
        /// major types.</param>
        /// <param name='value'>An integer 0 or greater associated with the
        /// major type, as follows. 0: integer 0 or greater; 1: the negative
        /// integer's absolute value is 1 plus this number; 2: length in bytes
        /// of the byte string; 3: length in bytes of the UTF-8 text string; 4:
        /// number of items in the array; 5: number of key-value pairs in the
        /// map; 6: tag number; 7: simple value number, which must be in the
        /// interval [0, 23] or [32, 255].</param>
        /// <returns>The number of bytes ordered to be written to the data
        /// stream.</returns>
        /// <exception cref='ArgumentException'>The parameter <paramref
        /// name='majorType'/> is 7 and value is greater than 255.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='outputStream'/> is null.</exception>
        public static int WriteValue(
          Stream outputStream,
          int majorType,
          ulong value)
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }
            if (value <= Int64.MaxValue)
            {
                return WriteValue(outputStream, majorType, (long)value);
            }
            else
            {
                if (majorType < 0)
                {
                    throw new ArgumentException("majorType(" + majorType +
                      ") is less than 0");
                }
                if (majorType > 7)
                {
                    throw new ArgumentException("majorType(" + majorType +
                      ") is more than 7");
                }
                if (majorType == 7)
                {
                    throw new ArgumentException("majorType is 7 and value is greater" +
                      "\u0020than 255");
                }
                byte[] bytes = {
          (byte)(27 | (majorType << 5)), (byte)((value >>
          56) & 0xff),
          (byte)((value >> 48) & 0xff), (byte)((value >> 40) & 0xff),
          (byte)((value >> 32) & 0xff), (byte)((value >> 24) & 0xff),
          (byte)((value >> 16) & 0xff), (byte)((value >> 8) & 0xff),
          (byte)(value & 0xff),
        };
                outputStream.Write(bytes, 0, bytes.Length);
                return bytes.Length;
            }
        }

        private static EInteger DecimalToEInteger(decimal dec)
        {
            return ((EDecimal)dec).ToEInteger();
        }

        /// <summary>Converts this object to a.NET decimal.</summary>
        /// <returns>The closest big integer to this object.</returns>
        /// <exception cref='InvalidOperationException'>This object does not
        /// represent a number (for this purpose, infinities and not-a-number
        /// or NaN values, but not CBORObject.Null, are considered
        /// numbers).</exception>
        /// <exception cref='OverflowException'>This object's value exceeds the
        /// range of a.NET decimal.</exception>
        public decimal AsDecimal()
        {
            return (this.ItemType == CBORObjectTypeInteger) ?
      ((decimal)(long)this.ThisItem) : ((this.HasOneTag(30) ||

                  this.HasOneTag(270)) ? (decimal)this.ToObject<ERational>() :
                (decimal)this.ToObject<EDecimal>());
        }

        /// <summary>Converts this object to a 64-bit unsigned integer after
        /// discarding any fractional part, if any, from its value.</summary>
        /// <returns>A 64-bit unsigned integer.</returns>
        /// <exception cref='InvalidOperationException'>This object does not
        /// represent a number (for this purpose, infinities and not-a-number
        /// or NaN values, but not CBORObject.Null, are considered
        /// numbers).</exception>
        /// <exception cref='OverflowException'>This object's value, if
        /// converted to an integer by discarding its fractional part, is
        /// outside the range of a 64-bit unsigned integer.</exception>
        [Obsolete("Instead, use the following:" +
            "\u0020(cbor.AsNumber().ToUInt64Checked()), or .ToObject<ulong>().")]
        public ulong AsUInt64()
        {
            return this.AsUInt64Legacy();
        }
        internal ulong AsUInt64Legacy()
        {
            EInteger bigint = this.ToObject<EInteger>();
            if (bigint.Sign < 0 ||
              bigint.GetUnsignedBitLengthAsEInteger().CompareTo(64) > 0)
            {
                throw new OverflowException("This object's value is out of range");
            }
            return (ulong)bigint;
        }

        /// <summary>Writes an 8-bit signed integer in CBOR format to a data
        /// stream.</summary>
        /// <param name='value'>The parameter <paramref name='value'/> is an
        /// 8-bit signed integer.</param>
        /// <param name='stream'>A writable data stream.</param>
        public static void Write(sbyte value, Stream stream)
        {
            Write((long)value, stream);
        }

        /// <summary>Writes a 64-bit unsigned integer in CBOR format to a data
        /// stream.</summary>
        /// <param name='value'>A 64-bit unsigned integer.</param>
        /// <param name='stream'>A writable data stream.</param>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='stream'/> is null.</exception>
        public static void Write(ulong value, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (value <= Int64.MaxValue)
            {
                Write((long)value, stream);
            }
            else
            {
                stream.WriteByte((byte)27);
                stream.WriteByte((byte)((value >> 56) & 0xff));
                stream.WriteByte((byte)((value >> 48) & 0xff));
                stream.WriteByte((byte)((value >> 40) & 0xff));
                stream.WriteByte((byte)((value >> 32) & 0xff));
                stream.WriteByte((byte)((value >> 24) & 0xff));
                stream.WriteByte((byte)((value >> 16) & 0xff));
                stream.WriteByte((byte)((value >> 8) & 0xff));
                stream.WriteByte((byte)(value & 0xff));
            }
        }

        /// <summary>Converts a.NET decimal to a CBOR object.</summary>
        /// <param name='value'>The parameter <paramref name='value'/> is a
        /// Decimal object.</param>
        /// <returns>A CBORObject object with the same value as the.NET
        /// decimal.</returns>
        public static CBORObject FromObject(decimal value)
        {
            return FromObject((EDecimal)value);
        }

        /// <summary>Writes a 32-bit unsigned integer in CBOR format to a data
        /// stream.</summary>
        /// <param name='value'>A 32-bit unsigned integer.</param>
        /// <param name='stream'>A writable data stream.</param>
        public static void Write(uint value, Stream stream)
        {
            Write((ulong)value, stream);
        }

        /// <summary>Writes a 16-bit unsigned integer in CBOR format to a data
        /// stream.</summary>
        /// <param name='value'>A 16-bit unsigned integer.</param>
        /// <param name='stream'>A writable data stream.</param>
        public static void Write(ushort value, Stream stream)
        {
            Write((ulong)value, stream);
        }

        /// <summary>Converts a signed 8-bit integer to a CBOR
        /// object.</summary>
        /// <param name='value'>The parameter <paramref name='value'/> is an
        /// 8-bit signed integer.</param>
        /// <returns>A CBORObject object.</returns>
        public static CBORObject FromObject(sbyte value)
        {
            return FromObject((long)value);
        }

        private static EInteger UInt64ToEInteger(ulong value)
        {
            var data = new byte[9];
            ulong uvalue = value;
            data[0] = (byte)(uvalue & 0xff);
            data[1] = (byte)((uvalue >> 8) & 0xff);
            data[2] = (byte)((uvalue >> 16) & 0xff);
            data[3] = (byte)((uvalue >> 24) & 0xff);
            data[4] = (byte)((uvalue >> 32) & 0xff);
            data[5] = (byte)((uvalue >> 40) & 0xff);
            data[6] = (byte)((uvalue >> 48) & 0xff);
            data[7] = (byte)((uvalue >> 56) & 0xff);
            data[8] = (byte)0;
            return EInteger.FromBytes(data, true);
        }

        /// <summary>Converts a 64-bit unsigned integer to a CBOR
        /// object.</summary>
        /// <param name='value'>A 64-bit unsigned integer.</param>
        /// <returns>A CBORObject object.</returns>
        public static CBORObject FromObject(ulong value)
        {
            return CBORObject.FromObject(UInt64ToEInteger(value));
        }

        /// <summary>Converts a 32-bit unsigned integer to a CBOR
        /// object.</summary>
        /// <param name='value'>A 32-bit unsigned integer.</param>
        /// <returns>A CBORObject object.</returns>
        public static CBORObject FromObject(uint value)
        {
            return FromObject((long)(Int64)value);
        }

        /// <summary>Converts a 16-bit unsigned integer to a CBOR
        /// object.</summary>
        /// <param name='value'>A 16-bit unsigned integer.</param>
        /// <returns>A CBORObject object.</returns>
        public static CBORObject FromObject(ushort value)
        {
            return FromObject((long)(Int64)value);
        }

        /// <summary>Generates a CBOR object from this one, but gives the
        /// resulting object a tag in addition to its existing tags (the new
        /// tag is made the outermost tag).</summary>
        /// <param name='tag'>A 64-bit integer that specifies a tag number. The
        /// tag number 55799 can be used to mark a "self-described CBOR"
        /// object. This document does not attempt to list all CBOR tags and
        /// their meanings. An up-to-date list can be found at the CBOR Tags
        /// registry maintained by the Internet Assigned Numbers Authority(
        /// <i>iana.org/assignments/cbor-tags</i> ).</param>
        /// <returns>A CBOR object with the same value as this one but given
        /// the tag <paramref name='tag'/> in addition to its existing tags
        /// (the new tag is made the outermost tag).</returns>
        public CBORObject WithTag(ulong tag)
        {
            return FromObjectAndTag(this, UInt64ToEInteger(tag));
        }

        /// <summary>Generates a CBOR object from an arbitrary object and gives
        /// the resulting object a tag.</summary>
        /// <param name='o'>The parameter <paramref name='o'/> is an arbitrary
        /// object, which can be null.
        /// <para><b>NOTE:</b> For security reasons, whenever possible, an
        /// application should not base this parameter on user input or other
        /// externally supplied data unless the application limits this
        /// parameter's inputs to types specially handled by this method (such
        /// as <c>int</c> or <c>String</c> ) and/or to plain-old-data types
        /// (POCO or POJO types) within the control of the application. If the
        /// plain-old-data type references other data types, those types should
        /// likewise meet either criterion above.</para>.</param>
        /// <param name='tag'>A 64-bit integer that specifies a tag number. The
        /// tag number 55799 can be used to mark a "self-described CBOR"
        /// object. This document does not attempt to list all CBOR tags and
        /// their meanings. An up-to-date list can be found at the CBOR Tags
        /// registry maintained by the Internet Assigned Numbers Authority(
        /// <i>iana.org/assignments/cbor-tags</i> ).</param>
        /// <returns>A CBOR object where the object <paramref name='o'/> is
        /// converted to a CBOR object and given the tag <paramref name='tag'/>
        /// . If "valueOb" is null, returns a version of CBORObject.Null with
        /// the given tag.</returns>
        public static CBORObject FromObjectAndTag(Object o, ulong tag)
        {
            return FromObjectAndTag(o, UInt64ToEInteger(tag));
        }

        /// <summary>
        /// <para>Converts this CBOR object to an object of an arbitrary type.
        /// See
        /// <see cref='PeterO.Cbor.CBORObject.ToObject(System.Type)'/> for
        /// further information.</para></summary>
        /// <typeparam name='T'>The type, class, or interface that this
        /// method's return value will belong to. <b>Note:</b> For security
        /// reasons, an application should not base this parameter on user
        /// input or other externally supplied data. Whenever possible, this
        /// parameter should be either a type specially handled by this method
        /// (such as <c>int</c> or <c>String</c> ) or a plain-old-data type
        /// (POCO or POJO type) within the control of the application. If the
        /// plain-old-data type references other data types, those types should
        /// likewise meet either criterion above.</typeparam>
        /// <returns>The converted object.</returns>
        /// <exception cref='NotSupportedException'>The given type "T", or this
        /// object's CBOR type, is not supported.</exception>
        public T ToObject<T>()
        {
            return (T)this.ToObject(typeof(T));
        }

        /// <summary>
        /// <para>Converts this CBOR object to an object of an arbitrary type.
        /// See
        /// <see cref='PeterO.Cbor.CBORObject.ToObject(System.Type)'/> for
        /// further information.</para></summary>
        /// <param name='mapper'>This parameter controls which data types are
        /// eligible for Plain-Old-Data deserialization and includes custom
        /// converters from CBOR objects to certain data types.</param>
        /// <typeparam name='T'>The type, class, or interface that this
        /// method's return value will belong to. <b>Note:</b> For security
        /// reasons, an application should not base this parameter on user
        /// input or other externally supplied data. Whenever possible, this
        /// parameter should be either a type specially handled by this method
        /// (such as <c>int</c> or <c>String</c> ) or a plain-old-data type
        /// (POCO or POJO type) within the control of the application. If the
        /// plain-old-data type references other data types, those types should
        /// likewise meet either criterion above.</typeparam>
        /// <returns>The converted object.</returns>
        /// <exception cref='NotSupportedException'>The given type "T", or this
        /// object's CBOR type, is not supported.</exception>
        public T ToObject<T>(CBORTypeMapper mapper)
        {
            return (T)this.ToObject(typeof(T), mapper);
        }

        /// <summary>
        /// <para>Converts this CBOR object to an object of an arbitrary type.
        /// See
        /// <see cref='PeterO.Cbor.CBORObject.ToObject(System.Type)'/> for
        /// further information.</para></summary>
        /// <param name='options'>Specifies options for controlling
        /// deserialization of CBOR objects.</param>
        /// <typeparam name='T'>The type, class, or interface that this
        /// method's return value will belong to. <b>Note:</b> For security
        /// reasons, an application should not base this parameter on user
        /// input or other externally supplied data. Whenever possible, this
        /// parameter should be either a type specially handled by this method
        /// (such as <c>int</c> or <c>String</c> ) or a plain-old-data type
        /// (POCO or POJO type) within the control of the application. If the
        /// plain-old-data type references other data types, those types should
        /// likewise meet either criterion above.</typeparam>
        /// <returns>The converted object.</returns>
        /// <exception cref='NotSupportedException'>The given type "T", or this
        /// object's CBOR type, is not supported.</exception>
        public T ToObject<T>(PODOptions options)
        {
            return (T)this.ToObject(typeof(T), options);
        }

        /// <summary>
        /// <para>Converts this CBOR object to an object of an arbitrary type.
        /// See
        /// <see cref='PeterO.Cbor.CBORObject.ToObject(System.Type)'/> for
        /// further information.</para></summary>
        /// <param name='mapper'>This parameter controls which data types are
        /// eligible for Plain-Old-Data deserialization and includes custom
        /// converters from CBOR objects to certain data types.</param>
        /// <param name='options'>Specifies options for controlling
        /// deserialization of CBOR objects.</param>
        /// <typeparam name='T'>The type, class, or interface that this
        /// method's return value will belong to. <b>Note:</b> For security
        /// reasons, an application should not base this parameter on user
        /// input or other externally supplied data. Whenever possible, this
        /// parameter should be either a type specially handled by this method
        /// (such as <c>int</c> or <c>String</c> ) or a plain-old-data type
        /// (POCO or POJO type) within the control of the application. If the
        /// plain-old-data type references other data types, those types should
        /// likewise meet either criterion above.</typeparam>
        /// <returns>The converted object.</returns>
        /// <exception cref='NotSupportedException'>The given type "T", or this
        /// object's CBOR type, is not supported.</exception>
        public T ToObject<T>(CBORTypeMapper mapper, PODOptions options)
        {
            return (T)this.ToObject(typeof(T), mapper, options);
        }

        /// <summary>Adds two CBOR objects and returns their result.</summary>
        /// <param name='a'>The parameter <paramref name='a'/> is a CBOR
        /// object.</param>
        /// <param name='b'>The parameter <paramref name='b'/> is a CBOR
        /// object.</param>
        /// <returns>The sum of the two objects.</returns>
        [Obsolete("May be removed in the next major version. Consider converting" +
            "\u0020the objects to CBOR numbers and performing the operation" +
    "\u0020there.")]
        public static CBORObject operator +(CBORObject a, CBORObject b)
        {
            return Addition(a, b);
        }

        /// <summary>Subtracts a CBORObject object from a CBORObject
        /// object.</summary>
        /// <param name='a'>The parameter <paramref name='a'/> is a CBOR
        /// object.</param>
        /// <param name='b'>The parameter <paramref name='b'/> is a CBOR
        /// object.</param>
        /// <returns>The difference of the two objects.</returns>
        [Obsolete("May be removed in the next major version. Consider converting" +
            "\u0020the objects to CBOR numbers and performing the operation" +
    "\u0020there.")]
        public static CBORObject operator -(CBORObject a, CBORObject b)
        {
            return Subtract(a, b);
        }

        /// <summary>Multiplies a CBORObject object by the value of a
        /// CBORObject object.</summary>
        /// <param name='a'>The parameter <paramref name='a'/> is a CBOR
        /// object.</param>
        /// <param name='b'>The parameter <paramref name='b'/> is a CBOR
        /// object.</param>
        /// <returns>The product of the two numbers.</returns>
        [Obsolete("May be removed in the next major version. Consider converting" +
            "\u0020the objects to CBOR numbers and performing the operation" +
    "\u0020there.")]
        public static CBORObject operator *(CBORObject a, CBORObject b)
        {
            return Multiply(a, b);
        }

        /// <summary>Divides a CBORObject object by the value of a CBORObject
        /// object.</summary>
        /// <param name='a'>The parameter <paramref name='a'/> is a CBOR
        /// object.</param>
        /// <param name='b'>The parameter <paramref name='b'/> is a CBOR
        /// object.</param>
        /// <returns>The quotient of the two objects.</returns>
        [Obsolete("May be removed in the next major version. Consider converting" +
            "\u0020the objects to CBOR numbers and performing the operation" +
    "\u0020there.")]
        public static CBORObject operator /(CBORObject a, CBORObject b)
        {
            return Divide(a, b);
        }

        /// <summary>Finds the remainder that results when a CBORObject object
        /// is divided by the value of a CBORObject object.</summary>
        /// <param name='a'>The parameter <paramref name='a'/> is a CBOR
        /// object.</param>
        /// <param name='b'>The parameter <paramref name='b'/> is a CBOR
        /// object.</param>
        /// <returns>The remainder of the two numbers.</returns>
        [Obsolete("May be removed in the next major version. Consider converting" +
            "\u0020the objects to CBOR numbers and performing the operation" +
    "\u0020there.")]
        public static CBORObject operator %(CBORObject a, CBORObject b)
        {
            return Remainder(a, b);
        }
    }
    #endregion

    #region CBORObjectMath
    /*
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:CBORObjectMath"]/*'/>
    internal static class CBORObjectMath
    {
        public static CBORObject Addition(CBORObject a, CBORObject b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }
            if (a.Type != CBORType.Number)
            {
                throw new ArgumentException("a.Type (" + a.Type +
                  ") is not equal to " + CBORType.Number);
            }
            if (b.Type != CBORType.Number)
            {
                throw new ArgumentException("b.Type (" + b.Type +
                  ") is not equal to " + CBORType.Number);
            }
            object objA = a.ThisItem;
            object objB = b.ThisItem;
            int typeA = a.ItemType;
            int typeB = b.ItemType;
            if (typeA == CBORObject.CBORObjectTypeInteger && typeB ==
            CBORObject.CBORObjectTypeInteger)
            {
                long valueA = (long)objA;
                long valueB = (long)objB;
                if ((valueA < 0 && valueB < Int64.MinValue - valueA) ||
                        (valueA > 0 && valueB > Int64.MaxValue - valueA))
                {
                    // would overflow, convert to EInteger
                    return CBORObject.FromObject(((EInteger)valueA) +
                    (EInteger)valueB);
                }
                return CBORObject.FromObject(valueA + valueB);
            }
            if (typeA == CBORObject.CBORObjectTypeExtendedRational ||
                   typeB == CBORObject.CBORObjectTypeExtendedRational)
            {
                ERational e1 =
                CBORObject.GetNumberInterface(typeA).AsExtendedRational(objA);
                ERational e2 =
                CBORObject.GetNumberInterface(typeB).AsExtendedRational(objB);
                return CBORObject.FromObject(e1.Add(e2));
            }
            if (typeA == CBORObject.CBORObjectTypeExtendedDecimal ||
                   typeB == CBORObject.CBORObjectTypeExtendedDecimal)
            {
                EDecimal e1 =
                CBORObject.GetNumberInterface(typeA).AsExtendedDecimal(objA);
                EDecimal e2 =
                CBORObject.GetNumberInterface(typeB).AsExtendedDecimal(objB);
                return CBORObject.FromObject(e1.Add(e2));
            }
            if (typeA == CBORObject.CBORObjectTypeExtendedFloat || typeB ==
            CBORObject.CBORObjectTypeExtendedFloat ||
                     typeA == CBORObject.CBORObjectTypeDouble || typeB ==
                     CBORObject.CBORObjectTypeDouble ||
                     typeA == CBORObject.CBORObjectTypeSingle || typeB ==
                     CBORObject.CBORObjectTypeSingle)
            {
                EFloat e1 =
                CBORObject.GetNumberInterface(typeA).AsExtendedFloat(objA);
                EFloat e2 = CBORObject.GetNumberInterface(typeB).AsExtendedFloat(objB);
                return CBORObject.FromObject(e1.Add(e2));
            }
            else
            {
                EInteger b1 = CBORObject.GetNumberInterface(typeA).AsEInteger(objA);
                EInteger b2 = CBORObject.GetNumberInterface(typeB).AsEInteger(objB);
                return CBORObject.FromObject(b1 + (EInteger)b2);
            }
        }

        public static CBORObject Subtract(CBORObject a, CBORObject b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }
            if (a.Type != CBORType.Number)
            {
                throw new ArgumentException("a.Type (" + a.Type +
                  ") is not equal to " + CBORType.Number);
            }
            if (b.Type != CBORType.Number)
            {
                throw new ArgumentException("b.Type (" + b.Type +
                  ") is not equal to " + CBORType.Number);
            }
            object objA = a.ThisItem;
            object objB = b.ThisItem;
            int typeA = a.ItemType;
            int typeB = b.ItemType;
            if (typeA == CBORObject.CBORObjectTypeInteger && typeB ==
            CBORObject.CBORObjectTypeInteger)
            {
                long valueA = (long)objA;
                long valueB = (long)objB;
                if ((valueB < 0 && Int64.MaxValue + valueB < valueA) ||
                        (valueB > 0 && Int64.MinValue + valueB > valueA))
                {
                    // would overflow, convert to EInteger
                    return CBORObject.FromObject(((EInteger)valueA) -
                    (EInteger)valueB);
                }
                return CBORObject.FromObject(valueA - valueB);
            }
            if (typeA == CBORObject.CBORObjectTypeExtendedRational ||
                   typeB == CBORObject.CBORObjectTypeExtendedRational)
            {
                ERational e1 =
                CBORObject.GetNumberInterface(typeA).AsExtendedRational(objA);
                ERational e2 =
                CBORObject.GetNumberInterface(typeB).AsExtendedRational(objB);
                return CBORObject.FromObject(e1.Subtract(e2));
            }
            if (typeA == CBORObject.CBORObjectTypeExtendedDecimal ||
                   typeB == CBORObject.CBORObjectTypeExtendedDecimal)
            {
                EDecimal e1 =
                CBORObject.GetNumberInterface(typeA).AsExtendedDecimal(objA);
                EDecimal e2 =
                CBORObject.GetNumberInterface(typeB).AsExtendedDecimal(objB);
                return CBORObject.FromObject(e1.Subtract(e2));
            }
            if (typeA == CBORObject.CBORObjectTypeExtendedFloat || typeB ==
            CBORObject.CBORObjectTypeExtendedFloat ||
                     typeA == CBORObject.CBORObjectTypeDouble || typeB ==
                     CBORObject.CBORObjectTypeDouble ||
                     typeA == CBORObject.CBORObjectTypeSingle || typeB ==
                     CBORObject.CBORObjectTypeSingle)
            {
                EFloat e1 =
                CBORObject.GetNumberInterface(typeA).AsExtendedFloat(objA);
                EFloat e2 = CBORObject.GetNumberInterface(typeB).AsExtendedFloat(objB);
                return CBORObject.FromObject(e1.Subtract(e2));
            }
            else
            {
                EInteger b1 = CBORObject.GetNumberInterface(typeA).AsEInteger(objA);
                EInteger b2 = CBORObject.GetNumberInterface(typeB).AsEInteger(objB);
                return CBORObject.FromObject(b1 - (EInteger)b2);
            }
        }

        public static CBORObject Multiply(CBORObject a, CBORObject b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }
            if (a.Type != CBORType.Number)
            {
                throw new ArgumentException("a.Type (" + a.Type +
                  ") is not equal to " + CBORType.Number);
            }
            if (b.Type != CBORType.Number)
            {
                throw new ArgumentException("b.Type (" + b.Type +
                  ") is not equal to " + CBORType.Number);
            }
            object objA = a.ThisItem;
            object objB = b.ThisItem;
            int typeA = a.ItemType;
            int typeB = b.ItemType;
            if (typeA == CBORObject.CBORObjectTypeInteger && typeB ==
            CBORObject.CBORObjectTypeInteger)
            {
                long valueA = (long)objA;
                long valueB = (long)objB;
                bool apos = valueA > 0L;
                bool bpos = valueB > 0L;
                if (
                  (apos && ((!bpos && (Int64.MinValue / valueA) > valueB) ||
                  (bpos && valueA > (Int64.MaxValue / valueB)))) ||
                  (!apos && ((!bpos && valueA != 0L &&
                  (Int64.MaxValue / valueA) > valueB) ||
                  (bpos && valueA < (Int64.MinValue / valueB)))))
                {
                    // would overflow, convert to EInteger
                    EInteger bvalueA = (EInteger)valueA;
                    EInteger bvalueB = (EInteger)valueB;
                    return CBORObject.FromObject(bvalueA * (EInteger)bvalueB);
                }
                return CBORObject.FromObject(valueA * valueB);
            }
            if (typeA == CBORObject.CBORObjectTypeExtendedRational ||
                   typeB == CBORObject.CBORObjectTypeExtendedRational)
            {
                ERational e1 =
                CBORObject.GetNumberInterface(typeA).AsExtendedRational(objA);
                ERational e2 =
                CBORObject.GetNumberInterface(typeB).AsExtendedRational(objB);
                return CBORObject.FromObject(e1.Multiply(e2));
            }
            if (typeA == CBORObject.CBORObjectTypeExtendedDecimal ||
                   typeB == CBORObject.CBORObjectTypeExtendedDecimal)
            {
                EDecimal e1 =
                CBORObject.GetNumberInterface(typeA).AsExtendedDecimal(objA);
                EDecimal e2 =
                CBORObject.GetNumberInterface(typeB).AsExtendedDecimal(objB);
                return CBORObject.FromObject(e1.Multiply(e2));
            }
            if (typeA == CBORObject.CBORObjectTypeExtendedFloat || typeB ==
            CBORObject.CBORObjectTypeExtendedFloat ||
                     typeA == CBORObject.CBORObjectTypeDouble || typeB ==
                     CBORObject.CBORObjectTypeDouble ||
                     typeA == CBORObject.CBORObjectTypeSingle || typeB ==
                     CBORObject.CBORObjectTypeSingle)
            {
                EFloat e1 =
                CBORObject.GetNumberInterface(typeA).AsExtendedFloat(objA);
                EFloat e2 = CBORObject.GetNumberInterface(typeB).AsExtendedFloat(objB);
                return CBORObject.FromObject(e1.Multiply(e2));
            }
            else
            {
                EInteger b1 = CBORObject.GetNumberInterface(typeA).AsEInteger(objA);
                EInteger b2 = CBORObject.GetNumberInterface(typeB).AsEInteger(objB);
                return CBORObject.FromObject(b1 * (EInteger)b2);
            }
        }

        public static CBORObject Divide(CBORObject a, CBORObject b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }
            if (a.Type != CBORType.Number)
            {
                throw new ArgumentException("a.Type (" + a.Type +
                  ") is not equal to " + CBORType.Number);
            }
            if (b.Type != CBORType.Number)
            {
                throw new ArgumentException("b.Type (" + b.Type +
                  ") is not equal to " + CBORType.Number);
            }
            object objA = a.ThisItem;
            object objB = b.ThisItem;
            int typeA = a.ItemType;
            int typeB = b.ItemType;
            if (typeA == CBORObject.CBORObjectTypeInteger && typeB ==
            CBORObject.CBORObjectTypeInteger)
            {
                long valueA = (long)objA;
                long valueB = (long)objB;
                if (valueB == 0)
                {
                    return (valueA == 0) ? CBORObject.NaN : ((valueA < 0) ?
                      CBORObject.NegativeInfinity : CBORObject.PositiveInfinity);
                }
                if (valueA == Int64.MinValue && valueB == -1)
                {
                    return CBORObject.FromObject(valueA).Negate();
                }
                long quo = valueA / valueB;
                long rem = valueA - (quo * valueB);
                return (rem == 0) ? CBORObject.FromObject(quo) :
                CBORObject.FromObject(
          ERational.Create(
          (EInteger)valueA,
          (EInteger)valueB));
            }
            if (typeA == CBORObject.CBORObjectTypeExtendedRational ||
                   typeB == CBORObject.CBORObjectTypeExtendedRational)
            {
                ERational e1 =
                CBORObject.GetNumberInterface(typeA).AsExtendedRational(objA);
                ERational e2 =
                CBORObject.GetNumberInterface(typeB).AsExtendedRational(objB);
                return CBORObject.FromObject(e1.Divide(e2));
            }
            if (typeA == CBORObject.CBORObjectTypeExtendedDecimal ||
                   typeB == CBORObject.CBORObjectTypeExtendedDecimal)
            {
                EDecimal e1 =
                CBORObject.GetNumberInterface(typeA).AsExtendedDecimal(objA);
                EDecimal e2 =
                CBORObject.GetNumberInterface(typeB).AsExtendedDecimal(objB);
                if (e1.IsZero && e2.IsZero)
                {
                    return CBORObject.NaN;
                }
                EDecimal eret = e1.Divide(e2, null);
                // If either operand is infinity or NaN, the result
                // is already exact. Likewise if the result is a finite number.
                if (!e1.IsFinite || !e2.IsFinite || eret.IsFinite)
                {
                    return CBORObject.FromObject(eret);
                }
                ERational er1 =
                CBORObject.GetNumberInterface(typeA).AsExtendedRational(objA);
                ERational er2 =
                CBORObject.GetNumberInterface(typeB).AsExtendedRational(objB);
                return CBORObject.FromObject(er1.Divide(er2));
            }
            if (typeA == CBORObject.CBORObjectTypeExtendedFloat || typeB ==
            CBORObject.CBORObjectTypeExtendedFloat ||
                     typeA == CBORObject.CBORObjectTypeDouble || typeB ==
                     CBORObject.CBORObjectTypeDouble ||
                     typeA == CBORObject.CBORObjectTypeSingle || typeB ==
                     CBORObject.CBORObjectTypeSingle)
            {
                EFloat e1 =
                CBORObject.GetNumberInterface(typeA).AsExtendedFloat(objA);
                EFloat e2 = CBORObject.GetNumberInterface(typeB).AsExtendedFloat(objB);
                if (e1.IsZero && e2.IsZero)
                {
                    return CBORObject.NaN;
                }
                EFloat eret = e1.Divide(e2, null);
                // If either operand is infinity or NaN, the result
                // is already exact. Likewise if the result is a finite number.
                if (!e1.IsFinite || !e2.IsFinite || eret.IsFinite)
                {
                    return CBORObject.FromObject(eret);
                }
                ERational er1 =
                CBORObject.GetNumberInterface(typeA).AsExtendedRational(objA);
                ERational er2 =
                CBORObject.GetNumberInterface(typeB).AsExtendedRational(objB);
                return CBORObject.FromObject(er1.Divide(er2));
            }
            else
            {
                EInteger b1 = CBORObject.GetNumberInterface(typeA).AsEInteger(objA);
                EInteger b2 = CBORObject.GetNumberInterface(typeB).AsEInteger(objB);
                if (b2.IsZero)
                {
                    return b1.IsZero ? CBORObject.NaN : ((b1.Sign < 0) ?
                      CBORObject.NegativeInfinity : CBORObject.PositiveInfinity);
                }
                EInteger bigrem;
                EInteger bigquo;
                {
                    EInteger[] divrem = b1.DivRem(b2);
                    bigquo = divrem[0];
                    bigrem = divrem[1];
                }
                return bigrem.IsZero ? CBORObject.FromObject(bigquo) :
                CBORObject.FromObject(ERational.Create(b1, b2));
            }
        }

        public static CBORObject Remainder(CBORObject a, CBORObject b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }
            if (a.Type != CBORType.Number)
            {
                throw new ArgumentException("a.Type (" + a.Type +
                  ") is not equal to " + CBORType.Number);
            }
            if (b.Type != CBORType.Number)
            {
                throw new ArgumentException("b.Type (" + b.Type +
                  ") is not equal to " + CBORType.Number);
            }
            object objA = a.ThisItem;
            object objB = b.ThisItem;
            int typeA = a.ItemType;
            int typeB = b.ItemType;
            if (typeA == CBORObject.CBORObjectTypeInteger && typeB ==
            CBORObject.CBORObjectTypeInteger)
            {
                long valueA = (long)objA;
                long valueB = (long)objB;
                return (valueA == Int64.MinValue && valueB == -1) ?
                CBORObject.FromObject(0) : CBORObject.FromObject(valueA % valueB);
            }
            if (typeA == CBORObject.CBORObjectTypeExtendedRational ||
                   typeB == CBORObject.CBORObjectTypeExtendedRational)
            {
                ERational e1 =
                CBORObject.GetNumberInterface(typeA).AsExtendedRational(objA);
                ERational e2 =
                CBORObject.GetNumberInterface(typeB).AsExtendedRational(objB);
                return CBORObject.FromObject(e1.Remainder(e2));
            }
            if (typeA == CBORObject.CBORObjectTypeExtendedDecimal ||
                   typeB == CBORObject.CBORObjectTypeExtendedDecimal)
            {
                EDecimal e1 =
                CBORObject.GetNumberInterface(typeA).AsExtendedDecimal(objA);
                EDecimal e2 =
                CBORObject.GetNumberInterface(typeB).AsExtendedDecimal(objB);
                return CBORObject.FromObject(e1.Remainder(e2, null));
            }
            if (typeA == CBORObject.CBORObjectTypeExtendedFloat || typeB ==
            CBORObject.CBORObjectTypeExtendedFloat ||
                     typeA == CBORObject.CBORObjectTypeDouble || typeB ==
                     CBORObject.CBORObjectTypeDouble ||
                     typeA == CBORObject.CBORObjectTypeSingle || typeB ==
                     CBORObject.CBORObjectTypeSingle)
            {
                EFloat e1 =
                CBORObject.GetNumberInterface(typeA).AsExtendedFloat(objA);
                EFloat e2 = CBORObject.GetNumberInterface(typeB).AsExtendedFloat(objB);
                return CBORObject.FromObject(e1.Remainder(e2, null));
            }
            else
            {
                EInteger b1 = CBORObject.GetNumberInterface(typeA).AsEInteger(objA);
                EInteger b2 = CBORObject.GetNumberInterface(typeB).AsEInteger(objB);
                return CBORObject.FromObject(b1 % (EInteger)b2);
            }
        }
    }
    */
    #endregion

    #region CBORObjectFactory
    internal class CBORObjectFactory
    {
        public CBORObjectFactory()
        {
        }
    }
    #endregion

    #region CBORReader
    internal class CBORReader
    {
        private readonly Stream stream;
        private readonly CBOREncodeOptions options;
        private int depth;
        private StringRefs stringRefs;
        private bool hasSharableObjects;

        public CBORReader(Stream inStream) : this(inStream,
            CBOREncodeOptions.Default)
        {
        }

        public CBORReader(Stream inStream, CBOREncodeOptions options)
        {
            this.stream = inStream;
            this.options = options;
        }

        private static EInteger ToUnsignedEInteger(long val)
        {
            var lval = (EInteger)(val & ~(1L << 63));
            if ((val >> 63) != 0)
            {
                EInteger bigintAdd = EInteger.One << 63;
                lval += (EInteger)bigintAdd;
            }
            return lval;
        }

        private void HandleItemTag(long uadditional)
        {
            int uad = uadditional >= 257 ? 257 : (uadditional < 0 ? 0 :
                (int)uadditional);
            switch (uad)
            {
                case 256:
                    // Tag 256: String namespace
                    this.stringRefs = this.stringRefs ?? new StringRefs();
                    this.stringRefs.Push();
                    break;
                case 25:
                    // String reference
                    if (this.stringRefs == null)
                    {
                        throw new CBORException("No stringref namespace");
                    }
                    break;
                case 28:
                case 29:
                    this.hasSharableObjects = true;
                    break;
            }
        }

        private CBORObject ObjectFromByteArray(byte[] data, int lengthHint)
        {
            CBORObject cbor = CBORObject.FromRaw(data);
            if (this.stringRefs != null)
            {
                this.stringRefs.AddStringIfNeeded(cbor, lengthHint);
            }
            return cbor;
        }

        private CBORObject ObjectFromUtf8Array(byte[] data, int lengthHint)
        {
            CBORObject cbor = data.Length == 0 ? CBORObject.FromObject(String.Empty) :
               CBORObject.FromRawUtf8(data);
            if (this.stringRefs != null)
            {
                this.stringRefs.AddStringIfNeeded(cbor, lengthHint);
            }
            return cbor;
        }

        private static CBORObject ResolveSharedRefs(
          CBORObject obj,
          SharedRefs sharedRefs)
        {
            if (obj == null)
            {
                return null;
            }
            CBORType type = obj.Type;
            bool hasTag = obj.HasMostOuterTag(29);
            if (hasTag)
            {
                CBORObject untagged = obj.UntagOne();
                if (untagged.IsTagged ||
                  untagged.Type != CBORType.Integer ||
        untagged.AsNumber().IsNegative())
                {
                    throw new CBORException(
                      "Shared ref index must be an untagged integer 0 or greater");
                }
                return sharedRefs.GetObject(untagged.AsEIntegerValue());
            }
            hasTag = obj.HasMostOuterTag(28);
            if (hasTag)
            {
                obj = obj.UntagOne();
                sharedRefs.AddObject(obj);
            }
            if (type == CBORType.Map)
            {
                foreach (CBORObject key in obj.Keys)
                {
                    CBORObject value = obj[key];
                    CBORObject newvalue = ResolveSharedRefs(value, sharedRefs);
                    if (value != newvalue)
                    {
                        obj[key] = newvalue;
                    }
                }
            }
            else if (type == CBORType.Array)
            {
                for (var i = 0; i < obj.Count; ++i)
                {
                    obj[i] = ResolveSharedRefs(obj[i], sharedRefs);
                }
            }
            return obj;
        }

        public CBORObject Read()
        {
            CBORObject obj = this.options.AllowEmpty ?
              this.ReadInternalOrEOF() : this.ReadInternal();
            if (this.options.ResolveReferences && this.hasSharableObjects)
            {
                var sharedRefs = new SharedRefs();
                return ResolveSharedRefs(obj, sharedRefs);
            }
            return obj;
        }

        private CBORObject ReadInternalOrEOF()
        {
            if (this.depth > 500)
            {
                throw new CBORException("Too deeply nested");
            }
            int firstbyte = this.stream.ReadByte();
            if (firstbyte < 0)
            {
                // End of stream
                return null;
            }
            return this.ReadForFirstByte(firstbyte);
        }

        private CBORObject ReadInternal()
        {
            if (this.depth > 500)
            {
                throw new CBORException("Too deeply nested");
            }
            int firstbyte = this.stream.ReadByte();
            if (firstbyte < 0)
            {
                throw new CBORException("Premature end of data");
            }
            return this.ReadForFirstByte(firstbyte);
        }

        private CBORObject ReadStringArrayMap(int type, long uadditional)
        {
            bool canonical = this.options.Ctap2Canonical;
            if (type == 2 || type == 3)
            { // Byte string or text string
                if ((uadditional >> 31) != 0)
                {
                    throw new CBORException("Length of " +
                      ToUnsignedEInteger(uadditional).ToString() + " is bigger" +
                      "\u0020than supported");
                }
                int hint = (uadditional > Int32.MaxValue ||
                    (uadditional >> 63) != 0) ? Int32.MaxValue : (int)uadditional;
                byte[] data = ReadByteData(this.stream, uadditional, null);
                if (type == 3)
                {
                    if (!CBORUtilities.CheckUtf8(data))
                    {
                        throw new CBORException("Invalid UTF-8");
                    }
                    return this.ObjectFromUtf8Array(data, hint);
                }
                else
                {
                    return this.ObjectFromByteArray(data, hint);
                }
            }
            if (type == 4)
            { // Array
                if (this.options.Ctap2Canonical && this.depth >= 4)
                {
                    throw new CBORException("Depth too high in canonical CBOR");
                }
                CBORObject cbor = CBORObject.NewArray();
                if ((uadditional >> 31) != 0)
                {
                    throw new CBORException("Length of " +
                      ToUnsignedEInteger(uadditional).ToString() + " is bigger than" +
          "\u0020supported");
                }
                if (PropertyMap.ExceedsKnownLength(this.stream, uadditional))
                {
                    throw new CBORException("Remaining data too small for array" +
          "\u0020length");
                }
                ++this.depth;
                for (long i = 0; i < uadditional; ++i)
                {
                    cbor.Add(
                      this.ReadInternal());
                }
                --this.depth;
                return cbor;
            }
            if (type == 5)
            { // Map, type 5
                if (this.options.Ctap2Canonical && this.depth >= 4)
                {
                    throw new CBORException("Depth too high in canonical CBOR");
                }
                CBORObject cbor = CBORObject.NewMap();
                if ((uadditional >> 31) != 0)
                {
                    throw new CBORException("Length of " +
                      ToUnsignedEInteger(uadditional).ToString() + " is bigger than" +
                      "\u0020supported");
                }
                if (PropertyMap.ExceedsKnownLength(this.stream, uadditional))
                {
                    throw new CBORException("Remaining data too small for map" +
          "\u0020length");
                }
                CBORObject lastKey = null;
                IComparer<CBORObject> comparer = CBORCanonical.Comparer;
                for (long i = 0; i < uadditional; ++i)
                {
                    ++this.depth;
                    CBORObject key = this.ReadInternal();
                    CBORObject value = this.ReadInternal();
                    --this.depth;
                    if (this.options.Ctap2Canonical && lastKey != null)
                    {
                        int cmp = comparer.Compare(lastKey, key);
                        if (cmp > 0)
                        {
                            throw new CBORException("Map key not in canonical order");
                        }
                        else if (cmp == 0)
                        {
                            throw new CBORException("Duplicate map key");
                        }
                    }
                    if (!this.options.AllowDuplicateKeys)
                    {
                        if (cbor.ContainsKey(key))
                        {
                            throw new CBORException("Duplicate key already exists");
                        }
                    }
                    lastKey = key;
                    cbor[key] = value;
                }
                return cbor;
            }
            return null;
        }

        public CBORObject ReadForFirstByte(int firstbyte)
        {
            if (this.depth > 500)
            {
                throw new CBORException("Too deeply nested");
            }
            if (firstbyte < 0)
            {
                throw new CBORException("Premature end of data");
            }
            if (firstbyte == 0xff)
            {
                throw new CBORException("Unexpected break code encountered");
            }
            int type = (firstbyte >> 5) & 0x07;
            int additional = firstbyte & 0x1f;
            long uadditional;
            CBORObject fixedObject;
            if (this.options.Ctap2Canonical)
            {
                if (additional >= 0x1c)
                {
                    // NOTE: Includes stop byte and indefinite length data items
                    throw new CBORException("Invalid canonical CBOR encountered");
                }
                // Check if this represents a fixed object (NOTE: All fixed objects
                // comply with CTAP2 canonical CBOR).
                fixedObject = CBORObject.GetFixedObject(firstbyte);
                if (fixedObject != null)
                {
                    return fixedObject;
                }
                if (type == 6)
                {
                    throw new CBORException("Tags not allowed in canonical CBOR");
                }
                uadditional = ReadDataLength(
                  this.stream,
                  firstbyte,
                  type,
                  type == 7);
                if (type == 0)
                {
                    return (uadditional >> 63) != 0 ?
                      CBORObject.FromObject(ToUnsignedEInteger(uadditional)) :
                      CBORObject.FromObject(uadditional);
                }
                else if (type == 1)
                {
                    return (uadditional >> 63) != 0 ? CBORObject.FromObject(
                        ToUnsignedEInteger(uadditional).Add(1).Negate()) :
                      CBORObject.FromObject((-uadditional) - 1L);
                }
                else if (type == 7)
                {
                    if (additional < 24)
                    {
                        return CBORObject.FromSimpleValue(additional);
                    }
                    else if (additional == 24 && uadditional < 32)
                    {
                        throw new CBORException("Invalid simple value encoding");
                    }
                    else if (additional == 24)
                    {
                        return CBORObject.FromSimpleValue((int)uadditional);
                    }
                    else if (additional == 25)
                    {
                        return CBORObject.FromFloatingPointBits(uadditional, 2);
                    }
                    else if (additional == 26)
                    {
                        return CBORObject.FromFloatingPointBits(uadditional, 4);
                    }
                    else if (additional == 27)
                    {
                        return CBORObject.FromFloatingPointBits(uadditional, 8);
                    }
                }
                else if (type >= 2 && type <= 5)
                {
                    return this.ReadStringArrayMap(type, uadditional);
                }
                throw new CBORException("Unexpected data encountered");
            }
            int expectedLength = CBORObject.GetExpectedLength(firstbyte);
            // Data checks
            if (expectedLength == -1)
            {
                // if the head byte is invalid
                throw new CBORException("Unexpected data encountered");
            }
            // Check if this represents a fixed object
            fixedObject = CBORObject.GetFixedObject(firstbyte);
            if (fixedObject != null)
            {
                return fixedObject;
            }
            // Read fixed-length data
            byte[] data = null;
            if (expectedLength != 0)
            {
                data = new byte[expectedLength];
                // include the first byte because GetFixedLengthObject
                // will assume it exists for some head bytes
                data[0] = unchecked((byte)firstbyte);
                if (expectedLength > 1 &&
                  this.stream.Read(data, 1, expectedLength - 1) != expectedLength
                  - 1)
                {
                    throw new CBORException("Premature end of data");
                }
                CBORObject cbor = CBORObject.GetFixedLengthObject(firstbyte, data);
                if (this.stringRefs != null && (type == 2 || type == 3))
                {
                    this.stringRefs.AddStringIfNeeded(cbor, expectedLength - 1);
                }
                return cbor;
            }
            if (additional == 31)
            {
                // Indefinite-length for major types 2 to 5 (other major
                // types were already handled in the call to
                // GetFixedLengthObject).
                switch (type)
                {
                    case 2:
                        {
                            // Streaming byte string
                            using (var ms = new MemoryStream())
                            {
                                // Requires same type as this one
                                while (true)
                                {
                                    int nextByte = this.stream.ReadByte();
                                    if (nextByte == 0xff)
                                    {
                                        // break if the "break" code was read
                                        break;
                                    }
                                    long len = ReadDataLength(this.stream, nextByte, 2);
                                    if ((len >> 63) != 0 || len > Int32.MaxValue)
                                    {
                                        throw new CBORException("Length" + ToUnsignedEInteger(len)
                      +
                                          " is bigger than supported ");
                                    }
                                    if (nextByte != 0x40)
                                    {
                                        // NOTE: 0x40 means the empty byte string
                                        ReadByteData(this.stream, len, ms);
                                    }
                                }
                                if (ms.Position > Int32.MaxValue)
                                {
                                    throw new
                                    CBORException("Length of bytes to be streamed is bigger" +
                    "\u0020than supported ");
                                }
                                data = ms.ToArray();
                                return CBORObject.FromRaw(data);
                            }
                        }
                    case 3:
                        {
                            // Streaming text string
                            var builder = new StringBuilder();
                            while (true)
                            {
                                int nextByte = this.stream.ReadByte();
                                if (nextByte == 0xff)
                                {
                                    // break if the "break" code was read
                                    break;
                                }
                                long len = ReadDataLength(this.stream, nextByte, 3);
                                if ((len >> 63) != 0 || len > Int32.MaxValue)
                                {
                                    throw new CBORException("Length" + ToUnsignedEInteger(len) +
                                      " is bigger than supported");
                                }
                                if (nextByte != 0x60)
                                {
                                    // NOTE: 0x60 means the empty string
                                    if (PropertyMap.ExceedsKnownLength(this.stream, len))
                                    {
                                        throw new CBORException("Premature end of data");
                                    }
                                    switch (
                                      DataUtilities.ReadUtf8(
                                        this.stream,
                                        (int)len,
                                        builder,
                                        false))
                                    {
                                        case -1:
                                            throw new CBORException("Invalid UTF-8");
                                        case -2:
                                            throw new CBORException("Premature end of data");
                                    }
                                }
                            }
                            return CBORObject.FromRaw(builder.ToString());
                        }
                    case 4:
                        {
                            CBORObject cbor = CBORObject.NewArray();
                            var vtindex = 0;
                            // Indefinite-length array
                            while (true)
                            {
                                int headByte = this.stream.ReadByte();
                                if (headByte < 0)
                                {
                                    throw new CBORException("Premature end of data");
                                }
                                if (headByte == 0xff)
                                {
                                    // Break code was read
                                    break;
                                }
                                ++this.depth;
                                CBORObject o = this.ReadForFirstByte(
                                    headByte);
                                --this.depth;
                                cbor.Add(o);
                                ++vtindex;
                            }
                            return cbor;
                        }
                    case 5:
                        {
                            CBORObject cbor = CBORObject.NewMap();
                            // Indefinite-length map
                            while (true)
                            {
                                int headByte = this.stream.ReadByte();
                                if (headByte < 0)
                                {
                                    throw new CBORException("Premature end of data");
                                }
                                if (headByte == 0xff)
                                {
                                    // Break code was read
                                    break;
                                }
                                ++this.depth;
                                CBORObject key = this.ReadForFirstByte(headByte);
                                CBORObject value = this.ReadInternal();
                                --this.depth;
                                if (!this.options.AllowDuplicateKeys)
                                {
                                    if (cbor.ContainsKey(key))
                                    {
                                        throw new CBORException("Duplicate key already exists");
                                    }
                                }
                                cbor[key] = value;
                            }
                            return cbor;
                        }
                    default: throw new CBORException("Unexpected data encountered");
                }
            }
            EInteger bigintAdditional = EInteger.Zero;
            uadditional = ReadDataLength(this.stream, firstbyte, type);
            // The following doesn't check for major types 0 and 1,
            // since all of them are fixed-length types and are
            // handled in the call to GetFixedLengthObject.
            if (type >= 2 && type <= 5)
            {
                return this.ReadStringArrayMap(type, uadditional);
            }
            if (type == 6)
            { // Tagged item
                var haveFirstByte = false;
                var newFirstByte = -1;
                if (this.options.ResolveReferences && (uadditional >> 32) == 0)
                {
                    // NOTE: HandleItemTag treats only certain tags up to 256 specially
                    this.HandleItemTag(uadditional);
                }
                ++this.depth;
                CBORObject o = haveFirstByte ? this.ReadForFirstByte(
                    newFirstByte) : this.ReadInternal();
                --this.depth;
                if ((uadditional >> 63) != 0)
                {
                    return CBORObject.FromObjectAndTag(o,
                        ToUnsignedEInteger(uadditional));
                }
                if (uadditional < 65536)
                {
                    if (this.options.ResolveReferences)
                    {
                        int uaddl = uadditional >= 257 ? 257 : (uadditional < 0 ? 0 :
                            (int)uadditional);
                        switch (uaddl)
                        {
                            case 256:
                                // string tag
                                this.stringRefs.Pop();
                                break;
                            case 25:
                                // stringref tag
                                if (o.IsTagged || o.Type != CBORType.Integer)
                                {
                                    throw new CBORException("stringref must be an unsigned" +
                                      "\u0020integer");
                                }
                                return this.stringRefs.GetString(o.AsEIntegerValue());
                        }
                    }
                    return CBORObject.FromObjectAndTag(
                        o,
                        (int)uadditional);
                }
                return CBORObject.FromObjectAndTag(
                    o,
                    (EInteger)uadditional);
            }
            throw new CBORException("Unexpected data encountered");
        }

        private static readonly byte[] EmptyByteArray = new byte[0];

        private static byte[] ReadByteData(
          Stream stream,
          long uadditional,
          Stream outputStream)
        {
            if (uadditional == 0)
            {
                return EmptyByteArray;
            }
            if ((uadditional >> 63) != 0 || uadditional > Int32.MaxValue)
            {
                throw new CBORException("Length" + ToUnsignedEInteger(uadditional) +
                  " is bigger than supported ");
            }
            if (PropertyMap.ExceedsKnownLength(stream, uadditional))
            {
                throw new CBORException("Premature end of stream");
            }
            if (uadditional <= 0x10000)
            {
                // Simple case: small size
                var data = new byte[(int)uadditional];
                if (stream.Read(data, 0, data.Length) != data.Length)
                {
                    throw new CBORException("Premature end of stream");
                }
                if (outputStream != null)
                {
                    outputStream.Write(data, 0, data.Length);
                    return null;
                }
                return data;
            }
            else
            {
                var tmpdata = new byte[0x10000];
                var total = (int)uadditional;
                if (outputStream != null)
                {
                    while (total > 0)
                    {
                        int bufsize = Math.Min(tmpdata.Length, total);
                        if (stream.Read(tmpdata, 0, bufsize) != bufsize)
                        {
                            throw new CBORException("Premature end of stream");
                        }
                        outputStream.Write(tmpdata, 0, bufsize);
                        total -= bufsize;
                    }
                    return null;
                }
                using (var ms = new MemoryStream(0x10000))
                {
                    while (total > 0)
                    {
                        int bufsize = Math.Min(tmpdata.Length, total);
                        if (stream.Read(tmpdata, 0, bufsize) != bufsize)
                        {
                            throw new CBORException("Premature end of stream");
                        }
                        ms.Write(tmpdata, 0, bufsize);
                        total -= bufsize;
                    }
                    return ms.ToArray();
                }
            }
        }

        private static long ReadDataLength(
          Stream stream,
          int headByte,
          int expectedType)
        {
            return ReadDataLength(stream, headByte, expectedType, true);
        }

        private static long ReadDataLength(
          Stream stream,
          int headByte,
          int expectedType,
          bool allowNonShortest)
        {
            if (headByte < 0)
            {
                throw new CBORException("Unexpected data encountered");
            }
            if (((headByte >> 5) & 0x07) != expectedType)
            {
                throw new CBORException("Unexpected data encountered");
            }
            headByte &= 0x1f;
            if (headByte < 24)
            {
                return headByte;
            }
            var data = new byte[8];
            switch (headByte)
            {
                case 24:
                    {
                        int tmp = stream.ReadByte();
                        if (tmp < 0)
                        {
                            throw new CBORException("Premature end of data");
                        }
                        if (!allowNonShortest && tmp < 24)
                        {
                            throw new CBORException("Non-shortest CBOR form");
                        }
                        return tmp;
                    }
                case 25:
                    {
                        if (stream.Read(data, 0, 2) != 2)
                        {
                            throw new CBORException("Premature end of data");
                        }
                        int lowAdditional = ((int)(data[0] & (int)0xff)) << 8;
                        lowAdditional |= (int)(data[1] & (int)0xff);
                        if (!allowNonShortest && lowAdditional < 256)
                        {
                            throw new CBORException("Non-shortest CBOR form");
                        }
                        return lowAdditional;
                    }
                case 26:
                    {
                        if (stream.Read(data, 0, 4) != 4)
                        {
                            throw new CBORException("Premature end of data");
                        }
                        long uadditional = ((long)(data[0] & 0xffL)) << 24;
                        uadditional |= ((long)(data[1] & 0xffL)) << 16;
                        uadditional |= ((long)(data[2] & 0xffL)) << 8;
                        uadditional |= (long)(data[3] & 0xffL);
                        if (!allowNonShortest && (uadditional >> 16) == 0)
                        {
                            throw new CBORException("Non-shortest CBOR form");
                        }
                        return uadditional;
                    }
                case 27:
                    {
                        if (stream.Read(data, 0, 8) != 8)
                        {
                            throw new CBORException("Premature end of data");
                        }
                        // Treat return value as an unsigned integer
                        long uadditional = ((long)(data[0] & 0xffL)) << 56;
                        uadditional |= ((long)(data[1] & 0xffL)) << 48;
                        uadditional |= ((long)(data[2] & 0xffL)) << 40;
                        uadditional |= ((long)(data[3] & 0xffL)) << 32;
                        uadditional |= ((long)(data[4] & 0xffL)) << 24;
                        uadditional |= ((long)(data[5] & 0xffL)) << 16;
                        uadditional |= ((long)(data[6] & 0xffL)) << 8;
                        uadditional |= (long)(data[7] & 0xffL);
                        if (!allowNonShortest && (uadditional >> 32) == 0)
                        {
                            throw new CBORException("Non-shortest CBOR form");
                        }
                        return uadditional;
                    }
                case 28:
                case 29:
                case 30:
                    throw new CBORException("Unexpected data encountered");
                case 31:
                    throw new CBORException("Indefinite-length data not allowed" +
          "\u0020here");
                default: return headByte;
            }
        }

#if !NET20 && !NET40
        /*
        // - - - - - ASYNCHRONOUS METHODS

        private static async Task<int> ReadByteAsync(Stream stream) {
          var bytes = new byte[1];
          if (await stream.ReadAsync(bytes, 0, 1) == 0) {
            return -1;
          } else {
            return bytes[0];
          }
        }
        */
#endif
    }
    #endregion

    #region CBORUtilities
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:CBORUtilities"]/*'/>
    /// <summary>Contains utility methods that may have use outside of the
    /// CBORObject class.</summary>
    internal static class CBORUtilities
    {
        private const long DoublePosInfinity = unchecked((long)(0x7ffL << 52));
        private const string HexAlphabet = "0123456789ABCDEF";

        public static int CompareStringsAsUtf8LengthFirst(string strA, string
          strB)
        {
            if (strA == null)
            {
                return (strB == null) ? 0 : -1;
            }
            if (strB == null)
            {
                return 1;
            }
            if (strA.Length == 0)
            {
                return strB.Length == 0 ? 0 : -1;
            }
            if (strB.Length == 0)
            {
                return strA.Length == 0 ? 0 : 1;
            }
            var cmp = 0;
            if (strA.Length < 128 && strB.Length < 128)
            {
                int istrAUpperBound = strA.Length * 3;
                if (istrAUpperBound < strB.Length)
                {
                    return -1;
                }
                int istrBUpperBound = strB.Length * 3;
                if (istrBUpperBound < strA.Length)
                {
                    return 1;
                }
                cmp = 0;
                if (strA.Length == strB.Length)
                {
                    var equalStrings = true;
                    for (int i = 0; i < strA.Length; ++i)
                    {
                        if (strA[i] != strB[i])
                        {
                            equalStrings = false;
                            cmp = (strA[i] < strB[i]) ? -1 : 1;
                            break;
                        }
                    }
                    if (equalStrings)
                    {
                        return 0;
                    }
                }
                var nonAscii = false;
                for (int i = 0; i < strA.Length; ++i)
                {
                    if (strA[i] >= 0x80)
                    {
                        nonAscii = true;
                        break;
                    }
                }
                for (int i = 0; i < strB.Length; ++i)
                {
                    if (strB[i] >= 0x80)
                    {
                        nonAscii = true;
                        break;
                    }
                }
                if (!nonAscii)
                {
                    if (strA.Length != strB.Length)
                    {
                        return strA.Length < strB.Length ? -1 : 1;
                    }
                    if (strA.Length == strB.Length)
                    {
                        return cmp;
                    }
                }
            }
            else
            {
                long strAUpperBound = strA.Length * 3;
                if (strAUpperBound < strB.Length)
                {
                    return -1;
                }
                long strBUpperBound = strB.Length * 3;
                if (strBUpperBound < strA.Length)
                {
                    return 1;
                }
            }
            // DebugUtility.Log("slow path "+strA.Length+","+strB.Length);
            var sapos = 0;
            var sbpos = 0;
            long sautf8 = 0L;
            long sbutf8 = 0L;
            cmp = 0;
            var haveboth = true;
            while (true)
            {
                int sa = 0, sb = 0;
                if (sapos == strA.Length)
                {
                    haveboth = false;
                    if (sbutf8 > sautf8)
                    {
                        return -1;
                    }
                    else if (sbpos == strB.Length)
                    {
                        break;
                    }
                    else if (cmp == 0)
                    {
                        cmp = -1;
                    }
                }
                else
                {
                    sa = DataUtilities.CodePointAt(strA, sapos, 1);
                    if (sa < 0)
                    {
                        throw new ArgumentException("strA has unpaired surrogate");
                    }
                    if (sa >= 0x10000)
                    {
                        sautf8 += 4;
                        sapos += 2;
                    }
                    else if (sa >= 0x800)
                    {
                        sautf8 += 3;
                        ++sapos;
                    }
                    else if (sa >= 0x80)
                    {
                        sautf8 += 2;
                        ++sapos;
                    }
                    else
                    {
                        ++sautf8;
                        ++sapos;
                    }
                }
                if (sbpos == strB.Length)
                {
                    haveboth = false;
                    if (sautf8 > sbutf8)
                    {
                        return 1;
                    }
                    else if (sapos == strA.Length)
                    {
                        break;
                    }
                    else if (cmp == 0)
                    {
                        cmp = 1;
                    }
                }
                else
                {
                    sb = DataUtilities.CodePointAt(strB, sbpos, 1);
                    if (sb < 0)
                    {
                        throw new ArgumentException("strB has unpaired surrogate");
                    }
                    if (sb >= 0x10000)
                    {
                        sbutf8 += 4;
                        sbpos += 2;
                    }
                    else if (sb >= 0x800)
                    {
                        sbutf8 += 3;
                        ++sbpos;
                    }
                    else if (sb >= 0x80)
                    {
                        ++sbpos;
                        sbutf8 += 2;
                    }
                    else
                    {
                        ++sbpos;
                        ++sbutf8;
                    }
                }
                if (haveboth && cmp == 0 && sa != sb)
                {
                    cmp = (sa < sb) ? -1 : 1;
                }
            }
            return (sautf8 != sbutf8) ? (sautf8 < sbutf8 ? -1 : 1) : cmp;
        }

        public static int CompareUtf16Utf8LengthFirst(string utf16, byte[] utf8)
        {
            if (utf16 == null)
            {
                return (utf8 == null) ? 0 : -1;
            }
            if (utf8 == null)
            {
                return 1;
            }
            if (utf16.Length == 0)
            {
                return utf8.Length == 0 ? 0 : -1;
            }
            if (utf16.Length == 0)
            {
                return utf8.Length == 0 ? 0 : 1;
            }
            long strAUpperBound = utf16.Length * 3;
            if (strAUpperBound < utf8.Length)
            {
                return -1;
            }
            long strBUpperBound = utf16.Length * 3;
            if (strBUpperBound < utf8.Length)
            {
                return 1;
            }
            var u16pos = 0;
            var u8pos = 0;
            long u16u8length = 0L;
            var cmp = 0;
            var haveboth = true;
            while (true)
            {
                int u16 = 0, u8 = 0;
                if (u16pos == utf16.Length)
                {
                    haveboth = false;
                    if (u8pos > u16u8length)
                    {
                        return -1;
                    }
                    else if (u8pos == utf8.Length)
                    {
                        break;
                    }
                    else if (cmp == 0)
                    {
                        cmp = -1;
                    }
                }
                else
                {
                    u16 = DataUtilities.CodePointAt(utf16, u16pos, 1);
                    if (u16 < 0)
                    {
                        throw new ArgumentException("utf16 has unpaired surrogate");
                    }
                    if (u16 >= 0x10000)
                    {
                        u16u8length += 4;
                        u16pos += 2;
                    }
                    else if (u16 >= 0x800)
                    {
                        u16u8length += 3;
                        ++u16pos;
                    }
                    else if (u16 >= 0x80)
                    {
                        u16u8length += 2;
                        ++u16pos;
                    }
                    else
                    {
                        ++u16u8length;
                        ++u16pos;
                    }
                }
                if (u8pos == utf8.Length)
                {
                    haveboth = false;
                    if (cmp == 0)
                    {
                        return 1;
                    }
                    else if (u16pos == utf16.Length)
                    {
                        break;
                    }
                    else if (cmp == 0)
                    {
                        cmp = 1;
                    }
                }
                else
                {
                    u8 = Utf8CodePointAt(utf8, u8pos);
                    if (u8 < 0)
                    {
                        throw new ArgumentException("utf8 has invalid encoding");
                    }
                    if (u8 >= 0x10000)
                    {
                        u8pos += 4;
                    }
                    else if (u8 >= 0x800)
                    {
                        u8pos += 3;
                    }
                    else if (u8 >= 0x80)
                    {
                        u8pos += 2;
                    }
                    else
                    {
                        ++u8pos;
                    }
                }
                if (haveboth && cmp == 0 && u16 != u8)
                {
                    cmp = (u16 < u8) ? -1 : 1;
                }
            }
            return (u16u8length != u8pos) ? (u16u8length < u8pos ? -1 : 1) : cmp;
        }

        public static int Utf8CodePointAt(byte[] utf8, int offset)
        {
            int endPos = utf8.Length;
            if (offset < 0 || offset >= endPos)
            {
                return -1;
            }
            int c = ((int)utf8[offset]) & 0xff;
            if (c <= 0x7f)
            {
                return c;
            }
            else if (c >= 0xc2 && c <= 0xdf)
            {
                ++offset;
                int c1 = offset < endPos ?
                  ((int)utf8[offset]) & 0xff : -1;
                return (
                    c1 < 0x80 || c1 > 0xbf) ? -2 : (((c - 0xc0) << 6) |
                    (c1 - 0x80));
            }
            else if (c >= 0xe0 && c <= 0xef)
            {
                ++offset;
                int c1 = offset < endPos ? ((int)utf8[offset++]) & 0xff : -1;
                int c2 = offset < endPos ? ((int)utf8[offset]) & 0xff : -1;
                int lower = (c == 0xe0) ? 0xa0 : 0x80;
                int upper = (c == 0xed) ? 0x9f : 0xbf;
                return (c1 < lower || c1 > upper || c2 < 0x80 || c2 > 0xbf) ?
                  -2 : (((c - 0xe0) << 12) | ((c1 - 0x80) << 6) | (c2 - 0x80));
            }
            else if (c >= 0xf0 && c <= 0xf4)
            {
                ++offset;
                int c1 = offset < endPos ? ((int)utf8[offset++]) & 0xff : -1;
                int c2 = offset < endPos ? ((int)utf8[offset++]) & 0xff : -1;
                int c3 = offset < endPos ? ((int)utf8[offset]) & 0xff : -1;
                int lower = (c == 0xf0) ? 0x90 : 0x80;
                int upper = (c == 0xf4) ? 0x8f : 0xbf;
                if (c1 < lower || c1 > upper || c2 < 0x80 || c2 > 0xbf ||
                  c3 < 0x80 || c3 > 0xbf)
                {
                    return -2;
                }
                return ((c - 0xf0) << 18) | ((c1 - 0x80) << 12) | ((c2 - 0x80) <<
                    6) | (c3 - 0x80);
            }
            else
            {
                return -2;
            }
        }

        // NOTE: StringHashCode and Utf8HashCode must
        // return the same hash code for the same sequence
        // of Unicode code points. Both must treat an illegally
        // encoded subsequence as ending the sequence for
        // this purpose.
        public static int StringHashCode(string str)
        {
            var upos = 0;
            var code = 0x7edede19;
            while (true)
            {
                if (upos == str.Length)
                {
                    return code;
                }
                int sc = DataUtilities.CodePointAt(str, upos, 1);
                if (sc < 0)
                {
                    return code;
                }
                code = unchecked((code * 31) + sc);
                if (sc >= 0x10000)
                {
                    upos += 2;
                }
                else
                {
                    ++upos;
                }
            }
        }

        public static int Utf8HashCode(byte[] utf8)
        {
            var upos = 0;
            var code = 0x7edede19;
            while (true)
            {
                int sc = Utf8CodePointAt(utf8, upos);
                if (sc == -1)
                {
                    return code;
                }
                if (sc == -2)
                {
                    return code;
                }
                code = unchecked((code * 31) + sc);
                if (sc >= 0x10000)
                {
                    upos += 4;
                }
                else if (sc >= 0x800)
                {
                    upos += 3;
                }
                else if (sc >= 0x80)
                {
                    upos += 2;
                }
                else
                {
                    ++upos;
                }
            }
        }

        public static bool CheckUtf16(string str)
        {
            var upos = 0;
            while (true)
            {
                if (upos == str.Length)
                {
                    return true;
                }
                int sc = DataUtilities.CodePointAt(str, upos, 1);
                if (sc < 0)
                {
                    return false;
                }
                if (sc >= 0x10000)
                {
                    upos += 2;
                }
                else
                {
                    ++upos;
                }
            }
        }

        public static bool CheckUtf8(byte[] utf8)
        {
            var upos = 0;
            while (true)
            {
                int sc = Utf8CodePointAt(utf8, upos);
                if (sc == -1)
                {
                    return true;
                }
                if (sc == -2)
                {
                    return false;
                }
                if (sc >= 0x10000)
                {
                    upos += 4;
                }
                else if (sc >= 0x800)
                {
                    upos += 3;
                }
                else if (sc >= 0x80)
                {
                    upos += 2;
                }
                else
                {
                    ++upos;
                }
            }
        }

        public static bool StringEqualsUtf8(string str, byte[] utf8)
        {
            if (str == null)
            {
                return utf8 == null;
            }
            if (utf8 == null)
            {
                return false;
            }
            long strAUpperBound = str.Length * 3;
            if (strAUpperBound < utf8.Length)
            {
                return false;
            }
            long strBUpperBound = utf8.Length * 3;
            if (strBUpperBound < str.Length)
            {
                return false;
            }
            var spos = 0;
            var upos = 0;
            while (true)
            {
                int sc = DataUtilities.CodePointAt(str, spos, 1);
                int uc = Utf8CodePointAt(utf8, upos);
                if (uc == -2)
                {
                    throw new InvalidOperationException("Invalid encoding");
                }
                if (sc == -1)
                {
                    return uc == -1;
                }
                if (sc != uc)
                {
                    return false;
                }
                if (sc >= 0x10000)
                {
                    spos += 2;
                    upos += 4;
                }
                else if (sc >= 0x800)
                {
                    ++spos;
                    upos += 3;
                }
                else if (sc >= 0x80)
                {
                    ++spos;
                    upos += 2;
                }
                else
                {
                    ++spos;
                    ++upos;
                }
            }
        }
        public static bool ByteArrayEquals(byte[] a, byte[] b)
        {
            if (a == null)
            {
                return b == null;
            }
            if (b == null)
            {
                return false;
            }
            if (a.Length != b.Length)
            {
                return false;
            }
            for (var i = 0; i < a.Length; ++i)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static int ByteArrayHashCode(byte[] a)
        {
            if (a == null)
            {
                return 0;
            }
            var ret = 19;
            unchecked
            {
                ret = (ret * 31) + a.Length;
                for (var i = 0; i < a.Length; ++i)
                {
                    ret = (ret * 31) + a[i];
                }
            }
            return ret;
        }

        public static int ByteArrayCompare(byte[] a, byte[] b)
        {
            if (a == null)
            {
                return (b == null) ? 0 : -1;
            }
            if (b == null)
            {
                return 1;
            }
            int c = Math.Min(a.Length, b.Length);
            for (var i = 0; i < c; ++i)
            {
                if (a[i] != b[i])
                {
                    return (a[i] < b[i]) ? -1 : 1;
                }
            }
            return (a.Length != b.Length) ? ((a.Length < b.Length) ? -1 : 1) : 0;
        }

        public static int ByteArrayCompareLengthFirst(byte[] a, byte[] b)
        {
            if (a == null)
            {
                return (b == null) ? 0 : -1;
            }
            if (b == null)
            {
                return 1;
            }
            if (a.Length != b.Length)
            {
                return a.Length < b.Length ? -1 : 1;
            }
            for (var i = 0; i < a.Length; ++i)
            {
                if (a[i] != b[i])
                {
                    return (a[i] < b[i]) ? -1 : 1;
                }
            }
            return 0;
        }

        public static string TrimDotZero(string str)
        {
            return (str.Length > 2 && str[str.Length - 1] == '0' && str[str.Length
                  - 2] == '.') ? str.Substring(0, str.Length - 2) :
              str;
        }

        public static long DoubleToInt64Bits(double dbl)
        {
            return BitConverter.ToInt64(BitConverter.GetBytes((double)dbl), 0);
        }

        public static int SingleToInt32Bits(float flt)
        {
            return BitConverter.ToInt32(BitConverter.GetBytes((float)flt), 0);
        }

        public static double Int64BitsToDouble(long bits)
        {
            return BitConverter.ToDouble(
                BitConverter.GetBytes(bits),
                0);
        }

        public static float Int32BitsToSingle(int bits)
        {
            return BitConverter.ToSingle(
                BitConverter.GetBytes(bits),
                0);
        }

        [Obsolete]
        public static string DoubleToString(double dbl)
        {
            return EFloat.FromDouble(dbl).ToShortestString(EContext.Binary64);
        }

        public static string DoubleBitsToString(long dblbits)
        {
            return EFloat.FromDoubleBits(dblbits).ToShortestString(EContext.Binary64);
        }

        [Obsolete]
        public static string SingleToString(float sing)
        {
            return EFloat.FromSingle(sing).ToShortestString(EContext.Binary32);
        }

        public static string LongToString(long longValue)
        {
            if (longValue == Int64.MinValue)
            {
                return "-9223372036854775808";
            }
            if (longValue == 0L)
            {
                return "0";
            }
            if (longValue == (long)Int32.MinValue)
            {
                return "-2147483648";
            }
            bool neg = longValue < 0;
            var count = 0;
            char[] chars;
            int intlongValue = unchecked((int)longValue);
            if ((long)intlongValue == longValue)
            {
                chars = new char[12];
                count = 11;
                if (neg)
                {
                    intlongValue = -intlongValue;
                }
                while (intlongValue > 43698)
                {
                    int intdivValue = intlongValue / 10;
                    char digit = HexAlphabet[(int)(intlongValue - (intdivValue *
                            10))];
                    chars[count--] = digit;
                    intlongValue = intdivValue;
                }
                while (intlongValue > 9)
                {
                    int intdivValue = (intlongValue * 26215) >> 18;
                    char digit = HexAlphabet[(int)(intlongValue - (intdivValue *
                            10))];
                    chars[count--] = digit;
                    intlongValue = intdivValue;
                }
                if (intlongValue != 0)
                {
                    chars[count--] = HexAlphabet[(int)intlongValue];
                }
                if (neg)
                {
                    chars[count] = '-';
                }
                else
                {
                    ++count;
                }
                return new String(chars, count, 12 - count);
            }
            else
            {
                chars = new char[24];
                count = 23;
                if (neg)
                {
                    longValue = -longValue;
                }
                while (longValue > 43698)
                {
                    long divValue = longValue / 10;
                    char digit = HexAlphabet[(int)(longValue - (divValue * 10))];
                    chars[count--] = digit;
                    longValue = divValue;
                }
                while (longValue > 9)
                {
                    long divValue = (longValue * 26215) >> 18;
                    char digit = HexAlphabet[(int)(longValue - (divValue * 10))];
                    chars[count--] = digit;
                    longValue = divValue;
                }
                if (longValue != 0)
                {
                    chars[count--] = HexAlphabet[(int)longValue];
                }
                if (neg)
                {
                    chars[count] = '-';
                }
                else
                {
                    ++count;
                }
                return new String(chars, count, 24 - count);
            }
        }

        private static EInteger FloorDiv(EInteger a, EInteger n)
        {
            return a.Sign >= 0 ? a.Divide(n) : EInteger.FromInt32(-1).Subtract(
                EInteger.FromInt32(-1).Subtract(a).Divide(n));
        }

        private static EInteger FloorMod(EInteger a, EInteger n)
        {
            return a.Subtract(FloorDiv(a, n).Multiply(n));
        }

        private static readonly int[] ValueNormalDays = {
      0, 31, 28, 31, 30, 31, 30,
      31, 31, 30,
      31, 30, 31,
    };

        private static readonly int[] ValueLeapDays = {
      0, 31, 29, 31, 30, 31, 30,
      31, 31, 30,
      31, 30, 31,
    };

        private static readonly int[] ValueNormalToMonth = {
      0, 0x1f, 0x3b, 90, 120,
      0x97, 0xb5,
      0xd4, 0xf3, 0x111, 0x130, 0x14e, 0x16d,
    };

        private static readonly int[] ValueLeapToMonth = {
      0, 0x1f, 60, 0x5b, 0x79,
      0x98, 0xb6,
      0xd5, 0xf4, 0x112, 0x131, 0x14f, 0x16e,
    };

        public static void GetNormalizedPartProlepticGregorian(
          EInteger year,
          int month,
          EInteger day,
          EInteger[] dest)
        {
            // NOTE: This method assumes month is 1 to 12
            if (month <= 0 || month > 12)
            {
                throw new ArgumentOutOfRangeException(nameof(month));
            }
            int[] dayArray = (year.Remainder(4).Sign != 0 || (
                  year.Remainder(100).Sign == 0 && year.Remainder(400).Sign !=
                  0)) ? ValueNormalDays : ValueLeapDays;
            if (day.CompareTo(101) > 0)
            {
                EInteger count = day.Subtract(100).Divide(146097);
                day = day.Subtract(count.Multiply(146097));
                year = year.Add(count.Multiply(400));
            }
            while (true)
            {
                EInteger days = EInteger.FromInt32(dayArray[month]);
                if (day.Sign > 0 && day.CompareTo(days) <= 0)
                {
                    break;
                }
                if (day.CompareTo(days) > 0)
                {
                    day = day.Subtract(days);
                    if (month == 12)
                    {
                        month = 1;
                        year = year.Add(1);
                        dayArray = (year.Remainder(4).Sign != 0 || (
                              year.Remainder(100).Sign == 0 &&
                              year.Remainder(400).Sign != 0)) ? ValueNormalDays :
                          ValueLeapDays;
                    }
                    else
                    {
                        ++month;
                    }
                }
                if (day.Sign <= 0)
                {
                    --month;
                    if (month <= 0)
                    {
                        year = year.Add(-1);
                        month = 12;
                    }
                    dayArray = (year.Remainder(4).Sign != 0 || (
                          year.Remainder(100).Sign == 0 &&
                          year.Remainder(400).Sign != 0)) ? ValueNormalDays :
                      ValueLeapDays;
                    day = day.Add(dayArray[month]);
                }
            }
            dest[0] = year;
            dest[1] = EInteger.FromInt32(month);
            dest[2] = day;
        }

        /*
           // Example: Apple Time is a 32-bit unsigned integer
           // of the number of seconds since the start of 1904.
           // EInteger appleTime = GetNumberOfDaysProlepticGregorian(
             // year,
             month // ,
             day)
           // .Subtract(GetNumberOfDaysProlepticGregorian(
           // EInteger.FromInt32(1904),
           1 // ,
           s1));*/
        public static EInteger GetNumberOfDaysProlepticGregorian(
          EInteger year,
          int month,
          int mday)
        {
            // NOTE: month = 1 is January, year = 1 is year 1
            if (month <= 0 || month > 12)
            {
                throw new ArgumentOutOfRangeException(nameof(month));
            }
            if (mday <= 0 || mday > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(mday));
            }
            EInteger numDays = EInteger.Zero;
            var startYear = 1970;
            if (year.CompareTo(startYear) < 0)
            {
                EInteger ei = EInteger.FromInt32(startYear - 1);
                EInteger diff = ei.Subtract(year);

                if (diff.CompareTo(401) > 0)
                {
                    EInteger blocks = diff.Subtract(401).Divide(400);
                    numDays = numDays.Subtract(blocks.Multiply(146097));
                    diff = diff.Subtract(blocks.Multiply(400));
                    ei = ei.Subtract(blocks.Multiply(400));
                }

                numDays = numDays.Subtract(diff.Multiply(365));
                var decrement = 1;
                for (;
                  ei.CompareTo(year) > 0;
                  ei = ei.Subtract(decrement))
                {
                    if (decrement == 1 && ei.Remainder(4).Sign == 0)
                    {
                        decrement = 4;
                    }
                    if (!(ei.Remainder(4).Sign != 0 || (
                          ei.Remainder(100).Sign == 0 && ei.Remainder(400).Sign !=
                          0)))
                    {
                        numDays = numDays.Subtract(1);
                    }
                }
                if (year.Remainder(4).Sign != 0 || (
                    year.Remainder(100).Sign == 0 && year.Remainder(400).Sign != 0))
                {
                    numDays = numDays.Subtract(365 - ValueNormalToMonth[month])
                      .Subtract(ValueNormalDays[month] - mday + 1);
                }
                else
                {
                    numDays = numDays
                      .Subtract(366 - ValueLeapToMonth[month])
                      .Subtract(ValueLeapDays[month] - mday + 1);
                }
            }
            else
            {
                bool isNormalYear = year.Remainder(4).Sign != 0 ||
                  (year.Remainder(100).Sign == 0 && year.Remainder(400).Sign != 0);

                EInteger ei = EInteger.FromInt32(startYear);
                if (ei.Add(401).CompareTo(year) < 0)
                {
                    EInteger y2 = year.Subtract(2);
                    numDays = numDays.Add(
                        y2.Subtract(startYear).Divide(400).Multiply(146097));
                    ei = y2.Subtract(
                        y2.Subtract(startYear).Remainder(400));
                }

                EInteger diff = year.Subtract(ei);
                numDays = numDays.Add(diff.Multiply(365));
                EInteger eileap = ei;
                if (ei.Remainder(4).Sign != 0)
                {
                    eileap = eileap.Add(4 - eileap.Remainder(4).ToInt32Checked());
                }
                numDays = numDays.Add(year.Subtract(eileap).Add(3).Divide(4));
                if (ei.Remainder(100).Sign != 0)
                {
                    ei = ei.Add(100 - ei.Remainder(100).ToInt32Checked());
                }
                while (ei.CompareTo(year) < 0)
                {
                    if (ei.Remainder(400).Sign != 0)
                    {
                        numDays = numDays.Subtract(1);
                    }
                    ei = ei.Add(100);
                }
                int yearToMonth = isNormalYear ? ValueNormalToMonth[month - 1] :
                  ValueLeapToMonth[month - 1];
                numDays = numDays.Add(yearToMonth)
                  .Add(mday - 1);
            }
            return numDays;
        }

        public static void BreakDownSecondsSinceEpoch(
          EDecimal edec,
          EInteger[] year,
          int[] lesserFields)
        {
            EInteger integerPart = edec.Quantize(0, ERounding.Floor)
              .ToEInteger();
            EDecimal fractionalPart = edec.Abs()
              .Subtract(EDecimal.FromEInteger(integerPart).Abs());
            int nanoseconds = fractionalPart.Multiply(1000000000)
              .ToInt32Checked();
            var normPart = new EInteger[3];
            EInteger days = FloorDiv(
                integerPart,
                EInteger.FromInt32(86400)).Add(1);
            int secondsInDay = FloorMod(
                integerPart,
                EInteger.FromInt32(86400)).ToInt32Checked();
            GetNormalizedPartProlepticGregorian(
              EInteger.FromInt32(1970),
              1,
              days,
              normPart);
            lesserFields[0] = normPart[1].ToInt32Checked();
            lesserFields[1] = normPart[2].ToInt32Checked();
            lesserFields[2] = secondsInDay / 3600;
            lesserFields[3] = (secondsInDay % 3600) / 60;
            lesserFields[4] = secondsInDay % 60;
            lesserFields[5] = nanoseconds / 100;
            lesserFields[6] = 0;
            year[0] = normPart[0];
        }

        public static bool NameStartsWithWord(String name, String word)
        {
            int wl = word.Length;
            return name.Length > wl && name.Substring(0, wl).Equals(word,
                StringComparison.Ordinal) && !(name[wl] >= 'a' && name[wl] <=
                'z') && !(name[wl] >= '0' && name[wl] <= '9');
        }

        public static String FirstCharLower(String name)
        {
            if (name.Length > 0 && name[0] >= 'A' && name[0] <= 'Z')
            {
                var sb = new StringBuilder();
                sb.Append((char)(name[0] + 0x20));
                sb.Append(name.Substring(1));
                return sb.ToString();
            }
            return name;
        }

        public static String FirstCharUpper(String name)
        {
            if (name.Length > 0 && name[0] >= 'a' && name[0] <= 'z')
            {
                var sb = new StringBuilder();
                sb.Append((char)(name[0] - 0x20));
                sb.Append(name.Substring(1));
                return sb.ToString();
            }
            return name;
        }

        private static bool IsValidDateTime(int[] dateTime)
        {
            if (dateTime == null || dateTime.Length < 8)
            {
                return false;
            }
            if (dateTime[1] < 1 || dateTime[1] > 12 || dateTime[2] < 1)
            {
                return false;
            }
            bool leap = IsLeapYear(dateTime[0]);
            if (dateTime[1] == 4 || dateTime[1] == 6 || dateTime[1] == 9 ||
              dateTime[1] == 11)
            {
                if (dateTime[2] > 30)
                {
                    return false;
                }
            }
            else if (dateTime[1] == 2)
            {
                if (dateTime[2] > (leap ? 29 : 28))
                {
                    return false;
                }
            }
            else
            {
                if (dateTime[2] > 31)
                {
                    return false;
                }
            }
            return !(dateTime[3] < 0 || dateTime[4] < 0 || dateTime[5] < 0 ||
                dateTime[3] >= 24 || dateTime[4] >= 60 || dateTime[5] >= 61 ||
                dateTime[6] < 0 ||
                dateTime[6] >= 1000000000 || dateTime[7] <= -1440 ||
                dateTime[7] >= 1440);
        }

        private static bool IsLeapYear(int yr)
        {
            yr %= 400;
            if (yr < 0)
            {
                yr += 400;
            }
            return (((yr % 4) == 0) && ((yr % 100) != 0)) || ((yr % 400) == 0);
        }

        public static void ParseAtomDateTimeString(
          string str,
          EInteger[] bigYearArray,
          int[] lf)
        {
            int[] d = ParseAtomDateTimeString(str);
            bigYearArray[0] = EInteger.FromInt32(d[0]);
            Array.Copy(d, 1, lf, 0, 7);
        }

        private static int[] ParseAtomDateTimeString(string str)
        {
            var bad = false;
            if (str.Length < 19)
            {
                throw new ArgumentException("Invalid date/time");
            }
            for (var i = 0; i < 19 && !bad; ++i)
            {
                if (i == 4 || i == 7)
                {
                    bad |= str[i] != '-';
                }
                else if (i == 13 || i == 16)
                {
                    bad |= str[i] != ':';
                }
                else if (i == 10)
                {
                    bad |= str[i] != 'T';
                    /*lowercase t not used to separate date/time,
                    following RFC 4287 sec. 3.3*/
                }
                else
                {
                    bad |= str[i] < '0' || str[i] >
                      '9';
                }
            }
            if (bad)
            {
                throw new ArgumentException("Invalid date/time");
            }
            int year = ((str[0] - '0') * 1000) + ((str[1] - '0') * 100) +
              ((str[2] - '0') * 10) + (str[3] - '0');
            int month = ((str[5] - '0') * 10) + (str[6] - '0');
            int day = ((str[8] - '0') * 10) + (str[9] - '0');
            int hour = ((str[11] - '0') * 10) + (str[12] - '0');
            int minute = ((str[14] - '0') * 10) + (str[15] - '0');
            int second = ((str[17] - '0') * 10) + (str[18] - '0');
            var index = 19;
            var nanoSeconds = 0;
            if (index <= str.Length && str[index] == '.')
            {
                var icount = 0;
                ++index;
                while (index < str.Length)
                {
                    if (str[index] < '0' || str[index] > '9')
                    {
                        break;
                    }
                    if (icount < 9)
                    {
                        nanoSeconds = (nanoSeconds * 10) + (str[index] - '0');
                        ++icount;
                    }
                    ++index;
                }
                while (icount < 9)
                {
                    nanoSeconds *= 10;
                    ++icount;
                }
            }
            var utcToLocal = 0;
            if (index + 1 == str.Length && str[index] == 'Z')
            {
                /*lowercase z not used to indicate UTC,
                  following RFC 4287 sec. 3.3*/
                utcToLocal = 0;
            }
            else if (index + 6 == str.Length)
            {
                bad = false;
                for (var i = 0; i < 6 && !bad; ++i)
                {
                    if (i == 0)
                    {
                        bad |= str[index + i] != '-' && str[index + i] != '+';
                    }
                    else if (i == 3)
                    {
                        bad |= str[index + i] != ':';
                    }
                    else
                    {
                        bad |= str[index + i] < '0' || str[index + i] > '9';
                    }
                }
                if (bad)
                {
                    throw new ArgumentException("Invalid date/time");
                }
                bool neg = str[index] == '-';
                int tzhour = ((str[index + 1] - '0') * 10) + (str[index + 2] - '0');
                int tzminute = ((str[index + 4] - '0') * 10) + (str[index + 5] - '0');
                if (tzminute >= 60)
                {
                    throw new ArgumentException("Invalid date/time");
                }
                utcToLocal = ((neg ? -1 : 1) * (tzhour * 60)) + tzminute;
            }
            else
            {
                throw new ArgumentException("Invalid date/time");
            }
            int[] dt = {
        year, month, day, hour, minute, second,
        nanoSeconds, utcToLocal,
      };
            if (!IsValidDateTime(dt))
            {
                throw new ArgumentException("Invalid date/time");
            }
            return dt;
        }

        public static string ToAtomDateTimeString(
          EInteger bigYear,
          int[] lesserFields)
        {
            if (lesserFields[6] != 0)
            {
                throw new NotSupportedException(
                  "Local time offsets not supported");
            }
            int smallYear = bigYear.ToInt32Checked();
            if (smallYear < 0)
            {
                throw new ArgumentException("year(" + smallYear +
                  ") is not greater or equal to 0");
            }
            if (smallYear > 9999)
            {
                throw new ArgumentException("year(" + smallYear +
                  ") is not less or equal to 9999");
            }
            int month = lesserFields[0];
            int day = lesserFields[1];
            int hour = lesserFields[2];
            int minute = lesserFields[3];
            int second = lesserFields[4];
            int fracSeconds = lesserFields[5];
            var charbuf = new char[32];
            charbuf[0] = (char)('0' + ((smallYear / 1000) % 10));
            charbuf[1] = (char)('0' + ((smallYear / 100) % 10));
            charbuf[2] = (char)('0' + ((smallYear / 10) % 10));
            charbuf[3] = (char)('0' + (smallYear % 10));
            charbuf[4] = '-';
            charbuf[5] = (char)('0' + ((month / 10) % 10));
            charbuf[6] = (char)('0' + (month % 10));
            charbuf[7] = '-';
            charbuf[8] = (char)('0' + ((day / 10) % 10));
            charbuf[9] = (char)('0' + (day % 10));
            charbuf[10] = 'T';
            charbuf[11] = (char)('0' + ((hour / 10) % 10));
            charbuf[12] = (char)('0' + (hour % 10));
            charbuf[13] = ':';
            charbuf[14] = (char)('0' + ((minute / 10) % 10));
            charbuf[15] = (char)('0' + (minute % 10));
            charbuf[16] = ':';
            charbuf[17] = (char)('0' + ((second / 10) % 10));
            charbuf[18] = (char)('0' + (second % 10));
            var charbufLength = 19;
            if (fracSeconds > 0)
            {
                charbuf[19] = '.';
                ++charbufLength;
                var digitdiv = 100000000;
                var index = 20;
                while (digitdiv > 0 && fracSeconds != 0)
                {
                    int digit = (fracSeconds / digitdiv) % 10;
                    fracSeconds -= digit * digitdiv;
                    charbuf[index++] = (char)('0' + digit);
                    ++charbufLength;
                    digitdiv /= 10;
                }
                charbuf[index] = 'Z';
                ++charbufLength;
            }
            else
            {
                charbuf[19] = 'Z';
                ++charbufLength;
            }
            return new String(charbuf, 0, charbufLength);
        }

        public static long IntegerToDoubleBits(int i)
        {
            if (i == Int32.MinValue)
            {
                return unchecked((long)0xc1e0000000000000L);
            }
            long longmant = Math.Abs(i);
            var expo = 0;
            while (longmant < (1 << 52))
            {
                longmant <<= 1;
                --expo;
            }
            // Clear the high bits where the exponent and sign are
            longmant &= 0xfffffffffffffL;
            longmant |= (long)(expo + 1075) << 52;
            if (i < 0)
            {
                longmant |= unchecked((long)(1L << 63));
            }
            return longmant;
        }

        public static bool IsBeyondSafeRange(long bits)
        {
            // Absolute value of double is greater than 9007199254740991.0,
            // or value is NaN
            bits &= ~(1L << 63);
            return bits >= DoublePosInfinity || bits > 0x433fffffffffffffL;
        }

        public static bool IsIntegerValue(long bits)
        {
            bits &= ~(1L << 63);
            if (bits == 0)
            {
                return true;
            }
            // Infinity and NaN
            if (bits >= DoublePosInfinity)
            {
                return false;
            }
            // Beyond non-integer range
            if ((bits >> 52) >= 0x433)
            {
                return true;
            }
            // Less than 1
            if ((bits >> 52) <= 0x3fe)
            {
                return false;
            }
            var exp = (int)(bits >> 52);
            long mant = bits & ((1L << 52) - 1);
            int shift = 52 - (exp - 0x3ff);
            return ((mant >> shift) << shift) == mant;
        }

        public static long GetIntegerValue(long bits)
        {
            long sgn;
            sgn = ((bits >> 63) != 0) ? -1L : 1L;
            bits &= ~(1L << 63);
            if (bits == 0)
            {
                return 0;
            }
            // Infinity and NaN
            if (bits >= DoublePosInfinity)
            {
                throw new NotSupportedException();
            }
            // Beyond safe range
            if ((bits >> 52) >= 0x434)
            {
                throw new NotSupportedException();
            }
            // Less than 1
            if ((bits >> 52) <= 0x3fe)
            {
                throw new NotSupportedException();
            }
            var exp = (int)(bits >> 52);
            long mant = bits & ((1L << 52) - 1);
            mant |= 1L << 52;
            int shift = 52 - (exp - 0x3ff);
            return (mant >> shift) * sgn;
        }

        [Obsolete]
        public static EInteger EIntegerFromDouble(double dbl)
        {
            return EIntegerFromDoubleBits(BitConverter.ToInt64(
                BitConverter.GetBytes((double)dbl),
                0));
        }

        public static EInteger EIntegerFromDoubleBits(long lvalue)
        {
            int value0 = unchecked((int)(lvalue & 0xffffffffL));
            int value1 = unchecked((int)((lvalue >> 32) & 0xffffffffL));
            var floatExponent = (int)((value1 >> 20) & 0x7ff);
            bool neg = (value1 >> 31) != 0;
            if (floatExponent == 2047)
            {
                throw new OverflowException("Value is infinity or NaN");
            }
            value1 &= 0xfffff; // Mask out the exponent and sign
            if (floatExponent == 0)
            {
                ++floatExponent;
            }
            else
            {
                value1 |= 0x100000;
            }
            if ((value1 | value0) != 0)
            {
                while ((value0 & 1) == 0)
                {
                    value0 >>= 1;
                    value0 &= 0x7fffffff;
                    value0 = unchecked(value0 | (value1 << 31));
                    value1 >>= 1;
                    ++floatExponent;
                }
            }
            floatExponent -= 1075;
            var bytes = new byte[9];
            EInteger bigmantissa;
            bytes[0] = (byte)(value0 & 0xff);
            bytes[1] = (byte)((value0 >> 8) & 0xff);
            bytes[2] = (byte)((value0 >> 16) & 0xff);
            bytes[3] = (byte)((value0 >> 24) & 0xff);
            bytes[4] = (byte)(value1 & 0xff);
            bytes[5] = (byte)((value1 >> 8) & 0xff);
            bytes[6] = (byte)((value1 >> 16) & 0xff);
            bytes[7] = (byte)((value1 >> 24) & 0xff);
            bytes[8] = (byte)0;
            bigmantissa = EInteger.FromBytes(bytes, true);
            if (floatExponent == 0)
            {
                if (neg)
                {
                    bigmantissa = -bigmantissa;
                }
                return bigmantissa;
            }
            if (floatExponent > 0)
            {
                // Value is an integer
                bigmantissa <<= floatExponent;
                if (neg)
                {
                    bigmantissa = -(EInteger)bigmantissa;
                }
                return bigmantissa;
            }
            else
            {
                // Value has a fractional part
                int exp = -floatExponent;
                bigmantissa >>= exp;
                if (neg)
                {
                    bigmantissa = -(EInteger)bigmantissa;
                }
                return bigmantissa;
            }
        }

        public static bool DoubleBitsNaN(long bits)
        {
            // Is NaN
            bits &= ~(1L << 63);
            return bits > unchecked((long)(0x7ffL << 52));
        }

        public static bool DoubleBitsFinite(long bits)
        {
            // Neither NaN nor infinity
            bits &= ~(1L << 63);
            return bits < unchecked((long)(0x7ffL << 52));
        }

        private static int RoundedShift(long mant, int shift)
        {
            long mask = (1L << shift) - 1;
            long half = 1L << (shift - 1);
            long shifted = mant >> shift;
            long masked = mant & mask;
            return (masked > half || (masked == half && (shifted & 1L) != 0)) ?
              unchecked((int)shifted) + 1 : unchecked((int)shifted);
        }

        private static int RoundedShift(int mant, int shift)
        {
            int mask = (1 << shift) - 1;
            int half = 1 << (shift - 1);
            int shifted = mant >> shift;
            int masked = mant & mask;
            return (masked > half || (masked == half && (shifted & 1) != 0)) ?
              shifted + 1 : shifted;
        }

        public static int DoubleToHalfPrecisionIfSameValue(long bits)
        {
            int exp = unchecked((int)((bits >> 52) & 0x7ffL));
            long mant = bits & 0xfffffffffffffL;
            int sign = unchecked((int)(bits >> 48)) & (1 << 15);
            int sexp = exp - 1008;
            // DebugUtility.Log("bits={0:X8}, exp=" + exp + " sexp=" + (sexp),bits);
            if (exp == 2047)
            { // Infinity and NaN
                int newmant = unchecked((int)(mant >> 42));
                return ((mant & ((1L << 42) - 1)) == 0) ? (sign | 0x7c00 | newmant) :
                  -1;
            }
            else if (sexp >= 31)
            { // overflow
                return -1;
            }
            else if (sexp < -10)
            { // underflow
                return -1;
            }
            else if (sexp > 0)
            { // normal
                return ((mant & ((1L << 42) - 1)) == 0) ? (sign | (sexp << 10) |
                    RoundedShift(mant, 42)) : -1;
            }
            else
            { // subnormal and zero
                int rs = RoundedShift(mant | (1L << 52), 42 - (sexp - 1));
                // DebugUtility.Log("mant=" + mant + " rs=" + (rs));
                if (sexp == -10 && rs == 0)
                {
                    return -1;
                }
                return ((mant & ((1L << (42 - (sexp - 1))) - 1)) == 0) ? (sign | rs) :
        -1;
            }
        }

        public static bool DoubleRetainsSameValueInSingle(long bits)
        {
            if ((bits & ~(1L << 63)) == 0)
            {
                return true;
            }
            int exp = unchecked((int)((bits >> 52) & 0x7ffL));
            long mant = bits & 0xfffffffffffffL;
            int sexp = exp - 896;
            // DebugUtility.Log("sng mant={0:X8}, exp=" + exp + " sexp=" + (sexp));
            if (exp == 2047)
            { // Infinity and NaN
                return (mant & ((1L << 29) - 1)) == 0;
            }
            else if (sexp < -23 || sexp >= 255)
            { // underflow or overflow
                return false;
            }
            else if (sexp > 0)
            { // normal
                return (mant & ((1L << 29) - 1)) == 0;
            }
            else if (sexp == -23)
            {
                return (mant & ((1L << (29 - (sexp - 1))) - 1)) == 0 &&
                   RoundedShift(mant | (1L << 52), 29 - (sexp - 1)) != 0;
            }
            else
            { // subnormal and zero
                return (mant & ((1L << (29 - (sexp - 1))) - 1)) == 0;
            }
        }

        // NOTE: Rounds to nearest, ties to even
        public static int SingleToRoundedHalfPrecision(int bits)
        {
            int exp = unchecked((int)((bits >> 23) & 0xff));
            int mant = bits & 0x7fffff;
            int sign = (bits >> 16) & (1 << 15);
            int sexp = exp - 112;
            if (exp == 255)
            { // Infinity and NaN
                int newmant = unchecked((int)(mant >> 13));
                return (mant != 0 && newmant == 0) ?
                  // signaling NaN truncated to have mantissa 0
                  (sign | 0x7c01) : (sign | 0x7c00 | newmant);
            }
            else if (sexp >= 31)
            { // overflow
                return sign | 0x7c00;
            }
            else if (sexp < -10)
            { // underflow
                return sign;
            }
            else if (sexp > 0)
            { // normal
                return sign | (sexp << 10) | RoundedShift(mant, 13);
            }
            else
            { // subnormal and zero
                return sign | RoundedShift(mant | (1 << 23), 13 - (sexp - 1));
            }
        }

        // NOTE: Rounds to nearest, ties to even
        public static int DoubleToRoundedHalfPrecision(long bits)
        {
            int exp = unchecked((int)((bits >> 52) & 0x7ffL));
            long mant = bits & 0xfffffffffffffL;
            int sign = unchecked((int)(bits >> 48)) & (1 << 15);
            int sexp = exp - 1008;
            if (exp == 2047)
            { // Infinity and NaN
                int newmant = unchecked((int)(mant >> 42));
                return (mant != 0 && newmant == 0) ?
                  // signaling NaN truncated to have mantissa 0
                  (sign | 0x7c01) : (sign | 0x7c00 | newmant);
            }
            else if (sexp >= 31)
            { // overflow
                return sign | 0x7c00;
            }
            else if (sexp < -10)
            { // underflow
                return sign;
            }
            else if (sexp > 0)
            { // normal
                return sign | (sexp << 10) | RoundedShift(mant, 42);
            }
            else
            { // subnormal and zero
                return sign | RoundedShift(mant | (1L << 52), 42 - (sexp - 1));
            }
        }

        // NOTE: Rounds to nearest, ties to even
        public static int DoubleToRoundedSinglePrecision(long bits)
        {
            int exp = unchecked((int)((bits >> 52) & 0x7ffL));
            long mant = bits & 0xfffffffffffffL;
            int sign = unchecked((int)(bits >> 32)) & (1 << 31);
            int sexp = exp - 896;
            if (exp == 2047)
            { // Infinity and NaN
                int newmant = unchecked((int)(mant >> 29));
                return (mant != 0 && newmant == 0) ?
                  // signaling NaN truncated to have mantissa 0
                  (sign | 0x7f800001) : (sign | 0x7f800000 | newmant);
            }
            else if (sexp >= 255)
            { // overflow
                return sign | 0x7f800000;
            }
            else if (sexp < -23)
            { // underflow
                return sign;
            }
            else if (sexp > 0)
            { // normal
                return sign | (sexp << 23) | RoundedShift(mant, 29);
            }
            else
            { // subnormal and zero
                return sign | RoundedShift(mant | (1L << 52), 29 - (sexp - 1));
            }
        }

        public static int SingleToHalfPrecisionIfSameValue(float f)
        {
            int bits = BitConverter.ToInt32(BitConverter.GetBytes((float)f), 0);
            int exp = (bits >> 23) & 0xff;
            int mant = bits & 0x7fffff;
            int sign = (bits >> 16) & 0x8000;
            if (exp == 255)
            { // Infinity and NaN
                return (bits & 0x1fff) == 0 ? sign + 0x7c00 + (mant >> 13) : -1;
            }
            else if (exp == 0)
            { // Subnormal
                return (bits & 0x1fff) == 0 ? sign + (mant >> 13) : -1;
            }
            if (exp <= 102 || exp >= 143)
            { // Overflow or underflow
                return -1;
            }
            else if (exp <= 112)
            { // Subnormal
                int shift = 126 - exp;
                int rs = (1024 >> (145 - exp)) + (mant >> shift);
                return (mant != 0 && exp == 103) ? (-1) : ((bits & ((1 << shift) -
        1)) == 0 ? sign + rs : -1);
            }
            else
            {
                return (bits & 0x1fff) == 0 ? sign + ((exp - 112) << 10) +
        -(mant >> 13) : -1;
            }
        }

        public static long SingleToDoublePrecision(int bits)
        {
            var negvalue = (long)((bits >> 31) & 1) << 63;
            int exp = (bits >> 23) & 0xff;
            int mant = bits & 0x7fffff;
            long value = 0;
            if (exp == 255)
            {
                value = 0x7ff0000000000000L | ((long)mant << 29) | negvalue;
            }
            else if (exp == 0)
            {
                if (mant == 0)
                {
                    value = negvalue;
                }
                else
                {
                    ++exp;
                    while (mant < 0x800000)
                    {
                        mant <<= 1;
                        --exp;
                    }
                    value = ((long)(exp + 896) << 52) | ((long)(mant & 0x7fffff) <<
                        29) | negvalue;
                }
            }
            else
            {
                value = ((long)(exp + 896) << 52) | ((long)mant << 29) | negvalue;
            }
            return value;
        }

        public static long HalfToDoublePrecision(int bits)
        {
            var negvalue = (long)(bits & 0x8000) << 48;
            int exp = (bits >> 10) & 31;
            int mant = bits & 0x3ff;
            long value = 0;
            if (exp == 31)
            {
                value = 0x7ff0000000000000L | ((long)mant << 42) | negvalue;
            }
            else if (exp == 0)
            {
                if (mant == 0)
                {
                    value = negvalue;
                }
                else
                {
                    ++exp;
                    while (mant < 0x400)
                    {
                        mant <<= 1;
                        --exp;
                    }
                    value = ((long)(exp + 1008) << 52) | ((long)(mant & 0x3ff) << 42) |
                      negvalue;
                }
            }
            else
            {
                value = ((long)(exp + 1008) << 52) | ((long)mant << 42) | negvalue;
            }
            return value;
        }
    }
    #endregion

    #region CBORDataUtilitiesByteArrayString
    internal static class CBORDataUtilitiesByteArrayString
    {
        private const long DoubleNegInfinity = unchecked((long)(0xfffL << 52));
        private const long DoublePosInfinity = unchecked((long)(0x7ffL << 52));

        internal static CBORObject ParseJSONNumber(
          byte[] chars,
          int offset,
          int count,
          JSONOptions options,
          int[] endOfNumber)
        {
            if (chars == null || chars.Length == 0 || count <= 0)
            {
                return null;
            }
            if (offset < 0 || offset > chars.Length)
            {
                return null;
            }
            if (count > chars.Length || chars.Length - offset < count)
            {
                return null;
            }
            JSONOptions opt = options ?? CBORDataUtilities.DefaultOptions;
            bool preserveNegativeZero = options.PreserveNegativeZero;
            JSONOptions.ConversionMode kind = options.NumberConversion;
            int endPos = offset + count;
            int initialOffset = offset;
            var negative = false;
            if (chars[initialOffset] == '-')
            {
                ++offset;
                negative = true;
            }
            int numOffset = offset;
            var haveDecimalPoint = false;
            var haveDigits = false;
            var haveDigitsAfterDecimal = false;
            var haveExponent = false;
            int i = offset;
            var decimalPointPos = -1;
            // Check syntax
            int k = i;
            if (endPos - 1 > k && chars[k] == '0' && chars[k + 1] >= '0' &&
              chars[k + 1] <= '9')
            {
                if (endOfNumber != null)
                {
                    endOfNumber[0] = k + 2;
                }
                return null;
            }
            for (; k < endPos; ++k)
            {
                byte c = chars[k];
                if (c >= '0' && c <= '9')
                {
                    haveDigits = true;
                    haveDigitsAfterDecimal |= haveDecimalPoint;
                }
                else if (c == '.')
                {
                    if (!haveDigits || haveDecimalPoint)
                    {
                        // no digits before the decimal point,
                        // or decimal point already seen
                        if (endOfNumber != null)
                        {
                            endOfNumber[0] = k;
                        }
                        return null;
                    }
                    haveDecimalPoint = true;
                    decimalPointPos = k;
                }
                else if (c == 'E' || c == 'e')
                {
                    ++k;
                    haveExponent = true;
                    break;
                }
                else
                {
                    if (endOfNumber != null)
                    {
                        endOfNumber[0] = k;
                        // Check if character can validly appear after a JSON number
                        if (c != ',' && c != ']' && c != '}' &&
                          c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09)
                        {
                            return null;
                        }
                        else
                        {
                            endPos = k;
                            break;
                        }
                    }
                    return null;
                }
            }
            if (!haveDigits || (haveDecimalPoint && !haveDigitsAfterDecimal))
            {
                if (endOfNumber != null)
                {
                    endOfNumber[0] = k;
                }
                return null;
            }
            var exponentPos = -1;
            var negativeExp = false;
            if (haveExponent)
            {
                haveDigits = false;
                if (k == endPos)
                {
                    if (endOfNumber != null)
                    {
                        endOfNumber[0] = k;
                    }
                    return null;
                }
                byte c = chars[k];
                if (c == '+')
                {
                    ++k;
                }
                else if (c == '-')
                {
                    negativeExp = true;
                    ++k;
                }
                for (; k < endPos; ++k)
                {
                    c = chars[k];
                    if (c >= '0' && c <= '9')
                    {
                        if (exponentPos < 0 && c != '0')
                        {
                            exponentPos = k;
                        }
                        haveDigits = true;
                    }
                    else if (endOfNumber != null)
                    {
                        endOfNumber[0] = k;
                        // Check if character can validly appear after a JSON number
                        if (c != ',' && c != ']' && c != '}' &&
                          c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09)
                        {
                            return null;
                        }
                        else
                        {
                            endPos = k;
                            break;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                if (!haveDigits)
                {
                    if (endOfNumber != null)
                    {
                        endOfNumber[0] = k;
                    }
                    return null;
                }
            }
            if (endOfNumber != null)
            {
                endOfNumber[0] = endPos;
            }
            if (exponentPos >= 0 && endPos - exponentPos > 20)
            {
                // Exponent too high for precision to overcome (which
                // has a length no bigger than Int32.MaxValue, which is 10 digits
                // long)
                if (negativeExp)
                {
                    // underflow
                    if (kind == JSONOptions.ConversionMode.Double ||
                      kind == JSONOptions.ConversionMode.IntOrFloat)
                    {
                        if (!negative)
                        {
                            return CBORObject.FromFloatingPointBits(0, 2);
                        }
                        else
                        {
                            return CBORObject.FromFloatingPointBits(0x8000, 2);
                        }
                    }
                    else if (kind ==
                    JSONOptions.ConversionMode.IntOrFloatFromDouble)
                    {
                        return CBORObject.FromObject(0);
                    }
                }
                else
                {
                    // overflow
                    if (kind == JSONOptions.ConversionMode.Double ||
                      kind == JSONOptions.ConversionMode.IntOrFloatFromDouble ||
                      kind == JSONOptions.ConversionMode.IntOrFloat)
                    {
                        return CBORObject.FromFloatingPointBits(
                            negative ? DoubleNegInfinity : DoublePosInfinity,
                            8);
                    }
                    else if (kind == JSONOptions.ConversionMode.Decimal128)
                    {
                        return CBORObject.FromObject(negative ?
                            EDecimal.NegativeInfinity : EDecimal.PositiveInfinity);
                    }
                }
            }
            if (!haveExponent && !haveDecimalPoint &&
              (endPos - numOffset) <= 16)
            {
                // Very common case of all-digit JSON number strings
                // less than 2^53 (with or without number sign)
                long v = 0L;
                int vi = numOffset;
                for (; vi < endPos; ++vi)
                {
                    v = (v * 10) + (int)(chars[vi] - '0');
                }
                if ((v != 0 || !negative) && v < (1L << 53) - 1)
                {
                    if (negative)
                    {
                        v = -v;
                    }
                    if (kind == JSONOptions.ConversionMode.Double)
                    {
                        return CBORObject.FromObject(EFloat.FromInt64(v).ToDoubleBits());
                    }
                    else if (kind == JSONOptions.ConversionMode.Decimal128)
                    {
                        return CBORObject.FromObject(EDecimal.FromInt64(v));
                    }
                    else
                    {
                        return CBORObject.FromObject(v);
                    }
                }
            }
            if (kind == JSONOptions.ConversionMode.Full)
            {
                if (!haveDecimalPoint && !haveExponent)
                {
                    EInteger ei = EInteger.FromSubstring(chars, initialOffset, endPos);
                    if (preserveNegativeZero && ei.IsZero && negative)
                    {
                        // TODO: In next major version, change to EDecimal.NegativeZero
                        return CBORObject.FromFloatingPointBits(0x8000, 2);
                    }
                    return CBORObject.FromObject(ei);
                }
                if (!haveExponent && haveDecimalPoint)
                {
                    // No more than 18 digits plus one decimal point (which
                    // should fit a long)
                    long lv = 0L;
                    int expo = -(endPos - (decimalPointPos + 1));
                    int vi = numOffset;
                    var digitCount = 0;
                    for (; vi < decimalPointPos; ++vi)
                    {
                        if (digitCount < 0 || digitCount >= 18)
                        {
                            digitCount = -1;
                            break;
                        }
                        else if (digitCount > 0 || chars[vi] != '0')
                        {
                            ++digitCount;
                        }
                        lv = checked((lv * 10) + (int)(chars[vi] - '0'));
                    }
                    for (vi = decimalPointPos + 1; vi < endPos; ++vi)
                    {
                        if (digitCount < 0 || digitCount >= 18)
                        {
                            digitCount = -1;
                            break;
                        }
                        else if (digitCount > 0 || chars[vi] != '0')
                        {
                            ++digitCount;
                        }
                        lv = checked((lv * 10) + (int)(chars[vi] - '0'));
                    }
                    if (negative)
                    {
                        lv = -lv;
                    }
                    if (digitCount >= 0 && (!negative || lv != 0))
                    {
                        if (expo == 0)
                        {
                            return CBORObject.FromObject(lv);
                        }
                        else
                        {
                            CBORObject cbor = CBORObject.FromArrayBackedObject(
                            new CBORObject[] {
                CBORObject.FromObject(expo),
                CBORObject.FromObject(lv),
                            });
                            return cbor.WithTag(4);
                        }
                    }
                }
                // DebugUtility.Log("convfull " + chars.Substring(initialOffset, endPos -
                // initialOffset));
                EDecimal ed = EDecimal.FromString(
                    chars,
                    initialOffset,
                    endPos - initialOffset);
                if (ed.IsZero && negative)
                {
                    if (ed.Exponent.IsZero)
                    {
                        // TODO: In next major version, use EDecimal
                        // for preserveNegativeZero
                        return preserveNegativeZero ?
                          CBORObject.FromFloatingPointBits(0x8000, 2) :
                          CBORObject.FromObject(0);
                    }
                    else if (!preserveNegativeZero)
                    {
                        return CBORObject.FromObject(ed.Negate());
                    }
                    else
                    {
                        return CBORObject.FromObject(ed);
                    }
                }
                else
                {
                    return ed.Exponent.IsZero ? CBORObject.FromObject(ed.Mantissa) :
                      CBORObject.FromObject(ed);
                }
            }
            else if (kind == JSONOptions.ConversionMode.Double)
            {
                EFloat ef = EFloat.FromString(
                    chars,
                    initialOffset,
                    endPos - initialOffset,
                    EContext.Binary64);
                long lb = ef.ToDoubleBits();
                if (!preserveNegativeZero && (lb == 1L << 63 || lb == 0L))
                {
                    lb = 0L;
                }
                return CBORObject.FromFloatingPointBits(lb, 8);
            }
            else if (kind == JSONOptions.ConversionMode.Decimal128)
            {
                EDecimal ed = EDecimal.FromString(
                    chars,
                    initialOffset,
                    endPos - initialOffset,
                    EContext.Decimal128);
                if (!preserveNegativeZero && ed.IsNegative && ed.IsZero)
                {
                    ed = ed.Negate();
                }
                return CBORObject.FromObject(ed);
            }
            else if (kind == JSONOptions.ConversionMode.IntOrFloatFromDouble)
            {
                EFloat ef = EFloat.FromString(
                    chars,
                    initialOffset,
                    endPos - initialOffset,
                    EContext.Binary64);
                long lb = ef.ToDoubleBits();
                return (!CBORUtilities.IsBeyondSafeRange(lb) &&
        CBORUtilities.IsIntegerValue(lb)) ?
                  CBORObject.FromObject(CBORUtilities.GetIntegerValue(lb)) :
                  CBORObject.FromFloatingPointBits(lb, 8);
            }
            else if (kind == JSONOptions.ConversionMode.IntOrFloat)
            {
                EContext ctx = EContext.Binary64.WithBlankFlags();
                EFloat ef = EFloat.FromString(
                    chars,
                    initialOffset,
                    endPos - initialOffset,
                    ctx);
                long lb = ef.ToDoubleBits();
                if ((ctx.Flags & EContext.FlagInexact) != 0)
                {
                    // Inexact conversion to double, meaning that the string doesn't
                    // represent an integer in [-(2^53)+1, 2^53), which is representable
                    // exactly as double, so treat as ConversionMode.Double
                    if (!preserveNegativeZero && (lb == 1L << 63 || lb == 0L))
                    {
                        lb = 0L;
                    }
                    return CBORObject.FromFloatingPointBits(lb, 8);
                }
                else
                {
                    // Exact conversion; treat as ConversionMode.IntToFloatFromDouble
                    return (!CBORUtilities.IsBeyondSafeRange(lb) &&
          CBORUtilities.IsIntegerValue(lb)) ?
                      CBORObject.FromObject(CBORUtilities.GetIntegerValue(lb)) :
                      CBORObject.FromFloatingPointBits(lb, 8);
                }
            }
            else
            {
                throw new ArgumentException("Unsupported conversion kind.");
            }
        }
    }
    #endregion

    #region CBORDataUtilitiesCharArrayString
    internal static class CBORDataUtilitiesCharArrayString
    {
        private const long DoubleNegInfinity = unchecked((long)(0xfffL << 52));
        private const long DoublePosInfinity = unchecked((long)(0x7ffL << 52));

        internal static CBORObject ParseJSONNumber(
          char[] chars,
          int offset,
          int count,
          JSONOptions options,
          int[] endOfNumber)
        {
            if (chars == null || chars.Length == 0 || count <= 0)
            {
                return null;
            }
            if (offset < 0 || offset > chars.Length)
            {
                return null;
            }
            if (count > chars.Length || chars.Length - offset < count)
            {
                return null;
            }
            JSONOptions opt = options ?? CBORDataUtilities.DefaultOptions;
            bool preserveNegativeZero = options.PreserveNegativeZero;
            JSONOptions.ConversionMode kind = options.NumberConversion;
            int endPos = offset + count;
            int initialOffset = offset;
            var negative = false;
            if (chars[initialOffset] == '-')
            {
                ++offset;
                negative = true;
            }
            int numOffset = offset;
            var haveDecimalPoint = false;
            var haveDigits = false;
            var haveDigitsAfterDecimal = false;
            var haveExponent = false;
            int i = offset;
            var decimalPointPos = -1;
            // Check syntax
            int k = i;
            if (endPos - 1 > k && chars[k] == '0' && chars[k + 1] >= '0' &&
              chars[k + 1] <= '9')
            {
                if (endOfNumber != null)
                {
                    endOfNumber[0] = k + 2;
                }
                return null;
            }
            for (; k < endPos; ++k)
            {
                char c = chars[k];
                if (c >= '0' && c <= '9')
                {
                    haveDigits = true;
                    haveDigitsAfterDecimal |= haveDecimalPoint;
                }
                else if (c == '.')
                {
                    if (!haveDigits || haveDecimalPoint)
                    {
                        // no digits before the decimal point,
                        // or decimal point already seen
                        if (endOfNumber != null)
                        {
                            endOfNumber[0] = k;
                        }
                        return null;
                    }
                    haveDecimalPoint = true;
                    decimalPointPos = k;
                }
                else if (c == 'E' || c == 'e')
                {
                    ++k;
                    haveExponent = true;
                    break;
                }
                else
                {
                    if (endOfNumber != null)
                    {
                        endOfNumber[0] = k;
                        // Check if character can validly appear after a JSON number
                        if (c != ',' && c != ']' && c != '}' &&
                          c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09)
                        {
                            return null;
                        }
                        else
                        {
                            endPos = k;
                            break;
                        }
                    }
                    return null;
                }
            }
            if (!haveDigits || (haveDecimalPoint && !haveDigitsAfterDecimal))
            {
                if (endOfNumber != null)
                {
                    endOfNumber[0] = k;
                }
                return null;
            }
            var exponentPos = -1;
            var negativeExp = false;
            if (haveExponent)
            {
                haveDigits = false;
                if (k == endPos)
                {
                    if (endOfNumber != null)
                    {
                        endOfNumber[0] = k;
                    }
                    return null;
                }
                char c = chars[k];
                if (c == '+')
                {
                    ++k;
                }
                else if (c == '-')
                {
                    negativeExp = true;
                    ++k;
                }
                for (; k < endPos; ++k)
                {
                    c = chars[k];
                    if (c >= '0' && c <= '9')
                    {
                        if (exponentPos < 0 && c != '0')
                        {
                            exponentPos = k;
                        }
                        haveDigits = true;
                    }
                    else if (endOfNumber != null)
                    {
                        endOfNumber[0] = k;
                        // Check if character can validly appear after a JSON number
                        if (c != ',' && c != ']' && c != '}' &&
                          c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09)
                        {
                            return null;
                        }
                        else
                        {
                            endPos = k;
                            break;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                if (!haveDigits)
                {
                    if (endOfNumber != null)
                    {
                        endOfNumber[0] = k;
                    }
                    return null;
                }
            }
            if (endOfNumber != null)
            {
                endOfNumber[0] = endPos;
            }
            if (exponentPos >= 0 && endPos - exponentPos > 20)
            {
                // Exponent too high for precision to overcome (which
                // has a length no bigger than Int32.MaxValue, which is 10 digits
                // long)
                if (negativeExp)
                {
                    // underflow
                    if (kind == JSONOptions.ConversionMode.Double ||
                      kind == JSONOptions.ConversionMode.IntOrFloat)
                    {
                        if (!negative)
                        {
                            return CBORObject.FromFloatingPointBits(0, 2);
                        }
                        else
                        {
                            return CBORObject.FromFloatingPointBits(0x8000, 2);
                        }
                    }
                    else if (kind ==
                    JSONOptions.ConversionMode.IntOrFloatFromDouble)
                    {
                        return CBORObject.FromObject(0);
                    }
                }
                else
                {
                    // overflow
                    if (kind == JSONOptions.ConversionMode.Double ||
                      kind == JSONOptions.ConversionMode.IntOrFloatFromDouble ||
                      kind == JSONOptions.ConversionMode.IntOrFloat)
                    {
                        return CBORObject.FromFloatingPointBits(
                            negative ? DoubleNegInfinity : DoublePosInfinity,
                            8);
                    }
                    else if (kind == JSONOptions.ConversionMode.Decimal128)
                    {
                        return CBORObject.FromObject(negative ?
                            EDecimal.NegativeInfinity : EDecimal.PositiveInfinity);
                    }
                }
            }
            if (!haveExponent && !haveDecimalPoint &&
              (endPos - numOffset) <= 16)
            {
                // Very common case of all-digit JSON number strings
                // less than 2^53 (with or without number sign)
                long v = 0L;
                int vi = numOffset;
                for (; vi < endPos; ++vi)
                {
                    v = (v * 10) + (int)(chars[vi] - '0');
                }
                if ((v != 0 || !negative) && v < (1L << 53) - 1)
                {
                    if (negative)
                    {
                        v = -v;
                    }
                    if (kind == JSONOptions.ConversionMode.Double)
                    {
                        return CBORObject.FromObject(EFloat.FromInt64(v).ToDoubleBits());
                    }
                    else if (kind == JSONOptions.ConversionMode.Decimal128)
                    {
                        return CBORObject.FromObject(EDecimal.FromInt64(v));
                    }
                    else
                    {
                        return CBORObject.FromObject(v);
                    }
                }
            }
            if (kind == JSONOptions.ConversionMode.Full)
            {
                if (!haveDecimalPoint && !haveExponent)
                {
                    EInteger ei = EInteger.FromSubstring(chars, initialOffset, endPos);
                    if (preserveNegativeZero && ei.IsZero && negative)
                    {
                        // TODO: In next major version, change to EDecimal.NegativeZero
                        return CBORObject.FromFloatingPointBits(0x8000, 2);
                    }
                    return CBORObject.FromObject(ei);
                }
                if (!haveExponent && haveDecimalPoint)
                {
                    // No more than 18 digits plus one decimal point (which
                    // should fit a long)
                    long lv = 0L;
                    int expo = -(endPos - (decimalPointPos + 1));
                    int vi = numOffset;
                    var digitCount = 0;
                    for (; vi < decimalPointPos; ++vi)
                    {
                        if (digitCount < 0 || digitCount >= 18)
                        {
                            digitCount = -1;
                            break;
                        }
                        else if (digitCount > 0 || chars[vi] != '0')
                        {
                            ++digitCount;
                        }
                        lv = checked((lv * 10) + (int)(chars[vi] - '0'));
                    }
                    for (vi = decimalPointPos + 1; vi < endPos; ++vi)
                    {
                        if (digitCount < 0 || digitCount >= 18)
                        {
                            digitCount = -1;
                            break;
                        }
                        else if (digitCount > 0 || chars[vi] != '0')
                        {
                            ++digitCount;
                        }
                        lv = checked((lv * 10) + (int)(chars[vi] - '0'));
                    }
                    if (negative)
                    {
                        lv = -lv;
                    }
                    if (digitCount >= 0 && (!negative || lv != 0))
                    {
                        if (expo == 0)
                        {
                            return CBORObject.FromObject(lv);
                        }
                        else
                        {
                            CBORObject cbor = CBORObject.FromArrayBackedObject(
                            new CBORObject[] {
                CBORObject.FromObject(expo),
                CBORObject.FromObject(lv),
                            });
                            return cbor.WithTag(4);
                        }
                    }
                }
                // DebugUtility.Log("convfull " + chars.Substring(initialOffset, endPos -
                // initialOffset));
                EDecimal ed = EDecimal.FromString(
                    chars,
                    initialOffset,
                    endPos - initialOffset);
                if (ed.IsZero && negative)
                {
                    if (ed.Exponent.IsZero)
                    {
                        // TODO: In next major version, use EDecimal
                        // for preserveNegativeZero
                        return preserveNegativeZero ?
                          CBORObject.FromFloatingPointBits(0x8000, 2) :
                          CBORObject.FromObject(0);
                    }
                    else if (!preserveNegativeZero)
                    {
                        return CBORObject.FromObject(ed.Negate());
                    }
                    else
                    {
                        return CBORObject.FromObject(ed);
                    }
                }
                else
                {
                    return ed.Exponent.IsZero ? CBORObject.FromObject(ed.Mantissa) :
                      CBORObject.FromObject(ed);
                }
            }
            else if (kind == JSONOptions.ConversionMode.Double)
            {
                EFloat ef = EFloat.FromString(
                    chars,
                    initialOffset,
                    endPos - initialOffset,
                    EContext.Binary64);
                long lb = ef.ToDoubleBits();
                if (!preserveNegativeZero && (lb == 1L << 63 || lb == 0L))
                {
                    lb = 0L;
                }
                return CBORObject.FromFloatingPointBits(lb, 8);
            }
            else if (kind == JSONOptions.ConversionMode.Decimal128)
            {
                EDecimal ed = EDecimal.FromString(
                    chars,
                    initialOffset,
                    endPos - initialOffset,
                    EContext.Decimal128);
                if (!preserveNegativeZero && ed.IsNegative && ed.IsZero)
                {
                    ed = ed.Negate();
                }
                return CBORObject.FromObject(ed);
            }
            else if (kind == JSONOptions.ConversionMode.IntOrFloatFromDouble)
            {
                EFloat ef = EFloat.FromString(
                    chars,
                    initialOffset,
                    endPos - initialOffset,
                    EContext.Binary64);
                long lb = ef.ToDoubleBits();
                return (!CBORUtilities.IsBeyondSafeRange(lb) &&
        CBORUtilities.IsIntegerValue(lb)) ?
                  CBORObject.FromObject(CBORUtilities.GetIntegerValue(lb)) :
                  CBORObject.FromFloatingPointBits(lb, 8);
            }
            else if (kind == JSONOptions.ConversionMode.IntOrFloat)
            {
                EContext ctx = EContext.Binary64.WithBlankFlags();
                EFloat ef = EFloat.FromString(
                    chars,
                    initialOffset,
                    endPos - initialOffset,
                    ctx);
                long lb = ef.ToDoubleBits();
                if ((ctx.Flags & EContext.FlagInexact) != 0)
                {
                    // Inexact conversion to double, meaning that the string doesn't
                    // represent an integer in [-(2^53)+1, 2^53), which is representable
                    // exactly as double, so treat as ConversionMode.Double
                    if (!preserveNegativeZero && (lb == 1L << 63 || lb == 0L))
                    {
                        lb = 0L;
                    }
                    return CBORObject.FromFloatingPointBits(lb, 8);
                }
                else
                {
                    // Exact conversion; treat as ConversionMode.IntToFloatFromDouble
                    return (!CBORUtilities.IsBeyondSafeRange(lb) &&
          CBORUtilities.IsIntegerValue(lb)) ?
                      CBORObject.FromObject(CBORUtilities.GetIntegerValue(lb)) :
                      CBORObject.FromFloatingPointBits(lb, 8);
                }
            }
            else
            {
                throw new ArgumentException("Unsupported conversion kind.");
            }
        }
    }
    #endregion

    #region CBORDataUtilitiesTextString
    internal static class CBORDataUtilitiesTextString
    {
        private const long DoubleNegInfinity = unchecked((long)(0xfffL << 52));
        private const long DoublePosInfinity = unchecked((long)(0x7ffL << 52));

        internal static CBORObject ParseJSONNumber(
          string chars,
          int offset,
          int count,
          JSONOptions options,
          int[] endOfNumber)
        {
            if (chars == null || chars.Length == 0 || count <= 0)
            {
                return null;
            }
            if (offset < 0 || offset > chars.Length)
            {
                return null;
            }
            if (count > chars.Length || chars.Length - offset < count)
            {
                return null;
            }
            JSONOptions opt = options ?? CBORDataUtilities.DefaultOptions;
            bool preserveNegativeZero = options.PreserveNegativeZero;
            JSONOptions.ConversionMode kind = options.NumberConversion;
            int endPos = offset + count;
            int initialOffset = offset;
            var negative = false;
            if (chars[initialOffset] == '-')
            {
                ++offset;
                negative = true;
            }
            int numOffset = offset;
            var haveDecimalPoint = false;
            var haveDigits = false;
            var haveDigitsAfterDecimal = false;
            var haveExponent = false;
            int i = offset;
            var decimalPointPos = -1;
            // Check syntax
            int k = i;
            if (endPos - 1 > k && chars[k] == '0' && chars[k + 1] >= '0' &&
              chars[k + 1] <= '9')
            {
                if (endOfNumber != null)
                {
                    endOfNumber[0] = k + 2;
                }
                return null;
            }
            for (; k < endPos; ++k)
            {
                char c = chars[k];
                if (c >= '0' && c <= '9')
                {
                    haveDigits = true;
                    haveDigitsAfterDecimal |= haveDecimalPoint;
                }
                else if (c == '.')
                {
                    if (!haveDigits || haveDecimalPoint)
                    {
                        // no digits before the decimal point,
                        // or decimal point already seen
                        if (endOfNumber != null)
                        {
                            endOfNumber[0] = k;
                        }
                        return null;
                    }
                    haveDecimalPoint = true;
                    decimalPointPos = k;
                }
                else if (c == 'E' || c == 'e')
                {
                    ++k;
                    haveExponent = true;
                    break;
                }
                else
                {
                    if (endOfNumber != null)
                    {
                        endOfNumber[0] = k;
                        // Check if character can validly appear after a JSON number
                        if (c != ',' && c != ']' && c != '}' &&
                          c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09)
                        {
                            return null;
                        }
                        else
                        {
                            endPos = k;
                            break;
                        }
                    }
                    return null;
                }
            }
            if (!haveDigits || (haveDecimalPoint && !haveDigitsAfterDecimal))
            {
                if (endOfNumber != null)
                {
                    endOfNumber[0] = k;
                }
                return null;
            }
            var exponentPos = -1;
            var negativeExp = false;
            if (haveExponent)
            {
                haveDigits = false;
                if (k == endPos)
                {
                    if (endOfNumber != null)
                    {
                        endOfNumber[0] = k;
                    }
                    return null;
                }
                char c = chars[k];
                if (c == '+')
                {
                    ++k;
                }
                else if (c == '-')
                {
                    negativeExp = true;
                    ++k;
                }
                for (; k < endPos; ++k)
                {
                    c = chars[k];
                    if (c >= '0' && c <= '9')
                    {
                        if (exponentPos < 0 && c != '0')
                        {
                            exponentPos = k;
                        }
                        haveDigits = true;
                    }
                    else if (endOfNumber != null)
                    {
                        endOfNumber[0] = k;
                        // Check if character can validly appear after a JSON number
                        if (c != ',' && c != ']' && c != '}' &&
                          c != 0x20 && c != 0x0a && c != 0x0d && c != 0x09)
                        {
                            return null;
                        }
                        else
                        {
                            endPos = k;
                            break;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                if (!haveDigits)
                {
                    if (endOfNumber != null)
                    {
                        endOfNumber[0] = k;
                    }
                    return null;
                }
            }
            if (endOfNumber != null)
            {
                endOfNumber[0] = endPos;
            }
            if (exponentPos >= 0 && endPos - exponentPos > 20)
            {
                // Exponent too high for precision to overcome (which
                // has a length no bigger than Int32.MaxValue, which is 10 digits
                // long)
                if (negativeExp)
                {
                    // underflow
                    if (kind == JSONOptions.ConversionMode.Double ||
                      kind == JSONOptions.ConversionMode.IntOrFloat)
                    {
                        if (!negative)
                        {
                            return CBORObject.FromFloatingPointBits(0, 2);
                        }
                        else
                        {
                            return CBORObject.FromFloatingPointBits(0x8000, 2);
                        }
                    }
                    else if (kind ==
                    JSONOptions.ConversionMode.IntOrFloatFromDouble)
                    {
                        return CBORObject.FromObject(0);
                    }
                }
                else
                {
                    // overflow
                    if (kind == JSONOptions.ConversionMode.Double ||
                      kind == JSONOptions.ConversionMode.IntOrFloatFromDouble ||
                      kind == JSONOptions.ConversionMode.IntOrFloat)
                    {
                        return CBORObject.FromFloatingPointBits(
                            negative ? DoubleNegInfinity : DoublePosInfinity,
                            8);
                    }
                    else if (kind == JSONOptions.ConversionMode.Decimal128)
                    {
                        return CBORObject.FromObject(negative ?
                            EDecimal.NegativeInfinity : EDecimal.PositiveInfinity);
                    }
                }
            }
            if (!haveExponent && !haveDecimalPoint &&
              (endPos - numOffset) <= 16)
            {
                // Very common case of all-digit JSON number strings
                // less than 2^53 (with or without number sign)
                long v = 0L;
                int vi = numOffset;
                for (; vi < endPos; ++vi)
                {
                    v = (v * 10) + (int)(chars[vi] - '0');
                }
                if ((v != 0 || !negative) && v < (1L << 53) - 1)
                {
                    if (negative)
                    {
                        v = -v;
                    }
                    if (kind == JSONOptions.ConversionMode.Double)
                    {
                        return CBORObject.FromObject(EFloat.FromInt64(v).ToDoubleBits());
                    }
                    else if (kind == JSONOptions.ConversionMode.Decimal128)
                    {
                        return CBORObject.FromObject(EDecimal.FromInt64(v));
                    }
                    else
                    {
                        return CBORObject.FromObject(v);
                    }
                }
            }
            if (kind == JSONOptions.ConversionMode.Full)
            {
                if (!haveDecimalPoint && !haveExponent)
                {
                    EInteger ei = EInteger.FromSubstring(chars, initialOffset, endPos);
                    if (preserveNegativeZero && ei.IsZero && negative)
                    {
                        // TODO: In next major version, change to EDecimal.NegativeZero
                        return CBORObject.FromFloatingPointBits(0x8000, 2);
                    }
                    return CBORObject.FromObject(ei);
                }
                if (!haveExponent && haveDecimalPoint)
                {
                    // No more than 18 digits plus one decimal point (which
                    // should fit a long)
                    long lv = 0L;
                    int expo = -(endPos - (decimalPointPos + 1));
                    int vi = numOffset;
                    var digitCount = 0;
                    for (; vi < decimalPointPos; ++vi)
                    {
                        if (digitCount < 0 || digitCount >= 18)
                        {
                            digitCount = -1;
                            break;
                        }
                        else if (digitCount > 0 || chars[vi] != '0')
                        {
                            ++digitCount;
                        }
                        lv = checked((lv * 10) + (int)(chars[vi] - '0'));
                    }
                    for (vi = decimalPointPos + 1; vi < endPos; ++vi)
                    {
                        if (digitCount < 0 || digitCount >= 18)
                        {
                            digitCount = -1;
                            break;
                        }
                        else if (digitCount > 0 || chars[vi] != '0')
                        {
                            ++digitCount;
                        }
                        lv = checked((lv * 10) + (int)(chars[vi] - '0'));
                    }
                    if (negative)
                    {
                        lv = -lv;
                    }
                    if (digitCount >= 0 && (!negative || lv != 0))
                    {
                        if (expo == 0)
                        {
                            return CBORObject.FromObject(lv);
                        }
                        else
                        {
                            CBORObject cbor = CBORObject.FromArrayBackedObject(
                            new CBORObject[] {
                CBORObject.FromObject(expo),
                CBORObject.FromObject(lv),
                            });
                            return cbor.WithTag(4);
                        }
                    }
                }
                // DebugUtility.Log("convfull " + chars.Substring(initialOffset, endPos -
                // initialOffset));
                EDecimal ed = EDecimal.FromString(
                    chars,
                    initialOffset,
                    endPos - initialOffset);
                if (ed.IsZero && negative)
                {
                    if (ed.Exponent.IsZero)
                    {
                        // TODO: In next major version, use EDecimal
                        // for preserveNegativeZero
                        return preserveNegativeZero ?
                          CBORObject.FromFloatingPointBits(0x8000, 2) :
                          CBORObject.FromObject(0);
                    }
                    else if (!preserveNegativeZero)
                    {
                        return CBORObject.FromObject(ed.Negate());
                    }
                    else
                    {
                        return CBORObject.FromObject(ed);
                    }
                }
                else
                {
                    return ed.Exponent.IsZero ? CBORObject.FromObject(ed.Mantissa) :
                      CBORObject.FromObject(ed);
                }
            }
            else if (kind == JSONOptions.ConversionMode.Double)
            {
                EFloat ef = EFloat.FromString(
                    chars,
                    initialOffset,
                    endPos - initialOffset,
                    EContext.Binary64);
                long lb = ef.ToDoubleBits();
                if (!preserveNegativeZero && (lb == 1L << 63 || lb == 0L))
                {
                    lb = 0L;
                }
                return CBORObject.FromFloatingPointBits(lb, 8);
            }
            else if (kind == JSONOptions.ConversionMode.Decimal128)
            {
                EDecimal ed = EDecimal.FromString(
                    chars,
                    initialOffset,
                    endPos - initialOffset,
                    EContext.Decimal128);
                if (!preserveNegativeZero && ed.IsNegative && ed.IsZero)
                {
                    ed = ed.Negate();
                }
                return CBORObject.FromObject(ed);
            }
            else if (kind == JSONOptions.ConversionMode.IntOrFloatFromDouble)
            {
                EFloat ef = EFloat.FromString(
                    chars,
                    initialOffset,
                    endPos - initialOffset,
                    EContext.Binary64);
                long lb = ef.ToDoubleBits();
                return (!CBORUtilities.IsBeyondSafeRange(lb) &&
        CBORUtilities.IsIntegerValue(lb)) ?
                  CBORObject.FromObject(CBORUtilities.GetIntegerValue(lb)) :
                  CBORObject.FromFloatingPointBits(lb, 8);
            }
            else if (kind == JSONOptions.ConversionMode.IntOrFloat)
            {
                EContext ctx = EContext.Binary64.WithBlankFlags();
                EFloat ef = EFloat.FromString(
                    chars,
                    initialOffset,
                    endPos - initialOffset,
                    ctx);
                long lb = ef.ToDoubleBits();
                if ((ctx.Flags & EContext.FlagInexact) != 0)
                {
                    // Inexact conversion to double, meaning that the string doesn't
                    // represent an integer in [-(2^53)+1, 2^53), which is representable
                    // exactly as double, so treat as ConversionMode.Double
                    if (!preserveNegativeZero && (lb == 1L << 63 || lb == 0L))
                    {
                        lb = 0L;
                    }
                    return CBORObject.FromFloatingPointBits(lb, 8);
                }
                else
                {
                    // Exact conversion; treat as ConversionMode.IntToFloatFromDouble
                    return (!CBORUtilities.IsBeyondSafeRange(lb) &&
          CBORUtilities.IsIntegerValue(lb)) ?
                      CBORObject.FromObject(CBORUtilities.GetIntegerValue(lb)) :
                      CBORObject.FromFloatingPointBits(lb, 8);
                }
            }
            else
            {
                throw new ArgumentException("Unsupported conversion kind.");
            }
        }
    }
    #endregion

    #region CBORUuidConverter
    internal class CBORUuidConverter : ICBORToFromConverter<Guid>
    {
        private static CBORObject ValidateObject(CBORObject obj)
        {
            if (obj.Type != CBORType.ByteString)
            {
                throw new CBORException("UUID must be a byte string");
            }
            byte[] bytes = obj.GetByteString();
            if (bytes.Length != 16)
            {
                throw new CBORException("UUID must be 16 bytes long");
            }
            return obj;
        }

        /// <summary>Internal API.</summary>
        /// <param name='obj'>The parameter <paramref name='obj'/> is an
        /// internal parameter.</param>
        /// <returns>A CBORObject object.</returns>
        public CBORObject ToCBORObject(Guid obj)
        {
            byte[] bytes = PropertyMap.UUIDToBytes(obj);
            return CBORObject.FromObjectAndTag(bytes, (int)37);
        }

        public Guid FromCBORObject(CBORObject obj)
        {
            if (!obj.HasMostOuterTag(37))
            {
                throw new CBORException("Must have outermost tag 37");
            }
            ValidateObject(obj);
            byte[] bytes = obj.GetByteString();
            var guidChars = new char[36];
            string hex = "0123456789abcdef";
            var index = 0;
            for (var i = 0; i < 16; ++i)
            {
                if (i == 4 || i == 6 || i == 8 || i == 10)
                {
                    guidChars[index++] = '-';
                }
                guidChars[index++] = hex[(int)(bytes[i] >> 4) & 15];
                guidChars[index++] = hex[(int)bytes[i] & 15];
            }
            string guidString = new String(guidChars);
            return new Guid(guidString);
        }
    }
    #endregion

    #region URIUtility
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:URIUtility"]/*'/>
    /// <summary>Contains utility methods for processing Uniform Resource
    /// Identifiers (URIs) and Internationalized Resource Identifiers
    /// (IRIs) under RFC3986 and RFC3987, respectively. In the following
    /// documentation, URIs and IRIs include URI references and IRI
    /// references, for convenience.
    /// <para>There are five components to a URI: scheme, authority, path,
    /// query, and fragment identifier. The generic syntax to these
    /// components is defined in RFC3986 and extended in RFC3987. According
    /// to RFC3986, different URI schemes can further restrict the syntax
    /// of the authority, path, and query component (see also RFC 7320).
    /// However, the syntax of fragment identifiers depends on the media
    /// type (also known as MIME type) of the resource a URI references
    /// (see also RFC 3986 and RFC 7320). As of September 3, 2019, only the
    /// following media types specify a syntax for fragment
    /// identifiers:</para>
    /// <list>
    /// <item>The following application/* media types: epub + zip, pdf,
    /// senml + cbor, senml + json, senml-exi, sensml + cbor, sensml +
    /// json, sensml-exi, smil, vnd.3gpp-v2x-local-service-information,
    /// vnd.3gpp.mcdata-signalling, vnd.collection.doc + json, vnd.hc +
    /// json, vnd.hyper + json, vnd.hyper-item + json, vnd.mason + json,
    /// vnd.microsoft.portable-executable, vnd.oma.bcast.sgdu,
    /// vnd.shootproof + json</item>
    /// <item>The following image/* media types: avci, avcs, heic,
    /// heic-sequence, heif, heif-sequence, hej2k, hsj2, jxra, jxrs, jxsi,
    /// jxss</item>
    /// <item>The XML media types: application/xml,
    /// application/xml-external-parsed-entity, text/xml,
    /// text/xml-external-parsed-entity, application/xml-dtd</item>
    /// <item>All media types with subtypes ending in "+xml" (see RFC 7303)
    /// use XPointer Framework syntax as fragment identifiers, except the
    /// following application/* media types: dicom + xml (syntax not
    /// defined), senml + xml (own syntax), sensml + xml (own syntax), ttml
    /// + xml (own syntax), xliff + xml (own syntax), yang-data + xml
    /// (syntax not defined)</item>
    /// <item>font/collection</item>
    /// <item>multipart/x-mixed-replace</item>
    /// <item>text/plain</item>
    /// <item>text/csv</item>
    /// <item>text/html</item>
    /// <item>text/markdown</item>
    /// <item>text/vnd.a</item></list></summary>
    public static class URIUtility
    {
        /// <summary>Specifies whether certain characters are allowed when
        /// parsing IRIs and URIs.</summary>
        public enum ParseMode
        {
            /// <summary>The rules follow the syntax for parsing IRIs. In
            /// particular, many code points outside the Basic Latin range (U+0000
            /// to U+007F) are allowed. Strings with unpaired surrogate code points
            /// are considered invalid.</summary>
            IRIStrict,

            /// <summary>The rules follow the syntax for parsing IRIs, except that
            /// code points outside the Basic Latin range (U+0000 to U+007F) are
            /// not allowed.</summary>
            URIStrict,

            /// <summary>The rules only check for the appropriate delimiters when
            /// splitting the path, without checking if all the characters in each
            /// component are valid. Even with this mode, strings with unpaired
            /// surrogate code points are considered invalid.</summary>
            IRILenient,

            /// <summary>The rules only check for the appropriate delimiters when
            /// splitting the path, without checking if all the characters in each
            /// component are valid. Code points outside the Basic Latin range
            /// (U+0000 to U+007F) are not allowed.</summary>
            URILenient,

            /// <summary>The rules only check for the appropriate delimiters when
            /// splitting the path, without checking if all the characters in each
            /// component are valid. Unpaired surrogate code points are treated as
            /// though they were replacement characters instead for the purposes of
            /// these rules, so that strings with those code points are not
            /// considered invalid strings.</summary>
            IRISurrogateLenient,
        }

        private const string HexChars = "0123456789ABCDEF";

        private static void AppendAuthority(
          StringBuilder builder,
          string refValue,
          int[] segments)
        {
            if (segments[2] >= 0)
            {
                builder.Append("//");
                builder.Append(
                  refValue.Substring(
                    segments[2],
                    segments[3] - segments[2]));
            }
        }

        private static void AppendFragment(
          StringBuilder builder,
          string refValue,
          int[] segments)
        {
            if (segments[8] >= 0)
            {
                builder.Append('#');
                builder.Append(
                  refValue.Substring(
                    segments[8],
                    segments[9] - segments[8]));
            }
        }

        private static void AppendNormalizedPath(
          StringBuilder builder,
          string refValue,
          int[] segments)
        {
            builder.Append(
              NormalizePath(
                refValue.Substring(
                  segments[4],
                  segments[5] - segments[4])));
        }

        private static void AppendPath(
          StringBuilder builder,
          string refValue,
          int[] segments)
        {
            builder.Append(
              refValue.Substring(
                segments[4],
                segments[5] - segments[4]));
        }

        private static void AppendQuery(
          StringBuilder builder,
          string refValue,
          int[] segments)
        {
            if (segments[6] >= 0)
            {
                builder.Append('?');
                builder.Append(
                  refValue.Substring(
                    segments[6],
                    segments[7] - segments[6]));
            }
        }

        private static void AppendScheme(
          StringBuilder builder,
          string refValue,
          int[] segments)
        {
            if (segments[0] >= 0)
            {
                builder.Append(
                  refValue.Substring(
                    segments[0],
                    segments[1] - segments[0]));
                builder.Append(':');
            }
        }

        /// <summary>Checks a text string representing a URI or IRI and escapes
        /// characters it has that can't appear in URIs or IRIs. The function
        /// is idempotent; that is, calling the function again on the result
        /// with the same mode doesn't change the result.</summary>
        /// <param name='s'>A text string representing a URI or IRI. Can be
        /// null.</param>
        /// <param name='mode'>Has the following meaning: 0 = Encode reserved
        /// code points, code points below U+0021, code points above U+007E,
        /// and square brackets within the authority component, and do the
        /// IRISurrogateLenient check. 1 = Encode code points above U+007E, and
        /// square brackets within the authority component, and do the
        /// IRIStrict check. 2 = Same as 1, except the check is
        /// IRISurrogateLenient. 3 = Same as 0, except that percent characters
        /// that begin illegal percent-encoding are also encoded.</param>
        /// <returns>A form of the URI or IRI that possibly contains escaped
        /// characters, or null if s is null.</returns>
        public static string EscapeURI(string s, int mode)
        {
            if (s == null)
            {
                return null;
            }
            int[] components = null;
            if (mode == 1)
            {
                components = (
                    s == null) ? null : SplitIRI(
                    s,
                    0,
                    s.Length,
                    URIUtility.ParseMode.IRIStrict);
                if (components == null)
                {
                    return null;
                }
            }
            else
            {
                components = (s == null) ? null : SplitIRI(
                  s,
                  0,
                  s.Length,
                  URIUtility.ParseMode.IRISurrogateLenient);
            }
            var index = 0;
            int valueSLength = s.Length;
            var builder = new StringBuilder();
            while (index < valueSLength)
            {
                int c = s[index];
                if ((c & 0xfc00) == 0xd800 && index + 1 < valueSLength &&
                  (s[index + 1] & 0xfc00) == 0xdc00)
                {
                    // Get the Unicode code point for the surrogate pair
                    c = 0x10000 + ((c & 0x3ff) << 10) + (s[index + 1] & 0x3ff);
                    ++index;
                }
                else if ((c & 0xf800) == 0xd800)
                {
                    c = 0xfffd;
                }
                if (mode == 0 || mode == 3)
                {
                    if (c == '%' && mode == 3)
                    {
                        // Check for illegal percent encoding
                        if (index + 2 >= valueSLength || !IsHexChar(s[index + 1]) ||
                          !IsHexChar(s[index + 2]))
                        {
                            PercentEncodeUtf8(builder, c);
                        }
                        else
                        {
                            if (c <= 0xffff)
                            {
                                builder.Append((char)c);
                            }
                            else if (c <= 0x10ffff)
                            {
                                builder.Append((char)((((c - 0x10000) >> 10) & 0x3ff) |
                                    0xd800));
                                builder.Append((char)(((c - 0x10000) & 0x3ff) | 0xdc00));
                            }
                        }
                        ++index;
                        continue;
                    }
                    if (c >= 0x7f || c <= 0x20 || ((c & 0x7f) == c &&
          "{}|^\\`<>\"".IndexOf((char)c) >= 0))
                    {
                        PercentEncodeUtf8(builder, c);
                    }
                    else if (c == '[' || c == ']')
                    {
                        if (components != null && index >= components[2] && index <
                          components[3])
                        {
                            // within the authority component, so don't percent-encode
                            builder.Append((char)c);
                        }
                        else
                        {
                            // percent encode
                            PercentEncodeUtf8(builder, c);
                        }
                    }
                    else
                    {
                        if (c <= 0xffff)
                        {
                            builder.Append((char)c);
                        }
                        else if (c <= 0x10ffff)
                        {
                            builder.Append((char)((((c - 0x10000) >> 10) & 0x3ff) |
              0xd800));
                            builder.Append((char)(((c - 0x10000) & 0x3ff) | 0xdc00));
                        }
                    }
                }
                else if (mode == 1 || mode == 2)
                {
                    if (c >= 0x80)
                    {
                        PercentEncodeUtf8(builder, c);
                    }
                    else if (c == '[' || c == ']')
                    {
                        if (components != null && index >= components[2] && index <
                          components[3])
                        {
                            // within the authority component, so don't percent-encode
                            builder.Append((char)c);
                        }
                        else
                        {
                            // percent encode
                            PercentEncodeUtf8(builder, c);
                        }
                    }
                    else
                    {
                        if (c <= 0xffff)
                        {
                            builder.Append((char)c);
                        }
                        else if (c <= 0x10ffff)
                        {
                            builder.Append((char)((((c - 0x10000) >> 10) & 0x3ff) |
              0xd800));
                            builder.Append((char)(((c - 0x10000) & 0x3ff) | 0xdc00));
                        }
                    }
                }
                ++index;
            }
            return builder.ToString();
        }

        /// <summary>
        ///  Determines whether the string is a valid IRI with a
        /// scheme component. This can be used to check for
        /// relative IRI references.
        /// <para>The following cases return true:</para>
        /// <code>xx-x:mm example:/ww</code>
        ///  The following cases return false:
        /// <code>x@y:/z /x/y/z example.xyz</code>
        ///  .
        /// </summary>
        /// <param name='refValue'>A string representing an IRI to
        /// check.</param>
        /// <returns><c>true</c>
        ///  if the string is a valid IRI with a scheme
        /// component; otherwise, <c>false</c>
        /// .</returns>
        public static bool HasScheme(string refValue)
        {
            int[] segments = (refValue == null) ? null : SplitIRI(
              refValue,
              0,
              refValue.Length,
              URIUtility.ParseMode.IRIStrict);
            return segments != null && segments[0] >= 0;
        }

        /// <summary>
        ///  Determines whether the string is a valid URI with a
        /// scheme component. This can be used to check for
        /// relative URI references. The following cases return
        /// true:
        /// <code>http://example/z xx-x:mm example:/ww</code>
        ///  The following cases return false:
        /// <code>x@y:/z /x/y/z example.xyz</code>
        ///  .
        /// </summary>
        /// <param name='refValue'>A string representing an IRI to
        /// check.</param>
        /// <returns><c>true</c>
        ///  if the string is a valid URI with a scheme
        /// component; otherwise, <c>false</c>
        /// .</returns>
        public static bool HasSchemeForURI(string refValue)
        {
            int[] segments = (refValue == null) ? null : SplitIRI(
              refValue,
              0,
              refValue.Length,
              URIUtility.ParseMode.URIStrict);
            return segments != null && segments[0] >= 0;
        }

        private static bool IsHexChar(char c)
        {
            return (c >= 'a' && c <= 'f') ||
              (c >= 'A' && c <= 'F') || (c >= '0' && c <= '9');
        }

        private static int ToHex(char b1)
        {
            if (b1 >= '0' && b1 <= '9')
            {
                return b1 - '0';
            }
            else if (b1 >= 'A' && b1 <= 'F')
            {
                return b1 + 10 - 'A';
            }
            else
            {
                return (b1 >= 'a' && b1 <= 'f') ? (b1 + 10 - 'a') : 1;
            }
        }

        /// <summary>Decodes percent-encoding (of the form "%XX" where X is a
        /// hexadecimal digit) in the given string. Successive percent-encoded
        /// bytes are assumed to form characters in UTF-8.</summary>
        /// <param name='str'>A string that may contain percent encoding. May
        /// be null.</param>
        /// <returns>The string in which percent-encoding was
        /// decoded.</returns>
        public static string PercentDecode(string str)
        {
            return (str == null) ? null : PercentDecode(str, 0, str.Length);
        }

        /// <summary>Decodes percent-encoding (of the form "%XX" where X is a
        /// hexadecimal digit) in the given portion of a string. Successive
        /// percent-encoded bytes are assumed to form characters in
        /// UTF-8.</summary>
        /// <param name='str'>A string a portion of which may contain percent
        /// encoding. May be null.</param>
        /// <param name='index'>Index starting at 0 showing where the desired
        /// portion of <paramref name='str'/> begins.</param>
        /// <param name='endIndex'>Index starting at 0 showing where the
        /// desired portion of <paramref name='str'/> ends. The character
        /// before this index is the last character.</param>
        /// <returns>The portion of the given string in which percent-encoding
        /// was decoded. Returns null if <paramref name='str'/> is
        /// ull.</returns>
        public static string PercentDecode(string str, int index, int endIndex)
        {
            if (str == null)
            {
                return null;
            }
            // Quick check
            var quickCheck = true;
            var lastIndex = 0;
            int i = index;
            for (; i < endIndex; ++i)
            {
                if (str[i] >= 0xd800 || str[i] == '%')
                {
                    quickCheck = false;
                    lastIndex = i;
                    break;
                }
            }
            if (quickCheck)
            {
                return str.Substring(index, endIndex - index);
            }
            var retString = new StringBuilder();
            retString.Append(str, index, lastIndex);
            var cp = 0;
            var bytesSeen = 0;
            var bytesNeeded = 0;
            var lower = 0x80;
            var upper = 0xbf;
            var markedPos = -1;
            for (i = lastIndex; i < endIndex; ++i)
            {
                int c = str[i];
                if ((c & 0xfc00) == 0xd800 && i + 1 < endIndex &&
                  (str[i + 1] & 0xfc00) == 0xdc00)
                {
                    // Get the Unicode code point for the surrogate pair
                    c = 0x10000 + ((c & 0x3ff) << 10) + (str[i + 1] & 0x3ff);
                    ++i;
                }
                else if ((c & 0xf800) == 0xd800)
                {
                    c = 0xfffd;
                }
                if (c == '%')
                {
                    if (i + 2 < endIndex)
                    {
                        int a = ToHex(str[i + 1]);
                        int b = ToHex(str[i + 2]);
                        if (a >= 0 && b >= 0)
                        {
                            b = (a * 16) + b;
                            i += 2;
                            // b now contains the byte read
                            if (bytesNeeded == 0)
                            {
                                // this is the lead byte
                                if (b < 0x80)
                                {
                                    retString.Append((char)b);
                                    continue;
                                }
                                else if (b >= 0xc2 && b <= 0xdf)
                                {
                                    markedPos = i;
                                    bytesNeeded = 1;
                                    cp = b - 0xc0;
                                }
                                else if (b >= 0xe0 && b <= 0xef)
                                {
                                    markedPos = i;
                                    lower = (b == 0xe0) ? 0xa0 : 0x80;
                                    upper = (b == 0xed) ? 0x9f : 0xbf;
                                    bytesNeeded = 2;
                                    cp = b - 0xe0;
                                }
                                else if (b >= 0xf0 && b <= 0xf4)
                                {
                                    markedPos = i;
                                    lower = (b == 0xf0) ? 0x90 : 0x80;
                                    upper = (b == 0xf4) ? 0x8f : 0xbf;
                                    bytesNeeded = 3;
                                    cp = b - 0xf0;
                                }
                                else
                                {
                                    // illegal byte in UTF-8
                                    retString.Append('\uFFFD');
                                    continue;
                                }
                                cp <<= 6 * bytesNeeded;
                                continue;
                            }
                            else
                            {
                                // this is a second or further byte
                                if (b < lower || b > upper)
                                {
                                    // illegal trailing byte
                                    cp = bytesNeeded = bytesSeen = 0;
                                    lower = 0x80;
                                    upper = 0xbf;
                                    i = markedPos; // reset to the last marked position
                                    retString.Append('\uFFFD');
                                    continue;
                                }
                                // reset lower and upper for the third
                                // and further bytes
                                lower = 0x80;
                                upper = 0xbf;
                                ++bytesSeen;
                                cp += (b - 0x80) << (6 * (bytesNeeded - bytesSeen));
                                markedPos = i;
                                if (bytesSeen != bytesNeeded)
                                {
                                    // continue if not all bytes needed
                                    // were read yet
                                    continue;
                                }
                                int ret = cp;
                                cp = 0;
                                bytesSeen = 0;
                                bytesNeeded = 0;
                                // append the Unicode character
                                if (ret <= 0xffff)
                                {
                                    retString.Append((char)ret);
                                }
                                else
                                {
                                    retString.Append((char)((((ret - 0x10000) >> 10) & 0x3ff) |
                                        0xd800));
                                    retString.Append((char)(((ret - 0x10000) & 0x3ff) |
                  0xdc00));
                                }
                                continue;
                            }
                        }
                    }
                }
                if (bytesNeeded > 0)
                {
                    // we expected further bytes here,
                    // so emit a replacement character instead
                    bytesNeeded = 0;
                    retString.Append('\uFFFD');
                }
                // append the code point as is
                if (c <= 0xffff)
                {
                    {
                        retString.Append((char)c);
                    }
                }
                else if (c <= 0x10ffff)
                {
                    retString.Append((char)((((c - 0x10000) >> 10) & 0x3ff) | 0xd800));
                    retString.Append((char)(((c - 0x10000) & 0x3ff) | 0xdc00));
                }
            }
            if (bytesNeeded > 0)
            {
                // we expected further bytes here,
                // so emit a replacement character instead
                bytesNeeded = 0;
                retString.Append('\uFFFD');
            }
            return retString.ToString();
        }

        /// <summary>Encodes characters other than "unreserved" characters for
        /// URIs.</summary>
        /// <param name='s'>A string to encode.</param>
        /// <returns>The encoded string.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='s'/> is null.</exception>
        public static string EncodeStringForURI(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }
            var index = 0;
            var builder = new StringBuilder();
            while (index < s.Length)
            {
                int c = s[index];
                if ((c & 0xfc00) == 0xd800 && index + 1 < s.Length &&
                  (s[index + 1] & 0xfc00) == 0xdc00)
                {
                    // Get the Unicode code point for the surrogate pair
                    c = 0x10000 + ((c & 0x3ff) << 10) + (s[index + 1] & 0x3ff);
                }
                else if ((c & 0xf800) == 0xd800)
                {
                    c = 0xfffd;
                }
                if (c >= 0x10000)
                {
                    ++index;
                }
                if ((c & 0x7F) == c && ((c >= 'A' && c <= 'Z') ||
                    (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') ||
        "-_.~".IndexOf((char)c) >= 0))
                {
                    builder.Append((char)c);
                    ++index;
                }
                else
                {
                    PercentEncodeUtf8(builder, c);
                    ++index;
                }
            }
            return builder.ToString();
        }

        private static bool IsIfragmentChar(int c)
        {
            // '%' omitted
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
              (c >= '0' && c <= '9') || ((c & 0x7F) == c &&
      "/?-._~:@!$&'()*+,;=".IndexOf((char)c) >= 0) ||

              (c >= 0xa0 && c <= 0xd7ff) || (c >= 0xf900 && c <= 0xfdcf) ||
              (c >= 0xfdf0 && c <= 0xffef) ||
              (c >= 0xe1000 && c <= 0xefffd) || (c >= 0x10000 && c <= 0xdfffd &&
                (c & 0xfffe) != 0xfffe);
        }

        private static bool IsIpchar(int c)
        {
            // '%' omitted
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
              (c >= '0' && c <= '9') || ((c & 0x7F) == c &&
      "/-._~:@!$&'()*+,;=".IndexOf((char)c) >= 0) ||

              (c >= 0xa0 && c <= 0xd7ff) || (c >= 0xf900 && c <= 0xfdcf) ||
              (c >= 0xfdf0 && c <= 0xffef) ||
              (c >= 0xe1000 && c <= 0xefffd) || (c >= 0x10000 && c <= 0xdfffd &&
                (c & 0xfffe) != 0xfffe);
        }

        private static bool IsIqueryChar(int c)
        {
            // '%' omitted
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
              (c >= '0' && c <= '9') || ((c & 0x7F) == c &&
      "/?-._~:@!$&'()*+,;=".IndexOf((char)c) >= 0) ||

              (c >= 0xa0 && c <= 0xd7ff) || (c >= 0xe000 && c <= 0xfdcf) ||
              (c >= 0xfdf0 && c <= 0xffef) ||
              (c >= 0x10000 && c <= 0x10fffd && (c & 0xfffe) != 0xfffe &&
                !(c >= 0xe0000 && c <= 0xe0fff));
        }

        private static bool IsIRegNameChar(int c)
        {
            // '%' omitted
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
              (c >= '0' && c <= '9') || ((c & 0x7F) == c &&
      "-._~!$&'()*+,;=".IndexOf((char)c) >= 0) ||

              (c >= 0xa0 && c <= 0xd7ff) || (c >= 0xf900 && c <= 0xfdcf) ||
              (c >= 0xfdf0 && c <= 0xffef) ||
              (c >= 0xe1000 && c <= 0xefffd) || (c >= 0x10000 && c <= 0xdfffd &&
                (c & 0xfffe) != 0xfffe);
        }

        private static bool IsIUserInfoChar(int c)
        {
            // '%' omitted
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
              (c >= '0' && c <= '9') || ((c & 0x7F) == c &&
      "-._~:!$&'()*+,;=".IndexOf((char)c) >= 0) ||

              (c >= 0xa0 && c <= 0xd7ff) || (c >= 0xf900 && c <= 0xfdcf) ||
              (c >= 0xfdf0 && c <= 0xffef) ||
              (c >= 0xe1000 && c <= 0xefffd) || (c >= 0x10000 && c <= 0xdfffd &&
                (c & 0xfffe) != 0xfffe);
        }

        /// <summary>Determines whether the substring is a valid CURIE
        /// reference under RDFA 1.1. (The CURIE reference is the part after
        /// the colon.).</summary>
        /// <param name='s'>A string containing a CURIE reference. Can be
        /// null.</param>
        /// <param name='offset'>An index starting at 0 showing where the
        /// desired portion of "s" begins.</param>
        /// <param name='length'>The number of elements in the desired portion
        /// of "s" (but not more than "s" 's length).</param>
        /// <returns><c>true</c> if the substring is a valid CURIE reference
        /// under RDFA 1; otherwise, <c>false</c>. Returns false if <paramref
        /// name='s'/> is null.</returns>
        /// <exception cref='ArgumentException'>Either <paramref
        /// name='offset'/> or <paramref name='length'/> is less than 0 or
        /// greater than <paramref name='s'/> 's length, or <paramref
        /// name='s'/> 's length minus <paramref name='offset'/> is less than
        /// <paramref name='length'/>.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='s'/> is null.</exception>
        public static bool IsValidCurieReference(
          string s,
          int offset,
          int length)
        {
            if (s == null)
            {
                return false;
            }
            if (offset < 0)
            {
                throw new ArgumentException("offset(" + offset + ") is less than " +
                  "0 ");
            }
            if (offset > s.Length)
            {
                throw new ArgumentException("offset(" + offset + ") is more than " +
                  s.Length);
            }
            if (length < 0)
            {
                throw new ArgumentException(
                  "length (" + length + ") is less than " + "0 ");
            }
            if (length > s.Length)
            {
                throw new ArgumentException(
                  "length (" + length + ") is more than " + s.Length);
            }
            if (s.Length - offset < length)
            {
                throw new ArgumentException(
                  "s's length minus " + offset + " (" + (s.Length - offset) +
                  ") is less than " + length);
            }
            if (length == 0)
            {
                return true;
            }
            int index = offset;
            int valueSLength = offset + length;
            var state = 0;
            if (index + 2 <= valueSLength && s[index] == '/' && s[index + 1] == '/')
            {
                // has an authority, which is not allowed
                return false;
            }
            state = 0; // IRI Path
            while (index < valueSLength)
            {
                // Get the next Unicode character
                int c = s[index];
                if ((c & 0xfc00) == 0xd800 && index + 1 < valueSLength &&
                  (s[index + 1] & 0xfc00) == 0xdc00)
                {
                    // Get the Unicode code point for the surrogate pair
                    c = 0x10000 + ((c & 0x3ff) << 10) + (s[index + 1] & 0x3ff);
                    ++index;
                }
                else if ((c & 0xf800) == 0xd800)
                {
                    // error
                    return false;
                }
                if (c == '%')
                {
                    // Percent encoded character
                    if (index + 2 < valueSLength && IsHexChar(s[index + 1]) &&
                      IsHexChar(s[index + 2]))
                    {
                        index += 3;
                        continue;
                    }
                    return false;
                }
                if (state == 0)
                { // Path
                    if (c == '?')
                    {
                        state = 1; // move to query state
                    }
                    else if (c == '#')
                    {
                        state = 2; // move to fragment state
                    }
                    else if (!IsIpchar(c))
                    {
                        return false;
                    }
                    ++index;
                }
                else if (state == 1)
                { // Query
                    if (c == '#')
                    {
                        state = 2; // move to fragment state
                    }
                    else if (!IsIqueryChar(c))
                    {
                        return false;
                    }
                    ++index;
                }
                else if (state == 2)
                { // Fragment
                    if (!IsIfragmentChar(c))
                    {
                        return false;
                    }
                    ++index;
                }
            }
            return true;
        }

        /// <summary>Builds an internationalized resource identifier (IRI) from
        /// its components.</summary>
        /// <param name='schemeAndAuthority'>String representing a scheme
        /// component, an authority component, or both. Examples of this
        /// parameter include "example://example", "example:", and "//example",
        /// but not "example". Can be null or empty.</param>
        /// <param name='path'>A string representing a path component. Can be
        /// null or empty.</param>
        /// <param name='query'>The query string. Can be null or empty.</param>
        /// <param name='fragment'>The fragment identifier. Can be null or
        /// empty.</param>
        /// <returns>A URI built from the given components.</returns>
        /// <exception cref='ArgumentException'>Invalid schemeAndAuthority
        /// parameter, or the arguments result in an invalid IRI.</exception>
        public static string BuildIRI(
          string schemeAndAuthority,
          string path,
          string query,
          string fragment)
        {
            var builder = new StringBuilder();
            if (!String.IsNullOrEmpty(schemeAndAuthority))
            {
                int[] irisplit = SplitIRI(schemeAndAuthority);
                // NOTE: Path component is always present in URIs;
                // we check here whether path component is empty
                if (irisplit == null || (irisplit[0] < 0 && irisplit[2] < 0) ||
                  irisplit[4] != irisplit[5] || irisplit[6] >= 0 || irisplit[8] >= 0)
                {
                    throw new ArgumentException("invalid schemeAndAuthority");
                }
            }
            if (String.IsNullOrEmpty(path))
            {
                path = String.Empty;
            }
            for (var phase = 0; phase < 3; ++phase)
            {
                string s = path;
                if (phase == 1)
                {
                    s = query;
                    if (query == null)
                    {
                        continue;
                    }
                    builder.Append('?');
                }
                else if (phase == 2)
                {
                    s = fragment;
                    if (fragment == null)
                    {
                        continue;
                    }
                    builder.Append('#');
                }
                var index = 0;
                if (query == null || fragment == null)
                {
                    continue;
                }
                while (index < s.Length)
                {
                    int c = s[index];
                    if ((c & 0xfc00) == 0xd800 && index + 1 < s.Length &&
                      (s[index + 1] & 0xfc00) == 0xdc00)
                    {
                        // Get the Unicode code point for the surrogate pair
                        c = 0x10000 + ((c & 0x3ff) << 10) + (s[index + 1] & 0x3ff);
                    }
                    else if ((c & 0xf800) == 0xd800)
                    {
                        c = 0xfffd;
                    }
                    if (c >= 0x10000)
                    {
                        ++index;
                    }
                    if (c == '%')
                    {
                        if (index + 2 < s.Length && IsHexChar(s[index + 1]) &&
                          IsHexChar(s[index + 2]))
                        {
                            builder.Append('%');
                            builder.Append(s[index + 1]);
                            builder.Append(s[index + 2]);
                            index += 3;
                        }
                        else
                        {
                            builder.Append("%25");
                            ++index;
                        }
                    }
                    else if ((c & 0x7f) == c &&
                    ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') ||
                      (c >= '0' && c <= '9') ||
                      "-_.~/(=):!$&'*+,;@".IndexOf((char)c) >= 0))
                    {
                        // NOTE: Question mark will be percent encoded even though
                        // it can appear in query and fragment strings
                        builder.Append((char)c);
                        ++index;
                    }
                    else
                    {
                        PercentEncodeUtf8(builder, c);
                        ++index;
                    }
                }
            }
            string ret = builder.ToString();
            if (SplitIRI(ret) == null)
            {
                throw new ArgumentException("The arguments result in an invalid IRI.");
            }
            return ret;
        }

        /// <summary>Returns whether a string is a valid IRI according to the
        /// IRIStrict parse mode.</summary>
        /// <param name='s'>A text string. Can be null.</param>
        /// <returns>True if the string is not null and is a valid IRI;
        /// otherwise, false.</returns>
        public static bool IsValidIRI(string s)
        {
            return ((s == null) ? null : SplitIRI(
              s,
              0,
              s.Length,
              URIUtility.ParseMode.IRIStrict)) != null;
        }

        /// <summary>Returns whether a string is a valid IRI according to the
        /// given parse mode.</summary>
        /// <param name='s'>A text string. Can be null.</param>
        /// <param name='parseMode'>The parse mode to use when checking for a
        /// valid IRI.</param>
        /// <returns>True if the string is not null and is a valid IRI;
        /// otherwise, false.</returns>
        public static bool IsValidIRI(string s, URIUtility.ParseMode parseMode)
        {
            return ((s == null) ? null : SplitIRI(
              s,
              0,
              s.Length,
              parseMode)) != null;
        }

        private const string ValueDotSlash = "." + "/";
        private const string ValueSlashDot = "/" + ".";

        private static string NormalizePath(string path)
        {
            int len = path.Length;
            if (len == 0 || path.Equals("..", StringComparison.Ordinal) ||
              path.Equals(".", StringComparison.Ordinal))
            {
                return String.Empty;
            }
            if (path.IndexOf(ValueSlashDot, StringComparison.Ordinal) < 0 &&
              path.IndexOf(
                ValueDotSlash,
                StringComparison.Ordinal) < 0)
            {
                return path;
            }
            var builder = new StringBuilder();
            var index = 0;
            while (index < len)
            {
                char c = path[index];
                if ((index + 3 <= len && c == '/' && path[index + 1] == '.' &&
                    path[index + 2] == '/') || (index + 2 == len && c == '.' &&
                    path[index + 1] == '.'))
                {
                    // begins with "/./" or is "..";
                    // move index by 2
                    index += 2;
                    continue;
                }
                if (index + 3 <= len && c == '.' &&
                  path[index + 1] == '.' && path[index + 2] == '/')
                {
                    // begins with "../";
                    // move index by 3
                    index += 3;
                    continue;
                }
                if ((index + 2 <= len && c == '.' &&
                    path[index + 1] == '/') || (index + 1 == len && c == '.'))
                {
                    // begins with "./" or is ".";
                    // move index by 1
                    ++index;
                    continue;
                }
                if (index + 2 == len && c == '/' &&
                  path[index + 1] == '.')
                {
                    // is "/."; append '/' and break
                    builder.Append('/');
                    break;
                }
                if (index + 3 == len && c == '/' &&
                  path[index + 1] == '.' && path[index + 2] == '.')
                {
                    // is "/.."; remove last segment,
                    // append "/" and return
                    int index2 = builder.Length - 1;
                    string builderString = builder.ToString();
                    while (index2 >= 0)
                    {
                        if (builderString[index2] == '/')
                        {
                            break;
                        }
                        --index2;
                    }
                    if (index2 < 0)
                    {
                        index2 = 0;
                    }
                    builder.Length = index2;
                    builder.Append('/');
                    break;
                }
                if (index + 4 <= len && c == '/' && path[index + 1] == '.' &&
                  path[index + 2] == '.' && path[index + 3] == '/')
                {
                    // begins with "/../"; remove last segment
                    int index2 = builder.Length - 1;
                    string builderString = builder.ToString();
                    while (index2 >= 0)
                    {
                        if (builderString[index2] == '/')
                        {
                            break;
                        }
                        --index2;
                    }
                    if (index2 < 0)
                    {
                        index2 = 0;
                    }
                    builder.Length = index2;
                    index += 3;
                    continue;
                }
                builder.Append(c);
                ++index;
                while (index < len)
                {
                    // Move the rest of the
                    // path segment until the next '/'
                    c = path[index];
                    if (c == '/')
                    {
                        break;
                    }
                    builder.Append(c);
                    ++index;
                }
            }
            return builder.ToString();
        }

        private static int ParseIPLiteral(string s, int offset, int endOffset)
        {
            int index = offset;
            if (offset == endOffset)
            {
                return -1;
            }
            // Assumes that the character before offset
            // is a '['
            if (s[index] == 'v')
            {
                // IPvFuture
                ++index;
                var hex = false;
                while (index < endOffset)
                {
                    char c = s[index];
                    if (IsHexChar(c))
                    {
                        hex = true;
                    }
                    else
                    {
                        break;
                    }
                    ++index;
                }
                if (!hex)
                {
                    return -1;
                }
                if (index >= endOffset || s[index] != '.')
                {
                    return -1;
                }
                ++index;
                hex = false;
                while (index < endOffset)
                {
                    char c = s[index];
                    if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                      (c >= '0' && c <= '9') || ((c & 0x7F) == c &&
                        ":-._~!$&'()*+,;=".IndexOf(c) >= 0))
                    {
                        hex = true;
                    }
                    else
                    {
                        break;
                    }
                    ++index;
                }
                if (!hex)
                {
                    return -1;
                }
                if (index >= endOffset || s[index] != ']')
                {
                    return -1;
                }
                ++index;
                return index;
            }
            if (s[index] == ':' ||
              IsHexChar(s[index]))
            {
                int startIndex = index;
                while (index < endOffset && ((s[index] >= 65 && s[index] <= 70) ||
                    (s[index] >= 97 && s[index] <= 102) || (s[index] >= 48 && s[index]
                      <= 58) || (s[index] == 46)))
                {
                    ++index;
                }
                if (index >= endOffset || (s[index] != ']' && s[index] != '%'))
                {
                    return -1;
                }
                // NOTE: Array is initialized to zeros
                var addressParts = new int[8];
                int ipEndIndex = index;
                var doubleColon = false;
                var doubleColonPos = 0;
                var totalParts = 0;
                var ipv4part = false;
                index = startIndex;
                // DebugUtility.Log(s.Substring(startIndex, ipEndIndex-startIndex));
                for (var part = 0; part < 8; ++part)
                {
                    if (!doubleColon &&
                      ipEndIndex - index > 1 && s[index] == ':' &&
                      s[index + 1] == ':')
                    {
                        doubleColon = true;
                        doubleColonPos = part;
                        index += 2;
                        if (index == ipEndIndex)
                        {
                            break;
                        }
                    }
                    var hex = 0;
                    var haveHex = false;
                    int curindex = index;
                    for (var i = 0; i < 4; ++i)
                    {
                        if (IsHexChar(s[index]))
                        {
                            hex = (hex << 4) | ToHex(s[index]);
                            haveHex = true;
                            ++index;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (!haveHex)
                    {
                        return -1;
                    }
                    if (index < ipEndIndex && s[index] == '.' && part < 7)
                    {
                        ipv4part = true;
                        index = curindex;
                        break;
                    }
                    addressParts[part] = hex;
                    ++totalParts;
                    if (index < ipEndIndex && s[index] != ':')
                    {
                        return -1;
                    }
                    if (index == ipEndIndex && doubleColon)
                    {
                        break;
                    }
                    // Skip single colon, but not double colon
                    if (index < ipEndIndex && (index + 1 >= ipEndIndex ||
                        s[index + 1] != ':'))
                    {
                        ++index;
                    }
                }
                if (index != ipEndIndex && !ipv4part)
                {
                    return -1;
                }
                if (doubleColon || ipv4part)
                {
                    if (ipv4part)
                    {
                        var ipparts = new int[4];
                        for (var part = 0; part < 4; ++part)
                        {
                            if (part > 0)
                            {
                                if (index < ipEndIndex && s[index] == '.')
                                {
                                    ++index;
                                }
                                else
                                {
                                    return -1;
                                }
                            }
                            if (index + 1 < ipEndIndex && s[index] == '0' &&
                              (s[index + 1] >= '0' && s[index + 1] <= '9'))
                            {
                                return -1;
                            }
                            var dec = 0;
                            var haveDec = false;
                            int curindex = index;
                            for (var i = 0; i < 4; ++i)
                            {
                                if (s[index] >= '0' && s[index] <= '9')
                                {
                                    dec = (dec * 10) + ((int)s[index] - '0');
                                    haveDec = true;
                                    ++index;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            if (!haveDec || dec > 255)
                            {
                                return -1;
                            }
                            ipparts[part] = dec;
                        }
                        if (index != ipEndIndex)
                        {
                            return -1;
                        }
                        addressParts[totalParts] = (ipparts[0] << 8) | ipparts[1];
                        addressParts[totalParts + 1] = (ipparts[2] << 8) | ipparts[3];
                        totalParts += 2;
                        if (!doubleColon && totalParts != 8)
                        {
                            return -1;
                        }
                    }
                    if (doubleColon)
                    {
                        int resid = 8 - totalParts;
                        if (resid == 0)
                        {
                            // Purported IPv6 address contains
                            // 8 parts and a double colon
                            return -1;
                        }
                        var newAddressParts = new int[8];
                        Array.Copy(addressParts, newAddressParts, doubleColonPos);
                        Array.Copy(
                          addressParts,
                          doubleColonPos,
                          newAddressParts,
                          doubleColonPos + resid,
                          totalParts - doubleColonPos);
                        Array.Copy(newAddressParts, addressParts, 8);
                    }
                }
                else if (totalParts != 8)
                {
                    return -1;
                }

                // DebugUtility.Log("{0:X4}:{0:X4}:{0:X4}:{0:X4}:{0:X4}:" +
                // "{0:X4}:{0:X4}:{0:X4}"
                // ,
                // addressParts[0], addressParts[1], addressParts[2],
                // addressParts[3], addressParts[4], addressParts[5],
                // addressParts[6], addressParts[7]);
                if (s[index] == '%')
                {
                    if (index + 2 < endOffset && s[index + 1] == '2' &&
                      s[index + 2] == '5' && (addressParts[0] & 0xFFC0) == 0xFE80)
                    {
                        // Zone identifier in an IPv6 address
                        // (see RFC6874)
                        // NOTE: Allowed only if address has prefix fe80::/10
                        index += 3;
                        var haveChar = false;
                        while (index < endOffset)
                        {
                            char c = s[index];
                            if (c == ']')
                            {
                                return haveChar ? index + 1 : -1;
                            }
                            if (c == '%')
                            {
                                if (index + 2 < endOffset && IsHexChar(s[index + 1]) &&
                                  IsHexChar(s[index + 2]))
                                {
                                    index += 3;
                                    haveChar = true;
                                    continue;
                                }
                                return -1;
                            }
                            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                              (c >= '0' && c <= '9') || c == '.' || c == '_' || c == '-' ||
                              c == '~')
                            {
                                // unreserved character under RFC3986
                                ++index;
                                haveChar = true;
                                continue;
                            }
                            return -1;
                        }
                        return -1;
                    }
                    return -1;
                }
                ++index;
                return index;
            }
            return -1;
        }

        private static string PathParent(
          string refValue,
          int startIndex,
          int endIndex)
        {
            if (startIndex > endIndex)
            {
                return String.Empty;
            }
            --endIndex;
            while (endIndex >= startIndex)
            {
                if (refValue[endIndex] == '/')
                {
                    return refValue.Substring(startIndex, (endIndex + 1) - startIndex);
                }
                --endIndex;
            }
            return String.Empty;
        }

        private static void PercentEncode(StringBuilder buffer, int b)
        {
            buffer.Append('%');
            buffer.Append(HexChars[(b >> 4) & 0x0f]);
            buffer.Append(HexChars[b & 0x0f]);
        }

        private static void PercentEncodeUtf8(StringBuilder buffer, int cp)
        {
            if (cp <= 0x7f)
            {
                buffer.Append('%');
                buffer.Append(HexChars[(cp >> 4) & 0x0f]);
                buffer.Append(HexChars[cp & 0x0f]);
            }
            else if (cp <= 0x7ff)
            {
                PercentEncode(buffer, 0xc0 | ((cp >> 6) & 0x1f));
                PercentEncode(buffer, 0x80 | (cp & 0x3f));
            }
            else if (cp <= 0xffff)
            {
                PercentEncode(buffer, 0xe0 | ((cp >> 12) & 0x0f));
                PercentEncode(buffer, 0x80 | ((cp >> 6) & 0x3f));
                PercentEncode(buffer, 0x80 | (cp & 0x3f));
            }
            else
            {
                PercentEncode(buffer, 0xf0 | ((cp >> 18) & 0x07));
                PercentEncode(buffer, 0x80 | ((cp >> 12) & 0x3f));
                PercentEncode(buffer, 0x80 | ((cp >> 6) & 0x3f));
                PercentEncode(buffer, 0x80 | (cp & 0x3f));
            }
        }

        /// <summary>Resolves a URI or IRI relative to another URI or
        /// IRI.</summary>
        /// <param name='refValue'>A string representing a URI or IRI
        /// reference. Example: <c>dir/file.txt</c>.</param>
        /// <param name='absoluteBase'>A string representing an absolute URI or
        /// IRI reference. Can be null. Example:
        /// <c>http://example.com/my/path/</c>.</param>
        /// <returns>The resolved IRI, or null if <paramref name='refValue'/>
        /// is null or is not a valid IRI. If <paramref name='absoluteBase'/>
        /// is null or is not a valid IRI, returns refValue. Example:
        /// <c>http://example.com/my/path/dir/file.txt</c>.</returns>
        public static string RelativeResolve(string refValue,
          string absoluteBase)
        {
            return RelativeResolve(
              refValue,
              absoluteBase,
              URIUtility.ParseMode.IRIStrict);
        }

        /// <summary>Resolves a URI or IRI relative to another URI or
        /// IRI.</summary>
        /// <param name='refValue'>A string representing a URI or IRI
        /// reference. Example: <c>dir/file.txt</c>. Can be null.</param>
        /// <param name='absoluteBase'>A string representing an absolute URI or
        /// IRI reference. Can be null. Example:
        /// <c>http://example.com/my/path/</c>.</param>
        /// <param name='parseMode'>Parse mode that specifies whether certain
        /// characters are allowed when parsing IRIs and URIs.</param>
        /// <returns>The resolved IRI, or null if <paramref name='refValue'/>
        /// is null or is not a valid IRI. If <paramref name='absoluteBase'/>
        /// is null or is not a valid IRI, returns refValue.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='refValue'/> or <paramref name='absoluteBase'/> or <paramref
        /// name='refValue'/> or <paramref name='refValue'/> is
        /// null.</exception>
        public static string RelativeResolve(
          string refValue,
          string absoluteBase,
          URIUtility.ParseMode parseMode)
        {
            int[] segments = (refValue == null) ? null : SplitIRI(
              refValue,
              0,
              refValue.Length,
              parseMode);
            if (segments == null)
            {
                return null;
            }
            int[] segmentsBase = (
                absoluteBase == null) ? null : SplitIRI(
                absoluteBase,
                0,
                absoluteBase.Length,
                parseMode);
            if (segmentsBase == null)
            {
                return refValue;
            }
            var builder = new StringBuilder();
            if (segments[0] >= 0)
            { // scheme present
                if (refValue == null)
                {
                    throw new InvalidOperationException();
                }
                AppendScheme(builder, refValue, segments);
                AppendAuthority(builder, refValue, segments);
                AppendNormalizedPath(builder, refValue, segments);
                AppendQuery(builder, refValue, segments);
                AppendFragment(builder, refValue, segments);
            }
            else if (segments[2] >= 0)
            { // authority present
                if (absoluteBase == null)
                {
                    throw new InvalidOperationException();
                }
                AppendScheme(builder, absoluteBase, segmentsBase);
                if (refValue == null)
                {
                    throw new ArgumentNullException(nameof(refValue));
                }
                AppendAuthority(builder, refValue, segments);
                AppendNormalizedPath(builder, refValue, segments);
                AppendQuery(builder, refValue, segments);
                AppendFragment(builder, refValue, segments);
            }
            else if (segments[4] == segments[5])
            {
                if (absoluteBase == null)
                {
                    throw new ArgumentNullException(nameof(absoluteBase));
                }
                AppendScheme(builder, absoluteBase, segmentsBase);
                AppendAuthority(builder, absoluteBase, segmentsBase);
                AppendPath(builder, absoluteBase, segmentsBase);
                if (segments[6] >= 0)
                {
                    if (refValue == null)
                    {
                        throw new ArgumentNullException(nameof(refValue));
                    }
                    AppendQuery(builder, refValue, segments);
                }
                else
                {
                    AppendQuery(builder, absoluteBase, segmentsBase);
                }
                if (refValue == null)
                {
                    throw new ArgumentNullException(nameof(refValue));
                }
                AppendFragment(builder, refValue, segments);
            }
            else
            {
                if (absoluteBase == null)
                {
                    throw new InvalidOperationException();
                }
                AppendScheme(builder, absoluteBase, segmentsBase);
                AppendAuthority(builder, absoluteBase, segmentsBase);
                if (refValue == null)
                {
                    throw new InvalidOperationException();
                }
                if (segments[4] < segments[5] && refValue[segments[4]] == '/')
                {
                    AppendNormalizedPath(builder, refValue, segments);
                }
                else
                {
                    var merged = new StringBuilder();
                    if (segmentsBase[2] >= 0 && segmentsBase[4] == segmentsBase[5])
                    {
                        merged.Append('/');
                        if (absoluteBase == null)
                        {
                            throw new InvalidOperationException();
                        }
                        AppendPath(merged, refValue, segments);
                        builder.Append(NormalizePath(merged.ToString()));
                    }
                    else
                    {
                        merged.Append(
                          PathParent(
                            absoluteBase,
                            segmentsBase[4],
                            segmentsBase[5]));
                        AppendPath(merged, refValue, segments);
                        builder.Append(NormalizePath(merged.ToString()));
                    }
                }
                AppendQuery(builder, refValue, segments);
                AppendFragment(builder, refValue, segments);
            }
            return builder.ToString();
        }

        private static string ToLowerCaseAscii(string str)
        {
            if (str == null)
            {
                return null;
            }
            int len = str.Length;
            var c = (char)0;
            var hasUpperCase = false;
            for (var i = 0; i < len; ++i)
            {
                c = str[i];
                if (c >= 'A' && c <= 'Z')
                {
                    hasUpperCase = true;
                    break;
                }
            }
            if (!hasUpperCase)
            {
                return str;
            }
            var builder = new StringBuilder();
            for (var i = 0; i < len; ++i)
            {
                c = str[i];
                if (c >= 'A' && c <= 'Z')
                {
                    builder.Append((char)(c + 0x20));
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        /// <summary>Parses an Internationalized Resource Identifier (IRI)
        /// reference under RFC3987. If the IRI reference is syntactically
        /// valid, splits the string into its components and returns an array
        /// containing those components.</summary>
        /// <param name='s'>A string that contains an IRI. Can be null.</param>
        /// <returns>If the string is a valid IRI reference, returns an array
        /// of five strings. Each of the five pairs corresponds to the IRI's
        /// scheme, authority, path, query, or fragment identifier,
        /// respectively. If a component is absent, the corresponding element
        /// will be null. If the string is null or is not a valid IRI, returns
        /// null.</returns>
        public static string[] SplitIRIToStrings(string s)
        {
            int[] indexes = SplitIRI(s);
            if (indexes == null)
            {
                return null;
            }
            if (s == null)
            {
                // Should not happen because indexes would be null
                // if s is null
                throw new InvalidOperationException();
            }
            string s1 = indexes[0] < 0 ? null : s.Substring(
              indexes[0],
              indexes[1] - indexes[0]);
            string s2 = indexes[2] < 0 ? null : s.Substring(
              indexes[2],
              indexes[3] - indexes[2]);
            string s3 = indexes[4] < 0 ? null : s.Substring(
              indexes[4],
              indexes[5] - indexes[4]);
            string s4 = indexes[6] < 0 ? null : s.Substring(
              indexes[6],
              indexes[7] - indexes[6]);
            string s5 = indexes[8] < 0 ? null : s.Substring(
              indexes[8],
              indexes[9] - indexes[8]);
            return new string[] {
        s1 == null ? null : ToLowerCaseAscii(s1),
        s2, s3, s4, s5,
      };
        }

        /// <summary>Parses an Internationalized Resource Identifier (IRI)
        /// reference under RFC3987. If the IRI reference is syntactically
        /// valid, splits the string into its components and returns an array
        /// containing the indices into the components.</summary>
        /// <param name='s'>A string that contains an IRI. Can be null.</param>
        /// <returns>If the string is a valid IRI reference, returns an array
        /// of 10 integers. Each of the five pairs corresponds to the start and
        /// end index of the IRI's scheme, authority, path, query, or fragment
        /// identifier, respectively. The scheme, authority, query, and
        /// fragment identifier, if present, will each be given without the
        /// ending colon, the starting "//", the starting "?", and the starting
        /// "#", respectively. If a component is absent, both indices in that
        /// pair will be -1. If the string is null or is not a valid IRI,
        /// returns null.</returns>
        public static int[] SplitIRI(string s)
        {
            return (s == null) ? null : SplitIRI(
                s,
                0,
                s.Length,
                URIUtility.ParseMode.IRIStrict);
        }

        /// <summary>Parses a substring that represents an Internationalized
        /// Resource Identifier (IRI) under RFC3987. If the IRI is
        /// syntactically valid, splits the string into its components and
        /// returns an array containing the indices into the
        /// components.</summary>
        /// <param name='s'>A string that contains an IRI. Can be null.</param>
        /// <param name='offset'>An index starting at 0 showing where the
        /// desired portion of "s" begins.</param>
        /// <param name='length'>The length of the desired portion of "s" (but
        /// not more than "s" 's length).</param>
        /// <param name='parseMode'>Parse mode that specifies whether certain
        /// characters are allowed when parsing IRIs and URIs.</param>
        /// <returns>If the string is a valid IRI, returns an array of 10
        /// integers. Each of the five pairs corresponds to the start and end
        /// index of the IRI's scheme, authority, path, query, or fragment
        /// component, respectively. The scheme, authority, query, and fragment
        /// components, if present, will each be given without the ending
        /// colon, the starting "//", the starting "?", and the starting "#",
        /// respectively. If a component is absent, both indices in that pair
        /// will be -1 (an index won't be less than 0 in any other case). If
        /// the string is null or is not a valid IRI, returns null.</returns>
        /// <exception cref='ArgumentException'>Either <paramref
        /// name='offset'/> or <paramref name='length'/> is less than 0 or
        /// greater than <paramref name='s'/> 's length, or <paramref
        /// name='s'/> 's length minus <paramref name='offset'/> is less than
        /// <paramref name='length'/>.</exception>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='s'/> is null.</exception>
        public static int[] SplitIRI(
          string s,
          int offset,
          int length,
          URIUtility.ParseMode parseMode)
        {
            if (s == null)
            {
                return null;
            }
            if (offset < 0)
            {
                throw new ArgumentException("offset(" + offset +
                  ") is less than 0");
            }
            if (offset > s.Length)
            {
                throw new ArgumentException("offset(" + offset +
                  ") is more than " + s.Length);
            }
            if (length < 0)
            {
                throw new ArgumentException("length(" + length +
                  ") is less than 0");
            }
            if (length > s.Length)
            {
                throw new ArgumentException("length(" + length +
                  ") is more than " + s.Length);
            }
            if (s.Length - offset < length)
            {
                throw new ArgumentException("s's length minus " + offset + "(" +
                  (s.Length - offset) + ") is less than " + length);
            }
            int[] retval = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
            if (length == 0)
            {
                retval[4] = 0;
                retval[5] = 0;
                return retval;
            }
            bool asciiOnly = parseMode == URIUtility.ParseMode.URILenient ||
              parseMode == URIUtility.ParseMode.URIStrict;
            bool strict = parseMode == URIUtility.ParseMode.URIStrict || parseMode ==
              URIUtility.ParseMode.IRIStrict;
            int index = offset;
            int valueSLength = offset + length;
            var scheme = false;
            // scheme
            while (index < valueSLength)
            {
                int c = s[index];
                if (index > offset && c == ':')
                {
                    scheme = true;
                    retval[0] = offset;
                    retval[1] = index;
                    ++index;
                    break;
                }
                if (strict && index == offset && !((c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z')))
                {
                    break;
                }
                if (strict && index > offset &&
                  !((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' &&
                      c <= '9') || c == '+' || c == '-' || c == '.'))
                {
                    break;
                }
                if (!strict && (c == '#' || c == ':' || c == '?' || c == '/'))
                {
                    break;
                }
                ++index;
            }
            if (!scheme)
            {
                index = offset;
            }
            var state = 0;
            if (index + 2 <= valueSLength && s[index] == '/' && s[index + 1] == '/')
            {
                // authority
                // (index + 2, valueSLength)
                index += 2;
                int authorityStart = index;
                retval[2] = authorityStart;
                retval[3] = valueSLength;
                state = 0; // userinfo
                           // Check for userinfo
                while (index < valueSLength)
                {
                    int c = s[index];
                    if (asciiOnly && c >= 0x80)
                    {
                        return null;
                    }
                    if ((c & 0xfc00) == 0xd800 && index + 1 < valueSLength &&
                      (s[index + 1] & 0xfc00) == 0xdc00)
                    {
                        // Get the Unicode code point for the surrogate pair
                        c = 0x10000 + ((c & 0x3ff) << 10) + (s[index + 1] & 0x3ff);
                        ++index;
                    }
                    else if ((c & 0xf800) == 0xd800)
                    {
                        if (parseMode == URIUtility.ParseMode.IRISurrogateLenient)
                        {
                            c = 0xfffd;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    if (c == '%' && (state == 0 || state == 1) && strict)
                    {
                        // Percent encoded character (except in port)
                        if (index + 2 < valueSLength && IsHexChar(s[index + 1]) &&
                          IsHexChar(s[index + 2]))
                        {
                            index += 3;
                            continue;
                        }
                        return null;
                    }
                    if (state == 0)
                    { // User info
                        if (c == '/' || c == '?' || c == '#')
                        {
                            // not user info
                            state = 1;
                            index = authorityStart;
                            continue;
                        }
                        if (strict && c == '@')
                        {
                            // is user info
                            ++index;
                            state = 1;
                            continue;
                        }
                        if (strict && IsIUserInfoChar(c))
                        {
                            ++index;
                            if (index == valueSLength)
                            {
                                // not user info
                                state = 1;
                                index = authorityStart;
                                continue;
                            }
                        }
                        else
                        {
                            // not user info
                            state = 1;
                            index = authorityStart;
                            continue;
                        }
                    }
                    else if (state == 1)
                    { // host
                        if (c == '/' || c == '?' || c == '#')
                        {
                            // end of authority
                            retval[3] = index;
                            break;
                        }
                        if (!strict)
                        {
                            ++index;
                        }
                        else if (c == '[')
                        {
                            ++index;
                            index = ParseIPLiteral(s, index, valueSLength);
                            if (index < 0)
                            {
                                return null;
                            }
                            continue;
                        }
                        else if (c == ':')
                        {
                            // port
                            state = 2;
                            ++index;
                        }
                        else if (IsIRegNameChar(c))
                        {
                            // is valid host name char
                            // (note: IPv4 addresses included
                            // in ireg-name)
                            ++index;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else if (state == 2)
                    { // Port
                        if (c == '/' || c == '?' || c == '#')
                        {
                            // end of authority
                            retval[3] = index;
                            break;
                        }
                        if (c >= '0' && c <= '9')
                        {
                            ++index;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            var colon = false;
            var segment = false;
            bool fullyRelative = index == offset;
            retval[4] = index; // path offsets
            retval[5] = valueSLength;
            state = 0; // IRI Path
            while (index < valueSLength)
            {
                // Get the next Unicode character
                int c = s[index];
                if (asciiOnly && c >= 0x80)
                {
                    return null;
                }
                if ((c & 0xfc00) == 0xd800 && index + 1 < valueSLength &&
                  (s[index + 1] & 0xfc00) == 0xdc00)
                {
                    // Get the Unicode code point for the surrogate pair
                    c = 0x10000 + ((c & 0x3ff) << 10) + (s[index + 1] & 0x3ff);
                    ++index;
                }
                else if ((c & 0xf800) == 0xd800)
                {
                    // error
                    return null;
                }
                if (c == '%' && strict)
                {
                    // Percent encoded character
                    if (index + 2 < valueSLength && IsHexChar(s[index + 1]) &&
                      IsHexChar(s[index + 2]))
                    {
                        index += 3;
                        continue;
                    }
                    return null;
                }
                if (state == 0)
                { // Path
                    if (c == ':' && fullyRelative)
                    {
                        colon = true;
                    }
                    else if (c == '/' && fullyRelative && !segment)
                    {
                        // noscheme path can't have colon before slash
                        if (strict && colon)
                        {
                            return null;
                        }
                        segment = true;
                    }
                    if (c == '?')
                    {
                        retval[5] = index;
                        retval[6] = index + 1;
                        retval[7] = valueSLength;
                        state = 1; // move to query state
                    }
                    else if (c == '#')
                    {
                        retval[5] = index;
                        retval[8] = index + 1;
                        retval[9] = valueSLength;
                        state = 2; // move to fragment state
                    }
                    else if (strict && !IsIpchar(c))
                    {
                        return null;
                    }
                    ++index;
                }
                else if (state == 1)
                { // Query
                    if (c == '#')
                    {
                        retval[7] = index;
                        retval[8] = index + 1;
                        retval[9] = valueSLength;
                        state = 2; // move to fragment state
                    }
                    else if (strict && !IsIqueryChar(c))
                    {
                        return null;
                    }
                    ++index;
                }
                else if (state == 2)
                { // Fragment
                    if (strict && !IsIfragmentChar(c))
                    {
                        return null;
                    }
                    ++index;
                }
            }
            if (strict && fullyRelative && colon && !segment)
            {
                return null; // ex. "x@y:z"
            }
            return retval;
        }

        /// <summary>Parses an Internationalized Resource Identifier (IRI)
        /// reference under RFC3987. If the IRI is syntactically valid, splits
        /// the string into its components and returns an array containing the
        /// indices into the components.</summary>
        /// <param name='s'>A string representing an IRI. Can be null.</param>
        /// <param name='parseMode'>The parameter <paramref name='parseMode'/>
        /// is a ParseMode object.</param>
        /// <returns>If the string is a valid IRI reference, returns an array
        /// of 10 integers. Each of the five pairs corresponds to the start and
        /// end index of the IRI's scheme, authority, path, query, or fragment
        /// identifier, respectively. The scheme, authority, query, and
        /// fragment identifier, if present, will each be given without the
        /// ending colon, the starting "//", the starting "?", and the starting
        /// "#", respectively. If a component is absent, both indices in that
        /// pair will be -1. If the string is null or is not a valid IRI,
        /// returns null.</returns>
        public static int[] SplitIRI(string s, URIUtility.ParseMode parseMode)
        {
            return (s == null) ? null : SplitIRI(s, 0, s.Length, parseMode);
        }

        private static bool PathHasDotComponent(string path)
        {
            if (path == null || path.Length == 0)
            {
                return false;
            }
            path = PercentDecode(path);
            if (path.Equals("..", StringComparison.Ordinal))
            {
                return true;
            }
            if (path.Equals(".", StringComparison.Ordinal))
            {
                return true;
            }
            if (path.IndexOf(ValueSlashDot, StringComparison.Ordinal) < 0 &&
              path.IndexOf(
                ValueDotSlash,
                StringComparison.Ordinal) < 0)
            {
                return false;
            }
            var index = 0;
            var len = path.Length;
            while (index < len)
            {
                char c = path[index];
                if ((index + 3 <= len && c == '/' && path[index + 1] == '.' &&
                    path[index + 2] == '/') || (index + 2 == len && c == '.' &&
                    path[index + 1] == '.'))
                {
                    // begins with "/./" or is "..";
                    return true;
                }
                if (index + 3 <= len && c == '.' &&
                  path[index + 1] == '.' && path[index + 2] == '/')
                {
                    // begins with "../";
                    return true;
                }
                if ((index + 2 <= len && c == '.' &&
                    path[index + 1] == '/') || (index + 1 == len && c == '.'))
                {
                    // begins with "./" or is ".";
                    return true;
                }
                if (index + 2 == len && c == '/' && path[index + 1] == '.')
                {
                    // is "/."
                    return true;
                }
                if (index + 3 == len && c == '/' &&
                  path[index + 1] == '.' && path[index + 2] == '.')
                {
                    // is "/.."
                    return true;
                }
                if (index + 4 <= len && c == '/' && path[index + 1] == '.' &&
                  path[index + 2] == '.' && path[index + 3] == '/')
                {
                    // begins with "/../"
                    return true;
                }
                ++index;
                while (index < len)
                {
                    // Move the rest of the
                    // path segment until the next '/'
                    c = path[index];
                    if (c == '/')
                    {
                        break;
                    }
                    ++index;
                }
            }
            return false;
        }

        private static string UriPath(string uri, URIUtility.ParseMode parseMode)
        {
            int[] indexes = SplitIRI(uri, parseMode);
            return (
                indexes == null) ? null : uri.Substring(
                indexes[4],
                indexes[5] - indexes[4]);
        }

        /// <summary>Extracts the scheme, the authority, and the path component
        /// (up to and including the last "/" in the path if any) from the
        /// given URI or IRI, using the IRIStrict parse mode to check the URI
        /// or IRI. Any "./" or "../" in the path is not condensed.</summary>
        /// <param name='uref'>A text string representing a URI or IRI. Can be
        /// null.</param>
        /// <returns>The directory path of the URI or IRI. Returns null if
        /// <paramref name='uref'/> is null or not a valid URI or
        /// IRI.</returns>
        /// <exception cref='ArgumentNullException'>The parameter <paramref
        /// name='uref'/> is null.</exception>
        public static string DirectoryPath(string uref)
        {
            return DirectoryPath(uref, URIUtility.ParseMode.IRIStrict);
        }

        /// <summary>Extracts the scheme, the authority, and the path component
        /// (up to and including the last "/" in the path if any) from the
        /// given URI or IRI, using the given parse mode to check the URI or
        /// IRI. Any "./" or "../" in the path is not condensed.</summary>
        /// <param name='uref'>A text string representing a URI or IRI. Can be
        /// null.</param>
        /// <param name='parseMode'>The parse mode to use to check the URI or
        /// IRI.</param>
        /// <returns>The directory path of the URI or IRI. Returns null if
        /// <paramref name='uref'/> is null or not a valid URI or
        /// IRI.</returns>
        public static string DirectoryPath(string uref, URIUtility.ParseMode
          parseMode)
        {
            int[] indexes = SplitIRI(uref, parseMode);
            if (indexes == null)
            {
                return null;
            }
            if (uref == null)
            {
                throw new InvalidOperationException();
            }
            string schemeAndAuthority = uref.Substring(0, indexes[4]);
            string path = uref.Substring(indexes[4], indexes[5] - indexes[4]);
            if (path.Length > 0)
            {
                for (int i = path.Length - 1; i >= 0; --i)
                {
                    if (path[i] == '/')
                    {
                        return schemeAndAuthority + path.Substring(0, i + 1);
                    }
                }
                return schemeAndAuthority + path;
            }
            else
            {
                return schemeAndAuthority;
            }
        }

        /// <summary>Resolves a URI or IRI relative to another URI or IRI, but
        /// only if the resolved URI has no "." or ".." component in its path
        /// and only if resolved URI's directory path matches that of the
        /// second URI or IRI.</summary>
        /// <param name='refValue'>A string representing a URI or IRI
        /// reference. Example: <c>dir/file.txt</c>.</param>
        /// <param name='absoluteBase'>A string representing an absolute URI
        /// reference. Example: <c>http://example.com/my/path/</c>.</param>
        /// <returns>The resolved IRI, or null if <paramref name='refValue'/>
        /// is null or is not a valid IRI, or <paramref name='refValue'/> if
        /// <paramref name='absoluteBase'/> is null or an empty string, or null
        /// if <paramref name='absoluteBase'/> is neither null nor empty and is
        /// not a valid IRI. Returns null instead if the resolved IRI has no
        /// "." or ".." component in its path or if the resolved URI's
        /// directory path does not match that of <paramref
        /// name='absoluteBase'/>. Example:
        /// <c>http://example.com/my/path/dir/file.txt</c>.</returns>
        public static string RelativeResolveWithinBaseURI(
          string refValue,
          string absoluteBase)
        {
            if (!String.IsNullOrEmpty(absoluteBase) &&
              SplitIRI(absoluteBase, URIUtility.ParseMode.IRIStrict) == null)
            {
                return null;
            }
            string rel = RelativeResolve(refValue, absoluteBase);
            if (rel == null)
            {
                return null;
            }
            if (refValue == null)
            {
                throw new InvalidOperationException();
            }
            string relpath = UriPath(refValue, URIUtility.ParseMode.IRIStrict);
            if (PathHasDotComponent(relpath))
            {
                // Resolved path has a dot component in it (usually
                // because that component is percent-encoded)
                return null;
            }
            string absuri = DirectoryPath(absoluteBase);
            string reluri = DirectoryPath(rel);
            return (absuri == null || reluri == null ||
                !absuri.Equals(reluri, StringComparison.Ordinal)) ? null : rel;
        }
    }

    #endregion
}
