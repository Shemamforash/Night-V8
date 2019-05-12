using Game.Combat.Player;
using Extensions;
using SamsHelper.ReactiveUI.Elements;

namespace Game.Combat.Ui
{
	public class EnemyUi : CharacterUi
	{
		public static EnemyUi         Instance;
		public        EnhancedText    NameText;
		public        UIHitController UiHitController;

		public override void Awake()
		{
			base.Awake();
			NameText        = gameObject.FindChildWithName<EnhancedText>("Name");
			UiHitController = gameObject.FindChildWithName<UIHitController>("Cover");
			Instance        = this;
		}

		protected override void LateUpdate()
		{
			Character = PlayerCombat.Instance.GetTarget();
			base.LateUpdate();
			if (Character == null) return;
			NameText.SetText(Character.GetDisplayName());
		}
	}
}