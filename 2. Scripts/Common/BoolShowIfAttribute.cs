using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class ShowIfTrueAttribute : PropertyAttribute
{
    public string ConditionFieldName;

    public ShowIfTrueAttribute(string conditionFieldName)
    {
        ConditionFieldName = conditionFieldName;
    }
}

public class ShowIfFalseAttribute : PropertyAttribute
{
    public string ConditionFieldName;

    public ShowIfFalseAttribute(string conditionFieldName)
    {
        ConditionFieldName = conditionFieldName;
    }
}


[CustomPropertyDrawer(typeof(ShowIfTrueAttribute))]
[CustomPropertyDrawer(typeof(ShowIfFalseAttribute))]
public class ShowIfDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (ShouldShow(property))
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        return 0f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (ShouldShow(property))
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    private bool ShouldShow(SerializedProperty property)
    {
        PropertyAttribute attr = attribute;
        string conditionFieldName = attr is ShowIfTrueAttribute showIfTrue
            ? showIfTrue.ConditionFieldName
            : ((ShowIfFalseAttribute)attr).ConditionFieldName;

        SerializedProperty conditionProperty = property.serializedObject.FindProperty(conditionFieldName);

        if (conditionProperty != null && conditionProperty.propertyType == SerializedPropertyType.Boolean)
        {
            bool value = conditionProperty.boolValue;
            return attr is ShowIfTrueAttribute ? value : !value;
        }

        return true; // 조건 못 찾으면 기본 표시
    }
}
#endif