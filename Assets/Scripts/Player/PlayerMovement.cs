using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum PlayerState
{
    IDLE,
    WALKING,
    CASTING,
    STUNNED
}

public class PlayerMovement : Photon.PunBehaviour
{
    public Transform model; // 3d model

    public Slider healthBar3D;
    public Text nameBar3D;

    private float defaultFriction = 5f; // friction drops when hit by spell
    private float moveForce = 40f;

    private Rigidbody rbody;
    private PlayerScript playerScript;

    public Animation anim; 
    public Transform playerMarker;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private PlayerState state = PlayerState.IDLE;
    private bool hasMovementOrder = false;
    private IEnumerator uncastCoroutine;
    private IEnumerator unstunCoroutine;
    private IEnumerator resetDragCoroutine;

    private Transform terrain;

    public Transform prefabParticlesOnClick;

    private float slowdownTime = 0f;
    private float cloakTime = 0f;

    // networking
    private Vector3 syncPosition;
    private Quaternion syncRotation;

    void Awake()
    {
        rbody = GetComponent<Rigidbody>();
        rbody.freezeRotation = true;
        rbody.drag = defaultFriction;

        playerScript = transform.GetComponent<PlayerScript>();

        terrain = GameObject.FindGameObjectWithTag("Terrain").transform;

        playerMarker.gameObject.SetActive(photonView.isMine);
    }

    void Update()
    {
        healthBar3D.value = Mathf.Lerp(healthBar3D.value, (int)photonView.owner.CustomProperties[PlayerProperties.HEALTH] / 100f, Time.deltaTime * 5f);

        if (state == PlayerState.IDLE)
            anim.CrossFade("free", 0.5f);
        if (state == PlayerState.WALKING)
            anim.CrossFade("walk", 0.5f);
        if (state == PlayerState.CASTING)
            anim.CrossFade("attack", 0.5f);
        if (state == PlayerState.STUNNED)
            anim.CrossFade("free", 0.5f);

        if (cloakTime > 0f)
        {
            model.gameObject.SetActive(false);
            healthBar3D.gameObject.SetActive(false);
            nameBar3D.gameObject.SetActive(false);
            cloakTime -= Time.deltaTime;
        }
        else
        {
            model.gameObject.SetActive(true);
            healthBar3D.gameObject.SetActive(true);
            nameBar3D.gameObject.SetActive(true);
        }

        if (!photonView.isMine)
        {
            transform.position = Vector3.Lerp(transform.position, syncPosition, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Slerp(transform.rotation, syncRotation, Time.deltaTime * 5f);
            return;
        }

        HandleInput();

        if (photonView.isMine && state == PlayerState.STUNNED)
        {
            transform.Rotate(Vector3.up * Time.deltaTime * 360 * 1.5f, Space.Self);
        }

        if (hasMovementOrder && DistanceToTarget() > 1f && state != PlayerState.STUNNED)
        {
            targetRotation = Quaternion.LookRotation(targetPosition - transform.position, Vector3.up);;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        if (slowdownTime > 0f)
            slowdownTime -= Time.deltaTime;

        if (DistanceToTarget() < 1f && state == PlayerState.WALKING)
            state = PlayerState.IDLE;
    }

    private void HandleInput()
    {
        if (Application.platform == RuntimePlatform.Android)
        { // ANDROID
            hasMovementOrder = false;
            if(state == PlayerState.WALKING)
                state = PlayerState.IDLE;

            if (Input.GetMouseButton(0) && state != PlayerState.CASTING && state != PlayerState.STUNNED)
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;

                if (playerScript.GetSpell() != null)
                    return;

                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (terrain.GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity))
                {
                    SetTargetPosition(hit.point);
                    hasMovementOrder = true;
                    rbody.drag = defaultFriction;
                    state = PlayerState.WALKING;
                }
            }
        }
        else if (Input.GetMouseButtonDown(1) && state != PlayerState.CASTING && state != PlayerState.STUNNED)
        { // PC
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (terrain.GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity))
            {
                SetTargetPosition(hit.point);
                hasMovementOrder = true;
                rbody.drag = defaultFriction;
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
    }

    void FixedUpdate()
    {
        if (!photonView.isMine)
            return;
        
        if (hasMovementOrder && state != PlayerState.STUNNED)
        {
            Vector3 force = targetPosition - transform.position;
            force.Normalize();

            if (DistanceToTarget() < 1f)
                force *= DistanceToTarget();

            if (slowdownTime > 0)
                force /= 2f;

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

    [PunRPC]
    public void SetSlowdown(float time)
    {
        slowdownTime = time;
    }

    [PunRPC]
    public void Cloak(float time)
    {
        cloakTime = time;
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
        if(state != PlayerState.STUNNED)
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
        if(state != PlayerState.STUNNED)
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
            stream.SendNext(transform.rotation);
            stream.SendNext(state);
        }
        else
        {
            Vector3 syncPos = (Vector3) stream.ReceiveNext();
            Quaternion syncRot = (Quaternion) stream.ReceiveNext();
            PlayerState syncState = (PlayerState) stream.ReceiveNext();

            syncPosition = syncPos;
            syncRotation = syncRot;
            state = syncState;
        }
    }

    void OnCollisionEnter(Collision c)
    {
        if (c.transform.tag == "Rock")
        {
            hasMovementOrder = false;
            if(state == PlayerState.WALKING)
                state = PlayerState.IDLE;
        }
    }
}
