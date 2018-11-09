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
        private TextMeshProUGUI _nameText, _costText, _claimText;

        private CanvasGroup _canvas;
        private SpriteRenderer _ring1, _ring2, _ring3, _icon, _shadow;
        private UIBorderController _border;
        private static Sprite _animalSprite, _dangerSprite, _gateSprite, _fountainSprite, _monumentSprite, _shelterSprite, _shrineSprite, _templeSprite, _noneSprite;
        private int _gritCost;
        private Region _region;
        private ParticleSystem _claimedParticles;

        private const float Ring1Alpha = 0.1f;
        private const float Ring2Alpha = 0.1f;
        private const float Ring3Alpha = 0.25f;
        private const float IconAlpha = 0.8f;
        private const float FadeTime = 0.5f;

        private AudioSource _audioSource;
        public AudioClip _enterClip;

        public void Awake()
        {
            _canvas = gameObject.FindChildWithName<CanvasGroup>("Canvas");
            _nameText = gameObject.FindChildWithName<TextMeshProUGUI>("Name");
            _costText = gameObject.FindChildWithName<TextMeshProUGUI>("Cost");
            _claimText = gameObject.FindChildWithName<TextMeshProUGUI>("Claim Bonus");
            _audioSource = GetComponent<AudioSource>();
            _ring1 = gameObject.FindChildWithName<SpriteRenderer>("Ring 1");
            _ring2 = gameObject.FindChildWithName<SpriteRenderer>("Ring 2");
            _ring3 = gameObject.FindChildWithName<SpriteRenderer>("Ring 3");
            _icon = gameObject.FindChildWithName<SpriteRenderer>("Icon");
            _shadow = gameObject.FindChildWithName<SpriteRenderer>("Shadow");
            _border = gameObject.FindChildWithName<UIBorderController>("Border");
            _border.SetActive();
            _claimedParticles = gameObject.FindChildWithName<ParticleSystem>("Claimed");
        }

        private void SetClaimParticlesActive(bool active)
        {
            ParticleSystem claimedParticles = gameObject.FindChildWithName<ParticleSystem>("Claimed");
            if (active) claimedParticles.Play();
        }

        private void SetTrailAlpha(float alpha)
        {
            ParticleSystem.TrailModule trails = _claimedParticles.trails;
            Color c = trails.colorOverTrail.color;
            c.a = alpha;
            trails.colorOverTrail = c;
        }

        private float GetTrailAlpha()
        {
            return _claimedParticles.trails.colorOverTrail.color.a;
        }

        public void Show()
        {
            _gritCost = RoutePlotter.RouteBetween(_region, CharacterManager.SelectedCharacter.TravelAction.GetCurrentNode()).Count - 1;
            if (_region.GetRegionType() == RegionType.Gate) _costText.text = "Return home";
            else _costText.text = _gritCost + " Grit";
            _ring1.DOFade(Ring1Alpha, FadeTime).SetUpdate(UpdateType.Normal, true);
            _ring2.DOFade(Ring2Alpha, FadeTime).SetUpdate(UpdateType.Normal, true);
            _ring3.DOFade(Ring3Alpha, FadeTime).SetUpdate(UpdateType.Normal, true);
            _icon.DOFade(IconAlpha, FadeTime).SetUpdate(UpdateType.Normal, true);
            _shadow.DOFade(1f, FadeTime).SetUpdate(UpdateType.Normal, true);
            DOTween.To(GetTrailAlpha, SetTrailAlpha, 1f, FadeTime).SetUpdate(UpdateType.Normal, true);
            _canvas.DOFade(1f, FadeTime).SetUpdate(UpdateType.Normal, true);
        }

        public void Hide()
        {
            _ring1.DOFade(0f, FadeTime).SetUpdate(UpdateType.Normal, true);
            _ring2.DOFade(0f, FadeTime).SetUpdate(UpdateType.Normal, true);
            _ring3.DOFade(0f, FadeTime).SetUpdate(UpdateType.Normal, true);
            _icon.DOFade(0f, FadeTime).SetUpdate(UpdateType.Normal, true);
            _shadow.DOFade(0f, FadeTime).SetUpdate(UpdateType.Normal, true);
            DOTween.To(GetTrailAlpha, SetTrailAlpha, 0f, FadeTime).SetUpdate(UpdateType.Normal, true);
            _canvas.DOFade(0f, FadeTime).SetUpdate(UpdateType.Normal, true);
        }

        public void SetRegion(Region region)
        {
            _region = region;
            SetClaimParticlesActive(region.ClaimRemaining > 0);
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
                    _icon.sprite = _noneSprite;
                    break;
            }
        }

        public void Enter()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.PlayOneShot(_enterClip);
        }

        public void Update()
        {
            _ring1.transform.Rotate(new Vector3(0, 0, 1), 5 * Time.deltaTime);
            _ring2.transform.Rotate(new Vector3(0, 0, 1), 3 * Time.deltaTime);
            _ring3.transform.Rotate(new Vector3(0, 0, 1), -4 * Time.deltaTime);
        }

        public int GetGritCost()
        {
            return _gritCost;
        }

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
            _audioSource.pitch = Random.Range(0.75f, 1.25f);
            _audioSource.DOFade(0.5f, 1);

            _icon.DOFade(IconAlpha * 2f, 1f);
            _ring1.DOFade(Ring1Alpha * 2f, 1f);
            _ring2.DOFade(Ring2Alpha * 2f, 1f);
            _ring3.DOFade(Ring3Alpha * 2f, 1f);

            _nameText.DOFade(1f, 1f);
            _costText.DOFade(1f, 1f);
            _claimText.DOFade(1f, 1f);
            _border.SetSelected();

            transform.DOScale(Vector2.one * 1.25f, 1f);
            MapMenuController.SetRoute(_region);
            MapMovementController.UpdateGrit(_gritCost);
        }

        public void LoseFocus(float time = 1f)
        {
            _audioSource.DOFade(0, time);

            _icon.DOFade(IconAlpha, time);
            _ring1.DOFade(Ring1Alpha, time);
            _ring2.DOFade(Ring2Alpha, time);
            _ring3.DOFade(Ring3Alpha, time);

            _nameText.DOFade(0.4f, time);
            _costText.DOFade(0.4f, time);
            _claimText.DOFade(0.4f, time);
            _border.SetActive();

            transform.DOScale(Vector2.one, time);
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