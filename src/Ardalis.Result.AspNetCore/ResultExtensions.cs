using System;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using static Ardalis.Result.GenericTypeHelpers;
namespace Ardalis.Result.AspNetCore
{
    /// <summary>
    /// Extensions to support converting Result to an ActionResult
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Convert an Ardalis.Result to a Microsoft.AspNetCore.Mvc.ActionResult
        /// </summary>
        /// <typeparam name="T">The value being returned</typeparam>
        /// <param name="controller">The controller this is called from</param>
        /// <param name="result">The Result to convert to an ActionResult</param>
        /// <returns></returns>
        public static ActionResult<T> ToActionResult<T>(this Result<T> result, ControllerBase controller)
        {
            return controller.ToActionResult((IResult)result);
        }

        /// <summary>
        /// Convert an Ardalis.Result to a Microsoft.AspNetCore.Mvc.ActionResult
        /// </summary>
        /// <typeparam name="T">The value being returned</typeparam>
        /// <param name="controller">The controller this is called from</param>
        /// <param name="result">The Result to convert to an ActionResult</param>
        /// <returns></returns>
        public static ActionResult<T> ToActionResult<T>(this ControllerBase controller,
            Result<T> result)
        {
            return controller.ToActionResult((IResult)result);
        }

        internal static ActionResult ToActionResult(this ControllerBase controller, IResult result)
        {
            switch (result.Status)
            {
                case ResultStatus.Ok: return controller.Ok(result.GetValue());
                case ResultStatus.NotFound: return controller.NotFound();
                case ResultStatus.Unauthorized: return controller.Unauthorized();
                case ResultStatus.Forbidden: return controller.Forbid();
                case ResultStatus.Invalid: return new BadRequestObjectResult(new ValidationDto(result?.ValidationErrors));
                case ResultStatus.Error: return UnprocessableEntity(controller, result);
                case ResultStatus.Conflict: return ConflictResult(controller, result);
                case ResultStatus.Created: return controller.Created(result?.Uri, result.GetValue());
                case ResultStatus.NoContent: return controller.NoContent();
                default:
                    throw new NotSupportedException($"Result {result.Status} conversion is not supported.");
            }
        }

        private static ActionResult BadRequest(ControllerBase controller, IResult result)
        {
            foreach (var error in result.ValidationErrors)
            {
                controller.ModelState.AddModelError(error.Identifier, error.ErrorMessage);
            }
            return controller.BadRequest(controller.ModelState);
        }

        private static ActionResult ConflictResult(ControllerBase controller, IResult result)
        {
            var identifier = GetInnerMostGenericTypeName(result.GetType()) ?? "unknown type";

            var errors = result.Errors.Select(e => new ValidationError()
            {
                ErrorCode = "Conflict",
                ErrorMessage = e,
                Severity = ValidationSeverity.Error,
                Identifier = identifier
            });
            var validationDto = new ValidationDto(errors.ToList(), HttpStatusCode.Conflict,"A conflict has been detected");
            return controller.StatusCode((int)HttpStatusCode.Conflict, validationDto);
        }

        //private static ActionResult ConflictResult(ControllerBase controller, IResult result)
        //{
        //    var details = new StringBuilder("Next error(s) occurred:");

        //    var errors = result.Errors.Select(e => new ValidationError()
        //    {
        //        ErrorCode = "Conflict",
        //        ErrorMessage = e,
        //        Severity = ValidationSeverity.Error,
        //        Identifier = result.GetType().Name
        //    });
        //    var validationDto = new ValidationDto(errors.ToList());

        //    var errs = result.Errors.ToList();

        //    var modelStateDictionary = new ModelStateDictionary();
        //    foreach (var error in errs)
        //    {
        //        var modelError = new ModelError(error);
        //        modelStateDictionary..AddModelError(modelError);
        //    }
        //    //return controller.Conflict()

        //    //foreach (var error in result.Errors) details.Append("* ").Append(error).AppendLine();

        //    //return controller.Conflict(new ProblemDetails()
        //    //{
        //    //    Title = "Item already exists",
        //    //    Detail = details.ToString()
        //    //});
        //}

        private static ActionResult UnprocessableEntity(ControllerBase controller, IResult result)
        {
            var details = new StringBuilder("Next error(s) occurred:");

            foreach (var error in result.Errors) details.Append("* ").Append(error).AppendLine();

            return controller.UnprocessableEntity(new ProblemDetails
            {
                Title = "Something went wrong.",
                Detail = details.ToString()
            });
        }
    }


}
