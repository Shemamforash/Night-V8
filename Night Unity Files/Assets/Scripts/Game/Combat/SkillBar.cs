using System.Collections.Generic;
using Game.Characters.Player;
using Game.Combat.Skills;
using SamsHelper;
using SamsHelper.Input;
using UnityEngine;

namespace Game.Combat
{
    public class SkillBar : MonoBehaviour
    {
        private const int NoSlots = 4;
        private Skill[] _skills;
        private readonly List<CooldownController> _skillView = new List<CooldownController>();
        public static SkillBar Instance;

        public void Awake()
        {
            Instance = this;
            for (int i = 0; i < NoSlots; ++i)
            {
                _skillView.Add(Helper.FindChildWithName<CooldownController>(gameObject, "Skill " + (i + 1)));
            }
            _skills = new Skill[NoSlots];
        }

        public void BindSkills(Player player)
        {
            BindSkill(0, player.CharacterSkillOne);
            BindSkill(1, player.CharacterSkillTwo);
            BindSkill(2, player.Weapon().WeaponSkillOne);
            BindSkill(3, player.Weapon().WeaponSkillTwo);
        }

        private void BindSkill(int slot, Skill skill)
        {
            _skills[slot]?.Cancel();
            _skills[slot] = skill;
            skill.SetController(_skillView[slot]);
        }

        public void ActivateSkill(int skillNo)
        {
            _skills[skillNo]?.Activate();
        }

        public void ResetSkillTimers()
        {
            for (int i = 0; i < NoSlots; ++i)
            {
                _skills[i].ResetTimer();
            }
        }
    }
}