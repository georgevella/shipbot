using CommandLine;

namespace Shipbot.Controller.Cmd
{
    public class CommandLineOptions
    {
        [Option('c', "config", Required = false, HelpText = "Path to configuration file (in JSON).")]
        public string ConfigFilePath { get; set; }
    }
}