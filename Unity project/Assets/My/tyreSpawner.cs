using MKStudio.EasyTweak;
using UnityEngine;
using System.Collections.Generic;

public class tyreSpawner : MonoBehaviour
{
    public GameObject tyre;

    [EasyTweak("forwards", "controls")]
    public string forwards
    {
        get
        {
            return "w";
        }
    }
    [EasyTweak("backwards", "controls")]
    public string backwards
    {
        get
        {
            return "s";
        }
    }
    [EasyTweak("left", "controls")]
    public string left
    {
        get
        {
            return "a";
        }
    }
    [EasyTweak("right", "controls")]
    public string right
    {
        get
        {
            return "d";
        }
    }
    [EasyTweak("camera", "controls")]
    public string camera
    {
        get
        {
            return "mouse movement, mouse wheel";
        }
    }

    [EasyTweak(1, 128, "amount tyres to spawn; when only 1 total, you can control the tyre", "content")]
    public int AmountTyresToSpawn
    {
        get { return this.amountTyresToSpawn; }
        set { this.amountTyresToSpawn = value; }
    }
    
    int amountTyresToSpawn = 1;


    List<GameObject> list = new List<GameObject>();


    [EasyTweak("spawn additional tyres", "content")]
    void Spawn()
    {
        float angle = 0f;
        if(camTransform != null)
        {
            angle = camTransform.eulerAngles.y;
        }
        for (int i = 0; i < amountTyresToSpawn; i++)
        {
            float z = Random.value * 75 - 50;
            float y = 20 + Random.value * 60;
            float x = (-1 + Random.value * 2) * z * Mathf.Tan(30 * Mathf.Deg2Rad);
            list.Add(Instantiate(tyre, Quaternion.AngleAxis(angle, Vector3.up) * new Vector3(x, y, z), tyre.transform.rotation));
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public string cameraGameObjectName;

    private Transform camTransform = null;

    void Awake()
    {
        GameObject cam = GameObject.Find(cameraGameObjectName);
        if (cam != null)
        {
            camTransform = cam.transform;
        }
    }

    private void OnEnable()
    {
        enabled = false;
        list.ForEach(item => Destroy(item));
        list.Clear();
        Spawn();
    }
}
