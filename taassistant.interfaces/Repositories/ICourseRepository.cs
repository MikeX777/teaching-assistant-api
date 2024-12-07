using LanguageExt;
using TaAssistant.Model;
using TaAssistant.Model.Entities;

namespace TaAssistant.Interfaces.Repositories
{
    public interface ICourseRepository
    {
        Task<Either<Error, IEnumerable<CourseEntity>>> GetCourses();
        Task<Either<Error, IEnumerable<GradeEntity>>> GetGrades();
    }
}
