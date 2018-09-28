using System;
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

        public static void Generate(Brand brand)
        {
            Vector2 position = GetPosition();
            
            if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Rite Starter");
            GameObject riteObject = Instantiate(_prefab);
            RiteStarter riteStarter = riteObject.GetComponent<RiteStarter>();
            riteStarter._brand = brand;
            riteObject.transform.position = position;
        }

        private static Vector2 GetPosition()
        {
            List<Vector2> points = AdvancedMaths.GetPoissonDiscDistribution(50, 1, 2, 5);
            foreach (Vector2 p in points)
            {
                if (p.magnitude > PathingGrid.CombatMovementDistance - 1) continue;
                Vector2 newPoint = p + (Vector2) PlayerCombat.Instance.transform.position;
                Vector2 topLeft = new Vector2(newPoint.x - 0.25f, newPoint.y + 0.25f);
                Vector2 bottomRight = new Vector2(newPoint.x + 0.25f, newPoint.y - 0.25f);
                if (!PathingGrid.IsSpaceAvailable(topLeft, bottomRight)) continue;
                return newPoint;
            }
            throw new Exception("No valid position found");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            Region r = new Region();
            r.SetRegionType(RegionType.Rite);
            Rite.SetBrand(_brand);
            CombatManager.SetCurrentRegion(r);
            SceneChanger.GoToCombatScene();
        }
    }
}