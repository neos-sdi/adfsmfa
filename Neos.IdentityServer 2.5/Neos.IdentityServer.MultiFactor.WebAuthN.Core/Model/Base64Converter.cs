using System;
using Newtonsoft.Json;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    /// <summary>
    /// Custom Converter for encoding/encoding byte[] using Base64Url instead of default Base64.
    /// </summary>
   // public class Base64UrlConverter : JsonConverter<byte[]>
    public class Base64UrlConverter : JsonConverter
    {
        private readonly Required _requirement = Required.DisallowNull;

        public Base64UrlConverter()
        {
        }

        public Base64UrlConverter(Required required = Required.DisallowNull)
        {
            _requirement = required;
        }

//        public override void WriteJson(JsonWriter writer, byte[] value, JsonSerializer serializer)
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(Base64Url.Encode(value as byte[]));
        }

//        public override byte[] ReadJson(JsonReader reader, Type objectType, byte[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            byte[] ret = null;

            if (null == reader.Value && _requirement == Required.AllowNull)
                return ret as object;

            if (null == reader.Value)
                throw new VerificationException("json value must not be null");
            if (Type.GetType("System.String") != reader.ValueType)
                throw new VerificationException("json valuetype must be string");
            try
            {
                ret = Base64Url.Decode((string)reader.Value);
            }
            catch (FormatException ex)
            {
                throw new VerificationException("json value must be valid base64 encoded string", ex);
            }
            return ret as object;
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}
