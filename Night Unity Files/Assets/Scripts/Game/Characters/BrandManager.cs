using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Combat.Generation;
using Game.Combat.Generation.Shrines;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.Persistence;
using UnityEngine;

namespace Game.Characters
{
    public class BrandManager
    {
        private readonly List<Brand> _lockedBrands = new List<Brand>();
        private Player _player;
        private readonly List<Brand> _completedBrands = new List<Brand>();
        private readonly List<Brand> _activeBrands = new List<Brand>();

        public void Load(XmlNode doc)
        {
            XmlNode brandsNode = doc.SelectSingleNode("Brands");
            foreach (XmlNode brandNode in brandsNode.SelectNodes("Brand"))
            {
                string name = brandNode.StringFromNode("Name");
                bool found = false;
                foreach (Brand brand in _completedBrands)
                {
                    if (brand.GetName() != name) continue;
                    brand.Load(brandNode);
                    found = true;
                    break;
                }

                if (found) continue;
                foreach (Brand brand in _activeBrands)
                {
                    if (brand.GetName() != name) continue;
                    brand.Load(brandNode);
                    break;
                }
            }
        }

        public XmlNode Save(XmlNode doc)
        {
            doc = doc.CreateChild("Brands");
            _completedBrands.ForEach(b => b.Save(doc));
            _activeBrands.ForEach(b => b.Save(doc));
            return doc;
        }
        
        public List<Brand> GetBrandChoice()
        {
            List<Brand> brandSelection = new List<Brand>();
            if (_lockedBrands.Count == 0) return brandSelection;
            _lockedBrands.Shuffle();
            for (int i = 0; i < 3 && i < _lockedBrands.Count; ++i)
            {
                brandSelection.Add(_lockedBrands[i]);
            }

            return brandSelection;
        }

        public void SetActiveBrand(Brand brand)
        {
            Debug.Log("brand is active");
            _activeBrands.Add(brand);
            brand.SetStatus(BrandStatus.Active);
        }

        public void Initialise(Player player)
        {
            _player = player;
            CreateBrands();
        }

        private void CreateBrands()
        {
            CreateAttributeBrands();
            CreateConditionBrands();
            CreateResourceFindBrands();
            CreateOtherBrands();
        }

        private void CreateAttributeBrands()
        {
            new StrengthBrand(_player, 10000);
            new StrengthBrand(_player, 10000);
            new StrengthBrand(_player, 10000);
            new StrengthBrand(_player, 10000);
            new StrengthBrand(_player, 10000);

            new PerceptionBrand(_player, 20);
            new PerceptionBrand(_player, 20);
            new PerceptionBrand(_player, 20);
            new PerceptionBrand(_player, 20);
            new PerceptionBrand(_player, 20);

            new WillpowerBrand(_player, 10);
            new WillpowerBrand(_player, 10);
            new WillpowerBrand(_player, 10);
            new WillpowerBrand(_player, 10);
            new WillpowerBrand(_player, 10);

            new EnduranceBrand(_player, 10);
            new EnduranceBrand(_player, 10);
            new EnduranceBrand(_player, 10);
            new EnduranceBrand(_player, 10);
            new EnduranceBrand(_player, 10);
        }

        private void CreateOtherBrands()
        {
            new EssenceChangeBrand(_player, 10);
            new EssenceChangeBrand(_player, 10);
            new HealthRecoveryBrand(_player, 10000);
            new HealthRecoveryBrand(_player, 10000);
            new WillpowerRecoveryBrand(_player, 100);
            new WillpowerRecoveryBrand(_player, 100);
            new AutomaticReloadBrand(_player, 25);
            new InstantReloadBrand(_player, 25);
            new OnlySkillBrand(_player, 25);
            new SkillKillBrand(_player, 25);
        }

        private void CreateResourceFindBrands()
        {
            new ResourceBrand(_player, 100);
            new FoodBrand(_player, 100);
            new WaterBrand(_player, 100);
        }

        private void CreateConditionBrands()
        {
            new IgniteBrand(_player,  500);
            new DecayBrand(_player,  500);
            new SicknessBrand(_player, 500);
        }

        public void IncreaseDamageDealt(int damage)
        {
            for (int i = _activeBrands.Count - 1; i >= 0; --i)
            {
                Brand b = _activeBrands[i];
                if (!(b is StrengthBrand)) continue;
                b.UpdateValue(damage);
            }
        }

