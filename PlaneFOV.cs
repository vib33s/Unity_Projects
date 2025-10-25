using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneFOV : MonoBehaviour
{
    public Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.LeftShift))
        {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, 100, 10f * Time.deltaTime);
        }
        else
        {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, 90, 10f * Time.deltaTime);
        }
    }
}
