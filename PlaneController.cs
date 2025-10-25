using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlaneController : MonoBehaviour
{
    public float throttleIncrement = 0.1f;
    public float maxThrust = 200f;
    public float responsiveness = 10f;
    public float lift = 135f;
    public Vector3 liftVector = new Vector3(0,2,1);
    public Camera camera;


    public float throttle;
    private float roll;
    private float pitch;
    private float yaw;

    private float responseModifier
    {
        get {
            return (rb.mass / 10f) * responsiveness;
        }
    }

    Rigidbody rb;
    // [SerializeField] TextMeshProUGUI hud;
    [SerializeField] Transform propeller;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void HandleInputs()
    {
        roll = Input.GetAxis("Horizontal");
        pitch = Input.GetAxis("Vertical");
        yaw = Input.GetAxis("Yaw");

        if (Input.GetKey(KeyCode.LeftShift)) throttle += throttleIncrement;
        else if (Input.GetKey(KeyCode.LeftControl)) throttle -= throttleIncrement;
        throttle = Mathf.Clamp(throttle, 0f, 100f);
    }

    private void Update()
    {
        HandleInputs();
        // UpdateHUD();

        propeller.Rotate(Vector3.forward * throttle/5);

        if (throttle>20)
        {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, 105, throttle/30 * Time.deltaTime);
        }
        else
        {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, 90, 2f * Time.deltaTime);
        }

    }

    private void FixedUpdate()
    {
        rb.AddForce(transform.forward * maxThrust * throttle);
        rb.AddTorque(transform.up * yaw * responseModifier);
        rb.AddTorque(transform.right * pitch * responseModifier);
        rb.AddTorque(-transform.forward * roll * responseModifier);

        rb.AddForce(Vector3.up * rb.velocity.magnitude * lift);
    }

    private void UpdateHUD()
    {
        // hud.text = "Throttle: " + throttle.ToString("F0") + "%\n";
        // hud.text += "Airspeed: " + (rb.velocity.magnitude * 3.6f).ToString("F0") + "km/h\n";
        // hud.text += "Altitude: " + transform.position.y.ToString("F0");
    }
}
