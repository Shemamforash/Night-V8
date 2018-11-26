using System.Collections.Generic;
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
            for (int i = 0; i < 20; ++i)
            {
                string markerName = "Marker";
                if (i != 0) markerName += " (" + i + ")";
                Image primaryMarker = gameObject.FindChildWithName<Image>(markerName);
                _markers.Add(new Marker(primaryMarker));
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
                _image.gameObject.SetActive(state != MarkerState.Inactive);
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
                Color c = _state == MarkerState.Faded ? UiAppearanceController.InvisibleColour : Color.white;
                _image.DOColor(c, 1f).SetUpdate(UpdateType.Normal, true);
            }
        }

        public void SetValue(float maxF, float currentF)
        {
            int max = Mathf.CeilToInt(maxF);
            int current = Mathf.CeilToInt(currentF);
            for (int i = 0; i < 20; ++i)
            {
                MarkerState newState;
                if (i < current)
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