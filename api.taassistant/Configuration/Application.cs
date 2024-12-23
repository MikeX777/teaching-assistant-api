﻿namespace Api.TaAssistant.Configuration
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

        /// <summary>
        /// The connection string for the data base.
        /// </summary>
        public string DefaultConnectionString { get; set; } = configuration.GetValue<string>("default-c2807") ?? string.Empty;

        public string EmailConnectionString { get; set; } = configuration.GetValue<string>("emailConnectionString") ?? string.Empty;
        public string StorageConnectionString { get; set; } = configuration.GetValue<string>("storageConnectionString") ?? string.Empty;
        public string Pepper { get; set; } = configuration.GetValue<string>("pepper") ?? string.Empty;
    }
}
