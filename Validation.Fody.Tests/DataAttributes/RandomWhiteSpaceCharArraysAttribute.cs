namespace Validation.Fody.Tests.DataAttributes
{
    public sealed class RandomWhiteSpaceCharArraysAttribute : RandomCharArraysAttribute
    {
        public RandomWhiteSpaceCharArraysAttribute(int ofLength = 10, int number = 5)
            : base(ofLength, number, Chars.WhiteSpace)
        {
        }
    }
}