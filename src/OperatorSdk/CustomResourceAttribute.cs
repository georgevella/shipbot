using System;

namespace OperatorSdk
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomResourceAttribute : Attribute
    {
        public CustomResourceAttribute(string apiGroup, string version, string plural)
        {
            ApiGroup = apiGroup;
            Version = version;
            Plural = plural;
        }

        public string ApiGroup { get; set; }
        
        public string Version { get; set; }
        
        public string Plural { get; set; }
    }
}