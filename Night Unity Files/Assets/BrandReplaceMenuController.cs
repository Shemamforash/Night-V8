using Game.Characters;
using Game.Combat.Generation.Shrines;
using Game.Combat.Player;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.MenuSystem;

public class BrandReplaceMenuController : Menu
{
    private static RiteShrineBehaviour _riteShrine;
    private static Brand _brand;

    private CharacterBrandUIController _brandUi;
    private BrandManager _brandManager;

    public override void Awake()
    {
        base.Awake();
        _brandUi = gameObject.FindChildWithName<CharacterBrandUIController>("Brands");
    }

    public static void Show(RiteShrineBehaviour riteShrine, Brand brand)
    {
        _riteShrine = riteShrine;
        _brand = brand;
        MenuStateMachine.ShowMenu("Brand Replace Menu");
    }

    public override void Enter()
    {
        base.Enter();
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
        MenuStateMachine.ShowMenu("HUD");
    }
}