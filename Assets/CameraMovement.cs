using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float speed;
    // Update is called once per frame
    void Update()
    {
        var input = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
        transform.Translate(Time.deltaTime * speed * input);
    }
}
