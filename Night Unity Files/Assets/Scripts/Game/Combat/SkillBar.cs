using System.Collections.Generic;
using Game.Combat.Skills;
using SamsHelper;
using SamsHelper.Input;
using UnityEngine;

namespace Game.Combat
{
    public class SkillBar : MonoBehaviour
    {
        private int _noSlots = 4;
        private Skill[] _skills;
        private readonly List<CooldownController> _skillView = new List<CooldownController>();
        public static SkillBar Instance;

        public void Awake()
        {
            Instance = this;
            for (int i = 0; i < _noSlots; ++i)
            {
                _skillView.Add(Helper.FindChildWithName<CooldownController>(gameObject, "Skill " + (i + 1)));
            }
            _skills = new Skill[_noSlots];
        }

        public void BindSkill(int slot, Skill skill)
        {
            --slot;
            if (slot >= _noSlots)
            {
                throw new Exceptions.SkillSlotOutOfRangeException(slot, _noSlots);
            }

            _skills[slot]?.Cancel();
            _skills[slot] = skill;
            skill.SetController(_skillView[slot]);
        }

        public void ActivateSkill(int skillNo)
        {
            _skills[skillNo]?.Activate();
        }
    }
}