        public void IncreaseItemsFound(int count)
        {
            for (int i = _activeBrands.Count - 1; i >= 0; --i)
            {
                Brand b = _activeBrands[i];
                if (!(b is PerceptionBrand)) continue;
                b.UpdateValue(count);
            }
        }

        public void IncreaseSkillsUsed(int count)
        {
            for (int i = _activeBrands.Count - 1; i >= 0; --i)
            {
                Brand b = _activeBrands[i];
                if (!(b is WillpowerBrand)) continue;
                b.UpdateValue(count);
            }
        }

        public void IncreaseRegionsExplored()
        {
            for (int i = _activeBrands.Count - 1; i >= 0; --i)
            {
                Brand b = _activeBrands[i];
                if (!(b is EnduranceBrand)) continue;
                b.UpdateValue(1);
            }
        }

        public void IncreaseEssenceInfused()
        {
            for (int i = _activeBrands.Count - 1; i >= 0; --i)
            {
                Brand b = _activeBrands[i];
                if (!(b is EssenceChangeBrand)) continue;
                b.UpdateValue(1);
            }
        }

        public void IncreaseDamageTaken(int damage)
        {
            for (int i = _activeBrands.Count - 1; i >= 0; --i)
            {
                Brand b = _activeBrands[i];
                if (!(b is HealthRecoveryBrand)) continue;
                b.UpdateValue(damage);
            }
        }

        public void IncreaseHumansKilled(int count)
        {
            for (int i = _activeBrands.Count - 1; i >= 0; --i)
            {
                Brand b = _activeBrands[i];
                if (!(b is WillpowerRecoveryBrand)) continue;
                b.UpdateValue(count);
            }
        }

        public void IncreaseBattlesNoSkills()
        {
            for (int i = _activeBrands.Count - 1; i >= 0; --i)
            {
                Brand b = _activeBrands[i];
                if (!(b is AutomaticReloadBrand)) continue;
                b.UpdateValue(1);
            }
        }

        public void IncreaseResourceFound()
        {
            for (int i = _activeBrands.Count - 1; i >= 0; --i)
            {
                Brand b = _activeBrands[i];
                if (!(b is ResourceBrand)) continue;
                b.UpdateValue(1);
            }
        }

        public void IncreaseFoodFound()
        {
            for (int i = _activeBrands.Count - 1; i >= 0; --i)
            {
                Brand b = _activeBrands[i];
                if (!(b is FoodBrand)) continue;
                b.UpdateValue(1);
            }
        }

        public void IncreaseWaterFound()
        {
            for (int i = _activeBrands.Count - 1; i >= 0; --i)
            {
                Brand b = _activeBrands[i];
                if (!(b is WaterBrand)) continue;
                b.UpdateValue(1);
            }
        }

        public void IncreaseOnlySkillBattles()
        {
            for (int i = _activeBrands.Count - 1; i >= 0; --i)
            {
                Brand b = _activeBrands[i];
                if (!(b is OnlySkillBrand)) continue;
                b.UpdateValue(1);
            }
        }

        public void IncreaseBurnCount()
        {
            for (int i = _activeBrands.Count - 1; i >= 0; --i)
            {
                Brand b = _activeBrands[i];
                if (!(b is IgniteBrand)) continue;
                b.UpdateValue(1);
            }
        }

        public void IncreaseDecayCount()
        {
            for (int i = _activeBrands.Count - 1; i >= 0; --i)
            {
                Brand b = _activeBrands[i];
                if (!(b is DecayBrand)) continue;
                b.UpdateValue(1);
            }
        }

        public void IncreaseSickenCount()
        {
            for (int i = _activeBrands.Count - 1; i >= 0; --i)
            {
                Brand b = _activeBrands[i];
                if (!(b is SicknessBrand)) continue;
                b.UpdateValue(1);
            }
        }

        public abstract class Brand
        {
            private readonly int _counterTarget;
            private readonly string _riteName, _successName, _failName;
            protected readonly Player Player;
            private int _counter;
            public BrandStatus Status = BrandStatus.Locked;
            private static readonly List<BrandStatus> _brandStatuses = new List<BrandStatus>();

