using System.Collections.ObjectModel;
using System.Net;

namespace TaAssistant.Model
{
    public class Error
    {
        /// <summary>
        /// An identifier to identify the request.
        /// </summary>
        public Guid RequestId { get; set; }
        /// <summary>
        /// A collection of errors that are attached to this response.
        /// </summary>
        public ICollection<Fault> Faults { get; set; } = new Collection<Fault>();
        /// <summary>
        /// The HttpStatusCode that came from the request.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// A static constructor class used to create the <see cref="Error"/> class.
        /// </summary>
        /// <param name="errorSource">The source of the error.</param>
        /// <param name="statusCode">The HttpStatusCode from the response.</param>
        /// <param name="message">The message for the error.</param>
        /// <param name="errorType">A type describing the error.</param>
        /// <param name="errorCode">A code that identifies the error.</param>
        /// <returns>An instance of the <see cref="Error"/> class.</returns>
        public static Error Create(
            ErrorSource errorSource,
            HttpStatusCode statusCode,
            string message,
            ErrorType errorType = ErrorType.Unspecified,
            Exception? exception = null,
            ErrorCode errorCode = ErrorCode.Unspecified) =>
                new()
                {
                    Faults = new List<Fault>
                    {
                        new Fault
                        {
                            // TODO: Exception here causes a circular reference on OpenAPI generation
                            //Exception = exception,
                            ErrorSource = errorSource,
                            Message = message,
                            ErrorType = errorType,
                            ErrorCode = errorCode
                        }
                    },
                    StatusCode = statusCode
                };
    }

    /// <summary>
    /// A class used to describe an error.
    /// </summary>
    public partial class Fault
    {
        /// <summary>
        /// The field the error is referring to.
        /// </summary>
        public string Field { get; set; } = string.Empty;
        /// <summary>
        /// The source of the error.
        /// </summary>
        public ErrorSource ErrorSource { get; set; } = ErrorSource.Unspecified;
        /// <summary>
        /// The type of the error.
        /// </summary>
        public ErrorType ErrorType { get; set; } = ErrorType.Unspecified;
        /// <summary>
        /// A code to uniquely identify an error.
        /// </summary>
        public ErrorCode ErrorCode { get; set; } = ErrorCode.Unspecified;
        /// <summary>
        /// A message describing the error.
        /// </summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// Exception if any associated with the request
        /// </summary>
        //public Exception? Exception { get; set; }
    }

    /// <summary>
    /// A defined source of where the error occurred.
    /// </summary>
    public enum ErrorSource
    {
        /// <summary>
        /// An unspecified error source.
        /// </summary>
        Unspecified,
        /// <summary>
        /// An error stemming from the TaAssistantAPI.
        /// </summary>
        TaAssistantApi,
        /// <summary>
        /// An error stemming from the UserTypeRepository.
        /// </summary>
        UserTypeRepository,
        /// <summary>
        /// An error stemming from the USerRepository.
        /// </summary>
        UserRepository
    }

    /// <summary>
    /// Types of errors that could occur.
    /// </summary>
    public enum ErrorType
    {
        /// <summary>
        /// An unspecified error type.
        /// </summary>
        Unspecified = 0,
        /// <summary>
        /// An error stemming from the account status.
        /// </summary>
        AccountStatus = 1,
        /// <summary>
        /// An error stemming from business rules.
        /// </summary>
        BusinessRules = 2,
        /// <summary>
        /// An error stemming from validation.
        /// </summary>
        Validation = 3,
        /// <summary>
        /// An error stemming from security.
        /// </summary>
        Security = 4,
        /// <summary>
        /// An error stemming from system logic.
        /// </summary>
        System = 5,
        /// <summary>
        /// An error stemming from integrations.
        /// </summary>
        Integrations = 6,
    }

