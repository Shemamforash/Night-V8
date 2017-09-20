using Game.Characters;
using UnityEngine;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class InventoryCharacterUi : SimpleItemUi
    {
        private DesolationCharacter _character;

        public InventoryCharacterUi(DesolationCharacter character, Transform parent) : base(parent)
        {
            _character = character;
        }

        public override void Update()
        {
            _nameText.text = _character.name;
            _typeText.text = _character.CharacterTrait.Name + " " + _character.CharacterClass.Name;
        }
    }
}