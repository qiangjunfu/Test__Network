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
        GUI.enabled = false; // �����ֶΣ�ʹ�䲻�ɱ༭
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true; // ��������GUI��Ӱ�������Ԫ��
    }
}
#endif


public class ReadOnlyAttribute : PropertyAttribute
{

}
