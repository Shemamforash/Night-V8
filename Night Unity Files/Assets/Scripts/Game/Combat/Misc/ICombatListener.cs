namespace Game.Combat
{
    public interface ICombatListener
    {
        void EnterCombat();
        void ExitCombat();
        void UpdateCombat();
    }
}