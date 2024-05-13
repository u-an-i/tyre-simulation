using MKStudio.EasyTweak;
using UnityEngine;

public class initSettings : MonoBehaviour
{
    [EasyTweak(500, 2500, "mouse horizontal movement sensitivity", "controls")]
    public int MouseXSensitivity
    {
        get { return followMe.mouseXSensitivity; }
        set { followMe.mouseXSensitivity = value; }
    }

    [EasyTweak(1000, 3500, "mouse vertical movement sensitivity", "controls")]
    public int MouseYSensitivity
    {
        get { return followMe.mouseYSensitivity; }
        set { followMe.mouseYSensitivity = value; }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
