namespace Validation.Fody.Tests.DataAttributes
{
    using System.Linq;
    using Xunit;

    public sealed class RandomCharArrayAttribute : CombinatorialValuesAttribute
    {
        public RandomCharArrayAttribute(int ofLength = 10, int number = 5)
            : base(Helpers.RandomStringsOfLength(ofLength).Take(number).Select(s => s.ToCharArray()).Cast<object>().ToArray())
        {
        }
    }
}