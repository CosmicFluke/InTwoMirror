using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// SD note: this class copied from http://answers.unity3d.com/questions/912621/make-a-camera-to-follow-two-players.html
// it may need to be replaced to meet academic requirements

public class CameraMover : MonoBehaviour
{

    public float dampTime = 0.15f;
    private Vector3 velocity = Vector3.zero;
    public Transform target;

    public float midX;
    public float midY;
    public float midZ;

    public Transform player1; //target1
    public Transform player2; //target2

    public Vector3 midPoint;
    public Vector3 distance;
    public float camDist;

    public float camOffset;
    public float bounds;

    private Vector3 pullBack = Vector3.zero;
    private bool allowPause = false;
    private bool isCameraZLocked = false;
    private float cameraZLock;

    Camera cam;


    void Awake() {
    }


    void Start()
    {
        camDist = 9.0f;
        bounds = 12.0f;
        cam = GetComponent<Camera>();
    }

    void Update()
    {

        distance = player1.position - player2.position;
        // TODO: Delete this debug code
        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    Debug.Log("P1 viewport = " + cam.WorldToViewportPoint(player1.position));
        //    Debug.Log("P1 cam diff = " + (transform.position - player1.position));
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    Debug.Log("P2 viewport = " + cam.WorldToViewportPoint(player2.position));
        //    Debug.Log("P2 cam diff = " + (transform.position - player2.position));
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha3))
        //{
        //    Debug.Log("viewportToWorldpoint = " + cam.ViewportToWorldPoint(new Vector3(0.5f, 0.2f, camDist + camOffset)));
        //    allowPause = !allowPause;
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha4))
        //{
        //    Debug.Log("InverseTransformDirection = " + transform.InverseTransformDirection(pullBack));
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha5))
        //{
        //    Debug.Log("Player distance = " + distance);
        //}

        // Check P2 still in frame
        Vector3 viewportP1 = cam.WorldToViewportPoint(player1.position);
        Vector3 viewportP2 = cam.WorldToViewportPoint(player2.position);
        if ((transform.position - player1.position).z >= -5)
        {
            pullBack.z = (transform.position - player1.position).z - 5f;
            Debug.Log("<color=blue>Pulling back for player1. pullback = " + pullBack + "</color>");
        } 

        if ((transform.position - player2.position).z >= -5)
        {
            pullBack.z = (transform.position - player2.position).z - 5f;
            Debug.Log("<color=blue>Pulling back for player2. pullback = " + pullBack + "</color>");
        }



        if (camDist >= 19.0f)
        {
            camDist = 19.0f;
        }
        if (camDist <= 6.0f)
        {
            camDist = 6.0f;
        }
        if (distance.x < 0)
        {
            distance.x = distance.x * -1;
        }
        if (distance.z < 0)
        {
            distance.z = distance.z * -1;
        }

        if (player1.position.x < (transform.position.x - bounds))
        {
            Vector3 pos = player1.position;
            pos.x = transform.position.x - bounds;
            player1.position = pos;
        }

        if (player1.position.x > (transform.position.x + bounds))
        {
            Vector3 pos = player1.position;
            pos.x = transform.position.x + bounds;
            player1.position = pos;
        }

        if (player2.position.x < (transform.position.x - bounds))
        {
            Vector3 pos = player2.position;
            pos.x = transform.position.x - bounds;
            player2.position = pos;
        }

        if (player2.position.x > (transform.position.x + bounds))
        {
            Vector3 pos = player2.position;
            pos.x = transform.position.x + bounds;
            player2.position = pos;
        }

        if (distance.x > 15.0f)
        {
            camOffset = distance.x * 0.9f; // 90% of x difference
            if (camOffset >= 8.5f)
            {
                camOffset = 8.5f;
            }
        }
        else if (distance.x <= 13.0f)
        {
            camOffset = distance.x * 0.9f;
        }
        else if (distance.z <= 13.0f)
        { // if they're too close
            Debug.Log("<color=blue>Resetting camera offset & Unlocking Z axis: They're close!</color>");
            camOffset = distance.x * 0.9f;
            pullBack = Vector3.zero;
        }

        if (distance.z >= 19.0f)
        { // if they're too far
            Debug.Log("<color=blue>Locking camera Z axis: They're far apart!</color>");
            isCameraZLocked = true;
            cameraZLock = transform.position.z;
        }
        else
        {
            isCameraZLocked = false;
        }

        midX = (player2.position.x + player1.position.x) / 2;
        midY = (player2.position.y + player1.position.y) / 2;
        midZ = (player2.position.z + player1.position.z) / 2;

        midPoint = new Vector3(midX, midY, midZ);

        if (player1)
        {
            Vector3 delta;
            delta = midPoint - cam.ViewportToWorldPoint(new Vector3(0.5f, 0.2f, camDist + camOffset)) + transform.InverseTransformDirection(pullBack);

            Vector3 destination = transform.position + delta;
            if (isCameraZLocked)
            {
                destination.z = cameraZLock;
            }
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
        }

    }

}