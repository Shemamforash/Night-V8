namespace Game.Characters.Brands
{
    public class StrengthBrand : Brand
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

    public class EnduranceBrand : Brand
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

    public class PerceptionBrand : Brand
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

    public class WillpowerBrand : Brand
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

    public class EssenceChangeBrand : Brand
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

    public class HealthRecoveryBrand : Brand
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

    public class WillpowerRecoveryBrand : Brand
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

    public class AdrenalineUsedBrand : Brand
    {
        public AdrenalineUsedBrand(Player player) : base(player, "Mastery")
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

    public class SkillKillBrand : Brand
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

    public class InstantReloadBrand : Brand
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

    public class AutomaticReloadBrand : Brand
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

    public class IgniteBrand : Brand
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

    public class DecayBrand : Brand
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

    public class SicknessBrand : Brand
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

    public class ResourceBrand : Brand
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

    public class FoodBrand : Brand
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

    public class WaterBrand : Brand
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
}