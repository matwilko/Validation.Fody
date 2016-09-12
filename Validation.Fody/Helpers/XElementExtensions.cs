namespace Validation.Fody.Helpers
{
    using System.Xml.Linq;

    internal static class XElementExtensions
    {
        public static XElement DetachClone(this XElement element)
        {
            using (var reader = element.CreateReader())
            {
                return XElement.Load(reader);
            }
        }
    }
}
