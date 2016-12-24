using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour {
    private Transform myPlayer;
    Vector3 offset = new Vector3(0, 25, -25);

    private float speed = 10f;
    private float mouseDeadzone = 0.05f;

    public float minX = -10f, maxX = 10f, minZ = -10f, maxZ = 10f;

    void Start()
    {
        transform.position = offset;
    }

    void LateUpdate()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView - scroll * 20, 35, 70);

        // mousePos is a vector from 0 to 1 for each axis
        Vector2 mousePos = Input.mousePosition;
        mousePos.x /= Screen.width;
        mousePos.y /= Screen.height;

        Vector3 newPos = transform.position;

        if (mousePos.x < mouseDeadzone)
        { // move left
            newPos += Vector3.left * speed * Time.deltaTime;
        }

        if (mousePos.x > 1f - mouseDeadzone)
        { // move right
            newPos += Vector3.right * speed * Time.deltaTime;
        }

        if (mousePos.y < mouseDeadzone)
        { // move back
            newPos += Vector3.back * speed * Time.deltaTime;
        }

        if (mousePos.y > 1f - mouseDeadzone)
        { // move forward
            newPos += Vector3.forward * speed * Time.deltaTime;
        }
      
        newPos.x = Mathf.Clamp(newPos.x, minX + offset.x, maxX + offset.x);
        newPos.y = transform.position.y;
        newPos.z = Mathf.Clamp(newPos.z, minZ + offset.z, maxZ + offset.z);

        transform.position = newPos;
    }
}
