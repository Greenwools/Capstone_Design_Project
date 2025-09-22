using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController playerController;

    public float MoveSpeed = 5.0f;
    public float RunSpeed = 8.0f;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        float currentSpeed = MoveSpeed;

        if (Input.GetKey(KeyCode.LeftShift)) currentSpeed = RunSpeed;

        float horizontalMove = Input.GetAxis("Horizontal");
        float verticalMove = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.forward * verticalMove + transform.right * horizontalMove;

        if (moveDirection.magnitude > 1) moveDirection.Normalize();         // 가속하지 않게 정규화

        playerController.Move(moveDirection * currentSpeed * Time.deltaTime);
    }
}
