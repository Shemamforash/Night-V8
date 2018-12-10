using System.Collections;
using Game.Combat.Enemies;
using Game.Global;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
    public class ChaseShrine : ShrineBehaviour
    {
        private int _pickupsLeft = 5;
        private static GameObject _shrinePickupPrefab;
        private GameObject _currentPickup;
        private SpriteRenderer _pickupGlow;
        private ParticleSystem _pickupDropMarkerA;
        private ParticleSystem _pickupDropMarkerB;

        public void Start()
        {
            _pickupGlow = gameObject.FindChildWithName<SpriteRenderer>("Pickup Glow");
            _pickupDropMarkerA = gameObject.FindChildWithName<ParticleSystem>("Ring A");
            _pickupDropMarkerB = gameObject.FindChildWithName<ParticleSystem>("Ring B");
        }

        protected override void StartChallenge()
        {
            StartCoroutine(SpawnChasers());
        }

        protected override string GetInstructionText() => "Return pure essence to the shrine before the timer ends";

        private IEnumerator SpawnChasers()
        {
            float shrineTimeMax = 20f;
            float currentTime = shrineTimeMax;
            _pickupsLeft = (int) (WorldState.Difficulty() / 10f + 3);
            while (_pickupsLeft > 0 && currentTime > 0f)
            {
                if (!CombatManager.IsCombatActive()) yield return null;
                EventTextController.SetOverrideText(_pickupsLeft + " pure essence remains");
                if (_currentPickup == null)
                {
                    if (_shrinePickupPrefab == null) _shrinePickupPrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Shrine Pickup");
                    _currentPickup = Instantiate(_shrinePickupPrefab);
                    _currentPickup.transform.position = PathingGrid.GetCellNearMe(PathingGrid.WorldToCellPosition(transform.position), 6f, 4f).Position;
                    _currentPickup.GetComponent<ShrinePickup>().SetShrine(this);
                    EnemyBehaviour b = CombatManager.SpawnEnemy(EnemyType.Shadow, PathingGrid.GetCellNearMe(PathingGrid.WorldToCellPosition(transform.position), 5f, 2f).Position);
                    currentTime = shrineTimeMax;
                }

                currentTime -= Time.deltaTime;
                UpdateCountdown(currentTime, shrineTimeMax);
                yield return null;
            }

            EventTextController.CloseOverrideText();
            EndChallenge();
        }

        public void StartDropMarker()
        {
            _pickupDropMarkerA.Play();
            _pickupDropMarkerB.Play();
        }

        private void StopDropMarker()
        {
            _pickupDropMarkerA.Stop();
            _pickupDropMarkerB.Stop();
        }

        protected override void EndChallenge()
        {
            base.EndChallenge();
            StopDropMarker();
            if (_currentPickup != null)
            {
                Destroy(_currentPickup);
                Fail();
            }

            if (_pickupsLeft != 0)
            {
                Fail();
            }
        }

        public void ReturnPickup()
        {
            StartCoroutine(ReturnPickupGlow());
            _currentPickup = null;
            --_pickupsLeft;
            if (_pickupsLeft == 0) Succeed();
        }

        private IEnumerator ReturnPickupGlow()
        {
            StopDropMarker();
            float maxTime = 0.5f;
            float currentTime = maxTime;
            while (currentTime > 0)
            {
                if (!CombatManager.IsCombatActive()) yield return null;
                _pickupGlow.color = new Color(1, 1, 1, currentTime / maxTime);
                currentTime -= Time.deltaTime;
                yield return null;
            }

            _pickupGlow.color = UiAppearanceController.InvisibleColour;
        }
    }
}