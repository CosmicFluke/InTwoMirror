using UnityEngine;

public class ObeliskAnimationScript : MonoBehaviour {

    public float RotationRate = 20f;
    public float BouncingRate = 0.1f;
    public bool ShouldRotate = true;
    public bool ShouldBounce = true;

    private float initialHeight;

	// Use this for initialization
	void Start () {
        initialHeight = transform.position.y;
	}
	
	// Update is called once per frame
	void Update () {
        if (ShouldRotate) transform.Rotate(new Vector3(0, 0, Time.deltaTime * RotationRate));
        if (ShouldBounce) transform.position = new Vector3(transform.position.x, initialHeight + (Mathf.Sin(Time.time) * BouncingRate), transform.position.z);
    }
}
