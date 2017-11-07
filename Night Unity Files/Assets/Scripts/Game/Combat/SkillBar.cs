using System.Collections.Generic;
using Game.Combat.Skills;
using SamsHelper;
using SamsHelper.Input;
using UnityEngine;

namespace Game.Combat
{
    public class SkillBar : MonoBehaviour, IInputListener
    {
        public int NoSlots;
        private Skill[] _skills;
        public GameObject SkillPrefab;
        private readonly List<CooldownController> _skillView = new List<CooldownController>();

        public void Awake()
        {
            InputHandler.RegisterInputListener(this);
            for (int i = 0; i < NoSlots; ++i)
            {
                GameObject newSkillObject = Helper.InstantiateUiObject(SkillPrefab, transform);
                CooldownController cooldownController = newSkillObject.GetComponent<CooldownController>();
                cooldownController.Text("--");
                _skillView.Add(cooldownController);
            }
            _skills = new Skill[NoSlots];
        }

        public void BindSkill(int slot, Skill skill)
        {
            --slot;
            if (slot >= NoSlots)
            {
                throw new Exceptions.SkillSlotOutOfRangeException(slot, NoSlots);
            }
            _skills[slot]?.Cancel();
            _skills[slot] = skill;
            skill.SetController(_skillView[slot]);
        }

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            switch (axis)
            {
                case InputAxis.SkillOne:
                    _skills[0]?.Activate();
                    break;
                case InputAxis.SkillTwo:
                    _skills[1]?.Activate();
                    break;
                case InputAxis.SkillThree:
                    _skills[2]?.Activate();
                    break;
                case InputAxis.SkillFour:
                    _skills[3]?.Activate();
                    break;
            }
        }

        public void OnInputUp(InputAxis axis)
        {
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }
    }
}