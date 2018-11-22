using System.Xml;
using Facilitating;
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

        private void LightFire()
        {
            DisplayName = "Lighting Fire";
            MinuteCallback = () =>
            {
                --Duration;
                Campfire.Tend();
                if (Duration != 0) return;
                Campfire.FinishTending();
                PlayerCharacter.RestAction.Enter();
            };
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

        public void StartCrafting(Recipe recipe)
        {
            Assert.IsTrue(_recipe == null);
            _recipe = recipe;
            _recipe.ConsumeResources();
            if (recipe.Name == "Fire") LightFire();
            else CraftThing();
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