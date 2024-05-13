using Unity.Mathematics;
using UnityEngine;

public class movement2 : MonoBehaviour
{
    int layerPlane, layerTyre;

    Material tyreDeform;
    int hitDirId, squishId;

    const float tyreRadius = .991199f;
    const float tyreWidth = 1f;
    public static float tyreMass = 40f;
    public static float unitTyrePressure = 0.25f;
    public static float dampening = 1200f;                   // 100 is minimum dampening for not bouncing back too high
    public static float tyreSpringConstant = 30000f;         // 5700 is minimum to not fall through ground when let fall from height 40

    const int numExtraAngleDirs = 8;
    const float initialLateralOffsetFactor = 1.0f / 3;
    const int lateralScans = 9;     // should be odd integer;

    bool hasContact = false;
    Vector3 contactDir;

    static Vector3 gravity = new Vector3(0, -9.81f, 0);
    public static float airFrictionCoefficient = 0.01f;
    public static float rollFrictionEarthRubberCoefficient = 0.0013f;
    public static float slideFrictionEarthRubberCoefficient = 100.0f;

    Vector3 velocity = Vector3.zero;
    Vector3 oldAcceleration = Vector3.zero;

    Vector3 motionDir = Vector3.zero;

    const float accelerationFactor = -10f;
    const float maxSteeringAngle = 30f;

    float angleSpeed = 0f;
    float oldAngleAcceleration = 0f;
    Vector3 oldSpinningInducedVelocity = Vector3.zero;

    const float spinningGeometryFactorForWheel = .5f;//8f/7f;           // https://uani.de/support/items-on-disc-momentum.png; for a ring disc, ring width = half disc radius, 2/3 is replaced by 7/8 (and hence the factor ought be 8/7 but 1/2 appears far better from experiments)
                                                                        // https://physics.stackexchange.com/questions/813207/momentum-of-spinning-disc-from-collision-conserving-momentum-of-input-momen

    Vector3 fixedLocalPosition;
    Quaternion fixedLocalRotation;
    float oldTiltAngle;
    const float tiltSpeed = 17f;

    //int framecount = 0, lastframecount = 0;


    // Start is called before the first frame update
    void Start()
    {
        layerPlane = 1 << LayerMask.NameToLayer("plane");
        layerTyre = 1 << LayerMask.NameToLayer("tyre");
        tyreDeform = GetComponent<MeshRenderer>().material;
        hitDirId = Shader.PropertyToID("_hitVector");
        squishId = Shader.PropertyToID("_squishStrength");
        fixedLocalPosition = transform.localPosition;
        fixedLocalRotation = transform.localRotation;
        oldTiltAngle = Vector3.Angle(Vector3.up, transform.forward);
    }

