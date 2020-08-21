using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Shipbot.Models
{
    public class Application
    {
        protected bool Equals(Application other)
        {
            return Name == other.Name && Images.Equals(other.Images) && AutoDeploy == other.AutoDeploy && Notifications.Equals(other.Notifications);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Application) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name.GetHashCode();
                hashCode = (hashCode * 397) ^ Images.GetHashCode();
                hashCode = (hashCode * 397) ^ AutoDeploy.GetHashCode();
                hashCode = (hashCode * 397) ^ Notifications.GetHashCode();
                return hashCode;
            }
        }

        public string Name { get; }
        public ImmutableList<Image> Images { get; }

        public bool AutoDeploy { get; }
        
        public NotificationSettings Notifications { get; }

        public Application(
            string name, 
            ImmutableList<Image> images,
            bool autoDeploy,
            NotificationSettings notifications
            )
        {
            Name = name;
            Images = images;
            AutoDeploy = autoDeploy;
            Notifications = notifications;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class NotificationSettings
    {
        private readonly HashSet<string> _channels;
        private readonly int _hash = 0;

        protected bool Equals(NotificationSettings other)
        {
            return Channels.Equals(other.Channels);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NotificationSettings) obj);
        }

        public override int GetHashCode()
        {
            return _hash;
        }

        public NotificationSettings(IEnumerable<string> channels = null)
        {
            _channels = channels?.ToHashSet() ?? new HashSet<string>();

            foreach (var channel in _channels)
            {
                _hash = (_hash * 397) ^ channel.GetHashCode();
            }
        }

        public NotificationSettings(string channel) 
            : this(new [] { channel } )
        {
        }

        public IEnumerable<string> Channels => _channels;
    }
}