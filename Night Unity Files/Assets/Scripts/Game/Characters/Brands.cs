namespace Game.Characters.Brands
{
    public class LifeBrand : Brand
    {
        public LifeBrand(Player player) : base(player, "Power")
        {
        }

        protected override void OnSucceed()
        {
            Player.Attributes.ChangeLifeMax(1);
        }


        protected override string GetProgressSubstring()
        {
            return "Dealt " + Progress() + " damage";
        }
    }

    public class GritBrand : Brand
    {
        public GritBrand(Player player) : base(player, "Stamina")
        {
        }

        protected override void OnSucceed()
        {
            Player.Attributes.ChangeGritMax(1);
        }


        protected override string GetProgressSubstring()
        {
            return "Discovered " + Progress() + " regions";
        }
    }

    public class FocusBrand : Brand
    {
        public FocusBrand(Player player) : base(player, "Vigilance")
        {
        }

        protected override void OnSucceed()
        {
            Player.Attributes.ChangeFocusMax(1);
        }


        protected override string GetProgressSubstring()
        {
            return "Found " + Progress() + " items";
        }
    }

    public class WillBrand : Brand
    {
        public WillBrand(Player player) : base(player, "Resolution")
        {
        }

        protected override void OnSucceed()
        {
            Player.Attributes.ChangeWillMax(1);
        }


        protected override string GetProgressSubstring()
        {
            return "Used " + Progress() + " skills";
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

        protected override string GetProgressSubstring()
        {
            return "Taken " + Progress() + " damage";
        }
    }

    public class WillRecoveryBrand : Brand
    {
        public WillRecoveryBrand(Player player) : base(player, "Apathy")
        {
        }

        protected override void OnSucceed()
        {
            Player.Attributes.ClaimRegionWillGainModifier += SuccessModifier;
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

        protected override string GetProgressSubstring()
        {
            return "Consumed " + Progress() + " adrenaline";
        }
    }

    public class InstantReloadBrand : Brand
    {
        public InstantReloadBrand(Player player) : base(player, "Perfection")
        {
        }

        protected override void OnSucceed()
        {
            Player.Attributes.ReloadOnFatalShot = true;
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

        protected override string GetProgressSubstring()
        {
            return Progress() + " perfect reloads";
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

        protected override string GetProgressSubstring()
        {
            return "Burnt " + Progress() + " enemies";
        }
    }

    public class DecayBrand : Brand
    {
        public DecayBrand(Player player) : base(player, "Shatter")
        {
        }

        protected override void OnSucceed()
        {
            Player.Attributes.DecayExplodeChance += SuccessModifier;
        }

        protected override string GetProgressSubstring()
        {
            return "Shattered " + Progress() + " enemies";
        }
    }

    public class VoidBrand : Brand
    {
        public VoidBrand(Player player) : base(player, "Void")
        {
        }

        protected override void OnSucceed()
        {
            Player.Attributes.SpreadVoid = true;
        }

        protected override string GetProgressSubstring()
        {
            return "Cursed " + Progress() + " enemies";
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

        protected override string GetProgressSubstring()
        {
            return "Found " + Progress() + " resources";
        }
    }

    public class FoodBrand : Brand
    {
        public FoodBrand(Player player) : base(player, "Hunting")
        {
        }

        protected override void OnSucceed()
        {
            Player.Attributes.HungerModifier += SuccessModifier;
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

        protected override string GetProgressSubstring()
        {
            return "Found " + Progress() + " water";
        }
    }
}