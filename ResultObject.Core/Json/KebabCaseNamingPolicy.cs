using System.Text.Json;

namespace ResultObject.Core.Json
{
    public class KebabCaseNamingPolicy : JsonNamingPolicy
    {
        public static KebabCaseNamingPolicy Instance { get; } = new KebabCaseNamingPolicy();

        public override string ConvertName(string name)
        {
            return name.ToKebabCase();
        }
    }
}