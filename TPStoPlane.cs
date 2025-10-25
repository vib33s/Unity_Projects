using UnityEngine;

public class TPStoPlane : MonoBehaviour
{
    public GameObject cameraHolder;
    public Transform cam;
    public GameObject player;
    public GameObject plane;
    public Camera planeCamera;

    private PlaneController planeController;

    private void Start()
    {
        planeController = plane.GetComponent<PlaneController>();

        // Initially disable the plane controller and its camera
        planeController.enabled = false;
        planeCamera.enabled = false;
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.L))
        {
            // Reverse the enable/disable logic
            if (player.activeSelf == false)
                player.transform.position = plane.transform.position;
                cameraHolder.SetActive(true);
                player.SetActive(true);
                planeController.enabled = false;
                planeCamera.enabled = false;
            
            
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            RaycastHit hit;

            if (Physics.Raycast(player.transform.position, cam.TransformDirection(Vector3.forward), out hit, 12f)) {

                if (hit.collider.gameObject.tag == "plane")
                {
                    // Disable the player and its camera
                    cameraHolder.SetActive(false);
                    player.SetActive(false);

                    // Enable the plane controller and its camera
                    planeController.enabled = true;
                    planeCamera.enabled = true;
                    // planeController.enabled = false;
                }
            }   
        }
    }
}