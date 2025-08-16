public interface ITurnState
{
    void OnEnter(Unit unit);
    void OnUpdate(Unit unit);
    void OnExit(Unit unit);
}