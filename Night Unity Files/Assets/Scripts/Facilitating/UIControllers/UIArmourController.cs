using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SamsHelper;
using SamsHelper.ReactiveUI;
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
        private event Action OnArmourChange;

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
            OnArmourChange += UpdateArmourText;
        }

        private void UpdateArmourText()
        {
            _armourText.text = CurrentArmour() / 10f + "x damage";
        }

        public void SetArmourValue(int armourLevel)
        {
            for (int i = 0; i < _armourChunks.Count; ++i)
            {
                if (i < armourLevel)
                {
                    _armourChunks[i].Reset();
                }
                else
                {
                    _armourChunks[i].Deactivate();
                }
            }
            OnArmourChange?.Invoke();
        }

        public void IncrementArmour(int amount)
        {
            Assert.IsFalse(amount == 0);
            foreach (ArmourChunk armourPiece in _armourChunks)
            {
                if (armourPiece.Remaining.ReachedMax()) continue;
                amount = armourPiece.RestoreArmour(amount);
                if (amount == 0) return;
            }

            OnArmourChange?.Invoke();
        }

        public void TakeDamage(float amount)
        {
            Assert.IsFalse(amount == 0);
            for (int i = _armourChunks.Count - 1; i >= 0; --i)
            {
                if (!_armourChunks[i].Active()) continue;
                amount = _armourChunks[i].TakeDamage(amount);
                if (amount == 0) break;
            }
        
            OnArmourChange?.Invoke();
        }
        
        public void RemovePiece()
        {
            for (int i = _armourChunks.Count - 1; i >= 0; --i)
            {
                if (!_armourChunks[i].Active()) continue;
                _armourChunks[i].TakeDamage(_armourChunks[i].Remaining.CurrentValue());
            }
        
            OnArmourChange?.Invoke();
        }

        public void Update()
        {
            _armourChunks.ForEach(a => a.Update());
        }

        private class ArmourChunk
        {
            public readonly Number Remaining;
            private bool _active;
            private const int ArmourHealth = 100;
            private readonly GameObject _armourObject;
            private const float MaxFadeTime = 1f;
            private float _currentFadeTime;

            public ArmourChunk(GameObject armourObject)
            {
                Remaining = new Number(ArmourHealth, 0, ArmourHealth);
                Remaining.OnMin(Deactivate);
                _armourObject = armourObject;
            }

            public int RestoreArmour(int amount)
            {
                int lost = (int) (ArmourHealth - Remaining.CurrentValue());
                Remaining.Increment(amount);
                _active = true;
                _armourObject.GetComponent<Image>().color = Color.white;
                if (amount > lost)
                {
                    return amount - lost;
                }

                return 0;
            }

            public void Update()
            {
                if (!_active || _currentFadeTime <= 0) return;
                float rValue = 1 - _currentFadeTime / MaxFadeTime;
                _currentFadeTime -= Time.deltaTime;
                if (_currentFadeTime < 0)
                {
                    _currentFadeTime = 0;
                    rValue = 1;
                }
                _armourObject.GetComponent<Image>().color = new Color(1, rValue, rValue, 1);
            }

            public int TakeDamage(float damage)
            {
                int remainder = (int) (damage - Remaining.CurrentValue());
                Remaining.Decrement(damage);
                if (remainder < 0)
                {
                    remainder = 0;
                }
                _currentFadeTime = MaxFadeTime;
                return remainder;
            }

            public bool Active()
            {
                return _active;
            }

            public void Reset()
            {
                RestoreArmour(ArmourHealth);
            }

            public void Deactivate()
            {
                _active = false;
                _armourObject.GetComponent<Image>().color = new Color(1, 1, 1, 0.4f);
            }
        }

        public int CurrentArmour()
        {
            return _armourChunks.Count(chunk => chunk.Active());
        }
    }
}