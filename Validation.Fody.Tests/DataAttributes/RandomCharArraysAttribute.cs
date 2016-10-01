namespace Validation.Fody.Tests.DataAttributes
{
    using System.Linq;
    using Xunit;

    public class RandomCharArraysAttribute : CombinatorialValuesAttribute
    {
        public RandomCharArraysAttribute(int ofLength = 10, int number = 5)
            : base(Helpers.RandomStringsOfLength(ofLength).Take(number).Select(s => s.ToCharArray()).Cast<object>().ToArray())
        {
        }
    }
}