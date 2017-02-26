using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Modified from code found on Unity forums, contributed by user 'mptp' (http://answers.unity3d.com/users/379201/mptp.html) on Oct 16, 2014 
 * Thread URL: http://answers.unity3d.com/questions/24785/how-to-use-the-xbox-360-controller-d-pad-pc-.html
 */
public class DPadButtons : MonoBehaviour
{
    public static bool up;
    public static bool down;
    public static bool left;
    public static bool right;

    float lastX;
    float lastY;

    void Start()
    {
        up = down = left = right = false;
        lastX = Input.GetAxis("DPadX");
        lastY = Input.GetAxis("DPadY");
    }

    void Update()
    {
        if (Input.GetAxis("DPadX") == 1 && lastX != 1) { right = true; } else { right = false; }
        if (Input.GetAxis("DPadX") == -1 && lastX != -1) { left = true; } else { left = false; }
        if (Input.GetAxis("DPadY") == 1 && lastY != 1) { up = true; } else { up = false; }
        if (Input.GetAxis("DPadY") == -1 && lastY != -1) { down = true; } else { down = false; }
        lastX = Input.GetAxis("DPadX");
        lastY = Input.GetAxis("DPadY");
    }
}