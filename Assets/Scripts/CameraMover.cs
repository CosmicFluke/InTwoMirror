using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// SD note: this class copied from http://answers.unity3d.com/questions/912621/make-a-camera-to-follow-two-players.html
// it may need to be replaced to meet academic requirements

public class CameraMover : MonoBehaviour
{
    public enum MarginDirection { North,East,South,West }

    // Public variables to adjust camera movement

    public MarginDirection marginDirection; // The direction of the goals

    public float dampTime = 0.15f; // Speed of camera adjustment
    public Transform target;

    public float MidpointOffset; // Distance of camera to midpoint in +Z direction
    public float MarginOffset; // How large margin should be in marginDirection
    public float PullbackThreshold = 5; // Distance between player and cam before pullback is applied

    public Transform player1; //target1
    public Transform player2; //target2

    // Varaibles made public for debugging purposes

    public float midX;
    public float midY;
    public float midZ;

    public Vector3 midPoint; // the middle point between players
    public Vector3 distance; // distance between players, for tracking only, can be deleted

    // Private variables

    private Vector3 velocity = Vector3.zero;

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
            Debug.Log("Player distance = " + distance);
            Debug.Log("Midpoint to Cam distance: " + (cam.transform.position - midPoint));
        }

        if (distance.x < 0) // invert the distance if they cross
        {
            distance.x = distance.x * -1;
        }

        if (distance.z < 0) // invert the distance if they cross
        {
            distance.z = distance.z * -1;
        }

        midX = (player2.position.x + player1.position.x) / 2;
        midY = (player2.position.y + player1.position.y) / 2;
        midZ = (player2.position.z + player1.position.z) / 2;

        midPoint = new Vector3(midX, midY, midZ);

        if (player1)
        {
            Vector3 delta;
            delta = midPoint;


            // Get player closest to camera
            Transform closestPlayer;
            if ((player1.transform.position - cam.transform.position).z < (player2.transform.position - cam.transform.position).z)
                closestPlayer = player1.transform;
            else
                closestPlayer = player2.transform;

            Vector3 destination = transform.position;
            
            // Hook camera Z position to closestPlayer
            destination.z = midPoint.z - MidpointOffset;
            destination.x = midPoint.x;

            // If there's a player behind the pullback threshold, base camera Z pos on that player
            if ((closestPlayer.position - transform.position).z <= PullbackThreshold)
            {
                destination.z = closestPlayer.position.z - PullbackThreshold;
            }

            // Apply Margin Offset
            if (marginDirection == MarginDirection.North)
                destination.z += MarginOffset;
            if (marginDirection == MarginDirection.South)
                destination.z -= MarginOffset;
            if (marginDirection == MarginDirection.East)
                destination.x -= MarginOffset;
            if (marginDirection == MarginDirection.West)
                destination.x += MarginOffset;

            transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
        }

    }

    public void SetPlayers(Transform p1, Transform p2)
    {
        player1 = p1;
        player2 = p2;
    }

}