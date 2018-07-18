using System.Collections;
using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Player;
using Game.Combat.Ui;
using SamsHelper.Input;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
    public class RiteShrineBehaviour : MonoBehaviour, IInputListener
    {
        public const int Width = 5;
        private static GameObject _riteShrinePrefab;
        private List<Characters.Player> _visitedPlayers = new List<Characters.Player>();
        private RiteColliderBehaviour _collider1, _collider2, _collider3;
        private List<BrandManager.Brand> _brandChoice;
        private BrandManager.Brand _targetBrand;

        public void Awake()
        {
            _collider1 = Helper.FindChildWithName<RiteColliderBehaviour>(gameObject, "Collider 1");
            _collider2 = Helper.FindChildWithName<RiteColliderBehaviour>(gameObject, "Collider 2");
            _collider3 = Helper.FindChildWithName<RiteColliderBehaviour>(gameObject, "Collider 3");
            _brandChoice = CharacterManager.SelectedCharacter.BrandManager.GetBrandChoice();
            if (_brandChoice.Count < 3)
            {
                _collider2.gameObject.SetActive(false);
            }

            if (_brandChoice.Count < 2)
            {
                _collider1.gameObject.SetActive(false);
            }
        }

        public static void Generate(Vector2 position)
        {
            if (_riteShrinePrefab == null) _riteShrinePrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Rite Shrine");
            GameObject riteShrineObject = Instantiate(_riteShrinePrefab);
            riteShrineObject.transform.position = position;
        }

        public void EnterShrineCollider(RiteColliderBehaviour riteColliderBehaviour)
        {
            InputHandler.RegisterInputListener(this);
            if (riteColliderBehaviour == _collider1)
            {
                _targetBrand = _brandChoice[0];
            }
            else if (riteColliderBehaviour == _collider2)
            {
                _targetBrand = _brandChoice[1];
            }
            else if (riteColliderBehaviour == _collider3)
            {
                _targetBrand = _brandChoice[2];
            }

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
            FadeCandles();
            Destroy(_collider1);
            Destroy(_collider2);
            Destroy(_collider3);
        }

        private void FadeCandles()
        {
            foreach (ParticleSystem candle in transform.Find("Candles").GetComponentsInChildren<ParticleSystem>())
            {
                StartCoroutine(FadeCandle(candle));
            }
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