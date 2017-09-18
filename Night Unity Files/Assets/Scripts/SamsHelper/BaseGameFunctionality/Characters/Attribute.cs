using SamsHelper.ReactiveUI.CustomTypes;

namespace Game.Gear
{
    public class Attribute
    {
        private MyInt _value;
        private AttributeType _attributeType;

        public Attribute(AttributeType attributeType, MyInt value)
        {
            _attributeType = attributeType;
            _value = value;
        }

        public MyInt Value()
        {
            return _value;
        }
    }
}