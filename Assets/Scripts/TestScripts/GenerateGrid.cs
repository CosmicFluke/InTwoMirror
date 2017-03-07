using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateGrid : MonoBehaviour {

    public GameObject cubePrefab;
    public int width;
    public int length;
    public Terrain terrain;
    public Material highlightMat;

    private GameObject[,] tiles;
    private Material origMat;

	// Use this for initialization
	void Start () {
        tiles = new GameObject[length, width];
        foreach (int z in Enumerable.Range(0, length))
        {
            foreach (int x in Enumerable.Range(0, width)) {
                Vector3 pos = new Vector3(x * (cubePrefab.transform.localScale.x + 0.02f) - (z % 2) * cubePrefab.transform.localScale.x * 0.5f, 0, z * (cubePrefab.transform.localScale.z + 0.02f));
                pos.y = terrain.SampleHeight(pos) - (transform.localScale.y / 4);
                tiles[z, x] = Instantiate(cubePrefab, pos, Quaternion.identity);
                tiles[z, x].transform.parent = transform;
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
