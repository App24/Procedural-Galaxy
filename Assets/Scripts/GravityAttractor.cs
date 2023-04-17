using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityAttractor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            other.gameObject.transform.SetParent(transform.parent, true);
            other.GetComponent<PlayerMovement>().UpdateOrbitingText();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            other.gameObject.transform.SetParent(transform.parent.parent, true);
            other.GetComponent<PlayerMovement>().UpdateOrbitingText();
        }
    }
}
