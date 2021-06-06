using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Editor;
using UnityEditor;

#if UNITY_EDITOR
public class ChangeStickNorthProcessorEditor : InputParameterEditor<ChangeStickNorthProcessor>
{
    private GUIContent m_DropdownLabel = new GUIContent("Stick North");
    private string[] options = { "Default", "Up-Right", "Right", "Down-Right", "Down", "Down-Left", "Left", "Up-Left" };

    // TODO: MAKE THE GUI A TEXT BOX THAT YOU CAN PUT DEGREES IN
    public override void OnGUI()
    {
        target.num45ToRight = EditorGUILayout.Popup(target.num45ToRight, options);
    }

}
#endif
