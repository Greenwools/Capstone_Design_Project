using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    public float MouseSensity = 500.0f;
    public Transform PlayerTransform;

    private float _xRot = 0f;
    private float _delay = 0.2f;     // ���� ���� �� ī�޶� �þ߰� �̻��ؼ� �ʹ� �Է� ���ÿ� ������

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (_delay > 0)
        {
            _delay -= Time.deltaTime;
            return;
        }

        RoatateCamera();
    }

    private void RoatateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * MouseSensity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * MouseSensity * Time.deltaTime;

        _xRot -= mouseY;
        _xRot = Mathf.Clamp(_xRot, -90f, 90f);

        transform.localRotation = Quaternion.Euler(_xRot, 0f, 0f);
        PlayerTransform.Rotate(Vector3.up * mouseX);
    }
}
