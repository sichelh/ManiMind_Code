using System.Collections;
using TMPro;
using UnityEngine;

public class TypeWriter : MonoBehaviour
{
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private float charsPerSecond = 35f; // 1초에 출력할 글자 수
    [SerializeField] private bool pauseEffect = true;    // 구두점에서 잠깐 멈출지 여부

    private Coroutine typingCoroutine;

    // 현재 타이핑 중인지
    public bool IsTyping => typingCoroutine != null;

    // 타이핑 효과 시작
    // overrideSpeed가 null이 아니면 기존 속도를 덮어씀
    public void StartTyping(string content, float? overrideSpeed = null)
    {
        StopTyping(); // 기존 효과 중단

        if (overrideSpeed.HasValue)
            charsPerSecond = overrideSpeed.Value;

        typingCoroutine = StartCoroutine(TypeText(content));
    }

    // 타이핑 즉시 완료
    public void CompleteTyping()
    {
        if (!IsTyping) return;

        StopCoroutine(typingCoroutine);
        targetText.ForceMeshUpdate();   // 텍스트 데이터 강제 갱신
        targetText.maxVisibleCharacters = targetText.textInfo.characterCount;   // 모든 글자 표시
        typingCoroutine = null;
    }

    // 타이핑 효과 중단
    private void StopTyping()
    {
        if(typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    // 한 글자씩 출력하는 코루틴
    private IEnumerator TypeText(string content)
    {
        targetText.text = content;
        targetText.ForceMeshUpdate();
        targetText.maxVisibleCharacters = 0; // 시작 시 아무 글자도 안 보이게

        int totalChars = targetText.textInfo.characterCount;

        for (int i = 0; i < totalChars; i++)
        {
            targetText.maxVisibleCharacters = i + 1; // 한 글자씩 추가 표시

            // 기본 지연 시간 (속도의 역수)
            float delay = 1f / Mathf.Max(1f, charsPerSecond);

            // 구두점에서 추가 지연
            if (pauseEffect && i < totalChars)
            {
                char c = targetText.textInfo.characterInfo[i].character;
                if (c == '.' || c == '!' || c == '?')
                    delay *= 6f; // 마침표 계열은 긴 정지
                else if (c == ',' || c == ';' || c == '、' || c == '，')
                    delay *= 3f; // 쉼표 계열은 짧은 정지
            }

            yield return new WaitForSeconds(delay);
        }

        typingCoroutine = null;
    }
}
