namespace Api.TaAssistant.Configuration
{
    /// <summary>
    /// The settings class for the application logger.
    /// </summary>
    public class ApplicationLogger
    {
        /// <summary>
        /// The configuration for the console sink.
        /// </summary>
        public Sink Console { get; set; } = new Sink();

        /// <summary>
        ///  The global minimum level for loggings.
        /// </summary>
        public string MinimumLevel { get; set; } = "Error";
    }

    /// <summary>
    /// A configuration used to describe settings for a serilog sink.
    /// </summary>
    public class Sink
    {
        /// <summary>
        /// Whether or not he sink is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The minimum level for the sink to log.
        /// </summary>
        public string MinimumLevel { get; set; } = "Error";
    }
}