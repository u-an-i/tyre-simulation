using System;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Mathematics.math;
using u_i;
using MKStudio.EasyTweak;


public class bumpPlanes : MonoBehaviour {

	public float maxBumpingHeight = 3F;
	public int numberBumpsPer10000 = 100;

    private int halfGridLength;
    private float[] heights;
	private int gridLineLength;
	private GameObject[] planesForHeights;

	private createGrid gridder;

    public GameObject tyre;


    [EasyTweak(.2f, 6f, "landscape max bump height", "landscape")]
    public float MaxBumpingHeight
    {
        get { return this.maxBumpingHeight; }
        set { this.maxBumpingHeight = value; }
    }


    [EasyTweak(10, 200, "landscape bumpiness", "landscape")]
    public int NumberBumpsPer10000
    {
        get { return this.numberBumpsPer10000; }
        set { this.numberBumpsPer10000 = value; }
    }


    [EasyTweak("bumpy landscape", "landscape")]
	bool doBump = true;

	tyreSpawner tyres;


    private void getPlanesForHeights()
	{
		planesForHeights = new GameObject[gridLineLength * gridLineLength];
		int sidelength = 10;
		int gridHalfLength = gridLineLength / 2;
		for (int a = 0; a < gridLineLength; a++)
		{
			for (int b = 0; b < gridLineLength; b++)
			{
				planesForHeights[a * gridLineLength + b] = GameObject.Find("plane_" + ((a - gridHalfLength) * sidelength) + "_" + ((b - gridHalfLength) * sidelength));
			}
		}
	}

	private void assignCollider()
	{
		for (int i = 0; i < planesForHeights.Length; i++)
		{
			GameObject g = planesForHeights[i];
			g.GetComponent<MeshCollider>().sharedMesh = g.GetComponent<MeshFilter>().mesh;
		}
	}

	private void setHeights(float xc, float zc, float height, float radius, int halfEdgeLength)
	{
		int limit = halfEdgeLength + 5;
		int xmin = Mathf.FloorToInt(xc - radius);
		int zmin = Mathf.FloorToInt(zc - radius);
		if (zmin < -limit)
		{
			zmin = -limit;
		}
		if (xmin < -limit)
		{
			xmin = -limit;
		}
		int r2 = Mathf.CeilToInt(2F * radius);
		int offsetX = 0;
		int xmax = xmin + r2;
		if (xmax > limit)
		{
			offsetX = xmax - limit;
		}
		int offsetZ = 0;
		int zmax = zmin + r2;
		if (zmax > limit)
        {
			offsetZ = zmax - limit;
		}
		int numPerLine = 2 * limit + 1;
		float c = -1.21F * 2.7F * 2.7F / (radius * radius);
		for (int i = 0; i <= r2 - offsetZ; i++)
        {
			for(int j = 0; j <= r2 - offsetX; j++)
            {
				heights[(zmin + i + limit) * numPerLine + xmin + j + limit] += height * pow(2F, c * ((zmin + i - zc) * (zmin + i - zc) + (xmin + j - xc) * (xmin + j - xc)));
			}
        }
	}

	private IEnumerator<float> rise()
    {
		int limit = 10 * (gridLineLength / 2) + 5;
		int numPerLine = 2 * limit + 1;
		float timeStart = Time.time;
		float p;
		do
		{
			p = (Time.time - timeStart) / 5F;
			for (int i = 0; i < planesForHeights.Length; i++)
			{
				GameObject gO = planesForHeights[i];
				Mesh mesh = gO.GetComponent<MeshFilter>().mesh;
				Vector3[] vertices = mesh.vertices;
				for (int j = 0; j < vertices.Length; j++)
				{
					Vector3 pos = gO.transform.position;
					Vector3 v = vertices[j];
					vertices[j].y = p * heights[(Mathf.RoundToInt(pos.z + v.z) + limit) * numPerLine + Mathf.RoundToInt(pos.x + v.x) + limit];
				}
				mesh.vertices = vertices;
				mesh.RecalculateBounds();
				mesh.RecalculateNormals();
				mesh.RecalculateTangents();
			}
			yield return 0F;
		}
		while (p < 1F);
		for (int i = 0; i < planesForHeights.Length; i++)
		{
			GameObject gO = planesForHeights[i];
			Mesh mesh = gO.GetComponent<MeshFilter>().mesh;
			Vector3[] vertices = mesh.vertices;
			for (int j = 0; j < vertices.Length; j++)
			{
				Vector3 pos = gO.transform.position;
				Vector3 v = vertices[j];
				vertices[j].y = heights[(Mathf.RoundToInt(pos.z + v.z) + limit) * numPerLine + Mathf.RoundToInt(pos.x + v.x) + limit];
			}
			mesh.vertices = vertices;
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();
            mesh.RecalculateBounds();
        }
		yield return 0F;
		heights = null;
		assignCollider();
		yield return 0F;
		planesForHeights = null;
        tyres.enabled = true;
    }

    private IEnumerator<float> startBumping()
	{
		yield return 0F;
		int halfEdgeLength = 10 * halfGridLength;
		heights = new float[(2 * halfEdgeLength + 11) * (2 * halfEdgeLength + 11)];
		Array.Fill(heights, 0F);
		yield return 0F;
		int numberBumps = 4 * halfEdgeLength * halfEdgeLength * numberBumpsPer10000 / 10000;
        for (int i = 0; i < numberBumps; i++) {
			float height = UnityEngine.Random.value * maxBumpingHeight;
			setHeights(2F * halfEdgeLength * (UnityEngine.Random.value - .5F), 2F * halfEdgeLength * (UnityEngine.Random.value - .5F), height, 1.719F * height * (maxBumpingHeight / 3F * (1F + UnityEngine.Random.value * 4F)), halfEdgeLength);
			//yield return 0F;
		}
		gridLineLength = 2 * halfGridLength + 1;
		getPlanesForHeights();
		yield return 0F;
		Timing.RunCoroutine(rise(), 4);
    }

    private void Awake()
    {
		tyres = GetComponent<tyreSpawner>();
		gridder = GetComponent<createGrid>();
    }

    void OnEnable ()
    {
        enabled = false;
        Timing.KillCoroutines(4);
        if (doBump)
        {
            halfGridLength = gridder.HalfGridLength;
            Timing.RunCoroutine(startBumping(), 4);
        }
		else
        {
            tyres.enabled = true;
        }
    }
}