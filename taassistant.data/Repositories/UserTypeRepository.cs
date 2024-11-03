using Dapper;
using LanguageExt;
using System.Data;
using TaAssistant.Interfaces.Repositories;
using TaAssistant.Model;
using TaAssistant.Model.Entities;
using static TaAssistant.Data.BaseRepository;

namespace TaAssistant.Data.Repositories
{
    public class UserTypeRepository : IUserTypeRepository
    {
        private readonly IDbConnection connection;

        public UserTypeRepository(IDbConnection connection) => this.connection = connection;

        public async Task<Either<Error, IEnumerable<UserTypeEntity>>> GetUserTypes() =>
            await TryFuncCatchExceptionAsync(async () => await connection.QueryAsync<UserTypeEntity>(
                """
                    SELECT ut.user_type_id, ut.type
                    FROM user_types ut
                """),
                mapError: (ex) => Error.Create(ErrorSource.UserTypeRepository, System.Net.HttpStatusCode.InternalServerError, ex.Message));
    }
}
