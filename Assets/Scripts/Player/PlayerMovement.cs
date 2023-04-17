using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody rb;

    [SerializeField]
    float speed = 50;

    [SerializeField]
    float fastForwardSpeed = 100;

    [SerializeField]
    float turningSpeed = 50;

    [SerializeField]
    float rebalanceSpeed = 1;

    [SerializeField]
    TextMeshProUGUI speedText;

    [SerializeField]
    TextMeshProUGUI orbitingBodyText;

    Coroutine rebalanceCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        UpdateOrbitingText();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            var angles = transform.localEulerAngles;
            angles.x = 0;
            angles.z = 0;
            transform.localEulerAngles = angles;
            rb.angularVelocity = Vector3.zero;
        }

        speedText.text = $"Speed: {rb.velocity.magnitude} km/s";
    }

    private void FixedUpdate()
    {
        var vertical = Input.GetAxisRaw("Vertical");
        var horizontal = Input.GetAxisRaw("Tilt Horizontal");
        var spaceVertical = Input.GetAxisRaw("Tilt Vertical");

        var tiltVertical = Input.GetAxisRaw("Space Vertical");
        var tiltHorizontal = Input.GetAxisRaw("Horizontal");

        if (rebalanceCoroutine == null)
        {
            rb.AddRelativeTorque(new Vector3(-tiltVertical, tiltHorizontal, -horizontal) * turningSpeed, ForceMode.Acceleration);

            if (Input.GetKey(KeyCode.LeftShift))
            {
                rb.AddRelativeForce(new Vector3(0, 0, vertical) * fastForwardSpeed, ForceMode.Acceleration);
            }
            else
            {
                rb.AddRelativeForce(new Vector3(0, 0, vertical) * speed, ForceMode.Acceleration);
            }
        }
    }

    private IEnumerator RebalanceCoroutine()
    {

        var angles = transform.eulerAngles;
        if (angles.x < 0) angles.x += 360;
        if (angles.z < 0) angles.x += 360;
        while (transform.eulerAngles.x != 0 && transform.eulerAngles.z != 0)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, LODManager.instance.transform.rotation, rebalanceSpeed * Time.deltaTime);
            yield return null;
        }
        angles.x = 0;
        angles.z = 0;
        transform.eulerAngles = angles;
        rebalanceCoroutine = null;
    }

    public void UpdateOrbitingText()
    {
        if (transform.parent == null)
        {
            orbitingBodyText.text = "Orbitting: Galaxy";
        }
        else
        {
            orbitingBodyText.text = $"Orbitting: {transform.parent.name}";
        }
    }
}
