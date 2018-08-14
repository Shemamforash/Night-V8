using System.Xml;
using Facilitating.Persistence;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Characters
{
    public class Effect
    {
        public int Duration;
        private AttributeModifier _modifier;
        private CharacterAttribute _target;
        private Player _player;

        public Effect(Player player, AttributeModifier modifier, CharacterAttribute target, float duration)
        {
            _target = target;
            _modifier = modifier;
            _player = player;
            if (duration == 0)
            {
                target.Increment(modifier.RawBonus());
                return;
            }
            _target.AddModifier(_modifier);
            Duration = Mathf.FloorToInt(duration * WorldState.MinutesPerHour);
            player.AddEffect(this);
        }

        public void UpdateEffect()
        {
            Duration -= 1;
            if (Duration != 0) return;
            _player.RemoveEffect(this);
            _target.RemoveModifier(_modifier);
        }

        public void Save(XmlNode doc)
        {
            doc = doc.CreateChild("Effect");
            doc.CreateChild("Duration", Duration);
            doc.CreateChild("Target", _target.AttributeType);
            _modifier.Save(doc);
        }
    }
}