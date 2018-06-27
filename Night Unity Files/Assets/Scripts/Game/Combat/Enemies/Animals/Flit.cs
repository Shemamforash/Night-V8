using Game.Combat.Enemies.Nightmares;
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
            Flee();
        }

        public override void Update()
        {
            if (!_discovered) return;
            base.Update();
        }
    }
}