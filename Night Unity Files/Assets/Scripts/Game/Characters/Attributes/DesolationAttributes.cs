using System.Xml;
using Facilitating.Persistence;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Persistence;

namespace Game.Characters.CharacterActions
{
    public abstract class DesolationAttributes : AttributeContainer
    {
        protected readonly Character Character;
        
        protected abstract void RegisterTimedEvents();
        protected abstract override void CacheAttributes();
        public abstract void Load(XmlNode doc, PersistenceType saveType);
        public abstract void Save(XmlNode doc, PersistenceType saveType);

        public DesolationAttributes(Character character)
        {
            Character = character;
            RegisterTimedEvents();
        }
        
        protected void LoadAttribute(XmlNode root, string attributeName, CharacterAttribute characterAttribute)
        {
            XmlNode attributeNode = root.SelectSingleNode(attributeName);
            XmlNode maxNode = attributeNode.SelectSingleNode("Max");
            characterAttribute.Max = SaveController.ParseIntFromSubNode(maxNode);
            XmlNode valNode = attributeNode.SelectSingleNode("Val");
            characterAttribute.SetCurrentValue(SaveController.ParseIntFromSubNode(valNode));
        }
        
        protected void SaveAttribute(XmlNode root, string attributeName, CharacterAttribute characterAttribute)
        {
            XmlNode attributeNode = SaveController.CreateNodeAndAppend(attributeName, root);
            SaveController.CreateNodeAndAppend("Val", attributeNode, characterAttribute.Max);
            SaveController.CreateNodeAndAppend("Max", attributeNode, characterAttribute.CurrentValue());
        }
    }
}