    /// <summary>
    /// Unique identifiers for errors.
    /// </summary>
    public enum ErrorCode
    {
        // General Error Codes
        /// <summary>
        /// An unspecified error code.
        /// </summary>
        Unspecified,
        /// <summary>
        /// An error that has to do with a field conflict.
        /// </summary>
        FieldConflict,
        /// <summary>
        /// An error from a field value that was not supplied.
        /// </summary>
        FieldValueRequired,
        /// <summary>
        /// An error from a forbidden action.
        /// </summary>
        Forbidden,
        /// <summary>
        /// An error stemming from a conflict of identifier.
        /// </summary>
        IdentifierConflict,
        /// <summary>
        /// An error stemming from an invalid value being assigned to a field.
        /// </summary>
        InvalidFieldValue,
        /// <summary>
        /// An error stemming from an invalid identifier.
        /// </summary>
        InvalidIdentifier,
        /// <summary>
        /// An error stemming from an invalid status.
        /// </summary>
        InvalidStatus,
        /// <summary>
        /// An error originating from an invalid string length issue.
        /// </summary>
        InvalidStringLength,
        /// <summary>
        /// An error occurred due to a user requesting a resource with a limit on it with that limit being reached.
        /// </summary>
        RateLimitExceeded,
        /// <summary>
        /// An error occurred because the request body was not sent while it was required.
        /// </summary>
        RequestBodyRequired,
        /// <summary>
        /// An error occurred because the requested setting is not supported.
        /// </summary>
        SettingsNotSupported,
        /// <summary>
        /// An error occurred because the request is unauthorized.
        /// </summary>
        Unauthorized,
        /// <summary>
        /// An unknown error occurred.
        /// </summary>
        Unknown,
        /// <summary>
        /// An error occurred while verifying.
        /// </summary>
        VerificationFailure,

        // Specific Error Codes
    }

    /// <summary>
    /// An interface used to log HTTP request and responses to and from the proxies.
    /// </summary>
    public interface IHttpCallLogger
    {
        /// <summary>
        /// The method to log the request and response.
        /// </summary>
        /// <param name="requestResponse">A concrete type used to hold values of the request and the response.</param>
        public void Log(HttpCall requestResponse);
        /// <summary>
        /// The sender of the request.
        /// </summary>
        public string Sender { get; init; }
        /// <summary>
        /// The recipient of the request.
        /// </summary>
        public string Recipient { get; init; }
        /// <summary>
        /// The header of the correlationId.
        /// </summary>
        public string CorrelationIdHeader { get; init; }
        /// <summary>
        /// Whether or not the implementation of the logger is a null logger, meaning that log functionality should be ignored.
        /// </summary>
        public bool IsSilent { get; }
    }

    /// <summary>
    /// Null logger that doesn't log anything. Used when no logging at proxy level is desired.
    /// </summary>
    public class NullHttpCallLogger : IHttpCallLogger
    {
        /// <inheritdoc />
        public void Log(HttpCall requestResponse) { }
        /// <inheritdoc />
        public string Sender { get; init; } = string.Empty;
        /// <inheritdoc />
        public string Recipient { get; init; } = string.Empty;
        /// <inheritdoc />
        public string CorrelationIdHeader { get; init; } = string.Empty;
        /// <inheritdoc />
        public bool IsSilent => true;
    }

    /// <summary>
    /// A type to hold information to describe the request and response.
    /// </summary>
    public class HttpCall
    {
        /// <summary>
        /// Time of the request start.
        /// </summary>
        public DateTimeOffset RequestStart { get; set; }
        /// <summary>
        /// Time of the request start.
        /// </summary>
        public DateTimeOffset RequestEnd { get; set; }
        /// <summary>
        /// The sender of the HttpCall.
        /// </summary>
        public string Sender { get; set; } = string.Empty;
        /// <summary>
        /// The recipient involved in the request.
        /// </summary>
        public string Recipient { get; set; } = string.Empty;
        /// <summary>
        /// The uri the request is accessing.
        /// </summary>
        public string Uri { get; set; } = string.Empty;
        /// <summary>
        /// The HTTP Method of the request.
        /// </summary>
        public string Method { get; set; } = string.Empty;
        /// <summary>
        /// The headers on the request.
        /// </summary>
        public string RequestHeaders { get; set; } = string.Empty;
        /// <summary>
        /// The body of the request.
        /// </summary>
        public string RequestBody { get; set; } = string.Empty;
        /// <summary>
        /// The status code of the response.
        /// </summary>
        public int StatusCode { get; set; }
        /// <summary>
        /// The headers on the response.
        /// </summary>
        public string ResponseHeaders { get; set; } = string.Empty;
        /// <summary>
        /// The body of the response.
        /// </summary>
        public string ResponseBody { get; set; } = string.Empty;
        /// <summary>
        /// The duration of the request response interaction.
        /// </summary>
        public long Duration { get; set; }
        /// <summary>
        /// The correlationId of the request.
        /// </summary>
        public string CorrelationId { get; set; } = string.Empty;
    }
}