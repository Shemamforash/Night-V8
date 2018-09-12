using System.Collections;
using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Player;
using Game.Combat.Ui;
using Game.Exploration.Regions;
using SamsHelper.Input;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Combat.Generation.Shrines
{
    public class RiteShrineBehaviour : MonoBehaviour, IInputListener
    {
        public const int Width = 5;
        private static GameObject _riteShrinePrefab;
        private RiteColliderBehaviour _collider1, _collider2, _collider3;
        private List<BrandManager.Brand> _brandChoice;
        private BrandManager.Brand _targetBrand;
        private RiteColliderBehaviour _targetRiteCollider;

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
            InputHandler.UnregisterInputListener(this);
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
            InputHandler.RegisterInputListener(this);
            _targetRiteCollider = riteColliderBehaviour;
            if (riteColliderBehaviour == _collider1)
                _targetBrand = _brandChoice[0];
            else if (riteColliderBehaviour == _collider2)
                _targetBrand = _brandChoice[1];
            else if (riteColliderBehaviour == _collider3)
                _targetBrand = _brandChoice[2];

            PlayerUi.SetEventText("Accept the " + _targetBrand.GetName() + " [T]");
        }

        public void ExitShrineCollider()
        {
            _targetBrand = null;
            InputHandler.UnregisterInputListener(this);
            PlayerUi.FadeTextOut();
        }

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (axis != InputAxis.TakeItem) return;
            PlayerCombat.Instance.Player.BrandManager.SetActiveBrand(_targetBrand);
            PlayerUi.SetEventText("The Rite begins...");
            PlayerUi.FadeTextOut();
            InputHandler.UnregisterInputListener(this);
            FadeCandles(_targetRiteCollider.transform);
            Destroy(_targetRiteCollider);
        }

        private void FadeCandles(Transform riteTransform)
        {
            Assert.IsNotNull(riteTransform);
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
                currentTime -= Time.deltaTime;
                float newEmission = startEmission * currentTime / 2f;
                emission.rateOverTime = newEmission;
                yield return null;
            }

            emission.rateOverTime = 0f;
        }

        public void OnInputUp(InputAxis axis)
        {
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }
    }
}