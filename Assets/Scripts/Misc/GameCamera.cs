using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    private Transform myPlayer;
    private Vector3 offset;
    Vector3 shakeOffset = Vector3.zero;
    private Vector3 camPos;

    private float speed = 10f;
    private float mouseDeadzone = 0.05f;

    public float minX = -10f, maxX = 10f, minZ = -10f, maxZ = 10f;

    void Start()
    {
        offset = camPos = transform.position;
    }

    void LateUpdate()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView - scroll * 20, 35, 70);

        // mousePos is a vector from 0 to 1 for each axis
        Vector2 mousePos = Input.mousePosition;
        mousePos.x /= Screen.width;
        mousePos.y /= Screen.height;

        if (mousePos.x < mouseDeadzone)
        { // move left
            camPos += Vector3.left * speed * Time.deltaTime;
        }

        if (mousePos.x > 1f - mouseDeadzone)
        { // move right
            camPos += Vector3.right * speed * Time.deltaTime;
        }

        if (mousePos.y < mouseDeadzone)
        { // move back
            camPos += Vector3.back * speed * Time.deltaTime;
        }

        if (mousePos.y > 1f - mouseDeadzone)
        { // move forward
            camPos += Vector3.forward * speed * Time.deltaTime;
        }
      
        camPos.x = Mathf.Clamp(camPos.x, minX + offset.x, maxX + offset.x);
        camPos.z = Mathf.Clamp(camPos.z, minZ + offset.z, maxZ + offset.z);

        transform.position = camPos;
        transform.position += shakeOffset;
    }

    public IEnumerator Shake(float intensity, float time)
    {
        float timePassed = 0f;
        while (timePassed <= time)
        {
            shakeOffset = Random.insideUnitSphere;
            shakeOffset *= intensity;

            yield return new WaitForEndOfFrame();
            timePassed += Time.deltaTime;
        }

        shakeOffset = Vector3.zero;
    }
}
