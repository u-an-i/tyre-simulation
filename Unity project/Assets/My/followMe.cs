using Unity.Mathematics;
using UnityEngine;


public class followMe : MonoBehaviour
{
    public string cameraGameObjectName;

    private Transform camTransform;


    public string groundLayerName = "plane";
    private int groundLayer;
    RaycastHit[] results = new RaycastHit[1];

    Outline outline;


    public static int mouseXSensitivity = 1500;
    public static int mouseYSensitivity = 2250;


    static private int count = 0;
    static bool update = true;

    float currentZoom;
    const float zoomBase = 1.35f;
    float zoomTarget = 10f;
    float zoomProgress = 1f;
    const float zoomDuration = .5f;
    float zoomStart;

    Vector3 targetEulerAngles;
    Vector3 currentEulerAngles;
    const float rotationCoefficient = 10f;


    static bool camToCenter = false;
    static Vector3 center = new Vector3(0f, 10f, 0f);


    public static bool mouseControlsCam = true;


    void OnDestroy()
    {
        if (--count == 1)
        {
            camToCenter = false;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        enabled = update;

        groundLayer = 1 << LayerMask.NameToLayer(groundLayerName);

        outline = GetComponent<Outline>();
        outline.enabled = false;
    }

    void Awake()
    {
        GameObject cam = GameObject.Find(cameraGameObjectName);
        if(cam != null)
        {
            camTransform = cam.transform;
            currentZoom = zoomTarget;
            zoomStart = currentZoom;
            targetEulerAngles = camTransform.eulerAngles;
            currentEulerAngles = targetEulerAngles;
            camToCenter = ++count > 1;
        }
        else
        {
            update = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (mouseControlsCam)
        {
            camTransform.position = camToCenter ? center : transform.position;
            targetEulerAngles.x += Input.GetAxis("Mouse Y") * mouseYSensitivity * Time.deltaTime;
            targetEulerAngles.y += Input.GetAxis("Mouse X") * mouseXSensitivity * Time.deltaTime;
            targetEulerAngles.x = math.clamp(targetEulerAngles.x, -89.99f, 89.99f);
            Vector3 delta = targetEulerAngles - currentEulerAngles;
            float distance = delta.magnitude;
            float rotationProgress = Time.deltaTime * rotationCoefficient * distance;
            currentEulerAngles += distance < rotationProgress ? delta : (delta.normalized * rotationProgress);
            camTransform.rotation = Quaternion.Euler(currentEulerAngles);
            zoomProgress += Time.deltaTime / zoomDuration;
            float state = math.round(zoomProgress / (zoomProgress + 1f));
            zoomProgress = state + (1f - state) * zoomProgress;
            float mouseWheel = -Input.GetAxis("Mouse ScrollWheel");
            state = math.ceil(math.abs(mouseWheel));
            float zoomDir = math.round(state / (state + .5f)) * math.sign(mouseWheel);
            zoomProgress = zoomProgress * (1f - math.abs(zoomDir));
            zoomStart = math.floor(1f - zoomProgress) * currentZoom + math.ceil(zoomProgress) * zoomStart;
            zoomTarget *= math.pow(zoomBase, zoomDir);
            zoomTarget = math.clamp(zoomTarget, .02f, 6000f);
            currentZoom = zoomStart + (zoomTarget - zoomStart) * zoomProgress;
            camTransform.position -= currentZoom * camTransform.forward;
            camTransform.LookAt(camToCenter ? center : transform.position);
            Vector3 occlusionDir = camTransform.position - transform.position;
            outline.enabled = Physics.RaycastNonAlloc(transform.position, occlusionDir, results, occlusionDir.magnitude, groundLayer) > 0;
        }
    }
}
