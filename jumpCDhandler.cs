using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jumpCDhandler : MonoBehaviour
{
    public PlayerMovement pm;

    // Start is called before the first frame update
    void Start()
    {
        pm = GetComponent<PlayerMovement>();
    }

    public void doCD()
    {
        StartCoroutine(jumpCD());
    }

    public IEnumerator jumpCD()
    {
        yield return new WaitForSeconds(1);

        pm.readyToJump = true;
    }
}
