using LanguageExt;
using MediatR;
using TaAssistant.Interfaces.Repositories;
using TaAssistant.Model.Api;
using TaAssistant.Model.Api.Responses;

namespace TaAssistant.Service.V1
{

    public record GetCourses() : IRequest<Either<ApiProblemDetails, IEnumerable<CourseResponse>>> { }
    public record GetGrades() : IRequest<Either<ApiProblemDetails, IEnumerable<GradeResponse>>> { }

    public class CourseHandler :
        IRequestHandler<GetCourses, Either<ApiProblemDetails, IEnumerable<CourseResponse>>>,
        IRequestHandler<GetGrades, Either<ApiProblemDetails, IEnumerable<GradeResponse>>>
    {
        ICourseRepository course;

        public CourseHandler(ICourseRepository course) => this.course = course;

        public async Task<Either<ApiProblemDetails, IEnumerable<CourseResponse>>> Handle(GetCourses request, CancellationToken cancellationToken) =>
            await (
                from c in Common.MapLeft(() => course.GetCourses()).ToAsync()
                select c.Select(e => new CourseResponse
                {
                    CourseId = e.CourseId,
                    Prefix = e.Prefix,
                    Code = e.Code,
                    RequireTA = e.RequireTA,
                })
            );

        public async Task<Either<ApiProblemDetails, IEnumerable<GradeResponse>>> Handle(GetGrades request, CancellationToken cancellationToken) =>
            await (
                from g in Common.MapLeft(() => course.GetGrades()).ToAsync()
                select g.Select(e => new GradeResponse
                {
                    GradeId = e.GradeId,
                    Grade = e.Grade,
                })
            );
    }
}
