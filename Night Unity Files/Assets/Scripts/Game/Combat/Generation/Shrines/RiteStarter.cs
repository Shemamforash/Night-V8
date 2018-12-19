using System.Collections.Generic;
using Game.Characters;
using Game.Characters.CharacterActions;
using Game.Combat.Player;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
    public class RiteStarter : MonoBehaviour
    {
        private Brand _brand;
        private static GameObject _prefab;
        private static RiteStarter _instance;

        public void Awake()
        {
            _instance = this;
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
            Vector2 position = brand == null ? Vector2.zero : GetPosition();
            if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Rite Starter");
            GameObject riteObject = Instantiate(_prefab);
            RiteStarter riteStarter = riteObject.GetComponent<RiteStarter>();
            riteStarter.Initialise(brand);
            riteObject.transform.position = position;
        }

        private void Initialise(Brand brand)
        {
            _brand = brand;
        }

        private static Vector2 GetPosition()
        {
            List<Vector2> points = AdvancedMaths.GetPoissonDiscDistribution(100, 5);
            foreach (Vector2 p in points)
            {
                if (p.magnitude > PathingGrid.CombatMovementDistance - 1) continue;
                Vector2 newPoint = p + (Vector2) PlayerCombat.Instance.transform.position;
                Vector2 topLeft = new Vector2(newPoint.x - 0.25f, newPoint.y + 0.25f);
                Vector2 bottomRight = new Vector2(newPoint.x + 0.25f, newPoint.y - 0.25f);
                if (!PathingGrid.IsSpaceAvailable(topLeft, bottomRight)) continue;
                return newPoint;
            }

            return PathingGrid.GetCellNearMe(PlayerCombat.Instance.CurrentCell(), 5, 1).Position;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (_brand == null)
                Return();
            else
                GoToRite();
            Destroy(this);
        }

        private void GoToRite()
        {
            if (CombatManager.GetCurrentRegion().GetRegionType() == RegionType.Rite) return;
            Region r = new Region();
            r.SetRegionType(RegionType.Rite);
            Rite.SetBrand(_brand, CharacterManager.SelectedCharacter.TravelAction.GetCurrentRegion());
            CombatManager.SetCurrentRegion(r);
            SceneChanger.GoToCombatScene();
            CombatManager.SetInCombat(false);
        }

        private void Return()
        {
            CombatManager.ExitCombat(false);
            Travel travel = CharacterManager.SelectedCharacter.TravelAction;
            travel.TravelToInstant(travel.GetCurrentRegion());
        }
    }
}