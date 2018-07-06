using System.Collections.Generic;
using Facilitating.UIControllers;
using Game.Combat.Generation;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class SkillBar : MonoBehaviour
    {
        private const int NoSlots = 4;
        private static Skill[] _skills;
        private static List<int> _skillsLocked;
        private static List<CooldownController> CooldownControllers;
        private static List<UISkillCostController> CostControllers;
        private static Cooldown _skillsCooldown;
        private static float _cooldownModifier;

        public void Awake()
        {
            CooldownControllers = new List<CooldownController>();
            CostControllers = new List<UISkillCostController>();

            for (int i = 0; i < NoSlots; ++i)
            {
                CooldownControllers.Add(Helper.FindChildWithName<CooldownController>(gameObject, "Skill " + (i + 1)));
                CostControllers.Add(Helper.FindChildWithName<UISkillCostController>(CooldownControllers[i].gameObject, "Cost"));
            }

            _skills = new Skill[NoSlots];
            _skillsLocked = new List<int>();
            _skillsCooldown = CombatManager.CreateCooldown();
            CooldownControllers.ForEach(s => _skillsCooldown.SetController(s));
        }

        public static void BindSkills(Characters.Player player, float skillCooldownModifier)
        {
            _cooldownModifier = skillCooldownModifier;
            Skill characterSkillOne = player.CharacterSkillOne;
            Skill characterSkillTwo = null;
            Skill weaponSkillOne = null;
            Skill weaponSkillTwo = null;
            
            if (player.Attributes.SkillOneUnlocked)
                characterSkillOne = player.CharacterSkillOne;

            if (player.Attributes.SkillTwoUnlocked)
                characterSkillTwo = player.CharacterSkillTwo;

            if (player.Weapon != null)
            {
                if (player.Attributes.WeaponSkillOneUnlocks.Contains(player.Weapon.WeaponType()))
                    weaponSkillOne = player.Weapon.WeaponSkillOne;

                if (player.Attributes.WeaponSkillTwoUnlocks.Contains(player.Weapon.WeaponType()))
                    weaponSkillTwo = player.Weapon.WeaponSkillTwo;
            }

            BindSkill(0, characterSkillOne);
            BindSkill(1, characterSkillTwo);
            BindSkill(2, weaponSkillOne);
            BindSkill(3, weaponSkillTwo);
        }

        private static void BindSkill(int slot, Skill skill)
        {
            _skills[slot] = skill;
            CooldownControllers[slot].SetVisible(skill != null);
            if (skill == null) return;
            CooldownControllers[slot].Text(skill.Name);
            CostControllers[slot].SetCost(skill.Cooldown());
        }

        public static void ActivateSkill(int skillNo)
        {
            if(_skillsLocked.Contains(skillNo)) return;
            if (_skillsCooldown.Running()) return;
            bool canActivate = false;
            float freeSkillChance = PlayerCombat.Instance.Player.Attributes.Val(AttributeType.FreeSkillChance);
            if (Random.Range(0f, 1f) < freeSkillChance)
            {
                canActivate = true;
            }
            float noSkillChance = PlayerCombat.Instance.Player.Attributes.Val(AttributeType.InactiveSkillChance);
            if (Random.Range(0f, 1f) < noSkillChance)
            {
                _skillsLocked.Add(skillNo);
            }

            if (!canActivate)
            {
                canActivate = PlayerCombat.Instance.ConsumeAdrenaline(_skills[skillNo].Cooldown());
                if (!canActivate) return;
            }
            CombatManager.IncreaseSkillsUsed();
            _skills[skillNo].Activate();
            _skillsCooldown.Duration = _skills[skillNo].Cooldown() * _cooldownModifier;
            _skillsCooldown.Start();
        }
    }
}