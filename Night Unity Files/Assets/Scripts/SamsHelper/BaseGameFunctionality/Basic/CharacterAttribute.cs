using System.Xml;
using Facilitating.Persistence;
using NUnit.Framework;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public class CharacterAttribute : Number
    {
        public readonly AttributeType AttributeType;
        private float _addMod;
        private float _multMod = 1;
        private float _calculatedValue;

        public CharacterAttribute(AttributeType attributeType, float value, float min = 0, float max = float.MaxValue) : base(value, min, max)
        {
            AttributeType = attributeType;
            AddOnValueChange(a => Recalculate());
        }
        
        public void Save(XmlNode doc, PersistenceType saveType)
        {
            SaveController.CreateNodeAndAppend("AttributeType", doc, AttributeType);
            SaveController.CreateNodeAndAppend("SummativeModifier", doc, _addMod);
            SaveController.CreateNodeAndAppend("MultiplicativeModifier", doc, _multMod);
        }

        public void DecrementMax()
        {
            --Max;
            if (CurrentValue() > Max)
            {
                SetCurrentValue(Max);
            }
        }

        public void IncrementMax()
        {
            ++Max;
            SetCurrentValue(CurrentValue());
        }
        
        public override float CurrentValue()
        {
            return _calculatedValue;
        }

        private float OriginalValue()
        {
            return base.CurrentValue();
        }

        private void Recalculate()
        {
            _calculatedValue = (OriginalValue() + _addMod) * _multMod;
            Assert.IsTrue(_multMod >= 0);
            Assert.IsTrue(_addMod + OriginalValue() >= 0);
            if (_calculatedValue > Max) _calculatedValue = Max;
            if (_calculatedValue < Min) _calculatedValue = Min;
        }

        public void ApplyAddMod(float addMod)
        {
            _addMod += addMod;
            Recalculate();
        }

        public void ApplyMultMod(float multMod)
        {
            _multMod *= multMod;
            Assert.IsTrue(_multMod >= 0);
            Recalculate();
        }
        
        public void RemoveAddMod(float addMod)
        {
            _addMod -= addMod;
            Recalculate();
        }

        public void RemoveMultMod(float multMod)
        {
            _multMod /= multMod;
            Assert.IsTrue(_multMod >= 0);
            Recalculate();
        }

        public void SetToMax()
        {
            SetCurrentValue(Max);
        }
    }
}