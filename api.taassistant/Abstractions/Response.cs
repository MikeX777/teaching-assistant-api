namespace Api.TaAssistant.Abstractions
{
    /// <summary>
    /// Default Response Object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Response<T>
    {
        /// <summary>
        /// Whether or not the response is successful
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// The data that pertains to the response.
        /// </summary>
        public T Data { get; set; }
    }
}
