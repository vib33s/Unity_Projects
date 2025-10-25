using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawner : MonoBehaviour
{
    private Vector3 startPos;

    private Quaternion startRotation;

    private Rigidbody rb;


    void Start()
    {
        startPos = transform.position;
        startRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "DeathPlane")
        {
            transform.position = startPos;
            transform.rotation = startRotation;
            rb.velocity = new Vector3(0, 0, 0);
        }
    }
}
