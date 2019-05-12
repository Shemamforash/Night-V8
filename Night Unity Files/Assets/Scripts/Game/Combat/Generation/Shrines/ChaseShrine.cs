using System.Collections;
using Extensions;
using Game.Combat.Enemies;
using Game.Global;

using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
	public class ChaseShrine : ShrineBehaviour
	{
		private static GameObject     _shrinePickupPrefab;
		private        GameObject     _currentPickup;
		private        ParticleSystem _pickupDropMarkerA;
		private        ParticleSystem _pickupDropMarkerB;
		private        SpriteRenderer _pickupGlow;
		private        int            _pickupsLeft = 5;

		public void Start()
		{
			_pickupGlow        = gameObject.FindChildWithName<SpriteRenderer>("Pickup Glow");
			_pickupDropMarkerA = gameObject.FindChildWithName<ParticleSystem>("Ring A");
			_pickupDropMarkerB = gameObject.FindChildWithName<ParticleSystem>("Ring B");
		}

		protected override void StartChallenge()
		{
			StartCoroutine(StartSpawning());
		}

		protected override string GetInstructionText() => "Return pure essence to the shrine before the timer ends";

		private void SpawnPickup()
		{
			if (_shrinePickupPrefab == null) _shrinePickupPrefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Shrine Pickup");
			_currentPickup                    = Instantiate(_shrinePickupPrefab);
			_currentPickup.transform.position = WorldGrid.GetCellNearMe(WorldGrid.WorldToCellPosition(Vector2.zero), 6f, 4f).Position;
			_currentPickup.GetComponent<ShrinePickup>().SetShrine(this);
		}

		private void SpawnChaser()
		{
			if (CombatManager.Instance().Enemies().Count == 4) return;
			CombatManager.Instance().SpawnEnemy(EnemyType.Shadow, WorldGrid.GetCellNearMe(WorldGrid.WorldToCellPosition(transform.position), 5f, 2f).Position);
		}

		private IEnumerator StartSpawning()
		{
			_pickupsLeft = (int) (WorldState.Difficulty() / 10f + 3);
			float shrineTimeMax      = 15f * _pickupsLeft;
			float currentTime        = shrineTimeMax;
			float spawnChaserTimeMax = 10f;
			float spawnChaserTime    = 0f;
			while (_pickupsLeft > 0 && currentTime > 0f)
			{
				if (!CombatManager.Instance().IsCombatActive()) yield return null;
				EventTextController.SetOverrideText(_pickupsLeft + " pure essence remains");
				if (_currentPickup == null) SpawnPickup();
				spawnChaserTime -= Time.deltaTime;
				if (spawnChaserTime < 0f)
				{
					SpawnChaser();
					spawnChaserTime = spawnChaserTimeMax;
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
			float maxTime     = 0.5f;
			float currentTime = maxTime;
			while (currentTime > 0)
			{
				if (!CombatManager.Instance().IsCombatActive()) yield return null;
				_pickupGlow.color =  new Color(1, 1, 1, currentTime / maxTime);
				currentTime       -= Time.deltaTime;
				yield return null;
			}

			_pickupGlow.color = UiAppearanceController.InvisibleColour;
		}
	}
}