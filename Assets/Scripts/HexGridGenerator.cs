using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent (typeof(BoardConfiguration))]
public class HexGridGenerator : MonoBehaviour {

    public GameObject hexPrefab;
    public int width;
    public int length;

    private GameObject[,] tiles;
    private Material origMat;

    private bool generated = false;

    // Use this for initialization
    void Awake() {
        tiles = new GameObject[length, width];
    }


    void Start(){
    }

    [ContextMenu("Generate tiles")]
    public void Generate() {
        if (tiles == null) tiles = new GameObject[length, width];
        HexMesh hexMesh = hexPrefab.GetComponent<HexMesh>();
        if (hexMesh == null) {
            Debug.Log("Could not generate hex tiles: hexPrefab does not have a HexMesh component.");
            return;
        }
        float outerRadius = hexPrefab.GetComponent<HexMesh>().radius;
        float innerRadius = outerRadius * HexMesh.radiusRatio;
        float rowOffset = 0, colOffset = 0;
        Vector3 parentPos = gameObject.transform.position;
        foreach (int z in Enumerable.Range(0, length))
        {
            rowOffset = -(z % 2) * innerRadius * 0.5f;
            foreach (int x in Enumerable.Range(0, width - 1 + (z % 2)))
            {
                Vector3 pos = new Vector3(parentPos.x / 2f + rowOffset, parentPos.y, parentPos.z / 2f + colOffset);
                tiles[z, x] = Instantiate(hexPrefab, pos, Quaternion.identity);
                tiles[z, x].transform.parent = gameObject.transform;
                rowOffset += innerRadius;
            }
            colOffset += (1.5f * outerRadius) / 2f;
        }
        generated = true;
    }

    [ContextMenu("Destroy tiles")]
    public void DestroyAll()
    {
        if (tiles == null) return;
        foreach (GameObject tile in tiles) {
            if (tile != null)
            {
                if (Application.isPlaying) Destroy(tile);
                else DestroyImmediate(tile);
            }
        }
        tiles = null;
        generated = false;
    }
	
	// Update is called once per frame
	void Update () {
    }

    private void OnApplicationQuit()
    {

    }
}
