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
        public int RageCostInitial, RageCostOverTime;
        private readonly string _name;
        private readonly bool _instant;
        private readonly Player _player;
        private Shot Shot;
        protected float RechargeTime;

        private Skill(Player player, bool instant, string name) : base(CombatManager.CombatCooldowns)
        {
            _name = name;
            _instant = instant;
            _player = player;
            Duration = 2;
        }

        public void Activate()
        {
            if (Running() || _player.Weapon().Empty()) return;
            if (!_player.RageController.Spend(RageCostInitial)) return;
            if (!_instant) return;
            OnFire();
            Deactivate();
        }

        private void Deactivate()
        {
            _player.UpdateMagazineUi();
            Start();
        }

        protected virtual void OnFire()
        {
            Deactivate();
            Shot = new Shot(CombatManager.GetCurrentTarget(), _player);
        }

        public override void SetController(CooldownController controller)
        {
            base.SetController(controller);
            controller.Text(_name);
        }

        public class PiercingShot : Skill
        {
            public PiercingShot(Player player) : base(player, true, nameof(PiercingShot))
            {
                Duration = 2f;
            }

            protected override void OnFire()
            {
                base.OnFire();
                Shot.SetPierceDepth(100);
                Shot.SetPierceChance(1);
                Shot.Fire();
            }
        }

        public class FullBlast : Skill
        {
            public FullBlast(Player player) : base(player, true, nameof(FullBlast))
            {
            }

            protected override void OnFire()
            {
                base.OnFire();
                Shot.UseRemainingShots();
                Shot.Fire();
            }
        }

        public class LegSweep : Skill
        {
            public LegSweep(Player player) : base(player, true, nameof(LegSweep))
            {
            }

            protected override void OnFire()
            {
                base.OnFire();
                Shot.GuaranteeHit();
                Shot.Fire();
                Shot knockdownShot = new Shot(Shot.Target(), Shot.Origin());
                knockdownShot.SetKnockdownChance(1, 5);
                knockdownShot.Fire();
            }
        }

        public class BulletCloud : Skill
        {
            public BulletCloud(Player player) : base(player, true, nameof(BulletCloud))
            {
            }

            protected override void OnFire()
            {
                base.OnFire();
                foreach (Enemy e in UIEnemyController.Enemies)
                {
                    Shot s = new Shot(e, Shot.Origin());
                    s.Fire();
                }
            }
        }

        public class TopUp : Skill
        {
            public TopUp(Player player) : base(player, true, nameof(TopUp))
            {
            }

            protected override void OnFire()
            {
                base.OnFire();
                _player.Weapon().Reload(_player.Inventory());
            }
        }

        public class HeavyLead : Skill
        {
            public HeavyLead(Player player) : base(player, true, nameof(HeavyLead))
            {
            }

            protected override void OnFire()
            {
                base.OnFire();
                _player.OnFireAction = s => { s.SetKnockdownChance(0.25f, 2); };
            }
        }

        public class DoubleUp : Skill
        {
            public DoubleUp(Player player) : base(player, true, nameof(DoubleUp))
            {
            }

            protected override void OnFire()
            {
                base.OnFire();
                _player.OnFireAction = s => { s.SetNumberOfShots(2); };
            }
        }

        public class Splinter : Skill
        {
            public Splinter(Player player) : base(player, true, nameof(Splinter))
            {
            }

            protected override void OnFire()
            {
                base.OnFire();
                _player.OnFireAction = s => { s.AddOnHit(() => { Explosion.CreateAndDetonate(s.Target().Position.CurrentValue(), 5, s.DamageDealt()); }); };
            }
        }

        public class Retribution : Skill
        {
            public Retribution(Player player) : base(player, true, nameof(Retribution))
            {
            }

            protected override void OnFire()
            {
                base.OnFire();
                _player.OnFireAction = s =>
                {
                    _player.RageController.Increase(100);
                    _player.RageController.TryStart();
                };
            }
        }

        public class Revenge : Skill
        {
            public Revenge(Player player) : base(player, true, nameof(Revenge))
            {
            }

            protected override void OnFire()
            {
                base.OnFire();
                _player.Retaliate = true;
            }
        }
    }
}