using System.Xml;
using Facilitating.Persistence;
using Facilitating.UIControllers;
using Game.Global;
using NUnit.Framework;
using SamsHelper.Libraries;

namespace Game.Characters.CharacterActions
{
    public class Craft : BaseCharacterAction
    {
        private Recipe _recipe;

        public Craft(Player playerCharacter) : base("Craft", playerCharacter)
        {
            DisplayName = "Crafting";
            MinuteCallback = () =>
            {
                --Duration;
                if (Duration != 0) return;
                CraftRecipe();
            };
        }

        protected override void OnClick()
        {
            UiGearMenuController.ShowCraftingMenu();
        }

        private void CraftRecipe()
        {
            _recipe.Craft();
            _recipe = null;
            PlayerCharacter.RestAction.Enter();
        }

        public void StartCrafting(Recipe recipe)
        {
            Assert.IsTrue(_recipe == null);
            if (recipe.Name == "Fire") CharacterManager.SelectedCharacter.LightFireAction.Enter();
            _recipe = recipe;
            _recipe.ConsumeResources();
            SetDuration();
            Enter();
        }

        public override XmlNode Load(XmlNode doc)
        {
            doc = base.Save(doc);
            _recipe = Recipe.FindRecipe(doc.StringFromNode("Recipe"));
            return doc;
        }

        public override XmlNode Save(XmlNode doc)
        {
            doc = base.Save(doc);
            doc.CreateChild("Recipe", _recipe.Name);
            return doc;
        }

        public string GetRecipeName()
        {
            return _recipe.Name;
        }
    }
}