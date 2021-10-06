using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int movementSpeed = 10;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float dx = 0.0f, dz = 0.0f;
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

        this.transform.localPosition += new Vector3(dx, 0.0f, dz) * movementSpeed * Time.deltaTime;
    }
}
