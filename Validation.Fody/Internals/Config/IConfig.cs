namespace Validation.Fody.Internals.Config
{
    using System.Xml.Linq;

    internal interface IConfig
    {
        XElement ConfigurationElement { get; }
    }
}
