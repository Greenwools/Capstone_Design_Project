using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerViewInteraction : MonoBehaviour
{
    private Camera _mainCamera;

    public float InteractionDistance = 3f;
    public LayerMask InteractionLayerMask;

    public Transform PlayerTransform;
    public Transform SpawnTransform;

    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {

        // 메인 카메라 정중앙에서 앞쪽으로 raycast == 플레이어가 바라보는 방향으로 raycast
        Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, InteractionDistance, InteractionLayerMask))
        {
            if (hit.collider.tag == "Interaction")
            {
                if (Input.GetKeyDown(KeyCode.E)) TeleportPlayer();
            }
        }
    }

    void TeleportPlayer()
    {
        PlayerTransform.GetComponent<CharacterController>().enabled = false;        // 플레이어 이동 불가 오류 발생
        PlayerTransform.position = SpawnTransform.position;                         // CharacterController 사용시 발생할 수 있는 오류로 판명
        PlayerTransform.GetComponent<CharacterController>().enabled = true;         // 이동 시 잠시 해제하고 이동 후 즉시 활성화
    }
}
