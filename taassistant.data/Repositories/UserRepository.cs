using Dapper;
using LanguageExt;
using Microsoft.AspNetCore.Identity.Data;
using System.Data;
using TaAssistant.Interfaces.Repositories;
using TaAssistant.Model;
using TaAssistant.Model.Api.Requests;
using TaAssistant.Model.Entities;
using static TaAssistant.Data.BaseRepository;

namespace TaAssistant.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnection connection;

        public UserRepository(IDbConnection connection) => this.connection = connection;

        public async Task<Either<Error, Unit>> CreateUser(CreateUserRequest request) =>
            await TryFuncCatchExceptionAsync(async () => {
                await connection.ExecuteAsync(
                    """
                INSERT INTO users
                    (email, given_name, family_name, password, password_salt, pending, user_type_id, created_at)
                    VALUES
                    (@email, @givenName, @familyName, @password, @passwordSalt, @pending, @userTypeId, @createdAt);
                """, new
                    {
                        email = request.Email,
                        givenName = request.GivenName,
                        familyName = request.FamilyName,
                        password = request.Password,
                        passwordSalt = request.PasswordSalt,
                        pending = true,
                        userTypeId = request.UserTypeId,
                        createdAt = DateTimeOffset.UtcNow,
                    });
                return LanguageExt.Unit.Default;
                },
                 mapError: (ex) => Error.Create(ErrorSource.UserTypeRepository, System.Net.HttpStatusCode.InternalServerError, ex.Message));

        public async Task<Either<Error, FullUserEntity>> GetFullUser(string email) =>
            await TryFuncCatchExceptionAsync(async () => await connection.QuerySingleAsync<FullUserEntity>(
                """
                SELECT u.user_id, u.email, u.given_name, u.family_name, u.password, u.password_salt, u.pending, u.user_type_id, u.verification_code, u.verification_expiration, u.created_at
                FROM users AS u
                WHERE u.email = @email
                """, new { email = email }),
                mapError: (ex) => Error.Create(ErrorSource.UserTypeRepository, System.Net.HttpStatusCode.InternalServerError, ex.Message));

        public Task<Either<Error, UserEntity>> Login(LoginRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<Either<Error, string>> SignIn(string email, string passwordHash) =>
            await TryFuncCatchExceptionAsync(async () =>
            {
                var userId = await connection.QuerySingleAsync<int>("SELECT u.user_id FROM users AS u WHERE u.email = @email AND u.password = @passwordHash",
                    new { email = email, passwordHash = passwordHash });
                var verificationCode = RandomString(8);
                await connection.ExecuteAsync("UPDATE users AS u SET u.verification_code = @verificationCode, u.verification_expiration = @verificationExpiration WHERE u.user_id = @userId",
                    new { userId = userId, verificationCode = verificationCode, verificationExpiration = DateTimeOffset.UtcNow.AddMinutes(15) });
                return verificationCode;
            },
            mapError: (ex) => Error.Create(ErrorSource.UserTypeRepository, System.Net.HttpStatusCode.InternalServerError, ex.Message));

        private static string RandomString(int length)
        {
            var random = new Random((int)DateTime.Now.Ticks);
            const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
            var chars = Enumerable.Range(0, length)
                .Select(x => pool[random.Next(0, pool.Length)]);
            return new string(chars.ToArray());
        }


    }
}
