namespace Validation.Fody.Tests.DataAttributes
{
    using System.Linq;
    using Xunit;

    public sealed class RandomStringsAttribute : CombinatorialValuesAttribute
    {
        public RandomStringsAttribute(int ofLength = 10, int number = 5)
            : base(Helpers.RandomStringsOfLength(ofLength).Take(number).Cast<object>().ToArray())
        {
        }
    }
}
