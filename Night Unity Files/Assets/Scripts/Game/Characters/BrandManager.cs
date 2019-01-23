using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters.Brands;
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
        private bool _gritBrandsAllowed;
        private bool _fettleBrandsAllowed;
        private bool _willBrandsAllowed;
        private bool _focusBrandsAllowed;

        public List<Brand> GetActiveBrands()
        {
            return new List<Brand> {_activeBrandOne, _activeBrandTwo, _activeBrandThree};
        }

        public void Load(XmlNode doc)
        {
            XmlNode brandsNode = doc.SelectSingleNode("Brands");
            foreach (XmlNode brandNode in brandsNode.SelectNodes("Brand")) LoadBrand(brandNode);
            _activeBrandOne = LoadBrand(brandsNode.SelectSingleNode("Brand0"));
            _activeBrandTwo = LoadBrand(brandsNode.SelectSingleNode("Brand1"));
            _activeBrandThree = LoadBrand(brandsNode.SelectSingleNode("Brand2"));
        }

        private Brand LoadBrand(XmlNode brandNode)
        {
            if (brandNode == null) return null;
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
                if (activeBrands[i] == null) continue;
                XmlNode brandNode = doc.CreateChild("Brand" + i);
                activeBrands[i].Save(brandNode);
            }
        }

        private void ResetAllowedBrands()
        {
            _gritBrandsAllowed = true;
            _fettleBrandsAllowed = true;
            _willBrandsAllowed = true;
            _focusBrandsAllowed = true;
            List<Brand> activeBrands = GetActiveBrands();
            activeBrands.ForEach(b =>
            {
                if (b is GritBrand) _gritBrandsAllowed = false;
                if (b is FettleBrand) _fettleBrandsAllowed = false;
                if (b is WillBrand) _willBrandsAllowed = false;
                if (b is FocusBrand) _focusBrandsAllowed = false;
            });
        }

        public List<Brand> GetBrandChoice(int ritesRemaining)
        {
            List<Brand> possibleBrands = new List<Brand>();
            if (_lockedBrands.Count == 0) return possibleBrands;
            ResetAllowedBrands();
            _lockedBrands.ForEach(b =>
            {
                if (!b.PlayerRequirementsMet(CharacterManager.SelectedCharacter)) return;
                if (possibleBrands.Any(v => v.GetType() == b.GetType())) return;
                possibleBrands.Add(b);
            });
            if (!_gritBrandsAllowed) possibleBrands.RemoveAll(b => b is GritBrand);
            if (!_fettleBrandsAllowed) possibleBrands.RemoveAll(b => b is FettleBrand);
            if (!_willBrandsAllowed) possibleBrands.RemoveAll(b => b is WillBrand);
            if (!_focusBrandsAllowed) possibleBrands.RemoveAll(b => b is FocusBrand);
            possibleBrands.Shuffle();
            List<Brand> brandSelection = new List<Brand>();
            for (int i = 0; i < ritesRemaining && i < possibleBrands.Count; ++i) brandSelection.Add(possibleBrands[i]);
            return brandSelection;
        }

        public bool TryActivateBrand(Brand targetBrand)
        {
            if (_activeBrandOne == null)
            {
                SetActiveBrandOne(targetBrand);
                return true;
            }

            if (_activeBrandTwo == null)
            {
                SetActiveBrandTwo(targetBrand);
                return true;
            }

            if (_activeBrandThree == null)
            {
                SetActiveBrandThree(targetBrand);
                return true;
            }

            return false;
        }

        public void SetBrandInactive(Brand brand)
        {
            if (brand == _activeBrandOne) _activeBrandOne = null;
            else if (brand == _activeBrandTwo) _activeBrandTwo = null;
            else if (brand == _activeBrandThree) _activeBrandThree = null;
        }

        public void SetActiveBrandOne(Brand brand)
        {
            _activeBrandOne?.SetStatus(BrandStatus.Locked);
            _activeBrandOne = brand;
            brand.SetStatus(BrandStatus.Active);
#if UNITY_EDITOR
//            brand.UpdateValue(5000);
#endif
        }

        public void SetActiveBrandTwo(Brand brand)
        {
            _activeBrandTwo?.SetStatus(BrandStatus.Locked);
            _activeBrandTwo = brand;
            brand.SetStatus(BrandStatus.Active);
#if UNITY_EDITOR
//            brand.UpdateValue(5000);
#endif
        }

        public void SetActiveBrandThree(Brand brand)
        {
            _activeBrandThree?.SetStatus(BrandStatus.Locked);
            _activeBrandThree = brand;
            brand.SetStatus(BrandStatus.Active);
#if UNITY_EDITOR
//            brand.UpdateValue(5000);
#endif
        }

        public void Initialise(Player player)
        {
            _player = player;
            CreateBrands();
            XmlNode node = Helper.OpenRootNode("Brands");
            _lockedBrands.ForEach(b => b.ReadData(node));
            Assert.IsTrue(_completedBrands.Count == 0);
        }

        public void UnlockAllBrands()
        {
            for (int i = _lockedBrands.Count - 1; i >= 0; --i) _lockedBrands[i].Succeed();
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
            new FettleBrand(_player);
            new FettleBrand(_player);
            new FettleBrand(_player);
            new FettleBrand(_player);
            new FettleBrand(_player);

            new FocusBrand(_player);
            new FocusBrand(_player);
            new FocusBrand(_player);
            new FocusBrand(_player);
            new FocusBrand(_player);

            new WillBrand(_player);
            new WillBrand(_player);
            new WillBrand(_player);
            new WillBrand(_player);
            new WillBrand(_player);

            new GritBrand(_player);
            new GritBrand(_player);
            new GritBrand(_player);
            new GritBrand(_player);
            new GritBrand(_player);
        }

        private void CreateOtherBrands()
        {
            new EssenceChangeBrand(_player);
            new EssenceChangeBrand(_player);
            new HealthRecoveryBrand(_player);
            new HealthRecoveryBrand(_player);
            new WillRecoveryBrand(_player);
            new WillRecoveryBrand(_player);
            new AutomaticReloadBrand(_player);
            new InstantReloadBrand(_player);
            new AdrenalineUsedBrand(_player);
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

        public void IncreaseDamageDealt(int damage) => UpdateBrandValue(typeof(FettleBrand), damage);
        public void IncreaseItemsFound() => UpdateBrandValue(typeof(FocusBrand), 1);
        public void IncreaseSkillsUsed() => UpdateBrandValue(typeof(WillBrand), 1);
        public void IncreaseRegionsExplored() => UpdateBrandValue(typeof(GritBrand), 1);

        public void IncreaseEssenceInfused() => UpdateBrandValue(typeof(EssenceChangeBrand), 1);
        public void IncreaseDamageTaken(int damage) => UpdateBrandValue(typeof(HealthRecoveryBrand), damage);
        public void IncreaseEnemiesKilled() => UpdateBrandValue(typeof(WillRecoveryBrand), 1);
        public void IncreasePerfectReloadCount() => UpdateBrandValue(typeof(AutomaticReloadBrand), 1);
        public void IncreaseLastRoundKills() => UpdateBrandValue(typeof(InstantReloadBrand), 1);
        public void IncreaseResourceFound() => UpdateBrandValue(typeof(ResourceBrand), 1);
        public void IncreaseFoodFound() => UpdateBrandValue(typeof(FoodBrand), 1);
        public void IncreaseWaterFound() => UpdateBrandValue(typeof(WaterBrand), 1);
        public void IncreaseAdrenalineUsed(int amount) => UpdateBrandValue(typeof(AdrenalineUsedBrand), amount);
        public void IncreaseBurnCount(int damage) => UpdateBrandValue(typeof(IgniteBrand), damage);
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
    }
}