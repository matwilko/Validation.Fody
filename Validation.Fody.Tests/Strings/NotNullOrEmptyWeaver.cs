namespace Validation.Fody.Tests.Strings
{
    using System;
    using System.Linq;
    using DataAttributes;
    using Compilation;
    using Xunit;

    public sealed class NotNullOrEmptyWeaver
    {
        [Theory, CombinatorialData]
        public void RejectsNullString(MemberType memberType)
        {
            var result = Transform.FromAttributeSource<string>(memberType, "[ValidationAttributes.Strings.NotNullOrEmptyAttribute]");

            Assert.True(result.Success);
            Assert.Throws<ArgumentNullException>(() => result.Action(null));
        }

        [Theory, CombinatorialData]
        public void RejectsNullCharArray(MemberType memberType)
        {
            var result = Transform.FromAttributeSource<char[]>(memberType, "[ValidationAttributes.Strings.NotNullOrEmptyAttribute]");

            Assert.True(result.Success);
            Assert.Throws<ArgumentNullException>(() => result.Action(null));
        }

        [Theory, CombinatorialData]
        public void RejectsEmptyStrings(MemberType memberType)
        {
            var result = Transform.FromAttributeSource<string>(memberType, "[ValidationAttributes.Strings.NotNullOrEmptyAttribute]");

            Assert.True(result.Success);
            Assert.Throws<ArgumentException>(() => result.Action(string.Empty));
        }

        [Theory, CombinatorialData]
        public void RejectsEmptyCharArray(MemberType memberType)
        {
            var result = Transform.FromAttributeSource<char[]>(memberType, "[ValidationAttributes.Strings.NotNullOrEmptyAttribute]");

            Assert.True(result.Success);
            Assert.Throws<ArgumentException>(() => result.Action(new char[0]));
        }

        [Theory, CombinatorialData]
        public void AcceptsRandomNonEmptyStrings(MemberType memberType, [RandomStrings] string testString)
        {
            var result = Transform.FromAttributeSource<string>(memberType, "[ValidationAttributes.Strings.NotNullOrEmptyAttribute]");

            Assert.True(result.Success);
            result.Action(testString);
        }

        [Theory, CombinatorialData]
        public void AcceptsRandomNonEmptyCharArrays(MemberType memberType, [RandomCharArrays] char[] testChars)
        {
            var result = Transform.FromAttributeSource<char[]>(memberType, "[ValidationAttributes.Strings.NotNullOrEmptyAttribute]");

            Assert.True(result.Success);
            result.Action(testChars);
        }

        [Theory, CombinatorialData]
        public void CompilationFailsOnNonStringOrCharArrayType(MemberType memberType, [Types(typeof(string), typeof(char[]))] Type type)
        {
            var result = Transform.FromAttributeSource(memberType, type, "[ValidationAttributes.Strings.NotNullOrEmptyAttribute]");
            Assert.False(result.Success);

            Assert.Single(result.BuildMessages);
            Assert.StrictEqual(result.BuildMessages.Single().Level, MessageLevel.Error);
            Assert.Contains("cannot be applied to", result.BuildMessages.Single().Message);
        }

        [Theory, CombinatorialData]
        public void CompilationSucceedsOnStringOrCharArrayType(MemberType memberType, [CombinatorialValues(typeof(string), typeof(char[]))] Type type)
        {
            var result = Transform.FromAttributeSource(memberType, type, "[ValidationAttributes.Strings.NotNullOrEmptyAttribute]");
            Assert.True(result.Success);
        }
    }
}
