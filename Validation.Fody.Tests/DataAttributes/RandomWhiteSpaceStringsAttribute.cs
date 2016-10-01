namespace Validation.Fody.Tests.DataAttributes
{
    public sealed class RandomWhiteSpaceStringsAttribute : RandomStringsAttribute
    {
        public RandomWhiteSpaceStringsAttribute(int ofLength = 10, int number = 5)
            : base(ofLength, number, Chars.WhiteSpace)
        {
        }
    }
}
