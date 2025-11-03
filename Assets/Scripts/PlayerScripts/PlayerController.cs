using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController _playerController;

    private Vector3 _playerVelocity;
    private AudioSource _audioSource;

    public float WalkPitch = 1.2f;
    public float RunPitch = 1.92f;

    public float gravity = -10f;
    public float MoveSpeed = 5.0f;
    public float RunSpeed = 8.0f;

    // Start is called before the first frame update
    void Start()
    {
        _playerController = GetComponent<CharacterController>();
        _audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.IsPlayerStop)
        {
            _audioSource.Stop();
            return;
        }

        MovePlayer();
        HandleFootSteps();
    }

    private void MovePlayer()
    {
        if (_playerController.isGrounded && _playerVelocity.y < 0) _playerVelocity.y = -2f;

        float currentSpeed = MoveSpeed;

        if (GameManager.LoopCount >= 1 && Input.GetKey(KeyCode.LeftShift)) 
            currentSpeed = RunSpeed;

        float horizontalMove = Input.GetAxis("Horizontal");
        float verticalMove = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.forward * verticalMove + transform.right * horizontalMove;

        if (moveDirection.magnitude > 1) moveDirection.Normalize();         

        Vector3 move = moveDirection * currentSpeed;

        _playerVelocity.y += gravity * Time.deltaTime;
        move.y = _playerVelocity.y;

        _playerController.Move(move * Time.deltaTime);
    }

    private void HandleFootSteps()
    {
        if (_playerController.isGrounded && _playerController.velocity.magnitude > 2f)
        {
            if (!_audioSource.isPlaying) _audioSource.Play();

            bool isRun = Input.GetKey(KeyCode.LeftShift);
            _audioSource.pitch = isRun ? RunPitch : WalkPitch;
        }

        else _audioSource.Stop();
    }
}
