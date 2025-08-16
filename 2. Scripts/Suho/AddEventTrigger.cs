#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEditor.Events;
using System.Collections.Generic;

public class AddEventTrigger : EditorWindow
{
    [MenuItem("Tools/Add Event Trigger to Buttons")]
    public static void AddEventTriggerToButtons()
    {
        // 👇 비활성화된 오브젝트까지 포함
        List<Button> buttons = new List<Button>(Object.FindObjectsOfType<Button>(true));

        foreach (Button button in buttons)
        {
            GameObject go = button.gameObject;

            // AudioManager에서 ButtonSound 가져오기
            GameObject audioManager = AudioManager.Instance.gameObject;
            ButtonSound buttonSound = audioManager.GetComponent<ButtonSound>();
            if (buttonSound == null)
            {
                Debug.LogWarning($"ButtonSound 컴포넌트가 AudioManager에 없습니다. 스킵됨.");
                continue;
            }

            // EventTrigger 추가
            EventTrigger eventTrigger = go.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = Undo.AddComponent<EventTrigger>(go);
                Undo.RegisterCreatedObjectUndo(eventTrigger, "Add EventTrigger");
            }

            // 중복 방지
            bool alreadyExists = false;
            foreach (var entry in eventTrigger.triggers)
            {
                if (entry.eventID == EventTriggerType.PointerEnter)
                {
                    alreadyExists = true;
                    break;
                }
            }

            if (!alreadyExists)
            {
                EventTrigger.Entry entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerEnter,
                    callback = new EventTrigger.TriggerEvent()
                };

                // ButtonSound.PlayHoverSound 등록
                UnityEventTools.AddPersistentListener(entry.callback, buttonSound.PlayHoverSound);

                eventTrigger.triggers.Add(entry);

                EditorUtility.SetDirty(eventTrigger);
                Undo.RecordObject(eventTrigger, "Add PointerEnter Event");
            }
        }

        Debug.Log("비활성화 포함 모든 버튼에 Hover 사운드 이벤트 등록 완료");
    }
}
#endif
