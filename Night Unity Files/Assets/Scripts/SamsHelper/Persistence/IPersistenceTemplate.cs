using System.Xml;

namespace SamsHelper.Persistence
{
    public interface IPersistenceTemplate
    {
        void Load(XmlNode doc);
        XmlNode Save(XmlNode doc);
    }
}