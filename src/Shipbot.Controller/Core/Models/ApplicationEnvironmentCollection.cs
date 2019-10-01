using System.Collections.Generic;
using System.Linq;

namespace Shipbot.Controller.Core.Models
{
    public class ApplicationEnvironmentCollection : Dictionary<string, ApplicationEnvironment> 
    {
        public override int GetHashCode()
        {
            return this.Aggregate(0,
                (i, pair) =>
                {
                    var code = i;
                    code = (i * 987) ^ pair.Key.GetHashCode();
                    code = (i * 987) ^ pair.Value.GetHashCode();

                    return code;
                }
            );
        }
    }
}