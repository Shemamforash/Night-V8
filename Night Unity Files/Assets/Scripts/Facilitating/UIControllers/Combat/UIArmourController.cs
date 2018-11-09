using System.Collections.Generic;
using DG.Tweening;
using Game.Gear.Armour;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class UIArmourController : MonoBehaviour
    {
        private const int SegmentSpacing = 5;
        private readonly List<ArmourChunk> _armourChunks = new List<ArmourChunk>();

        private HorizontalLayoutGroup _layoutGroup;
        private int _lastSlotsUsed = -1;
        private int _lastSlotsAvailable = -1;

        public void Awake()
        {
            _layoutGroup = GetComponent<HorizontalLayoutGroup>();
            _layoutGroup.spacing = SegmentSpacing;

            int childCount = _layoutGroup.transform.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                GameObject segment = _layoutGroup.transform.GetChild(i).gameObject;
                if (segment.name == "Gap") continue;
                _armourChunks.Add(new ArmourChunk(segment));
            }

            _armourChunks.Reverse();
        }

        public void UpdateArmour(ArmourController controller)
        {
            bool damageWasTaken = controller.DidJustTakeDamage();
            int slotsAvailable = controller.GetTotalProtection();
            int slotsUsed = controller.GetCurrentProtection();

            if (slotsAvailable == _lastSlotsAvailable && slotsUsed == _lastSlotsUsed) return;

            for (int i = 0; i < _armourChunks.Count; i++)
            {
                ArmourChunk chunk = _armourChunks[i];
                if (i < slotsAvailable)
                    if (i < slotsUsed)
                        chunk.Activate(i == slotsUsed - 1 && damageWasTaken);
                    else
                        chunk.Deactivate();
                else
                    chunk.SetUnused();
            }

            _lastSlotsAvailable = slotsAvailable;
            _lastSlotsUsed = slotsUsed;
        }

        private class ArmourChunk
        {
            private readonly CanvasGroup _armourCanvasGroup, _activeCanvasGroup;
            private readonly Image _brokenImage, _completeImage, _leftBar, _rightBar, _inactive;

            public ArmourChunk(GameObject armourObject)
            {
                _armourCanvasGroup = armourObject.GetComponent<CanvasGroup>();
                _activeCanvasGroup = armourObject.FindChildWithName<CanvasGroup>("Active");
                _inactive = armourObject.FindChildWithName<Image>("Inactive");
                _brokenImage = armourObject.FindChildWithName<Image>("Broken");
                _completeImage = armourObject.FindChildWithName<Image>("Complete");
                _leftBar = armourObject.FindChildWithName<Image>("Left Bar");
                _rightBar = armourObject.FindChildWithName<Image>("Right Bar");
            }

            private void FlashImage(Image image)
            {
                image.color = Color.red;
                image.DOColor(Color.white, 0.25f).SetUpdate(UpdateType.Normal, true);
            }

            public void Activate(bool damageWasTaken)
            {
                _armourCanvasGroup.alpha = 1f;
                _activeCanvasGroup.alpha = 1f;
                _inactive.SetAlpha(0f);
                _completeImage.SetAlpha(1f);
                _brokenImage.SetAlpha(0f);

                if (damageWasTaken)
                {
                    FlashImage(_completeImage);
                    FlashImage(_leftBar);
                    FlashImage(_rightBar);
                }
                else
                {
                    _completeImage.color = Color.white;
                    _leftBar.color = Color.white;
                    _rightBar.color = Color.white;
                }
            }

            public void Deactivate()
            {
                _armourCanvasGroup.alpha = 1f;
                _activeCanvasGroup.alpha = 1f;
                _inactive.SetAlpha(0f);
                _completeImage.SetAlpha(0f);
                _brokenImage.SetAlpha(1f);
            }

            public void SetUnused()
            {
                _armourCanvasGroup.alpha = 0.2f;
                _activeCanvasGroup.alpha = 0f;
                _inactive.SetAlpha(1f);
            }
        }
    }
}