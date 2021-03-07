namespace ResultObject.Core
{
    /// <summary>
    /// Determines which properties of message output are serialized.
    /// </summary>
    public class ResultMessageLevelOptions
    {
        public const string ResultMessageLevel = "ResultMessageLevel";
        public bool Code { get; set; }
        public bool Tokens { get; set; }
        public bool Template { get; set; }
        public bool LanguageCode { get; set; }
    }
}