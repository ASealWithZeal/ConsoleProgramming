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
            if (CheckCanExportData())
            {
                ExportEditorData();
                ExportNormalData();
            }
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
    Rect childRect = new Rect(0, 0, 325.0f, 125.0f);
    Rect selectingRect = new Rect(0, 0, 350.0f, 110.0f);
    Vector2 firstValuePosition = new Vector2(2.5f, 32.5f);
    Vector2 childValuePosition = new Vector2(22.5f, 32.5f);
    private bool keyPressed = false;

    /// <summary>
    /// Draw all values as a series of vertical rectangles
    /// </summary>
    private void DrawRects()
    {
        posRect.position = firstValuePosition;
        childRect.position = childValuePosition;
        Event current = Event.current;

        //GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        for (int i = 0; i < varList.Count; ++i)
        {
            if (i > 0)
            {
                // If the above rect is NOT childed, use these spacings:
                if (varList[i - 1].parent == null)
                {
                    posRect.y += 110;
                    childRect.y += 110;
                }

                // If the above rect IS childed, use these spacings, instead:
                else
                {
                    posRect.y += 130;
                    childRect.y += 130;
                }
            }

            float spaceOffset = 0.0f;
            // Draw a normal rect
            if (varList[i].parent == null)
            {
                if (posRect.y > windowSize.y - posRect.height || posRect.y < 0)
                    continue;
                if (varList[i].selected)
                {
                    selectingRect.x = posRect.x - 2.5f;
                    selectingRect.y = posRect.y - 2.5f;
                    selectingRect.width = posRect.width + 5;
                    selectingRect.height = posRect.height + 5;
                    EditorGUI.DrawRect(selectingRect, Color.black);
                }
                EditorGUI.DrawRect(posRect, Color.green);
                spaceOffset = 5.0f;
            }

            // Draw a child rect
            else
            {
                if (childRect.y > windowSize.y - childRect.height || childRect.y < 0)
                    continue;
                if (varList[i].selected)
                {
                    selectingRect.x = childRect.x - 2.5f;
                    selectingRect.y = childRect.y - 2.5f;
                    selectingRect.width = childRect.width + 5;
                    selectingRect.height = childRect.height + 5;
                    EditorGUI.DrawRect(selectingRect, Color.black);
                }
                EditorGUI.DrawRect(childRect, Color.green);
                spaceOffset = 25.0f;
            }
            
            // CONDITIONAL DATA

            DisplayGeneralData(i, spaceOffset);
            switch (varList[i].type)
            {
                case VariableList.Bool:
                    CreateBoolData(i, spaceOffset);
                    varList[i].minMax = false;
                    GUILayout.Space(space);
                    break;
                case VariableList.Int:
                    CreateIntData(i, spaceOffset);
                    CheckMinMax(i, MinMaxType.Int, spaceOffset);
                    break;
                case VariableList.Float:
                    CreateFloatData(i, spaceOffset);
                    CheckMinMax(i, MinMaxType.Float, spaceOffset);
                    break;
                case VariableList.Vector2:
                    CreateVec2Data(i, spaceOffset);
                    varList[i].minMax = false;
                    break;
                case VariableList.Vector3:
                    CreateVec3Data(i, spaceOffset);
                    varList[i].minMax = false;
                    break;
                case VariableList.String:
                    CreateStringData(i, spaceOffset);
                    varList[i].minMax = false;
                    GUILayout.Space(space);
                    break;
                default:
                    GUILayout.Space(space * 2);
                    break;
            }

            if (varList[i].parent != null)
                GUILayout.Space(space * 1f);
            if (varList[i].selected)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(spaceOffset);
                if (GUILayout.Button("Destroy This Variable", GUILayout.Width(windowSize.x - (spaceOffset + 7.5f))))
                    varList.Remove(varList[i]);
                EditorGUILayout.EndHorizontal();
            }
            else
                GUILayout.Space(space);

            UserEvents(current, i);
            GUILayout.Space(space * 0.5f);
        }
    }

    private void UserEvents(Event current, int i)
    {
        if (posRect.Contains(current.mousePosition) && current.type == EventType.MouseDown)
        {
            for (int j = 0; j < varList.Count; ++j)
                varList[j].selected = false;
            varList[i].selected = true;
        }

        // Keyboard events
        if (varList[i].selected && current.type == EventType.KeyDown && !keyPressed)
        {
            keyPressed = true;

            // Swap the selected block with the one below it
            if (current.keyCode == KeyCode.DownArrow && i < varList.Count)
            {
                CustomInspectorWindowBlock temp = new CustomInspectorWindowBlock();
                temp = varList[i + 1];
                varList[i + 1] = varList[i];
                varList[i] = temp;

                Debug.Log(i);
            }

            // Swap the selected block with the one above it
            else if (current.keyCode == KeyCode.UpArrow && i > 0)
            {
                CustomInspectorWindowBlock temp = new CustomInspectorWindowBlock();
                temp = varList[i - 1];
                varList[i - 1] = varList[i];
                varList[i] = temp;
            }

            // Child the selected block to the object above it, if possible
            else if (current.keyCode == KeyCode.RightArrow && i > 0)
            {
                varList[i].childIncrements = 1;
                if (varList[i - 1].childIncrements < 1)
                    varList[i].parent = varList[i - 1];
                else
                    varList[i].parent = varList[i - 2];
            }

            // Remove the selected block from its parent
            else if (current.keyCode == KeyCode.LeftArrow && i > 0)
            {
                varList[i].childIncrements = 0;
                varList[i].parent = null;
            }
        }
        if (current.type == EventType.KeyUp && keyPressed)
            keyPressed = false;
    }

    private void DisplayGeneralData(int i, float spaceOffset)
    {
        // Name
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(spaceOffset);
        varList[i].theName = EditorGUILayout.TextField("Variable Name", varList[i].theName);
        EditorGUILayout.EndHorizontal();

        // Variable type
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(spaceOffset);
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

    private void CreateBoolData(int i, float spaceOffset)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(spaceOffset);
        GUILayout.Label("Default");
        GUILayout.Space(15.0f);
        varList[i].theBool = EditorGUILayout.Toggle(varList[i].theBool);
        EditorGUILayout.EndHorizontal();
    }

    private void CreateIntData(int i, float spaceOffset)
    {
        // Default value
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(spaceOffset);
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

    private void CreateFloatData(int i, float spaceOffset)
    {
        // Default value
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(spaceOffset);
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

    private void CreateVec2Data(int i, float spaceOffset)
    {
        // Default value
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(2f + spaceOffset);
        varList[i].theVec2 = EditorGUILayout.Vector2Field("Default", varList[i].theVec2);
        EditorGUILayout.EndHorizontal();
    }

    private void CreateVec3Data(int i, float spaceOffset)
    {
        // Default value
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(2f + spaceOffset);
        varList[i].theVec3 = EditorGUILayout.Vector3Field("Default", varList[i].theVec3);
        EditorGUILayout.EndHorizontal();
    }

    private void CreateStringData(int i, float spaceOffset)
    {
        // Default value
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(2f + spaceOffset);

        varList[i].theString = EditorGUILayout.TextField("Default", varList[i].theString);
        EditorGUILayout.EndHorizontal();
    }

    private void CheckMinMax(int i, MinMaxType type, float spaceOffset)
    {
        if (varList[i].minMax)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(spaceOffset);

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

    // --------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Region that exports data for the "editor" script
    /// </summary>
    #region
    private void ExportEditorData()
    {
        scriptName = scriptName.Replace(" ", string.Empty);
        string s = null;
        s += s += "using System.Collections;\nusing System.Collections.Generic;\nusing UnityEngine;\nusing UnityEditor;\n\n[CustomEditor(typeof(" + scriptName + "))]\n";
        s += "public class " + scriptName + "Editor : Editor\n{\n\tpublic override void OnInspectorGUI()\n\t{\n\t\t" + scriptName + " Instance = (" + scriptName + ")target;\n"; 

        for (int i = 0; i < varList.Count; ++i)
        {
            if (varList[i].type != VariableList.String && varList[i].type != VariableList.Bool)
                s += GetNumFieldString(i);
            else if (varList[i].type == VariableList.String)
                s += GetCharFieldString(i);
            else if (varList[i].type == VariableList.Bool)
                s += GetBoolFieldString(i);
        }

        s += "\t}\n}";

        string path = Application.dataPath + "\\Editor\\" + scriptName + "Editor.cs";

        if (!Directory.Exists(Application.dataPath + "\\Editor"))
            Directory.CreateDirectory(Application.dataPath + "\\Editor");

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
        string n = varList[i].theName.Replace(" ", string.Empty);

        // String used for getter/setter functions
        string val = varList[i].theName[0].ToString().ToUpper();
        for (int j = 1; j < varList[i].theName.Length; ++j)
            val += varList[i].theName[j];
        val = val.Replace(" ", string.Empty);
        
        // Exports text based on whether the variable is public, private/protected, and includes a slider
        if ((varList[i].protection == Protection.Private || varList[i].protection == Protection.Protected) && !varList[i].minMax)
            s += "\t\tInstance.Set" + val + "(EditorGUILayout." + varList[i].type.ToString() + "Field(\"" + varList[i].theName + "\", Instance.Get" + val + "()));\n";
        else if (!varList[i].minMax)
            s += "\t\tInstance." + n + " = EditorGUILayout." + varList[i].type.ToString() + "Field(\"" + varList[i].theName + "\", Instance." + n + ");\n";
        else if ((varList[i].protection == Protection.Private || varList[i].protection == Protection.Protected) && varList[i].minMax)
            s += "\t\tInstance.Set" + val + "((" + varList[i].type.ToString().ToLower() + ")EditorGUILayout.Slider(\"" + varList[i].theName + "\", Instance.Get" + val + "(), " + varList[i].min + "f, " + varList[i].max + "f));\n";
        else if (varList[i].minMax)
            s += "\t\tInstance." + n + " = (" + varList[i].type.ToString().ToLower() + ")EditorGUILayout.Slider(\"" + varList[i].theName + "\", Instance." + n + ", " + varList[i].min + "f, " + varList[i].max + "f);\n";

        return s;
    }

    private string GetCharFieldString(int i)
    {
        string s = null;
        string n = varList[i].theName.Replace(" ", string.Empty);

        // String used for getter/setter functions
        string val = varList[i].theName[0].ToString().ToUpper();
        for (int j = 1; j < varList[i].theName.Length; ++j)
            val += varList[i].theName[j];
        val = val.Replace(" ", string.Empty);

        // Exports text based on whether the variable is public or private/protected
        if (varList[i].protection == Protection.Private || varList[i].protection == Protection.Protected)
            s += "\t\tInstance.Set" + val + "(EditorGUILayout.TextField(\"" + varList[i].theName + "\", Instance.Get" + val + "()));\n";
        else
            s += "\t\tInstance." + n + " = EditorGUILayout.TextField(\"" + varList[i].theName + "\", Instance." + n + ");\n";

        return s;
    }

    private string GetBoolFieldString(int i)
    {
        string s = null;
        string n = varList[i].theName.Replace(" ", string.Empty);

        // String used for getter/setter functions
        string val = varList[i].theName[0].ToString().ToUpper();
        for (int j = 1; j < varList[i].theName.Length; ++j)
            val += varList[i].theName[j];
        val = val.Replace(" ", string.Empty);

        // Exports text based on whether the variable is public or private/protected
        if (varList[i].protection == Protection.Private || varList[i].protection == Protection.Protected)
            s += "\t\tInstance.Set" + val + "(EditorGUILayout.Toggle(\"" + varList[i].theName + "\", Instance.Get" + val + "()));\n";
        else
            s += "\t\tInstance." + n + " = EditorGUILayout.Toggle(\"" + varList[i].theName + "\", Instance." + n + ");\n";

        return s;
    }
    #endregion

    // --------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Region that exports data for the "normal" script, if applicable
    /// </summary>
    #region
    /// <summary>
    /// Exports a "normal" script containing the below information
    /// </summary>
    private void ExportNormalData()
    {
        scriptName = scriptName.Replace(" ", string.Empty);
        string s = null;
        s += "using System.Collections;\nusing System.Collections.Generic;\nusing UnityEngine;\n\n";
        s += "public class " + scriptName + " : MonoBehaviour\n{\n";
        
        // Goes through each variable one-by-one
        for (int i = 0; i < varList.Count; ++i)
        {
            // Checks if the type needs to be made lowercase (int, float, etc.) or left alone (Vector2, Transform, etc.)
            string t = varList[i].type.ToString();
            if (varList[i].type != VariableList.Vector2 && varList[i].type != VariableList.Vector3)
                t = t.ToLower();

            // Outputs the basic variable information
            if (varList[i].minMax)
                s += "\t[Range(" + varList[i].min + ", " + varList[i].max + ")]\n";
            s += "\t" + varList[i].protection.ToString().ToLower() + " " + t + " " + varList[i].theName.Replace(" ", string.Empty) + " = " + varList[i].GetTypeValue() + "; ";

            // If the variable is Private or Protected, add a getter and a setter
            if (varList[i].protection == Protection.Private || varList[i].protection == Protection.Protected)
            {
                string varFunctionName = varList[i].theName[0].ToString().ToUpper();
                for (int j = 1; j < varList[i].theName.Length; ++j)
                    varFunctionName += varList[i].theName[j];

                s += "public " + t + " Get" + varFunctionName.Replace(" ", string.Empty) + "() { return " + varList[i].theName.Replace(" ", string.Empty) + "; } ";
                s += "public void Set" + varFunctionName.Replace(" ", string.Empty) + "(" + t + " val) { " + varList[i].theName.Replace(" ", string.Empty) + " = " + "val; }";
            }
            s += "\n";
        }

        s += "}";

        // Outputs the file, if possible
        string path = Application.dataPath + "\\" + scriptName + ".cs";

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
    #endregion

    // --------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Goes through every variable in each variable to ensure they are filled and valid before exporting any data
    /// </summary>
    #region
    private bool CheckCanExportData()
    {
        bool @bool = true;

        // Checks if the script name exists
        if (scriptName == null)
        {
            Debug.LogError("Script name not set!");
            @bool = false;
        }

        // Checks if the first char in the script's name is valid
        else if (!CheckScriptFirstChar(scriptName[0]))
        {
            Debug.LogError("Invalid first character " + scriptName[0] + " in script name!");
            @bool = false;
        }

        // Checks each other char in the script's name
        for (int i = 0; i < scriptName.Length; ++i)
        {
            if (!CheckValidBodyChars(scriptName[i]))
            {
                Debug.LogError("Script name contains invalid character " + scriptName[i] +"!");
                @bool = false;
            }
        }

        // Checks information for each variable to determine validity
        for (int i = 0; i < varList.Count; ++i)
        {
            string s = null;
            if (varList[i].theName == null)
                s += "Name of variable " + i + " not set!\n";
            else if (!CheckVarFirstChar(varList[i].theName[0]))
                s += "Invalid first character " + varList[i].theName[0] + " in name of variable " + i + "!\n";
            for (int j = 0; j < varList[i].theName.Length; ++j)
                if (!CheckValidBodyChars(varList[i].theName[j]))
                    s += "Name of variable " + i + " contains invalid character " + varList[i].theName[j] + "!\n";

            if (varList[i].type == 0)
                s += "Type of variable " + i + " not set!\n";

            if (varList[i].protection == 0)
                s += "Protection level of variable " + i + " not set!\n";

            if (varList[i].GetTypeValue() == null)
                s += "Type of variable " + i + " not set!\n";

            if (s != null)
            {
                Debug.LogError("Variable " + i + "Errors:\n" + s);
                @bool = false;
            }
        }

        return @bool;
    }

    // Checks the first character in the script to determine its validity
    private bool CheckScriptFirstChar(char c)
    {
        string[] alphabet = new string[27]
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "_"
        };

        for (int i = 0; i < alphabet.Length; ++i)
            if (c.ToString() == alphabet[i])
                return true;

        return false;
    }

    // Checks the first character in the script to determine its validity
    private bool CheckVarFirstChar(char c)
    {
        string[] alphabet = new string[53]
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
            "_"
        };

        for (int i = 0; i < alphabet.Length; ++i)
            if (c.ToString() == alphabet[i])
                return true;

        return false;
    }

    // Checks the first character in the script to determine its validity
    private bool CheckValidBodyChars(char c)
    {
        string[] alphabet = new string[64]
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
            "_", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", " "  // Spaces are legal characters to allow for more readable variable names
        };

        for (int i = 0; i < alphabet.Length; ++i)
            if (c.ToString() == alphabet[i])
                return true;

        return false;
    }
    #endregion
}
