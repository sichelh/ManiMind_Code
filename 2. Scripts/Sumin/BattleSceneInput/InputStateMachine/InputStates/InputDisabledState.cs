public class InputDisabledState : IInputState
{
    private readonly UnitSelector selector;
    private readonly InputContext context;

    public InputDisabledState(InputContext context, UnitSelector selector)
    {
        this.context = context;
        this.selector = selector;
    }

    public void Enter()
    {
        selector.ShowSelectableUnits(context.UnitLayer, false);
    }

    public void HandleInput() { }
    public void Exit() { }
}
