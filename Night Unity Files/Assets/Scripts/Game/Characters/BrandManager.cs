using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Combat.Generation.Shrines;
using SamsHelper.Libraries;
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

        public List<Brand> GetBrandChoice(int ritesRemaining)
        {
            List<Brand> brandSelection = new List<Brand>();
            if (_lockedBrands.Count == 0) return brandSelection;
            if (_activeBrands.Count(b => b is EnduranceBrand) != 0) brandSelection.RemoveAll(b => b is EnduranceBrand);
            if (_activeBrands.Count(b => b is PerceptionBrand) != 0) brandSelection.RemoveAll(b => b is PerceptionBrand);
            if (_activeBrands.Count(b => b is StrengthBrand) != 0) brandSelection.RemoveAll(b => b is StrengthBrand);
            if (_activeBrands.Count(b => b is WillpowerBrand) != 0) brandSelection.RemoveAll(b => b is WillpowerBrand);
            _lockedBrands.Shuffle();
            for (int i = 0; i < ritesRemaining && i < _lockedBrands.Count; ++i)
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
            XmlNode node = Helper.OpenRootNode("Brands");
            _lockedBrands.ForEach(b => b.ReadData(node));
            _completedBrands.ForEach(b => b.ReadData(node));
            _activeBrands.ForEach(b => b.ReadData(node));
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
            new StrengthBrand(_player);
            new StrengthBrand(_player);
            new StrengthBrand(_player);
            new StrengthBrand(_player);
            new StrengthBrand(_player);

            new PerceptionBrand(_player);
            new PerceptionBrand(_player);
            new PerceptionBrand(_player);
            new PerceptionBrand(_player);
            new PerceptionBrand(_player);

            new WillpowerBrand(_player);
            new WillpowerBrand(_player);
            new WillpowerBrand(_player);
            new WillpowerBrand(_player);
            new WillpowerBrand(_player);

            new EnduranceBrand(_player);
            new EnduranceBrand(_player);
            new EnduranceBrand(_player);
            new EnduranceBrand(_player);
            new EnduranceBrand(_player);
        }

        private void CreateOtherBrands()
        {
            new EssenceChangeBrand(_player);
            new EssenceChangeBrand(_player);
            new HealthRecoveryBrand(_player);
            new HealthRecoveryBrand(_player);
            new WillpowerRecoveryBrand(_player);
            new WillpowerRecoveryBrand(_player);
            new AutomaticReloadBrand(_player);
            new InstantReloadBrand(_player);
            new OnlySkillBrand(_player);
            new SkillKillBrand(_player);
        }

        private void CreateResourceFindBrands()
        {
            new ResourceBrand(_player);
            new FoodBrand(_player);
            new WaterBrand(_player);
        }

        private void CreateConditionBrands()
        {
            new IgniteBrand(_player);
            new DecayBrand(_player);
            new SicknessBrand(_player);
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
            private readonly string _riteName;
            protected readonly Player Player;

            private int _counterTarget;
            private string _successName, _failName, _successEffect, _failEffect, _requirementString;
            protected float SuccessModifier, FailModifier;

            private int _counter;
            public BrandStatus Status = BrandStatus.Locked;

            protected Brand(Player player, string riteName)
            {
                Player = player;
                _riteName = riteName;
                SetStatus(BrandStatus.Locked);
            }

            public void ReadData(XmlNode root)
            {
                root = root.SelectSingleNode(_riteName);
                _requirementString = root.StringFromNode("Requirement");
                _counterTarget = root.IntFromNode("TargetValue");
                _requirementString = _requirementString.Replace("num", _counterTarget.ToString());
                _successName = root.StringFromNode("SuccessName");
                _successEffect = root.StringFromNode("SuccessEffect");
                SuccessModifier = root.FloatFromNode("SuccessValue");
                _failName = root.StringFromNode("FailName");
                _failEffect = root.StringFromNode("FailEffect");
                FailModifier = root.FloatFromNode("FailValue");
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
                UiBrandMenu.ShowBrand(this);
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

            public void Load(XmlNode doc)
            {
                _counter = doc.IntFromNode("TimeRemaining");
                Status = (BrandStatus) doc.IntFromNode("Status");
                if (this is StrengthBrand || this is EnduranceBrand || this is PerceptionBrand || this is WillpowerBrand) return;
                if (Status == BrandStatus.Succeeded) OnSucceed();
                else if (Status == BrandStatus.Failed) OnFail();
            }

            public XmlNode Save(XmlNode doc)
            {
                doc = doc.CreateChild("Brand");
                doc.CreateChild("Name", _riteName);
                doc.CreateChild("TimeRemaining", _counter);
                doc.CreateChild("Status", (int) Status);
                return doc;
            }

            public string GetSuccessName()
            {
                return _successName;
            }

            public string GetFailName()
            {
                return _failName;
            }

            public string GetEffectString()
            {
                return Status == BrandStatus.Succeeded ? _successEffect : _failEffect;
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
                    if (_activeBrands.Contains(brand)) return;
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
            public StrengthBrand(Player player) : base(player, "Power")
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.ChangeStrengthMax((int) SuccessModifier);
            }

            protected override void OnFail()
            {
                Player.Attributes.ChangeStrengthMax((int) FailModifier);
            }

            protected override string GetProgressSubstring()
            {
                return "Dealt " + Progress() + " damage";
            }
        }

        private class EnduranceBrand : Brand
        {
            public EnduranceBrand(Player player) : base(player, "Stamina")
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.ChangeEnduranceMax((int) SuccessModifier);
            }

            protected override void OnFail()
            {
                Player.Attributes.ChangeEnduranceMax((int) FailModifier);
            }

            protected override string GetProgressSubstring()
            {
                return "Explored " + Progress() + " regions";
            }
        }

        private class PerceptionBrand : Brand
        {
            public PerceptionBrand(Player player) : base(player, "Vigilance")
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.ChangePerceptionMax((int) SuccessModifier);
            }

            protected override void OnFail()
            {
                Player.Attributes.ChangePerceptionMax((int) FailModifier);
            }

            protected override string GetProgressSubstring()
            {
                return "Found " + Progress() + " items";
            }
        }

        private class WillpowerBrand : Brand
        {
            public WillpowerBrand(Player player) : base(player, "Resolution")
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.ChangeWillpowerMax((int) SuccessModifier);
            }

            protected override void OnFail()
            {
                Player.Attributes.ChangeWillpowerMax((int) FailModifier);
            }

            protected override string GetProgressSubstring()
            {
                return "Used " + Progress() + " skills";
            }
        }

        private class EssenceChangeBrand : Brand
        {
            public EssenceChangeBrand(Player player) : base(player, "Insight")
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.EssenceRecoveryModifier += SuccessModifier;
            }

            protected override void OnFail()
            {
                Player.Attributes.DurabilityLossModifier += FailModifier;
            }

            protected override string GetProgressSubstring()
            {
                return "Infused " + Progress() + " essence";
            }
        }

        private class HealthRecoveryBrand : Brand
        {
            public HealthRecoveryBrand(Player player) : base(player, "Revival")
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.RallyHealthModifier += SuccessModifier;
            }

            protected override void OnFail()
            {
                Player.Attributes.StartHealthModifier += FailModifier;
            }

            protected override string GetProgressSubstring()
            {
                return "Taken " + Progress() + " damage";
            }
        }

        private class WillpowerRecoveryBrand : Brand
        {
            public WillpowerRecoveryBrand(Player player) : base(player, "Apathy")
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.ClaimRegionWillpowerGainModifier += SuccessModifier;
            }

            protected override void OnFail()
            {
                Player.Attributes.EnemyKillHealthLoss += FailModifier;
            }

            protected override string GetProgressSubstring()
            {
                return "Killed " + Progress() + " enemies";
            }
        }

        private class OnlySkillBrand : Brand
        {
            public OnlySkillBrand(Player player) : base(player, "Mastery")
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.FreeSkillChance += SuccessModifier;
            }

            protected override void OnFail()
            {
                Player.Attributes.SkillDisableChance += FailModifier;
            }

            protected override string GetProgressSubstring()
            {
                return "Used only skills in " + Progress() + " battles";
            }
        }

        private class SkillKillBrand : Brand
        {
            public SkillKillBrand(Player player) : base(player, "Prowess")
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.InstantCooldownChance += SuccessModifier;
            }

            protected override void OnFail()
            {
                Player.Attributes.SkillDisableChance += FailModifier;
            }

            protected override string GetProgressSubstring()
            {
                return "Killed " + Progress() + " enemies with skills";
            }
        }

        private class InstantReloadBrand : Brand
        {
            public InstantReloadBrand(Player player) : base(player, "Ingenuity")
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.ReloadOnLastRound = true;
            }

            protected override void OnFail()
            {
                Player.Attributes.ReloadFailureChance += FailModifier;
            }

            protected override string GetProgressSubstring()
            {
                return "Killed " + Progress() + " enemies with last round";
            }
        }

        private class AutomaticReloadBrand : Brand
        {
            public AutomaticReloadBrand(Player player) : base(player, "Finesse")
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.ReloadOnEmptyMag = true;
            }

            protected override void OnFail()
            {
                Player.Attributes.ReloadFailureChance += FailModifier;
            }

            protected override string GetProgressSubstring()
            {
                return "Used only bullets in " + Progress() + " battles";
            }
        }

        private class IgniteBrand : Brand
        {
            public IgniteBrand(Player player) : base(player, "Fire")
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.FireExplodeChance += SuccessModifier;
            }

            protected override void OnFail()
            {
                Player.Attributes.FireDamageModifier += FailModifier;
            }

            protected override string GetProgressSubstring()
            {
                return "Ignited " + Progress() + " enemies";
            }
        }

        private class DecayBrand : Brand
        {
            public DecayBrand(Player player) : base(player, "Decay")
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.DecayExplodeChance += SuccessModifier;
            }

            protected override void OnFail()
            {
                Player.Attributes.DecayDamageModifier += FailModifier;
            }

            protected override string GetProgressSubstring()
            {
                return "Decayed " + Progress() + " enemies";
            }
        }

        private class SicknessBrand : Brand
        {
            public SicknessBrand(Player player) : base(player, "Sickness")
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.SpreadSickness = true;
            }

            protected override void OnFail()
            {
                Player.Attributes.SicknessStackModifier += FailModifier;
            }

            protected override string GetProgressSubstring()
            {
                return "Sickened " + Progress() + " enemies";
            }
        }

        private class ResourceBrand : Brand
        {
            public ResourceBrand(Player player) : base(player, "Scavenging")
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.ResourceFindModifier += SuccessModifier;
            }

            protected override void OnFail()
            {
                Player.Attributes.ResourceFindModifier += FailModifier;
            }

            protected override string GetProgressSubstring()
            {
                return "Found " + Progress() + " resources";
            }
        }

        private class FoodBrand : Brand
        {
            public FoodBrand(Player player) : base(player, "Gathering")
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.HungerModifier += SuccessModifier;
            }

            protected override void OnFail()
            {
                Player.Attributes.WaterHungerModifier += FailModifier;
            }

            protected override string GetProgressSubstring()
            {
                return "Found " + Progress() + " food";
            }
        }

        private class WaterBrand : Brand
        {
            public WaterBrand(Player player) : base(player, "Divining")
            {
            }

            protected override void OnSucceed()
            {
                Player.Attributes.ThirstModifier += SuccessModifier;
            }

            protected override void OnFail()
            {
                Player.Attributes.FoodThirstModifier += FailModifier;
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