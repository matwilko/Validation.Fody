namespace ValidationAttributes
{
    using System;

    /// <summary>
    /// Indicates that the property or parameter should not allow a null reference
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class NotNullAttribute : Attribute
    {
    }
}
