namespace Validation.Fody.Tests.DataAttributes
{
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class RandomStringsAttribute : CombinatorialValuesAttribute
    {
        public RandomStringsAttribute(int ofLength = 10, int number = 5, char[] alphabet = null)
            : base(GetStrings(ofLength, alphabet).Take(number).Cast<object>().ToArray())
        {
        }

        private static IEnumerable<string> GetStrings(int ofLength, char[] alphabet)
        {
            return alphabet == null
                ? Helpers.RandomStringsOfLength(ofLength)
                : Helpers.RandomStringsOfLength(ofLength, alphabet);
        }
    }
}
