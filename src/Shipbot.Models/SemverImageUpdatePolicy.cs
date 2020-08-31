namespace Shipbot.Models
{
    class SemverImageUpdatePolicy : ImageUpdatePolicy
    {
        public override bool IsMatch(string value)
        {
            throw new System.NotImplementedException();
        }

        public override bool IsGreaterThen(string left, string right)
        {
            throw new System.NotImplementedException();
        }
    }
}