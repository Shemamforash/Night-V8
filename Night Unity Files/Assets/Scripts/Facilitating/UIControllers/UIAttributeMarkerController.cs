using System.Collections.Generic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class UIAttributeMarkerController : MonoBehaviour
    {
        private readonly List<GameObject> _attributeMarkers = new List<GameObject>();
        public bool FillFromLeft;

        public void SetValue(int currentValue, int max)
        {
            _attributeMarkers.ForEach(Destroy);
            if (FillFromLeft)
            {
                for (int i = 0; i < max; ++i) CreateMarker(i, currentValue);

                return;
            }

            for (int i = max - 1; i >= 0; --i) CreateMarker(i, currentValue);
        }

        private void CreateMarker(int i, int currentValue)
        {
            GameObject newMarker = Helper.InstantiateUiObject("Prefabs/AttributeMarkerPrefab", transform);
            Image markerImage = newMarker.GetComponent<Image>();
            markerImage.color = i < currentValue ? Color.white : UiAppearanceController.FadedColour;
            _attributeMarkers.Add(newMarker);
        }
    }
}