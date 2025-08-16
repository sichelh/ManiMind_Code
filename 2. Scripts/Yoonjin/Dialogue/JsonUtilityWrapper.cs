using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Unity JsonUtility는 배열 JSON을 직접 역직렬화할 수 없기 때문에,
/// 내부적으로 "items" 필드로 감싼 구조체를 통해 배열처럼 처리하도록 도와주는 유틸 클래스.
/// </summary>
public static class JsonUtilityWrapper
{
    [Serializable]
    private class Wrapper<T>
    {
        public List<T> items;
    }

    /// <summary>
    /// 이미 "items"로 감싸진 JSON 문자열을 List<T>로 파싱한다.
    /// </summary>
    public static List<T> FromJsonArray<T>(string wrappedJson)
    {
        return JsonUtility.FromJson<Wrapper<T>>(wrappedJson).items;
    }
}