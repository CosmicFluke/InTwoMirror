using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChanger : MonoBehaviour {

    public Material highlightMat;

    private Material origMat;

	// Use this for initialization
	void Start () {
        origMat = GetComponent<MeshRenderer>().material;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "skinny")
            GetComponent<MeshRenderer>().material = highlightMat;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "skinny")
            GetComponent<MeshRenderer>().material = origMat;
    }
}
