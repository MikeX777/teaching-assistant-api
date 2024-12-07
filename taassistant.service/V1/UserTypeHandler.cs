using LanguageExt;
using MediatR;
using TaAssistant.Interfaces.Repositories;
using TaAssistant.Model.Api;
using TaAssistant.Model.Api.Responses;

namespace TaAssistant.Service.V1
{
    public record GetUserTypes() : IRequest<Either<ApiProblemDetails, IEnumerable<UserTypeResponse>>> { }
    
    public class UserTypeHandler :
        IRequestHandler<GetUserTypes, Either<ApiProblemDetails, IEnumerable<UserTypeResponse>>>
    {
        private readonly IUserTypeRepository userType;

        public UserTypeHandler(IUserTypeRepository userType) => this.userType = userType;

        public async Task<Either<ApiProblemDetails, IEnumerable<UserTypeResponse>>> Handle(GetUserTypes request, CancellationToken cancellationToken) =>
            await (
                from ut in Common.MapLeft(() => userType.GetUserTypes()).ToAsync()
                select ut.Select(i => new UserTypeResponse
                {
                    UserTypeId = i.UserTypeId,
                    Type = i.Type,
                })
            );

    }
}
