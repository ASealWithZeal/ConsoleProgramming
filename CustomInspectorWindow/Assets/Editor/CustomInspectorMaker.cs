using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum MinMaxType
{
    Int = 0,
    Float
}

public class CustomInspectorMaker : EditorWindow
{
    List<CustomInspectorWindowBlock> varList = new List<CustomInspectorWindowBlock>();

    [MenuItem("Tools/Custom Inspector Maker")]
    public static void ShowWindow()
    {
        CustomInspectorMaker window = (CustomInspectorMaker)EditorWindow.GetWindow(typeof(CustomInspectorMaker));
        window.Show();
    }

    void OnGUI()
    {
        //GUILayout.Label("Base Settings", EditorStyles.boldLabel);

        for (int i = 0; i < varList.Count; ++i)
        {
            //GUILayout.BeginArea(new Rect(100, 10 + (100 * i), position.width, 200));
            DisplayGeneralData(i);

            if (varList[i].type == VariableList.Bool)
                CreateBoolData(i);
            else if (varList[i].type == VariableList.Int)
            {
                CreateIntData(i);
                CheckMinMax(i, MinMaxType.Int);
            }
            else if (varList[i].type == VariableList.Float)
            {
                CreateFloatData(i);
                CheckMinMax(i, MinMaxType.Float);
            }

            GUILayout.Space(25.0f);
            //GUILayout.EndArea();
        }

        if (GUILayout.Button("This ButtoN"))
        {
            CustomInspectorWindowBlock block = new CustomInspectorWindowBlock();
            varList.Add(block);
        }
    }

    private void DisplayGeneralData(int i)
    {
        // Name
        GUILayout.Label("Variable Name");
        //varList[i].name = (VariableList)EditorGUILayout.StringField(varList[i].name);

        // Variable type
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Variable");
        GUILayout.Space(10.0f);
        varList[i].type = (VariableList)EditorGUILayout.EnumPopup(varList[i].type);

        // Protection level
        GUILayout.Label("Protection");
        GUILayout.Space(10.0f);
        varList[i].protection = (Protection)EditorGUILayout.EnumPopup(varList[i].protection);
        EditorGUILayout.EndHorizontal();
    }

    private void CreateBoolData(int i)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Default");
        GUILayout.Space(10.0f);
        varList[i].theBool = EditorGUILayout.Toggle(varList[i].theBool);
        EditorGUILayout.EndHorizontal();
    }

    private void CreateIntData(int i)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Default");
        varList[i].theInt = EditorGUILayout.IntField(varList[i].theInt);
        GUILayout.Label("Min/Max");
        varList[i].minMax = EditorGUILayout.Toggle(varList[i].minMax);
        EditorGUILayout.EndHorizontal();
    }

    private void CreateFloatData(int i)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Default");
        varList[i].theFloat = EditorGUILayout.FloatField(varList[i].theFloat);
        GUILayout.Label("Min/Max");
        varList[i].minMax = EditorGUILayout.Toggle(varList[i].minMax);
        EditorGUILayout.EndHorizontal();
    }

    private void CheckMinMax(int i, MinMaxType type)
    {
        if (varList[i].minMax)
        {
            EditorGUILayout.BeginHorizontal();

            // Int Min/Max
            if (type == MinMaxType.Int)
            {
                GUILayout.Label("Minimum");
                varList[i].min = EditorGUILayout.IntField((int)varList[i].min);
                GUILayout.Label("Maximum");
                varList[i].max = EditorGUILayout.IntField((int)varList[i].max);
            }

            // Float Min/Max
            else if (type == MinMaxType.Float)
            {
                GUILayout.Label("Minimum");
                varList[i].min = EditorGUILayout.FloatField(varList[i].min);
                GUILayout.Label("Maximum");
                varList[i].max = EditorGUILayout.FloatField(varList[i].max);
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
