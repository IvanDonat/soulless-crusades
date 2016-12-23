using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    private float defaultFriction = 5f; // friction drops when hit by spell
    private float frictionRecoveryFactor = 0.2f;
    private float moveForce = 40f;

    private Rigidbody rbody;
    private Vector3 targetPosition;

    private bool isStunned = false;

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
        rbody.drag = Mathf.Lerp(rbody.drag, defaultFriction, frictionRecoveryFactor);

        targetPosition.y = transform.position.y; // top-down doesn't matter
        float distToTarget = (targetPosition - transform.position).magnitude;

        if (!isStunned)
        {
            Vector3 force = targetPosition - transform.position;
            force.Normalize();

            if (distToTarget < 1f)
                force *= distToTarget;

            rbody.AddForce(force * moveForce, ForceMode.Acceleration);
        }
    }

    public void SetTargetPosition(Vector3 pos)
    {
        targetPosition = pos;
    }

    public void Stun(float time)
    {
        isStunned = true;
        StartCoroutine("Unstun", time);
    }

    private IEnumerator Unstun(float time)
    {
        yield return new WaitForSeconds(time);
        isStunned = false;
    }
}
