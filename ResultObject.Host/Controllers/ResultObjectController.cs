using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ResultObject.Core;
using ResultObject.Core.Http.Extensions;
using Resx = ResultObject.Host.i18n.Message;

namespace ResultObject.Host.Controllers
{
    [ApiController]
    [Route("result-object/example")]
    public class ResultObjectController : ControllerBase
    {
        private readonly ILogger<ResultObjectController> logger;
        private readonly ResultMessageLevelOptions resultMessageLevelOptions;

        public ResultObjectController(
            ILogger<ResultObjectController> logger, 
            IOptions<ResultMessageLevelOptions> resultMessageLevelOptions)
        {
            this.logger = logger;
            this.resultMessageLevelOptions = resultMessageLevelOptions.Value;
        }

        [HttpGet("no-content")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult GetNoContent()
        {
            Result result = Result.Success()
                
                .WithLogMessage("Log message")
                .WithLogMessage("Another log message");

            logger.LogWarning(result.GetInvariantMessages());
            return result.ActionResult();
        }

        [HttpGet("content")]
        [ProducesResponseType(typeof(Result<Response>), StatusCodes.Status200OK)]
        public IActionResult GetContent()
        {
            Result<Response> result = Result.Success<Response>(new Response { Name = "jimbo jones", Mobile = "0400 123 123" })
                .WithInfo<Resx>(nameof(Resx.MsgKeyInfo), new { type = "some dynamic juicy content just for you"})
                .WithWarning<Resx>(nameof(Resx.MsgKeyWarning));

            logger.LogWarning(result.GetInvariantMessages());
            return result.ActionResult();
        }

        [HttpGet("bad-request")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        public IActionResult GetBadRequest()
        {
            Result result = Result.Failure()
                .WithError<Resx>(nameof(Resx.MsgKeyInfo), new {type = "some dynamic juicy content just for you"});

            logger.LogInformation(result.GetInvariantMessages());
            return result.ActionResult();
        }

        [HttpGet("not-found")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        public IActionResult GetNotFound()
        {
            var id = "1000";
            Result result = Result.Failure()
                .NotFound<string>(id);

            logger.LogInformation(result.GetInvariantMessages());
            return result.ActionResult();
        }

        [HttpGet("forbidden")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        public IActionResult GetForbidden()
        {
            Result result = Result.Failure().Forbidden();
            logger.LogInformation(result.GetInvariantMessages());
            return result.ActionResult();
        }

        [HttpGet("unauthorised")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        public IActionResult GetUnauthorised()
        {
            Result result = Result.Failure().Unauthorized();
            logger.LogInformation(result.GetInvariantMessages());
            return result.ActionResult();        
        }

        [HttpPost("validator")]
        public IActionResult GetValidator(ValidateRequest request)
        {
            var validator = new Validator();
            validator
                .ValidatePropertyIsRequired(nameof(request.Name), request.Name)
                .ValidateStringLength(nameof(request.Name), request.Name, 5, 3)
                .ValidateCollectionHasValues(nameof(request.Emails), request.Emails)
                .Validate<Resx>(
                    nameof(Resx.MsgKeyValidationError), 
                    new {email = request.Emails?.FirstOrDefault()}, 
                    request.Emails == null || request.Emails.Any() && request.Emails?.Any(email => email.EndsWith("gmail.com")) == true);
                
            Result result = validator.HasErrors 
                ? Result.Failure().WithValidator(validator)
                : Result.Success();
            return result.ActionResult();
        }
        
        [HttpGet("message-contract")]
        public IActionResult GetMessage()
        {
            return null;
        }

        [HttpGet("result-message-level-options")]
        public IActionResult ResultMessageLevelOptions()
        {
            return Ok(resultMessageLevelOptions);
        }
    }
    
    public class ValidateRequest
    {
        public string Name { get; set; }
        public string[] Emails { get; set; }
    }

    public class Response
    {
        public string Name { get; init; }
        public string Mobile { get; init; }
    }
}
