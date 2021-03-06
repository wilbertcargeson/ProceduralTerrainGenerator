using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public GameObject toFollow;

    private Vector3 offset;

    void Start()
    {
        offset = transform.position - toFollow.transform.position;
        this.transform.position = toFollow.transform.position + Vector3.up * 50;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = toFollow.transform.position + offset;
    }
}
