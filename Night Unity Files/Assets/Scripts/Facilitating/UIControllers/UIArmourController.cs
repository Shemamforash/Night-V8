using System;
using System.Collections.Generic;
using System.Linq;
using Game.Characters;
using Game.Gear.Armour;
using SamsHelper;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class UIArmourController : MonoBehaviour
    {
        private const int ArmourDivisions = 10;
        private readonly List<ArmourChunk> _armourChunks = new List<ArmourChunk>();
        private Transform _armourBar;
        private const int SegmentSpacing = 5;
        private HorizontalLayoutGroup _layoutGroup;
        private TextMeshProUGUI _armourText;
        private ArmourController _armourController;

        // Use this for initialization
        public void Awake()
        {
            _armourBar = Helper.FindChildWithName<Transform>(gameObject, "Armour Bar");
            _armourText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Armour Text");

            _layoutGroup = _armourBar.GetComponent<HorizontalLayoutGroup>();
            _layoutGroup.spacing = SegmentSpacing;

            List<GameObject> segments = new List<GameObject>();
            for (int i = 0; i < ArmourDivisions; ++i)
            {
                GameObject newSegment = new GameObject();
                newSegment.transform.SetParent(_armourBar, false);
                newSegment.AddComponent<Image>();
                newSegment.name = "Armour Piece " + (i + 1);
                segments.Add(newSegment);
            }

            for (int i = ArmourDivisions - 1; i >= 0; --i)
            {
                _armourChunks.Add(new ArmourChunk(segments[i]));
            }
        }

        public void SetCharacter(Character character)
        {
            _armourController = character.ArmourController;
        }

        private void SetSlotsFilled(bool damageWasTaken = false)
        {
            int slotsAvailable = _armourController.GetMaxArmour();
            int slotsUsed = _armourController.GetCurrentArmour();

            for (int i = 0; i < _armourChunks.Count; i++)
            {
                ArmourChunk chunk = _armourChunks[i];
                if (i < slotsAvailable)
                {
                    if (i < slotsUsed)
                    {
                        chunk.Activate(i == slotsUsed - 1 && damageWasTaken);
                    }
                    else
                    {
                        chunk.Deactivate();
                    }
                }
                else
                {
                    chunk.SetInvisible();
                }
            }
            _armourText.text = _armourController.GetCurrentArmour() / 10f + "x damage";
        }

        public int CurrentArmour()
        {
            return _armourController.GetCurrentArmour();
        }

        public void RepairArmour(float amount)
        {
            _armourController.Repair(amount);
            SetSlotsFilled();
        }

        public void TakeDamage(float damage)
        {
            _armourController.TakeDamage(damage);
            SetSlotsFilled(true);
        }

        public void Update()
        {
            _armourChunks.ForEach(a => a.Update());
        }

        private class ArmourChunk
        {
            private readonly GameObject _armourObject;
            private const float MaxFadeTime = 1f;
            private float _currentFadeTime;

            public ArmourChunk(GameObject armourObject)
            {
                _armourObject = armourObject;
            }

            public void Update()
            {
                if (_currentFadeTime <= 0) return;
                float rValue = 1 - _currentFadeTime / MaxFadeTime;
                _currentFadeTime -= Time.deltaTime;
                if (_currentFadeTime < 0)
                {
                    _currentFadeTime = 0;
                    rValue = 1;
                }

                _armourObject.GetComponent<Image>().color = new Color(1, rValue, rValue, 1);
            }

            public void Activate(bool damageWasTaken)
            {
                if (damageWasTaken) _currentFadeTime = MaxFadeTime;
                else _armourObject.GetComponent<Image>().color = Color.white;
            }

            public void Deactivate()
            {
                _armourObject.GetComponent<Image>().color = new Color(1, 1, 1, 0.4f);
            }

            public void SetInvisible()
            {
                _armourObject.GetComponent<Image>().color = new Color(1, 1, 1, 0);
            }
        }
    }
}