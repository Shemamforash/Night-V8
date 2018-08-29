using System.Collections.Generic;
using DG.Tweening;
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
            _armourBar = gameObject.FindChildWithName<Transform>("Armour Bar");
            _armourText = gameObject.FindChildWithName<TextMeshProUGUI>("Armour Text");

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

        public void TakeDamage(ArmourController controller)
        {
            SetSlotsFilled(controller, true);
        }

        private class ArmourChunk
        {
            private readonly GameObject _armourObject;
            private readonly Image _image;

            public ArmourChunk(GameObject armourObject)
            {
                _armourObject = armourObject;
                _image = _armourObject.GetComponent<Image>();
            }

            public void Activate(bool damageWasTaken)
            {
                if (damageWasTaken)
                {
                    _image.color= Color.red;
                    _image.DOBlendableColor(Color.white, 0.25f);
                }
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