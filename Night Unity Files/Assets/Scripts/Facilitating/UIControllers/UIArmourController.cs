using System.Collections.Generic;
using Game.Gear.Armour;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class UIArmourController : MonoBehaviour
    {
        private const int ArmourDivisions = 10;
        private const int SegmentSpacing = 5;
        private readonly List<ArmourChunk> _armourChunks = new List<ArmourChunk>();

        private Transform _armourBar;
        private TextMeshProUGUI _armourText;
        private HorizontalLayoutGroup _layoutGroup;

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

            for (int i = ArmourDivisions - 1; i >= 0; --i) _armourChunks.Add(new ArmourChunk(segments[i]));
        }

        private void SetSlotsFilled(ArmourController armourController, bool damageWasTaken = false)
        {
            int slotsAvailable = armourController.GetMaxArmour();
            int slotsUsed = armourController.GetCurrentArmour();

            for (int i = 0; i < _armourChunks.Count; i++)
            {
                ArmourChunk chunk = _armourChunks[i];
                if (i < slotsAvailable)
                    if (i < slotsUsed)
                        chunk.Activate(i == slotsUsed - 1 && damageWasTaken);
                    else
                        chunk.Deactivate();
                else
                    chunk.SetInvisible();
            }

            _armourText.text = armourController.GetCurrentArmour() / 10f + "x damage";
        }

        public void RepairArmour(ArmourController controller)
        {
            SetSlotsFilled(controller);
        }

        public void TakeDamage(ArmourController controller)
        {
            SetSlotsFilled(controller, true);
        }

        public void Update()
        {
            _armourChunks.ForEach(a => a.Update());
        }

        private class ArmourChunk
        {
            private const float MaxFadeTime = 1f;
            private readonly GameObject _armourObject;
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
                _armourObject.GetComponent<Image>().color = UiAppearanceController.FadedColour;
            }

            public void SetInvisible()
            {
                _armourObject.GetComponent<Image>().color = UiAppearanceController.InvisibleColour;
            }
        }
    }
}