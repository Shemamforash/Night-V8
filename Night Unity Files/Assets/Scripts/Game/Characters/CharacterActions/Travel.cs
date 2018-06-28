﻿using System;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Exploration.WorldEvents;
using Game.Global;
using UnityEngine;

namespace Game.Characters.CharacterActions
{
    public class Travel : BaseCharacterAction
    {
        private int TimeSpentTravelling;
        private Region _target;
        private Region CurrentNode;
        private bool _inTransit;

        public Travel(Player playerCharacter) : base("Travel", playerCharacter)
        {
            DisplayName = "Travelling";
            MinuteCallback = () =>
            {
                if (Duration == 0)
                {
                    if (_inTransit) ReachTarget();
                    return;
                }

                ++TimeSpentTravelling;
                if (TimeSpentTravelling == WorldState.MinutesPerHour)
                {
                    PlayerCharacter.Travel();
                    TimeSpentTravelling = 0;
                }

                --Duration;
            };
        }

        public bool AtHome()
        {
            //todo fix me
            if (CurrentNode == null) return true;
            return CurrentNode.GetRegionType() == RegionType.Gate;
        }

        private void ReachTarget()
        {
            CurrentNode = _target;
            _inTransit = false;
            if (AtHome())
            {
                PlayerCharacter.Attributes.DecreaseWillpower();
                TimeSpentTravelling = 0;
                PlayerCharacter.Inventory().MoveAllResources(WorldState.HomeInventory());
                PlayerCharacter.RestAction.Enter();
                WorldEventManager.GenerateEvent(new CharacterMessage("I'm back, but the journey has taken it's toll", PlayerCharacter));
            }
            else
            {
                CurrentNode.Discover();
                CharacterManager.SelectedCharacter.TravelAction.GetCurrentNode().Enter();
            }
        }

        protected override void OnClick()
        {
            SceneChanger.ChangeScene("Map");
        }

        public Region GetCurrentNode()
        {
            return CurrentNode ?? (CurrentNode = MapGenerator.GetInitialNode());
        }

        public bool InTransit()
        {
            return _inTransit;
        }

        public void TravelTo(Region target, int duration)
        {
            Enter();
            switch (target.GetRegionType())
            {
                case RegionType.Shelter:
                    WorldEventManager.GenerateEvent(new CharacterMessage("Perhaps I will find others", PlayerCharacter));
                    break;
                case RegionType.Gate:
                    WorldEventManager.GenerateEvent(new CharacterMessage("I'm going back to camp", PlayerCharacter));
                    break;
                case RegionType.Temple:
                    WorldEventManager.GenerateEvent(new CharacterMessage("I hope I will see you again", PlayerCharacter));
                    break;
                case RegionType.Animal:
                    WorldEventManager.GenerateEvent(new CharacterMessage("Perhaps I will find something to hunt", PlayerCharacter));
                    break;
                case RegionType.Danger:
                    WorldEventManager.GenerateEvent(new CharacterMessage("I pray I won't find trouble, but I will be ready if I do", PlayerCharacter));
                    break;
                case RegionType.Nightmare:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _inTransit = true;
            _target = target;
            Duration = duration;
        }

        public void SetCurrentNode(Region node)
        {
            CurrentNode = node;
        }
    }
}