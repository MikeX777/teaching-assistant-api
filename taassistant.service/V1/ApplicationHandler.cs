using LanguageExt;
using MediatR;
using TaAssistant.Interfaces.Repositories;
using TaAssistant.Model.Api;
using TaAssistant.Model.Api.Requests;
using TaAssistant.Model.Api.Responses;
using TaAssistant.Model.Entities;

namespace TaAssistant.Service.V1
{
    public record GetTerms() : IRequest<Either<ApiProblemDetails, IEnumerable<TermResponse>>> { }
    public record GetApplicationStatuses() : IRequest<Either<ApiProblemDetails, IEnumerable<ApplicationStatusResponse>>> { }
    public record SubmitApplication(SubmitApplicationRequest Request) : IRequest<Either<ApiProblemDetails, LanguageExt.Unit>> { }
    public record SubmitApplicationWithCourses(SubmitApplicationWithCoursesRequest Request) : IRequest<Either<ApiProblemDetails, LanguageExt.Unit>> { }
    public record GetUpcomingApplications(int UserId) : IRequest<Either<ApiProblemDetails, IEnumerable<ApplicationResponse>>> { }

    public class ApplicationHandler :
        IRequestHandler<GetTerms, Either<ApiProblemDetails, IEnumerable<TermResponse>>>,
        IRequestHandler<GetApplicationStatuses, Either<ApiProblemDetails, IEnumerable<ApplicationStatusResponse>>>,
        IRequestHandler<SubmitApplication, Either<ApiProblemDetails, LanguageExt.Unit>>,
        IRequestHandler<GetUpcomingApplications, Either<ApiProblemDetails, IEnumerable<ApplicationResponse>>>,
        IRequestHandler<SubmitApplicationWithCourses, Either<ApiProblemDetails, LanguageExt.Unit>>
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

        public async Task<Either<ApiProblemDetails, LanguageExt.Unit>> Handle(SubmitApplicationWithCourses request, CancellationToken cancellationToken) =>
            await (
                from s in Common.MapLeft(() => application.SubmitApplicationWithCourses(request.Request))
                select s);

        public async Task<Either<ApiProblemDetails, IEnumerable<ApplicationResponse>>> Handle(GetUpcomingApplications request, CancellationToken cancellationToekn) =>
            await (
                from a_s in Common.MapLeft(() => application.GetApplicationStatuses()).ToAsync()
                from t in Common.MapLeft(() => application.GetTerms()).ToAsync()
                from a in Common.MapLeft(() => application.GetUserApplicationsByStatus(request.UserId, NotFinalStateStatusIds(a_s))).ToAsync()
                select a.Select(e =>
                {
                    var term = t.FirstOrDefault(t_e => t_e.TermId == e.TermId) ?? new TermEntity();
                    var status = a_s.FirstOrDefault(a_s_e => a_s_e.ApplicationStatusId == e.ApplicationStatusId) ?? new ApplicationStatusEntity();
                    return new ApplicationResponse
                    {
                        ApplicationId = e.ApplicationId,
                        UserId = e.UserId,
                        Term = new TermResponse {  TermId = term.TermId, TermName = term.TermName },
                        Status = new ApplicationStatusResponse {  ApplicationStatusId = status.ApplicationStatusId, Status = status.Status },
                        Year = e.Year,
                        PreviousTA = e.PreviousTA,
                        InstructorNotes = e.InstructorNotes
                    };
                }));

        private IEnumerable<int> NotFinalStateStatusIds(IEnumerable<ApplicationStatusEntity> statuses) =>
            statuses.Where(s => s.Status != "Rejected" && s.Status != "Denied" && s.Status != "Completed").Select(s => s.ApplicationStatusId);
    }
}
