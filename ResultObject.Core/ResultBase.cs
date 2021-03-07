using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

[assembly: InternalsVisibleTo("ResultObject.Core.Http")]
[assembly: InternalsVisibleTo("ResultObject.Core.Tests")]
namespace ResultObject.Core
{
    public abstract class ResultBase
    {
        protected ResultBase()
        {
            Messages = Array.Empty<Message>();
            LogMessages = Array.Empty<string>();
            AttemptRetry = false;
        }

        [JsonIgnore]
        public bool IsSuccess { get; internal set; }

        [JsonIgnore]
        public bool HasContent { get; internal set; }

        [JsonIgnore]
        public bool AttemptRetry { get; internal set; }

        [JsonIgnore]
        internal string[] LogMessages { get; set; }

        public Message[] Messages { get; internal set; }

        /// <summary>
        /// Does the result have any messages of the <see cref="MessageType.Unauthorized" />?
        /// </summary>
        [JsonIgnore]
        public bool IsUnauthorized => HasMessageWithType(MessageType.Unauthorized);
        
        /// <summary>
        /// Does the result have any messages of the <see cref="MessageType.Forbidden" />?
        /// </summary>
        [JsonIgnore]
        public bool IsForbidden => HasMessageWithType(MessageType.Forbidden);

        /// <summary>
        /// Does the result have any messages of the <see cref="MessageType.NotFound" />?
        /// </summary>
        [JsonIgnore]
        public bool IsNotFound => HasMessageWithType(MessageType.NotFound);

        /// <summary>
        /// Does the result have any messages of the <see cref="MessageType.Information" />?
        /// </summary>
        [JsonIgnore]
        public bool HasInformationMessages => HasMessageWithType(MessageType.Information);

        /// <summary>
        /// Get all <see cref="MessageType.Information"/> Messages on the result.
        /// </summary>
        [JsonIgnore]
        public Message[] InformationMessages => GetMessages(MessageType.Information);
        
        /// <summary>
        /// Does the result have any messages of the <see cref="MessageType.Warning" />?
        /// </summary>
        [JsonIgnore]
        public bool HasWarnings => HasMessageWithType(MessageType.Warning);

        /// <summary>
        /// Get all <see cref="MessageType.Warning"/> Messages on the result.
        /// </summary>
        [JsonIgnore]
        public Message[] WarningMessages => GetMessages(MessageType.Warning);

        /// <summary>
        /// Does the result have any messages of the <see cref="MessageType.Error" />?
        /// </summary>
        [JsonIgnore]
        public bool HasErrors => HasMessageWithType(MessageType.Error);

        /// <summary>
        /// Get all <see cref="MessageType.Error"/> Messages on the result.
        /// </summary>
        [JsonIgnore]
        public Message[] ErrorMessages => GetMessages(MessageType.Error);

        /// <summary>
        /// Does the result have any messages of the <see cref="MessageType.ValidationError" />?
        /// </summary>
        [JsonIgnore]
        public bool HasValidationErrors => HasMessageWithType(MessageType.ValidationError);

        /// <summary>
        /// Get all <see cref="MessageType.ValidationError"/> Messages on the result.
        /// </summary>
        [JsonIgnore]
        public Message[] ValidationErrors => GetMessages(MessageType.ValidationError);
        
        /// <summary>
        /// Formats both the <see cref="LogMessages" /> and the untranslated <see cref="Messages" /> as a single string,
        /// using the nominated delimiter to separate each message. If no delimiter is provided,
        /// <see cref="Environment.NewLine" /> will be used.
        /// </summary>
        /// <param name="delimiter">The delimiter to be used to separate each message, or <c>null</c> to use
        /// <see cref="Environment.NewLine" />.</param>
        /// <returns>Message string</returns>
        public string GetInvariantMessages(string delimiter = null)
        {
            var invariantMessages = 
                from message in Messages
                select $"{message.Type}({message.Code}): {message.InvariantContent}";

            return string.Join(delimiter ?? Environment.NewLine, invariantMessages.Union(LogMessages));
        }

        private bool HasMessageWithType(MessageType messageType)
        {
            return Messages != null && Messages.Any(message => message.Type.Equals(messageType));
        }

        private Message[] GetMessages(MessageType messageType)
        {
            return Messages?.Where(message => message.Type.Equals(MessageType.Information)).ToArray();
        }

    }
}