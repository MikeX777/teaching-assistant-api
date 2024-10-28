namespace TaAssistant.Model.Extensions
{
    /// <summary>
    /// Async Helper to help running async methods.
    /// </summary>
    public static class AsyncHelper
    {
        private static readonly TaskFactory taskFactory = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        /// <summary>
        /// A method to run an asynchronous method synchronously.
        /// </summary>
        /// <typeparam name="TResult">The return type of the method.</typeparam>
        /// <param name="func">The function to run synchronously.</param>
        /// <returns>The result of the function that is passed in.</returns>
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
            => taskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();

        /// <summary>
        /// A method to run a asynchronous method without a return type synchronously.
        /// </summary>
        /// <param name="func">The function to run synchronously.</param>
        public static void RunSync(Func<Task> func)
            => taskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();
    }
}