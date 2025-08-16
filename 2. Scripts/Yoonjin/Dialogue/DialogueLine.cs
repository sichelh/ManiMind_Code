using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [Tooltip("이 대사가 속한 그룹의 키 (ex: STAGE_1_BEFORE)")]
    public string groupKey;

    [Tooltip("대사 중인 캐릭터 이름")]
    public string characterName;

    [Tooltip("대사 내용 (줄바꿈 지원)")]
    [TextArea]
    public string dialogue;

    [Tooltip("초상화 리소스 키 (Overlay 모드 전용)")]
    public string portraitKey;

    [Tooltip("배경 리소스 키")]
    public string backgroundKey;

    [Tooltip("좌측 초상화 리소스 키 (Fullscreen 모드 전용)")]
    public string portraitLeft;

    [Tooltip("우측 초상화 리소스 키 (Fullscreen 모드 전용)")]
    public string portraitRight;
}
