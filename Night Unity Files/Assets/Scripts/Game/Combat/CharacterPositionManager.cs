using Game.Characters.Player;
using Game.Combat.Enemies;
using SamsHelper;

namespace Game.Combat
{
    public static class CharacterPositionManager
    {
        public static void UpdatePlayerDirection()
        {
            Player player = CombatManager.Player;
            Enemy target = CombatManager.CurrentTarget;
            if (target == null) return;
            player.FacingDirection = target.DistanceToPlayer > 0 ? Direction.Right : Direction.Left; 
            UIEnemyController.Enemies.ForEach(e =>
            {
                if (player.FacingDirection == Direction.Left && e.DistanceToPlayer > 0)
                {
                    e.EnemyView.Hide();
                    return;
                }

                if (player.FacingDirection == Direction.Right && e.DistanceToPlayer < 0)
                {
                    e.EnemyView.Hide();
                    return;
                }

                e.EnemyView.Show();
            });
        }
    }
}