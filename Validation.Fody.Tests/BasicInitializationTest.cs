namespace Validation.Fody.Tests
{
    using Validation.Fody.Tests.Compilation;
    using Xunit;

    public sealed class BasicInitializationTest
    {
        [Fact]
        public void AllWeaversLoadWithoutException()
        {
            Transform.FromSource(@"namespace A.B { public class C { } }");
        }
    }
}
