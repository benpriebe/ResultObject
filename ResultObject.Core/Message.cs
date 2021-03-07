using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;

namespace ResultObject.Core
{
    public class Message
    {
        internal Message(){}
        
        public Message(
            MessageType type, 
            Type resxFile, 
            string resxKey, 
            object tokens = null) : this(type, resxFile, resxKey, Thread.CurrentThread.CurrentUICulture.Name, tokens)
        {
        }

        public Message(
            MessageType type, 
            Type resxFile, 
            string resxKey, 
            string locale,
            object tokens = null)
        {
            using (EphemeralUiCulture.ChangeLocale(locale))
            {
                Type = type;
                Code = resxKey.ToKebabCase();
                Tokens = tokens;

                var resourceManager = GetResourceManager(resxFile);

                // localised content
                LanguageCode = locale;

                var localizedTemplate = resourceManager.GetString(resxKey);
                if (string.IsNullOrWhiteSpace(localizedTemplate))
                {
                    Content = InvariantContent =
                        $"Missing resource \"{resxKey}\" from resource file: \"{resxFile.FullName}\"";
                    return;
                }

                Template = LowerCamelCaseTokens(localizedTemplate);
                Content = Format(localizedTemplate, tokens);

                // invariant content
                var englishTemplate = resourceManager.GetString(resxKey, CultureInfo.CreateSpecificCulture("en-US"));
                InvariantContent = Format(englishTemplate, tokens);
            }
        }

        /// <summary>
        ///     The type of message.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MessageType? Type { get; internal set; }

        /// <summary>
        ///     A descriptive code that can be used for lookup purposes allowing clients to maintain their own translations.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Code { get; internal set; }

        /// <summary>
        ///     Templated message with token place holders.
        /// </summary>
        /// <example>
        ///     "The first name - {firstName} - must contain - {min} - characters or more."
        /// </example>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Template { get; internal set; }

        /// <summary>
        ///     JSON compatible object.
        /// </summary>
        /// <example>
        /// { firstName: 'delilah', min: 10 }
        /// </example>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object Tokens { get; internal set;}

        /// <summary>
        ///     The language code used for the message content.
        /// </summary>
        /// <example>
        ///     en-US, en-AU, fr, vi, zh
        /// </example>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string LanguageCode { get; internal set; }

        /// <summary>
        ///     The translated tokenized template.
        /// </summary>
        /// <example>
        ///     "The first name - Ben - must contain - 8 - characters or more."
        /// </example>
        public string Content { get; internal set; }

        /// <summary>
        ///     The US English tokenized template.
        /// </summary>
        /// <example>
        ///     "The first name - Ben - must contain - 8 - characters or more."
        /// </example>
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public string InvariantContent { get; internal set; }

        private static ResourceManager GetResourceManager(Type resxFile)
        {
            var resourceManagerProperty =
                resxFile.GetProperty("ResourceManager", BindingFlags.Static | BindingFlags.NonPublic) ??
                resxFile.GetProperty("ResourceManager", BindingFlags.Static | BindingFlags.Public);

            return (ResourceManager) resourceManagerProperty.GetValue(resxFile);
        }

        /// <summary>
        ///     Converts a named token template string into an indexed based token string that can be evaluated at runtime.
        /// </summary>
        /// <param name="template">the named token string</param>
        /// <param name="tokens">the token object</param>
        /// <returns>the interpolated string</returns>
        internal static string Format(string template, object tokens)
        {
            if (tokens == null) return template;
            var propertyInfos = tokens.GetType().GetProperties();

            var tokenValues = new List<object>();
            var message = propertyInfos.Aggregate(template, (memo, propertyInfo) =>
            {
                var tokenKey = propertyInfo.Name;
                var tokenValue = propertyInfo.GetValue(tokens);
                tokenValues.Add(tokenValue);

                var pattern = $@"{{(?<Token>\s*?{tokenKey}\s*?)(?<Format>:[^}}]+)?}}";
                var replacement = Regex.Replace(memo, pattern, match =>
                {
                    var format = match.Groups["Format"].Value;
                    return $"{{{tokenValues.Count - 1}{format}}}";
                }, RegexOptions.IgnoreCase | RegexOptions.Multiline);

                return replacement;
            });

            // ensure that all named tokens in the template have a token value provided.
            var untokenizedTokens = Regex.Matches(message, "(?<!{){[A-Z][^{}]+?}(?!})", RegexOptions.IgnoreCase).ToArray();
            if (untokenizedTokens.Any())
            {
                var missingTokens = string.Join(", ", untokenizedTokens.Select(match => match.Value));
                var error =
                    $"The message template references tokens that have not been supplied.{Environment.NewLine}template: {template}{Environment.NewLine}missing token values: {missingTokens}";

                // todo: get the environment from somewhere.
                // var environment = "Production";
                //
                // if (!string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase))
                //     throw new ArgumentOutOfRangeException(error);

                return template;
            }

            message = string.Format(message, tokenValues.ToArray());
            return message;
        }

        private static string LowerCamelCaseTokens(string template)
        {
            return Regex.Replace(template, "(?<!{){(?<Token>[^{}]+?)}(?!})", match =>
            {
                var token = match.Groups["Token"].Value;
                return $"{{{char.ToLowerInvariant(token[0])}{token.Substring(1)}}}";
            });
        }

        
        public override string ToString()
        {
            return InvariantContent;
        }

        #region Creation Methods

        public static Message ValidationError<TResxFile>(string resxKey, object tokens = null)
        {
            return new(MessageType.ValidationError, typeof(TResxFile), resxKey, tokens);
        }

        public static Message Error<TResxFile>(string resxKey, object tokens = null)
        {
            return new(MessageType.Error, typeof(TResxFile), resxKey, tokens);
        }

        public static Message Warning<TResxFile>(string resxKey, object tokens = null)
        {
            return new(MessageType.Warning, typeof(TResxFile), resxKey, tokens);
        }

        public static Message Info<TResxFile>(string resxKey, object tokens = null)
        {
            return new(MessageType.Information, typeof(TResxFile), resxKey, tokens);
        }

        public static Message Unauthorized<TResxFile>(string resxKey, object tokens = null)
        {
            return new(MessageType.Unauthorized, typeof(TResxFile), resxKey, tokens);
        }

        public static Message Forbidden<TResxFile>(string resxKey, object tokens = null)
        {
            return new(MessageType.Forbidden, typeof(TResxFile), resxKey, tokens);
        }

        public static Message Unauthorized()
        {
            return new(
                MessageType.Unauthorized,
                typeof(i18n.Message),
                nameof(i18n.Message.Unauthorized)
            );
        }

        public static Message Forbidden()
        {
            return new(
                MessageType.Forbidden,
                typeof(i18n.Message),
                nameof(i18n.Message.Forbidden)
            );
        }

        public static Message NotFound<TEntity>(object identity)
        {
            var sourceType = typeof(TEntity);
            if (typeof(ResultBase).IsAssignableFrom(sourceType) && sourceType.IsGenericType)
                sourceType = sourceType.GenericTypeArguments[0];
            return new Message(
                MessageType.NotFound,
                typeof(i18n.Message),
                nameof(i18n.Message.NotFound),
                new
                {
                    type = sourceType.Name,
                    id = Convert.ToString(identity)
                });
        }

        #endregion
    }
}