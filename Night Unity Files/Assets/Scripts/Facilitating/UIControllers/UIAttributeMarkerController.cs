using System.Collections.Generic;
using DG.Tweening;
using Extensions;
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
				Image primaryMarker = gameObject.transform.GetChild(i).FindChildWithName<Image>("Outline");
				_markers.Add(new Marker(primaryMarker));
			}
		}

		public void SetValue(float maxF, float currentF, float offsetF)
		{
			int max                            = Mathf.CeilToInt(maxF);
			int current                        = Mathf.CeilToInt(currentF);
			int offset                         = Mathf.CeilToInt(offsetF);
			if (offsetF          < 0) current  += offset;
			if (current + offset > max) offset -= current + offset - max;

			for (int i = 0; i < 20; ++i)
			{
				MarkerState newState;
				if (i < current)
				{
					newState = MarkerState.Active;
				}
				else if (i < current + Mathf.Abs(offset))
				{
					newState = MarkerState.ActiveOffset;
				}
				else if (i < max)
				{
					newState = MarkerState.Faded;
				}
				else
				{
					newState = MarkerState.Inactive;
				}

				_markers[i].SetState(newState);
			}

			_markers.ForEach(m => m.UpdateColor());
		}

		private enum MarkerState
		{
			Inactive,
			Faded,
			Active,
			ActiveOffset
		}

		private class Marker
		{
			private readonly Image       _image;
			private readonly GameObject  _markerParent;
			private          MarkerState _state;
			private          bool        _stateChanged;

			public Marker(Image image)
			{
				_image        = image;
				_state        = MarkerState.Inactive;
				_image.color  = UiAppearanceController.InvisibleColour;
				_markerParent = _image.transform.parent.gameObject;
			}

			public void SetState(MarkerState state)
			{
				_markerParent.SetActive(state != MarkerState.Inactive);
				if (state == _state)
				{
					_stateChanged = false;
					return;
				}

				_stateChanged = true;
				_state        = state;
			}

			public void UpdateColor()
			{
				if (!_stateChanged) return;
				Color c;
				switch (_state)
				{
					case MarkerState.Faded:
						c = UiAppearanceController.InvisibleColour;
						break;
					case MarkerState.Active:
						c = Color.white;
						break;
					default:
						c = Color.red;
						break;
				}

				_image.DOColor(c, 1f).SetUpdate(UpdateType.Normal, true);
			}
		}
	}
}