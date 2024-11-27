using TaAssistant.Model;
using TaAssistant.Model.Api;
using LanguageExt;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace TaAssistant.Service.V1
{
    public static class Common
    {
        private const string GENERIC_ERROR_TITLE = "Error(s) processing request";
        private const string INTERNAL_SERVER_ERROR_TITLE = "Internal Server Error";

        /// <summary>
        /// A static method to convert an <see cref="Error"/> to an <see cref="ApiProblemDetails"/>.
        /// </summary>
        /// <param name="error">The error to map to <see cref="ApiProblemDetails"/>.</param>
        /// <param name="origination">The origination used to describe where in the api the error occurred.</param>
        /// <param name="errorResolver">An error code mapper that maps the error message to a <see cref="ErrorCode"/>.</param>
        /// <returns>An <see cref="ApiProblemDetails"/>.</returns>
        public static ApiProblemDetails ToProblemDetails(this Error error,
            ApiProblemDetailsOrigination origination = ApiProblemDetailsOrigination.Unspecified,
            Func<string, ErrorCode>? errorResolver = null) =>
            ApiProblemDetails.Create(GENERIC_ERROR_TITLE, (int)error.StatusCode, errors: error.Faults.Map(e => new Fault
            {
                ErrorSource = e.ErrorSource,
                ErrorType = e.ErrorType,
                Field = e.Field,
                Message = e.Message,
                ErrorCode = e.ErrorCode != ErrorCode.Unspecified || errorResolver == null ? e.ErrorCode : errorResolver(e.Message)
            }), error.RequestId.ToString(), origination);

        internal static ApiProblemDetails ToSystemErrorProblemsDetails(
            this Exception exception,
            ErrorCode errorCode = ErrorCode.Unspecified,
            ErrorSource errorSource = ErrorSource.TaAssistantApi,
            ErrorType errorType = ErrorType.System) =>
            ApiProblemDetails.Create(INTERNAL_SERVER_ERROR_TITLE, Status500InternalServerError, exception.Message, errorSource, errorType, errorCode,
                string.Empty);

        /// <summary>
        /// A method used to handle an either and map the left value to <see cref="ApiProblemDetails"/>.
        /// </summary>
        /// <typeparam name="TR">The right return type.</typeparam>
        /// <param name="f">The method to run and map the return of.</param>
        /// <param name="origination">The origination of the error, used to describe where it occurred in the API.</param>
        /// <param name="errorResolver">An error code mapper that maps the error message to the a <see cref="ErrorCode"/>.</param>
        /// <returns>An <see cref="ApiProblemDetails"/>.</returns>
        public static Either<ApiProblemDetails, TR> MapLeft<TR>(Func<Either<Error, TR>> f,
            ApiProblemDetailsOrigination origination = ApiProblemDetailsOrigination.Unspecified,
            Func<string, ErrorCode>? errorResolver = null) =>
            f().MapLeft(e => e.ToProblemDetails(origination, errorResolver));

        /// <summary>
        /// An asynchronous method used to handle an either and map the left value to <see cref="ApiProblemDetails"/>.
        /// </summary>
        /// <typeparam name="TR">The right return type.</typeparam>
        /// <param name="f">The method to run and map the return of.</param>
        /// <param name="origination">The origination of the error, used to describe where it occurred in the API.</param>
        /// <param name="errorResolver">An error code mapper that maps the error message to the <see cref="ErrorCode"/>.</param>
        /// <returns>An <see cref="ApiProblemDetails"/>.</returns>
        public static async Task<Either<ApiProblemDetails, TR>> MapLeft<TR>(Func<Task<Either<Error, TR>>> f,
            ApiProblemDetailsOrigination origination = ApiProblemDetailsOrigination.Unspecified,
            Func<string, ErrorCode>? errorResolver = null) =>
            (await f()).MapLeft(er => er.ToProblemDetails(origination, errorResolver));
    }
}