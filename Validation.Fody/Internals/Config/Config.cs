using Validation.Fody.Helpers;

namespace Validation.Fody.Internals.Config
{
    using System;
    using System.Xml.Linq;

    internal class Config : IConfig
    {
        private XElement GlobalConfigurationElement { get; }
        public XElement ConfigurationElement { get; }

        protected internal Config(Type requestingType, XElement configurationElement)
        {
            GlobalConfigurationElement = configurationElement.DetachClone();
            ConfigurationElement = GetInnerConfigurationElement(requestingType, GlobalConfigurationElement);
        }

        private static XElement EmptyXElement(string name) => new XElement(name);

        private static XElement GetInnerConfigurationElement(Type requestingType, XElement configurationElement)
        {
            var elementName = requestingType.Name.EndsWith("Weaver")
                ? requestingType.Name.Substring(0, requestingType.Name.Length - 6)
                : requestingType.Name;

            return configurationElement.Element(elementName)?.DetachClone() ?? EmptyXElement(elementName);
        }
    }
}