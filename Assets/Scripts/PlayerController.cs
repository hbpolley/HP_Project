using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public enum Axel
    {
        Front,
        Rear
    }

    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public Axel axel;
    }
    
    //variable accaleration and braking
    public float maxAccelaration = 100.0f;
    public float brakeAcceleration = 50.0f;
    //steering
    public float turnSensitivity = 1.0f;
    public float maxSteerAngle = 40.0f;
    //speed and brake strength
    public float topSpeed = 600.0f;
    public float brakeStrength = 200f;

    //anti-roll prevention
    public Vector3 antiRoll;

    //for unity editor
    public List<Wheel> wheels;

    float moveInput;
    float steerInput;

    private Rigidbody car;

    public Transform spawnPoint;

    public void ResetCar()
    {
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
    }

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        car = GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        movement();
        steer();
        brake();
        //i hate american spelling
        car.centerOfMass = antiRoll;
    }
    // Update is called once per frame
    void Update()
    {
        InputManagement();
    }

    //input manager. uses the old system, DO NOT UPDATE TO MODERN, BREAKS INPUTS
    void InputManagement()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }

    //movement controls
    void movement()
    {
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = moveInput * topSpeed * maxAccelaration * Time.deltaTime;
        }
        //Vector3 move = new Vector3(turnInput, 0, moveInput);
        //move.y = 0;
        //move *= topSpeed;
        //controller.Move(move * Time.deltaTime);
    }

    //steering controls
    void steer()
    {
        foreach(var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                var _steerAngle = steerInput * turnSensitivity * maxSteerAngle;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f);
            }
        }
    }

    void brake()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            foreach(var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = brakeStrength * brakeAcceleration * Time.deltaTime;
            }
        } 
        else 
        {
            foreach(var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 0;
            }
        }
    }
}

