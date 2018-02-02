using Game.Characters;
using Game.Characters.Player;
using Game.Combat.Enemies;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Combat.Skills
{
    
    
    public abstract class Skill : Cooldown
    {
        public int RageCost;
        private readonly string _name;
        private readonly bool _instant;
        protected Shot Shot;
        protected float RechargeTime;

        //todo read cost and cooldown from file
        
        protected Skill(bool instant, string name) : base(CombatManager.CombatCooldowns)
        {
            _name = name;
            _instant = instant;
            Duration = 2;
        }

        protected Player Player()
        {
            return CombatManager.Player;
        }
        
        public void Activate()
        {
            if (Running()) return;
            if (!CombatManager.Player.RageController.Spend(RageCost)) return;
            if (!_instant) return;
            OnFire();
            Deactivate();
        }

        public void ResetTimer()
        {
            Start();
        }

        private void Deactivate()
        {
            Player().UpdateMagazineUi();
            Start();
        }

        protected virtual void OnFire()
        {
            Deactivate();
            Shot = new Shot(CombatManager.GetCurrentTarget(), Player());
        }

        public override void SetController(CooldownController controller)
        {
            base.SetController(controller);
            controller.Text(_name);
        }
    }
}