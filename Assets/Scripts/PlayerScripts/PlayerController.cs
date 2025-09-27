using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController _playerController;

    private Vector3 _playerVelocity;

    public float gravity = -10f;
    public float MoveSpeed = 5.0f;
    public float RunSpeed = 8.0f;

    // Start is called before the first frame update
    void Start()
    {
        _playerController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        if (_playerController.isGrounded && _playerVelocity.y < 0) _playerVelocity.y = -2f;

        float currentSpeed = MoveSpeed;

        if (Input.GetKey(KeyCode.LeftShift)) currentSpeed = RunSpeed;

        float horizontalMove = Input.GetAxis("Horizontal");
        float verticalMove = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.forward * verticalMove + transform.right * horizontalMove;

        if (moveDirection.magnitude > 1) moveDirection.Normalize();         // 가속하지 않게 정규화

        Vector3 move = moveDirection * currentSpeed;

        _playerVelocity.y += gravity * Time.deltaTime;
        move.y = _playerVelocity.y;

        _playerController.Move(move * Time.deltaTime);
    }
}
