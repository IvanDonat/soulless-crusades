using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCamera : MonoBehaviour {
    enum TransitionStage
    {
        FIRST,    // makne se unazad
        SECOND,   // ode do canvasa
        THIRD     // uđe u canvas
    };

    private TransitionStage currStage = TransitionStage.FIRST;
    private bool inTransition = false;
    private Vector3 targetPosition;

    public Transform canvasMenu;
    public Transform canvasLobby;
    public Transform canvasCreateRoom;

    private Vector3 offsetFromCanvas = Vector3.back * 750;

    void Start()
    {
        //TransitionTo(canvasLobby);
    }

    void Update() {
        if (inTransition) 
        {
            if (currStage == TransitionStage.FIRST) 
            {
                Vector3 posToGo = new Vector3(transform.position.x, transform.position.y,
                    Mathf.Lerp(transform.position.z, offsetFromCanvas.z * 2, Time.deltaTime));

                transform.position = posToGo;
                if (transform.position.z < (offsetFromCanvas * 1.5f).z)
                    currStage = TransitionStage.SECOND;
            }

            if (currStage == TransitionStage.SECOND) 
            {
                Vector3 posToGo = new Vector3(Mathf.Lerp(transform.position.x, targetPosition.x, Time.deltaTime * 6),
                    Mathf.Lerp(transform.position.y, targetPosition.y, Time.deltaTime * 6),
                    transform.position.z);

                transform.position = posToGo;

                if (Mathf.Abs(transform.position.x - targetPosition.x) < 3 && Mathf.Abs(transform.position.y - targetPosition.y) < 3)
                    currStage = TransitionStage.THIRD;
            }

            if (currStage == TransitionStage.THIRD)
            {
                Vector3 posToGo = new Vector3(transform.position.x, transform.position.y,
                    Mathf.Lerp(transform.position.z, offsetFromCanvas.z, Time.deltaTime * 2));

                transform.position = posToGo;

                if (Mathf.Abs(transform.position.z - targetPosition.z) < 2)
                    inTransition = false;
            }
        }
    }

    public void TransitionTo(Transform t)
    {
        inTransition = true;
        currStage = TransitionStage.FIRST;

        targetPosition = t.position;
    }


    // methods for use by buttons
    public void TransitionToMainMenu()
    {
        TransitionTo(canvasMenu);
    }

    public void TransitionToLobby()
    {
        TransitionTo(canvasLobby);
    }

    public void TransitionToCreateRoom()
    {
        TransitionTo(canvasCreateRoom);
    }
}
