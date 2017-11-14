using Game.Characters;
using Game.Combat.Enemies;
using SamsHelper;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat
{
//    public class EnemyPlayerRelation
//    {
//        public readonly Enemy Enemy;
//        public readonly Player Player;
//        public readonly MyValue EnemyCover = new MyValue(0, 0, 100);
//        public readonly MyValue PlayerCover = new MyValue(0, 0, 100);
//        private ValueTextLink<string> PlayerSightLink = new ValueTextLink<string>();
//        private ValueTextLink<string> EnemySightLink = new ValueTextLink<string>();

//        public EnemyPlayerRelation(Enemy enemy, Player player)
//        {
//            Enemy = enemy;
//            Player = player;
//            EnemySightLink.AddTextObject(enemy.EnemyView().VisionText);
//            EnemyCover.AddThreshold(100, "Obscured");
//            EnemyCover.AddThreshold(80, "Barely Visible");
//            EnemyCover.AddThreshold(50, "Partial Sight");
//            EnemyCover.AddThreshold(20, "Clear shot");
//            EnemyCover.AddThreshold(00, "Totally Exposed");
//            PlayerSightLink.AddTextObject(enemy.EnemyView().CoverText);
//            PlayerCover.AddThreshold(100, "Obscured");
//            PlayerCover.AddThreshold(80, "Barely Visible");
//            PlayerCover.AddThreshold(50, "Partial Sight");
//            PlayerCover.AddThreshold(20, "Clear shot");
//            PlayerCover.AddThreshold(0, "Totally Exposed");
//            PlayerCover.AddOnValueChange(a => UpdateEnemySight());
//            PlayerCover.AddOnValueChange(a => UpdatePlayerCover());
//        }


        
//    }
}