            protected Brand(Player player, string riteName, string successName, string failName, int counterTarget)
            {
                Player = player;
                _riteName = riteName;
                _successName = successName;
                _failName = failName;
                _counterTarget = counterTarget;
                SetStatus(BrandStatus.Locked);
            }

            protected string Progress()
            {
                return _counter + "/" + _counterTarget;
            }

            public void SetStatus(BrandStatus status)
            {
                Status = status;
                Player.BrandManager.UpdateBrandStatus(this);
            }

            public string GetName()
            {
                return "Rite of " + _riteName;
            }

            public void UpdateValue(int amount)
            {
                _counter += amount;
                if (_counter >= _counterTarget)
                {
                    RiteStarter.Generate(this);
                }
            }

            public void Succeed()
            {
                Debug.Log("unlocked " + _successName);
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

            public void PrintStatus()
            {
                Debug.Log(this + " " + Status + " " + _counter + "/" + _counterTarget);
            }

            public string GetProgressString()
            {
                return "Rite of " + _riteName + ": " + GetProgressSubstring();
            }

            protected abstract string GetProgressSubstring();

            private static BrandStatus StringToBrandStatus(string brandStatusString)
            {
                if (_brandStatuses.Count == 0)
                {
                    foreach (BrandStatus brandStatus in Enum.GetValues(typeof(BrandStatus)))
                        _brandStatuses.Add(brandStatus);
                }

                foreach (BrandStatus brandStatus in _brandStatuses)
                {
                    if (brandStatus.ToString() == brandStatusString)
                    {
                        return brandStatus;
                    }
                }
                throw new ArgumentOutOfRangeException();
            }
            
            public void Load(XmlNode doc)
            {
                _counter = doc.IntFromNode("Progress");
                Status = (BrandStatus)doc.IntFromNode("Status");
            }

            public XmlNode Save(XmlNode doc)
            {
                doc = doc.CreateChild("Brand");
                doc.CreateChild("Name", _riteName);
                doc.CreateChild("Progress", _counter);
                doc.CreateChild("Status", (int)Status);
                return doc;
            }
        }

        private void UpdateBrandStatus(Brand brand)
        {
            switch (brand.Status)
            {
                case BrandStatus.Locked:
                    if (_lockedBrands.Contains(brand)) return;
                    _lockedBrands.Add(brand);
                    _activeBrands.Remove(brand);
                    _completedBrands.Remove(brand);
                    break;
                case BrandStatus.Active:
                    if (!_activeBrands.Contains(brand)) return;
                    _activeBrands.Add(brand);
                    _completedBrands.Remove(brand);
                    _lockedBrands.Remove(brand);
                    break;
                default:
                    if (_completedBrands.Contains(brand)) return;
                    _completedBrands.Add(brand);
                    _activeBrands.Remove(brand);
                    _lockedBrands.Remove(brand);
                    break;
            }
        }

        public enum BrandStatus
        {
            Locked,
            Active,
            Succeeded,
            Failed
        }

        private class StrengthBrand : Brand
        {
            public StrengthBrand(Player player, int counterTarget) : base(player, "Power", "Powerful", "Weak", counterTarget)
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

            protected override string GetProgressSubstring()
            {
                return "Dealt " + Progress() + " damage";
            }
        }

        private class EnduranceBrand : Brand
        {
            public EnduranceBrand(Player player, int counterTarget) : base(player, "Stamina", "Nomadic", "Sloth", counterTarget)
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

            protected override string GetProgressSubstring()
            {
                return "Explored " + Progress() + " regions";
            }
        }

        private class PerceptionBrand : Brand
        {
            public PerceptionBrand(Player player, int counterTarget) : base(player, "Vigilance", "Keen", "Oblivious", counterTarget)
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

            protected override string GetProgressSubstring()
            {
                return "Found " + Progress() + " items";
            }
        }

        private class WillpowerBrand : Brand
        {
            public WillpowerBrand(Player player, int counterTarget) : base(player, "Resolution", "Hardy", "Fearful", counterTarget)
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

            protected override string GetProgressSubstring()
            {
                return "Used " + Progress() + " skills";
            }
        }

        private class EssenceChangeBrand : Brand
        {
            public EssenceChangeBrand(Player player, int counterTarget) : base(player, "Insight", "Learned", "Ignorant", counterTarget)
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

            protected override string GetProgressSubstring()
            {
                return "Infused " + Progress() + " essence";
            }
        }

