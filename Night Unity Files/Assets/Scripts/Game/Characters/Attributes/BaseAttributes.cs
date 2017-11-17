using System.Xml;
using Game.Characters.CharacterActions;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Persistence;
using Random = UnityEngine.Random;

namespace Game.Characters
{
    public class BaseAttributes : DesolationAttributes
    {
        public readonly CharacterAttribute Strength = new CharacterAttribute(AttributeType.Strength, Random.Range(30, 70));
        public readonly CharacterAttribute Intelligence = new CharacterAttribute(AttributeType.Intelligence, Random.Range(30, 70));
        public readonly CharacterAttribute Endurance = new CharacterAttribute(AttributeType.Endurance, Random.Range(30, 70));
        public readonly CharacterAttribute Stability = new CharacterAttribute(AttributeType.Stability, Random.Range(30, 70));
        

        protected override void RegisterTimedEvents()
        {
        }

        protected override void CacheAttributes()
        {
            AddAttribute(Strength);
            AddAttribute(Intelligence);
            AddAttribute(Endurance);
            AddAttribute(Stability);
        }
        
        public float RemainingCarryCapacity()
        {
            return Strength.GetCurrentValue();
        }

        public override void Load(XmlNode doc, PersistenceType saveType)
        {
            LoadAttribute(doc, nameof(Strength), Strength);
            LoadAttribute(doc, nameof(Endurance), Endurance);
            LoadAttribute(doc, nameof(Stability), Stability);
            LoadAttribute(doc, nameof(Intelligence), Intelligence);
        }

        public override void Save(XmlNode doc, PersistenceType saveType)
        {
            SaveAttribute(doc, nameof(Strength), Strength);
            SaveAttribute(doc, nameof(Endurance), Endurance);
            SaveAttribute(doc, nameof(Stability), Stability);
            SaveAttribute(doc, nameof(Intelligence), Intelligence);
        }

        public BaseAttributes(Character character) : base(character)
        {
        }
    }
}