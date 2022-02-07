//******************************************************************************************************************************************************************************************//
// Copyright (c) 2021 abergs (https://github.com/abergs/fido2-net-lib)                                                                                                                      //                        
//                                                                                                                                                                                          //
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),                                       //
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,   //
// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:                                                                                   //
//                                                                                                                                                                                          //
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.                                                           //
//                                                                                                                                                                                          //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,                                      //
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,                            //
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                               //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
// Copyright (c) 2022 @redhook62 (adfsmfa@gmail.com)                                                                                                                                    //                        
//                                                                                                                                                                                          //
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),                                       //
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,   //
// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:                                                                                   //
//                                                                                                                                                                                          //
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.                                                           //
//                                                                                                                                                                                          //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,                                      //
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,                            //
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                               //
//                                                                                                                                                                                          //
//                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    public static class EnumExtensions
    {
#if Csharp73
        /// <summary>
        /// Gets the enum value from EnumMemberAttribute's value.
        /// </summary>
        /// <typeparam name="TEnum">The type of enum.</typeparam>
        /// <param name="value">The EnumMemberAttribute's value.</param>
        /// <param name="ignoreCase">ignores the case when comparing values.</param>
        /// <returns>TEnum.</returns>
        /// <exception cref="System.ArgumentException">No XmlEnumAttribute code exists for type " + typeof(TEnum).ToString() + " corresponding to value of " + value</exception>
        public static TEnum ToEnum<TEnum>(this string value, bool ignoreCase = true) where TEnum : struct, Enum
        {
            // Try to parse it normally on the first try
            if (Enum.TryParse<TEnum>(value, ignoreCase, out var result))
                return result;

            // Try with value from EnumMemberAttribute
            var values = Enum.GetValues(typeof(TEnum)).OfType<TEnum>().ToArray();
            foreach (var val in values)
            {
                if (ToEnumMemberValue(val).Equals(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                    return val;
            }

            throw new ArgumentException($"Value '{value}' is not a valid enum name of '{typeof(TEnum)}' ({nameof(ignoreCase)}={ignoreCase}). Valid values are: {string.Join(", ", values.Select(v => v.ToEnumMemberValue()))}.");
        }

        /// <summary>
        /// Gets the EnumMemberAttribute's value from the enum's value.
        /// </summary>
        /// <typeparam name="TEnum">The type of enum.</typeparam>
        /// <param name="value">The enum's value.</param>
        /// <returns>string.</returns>
        public static string ToEnumMemberValue<TEnum>(this TEnum value) where TEnum : struct, Enum
        {
            return typeof(TEnum).GetTypeInfo()
                                .DeclaredMembers
                                .SingleOrDefault(x => x.Name == value.ToString())
                                ?.GetCustomAttribute<EnumMemberAttribute>(false)
                                ?.Value;
        }
#endif
        /// <summary>
        /// Gets the enum value from EnumMemberAttribute's value.
        /// </summary>
        /// <typeparam name="TEnum">The type of enum.</typeparam>
        /// <param name="value">The EnumMemberAttribute's value.</param>
        /// <param name="ignoreCase">ignores the case when comparing values.</param>
        /// <returns>TEnum.</returns>
        /// <exception cref="System.ArgumentException">No XmlEnumAttribute code exists for type " + typeof(TEnum).ToString() + " corresponding to value of " + value</exception>
        public static TEnum ToEnum<TEnum>(this string value, bool ignoreCase = true) where TEnum : struct, IConvertible
        {
            // Try to parse it normally on the first try
            if (Enum.TryParse<TEnum>(value, ignoreCase, out var result))
                return result;

            // Try with value from EnumMemberAttribute
            var values = Enum.GetValues(typeof(TEnum)).OfType<TEnum>().ToArray();

            foreach (var val in values)
            {
                if (ToEnumMemberValue(val).Equals(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                    return val;
            }

            throw new ArgumentException("No EnumMemberAttribute code exists for type " + typeof(TEnum).ToString() + " corresponding to value of " + value);
        }

        /// <summary>
        /// Gets the EnumMemberAttribute's value from the enum's value.
        /// </summary>
        /// <typeparam name="TEnum">The type of enum.</typeparam>
        /// <param name="value">The enum's value.</param>
        /// <returns>string.</returns>
        public static string ToEnumMemberValue<TEnum>(this TEnum value) where TEnum : struct, IConvertible
        {
            return typeof(TEnum)
                .GetTypeInfo()
                .DeclaredMembers
                .SingleOrDefault(x => x.Name == value.ToString())
                ?.GetCustomAttribute<EnumMemberAttribute>(false)
                ?.Value;
        }
    }
}

