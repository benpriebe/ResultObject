using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;

namespace ResultObject.Core
{
    #region Result

    public class Result : ResultBase
    {
        public static readonly AsyncLocal<ResultMessageLevelOptions> MessageLevelOptions = new();
        
        /// <summary>
        /// Creates a success <see cref="Result"/> with no content.
        /// </summary>
        public static ResultBuilder Success()
        {
            var result = new Result { IsSuccess = true, HasContent = false };
            return new ResultBuilder(result);
        }

        /// <summary>
        /// Creates a failed <see cref="Result"/> with no content.
        /// </summary>
        public static FailureResultBuilder Failure()
        {
            return new FailureResultBuilder();
        }

        /// <summary>
        /// Creates a success <see cref="Result{TEntity}"/> with no content.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static SuccessResultBuilder<Result<TEntity>> Success<TEntity>()
        {
            return new SuccessResultBuilder<Result<TEntity>>();
        }

        /// <summary>
        /// Creates a success <see cref="Result{TEntity}"/> with content.
        /// </summary>
        /// <remarks>
        /// Event if the entity value passed is null or default{TEntity} the .HasContent property value is still true.
        /// </remarks>
        public static SuccessResultBuilder<Result<TEntity>> Success<TEntity>(TEntity entity)
        {
            return new SuccessResultBuilder<Result<TEntity>>(new Result<TEntity> { IsSuccess = true, HasContent = true, Value = entity });
        }

        /// <summary>
        /// Creates a failed <see cref="Result{TEntity}"/> with no content.
        /// </summary>
        public static FailureResultBuilder<Result<TEntity>> Failure<TEntity>()
        {
            return new FailureResultBuilder<Result<TEntity>>();
        }
    }

    public class ResultBuilder
    {
        protected readonly Result Result;

        public ResultBuilder(Result result)
        {
            Result = result;
        }

        public ResultBuilder WithInfo<TResxFile>(string resxKey, object tokens = null)
        {
            Result.Messages = Result.Messages.Concat(new[] { Message.Info<TResxFile>(resxKey, tokens) }).ToArray();
            return this;
        }

        public ResultBuilder WithWarning<TResxFile>(string resxKey, object tokens = null)
        {
            Result.Messages = Result.Messages.Concat(new[] { Message.Warning<TResxFile>(resxKey, tokens) }).ToArray();
            return this;
        }

        public ResultBuilder WithValidationError<TResxFile>(string resxKey, object tokens = null)
        {
            Result.Messages = Result.Messages.Concat(new[] { Message.ValidationError<TResxFile>(resxKey, tokens) }).ToArray();
            return this;
        }

        public ResultBuilder WithMessages(params Message[] messages)
        {
            Result.Messages = Result.Messages.Concat(messages).ToArray();
            return this;
        }

        public ResultBuilder WithMessages(IEnumerable<Message> messages)
        {
            return WithMessages(messages.ToArray());
        }

        public ResultBuilder WithLogMessage(string logMessage)
        {
            return WithLogMessages(logMessage);
        }

        public ResultBuilder WithLogMessages(IEnumerable<string> logMessages)
        {
            return WithLogMessages(logMessages.ToArray());
        }

        public ResultBuilder WithLogMessages(params string[] logMessages)
        {
            Result.LogMessages = Result.LogMessages.Concat(logMessages).ToArray();
            return this;
        }

        public static implicit operator Result(ResultBuilder builder)
        {
            return builder.Result;
        }
    }

    public class FailureResultBuilder : ResultBuilder
    {
        private readonly ResultBuilder resultBuilder;

        public FailureResultBuilder() : base(new Result { IsSuccess = false, HasContent = false })
        {
            resultBuilder = new ResultBuilder(Result);
        }

        public ResultBuilder NotFound<TEntity>(object identity)
        {
            Result.Messages = Result.Messages.Concat(new[] { Message.NotFound<TEntity>(identity) }).ToArray();
            return resultBuilder;
        }

        public ResultBuilder Unauthorized()
        {
            Result.Messages = Result.Messages.Concat(new[] { Message.Unauthorized() }).ToArray();
            return this;
        }

        public ResultBuilder Unauthorized<TResxFile>(string resxKey, object tokens = null)
        {
            Result.Messages = Result.Messages.Concat(new[] { Message.Unauthorized<TResxFile>(resxKey, tokens) }).ToArray();
            return this;
        }

        public ResultBuilder Forbidden()
        {
            Result.Messages = Result.Messages.Concat(new[] { Message.Forbidden() }).ToArray();
            return this;
        }

        public ResultBuilder Forbidden<TResxFile>(string resxKey, object tokens = null)
        {
            Result.Messages = Result.Messages.Concat(new[] { Message.Forbidden<TResxFile>(resxKey, tokens) }).ToArray();
            return this;
        }

        public ResultBuilder WithError<TResxFile>(string resxKey, object tokens = null)
        {
            Result.Messages = Result.Messages.Concat(new[] { Message.Error<TResxFile>(resxKey, tokens) }).ToArray();
            return this;
        }

        public ResultBuilder WithValidator(Validator validator, ErrorMode errorMode = ErrorMode.AllErrors)
        {
            Result.Messages = Result.Messages.Concat(errorMode == ErrorMode.FirstError ? new List<Message> { validator.Errors.FirstOrDefault() } : validator.Errors).ToArray();
            return this;
        }

        public FailureResultBuilder AttemptRetry()
        {
            Result.AttemptRetry = true;
            return this;
        }

        public static implicit operator Result(FailureResultBuilder builder)
        {
            return builder.Result;
        }
    }

    #endregion Result

    #region Result<TEntity>

    public class Result<TEntity> : ResultBase
    {
        private readonly TEntity value;

        [JsonPropertyName("data")]
        public TEntity Value
        {
            get => value;
            init
            {
                if (!HasContent && !Equals(value, default(TEntity)))
                {
                    throw new ArgumentException("You cannot set a value on the result object when HasContent is false.");
                }

                this.value = value;
            }
        }
    }

    #endregion

    public enum ErrorMode
    {
        FirstError,
        AllErrors
    }

}