using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubulle : MonoBehaviour
{
    public Vector3 position;    // position de la particule
    public Vector3 velocity;    // vitesse de la particule
    public float density;       // densité de la particule
    public float pressure;      // pression de la particule
    public Vector3 force;       // force s'appliquant sur la particule

    public Bubulle(Vector3 pos)
    {
        position = pos;
        velocity = Vector3.zero;
        density = 1.0f;
        pressure = 0.0f;
        force = Vector3.zero;
    }

    private void Update()
    {
        //gameObject.transform.position = position;
    }
}
