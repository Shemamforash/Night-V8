using System.Xml;
using Facilitating;
using Facilitating.Persistence;
using Facilitating.UIControllers;
using Game.Global;
using NUnit.Framework;
using SamsHelper.Libraries;
using UnityEngine;

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
            _recipe = null;
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
            _recipeName = _recipe.Name;
            if (_recipeName == "Fire") LightFire();
            else CraftThing();
            SetDuration(WorldState.MinutesPerHour / 4);
            Enter();
        }

        public override XmlNode Load(XmlNode doc)
        {
            doc = base.Load(doc);
            string recipeName = doc.StringFromNode("Recipe");
            if (recipeName == "") return doc;
            _recipe = Recipe.FindRecipe(recipeName);
            if (_recipe.Name == "Fire") LightFire();
            else CraftThing();
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