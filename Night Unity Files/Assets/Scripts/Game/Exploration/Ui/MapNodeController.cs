using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Characters;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Game.Exploration.Ui
{
    public class MapNodeController : MonoBehaviour
    {
        private const float LetterFadeInDuration = 0.5f;
        private static Sprite _animalSprite, _dangerSprite, _gateSprite, _fountainSprite, _monumentSprite, _shelterSprite, _shrineSprite, _templeSprite, _noneSprite;

        private readonly List<Letter> _letters = new List<Letter>();
        private string _completeWord;
        private int _currentLetter;
        private float _currentTime;
        private bool _doneFading;
        private TextMeshProUGUI _nameText, _costText, _claimText;

        private Image _icon, _selectedImage;
        private float _targetCentreAlpha, _targetNodeAlpha;
        private CanvasGroup _nodeCanvas, _centreCanvas;
        private UIBorderController _border;
        private int _gritCost;
        private Region _region;
        private ParticleSystem _claimedParticles;

        private bool _hidden;
        private bool _canAfford;
        private int _distance;

        public void Awake()
        {
            _nodeCanvas = gameObject.FindChildWithName<CanvasGroup>("Canvas");
            _centreCanvas = gameObject.FindChildWithName<CanvasGroup>("Centre Canvas");

            _selectedImage = gameObject.FindChildWithName<Image>("Selected");
            _nameText = gameObject.FindChildWithName<TextMeshProUGUI>("Name");
            _costText = gameObject.FindChildWithName<TextMeshProUGUI>("Cost");
            _claimText = gameObject.FindChildWithName<TextMeshProUGUI>("Claim Bonus");
            _icon = gameObject.FindChildWithName<Image>("Icon");

            _border = gameObject.FindChildWithName<UIBorderController>("Border");
            _border.SetActive();
            _claimedParticles = gameObject.FindChildWithName<ParticleSystem>("Claimed");
        }

        private void SetTrailAlpha(float alpha)
        {
            ParticleSystem.TrailModule trails = _claimedParticles.trails;
            trails.colorOverTrail = new Color(1, 1, 1, alpha);
        }

        private float GetTrailAlpha()
        {
            return _claimedParticles.trails.colorOverTrail.color.a;
        }

        private void SetGritText()
        {
            string gritString;
            if (_distance == 0) gritString = "Current Location";
            else if (_region.GetRegionType() == RegionType.Gate) gritString = "Return home";
            else if (!_canAfford) gritString = "Not Enough Grit";
            else if (_gritCost == 0) gritString = "Travel to Temple";
            else gritString = _gritCost + " Grit";
            _costText.text = gritString;
        }

        public void Show()
        {
            _hidden = false;
            _distance = RoutePlotter.RouteBetween(_region, CharacterManager.SelectedCharacter.TravelAction.GetCurrentRegion()).Count - 1;
            _gritCost = _distance;
            if (_region.GetRegionType() == RegionType.Gate || (_region.GetRegionType() == RegionType.Temple && _region.IsTempleCleansed())) _gritCost = 0;
            _canAfford = CharacterManager.SelectedCharacter.CanAffordTravel(_gritCost);
            SetGritText();
            _targetCentreAlpha = 0.6f;
            _targetNodeAlpha = _canAfford ? 1f : 0.5f;
            float claimAlpha = _region.ClaimRemaining > 0 ? 1 : 0;
            DOTween.To(GetTrailAlpha, SetTrailAlpha, claimAlpha, 1f).SetUpdate(UpdateType.Normal, true);
        }

        public void Hide()
        {
            _hidden = true;
            _targetNodeAlpha = 0f;
            DOTween.To(GetTrailAlpha, SetTrailAlpha, 0f, 1f).SetUpdate(UpdateType.Normal, true);
        }

        public void SetRegion(Region region)
        {
            _region = region;
            string nameText = region.GetRegionType() == RegionType.None ? "Unknown Region" : region.Name;
            for (int i = 0; i < nameText.Length; ++i)
            {
                _letters.Add(new Letter(nameText[i].ToString()));
                if (i > 0) _letters[i - 1].SetNextLetter(_letters[i]);
            }

            LoseFocus(0f);
            _letters[0]?.StartFade();
            if (gameObject.activeInHierarchy) StartCoroutine(FadeInLetters());
            AssignSprite(region.GetRegionType());
        }

        private void AssignSprite(RegionType regionType)
        {
            if (_animalSprite == null) _animalSprite = Resources.Load<Sprite>("Images/Regions/Animal");
            if (_dangerSprite == null) _dangerSprite = Resources.Load<Sprite>("Images/Regions/Danger");
            if (_gateSprite == null) _gateSprite = Resources.Load<Sprite>("Images/Regions/Gate");
            if (_fountainSprite == null) _fountainSprite = Resources.Load<Sprite>("Images/Regions/Fountain");
            if (_monumentSprite == null) _monumentSprite = Resources.Load<Sprite>("Images/Regions/Monument");
            if (_shelterSprite == null) _shelterSprite = Resources.Load<Sprite>("Images/Regions/Shelter");
            if (_shrineSprite == null) _shrineSprite = Resources.Load<Sprite>("Images/Regions/Shrine");
            if (_templeSprite == null) _templeSprite = Resources.Load<Sprite>("Images/Regions/Temple");
            if (_noneSprite == null) _noneSprite = Resources.Load<Sprite>("Images/Regions/None");
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
                default:
                    _icon.sprite = _noneSprite;
                    break;
            }
        }

        public void Update()
        {
            if (_nodeCanvas.alpha == 0 && _targetNodeAlpha == 0f) return;
            float currentCentreAlpha = _centreCanvas.alpha;
            float centreAlphaDifference = _targetCentreAlpha - currentCentreAlpha;
            if (Mathf.Abs(centreAlphaDifference) > 0.005f)
            {
                centreAlphaDifference = Time.deltaTime > Mathf.Abs(centreAlphaDifference) ? centreAlphaDifference : Time.deltaTime * centreAlphaDifference.Polarity();
                _centreCanvas.alpha += centreAlphaDifference;
            }

            _selectedImage.SetAlpha(Mathf.Clamp(_centreCanvas.alpha - 0.8f, 0f, 1f));

            float currentNodeAlpha = _nodeCanvas.alpha;
            float nodeAlphaDifference = _targetNodeAlpha - currentNodeAlpha;
            if (Mathf.Abs(nodeAlphaDifference) > 0.01f)
            {
                nodeAlphaDifference = Time.deltaTime > Mathf.Abs(nodeAlphaDifference) ? nodeAlphaDifference : Time.deltaTime * nodeAlphaDifference.Polarity();
                _nodeCanvas.alpha += nodeAlphaDifference;
            }
        }

        public int GetGritCost() => _gritCost;

        public int GetDistance() => _distance;

        private IEnumerator FadeInLetters()
        {
            _claimText.text = _region.ClaimBenefitString();

            _claimText.color = UiAppearanceController.InvisibleColour;
            _claimText.DOColor(UiAppearanceController.FadedColour, 1f);

            _costText.color = UiAppearanceController.InvisibleColour;
            _costText.DOColor(UiAppearanceController.FadedColour, 1f);
            while (_doneFading == false)
            {
                _doneFading = true;
                _completeWord = "";
                _letters.ForEach(l => l.Update(this));
                _nameText.text = _completeWord;
                yield return null;
            }
        }

        public void GainFocus()
        {
            if (_hidden || !_canAfford) return;
            _targetCentreAlpha = 1f;
            _border.SetSelected();
            transform.DOScale(Vector2.one * 1.25f, 1f).SetUpdate(UpdateType.Normal, true);
            MapMenuController.SetRoute(_region);
            MapMovementController.UpdateGrit(_gritCost);
        }

        public void LoseFocus(float time = 1f)
        {
            if (_hidden || !_canAfford) return;
            _targetCentreAlpha = 0.5f;
            _border.SetActive();
            transform.DOScale(Vector2.one, time).SetUpdate(UpdateType.Normal, true);
            MapMovementController.UpdateGrit(0);
        }

        private class Letter
        {
            private readonly string _letter;
            private float _age;
            private bool _fading;
            private Letter _nextLetter;

            public Letter(string letter)
            {
                _letter = letter;
            }

            public void SetNextLetter(Letter nextLetter)
            {
                _nextLetter = nextLetter;
            }

            public void StartFade()
            {
                _fading = true;
            }

            public void Update(MapNodeController fadeIn)
            {
                string letterWithAlpha = "";
                if (_fading)
                {
                    if (_age > LetterFadeInDuration / 4) _nextLetter?.StartFade();

                    if (_age > LetterFadeInDuration)
                    {
                        letterWithAlpha = _letter;
                    }
                    else
                    {
                        float alpha = _age / LetterFadeInDuration;
                        _age += Time.deltaTime;
                        letterWithAlpha += LetterToHex(alpha);
                        fadeIn._doneFading = false;
                    }
                }
                else
                {
                    letterWithAlpha = LetterToHex(0f);
                }

                fadeIn._completeWord += letterWithAlpha;
            }

            private string LetterToHex(float alpha)
            {
                string hexString = "<color=#FFFFFF" + ((int) (alpha * 255)).ToString("X2") + ">" + _letter + "</color>";
                return hexString;
            }
        }
    }
}