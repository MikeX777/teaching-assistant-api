using Api.TaAssistant.Abstractions;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TaAssistant.Model.Api;
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
    public class UserTypesController(IMediator mediator) : RespondController
    {
        private readonly IMediator mediator = mediator;


        /// <summary>
        /// A general dummy GET request to the User Types in the application.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{UserTypeResponse}"/></returns>
        [HttpGet]
        [SwaggerOperation("A action to get all user types.")]
        [ProducesResponseType(Status200OK, Type = typeof(Response<IEnumerable<UserTypeResponse>>))]
        public async Task<IActionResult> GetDummy() =>
            Respond(await mediator.Send(new GetUserTypes()));

    }
}
