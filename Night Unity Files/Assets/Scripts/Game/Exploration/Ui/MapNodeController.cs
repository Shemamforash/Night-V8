using System;
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
        private readonly List<Letter> _letters = new List<Letter>();
        private string _completeWord;
        private int _currentLetter;
        private float _currentTime;
        private bool _doneFading;
        private TextMeshProUGUI _fadeText, _costText, _claimText;

        private SpriteRenderer _ring1, _ring2, _ring3, _icon;
        private Image _inactive, _active;
        private static Sprite _animalSprite, _dangerSprite, _gateSprite, _fountainSprite, _monumentSprite, _shelterSprite, _shrineSprite, _templeSprite;
        private int _enduranceCost;
        private Region _region;

        private readonly Color _ring1Colour = new Color(1, 1, 1, 0.1f);
        private readonly Color _ring2Colour = new Color(1, 1, 1, 0.1f);
        private readonly Color _ring3Colour = new Color(1, 1, 1, 0.25f);
        private readonly Color _iconColor = new Color(1, 1, 1, 0.8f);

        private AudioSource _audioSource;
        public AudioClip _enterClip;

        public void Awake()
        {
            _fadeText = gameObject.FindChildWithName<TextMeshProUGUI>("Fade");
            _costText = gameObject.FindChildWithName<TextMeshProUGUI>("Cost");
            _claimText = gameObject.FindChildWithName<TextMeshProUGUI>("Claim Bonus");
            _audioSource = GetComponent<AudioSource>();
            _ring1 = gameObject.FindChildWithName<SpriteRenderer>("Ring 1");
            _ring2 = gameObject.FindChildWithName<SpriteRenderer>("Ring 2");
            _ring3 = gameObject.FindChildWithName<SpriteRenderer>("Ring 3");
            _icon = gameObject.FindChildWithName<SpriteRenderer>("Icon");
            _inactive = gameObject.FindChildWithName<Image>("Inactive");
            _active = gameObject.FindChildWithName<Image>("Active");
        }

        private void SetClaimParticlesActive(bool active)
        {
            ParticleSystem claimedParticles = gameObject.FindChildWithName<ParticleSystem>("Claimed");
            if (active) claimedParticles.Play();
        }

        public void SetRegion(Region region)
        {
            _region = region;
            SetClaimParticlesActive(region.ClaimRemaining > 0);
            _enduranceCost = RoutePlotter.RouteBetween(region, CharacterManager.SelectedCharacter.TravelAction.GetCurrentNode()).Count - 1;
            if (region.GetRegionType() == RegionType.Gate) _enduranceCost = 0;
            for (int i = 0; i < region.Name.Length; ++i)
            {
                _letters.Add(new Letter(region.Name[i].ToString()));
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
                case RegionType.Nightmare:
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
                    Debug.Log(regionType);
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Enter()
        {
            Camera.main.DOOrthoSize(3, 1f);
            Camera.main.transform.DOMove(transform.position, 0.25f);
            _audioSource = GetComponent<AudioSource>();
            _audioSource.PlayOneShot(_enterClip);
        }

        public void Update()
        {
            _ring1.transform.Rotate(new Vector3(0, 0, 1), 5 * Time.deltaTime);
            _ring2.transform.Rotate(new Vector3(0, 0, 1), 3 * Time.deltaTime);
            _ring3.transform.Rotate(new Vector3(0, 0, 1), -4 * Time.deltaTime);
        }

        private IEnumerator FadeInLetters()
        {
            if (_region.GetRegionType() == RegionType.Gate)
            {
                _costText.text = "Return home";
            }
            else
            {
                _costText.text = _enduranceCost + " end";
            }

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
                _fadeText.text = _completeWord;
                yield return null;
            }
        }

        public void GainFocus()
        {
            _audioSource.pitch = Random.Range(0.75f, 1.25f);
            _audioSource.DOFade(1, 1);
            _icon.DOColor(_iconColor, 1f);
            _ring1.DOColor(_ring1Colour, 1f);
            _ring2.DOColor(_ring2Colour, 1f);
            _ring3.DOColor(_ring3Colour, 1f);
            _fadeText.DOColor(Color.white, 1f);
            _costText.DOColor(Color.white, 1f);
            _claimText.DOColor(Color.white, 1f);
            _active.DOColor(Color.white, 1f);
            _inactive.DOColor(Color.white, 1f);
            transform.DOScale(Vector2.one * 1.25f, 1f);
            MapGenerator.SetRoute(_region);
            MapMovementController.UpdateEndurance(_enduranceCost);
        }

        public void LoseFocus(float time = 1f)
        {
            _audioSource.DOFade(0, 1);
            _icon.DOColor(UiAppearanceController.FadedColour, time);
            _ring1.DOColor(UiAppearanceController.InvisibleColour, time);
            _ring2.DOColor(UiAppearanceController.InvisibleColour, time);
            _ring3.DOColor(UiAppearanceController.FadedColour, time);
            _fadeText.DOColor(UiAppearanceController.FadedColour, time);
            _costText.DOColor(UiAppearanceController.FadedColour, time);
            _claimText.DOColor(UiAppearanceController.FadedColour, time);
            _inactive.DOColor(UiAppearanceController.FadedColour, time);
            _active.DOColor(UiAppearanceController.InvisibleColour, time);
            transform.DOScale(Vector2.one, time);
            MapMovementController.UpdateEndurance(0);
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