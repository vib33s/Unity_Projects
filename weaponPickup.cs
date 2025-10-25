using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weaponPickup : MonoBehaviour
{
    [SerializeField] private LayerMask weaponMask;
    [SerializeField] private Transform[] weaponSlots;
    [SerializeField] private Camera PlayerCam;
    [SerializeField] private Transform PickupTarget;
    [SerializeField] private float smooth;
    [SerializeField] private float swayMultiplier;
    [SerializeField] private Transform plrHand;
    [SerializeField] private Transform orientation;
    [Space]
    [SerializeField] private float PickupRange;
    public Transform camTransform;
    public Rigidbody CurrentObject;
    public GameObject CurrentGameObject;
    public bool isAxe = false;

    private float dropTimer = 0f;
    private bool isDroppingState = false;
    private const float dropDuration = 0.5f;

    public PlayerMovement pm;


    void Start()
    {
        setAllSlotsInactive();
        pm = GetComponent<PlayerMovement>();
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.F) && CurrentGameObject)
        {
            StopAllCoroutines();
            StartCoroutine(doDrop());
            return;
        }

        else if (Input.GetKeyDown(KeyCode.F))
        {

            Ray CameraRay = PlayerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(CameraRay, out RaycastHit HitInfo, PickupRange, weaponMask) && useThisIndex() != 7)
            {
                CurrentObject = HitInfo.rigidbody;
                CurrentGameObject = HitInfo.transform.gameObject;
                CurrentObject.useGravity = false;
                CurrentObject.isKinematic = true;
                CurrentObject.angularDrag = 2;
                setAllSlotsInactive();
                weaponSlots[useThisIndex()].gameObject.SetActive(true);
                CurrentObject.transform.parent = weaponSlots[useThisIndex()];
                CurrentObject.transform.position = plrHand.transform.position;
                CurrentObject.transform.rotation = weaponSlots[useThisIndex()].transform.rotation;
                if (CurrentGameObject.layer == 14)
                {
                    isAxe = true;
                }
                Physics.IgnoreLayerCollision(14, 16, true);
                Debug.Log("yay");
                Destroy(CurrentObject);
            }
        }

        if (Input.GetKeyDown(KeyCode.Q) && CurrentGameObject)
        {
            CurrentGameObject.AddComponent<Rigidbody>();
            CurrentObject = CurrentGameObject.GetComponent<Rigidbody>();
            CurrentObject.useGravity = true;
            CurrentObject.transform.parent = null;
            CurrentObject.angularDrag = 0;
            Physics.IgnoreLayerCollision(14, 16, false);
            CurrentObject.AddForce(camTransform.forward * 22.5f, ForceMode.Impulse);
            CurrentObject = null;
            CurrentGameObject = null;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            setAllSlotsInactive();
            weaponSlots[0].gameObject.SetActive(true);
            CurrentGameObject = null;
            CurrentObject = null;
            if (weaponSlots[0].transform.childCount > 0)
            {
                CurrentGameObject = weaponSlots[0].gameObject.transform.GetChild(0).gameObject;
                CurrentObject = CurrentGameObject.GetComponent<Rigidbody>();
                Debug.Log(CurrentGameObject.layer);
                if (CurrentGameObject.layer == 14)
                {
                    isAxe = true;
                    Debug.Log(isAxe);
                }
                else
                {
                    isAxe = false;
                }
            }
            else
            {
                isAxe = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            setAllSlotsInactive();
            weaponSlots[1].gameObject.SetActive(true);
            CurrentGameObject = null;
            CurrentObject = null;
            if (weaponSlots[1].transform.childCount > 0)
            {
                CurrentGameObject = weaponSlots[1].gameObject.transform.GetChild(0).gameObject;
                CurrentObject = CurrentGameObject.GetComponent<Rigidbody>();
                if (CurrentGameObject.layer == 14)
                {
                    isAxe = true;
                }
                else
                {
                    isAxe = false;
                }
            }
            else
            {
                isAxe = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            setAllSlotsInactive();
            weaponSlots[2].gameObject.SetActive(true);
            CurrentGameObject = null;
            CurrentObject = null;
            if (weaponSlots[2].transform.childCount > 0)
            {
                CurrentGameObject = weaponSlots[2].gameObject.transform.GetChild(0).gameObject;
                CurrentObject = CurrentGameObject.GetComponent<Rigidbody>();
                if (CurrentGameObject.layer == 14)
                {
                    isAxe = true;
                }
                else
                {
                    isAxe = false;
                }
            }
            else
            {
                isAxe = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            setAllSlotsInactive();
            weaponSlots[3].gameObject.SetActive(true);
            CurrentGameObject = null;
            CurrentObject = null;
            if (weaponSlots[3].transform.childCount > 0)
            {
                CurrentGameObject = weaponSlots[3].gameObject.transform.GetChild(0).gameObject;
                CurrentObject = CurrentGameObject.GetComponent<Rigidbody>();
                if (CurrentGameObject.layer == 14)
                {
                    isAxe = true;
                }
                else
                {
                    isAxe = false;
                }
            }
            else
            {
                isAxe = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            setAllSlotsInactive();
            weaponSlots[4].gameObject.SetActive(true);
            CurrentGameObject = null;
            CurrentObject = null;
            if (weaponSlots[4].transform.childCount > 0)
            {
                CurrentGameObject = weaponSlots[4].gameObject.transform.GetChild(0).gameObject;
                CurrentObject = CurrentGameObject.GetComponent<Rigidbody>();
                if (CurrentGameObject.layer == 14)
                {
                    isAxe = true;
                }
                else
                {
                    isAxe = false;
                }
            }
            else
            {
                isAxe = false;
            }
        }

    }

    public int useThisIndex()
    {
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i].transform.childCount == 0)
            {
                return i;
            }
        }
        return 7;
    }

    public void setAllSlotsInactive()
    {
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            weaponSlots[i].gameObject.SetActive(false);
        }
    }

    private IEnumerator doDrop()
    {
        if (CurrentGameObject)
        {

            yield return new WaitForSeconds(0.750f / 4f);
            CurrentGameObject.AddComponent<Rigidbody>();
            CurrentObject = CurrentGameObject.GetComponent<Rigidbody>();
            CurrentObject.useGravity = true;
            Physics.IgnoreLayerCollision(14, 16, false);
            CurrentObject.angularDrag = 0;
            CurrentObject.isKinematic = false;
            CurrentObject.transform.parent = null;
            CurrentObject = null;
            CurrentGameObject = null;
        }
        yield return new WaitForSeconds(0f);

    }


}
