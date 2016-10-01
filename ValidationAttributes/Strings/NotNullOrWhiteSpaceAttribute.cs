namespace ValidationAttributes.Strings
{
    using System;

    /// <summary>
    /// Indicates that the property or parameter should not be null or contain only whitespace characters
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class NotNullOrWhiteSpaceAttribute : Attribute
    {
    }
}
