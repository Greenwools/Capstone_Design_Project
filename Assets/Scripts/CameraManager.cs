using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    public float MouseSensity = 500.0f;
    public Transform PlayerTransform;

    private float xRot = 0f;
    private float delay = 0.2f;     // ���� ���� �� ī�޶� �þ߰� �̻��ؼ� �ʹ� �Է� ���ÿ� ������

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (delay > 0)
        {
            delay -= Time.deltaTime;
            return;
        }

        RoatateCamera();
    }

    private void RoatateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * MouseSensity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * MouseSensity * Time.deltaTime;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);
        PlayerTransform.Rotate(Vector3.up * mouseX);
    }
}
