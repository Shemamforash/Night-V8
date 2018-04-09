namespace Game.Combat.Misc
{
    public interface ICombatListener
    {
        void EnterCombat();
        void ExitCombat();
        void UpdateCombat();
    }
}