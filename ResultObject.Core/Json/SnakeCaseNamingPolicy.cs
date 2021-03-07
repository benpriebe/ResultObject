using System.Text.Json;

namespace ResultObject.Core.Json
{
    public class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public static SnakeCaseNamingPolicy Instance { get; } = new SnakeCaseNamingPolicy();

        public override string ConvertName(string name)
        {
            return name.ToSnakeCase();
        }
    }
}