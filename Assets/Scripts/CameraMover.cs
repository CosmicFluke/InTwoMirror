using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// SD note: this class copied from http://answers.unity3d.com/questions/912621/make-a-camera-to-follow-two-players.html
// it may need to be replaced to meet academic requirements

public class CameraMover : MonoBehaviour{

	// SD commented out as not required for InTwo public static CameraMover cTrack; //cFollow
	public float dampTime = 0.15f; //figure out what this is
	public float smoothing = 5.0f;
	private Vector3 velocity = Vector3.zero;
	public Transform target;

	public float midX;
	public float midY;
	public float midZ;

	public Transform player1; //target1
	public Transform player2; //target2

	public Vector3 midPoint;
	public Vector3 distance;
	public Vector3 offset;
	public float camDist;

	public float camOffset;
	public float bounds;

	Camera camera;

	void Start(){
		camDist = 9.0f;
		bounds = 12.0f;
		camera = GetComponent<Camera>();
	}

	void LateUpdate(){

		distance = player1.position - player2.position;
		Debug.Log("distance " + distance);
	
		if (camDist >= 13.0f) {
			camDist = 13.0f;
		}
		if (camDist <= 4.0f) {
			camDist = 4.0f;
		}
		if (distance.x < 0) {
			distance.x = distance.x * -1;
		}
		if (distance.z < 0) {
			distance.z = distance.z * -1;
		}

        if (player1.position.x < (transform.position.x - bounds)) {
			Vector3 pos = player1.position;
			pos.x = transform.position.x - bounds;
			player1.position = pos;
		}

		if (player1.position.x >(transform.position.x + bounds)) {
			Vector3 pos = player1.position;
			pos.x = transform.position.x + bounds;
			player1.position = pos;
		}

		if (player2.position.x < (transform.position.x - bounds)) {
			Vector3 pos = player2.position;
			pos.x = transform.position.x - bounds;
			player2.position = pos;
		}

		if (player2.position.x > (transform.position.x + bounds)) {
			Vector3 pos = player2.position;
			pos.x = transform.position.x + bounds;
			player2.position = pos;
		}


		if (distance.x > 13.0f) {
			camOffset = distance.x * 0.3f;
			if (camOffset >= 8.5f) {
				camOffset = 8.5f;
			}
		} else if (distance.x <= 13.0f) {
			camOffset = distance.x * 0.3f;
		} else if (distance.z <= 13.0f) {
			camOffset = distance.x * 0.3f;
		}

		midX = (player2.position.x + player1.position.x) /2; 
		midY = (player2.position.y + player1.position.y) /2;
		midZ = (player2.position.z + player1.position.z) /2;

		midPoint = new Vector3 (midX, midY, midZ);

		Debug.Log ("midpoint " + midPoint);

		float cameraZ = Camera.main.gameObject.transform.position.z;

//		if(player1.position.z < cameraZ){
////			print ("player1 z less than cameraZ");
////			Debug.Log ("Before" + midZ);
//			midZ = player1.position.z - 10;
////			Debug.Log ("After" + midZ);
//
//		}
//		else if(player2.position.z < cameraZ){
////			print ("player2 z less than cameraZ");
////			Debug.Log ("Before" + midZ);
//			midZ = player2.position.z - 10;
////			Debug.Log ("After" + midZ);
//
//		}

//		Debug.Log ("cameraZ " + cameraZ + " PLAYER1Z " + player1.position.z);
//		Debug.Log ("MidZ" + midZ);

		if (player1) {
			Vector3 point = camera.WorldToViewportPoint(midPoint);
			Debug.Log (point);
			Vector3 delta = midPoint - camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, camDist + camOffset)); //(new Vector3(0.5, 0.5, point.z));
			Vector3 destination = transform.position + delta;
			transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
			//transform.position = Vector3.(transform.position, midPoint, smoothing);
		}

	}

}