using LanguageExt;
using MediatR;
using TaAssistant.Model.Api;

namespace TaAssistant.Service.V1
{
    /// <summary>
    /// The command to get all the available dummy values.
    /// </summary>
    public record GetDummyValues() : IRequest<Either<ApiProblemDetails, IEnumerable<string>>> { }

    /// <summary>
    /// Handler for the dummy controller.
    /// </summary>
    public class DummyHandler :
        IRequestHandler<GetDummyValues, Either<ApiProblemDetails, IEnumerable<string>>>
    {
        /// <summary>
        /// Handler for the dummy values.
        /// </summary>
        /// <param name="request">The command to run.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Either a <see cref="ApiProblemDetails"/> or an <see cref="IEnumerable{String}"/></returns>
        public async Task<Either<ApiProblemDetails, IEnumerable<string>>> Handle(GetDummyValues request, CancellationToken cancellationToken) =>
            new List<string> { "test1", "test2" };
    }
}
