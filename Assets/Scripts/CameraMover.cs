using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// SD note: this class copied from http://answers.unity3d.com/questions/912621/make-a-camera-to-follow-two-players.html
// it may need to be replaced to meet academic requirements

public class CameraMover : MonoBehaviour
{
    public enum MarginDirection { North,East,South,West }

    public MarginDirection marginDirection;

    public float midX;
    public float midY;
    public float midZ;

    public float dampTime = 0.15f; //figure out what this is
    private Vector3 velocity = Vector3.zero;
    public Transform target;

    public float MidpointOffset;
    public float MarginOffset;


    public Transform player1; //target1
    public Transform player2; //target2

    public Vector3 midPoint; // the middle point between players
    public Vector3 distance; // distance between players
    public float camDist = 9.0f; // distance of the camera from the middle point

    public float ZCamOffset; // offset applied to Z axis to camera from distance to middle point
    public float XCamOffset = 0; // offset applied to X axis to camera from distance to middle point
    public float bounds = 12.0f; // bounds before camera adjustment?

    public float PullbackThreshold = 5;

    private Vector3 pullBack = Vector3.zero;
    private bool allowPause = false;
    private bool isCameraZLocked = false;
    private bool isCameraYLocked = false;
    private float cameraZLock;
    private float cameraYLimit = 12f;

    private bool p1PullBack = false;
    private bool p2PullBack = false;

    Camera cam;


    void Awake()
    {
    }


    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        if (player1 == null || player2 == null) return;
        distance = player1.position - player2.position;
        // TODO: Delete this debug code
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Debug.Log("<color=blue>CameraMover Debug</color>");
            Debug.Log("P1 viewport = " + cam.WorldToViewportPoint(player1.position));
            Debug.Log("P1 cam diff = " + (transform.position - player1.position));
            Debug.Log("P2 viewport = " + cam.WorldToViewportPoint(player2.position));
            Debug.Log("P2 cam diff = " + (transform.position - player2.position));
            Debug.Log("viewportToWorldpoint = " + cam.ViewportToWorldPoint(new Vector3(0.5f, 0.2f, camDist + ZCamOffset)));
            Debug.Log("InverseTransformDirection = " + transform.InverseTransformDirection(pullBack));
            Debug.Log("Player distance = " + distance);
            Debug.Log("Midpoint to Cam distance: " + (cam.transform.position - midPoint));
        }

        //// Pullback if player is far from camera

        //p1PullBack = (transform.position - player1.position).z >= -PullbackThreshold;
        //p2PullBack = (transform.position - player2.position).z >= -PullbackThreshold;

        //if (p1PullBack && p2PullBack) // pullback for both players
        //{
        //    pullBack.z = (transform.position - player1.position).z - PullbackThreshold;
        //    Debug.Log("Pulling back for Player 1. pullBack = " + pullBack);
        //    Debug.Log("PullbackThresh = " + PullbackThreshold);
        //}
        //else if (p1PullBack)
        //{
        //    pullBack.z = (transform.position - player1.position).z - PullbackThreshold;
        //    Debug.Log("Pulling back for Player 1. pullBack = " + pullBack);
        //    //Debug.Log("viewportToWorldpoint = " + cam.ViewportToWorldPoint(new Vector3(0.5f, 0.2f, camDist + camOffset)));
        //    //Debug.Log("InverseTransformDirection = " + transform.InverseTransformDirection(pullBack));
        //}
        //else if (p2PullBack)
        //{
        //    pullBack.z = (transform.position - player2.position).z - PullbackThreshold;
        //    Debug.Log("Pulling back for player2. pullBack = " + pullBack);
        //}


        if (distance.x < 0) // invert the distance if they cross
        {
            distance.x = distance.x * -1;
        }

        if (distance.z < 0) // invert the distance if they cross
        {
            distance.z = distance.z * -1;
        }


        //// Zoffset check for pullback

        //if (distance.x > 15.0f)
        //{
        //    ZCamOffset = distance.x * 0.9f; // 90% of x difference
        //    if (ZCamOffset >= 8.5f)
        //    {
        //        ZCamOffset = 8.5f;
        //    }
        //}
        //else if (distance.x <= 13.0f)
        //{
        //    ZCamOffset = distance.x * 0.9f;
        //}
        //else if (distance.z <= 13.0f)
        //{ // if they're too close
        //    ZCamOffset = distance.x * 0.9f;
        //    pullBack = Vector3.zero;
        //}


        //// Camera Locks

        //// activate camera X lock if it's below threshold
        //if ((cam.transform.position - midPoint).y <= cameraYLimit)
        //{
        //    isCameraYLocked = true;
        //}
        //if (distance.z >= 19.0f)
        //{ // if they're too far
        //    isCameraZLocked = true;
        //    cameraZLock = transform.position.z;
        //}
        //else
        //{
        //    isCameraZLocked = false;
        //}

        midX = (player2.position.x + player1.position.x) / 2;
        midY = (player2.position.y + player1.position.y) / 2;
        midZ = (player2.position.z + player1.position.z) / 2;

        midPoint = new Vector3(midX, midY, midZ);

        if (player1)
        {
            Vector3 delta;
            delta = midPoint;
            //Vector3 destination = transform.position + delta;
            Vector3 destination = transform.position;
            destination.z = midPoint.z - MidpointOffset;
            destination.x = midPoint.x;

            if (marginDirection == MarginDirection.North || marginDirection == MarginDirection.South)
                destination.z += MarginOffset;
            if (marginDirection == MarginDirection.East)
                destination.x += MarginOffset;
            if (marginDirection == MarginDirection.West)
                destination.x -= MarginOffset;

            // Apply axis locks
            if (isCameraZLocked)
                destination.z = cameraZLock;
            if (isCameraYLocked)
                destination.y = midPoint.y + cameraYLimit;

            transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
        }

    }

    public void SetPlayers(Transform p1, Transform p2)
    {
        player1 = p1;
        player2 = p2;
    }

}