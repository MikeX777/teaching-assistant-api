using LanguageExt;
using MediatR;
using TaAssistant.Interfaces.Repositories;
using TaAssistant.Model.Api;
using TaAssistant.Model.Api.Requests;
using TaAssistant.Model.Api.Responses;

namespace TaAssistant.Service.V1
{
    public record GetTerms() : IRequest<Either<ApiProblemDetails, IEnumerable<TermResponse>>> { }
    public record GetApplicationStatuses() : IRequest<Either<ApiProblemDetails, IEnumerable<ApplicationStatusResponse>>> { }
    public record SubmitApplication(SubmitApplicationRequest Request) : IRequest<Either<ApiProblemDetails, LanguageExt.Unit>> { }

    public class ApplicationHandler :
        IRequestHandler<GetTerms, Either<ApiProblemDetails, IEnumerable<TermResponse>>>,
        IRequestHandler<GetApplicationStatuses, Either<ApiProblemDetails, IEnumerable<ApplicationStatusResponse>>>,
        IRequestHandler<SubmitApplication, Either<ApiProblemDetails, LanguageExt.Unit>>
    {
        IApplicationRepository application;

        public ApplicationHandler(IApplicationRepository application) => this.application = application;


        public async Task<Either<ApiProblemDetails, LanguageExt.Unit>> Handle(SubmitApplication request, CancellationToken cancellationToken) =>
            await (
                from s in Common.MapLeft(() => application.SubmitApplication(request.Request))
                select s);

        public async Task<Either<ApiProblemDetails, IEnumerable<ApplicationStatusResponse>>> Handle(GetApplicationStatuses request, CancellationToken cancellationToken) =>
            await (
                from s in Common.MapLeft(() => application.GetApplicationStatuses()).ToAsync()
                select s.Select(e => new ApplicationStatusResponse
                {
                    ApplicationStatusId = e.ApplicationStatusId,
                    Status = e.Status,
                }));

        public async Task<Either<ApiProblemDetails, IEnumerable<TermResponse>>> Handle(GetTerms request, CancellationToken cancellationToken) =>
            await (
            from t in Common.MapLeft(() => application.GetTerms()).ToAsync()
            select t.Select(e => new TermResponse
            {
                TermId = e.TermId,
                TermName = e.TermName,
            }));
    }
}
