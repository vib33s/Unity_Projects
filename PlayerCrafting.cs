using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCrafting : MonoBehaviour
{
    [SerializeField] private LayerMask interactLayerMask;
    [SerializeField] private Transform Camera;
    float interactDist = 40f;

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Z))
        {
            RaycastHit hit;

            
            if (Physics.Raycast(Camera.position, Camera.TransformDirection(Vector3.forward), out hit, interactDist, interactLayerMask))
            {
                
                if(hit.transform.TryGetComponent(out CraftingAnvil craftingAnvil))
                {
                    craftingAnvil.Craft();
                }
            }
        }
    }
}
