using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody rb;

    [SerializeField]
    float speed = 50;

    [SerializeField]
    float turningSpeed = 50;

    [SerializeField]
    float rebalanceSpeed = 1;

    Coroutine rebalanceCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
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
            rb.AddRelativeTorque(new Vector3(-tiltVertical, tiltHorizontal, 0) * turningSpeed, ForceMode.Acceleration);

            rb.AddRelativeForce(new Vector3(horizontal, spaceVertical, vertical) * speed, ForceMode.Acceleration);
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
}
