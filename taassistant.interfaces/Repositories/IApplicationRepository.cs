using LanguageExt;
using TaAssistant.Model;
using TaAssistant.Model.Api.Requests;
using TaAssistant.Model.Entities;

namespace TaAssistant.Interfaces.Repositories
{
    public interface IApplicationRepository
    {
        Task<Either<Error, IEnumerable<TermEntity>>> GetTerms();
        Task<Either<Error, IEnumerable<ApplicationStatusEntity>>> GetApplicationStatuses();
        Task<Either<Error, Unit>> SubmitApplication(SubmitApplicationRequest request);
    }
}
