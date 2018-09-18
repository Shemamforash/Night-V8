using System.Collections;
using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Combat.Ui;
using Game.Exploration.Regions;
using SamsHelper.Input;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Combat.Generation.Shrines
{
    public class RiteShrineBehaviour : BasicShrineBehaviour, ICombatEvent
    {
        public const int Width = 5;
        private static GameObject _riteShrinePrefab;
        private RiteColliderBehaviour _collider1, _collider2, _collider3;
        private List<BrandManager.Brand> _brandChoice;
        private BrandManager.Brand _targetBrand;
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
            riteShrineObject.transform.position = region.ShrinePosition;
            PathingGrid.AddBlockingArea(region.ShrinePosition, 1.5f);
        }

        private void SetRites(int ritesRemaining)
        {
            _collider1 = gameObject.FindChildWithName<RiteColliderBehaviour>("Collider 1");
            _collider2 = gameObject.FindChildWithName<RiteColliderBehaviour>("Collider 2");
            _collider3 = gameObject.FindChildWithName<RiteColliderBehaviour>("Collider 3");
            _brandChoice = CharacterManager.SelectedCharacter.BrandManager.GetBrandChoice(ritesRemaining);
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
            StopCandles(_collider1.transform);
            Destroy(_collider1);
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
                _targetBrand = _brandChoice[0];
            else if (riteColliderBehaviour == _collider2)
                _targetBrand = _brandChoice[1];
            else if (riteColliderBehaviour == _collider3)
                _targetBrand = _brandChoice[2];
        }

        public void ExitShrineCollider()
        {
            _targetBrand = null;
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
            float currentTime = 2f;
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
            return _targetBrand == null ? -1 : 1;
        }

        public string GetEventText()
        {
            return "Accept the " + _targetBrand.GetName() + " [T]\n<size=30>" + _targetBrand.GetRequirementText() + "</size>";
        }

        public void Activate()
        {
            Triggered = true;
            PlayerCombat.Instance.Player.BrandManager.SetActiveBrand(_targetBrand);
            FadeCandles(_targetRiteCollider.transform);
            Destroy(_targetRiteCollider);
            _targetBrand = null;
        }

        protected override void StartShrine()
        {
        }
    }
}