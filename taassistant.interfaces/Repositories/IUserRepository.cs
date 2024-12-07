using LanguageExt;
using Microsoft.AspNetCore.Identity.Data;
using TaAssistant.Model;
using TaAssistant.Model.Api.Requests;
using TaAssistant.Model.Entities;

namespace TaAssistant.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<Either<Error, int>> CreateUser(CreateUserRequest request, string passwordSalt);
        Task<Either<Error, FullUserEntity>> GetFullUser(string email);
        Task<Either<Error, string>> SignIn(string email, string passwordHash);
        Task<Either<Error, UserEntity>> Login(LoginRequest request);
        Task<Either<Error, Unit>> UserDoesNotExist(string email);
    }
}
