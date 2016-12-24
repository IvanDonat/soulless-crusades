using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    IDLE,
    WALKING,
    CASTING,
    STUNNED
}

public class PlayerMovement : MonoBehaviour {
    private float defaultFriction = 5f; // friction drops when hit by spell
    private float frictionRecoveryFactor = 0.2f;
    private float moveForce = 40f;

    private Rigidbody rbody;
    private PlayerScript playerScript;

    public Animation anim;

    private Vector3 targetPosition;
    private PlayerState state = PlayerState.IDLE;
    private bool hasMovementOrder = false;
    private IEnumerator uncastCoroutine;

    private Transform terrain;

    public Transform prefabParticlesOnClick;

    void Awake()
    {
        rbody = GetComponent<Rigidbody>();
        rbody.freezeRotation = true;
        rbody.drag = defaultFriction;

        playerScript = transform.GetComponent<PlayerScript>();

        terrain = GameObject.FindGameObjectWithTag("Terrain").transform;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && state != PlayerState.CASTING && state != PlayerState.STUNNED)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (terrain.GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity))
            {
                SetTargetPosition(hit.point);
                hasMovementOrder = true;
                GameObject.Instantiate(prefabParticlesOnClick, hit.point + Vector3.up * 0.1f, Quaternion.identity);
                state = PlayerState.WALKING;
            }
        }

        if (Input.GetMouseButtonDown(1) && state == PlayerState.CASTING)
        {
            playerScript.CancelCast();
            state = PlayerState.IDLE;
        }

        if (hasMovementOrder && DistanceToTarget() > 1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(targetPosition - transform.position, Vector3.up);;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
        }

        if (DistanceToTarget() < 1f && state == PlayerState.WALKING)
            state = PlayerState.IDLE;

        if (state == PlayerState.IDLE)
            anim.CrossFade("free", 0.5f);
        if (state == PlayerState.WALKING)
            anim.CrossFade("walk", 0.5f);
        if (state == PlayerState.CASTING)
            anim.CrossFade("attack", 0.5f);
        if (state == PlayerState.STUNNED)
            anim.CrossFade("free", 0.5f);
    }

    void FixedUpdate()
    {
        rbody.drag = Mathf.Lerp(rbody.drag, defaultFriction, frictionRecoveryFactor);

        if (hasMovementOrder)
        {
            Vector3 force = targetPosition - transform.position;
            force.Normalize();

            if (DistanceToTarget() < 1f)
                force *= DistanceToTarget();

            rbody.AddForce(force * moveForce, ForceMode.Acceleration);
        }
    }

    public float DistanceToTarget()
    {
        targetPosition.y = transform.position.y; // top-down doesn't matter
        return (targetPosition - transform.position).magnitude;
    }

    public void SetTargetPosition(Vector3 pos)
    {
        targetPosition = pos;
    }

    public void Stun(float time)
    {
        hasMovementOrder = false;

        state = PlayerState.STUNNED;
        StartCoroutine("Unstun", time);
    }

    public void CastSpell(float time, Vector3 aimPos)
    {
        hasMovementOrder = false;

        aimPos.y = transform.position.y;
        transform.rotation = Quaternion.LookRotation(aimPos - transform.position, Vector3.up);

        state = PlayerState.CASTING;

        uncastCoroutine = Uncast(time);
        StartCoroutine(uncastCoroutine);
    }

    private IEnumerator Unstun(float time)
    {
        yield return new WaitForSeconds(time);
        state = PlayerState.IDLE;
    }

    private IEnumerator Uncast(float time)
    {
        yield return new WaitForSeconds(time);
        state = PlayerState.IDLE;
    }

    public void CancelCast()
    {
        if(uncastCoroutine != null)
            StopCoroutine(uncastCoroutine);
        state = PlayerState.IDLE;
    }

    public void SetState(PlayerState st)
    {
        this.state = st;
    }

    public PlayerState GetState()
    {
        return state;
    }
}
