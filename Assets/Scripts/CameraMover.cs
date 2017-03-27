using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// SD note: this class copied from http://answers.unity3d.com/questions/912621/make-a-camera-to-follow-two-players.html
// it may need to be replaced to meet academic requirements

public class CameraMover : MonoBehaviour
{

    // SD commented out as not required for InTwo public static CameraMover cTrack; //cFollow
    public float dampTime = 0.15f; //figure out what this is
    private Vector3 velocity = Vector3.zero;
    public Transform target;

    public float midX;
    public float midY;
    public float midZ;

    public Transform player1; //target1
    public Transform player2; //target2

    public Vector3 midPoint; // the middle point between players
    public Vector3 distance; // distance between players
    public float camDist = 9.0f; // distance of the camera from the middle point

    public float ZcamOffset; // offset applied to Z axis to camera from distance to middle point
    public float XcamOffset = 0; // offset applied to X axis to camera from distance to middle point
    public float bounds = 12.0f; // bounds before camera adjustment?

    private Vector3 pullBack = Vector3.zero;
    private bool allowPause = false;
    private bool isCameraZLocked = false;
    private bool isCameraYLocked = false;
    private float cameraZLock;
    private float cameraYLimit = 12f;

    Camera cam;


    void Awake()
    { //Startup of the game
      /*  SD commented out as unnecessary for InTwo
          if (cTrack == null) {
              DontDestroyOnLoad (gameObject);
              cTrack = this;

          } else if (cTrack != this) {
              Destroy (gameObject);
          }
          */
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
            Debug.Log("viewportToWorldpoint = " + cam.ViewportToWorldPoint(new Vector3(0.5f, 0.2f, camDist + ZcamOffset)));
            Debug.Log("InverseTransformDirection = " + transform.InverseTransformDirection(pullBack));
            Debug.Log("Player distance = " + distance);
            Debug.Log("Midpoint to Cam distance: " + (cam.transform.position - midPoint));
        }

        // Check P2 still in frame
        Vector3 viewportP1 = cam.WorldToViewportPoint(player1.position);
        Vector3 viewportP2 = cam.WorldToViewportPoint(player2.position);
        if ((transform.position - player1.position).z >= -5)
        {
            pullBack.z = (transform.position - player1.position).z - 5f;
            Debug.Log("Pulling back for Player 1. pullBack = " + pullBack);
            //Debug.Log("viewportToWorldpoint = " + cam.ViewportToWorldPoint(new Vector3(0.5f, 0.2f, camDist + camOffset)));
            //Debug.Log("InverseTransformDirection = " + transform.InverseTransformDirection(pullBack));
        }
        if((transform.position - player2.position).z >= -5)
        {
            pullBack.z = (transform.position - player2.position).z - 5f;
            Debug.Log("Pulling back for player2. pullBack = " + pullBack);
        }



        if (camDist >= 19.0f)
        {
            camDist = 19.0f;
        }
        if (camDist <= 6.0f)
        {
            camDist = 6.0f;
        }

        if (distance.x < 0) // invert the distance if they cross
        {
            distance.x = distance.x * -1;
        }

        if (distance.z < 0) // invert the distance if they cross
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

        if((cam.transform.position - midPoint).y <= cameraYLimit)
        {
            isCameraYLocked = true; // activate camera X lock if it's below threshold
        }
        if (distance.x > 15.0f)
        {
            ZcamOffset = distance.x * 0.9f; // 90% of x difference
            if (ZcamOffset >= 8.5f)
            {
                ZcamOffset = 8.5f;
            }
        }
        else if (distance.x <= 13.0f)
        {
            ZcamOffset = distance.x * 0.9f;
        }
        else if (distance.z <= 13.0f)
        { // if they're too close
            ZcamOffset = distance.x * 0.9f;
            pullBack = Vector3.zero;
        }

        if (distance.z >= 19.0f)
        { // if they're too far
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
            delta = midPoint - cam.ViewportToWorldPoint(new Vector3(0.5f, 0.2f + XcamOffset, camDist + ZcamOffset)) + transform.InverseTransformDirection(pullBack);
            Vector3 destination = transform.position + delta;

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