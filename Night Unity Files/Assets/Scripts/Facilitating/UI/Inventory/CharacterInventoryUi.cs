using Game.Characters;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace Facilitating.UI.Inventory
{
    public class CharacterInventoryUi : BaseInventoryUi
    {
        private readonly DesolationCharacter _character;

        public CharacterInventoryUi(DesolationCharacter character, Transform parent) : base(character, parent, "Prefabs/Inventory/Character")
        {
            _character = character;
        }

        protected override void CacheUiElements()
        {
            
        }

        public override void Update()
        {
            
        }
    }
}