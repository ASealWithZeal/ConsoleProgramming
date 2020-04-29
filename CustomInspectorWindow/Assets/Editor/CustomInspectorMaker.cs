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
    private static Vector2 windowSize = new Vector2(360, 700);

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
        DrawStart();
        DrawEnd();
    }

    private void DrawStart()
    {
        EditorGUI.DrawRect(new Rect(0, 0, 360, 30.0f), new Color(0.65f, 0.65f, 0.65f));
        scriptName = EditorGUI.TextField(new Rect(5, 5, 350, 20), "Script Name", scriptName);
        EditorGUI.DrawRect(new Rect(0, 30.0f, 360, 1), Color.black);
    }

    private void DrawEnd()
    {
        GUILayout.FlexibleSpace();
        EditorGUI.DrawRect(new Rect(0, 657.5f, 360, 50f), new Color(0.65f, 0.65f, 0.65f));
        EditorGUI.DrawRect(new Rect(0, 657.5f, 360, 1), Color.black);
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

    /// <summary>
    /// Region containing data to draw each rectangle with its relevant data
    /// </summary>
    #region
    Rect posRect = new Rect(0, 0, 345.0f, 85.0f);
    Rect childRect = new Rect(0, 0, 325.0f, 135.0f);
    Rect selectingRect = new Rect(0, 0, 347.5f, 110.0f);

    Vector2 firstValuePosition = new Vector2(2.5f, 32.5f);
    Vector2 childValuePosition = new Vector2(22.5f, 32.5f);

    // Scrollbar information
    Rect scrollBarPos = new Rect(347, 35, 16, 615);
    float lastRectY = 0;
    float lastChildRectY = 0;
    float scrollYPos = 0;

    // Colors used for each rect object
    Color[] rectColors =
    {
        new Color(0.95f, 0.95f, 0.95f, 1),  // Neutral  - Gray
        new Color(0.5f, 0.75f, 0.5f, 1),    // Bool     - Green
        new Color(0.75f, 0.75f, 1, 1),      // Int      - Blue
        new Color(1, 0.5f, 1, 1),           // Float    - Purple
        new Color(1, 1, 0.5f, 1),           // Vector2  - Yellow
        new Color(1, 1, 0.5f, 1),           // Vector3  - Yellow
        new Color(1, 0.5f, 0.5f, 1),        // String   - Red
    };
    
    // Draw all values as a series of stacked
    private void DrawRects()
    {
        scrollYPos = GUI.VerticalScrollbar(scrollBarPos, scrollYPos, 1, 1, varList.Count * 140);
        int childVars = 0;
        float spaceMult = 0;

        posRect.position = firstValuePosition;
        childRect.position = childValuePosition;
        Event current = Event.current;

        // Determine each rect's position based on the vertical scrollbar
        for (int i = 0; i < varList.Count; ++i)
        {
            float posMod = (firstValuePosition.y) + (50 * childVars);
            bool disableSection = false;

            // If the above rect is childed, use these spacings
            if (i > 0 && varList[i].parent != null)
            {
                childVars++;
                spaceMult = 2.5f;
                posRect.y = ((90 * i) + posMod) - scrollYPos;
                childRect.y = ((90 * i) + posMod) - scrollYPos;
            }

            // If the above rect is not childed, use these spacings, instead
            else
            {
                spaceMult = 0;
                posRect.y = ((90 * i) + posMod) - scrollYPos;
                childRect.y = ((90 * i) + posMod) - scrollYPos;
            }

            float spaceOffset = 0.0f;

            // Draw a normal rect
            if (varList[i].parent == null)
            {
                if (posRect.y > windowSize.y || posRect.y < -posRect.height)
                    continue;
                else if (posRect.y > windowSize.y - 120 || posRect.y < 25)
                    disableSection = true;
                if (varList[i].selected)
                {
                    selectingRect.x = posRect.x - 2.5f;
                    selectingRect.y = posRect.y - 2.5f;
                    selectingRect.width = posRect.width + 2.5f;
                    selectingRect.height = posRect.height + 5;
                    EditorGUI.DrawRect(selectingRect, Color.black);
                }
                EditorGUI.DrawRect(posRect, rectColors[(int)varList[i].type]);
                spaceOffset = 5.0f;
            }

            // Draw a child rect
            else
            {
                if (childRect.y > windowSize.y || childRect.y < -childRect.height)
                    continue;
                else if (childRect.y > windowSize.y - 120 || childRect.y < 25)
                    disableSection = true;
                if (varList[i].selected)
                {
                    selectingRect.x = childRect.x - 2.5f;
                    selectingRect.y = childRect.y - 2.5f;
                    selectingRect.width = childRect.width + 2.5f;
                    selectingRect.height = childRect.height + 5;
                    EditorGUI.DrawRect(selectingRect, Color.black);
                }
                EditorGUI.DrawRect(childRect, rectColors[(int)varList[i].type]);
                EditorGUI.DrawRect(new Rect(22.5f, childRect.y + (space * 2.25f), 325, 1), Color.black);
                spaceOffset = 25.0f;
            }

            EditorGUI.BeginDisabledGroup(disableSection);

            float offsetMod = ((spaceOffset * varList[i].childIncrements) / 3);
            if (varList[i].parent != null)
                DisplayConditionalData(i, 0, spaceOffset);

            DisplayGeneralData(i, space * (spaceMult + 0), spaceOffset, offsetMod);
            switch (varList[i].type)
            {
                case VariableList.Bool:
                    CreateBoolData(i, space * (spaceMult + 2), spaceOffset, offsetMod);
                    varList[i].minMax = false;
                    break;
                case VariableList.Int:
                    CreateIntData(i, space * (spaceMult + 2), spaceOffset, offsetMod);
                    CheckMinMax(i, space * (spaceMult + 3), MinMaxType.Int, spaceOffset, offsetMod);
                    break;
                case VariableList.Float:
                    CreateFloatData(i, space * (spaceMult + 2), spaceOffset, offsetMod);
                    CheckMinMax(i, space * (spaceMult + 3), MinMaxType.Float, spaceOffset, offsetMod);
                    break;
                case VariableList.Vector2:
                    CreateVec2Data(i, space * (spaceMult + 2), spaceOffset, offsetMod);
                    varList[i].minMax = false;
                    break;
                case VariableList.Vector3:
                    CreateVec3Data(i, space * (spaceMult + 2), spaceOffset, offsetMod);
                    varList[i].minMax = false;
                    break;
                case VariableList.String:
                    CreateStringData(i, space * (spaceMult + 2), spaceOffset, offsetMod);
                    varList[i].minMax = false;
                    break;
                default:
                    break;
            }

            UserEvents(current, i);
            EditorGUI.EndDisabledGroup();
        }
    }

    int selectedVar = -1;
    private void UserEvents(Event current, int i)
    {
        // Determines if the child's rect object contains the mouse
        if (((posRect.Contains(current.mousePosition) && varList[i].parent == null)
            || (childRect.Contains(current.mousePosition) && varList[i].parent != null))
            && current.type == EventType.ContextClick)
        {
            selectedVar = i;
            for (int j = 0; j < varList.Count; ++j)
                varList[j].selected = false;
            varList[i].selected = true;

            // Creates the menu
            GenericMenu menu = new GenericMenu();

            // Move Up
            if (i > 0 && (varList[i].parent == varList[i - 1].parent || varList[i].childIncrements <= varList[i - 1].childIncrements))
                menu.AddItem(new GUIContent("Move Up"), false, MoveBlockUp);
            else
                menu.AddDisabledItem(new GUIContent("Move Up"));

            // Move Down
            //  Checks if there are any eligible blocks below the current one
            bool canDraw = false;
            for (int j = selectedVar + 1; j < varList.Count; ++j)
                if (j < varList.Count && varList[j].childIncrements == varList[selectedVar].childIncrements)
                {
                    canDraw = true;
                    break;
                }

            //  Draws whether the variable can move or not
            if (canDraw && i < varList.Count - 1 && (varList[i].parent == varList[i + 1].parent || varList[i].childIncrements <= varList[i + 1].childIncrements))
                menu.AddItem(new GUIContent("Move Down"), false, MoveBlockDown);
            else
                menu.AddDisabledItem(new GUIContent("Move Down"));

            // Childs the Block
            if (i > 0 && varList[i].parent == null && (varList[i - 1].type != VariableList.None || varList[i - 1].parent != null))
                menu.AddItem(new GUIContent("Child this Variable"), false, ChildBlock);
            else
                menu.AddDisabledItem(new GUIContent("Child this Variable"));

            // Unchilds the Block
            if (i > 0 && varList[i].parent != null)
                menu.AddItem(new GUIContent("Unchild this Variable"), false, UnchildBlock);
            else
                menu.AddDisabledItem(new GUIContent("Unchild this Variable"));

            // Deletes the block
            menu.AddItem(new GUIContent("Delete Variable"), false, DeleteVariable);

            menu.ShowAsContext();
        }

        if (i > 0 && (varList[i].parent != null && varList[i].parent.type == VariableList.None))
            UnchildBlock(i);
    }

    // Prompts users to delete the chosen variable, then does so
    private void DeleteVariable()
    {
        bool confirmation = EditorUtility.DisplayDialog("Delete Variable?", "Deleting this variable will automatically unchild any of its child objects!\nThey will not be destroyed.", "Confirm", "Cancel");
        if (confirmation)
        {
            for (int i = selectedVar + 1; i < varList.Count; ++i)
            {
                if (varList[i].parent == varList[selectedVar])
                    UnchildBlock(i);
                else
                    break;
            }

            varList.Remove(varList[selectedVar]);
        }
    }

    // Moves the selected block up
    private void MoveBlockUp()
    {
        int movedInt = 0;
        int tempVar = selectedVar;
        CustomInspectorWindowBlock temp = new CustomInspectorWindowBlock();
        CustomInspectorWindowBlock thisBlock = varList[selectedVar];

        // Moves the object up so it passes any childed objects
        for (int i = selectedVar; i > 0; --i)
        {
            movedInt++;

            temp = varList[i - 1];
            varList[i - 1] = varList[i];
            varList[i] = temp;

            if (varList[i].childIncrements <= varList[i - 1].childIncrements)
                break;
        }

        // Moves any of the object's children up with it
        for (int i = 0; i < movedInt; ++i)
        {
            int val = tempVar - i;
            for (int j = val; j < thisBlock.children.Count + val; ++j)
            {
                if (j - 1 >= 0)
                {
                    temp = varList[j + 1];
                    varList[j + 1] = varList[j];
                    varList[j] = temp;
                }
            }
        }
    }

    // Moves the selected block down
    private void MoveBlockDown()
    {
        int movedInt = 0;
        int tempVar = selectedVar;
        CustomInspectorWindowBlock temp = new CustomInspectorWindowBlock();
        CustomInspectorWindowBlock thisBlock = varList[selectedVar];

        // Moves the object down so it passes any childed objects
        for (int i = selectedVar; i < varList.Count - 1; ++i)
        {
            bool end = true;
            movedInt++;

            temp = varList[i + 1];
            varList[i + 1] = varList[i];
            varList[i] = temp;
            
            if (i < varList.Count - 2 && varList[i + 2].parent != null && varList[i + 2].parent != varList[i + 1])
                end = false;
            if (i < varList.Count - 1 && varList[selectedVar].parent != null && varList[i + 1].parent == varList[selectedVar].parent)
                end = true;
            if (varList[i].childIncrements <= varList[i + 1].childIncrements && end)
                break;
        }

        // Adjusts the moved amount to account for the number of childed objects
        if (thisBlock.children.Count > 1)
            movedInt -= (thisBlock.children.Count - 1);

        // Moves any of the object's children down with it
        for (int i = 0; i < movedInt; ++i)
        {
            int val = tempVar + i;
            for (int j = (thisBlock.children.Count + val - 1); j >= val; --j)
            {
                if (j + 1 < varList.Count)
                {
                    temp = varList[j + 1];
                    varList[j + 1] = varList[j];
                    varList[j] = temp;
                }
            }
        }
    }

    // Childs the block to the one above it
    private void ChildBlock()
    {
        varList[selectedVar].childIncrements = 1;
        for (int i = selectedVar - 1; i >= 0; --i)
        {
            if (varList[i].childIncrements < varList[selectedVar].childIncrements)
            {
                varList[selectedVar].parent = varList[i];
                varList[selectedVar].parent.children.Add(varList[selectedVar]);
                break;
            }
        }

        for (int i = 0; i < varList[selectedVar].children.Count; ++i)
            UnchildBlock(varList[selectedVar].children[0]);
    }

    // Unchilds the block from the one above it
    private void UnchildBlock()
    {
        // If there are any child objects below the current block, move it down until this is not true
        for (int i = selectedVar + 1; i < varList.Count; ++i)
            if (i < varList.Count && varList[i].parent != null && varList[i].parent == varList[selectedVar].parent)
                UnchildBlock(i);

        varList[selectedVar].parent.children.Remove(varList[selectedVar]);
        varList[selectedVar].childIncrements = 0;
        varList[selectedVar].parent = null;
    }

    private void UnchildBlock(int index)
    {
        varList[index].parent.children.Remove(varList[index]);
        varList[index].childIncrements = 0;
        varList[index].parent = null;
    }

    private void UnchildBlock(CustomInspectorWindowBlock block)
    {
        block.parent.children.Remove(block);
        block.childIncrements = 0;
        block.parent = null;
    }

    // Basic conditional data
    private void DisplayConditionalData(int i, float spaceMult, float spaceOffset)
    {
        float offsetMod = ((spaceOffset * varList[i].childIncrements) / 3);

        EditorGUILayout.BeginHorizontal();
        GUI.Label(new Rect(spaceOffset, posRect.y + 2.5f + (space * spaceMult), 100, 20), "Condition");
        GUILayout.Space(75 + spaceOffset);
        GUI.Label(new Rect(spaceOffset + 75, posRect.y + 2.5f + (space * spaceMult), 250, 20), "Parent variable \"" + varList[i].parent.theName + "\" is");
        EditorGUILayout.EndHorizontal();

        switch (varList[i].parent.type)
        {
            case VariableList.Bool:
                CreateConditionalBool(i, space, spaceOffset);
                break;
            case VariableList.Int:
                CreateConditionalInt(i, space, spaceOffset);
                break;
            case VariableList.Float:
                CreateConditionalFloat(i, space, spaceOffset);
                break;
            case VariableList.Vector2:
                CreateConditionalVec2(i, space, spaceOffset);
                break;
            case VariableList.Vector3:
                CreateConditionalVec3(i, space, spaceOffset);
                break;
            case VariableList.String:
                CreateConditionalString(i, space, spaceOffset);
                break;
            default:
                GUILayout.Space(space * 3);
                break;
        }

        GUILayout.Space(space * 0.5f);
    }

    // Specific conditions
    private void CreateConditionalBool(int i, float spaceMult, float spaceOffset)
    {
        varList[i].boolCondition = (BoolConditions)EditorGUI.EnumPopup(new Rect(spaceOffset, posRect.y + 2.5f + spaceMult, 75, 18), varList[i].boolCondition);
    }

    private void CreateConditionalInt(int i, float spaceMult, float spaceOffset)
    {
        varList[i].numConditions = (NumConditions)EditorGUI.EnumPopup(new Rect(spaceOffset, posRect.y + 2.5f + spaceMult, 75, 18), varList[i].numConditions);

        if (varList[i].numConditions == NumConditions.Between)
        {
            varList[i].leftNum = EditorGUI.IntField(new Rect(spaceOffset + 100, posRect.y + 2.5f + spaceMult, 75, 18), (int)varList[i].leftNum);
            GUI.Label(new Rect(spaceOffset + 180, posRect.y + 2.5f + spaceMult, 100, 20), "and");
            varList[i].rightNum = EditorGUI.IntField(new Rect(spaceOffset + 225, posRect.y + 2.5f + spaceMult, 75, 18), (int)varList[i].rightNum);
        }

        else
            varList[i].leftNum = EditorGUI.IntField(new Rect(spaceOffset + 100, posRect.y + 2.5f + spaceMult, 75, 18), (int)varList[i].leftNum);
    }

    private void CreateConditionalFloat(int i, float spaceMult, float spaceOffset)
    {
        varList[i].numConditions = (NumConditions)EditorGUI.EnumPopup(new Rect(spaceOffset, posRect.y + 2.5f + spaceMult, 75, 18), varList[i].numConditions);

        if (varList[i].numConditions == NumConditions.Between)
        {
            varList[i].leftNum = EditorGUI.FloatField(new Rect(spaceOffset + 100, posRect.y + 2.5f + spaceMult, 75, 18), varList[i].leftNum);
            GUI.Label(new Rect(spaceOffset + 180, posRect.y + 2.5f + spaceMult, 100, 20), "and");
            varList[i].rightNum = EditorGUI.FloatField(new Rect(spaceOffset + 225, posRect.y + 2.5f + spaceMult, 75, 18), varList[i].rightNum);
        }

        else
            varList[i].leftNum = EditorGUI.FloatField(new Rect(spaceOffset + 100, posRect.y + 2.5f + spaceMult, 75, 18), varList[i].leftNum);
    }

    private void CreateConditionalVec2(int i, float spaceMult, float spaceOffset)
    {
        varList[i].otherConditions = (OtherConditions)EditorGUI.EnumPopup(new Rect(spaceOffset, posRect.y + 2.5f + spaceMult, 75, 18), varList[i].otherConditions);
        varList[i].theVec2 = EditorGUI.Vector2Field(new Rect(spaceOffset + 100, posRect.y + 2.5f + spaceMult, 200, 18), "", varList[i].theVec2);
    }

    private void CreateConditionalVec3(int i, float spaceMult, float spaceOffset)
    {
        varList[i].otherConditions = (OtherConditions)EditorGUI.EnumPopup(new Rect(spaceOffset, posRect.y + 2.5f + spaceMult, 75, 18), varList[i].otherConditions);
        varList[i].theVec3 = EditorGUI.Vector3Field(new Rect(spaceOffset + 100, posRect.y + 2.5f + spaceMult, 200, 18), "", varList[i].theVec3);
    }

    private void CreateConditionalString(int i, float spaceMult, float spaceOffset)
    {
        varList[i].otherConditions = (OtherConditions)EditorGUI.EnumPopup(new Rect(spaceOffset, posRect.y + 2.5f + spaceMult, 75, 18), varList[i].otherConditions);
        varList[i].theString = EditorGUI.TextField(new Rect(spaceOffset + 100, posRect.y + 2.5f + spaceMult, 200, 18), varList[i].theString);
    }

    // Other Data
    private void DisplayGeneralData(int i, float spaceMult, float spaceOffset, float offsetMod)
    {
        // Name
        GUILayout.Space(spaceOffset);
        varList[i].theName = EditorGUI.TextField(new Rect(spaceOffset, posRect.y + 2.5f + spaceMult, 300 - offsetMod, 18), "Variable Name", varList[i].theName);

        // Variable type
        EditorGUI.LabelField(new Rect(spaceOffset, posRect.y + 2.5f + (spaceMult + space), 100, 18), "Variable");
        varList[i].type = (VariableList)EditorGUI.EnumPopup(new Rect(spaceOffset + 75 - offsetMod, posRect.y + 2.5f + (spaceMult + space), 75 - offsetMod, 18), varList[i].type);

        // Protection level
        EditorGUI.LabelField(new Rect(spaceOffset + 175 - offsetMod, posRect.y + 2.5f + (spaceMult + space), 200, 18), "Protection");
        varList[i].protection = (Protection)EditorGUI.EnumPopup(new Rect(spaceOffset + 250 - offsetMod, posRect.y + 2.5f + (spaceMult + space), 75 - offsetMod, 18), varList[i].protection);
    }

    private void CreateBoolData(int i, float spaceMult, float spaceOffset, float offsetMod)
    {
        EditorGUI.LabelField(new Rect(spaceOffset, posRect.y + 2.5f + spaceMult, 200, 18), "Default");
        varList[i].theBool = EditorGUI.Toggle(new Rect(spaceOffset + 75 - offsetMod, posRect.y + 2.5f + spaceMult, 75 - offsetMod, 18), varList[i].theBool);
    }

    private void CreateIntData(int i, float spaceMult, float spaceOffset, float offsetMod)
    {
        // Default value
        EditorGUI.LabelField(new Rect(spaceOffset, posRect.y + 2.5f + spaceMult, 200, 18), "Default");
        varList[i].theInt = EditorGUI.IntField(new Rect(spaceOffset + 75 - offsetMod, posRect.y + 2.5f + spaceMult, 75 - offsetMod, 18), varList[i].theInt);

        // Min/Max toggle
        EditorGUI.LabelField(new Rect(spaceOffset + 175 - offsetMod, posRect.y + 2.5f + spaceMult, 200, 18), "Min/Max");
        varList[i].minMax = EditorGUI.Toggle(new Rect(spaceOffset + 250 - offsetMod, posRect.y + 2.5f + spaceMult, 75 - offsetMod, 18), varList[i].minMax);
    }

    private void CreateFloatData(int i, float spaceMult, float spaceOffset, float offsetMod)
    {
        // Default value
        EditorGUI.LabelField(new Rect(spaceOffset, posRect.y + 2.5f + spaceMult, 200, 18), "Default");
        varList[i].theFloat = EditorGUI.FloatField(new Rect(spaceOffset + 75 - offsetMod, posRect.y + 2.5f + spaceMult, 75 - offsetMod, 18), varList[i].theFloat);

        // Min/Max toggle
        EditorGUI.LabelField(new Rect(spaceOffset + 175, posRect.y + 2.5f + spaceMult, 200, 18), "Min/Max");
        varList[i].minMax = EditorGUI.Toggle(new Rect(spaceOffset + 250 - offsetMod, posRect.y + 2.5f + spaceMult, 75 - offsetMod, 18), varList[i].minMax);
    }

    private void CreateVec2Data(int i, float spaceMult, float spaceOffset, float offsetMod)
    {
        // Default value
        EditorGUI.LabelField(new Rect(spaceOffset, posRect.y + 2.5f + spaceMult, 200, 18), "Default");
        varList[i].theVec2 = EditorGUI.Vector2Field(new Rect(spaceOffset + 75 - offsetMod, posRect.y + 2.5f + spaceMult, 250 - offsetMod, 18), "", varList[i].theVec2);
    }

    private void CreateVec3Data(int i, float spaceMult, float spaceOffset, float offsetMod)
    {
        // Default value
        EditorGUI.LabelField(new Rect(spaceOffset, posRect.y + 2.5f + spaceMult, 200, 18), "Default");
        varList[i].theVec3 = EditorGUI.Vector3Field(new Rect(spaceOffset + 75 - offsetMod, posRect.y + 2.5f + spaceMult, 250 - offsetMod, 18), "", varList[i].theVec3);
    }

    private void CreateStringData(int i, float spaceMult, float spaceOffset, float offsetMod)
    {
        // Default value
        EditorGUI.LabelField(new Rect(spaceOffset, posRect.y + 2.5f + spaceMult, 200, 18), "Default");
        varList[i].theString = EditorGUI.TextField(new Rect(spaceOffset + 75 - offsetMod, posRect.y + 2.5f + spaceMult, 75 - offsetMod, 18), varList[i].theString);
    }

    private void CheckMinMax(int i, float spaceMult, MinMaxType type, float spaceOffset, float offsetMod)
    {
        if (varList[i].minMax)
        {
            GUILayout.Space(spaceOffset);

            // Int Min/Max
            if (type == MinMaxType.Int)
            {
                EditorGUI.LabelField(new Rect(spaceOffset, posRect.y + 2.5f + (spaceMult), 200, 18), "Minimum");
                varList[i].min = EditorGUI.IntField(new Rect(spaceOffset + 75 - offsetMod, posRect.y + (spaceMult), 75 - offsetMod, 18), (int)varList[i].min);

                EditorGUI.LabelField(new Rect(spaceOffset + 175, posRect.y + 2.5f + (spaceMult), 200, 18), "Maximum");
                varList[i].max = EditorGUI.IntField(new Rect(spaceOffset + 250 - offsetMod, posRect.y + (spaceMult), 75 - offsetMod, 18), (int)varList[i].max);
            }

            // Float Min/Max
            else if (type == MinMaxType.Float)
            {
                EditorGUI.LabelField(new Rect(spaceOffset, posRect.y + 2.5f + (spaceMult), 200, 18), "Minimum");
                varList[i].min = EditorGUI.FloatField(new Rect(spaceOffset + 75 - offsetMod, posRect.y + (spaceMult), 75 - offsetMod, 18), varList[i].min);

                EditorGUI.LabelField(new Rect(spaceOffset + 175, posRect.y + 2.5f + (spaceMult), 200, 18), "Maximum");
                varList[i].max = EditorGUI.FloatField(new Rect(spaceOffset + 250 - offsetMod, posRect.y + (spaceMult), 75 - offsetMod, 18), varList[i].max);
            }

            // Checks to make sure min and max values do not exceed logical boundaries
            if (varList[i].max < varList[i].min)
                varList[i].max = varList[i].min;
            else if (varList[i].min > varList[i].max)
                varList[i].min = varList[i].max;
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
            if (varList[i].parent != null)
                s += GetConditionalEffectString(i) + "\t";

            if (varList[i].type != VariableList.String && varList[i].type != VariableList.Bool)
                s += GetNumFieldString(i);
            else if (varList[i].type == VariableList.String)
                s += GetCharFieldString(i);
            else if (varList[i].type == VariableList.Bool)
                s += GetBoolFieldString(i);

            for (int j = i + 1; j < varList.Count; ++j)
            {
                if (j < varList.Count && varList[i].parent != null && varList[i].parent == varList[j].parent && GetConditionalEffectString(i) == GetConditionalEffectString(j))
                {
                    s += "\t";
                    if (varList[i].type != VariableList.String && varList[j].type != VariableList.Bool)
                        s += GetNumFieldString(j);
                    else if (varList[i].type == VariableList.String)
                        s += GetCharFieldString(j);
                    else if (varList[i].type == VariableList.Bool)
                        s += GetBoolFieldString(j);

                    ++i;
                }
            }

            if (varList[i].parent != null)
                s += "\t\t\tEditorGUI.indentLevel--;\n\t\t}\n";
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

    private string GetConditionalEffectString(int i)
    {
        string s = null;
        string n = varList[i].parent.theName.Replace(" ", string.Empty);

        // String used for getter/setter functions
        string val = varList[i].parent.theName[0].ToString().ToUpper();
        for (int j = 1; j < varList[i].parent.theName.Length; ++j)
            val += varList[i].parent.theName[j];
        val = val.Replace(" ", string.Empty);

        // Determines if the variable reads as a regular variable (Instance.x) or a Get variable (Instance.GetX())
        if ((varList[i].parent.protection == Protection.Private || varList[i - 1].protection == Protection.Protected))
            n = "Instance.Get" + val + "()";
        else if (!varList[i].minMax)
            n = "Instance." + n;

        switch (varList[i].parent.type)
        {
            case VariableList.Bool:
                if (varList[i].boolCondition == BoolConditions.True)
                    s += "\t\tif (" + n + ")\n\t\t{\n\t\t\tEditorGUI.indentLevel++;\n";
                else
                    s += "\t\tif (!" + n + ")\n\t\t{\n\t\t\tEditorGUI.indentLevel++;\n";
                break;
            case VariableList.Int:
            case VariableList.Float:
                if (varList[i].numConditions == NumConditions.EqualTo)
                    s += "\t\tif (" + n + " == " + varList[i].leftNum + ")\n\t\t{\n\t\t\tEditorGUI.indentLevel++;\n";
                else if (varList[i].numConditions == NumConditions.GreaterThan)
                    s += "\t\tif (" + n + " > " + varList[i].leftNum + ")\n\t\t{\n\t\t\tEditorGUI.indentLevel++;\n";
                else if (varList[i].numConditions == NumConditions.LessThan)
                    s += "\t\tif (" + n + " < " + varList[i].leftNum + ")\n\t\t{\n\t\t\tEditorGUI.indentLevel++;\n";
                else if (varList[i].numConditions == NumConditions.GreaterThanOrEqualTo)
                    s += "\t\tif (" + n + " >= " + varList[i].leftNum + ")\n\t\t{\n\t\t\tEditorGUI.indentLevel++;\n";
                else if (varList[i].numConditions == NumConditions.LessThanOrEqualTo)
                    s += "\t\tif (" + n + " <= " + varList[i].leftNum + ")\n\t\t{\n\t\t\tEditorGUI.indentLevel++;\n";
                else if (varList[i].numConditions == NumConditions.Between)
                    s += GetBetweenString(n, i);
                break;
            case VariableList.Vector2:
                if (varList[i].otherConditions == OtherConditions.Is)
                    s += "\t\tif (" + n + ".x == " + varList[i].checkedVec2.x + " && " + n + ".y == " + varList[i].checkedVec2.y + ")\n\t\t{\n\t\t\tEditorGUI.indentLevel++;\n";
                else
                    s += "\t\tif (" + n + ".x != " + varList[i].checkedVec2.x + " || " + n + ".y != " + varList[i].checkedVec2.y + ")\n\t\t{\n\t\t\tEditorGUI.indentLevel++;\n";
                break;
            case VariableList.Vector3:
                if (varList[i].otherConditions == OtherConditions.Is)
                    s += "\t\tif (" + n + ".x == " + varList[i].checkedVec3.x + " && " + n + ".y == " + varList[i].checkedVec3.y + " && " + n + ".z == " + varList[i].checkedVec3.z + ")\n\t\t{\n\t\t\tEditorGUI.indentLevel++;\n";
                else
                    s += "\t\tif (" + n + ".x != " + varList[i].checkedVec3.x + " || " + n + ".y != " + varList[i].checkedVec3.y + " || " + n + ".z != " + varList[i].checkedVec3.z + ")\n\t\t{\n\t\t\tEditorGUI.indentLevel++;\n";
                break;
            case VariableList.String:
                if (varList[i].otherConditions == OtherConditions.Is)
                    s += "\t\tif (" + n + " == \"" + varList[i].checkedString + "\")\n\t\t{\n\t\t\tEditorGUI.indentLevel++;\n";
                else
                    s += "\t\tif (" + n + " != \"" + varList[i].checkedString + "\")\n\t\t{\n\t\t\tEditorGUI.indentLevel++;\n";
                break;
        }
        return s;
    }

    private string GetBetweenString(string n, int i)
    {
        if (varList[i].leftNum > varList[i].rightNum)
            return "\t\t\tif(" + n + " > " + varList[i].rightNum + " && " + n + " < " + varList[i].leftNum + ")\n\t\t{\n\t\t\tEditorGUI.indentLevel++;\n";
        else if (varList[i].leftNum < varList[i].rightNum)
            return "\t\t\tif(" + n + " > " + varList[i].leftNum + " && " + n + " < " + varList[i].rightNum + ")\n\t\t{\n\t\t\tEditorGUI.indentLevel++;\n";
        else
            return "\t\t\tif(" + n + " == " + varList[i].leftNum + ")\n\t\t{\n\t\t\tEditorGUI.indentLevel++;\n\t";
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
        else
        {
            for (int i = 0; i < scriptName.Length; ++i)
            {
                if (!CheckValidBodyChars(scriptName[i]))
                {
                    Debug.LogError("Script name contains invalid character " + scriptName[i] + "!");
                    @bool = false;
                }
            }
        }

        // If there are no variables, output an error
        if (varList.Count == 0)
        {
            Debug.LogError("Script is empty!");
            @bool = false;
        }

        // Checks information for each variable to determine validity
        else
        {
            for (int i = 0; i < varList.Count; ++i)
            {
                string s = null;
                if (varList[i].theName == null)
                    s += "Name of variable " + i + " not set!\n";
                else if (!CheckForCopiedNames(varList[i].theName, i))
                    s += "Variable name " + varList[i].theName + " is the same as another variable and cannot be used!\n";
                else if (!CheckKeywords(varList[i].theName))
                    s += "Variable name " + varList[i].theName + " is a keyword and cannot be used!\n";
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
                    Debug.LogError("Variable " + i + " Errors:\n" + s);
                    @bool = false;
                }
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

    // Checks the variable name against all other variables to determine its validity
    private bool CheckForCopiedNames(string s, int index)
    {
        for (int i = 0; i < varList.Count; ++i)
            if (i != index && varList[i].theName == varList[index].theName)
                return false;

        return true;
    }

    // Checks the first character in the variable to determine its validity
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

    // Checks the second to last characters to determine their validity
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

    // Checks if any variable names match any existing keywords
    private bool CheckKeywords(string s)
    {
        string[] keywords = new string[48]
        {
            "bool", "enum",
            "byte", "short", "int", "long", "uint",
            "float", "double",
            "char", "string",
            "Vector2", "Vector3", "Vector2Int", "Vector3Int",
            "var", "GameObject", "Transform", "Color",

            "if", "else", "if else",
            "switch", "case",
            "class", "using", "instanceof", "typeof", "import",
            "while", "do", "for", "for each", "in",
            "break", "continue",
            "catch", "throw", "finally",
            "void", "delegate", "new", "static", "return",
            "null", "private", "public", "protected",
        };

        for (int i = 0; i < keywords.Length; ++i)
            if (s == keywords[i])
                return false;

        return true;
    }
    #endregion
}
