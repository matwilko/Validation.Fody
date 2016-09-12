namespace Validation.Fody.Tests.NotNullWeaver
{
    using System;
    using System.Linq;
    using DataAttributes;
    using Compilation;
    using Xunit;

    public sealed class NotNullWeaver
    {
        [Theory, CombinatorialData]
        public void CompilationSucceedsOnReferenceType(MemberType memberType, [ReferenceTypes] Type type)
        {
            var result = Transform.FromAttributeSource(memberType, type, "[ValidationAttributes.NotNull]");
            Assert.True(result.Success);
        }

        [Theory, CombinatorialData]
        public void CompilationSucceedsOnNullableValueType(MemberType memberType, [NullableValueTypes] Type type)
        {
            var result = Transform.FromAttributeSource(memberType, type, "[ValidationAttributes.NotNull]");
            Assert.True(result.Success);
        }

        [Theory, CombinatorialData]
        public void CompilationFailsOnNonNullableValueType(MemberType memberType, [ValueTypes(excludeNullables: true)] Type type)
        {
            var result = Transform.FromAttributeSource(memberType, type, "[ValidationAttributes.NotNull]");
            Assert.False(result.Success);

            Assert.Single(result.BuildMessages);
            Assert.StrictEqual(result.BuildMessages.Single().Level, MessageLevel.Error);
            Assert.Contains("cannot be applied to", result.BuildMessages.Single().Message);
        }

        [Theory, CombinatorialData]
        public void RejectsNull(MemberType memberType, [NullableTypes] Type type)
        {
            var result = Transform.FromAttributeSource(memberType, type, "[ValidationAttributes.NotNull]");

            Assert.True(result.Success);

            var method = result.GetAction(memberType, type);

            Assert.Throws<ArgumentNullException>(() => method(null));
        }

        [Theory, CombinatorialData]
        public void AcceptsNonNull(MemberType memberType, [NullableTypes] Type type)
        {
            var result = Transform.FromAttributeSource(memberType, type, "[ValidationAttributes.NotNull]");

            Assert.True(result.Success);

            var method = result.GetAction(memberType, type);

            // No assertion, just as long as there's no exception to fail the test
            method(ValueHelper.RandomValueFor(type));
        }
    }
}
