using System.Xml;
using SamsHelper.Persistence;

namespace Game.Characters
{
    public abstract class AttributeContainer : IPersistenceTemplate
    {
        public virtual void Load(XmlNode doc, PersistenceType saveType)
        {
            throw new System.NotImplementedException();
        }

        public virtual void Save(XmlNode doc, PersistenceType saveType)
        {
            throw new System.NotImplementedException();
        }
    }
}