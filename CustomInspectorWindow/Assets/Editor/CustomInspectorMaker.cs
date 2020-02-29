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
    private List<CustomInspectorWindowBlock> varList = new List<CustomInspectorWindowBlock>();
    private float space = 20.0f;
    private static Vector2 windowSize = new Vector2(350, 700);

    [MenuItem("Tools/Custom Inspector Maker")]
    public static void ShowWindow()
    {
        CustomInspectorMaker window = (CustomInspectorMaker)EditorWindow.GetWindow(typeof(CustomInspectorMaker));
        window.minSize = windowSize;
        window.maxSize = windowSize;
        window.Show();
    }
    void OnGUI()
    {
        DrawRects();

        GUILayout.FlexibleSpace();
        EditorGUI.DrawRect(new Rect(0, 657.5f, 350, 1), Color.black);
        if (GUILayout.Button("Create Variable"))
        {
            CustomInspectorWindowBlock block = new CustomInspectorWindowBlock();
            varList.Add(block);
        }

        if (GUILayout.Button("Export Data"))
        {
            //CustomInspectorWindowBlock block = new CustomInspectorWindowBlock();
            ExportData();
        }
    }

    /// <summary>
    /// Region containing data to draw each rectangle with its relevant data
    /// </summary>
    #region
    Rect posRect = new Rect(0, 0, 345.0f, 125.0f);
    Rect selectingRect = new Rect(0, 0, 350.0f, 130.0f);
    Vector2 firstValuePosition = new Vector2(2.5f, 2.5f);

    /// <summary>
    /// Draw all values as a series of vertical rectangles
    /// </summary>
    private void DrawRects()
    {
        posRect.position = firstValuePosition;
        Event current = Event.current;

        //GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        for (int i = 0; i < varList.Count; ++i)
        {
            if (i > 0)
                posRect.y += 130;
            if (posRect.y > windowSize.y - posRect.height || posRect.y < 0)
                continue;
            if (varList[i].selected)
            {
                selectingRect.x = posRect.x - 2.5f;
                selectingRect.y = posRect.y - 2.5f;
                EditorGUI.DrawRect(selectingRect, Color.black);
            }
            EditorGUI.DrawRect(posRect, Color.green);

            //GUILayout.Space(space * 0.25f);
            DisplayGeneralData(i);
            switch (varList[i].type)
            {
                case VariableList.Bool:
                    CreateBoolData(i);
                    GUILayout.Space(space);
                    break;
                case VariableList.Int:
                    CreateIntData(i);
                    CheckMinMax(i, MinMaxType.Int);
                    break;
                case VariableList.Float:
                    CreateFloatData(i);
                    CheckMinMax(i, MinMaxType.Float);
                    break;
                case VariableList.Vector2:
                    CreateVec2Data(i);
                    break;
                case VariableList.Vector3:
                    CreateVec3Data(i);
                    break;
                default:
                    GUILayout.Space(space * 2);
                    break;
            }

            if (varList[i].selected)
            {
                if (GUILayout.Button("Destroy This Variable"))
                    varList.Remove(varList[i]);
            }
            else
                GUILayout.Space(space * 1);

            if (posRect.Contains(current.mousePosition) && current.type == EventType.MouseDown)
            {
                for (int j = 0; j < varList.Count; ++j)
                    varList[j].selected = false;
                varList[i].selected = true;
            }

            GUILayout.Space(space * 0.625f);
            //GUILayout.EndArea();
        }
    }

    private void DisplayGeneralData(int i)
    {
        // Name
        GUILayout.Label("Var Name");
        varList[i].theName = EditorGUILayout.TextField(varList[i].theName);

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

            // Checks to make sure min and max values do not exceed logical boundaries
            if (varList[i].max < varList[i].min)
                varList[i].max = varList[i].min;
            else if (varList[i].min > varList[i].max)
                varList[i].min = varList[i].max;

            EditorGUILayout.EndHorizontal();
        }

        else
            GUILayout.Space(space);
    }
    #endregion

    private string ExportData()
    {
        string s = null;
        string scriptName = "PLACEHOLDER";
        s += "[CustomEditor(typeof(" + scriptName + "))]\n";
        s += "public class " + scriptName + "Editor : Editor\n{\n\tSerializedProperty " + scriptName + ";\n\n\tprivate void OnEnable()\n\t{\n\t\t" + scriptName + " = serializedObject.FindProperty(" + scriptName + ");\n\t}\n\n";
        s += "\tpublic override void OnInspectorGUI()\n\t{\n"; 

        for (int i = 0; i < varList.Count; ++i)
        {
            if (varList[i].type != VariableList.None && varList[i].type != VariableList.String)
                s += GetFieldString(i);
        }

        Debug.Log(s);
        return s;
    }

    private string GetFieldString(int i)
    {
        string s = null;
        string scriptName = "PLACEHOLDER";

        if (varList[i].theName != null)
            s += "\t\tEditorGUILayout." + varList[i].type.ToString() + "Field(\"" + varList[i].theName + "\", " + scriptName + "." + varList[i].theName + ");\n";
        else
            s += "\t\tEditorGUILayout." + varList[i].type.ToString() + "Field(" + varList[i].theInt + ");\n";

        return s;
    }
}
