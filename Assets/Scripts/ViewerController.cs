using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewerController : MonoBehaviour
{
    public int movementSpeed = 10;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float dx = 0.0f, dy = 0.0f, dz = 0.0f;
        int movementSpeedEva = movementSpeed;
        if (Input.GetKey(KeyCode.D))
        {
            dx += 1.0f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            dx -= 1.0f;
        }
        if (Input.GetKey(KeyCode.W))
        {
            dz += 1.0f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            dz -= 1.0f;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            dy += 1.0f;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            dy -= 1.0f;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            movementSpeedEva *= 2;
        }

        this.transform.localPosition += new Vector3(dx, dy, dz) * movementSpeedEva * Time.deltaTime;
    }
}