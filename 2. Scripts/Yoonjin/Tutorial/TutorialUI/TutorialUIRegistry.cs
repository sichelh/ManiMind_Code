using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUIRegistry : MonoBehaviour
{
    public static Dictionary<string, Button> buttonMap = new();

    public static void Register(string name, Button btn)
    {
        if(!buttonMap.ContainsKey(name))
        {
            buttonMap[name] = btn;
        }
    }

    public static Button Get(string name)
    {
        buttonMap.TryGetValue(name, out var btn);
        return btn;
    }

    public static void Clear()
    {
        buttonMap.Clear();
    }
}
