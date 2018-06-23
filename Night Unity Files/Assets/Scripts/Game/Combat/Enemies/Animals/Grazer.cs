using Game.Combat.Enemies.Nightmares;

namespace Game.Combat.Enemies.Animals
{
    public class Grazer : AnimalBehaviour
    {
        protected override void OnAlert()
        {
            Flee();
        }

//        private void OnDrawGizmos()
//        {
//            Gizmos.color = new Color(0,1,0, 0.4f);
//            PathingGrid._outOfRangeList.ForEach(c =>
//            {
//                Gizmos.DrawCube(c.Position, Vector3.one / PathingGrid.CellResolution);
//            });
//            Gizmos.color = new Color(0, 1, 0, 0.4f);
//        }
    }
}