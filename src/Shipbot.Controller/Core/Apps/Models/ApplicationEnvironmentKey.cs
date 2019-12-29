using System;

namespace Shipbot.Controller.Core.Apps.Models
{
    public class ApplicationEnvironmentKey
    {
        public string Application { get; }
        
        public string Environment { get; }

        public ApplicationEnvironmentKey(string application, string environment)
        {
            Application = application;
            Environment = environment;
        }

        public static implicit operator string(ApplicationEnvironmentKey key)
        {
            return $"{key.Application}:{key.Environment}";
        }

        public static implicit operator ApplicationKey(ApplicationEnvironmentKey key)
        {
            return key.Application;
        }

        public static ApplicationEnvironmentKey Parse(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (!key.Contains(':'))
                throw new ArgumentException("Key is not in the correct format");

            var parts = key.Split(':');
            return new ApplicationEnvironmentKey(parts[0], parts[1]);
        }
    }
}