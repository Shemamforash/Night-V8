using System.Collections.Generic;
using Game.Characters;
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
        private bool _returnHome;

        public static void Generate(Brand brand, bool returnHome)
        {
            Vector2 position = brand == null ? Vector2.zero : GetPosition();
            if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Rite Starter");
            GameObject riteObject = Instantiate(_prefab);
            RiteStarter riteStarter = riteObject.GetComponent<RiteStarter>();
            riteStarter.Initialise(brand, returnHome);
            riteObject.transform.position = position;
        }

        private void Initialise(Brand brand, bool returnHome)
        {
            _brand = brand;
            _returnHome = returnHome;
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
            if (_returnHome)
                ReturnHome();
            else
                GoToRite();
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

        private void ReturnHome()
        {
            CharacterManager.SelectedCharacter.TravelAction.ReturnToHomeInstant();
            SceneChanger.GoToGameScene();
        }
    }
}