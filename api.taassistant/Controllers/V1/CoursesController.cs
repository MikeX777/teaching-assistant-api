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
    /// Constructor for <see cref="CoursesController"/>.
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
    public class CoursesController(IMediator mediator) : RespondController
    {
        private readonly IMediator mediator = mediator;

        /// <summary>
        /// Retrieves the available courses.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SwaggerOperation("Get all courses.")]
        [ProducesResponseType(Status200OK, Type = typeof(Response<IEnumerable<CourseResponse>>))]
        public async Task<IActionResult> RetrieveCourses() =>
            Respond(await mediator.Send(new GetCourses()));

        /// <summary>
        /// Retrieves the available grades.
        /// </summary>
        /// <returns></returns>
        [HttpGet("grades")]
        [SwaggerOperation("An action to retrieve grade types.")]
        [ProducesResponseType(Status200OK, Type = typeof(Response<IEnumerable<GradeResponse>>))]
        public async Task<IActionResult> RetrieveGrades() =>
            Respond(await mediator.Send(new GetGrades()));

    }
}
