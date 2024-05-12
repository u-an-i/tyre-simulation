using MKStudio.EasyTweak;
using UnityEngine;

public class timeSettings : MonoBehaviour
{
    [EasyTweak(0.1f, 2f, "realtime speed = 1", "Timing")]
    public float timing
    {
        get { return Time.timeScale; }
        set { Time.timeScale = value; }
    }

    [EasyTweak(30, 1000, "Physics Calculation with Hz", "Timing")]
    public int physicsTiming
    {
        get { return Mathf.RoundToInt(1f / Time.fixedDeltaTime); }
        set { Time.fixedDeltaTime = 1f / value; }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
