using System.Xml;

namespace SamsHelper.Persistence
{
    public interface IPersistenceTemplate
    {
        void Load(XmlNode doc, PersistenceType saveType);
        XmlNode Save(XmlNode doc, PersistenceType saveType);
    }
}