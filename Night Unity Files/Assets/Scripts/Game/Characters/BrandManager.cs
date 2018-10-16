using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters.Brands;
using Game.Combat.Generation.Shrines;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Characters
{
    public class BrandManager
    {
        private readonly List<Brand> _lockedBrands = new List<Brand>();
        private Player _player;
        private readonly List<Brand> _completedBrands = new List<Brand>();
        private Brand _activeBrandOne, _activeBrandTwo, _activeBrandThree;

        public List<Brand> GetActiveBrands()
        {
            return new List<Brand> {_activeBrandOne, _activeBrandTwo, _activeBrandThree};
        }

        public void Load(XmlNode doc)
        {
            XmlNode brandsNode = doc.SelectSingleNode("Brands");
            foreach (XmlNode brandNode in brandsNode.SelectNodes("Brand"))
            {
                LoadBrand(brandNode);
            }

            _activeBrandOne = LoadBrand(brandsNode.SelectSingleNode("Brand_0"));
            _activeBrandTwo = LoadBrand(brandsNode.SelectSingleNode("Brand_1"));
            _activeBrandThree = LoadBrand(brandsNode.SelectSingleNode("Brand_2"));
        }

        private Brand LoadBrand(XmlNode brandNode)
        {
            string name = brandNode.StringFromNode("Name");
            foreach (Brand brand in _lockedBrands)
            {
                if (brand.GetName() != name) continue;
                brand.Load(brandNode);
                return brand;
            }

            return null;
        }

        public void Save(XmlNode doc)
        {
            doc = doc.CreateChild("Brands");
            _completedBrands.ForEach(b => { b.Save(doc.CreateChild("Brand")); });
            List<Brand> activeBrands = GetActiveBrands();
            for (int i = 0; i < activeBrands.Count; ++i)
            {
                XmlNode brandNode = doc.CreateChild("Brand_" + i);
                if (activeBrands[i] == null) continue;
                activeBrands[i].Save(brandNode);
            }

            return;
        }

        public List<Brand> GetBrandChoice(int ritesRemaining)
        {
            List<Brand> possibleBrands = new List<Brand>();
            if (_lockedBrands.Count == 0) return possibleBrands;
            List<Brand> activeBrands = GetActiveBrands();
            bool enduranceBrandsAllowed = true;
            bool strengthBrandsAllowed = true;
            bool willpowerBrandsAllowed = true;
            bool perceptionBrandsAllowed = true;
            activeBrands.ForEach(b =>
            {
                if (b is EnduranceBrand) enduranceBrandsAllowed = false;
                if (b is StrengthBrand) strengthBrandsAllowed = false;
                if (b is WillpowerBrand) willpowerBrandsAllowed = false;
                if (b is PerceptionBrand) perceptionBrandsAllowed = false;
            });
            _lockedBrands.ForEach(b =>
            {
                if (!b.PlayerRequirementsMet(CharacterManager.SelectedCharacter)) return;
                if (b is EnduranceBrand)
                {
                    if (!enduranceBrandsAllowed) return;
                    enduranceBrandsAllowed = false;
                }

                if (b is StrengthBrand)
                {
                    if (!strengthBrandsAllowed) return;
                    strengthBrandsAllowed = false;
                }

                if (b is WillpowerBrand)
                {
                    if (!willpowerBrandsAllowed) return;
                    willpowerBrandsAllowed = false;
                }

                if (b is PerceptionBrand)
                {
                    if (!perceptionBrandsAllowed) return;
                    perceptionBrandsAllowed = false;
                }

                if (possibleBrands.Any(v => v.GetType() == b.GetType())) return;
                possibleBrands.Add(b);
            });
            possibleBrands.Shuffle();
            List<Brand> brandSelection = new List<Brand>();
            for (int i = 0; i < ritesRemaining && i < possibleBrands.Count; ++i) brandSelection.Add(_lockedBrands[i]);
            return brandSelection;
        }

        public void SetActiveBrandOne(Brand brand)
        {
            _activeBrandOne?.SetStatus(BrandStatus.Locked);
            _activeBrandOne = brand;
            brand.SetStatus(BrandStatus.Active);
        }

        public void SetActiveBrandTwo(Brand brand)
        {
            _activeBrandTwo?.SetStatus(BrandStatus.Locked);
            _activeBrandTwo = brand;
            brand.SetStatus(BrandStatus.Active);
        }

        public void SetActiveBrandThree(Brand brand)
        {
            _activeBrandThree?.SetStatus(BrandStatus.Locked);
            _activeBrandThree = brand;
            brand.SetStatus(BrandStatus.Active);
        }

        public void Initialise(Player player)
        {
            _player = player;
            CreateBrands();
            XmlNode node = Helper.OpenRootNode("Brands");
            _lockedBrands.ForEach(b => b.ReadData(node));
            Assert.IsTrue(_completedBrands.Count == 0);
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
            new AdrenalineUsedBrand(_player);
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

        private void UpdateBrandValue(Type type, int amount)
        {
            List<Brand> activeBrands = GetActiveBrands();
            activeBrands.ForEach(b =>
            {
                if (b != null && b.GetType() == type) b.UpdateValue(amount);
            });
        }

        public void IncreaseDamageDealt(int damage) => UpdateBrandValue(typeof(StrengthBrand), damage);
        public void IncreaseItemsFound() => UpdateBrandValue(typeof(PerceptionBrand), 1);
        public void IncreaseSkillsUsed() => UpdateBrandValue(typeof(WillpowerBrand), 1);
        public void IncreaseRegionsExplored() => UpdateBrandValue(typeof(EnduranceBrand), 1);
        public void IncreaseEssenceInfused() => UpdateBrandValue(typeof(EssenceChangeBrand), 1);
        public void IncreaseDamageTaken(int damage) => UpdateBrandValue(typeof(HealthRecoveryBrand), damage);
        public void IncreaseEnemiesKilled() => UpdateBrandValue(typeof(WillpowerRecoveryBrand), 1);
        public void IncreaseBattlesNoSkills() => UpdateBrandValue(typeof(AutomaticReloadBrand), 1);
        public void IncreaseResourceFound() => UpdateBrandValue(typeof(ResourceBrand), 1);
        public void IncreaseFoodFound() => UpdateBrandValue(typeof(FoodBrand), 1);
        public void IncreaseWaterFound() => UpdateBrandValue(typeof(WaterBrand), 1);
        public void IncreaseAdrenalineUsed(int amount) => UpdateBrandValue(typeof(AdrenalineUsedBrand), amount);
        public void IncreaseBurnCount() => UpdateBrandValue(typeof(IgniteBrand), 1);
        public void IncreaseDecayCount() => UpdateBrandValue(typeof(DecayBrand), 1);
        public void IncreaseSickenCount() => UpdateBrandValue(typeof(SicknessBrand), 1);

        public void UpdateBrandStatus(Brand brand)
        {
            switch (brand.Status)
            {
                case BrandStatus.Locked:
                    if (_lockedBrands.Contains(brand)) return;
                    _lockedBrands.Add(brand);
                    _completedBrands.Remove(brand);
                    break;
                case BrandStatus.Active:
                    _completedBrands.Remove(brand);
                    _lockedBrands.Remove(brand);
                    break;
                default:
                    if (_completedBrands.Contains(brand)) return;
                    _completedBrands.Add(brand);
                    _lockedBrands.Remove(brand);
                    break;
            }
        }


        public bool TryActivateBrand(Brand targetBrand)
        {
            if (_activeBrandOne == null)
            {
                _activeBrandOne = targetBrand;
                return true;
            }

            if (_activeBrandTwo == null)
            {
                _activeBrandTwo = targetBrand;
                return true;
            }

            if (_activeBrandThree == null)
            {
                _activeBrandThree = targetBrand;
                return true;
            }

            return false;
        }
    }
}