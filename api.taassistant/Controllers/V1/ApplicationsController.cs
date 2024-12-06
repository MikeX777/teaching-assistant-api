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
    /// Constructor for <see cref="ApplicationsController"/>.
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
    public class ApplicationsController(IMediator mediator) : RespondController
    {
        private readonly IMediator mediator = mediator;


        /// <summary>
        /// Submits an application.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("submit")]
        [SwaggerOperation("Submits an Application")]
        [ProducesResponseType(Status200OK, Type = typeof(Response<LanguageExt.Unit>))]
        public async Task<IActionResult> SubmitApplication([FromBody] SubmitApplicationRequest request) =>
            Respond(await mediator.Send(new SubmitApplication(request)));

        [HttpPost("submit-with-courses")]
        [SwaggerOperation("Submits an application with the courses")]
        [ProducesResponseType(Status200OK, Type = typeof(Response<LanguageExt.Unit>))]
        public async Task<IActionResult> SubmitApplicationWithCourses([FromBody] SubmitApplicationWithCoursesRequest request) =>
            Respond(await mediator.Send(new SubmitApplicationWithCourses(request)));

        /// <summary>
        /// Gets the possible terms for an application.
        /// </summary>
        /// <returns></returns>
        [HttpGet("terms")]
        [SwaggerOperation("Get the different term types.")]
        [ProducesResponseType(Status200OK, Type = typeof(Response<IEnumerable<TermResponse>>))]
        public async Task<IActionResult> GetTerms() =>
            Respond(await mediator.Send(new GetTerms()));

        /// <summary>
        /// Gets the possible statuses for an application.
        /// </summary>
        /// <returns></returns>
        [HttpGet("statuses")]
        [SwaggerOperation("Get the different application status.")]
        [ProducesResponseType(Status200OK, Type = typeof(Response<IEnumerable<ApplicationStatusResponse>>))]
        public async Task<IActionResult> GetApplicationStatus() =>
            Respond(await mediator.Send(new GetApplicationStatuses()));


        /// <summary>
        /// Gets upcoming Application Data for specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <returns></returns>
        [HttpGet("users/{userId}/upcoming")]
        [SwaggerOperation("Gets the upcoming application for a user.")]
        [ProducesResponseType(Status200OK, Type = typeof(Response<IEnumerable<ApplicationResponse>>))]
        public async Task<IActionResult> GetUpcomingApplications([FromRoute] int userId) =>
            Respond(await mediator.Send(new GetUpcomingApplications(userId)));


    }
}
