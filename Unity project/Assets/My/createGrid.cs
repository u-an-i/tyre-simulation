using MKStudio.EasyTweak;
using UnityEngine;

public class createGrid : MonoBehaviour {

	public GameObject planePrefab;

	private const int singleSize = 10;
	private int halfGridLength = 50;
    private GameObject grid = null;

	private bumpPlanes bumper;


    [EasyTweak(5, 200, "landscape half edge length", "landscape")]
    public int HalfGridLength
    {
        get { return this.halfGridLength; }
        set { this.halfGridLength = value; }
    }


    public void doCreateGrid() {
		if (grid != null) {
			grid.transform.parent = null;
			Destroy (grid);
		}
		// empty GO to contain the grid for easier deletion on update (above)
		// don't child it yet for easier Instantiate() management
		grid = new GameObject ();
		grid.isStatic = true;
        Transform gridTransform = grid.transform;
		// get grid size setting
		int gridLength = 2 * halfGridLength + 1;
		// duplicate
		for (int i = 0; i < gridLength; i++) {
			for (int j = 0; j < gridLength; j++) {
				Transform duplicate = Instantiate (planePrefab, gridTransform).transform;
				float x = (i - halfGridLength) * singleSize;
				float z = (j - halfGridLength) * singleSize;
				duplicate.localPosition = new Vector3 (x, 0F, z);
				duplicate.name = "plane_" + x + "_" + z;
                duplicate.gameObject.isStatic = true;
			}
		}
		// make that GO child of self
		gridTransform.parent = transform;
    }


    [EasyTweak("rebuild landscape and respawn", "landscape")]
    void Build()
    {
        doCreateGrid();
        bumper.enabled = true;
    }


    // Use this for initialization
    void Start ()
    {
        enabled = false;
		bumper = GetComponent<bumpPlanes>();
        Build();
    }
}