using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubulle : MonoBehaviour
{
    public Vector3 position;    // position de la particule
    public Vector3 velocity;    // force s'appliquant sur la particule
    public Vector3 force;       // vitesse de la particule
    public float density;       // densité de la particule
    public float pressure;      // pression de la particule
    [HideInInspector]public Rigidbody rigidbody;

    private void Awake()
    {
        rigidbody = this.GetComponent<Rigidbody>();
        rigidbody.mass = 1.0f;
    }

    public Bubulle(Vector3 pos)
    {
        position = pos;
        //velocity = Vector3.zero;
        density = 1.0f;
        pressure = 0.0f;
        force = Vector3.zero;
    }

    private void Update()
    {
        //position = gameObject.transform.position;
    }

    private void OnCollisionEnter(Collision other)
    {
        float coefDeRestitution = 0.5f;
        velocity *= -coefDeRestitution;
    }

    /*private void OnTriggerEnter(Collider other)
    {
        float coefDeRestitution = 0.5f;
        velocity *= -coefDeRestitution;
    }*/

    
    
}
