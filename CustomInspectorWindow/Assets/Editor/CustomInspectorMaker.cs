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
    private float space = 20.0f;
    public float pull = 1.0f;

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
            //The rectangle is drawn in the Editor (when MyScript is attached) with the width depending on the value of the Slider
            EditorGUI.DrawRect(new Rect(2.5f, (2.5f + (110 * i)), Screen.width - 3.5f, 105.0f), Color.green);
            GUILayout.Space(space * 0.25f);
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
            else if (varList[i].type == VariableList.Vector2)
            {
                CreateVec2Data(i);
            }
            else if (varList[i].type == VariableList.Vector3)
            {
                CreateVec3Data(i);
            }

            else
            {
                GUILayout.Space(space * 2);
            }

            if (GUILayout.Button("Destroy This Variable"))
            {
                varList.Remove(varList[i]);
            }

            GUILayout.Space(space * 0.25f);
            //GUILayout.EndArea();
        }

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Create Variable"))
        {
            CustomInspectorWindowBlock block = new CustomInspectorWindowBlock();
            varList.Add(block);
        }

        if (GUILayout.Button("Export Data"))
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

        GUILayout.Space(25.0f);

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
        GUILayout.Space(15.0f);
        varList[i].theBool = EditorGUILayout.Toggle(varList[i].theBool);
        EditorGUILayout.EndHorizontal();
    }

    private void CreateIntData(int i)
    {
        // Default value
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Default");
        GUILayout.Space(15.0f);
        varList[i].theInt = EditorGUILayout.IntField(varList[i].theInt);

        GUILayout.Space(25.0f);

        // Min/Max toggle
        GUILayout.Label("Min/Max");
        GUILayout.Space(15.0f);
        varList[i].minMax = EditorGUILayout.Toggle(varList[i].minMax);
        EditorGUILayout.EndHorizontal();
    }

    private void CreateFloatData(int i)
    {
        // Default value
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Default");
        GUILayout.Space(15.0f);
        varList[i].theFloat = EditorGUILayout.FloatField(varList[i].theFloat);

        GUILayout.Space(25.0f);

        // Min/Max toggle
        GUILayout.Label("Min/Max");
        GUILayout.Space(15.0f);
        varList[i].minMax = EditorGUILayout.Toggle(varList[i].minMax);
        EditorGUILayout.EndHorizontal();
    }

    private void CreateVec2Data(int i)
    {
        // Default value
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(5.0f);
        varList[i].theVec2 = EditorGUILayout.Vector2Field("Default", varList[i].theVec2);
        EditorGUILayout.EndHorizontal();
    }

    private void CreateVec3Data(int i)
    {
        // Default value
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(5.0f);
        varList[i].theVec3 = EditorGUILayout.Vector3Field("Default", varList[i].theVec3);
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
                GUILayout.Space(5.0f);
                varList[i].min = EditorGUILayout.IntField((int)varList[i].min);

                GUILayout.Space(25.0f);

                GUILayout.Label("Maximum");
                varList[i].max = EditorGUILayout.IntField((int)varList[i].max);
            }

            // Float Min/Max
            else if (type == MinMaxType.Float)
            {
                GUILayout.Label("Minimum");
                GUILayout.Space(5.0f);
                varList[i].min = EditorGUILayout.FloatField(varList[i].min);

                GUILayout.Space(25.0f);

                GUILayout.Label("Maximum");
                varList[i].max = EditorGUILayout.FloatField(varList[i].max);
            }

            EditorGUILayout.EndHorizontal();
        }

        else
            GUILayout.Space(space);
    }
}
