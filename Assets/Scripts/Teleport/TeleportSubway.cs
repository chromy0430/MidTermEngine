using System;
using UnityEngine;

public class TeleportSubway : MonoBehaviour
{
    [SerializeField] private float cooltime = 0f;
    [SerializeField] private bool cooltimeTelpo = true;
    // 이동할 목표 지점
    [SerializeField] private Transform destination;

    private void Update()
    {
        cooltime += Time.deltaTime;
        if (cooltime > 5f && !cooltimeTelpo)
        {
            cooltime = 0f;
            cooltimeTelpo = true;
        }
    }

    // 트리거 충돌 감지
    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 오브젝트를 목표 지점으로 이동
        if (destination != null && cooltimeTelpo)
        {
            other.transform.position = destination.position;
            // 회전도 맞추고 싶다면 주석 해제
            other.transform.rotation = destination.rotation;
            cooltimeTelpo = false;
        }
    }

    // 인스펙터에서 destination이 설정되었는지 확인
    private void OnValidate()
    {
        if (destination == null)
        {
            Debug.LogWarning("Destination is not assigned in the inspector for " + gameObject.name);
        }
    }
}