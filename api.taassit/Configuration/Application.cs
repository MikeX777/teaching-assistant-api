namespace Api.TaAssistant.Configuration
{
    /// <summary>
    /// Configuration class for the application.
    /// </summary>
    /// <param name="configuration">The configuration interface used to retrieve application configuration.</param>
    public class Application(IConfiguration configuration)
    {
        /// <summary>
        /// The name of the application.
        /// </summary>
        public string Name { get; set; } = configuration.GetValue<string>("Name") ?? string.Empty;
    }
}
