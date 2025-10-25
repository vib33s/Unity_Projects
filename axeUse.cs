using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class axeUse : MonoBehaviour
{
    public Transform cam;
    public GameObject player;
    public LayerMask treeMask;
    public Transform heldItem;
    public weaponPickup wp;
    public GameObject treeHitObj;
    public Animation anim;
    public AnimationClip swing;

    void Awake()
    {
        wp = player.GetComponent<weaponPickup>();
        anim = GetComponent<Animation>();
    }


    void Update()
    {
        Debug.DrawRay(cam.transform.position, cam.transform.TransformDirection(Vector3.forward), Color.green);

        if (Input.GetKeyDown(KeyCode.Mouse0) && wp.isAxe == true) {
            Debug.Log("Here");
            if (Physics.Raycast(cam.transform.position, cam.transform.TransformDirection(Vector3.forward), out RaycastHit HitInfo, 2f, treeMask)) {
                treeHitObj = HitInfo.transform.gameObject;
                anim.clip = swing;
                anim.Play();
                StartCoroutine(wait());
            }
            else
            {
                anim.clip = swing;
                anim.Play();
                StartCoroutine(waitNoHit());
            }
        }
    }

    IEnumerator wait()
    {
        yield return new WaitForSeconds(1.5f);
        treeHitObj.GetComponent<DestroyTree>().doBreak();
    }
    IEnumerator waitNoHit()
    {
        yield return new WaitForSeconds(1.5f);
    }
}
