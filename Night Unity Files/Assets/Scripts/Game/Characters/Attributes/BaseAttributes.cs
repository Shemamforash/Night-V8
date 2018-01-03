using System.Xml;
using Game.Characters.CharacterActions;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Persistence;
using Random = UnityEngine.Random;

namespace Game.Characters
{
    public class BaseAttributes : DesolationAttributes
    {
        public readonly CharacterAttribute Strength = new CharacterAttribute(AttributeType.Strength, 0);
        public readonly CharacterAttribute Perception = new CharacterAttribute(AttributeType.Perception, 0);
        public readonly CharacterAttribute Endurance = new CharacterAttribute(AttributeType.Endurance, 0);
        public readonly CharacterAttribute Willpower = new CharacterAttribute(AttributeType.Willpower, 0);
        

        protected override void RegisterTimedEvents()
        {
        }

        protected override void CacheAttributes()
        {
            AddAttribute(Strength);
            AddAttribute(Perception);
            AddAttribute(Endurance);
            AddAttribute(Willpower);
        }
        
        public float RemainingCarryCapacity()
        {
            return Strength.CurrentValue();
        }

        public override void Load(XmlNode doc, PersistenceType saveType)
        {
            LoadAttribute(doc, nameof(Strength), Strength);
            LoadAttribute(doc, nameof(Endurance), Endurance);
            LoadAttribute(doc, nameof(Willpower), Willpower);
            LoadAttribute(doc, nameof(Perception), Perception);
        }

        public override void Save(XmlNode doc, PersistenceType saveType)
        {
            SaveAttribute(doc, nameof(Strength), Strength);
            SaveAttribute(doc, nameof(Endurance), Endurance);
            SaveAttribute(doc, nameof(Willpower), Willpower);
            SaveAttribute(doc, nameof(Perception), Perception);
        }

        public BaseAttributes(Character character) : base(character)
        {
        }
    }
}