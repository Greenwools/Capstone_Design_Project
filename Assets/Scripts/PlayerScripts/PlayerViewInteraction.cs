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

        // ���� ī�޶� ���߾ӿ��� �������� raycast == �÷��̾ �ٶ󺸴� �������� raycast
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
        PlayerTransform.GetComponent<CharacterController>().enabled = false;        // �÷��̾� �̵� �Ұ� ���� �߻�
        PlayerTransform.position = SpawnTransform.position;                         // CharacterController ���� �߻��� �� �ִ� ������ �Ǹ�
        PlayerTransform.GetComponent<CharacterController>().enabled = true;         // �̵� �� ��� �����ϰ� �̵� �� ��� Ȱ��ȭ
    }
}
