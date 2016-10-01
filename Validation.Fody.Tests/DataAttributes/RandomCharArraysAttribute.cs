namespace Validation.Fody.Tests.DataAttributes
{
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class RandomCharArraysAttribute : CombinatorialValuesAttribute
    {
        public RandomCharArraysAttribute(int ofLength = 10, int number = 5, char[] alphabet = null)
            : base(GetStrings(ofLength, alphabet).Take(number).Select(s => s.ToCharArray()).Cast<object>().ToArray())
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