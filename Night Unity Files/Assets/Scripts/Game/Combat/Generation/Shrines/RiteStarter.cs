using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Characters.CharacterActions;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper.Input;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
    public class RiteStarter : MonoBehaviour, ICombatEvent
    {
        private Brand _brand;
        private static GameObject _prefab;
        private static RiteStarter _instance;
        private bool _goToNextRegion;
        private SpriteRenderer _flashSprite, _dullFlashSprite;
        private AudioSource _audioSource;
        private bool _isTutorial;
        private bool _inRange;
        private Characters.Player _player;

        public void Awake()
        {
            _instance = this;
            _audioSource = GetComponent<AudioSource>();
            _flashSprite = gameObject.FindChildWithName<SpriteRenderer>("Flash");
            _dullFlashSprite = gameObject.FindChildWithName<SpriteRenderer>("Dull Flash");
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        public static bool Available()
        {
            return _instance == null;
        }

        public static void Generate(Brand brand)
        {
            if (_instance != null) return;
            Vector2 position = brand == null ? Vector2.zero : GetPosition();
            if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Rite Starter");
            GameObject riteObject = Instantiate(_prefab);
            RiteStarter riteStarter = riteObject.GetComponent<RiteStarter>();
            riteStarter._brand = brand;
            riteObject.transform.position = position;
        }

        public static void GenerateNextEnvironmentPortal()
        {
            if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Rite Starter");
            GameObject riteObject = Instantiate(_prefab);
            riteObject.transform.position = Vector2.zero;
            RiteStarter riteStarter = riteObject.GetComponent<RiteStarter>();
            riteStarter._goToNextRegion = true;
        }

        private static Vector2 GetPosition()
        {
//            List<Vector2> points = AdvancedMaths.GetPoissonDiscDistribution(100, 5);
//            foreach (Vector2 p in points)
//            {
//                if (p.magnitude > PathingGrid.CombatMovementDistance - 1) continue;
//                if (p.Distance(PlayerCombat.Position()) < 1) continue;
//                Vector2 newPoint = p + (Vector2) PlayerCombat.Position();
//                Vector2 topLeft = new Vector2(newPoint.x - 0.25f, newPoint.y + 0.25f);
//                Vector2 bottomRight = new Vector2(newPoint.x + 0.25f, newPoint.y - 0.25f);
//                if (!PathingGrid.IsSpaceAvailable(topLeft, bottomRight)) continue;
//                return newPoint;
//            }

            return WorldGrid.GetCellNearMe(PlayerCombat.Instance.CurrentCell(), 5, 1).Position;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _inRange = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _inRange = true;
        }

        public static RiteStarter Instance() => _instance;

        private void Flash()
        {
            _audioSource.Play();
            _dullFlashSprite.SetAlpha(1);
            _flashSprite.SetAlpha(1);
            if (PlayerCombat.Instance != null) PlayerCombat.Instance.gameObject.GetComponent<SpriteRenderer>().SetAlpha(0f);
            _dullFlashSprite.DOFade(0f, 2f).SetEase(Ease.OutQuad);
            _flashSprite.DOFade(0f, 2f).SetEase(Ease.OutQuad);
        }

        private void GoToRite()
        {
            if (CharacterManager.CurrentRegion().GetRegionType() == RegionType.Rite) return;
            Region r = new Region();
            r.SetRegionType(RegionType.Rite);
            Rite.SetBrand(_brand, _player.TravelAction.GetCurrentRegion());
            _player.TravelAction.SetCurrentRegion(r);
            SceneChanger.GoToCombatScene(_player);
            CombatManager.Instance().LeaveCombat();
            CombatManager.Instance().RestoreEnemies();
        }

        private void Return()
        {
            CombatManager.Instance().ExitCombat(false);
            if (_goToNextRegion)
            {
                WorldState.TravelToNextEnvironment();
                return;
            }

            _player.TravelAction.SetCurrentRegion(Rite.GetLastRegion());
            SceneChanger.GoToCombatScene(_player);
            CombatManager.Instance().LeaveCombat();
        }

        public static void GenerateTutorialStarter()
        {
            if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Rite Starter");
            GameObject riteObject = Instantiate(_prefab);
            riteObject.transform.position = Vector2.zero;
            RiteStarter riteStarter = riteObject.GetComponent<RiteStarter>();
            riteStarter._isTutorial = true;
        }

        public float InRange()
        {
            return _inRange ? 1 : -1;
        }

        public string GetEventText()
        {
            return "Use portal [" + InputHandler.GetBindingForKey(InputAxis.TakeItem) + "]";
        }

        public void Activate()
        {
            _player = PlayerCombat.Instance.Player;
            Sequence sequence = DOTween.Sequence();
            sequence.AppendCallback(Flash);
            sequence.AppendInterval(1f);
            sequence.AppendCallback(Teleport);
            Destroy(GetComponent<Collider2D>());
            _inRange = false;
        }

        private void Teleport()
        {
            if (_brand == null)
            {
                if (_isTutorial)
                {
                    CombatManager.Instance().ExitCombat(false);
                    StoryController.Show();
                    TutorialManager.FinishIntroTutorial();
                    UiGearMenuController.SetOpenAllowed(true);
                }
                else Return();
            }
            else
                GoToRite();
        }
    }
}