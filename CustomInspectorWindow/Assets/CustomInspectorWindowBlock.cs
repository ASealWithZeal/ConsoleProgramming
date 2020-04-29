using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VariableList
{
    None = 0,
    Bool,
    Int,
    Float,
    Vector2,
    Vector3,
    //Char,
    String,
    //GameObject,
    //Transform,
}

public enum Protection
{
    None = 0,
    Private,
    Public,
    Protected
}

public enum BoolConditions
{
    True = 0,
    False
}

public enum NumConditions
{
    EqualTo = 0,
    GreaterThan,
    GreaterThanOrEqualTo,
    LessThan,
    LessThanOrEqualTo,
    Between
}

public enum OtherConditions
{
    Is = 0,
    IsNot
}

public class CustomInspectorWindowBlock
{
    public VariableList type = VariableList.None;
    public string theName = null;

    // Childed values
    public CustomInspectorWindowBlock parent = null;
    public List<CustomInspectorWindowBlock> children = new List<CustomInspectorWindowBlock>();
    public int childIncrements = 0;

    // Conditional values
    public BoolConditions boolCondition = BoolConditions.True;
    public bool checkedBool = true;
    public NumConditions numConditions = NumConditions.EqualTo;
    public float leftNum = 0;
    public float rightNum = 0;
    public OtherConditions otherConditions = OtherConditions.Is;
    public Vector2 checkedVec2 = new Vector2();
    public Vector3 checkedVec3 = new Vector3();
    public string checkedString = "";

    // Variable values
    public bool theBool = false;
    public int theInt = 0;
    public float theFloat = 0.0f;
    public Vector2 theVec2 = new Vector2(0, 0);
    public Vector3 theVec3 = new Vector3(0, 0, 0);
    public char theChar = ' ';
    public string theString = " ";
    public GameObject theObj = null;
    public Transform theTransform = null;

    public bool selected = false;

    // Other values
    public Protection protection;
    public bool minMax = false;
    public float min = 0.0f;
    public float max = 0.0f;

    public string GetTypeValue()
    {
        string s = null;

        switch (type)
        {
            case VariableList.Bool:
                s = theBool.ToString().ToLower();
                break;
            case VariableList.Float:
                s = theFloat.ToString() + "f";
                break;
            case VariableList.Int:
                s = theInt.ToString();
                break;
            case VariableList.String:
                theString = UpdateString();
                s = "\"" + theString + "\"";
                break;
            case VariableList.Vector2:
                s = "new Vector2(" + theVec2.x + "f, " + theVec2.y + "f)";
                break;
            case VariableList.Vector3:
                s = "new Vector3(" + theVec3.x + "f, " + theVec3.y + "f, " + theVec3.z + "f)";
                break;
        }

        return s;
    }

    // Updates the string if it includes " or '
    private string UpdateString()
    {
        List<char> s = new List<char>();

        for (int i = 0; i < theString.Length; ++i)
        {
            if ((theString[i] == '\"' || theString[i] == '\'') && ((i > 0 && theString[i - 1] != '\\') || (i == 0)))
                s.Add('\\');
            s.Add(theString[i]);
        }

        string holder = "";
        for (int i = 0; i < s.Count; ++i)
            holder += s[i];
        return holder;
    }
}
