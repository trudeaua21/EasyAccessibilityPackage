using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


/// <summary>
/// Simple custom editor for the <see cref="ScreenReaderOutput"/> component.
/// Will hide the text input for the "Output Text" field if the checkbox to 
/// use the text from the object's <see cref="Text"/> component is checked.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(ScreenReaderOutput))]
public class ScreenReaderOutputEditor : Editor
{
    SerializedProperty outputText;
    SerializedProperty readFromTextObject;
    SerializedProperty readOnHover;

    void OnEnable()
    {
        readOnHover = serializedObject.FindProperty("readOnHover");
        readFromTextObject = serializedObject.FindProperty("readFromTextObject");
        outputText = serializedObject.FindProperty("outputText");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // hide the text input if we are getting text from the attached Text component
        EditorGUILayout.PropertyField(readOnHover);
        EditorGUILayout.PropertyField(readFromTextObject);
        if (!readFromTextObject.boolValue)
        {
            EditorGUILayout.PropertyField(outputText);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
