using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public enum RegionState { A, B, C }
public enum RegionEffect { Stable, Unstable, Volatile }
public enum Actions { Shift, Flip,  }



[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Region : MonoBehaviour {

    public static RegionEffect StateToEffect(RegionState state, PlayerID player)
    {
        if (state == RegionState.C) return RegionEffect.Volatile;
        else return player == PlayerID.P1 ? (RegionEffect)state : (RegionEffect)(((int)state - 1) * -1);
    }

    public Material[] TileMaterials
    {
        set
        {
            tileMaterials = value;
            updateMaterials();
        }
        get
        {
            return (Material[]) tileMaterials.Clone();
        }
    }

    public Material[] OutlineMaterials
    {
        set
        {
            outlineMaterials = value;
            updateMaterials();
        }
        get {
            return (Material[])outlineMaterials.Clone();
        }
    }

    public RegionState initialState = RegionState.A;
    public GameObject[] neighbouringRegions;
    public string RegionID;

    public RegionState State { get { return this.currentState; } }

    private RegionState currentState;
    private bool isMerged = false;
    private List<Vector3[]> outlineCorners = new List<Vector3[]>();
    private List<Vector3> originalPositions = new List<Vector3>();
    //private LineRenderer[] lineRenderers;

    private Material[] tileMaterials;
    private Material[] outlineMaterials;

    void Start () {
        currentState = initialState;
	}

    public void ShiftState(int offset) {
        currentState = (RegionState)(((int)currentState + offset) % 3);
        updateMaterials();
    }

    public void MakeRegionFromChildren() {
        MeshFilter[] toMerge = GetComponentsInChildren<MeshFilter>();
        mergeMeshes(toMerge);
        updateMaterials();
    }

    /**
     * Merge an array of meshes into a single mesh and add it to this object's MeshFilter
     */
    private void mergeMeshes(MeshFilter[] meshFilters) {
        if (isMerged)
        {
            Debug.Log("Sub-meshes for region \"" + RegionID + "\" are already merged.");
            return;
        }
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            i++;
        }
        outlineCorners.AddRange(meshFilters.Where(mf => mf != null && mf.GetComponent<HexMesh>() != null).Select(mf => mf.GetComponent<HexMesh>().OuterVertices));
        originalPositions.AddRange(meshFilters.Where(mf => mf != null && mf.GetComponent<HexMesh>() != null).Select(mf => mf.transform.position));
        Mesh newMesh = new Mesh();
        transform.GetComponent<MeshFilter>().sharedMesh = newMesh;
        newMesh.CombineMeshes(combine);
        isMerged = true;
    }

    private struct Triangle
    {
        public int[] vertices;
        public Triangle(int v1, int v2, int v3)
        {
            vertices = new int[] { v1, v2, v3 };
            if (vertices.Distinct().Count() != 3) throw new Exception("Not all diff.");
        }
        public Triangle(IEnumerable<int> vertices, int index) {
            this.vertices = new int[] { vertices.ElementAt(index), vertices.ElementAt(index + 1), vertices.ElementAt(index + 2) };
            if (this.vertices.Distinct().Count() != 3) throw new Exception("Not all diff.");
        }
    }

    private struct VertexContext {
        public int[] edges;
        public int[] triangles;
    }

    private void updateMaterials() {
        if (tileMaterials != null && tileMaterials[(int)currentState] != null)
            transform.GetComponent<MeshRenderer>().material = tileMaterials[(int)currentState];
        //if (lineRenderer != null && outlineMaterials != null && outlineMaterials[(int)currentState] != null)
        //    lineRenderer.material = outlineMaterials[(int)currentState];
    }

    public void SetTileMaterial(RegionState state, Material tileMaterial) {
        tileMaterials[(int)state] = tileMaterial;
        if (state == currentState) updateMaterials();
    }

    public void SetOutlineMaterial(RegionState state, Material outlineMaterial)
    {
        outlineMaterials[(int)state] = outlineMaterial;
        if (state == currentState) updateMaterials();
    }
}
