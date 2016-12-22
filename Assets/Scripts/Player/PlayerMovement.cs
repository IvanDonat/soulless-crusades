using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    private float defaultFriction = 3f; // friction drops when hit by spell

    private Rigidbody rbody;
    private Vector3 targetPosition;

    private Transform terrain;

    public Transform prefabParticlesOnClick;

    void Start()
    {
        rbody = GetComponent<Rigidbody>();
        rbody.freezeRotation = true;
        rbody.drag = defaultFriction;

        terrain = GameObject.FindGameObjectWithTag("Terrain").transform;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            // @TODO spawn particles at click pos

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (terrain.GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity))
            {
                SetTargetPosition(hit.point);

                GameObject.Instantiate(prefabParticlesOnClick, hit.point + Vector3.up * 0.1f, Quaternion.identity);
            }
        }

    }

    void FixedUpdate()
    {
        rbody.drag = Mathf.Lerp(rbody.drag, defaultFriction, Time.time);

        targetPosition.y = transform.position.y; // top-down doesn't matter
        float distToTarget = (targetPosition - transform.position).magnitude;

        if (distToTarget > 0.5f)
        { // apply force towards target
            Vector3 force = targetPosition - transform.position;
            force.Normalize();

            rbody.AddForce(force * 30, ForceMode.Acceleration);
        }
    }

    public void SetTargetPosition(Vector3 pos)
    {
        targetPosition = pos;
    }
}
