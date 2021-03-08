using System;
using Microsoft.AspNetCore.Mvc;

namespace ResultObject.Core.Http.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static IActionResult ActionResult(this ResultBase result)
        {
            return result.IsSuccess
                ? result.HasContent ? new OkResult() : new NoContentResult()
                : FailureResult(result);
        }

        public static IActionResult ActionResult<TEntity>(this Result<TEntity> result, bool includeEnvelope = true)
        {
            if (!result.IsSuccess)
            {
                return FailureResult(result);
            }

            if (result.HasContent)
            {
                return includeEnvelope 
                    ? new OkObjectResult(result) 
                    : new OkObjectResult(result.Value);
            }

            return new NoContentResult();
        }

        #region POST
        
        
        public static IActionResult PostActionResult(this Result result, Uri location)
        {
            return result.PostActionResult(() => location);
        }

        public static IActionResult PostActionResult(this Result result, Func<Uri> locationProvider)
        {
            return !result.IsSuccess ? FailureResult(result) : new CreatedResult(locationProvider.Invoke(), result);
        }

        public static IActionResult PostActionResult<TEntity>(this Result<TEntity> result, Uri location, bool includeEnvelope = true)
        {
            return result.PostActionResult(() => location, includeEnvelope);
        }

        private static IActionResult PostActionResult<TEntity>(this Result<TEntity> result, Func<Uri> locationProvider, bool includeEnvelope = true)
        {
            return !result.IsSuccess ? FailureResult(result) : new CreatedResult(locationProvider.Invoke(), includeEnvelope ? result : (object) result.Value);
        }

        #endregion POST
        
        private static IActionResult FailureResult(ResultBase result, bool includeResult = true)
        {
            if (result.IsUnauthorized)
                return includeResult ? new UnauthorizedObjectResult(result) : new UnauthorizedResult();

            if (result.IsForbidden)
                return new ForbidResult(); // there is no ForbidObjectResult due to security model

            if (result.IsNotFound)
                return includeResult ? new NotFoundObjectResult(result) :new NotFoundResult();

            return includeResult ? new BadRequestObjectResult(result) :new BadRequestResult();
        }
    }
}