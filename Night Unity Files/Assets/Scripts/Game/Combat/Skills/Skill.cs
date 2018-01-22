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
        public readonly string Name;
        private readonly bool _instant;
        protected readonly Player Player;
        protected Shot shot;

        private Skill(Player player, bool instant, string name) : base(CombatManager.CombatCooldowns)
        {
            Name = name;
            _instant = instant;
            Player = player;
            Duration = 2;
        }

        public void Activate()
        {
            if (Running() || Player.Weapon().Empty()) return;
            if (!Player.RageController.Spend(RageCostInitial)) return;
            if (!_instant) return;
            OnFire();
            Deactivate();
        }

        private void Deactivate()
        {
            Player.UpdateMagazineUi();
            Start();
        }

        protected virtual void OnFire()
        {
            Deactivate();
            shot = new Shot(CombatManager.GetCurrentTarget(), Player);
        }

        public override void SetController(CooldownController controller)
        {
            base.SetController(controller);
            controller.Text(Name);
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
                shot.SetPierceDepth(100);
                shot.Fire();
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
                shot.UseRemainingShots();
                shot.Fire();
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
                shot.GuaranteeHit();
                shot.SetKnockdownChance(1, 10);
                shot.Fire();
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
                shot.AddOnHit(() =>
                {
                    Explosion.CreateAndDetonate(shot.Target().Position.CurrentValue(), 25, shot.DamageDealt());
                });
                shot.Fire();
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
                Player.Weapon().Reload(Player.Inventory());
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
                Player.OnFireAction = s =>
                {
                    s.SetKnockdownChance(0.25f, 2);
                };
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
                Player.OnFireAction = s =>
                {
                    s.SetNumberOfShots(2);
                };
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
                Player.OnFireAction = s =>
                {
                    s.AddOnHit(() =>
                    {
                        Explosion.CreateAndDetonate(s.Target().Position.CurrentValue(), 5, s.DamageDealt());
                    });
                };
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
                Player.OnFireAction = s =>
                {
                    Player.RageController.Increase(100);
                    Player.RageController.TryStart();
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
                Player.OnFireAction = s =>
                {
                    Player.Retaliate = true;
                };
            }
        }
    }
}