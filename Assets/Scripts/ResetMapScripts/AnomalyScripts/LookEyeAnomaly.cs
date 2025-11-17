using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookEyeAnomaly : MonoBehaviour
{
    private Transform _playerCameraTransform;

    public float RotationSpeed = 3f;

    // Start is called before the first frame update
    void Start()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera != null ) _playerCameraTransform = mainCamera.transform;

        else
        {
            GameObject player = GameObject.FindWithTag("Player");

            if (player != null) _playerCameraTransform = player.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if ( _playerCameraTransform != null )
        {
            Vector3 dir = _playerCameraTransform.position - transform.position;

            Quaternion targetRot = Quaternion.LookRotation(dir);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, RotationSpeed * Time.deltaTime);
        }
    }
}