        private class HealthRecoveryBrand : Brand
        {
            public HealthRecoveryBrand(Player player, int counterTarget) : base(player, "Revival", "Immortal", "Pathetic", counterTarget)
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

            protected override string GetProgressSubstring()
            {
                return "Taken " + Progress() + " damage";
            }
        }

        private class WillpowerRecoveryBrand : Brand
        {
            public WillpowerRecoveryBrand(Player player, int counterTarget) : base(player, "Apathy", "Murderous", "Timid", counterTarget)
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

            protected override string GetProgressSubstring()
            {
                return "Killed " + Progress() + " enemies";
            }
        }

        private class OnlySkillBrand : Brand
        {
            public OnlySkillBrand(Player player, int counterTarget) : base(player, "Mastery", "Masterful", "Distracted", counterTarget)
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

            protected override string GetProgressSubstring()
            {
                return "Used only skills in " + Progress() + " battles";
            }
        }

        private class SkillKillBrand : Brand
        {
            public SkillKillBrand(Player player, int counterTarget) : base(player, "Prowes", "Alert", "Absent", counterTarget)
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.Get(AttributeType.InstantSkillRechargeChance).Increment(0.1f);
            }

            protected override void OnFail()
            {
                Player.Attributes.Get(AttributeType.InactiveSkillChance).Increment(0.05f);
            }

            protected override string GetProgressSubstring()
            {
                return "Killed " + Progress() + " enemies with skills";
            }
        }

        private class InstantReloadBrand : Brand
        {
            public InstantReloadBrand(Player player, int counterTarget) : base(player, "Ingenuity", "Devious", "Inept", counterTarget)
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.ReloadOnEmptyMag = true;
            }

            protected override void OnFail()
            {
                Player.Attributes.Get(AttributeType.ReloadFailChance).Increment(0.05f);
            }

            protected override string GetProgressSubstring()
            {
                return "Killed " + Progress() + " enemies with last round";
            }
        }

        private class AutomaticReloadBrand : Brand
        {
            public AutomaticReloadBrand(Player player, int counterTarget) : base(player, "Finesse", "Perpetual", "Clumsy", counterTarget)
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.ReloadOnLastRound = true;
            }

            protected override void OnFail()
            {
                Player.Attributes.Get(AttributeType.ReloadFailChance).Increment(0.05f);
            }

            protected override string GetProgressSubstring()
            {
                return "Used only bullets in " + Progress() + " battles";
            }
        }

        private class IgniteBrand : Brand
        {
            public IgniteBrand(Player player, int counterTarget) : base(player, "The Inferno", "Infernal", "Combustible", counterTarget)
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

            protected override string GetProgressSubstring()
            {
                return "Ignited " + Progress() + " enemies";
            }
        }

        private class DecayBrand : Brand
        {
            public DecayBrand(Player player, int counterTarget) : base(player, "The Voidwalker", "Void Born", "Crumbling", counterTarget)
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

            protected override string GetProgressSubstring()
            {
                return "Decayed " + Progress() + " enemies";
            }
        }

        private class SicknessBrand : Brand
        {
            public SicknessBrand(Player player, int counterTarget) : base(player, "The Plaguebearer", "Diseased", "Sickly", counterTarget)
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

            protected override string GetProgressSubstring()
            {
                return "Sickened " + Progress() + " enemies";
            }
        }

        private class ResourceBrand : Brand
        {
            public ResourceBrand(Player player, int counterTarget) : base(player, "Scavenging", "Keen", "Blind", counterTarget)
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

            protected override string GetProgressSubstring()
            {
                return "Found " + Progress() + " resources";
            }
        }

        private class FoodBrand : Brand
        {
            public FoodBrand(Player player, int counterTarget) : base(player, "Gathering", "Sated", "Greedy", counterTarget)
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

            protected override string GetProgressSubstring()
            {
                return "Found " + Progress() + " food";
            }
        }

        private class WaterBrand : Brand
        {
            public WaterBrand(Player player, int counterTarget) : base(player, "Divining", "Slaked", "Parched", counterTarget)
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

            protected override string GetProgressSubstring()
            {
                return "Found " + Progress() + " water";
            }
        }

        public List<Brand> GetActiveBrands()
        {
            return _activeBrands;
        }
    }
}