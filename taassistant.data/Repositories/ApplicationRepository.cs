using Dapper;
using LanguageExt;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using TaAssistant.Interfaces.Repositories;
using TaAssistant.Model;
using TaAssistant.Model.Api.Requests;
using TaAssistant.Model.Entities;
using static TaAssistant.Data.BaseRepository;

namespace TaAssistant.Data.Repositories
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly IDbConnection connection;

        public ApplicationRepository(IDbConnection connection) => this.connection = connection;

        public async Task<Either<Error, IEnumerable<ApplicationStatusEntity>>> GetApplicationStatuses() =>
            await TryFuncCatchExceptionAsync(async () => await connection.QueryAsync<ApplicationStatusEntity>(
                """
                    SELECT a_s.application_status_id, a_s.status
                    FROM application_statuses a_s
                """),
                mapError: (ex) => Error.Create(ErrorSource.ApplicationRepository, System.Net.HttpStatusCode.InternalServerError, ex.Message));

        public async Task<Either<Error, IEnumerable<TermEntity>>> GetTerms() =>
            await TryFuncCatchExceptionAsync(async () => await connection.QueryAsync<TermEntity>(
                """
                    SELECT t.term_id, t.term_name
                    FROM terms t
                """),
                mapError: (ex) => Error.Create(ErrorSource.ApplicationRepository, System.Net.HttpStatusCode.InternalServerError, ex.Message));

        public async Task<Either<Error, Unit>> SubmitApplication(SubmitApplicationRequest request) =>
            await TryFuncCatchExceptionAsync(async () =>
            {
                await connection.ExecuteAsync(
                    """
                    INSERT INTO applications
                        (user_id, term_id, application_status_id, year, previous_ta)
                        VALUES
                        (@userId, @termId, @applicationStatusId, @year, @previousTA);
                """, new
                    {
                        userId = request.UserId,
                        termId = request.TermId,
                        applicationStatusId = 1,
                        year = request.Year,
                        previousTA = request.PreviousTA
                    });
                return Unit.Default;
            }, mapError: (ex) => Error.Create(ErrorSource.ApplicationRepository, System.Net.HttpStatusCode.InternalServerError, ex.Message));

        public async Task<Either<Error, Unit>> SubmitApplicationWithCourses(SubmitApplicationWithCoursesRequest request) => 
            await TryFuncCatchExceptionAsync(async () =>
            {
                var applicationId = await connection.QuerySingleAsync<int>(
                    """
                        INSERT INTO applications
                            (user_id, term_id, application_status_id, year, previous_ta)
                        VALUES
                            (@userId, @termId, @applicationStatusId, @year, @previousTA)
                        RETURNING application_id;
                    """, new
                    {
                        userId = request.UserId,
                        termId = request.TermId,
                        applicationStatusId = 1,
                        year = request.Year,
                        previousTA = request.PreviousTA
                    });

                await connection.ExecuteAsync(
                    """
                        INSERT INTO application_courses
                            (user_id, course_id, term_id, year, grade_id, application_id, recommended)
                        VALUES
                            (@userId, @courseId, @termId, @year, @gradeId, @applicationId, @recommended)
                    """, request.Courses.Select(c => new
                    {
                        userId = request.UserId,
                        courseId = c.CourseId,
                        termId = c.TermId,
                        year = c.Year,
                        gradeId = c.GradeId,
                        applicationId = applicationId,
                        recommended = false
                    }));
                return Unit.Default;
            }, mapError: (ex) => Error.Create(ErrorSource.ApplicationRepository, System.Net.HttpStatusCode.InternalServerError, ex.Message));

        public async Task<Either<Error, IEnumerable<ApplicationEntity>>> GetUserApplicationsByStatus(int userId, IEnumerable<int> statusIds) =>
            await TryFuncCatchExceptionAsync(async () =>
            {
                //using var cmd = new NpgsqlCommand("""
                //    SELECT a.application_id, a.user_id, a.term_id, a.application_status_id, a.year, a.previous_ta, a.instructor_notes
                //            FROM applications a
                //            WHERE a.user_id = @userId and a.application_status_id IN @statusIds;
                //    """, connection);
                //cmd.Parameters.AddWithValue("@statusIds", NpgsqlTypes.NpgsqlDbType.Array, statusIds);
                //cmd.Parameters.AddWithValue("@userId", NpgsqlTypes.NpgsqlDbType.Integer, userId);
                //return cmd.ExecuteReaderAsync<ApplicationEntity>();
                //var statuses = new NpgsqlParameter("@statusIds", NpgsqlDbType.Array | NpgsqlDbType.Box);
                //statuses.Value = statusIds;
                //var parameters = new NpgsqlParameter[2]
                //{
                //    statuses,
                //    new NpgsqlParameter("@userId", NpgsqlTypes.NpgsqlDbType.Integer, userId)
                //};
                return await connection.QueryAsync<ApplicationEntity>(
                    """
                        SELECT a.application_id, a.user_id, a.term_id, a.application_status_id, a.year, a.previous_ta, a.instructor_notes
                        FROM applications a
                        WHERE a.user_id = @userId and a.application_status_id IN (1, 2, 4);
                    """, new
                    {
                        userId = userId,
                        //statusIds = GetIntArray(statusIds),
                    });
            },
                mapError: (ex) => Error.Create(ErrorSource.ApplicationRepository, System.Net.HttpStatusCode.InternalServerError, ex.Message));

        private string GetIntArray(IEnumerable<int> ints) => 
            ints.Aggregate("", (a, b) => $"a, b");

    }
}
