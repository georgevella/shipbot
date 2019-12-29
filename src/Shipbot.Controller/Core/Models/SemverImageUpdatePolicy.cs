namespace Shipbot.Controller.Core.Models
{
    class SemverImageUpdatePolicy : ImageUpdatePolicy
    {
        public override bool IsMatch(string value)
        {
            throw new System.NotImplementedException();
        }

        public override bool IsGreaterThen(string tag, string currentTag)
        {
            throw new System.NotImplementedException();
        }
    }
}