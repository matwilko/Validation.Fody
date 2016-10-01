namespace Validation.Fody.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class Helpers
    {
        private static Random Random { get; } = new Random();

        public static IEnumerable<int> RandomValuesBetween(int lowerBound, int upperBound)
        {
            while (true)
            {
                yield return Random.Next(lowerBound, upperBound);
            }
        }

        public static int RandomValueBetween(int lowerBound, int upperBound)
        {
            return RandomValuesBetween(lowerBound, upperBound).First();
        }

        public static IEnumerable<string> RandomStringsOfLength(int length)
        {
            var buffer = new byte[length];
            var characters = buffer.Select(b => (char)b);
            while (true)
            {
                Random.NextBytes(buffer);
                yield return new string(characters.ToArray());
            }
        }

        public static IEnumerable<string> RandomStringsOfLength(int length, char[] alphabet)
        {
            var buffer = new byte[length];
            var characters = buffer.Select(b => alphabet[b % alphabet.Length]);
            while (true)
            {
                Random.NextBytes(buffer);
                yield return new string(characters.ToArray());
            }
        }

        public static string RandomStringOfLength(int length) => RandomStringsOfLength(length).First();

        public static IEnumerable<object[]> AsTestArguments<T>(this IEnumerable<T> sequence) => sequence.Select(item => AsTestArguments(item));

        public static object[] AsTestArguments<T>(T item) => new object[] { item };
        public static object[] AsTestArguments<T1, T2>(T1 item1, T2 item2) => new object[] { item1, item2 };
        public static object[] AsTestArguments<T1, T2, T3>(T1 item1, T2 item2, T3 item3) => new object[] { item1, item2, item3 };

    }
}
