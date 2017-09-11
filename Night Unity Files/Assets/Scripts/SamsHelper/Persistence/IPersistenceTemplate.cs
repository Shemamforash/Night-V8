using System.Xml;

namespace SamsHelper.Persistence
{
    public interface IPersistenceTemplate
    {
        void Load(XmlNode doc, PersistenceType saveType);
        void Save(XmlNode doc, PersistenceType saveType);
    }
}