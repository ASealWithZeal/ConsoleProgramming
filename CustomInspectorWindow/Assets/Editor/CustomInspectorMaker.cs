using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum VariableList
{
    Bool = 0,
    Int,
    Float,
    Vector2,
    Vector3,
    Char,
    String,
    GameObject,
    Transform,
}

public class CustomInspectorMaker : EditorWindow
{
    List<VariableList> theList = new List<VariableList>();
    List<CustomInspectorWindowBlock> varList = new List<CustomInspectorWindowBlock>();

    [MenuItem("Tools/Custom Inspector Maker")]
    public static void ShowWindow()
    {
        CustomInspectorMaker window = (CustomInspectorMaker)EditorWindow.GetWindow(typeof(CustomInspectorMaker));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);

        for (int i = 0; i < varList.Count; ++i)
        {
            theList[i] = (VariableList)EditorGUILayout.EnumPopup("Variable", theList[i]);
            if (theList[i] == VariableList.Bool)
                varList[i].theBool = EditorGUILayout.Toggle("Toggle", varList[i].theBool);
            if (theList[i] == VariableList.Int)
                varList[i].theInt = EditorGUILayout.IntField("Toggle", varList[i].theInt);
        }

        if (GUILayout.Button("This ButtoN"))
        {
            CustomInspectorWindowBlock block = new CustomInspectorWindowBlock();
            varList.Add(block);

            theList.Add(new VariableList());
        }
    }
}
