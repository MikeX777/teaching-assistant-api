using LanguageExt;
using MediatR;
using System.Security.Cryptography;
using System.Text;
using TaAssistant.Interfaces.Repositories;
using TaAssistant.Model;
using TaAssistant.Model.Api;
using TaAssistant.Model.Api.Requests;
using TaAssistant.Model.Entities;

namespace TaAssistant.Service.V1
{
    public record CreateUser(CreateUserRequest User) : IRequest<Either<ApiProblemDetails, LanguageExt.Unit>> { }

    public class UserHandler :
        IRequestHandler<CreateUser, Either<ApiProblemDetails, LanguageExt.Unit>>
    {
        IUserRepository user;
        IUserTypeRepository userType;
        string pepper;

        public UserHandler(IUserRepository user, IUserTypeRepository userType, string pepper) =>
            (this.user, this.userType, this.pepper) = (user, userType, pepper);

        public async Task<Either<ApiProblemDetails, LanguageExt.Unit>> Handle(CreateUser request, CancellationToken cancellationToken) =>
            await (
                from ut in Common.MapLeft(() => userType.GetUserTypes()).ToAsync()
                from _ in Common.MapLeft(() => validateUserType(ut, request.User.UserTypeId)).ToAsync()
                from r in Common.MapLeft(() => updatePassword(request.User)).ToAsync()
                from u in Common.MapLeft(() => user.CreateUser(request.User)).ToAsync()
                select LanguageExt.Unit.Default
            );

        private Either<Error, LanguageExt.Unit> validateUserType(IEnumerable<UserTypeEntity> userTypes, int userTypeId) =>
            userTypes.Any(ut => ut.UserTypeId == userTypeId) ? 
                LanguageExt.Unit.Default : 
                Error.Create(
                    ErrorSource.UserHandler, 
                    System.Net.HttpStatusCode.BadRequest,
                    "User Type does not exist",
                    ErrorType.Validation);

        private Either<Error, CreateUserRequest> updatePassword(CreateUserRequest request)
        {
            var saltData = RandomNumberGenerator.GetBytes(64);
            request.PasswordSalt = Convert.ToBase64String(saltData);
            request.Password = Convert.ToBase64String(SHA512.HashData(UTF8Encoding.UTF8.GetBytes(request.Password + pepper + request.PasswordSalt)));
            return request;

        }
    }
}
