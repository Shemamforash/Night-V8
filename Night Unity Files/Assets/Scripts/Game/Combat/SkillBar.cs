﻿using System.Collections.Generic;
using Game.Characters.Player;
using Game.Combat.Skills;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using UnityEngine;

namespace Game.Combat
{
    public class SkillBar : MonoBehaviour
    {
        private const int NoSlots = 4;
        private static Skill[] _skills;
        private static readonly List<CooldownController> CooldownControllers = new List<CooldownController>();
        private static readonly List<UISkillCostController> CostControllers = new List<UISkillCostController>();
        private static CanvasGroup _canvas;
        private static Cooldown _skillsCooldown;

        public void Awake()
        {
            for (int i = 0; i < NoSlots; ++i)
            {
                CooldownControllers.Add(Helper.FindChildWithName<CooldownController>(gameObject, "Skill " + (i + 1)));
                CostControllers.Add(Helper.FindChildWithName<UISkillCostController>(CooldownControllers[i].gameObject, "Cost"));
            }

            _skills = new Skill[NoSlots];
            _canvas = GetComponent<CanvasGroup>();
            _skillsCooldown = CombatManager.CombatCooldowns.CreateCooldown();
            CooldownControllers.ForEach(s => _skillsCooldown.SetController(s));
        }

        public static void BindSkills(Player player)
        {
            BindSkill(0, player.CharacterSkillOne);
            BindSkill(1, player.CharacterSkillTwo);
            BindSkill(2, player.Weapon.WeaponSkillOne);
            BindSkill(3, player.Weapon.WeaponSkillTwo);
        }

        public static void SetVisible(bool visible)
        {
            _canvas.alpha = visible ? 1 : 0;
        }

        private static void BindSkill(int slot, Skill skill)
        {
            _skills[slot] = skill;
            CooldownControllers[slot].Text(skill.Name);
            CostControllers[slot].SetCost((int) skill.Cooldown());
        }

        public static void ActivateSkill(int skillNo)
        {
            if (_skillsCooldown.Running()) return;
            _skills[skillNo].Activate();
            _skillsCooldown.Duration = _skills[skillNo].Cooldown();
            _skillsCooldown.Start();
        }
    }
}