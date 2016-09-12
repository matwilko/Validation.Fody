namespace Validation.Fody.Internals
{
    using System;
    using System.Reflection;

    internal static class WeaverGenerator
    {
        public static WeaverWrapper CreateWeaver(Injector injector, Type type, Type attributeType)
        {
            return (WeaverWrapper) typeof (WeaverGenerator).GetMethod(nameof(CreateWeaver), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(type, attributeType)
                .Invoke(null, new object[] {injector});
        }

        private static WeaverWrapper CreateWeaver<TWeaver, TAttribute>(Injector injector)
            where TWeaver : IAttributeWeaver<TAttribute>
            where TAttribute : Attribute
        {
            return WeaverWrapper.CreateFromWeaver(injector.Resolve<TWeaver>());
        }
    }
}
