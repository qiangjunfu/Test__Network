using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
//#endif
using UnityEditor;


[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false; // 禁用字段，使其不可编辑
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true; // 重新启用GUI，影响后续的元素
    }
}
#endif


public class ReadOnlyAttribute : PropertyAttribute
{

}
