using System.Collections.Generic;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Characters
{
    public class BrandManager
    {
        private readonly List<Brand> _lockedBrands = new List<Brand>();
        private Player _player;
        private int _hoursAtHighWeight, _hoursAtLowWeight;
        private int _hoursIdling, _hoursTravelling;
        private readonly List<Brand> _readyBrands = new List<Brand>();
        private readonly List<Brand> _unlockedBrands = new List<Brand>();

        public void Initialise(Player player)
        {
            _player = player;
            CreateBrands();
        }
        
        private void CreateBrands()
        {
            CreateAttributeBrands();
            CreateSkillBrands();
            CreateConditionBrands();
            CreateResourceFindBrands();
            CreateOtherBrands();
        }

        private void CreateOtherBrands()
        {
            _lockedBrands.Add(new EssenceChangeBrand(_player, "Tinkerer", "Clumsy", 250));
            _lockedBrands.Add(new EssenceChangeBrand(_player, "Machinist", "Clumsy", 500));
            _lockedBrands.Add(new HealthRecoveryBrand(_player, "Reviving", "Clumsy", 10000));
            _lockedBrands.Add(new HealthRecoveryBrand(_player, "Rallying", "Clumsy", 20000));
            _lockedBrands.Add(new WillpowerRecoveryBrand(_player, "Murderous", "Clumsy", 100));
            _lockedBrands.Add(new WillpowerRecoveryBrand(_player, "Pyschopathic", "Clumsy", 200));
            _lockedBrands.Add(new ReloadBrand(_player, "Direct", "Clumsy", 25));
            _lockedBrands.Add(new ReloadBrand(_player, "Blunt", "Clumsy", 50));
            _lockedBrands.Add(new OnlySkillBrand(_player, "Direct", "Clumsy", 25));
            _lockedBrands.Add(new OnlySkillBrand(_player, "Blunt", "Clumsy", 50));
        }

        private void CreateResourceFindBrands()
        {
            _lockedBrands.Add(new ResourceBrand(_player, "Scavenger", "", 100));
            _lockedBrands.Add(new FoodBrand(_player, "Gatherer", "", 100));
            _lockedBrands.Add(new WaterBrand(_player, "Diviner", "", 100));
        }

        private void CreateConditionBrands()
        {
            _lockedBrands.Add(new IgniteBrand(_player, "Firestarter", "Flammable", 500));
            _lockedBrands.Add(new IgniteBrand(_player, "Decayer", "Decayed", 500));
            _lockedBrands.Add(new IgniteBrand(_player, "Plaguebearer", "Sickly", 500));
        }

        private void CreateSkillBrands()
        {
            foreach (WeaponType weaponType in WeaponGenerator.WeaponTypes)
            {
                _lockedBrands.Add(new WeaponSkillBrand(weaponType, _player, "Trained", "", 50));
                _lockedBrands.Add(new WeaponSkillBrand(weaponType, _player, "Gifted", "", 100));
            }

            _lockedBrands.Add(new CharacterSkillBrand(_player, "Enlighted", "", 7));
            _lockedBrands.Add(new CharacterSkillBrand(_player, "Clarified", "", 7));
        }

        public void IncreaseDamageDealt(int damage)
        {
            _lockedBrands.ForEach(b =>
            {
                if (!(b is StrengthBrand)) return;
                b.UpdateValue(damage);
            });
        }

        public void IncreaseItemsFound(int count)
        {
            _lockedBrands.ForEach(b =>
            {
                if (!(b is PerceptionBrand)) return;
                b.UpdateValue(count);
            });
        }

        public void IncreaseSkillsUsed(int count)
        {
            _lockedBrands.ForEach(b =>
            {
                if (!(b is WillpowerBrand)) return;
                b.UpdateValue(count);
            });
        }

        public void IncreaseRegionsExplored()
        {
            _lockedBrands.ForEach(b =>
            {
                if (!(b is EnduranceBrand)) return;
                b.UpdateValue(1);
            });
        }

        public void IncreaseEssenceInfused()
        {
            _lockedBrands.ForEach(b =>
            {
                if (!(b is EssenceChangeBrand)) return;
                b.UpdateValue(1);
            });
        }

        public void IncreaseDamageTaken(int damage)
        {
            _lockedBrands.ForEach(b =>
            {
                if (!(b is HealthRecoveryBrand)) return;
                b.UpdateValue(damage);
            });
        }

        public void IncreaseHumansKilled(int count)
        {
            _lockedBrands.ForEach(b =>
            {
                if (!(b is WillpowerRecoveryBrand)) return;
                b.UpdateValue(count);
            });
        }

        public void IncreaseBattlesNoSkills()
        {
            _lockedBrands.ForEach(b =>
            {
                if (!(b is ReloadBrand)) return;
                b.UpdateValue(1);
            });
        }

        public void IncreaseWeaponKills(WeaponType weaponType)
        {
            _lockedBrands.ForEach(b =>
            {
                if (!(b is WeaponSkillBrand)) return;
                if (((WeaponSkillBrand) b).WeaponType != weaponType) return;
                b.UpdateValue(1);
            });
        }

        public void IncreaseTimeSurvived()
        {
            _lockedBrands.ForEach(b =>
            {
                if (!(b is CharacterSkillBrand)) return;
                b.UpdateValue(1);
            });
        }

        public void IncreaseResourceFound()
        {
            _lockedBrands.ForEach(b =>
            {
                if (!(b is ResourceBrand)) return;
                b.UpdateValue(1);
            });
        }

        public void IncreaseFoodFound()
        {
            _lockedBrands.ForEach(b =>
            {
                if (!(b is FoodBrand)) return;
                b.UpdateValue(1);
            });
        }

        public void IncreaseWaterFound()
        {
            _lockedBrands.ForEach(b =>
            {
                if (!(b is WaterBrand)) return;
                b.UpdateValue(1);
            });
        }

        public void IncreaseOnlySkillBattles()
        {
            _lockedBrands.ForEach(b =>
            {
                if (!(b is OnlySkillBrand)) return;
                b.UpdateValue(1);
            });
        }

        public void IncreaseBurnCount()
        {
            _lockedBrands.ForEach(b =>
            {
                if (!(b is IgniteBrand)) return;
                b.UpdateValue(1);
            });
        }

        public void IncreaseDecayCount()
        {
            _lockedBrands.ForEach(b =>
            {
                if (!(b is DecayBrand)) return;
                b.UpdateValue(1);
            });
        }

        public void IncreaseSickenCount()
        {
            _lockedBrands.ForEach(b =>
            {
                if (!(b is SicknessBrand)) return;
                b.UpdateValue(1);
            });
        }

        private void CreateAttributeBrands()
        {
            _lockedBrands.Add(new StrengthBrand(_player, "Powerful", "Weak", 10000));
            _lockedBrands.Add(new StrengthBrand(_player, "Powerful", "Weak", 20000));
            _lockedBrands.Add(new StrengthBrand(_player, "Powerful", "Weak", 30000));
            _lockedBrands.Add(new StrengthBrand(_player, "Powerful", "Weak", 40000));
            _lockedBrands.Add(new StrengthBrand(_player, "Powerful", "Weak", 50000));

            _lockedBrands.Add(new PerceptionBrand(_player, "Powerful", "Weak", 20));
            _lockedBrands.Add(new PerceptionBrand(_player, "Powerful", "Weak", 40));
            _lockedBrands.Add(new PerceptionBrand(_player, "Powerful", "Weak", 60));
            _lockedBrands.Add(new PerceptionBrand(_player, "Powerful", "Weak", 80));
            _lockedBrands.Add(new PerceptionBrand(_player, "Powerful", "Weak", 100));

            _lockedBrands.Add(new WillpowerBrand(_player, "Powerful", "Weak", 10));
            _lockedBrands.Add(new WillpowerBrand(_player, "Powerful", "Weak", 200));
            _lockedBrands.Add(new WillpowerBrand(_player, "Powerful", "Weak", 300));
            _lockedBrands.Add(new WillpowerBrand(_player, "Powerful", "Weak", 400));
            _lockedBrands.Add(new WillpowerBrand(_player, "Powerful", "Weak", 500));

            _lockedBrands.Add(new EnduranceBrand(_player, "Powerful", "Weak", 10));
            _lockedBrands.Add(new EnduranceBrand(_player, "Powerful", "Weak", 20));
            _lockedBrands.Add(new EnduranceBrand(_player, "Powerful", "Weak", 30));
            _lockedBrands.Add(new EnduranceBrand(_player, "Powerful", "Weak", 40));
            _lockedBrands.Add(new EnduranceBrand(_player, "Powerful", "Weak", 50));
        }

        public abstract class Brand
        {
            private readonly int _counterTarget;
            private readonly string _successName, _failName;
            protected readonly Player Player;
            private int _counter;
            private bool _succeeded;
            public BrandStatus Status = BrandStatus.Locked;

            protected Brand(Player player, string successName, string failName, int counterTarget)
            {
                Player = player;
                _successName = successName;
                _failName = failName;
                _counterTarget = counterTarget;
                ResetCounter();
            }

            protected void ResetCounter()
            {
                _counter = 0;
                SetStatus(BrandStatus.Locked);
            }

            private void SetStatus(BrandStatus status)
            {
                Status = status;
                Player.BrandManager.UpdateBrandStatus(this);
            }

            public string GetName() => _succeeded ? _successName : _failName;

            public void UpdateValue(int amount)
            {
                if (Status != BrandStatus.Locked) return;
                _counter += amount;
                if (_counter >= _counterTarget) SetStatus(BrandStatus.Pending);
            }


            public void Succeed()
            {
                Debug.Log("unlocked " + _successName);
                _succeeded = true;
                SetStatus(BrandStatus.Succeeded);
                OnSucceed();
            }

            public void Fail()
            {
                SetStatus(BrandStatus.Failed);
                OnFail();
            }

            protected abstract void OnSucceed();
            protected abstract void OnFail();
        }

        private void UpdateBrandStatus(Brand brand)
        {
            switch (brand.Status)
            {
                case BrandStatus.Locked:
                    _readyBrands.Remove(brand);
                    _unlockedBrands.Remove(brand);
                    _lockedBrands.Add(brand);
                    break;
                case BrandStatus.Pending:
                    _readyBrands.Add(brand);
                    _unlockedBrands.Remove(brand);
                    _lockedBrands.Remove(brand);
                    break;
                default:
                    _readyBrands.Remove(brand);
                    _unlockedBrands.Add(brand);
                    _lockedBrands.Remove(brand);
                    break;
            }
        }

        public enum BrandStatus
        {
            Locked,
            Pending,
            Succeeded,
            Failed
        }

        private class StrengthBrand : Brand
        {
            public StrengthBrand(Player player, string successName, string failName, int counterTarget) : base(player, successName, failName, counterTarget)
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.ChangeStrengthMax(1);
            }

            protected override void OnFail()
            {
                Player.Attributes.ChangeStrengthMax(1);
            }
        }

        private class EnduranceBrand : Brand
        {
            public EnduranceBrand(Player player, string successName, string failName, int counterTarget) : base(player, successName, failName, counterTarget)
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.ChangeEnduranceMax(1);
            }

            protected override void OnFail()
            {
                Player.Attributes.ChangeEnduranceMax(1);
            }
        }

        private class PerceptionBrand : Brand
        {
            public PerceptionBrand(Player player, string successName, string failName, int counterTarget) : base(player, successName, failName, counterTarget)
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.ChangePerceptionMax(1);
            }

            protected override void OnFail()
            {
                Player.Attributes.ChangePerceptionMax(1);
            }
        }

        private class WillpowerBrand : Brand
        {
            public WillpowerBrand(Player player, string successName, string failName, int counterTarget) : base(player, successName, failName, counterTarget)
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.ChangeWillpowerMax(1);
            }

            protected override void OnFail()
            {
                Player.Attributes.ChangeWillpowerMax(1);
            }
        }

        private class EssenceChangeBrand : Brand
        {
            public EssenceChangeBrand(Player player, string successName, string failName, int counterTarget) : base(player, successName, failName, counterTarget)
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.Get(AttributeType.EssenceRecoveryBonus).Increment();
            }

            protected override void OnFail()
            {
                Player.Attributes.Get(AttributeType.EssenceRecoveryBonus).Increment(0.25f);
            }
        }

        private class HealthRecoveryBrand : Brand
        {
            public HealthRecoveryBrand(Player player, string successName, string failName, int counterTarget) : base(player, successName, failName, counterTarget)
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.Get(AttributeType.HealthRecoveryBonus).Increment(0.25f);
            }

            protected override void OnFail()
            {
                Player.Attributes.Get(AttributeType.HealthLossBonus).Increment(0.1f);
            }
        }

        private class WillpowerRecoveryBrand : Brand
        {
            public WillpowerRecoveryBrand(Player player, string successName, string failName, int counterTarget) : base(player, successName, failName, counterTarget)
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.Get(AttributeType.WillpowerLossBonus).Increment();
            }

            protected override void OnFail()
            {
                Player.Attributes.Get(AttributeType.MentalBreakBonus).Increment(0.05f);
            }
        }

        private class OnlySkillBrand : Brand
        {
            public OnlySkillBrand(Player player, string successName, string failName, int counterTarget) : base(player, successName, failName, counterTarget)
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.Get(AttributeType.FreeSkillChance).Increment(0.1f);
            }

            protected override void OnFail()
            {
                Player.Attributes.Get(AttributeType.InactiveSkillChance).Increment(0.05f);
            }
        }


        private class ReloadBrand : Brand
        {
            public ReloadBrand(Player player, string successName, string failName, int counterTarget) : base(player, successName, failName, counterTarget)
            {
            }

            protected override void OnSucceed()
            {
                if (Player.Attributes.ReloadOnLastRound)
                {
                    Player.Attributes.ReloadOnEmptyMag = true;
                }
                else if (Player.Attributes.ReloadOnEmptyMag)
                {
                    Player.Attributes.ReloadOnLastRound = true;
                }
                else if (Random.Range(0, 2) == 0)
                {
                    Player.Attributes.ReloadOnEmptyMag = true;
                }
                else
                {
                    Player.Attributes.ReloadOnLastRound = true;
                }
            }

            protected override void OnFail()
            {
                Player.Attributes.Get(AttributeType.ReloadFailChance).Increment(0.05f);
            }
        }

        private class WeaponSkillBrand : Brand
        {
            public readonly WeaponType WeaponType;

            public WeaponSkillBrand(WeaponType weaponType, Player player, string successName, string failName, int counterTarget) : base(player, successName, failName, counterTarget)
            {
                WeaponType = weaponType;
            }

            protected override void OnSucceed()
            {
                if (Player.Attributes.WeaponSkillOneUnlocks[WeaponType])
                {
                    Player.Attributes.WeaponSkillTwoUnlocks[WeaponType] = true;
                    return;
                }

                Player.Attributes.WeaponSkillOneUnlocks[WeaponType] = true;
            }

            protected override void OnFail()
            {
                ResetCounter();
            }
        }

        private class CharacterSkillBrand : Brand
        {
            public CharacterSkillBrand(Player player, string successName, string failName, int counterTarget) : base(player, successName, failName, counterTarget)
            {
            }

            protected override void OnSucceed()
            {
                if (Player.Attributes.SkillOneUnlocked)
                {
                    Player.Attributes.SkillTwoUnlocked = true;
                    return;
                }

                Player.Attributes.SkillOneUnlocked = true;
            }

            protected override void OnFail()
            {
                ResetCounter();
            }
        }

        private class IgniteBrand : Brand
        {
            public IgniteBrand(Player player, string successName, string failName, int counterTarget) : base(player, successName, failName, counterTarget)
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.LeaveFireTrail = true;
            }

            protected override void OnFail()
            {
                Player.Attributes.BurnWeakness = true;
            }
        }

        private class DecayBrand : Brand
        {
            public DecayBrand(Player player, string successName, string failName, int counterTarget) : base(player, successName, failName, counterTarget)
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.DecayRetaliate = true;
            }

            protected override void OnFail()
            {
                Player.Attributes.DecayWeakness = true;
            }
        }

        private class SicknessBrand : Brand
        {
            public SicknessBrand(Player player, string successName, string failName, int counterTarget) : base(player, successName, failName, counterTarget)
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.SpreadSickness = true;
            }

            protected override void OnFail()
            {
                Player.Attributes.SicknessWeakness = true;
            }
        }

        private class ResourceBrand : Brand
        {
            public ResourceBrand(Player player, string successName, string failName, int counterTarget) : base(player, successName, failName, counterTarget)
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.Get(AttributeType.ResourceFindBonus).Increment();
            }

            protected override void OnFail()
            {
                Player.Attributes.Get(AttributeType.ResourceFindBonus).Decrement();
            }
        }

        private class FoodBrand : Brand
        {
            public FoodBrand(Player player, string successName, string failName, int counterTarget) : base(player, successName, failName, counterTarget)
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.Get(AttributeType.HungerBonus).Increment();
            }

            protected override void OnFail()
            {
                Player.Attributes.Get(AttributeType.StarvingWaterBonus).Decrement();
            }
        }

        private class WaterBrand : Brand
        {
            public WaterBrand(Player player, string successName, string failName, int counterTarget) : base(player, successName, failName, counterTarget)
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.Get(AttributeType.ThirstBonus).Increment();
            }

            protected override void OnFail()
            {
                Player.Attributes.Get(AttributeType.DehydratingFoodBonus).Decrement();
            }
        }

        public Brand NextBrand()
        {
            return _readyBrands.Count == 0 ? null : _readyBrands[0];
        }
    }
}