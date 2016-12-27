using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum PlayerState
{
    IDLE,
    WALKING,
    CASTING,
    STUNNED
}

public class PlayerMovement : Photon.PunBehaviour {
    private float defaultFriction = 5f; // friction drops when hit by spell
    private float moveForce = 40f;

    private Rigidbody rbody;
    private PlayerScript playerScript;

    public Animation anim;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private PlayerState state = PlayerState.IDLE;
    private bool hasMovementOrder = false;
    private IEnumerator uncastCoroutine;
    private IEnumerator unstunCoroutine;
    private IEnumerator resetDragCoroutine;

    private Transform terrain;

    public Transform prefabParticlesOnClick;

    // networking
    private Vector3 syncPosition;
    private Vector3 syncVelocity;
    private Quaternion syncRotation;

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
        if (state == PlayerState.IDLE)
            anim.CrossFade("free", 0.5f);
        if (state == PlayerState.WALKING)
            anim.CrossFade("walk", 0.5f);
        if (state == PlayerState.CASTING)
            anim.CrossFade("attack", 0.5f);
        if (state == PlayerState.STUNNED)
            anim.CrossFade("free", 0.5f);

        if (!photonView.isMine)
        {
            transform.position = Vector3.Lerp(transform.position, syncPosition, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Slerp(transform.rotation, syncRotation, Time.deltaTime * 5f);
            return;
        }

        if (Input.GetMouseButtonDown(1) && state != PlayerState.CASTING && state != PlayerState.STUNNED)
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (terrain.GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity))
            {
                SetTargetPosition(hit.point);
                hasMovementOrder = true;
                GameObject.Instantiate(prefabParticlesOnClick, hit.point + Vector3.up * 0.1f, Quaternion.identity);
                state = PlayerState.WALKING;
                playerScript.SetSpell(null);
            }
        }

        if (Input.GetMouseButtonDown(1) && state == PlayerState.CASTING)
        {
            playerScript.CancelCast();
            state = PlayerState.IDLE;
        }

        if (hasMovementOrder && DistanceToTarget() > 1f)
        {
            targetRotation = Quaternion.LookRotation(targetPosition - transform.position, Vector3.up);;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        if (DistanceToTarget() < 1f && state == PlayerState.WALKING)
            state = PlayerState.IDLE;
    }

    void FixedUpdate()
    {
        if (!photonView.isMine)
            return;
        
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
        if (unstunCoroutine != null)
            StopCoroutine(unstunCoroutine);

        hasMovementOrder = false;

        state = PlayerState.STUNNED;
        unstunCoroutine = Unstun(time);
        StartCoroutine(unstunCoroutine);
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

    public void SetDrag(float dragValue, float time)
    {
        if (resetDragCoroutine != null)
            StopCoroutine(resetDragCoroutine);

        rbody.drag = dragValue;
        resetDragCoroutine = ResetDrag(time);
        StartCoroutine(resetDragCoroutine);
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

    public IEnumerator ResetDrag(float time)
    {
        yield return new WaitForSeconds(time);
        rbody.drag = defaultFriction;
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

    public Rigidbody GetRigidbody()
    {
        return rbody;
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(rbody.velocity);
            stream.SendNext(transform.rotation);
            stream.SendNext(state);
        }
        else
        {
            Vector3 syncPos = (Vector3) stream.ReceiveNext();
            Vector3 syncVel = (Vector3) stream.ReceiveNext();
            Quaternion syncRot = (Quaternion) stream.ReceiveNext();
            PlayerState syncState = (PlayerState) stream.ReceiveNext();

            syncPosition = syncPos;
            syncVelocity = syncVel;
            syncRotation = syncRot;

            //transform.position = syncPos;
            state = syncState;
        }
    }
}
