using Game.Combat.Enemies.Nightmares;
using Game.Combat.Generation;
using SamsHelper.ReactiveUI.Elements;

namespace Game.Combat.Enemies.Animals
{
    public class Flit : AnimalBehaviour
    {
        private bool _discovered;
        
        public override void Initialise(Enemy e)
        {
            base.Initialise(e);
            Sprite.color = UiAppearanceController.InvisibleColour;
        }

        protected override void OnAlert()
        {
            base.OnAlert();
            _discovered = true;
            Flee(PathingGrid.GetCellOutOfRange());
        }

        public override void Update()
        {
            if (!_discovered) return;
            base.Update();
        }

        public bool Discovered()
        {
            return _discovered;
        }
    }
}