public interface IGachaHandler
{
    bool CanDraw(int count); // 가챠 뽑을 수 있는지 확인용
    int GetTotalCost(int count); // 할인 포함 가챠 비용 
    string GetGachaTypeName(); // 가챠 이름 UI 띄우는 용도
    void DrawAndDisplayResult(int count); // 종류별 가챠 후 결과 보여주기
}
