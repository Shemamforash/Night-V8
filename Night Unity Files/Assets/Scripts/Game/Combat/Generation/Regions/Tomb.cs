using Game.Combat.Enemies.Bosses;
using Game.Exploration.Environment;

namespace Game.Combat.Generation
{
    public class Tomb : RegionGenerator //not a mine
    {
        protected override void Generate()
        {
            switch (EnvironmentManager.CurrentEnvironment.LevelNo)
            {
                case 0:
                    SerpentBehaviour.Create();
                    break;
                case 1:
                    StarfishBehaviour.Create();
                    break;
                case 2:
                    SwarmBehaviour.Create();
                    break;
                case 3:
                    OvaBehaviour.Create();
                    break;
                case 4:
                    break;
            }
        }
    }
}