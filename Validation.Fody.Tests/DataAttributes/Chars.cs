namespace Validation.Fody.Tests.DataAttributes
{
    internal static class Chars
    {
        public static char[] WhiteSpace { get; } =
        {
            '\x0020', '\x1680', '\x2000', '\x2001',
            '\x2002', '\x2003', '\x2004', '\x2005',
            '\x2006', '\x2007', '\x2008', '\x2009',
            '\x200A', '\x202F', '\x205F', '\x3000',
            '\x2028', '\x0009', '\x000A', '\x000B',
            '\x000C', '\x000D', '\x0085', '\x00A0'
        };
    }
}
