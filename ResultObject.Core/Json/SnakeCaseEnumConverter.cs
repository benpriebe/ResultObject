using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ResultObject.Core.Json
{
    public class SnakeCaseEnumConverter : JsonConverterFactory
    {
        private readonly bool allowIntegerValues;
        private readonly JsonStringEnumConverter baseConverter;

        public SnakeCaseEnumConverter() : this(true) { }

        public SnakeCaseEnumConverter(bool allowIntegerValues = true)
        {
            this.allowIntegerValues = allowIntegerValues;
            this.baseConverter = new JsonStringEnumConverter(null, allowIntegerValues);
        }
    
        public override bool CanConvert(Type typeToConvert) => baseConverter.CanConvert(typeToConvert);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return new JsonStringEnumConverter(new SnakeCaseNamingPolicy(), allowIntegerValues).CreateConverter(typeToConvert, options);
        }
    }
}