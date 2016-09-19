namespace ValidationAttributes.Strings
{
    using System;

    /// <summary>
    /// Indicates that the property or parameter should not contain an empty (zero-length) or null string
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class NotNullOrEmptyAttribute : Attribute
    {
    }
}
