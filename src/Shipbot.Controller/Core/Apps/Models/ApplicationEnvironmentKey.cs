using System;
using System.Collections.Generic;

namespace Shipbot.Controller.Core.Apps.Models
{
    public class ApplicationEnvironmentKey
    {
        private sealed class ApplicationEnvironmentEqualityComparer : IEqualityComparer<ApplicationEnvironmentKey>
        {
            public bool Equals(ApplicationEnvironmentKey x, ApplicationEnvironmentKey y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Application == y.Application && x.Environment == y.Environment;
            }

            public int GetHashCode(ApplicationEnvironmentKey obj)
            {
                unchecked
                {
                    return (obj.Application.GetHashCode() * 397) ^ obj.Environment.GetHashCode();
                }
            }
        }

        public static IEqualityComparer<ApplicationEnvironmentKey> EqualityComparer { get; } = new ApplicationEnvironmentEqualityComparer();

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