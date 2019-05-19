using System.Xml;
using Extensions;
using Facilitating;
using Facilitating.Persistence;
using Facilitating.UIControllers;
using Game.Global;
using NUnit.Framework;

namespace Game.Characters.CharacterActions
{
	public class Craft : BaseCharacterAction
	{
		private Recipe _recipe;
		private string _recipeName;

		public Craft(Player playerCharacter) : base("Craft", playerCharacter)
		{
		}

		protected override void OnClick()
		{
			UiGearMenuController.ShowCraftingMenu();
		}

		private void CraftRecipe()
		{
			_recipe.Craft();
			AchievementManager.Instance().IncreaseItemsCrafted();
			_recipe = null;
			PlayerCharacter.RestAction.Enter();
			ReopenMenu();
		}

		private void ReopenMenu()
		{
			CharacterManager.SelectedCharacter = PlayerCharacter;
			UiGearMenuController.ShowCraftingMenu();
		}

		private void CraftThing()
		{
			DisplayName = "Crafting";
			MinuteCallback = () =>
			{
				--Duration;
				if (Duration != 0) return;
				CraftRecipe();
			};
		}

		public void AbortCraft() => _recipe?.RestoreResources();

		public void StartCrafting(Recipe recipe)
		{
			Assert.IsTrue(_recipe == null);
			_recipe = recipe;
			_recipe.ConsumeResources();
			_recipeName = _recipe.Name;
			CraftThing();
			SetDuration(WorldState.MinutesPerHour / 4);
			Enter();
		}

		public override XmlNode Load(XmlNode doc)
		{
			doc = base.Load(doc);
			string recipeName = doc.ParseString("Recipe");
			if (recipeName == "") return doc;
			_recipe = Recipe.FindRecipe(recipeName);
			CraftThing();
			return doc;
		}

		public override XmlNode Save(XmlNode doc)
		{
			doc = base.Save(doc);
			string recipeName = _recipe == null ? "" : _recipe.Name;
			doc.CreateChild("Recipe", recipeName);
			return doc;
		}

		public string GetRecipeName() => _recipeName;
	}
}