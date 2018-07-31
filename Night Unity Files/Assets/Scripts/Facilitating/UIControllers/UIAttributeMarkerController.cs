using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class UIAttributeMarkerController : MonoBehaviour
    {
        private readonly List<Marker> _markers = new List<Marker>();

        public void Awake()
        {
            GameObject primaryMarkerParent = gameObject.FindChildWithName("Primary");
            GameObject secondaryMarkerParent = gameObject.FindChildWithName("Secondary");
            for (int i = 0; i < 10; ++i)
            {
                string markerName = "Marker";
                if (i != 0) markerName += " (" + i + ")";
                Image primaryMarker = primaryMarkerParent.FindChildWithName<Image>(markerName);
                _markers.Add(new Marker(primaryMarker));
            }

            for (int i = 0; i < 10; ++i)
            {
                string markerName = "Marker";
                if (i != 0) markerName += " (" + i + ")";
                Image secondaryMarker = secondaryMarkerParent.FindChildWithName<Image>(markerName);
                _markers.Add(new Marker(secondaryMarker));
            }
        }

        private enum MarkerState
        {
            Inactive,
            Faded,
            Active
        }

        private class Marker
        {
            private MarkerState _state;
            private readonly Image _image;
            private bool _stateChanged;

            public Marker(Image image)
            {
                _image = image;
                _state = MarkerState.Inactive;
                _image.color = UiAppearanceController.InvisibleColour;
            }

            public void SetState(MarkerState state)
            {
                if (state == _state)
                {
                    _stateChanged = false;
                    return;
                }

                _stateChanged = true;
                _state = state;
            }

            public void UpdateColor()
            {
                if (!_stateChanged) return;
                Color c = Color.white;
                switch (_state)
                {
                    case MarkerState.Inactive:
                        c = UiAppearanceController.InvisibleColour;
                        break;
                    case MarkerState.Faded:
                        c = UiAppearanceController.FadedColour;
                        break;
                    case MarkerState.Active:
                        c = Color.white;
                        break;
                }

                _image.DOColor(c, 1f);
            }
        }

        public void SetValue(CharacterAttribute attribute)
        {
            int max = (int) attribute.Max;
            int currentValue = Mathf.CeilToInt(attribute.CurrentValue());
            for (int i = 0; i < 20; ++i)
            {
                MarkerState newState;
                if (i < currentValue)
                    newState = MarkerState.Active;
                else if (i < max)
                    newState = MarkerState.Faded;
                else
                    newState = MarkerState.Inactive;
                _markers[i].SetState(newState);
            }

            _markers.ForEach(m => m.UpdateColor());
        }
    }
}