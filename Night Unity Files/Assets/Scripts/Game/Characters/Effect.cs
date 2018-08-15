using System.Xml;
using Facilitating.Persistence;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Characters
{
    public class Effect
    {
        private int Duration;
        private readonly AttributeModifier _modifier;
        private readonly CharacterAttribute _target;
        private readonly Player _player;

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

        public static void Load(Player player, XmlNode effectNode)
        {
            AttributeType targetAttributeType = CharacterAttributes.StringToAttributeType(effectNode.GetNodeText("Target"));
            CharacterAttribute target = player.Attributes.Get(targetAttributeType);
            AttributeModifier modifier = AttributeModifier.Load(effectNode);
            float duration = effectNode.IntFromNode("Duration");
            new Effect(player, modifier, target, duration);
        }
    }
}