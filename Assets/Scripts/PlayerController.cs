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
        Rigidbody rb = GetComponent<Rigidbody>();
        if (Input.GetKey(KeyCode.A))
            rb.AddForce(Vector3.left * Time.deltaTime * movementSpeed);
        if (Input.GetKey(KeyCode.D))
            rb.AddForce(Vector3.right * Time.deltaTime * movementSpeed);
        if (Input.GetKey(KeyCode.W))
            rb.AddForce(Vector3.forward * Time.deltaTime * movementSpeed);
        if (Input.GetKey(KeyCode.S))
            rb.AddForce(Vector3.back * Time.deltaTime * movementSpeed);
        if (Input.GetKey(KeyCode.Space))
            rb.AddForce(Vector3.up * Time.deltaTime * movementSpeed);
        if (Input.GetKey(KeyCode.LeftControl))
            rb.AddForce(Vector3.down * Time.deltaTime * movementSpeed);
    }
}
