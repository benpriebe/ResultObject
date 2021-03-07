# ResultObject

**ResultObject is a lightweight .NET Core library developed to solve a common problem. It returns an object indicating success or failure of an operation instead of throwing/using exceptions.**

## Key Features

- **Generalised container** which works in all contexts (ASP.NET MVC/WebApi, DDD Domain Model, etc)
- Store **multiple messages (info, warnings, errors, validation errors)** in one Result object
- Create **localised, tokenised messages** instead of only messages in string format
- Comes with fluent validator to validate most primitive types and generate localised messages  
- Use Http extensions to map `Result` objects to Http `IActionResults`
- Serialise localised messages with configurable metadata to allow clients to provide custom localised messages

## Why Results instead of exceptions

The pattern - returning a Result object indicating success or failure - is not a new idea. 
This pattern comes from functional programming languages. 
With ResultObject this pattern can also be applied in .NET/C#.

The article [Exceptions for Flow Control by Vladimir Khorikov](https://enterprisecraftsmanship.com/posts/exceptions-for-flow-control/)
describes which scenarios the Result pattern makes sense and which scenarios do not. 

See the [list of resources](https://github.com/benpriebe/ResultObject#interesting-resources-about-result-pattern) to learn more about the Result Pattern.

## Creating a Result

A Result can bundle/chain together one or more different messages to
give context to the success or failure of the result.

```csharp
// create a result which indicates success
Result successResult1 = Result.Success();

Result successResult2 = Result.Success()
                              .WithInfo<Resx>(nameof(Resx.MsgKey), new { tokenName = "tokenValue"});
```

```csharp
// create a result which indicates failure
Result errorResult1 = Result.Failure();

Result errorResult2 = Result.Failure()
                            .WithValidtionError<Resx>(nameof(Resx.MsgValidationKey));
                            .WithError<Resx>(nameof(Resx.MsgKey), new { tokenName = "tokenValue"});

Result isUnauthorised = Result.Failure().IsUnauthorised();
Result foridden = Result.Failure().Forbidden();
                            
```

The class `Result` is typically used by void methods which have no return value.

```csharp
public Result PerformOperation()
{
    if (this.InABadWay) {
        return Result.Failure();
    }

    // rest of the logic

    return Result.Success();
}
```

Additionally, the class `Result<T>` allows a value to returned.

```csharp
// create a result which indicates success
Result<int> successResult1 = Result.Success(42);
Result<MyCustomObject> successResult2 = Result.Success(new MyCustomObject());

// create a result which indicates failure
Result<int> errorResult1 = Result.Failure<int>.WithError<Resx>(nameof(Resx.MsgKeyInfo));
Result<int> errorResult2 = Result.Failure<int>.NotFound(objectId);
Result<int> errorResult3 = Result.Failure<int>.Forbidden();
Result<int> errorResult4 = Result.Failure<int>.Unauthorised();
```

## Handling/Processing a Result

After you get a Result object from a method you have to process it. 
This means, you have to check if the operation was completed successfully (`Result.IsSuccess`) or not (`!Result.IsSuccess`)

The value of a `Result<T>` can be accessed via the property `Value`.

```csharp
public bool PerformOperation()
{
    if (!result.IsSuccess) {
        logger.error(result.GetInvariantMessages());
        return false;
    }

    // rest of the logic

    return true;
}
```

The `result` instance may come with a collection or one or more messages.

```csharp
Result<int> result = DoSomething();
     
// get all messages (localised)
var messages = result.Messages;

// get all info messages (localised)
var errors = result.InformationMessages;

// get all warning messages (localised)
var errors = result.WarningMessages;

// get all error messages (localised)
var errors = result.ErrorMessages;

// get all validation messages (localised)
var errors = result.ValidationErrors;

// get all messages in default language invariant (good for logging)
var errors = result.GetInvariantMessages();
```

On top of the `info`, `warn`, `error` and `validation-error` messages there are three more types
that represent three typical scenarios found in most projects. These three scenarios are all types of a `Result.Failure` and
are convenience methods to determine if there is at least one message of that type.

```csharp
if (result.IsUnauthorised) { ... }

if (result.IsForbidden) { ... }

if (result.IsNotFound) { ... }
```

## Further features

### Create a result with log messages.

Not all messages returned on a `Result` need to be localised. A typical use case is to add messages for logging.

```csharp
var result = Result.Failure().WithLogMessage("Non localised message to be logged.");
```

### Converting to Http IActionResult

The `Result` and `Result<T>` objects don't have dependencies on any other framework/library. 
However, you may want to convert your `Result` objects for serialization in a web api.
The `ResultObject` Http Extensions allow you to map your `Result` success/failures to `IActionResults`

For `Result.Success()`, instances map to `StatusCodes.Status204NoContent`.

`Result<T>.Success(...)` instances map to `StatusCodes.Status200Ok`.

For `Result.Failure`, the various message types are mapped to corresponding HttpStatusCodes.

```
MessageType.NotFound -> StatusCodes.Status404NotFound
MessageType.Unauthorised -> StatusCodes.Status401Unauthorized
MessageType.Forbidden -> StatusCodes.Status403Forbidden

All others -> Status400BadRequest
```

Example:- 

```csharp

// returns a 200 Ok ActionResult
Result result = Result.Success(23);
result.ActionResult();

// returns a 400 Bad Request ActionResult.
Result result = Result.Failure();
result.ActionResult();

```

### Handling success/failures with messages

The `Result` object supports convenience methods to help determine if certain messages are present.

```csharp
var errors = result.HasInformationMessages();

var errors = result.HasWarnings());

var errors = result.HasErrors();

var errors = result.HasValidationErrors;
```

You can gain further control on handling an error by bundling the failure with a check for the presence of a message type.

```csharp
e.g.

if (!result.IsSuccess && result.HasWarnings()) {
    ...do something
}
```

### Json MessageOutputFormatter

By using the included Json `MessageOutputFormatter`, we can control the details of messages serialised on `Result` objects.

```csharp
services.AddControllers(options =>
{
    options.OutputFormatters.Insert(0, new MessageOutputFormatter(new JsonSerializerOptions(JsonSerializerDefaults.Web)));
});
```

The `MessageOutputFormatter` uses a configured `ResultMessageLevelOptions` instance to determine which fields should be serialized.

```csharp
public class ResultMessageLevelOptions
{
    public const string ResultMessageLevel = "ResultMessageLevel";
    public bool Code { get; set; }
    public bool Tokens { get; set; }
    public bool Template { get; set; }
    public bool LanguageCode { get; set; }
}
```

By default serialized `Result` messages are returned with the `MessageType` and localised `Content` fields.

```csharp
e.g.
{
    "messages": [
        {
            "type": "information",
            "content": "This is an info message with a parameter \"some dynamic juicy content just for you\"."
        },
        {
            "type": "warning",
            "content": "This is a warning message."
        }
    ]
}
```

However, the `Code`, `Template`, `Tokens` and `LanguageCode` properties can be configured for serialization too.

These properties can be configured at the dotnet core configuration level (e.g. appSettings.json)

```csharp
{
  "ResultMessageLevel": {
    "Code": true,
    "Tokens": true,
    "Template": true,
    "LanguageCode": true
  },
}
```

Or on a per Http request basis using the `Result-Message-Levels` http header value

```csharp
Result-Message-Levels: code,template,tokens,languagecode
```

Use the included `ResultMessageLevel` middleware to enable configuration of the `MessageOutputFormatter`.

```chsarp
app.UseResultMessageLevelMiddleware();
```


Example when all properties are set to be serialized:

```csharp
{
    "messages": [
        {
            "type": "information",
            "code": "msg-key-info",
            "template": "This is an info message with a parameter \"{type}\".",
            "tokens": {
                "type": "some dynamic juicy content just for you"
            },
            "languageCode": "en-US",
            "content": "This is an info message with a parameter \"some dynamic juicy content just for you\"."
        },
        {
            "type": "warning",
            "code": "msg-key-warning",
            "template": "This is a warning message.",
            "languageCode": "en-US",
            "content": "This is a warning message."
        }
    ]
}
```

When all properties are serialized the client has complete control on how to handle individual messages. 

### Asserting ResultObject 

todo: bring across the unit testing stuff for validators/results and document it up

## Interesting Resources about Result Pattern

- [Error Handling — Returning Results by Michael Altmann](https://medium.com/@michael_altmann/error-handling-returning-results-2b88b5ea11e9)
- [Operation Result Pattern by Carl-Hugo Marcotte](https://www.forevolve.com/en/articles/2018/03/19/operation-result/)
- [Exceptions for flow control in C# by Vladimir Khorikov](https://enterprisecraftsmanship.com/posts/exceptions-for-flow-control/)
- [Error handling: Exception or Result? by Vladimir Khorikov](https://enterprisecraftsmanship.com/posts/error-handling-exception-or-result/)
- [What is an exceptional situation in code? by Vladimir Khorikov](https://enterprisecraftsmanship.com/posts/what-is-exceptional-situation/)
- [Advanced error handling techniques by Vladimir Khorikov](https://enterprisecraftsmanship.com/posts/advanced-error-handling-techniques/)
- [A Simple Guide by Isaac Cummings](https://medium.com/@cummingsi1993/the-operation-result-pattern-a-simple-guide-fe10ff959080)
- [Flexible Error Handling w/ the Result Class by Khalil Stemmler](https://khalilstemmler.com/articles/enterprise-typescript-nodejs/handling-errors-result-class/)
- [Combining ASP.NET Core validation attributes with Value Objects by Vladimir Khorikov](https://enterprisecraftsmanship.com/posts/combining-asp-net-core-attributes-with-value-objects/)

## Contributors

Thanks to all the contributors and to all the people who gave feedback!

- [Gerrod Thomas](https://github.com/gerrod)

## License

See [LICENSE](https://raw.githubusercontent.com/benpriebe/ResultObject/master/LICENSE) for details. 