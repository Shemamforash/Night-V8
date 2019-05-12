using DG.Tweening;
using Facilitating.UIControllers;
using Game.Combat.Misc;
using Extensions;
using UnityEngine;

namespace Game.Combat.Ui
{
	public class CharacterUi : MonoBehaviour
	{
		private   UIArmourController    _armourController;
		private   UIHealthBarController _healthBarController;
		private   CanTakeDamage         _lastCharacter;
		private   Tweener               _voidGlowTween;
		private   CanvasGroup           CanvasGroup, _voidCanvas, _voidGlowCanvas;
		protected CanTakeDamage         Character;

		public virtual void Awake()
		{
			CanvasGroup          = GetComponent<CanvasGroup>();
			_healthBarController = gameObject.FindChildWithName<UIHealthBarController>("Health");
			_armourController    = gameObject.FindChildWithName<UIArmourController>("Armour");
			GameObject voidObject = gameObject.FindChildWithName("Void Meter");
			_voidCanvas           = voidObject.FindChildWithName<CanvasGroup>("Active");
			_voidGlowCanvas       = voidObject.FindChildWithName<CanvasGroup>("Glow");
			_voidGlowCanvas.alpha = 0f;
			_voidCanvas.alpha     = 0f;
		}

		private void SetAlpha(float a)
		{
			CanvasGroup.alpha = a;
		}

		protected virtual void LateUpdate()
		{
			if (Character == null)
			{
				SetAlpha(0);
				return;
			}

			SetAlpha(1);
			_healthBarController.SetValue(Character.HealthController.GetHealth(), Character == _lastCharacter);
			_armourController.UpdateArmour(Character.ArmourController);
			UpdateVoid();
			_lastCharacter = Character;
		}

		private void UpdateVoid()
		{
			if (Character.WasJustVoided())
			{
				Debug.Log(Character.WasJustVoided());
				_voidGlowTween?.Complete();
				_voidGlowCanvas.alpha = 1f;
				_voidGlowTween        = _voidGlowCanvas.DOFade(0f, 1f);
			}

			_voidCanvas.alpha = Character.GetVoid();
		}
	}
}