using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ResultObject.Core.Json
{
    public class KebabCaseEnumConverter : JsonConverterFactory
    {
        private readonly bool allowIntegerValues;
        private readonly JsonStringEnumConverter baseConverter;

        public KebabCaseEnumConverter() : this(true) { }

        public KebabCaseEnumConverter(bool allowIntegerValues = true)
        {
            this.allowIntegerValues = allowIntegerValues;
            this.baseConverter = new JsonStringEnumConverter(null, allowIntegerValues);
        }
    
        public override bool CanConvert(Type typeToConvert) => baseConverter.CanConvert(typeToConvert);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return new JsonStringEnumConverter(new KebabCaseNamingPolicy(), allowIntegerValues).CreateConverter(typeToConvert, options);
        }
    }
}