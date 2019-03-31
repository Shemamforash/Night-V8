using Game.Characters;
using Game.Combat.Generation.Shrines;
using Game.Combat.Player;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.MenuSystem;

public class BrandReplaceMenuController : Menu
{
    private static RiteShrineBehaviour _riteShrine;
    private static Brand _brand;

    private static CloseButtonController _closeButton;
    private CharacterBrandUIController _brandUi;
    private BrandManager _brandManager;

    protected override void Awake()
    {
        base.Awake();
        _brandUi = gameObject.FindChildWithName<CharacterBrandUIController>("Brands");
        _closeButton = gameObject.FindChildWithName<CloseButtonController>("Close Button");
        _closeButton.SetOnClick(Close);
    }

    public static void Show(RiteShrineBehaviour riteShrine, Brand brand)
    {
        _riteShrine = riteShrine;
        _brand = brand;
        MenuStateMachine.ShowMenu("Brand Replace Menu");
        _closeButton.Enable();
    }

    public override void PreEnter()
    {
        base.PreEnter();
        _brandManager = PlayerCombat.Instance.Player.BrandManager;
        _brandUi.UpdateBrands(_brandManager);
    }

    public void OverrideBrandOne()
    {
        _brandManager.SetActiveBrandOne(_brand);
        Hide();
    }

    public void OverrideBrandTwo()
    {
        _brandManager.SetActiveBrandTwo(_brand);
        Hide();
    }

    public void OverrideBrandThree()
    {
        _brandManager.SetActiveBrandThree(_brand);
        Hide();
    }

    public void Hide()
    {
        _riteShrine.ActivateBrand();
        Close();
    }

    private static void Close()
    {
        _riteShrine = null;
        _closeButton.Disable();
        MenuStateMachine.ShowMenu("HUD");
    }
}