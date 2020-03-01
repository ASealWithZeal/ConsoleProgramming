using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public enum MinMaxType
{
    None = -1,
    Int = 0,
    Float
}

public class CustomInspectorMaker : EditorWindow
{
    private List<CustomInspectorWindowBlock> varList = new List<CustomInspectorWindowBlock>();
    private string scriptName = null;
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
        DrawStart();
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
            ExportEditorData();
            ExportNormalData();
        }
    }

    private void DrawStart()
    {
        GUILayout.Space(space * 0.25f);
        scriptName = EditorGUILayout.TextField("Script Name", scriptName);
        EditorGUI.DrawRect(new Rect(0, 30.0f, 350, 1), Color.black);
        GUILayout.Space(space * 0.5f);
    }

    /// <summary>
    /// Region containing data to draw each rectangle with its relevant data
    /// </summary>
    #region
    Rect posRect = new Rect(0, 0, 345.0f, 105.0f);
    Rect selectingRect = new Rect(0, 0, 350.0f, 110.0f);
    Vector2 firstValuePosition = new Vector2(2.5f, 32.5f);

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
                posRect.y += 110;
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
                    varList[i].minMax = false;
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
                    varList[i].minMax = false;
                    break;
                case VariableList.Vector3:
                    CreateVec3Data(i);
                    varList[i].minMax = false;
                    break;
                case VariableList.String:
                    CreateStringData(i);
                    varList[i].minMax = false;
                    GUILayout.Space(space);
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

            GUILayout.Space(space * 0.5f);
        }
    }

    private void DisplayGeneralData(int i)
    {
        // Name
        varList[i].theName = EditorGUILayout.TextField("Variable Name", varList[i].theName);

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

    private void CreateStringData(int i)
    {
        // Default value
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(5.0f);

        varList[i].theString = EditorGUILayout.TextField("Default", varList[i].theString);
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


    /// <summary>
    /// Region that exports data for the "editor" script
    /// </summary>
    #region
    private void ExportEditorData()
    {
        scriptName = scriptName.Replace(" ", string.Empty);
        string s = null;
        s += s += "using System.Collections;\nusing System.Collections.Generic;\nusing UnityEngine;\nusing UnityEditor;\n\n[CustomEditor(typeof(" + scriptName + "))]\n";
        s += "public class " + scriptName + "Editor : Editor\n{\n\tSerializedProperty " + scriptName + ";\n\n\tprivate void OnEnable()\n\t{\n\t\t" + scriptName + " = serializedObject.FindProperty(" + scriptName + ");\n\t}\n\n";
        s += "\tpublic override void OnInspectorGUI()\n\t{\n"; 

        for (int i = 0; i < varList.Count; ++i)
        {
            if (varList[i].type != VariableList.None && varList[i].type != VariableList.String)
                s += GetNumFieldString(i);
            else if (varList[i].type == VariableList.String)
                s += GetCharFieldString(i);
        }

        s += "\t}\n}";

        string path = scriptName + "_Editor.txt";
        
        if (File.Exists(path))
        {
            Debug.Log(path + " already exists.");
            return;
        }
        var sr = File.CreateText(path);
        sr.WriteLine(s);
        sr.Close();

        //Print the text from the file
        Debug.Log(s);
    }

    private string GetNumFieldString(int i)
    {
        string s = null;

        varList[i].theName = varList[i].theName.Replace(" ", string.Empty);
        s += "\t\tEditorGUILayout." + varList[i].type.ToString() + "Field(\"" + varList[i].theName + "\", " + scriptName + "." + varList[i].theName + ");\n";

        return s;
    }

    private string GetCharFieldString(int i)
    {
        string s = null;

        varList[i].theName = varList[i].theName.Replace(" ", string.Empty);
        varList[i].theString = varList[i].theString.Replace(" ", string.Empty);
        s += "\t\tEditorGUILayout.TextField(\"" + varList[i].theName + "\", " + scriptName + "." + varList[i].theName + ");\n";

        return s;
    }
    #endregion

    private void ExportNormalData()
    {
        scriptName = scriptName.Replace(" ", string.Empty);
        string s = null;
        s += "using System.Collections;\nusing System.Collections.Generic;\nusing UnityEngine;\n\n";
        s += "public class " + scriptName + " : MonoBehaviour\n{\n\t";
        
        for (int i = 0; i < varList.Count; ++i)
        {
            if (varList[i].protection > 0 && varList[i].type > 0 && varList[i].theName != null)
            {
                if (varList[i].minMax)
                    s += "[Range(" + varList[i].min + ", " + varList[i].max + ")]\n\t";
                s += varList[i].protection.ToString().ToLower() + " " + varList[i].type.ToString().ToLower() + " " + varList[i].theName.Replace(" ", string.Empty) + " = " + varList[i].GetTypeValue() + "; ";
                if (varList[i].protection == Protection.Private || varList[i].protection == Protection.Protected)
                {
                    s += "public " + varList[i].type.ToString().ToLower() + " Get" + varList[i].theName.Replace(" ", string.Empty) + "() { return " + varList[i].theName.Replace(" ", string.Empty) + "; } ";
                    s += "public void Set" + varList[i].theName.Replace(" ", string.Empty) + "(" + varList[i].type.ToString().ToLower() + " val) { " + varList[i].theName.Replace(" ", string.Empty) + " = " + "val; }";
                }
                s += "\n\t";
            }

            else
                Debug.LogError("Missing information in variable " + i + "!");
        }

        s += "\n}";

        string path = scriptName + ".txt";

        if (File.Exists(path))
        {
            Debug.Log(path + " already exists.");
            return;
        }
        var sr = File.CreateText(path);
        sr.WriteLine(s);
        sr.Close();

        //Print the text from the file
        Debug.Log(s);
    }
}
