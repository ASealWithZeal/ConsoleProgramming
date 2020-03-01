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

public class CustomInspectorWindowBlock
{
    public VariableList type = VariableList.None;
    public string theName = null;

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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
                s = theString;
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
}
