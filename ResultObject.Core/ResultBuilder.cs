using System.Collections.Generic;
using System.Linq;

namespace ResultObject.Core
{
    public abstract class ResultBuilder<TResult> where TResult : ResultBase
    {
        protected readonly TResult Result;

        protected ResultBuilder(TResult result)
        {
            Result = result;
        }

        public ResultBuilder<TResult> WithInfo<TResxFile>(string resxKey, object tokens = null)
        {
            Result.Messages = Result.Messages.Concat(new[] { Message.Info<TResxFile>(resxKey, tokens) }).ToArray();
            return this;
        }

        public ResultBuilder<TResult> WithWarning<TResxFile>(string resxKey, object tokens = null)
        {
            Result.Messages = Result.Messages.Concat(new[] { Message.Warning<TResxFile>(resxKey, tokens) }).ToArray();
            return this;
        }

        public ResultBuilder<TResult> WithValidationError<TResxFile>(string resxKey, object tokens = null)
        {
            Result.Messages = Result.Messages.Concat(new[] { Message.ValidationError<TResxFile>(resxKey, tokens) }).ToArray();
            return this;
        }

        public ResultBuilder<TResult> WithMessages(params Message[] messages)
        {
            Result.Messages = Result.Messages.Concat(messages).ToArray();
            return this;
        }

        public ResultBuilder<TResult> WithMessages(IEnumerable<Message> messages)
        {
            return WithMessages(messages.ToArray());
        }

        public ResultBuilder<TResult> WithLogMessage(string logMessage)
        {
            return WithLogMessages(logMessage);
        }

        public ResultBuilder<TResult> WithLogMessages(IEnumerable<string> logMessages)
        {
            return WithLogMessages(logMessages.ToArray());
        }

        public ResultBuilder<TResult> WithLogMessages(params string[] logMessages)
        {
            Result.LogMessages = Result.LogMessages.Concat(logMessages).ToArray();
            return this;
        }

        public static implicit operator TResult(ResultBuilder<TResult> builder)
        {
            return builder.Result;
        }
    }

    public class SuccessResultBuilder<TResult> : ResultBuilder<TResult> where TResult : ResultBase, new()
    {
        public SuccessResultBuilder() : base(new TResult { IsSuccess = true, HasContent = false })
        {
        }

        public SuccessResultBuilder(TResult entity) : base(entity)
        {
        }
    }

    public class FailureResultBuilder<TResult> : ResultBuilder<TResult> where TResult : ResultBase, new()
    {
        public FailureResultBuilder() : base(new TResult { IsSuccess = false, HasContent = false })
        {
        }

        public ResultBuilder<TResult> NotFound(object identity)
        {
            Result.Messages = Result.Messages.Concat(new[] { Message.NotFound<TResult>(identity) }).ToArray();
            return this;
        }

        public ResultBuilder<TResult> Unauthorized()
        {
            Result.Messages = Result.Messages.Concat(new[] { Message.Unauthorized() }).ToArray();
            return this;
        }

        public ResultBuilder<TResult> Unauthorized<TResxFile>(string resxKey, object tokens = null)
        {
            Result.Messages = Result.Messages.Concat(new[] { Message.Unauthorized<TResxFile>(resxKey, tokens) }).ToArray();
            return this;
        }

        public ResultBuilder<TResult> Forbidden()
        {
            Result.Messages = Result.Messages.Concat(new[] { Message.Forbidden() }).ToArray();
            return this;
        }

        public ResultBuilder<TResult> Forbidden<TResxFile>(string resxKey, object tokens = null)
        {
            Result.Messages = Result.Messages.Concat(new[] { Message.Forbidden<TResxFile>(resxKey, tokens) }).ToArray();
            return this;
        }

        public ResultBuilder<TResult> WithError<TResxFile>(string resxKey, object tokens = null)
        {
            Result.Messages = Result.Messages.Concat(new[] { Message.Error<TResxFile>(resxKey, tokens) }).ToArray();
            return this;
        }

        public ResultBuilder<TResult> WithValidator(Validator validator, ErrorMode errorMode = ErrorMode.AllErrors)
        {
            Result.Messages = Result.Messages.Concat(errorMode == ErrorMode.FirstError ? new List<Message> { validator.Errors.FirstOrDefault() } : validator.Errors).ToArray();
            return this;
        }

        public ResultBuilder<TResult> AttemptRetry()
        {
            Result.AttemptRetry = true;
            return this;
        }

        public static implicit operator TResult(FailureResultBuilder<TResult> builder)
        {
            return builder.Result;
        }
    }
}