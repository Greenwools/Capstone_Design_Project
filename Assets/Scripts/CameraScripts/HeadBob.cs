using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [SerializeField] private CharacterController _playerController;

    private Vector3 _startPos;
    private float timer = 0f;

    public float WalkHeadBobSpeed = 7f;
    public float WalkHeadBobIntensity = 0.1f;

    public float RunHeadBobSpeed = 15f;
    public float RunHeadBobIntensity = 0.2f;

    public float StopSpeed = 10f;
    public bool IsHeadBob = true;

    // Start is called before the first frame update
    void Start()
    {
        _startPos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsHeadBob) return;

        // 벡터의 길이를 구해 플레이어의 속도 계산
        float playerSpeed = new Vector3(_playerController.velocity.x, 0, _playerController.velocity.z).magnitude;

        if (playerSpeed > 0.1f && _playerController.isGrounded)
        {
            bool isRun = playerSpeed > 5.1f;

            float headBobSpeed = isRun ? RunHeadBobSpeed : WalkHeadBobSpeed;
            float headBobIntensity = isRun ? RunHeadBobIntensity : WalkHeadBobIntensity;

            timer += Time.unscaledDeltaTime * headBobSpeed;

            float yPos = _startPos.y + Mathf.Sin(timer) * headBobIntensity;
            float xPos = _startPos.x + Mathf.Cos(timer / 2) * headBobIntensity / 2;

            transform.localPosition = new Vector3(xPos, yPos, _startPos.z);
        }

        // 플레이어가 정지했을 때 화면 흔들림 멈추기
        else
        {
            timer = 0f;
            transform.localPosition = Vector3.Lerp(transform.localPosition, _startPos, Time.deltaTime * StopSpeed);
        }
    }
}