    void FixedUpdate()
    {
        transform.localPosition = fixedLocalPosition;
        transform.localRotation = fixedLocalRotation;

        bool hadContactBefore = hasContact;

        motionDir = Vector3.Cross(Vector3.Cross(transform.forward, motionDir), transform.forward).normalized;       // transform.forward == tyre rotation axis

        RaycastHit[] results = new RaycastHit[1];
        if(hasContact)
        {
            for(int i = 0; i <= numExtraAngleDirs; ++i)
            {
                Vector3 angleDir = Vector3.Lerp(motionDir, contactDir.normalized, i/(float)numExtraAngleDirs);      // Slerp not necessary
                //Debug.DrawRay(transform.position, 2f * angleDir, Color.yellow);
                if (Physics.RaycastNonAlloc(transform.position, angleDir, results, tyreRadius, layerPlane) > 0)
                {
                    contactDir = angleDir;
                    goto endContactSearch;
                }
                Vector3 lateralOffset = initialLateralOffsetFactor * tyreWidth * transform.forward;
                if (Physics.RaycastNonAlloc(transform.position - lateralOffset, angleDir, results, tyreRadius, layerPlane) > 0)
                {
                    contactDir = angleDir;
                    goto endContactSearch;
                }
                if (Physics.RaycastNonAlloc(transform.position + lateralOffset, angleDir, results, tyreRadius, layerPlane) > 0)
                {
                    contactDir = angleDir;
                    goto endContactSearch;
                }
            }
            hasContact = false;
        }
        else
        {
            hasContact = true;
            Vector3 orthoDir = Vector3.Cross(motionDir, transform.forward);
            for (int i = 0; i <= numExtraAngleDirs; ++i)
            {
                Vector3 angleDir = Vector3.Lerp(motionDir, orthoDir, i / (float)numExtraAngleDirs);      // Slerp not necessary
                //Debug.DrawRay(transform.position, 2f * angleDir, Color.yellow);
                if (Physics.RaycastNonAlloc(transform.position, angleDir, results, tyreRadius, layerPlane) > 0)
                {
                    contactDir = angleDir;
                    goto endContactSearch;
                }
                Vector3 lateralOffset = initialLateralOffsetFactor * tyreWidth * transform.forward;
                if (Physics.RaycastNonAlloc(transform.position - lateralOffset, angleDir, results, tyreRadius, layerPlane) > 0)
                {
                    contactDir = angleDir;
                    goto endContactSearch;
                }
                if (Physics.RaycastNonAlloc(transform.position + lateralOffset, angleDir, results, tyreRadius, layerPlane) > 0)
                {
                    contactDir = angleDir;
                    goto endContactSearch;
                }
                angleDir = Vector3.Lerp(motionDir, -orthoDir, i / (float)numExtraAngleDirs);      // Slerp not necessary
                //Debug.DrawRay(transform.position, 2f * angleDir, Color.yellow);
                if (Physics.RaycastNonAlloc(transform.position, angleDir, results, tyreRadius, layerPlane) > 0)
                {
                    contactDir = angleDir;
                    goto endContactSearch;
                }
                if (Physics.RaycastNonAlloc(transform.position - lateralOffset, angleDir, results, tyreRadius, layerPlane) > 0)
                {
                    contactDir = angleDir;
                    goto endContactSearch;
                }
                if (Physics.RaycastNonAlloc(transform.position + lateralOffset, angleDir, results, tyreRadius, layerPlane) > 0)
                {
                    contactDir = angleDir;
                    goto endContactSearch;
                }
            }
            hasContact = false;
        }

    endContactSearch:
        float newAngleAcceleration = accelerationFactor * Input.GetAxis("Vertical");

        Vector3 springTyreAcceleration = Vector3.zero;

        if (hasContact)
        {
            RaycastHit[] scans = new RaycastHit[lateralScans];
            for (int i = 0; i < lateralScans; ++i)
            {
                if (Physics.RaycastNonAlloc(transform.position + (-tyreWidth / 2 + i * tyreWidth / (lateralScans - 1)) * transform.forward, contactDir, results, 3F * tyreRadius, layerPlane) > 0)
                {
                    scans[i] = results[0];
                }
                else
                {
                    scans[i] = new RaycastHit();
                    scans[i].distance = 0f;
                }
            }
            float min = float.MaxValue;
            float max = 0f;
            int indexMin = -1;
            int indexLowerEnd = -1, indexUpperEnd = -1;
            for (int i = 0; i < lateralScans; ++i)
            {
                if (scans[i].distance > 0f)
                {
                    if (scans[i].distance <= min)
                    {
                        if (i <= lateralScans / 2)
                        {
                            min = scans[i].distance;
                            indexMin = i;
                        }
                        else if(scans[i].distance < min)
                        {
                            min = scans[i].distance;
                            indexMin = i;
                        }
                    }
                    if (scans[i].distance > max)
                    {
                        max = scans[i].distance;
                    }
                    if (indexLowerEnd == -1)
                    {
                        indexLowerEnd = i;
                    }
                    else
                    {
                        indexUpperEnd = i;
                    }
                }
            }
            if (indexMin != lateralScans / 2)
            {
                if (max - min > 0.05F * tyreRadius)
                {
                    // lateral roll
                    if(indexMin < lateralScans / 2)
                    {
                        transform.RotateAround(scans[indexMin].point, Vector3.Cross(transform.forward, contactDir), math.atan((scans[indexUpperEnd].distance - min) / ((indexUpperEnd - indexMin) * tyreWidth / (lateralScans - 1))));
                    }
                    else
                    {
                        transform.RotateAround(scans[indexMin].point, Vector3.Cross(transform.forward, contactDir), math.atan((scans[indexLowerEnd].distance - min) / ((indexLowerEnd - indexMin) * tyreWidth / (lateralScans - 1))));
                    }
                }
            }
            Vector3 tyreSquishForceDir = -contactDir.normalized;
            //Debug.DrawLine(transform.position, scans[indexMin].point, Color.blue);
            float tyreSquishForceVal = (tyreRadius - min) / unitTyrePressure;   // this is an approximation: lateral roll is already applied, contact induced spring force is opposite contact dir at the value if the tyre was not laterally rolled

            tyreDeform.SetVector(hitDirId, -tyreSquishForceDir);
            tyreDeform.SetFloat(squishId, tyreSquishForceVal);

            // tyre acts like dampened spring
            // dampen force is proportionally tyre movement in spring force dir as long as tyre surface contacts the ground
            // spring force dir is tyreSquishForceDir
            // when tyre surface looses contact to ground, tyre surface stops moving as spring instantly (this is an approximation here)
            // springTyreAcceleration is acceleration of the whole tyre due to the acting like a spring
            springTyreAcceleration = (tyreSpringConstant * tyreSquishForceVal - dampening * Vector3.Dot(velocity, tyreSquishForceDir)) * tyreSquishForceDir / tyreMass;
            //Debug.DrawRay(transform.position, springTyreAcceleration, Color.green);

            newAngleAcceleration -= math.sign(angleSpeed) * rollFrictionEarthRubberCoefficient * math.abs(angleSpeed) / Time.fixedDeltaTime;

            //Debug.DrawRay(transform.position, 2f * Vector3.Cross(contactDir, transform.forward).normalized, Color.magenta);
        }
        else
        {
            tyreDeform.SetFloat(squishId, 0f);
            //spinSurfaceRubberAirFriction = ignored;
        }


        if (!hadContactBefore && hasContact)
        {
            Vector3 rollDir = Vector3.Cross(contactDir, transform.forward).normalized;
            float velocityInRollDirVal = Vector3.Dot(velocity, rollDir);        //Debug.Log("vR " + velocityInRollDirVal*rollDir);
            float angleSpeedFromVelocityInRollDir = spinningGeometryFactorForWheel * velocityInRollDirVal / tyreRadius;
            Vector3 velocityOrthoRollDir = velocity - velocityInRollDirVal * rollDir;       //Debug.Log("vO " + velocityOrthoRollDir);
            float debugang = angleSpeed;
            if (math.abs(angleSpeed) < math.abs(angleSpeedFromVelocityInRollDir))
            {
                angleSpeed = math.abs(math.sign(angleSpeed) - math.sign(angleSpeedFromVelocityInRollDir)) / 2f * angleSpeed + angleSpeedFromVelocityInRollDir;
            }
            velocityInRollDirVal = 0f;
            Vector3 debugvel = velocity;
            velocity = velocityOrthoRollDir + velocityInRollDirVal * rollDir;
            //Debug.Log("has contact again:\n\tvelocity val before: " + debugvel.magnitude + " velocity dir to roll before: " + Vector3.Dot(debugvel.normalized, rollDir) +
            //    "\n\tvelocity val now: " + velocity.magnitude + " velocity dir to roll now: " + Vector3.Dot(velocity.normalized, rollDir) +
            //    "\n\tangleSpeed before: " + debugang + " angleSpeed now: " + angleSpeed + " after " + (framecount - lastframecount) + " frames");
            //lastframecount = framecount;
        }


        float deltaAngle = angleSpeed * Time.fixedDeltaTime + (2 * oldAngleAcceleration + newAngleAcceleration) * Time.fixedDeltaTime * Time.fixedDeltaTime / 6f;
        angleSpeed += (oldAngleAcceleration + newAngleAcceleration) * Time.fixedDeltaTime / 2;
        if(angleSpeed > 10f)
        {
            angleSpeed = 10f;
        }
        else if(angleSpeed < -10f)
        {
            angleSpeed = -10f;
        }
        float slipfactor = 1f;//1f - math.pow(angleSpeed / 10f, 32f);

        oldAngleAcceleration = newAngleAcceleration;

        transform.Rotate(new Vector3(0f, 0f, deltaAngle * Mathf.Rad2Deg));

        Vector3 oldPosition = transform.localPosition;
        Vector3 spinningInducedVelocity = Vector3.zero;
        float slideFrictionCoefficient = 0f;
        if(hasContact)
        {
            Vector3 distanceDelta = slipfactor * deltaAngle * tyreRadius * Vector3.Cross(contactDir, transform.forward).normalized;
            transform.localPosition += distanceDelta;
            //velocity += distanceDelta / Time.fixedDeltaTime;      // do not add to velocity which is movement due to inertia here, spinning disc induces an independant motion on body due to friction with ground
            spinningInducedVelocity = 1f / spinningGeometryFactorForWheel * distanceDelta / Time.fixedDeltaTime;
            //spinSurfaceRubberAirFriction = ignored;
            if(math.abs(Vector3.Dot(velocity.normalized, transform.forward)) > 0.34f)
            {
                slideFrictionCoefficient = slideFrictionEarthRubberCoefficient;
            }
        }
        else
        {
            velocity += oldSpinningInducedVelocity;
            //if (oldSpinningInducedVelocity != Vector3.zero) { Debug.Log("lost contact: velocity val now = " + velocity.magnitude + " angleSpeed now = " + angleSpeed + " after " + (framecount - lastframecount) + " frames"); lastframecount = framecount; }
        }
        //++framecount;

        Vector3 newAcceleration = gravity - (airFrictionCoefficient + slideFrictionCoefficient) * velocity.sqrMagnitude * velocity.normalized + springTyreAcceleration;

        transform.localPosition += velocity * Time.fixedDeltaTime + (2 * oldAcceleration + newAcceleration) * Time.fixedDeltaTime * Time.fixedDeltaTime / 6f;
        velocity += (oldAcceleration + newAcceleration) * Time.fixedDeltaTime / 2;

        oldAcceleration = newAcceleration;
        oldSpinningInducedVelocity = spinningInducedVelocity;

        motionDir = transform.localPosition - oldPosition;


        transform.RotateAround(transform.position, Vector3.up, maxSteeringAngle * Input.GetAxis("Horizontal") * Time.fixedDeltaTime);


        fixedLocalPosition = transform.localPosition;
        fixedLocalRotation = transform.localRotation;


        // roll downhill due to gravity no yet implemented:
        /*
                Vector3 tyreForward = Vector3.Cross(Vector3.up, transform.forward);                             // tyre roll is "in" tyre local up, side plane (tyre axis is forward)
                tyreForward -= Vector3.Dot(tyreForward, results[0].normal) * results[0].normal;                 // determine direction on "contact plane"
                tyreForward.Normalize();
                Vector3 groundProjectedRollAcceleration = tyreForward;
                groundProjectedRollAcceleration.y = 0.0f;
                groundProjectedRollAcceleration.Normalize();
                float RollDir = -Mathf.Sign(tyreForward.y);
                float cosB = Vector3.Dot(groundProjectedRollAcceleration, tyreForward);
                // "correct" acceleration of disc on slope due to gravity (gravity is perpendicular world, gravity pulls down the rhs and lhs of a disc, r/lhs due to disc is split by gravity world upwards from contact point on slope, not in half along slope normal !!! calculation of pull rhs (if slope down to right) minus pull lhs solves nicely (use parallel axis (/Steiners) theorem, disc divided into smaller sections adds up nicely), scan of derivation will follow
                float newAngleAcceleration = fakeRollAccelerationFactor * RollDir * 8f / (9f * Mathf.PI * tyreRadius) * (1 - cosB * cosB * cosB) * -gravity.y - ((rictionPerTyreMassOnSand - rollFrictionPerTyreMassOnSand) * Mathf.Pow(2f, -400f * angleSpeed * angleSpeed) + rollFrictionPerTyreMassOnSand) * Mathf.Sign(angleSpeed);
        */
    }

    void Update()
    {
        float deltaTiltAngle = oldTiltAngle - Vector3.Angle(Vector3.up, transform.forward);
        float progressTiltAngle = -math.sign(deltaTiltAngle) * tiltSpeed * Time.deltaTime;
        transform.RotateAround(transform.position + tyreRadius * contactDir.normalized, Vector3.Cross(Vector3.up, transform.forward), math.abs(progressTiltAngle) > math.abs(deltaTiltAngle) ? 0f : (deltaTiltAngle + progressTiltAngle));
        oldTiltAngle = Vector3.Angle(Vector3.up, transform.forward);
    }
}