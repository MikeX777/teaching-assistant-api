using LanguageExt;
using TaAssistant.Model;
using TaAssistant.Model.Entities;

namespace TaAssistant.Interfaces.Repositories
{
    public interface IUserTypeRepository
    {
        Task<Either<Error, IEnumerable<UserTypeEntity>>> GetUserTypes();
    }
}
