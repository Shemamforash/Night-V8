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
        private static Skill[] _skills;
        private static readonly List<CooldownController> SkillView = new List<CooldownController>();
        private static readonly List<UISkillCostController> CostControllers = new List<UISkillCostController>();
        private static CanvasGroup _canvas;

        public void Awake()
        {
            for (int i = 0; i < NoSlots; ++i)
            {
                SkillView.Add(Helper.FindChildWithName<CooldownController>(gameObject, "Skill " + (i + 1)));
                CostControllers.Add(Helper.FindChildWithName<UISkillCostController>(SkillView[i].gameObject, "Cost"));
            }
            _skills = new Skill[NoSlots];
            _canvas = GetComponent<CanvasGroup>();
        }

        public static void BindSkills(Player player)
        {
            BindSkill(0, player.CharacterSkillOne);
            BindSkill(1, player.CharacterSkillTwo);
            BindSkill(2, player.Weapon().WeaponSkillOne);
            BindSkill(3, player.Weapon().WeaponSkillTwo);
        }

        public static void SetVisible(bool visible)
        {
            _canvas.alpha = visible ? 1 : 0;
        }
        
        private static void BindSkill(int slot, Skill skill)
        {
            _skills[slot]?.Cancel();
            _skills[slot] = skill;
            skill.SetController(SkillView[slot]);
            CostControllers[slot].SetCost(skill.Cost);
        }

        public static void ActivateSkill(int skillNo)
        {
            _skills[skillNo]?.Activate();
        }

        public static void ResetSkillTimers()
        {
            for (int i = 0; i < NoSlots; ++i)
            {
                _skills[i].Start();
            }
        }
    }
}