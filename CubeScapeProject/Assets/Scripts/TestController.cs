using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestController : MonoBehaviour
{
    public float speed = 3;
    public float rotSpeed = 100;

    float yRot = 0;
    float xRot = 0;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        yRot = transform.eulerAngles.y;
        xRot = transform.eulerAngles.x;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        yRot += mouseInput.x * rotSpeed * Time.deltaTime;
        xRot -= mouseInput.y * rotSpeed * Time.deltaTime;

        transform.rotation = Quaternion.Euler(xRot, yRot, 0);

        float vertical = 0;

        if(Input.GetKey(KeyCode.E))
        {
            vertical++;
        }
        
        if(Input.GetKey(KeyCode.Q))
        {
            vertical--;
        }

        transform.Translate(Vector3.right * input.x * speed * Time.deltaTime);
        transform.Translate(Vector3.forward * input.y * speed * Time.deltaTime);
        transform.Translate(Vector3.up * vertical * speed * Time.deltaTime);
    }
}
