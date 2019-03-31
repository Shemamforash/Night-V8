using System;
using InControl;
using UnityEngine;

namespace SamsHelper.Input
{
    public class InputActions : PlayerActionSet
    {
        private readonly PlayerAction Left, Right;
        public readonly PlayerOneAxisAction Horizontal;

        private readonly PlayerAction Up, Down;
        public readonly PlayerOneAxisAction Vertical;

        public readonly PlayerTwoAxisAction Move;

        private readonly PlayerAction LeftTab, RightTab;
        public readonly PlayerOneAxisAction ChangeTab;

        public readonly PlayerAction Menu;
        public readonly PlayerAction Cancel;
        public readonly PlayerAction Accept;
        public readonly PlayerAction Fire;
        public readonly PlayerAction Reload;
        public readonly PlayerAction Sprint;
        public readonly PlayerAction SkillOne, SkillTwo, SkillThree, SkillFour;
        public readonly PlayerAction Inventory;
        public readonly PlayerAction Compass;
        public readonly PlayerAction TakeItem;
        public readonly PlayerAction Swivel;

        public InputActions()
        {
            Left = CreatePlayerAction("Move Left");
            Right = CreatePlayerAction("Move Right");
            Horizontal = CreateOneAxisPlayerAction(Left, Right);

            Up = CreatePlayerAction("Move Up");
            Down = CreatePlayerAction("Move Down");
            Vertical = CreateOneAxisPlayerAction(Down, Up);

            Move = CreateTwoAxisPlayerAction(Left, Right, Down, Up);

            LeftTab = CreatePlayerAction("Tab Left");
            RightTab = CreatePlayerAction("Tab Right");
            ChangeTab = CreateOneAxisPlayerAction(LeftTab, RightTab);

            Menu = CreatePlayerAction("Menu");
            Cancel = CreatePlayerAction("Cancel");
            Accept = CreatePlayerAction("Accept");
            Fire = CreatePlayerAction("Fire");
            Reload = CreatePlayerAction("Reload");
            Sprint = CreatePlayerAction("Sprint");
            SkillOne = CreatePlayerAction("Skill One");
            SkillTwo = CreatePlayerAction("Skill Two");
            SkillThree = CreatePlayerAction("Skill Three");
            SkillFour = CreatePlayerAction("SKill Four");
            Inventory = CreatePlayerAction("Inventory");
            Compass = CreatePlayerAction("Compass");
            TakeItem = CreatePlayerAction("Take Item");
            Swivel = CreatePlayerAction("Swivel");
        }

        public Tuple<PlayerAction, PlayerAction> AxisToActions(PlayerOneAxisAction axis)
        {
            if (axis == Horizontal) return Tuple.Create(Left, Right);
            if (axis == Vertical) return Tuple.Create(Up, Down);
            return Tuple.Create(LeftTab, RightTab);
        }

        public void BindActions()
        {
            Left.AddDefaultBinding(Key.A);
            Left.AddDefaultBinding(InputControlType.LeftStickLeft);

            Right.AddDefaultBinding(Key.D);
            Right.AddDefaultBinding(InputControlType.LeftStickRight);

            Up.AddDefaultBinding(Key.W);
            Up.AddDefaultBinding(InputControlType.LeftStickUp);

            Down.AddDefaultBinding(Key.S);
            Down.AddDefaultBinding(InputControlType.LeftStickDown);

            LeftTab.AddDefaultBinding(Key.J);
            LeftTab.AddDefaultBinding(InputControlType.RightStickLeft);

            RightTab.AddDefaultBinding(Key.L);
            RightTab.AddDefaultBinding(InputControlType.RightStickRight);

            Menu.AddDefaultBinding(Key.Escape);
            Menu.AddDefaultBinding(InputControlType.Command);

            Cancel.AddDefaultBinding(Key.Escape);
            Cancel.AddDefaultBinding(Key.C);
            Cancel.AddDefaultBinding(InputControlType.Action2);

            Accept.AddDefaultBinding(Key.K);
            Accept.AddDefaultBinding(InputControlType.Action1);

            Fire.AddDefaultBinding(Key.K);
            Fire.AddDefaultBinding(InputControlType.RightBumper);
            Fire.AddDefaultBinding(Mouse.LeftButton);

            Reload.AddDefaultBinding(Key.R);
            Reload.AddDefaultBinding(InputControlType.Action3);

            Sprint.AddDefaultBinding(Key.Space);
            Sprint.AddDefaultBinding(InputControlType.LeftBumper);

            SkillOne.AddDefaultBinding(Key.Key1);
            SkillOne.AddDefaultBinding(InputControlType.DPadDown);

            SkillTwo.AddDefaultBinding(Key.Key2);
            SkillTwo.AddDefaultBinding(InputControlType.DPadLeft);

            SkillThree.AddDefaultBinding(Key.Key3);
            SkillThree.AddDefaultBinding(InputControlType.DPadUp);

            SkillFour.AddDefaultBinding(Key.Key4);
            SkillFour.AddDefaultBinding(InputControlType.DPadRight);

            Inventory.AddDefaultBinding(Key.I);
            Inventory.AddDefaultBinding(InputControlType.Action4);

            Compass.AddDefaultBinding(Key.Q);
            Compass.AddDefaultBinding(InputControlType.Action2);

            TakeItem.AddDefaultBinding(Key.F);
            TakeItem.AddDefaultBinding(Mouse.MiddleButton);
            TakeItem.AddDefaultBinding(InputControlType.Action1);

            Swivel.AddDefaultBinding(Mouse.RightButton);
        }
    }
}