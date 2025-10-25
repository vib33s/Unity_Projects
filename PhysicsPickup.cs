using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsPickup : MonoBehaviour
{
    [SerializeField] private LayerMask PickupMask;
    [SerializeField] private LayerMask weaponMask;
    [SerializeField] private Transform[] weaponSlots;
    [SerializeField] private Camera PlayerCam;
    [SerializeField] private Transform PickupTarget;
    [Space]
    [SerializeField] private float PickupRange;
    public Transform camTransform;
    private Rigidbody CurrentObject;
    private int useThisIndex;
    public SwingScript ss;
    public Grappling gg;


    void Start()
    {
        ss = GetComponent<SwingScript>();
        gg = GetComponent<Grappling>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (CurrentObject)
            {
                CurrentObject.useGravity = true;
                CurrentObject.angularDrag = 0;
                CurrentObject = null;
                return;
            }

            Ray CameraRay = PlayerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(CameraRay, out RaycastHit HitInfo, PickupRange, PickupMask))
            {
                CurrentObject = HitInfo.rigidbody;
                CurrentObject.useGravity = false;
                CurrentObject.angularDrag = 10;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q) && CurrentObject)
        {
            CurrentObject.useGravity = true;
            CurrentObject.angularDrag = 0;
            CurrentObject.AddForce(camTransform.forward * 22.5f, ForceMode.Impulse);
            Debug.Log("Yay");
            CurrentObject = null;
            return;
        }
    }

    void FixedUpdate()
    {
        if (CurrentObject)
        {
            // ss.StopSwing();
            // gg.StopGrapple();
            Vector3 DirectionToPoint = PickupTarget.position - CurrentObject.position;
            float DistanceToPoint = DirectionToPoint.magnitude;

            CurrentObject.velocity = DirectionToPoint * 12f * DistanceToPoint;
        }
    }
}
