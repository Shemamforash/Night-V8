using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Player;
using Game.Combat.Ui;
using SamsHelper.Input;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
    public class RiteShrineBehaviour : MonoBehaviour, IInputListener
    {
        public const int Width = 5;
        private static GameObject _riteShrinePrefab;
        private List<Characters.Player> _visitedPlayers = new List<Characters.Player>();
        private RiteColliderBehaviour _collider1, _collider2, _collider3;
        private List<BrandManager.Brand> _brandChoice;
        private BrandManager.Brand _targetBrand;

        public void Awake()
        {
            _collider1 = Helper.FindChildWithName<RiteColliderBehaviour>(gameObject, "Collider 1");
            _collider2 = Helper.FindChildWithName<RiteColliderBehaviour>(gameObject, "Collider 2");
            _collider3 = Helper.FindChildWithName<RiteColliderBehaviour>(gameObject, "Collider 3");
            _brandChoice = CharacterManager.SelectedCharacter.BrandManager.GetBrandChoice();
            if (_brandChoice.Count < 3)
            {
                _collider2.gameObject.SetActive(false);
            }

            if (_brandChoice.Count < 2)
            {
                _collider1.gameObject.SetActive(false);
            }
        }

        public static void Generate(Vector2 position)
        {
            if (_riteShrinePrefab == null) _riteShrinePrefab = Resources.Load<GameObject>("Prefabs/Combat/Rite Shrine");
            GameObject riteShrineObject = Instantiate(_riteShrinePrefab);
            riteShrineObject.transform.position = position;
        }

        public void EnterShrineCollider(RiteColliderBehaviour riteColliderBehaviour)
        {
            InputHandler.RegisterInputListener(this);
            if (riteColliderBehaviour == _collider1)
            {
                _targetBrand = _brandChoice[0];
            }
            else if (riteColliderBehaviour == _collider2)
            {
                _targetBrand = _brandChoice[1];
            }
            else if (riteColliderBehaviour == _collider3)
            {
                _targetBrand = _brandChoice[2];
            }

            PlayerUi.SetEventText(_targetBrand.GetName());
        }

        public void ExitShrineCollider()
        {
            _targetBrand = null;
            InputHandler.UnregisterInputListener(this);
            PlayerUi.FadeTextOut();
        }

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (axis == InputAxis.TakeItem)
            {
                PlayerCombat.Instance.Player.BrandManager.SetActiveBrand(_targetBrand);
            }
        }

        public void OnInputUp(InputAxis axis)
        {
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }
    }
}