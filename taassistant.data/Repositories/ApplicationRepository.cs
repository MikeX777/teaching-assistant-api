using Dapper;
using LanguageExt;
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
                    SELECT as.application_status_id, as.status
                    FROM application_statuses as
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
                        applicationStatusId = request.ApplicationStatusId,
                        year = request.Year,
                        previousTA = request.PreviousTA
                    });
                return Unit.Default;
            }, mapError: (ex) => Error.Create(ErrorSource.ApplicationRepository, System.Net.HttpStatusCode.InternalServerError, ex.Message));

    }
}
