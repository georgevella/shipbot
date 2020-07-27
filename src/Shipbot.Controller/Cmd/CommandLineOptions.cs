using System.Collections.Generic;
using CommandLine;

namespace Shipbot.Controller.Cmd
{
    public class CommandLineOptions
    {
        [Option('c', "config", Required = false, HelpText = "Path to configuration file (in JSON).")]
        public string ConfigFilePath { get; set; }

        [Option('a', "applications", Required = false, Separator = ';', HelpText = "Paths to application configuration files. Accepts wildcards in file names. Application configuration can be YAML or JSON.")]
        public IEnumerable<string> ApplicationsFilePath { get; set; }
    }
}