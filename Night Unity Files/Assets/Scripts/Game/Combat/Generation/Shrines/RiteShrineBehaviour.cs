using System.Collections;
using System.Collections.Generic;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Exploration.Regions;
using NUnit.Framework;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
    public class RiteShrineBehaviour : BasicShrineBehaviour, ICombatEvent
    {
        private static GameObject _riteShrinePrefab;
        private RiteColliderBehaviour _collider1, _collider2, _collider3;
        private List<Brand> _brandChoice;
        private int _targetBrand = -1;
        private RiteColliderBehaviour _targetRiteCollider;
        private static RiteShrineBehaviour _instance;

        public void Awake()
        {
            _instance = this;
        }

        public static RiteShrineBehaviour Instance()
        {
            return _instance;
        }

        public static void Generate(Region region)
        {
            if (_riteShrinePrefab == null) _riteShrinePrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Rite Shrine");
            GameObject riteShrineObject = Instantiate(_riteShrinePrefab);
            riteShrineObject.GetComponent<RiteShrineBehaviour>().SetRites(region.RitesRemaining);
            riteShrineObject.transform.position = Vector2.zero;
            PathingGrid.AddBlockingArea(Vector2.zero, 1.5f);
        }

        private void SetRites(int ritesRemaining)
        {
            _collider1 = gameObject.FindChildWithName<RiteColliderBehaviour>("Collider 1");
            _collider2 = gameObject.FindChildWithName<RiteColliderBehaviour>("Collider 2");
            _collider3 = gameObject.FindChildWithName<RiteColliderBehaviour>("Collider 3");
            _brandChoice = CharacterManager.SelectedCharacter.BrandManager.GetBrandChoice(ritesRemaining);
            Debug.Log(_brandChoice.Count);
            if (_brandChoice.Count < 3)
            {
                StopCandles(_collider3.transform);
                Destroy(_collider3);
            }

            if (_brandChoice.Count < 2)
            {
                StopCandles(_collider2.transform);
                Destroy(_collider2);
            }

            if (_brandChoice.Count != 0) return;
            GetComponent<CompassItem>().Die();
            StopCandles(_collider1.transform);
            Destroy(_collider1);
            Debug.Log("Destroyed");
            Destroy(this);
        }

        private void StopCandles(Transform candleParent)
        {
            foreach (ParticleSystem candle in candleParent.Find("Candles").GetComponentsInChildren<ParticleSystem>())
            {
                candle.Stop();
                candle.Clear();
            }
        }

        public void EnterShrineCollider(RiteColliderBehaviour riteColliderBehaviour)
        {
            _targetRiteCollider = riteColliderBehaviour;
            if (riteColliderBehaviour == _collider1)
                _targetBrand = 0;
            else if (riteColliderBehaviour == _collider2)
                _targetBrand = 1;
            else if (riteColliderBehaviour == _collider3)
                _targetBrand = 2;
        }

        public void ExitShrineCollider()
        {
            _targetBrand = -1;
        }

        private void FadeCandles(Transform riteTransform)
        {
            foreach (ParticleSystem candle in riteTransform.Find("Candles").GetComponentsInChildren<ParticleSystem>())
                StartCoroutine(FadeCandle(candle));
        }

        private IEnumerator FadeCandle(ParticleSystem candle)
        {
            ParticleSystem.EmissionModule emission = candle.emission;
            float startEmission = emission.rateOverTime.constant;
            float currentTime = 1f;
            while (currentTime > 0f)
            {
                if (!CombatManager.IsCombatActive()) yield return null;
                currentTime -= Time.deltaTime;
                float newEmission = startEmission * currentTime / 2f;
                emission.rateOverTime = newEmission;
                yield return null;
            }

            emission.rateOverTime = 0f;
        }

        public float InRange()
        {
            if (_targetBrand != -1) TutorialManager.TryOpenTutorial(15);
            return _targetBrand;
        }

        public string GetEventText()
        {
            Brand brand = _brandChoice[_targetBrand];
            return "Accept the " + brand.GetName() + " [T]\n<size=30>" + brand.GetRequirementText() + "</size>";
        }

        public void Activate()
        {
            BrandManager brandManager = PlayerCombat.Instance.Player.BrandManager;
            Brand brand = _brandChoice[_targetBrand];
            Assert.IsNotNull(brand);
            if (brandManager.TryActivateBrand(brand))
            {
                ActivateBrand();
                return;
            }

            BrandReplaceMenuController.Show(this, brand);
        }

        public void ActivateBrand()
        {
            Triggered = true;
            FadeCandles(_targetRiteCollider.transform);
            Destroy(_targetRiteCollider);
            _brandChoice[_targetBrand] = null;
            _targetBrand = -1;
            if (_brandChoice.TrueForAll(b => b == null)) GetComponent<CompassItem>().Die();
        }

        protected override void StartShrine()
        {
        }
    }
}