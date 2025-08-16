using System;
using System.Collections.Generic;

public static class EventBus
{
    // 이벤트 이름 → 콜백 리스트
    private static Dictionary<string, Action> eventTable = new();

    /// <summary>
    /// 특정 이름의 이벤트를 구독한다.
    /// <param name="eventName">구독할 이벤트 이름 (ex: "GachaClicked")</param>
    /// <param name="listener">호출할 콜백 함수</param>
    public static void Subscribe(string eventName, Action listener)
    {
        if (!eventTable.ContainsKey(eventName))
            eventTable[eventName] = listener;
        else
            eventTable[eventName] += listener;
    }

    /// <summary>
    /// 특정 이름의 이벤트 구독을 해제한다.
    /// </summary>
    /// <param name="eventName">이벤트 이름</param>
    /// <param name="listener">해제할 콜백 함수</param>
    public static void Unsubscribe(string eventName, Action listener)
    {
        if (!eventTable.ContainsKey(eventName)) return;

        eventTable[eventName] -= listener;

        // 더 이상 구독자가 없다면 해당 키 제거
        if (eventTable[eventName] == null)
            eventTable.Remove(eventName);
    }

    /// <summary>
    /// 이벤트를 발생시켜 모든 구독자에게 알린다.
    /// </summary>
    /// <param name="eventName">발행할 이벤트 이름</param>
    public static void Publish(string eventName)
    {
        if (eventTable.TryGetValue(eventName, out var callback))
            callback?.Invoke(); // null 체크 후 호출
    }
}