namespace Shipbot.Controller.Core.Configuration
{
    public class SlackConfiguration
    {
        /// <summary>
        ///     Request timeout in milliseconds.
        /// </summary>
        public int Timeout { get; set; } = 60000;
        
        public string Token { get; set; }
    }
}