using Game.Combat.Enemies;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class Loot : ContainerController
    {
        public readonly bool IsValid;

        public Loot(Vector2 position, Enemy enemy) : base(position, enemy.Name)
        {
            IsValid = Random.Range(0, 10) != 0 || enemy.Weapon != null;
            Inventory.Move(enemy.Weapon, 1);
            Inventory.SetReadonly(true);
            PrefabLocation = "Container";
        }
    }
}