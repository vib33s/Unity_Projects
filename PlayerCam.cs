using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;

public class PlayerCam : MonoBehaviour
{

    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;

    public float rotationSpeed;

    public PlayerMovement pm;

    public Transform combatLookAt;

    public GameObject combatCam;

    public CinemachineFreeLook cm;

    public float initialFOV;

    public float targetFOV = 60f;

    public bool isTransitioning = false;

    public float transitionSpeed = 0.1f;

    private void Start()
    {
        pm = player.GetComponent<PlayerMovement>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        initialFOV = cm.m_Lens.FieldOfView;
    }

    public void Update()
    {
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;

        Vector3 dirToCombatLookAt = combatLookAt.position - new Vector3(transform.position.x, combatLookAt.position.y, transform.position.z);
        orientation.forward = dirToCombatLookAt.normalized;

        playerObj.forward = dirToCombatLookAt.normalized;

        if (isTransitioning)
        {

            initialFOV = Mathf.Lerp(initialFOV, targetFOV, transitionSpeed * Time.deltaTime);
            cm.m_Lens.FieldOfView = initialFOV;

            if (Mathf.Abs(initialFOV - targetFOV) < 0.1f)
            {
                initialFOV = targetFOV;
                cm.m_Lens.FieldOfView = initialFOV;
                isTransitioning = false;
            }
        }
    }

    public void DoFov(float xAngle)
    {

    }

    public void DoTilt(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }

}
