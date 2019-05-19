﻿using System.Collections;
using DG.Tweening;
using Extensions;
using Game.Characters;
using Game.Exploration.Environment;
using Game.Exploration.Regions;

using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Exploration.Ui
{
	public class MapNodeController : MonoBehaviour
	{
		private static Sprite             _animalSprite, _dangerSprite, _gateSprite, _fountainSprite, _monumentSprite, _shelterSprite, _shrineSprite, _templeSprite, _cacheSprite, _noneSprite;
		private        UIBorderController _border;
		private        bool               _canAfford;
		private        float              _currentTime;
		private        int                _distance;

		private bool _hidden;

		private Image           _icon,       _selectedImage, _cleansedSprite;
		private TextMeshProUGUI _nameText,   _costText;
		private CanvasGroup     _nodeCanvas, _centreCanvas,  _ringCanvas;
		private Region          _region;
		private Sequence        _sequence;
		private float           _targetCentreAlpha, _targetNodeAlpha;
		private TextReveal      _textReveal;
		private int             _travelCost;

		public void Awake()
		{
			_nodeCanvas   = gameObject.FindChildWithName<CanvasGroup>("Canvas");
			_centreCanvas = gameObject.FindChildWithName<CanvasGroup>("Centre Canvas");

			_selectedImage = gameObject.FindChildWithName<Image>("Selected");
			_nameText      = gameObject.FindChildWithName<TextMeshProUGUI>("Name");
			_costText      = gameObject.FindChildWithName<TextMeshProUGUI>("Cost");
			_icon          = gameObject.FindChildWithName<Image>("Icon");

			_border = gameObject.FindChildWithName<UIBorderController>("Border");
			_border.SetActive();

			_cleansedSprite = gameObject.FindChildWithName<Image>("Cleansed");
			_cleansedSprite.SetAlpha(0);

			_ringCanvas       = gameObject.FindChildWithName<CanvasGroup>("Rings");
			_ringCanvas.alpha = 0;

			_textReveal = GetComponent<TextReveal>();
		}

		private void SetTravelCostText()
		{
			string travelString;
			if (_distance == 0)
			{
				travelString = "Current Location";
			}
			else if (_region.GetRegionType() == RegionType.Gate)
			{
				travelString = "Return home";
			}
			else if (!_canAfford)
			{
				travelString = "Not Enough Life";
			}
			else if (_travelCost == 0)
			{
				travelString = "Travel to Temple";
			}
			else
			{
				travelString = "Travel Here";
			}

			travelString   += _region.IsCleared() ? " - Region Cleared" : "";
			_costText.text =  travelString;
		}

		public void Show(Player player)
		{
			_hidden     = false;
			_distance   = RoutePlotter.RouteBetween(_region, player.TravelAction.GetCurrentRegion()).Count - 1;
			_travelCost = _distance;
			if (_region.GetRegionType() == RegionType.Gate || _region.GetRegionType() == RegionType.Temple && _region.IsTempleCleansed()) _travelCost = 0;
			_canAfford = player.CanAffordTravel(_travelCost);
			SetTravelCostText();
			_targetCentreAlpha = 0.6f;
			_targetNodeAlpha   = _canAfford ? 1f : 0.5f;
			float cleansedAlpha = _region.IsTempleCleansed() ? 0.5f : 0f;
			_sequence?.Kill();
			_sequence = DOTween.Sequence();
			_sequence.Insert(0f, _ringCanvas.DOFade(1f, 1f));
			_sequence.Insert(0f, _cleansedSprite.DOFade(cleansedAlpha, 1f));
			_sequence.SetUpdate(UpdateType.Normal, true);
		}

		public void Hide()
		{
			_hidden          = true;
			_targetNodeAlpha = 0f;
			_sequence?.Kill();
			_sequence = DOTween.Sequence();
			_sequence.Insert(0f, _ringCanvas.DOFade(0f, 1f));
			_sequence.Insert(0f, _cleansedSprite.DOFade(0f, 1f));
			_sequence.SetUpdate(UpdateType.Normal, true);
		}

		public void SetRegion(Region region)
		{
			_region = region;
			string nameString = region.GetRegionType() == RegionType.None ? "Unknown Region" : region.Name;
			_nameText.text = nameString;
			LoseFocus(0f);
			if (gameObject.activeInHierarchy) StartCoroutine(FadeInLetters());
			AssignSprite(region.GetRegionType());
		}

		private void AssignSprite(RegionType regionType)
		{
			if (_animalSprite   == null) _animalSprite   = Resources.Load<Sprite>("Images/Regions/Animal");
			if (_dangerSprite   == null) _dangerSprite   = Resources.Load<Sprite>("Images/Regions/Danger");
			if (_gateSprite     == null) _gateSprite     = Resources.Load<Sprite>("Images/Regions/Gate");
			if (_fountainSprite == null) _fountainSprite = Resources.Load<Sprite>("Images/Regions/Fountain");
			if (_monumentSprite == null) _monumentSprite = Resources.Load<Sprite>("Images/Regions/Monument");
			if (_shelterSprite  == null) _shelterSprite  = Resources.Load<Sprite>("Images/Regions/Shelter");
			if (_shrineSprite   == null) _shrineSprite   = Resources.Load<Sprite>("Images/Regions/Shrine");
			if (_templeSprite   == null) _templeSprite   = Resources.Load<Sprite>("Images/Regions/Temple");
			if (_cacheSprite    == null) _cacheSprite    = Resources.Load<Sprite>("Images/Regions/Cache");
			if (_noneSprite     == null) _noneSprite     = Resources.Load<Sprite>("Images/Regions/None");
			switch (regionType)
			{
				case RegionType.Shelter:
					_icon.sprite = _shelterSprite;
					break;
				case RegionType.Gate:
					_icon.sprite = _gateSprite;
					break;
				case RegionType.Temple:
					_icon.sprite = _templeSprite;
					break;
				case RegionType.Animal:
					_icon.sprite = _animalSprite;
					break;
				case RegionType.Danger:
					_icon.sprite = _dangerSprite;
					break;
				case RegionType.Fountain:
					_icon.sprite = _fountainSprite;
					break;
				case RegionType.Monument:
					_icon.sprite = _monumentSprite;
					break;
				case RegionType.Shrine:
					_icon.sprite = _shrineSprite;
					break;
				case RegionType.Cache:
					_icon.sprite = _cacheSprite;
					break;
				default:
					_icon.sprite = _noneSprite;
					break;
			}
		}

		public void Update()
		{
			if (_nodeCanvas.alpha == 0 && _targetNodeAlpha == 0f) return;
			float currentCentreAlpha    = _centreCanvas.alpha;
			float centreAlphaDifference = _targetCentreAlpha - currentCentreAlpha;
			if (Mathf.Abs(centreAlphaDifference) > 0.005f)
			{
				centreAlphaDifference =  Time.deltaTime > Mathf.Abs(centreAlphaDifference) ? centreAlphaDifference : Time.deltaTime * centreAlphaDifference.Polarity();
				_centreCanvas.alpha   += centreAlphaDifference;
			}

			_selectedImage.SetAlpha(Mathf.Clamp(_centreCanvas.alpha - 0.8f, 0f, 1f));

			float currentNodeAlpha    = _nodeCanvas.alpha;
			float nodeAlphaDifference = _targetNodeAlpha - currentNodeAlpha;
			if (Mathf.Abs(nodeAlphaDifference) > 0.01f)
			{
				nodeAlphaDifference =  Time.deltaTime > Mathf.Abs(nodeAlphaDifference) ? nodeAlphaDifference : Time.deltaTime * nodeAlphaDifference.Polarity();
				_nodeCanvas.alpha   += nodeAlphaDifference;
			}
		}

		public int GetTravelCost() => _travelCost;

		public int GetDistance() => _distance;

		private IEnumerator FadeInLetters()
		{
			_costText.color = UiAppearanceController.InvisibleColour;
			_costText.DOColor(UiAppearanceController.FadedColour, 1f);
			yield return _textReveal.Reveal(_nameText.text, s => _nameText.text = s);
		}

		public void GainFocus()
		{
			if (_hidden || !_canAfford) return;
			_targetCentreAlpha = 1f;
			_border.SetSelected();
			transform.DOScale(Vector2.one * 1.25f, 1f).SetUpdate(UpdateType.Normal, true);
			MapMenuController.Instance().SetRoute(_region);
		}

		public void LoseFocus(float time = 1f)
		{
			if (_hidden || !_canAfford) return;
			_targetCentreAlpha = 0.5f;
			_border.SetActive();
			transform.DOScale(Vector2.one, time).SetUpdate(UpdateType.Normal, true);
		}
	}
}