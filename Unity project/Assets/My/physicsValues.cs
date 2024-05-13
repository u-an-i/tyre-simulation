using MKStudio.EasyTweak;
using UnityEngine;

public class physicsValues : MonoBehaviour
{
    [EasyTweak("tyre radius", "physics")]
    public string TyreRadius
    {
        get
        {
            return "0.991199";
        }
    }
    [EasyTweak("tyre width", "physics")]
    public string TyreWidth
    {
        get
        {
            return "1";
        }
    }
    [EasyTweak(1f, 100f, "tyre mass", "physics")]
    public float TyreMass
    {
        get { return movement2.tyreMass; }
        set { movement2.tyreMass = value; }
    }
    [EasyTweak(0.01f, 1f, "tyre pressure", "physics")]
    public float TyrePressure
    {
        get { return movement2.unitTyrePressure; }
        set { movement2.unitTyrePressure = value; }
    }
    [EasyTweak(100f, 3000f, "tyre rubber dampening", "physics")]
    public float Dampening
    {
        get { return movement2.dampening; }
        set { movement2.dampening = value; }
    }
    [EasyTweak(5700f, 100000f, "tyre rubber spring constant", "physics")]
    public float TyreRubberSpringConstant
    {
        get { return movement2.tyreSpringConstant; }
        set { movement2.tyreSpringConstant = value; }
    }

    [EasyTweak(0f, 0.1f, "air friction", "physics")]
    public float AirFriction
    {
        get { return movement2.airFrictionCoefficient; }
        set { movement2.airFrictionCoefficient = value; }
    }
    [EasyTweak(0f, 0.01f, "roll friction (forward motion due to roll friction on ground is hardcoded, this is deceleration of rolling on ground)", "physics")]
    public float RollFriction
    {
        get { return movement2.rollFrictionEarthRubberCoefficient; }
        set { movement2.rollFrictionEarthRubberCoefficient = value; }
    }
    [EasyTweak(0f, 250f, "slide friction", "physics")]
    public float SlideFriction
    {
        get { return movement2.slideFrictionEarthRubberCoefficient; }
        set { movement2.slideFrictionEarthRubberCoefficient = value; }
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
