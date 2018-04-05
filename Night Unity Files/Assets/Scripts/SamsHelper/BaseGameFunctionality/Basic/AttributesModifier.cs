namespace SamsHelper.BaseGameFunctionality.Basic
{
//    public class AttributesModifier
//    {
//        protected readonly Dictionary<AttributeType, float> SummativeModifiers = new Dictionary<AttributeType, float>();
//        protected readonly Dictionary<AttributeType, float> MultiplicativeModifiers = new Dictionary<AttributeType, float>();
//        private bool _applied;
//        
//        public void Apply(AttributeContainer attributes)
//        {
//            if (_applied) return;
//            foreach (AttributeType attributeType in SummativeModifiers.Keys)
//            {
//                attributes.Get(attributeType).AddModifier(SummativeModifiers[attributeType], true);
//            }
//            foreach (AttributeType attributeType in MultiplicativeModifiers.Keys)
//            {
//                attributes.Get(attributeType).AddModifier(MultiplicativeModifiers[attributeType]);
//            }
//            _applied = true;
//        }
//
//        public void Remove(AttributeContainer attributes)
//        {
//            if (!_applied) return;
//            foreach (AttributeType attributeType in SummativeModifiers.Keys)
//            {
//                attributes.Get(attributeType).RemoveModifier(SummativeModifiers[attributeType], true);
//            }
//            foreach (AttributeType attributeType in MultiplicativeModifiers.Keys)
//            {
//                attributes.Get(attributeType).RemoveModifier(MultiplicativeModifiers[attributeType]);
//            }
//            _applied = false;
//        }
//
//        public void AddModifier(AttributeType type, float value, bool summative = false)
//        {
//            if (summative) SummativeModifiers[type] = value;
//            else MultiplicativeModifiers[type] = value;
//        }
//    }
}