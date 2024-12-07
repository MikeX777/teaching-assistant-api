using Api.TaAssistant.Abstractions;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TaAssistant.Model.Api;
using TaAssistant.Model.Api.Requests;
using TaAssistant.Model.Api.Responses;
using TaAssistant.Service.V1;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Api.TaAssistant.Controllers.V1
{
    /// <summary>
    /// A Controller used to show the basic setup.
    /// </summary>
    /// <remarks>
    /// Constructor for <see cref="UserTypesController"/>.
    /// </remarks>
    /// <param name="mediator">The mediator instance used to send commands.</param>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    [ProducesResponseType(Status401Unauthorized, Type = typeof(ApiProblemDetails))]
    [ProducesResponseType(Status403Forbidden, Type = typeof(ApiProblemDetails))]
    [ProducesResponseType(Status404NotFound, Type = typeof(ApiProblemDetails))]
    [ProducesResponseType(Status503ServiceUnavailable, Type = typeof(ApiProblemDetails))]
    [ProducesResponseType(Status504GatewayTimeout, Type = typeof(ApiProblemDetails))]
    [ProducesResponseType(Status500InternalServerError, Type = typeof(ApiProblemDetails))]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class UsersController(IMediator mediator) : RespondController
    {
        private readonly IMediator mediator = mediator;

        /// <summary>
        /// Action to create a new user
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("create")]
        [SwaggerOperation("An action to create a user")]
        [ProducesResponseType(Status200OK, Type = typeof(ApiProblemDetails))] // TODO: FIX
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request) =>
            Respond(await mediator.Send(new CreateUser(request)));

        /// <summary>
        /// Action to sign in a user
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("signin")]
        [SwaggerOperation("An action to sign in a user")]
        [ProducesResponseType(Status200OK, Type = typeof(ApiProblemDetails))] // TODO: FIX
        public async Task<IActionResult> SignInUsr([FromBody] SignInRequest request) =>
            Respond(await mediator.Send(new SignIn(request)));

        /// <summary>
        /// Action to verify the user.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("verify")]
        [SwaggerOperation("An action to sign in a user")]
        [ProducesResponseType(Status200OK, Type = typeof(Response<UserResponse>))]
        public async Task<IActionResult> VerifyUser([FromBody] VerifyRequest request) =>
            Respond(await mediator.Send(new Verify(request)));

        /// <summary>
        /// Uploads a CV for the user
        /// </summary>
        /// <param name="userId">The user to upload the cv for.</param>
        /// <param name="cv">The cv.</param>
        /// <returns></returns>
        [HttpPost("{userId}/cv")]
        [SwaggerOperation("Upload a CV")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(Status200OK, Type = typeof(Response<LanguageExt.Unit>))]
        public async Task<IActionResult> UploadCV([FromRoute] int userId, IFormFile cv) =>
            Respond(await mediator.Send(new UploadCVForUser(userId, cv)));

    }
}
