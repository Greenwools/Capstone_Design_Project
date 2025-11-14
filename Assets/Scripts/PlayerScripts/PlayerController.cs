using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController _playerController;

    private Vector3 _playerVelocity;
    private AudioSource _audioSource;
    private bool _isRun = false;

    public AudioClip ConcreteSoundLoop;
    public AudioClip WaterShallowSoundLoop;
    public AudioClip WaterDeepSoundLoop;

    public bool IsInWater = false;
    public float CurrentWaterLevel = 0;

    public float WalkPitch = 0.95f;
    public float RunPitch = 1.6f;

    public float gravity = -10f;
    public float MoveSpeed = 5.0f;
    public float RunSpeed = 8.0f;

    // Start is called before the first frame update
    void Start()
    {
        _playerController = GetComponent<CharacterController>();
        _audioSource = GetComponent<AudioSource>();
        _audioSource.loop = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.IsPlayerStop || GameManager.Instance.IsUIOpen())
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

        _isRun = false;

        if (GameManager.LoopCount >= 1 && GameManager.CanSprint && Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = RunSpeed;
            _isRun = true;
        }

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
            AudioClip clipToPlay = null;

            if (IsInWater)
            {
                if (CurrentWaterLevel < 0.5f) clipToPlay = WaterShallowSoundLoop;
                else clipToPlay = WaterDeepSoundLoop;
            }

            else clipToPlay = ConcreteSoundLoop;

            if (clipToPlay != null) 
            { 
                if (_audioSource.clip != clipToPlay)
                {
                    _audioSource.Stop();
                    _audioSource.clip = clipToPlay;
                    _audioSource.Play();
                }

                else if (!_audioSource.isPlaying) _audioSource.Play();
            }

            _audioSource.pitch = _isRun ? RunPitch : WalkPitch;
        }

        else _audioSource.Stop();
    }
}
