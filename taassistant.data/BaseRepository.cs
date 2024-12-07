using LanguageExt;
using TaAssistant.Model;

namespace TaAssistant.Data
{
    internal static class BaseRepository
    {
        internal static async Task<Either<Error, T>> TryFuncCatchExceptionAsync<T>(Func<Task<T>> func,
            Func<Exception, Error> mapError)
        {
            try
            {
                return await func();
            }
            catch (Exception e)
            {
                return mapError(e);
            }
        }

        internal static async Task<Either<Error, T>> TryFuncCatchExceptionAsync<T>(Func<Task<Either<Error, T>>> func,
            Func<Exception, Error> mapError)
        {
            try
            {
                return await (from output in func() select output);
            }
            catch (Exception e)
            {
                return mapError(e);
            }
        }

        internal static Either<Error, T> TryFuncCatchException<T>(Func<T> func, Func<Exception, Error> mapError)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                return mapError(e);
            }
        }
    }
}
