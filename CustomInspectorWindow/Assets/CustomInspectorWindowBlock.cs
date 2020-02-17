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
    Char,
    String,
    GameObject,
    Transform,
}

public enum Protection
{
    None = 0,
    Private,
    Public,
    Protected
}

public class CustomInspectorWindowBlock : MonoBehaviour
{
    public VariableList type = VariableList.None;
    public bool theBool = false;
    public int theInt = 0;
    public float theFloat = 0.0f;
    public Vector2 theVec2 = new Vector2(0, 0);
    public Vector3 theVec3 = new Vector3(0, 0, 0);
    public char theChar = ' ';
    public string theString = " ";
    public GameObject theObj = null;
    public Transform theTransform = null;


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
}
