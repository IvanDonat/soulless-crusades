﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCamera : MonoBehaviour
{
    enum TransitionStage
    {
        FIRST,    // makne se unazad
        SECOND,   // ode do canvasa
        THIRD     // uđe u canvas
    };

    private TransitionStage currStage = TransitionStage.FIRST;
    private bool inTransition = false;
    private Vector3 targetPosition;

    private float stageSecondSmoothTime = .15f;
    private float xVelocity, yVelocity, zVelocity;

    public Transform canvasLogin;
    public Transform canvasMenu;
    public Transform canvasLobby;
    public Transform canvasCreateRoom;
    public Transform canvasOptions;
    public Transform canvasCredits;
    public Transform canvasTutorial;

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
                    Mathf.Lerp(transform.position.z, offsetFromCanvas.z * 2, Time.deltaTime * 5));

                transform.position = posToGo;
                if (transform.position.z < (offsetFromCanvas * 1.8f).z)
                    currStage = TransitionStage.SECOND;
            }

            if (currStage == TransitionStage.SECOND) 
            {
                Vector3 posToGo = new Vector3(Mathf.SmoothDamp(transform.position.x, targetPosition.x, ref xVelocity, stageSecondSmoothTime),
                    Mathf.SmoothDamp(transform.position.y, targetPosition.y, ref yVelocity, stageSecondSmoothTime),
                    transform.position.z);

                transform.position = posToGo;

                if (Mathf.Abs(transform.position.x - targetPosition.x) < .6f && Mathf.Abs(transform.position.y - targetPosition.y) < .6f)
                    currStage = TransitionStage.THIRD;
            }

            if (currStage == TransitionStage.THIRD)
            {
                Vector3 posToGo = Vector3.Lerp(transform.position, targetPosition + offsetFromCanvas, Time.deltaTime * 5);

                transform.position = posToGo;

                if (Mathf.Abs(transform.position.z - targetPosition.z) < 2f)
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

    public void TransitionToLogin()
    {
        TransitionTo(canvasLogin);
    }

    public void TransitionToCredits()
    {
        TransitionTo(canvasCredits);
    }

    public void TransitionToTutorial()
    {
        TransitionTo(canvasTutorial);
    }

    internal void TransitionToOptions()
    {
        TransitionTo(canvasOptions);
    }
}
