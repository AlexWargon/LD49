using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private string mouseXInput, mouseYInput;
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private Rigidbody rigidbody;
    private float xAxisClamp;

    // Start is called before the first frame update

    void Awake()
    {
        LockCursor();
    }

    private void Update()
    {
        CameraRotation();
    }
    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        xAxisClamp = 0f;
    }
    private void CameraRotation()
    {
        float mouseX = Input.GetAxis(mouseXInput) * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis(mouseYInput) * mouseSensitivity * Time.deltaTime;

        xAxisClamp += mouseY;

        if(xAxisClamp > 90f)
        {
            xAxisClamp = 90f;
            mouseY = 0f;
            ClampXAxisRotationToValue(270f);
        }
        else if(xAxisClamp < -90f)
        {
            xAxisClamp = -90f;
            mouseY = 0f;
            ClampXAxisRotationToValue(90f);
        }
        transform.Rotate(Vector3.left * mouseY);
        rigidbody.transform.Rotate(Vector3.up * mouseX);
    }

    private void ClampXAxisRotationToValue(float value)
    {
        Vector3 eulerRotation = transform.eulerAngles;
        eulerRotation.x = value;
        transform.eulerAngles = eulerRotation;
    }
}
