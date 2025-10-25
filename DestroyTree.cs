using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTree : MonoBehaviour
{
    public GameObject applyRigidbodyObject;
    public GameObject spawnItem;
    public Transform targetSpawnArea;
    public LayerMask destroyMask;
    public float waitCooldown = 0.2f;

    void Update()
    {

        
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.layer);
        if (collision.gameObject.layer == 6)
        {
            Destroy(applyRigidbodyObject);
            Instantiate(spawnItem, targetSpawnArea.position, targetSpawnArea.rotation);
            Instantiate(spawnItem, targetSpawnArea.position, targetSpawnArea.rotation);
            Instantiate(spawnItem, targetSpawnArea.position, targetSpawnArea.rotation);
            spawnItem.GetComponent<Rigidbody>().AddExplosionForce(2f, targetSpawnArea.position, 1f, 1f);
        }
    }

    public void doBreak()
    {
        applyRigidbodyObject.AddComponent<Rigidbody>();
        applyRigidbodyObject.GetComponent<Rigidbody>().mass = 40000f;
        //applyRigidbodyObject.GetComponent<Rigidbody>().AddForce(transform.forward * 1f);
        Debug.Log("Here");
    }



}
