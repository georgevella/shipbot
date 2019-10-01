using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Shipbot.Controller.Core.Models
{
    public class ApplicationEnvironment
    {
        protected bool Equals(ApplicationEnvironment other)
        {
            return _hashCode == other._hashCode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApplicationEnvironment) obj);
        }

        private readonly int _hashCode = 0;

        public ApplicationEnvironment(string name, ImmutableList<Image> images, ApplicationSource source, bool autoDeploy, List<string> promotionEnvironments)
        {
            Name = name;
            Images = images;
            Source = source;
            AutoDeploy = autoDeploy;
            PromotionEnvironments = promotionEnvironments;
            
            unchecked
            {
                _hashCode = Images.Aggregate(0, (tempHashCode, image) => tempHashCode * 397 ^ image.GetHashCode());
                _hashCode = (_hashCode * 397) ^ PromotionEnvironments.Aggregate(0, (tempHashCode, env) => tempHashCode * 397 ^ env.GetHashCode());
                _hashCode = (_hashCode * 397) ^ Source.GetHashCode();
                _hashCode = (_hashCode * 397) ^ AutoDeploy.GetHashCode();
                _hashCode = (_hashCode * 397) ^ Name.GetHashCode();
            }
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }
        
        public string Name { get; }
        public ImmutableList<Image> Images { get; }

        public ApplicationSource Source { get; }
        
        public bool AutoDeploy { get; }
        
        public List<string> PromotionEnvironments { get; }
    }
}