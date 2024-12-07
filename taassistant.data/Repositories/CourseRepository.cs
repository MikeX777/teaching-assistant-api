using Dapper;
using LanguageExt;
using System.Data;
using TaAssistant.Interfaces.Repositories;
using TaAssistant.Model;
using TaAssistant.Model.Entities;
using static TaAssistant.Data.BaseRepository;


namespace TaAssistant.Data.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly IDbConnection connection;

        public CourseRepository(IDbConnection connection) => this.connection = connection;

        public async Task<Either<Error, IEnumerable<CourseEntity>>> GetCourses() =>
            await TryFuncCatchExceptionAsync(async () => await connection.QueryAsync<CourseEntity>(
                """
                    SELECT c.course_id, c.prefix, c.code, c.require_ta FROM courses c
                """),
                 mapError: (ex) => Error.Create(ErrorSource.CourseRepository, System.Net.HttpStatusCode.InternalServerError, ex.Message));

        public async Task<Either<Error, IEnumerable<GradeEntity>>> GetGrades() =>
            await TryFuncCatchExceptionAsync(async () => await connection.QueryAsync<GradeEntity>(
                """
                    SELECT g.grade_id, g.grade FROM grades g
                """),
                 mapError: (ex) => Error.Create(ErrorSource.CourseRepository, System.Net.HttpStatusCode.InternalServerError, ex.Message));

    }
}
