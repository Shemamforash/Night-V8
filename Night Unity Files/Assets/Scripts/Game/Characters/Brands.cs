using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Characters.Brands
{
	public class LifeBrand : Brand
	{
		public LifeBrand(Player player) : base(player, "Power")
		{
		}

		protected override void OnSucceed()
		{
			Player.Attributes.IncreaseAttribute(AttributeType.Life);
		}


		protected override string GetProgressSubstring() => "Dealt " + Progress() + " damage";
	}

	public class GritBrand : Brand
	{
		public GritBrand(Player player) : base(player, "Stamina")
		{
		}

		protected override void OnSucceed()
		{
			Player.Attributes.IncreaseAttribute(AttributeType.Life);
		}


		protected override string GetProgressSubstring() => "Discovered " + Progress() + " regions";
	}

	public class FocusBrand : Brand
	{
		public FocusBrand(Player player) : base(player, "Vigilance")
		{
		}

		protected override void OnSucceed()
		{
			Player.Attributes.IncreaseAttribute(AttributeType.Will);
		}


		protected override string GetProgressSubstring() => "Found " + Progress() + " items";
	}

	public class WillBrand : Brand
	{
		public WillBrand(Player player) : base(player, "Resolution")
		{
		}

		protected override void OnSucceed()
		{
			Player.Attributes.IncreaseAttribute(AttributeType.Will);
		}


		protected override string GetProgressSubstring() => "Used " + Progress() + " skills";
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

		protected override string GetProgressSubstring() => "Taken " + Progress() + " damage";
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

		protected override string GetProgressSubstring() => "Killed " + Progress() + " enemies with last round";
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

		protected override string GetProgressSubstring() => Progress() + " perfect reloads";
	}

	public class LifeStealBrand : Brand
	{
		public LifeStealBrand(Player player) : base(player, "Apathy")
		{
		}

		protected override void OnSucceed()
		{
			Player.Attributes.RecoverHealthOnKill = true;
		}

		protected override string GetProgressSubstring() => Progress() + " perfect reloads";
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

		protected override string GetProgressSubstring() => "Burnt " + Progress() + " enemies";
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

		protected override string GetProgressSubstring() => "Shattered " + Progress() + " enemies";
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

		protected override string GetProgressSubstring() => "Cursed " + Progress() + " enemies";
	}

	public class ResourceBrand : Brand
	{
		public ResourceBrand(Player player) : base(player, "Scavenging")
		{
		}

		protected override void OnSucceed()
		{
			Player.Attributes.CompassBonus = SuccessModifier;
		}

		protected override string GetProgressSubstring() => "Found " + Progress() + " resources";
	}
}